using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceTextureStageStateTests
{
    private static readonly List<(uint Stage, Texturestagestatetype State, uint Value)> NativeCalls = [];
    private static int _nativeResult;

    [TestInitialize]
    public void Initialize()
    {
        NativeCalls.Clear();
        _nativeResult = 0;
    }

    [TestMethod]
    public void WhenTextureTransformIsDisabledThenNativeSlot67ReceivesFixedStateAndValue()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.DisableTextureTransform(7);

        Assert.AreEqual(
            (0, 1, (7u, Texturestagestatetype.Texturetransformflags, 0u)),
            (result, NativeCalls.Count, NativeCalls[0]));
    }

    [TestMethod]
    public unsafe void WhenTextureTransformIsDisabledTwiceThenMatchingNativeCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 1;
        });

        int firstResult = device.DisableTextureTransform(2);
        int secondResult = device.DisableTextureTransform(2);

        Assert.AreEqual((1, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult, 0)]
    [DataRow(Direct3D9Factory.DeviceLostHResult, Direct3D9Factory.DeviceLostHResult, 0)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult, Direct3D9Factory.DisplayStateInvalidHResult,
        Direct3D9Factory.DriverInternalErrorHResult)]
    public unsafe void WhenDisablingTextureTransformFailsThenExpectedHResultIsReturnedAndStateIsRetried(
        int failureHResult,
        int expectedHResult,
        int expectedUnusableReason)
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return failureHResult;
        });

        int firstResult = device.DisableTextureTransform(3);
        int secondResult = device.DisableTextureTransform(3);

        Assert.AreEqual(
            (expectedHResult, expectedHResult, 2, expectedUnusableReason),
            (firstResult, secondResult, callCount, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenDisablingTextureTransformAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.DisableTextureTransform(5);

        Assert.AreEqual(
            (0, 1, (5u, Texturestagestatetype.Texturetransformflags, 0u)),
            (result, NativeCalls.Count, NativeCalls[0]));
    }

    [TestMethod]
    public void WhenDisablingTextureTransformWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.DisableTextureTransform(0));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    [TestMethod]
    public unsafe void WhenTextureStageStateIsInitiallyUnknownThenFirstZeroValueIsSubmitted()
    {
        List<(uint Stage, Texturestagestatetype State, uint Value)> calls = [];
        using Direct3D9Device device = CreateDevice((stage, state, value) =>
        {
            calls.Add((stage, state, value));
            return 0;
        });

        int result = device.SetTextureStageState(0, Texturestagestatetype.Texturetransformflags, 0);

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(
            new[] { (0u, Texturestagestatetype.Texturetransformflags, 0u) },
            calls);
    }

    [TestMethod]
    public unsafe void WhenTextureStageStateIsSetTwiceThenMatchingNativeCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 0;
        });

        int firstResult = device.SetTextureStageState(0, Texturestagestatetype.Colorop, 2);
        int secondResult = device.SetTextureStageState(0, Texturestagestatetype.Colorop, 2);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenTextureStageOrStateChangesThenNativeCallIsRepeated()
    {
        List<(uint Stage, Texturestagestatetype State)> calls = [];
        using Direct3D9Device device = CreateDevice((stage, state, _) =>
        {
            calls.Add((stage, state));
            return 0;
        });

        _ = device.SetTextureStageState(0, Texturestagestatetype.Colorop, 2);
        _ = device.SetTextureStageState(1, Texturestagestatetype.Colorop, 2);
        _ = device.SetTextureStageState(0, Texturestagestatetype.Alphaop, 2);

        CollectionAssert.AreEqual(
            new[]
            {
                (0u, Texturestagestatetype.Colorop),
                (1u, Texturestagestatetype.Colorop),
                (0u, Texturestagestatetype.Alphaop)
            },
            calls);
    }

    [TestMethod]
    public unsafe void WhenTextureStageStateSetFailsThenMatchingStateIsRetried()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return callCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
        });

        int failedResult = device.SetTextureStageState(0, Texturestagestatetype.Colorarg1, 2);
        int retryResult = device.SetTextureStageState(0, Texturestagestatetype.Colorarg1, 2);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 2), (failedResult, retryResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenMaximumTextureStageIsUsedThenNativeCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 0;
        });

        int result = device.SetTextureStageState(4, Texturestagestatetype.Colorop, 2);

        Assert.AreEqual((0, 0), (result, callCount));
    }

    [TestMethod]
    public unsafe void WhenTextureStageIsBeyondMaximumThenSettingStateThrowsArgumentOutOfRangeException()
    {
        using Direct3D9Device device = CreateDevice((_, _, _) => 0);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            device.SetTextureStageState(5, Texturestagestatetype.Colorop, 2));
    }

    [TestMethod]
    public unsafe void WhenUnsupportedTextureStageStateIsUsedThenSettingStateThrowsArgumentOutOfRangeException()
    {
        using Direct3D9Device device = CreateDevice((_, _, _) => 0);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            device.SetTextureStageState(0, Texturestagestatetype.Resultarg, 2));
    }

    [TestMethod]
    public unsafe void WhenTextureStageStateIsForcedThenMatchingRegularCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 0;
        });

        int forcedResult = device.ForceSetTextureStageState(0, Texturestagestatetype.Texturetransformflags, 0);
        int cachedResult = device.SetTextureStageState(0, Texturestagestatetype.Texturetransformflags, 0);

        Assert.AreEqual((0, 0, 1), (forcedResult, cachedResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenTextureStageStateIsForcedTwiceThenBothNativeCallsAreSubmitted()
    {
        List<(uint Stage, Texturestagestatetype State, uint Value)> calls = [];
        using Direct3D9Device device = CreateDevice((stage, state, value) =>
        {
            calls.Add((stage, state, value));
            return 1;
        });

        int firstResult = device.ForceSetTextureStageState(3, Texturestagestatetype.Alphaarg2, uint.MaxValue);
        int secondResult = device.ForceSetTextureStageState(3, Texturestagestatetype.Alphaarg2, uint.MaxValue);

        Assert.AreEqual((1, 1), (firstResult, secondResult));
        CollectionAssert.AreEqual(
            new[]
            {
                (3u, Texturestagestatetype.Alphaarg2, uint.MaxValue),
                (3u, Texturestagestatetype.Alphaarg2, uint.MaxValue)
            },
            calls);
    }

    [TestMethod]
    public unsafe void WhenForcedTextureStageStateFailsAfterKnownValueThenMatchingRegularCallIsRetried()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return callCount == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
        });

        int initialResult = device.SetTextureStageState(0, Texturestagestatetype.Texturetransformflags, 0);
        int forcedResult = device.ForceSetTextureStageState(0, Texturestagestatetype.Texturetransformflags, 0);
        int retryResult = device.SetTextureStageState(0, Texturestagestatetype.Texturetransformflags, 0);

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, 0, 3),
            (initialResult, forcedResult, retryResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenTextureCoordinateIndicesAreDefaultThenRepeatedResetIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 0;
        });

        int firstResult = device.SetDefaultTextureCoordinateIndices();
        int secondResult = device.SetDefaultTextureCoordinateIndices();

        Assert.AreEqual((0, 0, 4), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenDefaultTextureCoordinateIndicesReturnNonzeroSuccessThenResultIsPreservedAndResetIsCached()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 1;
        });

        int firstResult = device.SetDefaultTextureCoordinateIndices();
        int secondResult = device.SetDefaultTextureCoordinateIndices();

        Assert.AreEqual((1, 0, 4), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenTextureCoordinateIndexBecomesNonDefaultThenDefaultIndicesAreRestored()
    {
        List<(uint Stage, uint Value)> calls = [];
        using Direct3D9Device device = CreateDevice((stage, state, value) =>
        {
            if (state == Texturestagestatetype.Texcoordindex)
            {
                calls.Add((stage, value));
            }

            return 0;
        });

        _ = device.SetDefaultTextureCoordinateIndices();
        _ = device.ForceSetTextureStageState(2, Texturestagestatetype.Texcoordindex, 1);
        int result = device.SetDefaultTextureCoordinateIndices();

        Assert.AreEqual((0, (2u, 2u)), (result, calls[^1]));
    }

    [TestMethod]
    public unsafe void WhenNonTextureCoordinateStateDiffersFromStageThenDefaultIndicesRemainCached()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 0;
        });

        _ = device.SetDefaultTextureCoordinateIndices();
        _ = device.ForceSetTextureStageState(2, Texturestagestatetype.Alphaarg1, 1);
        int result = device.SetDefaultTextureCoordinateIndices();

        Assert.AreEqual((0, 5), (result, callCount));
    }

    [TestMethod]
    public unsafe void WhenForcedDefaultTextureCoordinateIndexFailsThenDefaultIndicesRemainCached()
    {
        int callCount = 0;
        bool failDefaultIndex = false;
        using Direct3D9Device device = CreateDevice((stage, state, value) =>
        {
            callCount++;
            return failDefaultIndex && stage == 2 && state == Texturestagestatetype.Texcoordindex && value == 2
                ? Direct3D9Factory.GenericFailureHResult
                : 0;
        });

        _ = device.SetDefaultTextureCoordinateIndices();
        failDefaultIndex = true;
        int failedResult = device.ForceSetTextureStageState(2, Texturestagestatetype.Texcoordindex, 2);
        int cachedResult = device.SetDefaultTextureCoordinateIndices();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 5),
            (failedResult, cachedResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenNonDefaultTextureCoordinateIndexSetFailsThenDefaultIndicesAreStillRestored()
    {
        List<(uint Stage, uint Value)> calls = [];
        using Direct3D9Device device = CreateDevice((stage, state, value) =>
        {
            if (state == Texturestagestatetype.Texcoordindex)
            {
                calls.Add((stage, value));
            }

            return stage == 2 && value == 1
                ? Direct3D9Factory.GenericFailureHResult
                : 0;
        });

        _ = device.SetDefaultTextureCoordinateIndices();
        calls.Clear();
        int failedResult = device.ForceSetTextureStageState(2, Texturestagestatetype.Texcoordindex, 1);
        int restoreResult = device.SetDefaultTextureCoordinateIndices();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, (2u, 2u)),
            (failedResult, restoreResult, calls[^1]));
    }

    [TestMethod]
    public unsafe void WhenDefaultTextureCoordinateRestoreFailsThenLaterStagesAreNotSubmitted()
    {
        List<uint> stages = [];
        using Direct3D9Device device = CreateDevice((stage, state, _) =>
        {
            if (state == Texturestagestatetype.Texcoordindex)
            {
                stages.Add(stage);
            }

            return stage == 1
                ? Direct3D9Factory.DeviceLostHResult
                : 0;
        });

        int result = device.SetDefaultTextureCoordinateIndices();

        Assert.AreEqual(Direct3D9Factory.DeviceLostHResult, result);
        CollectionAssert.AreEqual(new uint[] { 0, 1 }, stages);
    }

    [TestMethod]
    public unsafe void WhenDefaultTextureCoordinateRestoreFailsThenRetrySkipsAlreadyRestoredStages()
    {
        List<(uint Stage, uint Value)> calls = [];
        int stageTwoDefaultCallCount = 0;
        using Direct3D9Device device = CreateDevice((stage, state, value) =>
        {
            if (state != Texturestagestatetype.Texcoordindex)
            {
                return 0;
            }

            calls.Add((stage, value));
            if (stage == 2 && value == 2 && ++stageTwoDefaultCallCount == 1)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            return 0;
        });

        _ = device.ForceSetTextureStageState(2, Texturestagestatetype.Texcoordindex, 1);
        int failedResult = device.SetDefaultTextureCoordinateIndices();
        int retryResult = device.SetDefaultTextureCoordinateIndices();
        int cachedResult = device.SetDefaultTextureCoordinateIndices();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0),
            (failedResult, retryResult, cachedResult));
        CollectionAssert.AreEqual(
            new[]
            {
                (2u, 1u),
                (0u, 0u),
                (1u, 1u),
                (2u, 2u),
                (2u, 2u),
                (3u, 3u)
            },
            calls);
    }

    [TestMethod]
    public unsafe void WhenDeviceIsDisposedThenSettingDefaultTextureCoordinateIndicesThrowsObjectDisposedException()
    {
        int callCount = 0;
        Direct3D9Device device = CreateDevice((_, _, _) =>
        {
            callCount++;
            return 0;
        });
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetDefaultTextureCoordinateIndices());
        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public unsafe void WhenDeviceIsDisposedThenSettingTextureStageStateThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateDevice((_, _, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.SetTextureStageState(0, Texturestagestatetype.Colorop, 2));
    }

    [TestMethod]
    public void WhenSettingTextureStageStateThenNativeSlot67ReceivesExactParameters()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.SetTextureStageState(7, Texturestagestatetype.Colorarg2, uint.MaxValue);

        Assert.AreEqual(
            (0, 1, (7u, Texturestagestatetype.Colorarg2, uint.MaxValue)),
            (result, NativeCalls.Count, NativeCalls[0]));
    }

    [TestMethod]
    public void WhenTextureStageStateReturnsNonzeroSuccessThenStateIsCached()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _nativeResult = 1;

        int firstResult = device.SetTextureStageState(3, Texturestagestatetype.Alphaarg1, 0xFEDCBA98);
        int secondResult = device.SetTextureStageState(3, Texturestagestatetype.Alphaarg1, 0xFEDCBA98);

        Assert.AreEqual((1, 0, 1, 0), (firstResult, secondResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenNativeTextureStageStateSetFailsThenOriginalHResultIsPreservedAndStateIsRetried(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _nativeResult = failureHResult;

        int firstResult = device.SetTextureStageState(4, Texturestagestatetype.Texturetransformflags, uint.MaxValue);
        int secondResult = device.SetTextureStageState(4, Texturestagestatetype.Texturetransformflags, uint.MaxValue);

        Assert.AreEqual(
            (failureHResult, failureHResult, 2, 0),
            (firstResult, secondResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenSettingTextureStageStateAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.SetTextureStageState(5, Texturestagestatetype.Texcoordindex, 4);

        Assert.AreEqual(
            (0, 1, (5u, Texturestagestatetype.Texcoordindex, 4u)),
            (result, NativeCalls.Count, NativeCalls[0]));
    }

    [TestMethod]
    public void WhenSettingTextureStageStateWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.SetTextureStageState(6, Texturestagestatetype.Alphaop, 3));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    [TestMethod]
    public void WhenForcingTextureStageStateWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.ForceSetTextureStageState(6, Texturestagestatetype.Alphaop, 3));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    private static unsafe Direct3D9Device CreateDevice(Direct3D9SetTextureStageState setTextureStageState)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: new Caps9 { MaxTextureBlendStages = 4 },
            setTextureStageState: setTextureStageState);
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
    private static int SetTextureStageState(
        IDirect3DDevice9* self,
        uint stage,
        Texturestagestatetype state,
        uint value)
    {
        NativeCalls.Add((stage, state, value));
        return _nativeResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 69);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[67] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, Texturestagestatetype, uint, int>) &SetTextureStageState;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
