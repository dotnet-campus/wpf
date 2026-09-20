using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9BitmapTexturePopulationPreparedPopulatorTests
{
    private static List<string>? _calls;

    [TestMethod]
    public void WhenSharedBitsArePushedThenLockRemainsAliveUntilSurfaceCallerReferenceIsReleased()
    {
        using FakeSurfaceObject surfaceObject = new();
        _calls = [];
        byte* bits = stackalloc byte[32];
        using Direct3D9BitmapSystemMemorySurfaceSource surfaceSource = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                _calls.Add("surface");
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        Direct3D9BitmapTexturePopulationPreparer preparer = new(
            surfaceSource,
            (out nint bitmapLock) =>
            {
                _calls.Add("lock");
                bitmapLock = 41;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out uint bufferSize, out nint data) =>
            {
                bufferSize = 32;
                data = (nint) bits;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out uint stride) =>
            {
                stride = 16;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out MilPixelFormat format) =>
            {
                format = MilPixelFormat.Pbgra32Bpp;
                return Direct3D9Factory.SuccessHResult;
            },
            resource => _calls.Add($"prepare-release:{resource}"),
            true,
            2,
            4,
            2);
        Direct3D9BitmapTexturePopulator populator = new(
            preparer,
            (nint _, ReadOnlySpan<Direct3D9BitmapRealizationRectangle> _, Direct3D9SystemMemoryUpdateSurface _, bool copySource) =>
            {
                _calls.Add($"push:{copySource}");
                return Direct3D9Factory.SuccessHResult;
            },
            () =>
            {
                _calls.Add("mipmaps");
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _) => _calls.Add("commit"),
            resource => _calls.Add($"lock-release:{resource}"));
        Direct3D9BitmapRealizationRectangle dirtyRectangle = new(0, 0, 4, 2);

        int result = populator.Populate(5, 1, (nint) (&dirtyRectangle), 7, dirtyRectangle);
        surfaceSource.Dispose();

        Assert.AreEqual(
            (0, "lock,surface,push:False,mipmaps,commit,surface-release,lock-release:41,surface-release"),
            (result, string.Join(',', _calls)));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(void*** surface) => 2;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(void*** surface)
    {
        _calls!.Add("surface-release");
        return 0;
    }

    private struct FakeSurfaceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DSurface9* Surface;

        public FakeSurfaceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            Surface = (IDirect3DSurface9*) memory;
            void** vtable = memory + 1;
            *memory = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &AddRef;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &Release;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }
}
