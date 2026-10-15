namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class AcceptancePathsTests
{
    [TestMethod]
    [DataRow("../outside.dll")]
    [DataRow("../application-other/file.dll")]
    [DataRow("C:/outside.dll")]
    [DataRow("")]
    [DataRow(" ")]
    public void WhenSandboxPathEscapesApplicationThenItIsRejected(string path)
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => AcceptancePaths.Resolve(path, true, AppContext.BaseDirectory));
    }

    [TestMethod]
    public void WhenSandboxPathIsContainedThenItResolvesUnderApplication()
    {
        Assert.AreEqual(Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll"),
            AcceptancePaths.Resolve("Native/wpfgfx_cor3.dll", true, AppContext.BaseDirectory));
    }

    [TestMethod]
    public void WhenLocalPathIsRelativeThenItIsRejected()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => AcceptancePaths.Resolve("Native/wpfgfx_cor3.dll", false, AppContext.BaseDirectory));
    }

    [TestMethod]
    public void WhenLocalPathIsAbsoluteThenItRetainsConfiguredIdentity()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "configured.dll");
        Assert.AreEqual(path, AcceptancePaths.Resolve(path, false, AppContext.BaseDirectory));
    }

    [TestMethod]
    public void WhenSandboxRunnerStartsThenRelativeResultsStayInsideApplication()
    {
        if (!AcceptancePaths.Sandbox) return;
        Assert.AreEqual(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "TestResults")),
            Path.GetFullPath("TestResults"));
    }

    [TestMethod]
    public void WhenArtifactIsValidatedThenIdentityIsSha256()
    {
        Assert.AreEqual(64, AcceptancePaths.ValidateIdentity().Length);
    }

    [TestMethod]
    public void WhenSandboxPackageIsBuiltThenManifestMatchesOnlyWhitelistedInputs()
    {
        if (!AcceptancePaths.Sandbox) return;
        string root = AppContext.BaseDirectory;
        string[] manifest = File.ReadAllLines(Path.Combine(root, "package-files.txt"));
        string[] actual = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(root, file).Replace('\\', '/'))
            .Where(file => !file.StartsWith("Results/", StringComparison.OrdinalIgnoreCase)
                && !file.StartsWith("TestResults/", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal).ToArray();
        CollectionAssert.AreEqual(manifest.Order(StringComparer.Ordinal).ToArray(), actual);
    }
}
