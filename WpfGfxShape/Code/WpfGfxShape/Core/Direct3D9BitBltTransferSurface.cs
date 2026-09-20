using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9CheckBitBltTransferSurfaceFormat(Format format);

internal delegate int Direct3D9CreateBitBltTransferSurface(
    uint width,
    uint height,
    Format format,
    MultisampleType multisampleType,
    uint multisampleQuality,
    bool lockable,
    out nint surface);

internal sealed class Direct3D9BitBltTransferSurface : IDisposable
{
    private readonly Action<nint> _releaseSurface;
    private nint _surface;
    private bool _isDisposed;

    private Direct3D9BitBltTransferSurface(nint surface, Action<nint> releaseSurface)
    {
        _surface = surface;
        _releaseSurface = releaseSurface;
    }

    internal static int TryCreate(
        SurfaceDesc textureDescription,
        Direct3D9CheckBitBltTransferSurfaceFormat checkRenderTargetFormat,
        Direct3D9CreateBitBltTransferSurface createRenderTarget,
        Action<nint> releaseSurface,
        out Direct3D9BitBltTransferSurface? transferSurface)
    {
        ArgumentNullException.ThrowIfNull(checkRenderTargetFormat);
        ArgumentNullException.ThrowIfNull(createRenderTarget);
        ArgumentNullException.ThrowIfNull(releaseSurface);

        transferSurface = null;

        int result = checkRenderTargetFormat(textureDescription.Format);
        if (result < 0)
        {
            return result;
        }

        nint surface = 0;
        result = createRenderTarget(
            textureDescription.Width,
            textureDescription.Height,
            textureDescription.Format,
            MultisampleType.MultisampleNone,
            0,
            true,
            out surface);
        if (result < 0)
        {
            if (surface != 0)
            {
                releaseSurface(surface);
            }

            return result;
        }

        if (surface == 0)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        transferSurface = new Direct3D9BitBltTransferSurface(surface, releaseSurface);
        return result;
    }

    internal nint GetValidSurface(bool isColorSourceValid)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return isColorSourceValid ? _surface : 0;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        nint surface = _surface;
        _surface = 0;
        if (surface != 0)
        {
            _releaseSurface(surface);
        }
    }
}
