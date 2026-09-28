using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
internal readonly struct MilDrawingImageCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly uint Drawing;
    public uint Handle => _handle;

    internal MilDrawingImageCommand(uint handle, uint drawing)
    {
        Type = MilCommand.DrawingImage;
        _handle = handle;
        Drawing = drawing;
    }
}

internal static partial class GeneratedProtocolPacketWriter
{
    internal static byte[] WriteDrawingImage(uint handle, uint drawing = 0)
        => WriteDrawing(new MilDrawingImageCommand(handle, drawing));
}

internal sealed class GeneratedDrawingImageResource : GeneratedDrawingDependencyResource
{
    internal GeneratedDrawingImageResource() : base(MilResourceType.DrawingImage) { }

    internal GeneratedProtocolResource? Drawing { get; private set; }

    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length != sizeof(uint)
            || !TryResolve(table, MemoryMarshal.Read<uint>(value), IsDrawing, out GeneratedProtocolResource? drawing))
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        return CommitDependencies([drawing], () => Drawing = drawing);
    }

    protected override void OnFinalRelease()
    {
        Drawing = null;
        base.OnFinalRelease();
    }
}
