using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class MediaExportsTests
{
    [TestMethod]
    [DataRow(0xffff0000u)]
    [DataRow(0xff00ff00u)]
    [DataRow(0xff0000ffu)]
    public void WhenRealSolidVideoIsOpenedThenPublicVisualConsumesDecodedPixels(uint expected)
        => VerifySolidVideo(expected);

    [TestMethod]
    public void WhenRealPlayersAreRepeatedlyCreatedAndClosedThenAllColorsRemainCorrect()
    {
        for (int iteration = 0; iteration < 5; iteration++)
        {
            VerifySolidVideo(0xffff0000u);
            VerifySolidVideo(0xff00ff00u);
            VerifySolidVideo(0xff0000ffu);
        }
    }

    [TestMethod]
    public void WhenRealPlayersRunInParallelThenAllColorsRemainCorrect()
    {
        Parallel.For(0, 3, index =>
        {
            VerifySolidVideo(0xffff0000u);
            VerifySolidVideo(0xff00ff00u);
            VerifySolidVideo(0xff0000ffu);
        });
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenRealSourceFactoriesAreCreatedConcurrentlyThenEveryCreationSucceeds(bool candidate)
    {
        for (int iteration = 0; iteration < 20; iteration++)
        {
            Parallel.For(0, 3, _ =>
            {
                using var source = new SystemMedia(candidate);
                Assert.AreNotEqual((nint)0, source.Media);
            });
        }
    }

    [TestMethod]
    public void WhenRealPlayerIsReboundAndSourceIsReplacedThenNotificationsDriveAllColors()
        => VerifySolidVideo(0xffff0000u, replace: true);

    [TestMethod]
    public void WhenRealVideoTargetIsLockedThenPresentFailsAndReleasedTargetRetriesOriginalPixels()
        => VerifySolidVideo(0xffff0000u, lockTarget: true);

    [TestMethod]
    public void WhenCallerReleasesRealPlayerThenResourceStillConsumesOriginalPixels()
        => VerifySolidVideo(0xffff0000u, releaseCaller: true);

    private static void VerifySolidVideo(uint expected, bool replace = false, bool lockTarget = false, bool releaseCaller = false)
    {
        string path = Path.Combine(Path.GetTempPath(), $"wpf-solid-{Guid.NewGuid():N}.avi");
        WriteSolidVideo(path, expected);
        using var source = new SystemMedia(candidate: true);
        nint module = LoadCandidate();
        var open = (delegate* unmanaged[Stdcall]<nint, char*, int>)NativeLibrary.GetExport(module, "MILMediaOpen");
        var rate = (delegate* unmanaged[Stdcall]<nint, double, int>)NativeLibrary.GetExport(module, "MILMediaSetRate");
        fixed (char* name = path) Assert.AreEqual(0, open(source.Media, name));
        Assert.AreEqual(0, rate(source.Media, 1));
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var create = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
        var factoryCreate = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
        var targetCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateSWRenderTargetForBitmap");
        nint connection = 0, channel = 0, factory = 0, target = 0, output = 0;
        using var wic = new SystemWicBitmap();
        try
        {
            Assert.AreEqual(0, connect(1, &connection));
            Assert.AreEqual(0, channelCreate(connection, 0, &channel));
            Assert.AreEqual(0, factoryCreate(&factory, 0x200184C0));
            output = wic.Create(2, 1);
            Assert.AreEqual(0, targetCreate(factory, output, &target));
            uint player = 0, data = 0, visual = 0, destination = 0;
            Assert.AreEqual(0, create(channel, 1, &player));
            Assert.AreEqual(0, create(channel, 43, &data));
            Assert.AreEqual(0, create(channel, 39, &visual));
            Assert.AreEqual(0, create(channel, 47, &destination));
            byte* packet = stackalloc byte[60];
            new Span<byte>(packet, 60).Clear();
            *(uint*)packet = 0x17; *(uint*)(packet + 4) = player;
            *(ulong*)(packet + 8) = (ulong)source.Media; *(int*)(packet + 16) = 1;
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source.Media)[1])(source.Media);
            Assert.AreEqual(0, send(packet, 20, 0, channel));
            new Span<byte>(packet, 60).Clear();
            *(uint*)packet = 0x34; *(uint*)(packet + 4) = destination;
            *(ulong*)(packet + 16) = (ulong)target; *(uint*)(packet + 24) = 2; *(uint*)(packet + 28) = 1;
            Assert.AreEqual(0, send(packet, 36, 0, channel));
            new Span<byte>(packet, 60).Clear();
            *(uint*)packet = 0x18; *(uint*)(packet + 4) = data; *(uint*)(packet + 8) = 48;
            *(uint*)(packet + 12) = 48; *(uint*)(packet + 16) = 0x4b;
            *(double*)(packet + 36) = 2; *(double*)(packet + 44) = 1; *(uint*)(packet + 52) = player;
            Assert.AreEqual(0, send(packet, 60, 0, channel));
            uint* binding = stackalloc uint[] { 0x22, visual, data };
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            binding[0] = 0x35; binding[1] = destination; binding[2] = visual;
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            Assert.AreEqual(0, close(channel));
            Assert.AreEqual(0, commit(channel));
            if (releaseCaller) Assert.IsGreaterThan(0u, source.ReleaseCallerReference());
            Stopwatch deadline = Stopwatch.StartNew();
            uint[] pixels = new uint[2];
            uint[] originalPixels = ReadOriginalPixels(expected);
            foreach (uint color in replace ? new uint[] { expected, 0xff00ff00u, 0xff0000ffu } : new uint[] { expected })
            {
                if (replace)
                {
                    new Span<byte>(packet, 60).Clear();
                    *(uint*)packet = 0x17; *(uint*)(packet + 4) = player;
                    *(ulong*)(packet + 8) = (ulong)source.Media; *(int*)(packet + 16) = 1;
                    ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source.Media)[1])(source.Media);
                    Assert.AreEqual(0, send(packet, 20, 0, channel));
                    Assert.AreEqual(0, close(channel));
                    Assert.AreEqual(0, commit(channel));
                    string replacement = Path.Combine(Path.GetTempPath(), $"wpf-replacement-{Guid.NewGuid():N}.avi");
                    WriteSolidVideo(replacement, color);
                    fixed (char* name = replacement) Assert.AreEqual(0, open(source.Media, name));
                    Assert.AreEqual(0, rate(source.Media, 1));
                }
                uint[] decoded = replace ? ReadOriginalPixels(color) : originalPixels;
                deadline.Restart();
            do
            {
                // A synchronous software channel pulls samples; media lifecycle events are not frame readiness.
                source.WaitForNotification(TimeSpan.FromMilliseconds(20));
                Assert.IsNull(source.CallbackError);
                Assert.AreEqual(0, present(connection));
                var copy = (delegate* unmanaged[Stdcall]<nint, void*, uint, uint, byte*, int>)(*(void***)output)[7];
                fixed (uint* buffer = pixels) Assert.AreEqual(0, copy(output, null, 8, 8, (byte*)buffer));
                if (pixels.AsSpan().SequenceEqual(decoded)) break;
            } while (deadline.Elapsed < TimeSpan.FromSeconds(15));
            CollectionAssert.AreEqual(decoded, pixels, $"Media: {path}");
            }
            if (lockTarget)
            {
                nint heldLock = 0;
                try
                {
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)output)[8])(output, 0, 2, &heldLock));
                    Assert.AreEqual(unchecked((int)0x88982F0D), present(connection));
                }
                finally
                {
                    Release(heldLock);
                }
                Assert.AreEqual(0, present(connection));
                var copy = (delegate* unmanaged[Stdcall]<nint, void*, uint, uint, byte*, int>)(*(void***)output)[7];
                fixed (uint* buffer = pixels) Assert.AreEqual(0, copy(output, null, 8, 8, (byte*)buffer));
                CollectionAssert.AreEqual(originalPixels, pixels);
            }
            if (releaseCaller) return;
            byte hasVideo = 0, hasAudio = 1;
            uint naturalWidth = 0, naturalHeight = 0;
            deadline.Restart();
            do
            {
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, byte*, int>)NativeLibrary.GetExport(module, "MILMediaHasVideo"))(source.Media, &hasVideo));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, byte*, int>)NativeLibrary.GetExport(module, "MILMediaHasAudio"))(source.Media, &hasAudio));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)NativeLibrary.GetExport(module, "MILMediaGetNaturalWidth"))(source.Media, &naturalWidth));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)NativeLibrary.GetExport(module, "MILMediaGetNaturalHeight"))(source.Media, &naturalHeight));
                if (hasVideo == 1 && hasAudio == 0 && naturalWidth == 2 && naturalHeight == 1) break;
                Thread.Sleep(20);
            } while (deadline.Elapsed < TimeSpan.FromSeconds(15));
            Assert.AreEqual(((byte)1, (byte)0, 2u, 1u), (hasVideo, hasAudio, naturalWidth, naturalHeight));
        }
        finally
        {
            if (source.Media != 0) RecordMediaState(source, path, true, "public visual frame; see TRX assertion output");
            if (channel != 0) _ = ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel"))(channel);
            if (connection != 0) _ = ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect"))(connection);
            Release(target); Release(output); Release(factory);
        }
    }

    private static uint[] ReadOriginalPixels(uint color)
    {
        string path = Path.Combine(Path.GetTempPath(), $"wpf-original-{Guid.NewGuid():N}.avi");
        WriteSolidVideo(path, color);
        using var source = new SystemMedia(candidate: false);
        void** table = *(void***)source.Media;
        fixed (char* name = path)
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(source.Media, name));
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, double, int>)table[8])(source.Media, 1));
        Stopwatch deadline = Stopwatch.StartNew();
        string sample;
        do
        {
            source.WaitForNotification(TimeSpan.FromMilliseconds(20));
            sample = SampleDiagnosticPixels(source.Media);
            const string marker = "; pixels=";
            int start = sample.IndexOf(marker, StringComparison.Ordinal);
            if (sample.StartsWith("composition=00000000; ready=1; render=00000000; copy=00000000;", StringComparison.Ordinal) && start >= 0)
            {
                byte[] bytes = Convert.FromHexString(sample[(start + marker.Length)..]);
                uint[] pixels = MemoryMarshal.Cast<byte, uint>(bytes).ToArray();
                if (pixels.Length == 2 && pixels[0] != 0 && pixels[0] == pixels[1])
                {
                    RecordMediaState(source, path, false, sample);
                    return pixels;
                }
            }
        } while (deadline.Elapsed < TimeSpan.FromSeconds(15));
        Assert.Fail($"Original renderer did not provide a decoded frame: {sample}");
        return [];
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenRealPlayerClosesThenNativeDimensionsAndLengthAreCleared(bool candidate)
    {
        string path = Path.Combine(Path.GetTempPath(), $"wpf-close-{Guid.NewGuid():N}.avi");
        WriteSolidVideo(path);
        using var source = new SystemMedia(candidate);
        void** table = *(void***)source.Media;
        fixed (char* name = path)
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(source.Media, name));
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, double, int>)table[8])(source.Media, 1));
        Stopwatch deadline = Stopwatch.StartNew();
        uint width = 0;
        do
        {
            Assert.IsTrue(source.WaitForNotification(TimeSpan.FromSeconds(15) - deadline.Elapsed));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[19])(source.Media, &width));
        } while (width != 2 && deadline.Elapsed < TimeSpan.FromSeconds(15));
        Assert.AreEqual(2u, width);
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(source.Media));
        uint height = 1;
        long length = -1;
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[19])(source.Media, &width));
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[18])(source.Media, &height));
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, long*, int>)table[20])(source.Media, &length));
        Assert.AreEqual((0u, 0u, 0L), (width, height, length));
    }

    private static void WriteSolidVideo(string path, uint color = 0xffff0000)
    {
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream, Encoding.ASCII);
        long Begin(string id, string? type = null)
        {
            writer.Write(Encoding.ASCII.GetBytes(id)); writer.Write(0);
            long start = stream.Position;
            if (type is not null) writer.Write(Encoding.ASCII.GetBytes(type));
            return start;
        }
        void End(long start)
        {
            long end = stream.Position;
            stream.Position = start - 4; writer.Write(checked((int)(end - start))); stream.Position = end;
        }
        long riff = Begin("RIFF", "AVI "), header = Begin("LIST", "hdrl"), main = Begin("avih");
        writer.Write(100000); writer.Write(80); writer.Write(0); writer.Write(0);
        writer.Write(100); writer.Write(0); writer.Write(1); writer.Write(8); writer.Write(2); writer.Write(1);
        writer.Write(new byte[16]); End(main);
        long list = Begin("LIST", "strl"), info = Begin("strh");
        writer.Write(Encoding.ASCII.GetBytes("vidsDIB ")); writer.Write(0); writer.Write(0);
        writer.Write(0); writer.Write(1); writer.Write(10); writer.Write(0); writer.Write(100);
        writer.Write(8); writer.Write(-1); writer.Write(0);
        writer.Write((short)0); writer.Write((short)0); writer.Write((short)2); writer.Write((short)1); End(info);
        long format = Begin("strf");
        writer.Write(40); writer.Write(2); writer.Write(1); writer.Write((short)1); writer.Write((short)24);
        writer.Write(0); writer.Write(8); writer.Write(new byte[16]); End(format); End(list); End(header);
        long frames = Begin("LIST", "movi");
        for (int frame = 0; frame < 100; frame++)
        {
            long chunk = Begin("00db");
            writer.Write(new byte[] { (byte)color, (byte)(color >> 8), (byte)(color >> 16), (byte)color, (byte)(color >> 8), (byte)(color >> 16), 0, 0 });
            End(chunk);
        }
        End(frames); End(riff);
    }
}
