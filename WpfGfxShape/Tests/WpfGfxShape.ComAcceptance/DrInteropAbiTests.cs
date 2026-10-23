#if DR_INTEROP_ABI
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class DrInteropAbiTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Parameters
    {
        internal uint Width, Height, Format, Count, Multisample, Quality, SwapEffect;
        internal nint Window;
        internal int Windowed, AutoDepth;
        internal uint DepthFormat, Flags, Refresh, Interval;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void Available([MarshalAs(UnmanagedType.Bool)] bool available, uint version);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int Create(nint surface, double dpiX, double dpiY, uint version, Available callback,
        [MarshalAs(UnmanagedType.Bool)] bool fallback, out nint bitmap, out uint width, out uint height);

    [DllImport("d3d9.dll", ExactSpelling = true)]
    private static extern nint Direct3DCreate9(uint version);
    [DllImport("user32.dll")]
    private static extern nint GetDesktopWindow();
    [DllImport("kernel32.dll", EntryPoint = "GetModuleFileNameW", ExactSpelling = true, SetLastError = true)]
    private static extern uint GetModuleFileName(nint module, char* name, uint size);

    private static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private static uint Release(nint value) => value == 0 ? 0 :
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);

    /// <summary>Exercises independent Create and IUnknown declarations without a composition channel or Present.</summary>
    [TestMethod]
    public void WhenDrOfficialInteropCreateRunsThenIndependentAbiAndLifetimeAreRecorded() => Run(true);

    /// <summary>Runs the candidate in a separately selected supervised process.</summary>
    [TestMethod]
    public void WhenDrCandidateInteropCreateRunsThenIndependentAbiAndLifetimeAreRecorded() => Run(false);

    private static void Run(bool official)
    {
        string label = official ? "official" : "candidate";
        string root = NativeRun.PathsRoot;
        Directory.CreateDirectory(root);
        string trace = Path.Combine(root, $"dr-{label}.jsonl");
        void Record(string stage, object? data = null)
        {
            using var stream = new FileStream(trace, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(stream, leaveOpen: true);
            writer.WriteLine(JsonSerializer.Serialize(new { Utc = DateTimeOffset.UtcNow, Pid = Environment.ProcessId,
                Thread = Environment.CurrentManagedThreadId, Stage = stage, Data = data }));
            writer.Flush();
            stream.Flush(true);
            ExitDiagnostics.Stage($"DR {label} {stage}");
        }
        Assert.AreEqual(IntPtr.Size == 4 ? 56 : 64, sizeof(Parameters));
        Assert.AreEqual(IntPtr.Size == 4 ? 28 : 32, Marshal.OffsetOf<Parameters>(nameof(Parameters.Window)).ToInt32());
        string requested = official ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(requested);
        char[] name = new char[32768];
        string actual;
        fixed (char* buffer = name)
        {
            uint length = GetModuleFileName(module, buffer, (uint)name.Length);
            if (length == 0)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
            if (length >= (uint)name.Length)
                throw new System.ComponentModel.Win32Exception(122);
            actual = new string(buffer, 0, checked((int)length));
        }
        Assert.AreEqual(Path.GetFullPath(requested), Path.GetFullPath(actual), true);
        string assembly = typeof(DrInteropAbiTests).Assembly.Location;
        Record("identity", new { Requested = requested, Actual = actual, ModuleSha256 = Sha(actual),
            FileVersion = FileVersionInfo.GetVersionInfo(actual).FileVersion,
            ProductVersion = FileVersionInfo.GetVersionInfo(actual).ProductVersion,
            TestSha256 = Sha(assembly), TestMvid = typeof(DrInteropAbiTests).Module.ModuleVersionId,
            Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            PointerSize = IntPtr.Size, Wpf = typeof(System.Windows.Media.Visual).Assembly.FullName,
            ParametersSize = sizeof(Parameters), Callback = "Winapi BOOL32 UINT32; no forced adapter transition" });
        var create = Marshal.GetDelegateForFunctionPointer<Create>(NativeLibrary.GetExport(module, "InteropDeviceBitmap_Create"));
        var detach = (delegate* unmanaged[Stdcall]<nint, void>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_Detach");
        var dirty = (delegate* unmanaged[Stdcall]<int, int, int, int, nint, int>)NativeLibrary.GetExport(module, "InteropDeviceBitmap_AddDirtyRect");
        int callbacks = 0;
        var notifications = new System.Collections.Concurrent.ConcurrentQueue<(bool Available, uint Version, int Thread)>();
        Available callback = (available, version) =>
        {
            notifications.Enqueue((available, version, Environment.CurrentManagedThreadId));
            Interlocked.Increment(ref callbacks);
        };
        GCHandle callbackRoot = GCHandle.Alloc(callback);
        nint factory = 0, device = 0, surface = 0, bitmap = 0;
        try
        {
            factory = Direct3DCreate9(32);
            Assert.AreNotEqual(0, factory);
            Parameters parameters = new() { Width = 12, Height = 3, Count = 1, SwapEffect = 1,
                Window = GetDesktopWindow(), Windowed = 1 };
            Record("CreateDevice before");
            int hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, Parameters*, nint*, int>)(*(void***)factory)[16])
                (factory, 0, 1, parameters.Window, 0x26, &parameters, &device);
            Record("CreateDevice after", new { Hr = hr });
            Assert.AreEqual(0, hr);
            hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, uint, int, nint*, nint, int>)(*(void***)device)[28])
                (device, 12, 3, 21, 0, 0, 1, &surface, 0);
            Record("CreateRenderTarget after", new { Hr = hr });
            Assert.AreEqual(0, hr);
            hr = ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, 0xff123456);
            Record("ColorFill after", new { Hr = hr });
            Assert.AreEqual(0, hr);
            for (uint iteration = 0; iteration < 3; iteration++)
            {
                uint version = 0x91a20007 + iteration;
                Record("Create before", new { Iteration = iteration, Version = version, Fallback = false, DpiX = 111.5, DpiY = 123.25 });
#if FH_CRASH_EVIDENCE
                if (!official && iteration == 0) FhCrashEvidence.CandidateReady(module, actual);
#endif
                hr = create(surface, 111.5, 123.25, version, callback, false, out bitmap, out uint width, out uint height);
                Record("Create after", new { Hr = hr, Bitmap = (long)bitmap, Width = width, Height = height, Callbacks = callbacks });
                Assert.AreEqual(0, hr);
                Assert.AreEqual((12u, 3u), (width, height));
                Assert.AreNotEqual(0, bitmap);
                Guid iid = new("00000000-0000-0000-c000-000000000046");
                nint identity = 0;
                hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)bitmap)[0])(bitmap, &iid, &identity);
                Record("QueryIUnknown after", new { Hr = hr, Identity = (long)identity });
                Assert.AreEqual(0, hr);
                Assert.AreNotEqual(0, identity);
                Record("QueryIUnknown Release", new { Count = Release(identity) });
                uint count = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)bitmap)[1])(bitmap);
                Record("AddRef and balancing Release", new { AddRef = count, Release = Release(bitmap) });
                GC.Collect();
                GC.WaitForPendingFinalizers();
                hr = dirty(0, 0, 12, 3, bitmap);
                Record("Dirty after GC", new { Hr = hr });
                Assert.AreEqual(0, hr);
                Record("Detach before");
                detach(bitmap);
                Record("Detach after");
                uint final = Release(bitmap);
                bitmap = 0;
                Record("Bitmap final Release", new { Count = final });
                Assert.AreEqual(0u, final);
            }
            Record("callbacks", new { Count = callbacks, Notifications = notifications.Select(n => new { n.Available, n.Version, n.Thread }).ToArray(),
                NativeInvocationCovered = callbacks != 0, ConsumerCovered = false });
            Assert.IsTrue(notifications.All(n => n.Version >= 0x91a20007 && n.Version <= 0x91a20009));
            Assert.IsFalse(VersionMatches(0x91a20006));
            Record("negative control", new { RejectedStaleVersion = true });
        }
        finally
        {
            if (bitmap != 0) { Record("cleanup Detach before"); detach(bitmap); Record("cleanup bitmap Release", new { Count = Release(bitmap) }); }
            Record("surface Release", new { Count = Release(surface) });
            Record("device Release", new { Count = Release(device) });
            Record("factory Release", new { Count = Release(factory) });
            GC.KeepAlive(callback);
            callbackRoot.Free();
            Record("callback root freed; module resident");
        }
        Record("completed");
    }

    private static bool VersionMatches(uint version) => version >= 0x91a20007 && version <= 0x91a20009;
}
#endif
