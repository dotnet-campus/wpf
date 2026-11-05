using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class IndependentNativeAbiTests
{
    private sealed record Call(string Operation, long Result, long Output);
    private sealed record ModuleIdentity(string Path, string Sha256, long BaseAddress);
    private readonly List<Call> _calls = [];
    private sealed class State
    {
        internal int Calls, Disposals;
        internal byte[] Packet = [];
        internal nint Instance;
        internal bool ReleaseInCallback;
        internal uint CallbackRelease;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate void DisposeCallback(Descriptor* descriptor);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int RaiseCallback(Descriptor* descriptor, byte* bytes, uint length);
    [StructLayout(LayoutKind.Sequential)]
    private struct Descriptor
    {
        internal nint Dispose, Raise, Handle;
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private static ModuleIdentity[] Modules()
    {
        using var process = Process.GetCurrentProcess();
        return process.Modules.Cast<ProcessModule>()
            .Where(module => Path.GetFileName(module.FileName).Contains("wpfgfx", StringComparison.OrdinalIgnoreCase))
            .Select(module => new ModuleIdentity(Path.GetFullPath(module.FileName), Hash(module.FileName), (long)module.BaseAddress)).ToArray();
    }
    private static uint Release(nint instance) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)instance)[2])(instance);
    private static uint AddRef(nint instance) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)instance)[1])(instance);
    private long Record(string operation, long result, long output = 0)
    {
        _calls.Add(new(operation, result, output));
        return result;
    }

    /// <summary>Authenticates the stage13 production DLL and exercises only its supported factory safety gate and event ABI.</summary>
    [TestMethod]
    public void WhenStage14ProductionNativeIsLoadedThenFactoryGateAndEventAbiAreReal()
    {
        string root = Path.Combine(NativeRun.PathsRoot, "stage14-native-abi");
        Directory.CreateDirectory(root);
        string path = AcceptancePaths.AotDll;
        string sha = AcceptancePaths.ValidateIdentity();
        ModuleIdentity[] before = Modules();
        var exports = new List<object>();
        var state = new State();
        GCHandle handle = GCHandle.Alloc(state);
        DisposeCallback dispose = descriptor => ((State)GCHandle.FromIntPtr(descriptor->Handle).Target!).Disposals++;
        RaiseCallback raise = (descriptor, bytes, length) =>
        {
            var target = (State)GCHandle.FromIntPtr(descriptor->Handle).Target!;
            target.Calls++;
            target.Packet = new ReadOnlySpan<byte>(bytes, checked((int)length)).ToArray();
            if (target.ReleaseInCallback) target.CallbackRelease = Release(target.Instance);
            return unchecked((int)0x80004005);
        };
        nint factory = 0, proxy = 0, player = 0, queried = 0;
        bool callbackOwner = false;
        bool completed = false;
        Machine machine;
        using (var stream = File.OpenRead(path))
        using (var pe = new PEReader(stream)) machine = pe.PEHeaders.CoffHeader.Machine;
        try
        {
            Assert.AreEqual("10.0.13", Environment.Version.ToString());
            (int pointerSize, Machine expectedMachine) = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => (4, Machine.I386),
                Architecture.X64 => (8, Machine.Amd64),
                _ => throw new PlatformNotSupportedException()
            };
            Assert.AreEqual(pointerSize, IntPtr.Size);
            Assert.AreEqual(expectedMachine, machine);
            Assert.AreEqual(3 * pointerSize, sizeof(Descriptor));
            Assert.AreEqual((nint)pointerSize, Marshal.OffsetOf<Descriptor>(nameof(Descriptor.Raise)));
            Assert.AreEqual((nint)(2 * pointerSize), Marshal.OffsetOf<Descriptor>(nameof(Descriptor.Handle)));
            nint module = NativeLibrary.Load(path);
            using var process = Process.GetCurrentProcess();
            string actual = Path.GetFullPath(process.Modules.Cast<ProcessModule>().Single(item => item.BaseAddress == module).FileName);
            Assert.IsTrue(string.Equals(path, actual, StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(sha, Hash(actual));
            File.WriteAllText(Path.Combine(root, "identity.json"), JsonSerializer.Serialize(new
            {
                requested = path, actual, sha256 = sha, machine = machine.ToString(), module = (long)module,
                runtime = Environment.Version.ToString(), architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size,
                wrapperPath = typeof(IndependentNativeAbiTests).Assembly.Location,
                wrapperSha256 = Hash(typeof(IndependentNativeAbiTests).Assembly.Location),
                wrapperMvid = typeof(IndependentNativeAbiTests).Module.ModuleVersionId,
                businessSourceLinkUsed = false
            }, new JsonSerializerOptions { WriteIndented = true }));
            string[] names = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Contracts", "wpfgfx.def"))
                .Select(line => line.Trim()).Where(line => line.Length > 0 && !line.StartsWith(';') && line != "EXPORTS" && !line.StartsWith("LIBRARY ", StringComparison.Ordinal)).ToArray();
            foreach (string name in names)
            {
                bool found = NativeLibrary.TryGetExport(module, name, out nint address);
                exports.Add(new { name, found, address = (long)address, expected = true });
                Assert.IsTrue(found, name);
            }
            foreach (string name in new[] { "WpfGfxShape_RuntimeHeapDiagnostic_v1", "WpfGfxShape_NativeAotAbiProbe_v1" })
            {
                bool found = NativeLibrary.TryGetExport(module, name, out nint address);
                exports.Add(new { name, found, address = (long)address, expected = false });
                Assert.IsFalse(found, name);
            }
            var create = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
            var media = (delegate* unmanaged[Stdcall]<nint, nint, byte, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateMediaPlayer");
            var events = (delegate* unmanaged[Stdcall]<Descriptor*, nint*, int>)NativeLibrary.GetExport(module, "MILCreateEventProxy");
            Assert.AreEqual(0L, Record("MILCreateFactory", create(&factory, 0x200184C0), factory));
            Assert.AreNotEqual((nint)0, factory);
            Descriptor descriptor = new() { Dispose = Marshal.GetFunctionPointerForDelegate(dispose), Raise = Marshal.GetFunctionPointerForDelegate(raise), Handle = GCHandle.ToIntPtr(handle) };
            Assert.AreEqual(0L, Record("MILCreateEventProxy", events(&descriptor, &proxy), proxy));
            Assert.AreNotEqual((nint)0, proxy);
            state.Instance = proxy;
            foreach (byte canOpen in new byte[] { 0, 1 })
            {
                int hr = media(factory, proxy, canOpen, &player);
                Record($"MILFactoryCreateMediaPlayer({canOpen})", hr, player);
                Assert.AreEqual(0, hr);
                Assert.AreNotEqual((nint)0, player);
                Assert.AreEqual(0L, Record("player.Close", ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)player)[5])(player)));
                Assert.AreEqual(0L, Record("player.Shutdown", ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)player)[22])(player)));
                uint remaining = Release(player);
                player = 0;
                Assert.AreEqual(0L, Record("player.Release.final", remaining));
                Assert.AreEqual(2L, Record("proxy.AddRef.afterFactory", AddRef(proxy)));
                Assert.AreEqual(1L, Record("proxy.Release.afterFactory", Release(proxy)));
            }
            nint output = -1;
            Assert.AreEqual((long)unchecked((int)0x80004003), Record("factory.nullOutput", media(factory, proxy, 1, null)));
            int nullFactory = media(0, proxy, 1, &output);
            Record("factory.nullFactory", nullFactory, output);
            Assert.AreEqual((unchecked((int)0x80070057), (nint)0), (nullFactory, output));
            output = -1;
            int invalidCanOpen = media(factory, proxy, 2, &output);
            Record("factory.invalidCanOpen", invalidCanOpen, output);
            Assert.AreEqual((unchecked((int)0x80070057), (nint)0), (invalidCanOpen, output));
            output = -1;
            int nullClient = media(factory, 0, 1, &output);
            Record("factory.nullClient", nullClient, output);
            Assert.AreEqual((unchecked((int)0x80070057), (nint)0), (nullClient, output));
            // A real factory is a valid IUnknown that rejects IMILEventProxy QI.
            output = -1;
            int rejectedClient = media(factory, factory, 1, &output);
            Record("factory.rejectedClient", rejectedClient, output);
            Assert.AreEqual((unchecked((int)0x80004002), (nint)0), (rejectedClient, output));
            Assert.AreEqual(2L, Record("factory.AddRef.afterRejectedClient", AddRef(factory)));
            Assert.AreEqual(1L, Record("factory.Release.afterRejectedClient", Release(factory)));
            Assert.AreEqual(2L, Record("proxy.AddRef.afterRejectedClient", AddRef(proxy)));
            Assert.AreEqual(1L, Record("proxy.Release.afterRejectedClient", Release(proxy)));
            foreach (Guid iid in new[] { new Guid("342efd8b-669a-4d16-b163-d75f5ffd1a10"), new Guid("00000000-0000-0000-C000-000000000046") })
            {
                Guid id = iid;
                queried = 0;
                var query = (delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(nint**)proxy)[0];
                int hr = query(proxy, &id, &queried);
                Record($"proxy.QueryInterface({iid})", hr, queried);
                Assert.AreEqual((0, proxy), (hr, queried));
                uint remaining = Release(queried);
                queried = 0;
                Assert.AreEqual(1L, Record("proxy.Release.query", remaining));
            }
            // AV event packet: event, HRESULT, two UTF-16 lengths, trailing two WCHARs.
            byte[] packet = new byte[20];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(packet, 8);
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(4), unchecked((int)0x80070002));
            var dispatch = (delegate* unmanaged[Stdcall]<nint, byte*, uint, int>)(*(nint**)proxy)[3];
            fixed (byte* bytes = packet)
                Assert.AreEqual((long)unchecked((int)0x80004005), Record("proxy.RaiseEvent.packet", dispatch(proxy, bytes, (uint)packet.Length)));
            CollectionAssert.AreEqual(packet, state.Packet);
            uint callbackReferences = AddRef(proxy);
            callbackOwner = true;
            Assert.AreEqual(2L, Record("proxy.AddRef.callbackOwner", callbackReferences));
            state.ReleaseInCallback = true;
            fixed (byte* bytes = packet)
                Assert.AreEqual((long)unchecked((int)0x80004005), Record("proxy.RaiseEvent.reentrantRelease", dispatch(proxy, bytes, (uint)packet.Length)));
            callbackOwner = false;
            state.ReleaseInCallback = false;
            Record("proxy.callback.Release", state.CallbackRelease);
            // RaiseEvent retains one reference until the callback has returned (EventProxy.cs).
            Assert.AreEqual(2u, state.CallbackRelease);
            Assert.AreEqual(2L, Record("proxy.AddRef.afterDispatch", AddRef(proxy)));
            Assert.AreEqual(1L, Record("proxy.Release.afterDispatch", Release(proxy)));
            Assert.AreEqual(0, state.Disposals);
            uint finalProxy = Release(proxy);
            proxy = 0;
            Assert.AreEqual(0L, Record("proxy.Release.final", finalProxy));
            Assert.AreEqual(1, state.Disposals);
            uint finalFactory = Release(factory);
            factory = 0;
            Assert.AreEqual(0L, Record("factory.Release.final", finalFactory));
            ModuleIdentity[] after = Modules();
            Assert.AreEqual(1, after.Count(item => string.Equals(item.Path, path, StringComparison.OrdinalIgnoreCase)));
            Assert.IsFalse(after.Any(item => !string.Equals(item.Path, path, StringComparison.OrdinalIgnoreCase) && !before.Any(old => old.Path == item.Path)));
            completed = true;
        }
        finally
        {
            if (player != 0)
            {
                try
                {
                    Record("player.Close.cleanup", ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)player)[5])(player));
                }
                finally
                {
                    try { Record("player.Shutdown.cleanup", ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)player)[22])(player)); }
                    finally { Record("player.Release.cleanup", Release(player)); }
                }
            }
            if (queried != 0) Record("proxy.Release.query.cleanup", Release(queried));
            if (callbackOwner && state.CallbackRelease == 0) Record("proxy.Release.callbackOwner.cleanup", Release(proxy));
            if (proxy != 0) Record("proxy.Release.cleanup", Release(proxy));
            if (factory != 0) Record("factory.Release.cleanup", Release(factory));
            File.WriteAllText(Path.Combine(root, "business.json"), JsonSerializer.Serialize(new
            {
                completed, calls = _calls, callbackCalls = state.Calls, descriptorDisposals = state.Disposals,
                packetHex = Convert.ToHexString(state.Packet), beforeModules = before, afterModules = Modules(),
                limitations = "No media playback; callback release is non-final; no COM Shutdown slot on IMILEventProxy; preexisting root modules are not attributed to media factory."
            }, new JsonSerializerOptions { WriteIndented = true }));
            File.WriteAllText(Path.Combine(root, "exports.json"), JsonSerializer.Serialize(exports, new JsonSerializerOptions { WriteIndented = true }));
            GC.KeepAlive(dispose);
            GC.KeepAlive(raise);
            handle.Free();
        }
    }
}
