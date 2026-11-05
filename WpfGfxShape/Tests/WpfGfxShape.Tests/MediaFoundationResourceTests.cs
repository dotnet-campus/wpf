using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed partial class MediaFoundationResourceTests
{
    [TestMethod]
    public void WhenSystemSampleOwnsBufferThenBytesRoundTripAndResourcesClose()
    {
        Assert.IsTrue(OperatingSystem.IsWindows());
        byte[] input = [0, 17, 127, 128, 254, 255];
        byte[] output = new byte[input.Length];
        var operations = new List<object>();
        MediaFoundationSample? owner = null;
        bool completed = false;
        string root = Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT") ?? Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(root);
        string assembly = typeof(MediaFoundationResourceTests).Assembly.Location;
        void Save() => File.WriteAllText(Path.Combine(root, "media-foundation-resource.json"), JsonSerializer.Serialize(new
        {
            completed, runtime = RuntimeInformation.FrameworkDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            testAssembly = assembly, testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly))),
            testMvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId,
            implementation = typeof(MediaFoundationSample).Assembly.FullName,
            input, output, operations,
            owner?.ReleasedReferences, owner?.ShutdownCount, owner?.UnlockCount,
            owner?.SampleReleaseCount, owner?.BufferReleaseCount,
            modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
                .Where(module => string.Equals(module.ModuleName, "mfplat.dll", StringComparison.OrdinalIgnoreCase))
                .Select(module => new { module.FileName, version = module.FileVersionInfo.FileVersion }).ToArray()
        }, new JsonSerializerOptions { WriteIndented = true }));
        void Check(string operation, int hr, int expected = 0)
        {
            operations.Add(new { operation, hr = $"0x{hr:X8}" });
            Save();
            Assert.AreEqual(expected, hr, operation);
        }
        try
        {
            Check("MFStartup/CreateSample/CreateMemoryBuffer/AddBuffer", MediaFoundationSample.Create(32, out owner));
            Assert.IsNotNull(owner);
            Check("initial-lengths", owner.GetLengths(out uint maximum, out uint current, out uint total));
            operations.Add(new { operation = "initial-values", maximum, current, total });
            Assert.AreEqual(32u, maximum);
            Assert.AreEqual(0u, current);
            Assert.AreEqual(0u, total);
            Check("Lock/write/SetCurrentLength/Unlock", owner.Write(input));
            Check("written-lengths", owner.GetLengths(out maximum, out current, out total));
            operations.Add(new { operation = "written-values", maximum, current, total });
            Assert.AreEqual(6u, current);
            Assert.AreEqual(6u, total);
            Check("oversized-write", owner.Write(new byte[33]), unchecked((int)0x80070057));
            Check("undersized-read", owner.Read(new byte[5], out uint rejected), unchecked((int)0x8007007A));
            Assert.AreEqual(0u, rejected);
            Check("Lock/read/Unlock", owner.Read(output, out uint length));
            Assert.AreEqual(6u, length);
            CollectionAssert.AreEqual(input, output);
            Assert.AreEqual(4, owner.UnlockCount);
            Check("Release/Shutdown", owner.Close());
            Check("idempotent-close", owner.Close());
            Assert.AreEqual(2, owner.ReleasedReferences);
            Assert.AreEqual(1, owner.ShutdownCount);
            Assert.AreEqual(0u, owner.SampleReleaseCount);
            Assert.AreEqual(0u, owner.BufferReleaseCount);
            Assert.ThrowsExactly<ObjectDisposedException>(() => owner.GetLengths(out _, out _, out _));
            completed = true;
        }
        finally
        {
            int close = owner?.Close() ?? 0;
            operations.Add(new { operation = "finally-close", hr = $"0x{close:X8}" });
            Save();
            Assert.AreEqual(0, close);
        }
    }
}

public sealed partial class MediaFoundationResourceTests
{
    [TestMethod]
    public void WhenOriginalAviIsReadBySystemSourceReaderThenAllVideoFramesReachEosAndClose()
    {
        Assert.IsTrue(OperatingSystem.IsWindows());
        string root = Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT") ?? Path.Combine(AppContext.BaseDirectory, "Results", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "original-red-blue-40.avi");
        WriteSourceReaderAvi(input);
        string assembly = typeof(MediaFoundationResourceTests).Assembly.Location;
        string inputSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(input)));
        Assert.AreEqual("01BE17E50794DD8A076967E49D0C2CD6265A3908247748B4B95689AB2F07C872", inputSha);
        Assert.AreEqual(65284L, new FileInfo(input).Length);
        var reads = new List<object>();
        var frames = new List<(long Time, byte[] Pixels)>();
        var operations = new List<object>();
        MediaFoundationSourceReader? owner = null;
        bool completed = false, eos = false;
        void Save() => File.WriteAllText(Path.Combine(root, "media-foundation-source-reader.json"), JsonSerializer.Serialize(new
        {
            completed, input, inputSha, inputLength = new FileInfo(input).Length,
            source = "Original pure C# DIB24 2x1 stride8 10fps 40 frames 4s; red[0,2s), blue[2s,4s); PCM mono 8kHz 16bit ignored, not rendered",
            runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size,
            testAssembly = assembly, testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly))),
            testMvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId,
            implementation = typeof(MediaFoundationSourceReader).Assembly.FullName,
            reads, operations, frameCount = frames.Count, eos, owner?.Negotiation,
            nullReads = reads.Count - frames.Count,
            owner?.Cleanup, owner?.ShutdownCount, owner?.UninitializeCount, owner?.UnlockCount,
            owner?.SampleReleaseCount, owner?.BufferReleaseCount, owner?.ReaderFinalReferences,
            modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
                .Where(m => m.ModuleName is not null && (m.ModuleName.Equals("mfreadwrite.dll", StringComparison.OrdinalIgnoreCase) || m.ModuleName.Equals("mfplat.dll", StringComparison.OrdinalIgnoreCase)))
                .Select(m => new { m.FileName, version = m.FileVersionInfo.FileVersion }).ToArray()
        }, new JsonSerializerOptions { WriteIndented = true }));
        void Check(string name, int hr)
        {
            operations.Add(new { name, hr = $"0x{hr:X8}" }); Save(); Assert.AreEqual(0, hr, name);
        }
        Save();
        try
        {
            Check("CoInitializeEx/MFStartup/CreateReader/select-video/native-RGB", MediaFoundationSourceReader.Create(input, out owner));
            VerifyPropVariantStorage();
            Assert.IsNotNull(owner);
            var elapsed = Stopwatch.StartNew();
            for (int i = 0; i < 64 && !eos; i++)
            {
                Assert.IsTrue(elapsed.Elapsed < TimeSpan.FromSeconds(30));
                int hr = owner.Read(out uint stream, out uint flags, out long time, out byte[]? pixels);
                reads.Add(new { index = i, hr = $"0x{hr:X8}", stream, flags = $"0x{flags:X8}", timestamp100ns = time,
                    owner.Width, owner.Height, owner.Stride, owner.Subtype, owner.FrameRate,
                    nullSample = !owner.HasSample, owner.BufferLength, outputLength = pixels?.Length,
                    streamTick = (flags & 0x100) != 0, currentTypeChanged = (flags & 0x20) != 0,
                    platformContent = pixels is null ? null : pixels[2] > pixels[0] + 64 && pixels[2] > pixels[1] + 64 ? "red" : pixels[0] > pixels[2] + 64 && pixels[0] > pixels[1] + 64 ? "blue" : "other",
                    pixelsHex = pixels is null ? null : Convert.ToHexString(pixels),
                    sha256 = pixels is null ? null : Convert.ToHexString(SHA256.HashData(pixels)) });
                Save();
                Assert.AreEqual(0, hr);
                Assert.IsTrue(time <= 40000000);
                if (pixels is not null)
                {
                    Assert.AreEqual(2, owner.Width); Assert.AreEqual(1, owner.Height);
                    Assert.AreEqual(8, pixels.Length);
                    Assert.AreEqual(frames.Count * 1000000L, time);
                    frames.Add((time, pixels));
                }
                eos = (flags & 2) != 0;
            }
            Assert.IsTrue(eos);
            // Microsoft Source Reader contract: one uncompressed video frame per sample.
            Assert.AreEqual(40, frames.Count);
            foreach (int index in Enumerable.Range(0, frames.Count))
            {
                byte[] pixels = frames[index].Pixels;
                for (int p = 0; p < 8; p += 4)
                {
                    Assert.IsTrue(index < 20 ? pixels[p + 2] > pixels[p] + 64 && pixels[p + 2] > pixels[p + 1] + 64
                        : pixels[p] > pixels[p + 2] + 64 && pixels[p] > pixels[p + 1] + 64);
                }
            }
            Assert.AreNotEqual(Convert.ToHexString(frames[0].Pixels), Convert.ToHexString(frames[20].Pixels));
            Check("Close", owner.Close());
            owner.Dispose(); owner.Dispose();
            Check("repeat-Close", owner.Close());
            Assert.AreEqual(40, owner.SampleReleaseCount); Assert.AreEqual(40, owner.BufferReleaseCount);
            Assert.AreEqual(40, owner.UnlockCount); Assert.AreEqual(1, owner.ShutdownCount);
            Assert.AreEqual(1, owner.UninitializeCount); Assert.AreEqual(0u, owner.ReaderFinalReferences);
            Assert.ThrowsExactly<ObjectDisposedException>(() => owner.Read(out _, out _, out _, out _));
            completed = true;
        }
        finally
        {
            int close = owner?.Close() ?? 0;
            operations.Add(new { name = "finally-Close", hr = $"0x{close:X8}" }); Save();
            Assert.AreEqual(0, close);
        }
    }

    internal static void WriteAudioOracleAvi(string path) => WriteSourceReaderAvi(path);

    private static void WriteSourceReaderAvi(string path, bool includeAudio = true)
    {
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII);
        long Begin(string id, string? type = null)
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes(id)); writer.Write(0);
            long start = stream.Position;
            if (type is not null) writer.Write(System.Text.Encoding.ASCII.GetBytes(type));
            return start;
        }
        void End(long start)
        {
            long end = stream.Position;
            stream.Position = start - 4; writer.Write(checked((int)(end - start))); stream.Position = end;
        }
        long riff = Begin("RIFF", "AVI "), header = Begin("LIST", "hdrl"), main = Begin("avih");
        writer.Write(100000); writer.Write(16080); writer.Write(0); writer.Write(0);
        writer.Write(40); writer.Write(0); writer.Write(includeAudio ? 2 : 1); writer.Write(1600); writer.Write(2); writer.Write(1);
        writer.Write(new byte[16]); End(main);
        long list = Begin("LIST", "strl"), info = Begin("strh");
        writer.Write(System.Text.Encoding.ASCII.GetBytes("vidsDIB ")); writer.Write(0); writer.Write(0);
        writer.Write(0); writer.Write(1); writer.Write(10); writer.Write(0); writer.Write(40);
        writer.Write(8); writer.Write(-1); writer.Write(0);
        writer.Write((short)0); writer.Write((short)0); writer.Write((short)2); writer.Write((short)1); End(info);
        long format = Begin("strf");
        writer.Write(40); writer.Write(2); writer.Write(1); writer.Write((short)1); writer.Write((short)24);
        writer.Write(0); writer.Write(8); writer.Write(new byte[16]); End(format); End(list);
        if (includeAudio)
        {
        list = Begin("LIST", "strl"); info = Begin("strh");
        writer.Write(System.Text.Encoding.ASCII.GetBytes("auds")); writer.Write(0); writer.Write(0); writer.Write(0);
        writer.Write(0); writer.Write(2); writer.Write(16000); writer.Write(0); writer.Write(32000);
        writer.Write(1600); writer.Write(-1); writer.Write(2); writer.Write(new byte[8]); End(info);
        format = Begin("strf");
        writer.Write((short)1); writer.Write((short)1); writer.Write(8000); writer.Write(16000);
        writer.Write((short)2); writer.Write((short)16); End(format); End(list);
        }
        End(header);
        long movie = Begin("LIST", "movi");
        for (int frame = 0; frame < 40; frame++)
        {
            uint color = frame < 20 ? 0xffff0000u : 0xff0000ffu;
            long chunk = Begin("00db");
            writer.Write(new byte[] { (byte)color, (byte)(color >> 8), (byte)(color >> 16), (byte)color, (byte)(color >> 8), (byte)(color >> 16), 0, 0 });
            End(chunk);
            if (includeAudio)
            {
                chunk = Begin("01wb");
                for (int sample = 0; sample < 800; sample++) writer.Write((short)(1000 * Math.Sin(2 * Math.PI * 440 * (frame * 800 + sample) / 8000)));
                End(chunk);
            }
        }
        End(movie); End(riff);
    }
}
