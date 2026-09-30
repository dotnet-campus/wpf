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

    private static bool Visit(GeneratedVisualResource visual, GeneratedImageTransform transform, ref MilRectD? bounds, int depth)
    {
        if (depth > 256 || visual.Clip is not null) return false;
        if (visual.Content is GeneratedRenderDataResource data)
        {
            var stack = new Stack<GeneratedImageTransform>();
            var current = transform;
            foreach (var instruction in data.Instructions)
            {
                switch (instruction.Kind)
                {
                    case GeneratedRenderDataKind.PushTransform:
                        if (!GeneratedImageTransform.TryResolve(instruction.ResourceSlots[0], out var pushed)) return false;
                        stack.Push(current); current = current.Prepend(pushed); break;
                    case GeneratedRenderDataKind.PushOpacity:
                    case GeneratedRenderDataKind.PushOpacityAnimate:
                    case GeneratedRenderDataKind.PushOpacityMask:
                        stack.Push(current); break;
                    case GeneratedRenderDataKind.Pop:
                        if (!stack.TryPop(out current)) return false;
                        break;
                    case GeneratedRenderDataKind.DrawImage:
                    case GeneratedRenderDataKind.DrawImageAnimate:
                        if (instruction.ResourceSlots[0] is null) break;
                        if (instruction.ResourceSlots[0] is not (GeneratedBitmapSourceResource or GeneratedDoubleBufferedBitmapResource)) return false;
                        var rect = MemoryMarshal.Read<MilRectD>(instruction.Data.Span);
                        if (instruction.Kind == GeneratedRenderDataKind.DrawImageAnimate
                            && instruction.ResourceSlots[1] is GeneratedValueResource<MilRectD> animated) rect = animated.Value;
                        if (!Accumulate(rect, current, ref bounds)) return false;
                        break;
                    default: return false;
                }
            }
            if (stack.Count != 0) return false;
        }
        else if (visual.Content is not null) return false;
        foreach (var child in visual.Children)
        {
            if (!GeneratedImageTransform.TryResolve(child.Transform, out var local)) return false;
            var childTransform = transform.Prepend(new(1, 1, child.Offset.X, child.Offset.Y)).Prepend(local);
            if (!Visit(child, childTransform, ref bounds, depth + 1)) return false;
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
