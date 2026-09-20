using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
public sealed unsafe class Direct3D9ObjectsLifetimeTests
{
    private static readonly List<string> ReleaseOrder = [];

    [TestInitialize]
    public void Initialize() => ReleaseOrder.Clear();

    [TestMethod]
    public void WhenBaseFactoryIsDisposedThenOwnedResourcesAreReleasedInNativeOrder()
    {
        using FakeDirect3DObject baseFactory = new("base");
        Direct3D9SoftwareRasterizerLoader loader = CreateLoadedSoftwareRasterizerLoader();
        Direct3D9Objects objects = new(new TestModuleHandle("module"), baseFactory.Direct3D, null, loader);

        objects.Dispose();

        Assert.AreEqual("base,module,software", string.Join(',', ReleaseOrder));
    }

    [TestMethod]
    public void WhenBaseAndExtendedFactoriesAreDisposedThenIndependentReferencesAreReleasedInNativeOrder()
    {
        using FakeDirect3DObject baseFactory = new("base");
        using FakeDirect3DObject extendedFactory = new("extended");
        Direct3D9Objects objects = new(
            new TestModuleHandle("module"),
            baseFactory.Direct3D,
            extendedFactory.Direct3DEx);

        objects.Dispose();

        Assert.AreEqual("extended,base,module", string.Join(',', ReleaseOrder));
    }

    [TestMethod]
    public void WhenBaseAndExtendedFactoriesHaveSameAddressThenBothOwnedReferencesAreReleased()
    {
        using FakeDirect3DObject factory = new("factory");
        Direct3D9Objects objects = new(
            new TestModuleHandle("module"),
            factory.Direct3D,
            factory.Direct3DEx);

        objects.Dispose();

        Assert.AreEqual("factory,factory,module", string.Join(',', ReleaseOrder));
    }

    [TestMethod]
    public void WhenFactoryObjectsAreDisposedTwiceThenOwnedResourcesAreReleasedOnce()
    {
        using FakeDirect3DObject factory = new("factory");
        Direct3D9SoftwareRasterizerLoader loader = CreateLoadedSoftwareRasterizerLoader();
        Direct3D9Objects objects = new(
            new TestModuleHandle("module"),
            factory.Direct3D,
            factory.Direct3DEx,
            loader);

        objects.Dispose();
        objects.Dispose();

        Assert.AreEqual("factory,factory,module,software", string.Join(',', ReleaseOrder));
    }

    [TestMethod]
    public void WhenFactoryObjectsAreDisposedThenIdentityAccessIsRejected()
    {
        using FakeDirect3DObject factory = new("factory");
        Direct3D9Objects objects = new(new TestModuleHandle("module"), factory.Direct3D, null);
        objects.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = objects.Direct3D);
    }

    [TestMethod]
    public void WhenFactoryObjectsAreDisposedThenExtendedIdentityAccessIsRejected()
    {
        using FakeDirect3DObject factory = new("factory");
        Direct3D9Objects objects = new(new TestModuleHandle("module"), factory.Direct3D, null);
        objects.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = objects.Direct3DEx);
    }

    private static Direct3D9SoftwareRasterizerLoader CreateLoadedSoftwareRasterizerLoader()
    {
        Direct3D9SoftwareRasterizerLoader loader = new(
            _ => new TestModuleHandle("software"),
            (_, _) => 1);
        _ = loader.GetSoftwareInfo();
        return loader;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(void* self)
    {
        nint* memory = (nint*) self;
        GCHandle nameHandle = GCHandle.FromIntPtr(memory[1]);
        ReleaseOrder.Add((string) nameHandle.Target!);
        return 0;
    }

    private sealed class TestModuleHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private readonly string _name;

        internal TestModuleHandle(string name)
            : base(true)
        {
            _name = name;
            SetHandle(1);
        }

        protected override bool ReleaseHandle()
        {
            ReleaseOrder.Add(_name);
            handle = 0;
            return true;
        }
    }

    private struct FakeDirect3DObject : IDisposable
    {
        private nint _memory;
        private GCHandle _nameHandle;
        internal IDirect3D9* Direct3D;
        internal IDirect3D9Ex* Direct3DEx;

        internal FakeDirect3DObject(string name)
        {
            _nameHandle = GCHandle.Alloc(name);
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            nint* memory = (nint*) _memory;
            Direct3D = (IDirect3D9*) memory;
            Direct3DEx = (IDirect3D9Ex*) memory;
            void** vtable = (void**) (memory + 2);
            memory[0] = (nint) vtable;
            memory[1] = GCHandle.ToIntPtr(_nameHandle);
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &Release;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Direct3D = null;
            Direct3DEx = null;
            if (_nameHandle.IsAllocated)
            {
                _nameHandle.Free();
            }
        }
    }
}
