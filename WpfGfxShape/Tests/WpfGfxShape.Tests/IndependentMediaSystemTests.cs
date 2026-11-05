using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    [TestMethod]
    public async Task WhenComIdentityPlaysThenRealStateAndTransportReachDrawVideo()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-media-stage5");
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
        GeneratedProtocolHandleTable table = new();
        GeneratedProtocolChannelRegistry registry = new();
        Assert.IsTrue(registry.TryAdd(1, table));
        GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, registry).CreateRouter();
        List<object> frames = [], samples = [], calls = [];
        Stopwatch clock = Stopwatch.StartNew();
        bool released = false;
        IndependentVideoPlaybackOwner? owner = null;
        try
        {
            Assert.AreEqual(0, IdentityOpen(identity.Pointer, input));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(IndependentMediaState.Opened, identity.State);
            owner = identity.Owner;
            Assert.IsNotNull(owner);
            Assert.AreEqual(0, table.Create(1, MilResourceType.MediaPlayer));
            Assert.AreEqual(0, table.Create(2, MilResourceType.RenderData));
            Assert.AreEqual(0, table.Create(3, MilResourceType.Visual));
            Assert.AreEqual(2u, IdentityRef(identity.Pointer, 1));
            Assert.AreEqual(0, router.ProcessPackets([
                GeneratedProtocolPacketWriter.WriteMediaPlayer(1, unchecked((ulong)identity.Pointer), 0),
                GeneratedProtocolPacketWriter.WriteVisualCreate(3),
                GeneratedProtocolPacketWriter.WriteRenderData(2, GeneratedProtocolPacketWriter.WriteDrawVideoRecord(new(0, 0, 2, 1), 1)),
                GeneratedProtocolPacketWriter.WriteVisualSetContent(3, 2)]));
            Assert.IsTrue(table.TryGetResource(3, out var visual));
            owner.FrameReady += OnFrame;
            void OnFrame(long timestamp)
            {
                var query = IdentityQuery(identity.Pointer);
                Assert.AreEqual((0, timestamp, 0, (byte)1), query);
                frames.Add(new { timestamp, wallTicks = clock.Elapsed.Ticks, query });
                if (timestamp == 5000000 || timestamp == 25000000)
                    samples.Add(RenderPlayerTarget((GeneratedVisualResource)visual!, timestamp, clock.Elapsed.Ticks));
            }
            Assert.AreEqual(0, IdentityRate(identity.Pointer, 1));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            owner.FrameReady -= OnFrame;
            Assert.AreEqual((IndependentMediaState.Ended, 40, 39000000L, 2), (identity.State, owner.PublishedFrames, owner.LastTimestamp, samples.Count));
            calls.Add(new { close = IdentityCall(identity.Pointer, 5) });
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(IndependentMediaState.Closed, identity.State);
            Assert.AreEqual(0, IdentityCall(identity.Pointer, 5));
            Assert.AreEqual(unchecked((int)0x80000013), owner.Frames.Acquire(out _));
        }
        finally
        {
            IdentityCall(identity.Pointer, 5);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            _ = table.Delete(3, MilResourceType.Visual);
            _ = table.Delete(2, MilResourceType.RenderData);
            _ = table.Delete(1, MilResourceType.MediaPlayer);
            uint final = IdentityRef(identity.Pointer, 2);
            released = final == 0;
            File.WriteAllText(Path.Combine(root, "result.json"), JsonSerializer.Serialize(new
            {
                frames, samples, calls, finalReferenceCount = final, registryRemoved = IndependentMediaIdentity.Resolve(identity.Pointer) is null,
                state = identity.State.ToString(), identity.Failure, published = owner?.PublishedFrames, ended = owner?.Ended,
                runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
                testMvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId,
                testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(MediaFoundationResourceTests).Assembly.Location))),
                modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => m.ModuleName.StartsWith("mf", StringComparison.OrdinalIgnoreCase)).Select(m => new { m.ModuleName, m.FileName, version = m.FileVersionInfo.FileVersion }).ToArray(),
                cleanup = "CloseAsync completed; acquired-after-close HR asserted; resource deleted; final COM Release recorded; backend individual native ref counts not instrumented",
                events = "Not connected; factory E_NOTIMPL/null; no COM event assertion",
                clock = "PTS callback sampling, not independent wall clock; positions are last published PTS"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.IsTrue(released);
    }

    private static unsafe int IdentityOpen(nint self, string path)
    {
        fixed (char* text = path) return ((delegate* unmanaged[Stdcall]<nint, char*, int>)(*(nint**)self)[3])(self, text);
    }
    private static unsafe int IdentityCall(nint self, int slot) => ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)self)[slot])(self);
    private static unsafe int IdentityRate(nint self, double rate) => ((delegate* unmanaged[Stdcall]<nint, double, int>)(*(nint**)self)[8])(self, rate);
    private static unsafe uint IdentityRef(nint self, int slot) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)self)[slot])(self);
    private static unsafe (int positionHr, long position, int videoHr, byte video) IdentityQuery(nint self)
    {
        long position = -1;
        byte video = 0;
        int positionHr = ((delegate* unmanaged[Stdcall]<nint, long*, int>)(*(nint**)self)[6])(self, &position);
        int videoHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)(*(nint**)self)[16])(self, &video);
        return (positionHr, position, videoHr, video);
    }
}
