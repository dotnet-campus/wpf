using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class MediaExportsTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void WhenCandidateCreatesMediaThenSystemModuleMatchesLoadedPresentationCoreRuntime()
    {
        using var source = new SystemMedia(candidate: true);
        string presentationCore = LoadedModulePath("PresentationCore.dll");
        string expected = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(presentationCore)!, "wpfgfx_cor3.dll"));
        string actual = LoadedModulePath(expected);
        Assert.AreEqual(expected, Path.GetFullPath(actual), true);
        TestContext.WriteLine($"PresentationCore: {presentationCore}; version: {FileVersionInfo.GetVersionInfo(presentationCore).FileVersion}");
        TestContext.WriteLine($"System media module: {actual}; version: {FileVersionInfo.GetVersionInfo(actual).FileVersion}");
        FileVersionInfo managedVersion = FileVersionInfo.GetVersionInfo(presentationCore);
        FileVersionInfo nativeVersion = FileVersionInfo.GetVersionInfo(actual);
        Assert.AreEqual((managedVersion.FileMajorPart, managedVersion.FileMinorPart, managedVersion.FileBuildPart, managedVersion.FilePrivatePart),
            (nativeVersion.FileMajorPart, nativeVersion.FileMinorPart, nativeVersion.FileBuildPart, nativeVersion.FilePrivatePart));
    }

    private static string LoadedModulePath(string name)
    {
        nint module = BindingGetModuleHandle(name);
        if (module == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        for (int capacity = 260; ; capacity = checked(capacity * 2))
        {
            char[] buffer = new char[capacity];
            uint length;
            fixed (char* chars = buffer) length = BindingGetModuleFileName(module, chars, (uint)capacity);
            if (length == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
            if (length < capacity) return new string(buffer, 0, (int)length);
        }
    }

    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint BindingGetModuleHandle(string name);

    [DllImport("kernel32.dll", EntryPoint = "GetModuleFileNameW", SetLastError = true)]
    private static extern uint BindingGetModuleFileName(nint module, char* path, uint size);
}
