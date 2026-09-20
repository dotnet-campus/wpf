using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9PixelShaderInt4ConstantStateTests
{
    private static readonly List<(uint Register, int[] Constant, uint RegisterCount)> NativeCalls = [];
    private static int _result;

    [TestInitialize]
    public void Initialize()
    {
        NativeCalls.Clear();
        _result = 0;
    }

    [TestMethod]
    public void WhenFirstElementMatchesThenNativeWriteIsSkipped()
    {
        Direct3D9PixelShaderInt4ConstantState state = new();
        int callCount = 0;
        int SetConstant(uint _, ReadOnlySpan<int> __)
        {
            callCount++;
            return 0;
        }

        int firstResult = state.SetConstant(3, [7, 7, 7, 7], SetConstant);
        int secondResult = state.SetConstant(3, [7, 8, 9, 10], SetConstant);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public void WhenFirstElementDiffersThenEntireInt4IsWritten()
    {
        Direct3D9PixelShaderInt4ConstantState state = new();
        List<(uint Register, int[] Constant)> writes = [];
        int SetConstant(uint register, ReadOnlySpan<int> constant)
        {
            writes.Add((register, constant.ToArray()));
            return 0;
        }

        state.SetConstant(2, [1, 1, 1, 1], SetConstant);
        int result = state.SetConstant(2, [5, 6, 7, 8], SetConstant);

        Assert.AreEqual((0, 2u), (result, writes[1].Register));
        CollectionAssert.AreEqual(new[] { 5, 6, 7, 8 }, writes[1].Constant);
    }

    [TestMethod]
    public void WhenNativeWriteFailsThenRegisterBecomesUnknownAndNextCallRetries()
    {
        Direct3D9PixelShaderInt4ConstantState state = new();
        int callCount = 0;
        int SetConstant(uint _, ReadOnlySpan<int> __)
        {
            callCount++;
            return callCount == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
        }

        int firstResult = state.SetConstant(4, [1, 1, 1, 1], SetConstant);
        int failedResult = state.SetConstant(4, [2, 2, 2, 2], SetConstant);
        int retryResult = state.SetConstant(4, [2, 2, 2, 2], SetConstant);

        Assert.AreEqual((0, Direct3D9Factory.GenericFailureHResult, 0, 3),
            (firstResult, failedResult, retryResult, callCount));
    }

    [TestMethod]
    public void WhenForcedConstantReturnsNonzeroSuccessThenFirstElementIsCachedForOnlyThatRegister()
    {
        Direct3D9PixelShaderInt4ConstantState state = new();
        int callCount = 0;

        int forceResult = state.ForceSetConstant(3, [7, 8, 9, 10], (_, _) =>
        {
            callCount++;
            return 1;
        });
        int matchingRegisterResult = state.SetConstant(3, [7, 0, 0, 0], (_, _) =>
        {
            callCount++;
            return 0;
        });
        int differentRegisterResult = state.SetConstant(4, [7, 8, 9, 10], (_, _) =>
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
        Direct3D9PixelShaderInt4ConstantState state = new();
        state.ForceSetConstant(3, [7, 8, 9, 10], (_, _) => 0);

        int failureResult = state.ForceSetConstant(
            3,
            [7, 0, 0, 0],
            (_, _) => Direct3D9Factory.GenericFailureHResult);
        int retryCount = 0;
        int retryResult = state.SetConstant(3, [7, -1, -2, -3], (_, _) =>
        {
            retryCount++;
            return 0;
        });

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 1),
            (failureResult, retryResult, retryCount));
    }

    [TestMethod]
    public void WhenConstantDoesNotContainFourElementsThenArgumentExceptionIsThrown()
    {
        Direct3D9PixelShaderInt4ConstantState state = new();

        Assert.ThrowsExactly<ArgumentException>(
            () => state.SetConstant(0, [1, 2, 3], (_, _) => 0));
    }

    [TestMethod]
    public void WhenSettingConstantThenNativeSlot111ReceivesExactRegisterInt4AndCount()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.SetPixelShaderInt4Constant(uint.MaxValue, [int.MinValue, -1, 0, int.MaxValue]);

        Assert.AreEqual(
            (0, uint.MaxValue, 1u, int.MinValue, -1, 0, int.MaxValue),
            (result, NativeCalls[0].Register, NativeCalls[0].RegisterCount,
                NativeCalls[0].Constant[0], NativeCalls[0].Constant[1],
                NativeCalls[0].Constant[2], NativeCalls[0].Constant[3]));
    }

    [TestMethod]
    public void WhenMatchingFirstElementFollowsNonzeroSuccessThenNativeWriteIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = 1;

        int firstResult = device.SetPixelShaderInt4Constant(12, [7, 7, 7, 7]);
        int secondResult = device.SetPixelShaderInt4Constant(12, [7, 8, 9, 10]);

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

        int firstResult = device.SetPixelShaderInt4Constant(15, [3, 3, 3, 3]);
        int retryResult = device.SetPixelShaderInt4Constant(15, [3, 3, 3, 3]);

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

        int result = device.SetPixelShaderInt4Constant(0, [1, 1, 1, 1]);

        Assert.AreEqual((0, 1), (result, NativeCalls.Count));
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenSettingConstantThrowsBeforeNativeCall()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.SetPixelShaderInt4Constant(0, [1, 1, 1, 1]));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetPixelShaderConstantI(
        IDirect3DDevice9* self,
        uint register,
        int* constantData,
        uint registerCount)
    {
        NativeCalls.Add((register, new[]
        {
            constantData[0],
            constantData[1],
            constantData[2],
            constantData[3]
        }, registerCount));
        return _result;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 113);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[111] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, int*, uint, int>) &SetPixelShaderConstantI;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
