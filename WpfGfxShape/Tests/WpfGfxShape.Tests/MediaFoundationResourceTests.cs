using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class MediaFoundationResourceTests
{
    [TestMethod]
    public void WhenSystemSampleOwnsBufferThenBytesRoundTripAndResourcesClose()
    {
        Assert.IsTrue(OperatingSystem.IsWindows());
        byte[] input = [0, 17, 127, 128, 254, 255];
        byte[] output = new byte[input.Length];
        var operations = new List<object>();
        MediaFoundationSample? owner = null;
        bool completed = false;
        string root = Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT") ?? Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(root);
        string assembly = typeof(MediaFoundationResourceTests).Assembly.Location;
        void Save() => File.WriteAllText(Path.Combine(root, "media-foundation-resource.json"), JsonSerializer.Serialize(new
        {
            completed, runtime = RuntimeInformation.FrameworkDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            testAssembly = assembly, testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly))),
            testMvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId,
            implementation = typeof(MediaFoundationSample).Assembly.FullName,
            input, output, operations,
            owner?.ReleasedReferences, owner?.ShutdownCount, owner?.UnlockCount,
            owner?.SampleReleaseCount, owner?.BufferReleaseCount,
            modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
                .Where(module => string.Equals(module.ModuleName, "mfplat.dll", StringComparison.OrdinalIgnoreCase))
                .Select(module => new { module.FileName, version = module.FileVersionInfo.FileVersion }).ToArray()
        }, new JsonSerializerOptions { WriteIndented = true }));
        void Check(string operation, int hr, int expected = 0)
        {
            operations.Add(new { operation, hr = $"0x{hr:X8}" });
            Save();
            Assert.AreEqual(expected, hr, operation);
        }
        try
        {
            Check("MFStartup/CreateSample/CreateMemoryBuffer/AddBuffer", MediaFoundationSample.Create(32, out owner));
            Assert.IsNotNull(owner);
            Check("initial-lengths", owner.GetLengths(out uint maximum, out uint current, out uint total));
            operations.Add(new { operation = "initial-values", maximum, current, total });
            Assert.AreEqual(32u, maximum);
            Assert.AreEqual(0u, current);
            Assert.AreEqual(0u, total);
            Check("Lock/write/SetCurrentLength/Unlock", owner.Write(input));
            Check("written-lengths", owner.GetLengths(out maximum, out current, out total));
            operations.Add(new { operation = "written-values", maximum, current, total });
            Assert.AreEqual(6u, current);
            Assert.AreEqual(6u, total);
            Check("oversized-write", owner.Write(new byte[33]), unchecked((int)0x80070057));
            Check("undersized-read", owner.Read(new byte[5], out uint rejected), unchecked((int)0x8007007A));
            Assert.AreEqual(0u, rejected);
            Check("Lock/read/Unlock", owner.Read(output, out uint length));
            Assert.AreEqual(6u, length);
            CollectionAssert.AreEqual(input, output);
            Assert.AreEqual(4, owner.UnlockCount);
            Check("Release/Shutdown", owner.Close());
            Check("idempotent-close", owner.Close());
            Assert.AreEqual(2, owner.ReleasedReferences);
            Assert.AreEqual(1, owner.ShutdownCount);
            Assert.AreEqual(0u, owner.SampleReleaseCount);
            Assert.AreEqual(0u, owner.BufferReleaseCount);
            Assert.ThrowsExactly<ObjectDisposedException>(() => owner.GetLengths(out _, out _, out _));
            completed = true;
        }
        finally
        {
            int close = owner?.Close() ?? 0;
            operations.Add(new { operation = "finally-close", hr = $"0x{close:X8}" });
            Save();
            Assert.AreEqual(0, close);
        }
    }
}
