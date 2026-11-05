using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

// SDK 10.0.26100.0 mfreadwrite.h / mfobjects.h. Each instance owns its COM/MF balance.
// Synchronous local-file decoding is isolated from the video control thread.
internal sealed unsafe class MediaFoundationAudioReader : IDisposable
{
    private const uint AudioStream = 0xfffffffd;
    private const int InvalidType = unchecked((int)0xc00d36b4);
    private readonly int _thread = Environment.CurrentManagedThreadId;
    private nint _reader;
    private bool _com, _started, _closed;
    private int _closeResult;
    internal List<string> Evidence { get; } = [];
    internal AudioPcmFormat Format { get; private set; }
    internal bool Ended { get; private set; }
    internal ulong? Duration100ns { get; private set; }
    private static readonly Guid Major = new("48eba18e-f8c9-4687-bf11-0a74c9f96a8f");
    private static readonly Guid Subtype = new("f7e34c9a-42e8-4714-b74b-cb29d72c35e5");
    private static readonly Guid Audio = new("73647561-0000-0010-8000-00aa00389b71");
    private static readonly Guid Pcm = new("00000001-0000-0010-8000-00aa00389b71");
    private static readonly Guid Float = new("00000003-0000-0010-8000-00aa00389b71");
    private MediaFoundationAudioReader() { }
    private static nint* Table(nint p) => *(nint**)p;
    private int Record(string operation, int hr) { Evidence.Add($"{operation}:0x{hr:X8}"); return hr; }
    private void Release(string name, nint value)
    {
        if (value != 0) Evidence.Add($"{name}.Release:{((delegate* unmanaged[Stdcall]<nint, uint>)Table(value)[2])(value)}");
    }
    internal static int Create(string path, out MediaFoundationAudioReader? owner)
    {
        owner = null;
        if (!OperatingSystem.IsWindows()) return unchecked((int)0x80004001);
        if (string.IsNullOrWhiteSpace(path)) return unchecked((int)0x80070057);
        var pending = new MediaFoundationAudioReader();
        int hr = pending.Record("CoInitializeEx(MTA)", CoInitializeEx(0, 0));
        if (hr < 0) return hr;
        pending._com = true;
        nint reader = 0, type = 0;
        try
        {
            hr = pending.Record("MFStartup", MFStartup(0x20070, 0));
            if (hr < 0) return hr;
            pending._started = true;
            hr = pending.Record("MFCreateSourceReaderFromURL", MFCreateSourceReaderFromURL(path, 0, &reader));
            pending._reader = reader;
            if (hr < 0) return hr;
            hr = pending.Record("DeselectAll", ((delegate* unmanaged[Stdcall]<nint, uint, int, int>)Table(reader)[4])(reader, 0xfffffffe, 0));
            if (hr < 0) return hr;
            hr = pending.Record("SelectFirstAudio", ((delegate* unmanaged[Stdcall]<nint, uint, int, int>)Table(reader)[4])(reader, AudioStream, 1));
            if (hr < 0) return hr;
            hr = pending.RefreshFormat(native: true);
            if (hr < 0) return hr;
            bool compressed = hr == 1;
            AudioPcmFormat nativeFormat = pending.Format;
            Guid key;
            if (compressed)
            {
                hr = pending.Record("MFCreateMediaType", MFCreateMediaType(&type));
                if (hr < 0) return hr;
                key = Major;
                Guid value = Audio;
                hr = pending.Record("SetMajorAudio", ((delegate* unmanaged[Stdcall]<nint, Guid*, Guid*, int>)Table(type)[24])(type, &key, &value));
                if (hr < 0) return hr;
                key = Subtype; value = Pcm;
                hr = pending.Record("SetSubtypePCM", ((delegate* unmanaged[Stdcall]<nint, Guid*, Guid*, int>)Table(type)[24])(type, &key, &value));
                if (hr < 0) return hr;
                hr = pending.Record("SetCurrentMediaType", ((delegate* unmanaged[Stdcall]<nint, uint, uint*, nint, int>)Table(reader)[7])(reader, AudioStream, null, type));
                if (hr < 0) return hr;
            }
            // Keep the source's existing PCM path only when both complete formats satisfy our contract.
            hr = pending.RefreshFormat();
            if (hr < 0) return hr;
            if (!compressed && pending.Format != nativeFormat) return InvalidType;
            pending.Evidence.Add(compressed ? "OutputPath:DecodedPCM" : "OutputPath:NativePCM");
            // No ReadSample request is pending. Flush would discard samples, not seek.
            Guid timeFormat = Guid.Empty;
            Variant origin = new() { Type = 20, Value = 0 };
            hr = pending.Record("SetCurrentPosition(0)", ((delegate* unmanaged[Stdcall]<nint, Guid*, Variant*, int>)Table(reader)[8])(reader, &timeFormat, &origin));
            if (hr < 0) return hr;
            Variant duration = default;
            key = new("6c990d33-bb8e-477a-8598-0d5d96fcd88a");
            hr = pending.Record("Duration", ((delegate* unmanaged[Stdcall]<nint, uint, Guid*, Variant*, int>)Table(reader)[12])(reader, 0xffffffff, &key, &duration));
            if (hr >= 0 && duration.Type == 21) pending.Duration100ns = duration.Value;
            pending.Record("PropVariantClear", PropVariantClear(&duration));
            owner = pending;
            return 0;
        }
        finally
        {
            pending.Release("requestedType", type);
            if (owner is null) pending.Close();
        }
    }
    private int RefreshFormat(bool native = false)
    {
        nint type = 0;
        try
        {
            int hr = native
                ? Record("GetNativeMediaType(0)", ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint*, int>)Table(_reader)[5])(_reader, AudioStream, 0, &type))
                : Record("GetCurrentMediaType", ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)Table(_reader)[6])(_reader, AudioStream, &type));
            if (hr < 0) return hr;
            Guid key = Major, major = default, subtype = default;
            hr = Record("GetMajor", ((delegate* unmanaged[Stdcall]<nint, Guid*, Guid*, int>)Table(type)[10])(type, &key, &major));
            if (hr < 0) return hr;
            key = Subtype;
            hr = Record("GetSubtype", ((delegate* unmanaged[Stdcall]<nint, Guid*, Guid*, int>)Table(type)[10])(type, &key, &subtype));
            if (hr < 0) return hr;
            if (major != Audio) return InvalidType;
            if (subtype != Pcm && subtype != Float)
            {
                if (!native) return InvalidType;
                Evidence.Add($"NativeCompressed:major={major};subtype={subtype}");
                return 1;
            }
            uint* values = stackalloc uint[4];
            string[] keys = ["37e48bf5-645e-4c5b-89de-ada9e29b696a", "5faeeae7-0290-4c31-9e8a-c534f68d9dba", "f2deb57f-40fa-4764-aa33-ed4f2d1ff669", "322de230-9eeb-43bd-ab7a-ff412251541d"];
            for (int i = 0; i < keys.Length; i++)
            {
                key = new(keys[i]);
                hr = Record(keys[i], ((delegate* unmanaged[Stdcall]<nint, Guid*, uint*, int>)Table(type)[7])(type, &key, values + i));
                if (hr < 0) return hr;
            }
            var format = new AudioPcmFormat(values[0], values[1], values[2], values[3], subtype == Float);
            if (!format.IsSupported) return InvalidType;
            Format = format;
            Evidence.Add($"{(native ? "Native" : "Current")}Format:{format};major={major};subtype={subtype}");
            return 0;
        }
        finally { Release("currentType", type); }
    }
    internal int Read(Span<byte> destination, out int length, out long timestamp100ns, out long duration100ns, out uint flags)
    {
        CheckThread();
        length = 0; timestamp100ns = duration100ns = 0; flags = 0;
        if (_closed) return unchecked((int)0x80000013);
        if (Ended) { flags = 2; return 0; }
        nint sample = 0, buffer = 0;
        uint actual = 0, status = 0;
        long pts = 0, duration = 0;
        bool locked = false;
        try
        {
            int hr = Record("ReadSample", ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint*, uint*, long*, nint*, int>)Table(_reader)[9])(_reader, AudioStream, 0, &actual, &status, &pts, &sample));
            flags = status; timestamp100ns = pts;
            Evidence.Add($"Sample:stream={actual};flags={status};pts={pts}");
            if (hr < 0) return hr;
            if ((status & 1) != 0) return unchecked((int)0xc00d36b0);
            if ((status & 0x30) != 0) { hr = RefreshFormat(); if (hr < 0) return hr; }
            Ended = (status & 2) != 0;
            if (sample == 0) return 0;
            hr = Record("GetSampleDuration", ((delegate* unmanaged[Stdcall]<nint, long*, int>)Table(sample)[MediaFoundationSampleSlots.GetSampleDuration])(sample, &duration));
            if (hr < 0) return hr;
            duration100ns = duration;
            hr = Record("ConvertToContiguousBuffer", ((delegate* unmanaged[Stdcall]<nint, nint*, int>)Table(sample)[MediaFoundationSampleSlots.ConvertToContiguousBuffer])(sample, &buffer));
            if (hr < 0) return hr;
            byte* data = null;
            uint maximum = 0, current = 0;
            hr = Record("Lock", ((delegate* unmanaged[Stdcall]<nint, byte**, uint*, uint*, int>)Table(buffer)[3])(buffer, &data, &maximum, &current));
            if (hr < 0) return hr;
            locked = true;
            if (current > maximum || current % Format.BlockAlign != 0) return InvalidType;
            if (current > destination.Length) return unchecked((int)0x8007007a);
            new ReadOnlySpan<byte>(data, checked((int)current)).CopyTo(destination);
            length = (int)current;
            return 0;
        }
        finally
        {
            if (locked) Marshal.ThrowExceptionForHR(Record("Unlock", ((delegate* unmanaged[Stdcall]<nint, int>)Table(buffer)[4])(buffer)));
            Release("buffer", buffer); Release("sample", sample);
        }
    }
    private void CheckThread()
    {
        if (_thread != Environment.CurrentManagedThreadId) throw new InvalidOperationException();
    }
    internal int Close()
    {
        CheckThread();
        if (_closed) return _closeResult;
        _closed = true;
        Release("reader", _reader); _reader = 0;
        if (_started) { _closeResult = Record("MFShutdown", MFShutdown()); _started = false; }
        if (_com) { CoUninitialize(); Evidence.Add("CoUninitialize:void"); _com = false; }
        return _closeResult;
    }
    public void Dispose() => Marshal.ThrowExceptionForHR(Close());
    // PROPVARIANT has a pointer-bearing union: 24 bytes on x64, 16 on x86.
    [StructLayout(LayoutKind.Explicit)]
    private struct Variant
    {
        [FieldOffset(0)] internal ushort Type;
        [FieldOffset(8)] internal ulong Value;
        [FieldOffset(8)] internal VariantArray Array;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct VariantArray { internal uint Count; internal nint Pointer; }
    [DllImport("ole32.dll")] private static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32.dll")] private static extern void CoUninitialize();
    [DllImport("ole32.dll")] private static extern int PropVariantClear(Variant* value);
    [DllImport("mfplat.dll")] private static extern int MFStartup(uint version, uint flags);
    [DllImport("mfplat.dll")] private static extern int MFShutdown();
    [DllImport("mfplat.dll")] private static extern int MFCreateMediaType(nint* type);
    [DllImport("mfreadwrite.dll", CharSet = CharSet.Unicode)] private static extern int MFCreateSourceReaderFromURL(string url, nint attributes, nint* reader);
}

internal readonly record struct AudioPcmFormat(uint Channels, uint SampleRate, uint Bits, uint BlockAlign, bool FloatingPoint)
{
    internal bool IsSupported => Channels is 1 or 2 && SampleRate is >= 8000 and <= 192000 &&
        (FloatingPoint ? Bits == 32 : Bits == 16) && BlockAlign == Channels * (Bits / 8);
}

internal sealed class BoundedPcmBuffer
{
    private readonly byte[] _data;
    private readonly int _alignment;
    private int _read, _used;
    internal int Count => _used;
    internal BoundedPcmBuffer(int frames, AudioPcmFormat format)
    {
        if (!format.IsSupported || frames <= 0) throw new ArgumentOutOfRangeException(nameof(frames));
        _alignment = checked((int)format.BlockAlign);
        _data = new byte[checked(frames * _alignment)];
    }
    internal bool TryWrite(ReadOnlySpan<byte> source)
    {
        if (source.Length % _alignment != 0) throw new ArgumentException(null, nameof(source));
        if (source.Length > _data.Length - _used) return false;
        for (int i = 0; i < source.Length; i++) _data[(_read + _used + i) % _data.Length] = source[i];
        _used += source.Length;
        return true;
    }
    internal int Read(Span<byte> destination)
    {
        int length = Math.Min(_used, destination.Length / _alignment * _alignment);
        for (int i = 0; i < length; i++) destination[i] = _data[(_read + i) % _data.Length];
        _read = (_read + length) % _data.Length; _used -= length;
        return length;
    }
}
