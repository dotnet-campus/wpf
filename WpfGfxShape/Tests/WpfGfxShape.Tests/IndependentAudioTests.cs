using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed partial class IndependentAudioTests
{
    private static byte[] ExtractFixedSourceAudio(byte[] source, out List<object> ranges)
    {
        List<object> chunks = [];
        List<byte> audio = [];
        int streams = 0, audioStreams = 0, videoChunks = 0, audioChunks = 0;
        bool mainSeen = false, movieSeen = false;
        uint U32(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(source.AsSpan(offset, 4));
        ushort U16(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(offset, 2));
        string Id(int offset) => System.Text.Encoding.ASCII.GetString(source, offset, 4);
        void Require(bool condition)
        {
            if (!condition) throw new InvalidDataException("Invalid fixed AVI oracle structure.");
        }
        void Walk(int start, int end, string parent)
        {
            for (int offset = start; offset < end;)
            {
                Require(end - offset >= 8);
                uint size = U32(offset + 4);
                Require(size <= (uint)(end - offset - 8));
                int data = offset + 8, limit = data + (int)size;
                int next = checked(limit + (int)(size & 1));
                Require(next <= end);
                string id = Id(offset);
                if (id == "LIST")
                {
                    Require(size >= 4);
                    string kind = Id(data);
                    if (kind == "strl")
                    {
                        Require(parent == "hdrl");
                        int streamIndex = streams++;
                        int header = -1, format = -1, formatSize = 0;
                        for (int child = data + 4; child < limit;)
                        {
                            Require(limit - child >= 8);
                            uint childSize = U32(child + 4);
                            Require(childSize <= (uint)(limit - child - 8));
                            if (Id(child) == "strh") { Require(header < 0 && childSize == 56); header = child + 8; }
                            if (Id(child) == "strf") { Require(format < 0); format = child + 8; formatSize = (int)childSize; }
                            child = checked(child + 8 + (int)childSize + (int)(childSize & 1));
                            Require(child <= limit);
                        }
                        Require(header >= 0 && format >= 0);
                        if (Id(header) == "auds")
                        {
                            audioStreams++;
                            Require(streamIndex == 1 && formatSize == 16);
                            Require(U32(header + 16) == 0 && U32(header + 20) == 2 && U32(header + 24) == 16000);
                            Require(U32(header + 28) == 0 && U32(header + 32) == 32000 && U32(header + 44) == 2);
                            Require(U16(format) == 1 && U16(format + 2) == 1 && U32(format + 4) == 8000);
                            Require(U32(format + 8) == 16000 && U16(format + 12) == 2 && U16(format + 14) == 16);
                        }
                        else Require(streamIndex == 0 && Id(header) == "vids" && U32(header + 32) == 40);
                    }
                    else
                    {
                        Require((kind == "hdrl" || kind == "movi") && parent == "AVI ");
                        if (kind == "movi") { Require(!movieSeen); movieSeen = true; }
                        Walk(data + 4, limit, kind);
                    }
                }
                else if (parent == "hdrl" && id == "avih")
                {
                    Require(!mainSeen && size == 56); mainSeen = true;
                    Require(U32(data) == 100000 && U32(data + 16) == 40 && U32(data + 24) == 2);
                }
                else if (parent == "movi" && id == "01wb")
                {
                    Require(size == 1600); audioChunks++;
                    chunks.Add(new { fileStart = data, fileEnd = limit, pcmStart = audio.Count, pcmEnd = audio.Count + (int)size });
                    audio.AddRange(source.AsSpan(data, (int)size).ToArray());
                }
                else if (parent == "movi" && id == "00db") { Require(size == 8); videoChunks++; }
                else Require(false);
                offset = next;
            }
        }
        Require(source.Length >= 12 && Id(0) == "RIFF" && Id(8) == "AVI ");
        Require(U32(4) == source.Length - 8);
        Walk(12, source.Length, "AVI ");
        Require(mainSeen && movieSeen && streams == 2 && audioStreams == 1 && videoChunks == 40 && audioChunks == 40 && audio.Count == 64000);
        byte[] pcm = audio.ToArray();
        for (int i = 0; i < 32000; i++)
            Require(BinaryPrimitives.ReadInt16LittleEndian(pcm.AsSpan(i * 2)) == (short)(1000 * Math.Sin(2 * Math.PI * 440 * i / 8000)));
        ranges = chunks;
        return pcm;
    }

    [TestMethod]
    public void WhenPcmFormatIsMalformedThenItIsRejected()
        => Assert.IsFalse(new AudioPcmFormat(2, 8000, 16, 2, false).IsSupported);

    [TestMethod]
    [DataRow(1u, 8000u, 16u, 2u, false, true)]
    [DataRow(2u, 48000u, 32u, 8u, true, true)]
    [DataRow(1u, 8000u, 24u, 3u, false, false)]
    [DataRow(1u, 8000u, 16u, 2u, true, false)]
    [DataRow(0u, 8000u, 16u, 0u, false, false)]
    [DataRow(1u, 7999u, 16u, 2u, false, false)]
    [DataRow(1u, 192001u, 16u, 2u, false, false)]
    public void WhenNativeFormatIsCheckedThenCompleteOutputContractIsRequired(uint channels, uint rate, uint bits, uint alignment, bool floatingPoint, bool supported)
        => Assert.AreEqual(supported, new AudioPcmFormat(channels, rate, bits, alignment, floatingPoint).IsSupported);

    [TestMethod]
    public void WhenBoundedBufferIsFullThenWriteDoesNotOverwrite()
    {
        var buffer = new BoundedPcmBuffer(2, new(1, 8000, 16, 2, false));
        Assert.IsTrue(buffer.TryWrite([1, 2, 3, 4]));
        Assert.IsFalse(buffer.TryWrite([5, 6]));
        byte[] output = new byte[4];
        buffer.Read(output);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, output);
    }
    [TestMethod]
    public void WhenBufferReceivesPartialFrameThenItRejectsIt()
    {
        var buffer = new BoundedPcmBuffer(2, new(2, 8000, 16, 4, false));
        Assert.ThrowsExactly<ArgumentException>(() => buffer.TryWrite(new byte[2]));
    }
    [TestMethod]
    public void WhenBufferWrapsThenFramesRemainOrdered()
    {
        var buffer = new BoundedPcmBuffer(2, new(1, 8000, 16, 2, false));
        buffer.TryWrite([1, 2, 3, 4]);
        buffer.Read(new byte[2]);
        buffer.TryWrite([5, 6]);
        byte[] output = new byte[4];
        buffer.Read(output);
        CollectionAssert.AreEqual(new byte[] { 3, 4, 5, 6 }, output);
    }

    [TestMethod]
    public void WhenRealAudioDecodesThenFixedOracleAndEosPass()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-audio-stage7-decode");
        Directory.CreateDirectory(root);
        string path = Path.Combine(root, "source.avi");
        MediaFoundationResourceTests.WriteAudioOracleAvi(path);
        MediaFoundationAudioReader? reader = null;
        List<object> samples = [];
        byte[] pcm = new byte[64000], block = new byte[65536];
        int used = 0;
        long? firstPts = null;
        List<(long Offset, long Pts, long Duration, int Length, uint Flags)> timeline = [];
        bool oraclePassed = false;
        try
        {
            Assert.AreEqual(0, MediaFoundationAudioReader.Create(path, out reader));
            Assert.IsNotNull(reader);
            Assert.AreEqual(new AudioPcmFormat(1, 8000, 16, 2, false), reader.Format);
            var queue = new BoundedPcmBuffer(32768, reader.Format);
            for (int i = 0; i < 200 && !reader.Ended; i++)
            {
                Assert.AreEqual(0, reader.Read(block, out int length, out long pts, out long duration, out uint flags));
                samples.Add(new { length, pts, duration, flags });
                if (length == 0) continue;
                firstPts ??= pts;
                timeline.Add((used / 2 * 1250L, pts, duration, length, flags));
                Assert.IsTrue(queue.TryWrite(block.AsSpan(0, length)));
                used += queue.Read(pcm.AsSpan(used));
            }
            Assert.IsTrue(reader.Ended);
            Assert.AreEqual(64000, used);
            byte[] expected = new byte[64000];
            for (int i = 0; i < 32000; i++) BinaryPrimitives.WriteInt16LittleEndian(expected.AsSpan(i * 2), (short)(1000 * Math.Sin(2 * Math.PI * 440 * i / 8000)));
            CollectionAssert.AreEqual(expected, pcm);
            Assert.IsNotNull(firstPts);
            // ReadSample preserves the source timeline; AVI dwStart=0 does not require a zero first sample.
            // The independent oracle fixes every relative frame time and duration, not the observed origin.
            foreach (var sample in timeline)
            {
                Assert.AreEqual(sample.Offset, sample.Pts - firstPts.Value);
                Assert.AreEqual(sample.Length / 2 * 1250L, sample.Duration);
                Assert.AreEqual(0u, sample.Flags & 0x100u);
            }
            // Fixed one-second source window: 439..441 rising zero crossings, not fitted to the result.
            int crossings = 0, peak = 0;
            for (int i = 1; i < 8000; i++)
            {
                short a = BinaryPrimitives.ReadInt16LittleEndian(pcm.AsSpan((i - 1) * 2));
                short b = BinaryPrimitives.ReadInt16LittleEndian(pcm.AsSpan(i * 2));
                if (a <= 0 && b > 0) crossings++;
                peak = Math.Max(peak, Math.Abs((int)b));
            }
            Assert.IsTrue(crossings is >= 439 and <= 441);
            Assert.IsTrue(peak is >= 999 and <= 1000);
            Assert.AreEqual(0, reader.Close());
            Assert.AreEqual(0, reader.Close());
            Assert.AreEqual(unchecked((int)0x80000013), reader.Read(block, out _, out _, out _, out _));
            File.WriteAllBytes(Path.Combine(root, "decoded.pcm"), pcm);
            oraclePassed = true;
            File.WriteAllText(Path.Combine(root, "oracle.json"), JsonSerializer.Serialize(new { firstPts, sourceStart = 0, sourceInitialFrames = 0, sourceScale = 2, sourceRate = 16000, expectedDuration100ns = 40000000L, crossings, peak, frames = used / 2, sourceSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), pcmSha = Convert.ToHexString(SHA256.HashData(pcm)), reader.Format, reader.Duration100ns, samples }, new JsonSerializerOptions { WriteIndented = true }));
        }
        finally
        {
            reader?.Dispose();
            byte[] source = File.ReadAllBytes(path);
            byte[] sourcePcm = ExtractFixedSourceAudio(source, out List<object> sourceRanges);
            int sourceByteStart = used == 0 ? -1 : sourcePcm.AsSpan().IndexOf(pcm.AsSpan(0, used));
            int crossingsActual = 0;
            for (int i = 1; i < Math.Min(8000, used / 2); i++)
                if (BinaryPrimitives.ReadInt16LittleEndian(pcm.AsSpan((i - 1) * 2)) <= 0 && BinaryPrimitives.ReadInt16LittleEndian(pcm.AsSpan(i * 2)) > 0) crossingsActual++;
            File.WriteAllText(Path.Combine(root, "source-audio.json"), JsonSerializer.Serialize(new { bytes = sourcePcm.Length, frames = sourcePcm.Length / 2, sha = Convert.ToHexString(SHA256.HashData(sourcePcm)), sourceByteStart, sourceByteEnd = sourceByteStart < 0 ? -1 : sourceByteStart + used, crossingsActual, sourceRanges, timeline = timeline.Select(s => new { s.Pts, end = s.Pts + s.Duration, s.Duration, s.Length, s.Flags }) }, new JsonSerializerOptions { WriteIndented = true }));
            File.WriteAllBytes(Path.Combine(root, "actual.pcm"), pcm.AsSpan(0, used).ToArray());
            File.WriteAllText(Path.Combine(root, "actual-content.json"), JsonSerializer.Serialize(new { used, frames = used / 2, expectedBytes = 64000, expectedFrames = 32000, sha = Convert.ToHexString(SHA256.HashData(pcm.AsSpan(0, used))), sourceSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) }, new JsonSerializerOptions { WriteIndented = true }));
            File.WriteAllText(Path.Combine(root, "evidence.json"), JsonSerializer.Serialize(new { oraclePassed, used, firstPts, samples, evidence = reader?.Evidence, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), sha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(IndependentAudioTests).Assembly.Location))), mvid = typeof(IndependentAudioTests).Assembly.ManifestModule.ModuleVersionId, modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => m.ModuleName.StartsWith("mf", StringComparison.OrdinalIgnoreCase) || m.ModuleName.Contains("audio", StringComparison.OrdinalIgnoreCase)).Select(m => new { m.ModuleName, m.FileName }).ToArray() }, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
