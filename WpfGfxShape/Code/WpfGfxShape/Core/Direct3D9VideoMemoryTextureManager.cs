using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9VideoMemoryTextureManager : IDisposable
{
    private Direct3D9Device? _device;
    private Direct3D9BitmapTextureRequirements _requirements;
    private Direct3D9Surface? _systemMemorySurface;
    private Direct3D9Texture? _videoMemoryTexture;
    private bool _isSystemMemorySurfaceLocked;
    private bool _isDisposed;

    internal bool HasRealizationParameters => _device is not null;

    internal bool IsSystemMemorySurfaceValid => _systemMemorySurface?.IsValid == true;

    internal Direct3D9Texture? VideoMemoryTexture => _videoMemoryTexture?.IsValid == true
        ? _videoMemoryTexture
        : null;

    internal void SetRealizationParameters(
        Direct3D9Device device,
        Direct3D9BitmapTextureRequirements requirements)
    {
        ArgumentNullException.ThrowIfNull(device);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (HasRealizationParameters)
        {
            throw new InvalidOperationException("Realization parameters have already been set.");
        }

        _device = device;
        _requirements = requirements;
    }

    internal int ReCreateAndLockSystemMemorySurface(out LockedRect lockedRect)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        Direct3D9Device device = GetDevice();
        SurfaceDesc description = _requirements.Description;
        lockedRect = default;

        if (!IsSystemMemorySurfaceValid)
        {
            _systemMemorySurface?.Dispose();
            _systemMemorySurface = null;

            Direct3D9SystemMemoryUpdateSurface? nativeSurface = null;
            try
            {
                int result = device.TryCreateSystemMemoryUpdateSurface(
                    description.Width,
                    description.Height,
                    description.Format,
                    null,
                    out nativeSurface);
                if (result < 0)
                {
                    return result;
                }
                if (nativeSurface is null)
                {
                    return Direct3D9Factory.GenericFailureHResult;
                }

                result = Direct3D9Surface.TryCreateReference(
                    device.ResourceManager,
                    nativeSurface.Surface,
                    out _systemMemorySurface);
                if (result < 0)
                {
                    return result;
                }
            }
            finally
            {
                nativeSurface?.Dispose();
            }
        }

        Direct3D9SurfaceRect rectangle = new(
            0,
            0,
            checked((int) description.Width),
            checked((int) description.Height));
        int lockResult = _systemMemorySurface!.LockRect(
            out lockedRect,
            rectangle,
            (uint) D3D9.LockNoDirtyUpdate);
        if (lockResult >= 0)
        {
            _isSystemMemorySurfaceLocked = true;
        }

        return lockResult;
    }

    internal int UnlockSystemMemorySurface()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!IsSystemMemorySurfaceValid || !_isSystemMemorySurfaceLocked)
        {
            throw new InvalidOperationException("The system-memory surface is not locked.");
        }

        _isSystemMemorySurfaceLocked = false;
        return _systemMemorySurface!.UnlockRect();
    }

    internal int PushBitsToVideoMemoryTexture()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        Direct3D9Device device = GetDevice();
        if (!IsSystemMemorySurfaceValid)
        {
            throw new InvalidOperationException("A valid system-memory surface is required before uploading texture bits.");
        }

        if (_videoMemoryTexture is not null && !_videoMemoryTexture.IsValid)
        {
            _videoMemoryTexture.Dispose();
            _videoMemoryTexture = null;
        }

        if (_videoMemoryTexture is null)
        {
            int result = device.TryCreateBitmapTexture(
                _requirements,
                isEvictable: true,
                out _videoMemoryTexture);
            if (result < 0)
            {
                return result;
            }
            if (_videoMemoryTexture is null)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }
        }

        int getSurfaceResult = _videoMemoryTexture.TryGetSurfaceLevel(0, out Direct3D9Surface? videoMemorySurface);
        if (getSurfaceResult < 0)
        {
            return getSurfaceResult;
        }
        if (videoMemorySurface is null)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        using (videoMemorySurface)
        {
            int updateResult = device.UpdateSurface(
                _systemMemorySurface!.SurfaceForDeviceCall,
                null,
                videoMemorySurface.SurfaceForDeviceCall,
                null);
            if (updateResult < 0)
            {
                return updateResult;
            }
        }

        return _videoMemoryTexture.UpdateMipmapLevels(device);
    }

    internal void PrepareForNewRealization()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ReleaseResources();
        _device = null;
        _requirements = default;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        ReleaseResources();
        _device = null;
        _isDisposed = true;
    }

    private Direct3D9Device GetDevice() =>
        _device ?? throw new InvalidOperationException("Realization parameters have not been set.");

    private void ReleaseResources()
    {
        _isSystemMemorySurfaceLocked = false;
        _systemMemorySurface?.Dispose();
        _systemMemorySurface = null;
        _videoMemoryTexture?.Dispose();
        _videoMemoryTexture = null;
    }
}
