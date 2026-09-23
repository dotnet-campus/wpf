using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed class Direct3D9SoftwareGdiPresenterTests
{
    [TestMethod]
    public void WhenCreatedThenCompatibleDcAndTopDownDibAreSelectedAndReleasedInReverseOrder()
    {
        FakeGdi gdi = new();

        int result = Direct3D9SoftwareGdiPresenter.Create(10, 4, 3, out Direct3D9SoftwareGdiPresenter? presenter, gdi.Callbacks);
        presenter!.Dispose();

        Assert.AreEqual(
            (0, "GetDC:10|CreateDC:20|CreateDib:20:4:-3|Select:30:40|ReleaseDC:10:20|Select:30:50|DeleteObject:40|DeleteDC:30"),
            (result, string.Join('|', gdi.Calls)));
    }

    [TestMethod]
    public void WhenDirtyPresentThenPixelsAreCopiedAndRectanglesAreClippedBeforeBitBlt()
    {
        FakeGdi gdi = new();
        _ = Direct3D9SoftwareGdiPresenter.Create(10, 4, 3, out Direct3D9SoftwareGdiPresenter? presenter, gdi.Callbacks);
        _ = presenter!.Lock(out byte[] pixels, out _);
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = (byte) (i + 1);
        }
        gdi.Calls.Clear();

        int result = presenter.Present(
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            [new Direct3D9SurfaceRect(-2, 1, 3, 5)]);

        Assert.AreEqual(
            (0, "GetDC:10|BitBlt:20:0:1:3:2:30:0:1|ReleaseDC:10:20", pixels[16], pixels[43]),
            (result, string.Join('|', gdi.Calls), gdi.DibBytes[16], gdi.DibBytes[43]));
    }

    [TestMethod]
    public void WhenDirtyRegionIsEmptyThenNoWindowDcOrBitBltIsRequested()
    {
        FakeGdi gdi = new();
        _ = Direct3D9SoftwareGdiPresenter.Create(10, 4, 3, out Direct3D9SoftwareGdiPresenter? presenter, gdi.Callbacks);
        gdi.Calls.Clear();

        int result = presenter!.Present(
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            [new Direct3D9SurfaceRect(5, 5, 8, 8)]);

        Assert.AreEqual((0, 0), (result, gdi.Calls.Count));
    }

    [TestMethod]
    public void WhenResizedThenOldResourcesAreReleasedBeforeNewResourcesAreCreated()
    {
        FakeGdi gdi = new();
        _ = Direct3D9SoftwareGdiPresenter.Create(10, 4, 3, out Direct3D9SoftwareGdiPresenter? presenter, gdi.Callbacks);
        gdi.Calls.Clear();

        int result = presenter!.Resize(2, 5);

        Assert.AreEqual(
            (0, 2u, 5u, 8, "Select:30:50|DeleteObject:40|DeleteDC:30|GetDC:10|CreateDC:20|CreateDib:20:2:-5|Select:30:40|ReleaseDC:10:20"),
            (result, presenter.Width, presenter.Height, presenter.Stride, string.Join('|', gdi.Calls)));
    }

    [TestMethod]
    public void WhenBitBltFailsForDestroyedWindowThenInvalidWindowFailureIsReturnedAndDcIsReleased()
    {
        FakeGdi gdi = new() { BitBltResult = false, WindowValid = false };
        _ = Direct3D9SoftwareGdiPresenter.Create(10, 4, 3, out Direct3D9SoftwareGdiPresenter? presenter, gdi.Callbacks);
        gdi.Calls.Clear();

        int result = presenter!.Present(
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            [new Direct3D9SurfaceRect(0, 0, 1, 1)]);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidWindowHandleHResult, "GetDC:10|BitBlt:20:0:0:1:1:30:0:0|IsWindow:10|ReleaseDC:10:20"),
            (result, string.Join('|', gdi.Calls)));
    }

    [TestMethod]
    public void WhenDibCreationFailsThenPartialResourcesAreReleased()
    {
        FakeGdi gdi = new() { CreateDibResult = 0, WindowValid = true, LastError = 8 };

        int result = Direct3D9SoftwareGdiPresenter.Create(10, 4, 3, out Direct3D9SoftwareGdiPresenter? presenter, gdi.Callbacks);

        Assert.AreEqual(
            (unchecked((int) 0x80070008), null, "GetDC:10|CreateDC:20|CreateDib:20:4:-3|IsWindow:10|ReleaseDC:10:20|DeleteDC:30"),
            (result, presenter, string.Join('|', gdi.Calls)));
    }

    [TestMethod]
    public void WhenWindowTargetPresentsThenDirtyStateIsClearedEvenAfterFailure()
    {
        FakeGdi gdi = new() { BitBltResult = false, LastError = 5 };
        _ = Direct3D9SoftwareWindowRenderTarget.Create(10, 4, 3, out Direct3D9SoftwareWindowRenderTarget? renderTarget, gdi.Callbacks);
        _ = renderTarget!.InvalidateRect(new Direct3D9SurfaceRect(0, 0, 2, 2));
        gdi.Calls.Clear();

        int firstResult = renderTarget.Present();
        gdi.BitBltResult = true;
        gdi.Calls.Clear();
        int secondResult = renderTarget.Present();

        Assert.AreEqual((unchecked((int) 0x80070005), 0, 0), (firstResult, secondResult, gdi.Calls.Count));
    }

    [TestMethod]
    public void WhenWindowTargetIsDisposedThenRepeatedDisposeIsSafeAndFurtherCallsThrow()
    {
        FakeGdi gdi = new();
        _ = Direct3D9SoftwareWindowRenderTarget.Create(10, 4, 3, out Direct3D9SoftwareWindowRenderTarget? renderTarget, gdi.Callbacks);

        renderTarget!.Dispose();
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.Present());
    }

    private sealed class FakeGdi
    {
        internal List<string> Calls { get; } = [];

        internal byte[] DibBytes { get; } = new byte[256];

        internal bool BitBltResult { get; set; } = true;

        internal bool WindowValid { get; set; } = true;

        internal int LastError { get; set; }

        internal nint CreateDibResult { get; set; } = 40;

        internal Direct3D9SoftwareGdiCallbacks Callbacks => new(
            windowHandle =>
            {
                Calls.Add($"GetDC:{windowHandle}");
                return 20;
            },
            (windowHandle, deviceContext) =>
            {
                Calls.Add($"ReleaseDC:{windowHandle}:{deviceContext}");
                return true;
            },
            deviceContext =>
            {
                Calls.Add($"CreateDC:{deviceContext}");
                return 30;
            },
            (nint deviceContext, int width, int height, out nint bits) =>
            {
                Calls.Add($"CreateDib:{deviceContext}:{width}:{height}");
                if (CreateDibResult == 0)
                {
                    bits = 0;
                    return 0;
                }

                unsafe
                {
                    fixed (byte* pointer = DibBytes)
                    {
                        bits = (nint) pointer;
                    }
                }

                return CreateDibResult;
            },
            (deviceContext, gdiObject) =>
            {
                Calls.Add($"Select:{deviceContext}:{gdiObject}");
                return gdiObject == 40 ? 50 : 40;
            },
            (destination, x, y, width, height, source, sourceX, sourceY) =>
            {
                Calls.Add($"BitBlt:{destination}:{x}:{y}:{width}:{height}:{source}:{sourceX}:{sourceY}");
                return BitBltResult;
            },
            gdiObject =>
            {
                Calls.Add($"DeleteObject:{gdiObject}");
                return true;
            },
            deviceContext =>
            {
                Calls.Add($"DeleteDC:{deviceContext}");
                return true;
            },
            windowHandle =>
            {
                Calls.Add($"IsWindow:{windowHandle}");
                return WindowValid;
            },
            () => LastError);
    }
}
