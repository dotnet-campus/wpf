using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace NativeAcceptance.Pcore;

internal static partial class Program
{
    private static void RenderPureVideoPublic()
    {
        Evidence["caseId"] = "r16-public-failure-cancellation-lifecycle";
        Evidence["comFinalReferences"] = "Unknown: not instrumented";
        var events = new List<object>();
        var cleanup = new List<object>();
        Evidence["failureLifecycle"] = events;
        Evidence["cleanupDiagnostics"] = cleanup;
        string valid = Path.Combine(output, "recovery.avi");
        string invalid = Path.Combine(output, "invalid.avi");
        WriteMediaAsset(valid, 0, includeAudio: false);
        File.WriteAllBytes(invalid, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
        Evidence["validAsset"] = Identity(valid);
        Evidence["invalidAsset"] = Identity(invalid);
        var clock = Stopwatch.StartNew();
        var player = new MediaPlayer();
        int opened = 0, failed = 0, ended = 0;
        bool closeOnFailure = false;
        Exception? unexpected = null;
        player.MediaOpened += (_, _) => { opened++; events.Add(new { kind = "Opened", opened, elapsedMs = clock.ElapsedMilliseconds }); };
        player.MediaEnded += (_, _) => { ended++; events.Add(new { kind = "Ended", ended, elapsedMs = clock.ElapsedMilliseconds }); };
        player.MediaFailed += (_, e) =>
        {
            failed++;
            events.Add(new { kind = "Failed", failed, hr = $"0x{e.ErrorException.HResult:X8}", error = e.ErrorException.ToString(), elapsedMs = clock.ElapsedMilliseconds });
            if (closeOnFailure)
            {
                try { player.Close(); events.Add(new { kind = "Close-in-MediaFailed-return" }); }
                catch (Exception error) { unexpected = error; }
            }
            Checkpoint("public-failure-observed");
        };
        try
        {
            foreach (var source in new[]
            {
                new Uri(Path.Combine(output, "missing-" + Guid.NewGuid().ToString("N") + ".avi")),
                new Uri(invalid),
                new Uri("http://127.0.0.1:1/r16-missing.avi")
            })
            {
                closeOnFailure = true;
                int before = failed;
                int openedBefore = opened;
                player.Open(source);
                events.Add(new { kind = "Open-failure-source-return", source = source.AbsoluteUri });
                WaitMedia(() => failed > before, () => unexpected, "bounded public MediaFailed");
                if (failed != before + 1 || opened != openedBefore) throw new InvalidOperationException("Failure source unexpectedly opened or delivered duplicate failure.");
                closeOnFailure = false;
                Recover();
                Checkpoint("failure-recovery-completed");
            }
            for (int cycle = 0; cycle < 3; cycle++)
            {
                player.Close();
                player.Open(new Uri(valid));
                player.Close();
                player.Open(new Uri(valid));
                events.Add(new { kind = "immediate-open-close-open-return", cycle });
                int before = opened;
                int failedBefore = failed;
                WaitMedia(() => opened > before, () => unexpected, "cancelled-open replacement Opened");
                if (failed != failedBefore) throw new InvalidOperationException("Replacement received a failure.");
                AssertMetadata();
                player.Close();
                player.Close();
                events.Add(new { kind = "repeated-Close-return", cycle });
            }
            Evidence["publicCounts"] = new { opened, failed, ended };
            Evidence["scope"] = "Public Open failures, callback Close, recovery, immediate Open/Close/Open, repeated Close; not internal Flush failure injection, arbitrary admission proof or COM refs0";
        }
        finally
        {
            try { player.Close(); events.Add(new { kind = "final-Close-return" }); }
            catch (Exception error) { cleanup.Add(new { operation = "final-Close", error = error.ToString() }); }
            Checkpoint("failure-lifecycle-cleanup");
        }
        if (cleanup.Count != 0) throw new InvalidOperationException("Public cleanup failed.");

        void Recover()
        {
            int before = opened;
            int failedBefore = failed;
            player.Close();
            player.Open(new Uri(valid));
            WaitMedia(() => opened == before + 1, () => unexpected, "recovery Opened");
            if (failed != failedBefore) throw new InvalidOperationException("Recovery received a stale failure.");
            AssertMetadata();
            events.Add(new { kind = "recovery-metadata", player.HasVideo, player.HasAudio, player.NaturalVideoWidth, player.NaturalVideoHeight, duration = player.NaturalDuration.ToString() });
            player.Play();
            WaitMedia(() => player.Position >= TimeSpan.FromMilliseconds(200), () => unexpected, "recovery playback advances");
            player.Close();
            events.Add(new { kind = "playing-Close-return" });
        }

        void AssertMetadata()
        {
            if (!player.HasVideo || player.HasAudio || player.NaturalVideoWidth != 2 || player.NaturalVideoHeight != 1
                || !player.NaturalDuration.HasTimeSpan || player.NaturalDuration.TimeSpan != TimeSpan.FromSeconds(4))
                throw new InvalidOperationException("Unexpected recovery metadata.");
        }
    }
}
