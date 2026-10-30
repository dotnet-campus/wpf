using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [ThreadStatic] private static OfficialCacheObservation? officialCacheObservation;

    private sealed class OfficialCacheObservation : IDisposable
    {
        private readonly StreamWriter writer;
        private readonly long started = Stopwatch.GetTimestamp();

        internal OfficialCacheObservation(nint module, string requested, nint window)
        {
            Directory.CreateDirectory(NativeRun.PathsRoot);
            writer = new StreamWriter(Path.Combine(NativeRun.PathsRoot, $"official-cache-{Environment.ProcessId}-{window:X}.jsonl"));
            char* buffer = stackalloc char[32768];
            uint length = GetModuleFileNameW(module, buffer, 32768);
            string actual = new(buffer, 0, checked((int)length));
            using var stream = File.OpenRead(actual);
            Record("identity", new { Module = (long)module, Requested = requested, ActualPath = actual, Sha256 = Convert.ToHexString(SHA256.HashData(stream)), Hwnd = (long)window, ConnectionType = "official async connect0", Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(), CandidateSha256 = AcceptancePaths.ValidateIdentity(), Producer = "system D3D9 HAL; not consumer device", ConsumerDevice = "Unknown", Presented = "Unknown unless public message observed", NoPresent = "Unknown unless public message observed", CacheRenderingHint = "Unknown internal state; no explicit hint command submitted", ActualWorld = "Unknown internal state; submitted matrix recorded separately" });
        }

        internal void Record(string stage, object? value)
        {
            writer.WriteLine(JsonSerializer.Serialize(new { Qpc = Stopwatch.GetTimestamp(), ElapsedTicks = Stopwatch.GetTimestamp() - started, Frequency = Stopwatch.Frequency, Stage = stage, Value = value }));
            writer.Flush();
        }

        internal void Packet(nint channel, ReadOnlySpan<byte> bytes, int hr)
            => Record("submitted-command", new { Channel = (long)channel, Type = BitConverter.ToUInt32(bytes), Size = bytes.Length, Hex = Convert.ToHexString(bytes), HResult = hr });

        internal void DrainAfterResult(nint module, nint channel, bool completed)
        {
            Record("business-result-before-drain", new { Completed = completed, Channel = (long)channel, Semantics = "post-result only; no original drain or subsequent business observer; no extra flush or registration" });
            var peek = (delegate* unmanaged[Stdcall]<nint, void*, nuint, int*, int>)NativeLibrary.GetExport(module, "MilComposition_PeekNextMessage");
            byte* buffer = stackalloc byte[256];
            for (int count = 0; count < 64; count++)
            {
                new Span<byte>(buffer, 256).Clear();
                int retrieved = 0;
                int hr = peek(channel, buffer, 256, &retrieved);
                uint type = *(uint*)buffer;
                Record("public-peek-return", new { Channel = (long)channel, HResult = hr, Retrieved = retrieved, Type = type, Hex = Convert.ToHexString(new ReadOnlySpan<byte>(buffer, 256)), Scope = "channel-wide; public union does not identify HWND/target" });
                if (hr < 0 || retrieved == 0) return;
                if (type == 0x0a)
                    Record("public-presented-union", new { PresentationResults = *(uint*)(buffer + 8), RefreshRate = *(int*)(buffer + 12), PresentationTime = *(long*)(buffer + 16), Meaning = "official notification only; not content/backend proof" });
                else if (type == 0x10)
                    Record("public-bad-pixel-shader", new { Meaning = "channel bad shader notification; no shader handle in public union" });
            }
            Record("drain-bound", new { Limit = 64, Remaining = "Unknown" });
        }

        public void Dispose() => writer.Dispose();
    }
}
