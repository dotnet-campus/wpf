using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed class ReplacementRecoverySessionTests
{
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint OpenInputDesktop(uint flags, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint access);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseDesktop(nint desktop);

    /// <summary>Records actual session prerequisites without treating capability observations as successful device recovery.</summary>
    [TestMethod]
    public void WhenRecoveryPrerequisitesAreObservedThenSessionEvidenceIsSaved()
    {
        string root = Path.Combine(AcceptancePaths.Results, "replacement-recovery-z", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        nint desktop = OpenInputDesktop(0, false, 1);
        int error = Marshal.GetLastWin32Error();
        bool closed = desktop == 0 || CloseDesktop(desktop);
        using Process process = Process.GetCurrentProcess();
        File.WriteAllText(Path.Combine(root, "session.json"), JsonSerializer.Serialize(new
        {
            process.SessionId, Environment.UserInteractive,
            RemoteSession = GetSystemMetrics(0x1000), RemoteControl = GetSystemMetrics(0x2001),
            InputDesktopOpened = desktop != 0, InputDesktopError = desktop == 0 ? error : 0, InputDesktopClosed = closed,
            Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(), PointerSize = IntPtr.Size,
            RecoveryVerified = false,
            Contract = "Run existing WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover separately. Second device failure is not first device loss. Existing probe has no post-reset pixel oracle. No reset scripts or system process changes."
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.IsTrue(closed, root);
    }
}
