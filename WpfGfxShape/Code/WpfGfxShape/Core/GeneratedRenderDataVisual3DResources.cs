using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
internal readonly struct MilRenderDataCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly uint DataSize;
    public uint Handle => _handle;
    internal MilRenderDataCommand(uint handle, uint dataSize) { Type = MilCommand.RenderData; _handle = handle; DataSize = dataSize; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 40)]
internal readonly struct MilDrawLineData
{
    [FieldOffset(0)] internal readonly MilPoint2D Point0; [FieldOffset(16)] internal readonly MilPoint2D Point1; [FieldOffset(32)] internal readonly uint Pen;
    internal MilDrawLineData(MilPoint2D point0, MilPoint2D point1, uint pen) { Point0 = point0; Point1 = point1; Pen = pen; }
}
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 48)]
internal readonly struct MilDrawLineAnimateData
{
    [FieldOffset(0)] internal readonly MilPoint2D Point0; [FieldOffset(16)] internal readonly MilPoint2D Point1; [FieldOffset(32)] internal readonly uint Pen; [FieldOffset(36)] internal readonly uint Point0Animation; [FieldOffset(40)] internal readonly uint Point1Animation;
    internal MilDrawLineAnimateData(MilPoint2D point0, MilPoint2D point1, uint pen, uint point0Animation, uint point1Animation) { Point0 = point0; Point1 = point1; Pen = pen; Point0Animation = point0Animation; Point1Animation = point1Animation; }
}
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 40)]
internal readonly struct MilDrawRectangleData
{
    [FieldOffset(0)] internal readonly MilRectD Rectangle; [FieldOffset(32)] internal readonly uint Brush; [FieldOffset(36)] internal readonly uint Pen;
    internal MilDrawRectangleData(MilRectD rectangle, uint brush, uint pen) { Rectangle = rectangle; Brush = brush; Pen = pen; }
}
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 48)]
internal readonly struct MilDrawRectangleAnimateData
{
    [FieldOffset(0)] internal readonly MilRectD Rectangle; [FieldOffset(32)] internal readonly uint Brush; [FieldOffset(36)] internal readonly uint Pen; [FieldOffset(40)] internal readonly uint RectangleAnimation;
    internal MilDrawRectangleAnimateData(MilRectD rectangle, uint brush, uint pen, uint animation) { Rectangle = rectangle; Brush = brush; Pen = pen; RectangleAnimation = animation; }
}
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 56)]
internal readonly struct MilDrawRoundedRectangleData
{
    [FieldOffset(0)] internal readonly MilRectD Rectangle; [FieldOffset(32)] internal readonly double RadiusX; [FieldOffset(40)] internal readonly double RadiusY; [FieldOffset(48)] internal readonly uint Brush; [FieldOffset(52)] internal readonly uint Pen;
    internal MilDrawRoundedRectangleData(MilRectD rectangle, double radiusX, double radiusY, uint brush, uint pen) { Rectangle = rectangle; RadiusX = radiusX; RadiusY = radiusY; Brush = brush; Pen = pen; }
}
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 72)]
internal readonly struct MilDrawRoundedRectangleAnimateData
{
    [FieldOffset(0)] internal readonly MilRectD Rectangle; [FieldOffset(32)] internal readonly double RadiusX; [FieldOffset(40)] internal readonly double RadiusY; [FieldOffset(48)] internal readonly uint Brush; [FieldOffset(52)] internal readonly uint Pen; [FieldOffset(56)] internal readonly uint RectangleAnimation; [FieldOffset(60)] internal readonly uint RadiusXAnimation; [FieldOffset(64)] internal readonly uint RadiusYAnimation;
    internal MilDrawRoundedRectangleAnimateData(MilRectD rectangle, double radiusX, double radiusY, uint brush, uint pen, uint rectangleAnimation, uint radiusXAnimation, uint radiusYAnimation) { Rectangle = rectangle; RadiusX = radiusX; RadiusY = radiusY; Brush = brush; Pen = pen; RectangleAnimation = rectangleAnimation; RadiusXAnimation = radiusXAnimation; RadiusYAnimation = radiusYAnimation; }
}
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 40)]
internal readonly struct MilDrawEllipseData
{
    [FieldOffset(0)] internal readonly MilPoint2D Center; [FieldOffset(16)] internal readonly double RadiusX; [FieldOffset(24)] internal readonly double RadiusY; [FieldOffset(32)] internal readonly uint Brush; [FieldOffset(36)] internal readonly uint Pen;
    internal MilDrawEllipseData(MilPoint2D center, double radiusX, double radiusY, uint brush, uint pen) { Center = center; RadiusX = radiusX; RadiusY = radiusY; Brush = brush; Pen = pen; }
}
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 56)]
internal readonly struct MilDrawEllipseAnimateData
{
    [FieldOffset(0)] internal readonly MilPoint2D Center; [FieldOffset(16)] internal readonly double RadiusX; [FieldOffset(24)] internal readonly double RadiusY; [FieldOffset(32)] internal readonly uint Brush; [FieldOffset(36)] internal readonly uint Pen; [FieldOffset(40)] internal readonly uint CenterAnimation; [FieldOffset(44)] internal readonly uint RadiusXAnimation; [FieldOffset(48)] internal readonly uint RadiusYAnimation;
    internal MilDrawEllipseAnimateData(MilPoint2D center, double radiusX, double radiusY, uint brush, uint pen, uint centerAnimation, uint radiusXAnimation, uint radiusYAnimation) { Center = center; RadiusX = radiusX; RadiusY = radiusY; Brush = brush; Pen = pen; CenterAnimation = centerAnimation; RadiusXAnimation = radiusXAnimation; RadiusYAnimation = radiusYAnimation; }
}
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16)]
internal readonly struct MilDrawGeometryData
{
    [FieldOffset(0)] internal readonly uint Brush; [FieldOffset(4)] internal readonly uint Pen; [FieldOffset(8)] internal readonly uint Geometry;
    internal MilDrawGeometryData(uint brush, uint pen, uint geometry) { Brush = brush; Pen = pen; Geometry = geometry; }
}
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 40)]
internal readonly struct MilDrawImageData
{
    [FieldOffset(0)] internal readonly MilRectD Rectangle; [FieldOffset(32)] internal readonly uint ImageSource;
    internal MilDrawImageData(MilRectD rectangle, uint image) { Rectangle = rectangle; ImageSource = image; }
}
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 40)]
internal readonly struct MilDrawImageAnimateData
{
    [FieldOffset(0)] internal readonly MilRectD Rectangle; [FieldOffset(32)] internal readonly uint Source; [FieldOffset(36)] internal readonly uint RectangleAnimation;
    internal MilDrawImageAnimateData(MilRectD rectangle, uint source, uint animation) { Rectangle = rectangle; Source = source; RectangleAnimation = animation; }
}
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
internal readonly struct MilTwoHandleRenderData { [FieldOffset(0)] internal readonly uint First; [FieldOffset(4)] internal readonly uint Second; internal MilTwoHandleRenderData(uint first, uint second) { First = first; Second = second; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
internal readonly struct MilPushResourceData { [FieldOffset(0)] internal readonly uint Resource; internal MilPushResourceData(uint resource) { Resource = resource; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
internal readonly struct MilPushOpacityMaskData { [FieldOffset(0)] internal readonly MilRectF Bounds; [FieldOffset(16)] internal readonly uint Brush; internal MilPushOpacityMaskData(MilRectF bounds, uint brush) { Bounds = bounds; Brush = brush; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
internal readonly struct MilPushOpacityData { [FieldOffset(0)] internal readonly double Opacity; internal MilPushOpacityData(double opacity) { Opacity = opacity; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
internal readonly struct MilPushGuidelineY1Data { [FieldOffset(0)] internal readonly double Coordinate; internal MilPushGuidelineY1Data(double coordinate) { Coordinate = coordinate; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16)]
internal readonly struct MilPushGuidelineY2Data { [FieldOffset(0)] internal readonly double LeadingCoordinate; [FieldOffset(8)] internal readonly double OffsetToDrivenCoordinate; internal MilPushGuidelineY2Data(double leading, double offset) { LeadingCoordinate = leading; OffsetToDrivenCoordinate = offset; } }

internal enum GeneratedRenderDataKind
{
    DrawLine, DrawLineAnimate, DrawRectangle, DrawRectangleAnimate, DrawRoundedRectangle, DrawRoundedRectangleAnimate, DrawEllipse, DrawEllipseAnimate, DrawGeometry, DrawImage, DrawImageAnimate, DrawGlyphRun, DrawDrawing, DrawVideo, DrawVideoAnimate, PushClip, PushOpacityMask, PushOpacity, PushOpacityAnimate, PushTransform, PushGuidelineSet, PushGuidelineY1, PushGuidelineY2, Pop
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16)]
internal readonly struct MilPushOpacityAnimateData
{
    [FieldOffset(0)] internal readonly double Opacity;
    [FieldOffset(8)] internal readonly uint Animation;
    internal MilPushOpacityAnimateData(double opacity, uint animation) { Opacity = opacity; Animation = animation; }
}

internal sealed record GeneratedRenderDataInstruction(
    GeneratedRenderDataKind Kind,
    IReadOnlyList<GeneratedProtocolResource> Resources,
    ReadOnlyMemory<byte> Data,
    IReadOnlyList<GeneratedProtocolResource?> ResourceSlots);

internal static partial class GeneratedProtocolPacketWriter
{
    internal static byte[] WriteRenderData(uint handle, params byte[][] records)
    {
        int payloadSize = records.Sum(static record => record.Length); byte[] packet = new byte[Marshal.SizeOf<MilRenderDataCommand>() + payloadSize]; MilRenderDataCommand command = new(handle, (uint)payloadSize); MemoryMarshal.Write(packet, in command); int offset = Marshal.SizeOf<MilRenderDataCommand>(); foreach (byte[] record in records) { record.CopyTo(packet, offset); offset += record.Length; } return packet;
    }
    internal static byte[] WriteDrawLineRecord(MilPoint2D p0, MilPoint2D p1, uint pen) => WriteRenderRecord(MilCommand.DrawLine, new MilDrawLineData(p0, p1, pen));
    internal static byte[] WriteDrawLineAnimateRecord(MilPoint2D p0, MilPoint2D p1, uint pen, uint p0Animation, uint p1Animation) => WriteRenderRecord(MilCommand.DrawLineAnimate, new MilDrawLineAnimateData(p0, p1, pen, p0Animation, p1Animation));
    internal static byte[] WriteDrawRectangleRecord(MilRectD rectangle, uint brush, uint pen) => WriteRenderRecord(MilCommand.DrawRectangle, new MilDrawRectangleData(rectangle, brush, pen));
    internal static byte[] WriteDrawRectangleAnimateRecord(MilRectD rectangle, uint brush, uint pen, uint animation) => WriteRenderRecord(MilCommand.DrawRectangleAnimate, new MilDrawRectangleAnimateData(rectangle, brush, pen, animation));
    internal static byte[] WriteDrawRoundedRectangleRecord(MilRectD rectangle, double radiusX, double radiusY, uint brush, uint pen) => WriteRenderRecord(MilCommand.DrawRoundedRectangle, new MilDrawRoundedRectangleData(rectangle, radiusX, radiusY, brush, pen));
    internal static byte[] WriteDrawRoundedRectangleAnimateRecord(MilRectD rectangle, double radiusX, double radiusY, uint brush, uint pen, uint rectangleAnimation, uint radiusXAnimation, uint radiusYAnimation) => WriteRenderRecord(MilCommand.DrawRoundedRectangleAnimate, new MilDrawRoundedRectangleAnimateData(rectangle, radiusX, radiusY, brush, pen, rectangleAnimation, radiusXAnimation, radiusYAnimation));
    internal static byte[] WriteDrawEllipseRecord(MilPoint2D center, double radiusX, double radiusY, uint brush, uint pen) => WriteRenderRecord(MilCommand.DrawEllipse, new MilDrawEllipseData(center, radiusX, radiusY, brush, pen));
    internal static byte[] WriteDrawEllipseAnimateRecord(MilPoint2D center, double radiusX, double radiusY, uint brush, uint pen, uint centerAnimation, uint radiusXAnimation, uint radiusYAnimation) => WriteRenderRecord(MilCommand.DrawEllipseAnimate, new MilDrawEllipseAnimateData(center, radiusX, radiusY, brush, pen, centerAnimation, radiusXAnimation, radiusYAnimation));
    internal static byte[] WriteDrawGeometryRecord(uint brush, uint pen, uint geometry) => WriteRenderRecord(MilCommand.DrawGeometry, new MilDrawGeometryData(brush, pen, geometry));
    internal static byte[] WriteDrawImageRecord(MilRectD rectangle, uint image) => WriteRenderRecord(MilCommand.DrawImage, new MilDrawImageData(rectangle, image));
    internal static byte[] WriteDrawImageAnimateRecord(MilRectD rectangle, uint image, uint animation) => WriteRenderRecord(MilCommand.DrawImageAnimate, new MilDrawImageAnimateData(rectangle, image, animation));
    internal static byte[] WriteDrawGlyphRunRecord(uint brush, uint glyphRun) => WriteRenderRecord(MilCommand.DrawGlyphRun, new MilTwoHandleRenderData(brush, glyphRun));
    internal static byte[] WriteDrawDrawingRecord(uint drawing) => WriteRenderRecord(MilCommand.DrawDrawing, new MilPushResourceData(drawing));
    internal static byte[] WriteDrawVideoRecord(MilRectD rectangle, uint player) => WriteRenderRecord(MilCommand.DrawVideo, new MilDrawImageData(rectangle, player));
    internal static byte[] WriteDrawVideoAnimateRecord(MilRectD rectangle, uint player, uint animation) => WriteRenderRecord(MilCommand.DrawVideoAnimate, new MilDrawImageAnimateData(rectangle, player, animation));
    internal static byte[] WritePushClipRecord(uint geometry) => WriteRenderRecord(MilCommand.PushClip, new MilPushResourceData(geometry));
    internal static byte[] WritePushOpacityMaskRecord(MilRectF bounds, uint brush) => WriteRenderRecord(MilCommand.PushOpacityMask, new MilPushOpacityMaskData(bounds, brush));
    internal static byte[] WritePushOpacityRecord(double opacity) => WriteRenderRecord(MilCommand.PushOpacity, new MilPushOpacityData(opacity));
    internal static byte[] WritePushOpacityAnimateRecord(double opacity, uint animation) => WriteRenderRecord(MilCommand.PushOpacityAnimate, new MilPushOpacityAnimateData(opacity, animation));
    internal static byte[] WritePushTransformRecord(uint transform) => WriteRenderRecord(MilCommand.PushTransform, new MilPushResourceData(transform));
    internal static byte[] WritePushGuidelineSetRecord(uint guidelines) => WriteRenderRecord(MilCommand.PushGuidelineSet, new MilPushResourceData(guidelines));
    internal static byte[] WritePushGuidelineY1Record(double coordinate) => WriteRenderRecord(MilCommand.PushGuidelineY1, new MilPushGuidelineY1Data(coordinate));
    internal static byte[] WritePushGuidelineY2Record(double leading, double offset) => WriteRenderRecord(MilCommand.PushGuidelineY2, new MilPushGuidelineY2Data(leading, offset));
    internal static byte[] WritePopRecord() { byte[] record = new byte[8]; BinaryPrimitives.WriteInt32LittleEndian(record, 8); BinaryPrimitives.WriteUInt32LittleEndian(record.AsSpan(4), (uint)MilCommand.Pop); return record; }
    private static byte[] WriteRenderRecord<T>(MilCommand id, T data) where T : unmanaged { int dataSize = Marshal.SizeOf<T>(); int total = checked(dataSize + 8); byte[] record = new byte[total]; BinaryPrimitives.WriteInt32LittleEndian(record, total); BinaryPrimitives.WriteUInt32LittleEndian(record.AsSpan(4), (uint)id); MemoryMarshal.Write(record.AsSpan(8), in data); return record; }
}

internal sealed class GeneratedRenderDataResource : GeneratedDependencyResource
{
    internal GeneratedRenderDataResource() : base(MilResourceType.RenderData) { }
    internal IReadOnlyList<GeneratedRenderDataInstruction> Instructions { get; private set; } = [];
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length < 4) return Direct3D9Factory.UceMalformedPacketHResult; uint declared = MemoryMarshal.Read<uint>(value); ReadOnlySpan<byte> payload = value[4..]; if (declared != payload.Length) return Direct3D9Factory.UceMalformedPacketHResult;
        List<GeneratedRenderDataInstruction> instructions = []; List<GeneratedProtocolResource?> dependencies = []; int stackDepth = 0; int offset = 0;
        while (offset < payload.Length)
        {
            if (payload.Length - offset < 8) return Direct3D9Factory.UceMalformedPacketHResult; int size = BinaryPrimitives.ReadInt32LittleEndian(payload[offset..]); MilCommand id = (MilCommand)BinaryPrimitives.ReadUInt32LittleEndian(payload[(offset + 4)..]);
            if (size < 8 || size % 8 != 0 || size > payload.Length - offset) return Direct3D9Factory.UceMalformedPacketHResult; ReadOnlySpan<byte> data = payload.Slice(offset + 8, size - 8); int result = ParseInstruction(table, id, data, instructions, dependencies, ref stackDepth); if (result < 0) return result; offset += size;
        }
        if (stackDepth != 0) return Direct3D9Factory.UceMalformedPacketHResult; GeneratedProtocolResource?[] refs = [.. dependencies]; GeneratedRenderDataInstruction[] committed = [.. instructions]; return CommitDependencies(refs, () => Instructions = committed);
    }
    private static int ParseInstruction(GeneratedProtocolHandleTable table, MilCommand id, ReadOnlySpan<byte> data, List<GeneratedRenderDataInstruction> instructions, List<GeneratedProtocolResource?> dependencies, ref int stackDepth)
    {
        GeneratedRenderDataKind kind; (int Offset, Func<MilResourceType, bool> Accepts)[] handles;
        switch (id)
        {
            case MilCommand.DrawLine when data.Length == 40: kind = GeneratedRenderDataKind.DrawLine; handles = [(32, static t => t == MilResourceType.Pen)]; break;
            case MilCommand.DrawLineAnimate when data.Length == 48: kind = GeneratedRenderDataKind.DrawLineAnimate; handles = [(32, static t => t == MilResourceType.Pen), (36, static t => t == MilResourceType.PointResource), (40, static t => t == MilResourceType.PointResource)]; break;
            case MilCommand.DrawRectangle when data.Length == 40: kind = GeneratedRenderDataKind.DrawRectangle; handles = [(32, IsBrush), (36, static t => t == MilResourceType.Pen)]; break;
            case MilCommand.DrawRectangleAnimate when data.Length == 48: kind = GeneratedRenderDataKind.DrawRectangleAnimate; handles = [(32, IsBrush), (36, static t => t == MilResourceType.Pen), (40, static t => t == MilResourceType.RectResource)]; break;
            case MilCommand.DrawRoundedRectangle when data.Length == 56: kind = GeneratedRenderDataKind.DrawRoundedRectangle; handles = [(48, IsBrush), (52, static t => t == MilResourceType.Pen)]; break;
            case MilCommand.DrawRoundedRectangleAnimate when data.Length == 72: kind = GeneratedRenderDataKind.DrawRoundedRectangleAnimate; handles = [(48, IsBrush), (52, static t => t == MilResourceType.Pen), (56, static t => t == MilResourceType.RectResource), (60, static t => t == MilResourceType.DoubleResource), (64, static t => t == MilResourceType.DoubleResource)]; break;
            case MilCommand.DrawEllipse when data.Length == 40: kind = GeneratedRenderDataKind.DrawEllipse; handles = [(32, IsBrush), (36, static t => t == MilResourceType.Pen)]; break;
            case MilCommand.DrawEllipseAnimate when data.Length == 56: kind = GeneratedRenderDataKind.DrawEllipseAnimate; handles = [(32, IsBrush), (36, static t => t == MilResourceType.Pen), (40, static t => t == MilResourceType.PointResource), (44, static t => t == MilResourceType.DoubleResource), (48, static t => t == MilResourceType.DoubleResource)]; break;
            case MilCommand.DrawGeometry when data.Length == 16: kind = GeneratedRenderDataKind.DrawGeometry; handles = [(0, IsBrush), (4, static t => t == MilResourceType.Pen), (8, IsGeometry)]; break;
            case MilCommand.DrawImage when data.Length == 40: kind = GeneratedRenderDataKind.DrawImage; handles = [(32, IsImage)]; break;
            case MilCommand.DrawImageAnimate when data.Length == 40: kind = GeneratedRenderDataKind.DrawImageAnimate; handles = [(32, IsImage), (36, static t => t == MilResourceType.RectResource)]; break;
            case MilCommand.DrawGlyphRun when data.Length == 8: kind = GeneratedRenderDataKind.DrawGlyphRun; handles = [(0, IsBrush), (4, static t => t == MilResourceType.GlyphRun)]; break;
            case MilCommand.DrawDrawing when data.Length == 8: kind = GeneratedRenderDataKind.DrawDrawing; handles = [(0, IsDrawing)]; break;
            case MilCommand.DrawVideo when data.Length == 40: kind = GeneratedRenderDataKind.DrawVideo; handles = [(32, static t => t == MilResourceType.MediaPlayer)]; break;
            case MilCommand.DrawVideoAnimate when data.Length == 40: kind = GeneratedRenderDataKind.DrawVideoAnimate; handles = [(32, static t => t == MilResourceType.MediaPlayer), (36, static t => t == MilResourceType.RectResource)]; break;
            case MilCommand.PushClip when data.Length == 8: kind = GeneratedRenderDataKind.PushClip; handles = [(0, IsGeometry)]; stackDepth++; break;
            case MilCommand.PushOpacityMask when data.Length == 24: kind = GeneratedRenderDataKind.PushOpacityMask; handles = [(16, IsBrush)]; stackDepth++; break;
            case MilCommand.PushOpacity when data.Length == 8: kind = GeneratedRenderDataKind.PushOpacity; handles = []; stackDepth++; break;
            case MilCommand.PushOpacityAnimate when data.Length == 16: kind = GeneratedRenderDataKind.PushOpacityAnimate; handles = [(8, static t => t == MilResourceType.DoubleResource)]; stackDepth++; break;
            case MilCommand.PushTransform when data.Length == 8: kind = GeneratedRenderDataKind.PushTransform; handles = [(0, IsTransform)]; stackDepth++; break;
            case MilCommand.PushGuidelineSet when data.Length == 8: kind = GeneratedRenderDataKind.PushGuidelineSet; handles = [(0, static t => t == MilResourceType.GuidelineSet)]; stackDepth++; break;
            case MilCommand.PushGuidelineY1 when data.Length == 8: kind = GeneratedRenderDataKind.PushGuidelineY1; handles = []; stackDepth++; break;
            case MilCommand.PushGuidelineY2 when data.Length == 16: kind = GeneratedRenderDataKind.PushGuidelineY2; handles = []; stackDepth++; break;
            case MilCommand.Pop when data.Length == 0 && stackDepth > 0: kind = GeneratedRenderDataKind.Pop; handles = []; stackDepth--; break;
            default: return Direct3D9Factory.UceMalformedPacketHResult;
        }
        List<GeneratedProtocolResource> resources = [];
        List<GeneratedProtocolResource?> slots = [];
        foreach ((int handleOffset, Func<MilResourceType, bool> accepts) in handles)
        {
            uint handle = MemoryMarshal.Read<uint>(data[handleOffset..]);
            if (handle == 0)
            {
                slots.Add(null);
                continue;
            }

            if (!table.TryGetResource(handle, out GeneratedProtocolResource? resource) || resource is null || !accepts(resource.ResourceType))
            {
                return Direct3D9Factory.UceMalformedPacketHResult;
            }

            slots.Add(resource);
            resources.Add(resource);
            dependencies.Add(resource);
        }

        instructions.Add(new(kind, resources, data.ToArray(), slots));
        return Direct3D9Factory.SuccessHResult;
    }
    private static bool IsBrush(MilResourceType t) => GeneratedEffectResource.IsBrush(t);
    private static bool IsGeometry(MilResourceType t) => t is >= MilResourceType.LineGeometry and <= MilResourceType.PathGeometry;
    private static bool IsTransform(MilResourceType t) => t is >= MilResourceType.TransformGroup and <= MilResourceType.MatrixTransform;
    private static bool IsDrawing(MilResourceType t) => t is >= MilResourceType.GeometryDrawing and <= MilResourceType.DrawingGroup;
    private static bool IsImage(MilResourceType t) => t is MilResourceType.DrawingImage or MilResourceType.BitmapSource or MilResourceType.DoubleBufferedBitmap or MilResourceType.D3DImage;
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
internal readonly struct MilVisual3DDependencyCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; [FieldOffset(8)] internal readonly uint Dependency; internal MilVisual3DDependencyCommand(MilCommand type, uint handle, uint dependency) { Type = type; Handle = handle; Dependency = dependency; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 40)]
internal readonly struct MilViewport3DSetViewportCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; [FieldOffset(8)] internal readonly MilRectD Viewport; internal MilViewport3DSetViewportCommand(uint handle, MilRectD viewport) { Type = MilCommand.Viewport3DVisualSetViewport; Handle = handle; Viewport = viewport; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
internal readonly struct MilVisual3DRemoveAllCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; internal MilVisual3DRemoveAllCommand(uint handle) { Type = MilCommand.Visual3DRemoveAllChildren; Handle = handle; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16)]
internal readonly struct MilVisual3DInsertCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; [FieldOffset(8)] internal readonly uint Child; [FieldOffset(12)] internal readonly uint Index; internal MilVisual3DInsertCommand(uint handle, uint child, uint index) { Type = MilCommand.Visual3DInsertChildAt; Handle = handle; Child = child; Index = index; } }

internal static partial class GeneratedProtocolPacketWriter
{
    internal static byte[] WriteViewport3DSetCamera(uint handle, uint camera) => WriteVisual3D(new MilVisual3DDependencyCommand(MilCommand.Viewport3DVisualSetCamera, handle, camera));
    internal static byte[] WriteViewport3DSetViewport(uint handle, MilRectD viewport) => WriteVisual3D(new MilViewport3DSetViewportCommand(handle, viewport));
    internal static byte[] WriteViewport3DSetChild(uint handle, uint child) => WriteVisual3D(new MilVisual3DDependencyCommand(MilCommand.Viewport3DVisualSet3DChild, handle, child));
    internal static byte[] WriteVisual3DSetContent(uint handle, uint content) => WriteVisual3D(new MilVisual3DDependencyCommand(MilCommand.Visual3DSetContent, handle, content));
    internal static byte[] WriteVisual3DSetTransform(uint handle, uint transform) => WriteVisual3D(new MilVisual3DDependencyCommand(MilCommand.Visual3DSetTransform, handle, transform));
    internal static byte[] WriteVisual3DRemoveAll(uint handle) => WriteVisual3D(new MilVisual3DRemoveAllCommand(handle));
    internal static byte[] WriteVisual3DRemoveChild(uint handle, uint child) => WriteVisual3D(new MilVisual3DDependencyCommand(MilCommand.Visual3DRemoveChild, handle, child));
    internal static byte[] WriteVisual3DInsertChild(uint handle, uint child, uint index) => WriteVisual3D(new MilVisual3DInsertCommand(handle, child, index));
    private static byte[] WriteVisual3D<T>(T command) where T : unmanaged { byte[] packet = new byte[Marshal.SizeOf<T>()]; MemoryMarshal.Write(packet, in command); return packet; }
}

internal sealed class GeneratedVisual3DResource : GeneratedProtocolResource
{
    private readonly List<GeneratedVisual3DResource> _children = []; private GeneratedProtocolResource? _content; private GeneratedProtocolResource? _transform;
    internal GeneratedVisual3DResource() : base(MilResourceType.Visual3D) { }
    internal GeneratedProtocolResource? Content => _content; internal GeneratedProtocolResource? Transform => _transform; internal GeneratedProtocolResource? Parent { get; private set; } internal IReadOnlyList<GeneratedVisual3DResource> Children => _children;
    internal int ProcessCommand(GeneratedProtocolHandleTable table, MilCommand command, ReadOnlySpan<byte> packet) => command switch { MilCommand.Visual3DSetContent => SetDependency(table, packet, IsModel, ref _content), MilCommand.Visual3DSetTransform => SetDependency(table, packet, IsTransform3D, ref _transform), MilCommand.Visual3DInsertChildAt => Insert(table, packet), MilCommand.Visual3DRemoveChild => Remove(table, packet), MilCommand.Visual3DRemoveAllChildren => RemoveAll(packet), _ => Direct3D9Factory.UceUnknownPacketHResult };
    private int SetDependency(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> packet, Func<MilResourceType, bool> accepts, ref GeneratedProtocolResource? field) { if (packet.Length != 12) return Direct3D9Factory.UceMalformedPacketHResult; uint handle = MemoryMarshal.Read<uint>(packet[8..]); GeneratedProtocolResource? next = null; if (handle != 0 && (!table.TryGetResource(handle, out next) || next is null || !accepts(next.ResourceType))) return Direct3D9Factory.UceMalformedPacketHResult; if (ReferenceEquals(field, next)) return 0; next?.AddListener(this); GeneratedProtocolResource? previous = field; field = next; previous?.RemoveListener(this); NotifyChanged(); return 0; }
    private int Insert(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> packet) { if (packet.Length != 16) return Direct3D9Factory.UceMalformedPacketHResult; uint childHandle = MemoryMarshal.Read<uint>(packet[8..]); uint index = MemoryMarshal.Read<uint>(packet[12..]); if (!table.TryGetResource(childHandle, MilResourceType.Visual3D, out GeneratedProtocolResource? value) || value is not GeneratedVisual3DResource child || index > _children.Count || ReferenceEquals(child, this) || child.Parent is not null || IsAncestor(child)) return Direct3D9Factory.UceMalformedPacketHResult; child.AddListener(this); _children.Insert((int)index, child); child.Parent = this; NotifyChanged(); return 0; }
    private bool IsAncestor(GeneratedVisual3DResource child) { for (GeneratedProtocolResource? parent = this; parent is not null; parent = parent is GeneratedVisual3DResource visual ? visual.Parent : null) if (ReferenceEquals(parent, child)) return true; return false; }
    private int Remove(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> packet) { if (packet.Length != 12) return Direct3D9Factory.UceMalformedPacketHResult; uint handle = MemoryMarshal.Read<uint>(packet[8..]); if (!table.TryGetResource(handle, MilResourceType.Visual3D, out GeneratedProtocolResource? value) || value is not GeneratedVisual3DResource child || !ReferenceEquals(child.Parent, this) || !_children.Remove(child)) return Direct3D9Factory.UceMalformedPacketHResult; child.Parent = null; child.RemoveListener(this); NotifyChanged(); return 0; }
    private int RemoveAll(ReadOnlySpan<byte> packet) { if (packet.Length != 8) return Direct3D9Factory.UceMalformedPacketHResult; foreach (GeneratedVisual3DResource child in _children) { child.Parent = null; child.RemoveListener(this); } _children.Clear(); NotifyChanged(); return 0; }
    internal bool TrySetViewportParent(GeneratedViewport3DVisualResource? parent) { if (parent is not null && Parent is not null) return false; Parent = parent; return true; }
    protected override void OnFinalRelease() { _content?.RemoveListener(this); _transform?.RemoveListener(this); foreach (GeneratedVisual3DResource child in _children) { child.Parent = null; child.RemoveListener(this); } _children.Clear(); _content = null; _transform = null; }
    private static bool IsModel(MilResourceType t) => t is >= MilResourceType.Model3DGroup and <= MilResourceType.GeometryModel3D;
    private static bool IsTransform3D(MilResourceType t) => t is >= MilResourceType.Transform3DGroup and <= MilResourceType.MatrixTransform3D;
}

internal sealed class GeneratedViewport3DVisualResource : GeneratedProtocolResource
{
    private GeneratedProtocolResource? _camera; private GeneratedVisual3DResource? _child;
    internal GeneratedViewport3DVisualResource() : base(MilResourceType.Viewport3DVisual) { }
    internal GeneratedProtocolResource? Camera => _camera; internal MilRectD Viewport { get; private set; } internal GeneratedVisual3DResource? Child => _child;
    internal int ProcessCommand(GeneratedProtocolHandleTable table, MilCommand command, ReadOnlySpan<byte> packet) => command switch { MilCommand.Viewport3DVisualSetCamera => SetCamera(table, packet), MilCommand.Viewport3DVisualSetViewport => SetViewport(packet), MilCommand.Viewport3DVisualSet3DChild => SetChild(table, packet), _ => Direct3D9Factory.UceUnknownPacketHResult };
    private int SetCamera(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> packet) { if (packet.Length != 12) return Direct3D9Factory.UceMalformedPacketHResult; uint handle = MemoryMarshal.Read<uint>(packet[8..]); GeneratedProtocolResource? next = null; if (handle != 0 && (!table.TryGetResource(handle, out next) || next is null || next.ResourceType is < MilResourceType.PerspectiveCamera or > MilResourceType.MatrixCamera)) return Direct3D9Factory.UceMalformedPacketHResult; if (ReferenceEquals(_camera, next)) return 0; next?.AddListener(this); GeneratedProtocolResource? previous = _camera; _camera = next; previous?.RemoveListener(this); NotifyChanged(); return 0; }
    private int SetViewport(ReadOnlySpan<byte> packet) { if (packet.Length != 40) return Direct3D9Factory.UceMalformedPacketHResult; Viewport = MemoryMarshal.Read<MilRectD>(packet[8..]); NotifyChanged(); return 0; }
    private int SetChild(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> packet) { if (packet.Length != 12) return Direct3D9Factory.UceMalformedPacketHResult; uint handle = MemoryMarshal.Read<uint>(packet[8..]); GeneratedVisual3DResource? next = null; if (handle != 0) { if (!table.TryGetResource(handle, MilResourceType.Visual3D, out GeneratedProtocolResource? value) || value is not GeneratedVisual3DResource child || (child.Parent is not null && !ReferenceEquals(child.Parent, this))) return Direct3D9Factory.UceMalformedPacketHResult; next = child; } if (ReferenceEquals(_child, next)) return 0; if (next is not null && !next.TrySetViewportParent(this)) return Direct3D9Factory.UceMalformedPacketHResult; next?.AddListener(this); GeneratedVisual3DResource? previous = _child; _child = next; if (previous is not null) { previous.TrySetViewportParent(null); previous.RemoveListener(this); } NotifyChanged(); return 0; }
    protected override void OnFinalRelease() { _camera?.RemoveListener(this); if (_child is not null) { _child.TrySetViewportParent(null); _child.RemoveListener(this); } _camera = null; _child = null; }
}
