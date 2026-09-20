using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceRenderStateTests
{
    private static readonly List<(Renderstatetype State, uint Value)> NativeCalls = [];
    private static int _nativeResult;

    [TestInitialize]
    public void Initialize()
    {
        NativeCalls.Clear();
        _nativeResult = 0;
    }

    [TestMethod]
    public void WhenInitialRenderStateValueIsZeroThenNativeCallRuns()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return 0;
        });

        int result = device.SetRenderState(Renderstatetype.Zenable, 0);

        Assert.AreEqual((0, 1), (result, callCount));
    }

    [TestMethod]
    public unsafe void WhenRenderStateIsSetTwiceThenMatchingNativeCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return 0;
        });

        int firstResult = device.SetRenderState(Renderstatetype.Cullmode, 1);
        int secondResult = device.SetRenderState(Renderstatetype.Cullmode, 1);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public void WhenMatchingRenderStateIsForcedTwiceThenBothNativeCallsRun()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return 0;
        });

        int firstResult = device.ForceSetRenderState(Renderstatetype.Cullmode, 1);
        int secondResult = device.ForceSetRenderState(Renderstatetype.Cullmode, 1);

        Assert.AreEqual((0, 0, 2), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenRenderStateOrValueChangesThenNativeCallIsRepeated()
    {
        List<(Renderstatetype State, uint Value)> calls = [];
        using Direct3D9Device device = CreateDevice((state, value) =>
        {
            calls.Add((state, value));
            return 0;
        });

        _ = device.SetRenderState(Renderstatetype.Cullmode, 1);
        _ = device.SetRenderState(Renderstatetype.Cullmode, 2);
        _ = device.SetRenderState(Renderstatetype.Zfunc, 2);

        CollectionAssert.AreEqual(
            new[]
            {
                (Renderstatetype.Cullmode, 1u),
                (Renderstatetype.Cullmode, 2u),
                (Renderstatetype.Zfunc, 2u)
            },
            calls);
    }

    [TestMethod]
    public unsafe void WhenRenderStateSetFailsThenMatchingStateIsRetried()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return callCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
        });

        int failedResult = device.SetRenderState(Renderstatetype.Zwriteenable, 1);
        int retryResult = device.SetRenderState(Renderstatetype.Zwriteenable, 1);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 2), (failedResult, retryResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenForcedRenderStateFailsAfterKnownValueThenStateBecomesUnknownAndMatchingCallIsRetried()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return callCount == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
        });

        int initialResult = device.SetRenderState(Renderstatetype.Zwriteenable, 1);
        int forcedResult = device.ForceSetRenderState(Renderstatetype.Zwriteenable, 1);
        int getResult = device.GetRenderState(Renderstatetype.Zwriteenable, out uint cachedValue);
        int retryResult = device.SetRenderState(Renderstatetype.Zwriteenable, 1);

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult, 0u, 0, 3),
            (initialResult, forcedResult, getResult, cachedValue, retryResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenRenderStateIsForcedThenMatchingRegularCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return 0;
        });

        int forcedResult = device.ForceSetRenderState(Renderstatetype.Multisampleantialias, 1);
        int cachedResult = device.SetRenderState(Renderstatetype.Multisampleantialias, 1);

        Assert.AreEqual((0, 0, 1), (forcedResult, cachedResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenUnsupportedRegularRenderStateIsUsedThenSettingThrowsArgumentOutOfRangeException()
    {
        using Direct3D9Device device = CreateDevice((_, _) => 0);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            device.SetRenderState(Renderstatetype.Shademode, 2));
    }

    [TestMethod]
    public unsafe void WhenUnsupportedDefaultRenderStateIsForcedThenNativeCallSucceeds()
    {
        List<(Renderstatetype State, uint Value)> calls = [];
        using Direct3D9Device device = CreateDevice((state, value) =>
        {
            calls.Add((state, value));
            return 0;
        });

        int result = device.ForceSetRenderState(Renderstatetype.Shademode, 2);

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(new[] { (Renderstatetype.Shademode, 2u) }, calls);
    }

    [TestMethod]
    public void WhenRenderStateIsUnknownThenGettingReturnsFailureAndZero()
    {
        using Direct3D9Device device = CreateDevice((_, _) => 0);

        int result = device.GetRenderState(Renderstatetype.Cullmode, out uint value);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0u), (result, value));
    }

    [TestMethod]
    public void WhenRenderStateSetSucceedsThenGettingReturnsCachedValueWithoutAnotherNativeCall()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return 1;
        });
        _ = device.SetRenderState(Renderstatetype.Blendfactor, uint.MaxValue);

        int result = device.GetRenderState(Renderstatetype.Blendfactor, out uint value);

        Assert.AreEqual((0, uint.MaxValue, 1), (result, value, callCount));
    }

    [TestMethod]
    public void WhenRenderStateSetFailsThenGettingReturnsFailureAndZero()
    {
        using Direct3D9Device device = CreateDevice((_, _) => Direct3D9Factory.DeviceLostHResult);
        _ = device.SetRenderState(Renderstatetype.Zwriteenable, uint.MaxValue);

        int result = device.GetRenderState(Renderstatetype.Zwriteenable, out uint value);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0u), (result, value));
    }

    [TestMethod]
    public void WhenUnsupportedRenderStateIsGottenThenArgumentOutOfRangeExceptionIsThrown()
    {
        using Direct3D9Device device = CreateDevice((_, _) => 0);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            device.GetRenderState(Renderstatetype.Shademode, out _));
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenGettingRenderStateThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateDevice((_, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.GetRenderState(Renderstatetype.Cullmode, out _));
    }

    [TestMethod]
    public unsafe void WhenDeviceIsDisposedThenSettingRenderStateThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateDevice((_, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.SetRenderState(Renderstatetype.Cullmode, 1));
    }

    [TestMethod]
    public void WhenSettingRenderStateThenNativeSlot57ReceivesExactParameters()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.SetRenderState(Renderstatetype.Blendfactor, uint.MaxValue);

        Assert.AreEqual(
            (0, 1, (Renderstatetype.Blendfactor, uint.MaxValue)),
            (result, NativeCalls.Count, NativeCalls[0]));
    }

    [TestMethod]
    public void WhenRenderStateReturnsNonzeroSuccessThenStateIsCached()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _nativeResult = 1;

        int firstResult = device.SetRenderState(Renderstatetype.Colorwriteenable, 0xFEDCBA98);
        int secondResult = device.SetRenderState(Renderstatetype.Colorwriteenable, 0xFEDCBA98);

        Assert.AreEqual((1, 0, 1, 0), (firstResult, secondResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenNativeRenderStateSetFailsThenOriginalHResultIsPreservedAndStateIsRetried(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _nativeResult = failureHResult;

        int firstResult = device.SetRenderState(Renderstatetype.Multisampleantialias, uint.MaxValue);
        int secondResult = device.SetRenderState(Renderstatetype.Multisampleantialias, uint.MaxValue);

        Assert.AreEqual(
            (failureHResult, failureHResult, 2, 0),
            (firstResult, secondResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenSettingRenderStateAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.SetRenderState(Renderstatetype.Cullmode, 3);

        Assert.AreEqual(
            (0, 1, (Renderstatetype.Cullmode, 3u)),
            (result, NativeCalls.Count, NativeCalls[0]));
    }

    [TestMethod]
    public void WhenSettingRenderStateWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.SetRenderState(Renderstatetype.Zenable, 1));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    [TestMethod]
    public void WhenForcingRenderStateWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.ForceSetRenderState(Renderstatetype.Zenable, 1));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    private static Direct3D9Device CreateDevice(Direct3D9SetRenderState setRenderState)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setRenderState: setRenderState);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(
            device,
            null,
            0,
            Devtype.Hal,
            0,
            default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetRenderState(IDirect3DDevice9* self, Renderstatetype state, uint value)
    {
        NativeCalls.Add((state, value));
        return _nativeResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 59);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[57] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Renderstatetype, uint, int>) &SetRenderState;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
