using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceSwapChainRetrievalTests
{
    private static uint _groupAdapterOrdinal;
    private static nint _swapChainToReturn;
    private static int _getResult;
    private static int _getCallCount;
    private static PresentParameters _createdPresentParameters;
    private static int _createResult;
    private static int _createCallCount;
    private static int _getPresentParametersResult;
    private static int _getPresentParametersCallCount;
    private static int _getBackBufferResult;
    private static int _getBackBufferCallCount;
    private static nint _backBufferToReturn;
    private static int _swapChainAddRefCount;
    private static int _swapChainReleaseCount;
    private static int _swapChainQueryInterfaceResult;
    private static nint _swapChainQueryInterfacePointer;
    private static int _swapChainQueryInterfaceCallCount;
    private static Guid _swapChainQueryInterfaceId;
    private static int _swapChainExReleaseCount;
    private static int _backBufferReleaseCount;
    private static uint _deviceBackBufferSwapChainOrdinal;
    private static uint _deviceBackBufferOrdinal;
    private static BackbufferType _deviceBackBufferType;
    private static int _deviceGetBackBufferResult;
    private static int _deviceGetBackBufferCallCount;

    [TestInitialize]
    public void Initialize()
    {
        _groupAdapterOrdinal = uint.MaxValue;
        _swapChainToReturn = 0;
        _getResult = 0;
        _getCallCount = 0;
        _createdPresentParameters = default;
        _createResult = 0;
        _createCallCount = 0;
        _getPresentParametersResult = 0;
        _getPresentParametersCallCount = 0;
        _getBackBufferResult = 0;
        _getBackBufferCallCount = 0;
        _backBufferToReturn = 0;
        _swapChainAddRefCount = 0;
        _swapChainReleaseCount = 0;
        _swapChainQueryInterfaceResult = unchecked((int) 0x80004002);
        _swapChainQueryInterfacePointer = 0;
        _swapChainQueryInterfaceCallCount = 0;
        _swapChainQueryInterfaceId = default;
        _swapChainExReleaseCount = 0;
        _backBufferReleaseCount = 0;
        _deviceBackBufferSwapChainOrdinal = uint.MaxValue;
        _deviceBackBufferOrdinal = uint.MaxValue;
        _deviceBackBufferType = unchecked((BackbufferType) uint.MaxValue);
        _deviceGetBackBufferResult = 0;
        _deviceGetBackBufferCallCount = 0;
    }

    [TestMethod]
    public void WhenGettingSwapChainThenSlot14OrdinalOwnershipAndResourceRegistrationArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetSwapChain(3, out Direct3D9SwapChain? swapChain);
        nint nativeSwapChain = (nint) swapChain!.SwapChain;
        int resourceCount = device.ResourceCount;
        swapChain.Dispose();

        Assert.AreEqual(
            (0, 3u, 1, 1, 1, (nint) swapChainObject.SwapChain, 2, 1, 2, 1),
            (result, _groupAdapterOrdinal, _getCallCount, _getPresentParametersCallCount,
                _getBackBufferCallCount, nativeSwapChain, resourceCount,
                _swapChainAddRefCount, _swapChainReleaseCount, _backBufferReleaseCount));
    }

    [TestMethod]
    public void WhenGettingSwapChainSupportsExThenIndependentExReferenceIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _swapChainQueryInterfaceResult = 0;
        _swapChainQueryInterfacePointer = (nint) swapChainObject.SwapChainEx;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetSwapChain(0, out Direct3D9SwapChain? swapChain);
        swapChain!.Dispose();

        Assert.AreEqual(
            (0, 1, IDirect3DSwapChain9Ex.Guid, 1, 2),
            (result, _swapChainQueryInterfaceCallCount, _swapChainQueryInterfaceId,
                _swapChainExReleaseCount, _swapChainReleaseCount));
    }

    [TestMethod]
    public void WhenSwapChainExProbeFailsWithPointerThenPointerIsReleasedAndCreationContinues()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _swapChainQueryInterfacePointer = (nint) swapChainObject.SwapChainEx;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetSwapChain(0, out Direct3D9SwapChain? swapChain);
        swapChain!.Dispose();

        Assert.AreEqual(
            (0, 1, IDirect3DSwapChain9Ex.Guid, 1, 2),
            (result, _swapChainQueryInterfaceCallCount, _swapChainQueryInterfaceId,
                _swapChainExReleaseCount, _swapChainReleaseCount));
    }

    [TestMethod]
    public void WhenGettingSwapChainFailsWithPointerThenPointerIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _getResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetSwapChain(1, out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, 0, null),
            (result, _getCallCount, _swapChainReleaseCount, device.ResourceCount, swapChain));
    }

    [TestMethod]
    public void WhenGettingSwapChainReportsDriverInternalErrorThenHandleDieMappingIsApplied()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _getResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetSwapChain(0, out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1, null),
            (result, device.UnusableReasonHResult, _swapChainReleaseCount, swapChain));
    }

    [TestMethod]
    public void WhenGettingSwapChainReturnsNonzeroSuccessThenOriginalHResultAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _getResult = 1;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetSwapChain(uint.MaxValue, out Direct3D9SwapChain? swapChain);
        swapChain!.Dispose();

        Assert.AreEqual(
            (1, uint.MaxValue, 1, 1, 1, 2, 1),
            (result, _groupAdapterOrdinal, _getCallCount, _getPresentParametersCallCount,
                _getBackBufferCallCount, _swapChainReleaseCount, _backBufferReleaseCount));
    }

    [TestMethod]
    public void WhenGettingSwapChainReturnsOrdinaryFailureThenDeviceRemainsUsable()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _getResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetSwapChain(4, out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, 4u, 1, 1, null),
            (result, device.UnusableReasonHResult, _groupAdapterOrdinal, _getCallCount,
                _swapChainReleaseCount, swapChain));
    }

    [TestMethod]
    public void WhenGettingSwapChainReturnsDeviceLostThenOriginalHResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _getResult = Direct3D9Factory.DeviceLostHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetSwapChain(5, out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, 0, 5u, 1, 1, null),
            (result, device.UnusableReasonHResult, _groupAdapterOrdinal, _getCallCount,
                _swapChainReleaseCount, swapChain));
    }

    [TestMethod]
    public void WhenRetrievedSwapChainPresentParametersFailThenSwapChainIsReleasedAndOutputIsCleared()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _getPresentParametersResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetSwapChain(0, out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, 0, 1, 0, null),
            (result, _getCallCount, _getPresentParametersCallCount, _getBackBufferCallCount,
                _swapChainReleaseCount, device.ResourceCount, swapChain));
    }

    [TestMethod]
    public void WhenRetrievedSwapChainBackBufferInitializationFailsThenTemporaryResourcesAreReleasedAndOutputIsCleared()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _getBackBufferResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetSwapChain(0, out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, 1, 1, 2, 0, null),
            (result, _getCallCount, _getPresentParametersCallCount, _getBackBufferCallCount,
                _backBufferReleaseCount, _swapChainReleaseCount, device.ResourceCount, swapChain));
    }

    [DataTestMethod]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenAdditionalSwapChainWrapperInitializationReportsDeviceFailureThenDeviceIsInvalidatedAndTemporaryResourcesAreReleased(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _getBackBufferResult = failureHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        PresentParameters presentParameters = new(
            backBufferFormat: Format.A8R8G8B8,
            backBufferCount: 1,
            windowed: true);

        int result = device.TryCreateAdditionalSwapChain(presentParameters, out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DisplayStateInvalidHResult, presentParameters, 1, 1, 1, 2, 0, null),
            (result, device.UnusableReasonHResult, _createdPresentParameters, _createCallCount,
                _getBackBufferCallCount, _backBufferReleaseCount, _swapChainReleaseCount,
                device.ResourceCount, swapChain));
    }

    [TestMethod]
    public void WhenAdditionalSwapChainCreationReturnsNonzeroSuccessThenSlot13ParametersBackBufferCountAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _createResult = 1;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        PresentParameters presentParameters = new(
            backBufferWidth: 12,
            backBufferHeight: 34,
            backBufferFormat: Format.A8R8G8B8,
            backBufferCount: 3,
            windowed: true);

        int result = device.TryCreateAdditionalSwapChain(presentParameters, out Direct3D9SwapChain? swapChain);
        int resourceCount = device.ResourceCount;
        swapChain!.Dispose();

        Assert.AreEqual(
            (1, presentParameters, 1, 0, 3, 2, 3, 3u, 4, 0),
            (result, _createdPresentParameters, _createCallCount, _getPresentParametersCallCount,
                _getBackBufferCallCount, _swapChainReleaseCount, _backBufferReleaseCount,
                presentParameters.BackBufferCount, resourceCount, device.ResourceCount));
    }

    [TestMethod]
    public void WhenAdditionalSwapChainCreationFailsWithPointerThenPointerIsReleasedAndOutputIsCleared()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        PresentParameters presentParameters = new(
            backBufferWidth: 56,
            backBufferHeight: 78,
            backBufferFormat: Format.X8R8G8B8,
            backBufferCount: 2,
            windowed: true);

        int result = device.TryCreateAdditionalSwapChain(presentParameters, out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, presentParameters, 1, 1, 0, 0, (Direct3D9SwapChain?) null, false),
            (result, _createdPresentParameters, _createCallCount, _swapChainReleaseCount,
                _getPresentParametersCallCount, device.ResourceCount, swapChain, device.IsUnusable));
    }

    [DataTestMethod]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenAdditionalSwapChainCreationReportsDeviceFailureThenDeviceIsInvalidatedOnceAndPointerIsReleased(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        _createResult = failureHResult;
        int unusableNotificationCount = 0;
        using Direct3D9Device device = new(
            deviceObject.Device,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            unusableNotification: _ => unusableNotificationCount++,
            capabilities: new Caps9 { MaxTextureWidth = 4096, MaxTextureHeight = 4096 });

        int result = device.TryCreateAdditionalSwapChain(
            new PresentParameters(backBufferCount: 1, windowed: true),
            out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DisplayStateInvalidHResult, 1, 1, 1, 0, (Direct3D9SwapChain?) null),
            (result, device.UnusableReasonHResult, _createCallCount, _swapChainReleaseCount,
                unusableNotificationCount, device.ResourceCount, swapChain));
    }

    [TestMethod]
    public void WhenDeviceWasMarkedUnusableThenGettingSwapChainStillCallsDriver()
    { 
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.TryGetSwapChain(2, out Direct3D9SwapChain? swapChain);
        swapChain!.Dispose();

        Assert.AreEqual((0, 2u, 1, 2),
            (result, _groupAdapterOrdinal, _getCallCount, _swapChainReleaseCount));
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenRetrievedSwapChainIsReleasedAndPointerAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _swapChainToReturn = (nint) swapChainObject.SwapChain;
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.TryGetSwapChain(0, out Direct3D9SwapChain? swapChain);

        device.Dispose();

        Assert.AreEqual(2, _swapChainReleaseCount);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = swapChain!.SwapChain);
    }

    [TestMethod]
    public void WhenGettingSwapChainAfterDeviceWasDisposedThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.TryGetSwapChain(0, out _));
    }

    [TestMethod]
    public void WhenGettingDeviceBackBufferThenSlot18InputsOwnershipAndResourceRegistrationArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _backBufferToReturn = (nint) swapChainObject.BackBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetBackBuffer(3, 4, BackbufferType.Mono, out Direct3D9Surface? backBuffer);
        nint nativeBackBuffer = (nint) backBuffer!.Surface;
        int resourceCount = device.ResourceCount;
        backBuffer.Dispose();

        Assert.AreEqual(
            (0, 3u, 4u, BackbufferType.Mono, 1, (nint) swapChainObject.BackBuffer, 1, 1),
            (result, _deviceBackBufferSwapChainOrdinal, _deviceBackBufferOrdinal, _deviceBackBufferType,
                _deviceGetBackBufferCallCount, nativeBackBuffer, resourceCount, _backBufferReleaseCount));
    }

    [TestMethod]
    public void WhenGettingDeviceBackBufferFailsWithPointerThenPointerIsReleasedAndOutputIsCleared()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _backBufferToReturn = (nint) swapChainObject.BackBuffer;
        _deviceGetBackBufferResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetBackBuffer(1, 2, BackbufferType.Mono, out Direct3D9Surface? backBuffer);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, 0, (Direct3D9Surface?) null),
            (result, _deviceGetBackBufferCallCount, _backBufferReleaseCount, device.ResourceCount, backBuffer));
    }

    [TestMethod]
    public void WhenGettingDeviceBackBufferReportsDriverInternalErrorThenHandleDieMappingIsApplied()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _backBufferToReturn = (nint) swapChainObject.BackBuffer;
        _deviceGetBackBufferResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetBackBuffer(0, 0, BackbufferType.Mono, out Direct3D9Surface? backBuffer);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1, null),
            (result, device.UnusableReasonHResult, _backBufferReleaseCount, backBuffer));
    }

    [TestMethod]
    public void WhenGettingDeviceBackBufferReturnsNonzeroSuccessThenOriginalHResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSwapChainObject swapChainObject = new();
        _backBufferToReturn = (nint) swapChainObject.BackBuffer;
        _deviceGetBackBufferResult = 1;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryGetBackBuffer(uint.MaxValue, 7, BackbufferType.Mono, out Direct3D9Surface? backBuffer);
        backBuffer!.Dispose();

        Assert.AreEqual(
            (1, uint.MaxValue, 7u, BackbufferType.Mono, 1, 1),
            (result, _deviceBackBufferSwapChainOrdinal, _deviceBackBufferOrdinal,
                _deviceBackBufferType, _deviceGetBackBufferCallCount, _backBufferReleaseCount));
    }

    [TestMethod]
    public void WhenGettingDeviceBackBufferAfterDeviceWasDisposedThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.TryGetBackBuffer(0, 0, BackbufferType.Mono, out _));
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(
            device,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            capabilities: new Caps9 { MaxTextureWidth = 4096, MaxTextureHeight = 4096 });
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self)
    {
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateAdditionalSwapChain(
        IDirect3DDevice9* self,
        PresentParameters* presentParameters,
        IDirect3DSwapChain9** swapChain)
    {
        _createCallCount++;
        _createdPresentParameters = *presentParameters;
        *swapChain = (IDirect3DSwapChain9*) _swapChainToReturn;
        return _createResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSwapChain(
        IDirect3DDevice9* self,
        uint groupAdapterOrdinal,
        IDirect3DSwapChain9** swapChain)
    {
        _getCallCount++;
        _groupAdapterOrdinal = groupAdapterOrdinal;
        *swapChain = (IDirect3DSwapChain9*) _swapChainToReturn;
        return _getResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetDeviceBackBuffer(
        IDirect3DDevice9* self,
        uint swapChainOrdinal,
        uint backBufferOrdinal,
        BackbufferType type,
        IDirect3DSurface9** backBuffer)
    {
        _deviceGetBackBufferCallCount++;
        _deviceBackBufferSwapChainOrdinal = swapChainOrdinal;
        _deviceBackBufferOrdinal = backBufferOrdinal;
        _deviceBackBufferType = type;
        *backBuffer = (IDirect3DSurface9*) _backBufferToReturn;
        return _deviceGetBackBufferResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int QuerySwapChainInterface(IDirect3DSwapChain9* self, Guid* interfaceId, void** result)
    {
        _swapChainQueryInterfaceCallCount++;
        _swapChainQueryInterfaceId = *interfaceId;
        *result = (void*) _swapChainQueryInterfacePointer;
        return _swapChainQueryInterfaceResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefSwapChain(IDirect3DSwapChain9* self)
    {
        _swapChainAddRefCount++;
        return 2;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSwapChain(IDirect3DSwapChain9* self)
    {
        _swapChainReleaseCount++;
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSwapChainEx(IDirect3DSwapChain9Ex* self)
    {
        _swapChainExReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetPresentParameters(IDirect3DSwapChain9* self, PresentParameters* presentParameters)
    {
        _getPresentParametersCallCount++;
        presentParameters->BackBufferCount = 1;
        return _getPresentParametersResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetBackBuffer(
        IDirect3DSwapChain9* self,
        uint index,
        BackbufferType type,
        IDirect3DSurface9** backBuffer)
    {
        _getBackBufferCallCount++;
        *backBuffer = (IDirect3DSurface9*) _backBufferToReturn;
        return _getBackBufferResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseBackBuffer(IDirect3DSurface9* self)
    {
        _backBufferReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetBackBufferDescription(IDirect3DSurface9* self, SurfaceDesc* description)
    {
        *description = new SurfaceDesc { Width = 1, Height = 1, Format = Format.A8R8G8B8 };
        return 0;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 20);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[13] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, PresentParameters*, IDirect3DSwapChain9**, int>) &CreateAdditionalSwapChain;
            vtable[14] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DSwapChain9**, int>) &GetSwapChain;
            vtable[18] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, BackbufferType, IDirect3DSurface9**, int>) &GetDeviceBackBuffer;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private struct FakeSwapChainObject : IDisposable
    {
        private nint _memory;
        private nint _swapChainExMemory;
        private nint _backBufferMemory;
        internal IDirect3DSwapChain9* SwapChain;
        internal IDirect3DSwapChain9Ex* SwapChainEx;
        internal IDirect3DSurface9* BackBuffer;

        public FakeSwapChainObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 12);
            void** memory = (void**) _memory;
            SwapChain = (IDirect3DSwapChain9*) memory;
            void** vtable = memory + 1;
            SwapChain->LpVtbl = vtable;
            vtable[0] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, Guid*, void**, int>) &QuerySwapChainInterface;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, uint>) &AddRefSwapChain;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, uint>) &ReleaseSwapChain;
            vtable[5] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, uint, BackbufferType, IDirect3DSurface9**, int>) &GetBackBuffer;
            vtable[9] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, PresentParameters*, int>) &GetPresentParameters;

            _swapChainExMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** swapChainExMemory = (void**) _swapChainExMemory;
            SwapChainEx = (IDirect3DSwapChain9Ex*) swapChainExMemory;
            void** swapChainExVtable = swapChainExMemory + 1;
            SwapChainEx->LpVtbl = swapChainExVtable;
            swapChainExVtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9Ex*, uint>) &ReleaseSwapChainEx;

            _backBufferMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 15);
            void** backBufferMemory = (void**) _backBufferMemory;
            BackBuffer = (IDirect3DSurface9*) backBufferMemory;
            void** backBufferVtable = backBufferMemory + 1;
            BackBuffer->LpVtbl = backBufferVtable;
            backBufferVtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseBackBuffer;
            backBufferVtable[12] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) &GetBackBufferDescription;
            _backBufferToReturn = (nint) BackBuffer;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _backBufferMemory);
            NativeMemory.Free((void*) _swapChainExMemory);
            NativeMemory.Free((void*) _memory);
            _backBufferMemory = 0;
            _swapChainExMemory = 0;
            _memory = 0;
            BackBuffer = null;
            SwapChain = null;
        }
    }
}
