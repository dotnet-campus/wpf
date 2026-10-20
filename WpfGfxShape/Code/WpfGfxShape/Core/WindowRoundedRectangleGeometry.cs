using System.Numerics;

namespace WpfGfxShape.Core;

internal static class WindowRoundedRectangleGeometry
{
    internal static int Draw(GeneratedRenderDataInstruction instruction, MilRectD rectangle,
        double radiusX, double radiusY, Direct3D9Device device, Direct3D9SurfaceRenderTarget target,
        GeneratedImageTransform transform, Direct3D9SurfaceRect? clip, MilCompositingMode compositing)
    {
        // Coverage is generated in device space and submitted as disjoint GPU geometry, not a bitmap upload.
        if (compositing != MilCompositingMode.SourceOver || clip is not { } viewport
            || transform.ScaleX != 1 || transform.ScaleY != 1 || transform.M12 != 0 || transform.M21 != 0
            || !double.IsFinite(transform.X) || !double.IsFinite(transform.Y)
            || !float.IsFinite((float)rectangle.X) || !float.IsFinite((float)rectangle.Y)
            || !float.IsFinite((float)rectangle.Width) || !float.IsFinite((float)rectangle.Height))
            return Direct3D9Factory.NotImplementedHResult;
        var fill = instruction.ResourceSlots[0];
        var pen = instruction.ResourceSlots[1] as GeneratedPenResource;
        bool stroke = pen?.HasNonEmptyStroke == true;
        if (!GeneratedHardwareRectangle.TryColor(fill, out uint fillColor)
            || (fill is not null && (fillColor >> 24) != 255)
            || (stroke && (!GeneratedHardwareRectangle.TryColor(pen!.Brush, out uint penColor)
                || (penColor >> 24) != 255 || !float.IsFinite((float)pen.CurrentThickness))))
            return Direct3D9Factory.NotImplementedHResult;
        var geometry = GeneratedRectangleGeometryResource.CreateStatic(rectangle, radiusX, radiusY);
        SoftwareImageCoverage? fillCoverage = null, strokeCoverage = null;
        if (fill is not null)
        {
            if (!SoftwarePathCoverage.TryCreateRoundedRectangle(geometry, transform, out var path) || path is null)
                return Direct3D9Factory.NotImplementedHResult;
            fillCoverage = new(path);
        }
        if (stroke && !GeneratedStrokeGeometry.TryCreate(geometry, pen!, transform, out strokeCoverage, out _))
            return Direct3D9Factory.NotImplementedHResult;
        if (!GeneratedGeometryBounds.TryGet(geometry, transform, out var bounds))
            return Direct3D9Factory.NotImplementedHResult;
        if (stroke)
        {
            if (!GeneratedStrokeGeometry.TryGetBounds(geometry, pen!, transform, out var strokeBounds))
                return Direct3D9Factory.NotImplementedHResult;
            double unionLeft = Math.Min(bounds.X, strokeBounds.X), unionTop = Math.Min(bounds.Y, strokeBounds.Y);
            bounds = new(unionLeft, unionTop,
                Math.Max(bounds.X + bounds.Width, strokeBounds.X + strokeBounds.Width) - unionLeft,
                Math.Max(bounds.Y + bounds.Height, strokeBounds.Y + strokeBounds.Height) - unionTop);
        }
        double left = Math.Max(viewport.Left, Math.Floor(bounds.X));
        double top = Math.Max(viewport.Top, Math.Floor(bounds.Y));
        double right = Math.Min(viewport.Right, Math.Ceiling(bounds.X + bounds.Width));
        double bottom = Math.Min(viewport.Bottom, Math.Ceiling(bounds.Y + bounds.Height));
        if (right <= left || bottom <= top) return 0;
        if (left < int.MinValue || top < int.MinValue || right >= int.MaxValue || bottom >= int.MaxValue)
            return Direct3D9Factory.NotImplementedHResult;
        int result = Submit(fillCoverage, fillColor);
        if (result < 0 || !stroke) return result;
        GeneratedHardwareRectangle.TryColor(pen!.Brush, out uint strokeColor);
        return Submit(strokeCoverage, strokeColor);

        int Submit(SoftwareImageCoverage? coverage, uint color)
        {
            if (coverage is null) return 0;
            using var source = new Direct3D9ConstantColorSource(new(1, 1, 1, 1));
            for (int y = (int)top; y < (int)bottom; y++)
            {
                int x = (int)left;
                while (x < (int)right)
                {
                    int start = x, samples = coverage.GetCoverage(x++, y);
                    while (x < (int)right && coverage.GetCoverage(x, y) == samples) x++;
                    if (samples == 0) continue;
                    uint alpha = (uint)((samples * 255 + 32) / 64);
                    uint shaded = 0;
                    for (int shift = 0; shift < 32; shift += 8)
                        shaded |= ((((color >> shift) & 255) * alpha + 127) / 255) << shift;
                    MilPoint2D a = new(start, y), b = new(x, y), c = new(start, y + 1), d = new(x, y + 1);
                    using var operations = new GeneratedHardwareBitmapOperations(device, target, source.PipelineColorSource,
                        GeneratedImageTransform.Identity, viewport, compositing, [a, b, c, c, b, d], shaded, solid: true);
                    int hr = target.ProductionDrawBitmap(new Direct3D9BitmapDrawState(Matrix4x4.Identity, new(0, 0, 1, 1)),
                        operations.BitmapSource, 0, operations.Operations);
                    if (hr < 0) return hr;
                }
            }
            return 0;
        }
    }
}
