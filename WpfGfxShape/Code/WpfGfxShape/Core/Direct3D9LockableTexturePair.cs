using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal readonly unsafe struct Direct3D9LockableTextureData
{
    internal Direct3D9LockableTextureData(byte* mainBits, byte* auxiliaryBits, int pitch)
    {
        MainBits = mainBits;
        AuxiliaryBits = auxiliaryBits;
        Pitch = pitch;
    }

    internal byte* MainBits { get; }

    internal byte* AuxiliaryBits { get; }

    internal int Pitch { get; }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9LockableTexturePair
{
    private readonly Direct3D9Texture _mainTexture;
    private Direct3D9Texture? _auxiliaryTexture;

    internal Direct3D9LockableTexturePair(Direct3D9Texture mainTexture)
    {
        ArgumentNullException.ThrowIfNull(mainTexture);
        _mainTexture = mainTexture;
    }

    internal void InitializeAuxiliaryTexture(Direct3D9Texture auxiliaryTexture)
    {
        ArgumentNullException.ThrowIfNull(auxiliaryTexture);
        if (_auxiliaryTexture is not null)
        {
            throw new InvalidOperationException("The auxiliary texture has already been initialized.");
        }

        _auxiliaryTexture = auxiliaryTexture;
    }

    internal Direct3D9LockableTexturePairLock CreateLock()
    {
        return new Direct3D9LockableTexturePairLock(_mainTexture, _auxiliaryTexture);
    }

    internal int Draw(Direct3D9Device device, Direct3D9PointAndSizeRect destination, bool useAuxiliary)
    {
        ArgumentNullException.ThrowIfNull(device);

        if (!useAuxiliary)
        {
            return device.RenderTexture(_mainTexture, destination, Direct3D9TextureBlendMode.Default);
        }

        if (_auxiliaryTexture is null)
        {
            throw new InvalidOperationException("An auxiliary texture is required for vector alpha.");
        }

        int result = device.RenderTexture(
            _auxiliaryTexture,
            destination,
            Direct3D9TextureBlendMode.ApplyVectorAlpha);
        return result < 0
            ? result
            : device.RenderTexture(_mainTexture, destination, Direct3D9TextureBlendMode.AddColors);
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9LockableTexturePairLock : IDisposable
{
    private readonly Direct3D9Texture _mainTexture;
    private readonly Direct3D9Texture? _auxiliaryTexture;
    private bool _isMainLocked;
    private bool _isAuxiliaryLocked;
    private bool _isDisposed;

    internal Direct3D9LockableTexturePairLock(
        Direct3D9Texture mainTexture,
        Direct3D9Texture? auxiliaryTexture)
    {
        _mainTexture = mainTexture;
        _auxiliaryTexture = auxiliaryTexture;
    }

    internal int Lock(
        uint width,
        uint height,
        out Direct3D9LockableTextureData lockData,
        bool useAuxiliary = false)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_isMainLocked || _isAuxiliaryLocked)
        {
            throw new InvalidOperationException("The texture pair is already locked.");
        }

        lockData = default;
        int result = LockOne(_mainTexture, width, height, out LockedRect lockedRect);
        if (result < 0)
        {
            return result;
        }

        _isMainLocked = true;
        byte* mainBits = (byte*) lockedRect.PBits;
        int pitch = lockedRect.Pitch;

        if (!useAuxiliary)
        {
            lockData = new Direct3D9LockableTextureData(mainBits, null, pitch);
            return Direct3D9Factory.SuccessHResult;
        }

        if (_auxiliaryTexture is null)
        {
            throw new InvalidOperationException("An auxiliary texture is required for vector alpha.");
        }

        result = LockOne(_auxiliaryTexture, width, height, out lockedRect);
        if (result < 0)
        {
            return result;
        }

        _isAuxiliaryLocked = true;
        if (lockedRect.Pitch <= 0)
        {
            return Direct3D9Factory.InternalErrorHResult;
        }

        lockData = new Direct3D9LockableTextureData(mainBits, (byte*) lockedRect.PBits, pitch);
        return Direct3D9Factory.SuccessHResult;
    }

    private static int LockOne(
        Direct3D9Texture texture,
        uint width,
        uint height,
        out LockedRect lockedRect)
    {
        Direct3D9SurfaceRect rectangle = new(0, 0, checked((int) width), checked((int) height));
        int result = texture.LockRect(out lockedRect, rectangle, (uint) D3D9.LockNoDirtyUpdate);
        if (result < 0)
        {
            return result;
        }

        result = texture.AddDirtyRect(rectangle);
        if (result < 0)
        {
            return result;
        }

        byte* scanline = (byte*) lockedRect.PBits;
        nuint scanlineByteCount = (nuint) width * sizeof(uint);
        for (uint row = 0; row < height; row++)
        {
            NativeMemory.Clear(scanline, scanlineByteCount);
            scanline += lockedRect.Pitch;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        if (_isMainLocked)
        {
            _ = _mainTexture.UnlockRect();
            _isMainLocked = false;
        }

        if (_isAuxiliaryLocked)
        {
            _ = _auxiliaryTexture!.UnlockRect();
            _isAuxiliaryLocked = false;
        }
    }
}
