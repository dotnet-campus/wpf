using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class IndependentAudioTests
{
    private const string FixedPcmHash = "018817036867AC8AD146FC532BABBE027CCDEB89F66E786B4BBE78DF5B746CFD";
    private sealed record RiffChunk(string Id, string Parent, int Header, int Data, int Size);
    private static List<RiffChunk> ParseRiff(byte[] bytes, string type)
    {
        static void Require(bool value) { if (!value) throw new InvalidDataException("Invalid RIFF boundary."); }
        string Id(int offset) => Encoding.ASCII.GetString(bytes, offset, 4);
        uint U32(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset, 4));
        Require(bytes.Length >= 12 && Id(0) == "RIFF" && Id(8) == type && U32(4) == bytes.Length - 8);
        List<RiffChunk> chunks = [];
        void Walk(int start, int end, string parent)
        {
            for (int offset = start; offset < end;)
            {
                Require(end - offset >= 8);
                uint size = U32(offset + 4);
                Require(size <= (uint)(end - offset - 8));
                int data = offset + 8, limit = checked(data + (int)size), next = checked(limit + (int)(size & 1));
                Require(next <= end);
                string id = Id(offset);
                chunks.Add(new(id, parent, offset, data, (int)size));
                if (id == "LIST") { Require(size >= 4); Walk(data + 4, limit, Id(data)); }
                offset = next;
            }
        }
        Walk(12, bytes.Length, type);
        return chunks;
    }
    private static byte[] FixedPcm()
    {
        byte[] pcm = new byte[64000];
        for (int i = 0; i < 32000; i++) BinaryPrimitives.WriteInt16LittleEndian(pcm.AsSpan(i * 2), (short)(1000 * Math.Sin(2 * Math.PI * 440 * i / 8000)));
        return pcm;
    }
    private static byte[] MakeWave(byte[] pcm)
    {
        if (pcm.Length != 64000) throw new InvalidDataException("Invalid fixed PCM length.");
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.ASCII, true);
        writer.Write("RIFF"u8); writer.Write(36 + pcm.Length); writer.Write("WAVEfmt "u8);
        writer.Write(16); writer.Write((ushort)1); writer.Write((ushort)1); writer.Write(8000);
        writer.Write(16000); writer.Write((ushort)2); writer.Write((ushort)16);
        writer.Write("data"u8); writer.Write(pcm.Length); writer.Write(pcm);
        writer.Flush();
        return stream.ToArray();
    }
    private static byte[] ValidateWave(byte[] wave)
    {
        var chunks = ParseRiff(wave, "WAVE");
        if (chunks.Count != 2 || chunks[0] != new RiffChunk("fmt ", "WAVE", 12, 20, 16) || chunks[1] != new RiffChunk("data", "WAVE", 36, 44, 64000)) throw new InvalidDataException("Invalid canonical WAVE chunks.");
        ushort U16(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(wave.AsSpan(offset, 2));
        uint U32(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(wave.AsSpan(offset, 4));
        if (U16(20) != 1 || U16(22) != 1 || U32(24) != 8000 || U32(28) != 16000 || U16(32) != 2 || U16(34) != 16) throw new InvalidDataException("Invalid PCM WAVE format.");
        byte[] pcm = wave.AsSpan(44).ToArray();
        if (!pcm.AsSpan().SequenceEqual(FixedPcm())) throw new InvalidDataException("Invalid independent source oracle.");
        return pcm;
    }
    private static object InspectAvi(byte[] source)
    {
        var chunks = ParseRiff(source, "AVI ");
        uint U32(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(source.AsSpan(offset, 4));
        ushort U16(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(offset, 2));
        string Id(int offset) => Encoding.ASCII.GetString(source, offset, 4);
        var main = chunks.Single(c => c.Id == "avih");
        var headers = chunks.Where(c => c.Id == "strh").Select(c => new { type = Id(c.Data), handler = Id(c.Data + 4), c.Size, flags = U32(c.Data + 8), priority = U16(c.Data + 12), language = U16(c.Data + 14), initialFrames = U32(c.Data + 16), scale = U32(c.Data + 20), rate = U32(c.Data + 24), start = U32(c.Data + 28), length = U32(c.Data + 32), suggestedBufferSize = U32(c.Data + 36), quality = U32(c.Data + 40), sampleSize = U32(c.Data + 44), duration100ns = U32(c.Data + 32) * (ulong)U32(c.Data + 20) * 10000000 / U32(c.Data + 24), start100ns = U32(c.Data + 28) * (ulong)U32(c.Data + 20) * 10000000 / U32(c.Data + 24) }).ToArray();
        var format = chunks.Last(c => c.Id == "strf");
        var movie = chunks.Single(c => c.Id == "LIST" && Id(c.Data) == "movi");
        return new { sourceSha = Convert.ToHexString(SHA256.HashData(source)), microSecPerFrame = U32(main.Data), maxBytesPerSec = U32(main.Data + 4), paddingGranularity = U32(main.Data + 8), flags = U32(main.Data + 12), isInterleaved = (U32(main.Data + 12) & 0x100) != 0, hasIndex = (U32(main.Data + 12) & 0x10) != 0, totalFrames = U32(main.Data + 16), initialFrames = U32(main.Data + 20), streams = U32(main.Data + 24), suggestedBufferSize = U32(main.Data + 28), headers, audioFormat = new { format.Size, tag = U16(format.Data), channels = U16(format.Data + 2), sampleRate = U32(format.Data + 4), byteRate = U32(format.Data + 8), blockAlign = U16(format.Data + 12), bits = U16(format.Data + 14), cbSize = format.Size >= 18 ? (int?)U16(format.Data + 16) : null }, index = chunks.Where(c => c.Id == "idx1").ToArray(), packets = chunks.Where(c => c.Parent == "movi").Select(c => new { c.Id, c.Header, c.Data, c.Size, moviRelativeOffset = c.Header - movie.Data }).ToArray(), timestampContract = "AVI chunks have no explicit per-packet PTS; stream sample index * scale/rate. Audio first sample index=0, video first frame index=0." };
    }
    [TestMethod]
    public void WhenCanonicalWaveIsGeneratedThenIndependentOracleMatches()
        => Assert.AreEqual(FixedPcmHash, Convert.ToHexString(SHA256.HashData(ValidateWave(MakeWave(FixedPcm())))));

    [TestMethod]
    [DataRow(4)] [DataRow(16)] [DataRow(20)] [DataRow(22)] [DataRow(24)] [DataRow(28)] [DataRow(32)] [DataRow(34)] [DataRow(40)]
    public void WhenWaveHeaderIsCorruptedThenStrictParserRejectsIt(int offset)
    {
        byte[] wave = MakeWave(FixedPcm()); wave[offset] ^= 1;
        Assert.ThrowsExactly<InvalidDataException>(() => ValidateWave(wave));
    }
    [TestMethod]
    public void WhenRiffChildCrossesParentThenParserRejectsIt()
    {
        byte[] source = new byte[24]; "RIFF"u8.CopyTo(source); BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(4), 16); "AVI LIST"u8.CopyTo(source.AsSpan(8)); BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(16), 5); "movi"u8.CopyTo(source.AsSpan(20));
        Assert.ThrowsExactly<InvalidDataException>(() => ParseRiff(source, "AVI "));
    }
    [TestMethod]
    public void WhenFixedAviIsParsedThenIndependentPcmMatches()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".avi");
        MediaFoundationResourceTests.WriteAudioOracleAvi(path);
        byte[] source = File.ReadAllBytes(path);
        _ = InspectAvi(source);
        Assert.AreEqual(FixedPcmHash, Convert.ToHexString(SHA256.HashData(ExtractFixedSourceAudio(source, out _))));
    }

    private sealed record DecodeComparison(bool Passed, int Bytes, int Frames, string Hash, int MatchingOffset, long? FirstPts, ulong? Duration100ns, bool Ended, int Crossings, object[] Samples, string[] Evidence);
    private static DecodeComparison DecodeContainer(string path, byte[] expected, string root, string name)
    {
        MediaFoundationAudioReader? reader = null;
        using MemoryStream actual = new();
        List<object> samples = [];
        byte[] block = new byte[65536];
        long? first = null, end = null;
        bool timeValid = true, ended = false;
        ulong? duration = null;
        int createHr = MediaFoundationAudioReader.Create(path, out reader), readHr = createHr;
        try
        {
            if (createHr >= 0 && reader is not null)
            {
                duration = reader.Duration100ns;
                for (int i = 0; i < 200 && !reader.Ended; i++)
                {
                    readHr = reader.Read(block, out int length, out long pts, out long sampleDuration, out uint flags);
                    samples.Add(new { readHr, length, pts, duration = sampleDuration, flags, reader.Format });
                    if (readHr < 0) break;
                    if (length == 0) continue;
                    first ??= pts;
                    timeValid &= (end is null || end == pts) && sampleDuration == length / 2 * 1250L && (flags & 0x100) == 0;
                    end = pts + sampleDuration;
                    actual.Write(block, 0, length);
                }
                ended = reader.Ended;
            }
        }
        finally { reader?.Dispose(); }
        byte[] bytes = actual.ToArray();
        int crossings = 0;
        for (int i = 1; i < Math.Min(8000, bytes.Length / 2); i++) if (BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan((i - 1) * 2)) <= 0 && BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(i * 2)) > 0) crossings++;
        bool passed = createHr >= 0 && readHr >= 0 && ended && first == 0 && end == 40000000 && timeValid && duration == 40000000 && bytes.AsSpan().SequenceEqual(expected) && crossings is >= 439 and <= 441;
        var result = new DecodeComparison(passed, bytes.Length, bytes.Length / 2, Convert.ToHexString(SHA256.HashData(bytes)), bytes.Length == 0 ? -1 : expected.AsSpan().IndexOf(bytes), first, duration, ended, crossings, samples.ToArray(), reader?.Evidence.ToArray() ?? []);
        File.WriteAllBytes(Path.Combine(root, name + "-actual.pcm"), bytes);
        File.WriteAllText(Path.Combine(root, name + "-result.json"), JsonSerializer.Serialize(new { createHr, readHr, sourceBytes = new FileInfo(path).Length, sourceHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), expectedHash = FixedPcmHash, result }, new JsonSerializerOptions { WriteIndented = true }));
        return result;
    }
    [TestMethod]
    public void WhenWaveAndAviAreComparedThenBothFixedOraclesAreRequired()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-audio-stage7-comparison");
        Directory.CreateDirectory(root);
        string avi = Path.Combine(root, "source.avi"), wav = Path.Combine(root, "source.wav");
        MediaFoundationResourceTests.WriteAudioOracleAvi(avi);
        byte[] source = File.ReadAllBytes(avi), expected = ExtractFixedSourceAudio(source, out var ranges);
        byte[] wave = MakeWave(expected); ValidateWave(wave); File.WriteAllBytes(wav, wave);
        File.WriteAllText(Path.Combine(root, "container.json"), JsonSerializer.Serialize(new { avi = InspectAvi(source), ranges, waveHeaderBytes = 44, waveBytes = wave.Length, pcmBytes = expected.Length, pcmFrames = expected.Length / 2, pcmHash = Convert.ToHexString(SHA256.HashData(expected)), runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), assemblySha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(IndependentAudioTests).Assembly.Location))), mvid = typeof(IndependentAudioTests).Assembly.ManifestModule.ModuleVersionId }, new JsonSerializerOptions { WriteIndented = true }));
        DecodeComparison wavResult = DecodeContainer(wav, expected, root, "wav");
        DecodeComparison aviResult = DecodeContainer(avi, expected, root, "avi");
        File.WriteAllText(Path.Combine(root, "comparison.json"), JsonSerializer.Serialize(new { wavResult.Passed, wav = wavResult, avi = aviResult, conclusion = wavResult.Passed && !aviResult.Passed ? "Container-dependent boundary implicated; internal demux cause unproven; AVI Failed." : "Shared reader/MF path requires investigation; no system-bug attribution." }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.IsTrue(wavResult.Passed && aviResult.Passed, "Both independent container hard gates are required; see saved comparison evidence.");
    }
}
