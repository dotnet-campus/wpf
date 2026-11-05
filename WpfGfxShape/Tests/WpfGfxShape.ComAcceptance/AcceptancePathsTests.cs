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
    public void WhenCandidatePeIsInvalidThenIdentityIsRejected()
    {
        using var stream = new MemoryStream(new byte[] { 0x4d, 0x5a });
        Assert.ThrowsExactly<BadImageFormatException>(() => AcceptancePaths.ValidateIdentity(stream, new string('0', 64)));
    }

    [TestMethod]
    public void WhenCandidateHasClrHeaderThenIdentityIsRejected()
    {
        ushort machine = MatchingMachine();
        byte[] bytes = NativePe(machine);
        Array.Resize(ref bytes, 1024);
        using var stream = new MemoryStream(bytes);
        using var writer = new BinaryWriter(stream);
        stream.Position = 0x86;
        writer.Write((ushort)1);
        stream.Position = 0x98 + (machine == 0x14c ? 96 : 112) + 14 * 8;
        writer.Write(0x2000);
        writer.Write(72);
        stream.Position = 0x98 + (machine == 0x14c ? 224 : 240) + 8;
        writer.Write(512);
        writer.Write(0x2000);
        writer.Write(512);
        writer.Write(512);
        // Map both CLR and metadata directories so parsing reaches the native-only identity gate.
        stream.Position = 512;
        writer.Write(72);
        writer.Write((ushort)2);
        writer.Write((ushort)5);
        writer.Write(0x2000 + 72);
        writer.Write(1);
        string sha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
        stream.Position = 0;
        Assert.ThrowsExactly<InvalidDataException>(() => AcceptancePaths.ValidateIdentity(stream, sha));
    }

    [TestMethod]
    public void WhenCandidateHasNoOptionalHeaderThenIdentityIsRejected()
    {
        // A COFF-only header parses with PEHeader null, rather than failing on a truncated PE.
        byte[] bytes = NativePe(MatchingMachine()).AsSpan(0x84, 20).ToArray();
        bytes[16] = 0;
        bytes[17] = 0;
        string sha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
        using var stream = new MemoryStream(bytes);
        Assert.ThrowsExactly<InvalidDataException>(() => AcceptancePaths.ValidateIdentity(stream, sha));
    }

    [TestMethod]
    public void WhenCandidateArchitectureDiffersThenIdentityIsRejected()
    {
        using var stream = new MemoryStream(NativePe(MatchingMachine() == 0x8664 ? (ushort)0x14c : (ushort)0x8664));
        Assert.ThrowsExactly<InvalidDataException>(() => AcceptancePaths.ValidateIdentity(stream, new string('0', 64)));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("0000000000000000000000000000000000000000000000000000000000000000")]
    public void WhenCandidateFrozenHashDiffersThenIdentityIsRejected(string expectedSha)
    {
        using var stream = new MemoryStream(NativePe(MatchingMachine()));
        Assert.ThrowsExactly<InvalidDataException>(() => AcceptancePaths.ValidateIdentity(stream, expectedSha));
    }

    [TestMethod]
    public void WhenCandidateFrozenHashMatchesThenValidatedDigestIsReturned()
    {
        byte[] bytes = NativePe(MatchingMachine());
        string sha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
        using var stream = new MemoryStream(bytes);
        Assert.AreEqual(sha, AcceptancePaths.ValidateIdentity(stream, sha.ToLowerInvariant()));
    }

    [TestMethod]
    [DataRow("media")]
    [DataRow("dispatcher")]
    public void WhenCandidateCoexistsWithRuntimeOriginalThenModuleGateRejectsIt(string scene)
    {
        string candidate = Path.Combine(AppContext.BaseDirectory, scene, "Native", "wpfgfx_cor3.dll");
        string runtime = Path.Combine(AppContext.BaseDirectory, "runtime", "wpfgfx_cor3.dll");
        Assert.ThrowsExactly<InvalidOperationException>(() => AcceptancePaths.ValidateCandidateModules(candidate, [candidate, runtime]));
    }

    [TestMethod]
    public void WhenCandidateWasNotLoadedThenModuleGateRejectsIt()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => AcceptancePaths.ValidateCandidateModules(
            Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll"), []));
    }

    [TestMethod]
    public void WhenOnlyAuthenticatedCandidateIsLoadedThenModuleGateAcceptsIt()
    {
        string candidate = Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll");
        AcceptancePaths.ValidateCandidateModules(candidate, [candidate]);
    }

    [TestMethod]
    public void WhenRecordedDigestDiffersFromValidatedDigestThenIdentityIsRejected()
    {
        Assert.ThrowsExactly<InvalidDataException>(() => AcceptancePaths.ValidateDigest(new string('1', 64), new string('2', 64)));
    }

    [TestMethod]
    public void WhenFailureInspectionFindsDuplicateCandidateModulesThenItRejectsThem()
    {
        string candidate = Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll");
        Assert.ThrowsExactly<InvalidOperationException>(() => AcceptancePaths.ValidateCandidateModules(candidate, [candidate, candidate], false));
    }

    [TestMethod]
    public void WhenFailureInspectionFindsNoCandidateModulesThenItAcceptsThem()
    {
        string candidate = Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll");
        AcceptancePaths.ValidateCandidateModules(candidate, [], false);
    }

    [TestMethod]
    public void WhenFailureInspectionFindsOnlyDifferentPathModuleThenItRejectsIt()
    {
        string candidate = Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll");
        string other = Path.Combine(AppContext.BaseDirectory, "other", "wpfgfx_cor3.dll");
        Assert.ThrowsExactly<InvalidOperationException>(() => AcceptancePaths.ValidateCandidateModules(candidate, [other], false));
    }

    private static ushort MatchingMachine() => System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture switch
    {
        System.Runtime.InteropServices.Architecture.X64 => 0x8664,
        System.Runtime.InteropServices.Architecture.X86 => 0x14c,
        System.Runtime.InteropServices.Architecture.Arm64 => 0xaa64,
        _ => throw new PlatformNotSupportedException()
    };

    private static byte[] NativePe(ushort machine)
    {
        // Header-only, non-executable fixture: no sections, code, imports or CLR directory.
        byte[] bytes = new byte[512];
        using var stream = new MemoryStream(bytes);
        using var writer = new BinaryWriter(stream);
        writer.Write((ushort)0x5a4d);
        stream.Position = 0x3c;
        writer.Write(0x80);
        stream.Position = 0x80;
        writer.Write(0x00004550);
        writer.Write(machine);
        stream.Position = 0x94;
        writer.Write((ushort)(machine == 0x14c ? 224 : 240));
        writer.Write((ushort)0x2002);
        writer.Write((ushort)(machine == 0x14c ? 0x10b : 0x20b));
        stream.Position = 0x98 + 60;
        writer.Write(512);
        stream.Position = 0x98 + (machine == 0x14c ? 92 : 108);
        writer.Write(16);
        return bytes;
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
                && !file.StartsWith("TestResults/", StringComparison.OrdinalIgnoreCase)
                && (!file.StartsWith("ReferenceData/", StringComparison.OrdinalIgnoreCase) || manifest.Contains(file, StringComparer.Ordinal)))
            .Order(StringComparer.Ordinal).ToArray();
        CollectionAssert.AreEqual(manifest.Order(StringComparer.Ordinal).ToArray(), actual);
    }
}
