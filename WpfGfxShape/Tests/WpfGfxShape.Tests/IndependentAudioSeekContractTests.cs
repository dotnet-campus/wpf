using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class IndependentAudioTests
{
    /// <summary>Isolates the initial seek from native PCM stream selection on the same real AVI.</summary>
    [TestMethod]
    public unsafe void WhenAviInitialSeekIsComparedThenNativePcmEvidenceIsRecorded()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string path = Path.Combine(root, "source.avi");
        MediaFoundationResourceTests.WriteAudioOracleAvi(path);
        byte[] source = File.ReadAllBytes(path);
        byte[] expected = ExtractFixedSourceAudio(source, out _);
        uint U32(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(source.AsSpan(offset, 4));
        int audioHeader = source.AsSpan().IndexOf("auds"u8);
        int mainHeader = source.AsSpan().IndexOf("avih"u8) + 8;
        var header = new
        {
            dwInitialFrames = U32(audioHeader + 16), dwStart = U32(audioHeader + 28),
            dwScale = U32(audioHeader + 20), dwRate = U32(audioHeader + 24),
            dwSampleSize = U32(audioHeader + 44), dwLength = U32(audioHeader + 32),
            avih = Enumerable.Range(0, 14).Select(i => U32(mainHeader + i * 4)).ToArray()
        };
        List<object> groups = [];
        string results = Path.Combine(AppContext.BaseDirectory, "TestResults");
        Directory.CreateDirectory(results);
        int initializeHr = AudioCoInitializeEx(0, 0);
        Assert.IsTrue(initializeHr is 0 or 1, $"CoInitializeEx:0x{initializeHr:X8}");
        try
        {
            CheckAudioHr(AudioMFStartup(0x20070, 0));
            try
            {
                foreach (bool seekZero in new[] { false, true })
                {
                    nint reader = 0, native = 0, current = 0;
                    List<int> readHrs = [];
                    long? firstPts = null;
                    int firstLength = 0;
                    bool eos = false;
                    using MemoryStream decoded = new();
                    try
                    {
                        CheckAudioHr(AudioMFCreateSourceReaderFromURL(path, 0, &reader));
                        CheckAudioHr(((delegate* unmanaged[Stdcall]<nint, uint, int, int>)AudioTable(reader)[4])(reader, 0xfffffffe, 0));
                        CheckAudioHr(((delegate* unmanaged[Stdcall]<nint, uint, int, int>)AudioTable(reader)[4])(reader, 0xfffffffd, 1));
                        CheckAudioHr(((delegate* unmanaged[Stdcall]<nint, uint, uint, nint*, int>)AudioTable(reader)[5])(reader, 0xfffffffd, 0, &native));
                        CheckAudioHr(((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)AudioTable(reader)[6])(reader, 0xfffffffd, &current));
                        Guid key = new("f7e34c9a-42e8-4714-b74b-cb29d72c35e5"), subtype = default;
                        CheckAudioHr(((delegate* unmanaged[Stdcall]<nint, Guid*, Guid*, int>)AudioTable(current)[10])(current, &key, &subtype));
                        Assert.AreEqual(new Guid("00000001-0000-0010-8000-00aa00389b71"), subtype);
                        uint equal = 0;
                        CheckAudioHr(((delegate* unmanaged[Stdcall]<nint, nint, uint*, int>)AudioTable(current)[35])(current, native, &equal));
                        Assert.AreEqual(15u, equal);
                        int? seekHr = null;
                        if (seekZero)
                        {
                            Guid time = Guid.Empty;
                            AudioSeekVariant origin = new() { Type = 20, Value = 0 };
                            seekHr = ((delegate* unmanaged[Stdcall]<nint, Guid*, AudioSeekVariant*, int>)AudioTable(reader)[8])(reader, &time, &origin);
                            CheckAudioHr(seekHr.Value);
                        }
                        for (int i = 0; i < 200 && !eos; i++)
                        {
                            nint sample = 0, buffer = 0;
                            bool locked = false;
                            uint stream = 0, flags = 0;
                            long pts = 0;
                            try
                            {
                                int hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint*, uint*, long*, nint*, int>)AudioTable(reader)[9])(reader, 0xfffffffd, 0, &stream, &flags, &pts, &sample);
                                readHrs.Add(hr);
                                CheckAudioHr(hr);
                                Assert.AreEqual(0u, flags & 0x31);
                                eos = (flags & 2) != 0;
                                if (sample == 0) continue;
                                CheckAudioHr(((delegate* unmanaged[Stdcall]<nint, nint*, int>)AudioTable(sample)[MediaFoundationSampleSlots.ConvertToContiguousBuffer])(sample, &buffer));
                                byte* bytes = null;
                                uint maximum = 0, length = 0;
                                CheckAudioHr(((delegate* unmanaged[Stdcall]<nint, byte**, uint*, uint*, int>)AudioTable(buffer)[3])(buffer, &bytes, &maximum, &length));
                                locked = true;
                                if (length > 0 && firstPts is null) { firstPts = pts; firstLength = (int)length; }
                                decoded.Write(new ReadOnlySpan<byte>(bytes, checked((int)length)));
                            }
                            finally
                            {
                                if (locked) CheckAudioHr(((delegate* unmanaged[Stdcall]<nint, int>)AudioTable(buffer)[4])(buffer));
                                AudioRelease(buffer); AudioRelease(sample);
                            }
                        }
                        byte[] actual = decoded.ToArray();
                        int sourceOffset = expected.AsSpan().IndexOf(actual);
                        groups.Add(new { seekZero, seekHr, firstPts, firstLength, totalBytes = actual.Length, sourceOffset, eos, readHrs, subtype, equal, sha256 = Convert.ToHexString(SHA256.HashData(actual)) });
                        Assert.IsTrue(eos);
                        Assert.IsTrue(actual.Length > 0 && sourceOffset >= 0);
                        Assert.AreEqual(expected.Length, sourceOffset + actual.Length);
                    }
                    finally { AudioRelease(current); AudioRelease(native); AudioRelease(reader); }
                }
            }
            finally { CheckAudioHr(AudioMFShutdown()); }
        }
        finally
        {
            AudioCoUninitialize();
            File.WriteAllText(Path.Combine(results, "avi-initial-seek.json"), JsonSerializer.Serialize(new
            {
                fixtureSha256 = Convert.ToHexString(SHA256.HashData(source)), expectedBytes = expected.Length,
                header, groups, fixturePath = path
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.AreEqual(2, groups.Count);
    }

    private static unsafe nint* AudioTable(nint value) => *(nint**)value;
    private static unsafe void AudioRelease(nint value)
    {
        if (value != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)AudioTable(value)[2])(value);
    }
    private static void CheckAudioHr(int hr) => Assert.AreEqual(0, hr, $"HRESULT 0x{hr:X8}");
    [StructLayout(LayoutKind.Explicit)]
    private struct AudioSeekVariant
    {
        [FieldOffset(0)] internal ushort Type;
        [FieldOffset(8)] internal long Value;
        [FieldOffset(8)] internal AudioSeekArray Array;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct AudioSeekArray { internal uint Count; internal nint Pointer; }
    [DllImport("ole32.dll", EntryPoint = "CoInitializeEx")] private static extern int AudioCoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32.dll", EntryPoint = "CoUninitialize")] private static extern void AudioCoUninitialize();
    [DllImport("mfplat.dll", EntryPoint = "MFStartup")] private static extern int AudioMFStartup(uint version, uint flags);
    [DllImport("mfplat.dll", EntryPoint = "MFShutdown")] private static extern int AudioMFShutdown();
    [DllImport("mfreadwrite.dll", EntryPoint = "MFCreateSourceReaderFromURL", CharSet = CharSet.Unicode)]
    private static extern unsafe int AudioMFCreateSourceReaderFromURL(string path, nint attributes, nint* reader);
}
