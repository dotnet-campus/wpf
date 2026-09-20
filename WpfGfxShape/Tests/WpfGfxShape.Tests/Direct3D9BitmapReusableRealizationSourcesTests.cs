using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapReusableRealizationSourcesTests
{
    [TestMethod]
    public void WhenValidOverlappingSourceHasPartialDirtyInformationThenSourceIsRetained()
    {
        ReusableSourcesHarness harness = new();
        harness.AddSource(1, 0, CreateReusableState(cachedUniquenessToken: 8));
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources();

        sources.SetSources(1);

        Assert.AreEqual((1, "add:1,set-next:1:0,set-next:1:0,add:1,release:1", string.Empty),
            (sources.Head, harness.ReferenceCalls, harness.AdoptCalls));
    }

    [TestMethod]
    public void WhenSourcesAreProcessedThenRejectedSystemSurfaceIsConsideredAndAcceptedOrderIsPreserved()
    {
        ReusableSourcesHarness harness = new();
        harness.AddSource(1, 2, CreateReusableState(cachedUniquenessToken: 7));
        harness.AddSource(2, 3, CreateReusableState(bitmap: 0));
        harness.AddSource(3, 0, CreateReusableState(cachedUniquenessToken: 9));
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources();

        sources.SetSources(1);

        Assert.AreEqual(
            (3, "add:1,set-next:1:0,release:1,set-next:2:0,set-next:2:0,add:2,release:2,set-next:3:0,set-next:3:2,add:3,release:3", "1"),
            (sources.Head, harness.ReferenceCalls, harness.AdoptCalls));
    }

    [TestMethod]
    public void WhenCandidatesAreConsumedThenCandidateOwnershipTransfersThroughFiltering()
    {
        ReusableSourcesHarness harness = new();
        harness.AddSource(1, 0, CreateReusableState(cachedUniquenessToken: 7));
        harness.AddSource(2, 0, CreateReusableState(bitmap: 0));
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(1);
        candidates.Add(2);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources();
        harness.ClearCalls();

        sources.Consume(candidates);

        Assert.AreEqual(
            (2, 0, "set-next:2:0,set-next:2:0,add:2,release:2,set-next:1:0,release:1", "1"),
            (sources.Head, candidates.Head, harness.ReferenceCalls, harness.AdoptCalls));
    }

    [TestMethod]
    public void WhenCandidateFilteringFailsThenCurrentAndRemainingCandidateReferencesAreReleased()
    {
        ReusableSourcesHarness harness = new();
        harness.AddSource(1, 0, CreateReusableState(cachedUniquenessToken: 8));
        harness.AddSource(2, 0, CreateReusableState(bitmap: 0));
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(1);
        candidates.Add(2);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(
            tryGetState: (nint source, out Direct3D9BitmapReusableRealizationSourceState state) =>
            {
                state = default;
                throw new InvalidOperationException($"state:{source}");
            });
        harness.ClearCalls();

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => sources.Consume(candidates));

        Assert.AreEqual(
            ("state:2", 0, 0, "set-next:2:0,release:2,set-next:1:0,release:1"),
            (exception.Message, sources.Head, candidates.Head, harness.ReferenceCalls));
    }

    [TestMethod]
    public void WhenTransferredSourceFilteringFailsThenCurrentAndRemainingReferencesAreReleased()
    {
        ReusableSourcesHarness harness = new();
        harness.AddSource(1, 2, CreateReusableState(cachedUniquenessToken: 8));
        harness.AddSource(2, 3, CreateReusableState(bitmap: 0));
        harness.AddSource(3, 0, CreateReusableState(bitmap: 0));
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(
            tryGetState: (nint source, out Direct3D9BitmapReusableRealizationSourceState state) =>
            {
                state = default;
                throw new InvalidOperationException($"state:{source}");
            });

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => sources.SetSources(1));

        Assert.AreEqual(
            ("state:1", 0, "add:1,set-next:1:0,release:1,set-next:2:0,release:2,set-next:3:0,release:3"),
            (exception.Message, sources.Head, harness.ReferenceCalls));
    }

    [TestMethod]
    public void WhenTransferredSourceAdoptionFailsThenCurrentAndRemainingReferencesAreReleased()
    {
        ReusableSourcesHarness harness = new();
        harness.AddSource(1, 2, CreateReusableState(bitmap: 20));
        harness.AddSource(2, 0, CreateReusableState(bitmap: 20));
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(
            tryAdoptSystemMemorySurface: source => throw new InvalidOperationException($"adopt:{source}"));

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => sources.SetSources(1));

        Assert.AreEqual(
            ("adopt:1", 0, "add:1,set-next:1:0,release:1,set-next:2:0,release:2"),
            (exception.Message, sources.Head, harness.ReferenceCalls));
    }

    [TestMethod]
    public void WhenSourceIsCompletelyDirtyThenVideoMemoryReuseIsRejected()
    {
        ReusableSourcesHarness harness = new();
        harness.AddSource(1, 0, CreateReusableState(cachedUniquenessToken: 8) with { IsCompletelyDirty = true });
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources();

        sources.SetSources(1);

        Assert.AreEqual((0, "1"), (sources.Head, harness.AdoptCalls));
    }

    [TestMethod]
    public void WhenSourcesAreReplacedThenPriorRetainedReferencesAreReleasedBeforeInputIsConsumed()
    {
        ReusableSourcesHarness harness = new();
        harness.AddSource(1, 0, CreateReusableState(cachedUniquenessToken: 8));
        harness.AddSource(2, 0, CreateReusableState(bitmap: 0));
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources();
        sources.SetSources(1);
        harness.ClearCalls();

        sources.SetSources(2);

        Assert.AreEqual(
            "add:2,set-next:1:0,release:1,set-next:2:0,set-next:2:0,add:2,release:2",
            harness.ReferenceCalls);
    }

    [TestMethod]
    public void WhenReplacingSourcesAndNewSourceAdoptionFailsThenPriorAndTransferredReferencesAreReleased()
    {
        ReusableSourcesHarness harness = new();
        harness.AddSource(1, 0, CreateReusableState(cachedUniquenessToken: 8));
        harness.AddSource(2, 3, CreateReusableState(bitmap: 20));
        harness.AddSource(3, 0, CreateReusableState(bitmap: 20));
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(
            tryAdoptSystemMemorySurface: source => throw new InvalidOperationException($"adopt:{source}"));
        sources.SetSources(1);
        harness.ClearCalls();

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => sources.SetSources(2));

        Assert.AreEqual(
            ("adopt:2", 0, "add:2,set-next:1:0,release:1,set-next:2:0,release:2,set-next:3:0,release:3"),
            (exception.Message, sources.Head, harness.ReferenceCalls));
    }

    [TestMethod]
    public void WhenSourcesAreReleasedThenEveryTransferredLinkIsClearedAndReleased()
    {
        ReusableSourcesHarness harness = new();
        harness.AddSource(1, 2, CreateReusableState(bitmap: 0));
        harness.AddSource(2, 0, CreateReusableState(bitmap: 0));
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources();
        sources.SetSources(1);
        harness.ClearCalls();

        sources.ReleaseSources();

        Assert.AreEqual((0, "set-next:2:0,release:2,set-next:1:0,release:1"),
            (sources.Head, harness.ReferenceCalls));
    }

    private static Direct3D9BitmapReusableRealizationSourceState CreateReusableState(
        nint bitmap = 10,
        uint cachedUniquenessToken = 8) =>
        new(
            bitmap,
            IsValid: true,
            IsRenderTarget: false,
            PrefilterWidth: 100,
            PrefilterHeight: 80,
            new Direct3D9BitmapRealizationRectangle(20, 10, 80, 70),
            cachedUniquenessToken,
            HasValidDirtyRectInformation: true,
            IsCompletelyDirty: false);

    private sealed class ReusableSourcesHarness
    {
        private readonly Dictionary<nint, nint> _next = [];
        private readonly Dictionary<nint, Direct3D9BitmapReusableRealizationSourceState> _states = [];
        private readonly List<string> _referenceCalls = [];
        private readonly List<nint> _adoptCalls = [];

        internal string ReferenceCalls => string.Join(',', _referenceCalls);

        internal string AdoptCalls => string.Join(',', _adoptCalls);

        internal void AddSource(
            nint source,
            nint next,
            Direct3D9BitmapReusableRealizationSourceState state)
        {
            _next.Add(source, next);
            _states.Add(source, state);
        }

        internal Direct3D9BitmapReusableRealizationCandidates CreateCandidates() =>
            new(
                source => _next[source],
                SetNext,
                source => _referenceCalls.Add($"add:{source}"),
                source => _referenceCalls.Add($"release:{source}"));

        internal Direct3D9BitmapReusableRealizationSources CreateSources(
            Direct3D9TryGetReusableRealizationSourceState? tryGetState = null,
            Action<nint>? tryAdoptSystemMemorySurface = null) =>
            new(
                new Direct3D9BitmapReusableRealizationTargetState(
                    Bitmap: 10,
                    IsRenderTarget: true,
                    CanStretchRectFromTextures: true,
                    PrefilterWidth: 100,
                    PrefilterHeight: 80,
                    new Direct3D9BitmapRealizationRectangle(0, 0, 60, 60),
                    CachedUniquenessToken: 7),
                tryGetState ?? TryGetState,
                source => _next[source],
                SetNext,
                source => _referenceCalls.Add($"add:{source}"),
                source => _referenceCalls.Add($"release:{source}"),
                tryAdoptSystemMemorySurface ?? (source => _adoptCalls.Add(source)));

        internal void ClearCalls()
        {
            _referenceCalls.Clear();
            _adoptCalls.Clear();
        }

        private bool TryGetState(
            nint source,
            out Direct3D9BitmapReusableRealizationSourceState state) =>
            _states.TryGetValue(source, out state);

        private void SetNext(nint source, nint next)
        {
            _next[source] = next;
            _referenceCalls.Add($"set-next:{source}:{next}");
        }
    }
}
