using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionTests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    public void WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(int action)
    {
        string path = typeof(CompositionTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "AotDllPath").Value!;
        nint module = NativeLibrary.Load(path);
        var createConnection = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var createChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var destroyChannel = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var createResource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var releaseResource = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)NativeLibrary.GetExport(module, "MilResource_ReleaseOnChannel");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
        var factoryCreate = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
        var targetCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateSWRenderTargetForBitmap");
        var wrap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateCWICWrapperBitmap");
        using var wic = new SystemWicBitmap();
        nint connection = 0, channel = 0, factory = 0, target = 0, output = 0, stream = 0, frame = 0, wrapper = 0;
        nint secondTarget = 0, secondOutput = 0, secondLock = 0;
        bool armed = false;
        int calls = 0, nestedResult = 0, streamDisposals = 0;
        nint callbackConnection = 0, callbackChannel = 0, replacement = 0;
        uint callbackImage = 0, callbackTarget = 0, callbackVisual = 0;
        try
        {
            Assert.AreEqual(0, createConnection(1, &connection));
            callbackConnection = connection;
            Assert.AreEqual(0, createChannel(connection, 0, &channel));
            callbackChannel = channel;
            Assert.AreEqual(0, factoryCreate(&factory, 0x200184C0));
            output = wic.Create(2, 1);
            Assert.AreEqual(0, targetCreate(factory, output, &target));
            byte[] bmp = new byte[62];
            bmp[0] = (byte)'B'; bmp[1] = (byte)'M';
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(2), bmp.Length);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(10), 54);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(14), 40);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(18), 2);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(22), 1);
            bmp[26] = 1; bmp[28] = 24;
            bmp[56] = 255; bmp[58] = 255;
            stream = ManagedStreamTests.CreateCallbackStream(module, bmp, () =>
            {
                if (!armed) return;
                armed = false;
                calls++;
                switch (action)
                {
                    case 0: nestedResult = present(callbackConnection); break;
                    case 1: nestedResult = destroyChannel(callbackChannel); break;
                    case 2: nestedResult = disconnect(callbackConnection); break;
                    case 7:
                        uint* options = stackalloc uint[] { 0x21, callbackVisual, 1, 0, 0, 3, 0, 0, 0 };
                        nestedResult = send(options, 36, 0, callbackChannel);
                        if (nestedResult >= 0) nestedResult = close(callbackChannel);
                        if (nestedResult >= 0) nestedResult = commit(callbackChannel);
                        break;
                    case 4:
                        int deleted = 0;
                        nestedResult = releaseResource(callbackChannel, callbackImage, &deleted);
                        if (nestedResult >= 0) nestedResult = releaseResource(callbackChannel, callbackTarget, &deleted);
                        if (nestedResult >= 0) nestedResult = close(callbackChannel);
                        if (nestedResult >= 0) nestedResult = commit(callbackChannel);
                        break;
                    case 3:
                    case 5:
                    case 6:
                        BitmapSourcePacket update = new() { Type = 0x0c, Handle = callbackImage, Bitmap = replacement };
                        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)replacement)[1])(replacement);
                        nestedResult = send(&update, (uint)sizeof(BitmapSourcePacket), 0, callbackChannel);
                        if (nestedResult >= 0) nestedResult = close(callbackChannel);
                        if (nestedResult >= 0) nestedResult = commit(callbackChannel);
                        break;
                }
            }, () => streamDisposals++);
            frame = wic.DecodeFrame(stream);
            Assert.AreEqual(0, wrap(frame, &wrapper));
            uint image = 0, data = 0, visual = 0, destination = 0;
            Assert.AreEqual(0, createResource(channel, 95, &image));
            callbackImage = image;
            nint replacementBitmap = wic.Create(2, 1), replacementWrapper = 0;
            try
            {
                Write(replacementBitmap, 0xff0000ff, 0xffffffff);
                Assert.AreEqual(0, wrap(replacementBitmap, &replacementWrapper));
                replacement = replacementWrapper;
            }
            finally { Release(replacementBitmap); }
            Assert.AreEqual(0, createResource(channel, 43, &data));
            Assert.AreEqual(0, createResource(channel, 39, &visual));
            Assert.AreEqual(0, createResource(channel, 47, &destination));
            callbackTarget = destination;
            callbackVisual = visual;
            BitmapSourcePacket source = new() { Type = 0x0c, Handle = image, Bitmap = wrapper };
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)wrapper)[1])(wrapper);
            Assert.AreEqual(0, send(&source, (uint)sizeof(BitmapSourcePacket), 0, channel));
            DrawPacket draw = new() { Type = 0x18, Handle = data, DataSize = 48, RecordSize = 48, Command = 0x47, Width = 2, Height = 1, Image = image };
            Assert.AreEqual(0, send(&draw, (uint)sizeof(DrawPacket), 0, channel));
            uint* binding = stackalloc uint[] { 0x22, visual, data };
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            TargetPacket targetPacket = new() { Type = 0x34, Handle = destination, Target = (ulong)target, Width = 2, Height = 1 };
            Assert.AreEqual(0, send(&targetPacket, (uint)sizeof(TargetPacket), 0, channel));
            binding[0] = 0x35; binding[1] = destination; binding[2] = visual;
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            if (action == 7)
            {
                draw.X = -1; draw.Width = 4;
                byte* draws = stackalloc byte[108];
                *(uint*)draws = 0x18; *(uint*)(draws + 4) = data; *(uint*)(draws + 8) = 96;
                new ReadOnlySpan<byte>((byte*)&draw + 12, 48).CopyTo(new Span<byte>(draws + 12, 48));
                new ReadOnlySpan<byte>((byte*)&draw + 12, 48).CopyTo(new Span<byte>(draws + 60, 48));
                Assert.AreEqual(0, send(draws, 108, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            }
            if (action is 5 or 6)
            {
                secondOutput = wic.Create(2, 1);
                Assert.AreEqual(0, targetCreate(factory, secondOutput, &secondTarget));
                uint secondDestination = 0;
                Assert.AreEqual(0, createResource(channel, 47, &secondDestination));
                targetPacket.Handle = secondDestination; targetPacket.Target = (ulong)secondTarget;
                Assert.AreEqual(0, send(&targetPacket, (uint)sizeof(TargetPacket), 0, channel));
                binding[1] = secondDestination;
                Assert.AreEqual(0, send(binding, 12, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Write(secondOutput, 0, 0);
                if (action == 6)
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)secondOutput)[8])(secondOutput, 0, 2, &secondLock));
            }
            Release(wrapper); wrapper = 0; Release(frame); frame = 0; Release(stream); stream = 0;
            armed = true;
            Assert.AreEqual(action == 6 ? unchecked((int)0x88982F0D) : 0, present(connection));
            Assert.AreEqual((1, action == 0 ? unchecked((int)0x8000FFFF) : 0, action == 7 ? (0xffbf4000u, 0xff40bf00u) : (0xffff0000u, 0xff00ff00u)), (calls, nestedResult, Read(output)));
            if (action == 6)
            {
                Release(secondLock); secondLock = 0;
                Assert.AreEqual((0u, 0u), Read(secondOutput));
                Assert.AreEqual(0, present(connection));
            }
            if (action is 5 or 6)
                Assert.AreEqual((0xff0000ffu, 0xffffffffu), Read(secondOutput));
            if (action == 1) channel = 0;
            if (action == 2) connection = 0;
            if (action == 4)
            {
                Write(output, 0, 0);
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual((0u, 0u), Read(output));
            }
            if (action == 7)
            {
                Write(output, 0, 0);
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual((0xffff0000u, 0xff00ff00u), Read(output));
            }
            if (action == 3)
            {
                Write(output, 0, 0);
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual((0xff0000ffu, 0xffffffffu), Read(output));
            }
        }
        finally
        {
            armed = false;
            Release(secondLock); Release(secondTarget); Release(secondOutput);
            if (channel != 0) destroyChannel(channel);
            if (connection != 0) disconnect(connection);
            Release(replacement); Release(wrapper); Release(frame); Release(stream); Release(target); Release(output); Release(factory);
        }
        Assert.AreEqual(1, streamDisposals);
    }
}
