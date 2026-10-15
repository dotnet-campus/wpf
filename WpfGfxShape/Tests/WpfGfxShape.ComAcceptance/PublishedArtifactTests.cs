using System.Reflection;
using System.Security.Cryptography;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class PublishedArtifactTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void WhenPublishedArtifactIsValidatedThenItsSha256IsRecorded()
    {
        string path = AcceptancePaths.AotDll;
        using var stream = File.OpenRead(path);
        string hash = Convert.ToHexString(SHA256.HashData(stream));
        TestContext.WriteLine($"Native AOT artifact: {path}; SHA256: {hash}");
        Assert.AreEqual(64, hash.Length);
    }
}
