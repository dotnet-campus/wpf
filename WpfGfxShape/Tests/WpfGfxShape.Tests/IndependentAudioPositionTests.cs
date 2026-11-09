using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class IndependentAudioPositionTests
{
    /// <summary>Device units are frequency-relative, not necessarily PCM frames or QPC ticks.</summary>
    [DataTestMethod]
    [DataRow(48000UL, 48000UL, 20000000UL, 10000000L)]
    [DataRow(192000UL, 192000UL, 20000000UL, 10000000L)]
    [DataRow(1UL, 3UL, 20000000UL, 3333333L)]
    [DataRow(ulong.MaxValue, ulong.MaxValue, ulong.MaxValue, 10000000L)]
    [DataRow(ulong.MaxValue, 1UL, ulong.MaxValue, long.MaxValue)]
    [DataRow(48000UL, 48000UL, 5000000UL, 5000000L)]
    [DataRow(0UL, 48000UL, 40000000UL, 0L)]
    public void WhenDeviceClockIsConvertedThenUnitsOverflowAndDurationArePreserved(ulong position, ulong frequency, ulong duration, long expected)
    {
        Assert.AreEqual(expected, IndependentAudioPlaybackOwner.ClockPosition100ns(position, frequency, duration));
    }

    /// <summary>A zero frequency cannot provide a valid device timeline.</summary>
    [TestMethod]
    public void WhenDeviceFrequencyIsZeroThenConversionRejectsIt()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IndependentAudioPlaybackOwner.ClockPosition100ns(1, 0, 40000000));
    }
}
