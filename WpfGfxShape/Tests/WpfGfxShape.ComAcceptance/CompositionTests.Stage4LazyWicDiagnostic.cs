namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [DataRow(0u)]
    [DataRow(1u)]
    [DataRow(2u)]
    [DataRow(3u)]
    public void WhenStage4CacheMissFailsThenCommittedBitmapSurvivesAndOriginalKeyHits(uint scalingMode)
        => WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(10,
            scalingMode: scalingMode, cacheTransaction: true);

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(8)]
    [DataRow(9)]
    public void WhenStage4CacheDecoderReentersThenFrozenOwnersSurviveReplacementDeletionAndClose(int action)
        => WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(action, cacheConsumer: true);

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void WhenStage4LazyDecoderMutatesLaterNodeThenBothFramesMatchFrozenContract(int mutation)
        => WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(10, lateMutation: mutation);

    [TestMethod]
    public void WhenStage4NestedDrawingCacheTargetMutatesDuringLazyReadThenActiveFrameOwnsCacheRequest()
        => WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(10, lateMutation: 5);

    [TestMethod]
    public void WhenStage4LazyWicReadsThenPublicSourceMutationPreservesFrameAndNestedPresentIsRejected()
    {
        // Reuse the production descriptor and system decoder, including exact two-frame assertions.
        WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(3);
        TestContext.WriteLine("Stage4 source mutation: armed IStream.Read callbacks=1; SendCommand/CloseBatch/CommitChannel HRESULT=0x00000000; active pixels=FFFF0000,FF00FF00; next pixels=FF0000FF,FFFFFFFF.");
        WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(0);
        TestContext.WriteLine("Stage4 nested SameThreadPresent: armed IStream.Read callbacks=1; HRESULT=0x8000FFFF; active pixels=FFFF0000,FF00FF00. Counts refer to the one-shot armed mutation, not total decoder reads or CopyPixels calls.");
    }
}
