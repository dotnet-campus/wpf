using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceSamplerStateTests
{
    private static readonly List<(uint Sampler, Samplerstatetype State, uint Value)> NativeCalls = [];
    private static int _nativeResult;

    [TestInitialize]
    public void Initialize()
    {
        NativeCalls.Clear();
        _nativeResult = 0;
    }

    [TestMethod]
    public unsafe void WhenSamplerStateIsInitiallyZeroThenFirstNativeCallIsNotSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 0;
        });

        int result = device.SetSamplerState(0, Samplerstatetype.Bordercolor, 0);

        Assert.AreEqual((0, 1), (result, callCount));
    }

    [TestMethod]
    public unsafe void WhenSamplerStateIsSetTwiceThenMatchingNativeCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 0;
        });

        int firstResult = device.SetSamplerState(0, Samplerstatetype.Magfilter, 2);
        int secondResult = device.SetSamplerState(0, Samplerstatetype.Magfilter, 2);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenSamplerOrStateChangesThenNativeCallIsRepeated()
    {
        List<(uint Sampler, Samplerstatetype State)> calls = [];
        using Direct3D9Device device = CreateDevice((sampler, state, _) =>
        {
            calls.Add((sampler, state));
            return 0;
        });

        _ = device.SetSamplerState(0, Samplerstatetype.Magfilter, 2);
        _ = device.SetSamplerState(1, Samplerstatetype.Magfilter, 2);
        _ = device.SetSamplerState(0, Samplerstatetype.Minfilter, 2);

        CollectionAssert.AreEqual(
            new[]
            {
                (0u, Samplerstatetype.Magfilter),
                (1u, Samplerstatetype.Magfilter),
                (0u, Samplerstatetype.Minfilter)
            },
            calls);
    }

    [TestMethod]
    public unsafe void WhenSamplerStateSetFailsThenMatchingStateIsRetried()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return callCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
        });

        int failedResult = device.SetSamplerState(0, Samplerstatetype.Addressu, 1);
        int retryResult = device.SetSamplerState(0, Samplerstatetype.Addressu, 1);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 2), (failedResult, retryResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenMaximumSamplerIsUsedThenNativeCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 0;
        });

        int result = device.SetSamplerState(4, Samplerstatetype.Magfilter, 2);

        Assert.AreEqual((0, 0), (result, callCount));
    }

    [TestMethod]
    public unsafe void WhenSamplerIsBeyondMaximumThenSettingStateThrowsArgumentOutOfRangeException()
    {
        using Direct3D9Device device = CreateDevice((_, _, _) => 0);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            device.SetSamplerState(5, Samplerstatetype.Magfilter, 2));
    }

    [TestMethod]
    public unsafe void WhenUnsupportedSamplerStateIsUsedThenSettingStateThrowsArgumentOutOfRangeException()
    {
        using Direct3D9Device device = CreateDevice((_, _, _) => 0);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            device.SetSamplerState(0, Samplerstatetype.Maxanisotropy, 4));
    }

    [TestMethod]
    public unsafe void WhenSamplerStateIsForcedThenMatchingRegularCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 0;
        });

        int forcedResult = device.ForceSetSamplerState(0, Samplerstatetype.Bordercolor, 0x12345678);
        int cachedResult = device.SetSamplerState(0, Samplerstatetype.Bordercolor, 0x12345678);

        Assert.AreEqual((0, 0, 1), (forcedResult, cachedResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenForcedSamplerStateFailsAfterKnownValueThenMatchingRegularCallIsRetried()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return callCount == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
        });

        int initialResult = device.SetSamplerState(0, Samplerstatetype.Bordercolor, 0x12345678);
        int forcedResult = device.ForceSetSamplerState(0, Samplerstatetype.Bordercolor, 0x12345678);
        int retryResult = device.SetSamplerState(0, Samplerstatetype.Bordercolor, 0x12345678);

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, 0, 3),
            (initialResult, forcedResult, retryResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenDeviceIsDisposedThenSettingSamplerStateThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateDevice((_, _, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.SetSamplerState(0, Samplerstatetype.Magfilter, 2));
    }

    [TestMethod]
    public unsafe void WhenDeviceIsDisposedThenForcingSamplerStateThrowsWithoutNativeCall()
    {
        int callCount = 0;
        Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 0;
        });
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.ForceSetSamplerState(0, Samplerstatetype.Maxanisotropy, 4));
        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public void WhenSettingSamplerStateThenNativeSlot69ReceivesExactParameters()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.SetSamplerState(7, Samplerstatetype.Bordercolor, uint.MaxValue);

        Assert.AreEqual((0, "7:SampBordercolor:4294967295"), (result, FormatNativeCalls()));
    }

    [TestMethod]
    public void WhenSamplerStateReturnsNonzeroSuccessThenStateIsCached()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _nativeResult = 1;

        int firstResult = device.SetSamplerState(3, Samplerstatetype.Addressv, 2);
        int secondResult = device.SetSamplerState(3, Samplerstatetype.Addressv, 2);

        Assert.AreEqual((1, 0, 1, 0), (firstResult, secondResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenNativeSamplerStateSetFailsThenOriginalHResultIsPreservedAndStateIsRetried(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _nativeResult = failureHResult;

        int firstResult = device.SetSamplerState(4, Samplerstatetype.Minfilter, 3);
        int secondResult = device.SetSamplerState(4, Samplerstatetype.Minfilter, 3);

        Assert.AreEqual(
            (failureHResult, failureHResult, 2, 0),
            (firstResult, secondResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenSettingSamplerStateAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.SetSamplerState(5, Samplerstatetype.Mipfilter, 1);

        Assert.AreEqual((0, "5:SampMipfilter:1"), (result, FormatNativeCalls()));
    }

    [TestMethod]
    public void WhenSettingSamplerStateWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.SetSamplerState(6, Samplerstatetype.Addressu, 1));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    private static string FormatNativeCalls() =>
        string.Join(',', NativeCalls.Select(call => $"{call.Sampler}:{call.State}:{call.Value}"));

    private static Direct3D9Device CreateDevice(Direct3D9SetSamplerState setSamplerState)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: new Caps9 { MaxTextureBlendStages = 4 },
            setSamplerState: setSamplerState);
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
            capabilities: new Caps9 { MaxTextureBlendStages = 8 });
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetSamplerState(
        IDirect3DDevice9* self,
        uint sampler,
        Samplerstatetype state,
        uint value)
    {
        NativeCalls.Add((sampler, state, value));
        return _nativeResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 71);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[69] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, Samplerstatetype, uint, int>) &SetSamplerState;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
