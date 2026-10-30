using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int AllowSetForegroundWindow(uint processId);

    internal static int RunExclusiveFocusRole()
    {
        using var owner = new PresentationWindowThread();
        windowThread = owner;
        nint window = CreateOutputWindow("D3D9 focus displacement control", 100, 40);
        try
        {
            Console.WriteLine($"{window}");
            Console.Out.Flush();
            if (Console.ReadLine() != "activate") return 3;
            owner.Invoke(() =>
            {
                ShowWindow(window, 5);
                int result = SetForegroundWindow(window);
                LogExclusiveForeground("child-activate", window, result);
                Console.Out.Flush();
                return 0;
            });
            string? command = Console.ReadLine();
            if (command is null || !command.StartsWith("return ", StringComparison.Ordinal)) return 4;
            uint parent = uint.Parse(command[7..], System.Globalization.CultureInfo.InvariantCulture);
            int granted = AllowSetForegroundWindow(parent);
            int error = granted == 0 ? Marshal.GetLastWin32Error() : 0;
            Console.WriteLine($"DeviceLossForeground returnGrant={granted} succeeded={granted != 0} error={error} parentPid={parent} childPid={Environment.ProcessId} foreground=0x{GetForegroundWindow():X}");
            Console.Out.Flush();
            if (granted == 0) return 5;
            return Console.ReadLine() == "release" ? 0 : 4;
        }
        finally { DestroyWindow(window); windowThread = null; }
    }

    [DllImport("kernel32.dll", EntryPoint = "ProcessIdToSessionId", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ExclusiveSession(uint pid, out uint session);

    private static readonly System.Resources.ResourceManager ExclusiveResources = new("WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(D3DImageTests).Assembly);

    private static void LogExclusiveForeground(string phase, nint target, int result)
    {
        nint foreground = GetForegroundWindow();
        uint thread = GetWindowThreadProcessId(foreground, out uint pid);
        bool sessionOk = ExclusiveSession(pid, out uint session);
        int sessionError = sessionOk ? 0 : Marshal.GetLastWin32Error();
        using var current = Process.GetCurrentProcess();
        ExitDiagnostics.Stage($"DeviceLossForeground phase={phase} setForeground={result} target=0x{target:X} foreground=0x{foreground:X} foregroundPid={pid} foregroundThread={thread} foregroundSession={session} sessionQuery={sessionOk} sessionError={sessionError} callerPid={Environment.ProcessId} callerSession={current.SessionId} sameProcess={pid == (uint)Environment.ProcessId}");
        Console.WriteLine($"DeviceLossForeground phase={phase}  setForeground={result} target=0x{target:X} foreground=0x{foreground:X} foregroundPid={pid} foregroundThread={thread} foregroundSession={session} sessionQuery={sessionOk} sessionError={sessionError} callerPid={Environment.ProcessId} callerSession={current.SessionId} sameProcess={pid == (uint)Environment.ProcessId}");
    }

    private static void EstablishExclusiveForeground(nint window, string phase)
    {
        windowThread!.Invoke(() =>
        {
            ShowWindow(window, 9);
            Assert.AreNotEqual(0, SetWindowPos(window, -1, 0, 0, 0, 0, 0x0043));
#if SANDBOX_FOREGROUND_CONTROL_DIAGNOSTIC
            if (phase == "initial") SandboxForegroundControl.BeforeInitial();
#endif
            if (phase == "initial") ForegroundTopology.Capture("initial-before-activation");
            int result = SetForegroundWindow(window);
            LogExclusiveForeground(phase, window, result);
            if (phase == "initial") ForegroundTopology.Capture("initial-after-activation");
            Assert.AreNotEqual(0, result, ExclusiveResources.GetString("ExclusiveForegroundPrerequisite"));
            Assert.AreEqual(window, GetForegroundWindow(), ExclusiveResources.GetString("ExclusiveForegroundPrerequisite"));
            UpdateWindow(window);
            ProcessWindowMessages(window);
            return 0;
        });
    }

    private static string ReadFocusLine(Process process)
    {
        Task<string?> read = process.StandardOutput.ReadLineAsync();
        Assert.IsTrue(read.Wait(TimeSpan.FromSeconds(15)), "Focus role protocol timed out.");
        return read.Result ?? throw new InvalidOperationException("Focus role exited before protocol completion.");
    }

    private static int ExclusiveCooperative(nint device)
        => windowThread!.Invoke(() => ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)device)[3])(device));

    private static int ExclusivePresent(nint device)
        => windowThread!.Invoke(() => ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, int>)(*(void***)device)[17])(device, 0, 0, 0, 0));

    private static void RunExclusiveFocusLoss()
    {
        nint window = CreateOutputWindow("Exclusive device loss probe", 40, 40);
        nint factory = 0, device = 0;
        Process? trigger = null;
        try
        {
            factory = windowThread!.Invoke(() => Direct3DCreate9(32));
            Assert.AreNotEqual(0, factory);
            NativeDisplayMode mode = windowThread.Invoke(() =>
            {
                NativeDisplayMode value = default;
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, NativeDisplayMode*, int>)(*(void***)factory)[8])(factory, 0, &value));
                return value;
            });
            EstablishExclusiveForeground(window, "initial");
            PresentParameters parameters = new() { Width = mode.Width, Height = mode.Height, Format = mode.Format, Count = 1, SwapEffect = 1, Window = window, Windowed = 0 };
            int created = windowThread.Invoke(() => CreateExclusiveDevice(factory, window, parameters, out device));
            Console.WriteLine($"DeviceLossProbe firstCreate=0x{created:X8} owner={windowThread.Invoke(GetCurrentThreadId)} mode={mode.Width}x{mode.Height} format={mode.Format} focus=0x{GetForegroundWindow():X}");
            Assert.AreEqual(0, created);
            windowThread.Invoke(() => { VerifyExclusiveParameters(device, window); VerifyExclusiveFrame(device, mode, 0xff123456); return 0; });
            Assert.AreEqual(0, ExclusivePresent(device));
            Assert.AreEqual(window, GetForegroundWindow());
            var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException())
            { UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
            start.ArgumentList.Add("--exclusive-focus-role");
            trigger = Process.Start(start) ?? throw new InvalidOperationException();
            nint triggerWindow = nint.Parse(ReadFocusLine(trigger), System.Globalization.CultureInfo.InvariantCulture);
            int allowed = AllowSetForegroundWindow((uint)trigger.Id);
            int allowError = allowed == 0 ? Marshal.GetLastWin32Error() : 0;
            Console.WriteLine($"DeviceLossProbe allowForeground={allowed} error={allowError} triggerPid={trigger.Id} triggerWindow=0x{triggerWindow:X}");
            Assert.AreNotEqual(0, allowed);
            trigger.StandardInput.WriteLine("activate"); trigger.StandardInput.Flush();
            Console.WriteLine(ReadFocusLine(trigger));
            Assert.AreEqual(triggerWindow, GetForegroundWindow(), "NotCovered: displacement HWND did not become foreground.");
            int present = ExclusivePresent(device);
            int lost = ExclusiveCooperative(device);
            Console.WriteLine($"DeviceLossProbe displacedPresent=0x{present:X8} cooperative=0x{lost:X8}");
            Assert.AreEqual(unchecked((int)0x88760868), present, "NotCovered: no real device loss.");
            Assert.AreEqual(unchecked((int)0x88760868), lost);
            trigger.StandardInput.WriteLine($"return {Environment.ProcessId}"); trigger.StandardInput.Flush();
            string grant = ReadFocusLine(trigger);
            Console.WriteLine(grant);
            Assert.IsTrue(grant.Contains(" succeeded=True ", StringComparison.Ordinal), ExclusiveResources.GetString("ExclusiveForegroundPrerequisite"));
            EstablishExclusiveForeground(window, "recovery");
            trigger.StandardInput.WriteLine("release"); trigger.StandardInput.Flush();
            Assert.IsTrue(trigger.WaitForExit(15000));
            Console.WriteLine($"DeviceLossProbe triggerExit={trigger.ExitCode} stderr={trigger.StandardError.ReadToEnd()}");
            Assert.AreEqual(0, trigger.ExitCode);
            Console.WriteLine($"DeviceLossProbe recoveryBefore iconic={IsIconic(window)} visible={IsWindowVisible(window)}");
            Assert.AreEqual(window, GetForegroundWindow(), ExclusiveResources.GetString("ExclusiveForegroundPrerequisite"));
            Console.WriteLine($"DeviceLossProbe recoveryAfter iconic={IsIconic(window)} visible={IsWindowVisible(window)}");
            long deadline = Environment.TickCount64 + 5000;
            do { lost = ExclusiveCooperative(device); Thread.Yield(); }
            while (lost == unchecked((int)0x88760868) && Environment.TickCount64 < deadline);
            Console.WriteLine($"DeviceLossProbe beforeReset=0x{lost:X8} foreground=0x{GetForegroundWindow():X} defaultPoolReferences=0");
            Assert.AreEqual(unchecked((int)0x88760869), lost);
            int reset = windowThread.Invoke(() => ResetExclusiveDevice(device, parameters));
            Console.WriteLine($"DeviceLossProbe reset=0x{reset:X8}");
            Assert.AreEqual(0, reset);
            Assert.AreEqual(0, ExclusiveCooperative(device));
            windowThread.Invoke(() => { VerifyExclusiveFrame(device, mode, 0xff654321); return 0; });
            Assert.AreEqual(0, ExclusivePresent(device));
        }
        finally
        {
            if (trigger is not null)
            {
                if (!trigger.HasExited) { trigger.Kill(true); Assert.IsTrue(trigger.WaitForExit(15000)); }
                Console.WriteLine($"DeviceLossProbe cleanup triggerExit={trigger.ExitCode}");
                trigger.Dispose();
            }
            windowThread!.Invoke(() => { Release(device); Release(factory); return 0; });
            DestroyWindow(window);
            Console.WriteLine("DeviceLossProbe cleanup device/factory/window complete");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ExclusiveCreationParameters
    {
        internal uint Adapter, Type;
        internal nint Focus;
        internal uint Flags;
    }

    private static void VerifyExclusiveParameters(nint device, nint window)
    {
        ExclusiveCreationParameters creation = default;
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, ExclusiveCreationParameters*, int>)(*(void***)device)[9])(device, &creation));
        nint chain = 0;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)device)[14])(device, 0, &chain));
            PresentParameters actual = default;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, PresentParameters*, int>)(*(void***)chain)[9])(chain, &actual));
            ExitDiagnostics.Stage($"DeviceLossProbe actual adapter={creation.Adapter} type={creation.Type} focus=0x{creation.Focus:X} flags=0x{creation.Flags:X} windowed={actual.Windowed} window=0x{actual.Window:X} size={actual.Width}x{actual.Height} format={actual.Format} visible={IsWindowVisible(window)} iconic={IsIconic(window)} foreground=0x{GetForegroundWindow():X}");
            Assert.AreEqual(0, actual.Windowed);
            Assert.AreEqual(window, creation.Focus);
            Assert.AreEqual(window, actual.Window);
            Assert.AreEqual(0u, creation.Adapter);
            Assert.AreEqual(1u, creation.Type);
        }
        finally { Release(chain); }
    }

    private static void VerifyExclusiveFrame(nint device, NativeDisplayMode mode, uint expected)
    {
        nint back = 0, staging = 0;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)device)[43])(device, 0, 0, 1, expected, 1, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, nint*, int>)(*(void***)device)[18])(device, 0, 0, 0, &back));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, nint*, nint, int>)(*(void***)device)[36])(device, mode.Width, mode.Height, mode.Format, 2, &staging, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, int>)(*(void***)device)[32])(device, back, staging));
            LockedSurface locked = default;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, LockedSurface*, nint, uint, int>)(*(void***)staging)[13])(staging, &locked, 0, 0x10));
            uint actual;
            try { actual = *(uint*)locked.Bits; }
            finally { Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)staging)[14])(staging)); }
            ExitDiagnostics.Stage($"DeviceLossProbe newSurface expected=0x{expected:X8} actual=0x{actual:X8}");
            Console.WriteLine($"DeviceLossProbe newSurface expected=0x{expected:X8} actual=0x{actual:X8}");
            Assert.AreEqual(expected & 0xffffff, actual & 0xffffff);
        }
        finally { Release(staging); Release(back); }
    }
}
