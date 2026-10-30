using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

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
            result = ((delegate* unmanaged[Stdcall]<nint, nint, int>)Table(sample)[42])(sample, buffer);
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
        if (result >= 0) result = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)Table(_sample)[45])(_sample, &sum);
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

    private static void** Table(nint value) => *(void***)value;
    private static uint Release(nint value) => ((delegate* unmanaged[Stdcall]<nint, uint>)Table(value)[2])(value);

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
