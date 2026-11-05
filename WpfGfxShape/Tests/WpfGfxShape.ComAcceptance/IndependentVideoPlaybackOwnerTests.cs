extern alias Stage3Production;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class IndependentVideoPlaybackOwnerTests
{
    /// <summary>Uses the production opt-in reader and native MF callback admission barrier.</summary>
    [TestMethod]
    public Task WhenCancellableReaderClosesThenRealCallbackFlushAndDrainCompleteAsync()
        => new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenRealReadCallbackIsAdmittedThenCloseFlushesAndDrainsAsync();

    /// <summary>Verifies that external receive callbacks do not retain the owner admission lock.</summary>
    [TestMethod]
    public Task WhenCancellableReceiveClosesThenCrossThreadAdmissionAndCleanupCompleteAsync()
        => new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenAsyncReceiveRequestsCloseFromAnotherThreadThenAdmissionDoesNotHoldOwnerLockAsync();

    /// <summary>Preserves normal forty-frame decoding and EOS on the asynchronous backend.</summary>
    [TestMethod]
    public Task WhenCancellableReaderReadsThenFortyFramesReachEosAsync()
        => new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenAsyncReaderReadsOriginalAviThenFortyFramesAndEosArePreservedAsync();

    /// <summary>Combines source-linked SDK storage, real MF samples, forty-frame EOS and COM duration/seek contracts.</summary>
    [TestMethod]
    public async Task WhenCrossArchPropVariantRunsThenMfReadSeekAndDurationCompleteAsync()
    {
        var local = new WpfGfxShape.Tests.MediaFoundationResourceTests();
        local.WhenPropVariantScalarsAreStoredThenSdkOffsetsAndRangesArePreserved();
        local.WhenSystemSampleOwnsBufferThenBytesRoundTripAndResourcesClose();
        local.WhenOriginalAviIsReadBySystemSourceReaderThenAllVideoFramesReachEosAndClose();
        await new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenDurationSourceEndsThenReplayUsesNewEpochAndClockAnchor();
    }

    /// <summary>Validates the new internal factory/resource bridge and existing public guard and controls.</summary>
    [TestMethod]
    public async Task WhenStage15InternalBridgeRunsThenRealResourceAndControlsComplete()
    {
        var bridge = new Stage3Production::WpfGfxShape.Tests.IndependentMediaResourceBindingTests();
        bridge.WhenInternalCreateBindsTwoResourcesThenReferencesAndRegistryAreBalanced();
        bridge.WhenOpaqueIdentityIsResolvedThenItIsRejectedWithoutDereference();
        bridge.WhenClientIsNullThenInternalCreateClearsOutput(0);
        bridge.WhenClientIsNullThenInternalCreateClearsOutput(1);
        bridge.WhenClientIsNullThenInternalCreateClearsOutput(2);
        bridge.WhenListenerUnsubscribesReentrantlyThenLaterNotificationsAreRejected();
        var negative = new Stage3Production::WpfGfxShape.Tests.MediaCreationUnavailableTests();
        negative.WhenValidCreationIsRequestedThenIndependentIdentityCanCloseAndRelease(0);
        negative.WhenValidCreationIsRequestedThenIndependentIdentityCanCloseAndRelease(1);
        negative.WhenFactoryIsNullThenReturnsInvalidArgumentAndClearsOutput();
        negative.WhenOutputIsNullThenReturnsPointerError();
        var tests = new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests();
        await tests.WhenPublicVideoOnlyPolicySeesAudioThenRealOpenFailsBeforePrerollAsync();
        await tests.WhenInternalBridgePlaysThenResourceNotificationsAndDrawVideoAreReal();
        await tests.WhenComControlsPauseSeekResumeThenRealEventsAndDrawVideoRemainOrdered();
        await tests.WhenPausedScrubSeeksThenRealFramesAndUiEpochsRemainPaused();
        await tests.WhenDurationSourceEndsThenReplayUsesNewEpochAndClockAnchor();
        await tests.WhenRealOpenEpochsPrerollThenOpenedPrecedesOnePrerolledAsync();
    }

    /// <summary>Runs the final shutdown, exit and same-proxy reopen system contract.</summary>
    [TestMethod]
    public async Task WhenStage13ShutdownExitAndReopenRunThenRealLifecycleCompletes()
    {
        var tests = new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests();
        await tests.WhenClosedSourceReopensThenSameProxyReceivesOpenedAndFrames();
        await WhenLifecycleClosesThenCallbacksAndMtaCleanupComplete();
    }

    /// <summary>Runs fixed-handshake close and final-release cases against the real MF owner.</summary>
    [TestMethod]
    public async Task WhenLifecycleClosesThenCallbacksAndMtaCleanupComplete()
    {
        var pure = new Stage3Production::WpfGfxShape.Tests.IndependentMediaLifecycleTests();
        pure.WhenCallbackClosesOrReleasesIdentityThenCallbackReturnsWithoutSelfJoin(false);
        pure.WhenCallbackClosesOrReleasesIdentityThenCallbackReturnsWithoutSelfJoin(true);
        pure.WhenTwoExternalClosersRaceThenBothAreAcceptedAndNewOperationsAreRejected();
        pure.WhenProxyClosesWithAnAdmittedCallbackThenQueuedAndLateEventsAreRejected();
        pure.WhenTerminalSlotRunsThenItIsDistinctAndControlsAreRejected(22);
        pure.WhenTerminalSlotRunsThenItIsDistinctAndControlsAreRejected(23);
        await new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenMissingOpenFailsThenCallbackCanCloseAndRelease();
        await new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenLifecycleCallbacksRunThenRealCloseAndReleaseDrainBeforeCompletion();
    }

    [TestMethod]
    public async Task WhenComIdentityAndRegressionRunThenRealTransportAndStatesPass()
    {
        var identity = new Stage3Production::WpfGfxShape.Tests.IndependentMediaIdentityTests();
        identity.WhenQueriedThenUnknownAndMediaHaveSameIdentityAndBalancedReferences();
        identity.WhenUnsupportedInterfaceIsQueriedThenOutputIsCleared();
        identity.WhenOneInstanceClosesThenOtherRemainsIndependentAndUnsupportedIsPrecise();
        await new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenComIdentityPlaysThenRealStateAndTransportReachDrawVideo();
    }

    /// <summary>Exercises the production COM event proxy before the real AVI controls scenario.</summary>
    [TestMethod]
    public async Task WhenPublicComProxyRunsThenPacketsAndRealVideoControlsPass()
    {
        var bridge = new Stage3Production::WpfGfxShape.Tests.MediaComEventBridgeTests();
        bridge.WhenComClientIsNullThenBindingFailsWithoutAnOwner();
        bridge.WhenProductionComProxyIsBridgedThenPacketAndFailureArePreservedUntilFinalRelease();
        await new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenWpfStatusCasesRunThenRealMetadataRequestsAndFailureArePreserved();
        await new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenComControlsPauseSeekResumeThenRealEventsAndDrawVideoRemainOrdered();
    }

    /// <summary>Runs real paused scrubbing and the existing video controls regression through COM.</summary>
    [TestMethod]
    public async Task WhenScrubbingRunsThenRealPausedFramesAndControlsPass()
    {
        var tests = new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests();
        await tests.WhenPausedScrubSeeksThenRealFramesAndUiEpochsRemainPaused();
        await tests.WhenComControlsPauseSeekResumeThenRealEventsAndDrawVideoRemainOrdered();
    }

    [TestMethod]
    public Task WhenControlsAndEventsRunThenPauseSeekResumeReachProductionDrawVideo()
        => new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenComControlsPauseSeekResumeThenRealEventsAndDrawVideoRemainOrdered();

    /// <summary>Runs the duration, controls, rendering, events and replay acceptance together.</summary>
    [TestMethod]
    public async Task WhenDurationControlsAndReplayRunThenProductionContractIsPreserved()
    {
        var tests = new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests();
        await tests.WhenComControlsPauseSeekResumeThenRealEventsAndDrawVideoRemainOrdered();
        await tests.WhenDurationSourceEndsThenReplayUsesNewEpochAndClockAnchor();
    }

    [TestMethod]
    public Task WhenProductionDrawVideoRendersThenTimedRedBlueReachSoftwareRtb()
        => new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenPlayerDrawVideoRendersThenTimedRedBlueReachSoftwareTarget();

    /// <summary>Runs Stage19 real MF rate and per-open preroll ordering on the sandbox system.</summary>
    [TestMethod]
    public async Task WhenStage19RealMfRunsThenSeekingRatesAndOpenEpochOrderArePreservedAsync()
    {
        var tests = new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests();
        await tests.WhenRealOpenEpochsPrerollThenOpenedPrecedesOnePrerolledAsync();
        await tests.WhenRateIsSetDuringRealSeekingThenDeferredIntentIsAcceptedAsync(0);
        await tests.WhenRateIsSetDuringRealSeekingThenDeferredIntentIsAcceptedAsync(1);
    }

    [TestMethod]
    public Task WhenRealMfOwnerPlaysThenFortyPtsFramesReachEosAndClose()
        => new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenDedicatedOwnerPlaysThenFortyPtsFramesReachEosAndCloseStopsAcquisitions();
}
