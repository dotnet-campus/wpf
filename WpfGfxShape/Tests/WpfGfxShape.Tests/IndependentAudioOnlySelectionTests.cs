using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class IndependentAudioTests
{
    [TestMethod]
    public void WhenAudioOnlyPcmIsRequiredThenWaveSamplesAreDeliveredExactly()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string avi = Path.Combine(root, "source.avi");
        string wav = Path.Combine(root, "source.wav");
        MediaFoundationResourceTests.WriteAudioOracleAvi(avi);
        byte[] expected = ExtractFixedSourceAudio(File.ReadAllBytes(avi), out _);
        File.WriteAllBytes(wav, MakeWave(expected));
        Console.WriteLine(root);
        Assert.AreEqual(0, MediaFoundationAudioReader.Create(wav, out var reader, requireAudioOnlyPcm: true));
        using (reader)
        {
            Assert.IsNotNull(reader);
            using MemoryStream decoded = new();
            byte[] block = new byte[65536];
            for (int i = 0; i < 100 && !reader.Ended; i++)
            {
                Assert.AreEqual(0, reader.Read(block, out int length, out _, out _, out _));
                decoded.Write(block, 0, length);
            }
            Assert.IsTrue(reader.Ended);
            CollectionAssert.AreEqual(expected, decoded.ToArray());
            Assert.AreEqual(40000000UL, reader.Duration100ns);
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenWaveSeeksThenRealPcmAndTimestampRestartAtTarget(bool afterEos)
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string avi = Path.Combine(root, "source.avi");
        string wav = Path.Combine(root, "source.wav");
        MediaFoundationResourceTests.WriteAudioOracleAvi(avi);
        byte[] expected = ExtractFixedSourceAudio(File.ReadAllBytes(avi), out _);
        File.WriteAllBytes(wav, MakeWave(expected));
        Console.WriteLine(root);
        Assert.AreEqual(0, MediaFoundationAudioReader.Create(wav, out var reader, requireAudioOnlyPcm: true));
        using (reader)
        {
            Assert.IsNotNull(reader);
            byte[] block = new byte[65536];
            for (int i = 0; afterEos && i < 100 && !reader.Ended; i++)
                Assert.AreEqual(0, reader.Read(block, out _, out _, out _, out _));
            Assert.AreEqual(afterEos, reader.Ended);
            Assert.AreEqual(0, reader.Seek(20000000));
            Assert.IsFalse(reader.Ended);
            Assert.AreEqual(0, reader.Read(block, out int length, out long pts, out _, out _));
            Console.WriteLine($"Seek first pts={pts};length={length}");
            Assert.AreEqual(20000000L, pts);
            int offset = checked((int)(pts * reader.Format.SampleRate / 10000000 * reader.Format.BlockAlign));
            CollectionAssert.AreEqual(expected.AsSpan(offset, length).ToArray(), block.AsSpan(0, length).ToArray());
        }
    }

    [TestMethod]
    public void WhenAudioOnlyPcmIsRequiredThenAudioVideoInputIsRejected()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string path = Path.Combine(root, "source.avi");
        MediaFoundationResourceTests.WriteAudioOracleAvi(path);
        Console.WriteLine(root);
        int result = MediaFoundationAudioReader.Create(path, out var reader, requireAudioOnlyPcm: true);
        using (reader)
        {
            Assert.AreEqual(unchecked((int)0xc00d36b4), result);
            Assert.IsNull(reader);
        }
    }
}
