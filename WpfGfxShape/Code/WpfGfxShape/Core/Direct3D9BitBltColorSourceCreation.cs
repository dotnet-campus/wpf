using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9PrepareBitBltColorSource(
    out SurfaceDesc textureDescription,
    out uint levels);

internal delegate int Direct3D9InitializeBitBltColorSource(
    SurfaceDesc textureDescription,
    uint levels,
    bool isDependent,
    out IDisposable? colorSource);

internal sealed class Direct3D9BitBltColorSourceCreation : IDisposable
{
    private readonly IDisposable _colorSource;
    private readonly Direct3D9BitBltTransferSurface _transferSurface;
    private bool _isDisposed;

    private Direct3D9BitBltColorSourceCreation(
        IDisposable colorSource,
        Direct3D9BitBltTransferSurface transferSurface)
    {
        _colorSource = colorSource;
        _transferSurface = transferSurface;
    }

    internal static int TryCreate(
        bool isDependent,
        Direct3D9PrepareBitBltColorSource prepareColorSource,
        Direct3D9InitializeBitBltColorSource initializeColorSource,
        Direct3D9CheckBitBltTransferSurfaceFormat checkRenderTargetFormat,
        Direct3D9CreateBitBltTransferSurface createRenderTarget,
        Action<nint> releaseSurface,
        out Direct3D9BitBltColorSourceCreation? creation)
    {
        ArgumentNullException.ThrowIfNull(prepareColorSource);
        ArgumentNullException.ThrowIfNull(initializeColorSource);
        ArgumentNullException.ThrowIfNull(checkRenderTargetFormat);
        ArgumentNullException.ThrowIfNull(createRenderTarget);
        ArgumentNullException.ThrowIfNull(releaseSurface);

        creation = null;

        int result = prepareColorSource(out SurfaceDesc textureDescription, out uint levels);
        if (result < 0)
        {
            return result;
        }

        IDisposable? colorSource = null;
        Direct3D9BitBltTransferSurface? transferSurface = null;
        try
        {
            result = initializeColorSource(textureDescription, levels, isDependent, out colorSource);
            if (result < 0)
            {
                return result;
            }

            if (colorSource is null)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            result = Direct3D9BitBltTransferSurface.TryCreate(
                textureDescription,
                checkRenderTargetFormat,
                createRenderTarget,
                releaseSurface,
                out transferSurface);
            if (result < 0)
            {
                return result;
            }

            if (transferSurface is null)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            creation = new Direct3D9BitBltColorSourceCreation(colorSource, transferSurface);
            colorSource = null;
            transferSurface = null;
            return result;
        }
        finally
        {
            transferSurface?.Dispose();
            colorSource?.Dispose();
        }
    }

    internal IDisposable ColorSource
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _colorSource;
        }
    }

    internal nint GetValidTransferSurface(bool isColorSourceValid)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _transferSurface.GetValidSurface(isColorSourceValid);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _transferSurface.Dispose();
        _colorSource.Dispose();
    }
}
