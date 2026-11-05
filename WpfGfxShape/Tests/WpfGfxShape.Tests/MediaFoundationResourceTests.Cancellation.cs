using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    [TestMethod]
    public async Task WhenRealReadCallbackIsAdmittedThenCloseFlushesAndDrainsAsync()
    {
        string root = Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT") ?? Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(root);
        string path = Path.Combine(root, "cancel-" + Guid.NewGuid().ToString("N") + ".avi");
        WriteSourceReaderAvi(path, false);
        var owner = new CancellableVideoReaderOwner();
        using var admitted = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        int delivered = 0;
        var timeline = new System.Collections.Concurrent.ConcurrentQueue<string>();
        void Record(string text) => timeline.Enqueue($"{Stopwatch.GetTimestamp()}:{text}");
        void Save()
        {
            string assembly = typeof(MediaFoundationResourceTests).Assembly.Location;
            File.WriteAllText(Path.Combine(root, "cancellable-reader.json"), JsonSerializer.Serialize(new
            {
                evidence = "real system OnReadSample admitted and held; source-level pending before callback Unknown",
                runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly))),
                mvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId,
                implementationMvid = typeof(CancellableVideoReaderOwner).Assembly.ManifestModule.ModuleVersionId,
                delivered, timeline = timeline.ToArray(), owner.ThreadExited,
                owner.Callback.ReadCallbacks, owner.Callback.FlushCallbacks, owner.Callback.SamplesRetained,
                owner.Callback.SamplesReleased, owner.Callback.Drained,
                owner.Reader?.FlushResult, owner.Reader?.ReaderFinalReferences, owner.Reader?.CallbackFinalReferences,
                owner.Reader?.ShutdownCount, owner.Reader?.UninitializeCount, cleanup = owner.Reader?.Cleanup,
                modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
                    .Where(m => m.ModuleName is "mfplat.dll" or "mfreadwrite.dll")
                    .Select(m => new { m.FileName, version = m.FileVersionInfo.FileVersion }).ToArray()
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        try
        {
            await owner.OpenAsync(path).WaitAsync(TimeSpan.FromSeconds(10));
            Record("Open completed");
            owner.Callback.ReadCallbackAdmitted = () =>
            {
                Record("native callback admitted with sample retained"); admitted.Set();
                if (!resume.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
                Record("native callback released");
            };
            Task read = owner.ReadAsync((_, _, _) => Interlocked.Increment(ref delivered));
            Assert.IsTrue(admitted.Wait(TimeSpan.FromSeconds(10)));
            Record("cancel accepted");
            Task close = owner.CloseAsync();
            Assert.AreSame(close, owner.CloseAsync());
            Assert.IsFalse(close.IsCompleted);
            Record("close incomplete while callback held");
            resume.Set();
            try { await read.WaitAsync(TimeSpan.FromSeconds(10)); Assert.Fail("Cancelled read published a frame"); }
            catch (OperationCanceledException) { Record("read cancelled"); }
            await close.WaitAsync(TimeSpan.FromSeconds(15));
            Record("close completed");
            Save();
            Assert.AreEqual(0, delivered);
            Assert.AreEqual(0, owner.Reader!.FlushResult);
            Assert.AreEqual(1, owner.Callback.FlushCallbacks);
            Assert.AreEqual(1, owner.Callback.ReadCallbacks);
            Assert.AreEqual(1, owner.Callback.SamplesRetained);
            Assert.AreEqual(1, owner.Callback.SamplesReleased);
            Assert.IsTrue(owner.Callback.Drained);
            Assert.AreEqual(0u, owner.Reader.ReaderFinalReferences);
            Assert.AreEqual(0u, owner.Reader.CallbackFinalReferences);
            Assert.AreEqual(1, owner.Reader.ShutdownCount);
            Assert.AreEqual(1, owner.Reader.UninitializeCount);
            Assert.IsTrue(owner.ThreadExited);
        }
        finally
        {
            resume.Set();
            try { await owner.CloseAsync().WaitAsync(TimeSpan.FromSeconds(15)); }
            finally { Save(); }
        }
    }

    [TestMethod]
    public async Task WhenAsyncReceiveRequestsCloseFromAnotherThreadThenAdmissionDoesNotHoldOwnerLockAsync()
    {
        string root = Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT") ?? Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(root);
        string path = Path.Combine(root, "async-reentrant-" + Guid.NewGuid().ToString("N") + ".avi");
        WriteSourceReaderAvi(path, false);
        var owner = new CancellableVideoReaderOwner();
        Task? close = null;
        int delivered = 0;
        try
        {
            await owner.OpenAsync(path).WaitAsync(TimeSpan.FromSeconds(10));
            await owner.ReadAsync((_, _, pixels) =>
            {
                Assert.IsNotNull(pixels);
                delivered++;
                // Wait only for Close admission, never for MTA cleanup from its own callback.
                Task admission = Task.Run(() => { close = owner.CloseAsync(); });
                Assert.IsTrue(admission.Wait(TimeSpan.FromSeconds(5)));
            }).WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsNotNull(close);
            await close.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(1, delivered);
            Assert.IsTrue(owner.ThreadExited);
            Assert.AreEqual(0u, owner.Reader!.ReaderFinalReferences);
            Assert.AreEqual(0u, owner.Reader.CallbackFinalReferences);
            File.WriteAllText(Path.Combine(root, "cancellable-reader-reentrant.json"), JsonSerializer.Serialize(new
            {
                delivered, owner.ThreadExited, owner.Callback.ReadCallbacks, owner.Callback.FlushCallbacks,
                owner.Reader.ReaderFinalReferences, owner.Reader.CallbackFinalReferences,
                owner.Reader.ShutdownCount, owner.Reader.UninitializeCount, cleanup = owner.Reader.Cleanup
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        finally
        {
            await owner.CloseAsync().WaitAsync(TimeSpan.FromSeconds(15));
        }
    }

    [TestMethod]
    public async Task WhenAsyncReaderReadsOriginalAviThenFortyFramesAndEosArePreservedAsync()
    {
        string root = Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT") ?? Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(root);
        string path = Path.Combine(root, "async-normal-" + Guid.NewGuid().ToString("N") + ".avi");
        WriteSourceReaderAvi(path, false);
        var owner = new CancellableVideoReaderOwner();
        int frames = 0;
        bool ended = false;
        long previous = -1;
        try
        {
            await owner.OpenAsync(path).WaitAsync(TimeSpan.FromSeconds(10));
            for (int index = 0; index < 45 && !ended; index++)
                await owner.ReadAsync((flags, timestamp, pixels) =>
                {
                    ended = (flags & 2) != 0;
                    if (pixels is null) return;
                    Assert.IsTrue(timestamp > previous);
                    Assert.AreEqual(8, pixels.Length);
                    previous = timestamp;
                    frames++;
                }).WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(40, frames);
            Assert.IsTrue(ended);
        }
        finally
        {
            await owner.CloseAsync().WaitAsync(TimeSpan.FromSeconds(15));
            File.WriteAllText(Path.Combine(root, "cancellable-reader-normal.json"), JsonSerializer.Serialize(new
            {
                frames, ended, previous, owner.ThreadExited, owner.Callback.ReadCallbacks,
                owner.Callback.SamplesRetained, owner.Callback.SamplesReleased,
                owner.Reader?.SampleReleaseCount, owner.Reader?.BufferReleaseCount, owner.Reader?.UnlockCount,
                cleanup = owner.Reader?.Cleanup
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
