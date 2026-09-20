using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceDepthStencilStateTests
{
    private static List<string>? _surfaceReleaseEvents;
    private static nint _nativeDepthStencilSurface;
    private static int _nativeDepthStencilResult;
    private static int _nativeDepthStencilCallCount;
    private static uint _surfaceUsage = D3D9.UsageDepthstencil;

    [TestMethod]
    public void WhenNullDepthStencilSurfaceIsSetTwiceThenFirstNativeCallIsMadeAndMatchingCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice(surface =>
        {
            Assert.AreEqual(0, (nint) surface);
            callCount++;
            return 0;
        });

        int firstResult = device.SetDepthStencilSurfaceInline(null, 0, 0);
        int secondResult = device.SetDepthStencilSurfaceInline(null, 16, 24);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public void WhenDepthStencilSurfaceSetReturnsNonzeroSuccessThenStateAndDimensionsAreCached()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice(_ =>
        {
            callCount++;
            return 1;
        });
        IDirect3DSurface9* surface = (IDirect3DSurface9*) 1;

        int firstResult = device.SetDepthStencilSurfaceInline(surface, 16, 24);
        int secondResult = device.SetDepthStencilSurfaceInline(surface, 32, 48);
        bool isSmaller = device.IsDepthStencilSurfaceSmallerThan(17, 24);

        Assert.AreEqual((1, 0, true, 1), (firstResult, secondResult, isSmaller, callCount));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenForcingDepthStencilSurfaceFailsThenOriginalHResultIsPreservedWithoutErrorMapping(int failureHResult)
    {
        using Direct3D9Device device = CreateDevice(_ => failureHResult);

        int result = device.ForceSetDepthStencilSurface((IDirect3DSurface9*) 1, 16, 24);

        Assert.AreEqual((failureHResult, 0), (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenSettingDepthStencilSurfaceThenNativeSlot39ReceivesExactPointer()
    {
        _nativeDepthStencilSurface = 0;
        _nativeDepthStencilResult = 1;
        _nativeDepthStencilCallCount = 0;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);
        IDirect3DSurface9* surface = (IDirect3DSurface9*) 0x1234;

        int result = device.SetDepthStencilSurfaceInline(surface, uint.MaxValue, uint.MaxValue);

        Assert.AreEqual((1, (nint) surface, 1), (result, _nativeDepthStencilSurface, _nativeDepthStencilCallCount));
    }

    [TestMethod]
    public void WhenDepthStencilSurfaceIsSetTwiceThenMatchingNativeCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice(_ =>
        {
            callCount++;
            return 0;
        });
        IDirect3DSurface9* surface = (IDirect3DSurface9*) 1;

        int firstResult = device.SetDepthStencilSurfaceInline(surface, 16, 24);
        int secondResult = device.SetDepthStencilSurfaceInline(surface, 32, 48);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public void WhenDepthStencilSurfaceChangesThenNativeCallIsRepeated()
    {
        List<nint> calls = [];
        using Direct3D9Device device = CreateDevice(surface =>
        {
            calls.Add((nint) surface);
            return 0;
        });

        _ = device.SetDepthStencilSurfaceInline((IDirect3DSurface9*) 1, 16, 24);
        _ = device.SetDepthStencilSurfaceInline((IDirect3DSurface9*) 2, 32, 48);

        CollectionAssert.AreEqual(new nint[] { 1, 2 }, calls);
    }

    [TestMethod]
    public void WhenDepthStencilSurfaceSetFailsThenStateIsUnknownAndRetried()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice(_ =>
        {
            callCount++;
            return callCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
        });
        IDirect3DSurface9* surface = (IDirect3DSurface9*) 1;

        int failedResult = device.SetDepthStencilSurfaceInline(surface, 16, 24);
        bool isSmallerWhileUnknown = device.IsDepthStencilSurfaceSmallerThan(1, 1);
        int retryResult = device.SetDepthStencilSurfaceInline(surface, 16, 24);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, 0, 2),
            (failedResult, isSmallerWhileUnknown, retryResult, callCount));
    }

    [TestMethod]
    public void WhenKnownDepthStencilSurfaceHasSmallerDimensionThenSizeCheckReturnsTrue()
    {
        using Direct3D9Device device = CreateDevice(_ => 0);
        _ = device.SetDepthStencilSurfaceInline((IDirect3DSurface9*) 1, 16, 24);

        bool widthIsSmaller = device.IsDepthStencilSurfaceSmallerThan(17, 24);
        bool heightIsSmaller = device.IsDepthStencilSurfaceSmallerThan(16, 25);
        bool surfaceIsLargeEnough = device.IsDepthStencilSurfaceSmallerThan(16, 24);

        Assert.AreEqual((true, true, false), (widthIsSmaller, heightIsSmaller, surfaceIsLargeEnough));
    }

    [TestMethod]
    public void WhenNoDepthStencilSurfaceIsKnownThenSizeCheckReturnsFalse()
    {
        using Direct3D9Device device = CreateDevice(_ => 0);
        _ = device.ForceSetDepthStencilSurface(null, 0, 0);

        bool result = device.IsDepthStencilSurfaceSmallerThan(uint.MaxValue, uint.MaxValue);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void WhenMatchingDepthStencilSurfaceUseIsReleasedThenNativeSurfaceIsCleared()
    {
        List<nint> calls = [];
        using Direct3D9Device device = CreateDevice(surface =>
        {
            calls.Add((nint) surface);
            return 0;
        });
        IDirect3DSurface9* surface = (IDirect3DSurface9*) 1;
        _ = device.SetDepthStencilSurfaceInline(surface, 16, 24);

        device.ReleaseUseOfDepthStencilBuffer(surface);

        CollectionAssert.AreEqual(new nint[] { 1, 0 }, calls);
    }

    [TestMethod]
    public void WhenDifferentDepthStencilSurfaceUseIsReleasedThenNativeCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice(_ =>
        {
            callCount++;
            return 0;
        });
        _ = device.SetDepthStencilSurfaceInline((IDirect3DSurface9*) 1, 16, 24);

        int result = device.ReleaseUseOfDepthStencilBuffer((IDirect3DSurface9*) 2);

        Assert.AreEqual((0, 1), (result, callCount));
    }

    [TestMethod]
    public void WhenNullDepthStencilSurfaceUseIsReleasedThenNativeCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice(_ =>
        {
            callCount++;
            return 0;
        });
        _ = device.SetDepthStencilSurfaceInline(null, 0, 0);

        int result = device.ReleaseUseOfDepthStencilBuffer(null);

        Assert.AreEqual((0, 1), (result, callCount));
    }

    [TestMethod]
    public void WhenMatchingDepthStencilReleaseReturnsNonzeroSuccessThenResultIsPreservedAndNullStateIsCached()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice(_ =>
        {
            callCount++;
            return callCount == 1 ? 0 : 1;
        });
        IDirect3DSurface9* surface = (IDirect3DSurface9*) 1;
        _ = device.SetDepthStencilSurfaceInline(surface, 16, 24);

        int releaseResult = device.ReleaseUseOfDepthStencilBuffer(surface);
        int cachedResult = device.SetDepthStencilSurfaceInline(null, 16, 24);

        Assert.AreEqual((1, 0, 2), (releaseResult, cachedResult, callCount));
    }

    [TestMethod]
    public void WhenMatchingDepthStencilReleaseFailsThenOriginalFailureIsReturnedAndReleaseCanBeRetried()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice(_ =>
        {
            callCount++;
            return callCount == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
        });
        IDirect3DSurface9* surface = (IDirect3DSurface9*) 1;
        _ = device.SetDepthStencilSurfaceInline(surface, 16, 24);

        int failedResult = device.ReleaseUseOfDepthStencilBuffer(surface);
        int retryResult = device.SetDepthStencilSurfaceInline(null, 0, 0);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 3),
            (failedResult, retryResult, callCount));
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenDepthStencilReleaseIsRejected()
    {
        Direct3D9Device device = CreateDevice(_ => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.ReleaseUseOfDepthStencilBuffer((IDirect3DSurface9*) 1));
    }

    [TestMethod]
    public void WhenDepthStencilSurfaceIsForcedThenMatchingRegularCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice(_ =>
        {
            callCount++;
            return 0;
        });
        IDirect3DSurface9* surface = (IDirect3DSurface9*) 1;

        int forcedResult = device.ForceSetDepthStencilSurface(surface, 16, 24);
        int cachedResult = device.SetDepthStencilSurfaceInline(surface, 16, 24);

        Assert.AreEqual((0, 0, 1), (forcedResult, cachedResult, callCount));
    }

    [TestMethod]
    public void WhenDepthStencilSurfaceIsSetThenDepthIsEnabledBeforeBinding()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(
            surface =>
            {
                calls.Add($"Depth:{(nint) surface}");
                return 0;
            },
            (state, value) =>
            {
                calls.Add($"State:{(uint) state}:{value}");
                return 0;
            });

        int result = device.SetDepthStencilSurface((IDirect3DSurface9*) 1);

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(
            new[] { "State:7:1", "Depth:1" },
            calls);
    }

    [TestMethod]
    public void WhenDepthStencilSurfaceIsClearedThenDepthAndStencilAreDisabledBeforeUnbinding()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(
            surface =>
            {
                calls.Add($"Depth:{(nint) surface}");
                return 0;
            },
            (state, value) =>
            {
                calls.Add($"State:{(uint) state}:{value}");
                return 0;
            });

        int result = device.SetDepthStencilSurface(null);

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(
            new[] { "State:7:0", "State:52:0", "Depth:0" },
            calls);
    }

    [TestMethod]
    public void WhenDepthStencilBindingFailsThenOriginalFailureIsPreservedAndStateIsCleared()
    {
        List<string> calls = [];
        int depthCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            surface =>
            {
                calls.Add($"Depth:{(nint) surface}");
                depthCallCount++;
                return depthCallCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
            },
            (state, value) =>
            {
                calls.Add($"State:{(uint) state}:{value}");
                return 0;
            });

        int result = device.SetDepthStencilSurface((IDirect3DSurface9*) 1);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(
            new[]
            {
                "State:7:1",
                "Depth:1",
                "State:7:0",
                "State:52:0",
                "Depth:0"
            },
            calls);
    }

    [TestMethod]
    public void WhenDepthStencilCleanupFailsThenOriginalBindingFailureIsPreserved()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(
            surface =>
            {
                calls.Add($"Depth:{(nint) surface}");
                return Direct3D9Factory.GenericFailureHResult;
            },
            (state, value) =>
            {
                calls.Add($"State:{(uint) state}:{value}");
                return value == 0 ? Direct3D9Factory.DriverInternalErrorHResult : 0;
            });

        int result = device.SetDepthStencilSurface((IDirect3DSurface9*) 1);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(
            new[]
            {
                "State:7:1",
                "Depth:1",
                "State:7:0",
                "State:52:0",
                "Depth:0"
            },
            calls);
    }

    [TestMethod]
    public void WhenDepthEnableFailsThenBindingIsSkippedAndCleanupOrderIsPreserved()
    {
        List<string> calls = [];
        int stateCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            surface =>
            {
                calls.Add($"Depth:{(nint) surface}");
                return 0;
            },
            (state, value) =>
            {
                calls.Add($"State:{(uint) state}:{value}");
                stateCallCount++;
                return stateCallCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        int result = device.SetDepthStencilSurface((IDirect3DSurface9*) 1);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(
            new[] { "State:7:1", "State:7:0", "State:52:0", "Depth:0" },
            calls);
    }

    [TestMethod]
    public void WhenStencilDisableFailsThenPrimaryUnbindIsSkippedAndCleanupOrderIsPreserved()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(
            surface =>
            {
                calls.Add($"Depth:{(nint) surface}");
                return 0;
            },
            (state, value) =>
            {
                calls.Add($"State:{(uint) state}:{value}");
                return state == Renderstatetype.Stencilenable
                    ? Direct3D9Factory.GenericFailureHResult
                    : 0;
            });

        int result = device.SetDepthStencilSurface(null);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(
            new[] { "State:7:0", "State:52:0", "State:52:0", "Depth:0" },
            calls);
    }

    [TestMethod]
    public void WhenBindingFailsThenReleaseNotificationDoesNotTrustUnknownStateAndRetryStillBinds()
    {
        List<nint> calls = [];
        using Direct3D9Device device = CreateDevice(surface =>
        {
            calls.Add((nint) surface);
            return calls.Count == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
        }, (_, _) => 0);
        IDirect3DSurface9* surface = (IDirect3DSurface9*) 1;

        int failedResult = device.SetDepthStencilSurface(surface);
        device.ReleaseUseOfDepthStencilBuffer(surface);
        int retryResult = device.SetDepthStencilSurface(surface);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (failedResult, retryResult));
        CollectionAssert.AreEqual(new nint[] { 1, 0, 1 }, calls);
    }

    [TestMethod]
    public void WhenCurrentTrackingDiffersThenReleaseStillUnbindsMatchingStateManagerSurface()
    {
        List<nint> calls = [];
        using Direct3D9Device device = CreateDevice(surface =>
        {
            calls.Add((nint) surface);
            return 0;
        });
        IDirect3DSurface9* trackedSurface = (IDirect3DSurface9*) 1;
        IDirect3DSurface9* stateManagerSurface = (IDirect3DSurface9*) 2;
        _ = device.SetDepthStencilSurfaceForCurrentRenderTarget(trackedSurface, 16, 24);
        _ = device.ForceSetDepthStencilSurface(stateManagerSurface, 32, 48);

        int result = device.ReleaseUseOfDepthStencilBuffer(stateManagerSurface);

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(new nint[] { 1, 2, 0 }, calls);
    }

    [TestMethod]
    public void WhenDepthStencilSurfaceHasDriverInternalErrorThenFailureIsMapped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice(
            _ =>
            {
                callCount++;
                return Direct3D9Factory.DriverInternalErrorHResult;
            },
            (_, _) => 0);

        int result = device.SetDepthStencilSurface((IDirect3DSurface9*) 1);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 2),
            (result, device.UnusableReasonHResult, callCount));
    }

    [TestMethod]
    public void WhenDeviceOwnsCurrentDepthStencilResourceThenDisposeUnbindsBeforeSurfaceAndDeviceRelease()
    {
        List<string> events = [];
        _surfaceReleaseEvents = events;
        using FakeDeviceObject deviceObject = new();
        using FakeDepthStencilSurface surfaceObject = new();
        Direct3D9Device device = new(
            deviceObject.Device,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setDepthStencilSurface: surface =>
            {
                events.Add(surface is null ? "Depth:None" : "Depth:Set");
                return 0;
            });
        int createResult = Direct3D9Surface.TryCreate(
            device.ResourceManager,
            surfaceObject.Surface,
            out Direct3D9Surface? surface);
        _ = device.SetDepthStencilSurfaceForCurrentRenderTarget(surface!.SurfaceForDeviceCall, 16, 24);

        device.Dispose();
        device.Dispose();
        surface.Dispose();

        Assert.AreEqual(0, createResult);
        CollectionAssert.AreEqual(
            new[] { "Depth:Set", "Depth:None", "Surface:Release", "Device:Release" },
            events);
        _surfaceReleaseEvents = null;
    }

    [TestMethod]
    public void WhenCurrentRenderTargetAndDepthStencilShareOneResourceThenDisposeReleasesItOnce()
    {
        List<string> events = [];
        _surfaceReleaseEvents = events;
        _surfaceUsage = D3D9.UsageRendertarget | D3D9.UsageDepthstencil;
        using FakeDepthStencilSurface surfaceObject = new();
        Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            getRenderTargetDescription: surface => surface.GetDescription(),
            setRenderTarget: _ => 0,
            setViewport: _ => 0,
            setSurfaceToClippingMatrix: _ => 0,
            setDepthStencilSurface: surface =>
            {
                events.Add(surface is null ? "Depth:None" : "Depth:Set");
                return 0;
            });
        Assert.AreEqual(0, Direct3D9Surface.TryCreate(
            device.ResourceManager,
            surfaceObject.Surface,
            out Direct3D9Surface? surface));
        Assert.IsNotNull(surface);
        Assert.AreEqual(0, device.SetRenderTarget(surface));
        Assert.AreEqual(0, device.SetDepthStencilSurfaceForCurrentRenderTarget(surface.SurfaceForDeviceCall, 16, 24));

        device.Dispose();
        device.Dispose();
        surface.Dispose();

        CollectionAssert.AreEqual(new[] { "Depth:Set", "Depth:None", "Surface:Release" }, events);
        _surfaceUsage = D3D9.UsageDepthstencil;
        _surfaceReleaseEvents = null;
    }

    [TestMethod]
    public void WhenDepthStencilUnbindFailsDuringDeviceDisposeThenSurfaceIsStillReleased()
    {
        List<string> events = [];
        _surfaceReleaseEvents = events;
        using FakeDepthStencilSurface surfaceObject = new();
        int depthCallCount = 0;
        Direct3D9Device device = CreateDevice(surface =>
        {
            depthCallCount++;
            events.Add(surface is null ? "Depth:None" : "Depth:Set");
            return depthCallCount == 1 ? 0 : Direct3D9Factory.GenericFailureHResult;
        });
        _ = Direct3D9Surface.TryCreate(
            device.ResourceManager,
            surfaceObject.Surface,
            out Direct3D9Surface? surface);
        _ = device.SetDepthStencilSurfaceForCurrentRenderTarget(surface!.SurfaceForDeviceCall, 16, 24);

        device.Dispose();

        CollectionAssert.AreEqual(new[] { "Depth:Set", "Depth:None", "Surface:Release" }, events);
        surface.Dispose();
        _surfaceReleaseEvents = null;
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenDepthStencilStateOperationsThrowObjectDisposedException()
    {
        Direct3D9Device device = CreateDevice(_ => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.SetDepthStencilSurfaceInline(null, 0, 0));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSurface(IDirect3DSurface9* self)
    {
        _surfaceReleaseEvents!.Add("Surface:Release");
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceDescription(IDirect3DSurface9* self, SurfaceDesc* description)
    {
        *description = new SurfaceDesc(
            Format.D24S8,
            Resourcetype.Surface,
            _surfaceUsage,
            Pool.Default,
            MultisampleType.MultisampleNone,
            0,
            16,
            24);
        return 0;
    }

    private struct FakeDepthStencilSurface : IDisposable
    {
        private nint _memory;
        internal IDirect3DSurface9* Surface;

        public FakeDepthStencilSurface()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 14);
            void** memory = (void**) _memory;
            Surface = (IDirect3DSurface9*) memory;
            void** vtable = memory + 1;
            Surface->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseSurface;
            vtable[12] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) &GetSurfaceDescription;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }

    private static Direct3D9Device CreateDevice(
        Direct3D9SetDepthStencilSurface setDepthStencilSurface,
        Direct3D9SetRenderState? setRenderState = null)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setDepthStencilSurface: setDepthStencilSurface,
            setRenderState: setRenderState);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self)
    {
        _surfaceReleaseEvents?.Add("Device:Release");
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetDepthStencilSurface(IDirect3DDevice9* self, IDirect3DSurface9* surface)
    {
        _nativeDepthStencilCallCount++;
        _nativeDepthStencilSurface = (nint) surface;
        return _nativeDepthStencilResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 42);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[39] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, int>) &SetDepthStencilSurface;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
