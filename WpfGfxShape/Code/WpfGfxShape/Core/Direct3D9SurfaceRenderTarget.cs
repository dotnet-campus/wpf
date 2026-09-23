using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal readonly record struct MilColorF(float Alpha, float Red, float Green, float Blue);

internal readonly record struct MilRectF(float Left, float Top, float Right, float Bottom);

internal enum MilAntiAliasMode
{
    None,
    EightByEight
}

internal enum MilCompositingMode
{
    SourceOver,
    SourceCopy,
    SourceAdd,
    SourceAlphaMultiply,
    SourceInverseAlphaMultiply,
    SourceUnder,
    SourceOverNonPremultiplied,
    SourceInverseAlphaOverNonPremultiplied,
    DestInvert,
    Last
}

internal enum InternalRenderTargetType : uint
{
    SoftwareRaster = 0x00000400,
    HardwareRaster = 0x00000800
}

[Flags]
internal enum MilTransparencyFlags
{
    Opaque = 0,
    ConstantAlpha = 1,
    PerPixelAlpha = 2,
    ColorKey = 4
}

internal readonly record struct Direct3D9WindowPosition(int X, int Y);

internal readonly record struct Direct3D9LayeredWindowProperties(
    uint UpdateFlags,
    byte SourceConstantAlpha,
    byte AlphaFormat,
    uint ColorKey);

internal readonly record struct Direct3D9Begin3DResult(int HResult, MultisampleType MultisampleTypeReceived);

internal sealed class Direct3D9VideoRenderState
{
    internal bool PrefilterEnabled { get; set; }
}

internal sealed record Direct3D9VideoSurfaceRenderer(
    Direct3D9BeginVideoRender BeginRender,
    Func<int> EndRender);

internal readonly record struct Direct3D9GlyphDrawState(
    bool TargetSupportsClearType,
    bool CanDrawText,
    bool RealizedBrushMayNeedNonPowerOfTwoTiling = false,
    bool RealizedBrushWillHaveSourceClip = false,
    bool RealizedBrushSourceClipMayBeEntireSource = true);

internal readonly record struct Direct3D9RealizedGlyphBrushState(
    bool HasBrush,
    bool IsBitmapBrush = false,
    bool HasSourceClip = false,
    bool SourceClipIsEntireSource = true);

internal delegate int Direct3D9EnsureGlyphBrushRealization(out Direct3D9RealizedGlyphBrushState realizedBrushState);

internal readonly record struct Direct3D9RealizedSoftwareGlyphBrushState(
    bool HasBrush,
    float EffectAlpha = 1);

internal delegate int Direct3D9EnsureSoftwareGlyphBrushRealization(out Direct3D9RealizedSoftwareGlyphBrushState realizedBrushState);

internal sealed record Direct3D9SoftwareGlyphRenderer(
    Direct3D9EnsureSoftwareGlyphBrushRealization EnsureBrushRealization,
    Func<int, int> GetSoftwareFallback,
    Func<bool, float, int> DrawGlyphs);

[Flags]
internal enum Direct3D9IntermediateRenderTargetUsageFlags
{
    None = 0,
    ForBlending = 1,
    ForUseIn3D = 2
}

internal readonly record struct Direct3D9IntermediateRenderTargetUsage(
    Direct3D9IntermediateRenderTargetUsageFlags Flags,
    MilBitmapWrapMode WrapMode);

internal delegate int Direct3D9CreateIntermediateRenderTarget(bool forBlending, out nint renderTargetBitmap);
internal delegate int Direct3D9CreateHardwareIntermediateRenderTarget(
    uint width,
    uint height,
    Direct3D9Device device,
    uint? associatedDisplayIndex,
    bool forBlending,
    out Direct3D9TextureRenderTarget? renderTargetBitmap);

internal readonly record struct Direct3D9LayerBeginState(
    Direct3D9SurfaceRect LayerBounds,
    bool HasAlphaMaskBrush);

internal delegate bool Direct3D9GetPartialLayerCaptureRects(out IReadOnlyList<Direct3D9SurfaceRect> copyRects);

internal delegate int Direct3D9CaptureLayerTarget(
    Direct3D9SurfaceRect layerBounds,
    IReadOnlyList<Direct3D9SurfaceRect>? copyRects,
    out nint sourceBitmap);

internal readonly record struct Direct3D9LayerEndState(
    Direct3D9SurfaceRect LayerBounds,
    Direct3D9SurfaceRect CurrentClip,
    nint SourceBitmap,
    Direct3D9SurfaceRect PreviousBounds = default,
    bool SavedClearTypeHint = false);

internal delegate int Direct3D9CompositeSavedLayer(
    nint sourceBitmap,
    Direct3D9SurfaceRect layerBounds,
    MilCompositingMode compositingMode);

internal readonly record struct Direct3D9LayerState(
    Direct3D9SurfaceRect LayerBounds,
    Direct3D9SurfaceRect CurrentClip,
    float Alpha = 1,
    MilAntiAliasMode AntiAliasMode = MilAntiAliasMode.None,
    nint GeometricMask = 0,
    nint AlphaMaskBrush = 0);

internal readonly record struct Direct3D9LayerCompositeState(
    nint SourceBitmap,
    Direct3D9SurfaceRect LayerBounds,
    float Alpha,
    MilAntiAliasMode AntiAliasMode,
    nint GeometricMask,
    nint AlphaMaskBrush,
    bool TargetHasAlpha,
    bool RequiresSourceUnder);

internal delegate int Direct3D9RetainLayerResource(nint resource, out nint retainedResource);

internal delegate int Direct3D9CreateLayerMaskShape(nint geometricMask, out Direct3D9LayerMaskShape? shape);

internal delegate int Direct3D9CreateLayerGeometryGenerator(
    Direct3D9LayerMaskShape? shape,
    Direct3D9SurfaceRect layerBounds,
    out Direct3D9PathGeometryGenerator? geometryGenerator);

internal delegate int Direct3D9CombineLayerMaskWithBounds(
    Direct3D9SurfaceRect layerBounds,
    Direct3D9LayerMaskShape geometricMask,
    out Direct3D9LayerMaskShape? complementedShape);

internal delegate int Direct3D9CreateLayerBrush(nint sourceBitmap, bool useOpaqueBlack, out Direct3D9PathHardwareBrush? brush);

internal delegate int Direct3D9CreateLayerAlphaScaleEffect(float alpha, out Direct3D9LayerEffectList? effectList);

internal delegate int Direct3D9FillLayerPath(
    MilCompositingMode compositingMode,
    Direct3D9PathGeometryGenerator geometryGenerator,
    Direct3D9PathHardwareBrush hardwareBrush,
    nint effects,
    Direct3D9PathBrushContext brushContext,
    Direct3D9SurfaceRect? complementBounds,
    bool needInside);

internal delegate int Direct3D9ExecuteEffectLayerPath(
    MilCompositingMode compositingMode,
    Direct3D9PathGeometryGenerator geometryGenerator,
    Direct3D9PathHardwareBrush hardwareBrush,
    Direct3D9EffectList? effects,
    Direct3D9PathBrushContext brushContext,
    Direct3D9SurfaceRect? complementBounds,
    bool needInside);

internal sealed record Direct3D9LayerMaskOperations(
    Direct3D9CreateLayerMaskShape CreateMaskShape,
    Direct3D9CreateLayerGeometryGenerator CreateAntialiasedGeometryGenerator,
    Direct3D9CombineLayerMaskWithBounds CombineMaskWithBounds,
    Direct3D9CreateLayerGeometryGenerator CreateAliasedGeometryGenerator,
    Direct3D9CreateLayerGeometryGenerator CreateBoundsGeometryGenerator,
    Direct3D9CreateLayerBrush CreateBrush,
    Direct3D9CreateLayerAlphaScaleEffect CreateAlphaScaleEffect,
    Direct3D9FillLayerPath FillPath,
    Direct3D9RetainEffectResource? RetainAlphaMaskResource = null,
    Action<nint>? ReleaseAlphaMaskResource = null,
    Direct3D9ExecuteEffectLayerPath? FillEffectPath = null);

internal sealed record Direct3D9LayerOperations(
    Direct3D9GetPartialLayerCaptureRects GetPartialCaptureRects,
    Direct3D9CaptureLayerTarget CaptureTarget,
    Func<Direct3D9SurfaceRect, int> ClearTargetToTransparent,
    Direct3D9RetainLayerResource RetainGeometricMask,
    Direct3D9RetainLayerResource RetainAlphaMaskBrush,
    Func<Direct3D9SurfaceRect, int> RestoreParentTargetState,
    Func<Direct3D9LayerCompositeState, int> Composite,
    Action<nint> ReleaseSourceBitmap,
    Action<nint> ReleaseGeometricMask,
    Action<nint> ReleaseAlphaMaskBrush,
    Direct3D9LayerMaskOperations? MaskOperations = null);

internal readonly record struct Direct3D9EffectComposeState(
    nint ScaleTransform,
    nint Effect,
    uint IntermediateWidth,
    uint IntermediateHeight,
    nint ImplicitInput);

internal delegate int Direct3D9ResolveImplicitEffectInput(
    nint implicitInput,
    out nint textureRenderTarget);

internal delegate int Direct3D9ApplyEffect(
    nint effect,
    nint scaleTransform,
    uint intermediateWidth,
    uint intermediateHeight,
    nint textureRenderTarget);

internal readonly record struct Direct3D9BitmapDrawState(
    Matrix4x4 WorldToDevice,
    Direct3D9PointAndSizeRect? SourceRect = null);

internal delegate int Direct3D9GetScratchBitmapBrush(out nint scratchBrush);

internal delegate int Direct3D9FillBitmapPath(
    nint scratchBrush,
    nint bitmapSource,
    nint effects,
    MilRectF sourceRect,
    Matrix4x4 shapeToDevice,
    Matrix4x4 baseSamplingToDevice);

internal delegate void Direct3D9SetScratchBitmapBrush(
    nint scratchBrush,
    nint bitmapSource,
    Matrix4x4 bitmapToSamplingSpace);

internal delegate int Direct3D9CreateBitmapShape(MilRectF sourceRect, out nint shape);

internal readonly record struct Direct3D9SafeClippedShape(
    nint Shape,
    Matrix4x4? ShapeToDevice,
    MilRectF Bounds,
    bool WasClipped);

internal delegate int Direct3D9ClipToSafeDeviceBounds(
    nint shape,
    Matrix4x4? shapeToDevice,
    MilRectF bounds,
    out Direct3D9SafeClippedShape clippedShape);

internal delegate int Direct3D9GetRealizedBrush(
    bool convertNullToTransparent,
    out nint brush,
    out nint effects);

internal delegate int Direct3D9FillPathWithBrush(
    nint shape,
    Matrix4x4? shapeToDevice,
    MilRectF bounds,
    nint brush,
    Matrix4x4 worldToDevice,
    nint effects);

internal delegate int Direct3D9SoftwareFillPath(
    nint shape,
    Matrix4x4? shapeToDevice,
    nint brushRealizer,
    int reasonForFallback);

internal delegate int Direct3D9SoftwareFillBitmapPath(
    nint shape,
    Matrix4x4? shapeToDevice,
    Direct3D9ImmediateBrushRealizer brushRealizer,
    int reasonForFallback);

internal delegate int Direct3D9EnsurePathBrushRealization(nint brushRealizer);

internal delegate int Direct3D9CreateInfinitePathShape(MilRectF renderTargetBounds, out nint shape);

internal delegate int Direct3D9GetPathShapeBounds(nint shape, out MilRectF bounds);

internal delegate int Direct3D9WidenPathShape(
    nint shape,
    nint pen,
    Matrix4x4? shapeToDevice,
    Direct3D9SurfaceRect renderTargetBounds,
    out nint widenedShape);

internal delegate int Direct3D9GetPathRealizedBrush(
    nint brushRealizer,
    bool convertNullToTransparent,
    out nint brush,
    out nint effects);

internal readonly record struct Direct3D9PathClipperState(
    nint Shape,
    Matrix4x4? ShapeToDevice,
    MilRectF Bounds);

internal delegate int Direct3D9ApplyPathGuidelines(
    Direct3D9PathClipperState clipperState,
    out Direct3D9PathClipperState updatedClipperState);

internal delegate int Direct3D9ApplyPathBrushClip(
    Direct3D9PathClipperState clipperState,
    nint brush,
    Matrix4x4 worldToDevice,
    out Direct3D9PathClipperState updatedClipperState);

internal delegate int Direct3D9GetPathBoundsInDeviceSpace(
    Direct3D9PathClipperState clipperState,
    out MilRectF bounds);

internal sealed record Direct3D9PathBrushContext(
    Matrix4x4 WorldToDevice,
    Direct3D9SurfaceRect RenderingBounds,
    MilRectF SamplingBounds,
    bool CanFallback);

internal delegate int Direct3D9DeriveHardwareBrush(
    nint brush,
    Direct3D9PathBrushContext brushContext,
    out nint hardwareBrush);

internal delegate int Direct3D9CreatePathGeometryGenerator(
    Direct3D9PathClipperState clipperState,
    out nint geometryGenerator);

internal delegate int Direct3D9DrawShaderPathGeometry(
    nint shader,
    nint geometryGenerator,
    Direct3D9SurfaceRect renderingBounds,
    bool useZBuffer);

internal delegate int Direct3D9AcceleratedFillPath(
    nint geometryGenerator,
    nint hardwareBrush,
    nint effects,
    Direct3D9PathBrushContext brushContext);

internal delegate int Direct3D9InitializeShaderPipeline(
    MilCompositingMode compositingMode,
    nint geometryGenerator,
    nint hardwareBrush,
    nint effects,
    Direct3D9PathBrushContext effectContext,
    Direct3D9SurfaceRect? outsideBounds,
    bool needInside);

internal sealed record Direct3D9ShaderPipeline(
    Direct3D9InitializeShaderPipeline InitializeForRendering,
    Func<int> Execute,
    Action ReleaseExpensiveResources);

internal delegate Direct3D9ShaderPipeline Direct3D9CreateShaderPipeline(
    bool is2D,
    Direct3D9Device device);

internal readonly record struct Direct3D9ProjectedMeshState(
    Matrix4x4 BaseSamplingToIdealSampling,
    Direct3D9SurfaceRect RenderBoundsDeviceSpace,
    MilRectF BrushSamplingBounds,
    bool IsVisible);

internal delegate int Direct3D9ApplyProjectedMeshTo2DState(
    Direct3D9ContextState contextState,
    Direct3D9SurfaceRect currentClip,
    out Direct3D9ProjectedMeshState projectedMeshState);

internal delegate int Direct3D9DeriveMeshShader(
    Direct3D9ProjectedMeshState projectedMeshState,
    out Direct3D9DerivedMeshShader? shader);

internal delegate int Direct3D9GetMeshBounds(out Direct3D9Box meshBounds);

internal sealed class Direct3D9DerivedMeshShader : IDisposable
{
    private readonly Action _release;
    private bool _isDisposed;

    internal Direct3D9DerivedMeshShader(
        Func<int> begin,
        Func<bool> canRunShaderPath,
        Func<int> shaderDrawMesh3D,
        Func<int> fixedFunctionDrawMesh3D,
        Func<int> finish,
        Action release)
    {
        ArgumentNullException.ThrowIfNull(begin);
        ArgumentNullException.ThrowIfNull(canRunShaderPath);
        ArgumentNullException.ThrowIfNull(shaderDrawMesh3D);
        ArgumentNullException.ThrowIfNull(fixedFunctionDrawMesh3D);
        ArgumentNullException.ThrowIfNull(finish);
        ArgumentNullException.ThrowIfNull(release);

        Begin = begin;
        CanRunShaderPath = canRunShaderPath;
        ShaderDrawMesh3D = shaderDrawMesh3D;
        FixedFunctionDrawMesh3D = fixedFunctionDrawMesh3D;
        Finish = finish;
        _release = release;
    }

    internal Func<int> Begin { get; }

    internal Func<bool> CanRunShaderPath { get; }

    internal Func<int> ShaderDrawMesh3D { get; }

    internal Func<int> FixedFunctionDrawMesh3D { get; }

    internal Func<int> Finish { get; }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _release();
        _isDisposed = true;
    }
}

internal readonly record struct Direct3D9ContextState(
    Matrix4x4 WorldTransform,
    Matrix4x4 ViewTransform,
    Matrix4x4 ProjectionTransform,
    Matrix4x4 ViewportProjectionModifier,
    Cull CullMode,
    Cmpfunc DepthBufferFunction,
    bool IsAntialiasingEnabled,
    bool In3D = false,
    Direct3D9SurfaceRect? AliasedClip = null);

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9SurfaceRenderTarget : IDisposable
{
    private const uint UpdateLayeredWindowColorKey = 0x00000001;
    private const uint UpdateLayeredWindowAlpha = 0x00000002;
    private const uint UpdateLayeredWindowOpaque = 0x00000004;
    private const uint UpdateLayeredWindowNoResize = 0x00000008;
    private const byte SourceAlpha = 0x01;

    private readonly Direct3D9Device _device;
    private readonly Direct3D9RenderTargetInitializationFlags _initializationFlags;
    private bool _forceClearType;
    private readonly MultisampleType _multisampleType;
    private readonly MultisampleType _multisampleTypeFor3D;
    private readonly IDirect3DSurface9* _depthStencilSurface;
    private readonly bool _isDepthBufferEnabled;
    private Direct3D9Surface? _renderTargetSurface;
    private readonly Direct3D9Surface? _renderTargetSurfaceFor3D;
    private readonly Action? _resetPerPrimitiveResourceUsage;
    private readonly MilPixelFormat _pixelFormat;
    private readonly Format _targetSurfaceFormat;
    private readonly Matrix3x2 _deviceTransform;
    private readonly uint? _associatedDisplayIndex;
    private uint _width;
    private uint _height;
    private readonly Func<Direct3D9SurfaceRect, float, bool, MultisampleType, Direct3D9Begin3DResult>? _begin3DInternal;
    private readonly Action? _setMultisampleFailed;
    private readonly Func<bool> _isDraw3DDisabled;
    private Direct3D9SurfaceRect _bounds;
    private Direct3D9SurfaceRect _boundsPre3D;
    private Direct3D9Surface? _createdDepthStencilSurface;
    private bool _isActiveDepthBufferEnabled;
    private Direct3D9Surface? _intermediateMultisampleSurface;
    private Direct3D9Surface? _activeRenderTargetSurfaceFor3D;
    private Direct3D9SwapChain? _swapChain;
    private PresentParameters _presentParameters;
    private bool _isRenderingEnabled = true;
    private bool _hasValidContents;
    private bool _ownsRenderTargetSurface;
    private int _displayInvalidHResult;
    private readonly List<Direct3D9SurfaceRect> _invalidatedRects = [];
    private Direct3D9WindowPosition _windowPosition;
    private uint _updateLayeredWindowFlags = UpdateLayeredWindowOpaque;
    private byte _sourceConstantAlpha = byte.MaxValue;
    private byte _alphaFormat;
    private uint _colorKey;
    private readonly uint _presentFlags;
    private readonly int _initializationHResult;
    private bool _hasEmptyInvalidation;
    private bool _in3D;
    private bool _wasUsedToCreateHardwareRenderTarget;
    private readonly List<Direct3D9LayerFrame> _layerStack = [];
    private bool _isDisposed;

    internal Direct3D9SurfaceRenderTarget(
        Direct3D9Device device,
        MultisampleType multisampleType,
        MultisampleType multisampleTypeFor3D = MultisampleType.MultisampleNone,
        IDirect3DSurface9* depthStencilSurface = null,
        bool isDepthBufferEnabled = false,
        Direct3D9Surface? renderTargetSurface = null,
        Direct3D9Surface? renderTargetSurfaceFor3D = null,
        Action? resetPerPrimitiveResourceUsage = null,
        MilPixelFormat pixelFormat = MilPixelFormat.Undefined,
        uint width = 0,
        uint height = 0,
        Func<Direct3D9SurfaceRect, float, bool, MultisampleType, Direct3D9Begin3DResult>? begin3DInternal = null,
        Action? setMultisampleFailed = null,
        Direct3D9SurfaceRect? initialBounds = null,
        uint? associatedDisplayIndex = null,
        Swapeffect swapEffect = Swapeffect.Discard,
        Direct3D9RenderTargetInitializationFlags initializationFlags = Direct3D9RenderTargetInitializationFlags.None,
        bool forceClearType = false,
        PresentParameters? presentParameters = null,
        Func<bool>? isDraw3DDisabled = null)
    {
        ArgumentNullException.ThrowIfNull(device);
        if (isDepthBufferEnabled && depthStencilSurface is null)
        {
            throw new ArgumentException("A depth-stencil surface is required when the depth buffer is enabled.", nameof(depthStencilSurface));
        }

        _device = device;
        _initializationFlags = initializationFlags;
        _forceClearType = forceClearType;
        _multisampleType = multisampleType;
        _multisampleTypeFor3D = multisampleTypeFor3D;
        _depthStencilSurface = depthStencilSurface;
        _isDepthBufferEnabled = isDepthBufferEnabled;
        _renderTargetSurface = renderTargetSurface;
        _ownsRenderTargetSurface = renderTargetSurface is not null;
        _renderTargetSurfaceFor3D = renderTargetSurfaceFor3D;
        _resetPerPrimitiveResourceUsage = resetPerPrimitiveResourceUsage;
        _pixelFormat = pixelFormat;
        _deviceTransform = Direct3D9PrimaryDisplayDpi.GetDeviceTransform();
        _associatedDisplayIndex = associatedDisplayIndex;
        _width = width;
        _height = height;
        Format backBufferFormat = presentParameters?.BackBufferFormat ?? ToDirect3DFormat(pixelFormat);
        _targetSurfaceFormat = backBufferFormat;
        _presentParameters = presentParameters ?? new PresentParameters(
            backBufferWidth: width,
            backBufferHeight: height,
            backBufferFormat: backBufferFormat,
            backBufferCount: 1,
            swapEffect: swapEffect,
            windowed: true,
            enableAutoDepthStencil: false,
            autoDepthStencilFormat: Format.Unknown);
        _initializationHResult = GetPresentFlags(
            backBufferFormat,
            device.DisplayMode.Format,
            device.Capabilities,
            out _presentFlags);
        if (_initializationHResult < 0)
        {
            _displayInvalidHResult = _initializationHResult;
            _isRenderingEnabled = false;
        }
        _begin3DInternal = begin3DInternal;
        _setMultisampleFailed = setMultisampleFailed;
        _isDraw3DDisabled = isDraw3DDisabled ?? (static () => false);
        _bounds = initialBounds ?? new Direct3D9SurfaceRect(0, 0, checked((int) width), checked((int) height));
    }

    internal static int TryCreateDisplayRenderTarget(
        Direct3D9DeviceManager deviceManager,
        Direct3D9DeviceRequest request,
        Caps9 capabilities,
        Func<Direct3D9TargetFormatTestStatus, int> testRenderTargetFormat,
        out Direct3D9SurfaceRenderTarget? renderTarget)
    {
        return TryCreateDisplayRenderTarget(
            deviceManager,
            request,
            capabilities,
            testRenderTargetFormat,
            static (deviceParameters, request) => new Direct3D9SurfaceRenderTarget(
                deviceParameters.Device,
                MultisampleType.MultisampleNone,
                pixelFormat: ToMilPixelFormat(deviceParameters.PresentParameters.BackBufferFormat),
                associatedDisplayIndex: request.DisplayAdapterOrdinal,
                initializationFlags: request.InitializationFlags,
                presentParameters: deviceParameters.PresentParameters),
            out renderTarget);
    }

    internal static int TryCreateDisplayRenderTarget(
        Direct3D9DeviceManager deviceManager,
        Direct3D9DeviceRequest request,
        Caps9 capabilities,
        Func<Direct3D9TargetFormatTestStatus, int> testRenderTargetFormat,
        Func<Direct3D9DeviceAndPresentParameters, Direct3D9DeviceRequest, Direct3D9SurfaceRenderTarget> createRenderTarget,
        out Direct3D9SurfaceRenderTarget? renderTarget)
    {
        ArgumentNullException.ThrowIfNull(deviceManager);
        ArgumentNullException.ThrowIfNull(testRenderTargetFormat);
        ArgumentNullException.ThrowIfNull(createRenderTarget);

        renderTarget = null;
        Direct3D9SurfaceRenderTarget? candidate = null;
        try
        {
            Direct3D9DeviceAndPresentParameters deviceParameters =
                deviceManager.GetDeviceAndPresentParameters(request, capabilities);
            int result = deviceParameters.Device.CheckRenderTargetFormat(
                deviceParameters.PresentParameters.BackBufferFormat,
                testRenderTargetFormat,
                out int? getDeviceContextHResult);
            if (result < 0)
            {
                return result;
            }

            if ((request.InitializationFlags & Direct3D9RenderTargetInitializationFlags.PresentUsingMask)
                    != Direct3D9RenderTargetInitializationFlags.PresentUsingHal
                && getDeviceContextHResult is < 0)
            {
                return Direct3D9Factory.NoHardwareDeviceHResult;
            }

            PresentParameters presentParameters = deviceParameters.PresentParameters;
            presentParameters.BackBufferWidth = 0;
            presentParameters.BackBufferHeight = 0;
            deviceParameters = deviceParameters with { PresentParameters = presentParameters };
            candidate = createRenderTarget(deviceParameters, request);
            result = candidate.Resize(0, 0);
            if (result < 0)
            {
                candidate.Dispose();
                candidate = null;
                return result;
            }

            result = TransferInitializedDisplayRenderTarget(candidate, out renderTarget);
            candidate = null;
            return result;
        }
        catch (COMException exception)
        {
            candidate?.Dispose();
            renderTarget = null;
            return exception.HResult;
        }
        catch (OutOfMemoryException)
        {
            candidate?.Dispose();
            renderTarget = null;
            return Direct3D9Factory.OutOfMemoryHResult;
        }
    }

    internal static int TransferInitializedDisplayRenderTarget(
        Direct3D9SurfaceRenderTarget candidate,
        out Direct3D9SurfaceRenderTarget? renderTarget)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        if (candidate._initializationHResult < 0)
        {
            int initializationHResult = candidate._initializationHResult;
            candidate.Dispose();
            renderTarget = null;
            return initializationHResult;
        }

        renderTarget = candidate;
        return 0;
    }

    internal InternalRenderTargetType GetRenderTargetType()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return InternalRenderTargetType.HardwareRaster;
    }

    internal MilPixelFormat GetPixelFormat()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _pixelFormat;
    }

    internal int SetClearTypeHint(bool forceClearType)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _forceClearType = forceClearType;
        return Direct3D9Factory.SuccessHResult;
    }

    internal (uint Width, uint Height) GetSize()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return (_width, _height);
    }

    internal MilRectF GetBounds()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return new MilRectF(0, 0, _width, _height);
    }

    internal Matrix3x2 GetDeviceTransform()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _deviceTransform;
    }

    internal Format GetDirect3DTextureFormat()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _targetSurfaceFormat;
    }

    internal bool CanUseShaderPipeline()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _device.PixelShaderVersion >= 0xFFFF0200
            && _device.VertexShaderVersion >= 0xFFFE0200;
    }

    internal uint GetRealizationCacheIndex()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _device.RealizationCacheIndex;
    }

    internal uint? GetDisplayId()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _associatedDisplayIndex;
    }

    internal int ReadEnabledDisplays(Span<bool> enabledDisplays)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_associatedDisplayIndex is uint displayIndex && displayIndex >= (uint) enabledDisplays.Length)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        enabledDisplays.Clear();
        if (_associatedDisplayIndex is uint associatedDisplayIndex)
        {
            enabledDisplays[(int) associatedDisplayIndex] = true;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal Direct3D9SurfaceRect Bounds => _bounds;

    internal int LayerCount => _layerStack.Count;

    internal bool ForceClearType => _forceClearType;

    internal bool In3D => _in3D;

    internal bool IsRenderingEnabled => _isRenderingEnabled;

    internal bool HasValidContents => _hasValidContents;

    internal int DisplayInvalidHResult => _displayInvalidHResult;

    internal Direct3D9WindowPosition WindowPosition => _windowPosition;

    internal Direct3D9LayeredWindowProperties LayeredWindowProperties => new(
        _updateLayeredWindowFlags | UpdateLayeredWindowNoResize,
        _sourceConstantAlpha,
        _alphaFormat,
        _colorKey);

    internal bool IsValid => !_isDisposed
        && _isRenderingEnabled
        && (_swapChain is not null
            ? _swapChain.IsValid
            : _renderTargetSurface is { IsValid: true });

    internal int GetNumQueuedPresents(out uint queuedPresentCount)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (IsValid)
        {
            using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
            return _device.GetNumQueuedPresents(out queuedPresentCount);
        }

        queuedPresentCount = 0;
        return 0;
    }

    internal int WaitForVBlank()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_displayInvalidHResult < 0)
        {
            return Direct3D9Factory.NoHardwareDeviceHResult;
        }

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        return _device.WaitForVBlank(0);
    }

    internal void AdvanceFrame(uint frameNumber)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_associatedDisplayIndex is not null && _displayInvalidHResult >= 0)
        {
            using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
            _device.AdvanceFrame(frameNumber);
        }
    }

    internal void SetPosition(Direct3D9WindowPosition position)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _windowPosition = position;
    }

    internal void UpdatePresentProperties(
        MilTransparencyFlags transparencyFlags,
        byte constantAlpha,
        uint colorKey)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (transparencyFlags == MilTransparencyFlags.Opaque)
        {
            _updateLayeredWindowFlags = UpdateLayeredWindowOpaque;
            _sourceConstantAlpha = byte.MaxValue;
            _alphaFormat = 0;
            return;
        }

        _updateLayeredWindowFlags = (transparencyFlags & MilTransparencyFlags.ColorKey) != 0
            ? UpdateLayeredWindowColorKey
            : 0;
        if ((transparencyFlags & MilTransparencyFlags.ConstantAlpha) != 0)
        {
            _updateLayeredWindowFlags |= UpdateLayeredWindowAlpha;
        }

        if ((transparencyFlags & MilTransparencyFlags.PerPixelAlpha) != 0
            && (_initializationFlags & Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha) != 0)
        {
            _updateLayeredWindowFlags |= UpdateLayeredWindowAlpha;
            _alphaFormat = SourceAlpha;
        }
        else
        {
            _alphaFormat = 0;
        }

        if (_updateLayeredWindowFlags == 0)
        {
            _updateLayeredWindowFlags = UpdateLayeredWindowOpaque;
        }

        _sourceConstantAlpha = constantAlpha;
        _colorKey = colorKey;
    }

    internal int ScrollBlt(Direct3D9SurfaceRect source, Direct3D9SurfaceRect destination)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return Direct3D9Factory.NotImplementedHResult;
    }

    internal int FindInterface(Guid interfaceId, out nint interfacePointer)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        interfacePointer = 0;
        return Direct3D9Factory.NoInterfaceHResult;
    }

    internal static bool IsOnPixelBoundary(float value)
    {
        const int fix4One = 1 << 4;
        int fixedValue = checked((int) MathF.Floor((value * fix4One) + 0.5f));
        return fixedValue % fix4One == 0;
    }

    internal static int GetPresentFlags(
        Format backBufferFormat,
        Format displayFormat,
        Caps9 capabilities,
        out uint presentFlags)
    {
        presentFlags = 0;
        if (backBufferFormat != Format.A2R10G10B10 || displayFormat == Format.A2R10G10B10)
        {
            return 0;
        }

        if ((capabilities.Caps3 & (uint) D3D9.Caps3LinearToSrgbPresentation) == 0)
        {
            return Direct3D9Factory.DisplayFormatNotSupportedHResult;
        }

        presentFlags = (uint) D3D9.PresentLinearContent;
        return 0;
    }

    internal int DrawBitmap(Func<int> drawBitmap) => ExecuteDisplayDrawing(drawBitmap);

    internal int ProductionDrawBitmap(
        Direct3D9BitmapDrawState drawState,
        nint bitmapSource,
        nint effects,
        Direct3D9ProductionBitmapDrawOperations operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        return DrawBitmap(
            drawState,
            bitmapSource,
            effects,
            operations.GetScratchBitmapBrush,
            operations.SetScratchBitmapBrush,
            operations.ClearScratchBitmapBrush,
            operations.CreateBrushRealizer,
            operations.CreateBitmapShape,
            operations.EnsureState,
            operations.ClipToSafeDeviceBounds,
            (shape, shapeToDevice, bounds, brush, worldToDevice, currentEffects) =>
                ProductionFillPathWithBrush(
                    shape,
                    shapeToDevice,
                    bounds,
                    brush,
                    worldToDevice,
                    currentEffects,
                    operations.AntiAliasMode,
                    operations.CurrentClip,
                    operations.ApplyGuidelines,
                    operations.ApplyBrushClip,
                    operations.GetBoundsInDeviceSpace,
                    operations.CreateHardwareBrush,
                    operations.CreateAntialiasedGeometryGenerator,
                    operations.CreateAliasedGeometryGenerator,
                    (geometryGenerator, hardwareBrush, acceleratedEffects, brushContext) =>
                        ProductionAcceleratedFillPath(
                            operations.CompositingMode,
                            geometryGenerator,
                            hardwareBrush,
                            acceleratedEffects,
                            brushContext)),
            operations.SoftwareFillPath);
    }

    internal int DrawBitmap(
        Direct3D9BitmapDrawState drawState,
        nint bitmapSource,
        nint effects,
        Direct3D9GetScratchBitmapBrush getScratchBitmapBrush,
        Func<int> ensureState,
        Direct3D9FillBitmapPath fillPath)
    {
        ArgumentNullException.ThrowIfNull(fillPath);

        return DrawBitmap(
            drawState,
            bitmapSource,
            effects,
            getScratchBitmapBrush,
            static (_, _, _) => { },
            static _ => { },
            scratchBrush => new Direct3D9ImmediateBrushRealizer(
                scratchBrush,
                static _ => { },
                static _ => { },
                static (_, _) => { },
                static _ => false,
                static _ => Direct3D9BrushType.Bitmap,
                static _ => false,
                static _ => false,
                static _ => 0,
                static _ => 0,
                static (_, _, _, _) => 0,
                static (_, _, _, _) => 0,
                static (_, _) => { },
                static (_, _) => { }),
            ensureState,
            (sourceRect, brushRealizer, shapeToDevice, baseSamplingToDevice) => fillPath(
                brushRealizer.GetRealizedBrush(false),
                bitmapSource,
                brushRealizer.Effects,
                sourceRect,
                shapeToDevice,
                baseSamplingToDevice));
    }

    internal int DrawBitmap(
        Direct3D9BitmapDrawState drawState,
        nint bitmapSource,
        nint effects,
        Direct3D9GetScratchBitmapBrush getScratchBitmapBrush,
        Direct3D9SetScratchBitmapBrush setScratchBitmapBrush,
        Action<nint> clearScratchBitmapBrush,
        Func<nint, Direct3D9ImmediateBrushRealizer> createBrushRealizer,
        Direct3D9CreateBitmapShape createBitmapShape,
        Func<int> ensureState,
        Direct3D9ClipToSafeDeviceBounds clipToSafeDeviceBounds,
        Direct3D9FillPathWithBrush fillPathWithBrush,
        Direct3D9SoftwareFillBitmapPath softwareFillPath)
    {
        ArgumentNullException.ThrowIfNull(createBitmapShape);
        ArgumentNullException.ThrowIfNull(clipToSafeDeviceBounds);
        ArgumentNullException.ThrowIfNull(fillPathWithBrush);
        ArgumentNullException.ThrowIfNull(softwareFillPath);

        nint shape = 0;
        return DrawBitmap(
            drawState,
            bitmapSource,
            effects,
            getScratchBitmapBrush,
            setScratchBitmapBrush,
            clearScratchBitmapBrush,
            createBrushRealizer,
            sourceRect =>
            {
                int result = createBitmapShape(sourceRect, out shape);
                if (result < 0)
                {
                    return result;
                }

                return shape == 0 ? Direct3D9Factory.InternalErrorHResult : 0;
            },
            ensureState,
            (sourceRect, brushRealizer, shapeToDevice, baseSamplingToDevice) =>
            {
                return FillPath(
                    shape,
                    shapeToDevice,
                    sourceRect,
                    brushRealizer.GetRealizedBrush(false),
                    baseSamplingToDevice,
                    clipToSafeDeviceBounds,
                    (bool convertNullToTransparent, out nint brush, out nint realizedEffects) =>
                    {
                        brush = brushRealizer.GetRealizedBrush(convertNullToTransparent);
                        realizedEffects = brushRealizer.Effects;
                        return 0;
                    },
                    ensureState,
                    fillPathWithBrush,
                    (currentShape, currentShapeToDevice, _, reasonForFallback) => softwareFillPath(
                        currentShape,
                        currentShapeToDevice,
                        brushRealizer,
                        reasonForFallback));
            });
    }

    internal int DrawBitmap(
        Direct3D9BitmapDrawState drawState,
        nint bitmapSource,
        nint effects,
        Direct3D9GetScratchBitmapBrush getScratchBitmapBrush,
        Direct3D9SetScratchBitmapBrush setScratchBitmapBrush,
        Action<nint> clearScratchBitmapBrush,
        Func<nint, Direct3D9ImmediateBrushRealizer> createBrushRealizer,
        Func<int> ensureState,
        Func<MilRectF, Direct3D9ImmediateBrushRealizer, Matrix4x4, Matrix4x4, int> fillPath)
    {
        return DrawBitmap(
            drawState,
            bitmapSource,
            effects,
            getScratchBitmapBrush,
            setScratchBitmapBrush,
            clearScratchBitmapBrush,
            createBrushRealizer,
            static _ => 0,
            ensureState,
            fillPath);
    }

    private int DrawBitmap(
        Direct3D9BitmapDrawState drawState,
        nint bitmapSource,
        nint effects,
        Direct3D9GetScratchBitmapBrush getScratchBitmapBrush,
        Direct3D9SetScratchBitmapBrush setScratchBitmapBrush,
        Action<nint> clearScratchBitmapBrush,
        Func<nint, Direct3D9ImmediateBrushRealizer> createBrushRealizer,
        Func<MilRectF, int> prepareShape,
        Func<int> ensureState,
        Func<MilRectF, Direct3D9ImmediateBrushRealizer, Matrix4x4, Matrix4x4, int> fillPath)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapSource);
        ArgumentNullException.ThrowIfNull(getScratchBitmapBrush);
        ArgumentNullException.ThrowIfNull(setScratchBitmapBrush);
        ArgumentNullException.ThrowIfNull(clearScratchBitmapBrush);
        ArgumentNullException.ThrowIfNull(createBrushRealizer);
        ArgumentNullException.ThrowIfNull(prepareShape);
        ArgumentNullException.ThrowIfNull(ensureState);
        ArgumentNullException.ThrowIfNull(fillPath);
        if (!_isRenderingEnabled)
        {
            return 0;
        }

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        using Direct3D9UseContextGuard useContext = new(_device);

        if ((_swapChain is not null || _renderTargetSurface is not null) && !IsValid)
        {
            return CompleteDisplayDrawing(0);
        }

        int result = getScratchBitmapBrush(out nint scratchBrush);
        if (result < 0)
        {
            return CompleteDisplayDrawing(NormalizeNoRenderResult(result));
        }

        MilRectF sourceRect;
        if (drawState.SourceRect is Direct3D9PointAndSizeRect explicitSourceRect)
        {
            sourceRect = new MilRectF(
                explicitSourceRect.X,
                explicitSourceRect.Y,
                explicitSourceRect.X + explicitSourceRect.Width,
                explicitSourceRect.Y + explicitSourceRect.Height);
        }
        else
        {
            result = GetBitmapSourceBounds(bitmapSource, out sourceRect);
            if (result < 0)
            {
                return CompleteDisplayDrawing(NormalizeNoRenderResult(result));
            }
        }

        result = prepareShape(sourceRect);
        if (result < 0)
        {
            return CompleteDisplayDrawing(NormalizeNoRenderResult(result));
        }

        result = ensureState();
        if (result < 0)
        {
            return CompleteDisplayDrawing(NormalizeNoRenderResult(result));
        }

        if (result == Direct3D9Factory.ClippedToEmptyHResult)
        {
            return CompleteDisplayDrawing(0);
        }

        Direct3D9ImmediateBrushRealizer? brushRealizer = null;
        bool scratchBrushWasSet = false;
        try
        {
            setScratchBitmapBrush(scratchBrush, bitmapSource, drawState.WorldToDevice);
            scratchBrushWasSet = true;

            brushRealizer = createBrushRealizer(scratchBrush)
                ?? throw new InvalidOperationException("The immediate brush realizer factory returned null.");
            brushRealizer.SetBrush(scratchBrush, effects, skipMetaFixups: true);

            result = fillPath(
                sourceRect,
                brushRealizer,
                drawState.WorldToDevice,
                drawState.WorldToDevice);
            return CompleteDisplayDrawing(NormalizeNoRenderResult(result));
        }
        finally
        {
            brushRealizer?.Release();
            if (scratchBrushWasSet)
            {
                clearScratchBitmapBrush(scratchBrush);
            }
        }
    }

    internal int DrawMesh3D(Func<int> drawMesh3D) => ExecuteDisplayDrawing(drawMesh3D);

    internal int ProductionDrawMesh3D(
        Direct3D9ContextState contextState,
        Direct3D9ProductionMeshDrawOperations operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        if (!_in3D)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        return DrawMesh3DCore(
            contextState,
            operations.EnsureBrushRealizations,
            operations.EnsureState ?? EnsureState,
            operations.ApplyProjectedMeshTo2DState,
            operations.DeriveMeshShader,
            operations.IsMeshBoundsDebugEnabled,
            operations.GetMeshBounds,
            operations.DrawMeshBounds);
    }

    internal int DrawMesh3D(
        Func<int> beginShader,
        Func<bool> canRunShaderPath,
        Func<int> shaderDrawMesh3D,
        Func<int> fixedFunctionDrawMesh3D,
        Func<int> finishShader)
    {
        return ExecuteDisplayDrawing(() => Direct3D9MeshShaderRenderer.Render(
            beginShader,
            canRunShaderPath,
            shaderDrawMesh3D,
            fixedFunctionDrawMesh3D,
            finishShader));
    }

    internal int DrawMesh3D(
        Direct3D9ContextState contextState,
        Func<int> ensureBrushRealizations,
        Direct3D9ApplyProjectedMeshTo2DState applyProjectedMeshTo2DState,
        Direct3D9MeshShaderType shaderType,
        Direct3D9GetMeshShaderSurfaceSource getSurfaceSource,
        Direct3D9FixedFunctionMeshShaderFactory shaderFactory,
        Direct3D9FixedFunctionMeshDrawData drawData)
    {
        ArgumentNullException.ThrowIfNull(getSurfaceSource);
        ArgumentNullException.ThrowIfNull(shaderFactory);
        ArgumentNullException.ThrowIfNull(drawData);

        return DrawMesh3D(
            contextState,
            ensureBrushRealizations,
            applyProjectedMeshTo2DState,
            (Direct3D9ProjectedMeshState projectedMeshState, out Direct3D9DerivedMeshShader? shader) =>
                shaderFactory.Derive(
                    shaderType,
                    getSurfaceSource,
                    projectedMeshState,
                    drawData,
                    _device,
                    out shader));
    }

    internal int DrawMesh3D(
        Direct3D9ContextState contextState,
        Func<int> ensureBrushRealizations,
        Direct3D9ApplyProjectedMeshTo2DState applyProjectedMeshTo2DState,
        Direct3D9DeriveMeshShader deriveMeshShader,
        Func<bool>? isMeshBoundsDebugEnabled = null,
        Direct3D9GetMeshBounds? getMeshBounds = null,
        Func<Direct3D9Box, int>? drawMeshBounds = null)
    {
        ArgumentNullException.ThrowIfNull(ensureBrushRealizations);
        ArgumentNullException.ThrowIfNull(applyProjectedMeshTo2DState);
        ArgumentNullException.ThrowIfNull(deriveMeshShader);

        return DrawMesh3DCore(
            contextState,
            ensureBrushRealizations,
            EnsureState,
            applyProjectedMeshTo2DState,
            deriveMeshShader,
            isMeshBoundsDebugEnabled,
            getMeshBounds,
            drawMeshBounds);
    }

    private int DrawMesh3DCore(
        Direct3D9ContextState contextState,
        Func<int> ensureBrushRealizations,
        Func<Direct3D9ContextState, int> ensureState,
        Direct3D9ApplyProjectedMeshTo2DState applyProjectedMeshTo2DState,
        Direct3D9DeriveMeshShader deriveMeshShader,
        Func<bool>? isMeshBoundsDebugEnabled,
        Direct3D9GetMeshBounds? getMeshBounds,
        Func<Direct3D9Box, int>? drawMeshBounds)
    {
        return ExecuteDisplayDrawing(() =>
        {
            if (_bounds.Right <= _bounds.Left || _bounds.Bottom <= _bounds.Top)
            {
                return 0;
            }

            if (_isDraw3DDisabled())
            {
                return 0;
            }

            int result = ensureBrushRealizations();
            if (result < 0)
            {
                return NormalizeMeshResult(result);
            }

            result = ensureState(contextState);
            if (result == Direct3D9Factory.ClippedToEmptyHResult)
            {
                return 0;
            }

            if (result < 0)
            {
                return NormalizeMeshResult(result);
            }

            Direct3D9SurfaceRect aliasedClip = contextState.AliasedClip ?? _bounds;
            if (!TryIntersect(_bounds, aliasedClip, out Direct3D9SurfaceRect currentClip))
            {
                return 0;
            }

            result = applyProjectedMeshTo2DState(contextState, currentClip, out Direct3D9ProjectedMeshState projectedMeshState);
            if (result < 0)
            {
                return NormalizeMeshResult(result);
            }

            if (!projectedMeshState.IsVisible)
            {
                return 0;
            }

            result = deriveMeshShader(projectedMeshState, out Direct3D9DerivedMeshShader? shader);
            if (result < 0)
            {
                shader?.Dispose();
                return NormalizeMeshResult(result);
            }

            if (shader is null)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }

            using (shader)
            {
                result = Direct3D9MeshShaderRenderer.Render(
                    shader.Begin,
                    shader.CanRunShaderPath,
                    shader.ShaderDrawMesh3D,
                    shader.FixedFunctionDrawMesh3D,
                    shader.Finish);
                if (result < 0)
                {
                    return NormalizeMeshResult(result);
                }

                if (isMeshBoundsDebugEnabled?.Invoke() == true
                    && getMeshBounds is not null
                    && drawMeshBounds is not null
                    && getMeshBounds(out Direct3D9Box meshBounds) >= 0)
                {
                    _ = drawMeshBounds(meshBounds);
                }

                return result;
            }
        });
    }

    internal int FillPath(
        nint shape,
        Matrix4x4? shapeToDevice,
        MilRectF shapeBounds,
        nint brushRealizer,
        Matrix4x4 worldToDevice,
        Direct3D9ClipToSafeDeviceBounds clipToSafeDeviceBounds,
        Direct3D9GetRealizedBrush getRealizedBrush,
        Func<int> ensureState,
        Direct3D9FillPathWithBrush fillPathWithBrush,
        Direct3D9SoftwareFillPath softwareFillPath)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfZero(shape);
        ArgumentOutOfRangeException.ThrowIfZero(brushRealizer);
        ArgumentNullException.ThrowIfNull(clipToSafeDeviceBounds);
        ArgumentNullException.ThrowIfNull(getRealizedBrush);
        ArgumentNullException.ThrowIfNull(ensureState);
        ArgumentNullException.ThrowIfNull(fillPathWithBrush);
        ArgumentNullException.ThrowIfNull(softwareFillPath);

        using Direct3D9UseContextGuard useContext = new(_device);

        int result = clipToSafeDeviceBounds(shape, shapeToDevice, shapeBounds, out Direct3D9SafeClippedShape clippedShape);
        nint currentShape = shape;
        Matrix4x4? currentShapeToDevice = shapeToDevice;
        MilRectF currentBounds = shapeBounds;
        if (result >= 0 && clippedShape.WasClipped)
        {
            currentShape = clippedShape.Shape;
            currentShapeToDevice = null;
            currentBounds = clippedShape.Bounds;
        }

        if (result >= 0)
        {
            result = getRealizedBrush(false, out nint brush, out nint effects);
            if (result >= 0 && brush != 0)
            {
                result = ensureState();
                if (result == Direct3D9Factory.ClippedToEmptyHResult)
                {
                    result = 0;
                }
                else if (result >= 0)
                {
                    result = fillPathWithBrush(
                        currentShape,
                        currentShapeToDevice,
                        currentBounds,
                        brush,
                        worldToDevice,
                        effects);
                }
            }
        }

        if (result == Direct3D9Factory.NotImplementedHResult)
        {
            result = softwareFillPath(currentShape, currentShapeToDevice, brushRealizer, result);
        }

        return NormalizeNoRenderResult(result);
    }

    internal int FillPathWithBrush(
        nint shape,
        Matrix4x4? shapeToDevice,
        MilRectF shapeBounds,
        nint brush,
        Matrix4x4 worldToDevice,
        nint effects,
        MilAntiAliasMode antiAliasMode,
        Direct3D9SurfaceRect currentClip,
        Direct3D9ApplyPathGuidelines applyGuidelines,
        Direct3D9ApplyPathBrushClip applyBrushClip,
        Direct3D9GetPathBoundsInDeviceSpace getBoundsInDeviceSpace,
        Direct3D9DeriveHardwareBrush deriveHardwareBrush,
        Direct3D9CreatePathGeometryGenerator createAntialiasedGeometryGenerator,
        Direct3D9CreatePathGeometryGenerator createAliasedGeometryGenerator,
        Direct3D9AcceleratedFillPath acceleratedFillPath,
        Action<nint> releaseHardwareBrush,
        Action<nint> releaseGeometryGenerator)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfZero(shape);
        ArgumentOutOfRangeException.ThrowIfZero(brush);
        ArgumentNullException.ThrowIfNull(applyGuidelines);
        ArgumentNullException.ThrowIfNull(applyBrushClip);
        ArgumentNullException.ThrowIfNull(getBoundsInDeviceSpace);
        ArgumentNullException.ThrowIfNull(deriveHardwareBrush);
        ArgumentNullException.ThrowIfNull(createAntialiasedGeometryGenerator);
        ArgumentNullException.ThrowIfNull(createAliasedGeometryGenerator);
        ArgumentNullException.ThrowIfNull(acceleratedFillPath);
        ArgumentNullException.ThrowIfNull(releaseHardwareBrush);
        ArgumentNullException.ThrowIfNull(releaseGeometryGenerator);

        Direct3D9PathClipperState clipperState = new(shape, shapeToDevice, shapeBounds);
        int result = applyGuidelines(clipperState, out clipperState);
        if (result < 0)
        {
            return result;
        }

        result = applyBrushClip(clipperState, brush, worldToDevice, out clipperState);
        if (result < 0)
        {
            return result;
        }

        result = getBoundsInDeviceSpace(clipperState, out MilRectF deviceBounds);
        if (result < 0 || !TryIntersectBoundsWithSurface(deviceBounds, currentClip, antiAliasMode, out Direct3D9SurfaceRect renderingBounds))
        {
            return result;
        }

        Direct3D9PathBrushContext brushContext = new(
            worldToDevice,
            renderingBounds,
            new MilRectF(renderingBounds.Left, renderingBounds.Top, renderingBounds.Right, renderingBounds.Bottom),
            CanFallback: true);
        nint hardwareBrush = 0;
        nint geometryGenerator = 0;
        try
        {
            result = deriveHardwareBrush(brush, brushContext, out hardwareBrush);
            if (result < 0)
            {
                return result;
            }

            Direct3D9CreatePathGeometryGenerator createGeometryGenerator = antiAliasMode == MilAntiAliasMode.None
                ? createAliasedGeometryGenerator
                : createAntialiasedGeometryGenerator;
            result = createGeometryGenerator(clipperState, out geometryGenerator);
            if (result == Direct3D9Factory.EmptyFillHResult)
            {
                return 0;
            }

            if (result < 0 || geometryGenerator == 0)
            {
                return result;
            }

            return acceleratedFillPath(geometryGenerator, hardwareBrush, effects, brushContext);
        }
        finally
        {
            if (hardwareBrush != 0)
            {
                releaseHardwareBrush(hardwareBrush);
            }

            if (geometryGenerator != 0)
            {
                releaseGeometryGenerator(geometryGenerator);
            }
        }
    }

    internal int ProductionFillPathWithBrush(
        nint shape,
        Matrix4x4? shapeToDevice,
        MilRectF shapeBounds,
        nint brush,
        Matrix4x4 worldToDevice,
        nint effects,
        MilAntiAliasMode antiAliasMode,
        Direct3D9SurfaceRect currentClip,
        Direct3D9ApplyPathGuidelines applyGuidelines,
        Direct3D9ApplyPathBrushClip applyBrushClip,
        Direct3D9GetPathBoundsInDeviceSpace getBoundsInDeviceSpace,
        Direct3D9CreatePathHardwareBrush createHardwareBrush,
        Direct3D9CreateTypedPathGeometryGenerator createAntialiasedGeometryGenerator,
        Direct3D9CreateTypedPathGeometryGenerator createAliasedGeometryGenerator,
        Direct3D9ExecuteTypedAcceleratedFillPath acceleratedFillPath)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfZero(shape);
        ArgumentOutOfRangeException.ThrowIfZero(brush);
        ArgumentNullException.ThrowIfNull(applyGuidelines);
        ArgumentNullException.ThrowIfNull(applyBrushClip);
        ArgumentNullException.ThrowIfNull(getBoundsInDeviceSpace);
        ArgumentNullException.ThrowIfNull(createHardwareBrush);
        ArgumentNullException.ThrowIfNull(createAntialiasedGeometryGenerator);
        ArgumentNullException.ThrowIfNull(createAliasedGeometryGenerator);
        ArgumentNullException.ThrowIfNull(acceleratedFillPath);

        Direct3D9PathClipperState clipperState = new(shape, shapeToDevice, shapeBounds);
        int result = applyGuidelines(clipperState, out clipperState);
        if (result < 0)
        {
            return result;
        }

        result = applyBrushClip(clipperState, brush, worldToDevice, out clipperState);
        if (result < 0)
        {
            return result;
        }

        result = getBoundsInDeviceSpace(clipperState, out MilRectF deviceBounds);
        if (result < 0 || !TryIntersectBoundsWithSurface(deviceBounds, currentClip, antiAliasMode, out Direct3D9SurfaceRect renderingBounds))
        {
            return result;
        }

        Direct3D9PathBrushContext brushContext = new(
            worldToDevice,
            renderingBounds,
            new MilRectF(renderingBounds.Left, renderingBounds.Top, renderingBounds.Right, renderingBounds.Bottom),
            CanFallback: true);
        Direct3D9PathHardwareBrush? hardwareBrush = null;
        Direct3D9PathGeometryGenerator? geometryGenerator = null;
        try
        {
            result = createHardwareBrush(brush, brushContext, out hardwareBrush);
            if (result < 0)
            {
                return result;
            }

            if (hardwareBrush is null)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }

            Direct3D9CreateTypedPathGeometryGenerator createGeometryGenerator = antiAliasMode == MilAntiAliasMode.None
                ? createAliasedGeometryGenerator
                : createAntialiasedGeometryGenerator;
            result = createGeometryGenerator(clipperState, out geometryGenerator);
            if (result == Direct3D9Factory.EmptyFillHResult)
            {
                return Direct3D9Factory.SuccessHResult;
            }

            if (result < 0)
            {
                return result;
            }

            if (geometryGenerator is null)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }

            return acceleratedFillPath(geometryGenerator, hardwareBrush, effects, brushContext);
        }
        finally
        {
            hardwareBrush?.Dispose();
            geometryGenerator?.Dispose();
        }
    }

    internal int ProductionAcceleratedFillPath(
        MilCompositingMode compositingMode,
        Direct3D9PathGeometryGenerator geometryGenerator,
        Direct3D9PathHardwareBrush hardwareBrush,
        Direct3D9EffectList? effects,
        Direct3D9PathBrushContext effectContext)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(geometryGenerator);
        ArgumentNullException.ThrowIfNull(hardwareBrush);
        ArgumentNullException.ThrowIfNull(effectContext);

        int result = ExecutePathPipeline(
            compositingMode,
            geometryGenerator,
            hardwareBrush,
            effects,
            effectContext,
            static (Direct3D9PathHardwareBrush brush, MilCompositingMode mode, Direct3D9EffectList? currentEffects, Direct3D9PathBrushContext context, out Direct3D9PathPipelineDescription? description) =>
                brush.CreateShaderPipeline(mode, currentEffects, context, out description));
        return result == Direct3D9Factory.NotImplementedHResult
            ? ExecutePathPipeline(
                compositingMode,
                geometryGenerator,
                hardwareBrush,
                effects,
                effectContext,
                static (Direct3D9PathHardwareBrush brush, MilCompositingMode mode, Direct3D9EffectList? currentEffects, Direct3D9PathBrushContext context, out Direct3D9PathPipelineDescription? description) =>
                    brush.CreateFixedFunctionPipeline(mode, currentEffects, context, out description))
            : result;
    }

    internal int ProductionAcceleratedFillPath(
        MilCompositingMode compositingMode,
        Direct3D9PathGeometryGenerator geometryGenerator,
        Direct3D9PathHardwareBrush hardwareBrush,
        nint effects,
        Direct3D9PathBrushContext effectContext)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(geometryGenerator);
        ArgumentNullException.ThrowIfNull(hardwareBrush);
        ArgumentNullException.ThrowIfNull(effectContext);

        int result = ExecutePathPipeline(
            compositingMode,
            geometryGenerator,
            hardwareBrush,
            effects,
            effectContext,
            static (Direct3D9PathHardwareBrush brush, MilCompositingMode mode, nint currentEffects, Direct3D9PathBrushContext context, out Direct3D9PathPipelineDescription? description) =>
                brush.CreateShaderPipeline(mode, currentEffects, context, out description));
        return result == Direct3D9Factory.NotImplementedHResult
            ? ExecutePathPipeline(
                compositingMode,
                geometryGenerator,
                hardwareBrush,
                effects,
                effectContext,
                static (Direct3D9PathHardwareBrush brush, MilCompositingMode mode, nint currentEffects, Direct3D9PathBrushContext context, out Direct3D9PathPipelineDescription? description) =>
                    brush.CreateFixedFunctionPipeline(mode, currentEffects, context, out description))
            : result;
    }

    private static int ExecutePathPipeline(
        MilCompositingMode compositingMode,
        Direct3D9PathGeometryGenerator geometryGenerator,
        Direct3D9PathHardwareBrush hardwareBrush,
        Direct3D9EffectList? effects,
        Direct3D9PathBrushContext effectContext,
        Direct3D9CreateOwnedEffectPathPipelineDescription createPipeline)
    {
        int result = createPipeline(hardwareBrush, compositingMode, effects, effectContext, out Direct3D9PathPipelineDescription? description);
        if (result < 0)
        {
            return result;
        }

        if (description is null)
        {
            return Direct3D9Factory.InternalErrorHResult;
        }

        Direct3D9Pipeline? pipeline = null;
        try
        {
            result = description.Initializer.Initialize(
                geometryGenerator,
                description.OutsideBounds,
                description.NeedInside,
                out pipeline);
            if (result < 0)
            {
                return result;
            }

            return pipeline?.Execute() ?? Direct3D9Factory.InternalErrorHResult;
        }
        finally
        {
            pipeline?.ReleaseExpensiveResources();
        }
    }

    private static int ExecutePathPipeline(
        MilCompositingMode compositingMode,
        Direct3D9PathGeometryGenerator geometryGenerator,
        Direct3D9PathHardwareBrush hardwareBrush,
        nint effects,
        Direct3D9PathBrushContext effectContext,
        Direct3D9CreateOwnedPathPipelineDescription createPipeline)
    {
        int result = createPipeline(hardwareBrush, compositingMode, effects, effectContext, out Direct3D9PathPipelineDescription? description);
        if (result < 0)
        {
            return result;
        }

        if (description is null)
        {
            return Direct3D9Factory.InternalErrorHResult;
        }

        Direct3D9Pipeline? pipeline = null;
        try
        {
            result = description.Initializer.Initialize(
                geometryGenerator,
                description.OutsideBounds,
                description.NeedInside,
                out pipeline);
            if (result < 0)
            {
                return result;
            }

            return pipeline?.Execute() ?? Direct3D9Factory.InternalErrorHResult;
        }
        finally
        {
            pipeline?.ReleaseExpensiveResources();
        }
    }

    internal int HwShaderFillPath(
        nint shader,
        nint shape,
        Matrix4x4? shapeToDevice,
        Direct3D9SurfaceRect renderingBounds,
        MilAntiAliasMode antiAliasMode,
        bool useZBuffer,
        Direct3D9CreatePathGeometryGenerator createAntialiasedGeometryGenerator,
        Direct3D9CreatePathGeometryGenerator createAliasedGeometryGenerator,
        Direct3D9DrawShaderPathGeometry drawShaderPathGeometry,
        Action<nint> releaseGeometryGenerator)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfZero(shader);
        ArgumentOutOfRangeException.ThrowIfZero(shape);
        ArgumentNullException.ThrowIfNull(createAntialiasedGeometryGenerator);
        ArgumentNullException.ThrowIfNull(createAliasedGeometryGenerator);
        ArgumentNullException.ThrowIfNull(drawShaderPathGeometry);
        ArgumentNullException.ThrowIfNull(releaseGeometryGenerator);

        Direct3D9PathClipperState shapeState = new(
            shape,
            shapeToDevice,
            new MilRectF(renderingBounds.Left, renderingBounds.Top, renderingBounds.Right, renderingBounds.Bottom));
        nint geometryGenerator = 0;
        try
        {
            Direct3D9CreatePathGeometryGenerator createGeometryGenerator = antiAliasMode == MilAntiAliasMode.None
                ? createAliasedGeometryGenerator
                : createAntialiasedGeometryGenerator;
            int result = createGeometryGenerator(shapeState, out geometryGenerator);
            if (result == Direct3D9Factory.EmptyFillHResult)
            {
                return 0;
            }

            if (result < 0)
            {
                return result;
            }

            if (geometryGenerator == 0)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }

            return drawShaderPathGeometry(shader, geometryGenerator, renderingBounds, useZBuffer);
        }
        finally
        {
            if (geometryGenerator != 0)
            {
                releaseGeometryGenerator(geometryGenerator);
            }
        }
    }

    internal int AcceleratedFillPath(
        MilCompositingMode compositingMode,
        nint geometryGenerator,
        nint hardwareBrush,
        nint effects,
        Direct3D9PathBrushContext effectContext,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside,
        Direct3D9CreateShaderPipeline createShaderPipeline)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfZero(geometryGenerator);
        ArgumentOutOfRangeException.ThrowIfZero(hardwareBrush);
        ArgumentNullException.ThrowIfNull(effectContext);
        ArgumentNullException.ThrowIfNull(createShaderPipeline);

        return ShaderAcceleratedFillPath(
            compositingMode,
            geometryGenerator,
            hardwareBrush,
            effects,
            effectContext,
            outsideBounds,
            needInside,
            createShaderPipeline);
    }

    internal int ShaderAcceleratedFillPath(
        MilCompositingMode compositingMode,
        nint geometryGenerator,
        nint hardwareBrush,
        nint effects,
        Direct3D9PathBrushContext effectContext,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside,
        Direct3D9CreateShaderPipeline createShaderPipeline)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfZero(geometryGenerator);
        ArgumentOutOfRangeException.ThrowIfZero(hardwareBrush);
        ArgumentNullException.ThrowIfNull(effectContext);
        ArgumentNullException.ThrowIfNull(createShaderPipeline);

        Direct3D9ShaderPipeline pipeline = createShaderPipeline(true, _device);
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(pipeline.InitializeForRendering);
        ArgumentNullException.ThrowIfNull(pipeline.Execute);
        ArgumentNullException.ThrowIfNull(pipeline.ReleaseExpensiveResources);

        try
        {
            int result = pipeline.InitializeForRendering(
                compositingMode,
                geometryGenerator,
                hardwareBrush,
                effects,
                effectContext,
                outsideBounds,
                needInside);
            return result < 0 ? result : pipeline.Execute();
        }
        finally
        {
            pipeline.ReleaseExpensiveResources();
        }
    }

    internal int DrawPath(Func<int> drawPath) => ExecuteDisplayDrawing(drawPath);

    internal int ProductionDrawPath(
        Matrix4x4 worldToDevice,
        nint shape,
        nint pen,
        nint strokeBrushRealizer,
        nint fillBrushRealizer,
        Direct3D9ProductionPathDrawOperations operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        return DrawPath(
            worldToDevice,
            shape,
            pen,
            strokeBrushRealizer,
            fillBrushRealizer,
            operations.EnsureBrushRealization,
            operations.GetShapeBounds,
            operations.WidenShape,
            operations.GetRealizedBrush,
            operations.ClipToSafeDeviceBounds,
            operations.EnsureState,
            (currentShape, currentShapeToDevice, bounds, brush, currentWorldToDevice, effects) =>
                ProductionFillPathWithBrush(
                    currentShape,
                    currentShapeToDevice,
                    bounds,
                    brush,
                    currentWorldToDevice,
                    effects,
                    operations.AntiAliasMode,
                    operations.CurrentClip,
                    operations.ApplyGuidelines,
                    operations.ApplyBrushClip,
                    operations.GetBoundsInDeviceSpace,
                    operations.CreateHardwareBrush,
                    operations.CreateAntialiasedGeometryGenerator,
                    operations.CreateAliasedGeometryGenerator,
                    (geometryGenerator, hardwareBrush, currentEffects, brushContext) =>
                        ProductionAcceleratedFillPath(
                            operations.CompositingMode,
                            geometryGenerator,
                            hardwareBrush,
                            currentEffects,
                            brushContext)),
            operations.SoftwareFillPath);
    }

    internal int DrawPath(
        Matrix4x4 worldToDevice,
        nint shape,
        nint pen,
        nint strokeBrushRealizer,
        nint fillBrushRealizer,
        Direct3D9EnsurePathBrushRealization ensureBrushRealization,
        Direct3D9GetPathShapeBounds getShapeBounds,
        Direct3D9WidenPathShape widenShape,
        Direct3D9GetPathRealizedBrush getRealizedBrush,
        Direct3D9ClipToSafeDeviceBounds clipToSafeDeviceBounds,
        Func<int> ensureState,
        Direct3D9FillPathWithBrush fillPathWithBrush,
        Direct3D9SoftwareFillPath softwareFillPath)
    {
        if (!_isRenderingEnabled)
        {
            return DrawPathInternal(
                worldToDevice,
                worldToDevice,
                shape,
                pen,
                strokeBrushRealizer,
                fillBrushRealizer,
                ensureBrushRealization,
                getShapeBounds,
                widenShape,
                getRealizedBrush,
                clipToSafeDeviceBounds,
                ensureState,
                fillPathWithBrush,
                softwareFillPath);
        }

        return CompleteDisplayDrawing(DrawPathInternal(
            worldToDevice,
            worldToDevice,
            shape,
            pen,
            strokeBrushRealizer,
            fillBrushRealizer,
            ensureBrushRealization,
            getShapeBounds,
            widenShape,
            getRealizedBrush,
            clipToSafeDeviceBounds,
            ensureState,
            fillPathWithBrush,
            softwareFillPath));
    }

    private int DrawPathInternal(
        Matrix4x4 worldToDevice,
        Matrix4x4? shapeToDevice,
        nint shape,
        nint pen,
        nint strokeBrushRealizer,
        nint fillBrushRealizer,
        Direct3D9EnsurePathBrushRealization ensureBrushRealization,
        Direct3D9GetPathShapeBounds getShapeBounds,
        Direct3D9WidenPathShape widenShape,
        Direct3D9GetPathRealizedBrush getRealizedBrush,
        Direct3D9ClipToSafeDeviceBounds clipToSafeDeviceBounds,
        Func<int> ensureState,
        Direct3D9FillPathWithBrush fillPathWithBrush,
        Direct3D9SoftwareFillPath softwareFillPath,
        Direct3D9CreateInfinitePathShape? createInfinitePathShape = null,
        MilRectF infinitePathBounds = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (createInfinitePathShape is null)
        {
            ArgumentOutOfRangeException.ThrowIfZero(shape);
        }

        ArgumentNullException.ThrowIfNull(ensureBrushRealization);
        ArgumentNullException.ThrowIfNull(getShapeBounds);
        ArgumentNullException.ThrowIfNull(widenShape);
        ArgumentNullException.ThrowIfNull(getRealizedBrush);
        ArgumentNullException.ThrowIfNull(clipToSafeDeviceBounds);
        ArgumentNullException.ThrowIfNull(ensureState);
        ArgumentNullException.ThrowIfNull(fillPathWithBrush);
        ArgumentNullException.ThrowIfNull(softwareFillPath);
        if (!_isRenderingEnabled)
        {
            return 0;
        }

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        using Direct3D9UseContextGuard useContext = new(_device);

        if ((_swapChain is not null || _renderTargetSurface is not null) && !IsValid)
        {
            return 0;
        }

        if (createInfinitePathShape is not null)
        {
            int result = createInfinitePathShape(infinitePathBounds, out shape);
            if (result < 0)
            {
                return NormalizeNoRenderResult(result);
            }

            if (shape == 0)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }
        }

        if (fillBrushRealizer != 0)
        {
            int result = ensureBrushRealization(fillBrushRealizer);
            if (result < 0)
            {
                return NormalizeNoRenderResult(result);
            }

            result = getShapeBounds(shape, out MilRectF fillBounds);
            if (result < 0)
            {
                return NormalizeNoRenderResult(result);
            }

            result = FillPath(
                shape,
                shapeToDevice,
                fillBounds,
                fillBrushRealizer,
                worldToDevice,
                clipToSafeDeviceBounds,
                (bool convertNullToTransparent, out nint brush, out nint effects) => getRealizedBrush(
                    fillBrushRealizer,
                    convertNullToTransparent,
                    out brush,
                    out effects),
                ensureState,
                fillPathWithBrush,
                softwareFillPath);
            if (result < 0)
            {
                return NormalizeNoRenderResult(result);
            }
        }

        if (pen != 0 && strokeBrushRealizer != 0)
        {
            int result = ensureBrushRealization(strokeBrushRealizer);
            if (result < 0)
            {
                return NormalizeNoRenderResult(result);
            }

            result = widenShape(shape, pen, shapeToDevice, _bounds, out nint widenedShape);
            if (result < 0)
            {
                return NormalizeNoRenderResult(result);
            }

            if (widenedShape == 0)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }

            result = getShapeBounds(widenedShape, out MilRectF widenedBounds);
            if (result < 0)
            {
                return NormalizeNoRenderResult(result);
            }

            result = FillPath(
                widenedShape,
                null,
                widenedBounds,
                strokeBrushRealizer,
                worldToDevice,
                clipToSafeDeviceBounds,
                (bool convertNullToTransparent, out nint brush, out nint effects) => getRealizedBrush(
                    strokeBrushRealizer,
                    convertNullToTransparent,
                    out brush,
                    out effects),
                ensureState,
                fillPathWithBrush,
                softwareFillPath);
            if (result < 0)
            {
                return NormalizeNoRenderResult(result);
            }
        }

        return 0;
    }

    internal int DrawInfinitePath(Func<int> drawInfinitePath) => ExecuteDisplayDrawing(drawInfinitePath);

    internal int ComposeEffect(
        Direct3D9EffectComposeState composeState,
        Func<int> ensureState,
        Func<MilCompositingMode, int> setAlphaBlendMode,
        Direct3D9ResolveImplicitEffectInput resolveImplicitInput,
        Func<nint, bool> isImplicitInputValid,
        Direct3D9ApplyEffect applyEffect)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfZero(composeState.ScaleTransform);
        ArgumentOutOfRangeException.ThrowIfZero(composeState.Effect);
        ArgumentNullException.ThrowIfNull(ensureState);
        ArgumentNullException.ThrowIfNull(setAlphaBlendMode);
        ArgumentNullException.ThrowIfNull(resolveImplicitInput);
        ArgumentNullException.ThrowIfNull(isImplicitInputValid);
        ArgumentNullException.ThrowIfNull(applyEffect);

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        using Direct3D9UseContextGuard useContext = new(_device);

        if (!IsValid)
        {
            return 0;
        }

        int result = ensureState();
        if (result == Direct3D9Factory.ClippedToEmptyHResult)
        {
            return 0;
        }

        if (result < 0)
        {
            return result;
        }

        result = setAlphaBlendMode(MilCompositingMode.SourceOver);
        if (result < 0)
        {
            return result;
        }

        nint textureRenderTarget = 0;
        if (composeState.ImplicitInput != 0)
        {
            result = resolveImplicitInput(composeState.ImplicitInput, out textureRenderTarget);
            if (result < 0)
            {
                return result;
            }

            if (textureRenderTarget == 0)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }

            if (!isImplicitInputValid(textureRenderTarget))
            {
                return 0;
            }
        }

        return applyEffect(
            composeState.Effect,
            composeState.ScaleTransform,
            composeState.IntermediateWidth,
            composeState.IntermediateHeight,
            textureRenderTarget);
    }

    internal int DrawInfinitePath(
        Matrix4x4 worldToDevice,
        nint fillBrushRealizer,
        Direct3D9CreateInfinitePathShape createInfinitePathShape,
        Direct3D9EnsurePathBrushRealization ensureBrushRealization,
        Direct3D9GetPathShapeBounds getShapeBounds,
        Direct3D9GetPathRealizedBrush getRealizedBrush,
        Direct3D9ClipToSafeDeviceBounds clipToSafeDeviceBounds,
        Func<int> ensureState,
        Direct3D9FillPathWithBrush fillPathWithBrush,
        Direct3D9SoftwareFillPath softwareFillPath)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfZero(fillBrushRealizer);
        ArgumentNullException.ThrowIfNull(createInfinitePathShape);
        if (!_isRenderingEnabled)
        {
            return 0;
        }

        MilRectF renderTargetBounds = new(_bounds.Left, _bounds.Top, _bounds.Right, _bounds.Bottom);
        return CompleteDisplayDrawing(DrawPathInternal(
            worldToDevice,
            null,
            0,
            0,
            0,
            fillBrushRealizer,
            ensureBrushRealization,
            getShapeBounds,
            static (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                widenedShape = 0;
                return Direct3D9Factory.InternalErrorHResult;
            },
            getRealizedBrush,
            clipToSafeDeviceBounds,
            ensureState,
            fillPathWithBrush,
            softwareFillPath,
            createInfinitePathShape,
            renderTargetBounds));
    }

    internal bool WasUsedToCreateHardwareRenderTarget
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _wasUsedToCreateHardwareRenderTarget;
        }
    }

    internal void ResetUsedToCreateHardwareRenderTarget()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _wasUsedToCreateHardwareRenderTarget = false;
    }

    internal int CreateRenderTargetBitmap(
        uint width,
        uint height,
        Direct3D9IntermediateRenderTargetUsage usage,
        Direct3D9RenderTargetInitializationFlags initializationFlags,
        bool hasValidRealizationCacheIndex,
        out Direct3D9TextureRenderTarget? renderTargetBitmap,
        Direct3D9CreateHardwareIntermediateRenderTarget? createHardwareRenderTarget = null)
    {
        const uint MaxIntToFloat = 1u << 24;

        ObjectDisposedException.ThrowIf(_isDisposed, this);
        renderTargetBitmap = null;

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);

        if (width > MaxIntToFloat || height > MaxIntToFloat)
        {
            return Direct3D9Factory.UnsupportedTextureSizeHResult;
        }

        bool wrapModeForcesSoftware = usage.WrapMode != MilBitmapWrapMode.Extend
            && (usage.Flags & Direct3D9IntermediateRenderTargetUsageFlags.ForUseIn3D) == 0;
        bool canCreateHardware = !wrapModeForcesSoftware
            && initializationFlags != Direct3D9RenderTargetInitializationFlags.SoftwareOnly
            && !_device.IsSoftwareDevice
            && hasValidRealizationCacheIndex;

        if (canCreateHardware)
        {
            createHardwareRenderTarget ??= static (
                uint candidateWidth,
                uint candidateHeight,
                Direct3D9Device candidateDevice,
                uint? displayIndex,
                bool _,
                out Direct3D9TextureRenderTarget? candidate) =>
                    Direct3D9TextureRenderTarget.TryCreate(candidateWidth, candidateHeight, candidateDevice, displayIndex, out candidate);
            int result = createHardwareRenderTarget(
                width,
                height,
                _device,
                _associatedDisplayIndex,
                (usage.Flags & Direct3D9IntermediateRenderTargetUsageFlags.ForBlending) != 0,
                out Direct3D9TextureRenderTarget? candidate);
            if (result < 0)
            {
                candidate?.Dispose();
                return result;
            }
            if (candidate is null)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            renderTargetBitmap = candidate;
            _wasUsedToCreateHardwareRenderTarget = true;
            return result;
        }

        if (initializationFlags == Direct3D9RenderTargetInitializationFlags.ForceCompatible)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        return Direct3D9Factory.UnsupportedOperationHResult;
    }

    internal int CreateRenderTargetBitmap(
        uint width,
        uint height,
        Direct3D9IntermediateRenderTargetUsage usage,
        Direct3D9RenderTargetInitializationFlags initializationFlags,
        bool hasValidRealizationCacheIndex,
        Direct3D9CreateIntermediateRenderTarget createHardwareRenderTarget,
        Direct3D9CreateIntermediateRenderTarget createSoftwareRenderTarget,
        out nint renderTargetBitmap)
    {
        const uint MaxIntToFloat = 1u << 24;

        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(createHardwareRenderTarget);
        ArgumentNullException.ThrowIfNull(createSoftwareRenderTarget);
        renderTargetBitmap = 0;

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);

        if (width > MaxIntToFloat || height > MaxIntToFloat)
        {
            return Direct3D9Factory.UnsupportedTextureSizeHResult;
        }

        bool wrapModeForcesSoftware = usage.WrapMode != MilBitmapWrapMode.Extend
            && (usage.Flags & Direct3D9IntermediateRenderTargetUsageFlags.ForUseIn3D) == 0;
        bool canCreateHardware = !wrapModeForcesSoftware
            && initializationFlags != Direct3D9RenderTargetInitializationFlags.SoftwareOnly
            && !_device.IsSoftwareDevice
            && hasValidRealizationCacheIndex;

        if (canCreateHardware)
        {
            int result = createHardwareRenderTarget(
                (usage.Flags & Direct3D9IntermediateRenderTargetUsageFlags.ForBlending) != 0,
                out renderTargetBitmap);
            if (result >= 0)
            {
                _wasUsedToCreateHardwareRenderTarget = true;
            }

            return result;
        }

        if (initializationFlags == Direct3D9RenderTargetInitializationFlags.ForceCompatible)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        return createSoftwareRenderTarget(
            (usage.Flags & Direct3D9IntermediateRenderTargetUsageFlags.ForBlending) != 0,
            out renderTargetBitmap);
    }

    internal int BeginLayer(Direct3D9LayerState layerState, Direct3D9LayerOperations operations)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(operations);
        if (!float.IsFinite(layerState.Alpha) || layerState.Alpha < 0 || layerState.Alpha > 1)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        Direct3D9SurfaceRect previousBounds = _bounds;
        Direct3D9SurfaceRect layerBounds = IntersectSurfaceRects(previousBounds, layerState.LayerBounds);
        bool isEmpty = IsEmpty(layerBounds) || layerState.Alpha <= 0;
        bool needsFixup = !isEmpty
            && (layerState.GeometricMask != 0 || layerState.Alpha < 1 || layerState.AlphaMaskBrush != 0);
        bool savedClearTypeHint = _forceClearType;
        nint geometricMask = 0;
        nint alphaMaskBrush = 0;
        nint sourceBitmap = 0;

        if (needsFixup && HasAlpha())
        {
            _forceClearType = false;
        }

        int result = 0;
        bool pushed = false;
        try
        {
            if (needsFixup && layerState.GeometricMask != 0)
            {
                result = operations.RetainGeometricMask(layerState.GeometricMask, out geometricMask);
                if (result < 0)
                {
                    return result;
                }
            }

            if (needsFixup && layerState.AlphaMaskBrush != 0)
            {
                result = operations.RetainAlphaMaskBrush(layerState.AlphaMaskBrush, out alphaMaskBrush);
                if (result < 0)
                {
                    return result;
                }
            }

            if (needsFixup)
            {
                result = BeginLayerInternal(
                    new Direct3D9LayerBeginState(layerBounds, HasAlphaMaskBrush: false),
                    operations.GetPartialCaptureRects,
                    operations.CaptureTarget,
                    operations.ClearTargetToTransparent,
                    out sourceBitmap);
                if (result < 0)
                {
                    return result;
                }
            }

            _layerStack.Add(new Direct3D9LayerFrame(
                layerBounds,
                previousBounds,
                IntersectSurfaceRects(layerBounds, layerState.CurrentClip),
                layerState.Alpha,
                layerState.AntiAliasMode,
                geometricMask,
                alphaMaskBrush,
                sourceBitmap,
                savedClearTypeHint,
                operations));
            _bounds = layerBounds;
            geometricMask = 0;
            alphaMaskBrush = 0;
            sourceBitmap = 0;
            pushed = true;
            return 0;
        }
        finally
        {
            if (sourceBitmap != 0)
            {
                operations.ReleaseSourceBitmap(sourceBitmap);
            }

            if (alphaMaskBrush != 0)
            {
                operations.ReleaseAlphaMaskBrush(alphaMaskBrush);
            }

            if (geometricMask != 0)
            {
                operations.ReleaseGeometricMask(geometricMask);
            }

            if (!pushed)
            {
                _bounds = previousBounds;
                _forceClearType = savedClearTypeHint;
            }
        }
    }

    internal int EndLayer()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_layerStack.Count == 0)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        int index = _layerStack.Count - 1;
        Direct3D9LayerFrame frame = _layerStack[index];
        try
        {
            if (frame.SourceBitmap == 0)
            {
                return 0;
            }

            using Direct3D9UseContextGuard useContext = new(_device);
            using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
            if (!IsValid)
            {
                return 0;
            }

            int result = frame.Operations.RestoreParentTargetState(frame.CurrentClip);
            if (result >= 0)
            {
                result = (frame.GeometricMask != 0 || frame.AlphaMaskBrush != 0)
                    && frame.Operations.MaskOperations is not null
                    ? CompositeLayerMask(frame, frame.Operations.MaskOperations)
                    : frame.Operations.Composite(new Direct3D9LayerCompositeState(
                        frame.SourceBitmap,
                        frame.LayerBounds,
                        frame.Alpha,
                        frame.AntiAliasMode,
                        frame.GeometricMask,
                        frame.AlphaMaskBrush,
                        HasAlpha(),
                        RequiresSourceUnder: HasAlpha()));
            }

            return NormalizeNoRenderResult(result);
        }
        finally
        {
            _layerStack.RemoveAt(index);
            _bounds = frame.PreviousBounds;
            _forceClearType = frame.SavedClearTypeHint;
            frame.Release();
        }
    }

    private int CompositeLayerMask(Direct3D9LayerFrame frame, Direct3D9LayerMaskOperations operations)
    {
        Direct3D9LayerMaskShape? maskShape = null;
        int result = Direct3D9Factory.SuccessHResult;
        if (frame.GeometricMask != 0)
        {
            result = operations.CreateMaskShape(frame.GeometricMask, out maskShape);
            if (result < 0)
            {
                return result;
            }

            if (maskShape is null)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }
        }

        using (maskShape)
        {
            bool targetHasAlpha = HasAlpha();
            bool needsConstantAlphaFixup = frame.Alpha < 1;
            MilCompositingMode complementedMaskMode = targetHasAlpha
                ? MilCompositingMode.SourceInverseAlphaMultiply
                : MilCompositingMode.SourceOverNonPremultiplied;
            MilCompositingMode regularMaskMode = targetHasAlpha
                ? MilCompositingMode.SourceAlphaMultiply
                : MilCompositingMode.SourceInverseAlphaOverNonPremultiplied;

            if (maskShape is null)
            {
                result = operations.CreateBoundsGeometryGenerator(null, frame.LayerBounds, out Direct3D9PathGeometryGenerator? boundsGeometry);
                if (result == Direct3D9Factory.EmptyFillHResult)
                {
                    return Direct3D9Factory.SuccessHResult;
                }

                if (result < 0)
                {
                    return result;
                }

                if (boundsGeometry is null)
                {
                    return Direct3D9Factory.InternalErrorHResult;
                }

                using (boundsGeometry)
                {
                    result = FillLayerPath(
                        frame,
                        operations,
                        boundsGeometry,
                        frame.Alpha < 1 ? frame.Alpha : null,
                        regularMaskMode,
                        null,
                        needInside: true,
                        targetHasAlpha);
                    if (result < 0 || !targetHasAlpha)
                    {
                        return result;
                    }
                }

                result = operations.CreateBoundsGeometryGenerator(null, frame.LayerBounds, out Direct3D9PathGeometryGenerator? alphaMaskSourceUnderGeometry);
                if (result == Direct3D9Factory.EmptyFillHResult)
                {
                    return Direct3D9Factory.SuccessHResult;
                }

                if (result < 0)
                {
                    return result;
                }

                if (alphaMaskSourceUnderGeometry is null)
                {
                    return Direct3D9Factory.InternalErrorHResult;
                }

                using (alphaMaskSourceUnderGeometry)
                {
                    return FillLayerPath(
                        frame,
                        operations,
                        alphaMaskSourceUnderGeometry,
                        null,
                        MilCompositingMode.SourceUnder,
                        null,
                        needInside: true,
                        useOpaqueBlack: false,
                        includeAlphaMask: false);
                }
            }

            if (frame.AntiAliasMode != MilAntiAliasMode.None)
            {
                result = operations.CreateAntialiasedGeometryGenerator(maskShape, frame.LayerBounds, out Direct3D9PathGeometryGenerator? geometryGenerator);
                if (result != Direct3D9Factory.EmptyFillHResult && result < 0)
                {
                    return result;
                }

                if (result != Direct3D9Factory.EmptyFillHResult)
                {
                    if (geometryGenerator is null)
                    {
                        return Direct3D9Factory.InternalErrorHResult;
                    }

                    using (geometryGenerator)
                    {
                        result = operations.CreateAlphaScaleEffect(frame.Alpha, out Direct3D9LayerEffectList? effectList);
                        if (result < 0)
                        {
                            return result;
                        }

                        if (effectList is null)
                        {
                            return Direct3D9Factory.InternalErrorHResult;
                        }

                        using (effectList)
                        {
                            result = FillLayerPath(
                                frame,
                                operations,
                                geometryGenerator,
                                effectList.Handle,
                                regularMaskMode,
                                frame.LayerBounds,
                                needsConstantAlphaFixup,
                                targetHasAlpha);
                            if (result < 0)
                            {
                                return result;
                            }
                        }
                    }
                }

                needsConstantAlphaFixup = false;
            }
            else
            {
                result = operations.CombineMaskWithBounds(frame.LayerBounds, maskShape, out Direct3D9LayerMaskShape? complementedShape);
                if (result != Direct3D9Factory.EmptyFillHResult && result < 0)
                {
                    return result;
                }

                if (result != Direct3D9Factory.EmptyFillHResult)
                {
                    if (complementedShape is null)
                    {
                        return Direct3D9Factory.InternalErrorHResult;
                    }

                    using (complementedShape)
                    {
                        result = operations.CreateAliasedGeometryGenerator(complementedShape, frame.LayerBounds, out Direct3D9PathGeometryGenerator? geometryGenerator);
                        if (result != Direct3D9Factory.EmptyFillHResult && result < 0)
                        {
                            return result;
                        }

                        if (result != Direct3D9Factory.EmptyFillHResult)
                        {
                            if (geometryGenerator is null)
                            {
                                return Direct3D9Factory.InternalErrorHResult;
                            }

                            using (geometryGenerator)
                            {
                                result = FillLayerPath(
                                    frame,
                                    operations,
                                    geometryGenerator,
                                    0,
                                    complementedMaskMode,
                                    null,
                                    needInside: true,
                                    useOpaqueBlack: targetHasAlpha);
                                if (result < 0)
                                {
                                    return result;
                                }
                            }
                        }
                    }
                }
            }

            if (needsConstantAlphaFixup)
            {
                result = operations.CreateBoundsGeometryGenerator(maskShape, frame.LayerBounds, out Direct3D9PathGeometryGenerator? boundsGeometry);
                if (result != Direct3D9Factory.EmptyFillHResult && result < 0)
                {
                    return result;
                }

                if (result != Direct3D9Factory.EmptyFillHResult)
                {
                    if (boundsGeometry is null)
                    {
                        return Direct3D9Factory.InternalErrorHResult;
                    }

                    using (boundsGeometry)
                    {
                        result = operations.CreateAlphaScaleEffect(1 - frame.Alpha, out Direct3D9LayerEffectList? effectList);
                        if (result < 0)
                        {
                            return result;
                        }

                        if (effectList is null)
                        {
                            return Direct3D9Factory.InternalErrorHResult;
                        }

                        using (effectList)
                        {
                            result = FillLayerPath(
                                frame,
                                operations,
                                boundsGeometry,
                                effectList.Handle,
                                complementedMaskMode,
                                null,
                                needInside: true,
                                useOpaqueBlack: targetHasAlpha);
                            if (result < 0)
                            {
                                return result;
                            }
                        }
                    }
                }
            }

            if (!targetHasAlpha)
            {
                return Direct3D9Factory.SuccessHResult;
            }

            result = operations.CreateBoundsGeometryGenerator(maskShape, frame.LayerBounds, out Direct3D9PathGeometryGenerator? sourceUnderGeometry);
            if (result == Direct3D9Factory.EmptyFillHResult)
            {
                return Direct3D9Factory.SuccessHResult;
            }

            if (result < 0)
            {
                return result;
            }

            if (sourceUnderGeometry is null)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }

            using (sourceUnderGeometry)
            {
                return FillLayerPath(
                    frame,
                    operations,
                    sourceUnderGeometry,
                    0,
                    MilCompositingMode.SourceUnder,
                    null,
                    needInside: true,
                    useOpaqueBlack: false);
            }
        }
    }

    private static int FillLayerPath(
        Direct3D9LayerFrame frame,
        Direct3D9LayerMaskOperations operations,
        Direct3D9PathGeometryGenerator geometryGenerator,
        nint effects,
        MilCompositingMode compositingMode,
        Direct3D9SurfaceRect? complementBounds,
        bool needInside,
        bool useOpaqueBlack)
    {
        if (frame.AlphaMaskBrush == 0 || operations.FillEffectPath is null)
        {
            return FillLayerPathLegacy(
                frame,
                operations,
                geometryGenerator,
                effects,
                compositingMode,
                complementBounds,
                needInside,
                useOpaqueBlack);
        }

        float? alpha = effects == 0 ? null : frame.Alpha;
        return FillLayerPath(
            frame,
            operations,
            geometryGenerator,
            alpha,
            compositingMode,
            complementBounds,
            needInside,
            useOpaqueBlack);
    }

    private static int FillLayerPath(
        Direct3D9LayerFrame frame,
        Direct3D9LayerMaskOperations operations,
        Direct3D9PathGeometryGenerator geometryGenerator,
        float? alpha,
        MilCompositingMode compositingMode,
        Direct3D9SurfaceRect? complementBounds,
        bool needInside,
        bool useOpaqueBlack,
        bool includeAlphaMask = true)
    {
        if (operations.FillEffectPath is null)
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        using Direct3D9EffectList effectList = new();
        if (includeAlphaMask && frame.AlphaMaskBrush != 0)
        {
            if (operations.RetainAlphaMaskResource is null || operations.ReleaseAlphaMaskResource is null)
            {
                return Direct3D9Factory.UnsupportedOperationHResult;
            }

            int result = effectList.AddAlphaMask(
                new Direct3D9AlphaMaskParameters(Matrix4x4.Identity),
                frame.AlphaMaskBrush,
                operations.RetainAlphaMaskResource,
                operations.ReleaseAlphaMaskResource);
            if (result < 0)
            {
                return result;
            }
        }

        if (alpha is float alphaValue)
        {
            int result = effectList.AddAlphaScale(alphaValue);
            if (result < 0)
            {
                return result;
            }
        }

        int brushResult = operations.CreateBrush(frame.SourceBitmap, useOpaqueBlack, out Direct3D9PathHardwareBrush? brush);
        if (brushResult < 0)
        {
            return brushResult;
        }

        if (brush is null)
        {
            return Direct3D9Factory.InternalErrorHResult;
        }

        using (brush)
        {
            Direct3D9PathBrushContext context = CreateLayerBrushContext(frame);
            return NormalizeNoRenderResult(operations.FillEffectPath(
                compositingMode,
                geometryGenerator,
                brush,
                effectList.Entries.Count == 0 ? null : effectList,
                context,
                complementBounds,
                needInside));
        }
    }

    private static int FillLayerPathLegacy(
        Direct3D9LayerFrame frame,
        Direct3D9LayerMaskOperations operations,
        Direct3D9PathGeometryGenerator geometryGenerator,
        nint effects,
        MilCompositingMode compositingMode,
        Direct3D9SurfaceRect? complementBounds,
        bool needInside,
        bool useOpaqueBlack)
    {
        int result = operations.CreateBrush(frame.SourceBitmap, useOpaqueBlack, out Direct3D9PathHardwareBrush? brush);
        if (result < 0)
        {
            return result;
        }

        if (brush is null)
        {
            return Direct3D9Factory.InternalErrorHResult;
        }

        using (brush)
        {
            return NormalizeNoRenderResult(operations.FillPath(
                compositingMode,
                geometryGenerator,
                brush,
                effects,
                CreateLayerBrushContext(frame),
                complementBounds,
                needInside));
        }
    }

    private static Direct3D9PathBrushContext CreateLayerBrushContext(Direct3D9LayerFrame frame) => new(
        Matrix4x4.Identity,
        frame.LayerBounds,
        new MilRectF(frame.LayerBounds.Left, frame.LayerBounds.Top, frame.LayerBounds.Right, frame.LayerBounds.Bottom),
        CanFallback: false);

    internal void EndAndIgnoreAllLayers()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        EndAndIgnoreAllLayersCore();
    }

    internal int BeginLayerInternal(
        Direct3D9LayerBeginState layerState,
        Direct3D9GetPartialLayerCaptureRects getPartialLayerCaptureRects,
        Direct3D9CaptureLayerTarget captureLayerTarget,
        out nint sourceBitmap)
    {
        return BeginLayerInternal(
            layerState,
            getPartialLayerCaptureRects,
            captureLayerTarget,
            layerBounds => _device.ColorFill(Get2DRenderTargetSurface().Surface, layerBounds, 0),
            out sourceBitmap);
    }

    internal int BeginLayerInternal(
        Direct3D9LayerBeginState layerState,
        Direct3D9GetPartialLayerCaptureRects getPartialLayerCaptureRects,
        Direct3D9CaptureLayerTarget captureLayerTarget,
        Func<Direct3D9SurfaceRect, int> clearLayerToTransparent,
        out nint sourceBitmap)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(getPartialLayerCaptureRects);
        ArgumentNullException.ThrowIfNull(captureLayerTarget);
        ArgumentNullException.ThrowIfNull(clearLayerToTransparent);
        sourceBitmap = 0;

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        using Direct3D9UseContextGuard useContext = new(_device);

        if (!IsValid)
        {
            return 0;
        }

        if (layerState.HasAlphaMaskBrush)
        {
            return Direct3D9Factory.NotImplementedHResult;
        }

        bool hasAlpha = HasAlpha();
        bool copyEntireLayer = true;
        IReadOnlyList<Direct3D9SurfaceRect>? copyRects = null;
        if (!hasAlpha && getPartialLayerCaptureRects(out IReadOnlyList<Direct3D9SurfaceRect> partialCopyRects))
        {
            ArgumentNullException.ThrowIfNull(partialCopyRects);
            copyEntireLayer = false;
            copyRects = partialCopyRects;
        }

        if (copyEntireLayer || copyRects!.Count > 0)
        {
            int result = captureLayerTarget(
                layerState.LayerBounds,
                copyEntireLayer ? null : copyRects,
                out sourceBitmap);
            if (result < 0)
            {
                sourceBitmap = 0;
                return result;
            }

            if (hasAlpha)
            {
                result = clearLayerToTransparent(layerState.LayerBounds);
                if (result < 0)
                {
                    return result;
                }
            }
        }

        return 0;
    }

    internal int EndLayer(
        Direct3D9LayerEndState layerState,
        Direct3D9CompositeSavedLayer compositeSavedLayer)
    {
        ArgumentNullException.ThrowIfNull(compositeSavedLayer);
        return EndLayer(
            layerState,
            () => EndLayerInternal(
                layerState with { CurrentClip = layerState.LayerBounds },
                compositeSavedLayer),
            Direct3D9Factory.Release);
    }

    internal int EndLayer(
        Direct3D9LayerEndState layerState,
        Func<int> endLayerInternal,
        Action<nint> releaseSourceBitmap)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(endLayerInternal);
        ArgumentNullException.ThrowIfNull(releaseSourceBitmap);

        try
        {
            int result = layerState.SourceBitmap == 0 ? 0 : endLayerInternal();
            return NormalizeNoRenderResult(result);
        }
        finally
        {
            _bounds = layerState.PreviousBounds;
            if (HasAlpha())
            {
                _forceClearType = layerState.SavedClearTypeHint;
            }

            if (layerState.SourceBitmap != 0)
            {
                releaseSourceBitmap(layerState.SourceBitmap);
            }
        }
    }

    internal int EndLayerInternal(
        Direct3D9LayerEndState layerState,
        Direct3D9CompositeSavedLayer compositeSavedLayer)
    {
        return EndLayerInternal(
            layerState,
            SetAsRenderTarget,
            layerBounds => _device.SetClipRect(layerBounds),
            Ensure2DState,
            compositeSavedLayer);
    }

    internal int EndLayerInternal(
        Direct3D9LayerEndState layerState,
        Func<int> setAsRenderTarget,
        Func<Direct3D9SurfaceRect, int> setClipRect,
        Func<int> ensure2DState,
        Direct3D9CompositeSavedLayer compositeSavedLayer)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(setAsRenderTarget);
        ArgumentNullException.ThrowIfNull(setClipRect);
        ArgumentNullException.ThrowIfNull(ensure2DState);
        ArgumentNullException.ThrowIfNull(compositeSavedLayer);
        ArgumentOutOfRangeException.ThrowIfZero(layerState.SourceBitmap);

        using Direct3D9UseContextGuard useContext = new(_device);
        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);

        if (!IsValid)
        {
            return 0;
        }

        int result = setAsRenderTarget();
        if (result < 0)
        {
            return result;
        }

        result = setClipRect(layerState.CurrentClip);
        if (result < 0)
        {
            return result;
        }

        result = ensure2DState();
        if (result < 0 || !HasAlpha())
        {
            return result;
        }

        return compositeSavedLayer(
            layerState.SourceBitmap,
            layerState.LayerBounds,
            MilCompositingMode.SourceUnder);
    }

    private static bool GlyphBoundsIntersectTarget(MilRectF bounds, Direct3D9SurfaceRect targetBounds) =>
        IsEmpty(targetBounds)
        || (bounds.Left < targetBounds.Right
            && bounds.Right > targetBounds.Left
            && bounds.Top < targetBounds.Bottom
            && bounds.Bottom > targetBounds.Top);

    internal int DrawGlyphs(Func<int> drawGlyphs) => ExecuteDisplayDrawing(drawGlyphs);

    internal int ProductionDrawGlyphs(
        Direct3D9GlyphDrawState drawState,
        Direct3D9GlyphRun glyphRun,
        Direct3D9GlyphRunDrawOperations operations)
    {
        ArgumentNullException.ThrowIfNull(glyphRun);
        ArgumentNullException.ThrowIfNull(operations);

        int validationResult = glyphRun.Validate();
        if (validationResult < 0)
        {
            return validationResult;
        }

        if (glyphRun.IsEmpty || !GlyphBoundsIntersectTarget(glyphRun.Bounds, _bounds))
        {
            return _isRenderingEnabled ? CompleteDisplayDrawing(0) : 0;
        }

        if (operations.DeviceIdentity == 0
            || !float.IsFinite(operations.ScaleX)
            || !float.IsFinite(operations.ScaleY)
            || operations.ScaleX <= 0
            || operations.ScaleY <= 0
            || !Enum.IsDefined(operations.RecommendedBlendMode))
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        Direct3D9GlyphBlendMode blendMode = operations.RecommendedBlendMode == Direct3D9GlyphBlendMode.ClearType
            && drawState.TargetSupportsClearType
            ? Direct3D9GlyphBlendMode.ClearType
            : Direct3D9GlyphBlendMode.Grayscale;
        Direct3D9GlyphBankKey key = new(
            operations.DeviceIdentity,
            operations.DisplayIndex,
            glyphRun.CacheKey,
            blendMode,
            glyphRun.UseSubpixelPositioning,
            operations.ScaleX,
            operations.ScaleY);

        return DrawGlyphs(
            drawState,
            operations.EnsureHardwareBrushRealization,
            operations.EnsureState,
            supportsClearType =>
            {
                int result = operations.GlyphBank.GetOrCreate(
                    glyphRun,
                    key,
                    operations.CreateRealization,
                    out Direct3D9GlyphRealization? realization);
                if (result < 0)
                {
                    return result;
                }

                if (realization is null)
                {
                    return Direct3D9Factory.InternalErrorHResult;
                }

                using Direct3D9GlyphRunPainter painter = new(
                    glyphRun,
                    realization,
                    blendMode == Direct3D9GlyphBlendMode.ClearType && supportsClearType,
                    operations.PaintRealization,
                    operations.ReleasePainter);
                return painter.Paint();
            },
            operations.SoftwareRenderer);
    }

    internal int ProductionDrawGlyphs(
        Direct3D9GlyphDrawState drawState,
        Direct3D9ProductionGlyphDrawOperations operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        if (!operations.HasGlyphs)
        {
            return _isRenderingEnabled ? CompleteDisplayDrawing(0) : 0;
        }

        return DrawGlyphs(
            drawState,
            operations.EnsureHardwareBrushRealization,
            operations.EnsureState,
            supportsClearType =>
            {
                int result = operations.CreateHardwareRenderer(supportsClearType, out Direct3D9ProductionGlyphRenderer? renderer);
                if (result < 0)
                {
                    renderer?.Dispose();
                    return result;
                }

                if (renderer is null)
                {
                    return Direct3D9Factory.InternalErrorHResult;
                }

                using (renderer)
                {
                    return renderer.Paint();
                }
            },
            operations.SoftwareRenderer);
    }

    internal int DrawGlyphs(
        Func<int> ensureHardwareBrushRealization,
        Func<int> ensureState,
        Func<bool, int> drawHardwareGlyphs,
        Func<bool, int, int> drawSoftwareGlyphs)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return DrawGlyphs(
            _device.CanDrawText,
            ensureHardwareBrushRealization,
            ensureState,
            drawHardwareGlyphs,
            drawSoftwareGlyphs);
    }

    internal int DrawGlyphs(
        bool canDrawText,
        Func<int> ensureHardwareBrushRealization,
        Func<int> ensureState,
        Func<bool, int> drawHardwareGlyphs,
        Func<bool, int, int> drawSoftwareGlyphs)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return DrawGlyphs(
            new Direct3D9GlyphDrawState(
                TargetSupportsClearType: _forceClearType || !HasAlpha(),
                CanDrawText: canDrawText),
            ensureHardwareBrushRealization,
            ensureState,
            drawHardwareGlyphs,
            drawSoftwareGlyphs);
    }

    internal int DrawGlyphs(
        Direct3D9GlyphDrawState drawState,
        Func<int> ensureHardwareBrushRealization,
        Func<int> ensureState,
        Func<bool, int> drawHardwareGlyphs,
        Func<bool, int, int> drawSoftwareGlyphs)
    {
        ArgumentNullException.ThrowIfNull(ensureHardwareBrushRealization);

        return DrawGlyphs(
            drawState,
            (out Direct3D9RealizedGlyphBrushState realizedBrushState) =>
            {
                int result = ensureHardwareBrushRealization();
                realizedBrushState = new Direct3D9RealizedGlyphBrushState(HasBrush: result >= 0);
                return result;
            },
            ensureState,
            drawHardwareGlyphs,
            drawSoftwareGlyphs);
    }

    internal int DrawGlyphs(
        Direct3D9GlyphDrawState drawState,
        Direct3D9EnsureGlyphBrushRealization ensureHardwareBrushRealization,
        Func<int> ensureState,
        Func<bool, int> drawHardwareGlyphs,
        Func<bool, int, int> drawSoftwareGlyphs)
    {
        if (!_isRenderingEnabled)
        {
            return DrawGlyphsCore(
                drawState,
                ensureHardwareBrushRealization,
                ensureState,
                drawHardwareGlyphs,
                drawSoftwareGlyphs);
        }

        return CompleteDisplayDrawing(DrawGlyphsCore(
            drawState,
            ensureHardwareBrushRealization,
            ensureState,
            drawHardwareGlyphs,
            drawSoftwareGlyphs));
    }

    private int DrawGlyphsCore(
        Direct3D9GlyphDrawState drawState,
        Direct3D9EnsureGlyphBrushRealization ensureHardwareBrushRealization,
        Func<int> ensureState,
        Func<bool, int> drawHardwareGlyphs,
        Func<bool, int, int> drawSoftwareGlyphs)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(ensureHardwareBrushRealization);
        ArgumentNullException.ThrowIfNull(ensureState);
        ArgumentNullException.ThrowIfNull(drawHardwareGlyphs);
        ArgumentNullException.ThrowIfNull(drawSoftwareGlyphs);
        if (!_isRenderingEnabled)
        {
            return 0;
        }

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        using Direct3D9UseContextGuard useContext = new(_device);

        if ((_swapChain is not null || _renderTargetSurface is not null) && !IsValid)
        {
            return 0;
        }

        bool attemptHardwareText = drawState.CanDrawText;
        if (attemptHardwareText
            && ((_device.SupportsTextureCapability((uint) D3D9.PtexturecapsPow2)
                    && drawState.RealizedBrushMayNeedNonPowerOfTwoTiling)
                || (drawState.RealizedBrushWillHaveSourceClip
                    && (!_device.SupportsBorderColor || !drawState.RealizedBrushSourceClipMayBeEntireSource))))
        {
            attemptHardwareText = false;
        }

        if (attemptHardwareText)
        {
            int realizationResult = ensureHardwareBrushRealization(out Direct3D9RealizedGlyphBrushState realizedBrushState);
            if (realizationResult < 0)
            {
                if (realizationResult is Direct3D9Factory.DeviceCannotRenderTextHResult or Direct3D9Factory.NotImplementedHResult)
                {
                    return NormalizeNoRenderResult(drawSoftwareGlyphs(drawState.TargetSupportsClearType, realizationResult));
                }

                return NormalizeNoRenderResult(realizationResult);
            }

            if (!realizedBrushState.HasBrush)
            {
                return 0;
            }

            if (realizedBrushState.IsBitmapBrush
                && realizedBrushState.HasSourceClip
                && !realizedBrushState.SourceClipIsEntireSource)
            {
                attemptHardwareText = false;
            }
        }

        int result = ensureState();
        if (result == Direct3D9Factory.ClippedToEmptyHResult)
        {
            return 0;
        }

        if (result < 0)
        {
            if (result is Direct3D9Factory.DeviceCannotRenderTextHResult or Direct3D9Factory.NotImplementedHResult)
            {
                result = drawSoftwareGlyphs(drawState.TargetSupportsClearType, result);
            }

            return NormalizeNoRenderResult(result);
        }

        result = attemptHardwareText
            ? drawHardwareGlyphs(drawState.TargetSupportsClearType)
            : Direct3D9Factory.DeviceCannotRenderTextHResult;
        if (result is Direct3D9Factory.DeviceCannotRenderTextHResult or Direct3D9Factory.NotImplementedHResult)
        {
            result = drawSoftwareGlyphs(drawState.TargetSupportsClearType, result);
        }

        return NormalizeNoRenderResult(result);
    }

    internal int DrawGlyphs(
        Direct3D9GlyphDrawState drawState,
        Direct3D9EnsureGlyphBrushRealization ensureHardwareBrushRealization,
        Func<int> ensureState,
        Func<bool, int> drawHardwareGlyphs,
        Direct3D9SoftwareGlyphRenderer softwareRenderer)
    {
        ArgumentNullException.ThrowIfNull(softwareRenderer);
        ArgumentNullException.ThrowIfNull(softwareRenderer.EnsureBrushRealization);
        ArgumentNullException.ThrowIfNull(softwareRenderer.GetSoftwareFallback);
        ArgumentNullException.ThrowIfNull(softwareRenderer.DrawGlyphs);

        return DrawGlyphs(
            drawState,
            ensureHardwareBrushRealization,
            ensureState,
            drawHardwareGlyphs,
            (supportsClearType, reason) => SoftwareDrawGlyphs(softwareRenderer, supportsClearType, reason));
    }

    private int SoftwareDrawGlyphs(
        Direct3D9SoftwareGlyphRenderer softwareRenderer,
        bool targetSupportsClearType,
        int reasonForFallback)
    {
        using Direct3D9UseContextGuard useContext = new(_device);

        int result = softwareRenderer.EnsureBrushRealization(out Direct3D9RealizedSoftwareGlyphBrushState realizedBrushState);
        if (result < 0 || !realizedBrushState.HasBrush)
        {
            return result;
        }

        result = softwareRenderer.GetSoftwareFallback(reasonForFallback);
        if (result < 0)
        {
            return result;
        }

        return softwareRenderer.DrawGlyphs(targetSupportsClearType, realizedBrushState.EffectAlpha);
    }

    internal int DrawVideo(Func<int> drawVideo) => ExecuteDisplayDrawing(drawVideo);

    internal int ProductionDrawVideo(
        Direct3D9VideoRenderState renderState,
        Direct3D9VideoSurfaceRenderer? surfaceRenderer,
        nint bitmapSource,
        Direct3D9ProductionVideoDrawOperations operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        return DrawVideo(
            renderState,
            surfaceRenderer,
            bitmapSource,
            currentBitmapSource => ProductionDrawBitmap(
                operations.BitmapDrawState,
                currentBitmapSource,
                operations.Effects,
                operations.BitmapOperations));
    }

    internal int DrawVideo(
        Direct3D9VideoRenderState renderState,
        Direct3D9VideoSurfaceRenderer? surfaceRenderer,
        nint bitmapSource,
        Direct3D9BitmapDrawState drawState,
        nint effects,
        Direct3D9GetScratchBitmapBrush getScratchBitmapBrush,
        Direct3D9SetScratchBitmapBrush setScratchBitmapBrush,
        Action<nint> clearScratchBitmapBrush,
        Func<nint, Direct3D9ImmediateBrushRealizer> createBrushRealizer,
        Direct3D9CreateBitmapShape createBitmapShape,
        Func<int> ensureState,
        Direct3D9ClipToSafeDeviceBounds clipToSafeDeviceBounds,
        Direct3D9FillPathWithBrush fillPathWithBrush,
        Direct3D9SoftwareFillBitmapPath softwareFillPath)
    {
        return DrawVideo(
            renderState,
            surfaceRenderer,
            bitmapSource,
            currentBitmapSource => DrawBitmap(
                drawState,
                currentBitmapSource,
                effects,
                getScratchBitmapBrush,
                setScratchBitmapBrush,
                clearScratchBitmapBrush,
                createBrushRealizer,
                createBitmapShape,
                ensureState,
                clipToSafeDeviceBounds,
                fillPathWithBrush,
                softwareFillPath));
    }

    internal int DrawVideo(
        Direct3D9VideoRenderState renderState,
        Direct3D9VideoSurfaceRenderer? surfaceRenderer,
        nint bitmapSource,
        Func<nint, int> drawBitmap)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(renderState);
        ArgumentNullException.ThrowIfNull(drawBitmap);
        if (!_isRenderingEnabled)
        {
            return 0;
        }

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        using Direct3D9UseContextGuard useContext = new(_device);

        if ((_swapChain is not null || _renderTargetSurface is not null) && !IsValid)
        {
            return CompleteDisplayDrawing(0);
        }

        bool restorePrefilter = renderState.PrefilterEnabled;
        bool endRender = false;
        nint currentBitmapSource = 0;
        int result = 0;
        try
        {
            if (surfaceRenderer is not null)
            {
                ArgumentNullException.ThrowIfNull(surfaceRenderer.BeginRender);
                ArgumentNullException.ThrowIfNull(surfaceRenderer.EndRender);
                result = _device.DrawVideoToSurface(surfaceRenderer.BeginRender, out currentBitmapSource);
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
                return CompleteDisplayDrawing(0);
            }

            renderState.PrefilterEnabled = false;
            return CompleteDisplayDrawing(drawBitmap(currentBitmapSource));
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

    private static int GetBitmapSourceBounds(nint bitmapSource, out MilRectF bounds)
    {
        uint width = 0;
        uint height = 0;
        void*** instance = (void***) bitmapSource;
        void** vtable = *instance;
        delegate* unmanaged[Stdcall]<void***, uint*, uint*, int> getSize =
            (delegate* unmanaged[Stdcall]<void***, uint*, uint*, int>) vtable[3];
        int result = getSize(instance, &width, &height);
        bounds = result >= 0
            ? new MilRectF(0, 0, width, height)
            : default;
        return result;
    }

    private static int NormalizeMeshResult(int result)
    {
        return result == Direct3D9Factory.NonInvertibleMatrixHResult ? 0 : result;
    }

    private static int NormalizeNoRenderResult(int result)
    {
        return result is Direct3D9Factory.NonInvertibleMatrixHResult
            or Direct3D9Factory.BadNumberHResult
            or Direct3D9Factory.ClippedToEmptyHResult
            ? 0
            : result;
    }

    private int ExecuteDisplayDrawing(Func<int> drawingOperation)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_isRenderingEnabled)
        {
            return 0;
        }

        ArgumentNullException.ThrowIfNull(drawingOperation);
        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        using Direct3D9UseContextGuard useContext = new(_device);
        return CompleteDisplayDrawing(drawingOperation());
    }

    private int CompleteDisplayDrawing(int result)
    {
        if (result >= 0)
        {
            _hasValidContents = true;
        }

        return result;
    }

    internal int Present()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_layerStack.Count > 0)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        try
        {
            return PresentCore();
        }
        finally
        {
            ClearInvalidatedRectsCore();
        }
    }

    internal int Present(Direct3D9SurfaceRect inputRect)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_layerStack.Count > 0)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        try
        {
            if (!_isRenderingEnabled)
            {
                return _displayInvalidHResult;
            }

            if ((_initializationFlags & Direct3D9RenderTargetInitializationFlags.DisableDirtyRectangles) != 0)
            {
                _hasEmptyInvalidation = true;
            }

            if (!ShouldPresent(inputRect, out Direct3D9SurfaceRect presentRect, out IReadOnlyList<Direct3D9SurfaceRect>? dirtyRegion))
            {
                return 0;
            }

            return PresentCore(presentRect, dirtyRegion);
        }
        finally
        {
            ClearInvalidatedRectsCore();
        }
    }

    internal int InvalidateRect(Direct3D9SurfaceRect rect)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_isRenderingEnabled)
        {
            return 0;
        }

        if (rect.Left >= rect.Right || rect.Top >= rect.Bottom)
        {
            _hasEmptyInvalidation = true;
            return 0;
        }

        AddInvalidatedRect(rect);
        return 0;
    }

    private int PresentCore()
    {
        return PresentCore(null, null);
    }

    private int PresentCore(Direct3D9SurfaceRect? presentRect, IReadOnlyList<Direct3D9SurfaceRect>? dirtyRegion)
    {
        if (!_isRenderingEnabled)
        {
            return _displayInvalidHResult;
        }

        if (_swapChain is null)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        Direct3D9PresentRequest request = _presentParameters.SwapEffect == Swapeffect.Copy
            ? new Direct3D9PresentRequest(presentRect, presentRect, dirtyRegion, _presentFlags, _presentParameters.HDeviceWindow)
            : new Direct3D9PresentRequest(null, null, null, _presentFlags, _presentParameters.HDeviceWindow);
        Direct3D9DeviceState state = _device.Present(_swapChain, request);
        int result = state.HResult;
        if (state.PresentProcessed && _presentParameters.SwapEffect == Swapeffect.Discard)
        {
            _hasValidContents = false;
        }

        if (state.Kind == Direct3D9DeviceStateKind.ModeChanged)
        {
            result = Direct3D9Factory.DeviceLostHResult;
        }

        if (result < 0)
        {
            result = _device.HandlePresentFailure(result).HResult;
            if (result == Direct3D9Factory.DisplayStateInvalidHResult)
            {
                _displayInvalidHResult = result;
            }

            _isRenderingEnabled = false;
        }

        return result;
    }

    private bool ShouldPresent(
        Direct3D9SurfaceRect inputRect,
        out Direct3D9SurfaceRect resultRect,
        out IReadOnlyList<Direct3D9SurfaceRect>? dirtyRegion)
    {
        resultRect = inputRect;
        dirtyRegion = null;
        if (inputRect.Left >= inputRect.Right || inputRect.Top >= inputRect.Bottom)
        {
            return false;
        }
        if (_hasEmptyInvalidation)
        {
            return true;
        }

        List<Direct3D9SurfaceRect> intersections = [];
        foreach (Direct3D9SurfaceRect invalidatedRect in _invalidatedRects)
        {
            Direct3D9SurfaceRect intersection = new(
                Math.Max(inputRect.Left, invalidatedRect.Left),
                Math.Max(inputRect.Top, invalidatedRect.Top),
                Math.Min(inputRect.Right, invalidatedRect.Right),
                Math.Min(inputRect.Bottom, invalidatedRect.Bottom));
            if (intersection.Left < intersection.Right && intersection.Top < intersection.Bottom)
            {
                intersections.Add(intersection);
            }
        }
        if (intersections.Count == 0)
        {
            return false;
        }

        Direct3D9SurfaceRect bounds = GetBounds(intersections);
        long regionArea = 0;
        foreach (Direct3D9SurfaceRect intersection in intersections)
        {
            regionArea += (long) (intersection.Right - intersection.Left) * (intersection.Bottom - intersection.Top);
        }
        long boundsArea = (long) (bounds.Right - bounds.Left) * (bounds.Bottom - bounds.Top);
        if (regionArea == boundsArea)
        {
            resultRect = bounds;
        }
        else
        {
            dirtyRegion = intersections;
        }

        return true;
    }

    private void AddInvalidatedRect(Direct3D9SurfaceRect rect)
    {
        List<Direct3D9SurfaceRect> pending = [rect];
        foreach (Direct3D9SurfaceRect existing in _invalidatedRects)
        {
            List<Direct3D9SurfaceRect> next = [];
            foreach (Direct3D9SurfaceRect candidate in pending)
            {
                Subtract(candidate, existing, next);
            }
            pending = next;
            if (pending.Count == 0)
            {
                return;
            }
        }

        _invalidatedRects.AddRange(pending);
    }

    private static void Subtract(Direct3D9SurfaceRect source, Direct3D9SurfaceRect excluded, List<Direct3D9SurfaceRect> result)
    {
        int left = Math.Max(source.Left, excluded.Left);
        int top = Math.Max(source.Top, excluded.Top);
        int right = Math.Min(source.Right, excluded.Right);
        int bottom = Math.Min(source.Bottom, excluded.Bottom);
        if (left >= right || top >= bottom)
        {
            result.Add(source);
            return;
        }

        AddIfNotEmpty(result, new Direct3D9SurfaceRect(source.Left, source.Top, source.Right, top));
        AddIfNotEmpty(result, new Direct3D9SurfaceRect(source.Left, bottom, source.Right, source.Bottom));
        AddIfNotEmpty(result, new Direct3D9SurfaceRect(source.Left, top, left, bottom));
        AddIfNotEmpty(result, new Direct3D9SurfaceRect(right, top, source.Right, bottom));
    }

    private static Direct3D9SurfaceRect IntersectSurfaceRects(
        Direct3D9SurfaceRect first,
        Direct3D9SurfaceRect second)
    {
        Direct3D9SurfaceRect intersection = new(
            Math.Max(first.Left, second.Left),
            Math.Max(first.Top, second.Top),
            Math.Min(first.Right, second.Right),
            Math.Min(first.Bottom, second.Bottom));
        return IsEmpty(intersection) ? default : intersection;
    }

    private static bool IsEmpty(Direct3D9SurfaceRect rectangle) =>
        rectangle.Left >= rectangle.Right || rectangle.Top >= rectangle.Bottom;

    private static void AddIfNotEmpty(List<Direct3D9SurfaceRect> rectangles, Direct3D9SurfaceRect rectangle)
    {
        if (rectangle.Left < rectangle.Right && rectangle.Top < rectangle.Bottom)
        {
            rectangles.Add(rectangle);
        }
    }

    private static Direct3D9SurfaceRect GetBounds(List<Direct3D9SurfaceRect> rectangles)
    {
        Direct3D9SurfaceRect bounds = rectangles[0];
        for (int index = 1; index < rectangles.Count; index++)
        {
            Direct3D9SurfaceRect rectangle = rectangles[index];
            bounds = new Direct3D9SurfaceRect(
                Math.Min(bounds.Left, rectangle.Left),
                Math.Min(bounds.Top, rectangle.Top),
                Math.Max(bounds.Right, rectangle.Right),
                Math.Max(bounds.Bottom, rectangle.Bottom));
        }
        return bounds;
    }

    internal int ClearInvalidatedRects()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ClearInvalidatedRectsCore();
        return 0;
    }

    private void ClearInvalidatedRectsCore()
    {
        _invalidatedRects.Clear();
        _hasEmptyInvalidation = false;
    }

    internal int Resize(uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);

        EndAndIgnoreAllLayersCore();
        _hasValidContents = false;

        if (_ownsRenderTargetSurface)
        {
            _renderTargetSurface?.Dispose();
        }
        _renderTargetSurface = null;
        _ownsRenderTargetSurface = false;
        _activeRenderTargetSurfaceFor3D = null;
        ReleaseIntermediateMultisampleSurfaceWhenTooLarge(width, height);
        _swapChain?.Dispose();
        _swapChain = null;

        if (_initializationHResult < 0)
        {
            _isRenderingEnabled = false;
            return _initializationHResult;
        }

        if (width == 0 || height == 0)
        {
            _isRenderingEnabled = false;
            return 0;
        }

        _presentParameters.BackBufferWidth = width;
        _presentParameters.BackBufferHeight = height;
        int result = _device.TryCreateAdditionalSwapChain(_presentParameters, out Direct3D9SwapChain? swapChain);
        if (result >= 0)
        {
            _swapChain = swapChain
                ?? throw new InvalidOperationException("Direct3D additional swap chain creation returned no swap chain.");
            _bounds = new Direct3D9SurfaceRect(0, 0, checked((int) width), checked((int) height));
            _width = width;
            _height = height;
            result = _swapChain.TryGetBackBuffer(0, out _renderTargetSurface);
            _ownsRenderTargetSurface = result >= 0;
            if (result >= 0)
            {
                ClearInvalidatedRectsCore();
            }
        }

        if (result < 0)
        {
            if (result == Direct3D9Factory.DisplayStateInvalidHResult)
            {
                _displayInvalidHResult = result;
            }

            if (_ownsRenderTargetSurface)
            {
                _renderTargetSurface?.Dispose();
            }
            _renderTargetSurface = null;
            _ownsRenderTargetSurface = false;
            _swapChain?.Dispose();
            _swapChain = null;
            _isRenderingEnabled = false;
        }
        else
        {
            _isRenderingEnabled = true;
        }

        return result;
    }

    internal int Begin3D(MilRectF bounds, MilAntiAliasMode antiAliasMode, bool useZBuffer, float z)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_isRenderingEnabled)
        {
            return 0;
        }

        if (_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        _boundsPre3D = _bounds;
        int result = 0;
        if (TryIntersectBoundsWithSurface(bounds, _bounds, antiAliasMode, out _bounds))
        {
            MultisampleType requested = MultisampleType.MultisampleNone;
            if (antiAliasMode != MilAntiAliasMode.None && _device.ShouldAttemptMultisample)
            {
                requested = _device.GetSupportedMultisampleType(_pixelFormat);
            }

            Direct3D9Begin3DResult beginResult;
            using (Direct3D9DeviceEntryGuard deviceEntry = new(_device))
            {
                bool hasRenderTargetResource = _swapChain is not null || _renderTargetSurface is not null;
                if (hasRenderTargetResource && !IsValid)
                {
                    _bounds = default;
                    beginResult = new Direct3D9Begin3DResult(0, requested);
                }
                else
                {
                    beginResult = _begin3DInternal?.Invoke(_bounds, z, useZBuffer, requested)
                        ?? Begin3DInternal(z, useZBuffer, requested);
                }
            }
            result = beginResult.HResult;
            if (result >= 0 && beginResult.MultisampleTypeReceived != requested)
            {
                _device.SetMultisampleFailed();
                _setMultisampleFailed?.Invoke();
            }
        }

        if (result < 0)
        {
            _bounds = _boundsPre3D;
        }
        else
        {
            _in3D = true;
        }

        return result;
    }

    internal int End3D()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_isRenderingEnabled)
        {
            return 0;
        }

        if (!_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        int result = 0;
        try
        {
            if (_bounds.Left < _bounds.Right
                && _bounds.Top < _bounds.Bottom
                && _activeRenderTargetSurfaceFor3D is not null
                && _renderTargetSurface is not null
                && !ReferenceEquals(_activeRenderTargetSurfaceFor3D, _renderTargetSurface))
            {
                using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
                result = _device.StretchRect(
                    _activeRenderTargetSurfaceFor3D,
                    _bounds,
                    _renderTargetSurface,
                    _bounds);
            }
        }
        finally
        {
            _in3D = false;
            _isActiveDepthBufferEnabled = false;
            _bounds = _boundsPre3D;
        }

        return result;
    }

    private Direct3D9Begin3DResult Begin3DInternal(float z, bool useZBuffer, MultisampleType multisampleType)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        Direct3D9Surface renderTarget = _renderTargetSurface
            ?? throw new InvalidOperationException("A render-target surface is required.");

        int result = Setup3DRenderTargetAndDepthState(z, useZBuffer, multisampleType);
        if (result == Direct3D9Factory.OutOfVideoMemoryHResult
            && multisampleType != MultisampleType.MultisampleNone)
        {
            multisampleType = MultisampleType.MultisampleNone;
            _activeRenderTargetSurfaceFor3D = null;
            ReleaseIntermediateMultisampleSurface();
            result = Setup3DRenderTargetAndDepthState(z, useZBuffer, multisampleType);
        }

        if (result >= 0
            && _activeRenderTargetSurfaceFor3D is not null
            && !ReferenceEquals(_activeRenderTargetSurfaceFor3D, renderTarget))
        {
            result = _device.StretchRect(renderTarget, _bounds, _activeRenderTargetSurfaceFor3D, _bounds);
        }

        return new Direct3D9Begin3DResult(result, multisampleType);
    }

    private int Setup3DRenderTargetAndDepthState(float z, bool useZBuffer, MultisampleType multisampleType)
    {
        int result = Ensure3DRenderTarget(multisampleType);
        if (result < 0)
        {
            return result;
        }

        Direct3D9Surface renderTarget = _activeRenderTargetSurfaceFor3D
            ?? throw new InvalidOperationException("A 3D render-target surface is required.");
        result = _device.SetRenderTarget(renderTarget);
        if (result < 0)
        {
            return result;
        }

        result = _device.SetClipRect(_bounds);
        if (result < 0)
        {
            return result;
        }

        _isActiveDepthBufferEnabled = useZBuffer;
        if (!useZBuffer)
        {
            return result;
        }

        SurfaceDesc renderTargetDescription = _device.GetRenderTargetDescription(renderTarget);
        result = EnsureDepthState(renderTargetDescription);
        if (result < 0)
        {
            return result;
        }

        return _device.ClearDepth(z);
    }

    private int Ensure3DRenderTarget(MultisampleType multisampleType)
    {
        Direct3D9Surface renderTarget = _renderTargetSurface
            ?? throw new InvalidOperationException("A render-target surface is required.");
        _activeRenderTargetSurfaceFor3D = renderTarget;
        SurfaceDesc renderTargetDescription = _device.GetRenderTargetDescription(renderTarget);
        if (multisampleType == MultisampleType.MultisampleNone
            || renderTargetDescription.MultiSampleType == multisampleType)
        {
            return 0;
        }

        Direct3D9Surface? intermediate = _intermediateMultisampleSurface;
        if (intermediate is not null)
        {
            SurfaceDesc description = _device.GetRenderTargetDescription(intermediate);
            if (!intermediate.IsValid
                || description.Width < _width
                || description.Height < _height
                || description.MultiSampleType != multisampleType)
            {
                if (ReferenceEquals(intermediate, _intermediateMultisampleSurface))
                {
                    ReleaseIntermediateMultisampleSurface();
                }
                intermediate = null;
            }
        }

        if (intermediate is null)
        {
            int result = _device.TryCreateRenderTarget(
                _width,
                _height,
                renderTargetDescription.Format,
                multisampleType,
                0,
                false,
                out intermediate);
            if (result < 0)
            {
                return result;
            }

            _intermediateMultisampleSurface = intermediate
                ?? throw new InvalidOperationException("Direct3D render-target creation returned no surface.");
        }

        _activeRenderTargetSurfaceFor3D = intermediate;
        return 0;
    }

    private int EnsureDepthState(SurfaceDesc renderTargetDescription)
    {
        Direct3D9Surface? depthStencilSurface = _createdDepthStencilSurface;
        if (depthStencilSurface is not null)
        {
            bool releaseDepthStencilSurface = !depthStencilSurface.IsValid;
            if (!releaseDepthStencilSurface)
            {
                SurfaceDesc depthDescription = _device.GetRenderTargetDescription(depthStencilSurface);
                releaseDepthStencilSurface = depthDescription.Width < renderTargetDescription.Width
                    || depthDescription.Height < renderTargetDescription.Height
                    || depthDescription.MultiSampleType != renderTargetDescription.MultiSampleType;
            }

            if (releaseDepthStencilSurface)
            {
                if (depthStencilSurface.IsValid)
                {
                    _device.ReleaseUseOfDepthStencilBuffer(depthStencilSurface.SurfaceForDeviceCall);
                }

                depthStencilSurface.Dispose();
                _createdDepthStencilSurface = null;
                depthStencilSurface = null;
            }
        }

        if (depthStencilSurface is null)
        {
            int createResult = _device.CreateDepthBuffer(
                renderTargetDescription.Width,
                renderTargetDescription.Height,
                renderTargetDescription.MultiSampleType,
                out depthStencilSurface);
            if (createResult < 0)
            {
                _ = _device.SetDepthStencilSurface(null);
                return createResult;
            }

            _createdDepthStencilSurface = depthStencilSurface
                ?? throw new InvalidOperationException("Direct3D depth-stencil creation returned no surface.");
        }

        int result = _device.SetDepthStencilSurfaceForCurrentRenderTarget(
            depthStencilSurface.SurfaceForDeviceCall,
            renderTargetDescription.Width,
            renderTargetDescription.Height);
        if (result >= 0)
        {
            return result;
        }

        _ = _device.SetDepthStencilSurface(null);
        return result;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        EndAndIgnoreAllLayersCore();
        _isDisposed = true;
        _in3D = false;
        _bounds = _boundsPre3D;
        _activeRenderTargetSurfaceFor3D = null;
        _swapChain?.Dispose();
        _swapChain = null;
        if (_ownsRenderTargetSurface)
        {
            _renderTargetSurface?.Dispose();
        }
        _renderTargetSurface = null;
        _ownsRenderTargetSurface = false;
        ClearInvalidatedRectsCore();
        ReleaseIntermediateMultisampleSurface();
        if (_createdDepthStencilSurface is not null)
        {
            _device.ReleaseUseOfDepthStencilBuffer(_createdDepthStencilSurface.SurfaceForDeviceCall);
            _createdDepthStencilSurface.Dispose();
            _createdDepthStencilSurface = null;
        }
    }

    private void EndAndIgnoreAllLayersCore()
    {
        if (_layerStack.Count == 0)
        {
            return;
        }

        _forceClearType = _layerStack[0].SavedClearTypeHint;
        while (_layerStack.Count > 0)
        {
            int index = _layerStack.Count - 1;
            Direct3D9LayerFrame frame = _layerStack[index];
            _layerStack.RemoveAt(index);
            frame.Release();
        }

        _bounds = new Direct3D9SurfaceRect(0, 0, checked((int) _width), checked((int) _height));
    }

    private sealed class Direct3D9LayerFrame
    {
        private bool _isReleased;

        internal Direct3D9LayerFrame(
            Direct3D9SurfaceRect layerBounds,
            Direct3D9SurfaceRect previousBounds,
            Direct3D9SurfaceRect currentClip,
            float alpha,
            MilAntiAliasMode antiAliasMode,
            nint geometricMask,
            nint alphaMaskBrush,
            nint sourceBitmap,
            bool savedClearTypeHint,
            Direct3D9LayerOperations operations)
        {
            LayerBounds = layerBounds;
            PreviousBounds = previousBounds;
            CurrentClip = currentClip;
            Alpha = alpha;
            AntiAliasMode = antiAliasMode;
            GeometricMask = geometricMask;
            AlphaMaskBrush = alphaMaskBrush;
            SourceBitmap = sourceBitmap;
            SavedClearTypeHint = savedClearTypeHint;
            Operations = operations;
        }

        internal Direct3D9SurfaceRect LayerBounds { get; }

        internal Direct3D9SurfaceRect PreviousBounds { get; }

        internal Direct3D9SurfaceRect CurrentClip { get; }

        internal float Alpha { get; }

        internal MilAntiAliasMode AntiAliasMode { get; }

        internal nint GeometricMask { get; }

        internal nint AlphaMaskBrush { get; }

        internal nint SourceBitmap { get; }

        internal bool SavedClearTypeHint { get; }

        internal Direct3D9LayerOperations Operations { get; }

        internal void Release()
        {
            if (_isReleased)
            {
                return;
            }

            _isReleased = true;
            if (SourceBitmap != 0)
            {
                Operations.ReleaseSourceBitmap(SourceBitmap);
            }

            if (AlphaMaskBrush != 0)
            {
                Operations.ReleaseAlphaMaskBrush(AlphaMaskBrush);
            }

            if (GeometricMask != 0)
            {
                Operations.ReleaseGeometricMask(GeometricMask);
            }
        }
    }

    private void ReleaseIntermediateMultisampleSurface()
    {
        _intermediateMultisampleSurface?.Dispose();
        _intermediateMultisampleSurface = null;
    }

    private void ReleaseIntermediateMultisampleSurfaceWhenTooLarge(uint width, uint height)
    {
        if (_intermediateMultisampleSurface is null)
        {
            return;
        }

        SurfaceDesc description = _device.GetRenderTargetDescription(_intermediateMultisampleSurface);
        ulong currentArea = (ulong) description.Width * description.Height;
        ulong newArea = (ulong) width * height;
        if (newArea < currentArea / 4)
        {
            ReleaseIntermediateMultisampleSurface();
        }
    }

    internal bool HasAlpha()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _pixelFormat is
            MilPixelFormat.Pbgra32Bpp or
            MilPixelFormat.Prgba64Bpp or
            MilPixelFormat.Prgba128BppFloat;
    }

    private static Format ToDirect3DFormat(MilPixelFormat pixelFormat) => pixelFormat switch
    {
        MilPixelFormat.Pbgra32Bpp => Format.A8R8G8B8,
        MilPixelFormat.Bgr32Bpp101010 => Format.A2R10G10B10,
        _ => Format.X8R8G8B8
    };

    private static MilPixelFormat ToMilPixelFormat(Format format) => format switch
    {
        Format.A8R8G8B8 => MilPixelFormat.Pbgra32Bpp,
        Format.A2R10G10B10 => MilPixelFormat.Bgr32Bpp101010,
        _ => MilPixelFormat.Bgr32Bpp
    };

    internal int Clear(MilColorF? color, Direct3D9SurfaceRect? aliasedClip)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_isRenderingEnabled)
        {
            return 0;
        }

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);

        if (!IsValid || !color.HasValue || _width == 0 || _height == 0)
        {
            _hasValidContents = true;
            return 0;
        }

        if (!TryIntersect(_bounds, aliasedClip ?? _bounds, out Direct3D9SurfaceRect clip))
        {
            _hasValidContents = true;
            return 0;
        }

        int result = SetAsRenderTarget();
        if (result < 0)
        {
            return result;
        }

        result = _device.SetClipRect(clip);
        if (result < 0)
        {
            return result;
        }

        bool premultiply = _pixelFormat is MilPixelFormat.Pbgra32Bpp
            or MilPixelFormat.Prgba64Bpp
            or MilPixelFormat.Prgba128BppFloat;
        result = _device.ClearTarget(ConvertToSrgb(color.GetValueOrDefault(), premultiply));
        if (result >= 0)
        {
            _hasValidContents = true;
        }

        return result;
    }

    internal int PopulateDestinationTexture(
        Direct3D9SurfaceRect sourceRect,
        Direct3D9SurfaceRect destinationRect,
        Direct3D9Texture destinationTexture)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(destinationTexture);

        Direct3D9Surface sourceSurface = _renderTargetSurface
            ?? throw new InvalidOperationException("A render-target surface is required.");

        using Direct3D9UseContextGuard useContext = new(_device);
        int result = destinationTexture.TryGetSurfaceLevel(0, out Direct3D9Surface? destinationSurface);
        if (result < 0)
        {
            return result;
        }

        using (destinationSurface)
        {
            return destinationSurface is null
                ? Direct3D9Factory.GenericFailureHResult
                : _device.StretchRect(sourceSurface, sourceRect, destinationSurface, destinationRect);
        }
    }

    internal int SetAsRenderTarget()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        Direct3D9Surface renderTarget = _in3D
            ? Get3DRenderTargetSurface()
            : Get2DRenderTargetSurface();
        return _device.SetRenderTarget(renderTarget);
    }

    internal int SetAsRenderTargetFor3D()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _device.SetRenderTarget(Get3DRenderTargetSurface());
    }

    internal int EnsureState(Direct3D9ContextState contextState)
    { 
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        int result = SetAsRenderTarget();
        if (result < 0)
        {
            return result;
        }

        result = EnsureClip(contextState);
        if (result < 0 || result == Direct3D9Factory.ClippedToEmptyHResult)
        {
            return result;
        }

        _resetPerPrimitiveResourceUsage?.Invoke();
        return contextState.In3D ? Ensure3DState(contextState) : Ensure2DState();
    }

    internal int EnsureClip(Direct3D9ContextState contextState)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        Direct3D9SurfaceRect aliasedClip = contextState.AliasedClip ?? _bounds;
        if (!TryIntersect(_bounds, aliasedClip, out Direct3D9SurfaceRect currentClip))
        {
            return Direct3D9Factory.ClippedToEmptyHResult;
        }

        return _device.SetClipRect(currentClip);
    }

    internal int Ensure2DState()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        int result = _device.SetDepthStencilSurface(null);
        if (result < 0)
        {
            return result;
        }

        result = _device.Set2DTransformForFixedFunction();
        if (result < 0)
        {
            return result;
        }

        result = _device.SetRenderState(Renderstatetype.Cullmode, (uint) Cull.None);
        if (result < 0)
        {
            return result;
        }

        result = _device.SetRenderState(Renderstatetype.Zfunc, (uint) Cmpfunc.Lessequal);
        if (result < 0)
        {
            return result;
        }

        result = _device.SetRenderState(Renderstatetype.Zwriteenable, 0);
        if (result < 0)
        {
            return result;
        }

        MultisampleType multisampleType = _renderTargetSurface is null
            ? _multisampleType
            : _device.GetRenderTargetDescription(_renderTargetSurface).MultiSampleType;
        return multisampleType == MultisampleType.MultisampleNone
            ? result
            : _device.SetRenderState(Renderstatetype.Multisampleantialias, 0);
    }

    private Direct3D9Surface Get2DRenderTargetSurface()
    {
        return _renderTargetSurface
            ?? throw new InvalidOperationException("A render-target surface is required.");
    }

    private Direct3D9Surface Get3DRenderTargetSurface()
    {
        return _activeRenderTargetSurfaceFor3D ?? _renderTargetSurfaceFor3D ?? _renderTargetSurface
            ?? throw new InvalidOperationException("A 3D render-target surface is required.");
    }

    private static bool TryIntersectBoundsWithSurface(
        MilRectF bounds,
        Direct3D9SurfaceRect surface,
        MilAntiAliasMode antiAliasMode,
        out Direct3D9SurfaceRect intersection)
    {
        if (float.IsNaN(bounds.Left)
            || float.IsNaN(bounds.Top)
            || float.IsNaN(bounds.Right)
            || float.IsNaN(bounds.Bottom))
        {
            intersection = default;
            return false;
        }

        int left = antiAliasMode == MilAntiAliasMode.None
            ? RasterizerRound(bounds.Left, surface.Left, surface.Right)
            : FloorToSurface(bounds.Left, surface.Left, surface.Right);
        int top = antiAliasMode == MilAntiAliasMode.None
            ? RasterizerRound(bounds.Top, surface.Top, surface.Bottom)
            : FloorToSurface(bounds.Top, surface.Top, surface.Bottom);
        int right = antiAliasMode == MilAntiAliasMode.None
            ? RasterizerRound(bounds.Right, surface.Left, surface.Right)
            : CeilingToSurface(bounds.Right, surface.Left, surface.Right);
        int bottom = antiAliasMode == MilAntiAliasMode.None
            ? RasterizerRound(bounds.Bottom, surface.Top, surface.Bottom)
            : CeilingToSurface(bounds.Bottom, surface.Top, surface.Bottom);

        return TryIntersect(surface, new Direct3D9SurfaceRect(left, top, right, bottom), out intersection);
    }

    private static int RasterizerRound(float value, int minimum, int maximum)
    {
        if (value <= minimum - 1f)
        {
            return minimum;
        }

        if (value >= maximum + 1f)
        {
            return maximum;
        }

        int fixedValue = checked((int) MathF.Floor((value * 16f) + 0.5f));
        return Math.Clamp((fixedValue + 7) >> 4, minimum, maximum);
    }

    private static int FloorToSurface(float value, int minimum, int maximum)
    {
        if (value <= minimum)
        {
            return minimum;
        }

        return value >= maximum ? maximum : (int) MathF.Floor(value);
    }

    private static int CeilingToSurface(float value, int minimum, int maximum)
    {
        if (value <= minimum)
        {
            return minimum;
        }

        return value >= maximum ? maximum : (int) MathF.Ceiling(value);
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

    private static byte RoundToByte(float value) => RoundByteValue(value * 255f);

    private static byte RoundByteValue(float value)
    {
        if (!(value > 0f))
        {
            return 0;
        }

        return value >= byte.MaxValue
            ? byte.MaxValue
            : (byte) MathF.Floor(value + 0.5f);
    }

    internal int Ensure3DState(Direct3D9ContextState contextState)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        int result = _device.Set3DTransforms(
            contextState.WorldTransform,
            contextState.ViewTransform,
            contextState.ProjectionTransform,
            contextState.ViewportProjectionModifier);
        if (result < 0)
        {
            return result;
        }

        result = _device.SetRenderState(Renderstatetype.Cullmode, (uint) contextState.CullMode);
        if (result < 0)
        {
            return result;
        }

        bool isDepthBufferEnabled = _in3D ? _isActiveDepthBufferEnabled : _isDepthBufferEnabled;
        IDirect3DSurface9* depthStencilSurface = _in3D
            ? _createdDepthStencilSurface?.SurfaceForDeviceCall
            : _depthStencilSurface;
        result = _device.SetDepthStencilSurface(isDepthBufferEnabled ? depthStencilSurface : null);
        if (result < 0)
        {
            return result;
        }

        if (isDepthBufferEnabled)
        {
            result = _device.SetRenderState(Renderstatetype.Zfunc, (uint) contextState.DepthBufferFunction);
            if (result < 0)
            {
                return result;
            }
        }

        Direct3D9Surface? renderTargetSurfaceFor3D = _activeRenderTargetSurfaceFor3D
            ?? _renderTargetSurfaceFor3D
            ?? _renderTargetSurface;
        MultisampleType multisampleType = renderTargetSurfaceFor3D is null
            ? _multisampleTypeFor3D
            : _device.GetRenderTargetDescription(renderTargetSurfaceFor3D).MultiSampleType;
        return multisampleType == MultisampleType.MultisampleNone
            ? result
            : _device.SetRenderState(
                Renderstatetype.Multisampleantialias,
                contextState.IsAntialiasingEnabled ? 1u : 0u);
    }
}
