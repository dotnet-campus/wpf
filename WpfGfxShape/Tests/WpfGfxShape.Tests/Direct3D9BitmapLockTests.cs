using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9BitmapLockTests
{
    private static Direct3D9BitmapSourceRectangle _lockRectangle;
    private static uint _lockFlags;
    private static int _lockResult;
    private static int _releaseCount;
    private static uint _width;
    private static uint _height;
    private static uint _stride;
    private static uint _bufferSize;
    private static nint _data;
    private static MilPixelFormat _pixelFormat;

    [TestInitialize]
    public void Initialize()
    {
        _lockRectangle = default;
        _lockFlags = 0;
        _lockResult = Direct3D9Factory.SuccessHResult;
        _releaseCount = 0;
        _width = 8;
        _height = 16;
        _stride = 64;
        _bufferSize = 512;
        _data = 0x1234;
        _pixelFormat = MilPixelFormat.Pbgra32Bpp;
    }

    [TestMethod]
    public void WhenBitmapIsLockedThenSlotEightReceivesRectangleAndReadFlag()
    {
        using FakeBitmapLockObject lockObject = new();
        using FakeBitmapObject bitmapObject = new(lockObject.Instance);
        Direct3D9BitmapSourceRectangle rectangle = new(1, 2, 3, 4);

        int result = Direct3D9Bitmap.Lock(bitmapObject.Instance, rectangle, MilBitmapLockFlags.Read, out nint bitmapLock);

        Assert.AreEqual(
            (0, lockObject.Instance, rectangle, (uint) MilBitmapLockFlags.Read),
            (result, bitmapLock, _lockRectangle, _lockFlags));
    }

    [TestMethod]
    public void WhenBitmapLockFailsWithAnInterfaceThenReturnedReferenceIsReleasedAndOutputIsCleared()
    {
        using FakeBitmapLockObject lockObject = new();
        using FakeBitmapObject bitmapObject = new(lockObject.Instance);
        _lockResult = Direct3D9Factory.GenericFailureHResult;

        int result = Direct3D9Bitmap.Lock(bitmapObject.Instance, default, MilBitmapLockFlags.Read, out nint bitmapLock);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 1),
            (result, bitmapLock, _releaseCount));
    }

    [TestMethod]
    public void WhenLockValuesAreQueriedThenDocumentedSlotsReturnNativeValues()
    {
        using FakeBitmapLockObject lockObject = new();

        int sizeResult = Direct3D9BitmapLock.GetSize(lockObject.Instance, out uint width, out uint height);
        int strideResult = Direct3D9BitmapLock.GetStride(lockObject.Instance, out uint stride);
        int dataResult = Direct3D9BitmapLock.GetDataPointer(lockObject.Instance, out uint bufferSize, out nint data);
        int formatResult = Direct3D9BitmapLock.GetPixelFormat(lockObject.Instance, out MilPixelFormat pixelFormat);

        Assert.AreEqual(
            (0, 8u, 16u, 0, 64u, 0, 512u, (nint) 0x1234, 0, MilPixelFormat.Pbgra32Bpp),
            (sizeResult, width, height, strideResult, stride, dataResult, bufferSize, data, formatResult, pixelFormat));
    }

    [TestMethod]
    public void WhenLockPointerIsNullThenQueriesReturnInvalidCallAndClearedOutputs()
    {
        int sizeResult = Direct3D9BitmapLock.GetSize(0, out uint width, out uint height);
        int strideResult = Direct3D9BitmapLock.GetStride(0, out uint stride);
        int dataResult = Direct3D9BitmapLock.GetDataPointer(0, out uint bufferSize, out nint data);
        int formatResult = Direct3D9BitmapLock.GetPixelFormat(0, out MilPixelFormat pixelFormat);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0u, 0u, Direct3D9Factory.InvalidCallHResult, 0u, Direct3D9Factory.InvalidCallHResult, 0u, 0, Direct3D9Factory.InvalidCallHResult, MilPixelFormat.Undefined),
            (sizeResult, width, height, strideResult, stride, dataResult, bufferSize, data, formatResult, pixelFormat));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Lock(
        void*** bitmap,
        Direct3D9BitmapSourceRectangle* rectangle,
        uint flags,
        void**** bitmapLock)
    {
        _lockRectangle = *rectangle;
        _lockFlags = flags;
        *bitmapLock = *(void****) ((byte*) bitmap + sizeof(nint));
        return _lockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(void*** bitmapLock)
    {
        _releaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSize(void*** bitmapLock, uint* width, uint* height)
    {
        *width = _width;
        *height = _height;
        return Direct3D9Factory.SuccessHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetStride(void*** bitmapLock, uint* stride)
    {
        *stride = _stride;
        return Direct3D9Factory.SuccessHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetDataPointer(void*** bitmapLock, uint* bufferSize, byte** data)
    {
        *bufferSize = _bufferSize;
        *data = (byte*) _data;
        return Direct3D9Factory.SuccessHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetPixelFormat(void*** bitmapLock, MilPixelFormat* pixelFormat)
    {
        *pixelFormat = _pixelFormat;
        return Direct3D9Factory.SuccessHResult;
    }

    private struct FakeBitmapObject : IDisposable
    {
        private nint _memory;
        internal nint Instance;

        internal FakeBitmapObject(nint bitmapLock)
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 11);
            void** memory = (void**) _memory;
            Instance = (nint) memory;
            void** vtable = memory + 2;
            *memory = vtable;
            memory[1] = (void*) bitmapLock;
            vtable[8] = (void*) (delegate* unmanaged[Stdcall]<void***, Direct3D9BitmapSourceRectangle*, uint, void****, int>) &Lock;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Instance = 0;
        }
    }

    private sealed class FakeBitmapLockObject : IDisposable
    {
        private nint _memory;
        internal nint Instance;

        internal FakeBitmapLockObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 8);
            void** memory = (void**) _memory;
            Instance = (nint) memory;
            void** vtable = memory + 1;
            *memory = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &Release;
            vtable[3] = (void*) (delegate* unmanaged[Stdcall]<void***, uint*, uint*, int>) &GetSize;
            vtable[4] = (void*) (delegate* unmanaged[Stdcall]<void***, uint*, int>) &GetStride;
            vtable[5] = (void*) (delegate* unmanaged[Stdcall]<void***, uint*, byte**, int>) &GetDataPointer;
            vtable[6] = (void*) (delegate* unmanaged[Stdcall]<void***, MilPixelFormat*, int>) &GetPixelFormat;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Instance = 0;
        }
    }
}
