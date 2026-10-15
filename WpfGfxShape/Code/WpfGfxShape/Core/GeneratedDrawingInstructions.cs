using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal static class GeneratedDrawingInstructions
{
    internal static bool TryRead(GeneratedProtocolResource? drawing, out GeneratedRenderDataInstruction[] instructions)
    {
        var result = new List<GeneratedRenderDataInstruction>();
        bool success = Append(drawing, result, new HashSet<GeneratedProtocolResource>());
        instructions = result.ToArray();
        return success;
    }

    private static GeneratedRenderDataInstruction Instruction(GeneratedRenderDataKind kind,
        GeneratedProtocolResource?[] slots, byte[]? data = null) =>
        new(kind, slots.OfType<GeneratedProtocolResource>().ToArray(), data ?? [], slots);

    private static byte[] Bytes<T>(T value) where T : unmanaged
    {
        var data = new byte[Marshal.SizeOf<T>()];
        MemoryMarshal.Write(data, in value);
        return data;
    }

    private static bool Append(GeneratedProtocolResource? drawing, List<GeneratedRenderDataInstruction> output,
        HashSet<GeneratedProtocolResource> active)
    {
        if (drawing is null) return true;
        if (drawing is GeneratedRenderDataResource frozen) { output.AddRange(frozen.Instructions); return true; }
        if (!active.Add(drawing) || active.Count > 256) return false;
        try
        {
            if (drawing is GeneratedGeometryDrawingResource geometry)
            {
                if (geometry.Slots.Count == 0) return true;
                output.Add(Instruction(GeneratedRenderDataKind.DrawGeometry, geometry.Slots.ToArray()));
                return true;
            }
            if (drawing is GeneratedGlyphRunDrawingResource glyphDrawing)
            {
                if (glyphDrawing.Slots.Count == 0) return true;
                output.Add(Instruction(GeneratedRenderDataKind.DrawGlyphRun, [glyphDrawing.Slots[1], glyphDrawing.Slots[0]]));
                return true;
            }
            if (drawing is GeneratedImageDrawingResource imageDrawing)
            {
                if (imageDrawing.Slots.Count == 0) return true;
                var rectangle = imageDrawing.Slots[1] is GeneratedValueResource<MilRectD> animatedRect
                    ? animatedRect.Value : imageDrawing.Rect;
                output.Add(Instruction(GeneratedRenderDataKind.DrawImage, [imageDrawing.Slots[0]], Bytes(rectangle)));
                return true;
            }
            if (drawing is GeneratedVideoDrawingResource videoDrawing)
            {
                if (videoDrawing.Slots.Count == 0) return true;
                var rectangle = videoDrawing.Slots[1] is GeneratedValueResource<MilRectD> animatedRect
                    ? animatedRect.Value : videoDrawing.Rect;
                output.Add(Instruction(GeneratedRenderDataKind.DrawVideo, [videoDrawing.Slots[0]], Bytes(rectangle)));
                return true;
            }
            if (drawing is not GeneratedDrawingGroupResource group) return false;
            var slots = group.Slots;
            if (slots.Count < 5 || slots[4] is not null || group.EdgeMode != MilEdgeMode.Unspecified
                || group.BitmapScalingMode != MilBitmapScalingMode.Unspecified
                || group.ClearTypeHint != MilClearTypeHint.Auto) return false;
            int pushes = 0;
            if (slots[3] is { } transform)
            {
                output.Add(Instruction(GeneratedRenderDataKind.PushTransform, [transform]));
                pushes++;
            }
            if (slots[0] is { } clip)
            {
                output.Add(Instruction(GeneratedRenderDataKind.PushClip, [clip]));
                pushes++;
            }
            double opacity = slots[1] is GeneratedValueResource<double> animated ? animated.Value : group.Opacity;
            if (!double.IsFinite(opacity)) return false;
            if (opacity != 1)
            {
                output.Add(Instruction(GeneratedRenderDataKind.PushOpacity, [], Bytes(opacity)));
                pushes++;
            }
            if (slots[2] is { } mask)
            {
                output.Add(Instruction(GeneratedRenderDataKind.PushOpacityMask, [mask]));
                pushes++;
            }
            foreach (var child in group.Children)
                if (!Append(child, output, active)) return false;
            for (int i = 0; i < pushes; i++) output.Add(Instruction(GeneratedRenderDataKind.Pop, []));
            return true;
        }
        finally { active.Remove(drawing); }
    }
}
