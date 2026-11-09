using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

// SDK 10.0.26100.0 mmdeviceapi.h / Audioclient.h. All native ownership stays on the caller.
internal sealed unsafe class WasapiSharedOutput
{
    internal List<string> Evidence { get; } = [];
    internal string Status { get; private set; } = "NotExecuted";
    internal ulong SubmittedFrames { get; private set; }
    internal ulong FirstClock { get; private set; }
    internal ulong LastClock { get; private set; }
    internal ulong ClockFrequency { get; private set; }
    internal uint FinalPadding { get; private set; }
    private static nint* V(nint p) => *(nint**)p;
    private int Hr(string name, int hr) { Evidence.Add($"{name}:0x{hr:X8}"); return hr; }
    private void Release(string name, nint p)
    {
        if (p != 0) Evidence.Add($"{name}.Release:{((delegate* unmanaged[Stdcall]<nint, uint>)V(p)[2])(p)}");
    }
    private int Call(nint p, int slot, string name) => Hr(name, ((delegate* unmanaged[Stdcall]<nint, int>)V(p)[slot])(p));
    private void Format(string name, nint p)
    {
        if (p == 0) { Evidence.Add(name + ":null"); return; }
        WaveFormat f = *(WaveFormat*)p;
        Evidence.Add($"{name}:tag={f.Tag};channels={f.Channels};rate={f.Rate};bytes={f.ByteRate};align={f.Align};bits={f.Bits};extra={f.Extra}");
    }
    internal int Render(byte[] pcm, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pcm);
        if (pcm.Length != 64000) return unchecked((int)0x80070057);
        return RenderCore(pcm, null, new(1, 8000, 16, 2, false), cancellationToken);
    }

    internal int Render(MediaFoundationAudioReader reader, CancellationToken cancellationToken, IndependentAudioPlaybackOwner? owner = null)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return RenderCore(new byte[65536], reader, reader.Format, cancellationToken, owner);
    }

    internal static uint WritableFrames(uint capacity, uint padding, int bytes, AudioPcmFormat format)
    {
        if (!format.IsSupported || padding > capacity || bytes < 0 || bytes % format.BlockAlign != 0)
            throw new ArgumentOutOfRangeException(nameof(bytes));
        return Math.Min(capacity - padding, (uint)bytes / format.BlockAlign);
    }

    // This linear attenuation contract is not a claim about WMP's undocumented gain curve.
    internal static (float Left, float Right) BalanceGains(double value)
    {
        if (!double.IsFinite(value) || value < -1 || value > 1)
            throw new ArgumentOutOfRangeException(nameof(value));
        // WmpPlayer::SetBalance submits a LONG percentage (C++ truncates toward zero).
        // Both initial and queued balances arrive here as raw targets; never quantize the cache.
        value = (int)(value * 100 + 0.5) / 100.0;
        return ((float)Math.Clamp(1 - Math.Max(0, value), 0, 1),
            (float)Math.Clamp(1 + Math.Min(0, value), 0, 1));
    }

    private int ApplyBalance(nint streamVolume, uint channels, double value)
    {
        if (channels == 1 && value != 0) return unchecked((int)0x80004001);
        var gains = BalanceGains(value);
        float* levels = stackalloc float[2] { gains.Left, gains.Right };
        return Hr("IAudioStreamVolume.SetAllVolumes", ((delegate* unmanaged[Stdcall]<nint, uint, float*, int>)V(streamVolume)[6])(streamVolume, channels, levels));
    }

    private int RenderCore(byte[] pcm, MediaFoundationAudioReader? reader, AudioPcmFormat sourceFormat, CancellationToken cancellationToken, IndependentAudioPlaybackOwner? owner = null)
    {
        if (!sourceFormat.IsSupported) return unchecked((int)0xc00d36b4);
        int offset = 0, buffered = reader is null ? pcm.Length : 0;
        bool ended = reader is null;
        if (!OperatingSystem.IsWindows()) { Status = "Unsupported"; return unchecked((int)0x80004001); }
        Status = "Failed";
        SubmittedFrames = 0;
        FirstClock = LastClock = ClockFrequency = 0;
        nint enumerator = 0, device = 0, client = 0, render = 0, clock = 0, collection = 0, volume = 0, streamVolume = 0;
        uint streamChannels = 0;
        nint mix = 0, closest = 0, id = 0;
        bool com = false, initialized = false, started = false;
        using EventWaitHandle ready = new(false, EventResetMode.AutoReset);
        Stopwatch deadline = Stopwatch.StartNew();
        try
        {
            int hr = Hr("CoInitializeEx(MTA)", CoInitializeEx(0, 0));
            if (hr < 0) return hr;
            com = true;
            Guid clsid = new("bcde0395-e52f-467c-8e3d-c4579291692e"), iid = new("a95664d2-9614-4f35-a746-de8db63617e6");
            hr = Hr("CoCreateInstance(MMDeviceEnumerator)", CoCreateInstance(&clsid, 0, 1, &iid, &enumerator));
            if (hr < 0) return hr;
            hr = Hr("GetDefaultAudioEndpoint(render,console)", ((delegate* unmanaged[Stdcall]<nint, int, int, nint*, int>)V(enumerator)[4])(enumerator, 0, 0, &device));
            if (hr < 0)
            {
                int enumHr = Hr("EnumAudioEndpoints(active,render)", ((delegate* unmanaged[Stdcall]<nint, int, uint, nint*, int>)V(enumerator)[3])(enumerator, 0, 1, &collection));
                if (enumHr >= 0 && collection != 0)
                {
                    uint count = 0;
                    Hr("IMMDeviceCollection.GetCount", ((delegate* unmanaged[Stdcall]<nint, uint*, int>)V(collection)[3])(collection, &count));
                    Evidence.Add($"ActiveRenderEndpointCount:{count}");
                }
                Status = hr == unchecked((int)0x80070490) ? "UnavailableNotCovered" : "EndpointFailedNotCovered";
                return hr;
            }
            hr = Hr("IMMDevice.GetId", ((delegate* unmanaged[Stdcall]<nint, nint*, int>)V(device)[5])(device, &id));
            if (hr < 0) return hr;
            Evidence.Add("EndpointId:" + Marshal.PtrToStringUni(id));
            iid = new("1cb9ad4c-dbfa-4c32-b178-c2f568a703b2");
            hr = Hr("IMMDevice.Activate(IAudioClient)", ((delegate* unmanaged[Stdcall]<nint, Guid*, uint, nint, nint*, int>)V(device)[3])(device, &iid, 1, 0, &client));
            if (hr < 0) return hr;
            hr = Hr("GetMixFormat", ((delegate* unmanaged[Stdcall]<nint, nint*, int>)V(client)[8])(client, &mix));
            if (hr < 0) return hr;
            Format("MixFormat", mix);
            WaveFormat format = new() { Tag = sourceFormat.FloatingPoint ? (ushort)3 : (ushort)1,
                Channels = (ushort)sourceFormat.Channels, Rate = sourceFormat.SampleRate,
                ByteRate = checked(sourceFormat.SampleRate * sourceFormat.BlockAlign),
                Align = (ushort)sourceFormat.BlockAlign, Bits = (ushort)sourceFormat.Bits };
            Hr("IsFormatSupported(fixedPCM,shared)", ((delegate* unmanaged[Stdcall]<nint, int, WaveFormat*, nint*, int>)V(client)[7])(client, 0, &format, &closest));
            Format("ClosestFormat", closest);
            // IsFormatSupported has no conversion flags. Initialize is authoritative for engine SRC.
            hr = Hr("Initialize(shared,event,autoconvert,quality)", ((delegate* unmanaged[Stdcall]<nint, int, uint, long, long, WaveFormat*, nint, int>)V(client)[3])(client, 0, 0x88040000, 0, 0, &format, 0));
            if (hr < 0) { Status = hr == unchecked((int)0x88890008) ? "Unsupported" : "InitializeFailed"; return hr; }
            initialized = true;
            hr = Hr("SetEventHandle", ((delegate* unmanaged[Stdcall]<nint, nint, int>)V(client)[13])(client, ready.SafeWaitHandle.DangerousGetHandle()));
            if (hr < 0) return hr;
            uint capacity = 0;
            hr = Hr("GetBufferSize", ((delegate* unmanaged[Stdcall]<nint, uint*, int>)V(client)[4])(client, &capacity));
            if (hr < 0) return hr;
            if (capacity == 0 || capacity > 80000) return unchecked((int)0x80070057);
            Evidence.Add($"BufferCapacityFrames:{capacity}");
            iid = new("f294acfc-3146-4483-a7bf-addca7c260e2");
            hr = Hr("GetService(IAudioRenderClient)", ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)V(client)[14])(client, &iid, &render));
            if (hr < 0) return hr;
            iid = new("cd63314f-3fba-4a1b-812c-ef96358728e7");
            hr = Hr("GetService(IAudioClock)", ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)V(client)[14])(client, &iid, &clock));
            if (hr < 0) return hr;
            if (owner is not null)
            {
                // SDK Audioclient.h: IUnknown, GetChannelCount(3), SetChannelVolume(4),
                // GetChannelVolume(5), SetAllVolumes(6), GetAllVolumes(7).
                iid = new("93014887-242d-4068-8a15-cf5e93b90fe3");
                hr = Hr("GetService(IAudioStreamVolume)", ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)V(client)[14])(client, &iid, &streamVolume));
                if (hr < 0) return hr;
                hr = Hr("IAudioStreamVolume.GetChannelCount", ((delegate* unmanaged[Stdcall]<nint, uint*, int>)V(streamVolume)[3])(streamVolume, &streamChannels));
                if (hr < 0) return hr;
                if (streamChannels != sourceFormat.Channels) return unchecked((int)0xc00d36b4);
                hr = ApplyBalance(streamVolume, streamChannels, owner.InitialBalance);
                if (hr < 0) return hr;
                iid = new("87ce5498-68d6-44e5-9215-6da47ef883d8");
                hr = Hr("GetService(ISimpleAudioVolume)", ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)V(client)[14])(client, &iid, &volume));
                if (hr < 0) return hr;
            }
            ulong frequency = 0, position = 0, qpc = 0;
            hr = Hr("IAudioClock.GetFrequency", ((delegate* unmanaged[Stdcall]<nint, ulong*, int>)V(clock)[3])(clock, &frequency));
            if (hr < 0) return hr;
            if (frequency == 0) return unchecked((int)0x80004005);
            ClockFrequency = frequency;
            hr = Hr("IAudioClock.GetPosition(before)", ((delegate* unmanaged[Stdcall]<nint, ulong*, ulong*, int>)V(clock)[4])(clock, &position, &qpc));
            if (hr < 0) return hr;
            FirstClock = position;
            owner?.ResetPosition(0, position, sourceFormat.SampleRate);
            owner?.CapturePosition(position, frequency);
            cancellationToken.ThrowIfCancellationRequested();
            if (owner?.InitiallyPaused != true)
            {
                hr = Call(client, 10, "Start");
                if (hr < 0) return hr;
                started = true;
            }
            // Ready acknowledges initialized services, not reader creation or end of stream.
            owner?.NotifyOutputReady();
            deadline.Restart();
            uint previousPadding = 0;
            WaitHandle[] runningWait = owner is null
                ? [cancellationToken.WaitHandle, ready]
                : [cancellationToken.WaitHandle, owner.CommandsReady, ready];
            WaitHandle[] pausedWait = owner is null
                ? [cancellationToken.WaitHandle]
                : [cancellationToken.WaitHandle, owner.CommandsReady];
            while (!started || deadline.Elapsed < TimeSpan.FromSeconds(15))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (owner is not null)
                {
                    while (owner.TryTakeCommand(out IndependentAudioPlaybackOwner.Command? command))
                    {
                        // A dequeued command is always completed, even when the native call fails.
                        int commandHr;
                        if (command!.Kind == IndependentAudioPlaybackOwner.CommandKind.Balance)
                            commandHr = ApplyBalance(streamVolume, streamChannels, command.Value);
                        else if (command.Kind == IndependentAudioPlaybackOwner.CommandKind.Volume)
                            commandHr = Hr("ISimpleAudioVolume.SetMasterVolume", ((delegate* unmanaged[Stdcall]<nint, float, nint, int>)V(volume)[3])(volume, (float)command.Value, 0));
                        else if (command.Kind == IndependentAudioPlaybackOwner.CommandKind.Seek)
                        {
                            commandHr = unchecked((int)0x800704c7);
                            try
                            {
                                long target = command.Timestamp100ns;
                                if (reader is null || reader.Duration100ns is not ulong duration)
                                    commandHr = unchecked((int)0x8007139f);
                                else
                                {
                                    target = (long)Math.Min((ulong)target, Math.Min(duration, (ulong)long.MaxValue));
                                    bool resumeAfterSeek = started;
                                    commandHr = Call(client, 11, "Seek.Stop");
                                    if (commandHr >= 0)
                                    {
                                        started = false;
                                        commandHr = Call(client, 12, "Seek.Reset");
                                    }
                                    // Commands run outside GetBuffer/ReleaseBuffer; no native buffer is outstanding.
                                    offset = buffered = 0;
                                    ended = false;
                                    previousPadding = FinalPadding = 0;
                                    if (commandHr >= 0) commandHr = reader.Seek(target);
                                    long anchor = target;
                                    while (commandHr >= 0 && buffered == 0 && !ended)
                                    {
                                        cancellationToken.ThrowIfCancellationRequested();
                                        commandHr = reader.Read(pcm, out buffered, out long pts, out _, out _);
                                        if (commandHr < 0) break;
                                        if (reader.Format != sourceFormat) { commandHr = unchecked((int)0xc00d36b4); break; }
                                        ended = reader.Ended;
                                        if (buffered == 0) continue;
                                        long skip = pts >= target ? 0 : (long)UInt128.Min(((UInt128)((Int128)target - pts) * sourceFormat.SampleRate + 9999999) / 10000000, (uint)buffered / sourceFormat.BlockAlign);
                                        offset = checked((int)skip * (int)sourceFormat.BlockAlign);
                                        buffered -= offset;
                                        anchor = (long)Int128.Clamp((Int128)pts + (Int128)skip * 10000000 / sourceFormat.SampleRate, 0, Math.Min(duration, (ulong)long.MaxValue));
                                    }
                                    if (commandHr >= 0)
                                        commandHr = Hr("IAudioClock.GetPosition(seek)", ((delegate* unmanaged[Stdcall]<nint, ulong*, ulong*, int>)V(clock)[4])(clock, &position, &qpc));
                                    if (commandHr >= 0)
                                    {
                                        cancellationToken.ThrowIfCancellationRequested();
                                        owner.ResetPosition(buffered == 0 ? target : anchor, position, sourceFormat.SampleRate);
                                        FirstClock = LastClock = position;
                                        if (resumeAfterSeek)
                                        {
                                            commandHr = Call(client, 10, "Seek.Start");
                                            started = commandHr >= 0;
                                        }
                                    }
                                }
                            }
                            catch (COMException error) { commandHr = error.HResult; }
                            catch (OperationCanceledException)
                            {
                                commandHr = unchecked((int)0x800704c7);
                                throw;
                            }
                            catch (Exception error) when (error is IOException or ArgumentException or InvalidOperationException or OutOfMemoryException)
                            {
                                command.Completion.TrySetException(error);
                                throw;
                            }
                            finally { command.Completion.TrySetResult(commandHr); }
                        }
                        else
                        {
                            bool resume = command.Kind == IndependentAudioPlaybackOwner.CommandKind.Resume;
                            commandHr = Call(client, resume ? 10 : 11, resume ? "Resume.Start" : "Pause.Stop");
                            if (commandHr >= 0)
                            {
                                started = resume;
                                commandHr = Hr("IAudioClock.GetPosition(control)", ((delegate* unmanaged[Stdcall]<nint, ulong*, ulong*, int>)V(clock)[4])(clock, &position, &qpc));
                                if (commandHr >= 0) owner.CapturePosition(position, frequency);
                            }
                        }
                        Evidence.Add($"Command.{command.Kind}.Executed:0x{commandHr:X8};thread={Environment.CurrentManagedThreadId}");
                        command.Completion.TrySetResult(commandHr);
                        if (commandHr < 0) return commandHr;
                        deadline.Restart();
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                if (!started)
                {
                    WaitHandle.WaitAny(pausedWait);
                    continue;
                }
                int remaining = (int)Math.Max(1, 15000 - deadline.ElapsedMilliseconds);
                int signal = WaitHandle.WaitAny(runningWait, Math.Min(remaining, 250));
                if (signal != runningWait.Length - 1) continue;
                uint padding = 0;
                hr = Hr("GetCurrentPadding", ((delegate* unmanaged[Stdcall]<nint, uint*, int>)V(client)[6])(client, &padding));
                if (hr < 0) return hr;
                FinalPadding = padding;
                if (padding < previousPadding) deadline.Restart();
                previousPadding = padding;
                if (Evidence.Count > 256) Evidence.RemoveRange(0, Evidence.Count - 256);
                if (padding > capacity) return unchecked((int)0x80070057);
                hr = Hr("IAudioClock.GetPosition", ((delegate* unmanaged[Stdcall]<nint, ulong*, ulong*, int>)V(clock)[4])(clock, &position, &qpc));
                if (hr < 0) return hr;
                LastClock = position;
                owner?.CapturePosition(position, frequency);
                Evidence.Add($"Clock:position={position};qpc100ns={qpc};frequency={frequency}");
                if (buffered == 0 && !ended && padding < capacity)
                {
                    hr = reader!.Read(pcm, out buffered, out _, out _, out _);
                    if (hr < 0) return hr;
                    if (reader.Format != sourceFormat) return unchecked((int)0xc00d36b4);
                    offset = 0;
                    ended = reader.Ended;
                }
                if (ended && buffered == 0 && padding == 0)
                {
                    bool valid = SubmittedFrames == 0 || LastClock > FirstClock && frequency > 0;
                    if (reader is null) valid &= SubmittedFrames == 32000;
                    Status = valid ? "RenderingSubmittedAndClockAdvanced" : "ClockOrFramesFailed";
                    return valid ? 0 : unchecked((int)0x80004005);
                }
                uint frames = WritableFrames(capacity, padding, buffered, sourceFormat);
                if (frames == 0) continue;
                nint data = 0;
                hr = Hr("IAudioRenderClient.GetBuffer", ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)V(render)[3])(render, frames, &data));
                if (hr < 0) return hr;
                // Both lengths were bounded before GetBuffer; no throwing operation before ReleaseBuffer.
                uint bytes = frames * sourceFormat.BlockAlign;
                fixed (byte* source = pcm) Buffer.MemoryCopy(source + offset, (void*)data, bytes, bytes);
                hr = Hr("IAudioRenderClient.ReleaseBuffer", ((delegate* unmanaged[Stdcall]<nint, uint, uint, int>)V(render)[4])(render, frames, 0));
                if (hr < 0) return hr;
                offset += (int)bytes;
                buffered -= (int)bytes;
                SubmittedFrames += frames;
                owner?.SubmitFrames(frames);
                deadline.Restart();
            }
            Status = "DeadlineExceeded";
            return unchecked((int)0x800705b4);
        }
        finally
        {
            if (started) Call(client, 11, "Stop");
            if (initialized) Call(client, 12, "Reset");
            Release("streamVolume", streamVolume);
            Release("volume", volume); Release("clock", clock); Release("render", render); Release("client", client);
            Release("device", device); Release("collection", collection); Release("enumerator", enumerator);
            if (mix != 0) { Marshal.FreeCoTaskMem(mix); Evidence.Add("MixFormat.CoTaskMemFree:void"); }
            if (closest != 0) { Marshal.FreeCoTaskMem(closest); Evidence.Add("ClosestFormat.CoTaskMemFree:void"); }
            if (id != 0) { Marshal.FreeCoTaskMem(id); Evidence.Add("EndpointId.CoTaskMemFree:void"); }
            ready.Dispose(); Evidence.Add("OwnedEvent.Close:void");
            if (com) { CoUninitialize(); Evidence.Add("CoUninitialize:void"); }
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    private struct WaveFormat
    {
        internal ushort Tag, Channels;
        internal uint Rate, ByteRate;
        internal ushort Align, Bits, Extra;
    }
    [DllImport("ole32.dll", ExactSpelling = true)] private static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32.dll", ExactSpelling = true)] private static extern void CoUninitialize();
    [DllImport("ole32.dll", ExactSpelling = true)] private static extern int CoCreateInstance(Guid* clsid, nint outer, uint context, Guid* iid, nint* result);
}
