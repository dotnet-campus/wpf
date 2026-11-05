using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

internal static partial class StrictRenderBusiness
{
    private static readonly Dictionary<string, object?> Evidence = new();
    private static readonly List<string> Events = new();
    private static readonly List<object> Requests = new();
    private static string output = "", selected = "", digest = "", stage = "bootstrap";
    private static nint module;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Run(string[] args)
    {
        if (args.Length != 4 || args[1] != "geometry-public-r27") return 64;
        selected = Path.GetFullPath(args[0]);
        output = Path.GetFullPath(args[2]);
        digest = args[3];
        Directory.CreateDirectory(output);
        try
        {
            Evidence["pid"] = Environment.ProcessId;
            Evidence["baseline"] = Modules();
            if (Modules().Any(p => Path.GetFileName(p).StartsWith("wpfgfx_", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Unexpected graphics module before authenticated scene execution.");
            Evidence["harness"] = Identity(typeof(StrictRenderBusiness).Assembly.Location);
            Evidence["mvid"] = typeof(StrictRenderBusiness).Module.ModuleVersionId;
            using var lease = File.OpenRead(selected);
            stage = "sha-pe-validation";
            if (digest.Length != 64) throw new InvalidDataException(stage);
            AcceptancePaths.ValidateIdentity(lease, digest);
            Evidence["candidate"] = Identity(selected);
            Evidence["processArchitecture"] = RuntimeInformation.ProcessArchitecture.ToString();
            Checkpoint("authenticated-before-WPF");
            EstablishNativeBoundary();
            InstallResolvers();
            Checkpoint("execute-wpf-before");
            ExecuteWpf();
            VerifyGraphicsModule();
            Evidence["exitCode"] = 0;
            Checkpoint("geometry-public-r27-completed");
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

    // No WPF types here: authenticate and load the selected native module before JITting the scene.
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
            return module;
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
        if (!SetDefaultDllDirectories(0x400 | 0x800) || AddDllDirectory(AppContext.BaseDirectory) == 0 || AddDllDirectory(Path.GetDirectoryName(selected)!) == 0)
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        module = NativeLibrary.Load(selected);
        VerifyGraphicsModule();
        if (NativeLibrary.Load("wpfgfx_cor3.dll") != module) throw new InvalidOperationException("Native basename resolved to another graphics module.");
        Evidence["nativeBasenameHandleMatches"] = true;
        Checkpoint("candidate-preloaded-before-WPF");
    }

    private static void VerifyGraphicsModule()
    {
        var path = new char[32768];
        uint length = GetModuleFileName(module, path, path.Length);
        if (length == 0 || length >= path.Length) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        string actual = new(path, 0, checked((int)length));
        if (!Path.GetFullPath(actual).Equals(selected, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unexpected actual graphics module path.");
        AcceptancePaths.ValidateDigest(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual))), digest);
        AcceptancePaths.ValidateCandidateModules(selected, Modules().Where(p => Path.GetFileName(p).StartsWith("wpfgfx_", StringComparison.OrdinalIgnoreCase)).ToArray());
        Evidence["actualNative"] = Identity(actual);
        Evidence["selectedHandle"] = module.ToString("X");
        Evidence["moduleEvidence"] = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Select(m => new { path = m.FileName, address = m.BaseAddress.ToString("X"), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(m.FileName))) }).ToArray();
    }

    private static string[] Modules() => Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Select(m => m.FileName).ToArray();
    private static object Identity(string path) { using var file = File.OpenRead(path); return new { path, sha256 = Convert.ToHexString(SHA256.HashData(file)), length = file.Length }; }
    private static void Checkpoint(string value)
    {
        lock (Evidence)
        {
            stage = value;
            if (value == "DrawingVisual-construction" || Events.Count > 0)
            {
                if (value != "graphics-import-request") Events.Add(value);
                Evidence["businessEvents"] = Events;
            }
            Evidence["stage"] = stage;
            Evidence["requests"] = Requests;
            File.WriteAllText(Path.Combine(output, "result.json"), JsonSerializer.Serialize(Evidence, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ExecuteWpf()
    {
        try { RenderGeometry(); }
        finally
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Evidence["cleanup"] = "Dispatcher shutdown; finalizers drained; native module retained until process exit";
        }
    }
}
