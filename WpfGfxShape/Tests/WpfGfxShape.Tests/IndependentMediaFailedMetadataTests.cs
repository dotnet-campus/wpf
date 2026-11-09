using System.Reflection;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class IndependentMediaFailedMetadataTests
{
    [DataTestMethod]
    [DataRow(true, true, 640u, 360u, 7438266666L, 0)]
    [DataRow(true, false, 0u, 0u, 40000000L, 0)]
    [DataRow(false, true, 2u, 1u, 40000000L, 0)]
    [DataRow(false, false, 0u, 0u, 0L, unchecked((int)0xc00d36e6))]
    public void WhenFailedThenComGettersReturnKnownMetadataWithoutChangingFailure(bool audio, bool video, uint width, uint height, long duration, int metadataResult)
    {
        var media = IndependentMediaIdentity.Create();
        var owner = new IndependentVideoPlaybackOwner();
        const int failure = unchecked((int)0xc00d36b4);
        try
        {
            Set(owner, "HasAudio", metadataResult == 0 ? (bool?)audio : null);
            Set(owner, "AudioMetadataResult", metadataResult);
            Set(owner, "Width", width);
            Set(owner, "Height", height);
            Set(owner, "Duration100ns", metadataResult == 0 ? (long?)duration : null);
            Set(owner, "DurationResult", metadataResult);
            typeof(IndependentMediaIdentity).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            Set(media, "State", IndependentMediaState.Failed);
            Set(media, "Failure", failure);
            nint* table = *(nint**)media.Pointer;
            byte actualAudio = 9, actualVideo = 9;
            uint actualWidth = 9, actualHeight = 9;
            long actualDuration = -1;
            int audioHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[17])(media.Pointer, &actualAudio);
            int videoHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[16])(media.Pointer, &actualVideo);
            int widthHr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[19])(media.Pointer, &actualWidth);
            int heightHr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[18])(media.Pointer, &actualHeight);
            int lengthHr = ((delegate* unmanaged[Stdcall]<nint, long*, int>)table[20])(media.Pointer, &actualDuration);
            Assert.AreEqual((0, audio ? (byte)1 : (byte)0, 0, video ? (byte)1 : (byte)0, 0, width, 0, height, 0, duration, failure),
                (audioHr, actualAudio, videoHr, actualVideo, widthHr, actualWidth, heightHr, actualHeight, lengthHr, actualDuration, media.Failure));
        }
        finally
        {
            nint* table = *(nint**)media.Pointer;
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(media.Pointer);
            ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(media.Pointer);
        }
    }

    [TestMethod]
    public void WhenNullSourceReplacesFailedMediaThenMetadataAndFramesAreCleared()
    {
        var media = IndependentMediaIdentity.Create();
        var owner = new IndependentVideoPlaybackOwner();
        nint* table = *(nint**)media.Pointer;
        try
        {
            typeof(IndependentMediaIdentity).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            typeof(IndependentMediaIdentity).GetField("_path", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, "old-source");
            Set(media, "State", IndependentMediaState.Failed);
            Set(media, "Failure", unchecked((int)0xc00d36b4));
            Set(owner, "Width", 640u);
            Set(owner, "Height", 360u);
            Set(owner, "Duration100ns", (long?)40000000);
            Set(owner, "LastTimestamp", 20000000L);
            owner.Frames.Publish(new byte[4], 1, 1, 0, 0, 1, 1, 20000000);
            int open = ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(media.Pointer, null);
            uint width = 9, height = 9;
            long length = -1, position = -1;
            double progress = -1;
            int widthHr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[19])(media.Pointer, &width);
            int heightHr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[18])(media.Pointer, &height);
            int lengthHr = ((delegate* unmanaged[Stdcall]<nint, long*, int>)table[20])(media.Pointer, &length);
            int positionHr = ((delegate* unmanaged[Stdcall]<nint, long*, int>)table[6])(media.Pointer, &position);
            int progressHr = ((delegate* unmanaged[Stdcall]<nint, double*, int>)table[14])(media.Pointer, &progress);
            int frameHr = media.AcquireFrame(out SystemVideoSample? frame);
            frame?.Dispose();
            Assert.AreEqual((0, 0, 0u, 0, 0u, 0, 0L, 0, 0L, 0, 0d, 0, true, 0),
                (open, widthHr, width, heightHr, height, lengthHr, length, positionHr, position, progressHr, progress, frameHr, frame is null, media.Failure));
        }
        finally
        {
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(media.Pointer);
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(media.Pointer);
        }
    }

    [DataTestMethod]
    [DataRow(IndependentMediaState.Closed)]
    [DataRow(IndependentMediaState.Opening)]
    [DataRow(IndependentMediaState.Opened)]
    [DataRow(IndependentMediaState.Paused)]
    [DataRow(IndependentMediaState.Playing)]
    [DataRow(IndependentMediaState.Seeking)]
    [DataRow(IndependentMediaState.Ended)]
    [DataRow(IndependentMediaState.Failed)]
    public void WhenSourceChangesThenOldCallbacksCannotChangeReplacement(int state)
    {
        var media = IndependentMediaIdentity.Create();
        var owner = new IndependentVideoPlaybackOwner();
        nint* table = *(nint**)media.Pointer;
        try
        {
            typeof(IndependentMediaIdentity).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            typeof(IndependentMediaIdentity).GetField("_path", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, "old-source");
            typeof(IndependentMediaIdentity).GetField("_deferredRate", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, (double?)1);
            Set(media, "State", (IndependentMediaState)state);
            string replacement = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.mp4");
            int result;
            fixed (char* path = replacement) result = ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(media.Pointer, path);
            IndependentVideoPlaybackOwner? current = media.Owner;
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            var before = (media.State, media.Failure, media.UiFrameGeneration, media.Completion);
            typeof(IndependentMediaIdentity).GetMethod("OnFailed", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(media, [owner, unchecked((int)0x80004005)]);
            typeof(IndependentMediaIdentity).GetMethod("OnFrame", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(media, [owner, 123L]);
            typeof(IndependentMediaIdentity).GetMethod("OnEnded", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(media, [owner]);
            Assert.AreEqual((0, false, before),
                (result, ReferenceEquals(owner, current), (media.State, media.Failure, media.UiFrameGeneration, media.Completion)));
        }
        finally
        {
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(media.Pointer);
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(media.Pointer);
        }
    }

    [DataTestMethod]
    [DataRow(IndependentMediaState.Opening)]
    [DataRow(IndependentMediaState.Playing)]
    [DataRow(IndependentMediaState.Paused)]
    [DataRow(IndependentMediaState.Failed)]
    public void WhenSameSourceIsOpenedThenOwnerStateAndOperationAreUnchanged(int state)
    {
        var media = IndependentMediaIdentity.Create();
        var owner = new IndependentVideoPlaybackOwner();
        nint* table = *(nint**)media.Pointer;
        try
        {
            const string source = "same-source";
            typeof(IndependentMediaIdentity).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            typeof(IndependentMediaIdentity).GetField("_path", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, source);
            Set(media, "State", (IndependentMediaState)state);
            Set(media, "Failure", -123);
            Task operation = media.Completion;
            int result;
            fixed (char* path = source) result = ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(media.Pointer, path);
            Assert.AreEqual((0, owner, (IndependentMediaState)state, -123, operation),
                (result, media.Owner, media.State, media.Failure, media.Completion));
        }
        finally
        {
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(media.Pointer);
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(media.Pointer);
        }
    }

    /// <summary>Verifies source clearing notifies an existing subscriber but same-source opens do not.</summary>
    [TestMethod]
    public void WhenNullSourceClearsMediaThenExistingSubscriberIsNotifiedOnlyForSourceChange()
    {
        var media = IndependentMediaIdentity.Create();
        var owner = new IndependentVideoPlaybackOwner();
        nint* table = *(nint**)media.Pointer;
        int notifications = 0;
        Action listener = () => notifications++;
        try
        {
            const string source = "same-source";
            typeof(IndependentMediaIdentity).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            typeof(IndependentMediaIdentity).GetField("_path", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, source);
            Set(media, "State", IndependentMediaState.Failed);
            media.Source.Subscribe(listener);
            int same;
            fixed (char* path = source) same = ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(media.Pointer, path);
            int afterSame = notifications;
            int clear = ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(media.Pointer, null);
            int afterClear = notifications;
            int repeatedClear = ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(media.Pointer, null);
            Assert.AreEqual((0, 0, 0, 1, 0, 1),
                (same, afterSame, clear, afterClear, repeatedClear, notifications));
        }
        finally
        {
            media.Source.Unsubscribe(listener);
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(media.Pointer);
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(media.Pointer);
        }
    }

    /// <summary>Checks the public COM return boundary; reflection assembly is not a Native scheduling test.</summary>
    [DataTestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void WhenReplacementOpensThenComQueriesKeepPreviousMetadata(bool audio, bool video)
    {
        var media = IndependentMediaIdentity.Create();
        var owner = new IndependentVideoPlaybackOwner();
        nint* table = *(nint**)media.Pointer;
        try
        {
            typeof(IndependentMediaIdentity).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            typeof(IndependentMediaIdentity).GetField("_path", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, "old-source");
            Set(media, "State", IndependentMediaState.Opened);
            Set(owner, "HasAudio", (bool?)audio);
            Set(owner, "Width", video ? 2u : 0u);
            Set(owner, "Height", video ? 1u : 0u);
            string replacement = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.mp4");
            object gate = typeof(IndependentMediaIdentity).GetField("_gate", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(media)!;
            lock (gate)
            {
                int result;
                fixed (char* path = replacement) result = ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(media.Pointer, path);
                var opening = QueryPresence(media);
                Set(owner, "HasAudio", (bool?)!audio);
                Set(owner, "Width", video ? 0u : 2u);
                Set(owner, "Height", video ? 0u : 1u);
                typeof(IndependentMediaIdentity).GetMethod("PublishMetadata", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(media, [owner]);
                var afterStale = QueryPresence(media);
                Task operation = media.Completion;
                int same;
                fixed (char* path = replacement) same = ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(media.Pointer, path);
                Assert.AreEqual((0, IndependentMediaState.Opening, (0, audio, 0, video), opening, 0, operation),
                    (result, media.State, opening, afterStale, same, media.Completion));
            }
        }
        finally
        {
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(media.Pointer);
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(media.Pointer);
        }
    }

    /// <summary>Checks matching metadata replaces cached values in either direction through COM queries.</summary>
    [DataTestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void WhenCurrentOwnerPublishesThenOpeningComQueriesUseItsMetadata(bool audio, bool video)
    {
        var media = IndependentMediaIdentity.Create();
        var owner = new IndependentVideoPlaybackOwner();
        nint* table = *(nint**)media.Pointer;
        try
        {
            typeof(IndependentMediaIdentity).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            typeof(IndependentMediaIdentity).GetField("_hasAudio", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, !audio);
            typeof(IndependentMediaIdentity).GetField("_hasVideo", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, !video);
            Set(media, "State", IndependentMediaState.Opening);
            Set(owner, "HasAudio", (bool?)audio);
            Set(owner, "Width", video ? 2u : 0u);
            Set(owner, "Height", video ? 1u : 0u);
            typeof(IndependentMediaIdentity).GetMethod("PublishMetadata", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(media, [owner]);
            Assert.AreEqual((0, audio, 0, video), QueryPresence(media));
        }
        finally
        {
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(media.Pointer);
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(media.Pointer);
        }
    }

    /// <summary>Checks no invented metadata on first open and preserves existing null-source clearing.</summary>
    [TestMethod]
    public void WhenFirstOpeningOrClearedThenComPresenceIsFalse()
    {
        var media = IndependentMediaIdentity.Create();
        var owner = new IndependentVideoPlaybackOwner();
        nint* table = *(nint**)media.Pointer;
        try
        {
            typeof(IndependentMediaIdentity).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            typeof(IndependentMediaIdentity).GetField("_path", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, "first-source");
            Set(media, "State", IndependentMediaState.Opening);
            var first = QueryPresence(media);
            Set(owner, "HasAudio", (bool?)true);
            Set(owner, "Width", 2u);
            Set(owner, "Height", 1u);
            typeof(IndependentMediaIdentity).GetMethod("PublishMetadata", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(media, [owner]);
            int clear = ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(media.Pointer, null);
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            Assert.AreEqual(((0, false, 0, false), 0, (0, false, 0, false)), (first, clear, QueryPresence(media)));
        }
        finally
        {
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(media.Pointer);
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(media.Pointer);
        }
    }

    private static (int AudioResult, bool Audio, int VideoResult, bool Video) QueryPresence(IndependentMediaIdentity media)
    {
        nint* table = *(nint**)media.Pointer;
        byte audio = 9, video = 9;
        int audioResult = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[17])(media.Pointer, &audio);
        int videoResult = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[16])(media.Pointer, &video);
        return (audioResult, audio != 0, videoResult, video != 0);
    }

    private static void Set(object target, string property, object? value)
        => target.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);
}
