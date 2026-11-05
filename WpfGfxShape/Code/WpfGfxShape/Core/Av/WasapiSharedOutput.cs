using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

// SDK 10.0.26100.0 mmdeviceapi.h / Audioclient.h. All native ownership stays on the caller.
internal sealed unsafe class WasapiSharedOutput
{
    internal List<string> Evidence { get; } = [];
    internal string Status { get; private set; } = "NotExecuted";
    internal uint SubmittedFrames { get; private set; }
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
        if (!OperatingSystem.IsWindows()) { Status = "Unsupported"; return unchecked((int)0x80004001); }
        if (pcm.Length != 64000) { Status = "Unsupported"; return unchecked((int)0x80070057); }
        Status = "Failed";
        SubmittedFrames = 0;
        FirstClock = LastClock = ClockFrequency = 0;
        nint enumerator = 0, device = 0, client = 0, render = 0, clock = 0, collection = 0;
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
            WaveFormat format = new() { Tag = 1, Channels = 1, Rate = 8000, ByteRate = 16000, Align = 2, Bits = 16 };
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
            ulong frequency = 0, position = 0, qpc = 0;
            hr = Hr("IAudioClock.GetFrequency", ((delegate* unmanaged[Stdcall]<nint, ulong*, int>)V(clock)[3])(clock, &frequency));
            if (hr < 0) return hr;
            ClockFrequency = frequency;
            hr = Hr("IAudioClock.GetPosition(before)", ((delegate* unmanaged[Stdcall]<nint, ulong*, ulong*, int>)V(clock)[4])(clock, &position, &qpc));
            if (hr < 0) return hr;
            FirstClock = position;
            hr = Call(client, 10, "Start");
            if (hr < 0) return hr;
            started = true;
            bool tailSubmitted = false;
            while (deadline.Elapsed < TimeSpan.FromSeconds(15))
            {
                cancellationToken.ThrowIfCancellationRequested();
                int remaining = (int)Math.Max(1, 15000 - deadline.ElapsedMilliseconds);
                if (!ready.WaitOne(Math.Min(remaining, 250))) continue;
                uint padding = 0;
                hr = Hr("GetCurrentPadding", ((delegate* unmanaged[Stdcall]<nint, uint*, int>)V(client)[6])(client, &padding));
                if (hr < 0) return hr;
                FinalPadding = padding;
                Evidence.Add($"PaddingFrames:{padding}");
                if (padding > capacity) return unchecked((int)0x80070057);
                hr = Hr("IAudioClock.GetPosition", ((delegate* unmanaged[Stdcall]<nint, ulong*, ulong*, int>)V(clock)[4])(clock, &position, &qpc));
                if (hr < 0) return hr;
                LastClock = position;
                Evidence.Add($"Clock:position={position};qpc100ns={qpc};frequency={frequency}");
                if (tailSubmitted && padding == 0)
                {
                    Status = SubmittedFrames == 32000 && LastClock > FirstClock && frequency > 0 ? "RenderingSubmittedAndClockAdvanced" : "ClockOrFramesFailed";
                    return Status == "RenderingSubmittedAndClockAdvanced" ? 0 : unchecked((int)0x80004005);
                }
                uint available = capacity - padding;
                if (available == 0 || tailSubmitted) continue;
                uint frames = SubmittedFrames < 32000 ? Math.Min(available, 32000 - SubmittedFrames) : Math.Min(available, 80u);
                nint data = 0;
                hr = Hr("IAudioRenderClient.GetBuffer", ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)V(render)[3])(render, frames, &data));
                if (hr < 0) return hr;
                bool silent = SubmittedFrames == 32000;
                // No throwing operation between successful GetBuffer and its paired ReleaseBuffer.
                if (!silent)
                {
                    fixed (byte* source = pcm) Buffer.MemoryCopy(source + SubmittedFrames * 2, (void*)data, frames * 2, frames * 2);
                }
                hr = Hr("IAudioRenderClient.ReleaseBuffer", ((delegate* unmanaged[Stdcall]<nint, uint, uint, int>)V(render)[4])(render, frames, silent ? 2u : 0u));
                Evidence.Add($"Packet:frames={frames};silent={silent};sourceOffsetFrames={SubmittedFrames}");
                if (hr < 0) return hr;
                if (silent) tailSubmitted = true; else SubmittedFrames += frames;
            }
            Status = "DeadlineExceeded";
            return unchecked((int)0x800705b4);
        }
        finally
        {
            if (started) Call(client, 11, "Stop");
            if (initialized) Call(client, 12, "Reset");
            Release("clock", clock); Release("render", render); Release("client", client);
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
