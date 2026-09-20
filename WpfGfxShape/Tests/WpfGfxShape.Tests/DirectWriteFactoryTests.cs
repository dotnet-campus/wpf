using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
public sealed unsafe class DirectWriteFactoryTests
{
    private static void*** _unknown;
    private static void*** _factory;
    private static Guid _requestedInterfaceId;
    private static int _createCalls;
    private static int _queryInterfaceCalls;
    private static int _unknownReleaseCalls;
    private static int _factoryReleaseCalls;
    private static int _createResult;
    private static int _queryInterfaceResult;
    private static bool _returnUnknownOnFailure;
    private static bool _returnFactoryOnFailure;

    [TestInitialize]
    public void Initialize()
    {
        _unknown = null;
        _factory = null;
        _requestedInterfaceId = default;
        _createCalls = 0;
        _queryInterfaceCalls = 0;
        _unknownReleaseCalls = 0;
        _factoryReleaseCalls = 0;
        _createResult = 0;
        _queryInterfaceResult = 0;
        _returnUnknownOnFailure = false;
        _returnFactoryOnFailure = false;
    }

    [TestMethod]
    public void WhenFactoryIsCreatedThenSharedFactoryAndQueryInterfaceAreUsed()
    {
        using FakeDirectWriteObjects objects = new();
        using DirectWriteFactory factory = DirectWriteFactory.Create(objects.CreateFactoryEntryPoint);

        Assert.AreEqual(
            (objects.Factory, DirectWriteFactory.InterfaceId, 1, 1, 1),
            (factory.DangerousGetFactoryNoRef(), _requestedInterfaceId, _createCalls, _queryInterfaceCalls, _unknownReleaseCalls));
    }

    [TestMethod]
    public void WhenFactoryIsDisposedTwiceThenOwnedReferenceIsReleasedOnce()
    {
        using FakeDirectWriteObjects objects = new();
        DirectWriteFactory factory = DirectWriteFactory.Create(objects.CreateFactoryEntryPoint);

        factory.Dispose();
        factory.Dispose();

        Assert.AreEqual(1, _factoryReleaseCalls);
    }

    [TestMethod]
    public void WhenFactoryIsDisposedThenBorrowingThrows()
    {
        using FakeDirectWriteObjects objects = new();
        DirectWriteFactory factory = DirectWriteFactory.Create(objects.CreateFactoryEntryPoint);
        factory.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => factory.DangerousGetFactoryNoRef());
    }

    [TestMethod]
    public void WhenCreateFailsAfterReturningUnknownThenPartialReferenceIsReleased()
    {
        using FakeDirectWriteObjects objects = new();
        _createResult = unchecked((int) 0x80004005);
        _returnUnknownOnFailure = true;

        Assert.ThrowsExactly<COMException>(() => DirectWriteFactory.Create(objects.CreateFactoryEntryPoint));

        Assert.AreEqual(1, _unknownReleaseCalls);
    }

    [TestMethod]
    public void WhenQueryInterfaceFailsAfterReturningFactoryThenBothReferencesAreReleased()
    {
        using FakeDirectWriteObjects objects = new();
        _queryInterfaceResult = unchecked((int) 0x80004002);
        _returnFactoryOnFailure = true;

        Assert.ThrowsExactly<InvalidCastException>(() => DirectWriteFactory.Create(objects.CreateFactoryEntryPoint));

        Assert.AreEqual((1, 1), (_unknownReleaseCalls, _factoryReleaseCalls));
    }

    [TestMethod]
    public void WhenCacheIsReadTwiceThenEntryPointIsLoadedAndFactoryIsCreatedOnce()
    {
        using FakeDirectWriteObjects objects = new();
        TrackingDisposable moduleOwner = new();
        int loadCalls = 0;
        using DirectWriteFactoryCache cache = new(() =>
        {
            loadCalls++;
            return (objects.CreateFactoryEntryPoint, moduleOwner);
        });

        nint first = cache.GetFactoryNoRef();
        nint second = cache.GetFactoryNoRef();

        Assert.AreEqual((objects.Factory, objects.Factory, 1, 1), (first, second, loadCalls, _createCalls));
    }

    [TestMethod]
    public void WhenCacheIsDisposedThenFactoryIsReleasedBeforeModuleOwner()
    {
        using FakeDirectWriteObjects objects = new();
        TrackingDisposable moduleOwner = new(() => Assert.AreEqual(1, _factoryReleaseCalls));
        DirectWriteFactoryCache cache = new(() => (objects.CreateFactoryEntryPoint, moduleOwner));
        cache.GetFactoryNoRef();

        cache.Dispose();

        Assert.IsTrue(moduleOwner.IsDisposed);
    }

    [TestMethod]
    public void WhenFactoryCreationFailsThenLoadedModuleOwnerIsDisposed()
    {
        using FakeDirectWriteObjects objects = new();
        TrackingDisposable moduleOwner = new();
        _createResult = unchecked((int) 0x80004005);
        using DirectWriteFactoryCache cache = new(() => (objects.CreateFactoryEntryPoint, moduleOwner));

        Assert.ThrowsExactly<COMException>(() => cache.GetFactoryNoRef());

        Assert.IsTrue(moduleOwner.IsDisposed);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateFactory(uint factoryType, Guid* interfaceId, void**** unknown)
    {
        _createCalls++;
        _requestedInterfaceId = *interfaceId;
        *unknown = _createResult >= 0 || _returnUnknownOnFailure ? _unknown : null;
        return _createResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int QueryInterface(void*** self, Guid* interfaceId, void**** factory)
    {
        _queryInterfaceCalls++;
        _requestedInterfaceId = *interfaceId;
        *factory = _queryInterfaceResult >= 0 || _returnFactoryOnFailure ? _factory : null;
        return _queryInterfaceResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint UnknownRelease(void*** self)
    {
        _unknownReleaseCalls++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint FactoryRelease(void*** self)
    {
        _factoryReleaseCalls++;
        return 0;
    }

    private struct FakeDirectWriteObjects : IDisposable
    {
        private nint _memory;

        public FakeDirectWriteObjects()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 10);
            void** memory = (void**) _memory;
            _unknown = (void***) memory;
            void** unknownVtable = memory + 2;
            _factory = (void***) (memory + 5);
            void** factoryVtable = memory + 6;
            *_unknown = unknownVtable;
            unknownVtable[0] = (void*) (delegate* unmanaged[Stdcall]<void***, Guid*, void****, int>) &QueryInterface;
            unknownVtable[2] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &UnknownRelease;
            *_factory = factoryVtable;
            factoryVtable[2] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &FactoryRelease;
        }

        internal nint Factory => (nint) _factory;

        internal nint CreateFactoryEntryPoint =>
            (nint) (delegate* unmanaged[Stdcall]<uint, Guid*, void****, int>) &CreateFactory;

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            _unknown = null;
            _factory = null;
        }
    }

    private sealed class TrackingDisposable(Action? disposing = null) : IDisposable
    {
        internal bool IsDisposed { get; private set; }

        public void Dispose()
        {
            disposing?.Invoke();
            IsDisposed = true;
        }
    }
}
