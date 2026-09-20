using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitBltTransferSurfaceTests
{
    [TestMethod]
    public void WhenCreationSucceedsThenFormatIsCheckedBeforeCreatingLockableRenderTarget()
    {
        List<string> calls = [];
        SurfaceDesc description = new(
            format: Format.A2R10G10B10,
            usage: D3D9.UsageRendertarget,
            width: 320,
            height: 240);

        int result = Direct3D9BitBltTransferSurface.TryCreate(
            description,
            format =>
            {
                calls.Add($"check:{format}");
                return Direct3D9Factory.SuccessHResult;
            },
            (uint width, uint height, Format format, MultisampleType multisampleType, uint quality, bool lockable, out nint surface) =>
            {
                calls.Add($"create:{width}:{height}:{format}:{multisampleType}:{quality}:{lockable}");
                surface = 41;
                return Direct3D9Factory.SuccessHResult;
            },
            surface => calls.Add($"release:{surface}"),
            out Direct3D9BitBltTransferSurface? transferSurface);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|41|check:FmtA2R10G10B10|create:320:240:FmtA2R10G10B10:MultisampleNone:0:True",
            $"{result}|{transferSurface!.GetValidSurface(true)}|{string.Join('|', calls)}");

        transferSurface.Dispose();
        Assert.AreEqual("release:41", calls[^1]);
    }

    [TestMethod]
    public void WhenFormatCheckFailsThenRenderTargetIsNotCreated()
    {
        int createCalls = 0;

        int result = Direct3D9BitBltTransferSurface.TryCreate(
            new SurfaceDesc(format: Format.X8R8G8B8, width: 10, height: 20),
            _ => Direct3D9Factory.NotAvailableHResult,
            (uint _, uint _, Format _, MultisampleType _, uint _, bool _, out nint surface) =>
            {
                createCalls++;
                surface = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            _ => { },
            out Direct3D9BitBltTransferSurface? transferSurface);

        Assert.AreEqual(
            (Direct3D9Factory.NotAvailableHResult, 0, null),
            (result, createCalls, transferSurface));
    }

    [TestMethod]
    public void WhenCreationFailsWithSurfaceThenSurfaceIsReleased()
    {
        List<nint> releasedSurfaces = [];

        int result = Direct3D9BitBltTransferSurface.TryCreate(
            new SurfaceDesc(format: Format.A8R8G8B8, width: 10, height: 20),
            _ => Direct3D9Factory.SuccessHResult,
            (uint _, uint _, Format _, MultisampleType _, uint _, bool _, out nint surface) =>
            {
                surface = 73;
                return Direct3D9Factory.InvalidCallHResult;
            },
            releasedSurfaces.Add,
            out Direct3D9BitBltTransferSurface? transferSurface);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, "73", null),
            (result, string.Join(',', releasedSurfaces), transferSurface));
    }

    [TestMethod]
    public void WhenColorSourceIsInvalidThenTransferSurfaceIsHidden()
    {
        _ = Direct3D9BitBltTransferSurface.TryCreate(
            new SurfaceDesc(format: Format.A8R8G8B8, width: 1, height: 1),
            _ => Direct3D9Factory.SuccessHResult,
            (uint _, uint _, Format _, MultisampleType _, uint _, bool _, out nint surface) =>
            {
                surface = 97;
                return Direct3D9Factory.SuccessHResult;
            },
            _ => { },
            out Direct3D9BitBltTransferSurface? transferSurface);

        using (transferSurface)
        {
            Assert.AreEqual((0, 97), (transferSurface!.GetValidSurface(false), transferSurface.GetValidSurface(true)));
        }
    }

    [TestMethod]
    public void WhenDisposedThenSurfaceIsReleasedOnceAndCannotBeRetrieved()
    {
        int releaseCalls = 0;
        _ = Direct3D9BitBltTransferSurface.TryCreate(
            new SurfaceDesc(format: Format.A8R8G8B8, width: 1, height: 1),
            _ => Direct3D9Factory.SuccessHResult,
            (uint _, uint _, Format _, MultisampleType _, uint _, bool _, out nint surface) =>
            {
                surface = 101;
                return Direct3D9Factory.SuccessHResult;
            },
            _ => releaseCalls++,
            out Direct3D9BitBltTransferSurface? transferSurface);

        transferSurface!.Dispose();
        transferSurface.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => transferSurface.GetValidSurface(true));
        Assert.AreEqual(1, releaseCalls);
    }
}
