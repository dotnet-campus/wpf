using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9PixelShaderConstantStateTests
{
    private static readonly List<(uint StartRegister, uint RegisterCount, float[] Values)> NativeCalls = [];
    private static int _result;

    [TestInitialize]
    public void Initialize()
    {
        NativeCalls.Clear();
        _result = 0;
    }

    [TestMethod]
    public void WhenAllRegistersMatchThenNativeWriteIsSkipped()
    {
        Direct3D9PixelShaderConstantState state = new();
        Vector4[] constants = [new(1, 2, 3, 4), new(5, 6, 7, 8)];
        int callCount = 0;
        int SetConstants(uint _, ReadOnlySpan<Vector4> __)
        {
            callCount++;
            return 0;
        }

        int firstResult = state.SetConstants(3, constants, SetConstants);
        int secondResult = state.SetConstants(3, constants, SetConstants);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public void WhenAnyRegisterDiffersThenEntireRangeIsWritten()
    {
        Direct3D9PixelShaderConstantState state = new();
        Vector4[] first = [new(1, 2, 3, 4), new(5, 6, 7, 8)];
        Vector4[] second = [first[0], new(9, 10, 11, 12)];
        List<(uint StartRegister, Vector4[] Constants)> writes = [];
        int SetConstants(uint startRegister, ReadOnlySpan<Vector4> constants)
        {
            writes.Add((startRegister, constants.ToArray()));
            return 0;
        }

        state.SetConstants(3, first, SetConstants);
        int result = state.SetConstants(3, second, SetConstants);

        Assert.AreEqual((0, 3u, 2), (result, writes[1].StartRegister, writes[1].Constants.Length));
        CollectionAssert.AreEqual(second, writes[1].Constants);
    }

    [TestMethod]
    public void WhenNativeWriteFailsThenEveryRegisterBecomesUnknownAndNextCallRetries()
    {
        Direct3D9PixelShaderConstantState state = new();
        Vector4[] constants = [new(1, 2, 3, 4), new(5, 6, 7, 8)];
        int callCount = 0;
        int SetConstants(uint _, ReadOnlySpan<Vector4> __)
        {
            callCount++;
            return callCount == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
        }

        int firstResult = state.SetConstants(4, constants, SetConstants);
        constants[1] = new Vector4(9, 10, 11, 12);
        int failedResult = state.SetConstants(4, constants, SetConstants);
        int retryResult = state.SetConstants(4, constants, SetConstants);

        Assert.AreEqual((0, Direct3D9Factory.GenericFailureHResult, 0, 3),
            (firstResult, failedResult, retryResult, callCount));
    }

    [TestMethod]
    public void WhenForcedRangeReturnsNonzeroSuccessThenEveryRegisterIsCached()
    {
        Direct3D9PixelShaderConstantState state = new();
        Vector4[] constants = [new(1, 2, 3, 4), new(5, 6, 7, 8)];
        int callCount = 0;

        int forceResult = state.ForceSetConstants(4, constants, (_, _) =>
        {
            callCount++;
            return 1;
        });
        int firstRegisterResult = state.SetConstants(4, constants.AsSpan(0, 1), (_, _) =>
        {
            callCount++;
            return 0;
        });
        int secondRegisterResult = state.SetConstants(5, constants.AsSpan(1, 1), (_, _) =>
        {
            callCount++;
            return 0;
        });

        Assert.AreEqual((1, 0, 0, 1), (forceResult, firstRegisterResult, secondRegisterResult, callCount));
    }

    [TestMethod]
    public void WhenForcedRangeFailsThenEveryRegisterBecomesUnknown()
    {
        Direct3D9PixelShaderConstantState state = new();
        Vector4[] constants = [new(1, 2, 3, 4), new(5, 6, 7, 8)];
        state.ForceSetConstants(4, constants, (_, _) => 0);

        int failureResult = state.ForceSetConstants(
            4,
            constants,
            (_, _) => Direct3D9Factory.GenericFailureHResult);
        int retryCount = 0;
        state.SetConstants(4, constants.AsSpan(0, 1), (_, _) =>
        {
            retryCount++;
            return 0;
        });
        state.SetConstants(5, constants.AsSpan(1, 1), (_, _) =>
        {
            retryCount++;
            return 0;
        });

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 2), (failureResult, retryCount));
    }

    [TestMethod]
    public void WhenSettingConstantsThenNativeSlot109ReceivesExactRegisterRangeAndFloatLayout()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Vector4[] constants = [new(1.25f, -2.5f, 3.75f, -4.125f), new(5.5f, 6.625f, -7.75f, 8.875f)];

        int result = device.SetPixelShaderConstants(uint.MaxValue, constants);

        Assert.AreEqual((0, uint.MaxValue, 2u), (result, NativeCalls[0].StartRegister, NativeCalls[0].RegisterCount));
        CollectionAssert.AreEqual(new[] { 1.25f, -2.5f, 3.75f, -4.125f, 5.5f, 6.625f, -7.75f, 8.875f }, NativeCalls[0].Values);
    }

    [TestMethod]
    public void WhenMatchingConstantsFollowNonzeroSuccessThenNativeWriteIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Vector4[] constants = [new(1, 2, 3, 4)];
        _result = 1;

        int firstResult = device.SetPixelShaderConstants(7, constants);
        int secondResult = device.SetPixelShaderConstants(7, constants);

        Assert.AreEqual((1, 0, 1, 0), (firstResult, secondResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenSettingConstantsFailsThenOriginalHResultIsPreservedAndRangeIsRetried(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Vector4[] constants = [new(1, 2, 3, 4), new(5, 6, 7, 8)];
        _result = failureHResult;

        int firstResult = device.SetPixelShaderConstants(11, constants);
        int retryResult = device.SetPixelShaderConstants(11, constants);

        Assert.AreEqual(
            (failureHResult, failureHResult, 2, 0),
            (firstResult, retryResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenSettingConstantsAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.SetPixelShaderConstants(0, [Vector4.One]);

        Assert.AreEqual((0, 1), (result, NativeCalls.Count));
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenSettingConstantsThrowsObjectDisposedException()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.SetPixelShaderConstants(0, [Vector4.One]));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetPixelShaderConstantF(
        IDirect3DDevice9* self,
        uint startRegister,
        float* constantData,
        uint registerCount)
    {
        int valueCount = checked((int) registerCount * 4);
        float[] values = new float[valueCount];
        new ReadOnlySpan<float>(constantData, valueCount).CopyTo(values);
        NativeCalls.Add((startRegister, registerCount, values));
        return _result;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 114);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[109] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, float*, uint, int>) &SetPixelShaderConstantF;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
