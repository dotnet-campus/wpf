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
#if GEOMETRY_R27
    private const string Scene = "geometry-public-r27";
#else
    private const string Scene = "purevideo-public";
#endif
    private static readonly Dictionary<string, object?> Evidence = new();
    private static string output = "";
    private static string stage = "bootstrap";
    private static nint module;
    private static string selected = "";
    private static readonly List<object> Requests = new();
#if STRICT_R28
    private static string strictDigest = "";
    private static readonly List<string> businessEvents = new();
#endif
    private static string ExpectedDigest()
    {
#if STRICT_R28
        return strictDigest;
#else
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "aot.sha256")).Trim();
#endif
    }

    [STAThread]
    private static int Main(string[] args)
    {
        output = Path.Combine(AppContext.BaseDirectory, "Evidence");
        Directory.CreateDirectory(output);
        try
        {
            Evidence["baseline"] = Modules();
            RejectPreloadedGraphics();
            Evidence["harness"] = Identity(typeof(Program).Assembly.Location);
            Evidence["mvid"] = typeof(Program).Module.ModuleVersionId;
#if STRICT_R28
            if (args.Length != 4 || args[1] != Scene) return 64;
            selected = Path.GetFullPath(args[0]);
            output = Path.GetFullPath(args[2]);
            Directory.CreateDirectory(output);
            strictDigest = args[3];
            Evidence["pid"] = Environment.ProcessId;
#else
            if (args.Length != 2 || args[0] != "candidate" || args[1] != Scene) return 64;
            selected = Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll");
#endif
            using var lease = File.OpenRead(selected);
            stage = "digest-required";
            string expected = ExpectedDigest();
            stage = "sha-validation";
            string actual = Convert.ToHexString(SHA256.HashData(lease));
            if (expected.Length != 64 || !actual.Equals(expected, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException(stage);
            stage = "pe-architecture-validation";
            lease.Position = 0;
            using var pe = new PEReader(lease, PEStreamOptions.LeaveOpen);
            Machine expectedMachine = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => Machine.Amd64,
                Architecture.X86 => Machine.I386,
                _ => throw new PlatformNotSupportedException(stage)
            };
            Evidence["processArchitecture"] = RuntimeInformation.ProcessArchitecture.ToString();
            if (pe.PEHeaders.CoffHeader.Machine != expectedMachine || pe.PEHeaders.PEHeader is null || pe.PEHeaders.CorHeader is not null) throw new BadImageFormatException(stage);
            Evidence["candidate"] = Identity(selected);
            Checkpoint("authenticated-before-WPF");
            EstablishNativeBoundary();
            InstallResolvers();
            Checkpoint("execute-wpf-before");
            ExecuteWpf();
            VerifyGraphicsModule();
            Evidence["exitCode"] = 0;
            Checkpoint(Scene == "purevideo-public" ? "purevideo-completed" : Scene + "-completed");
            return 0;
        }
        catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException)
        {
            Evidence["exception"] = error.ToString();
            Evidence["failureStage"] = stage;
            Evidence["exitCode"] = 1;
            Console.Error.WriteLine(error);
            return 1;
        }
        finally { Evidence["finalModules"] = Modules(); Checkpoint(stage); }
    }

    // No WPF types in this method or its caller; installation precedes the separately JITted scene.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void InstallResolvers()
    {
        stage = "resolver-installation";
        foreach (string name in new[] { "WindowsBase", "PresentationCore", "PresentationFramework" })
        {
            Assembly owner = Assembly.Load(name);
            Evidence[name] = Identity(owner.Location);
            NativeLibrary.SetDllImportResolver(owner, Resolve);
        }
        Evidence["preSceneModules"] = Modules();
        VerifyGraphicsModule();
        Checkpoint("resolver-installed-before-scene");
    }

    private static nint Resolve(string name, Assembly owner, DllImportSearchPath? flags)
    {
        if (!name.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)) return 0;
        lock (Evidence)
        {
            Requests.Add(new { name, owner = owner.FullName, flags, selected });
            Checkpoint("graphics-import-request");
            if (!name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) && !name.Equals("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase)) throw new DllNotFoundException(name);
            return module != 0 ? module : module = NativeLibrary.Load(selected);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetDefaultDllDirectories(uint flags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint AddDllDirectory(string path);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetModuleFileName(nint handle, [Out] char[] path, int size);

    private static void EstablishNativeBoundary()
    {
        Checkpoint("native-search-boundary");
#if !STRICT_R28
        string[] gfx = Directory.GetFiles(AppContext.BaseDirectory, "*", SearchOption.AllDirectories).Where(p => Path.GetFileName(p).Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (gfx.Length != 1 || !Path.GetFullPath(gfx[0]).Equals(Path.GetFullPath(selected), StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unexpected graphics package file.");
#endif
        // Exclude current directory, PATH and shared WPF installations even for native LoadLibrary calls.
        if (!SetDefaultDllDirectories(0x400 | 0x800) || AddDllDirectory(AppContext.BaseDirectory) == 0 || AddDllDirectory(Path.GetDirectoryName(selected)!) == 0) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        module = NativeLibrary.Load(selected);
        VerifyGraphicsModule();
        nint byName = NativeLibrary.Load("wpfgfx_cor3.dll");
        if (byName != module) throw new InvalidOperationException("Native basename resolved to another graphics module.");
        Evidence["nativeBasenameHandleMatches"] = true;
        Checkpoint("candidate-preloaded-before-WPF");
    }

    private static void VerifyGraphicsModule()
    {
        var path = new char[32768];
        uint length = GetModuleFileName(module, path, path.Length);
        if (length == 0 || length >= path.Length) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        string actual = new(path, 0, checked((int)length));
        if (!Path.GetFullPath(actual).Equals(Path.GetFullPath(selected), StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unexpected actual graphics module path.");
        string digest = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual)));
        if (digest != ExpectedDigest()) throw new InvalidDataException("Loaded graphics digest mismatch.");
        if (Modules().Any(p => Path.GetFileName(p).Contains("wpfgfx", StringComparison.OrdinalIgnoreCase) && !Path.GetFullPath(p).Equals(Path.GetFullPath(selected), StringComparison.OrdinalIgnoreCase))) throw new InvalidDataException("Unexpected graphics module.");
        Evidence["actualNative"] = Identity(actual);
#if STRICT_R28
        Evidence["selectedHandle"] = module.ToString("X");
        Evidence["moduleEvidence"] = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Select(m => new { path = m.FileName, address = m.BaseAddress.ToString("X"), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(m.FileName))) }).ToArray();
#endif
    }

    private static void RejectPreloadedGraphics()
    {
        if (Modules().Any(path => Path.GetFileName(path).Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Unexpected graphics module before authenticated scene execution.");
    }

    private static string[] Modules() => Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Select(m => m.FileName).ToArray();
    private static object Identity(string path) { using var file = File.OpenRead(path); return new { path, sha256 = Convert.ToHexString(SHA256.HashData(file)), length = file.Length }; }
    private static void Checkpoint(string value)
    {
        lock (Evidence)
        {
            stage = value;
#if STRICT_R28
            if (value == "DrawingVisual-construction" || businessEvents.Count > 0)
            {
                if (value != "graphics-import-request") businessEvents.Add(value);
                Evidence["businessEvents"] = businessEvents;
            }
#endif
            Evidence["stage"] = stage;
            Evidence["requests"] = Requests;
            File.WriteAllText(Path.Combine(output, "result.json"), JsonSerializer.Serialize(Evidence, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ExecuteWpf()
    {
        try
        {
#if GEOMETRY_R27
            RenderGeometryR27();
#else
            RenderPureVideoPublic();
#endif
        }
        finally
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Evidence["cleanup"] = "Dispatcher shutdown; finalizers drained; native module retained until process exit";
        }
    }
}
