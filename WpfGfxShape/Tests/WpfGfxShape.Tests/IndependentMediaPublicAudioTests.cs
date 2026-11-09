using System.Reflection;
using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    /// <summary>Exercises the public factory with real PCM and preserves actual device readiness failures.</summary>
    [TestMethod]
    public async Task WhenPublicPcmOpensThenRealReadyControlsNotificationAndVideoCanRecover()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string wave = Path.Combine(root, "content.bin");
        using (var writer = new BinaryWriter(File.Create(wave)))
        {
            writer.Write("RIFF"u8); writer.Write(64036); writer.Write("WAVEfmt "u8);
            writer.Write(16); writer.Write((ushort)1); writer.Write((ushort)1);
            writer.Write(8000); writer.Write(16000); writer.Write((ushort)2); writer.Write((ushort)16);
            writer.Write("data"u8); writer.Write(64000); writer.Write(new byte[64000]);
        }
        string video = Path.Combine(root, "video.avi");
        WriteSourceReaderAvi(video, includeAudio: false);
        // Reflection is limited to the managed ABI entry point and private owner observation;
        // no replacement implementations, production injection or NativeAOT claim.
        IndependentMediaIdentity registration = IndependentMediaIdentity.Create();
        ControlEvents events = new(registration);
        nint proxy = CreateControlProxy(events);
        nint media = CreatePublicAudioMedia(proxy);
        ReleaseControlProxy(proxy);
        IdentityRef(registration.Pointer, 2);
        IndependentMediaIdentity identity = IndependentMediaIdentity.Resolve(media)!;
        IndependentAudioPlaybackOwner? audio = null;
        int ready = int.MinValue;
        uint[] audioEvents = [];
        try
        {
            object gate = typeof(IndependentMediaIdentity).GetField("_gate", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(identity)!;
            lock (gate)
            {
                Assert.AreEqual(0, IdentityOpen(media, wave));
                Assert.AreEqual(0, IdentitySeek(media, 0));
                Assert.AreEqual(0, IdentitySeek(media, 0));
            }
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            audio = (IndependentAudioPlaybackOwner?)typeof(IndependentMediaIdentity)
                .GetField("_audioOwner", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(identity);
            Assert.IsNotNull(audio);
            ready = await audio.Ready;
            Assert.AreEqual(0L, QueryContinuousSeekPosition(identity));
            await Task.Delay(50);
            Assert.AreEqual(0L, QueryContinuousSeekPosition(identity));
            Assert.AreEqual(audio.Position100ns, QueryContinuousSeekPosition(identity));
            Assert.AreEqual(true, audio.InitiallyPaused);
            Assert.AreEqual(0UL, audio.SubmittedFrames);
            await (ready < 0 ? (Task)events.Failed.Task : events.Opened.Task).WaitAsync(TimeSpan.FromSeconds(5));
            audioEvents = events.Kinds.ToArray();
            CollectionAssert.AreEqual(new uint[] { ready < 0 ? 8u : 1u }, audioEvents);
            Assert.AreEqual(ready < 0 ? ready : 0, identity.Failure);
            Assert.AreEqual(ready < 0 ? IndependentMediaState.Failed : IndependentMediaState.Opened, identity.State);
            Assert.AreEqual((0, (byte)1, 0, (byte)0, 0u, 0u, 40000000L), PublicAudioMetadata(media));
            // Failed initialization retains its native HRESULT. A ready mono stream instead
            // admits Balance asynchronously; E_NOTIMPL is its execution result, not admission.
            object audioGate = typeof(IndependentAudioPlaybackOwner)
                .GetField("_gate", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(audio)!;
            lock (audioGate)
            {
                Assert.AreEqual(ready, PublicAudioBalance(media));
            }
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            int controlFailure = ready < 0 ? ready : unchecked((int)0x80004001);
            Assert.AreEqual(controlFailure, await events.Failed.Task.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(IndependentMediaState.Failed, identity.State);
            Assert.AreEqual(controlFailure, identity.Failure);
            Assert.AreEqual(controlFailure, PublicAudioBalance(media));
            Assert.AreEqual(0, IdentitySeek(media, 0));
            Assert.AreEqual(0, IdentitySeek(media, 0));
            Assert.AreEqual(controlFailure, IdentitySeek(media, 1));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.AreEqual(controlFailure, SubmitGenerationStop(media));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.AreEqual(unchecked((int)0x80004001), IdentityRate(media, 2));
            Assert.AreEqual(0, IdentityCall(media, 5));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.IsTrue(audio.Completion.IsCompleted);
            Assert.AreEqual(0L, audio.Position100ns);
            Assert.AreEqual(0L, QueryContinuousSeekPosition(identity));
            Assert.AreEqual(0, IdentityOpen(media, video));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.AreEqual(IndependentMediaState.Opened, identity.State);
            Assert.IsNotNull(identity.Owner);
            Assert.AreEqual(0, IdentityOpen(media, null!));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.AreEqual(IndependentMediaState.Closed, identity.State);
            Assert.AreEqual(0L, QueryContinuousSeekPosition(identity));
        }
        finally
        {
            IdentityCall(media, 5);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            IdentityRef(media, 2);
            string results = Path.Combine(AppContext.BaseDirectory, "TestResults");
            Directory.CreateDirectory(results);
            File.WriteAllText(Path.Combine(results, "public-audio.json"), JsonSerializer.Serialize(new
            {
                architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
                productAssembly = typeof(IndependentMediaIdentity).Assembly.Location,
                productModuleVersionId = typeof(IndependentMediaIdentity).Assembly.ManifestModule.ModuleVersionId,
                productSha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(typeof(IndependentMediaIdentity).Assembly.Location))),
                ready, audioEvents, initiallyPaused = audio?.InitiallyPaused,
                submittedFrames = audio?.SubmittedFrames, completion = audio?.Completion.IsCompleted,
                registryRemoved = IndependentMediaIdentity.Resolve(media) is null,
                evidence = audio?.Evidence, input = root,
                scope = "Managed public ABI PCM initialization and recovery; no Native or audible-output certification"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.IsTrue(events.Disposed);
    }

    /// <summary>Checks origin retirement through the real ABI; reflected state assembly is not successful device output.</summary>
    [DataTestMethod]
    [DataRow("play")]
    [DataRow("source")]
    [DataRow("null")]
    public async Task WhenPublicPcmOriginRetiresThenOnlyNewSourceCanInitializeAtZero(string action)
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string wave = Path.Combine(root, "origin.wav");
        using (var writer = new BinaryWriter(File.Create(wave)))
        {
            writer.Write("RIFF"u8); writer.Write(64036); writer.Write("WAVEfmt "u8);
            writer.Write(16); writer.Write((ushort)1); writer.Write((ushort)1);
            writer.Write(8000); writer.Write(16000); writer.Write((ushort)2); writer.Write((ushort)16);
            writer.Write("data"u8); writer.Write(64000); writer.Write(new byte[64000]);
        }
        string replacement = Path.Combine(root, "replacement.wav");
        File.Copy(wave, replacement);
        IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
        identity.RequireVideoOnly = true;
        Type type = typeof(IndependentMediaIdentity);
        object gate = type.GetField("_gate", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(identity)!;
        FieldInfo origin = type.GetField("_audioAtOrigin", BindingFlags.Instance | BindingFlags.NonPublic)!;
        try
        {
            lock (gate)
            {
                Assert.AreEqual(0, IdentityOpen(identity.Pointer, wave));
                Assert.AreEqual(0, IdentitySeek(identity.Pointer, 0));
            }
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.AreEqual(true, origin.GetValue(identity));
            FieldInfo ownerField = type.GetField("_audioOwner", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var openedOwner = (IndependentAudioPlaybackOwner)ownerField.GetValue(identity)!;
            await openedOwner.CloseAsync(TimeSpan.FromSeconds(20));
            IndependentAudioPlaybackOwner failedOwner = new(Path.Combine(root, "missing", "origin.wav"));
            await failedOwner.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            const int pathNotFound = unchecked((int)0x80070003);
            Assert.AreEqual(pathNotFound, await failedOwner.Ready);
            // Assemble transport routing only, never successful output. A completed real
            // failure makes command admission/execution deterministic without a device.
            lock (gate)
            {
                ownerField.SetValue(identity, failedOwner);
                type.GetProperty("State", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(identity, IndependentMediaState.Opened);
                Assert.AreEqual(0, IdentityRate(identity.Pointer, 1));
                Assert.AreEqual(false, origin.GetValue(identity));
            }
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.AreEqual(IndependentMediaState.Failed, identity.State);
            Assert.AreEqual(pathNotFound, identity.Failure);
            Assert.AreEqual(pathNotFound, IdentitySeek(identity.Pointer, 0));
            Assert.AreEqual(pathNotFound, SubmitGenerationStop(identity.Pointer));
            Assert.AreSame(failedOwner, ownerField.GetValue(identity));
            Assert.AreEqual(false, origin.GetValue(identity));
            if (action != "play")
            {
                lock (gate)
                {
                    Assert.AreEqual(0, IdentityOpen(identity.Pointer, action == "null" ? null! : replacement));
                    Assert.AreEqual(false, origin.GetValue(identity));
                    Assert.AreEqual(0, IdentitySeek(identity.Pointer, 0));
                }
                await identity.Completion.WaitAsync(TimeSpan.FromSeconds(20));
                Assert.AreEqual(action == "source", origin.GetValue(identity));
                Assert.AreNotSame(failedOwner, ownerField.GetValue(identity));
                Assert.AreEqual(pathNotFound, await failedOwner.SeekAsync(0));
                Assert.AreEqual(action == "source", origin.GetValue(identity));
                Assert.AreEqual(0, IdentitySeek(identity.Pointer, 0));
                Assert.AreEqual(0L, QueryContinuousSeekPosition(identity));
            }
        }
        finally
        {
            IdentityCall(identity.Pointer, 5);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.AreEqual(false, origin.GetValue(identity));
            IdentityRef(identity.Pointer, 2);
        }
    }

    private static unsafe nint CreatePublicAudioMedia(nint proxy)
    {
        var method = typeof(WpfGfxShape.Abi.MediaExports).GetMethod("CreateMediaPlayer", BindingFlags.Static | BindingFlags.NonPublic)!;
        var create = (delegate* unmanaged[Stdcall]<nint, nint, byte, nint*, int>)method.MethodHandle.GetFunctionPointer();
        nint media = 0;
        Assert.AreEqual(0, create(1, proxy, 1, &media));
        return media;
    }

    private static unsafe int PublicAudioBalance(nint media)
        => ((delegate* unmanaged[Stdcall]<nint, double, int>)(*(nint**)media)[10])(media, 0);

    private static unsafe (int, byte, int, byte, uint, uint, long) PublicAudioMetadata(nint media)
    {
        nint* table = *(nint**)media;
        byte audio = 0, video = 1;
        uint width = 1, height = 1;
        long duration = 0;
        int audioHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[17])(media, &audio);
        int videoHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[16])(media, &video);
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[19])(media, &width));
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[18])(media, &height));
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, long*, int>)table[20])(media, &duration));
        return (audioHr, audio, videoHr, video, width, height, duration);
    }
}
