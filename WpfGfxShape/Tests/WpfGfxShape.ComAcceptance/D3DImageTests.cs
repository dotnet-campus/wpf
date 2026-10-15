using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe partial class D3DImageTests
{
    [DllImport("d3d9.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern nint Direct3DCreate9(uint version);
    [DllImport("d3d9.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int Direct3DCreate9Ex(uint version, nint* factory);
    [DllImport("user32.dll")]
    private static extern nint GetDesktopWindow();

    [StructLayout(LayoutKind.Sequential)]
    private struct PresentParameters
    {
        internal uint Width, Height, Format, Count, Multisample, Quality, SwapEffect;
        internal nint Window;
        internal int Windowed, AutoDepth;
        internal uint DepthFormat, Flags, Refresh, Interval;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate void AvailabilityCallback(int available, uint version);

    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
    private static void Available(int available, uint version) { }

    [TestMethod]
    [DataRow(21u)]
    [DataRow(22u)]
    public void WhenRealD3D9RenderTargetIsReadThenSoftwareSnapshotContainsDevicePixels(uint format)
    {
        string path = AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        var create = (delegate* unmanaged[Stdcall]<nint, double, double, uint, delegate* unmanaged[Stdcall]<int, uint, void>, int, nint*, uint*, uint*, int>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_Create");
        var snapshot = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_GetAsSoftwareBitmap");
        var detach = (delegate* unmanaged[Stdcall]<nint, void>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_Detach");
        nint d3d = Direct3DCreate9(32), device = 0, surface = 0, bitmap = 0, copy = 0;
        try
        {
            Assert.AreNotEqual(0, d3d);
            PresentParameters parameters = new() { Width = 2, Height = 1, Count = 1, SwapEffect = 1, Window = GetDesktopWindow(), Windowed = 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)d3d)[16])(d3d, 0, 1, parameters.Window, 0x26, &parameters, &device));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, uint, int, nint*, nint, int>)(*(void***)device)[28])(device, 12, 1, format, 0, 0, 1, &surface, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, 0xff123456));
            uint width = 0, height = 0;
            Assert.AreEqual(0, create(surface, 96, 96, 7, &Available, 1, &bitmap, &width, &height));
            Assert.AreEqual((12u, 1u), (width, height));
            Assert.AreEqual(0, snapshot(bitmap, &copy));
            uint* pixels = stackalloc uint[12];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, void*, int>)(*(void***)copy)[7])(copy, 0, 48, 48, pixels));
            Assert.AreEqual(0xff123456u, pixels[0]);
            VerifyChannel(module, device, surface, bitmap, detach);
            nint rejected = 0;
            Assert.AreEqual(unchecked((int)0x80004005), snapshot(bitmap, &rejected));
            Assert.AreEqual(0, rejected);
        }
        finally
        {
            Release(copy); Release(bitmap); Release(surface); Release(device); Release(d3d);
        }
    }

    [TestMethod]
    [DataRow(21u, false)]
    [DataRow(22u, false)]
    [DataRow(21u, true)]
    [DataRow(22u, true)]
    public void WhenCallerReleasesSurfaceThenInteropOwnsItAndSnapshotSurvivesDetach(uint format, bool retry)
    {
        string path = AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        var create = (delegate* unmanaged[Stdcall]<nint, double, double, uint, delegate* unmanaged[Stdcall]<int, uint, void>, int, nint*, uint*, uint*, int>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_Create");
        var snapshot = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_GetAsSoftwareBitmap");
        var detach = (delegate* unmanaged[Stdcall]<nint, void>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_Detach");
        nint d3d = Direct3DCreate9(32), device = 0, surface = 0, bitmap = 0, copy = 0;
        bool locked = false;
        try
        {
            PresentParameters parameters = new() { Width = 2, Height = 1, Count = 1, SwapEffect = 1, Window = GetDesktopWindow(), Windowed = 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)d3d)[16])(d3d, 0, 1, parameters.Window, 0x26, &parameters, &device));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, uint, int, nint*, nint, int>)(*(void***)device)[28])(device, 2, 1, format, 0, 0, 1, &surface, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, 0xff123456));
            uint width = 0, height = 0;
            Assert.AreEqual(0, create(surface, 96, 96, 19, &Available, 1, &bitmap, &width, &height));
            if (retry)
            {
                LockedSurface pixels;
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, LockedSurface*, nint, uint, int>)(*(void***)surface)[13])(surface, &pixels, 0, 0));
                locked = true;
                Assert.IsLessThan(0, snapshot(bitmap, &copy));
                Assert.AreEqual(0, copy);
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)surface)[14])(surface));
                locked = false;
            }
            Release(surface); surface = 0;
            Assert.AreEqual(0, snapshot(bitmap, &copy));
            detach(bitmap);
            Release(bitmap); bitmap = 0;
            Assert.AreEqual((0xff123456u, 0xff123456u), Read(copy));
        }
        finally
        {
            if (locked) ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)surface)[14])(surface);
            Release(copy); Release(bitmap); Release(surface); Release(device); Release(d3d);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int DuplicateHandle(nint sourceProcess, nint source, nint targetProcess, nint* target, uint access, int inherit, uint options);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenPresentPacketIsDroppedThenTransferredCompletionEventIsSignalled(bool commitBatch)
    {
        string path = AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var create = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        nint connection = 0, channel = 0;
        using var completion = new EventWaitHandle(false, EventResetMode.ManualReset);
        try
        {
            Assert.AreEqual(0, connect(1, &connection));
            Assert.AreEqual(0, create(connection, 0, &channel));
            uint image = 0;
            Assert.AreEqual(0, resource(channel, 97, &image));
            nint transferred = 0;
            Assert.AreNotEqual(0, DuplicateHandle(-1, completion.SafeWaitHandle.DangerousGetHandle(), -1, &transferred, 0, 0, 2));
            PresentPacket frame = new() { Type = 0x0b, Handle = image, Event = (ulong)transferred };
            Assert.AreEqual(0, send(&frame, (uint)sizeof(PresentPacket), 0, channel));
            if (commitBatch)
            {
                Assert.AreEqual(0, close(channel));
                Assert.AreEqual(0, commit(channel));
            }
            Assert.AreEqual(0, destroy(channel)); channel = 0;
            Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(1)));
        }
        finally
        {
            if (channel != 0) destroy(channel);
            if (connection != 0) disconnect(connection);
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowExW(uint extendedStyle, string className, string title,
        uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll")]
    private static extern int DestroyWindow(nint window);
    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);
    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint dc);
    [DllImport("gdi32.dll")]
    private static extern uint GetPixel(nint dc, int x, int y);
    [DllImport("dwmapi.dll")]
    private static extern int DwmFlush();
    [DllImport("user32.dll")]
    private static extern int UpdateWindow(nint window);
    [DllImport("user32.dll")]
    private static extern int IsWindowVisible(nint window);
    [DllImport("user32.dll")]
    private static extern int IsIconic(nint window);
    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")]
    private static extern int SetForegroundWindow(nint window);
    [DllImport("user32.dll")]
    private static extern int GetClientRect(nint window, out WindowRectangle rectangle);
    [DllImport("user32.dll")]
    private static extern int GetWindowRect(nint window, out WindowRectangle rectangle);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassNameW(nint window, char* name, int capacity);
    [DllImport("user32.dll")]
    private static extern int ClientToScreen(nint window, ref WindowPoint point);
    [DllImport("user32.dll")]
    private static extern nint WindowFromPoint(WindowPoint point);
    [DllImport("user32.dll")]
    private static extern int SetWindowPos(nint window, nint insertAfter, int x, int y, int width, int height, uint flags);

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowRectangle
    {
        internal int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowPoint
    {
        internal int X, Y;
    }
    [DllImport("user32.dll")]
    private static extern int PeekMessageW(out WindowMessage message, nint window, uint minimum, uint maximum, uint remove);
    [DllImport("user32.dll")]
    private static extern nint DispatchMessageW(in WindowMessage message);

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowMessage
    {
        internal nint Window;
        internal uint Message;
        internal nuint WParam;
        internal nint LParam;
        internal uint Time;
        internal int X, Y;
        internal uint Private;
    }

    private static uint BindPassThroughEffect(
        delegate* unmanaged[Stdcall]<nint, uint, uint*, int> resource,
        delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int> send, nint channel, uint visual, bool malformed = false)
    {
        uint shader = 0, brush = 0, effect = 0;
        Assert.AreEqual(0, resource(channel, 33, &shader));
        Assert.AreEqual(0, resource(channel, 34, &brush));
        Assert.AreEqual(0, resource(channel, 38, &effect));
        // Own ps_2_0: dcl t0.xy; dcl_2d s0; texld r0,t0,s0; mov oC0,r0.
        uint[] code = [0xffff0200, 0x0200001f, 0x80000000, 0xb0030000,
            0x0200001f, 0x90000000, 0xa00f0800, 0x03000042, 0x800f0000,
            0xb0e40000, 0xa0e40800, 0x02000001, 0x800f0800, 0x80e40000, 0x0000ffff];
        if (malformed) code = [0xffff0200, 0x01000028, 0x80000000, 0xffff];
        byte[] shaderPacket = new byte[20 + code.Length * 4];
        Span<uint> shaderWords = MemoryMarshal.Cast<byte, uint>(shaderPacket.AsSpan());
        shaderWords[0] = 0x6c; shaderWords[1] = shader; shaderWords[2] = 2;
        shaderWords[3] = (uint)code.Length * 4;
        code.CopyTo(shaderWords[5..]);
        fixed (byte* packet = shaderPacket)
            Assert.AreEqual(0, send(packet, (uint)shaderPacket.Length, 0, channel));
        byte[] brushPacket = new byte[28];
        Span<uint> brushWords = MemoryMarshal.Cast<byte, uint>(brushPacket.AsSpan());
        brushWords[0] = 0x6d; brushWords[1] = brush;
        MemoryMarshal.Write(brushPacket.AsSpan(8), 1.0);
        fixed (byte* packet = brushPacket)
            Assert.AreEqual(0, send(packet, 28, 0, channel));
        byte[] effectPacket = new byte[92];
        Span<uint> effectWords = MemoryMarshal.Cast<byte, uint>(effectPacket.AsSpan());
        effectWords[0] = 0x70; effectWords[1] = effect;
        effectWords[10] = shader; effectWords[11] = uint.MaxValue;
        effectWords[18] = 8; effectWords[19] = 4;
        effectWords[20] = 0; effectWords[21] = 1; effectWords[22] = brush;
        fixed (byte* packet = effectPacket)
            Assert.AreEqual(0, send(packet, 92, 0, channel));
        uint* binding = stackalloc uint[] { 0x1d, visual, effect };
        Assert.AreEqual(0, send(binding, 12, 0, channel));
        return shader;
    }

    private static void ProcessWindowMessages(nint window)
    {
        while (PeekMessageW(out WindowMessage message, window, 0, 0, 1) != 0)
            DispatchMessageW(in message);
    }

    [TestMethod]
    [DataRow(40, 40, 21u, false, 0, false)]
    [DataRow(120, 80, 21u, false, 0, false)]
    [DataRow(40, 40, 21u, true, 0, false)]
    [DataRow(120, 80, 22u, true, 0, false)]
    [DataRow(40, 40, 21u, true, 1, false)]
    [DataRow(40, 40, 21u, true, 2, false)]
    [DataRow(40, 40, 21u, true, 3, false)]
    [DataRow(40, 40, 21u, true, 0, true)]
    public void WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels(int x, int y, uint format, bool shared, int callbackMode, bool shaderEffect)
        => RunPublicShaderFrames(x, y, format, shared, callbackMode, shaderEffect);

    [TestMethod]
    public void WhenHardwareShaderGenerationIsReplacedThenTwoFramesConsumeLegalBytecode()
        => RunPublicShaderFrames(40, 40, 21, true, 0, true, replaceShader: true);

    [TestMethod]
    public void WhenMalformedHardwareShaderFallsBackThenIdentityPixelsAndBadShaderFifoAreReal()
        => RunPublicShaderFrames(40, 40, 21, true, 0, true, malformed: true);

    [TestMethod]
    public void WhenShaderChannelAndDeviceAreClosedThenNewPublicTargetRendersReplacementGeneration()
    {
        RunPublicShaderFrames(40, 40, 21, true, 0, true, replaceShader: true);
        RunPublicShaderFrames(40, 40, 21, true, 0, true, replaceShader: true);
    }

    private void RunPublicShaderFrames(int x, int y, uint format, bool shared, int callbackMode, bool shaderEffect,
        bool replaceShader = false, bool malformed = false)
    {
        string path = AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        var create = (delegate* unmanaged[Stdcall]<nint, double, double, uint, delegate* unmanaged[Stdcall]<int, uint, void>, int, nint*, uint*, uint*, int>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_Create");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var channelDestroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
        var dirty = (delegate* unmanaged[Stdcall]<int, int, int, int, nint, int>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_AddDirtyRect");
        nint window = CreateWindowExW(8, "STATIC", "Hardware D3DImage acceptance", 0x90000000,
            x, y, 32, 16, 0, 0, 0, 0);
        nint d3d = 0, device = 0, surface = 0, bitmap = 0, connection = 0, channel = 0;
        var detach = (delegate* unmanaged[Stdcall]<nint, void>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_Detach");
        int unavailableCallbacks = 0;
        uint unavailableVersion = 0;
        nint callbackBitmap = 0, callbackReplacement = 0, callbackSurface = 0;
        bool callerReferenceReleased = false;
        int replacementResult = int.MinValue;
        uint replacementWidth = 0, replacementHeight = 0;
        AvailabilityCallback availability = (available, version) =>
        {
            if (available != 0 || version != 8 || callbackBitmap == 0) return;
            unavailableVersion = version;
            Interlocked.Increment(ref unavailableCallbacks);
            if (callbackMode == 1)
            {
                nint created = 0;
                uint createdWidth = 0, createdHeight = 0;
                replacementResult = create(callbackSurface, 144, 192, 9, &Available, 1, &created, &createdWidth, &createdHeight);
                callbackReplacement = created;
                replacementWidth = createdWidth;
                replacementHeight = createdHeight;
            }
            detach(callbackBitmap);
            if (callbackMode == 2 || callbackMode == 3)
            {
                nint closing = callbackBitmap;
                callbackBitmap = 0;
                callerReferenceReleased = true;
                if (callbackMode == 3)
                    Task.Run(() => Release(closing)).GetAwaiter().GetResult();
                else
                    Release(closing);
            }
            else
            {
                ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)callbackBitmap)[1])(callbackBitmap);
                Release(callbackBitmap);
            }
        };
        var callback = (delegate* unmanaged[Stdcall]<int, uint, void>)Marshal.GetFunctionPointerForDelegate(availability);
        try
        {
            if (shared) Assert.AreEqual(0, Direct3DCreate9Ex(32, &d3d));
            else d3d = Direct3DCreate9(32);
            Assert.AreNotEqual(0, d3d);
            Assert.AreNotEqual(0, window);
            Assert.AreNotEqual(0, SetWindowPos(window, -1, x, y, 32, 16, 0x0040));
            SetForegroundWindow(window);
            UpdateWindow(window);
            ProcessWindowMessages(window);
            Assert.AreNotEqual(0, GetClientRect(window, out WindowRectangle client));
            WindowPoint center = new() { X = 16, Y = 8 };
            Assert.AreNotEqual(0, ClientToScreen(window, ref center));
            Console.WriteLine($"HW window=0x{window:X} visible={IsWindowVisible(window)} iconic={IsIconic(window)} foreground=0x{GetForegroundWindow():X} client={client.Right - client.Left}x{client.Bottom - client.Top} desktop=({center.X},{center.Y}) covering=0x{WindowFromPoint(center):X}");
            nint covering = WindowFromPoint(center);
            char* coveringClass = stackalloc char[256];
            int coveringClassLength = GetClassNameW(covering, coveringClass, 256);
            GetWindowRect(covering, out WindowRectangle coveringBounds);
            Console.WriteLine($"Covering class={new string(coveringClass, 0, coveringClassLength)} bounds=({coveringBounds.Left},{coveringBounds.Top},{coveringBounds.Right},{coveringBounds.Bottom})");
            Assert.AreNotEqual(0, IsWindowVisible(window));
            Assert.AreEqual(0, IsIconic(window));
            Assert.AreEqual(0, DwmFlush());
            PresentParameters parameters = new() { Width = 32, Height = 16, Count = 1, SwapEffect = 1, Window = window, Windowed = 1 };
            if (shared)
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint, nint*, int>)(*(void***)d3d)[20])(d3d, 0, 1, window, 0x26, &parameters, 0, &device));
            else
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)d3d)[16])(d3d, 0, 1, window, 0x26, &parameters, &device));
            DiagnoseNativePresent(window, device, shared);
            uint* caps = stackalloc uint[128];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)device)[7])(device, caps));
            Console.WriteLine($"Interop selection: Ex={shared}, format={format}, Caps2=0x{caps[3]:X8}, DevCaps2=0x{caps[53]:X8}, OS={Environment.OSVersion}");
            if (shared)
            {
                Assert.AreNotEqual(0u, caps[3] & 0x80000000u);
                Assert.AreNotEqual(0u, caps[53] & 0x10u);
            }
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, uint, int, nint*, nint, int>)(*(void***)device)[28])(device, 12, 1, format, 0, 0, shared ? 0 : 1, &surface, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, 0xff123456));
            uint width = 0, height = 0;
            Assert.AreEqual(0, create(surface, 96, 96, 7, &Available, 0, &bitmap, &width, &height));
            Assert.AreEqual(0, connect(1, &connection));
            Assert.AreEqual(0, channelCreate(connection, 0, &channel));
            uint image = 0, data = 0, visual = 0, destination = 0;
            Assert.AreEqual(0, resource(channel, 97, &image));
            Assert.AreEqual(0, resource(channel, 43, &data));
            Assert.AreEqual(0, resource(channel, 39, &visual));
            Assert.AreEqual(0, resource(channel, 46, &destination));
            byte* target = stackalloc byte[92];
            new Span<byte>(target, 92).Clear();
            *(uint*)target = 0x31; *(uint*)(target + 4) = destination;
            *(ulong*)(target + 8) = (ulong)window;
            *(uint*)(target + 32) = 32; *(uint*)(target + 36) = 16;
            *(float*)(target + 52) = 1;
            Assert.AreEqual(0, send(target, 92, 0, channel));
            ImagePacket update = new() { Type = 0x0a, Handle = image, Interop = (ulong)bitmap };
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)bitmap)[1])(bitmap);
            Assert.AreEqual(0, send(&update, (uint)sizeof(ImagePacket), 0, channel));
            DrawPacket draw = new() { Type = 0x18, Handle = data, DataSize = 48, RecordSize = 48, Command = 0x47, Width = 32, Height = 16, Image = image };
            Assert.AreEqual(0, send(&draw, (uint)sizeof(DrawPacket), 0, channel));
            uint* binding = stackalloc uint[] { 0x22, visual, data };
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            uint shaderHandle = shaderEffect ? BindPassThroughEffect(resource, send, channel, visual, malformed) : 0;
            if (malformed)
            {
                uint* registration = stackalloc uint[] { 3, 1 };
                Assert.AreEqual(0, send(registration, 8, 0, channel));
            }
            binding[0] = 0x35; binding[1] = destination; binding[2] = visual;
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            Assert.AreEqual(0, dirty(0, 0, 12, 1, bitmap));
            PresentPacket frame = new() { Type = 0x0b, Handle = image };
            Assert.AreEqual(0, send(&frame, (uint)sizeof(PresentPacket), 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual(0, DwmFlush());
            nint dc = GetDC(window);
            try { Assert.AreEqual(0x00563412u, GetPixel(dc, 16, 8)); }
            finally { ReleaseDC(window, dc); }
            ProcessWindowMessages(window);
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual(0, DwmFlush());
            dc = GetDC(window);
            try { Assert.AreEqual(0x00563412u, GetPixel(dc, 16, 8)); }
            finally { ReleaseDC(window, dc); }
            if (malformed)
            {
                var peek = (delegate* unmanaged[Stdcall]<nint, void*, nuint, int*, int>)NativeLibrary.GetExport(module, "MilComposition_PeekNextMessage");
                byte* message = stackalloc byte[80];
                int retrieved = 0;
                Assert.AreEqual(0, peek(channel, message, 4, &retrieved));
                Assert.AreEqual(1, retrieved);
                Assert.AreEqual(5u, *(uint*)message);
                Assert.AreEqual(0, peek(channel, message, 56, &retrieved));
                Assert.AreEqual(1, retrieved);
                Assert.AreEqual(0x10u, *(uint*)message);
                Assert.AreEqual(0, peek(channel, message, 80, &retrieved));
                Assert.AreEqual(0, retrieved);
            }
            if (replaceShader)
            {
                // Empty bytecode is the native legal identity policy, updating the same resource.
                uint* shaderUpdate = stackalloc uint[] { 0x6c, shaderHandle, 2, 0, 0 };
                Assert.AreEqual(0, send(shaderUpdate, 20, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, DwmFlush());
                dc = GetDC(window);
                try { Assert.AreEqual(0x00563412u, GetPixel(dc, 16, 8)); }
                finally { ReleaseDC(window, dc); }
            }
            // Reuse the public target with a changed consumer rectangle and new device pixels.
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, 0xffabcdef));
            draw.Width = 16;
            Assert.AreEqual(0, send(&draw, (uint)sizeof(DrawPacket), 0, channel));
            Assert.AreEqual(0, dirty(0, 0, 12, 1, bitmap));
            Assert.AreEqual(0, send(&frame, (uint)sizeof(PresentPacket), 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual(0, DwmFlush());
            dc = GetDC(window);
            try
            {
                Assert.AreEqual(0x00efcdabu, GetPixel(dc, 8, 8));
                Assert.AreEqual(0u, GetPixel(dc, 24, 8));
            }
            finally { ReleaseDC(window, dc); }
            nint replacement = 0;
            Assert.AreEqual(0, create(surface, 192, 192, 8, callback, 0, &replacement, &width, &height));
            Release(bitmap);
            bitmap = replacement;
            callbackBitmap = bitmap;
            update.Interop = (ulong)bitmap;
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)bitmap)[1])(bitmap);
            Assert.AreEqual(0, send(&update, (uint)sizeof(ImagePacket), 0, channel));
            Assert.AreEqual(0, dirty(0, 0, 12, 1, bitmap));
            Assert.AreEqual(0, send(&frame, (uint)sizeof(PresentPacket), 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual(0, DwmFlush());
            dc = GetDC(window);
            try { Assert.AreEqual(0x00efcdabu, GetPixel(dc, 8, 8)); }
            finally { ReleaseDC(window, dc); }
            callbackSurface = surface;
            Assert.AreEqual(0, channelDestroy(channel));
            channel = 0;
            Assert.AreEqual(0, disconnect(connection));
            connection = 0;
            Assert.AreEqual(1, unavailableCallbacks);
            Assert.AreEqual(8u, unavailableVersion);
            if (callbackMode == 1)
            {
                Assert.AreEqual(0, replacementResult);
                Assert.AreEqual((12u, 1u), (replacementWidth, replacementHeight));
                var snapshot = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_GetAsSoftwareBitmap");
                nint copy = 0;
                try
                {
                    Assert.AreEqual(0, snapshot(callbackReplacement, &copy));
                    uint pixel = 0;
                    int* rectangle = stackalloc int[4] { 0, 0, 1, 1 };
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, void*, uint, uint, void*, int>)(*(void***)copy)[7])(copy, rectangle, 4, 4, &pixel));
                    Assert.AreEqual(0xffabcdefu, pixel);
                }
                finally { Release(copy); }
            }
            if (callbackMode == 2 || callbackMode == 3)
                Assert.IsTrue(callerReferenceReleased);
        }
        finally
        {
            if (channel != 0) channelDestroy(channel);
            if (connection != 0) disconnect(connection);
            Release(callbackReplacement);
            if (!callerReferenceReleased) Release(bitmap);
            Release(surface); Release(device); Release(d3d);
            if (window != 0) DestroyWindow(window);
            GC.KeepAlive(availability);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LockedSurface { internal int Pitch; internal nint Bits; }

    private static void VerifyChannel(nint module, nint device, nint surface, nint bitmap, delegate* unmanaged[Stdcall]<nint, void> detach)
    {
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var channelDestroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
        var factoryCreate = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
        var targetCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateSWRenderTargetForBitmap");
        var dirty = (delegate* unmanaged[Stdcall]<int, int, int, int, nint, int>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_AddDirtyRect");
        nint connection = 0, channel = 0, factory = 0, target = 0, output = 0;
        using var wic = new SystemWicBitmap();
        try
        {
            Assert.AreEqual(0, connect(1, &connection));
            Assert.AreEqual(0, channelCreate(connection, 0, &channel));
            Assert.AreEqual(0, factoryCreate(&factory, 0x200184C0));
            output = wic.Create(12, 1);
            Assert.AreEqual(0, targetCreate(factory, output, &target));
            uint image = 0, data = 0, visual = 0, destination = 0;
            Assert.AreEqual(0, resource(channel, 97, &image));
            Assert.AreEqual(0, resource(channel, 43, &data));
            Assert.AreEqual(0, resource(channel, 39, &visual));
            Assert.AreEqual(0, resource(channel, 47, &destination));
            TargetPacket targetPacket = new() { Type = 0x34, Handle = destination, Target = (ulong)target, Width = 12, Height = 1 };
            Assert.AreEqual(0, send(&targetPacket, (uint)sizeof(TargetPacket), 0, channel));
            ImagePacket update = new() { Type = 0x0a, Handle = image, Interop = (ulong)bitmap };
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)bitmap)[1])(bitmap);
            Assert.AreEqual(0, send(&update, (uint)sizeof(ImagePacket), 0, channel));
            DrawPacket draw = new() { Type = 0x18, Handle = data, DataSize = 48, RecordSize = 48, Command = 0x47, Width = 12, Height = 1, Image = image };
            Assert.AreEqual(0, send(&draw, (uint)sizeof(DrawPacket), 0, channel));
            uint* binding = stackalloc uint[] { 0x22, visual, data };
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            binding[0] = 0x35; binding[1] = destination; binding[2] = visual;
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            using var completion = new EventWaitHandle(false, EventResetMode.ManualReset);
            nint transferred = 0;
            Assert.AreNotEqual(0, DuplicateHandle(-1, completion.SafeWaitHandle.DangerousGetHandle(), -1, &transferred, 0, 0, 2));
            PresentPacket frame = new() { Type = 0x0b, Handle = image, Event = (ulong)transferred };
            Assert.AreEqual(0, dirty(0, 0, 12, 1, bitmap));
            Assert.AreEqual(0, send(&frame, (uint)sizeof(PresentPacket), 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(1)));
            frame.Event = 0;
            Assert.AreEqual((0xff123456u, 0xff123456u), Read(output));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, 0xffabcdef));
            Assert.AreEqual(0, dirty(0, 0, 1, 1, bitmap));
            Assert.AreEqual(0, send(&frame, (uint)sizeof(PresentPacket), 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual((0xffabcdefu, 0xff123456u), Read(output));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, 0xff56789a));
            for (int i = 0; i < 5; i++) Assert.AreEqual(0, dirty(i * 2, 0, 1, 1, bitmap));
            for (int i = 0; i < 6; i++) Assert.AreEqual(0, dirty(0, 0, 1, 1, bitmap));
            Assert.AreEqual(0, send(&frame, (uint)sizeof(PresentPacket), 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            CollectionAssert.AreEqual(new uint[]
            {
                0xff56789a, 0xff123456, 0xff56789a, 0xff123456,
                0xff56789a, 0xff123456, 0xff56789a, 0xff123456,
                0xff56789a, 0xff123456, 0xff123456, 0xff123456
            }, ReadPixels(output));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, 0xffabcdef));
            for (int i = 0; i < 6; i++) Assert.AreEqual(0, dirty(i * 2, 0, 1, 1, bitmap));
            LockedSurface locked;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, LockedSurface*, nint, uint, int>)(*(void***)surface)[13])(surface, &locked, 0, 0));
            completion.Reset();
            transferred = 0;
            Assert.AreNotEqual(0, DuplicateHandle(-1, completion.SafeWaitHandle.DangerousGetHandle(), -1, &transferred, 0, 0, 2));
            frame.Event = (ulong)transferred;
            int failed;
            try
            {
                Assert.AreEqual(0, send(&frame, (uint)sizeof(PresentPacket), 0, channel));
                Assert.AreEqual(0, close(channel));
                failed = commit(channel);
            }
            finally { Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)surface)[14])(surface)); }
            Assert.AreEqual(0, failed);
            Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(1)));
            frame.Event = 0;
            CollectionAssert.AreEqual(new uint[]
            {
                0xff56789a, 0xff123456, 0xff56789a, 0xff123456,
                0xff56789a, 0xff123456, 0xff56789a, 0xff123456,
                0xff56789a, 0xff123456, 0xff123456, 0xff123456
            }, ReadPixels(output));
            Assert.AreEqual(0, send(&frame, (uint)sizeof(PresentPacket), 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            CollectionAssert.AreEqual(new uint[]
            {
                0xffabcdef, 0xffabcdef, 0xffabcdef, 0xffabcdef,
                0xffabcdef, 0xffabcdef, 0xffabcdef, 0xffabcdef,
                0xffabcdef, 0xffabcdef, 0xffabcdef, 0xff123456
            }, ReadPixels(output));
            detach(bitmap);
            Assert.AreEqual(0, send(&frame, (uint)sizeof(PresentPacket), 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual((0xffabcdefu, 0xffabcdefu), Read(output));
        }
        finally
        {
            if (channel != 0) Assert.AreEqual(0, channelDestroy(channel));
            if (connection != 0) Assert.AreEqual(0, disconnect(connection));
            Release(target); Release(output); Release(factory);
        }
    }

    private static (uint, uint) Read(nint bitmap)
    {
        uint[] pixels = ReadPixels(bitmap);
        return (pixels[0], pixels[1]);
    }

    private static uint[] ReadPixels(nint bitmap)
    {
        uint[] pixels = new uint[12];
        fixed (uint* buffer = pixels)
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)bitmap)[7])(bitmap, 0, 48, 48, buffer));
        return pixels;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TargetPacket { internal uint Type, Handle; internal ulong Window, Target; internal uint Width, Height, Dummy; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ImagePacket { internal uint Type, Handle; internal ulong Interop, Software; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PresentPacket { internal uint Type, Handle; internal ulong Event; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DrawPacket { internal uint Type, Handle, DataSize, RecordSize, Command; internal double X, Y, Width, Height; internal uint Image, Padding; }

    private static void Release(nint value)
    {
        if (value != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
    }
}
