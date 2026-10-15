using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal static class GeneratedImageContentBounds
{
    internal static bool TryGet(GeneratedVisualResource visual, out MilRectD bounds)
    {
        MilRectD? accumulated = null;
        bool success = Visit(visual, GeneratedImageTransform.Identity, ref accumulated, 0);
        bounds = accumulated ?? default;
        return success;
    }

    internal static bool TryGet(FrozenVisualTree visual, out MilRectD bounds)
    {
        MilRectD? accumulated = null;
        bool success = VisitFrozen(visual, GeneratedImageTransform.Identity, ref accumulated, 0);
        bounds = accumulated ?? default;
        return success;
    }

    private static bool VisitFrozen(FrozenVisualTree visual, GeneratedImageTransform transform,
        ref MilRectD? bounds, int depth)
    {
        if (depth > 256 || !ReadInstructions(visual.Instructions, 0, false, transform, ref bounds)) return false;
        foreach (var child in visual.Children)
        {
            if (child.Effect is not null || child.HasUnsupportedEffect) return false;
            MilRectD? childBounds = null;
            if (!VisitFrozen(child, GeneratedImageTransform.Identity, ref childBounds, depth + 1)
                || !TryClip(child.Clip?.Geometry, GeneratedImageTransform.Identity, null, out var childClip)) return false;
            var childTransform = transform.Prepend(new(1, 1, child.Offset.X, child.Offset.Y)).Prepend(child.Transform);
            if (childBounds is { } inner)
            {
                if (childClip is { } clipping) inner = Intersect(inner, clipping);
                if (!Accumulate(inner, childTransform, ref bounds)) return false;
            }
        }
        return true;
    }

    internal static int GetVisualOuterBounds(FrozenVisualTree visual, out MilRectD bounds)
    {
        bounds = default;
        if (visual.Effect is not null || visual.HasUnsupportedEffect || !TryGet(visual, out var inner)
            || !TryClip(visual.Clip?.Geometry, GeneratedImageTransform.Identity, null, out var clip))
            return Direct3D9Factory.NotImplementedHResult;
        if (clip is { } clipping) inner = Intersect(inner, clipping);
        MilRectD? outer = null;
        if (!Accumulate(inner, GeneratedImageTransform.Identity.Prepend(new(1, 1, visual.Offset.X, visual.Offset.Y))
            .Prepend(visual.Transform), ref outer)) return Direct3D9Factory.InvalidArgumentHResult;
        bounds = outer ?? default;
        return 0;
    }

    internal static int GetVisualOuterBounds(GeneratedVisualResource visual, out MilRectD bounds)
    {
        bounds = default;
        if (visual.Effect is not null || !TryGet(visual, out var inner)
            || !TryClip(visual.Clip, GeneratedImageTransform.Identity, null, out var clip))
            return Direct3D9Factory.NotImplementedHResult;
        if (!GeneratedImageTransform.TryResolve(visual.Transform, out var local))
            return Direct3D9Factory.NonInvertibleMatrixHResult;
        if (clip is { } clipping) inner = Intersect(inner, clipping);
        MilRectD? outer = null;
        if (!Accumulate(inner, GeneratedImageTransform.Identity.Prepend(new(1, 1, visual.Offset.X, visual.Offset.Y)).Prepend(local), ref outer))
            return Direct3D9Factory.InvalidArgumentHResult;
        bounds = outer ?? default;
        return 0;
    }

    internal static bool TryGetMaskScope(IReadOnlyList<GeneratedRenderDataInstruction> instructions, int start, out MilRectD bounds)
    {
        MilRectD? accumulated = null;
        bool success = ReadInstructions(instructions, start, true, GeneratedImageTransform.Identity, ref accumulated);
        bounds = accumulated ?? default;
        return success;
    }

    internal static bool TryGetDrawing(IReadOnlyList<GeneratedRenderDataInstruction> instructions, out MilRectD bounds)
    {
        MilRectD? accumulated = null;
        bool success = ReadInstructions(instructions, 0, false, GeneratedImageTransform.Identity, ref accumulated);
        bounds = accumulated ?? default;
        return success;
    }

    private static bool ReadInstructions(IReadOnlyList<GeneratedRenderDataInstruction> instructions, int start,
        bool stopAtPop, GeneratedImageTransform transform, ref MilRectD? bounds)
    {
        var stack = new Stack<(GeneratedImageTransform Transform, MilRectD? Clip)>();
        var current = transform;
        MilRectD? clip = null;
        for (int i = start; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            switch (instruction.Kind)
            {
                case GeneratedRenderDataKind.Pop:
                    if (stack.Count == 0) return stopAtPop;
                    (current, clip) = stack.Pop();
                    break;
                case GeneratedRenderDataKind.PushTransform:
                    if (!GeneratedImageTransform.TryResolve(instruction.ResourceSlots[0], out var pushed)) return false;
                    stack.Push((current, clip)); current = current.Prepend(pushed);
                    break;
                case GeneratedRenderDataKind.PushClip:
                    if (!TryClip(instruction.ResourceSlots[0], current, clip, out var nextClip)) return false;
                    stack.Push((current, clip)); clip = nextClip;
                    break;
                case GeneratedRenderDataKind.PushOpacity:
                case GeneratedRenderDataKind.PushOpacityAnimate:
                case GeneratedRenderDataKind.PushOpacityMask:
                    stack.Push((current, clip));
                    break;
                case GeneratedRenderDataKind.DrawImage:
                case GeneratedRenderDataKind.DrawImageAnimate:
                    if (instruction.ResourceSlots[0] is null) break;
                    if (instruction.ResourceSlots[0] is GeneratedDrawingImageResource drawingImage)
                    {
                        if (!GeneratedDrawingInstructions.TryRead(drawingImage.Drawing, out var drawingInstructions)
                            || !TryGetDrawing(drawingInstructions, out var content)) return false;
                        if (content.Width <= 0 || content.Height <= 0) break;
                    }
                    else if (instruction.ResourceSlots[0] is not (GeneratedBitmapSourceResource or GeneratedDoubleBufferedBitmapResource or GeneratedD3DImageResource)) return false;
                    var rect = MemoryMarshal.Read<MilRectD>(instruction.Data.Span);
                    if (instruction.Kind == GeneratedRenderDataKind.DrawImageAnimate
                        && instruction.ResourceSlots[1] is GeneratedValueResource<MilRectD> animated) rect = animated.Value;
                    // Native bounds targets intersect the transformed shape bounds with the clip bounds;
                    // exact geometry coverage is handled separately by the rasterizer.
                    MilRectD? imageBounds = null;
                    if (!Accumulate(rect, current, ref imageBounds)) return false;
                    if (imageBounds is { } image)
                    {
                        if (clip is { } clipping) image = Intersect(image, clipping);
                        if (!Accumulate(image, GeneratedImageTransform.Identity, ref bounds)) return false;
                    }
                    break;
                case GeneratedRenderDataKind.DrawGlyphRun:
                    if (instruction.ResourceSlots[0] is null
                        || instruction.ResourceSlots[1] is not GeneratedGlyphRunResource glyph) break;
                    var glyphBounds = current.Apply(glyph.Command.Bounds);
                    if (clip is { } glyphClip) glyphBounds = Intersect(glyphBounds, glyphClip);
                    if (!Accumulate(glyphBounds, GeneratedImageTransform.Identity, ref bounds)) return false;
                    break;
                case GeneratedRenderDataKind.DrawGeometry:
                    if (instruction.ResourceSlots[2] is not { } geometry) break;
                    if (instruction.ResourceSlots[1] is GeneratedPenResource { HasNonEmptyStroke: true } pen)
                    {
                        if (!GeneratedNativeStroke.TryCapture(geometry, pen, current, out var stroke)
                            || !stroke!.TryGetBounds(localSpace: false, out var strokeBounds)) return false;
                        if (clip is { } strokeClip) strokeBounds = Intersect(strokeBounds, strokeClip);
                        if (!Accumulate(strokeBounds, GeneratedImageTransform.Identity, ref bounds)) return false;
                    }
                    if (instruction.ResourceSlots[0] is null) break;
                    if (!GeneratedGeometryBounds.TryGet(geometry, current, out var geometryBounds)) return false;
                    if (clip is { } geometryClip) geometryBounds = Intersect(geometryBounds, geometryClip);
                    if (!Accumulate(geometryBounds, GeneratedImageTransform.Identity, ref bounds)) return false;
                    break;
                default: return false;
            }
        }
        return !stopAtPop && stack.Count == 0;
    }

    private static bool TryClip(GeneratedProtocolResource? resource, GeneratedImageTransform current,
        MilRectD? inherited, out MilRectD? clip)
    {
        clip = inherited;
        if (resource is null) return true;
        if (!GeneratedGeometryBounds.TryGet(resource, current, out var bounds)) return false;
        clip = inherited is { } previous ? Intersect(previous, bounds) : bounds;
        return true;
    }

    private static MilRectD Intersect(MilRectD a, MilRectD b)
    {
        double left = Math.Max(a.X, b.X), top = Math.Max(a.Y, b.Y);
        return new(left, top, Math.Max(0, Math.Min(a.X + a.Width, b.X + b.Width) - left),
            Math.Max(0, Math.Min(a.Y + a.Height, b.Y + b.Height) - top));
    }

    private static bool Visit(GeneratedVisualResource visual, GeneratedImageTransform transform, ref MilRectD? bounds, int depth)
    {
        if (depth > 256) return false;
        if (visual.Content is GeneratedRenderDataResource data)
        {
            if (!ReadInstructions(data.Instructions, 0, false, transform, ref bounds)) return false;
        }
        else if (visual.Content is not null)
        {
            if (!GeneratedDrawingInstructions.TryRead(visual.Content, out var instructions)
                || !ReadInstructions(instructions, 0, false, transform, ref bounds)) return false;
        }
        foreach (var child in visual.Children)
        {
            if (child.Effect is not null) return false;
            MilRectD? childBounds = null;
            if (!Visit(child, GeneratedImageTransform.Identity, ref childBounds, depth + 1)) return false;
            if (!TryClip(child.Clip, GeneratedImageTransform.Identity, null, out var childClip)) return false;
            if (!GeneratedImageTransform.TryResolve(child.Transform, out var local)) return false;
            var childTransform = transform.Prepend(new(1, 1, child.Offset.X, child.Offset.Y)).Prepend(local);
            if (childBounds is { } inner)
            {
                // Native child outer bounds apply its clip before its transform and offset.
                if (childClip is { } clipping) inner = Intersect(inner, clipping);
                if (!Accumulate(inner, childTransform, ref bounds)) return false;
            }
        }
        return true;
    }

    private static bool Accumulate(MilRectD rect, GeneratedImageTransform transform, ref MilRectD? bounds)
    {
        if (rect.Width <= 0 || rect.Height <= 0) return true;
        var a = transform.Apply(rect.X, rect.Y);
        var b = transform.Apply(rect.X + rect.Width, rect.Y);
        var c = transform.Apply(rect.X, rect.Y + rect.Height);
        var d = transform.Apply(rect.X + rect.Width, rect.Y + rect.Height);
        double left = Math.Min(Math.Min(a.X, b.X), Math.Min(c.X, d.X));
        double top = Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y));
        double right = Math.Max(Math.Max(a.X, b.X), Math.Max(c.X, d.X));
        double bottom = Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y));
        if (!double.IsFinite(left) || !double.IsFinite(top) || !double.IsFinite(right) || !double.IsFinite(bottom)) return false;
        if (bounds is { } previous)
        {
            left = Math.Min(left, previous.X); top = Math.Min(top, previous.Y);
            right = Math.Max(right, previous.X + previous.Width); bottom = Math.Max(bottom, previous.Y + previous.Height);
        }
        bounds = new(left, top, right - left, bottom - top);
        return double.IsFinite(right - left) && double.IsFinite(bottom - top);
    }
}
