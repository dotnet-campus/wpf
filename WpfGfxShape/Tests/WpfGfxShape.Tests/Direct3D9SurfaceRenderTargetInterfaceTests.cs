using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    public unsafe void WhenScrollingThenNotImplementedIsReturned()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int result = renderTarget.ScrollBlt(
            new Direct3D9SurfaceRect(0, 0, 4, 4),
            new Direct3D9SurfaceRect(4, 4, 8, 8));

        Assert.AreEqual(Direct3D9Factory.NotImplementedHResult, result);
    }

    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    [DataRow("00000000-0000-0000-C000-000000000046")]
    [DataRow("8A6C3E10-8C6F-45A2-9244-BF137A91BDEE")]
    public unsafe void WhenAnyDisplayInterfaceIsRequestedThenNoInterfaceIsReturnedAndOutputIsCleared(string interfaceId)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int result = renderTarget.FindInterface(Guid.Parse(interfaceId), out nint interfacePointer);

        Assert.AreEqual((Direct3D9Factory.NoInterfaceHResult, 0), (result, interfacePointer));
    }

    [TestMethod]
    public unsafe void WhenDisplayInterfaceIsRequestedThenRenderTargetStateAndDirtyRegionAreUnchanged()
    {
        int presentCalls = 0;
        Direct3D9PresentRequest request = default;
        using Direct3D9Device device = CreatePresentDevice(value =>
        {
            presentCalls++;
            request = value;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            presentParameters: new PresentParameters(swapEffect: Swapeffect.Copy, windowed: true));
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        Assert.AreEqual(0, renderTarget.DrawBitmap(static () => 0));
        Direct3D9SurfaceRect dirtyRect = new(1, 2, 7, 9);
        Assert.AreEqual(0, renderTarget.InvalidateRect(dirtyRect));

        int result = renderTarget.FindInterface(Guid.NewGuid(), out nint interfacePointer);

        Assert.AreEqual(
            (Direct3D9Factory.NoInterfaceHResult, 0, true, true, 0, false, 0),
            (result, interfacePointer, renderTarget.IsRenderingEnabled, renderTarget.HasValidContents,
                renderTarget.DisplayInvalidHResult, device.IsEntered(), presentCalls));
        Assert.AreEqual(0, renderTarget.Present(dirtyRect));
        Assert.AreEqual((1, dirtyRect, null), (presentCalls, request.SourceRect, request.DirtyRegion));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenFindInterfaceRejectsUse()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.FindInterface(Guid.Empty, out _));
    }
}
