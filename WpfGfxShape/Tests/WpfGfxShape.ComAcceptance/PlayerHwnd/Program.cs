namespace NativeAcceptance.PlayerHwnd;
using System.Runtime.InteropServices;
using System.Text.Json;

internal static unsafe class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint CreateWindowExW(uint extendedStyle, string className, string title, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyWindow(nint window);

    internal static int RunRole(string[] args)
    {
        if (args.Length != 2) return 2;
        args[1] = Path.GetFullPath(args[1], AppContext.BaseDirectory);
        if (args[0] != "reference") args[0] = Path.GetFullPath(args[0], AppContext.BaseDirectory);
        Directory.CreateDirectory(args[1]);
        try { return Run(args); }
        catch (SystemException error)
        {
            File.WriteAllText(Path.Combine(args[1], "failure.txt"), error.ToString());
            return 1;
        }
    }

    private static int Run(string[] args)
    {
        string library = args[0];
        if (library == "reference") library = WpfGfxShape.ComAcceptance.AcceptancePaths.OriginalDll;
        library = Path.GetFullPath(library);
        string sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(library)));
        nint dwm = NativeLibrary.Load(Path.Combine(Environment.SystemDirectory, "dwmapi.dll"));
        bool dwmAttachAvailable = NativeLibrary.TryGetExport(dwm, "DwmAttachMilContent", out _);
        bool dwmDetachAvailable = NativeLibrary.TryGetExport(dwm, "DwmDetachMilContent", out _);
        NativeLibrary.Free(dwm);
        nint module = NativeLibrary.Load(library);
        var results = new List<object>();
        nint window = CreateWindowExW(0, "STATIC", "MIL HWND acceptance", 0, 0, 0, 64, 64, 0, 0, 0, 0);
        if (window == 0) throw new System.ComponentModel.Win32Exception();
        bool destroyed = false;
        int failures = 0;
        try
        {
            nint attach = NativeLibrary.GetExport(module, "MilVisualTarget_AttachToHwnd");
            nint detach = NativeLibrary.GetExport(module, "MilVisualTarget_DetachFromHwnd");
            nint contentAttach = NativeLibrary.GetExport(module, "MilContent_AttachToHwnd");
            nint contentDetach = NativeLibrary.GetExport(module, "MilContent_DetachFromHwnd");
            void Check(string operation, int actual, int expected)
            {
                results.Add(new { operation, actual, expected, passed = actual == expected });
                if (actual != expected) failures++;
            }
            Check("null-detach-missing", Call(detach, 0), unchecked((int)0x80070057));
            Check("null-attach", Call(attach, 0), 0);
            Check("null-duplicate", Call(attach, 0), unchecked((int)0x80070005));
            Check("null-detach", Call(detach, 0), 0);
            Check("null-detach-repeat", Call(detach, 0), unchecked((int)0x80070057));
            Check("null-content-attach", Call(contentAttach, 0), 0);
            Check("null-content-detach", Call(contentDetach, 0), 0);
            Check("attach", Call(attach, window), 0);
            Check("duplicate", Call(attach, window), unchecked((int)0x80070005));
            Check("content-attach", Call(contentAttach, window), 0);
            Check("content-duplicate", Call(contentAttach, window), 0);
            int otherThread = 1;
            var thread = new Thread(() => otherThread = Call(detach, window));
            thread.Start();
            thread.Join();
            Check("cross-thread-detach", otherThread, 0);
            Check("detach-missing", Call(detach, window), unchecked((int)0x80070057));
            var attachingThread = new Thread(() => otherThread = Call(attach, window));
            attachingThread.Start();
            attachingThread.Join();
            Check("cross-thread-reattach", otherThread, 0);
            Check("content-detach", Call(contentDetach, window), 0);
            Check("content-detach-repeat", Call(contentDetach, window), 0);
            if (!DestroyWindow(window)) throw new System.ComponentModel.Win32Exception();
            destroyed = true;
            Check("destroy-does-not-release-registration", Call(attach, window), unchecked((int)0x80070005));
            Check("detach-after-destroy", Call(detach, window), 0);
            Check("detach-after-destroy-repeat", Call(detach, window), unchecked((int)0x80070057));
            Check("content-attach-after-destroy", Call(contentAttach, window), 0);
            Check("content-detach-after-destroy", Call(contentDetach, window), 0);
        }
        finally
        {
            if (!destroyed) DestroyWindow(window);
            NativeLibrary.Free(module);
            Directory.CreateDirectory(args[1]);
            File.WriteAllText(Path.Combine(args[1], "hwnd-report.json"), JsonSerializer.Serialize(new { mode = args[0], library, sha256, dwmAttachAvailable, dwmDetachAvailable, failures, results }, new JsonSerializerOptions { WriteIndented = true }));
        }
        return failures == 0 ? 0 : 1;
    }

    private static int Call(nint address, nint window) => ((delegate* unmanaged[Stdcall]<nint, int>)address)(window);
}
