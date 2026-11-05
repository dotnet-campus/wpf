using System.Diagnostics;
using System.Text.Json;
using WpfGfxShape.Core;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    [TestMethod]
    public async Task WhenPlayerDrawVideoRendersThenTimedRedBlueReachSoftwareTarget()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-player-render-stage4");
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        GeneratedProtocolHandleTable table = new();
        GeneratedProtocolChannelRegistry registry = new();
        Assert.IsTrue(registry.TryAdd(1, table));
        GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, registry).CreateRouter();
        Assert.AreEqual(0, table.Create(1, MilResourceType.MediaPlayer));
        Assert.AreEqual(0, table.Create(2, MilResourceType.RenderData));
        Assert.AreEqual(0, table.Create(3, MilResourceType.Visual));
        Assert.AreEqual(0, router.ProcessPackets([
            GeneratedProtocolPacketWriter.WriteVisualCreate(3),
            GeneratedProtocolPacketWriter.WriteRenderData(2, GeneratedProtocolPacketWriter.WriteDrawVideoRecord(new(0, 0, 2, 1), 1)),
            GeneratedProtocolPacketWriter.WriteVisualSetContent(3, 2)]));
        Assert.IsTrue(table.TryGetResource(1, out var media));
        Assert.IsTrue(table.TryGetResource(3, out var visual));
        GeneratedMediaPlayerResource resource = (GeneratedMediaPlayerResource)media!;
        IndependentVideoPlaybackOwner player = new();
        resource.BindIndependentPlayer(player);
        List<object> frames = [], samples = [];
        Stopwatch clock = Stopwatch.StartNew();
        int sampled = 0;
        bool closed = false;
        try
        {
            await player.PlayAsync(input, timestamp =>
            {
                Assert.IsFalse(closed);
                frames.Add(new { timestamp, wallTicks = clock.Elapsed.Ticks });
                if (timestamp == 5000000 || timestamp == 25000000)
                {
                    // Platform points are fixed before playback, never selected by color.
                    samples.Add(RenderPlayerTarget((GeneratedVisualResource)visual!, timestamp, clock.Elapsed.Ticks));
                    sampled++;
                }
            }).WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual((40, true, 39000000L, 2), (player.PublishedFrames, player.Ended, player.LastTimestamp, sampled));
            Assert.IsTrue(clock.Elapsed >= TimeSpan.FromSeconds(3.8));
        }
        finally
        {
            await player.CloseAsync().WaitAsync(TimeSpan.FromSeconds(10));
            closed = true;
            _ = table.Delete(3, MilResourceType.Visual);
            _ = table.Delete(2, MilResourceType.RenderData);
            _ = table.Delete(1, MilResourceType.MediaPlayer);
            File.WriteAllText(Path.Combine(root, "result.json"), JsonSerializer.Serialize(new
            {
                frames, samples, player.PublishedFrames, player.Ended, player.LastTimestamp,
                elapsed = clock.Elapsed.TotalSeconds, resource.IsReleased,
                runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                testMvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId,
                backend = "GeneratedVisualRenderer.Render/Capture DrawVideo -> SoftwareImageRenderSession -> Direct3D9SoftwareImageRenderer -> BitmapRenderTargetExports software RTB; managed not NativeAOT"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.IsTrue(resource.IsReleased);
        Assert.AreEqual(unchecked((int)0x80000013), player.Frames.Acquire(out _));
        await player.CloseAsync();
    }

    private static unsafe object RenderPlayerTarget(GeneratedVisualResource visual, long timestamp, long wallTicks)
    {
        nint target = 0, bitmap = 0;
        try
        {
            Assert.AreEqual(0, WpfGfxShape.Abi.BitmapRenderTargetExports.CreateDrawingIntermediate(2, 1, out target, out bitmap, false));
            Assert.AreEqual(0, GeneratedVisualRenderer.Render(visual, target, 2, 1));
            byte[] pixels = new byte[8];
            fixed (byte* destination = pixels)
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 8, 8, destination));
            string expected = timestamp == 5000000 ? "0000FFFF0000FFFF" : "FF0000FFFF0000FF";
            Assert.AreEqual(expected, Convert.ToHexString(pixels));
            return new { timestamp, wallTicks, width = 2, height = 1, stride = 8, pixels = Convert.ToHexString(pixels) };
        }
        finally
        {
            Direct3D9Factory.Release(bitmap);
            Direct3D9Factory.Release(target);
        }
    }

    [TestMethod]
    public async Task WhenDedicatedOwnerPlaysThenFortyPtsFramesReachEosAndCloseStopsAcquisitions()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-player-stage4");
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        IndependentVideoPlaybackOwner player = new();
        GeneratedMediaPlayerResource resource = new();
        resource.BindIndependentFrames(player.Frames);
        List<object> frames = [];
        Stopwatch clock = Stopwatch.StartNew();
        try
        {
            await player.PlayAsync(input, timestamp =>
            {
                Assert.AreEqual(0, resource.AcquireSystemFrame(out SystemVideoSample? lease));
                using (lease)
                {
                    Assert.IsNotNull(lease);
                    Assert.AreEqual(timestamp, lease.Timestamp);
                    frames.Add(new { timestamp, wallTicks = clock.Elapsed.Ticks });
                }
            }).WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(40, player.PublishedFrames);
            Assert.IsTrue(player.Ended);
            Assert.AreEqual(39000000L, player.LastTimestamp);
            Assert.IsTrue(clock.Elapsed >= TimeSpan.FromSeconds(3.8));
        }
        finally
        {
            await player.CloseAsync().WaitAsync(TimeSpan.FromSeconds(10));
            resource.Release();
            File.WriteAllText(Path.Combine(root, "result.json"), JsonSerializer.Serialize(new
            {
                frames, player.PublishedFrames, player.Ended, player.LastTimestamp,
                elapsed = clock.Elapsed.TotalSeconds,
                boundary = "managed production owner and resource Acquire; NOT COM identity or DrawVideo/RTB"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.AreEqual(unchecked((int)0x80000013), player.Frames.Acquire(out _));
        await player.CloseAsync();
    }
}
