using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal enum MilColorInterpolationMode : uint { ScRgbLinearInterpolation, SRgbLinearInterpolation }
internal enum MilBrushMappingMode : uint { Absolute, RelativeToBoundingBox }
internal enum MilGradientSpreadMethod : uint { Pad, Reflect, Repeat }
internal enum MilStretch : uint { None, Fill, Uniform, UniformToFill }
internal enum MilTileMode : uint { None = 0, FlipX = 1, FlipY = 2, FlipXY = 3, Tile = 4, Extend = 5 }
internal enum MilHorizontalAlignment : uint { Left, Center, Right }
internal enum MilVerticalAlignment : uint { Top, Center, Bottom }
internal enum MilCachingHint : uint { Unspecified, Cache }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilGradientStop(double Position, MilColorF Color);

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 48)]
internal readonly struct MilSolidColorBrushCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double Opacity;
    [FieldOffset(16)] internal readonly MilColorF Color;
    [FieldOffset(32)] internal readonly uint OpacityAnimation;
    [FieldOffset(36)] internal readonly uint Transform;
    [FieldOffset(40)] internal readonly uint RelativeTransform;
    [FieldOffset(44)] internal readonly uint ColorAnimation;
    public uint Handle => _handle;
    internal MilSolidColorBrushCommand(uint handle, double opacity, MilColorF color, uint opacityAnimation, uint transform, uint relativeTransform, uint colorAnimation)
    { Type = MilCommand.SolidColorBrush; _handle = handle; Opacity = opacity; Color = color; OpacityAnimation = opacityAnimation; Transform = transform; RelativeTransform = relativeTransform; ColorAnimation = colorAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 84)]
internal readonly struct MilLinearGradientBrushCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double Opacity; [FieldOffset(16)] internal readonly MilPoint2D StartPoint; [FieldOffset(32)] internal readonly MilPoint2D EndPoint;
    [FieldOffset(48)] internal readonly uint OpacityAnimation; [FieldOffset(52)] internal readonly uint Transform; [FieldOffset(56)] internal readonly uint RelativeTransform;
    [FieldOffset(60)] internal readonly MilColorInterpolationMode ColorInterpolationMode; [FieldOffset(64)] internal readonly MilBrushMappingMode MappingMode;
    [FieldOffset(68)] internal readonly MilGradientSpreadMethod SpreadMethod; [FieldOffset(72)] internal readonly uint GradientStopsSize;
    [FieldOffset(76)] internal readonly uint StartPointAnimation; [FieldOffset(80)] internal readonly uint EndPointAnimation;
    public uint Handle => _handle;
    internal MilLinearGradientBrushCommand(uint handle, double opacity, MilPoint2D start, MilPoint2D end, uint opacityAnimation, uint transform, uint relativeTransform, MilColorInterpolationMode colorMode, MilBrushMappingMode mappingMode, MilGradientSpreadMethod spread, uint stopsSize, uint startAnimation, uint endAnimation)
    { Type = MilCommand.LinearGradientBrush; _handle = handle; Opacity = opacity; StartPoint = start; EndPoint = end; OpacityAnimation = opacityAnimation; Transform = transform; RelativeTransform = relativeTransform; ColorInterpolationMode = colorMode; MappingMode = mappingMode; SpreadMethod = spread; GradientStopsSize = stopsSize; StartPointAnimation = startAnimation; EndPointAnimation = endAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 108)]
internal readonly struct MilRadialGradientBrushCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly double Opacity;
    [FieldOffset(16)] internal readonly MilPoint2D Center; [FieldOffset(32)] internal readonly double RadiusX; [FieldOffset(40)] internal readonly double RadiusY; [FieldOffset(48)] internal readonly MilPoint2D GradientOrigin;
    [FieldOffset(64)] internal readonly uint OpacityAnimation; [FieldOffset(68)] internal readonly uint Transform; [FieldOffset(72)] internal readonly uint RelativeTransform;
    [FieldOffset(76)] internal readonly MilColorInterpolationMode ColorInterpolationMode; [FieldOffset(80)] internal readonly MilBrushMappingMode MappingMode; [FieldOffset(84)] internal readonly MilGradientSpreadMethod SpreadMethod;
    [FieldOffset(88)] internal readonly uint GradientStopsSize; [FieldOffset(92)] internal readonly uint CenterAnimation; [FieldOffset(96)] internal readonly uint RadiusXAnimation; [FieldOffset(100)] internal readonly uint RadiusYAnimation; [FieldOffset(104)] internal readonly uint GradientOriginAnimation;
    public uint Handle => _handle;
    internal MilRadialGradientBrushCommand(uint handle, double opacity, MilPoint2D center, double radiusX, double radiusY, MilPoint2D origin, uint opacityAnimation, uint transform, uint relativeTransform, MilColorInterpolationMode colorMode, MilBrushMappingMode mappingMode, MilGradientSpreadMethod spread, uint stopsSize, uint centerAnimation, uint radiusXAnimation, uint radiusYAnimation, uint originAnimation)
    { Type = MilCommand.RadialGradientBrush; _handle = handle; Opacity = opacity; Center = center; RadiusX = radiusX; RadiusY = radiusY; GradientOrigin = origin; OpacityAnimation = opacityAnimation; Transform = transform; RelativeTransform = relativeTransform; ColorInterpolationMode = colorMode; MappingMode = mappingMode; SpreadMethod = spread; GradientStopsSize = stopsSize; CenterAnimation = centerAnimation; RadiusXAnimation = radiusXAnimation; RadiusYAnimation = radiusYAnimation; GradientOriginAnimation = originAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 148)]
internal readonly struct MilTileBrushCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly double Opacity;
    [FieldOffset(16)] internal readonly MilRectD Viewport; [FieldOffset(48)] internal readonly MilRectD Viewbox;
    [FieldOffset(80)] internal readonly double CacheMinimum; [FieldOffset(88)] internal readonly double CacheMaximum;
    [FieldOffset(96)] internal readonly uint OpacityAnimation; [FieldOffset(100)] internal readonly uint Transform; [FieldOffset(104)] internal readonly uint RelativeTransform;
    [FieldOffset(108)] internal readonly MilBrushMappingMode ViewportUnits; [FieldOffset(112)] internal readonly MilBrushMappingMode ViewboxUnits;
    [FieldOffset(116)] internal readonly uint ViewportAnimation; [FieldOffset(120)] internal readonly uint ViewboxAnimation;
    [FieldOffset(124)] internal readonly MilStretch Stretch; [FieldOffset(128)] internal readonly MilTileMode TileMode; [FieldOffset(132)] internal readonly MilHorizontalAlignment AlignmentX; [FieldOffset(136)] internal readonly MilVerticalAlignment AlignmentY; [FieldOffset(140)] internal readonly MilCachingHint CachingHint;
    [FieldOffset(144)] internal readonly uint Source;
    public uint Handle => _handle;
    internal MilTileBrushCommand(MilCommand type, uint handle, double opacity, MilRectD viewport, MilRectD viewbox, double cacheMinimum, double cacheMaximum, uint opacityAnimation, uint transform, uint relativeTransform, MilBrushMappingMode viewportUnits, MilBrushMappingMode viewboxUnits, uint viewportAnimation, uint viewboxAnimation, MilStretch stretch, MilTileMode tileMode, MilHorizontalAlignment alignmentX, MilVerticalAlignment alignmentY, MilCachingHint cachingHint, uint source)
    { Type = type; _handle = handle; Opacity = opacity; Viewport = viewport; Viewbox = viewbox; CacheMinimum = cacheMinimum; CacheMaximum = cacheMaximum; OpacityAnimation = opacityAnimation; Transform = transform; RelativeTransform = relativeTransform; ViewportUnits = viewportUnits; ViewboxUnits = viewboxUnits; ViewportAnimation = viewportAnimation; ViewboxAnimation = viewboxAnimation; Stretch = stretch; TileMode = tileMode; AlignmentX = alignmentX; AlignmentY = alignmentY; CachingHint = cachingHint; Source = source; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 36)]
internal readonly struct MilBitmapCacheBrushCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly double Opacity;
    [FieldOffset(16)] internal readonly uint OpacityAnimation; [FieldOffset(20)] internal readonly uint Transform; [FieldOffset(24)] internal readonly uint RelativeTransform; [FieldOffset(28)] internal readonly uint BitmapCache; [FieldOffset(32)] internal readonly uint InternalTarget;
    public uint Handle => _handle;
    internal MilBitmapCacheBrushCommand(uint handle, double opacity, uint opacityAnimation, uint transform, uint relativeTransform, uint bitmapCache, uint internalTarget)
    { Type = MilCommand.BitmapCacheBrush; _handle = handle; Opacity = opacity; OpacityAnimation = opacityAnimation; Transform = transform; RelativeTransform = relativeTransform; BitmapCache = bitmapCache; InternalTarget = internalTarget; }
}

internal static partial class GeneratedProtocolPacketWriter
{
    internal static byte[] WriteSolidColorBrush(uint handle, double opacity, MilColorF color, uint opacityAnimation = 0, uint transform = 0, uint relativeTransform = 0, uint colorAnimation = 0) => WriteBrush(new MilSolidColorBrushCommand(handle, opacity, color, opacityAnimation, transform, relativeTransform, colorAnimation));
    internal static byte[] WriteLinearGradientBrush(uint handle, double opacity, MilPoint2D start, MilPoint2D end, ReadOnlySpan<MilGradientStop> stops, uint opacityAnimation = 0, uint transform = 0, uint relativeTransform = 0, MilColorInterpolationMode colorMode = MilColorInterpolationMode.SRgbLinearInterpolation, MilBrushMappingMode mappingMode = MilBrushMappingMode.RelativeToBoundingBox, MilGradientSpreadMethod spread = MilGradientSpreadMethod.Pad, uint startAnimation = 0, uint endAnimation = 0)
        => WriteGradient(new MilLinearGradientBrushCommand(handle, opacity, start, end, opacityAnimation, transform, relativeTransform, colorMode, mappingMode, spread, checked((uint)(stops.Length * Marshal.SizeOf<MilGradientStop>())), startAnimation, endAnimation), stops);
    internal static byte[] WriteRadialGradientBrush(uint handle, double opacity, MilPoint2D center, double radiusX, double radiusY, MilPoint2D origin, ReadOnlySpan<MilGradientStop> stops, uint opacityAnimation = 0, uint transform = 0, uint relativeTransform = 0, MilColorInterpolationMode colorMode = MilColorInterpolationMode.SRgbLinearInterpolation, MilBrushMappingMode mappingMode = MilBrushMappingMode.RelativeToBoundingBox, MilGradientSpreadMethod spread = MilGradientSpreadMethod.Pad, uint centerAnimation = 0, uint radiusXAnimation = 0, uint radiusYAnimation = 0, uint originAnimation = 0)
        => WriteGradient(new MilRadialGradientBrushCommand(handle, opacity, center, radiusX, radiusY, origin, opacityAnimation, transform, relativeTransform, colorMode, mappingMode, spread, checked((uint)(stops.Length * Marshal.SizeOf<MilGradientStop>())), centerAnimation, radiusXAnimation, radiusYAnimation, originAnimation), stops);
    internal static byte[] WriteTileBrush(MilCommand type, uint handle, double opacity, MilRectD viewport, MilRectD viewbox, double cacheMinimum, double cacheMaximum, uint source = 0, uint opacityAnimation = 0, uint transform = 0, uint relativeTransform = 0, MilBrushMappingMode viewportUnits = MilBrushMappingMode.RelativeToBoundingBox, MilBrushMappingMode viewboxUnits = MilBrushMappingMode.RelativeToBoundingBox, uint viewportAnimation = 0, uint viewboxAnimation = 0, MilStretch stretch = MilStretch.Fill, MilTileMode tileMode = MilTileMode.None, MilHorizontalAlignment alignmentX = MilHorizontalAlignment.Center, MilVerticalAlignment alignmentY = MilVerticalAlignment.Center, MilCachingHint cachingHint = MilCachingHint.Unspecified)
        => WriteBrush(new MilTileBrushCommand(type, handle, opacity, viewport, viewbox, cacheMinimum, cacheMaximum, opacityAnimation, transform, relativeTransform, viewportUnits, viewboxUnits, viewportAnimation, viewboxAnimation, stretch, tileMode, alignmentX, alignmentY, cachingHint, source));
    internal static byte[] WriteBitmapCacheBrush(uint handle, double opacity, uint opacityAnimation = 0, uint transform = 0, uint relativeTransform = 0, uint bitmapCache = 0, uint internalTarget = 0) => WriteBrush(new MilBitmapCacheBrushCommand(handle, opacity, opacityAnimation, transform, relativeTransform, bitmapCache, internalTarget));

    private static byte[] WriteGradient<T>(T command, ReadOnlySpan<MilGradientStop> stops) where T : unmanaged
    {
        int headerSize = Marshal.SizeOf<T>(); byte[] packet = new byte[headerSize + stops.Length * Marshal.SizeOf<MilGradientStop>()];
        MemoryMarshal.Write(packet, in command); stops.CopyTo(MemoryMarshal.Cast<byte, MilGradientStop>(packet.AsSpan(headerSize))); return packet;
    }
    private static byte[] WriteBrush<T>(T command) where T : unmanaged { byte[] packet = new byte[Marshal.SizeOf<T>()]; MemoryMarshal.Write(packet, in command); return packet; }
}

internal abstract class GeneratedBrushResource : GeneratedDependencyResource
{
    protected GeneratedBrushResource(MilResourceType type) : base(type) { }
    protected static bool IsTransform(MilResourceType type) => type is >= MilResourceType.TransformGroup and <= MilResourceType.MatrixTransform;
    protected static bool IsDrawing(MilResourceType type) => type is >= MilResourceType.GeometryDrawing and <= MilResourceType.DrawingGroup;
    protected static bool IsImageSource(MilResourceType type) => type is MilResourceType.DrawingImage or MilResourceType.BitmapSource or MilResourceType.DoubleBufferedBitmap or MilResourceType.D3DImage;
    protected static bool Valid(MilColorInterpolationMode value) => value <= MilColorInterpolationMode.SRgbLinearInterpolation;
    protected static bool Valid(MilBrushMappingMode value) => value <= MilBrushMappingMode.RelativeToBoundingBox;
    protected static bool Valid(MilGradientSpreadMethod value) => value <= MilGradientSpreadMethod.Repeat;
    protected static bool Valid(MilStretch value) => value <= MilStretch.UniformToFill;
    protected static bool Valid(MilTileMode value) => value is MilTileMode.None or MilTileMode.FlipX or MilTileMode.FlipY or MilTileMode.FlipXY or MilTileMode.Tile or MilTileMode.Extend;
    protected static bool Valid(MilHorizontalAlignment value) => value <= MilHorizontalAlignment.Right;
    protected static bool Valid(MilVerticalAlignment value) => value <= MilVerticalAlignment.Bottom;
    protected static bool Valid(MilCachingHint value) => value <= MilCachingHint.Cache;
    protected static GeneratedProtocolResource[] Present(GeneratedProtocolResource?[] values) => values.Where(static value => value is not null).Cast<GeneratedProtocolResource>().ToArray();
}

internal sealed class GeneratedSolidColorBrushResource : GeneratedBrushResource
{
    internal GeneratedSolidColorBrushResource() : base(MilResourceType.SolidColorBrush) { }
    internal (double Opacity, MilColorF Color) Value { get; private set; }
    internal IReadOnlyList<GeneratedProtocolResource> Dependencies { get; private set; } = [];
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length != 40) return Direct3D9Factory.UceMalformedPacketHResult;
        if (!TryResolve(table, MemoryMarshal.Read<uint>(value[24..]), MilResourceType.DoubleResource, out GeneratedProtocolResource? opacityAnimation)
            || !TryResolve(table, MemoryMarshal.Read<uint>(value[28..]), IsTransform, out GeneratedProtocolResource? transform)
            || !TryResolve(table, MemoryMarshal.Read<uint>(value[32..]), IsTransform, out GeneratedProtocolResource? relativeTransform)
            || !TryResolve(table, MemoryMarshal.Read<uint>(value[36..]), MilResourceType.ColorResource, out GeneratedProtocolResource? colorAnimation)) return Direct3D9Factory.UceMalformedPacketHResult;
        double opacity = MemoryMarshal.Read<double>(value); MilColorF color = MemoryMarshal.Read<MilColorF>(value[8..]); GeneratedProtocolResource?[] dependencies = [opacityAnimation, transform, relativeTransform, colorAnimation];
        return CommitDependencies(dependencies, () => { Value = (opacity, color); Dependencies = Present(dependencies); });
    }
}

internal abstract class GeneratedGradientBrushResource : GeneratedBrushResource
{
    protected GeneratedGradientBrushResource(MilResourceType type) : base(type) { }
    internal MilGradientStop[] Stops { get; private set; } = [];
    protected int CommitGradient(GeneratedProtocolResource?[] dependencies, ReadOnlySpan<byte> payload, Action commit)
    {
        if (payload.Length % Marshal.SizeOf<MilGradientStop>() != 0) return Direct3D9Factory.UceMalformedPacketHResult;
        MilGradientStop[] stops = MemoryMarshal.Cast<byte, MilGradientStop>(payload).ToArray();
        return CommitDependencies(dependencies, () => { Stops = stops; commit(); });
    }
}

internal sealed class GeneratedLinearGradientBrushResource : GeneratedGradientBrushResource
{
    internal GeneratedLinearGradientBrushResource() : base(MilResourceType.LinearGradientBrush) { }
    internal (double Opacity, MilPoint2D Start, MilPoint2D End, MilColorInterpolationMode ColorMode, MilBrushMappingMode MappingMode, MilGradientSpreadMethod Spread) Value { get; private set; }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length < 76) return Direct3D9Factory.UceMalformedPacketHResult;
        MilColorInterpolationMode colorMode = MemoryMarshal.Read<MilColorInterpolationMode>(value[52..]); MilBrushMappingMode mapping = MemoryMarshal.Read<MilBrushMappingMode>(value[56..]); MilGradientSpreadMethod spread = MemoryMarshal.Read<MilGradientSpreadMethod>(value[60..]); uint size = MemoryMarshal.Read<uint>(value[64..]); ReadOnlySpan<byte> payload = value[76..];
        if (!Valid(colorMode) || !Valid(mapping) || !Valid(spread) || size != payload.Length
            || !ResolveCommon(table, value, out GeneratedProtocolResource?[] dependencies)) return Direct3D9Factory.UceMalformedPacketHResult;
        double opacity = MemoryMarshal.Read<double>(value); MilPoint2D start = MemoryMarshal.Read<MilPoint2D>(value[8..]); MilPoint2D end = MemoryMarshal.Read<MilPoint2D>(value[24..]);
        return CommitGradient(dependencies, payload, () => Value = (opacity, start, end, colorMode, mapping, spread));
    }
    private bool ResolveCommon(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value, out GeneratedProtocolResource?[] dependencies)
    {
        dependencies = new GeneratedProtocolResource?[5];
        return TryResolve(table, MemoryMarshal.Read<uint>(value[40..]), MilResourceType.DoubleResource, out dependencies[0])
            && TryResolve(table, MemoryMarshal.Read<uint>(value[44..]), IsTransform, out dependencies[1]) && TryResolve(table, MemoryMarshal.Read<uint>(value[48..]), IsTransform, out dependencies[2])
            && TryResolve(table, MemoryMarshal.Read<uint>(value[68..]), MilResourceType.PointResource, out dependencies[3]) && TryResolve(table, MemoryMarshal.Read<uint>(value[72..]), MilResourceType.PointResource, out dependencies[4]);
    }
}

internal sealed class GeneratedRadialGradientBrushResource : GeneratedGradientBrushResource
{
    internal GeneratedRadialGradientBrushResource() : base(MilResourceType.RadialGradientBrush) { }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length < 100) return Direct3D9Factory.UceMalformedPacketHResult;
        MilColorInterpolationMode colorMode = MemoryMarshal.Read<MilColorInterpolationMode>(value[68..]); MilBrushMappingMode mapping = MemoryMarshal.Read<MilBrushMappingMode>(value[72..]); MilGradientSpreadMethod spread = MemoryMarshal.Read<MilGradientSpreadMethod>(value[76..]); uint size = MemoryMarshal.Read<uint>(value[80..]); ReadOnlySpan<byte> payload = value[100..];
        GeneratedProtocolResource?[] dependencies = new GeneratedProtocolResource?[7];
        if (!Valid(colorMode) || !Valid(mapping) || !Valid(spread) || size != payload.Length
            || !TryResolve(table, MemoryMarshal.Read<uint>(value[56..]), MilResourceType.DoubleResource, out dependencies[0]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[60..]), IsTransform, out dependencies[1]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[64..]), IsTransform, out dependencies[2])
            || !TryResolve(table, MemoryMarshal.Read<uint>(value[84..]), MilResourceType.PointResource, out dependencies[3]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[88..]), MilResourceType.DoubleResource, out dependencies[4]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[92..]), MilResourceType.DoubleResource, out dependencies[5]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[96..]), MilResourceType.PointResource, out dependencies[6])) return Direct3D9Factory.UceMalformedPacketHResult;
        return CommitGradient(dependencies, payload, static () => { });
    }
}

internal abstract class GeneratedTileBrushResource : GeneratedBrushResource
{
    private readonly Func<MilResourceType, bool> _sourceType;
    protected GeneratedTileBrushResource(MilResourceType type, Func<MilResourceType, bool> sourceType) : base(type) => _sourceType = sourceType;
    internal MilTileMode TileMode { get; private set; }
    internal GeneratedProtocolResource? Source { get; private set; }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length != 140) return Direct3D9Factory.UceMalformedPacketHResult;
        MilBrushMappingMode viewportUnits = MemoryMarshal.Read<MilBrushMappingMode>(value[100..]); MilBrushMappingMode viewboxUnits = MemoryMarshal.Read<MilBrushMappingMode>(value[104..]); MilStretch stretch = MemoryMarshal.Read<MilStretch>(value[116..]); MilTileMode tileMode = MemoryMarshal.Read<MilTileMode>(value[120..]); MilHorizontalAlignment x = MemoryMarshal.Read<MilHorizontalAlignment>(value[124..]); MilVerticalAlignment y = MemoryMarshal.Read<MilVerticalAlignment>(value[128..]); MilCachingHint caching = MemoryMarshal.Read<MilCachingHint>(value[132..]);
        GeneratedProtocolResource?[] dependencies = new GeneratedProtocolResource?[6];
        if (!Valid(viewportUnits) || !Valid(viewboxUnits) || !Valid(stretch) || !Valid(tileMode) || !Valid(x) || !Valid(y) || !Valid(caching)
            || !TryResolve(table, MemoryMarshal.Read<uint>(value[88..]), MilResourceType.DoubleResource, out dependencies[0]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[92..]), IsTransform, out dependencies[1]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[96..]), IsTransform, out dependencies[2])
            || !TryResolve(table, MemoryMarshal.Read<uint>(value[108..]), MilResourceType.RectResource, out dependencies[3]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[112..]), MilResourceType.RectResource, out dependencies[4]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[136..]), _sourceType, out dependencies[5])) return Direct3D9Factory.UceMalformedPacketHResult;
        return CommitDependencies(dependencies, () => { TileMode = tileMode; Source = dependencies[5]; });
    }
}

internal sealed class GeneratedImageBrushResource : GeneratedTileBrushResource { internal GeneratedImageBrushResource() : base(MilResourceType.ImageBrush, IsImageSource) { } }
internal sealed class GeneratedDrawingBrushResource : GeneratedTileBrushResource { internal GeneratedDrawingBrushResource() : base(MilResourceType.DrawingBrush, IsDrawing) { } }
internal sealed class GeneratedVisualBrushResource : GeneratedTileBrushResource { internal GeneratedVisualBrushResource() : base(MilResourceType.VisualBrush, static type => type == MilResourceType.Visual) { } }

internal sealed class GeneratedBitmapCacheBrushResource : GeneratedBrushResource
{
    internal GeneratedBitmapCacheBrushResource() : base(MilResourceType.BitmapCacheBrush) { }
    internal GeneratedProtocolResource? BitmapCache { get; private set; }
    internal GeneratedProtocolResource? InternalTarget { get; private set; }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length != 28) return Direct3D9Factory.UceMalformedPacketHResult; GeneratedProtocolResource?[] dependencies = new GeneratedProtocolResource?[5];
        if (!TryResolve(table, MemoryMarshal.Read<uint>(value[8..]), MilResourceType.DoubleResource, out dependencies[0]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[12..]), IsTransform, out dependencies[1]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[16..]), IsTransform, out dependencies[2]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[20..]), MilResourceType.BitmapCache, out dependencies[3]) || !TryResolve(table, MemoryMarshal.Read<uint>(value[24..]), MilResourceType.Visual, out dependencies[4])) return Direct3D9Factory.UceMalformedPacketHResult;
        return CommitDependencies(dependencies, () => { BitmapCache = dependencies[3]; InternalTarget = dependencies[4]; });
    }
}
