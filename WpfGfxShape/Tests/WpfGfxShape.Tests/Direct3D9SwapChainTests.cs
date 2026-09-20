using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9SwapChainTests
{
    private static readonly nint[] BackBuffers = new nint[2];
    private static int _getBackBufferCallCount;
    private static int _getBackBufferFailureIndex;
    private static int _swapChainAddRefCount;
    private static int _swapChainReleaseCount;
    private static int _swapChainExReleaseCount;
    private static nint _swapChainEx;
    private static readonly int[] SurfaceAddRefCounts = new int[2];
    private static readonly int[] SurfaceReleaseCounts = new int[2];
    private static readonly List<string> ReleaseOrder = [];

    [TestInitialize]
    public void Initialize()
    {
        Array.Clear(BackBuffers);
        Array.Clear(SurfaceAddRefCounts);
        Array.Clear(SurfaceReleaseCounts);
        _getBackBufferCallCount = 0;
        _getBackBufferFailureIndex = -1;
        _swapChainAddRefCount = 0;
        _swapChainReleaseCount = 0;
        _swapChainExReleaseCount = 0;
        _swapChainEx = 0;
        ReleaseOrder.Clear();
    }

    [TestMethod]
    public void WhenSwapChainIsInitializedThenAllBackBuffersAreCachedWithIndependentReturnedOwnership()
    {
        using FakeSurfaceObject firstSurface = new(0);
        using FakeSurfaceObject secondSurface = new(1);
        using FakeSwapChainObject swapChainObject = new(firstSurface.Surface, secondSurface.Surface);
        Direct3D9ResourceManager resourceManager = new();

        int result = Direct3D9SwapChain.TryCreate(
            resourceManager,
            swapChainObject.SwapChain,
            2,
            out Direct3D9SwapChain? swapChain);
        int getResult = swapChain!.TryGetBackBuffer(1, out Direct3D9Surface? returnedSurface);
        returnedSurface!.Dispose();
        int resourceCountBeforeRelease = resourceManager.ResourceCount;
        swapChain.Dispose();

        Assert.AreEqual(
            (0, 0, 2, 1, 2, 3, 0, 1),
            (result, getResult, _getBackBufferCallCount, SurfaceAddRefCounts[1], SurfaceReleaseCounts[1],
                resourceCountBeforeRelease, resourceManager.ResourceCount, _swapChainReleaseCount));
    }

    [TestMethod]
    public void WhenCachedBackBufferIsBorrowedThenOnlyCachedSurfaceMemoryRemainsCounted()
    {
        using FakeSurfaceObject firstSurface = new(0);
        using FakeSurfaceObject secondSurface = new(1);
        using FakeSwapChainObject swapChainObject = new(firstSurface.Surface, secondSurface.Surface);
        Direct3D9ResourceManager resourceManager = new();
        _ = Direct3D9SwapChain.TryCreate(resourceManager, swapChainObject.SwapChain, 2, out Direct3D9SwapChain? swapChain);

        int result = swapChain!.TryGetBackBuffer(0, out Direct3D9Surface? backBuffer);
        uint memoryWithBorrowedBackBuffer = resourceManager.TotalVideoMemoryConsumption;
        backBuffer!.Dispose();
        uint memoryWithCachedBackBuffers = resourceManager.TotalVideoMemoryConsumption;
        swapChain.Dispose();

        Assert.AreEqual(
            (0, 12288u, 12288u, 0u),
            (result, memoryWithBorrowedBackBuffer, memoryWithCachedBackBuffers,
                resourceManager.TotalVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenCachedBackBufferIndexIsOutOfRangeThenInvalidArgumentIsReturnedWithoutNativeOrReferenceSideEffects()
    {
        using FakeSurfaceObject firstSurface = new(0);
        using FakeSurfaceObject secondSurface = new(1);
        using FakeSwapChainObject swapChainObject = new(firstSurface.Surface, secondSurface.Surface);
        Direct3D9ResourceManager resourceManager = new();
        _ = Direct3D9SwapChain.TryCreate(resourceManager, swapChainObject.SwapChain, 2, out Direct3D9SwapChain? swapChain);

        int result = swapChain!.TryGetBackBuffer(2, out Direct3D9Surface? backBuffer);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, null, 2, 0, 0, 3),
            (result, backBuffer, _getBackBufferCallCount, SurfaceAddRefCounts[0], SurfaceAddRefCounts[1],
                resourceManager.ResourceCount));
        swapChain.Dispose();
    }

    [TestMethod]
    public void WhenCachedBackBufferIsReturnedThenUseContextStateIsUnchanged()
    {
        using FakeSurfaceObject firstSurface = new(0);
        using FakeSurfaceObject secondSurface = new(1);
        using FakeSwapChainObject swapChainObject = new(firstSurface.Surface, secondSurface.Surface);
        Direct3D9ResourceManager resourceManager = new();
        _ = Direct3D9SwapChain.TryCreate(resourceManager, swapChainObject.SwapChain, 2, out Direct3D9SwapChain? swapChain);
        uint depth = resourceManager.EnterUseContext();

        int result = swapChain!.TryGetBackBuffer(0, out Direct3D9Surface? backBuffer);

        Assert.AreEqual((0, true, 0u), (result, resourceManager.IsInUseContext, backBuffer!.ActiveUseContextDepth));
        backBuffer.Dispose();
        resourceManager.ExitUseContext(depth);
        swapChain.Dispose();
    }

    [TestMethod]
    public void WhenReleasedSwapChainBackBufferIsRequestedThenObjectDisposedIsThrownWithoutNativeCall()
    {
        using FakeSurfaceObject firstSurface = new(0);
        using FakeSurfaceObject secondSurface = new(1);
        using FakeSwapChainObject swapChainObject = new(firstSurface.Surface, secondSurface.Surface);
        Direct3D9ResourceManager resourceManager = new();
        _ = Direct3D9SwapChain.TryCreate(resourceManager, swapChainObject.SwapChain, 2, out Direct3D9SwapChain? swapChain);
        swapChain!.Dispose();

        Assert.Throws<ObjectDisposedException>(() => swapChain.TryGetBackBuffer(0, out _));
        Assert.AreEqual(2, _getBackBufferCallCount);
    }

    [TestMethod]
    public void WhenSwapChainIsReleasedThenNativeSwapChainPrecedesCachedBackBuffers()
    {
        using FakeSurfaceObject firstSurface = new(0);
        using FakeSurfaceObject secondSurface = new(1);
        using FakeSwapChainObject swapChainObject = new(firstSurface.Surface, secondSurface.Surface);
        Direct3D9ResourceManager resourceManager = new();
        _ = Direct3D9SwapChain.TryCreate(resourceManager, swapChainObject.SwapChain, 2, out Direct3D9SwapChain? swapChain);

        swapChain!.Dispose();

        CollectionAssert.AreEqual(
            new[] { "SwapChain", "Surface0", "Surface1" },
            ReleaseOrder);
    }

    [TestMethod]
    public void WhenSwapChainWithExInterfaceIsReleasedThenBaseExAndCachedBackBuffersAreReleasedInOrder()
    {
        using FakeSurfaceObject firstSurface = new(0);
        using FakeSurfaceObject secondSurface = new(1);
        using FakeSwapChainObject swapChainObject = new(firstSurface.Surface, secondSurface.Surface, supportsEx: true);
        Direct3D9ResourceManager resourceManager = new();
        _ = Direct3D9SwapChain.TryCreate(resourceManager, swapChainObject.SwapChain, 2, out Direct3D9SwapChain? swapChain);

        swapChain!.Dispose();

        CollectionAssert.AreEqual(
            new[] { "SwapChain", "SwapChainEx", "Surface0", "Surface1" },
            ReleaseOrder);
    }

    [TestMethod]
    public void WhenReleasedSwapChainIsDisposedAgainThenNativeAndCachedResourcesAreNotReleasedAgain()
    {
        using FakeSurfaceObject firstSurface = new(0);
        using FakeSurfaceObject secondSurface = new(1);
        using FakeSwapChainObject swapChainObject = new(firstSurface.Surface, secondSurface.Surface, supportsEx: true);
        Direct3D9ResourceManager resourceManager = new();
        _ = Direct3D9SwapChain.TryCreate(resourceManager, swapChainObject.SwapChain, 2, out Direct3D9SwapChain? swapChain);

        swapChain!.Dispose();
        swapChain.Dispose();

        Assert.AreEqual(
            (1, 1, 1, 1, 0),
            (_swapChainReleaseCount, _swapChainExReleaseCount, SurfaceReleaseCounts[0], SurfaceReleaseCounts[1],
                resourceManager.ResourceCount));
    }

    [TestMethod]
    public void WhenManagerDestroysSwapChainInsideUseContextThenReleaseOrderAndUseContextArePreserved()
    {
        using FakeSurfaceObject firstSurface = new(0);
        using FakeSurfaceObject secondSurface = new(1);
        using FakeSwapChainObject swapChainObject = new(firstSurface.Surface, secondSurface.Surface, supportsEx: true);
        Direct3D9ResourceManager resourceManager = new();
        _ = Direct3D9SwapChain.TryCreate(resourceManager, swapChainObject.SwapChain, 2, out _);
        uint depth = resourceManager.EnterUseContext();

        resourceManager.DestroyAllResources();

        Assert.AreEqual(
            (true, depth, 0, "SwapChain,SwapChainEx,Surface0,Surface1"),
            (resourceManager.IsInUseContext, depth, resourceManager.ResourceCount, string.Join(',', ReleaseOrder)));
        resourceManager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenBackBufferInitializationFailsThenInitializedBuffersAndSwapChainAreReleased()
    {
        using FakeSurfaceObject firstSurface = new(0);
        using FakeSurfaceObject secondSurface = new(1);
        using FakeSwapChainObject swapChainObject = new(firstSurface.Surface, secondSurface.Surface);
        Direct3D9ResourceManager resourceManager = new();
        _getBackBufferFailureIndex = 1;

        int result = Direct3D9SwapChain.TryCreate(
            resourceManager,
            swapChainObject.SwapChain,
            2,
            out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 2, 1, 1, 1, 0, null),
            (result, _getBackBufferCallCount, SurfaceReleaseCounts[0], SurfaceReleaseCounts[1],
                _swapChainReleaseCount, resourceManager.ResourceCount, swapChain));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int QuerySwapChainInterface(IDirect3DSwapChain9* self, Guid* interfaceId, void** result)
    {
        *result = (void*) _swapChainEx;
        return _swapChainEx == 0 ? unchecked((int) 0x80004002) : 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetBackBuffer(
        IDirect3DSwapChain9* self,
        uint index,
        BackbufferType type,
        IDirect3DSurface9** surface)
    {
        _getBackBufferCallCount++;
        *surface = (IDirect3DSurface9*) BackBuffers[index];
        return index == _getBackBufferFailureIndex ? Direct3D9Factory.InvalidCallHResult : 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefSwapChain(IDirect3DSwapChain9* self)
    {
        _swapChainAddRefCount++;
        return 2;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSwapChain(IDirect3DSwapChain9* self)
    {
        _swapChainReleaseCount++;
        ReleaseOrder.Add("SwapChain");
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSwapChainEx(IDirect3DSwapChain9Ex* self)
    {
        _swapChainExReleaseCount++;
        ReleaseOrder.Add("SwapChainEx");
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefSurface(IDirect3DSurface9* self)
    {
        SurfaceAddRefCounts[GetSurfaceIndex(self)]++;
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSurface(IDirect3DSurface9* self)
    {
        int index = GetSurfaceIndex(self);
        SurfaceReleaseCounts[index]++;
        ReleaseOrder.Add($"Surface{index}");
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceDescription(IDirect3DSurface9* self, SurfaceDesc* description)
    {
        int index = GetSurfaceIndex(self);
        *description = new SurfaceDesc(
            format: Format.A8R8G8B8,
            type: Resourcetype.Surface,
            pool: Pool.Default,
            width: index == 0 ? 64u : 32u,
            height: 32);
        return 0;
    }

    private static int GetSurfaceIndex(IDirect3DSurface9* surface)
    {
        return *((int*) surface + 2);
    }

    private struct FakeSwapChainObject : IDisposable
    {
        private nint _memory;
        private nint _exMemory;
        internal IDirect3DSwapChain9* SwapChain;

        internal FakeSwapChainObject(
            IDirect3DSurface9* firstSurface,
            IDirect3DSurface9* secondSurface,
            bool supportsEx = false)
        {
            BackBuffers[0] = (nint) firstSurface;
            BackBuffers[1] = (nint) secondSurface;
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 11);
            void** memory = (void**) _memory;
            SwapChain = (IDirect3DSwapChain9*) memory;
            void** vtable = memory + 1;
            SwapChain->LpVtbl = vtable;
            vtable[0] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, Guid*, void**, int>) &QuerySwapChainInterface;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, uint>) &AddRefSwapChain;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, uint>) &ReleaseSwapChain;
            vtable[5] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, uint, BackbufferType, IDirect3DSurface9**, int>) &GetBackBuffer;

            _exMemory = 0;
            if (supportsEx)
            {
                _exMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
                void** exMemory = (void**) _exMemory;
                IDirect3DSwapChain9Ex* swapChainEx = (IDirect3DSwapChain9Ex*) exMemory;
                void** exVtable = exMemory + 1;
                swapChainEx->LpVtbl = exVtable;
                exVtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9Ex*, uint>) &ReleaseSwapChainEx;
                _swapChainEx = (nint) swapChainEx;
            }
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _exMemory);
            NativeMemory.Free((void*) _memory);
            _exMemory = 0;
            _memory = 0;
            _swapChainEx = 0;
            SwapChain = null;
        }
    }

    private struct FakeSurfaceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DSurface9* Surface;

        internal FakeSurfaceObject(int index)
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) ((sizeof(nint) * 14) + sizeof(int)));
            void** memory = (void**) _memory;
            Surface = (IDirect3DSurface9*) memory;
            void** vtable = memory + 1;
            Surface->LpVtbl = vtable;
            *((int*) Surface + 2) = index;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &AddRefSurface;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseSurface;
            vtable[12] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) &GetSurfaceDescription;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }
}
