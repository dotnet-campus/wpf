using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceGpuQueryCreationTests
{
    private static Querytype _queryType;
    private static int _probeCallCount;
    private static int _createCallCount;
    private static int _issueCallCount;
    private static int _queryReleaseCount;
    private static int _probeResult;
    private static int _createResult;
    private static nint _queryToReturn;

    [TestInitialize]
    public void Initialize()
    {
        _queryType = 0;
        _probeCallCount = 0;
        _createCallCount = 0;
        _issueCallCount = 0;
        _queryReleaseCount = 0;
        _probeResult = 0;
        _createResult = 0;
        _queryToReturn = 0;
    }

    [TestMethod]
    public void WhenInsertingFirstMarkerThenSupportIsProbedWithNullOutputBeforeQueryCreation()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeQueryObject queryObject = new();
        _queryToReturn = (nint) queryObject.Query;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.InsertGpuMarker(1);

        Assert.AreEqual(
            (0, Querytype.Event, 1, 1, 1, 0),
            (result, _queryType, _probeCallCount, _createCallCount, _issueCallCount, _queryReleaseCount));
    }

    [TestMethod]
    public void WhenProbeAndQueryCreationReturnNonzeroSuccessThenQueryOwnershipIsTransferred()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeQueryObject queryObject = new();
        _probeResult = 1;
        _createResult = 1;
        _queryToReturn = (nint) queryObject.Query;
        Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.InsertGpuMarker(1);
        device.Dispose();
        device.Dispose();

        Assert.AreEqual((0, 1, 1, 1, 1), (result, _probeCallCount, _createCallCount, _issueCallCount, _queryReleaseCount));
    }

    [TestMethod]
    public void WhenQueryCreationFailsWithPointerThenPointerIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeQueryObject queryObject = new();
        _queryToReturn = (nint) queryObject.Query;
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.InsertGpuMarker(1);

        Assert.AreEqual((0, 1, 1, 0, 1), (result, _probeCallCount, _createCallCount, _issueCallCount, _queryReleaseCount));
    }

    [TestMethod]
    public void WhenQueryCreationSucceedsWithoutPointerThenMarkerIsNotIssued()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.InsertGpuMarker(1);

        Assert.AreEqual((0, 1, 1, 0), (result, _probeCallCount, _createCallCount, _issueCallCount));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.NotAvailableHResult)]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    public void WhenProbeFailsThenFailureIsNormalizedAndQueriesRemainDisabled(int probeResult)
    {
        using FakeDeviceObject deviceObject = new();
        _probeResult = probeResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int firstResult = device.InsertGpuMarker(1);
        int secondResult = device.InsertGpuMarker(2);

        Assert.AreEqual((0, 0, 1, 0), (firstResult, secondResult, _probeCallCount, _createCallCount));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.NotAvailableHResult)]
    public void WhenQueryCreationHasRecoverableFailureThenQueryIsReleasedWithoutDisablingMarkers(int createResult)
    {
        using FakeDeviceObject deviceObject = new();
        using FakeQueryObject queryObject = new();
        _queryToReturn = (nint) queryObject.Query;
        _createResult = createResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int firstResult = device.InsertGpuMarker(1);
        int secondResult = device.InsertGpuMarker(2);

        Assert.AreEqual((0, 0, 1, 2, 2), (firstResult, secondResult, _probeCallCount, _createCallCount, _queryReleaseCount));
    }

    [TestMethod]
    public void WhenQueryCreationHasDriverInternalErrorThenQueryIsReleasedAndMarkersAreDisabled()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeQueryObject queryObject = new();
        _queryToReturn = (nint) queryObject.Query;
        _createResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int firstResult = device.InsertGpuMarker(1);
        int secondResult = device.InsertGpuMarker(2);

        Assert.AreEqual((0, 0, 1, 1, 1), (firstResult, secondResult, _probeCallCount, _createCallCount, _queryReleaseCount));
    }

    [TestMethod]
    public void WhenNativeQueryIsDisposedTwiceThenItIsReleasedOnceAndRejectsOperations()
    {
        using FakeQueryObject queryObject = new();
        Direct3D9GpuQuery query = new(queryObject.Query);

        query.Dispose();
        query.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => query.Issue());
        Assert.AreEqual(1, _queryReleaseCount);
    }

    [TestMethod]
    public void WhenInsertingMarkerAfterDisposalThenNativeQueryCreationIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.InsertGpuMarker(1));
        Assert.AreEqual((0, 0), (_probeCallCount, _createCallCount));
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateQuery(IDirect3DDevice9* self, Querytype type, IDirect3DQuery9** query)
    {
        _queryType = type;
        if (query is null)
        {
            _probeCallCount++;
            return _probeResult;
        }

        _createCallCount++;
        *query = (IDirect3DQuery9*) _queryToReturn;
        return _createResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseQuery(IDirect3DQuery9* self)
    {
        _queryReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int IssueQuery(IDirect3DQuery9* self, uint issueFlags)
    {
        _issueCallCount++;
        return 0;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 121);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            Device->LpVtbl = memory + 1;
            Device->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            Device->LpVtbl[118] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Querytype, IDirect3DQuery9**, int>) &CreateQuery;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private struct FakeQueryObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DQuery9* Query;

        public FakeQueryObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 9);
            void** memory = (void**) _memory;
            Query = (IDirect3DQuery9*) memory;
            Query->LpVtbl = memory + 1;
            Query->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DQuery9*, uint>) &ReleaseQuery;
            Query->LpVtbl[6] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DQuery9*, uint, int>) &IssueQuery;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Query = null;
        }
    }
}
