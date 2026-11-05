using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

// Windows SDK 10.0.26100.0 mfobjects.h: IMFSampleVtbl, lines 996–1275.
internal static class MediaFoundationSampleSlots
{
    internal const int GetSampleFlags = 33, SetSampleFlags = 34;
    internal const int GetSampleTime = 35, SetSampleTime = 36;
    internal const int GetSampleDuration = 37, SetSampleDuration = 38;
    internal const int GetBufferCount = 39, GetBufferByIndex = 40;
    internal const int ConvertToContiguousBuffer = 41, AddBuffer = 42;
    internal const int RemoveBufferByIndex = 43, RemoveAllBuffers = 44;
    internal const int GetTotalLength = 45, CopyToBuffer = 46;
}

// ABI source: Windows SDK 10.0.26100.0 mfapi.h and mfobjects.h.
// Serialized, thread-affine owner; never create/close on an MF work-queue callback.
internal sealed unsafe class MediaFoundationSample
{
    private nint _sample;
    private nint _buffer;
    private bool _started;
    private readonly int _thread = Environment.CurrentManagedThreadId;
    internal int ReleasedReferences { get; private set; }
    internal int ShutdownCount { get; private set; }
    internal int UnlockCount { get; private set; }
    internal uint SampleReleaseCount { get; private set; }
    internal uint BufferReleaseCount { get; private set; }
    private int _closeResult;

    private MediaFoundationSample() { }

    internal static int Create(uint capacity, out MediaFoundationSample? owner)
    {
        owner = null;
        if (!OperatingSystem.IsWindows()) return unchecked((int)0x80004001);
        if (capacity == 0) return unchecked((int)0x80070057);
        var pending = new MediaFoundationSample();
        int result = MFStartup(0x00020070, 0);
        if (result < 0) return result;
        pending._started = true;
        try
        {
            nint sample = 0;
            result = MFCreateSample(&sample);
            pending._sample = sample;
            if (result < 0) return result;
            nint buffer = 0;
            result = MFCreateMemoryBuffer(capacity, &buffer);
            pending._buffer = buffer;
            if (result < 0) return result;
            // IUnknown(3) + IMFAttributes(30) + sample methods: AddBuffer = 42.
            result = ((delegate* unmanaged[Stdcall]<nint, nint, int>)Table(sample)[MediaFoundationSampleSlots.AddBuffer])(sample, buffer);
            if (result < 0) return result;
            owner = pending;
            return result;
        }
        finally
        {
            if (owner is null)
            {
                int close = pending.Close();
                if (close < 0) Marshal.ThrowExceptionForHR(close);
            }
        }
    }

    internal int Write(ReadOnlySpan<byte> data)
    {
        CheckOpen();
        byte* pointer = null;
        uint maximum = 0, current = 0;
        int result = ((delegate* unmanaged[Stdcall]<nint, byte**, uint*, uint*, int>)Table(_buffer)[3])
            (_buffer, &pointer, &maximum, &current);
        if (result < 0) return result;
        int unlock;
        try
        {
            if ((uint)data.Length > maximum) result = unchecked((int)0x80070057);
            else
            {
                data.CopyTo(new Span<byte>(pointer, data.Length));
                result = ((delegate* unmanaged[Stdcall]<nint, uint, int>)Table(_buffer)[6])(_buffer, (uint)data.Length);
            }
        }
        finally { unlock = Unlock(); }
        return result < 0 ? result : unlock;
    }

    internal int Read(Span<byte> destination, out uint length)
    {
        CheckOpen();
        length = 0;
        byte* pointer = null;
        uint maximum = 0, current = 0;
        int result = ((delegate* unmanaged[Stdcall]<nint, byte**, uint*, uint*, int>)Table(_buffer)[3])
            (_buffer, &pointer, &maximum, &current);
        if (result < 0) return result;
        int unlock;
        try
        {
            if (current > (uint)destination.Length) result = unchecked((int)0x8007007A);
            else
            {
                new ReadOnlySpan<byte>(pointer, checked((int)current)).CopyTo(destination);
                length = current;
            }
        }
        finally { unlock = Unlock(); }
        return result < 0 ? result : unlock;
    }

    internal int GetLengths(out uint maximum, out uint current, out uint total)
    {
        CheckOpen();
        uint max = 0, used = 0, sum = 0;
        int result = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)Table(_buffer)[7])(_buffer, &max);
        if (result >= 0) result = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)Table(_buffer)[5])(_buffer, &used);
        if (result >= 0) result = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)Table(_sample)[MediaFoundationSampleSlots.GetTotalLength])(_sample, &sum);
        maximum = max;
        current = used;
        total = sum;
        return result;
    }

    internal int Close()
    {
        CheckThread();
        if (_sample != 0)
        {
            SampleReleaseCount = Release(_sample);
            _sample = 0;
            ReleasedReferences++;
        }
        if (_buffer != 0)
        {
            BufferReleaseCount = Release(_buffer);
            _buffer = 0;
            ReleasedReferences++;
        }
        if (_started)
        {
            _started = false;
            ShutdownCount++;
            _closeResult = MFShutdown();
        }
        return _closeResult;
    }

    private int Unlock()
    {
        UnlockCount++;
        return ((delegate* unmanaged[Stdcall]<nint, int>)Table(_buffer)[4])(_buffer);
    }

    private void CheckThread()
    {
        if (_thread != Environment.CurrentManagedThreadId) throw new InvalidOperationException();
    }

    private void CheckOpen()
    {
        CheckThread();
        ObjectDisposedException.ThrowIf(!_started || _sample == 0 || _buffer == 0, this);
    }

    internal static void** Table(nint value) => *(void***)value;
    internal static uint Release(nint value) => ((delegate* unmanaged[Stdcall]<nint, uint>)Table(value)[2])(value);

    [DllImport("mfplat.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int MFStartup(uint version, uint flags);
    [DllImport("mfplat.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int MFShutdown();
    [DllImport("mfplat.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int MFCreateSample(nint* sample);
    [DllImport("mfplat.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int MFCreateMemoryBuffer(uint maximum, nint* buffer);
}

// Synchronous bounded video extraction only. The caller owns the MTA thread and must
// serialize Create/Read/Close there; in-flight cancellation is not implemented.
internal sealed unsafe class MediaFoundationSourceReader : IDisposable
{
    private const uint VideoStream = 0xfffffffc;
    private const int InvalidMediaType = unchecked((int)0xc00d36b4);
    private readonly int _thread = Environment.CurrentManagedThreadId;
    private nint _reader;
    private bool _com, _started, _ended;
    private int _reads, _closeResult;
    private long _previous = -1;
    internal int ShutdownCount { get; private set; }
    internal int UninitializeCount { get; private set; }
    internal int UnlockCount { get; private set; }
    internal int SampleReleaseCount { get; private set; }
    internal int BufferReleaseCount { get; private set; }
    internal uint ReaderFinalReferences { get; private set; }
    internal List<string> Cleanup { get; } = [];
    internal int Width { get; private set; }
    internal int Height { get; private set; }
    internal int Stride { get; private set; }
    internal Guid Subtype { get; private set; }
    internal ulong FrameRate { get; private set; }
    internal long? Duration100ns { get; private set; }
    internal int DurationResult { get; private set; } = unchecked((int)0xc00d36e6);
    internal bool? HasAudio { get; private set; }
    internal int AudioMetadataResult { get; private set; } = unchecked((int)0xc00d36e6);
    internal uint? Characteristics { get; private set; }
    internal int CharacteristicsResult { get; private set; } = unchecked((int)0xc00d36e6);
    internal int DisplayX { get; private set; }
    internal int DisplayY { get; private set; }
    internal int DisplayWidth { get; private set; }
    internal int DisplayHeight { get; private set; }
    internal string DisplayAreaSource { get; private set; } = "FrameSize";
    internal List<string> Formats { get; } = [];

    [StructLayout(LayoutKind.Sequential)]
    private struct VideoOffset
    {
        internal ushort Fraction;
        internal short Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct VideoArea
    {
        internal VideoOffset X, Y;
        internal int Width, Height;
    }

    private int ReadDisplayArea(nint type, int width, int height)
    {
        DisplayX = DisplayY = 0;
        DisplayWidth = width; DisplayHeight = height;
        DisplayAreaSource = "FrameSize";
        bool selected = false;
        foreach (var attribute in new[] {
            ("MinimumDisplayAperture", new Guid("d7388766-18fe-48c6-a177-ee894867c8c4")),
            ("GeometricAperture", new Guid("66758743-7e5f-400d-980a-aa8596c85696")) })
        {
            Guid key = attribute.Item2;
            VideoArea area = default;
            uint length = 0;
            int hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, byte*, uint, uint*, int>)Table(type)[15])
                (type, &key, (byte*)&area, (uint)sizeof(VideoArea), &length);
            Formats.Add($"{attribute.Item1}:0x{hr:X8};bytes={length};x={area.X.Value}+{area.X.Fraction}/65536;y={area.Y.Value}+{area.Y.Fraction}/65536;size={area.Width}x{area.Height}");
            if (hr == unchecked((int)0xc00d36e6)) continue;
            if (hr < 0) return hr;
            // Fractional apertures require resampling, which this integer-pixel shell does not implement.
            if (length != sizeof(VideoArea) || area.X.Fraction != 0 || area.Y.Fraction != 0 ||
                area.X.Value < 0 || area.Y.Value < 0 || area.Width <= 0 || area.Height <= 0 ||
                (long)area.X.Value + area.Width > width || (long)area.Y.Value + area.Height > height)
                return InvalidMediaType;
            if (!selected)
            {
                DisplayX = area.X.Value; DisplayY = area.Y.Value;
                DisplayWidth = area.Width; DisplayHeight = area.Height;
                DisplayAreaSource = attribute.Item1;
                selected = true;
            }
        }
        return 0;
    }
    private static readonly Guid MajorKey = new("48eba18e-f8c9-4687-bf11-0a74c9f96a8f");
    private static readonly Guid SubtypeKey = new("f7e34c9a-42e8-4714-b74b-cb29d72c35e5");
    private static readonly Guid Rgb32 = new("00000016-0000-0010-8000-00aa00389b71");
    private static readonly Guid Argb32 = new("00000015-0000-0010-8000-00aa00389b71");
    private static readonly Guid Rgb24 = new("00000014-0000-0010-8000-00aa00389b71");
    internal uint BufferLength { get; private set; }
    internal bool HasSample { get; private set; }
    internal List<string> Negotiation { get; } = [];
    internal Dictionary<string, string> SourceMetadata { get; } = [];
    private bool _compressedVideo;
    private readonly MediaFoundationReaderCallback? _callback;
    internal int? FlushResult { get; private set; }
    internal uint? CallbackFinalReferences { get; private set; }
    internal void RequestCancel() => _callback?.RequestClose();
    private MediaFoundationSourceReader(MediaFoundationReaderCallback? callback = null) { _callback = callback; }

    internal static int Create(string path, out MediaFoundationSourceReader? owner, bool enableCompressedVideo = false,
        Action<MediaFoundationSourceReader>? captureMetadata = null, MediaFoundationReaderCallback? callback = null)
    {
        owner = null;
        if (!OperatingSystem.IsWindows()) return unchecked((int)0x80004001);
        if (string.IsNullOrWhiteSpace(path)) return unchecked((int)0x80070057);
        var pending = new MediaFoundationSourceReader(callback) { _compressedVideo = enableCompressedVideo };
        nint attributes = 0, reader = 0;
        int hr = CoInitializeEx(0, 0);
        if (hr < 0) return hr;
        pending._com = true;
        try
        {
            hr = MFStartup(0x00020070, 0);
            if (hr < 0) return hr;
            pending._started = true;
            hr = MFCreateAttributes(&attributes, 0);
            if (hr < 0) return hr;
            if (callback is not null)
            {
                Guid asyncKey = new("1e3dbeac-bb43-4c35-b507-cd644464c965");
                hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint, int>)Table(attributes)[27])(attributes, &asyncKey, callback.Pointer);
                if (hr < 0) return hr;
            }
            if (enableCompressedVideo)
            {
                Guid processing = new("fb394f3d-ccf1-42ee-bbb3-f9b845d5681d");
                hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, uint, int>)Table(attributes)[21])(attributes, &processing, 1);
                pending.Negotiation.Add($"ENABLE_VIDEO_PROCESSING(UINT32=1):0x{hr:X8}");
                if (hr < 0) return hr;
            }
            hr = MFCreateSourceReaderFromURL(path, attributes, &reader);
            pending._reader = reader;
            if (hr < 0) return hr;
            // Inspect the source's native audio type before deselecting audio for video decoding.
            nint audioType = 0;
            int audioResult = ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint*, int>)Table(reader)[5])
                (reader, 0xfffffffd, 0, &audioType);
            try
            {
                if (audioResult >= 0 && audioType != 0)
                {
                    Guid major = default;
                    Guid key = MajorKey;
                    audioResult = ((delegate* unmanaged[Stdcall]<nint, Guid*, Guid*, int>)Table(audioType)[10])
                        (audioType, &key, &major);
                    if (audioResult >= 0)
                    {
                        if (major == new Guid("73647561-0000-0010-8000-00aa00389b71")) pending.HasAudio = true;
                        else audioResult = InvalidMediaType;
                    }
                }
                else if (audioResult == unchecked((int)0xc00d36b3))
                {
                    pending.HasAudio = false;
                    audioResult = 0;
                }
                pending.AudioMetadataResult = audioResult;
            }
            finally { if (audioType != 0) MediaFoundationSample.Release(audioType); }
            hr = ((delegate* unmanaged[Stdcall]<nint, uint, int, int>)Table(reader)[4])(reader, 0xfffffffe, 0);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint, int, int>)Table(reader)[4])(reader, VideoStream, 1);
            if (hr < 0) return hr;
            hr = pending.ReadPresentationMetadata();
            if (hr < 0) return hr;
            if (enableCompressedVideo) pending.ReadSourceMetadata();
            hr = pending.Configure();
            if (hr < 0) return hr;
            owner = pending;
            return 0;
        }
        finally
        {
            if (attributes != 0) pending.Cleanup.Add($"attributes.Release:{MediaFoundationSample.Release(attributes)}");
            if (owner is null)
            {
                // Snapshot partial metadata before releasing a source rejected by negotiation.
                try { captureMetadata?.Invoke(pending); }
                finally { pending.Dispose(); }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    private struct SourceVariant
    {
        [FieldOffset(0)] internal ushort Type;
        [FieldOffset(8)] internal ulong Unsigned;
        [FieldOffset(8)] internal nint Pointer;
    }

    internal static int DecodeDuration(int result, ushort type, ulong raw, out long? duration)
    {
        duration = null;
        if (result < 0) return result;
        if (type != 21 || raw > long.MaxValue) return InvalidMediaType;
        duration = (long)raw;
        return 0;
    }

    private int ReadPresentationMetadata()
    {
        Guid key = new("6c990d33-bb8e-477a-8598-0d5d96fcd88a");
        SourceVariant value = default;
        int hr = ((delegate* unmanaged[Stdcall]<nint, uint, Guid*, SourceVariant*, int>)Table(_reader)[12])(_reader, 0xffffffff, &key, &value);
        int clear;
        try
        {
            DurationResult = DecodeDuration(hr, value.Type, value.Unsigned, out long? duration);
            Duration100ns = duration;
            SourceMetadata["duration100ns.hr"] = $"0x{DurationResult:X8}";
            SourceMetadata["duration100ns.vt"] = value.Type.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (duration.HasValue) SourceMetadata["duration100ns"] = duration.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        finally { clear = PropVariantClear(&value); }
        if (clear < 0) return clear;
        if (DurationResult < 0 && DurationResult != unchecked((int)0xc00d36e6)) return DurationResult;
        key = new("6d23f5c8-c5d7-4a9b-9971-5d11f8bca880");
        value = default;
        hr = ((delegate* unmanaged[Stdcall]<nint, uint, Guid*, SourceVariant*, int>)Table(_reader)[12])(_reader, 0xffffffff, &key, &value);
        try
        {
            CharacteristicsResult = hr < 0 ? hr : value.Type == 19 ? 0 : InvalidMediaType;
            if (CharacteristicsResult >= 0) Characteristics = (uint)value.Unsigned;
            SourceMetadata["characteristics.hr"] = $"0x{CharacteristicsResult:X8}";
            SourceMetadata["characteristics.vt"] = value.Type.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (Characteristics.HasValue) SourceMetadata["characteristics"] = Characteristics.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        finally { clear = PropVariantClear(&value); }
        if (clear < 0) return clear;
        return CharacteristicsResult == unchecked((int)0xc00d36e6) ? 0 : CharacteristicsResult;
    }

    private void ReadSourceMetadata()
    {
        foreach (var item in new[] { ("duration100ns", "6c990d33-bb8e-477a-8598-0d5d96fcd88a"), ("mimeType", "6c990d37-bb8e-477a-8598-0d5d96fcd88a") })
        {
            Guid key = new(item.Item2);
            SourceVariant value = default;
            int hr = ((delegate* unmanaged[Stdcall]<nint, uint, Guid*, SourceVariant*, int>)Table(_reader)[12])(_reader, 0xffffffff, &key, &value);
            try
            {
                SourceMetadata[item.Item1 + ".hr"] = $"0x{hr:X8}";
                if (hr >= 0) SourceMetadata[item.Item1] = value.Type == 21 ? value.Unsigned.ToString(System.Globalization.CultureInfo.InvariantCulture) : value.Type == 31 ? Marshal.PtrToStringUni(value.Pointer) ?? "" : $"VT:{value.Type}";
            }
            finally { Marshal.ThrowExceptionForHR(PropVariantClear(&value)); }
        }
        for (uint stream = 0; stream < 16; stream++)
        {
            nint type = 0;
            int hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint*, int>)Table(_reader)[5])(_reader, stream, 0, &type);
            SourceMetadata[$"stream{stream}.hr"] = $"0x{hr:X8}";
            if (hr < 0) break;
            try
            {
                Guid key = MajorKey, major = default, subtype = default;
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, Guid*, Guid*, int>)Table(type)[10])(type, &key, &major));
                key = SubtypeKey;
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, Guid*, Guid*, int>)Table(type)[10])(type, &key, &subtype));
                SourceMetadata[$"stream{stream}.major"] = major.ToString();
                SourceMetadata[$"stream{stream}.subtype"] = subtype.ToString();
            }
            finally { Cleanup.Add($"metadataType.Release:{MediaFoundationSample.Release(type)}"); }
        }
    }

    [DllImport("ole32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int PropVariantClear(SourceVariant* value);

    private int Configure()
    {
        nint type = 0;
        try
        {
            // The limited Source Reader processor only promises YUV-to-RGB32.
            // Read supported native RGB directly rather than requesting RGB24 conversion.
            int hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint*, int>)Table(_reader)[5])(_reader, VideoStream, 0, &type);
            Negotiation.Add($"GetNativeMediaType:0x{hr:X8}");
            if (hr < 0) return hr;
            Guid frameSizeKey = new("1652c33d-d6b2-4012-b834-72030849a37d");
            ulong nativeSize = 0;
            int sizeResult = ((delegate* unmanaged[Stdcall]<nint, Guid*, ulong*, int>)Table(type)[8])(type, &frameSizeKey, &nativeSize);
            if (sizeResult >= 0 && (nativeSize >> 32) is > 0 and <= 8192 && (uint)nativeSize is > 0 and <= 8192)
            {
                DisplayWidth = (int)(nativeSize >> 32);
                DisplayHeight = (int)(uint)nativeSize;
            }
            hr = ValidateRgbType(type, out Guid subtype);
            Negotiation.Add($"native.Subtype:{subtype};validation:0x{hr:X8}");
            if (hr == InvalidMediaType && _compressedVideo)
            {
                SourceMetadata["nativeVideoSubtype"] = subtype.ToString();
                MediaFoundationSample.Release(type); type = 0;
                hr = MFCreateMediaType(&type);
                if (hr < 0) return hr;
                Guid key = MajorKey, value = new("73646976-0000-0010-8000-00aa00389b71");
                hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, Guid*, int>)Table(type)[24])(type, &key, &value);
                if (hr < 0) return hr;
                key = SubtypeKey; value = Rgb32;
                hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, Guid*, int>)Table(type)[24])(type, &key, &value);
            }
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint*, nint, int>)Table(_reader)[7])(_reader, VideoStream, null, type);
            Negotiation.Add($"SetCurrentMediaType:0x{hr:X8}");
            return hr < 0 ? hr : RefreshFormat();
        }
        finally { if (type != 0) Cleanup.Add($"nativeType.Release:{MediaFoundationSample.Release(type)}"); }
    }

    private static int ValidateRgbType(nint type, out Guid subtype)
    {
        Guid key = MajorKey, major = default, value = default;
        subtype = default;
        int hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, Guid*, int>)Table(type)[10])(type, &key, &major);
        if (hr < 0) return hr;
        if (major != new Guid("73646976-0000-0010-8000-00aa00389b71")) return InvalidMediaType;
        key = SubtypeKey;
        hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, Guid*, int>)Table(type)[10])(type, &key, &value);
        subtype = value;
        return hr < 0 ? hr : value == Rgb24 || value == Rgb32 || value == Argb32 ? 0 : InvalidMediaType;
    }

    private int RefreshFormat()
    {
        nint type = 0;
        try
        {
            int hr = ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)Table(_reader)[6])(_reader, VideoStream, &type);
            if (hr < 0) return hr;
            hr = ValidateRgbType(type, out Guid subtype);
            if (hr < 0)
            {
                Negotiation.Add($"Unsupported current subtype:{subtype};HRESULT=0x{hr:X8}");
                return hr;
            }
            Guid key = new("1652c33d-d6b2-4012-b834-72030849a37d");
            ulong size = 0;
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, ulong*, int>)Table(type)[8])(type, &key, &size);
            if (hr < 0) return hr;
            uint width = (uint)(size >> 32), height = (uint)size;
            // This owner deliberately bounds allocations, not a general playback cap.
            if (width == 0 || height == 0 || width > 8192 || height > 8192) return InvalidMediaType;
            key = new("644b4e48-1e02-4516-b0eb-c01ca9d49ac6");
            uint rawStride = 0;
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, uint*, int>)Table(type)[7])(type, &key, &rawStride);
            int stride = unchecked((int)rawStride);
            if (hr == unchecked((int)0xc00d36e6)) hr = MFGetStrideForBitmapInfoHeader(BitConverter.ToUInt32(subtype.ToByteArray()), width, &stride);
            if (hr < 0) return hr;
            int bytesPerPixel = subtype == Rgb24 ? 3 : 4;
            if (Math.Abs((long)stride) < width * (long)bytesPerPixel || Math.Abs((long)stride) * height > 268435456) return InvalidMediaType;
            key = new("c459a2e8-3d2c-4e44-b132-fee5156c7bb0");
            ulong rate = 0;
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, ulong*, int>)Table(type)[8])(type, &key, &rate);
            if (hr < 0) return hr;
            hr = ReadDisplayArea(type, (int)width, (int)height);
            if (hr < 0) return hr;
            Width = (int)width; Height = (int)height; Stride = stride; Subtype = subtype; FrameRate = rate;
            Formats.Add($"coded={Width}x{Height};stride={Stride};subtype={Subtype};display={DisplayX},{DisplayY},{DisplayWidth}x{DisplayHeight};source={DisplayAreaSource}");
            return 0;
        }
        finally { if (type != 0) Cleanup.Add($"currentType.Release:{MediaFoundationSample.Release(type)}"); }
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    private struct SeekPosition
    {
        [FieldOffset(0)] internal ushort Type;
        [FieldOffset(8)] internal long Value;
    }

    internal Task? PendingRead => _callback?.ReadCompletion;
    internal Task? PendingFlush => _callback?.FlushCompletion;

    internal int BeginRead()
    {
        CheckThread();
        ObjectDisposedException.ThrowIf(_reader == 0, this);
        return (_callback ?? throw new InvalidOperationException()).RequestRead(_reader);
    }

    internal int BeginFlush()
    {
        CheckThread();
        ObjectDisposedException.ThrowIf(_reader == 0, this);
        _callback?.BeginFlush();
        int hr = ((delegate* unmanaged[Stdcall]<nint, uint, int>)Table(_reader)[10])(_reader, 0xfffffffe);
        Cleanup.Add($"BeginFlush:epoch={_callback?.Epoch};hr=0x{hr:X8}");
        return hr;
    }

    internal int CompleteSeek(long timestamp)
    {
        CheckThread();
        _callback?.EndFlush();
        Cleanup.Add($"OnFlush+callbackDrain:epoch={_callback?.Epoch}");
        return SetPosition(timestamp);
    }

    internal int Seek(long timestamp)
    {
        CheckThread();
        ObjectDisposedException.ThrowIf(_reader == 0, this);
        if (timestamp < 0) return unchecked((int)0x80070057);
        int hr = BeginFlush();
        if (hr < 0) return hr;
        if (_callback is not null)
        {
            if (!_callback.WaitDrain(TimeSpan.FromSeconds(10))) return unchecked((int)0x800705b4);
            return CompleteSeek(timestamp);
        }
        return SetPosition(timestamp);
    }

    private int SetPosition(long timestamp)
    {
        Guid format = Guid.Empty;
        SeekPosition position = new() { Type = 20, Value = timestamp };
        int hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, SeekPosition*, int>)Table(_reader)[8])(_reader, &format, &position);
        if (hr >= 0) { _ended = false; _previous = -1; _reads = 0; }
        return hr;
    }

    internal int Read(out uint stream, out uint flags, out long timestamp, out byte[]? pixels)
        => ReadCore(true, out stream, out flags, out timestamp, out pixels);

    internal int CompleteRead(out uint stream, out uint flags, out long timestamp, out byte[]? pixels)
        => ReadCore(false, out stream, out flags, out timestamp, out pixels);

    private int ReadCore(bool request, out uint stream, out uint flags, out long timestamp, out byte[]? pixels)
    {
        CheckThread();
        ObjectDisposedException.ThrowIf(_reader == 0, this);
        stream = flags = 0; timestamp = 0; pixels = null;
        BufferLength = 0; HasSample = false;
        if (_ended || ++_reads > 4096) return unchecked((int)0x8000000b);
        nint sample = 0, buffer = 0;
        uint actual = 0, status = 0;
        long time = 0;
        try
        {
            int hr;
            if (_callback is null)
                hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint*, uint*, long*, nint*, int>)Table(_reader)[9])
                    (_reader, VideoStream, 0, &actual, &status, &time, &sample);
            else
            {
                hr = request ? _callback.RequestRead(_reader) : 0;
                if (hr >= 0) hr = _callback.WaitSample(out actual, out status, out time, out sample);
            }
            stream = actual; flags = status; timestamp = time; HasSample = sample != 0;
            if (hr < 0) return hr;
            _ended = (status & 2) != 0;
            if ((status & 1) != 0) return unchecked((int)0x80004005);
            if ((status & 0x10) != 0) hr = Configure();
            else if ((status & 0x20) != 0) hr = RefreshFormat();
            if (hr < 0 || sample == 0) return hr;
            if (time < 0 || time <= _previous) return InvalidMediaType;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)Table(sample)[MediaFoundationSampleSlots.ConvertToContiguousBuffer])(sample, &buffer);
            if (hr < 0) return hr;
            byte* data = null;
            uint maximum = 0, current = 0;
            hr = ((delegate* unmanaged[Stdcall]<nint, byte**, uint*, uint*, int>)Table(buffer)[3])(buffer, &data, &maximum, &current);
            if (hr < 0) return hr;
            int unlock;
            try
            {
                BufferLength = current;
                long pitch = Math.Abs((long)Stride), required = pitch * Height;
                if (data == null || current > maximum || current < required) hr = InvalidMediaType;
                else
                {
                    pixels = new byte[checked(Width * Height * 4)];
                    for (int y = 0; y < Height; y++)
                    {
                        long offset = (Stride < 0 ? Height - 1 - y : y) * pitch;
                        if (Subtype == Rgb24)
                        {
                            var row = new ReadOnlySpan<byte>(data + offset, Width * 3);
                            for (int x = 0; x < Width; x++)
                            {
                                int target = (y * Width + x) * 4;
                                row.Slice(x * 3, 3).CopyTo(pixels.AsSpan(target, 3));
                                pixels[target + 3] = 255;
                            }
                        }
                        else new ReadOnlySpan<byte>(data + offset, Width * 4).CopyTo(pixels.AsSpan(y * Width * 4));
                    }
                }
            }
            finally
            {
                unlock = ((delegate* unmanaged[Stdcall]<nint, int>)Table(buffer)[4])(buffer);
                UnlockCount++;
                Cleanup.Add($"Unlock:0x{unlock:X8}");
            }
            if (hr < 0 || unlock < 0) { pixels = null; return hr < 0 ? hr : unlock; }
            _previous = time;
            return hr;
        }
        finally
        {
            if (buffer != 0) { Cleanup.Add($"buffer.Release:{MediaFoundationSample.Release(buffer)}"); BufferReleaseCount++; }
            if (sample != 0) { Cleanup.Add($"sample.Release:{MediaFoundationSample.Release(sample)}"); SampleReleaseCount++; }
        }
    }

    internal int Close()
    {
        CheckThread();
        if (_reader != 0 && _callback is not null)
        {
            _callback.RequestClose();
            _callback.BeginFlush();
            if (FlushResult is null)
                FlushResult = ((delegate* unmanaged[Stdcall]<nint, uint, int>)Table(_reader)[10])(_reader, 0xfffffffe);
            Cleanup.Add($"Flush:0x{FlushResult.Value:X8}");
            if (FlushResult < 0) return FlushResult.Value;
            if (!_callback.WaitDrain(TimeSpan.FromSeconds(10))) return unchecked((int)0x800705b4);
            Cleanup.Add("OnFlush+callbackDrain");
        }
        if (_reader != 0)
        {
            ReaderFinalReferences = MediaFoundationSample.Release(_reader);
            Cleanup.Add($"reader.Release:{ReaderFinalReferences}"); _reader = 0;
        }
        if (_callback is not null && CallbackFinalReferences is null)
        {
            CallbackFinalReferences = _callback.ReleaseOwner();
            Cleanup.Add($"callback.Release:{CallbackFinalReferences}");
        }
        if (_started)
        {
            _started = false; ShutdownCount++; _closeResult = MFShutdown();
            Cleanup.Add($"MFShutdown:0x{_closeResult:X8}");
        }
        if (_com) { _com = false; CoUninitialize(); UninitializeCount++; Cleanup.Add("CoUninitialize"); }
        return _closeResult;
    }

    /// <summary>Closes this synchronous owner on its creating MTA thread.</summary>
    public void Dispose() => Marshal.ThrowExceptionForHR(Close());
    private void CheckThread() { if (_thread != Environment.CurrentManagedThreadId) throw new InvalidOperationException(); }
    private static void** Table(nint value) => MediaFoundationSample.Table(value);
    [DllImport("ole32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern void CoUninitialize();
    [DllImport("mfplat.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int MFStartup(uint version, uint flags);
    [DllImport("mfplat.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int MFShutdown();
    [DllImport("mfplat.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int MFCreateAttributes(nint* attributes, uint size);
    [DllImport("mfplat.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int MFCreateMediaType(nint* type);
    [DllImport("mfplat.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int MFGetStrideForBitmapInfoHeader(uint format, uint width, int* stride);
    [DllImport("mfreadwrite.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int MFCreateSourceReaderFromURL(string url, nint attributes, nint* reader);
}
