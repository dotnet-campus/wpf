using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9PixelShaderBoolConstantStateTests
{
    private static readonly List<(uint Register, int Constant, uint RegisterCount)> NativeCalls = [];
    private static int _result;

    [TestInitialize]
    public void Initialize()
    {
        NativeCalls.Clear();
        _result = 0;
    }

    [TestMethod]
    public void WhenConstantMatchesThenNativeWriteIsSkipped()
    {
        Direct3D9PixelShaderBoolConstantState state = new();
        int callCount = 0;
        int SetConstant(uint _, int __)
        {
            callCount++;
            return 0;
        }

        int firstResult = state.SetConstant(3, 1, SetConstant);
        int secondResult = state.SetConstant(3, 1, SetConstant);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public void WhenConstantDiffersThenNewValueIsWritten()
    {
        Direct3D9PixelShaderBoolConstantState state = new();
        List<(uint Register, int Constant)> writes = [];
        int SetConstant(uint register, int constant)
        {
            writes.Add((register, constant));
            return 0;
        }

        state.SetConstant(2, 0, SetConstant);
        int result = state.SetConstant(2, 1, SetConstant);

        Assert.AreEqual((0, 2u, 1), (result, writes[1].Register, writes[1].Constant));
    }

    [TestMethod]
    public void WhenNativeWriteFailsThenRegisterBecomesUnknownAndNextCallRetries()
    {
        Direct3D9PixelShaderBoolConstantState state = new();
        int callCount = 0;
        int SetConstant(uint _, int __)
        {
            callCount++;
            return callCount == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
        }

        int firstResult = state.SetConstant(4, 0, SetConstant);
        int failedResult = state.SetConstant(4, 1, SetConstant);
        int retryResult = state.SetConstant(4, 1, SetConstant);

        Assert.AreEqual((0, Direct3D9Factory.GenericFailureHResult, 0, 3),
            (firstResult, failedResult, retryResult, callCount));
    }

    [TestMethod]
    public void WhenRegistersDifferThenEachRegisterIsCachedIndependently()
    {
        Direct3D9PixelShaderBoolConstantState state = new();
        int callCount = 0;
        int SetConstant(uint _, int __)
        {
            callCount++;
            return 0;
        }

        state.SetConstant(1, 1, SetConstant);
        state.SetConstant(2, 1, SetConstant);
        int result = state.SetConstant(1, 1, SetConstant);

        Assert.AreEqual((0, 2), (result, callCount));
    }

    [TestMethod]
    public void WhenForcedConstantReturnsNonzeroSuccessThenValueIsCachedForOnlyThatRegister()
    {
        Direct3D9PixelShaderBoolConstantState state = new();
        int callCount = 0;

        int forceResult = state.ForceSetConstant(3, -1, (_, _) =>
        {
            callCount++;
            return 1;
        });
        int matchingRegisterResult = state.SetConstant(3, -1, (_, _) =>
        {
            callCount++;
            return 0;
        });
        int differentRegisterResult = state.SetConstant(4, -1, (_, _) =>
        {
            callCount++;
            return 0;
        });

        Assert.AreEqual((1, 0, 0, 2),
            (forceResult, matchingRegisterResult, differentRegisterResult, callCount));
    }

    [TestMethod]
    public void WhenForcedConstantFailsThenRegisterBecomesUnknownAndMatchingCallRetries()
    {
        Direct3D9PixelShaderBoolConstantState state = new();
        state.ForceSetConstant(3, 1, (_, _) => 0);

        int failureResult = state.ForceSetConstant(
            3,
            1,
            (_, _) => Direct3D9Factory.GenericFailureHResult);
        int retryCount = 0;
        int retryResult = state.SetConstant(3, 1, (_, _) =>
        {
            retryCount++;
            return 0;
        });

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 1),
            (failureResult, retryResult, retryCount));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(-1)]
    public void WhenSettingConstantThenNativeSlot113ReceivesExactRegisterValueAndCount(int constant)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.SetPixelShaderBoolConstant(uint.MaxValue, constant);

        Assert.AreEqual((0, uint.MaxValue, constant, 1u),
            (result, NativeCalls[0].Register, NativeCalls[0].Constant, NativeCalls[0].RegisterCount));
    }

    [TestMethod]
    public void WhenMatchingConstantFollowsNonzeroSuccessThenNativeWriteIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = 1;

        int firstResult = device.SetPixelShaderBoolConstant(12, 1);
        int secondResult = device.SetPixelShaderBoolConstant(12, 1);

        Assert.AreEqual((1, 0, 1, 0),
            (firstResult, secondResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenSettingConstantFailsThenOriginalHResultIsPreservedAndRegisterIsRetried(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = failureHResult;

        int firstResult = device.SetPixelShaderBoolConstant(15, 1);
        int retryResult = device.SetPixelShaderBoolConstant(15, 1);

        Assert.AreEqual(
            (failureHResult, failureHResult, 2, 0),
            (firstResult, retryResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenSettingConstantAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.SetPixelShaderBoolConstant(0, 1);

        Assert.AreEqual((0, 1), (result, NativeCalls.Count));
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenSettingConstantThrowsBeforeNativeCall()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.SetPixelShaderBoolConstant(0, 1));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetPixelShaderConstantB(
        IDirect3DDevice9* self,
        uint register,
        int* constantData,
        uint registerCount)
    {
        NativeCalls.Add((register, *constantData, registerCount));
        return _result;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 115);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[113] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, int*, uint, int>) &SetPixelShaderConstantB;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
