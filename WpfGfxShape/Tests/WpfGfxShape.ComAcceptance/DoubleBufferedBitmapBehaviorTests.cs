using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class DoubleBufferedBitmapBehaviorTests
{
    [TestMethod]
    public void WhenOwnerIsReleasedThenBackBufferAndLockRetainPixels()
    {
        string path = typeof(DoubleBufferedBitmapBehaviorTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "AotDllPath").Value!;
        nint module = NativeLibrary.Load(path);
        var create = (delegate* unmanaged[Stdcall]<uint, uint, double, double, Guid*, nint, nint*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapCreate");
        var getBack = (delegate* unmanaged[Stdcall]<nint, nint*, uint*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapGetBackBuffer");
        nint owner = 0, bitmap = 0, bitmapLock = 0;
        Guid format = new("6fddc324-4e03-4bfe-b185-3d77768dc910");
        try
        {
            Assert.AreEqual(0, create(2, 2, 96, 120, &format, 0, &owner));
            uint size = 0;
            Assert.AreEqual(0, getBack(owner, &bitmap, &size));
            Assert.AreEqual(16u, size);
            Release(owner);
            owner = 0;
            var lockBitmap = (delegate* unmanaged[Stdcall]<nint, void*, uint, nint*, int>)(*(void***)bitmap)[8];
            Assert.AreEqual(0, lockBitmap(bitmap, null, 2, &bitmapLock));
            Release(bitmap);
            bitmap = 0;
            byte* pixels = null;
            var getData = (delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5];
            Assert.AreEqual(0, getData(bitmapLock, &size, &pixels));
            pixels[0] = 0x12;
            pixels[15] = 0xEF;
            Assert.AreEqual((0x12, 0xEF), ((int)pixels[0], (int)pixels[15]));
        }
        finally
        {
            Release(bitmapLock);
            Release(bitmap);
            Release(owner);
        }
    }

    [TestMethod]
    [DataRow("6fddc324-4e03-4bfe-b185-3d77768dc90c", 9u, 21u)]
    [DataRow("6fddc324-4e03-4bfe-b185-3d77768dc908", 3u, 7u)]
    [DataRow("6fddc324-4e03-4bfe-b185-3d77768dc90b", 6u, 14u)]
    public void WhenFormatHasPaddingThenCopyPixelsOmitsRowPadding(string formatId, uint rowBytes, uint expectedSize)
    {
        string path = typeof(DoubleBufferedBitmapBehaviorTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "AotDllPath").Value!;
        nint module = NativeLibrary.Load(path);
        var create = (delegate* unmanaged[Stdcall]<uint, uint, double, double, Guid*, nint, nint*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapCreate");
        var getBack = (delegate* unmanaged[Stdcall]<nint, nint*, uint*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapGetBackBuffer");
        int apartment = CoInitializeEx(0, 0);
        Assert.IsTrue(apartment >= 0 || apartment == unchecked((int)0x80010106));
        nint owner = 0, bitmap = 0, bitmapLock = 0;
        Guid format = new(formatId);
        try
        {
            Assert.AreEqual(0, create(3, 2, 96, 96, &format, 0, &owner));
            uint size = 0;
            Assert.AreEqual(0, getBack(owner, &bitmap, &size));
            Assert.AreEqual(expectedSize, size);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, void*, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, null, 2, &bitmapLock));
            byte* data = null;
            uint stride = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)bitmapLock)[4])(bitmapLock, &stride));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &data));
            data[0] = 0x12; data[stride] = 0x34;
            Release(bitmapLock); bitmapLock = 0;
            byte* copied = stackalloc byte[32];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, void*, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, null, rowBytes, rowBytes * 2, copied));
            Assert.AreEqual((0x12, 0x34), ((int)copied[0], (int)copied[rowBytes]));
        }
        finally
        {
            Release(bitmapLock); Release(bitmap); Release(owner);
            if (apartment >= 0) CoUninitialize();
        }
    }

    [DllImport("ole32.dll")]
    private static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32.dll")]
    private static extern void CoUninitialize();

    [TestMethod]
    public void WhenCopyForwardIsSubmittedThenDuplicatedCompletionEventIsSignaled()
    {
        string path = typeof(DoubleBufferedBitmapBehaviorTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "AotDllPath").Value!;
        nint module = NativeLibrary.Load(path);
        var create = (delegate* unmanaged[Stdcall]<uint, uint, double, double, Guid*, nint, nint*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapCreate");
        var createConnection = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var createChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var createResource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        nint owner = 0, connection = 0, channel = 0, duplicate = 0;
        using var completed = new EventWaitHandle(false, EventResetMode.ManualReset);
        Guid format = new("6fddc324-4e03-4bfe-b185-3d77768dc910");
        try
        {
            Assert.AreEqual(0, create(2, 2, 96, 96, &format, 0, &owner));
            Assert.AreEqual(0, createConnection(1, &connection));
            Assert.AreEqual(0, createChannel(connection, 0, &channel));
            uint resource = 0;
            Assert.AreEqual(0, createResource(channel, 96, &resource));
            var getBack = (delegate* unmanaged[Stdcall]<nint, nint*, uint*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapGetBackBuffer");
            var addDirty = (delegate* unmanaged[Stdcall]<nint, void*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapAddDirtyRect");
            var protect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapProtectBackBuffer");
            nint back = 0, bitmapLock = 0;
            try
            {
                Assert.AreEqual(0, getBack(owner, &back, null));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, void*, uint, nint*, int>)(*(void***)back)[8])(back, null, 2, &bitmapLock));
                uint size = 0;
                byte* data = null;
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &data));
                for (int i = 0; i < 16; i++) data[i] = (byte)(i * 7);
                Release(bitmapLock);
                bitmapLock = 0;
                int* rectangle = stackalloc int[] { 0, 0, 2, 2 };
                Assert.AreEqual(0, addDirty(owner, rectangle));
                Assert.AreEqual(0, protect(owner));
            }
            finally { Release(bitmapLock); Release(back); }
            var update = new UpdatePacket { Type = 0x3B, Handle = resource, Bitmap = (ulong)owner };
            Assert.AreEqual(0, send(&update, (uint)sizeof(UpdatePacket), 0, channel));
            owner = 0;
            Assert.IsTrue(DuplicateHandle((nint)(-1), completed.SafeWaitHandle.DangerousGetHandle(), (nint)(-1), out duplicate, 0, false, 2));
            var copy = new CopyPacket { Type = 0x3C, Handle = resource, Event = (ulong)duplicate };
            Assert.AreEqual(0, send(&copy, (uint)sizeof(CopyPacket), 0, channel));
            duplicate = 0;
            Assert.AreEqual(0, close(channel));
            Assert.AreEqual(0, commit(channel));
            Assert.IsTrue(completed.WaitOne(0));
        }
        finally
        {
            if (duplicate != 0) CloseHandle(duplicate);
            if (channel != 0) destroy(channel);
            if (connection != 0) disconnect(connection);
            Release(owner);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct UpdatePacket { internal uint Type, Handle; internal ulong Bitmap; internal int UseBackBuffer; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct CopyPacket { internal uint Type, Handle; internal ulong Event; }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DuplicateHandle(nint sourceProcess, nint source, nint targetProcess, out nint target, uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint options);
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(nint handle);

    private static void Release(nint instance)
    {
        if (instance != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance);
    }
}
