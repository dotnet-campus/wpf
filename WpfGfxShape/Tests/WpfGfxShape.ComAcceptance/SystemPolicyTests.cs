using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class SystemPolicyTests
{
    private sealed record Observation(string Name, long Value);

    [TestMethod]
    public void WhenProcessPoliciesAreChangedAndRestoredThenOriginalAndCandidateAgree()
    {
        string reference = Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll");
        var original = Observe(reference);
        var candidate = Observe(AcceptancePaths.AotDll);
        File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"system-policy-{Guid.NewGuid():N}.json"),
            JsonSerializer.Serialize(new { Reference = Identity(reference), Candidate = Identity(AcceptancePaths.AotDll), Original = original, Actual = candidate }, new JsonSerializerOptions { WriteIndented = true }));
        CollectionAssert.AreEqual(original, candidate);
    }

    private static object Identity(string path) => new { Path = path, Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), FileVersion = FileVersionInfo.GetVersionInfo(path).FileVersion, Runtime = RuntimeInformation.FrameworkDescription };

    private static List<Observation> Observe(string path)
    {
        nint module = NativeLibrary.Load(path);
        var set = (delegate* unmanaged[Stdcall]<int, void>)NativeLibrary.GetExport(module, "RenderOptions_ForceSoftwareRenderingModeForProcess");
        var get = (delegate* unmanaged[Stdcall]<int>)NativeLibrary.GetExport(module, "RenderOptions_IsSoftwareRenderingForcedForProcess");
        var rdp = (delegate* unmanaged[Stdcall]<int, void>)NativeLibrary.GetExport(module, "RenderOptions_EnableHardwareAccelerationInRdp");
        var update = (delegate* unmanaged[Stdcall]<int>)NativeLibrary.GetExport(module, "MILUpdateSystemParametersInfo");
        var stream = (delegate* unmanaged[Stdcall]<int>)NativeLibrary.GetExport(module, "WgxConnection_ShouldForceSoftwareForGraphicsStreamClient");
        var nextId = (delegate* unmanaged[Stdcall]<long>)NativeLibrary.GetExport(module, "GetNextPerfElementId");
        var load = (delegate* unmanaged[Stdcall]<char*, byte**, int*, int>)NativeLibrary.GetExport(module, "MILLoadResource");
        int previous = get();
        List<Observation> results = [];
        try
        {
            foreach (int value in new[] { 0, 1, -1, 2, 0 })
            {
                set(value);
                results.Add(new($"force-{value}", get()));
                using var observed = new ManualResetEventSlim();
                int otherThread = -1;
                var worker = new Thread(() => { otherThread = get(); observed.Set(); });
                worker.Start();
                Assert.IsTrue(observed.Wait(5000));
                Assert.IsTrue(worker.Join(5000));
                results.Add(new($"thread-{value}", otherThread));
                rdp(value);
                results.Add(new($"update-{value}", update()));
            }
            results.Add(new("graphics-stream", stream()));
            long first = nextId(), second = nextId();
            results.Add(new("id-increment", second - first));
            byte* memory = (byte*)123;
            int size = 456;
            fixed (char* name = "WpfGfxShape.Nonexistent.Image.Resource")
                results.Add(new("resource-missing", load(name, &memory, &size)));
            results.Add(new("resource-failure-preserves-pointer", (long)memory));
            results.Add(new("resource-failure-preserves-size", size));
        }
        finally
        {
            set(previous);
            rdp(0);
        }
        results.Add(new("restored", get() == previous ? 1 : 0));
        return results;
    }

    [TestMethod]
    public void WhenPixelCopiesHaveDifferentBitOffsetsThenOriginalAndCandidateAgree()
    {
        string reference = Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll");
        var original = ObservePixels(reference);
        var candidate = ObservePixels(AcceptancePaths.AotDll);
        File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"pixel-copy-{Guid.NewGuid():N}.json"),
            JsonSerializer.Serialize(new { Reference = Identity(reference), Candidate = Identity(AcceptancePaths.AotDll), Original = original, Actual = candidate }, new JsonSerializerOptions { WriteIndented = true }));
        CollectionAssert.AreEqual(original, candidate);
    }

    private static List<string> ObservePixels(string path)
    {
        nint module = NativeLibrary.Load(path);
        var copy = (delegate* unmanaged[Stdcall]<byte*, uint, uint, uint, byte*, uint, uint, uint, uint, uint, int>)NativeLibrary.GetExport(module, "MilUtility_CopyPixelBuffer");
        List<string> results = [];
        byte[] source = [0x96, 0x3C, 0xF0, 0xA5, 0x69, 0xC3, 0x0F, 0x5A];
        for (uint inputOffset = 0; inputOffset < 8; inputOffset++)
        for (uint outputOffset = 0; outputOffset < 8; outputOffset++)
        for (uint width = 1; width <= 17; width++)
        {
            byte[] destination = [0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA];
            fixed (byte* input = source, output = destination)
                results.Add($"{copy(output, 8, 4, outputOffset, input, 8, 4, inputOffset, 2, width):X8}:{Convert.ToHexString(destination)}");
        }
        fixed (byte* input = source)
        {
            results.Add(copy(null, 0, 0, 99, null, 0, 0, 99, 0, 1).ToString());
            results.Add(copy(input, 8, 4, 8, input, 8, 4, 0, 1, 1).ToString());
            results.Add(copy(input, 8, 4, 0, input, 8, 4, 0, 2, 33).ToString());
            results.Add(copy(input, 1, 4, 0, input, 8, 4, 0, 2, 8).ToString());
            results.Add(copy(input, uint.MaxValue, uint.MaxValue, 0, input, 8, 4, 0, 3, 8).ToString());
            results.Add(copy(input, 8, 4, 1, input, 8, 4, 0, 1, uint.MaxValue).ToString());
        }
        return results;
    }
}
