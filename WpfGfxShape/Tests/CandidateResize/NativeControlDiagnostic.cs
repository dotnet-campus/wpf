using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    private static int diagnosticMessages;
    private static void DiagnosticMessage(nint window, uint message)
    {
        if (message is 0x0001 or 0x0002 or 0x0005 or 0x000f or 0x0014 or 0x0047 or 0x0082 && Interlocked.Increment(ref diagnosticMessages) <= 64)
            DiagnosticRecord("message", new { Window = (long)window, Message = message });
    }

    private static void DiagnosticRecord(string stage, object data)
    {
        string line = JsonSerializer.Serialize(new { Qpc = Stopwatch.GetTimestamp(), Frequency = Stopwatch.Frequency, Tid = GetCurrentThreadId(), Stage = stage, Data = data });
        File.AppendAllText(Path.Combine(AcceptancePaths.Results, "native-control-r9.jsonl"), line + Environment.NewLine);
    }

    private static void DiagnosticSample(nint window, string stage)
    {
        WindowPoint point = new() { X = 16, Y = 8 };
        int converted = ClientToScreen(window, ref point);
        uint owner = GetWindowThreadProcessId(window, out uint pid);
        DiagnosticRecord(stage + "-getdc-before", new { });
        nint windowDc = GetDC(window);
        DiagnosticRecord(stage + "-getdc-window-returned", new { Dc = (long)windowDc });
        nint screenDc = GetDC(0);
        DiagnosticRecord(stage + "-getpixel-window-before", new { Dc = (long)screenDc });
        try
        {
            uint windowPixel = GetPixel(windowDc, 16, 8);
            DiagnosticRecord(stage + "-getpixel-window-returned", new { Pixel = windowPixel });
            uint screenPixel = GetPixel(screenDc, point.X, point.Y);
            DiagnosticRecord(stage + "-getpixel-screen-returned", new { Pixel = screenPixel });
            GetClientRect(window, out WindowRectangle client);
            GetWindowRect(window, out WindowRectangle bounds);
            DiagnosticRecord(stage, new { Window = (long)window, OwnerTid = owner, OwnerPid = pid, Converted = converted, ScreenX = point.X, ScreenY = point.Y, WindowDc = (long)windowDc, ScreenDc = (long)screenDc, WindowPixel = windowPixel, ScreenPixel = screenPixel, Cover = (long)WindowFromPoint(point), Client = new[] { client.Left, client.Top, client.Right, client.Bottom }, Bounds = new[] { bounds.Left, bounds.Top, bounds.Right, bounds.Bottom } });
        }
        finally
        {
            int wr = windowDc != 0 ? ReleaseDC(window, windowDc) : 0;
            int sr = screenDc != 0 ? ReleaseDC(0, screenDc) : 0;
            DiagnosticRecord(stage + "-dc-released", new { Window = wr, Screen = sr });
        }
    }

    [DllImport("gdi32.dll")] private static extern uint SetPixel(nint dc, int x, int y, uint color);
    [DllImport("gdi32.dll")] private static extern int GdiFlush();
    private static void DiagnosticGdiControl(nint window)
    {
        DiagnosticRecord("post-oracle-gdi-control-before", new { });
        nint dc = GetDC(window);
        try
        {
            uint written = SetPixel(dc, 16, 8, 0x00563412);
            int flushed = GdiFlush();
            uint read = GetPixel(dc, 16, 8);
            DiagnosticRecord("post-oracle-gdi-control", new { Written = written, Flushed = flushed, Read = read });
        }
        finally { DiagnosticRecord("post-oracle-gdi-release", new { Result = ReleaseDC(window, dc) }); }
        DiagnosticSample(window, "post-oracle-gdi-double-dc");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DiagnosticCreation { internal uint Adapter, Type; internal nint Focus; internal uint Flags; }
    [StructLayout(LayoutKind.Sequential)]
    private struct DiagnosticSurface { internal uint Format, Type, Usage, Pool, Multisample, Quality, Width, Height; }

    [StructLayout(LayoutKind.Sequential)]
    private struct DiagnosticLockedSurface { internal int Pitch; internal nint Bits; }

    private static void DiagnosticClearedBuffer(nint device)
    {
        nint buffer = 0, staging = 0;
        try
        {
            int bufferResult = ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, nint*, int>)(*(void***)device)[18])(device, 0, 0, 0, &buffer);
            DiagnosticSurface desc = default;
            int descResult = buffer != 0 ? ((delegate* unmanaged[Stdcall]<nint, DiagnosticSurface*, int>)(*(void***)buffer)[12])(buffer, &desc) : -1;
            int createResult = descResult == 0 ? ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, nint*, nint, int>)(*(void***)device)[36])(device, desc.Width, desc.Height, desc.Format, 2, &staging, 0) : -1;
            int copyResult = createResult == 0 ? ((delegate* unmanaged[Stdcall]<nint, nint, nint, int>)(*(void***)device)[32])(device, buffer, staging) : -1;
            DiagnosticLockedSurface locked = default;
            int lockResult = copyResult == 0 ? ((delegate* unmanaged[Stdcall]<nint, DiagnosticLockedSurface*, nint, uint, int>)(*(void***)staging)[13])(staging, &locked, 0, 0x10) : -1;
            uint? pixel = null;
            int? unlockResult = null;
            if (lockResult == 0)
            {
                try { pixel = *(uint*)((byte*)locked.Bits + 8 * locked.Pitch + 16 * sizeof(uint)); }
                finally { unlockResult = ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)staging)[14])(staging); }
            }
            DiagnosticRecord("clear-before-present-staging-diagnostic-only", new { bufferResult, descResult, createResult, copyResult, lockResult, unlockResult, desc.Width, desc.Height, desc.Format, locked.Pitch, pixel });
        }
        finally { Release(staging); Release(buffer); }
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint DiagnosticWindowLong(nint window, int index);
    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleExW", CharSet = CharSet.Unicode)]
    private static extern int DiagnosticModule(uint flags, nint address, out nint module);

    private static object DiagnosticFunction(nint instance, int slot)
    {
        nint address = (nint)(*(void***)instance)[slot];
        int found = DiagnosticModule(6, address, out nint module);
        char* path = stackalloc char[1024];
        uint length = found != 0 ? GetModuleFileNameW(module, path, 1024) : 0;
        return new { Slot = slot, Address = (long)address, Module = (long)module, Found = found, Path = new string(path, 0, (int)length) };
    }

    private static void DiagnosticImplicitSwapChain(nint window, nint device)
    {
        nint chain = 0, fromDevice = 0, fromChain = 0, returnedDevice = 0, deviceIdentity = 0, chainIdentity = 0;
        try
        {
            int chainResult = ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)device)[14])(device, 0, &chain);
            DiagnosticRecord("r10-implicit-chain-acquired-diagnostic-only", new { chainResult, Chain = (long)chain, Style = (long)DiagnosticWindowLong(window, -16), ExStyle = (long)DiagnosticWindowLong(window, -20), PresentSize = sizeof(PresentParameters), WindowOffset = (int)Marshal.OffsetOf<PresentParameters>(nameof(PresentParameters.Window)), WindowedOffset = (int)Marshal.OffsetOf<PresentParameters>(nameof(PresentParameters.Windowed)), CreationSize = sizeof(DiagnosticCreation), SurfaceSize = sizeof(DiagnosticSurface), LockedSize = sizeof(DiagnosticLockedSurface), BitsOffset = (int)Marshal.OffsetOf<DiagnosticLockedSurface>(nameof(DiagnosticLockedSurface.Bits)), DevicePresent = DiagnosticFunction(device, 17), DeviceBackBuffer = DiagnosticFunction(device, 18), DeviceSwapChain = DiagnosticFunction(device, 14) });
            if (chainResult != 0 || chain == 0) return;
            int deviceBufferResult = ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, nint*, int>)(*(void***)device)[18])(device, 0, 0, 0, &fromDevice);
            int chainBufferResult = ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint*, int>)(*(void***)chain)[5])(chain, 0, 0, &fromChain);
            int deviceResult = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)chain)[8])(chain, &returnedDevice);
            Guid iid = new("00000000-0000-0000-C000-000000000046");
            int deviceIdentityResult = fromDevice != 0 ? ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)fromDevice)[0])(fromDevice, &iid, &deviceIdentity) : -1;
            int chainIdentityResult = fromChain != 0 ? ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)fromChain)[0])(fromChain, &iid, &chainIdentity) : -1;
            PresentParameters parameters = default;
            int parametersResult = ((delegate* unmanaged[Stdcall]<nint, PresentParameters*, int>)(*(void***)chain)[9])(chain, &parameters);
            DiagnosticRecord("r10-implicit-chain-identity-diagnostic-only", new { deviceBufferResult, chainBufferResult, deviceResult, deviceIdentityResult, chainIdentityResult, parametersResult, Device = (long)device, ReturnedDevice = (long)returnedDevice, DeviceBuffer = (long)fromDevice, ChainBuffer = (long)fromChain, DeviceBufferIdentity = (long)deviceIdentity, ChainBufferIdentity = (long)chainIdentity, SameBufferIdentity = deviceIdentity != 0 && deviceIdentity == chainIdentity, SameDevice = returnedDevice == device, parameters.Width, parameters.Height, parameters.Format, parameters.Count, parameters.Multisample, parameters.Quality, parameters.SwapEffect, Window = (long)parameters.Window, parameters.Windowed, parameters.AutoDepth, parameters.DepthFormat, parameters.Flags, parameters.Refresh, parameters.Interval, ChainPresent = DiagnosticFunction(chain, 3), ChainBackBuffer = DiagnosticFunction(chain, 5), ChainParameters = DiagnosticFunction(chain, 9) });
            if (deviceBufferResult != 0 || chainBufferResult != 0 || deviceResult != 0 || parametersResult != 0 || deviceIdentity == 0 || deviceIdentity != chainIdentity || returnedDevice != device || parameters.Window != window) return;
            int clearResult = ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)device)[43])(device, 0, 0, 1, 0xff123456, 1, 0);
            DiagnosticRecord("r10-chain-clear-diagnostic-only", new { clearResult });
            if (clearResult != 0) return;
            DiagnosticClearedBuffer(device);
            int presentResult = ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, uint, int>)(*(void***)chain)[3])(chain, 0, 0, window, 0, 0);
            DiagnosticRecord("r10-chain-present-diagnostic-only", new { presentResult, Override = (long)window, SourceRect = 0, DestinationRect = 0, DirtyRegion = 0, Flags = 0 });
            if (presentResult == 0) CompleteNativePresent(device, window);
            DiagnosticSample(window, "r10-chain-consumer-diagnostic-only-original-failure-retained");
        }
        finally
        {
            Release(chainIdentity); Release(deviceIdentity); Release(returnedDevice); Release(fromChain); Release(fromDevice); Release(chain);
            DiagnosticRecord("r10-chain-released-original-failure-retained", new { });
        }
    }

    private static void DiagnosticDevice(nint window, nint device)
    {
        DiagnosticCreation creation = default;
        int creationResult = ((delegate* unmanaged[Stdcall]<nint, DiagnosticCreation*, int>)(*(void***)device)[9])(device, &creation);
        nint buffer = 0;
        int bufferResult = ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, nint*, int>)(*(void***)device)[18])(device, 0, 0, 0, &buffer);
        try
        {
            DiagnosticSurface desc = default;
            int descResult = buffer != 0 ? ((delegate* unmanaged[Stdcall]<nint, DiagnosticSurface*, int>)(*(void***)buffer)[12])(buffer, &desc) : -1;
            DiagnosticRecord("device", new { Window = (long)window, CreationResult = creationResult, Focus = (long)creation.Focus, Flags = creation.Flags, Adapter = creation.Adapter, Type = creation.Type, BufferResult = bufferResult, DescResult = descResult, Width = desc.Width, Height = desc.Height, Format = desc.Format });
        }
        finally { Release(buffer); }
    }
}
