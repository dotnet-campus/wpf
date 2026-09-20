using System.Numerics;
using System.Runtime.Versioning;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed class Direct3D9BitmapColorSourceTextureRealization : IDisposable
{
    private readonly Direct3D9BitmapColorSourceTextureUpdater _updater;
    private readonly IDisposable _systemMemorySurfaceSource;
    private readonly IDisposable _texture;
    private readonly IDisposable? _reusableContext;
    private readonly Action _releaseRealizationSources;
    private bool _isDisposed;

    internal Direct3D9BitmapColorSourceTextureRealization(
        Direct3D9BitmapColorSourceTextureUpdater updater,
        IDisposable systemMemorySurfaceSource,
        IDisposable texture,
        IDisposable? reusableContext = null,
        Action? releaseRealizationSources = null)
    {
        _updater = updater;
        _systemMemorySurfaceSource = systemMemorySurfaceSource;
        _texture = texture;
        _reusableContext = reusableContext;
        _releaseRealizationSources = releaseRealizationSources ?? (() => { });
    }

    internal Direct3D9BitmapColorSourceTextureRealization(
        Direct3D9BitmapColorSourceTextureUpdater updater,
        Direct3D9BitmapSystemMemorySurfaceSource systemMemorySurfaceSource,
        Direct3D9Texture texture,
        Direct3D9BitmapColorSourceRealizationState state,
        bool isRenderTarget,
        Direct3D9BitmapReusableRealizationContext? reusableContext = null)
        : this(
            updater,
            systemMemorySurfaceSource,
            texture,
            reusableContext,
            reusableContext is null ? null : reusableContext.Sources.ReleaseSources)
    {
        ArgumentNullException.ThrowIfNull(state);

        State = state;
        Texture = texture;
        IsRenderTarget = isRenderTarget;
    }

    internal Direct3D9BitmapColorSourceRealizationState State { get; private init; } = null!;

    internal Direct3D9Texture Texture { get; private init; } = null!;

    internal Direct3D9BitmapSystemMemorySurfaceSource SystemMemorySurfaceSource =>
        _systemMemorySurfaceSource as Direct3D9BitmapSystemMemorySurfaceSource
        ?? throw new InvalidOperationException("The bitmap color source realization does not own a production system-memory surface source.");

    internal bool IsRenderTarget { get; private init; }

    internal static int TryCreate(
        Direct3D9Device device,
        nint bitmap,
        nint bitmapSource,
        bool bitmapSourceIsBitmap,
        bool createAsRenderTarget,
        bool isEvictable,
        Direct3D9BitmapRealizationProperties properties,
        bool isDeviceBitmap,
        Direct3D9DelayedBounds realizationBounds,
        Matrix3x2 bitmapToXSpace,
        Direct3D9BitmapReusableRealizationContext? reusableContext,
        out Direct3D9BitmapColorSourceTextureRealization? realization)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(realizationBounds);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapSource);

        realization = null;
        bool ownsReusableContext = reusableContext is not null;
        Direct3D9BitmapTextureRequirements requirements =
            Direct3D9BitmapRealizationParameterComputer.GetTextureCreationRequirements(
                device.CanAutoGenerateMipmaps,
                createAsRenderTarget,
                properties);

        int result = device.TryCreateBitmapTexture(requirements, isEvictable, out Direct3D9Texture? texture);
        if (result < 0)
        {
            texture?.Dispose();
            reusableContext?.Dispose();
            return result;
        }
        if (texture is null)
        {
            throw new InvalidOperationException("Direct3D bitmap texture creation returned a null texture.");
        }

        Direct3D9BitmapSystemMemorySurfaceSource? surfaceSource = null;
        try
        {
            Direct3D9BitmapColorSourceRealizationState state = new(
                bitmap,
                properties.TextureFormat,
                Direct3D9Bitmap.GetUniquenessToken);
            result = reusableContext is null
                ? state.SetBitmapAndContext(
                    bitmapSource,
                    properties,
                    isDeviceBitmap,
                    realizationBounds,
                    bitmapToXSpace)
                : state.SetBitmapAndContext(
                    bitmapSource,
                    properties,
                    isDeviceBitmap,
                    reusableContext.HasContributorFromDifferentAdapter,
                    realizationBounds,
                    reusableContext.Candidates,
                    reusableContext.Sources,
                    bitmapToXSpace);
            if (result < 0)
            {
                return result;
            }

            surfaceSource = new Direct3D9BitmapSystemMemorySurfaceSource(
                device,
                requirements.Description.Format);
            Direct3D9BitmapTexturePopulationPreparer populationPreparer = new(
                bitmapSource,
                bitmapSourceIsBitmap,
                properties.LayoutU.TexelLayout,
                properties.LayoutV.TexelLayout,
                device.IsLddmDevice,
                properties.Width,
                properties.Height,
                requirements.Description.Width,
                requirements.Description.Height,
                surfaceSource);
            Direct3D9BitmapTextureLevelZeroPusher levelZeroPusher = new(
                texture,
                device,
                requirements,
                properties.LayoutU.TexelLayout,
                properties.LayoutV.TexelLayout,
                properties.SourceContained);
            Direct3D9BitmapTexturePopulator populator = new(
                populationPreparer,
                levelZeroPusher.Push,
                reusableContext?.Sources,
                reusableContext?.Population,
                () => texture.UpdateMipmapLevels(device),
                state.Commit,
                Direct3D9Factory.Release);
            Direct3D9BitmapColorSourceTextureUpdater updater = new(state, populator);

            realization = new Direct3D9BitmapColorSourceTextureRealization(
                updater,
                surfaceSource,
                texture,
                reusableContext)
            {
                State = state,
                Texture = texture,
                IsRenderTarget = createAsRenderTarget
            };
            surfaceSource = null;
            texture = null;
            ownsReusableContext = false;
            return Direct3D9Factory.SuccessHResult;
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }
        finally
        {
            surfaceSource?.Dispose();
            texture?.Dispose();
            if (ownsReusableContext)
            {
                reusableContext?.Dispose();
            }
        }
    }

    internal int Update()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _updater.Update();
    }

    internal void ReleaseRealizationSources()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _releaseRealizationSources();
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _systemMemorySurfaceSource.Dispose();
        _texture.Dispose();
        _reusableContext?.Dispose();
        _isDisposed = true;
    }
}
