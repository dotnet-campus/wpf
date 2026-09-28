using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal enum MilFillMode : uint
{
    Alternate,
    Winding
}

internal enum MilCombineMode : uint
{
    Union,
    Intersect,
    Xor,
    Exclude
}

internal enum MilSegmentType : uint
{
    None,
    Line,
    Bezier,
    QuadraticBezier,
    Arc,
    PolyLine,
    PolyBezier,
    PolyQuadraticBezier
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 52)]
internal readonly struct MilLineGeometryCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly MilPoint2D StartPoint;
    [FieldOffset(24)] internal readonly MilPoint2D EndPoint;
    [FieldOffset(40)] internal readonly uint Transform;
    [FieldOffset(44)] internal readonly uint StartPointAnimation;
    [FieldOffset(48)] internal readonly uint EndPointAnimation;
    public uint Handle => _handle;
    internal MilLineGeometryCommand(uint handle, MilPoint2D startPoint, MilPoint2D endPoint, uint transform, uint startPointAnimation, uint endPointAnimation)
    { Type = MilCommand.LineGeometry; _handle = handle; StartPoint = startPoint; EndPoint = endPoint; Transform = transform; StartPointAnimation = startPointAnimation; EndPointAnimation = endPointAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 72)]
internal readonly struct MilRectangleGeometryCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double RadiusX;
    [FieldOffset(16)] internal readonly double RadiusY;
    [FieldOffset(24)] internal readonly MilRectD Rect;
    [FieldOffset(56)] internal readonly uint Transform;
    [FieldOffset(60)] internal readonly uint RadiusXAnimation;
    [FieldOffset(64)] internal readonly uint RadiusYAnimation;
    [FieldOffset(68)] internal readonly uint RectAnimation;
    public uint Handle => _handle;
    internal MilRectangleGeometryCommand(uint handle, double radiusX, double radiusY, MilRectD rect, uint transform, uint radiusXAnimation, uint radiusYAnimation, uint rectAnimation)
    { Type = MilCommand.RectangleGeometry; _handle = handle; RadiusX = radiusX; RadiusY = radiusY; Rect = rect; Transform = transform; RadiusXAnimation = radiusXAnimation; RadiusYAnimation = radiusYAnimation; RectAnimation = rectAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 56)]
internal readonly struct MilEllipseGeometryCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double RadiusX;
    [FieldOffset(16)] internal readonly double RadiusY;
    [FieldOffset(24)] internal readonly MilPoint2D Center;
    [FieldOffset(40)] internal readonly uint Transform;
    [FieldOffset(44)] internal readonly uint RadiusXAnimation;
    [FieldOffset(48)] internal readonly uint RadiusYAnimation;
    [FieldOffset(52)] internal readonly uint CenterAnimation;
    public uint Handle => _handle;
    internal MilEllipseGeometryCommand(uint handle, double radiusX, double radiusY, MilPoint2D center, uint transform, uint radiusXAnimation, uint radiusYAnimation, uint centerAnimation)
    { Type = MilCommand.EllipseGeometry; _handle = handle; RadiusX = radiusX; RadiusY = radiusY; Center = center; Transform = transform; RadiusXAnimation = radiusXAnimation; RadiusYAnimation = radiusYAnimation; CenterAnimation = centerAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 20)]
internal readonly struct MilGeometryGroupCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly uint Transform;
    [FieldOffset(12)] internal readonly MilFillMode FillRule;
    [FieldOffset(16)] internal readonly uint ChildrenSize;
    public uint Handle => _handle;
    internal MilGeometryGroupCommand(uint handle, uint transform, MilFillMode fillRule, uint childrenSize)
    { Type = MilCommand.GeometryGroup; _handle = handle; Transform = transform; FillRule = fillRule; ChildrenSize = childrenSize; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
internal readonly struct MilCombinedGeometryCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly uint Transform;
    [FieldOffset(12)] internal readonly MilCombineMode GeometryCombineMode;
    [FieldOffset(16)] internal readonly uint Geometry1;
    [FieldOffset(20)] internal readonly uint Geometry2;
    public uint Handle => _handle;
    internal MilCombinedGeometryCommand(uint handle, uint transform, MilCombineMode geometryCombineMode, uint geometry1, uint geometry2)
    { Type = MilCommand.CombinedGeometry; _handle = handle; Transform = transform; GeometryCombineMode = geometryCombineMode; Geometry1 = geometry1; Geometry2 = geometry2; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 20)]
internal readonly struct MilPathGeometryCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly uint Transform;
    [FieldOffset(12)] internal readonly MilFillMode FillRule;
    [FieldOffset(16)] internal readonly uint FiguresSize;
    public uint Handle => _handle;
    internal MilPathGeometryCommand(uint handle, uint transform, MilFillMode fillRule, uint figuresSize)
    { Type = MilCommand.PathGeometry; _handle = handle; Transform = transform; FillRule = fillRule; FiguresSize = figuresSize; }
}

internal static partial class GeneratedProtocolPacketWriter
{
    internal static byte[] WriteLineGeometry(uint handle, MilPoint2D startPoint, MilPoint2D endPoint, uint transform = 0, uint startPointAnimation = 0, uint endPointAnimation = 0)
        => WriteGeometry(new MilLineGeometryCommand(handle, startPoint, endPoint, transform, startPointAnimation, endPointAnimation));

    internal static byte[] WriteRectangleGeometry(uint handle, double radiusX, double radiusY, MilRectD rect, uint transform = 0, uint radiusXAnimation = 0, uint radiusYAnimation = 0, uint rectAnimation = 0)
        => WriteGeometry(new MilRectangleGeometryCommand(handle, radiusX, radiusY, rect, transform, radiusXAnimation, radiusYAnimation, rectAnimation));

    internal static byte[] WriteEllipseGeometry(uint handle, double radiusX, double radiusY, MilPoint2D center, uint transform = 0, uint radiusXAnimation = 0, uint radiusYAnimation = 0, uint centerAnimation = 0)
        => WriteGeometry(new MilEllipseGeometryCommand(handle, radiusX, radiusY, center, transform, radiusXAnimation, radiusYAnimation, centerAnimation));

    internal static byte[] WriteGeometryGroup(uint handle, MilFillMode fillRule, uint transform = 0, params uint[] children)
    {
        ArgumentNullException.ThrowIfNull(children);
        int payloadSize = checked(children.Length * sizeof(uint));
        byte[] packet = new byte[Marshal.SizeOf<MilGeometryGroupCommand>() + payloadSize];
        MilGeometryGroupCommand command = new(handle, transform, fillRule, (uint)payloadSize);
        MemoryMarshal.Write(packet, in command);
        children.AsSpan().CopyTo(MemoryMarshal.Cast<byte, uint>(packet.AsSpan(Marshal.SizeOf<MilGeometryGroupCommand>())));
        return packet;
    }

    internal static byte[] WriteCombinedGeometry(uint handle, MilCombineMode combineMode, uint transform = 0, uint geometry1 = 0, uint geometry2 = 0)
        => WriteGeometry(new MilCombinedGeometryCommand(handle, transform, combineMode, geometry1, geometry2));

    internal static byte[] WritePathGeometry(uint handle, MilFillMode fillRule, ReadOnlySpan<byte> figures, uint transform = 0)
    {
        byte[] packet = new byte[Marshal.SizeOf<MilPathGeometryCommand>() + figures.Length];
        MilPathGeometryCommand command = new(handle, transform, fillRule, (uint)figures.Length);
        MemoryMarshal.Write(packet, in command);
        figures.CopyTo(packet.AsSpan(Marshal.SizeOf<MilPathGeometryCommand>()));
        return packet;
    }

    internal static byte[] WriteEmptyPathFigures()
    {
        byte[] figures = new byte[48];
        MemoryMarshal.Write(figures, 48u);
        return figures;
    }

    private static byte[] WriteGeometry<T>(T command) where T : unmanaged
    {
        byte[] packet = new byte[Marshal.SizeOf<T>()];
        MemoryMarshal.Write(packet, in command);
        return packet;
    }
}

internal abstract class GeneratedGeometryResource : GeneratedDependencyResource
{
    protected GeneratedGeometryResource(MilResourceType resourceType) : base(resourceType) { }

    protected static bool IsTransform(MilResourceType resourceType) => resourceType is
        MilResourceType.TransformGroup or MilResourceType.TranslateTransform or MilResourceType.ScaleTransform or
        MilResourceType.SkewTransform or MilResourceType.RotateTransform or MilResourceType.MatrixTransform;

    protected static bool IsGeometry(MilResourceType resourceType) => resourceType is
        MilResourceType.LineGeometry or MilResourceType.RectangleGeometry or MilResourceType.EllipseGeometry or
        MilResourceType.GeometryGroup or MilResourceType.CombinedGeometry or MilResourceType.PathGeometry;

    protected static bool IsValid(MilFillMode value) => value is MilFillMode.Alternate or MilFillMode.Winding;
    protected static bool IsValid(MilCombineMode value) => value is >= MilCombineMode.Union and <= MilCombineMode.Exclude;
}

internal sealed class GeneratedLineGeometryResource : GeneratedGeometryResource
{
    internal GeneratedLineGeometryResource() : base(MilResourceType.LineGeometry) { }
    internal (MilPoint2D StartPoint, MilPoint2D EndPoint) Value { get; private set; }
    internal IReadOnlyList<GeneratedProtocolResource> Dependencies { get; private set; } = [];

    internal override int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value)
    {
        if (value.Length != 44) return Direct3D9Factory.UceMalformedPacketHResult;
        MilPoint2D start = MemoryMarshal.Read<MilPoint2D>(value);
        MilPoint2D end = MemoryMarshal.Read<MilPoint2D>(value[16..]);
        if (!TryResolve(handleTable, MemoryMarshal.Read<uint>(value[32..]), IsTransform, out GeneratedProtocolResource? transform)
            || !TryResolve(handleTable, MemoryMarshal.Read<uint>(value[36..]), MilResourceType.PointResource, out GeneratedProtocolResource? startAnimation)
            || !TryResolve(handleTable, MemoryMarshal.Read<uint>(value[40..]), MilResourceType.PointResource, out GeneratedProtocolResource? endAnimation))
            return Direct3D9Factory.UceMalformedPacketHResult;
        GeneratedProtocolResource?[] dependencies = [transform, startAnimation, endAnimation];
        return CommitDependencies(dependencies, () => { Value = (start, end); Dependencies = Present(dependencies); });
    }

    private static GeneratedProtocolResource[] Present(GeneratedProtocolResource?[] dependencies) => dependencies.Where(static item => item is not null).Cast<GeneratedProtocolResource>().ToArray();
}

internal sealed class GeneratedRectangleGeometryResource : GeneratedGeometryResource
{
    internal GeneratedRectangleGeometryResource() : base(MilResourceType.RectangleGeometry) { }
    internal (double RadiusX, double RadiusY, MilRectD Rect) Value { get; private set; }
    internal IReadOnlyList<GeneratedProtocolResource> Dependencies { get; private set; } = [];

    internal override int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value)
    {
        if (value.Length != 64) return Direct3D9Factory.UceMalformedPacketHResult;
        double radiusX = MemoryMarshal.Read<double>(value);
        double radiusY = MemoryMarshal.Read<double>(value[8..]);
        MilRectD rect = MemoryMarshal.Read<MilRectD>(value[16..]);
        if (!TryResolve(handleTable, MemoryMarshal.Read<uint>(value[48..]), IsTransform, out GeneratedProtocolResource? transform)
            || !TryResolve(handleTable, MemoryMarshal.Read<uint>(value[52..]), MilResourceType.DoubleResource, out GeneratedProtocolResource? radiusXAnimation)
            || !TryResolve(handleTable, MemoryMarshal.Read<uint>(value[56..]), MilResourceType.DoubleResource, out GeneratedProtocolResource? radiusYAnimation)
            || !TryResolve(handleTable, MemoryMarshal.Read<uint>(value[60..]), MilResourceType.RectResource, out GeneratedProtocolResource? rectAnimation))
            return Direct3D9Factory.UceMalformedPacketHResult;
        GeneratedProtocolResource?[] dependencies = [transform, radiusXAnimation, radiusYAnimation, rectAnimation];
        return CommitDependencies(dependencies, () => { Value = (radiusX, radiusY, rect); Dependencies = Present(dependencies); });
    }

    private static GeneratedProtocolResource[] Present(GeneratedProtocolResource?[] dependencies) => dependencies.Where(static item => item is not null).Cast<GeneratedProtocolResource>().ToArray();
}

internal sealed class GeneratedEllipseGeometryResource : GeneratedGeometryResource
{
    internal GeneratedEllipseGeometryResource() : base(MilResourceType.EllipseGeometry) { }
    internal (double RadiusX, double RadiusY, MilPoint2D Center) Value { get; private set; }
    internal IReadOnlyList<GeneratedProtocolResource> Dependencies { get; private set; } = [];

    internal override int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value)
    {
        if (value.Length != 48) return Direct3D9Factory.UceMalformedPacketHResult;
        double radiusX = MemoryMarshal.Read<double>(value);
        double radiusY = MemoryMarshal.Read<double>(value[8..]);
        MilPoint2D center = MemoryMarshal.Read<MilPoint2D>(value[16..]);
        if (!TryResolve(handleTable, MemoryMarshal.Read<uint>(value[32..]), IsTransform, out GeneratedProtocolResource? transform)
            || !TryResolve(handleTable, MemoryMarshal.Read<uint>(value[36..]), MilResourceType.DoubleResource, out GeneratedProtocolResource? radiusXAnimation)
            || !TryResolve(handleTable, MemoryMarshal.Read<uint>(value[40..]), MilResourceType.DoubleResource, out GeneratedProtocolResource? radiusYAnimation)
            || !TryResolve(handleTable, MemoryMarshal.Read<uint>(value[44..]), MilResourceType.PointResource, out GeneratedProtocolResource? centerAnimation))
            return Direct3D9Factory.UceMalformedPacketHResult;
        GeneratedProtocolResource?[] dependencies = [transform, radiusXAnimation, radiusYAnimation, centerAnimation];
        return CommitDependencies(dependencies, () => { Value = (radiusX, radiusY, center); Dependencies = Present(dependencies); });
    }

    private static GeneratedProtocolResource[] Present(GeneratedProtocolResource?[] dependencies) => dependencies.Where(static item => item is not null).Cast<GeneratedProtocolResource>().ToArray();
}

internal sealed class GeneratedGeometryGroupResource : GeneratedGeometryResource
{
    internal GeneratedGeometryGroupResource() : base(MilResourceType.GeometryGroup) { }
    internal MilFillMode FillRule { get; private set; }
    internal GeneratedProtocolResource? Transform { get; private set; }
    internal IReadOnlyList<GeneratedProtocolResource> Children { get; private set; } = [];

    internal override int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value)
    {
        if (value.Length < 12) return Direct3D9Factory.UceMalformedPacketHResult;
        uint transformHandle = MemoryMarshal.Read<uint>(value);
        MilFillMode fillRule = MemoryMarshal.Read<MilFillMode>(value[4..]);
        uint childrenSize = MemoryMarshal.Read<uint>(value[8..]);
        ReadOnlySpan<byte> payload = value[12..];
        if (!IsValid(fillRule) || childrenSize != payload.Length || childrenSize % sizeof(uint) != 0
            || !TryResolve(handleTable, transformHandle, IsTransform, out GeneratedProtocolResource? transform))
            return Direct3D9Factory.UceMalformedPacketHResult;
        ReadOnlySpan<uint> handles = MemoryMarshal.Cast<byte, uint>(payload);
        GeneratedProtocolResource?[] dependencies = new GeneratedProtocolResource?[handles.Length + 1];
        dependencies[0] = transform;
        for (int i = 0; i < handles.Length; i++)
        {
            if (handles[i] == 0 || !TryResolve(handleTable, handles[i], IsGeometry, out dependencies[i + 1]))
                return Direct3D9Factory.UceMalformedPacketHResult;
        }
        return CommitDependencies(dependencies, () => { FillRule = fillRule; Transform = transform; Children = dependencies[1..].Cast<GeneratedProtocolResource>().ToArray(); });
    }
}

internal sealed class GeneratedCombinedGeometryResource : GeneratedGeometryResource
{
    internal GeneratedCombinedGeometryResource() : base(MilResourceType.CombinedGeometry) { }
    internal MilCombineMode CombineMode { get; private set; }
    internal GeneratedProtocolResource? Transform { get; private set; }
    internal GeneratedProtocolResource? Geometry1 { get; private set; }
    internal GeneratedProtocolResource? Geometry2 { get; private set; }

    internal override int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value)
    {
        if (value.Length != 16) return Direct3D9Factory.UceMalformedPacketHResult;
        MilCombineMode combineMode = MemoryMarshal.Read<MilCombineMode>(value[4..]);
        if (!IsValid(combineMode)
            || !TryResolve(handleTable, MemoryMarshal.Read<uint>(value), IsTransform, out GeneratedProtocolResource? transform)
            || !TryResolve(handleTable, MemoryMarshal.Read<uint>(value[8..]), IsGeometry, out GeneratedProtocolResource? geometry1)
            || !TryResolve(handleTable, MemoryMarshal.Read<uint>(value[12..]), IsGeometry, out GeneratedProtocolResource? geometry2))
            return Direct3D9Factory.UceMalformedPacketHResult;
        GeneratedProtocolResource?[] dependencies = [transform, geometry1, geometry2];
        return CommitDependencies(dependencies, () => { CombineMode = combineMode; Transform = transform; Geometry1 = geometry1; Geometry2 = geometry2; });
    }
}

internal sealed class GeneratedPathGeometryResource : GeneratedGeometryResource
{
    internal GeneratedPathGeometryResource() : base(MilResourceType.PathGeometry) { }
    internal MilFillMode FillRule { get; private set; }
    internal GeneratedProtocolResource? Transform { get; private set; }
    internal byte[] Figures { get; private set; } = [];

    internal override int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value)
    {
        if (value.Length < 12) return Direct3D9Factory.UceMalformedPacketHResult;
        uint transformHandle = MemoryMarshal.Read<uint>(value);
        MilFillMode fillRule = MemoryMarshal.Read<MilFillMode>(value[4..]);
        uint figuresSize = MemoryMarshal.Read<uint>(value[8..]);
        ReadOnlySpan<byte> figures = value[12..];
        if (!IsValid(fillRule) || figuresSize != figures.Length || !MilPathGeometryValidator.IsValid(figures)
            || !TryResolve(handleTable, transformHandle, IsTransform, out GeneratedProtocolResource? transform))
            return Direct3D9Factory.UceMalformedPacketHResult;
        byte[] nextFigures = figures.ToArray();
        GeneratedProtocolResource?[] dependencies = [transform];
        return CommitDependencies(dependencies, () => { FillRule = fillRule; Transform = transform; Figures = nextFigures; });
    }
}

internal static class MilPathGeometryValidator
{
    private const int GeometrySize = 48;
    private const int FigureSize = 40;
    private const int SegmentSize = 12;
    private const uint IsRegionData = 0x10;

    internal static bool IsValid(ReadOnlySpan<byte> data)
    {
        if (data.Length < GeometrySize || ReadUInt32(data) != data.Length)
            return false;
        bool isRegion = (ReadUInt32(data[4..]) & IsRegionData) != 0;
        uint expectedFigures = ReadUInt32(data[40..]);
        int offset = GeometrySize;
        int figureCount = 0;
        int previousFigureOffset = offset;
        while (offset < data.Length)
        {
            if (data.Length - offset < FigureSize) return false;
            ReadOnlySpan<byte> figure = data[offset..];
            uint backSize = ReadUInt32(figure);
            uint segmentCount = ReadUInt32(figure[8..]);
            uint declaredFigureSize = ReadUInt32(figure[12..]);
            uint offsetToLastSegment = ReadUInt32(figure[32..]);
            if (offset - checked((int)backSize) != previousFigureOffset || declaredFigureSize < FigureSize || declaredFigureSize > data.Length - offset)
                return false;
            int figureStart = offset;
            offset += FigureSize;
            int previousSegmentOffset = offset;
            int lastSegmentOffset = -1;
            for (uint i = 0; i < segmentCount; i++)
            {
                if (data.Length - offset < SegmentSize) return false;
                ReadOnlySpan<byte> segment = data[offset..];
                MilSegmentType type = (MilSegmentType)ReadUInt32(segment);
                uint segmentBackSize = ReadUInt32(segment[8..]);
                if (offset - checked((int)segmentBackSize) != previousSegmentOffset || (isRegion && type != MilSegmentType.PolyLine))
                    return false;
                int segmentBytes = GetSegmentSize(type, segment, isRegion);
                if (segmentBytes < 0 || segmentBytes > data.Length - offset) return false;
                previousSegmentOffset = offset;
                lastSegmentOffset = offset;
                offset += segmentBytes;
            }
            if (offset - figureStart != declaredFigureSize || (lastSegmentOffset >= 0 && figureStart + offsetToLastSegment != lastSegmentOffset)
                || (lastSegmentOffset < 0 && offsetToLastSegment != 0))
                return false;
            previousFigureOffset = figureStart;
            figureCount++;
        }
        return offset == data.Length && figureCount == expectedFigures;
    }

    private static int GetSegmentSize(MilSegmentType type, ReadOnlySpan<byte> segment, bool isRegion)
    {
        return type switch
        {
            MilSegmentType.None => 12,
            MilSegmentType.Line => 32,
            MilSegmentType.Bezier => 64,
            MilSegmentType.QuadraticBezier => 48,
            MilSegmentType.Arc => 56,
            MilSegmentType.PolyLine => GetPolySize(segment, isRegion ? 3u : null, 1),
            MilSegmentType.PolyBezier => GetPolySize(segment, null, 3),
            MilSegmentType.PolyQuadraticBezier => GetPolySize(segment, null, 2),
            _ => -1
        };
    }

    private static int GetPolySize(ReadOnlySpan<byte> segment, uint? requiredCount, uint divisor)
    {
        if (segment.Length < 16) return -1;
        uint count = ReadUInt32(segment[12..]);
        if (count == 0 || requiredCount.HasValue && count != requiredCount.Value || count % divisor != 0)
            return -1;
        long size = 16L + count * 16L;
        return size > int.MaxValue ? -1 : (int)size;
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> value) => MemoryMarshal.Read<uint>(value);
}
