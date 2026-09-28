using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal enum MilPenCap : uint { Flat, Square, Round, Triangle }
internal enum MilPenJoin : uint { Miter, Bevel, Round }
internal enum MilEdgeMode : uint { Unspecified, Aliased }
internal enum MilBitmapScalingMode : uint { Unspecified, LowQuality, HighQuality, NearestNeighbor }
internal enum MilClearTypeHint : uint { Auto, Enabled }

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
internal readonly struct MilDashStyleCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double Offset; [FieldOffset(16)] internal readonly uint OffsetAnimation; [FieldOffset(20)] internal readonly uint DashesSize;
    public uint Handle => _handle;
    internal MilDashStyleCommand(uint handle, double offset, uint offsetAnimation, uint dashesSize) { Type = MilCommand.DashStyle; _handle = handle; Offset = offset; OffsetAnimation = offsetAnimation; DashesSize = dashesSize; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 52)]
internal readonly struct MilPenCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double Thickness; [FieldOffset(16)] internal readonly double MiterLimit;
    [FieldOffset(24)] internal readonly uint Brush; [FieldOffset(28)] internal readonly uint ThicknessAnimation;
    [FieldOffset(32)] internal readonly MilPenCap StartLineCap; [FieldOffset(36)] internal readonly MilPenCap EndLineCap; [FieldOffset(40)] internal readonly MilPenCap DashCap; [FieldOffset(44)] internal readonly MilPenJoin LineJoin; [FieldOffset(48)] internal readonly uint DashStyle;
    public uint Handle => _handle;
    internal MilPenCommand(uint handle, double thickness, double miterLimit, uint brush, uint thicknessAnimation, MilPenCap start, MilPenCap end, MilPenCap dash, MilPenJoin join, uint dashStyle)
    { Type = MilCommand.Pen; _handle = handle; Thickness = thickness; MiterLimit = miterLimit; Brush = brush; ThicknessAnimation = thicknessAnimation; StartLineCap = start; EndLineCap = end; DashCap = dash; LineJoin = join; DashStyle = dashStyle; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 20)]
internal readonly struct MilGeometryDrawingCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly uint Brush; [FieldOffset(12)] internal readonly uint Pen; [FieldOffset(16)] internal readonly uint Geometry;
    public uint Handle => _handle; internal MilGeometryDrawingCommand(uint handle, uint brush, uint pen, uint geometry) { Type = MilCommand.GeometryDrawing; _handle = handle; Brush = brush; Pen = pen; Geometry = geometry; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16)]
internal readonly struct MilGlyphRunDrawingCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly uint GlyphRun; [FieldOffset(12)] internal readonly uint ForegroundBrush;
    public uint Handle => _handle; internal MilGlyphRunDrawingCommand(uint handle, uint glyphRun, uint brush) { Type = MilCommand.GlyphRunDrawing; _handle = handle; GlyphRun = glyphRun; ForegroundBrush = brush; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 48)]
internal readonly struct MilImageDrawingCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly MilRectD Rect; [FieldOffset(40)] internal readonly uint ImageSource; [FieldOffset(44)] internal readonly uint RectAnimation;
    public uint Handle => _handle; internal MilImageDrawingCommand(uint handle, MilRectD rect, uint image, uint animation) { Type = MilCommand.ImageDrawing; _handle = handle; Rect = rect; ImageSource = image; RectAnimation = animation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 48)]
internal readonly struct MilVideoDrawingCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly MilRectD Rect; [FieldOffset(40)] internal readonly uint Player; [FieldOffset(44)] internal readonly uint RectAnimation;
    public uint Handle => _handle; internal MilVideoDrawingCommand(uint handle, MilRectD rect, uint player, uint animation) { Type = MilCommand.VideoDrawing; _handle = handle; Rect = rect; Player = player; RectAnimation = animation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 52)]
internal readonly struct MilDrawingGroupCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly double Opacity; [FieldOffset(16)] internal readonly uint ChildrenSize;
    [FieldOffset(20)] internal readonly uint ClipGeometry; [FieldOffset(24)] internal readonly uint OpacityAnimation; [FieldOffset(28)] internal readonly uint OpacityMask; [FieldOffset(32)] internal readonly uint Transform; [FieldOffset(36)] internal readonly uint GuidelineSet;
    [FieldOffset(40)] internal readonly MilEdgeMode EdgeMode; [FieldOffset(44)] internal readonly MilBitmapScalingMode BitmapScalingMode; [FieldOffset(48)] internal readonly MilClearTypeHint ClearTypeHint;
    public uint Handle => _handle;
    internal MilDrawingGroupCommand(uint handle, double opacity, uint childrenSize, uint clip, uint opacityAnimation, uint opacityMask, uint transform, uint guidelineSet, MilEdgeMode edge, MilBitmapScalingMode scaling, MilClearTypeHint clearType)
    { Type = MilCommand.DrawingGroup; _handle = handle; Opacity = opacity; ChildrenSize = childrenSize; ClipGeometry = clip; OpacityAnimation = opacityAnimation; OpacityMask = opacityMask; Transform = transform; GuidelineSet = guidelineSet; EdgeMode = edge; BitmapScalingMode = scaling; ClearTypeHint = clearType; }
}

internal static partial class GeneratedProtocolPacketWriter
{
    internal static byte[] WriteDashStyle(uint handle, double offset, ReadOnlySpan<double> dashes, uint offsetAnimation = 0)
    {
        byte[] packet = new byte[Marshal.SizeOf<MilDashStyleCommand>() + dashes.Length * sizeof(double)]; MilDashStyleCommand command = new(handle, offset, offsetAnimation, checked((uint)(dashes.Length * sizeof(double)))); MemoryMarshal.Write(packet, in command); dashes.CopyTo(MemoryMarshal.Cast<byte, double>(packet.AsSpan(Marshal.SizeOf<MilDashStyleCommand>()))); return packet;
    }
    internal static byte[] WritePen(uint handle, double thickness, double miterLimit, uint brush = 0, uint thicknessAnimation = 0, MilPenCap start = MilPenCap.Flat, MilPenCap end = MilPenCap.Flat, MilPenCap dash = MilPenCap.Square, MilPenJoin join = MilPenJoin.Miter, uint dashStyle = 0) => WriteDrawing(new MilPenCommand(handle, thickness, miterLimit, brush, thicknessAnimation, start, end, dash, join, dashStyle));
    internal static byte[] WriteGeometryDrawing(uint handle, uint brush = 0, uint pen = 0, uint geometry = 0) => WriteDrawing(new MilGeometryDrawingCommand(handle, brush, pen, geometry));
    internal static byte[] WriteGlyphRunDrawing(uint handle, uint glyphRun = 0, uint foregroundBrush = 0) => WriteDrawing(new MilGlyphRunDrawingCommand(handle, glyphRun, foregroundBrush));
    internal static byte[] WriteImageDrawing(uint handle, MilRectD rect, uint imageSource = 0, uint rectAnimation = 0) => WriteDrawing(new MilImageDrawingCommand(handle, rect, imageSource, rectAnimation));
    internal static byte[] WriteVideoDrawing(uint handle, MilRectD rect, uint player = 0, uint rectAnimation = 0) => WriteDrawing(new MilVideoDrawingCommand(handle, rect, player, rectAnimation));
    internal static byte[] WriteDrawingGroup(uint handle, double opacity, ReadOnlySpan<uint> children, uint clip = 0, uint opacityAnimation = 0, uint opacityMask = 0, uint transform = 0, uint guidelineSet = 0, MilEdgeMode edge = MilEdgeMode.Unspecified, MilBitmapScalingMode scaling = MilBitmapScalingMode.Unspecified, MilClearTypeHint clearType = MilClearTypeHint.Auto)
    {
        int payloadSize = checked(children.Length * sizeof(uint)); byte[] packet = new byte[Marshal.SizeOf<MilDrawingGroupCommand>() + payloadSize]; MilDrawingGroupCommand command = new(handle, opacity, (uint)payloadSize, clip, opacityAnimation, opacityMask, transform, guidelineSet, edge, scaling, clearType); MemoryMarshal.Write(packet, in command); children.CopyTo(MemoryMarshal.Cast<byte, uint>(packet.AsSpan(Marshal.SizeOf<MilDrawingGroupCommand>()))); return packet;
    }
    private static byte[] WriteDrawing<T>(T command) where T : unmanaged { byte[] packet = new byte[Marshal.SizeOf<T>()]; MemoryMarshal.Write(packet, in command); return packet; }
}

internal abstract class GeneratedDrawingDependencyResource : GeneratedDependencyResource
{
    protected GeneratedDrawingDependencyResource(MilResourceType type) : base(type) { }
    protected static bool IsBrush(MilResourceType type) => GeneratedEffectResource.IsBrush(type);
    protected static bool IsGeometry(MilResourceType type) => type is >= MilResourceType.LineGeometry and <= MilResourceType.PathGeometry;
    protected static bool IsTransform(MilResourceType type) => type is >= MilResourceType.TransformGroup and <= MilResourceType.MatrixTransform;
    protected static bool IsDrawing(MilResourceType type) => type is >= MilResourceType.GeometryDrawing and <= MilResourceType.DrawingGroup;
    protected static bool IsImageSource(MilResourceType type) => type is MilResourceType.DrawingImage or MilResourceType.BitmapSource or MilResourceType.DoubleBufferedBitmap or MilResourceType.D3DImage;
    protected static bool Valid(MilPenCap value) => value <= MilPenCap.Triangle; protected static bool Valid(MilPenJoin value) => value <= MilPenJoin.Round;
    protected static bool Valid(MilEdgeMode value) => value <= MilEdgeMode.Aliased; protected static bool Valid(MilBitmapScalingMode value) => value <= MilBitmapScalingMode.NearestNeighbor; protected static bool Valid(MilClearTypeHint value) => value <= MilClearTypeHint.Enabled;
}

internal sealed class GeneratedDashStyleResource : GeneratedDrawingDependencyResource
{
    internal GeneratedDashStyleResource() : base(MilResourceType.DashStyle) { } internal double Offset { get; private set; } internal double[] Dashes { get; private set; } = [];
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length < 16) return Direct3D9Factory.UceMalformedPacketHResult; double offset = MemoryMarshal.Read<double>(value); uint size = MemoryMarshal.Read<uint>(value[12..]); ReadOnlySpan<byte> payload = value[16..];
        if (size != payload.Length || payload.Length % sizeof(double) != 0 || !TryResolve(table, MemoryMarshal.Read<uint>(value[8..]), MilResourceType.DoubleResource, out GeneratedProtocolResource? animation)) return Direct3D9Factory.UceMalformedPacketHResult;
        double[] dashes = MemoryMarshal.Cast<byte, double>(payload).ToArray(); GeneratedProtocolResource?[] dependencies = [animation]; return CommitDependencies(dependencies, () => { Offset = offset; Dashes = dashes; });
    }
}

internal sealed class GeneratedPenResource : GeneratedDrawingDependencyResource
{
    internal GeneratedPenResource() : base(MilResourceType.Pen) { } internal MilPenCap StartLineCap { get; private set; } internal MilPenJoin LineJoin { get; private set; }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length != 44) return Direct3D9Factory.UceMalformedPacketHResult; MilPenCap start = MemoryMarshal.Read<MilPenCap>(value[24..]); MilPenCap end = MemoryMarshal.Read<MilPenCap>(value[28..]); MilPenCap dash = MemoryMarshal.Read<MilPenCap>(value[32..]); MilPenJoin join = MemoryMarshal.Read<MilPenJoin>(value[36..]); GeneratedProtocolResource?[] dependencies = new GeneratedProtocolResource?[3];
        if (!Valid(start) || !Valid(end) || !Valid(dash) || !Valid(join) || !TryResolve(table, MemoryMarshal.Read<uint>(value[16..]), IsBrush, out dependencies[0]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[20..]), MilResourceType.DoubleResource, out dependencies[1]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[40..]), MilResourceType.DashStyle, out dependencies[2])) return Direct3D9Factory.UceMalformedPacketHResult;
        return CommitDependencies(dependencies, () => { StartLineCap = start; LineJoin = join; });
    }
}

internal sealed class GeneratedGeometryDrawingResource : GeneratedDrawingDependencyResource
{
    internal GeneratedGeometryDrawingResource() : base(MilResourceType.GeometryDrawing) { }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value) => ProcessThree(table, value, IsBrush, static type => type == MilResourceType.Pen, IsGeometry);
    private int ProcessThree(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value, Func<MilResourceType, bool> first, Func<MilResourceType, bool> second, Func<MilResourceType, bool> third)
    { if (value.Length != 12) return Direct3D9Factory.UceMalformedPacketHResult; GeneratedProtocolResource?[] d = new GeneratedProtocolResource?[3]; if (!TryResolve(table, MemoryMarshal.Read<uint>(value), first, out d[0]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[4..]), second, out d[1]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[8..]), third, out d[2])) return Direct3D9Factory.UceMalformedPacketHResult; return CommitDependencies(d, static () => { }); }
}

internal sealed class GeneratedGlyphRunDrawingResource : GeneratedDrawingDependencyResource
{
    internal GeneratedGlyphRunDrawingResource() : base(MilResourceType.GlyphRunDrawing) { }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    { if (value.Length != 8) return Direct3D9Factory.UceMalformedPacketHResult; GeneratedProtocolResource?[] d = new GeneratedProtocolResource?[2]; if (!TryResolve(table, MemoryMarshal.Read<uint>(value), MilResourceType.GlyphRun, out d[0]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[4..]), IsBrush, out d[1])) return Direct3D9Factory.UceMalformedPacketHResult; return CommitDependencies(d, static () => { }); }
}

internal sealed class GeneratedImageDrawingResource : GeneratedDrawingDependencyResource
{
    internal GeneratedImageDrawingResource() : base(MilResourceType.ImageDrawing) { } internal MilRectD Rect { get; private set; }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    { if (value.Length != 40) return Direct3D9Factory.UceMalformedPacketHResult; MilRectD rect = MemoryMarshal.Read<MilRectD>(value); GeneratedProtocolResource?[] d = new GeneratedProtocolResource?[2]; if (!TryResolve(table, MemoryMarshal.Read<uint>(value[32..]), IsImageSource, out d[0]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[36..]), MilResourceType.RectResource, out d[1])) return Direct3D9Factory.UceMalformedPacketHResult; return CommitDependencies(d, () => Rect = rect); }
}

internal sealed class GeneratedVideoDrawingResource : GeneratedDrawingDependencyResource
{
    internal GeneratedVideoDrawingResource() : base(MilResourceType.VideoDrawing) { }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    { if (value.Length != 40) return Direct3D9Factory.UceMalformedPacketHResult; GeneratedProtocolResource?[] d = new GeneratedProtocolResource?[2]; if (!TryResolve(table, MemoryMarshal.Read<uint>(value[32..]), MilResourceType.MediaPlayer, out d[0]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[36..]), MilResourceType.RectResource, out d[1])) return Direct3D9Factory.UceMalformedPacketHResult; return CommitDependencies(d, static () => { }); }
}

internal sealed class GeneratedDrawingGroupResource : GeneratedDrawingDependencyResource
{
    internal GeneratedDrawingGroupResource() : base(MilResourceType.DrawingGroup) { } internal IReadOnlyList<GeneratedProtocolResource> Children { get; private set; } = []; internal MilEdgeMode EdgeMode { get; private set; }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length < 44) return Direct3D9Factory.UceMalformedPacketHResult; uint size = MemoryMarshal.Read<uint>(value[8..]); MilEdgeMode edge = MemoryMarshal.Read<MilEdgeMode>(value[32..]); MilBitmapScalingMode scaling = MemoryMarshal.Read<MilBitmapScalingMode>(value[36..]); MilClearTypeHint clearType = MemoryMarshal.Read<MilClearTypeHint>(value[40..]); ReadOnlySpan<byte> payload = value[44..];
        if (size != payload.Length || size % sizeof(uint) != 0 || !Valid(edge) || !Valid(scaling) || !Valid(clearType)) return Direct3D9Factory.UceMalformedPacketHResult; ReadOnlySpan<uint> handles = MemoryMarshal.Cast<byte, uint>(payload); GeneratedProtocolResource?[] d = new GeneratedProtocolResource?[handles.Length + 5];
        if (!TryResolve(table, MemoryMarshal.Read<uint>(value[12..]), IsGeometry, out d[0]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[16..]), MilResourceType.DoubleResource, out d[1]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[20..]), IsBrush, out d[2]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[24..]), IsTransform, out d[3]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[28..]), MilResourceType.GuidelineSet, out d[4])) return Direct3D9Factory.UceMalformedPacketHResult;
        for (int i = 0; i < handles.Length; i++) { if (handles[i] == 0 || !TryResolve(table, handles[i], IsDrawing, out d[i + 5])) return Direct3D9Factory.UceMalformedPacketHResult; }
        return CommitDependencies(d, () => { EdgeMode = edge; Children = d[5..].Cast<GeneratedProtocolResource>().ToArray(); });
    }
}
