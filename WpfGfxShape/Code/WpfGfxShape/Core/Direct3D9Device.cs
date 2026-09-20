using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using Windows.Win32;

namespace WpfGfxShape.Core;

internal enum Direct3D9DeviceStateSource
{
    CooperativeLevel,
    ExtendedCheck,
    Present
}

internal enum Direct3D9DeviceStateKind
{
    Operational,
    Occluded,
    ModeChanged,
    DeviceLost,
    Failure
}

internal readonly record struct Direct3D9FrameMetrics(uint Vertices, uint Primitives);

internal readonly record struct Direct3D9DeviceState(int HResult, Direct3D9DeviceStateSource Source)
{
    internal bool PresentProcessed { get; init; } = HResult == 0;

    internal Direct3D9DeviceStateKind Kind => HResult switch
    {
        0 => Direct3D9DeviceStateKind.Operational,
        Direct3D9Factory.PresentOccludedHResult => Direct3D9DeviceStateKind.Occluded,
        Direct3D9Factory.PresentModeChangedHResult => Direct3D9DeviceStateKind.ModeChanged,
        Direct3D9Factory.DeviceLostHResult or
        Direct3D9Factory.DeviceHungHResult or
        Direct3D9Factory.DeviceRemovedHResult => Direct3D9DeviceStateKind.DeviceLost,
        _ => Direct3D9DeviceStateKind.Failure
    };

    internal bool IsOperational => Kind is Direct3D9DeviceStateKind.Operational or Direct3D9DeviceStateKind.Occluded;

    internal bool RequiresDeviceRecreation => Kind is Direct3D9DeviceStateKind.ModeChanged or Direct3D9DeviceStateKind.DeviceLost;

    internal bool UsedExtendedCheck => Source == Direct3D9DeviceStateSource.ExtendedCheck;
}

internal unsafe interface IDirect3D9MediaDeviceConsumer
{
    void SetDirect3DDevice9(IDirect3DDevice9* device);
}

internal unsafe delegate int Direct3D9SetVertexShader(IDirect3DVertexShader9* vertexShader);

internal delegate int Direct3D9GetPresentTimestamp(out ulong timestamp);

internal delegate uint Direct3D9RegisterWindowMessage(string messageName);

internal delegate void Direct3D9PostWindowMessage(nint window, uint message);

internal unsafe delegate int Direct3D9SetPixelShader(IDirect3DPixelShader9* pixelShader);

internal delegate int Direct3D9SetFlexibleVertexFormat(uint flexibleVertexFormat);

internal delegate int Direct3D9SetConvolutionMonoKernel(uint width, uint height);

internal unsafe delegate int Direct3D9SetStreamSource(
    uint streamNumber,
    IDirect3DVertexBuffer9* streamData,
    uint offsetInBytes,
    uint stride);

internal unsafe delegate int Direct3D9SetIndices(IDirect3DIndexBuffer9* indexData);

internal delegate int Direct3D9DrawIndexedTriangleList(
    uint baseVertexIndex,
    uint minIndex,
    uint vertexCount,
    uint startIndex,
    uint primitiveCount);

internal delegate int Direct3D9DrawTriangleList(uint startVertex, uint primitiveCount);

internal unsafe delegate int Direct3D9DrawPrimitiveUp(
    Primitivetype primitiveType,
    uint primitiveCount,
    void* vertexStreamZeroData,
    uint vertexStreamZeroStride);

internal delegate int Direct3D9BeginVideoRender(Direct3D9Device device, out nint bitmapSource);

internal unsafe delegate int Direct3D9SetTexture(uint stage, IDirect3DBaseTexture9* texture);

internal unsafe delegate int Direct3D9SetDepthStencilSurface(IDirect3DSurface9* depthStencilSurface);

internal delegate int Direct3D9CreateDepthBuffer(
    uint width,
    uint height,
    MultisampleType multisampleType,
    out Direct3D9Surface? surface);

internal delegate int Direct3D9CreateRenderTarget(
    uint width,
    uint height,
    Format format,
    MultisampleType multisampleType,
    out Direct3D9Surface? surface);

internal delegate int Direct3D9StretchRect(
    Direct3D9Surface source,
    Direct3D9SurfaceRect sourceRect,
    Direct3D9Surface destination,
    Direct3D9SurfaceRect destinationRect);

internal delegate int Direct3D9CreateAdditionalSwapChain(
    PresentParameters presentParameters,
    out Direct3D9SwapChain? swapChain);

internal delegate int Direct3D9SetTextureStageState(uint stage, Texturestagestatetype state, uint value);

internal delegate int Direct3D9SetSamplerState(uint sampler, Samplerstatetype state, uint value);

internal delegate int Direct3D9SetRenderState(Renderstatetype state, uint value);

internal delegate int Direct3D9SetTransform(Transformstatetype state, Matrix4x4 matrix);

internal delegate int Direct3D9Clear(uint count, uint flags, uint color, float depth, uint stencil);

internal delegate int Direct3D9WaitForVBlank(uint swapChainIndex);

internal delegate int Direct3D9GetNumQueuedPresents(out uint queuedPresentCount);

internal delegate int Direct3D9CreateGpuQuery(out Direct3D9GpuQuery? query);

internal delegate int Direct3D9CreateTextPixelShader(uint resourceId, out Direct3D9PixelShader? pixelShader);

internal enum Direct3D9TextureBlendMode
{
    Default = 0,
    Copy = 1,
    ApplyVectorAlpha = 2,
    AddColors = 3
}

internal readonly record struct Direct3D9FilterMode(
    Texturefiltertype MagnificationFilter,
    Texturefiltertype MinificationFilter,
    Texturefiltertype MipmapFilter);

internal readonly record struct Direct3D9Box(
    float X,
    float Y,
    float Z,
    float LengthX,
    float LengthY,
    float LengthZ);

[StructLayout(LayoutKind.Sequential)]
internal readonly struct Direct3D9VertexXyzDiffuseUv2
{
    internal Direct3D9VertexXyzDiffuseUv2(float x, float y, float z, uint diffuse)
        : this(x, y, z, diffuse, 0, 0)
    {
    }

    internal Direct3D9VertexXyzDiffuseUv2(float x, float y, float z, uint diffuse, float u0, float v0)
    {
        X = x;
        Y = y;
        Z = z;
        Diffuse = diffuse;
        U0 = u0;
        V0 = v0;
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

[StructLayout(LayoutKind.Sequential)]
internal readonly struct Direct3D9VertexXyzDiffuseUv6
{
    internal readonly float X;
    internal readonly float Y;
    internal readonly float Z;
    internal readonly uint Diffuse;
    internal readonly float U0;
    internal readonly float V0;
    internal readonly float U1;
    internal readonly float V1;
    internal readonly float U2;
    internal readonly float V2;
    internal readonly float U3;
    internal readonly float V3;
    internal readonly float U4;
    internal readonly float V4;
    internal readonly float U5;
    internal readonly float V5;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct Direct3D9VertexXyzDiffuseUv8
{
    internal readonly float X;
    internal readonly float Y;
    internal readonly float Z;
    internal readonly uint Diffuse;
    internal readonly float U0;
    internal readonly float V0;
    internal readonly float U1;
    internal readonly float V1;
    internal readonly float U2;
    internal readonly float V2;
    internal readonly float U3;
    internal readonly float V3;
    internal readonly float U4;
    internal readonly float V4;
    internal readonly float U5;
    internal readonly float V5;
    internal readonly float U6;
    internal readonly float V6;
    internal readonly float U7;
    internal readonly float V7;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct Direct3D9VertexXyzNormalDiffuseSpecularUv4
{
    internal readonly float X;
    internal readonly float Y;
    internal readonly float Z;
    internal readonly float NormalX;
    internal readonly float NormalY;
    internal readonly float NormalZ;
    internal readonly uint Diffuse;
    internal readonly uint Specular;
    internal readonly float U0;
    internal readonly float V0;
    internal readonly float U1;
    internal readonly float V1;
    internal readonly float U2;
    internal readonly float V2;
    internal readonly float U3;
    internal readonly float V3;
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9Device : IDisposable
{
    private const Transformstatetype WorldTransform = (Transformstatetype) 256;
    private const uint ReadScanlineCapability = 0x00020000;
    private const uint MinificationAnisotropicFilterCapability = 0x00000400;
    private const uint MagnificationAnisotropicFilterCapability = 0x04000000;
    private const int Tier2 = 2 << 16;
    private const int SuccessfulPresentsBeforeGpuMarkerFlush = 3;
    private const int MaximumActiveGpuMarkers = 35;
    private const uint PixelShaderVersion20 = 0xFFFF0200;
    private const int TextPixelShaderCount = 4;
    private const uint HardwareVertexBufferCapacity = 20001 * 32;
    private const uint HardwareIndexBufferCapacity = 20001 * 3 * sizeof(ushort);

    private readonly Direct3D9ResourceManager _resourceManager;
    private readonly Dictionary<Format, Direct3D9TargetFormatTestStatus> _targetFormatTestStatuses = new()
    {
        [Format.X8R8G8B8] = new(),
        [Format.A8R8G8B8] = new(),
        [Format.A2R10G10B10] = new()
    };
    private readonly Action<Direct3D9Device>? _unusableNotification;
    private readonly Action<Direct3D9Device>? _disposedNotification;
    private Action<uint>? _unexpectedAdapterErrorNotification;
    private readonly Func<nint, int>? _checkDeviceState;
    private readonly Func<uint, Devtype, Format, Format, Format, int>? _checkDepthStencilMatch;
    private readonly Func<uint, uint, Format, MultisampleType, uint, bool, int>? _createRenderTargetForFormatTest;
    private readonly Func<PresentParameters, Direct3D9TargetFormatTestStatus, int>? _testLockableSwapChainForFormatTest;
    private readonly Func<int>? _setRenderTargetForFormatTest;
    private readonly Func<int>? _clearDepthStencilSurfaceForFormatTest;
    private readonly Func<uint, uint, uint, uint, Format, Pool, int>? _createLockableTextureForFormatTest;
    private readonly Func<int>? _beginSceneForFormatTest;
    private readonly Func<int>? _renderTextureForFormatTest;
    private readonly Func<int>? _endSceneForFormatTest;
    private readonly Func<Direct3D9Surface, SurfaceDesc>? _getRenderTargetDescription;
    private readonly Func<Direct3D9Surface, int>? _setRenderTarget;
    private readonly Func<Viewport9, int>? _setViewport;
    private readonly Func<Direct3D9PointAndSizeRect?, int>? _setScissorRect;
    private readonly Func<Direct3D9PointAndSizeRect, int>? _setSurfaceToClippingMatrix;
    private readonly Func<uint, Matrix4x4, int>? _setVertexShaderConstants;
    private readonly Direct3D9SetPixelShaderFloat4Constants? _setPixelShaderConstants;
    private readonly Direct3D9SetPixelShaderInt4Constant? _setPixelShaderInt4Constant;
    private readonly Direct3D9SetPixelShaderBoolConstant? _setPixelShaderBoolConstant;
    private readonly Direct3D9SetTextureStageState? _setTextureStageState;
    private readonly Direct3D9SetVertexShader? _setVertexShader;
    private readonly Direct3D9SetPixelShader? _setPixelShader;
    private readonly Direct3D9SetFlexibleVertexFormat? _setFlexibleVertexFormat;
    private readonly Direct3D9SetConvolutionMonoKernel? _setConvolutionMonoKernel;
    private readonly Direct3D9SetStreamSource? _setStreamSource;
    private readonly Direct3D9SetIndices? _setIndices;
    private readonly Direct3D9DrawIndexedTriangleList? _drawIndexedTriangleList;
    private readonly Direct3D9DrawTriangleList? _drawTriangleList;
    private readonly Direct3D9DrawPrimitiveUp? _drawPrimitiveUp;
    private readonly Direct3D9SetTexture? _setTexture;
    private readonly Direct3D9SetDepthStencilSurface? _setDepthStencilSurface;
    private readonly Direct3D9CreateDepthBuffer? _createDepthBuffer;
    private readonly Direct3D9CreateRenderTarget? _createRenderTarget;
    private readonly Direct3D9StretchRect? _stretchRect;
    private readonly Direct3D9CreateAdditionalSwapChain? _createAdditionalSwapChain;
    private readonly Direct3D9SetSamplerState? _setSamplerState;
    private readonly Direct3D9SetRenderState? _setRenderState;
    private readonly Direct3D9SetTransform? _setTransform;
    private readonly Direct3D9Clear? _clear;
    private readonly Direct3D9WaitForVBlank? _waitForVBlank;
    private readonly Direct3D9GetNumQueuedPresents? _getNumQueuedPresents;
    private readonly Direct3D9CreateGpuQuery? _createGpuQuery;
    private readonly Func<int>? _recordSuccessfulPresent;
    private readonly Action<uint> _presentFailureDelay;
    private readonly Direct3D9PostWindowMessage _postWindowMessage;
    private readonly uint _presentFailureWindowMessage;
    private readonly Func<uint, uint, uint, uint, Format, Pool, int>? _createTexture;
    private readonly Func<Format, int>? _checkRenderTargetFormat;
    private readonly bool _gpuThrottlingDisabled;
    private readonly Func<long> _getTimestamp;
    private readonly Direct3D9GetPresentTimestamp _getPresentTimestamp;
    private readonly long _timestampFrequency;
    private readonly Direct3D9VertexShaderConstantState _vertexShaderConstantState = new();
    private readonly Direct3D9PixelShaderConstantState _pixelShaderConstantState = new();
    private readonly Direct3D9PixelShaderInt4ConstantState _pixelShaderInt4ConstantState = new();
    private readonly Direct3D9PixelShaderBoolConstantState _pixelShaderBoolConstantState = new();
    private IDirect3D9* _direct3D;
    private IDirect3DDevice9* _device;
    private IDirect3DDevice9Ex* _deviceEx;
    private readonly Direct3D9ScissorState _scissorState;
    private Direct3D9PointAndSizeRect _targetSurface;
    private Direct3D9PointAndSizeRect _viewport;
    private Direct3D9PointAndSizeRect _clip;
    private Matrix4x4 _twoDimensionalProjection = Matrix4x4.Identity;
    private Matrix4x4 _worldTransform = Matrix4x4.Identity;
    private Matrix4x4 _viewTransform = Matrix4x4.Identity;
    private Matrix4x4 _projectionTransform = Matrix4x4.Identity;
    private readonly Matrix4x4[] _nonWorldTransforms = new Matrix4x4[256];
    private readonly bool[] _areNonWorldTransformsKnown = new bool[256];
    private bool _isWorldTransformKnown;
    private bool _isClipSet;
    private bool _twoDimensionalTransformsApplied;
    private bool _twoDimensionalVertexShaderTransformApplied;
    private uint _twoDimensionalVertexShaderStartRegister;
    private IDirect3DVertexShader9* _vertexShader;
    private IDirect3DPixelShader9* _pixelShader;
    private IDirect3DVertexBuffer9* _streamSourceVertexBuffer;
    private uint _streamSourceVertexStride;
    private Direct3D9HardwareVertexBuffer? _hardwareVertexBuffer;
    private Direct3D9HardwareIndexBuffer? _hardwareIndexBuffer;
    private readonly Direct3D9PrimitiveVertexBuffer _primitiveVertexBufferDuv2 = new();
    private readonly Direct3D9PrimitiveVertexBufferDuv6 _primitiveVertexBufferDuv6 = new();
    private readonly Direct3D9PrimitiveVertexBufferXyzNormalDiffuseSpecularUv4 _primitiveVertexBufferXyzNormalDiffuseSpecularUv4 = new();
    private readonly Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzDiffuseUv2> _vertexBufferXyzDuv2 = new();
    private readonly Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzDiffuseUv8> _vertexBufferXyzRhwDuv8 = new();
    private readonly nint[] _textures = new nint[8];
    private readonly bool[] _areTexturesKnown = new bool[8];
    private readonly uint[] _textureStageStates = new uint[8 * 33];
    private readonly bool[] _areTextureStageStatesKnown = new bool[8 * 33];
    private readonly uint[] _samplerStates = new uint[8 * 14];
    private readonly bool[] _areSamplerStatesKnown = new bool[8 * 14];
    private readonly uint[] _renderStates = new uint[210];
    private readonly bool[] _areRenderStatesKnown = new bool[210];
    private IDirect3DSurface9* _dummyBackBuffer;
    private IDirect3DSurface9* _currentRenderTarget;
    private IDirect3DSurface9* _depthStencilSurface;
    private IDirect3DSurface9* _depthStencilSurfaceForCurrentRenderTarget;
    private uint _depthStencilSurfaceWidth;
    private uint _depthStencilSurfaceHeight;
    private bool _isDepthStencilSurfaceKnown;
    private uint _flexibleVertexFormat;
    private bool _isVertexShaderKnown;
    private bool _isPixelShaderKnown;
    private bool _isStreamSourceKnown;
    private bool _isFlexibleVertexFormatKnown;
    private bool _areTextureCoordinateIndicesDefault;
    private readonly object _deviceLostSyncRoot = new();
    private bool _displayInvalidSubmitted;
    private bool _deviceLostProcessed;
    private bool _hardwareVBlankTested;
    private bool _hardwareVBlankSupported;
    private uint _frameNumber;
    private readonly List<Direct3D9GpuMarker> _activeGpuMarkers = [];
    private readonly List<Direct3D9GpuMarker> _freeGpuMarkers = [];
    private readonly Direct3D9PixelShader?[] _textPixelShaders = new Direct3D9PixelShader?[TextPixelShaderCount];
    private ulong _lastGpuMarkerId;
    private ulong _lastConsumedGpuMarkerId;
    private uint _successfulPresentsSinceGpuMarkerFlush;
    private bool _gpuMarkersTested;
    private bool _gpuMarkersEnabled;
    private bool _gpuMarkerConsumed;
    private bool _multisampleFailed;
    private bool _inScene;
    private int _tier;
    private readonly bool _isExtendedDevice;
    private readonly Pool _managedPool;
    private bool _isDisposed;
    private readonly uint _realizationCacheIndex;
    private readonly bool _ownsRealizationCacheIndex;
    private readonly object? _deviceEntryLock;
    private int _entryCount;
    private uint _entryThreadId;
    private Direct3D9FilterMode _supportedAnisotropicFilterMode;
    private readonly Action<Direct3D9FrameMetrics>? _consumeFrameMetrics;
    private uint _metricsVerticesPerFrame;
    private uint _metricsPrimitivesPerFrame;

    internal Direct3D9Device(
        IDirect3DDevice9* device,
        IDirect3DDevice9Ex* deviceEx,
        uint adapterOrdinal,
        Devtype deviceType,
        uint behaviorFlags,
        PresentParameters presentParameters,
        Action<Direct3D9Device>? unusableNotification = null,
        Action<Direct3D9Device>? disposedNotification = null,
        Caps9 capabilities = default,
        Displaymode displayMode = default,
        IDirect3D9* direct3D = null,
        Func<uint, Devtype, Format, Format, Format, int>? checkDepthStencilMatch = null,
        nint focusWindow = 0,
        Func<uint, uint, Format, MultisampleType, uint, bool, int>? createRenderTargetForFormatTest = null,
        Func<PresentParameters, Direct3D9TargetFormatTestStatus, int>? testLockableSwapChainForFormatTest = null,
        Func<int>? setRenderTargetForFormatTest = null,
        Func<int>? clearDepthStencilSurfaceForFormatTest = null,
        Func<uint, uint, uint, uint, Format, Pool, int>? createLockableTextureForFormatTest = null,
        Func<int>? beginSceneForFormatTest = null,
        Func<int>? renderTextureForFormatTest = null,
        Func<int>? endSceneForFormatTest = null,
        Func<Direct3D9Surface, SurfaceDesc>? getRenderTargetDescription = null,
        Func<Direct3D9Surface, int>? setRenderTarget = null,
        Func<Viewport9, int>? setViewport = null,
        Func<Direct3D9PointAndSizeRect?, int>? setScissorRect = null,
        Func<Direct3D9PointAndSizeRect, int>? setSurfaceToClippingMatrix = null,
        Func<uint, Matrix4x4, int>? setVertexShaderConstants = null,
        Direct3D9SetPixelShaderFloat4Constants? setPixelShaderConstants = null,
        Direct3D9SetPixelShaderInt4Constant? setPixelShaderInt4Constant = null,
        Direct3D9SetPixelShaderBoolConstant? setPixelShaderBoolConstant = null,
        Direct3D9SetTextureStageState? setTextureStageState = null,
        Direct3D9SetVertexShader? setVertexShader = null,
        Direct3D9SetPixelShader? setPixelShader = null,
        Direct3D9SetFlexibleVertexFormat? setFlexibleVertexFormat = null,
        Direct3D9SetConvolutionMonoKernel? setConvolutionMonoKernel = null,
        Direct3D9SetStreamSource? setStreamSource = null,
        Direct3D9SetTexture? setTexture = null,
        Direct3D9SetSamplerState? setSamplerState = null,
        Direct3D9SetDepthStencilSurface? setDepthStencilSurface = null,
        Direct3D9CreateDepthBuffer? createDepthBuffer = null,
        Direct3D9SetRenderState? setRenderState = null,
        Direct3D9SetTransform? setTransform = null,
        Direct3D9Clear? clear = null,
        Direct3D9CreateRenderTarget? createRenderTarget = null,
        Direct3D9StretchRect? stretchRect = null,
        Direct3D9CreateAdditionalSwapChain? createAdditionalSwapChain = null,
        Direct3D9WaitForVBlank? waitForVBlank = null,
        Direct3D9GetNumQueuedPresents? getNumQueuedPresents = null,
        Func<long>? getTimestamp = null,
        Direct3D9GetPresentTimestamp? getPresentTimestamp = null,
        long timestampFrequency = 0,
        Direct3D9CreateGpuQuery? createGpuQuery = null,
        bool gpuThrottlingDisabled = false,
        Func<int>? recordSuccessfulPresent = null,
        Func<uint, uint, uint, uint, Format, Pool, int>? createTexture = null,
        Func<Format, int>? checkRenderTargetFormat = null,
        Direct3D9SetIndices? setIndices = null,
        Direct3D9DrawIndexedTriangleList? drawIndexedTriangleList = null,
        Direct3D9DrawTriangleList? drawTriangleList = null,
        long adapterLuid = 0,
        uint realizationCacheIndex = Direct3D9ImmediateBrushRealizer.InvalidRealizationCacheIndex,
        bool ownsRealizationCacheIndex = false,
        bool canDrawText = false,
        Func<nint, int>? checkDeviceState = null,
        IDirect3DSurface9* dummyBackBuffer = null,
        Direct3D9RegisterWindowMessage? registerWindowMessage = null,
        Action<uint>? presentFailureDelay = null,
        Direct3D9PostWindowMessage? postWindowMessage = null,
        Direct3D9DrawPrimitiveUp? drawPrimitiveUp = null,
        Action<Direct3D9FrameMetrics>? consumeFrameMetrics = null)
    {
        _resourceManager = new Direct3D9ResourceManager(this);
        Direct3D9Factory.AddRef(direct3D);
        _direct3D = direct3D;
        _device = device;
        _deviceEx = deviceEx;
        _dummyBackBuffer = dummyBackBuffer;
        _isExtendedDevice = deviceEx is not null;
        _managedPool = _isExtendedDevice ? (Pool) 6 : Pool.Managed;
        _scissorState = new Direct3D9ScissorState(SetNativeScissorRect, SetRenderState);
        _unusableNotification = unusableNotification;
        _disposedNotification = disposedNotification;
        _checkDeviceState = checkDeviceState;
        _checkDepthStencilMatch = checkDepthStencilMatch;
        _createRenderTargetForFormatTest = createRenderTargetForFormatTest;
        _testLockableSwapChainForFormatTest = testLockableSwapChainForFormatTest;
        _setRenderTargetForFormatTest = setRenderTargetForFormatTest;
        _clearDepthStencilSurfaceForFormatTest = clearDepthStencilSurfaceForFormatTest;
        _createLockableTextureForFormatTest = createLockableTextureForFormatTest;
        _beginSceneForFormatTest = beginSceneForFormatTest;
        _renderTextureForFormatTest = renderTextureForFormatTest;
        _endSceneForFormatTest = endSceneForFormatTest;
        _getRenderTargetDescription = getRenderTargetDescription;
        _setRenderTarget = setRenderTarget;
        _setViewport = setViewport;
        _setScissorRect = setScissorRect;
        _setSurfaceToClippingMatrix = setSurfaceToClippingMatrix;
        _setVertexShaderConstants = setVertexShaderConstants;
        _setPixelShaderConstants = setPixelShaderConstants;
        _setPixelShaderInt4Constant = setPixelShaderInt4Constant;
        _setPixelShaderBoolConstant = setPixelShaderBoolConstant;
        _setTextureStageState = setTextureStageState;
        _setVertexShader = setVertexShader;
        _setPixelShader = setPixelShader;
        _setFlexibleVertexFormat = setFlexibleVertexFormat;
        _setConvolutionMonoKernel = setConvolutionMonoKernel;
        _setStreamSource = setStreamSource;
        _setIndices = setIndices;
        _drawIndexedTriangleList = drawIndexedTriangleList;
        _drawTriangleList = drawTriangleList;
        _drawPrimitiveUp = drawPrimitiveUp;
        _consumeFrameMetrics = consumeFrameMetrics;
        _setTexture = setTexture;
        _setSamplerState = setSamplerState;
        _setDepthStencilSurface = setDepthStencilSurface;
        _createDepthBuffer = createDepthBuffer;
        _createRenderTarget = createRenderTarget;
        _stretchRect = stretchRect;
        _createAdditionalSwapChain = createAdditionalSwapChain;
        _setRenderState = setRenderState;
        _setTransform = setTransform;
        _clear = clear;
        _waitForVBlank = waitForVBlank;
        _getNumQueuedPresents = getNumQueuedPresents;
        _createGpuQuery = createGpuQuery;
        _recordSuccessfulPresent = recordSuccessfulPresent;
        _presentFailureDelay = presentFailureDelay ?? PInvoke.Sleep;
        _postWindowMessage = postWindowMessage ?? PostWindowMessage;
        _presentFailureWindowMessage = (registerWindowMessage ?? RegisterWindowMessage)("NeedsRePresentOnWake");
        _createTexture = createTexture;
        _checkRenderTargetFormat = checkRenderTargetFormat;
        _gpuThrottlingDisabled = gpuThrottlingDisabled;
        _getTimestamp = getTimestamp ?? Stopwatch.GetTimestamp;
        _getPresentTimestamp = getPresentTimestamp ?? GetPresentTimestamp;
        _timestampFrequency = timestampFrequency > 0 ? timestampFrequency : Stopwatch.Frequency;
        AdapterOrdinal = adapterOrdinal;
        AdapterLuid = adapterLuid;
        _realizationCacheIndex = realizationCacheIndex;
        _ownsRealizationCacheIndex = ownsRealizationCacheIndex;
        CanDrawText = canDrawText;
        DeviceType = deviceType;
        BehaviorFlags = behaviorFlags;
        PresentParameters = presentParameters;
        Capabilities = NormalizeCapabilities(capabilities);
        _supportedAnisotropicFilterMode = SelectSupportedAnisotropicFilterMode(Capabilities.TextureFilterCaps);
        DisplayMode = displayMode;
        FocusWindow = focusWindow;
        _deviceEntryLock = deviceType == Devtype.SW ? new object() : null;
        _resourceManager.SurfaceReleaseNotification = OnSurfaceReleased;
    }

    internal IDirect3DDevice9* Device
    {
        get
        {
            ObjectDisposedException.ThrowIf(_device is null, this);
            return _device;
        }
    }

    internal void InitializeMediaDeviceConsumer(IDirect3D9MediaDeviceConsumer consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        consumer.SetDirect3DDevice9(Device);
    }

    internal uint AdapterOrdinal
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return field;
        }
    }

    internal uint RealizationCacheIndex
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _realizationCacheIndex;
        }
    }

    internal long AdapterLuid
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return field;
        }
    }

    internal Devtype DeviceType
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return field;
        }
    }

    internal uint BehaviorFlags
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return field;
        }
    }

    internal PresentParameters PresentParameters { get; }

    internal Caps9 Capabilities { get; private set; }

    internal bool SupportsGetScanLine
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return (Capabilities.Caps & ReadScanlineCapability) != 0;
        }
    }

    internal bool IsLddmDevice
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return Direct3D9HardwareCapabilities.HasWddmSupport(Capabilities);
        }
    }

    internal bool IsHardwareDevice
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return Capabilities.DeviceType == Devtype.Hal;
        }
    }

    internal bool IsSoftwareDevice
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return Capabilities.DeviceType == Devtype.SW;
        }
    }

    internal bool IsPureDevice
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return (BehaviorFlags & D3D9.CreatePuredevice) != 0;
        }
    }

    internal bool CanAutoGenerateMipmaps
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return (Capabilities.Caps2 & (uint) D3D9.Caps2Canautogenmipmap) != 0;
        }
    }

    internal bool CanGenerateMipmapsWithStretchRect
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return (Capabilities.StretchRectFilterCaps & (uint) D3D9.PtfiltercapsMinflinear) != 0;
        }
    }

    internal bool CanStretchRectFromTextures
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return (Capabilities.DevCaps2 & (uint) D3D9.Devcaps2CanStretchrectFromTextures) != 0;
        }
    }

    internal bool CanMaskColorChannels
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return (Capabilities.PrimitiveMiscCaps & (uint) D3D9.PmisccapsColorwriteenable) != 0;
        }
    }

    internal bool CanHandleBlendFactor
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return (Capabilities.SrcBlendCaps & (uint) D3D9.PblendcapsBlendfactor) != 0;
        }
    }

    internal bool SupportsA8TextureFormat
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return TextureFormatSupport.SupportsA8;
        }
    }

    internal bool SupportsP8TextureFormat
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return TextureFormatSupport.SupportsP8;
        }
    }

    internal bool SupportsL8TextureFormat
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return TextureFormatSupport.SupportsL8;
        }
    }

    internal bool SupportsBorderColor
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return (Capabilities.TextureAddressCaps & (uint) D3D9.PtaddresscapsBorder) != 0;
        }
    }

    internal bool CanDrawText { get; private set; }

    internal bool IsTextPixelShaderInitializationEligible
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return field;
        }
        private set;
    }

    internal bool IsTextPixelShaderInitialized
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return field;
        }
        private set;
    }

    internal Direct3D9GlyphAlphaTextureFormat GlyphAlphaTextureFormat
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return field;
        }
        private set;
    }

    internal uint MaximumDesiredAnisotropicFilterLevel
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return Math.Min(4u, Capabilities.MaxAnisotropy);
        }
    }

    internal bool SupportsAnisotropicFiltering => MaximumDesiredAnisotropicFilterLevel > 1;

    internal Direct3D9FilterMode SupportedAnisotropicFilterMode
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _supportedAnisotropicFilterMode;
        }
    }

    internal bool SupportsConditionalNonPowerOfTwoTextures
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            uint textureCaps = Capabilities.TextureCaps;
            return (textureCaps & (uint) D3D9.PtexturecapsNonpow2Conditional) != 0
                && (textureCaps & (uint) D3D9.PtexturecapsPow2) != 0;
        }
    }

    internal bool SupportsUnconditionalNonPowerOfTwoTextures
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            uint textureCaps = Capabilities.TextureCaps;
            return (textureCaps & (uint) D3D9.PtexturecapsNonpow2Conditional) == 0
                && (textureCaps & (uint) D3D9.PtexturecapsPow2) == 0;
        }
    }

    internal bool SupportsTextureCapability(uint textureCapability)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return (Capabilities.TextureCaps & textureCapability) != 0;
    }

    internal uint MaximumStreams
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return Capabilities.MaxStreams;
        }
    }

    internal uint MaximumTextureBlendStages
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return Capabilities.MaxTextureBlendStages;
        }
    }

    internal uint MaximumSimultaneousTextures
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return Capabilities.MaxSimultaneousTextures;
        }
    }

    internal uint MaximumTextureWidth
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return Capabilities.MaxTextureWidth;
        }
    }

    internal uint MaximumTextureHeight
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return Capabilities.MaxTextureHeight;
        }
    }

    internal uint VertexShaderVersion
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return Capabilities.VertexShaderVersion;
        }
    }

    internal uint PixelShaderVersion
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return Capabilities.PixelShaderVersion;
        }
    }

    internal bool SupportsScissorRectangle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return (Capabilities.RasterCaps & (uint) D3D9.PrastercapsScissortest) != 0;
        }
    }

    internal bool SupportsLinearToSrgbPresentation
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return (Capabilities.Caps3 & (uint) D3D9.Caps3LinearToSrgbPresentation) != 0;
        }
    }

    internal Direct3D9TextureFormatSupport TextureFormatSupport { get; private set; }

    internal Direct3D9MultisampleSupport MultisampleSupport { get; private set; }

    internal int Tier
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _tier;
        }
    }

    internal bool ShouldAttemptMultisample
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return !_multisampleFailed && Tier >= Tier2;
        }
    }

    internal void UpdateCapabilities(Caps9 capabilities)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        Capabilities = NormalizeCapabilities(capabilities);
        _supportedAnisotropicFilterMode = SelectSupportedAnisotropicFilterMode(Capabilities.TextureFilterCaps);
    }

    private static Caps9 NormalizeCapabilities(Caps9 capabilities)
    {
        if (capabilities.MaxAnisotropy == 0)
        {
            capabilities.MaxAnisotropy = 1;
        }

        return capabilities;
    }

    private static Direct3D9FilterMode SelectSupportedAnisotropicFilterMode(uint filterCapabilities)
    {
        bool supportsMagnificationAnisotropy =
            (filterCapabilities & MagnificationAnisotropicFilterCapability) != 0;
        bool supportsMinificationAnisotropy =
            (filterCapabilities & MinificationAnisotropicFilterCapability) != 0;

        if (supportsMagnificationAnisotropy && supportsMinificationAnisotropy)
        {
            return new Direct3D9FilterMode(
                Texturefiltertype.Anisotropic,
                Texturefiltertype.Anisotropic,
                Texturefiltertype.Linear);
        }

        if (supportsMinificationAnisotropy)
        {
            return new Direct3D9FilterMode(
                Texturefiltertype.Linear,
                Texturefiltertype.Anisotropic,
                Texturefiltertype.Linear);
        }

        return new Direct3D9FilterMode(
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.None);
    }

    internal void UpdateTextureFormatSupport(Direct3D9TextureFormatSupport textureFormatSupport)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        TextureFormatSupport = textureFormatSupport;
    }

    internal void InitializeTextRenderingPrerequisites(bool isHardwareTextDisabled)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        IsTextPixelShaderInitializationEligible = false;
        CanDrawText = false;

        const uint pixelShaderVersion11 = 0xFFFF0101;
        if (Capabilities.PixelShaderVersion < pixelShaderVersion11
            || Capabilities.MaxTextureBlendStages < 4
            || !CanHandleBlendFactor
            || isHardwareTextDisabled)
        {
            return;
        }

        IsTextPixelShaderInitializationEligible = InitializeGlyphAlphaTextureFormat() >= 0;
    }

    internal int InitializeTextPixelShaders(Direct3D9CreateTextPixelShader createPixelShader)
    {
        ArgumentNullException.ThrowIfNull(createPixelShader);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!IsTextPixelShaderInitializationEligible)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        uint firstResourceId = Capabilities.PixelShaderVersion >= PixelShaderVersion20
            ? GlyphAlphaTextureFormat == Direct3D9GlyphAlphaTextureFormat.L8 ? 112u : 108u
            : GlyphAlphaTextureFormat == Direct3D9GlyphAlphaTextureFormat.L8 ? 104u : 100u;

        ReleaseTextPixelShaders();
        for (int index = 0; index < _textPixelShaders.Length; index++)
        {
            int result = createPixelShader(firstResourceId + (uint) index, out Direct3D9PixelShader? pixelShader);
            if (result < 0)
            {
                pixelShader?.Dispose();
                ReleaseTextPixelShaders();
                return result;
            }
            if (pixelShader is null)
            {
                ReleaseTextPixelShaders();
                throw new InvalidOperationException("Direct3D text pixel shader creation returned a null shader.");
            }

            _textPixelShaders[index] = pixelShader;
        }

        IsTextPixelShaderInitialized = true;
        CanDrawText = true;
        return 0;
    }

    internal unsafe int InitializeTextPixelShadersFromResources()
    {
        return InitializeTextPixelShaders(CreatePixelShaderFromResource);
    }

    private unsafe int CreatePixelShaderFromResource(uint resourceId, out Direct3D9PixelShader? pixelShader)
    {
        if (!Direct3D9TextPixelShaderResources.TryGetShaderBytecode(resourceId, out ReadOnlySpan<uint> bytecode))
        {
            pixelShader = null;
            return Direct3D9Factory.GenericFailureHResult;
        }

        fixed (uint* shaderFunction = bytecode)
        {
            return TryCreatePixelShader(shaderFunction, out pixelShader);
        }
    }

    internal int InitializeGlyphAlphaTextureFormat()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        Direct3D9GlyphAlphaTextureFormat format;
        if (SupportsA8TextureFormat)
        {
            format = Direct3D9GlyphAlphaTextureFormat.A8;
        }
        else if (SupportsL8TextureFormat)
        {
            format = Direct3D9GlyphAlphaTextureFormat.L8;
        }
        else if (SupportsP8TextureFormat)
        {
            int result = SetLinearPalette();
            if (result < 0)
            {
                return result;
            }

            format = Direct3D9GlyphAlphaTextureFormat.P8;
        }
        else
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        GlyphAlphaTextureFormat = format;
        return 0;
    }

    internal void UpdateMultisampleSupport(Direct3D9MultisampleSupport multisampleSupport)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        MultisampleSupport = multisampleSupport;
    }

    internal void UpdateTier(int tier)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _tier = tier;
    }

    internal void SetMultisampleFailed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _multisampleFailed = true;
    }

    internal int GetSupportedTextureFormat(
        MilPixelFormat bitmapSourceFormat,
        MilPixelFormat destinationSurfaceFormat,
        bool forceAlpha,
        out MilPixelFormat textureSourceFormat)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        bool useAlpha = forceAlpha || MilPixelFormatInfo.HasAlphaChannel(bitmapSourceFormat);
        if (destinationSurfaceFormat == MilPixelFormat.Bgr32Bpp101010)
        {
            if (bitmapSourceFormat == MilPixelFormat.Rgb128BppFloat)
            {
                textureSourceFormat = useAlpha
                    ? TextureFormatSupport.SupportFor128BppPrgbaFloat
                    : TextureFormatSupport.SupportFor128BppRgbFloat;
            }
            else if (MilPixelFormatInfo.GetBitsPerPixel(bitmapSourceFormat) <= 32 && !useAlpha)
            {
                textureSourceFormat = TextureFormatSupport.SupportFor32BppBgr101010;
            }
            else
            {
                textureSourceFormat = TextureFormatSupport.SupportFor128BppPrgbaFloat;
            }
        }
        else
        {
            textureSourceFormat = useAlpha
                ? TextureFormatSupport.SupportFor32BppPbgra
                : TextureFormatSupport.SupportFor32BppBgr;
        }

        return textureSourceFormat == MilPixelFormat.Undefined
            ? Direct3D9Factory.UnsupportedPixelFormatHResult
            : Direct3D9Factory.SuccessHResult;
    }

    internal MultisampleType GetSupportedMultisampleType(MilPixelFormat destinationSurfaceFormat)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return destinationSurfaceFormat switch
        {
            MilPixelFormat.Bgr32Bpp => MultisampleSupport.Bgr32,
            MilPixelFormat.Pbgra32Bpp => MultisampleSupport.Pbgra32,
            _ => MultisampleSupport.Bgr101010
        };
    }

    internal Displaymode DisplayMode { get; }

    internal nint FocusWindow
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return field;
        }
    }

    internal bool IsExtended
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _isExtendedDevice;
        }
    }

    internal Pool ManagedPool
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _managedPool;
        }
    }

    internal bool IsUnusable => _displayInvalidSubmitted;

    internal bool IsInScene
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _inScene;
        }
    }

    internal bool IsEnsuringCorrectMultithreadedRendering
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _deviceEntryLock is not null;
        }
    }

    internal int UnusableReasonHResult { get; private set; }

    internal void Enter()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_deviceEntryLock is not null)
        {
            Monitor.Enter(_deviceEntryLock);
        }

        _entryCount++;
        _entryThreadId = PInvoke.GetCurrentThreadId();
    }

    internal void Leave()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_entryCount <= 0)
        {
            throw new InvalidOperationException("Direct3D device Leave was called without a matching Enter.");
        }

        LeaveDeviceEntry();
    }

    internal bool IsProtected(bool forceEntryConfirmation)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return (!forceEntryConfirmation && (BehaviorFlags & D3D9.CreateMultithreaded) == 0)
            || _entryThreadId == PInvoke.GetCurrentThreadId();
    }

    internal bool IsEntered()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _entryCount > 0;
    }

    internal int ResourceCount => _resourceManager.ResourceCount;

    internal Direct3D9ResourceManager ResourceManager
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _resourceManager;
        }
    }

    internal uint EnterUseContext()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _resourceManager.EnterUseContext();
    }

    internal void ExitUseContext(uint depth)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _resourceManager.ExitUseContext(depth);
    }

    internal bool IsInUseContext()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _resourceManager.IsInUseContext;
    }

    internal void Use(Direct3D9Resource resource)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _resourceManager.Use(resource);
    }

    internal void SetUnexpectedAdapterErrorNotification(Action<uint>? notification)
    {
        _unexpectedAdapterErrorNotification = notification;
    }

    internal void MarkUnusable(
        int reasonHResult = Direct3D9Factory.DisplayStateInvalidHResult,
        bool mayBeMultithreadedCall = false)
    {
        lock (_deviceLostSyncRoot)
        {
            if (!_displayInvalidSubmitted)
            {
                int submittedReason = UnusableReasonHResult < 0 ? UnusableReasonHResult : reasonHResult;
                if (submittedReason == Direct3D9Factory.DriverInternalErrorHResult)
                {
                    _unexpectedAdapterErrorNotification?.Invoke(AdapterOrdinal);
                }

                UnusableReasonHResult = Direct3D9Factory.DisplayStateInvalidHResult;
                _displayInvalidSubmitted = true;
            }

            if (_deviceLostProcessed || (mayBeMultithreadedCall && !IsProtected(forceEntryConfirmation: true)))
            {
                return;
            }

            ResetGpuMarkers();
            _deviceLostProcessed = true;
            _unusableNotification?.Invoke(this);
            _resourceManager.DestroyAllResources();
        }
    }

    private int HandleDeviceInternalError(int hResult)
    {
        if (hResult == Direct3D9Factory.DriverInternalErrorHResult)
        {
            UnusableReasonHResult = Direct3D9Factory.DriverInternalErrorHResult;
            return Direct3D9Factory.DisplayStateInvalidHResult;
        }

        return hResult;
    }

    internal int PreflightPresent()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (UnusableReasonHResult >= 0)
        {
            return 0;
        }

        MarkUnusable(UnusableReasonHResult);
        return UnusableReasonHResult;
    }

    internal Direct3D9DeviceState Present(Direct3D9SwapChain swapChain, Direct3D9PresentRequest request)
    {
        ArgumentNullException.ThrowIfNull(swapChain);
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        int preflightResult = PreflightPresent();
        if (preflightResult < 0)
        {
            return new Direct3D9DeviceState(preflightResult, Direct3D9DeviceStateSource.Present);
        }

        ConsumeFrameMetrics();

        bool restoreScene = _inScene;
        if (restoreScene)
        {
            int endSceneResult = EndScene();
            if (endSceneResult < 0)
            {
                return new Direct3D9DeviceState(endSceneResult, Direct3D9DeviceStateSource.Present);
            }
        }

        Direct3D9DeviceState state = swapChain.Present(request);
        bool presentProcessed = state.PresentProcessed;
        if (state.Kind == Direct3D9DeviceStateKind.Occluded)
        {
            _presentFailureDelay(100);
            _postWindowMessage(request.DestinationWindowOverride, _presentFailureWindowMessage);
            state = new Direct3D9DeviceState(0, Direct3D9DeviceStateSource.Present)
            {
                PresentProcessed = false
            };
        }

        if (restoreScene && state.Kind == Direct3D9DeviceStateKind.Operational)
        {
            int beginSceneResult = BeginScene();
            if (beginSceneResult < 0)
            {
                return new Direct3D9DeviceState(beginSceneResult, Direct3D9DeviceStateSource.Present);
            }
        }

        if (presentProcessed)
        {
            int markerResult = _recordSuccessfulPresent?.Invoke() ?? RecordSuccessfulPresent();
            if (markerResult < 0)
            {
                return new Direct3D9DeviceState(markerResult, Direct3D9DeviceStateSource.Present);
            }
        }

        return state;
    }

    private static uint RegisterWindowMessage(string messageName)
    {
        return PInvoke.RegisterWindowMessage(messageName);
    }

    private static void PostWindowMessage(nint window, uint message)
    {
        _ = PInvoke.PostMessage((Windows.Win32.Foundation.HWND) window, message, default, default);
    }

    internal Direct3D9DeviceState HandlePresentFailure(int hResult)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(hResult, 0);

        if (_currentRenderTarget != null)
        {
            _currentRenderTarget = null;
            if (_device != null && _dummyBackBuffer != null)
            {
                _ = SetNativeRenderTarget(_dummyBackBuffer);
            }

            ReleaseUseOfDepthStencilBuffer(_depthStencilSurfaceForCurrentRenderTarget);
        }

        int unusableReason = hResult;
        if (hResult is Direct3D9Factory.GenericFailureHResult or Direct3D9Factory.DriverInternalErrorHResult)
        {
            hResult = Direct3D9Factory.DeviceLostHResult;
            unusableReason = Direct3D9Factory.DriverInternalErrorHResult;
        }
        else if (hResult == Direct3D9Factory.InvalidArgumentHResult && IsLddmDevice)
        {
            hResult = Direct3D9Factory.NeedRecreateAndPresentHResult;
        }

        if (hResult is Direct3D9Factory.DeviceLostHResult
            or Direct3D9Factory.DeviceHungHResult
            or Direct3D9Factory.DeviceRemovedHResult)
        {
            MarkUnusable(unusableReason);
            hResult = Direct3D9Factory.DisplayStateInvalidHResult;
        }

        return new Direct3D9DeviceState(hResult, Direct3D9DeviceStateSource.Present);
    }

    internal Direct3D9DeviceState CheckDeviceState(nint destinationWindow = 0)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        if (_deviceEx is not null)
        {
            int result = _checkDeviceState is null
                ? CheckNativeDeviceState(destinationWindow)
                : _checkDeviceState(destinationWindow);

            if (result == Direct3D9Factory.PresentModeChangedHResult)
            {
                result = Direct3D9Factory.DisplayStateInvalidHResult;
            }
            else if (result == Direct3D9Factory.PresentOccludedHResult)
            {
                result = 0;
            }
            else if (result is Direct3D9Factory.DeviceLostHResult
                     or Direct3D9Factory.DeviceHungHResult
                     or Direct3D9Factory.DeviceRemovedHResult)
            {
                MarkUnusable(result);
                result = Direct3D9Factory.DisplayStateInvalidHResult;
            }

            return new Direct3D9DeviceState(result, Direct3D9DeviceStateSource.ExtendedCheck);
        }

        return new Direct3D9DeviceState(
            Direct3D9Factory.NotImplementedHResult,
            Direct3D9DeviceStateSource.CooperativeLevel);
    }

    private int CheckNativeDeviceState(nint destinationWindow)
    {
        ObjectDisposedException.ThrowIf(_deviceEx is null, this);

        void** extendedVtable = _deviceEx->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, nint, int> checkDeviceState =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, nint, int>) extendedVtable[128];
        return checkDeviceState(_deviceEx, destinationWindow);
    }

    internal void AdvanceFrame(uint frameNumber)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_frameNumber == frameNumber)
        {
            return;
        }

        _frameNumber = frameNumber;
        _resourceManager.EndFrame();
        _resourceManager.DestroyReleasedResourcesFromLastFrame();
        _resourceManager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
    }

    internal void CleanupFreedResources()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _resourceManager.DestroyReleasedResourcesFromLastFrame();
        _resourceManager.DestroyResources(Direct3D9DestroyResourcesStyle.WithoutDelay);
    }

    internal int GetRasterStatus(uint swapChainIndex, out RasterStatus rasterStatus)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ObjectDisposedException.ThrowIf(_device is null, this);

        rasterStatus = default;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, RasterStatus*, int> getRasterStatus =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, RasterStatus*, int>) vtable[19];
        fixed (RasterStatus* rasterStatusPointer = &rasterStatus)
        {
            return getRasterStatus(_device, swapChainIndex, rasterStatusPointer);
        }
    }

    internal int GetMinimalTextureDescription(
        ref SurfaceDesc description,
        bool paletteUsesAlpha,
        Direct3D9MinimalTextureDescriptionFlags flags)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);
        return Direct3D9TextureDescription.GetMinimal(
            _device,
            DisplayMode.Format,
            Capabilities,
            ref description,
            paletteUsesAlpha,
            flags);
    }

    internal int SetLinearPalette()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ObjectDisposedException.ThrowIf(_device is null, this);

        Span<uint> paletteEntries = stackalloc uint[256];
        for (int index = 0; index < paletteEntries.Length; index++)
        {
            paletteEntries[index] = (uint) index * 0x01010101;
        }

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint*, int> setPaletteEntries =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint*, int>) vtable[71];
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, int> setCurrentTexturePalette =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, int>) vtable[73];

        fixed (uint* paletteEntriesPointer = paletteEntries)
        {
            int result = setPaletteEntries(_device, 0, paletteEntriesPointer);
            if (result < 0)
            {
                return HandleDeviceInternalError(result);
            }
        }

        return HandleDeviceInternalError(setCurrentTexturePalette(_device, 0));
    }

    internal int GetNumQueuedPresents(out uint queuedPresentCount)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        queuedPresentCount = 0;
        if (_getNumQueuedPresents is not null)
        {
            return _getNumQueuedPresents(out queuedPresentCount);
        }

        if (!_gpuMarkersTested || !_gpuMarkersEnabled || Direct3D9HardwareCapabilities.HasWddmSupport(Capabilities))
        {
            return 0;
        }

        bool forceFlush = _successfulPresentsSinceGpuMarkerFlush >= SuccessfulPresentsBeforeGpuMarkerFlush;
        ConsumePresentMarkers(forceFlush);
        if (_activeGpuMarkers.Count > 2 && !forceFlush)
        {
            ConsumePresentMarkers(true);
        }

        if (_gpuMarkerConsumed)
        {
            queuedPresentCount = (uint)_activeGpuMarkers.Count;
        }

        return 0;
    }

    internal int InsertGpuMarker(ulong markerId)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (markerId < _lastGpuMarkerId || _device is null && _createGpuQuery is null)
        {
            return 0;
        }

        if (!_gpuMarkersTested)
        {
            _gpuMarkersEnabled = TryProbeGpuQuerySupport() >= 0;
            _gpuMarkersTested = true;
        }

        if (!_gpuMarkersEnabled)
        {
            _lastGpuMarkerId = markerId;
            return 0;
        }

        Direct3D9GpuMarker? marker = null;
        try
        {
            if (_freeGpuMarkers.Count > 0)
            {
                int index = _freeGpuMarkers.Count - 1;
                marker = _freeGpuMarkers[index];
                _freeGpuMarkers.RemoveAt(index);
                marker.Reset(markerId);
            }
            else
            {
                int createResult = TryCreateGpuQuery(out Direct3D9GpuQuery? query);
                if (createResult < 0 || query is null)
                {
                    if (!IsRecoverableGpuMarkerFailure(createResult))
                    {
                        DisableGpuMarkers();
                    }

                    return 0;
                }

                marker = new Direct3D9GpuMarker(query, markerId);
            }

            int issueResult = marker.InsertIntoCommandStream();
            if (issueResult < 0)
            {
                if (!IsRecoverableGpuMarkerFailure(issueResult))
                {
                    marker.Dispose();
                    marker = null;
                    DisableGpuMarkers();
                }

                return 0;
            }

            _activeGpuMarkers.Add(marker);
            marker = null;
        }
        finally
        {
            marker?.Dispose();
        }

        if (_activeGpuMarkers.Count > MaximumActiveGpuMarkers)
        {
            DisableGpuMarkers();
        }

        return 0;
    }

    internal int RecordSuccessfulPresent()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_gpuThrottlingDisabled || Direct3D9HardwareCapabilities.HasWddmSupport(Capabilities))
        {
            return 0;
        }

        _successfulPresentsSinceGpuMarkerFlush++;
        int timestampResult = _getPresentTimestamp(out ulong timestamp);
        return timestampResult < 0 ? timestampResult : InsertGpuMarker(timestamp);
    }

    internal int RecordSuccessfulPresent(ulong markerId)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_gpuThrottlingDisabled || Direct3D9HardwareCapabilities.HasWddmSupport(Capabilities))
        {
            return 0;
        }

        _successfulPresentsSinceGpuMarkerFlush++;
        return InsertGpuMarker(markerId);
    }

    private static int GetPresentTimestamp(out ulong timestamp)
    {
        timestamp = unchecked((ulong)Stopwatch.GetTimestamp());
        return 0;
    }

    private void ConsumePresentMarkers(bool forceFlush)
    {
        for (int index = _activeGpuMarkers.Count - 1; index >= 0; index--)
        {
            bool consumed = IsGpuMarkerConsumed(index, forceFlush);
            if (!_gpuMarkersEnabled)
            {
                return;
            }

            if (consumed)
            {
                FreeGpuMarkerAndPredecessors(index);
                break;
            }

            if (forceFlush)
            {
                forceFlush = false;
                _successfulPresentsSinceGpuMarkerFlush = 0;
            }
        }
    }

    private bool IsGpuMarkerConsumed(int index, bool flush)
    {
        int result = _activeGpuMarkers[index].CheckStatus(flush, out bool consumed);
        if (result == Direct3D9Factory.DeviceLostHResult)
        {
            consumed = true;
            result = 0;
        }

        if (result < 0)
        {
            DisableGpuMarkers();
            return true;
        }

        if (consumed)
        {
            _gpuMarkerConsumed = true;
        }

        return consumed;
    }

    private void FreeGpuMarkerAndPredecessors(int index)
    {
        _lastConsumedGpuMarkerId = _activeGpuMarkers[index].Id;
        int consumedCount = index + 1;
        for (int markerIndex = 0; markerIndex < consumedCount; markerIndex++)
        {
            _freeGpuMarkers.Add(_activeGpuMarkers[markerIndex]);
        }

        _activeGpuMarkers.RemoveRange(0, consumedCount);
    }

    private int TryProbeGpuQuerySupport()
    {
        if (_createGpuQuery is not null)
        {
            int createResult = _createGpuQuery(out Direct3D9GpuQuery? query);
            query?.Dispose();
            return createResult;
        }

        if (_device is null)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Querytype, IDirect3DQuery9**, int> createQuery =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Querytype, IDirect3DQuery9**, int>)vtable[118];
        return createQuery(_device, Querytype.Event, null);
    }

    private int TryCreateGpuQuery(out Direct3D9GpuQuery? query)
    {
        if (_createGpuQuery is not null)
        {
            int createResult = _createGpuQuery(out query);
            if (createResult < 0)
            {
                query?.Dispose();
                query = null;
            }

            return createResult;
        }

        query = null;
        if (_device is null)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Querytype, IDirect3DQuery9**, int> createQuery =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Querytype, IDirect3DQuery9**, int>)vtable[118];
        IDirect3DQuery9* nativeQuery = null;
        int result = createQuery(_device, Querytype.Event, &nativeQuery);
        if (result < 0)
        {
            Direct3D9Factory.Release(nativeQuery);
            return result;
        }

        if (nativeQuery is null)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        query = new Direct3D9GpuQuery(nativeQuery);
        return result;
    }

    private static bool IsRecoverableGpuMarkerFailure(int result)
    {
        return result is Direct3D9Factory.DeviceLostHResult or Direct3D9Factory.NotAvailableHResult;
    }

    private void DisableGpuMarkers()
    {
        _gpuMarkersEnabled = false;
        ResetGpuMarkers();
    }

    private void ResetGpuMarkers()
    {
        _lastConsumedGpuMarkerId = _lastGpuMarkerId;
        foreach (Direct3D9GpuMarker marker in _freeGpuMarkers)
        {
            marker.Dispose();
        }

        _freeGpuMarkers.Clear();
        foreach (Direct3D9GpuMarker marker in _activeGpuMarkers)
        {
            marker.Dispose();
        }

        _activeGpuMarkers.Clear();
    }

    internal int WaitForVBlank(uint swapChainIndex)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_deviceEx is null && _waitForVBlank is null)
        {
            return Direct3D9Factory.NoHardwareDeviceHResult;
        }

        if (!_hardwareVBlankTested)
        {
            _hardwareVBlankTested = true;
            long start = _getTimestamp();
            int result = WaitForVBlankCore(swapChainIndex);
            long elapsed = _getTimestamp() - start;
            _hardwareVBlankSupported = result >= 0
                && elapsed >= 0
                && elapsed * 1000 < _timestampFrequency * 75;
            return _hardwareVBlankSupported ? result : Direct3D9Factory.NoHardwareDeviceHResult;
        }

        if (!_hardwareVBlankSupported)
        {
            return Direct3D9Factory.NoHardwareDeviceHResult;
        }

        int waitResult = WaitForVBlankCore(swapChainIndex);
        return waitResult >= 0 ? waitResult : Direct3D9Factory.NoHardwareDeviceHResult;
    }

    private int WaitForVBlankCore(uint swapChainIndex)
    {
        if (_waitForVBlank is not null)
        {
            return _waitForVBlank(swapChainIndex);
        }

        void** vtable = _deviceEx->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, uint, int> waitForVBlank =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, uint, int>) vtable[124];
        return waitForVBlank(_deviceEx, swapChainIndex);
    }

    internal int CheckRenderTargetFormat(Format format)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _checkRenderTargetFormat is null
            ? CheckRenderTargetFormat(format, static _ => Direct3D9Factory.SuccessHResult, out _)
            : _checkRenderTargetFormat(format);
    }

    internal int CheckRenderTargetFormat(
        Format format,
        Func<Direct3D9TargetFormatTestStatus, int> test,
        out int? getDeviceContextHResult)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(test);
        using Direct3D9DeviceEntryGuard deviceEntry = new(this);

        int getEntryResult = GetRenderTargetFormatTestEntry(format, out Direct3D9TargetFormatTestStatus? status);
        if (getEntryResult < 0)
        {
            getDeviceContextHResult = null;
            return getEntryResult;
        }

        int hResult = status!.TestRenderTargetFormat(testStatus =>
        {
            int depthStencilResult = CheckDepthStencilMatch(format);
            if (depthStencilResult < 0)
            {
                return depthStencilResult;
            }

            Direct3D9Surface? renderTarget = null;
            Direct3D9Texture? lockableTexture = null;
            Direct3D9SwapChain? swapChain = null;
            try
            {
                if (FocusWindow == 0)
                {
                    int createRenderTargetResult = CreateRenderTargetForFormatTest(format, out renderTarget);
                    if (createRenderTargetResult < 0)
                    {
                        return createRenderTargetResult;
                    }
                }
                else
                {
                    int swapChainResult = TestLockableSwapChainForFormatTest(
                        format,
                        testStatus,
                        out renderTarget,
                        out swapChain);
                    if (swapChainResult < 0)
                    {
                        return swapChainResult;
                    }
                }

                int setRenderTargetResult = SetRenderTargetForFormatTest(renderTarget);
                if (setRenderTargetResult < 0)
                {
                    return setRenderTargetResult;
                }

                int beginSceneResult = BeginSceneForFormatTest();
                if (beginSceneResult < 0)
                {
                    return beginSceneResult;
                }

                int sceneResult = 0;
                try
                {
                    int clearDepthStencilResult = ClearDepthStencilSurfaceForFormatTest();
                    if (clearDepthStencilResult < 0)
                    {
                        sceneResult = clearDepthStencilResult;
                    }
                    else
                    {
                        int createTextureResult = CreateLockableTextureForFormatTest(out lockableTexture);
                        if (createTextureResult < 0)
                        {
                            sceneResult = createTextureResult;
                        }
                        else
                        {
                            int renderTextureResult = RenderTextureForFormatTest(lockableTexture);
                            sceneResult = renderTextureResult < 0
                                ? renderTextureResult
                                : test(testStatus);
                        }
                    }
                }
                finally
                {
                    int endSceneResult = EndSceneForFormatTest();
                    if (sceneResult >= 0 && endSceneResult < 0)
                    {
                        sceneResult = endSceneResult;
                    }
                }

                return sceneResult;
            }
            finally
            {
                renderTarget?.Dispose();
                lockableTexture?.Dispose();
                swapChain?.Dispose();
            }
        });
        getDeviceContextHResult = status.WasGetDeviceContextTested
            ? status.GetDeviceContextHResult
            : null;
        return hResult;
    }

    private int GetRenderTargetFormatTestEntry(
        Format format,
        out Direct3D9TargetFormatTestStatus? status)
    {
        if (_targetFormatTestStatuses.TryGetValue(format, out status))
        {
            return Direct3D9Factory.SuccessHResult;
        }

        status = null;
        return Direct3D9Factory.InvalidArgumentHResult;
    }

    private int CreateRenderTargetForFormatTest(Format format, out Direct3D9Surface? renderTarget)
    {
        renderTarget = null;
        if (_createRenderTargetForFormatTest is not null)
        {
            return _createRenderTargetForFormatTest(
                128,
                128,
                format,
                MultisampleType.MultisampleNone,
                0,
                true);
        }

        return TryCreateRenderTarget(
            128,
            128,
            format,
            MultisampleType.MultisampleNone,
            0,
            true,
            out renderTarget);
    }

    private int TestLockableSwapChainForFormatTest(
        Format format,
        Direct3D9TargetFormatTestStatus testStatus,
        out Direct3D9Surface? renderTarget,
        out Direct3D9SwapChain? swapChain)
    {
        renderTarget = null;
        swapChain = null;
        PresentParameters presentParameters = new(
            backBufferWidth: 128,
            backBufferHeight: 128,
            backBufferFormat: format,
            backBufferCount: 1,
            multiSampleType: MultisampleType.MultisampleNone,
            multiSampleQuality: 0,
            swapEffect: Swapeffect.Copy,
            hDeviceWindow: FocusWindow,
            windowed: true,
            enableAutoDepthStencil: false,
            autoDepthStencilFormat: Format.Unknown,
            flags: unchecked((uint) D3D9.PresentflagLockableBackbuffer),
            fullScreenRefreshRateInHz: 0,
            presentationInterval: D3D9.PresentIntervalImmediate);
        if (_testLockableSwapChainForFormatTest is not null)
        {
            return _testLockableSwapChainForFormatTest(presentParameters, testStatus);
        }

        int result = TryCreateAdditionalSwapChain(presentParameters, out swapChain);
        if (result < 0)
        {
            return result;
        }

        result = swapChain.TryGetBackBuffer(0, out renderTarget);
        if (result < 0)
        {
            return result;
        }

        _ = renderTarget.TestGetDeviceContext(testStatus);
        return result;
    }

    private int SetRenderTargetForFormatTest(Direct3D9Surface? renderTarget)
    {
        if (_setRenderTargetForFormatTest is not null)
        {
            return _setRenderTargetForFormatTest();
        }

        return SetRenderTarget(renderTarget
            ?? throw new InvalidOperationException("The render-target format test did not create a surface."));
    }

    private int ClearDepthStencilSurfaceForFormatTest()
    {
        if (_clearDepthStencilSurfaceForFormatTest is not null)
        {
            return _clearDepthStencilSurfaceForFormatTest();
        }

        return SetDepthStencilSurface(null);
    }

    private int CreateLockableTextureForFormatTest(out Direct3D9Texture? texture)
    {
        const uint width = 128;
        const uint height = 128;
        const uint levels = 1;
        const uint usage = 0;
        const Format format = Format.A8R8G8B8;
        Pool pool = ManagedPool;

        texture = null;
        if (_createLockableTextureForFormatTest is not null)
        {
            return _createLockableTextureForFormatTest(width, height, levels, usage, format, pool);
        }

        return TryCreateLockableTexture(width, height, usage, format, pool, out texture);
    }

    private int BeginSceneForFormatTest()
    {
        if (_beginSceneForFormatTest is not null)
        {
            return _beginSceneForFormatTest();
        }

        return _inScene ? Direct3D9Factory.SuccessHResult : BeginScene();
    }

    private int RenderTextureForFormatTest(Direct3D9Texture? texture)
    {
        if (_renderTextureForFormatTest is not null)
        {
            return _renderTextureForFormatTest();
        }

        return RenderTexture(texture
            ?? throw new InvalidOperationException("The render-target format test did not create a texture."));
    }

    private int EndSceneForFormatTest()
    {
        return _endSceneForFormatTest?.Invoke() ?? EndScene();
    }

    private int CheckDepthStencilMatch(Format renderTargetFormat)
    {
        if (DeviceType != Devtype.Hal)
        {
            return 0;
        }

        if (_checkDepthStencilMatch is not null)
        {
            return _checkDepthStencilMatch(
                AdapterOrdinal,
                Devtype.Hal,
                DisplayMode.Format,
                renderTargetFormat,
                Format.D24S8);
        }

        if (_direct3D is null)
        {
            throw new InvalidOperationException("Direct3D depth-stencil matching requires an IDirect3D9 interface.");
        }

        void** vtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Format, Format, Format, int> checkDepthStencilMatch =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Format, Format, Format, int>) vtable[12];
        return checkDepthStencilMatch(
            _direct3D,
            AdapterOrdinal,
            Devtype.Hal,
            DisplayMode.Format,
            renderTargetFormat,
            Format.D24S8);
    }

    internal int SetRenderTarget(Direct3D9Surface renderTarget)
    {
        ArgumentNullException.ThrowIfNull(renderTarget);
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        IDirect3DSurface9* nativeRenderTarget = _setRenderTarget is null
            ? renderTarget.SurfaceForDeviceCall
            : renderTarget.NativeIdentity;
        if (nativeRenderTarget != null && nativeRenderTarget == _currentRenderTarget)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        int result;
        if (_device != null && _inScene)
        {
            result = EndScene();
            if (result < 0)
            {
                return result;
            }
        }

        SurfaceDesc description = _getRenderTargetDescription?.Invoke(renderTarget) ?? renderTarget.GetDescription();
        result = _setRenderTarget?.Invoke(renderTarget) ?? SetNativeRenderTarget(nativeRenderTarget);
        if (result < 0)
        {
            return HandleDeviceInternalError(result);
        }

        _currentRenderTarget = nativeRenderTarget;
        _isClipSet = false;
        Direct3D9PointAndSizeRect viewport = new(
            0,
            0,
            unchecked((int) description.Width),
            unchecked((int) description.Height));
        _targetSurface = viewport;
        if ((Capabilities.RasterCaps & (uint) D3D9.PrastercapsScissortest) != 0)
        {
            _scissorState.ScissorRectChanged(viewport);
        }

        result = SetViewport(viewport);
        if (result < 0)
        {
            if (_currentRenderTarget != null)
            {
                ReleaseUseOfRenderTarget(_currentRenderTarget);
            }

            return HandleDeviceInternalError(result);
        }

        result = _setSurfaceToClippingMatrix?.Invoke(viewport) ?? SetSurfaceToClippingMatrix(viewport);
        if (result >= 0 && _device != null)
        {
            result = BeginScene();
        }

        if (result < 0 && _currentRenderTarget != null)
        {
            ReleaseUseOfRenderTarget(_currentRenderTarget);
        }

        return HandleDeviceInternalError(result);
    }

    private int SetNativeRenderTarget(IDirect3DSurface9* renderTarget)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DSurface9*, int> setRenderTarget =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DSurface9*, int>) vtable[37];
        return setRenderTarget(_device, 0, renderTarget);
    }

    internal int ClearTarget(uint color)
    {
        return Clear(1, color, 0f);
    }

    internal int ClearDepth(float depth)
    {
        return Clear(2, 0, depth);
    }

    private int Clear(uint flags, uint color, float depth)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if ((_setDepthStencilSurface is not null || _device is not null) &&
            IsDepthStencilSurfaceSmallerThan(
                unchecked((uint) _targetSurface.Width),
                unchecked((uint) _targetSurface.Height)))
        {
            int result = SetDepthStencilSurfaceInline(null, 0, 0);
            if (result < 0)
            {
                return HandleDeviceInternalError(result);
            }
        }

        if (_clear is not null)
        {
            return HandleDeviceInternalError(_clear(0, flags, color, depth, 0));
        }

        ObjectDisposedException.ThrowIf(_device is null, this);
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, Rect*, uint, uint, float, uint, int> clear =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, Rect*, uint, uint, float, uint, int>) vtable[43];
        return HandleDeviceInternalError(clear(_device, 0, null, flags, color, depth, 0));
    }

    internal int SetViewport(Direct3D9PointAndSizeRect viewport)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        Viewport9 nativeViewport = new(
            unchecked((uint) viewport.X),
            unchecked((uint) viewport.Y),
            unchecked((uint) viewport.Width),
            unchecked((uint) viewport.Height),
            0f,
            1f);
        int result = _setViewport?.Invoke(nativeViewport) ?? SetNativeViewport(nativeViewport);
        _viewport = viewport;
        return result;
    }

    private int SetNativeViewport(Viewport9 viewport)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Viewport9*, int> setViewport =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Viewport9*, int>) vtable[47];
        return setViewport(_device, &viewport);
    }

    internal int SetDepthStencilSurface(IDirect3DSurface9* depthStencilSurface)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        int result;
        if (depthStencilSurface is not null)
        {
            result = SetRenderState(Renderstatetype.Zenable, 1);
        }
        else
        {
            result = SetRenderState(Renderstatetype.Zenable, 0);
            if (result >= 0)
            {
                result = SetRenderState(Renderstatetype.Stencilenable, 0);
            }
        }

        if (result >= 0)
        {
            _depthStencilSurfaceForCurrentRenderTarget = depthStencilSurface;
            result = SetDepthStencilSurfaceInline(depthStencilSurface, 0, 0);
        }

        if (result < 0)
        {
            _ = SetRenderState(Renderstatetype.Zenable, 0);
            _ = SetRenderState(Renderstatetype.Stencilenable, 0);
            _ = SetDepthStencilSurfaceInline(null, 0, 0);
        }

        return HandleDeviceInternalError(result);
    }

    internal int SetDepthStencilSurfaceForCurrentRenderTarget(
        IDirect3DSurface9* depthStencilSurface,
        uint width,
        uint height)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _depthStencilSurfaceForCurrentRenderTarget = depthStencilSurface;
        return SetDepthStencilSurfaceInline(depthStencilSurface, width, height);
    }

    internal int SetDepthStencilSurfaceInline(
        IDirect3DSurface9* depthStencilSurface,
        uint width,
        uint height)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_isDepthStencilSurfaceKnown && _depthStencilSurface == depthStencilSurface)
        {
            return 0;
        }

        return ForceSetDepthStencilSurface(depthStencilSurface, width, height);
    }

    internal int ForceSetDepthStencilSurface(
        IDirect3DSurface9* depthStencilSurface,
        uint width,
        uint height)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        int result = (_setDepthStencilSurface ?? SetNativeDepthStencilSurface)(depthStencilSurface);
        if (result >= 0)
        {
            _depthStencilSurface = depthStencilSurface;
            _depthStencilSurfaceWidth = width;
            _depthStencilSurfaceHeight = height;
            _isDepthStencilSurfaceKnown = true;
        }
        else
        {
            _isDepthStencilSurfaceKnown = false;
        }

        return result;
    }

    internal int ReleaseUseOfDepthStencilBuffer(IDirect3DSurface9* depthStencilSurface)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (depthStencilSurface == null)
        {
            return 0;
        }

        int result = 0;
        if (_isDepthStencilSurfaceKnown && _depthStencilSurface == depthStencilSurface)
        {
            result = ForceSetDepthStencilSurface(null, 0, 0);
        }

        if (_depthStencilSurfaceForCurrentRenderTarget == depthStencilSurface)
        {
            _depthStencilSurfaceForCurrentRenderTarget = null;
        }

        return result;
    }

    private void OnSurfaceReleased(nint surface, uint usage)
    {
        if (_isDisposed)
        {
            return;
        }

        IDirect3DSurface9* nativeSurface = (IDirect3DSurface9*) surface;
        if ((usage & D3D9.UsageRendertarget) != 0)
        {
            ReleaseUseOfRenderTarget(nativeSurface);
        }

        if ((usage & D3D9.UsageDepthstencil) != 0)
        {
            ReleaseUseOfDepthStencilBuffer(nativeSurface);
        }
    }

    private void ReleaseUseOfRenderTarget(IDirect3DSurface9* renderTarget)
    {
        if (renderTarget != _currentRenderTarget)
        {
            return;
        }

        _currentRenderTarget = null;
        if (_inScene)
        {
            _ = EndSceneWithoutErrorMapping();
        }

        if (_device != null && _dummyBackBuffer != null)
        {
            _ = SetNativeRenderTarget(_dummyBackBuffer);
        }

        ReleaseUseOfDepthStencilBuffer(_depthStencilSurfaceForCurrentRenderTarget);
    }

    internal bool IsDepthStencilSurfaceSmallerThan(uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_isDepthStencilSurfaceKnown)
        {
            return true;
        }

        return _depthStencilSurface is not null &&
            (_depthStencilSurfaceWidth < width || _depthStencilSurfaceHeight < height);
    }

    private int SetNativeDepthStencilSurface(IDirect3DSurface9* depthStencilSurface)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, int> setDepthStencilSurface =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, int>) vtable[39];
        return setDepthStencilSurface(_device, depthStencilSurface);
    }

    internal int BeginScene()
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, int> beginScene =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, int>) vtable[41];
        int result = beginScene(_device);
        if (result >= 0)
        {
            _inScene = true;
        }

        return HandleDeviceInternalError(result);
    }

    internal int EndScene()
    {
        return HandleDeviceInternalError(EndSceneWithoutErrorMapping());
    }

    private int EndSceneWithoutErrorMapping()
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, int> endScene =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, int>) vtable[42];
        int result = endScene(_device);
        if (result >= 0)
        {
            _inScene = false;
        }

        return result;
    }

    internal int GetRenderState(Renderstatetype state, out uint value)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!IsSupportedRenderState(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        int stateIndex = checked((int) state);
        value = _renderStates[stateIndex];
        return _areRenderStatesKnown[stateIndex] ? 0 : Direct3D9Factory.GenericFailureHResult;
    }

    internal int SetRenderState(Renderstatetype state, uint value)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!IsSupportedRenderState(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        int stateIndex = checked((int) state);
        if (_areRenderStatesKnown[stateIndex] && _renderStates[stateIndex] == value)
        {
            return 0;
        }

        return ForceSetRenderState(state, value);
    }

    internal int ForceSetRenderState(Renderstatetype state, uint value)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint) state, 210u);

        int stateIndex = checked((int) state);
        int result = (_setRenderState ?? SetNativeRenderState)(state, value);
        _renderStates[stateIndex] = result >= 0 ? value : 0;
        _areRenderStatesKnown[stateIndex] = result >= 0;
        return result;
    }

    private int SetNativeRenderState(Renderstatetype state, uint value)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Renderstatetype, uint, int> setRenderState =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Renderstatetype, uint, int>) vtable[57];
        return setRenderState(_device, state, value);
    }

    private static bool IsSupportedRenderState(Renderstatetype state)
    {
        return state is Renderstatetype.Diffusematerialsource or
            Renderstatetype.Specularmaterialsource or
            Renderstatetype.Fillmode or
            Renderstatetype.Alphablendenable or
            Renderstatetype.Srcblend or
            Renderstatetype.Destblend or
            Renderstatetype.Blendfactor or
            Renderstatetype.Colorwriteenable or
            Renderstatetype.Scissortestenable or
            Renderstatetype.Zenable or
            Renderstatetype.Stencilenable or
            Renderstatetype.Zwriteenable or
            Renderstatetype.Cullmode or
            Renderstatetype.Zfunc or
            Renderstatetype.Multisampleantialias;
    }

    internal int SetTransform(Transformstatetype state, Matrix4x4 matrix)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (IsTransformKnown(state, matrix))
        {
            return 0;
        }

        int result = ForceSetTransform(state, matrix);
        if (result >= 0)
        {
            _twoDimensionalTransformsApplied = false;
            _twoDimensionalVertexShaderTransformApplied = false;
        }

        return result;
    }

    internal int GetTransform(Transformstatetype state, out Matrix4x4 matrix)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (state == WorldTransform)
        {
            matrix = _worldTransform;
            return _isWorldTransformKnown ? 0 : Direct3D9Factory.GenericFailureHResult;
        }

        uint stateValue = (uint) state;
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(stateValue, 256u);
        int stateIndex = checked((int) stateValue);
        matrix = _nonWorldTransforms[stateIndex];
        return _areNonWorldTransformsKnown[stateIndex] ? 0 : Direct3D9Factory.GenericFailureHResult;
    }

    internal int ForceSetTransform(Transformstatetype state, Matrix4x4 matrix)
    {
        int result = (_setTransform ?? SetNativeTransform)(state, matrix);
        if (result >= 0)
        {
            _twoDimensionalTransformsApplied = false;
            _twoDimensionalVertexShaderTransformApplied = false;
        }

        if (state == WorldTransform)
        {
            _isWorldTransformKnown = result >= 0;
            if (result >= 0)
            {
                _worldTransform = matrix;
            }

            return result;
        }

        uint stateValue = (uint) state;
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(stateValue, 256u);
        int stateIndex = checked((int) stateValue);
        _areNonWorldTransformsKnown[stateIndex] = result >= 0;
        if (result >= 0)
        {
            _nonWorldTransforms[stateIndex] = matrix;
            if (state == Transformstatetype.View)
            {
                _viewTransform = matrix;
            }
            else if (state == Transformstatetype.Projection)
            {
                _projectionTransform = matrix;
            }
        }

        return result;
    }

    private bool IsTransformKnown(Transformstatetype state, Matrix4x4 matrix)
    {
        if (state == WorldTransform)
        {
            return _isWorldTransformKnown && _worldTransform == matrix;
        }

        uint stateValue = (uint) state;
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(stateValue, 256u);
        int stateIndex = checked((int) stateValue);
        return _areNonWorldTransformsKnown[stateIndex] && _nonWorldTransforms[stateIndex] == matrix;
    }

    private int SetNativeTransform(Transformstatetype state, Matrix4x4 matrix)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Transformstatetype, Matrix4x4*, int> setTransform =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Transformstatetype, Matrix4x4*, int>) vtable[44];
        return setTransform(_device, state, &matrix);
    }

    internal int SetMaterial(Material9 material)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Material9*, int> setMaterial =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Material9*, int>) vtable[49];
        return setMaterial(_device, &material);
    }

    internal int SetPixelShader(IDirect3DPixelShader9* pixelShader)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_isPixelShaderKnown && _pixelShader == pixelShader)
        {
            return 0;
        }

        return ForceSetPixelShader(pixelShader);
    }

    internal int ForceSetPixelShader(IDirect3DPixelShader9* pixelShader)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        int result = (_setPixelShader ?? SetNativePixelShader)(pixelShader);
        _isPixelShaderKnown = result >= 0;
        _pixelShader = result >= 0 ? pixelShader : null;
        return result;
    }

    private int SetNativePixelShader(IDirect3DPixelShader9* pixelShader)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DPixelShader9*, int> setPixelShader =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DPixelShader9*, int>) vtable[107];
        return setPixelShader(_device, pixelShader);
    }

    internal int SetPixelShaderConstants(uint startRegister, ReadOnlySpan<Vector4> constants)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _pixelShaderConstantState.SetConstants(
            startRegister,
            constants,
            _setPixelShaderConstants ?? SetNativePixelShaderConstants);
    }

    private int SetNativePixelShaderConstants(uint startRegister, ReadOnlySpan<Vector4> constants)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, float*, uint, int> setPixelShaderConstantF =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, float*, uint, int>) vtable[109];
        fixed (Vector4* constantsPointer = constants)
        {
            return setPixelShaderConstantF(_device, startRegister, (float*) constantsPointer, (uint) constants.Length);
        }
    }

    internal int SetPixelShaderInt4Constant(uint register, ReadOnlySpan<int> constant)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _pixelShaderInt4ConstantState.SetConstant(
            register,
            constant,
            _setPixelShaderInt4Constant ?? SetNativePixelShaderInt4Constant);
    }

    private int SetNativePixelShaderInt4Constant(uint register, ReadOnlySpan<int> constant)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, int*, uint, int> setPixelShaderConstantI =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, int*, uint, int>) vtable[111];
        fixed (int* constantPointer = constant)
        {
            return setPixelShaderConstantI(_device, register, constantPointer, 1);
        }
    }

    internal int SetPixelShaderBoolConstant(uint register, int constant)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _pixelShaderBoolConstantState.SetConstant(
            register,
            constant,
            _setPixelShaderBoolConstant ?? SetNativePixelShaderBoolConstant);
    }

    private int SetNativePixelShaderBoolConstant(uint register, int constant)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, int*, uint, int> setPixelShaderConstantB =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, int*, uint, int>) vtable[113];
        return setPixelShaderConstantB(_device, register, &constant, 1);
    }

    internal int SetVertexShader(IDirect3DVertexShader9* vertexShader)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_isVertexShaderKnown && _vertexShader == vertexShader)
        {
            return 0;
        }

        return ForceSetVertexShader(vertexShader);
    }

    internal int ForceSetVertexShader(IDirect3DVertexShader9* vertexShader)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        int result = (_setVertexShader ?? SetNativeVertexShader)(vertexShader);
        _isVertexShaderKnown = result >= 0;
        _vertexShader = result >= 0 ? vertexShader : null;
        return result;
    }

    private int SetNativeVertexShader(IDirect3DVertexShader9* vertexShader)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DVertexShader9*, int> setVertexShader =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DVertexShader9*, int>) vtable[92];
        return setVertexShader(_device, vertexShader);
    }

    internal int SetVertexShaderConstants(uint startRegister, ReadOnlySpan<Vector4> constants)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _vertexShaderConstantState.SetConstants(
            startRegister,
            constants,
            ref _twoDimensionalVertexShaderTransformApplied,
            _twoDimensionalVertexShaderStartRegister,
            SetNativeVertexShaderConstants);
    }

    private int SetCachedVertexShaderConstants(uint startRegister, Matrix4x4 matrix)
    {
        Matrix4x4 constantsMatrix = matrix;
        ReadOnlySpan<Vector4> constants = new(&constantsMatrix, 4);
        return _vertexShaderConstantState.SetConstants(
            startRegister,
            constants,
            ref _twoDimensionalVertexShaderTransformApplied,
            _twoDimensionalVertexShaderStartRegister,
            _setVertexShaderConstants is null
                ? SetNativeVertexShaderConstants
                : (register, _) => _setVertexShaderConstants(register, matrix));
    }

    private int ForceSetVertexShaderConstants(uint startRegister, Matrix4x4 matrix)
    {
        if (_setVertexShaderConstants is not null)
        {
            int result = _setVertexShaderConstants(startRegister, matrix);
            ReadOnlySpan<Vector4> injectedConstants = new(&matrix, 4);
            return _vertexShaderConstantState.UpdateAfterForceSet(startRegister, injectedConstants, result);
        }

        ReadOnlySpan<Vector4> constants = new(&matrix, 4);
        return _vertexShaderConstantState.ForceSetConstants(
            startRegister,
            constants,
            SetNativeVertexShaderConstants);
    }

    private int SetNativeVertexShaderConstants(uint startRegister, ReadOnlySpan<Vector4> constants)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, float*, uint, int> setVertexShaderConstantF =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, float*, uint, int>) vtable[94];
        fixed (Vector4* constantsPointer = constants)
        {
            return setVertexShaderConstantF(_device, startRegister, (float*) constantsPointer, (uint) constants.Length);
        }
    }

    internal int SetStreamSource(
        uint streamNumber,
        IDirect3DVertexBuffer9* streamData,
        uint offsetInBytes,
        uint stride)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        bool isCachedStream = streamNumber == 0 && offsetInBytes == 0;
        if (isCachedStream &&
            _isStreamSourceKnown &&
            _streamSourceVertexBuffer == streamData &&
            _streamSourceVertexStride == stride)
        {
            return 0;
        }

        if (isCachedStream)
        {
            return ForceSetStreamSource(streamData, stride);
        }

        int result = (_setStreamSource ?? SetNativeStreamSource)(streamNumber, streamData, offsetInBytes, stride);
        if (streamNumber == 0)
        {
            _streamSourceVertexBuffer = null;
            _streamSourceVertexStride = 0;
            _isStreamSourceKnown = false;
        }

        return result;
    }

    internal int ForceSetStreamSource(IDirect3DVertexBuffer9* streamData, uint stride)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        int result = (_setStreamSource ?? SetNativeStreamSource)(0, streamData, 0, stride);
        _streamSourceVertexBuffer = result >= 0 ? streamData : null;
        _streamSourceVertexStride = result >= 0 ? stride : 0;
        _isStreamSourceKnown = result >= 0;
        return result;
    }

    private int SetNativeStreamSource(
        uint streamNumber,
        IDirect3DVertexBuffer9* streamData,
        uint offsetInBytes,
        uint stride)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DVertexBuffer9*, uint, uint, int> setStreamSource =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DVertexBuffer9*, uint, uint, int>) vtable[100];
        return setStreamSource(_device, streamNumber, streamData, offsetInBytes, stride);
    }

    internal int SetIndices(IDirect3DIndexBuffer9* indexData)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return ForceSetIndices(indexData);
    }

    internal int ForceSetIndices(IDirect3DIndexBuffer9* indexData)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return (_setIndices ?? SetNativeIndices)(indexData);
    }

    private int SetNativeIndices(IDirect3DIndexBuffer9* indexData)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DIndexBuffer9*, int> setIndices =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DIndexBuffer9*, int>) vtable[104];
        return setIndices(_device, indexData);
    }

    internal Direct3D9HardwareVertexBuffer? Get3DVertexBuffer()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _hardwareVertexBuffer;
    }

    internal Direct3D9HardwareIndexBuffer? Get3DIndexBuffer()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _hardwareIndexBuffer;
    }

    internal int InitializeDynamicBuffers()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_hardwareIndexBuffer is not null || _hardwareVertexBuffer is not null)
        {
            return 0;
        }

        int result = Direct3D9HardwareIndexBuffer.TryCreate(this, HardwareIndexBufferCapacity, out _hardwareIndexBuffer);
        if (result < 0)
        {
            return HandleDeviceInternalError(result);
        }

        result = Direct3D9HardwareVertexBuffer.TryCreate(this, HardwareVertexBufferCapacity, out _hardwareVertexBuffer);
        if (result < 0)
        {
            _hardwareIndexBuffer.Dispose();
            _hardwareIndexBuffer = null;
        }

        return HandleDeviceInternalError(result);
    }

    internal int DrawBox(Direct3D9Box box, Fillmode fillMode, uint color)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        int result = GetRenderState(Renderstatetype.Fillmode, out uint originalFillMode);
        if (result < 0)
        {
            return HandleDeviceInternalError(result);
        }

        result = GetRenderState(Renderstatetype.Zfunc, out uint originalDepthTest);
        if (result < 0)
        {
            return HandleDeviceInternalError(result);
        }

        Direct3D9VertexXyzDiffuseUv2* vertices = stackalloc Direct3D9VertexXyzDiffuseUv2[8]
        {
            new(box.X, box.Y, box.Z, color),
            new(box.X + box.LengthX, box.Y, box.Z, color),
            new(box.X + box.LengthX, box.Y + box.LengthY, box.Z, color),
            new(box.X, box.Y + box.LengthY, box.Z, color),
            new(box.X, box.Y, box.Z + box.LengthZ, color),
            new(box.X + box.LengthX, box.Y, box.Z + box.LengthZ, color),
            new(box.X + box.LengthX, box.Y + box.LengthY, box.Z + box.LengthZ, color),
            new(box.X, box.Y + box.LengthY, box.Z + box.LengthZ, color)
        };
        ushort* indices = stackalloc ushort[]
        {
            0, 2, 1, 0, 3, 2,
            4, 5, 6, 4, 6, 7,
            0, 4, 3, 3, 4, 7,
            1, 2, 5, 2, 6, 5,
            2, 3, 6, 3, 7, 6,
            0, 1, 4, 1, 5, 4
        };

        result = SetRenderState(Renderstatetype.Fillmode, (uint) fillMode);
        if (result >= 0)
        {
            result = SetRenderState(Renderstatetype.Zfunc, (uint) Cmpfunc.Always);
        }

        if (result >= 0)
        {
            result = SetFlexibleVertexFormat((uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex2));
        }

        if (result >= 0)
        {
            result = Direct3D9Level1DeviceTest.SetAlphaSolidBrushState(
                Capabilities.MaxTextureBlendStages,
                SetRenderState,
                () => SetPixelShader(null),
                () => SetVertexShader(null),
                SetTextureStageState,
                stage => SetTexture(stage, null));
        }

        if (result >= 0)
        {
            result = _hardwareVertexBuffer is null || _hardwareIndexBuffer is null
                ? Direct3D9Factory.GenericFailureHResult
                : DrawIndexedTriangleListUp(
                    _hardwareVertexBuffer,
                    _hardwareIndexBuffer,
                    8,
                    12,
                    indices,
                    vertices,
                    (uint) sizeof(Direct3D9VertexXyzDiffuseUv2));
        }

        if (result >= 0)
        {
            result = SetRenderState(Renderstatetype.Fillmode, originalFillMode);
            if (result >= 0)
            {
                result = SetRenderState(Renderstatetype.Zfunc, originalDepthTest);
            }
        }

        if (result < 0)
        {
            _ = SetRenderState(Renderstatetype.Fillmode, originalFillMode);
            _ = SetRenderState(Renderstatetype.Zfunc, originalDepthTest);
        }

        return HandleDeviceInternalError(result);
    }

    internal int DrawIndexedTriangleListUp(
        Direct3D9HardwareVertexBuffer vertexBuffer,
        Direct3D9HardwareIndexBuffer indexBuffer,
        uint vertexCount,
        uint primitiveCount,
        ushort* indexData,
        void* vertexStreamZeroData,
        uint vertexStreamZeroStride)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        ArgumentNullException.ThrowIfNull(indexBuffer);

        uint indexCount = checked(primitiveCount * 3);
        void* lockedVertices = null;
        ushort* lockedIndices = null;
        bool vertexBufferLocked = false;
        bool indexBufferLocked = false;
        uint startVertex = 0;
        uint startIndex = 0;
        int result = vertexBuffer.Lock(vertexCount, vertexStreamZeroStride, out lockedVertices, out startVertex);
        if (result >= 0)
        {
            vertexBufferLocked = true;
            result = indexBuffer.Lock(indexCount, out lockedIndices, out startIndex);
            indexBufferLocked = result >= 0;
        }

        try
        {
            if (!vertexBufferLocked || !indexBufferLocked)
            {
                result = SetStreamSource(0, null, 0, 0);
                if (result >= 0)
                {
                    result = SetIndices(null);
                }

                if (result >= 0)
                {
                    result = DrawNativeIndexedTriangleListUp(
                        vertexCount,
                        primitiveCount,
                        indexData,
                        vertexStreamZeroData,
                        vertexStreamZeroStride);
                }

                if (result >= 0)
                {
                    UpdateMetrics(vertexCount, primitiveCount);
                }

                return HandleDeviceInternalError(result);
            }

            nuint vertexByteCount = checked((nuint) vertexCount * vertexStreamZeroStride);
            Buffer.MemoryCopy(vertexStreamZeroData, lockedVertices, vertexByteCount, vertexByteCount);
            result = vertexBuffer.Unlock(vertexCount);
            if (result < 0)
            {
                return HandleDeviceInternalError(result);
            }

            vertexBufferLocked = false;
            nuint indexByteCount = checked((nuint) indexCount * sizeof(ushort));
            Buffer.MemoryCopy(indexData, lockedIndices, indexByteCount, indexByteCount);
            result = indexBuffer.Unlock();
            if (result < 0)
            {
                return HandleDeviceInternalError(result);
            }

            indexBufferLocked = false;
            result = SetStreamSource(
                0,
                vertexBuffer.DangerousGetDirect3DVertexBuffer(),
                0,
                vertexStreamZeroStride);
            if (result >= 0)
            {
                result = SetIndices(indexBuffer.DangerousGetDirect3DIndexBuffer());
            }

            if (result >= 0)
            {
                result = DrawIndexedTriangleList(startVertex, 0, vertexCount, startIndex, primitiveCount);
            }

            return HandleDeviceInternalError(result);
        }
        finally
        {
            if (vertexBufferLocked)
            {
                _ = vertexBuffer.Unlock(vertexCount);
            }

            if (indexBufferLocked)
            {
                _ = indexBuffer.Unlock();
            }
        }
    }

    private int DrawNativeIndexedTriangleListUp(
        uint vertexCount,
        uint primitiveCount,
        ushort* indexData,
        void* vertexStreamZeroData,
        uint vertexStreamZeroStride)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, uint, uint, uint, void*, Format, void*, uint, int>
            drawIndexedPrimitiveUp =
                (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, uint, uint, uint, void*, Format, void*, uint, int>) vtable[84];
        return drawIndexedPrimitiveUp(
            _device,
            Primitivetype.Trianglelist,
            0,
            vertexCount,
            primitiveCount,
            indexData,
            Format.Index16,
            vertexStreamZeroData,
            vertexStreamZeroStride);
    }

    internal int DrawIndexedTriangleList(
        uint baseVertexIndex,
        uint minIndex,
        uint vertexCount,
        uint startIndex,
        uint primitiveCount)
    { 
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        int result = (_drawIndexedTriangleList ?? DrawNativeIndexedTriangleList)(
            baseVertexIndex,
            minIndex,
            vertexCount,
            startIndex,
            primitiveCount);
        if (result >= 0)
        {
            UpdateMetrics(vertexCount, primitiveCount);
        }

        return HandleDeviceInternalError(result);
    }

    private int DrawNativeIndexedTriangleList(
        uint baseVertexIndex,
        uint minIndex,
        uint vertexCount,
        uint startIndex,
        uint primitiveCount)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, int, uint, uint, uint, uint, int> drawIndexedPrimitive =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, int, uint, uint, uint, uint, int>) vtable[82];
        return drawIndexedPrimitive(
            _device,
            Primitivetype.Trianglelist,
            unchecked((int) baseVertexIndex),
            minIndex,
            vertexCount,
            startIndex,
            primitiveCount);
    }

    internal int StartPrimitive(out Direct3D9PrimitiveVertexBuffer vertexBuffer)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        vertexBuffer = _primitiveVertexBufferDuv2;
        vertexBuffer.Clear();
        return SetFlexibleVertexFormat((uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex2));
    }

    internal int StartPrimitive(out Direct3D9PrimitiveVertexBufferDuv6 vertexBuffer)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        vertexBuffer = _primitiveVertexBufferDuv6;
        vertexBuffer.Clear();
        return SetFlexibleVertexFormat((uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex6));
    }

    internal int StartPrimitive(out Direct3D9PrimitiveVertexBufferXyzNormalDiffuseSpecularUv4 vertexBuffer)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        vertexBuffer = _primitiveVertexBufferXyzNormalDiffuseSpecularUv4;
        vertexBuffer.Clear();
        return SetFlexibleVertexFormat((uint) (D3D9.FvfXyz | D3D9.FvfNormal | D3D9.FvfDiffuse | D3D9.FvfSpecular | D3D9.FvfTex4));
    }

    internal Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzDiffuseUv2> GetVertexBufferXyzDuv2()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _vertexBufferXyzDuv2;
    }

    internal Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzDiffuseUv8> GetVertexBufferXyzRhwDuv8()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _vertexBufferXyzRhwDuv8;
    }

    internal int EndPrimitiveFan(Direct3D9PrimitiveVertexBuffer vertexBuffer)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        return FlushBufferFan(vertexBuffer.Vertices);
    }

    internal int EndPrimitiveFan(Direct3D9PrimitiveVertexBufferDuv6 vertexBuffer)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        return FlushBufferFan(vertexBuffer.Vertices);
    }

    internal int EndPrimitiveFan(Direct3D9PrimitiveVertexBufferXyzNormalDiffuseSpecularUv4 vertexBuffer)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        return FlushBufferFan(vertexBuffer.Vertices);
    }

    private int FlushBufferFan<TVertex>(ReadOnlySpan<TVertex> vertices)
        where TVertex : unmanaged
    {
        if (vertices.Length <= 2)
        {
            return 0;
        }

        fixed (TVertex* vertexPointer = vertices)
        {
            return DrawPrimitiveUp(
                Primitivetype.Trianglefan,
                checked((uint) vertices.Length - 2),
                vertexPointer,
                (uint) sizeof(TVertex));
        }
    }

    internal int DrawTriangleList(uint startVertex, uint primitiveCount)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        int result = (_drawTriangleList ?? DrawNativeTriangleList)(startVertex, primitiveCount);
        if (result >= 0)
        {
            UpdateMetrics(primitiveCount * 3, primitiveCount);
        }

        return HandleDeviceInternalError(result);
    }

    private int DrawNativeTriangleList(uint startVertex, uint primitiveCount)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        return DrawNativePrimitive(Primitivetype.Trianglelist, startVertex, primitiveCount);
    }

    internal int DrawTriangleStrip(uint startVertex, uint primitiveCount)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        int result = DrawNativePrimitive(Primitivetype.Trianglestrip, startVertex, primitiveCount);
        if (result >= 0)
        {
            UpdateMetrics(primitiveCount + 2, primitiveCount);
        }

        return HandleDeviceInternalError(result);
    }

    internal int DrawVideoToSurface(Direct3D9BeginVideoRender beginRender, out nint bitmapSource)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(beginRender);

        int result = beginRender(this, out bitmapSource);
        return HandleDeviceInternalError(result);
    }

    private int DrawNativePrimitive(Primitivetype primitiveType, uint startVertex, uint primitiveCount)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, uint, uint, int> drawPrimitive =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, uint, uint, int>) vtable[81];
        return drawPrimitive(_device, primitiveType, startVertex, primitiveCount);
    }

    internal Direct3D9PointAndSizeRect ScissorRectCache => _scissorState.ScissorRect;

    internal Direct3D9PointAndSizeRect ViewportCache => _viewport;

    internal Direct3D9PointAndSizeRect ClipCache => _clip;

    internal bool IsClipSet => _isClipSet;

    internal Direct3D9PointAndSizeRect GetClipRect()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _isClipSet ? _clip : _targetSurface;
    }

    internal int SetClipRect(Direct3D9SurfaceRect? clipRect)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        Direct3D9PointAndSizeRect? newClip = null;
        if (clipRect.HasValue)
        {
            Direct3D9SurfaceRect value = clipRect.GetValueOrDefault();
            Direct3D9PointAndSizeRect requestedClip = new(
                value.Left,
                value.Top,
                unchecked(value.Right - value.Left),
                unchecked(value.Bottom - value.Top));
            if (!TryIntersect(_targetSurface, requestedClip, out Direct3D9PointAndSizeRect intersection))
            {
                return Direct3D9Factory.ClippedToEmptyHResult;
            }

            if (!_isClipSet || intersection != _clip)
            {
                newClip = intersection;
            }
        }
        else if (_isClipSet)
        {
            newClip = _targetSurface;
        }

        if (!newClip.HasValue)
        {
            return 0;
        }

        Direct3D9PointAndSizeRect valueToApply = newClip.GetValueOrDefault();
        int result;
        if ((Capabilities.RasterCaps & (uint) D3D9.PrastercapsScissortest) != 0)
        {
            Direct3D9PointAndSizeRect? scissorRect = valueToApply == _targetSurface ? null : valueToApply;
            result = _setScissorRect?.Invoke(scissorRect) ?? SetScissorRect(scissorRect);
        }
        else
        {
            result = SetViewport(valueToApply);
            if (result >= 0)
            {
                result = _setSurfaceToClippingMatrix?.Invoke(valueToApply) ?? SetSurfaceToClippingMatrix(valueToApply);
            }
        }

        if (result < 0)
        {
            return result;
        }

        _isClipSet = clipRect.HasValue;
        _clip = valueToApply;
        return result;
    }

    private static bool TryIntersect(
        Direct3D9PointAndSizeRect first,
        Direct3D9PointAndSizeRect second,
        out Direct3D9PointAndSizeRect intersection)
    {
        long left = Math.Max((long) first.X, second.X);
        long top = Math.Max((long) first.Y, second.Y);
        long right = Math.Min((long) first.X + first.Width, (long) second.X + second.Width);
        long bottom = Math.Min((long) first.Y + first.Height, (long) second.Y + second.Height);
        if (right <= left || bottom <= top)
        {
            intersection = default;
            return false;
        }

        intersection = new Direct3D9PointAndSizeRect(
            checked((int) left),
            checked((int) top),
            checked((int) (right - left)),
            checked((int) (bottom - top)));
        return true;
    }

    private int SetSurfaceToClippingMatrix(Direct3D9PointAndSizeRect viewport)
    {
        float reciprocalWidth = 1f / viewport.Width;
        float reciprocalHeight = 1f / viewport.Height;
        Matrix4x4 projection = Matrix4x4.Identity;
        projection.M11 = 2f * reciprocalWidth;
        projection.M41 = -((viewport.X * projection.M11) + 1f + reciprocalWidth);
        projection.M22 = -2f * reciprocalHeight;
        projection.M42 = (-viewport.Y * projection.M22) + 1f + reciprocalHeight;

        Define2DTransforms(projection);
        return Set2DTransformForFixedFunction();
    }

    internal void Define2DTransforms(Matrix4x4 projection)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _twoDimensionalProjection = projection;
        _twoDimensionalTransformsApplied = false;
        _twoDimensionalVertexShaderTransformApplied = false;
        _twoDimensionalVertexShaderStartRegister = uint.MaxValue;
    }

    internal int Set2DTransformForFixedFunction()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return Direct3D9TransformState.Set2DTransformsForFixedFunction(
            _twoDimensionalProjection,
            ref _twoDimensionalTransformsApplied,
            ForceSetTransform);
    }

    internal int Set2DTransformForVertexShader(uint startRegister)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return Direct3D9TransformState.Set2DTransformForVertexShader(
            startRegister,
            ref _twoDimensionalVertexShaderTransformApplied,
            ref _twoDimensionalVertexShaderStartRegister,
            GetTransform,
            ForceSetVertexShaderConstants);
    }

    internal int Set3DTransforms(
        Matrix4x4 world,
        Matrix4x4 view,
        Matrix4x4 projection,
        Matrix4x4 viewportProjectionModifier)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return Direct3D9TransformState.Set3DTransforms(
            world,
            view,
            projection,
            viewportProjectionModifier,
            _twoDimensionalProjection,
            SetTransform);
    }

    internal int Set3DTransformForVertexShader(uint startRegister)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return Direct3D9TransformState.Set3DTransformForVertexShader(
            startRegister,
            GetTransform,
            SetCachedVertexShaderConstants);
    }

    internal void SetClipSet(bool isClipSet)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _isClipSet = isClipSet;
    }

    internal void ResetScissorAndClipCache()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _scissorState.InitializeDefaultCache();
        _isClipSet = false;
    }

    internal void InvalidateScissorRect()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _scissorState.Invalidate();
    }

    internal void ScissorRectChanged(Direct3D9PointAndSizeRect scissorRect)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _scissorState.ScissorRectChanged(scissorRect);
    }

    internal int SetScissorRect(Direct3D9PointAndSizeRect? scissorRect)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);
        return _scissorState.SetScissorRect(scissorRect);
    }

    private int SetNativeScissorRect(Direct3D9SurfaceRect scissorRect)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Direct3D9SurfaceRect*, int> setScissorRect =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Direct3D9SurfaceRect*, int>) vtable[75];
        return setScissorRect(_device, &scissorRect);
    }

    internal int DisableTextureStage(uint stage)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        uint maximumTextureBlendStages = Math.Min(8u, Capabilities.MaxTextureBlendStages);
        return Direct3D9TextureStageState.DisableTextureStage(
            stage,
            maximumTextureBlendStages,
            SetTextureStageState);
    }

    internal int DisableTextureTransform(uint stage)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return HandleDeviceInternalError(
            SetTextureStageState(stage, Texturestagestatetype.Texturetransformflags, 0));
    }

    internal int SetTextureStageState(uint stage, Texturestagestatetype state, uint value)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        uint maximumTextureBlendStage = Math.Min(8u, Capabilities.MaxTextureBlendStages);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(stage, maximumTextureBlendStage);

        if (stage == maximumTextureBlendStage)
        {
            return 0;
        }

        if (state is not (Texturestagestatetype.Colorop or
            Texturestagestatetype.Colorarg1 or
            Texturestagestatetype.Colorarg2 or
            Texturestagestatetype.Alphaop or
            Texturestagestatetype.Alphaarg1 or
            Texturestagestatetype.Alphaarg2 or
            Texturestagestatetype.Texturetransformflags or
            Texturestagestatetype.Texcoordindex))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        int stateIndex = checked((int) (stage * 33 + (uint) state));
        if (_areTextureStageStatesKnown[stateIndex] && _textureStageStates[stateIndex] == value)
        {
            return 0;
        }

        return ForceSetTextureStageState(stage, state, value);
    }

    internal int ForceSetTextureStageState(uint stage, Texturestagestatetype state, uint value)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(stage, 8u);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint) state, 33u);

        if (state == Texturestagestatetype.Texcoordindex && value != stage)
        {
            _areTextureCoordinateIndicesDefault = false;
        }

        int stateIndex = checked((int) (stage * 33 + (uint) state));
        int result = (_setTextureStageState ?? SetNativeTextureStageState)(stage, state, value);
        _textureStageStates[stateIndex] = result >= 0 ? value : 0;
        _areTextureStageStatesKnown[stateIndex] = result >= 0;
        return result;
    }

    internal int SetDefaultTextureCoordinateIndices()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_areTextureCoordinateIndicesDefault)
        {
            return 0;
        }

        int result = 0;
        uint maximumTextureBlendStage = Math.Min(8u, Capabilities.MaxTextureBlendStages);
        for (uint stage = 0; stage < maximumTextureBlendStage; stage++)
        {
            result = SetTextureStageState(stage, Texturestagestatetype.Texcoordindex, stage);
            if (result < 0)
            {
                return result;
            }
        }

        _areTextureCoordinateIndicesDefault = true;
        return result;
    }

    private int SetNativeTextureStageState(uint stage, Texturestagestatetype state, uint value)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, Texturestagestatetype, uint, int> setTextureStageState =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, Texturestagestatetype, uint, int>) vtable[67];
        return setTextureStageState(_device, stage, state, value);
    }

    internal int SetSamplerState(uint sampler, Samplerstatetype state, uint value)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        uint maximumSampler = Math.Min(8u, Capabilities.MaxTextureBlendStages);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sampler, maximumSampler);

        if (sampler == maximumSampler)
        {
            return 0;
        }

        if (state is not (Samplerstatetype.Magfilter or
            Samplerstatetype.Minfilter or
            Samplerstatetype.Mipfilter or
            Samplerstatetype.Addressu or
            Samplerstatetype.Addressv or
            Samplerstatetype.Bordercolor))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        int stateIndex = checked((int) (sampler * 14 + (uint) state));
        if (_areSamplerStatesKnown[stateIndex] && _samplerStates[stateIndex] == value)
        {
            return 0;
        }

        return ForceSetSamplerState(sampler, state, value);
    }

    internal int ForceSetSamplerState(uint sampler, Samplerstatetype state, uint value)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(sampler, 8u);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint) state, 14u);

        int stateIndex = checked((int) (sampler * 14 + (uint) state));
        int result = (_setSamplerState ?? SetNativeSamplerState)(sampler, state, value);
        _samplerStates[stateIndex] = result >= 0 ? value : 0;
        _areSamplerStatesKnown[stateIndex] = result >= 0;
        return result;
    }

    private int SetNativeSamplerState(uint sampler, Samplerstatetype state, uint value)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, Samplerstatetype, uint, int> setSamplerState =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, Samplerstatetype, uint, int>) vtable[69];
        return setSamplerState(_device, sampler, state, value);
    }

    internal int SetTexture(uint stage, Direct3D9Texture? texture)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        IDirect3DBaseTexture9* nativeTexture = null;
        if (texture is not null)
        {
            Use(texture);
            nativeTexture = (IDirect3DBaseTexture9*) texture.Texture;
        }

        return HandleDeviceInternalError(SetD3DTexture(stage, nativeTexture));
    }

    internal int SetD3DTexture(uint stage, IDirect3DBaseTexture9* texture)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(stage, (uint) _textures.Length);

        if (_areTexturesKnown[stage] && _textures[stage] == (nint) texture)
        {
            return 0;
        }

        return ForceSetTexture(stage, texture);
    }

    internal int ForceSetTexture(uint stage, IDirect3DBaseTexture9* texture)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(stage, (uint) _textures.Length);

        int result = (_setTexture ?? SetNativeTexture)(stage, texture);
        _textures[stage] = result >= 0 ? (nint) texture : 0;
        _areTexturesKnown[stage] = result >= 0;
        return result;
    }

    private int SetNativeTexture(uint stage, IDirect3DBaseTexture9* texture)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DBaseTexture9*, int> setTexture =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DBaseTexture9*, int>) vtable[65];
        return setTexture(_device, stage, texture);
    }

    internal int SetFlexibleVertexFormat(uint flexibleVertexFormat)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (IsFlexibleVertexFormatSet(flexibleVertexFormat))
        {
            return 0;
        }

        return ForceSetFlexibleVertexFormat(flexibleVertexFormat);
    }

    internal bool IsFlexibleVertexFormatSet(uint flexibleVertexFormat)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _isFlexibleVertexFormatKnown && _flexibleVertexFormat == flexibleVertexFormat;
    }

    internal int ForceSetFlexibleVertexFormat(uint flexibleVertexFormat)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        int result = (_setFlexibleVertexFormat ?? SetNativeFlexibleVertexFormat)(flexibleVertexFormat);
        _isFlexibleVertexFormatKnown = result >= 0;
        _flexibleVertexFormat = result >= 0 ? flexibleVertexFormat : 0;
        return result;
    }

    private int SetNativeFlexibleVertexFormat(uint flexibleVertexFormat)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, int> setFlexibleVertexFormat =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, int>) vtable[89];
        return setFlexibleVertexFormat(_device, flexibleVertexFormat);
    }

    internal int DrawPrimitiveUp(
        Primitivetype primitiveType,
        uint primitiveCount,
        void* vertexStreamZeroData,
        uint vertexStreamZeroStride)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || (_device is null && _drawPrimitiveUp is null), this);

        uint vertexCount = primitiveType switch
        {
            Primitivetype.Linelist => primitiveCount * 2,
            Primitivetype.Trianglelist => primitiveCount * 3,
            Primitivetype.Trianglefan or Primitivetype.Trianglestrip => primitiveCount + 2,
            _ => 0
        };
        if (vertexCount == 0)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        if (primitiveCount > Capabilities.MaxPrimitiveCount ||
            (Capabilities.MaxPrimitiveCount == ushort.MaxValue && vertexCount > ushort.MaxValue))
        {
            return DrawLargePrimitiveUp(primitiveType, primitiveCount, vertexStreamZeroData, vertexStreamZeroStride);
        }

        void* lockedVertices = null;
        uint startVertex = 0;
        bool vertexBufferLocked = false;
        int result = _hardwareVertexBuffer?.Lock(
            vertexCount,
            vertexStreamZeroStride,
            out lockedVertices,
            out startVertex) ?? Direct3D9Factory.GenericFailureHResult;
        vertexBufferLocked = result >= 0;

        try
        {
            if (!vertexBufferLocked)
            {
                result = SetStreamSource(0, null, 0, 0);
                if (result >= 0)
                {
                    result = DrawNativePrimitiveUp(
                        primitiveType,
                        primitiveCount,
                        vertexStreamZeroData,
                        vertexStreamZeroStride);
                }

                if (result >= 0)
                {
                    UpdateMetrics(vertexCount, primitiveCount);
                }

                return HandleDeviceInternalError(result);
            }

            nuint vertexByteCount = checked((nuint) vertexCount * vertexStreamZeroStride);
            Buffer.MemoryCopy(vertexStreamZeroData, lockedVertices, vertexByteCount, vertexByteCount);
            result = _hardwareVertexBuffer!.Unlock(vertexCount);
            if (result < 0)
            {
                return HandleDeviceInternalError(result);
            }

            vertexBufferLocked = false;
            result = SetStreamSource(
                0,
                _hardwareVertexBuffer.DangerousGetDirect3DVertexBuffer(),
                0,
                vertexStreamZeroStride);
            if (result >= 0)
            {
                result = DrawNativePrimitive(primitiveType, startVertex, primitiveCount);
            }

            if (result >= 0)
            {
                UpdateMetrics(vertexCount, primitiveCount);
            }

            return HandleDeviceInternalError(result);
        }
        finally
        {
            if (vertexBufferLocked)
            {
                _ = _hardwareVertexBuffer!.Unlock(vertexCount);
            }
        }
    }

    internal int DrawLargePrimitiveUp(
        Primitivetype primitiveType,
        uint primitiveCount,
        void* vertexStreamZeroData,
        uint vertexStreamZeroStride)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        uint maximumPrimitiveCount = primitiveType switch
        {
            Primitivetype.Linelist => ushort.MaxValue / 2,
            Primitivetype.Trianglelist => ushort.MaxValue / 3,
            Primitivetype.Trianglestrip => ushort.MaxValue - 2,
            _ => 0
        };
        if (maximumPrimitiveCount == 0)
        {
            return Direct3D9Factory.NotImplementedHResult;
        }
        if (Capabilities.MaxPrimitiveCount != ushort.MaxValue)
        {
            maximumPrimitiveCount = Capabilities.MaxPrimitiveCount;
        }
        if (maximumPrimitiveCount == 0)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        byte* vertexStreamPosition = (byte*) vertexStreamZeroData;
        while (primitiveCount > 0)
        {
            uint drawPrimitiveCount = Math.Min(primitiveCount, maximumPrimitiveCount);
            int result = SetStreamSource(0, null, 0, 0);
            if (result < 0)
            {
                return HandleDeviceInternalError(result);
            }

            result = DrawNativePrimitiveUp(
                primitiveType,
                drawPrimitiveCount,
                vertexStreamPosition,
                vertexStreamZeroStride);
            if (result < 0)
            {
                return HandleDeviceInternalError(result);
            }

            uint drawnVertexCount = primitiveType switch
            {
                Primitivetype.Linelist => drawPrimitiveCount * 2,
                Primitivetype.Trianglelist => drawPrimitiveCount * 3,
                Primitivetype.Trianglestrip => drawPrimitiveCount + 2,
                _ => 0
            };
            UpdateMetrics(drawnVertexCount, drawPrimitiveCount);

            primitiveCount -= drawPrimitiveCount;
            uint verticesToAdvance = primitiveType switch
            {
                Primitivetype.Linelist => drawPrimitiveCount * 2,
                Primitivetype.Trianglelist => drawPrimitiveCount * 3,
                Primitivetype.Trianglestrip => drawPrimitiveCount,
                _ => 0
            };
            vertexStreamPosition += verticesToAdvance * vertexStreamZeroStride;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private void UpdateMetrics(uint vertexCount, uint primitiveCount)
    {
        if (_consumeFrameMetrics is null)
        {
            return;
        }

        _metricsVerticesPerFrame += vertexCount;
        _metricsPrimitivesPerFrame += primitiveCount;
    }

    private void ConsumeFrameMetrics()
    {
        if (_consumeFrameMetrics is null)
        {
            return;
        }

        Direct3D9FrameMetrics metrics = new(_metricsVerticesPerFrame, _metricsPrimitivesPerFrame);
        _consumeFrameMetrics(metrics);
        _metricsVerticesPerFrame = 0;
        _metricsPrimitivesPerFrame = 0;
    }

    private int DrawNativePrimitiveUp(
        Primitivetype primitiveType,
        uint primitiveCount,
        void* vertexStreamZeroData,
        uint vertexStreamZeroStride)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || (_device is null && _drawPrimitiveUp is null), this);
        return (_drawPrimitiveUp ?? DrawNativePrimitiveUpCore)(
            primitiveType,
            primitiveCount,
            vertexStreamZeroData,
            vertexStreamZeroStride);
    }

    private int DrawNativePrimitiveUpCore(
        Primitivetype primitiveType,
        uint primitiveCount,
        void* vertexStreamZeroData,
        uint vertexStreamZeroStride)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, uint, void*, uint, int> drawPrimitiveUp =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, uint, void*, uint, int>) vtable[83];
        return drawPrimitiveUp(_device, primitiveType, primitiveCount, vertexStreamZeroData, vertexStreamZeroStride);
    }

    internal int RenderTexture(Direct3D9Texture texture)
    {
        return RenderTexture(
            texture,
            new Direct3D9PointAndSizeRect(0, 0, 128, 128),
            Direct3D9TextureBlendMode.Default);
    }

    internal int RenderTexture(
        Direct3D9Texture texture,
        Direct3D9PointAndSizeRect destination,
        Direct3D9TextureBlendMode blendMode = Direct3D9TextureBlendMode.Default)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint) blendMode, (uint) Direct3D9TextureBlendMode.AddColors);
        ArgumentOutOfRangeException.ThrowIfNegative(destination.X);
        ArgumentOutOfRangeException.ThrowIfNegative(destination.Y);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(destination.Width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(destination.Height);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint) destination.Width, texture.Width);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint) destination.Height, texture.Height);

        int result = SetTexture(0, texture);
        if (result < 0)
        {
            return HandleDeviceInternalError(result);
        }

        result = SetRenderState(Renderstatetype.Diffusematerialsource, (uint) Materialcolorsource.Color1);
        if (result < 0)
        {
            return HandleDeviceInternalError(result);
        }

        result = SetRenderState(Renderstatetype.Specularmaterialsource, (uint) Materialcolorsource.Color1);
        if (result < 0)
        {
            return HandleDeviceInternalError(result);
        }

        result = Direct3D9Level1DeviceTest.SetNearestNeighborDiffuseTextureState(
            Capabilities.MaxTextureBlendStages,
            blendMode,
            () => SetPixelShader(null),
            () => SetVertexShader(null),
            SetSamplerState,
            SetRenderState,
            SetTextureStageState);
        if (result < 0)
        {
            return HandleDeviceInternalError(result);
        }

        result = StartPrimitive(out Direct3D9PrimitiveVertexBuffer vertexBuffer);
        if (result < 0)
        {
            return HandleDeviceInternalError(result);
        }

        result = vertexBuffer.GetNewVertices(out Span<Direct3D9VertexXyzDiffuseUv2> vertices);
        if (result < 0)
        {
            return HandleDeviceInternalError(result);
        }

        float left = destination.X;
        float top = destination.Y;
        float right = left + destination.Width;
        float bottom = top + destination.Height;
        float uRight = (float) destination.Width / texture.Width;
        float vBottom = (float) destination.Height / texture.Height;
        vertices[0] = new Direct3D9VertexXyzDiffuseUv2(left, top, 0.5f, uint.MaxValue, 0, 0);
        vertices[1] = new Direct3D9VertexXyzDiffuseUv2(left, bottom, 0.5f, uint.MaxValue, 0, vBottom);
        vertices[2] = new Direct3D9VertexXyzDiffuseUv2(right, bottom, 0.5f, uint.MaxValue, uRight, vBottom);
        vertices[3] = new Direct3D9VertexXyzDiffuseUv2(right, top, 0.5f, uint.MaxValue, uRight, 0);
        return EndPrimitiveFan(vertexBuffer);
    }

    internal Direct3D9Texture CreateTexture(
        uint width,
        uint height,
        uint levels = 1,
        uint usage = 0,
        Format format = Format.A8R8G8B8,
        Pool pool = Pool.Managed)
    {
        int result = TryCreateTexture(width, height, levels, usage, format, pool, out Direct3D9Texture? texture);
        Marshal.ThrowExceptionForHR(result);
        return texture ?? throw new InvalidOperationException("Direct3D texture creation returned a null interface pointer.");
    }

    private int TryCreateTexture(
        uint width,
        uint height,
        uint levels,
        uint usage,
        Format format,
        Pool pool,
        out Direct3D9Texture? texture)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        texture = null;
        if (_deviceLostProcessed)
        {
            return Direct3D9Factory.DisplayStateInvalidHResult;
        }

        return TryCreateTextureCore(width, height, levels, usage, format, pool, null, out texture);
    }

    internal int TryCreateTexture(
        SurfaceDesc description,
        uint levels,
        ref nint sharedHandle,
        out Direct3D9Texture? texture)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        texture = null;
        if (_deviceLostProcessed)
        {
            return Direct3D9Factory.DisplayStateInvalidHResult;
        }

        fixed (nint* sharedHandlePointer = &sharedHandle)
        {
            return TryCreateTextureCore(
                description.Width,
                description.Height,
                levels,
                description.Usage,
                description.Format,
                description.Pool,
                sharedHandlePointer,
                out texture);
        }
    }

    internal int TryCreateBitmapTexture(
        IDirect3DTexture9* existingTexture,
        bool isEvictable,
        out Direct3D9Texture? texture)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return Direct3D9Texture.TryCreate(_resourceManager, existingTexture, isEvictable, out texture);
    }

    internal int TryCreateRenderTargetTexture(
        SurfaceDesc description,
        out Direct3D9Texture? texture)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return TryCreateTexture(
            description.Width,
            description.Height,
            levels: 1,
            description.Usage,
            description.Format,
            description.Pool,
            out texture);
    }

    internal int TryCreateBitmapTexture(
        Direct3D9BitmapTextureRequirements requirements,
        bool isEvictable,
        out Direct3D9Texture? texture)
    {
        SurfaceDesc description = requirements.Description;
        int result = TryCreateTexture(
            description.Width,
            description.Height,
            requirements.Levels,
            description.Usage,
            description.Format,
            description.Pool,
            out texture);
        if (result >= 0 && isEvictable && texture is not null)
        {
            texture.SetAsEvictable();
        }

        return result;
    }

    internal int TryCreateLockableTexture(
        uint width,
        uint height,
        uint usage,
        Format format,
        Pool pool,
        out Direct3D9Texture? texture)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        uint levels = (usage & D3D9.UsageAutogenmipmap) != 0 ? 0u : 1u;
        return TryCreateTextureCore(width, height, levels, usage, format, pool, null, out texture);
    }

    internal int TryCreateSystemMemoryReferenceTexture(
        SurfaceDesc surfaceDescription,
        void* pixels,
        out Direct3D9SystemMemoryReferenceTexture? texture)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        texture = null;
        IDirect3DTexture9* direct3DTexture = null;
        void* pixelReference = pixels;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, uint, Format, Pool, IDirect3DTexture9**, void**, int> createTexture =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, uint, Format, Pool, IDirect3DTexture9**, void**, int>) vtable[23];
        int result = createTexture(
            _device,
            surfaceDescription.Width,
            surfaceDescription.Height,
            1,
            surfaceDescription.Usage,
            surfaceDescription.Format,
            surfaceDescription.Pool,
            &direct3DTexture,
            &pixelReference);
        if (result < 0)
        {
            Direct3D9Factory.Release(direct3DTexture);
            return HandleDeviceInternalError(result);
        }
        if (direct3DTexture is null)
        {
            throw new InvalidOperationException("Direct3D system-memory reference texture creation returned a null interface pointer.");
        }

        texture = new Direct3D9SystemMemoryReferenceTexture(direct3DTexture);
        return result;
    }

    internal int TryCreateSystemMemoryUpdateSurface(
        uint width,
        uint height,
        Format format,
        void* pixels,
        out Direct3D9SystemMemoryUpdateSurface? surface)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        surface = null;
        IDirect3DSurface9* direct3DSurface = null;
        int result;
        if (Direct3D9HardwareCapabilities.HasWddmSupport(Capabilities))
        {
            void* pixelReference = pixels;
            void** vtable = _device->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, Pool, IDirect3DSurface9**, void**, int> createOffscreenPlainSurface =
                (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, Pool, IDirect3DSurface9**, void**, int>) vtable[36];
            result = createOffscreenPlainSurface(
                _device,
                width,
                height,
                format,
                Pool.Systemmem,
                &direct3DSurface,
                pixels is null ? null : &pixelReference);
            if (result < 0)
            {
                Direct3D9Factory.Release(direct3DSurface);
                return HandleDeviceInternalError(result);
            }
        }
        else
        {
            if (_deviceLostProcessed)
            {
                return Direct3D9Factory.DisplayStateInvalidHResult;
            }

            IDirect3DTexture9* direct3DTexture = null;
            void** deviceVtable = _device->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, uint, Format, Pool, IDirect3DTexture9**, void**, int> createTexture =
                (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, uint, Format, Pool, IDirect3DTexture9**, void**, int>) deviceVtable[23];
            result = createTexture(
                _device,
                width,
                height,
                1,
                0,
                format,
                Pool.Systemmem,
                &direct3DTexture,
                null);
            if (result < 0)
            {
                Direct3D9Factory.Release(direct3DTexture);
                return HandleDeviceInternalError(result);
            }
            if (direct3DTexture is null)
            {
                throw new InvalidOperationException("Direct3D system-memory update texture creation returned a null interface pointer.");
            }

            try
            {
                void** textureVtable = direct3DTexture->LpVtbl;
                delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int> getSurfaceLevel =
                    (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>) textureVtable[18];
                result = getSurfaceLevel(direct3DTexture, 0, &direct3DSurface);
                if (result < 0)
                {
                    Direct3D9Factory.Release(direct3DSurface);
                    return result;
                }
            }
            finally
            {
                Direct3D9Factory.Release(direct3DTexture);
            }
        }

        if (direct3DSurface is null)
        {
            throw new InvalidOperationException("Direct3D system-memory update surface creation returned a null interface pointer.");
        }

        surface = new Direct3D9SystemMemoryUpdateSurface(direct3DSurface);
        return result;
    }

    private int TryCreateTextureCore(
        uint width,
        uint height,
        uint levels,
        uint usage,
        Format format,
        Pool pool,
        nint* sharedHandle,
        out Direct3D9Texture? texture)
    {
        texture = null;
        if (_createTexture is not null)
        {
            return HandleDeviceInternalError(_createTexture(width, height, levels, usage, format, pool));
        }

        IDirect3DTexture9* direct3DTexture = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, uint, Format, Pool, IDirect3DTexture9**, void**, int> createTexture =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, uint, Format, Pool, IDirect3DTexture9**, void**, int>) vtable[23];
        int result = createTexture(_device, width, height, levels, usage, format, pool, &direct3DTexture, (void**) sharedHandle);
        if (result < 0)
        {
            Direct3D9Factory.Release(direct3DTexture);
            return HandleDeviceInternalError(result);
        }
        if (direct3DTexture is null)
        {
            throw new InvalidOperationException("Direct3D texture creation returned a null interface pointer.");
        }

        result = Direct3D9Texture.TryCreate(
            _resourceManager,
            direct3DTexture,
            isEvictable: false,
            out texture);
        Direct3D9Factory.Release(direct3DTexture);
        return result < 0 ? HandleDeviceInternalError(result) : result;
    }

    internal int TryCreateVertexBuffer(
        uint length,
        uint usage,
        uint flexibleVertexFormat,
        Pool pool,
        out Direct3D9VertexBuffer? vertexBuffer)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        vertexBuffer = null;
        IDirect3DVertexBuffer9* direct3DVertexBuffer = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, Pool, IDirect3DVertexBuffer9**, void**, int> createVertexBuffer =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, Pool, IDirect3DVertexBuffer9**, void**, int>) vtable[26];
        int result = createVertexBuffer(
            _device,
            length,
            usage,
            flexibleVertexFormat,
            pool,
            &direct3DVertexBuffer,
            null);
        if (result < 0)
        {
            Direct3D9Factory.Release(direct3DVertexBuffer);
            return HandleDeviceInternalError(result);
        }
        if (direct3DVertexBuffer is null)
        {
            throw new InvalidOperationException("Direct3D vertex buffer creation returned a null interface pointer.");
        }

        vertexBuffer = new Direct3D9VertexBuffer(direct3DVertexBuffer);
        return result;
    }

    internal int TryCreateIndexBuffer(
        uint length,
        uint usage,
        Format format,
        Pool pool,
        out Direct3D9IndexBuffer? indexBuffer)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        indexBuffer = null;
        IDirect3DIndexBuffer9* direct3DIndexBuffer = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, Pool, IDirect3DIndexBuffer9**, void**, int> createIndexBuffer =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, Pool, IDirect3DIndexBuffer9**, void**, int>) vtable[27];
        int result = createIndexBuffer(_device, length, usage, format, pool, &direct3DIndexBuffer, null);
        if (result < 0)
        {
            Direct3D9Factory.Release(direct3DIndexBuffer);
            return HandleDeviceInternalError(result);
        }
        if (direct3DIndexBuffer is null)
        {
            throw new InvalidOperationException("Direct3D index buffer creation returned a null interface pointer.");
        }

        indexBuffer = new Direct3D9IndexBuffer(direct3DIndexBuffer);
        return result;
    }

    internal int SetConvolutionMonoKernel(uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);
        if (_deviceEx is null && _setConvolutionMonoKernel is null)
        {
            throw new InvalidOperationException("SetConvolutionMonoKernel requires a Direct3D 9Ex device.");
        }

        int result;
        if (_setConvolutionMonoKernel is not null)
        {
            result = _setConvolutionMonoKernel(width, height);
        }
        else
        {
            void** vtable = _deviceEx->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, uint, uint, float*, float*, int> setConvolutionMonoKernel =
                (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, uint, uint, float*, float*, int>) vtable[119];
            result = setConvolutionMonoKernel(_deviceEx, width, height, null, null);
        }

        return HandleDeviceInternalError(result);
    }

    internal int ComposeRects(
        Direct3D9Surface source,
        Direct3D9Surface destination,
        Direct3D9VertexBuffer sourceRectDescriptors,
        uint rectangleCount,
        Direct3D9VertexBuffer destinationRectDescriptors,
        Composerectsop operation)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(sourceRectDescriptors);
        ArgumentNullException.ThrowIfNull(destinationRectDescriptors);
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);
        if (_deviceEx is null)
        {
            throw new InvalidOperationException("ComposeRects requires a Direct3D 9Ex device.");
        }

        void** vtable = _deviceEx->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, IDirect3DSurface9*, IDirect3DSurface9*, IDirect3DVertexBuffer9*, uint, IDirect3DVertexBuffer9*, Composerectsop, int, int, int> composeRects =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, IDirect3DSurface9*, IDirect3DSurface9*, IDirect3DVertexBuffer9*, uint, IDirect3DVertexBuffer9*, Composerectsop, int, int, int>) vtable[120];
        int result = composeRects(
            _deviceEx,
            source.SurfaceForDeviceCall,
            destination.SurfaceForDeviceCall,
            sourceRectDescriptors.VertexBuffer,
            rectangleCount,
            destinationRectDescriptors.VertexBuffer,
            operation,
            0,
            0);
        return HandleDeviceInternalError(result);
    }

    internal int TryCreateStateBlock(Stateblocktype type, out Direct3D9StateBlock? stateBlock)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        stateBlock = null;
        IDirect3DStateBlock9* direct3DStateBlock = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Stateblocktype, IDirect3DStateBlock9**, int> createStateBlock =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Stateblocktype, IDirect3DStateBlock9**, int>) vtable[59];
        int result = createStateBlock(_device, type, &direct3DStateBlock);
        if (result < 0)
        {
            Direct3D9Factory.Release(direct3DStateBlock);
            return HandleDeviceInternalError(result);
        }
        if (direct3DStateBlock is null)
        {
            throw new InvalidOperationException("Direct3D state block creation returned a null interface pointer.");
        }

        stateBlock = new Direct3D9StateBlock(direct3DStateBlock);
        return result;
    }

    internal int TryCreateVertexShader(uint* shaderFunction, out Direct3D9VertexShader? vertexShader)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        vertexShader = null;
        IDirect3DVertexShader9* nativeVertexShader = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint*, IDirect3DVertexShader9**, int> createVertexShader =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint*, IDirect3DVertexShader9**, int>) vtable[91];
        int result = createVertexShader(_device, shaderFunction, &nativeVertexShader);
        if (result < 0)
        {
            Direct3D9Factory.Release(nativeVertexShader);
            return HandleDeviceInternalError(result);
        }
        if (nativeVertexShader is null)
        {
            throw new InvalidOperationException("Direct3D vertex shader creation returned a null interface pointer.");
        }

        vertexShader = new Direct3D9VertexShader(nativeVertexShader);
        return result;
    }

    internal int TryCreatePixelShader(uint* shaderFunction, out Direct3D9PixelShader? pixelShader)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        pixelShader = null;
        IDirect3DPixelShader9* nativePixelShader = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint*, IDirect3DPixelShader9**, int> createPixelShader =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint*, IDirect3DPixelShader9**, int>) vtable[106];
        int result = createPixelShader(_device, shaderFunction, &nativePixelShader);
        if (result < 0)
        {
            Direct3D9Factory.Release(nativePixelShader);
            return HandleDeviceInternalError(result);
        }
        if (nativePixelShader is null)
        {
            throw new InvalidOperationException("Direct3D pixel shader creation returned a null interface pointer.");
        }

        pixelShader = new Direct3D9PixelShader(nativePixelShader);
        return result;
    }

    internal SurfaceDesc GetRenderTargetDescription(Direct3D9Surface renderTarget)
    {
        ArgumentNullException.ThrowIfNull(renderTarget);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _getRenderTargetDescription?.Invoke(renderTarget) ?? renderTarget.GetDescription();
    }

    internal int CreateDepthBuffer(
        uint width,
        uint height,
        MultisampleType multisampleType,
        out Direct3D9Surface? surface)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_createDepthBuffer is not null)
        {
            int delegatedResult = _createDepthBuffer(width, height, multisampleType, out surface);
            if (delegatedResult < 0)
            {
                surface?.Dispose();
                surface = null;
            }

            return HandleDeviceInternalError(delegatedResult);
        }

        ObjectDisposedException.ThrowIf(_device is null, this);
        surface = null;
        IDirect3DSurface9* nativeSurface = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, MultisampleType, uint, int, IDirect3DSurface9**, void**, int> createDepthStencilSurface =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, MultisampleType, uint, int, IDirect3DSurface9**, void**, int>) vtable[29];
        int result = createDepthStencilSurface(
            _device,
            width,
            height,
            Format.D24S8,
            multisampleType,
            0,
            0,
            &nativeSurface,
            null);
        if (result < 0)
        {
            Direct3D9Factory.Release(nativeSurface);
            return HandleDeviceInternalError(result);
        }
        if (nativeSurface is null)
        {
            throw new InvalidOperationException("Direct3D depth-stencil creation returned a null interface pointer.");
        }

        result = Direct3D9Surface.TryCreate(_resourceManager, nativeSurface, out surface);
        return result < 0 ? HandleDeviceInternalError(result) : result;
    }

    internal Direct3D9Surface CreateRenderTarget(
        uint width,
        uint height,
        Format format = Format.A8R8G8B8,
        MultisampleType multiSampleType = MultisampleType.MultisampleNone,
        uint multiSampleQuality = 0,
        bool lockable = false)
    {
        int result = TryCreateRenderTarget(
            width,
            height,
            format,
            multiSampleType,
            multiSampleQuality,
            lockable,
            out Direct3D9Surface? surface);
        Marshal.ThrowExceptionForHR(result);
        return surface ?? throw new InvalidOperationException("Direct3D render target creation returned a null interface pointer.");
    }

    internal int TryCreateRenderTarget(
        uint width,
        uint height,
        Format format,
        MultisampleType multiSampleType,
        uint multiSampleQuality,
        bool lockable,
        out Direct3D9Surface? renderTarget)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_createRenderTarget is not null)
        {
            int delegatedResult = _createRenderTarget(width, height, format, multiSampleType, out renderTarget);
            if (delegatedResult < 0)
            {
                renderTarget?.Dispose();
                renderTarget = null;
            }

            return HandleDeviceInternalError(delegatedResult);
        }

        renderTarget = null;
        int result = TryCreateRenderTargetNative(
            width,
            height,
            format,
            multiSampleType,
            multiSampleQuality,
            lockable,
            out IDirect3DSurface9* surface);
        if (result < 0)
        {
            return result;
        }

        return Direct3D9Surface.TryCreate(_resourceManager, surface, out renderTarget);
    }

    internal int TryCreateRenderTargetUntracked(
        uint width,
        uint height,
        Format format,
        MultisampleType multiSampleType,
        uint multiSampleQuality,
        bool lockable,
        out Direct3D9UntrackedSurface? renderTarget)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        renderTarget = null;
        int result = TryCreateRenderTargetNative(
            width,
            height,
            format,
            multiSampleType,
            multiSampleQuality,
            lockable,
            out IDirect3DSurface9* surface);
        if (result < 0)
        {
            return result;
        }

        renderTarget = new Direct3D9UntrackedSurface(surface);
        return result;
    }

    private int TryCreateRenderTargetNative(
        uint width,
        uint height,
        Format format,
        MultisampleType multiSampleType,
        uint multiSampleQuality,
        bool lockable,
        out IDirect3DSurface9* surface)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);
        IDirect3DSurface9* nativeSurface = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, MultisampleType, uint, int, IDirect3DSurface9**, void**, int> createRenderTarget =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, MultisampleType, uint, int, IDirect3DSurface9**, void**, int>) vtable[28];
        int result = createRenderTarget(
            _device,
            width,
            height,
            format,
            multiSampleType,
            multiSampleQuality,
            lockable ? 1 : 0,
            &nativeSurface,
            null);
        if (result < 0)
        {
            Direct3D9Factory.Release(nativeSurface);
            surface = null;
            return HandleDeviceInternalError(result);
        }
        if (nativeSurface is null)
        {
            throw new InvalidOperationException("Direct3D render target creation returned a null interface pointer.");
        }

        surface = nativeSurface;
        return result;
    }

    internal int GetRenderTargetData(
        IDirect3DSurface9* sourceSurface,
        IDirect3DSurface9* destinationSurface)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, IDirect3DSurface9*, int> getRenderTargetData =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, IDirect3DSurface9*, int>) vtable[32];
        int result = getRenderTargetData(_device, sourceSurface, destinationSurface);
        if (result == Direct3D9Factory.DeviceLostHResult)
        {
            result = Direct3D9Factory.DisplayStateInvalidHResult;
        }

        return HandleDeviceInternalError(result);
    }

    internal int ColorFill(
        IDirect3DSurface9* surface,
        Direct3D9SurfaceRect? rectangle,
        uint color)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        Direct3D9SurfaceRect bounds = rectangle.GetValueOrDefault();
        Direct3D9SurfaceRect* boundsPointer = rectangle.HasValue ? &bounds : null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, uint, int> colorFill =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, uint, int>) vtable[35];
        return colorFill(_device, surface, boundsPointer, color);
    }

    internal int UpdateSurface(
        IDirect3DSurface9* systemMemorySourceSurface,
        Direct3D9SurfaceRect? sourceRectangle,
        IDirect3DSurface9* poolDefaultDestinationSurface,
        Direct3D9Point? destinationPoint)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        Direct3D9SurfaceRect sourceBounds = sourceRectangle.GetValueOrDefault();
        Direct3D9Point destination = destinationPoint.GetValueOrDefault();
        Direct3D9SurfaceRect* sourceBoundsPointer = sourceRectangle.HasValue ? &sourceBounds : null;
        Direct3D9Point* destinationPointer = destinationPoint.HasValue ? &destination : null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9Point*, int> updateSurface =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9Point*, int>) vtable[30];
        int result = updateSurface(
            _device,
            systemMemorySourceSurface,
            sourceBoundsPointer,
            poolDefaultDestinationSurface,
            destinationPointer);
        return HandleDeviceInternalError(result);
    }

    internal int UpdateTexture(
        IDirect3DTexture9* systemMemorySourceTexture,
        IDirect3DTexture9* poolDefaultDestinationTexture)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DTexture9*, IDirect3DTexture9*, int> updateTexture =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DTexture9*, IDirect3DTexture9*, int>) vtable[31];
        int result = updateTexture(_device, systemMemorySourceTexture, poolDefaultDestinationTexture);
        return HandleDeviceInternalError(result);
    }

    internal int CopyTexture(
        IDirect3DTexture9* sourceTexture,
        IDirect3DTexture9* destinationTexture)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        IDirect3DSurface9* sourceSurface = null;
        IDirect3DSurface9* destinationSurface = null;
        int result;

        try
        {
            void** sourceVtable = sourceTexture->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int> getSourceSurfaceLevel =
                (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>) sourceVtable[18];
            result = getSourceSurfaceLevel(sourceTexture, 0, &sourceSurface);
            if (result < 0)
            {
                return HandleDeviceInternalError(result);
            }

            void** destinationVtable = destinationTexture->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int> getDestinationSurfaceLevel =
                (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>) destinationVtable[18];
            result = getDestinationSurfaceLevel(destinationTexture, 0, &destinationSurface);
            if (result < 0)
            {
                return HandleDeviceInternalError(result);
            }

            void** deviceVtable = _device->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9SurfaceRect*, Texturefiltertype, int> stretchRect =
                (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9SurfaceRect*, Texturefiltertype, int>) deviceVtable[34];
            result = stretchRect(_device, sourceSurface, null, destinationSurface, null, Texturefiltertype.None);
            return HandleDeviceInternalError(result);
        }
        finally
        {
            Direct3D9Factory.Release(sourceSurface);
            Direct3D9Factory.Release(destinationSurface);
        }
    }

    internal int StretchRect(
        nint sourceSurface,
        Direct3D9SurfaceRect sourceRectangle,
        nint destinationSurface,
        Direct3D9SurfaceRect destinationRectangle)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);
        ArgumentOutOfRangeException.ThrowIfZero(sourceSurface);
        ArgumentOutOfRangeException.ThrowIfZero(destinationSurface);

        Direct3D9SurfaceRect sourceBounds = sourceRectangle;
        Direct3D9SurfaceRect destinationBounds = destinationRectangle;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9SurfaceRect*, Texturefiltertype, int> stretchRect =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9SurfaceRect*, Texturefiltertype, int>) vtable[34];
        int result = stretchRect(
            _device,
            (IDirect3DSurface9*) sourceSurface,
            &sourceBounds,
            (IDirect3DSurface9*) destinationSurface,
            &destinationBounds,
            Texturefiltertype.None);
        return HandleDeviceInternalError(result);
    }

    internal int StretchRect(
        Direct3D9Surface source,
        Direct3D9SurfaceRect sourceRect,
        Direct3D9Surface destination,
        Direct3D9SurfaceRect destinationRect)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_stretchRect is not null)
        {
            return HandleDeviceInternalError(_stretchRect(source, sourceRect, destination, destinationRect));
        }

        return StretchRect(
            source,
            sourceRect,
            destination.SurfaceForDeviceCall,
            destinationRect,
            Texturefiltertype.None);
    }

    internal int StretchRect(
        Direct3D9Surface source,
        Direct3D9SurfaceRect? sourceRectangle,
        IDirect3DSurface9* destinationSurface,
        Direct3D9SurfaceRect? destinationRectangle,
        Texturefiltertype filter)
    {
        ArgumentNullException.ThrowIfNull(source);
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        IDirect3DSurface9* sourceSurface = source.SurfaceForDeviceCall;
        Direct3D9SurfaceRect sourceBounds = sourceRectangle.GetValueOrDefault();
        Direct3D9SurfaceRect destinationBounds = destinationRectangle.GetValueOrDefault();
        Direct3D9SurfaceRect* sourceBoundsPointer = sourceRectangle.HasValue ? &sourceBounds : null;
        Direct3D9SurfaceRect* destinationBoundsPointer = destinationRectangle.HasValue ? &destinationBounds : null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9SurfaceRect*, Texturefiltertype, int> stretchRect =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9SurfaceRect*, Texturefiltertype, int>) vtable[34];
        int result = stretchRect(
            _device,
            sourceSurface,
            sourceBoundsPointer,
            destinationSurface,
            destinationBoundsPointer,
            filter);
        return HandleDeviceInternalError(result);
    }

    internal int TryGetSwapChain(uint groupAdapterOrdinal, out Direct3D9SwapChain? swapChain)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        swapChain = null;
        IDirect3DSwapChain9* nativeSwapChain = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DSwapChain9**, int> getSwapChain =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DSwapChain9**, int>) vtable[14];
        int result = getSwapChain(_device, groupAdapterOrdinal, &nativeSwapChain);
        if (result < 0)
        {
            Direct3D9Factory.Release(nativeSwapChain);
            return HandleDeviceInternalError(result);
        }
        if (nativeSwapChain is null)
        {
            throw new InvalidOperationException("Direct3D swap chain retrieval returned a null interface pointer.");
        }

        int initializationResult = Direct3D9SwapChain.TryCreate(
            _resourceManager,
            nativeSwapChain,
            0,
            out swapChain);
        Direct3D9Factory.Release(nativeSwapChain);
        return initializationResult < 0 ? HandleDeviceInternalError(initializationResult) : result;
    }

    internal int TryGetBackBuffer(
        uint swapChainOrdinal,
        uint backBufferOrdinal,
        BackbufferType type,
        out Direct3D9Surface? backBuffer)
    {
        ObjectDisposedException.ThrowIf(_isDisposed || _device is null, this);

        backBuffer = null;
        IDirect3DSurface9* nativeBackBuffer = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, BackbufferType, IDirect3DSurface9**, int> getBackBuffer =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, BackbufferType, IDirect3DSurface9**, int>) vtable[18];
        int result = getBackBuffer(
            _device,
            swapChainOrdinal,
            backBufferOrdinal,
            type,
            &nativeBackBuffer);
        if (result < 0)
        {
            Direct3D9Factory.Release(nativeBackBuffer);
            return HandleDeviceInternalError(result);
        }
        if (nativeBackBuffer is null)
        {
            throw new InvalidOperationException("Direct3D back-buffer retrieval returned a null interface pointer.");
        }

        int initializationResult = Direct3D9Surface.TryCreate(
            _resourceManager,
            nativeBackBuffer,
            out backBuffer);
        return initializationResult < 0 ? HandleDeviceInternalError(initializationResult) : result;
    }

    internal Direct3D9SwapChain CreateAdditionalSwapChain(
        uint width,
        uint height,
        Format format = Format.X8R8G8B8)
    { 
        PresentParameters presentParameters = new(
            backBufferWidth: width,
            backBufferHeight: height,
            backBufferFormat: format,
            backBufferCount: 1,
            swapEffect: Swapeffect.Discard,
            hDeviceWindow: (nint) PInvoke.GetDesktopWindow().Value,
            windowed: true,
            enableAutoDepthStencil: false,
            autoDepthStencilFormat: Format.Unknown);
        int result = TryCreateAdditionalSwapChain(presentParameters, out Direct3D9SwapChain? swapChain);
        Marshal.ThrowExceptionForHR(result);
        return swapChain ?? throw new InvalidOperationException("Direct3D additional swap chain creation returned a null interface pointer.");
    }

    internal int TryCreateAdditionalSwapChain(
        PresentParameters presentParameters,
        out Direct3D9SwapChain? additionalSwapChain)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        additionalSwapChain = null;
        if (IsUnusable)
        {
            return Direct3D9Factory.DisplayStateInvalidHResult;
        }
        bool hasKnownTextureLimits = _createAdditionalSwapChain is null
            || Capabilities.MaxTextureWidth != 0
            || Capabilities.MaxTextureHeight != 0;
        if (hasKnownTextureLimits
            && (presentParameters.BackBufferWidth > Capabilities.MaxTextureWidth
                || presentParameters.BackBufferHeight > Capabilities.MaxTextureHeight))
        {
            return Direct3D9Factory.OutOfVideoMemoryHResult;
        }

        if (_createAdditionalSwapChain is not null)
        {
            int delegatedResult = _createAdditionalSwapChain(presentParameters, out additionalSwapChain);
            if (delegatedResult < 0)
            {
                delegatedResult = HandleAdditionalSwapChainCreationFailure(delegatedResult);
                additionalSwapChain?.Dispose();
                additionalSwapChain = null;
            }

            return delegatedResult;
        }

        ObjectDisposedException.ThrowIf(_device is null, this);
        IDirect3DSwapChain9* swapChain = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, PresentParameters*, IDirect3DSwapChain9**, int> createAdditionalSwapChain =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, PresentParameters*, IDirect3DSwapChain9**, int>) vtable[13];
        int result = createAdditionalSwapChain(_device, &presentParameters, &swapChain);
        if (result < 0)
        {
            result = HandleAdditionalSwapChainCreationFailure(result);
            Direct3D9Factory.Release(swapChain);
            return result;
        }
        if (swapChain is null)
        {
            throw new InvalidOperationException("Direct3D additional swap chain creation returned a null interface pointer.");
        }

        int initializationResult = Direct3D9SwapChain.TryCreate(
            _resourceManager,
            swapChain,
            presentParameters.BackBufferCount,
            out additionalSwapChain);
        Direct3D9Factory.Release(swapChain);
        return initializationResult < 0
            ? HandleAdditionalSwapChainCreationFailure(initializationResult)
            : result;
    }

    private int HandleAdditionalSwapChainCreationFailure(int hResult)
    {
        if (hResult == Direct3D9Factory.DriverInternalErrorHResult)
        {
            MarkUnusable(Direct3D9Factory.DriverInternalErrorHResult);
            return Direct3D9Factory.DisplayStateInvalidHResult;
        }
        if (hResult == Direct3D9Factory.DeviceLostHResult)
        {
            MarkUnusable(Direct3D9Factory.DeviceLostHResult);
            return Direct3D9Factory.DisplayStateInvalidHResult;
        }

        return hResult;
    }

    private void ReleaseTextPixelShaders()
    {
        IsTextPixelShaderInitialized = false;
        CanDrawText = false;
        for (int index = 0; index < _textPixelShaders.Length; index++)
        {
            _textPixelShaders[index]?.Dispose();
            _textPixelShaders[index] = null;
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        if (_entryCount != 0 || _entryThreadId != 0)
        {
            throw new InvalidOperationException("The device cannot be disposed while a device entry is active.");
        }

        if (_resourceManager.IsInUseContext)
        {
            throw new InvalidOperationException("The device cannot be disposed while a resource use context is active.");
        }

        Enter();
        try
        {
            ReleaseTextPixelShaders();
            ResetGpuMarkers();

            Direct3D9Factory.Release(_dummyBackBuffer);
            _dummyBackBuffer = null;

            Direct3D9Factory.Release(_deviceEx);
            _deviceEx = null;

            _currentRenderTarget = null;

            _hardwareIndexBuffer?.Dispose();
            _hardwareIndexBuffer = null;
            _hardwareVertexBuffer?.Dispose();
            _hardwareVertexBuffer = null;

            _resourceManager.DestroyAllResources();
            _depthStencilSurfaceForCurrentRenderTarget = null;
            _resourceManager.Close();

            Direct3D9Factory.Release(_device);
            _device = null;

            Direct3D9Factory.Release(_direct3D);
            _direct3D = null;
            if (_ownsRealizationCacheIndex)
            {
                Direct3D9ResourceCacheIndexManager.ReleaseIndex(_realizationCacheIndex);
            }
            _disposedNotification?.Invoke(this);
            _isDisposed = true;
        }
        finally
        {
            LeaveDeviceEntry();
        }
    }

    private void LeaveDeviceEntry()
    {
        if (--_entryCount == 0)
        {
            _entryThreadId = 0;
        }

        if (_deviceEntryLock is not null)
        {
            Monitor.Exit(_deviceEntryLock);
        }
    }
}
