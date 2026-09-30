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
    private nint _bitmap;
    private bool _useBackBuffer;

    internal nint AcquireBitmapSource() => _bitmap == 0 ? 0 : DoubleBufferedBitmapExports.AcquireBitmapSource(_bitmap, _useBackBuffer);

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
            nint previous = _bitmap;
            _bitmap = (nint)update.Bitmap;
            _useBackBuffer = update.UseBackBuffer != 0;
            Direct3D9Factory.Release(previous);
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
        nint bitmap = _bitmap;
        _bitmap = 0;
        Direct3D9Factory.Release(bitmap);
    }
}
