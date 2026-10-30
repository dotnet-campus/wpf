#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class UceChannelTests
{
    private sealed class WicFinalReleaseCapture
    {
        private readonly nint[] _addresses = new nint[96];
        private readonly long[] _ticks = new long[12];
        private readonly uint[] _threads = new uint[12];
        private ushort _count;
        private int _captured;
        private long _callbackTick;
        private uint _callbackThread;

        internal WicFinalReleaseCapture()
        {
            // Resolve native imports before entering the unmanaged disposal callback.
            _ = OwnerQueryPerformanceCounter(out _);
            _ = OwnerCaptureStack(0, 0, null, null);
            Mark(0);
        }

        internal void Mark(int phase)
        {
            _ = OwnerQueryPerformanceCounter(out _ticks[phase]);
            _threads[phase] = DrainGetCurrentThreadId();
        }

        internal void Capture()
        {
            if (Interlocked.CompareExchange(ref _captured, 1, 0) != 0) return;
            _ = OwnerQueryPerformanceCounter(out _callbackTick);
            _callbackThread = DrainGetCurrentThreadId();
            fixed (nint* addresses = _addresses)
                _count = OwnerCaptureStack(0, (uint)_addresses.Length, addresses, null);
        }

        internal void Write()
        {
            if (Volatile.Read(ref _captured) == 0) return;
            using var process = Process.GetCurrentProcess();
            var modules = process.Modules.Cast<ProcessModule>().Select(item => new
            {
                path = item.FileName,
                baseAddress = unchecked((ulong)(nuint)item.BaseAddress),
                size = item.ModuleMemorySize,
                sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(item.FileName)))
            }).ToArray();
            var frames = _addresses.Take(_count).Select((address, index) =>
            {
                ulong value = unchecked((ulong)(nuint)address);
                var module = modules.FirstOrDefault(item => value >= item.baseAddress && value - item.baseAddress < (ulong)item.size);
                return new { index, address = $"0x{value:X}", modulePath = module?.path,
                    moduleBase = module is null ? null : $"0x{module.baseAddress:X}",
                    moduleSha256 = module?.sha256, rva = module is null ? null : $"0x{value - module.baseAddress:X}" };
            }).ToArray();
            File.WriteAllText(Path.Combine(AcceptancePaths.Results, "wic-final-release-owner.json"), JsonSerializer.Serialize(new
            {
                capture = "RtlCaptureStackBackTrace; raw return addresses, not a complete managed/native unwind",
                pointerSize = IntPtr.Size, callerThread = _threads[0], callbackThread = _callbackThread,
                callbackQpc = _callbackTick, phases = _ticks.Select((tick, index) => new { index, tick, thread = _threads[index] }).ToArray(),
                phaseNames = new[] { "setup", "transfer.before", "transfer.after", "wrapper.release.after", "frame.release.after", "disconnect.before", "disconnect.after", "stream.release.before", "stream.release.after", "destroy.before", "destroy.after", "finally.cleanup.after" },
                frames, modules
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    [DllImport("ntdll.dll", EntryPoint = "RtlCaptureStackBackTrace", ExactSpelling = true)]
    private static extern ushort OwnerCaptureStack(uint skip, uint count, nint* addresses, uint* hash);
    [DllImport("kernel32.dll", EntryPoint = "QueryPerformanceCounter", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OwnerQueryPerformanceCounter(out long counter);
}
#endif
