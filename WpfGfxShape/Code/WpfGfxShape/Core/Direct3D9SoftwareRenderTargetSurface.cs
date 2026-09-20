using System.Numerics;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9LockSoftwareRenderTarget(
    out byte[] targetPixels,
    out int targetStride);

internal sealed record Direct3D9SoftwareRenderTargetLockCallbacks(
    Func<int> Lock,
    Func<(int Result, int Stride)> GetStride,
    Func<(int Result, byte[]? Pixels)> GetDataPointer,
    Action Release);

internal delegate int Direct3D9CreateSoftware3DSurface(
    out Direct3D9Software3DSurface? software3DSurface);

internal readonly record struct Direct3D9SoftwareRenderTargetBinding(
    uint Width,
    uint Height,
    MilPixelFormat PixelFormat,
    double DpiX,
    double DpiY);

internal readonly record struct Direct3D9SoftwareRenderTargetState(
    uint Width,
    uint Height,
    MilPixelFormat PixelFormat,
    MilPixelFormat ColorDataPixelFormat,
    uint BytesPerPixel,
    float DpiX,
    float DpiY);

internal delegate int Direct3D9BindSoftwareRenderTarget(
    out Direct3D9SoftwareRenderTargetBinding binding);

internal delegate int Direct3D9ClearSoftwareRenderTarget(
    byte[] targetPixels,
    int targetStride,
    Direct3D9SoftwareRenderTargetState state,
    MilColorF color,
    Direct3D9SurfaceRect clip);

internal delegate int Direct3D9DrawBitmapSoftwareRenderTarget(
    byte[] targetPixels,
    int targetStride,
    Direct3D9SoftwareRenderTargetState state,
    Direct3D9SurfaceRect clip);

internal delegate int Direct3D9DrawPathSoftwareRenderTarget(
    byte[] targetPixels,
    int targetStride,
    Direct3D9SoftwareRenderTargetState state,
    Direct3D9SurfaceRect clip);

internal delegate int Direct3D9DrawGlyphsSoftwareRenderTarget(
    byte[] targetPixels,
    int targetStride,
    Direct3D9SoftwareRenderTargetState state,
    Direct3D9SurfaceRect clip,
    float alphaScale,
    bool targetSupportsClearType);

internal delegate int Direct3D9BeginSoftwareVideoRender(out nint bitmapSource);

internal sealed record Direct3D9SoftwareVideoSurfaceRenderer(
    Direct3D9BeginSoftwareVideoRender BeginRender,
    Func<int> EndRender);

internal readonly record struct Direct3D9SoftwareRenderTargetBitmapCreationRequest(
    uint Width,
    uint Height,
    MilPixelFormat PixelFormat,
    float DpiX,
    float DpiY,
    uint? AssociatedDisplayIndex,
    Direct3D9RenderTargetInitializationFlags InitializationFlags);

internal delegate int Direct3D9CreateSoftwareRenderTargetBitmapSurface(
    Direct3D9SoftwareRenderTargetBitmapCreationRequest request,
    out nint internalSurface);

internal delegate int Direct3D9WrapSoftwareRenderTargetBitmap(
    nint internalSurface,
    Direct3D9SoftwareRenderTargetBitmapCreationRequest request,
    out Direct3D9SoftwareRenderTargetBitmap? renderTargetBitmap);

internal sealed class Direct3D9SoftwareRenderTargetSurface : IDisposable
{
    private Direct3D9SoftwareRenderTargetState _state;
    private Matrix3x2 _deviceTransform;
    private Direct3D9SurfaceRect _currentClip;
    private uint _resizeUniqueness = 1;
    private readonly Direct3D9LockSoftwareRenderTarget _lockTarget;
    private readonly Action _unlockTarget;
    private readonly Direct3D9SoftwareRenderTargetLockCallbacks? _stagedLockTarget;
    private bool _stagedTargetLockAcquired;
    private readonly Direct3D9CreateSoftware3DSurface _createSoftware3DSurface;
    private readonly Action<Direct3D9Software3DSurface> _cleanup3DResources;
    private readonly Action<Direct3D9Software3DSurface> _releaseSoftware3DSurface;
    private readonly Direct3D9SoftwareIntermediateBuffers _intermediateBuffers;
    private readonly Func<Direct3D9SoftwareRenderTargetState, int> _allocateIntermediateBuffers;
    private readonly Func<int> _initializeBaseRenderTarget;
    private readonly Func<Direct3D9Software3DSurface, uint, uint, int> _resizeSoftware3DSurface;
    private readonly Action _cleanupSurfaceBinding;
    private readonly Action _freeIntermediateBuffers;
    private readonly Direct3D9ClearSoftwareRenderTarget _clearSoftwareRenderTarget;
    private readonly Direct3D9DrawBitmapSoftwareRenderTarget _drawBitmapSoftwareRenderTarget;
    private readonly Direct3D9DrawPathSoftwareRenderTarget _fillPathSoftwareRenderTarget;
    private readonly Func<int> _widenPath;
    private readonly Direct3D9DrawPathSoftwareRenderTarget _strokePathSoftwareRenderTarget;
    private readonly Func<int> _ensureGlyphBrushRealization;
    private readonly Func<bool> _hasRealizedGlyphBrush;
    private readonly Func<float> _getGlyphBrushOpacity;
    private readonly Direct3D9DrawGlyphsSoftwareRenderTarget _drawGlyphsSoftwareRenderTarget;
    private readonly bool _forceClearType;
    private readonly Direct3D9SoftwareScanPipeline _scanPipeline;
    private readonly Direct3D9SoftwareIntermediateRenderTargetCreator _intermediateRenderTargetCreator;
    private Direct3D9Software3DSurface? _software3DSurface;
    private byte[]? _targetPixels;
    private int _targetStride;
    private bool _in3D;
    private bool _isDisposed;

    internal Direct3D9SoftwareRenderTargetSurface(
        uint width,
        uint height,
        Direct3D9LockSoftwareRenderTarget lockTarget,
        Action unlockTarget,
        Direct3D9CreateSoftware3DSurface createSoftware3DSurface,
        Action<Direct3D9Software3DSurface> cleanup3DResources,
        Action<Direct3D9Software3DSurface> releaseSoftware3DSurface,
        MilPixelFormat pixelFormat = MilPixelFormat.Pbgra32Bpp,
        double dpiX = 96.0,
        double dpiY = 96.0,
        Func<Direct3D9SoftwareRenderTargetState, int>? allocateIntermediateBuffers = null,
        Func<int>? initializeBaseRenderTarget = null,
        Func<Direct3D9Software3DSurface, uint, uint, int>? resizeSoftware3DSurface = null,
        Action? cleanupSurfaceBinding = null,
        Action? freeIntermediateBuffers = null,
        Direct3D9ClearSoftwareRenderTarget? clearSoftwareRenderTarget = null,
        Direct3D9DrawBitmapSoftwareRenderTarget? drawBitmapSoftwareRenderTarget = null,
        Direct3D9DrawPathSoftwareRenderTarget? fillPathSoftwareRenderTarget = null,
        Func<int>? widenPath = null,
        Direct3D9DrawPathSoftwareRenderTarget? strokePathSoftwareRenderTarget = null,
        Func<int>? ensureGlyphBrushRealization = null,
        Func<bool>? hasRealizedGlyphBrush = null,
        Func<float>? getGlyphBrushOpacity = null,
        Direct3D9DrawGlyphsSoftwareRenderTarget? drawGlyphsSoftwareRenderTarget = null,
        bool forceClearType = false,
        Direct3D9SoftwareRenderTargetLockCallbacks? stagedLockTarget = null,
        uint? associatedDisplayIndex = null,
        Direct3D9AcquireCurrentDisplaySet? acquireCurrentDisplaySet = null,
        Action<Direct3D9DisplaySet>? releaseDisplaySet = null,
        Direct3D9SoftwareScanPipelineCallbacks? scanPipelineCallbacks = null)
    {
        ArgumentNullException.ThrowIfNull(lockTarget);
        ArgumentNullException.ThrowIfNull(unlockTarget);
        ArgumentNullException.ThrowIfNull(createSoftware3DSurface);
        ArgumentNullException.ThrowIfNull(cleanup3DResources);
        ArgumentNullException.ThrowIfNull(releaseSoftware3DSurface);

        int result = TryCreateState(
            new Direct3D9SoftwareRenderTargetBinding(width, height, pixelFormat, dpiX, dpiY),
            out _state);
        if (result < 0)
        {
            throw new ArgumentException("The initial software render target state is invalid.", nameof(pixelFormat));
        }

        _deviceTransform = CreateDeviceTransform(_state);
        _lockTarget = lockTarget;
        _unlockTarget = unlockTarget;
        _stagedLockTarget = stagedLockTarget;
        _createSoftware3DSurface = createSoftware3DSurface;
        _cleanup3DResources = cleanup3DResources;
        _releaseSoftware3DSurface = releaseSoftware3DSurface;
        _intermediateBuffers = new Direct3D9SoftwareIntermediateBuffers();
        _allocateIntermediateBuffers = allocateIntermediateBuffers ?? (state => _intermediateBuffers.AllocateBuffers(state.Width));
        _initializeBaseRenderTarget = initializeBaseRenderTarget ?? (() => Direct3D9Factory.SuccessHResult);
        _resizeSoftware3DSurface = resizeSoftware3DSurface ?? ((surface, newWidth, newHeight) =>
        {
            surface.Resize(newWidth, newHeight);
            return Direct3D9Factory.SuccessHResult;
        });
        _cleanupSurfaceBinding = cleanupSurfaceBinding ?? (() => { });
        _freeIntermediateBuffers = freeIntermediateBuffers ?? _intermediateBuffers.FreeBuffers;
        _clearSoftwareRenderTarget = clearSoftwareRenderTarget ?? ((_, _, _, _, _) => Direct3D9Factory.GenericFailureHResult);
        _drawBitmapSoftwareRenderTarget = drawBitmapSoftwareRenderTarget ?? ((_, _, _, _) => Direct3D9Factory.GenericFailureHResult);
        _fillPathSoftwareRenderTarget = fillPathSoftwareRenderTarget ?? ((_, _, _, _) => Direct3D9Factory.GenericFailureHResult);
        _widenPath = widenPath ?? (() => Direct3D9Factory.GenericFailureHResult);
        _strokePathSoftwareRenderTarget = strokePathSoftwareRenderTarget ?? ((_, _, _, _) => Direct3D9Factory.GenericFailureHResult);
        _ensureGlyphBrushRealization = ensureGlyphBrushRealization ?? (() => Direct3D9Factory.GenericFailureHResult);
        _hasRealizedGlyphBrush = hasRealizedGlyphBrush ?? (() => false);
        _getGlyphBrushOpacity = getGlyphBrushOpacity ?? (() => 1f);
        _drawGlyphsSoftwareRenderTarget = drawGlyphsSoftwareRenderTarget ?? ((_, _, _, _, _, _) => Direct3D9Factory.GenericFailureHResult);
        _forceClearType = forceClearType;
        _scanPipeline = new Direct3D9SoftwareScanPipeline(scanPipelineCallbacks);
        _intermediateRenderTargetCreator = new Direct3D9SoftwareIntermediateRenderTargetCreator(
            _state.PixelFormat,
            associatedDisplayIndex,
            acquireCurrentDisplaySet,
            releaseDisplaySet);
    }

    internal bool In3D => _in3D;

    internal bool HasSoftware3DSurface => _software3DSurface is not null;

    internal Direct3D9SoftwareRenderTargetState State => _state;

    internal Direct3D9SoftwareIntermediateBuffers IntermediateBuffers => _intermediateBuffers;

    internal uint GetResizeUniqueness()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _resizeUniqueness;
    }

    internal Matrix3x2 GetDeviceTransform()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _deviceTransform;
    }

    internal Direct3D9SurfaceRect GetCurrentClip()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _currentClip;
    }

    internal InternalRenderTargetType GetRenderTargetType()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return InternalRenderTargetType.SoftwareRaster;
    }

    internal uint GetRealizationCacheIndex()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return Direct3D9ImmediateBrushRealizer.SoftwareRealizationCacheIndex;
    }

    internal bool HasAlpha()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _state.PixelFormat == MilPixelFormat.Pbgra32Bpp;
    }

    internal int CreateRenderTargetBitmap(
        uint width,
        uint height,
        Direct3D9IntermediateRenderTargetUsage usage,
        Direct3D9RenderTargetInitializationFlags initializationFlags,
        Direct3D9CreateSoftwareRenderTargetBitmapSurface createInternalSurface,
        out Direct3D9SoftwareRenderTargetBitmap? renderTargetBitmap,
        Direct3D9WrapSoftwareRenderTargetBitmap? wrapRenderTargetBitmap = null)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _intermediateRenderTargetCreator.CreateRenderTargetBitmap(
            width,
            height,
            usage,
            initializationFlags,
            createInternalSurface,
            out renderTargetBitmap,
            wrapRenderTargetBitmap);
    }

    internal int ReadEnabledDisplays(Span<bool> enabledDisplays)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _intermediateRenderTargetCreator.ReadEnabledDisplays(enabledDisplays);
    }

    internal int SetupPipeline(
        MilPixelFormat colorDataPixelFormat,
        nint colorSource,
        bool perPrimitiveAntiAliasing,
        bool complementAlpha,
        MilCompositingMode compositingMode,
        uint clipWidth,
        nint effectList = 0,
        nint effectToDevice = 0,
        nint contextState = 0)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _ = colorDataPixelFormat;
        return _scanPipeline.InitializeForRendering(
            new Direct3D9SoftwareRenderingPipelineRequest(
                _state.PixelFormat,
                colorSource,
                perPrimitiveAntiAliasing,
                complementAlpha,
                compositingMode,
                clipWidth,
                effectList,
                effectToDevice,
                contextState));
    }

    internal int SetupPipelineForText(
        nint colorSource,
        MilCompositingMode compositingMode,
        nint glyphPainter,
        bool needsAntiAliasing)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _scanPipeline.InitializeForTextRendering(
            new Direct3D9SoftwareTextPipelineRequest(
                _state.PixelFormat,
                colorSource,
                compositingMode,
                glyphPainter,
                needsAntiAliasing));
    }

    internal int OutputSpan(int y, int xMin, int xMax)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_targetPixels is null ||
            y < 0 ||
            xMin < 0 ||
            xMax <= xMin ||
            (uint) y >= _state.Height ||
            (uint) xMax > _state.Width)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        try
        {
            int destinationOffset = checked((y * _targetStride) + (xMin * checked((int) _state.BytesPerPixel)));
            int byteCount = checked((xMax - xMin) * checked((int) _state.BytesPerPixel));
            if (destinationOffset < 0 || destinationOffset > _targetPixels.Length - byteCount)
            {
                return Direct3D9Factory.InvalidArgumentHResult;
            }

            _scanPipeline.Run(_targetPixels, destinationOffset, checked((uint) (xMax - xMin)), xMin, y);
            return Direct3D9Factory.SuccessHResult;
        }
        catch (OverflowException)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }
        catch (InvalidOperationException)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }
    }

    internal void SetAntialiasedFiller(object? filler)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _scanPipeline.SetAntialiasedFiller(filler);
    }

    internal void ReleaseExpensiveResources()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ReleaseExpensiveResourcesCore();
    }

    internal int Resize(uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        _state = _state with { Width = width, Height = height };
        _software3DSurface?.Resize(width, height);
        return Direct3D9Factory.SuccessHResult;
    }

    internal int SetSurface(Direct3D9BindSoftwareRenderTarget bindTarget)
    {
        ArgumentNullException.ThrowIfNull(bindTarget);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        Cleanup(releaseSoftware3DSurface: false);
        UpdateResizeUniqueness();

        Direct3D9SoftwareRenderTargetState state = default;
        int result = bindTarget(out Direct3D9SoftwareRenderTargetBinding binding);
        if (result >= 0)
        {
            result = TryCreateState(binding, out state);
        }

        if (result >= 0)
        {
            result = _allocateIntermediateBuffers(state);
        }

        if (result >= 0)
        {
            result = _initializeBaseRenderTarget();
        }

        if (result >= 0 && _software3DSurface is not null)
        {
            result = _resizeSoftware3DSurface(_software3DSurface, state.Width, state.Height);
        }

        if (result < 0)
        {
            Cleanup(releaseSoftware3DSurface: true);
            return result;
        }

        _state = state;
        _deviceTransform = CreateDeviceTransform(state);
        return result;
    }

    internal int Clear(MilColorF? color, Direct3D9SurfaceRect? aliasedClip = null)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        if (!color.HasValue)
        {
            UnlockTarget();
            return Direct3D9Factory.SuccessHResult;
        }

        int result = LockTarget(out byte[] targetPixels, out int targetStride);
        if (result >= 0)
        {
            result = ValidateLockedTarget(targetPixels, targetStride, _state);
        }

        if (result >= 0)
        {
            try
            {
                Direct3D9SurfaceRect surfaceBounds = new(0, 0, checked((int) _state.Width), checked((int) _state.Height));
                if (TryIntersect(surfaceBounds, aliasedClip ?? surfaceBounds, out Direct3D9SurfaceRect clip))
                {
                    result = ClearLockedSurface(targetPixels, targetStride, color.GetValueOrDefault(), clip);
                }
            }
            catch (OverflowException)
            {
                result = Direct3D9Factory.InvalidArgumentHResult;
            }
        }

        UnlockTarget();
        return result;
    }

    internal int DrawBitmap(Direct3D9SurfaceRect aliasedClip)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        int result = Direct3D9Factory.SuccessHResult;
        try
        {
            if (!UpdateCurrentClip(aliasedClip, out Direct3D9SurfaceRect clip))
            {
                return result;
            }

            result = LockTarget(out byte[] targetPixels, out int targetStride);
            if (result >= 0)
            {
                result = ValidateLockedTarget(targetPixels, targetStride, _state);
            }

            if (result >= 0)
            {
                SetLockedTarget(targetPixels, targetStride);
                result = NormalizeNoRenderResult(
                    _drawBitmapSoftwareRenderTarget(targetPixels, targetStride, _state, clip));
            }
        }
        catch (OverflowException)
        {
            result = Direct3D9Factory.InvalidArgumentHResult;
        }
        finally
        {
            ReleaseExpensiveResourcesCore();
            UnlockTarget();
        }

        return result;
    }

    internal int DrawPath(Direct3D9SurfaceRect aliasedClip, bool hasFillBrush, bool hasPen, bool hasStrokeBrush)
    {
        return DrawPathInternal(aliasedClip, hasFillBrush, hasPen, hasStrokeBrush);
    }

    internal int DrawInfinitePath(Direct3D9SurfaceRect aliasedClip)
    {
        return DrawPathInternal(aliasedClip, hasFillBrush: true, hasPen: false, hasStrokeBrush: false);
    }

    private int DrawPathInternal(Direct3D9SurfaceRect aliasedClip, bool hasFillBrush, bool hasPen, bool hasStrokeBrush)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        int result = Direct3D9Factory.SuccessHResult;
        try
        {
            if (!UpdateCurrentClip(aliasedClip, out Direct3D9SurfaceRect clip))
            {
                return result;
            }

            result = LockTarget(out byte[] targetPixels, out int targetStride);
            if (result >= 0)
            {
                result = ValidateLockedTarget(targetPixels, targetStride, _state);
            }

            if (result >= 0)
            {
                SetLockedTarget(targetPixels, targetStride);
            }

            if (result >= 0 && hasFillBrush)
            {
                result = _fillPathSoftwareRenderTarget(targetPixels, targetStride, _state, clip);
            }

            if (result >= 0 && hasPen && hasStrokeBrush)
            {
                result = _widenPath();
                if (result >= 0)
                {
                    result = _strokePathSoftwareRenderTarget(targetPixels, targetStride, _state, clip);
                }
            }
        }
        catch (OverflowException)
        {
            result = Direct3D9Factory.InvalidArgumentHResult;
        }
        finally
        {
            ReleaseExpensiveResourcesCore();
            UnlockTarget();
        }

        return NormalizeNoRenderResult(result);
    }

    internal int DrawGlyphs(Direct3D9SurfaceRect aliasedClip)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        int result = Direct3D9Factory.SuccessHResult;
        try
        {
            if (!UpdateCurrentClip(aliasedClip, out Direct3D9SurfaceRect clip))
            {
                return result;
            }

            result = _ensureGlyphBrushRealization();
            if (result < 0)
            {
                return NormalizeNoRenderResult(result);
            }

            bool hasRealizedGlyphBrush = _hasRealizedGlyphBrush();
            float alphaScale = _getGlyphBrushOpacity();
            if (!hasRealizedGlyphBrush)
            {
                return result;
            }

            result = LockTarget(out byte[] targetPixels, out int targetStride);
            if (result >= 0)
            {
                result = ValidateLockedTarget(targetPixels, targetStride, _state);
            }

            if (result >= 0)
            {
                SetLockedTarget(targetPixels, targetStride);
                bool targetSupportsClearType = _forceClearType || !HasAlpha();
                result = _drawGlyphsSoftwareRenderTarget(
                    targetPixels,
                    targetStride,
                    _state,
                    clip,
                    alphaScale,
                    targetSupportsClearType);
            }
        }
        catch (OverflowException)
        {
            result = Direct3D9Factory.InvalidArgumentHResult;
        }
        finally
        {
            ReleaseExpensiveResourcesCore();
            UnlockTarget();
        }

        return NormalizeNoRenderResult(result);
    }

    internal int GetNumQueuedPresents(out uint queuedPresentCount)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        queuedPresentCount = 0;
        return Direct3D9Factory.SuccessHResult;
    }

    internal int DrawVideo(
        Direct3D9VideoRenderState renderState,
        Direct3D9SoftwareVideoSurfaceRenderer? surfaceRenderer,
        nint bitmapSource,
        Func<nint, int> drawBitmap)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(renderState);
        ArgumentNullException.ThrowIfNull(drawBitmap);
        if (_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        bool restorePrefilter = renderState.PrefilterEnabled;
        bool endRender = false;
        nint currentBitmapSource = 0;
        int result;
        try
        {
            if (surfaceRenderer is not null)
            {
                ArgumentNullException.ThrowIfNull(surfaceRenderer.BeginRender);
                ArgumentNullException.ThrowIfNull(surfaceRenderer.EndRender);
                result = surfaceRenderer.BeginRender(out currentBitmapSource);
                if (result < 0)
                {
                    return result;
                }

                endRender = true;
            }
            else
            {
                currentBitmapSource = bitmapSource;
                if (currentBitmapSource != 0)
                {
                    Direct3D9Factory.AddRef(currentBitmapSource);
                }
            }

            if (currentBitmapSource == 0)
            {
                return Direct3D9Factory.SuccessHResult;
            }

            renderState.PrefilterEnabled = false;
            return drawBitmap(currentBitmapSource);
        }
        finally
        {
            if (endRender)
            {
                _ = surfaceRenderer!.EndRender();
            }

            if (currentBitmapSource != 0)
            {
                Direct3D9Factory.Release(currentBitmapSource);
            }

            renderState.PrefilterEnabled = restorePrefilter;
        }
    }

    internal int Begin3D(Direct3D9SurfaceRect bounds, bool useZBuffer, float z)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        int result = LockTarget(out byte[] targetPixels, out int targetStride);
        if (result >= 0)
        {
            result = ValidateLockedTarget(targetPixels, targetStride, _state);
        }

        if (result < 0)
        {
            UnlockTarget();
            Cleanup3DResources();
            return result;
        }

        _targetPixels = targetPixels;
        _targetStride = targetStride;

        if (_software3DSurface is null)
        {
            result = _createSoftware3DSurface(out Direct3D9Software3DSurface? software3DSurface);
            if (result < 0)
            {
                if (software3DSurface is not null)
                {
                    _releaseSoftware3DSurface(software3DSurface);
                }

                UnlockTarget();
                Cleanup3DResources();
                if (IsUnavailableSoftwareRasterizer(result))
                {
                    _in3D = true;
                    return Direct3D9Factory.SuccessHResult;
                }

                return result;
            }

            if (software3DSurface is null)
            {
                UnlockTarget();
                Cleanup3DResources();
                return Direct3D9Factory.UnexpectedHResult;
            }

            _software3DSurface = software3DSurface;
        }

        Direct3D9SurfaceRect surfaceBounds = IntersectWithTarget(bounds);
        result = _software3DSurface.BeginSw3D(
            targetPixels,
            targetStride,
            surfaceBounds,
            useZBuffer,
            z);
        if (result < 0)
        {
            UnlockTarget();
        }
        else
        {
            _in3D = true;
        }

        Cleanup3DResources();
        return result;
    }

    internal int DrawMesh3D(Func<int> drawMesh3D, bool draw3DDisabled = false)
    {
        ArgumentNullException.ThrowIfNull(drawMesh3D);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        int result = draw3DDisabled || _software3DSurface is null
            ? Direct3D9Factory.SuccessHResult
            : _software3DSurface.DrawMesh3D(drawMesh3D);
        Cleanup3DResources();
        return result;
    }

    internal int End3D()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        int result = Direct3D9Factory.SuccessHResult;
        try
        {
            if (_software3DSurface is not null)
            {
                if (_targetPixels is null)
                {
                    result = Direct3D9Factory.WgxInvalidCallHResult;
                }
                else
                {
                    result = _software3DSurface.EndSw3D(_targetPixels, _targetStride);
                }
            }
        }
        finally
        {
            _in3D = false;
            UnlockTarget();
            Cleanup3DResources();
        }

        return result;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _scanPipeline.Dispose();
        _intermediateRenderTargetCreator.Dispose();
        if (_in3D || _targetPixels is not null)
        {
            _in3D = false;
            UnlockTarget();
        }

        Cleanup(releaseSoftware3DSurface: true);
        _intermediateBuffers.Dispose();
    }

    private void Cleanup(bool releaseSoftware3DSurface)
    {
        _cleanupSurfaceBinding();
        _freeIntermediateBuffers();
        if (releaseSoftware3DSurface)
        {
            ReleaseSoftware3DSurface();
        }
    }

    private int LockTarget(out byte[] targetPixels, out int targetStride)
    {
        targetPixels = null!;
        targetStride = 0;

        if (_stagedLockTarget is null)
        {
            return _lockTarget(out targetPixels, out targetStride);
        }

        int result = _stagedLockTarget.Lock();
        if (result < 0)
        {
            return result;
        }

        _stagedTargetLockAcquired = true;
        (result, targetStride) = _stagedLockTarget.GetStride();
        if (result >= 0)
        {
            (result, targetPixels) = _stagedLockTarget.GetDataPointer();
            if (result >= 0 && targetPixels is null)
            {
                result = Direct3D9Factory.UnexpectedHResult;
            }
        }

        if (result < 0)
        {
            UnlockTarget();
            targetPixels = null!;
            targetStride = 0;
        }

        return result;
    }

    private void SetLockedTarget(byte[] targetPixels, int targetStride)
    {
        _targetPixels = targetPixels;
        _targetStride = targetStride;
    }

    private void ReleaseExpensiveResourcesCore()
    {
        _scanPipeline.ReleaseExpensiveResources();
    }

    private void UnlockTarget()
    {
        _targetPixels = null;
        _targetStride = 0;
        if (_stagedLockTarget is null)
        {
            _unlockTarget();
        }
        else if (_stagedTargetLockAcquired)
        {
            _stagedTargetLockAcquired = false;
            _stagedLockTarget.Release();
        }
    }

    private void Cleanup3DResources()
    {
        if (_software3DSurface is not null)
        {
            _cleanup3DResources(_software3DSurface);
        }
    }

    private void ReleaseSoftware3DSurface()
    {
        Direct3D9Software3DSurface? software3DSurface = _software3DSurface;
        _software3DSurface = null;
        if (software3DSurface is not null)
        {
            _releaseSoftware3DSurface(software3DSurface);
        }
    }

    private int ClearLockedSurface(
        byte[] targetPixels,
        int targetStride,
        MilColorF color,
        Direct3D9SurfaceRect clip)
    {
        if (_state.PixelFormat is not (MilPixelFormat.Pbgra32Bpp or MilPixelFormat.Bgra32Bpp or MilPixelFormat.Bgr32Bpp))
        {
            return _clearSoftwareRenderTarget(targetPixels, targetStride, _state, color, clip);
        }

        bool premultiply = _state.PixelFormat == MilPixelFormat.Pbgra32Bpp;
        uint argb = ConvertToSrgb(color, premultiply);
        for (int y = clip.Top; y < clip.Bottom; y++)
        {
            int pixelOffset = checked((y * targetStride) + (clip.Left * 4));
            for (int x = clip.Left; x < clip.Right; x++)
            {
                targetPixels[pixelOffset] = (byte) argb;
                targetPixels[pixelOffset + 1] = (byte) (argb >> 8);
                targetPixels[pixelOffset + 2] = (byte) (argb >> 16);
                targetPixels[pixelOffset + 3] = (byte) (argb >> 24);
                pixelOffset += 4;
            }
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private Direct3D9SurfaceRect IntersectWithTarget(Direct3D9SurfaceRect bounds)
    {
        return new Direct3D9SurfaceRect(
            Math.Max(0, bounds.Left),
            Math.Max(0, bounds.Top),
            Math.Min(checked((int) _state.Width), bounds.Right),
            Math.Min(checked((int) _state.Height), bounds.Bottom));
    }

    private bool UpdateCurrentClip(
        Direct3D9SurfaceRect aliasedClip,
        out Direct3D9SurfaceRect currentClip)
    {
        Direct3D9SurfaceRect surfaceBounds = new(
            0,
            0,
            checked((int) _state.Width),
            checked((int) _state.Height));
        if (TryIntersect(surfaceBounds, aliasedClip, out currentClip))
        {
            _currentClip = currentClip;
            return true;
        }

        _currentClip = default;
        return false;
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
        if (right > left && bottom > top)
        {
            intersection = new Direct3D9SurfaceRect(left, top, right, bottom);
            return true;
        }

        intersection = default;
        return false;
    }

    private static int NormalizeNoRenderResult(int result)
    {
        return result is Direct3D9Factory.NonInvertibleMatrixHResult or Direct3D9Factory.BadNumberHResult
            ? Direct3D9Factory.SuccessHResult
            : result;
    }

    private static int GetBestBlendingFormat(MilPixelFormat pixelFormat, out MilPixelFormat blendingFormat)
    {
        if (pixelFormat is
            MilPixelFormat.Bgr32Bpp101010 or
            MilPixelFormat.Rgb48BppFixedPoint or
            MilPixelFormat.Bgr96BppFixedPoint or
            MilPixelFormat.Rgb128BppFloat or
            MilPixelFormat.Rgba128BppFloat or
            MilPixelFormat.Prgba128BppFloat or
            MilPixelFormat.Gray16BppFixedPoint or
            MilPixelFormat.Gray32BppFloat)
        {
            blendingFormat = MilPixelFormat.Prgba128BppFloat;
            return Direct3D9Factory.SuccessHResult;
        }

        if (pixelFormat is >= MilPixelFormat.Indexed1Bpp and <= MilPixelFormat.Prgba64Bpp)
        {
            blendingFormat = MilPixelFormat.Pbgra32Bpp;
            return Direct3D9Factory.SuccessHResult;
        }

        blendingFormat = MilPixelFormat.Undefined;
        return Direct3D9Factory.WinCodecInternalErrorHResult;
    }

    private static int WrapRenderTargetBitmap(
        nint internalSurface,
        Direct3D9SoftwareRenderTargetBitmapCreationRequest request,
        out Direct3D9SoftwareRenderTargetBitmap? renderTargetBitmap)
    {
        try
        {
            renderTargetBitmap = new Direct3D9SoftwareRenderTargetBitmap(internalSurface, request);
            return Direct3D9Factory.SuccessHResult;
        }
        catch (OutOfMemoryException)
        {
            renderTargetBitmap = null;
            return Direct3D9Factory.OutOfMemoryHResult;
        }
    }

    private static uint ConvertToSrgb(MilColorF color, bool premultiply)
    {
        byte alpha = RoundToByte(color.Alpha);
        byte red = ConvertScRgbChannelToSrgbByte(color.Red);
        byte green = ConvertScRgbChannelToSrgbByte(color.Green);
        byte blue = ConvertScRgbChannelToSrgbByte(color.Blue);
        if (premultiply)
        {
            float clampedAlpha = Math.Clamp(color.Alpha, 0f, 1f);
            red = RoundByteValue(red * clampedAlpha);
            green = RoundByteValue(green * clampedAlpha);
            blue = RoundByteValue(blue * clampedAlpha);
        }

        return (uint) (alpha << 24 | red << 16 | green << 8 | blue);
    }

    private static byte ConvertScRgbChannelToSrgbByte(float value)
    {
        if (!(value > 0f))
        {
            return 0;
        }

        if (value >= 1f)
        {
            return byte.MaxValue;
        }

        const float lookupScale = 3354f;
        float lookupValue = MathF.Floor((lookupScale * value) + 0.5f) / lookupScale;
        float srgb = lookupValue <= 0.0031308f
            ? lookupValue * 12.92f
            : (1.055f * MathF.Pow(lookupValue, 1f / 2.4f)) - 0.055f;
        return RoundToByte(srgb);
    }

    private static byte RoundToByte(float value) => RoundByteValue(Math.Clamp(value, 0f, 1f) * byte.MaxValue);

    private static byte RoundByteValue(float value) => (byte) Math.Clamp(MathF.Floor(value + 0.5f), 0f, byte.MaxValue);

    private static int TryCreateState(
        Direct3D9SoftwareRenderTargetBinding binding,
        out Direct3D9SoftwareRenderTargetState state)
    {
        state = default;
        int result = GetColorDataPixelFormat(binding.PixelFormat, out MilPixelFormat colorDataPixelFormat);
        if (result < 0)
        {
            return result;
        }

        float dpiX = (float) binding.DpiX;
        float dpiY = (float) binding.DpiY;
        if (!(binding.DpiX > 0) ||
            !(binding.DpiY > 0) ||
            !double.IsFinite(binding.DpiX) ||
            !double.IsFinite(binding.DpiY) ||
            !float.IsFinite(dpiX) ||
            !float.IsFinite(dpiY))
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        byte bitsPerPixel = MilPixelFormatInfo.GetBitsPerPixel(binding.PixelFormat);
        if (bitsPerPixel <= 8 || binding.PixelFormat is
            MilPixelFormat.Indexed1Bpp or
            MilPixelFormat.Indexed2Bpp or
            MilPixelFormat.Indexed4Bpp or
            MilPixelFormat.Indexed8Bpp)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        state = new Direct3D9SoftwareRenderTargetState(
            binding.Width,
            binding.Height,
            binding.PixelFormat,
            colorDataPixelFormat,
            (uint) bitsPerPixel >> 3,
            dpiX,
            dpiY);
        return Direct3D9Factory.SuccessHResult;
    }

    private static Matrix3x2 CreateDeviceTransform(Direct3D9SoftwareRenderTargetState state) =>
        Matrix3x2.CreateScale(state.DpiX, state.DpiY);

    private void UpdateResizeUniqueness()
    {
        _resizeUniqueness++;
        if (_resizeUniqueness == 0)
        {
            _resizeUniqueness++;
        }
    }

    private static int GetColorDataPixelFormat(
        MilPixelFormat pixelFormat,
        out MilPixelFormat colorDataPixelFormat)
    {
        if (pixelFormat is
            MilPixelFormat.Bgr32Bpp101010 or
            MilPixelFormat.Rgb48BppFixedPoint or
            MilPixelFormat.Bgr96BppFixedPoint or
            MilPixelFormat.Rgb128BppFloat or
            MilPixelFormat.Rgba128BppFloat or
            MilPixelFormat.Prgba128BppFloat or
            MilPixelFormat.Gray16BppFixedPoint or
            MilPixelFormat.Gray32BppFloat)
        {
            colorDataPixelFormat = MilPixelFormat.Prgba128BppFloat;
            return Direct3D9Factory.SuccessHResult;
        }

        if (pixelFormat is >= MilPixelFormat.Indexed1Bpp and <= MilPixelFormat.Prgba64Bpp)
        {
            colorDataPixelFormat = MilPixelFormat.Pbgra32Bpp;
            return Direct3D9Factory.SuccessHResult;
        }

        colorDataPixelFormat = MilPixelFormat.Undefined;
        return Direct3D9Factory.WinCodecInternalErrorHResult;
    }

    private static int ValidateLockedTarget(
        byte[] targetPixels,
        int targetStride,
        Direct3D9SoftwareRenderTargetState state)
    {
        if (targetPixels is null)
        {
            return Direct3D9Factory.UnexpectedHResult;
        }

        try
        {
            int rowBytes = checked((int) state.Width * (int) state.BytesPerPixel);
            int height = checked((int) state.Height);
            int requiredSize = height == 0
                ? 0
                : checked((height - 1) * targetStride + rowBytes);
            if (targetStride < rowBytes || targetPixels.Length < requiredSize)
            {
                return Direct3D9Factory.InvalidArgumentHResult;
            }
        }
        catch (OverflowException)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private static bool IsUnavailableSoftwareRasterizer(int result)
    {
        return result is Direct3D9Factory.NotAvailableHResult or Direct3D9Factory.NotFoundHResult;
    }
}
