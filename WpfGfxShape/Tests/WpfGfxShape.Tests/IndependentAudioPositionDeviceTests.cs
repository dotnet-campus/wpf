using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class IndependentAudioTests
{
    /// <summary>Requires a real endpoint; absence remains a failing hardware gate, never a skipped or simulated pass.</summary>
    [TestMethod]
    public async Task WhenAudioDeviceClockRunsThenPauseResumeAndDrainPreservePosition()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string wave = Path.Combine(root, "position.wav");
        File.WriteAllBytes(wave, MakeWave(new byte[64000]));
        IndependentAudioPlaybackOwner owner = new(wave, initiallyPaused: true);
        int ready = await owner.Ready.WaitAsync(TimeSpan.FromSeconds(20));
        long paused = 0, resumed = 0, ended = 0;
        try
        {
            Assert.AreEqual(0, ready, $"Device clock Blocked HRESULT=0x{ready:X8}");
            Assert.AreEqual(0L, owner.Position100ns);
            Assert.AreEqual(0, await owner.ResumeAsync().WaitAsync(TimeSpan.FromSeconds(5)));
            await WaitForAudioPositionAsync(() => owner.Position100ns > 0);
            Assert.AreEqual(0, await owner.PauseAsync().WaitAsync(TimeSpan.FromSeconds(5)));
            paused = owner.Position100ns;
            await Task.Delay(100);
            Assert.AreEqual(paused, owner.Position100ns);
            Assert.AreEqual(0, await owner.ResumeAsync().WaitAsync(TimeSpan.FromSeconds(5)));
            await WaitForAudioPositionAsync(() => owner.Position100ns > paused);
            resumed = owner.Position100ns;
            Assert.AreEqual(0, await owner.Completion.WaitAsync(TimeSpan.FromSeconds(25)));
            ended = owner.Position100ns;
            Assert.IsTrue(ended >= resumed && ended <= 40000000);
            await Task.Delay(50);
            Assert.AreEqual(ended, owner.Position100ns);
        }
        finally
        {
            await owner.CloseAsync(TimeSpan.FromSeconds(20));
            string results = Path.Combine(AppContext.BaseDirectory, "TestResults");
            Directory.CreateDirectory(results);
            File.WriteAllText(Path.Combine(results, "audio-position-device.json"), JsonSerializer.Serialize(new
            {
                ready, paused, resumed, ended, closed = owner.Position100ns,
                owner.SubmittedFrames, owner.Evidence,
                scope = "Managed real device clock; no Native equivalence claim"
            }));
        }
        Assert.AreEqual(0L, owner.Position100ns);
    }

    private static async Task WaitForAudioPositionAsync(Func<bool> condition)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));
        while (!condition()) await Task.Delay(10, timeout.Token);
    }
}
