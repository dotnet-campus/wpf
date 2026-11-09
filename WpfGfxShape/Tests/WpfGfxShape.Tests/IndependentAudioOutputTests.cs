using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class IndependentAudioTests
{
    [TestMethod]
    public void WhenCompleteWaveIsRenderedThenEndpointClockAndDrainAreRequired()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-audio-output-stage7b");
        Directory.CreateDirectory(root);
        string avi = Path.Combine(root, "source.avi"), wav = Path.Combine(root, "source.wav");
        MediaFoundationResourceTests.WriteAudioOracleAvi(avi);
        byte[] expected = ExtractFixedSourceAudio(File.ReadAllBytes(avi), out _);
        File.WriteAllBytes(wav, MakeWave(expected));
        MediaFoundationAudioReader? reader = null;
        List<string> readerEvidence = [];
        using MemoryStream decoded = new();
        int createHr = MediaFoundationAudioReader.Create(wav, out reader), readHr = createHr;
        bool ended = false;
        try
        {
            Assert.IsTrue(createHr >= 0 && reader is not null);
            byte[] block = new byte[65536];
            for (int i = 0; i < 100 && reader is not null && !reader.Ended; i++)
            {
                readHr = reader.Read(block, out int length, out _, out _, out _);
                if (readHr < 0) break;
                decoded.Write(block, 0, length);
            }
            ended = reader?.Ended == true;
        }
        finally
        {
            reader?.Dispose();
            if (reader is not null) readerEvidence.AddRange(reader.Evidence);
        }
        byte[] pcm = decoded.ToArray();
        string before = Convert.ToHexString(SHA256.HashData(pcm));
        Assert.IsTrue(readHr >= 0 && ended && pcm.AsSpan().SequenceEqual(expected));
        WasapiSharedOutput output = new();
        int hr = unchecked((int)0x80004005);
        try { hr = output.Render(pcm, CancellationToken.None); }
        finally
        {
            List<object> modules = [];
            using Process process = Process.GetCurrentProcess();
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName.Equals("AudioSes.dll", StringComparison.OrdinalIgnoreCase) || module.ModuleName.Equals("MMDevAPI.dll", StringComparison.OrdinalIgnoreCase) || module.ModuleName.Equals("audioclient.dll", StringComparison.OrdinalIgnoreCase) || module.ModuleName.Equals("mfplat.dll", StringComparison.OrdinalIgnoreCase) || module.ModuleName.Equals("mfreadwrite.dll", StringComparison.OrdinalIgnoreCase))
                    modules.Add(new { module.ModuleName, module.FileName, module.FileVersionInfo.FileVersion, sha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(module.FileName))) });
            }
            File.WriteAllText(Path.Combine(root, "output.json"), JsonSerializer.Serialize(new
            {
                hr, hrHex = $"0x{hr:X8}", output.Status, output.SubmittedFrames, output.FirstClock, output.LastClock, output.ClockFrequency, output.FinalPadding,
                sourceFrames = pcm.Length / 2, beforeHash = before, afterHash = Convert.ToHexString(SHA256.HashData(pcm)), expectedHash = FixedPcmHash,
                createHr, readHr, ended, readerEvidence, output.Evidence, modules,
                runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                assemblySha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(IndependentAudioTests).Assembly.Location))),
                mvid = typeof(IndependentAudioTests).Assembly.ManifestModule.ModuleVersionId
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.IsTrue(hr == 0 && output.Status == "RenderingSubmittedAndClockAdvanced" && before == Convert.ToHexString(SHA256.HashData(pcm)),
            $"HRESULT=0x{hr:X8}; Status={output.Status}; {string.Join("; ", output.Evidence)}");
    }
}
