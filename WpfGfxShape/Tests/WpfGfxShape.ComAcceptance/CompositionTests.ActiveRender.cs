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
    [DataRow(0, true)]
    [DataRow(1, true)]
    [DataRow(2, true)]
    [DataRow(3, true)]
    [DataRow(4, true)]
    [DataRow(7, true)]
    [DataRow(8)]
    [DataRow(8, true)]
    [DataRow(0, false, 1)]
    [DataRow(1, false, 1)]
    [DataRow(2, false, 1)]
    [DataRow(3, false, 1)]
    [DataRow(4, false, 1)]
    [DataRow(6, false, 1)]
    [DataRow(8, false, 1)]
    [DataRow(0, false, 2)]
    [DataRow(1, false, 2)]
    [DataRow(2, false, 2)]
    [DataRow(3, false, 2)]
    [DataRow(4, false, 2)]
    [DataRow(6, false, 2)]
    [DataRow(8, false, 2)]
    [DataRow(0, true, 1)]
    [DataRow(1, true, 1)]
    [DataRow(2, true, 1)]
    [DataRow(3, true, 1)]
    [DataRow(4, true, 1)]
    [DataRow(6, true, 1)]
    [DataRow(8, true, 1)]
    [DataRow(0, true, 2)]
    [DataRow(1, true, 2)]
    [DataRow(2, true, 2)]
    [DataRow(3, true, 2)]
    [DataRow(4, true, 2)]
    [DataRow(6, true, 2)]
    [DataRow(8, true, 2)]
    [DataRow(0, false, 3)]
    [DataRow(1, false, 3)]
    [DataRow(2, false, 3)]
    [DataRow(3, false, 3)]
    [DataRow(4, false, 3)]
    [DataRow(6, false, 3)]
    [DataRow(8, false, 3)]
    [DataRow(0, true, 3)]
    [DataRow(1, true, 3)]
    [DataRow(2, true, 3)]
    [DataRow(3, true, 3)]
    [DataRow(4, true, 3)]
    [DataRow(6, true, 3)]
    [DataRow(8, true, 3)]
    [DataRow(0, false, 1, true)]
    [DataRow(0, false, 2, true)]
    [DataRow(0, false, 3, true)]
    [DataRow(0, false, 3, false, true)]
    [DataRow(8, false, 3, false, true)]
    [DataRow(0, true, 3, false, true)]
    [DataRow(8, true, 3, false, true)]
    [DataRow(0, false, 3, false, false, true)]
    [DataRow(8, false, 3, false, false, true)]
    [DataRow(0, true, 3, false, false, true)]
    [DataRow(8, true, 3, false, false, true)]
    [DataRow(9, false, 3)]
    [DataRow(9, true, 3)]
    [DataRow(9, false, 3, false, false, true)]
    [DataRow(9, true, 3, false, false, true)]
    [DataRow(3, false, 3, false, false, true, true)]
    [DataRow(8, false, 3, false, false, true, true)]
    [DataRow(9, false, 3, false, false, true, true)]
    [DataRow(0, false, 3, true, false, true, true)]
    [DataRow(0, false, 3, false, true, false, true)]
    [DataRow(3, true, 3, false, false, true, true)]
    [DataRow(8, true, 3, false, false, true, true)]
    [DataRow(9, true, 3, false, false, true, true)]
    [DataRow(0, false, 1, false, false, false, true, true)]
    [DataRow(3, false, 1, false, false, false, true, true)]
    [DataRow(8, false, 1, false, false, false, true, true)]
    [DataRow(9, false, 1, false, false, false, true, true)]
    [DataRow(0, true, 1, false, false, false, true, true)]
    [DataRow(8, true, 1, false, false, false, true, true)]
    [DataRow(9, true, 1, false, false, false, true, true)]
    [DataRow(0, false, 1, true, false, false, true, true)]
    [DataRow(0, false, 1, false, false, false, true, true, true)]
    [DataRow(3, false, 1, false, false, false, true, true, true)]
    [DataRow(8, false, 1, false, false, false, true, true, true)]
    [DataRow(9, false, 1, false, false, false, true, true, true)]
    [DataRow(0, true, 1, false, false, false, true, true, true)]
    [DataRow(3, true, 1, false, false, false, true, true, true)]
    [DataRow(8, true, 1, false, false, false, true, true, true)]
    [DataRow(9, true, 1, false, false, false, true, true, true)]
    [DataRow(0, false, 1, false, false, false, true, true, false, true)]
    [DataRow(3, false, 1, false, false, false, true, true, false, true)]
    [DataRow(8, false, 1, false, false, false, true, true, false, true)]
    [DataRow(9, false, 1, false, false, false, true, true, false, true)]
    [DataRow(0, true, 1, false, false, false, true, true, false, true)]
    [DataRow(3, true, 1, false, false, false, true, true, false, true)]
    [DataRow(8, true, 1, false, false, false, true, true, false, true)]
    [DataRow(9, true, 1, false, false, false, true, true, false, true)]
    [DataRow(0, false, 3, false, false, true, true, false, false, true)]
    [DataRow(3, false, 3, false, false, true, true, false, false, true)]
    [DataRow(8, false, 3, false, false, true, true, false, false, true)]
    [DataRow(9, false, 3, false, false, true, true, false, false, true)]
    [DataRow(0, true, 3, false, false, true, true, false, false, true)]
    [DataRow(3, true, 3, false, false, true, true, false, false, true)]
    [DataRow(8, true, 3, false, false, true, true, false, false, true)]
    [DataRow(9, true, 3, false, false, true, true, false, false, true)]
    [DataRow(0, false, 3, true, false, true, true, false, false, true)]
    [DataRow(0, false, 3, false, true, false, true, false, false, true)]
    [DataRow(0, false, 3, false, false, true, false, false, false, false, true)]
    [DataRow(3, false, 3, false, false, true, false, false, false, false, true)]
    [DataRow(8, false, 3, false, false, true, false, false, false, false, true)]
    [DataRow(9, false, 3, false, false, true, false, false, false, false, true)]
    [DataRow(0, true, 3, false, false, true, false, false, false, false, true)]
    [DataRow(3, true, 3, false, false, true, false, false, false, false, true)]
    [DataRow(8, true, 3, false, false, true, false, false, false, false, true)]
    [DataRow(9, true, 3, false, false, true, false, false, false, false, true)]
    [DataRow(0, false, 3, true, false, true, false, false, false, false, true)]
    [DataRow(0, false, 3, false, true, false, false, false, false, false, true)]
    [DataRow(0, false, 3, false, false, true, false, false, false, false, false, true)]
    [DataRow(3, false, 3, false, false, true, false, false, false, false, false, true)]
    [DataRow(8, false, 3, false, false, true, false, false, false, false, false, true)]
    [DataRow(9, false, 3, false, false, true, false, false, false, false, false, true)]
    [DataRow(0, true, 3, false, false, true, false, false, false, false, false, true)]
    [DataRow(3, true, 3, false, false, true, false, false, false, false, false, true)]
    [DataRow(8, true, 3, false, false, true, false, false, false, false, false, true)]
    [DataRow(9, true, 3, false, false, true, false, false, false, false, false, true)]
    [DataRow(0, false, 3, true, false, true, false, false, false, false, false, true)]
    [DataRow(0, false, 3, false, true, false, false, false, false, false, false, true)]
    [DataRow(0, false, 3, false, false, true, false, false, false, false, true, false, true)]
    [DataRow(3, false, 3, false, false, true, false, false, false, false, true, false, true)]
    [DataRow(8, false, 3, false, false, true, false, false, false, false, true, false, true)]
    [DataRow(9, false, 3, false, false, true, false, false, false, false, true, false, true)]
    [DataRow(0, true, 3, false, false, true, false, false, false, false, true, false, true)]
    [DataRow(3, true, 3, false, false, true, false, false, false, false, true, false, true)]
    [DataRow(8, true, 3, false, false, true, false, false, false, false, true, false, true)]
    [DataRow(9, true, 3, false, false, true, false, false, false, false, true, false, true)]
    [DataRow(0, false, 3, true, false, true, false, false, false, false, true, false, true)]
    [DataRow(0, false, 3, false, true, false, false, false, false, false, true, false, true)]
    [DataRow(0, false, 3, false, false, true, false, false, false, false, false, false, false, 0)]
    [DataRow(3, false, 3, false, false, true, false, false, false, false, false, false, false, 1)]
    [DataRow(8, true, 3, false, false, true, false, false, false, false, false, false, false, 2)]
    [DataRow(9, true, 3, false, false, true, false, false, false, false, false, false, false, 3)]
    [DataRow(0, false, 3, true, false, true, false, false, false, false, false, false, false, 0)]
    [DataRow(0, false, 3, false, true, false, false, false, false, false, false, false, false, 1)]
    [DataRow(0, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true)]
    [DataRow(3, true, 3, false, false, true, true, false, false, false, false, false, false, -1, true)]
    [DataRow(8, true, 3, false, false, true, true, false, false, false, false, false, false, -1, true)]
    [DataRow(9, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true)]
    [DataRow(0, false, 3, true, false, true, true, false, false, false, false, false, false, -1, true)]
    [DataRow(0, false, 3, false, true, false, true, false, false, false, false, false, false, -1, true)]
    [DataRow(0, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 1)]
    [DataRow(3, true, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 1)]
    [DataRow(8, true, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 1)]
    [DataRow(9, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 1)]
    [DataRow(0, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 2)]
    [DataRow(3, true, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 2)]
    [DataRow(8, true, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 2)]
    [DataRow(9, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 2)]
    [DataRow(0, false, 3, false, false, true, true, false, false, false, false, false, true, -1, true, 3)]
    [DataRow(3, true, 3, false, false, true, true, false, false, false, false, false, true, -1, true, 3)]
    [DataRow(8, true, 3, false, false, true, true, false, false, false, false, false, true, -1, true, 3)]
    [DataRow(9, false, 3, false, false, true, true, false, false, false, false, false, true, -1, true, 3)]
    [DataRow(0, false, 3, true, false, true, true, false, false, false, false, false, false, -1, true, 3)]
    [DataRow(0, false, 3, false, true, false, true, false, false, false, false, false, false, -1, true, 1)]
    [DataRow(0, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 1, 1)]
    [DataRow(3, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 2, 1)]
    [DataRow(8, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 3, 1)]
    [DataRow(9, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 3, 1)]
    [DataRow(0, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 1, 2)]
    [DataRow(3, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 2, 2)]
    [DataRow(8, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 3, 2)]
    [DataRow(9, false, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 3, 2)]
    [DataRow(0, true, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 1, 1)]
    [DataRow(3, true, 3, false, false, true, true, false, false, false, false, false, true, -1, true, 2, 1)]
    [DataRow(8, true, 3, false, false, true, true, false, false, false, false, false, true, -1, true, 3, 1)]
    [DataRow(9, true, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 3, 1)]
    [DataRow(0, false, 3, true, false, true, true, false, false, false, false, false, false, -1, true, 3, 1)]
    [DataRow(0, false, 3, false, true, false, true, false, false, false, false, false, true, -1, true, 1, 1)]
    [DataRow(0, true, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 1, 2)]
    [DataRow(3, true, 3, false, false, true, true, false, false, false, false, false, true, -1, true, 2, 2)]
    [DataRow(8, true, 3, false, false, true, true, false, false, false, false, false, true, -1, true, 3, 2)]
    [DataRow(9, true, 3, false, false, true, true, false, false, false, false, false, false, -1, true, 3, 2)]
    [DataRow(0, false, 3, true, false, true, true, false, false, false, false, false, false, -1, true, 3, 2)]
    [DataRow(0, false, 3, false, true, false, true, false, false, false, false, false, true, -1, true, 1, 2)]
    [DataRow(0, false, 3, false, false, true, false, false, false, false, false, false, false, -1, false, 0, 0, true)]
    [DataRow(3, false, 3, false, false, true, false, false, false, false, false, false, false, -1, false, 0, 0, true)]
    [DataRow(8, false, 3, false, false, true, false, false, false, false, false, false, false, -1, false, 0, 0, true)]
    [DataRow(9, false, 3, false, false, true, false, false, false, false, false, false, false, -1, false, 0, 0, true)]
    [DataRow(0, false, 3, true, false, true, false, false, false, false, false, false, false, -1, false, 0, 0, true)]
    [DataRow(0, false, 3, false, true, false, false, false, false, false, false, false, false, -1, false, 0, 0, true)]
    [DataRow(0, false, 3, false, false, true, false, false, false, false, false, false, false, -1, false, 0, 0, false, true)]
    [DataRow(3, false, 3, false, false, true, false, false, false, false, false, false, false, -1, false, 0, 0, false, true)]
    [DataRow(8, false, 3, false, false, true, false, false, false, false, false, false, false, -1, false, 0, 0, false, true)]
    [DataRow(9, false, 3, false, false, true, false, false, false, false, false, false, false, -1, false, 0, 0, false, true)]
    [DataRow(0, false, 3, true, false, true, false, false, false, false, false, false, false, -1, false, 0, 0, false, true)]
    [DataRow(0, false, 3, false, true, false, false, false, false, false, false, false, false, -1, false, 0, 0, false, true)]
    public void WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(int action, bool imageMask = false, int geometryKind = 0, bool outsideTarget = false, bool failFill = false, bool solidFill = false, bool pathGeometry = false, bool widePath = false, bool reversePath = false, bool gapPath = false, bool ellipseGeometry = false, bool roundedGeometry = false, bool groupGeometry = false, int combinedMode = -1, bool curvedPath = false, int curveMode = 0, int deviceMode = 0, bool drawingImage = false, bool drawingBrushSource = false)
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
        nint secondTarget = 0, secondOutput = 0, secondLock = 0, fillBitmap = 0, fillLock = 0;
        bool armed = false;
        int calls = 0, nestedResult = 0, streamDisposals = 0;
        nint callbackConnection = 0, callbackChannel = 0, replacement = 0;
        nint callbackRenderTarget = 0, callbackFactory = 0, callbackInternalTarget = 0;
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
                    case 9:
                        nestedResult = ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, nint, nint, int>)
                            (*(void***)callbackInternalTarget)[10])(callbackInternalTarget, 0, 0, 0, 0, 0, 0);
                        break;
                    case 1: nestedResult = destroyChannel(callbackChannel); break;
                    case 2: nestedResult = disconnect(callbackConnection); break;
                    case 8:
                        Release(callbackRenderTarget); callbackRenderTarget = 0;
                        Release(callbackFactory); callbackFactory = 0;
                        nestedResult = destroyChannel(callbackChannel);
                        if (nestedResult >= 0) nestedResult = disconnect(callbackConnection);
                        break;
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
                Write(replacementBitmap, imageMask ? 0u : 0xff0000ffu, 0xffffffff);
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
            uint opacityMaskBrush = 0;
            if (imageMask)
            {
                uint brush = 0, contentImage = 0;
                Assert.AreEqual(0, createResource(channel, 80, &brush));
                Assert.AreEqual(0, createResource(channel, 95, &contentImage));
                nint contentBitmap = wic.Create(2, 1), contentWrapper = 0;
                try
                {
                    Write(contentBitmap, 0xffff0000, 0xff00ff00);
                    Assert.AreEqual(0, wrap(contentBitmap, &contentWrapper));
                    BitmapSourcePacket contentSource = new() { Type = 0x0c, Handle = contentImage, Bitmap = contentWrapper };
                    ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)contentWrapper)[1])(contentWrapper);
                    Assert.AreEqual(0, send(&contentSource, (uint)sizeof(BitmapSourcePacket), 0, channel));
                }
                finally { Release(contentWrapper); Release(contentBitmap); }
                byte* brushPacket = stackalloc byte[148];
                new Span<byte>(brushPacket, 148).Clear();
                *(uint*)brushPacket = 0x81; *(uint*)(brushPacket + 4) = brush;
                *(double*)(brushPacket + 8) = 1;
                *(double*)(brushPacket + 32) = 2; *(double*)(brushPacket + 40) = 1;
                *(double*)(brushPacket + 64) = 1; *(double*)(brushPacket + 72) = 1;
                *(uint*)(brushPacket + 112) = 1; *(uint*)(brushPacket + 124) = 1;
                *(uint*)(brushPacket + 144) = image;
                Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
                opacityMaskBrush = brush;
                binding[0] = 0x23; binding[2] = brush;
                Assert.AreEqual(0, send(binding, 12, 0, channel));
                draw.Image = contentImage;
                Assert.AreEqual(0, send(&draw, (uint)sizeof(DrawPacket), 0, channel));
            }
            if (geometryKind != 0)
            {
                uint brush = 0, geometry = 0, pen = 0;
                Assert.AreEqual(0, createResource(channel, 80, &brush));
                byte* brushPacket = stackalloc byte[148]; new Span<byte>(brushPacket, 148).Clear();
                *(uint*)brushPacket = 0x81; *(uint*)(brushPacket + 4) = brush;
                *(double*)(brushPacket + 8) = 1;
                *(double*)(brushPacket + 32) = 2; *(double*)(brushPacket + 40) = 1;
                *(double*)(brushPacket + 64) = 1; *(double*)(brushPacket + 72) = 1;
                *(uint*)(brushPacket + 112) = 1; *(uint*)(brushPacket + 124) = 1;
                *(uint*)(brushPacket + 144) = image;
                if (drawingBrushSource)
                {
                    uint sourceDrawing = 0, sourceImage = 0;
                    Assert.AreEqual(0, createResource(channel, 89, &sourceDrawing));
                    Assert.AreEqual(0, createResource(channel, 59, &sourceImage));
                    byte* sourcePacket = stackalloc byte[48]; new Span<byte>(sourcePacket, 48).Clear();
                    *(uint*)sourcePacket = 0x89; *(uint*)(sourcePacket + 4) = sourceDrawing;
                    *(double*)(sourcePacket + 24) = 2; *(double*)(sourcePacket + 32) = 1;
                    *(uint*)(sourcePacket + 40) = image;
                    Assert.AreEqual(0, send(sourcePacket, 48, 0, channel));
                    uint* drawingPacket = stackalloc uint[] { 0x71, sourceImage, sourceDrawing };
                    Assert.AreEqual(0, send(drawingPacket, 12, 0, channel));
                    *(uint*)(brushPacket + 144) = sourceImage;
                }
                uint inverseTransform = 0;
                if (deviceMode != 0)
                {
                    uint deviceTransform = 0;
                    Assert.AreEqual(0, createResource(channel, 66, &deviceTransform));
                    Assert.AreEqual(0, createResource(channel, 66, &inverseTransform));
                    byte* matrix = stackalloc byte[60]; new Span<byte>(matrix, 60).Clear();
                    *(uint*)matrix = 0x77; *(uint*)(matrix + 4) = deviceTransform;
                    *(double*)(matrix + 8) = deviceMode == 1 ? 2 : 0;
                    *(double*)(matrix + 16) = deviceMode == 1 ? 0 : 1;
                    *(double*)(matrix + 24) = deviceMode == 1 ? 0 : -1;
                    *(double*)(matrix + 32) = deviceMode == 1 ? 0.5 : 0;
                    Assert.AreEqual(0, send(matrix, 60, 0, channel));
                    uint* visualTransform = stackalloc uint[] { 0x1c, visual, deviceTransform };
                    Assert.AreEqual(0, send(visualTransform, 12, 0, channel));
                    *(uint*)(matrix + 4) = inverseTransform;
                    *(double*)(matrix + 8) = deviceMode == 1 ? 0.5 : 0;
                    *(double*)(matrix + 16) = deviceMode == 1 ? 0 : -1;
                    *(double*)(matrix + 24) = deviceMode == 1 ? 0 : 1;
                    *(double*)(matrix + 32) = deviceMode == 1 ? 2 : 0;
                    Assert.AreEqual(0, send(matrix, 60, 0, channel));
                    *(uint*)(brushPacket + 100) = inverseTransform;
                    if (opacityMaskBrush != 0)
                    {
                        *(uint*)(brushPacket + 4) = opacityMaskBrush;
                        Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
                        *(uint*)(brushPacket + 4) = brush;
                    }
                }
                Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
                Assert.AreEqual(0, createResource(channel, ellipseGeometry ? 70u : pathGeometry ? 73u : geometryKind is 1 or 3 ? 69u : 68u, &geometry));
                byte* geometryPacket = stackalloc byte[72]; new Span<byte>(geometryPacket, 72).Clear();
                *(uint*)geometryPacket = geometryKind is 1 or 3 ? 0x79u : 0x78u;
                *(uint*)(geometryPacket + 4) = geometry;
                if (geometryKind is 1 or 3)
                {
                    *(double*)(geometryPacket + 40) = 2; *(double*)(geometryPacket + 48) = 1;
                    if (roundedGeometry)
                    {
                        *(double*)(geometryPacket + 8) = 0.25; *(double*)(geometryPacket + 16) = 0.25;
                    }
                }
                else
                {
                    *(double*)(geometryPacket + 16) = 0.5;
                    *(double*)(geometryPacket + 24) = 2; *(double*)(geometryPacket + 32) = 0.5;
                }
                if (geometryKind != 1)
                {
                    Assert.AreEqual(0, createResource(channel, 85, &pen));
                    byte* penPacket = stackalloc byte[52]; new Span<byte>(penPacket, 52).Clear();
                    *(uint*)penPacket = 0x86; *(uint*)(penPacket + 4) = pen;
                    // Geometry transforms do not transform the pen; the Visual transform does.
                    *(double*)(penPacket + 8) = deviceMode == 1 ? 4 : geometryKind == 3 ? 2 : 1;
                    *(double*)(penPacket + 16) = 10;
                    *(uint*)(penPacket + 24) = brush;
                    Assert.AreEqual(0, send(penPacket, 52, 0, channel));
                }
                if (pathGeometry)
                {
                    // A real MIL_PATHGEOMETRY triangle traverses the source figure adapter.
                    byte* pathPacket = stackalloc byte[204]; new Span<byte>(pathPacket, 204).Clear();
                    *(uint*)pathPacket = 0x7d; *(uint*)(pathPacket + 4) = geometry;
                    *(uint*)(pathPacket + 12) = 1; *(uint*)(pathPacket + 16) = 184;
                    byte* figures = pathPacket + 20;
                    *(uint*)figures = 184; *(uint*)(figures + 40) = 1;
                    byte* figure = figures + 48;
                    *(uint*)(figure + 4) = 12; *(uint*)(figure + 8) = 3; *(uint*)(figure + 12) = 136;
                    *(uint*)(figure + 32) = 104;
                    *(double*)(figure + 16) = 0; *(double*)(figure + 24) = 0;
                    for (int segment = 0; segment < 3; segment++)
                    {
                        byte* line = figure + 40 + segment * 32;
                        *(uint*)line = 1;
                        *(uint*)(line + 4) = gapPath && (geometryKind == 1 || segment == 2) ? 4u : 0u;
                        *(uint*)(line + 8) = segment == 0 ? 0u : 32u;
                        *(double*)(line + 16) = segment == (reversePath ? 1 : 0) ? (widePath ? 4 : 2) : 0;
                        *(double*)(line + 24) = segment == (reversePath ? 0 : 1) ? (widePath ? 4 : 1) : 0;
                    }
                    Assert.AreEqual(0, send(pathPacket, 204, 0, channel));
                }
                else if (ellipseGeometry)
                {
                    new Span<byte>(geometryPacket, 72).Clear();
                    *(uint*)geometryPacket = 0x7a; *(uint*)(geometryPacket + 4) = geometry;
                    // Keep the curved edge outside the sampled pixels; this fixture verifies owner lifetime, not edge coverage.
                    *(double*)(geometryPacket + 8) = 2; *(double*)(geometryPacket + 16) = 1;
                    *(double*)(geometryPacket + 24) = 1; *(double*)(geometryPacket + 32) = 0.5;
                    Assert.AreEqual(0, send(geometryPacket, 56, 0, channel));
                }
                else Assert.AreEqual(0, send(geometryPacket, geometryKind is 1 or 3 ? 72u : 52u, 0, channel));
                if (curvedPath)
                {
                    byte* packet = stackalloc byte[172]; new Span<byte>(packet, 172).Clear();
                    *(uint*)packet = 0x7d; *(uint*)(packet + 4) = geometry;
                    *(uint*)(packet + 8) = inverseTransform;
                    *(uint*)(packet + 12) = 1; *(uint*)(packet + 16) = 152;
                    byte* figures = packet + 20;
                    *(uint*)figures = 152; *(uint*)(figures + 40) = 1;
                    byte* figure = figures + 48;
                    *(uint*)(figure + 4) = 12; *(uint*)(figure + 8) = 1; *(uint*)(figure + 12) = 104;
                    *(uint*)(figure + 32) = 40;
                    *(double*)(figure + 16) = 0; *(double*)(figure + 24) = 0.5;
                    byte* curve = figure + 40; *(uint*)curve = 2;
                    *(double*)(curve + 16) = 0.5; *(double*)(curve + 24) = 0.6;
                    *(double*)(curve + 32) = 1.5; *(double*)(curve + 40) = 0.6;
                    *(double*)(curve + 48) = 2; *(double*)(curve + 56) = 0.5;
                    if (curveMode is 1 or 3)
                    {
                        *(double*)(curve + 16) = 0; *(double*)(curve + 24) = 0.5;
                    }
                    if (curveMode is 2 or 3)
                    {
                        *(double*)(curve + 32) = 2; *(double*)(curve + 40) = 0.5;
                    }
                    Assert.AreEqual(0, send(packet, 172, 0, channel));
                }
                if (combinedMode >= 0)
                {
                    uint other = 0, combined = 0;
                    Assert.AreEqual(0, createResource(channel, 69, &other));
                    byte* packet = stackalloc byte[72]; new Span<byte>(packet, 72).Clear();
                    *(uint*)packet = 0x79; *(uint*)(packet + 4) = other;
                    *(double*)(packet + 24) = combinedMode == 1 ? -4 : 10;
                    *(double*)(packet + 32) = combinedMode == 1 ? -4 : 10;
                    *(double*)(packet + 40) = 12; *(double*)(packet + 48) = 12;
                    Assert.AreEqual(0, send(packet, 72, 0, channel));
                    Assert.AreEqual(0, createResource(channel, 72, &combined));
                    uint* combination = stackalloc uint[] { 0x7c, combined, 0, (uint)combinedMode, geometry, other };
                    Assert.AreEqual(0, send(combination, 24, 0, channel));
                    geometry = combined;
                }
                if (groupGeometry)
                {
                    uint group = 0;
                    Assert.AreEqual(0, createResource(channel, 71, &group));
                    byte* groupPacket = stackalloc byte[24]; new Span<byte>(groupPacket, 24).Clear();
                    *(uint*)groupPacket = 0x7b; *(uint*)(groupPacket + 4) = group;
                    *(uint*)(groupPacket + 12) = 1; *(uint*)(groupPacket + 16) = 4;
                    *(uint*)(groupPacket + 20) = geometry;
                    Assert.AreEqual(0, send(groupPacket, 24, 0, channel));
                    geometry = group;
                }
                uint fillBrush = geometryKind is 1 or 3 ? brush : 0;
                if (failFill)
                {
                    uint fillImage = 0;
                    nint fillWrapper = 0;
                    fillBitmap = wic.Create(2, 1);
                    Write(fillBitmap, 0xffff0000, 0xff00ff00);
                    Assert.AreEqual(0, createResource(channel, 95, &fillImage));
                    Assert.AreEqual(0, wrap(fillBitmap, &fillWrapper));
                    try
                    {
                        BitmapSourcePacket fillSource = new() { Type = 0x0c, Handle = fillImage, Bitmap = fillWrapper };
                        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)fillWrapper)[1])(fillWrapper);
                        Assert.AreEqual(0, send(&fillSource, (uint)sizeof(BitmapSourcePacket), 0, channel));
                    }
                    finally { Release(fillWrapper); }
                    fillBrush = 0;
                    Assert.AreEqual(0, createResource(channel, 80, &fillBrush));
                    *(uint*)(brushPacket + 4) = fillBrush; *(uint*)(brushPacket + 144) = fillImage;
                    Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
                }
                if (solidFill)
                {
                    fillBrush = 0;
                    Assert.AreEqual(0, createResource(channel, 75, &fillBrush));
                    byte* solidPacket = stackalloc byte[48]; new Span<byte>(solidPacket, 48).Clear();
                    *(uint*)solidPacket = 0x7e; *(uint*)(solidPacket + 4) = fillBrush;
                    *(double*)(solidPacket + 8) = 1;
                    *(float*)(solidPacket + 16) = 1; *(float*)(solidPacket + 20) = 1;
                    Assert.AreEqual(0, send(solidPacket, 48, 0, channel));
                }
                uint* records = stackalloc uint[] { 0x18, data, 24, 24, 0x46, fillBrush, pen, geometry, 0 };
                Assert.AreEqual(0, send(records, 36, 0, channel));
                if (drawingImage)
                {
                    uint drawing = 0, drawingSource = 0;
                    Assert.AreEqual(0, createResource(channel, 87, &drawing));
                    Assert.AreEqual(0, createResource(channel, 59, &drawingSource));
                    uint* drawingPacket = stackalloc uint[] { 0x87, drawing, fillBrush, pen, geometry };
                    Assert.AreEqual(0, send(drawingPacket, 20, 0, channel));
                    uint* imagePacket = stackalloc uint[] { 0x71, drawingSource, drawing };
                    Assert.AreEqual(0, send(imagePacket, 12, 0, channel));
                    DrawPacket drawingRecord = new() { Type = 0x18, Handle = data, DataSize = 48, RecordSize = 48,
                        Command = 0x47, X = -1, Y = -1, Width = 4, Height = 3, Image = drawingSource };
                    Assert.AreEqual(0, send(&drawingRecord, (uint)sizeof(DrawPacket), 0, channel));
                }
            }
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
            if (action == 8)
            {
                callbackRenderTarget = target; target = 0;
                callbackFactory = factory; factory = 0;
            }
            if (action == 9)
            {
                Guid internalId = new("b73b1159-a295-4c76-bb56-c18e282ae007");
                nint internalTarget = 0;
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])
                    (target, &internalId, &internalTarget));
                callbackInternalTarget = internalTarget;
            }
            if (failFill)
            {
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)fillBitmap)[8])(fillBitmap, 0, 2, &fillLock));
                armed = true;
                Assert.AreEqual(unchecked((int)0x88982F0D), present(connection));
                Assert.AreEqual((0, (0u, 0u)), (calls, Read(output)));
                Release(fillLock); fillLock = 0;
            }
            if (outsideTarget)
            {
                byte* offset = stackalloc byte[24]; new Span<byte>(offset, 24).Clear();
                *(uint*)offset = 0x1b; *(uint*)(offset + 4) = visual; *(double*)(offset + 8) = 100;
                Assert.AreEqual(0, send(offset, 24, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                armed = true;
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual((0, (0u, 0u)), (calls, Read(output)));
                *(double*)(offset + 8) = 0;
                Assert.AreEqual(0, send(offset, 24, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            }
            armed = true;
            Assert.AreEqual(action == 6 ? unchecked((int)0x88982F0D) : 0, present(connection));
            Assert.AreEqual((1, action == 9 ? unchecked((int)0x88980085) : action == 0 ? unchecked((int)0x8000FFFF) : 0, action == 7 ? (0xffbf4000u, 0xff40bf00u) : (0xffff0000u, 0xff00ff00u)), (calls, nestedResult, Read(output)));
            if (action == 6)
            {
                Release(secondLock); secondLock = 0;
                Assert.AreEqual((0u, 0u), Read(secondOutput));
                Assert.AreEqual(0, present(connection));
            }
            if (action is 5 or 6)
                Assert.AreEqual(imageMask ? (0u, geometryKind == 0 ? 0xff00ff00u : 0xffffffffu)
                    : (0xff0000ffu, 0xffffffffu), Read(secondOutput));
            if (action is 1 or 8) channel = 0;
            if (action is 2 or 8) connection = 0;
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
                Assert.AreEqual(imageMask ? (0u, geometryKind == 0 ? 0xff00ff00u : 0xffffffffu)
                    : (0xff0000ffu, 0xffffffffu), Read(output));
            }
        }
        finally
        {
            armed = false;
            Release(fillLock); Release(fillBitmap);
            Release(secondLock); Release(secondTarget); Release(secondOutput);
            if (channel != 0) destroyChannel(channel);
            if (connection != 0) disconnect(connection);
            Release(callbackInternalTarget); Release(callbackRenderTarget); Release(callbackFactory);
            Release(replacement); Release(wrapper); Release(frame); Release(stream); Release(target); Release(output); Release(factory);
        }
        Assert.AreEqual(1, streamDisposals);
    }
}
