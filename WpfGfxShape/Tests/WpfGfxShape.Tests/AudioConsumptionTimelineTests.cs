using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class AudioConsumptionTimelineTests
{
    /// <summary>Queued frames do not advance media until the device consumes them.</summary>
    [DataTestMethod]
    [DataRow(0UL, 0L)]
    [DataRow(24000UL, 5000000L)]
    [DataRow(48000UL, 10000000L)]
    [DataRow(96000UL, 10000000L)]
    public void WhenUnityFramesAreSubmittedThenOnlyConsumptionAdvancesPosition(ulong clock, long expected)
    {
        AudioConsumptionTimeline timeline = new();
        timeline.Reset(0, 0, 48000);
        timeline.Submit(48000, 48000);
        Assert.AreEqual(expected, timeline.Position100ns(clock, 48000, 40000000));
    }

    /// <summary>Changing segment ratios cannot reinterpret audio queued before the boundary.</summary>
    [DataTestMethod]
    [DataRow(24000UL, 5000000L)]
    [DataRow(48000UL, 10000000L)]
    [DataRow(60000UL, 15000000L)]
    [DataRow(72000UL, 20000000L)]
    [DataRow(96000UL, 22500000L)]
    [DataRow(120000UL, 25000000L)]
    [DataRow(144000UL, 25000000L)]
    public void WhenRatiosChangeThenConsumedSegmentsAreIntegrated(ulong clock, long expected)
    {
        AudioConsumptionTimeline timeline = new();
        timeline.Reset(0, 0, 48000);
        timeline.Submit(48000, 48000);
        timeline.Submit(24000, 48000);
        timeline.Submit(48000, 24000);
        Assert.AreEqual(expected, timeline.Position100ns(clock, 48000, 40000000));
    }

    /// <summary>Chunk boundaries and repeated observations must not discard fractional progress.</summary>
    [TestMethod]
    public void WhenFramesAreSplitThenConsumptionMatchesSingleChunk()
    {
        AudioConsumptionTimeline split = new(), whole = new();
        split.Reset(0, 17, 48000);
        whole.Reset(0, 17, 48000);
        whole.Submit(48000, 96000);
        split.Submit(12000, 24000);
        split.Submit(36000, 72000);
        _ = split.Position100ns(12017, 48000, 40000000);
        Assert.AreEqual(whole.Position100ns(33018, 48000, 40000000), split.Position100ns(33018, 48000, 40000000));
    }

    /// <summary>A seek starts a new timeline and removes all old queued ratios.</summary>
    [TestMethod]
    public void WhenSeekResetsThenOldSegmentsCannotAdvanceTheNewSource()
    {
        AudioConsumptionTimeline timeline = new();
        timeline.Reset(0, 0, 48000);
        timeline.Submit(48000, 96000);
        timeline.Reset(5000000, 100, 8000);
        timeline.Submit(8000, 8000);
        Assert.AreEqual(10000000L, timeline.Position100ns(4100, 8000, 40000000));
    }

    /// <summary>Close or cancellation discards queued progress without draining it as successful audio.</summary>
    [TestMethod]
    public void WhenTimelineIsClearedThenQueuedAudioCannotAdvance()
    {
        AudioConsumptionTimeline timeline = new();
        timeline.Reset(0, 0, 48000);
        timeline.Submit(48000, 96000);
        timeline.Clear();
        Assert.AreEqual(0L, timeline.Position100ns(48000, 48000, 40000000));
    }

    /// <summary>Clock units need not equal PCM frames.</summary>
    [TestMethod]
    public void WhenClockUsesThirdsThenFractionalFramesRetainExactTicks()
    {
        AudioConsumptionTimeline timeline = new();
        timeline.Reset(0, 0, 48000);
        timeline.Submit(48000, 48000);
        Assert.AreEqual(3333333L, timeline.Position100ns(1, 3, 40000000));
    }

    /// <summary>Consumption is clamped to known duration.</summary>
    [TestMethod]
    public void WhenMediaReachesDurationThenPositionIsClamped()
    {
        AudioConsumptionTimeline timeline = new();
        timeline.Reset(0, 0, 48000);
        timeline.Submit(48000, 96000);
        Assert.AreEqual(15000000L, timeline.Position100ns(48000, 48000, 15000000));
    }

    /// <summary>A clock reading before the seek anchor contributes no elapsed media.</summary>
    [TestMethod]
    public void WhenClockPrecedesAnchorThenPositionStaysAtAnchor()
    {
        AudioConsumptionTimeline timeline = new();
        timeline.Reset(5000000, 100, 48000);
        timeline.Submit(48000, 48000);
        Assert.AreEqual(5000000L, timeline.Position100ns(99, 48000, 40000000));
    }

    /// <summary>Retired ratios must not reinterpret a backward or repeated clock observation.</summary>
    [DataTestMethod]
    [DataRow(12000UL, 15000000L)]
    [DataRow(60000UL, 15000000L)]
    [DataRow(66000UL, 17500000L)]
    public void WhenClockRegressesAfterSegmentRetirementThenProgressRemainsMonotonic(ulong clock, long expected)
    {
        AudioConsumptionTimeline timeline = new();
        timeline.Reset(0, 0, 48000);
        timeline.Submit(48000, 48000);
        timeline.Submit(24000, 48000);
        _ = timeline.Position100ns(60000, 48000, 40000000);
        _ = timeline.Position100ns(12000, 48000, 40000000);
        Assert.AreEqual(expected, timeline.Position100ns(clock, 48000, 40000000));
    }

    /// <summary>A seek clears the observation high-water mark as well as queued ratios.</summary>
    [TestMethod]
    public void WhenResetFollowsConsumptionThenNewClockStartsAtItsOwnAnchor()
    {
        AudioConsumptionTimeline timeline = new();
        timeline.Reset(0, 0, 48000);
        timeline.Submit(48000, 48000);
        _ = timeline.Position100ns(48000, 48000, 40000000);
        timeline.Reset(5000000, 100, 8000);
        timeline.Submit(8000, 8000);
        Assert.AreEqual(10000000L, timeline.Position100ns(4100, 8000, 40000000));
    }

    /// <summary>The full clock and sample-rate types must not overflow intermediate frame products.</summary>
    [DataTestMethod]
    [DataRow(ulong.MaxValue, ulong.MaxValue, uint.MaxValue, 10000000L)]
    [DataRow(ulong.MaxValue, 1UL, uint.MaxValue, 10000000L)]
    [DataRow(1UL, ulong.MaxValue, uint.MaxValue, 0L)]
    [DataRow(ulong.MaxValue, ulong.MaxValue, 192000U, 10000000L)]
    [DataRow(ulong.MaxValue, ulong.MaxValue, 8000U, 10000000L)]
    public void WhenClockAndRateReachTypeLimitsThenConversionRemainsExact(ulong elapsed, ulong frequency, uint sampleRate, long expected)
    {
        AudioConsumptionTimeline timeline = new();
        timeline.Reset(0, 0, sampleRate);
        timeline.Submit(sampleRate, sampleRate);
        Assert.AreEqual(expected, timeline.Position100ns(elapsed, frequency, ulong.MaxValue));
    }

    /// <summary>Large fractional clocks preserve the tick immediately before a segment boundary.</summary>
    [TestMethod]
    public void WhenExtremeClockPrecedesBoundaryThenFractionIsNotRoundedUp()
    {
        AudioConsumptionTimeline timeline = new();
        timeline.Reset(0, 0, uint.MaxValue);
        timeline.Submit(uint.MaxValue, uint.MaxValue);
        Assert.AreEqual(9999999L, timeline.Position100ns(ulong.MaxValue - 1, ulong.MaxValue, ulong.MaxValue));
    }

    /// <summary>Zero clock frequency cannot describe consumed time.</summary>
    [TestMethod]
    public void WhenFrequencyIsZeroThenTimelineRejectsIt()
    {
        AudioConsumptionTimeline timeline = new();
        timeline.Reset(0, 0, 48000);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => timeline.Position100ns(1, 0, 40000000));
    }
}
