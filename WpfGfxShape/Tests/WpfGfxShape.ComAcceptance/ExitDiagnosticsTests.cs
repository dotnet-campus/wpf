namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class ExitDiagnosticsTests
{
    [TestMethod]
    [DataRow(1)]
    [DataRow(120)]
    public void WhenLegacyFilteredBudgetIsValidThenArgumentsArePreserved(int seconds)
    {
        var options = ExitDiagnostics.Parse(["Results/control", seconds.ToString(System.Globalization.CultureInfo.InvariantCulture), "FullyQualifiedName~PublishedArtifactTests"]);
        CollectionAssert.AreEqual(new[] { "--filter", "FullyQualifiedName~PublishedArtifactTests" }, options.ChildArguments[..2]);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(1500)]
    public void WhenFullIsExplicitThenCompleteChildArgumentsContainNoFilter(int seconds)
    {
        string root = Path.GetFullPath("Results/full");
        var options = ExitDiagnostics.Parse(["--full", root, seconds.ToString(System.Globalization.CultureInfo.InvariantCulture)]);
        CollectionAssert.AreEqual(new[] { "--report-trx", "--report-trx-filename", "child.trx", "--results-directory", Path.Combine(root, "TestResults"), "--diagnostic", "--diagnostic-synchronous-write", "--diagnostic-output-directory", Path.Combine(root, "Diagnostic"), "--no-progress" }, options.ChildArguments);
    }

    [TestMethod]
    [DataRow("root", "0", "filter")]
    [DataRow("root", "-1", "filter")]
    [DataRow("root", "121", "filter")]
    [DataRow("--full", "root", "0")]
    [DataRow("--full", "root", "-1")]
    [DataRow("--full", "root", "1501")]
    public void WhenBudgetIsOutsideModeBoundsThenItIsRejected(string first, string second, string third)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ExitDiagnostics.Parse([first, second, third]));
    }

    [TestMethod]
    [DataRow("root", "abc", "filter")]
    [DataRow("--full", "root", "1.5")]
    public void WhenBudgetIsNotIntegerThenItIsRejected(string first, string second, string third)
    {
        Assert.ThrowsExactly<FormatException>(() => ExitDiagnostics.Parse([first, second, third]));
    }

    [TestMethod]
    public void WhenBudgetOverflowsThenItIsRejected()
    {
        Assert.ThrowsExactly<OverflowException>(() => ExitDiagnostics.Parse(["--full", "root", "2147483648"]));
    }

    [TestMethod]
    [DataRow("", "1", "filter")]
    [DataRow("root", "1", " ")]
    [DataRow("root", "1", "")]
    [DataRow("--full", " ", "1")]
    public void WhenRequiredArgumentIsBlankThenItIsRejected(string first, string second, string third)
    {
        Assert.ThrowsExactly<ArgumentException>(() => ExitDiagnostics.Parse([first, second, third]));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(4)]
    public void WhenArgumentCountIsInvalidThenItIsRejected(int count)
    {
        Assert.ThrowsExactly<ArgumentException>(() => ExitDiagnostics.Parse(new string[count]));
    }

    [TestMethod]
    public void WhenArgumentsAreNullThenTheyAreRejected()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ExitDiagnostics.Parse(null!));
    }

    [TestMethod]
    public void WhenWildcardFilterIsUsedThenModeRemainsFiltered()
    {
        Assert.AreEqual("filtered", ExitDiagnostics.Parse(["root", "120", "*"]).Mode);
    }

    [TestMethod]
    public void WhenFullIsExplicitThenModeIsFull()
    {
        Assert.AreEqual("full", ExitDiagnostics.Parse(["--full", "root", "1500"]).Mode);
    }
}
