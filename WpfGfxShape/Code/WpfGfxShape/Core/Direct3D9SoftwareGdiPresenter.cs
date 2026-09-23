using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WpfGfxShape.Core;

internal delegate nint Direct3D9CreateDibSection(
    nint deviceContext,
    int width,
    int height,
    out nint bits);

internal delegate bool Direct3D9BitBlt(
    nint destinationDeviceContext,
    int x,
    int y,
    int width,
    int height,
    nint sourceDeviceContext,
    int sourceX,
    int sourceY);

internal sealed record Direct3D9SoftwareGdiCallbacks(
    Func<nint, nint> GetWindowDeviceContext,
    Func<nint, nint, bool> ReleaseWindowDeviceContext,
    Func<nint, nint> CreateCompatibleDeviceContext,
    Direct3D9CreateDibSection CreateDibSection,
    Func<nint, nint, nint> SelectObject,
    Direct3D9BitBlt BitBlt,
    Func<nint, bool> DeleteObject,
    Func<nint, bool> DeleteDeviceContext,
    Func<nint, bool> IsWindow,
    Func<int> GetLastError);

[SupportedOSPlatform("windows5.1.2600")]
internal sealed class Direct3D9SoftwareGdiPresenter : IDisposable
{
    private readonly nint _windowHandle;
    private readonly Direct3D9SoftwareGdiCallbacks _callbacks;
    private nint _backBufferDeviceContext;
    private nint _backBufferBitmap;
    private nint _previousBitmap;
    private nint _dibBits;
    private byte[]? _renderBits;
    private int _stride;
    private uint _width;
    private uint _height;
    private bool _isDisposed;

    private Direct3D9SoftwareGdiPresenter(nint windowHandle, Direct3D9SoftwareGdiCallbacks callbacks)
    {
        _windowHandle = windowHandle;
        _callbacks = callbacks;
    }

    internal uint Width => _width;

    internal uint Height => _height;

    internal int Stride => _stride;

    internal bool HasResources => _backBufferDeviceContext != 0 && _backBufferBitmap != 0 && _dibBits != 0;

    internal static int Create(
        nint windowHandle,
        uint width,
        uint height,
        out Direct3D9SoftwareGdiPresenter? presenter,
        Direct3D9SoftwareGdiCallbacks? callbacks = null)
    {
        presenter = null;
        if (windowHandle == 0)
        {
            return Direct3D9Factory.InvalidWindowHandleHResult;
        }

        Direct3D9SoftwareGdiPresenter candidate = new(
            windowHandle,
            callbacks ?? Direct3D9SoftwareGdiNative.CreateCallbacks());
        int result = candidate.Resize(width, height);
        if (result < 0)
        {
            candidate.Dispose();
            return result;
        }

        presenter = candidate;
        return result;
    }

    internal int Lock(out byte[] pixels, out int stride)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_renderBits is null)
        {
            pixels = [];
            stride = 0;
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        pixels = _renderBits;
        stride = _stride;
        return Direct3D9Factory.SuccessHResult;
    }

    internal void Unlock()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    internal int Resize(uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        FreeResources();

        if (width == 0 || height == 0)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        int managedStride;
        int bufferSize;
        try
        {
            managedStride = checked((int) width * 4);
            bufferSize = checked(managedStride * (int) height);
        }
        catch (OverflowException)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        nint frontDeviceContext = _callbacks.GetWindowDeviceContext(_windowHandle);
        if (frontDeviceContext == 0)
        {
            return GetGdiFailureResult();
        }

        int result = Direct3D9Factory.SuccessHResult;
        try
        {
            _backBufferDeviceContext = _callbacks.CreateCompatibleDeviceContext(frontDeviceContext);
            if (_backBufferDeviceContext == 0)
            {
                result = GetGdiFailureResult();
            }

            if (result >= 0)
            {
                _backBufferBitmap = _callbacks.CreateDibSection(
                    frontDeviceContext,
                    checked((int) width),
                    -checked((int) height),
                    out _dibBits);
                if (_backBufferBitmap == 0 || _dibBits == 0)
                {
                    result = GetGdiFailureResult();
                }
            }

            if (result >= 0)
            {
                _previousBitmap = _callbacks.SelectObject(_backBufferDeviceContext, _backBufferBitmap);
                if (_previousBitmap == 0 || _previousBitmap == new nint(-1))
                {
                    result = GetGdiFailureResult();
                }
            }

            if (result >= 0)
            {
                try
                {
                    _renderBits = new byte[bufferSize];
                }
                catch (OutOfMemoryException)
                {
                    result = Direct3D9Factory.OutOfMemoryHResult;
                }
            }

            if (result >= 0)
            {
                _width = width;
                _height = height;
                _stride = managedStride;
            }
        }
        finally
        {
            _ = _callbacks.ReleaseWindowDeviceContext(_windowHandle, frontDeviceContext);
        }

        if (result < 0)
        {
            FreeResources();
        }

        return result;
    }

    internal int Present(Direct3D9SurfaceRect presentRect, IReadOnlyList<Direct3D9SurfaceRect>? dirtyRectangles = null)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_renderBits is null || !HasResources)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        Direct3D9SurfaceRect bounds = new(0, 0, checked((int) _width), checked((int) _height));
        if (!TryIntersect(bounds, presentRect, out Direct3D9SurfaceRect clippedPresent))
        {
            return Direct3D9Factory.SuccessHResult;
        }

        List<Direct3D9SurfaceRect> rectangles = [];
        if (dirtyRectangles is null)
        {
            rectangles.Add(clippedPresent);
        }
        else
        {
            foreach (Direct3D9SurfaceRect dirtyRectangle in dirtyRectangles)
            {
                if (TryIntersect(clippedPresent, dirtyRectangle, out Direct3D9SurfaceRect clippedDirty))
                {
                    rectangles.Add(clippedDirty);
                }
            }
        }

        if (rectangles.Count == 0)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        nint frontDeviceContext = _callbacks.GetWindowDeviceContext(_windowHandle);
        if (frontDeviceContext == 0)
        {
            return GetGdiFailureResult();
        }

        try
        {
            foreach (Direct3D9SurfaceRect rectangle in rectangles)
            {
                CopyToDib(rectangle);
                if (!_callbacks.BitBlt(
                    frontDeviceContext,
                    rectangle.Left,
                    rectangle.Top,
                    rectangle.Right - rectangle.Left,
                    rectangle.Bottom - rectangle.Top,
                    _backBufferDeviceContext,
                    rectangle.Left,
                    rectangle.Top))
                {
                    return GetGdiFailureResult();
                }
            }
        }
        finally
        {
            _ = _callbacks.ReleaseWindowDeviceContext(_windowHandle, frontDeviceContext);
        }

        return Direct3D9Factory.SuccessHResult;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        FreeResources();
        _isDisposed = true;
    }

    private void CopyToDib(Direct3D9SurfaceRect rectangle)
    {
        int rowBytes = checked((rectangle.Right - rectangle.Left) * 4);
        for (int y = rectangle.Top; y < rectangle.Bottom; y++)
        {
            int offset = checked(y * _stride + rectangle.Left * 4);
            Marshal.Copy(_renderBits!, offset, _dibBits + offset, rowBytes);
        }
    }

    private int GetGdiFailureResult()
    {
        if (!_callbacks.IsWindow(_windowHandle))
        {
            return Direct3D9Factory.InvalidWindowHandleHResult;
        }

        int error = _callbacks.GetLastError();
        return error == 0
            ? Direct3D9Factory.GenericFailureHResult
            : unchecked((int) (0x80070000u | (uint) error));
    }

    private void FreeResources()
    {
        _renderBits = null;
        _width = 0;
        _height = 0;
        _stride = 0;
        _dibBits = 0;

        if (_backBufferDeviceContext != 0 && _previousBitmap != 0 && _previousBitmap != new nint(-1))
        {
            _ = _callbacks.SelectObject(_backBufferDeviceContext, _previousBitmap);
        }

        _previousBitmap = 0;

        if (_backBufferBitmap != 0)
        {
            _ = _callbacks.DeleteObject(_backBufferBitmap);
            _backBufferBitmap = 0;
        }

        if (_backBufferDeviceContext != 0)
        {
            _ = _callbacks.DeleteDeviceContext(_backBufferDeviceContext);
            _backBufferDeviceContext = 0;
        }
    }

    private static bool TryIntersect(
        Direct3D9SurfaceRect first,
        Direct3D9SurfaceRect second,
        out Direct3D9SurfaceRect intersection)
    {
        int left = Math.Max(first.Left, second.Left);
        int top = Math.Max(first.Top, second.Top);
        int right = Math.Min(first.Right, second.Right);
        int bottom = Math.Min(first.Bottom, second.Bottom);
        intersection = new Direct3D9SurfaceRect(left, top, right, bottom);
        return left < right && top < bottom;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed class Direct3D9SoftwareWindowRenderTarget : IDisposable
{
    private readonly Direct3D9SoftwareGdiPresenter _presenter;
    private readonly Direct3D9SoftwareRenderTargetSurface _surface;
    private readonly List<Direct3D9SurfaceRect> _dirtyRectangles = [];
    private bool _isDisposed;

    private Direct3D9SoftwareWindowRenderTarget(
        Direct3D9SoftwareGdiPresenter presenter,
        Direct3D9SoftwareRenderTargetSurface surface)
    {
        _presenter = presenter;
        _surface = surface;
    }

    internal Direct3D9SoftwareRenderTargetSurface Surface => _surface;

    internal static int Create(
        nint windowHandle,
        uint width,
        uint height,
        out Direct3D9SoftwareWindowRenderTarget? renderTarget,
        Direct3D9SoftwareGdiCallbacks? callbacks = null)
    {
        renderTarget = null;
        int result = Direct3D9SoftwareGdiPresenter.Create(
            windowHandle,
            width,
            height,
            out Direct3D9SoftwareGdiPresenter? presenter,
            callbacks);
        if (result < 0 || presenter is null)
        {
            return result;
        }

        Direct3D9SoftwareRenderTargetSurface surface = new(
            width,
            height,
            presenter.Lock,
            presenter.Unlock,
            (out Direct3D9Software3DSurface? software3DSurface) =>
            {
                software3DSurface = null;
                return Direct3D9Factory.NotAvailableHResult;
            },
            _ => { },
            _ => { });

        result = BindSurface(surface, presenter);
        if (result < 0)
        {
            surface.Dispose();
            presenter.Dispose();
            return result;
        }

        renderTarget = new Direct3D9SoftwareWindowRenderTarget(presenter, surface);
        return result;
    }

    internal int InvalidateRect(Direct3D9SurfaceRect rectangle)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        Direct3D9SurfaceRect bounds = new(0, 0, checked((int) _presenter.Width), checked((int) _presenter.Height));
        if (TryIntersect(bounds, rectangle, out Direct3D9SurfaceRect dirtyRectangle))
        {
            _dirtyRectangles.Add(dirtyRectangle);
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal int Present(Direct3D9SurfaceRect? rectangle = null)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        Direct3D9SurfaceRect bounds = new(0, 0, checked((int) _presenter.Width), checked((int) _presenter.Height));
        try
        {
            return _presenter.Present(rectangle ?? bounds, _dirtyRectangles);
        }
        finally
        {
            _dirtyRectangles.Clear();
        }
    }

    internal int Resize(uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _dirtyRectangles.Clear();
        int result = _presenter.Resize(width, height);
        if (result >= 0 && width != 0 && height != 0)
        {
            result = BindSurface(_surface, _presenter);
        }
        else if (result >= 0)
        {
            result = _surface.Resize(width, height);
        }

        return result;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _surface.Dispose();
        _presenter.Dispose();
        _dirtyRectangles.Clear();
        _isDisposed = true;
    }

    private static int BindSurface(
        Direct3D9SoftwareRenderTargetSurface surface,
        Direct3D9SoftwareGdiPresenter presenter)
    {
        return surface.SetSurface((out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            binding = new Direct3D9SoftwareRenderTargetBinding(
                presenter.Width,
                presenter.Height,
                MilPixelFormat.Pbgra32Bpp,
                96.0,
                96.0);
            return Direct3D9Factory.SuccessHResult;
        });
    }

    private static bool TryIntersect(
        Direct3D9SurfaceRect first,
        Direct3D9SurfaceRect second,
        out Direct3D9SurfaceRect intersection)
    {
        int left = Math.Max(first.Left, second.Left);
        int top = Math.Max(first.Top, second.Top);
        int right = Math.Min(first.Right, second.Right);
        int bottom = Math.Min(first.Bottom, second.Bottom);
        intersection = new Direct3D9SurfaceRect(left, top, right, bottom);
        return left < right && top < bottom;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal static partial class Direct3D9SoftwareGdiNative
{
    internal static Direct3D9SoftwareGdiCallbacks CreateCallbacks()
    {
        return new Direct3D9SoftwareGdiCallbacks(
            windowHandle => NativeMethods.GetDC(windowHandle),
            (windowHandle, deviceContext) => NativeMethods.ReleaseDC(windowHandle, deviceContext) != 0,
            NativeMethods.CreateCompatibleDC,
            CreateDibSection,
            NativeMethods.SelectObject,
            (destination, x, y, width, height, source, sourceX, sourceY) =>
                NativeMethods.BitBlt(destination, x, y, width, height, source, sourceX, sourceY, 0x00CC0020),
            NativeMethods.DeleteObject,
            NativeMethods.DeleteDC,
            NativeMethods.IsWindow,
            Marshal.GetLastPInvokeError);
    }

    private static unsafe nint CreateDibSection(
        nint deviceContext,
        int width,
        int height,
        out nint bits)
    {
        BitmapInfo bitmapInfo = new()
        {
            Header = new BitmapInfoHeader
            {
                Size = (uint) sizeof(BitmapInfoHeader),
                Width = width,
                Height = height,
                Planes = 1,
                BitCount = 32,
                Compression = 0
            }
        };

        return NativeMethods.CreateDIBSection(
            deviceContext,
            in bitmapInfo,
            0,
            out bits,
            0,
            0);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfoHeader
    {
        internal uint Size;
        internal int Width;
        internal int Height;
        internal ushort Planes;
        internal ushort BitCount;
        internal uint Compression;
        internal uint SizeImage;
        internal int XPelsPerMeter;
        internal int YPelsPerMeter;
        internal uint ClrUsed;
        internal uint ClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo
    {
        internal BitmapInfoHeader Header;
        internal uint Colors;
    }

    private static partial class NativeMethods
    {
        [LibraryImport("user32.dll", SetLastError = true)]
        internal static partial nint GetDC(nint windowHandle);

        [LibraryImport("user32.dll", SetLastError = true)]
        internal static partial int ReleaseDC(nint windowHandle, nint deviceContext);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool IsWindow(nint windowHandle);

        [LibraryImport("gdi32.dll", SetLastError = true)]
        internal static partial nint CreateCompatibleDC(nint deviceContext);

        [LibraryImport("gdi32.dll", SetLastError = true)]
        internal static partial nint CreateDIBSection(
            nint deviceContext,
            in BitmapInfo bitmapInfo,
            uint usage,
            out nint bits,
            nint section,
            uint offset);

        [LibraryImport("gdi32.dll", SetLastError = true)]
        internal static partial nint SelectObject(nint deviceContext, nint gdiObject);

        [LibraryImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool BitBlt(
            nint destinationDeviceContext,
            int x,
            int y,
            int width,
            int height,
            nint sourceDeviceContext,
            int sourceX,
            int sourceY,
            uint rasterOperation);

        [LibraryImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool DeleteObject(nint gdiObject);

        [LibraryImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool DeleteDC(nint deviceContext);
    }
}
