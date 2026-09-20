using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceInterfaceQueryFailureTests
{
    private static nint _createdDevice;
    private static nint _queryInterfaceResult;
    private static int _queryInterfaceHResult;
    private static int _queryInterfaceCallCount;
    private static int _deviceCapsHResult;
    private static Caps9 _deviceCapabilities;
    private static int _getBackBufferHResult;
    private static int _getBackBufferCallCount;
    private static uint _backBufferSwapChainOrdinal;
    private static uint _backBufferOrdinal;
    private static BackbufferType _backBufferType;
    private static int _backBufferReleaseCount;
    private static nint _dummyBackBuffer;
    private static int _deviceCapsCallCount;
    private static bool _deviceCapsOutputWasCleared;
    private static int _createdDeviceReleaseCount;
    private static int _queryInterfaceResultReleaseCount;

    [TestInitialize]
    public void Initialize()
    {
        _createdDevice = 0;
        _queryInterfaceResult = 0;
        _queryInterfaceHResult = 0;
        _queryInterfaceCallCount = 0;
        _deviceCapsHResult = 0;
        _deviceCapabilities = new Caps9 { DeviceType = Devtype.SW };
        _getBackBufferHResult = 0;
        _getBackBufferCallCount = 0;
        _backBufferSwapChainOrdinal = uint.MaxValue;
        _backBufferOrdinal = uint.MaxValue;
        _backBufferType = unchecked((BackbufferType) uint.MaxValue);
        _backBufferReleaseCount = 0;
        _dummyBackBuffer = 0;
        _deviceCapsCallCount = 0;
        _deviceCapsOutputWasCleared = false;
        _createdDeviceReleaseCount = 0;
        _queryInterfaceResultReleaseCount = 0;
    }

    [TestMethod]
    public void WhenBaseDeviceQueryForExtendedInterfaceFailsWithPointerThenTemporaryReferenceIsReleased()
    {
        using FakeDirect3DObject direct3DObject = new();
        using FakeDeviceObject createdDevice = new(&ReleaseCreatedDevice);
        using FakeDeviceObject queryResult = new(&ReleaseQueryInterfaceResult);
        _createdDevice = (nint) createdDevice.Device;
        _queryInterfaceResult = (nint) queryResult.DeviceEx;
        _queryInterfaceHResult = Direct3D9Factory.GenericFailureHResult;
        using Direct3D9Objects objects = new(new TestModuleHandle(), direct3DObject.Direct3D, null);

        using Direct3D9Device device = objects.CreateDevice();

        Assert.AreEqual(
            (false, Pool.Managed, false, Pool.Managed, 1, 1, 0),
            (device.IsExtended, device.ManagedPool, device.IsExtended, device.ManagedPool,
                _queryInterfaceCallCount, _queryInterfaceResultReleaseCount, _createdDeviceReleaseCount));
    }

    [TestMethod]
    public void WhenBaseDeviceQueryForExtendedInterfaceSucceedsThenExtendedIdentityAndPoolAreCommitted()
    {
        using FakeDirect3DObject direct3DObject = new();
        using FakeDeviceObject createdDevice = new(&ReleaseCreatedDevice);
        using FakeDeviceObject queryResult = new(&ReleaseQueryInterfaceResult);
        _createdDevice = (nint) createdDevice.Device;
        _queryInterfaceResult = (nint) queryResult.DeviceEx;
        using Direct3D9Objects objects = new(new TestModuleHandle(), direct3DObject.Direct3D, null);
        Direct3D9Device device = objects.CreateDevice();

        (bool FirstIdentity, Pool FirstPool, bool SecondIdentity, Pool SecondPool) identity =
            (device.IsExtended, device.ManagedPool, device.IsExtended, device.ManagedPool);
        device.Dispose();

        Assert.AreEqual(
            (true, (Pool) 6, true, (Pool) 6, 1, 1, 1),
            (identity.FirstIdentity, identity.FirstPool, identity.SecondIdentity, identity.SecondPool,
                _queryInterfaceCallCount, _queryInterfaceResultReleaseCount, _createdDeviceReleaseCount));
    }

    [TestMethod]
    public void WhenExtendedDeviceQueryForBaseInterfaceFailsWithPointerThenBothReferencesAreReleased()
    {
        using FakeDirect3DObject direct3DObject = new();
        using FakeDeviceObject createdDevice = new(&ReleaseCreatedDevice);
        using FakeDeviceObject queryResult = new(&ReleaseQueryInterfaceResult);
        _createdDevice = (nint) createdDevice.DeviceEx;
        _queryInterfaceResult = (nint) queryResult.Device;
        _queryInterfaceHResult = Direct3D9Factory.GenericFailureHResult;
        using Direct3D9Objects objects = new(
            new TestModuleHandle(),
            direct3DObject.Direct3D,
            direct3DObject.Direct3DEx);

        COMException exception = Assert.ThrowsExactly<COMException>(() => objects.CreateDevice());

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, 1),
            (exception.HResult, _queryInterfaceResultReleaseCount, _createdDeviceReleaseCount));
    }

    [TestMethod]
    public void WhenCreatedDeviceCapabilitiesDifferFromFactoryCapabilitiesThenActualDeviceCapabilitiesAreCached()
    {
        using FakeDirect3DObject direct3DObject = new();
        using FakeDeviceObject createdDevice = new(&ReleaseCreatedDevice);
        _createdDevice = (nint) createdDevice.Device;
        _deviceCapabilities = new Caps9
        {
            AdapterOrdinal = 7,
            DeviceType = Devtype.SW,
            MaxTextureWidth = 4096,
            PixelShaderVersion = 0xFFFF0300
        };
        using Direct3D9Objects objects = new(new TestModuleHandle(), direct3DObject.Direct3D, null);

        using Direct3D9Device device = objects.CreateDevice();

        Assert.AreEqual(
            (1, true, 7u, Devtype.SW, 4096u, 0xFFFF0300u),
            (_deviceCapsCallCount, _deviceCapsOutputWasCleared, device.AdapterOrdinal, device.DeviceType,
                device.Capabilities.MaxTextureWidth, device.Capabilities.PixelShaderVersion));
    }

    [TestMethod]
    public void WhenCreatedDeviceCapabilitiesReturnNonzeroSuccessThenCapabilitiesAreCached()
    {
        using FakeDirect3DObject direct3DObject = new();
        using FakeDeviceObject createdDevice = new(&ReleaseCreatedDevice);
        _createdDevice = (nint) createdDevice.Device;
        _deviceCapsHResult = 1;
        _deviceCapabilities = new Caps9 { DeviceType = Devtype.SW, MaxTextureHeight = 2048 };
        using Direct3D9Objects objects = new(new TestModuleHandle(), direct3DObject.Direct3D, null);

        using Direct3D9Device device = objects.CreateDevice();

        Assert.AreEqual((1, 2048u), (_deviceCapsCallCount, device.Capabilities.MaxTextureHeight));
    }

    [TestMethod]
    [DataRow(Devtype.Hal)]
    [DataRow(Devtype.Ref)]
    public void WhenNonSoftwareDeviceDoesNotSupportPixelShader20ThenCreationFailsBeforeInterfaceAndBackBufferCommit(
        Devtype deviceType)
    {
        using FakeDirect3DObject direct3DObject = new();
        using FakeDeviceObject createdDevice = new(&ReleaseCreatedDevice);
        _createdDevice = (nint) createdDevice.Device;
        _deviceCapabilities = new Caps9
        {
            DeviceType = deviceType,
            PixelShaderVersion = 0xFFFF0104
        };
        using Direct3D9Objects objects = new(new TestModuleHandle(), direct3DObject.Direct3D, null);

        COMException exception = Assert.ThrowsExactly<COMException>(() => objects.CreateDevice());

        Assert.AreEqual(
            (Direct3D9Factory.InsufficientGpuCapabilitiesHResult, 1, 0, 0, 1),
            (exception.HResult, _deviceCapsCallCount, _getBackBufferCallCount,
                _queryInterfaceResultReleaseCount, _createdDeviceReleaseCount));
    }

    [TestMethod]
    public void WhenSoftwareDeviceDoesNotSupportPixelShader20ThenCreationContinues()
    {
        using FakeDirect3DObject direct3DObject = new();
        using FakeDeviceObject createdDevice = new(&ReleaseCreatedDevice);
        _createdDevice = (nint) createdDevice.Device;
        _deviceCapabilities = new Caps9
        {
            DeviceType = Devtype.SW,
            PixelShaderVersion = 0xFFFF0104
        };
        using Direct3D9Objects objects = new(new TestModuleHandle(), direct3DObject.Direct3D, null);

        using Direct3D9Device device = objects.CreateDevice();

        Assert.AreEqual(
            (Devtype.SW, 0xFFFF0104u, 1, 1),
            (device.DeviceType, device.PixelShaderVersion, _deviceCapsCallCount, _getBackBufferCallCount));
    }

    [TestMethod]
    public void WhenDeviceIsCreatedThenDummyBackBufferIsOwnedUntilDeviceDisposal()
    {
        using FakeDirect3DObject direct3DObject = new();
        using FakeDeviceObject createdDevice = new(&ReleaseCreatedDevice);
        _createdDevice = (nint) createdDevice.Device;
        using Direct3D9Objects objects = new(new TestModuleHandle(), direct3DObject.Direct3D, null);
        Direct3D9Device device = objects.CreateDevice();

        device.Dispose();

        Assert.AreEqual(
            (1, 0u, 0u, BackbufferType.Mono, 1, 1),
            (_getBackBufferCallCount, _backBufferSwapChainOrdinal, _backBufferOrdinal,
                _backBufferType, _backBufferReleaseCount, _createdDeviceReleaseCount));
    }

    [TestMethod]
    public void WhenDummyBackBufferRetrievalFailsWithPointerThenReferencesAreReleasedInCleanup()
    {
        using FakeDirect3DObject direct3DObject = new();
        using FakeDeviceObject createdDevice = new(&ReleaseCreatedDevice);
        _createdDevice = (nint) createdDevice.Device;
        _getBackBufferHResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Objects objects = new(new TestModuleHandle(), direct3DObject.Direct3D, null);

        COMException exception = Assert.ThrowsExactly<COMException>(() => objects.CreateDevice());

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, 1),
            (exception.HResult, _getBackBufferCallCount, _backBufferReleaseCount, _createdDeviceReleaseCount));
    }

    [TestMethod]
    public void WhenCreatedDeviceCapabilitiesFailThenCreatedDeviceIsReleasedAndFailureIsPropagated()
    {
        using FakeDirect3DObject direct3DObject = new();
        using FakeDeviceObject createdDevice = new(&ReleaseCreatedDevice);
        _createdDevice = (nint) createdDevice.Device;
        _deviceCapsHResult = Direct3D9Factory.DriverInternalErrorHResult;
        _deviceCapabilities = new Caps9 { MaxTextureWidth = uint.MaxValue };
        using Direct3D9Objects objects = new(new TestModuleHandle(), direct3DObject.Direct3D, null);

        COMException exception = Assert.ThrowsExactly<COMException>(() => objects.CreateDevice());

        Assert.AreEqual(
            (Direct3D9Factory.DriverInternalErrorHResult, 1, true, 1),
            (exception.HResult, _deviceCapsCallCount, _deviceCapsOutputWasCleared, _createdDeviceReleaseCount));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(void* self) => 1;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDirect3D(void* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetAdapterDisplayMode(IDirect3D9* self, uint adapter, Displaymode* displayMode)
    {
        *displayMode = new Displaymode(1, 1, 60, Format.X8R8G8B8);
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetDeviceCaps(IDirect3D9* self, uint adapter, Devtype deviceType, Caps9* caps)
    {
        *caps = default;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateDevice(
        IDirect3D9* self,
        uint adapter,
        Devtype deviceType,
        nint focusWindow,
        uint behaviorFlags,
        PresentParameters* presentParameters,
        IDirect3DDevice9** device)
    {
        *device = (IDirect3DDevice9*) _createdDevice;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateDeviceEx(
        IDirect3D9Ex* self,
        uint adapter,
        Devtype deviceType,
        nint focusWindow,
        uint behaviorFlags,
        PresentParameters* presentParameters,
        Displaymodeex* fullscreenDisplayMode,
        IDirect3DDevice9Ex** device)
    {
        *device = (IDirect3DDevice9Ex*) _createdDevice;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int QueryInterface(IDirect3DDevice9* self, Guid* interfaceId, void** result)
    {
        _queryInterfaceCallCount++;
        *result = (void*) _queryInterfaceResult;
        return _queryInterfaceHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetCreatedDeviceCaps(IDirect3DDevice9* self, Caps9* capabilities)
    {
        _deviceCapsCallCount++;
        _deviceCapsOutputWasCleared = capabilities->AdapterOrdinal == 0
            && capabilities->DeviceType == default
            && capabilities->MaxTextureWidth == 0
            && capabilities->PixelShaderVersion == 0;
        *capabilities = _deviceCapabilities;
        return _deviceCapsHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetBackBuffer(
        IDirect3DDevice9* self,
        uint swapChainOrdinal,
        uint backBufferOrdinal,
        BackbufferType type,
        IDirect3DSurface9** backBuffer)
    {
        _getBackBufferCallCount++;
        _backBufferSwapChainOrdinal = swapChainOrdinal;
        _backBufferOrdinal = backBufferOrdinal;
        _backBufferType = type;
        *backBuffer = (IDirect3DSurface9*) _dummyBackBuffer;
        return _getBackBufferHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseCreatedDevice(void* self)
    {
        _createdDeviceReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseBackBuffer(void* self)
    {
        _backBufferReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseQueryInterfaceResult(void* self)
    {
        _queryInterfaceResultReleaseCount++;
        return 0;
    }

    private sealed class TestModuleHandle : SafeHandle
    {
        internal TestModuleHandle()
            : base(0, true)
        {
            SetHandle(1);
        }

        public override bool IsInvalid => false;

        protected override bool ReleaseHandle()
        {
            handle = 0;
            return true;
        }
    }

    private struct FakeDirect3DObject : IDisposable
    {
        private nint _baseMemory;
        private nint _extendedMemory;
        internal IDirect3D9* Direct3D;
        internal IDirect3D9Ex* Direct3DEx;

        public FakeDirect3DObject()
        {
            _baseMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 24);
            void** baseMemory = (void**) _baseMemory;
            Direct3D = (IDirect3D9*) baseMemory;
            void** baseVtable = baseMemory + 1;
            Direct3D->LpVtbl = baseVtable;
            baseVtable[1] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &AddRef;
            baseVtable[2] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &ReleaseDirect3D;
            baseVtable[8] = (void*) (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Displaymode*, int>) &GetAdapterDisplayMode;
            baseVtable[14] = (void*) (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Caps9*, int>) &GetDeviceCaps;
            baseVtable[16] = (void*) (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, nint, uint, PresentParameters*, IDirect3DDevice9**, int>) &CreateDevice;

            _extendedMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 24);
            void** extendedMemory = (void**) _extendedMemory;
            Direct3DEx = (IDirect3D9Ex*) extendedMemory;
            void** extendedVtable = extendedMemory + 1;
            Direct3DEx->LpVtbl = extendedVtable;
            extendedVtable[1] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &AddRef;
            extendedVtable[2] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &ReleaseDirect3D;
            extendedVtable[20] = (void*) (delegate* unmanaged[Stdcall]<IDirect3D9Ex*, uint, Devtype, nint, uint, PresentParameters*, Displaymodeex*, IDirect3DDevice9Ex**, int>) &CreateDeviceEx;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _extendedMemory);
            NativeMemory.Free((void*) _baseMemory);
            _extendedMemory = 0;
            _baseMemory = 0;
            Direct3D = null;
            Direct3DEx = null;
        }
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        private nint _dummyBackBufferMemory;
        internal IDirect3DDevice9* Device;
        internal IDirect3DDevice9Ex* DeviceEx;
        internal IDirect3DSurface9* DummyBackBuffer;

        public FakeDeviceObject(delegate* unmanaged[Stdcall]<void*, uint> release)
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 20);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            DeviceEx = (IDirect3DDevice9Ex*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[0] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Guid*, void**, int>) &QueryInterface;
            vtable[2] = (void*) release;
            vtable[7] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Caps9*, int>) &GetCreatedDeviceCaps;
            vtable[18] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, BackbufferType, IDirect3DSurface9**, int>) &GetBackBuffer;

            _dummyBackBufferMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** dummyBackBufferMemory = (void**) _dummyBackBufferMemory;
            DummyBackBuffer = (IDirect3DSurface9*) dummyBackBufferMemory;
            void** dummyBackBufferVtable = dummyBackBufferMemory + 1;
            DummyBackBuffer->LpVtbl = dummyBackBufferVtable;
            dummyBackBufferVtable[2] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &ReleaseBackBuffer;
            _dummyBackBuffer = (nint) DummyBackBuffer;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _dummyBackBufferMemory);
            NativeMemory.Free((void*) _memory);
            _dummyBackBufferMemory = 0;
            _memory = 0;
            Device = null;
            DeviceEx = null;
            DummyBackBuffer = null;
        }
    }
}
