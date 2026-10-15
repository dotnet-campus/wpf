using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionTests
{
    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    [DataRow(17)]
    [DataRow(18)]
    [DataRow(19)]
    [DataRow(20)]
    [DataRow(21)]
    [DataRow(22)]
    [DataRow(23)]
    [DataRow(24)]
    [DataRow(25)]
    [DataRow(26)]
    [DataRow(27)]
    [DataRow(28)]
    [DataRow(16, true)]
    public void WhenAuthorizedDoubleBufferedFormatIsCopiedAndReplacedThenVisualConsumesRealFrontBuffer(int code, bool failCopyWithLockedBackBuffer = false)
    {
        string path = AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        var create = (delegate* unmanaged[Stdcall]<uint, uint, double, double, Guid*, nint, nint*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapCreate");
        var getBack = (delegate* unmanaged[Stdcall]<nint, nint*, uint*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapGetBackBuffer");
        var dirty = (delegate* unmanaged[Stdcall]<nint, int*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapAddDirtyRect");
        var protect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapProtectBackBuffer");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var factoryCreate = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
        var targetCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateSWRenderTargetForBitmap");
        using var apartment = new SystemWicBitmap();
        nint connection = 0, channel = 0, factory = 0, target = 0, output = 0, owner = 0, back = 0, palette = 0, wicFactory = 0, converter = 0, heldLock = 0;
        try
        {
            Guid clsid = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
            Guid iid = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
            Assert.AreEqual(0, FormatCoCreateInstance(&clsid, 0, 1, &iid, &wicFactory));
            if (code <= 4)
            {
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)wicFactory)[9])(wicFactory, &palette));
                uint* colors = stackalloc uint[] { 0xff203040, 0xffa08060, 0xff102030, 0xffe0c0a0 };
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, uint, int>)(*(void***)palette)[4])(palette, colors, 4));
            }
            Guid format = new($"6fddc324-4e03-4bfe-b185-3d77768dc9{code:x2}");
            Assert.AreEqual(0, create(2, 1, 96, 96, &format, palette, &owner));
            Assert.AreEqual(0, getBack(owner, &back, null));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)back)[8])(back, 0, 2, &heldLock));
            uint size = 0; byte* bytes = null;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)heldLock)[5])(heldLock, &size, &bytes));
            new Span<byte>(bytes, (int)size).Fill(0x20);
            if (code is 17 or 25 or 26 or 27)
                new Span<float>(bytes, (int)size / 4).Fill(0.5f);
            Release(heldLock); heldLock = 0;
            bool alpha = code is <= 4 or 15 or 16 or 22 or 23 or 25 or 26;
            Guid frontFormat = new(alpha ? "6fddc324-4e03-4bfe-b185-3d77768dc910" : "6fddc324-4e03-4bfe-b185-3d77768dc90e");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)wicFactory)[10])(wicFactory, &converter));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, Guid*, uint, nint, double, uint, int>)(*(void***)converter)[8])(converter, back, &frontFormat, 0, palette, 0, 0));
            var expected = Read(converter);
            if (!alpha) expected = (expected.Item1 | 0xff000000u, expected.Item2 | 0xff000000u);
            Assert.AreNotEqual((0u, 0u), expected);
            Assert.AreEqual(0, connect(1, &connection));
            Assert.AreEqual(0, channelCreate(connection, 0, &channel));
            Assert.AreEqual(0, factoryCreate(&factory, 0x200184C0));
            output = apartment.Create(2, 1);
            Assert.AreEqual(0, targetCreate(factory, output, &target));
            uint image = 0, data = 0, visual = 0, destination = 0;
            Assert.AreEqual(0, resource(channel, 96, &image));
            Assert.AreEqual(0, resource(channel, 43, &data));
            Assert.AreEqual(0, resource(channel, 39, &visual));
            Assert.AreEqual(0, resource(channel, 47, &destination));
            TargetPacket targetPacket = new() { Type = 0x34, Handle = destination, Target = (ulong)target, Width = 2, Height = 1 };
            Assert.AreEqual(0, send(&targetPacket, (uint)sizeof(TargetPacket), 0, channel));
            ImagePacket update = new() { Type = 0x3b, Handle = image, Bitmap = (ulong)owner };
            Assert.AreEqual(0, send(&update, (uint)sizeof(ImagePacket), 0, channel));
            owner = 0;
            DrawPacket draw = new() { Type = 0x18, Handle = data, DataSize = 48, RecordSize = 48, Command = 0x47, Width = 2, Height = 1, Image = image };
            Assert.AreEqual(0, send(&draw, (uint)sizeof(DrawPacket), 0, channel));
            uint* binding = stackalloc uint[] { 0x22, visual, data };
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            binding[0] = 0x35; binding[1] = destination; binding[2] = visual;
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual(alpha ? (0u, 0u) : (0xff000000u, 0xff000000u), Read(output));
            int* rect = stackalloc int[] { 0, 0, 2, 1 };
            Assert.AreEqual(0, dirty((nint)update.Bitmap, rect));
            Assert.AreEqual(0, protect((nint)update.Bitmap));
            CopyPacket copy = new() { Type = 0x3c, Handle = image };
            if (failCopyWithLockedBackBuffer)
            {
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)back)[8])(back, 0, 2, &heldLock));
                using var completed = new EventWaitHandle(false, EventResetMode.ManualReset);
                Assert.IsTrue(FormatDuplicateHandle(-1, completed.SafeWaitHandle.DangerousGetHandle(), -1, out nint completionHandle, 0, false, 2));
                copy.Event = (ulong)completionHandle;
                Assert.AreEqual(0, send(&copy, (uint)sizeof(CopyPacket), 0, channel));
                Assert.AreEqual(0, close(channel));
                Assert.AreEqual(unchecked((int)0x88982f0d), commit(channel));
                Assert.IsTrue(completed.WaitOne(0));
                Assert.IsFalse(FormatGetHandleInformation(completionHandle, out _));
                Assert.AreEqual(6, Marshal.GetLastWin32Error());
                Release(heldLock); heldLock = 0;
                Assert.AreEqual(unchecked((int)0x88982f0d), present(connection));
                Assert.AreEqual((0u, 0u), Read(output));
                Assert.AreEqual(0, create(2, 1, 96, 96, &format, palette, &owner));
                Release(back); back = 0;
                Assert.AreEqual(0, getBack(owner, &back, null));
                update.Bitmap = (ulong)owner;
                Assert.AreEqual(0, send(&update, (uint)sizeof(ImagePacket), 0, channel)); owner = 0;
                completed.Reset();
                Assert.IsTrue(FormatDuplicateHandle(-1, completed.SafeWaitHandle.DangerousGetHandle(), -1, out completionHandle, 0, false, 2));
                copy.Event = (ulong)completionHandle;
                Assert.AreEqual(0, send(&copy, (uint)sizeof(CopyPacket), 0, channel));
                Assert.AreEqual(0, close(channel));
                Assert.AreEqual(unchecked((int)0x88982f0d), commit(channel));
                Assert.IsTrue(completed.WaitOne(0));
                Assert.IsFalse(FormatGetHandleInformation(completionHandle, out _));
                Assert.AreEqual(6, Marshal.GetLastWin32Error());
                Assert.AreEqual(0, destroy(channel)); channel = 0;
                Assert.AreEqual(0, disconnect(connection)); connection = 0;
                Write(back, 0xff123456, 0xffabcdef);
                Assert.AreEqual((0xff123456u, 0xffabcdefu), Read(back));
                return;
            }
            Assert.AreEqual(0, send(&copy, (uint)sizeof(CopyPacket), 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual(expected, Read(output));
            Release(converter); converter = 0;
            format = new("6fddc324-4e03-4bfe-b185-3d77768dc910");
            Assert.AreEqual(0, create(2, 1, 96, 96, &format, 0, &owner));
            Release(back); back = 0;
            Assert.AreEqual(0, getBack(owner, &back, null));
            Write(back, 0xff123456, 0xffabcdef);
            Assert.AreEqual(0, dirty(owner, rect)); Assert.AreEqual(0, protect(owner));
            update.Bitmap = (ulong)owner;
            Assert.AreEqual(0, send(&update, (uint)sizeof(ImagePacket), 0, channel)); owner = 0;
            Assert.AreEqual(0, send(&copy, (uint)sizeof(CopyPacket), 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)output)[8])(output, 0, 1, &heldLock));
            Assert.AreEqual(unchecked((int)0x88982f0d), present(connection));
            Release(heldLock); heldLock = 0;
            Write(output, 0, 0);
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual((0xff123456u, 0xffabcdefu), Read(output));
            Assert.AreEqual(0, destroy(channel)); channel = 0;
            Assert.AreEqual(0, disconnect(connection)); connection = 0;
            Assert.AreEqual((0xff123456u, 0xffabcdefu), Read(back));
        }
        finally
        {
            Release(heldLock);
            if (channel != 0) destroy(channel);
            if (connection != 0) disconnect(connection);
            Release(converter); Release(back); Release(owner); Release(target); Release(output); Release(factory); Release(palette); Release(wicFactory);
        }
    }

    [DllImport("kernel32.dll", EntryPoint = "DuplicateHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FormatDuplicateHandle(nint sourceProcess, nint source, nint targetProcess, out nint target, uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint options);

    [DllImport("kernel32.dll", EntryPoint = "GetHandleInformation", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FormatGetHandleInformation(nint handle, out uint flags);

    [DllImport("ole32.dll", EntryPoint = "CoCreateInstance")]
    private static extern int FormatCoCreateInstance(Guid* clsid, nint outer, uint context, Guid* iid, nint* instance);
}
