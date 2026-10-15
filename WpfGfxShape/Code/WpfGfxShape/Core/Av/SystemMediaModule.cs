using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace WpfGfxShape.Core.Av;

internal static unsafe class SystemMediaModule
{
    private static readonly object Gate = new();
    private static readonly Dictionary<string, nint> Modules = new(StringComparer.OrdinalIgnoreCase);

    internal static nint Acquire()
    {
        lock (Gate)
        {
            nint presentationCore = GetModuleHandle("PresentationCore.dll");
            if (presentationCore == 0)
                throw new DllNotFoundException("PresentationCore.dll", new Win32Exception(Marshal.GetLastWin32Error()));
            string runtimePath = ModulePath(presentationCore);
            string path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(runtimePath)!, "wpfgfx_cor3.dll"));
            if (!GetModuleHandleEx(6, (nint)(delegate* unmanaged[Stdcall]<void>)&Identify, out nint self))
                throw new DllNotFoundException("wpfgfx_cor3.dll", new Win32Exception(Marshal.GetLastWin32Error()));
            string ownPath = ModulePath(self);
            if (FileIdentity(path) == FileIdentity(ownPath)) throw new DllNotFoundException("wpfgfx_cor3.dll");
            if (!Modules.TryGetValue(path, out nint handle))
            {
                handle = NativeLibrary.Load(path);
                // Intentionally process-owned: native providers, samples and asynchronous
                // event-proxy return frames can outlive the last managed player reference.
                // There is no FreeLibrary, including when Create or a callback fails.
                Modules.Add(path, handle);
            }
            return handle;
        }
    }

    private static string ModulePath(nint module)
    {
        for (int capacity = 260; ; capacity = checked(capacity * 2))
        {
            char[] buffer = new char[capacity];
            uint length;
            fixed (char* chars = buffer) length = GetModuleFileName(module, chars, (uint)capacity);
            if (length == 0)
                throw new DllNotFoundException("GetModuleFileNameW", new Win32Exception(Marshal.GetLastWin32Error()));
            if (length < capacity) return new string(buffer, 0, (int)length);
        }
    }

    private static (uint Volume, uint High, uint Low) FileIdentity(string path)
    {
        using SafeFileHandle file = File.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        if (!GetFileInformationByHandle(file, out FileInformation information))
            throw new IOException(path, Marshal.GetHRForLastWin32Error());
        return (information.Volume, information.IndexHigh, information.IndexLow);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileInformation
    {
        internal uint Attributes;
        internal System.Runtime.InteropServices.ComTypes.FILETIME Creation, Access, Write;
        internal uint Volume, SizeHigh, SizeLow, Links, IndexHigh, IndexLow;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Identify() { }

    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint GetModuleHandle(string name);

    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleExW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetModuleHandleEx(uint flags, nint address, out nint module);
    [DllImport("kernel32.dll", EntryPoint = "GetModuleFileNameW", SetLastError = true)]
    private static extern uint GetModuleFileName(nint module, char* path, uint size);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandle(SafeFileHandle file, out FileInformation information);
}
