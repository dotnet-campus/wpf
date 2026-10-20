using System.Numerics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal static class GeneratedHardwareRectangle
{
    internal static int Draw(GeneratedRenderDataInstruction instruction, Direct3D9Device device,
        Direct3D9SurfaceRenderTarget target, GeneratedImageTransform transform,
        Direct3D9SurfaceRect? clip, MilCompositingMode compositing)
    {
        bool rounded = instruction.Kind is GeneratedRenderDataKind.DrawRoundedRectangle or GeneratedRenderDataKind.DrawRoundedRectangleAnimate;
        bool animated = instruction.Kind is GeneratedRenderDataKind.DrawRectangleAnimate or GeneratedRenderDataKind.DrawRoundedRectangleAnimate;
        if (instruction.ResourceSlots.Count < (animated ? rounded ? 5 : 3 : 2)
            || instruction.Data.Length < (rounded ? 48 : 32))
            return Direct3D9Factory.UceMalformedPacketHResult;
        var rectangle = MemoryMarshal.Read<MilRectD>(instruction.Data.Span);
        if (animated
            && instruction.ResourceSlots.Count > 2
            && instruction.ResourceSlots[2] is GeneratedValueResource<MilRectD> animation)
            rectangle = animation.Value;
        if (!(rectangle.Width >= 0) || !(rectangle.Height >= 0)) return 0;
        var fill = instruction.ResourceSlots[0];
        var pen = instruction.ResourceSlots[1] as GeneratedPenResource;
        bool stroke = pen?.HasNonEmptyStroke == true;
        if (fill is null && !stroke) return 0;
        if (rounded)
        {
            double radiusX = MemoryMarshal.Read<double>(instruction.Data.Span[32..]);
            double radiusY = MemoryMarshal.Read<double>(instruction.Data.Span[40..]);
            if (animated)
            {
                if (instruction.ResourceSlots[3] is GeneratedValueResource<double> x) radiusX = x.Value;
                if (instruction.ResourceSlots[4] is GeneratedValueResource<double> y) radiusY = y.Value;
            }
            // A zero axis makes the rounded rectangle exactly rectangular; do not discard nonzero corners.
            if (!double.IsFinite(radiusX) || !double.IsFinite(radiusY))
                return Direct3D9Factory.NotImplementedHResult;
            if (radiusX != 0 && radiusY != 0)
                return WindowRoundedRectangleGeometry.Draw(instruction, rectangle, radiusX, radiusY,
                    device, target, transform, clip, compositing);
        }
        if (compositing is not (MilCompositingMode.SourceOver or MilCompositingMode.SourceCopy)
            || transform.M12 != 0 || transform.M21 != 0)
            return Direct3D9Factory.NotImplementedHResult;
        if (!TryColor(fill, out uint fillColor)
            || (stroke && (!TryColor(pen!.Brush, out _)
                || pen.LineJoin != MilPenJoin.Miter || pen.MiterLimit < Math.Sqrt(2)
                || pen.DashStyle is { Dashes.Length: > 0 })))
            return Direct3D9Factory.NotImplementedHResult;
        double radius = stroke ? Math.Abs((float)pen!.CurrentThickness) / 2 : 0;
        if (!Aligned(rectangle.X - radius, rectangle.Y - radius)
            || !Aligned(rectangle.X + rectangle.Width + radius, rectangle.Y + rectangle.Height + radius)
            || !Aligned(rectangle.X + radius, rectangle.Y + radius)
            || !Aligned(rectangle.X + rectangle.Width - radius, rectangle.Y + rectangle.Height - radius))
            return Direct3D9Factory.NotImplementedHResult;
        int result = 0;
        if (fill is not null && rectangle.Width > 0 && rectangle.Height > 0)
            result = Submit(rectangle, fillColor);
        if (result < 0 || !stroke) return result;
        TryColor(pen!.Brush, out uint strokeColor);
        double left = rectangle.X - radius, top = rectangle.Y - radius;
        double width = rectangle.Width + radius * 2, height = rectangle.Height + radius * 2;
        double innerWidth = rectangle.Width - radius * 2, innerHeight = rectangle.Height - radius * 2;
        if (innerWidth <= 0 || innerHeight <= 0)
            return Submit(new(left, top, width, height), strokeColor);
        result = Submit(new(left, top, width, radius * 2), strokeColor);
        if (result >= 0) result = Submit(new(left, rectangle.Y + rectangle.Height - radius, width, radius * 2), strokeColor);
        if (result >= 0) result = Submit(new(left, rectangle.Y + radius, radius * 2, innerHeight), strokeColor);
        if (result >= 0) result = Submit(new(rectangle.X + rectangle.Width - radius, rectangle.Y + radius, radius * 2, innerHeight), strokeColor);
        return result;

        bool Aligned(double x, double y)
        {
            var point = transform.Apply(x, y);
            return double.IsFinite(point.X) && double.IsFinite(point.Y)
                && point.X == Math.Round(point.X) && point.Y == Math.Round(point.Y);
        }

        int Submit(MilRectD bounds, uint color)
        {
            MilPoint2D a = new(bounds.X, bounds.Y), b = new(bounds.X + bounds.Width, bounds.Y);
            MilPoint2D c = new(bounds.X, bounds.Y + bounds.Height), d = new(bounds.X + bounds.Width, bounds.Y + bounds.Height);
            using var source = new Direct3D9ConstantColorSource(new(1, 1, 1, 1));
            using var operations = new GeneratedHardwareBitmapOperations(device, target, source.PipelineColorSource,
                transform, clip ?? new(0, 0, int.MaxValue, int.MaxValue), compositing,
                [a, b, c, c, b, d], color, solid: true);
            return target.ProductionDrawBitmap(new Direct3D9BitmapDrawState(Matrix4x4.Identity, new(0, 0, 1, 1)),
                operations.BitmapSource, 0, operations.Operations);
        }
    }

    internal static bool TryColor(GeneratedProtocolResource? brush, out uint color)
    {
        color = 0;
        if (brush is null) return true;
        if (brush is not GeneratedSolidColorBrushResource solid) return false;
        var value = solid.CurrentValue;
        if (!double.IsFinite(value.Opacity) || !float.IsFinite(value.Color.Alpha)
            || !float.IsFinite(value.Color.Red) || !float.IsFinite(value.Color.Green) || !float.IsFinite(value.Color.Blue))
            return false;
        color = Direct3D9ConstantColorSource.ConvertToPremultipliedSrgb(new(
            (float)(Math.Clamp(value.Opacity, 0, 1) * Math.Clamp(value.Color.Alpha, 0, 1)),
            value.Color.Red, value.Color.Green, value.Color.Blue));
        return true;
    }
}
