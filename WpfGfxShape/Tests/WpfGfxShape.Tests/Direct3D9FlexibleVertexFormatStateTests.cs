using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9FlexibleVertexFormatStateTests
{
    private static readonly List<uint> FlexibleVertexFormats = [];
    private static int _result;

    [TestInitialize]
    public void Initialize()
    {
        FlexibleVertexFormats.Clear();
        _result = 0;
    }

    [TestMethod]
    public void WhenSettingFlexibleVertexFormatThenNativeSlot89ReceivesExactValue()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.SetFlexibleVertexFormat(0x1C4);

        Assert.AreEqual((0, "452"), (result, string.Join(',', FlexibleVertexFormats)));
    }

    [TestMethod]
    public void WhenInitialFlexibleVertexFormatIsZeroThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.SetFlexibleVertexFormat(0);

        Assert.AreEqual((0, "0"), (result, string.Join(',', FlexibleVertexFormats)));
    }

    [TestMethod]
    public void WhenFlexibleVertexFormatIsSetTwiceThenMatchingNativeCallIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int firstResult = device.SetFlexibleVertexFormat(0x144);
        int secondResult = device.SetFlexibleVertexFormat(0x144);

        Assert.AreEqual((0, 0, "324"), (firstResult, secondResult, string.Join(',', FlexibleVertexFormats)));
    }

    [TestMethod]
    public void WhenFlexibleVertexFormatIsForcedTwiceThenBothNativeCallsRun()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int firstResult = device.ForceSetFlexibleVertexFormat(0x144);
        int secondResult = device.ForceSetFlexibleVertexFormat(0x144);

        Assert.AreEqual((0, 0, "324,324"), (firstResult, secondResult, string.Join(',', FlexibleVertexFormats)));
    }

    [TestMethod]
    public void WhenFlexibleVertexFormatChangesThenNativeCallIsRepeated()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int firstResult = device.SetFlexibleVertexFormat(0x144);
        int secondResult = device.SetFlexibleVertexFormat(0x1C4);

        Assert.AreEqual((0, 0, "324,452"), (firstResult, secondResult, string.Join(',', FlexibleVertexFormats)));
    }

    [TestMethod]
    public void WhenSettingMatchingFlexibleVertexFormatAfterNonzeroSuccessThenNativeCallIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = 1;

        int firstResult = device.SetFlexibleVertexFormat(0x144);
        int secondResult = device.SetFlexibleVertexFormat(0x144);

        Assert.AreEqual((1, 0, 1, 0), (firstResult, secondResult, FlexibleVertexFormats.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenForcedFlexibleVertexFormatReturnsNonzeroSuccessThenMatchingNormalCallIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = 1;

        int forcedResult = device.ForceSetFlexibleVertexFormat(uint.MaxValue);
        int cachedResult = device.SetFlexibleVertexFormat(uint.MaxValue);

        Assert.AreEqual((1, 0, "4294967295"), (forcedResult, cachedResult, string.Join(',', FlexibleVertexFormats)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenForcedFlexibleVertexFormatFailsThenOriginalHResultIsPreservedAndNormalCallRetries(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = failureHResult;

        int forcedResult = device.ForceSetFlexibleVertexFormat(0x144);
        _result = 0;
        int retryResult = device.SetFlexibleVertexFormat(0x144);

        Assert.AreEqual(
            (failureHResult, 0, "324,324", 0),
            (forcedResult, retryResult, string.Join(',', FlexibleVertexFormats), device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenFlexibleVertexFormatSetFailsThenOriginalHResultIsPreservedAndMatchingValueIsRetried(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = failureHResult;

        int failedResult = device.SetFlexibleVertexFormat(0x144);
        int retryResult = device.SetFlexibleVertexFormat(0x144);

        Assert.AreEqual(
            (failureHResult, failureHResult, "324,324", 0),
            (failedResult, retryResult, string.Join(',', FlexibleVertexFormats), device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenFlexibleVertexFormatChangeFailsThenPreviouslyKnownValueIsRetried()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int initialResult = device.SetFlexibleVertexFormat(0x144);
        _result = Direct3D9Factory.GenericFailureHResult;
        int failedResult = device.SetFlexibleVertexFormat(0x1C4);
        _result = 0;
        int restoreResult = device.SetFlexibleVertexFormat(0x144);

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, 0, "324,452,324"),
            (initialResult, failedResult, restoreResult, string.Join(',', FlexibleVertexFormats)));
    }

    [TestMethod]
    public void WhenSettingFlexibleVertexFormatAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.SetFlexibleVertexFormat(0x144);

        Assert.AreEqual((0, 1), (result, FlexibleVertexFormats.Count));
    }

    [TestMethod]
    public void WhenFlexibleVertexFormatIsInitiallyUnknownThenQueryReturnsFalseWithoutNativeCall()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        bool isSet = device.IsFlexibleVertexFormatSet(0);

        Assert.AreEqual((false, 0), (isSet, FlexibleVertexFormats.Count));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public void WhenFlexibleVertexFormatSetSucceedsThenQueryMatchesExactValueWithoutNativeCall(int successHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = successHResult;
        device.SetFlexibleVertexFormat(0x144);
        int nativeCallCount = FlexibleVertexFormats.Count;

        bool matchingIsSet = device.IsFlexibleVertexFormatSet(0x144);
        bool differentIsSet = device.IsFlexibleVertexFormatSet(0x1C4);

        Assert.AreEqual((true, false, nativeCallCount), (matchingIsSet, differentIsSet, FlexibleVertexFormats.Count));
    }

    [TestMethod]
    public void WhenFlexibleVertexFormatSetFailsThenQueryReturnsFalseWithoutChangingRetryState()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = Direct3D9Factory.GenericFailureHResult;
        device.SetFlexibleVertexFormat(0x144);

        bool isSet = device.IsFlexibleVertexFormatSet(0x144);
        _result = 0;
        int retryResult = device.SetFlexibleVertexFormat(0x144);

        Assert.AreEqual((false, 0, "324,324"), (isSet, retryResult, string.Join(',', FlexibleVertexFormats)));
    }

    [TestMethod]
    public void WhenFlexibleVertexFormatIsForcedThenQueryMatchesForcedValueWithoutNativeCall()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.ForceSetFlexibleVertexFormat(uint.MaxValue);
        int nativeCallCount = FlexibleVertexFormats.Count;

        bool isSet = device.IsFlexibleVertexFormatSet(uint.MaxValue);

        Assert.AreEqual((true, nativeCallCount), (isSet, FlexibleVertexFormats.Count));
    }

    [TestMethod]
    public void WhenQueryingFlexibleVertexFormatThenQueryDoesNotChangeCachedState()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.SetFlexibleVertexFormat(0x144);

        device.IsFlexibleVertexFormatSet(0x1C4);
        int result = device.SetFlexibleVertexFormat(0x144);

        Assert.AreEqual((0, "324"), (result, string.Join(',', FlexibleVertexFormats)));
    }

    [TestMethod]
    public void WhenQueryingFlexibleVertexFormatWithReleasedDeviceThenQueryIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.IsFlexibleVertexFormatSet(0x144));
        Assert.AreEqual(0, FlexibleVertexFormats.Count);
    }

    [TestMethod]
    public void WhenSettingFlexibleVertexFormatWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetFlexibleVertexFormat(0x144));
        Assert.AreEqual(0, FlexibleVertexFormats.Count);
    }

    [TestMethod]
    public void WhenForcingFlexibleVertexFormatWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.ForceSetFlexibleVertexFormat(0x144));
        Assert.AreEqual(0, FlexibleVertexFormats.Count);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetFlexibleVertexFormat(IDirect3DDevice9* self, uint flexibleVertexFormat)
    {
        FlexibleVertexFormats.Add(flexibleVertexFormat);
        return _result;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 91);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[89] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, int>) &SetFlexibleVertexFormat;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
