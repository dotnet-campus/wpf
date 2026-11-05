using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class MediaExportsTests
{
    public TestContext TestContext { get; set; } = null!;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate void FactoryContractDispose(FactoryContractDescriptor* descriptor);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int FactoryContractRaise(FactoryContractDescriptor* descriptor, byte* bytes, uint length);
    [StructLayout(LayoutKind.Sequential)]
    private struct FactoryContractDescriptor
    {
        internal nint Dispose, Raise, Handle;
    }

    /// <summary>Checks independent module identity rather than the former official-provider version coupling.</summary>
    [TestMethod]
    public void WhenCandidateCreatesMediaThenOnlyIndependentNativeModuleIsLoaded()
    {
        VerifyIndependentFactoryContract(1);
    }

    private static string[] FactoryContractModules()
    {
        using var process = Process.GetCurrentProcess();
        return process.Modules.Cast<ProcessModule>()
            .Select(module => Path.GetFullPath(module.FileName))
            .Where(path => string.Equals(Path.GetFileName(path), "wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private void VerifyIndependentFactoryContract(byte canOpen)
    {
        string path = Path.GetFullPath(AcceptancePaths.AotDll);
        string expectedSha = AcceptancePaths.ValidateIdentity();
        string[] before = FactoryContractModules();
        var calls = new List<object>();
        int disposals = 0;
        FactoryContractDispose dispose = descriptor => disposals++;
        FactoryContractRaise raise = (descriptor, bytes, length) => 0;
        nint factory = 0, proxy = 0, player = 0;
        uint release(nint value) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)value)[2])(value);
        uint addRef(nint value) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)value)[1])(value);
        void record(string operation, long result, nint output = 0) => calls.Add(new { operation, result, output = (long)output });
        void closePlayer()
        {
            if (player == 0) return;
            try
            {
                int hr = ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)player)[5])(player);
                record("player.Close", hr);
                Assert.AreEqual(0, hr);
            }
            finally
            {
                try
                {
                    int hr = ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)player)[22])(player);
                    record("player.Shutdown", hr);
                    Assert.AreEqual(0, hr);
                }
                finally
                {
                    uint remaining = release(player);
                    player = 0;
                    record("player.Release", remaining);
                    Assert.AreEqual(0u, remaining);
                }
            }
        }
        try
        {
            nint module = LoadCandidate();
            var createFactory = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
            var createMedia = (delegate* unmanaged[Stdcall]<nint, nint, byte, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateMediaPlayer");
            var createProxy = (delegate* unmanaged[Stdcall]<FactoryContractDescriptor*, nint*, int>)NativeLibrary.GetExport(module, "MILCreateEventProxy");
            Assert.AreEqual(0, createFactory(&factory, 0x200184C0));
            FactoryContractDescriptor descriptor = new() { Dispose = Marshal.GetFunctionPointerForDelegate(dispose), Raise = Marshal.GetFunctionPointerForDelegate(raise) };
            Assert.AreEqual(0, createProxy(&descriptor, &proxy));
            nint createdPlayer = 0;
            int hr = createMedia(factory, proxy, canOpen, &createdPlayer);
            player = createdPlayer;
            record("factory.Create", hr, player);
            Assert.AreEqual(0, hr);
            Assert.AreNotEqual((nint)0, player);
            closePlayer();
            Assert.AreEqual(unchecked((int)0x80004003), createMedia(factory, proxy, canOpen, null));
            foreach (int invalid in new[] { 0, 1, 2, 3 })
            {
                nint output = 0;
                hr = createMedia(invalid == 0 ? 0 : factory, invalid == 1 ? 0 : invalid == 3 ? factory : proxy,
                    invalid == 2 ? (byte)2 : canOpen, &output);
                player = output;
                record($"factory.Negative.{invalid}", hr, output);
                Assert.AreEqual((invalid == 3 ? unchecked((int)0x80004002) : unchecked((int)0x80070057), (nint)0), (hr, output));
            }
            uint proxyAdded = addRef(proxy);
            uint proxyRemaining = release(proxy);
            record("proxy.Rollback.AddRef", proxyAdded);
            record("proxy.Rollback.Release", proxyRemaining);
            Assert.AreEqual((2u, 1u), (proxyAdded, proxyRemaining));
            uint factoryAdded = addRef(factory);
            uint factoryRemaining = release(factory);
            record("factory.Rollback.AddRef", factoryAdded);
            record("factory.Rollback.Release", factoryRemaining);
            Assert.AreEqual((2u, 1u), (factoryAdded, factoryRemaining));
            string[] after = FactoryContractModules();
            CollectionAssert.AreEqual(new[] { path }, after);
            Assert.IsTrue(before.All(modulePath => string.Equals(modulePath, path, StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(expectedSha, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(after.Single()))));
            record("independentModule.NoSecondOfficial", 0);
        }
        finally
        {
            try { closePlayer(); }
            finally
            {
                try
                {
                    if (proxy != 0)
                    {
                        uint remaining = release(proxy);
                        proxy = 0;
                        record("proxy.Release.final", remaining);
                        Assert.AreEqual(0u, remaining);
                    }
                }
                finally
                {
                    try
                    {
                        if (factory != 0)
                        {
                            uint remaining = release(factory);
                            factory = 0;
                            record("factory.Release.final", remaining);
                            Assert.AreEqual(0u, remaining);
                        }
                        Assert.AreEqual(1, disposals);
                    }
                    finally
                    {
                        string root = Path.Combine(NativeRun.PathsRoot, "media-factory-contract");
                        Directory.CreateDirectory(root);
                        File.WriteAllText(Path.Combine(root, $"{TestContext.TestName}-{canOpen}.json"), JsonSerializer.Serialize(new
                        {
                            canOpen, calls, disposals, beforeModules = before, afterModules = FactoryContractModules(),
                            nativePath = path, nativeSha256 = expectedSha, runtime = Environment.Version.ToString(),
                            architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size,
                            testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(MediaExportsTests).Assembly.Location))),
                            testMvid = typeof(MediaExportsTests).Module.ModuleVersionId, businessSourceLinkUsed = false
                        }, new JsonSerializerOptions { WriteIndented = true }));
                        GC.KeepAlive(dispose);
                        GC.KeepAlive(raise);
                    }
                }
            }
        }
    }
}
