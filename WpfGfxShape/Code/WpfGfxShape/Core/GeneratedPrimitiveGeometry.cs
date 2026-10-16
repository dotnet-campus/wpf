using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal static class GeneratedPrimitiveGeometry
{
    internal static bool IsPrimitive(GeneratedRenderDataKind kind)
        => kind is >= GeneratedRenderDataKind.DrawLine and <= GeneratedRenderDataKind.DrawEllipseAnimate;

    internal static GeneratedProtocolResource? Create(GeneratedRenderDataInstruction instruction,
        out GeneratedProtocolResource? brush, out GeneratedProtocolResource? pen)
    {
        var data = instruction.Data.Span;
        var slots = instruction.ResourceSlots;
        var kind = instruction.Kind;
        brush = null;
        pen = slots[0];
        if (kind is GeneratedRenderDataKind.DrawLine or GeneratedRenderDataKind.DrawLineAnimate)
        {
            if (pen is not GeneratedPenResource { HasNonEmptyStroke: true }) return null;
            var start = MemoryMarshal.Read<MilPoint2D>(data);
            var end = MemoryMarshal.Read<MilPoint2D>(data[16..]);
            if (kind == GeneratedRenderDataKind.DrawLineAnimate)
            {
                start = Animated(slots[1], start);
                end = Animated(slots[2], end);
            }
            return GeneratedLineGeometryResource.CreateStatic(start, end);
        }

        brush = slots[0];
        pen = slots[1];
        if (brush is null && pen is not GeneratedPenResource { HasNonEmptyStroke: true }) return null;
        if (kind is GeneratedRenderDataKind.DrawEllipse or GeneratedRenderDataKind.DrawEllipseAnimate)
        {
            var center = MemoryMarshal.Read<MilPoint2D>(data);
            double radiusX = MemoryMarshal.Read<double>(data[16..]);
            double radiusY = MemoryMarshal.Read<double>(data[24..]);
            if (kind == GeneratedRenderDataKind.DrawEllipseAnimate)
            {
                center = Animated(slots[2], center);
                radiusX = Animated(slots[3], radiusX);
                radiusY = Animated(slots[4], radiusY);
            }
            return GeneratedEllipseGeometryResource.CreateStatic(center, radiusX, radiusY);
        }

        var rectangle = MemoryMarshal.Read<MilRectD>(data);
        double rx = 0, ry = 0;
        if (kind is GeneratedRenderDataKind.DrawRoundedRectangle or GeneratedRenderDataKind.DrawRoundedRectangleAnimate)
        {
            rx = MemoryMarshal.Read<double>(data[32..]);
            ry = MemoryMarshal.Read<double>(data[40..]);
        }
        if (kind is GeneratedRenderDataKind.DrawRectangleAnimate or GeneratedRenderDataKind.DrawRoundedRectangleAnimate)
            rectangle = Animated(slots[2], rectangle);
        if (kind == GeneratedRenderDataKind.DrawRoundedRectangleAnimate)
        {
            rx = Animated(slots[3], rx);
            ry = Animated(slots[4], ry);
        }
        // Native DrawRectangle does not normalize inverted or invalid rectangles.
        if (!(rectangle.Width >= 0) || !(rectangle.Height >= 0)) return null;
        return GeneratedRectangleGeometryResource.CreateStatic(rectangle, rx, ry);
    }

    private static T Animated<T>(GeneratedProtocolResource? resource, T value) where T : unmanaged
        => resource is GeneratedValueResource<T> animation ? animation.Value : value;
}
