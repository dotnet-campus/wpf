using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace NativeAcceptance.Pcore;

internal static partial class Program
{
    private static readonly Dictionary<string, object?> Evidence = new();
    private static readonly List<object> Requests = new();
    private static string output = "", selected = "", stage = "bootstrap";
    private static nint module;
    private static bool finalized;

    [STAThread]
    private static int Main(string[] args)
    {
        output = Path.Combine(AppContext.BaseDirectory, "Evidence");
        Directory.CreateDirectory(output);
        try
        {
            Evidence["baseline"] = Modules();
            if (Modules().Any(IsGraphics)) throw new InvalidOperationException("Graphics already loaded.");
            Evidence["harness"] = Identity(typeof(Program).Assembly.Location);
            Evidence["mvid"] = typeof(Program).Module.ModuleVersionId;
            if (args.Length != 2 || args[0] != "candidate" || args[1] != "purevideo-public") return 64;
            selected = Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll");
            using var lease = File.OpenRead(selected);
            stage = "digest-required";
            string expected = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "aot.sha256")).Trim();
            stage = "sha-validation";
            if (expected.Length != 64 || Convert.ToHexString(SHA256.HashData(lease)) != expected) throw new InvalidDataException(stage);
            stage = "pe-architecture-validation";
            lease.Position = 0;
            using var pe = new PEReader(lease, PEStreamOptions.LeaveOpen);
            Evidence["processArchitecture"] = RuntimeInformation.ProcessArchitecture.ToString();
            Machine machine = RuntimeInformation.ProcessArchitecture == Architecture.X86 ? Machine.I386 : Machine.Amd64;
            if (pe.PEHeaders.CoffHeader.Machine != machine || pe.PEHeaders.PEHeader is null || pe.PEHeaders.CorHeader is not null) throw new BadImageFormatException(stage);
            Evidence["candidate"] = Identity(selected);
            string[] gfx = Directory.GetFiles(AppContext.BaseDirectory, "*", SearchOption.AllDirectories).Where(IsGraphics).ToArray();
            if (gfx.Length != 1 || Path.GetFullPath(gfx[0]) != Path.GetFullPath(selected)) throw new InvalidDataException("Unexpected package graphics.");
            if (!SetDefaultDllDirectories(0x400 | 0x800) || AddDllDirectory(AppContext.BaseDirectory) == 0 || AddDllDirectory(Path.GetDirectoryName(selected)!) == 0) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            module = NativeLibrary.Load(selected);
            if (NativeLibrary.Load("wpfgfx_cor3.dll") != module) throw new InvalidDataException("Unexpected basename resolution.");
            Evidence["nativeBasenameHandleMatches"] = true;
            foreach (string name in new[] { "WindowsBase", "PresentationCore", "PresentationFramework" })
                NativeLibrary.SetDllImportResolver(Assembly.Load(name), Resolve);
            ExecuteWpf();
            VerifyGraphicsModule();
            lock (Evidence)
            {
                Evidence["exitCode"] = 0;
                Checkpoint("purevideo-completed");
                finalized = true;
            }
            return 0;
        }
        catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException)
        {
            lock (Evidence)
            {
                Evidence["exception"] = error.ToString();
                Evidence["failureStage"] = stage;
                Evidence["exitCode"] = 1;
                Checkpoint(stage);
                finalized = true;
            }
            Console.Error.WriteLine(error);
            return 1;
        }
        finally
        {
            lock (Evidence) { Evidence["finalModules"] = Modules(); Checkpoint(stage); }
        }
    }

    private static nint Resolve(string name, Assembly owner, DllImportSearchPath? flags)
    {
        if (!name.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)) return 0;
        lock (Evidence)
        {
            Requests.Add(new { name, owner = owner.FullName, flags, selected });
            if (!name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) && !name.Equals("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase)) throw new DllNotFoundException(name);
            Checkpoint(finalized ? stage : "graphics-import-request");
            return module;
        }
    }

    private static void VerifyGraphicsModule()
    {
        char[] buffer = new char[32768];
        uint length = GetModuleFileName(module, buffer, buffer.Length);
        if (length == 0 || length >= buffer.Length) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        string actual = new(buffer, 0, checked((int)length));
        if (!Path.GetFullPath(actual).Equals(Path.GetFullPath(selected), StringComparison.OrdinalIgnoreCase)
            || Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual))) != File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "aot.sha256")).Trim()
            || Modules().Where(IsGraphics).Any(p => !Path.GetFullPath(p).Equals(Path.GetFullPath(selected), StringComparison.OrdinalIgnoreCase))) throw new InvalidDataException("Unexpected loaded graphics.");
        Evidence["actualNative"] = Identity(actual);
    }

    private static bool IsGraphics(string path) => Path.GetFileName(path).Contains("wpfgfx", StringComparison.OrdinalIgnoreCase);
    private static string[] Modules() => Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Select(m => m.FileName).ToArray();
    private static object Identity(string path) { using var file = File.OpenRead(path); return new { path, sha256 = Convert.ToHexString(SHA256.HashData(file)), length = file.Length }; }
    private static void Checkpoint(string value)
    {
        lock (Evidence)
        {
            stage = value;
            Evidence["stage"] = stage;
            Evidence["requests"] = Requests;
            File.WriteAllText(Path.Combine(output, "result.json"), JsonSerializer.Serialize(Evidence, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ExecuteWpf()
    {
        try { RenderPureVideoPublic(); }
        finally { System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown(); }
    }
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetDefaultDllDirectories(uint flags);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint AddDllDirectory(string path);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetModuleFileName(nint handle, [Out] char[] path, int size);
}
