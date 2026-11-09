using System.Reflection;
using System.Runtime.InteropServices;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class IndependentAudioBalanceBoundaryTests
{
    [DataTestMethod]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    [DataRow(-1.01)]
    [DataRow(1.01)]
    public async Task WhenComBalanceIsOutsideRangeThenInvalidArgumentIsReturned(double value)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            Assert.AreEqual(unchecked((int)0x80070057), SetTarget(media.Pointer, 10, value));
        }
        finally { await ReleaseAsync(media); }
    }

    [DataTestMethod]
    [DataRow(-1.0)]
    [DataRow(0.0)]
    [DataRow(1.0)]
    public async Task WhenPreopenBalanceIsAdmittedThenOnlyTargetIsCached(double value)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            Assert.AreEqual(0, SetTarget(media.Pointer, 10, value));
            Assert.AreEqual(value, media.TargetBalance);
        }
        finally { await ReleaseAsync(media); }
    }

    [DataTestMethod]
    [DataRow(-1.0)]
    [DataRow(0.0)]
    [DataRow(1.0)]
    public async Task WhenFailedAudioOwnerExistsThenBalanceRetainsOriginalNativeFailure(double value)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        IndependentAudioPlaybackOwner owner = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav"));
        await owner.Completion.WaitAsync(TimeSpan.FromSeconds(20));
        try
        {
            // A real failed owner establishes routing only; this is not output acceptance.
            typeof(IndependentMediaIdentity).GetField("_audioOwner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            const int pathNotFound = unchecked((int)0x80070003);
            Assert.AreEqual(pathNotFound, await owner.Ready);
            Assert.AreEqual(pathNotFound, SetTarget(media.Pointer, 10, value));
            Assert.AreEqual(0.0, media.TargetBalance);
            Assert.AreEqual(pathNotFound, await owner.CloseAsync(TimeSpan.FromSeconds(20)));
            Assert.IsTrue(owner.Completion.IsCompletedSuccessfully);
            Assert.AreEqual(0L, owner.Position100ns);
            Assert.AreEqual(pathNotFound, SetTarget(media.Pointer, 10, value));
            Assert.AreEqual(pathNotFound, await owner.Ready);
        }
        finally { await ReleaseAsync(media); }
    }

    [DataTestMethod]
    [DataRow(0.5)]
    [DataRow(2.0)]
    public async Task WhenAudioOwnerExistsThenNonUnitRateIsNotSilentlyAccepted(double value)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        IndependentAudioPlaybackOwner owner = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav"));
        await owner.Completion.WaitAsync(TimeSpan.FromSeconds(20));
        try
        {
            typeof(IndependentMediaIdentity).GetField("_audioOwner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            Assert.AreEqual(unchecked((int)0x80004001), SetTarget(media.Pointer, 8, value));
        }
        finally { await ReleaseAsync(media); }
    }

    [DataTestMethod]
    [DataRow(1u, 16u, 2u, false, true)]
    [DataRow(2u, 16u, 4u, false, true)]
    [DataRow(1u, 32u, 4u, true, true)]
    [DataRow(2u, 32u, 8u, true, true)]
    [DataRow(6u, 16u, 12u, false, false)]
    [DataRow(2u, 16u, 2u, false, false)]
    public void WhenPcmLayoutIsCheckedThenOnlyAlignedMonoAndStereoAreSupported(uint channels, uint bits, uint align, bool floatingPoint, bool expected)
    {
        Assert.AreEqual(expected, new AudioPcmFormat(channels, 48000, bits, align, floatingPoint).IsSupported);
    }

    [DataTestMethod]
    [DataRow(0.0, 1.0f, 1.0f)]
    [DataRow(-1.0, 1.0f, 0.01f)]
    [DataRow(1.0, 0.0f, 1.0f)]
    [DataRow(-0.5, 1.0f, 0.51f)]
    [DataRow(0.5, 0.5f, 1.0f)]
    public void WhenStereoBalanceIsCalculatedThenOnlyOppositeChannelIsAttenuated(double balance, float left, float right)
    {
        Assert.AreEqual((left, right), WasapiSharedOutput.BalanceGains(balance));
    }

    [DataTestMethod]
    [DataRow(-1.0, -99)]
    [DataRow(0.0, 0)]
    [DataRow(1.0, 100)]
    [DataRow(-0.5, -49)]
    [DataRow(0.5, 50)]
    public void WhenUpstreamPercentIsQuantizedThenTruncationIsAsymmetric(double balance, int percent)
    {
        var gains = WasapiSharedOutput.BalanceGains(balance);
        Assert.AreEqual(((float)(1 - Math.Max(0, percent / 100.0)), (float)(1 + Math.Min(0, percent / 100.0))), gains);
    }

    [DataTestMethod]
    [DataRow(-1.0, 0.5f, 0.5f, 0.005f)]
    [DataRow(0.0, 0.5f, 0.5f, 0.5f)]
    [DataRow(1.0, 0.5f, 0.0f, 0.5f)]
    [DataRow(0.5, 0.0f, 0.0f, 0.0f)]
    [DataRow(-0.5, 1.0f, 1.0f, 0.51f)]
    public void WhenSessionVolumeMultipliesChannelGainThenNeutralAndMuteArePreserved(double balance, float volume, float left, float right)
    {
        var gains = WasapiSharedOutput.BalanceGains(balance);
        Assert.AreEqual((left, right), (volume * gains.Left, volume * gains.Right));
    }

    [DataTestMethod]
    [DataRow(-1.0, 1f, 0.01f)]
    [DataRow(-0.5, 1f, 0.51f)]
    [DataRow(-0.015, 1f, 0.99f)]
    [DataRow(-0.005, 1f, 1f)]
    [DataRow(0.0, 1f, 1f)]
    [DataRow(0.0049, 1f, 1f)]
    [DataRow(0.005, 0.99f, 1f)]
    [DataRow(0.5, 0.5f, 1f)]
    [DataRow(0.995, 0f, 1f)]
    [DataRow(1.0, 0f, 1f)]
    public async Task WhenInitialAndRepeatedTargetsReachNativeThenEachIsQuantizedOnlyOnce(double value, float left, float right)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        IndependentAudioPlaybackOwner? owner = null;
        try
        {
            Assert.AreEqual(0, SetTarget(media.Pointer, 10, value));
            owner = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav"), initialBalance: media.TargetBalance);
            AssertNativeFailureArguments(owner.InitialBalance, left, right);
            foreach (double target in new[] { 0.5, value, 0.0, value })
            {
                Assert.AreEqual(0, SetTarget(media.Pointer, 10, target));
                AssertNativeFailureArguments(media.TargetBalance, target == value ? left : target == 0 ? 1f : 0.5f, target == value ? right : 1f);
            }
            Assert.AreEqual(value, media.TargetBalance);
        }
        finally
        {
            if (owner is not null) await owner.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            await ReleaseAsync(media);
        }
    }

    private static unsafe void AssertNativeFailureArguments(double target, float left, float right)
    {
        // A failing ABI recorder observes the production call; it never claims successful output.
        nint* table = stackalloc nint[7];
        table[6] = (nint)(delegate* unmanaged[Stdcall]<nint, uint, float*, int>)&RecordFailure;
        nint* instance = stackalloc nint[3];
        instance[0] = (nint)table;
        MethodInfo apply = typeof(WasapiSharedOutput).GetMethod("ApplyBalance", BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert.AreEqual(unchecked((int)0x80004005), (int)apply.Invoke(new WasapiSharedOutput(), [(nint)instance, 2u, target])!);
        Assert.AreEqual((left, right), (*(float*)&instance[1], *(float*)&instance[2]));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
    private static unsafe int RecordFailure(nint self, uint channels, float* levels)
    {
        if (channels != 2) return unchecked((int)0x80070057);
        *(float*)&((nint*)self)[1] = levels[0];
        *(float*)&((nint*)self)[2] = levels[1];
        return unchecked((int)0x80004005);
    }

    private static unsafe int SetTarget(nint media, int slot, double value) =>
        ((delegate* unmanaged[Stdcall]<nint, double, int>)(*(nint**)media)[slot])(media, value);

    private static async Task ReleaseAsync(IndependentMediaIdentity media)
    {
        Close(media.Pointer);
        await media.Completion.WaitAsync(TimeSpan.FromSeconds(20));
        Release(media.Pointer);
    }

    private static unsafe void Close(nint media) =>
        ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)media)[5])(media);

    private static unsafe void Release(nint media) =>
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)media)[2])(media);
}
