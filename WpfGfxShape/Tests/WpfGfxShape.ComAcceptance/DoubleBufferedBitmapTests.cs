using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed class DoubleBufferedBitmapTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [DataRow("MILSwDoubleBufferedBitmapCreate")]
    [DataRow("MILSwDoubleBufferedBitmapGetBackBuffer")]
    [DataRow("MILSwDoubleBufferedBitmapAddDirtyRect")]
    [DataRow("MILSwDoubleBufferedBitmapProtectBackBuffer")]
    [DataRow("WgxConnection_Create")]
    [DataRow("WgxConnection_Disconnect")]
    [DataRow("MilConnection_CreateChannel")]
    [DataRow("MilConnection_DestroyChannel")]
    [DataRow("MilResource_CreateOrAddRefOnChannel")]
    [DataRow("MilResource_ReleaseOnChannel")]
    [DataRow("MilResource_SendCommand")]
    [DataRow("MilChannel_CloseBatch")]
    [DataRow("MilChannel_CommitChannel")]
    [DataRow("WgxConnection_SameThreadPresent")]
    public void WhenDoubleBufferedModuleIsPublishedThenRequiredProductionEntryExists(string exportName)
    {
        string path = typeof(DoubleBufferedBitmapTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "AotDllPath").Value!;
        using (var file = File.OpenRead(path))
        using (var pe = new PEReader(file))
        {
            Assert.IsNull(pe.PEHeaders.CorHeader);
            Machine machine = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => Machine.Amd64,
                Architecture.Arm64 => Machine.Arm64,
                Architecture.X86 => Machine.I386,
                _ => throw new PlatformNotSupportedException()
            };
            Assert.AreEqual(machine, pe.PEHeaders.CoffHeader.Machine);
        }
        TestContext.WriteLine($"{path}; SHA256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}");
        // Keep Native AOT mapped until the standard test process exits.
        nint module = NativeLibrary.Load(path);
        Assert.IsTrue(NativeLibrary.TryGetExport(module, exportName, out _), exportName);
    }
}
