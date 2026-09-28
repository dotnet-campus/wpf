using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct MilBitmapSourceCommand
{
    internal readonly MilCommand Type;
    internal readonly uint Handle;
    internal readonly nint Bitmap;

    internal MilBitmapSourceCommand(uint handle, nint bitmap)
    {
        Type = MilCommand.BitmapSource;
        Handle = handle;
        Bitmap = bitmap;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct MilBitmapInvalidateCommand
{
    internal readonly MilCommand Type;
    internal readonly uint Handle;
    internal readonly int UseDirtyRect;
    internal readonly MilRectL DirtyRect;

    internal MilBitmapInvalidateCommand(uint handle, int useDirtyRect, MilRectL dirtyRect)
    {
        Type = MilCommand.BitmapInvalidate;
        Handle = handle;
        UseDirtyRect = useDirtyRect;
        DirtyRect = dirtyRect;
    }
}

internal static partial class GeneratedProtocolPacketWriter
{
    // The caller supplies one transport-owned reference; serialization itself does not AddRef.
    internal static byte[] WriteBitmapSource(uint handle, nint bitmap)
        => WriteDrawing(new MilBitmapSourceCommand(handle, bitmap));

    internal static byte[] WriteBitmapInvalidate(uint handle, int useDirtyRect = 0, MilRectL dirtyRect = default)
        => WriteDrawing(new MilBitmapInvalidateCommand(handle, useDirtyRect, dirtyRect));
}

internal sealed unsafe class GeneratedBitmapSourceResource : GeneratedProtocolResource
{
    private nint _bitmap;

    internal GeneratedBitmapSourceResource() : base(MilResourceType.BitmapSource) { }

    internal nint Bitmap => _bitmap;

    internal int ProcessCommand(MilCommand command, ReadOnlySpan<byte> packet)
    {
        if (IsReleased)
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        if (command == MilCommand.BitmapSource && packet.Length == Marshal.SizeOf<MilBitmapSourceCommand>())
        {
            return ProcessSource(MemoryMarshal.Read<MilBitmapSourceCommand>(packet).Bitmap);
        }

        if (command == MilCommand.BitmapInvalidate && packet.Length == Marshal.SizeOf<MilBitmapInvalidateCommand>())
        {
            MilBitmapInvalidateCommand value = MemoryMarshal.Read<MilBitmapInvalidateCommand>(packet);
            int result = Direct3D9Factory.SuccessHResult;
            if (_bitmap != 0)
            {
                MilRectL rectangle = value.DirtyRect;
                void** vtable = *(void***)_bitmap;
                var addDirtyRect = (delegate* unmanaged[Stdcall]<nint, MilRectL*, int>)vtable[11];
                result = addDirtyRect(_bitmap, value.UseDirtyRect != 0 ? &rectangle : null);
            }

            NotifyChanged();
            return result;
        }

        return Direct3D9Factory.UceMalformedPacketHResult;
    }

    private int ProcessSource(nint transportSource)
    {
        nint bitmap = 0;
        try
        {
            if (transportSource == 0)
            {
                return unchecked((int)0x80070006); // Native IFCNULL uses E_HANDLE.
            }

            // Native code downcasts CWICWrapperBitmap. QI avoids assuming its C++ base offsets.
            Guid bitmapId = new("C46D6FDE-0E59-4CFD-89B1-C935906DFBD9");
            void** vtable = *(void***)transportSource;
            var queryInterface = (delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)vtable[0];
            int result = queryInterface(transportSource, &bitmapId, &bitmap);
            if (result < 0)
            {
                return result;
            }

            if (bitmap == 0)
            {
                return Direct3D9Factory.NoInterfaceHResult;
            }

            nint previous = _bitmap;
            _bitmap = bitmap;
            bitmap = 0;
            Direct3D9Factory.Release(previous);
            return Direct3D9Factory.SuccessHResult;
        }
        finally
        {
            Direct3D9Factory.Release(bitmap);
            NotifyChanged();
            Direct3D9Factory.Release(transportSource);
        }
    }

    protected override void OnFinalRelease()
    {
        nint bitmap = _bitmap;
        _bitmap = 0;
        Direct3D9Factory.Release(bitmap);
    }
}
