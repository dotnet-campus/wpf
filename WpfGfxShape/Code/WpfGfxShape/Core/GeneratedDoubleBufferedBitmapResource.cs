using System.Runtime.InteropServices;
using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct MilDoubleBufferedBitmapCommand
{
    internal readonly MilCommand Type;
    internal readonly uint Handle;
    internal readonly ulong Bitmap;
    internal readonly int UseBackBuffer;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct MilDoubleBufferedBitmapCopyCommand
{
    internal readonly MilCommand Type;
    internal readonly uint Handle;
    internal readonly ulong Event;
}

internal sealed partial class GeneratedDoubleBufferedBitmapResource() : GeneratedProtocolResource(MilResourceType.DoubleBufferedBitmap)
{
    private FrozenComSlot? _sourceSlot;
    private nint _bitmap => _sourceSlot?.Pointer ?? 0;

    internal FrozenBitmapLease? PinBitmapSource(out nint source)
    {
        FrozenBitmapLease? lease = _sourceSlot?.Pin();
        source = lease is null ? 0 : DoubleBufferedBitmapExports.GetBitmapSourceNoAddRef(lease.Pointer, _useBackBuffer);
        return lease;
    }
    private bool _useBackBuffer;

    internal bool HasContent => _bitmap != 0;

    internal nint AcquireBitmapSource()
    {
        using FrozenBitmapLease? lease = PinBitmapSource(out nint source);
        Direct3D9Factory.AddRef(source);
        return source;
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int SetEvent(nint handle);
    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int CloseHandle(nint handle);

    internal static void Complete(nint handle)
    {
        SetEvent(handle);
        CloseHandle(handle);
    }

    internal int ProcessCommand(MilCommand command, ReadOnlySpan<byte> packet)
    {
        if (command == MilCommand.DoubleBufferedBitmap && packet.Length == Marshal.SizeOf<MilDoubleBufferedBitmapCommand>())
        {
            var update = MemoryMarshal.Read<MilDoubleBufferedBitmapCommand>(packet);
            if (update.Bitmap == 0) return Direct3D9Factory.InvalidArgumentHResult;
            var next = new FrozenComSlot((nint)update.Bitmap);
            FrozenComSlot? previous = _sourceSlot;
            _sourceSlot = next;
            _useBackBuffer = update.UseBackBuffer != 0;
            previous?.Retire();
            NotifyChanged();
            return 0;
        }
        if (command == MilCommand.DoubleBufferedBitmapCopyForward && packet.Length == Marshal.SizeOf<MilDoubleBufferedBitmapCopyCommand>())
        {
            var copy = MemoryMarshal.Read<MilDoubleBufferedBitmapCopyCommand>(packet);
            try
            {
                if (_bitmap == 0) return Direct3D9Factory.NotInitializedHResult;
                if (_useBackBuffer) return unchecked((int)0x80004005);
                int result = DoubleBufferedBitmapExports.CopyForward(_bitmap);
                if (result >= 0) NotifyChanged();
                return result;
            }
            finally { Complete((nint)copy.Event); }
        }
        return Direct3D9Factory.UceMalformedPacketHResult;
    }

    protected override void OnFinalRelease()
    {
        FrozenComSlot? previous = _sourceSlot;
        _sourceSlot = null;
        previous?.Retire();
    }
}
