using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed unsafe class RetailPlayerTests
{
    /// <summary>Compares retail argument validation without decoding private prerelease packets.</summary>
    [TestMethod]
    public void WhenRetailPlayerIsCalledThenCurrentOfficialContractMatches()
    {
        object Observe(string path, out List<(string, int, nint)> observations)
        {
            nint module = NativeLibrary.Load(path);
            var create = (delegate* unmanaged[Stdcall]<nint*, int>)NativeLibrary.GetExport(module, "MilPlayer_Create");
            var process = (delegate* unmanaged[Stdcall]<nint, byte*, byte*, byte*, uint, int>)NativeLibrary.GetExport(module, "MilPlayer_Process");
            observations = [];
            nint sentinel = 0x12345678;
            observations.Add(("create-null", create(null), sentinel));
            nint output = sentinel;
            int hr = create(&output);
            observations.Add(("create-output", hr, output));
            byte header = 0, rdp = 0, data = 0;
            nint player = (nint)(&data);
            for (int mask = 0; mask < 16; mask++)
            {
                nint handle = (mask & 1) == 0 ? 0 : player;
                byte* packet = (mask & 2) == 0 ? null : &header;
                byte* optionalRdp = (mask & 4) == 0 ? null : &rdp;
                byte* optionalData = (mask & 8) == 0 ? null : &data;
                foreach (uint size in new uint[] { 0, 1, uint.MaxValue })
                    observations.Add(($"process-{mask}-{size}", process(handle, packet, optionalRdp, optionalData, size), sentinel));
            }
            return new { Path = path, Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), Runtime = RuntimeInformation.FrameworkDescription,
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(), Calls = observations.Select(item => new { Name = item.Item1, HResult = item.Item2, Output = (long)item.Item3 }) };
        }

        object original = Observe(AcceptancePaths.OriginalDll, out var expected);
        object? candidate = null;
        try
        {
            candidate = Observe(AcceptancePaths.AotDll, out var actual);
            CollectionAssert.AreEqual(expected, actual);
        }
        finally
        {
            File.WriteAllText(Path.Combine(AcceptancePaths.Results, "retail-player.json"), JsonSerializer.Serialize(new { Original = original, Candidate = candidate }, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
