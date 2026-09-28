using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct MilMediaPlayerCommand
{
    internal readonly MilCommand Type;
    internal readonly uint Handle;
    internal readonly ulong Media;
    internal readonly int NotifyUceDirect;

    internal MilMediaPlayerCommand(uint handle, ulong media, int notifyUceDirect)
    {
        Type = MilCommand.MediaPlayer;
        Handle = handle;
        Media = media;
        NotifyUceDirect = notifyUceDirect;
    }
}

internal static partial class GeneratedProtocolPacketWriter
{
    // Serialization does not AddRef; the caller supplies one transport-owned reference.
    internal static byte[] WriteMediaPlayer(uint handle, ulong media, int notifyUceDirect)
        => WriteDrawing(new MilMediaPlayerCommand(handle, media, notifyUceDirect));
}

internal sealed unsafe class GeneratedMediaPlayerResource : GeneratedProtocolResource
{
    internal GeneratedMediaPlayerResource() : base(MilResourceType.MediaPlayer) { }

    internal bool NotifyUceDirect { get; private set; }

    internal int ProcessCommand(ReadOnlySpan<byte> packet)
    {
        if (IsReleased || packet.Length != Marshal.SizeOf<MilMediaPlayerCommand>())
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        MilMediaPlayerCommand value = MemoryMarshal.Read<MilMediaPlayerCommand>(packet);
        if (value.Type != MilCommand.MediaPlayer || (IntPtr.Size == 4 && value.Media > uint.MaxValue))
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        NotifyUceDirect = value.NotifyUceDirect != 0;
        nint media = unchecked((nint)value.Media);
        nint provider = 0;
        try
        {
            if (media == 0)
            {
                return unchecked((int)0x80070006); // Native IFCNULL returns E_HANDLE.
            }

            Guid providerId = new("E6F1CC74-A0EB-4EBE-8241-089D1CB079D7");
            void** vtable = *(void***)media;
            var queryInterface = (delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)vtable[0];
            int result = queryInterface(media, &providerId, &provider);
            if (result < 0)
            {
                return result;
            }

            if (provider == 0)
            {
                return Direct3D9Factory.NoInterfaceHResult;
            }

            // RegisterResource requires a real CMilSlaveVideo*, whose nonvirtual C++
            // callbacks cannot accept a managed object or a GCHandle. Fail closed
            // until the AV/composition bridge exists; never pretend registration succeeded.
            return unchecked((int)0x80004001);
        }
        finally
        {
            Direct3D9Factory.Release(media);
            Direct3D9Factory.Release(provider);
        }
    }
}
