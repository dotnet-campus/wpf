using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceScissorStateTests
{
    private static readonly List<string> NativeCalls = [];
    private static int _scissorResult;
    private static int _renderStateResult;

    [TestInitialize]
    public void Initialize()
    {
        NativeCalls.Clear();
        _scissorResult = 0;
        _renderStateResult = 0;
    }

    [TestMethod]
    public void WhenSettingScissorThenNativeSlot75ReceivesExactRectangleBeforeEnable()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.SetScissorRect(new Direct3D9PointAndSizeRect(-2, 3, 5, 7));

        Assert.AreEqual((0, "Rect:-2:3:3:10,State:174:1"), (result, string.Join(',', NativeCalls)));
    }

    [TestMethod]
    public void WhenNativeCallsReturnNonzeroSuccessThenFinalRenderStateResultIsPreservedAndRectangleIsCached()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _scissorResult = 1;
        _renderStateResult = 2;
        Direct3D9PointAndSizeRect rectangle = new(2, 3, 5, 7);

        int firstResult = device.SetScissorRect(rectangle);
        int secondResult = device.SetScissorRect(rectangle);

        Assert.AreEqual((2, 0, "Rect:2:3:7:10,State:174:1"), (firstResult, secondResult, string.Join(',', NativeCalls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenNativeScissorSetFailsThenOriginalHResultIsPreservedAndDisableIsAttempted(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _scissorResult = failureHResult;
        _renderStateResult = unchecked((int) 0x80070005);

        int result = device.SetScissorRect(new Direct3D9PointAndSizeRect(2, 3, 5, 7));

        Assert.AreEqual(
            (failureHResult, 0, "Rect:2:3:7:10,State:174:0"),
            (result, device.UnusableReasonHResult, string.Join(',', NativeCalls)));
    }

    [TestMethod]
    public void WhenNativeScissorSetFailsThenSameRectangleIsRetried()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9PointAndSizeRect rectangle = new(2, 3, 5, 7);
        _scissorResult = Direct3D9Factory.GenericFailureHResult;
        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, device.SetScissorRect(rectangle));
        NativeCalls.Clear();
        _scissorResult = 0;
        _renderStateResult = 0;

        int result = device.SetScissorRect(rectangle);

        Assert.AreEqual((0, "Rect:2:3:7:10,State:174:1"), (result, string.Join(',', NativeCalls)));
    }

    [TestMethod]
    public void WhenRenderStateSetFailsThenOriginalHResultIsPreservedAndRectangleIsRetried()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9PointAndSizeRect rectangle = new(2, 3, 5, 7);
        _renderStateResult = Direct3D9Factory.DeviceLostHResult;
        int failedResult = device.SetScissorRect(rectangle);
        NativeCalls.Clear();
        _renderStateResult = 0;

        int retryResult = device.SetScissorRect(rectangle);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, 0, 0, "Rect:2:3:7:10,State:174:1"),
            (failedResult, retryResult, device.UnusableReasonHResult, string.Join(',', NativeCalls)));
    }

    [TestMethod]
    public void WhenSettingScissorAfterDeviceLossWasProcessedThenNativeCallsStillRun()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.SetScissorRect(new Direct3D9PointAndSizeRect(2, 3, 5, 7));

        Assert.AreEqual((0, "Rect:2:3:7:10,State:174:1"), (result, string.Join(',', NativeCalls)));
    }

    [TestMethod]
    public void WhenSettingScissorWithReleasedDeviceThenNativeCallsAreRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();
        NativeCalls.Clear();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.SetScissorRect(new Direct3D9PointAndSizeRect(2, 3, 5, 7)));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device) =>
        new(device, null, 0, Devtype.Hal, 0, default);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetRenderState(IDirect3DDevice9* self, Renderstatetype state, uint value)
    {
        NativeCalls.Add($"State:{(uint) state}:{value}");
        return _renderStateResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetScissorRect(IDirect3DDevice9* self, Direct3D9SurfaceRect* rectangle)
    {
        NativeCalls.Add($"Rect:{rectangle->Left}:{rectangle->Top}:{rectangle->Right}:{rectangle->Bottom}");
        return _scissorResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 77);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[57] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Renderstatetype, uint, int>) &SetRenderState;
            vtable[75] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Direct3D9SurfaceRect*, int>) &SetScissorRect;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
