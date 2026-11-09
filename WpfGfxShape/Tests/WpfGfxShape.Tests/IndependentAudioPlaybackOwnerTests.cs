using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class IndependentAudioPlaybackOwnerTests
{
    // Existing friend assembly access; no new production visibility or output substitute.
    [TestMethod]
    [DataRow(100u, 25u, 600, 2u, 75u)]
    [DataRow(100u, 100u, 600, 2u, 0u)]
    [DataRow(100u, 0u, 16, 8u, 2u)]
    [DataRow(100u, 0u, 0, 2u, 0u)]
    public void WhenPaddingLimitsSpaceThenOnlyWholeWritableFramesAreReturned(uint capacity, uint padding, int bytes, uint align, uint expected)
    {
        AudioPcmFormat format = align == 8 ? new(2, 48000, 32, 8, true) : new(1, 8000, 16, 2, false);
        Assert.AreEqual(expected, WasapiSharedOutput.WritableFrames(capacity, padding, bytes, format));
    }

    [TestMethod]
    [DataRow(101u, 2)]
    [DataRow(0u, 3)]
    public void WhenPaddingOrPacketIsInvalidThenWritableFramesRejectsIt(uint padding, int bytes)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => WasapiSharedOutput.WritableFrames(100, padding, bytes, new(1, 8000, 16, 2, false)));
    }

    [TestMethod]
    public async Task WhenSourceInitializationFailsThenOwnerCompletesWithOriginalHresult()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav");
        IndependentAudioPlaybackOwner owner = new(path);
        int result = await owner.Completion.WaitAsync(TimeSpan.FromSeconds(20));
        Console.WriteLine(string.Join(Environment.NewLine, owner.Evidence));
        // The containing directory does not exist: ERROR_PATH_NOT_FOUND, not FILE_NOT_FOUND.
        Assert.AreEqual(unchecked((int)0x80070003), result);
    }

    [TestMethod]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    [DataRow(-0.01)]
    [DataRow(1.01)]
    public async Task WhenVolumeIsOutsidePublicRangeThenItIsRejected(double value)
    {
        IndependentAudioPlaybackOwner owner = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav"));
        int result = await owner.SetVolumeAsync(value);
        await owner.Completion.WaitAsync(TimeSpan.FromSeconds(20));
        Assert.AreEqual(unchecked((int)0x80070057), result);
    }

    [TestMethod]
    public async Task WhenInitializationFailsThenEveryAdmittedCommandCompletesWithOriginalError()
    {
        IndependentAudioPlaybackOwner owner = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav"));
        Task<int>[] commands = [owner.PauseAsync(), owner.SetVolumeAsync(0), owner.SetVolumeAsync(0.5), owner.SetVolumeAsync(1), owner.ResumeAsync(), owner.SeekAsync(20000000), owner.SeekAsync(10000000), owner.SetBalanceAsync(-1), owner.SetBalanceAsync(0), owner.SetBalanceAsync(1)];
        int[] results = await Task.WhenAll(commands).WaitAsync(TimeSpan.FromSeconds(20));
        await owner.Completion.WaitAsync(TimeSpan.FromSeconds(20));
        Console.WriteLine(string.Join(Environment.NewLine, owner.Evidence));
        CollectionAssert.AreEqual(Enumerable.Repeat(unchecked((int)0x80070003), 10).ToArray(), results);
    }

    [TestMethod]
    public async Task WhenFailedOwnerHasExitedThenLaterCommandsRetainOriginalError()
    {
        IndependentAudioPlaybackOwner owner = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav"));
        await owner.Completion.WaitAsync(TimeSpan.FromSeconds(20));
        int[] results = await Task.WhenAll(owner.PauseAsync(), owner.ResumeAsync(), owner.SetVolumeAsync(0), owner.SeekAsync(20000000), owner.SetBalanceAsync(0));
        CollectionAssert.AreEqual(Enumerable.Repeat(unchecked((int)0x80070003), 5).ToArray(), results);
    }

    [TestMethod]
    public async Task WhenCloseRacesCommandsThenNoCommandWaitIsLost()
    {
        IndependentAudioPlaybackOwner owner = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav"));
        Task<int>[] commands = [owner.PauseAsync(), owner.ResumeAsync(), owner.SetVolumeAsync(0), owner.SeekAsync(20000000), owner.SeekAsync(10000000), owner.SetBalanceAsync(0.5)];
        int close = await owner.CloseAsync(TimeSpan.FromSeconds(20));
        Assert.AreEqual(close, await owner.Ready.WaitAsync(TimeSpan.FromSeconds(2)));
        var metadataError = await Assert.ThrowsAsync<Exception>(async () => await owner.Metadata.WaitAsync(TimeSpan.FromSeconds(2)));
        Assert.AreEqual(close, metadataError.HResult);
        int[] results = await Task.WhenAll(commands).WaitAsync(TimeSpan.FromSeconds(2));
        CollectionAssert.AreEqual(Enumerable.Repeat(close, 6).ToArray(), results);
    }

    [TestMethod]
    public async Task WhenOwnerHasExitedThenCloseIsRepeatable()
    {
        IndependentAudioPlaybackOwner owner = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav"));
        int result = await owner.Completion.WaitAsync(TimeSpan.FromSeconds(20));
        Assert.AreEqual(result, await owner.CloseAsync(TimeSpan.FromSeconds(1)));
    }
}

public sealed partial class IndependentAudioTests
{
    [TestMethod]
    public async Task WhenWaveMetadataSucceedsThenReadinessStillRequiresRealOutput()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string wave = Path.Combine(root, "ready.wav");
        File.WriteAllBytes(wave, MakeWave(new byte[64000]));
        IndependentAudioPlaybackOwner owner = new(wave, initiallyPaused: true);
        IndependentAudioPlaybackOwner.AudioMetadata metadata = await owner.Metadata.WaitAsync(TimeSpan.FromSeconds(20));
        int ready = await owner.Ready.WaitAsync(TimeSpan.FromSeconds(20));
        Task<int>[] commands = [owner.PauseAsync(), owner.SetVolumeAsync(0.5)];
        int[] controls = await Task.WhenAll(commands).WaitAsync(TimeSpan.FromSeconds(5));
        bool completionBeforeClose = owner.Completion.IsCompleted;
        ulong framesBeforeClose = owner.SubmittedFrames;
        int completion = await owner.CloseAsync(TimeSpan.FromSeconds(20));
        string results = Path.Combine(AppContext.BaseDirectory, "TestResults");
        Directory.CreateDirectory(results);
        File.WriteAllText(Path.Combine(results, "audio-ready.json"), JsonSerializer.Serialize(new
        {
            metadata, ready, completion, controls, completionBeforeClose, framesBeforeClose,
            owner.SubmittedFrames, owner.Evidence,
            status = ready == unchecked((int)0x80070490) ? "DeviceBlocked" : "InitializedPaused",
            scope = "Readiness error propagation contract, not real output acceptance"
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.AreEqual(new AudioPcmFormat(1, 8000, 16, 2, false), metadata.Format);
        Assert.AreEqual(40000000ul, metadata.Duration100ns);
        Assert.AreEqual(0ul, framesBeforeClose);
        Assert.IsTrue(ready == 0 || ready == unchecked((int)0x80070490), $"Unexpected readiness HRESULT: 0x{ready:X8}");
        Assert.AreEqual(ready < 0, completionBeforeClose);
        Assert.AreEqual(ready < 0 ? ready : unchecked((int)0x800704c7), completion);
        CollectionAssert.AreEqual(new[] { ready < 0, ready < 0 }, controls.Select(result => result < 0).ToArray());
        Assert.IsTrue(controls.All(result => ready >= 0 || result == ready));
    }

    [TestMethod]
    public async Task WhenMissingSourceOpensThenReadyAndMetadataFailWithoutWaitingForEos()
    {
        IndependentAudioPlaybackOwner owner = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav"), initiallyPaused: true);
        Task<int>[] commands = [owner.PauseAsync(), owner.ResumeAsync(), owner.SetVolumeAsync(0)];
        int ready = await owner.Ready.WaitAsync(TimeSpan.FromSeconds(20));
        var error = await Assert.ThrowsExactlyAsync<DirectoryNotFoundException>(async () => await owner.Metadata.WaitAsync(TimeSpan.FromSeconds(2)));
        int completion = await owner.Completion.WaitAsync(TimeSpan.FromSeconds(2));
        int[] controls = await Task.WhenAll(commands).WaitAsync(TimeSpan.FromSeconds(2));
        Assert.AreEqual(unchecked((int)0x80070003), ready);
        Assert.AreEqual(ready, error.HResult);
        Assert.AreEqual(ready, completion);
        CollectionAssert.AreEqual(new[] { ready, ready, ready }, controls);
    }

    [TestMethod]
    public async Task WhenPcmOwnerRendersWaveThenRealEndpointAndDrainAreRequired()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string wave = Path.Combine(root, "source.wav");
        // Exact four seconds of PCM; no container experiment and no test output sink.
        File.WriteAllBytes(wave, MakeWave(new byte[64000]));
        IndependentAudioPlaybackOwner owner = new(wave);
        Task<int>[] controls = [owner.PauseAsync(), owner.SetVolumeAsync(0), owner.SetVolumeAsync(0.5), owner.ResumeAsync()];
        int result = await owner.Completion.WaitAsync(TimeSpan.FromSeconds(25));
        int[] commandResults = await Task.WhenAll(controls).WaitAsync(TimeSpan.FromSeconds(2));
        string results = Path.Combine(AppContext.BaseDirectory, "TestResults");
        Directory.CreateDirectory(results);
        File.WriteAllText(Path.Combine(results, "pcm-owner.json"), JsonSerializer.Serialize(new
        {
            result, hr = $"0x{result:X8}", owner.SubmittedFrames, owner.Evidence, commandResults,
            expectedFrames = 32000, scope = "Internal PCM owner, not public audio acceptance"
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.IsTrue(result == 0 && owner.SubmittedFrames == 32000, $"HRESULT=0x{result:X8}; frames={owner.SubmittedFrames}");
    }
}
