using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed class Direct3D9DeviceBitmapColorSourceFlusherTests
{
    [TestMethod]
    public void WhenSurfaceIsLargerThanFlushLimitThenCopyAndReadbackUseNativeDimensionsAndOrder()
    {
        List<string> calls = [];
        Direct3D9DeviceBitmapColorSourceFlusher flusher = new(
            (uint width, uint height, Format format, MultisampleType multisampleType, uint multisampleQuality, bool lockable, out nint surface) =>
            {
                calls.Add($"create:{width}:{height}:{format}:{multisampleType}:{multisampleQuality}:{lockable}");
                surface = 73;
                return Direct3D9Factory.SuccessHResult;
            },
            (source, sourceRectangle, destination, destinationRectangle, filter) =>
            {
                calls.Add($"stretch:{source}:{sourceRectangle}:{destination}:{destinationRectangle}:{filter}");
                return Direct3D9Factory.SuccessHResult;
            },
            (surface, rectangle, flags) =>
            {
                calls.Add($"lock:{surface}:{rectangle}:{flags}");
                return Direct3D9Factory.SuccessHResult;
            },
            surface =>
            {
                calls.Add($"unlock:{surface}");
                return Direct3D9Factory.InvalidCallHResult;
            },
            surface => calls.Add($"release:{surface}"));

        int result = flusher.Flush(
            41,
            new SurfaceDesc(format: Format.A2R10G10B10, width: 64, height: 32));

        Assert.AreEqual(
            string.Join('|',
                "create:16:16:FmtA2R10G10B10:MultisampleNone:0:True",
                "stretch:41:Direct3D9SurfaceRect { Left = 0, Top = 0, Right = 16, Bottom = 16 }:73:Direct3D9SurfaceRect { Left = 0, Top = 0, Right = 16, Bottom = 16 }:TexfNone",
                $"lock:73:Direct3D9SurfaceRect {{ Left = 0, Top = 0, Right = 1, Bottom = 1 }}:{D3D9.LockReadonly}",
                "unlock:73",
                "release:73"),
            string.Join('|', calls));
        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);
    }

    [TestMethod]
    public void WhenSurfaceIsSmallerThanFlushLimitThenFullSurfaceIsCopied()
    {
        Direct3D9SurfaceRect copiedRectangle = default;
        Direct3D9DeviceBitmapColorSourceFlusher flusher = new(
            (uint width, uint height, Format _, MultisampleType _, uint _, bool _, out nint surface) =>
            {
                Assert.AreEqual((7u, 9u), (width, height));
                surface = 73;
                return Direct3D9Factory.SuccessHResult;
            },
            (_, sourceRectangle, _, _, _) =>
            {
                copiedRectangle = sourceRectangle;
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _, _) => Direct3D9Factory.SuccessHResult,
            _ => Direct3D9Factory.SuccessHResult,
            _ => { });

        int result = flusher.Flush(41, new SurfaceDesc(format: Format.X8R8G8B8, width: 7, height: 9));

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, new Direct3D9SurfaceRect(0, 0, 7, 9)),
            (result, copiedRectangle));
    }

    [TestMethod]
    public void WhenStretchFailsThenFirstFailurePropagatesAndSurfaceIsUnlockedAndReleased()
    {
        int lockCalls = 0;
        List<string> cleanup = [];
        Direct3D9DeviceBitmapColorSourceFlusher flusher = new(
            (uint _, uint _, Format _, MultisampleType _, uint _, bool _, out nint surface) =>
            {
                surface = 73;
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _, _, _, _) => Direct3D9Factory.InvalidCallHResult,
            (_, _, _) =>
            {
                lockCalls++;
                return Direct3D9Factory.SuccessHResult;
            },
            surface =>
            {
                cleanup.Add($"unlock:{surface}");
                return Direct3D9Factory.GenericFailureHResult;
            },
            surface => cleanup.Add($"release:{surface}"));

        int result = flusher.Flush(41, new SurfaceDesc(format: Format.A8R8G8B8, width: 20, height: 20));

        CollectionAssert.AreEqual(new[] { "unlock:73", "release:73" }, cleanup);
        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 0), (result, lockCalls));
    }

    [TestMethod]
    public void WhenCreationFailsWithSurfaceThenSurfaceIsUnlockedAndReleased()
    {
        List<string> cleanup = [];
        Direct3D9DeviceBitmapColorSourceFlusher flusher = new(
            (uint _, uint _, Format _, MultisampleType _, uint _, bool _, out nint surface) =>
            {
                surface = 73;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            (_, _, _, _, _) => throw new AssertFailedException("StretchRect must not run."),
            (_, _, _) => throw new AssertFailedException("LockRect must not run."),
            surface =>
            {
                cleanup.Add($"unlock:{surface}");
                return Direct3D9Factory.SuccessHResult;
            },
            surface => cleanup.Add($"release:{surface}"));

        int result = flusher.Flush(41, new SurfaceDesc(format: Format.A8R8G8B8, width: 20, height: 20));

        CollectionAssert.AreEqual(new[] { "unlock:73", "release:73" }, cleanup);
        Assert.AreEqual(Direct3D9Factory.OutOfMemoryHResult, result);
    }
}
