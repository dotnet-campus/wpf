using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapReusableRealizationCandidatesTests
{
    [TestMethod]
    public void WhenCandidateIsAddedThenItsPriorChainIsReleasedBeforeItBecomesHead()
    {
        CandidateHarness harness = new();
        harness.SetNext(1, 2);
        harness.SetNext(2, 0);
        harness.ClearCalls();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();

        candidates.Add(1);

        Assert.AreEqual(
            (1, "set-next:2:0,release:2,set-next:1:0,add:1"),
            (candidates.Head, harness.Calls));
    }

    [TestMethod]
    public void WhenCandidatesAreAddedThenTheyArePrependedAndOwnIndependentReferences()
    {
        CandidateHarness harness = new();
        harness.SetNext(1, 0);
        harness.SetNext(2, 0);
        harness.ClearCalls();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();

        candidates.Add(1);
        candidates.Add(2);

        Assert.AreEqual(
            (2, 1, "set-next:1:0,add:1,set-next:2:1,add:2"),
            (candidates.Head, harness.GetNext(2), harness.Calls));
    }

    [TestMethod]
    public void WhenCandidatesAreDetachedThenOwnershipTransfersWithoutRelease()
    {
        CandidateHarness harness = new();
        harness.SetNext(1, 0);
        harness.ClearCalls();
        Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(1);
        harness.ClearCalls();

        nint head = candidates.Detach();
        nint remainingHead = candidates.Head;
        candidates.Dispose();

        Assert.AreEqual((1, 0, string.Empty), (head, remainingHead, harness.Calls));
    }

    [TestMethod]
    public void WhenCandidatesAreDisposedThenEveryOwnedLinkIsClearedAndReleased()
    {
        CandidateHarness harness = new();
        harness.SetNext(1, 0);
        harness.SetNext(2, 0);
        Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(1);
        candidates.Add(2);
        harness.ClearCalls();

        candidates.Dispose();
        candidates.Dispose();

        Assert.AreEqual("set-next:2:0,release:2,set-next:1:0,release:1", harness.Calls);
    }

    private sealed class CandidateHarness
    {
        private readonly Dictionary<nint, nint> _next = [];
        private readonly List<string> _calls = [];

        internal string Calls => string.Join(',', _calls);

        internal Direct3D9BitmapReusableRealizationCandidates CreateCandidates() =>
            new(
                GetNext,
                SetNext,
                source => _calls.Add($"add:{source}"),
                source => _calls.Add($"release:{source}"));

        internal nint GetNext(nint source) => _next[source];

        internal void SetNext(nint source, nint next)
        {
            _next[source] = next;
            _calls.Add($"set-next:{source}:{next}");
        }

        internal void ClearCalls() => _calls.Clear();
    }
}
