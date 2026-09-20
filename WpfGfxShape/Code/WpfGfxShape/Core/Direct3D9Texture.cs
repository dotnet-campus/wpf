using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct Direct3D9TextureVertex
{
    internal Direct3D9TextureVertex(float x, float y, float u, float v)
    {
        X = x;
        Y = y;
        Z = 0.5f;
        Diffuse = uint.MaxValue;
        U0 = u;
        V0 = v;
        U1 = 0;
        V1 = 0;
    }

    internal readonly float X;
    internal readonly float Y;
    internal readonly float Z;
    internal readonly uint Diffuse;
    internal readonly float U0;
    internal readonly float V0;
    internal readonly float U1;
    internal readonly float V1;
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9Texture : Direct3D9Resource
{
    private readonly Direct3D9ResourceManager _resourceManager;
    private readonly uint _width;
    private readonly uint _height;
    private IDirect3DTexture9* _texture;
    private Direct3D9Surface?[]? _surfaceLevels;

    internal Direct3D9Texture(
        Direct3D9ResourceManager resourceManager,
        IDirect3DTexture9* texture,
        uint width,
        uint height)
        : this(resourceManager, texture, width, height, addReference: false)
    {
    }

    private Direct3D9Texture(
        Direct3D9ResourceManager resourceManager,
        IDirect3DTexture9* texture,
        uint width,
        uint height,
        bool addReference,
        uint resourceSize = 0)
        : base(resourceManager, resourceSize: resourceSize)
    {
        _resourceManager = resourceManager;
        _texture = texture;
        if (addReference)
        {
            AddRef(texture);
        }

        _width = width;
        _height = height;
    }

    internal uint Width
    {
        get
        {
            ObjectDisposedException.ThrowIf(!IsValid, this);
            return _width;
        }
    }

    internal uint Height
    {
        get
        {
            ObjectDisposedException.ThrowIf(!IsValid, this);
            return _height;
        }
    }

    internal uint LevelCount { get; private init; } = 1;

    internal override bool RequiresDelayedRelease => true;

    internal static int TryCreate(
        Direct3D9ResourceManager resourceManager,
        IDirect3DTexture9* existingTexture,
        bool isEvictable,
        out Direct3D9Texture? texture)
    {
        ArgumentNullException.ThrowIfNull(resourceManager);
        texture = null;
        if (existingTexture is null)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        void** vtable = existingTexture->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint> getLevelCount =
            (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) vtable[13];
        delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, SurfaceDesc*, int> getLevelDescription =
            (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, SurfaceDesc*, int>) vtable[17];

        uint levelCount = getLevelCount(existingTexture);
        if (levelCount is < 1 or > 32)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        SurfaceDesc levelZeroDescription = default;
        uint resourceSize = 0;
        int result = 0;
        for (uint level = 0; level < levelCount; level++)
        {
            SurfaceDesc description = default;
            result = getLevelDescription(existingTexture, level, &description);
            if (result < 0)
            {
                return result;
            }

            uint formatSize = GetFormatSize(description.Format);
            if (formatSize == 0)
            {
                return Direct3D9Factory.WrongTextureFormatHResult;
            }

            if (level == 0)
            {
                levelZeroDescription = description;
            }

            resourceSize += description.Width * description.Height * formatSize;
        }

        texture = new Direct3D9Texture(
            resourceManager,
            existingTexture,
            levelZeroDescription.Width,
            levelZeroDescription.Height,
            addReference: true,
            resourceSize)
        {
            LevelCount = levelCount
        };
        if (isEvictable)
        {
            texture.SetAsEvictable();
        }

        return result;
    }

    internal IDirect3DTexture9* Texture
    {
        get
        {
            ObjectDisposedException.ThrowIf(!IsValid || _texture is null, this);
            return _texture;
        }
    }

    internal int TryGetSurfaceLevel(uint level, out Direct3D9Surface? surfaceLevel)
    {
        ObjectDisposedException.ThrowIf(!IsValid || _texture is null, this);
        surfaceLevel = null;
        if (level >= LevelCount)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        _resourceManager.Use(this);
        _surfaceLevels ??= new Direct3D9Surface?[LevelCount];
        Direct3D9Surface? cachedSurface = _surfaceLevels[level];
        if (cachedSurface is not null)
        {
            surfaceLevel = cachedSurface.AddRef();
            return 0;
        }

        IDirect3DSurface9* nativeSurface = null;
        void** vtable = _texture->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int> getSurfaceLevel =
            (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>) vtable[18];
        int result = getSurfaceLevel(_texture, level, &nativeSurface);
        try
        {
            if (result < 0)
            {
                return result;
            }
            if (nativeSurface is null)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            result = Direct3D9Surface.TryCreateTextureLevel(_resourceManager, nativeSurface, out cachedSurface);
            if (result < 0)
            {
                return result;
            }

            _surfaceLevels[level] = cachedSurface;
            surfaceLevel = cachedSurface!.AddRef();
            return result;
        }
        finally
        {
            Direct3D9Factory.Release(nativeSurface);
        }
    }

    internal int TryGetSurfaceLevelReference(uint level, out nint surface)
    {
        int result = TryGetSurfaceLevel(level, out Direct3D9Surface? surfaceLevel);
        if (result < 0 || surfaceLevel is null)
        {
            surface = 0;
            return result < 0 ? result : Direct3D9Factory.GenericFailureHResult;
        }

        using (surfaceLevel)
        {
            surface = (nint) surfaceLevel.Surface;
            Direct3D9Factory.AddRef(surface);
            return Direct3D9Factory.SuccessHResult;
        }
    }

    internal int UpdateMipmapLevels(Direct3D9Device device)
    {
        ArgumentNullException.ThrowIfNull(device);
        ObjectDisposedException.ThrowIf(!IsValid || _texture is null, this);
        if (LevelCount <= 1)
        {
            return 0;
        }

        if (device.CanAutoGenerateMipmaps)
        {
            void** vtable = _texture->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3DTexture9*, void> generateMipSubLevels =
                (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, void>) vtable[16];
            generateMipSubLevels(_texture);
            return 0;
        }

        int result = TryGetSurfaceLevel(0, out Direct3D9Surface? source);
        if (result < 0)
        {
            return result;
        }

        try
        {
            for (uint level = 1; level < LevelCount; level++)
            {
                result = TryGetSurfaceLevel(level, out Direct3D9Surface? destination);
                if (result < 0)
                {
                    return result;
                }

                try
                {
                    result = device.StretchRect(
                        source!,
                        null,
                        destination!.SurfaceForDeviceCall,
                        null,
                        Texturefiltertype.Linear);
                    if (result < 0)
                    {
                        return result;
                    }

                    source.Dispose();
                    source = destination;
                    destination = null;
                }
                finally
                {
                    destination?.Dispose();
                }
            }

            return 0;
        }
        finally
        {
            source?.Dispose();
        }
    }

    internal int LockRect(out LockedRect lockedRect, Direct3D9SurfaceRect? rectangle, uint flags)
    {
        ObjectDisposedException.ThrowIf(!IsValid || _texture is null, this);

        lockedRect = default;
        void** vtable = _texture->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, LockedRect*, Direct3D9SurfaceRect*, uint, int> lockRect =
            (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, LockedRect*, Direct3D9SurfaceRect*, uint, int>) vtable[19];
        fixed (LockedRect* lockedRectPointer = &lockedRect)
        {
            if (rectangle is not { } rectangleValue)
            {
                return lockRect(_texture, 0, lockedRectPointer, null, flags);
            }

            return lockRect(_texture, 0, lockedRectPointer, &rectangleValue, flags);
        }
    }

    internal int UnlockRect()
    {
        ObjectDisposedException.ThrowIf(!IsValid || _texture is null, this);

        void** vtable = _texture->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, int> unlockRect =
            (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, int>) vtable[20];
        return unlockRect(_texture, 0);
    }

    internal int AddDirtyRect(Direct3D9SurfaceRect rectangle)
    {
        ObjectDisposedException.ThrowIf(!IsValid || _texture is null, this);

        void** vtable = _texture->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DTexture9*, Direct3D9SurfaceRect*, int> addDirtyRect =
            (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, Direct3D9SurfaceRect*, int>) vtable[21];
        return addDirtyRect(_texture, &rectangle);
    }

    private static void AddRef(IDirect3DTexture9* texture)
    {
        void** vtable = texture->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint> addRef =
            (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) vtable[1];
        _ = addRef(texture);
    }

    private static uint GetFormatSize(Format format)
    {
        return format switch
        {
            Format.A32B32G32R32f => 16,
            Format.A8R8G8B8 or Format.X8R8G8B8 or Format.D24S8 or Format.A2R10G10B10 => 4,
            Format.R8G8B8 => 3,
            Format.R5G6B5 or Format.X1R5G5B5 or Format.D16 => 2,
            Format.P8 or Format.L8 => 1,
            _ => 0
        };
    }

    protected override void ReleaseD3DResources()
    {
        Direct3D9Factory.Release(_texture);
        _texture = null;

        if (_surfaceLevels is not null)
        {
            for (int level = _surfaceLevels.Length - 1; level >= 0; level--)
            {
                _surfaceLevels[level]?.Dispose();
                _surfaceLevels[level] = null;
            }

            _surfaceLevels = null;
        }
    }
}
