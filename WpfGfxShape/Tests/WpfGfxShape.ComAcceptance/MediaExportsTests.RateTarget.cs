using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class MediaExportsTests
{
    /// <summary>No source admits native target intent without creating an output owner.</summary>
    [DataTestMethod]
    [DataRow(-1d)]
    [DataRow(0d)]
    [DataRow(0.5d)]
    [DataRow(double.Epsilon)]
    [DataRow(double.MaxValue)]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    public void WhenNoSourceReceivesRateThenNativeTargetIsAccepted(double rate)
    {
        using var source = new SystemMedia(candidate: true);
        var set = (delegate* unmanaged[Stdcall]<nint, double, int>)NativeLibrary.GetExport(LoadCandidate(), "MILMediaSetRate");
        Assert.AreEqual(0, set(source.Media, rate));
    }

    /// <summary>Close cancels old intent but accepts a subsequent target for the next Open.</summary>
    [DataTestMethod]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NaN)]
    [DataRow(0.5d)]
    public void WhenClosedPlayerReceivesNewRateThenNextGenerationTargetIsAccepted(double rate)
    {
        using var source = new SystemMedia(candidate: true);
        nint module = LoadCandidate();
        var set = (delegate* unmanaged[Stdcall]<nint, double, int>)NativeLibrary.GetExport(module, "MILMediaSetRate");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MILMediaClose");
        Assert.AreEqual((0, 0, 0), (set(source.Media, double.NegativeInfinity), close(source.Media), set(source.Media, rate)));
    }
}
