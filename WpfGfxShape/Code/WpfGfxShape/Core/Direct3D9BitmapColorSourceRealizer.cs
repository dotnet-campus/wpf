using System.Numerics;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9BitmapColorSourceTextureRealizer : IDisposable
{
    private readonly Func<(int Result, Direct3D9BitmapColorSourceTextureRealization? Realization)> _createRealization;
    private readonly Action _disposePendingRealizationState;
    private Direct3D9BitmapColorSourceTextureRealization? _realization;
    private bool _isDisposed;

    internal Direct3D9BitmapColorSourceTextureRealizer(
        Func<(int Result, Direct3D9BitmapColorSourceTextureRealization? Realization)> createRealization,
        Action? disposePendingRealizationState = null)
    {
        ArgumentNullException.ThrowIfNull(createRealization);
        _createRealization = createRealization;
        _disposePendingRealizationState = disposePendingRealizationState ?? (() => { });
    }

    internal static Direct3D9BitmapColorSourceTextureRealizer Create(
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
        Func<Direct3D9BitmapReusableRealizationContext?>? createReusableContext = null)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(realizationBounds);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapSource);

        Direct3D9BitmapReusableRealizationContext? pendingReusableContext = createReusableContext?.Invoke();
        return new Direct3D9BitmapColorSourceTextureRealizer(
            () =>
            {
                Direct3D9BitmapReusableRealizationContext? reusableContext = pendingReusableContext;
                pendingReusableContext = null;
                int result = Direct3D9BitmapColorSourceTextureRealization.TryCreate(
                    device,
                    bitmap,
                    bitmapSource,
                    bitmapSourceIsBitmap,
                    createAsRenderTarget,
                    isEvictable,
                    properties,
                    isDeviceBitmap,
                    realizationBounds,
                    bitmapToXSpace,
                    reusableContext,
                    out Direct3D9BitmapColorSourceTextureRealization? realization);
                return (result, realization);
            },
            () =>
            {
                pendingReusableContext?.Dispose();
                pendingReusableContext = null;
            });
    }

    internal Direct3D9PipelineColorSource CreatePipelineColorSource(
        Direct3D9Device device,
        Direct3D9BitmapRealizationProperties properties,
        Matrix3x2 bitmapToXSpace,
        bool useHardwareTransform,
        uint? shaderTextureTransformRegister,
        Direct3D9SetPipelineShaderMatrix3x2? setShaderMatrix = null)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(device);

        int result = Direct3D9BitmapColorSourceTransform.Calculate(
            bitmapToXSpace,
            properties.LayoutU.Length,
            properties.LayoutV.Length,
            properties.BitmapWidth,
            properties.BitmapHeight,
            properties.Width,
            properties.Height,
            properties.SourceContained,
            properties.LayoutU.TexelLayout,
            properties.LayoutV.TexelLayout,
            out Matrix3x2 xSpaceToTextureUv);
        if (result < 0)
        {
            throw new InvalidOperationException("The bitmap color source texture transform is not invertible.");
        }

        Direct3D9BitmapColorSourceDeviceState state = new(
            device,
            GetTexture,
            properties.InterpolationMode,
            properties.LayoutU.TextureAddress,
            properties.LayoutV.TextureAddress,
            useHardwareTransform,
            shaderTextureTransformRegister,
            xSpaceToTextureUv,
            setShaderMatrix);
        return state.CreatePipelineColorSource(
            Realize,
            isOpaque: !MilPixelFormatInfo.HasAlphaChannel(properties.TextureFormat));
    }

    internal Direct3D9BitmapColorSourceRealizationState? RealizationState
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _realization?.State;
        }
    }

    internal Direct3D9Texture? RealizedTexture
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _realization?.Texture;
        }
    }

    internal Direct3D9BitmapSystemMemorySurfaceSource? SystemMemorySurfaceSource
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _realization?.SystemMemorySurfaceSource;
        }
    }

    internal bool IsRenderTarget
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _realization?.IsRenderTarget ?? false;
        }
    }

    internal int Realize()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        try
        {
            if (_realization is not null && !_realization.Texture.IsValid)
            {
                _realization.Dispose();
                _realization = null;
            }

            if (_realization is null)
            {
                (int result, Direct3D9BitmapColorSourceTextureRealization? realization) = _createRealization();
                if (result < 0)
                {
                    realization?.Dispose();
                    return result;
                }
                if (realization is null)
                {
                    return Direct3D9Factory.GenericFailureHResult;
                }

                _realization = realization;
                _realization.State.ResetCachedRealization();
            }

            return _realization.State.IsRealizationValid()
                ? Direct3D9Factory.SuccessHResult
                : _realization.Update();
        }
        finally
        {
            _realization?.ReleaseRealizationSources();
        }
    }

    private IDirect3DBaseTexture9* GetTexture()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _realization is { Texture.IsValid: true }
            ? (IDirect3DBaseTexture9*) _realization.Texture.Texture
            : null;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _realization?.Dispose();
        _realization = null;
        _disposePendingRealizationState();
        _isDisposed = true;
    }
}
