using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    private static readonly uint[] TwoInputShaderTokens =
    [
        0xFFFF0200, 0x002AFFFE, 0x42415443, 0x0000001C,
        0x0000007B, 0xFFFF0200, 0x00000002, 0x0000001C,
        0x00001100, 0x00000074, 0x00000044, 0x00000003,
        0x00020001, 0x0000004C, 0x00000000, 0x0000005C,
        0x00010003, 0x00060001, 0x00000064, 0x00000000,
        0x75706E69, 0xAB003074, 0x000C0004, 0x00010001,
        0x00000001, 0x00000000, 0x75706E69, 0xAB003174,
        0x000C0004, 0x00010001, 0x00000001, 0x00000000,
        0x325F7370, 0x4D00305F, 0x6F726369, 0x74666F73,
        0x29522820, 0x534C4820, 0x6853204C, 0x72656461,
        0x6D6F4320, 0x656C6970, 0x30312072, 0xAB00312E,
        0x05000051, 0xA00F0000, 0x3F000000, 0x00000000,
        0x00000000, 0x00000000, 0x0200001F, 0x80000000,
        0xB0030000, 0x0200001F, 0x90000000, 0xA00F0800,
        0x0200001F, 0x90000000, 0xA00F0801, 0x03000042,
        0x800F0000, 0xB0E40000, 0xA0E40800, 0x03000042,
        0x800F0001, 0xB0E40000, 0xA0E40801, 0x04000004,
        0x800F0000, 0x80E40000, 0xA0000000, 0x80E40001,
        0x02000001, 0x800F0800, 0x80E40000, 0x0000FFFF
    ];
    [TestMethod]
    [DataRow(false, 0xff00ff00u, 0x0000ff80u)]
    [DataRow(true, 0xff0000ffu, 0x00ff0080u)]
    [DataRow(true, 0u, 0x00000080u)]
    public void WhenSecondSamplerUsesVisualOrCacheThenNonIdentityShaderCombinesBothInputs(bool cache, uint color, uint expected)
        => RunVisualCacheConsumer(cache, color, expected, false);

    private static void RunVisualCacheConsumer(bool cache, uint color, uint expected, bool geometryConsumer, bool configurationRecovery = false, bool wicSource = false, int callbackAction = -1, bool transactionRecovery = false, bool multipleDevices = false, bool closeSecondFirst = false, bool resizeWindow = false, bool official = false)
    {
#if NATIVE_OBSERVATION_CONTRACT
        using NativeObservationScope? observation = resizeWindow && !official ? new NativeObservationScope() : null;
#endif
        string path = official ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        if (resizeWindow) LogHardwareModuleIdentity(module, path);
        var create = (delegate* unmanaged[Stdcall]<nint, double, double, uint, delegate* unmanaged[Stdcall]<int, uint, void>, int, nint*, uint*, uint*, int>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_Create");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var releaseResource = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)NativeLibrary.GetExport(module, "MilResource_ReleaseOnChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
        var dirty = (delegate* unmanaged[Stdcall]<int, int, int, int, nint, int>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_AddDirtyRect");
        ExitDiagnostics.Stage("visual CreateOutputWindow before");
        nint window = CreateOutputWindow("Two input shader acceptance", 40, 40);
        ExitDiagnostics.Stage("visual CreateOutputWindow after");
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)NativeLibrary.GetExport(module, "MilCompositionEngine_InitializePartitionManager");
        var deinitialize = (delegate* unmanaged[Stdcall]<int>)NativeLibrary.GetExport(module, "MilCompositionEngine_DeinitializePartitionManager");
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilComposition_SyncFlush");
        bool initialized = false;
        var officialImages = new List<uint>();
        nint secondWindow = 0;
        uint secondDestination = 0;
        uint worldTransform = 0, contentRoot = 0;
        nint d3d = 0, device = 0, connection = 0, channel = 0;
        nint[] surfaces = new nint[2], bitmaps = new nint[2];
        nint lazyStream = 0, lazyFrame = 0, transactionBitmap = 0;
        bool armed = false;
        int callbacks = 0, streamDisposals = 0, nestedResult = 0;
        uint callbackImage = 0;
        bool businessCompleted = false;
        var cleanupFailures = new List<string>();
#if OFFICIAL_CACHE_FIRSTFRAME
        using OfficialCacheObservation? cacheObservation = official && cache && resizeWindow ? new OfficialCacheObservation(module, path, window) : null;
        officialCacheObservation = cacheObservation;
#endif
        void RecordCleanup(string operation, int hr)
        {
            ExitDiagnostics.Stage($"visual cleanup {operation} HRESULT=0x{unchecked((uint)hr):X8}");
            if (hr < 0) cleanupFailures.Add($"{operation}: 0x{unchecked((uint)hr):X8}");
        }
        try
        {
            Assert.AreNotEqual(0, window);
            ExitDiagnostics.Stage("visual EstablishObservableWindow before");
            EstablishObservableWindow(window);
#if NATIVE_OBSERVATION_CONTRACT
            observation?.Window(window);
#endif
            ExitDiagnostics.Stage("visual EstablishObservableWindow after; Direct3DCreate9 before");
            d3d = Direct3DCreate9(32);
            ExitDiagnostics.Stage("visual Direct3DCreate9 after");
            Assert.AreNotEqual(0, d3d);
            PresentParameters parameters = new() { Width = 32, Height = 16, Count = 1, SwapEffect = 1, Window = window, Windowed = 1 };
            ExitDiagnostics.Stage("visual CreateDevice before");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)d3d)[16])(d3d, 0, 1, window, 0x26, &parameters, &device));
            ExitDiagnostics.Stage("visual CreateDevice after; PresentNativeControl before");
#if ORIGINAL_PRESENT_SYSTEM_TRACE
            using (OriginalPresentSystemTrace? trace = resizeWindow && !official ? new OriginalPresentSystemTrace() : null)
            {
                PresentNativeControl(window, device, false);
            }
#else
            PresentNativeControl(window, device, false);
#endif
            ExitDiagnostics.Stage("visual PresentNativeControl after; DiagnoseNativePresent before");
            DiagnoseNativePresent(window, device, false, module);
            ExitDiagnostics.Stage("visual DiagnoseNativePresent after; AssertCombinedPixel before");
            AssertCombinedPixel(window, 0x00563412);
            ExitDiagnostics.Stage("visual AssertCombinedPixel after; connect before");
            nint nativeConnection = 0;
            if (official)
            {
                Assert.AreEqual(0, initialize(0));
                initialized = true;
            }
            Assert.AreEqual(0, connect(official ? (byte)0 : (byte)1, &nativeConnection));
            ExitDiagnostics.Stage("visual connect after");
            connection = nativeConnection;
            nint nativeChannel = 0;
            ExitDiagnostics.Stage("visual channelCreate before");
            Assert.AreEqual(0, channelCreate(connection, 0, &nativeChannel));
            channel = nativeChannel;
            officialCacheObservation?.Record("connection", new { Connection = (long)connection, Channel = (long)channel, Hwnd = (long)window, Async = official });
            ExitDiagnostics.Stage("visual channelCreate after");
            uint[] visuals = new uint[2];
            uint secondaryImage = 0;
            for (int i = 0; i < 2; i++)
            {
                nint surface = 0, bitmap = 0;
                ExitDiagnostics.Stage($"visual surface {i} CreateRenderTarget before");
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, uint, int, nint*, nint, int>)(*(void***)device)[28])(device, 12, 1, 21, 0, 0, 1, &surface, 0));
                surfaces[i] = surface;
                ExitDiagnostics.Stage($"visual surface {i} CreateRenderTarget after; ColorFill before");
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, i == 0 ? 0xffff0000u : color));
                uint width = 0, height = 0;
                ExitDiagnostics.Stage($"visual surface {i} ColorFill after; InteropDeviceBitmap_Create before");
                Assert.AreEqual(0, create(surface, 96, 96, 7, &Available, 0, &bitmap, &width, &height));
                bitmaps[i] = bitmap;
                ExitDiagnostics.Stage($"visual surface {i} InteropDeviceBitmap_Create after; resources before");
                uint image = 0, data = 0, visual = 0;
                Assert.AreEqual(0, resource(channel, 97, &image));
                Assert.AreEqual(0, resource(channel, 43, &data));
                Assert.AreEqual(0, resource(channel, 39, &visual));
                visuals[i] = visual;
                if (i == 1) secondaryImage = image;
                ExitDiagnostics.Stage($"visual surface {i} resources after; image command before");
                ImagePacket update = new() { Type = 0x0a, Handle = image, Interop = (ulong)bitmap };
                ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)bitmap)[1])(bitmap);
                Assert.AreEqual(0, send(&update, (uint)sizeof(ImagePacket), 0, channel));
                DrawPacket draw = new() { Type = 0x18, Handle = data, DataSize = 48, RecordSize = 48, Command = 0x47, Width = 32, Height = 16, Image = image };
                Assert.AreEqual(0, send(&draw, (uint)sizeof(DrawPacket), 0, channel));
                uint* content = stackalloc uint[] { 0x22, visual, data };
                Assert.AreEqual(0, send(content, 12, 0, channel));
                ExitDiagnostics.Stage($"visual surface {i} draw commands after; dirty before");
                int dirtyResult = dirty(0, 0, 12, 1, bitmap);
                officialCacheObservation?.Record("source-resource", new { Index = i, Image = image, Data = data, Visual = visual, Bitmap = (long)bitmap, Surface = (long)surface, ActualWidth = width, ActualHeight = height, Dirty = "0,0,12,1", DirtyHResult = dirtyResult, DrawWidth = draw.Width, DrawHeight = draw.Height, ContentDirty = "image/drawing/content commands submitted; internal dirty Unknown" });
                Assert.AreEqual(0, dirtyResult);
                if (official) officialImages.Add(image);
                else
                {
                    PresentPacket frame = new() { Type = 0x0b, Handle = image };
                    Assert.AreEqual(0, send(&frame, (uint)sizeof(PresentPacket), 0, channel));
                }
            }
            using var wic = new SystemWicBitmap();
            nint wicBitmap = 0;
            if (wicSource)
            {
                if (callbackAction >= 0)
                {
                    byte[] bmp = new byte[62];
                    bmp[0] = (byte)'B'; bmp[1] = (byte)'M';
                    MemoryMarshal.Write(bmp.AsSpan(2), bmp.Length);
                    MemoryMarshal.Write(bmp.AsSpan(10), 54);
                    MemoryMarshal.Write(bmp.AsSpan(14), 40);
                    MemoryMarshal.Write(bmp.AsSpan(18), 2);
                    MemoryMarshal.Write(bmp.AsSpan(22), 1);
                    bmp[26] = 1; bmp[28] = 24;
                    for (int pixel = 0; pixel < 2; pixel++)
                    {
                        bmp[54 + pixel * 3] = (byte)color;
                        bmp[55 + pixel * 3] = (byte)(color >> 8);
                        bmp[56 + pixel * 3] = (byte)(color >> 16);
                    }
                    lazyStream = ManagedStreamTests.CreateCallbackStream(module, bmp, () =>
                    {
                        if (!armed) return;
                        armed = false; callbacks++;
                        if (callbackAction == 0) nestedResult = present(connection);
                        else if (callbackAction == 1) { nestedResult = destroy(channel); channel = 0; }
                        else if (callbackAction == 2) { nestedResult = disconnect(connection); connection = 0; }
                        else if (callbackAction == 4)
                        {
                            var releaseResource = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)NativeLibrary.GetExport(module, "MilResource_ReleaseOnChannel");
                            int deleted = 0;
                            nestedResult = releaseResource(channel, callbackImage, &deleted);
                            if (nestedResult >= 0) nestedResult = close(channel);
                            if (nestedResult >= 0) nestedResult = commit(channel);
                        }
                        else
                        {
                            nint replacementBitmap = wic.Create(2, 1), replacementWrapper = 0;
                            try
                            {
                                WriteHardwareWic(replacementBitmap, 0xff0000ffu);
                                var replacementWrap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateCWICWrapperBitmap");
                                nestedResult = replacementWrap(replacementBitmap, &replacementWrapper);
                                if (nestedResult < 0) return;
                                byte[] packet = new byte[8 + IntPtr.Size];
                                MemoryMarshal.Write(packet.AsSpan(), 0x0cu);
                                MemoryMarshal.Write(packet.AsSpan(4), callbackImage);
                                MemoryMarshal.Write(packet.AsSpan(8), replacementWrapper);
                                SendSamplerPacket(send, channel, packet);
                                nestedResult = close(channel);
                                if (nestedResult >= 0) nestedResult = commit(channel);
                            }
                            finally { Release(replacementBitmap); }
                        }
                    }, () => streamDisposals++);
                    lazyFrame = wic.DecodeFrame(lazyStream);
                    wicBitmap = lazyFrame;
                    ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)wicBitmap)[1])(wicBitmap);
                }
                else
                {
                    wicBitmap = wic.Create(2, 1);
                    WriteHardwareWic(wicBitmap, color);
                    if (transactionRecovery || multipleDevices)
                    {
                        transactionBitmap = wicBitmap;
                        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)wicBitmap)[1])(wicBitmap);
                    }
                }
                try
                {
                    var wrap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateCWICWrapperBitmap");
                    nint wrapper = 0;
                    Assert.AreEqual(0, wrap(wicBitmap, &wrapper));
                    uint image = 0;
                    Assert.AreEqual(0, resource(channel, 95, &image));
                    byte[] sourcePacket = new byte[8 + IntPtr.Size];
                    MemoryMarshal.Write(sourcePacket.AsSpan(), 0x0cu);
                    MemoryMarshal.Write(sourcePacket.AsSpan(4), image);
                    MemoryMarshal.Write(sourcePacket.AsSpan(8), wrapper);
                    SendSamplerPacket(send, channel, sourcePacket);
                    uint data = 0;
                    Assert.AreEqual(0, resource(channel, 43, &data));
                    DrawPacket draw = new() { Type = 0x18, Handle = data, DataSize = 48, RecordSize = 48, Command = 0x47, Width = 32, Height = 16, Image = image };
                    Assert.AreEqual(0, send(&draw, (uint)sizeof(DrawPacket), 0, channel));
                    uint* content = stackalloc uint[] { 0x22, visuals[1], data };
                    Assert.AreEqual(0, send(content, 12, 0, channel));
                    secondaryImage = image;
                    callbackImage = image;
                }
                finally { Release(wicBitmap); }
            }
            ExitDiagnostics.Stage("visual resources complete; native shader before");
            VerifyTwoInputShaderOnNativeDevice(device);
            ExitDiagnostics.Stage("visual native shader after; bind effect before");
            uint hardwareMode = 0;
            if (geometryConsumer)
                hardwareMode = BindHardwareCacheGeometry(resource, send, channel, visuals[0], visuals[1]);
            else
                BindTwoInputEffect(resource, send, channel, visuals[0], color == 0 ? 0u : visuals[1], cache);
            ExitDiagnostics.Stage("visual bind effect after; target before");
            uint destination = 0;
            Assert.AreEqual(0, resource(channel, 46, &destination));
            byte* target = stackalloc byte[92];
            new Span<byte>(target, 92).Clear();
            *(uint*)target = 0x31; *(uint*)(target + 4) = destination;
            *(ulong*)(target + 8) = (ulong)window;
            *(uint*)(target + 32) = 32; *(uint*)(target + 36) = 16;
            *(float*)(target + 52) = 1;
            if (official)
            {
                *(uint*)(target + 56) = 12;
                int protocolContext = 0;
                foreach (int value in new[] { -1, -2, -3, -4 })
                    if (AreDpiAwarenessContextsEqual(GetWindowDpiAwarenessContext(window), value) != 0) protocolContext = value;
                Assert.AreNotEqual(0, protocolContext);
                *(int*)(target + 72) = protocolContext;
                *(double*)(target + 76) = GetDpiForWindow(window) / 96.0;
                *(double*)(target + 84) = GetDpiForWindow(window) / 96.0;
            }
            Assert.AreEqual(0, send(target, 92, 0, channel));
            SendOutputWindowSettings(send, channel, destination, window);
            // HwndTarget binds its DPI transform to a separate content root, above the business visual.
            Assert.AreEqual(0, resource(channel, 39, &contentRoot));
            Assert.AreEqual(0, resource(channel, 66, &worldTransform));
            Assert.AreNotEqual(0u, contentRoot);
            Assert.AreNotEqual(0u, worldTransform);
            ExitDiagnostics.Stage($"visual world create root={contentRoot} transform={worldTransform} child={visuals[0]}");
            byte* matrix = stackalloc byte[60];
            new Span<byte>(matrix, 60).Clear();
            *(uint*)matrix = 0x77;
            *(uint*)(matrix + 4) = worldTransform;
            double dpiScale = GetDpiForWindow(window) / 96.0;
            *(double*)(matrix + 8) = dpiScale;
            *(double*)(matrix + 32) = dpiScale;
            int matrixResult = send(matrix, 60, 0, channel);
            officialCacheObservation?.Packet(channel, new ReadOnlySpan<byte>(matrix, 60), matrixResult);
            officialCacheObservation?.Record("target-resource-graph", new { Target = destination, Hwnd = (long)window, Channel = (long)channel, Root = contentRoot, Transform = worldTransform, Child = visuals[0], Secondary = visuals[1], TargetHex = Convert.ToHexString(new ReadOnlySpan<byte>(target, 92)), SubmittedWorld = new[] { dpiScale, 0.0, 0.0, dpiScale, 0.0, 0.0 }, TargetWidth = 32, TargetHeight = 16 });
            Assert.AreEqual(0, matrixResult);
            ExitDiagnostics.Stage($"visual world update transform={worldTransform} M11={dpiScale:R} M22={dpiScale:R} size=60 animation=0");
            uint* transform = stackalloc uint[] { 0x1c, contentRoot, worldTransform };
            Assert.AreEqual(0, send(transform, 12, 0, channel));
            uint* child = stackalloc uint[] { 0x26, contentRoot, visuals[0], 0 };
            Assert.AreEqual(0, send(child, 16, 0, channel));
            ExitDiagnostics.Stage($"visual world bind root={contentRoot} transform={worldTransform} child={visuals[0]}");
            uint* root = stackalloc uint[] { 0x35, destination, contentRoot };
            Assert.AreEqual(0, send(root, 12, 0, channel));
            ExitDiagnostics.Stage($"visual world SetRoot target={destination} root={contentRoot}");
            if (multipleDevices)
            {
                secondWindow = CreateOutputWindow("Second cache device", 100, 40);
                EstablishObservableWindow(secondWindow);
                Assert.AreEqual(0, resource(channel, 46, &secondDestination));
                *(uint*)(target + 4) = secondDestination;
                *(ulong*)(target + 8) = (ulong)secondWindow;
                Assert.AreEqual(0, send(target, 92, 0, channel));
                SendOutputWindowSettings(send, channel, secondDestination, secondWindow);
                root[1] = secondDestination;
                Assert.AreEqual(0, send(root, 12, 0, channel));
            }
            ExitDiagnostics.Stage("visual target after; close before");
            Assert.AreEqual(0, close(channel));
            ExitDiagnostics.Stage("visual close after; commit before");
            int initialCommitResult = commit(channel);
            ExitDiagnostics.Stage($"visual commit returned HRESULT=0x{initialCommitResult:X8}");
            Assert.AreEqual(0, initialCommitResult);
            if (official)
            {
                Assert.AreEqual(0, flush(channel));
                foreach (uint image in officialImages) JhPresentImage(send, commit, flush, channel, image);
            }
            ExitDiagnostics.Stage("visual commit after; bitmap0 release before");
            Release(bitmaps[0]);
            bitmaps[0] = 0;
            armed = callbackAction >= 0;
            for (int frame = 0; frame < 2; frame++)
            {
                ExitDiagnostics.Stage($"visual frame {frame} present before");
                int frameResult = official ? flush(channel) : present(connection);
                officialCacheObservation?.Record("initial-frame-flush", new { Frame = frame, HResult = frameResult, Channel = (long)channel, Hwnd = (long)window });
                Assert.AreEqual(0, frameResult, "Two-input Present");
                ExitDiagnostics.Stage($"visual frame {frame} present after");
                Assert.AreEqual(0, DwmFlush());
                AssertCombinedPixel(window, expected);
                if (callbackAction >= 0)
                {
                    Assert.AreEqual(1, callbacks);
                    Assert.AreEqual(callbackAction == 0 ? unchecked((int)0x8000ffff) : 0, nestedResult);
                    if (callbackAction == 3)
                    {
                        for (int replacementFrame = 0; replacementFrame < 2; replacementFrame++)
                        {
                            Assert.AreEqual(0, present(connection), "Callback replacement must invalidate the frozen candidate key");
                            Assert.AreEqual(0, DwmFlush());
                            AssertCombinedPixel(window, 0x00ff0000u);
                        }
                        Assert.AreEqual(1, callbacks);
                    }
                    return;
                }
            }
            if (resizeWindow)
            {
                LogHardwareModuleIdentity(module, path);
                foreach (int width in new[] { 64, 48, 32 })
                {
                    Assert.AreNotEqual(0, windowThread!.Invoke(() => SetWindowPos(window, -1, 40, 40, width, 24, 0x0040)));
                    Assert.AreNotEqual(0, GetClientRect(window, out WindowRectangle resized));
                    Assert.AreEqual(width, resized.Right - resized.Left);
                    Assert.AreEqual(24, resized.Bottom - resized.Top);
                    ExitDiagnostics.Stage($"visual resize official={official} client={resized.Right - resized.Left}x{resized.Bottom - resized.Top} settings before");
                    SendOutputWindowSettings(send, channel, destination, window);
                    Assert.AreEqual(0, close(channel));
                    Assert.AreEqual(0, commit(channel));
                    int resizePresentResult = official ? flush(channel) : present(connection);
                    ExitDiagnostics.Stage($"visual resize width={width} export={(official ? "MilComposition_SyncFlush" : "WgxConnection_SameThreadPresent")} HRESULT=0x{resizePresentResult:X8}");
                    Assert.AreEqual(0, resizePresentResult);
                    Assert.AreEqual(0, DwmFlush());
                    Console.WriteLine($"HardwareResize official={official} client={width}x24");
                    AssertCombinedPixel(window, expected);
                }
            }
            if (multipleDevices)
            {
                nint bitmapLock = 0;
                try
                {
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)transactionBitmap)[8])(transactionBitmap, 0, 2, &bitmapLock));
                    Assert.AreEqual(0, present(connection), "Both committed device keys must hit without CopyPixels");
                    Assert.AreEqual(0, DwmFlush());
                    AssertCombinedPixel(window, expected);
                    AssertCombinedPixel(secondWindow, expected);
                }
                finally { Release(bitmapLock); }
                var releaseTarget = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)NativeLibrary.GetExport(module, "MilResource_ReleaseOnChannel");
                int deleted = 0;
                Assert.AreEqual(0, releaseTarget(channel, closeSecondFirst ? secondDestination : destination, &deleted));
                Assert.AreEqual(1, deleted);
                Assert.AreEqual(0, close(channel));
                Assert.AreEqual(0, commit(channel));
                if (closeSecondFirst) DestroyWindow(secondWindow);
                else
                {
                    DestroyWindow(window);
                    window = secondWindow;
                }
                secondWindow = 0;
                Assert.AreEqual(0, present(connection), "Surviving target must retain its device cache after peer close");
                Assert.AreEqual(0, DwmFlush());
                AssertCombinedPixel(window, expected);
            }
            if (transactionRecovery)
            {
                nint bitmapLock = 0;
                try
                {
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)transactionBitmap)[8])(transactionBitmap, 0, 2, &bitmapLock));
                    byte[] configuration = new byte[28];
                    MemoryMarshal.Write(configuration.AsSpan(), 0x8du);
                    MemoryMarshal.Write(configuration.AsSpan(4), hardwareMode);
                    MemoryMarshal.Write(configuration.AsSpan(8), 2.0);
                    SendSamplerPacket(send, channel, configuration);
                    Assert.AreEqual(0, close(channel));
                    Assert.AreEqual(0, commit(channel));
                    Assert.AreEqual(unchecked((int)0x88982f0d), present(connection), "System WIC write lock rejects candidate CopyPixels");
                    AssertCombinedPixel(window, expected);
                    Assert.AreEqual(unchecked((int)0x88982f0d), present(connection), "Failed candidate must not become a cache hit");
                    AssertCombinedPixel(window, expected);
                }
                finally { Release(bitmapLock); }
                Assert.AreEqual(0, present(connection), "Retry identical generation after releasing the WIC write lock");
                Assert.AreEqual(0, DwmFlush());
                AssertCombinedPixel(window, expected);
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, DwmFlush());
                AssertCombinedPixel(window, expected);
            }
            if (configurationRecovery)
            {
                byte[] configuration = new byte[28];
                MemoryMarshal.Write(configuration.AsSpan(), 0x8du);
                MemoryMarshal.Write(configuration.AsSpan(4), hardwareMode);
                MemoryMarshal.Write(configuration.AsSpan(8), 1.0);
                MemoryMarshal.Write(configuration.AsSpan(24), 1);
                SendSamplerPacket(send, channel, configuration);
                Assert.AreEqual(0, close(channel));
                Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(unchecked((int)0x80004001), present(connection));
                Assert.AreEqual(unchecked((int)0x80004001), present(connection));
                AssertCombinedPixel(window, expected);
                MemoryMarshal.Write(configuration.AsSpan(24), 0);
                MemoryMarshal.Write(configuration.AsSpan(8), 2.0);
                SendSamplerPacket(send, channel, configuration);
                Assert.AreEqual(0, close(channel));
                Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, DwmFlush());
                AssertCombinedPixel(window, expected);
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, DwmFlush());
                AssertCombinedPixel(window, expected);
            }
            uint changedColor = color == 0xff00ff00u ? 0xff0000ffu : 0xff00ff00u;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surfaces[1], 0, changedColor));
            if (wicSource)
            {
                nint replacementBitmap = wic.Create(2, 1), wrapper = 0;
                try
                {
                    WriteHardwareWic(replacementBitmap, changedColor);
                    var wrap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateCWICWrapperBitmap");
                    Assert.AreEqual(0, wrap(replacementBitmap, &wrapper));
                    byte[] packet = new byte[8 + IntPtr.Size];
                    MemoryMarshal.Write(packet.AsSpan(), 0x0cu);
                    MemoryMarshal.Write(packet.AsSpan(4), secondaryImage);
                    MemoryMarshal.Write(packet.AsSpan(8), wrapper);
                    SendSamplerPacket(send, channel, packet);
                }
                finally { Release(replacementBitmap); }
            }
            else
            {
                Assert.AreEqual(0, dirty(0, 0, 12, 1, bitmaps[1]));
                if (!official)
                {
                    PresentPacket changed = new() { Type = 0x0b, Handle = secondaryImage };
                    Assert.AreEqual(0, send(&changed, (uint)sizeof(PresentPacket), 0, channel));
                }
            }
            Assert.AreEqual(0, close(channel));
            Assert.AreEqual(0, commit(channel));
            if (official) JhPresentImage(send, commit, flush, channel, secondaryImage);
            Release(bitmaps[1]);
            bitmaps[1] = 0;
            for (int frame = 0; frame < 2; frame++)
            {
                Assert.AreEqual(0, official ? flush(channel) : present(connection), "Present after secondary mutation and caller release");
                Assert.AreEqual(0, DwmFlush());
                AssertCombinedPixel(window, geometryConsumer
                    ? color == 0xff00ff00u ? 0x00ff0000u : 0x0000ff00u
                    : color == 0 ? expected
                        : color == 0xff00ff00u ? 0x00ff0080u : 0x0000ff80u);
            }
            businessCompleted = true;
        }
        finally
        {
            if (channel != 0) officialCacheObservation?.DrainAfterResult(module, channel, businessCompleted);
            officialCacheObservation = null;
            if (channel != 0 && (worldTransform != 0 || contentRoot != 0))
            {
                if (worldTransform != 0)
                {
                    int deleted = 0;
                    RecordCleanup("release world transform", releaseResource(channel, worldTransform, &deleted));
                    ExitDiagnostics.Stage($"visual cleanup world transform={worldTransform} deleted={deleted}");
                    if (deleted != 1) cleanupFailures.Add($"world transform deleted: {deleted}");
                }
                if (contentRoot != 0)
                {
                    int deleted = 0;
                    RecordCleanup("release content root", releaseResource(channel, contentRoot, &deleted));
                    ExitDiagnostics.Stage($"visual cleanup content root={contentRoot} deleted={deleted}");
                    if (deleted != 1) cleanupFailures.Add($"content root deleted: {deleted}");
                }
                RecordCleanup("world release close", close(channel));
                RecordCleanup("world release commit", commit(channel));
            }
            ExitDiagnostics.Stage("visual cleanup destroy channel before");
            if (channel != 0) RecordCleanup("destroy", destroy(channel));
            ExitDiagnostics.Stage("visual cleanup destroy channel after; disconnect before");
            if (connection != 0) RecordCleanup("disconnect", disconnect(connection));
            if (initialized) RecordCleanup("deinitialize", deinitialize());
            ExitDiagnostics.Stage("visual cleanup disconnect after; surfaces before");
            for (int i = 1; i >= 0; i--)
            {
                ExitDiagnostics.Stage($"visual cleanup bitmap {i} before");
                Release(bitmaps[i]);
                ExitDiagnostics.Stage($"visual cleanup bitmap {i} after; surface before");
                Release(surfaces[i]);
                ExitDiagnostics.Stage($"visual cleanup surface {i} after");
            }
            Release(transactionBitmap); Release(lazyFrame); Release(lazyStream);
            ExitDiagnostics.Stage("visual cleanup device before");
            Release(device); Release(d3d);
            ExitDiagnostics.Stage("visual cleanup device after; windows before");
            if (secondWindow != 0) DestroyWindow(secondWindow);
            if (window != 0) DestroyWindow(window);
            if (lazyStream != 0 && streamDisposals != 1) cleanupFailures.Add($"streamDisposals: {streamDisposals}");
            ExitDiagnostics.Stage("visual cleanup complete: " + string.Join("; ", cleanupFailures));
            if (businessCompleted) Assert.IsEmpty(cleanupFailures, string.Join("; ", cleanupFailures));
        }
    }

    private static void VerifyTwoInputShaderOnNativeDevice(nint device)
    {
        uint[] tokens = TwoInputShaderTokens;
        nint shader = 0;
        int hr;
        fixed (uint* code = tokens)
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, nint*, int>)(*(void***)device)[106])(device, code, &shader);
        Console.WriteLine($"IDirect3DDevice9::CreatePixelShader ({tokens.Length} DWORD/{tokens.Length * sizeof(uint)} bytes): HRESULT=0x{hr:X8}");
        Release(shader);
        Assert.AreEqual(0, hr, "Native CreatePixelShader for specified two-input bytecode");
    }

    private static void AssertCombinedPixel(nint window, uint expected)
    {
        nint dc = GetDC(window);
        try
        {
#if NATIVE_OBSERVATION_CONTRACT
            nativeObservation?.BeforePixel(window, dc);
#endif
            uint actual = GetPixel(dc, 16, 8);
#if NATIVE_OBSERVATION_CONTRACT
            nativeObservation?.Pixel(actual, expected);
#endif
            officialCacheObservation?.Record("pixel", new { Hwnd = (long)window, X = 16, Y = 8, Expected = expected, Actual = actual });
            Console.WriteLine($"Two-input pixel expected=0x{expected:X8} actual=0x{actual:X8}");
            for (int shift = 0; shift < 24; shift += 8)
                Assert.IsTrue(Math.Abs((int)((actual >> shift) & 255) - (int)((expected >> shift) & 255)) <= 1, $"Pixel 0x{actual:X8}, expected 0x{expected:X8}");
        }
        finally
        {
#if NATIVE_OBSERVATION_CONTRACT
            int released = ReleaseDC(window, dc);
            nativeObservation?.Released("window", released);
#else
            ReleaseDC(window, dc);
#endif
        }
    }

    private static void BindTwoInputEffect(delegate* unmanaged[Stdcall]<nint, uint, uint*, int> resource,
        delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int> send, nint channel, uint visual, uint source, bool cache)
    {
        uint shader = 0, implicitBrush = 0, secondary = 0, effect = 0;
        Assert.AreEqual(0, resource(channel, 33, &shader));
        Assert.AreEqual(0, resource(channel, 34, &implicitBrush));
        Assert.AreEqual(0, resource(channel, cache ? 83u : 82u, &secondary));
        Assert.AreEqual(0, resource(channel, 38, &effect));
        uint[] tokens = TwoInputShaderTokens;
        uint bytecodeLength = checked((uint)(tokens.Length * sizeof(uint)));
        byte[] packet = new byte[20 + bytecodeLength];
        MemoryMarshal.Write(packet.AsSpan(0), 0x6cu); MemoryMarshal.Write(packet.AsSpan(4), shader);
        MemoryMarshal.Write(packet.AsSpan(8), 2u); MemoryMarshal.Write(packet.AsSpan(12), bytecodeLength);
        tokens.CopyTo(MemoryMarshal.Cast<byte, uint>(packet.AsSpan(20)));
        SendSamplerPacket(send, channel, packet);
        packet = new byte[28];
        MemoryMarshal.Write(packet.AsSpan(0), 0x6du); MemoryMarshal.Write(packet.AsSpan(4), implicitBrush);
        MemoryMarshal.Write(packet.AsSpan(8), 1.0);
        SendSamplerPacket(send, channel, packet);
        if (cache)
        {
            uint mode = 0;
            Assert.AreEqual(0, resource(channel, 94, &mode));
            packet = new byte[28];
            MemoryMarshal.Write(packet.AsSpan(0), 0x8du); MemoryMarshal.Write(packet.AsSpan(4), mode);
            MemoryMarshal.Write(packet.AsSpan(8), 1.0);
            SendSamplerPacket(send, channel, packet);
            packet = new byte[36];
            MemoryMarshal.Write(packet.AsSpan(0), 0x84u); MemoryMarshal.Write(packet.AsSpan(4), secondary);
            MemoryMarshal.Write(packet.AsSpan(8), 1.0);
            MemoryMarshal.Write(packet.AsSpan(28), mode); MemoryMarshal.Write(packet.AsSpan(32), source);
        }
        else
        {
            packet = new byte[148];
            MemoryMarshal.Write(packet.AsSpan(0), 0x83u); MemoryMarshal.Write(packet.AsSpan(4), secondary);
            MemoryMarshal.Write(packet.AsSpan(8), 1.0);
            foreach (int offset in new[] { 32, 40, 64, 72 }) MemoryMarshal.Write(packet.AsSpan(offset), 1.0);
            MemoryMarshal.Write(packet.AsSpan(80), 0.5); MemoryMarshal.Write(packet.AsSpan(88), 2.0);
            MemoryMarshal.Write(packet.AsSpan(108), 1u); MemoryMarshal.Write(packet.AsSpan(112), 1u);
            MemoryMarshal.Write(packet.AsSpan(124), 1u); MemoryMarshal.Write(packet.AsSpan(144), source);
        }
        SendSamplerPacket(send, channel, packet);
        packet = new byte[122];
        MemoryMarshal.Write(packet.AsSpan(0), 0x70u); MemoryMarshal.Write(packet.AsSpan(4), effect);
        MemoryMarshal.Write(packet.AsSpan(40), shader); MemoryMarshal.Write(packet.AsSpan(44), -1);
        MemoryMarshal.Write(packet.AsSpan(48), 2u); MemoryMarshal.Write(packet.AsSpan(52), 16u);
        MemoryMarshal.Write(packet.AsSpan(72), 16u); MemoryMarshal.Write(packet.AsSpan(76), 8u);
        MemoryMarshal.Write(packet.AsSpan(80), (ushort)0);
        for (int i = 0; i < 3; i++) MemoryMarshal.Write(packet.AsSpan(82 + 4 * i), 0.5f);
        int[] samplerInfo = [0, 1, 1, 1];
        for (int i = 0; i < 4; i++) MemoryMarshal.Write(packet.AsSpan(98 + 4 * i), samplerInfo[i]);
        MemoryMarshal.Write(packet.AsSpan(114), implicitBrush); MemoryMarshal.Write(packet.AsSpan(118), secondary);
        SendSamplerPacket(send, channel, packet);
        uint* binding = stackalloc uint[] { 0x1d, visual, effect };
        Assert.AreEqual(0, send(binding, 12, 0, channel));
    }

    private static void SendSamplerPacket(delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int> send, nint channel, byte[] packet)
    {
        fixed (byte* pointer = packet)
        {
            int hr = send(pointer, (uint)packet.Length, 0, channel);
            officialCacheObservation?.Packet(channel, packet, hr);
            Assert.AreEqual(0, hr, $"Command 0x{BitConverter.ToUInt32(packet):X}, {packet.Length} bytes");
        }
    }
}
