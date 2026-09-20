using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9StreamSourceStateTests
{
    private static readonly List<(uint StreamNumber, nint StreamData, uint OffsetInBytes, uint Stride)> NativeCalls = [];
    private static int _result;

    [TestInitialize]
    public void Initialize()
    {
        NativeCalls.Clear();
        _result = 0;
    }

    [TestMethod]
    public void WhenInitialStreamZeroIsNullWithZeroStrideThenNativeCallIsMade()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _, _) =>
        {
            callCount++;
            return 0;
        });

        int result = device.SetStreamSource(0, null, 0, 0);

        Assert.AreEqual((0, 1), (result, callCount));
    }

    [TestMethod]
    public void WhenStreamSourceIsSetTwiceThenMatchingNativeCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _, _) =>
        {
            callCount++;
            return 0;
        });
        IDirect3DVertexBuffer9* vertexBuffer = (IDirect3DVertexBuffer9*) 1;

        int firstResult = device.SetStreamSource(0, vertexBuffer, 0, 24);
        int secondResult = device.SetStreamSource(0, vertexBuffer, 0, 24);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public void WhenStreamSourceBufferChangesThenNativeCallIsRepeated()
    {
        List<nint> buffers = [];
        using Direct3D9Device device = CreateDevice((_, streamData, _, _) =>
        {
            buffers.Add((nint) streamData);
            return 0;
        });

        int firstResult = device.SetStreamSource(0, (IDirect3DVertexBuffer9*) 1, 0, 24);
        int secondResult = device.SetStreamSource(0, (IDirect3DVertexBuffer9*) 2, 0, 24);

        Assert.AreEqual((0, 0, "1,2"), (firstResult, secondResult, string.Join(',', buffers)));
    }

    [TestMethod]
    public void WhenStreamSourceStrideChangesThenNativeCallIsRepeated()
    {
        List<uint> strides = [];
        using Direct3D9Device device = CreateDevice((_, _, _, stride) =>
        {
            strides.Add(stride);
            return 0;
        });
        IDirect3DVertexBuffer9* vertexBuffer = (IDirect3DVertexBuffer9*) 1;

        int firstResult = device.SetStreamSource(0, vertexBuffer, 0, 24);
        int secondResult = device.SetStreamSource(0, vertexBuffer, 0, 32);

        Assert.AreEqual((0, 0, "24,32"), (firstResult, secondResult, string.Join(',', strides)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenStreamSourceSetFailsThenOriginalHResultIsPreservedAndMatchingStateIsRetried(int failureHResult)
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _, _) =>
        {
            callCount++;
            return failureHResult;
        });
        IDirect3DVertexBuffer9* vertexBuffer = (IDirect3DVertexBuffer9*) 1;

        int firstResult = device.SetStreamSource(0, vertexBuffer, 0, 24);
        int retryResult = device.SetStreamSource(0, vertexBuffer, 0, 24);

        Assert.AreEqual(
            (failureHResult, failureHResult, 2, 0),
            (firstResult, retryResult, callCount, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenStreamSourceSetFailsThenBufferAndStrideBothBecomeUnknown()
    {
        List<(nint Buffer, uint Stride)> calls = [];
        using Direct3D9Device device = CreateDevice((_, streamData, _, stride) =>
        {
            calls.Add(((nint) streamData, stride));
            return calls.Count == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
        });
        IDirect3DVertexBuffer9* vertexBuffer = (IDirect3DVertexBuffer9*) 1;

        int initialResult = device.SetStreamSource(0, vertexBuffer, 0, 24);
        int failedResult = device.SetStreamSource(0, vertexBuffer, 0, 32);
        int retryResult = device.SetStreamSource(0, vertexBuffer, 0, 24);

        Assert.AreEqual(
            $"0,{Direct3D9Factory.GenericFailureHResult},0|1:24,1:32,1:24",
            $"{initialResult},{failedResult},{retryResult}|{string.Join(',', calls.Select(call => $"{call.Buffer}:{call.Stride}"))}");
    }

    [TestMethod]
    public void WhenNonzeroStreamIsSetTwiceThenBothNativeCallsAreMade()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _, _) =>
        {
            callCount++;
            return 0;
        });

        int firstResult = device.SetStreamSource(1, null, 0, 0);
        int secondResult = device.SetStreamSource(1, null, 0, 0);

        Assert.AreEqual((0, 0, 2), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public void WhenSettingStreamZeroWithNonzeroOffsetThenCachedStateBecomesUnknown()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _, _) =>
        {
            callCount++;
            return 0;
        });
        IDirect3DVertexBuffer9* vertexBuffer = (IDirect3DVertexBuffer9*) 1;

        device.SetStreamSource(0, vertexBuffer, 0, 24);
        device.SetStreamSource(0, vertexBuffer, 4, 24);
        int result = device.SetStreamSource(0, vertexBuffer, 0, 24);

        Assert.AreEqual((0, 3), (result, callCount));
    }

    [TestMethod]
    public void WhenSettingStreamSourceThenNativeSlot100ReceivesExactParameters()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DVertexBuffer9* vertexBuffer = (IDirect3DVertexBuffer9*) 0x1234;

        int result = device.SetStreamSource(uint.MaxValue, vertexBuffer, 0x3456, 0x789A);

        Assert.AreEqual(
            (0, uint.MaxValue, (nint) vertexBuffer, 0x3456u, 0x789Au),
            (result, NativeCalls[0].StreamNumber, NativeCalls[0].StreamData, NativeCalls[0].OffsetInBytes, NativeCalls[0].Stride));
    }

    [TestMethod]
    public void WhenSettingMatchingStreamSourceAfterNonzeroSuccessThenNativeCallIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DVertexBuffer9* vertexBuffer = (IDirect3DVertexBuffer9*) 0x1234;
        _result = 1;

        int firstResult = device.SetStreamSource(0, vertexBuffer, 0, 24);
        int secondResult = device.SetStreamSource(0, vertexBuffer, 0, 24);

        Assert.AreEqual((1, 0, 1, 0), (firstResult, secondResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenForceSettingMatchingNullStreamSourceWithNonzeroStrideThenNativeCallIsRepeatedAndStateIsCached()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, streamData, offsetInBytes, stride) =>
        {
            Assert.AreEqual((nint.Zero, 0u, 24u), ((nint) streamData, offsetInBytes, stride));
            callCount++;
            return 1;
        });

        int firstResult = device.ForceSetStreamSource(null, 24);
        int secondResult = device.ForceSetStreamSource(null, 24);
        int cachedResult = device.SetStreamSource(0, null, 0, 24);

        Assert.AreEqual((1, 1, 0, 2), (firstResult, secondResult, cachedResult, callCount));
    }

    [TestMethod]
    public void WhenForceSettingStreamSourceFailsThenBufferAndStrideBothBecomeUnknown()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _, _, _) =>
        {
            callCount++;
            return callCount == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
        });
        IDirect3DVertexBuffer9* vertexBuffer = (IDirect3DVertexBuffer9*) 1;

        int initialResult = device.SetStreamSource(0, vertexBuffer, 0, 24);
        int failedResult = device.ForceSetStreamSource(vertexBuffer, 24);
        int retryResult = device.SetStreamSource(0, vertexBuffer, 0, 24);

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, 0, 3),
            (initialResult, failedResult, retryResult, callCount));
    }

    [TestMethod]
    public void WhenSettingStreamSourceAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.SetStreamSource(0, (IDirect3DVertexBuffer9*) 0x1234, 0, 24);

        Assert.AreEqual((0, 1), (result, NativeCalls.Count));
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenSettingStreamSourceThrowsObjectDisposedException()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetStreamSource(0, null, 0, 0));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    private static Direct3D9Device CreateDevice(Direct3D9SetStreamSource setStreamSource)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            setStreamSource: setStreamSource);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetStreamSource(
        IDirect3DDevice9* self,
        uint streamNumber,
        IDirect3DVertexBuffer9* streamData,
        uint offsetInBytes,
        uint stride)
    {
        NativeCalls.Add((streamNumber, (nint) streamData, offsetInBytes, stride));
        return _result;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 112);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[100] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DVertexBuffer9*, uint, uint, int>) &SetStreamSource;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
