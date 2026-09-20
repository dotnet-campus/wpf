using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal unsafe delegate int Direct3D9CreateBitmapSystemMemorySurface(
    uint width,
    uint height,
    Format format,
    void* pixels,
    out Direct3D9SystemMemoryUpdateSurface? surface);

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9BitmapSystemMemorySurfaceSource : IDisposable
{
    private readonly Direct3D9CreateBitmapSystemMemorySurface _createSurface;
    private readonly Format _format;
    private IDirect3DSurface9* _cachedSurface;
    private void* _referencedSystemBits;
    private bool _isDisposed;

    internal Direct3D9BitmapSystemMemorySurfaceSource(Direct3D9Device device, Format format)
        : this(
            (uint width, uint height, Format surfaceFormat, void* pixels, out Direct3D9SystemMemoryUpdateSurface? surface) =>
                device.TryCreateSystemMemoryUpdateSurface(width, height, surfaceFormat, pixels, out surface),
            format)
    {
        ArgumentNullException.ThrowIfNull(device);
    }

    internal Direct3D9BitmapSystemMemorySurfaceSource(
        Direct3D9CreateBitmapSystemMemorySurface createSurface,
        Format format)
    {
        ArgumentNullException.ThrowIfNull(createSurface);

        _createSurface = createSurface;
        _format = format;
    }

    internal bool TryAdoptCachedSurfaceFrom(Direct3D9BitmapSystemMemorySurfaceSource reusableSource)
    {
        ArgumentNullException.ThrowIfNull(reusableSource);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ObjectDisposedException.ThrowIf(reusableSource._isDisposed, reusableSource);

        if (_cachedSurface is not null || reusableSource._cachedSurface is null)
        {
            return false;
        }

        Direct3D9Factory.AddRef((nint) reusableSource._cachedSurface);
        _cachedSurface = reusableSource._cachedSurface;
        _referencedSystemBits = reusableSource._referencedSystemBits;
        return true;
    }

    internal int GetSurface(
        void* currentBits,
        uint width,
        uint height,
        bool canCreateFromBits,
        out Direct3D9SystemMemoryUpdateSurface? surface)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        surface = null;
        if (currentBits is not null && currentBits == _referencedSystemBits)
        {
            if (_cachedSurface is null)
            {
                throw new InvalidOperationException("The cached system-memory bits do not have a corresponding surface.");
            }

            Direct3D9Factory.AddRef((nint) _cachedSurface);
            surface = new Direct3D9SystemMemoryUpdateSurface(_cachedSurface);
            return Direct3D9Factory.SuccessHResult;
        }

        if (currentBits is not null && _referencedSystemBits is not null)
        {
            throw new InvalidOperationException("The bitmap bits moved after a system-memory surface cached their address.");
        }

        int result = _createSurface(
            width,
            height,
            _format,
            canCreateFromBits ? currentBits : null,
            out Direct3D9SystemMemoryUpdateSurface? createdSurface);
        if (result < 0)
        {
            createdSurface?.Dispose();
            return result;
        }
        if (createdSurface is null)
        {
            throw new InvalidOperationException("Direct3D system-memory update surface creation returned a null surface.");
        }

        if (currentBits is not null)
        {
            IDirect3DSurface9* newCachedSurface = createdSurface.Surface;
            Direct3D9Factory.AddRef((nint) newCachedSurface);
            Direct3D9Factory.Release(_cachedSurface);
            _cachedSurface = newCachedSurface;
            _referencedSystemBits = currentBits;
        }

        surface = createdSurface;
        return result;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        Direct3D9Factory.Release(_cachedSurface);
        _cachedSurface = null;
        _referencedSystemBits = null;
        _isDisposed = true;
    }
}
