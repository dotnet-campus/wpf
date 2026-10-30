#if KG_OFFICIAL_SHADER_CONTRACT
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    /// <summary>Runs the unchanged official scene in a dedicated filtered process, without installing a resolver.</summary>
    [TestMethod]
    public void WhenKgOfficialShaderContractIsObservedThenOriginalContentContractRemains()
    {
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            Contract = "KG unchanged official scene; native precondition failure stops the scene",
            TestSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(D3DImageTests).Assembly.Location))),
            TestMvid = typeof(D3DImageTests).Module.ModuleVersionId,
            WpfVersion = typeof(System.Windows.Media.Visual).Assembly.GetName().Version?.ToString(),
            Runtime = RuntimeInformation.FrameworkDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            ShaderSha256 = Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(TwoInputShaderTokens.AsSpan()))),
            ShaderBytes = TwoInputShaderTokens.Length * sizeof(uint),
            InputDifferences = Array.Empty<string>(),
            OriginalTestId = "14e862b3-9b71-843a-9ff5-67f779272c33"
        }));
        WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive();
    }
}
#endif
