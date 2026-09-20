namespace WpfGfxShape.Core;

internal sealed class Direct3D9BitmapColorSourceOwner
{
    internal Direct3D9BitmapColorSourceOwner(
        Direct3D9BitmapColorSourceTextureRealizer textureRealizer,
        bool isDeviceBitmap)
    {
        ArgumentNullException.ThrowIfNull(textureRealizer);

        TextureRealizer = textureRealizer;
        IsDeviceBitmap = isDeviceBitmap;
    }

    internal Direct3D9BitmapColorSourceTextureRealizer TextureRealizer { get; }

    internal bool IsDeviceBitmap { get; }

    internal Direct3D9BitmapColorSourceRealizationState RealizationState =>
        TextureRealizer.RealizationState
        ?? throw new InvalidOperationException("The bitmap color source has not been realized.");

    internal Direct3D9Texture Texture =>
        TextureRealizer.RealizedTexture
        ?? throw new InvalidOperationException("The bitmap color source has not been realized.");

    internal bool IsRenderTarget
    {
        get
        {
            _ = RealizationState;
            return TextureRealizer.IsRenderTarget;
        }
    }

    internal Direct3D9BitmapSystemMemorySurfaceSource SystemMemorySurfaceSource =>
        TextureRealizer.SystemMemorySurfaceSource
        ?? throw new InvalidOperationException("The bitmap color source has not been realized.");

    internal unsafe Direct3D9DeviceBitmapColorSourcePixelCopier CreatePixelCopier(Direct3D9Device device)
    {
        ArgumentNullException.ThrowIfNull(device);
        Direct3D9BitmapColorSourceRealizationState state = RealizationState;

        return new Direct3D9DeviceBitmapColorSourcePixelCopier(
            device,
            state.PrefilteredBitmap,
            state.CachedRealizationBounds,
            (out nint surface) => Texture.TryGetSurfaceLevelReference(0, out surface),
            (surface, sourceRectangle, clipRectangles, outputFormat, outputStride, outputBufferSize, outputBuffer) =>
            {
                int result = Direct3D9Surface.TryCreateTextureLevel(
                    device.ResourceManager,
                    (Silk.NET.Direct3D9.IDirect3DSurface9*) surface,
                    out Direct3D9Surface? sourceSurface);
                if (result < 0 || sourceSurface is null)
                {
                    return result < 0 ? result : Direct3D9Factory.GenericFailureHResult;
                }

                using (sourceSurface)
                {
                    return sourceSurface.ReadIntoSystemMemoryBuffer(
                        sourceRectangle,
                        clipRectangles,
                        outputFormat,
                        outputStride,
                        outputBufferSize,
                        outputBuffer);
                }
            },
            Direct3D9Factory.Release);
    }
}

internal sealed class Direct3D9BitmapColorSourceRegistry
{
    private readonly Dictionary<nint, Direct3D9BitmapColorSourceOwner> _owners = [];

    internal void Register(nint bitmapColorSource, Direct3D9BitmapColorSourceOwner owner)
    {
        ArgumentOutOfRangeException.ThrowIfZero(bitmapColorSource);
        ArgumentNullException.ThrowIfNull(owner);

        if (!_owners.TryAdd(bitmapColorSource, owner))
        {
            throw new InvalidOperationException("The bitmap color source is already registered.");
        }
    }

    internal void Unregister(nint bitmapColorSource, Direct3D9BitmapColorSourceOwner owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        if (_owners.TryGetValue(bitmapColorSource, out Direct3D9BitmapColorSourceOwner? registeredOwner)
            && ReferenceEquals(registeredOwner, owner))
        {
            _owners.Remove(bitmapColorSource);
        }
    }

    internal Direct3D9BitmapColorSourceOwner Resolve(nint bitmapColorSource)
    {
        ArgumentOutOfRangeException.ThrowIfZero(bitmapColorSource);

        return _owners.TryGetValue(bitmapColorSource, out Direct3D9BitmapColorSourceOwner? owner)
            ? owner
            : throw new InvalidOperationException("The bitmap color source is not registered.");
    }

    internal Direct3D9DeviceBitmapColorSourcePixelCopier CreatePixelCopier(
        nint bitmapColorSource,
        Direct3D9Device device) => Resolve(bitmapColorSource).CreatePixelCopier(device);

    internal Direct3D9BitmapReusableRealizationSources CreateReusableRealizationSources(
        nint bitmapColorSource,
        bool canStretchRectFromTextures,
        Func<nint, nint> getNext,
        Action<nint, nint> setNext,
        Action<nint> addReference,
        Action<nint> release)
    {
        Direct3D9BitmapColorSourceOwner targetOwner = Resolve(bitmapColorSource);
        Direct3D9BitmapColorSourceRealizationState targetState = targetOwner.RealizationState;
        Direct3D9BitmapReusableRealizationTargetState target = new(
            targetState.Bitmap,
            targetOwner.IsRenderTarget,
            canStretchRectFromTextures,
            targetState.PrefilterWidth,
            targetState.PrefilterHeight,
            targetState.RequiredRealizationBounds,
            targetState.CachedUniquenessToken);

        return new Direct3D9BitmapReusableRealizationSources(
            target,
            (nint source, out Direct3D9BitmapReusableRealizationSourceState state) =>
            {
                state = ResolveReusableSourceState(source);
                return true;
            },
            getNext,
            setNext,
            addReference,
            release,
            source => TryAdoptSystemMemorySurface(bitmapColorSource, source));
    }

    internal bool TryAdoptSystemMemorySurface(nint targetBitmapColorSource, nint reusableBitmapColorSource)
    {
        Direct3D9BitmapColorSourceOwner targetOwner = Resolve(targetBitmapColorSource);
        Direct3D9BitmapColorSourceOwner reusableOwner = Resolve(reusableBitmapColorSource);
        return targetOwner.SystemMemorySurfaceSource.TryAdoptCachedSurfaceFrom(
            reusableOwner.SystemMemorySurfaceSource);
    }

    internal Direct3D9BitmapReusableRealizationSourceState ResolveReusableSourceState(nint bitmapColorSource)
    {
        Direct3D9BitmapColorSourceOwner owner = Resolve(bitmapColorSource);
        Direct3D9BitmapColorSourceRealizationState realizationState = owner.RealizationState;
        bool hasValidDirtyRectInformation = Direct3D9Bitmap.GetDirtyRectangles(
            realizationState.Bitmap,
            realizationState.CachedUniquenessToken,
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
            out _);
        bool isCompletelyDirty = hasValidDirtyRectInformation
            && dirtyRectangles.Count == 1
            && dirtyRectangles[0] == new Direct3D9BitmapRealizationRectangle(
                0,
                0,
                realizationState.BitmapWidth,
                realizationState.BitmapHeight);

        return new Direct3D9BitmapReusableRealizationSourceState(
            realizationState.Bitmap,
            owner.Texture.IsValid,
            owner.IsRenderTarget,
            realizationState.PrefilterWidth,
            realizationState.PrefilterHeight,
            realizationState.RequiredRealizationBounds,
            realizationState.CachedUniquenessToken,
            hasValidDirtyRectInformation,
            isCompletelyDirty);
    }
}
