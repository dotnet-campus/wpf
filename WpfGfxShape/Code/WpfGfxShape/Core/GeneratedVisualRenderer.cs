using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal static unsafe class GeneratedVisualRenderer
{
    private readonly record struct ImageDraw(nint Source, MilRectD Rectangle, bool IsMilSource,
        SoftwareImageDrawingContext Context, int LayerAction = 0, double Opacity = 1, SoftwareImageCoverage? Mask = null, float? AlphaMask = null, GeneratedImageMask? ImageMask = null)
    {
        internal static ImageDraw BeginLayer(double opacity, SoftwareImageCoverage? mask = null, float? alphaMask = null, GeneratedImageMask? imageMask = null)
            => new(0, default, false, default, 1, opacity, mask, alphaMask, imageMask);
        internal static ImageDraw EndLayer => new(0, default, false, default, 2);
    }

    internal static int Render(GeneratedVisualResource root, nint target, uint width, uint height)
    {
        List<ImageDraw> draws = [];
        SoftwareImageRenderSession? session = null;
        root.AddRef();
        try
        {
            // Snapshot before invoking target COM: callbacks may replace or delete the scene.
            int hr = Capture(root, draws, GeneratedImageTransform.Identity, null, 0);
            if (hr < 0) return hr;
            hr = SoftwareImageRenderSession.Open(target, width, height, out session);
            if (hr < 0) return hr;
            foreach (ImageDraw draw in draws)
            {
                hr = draw.LayerAction switch
                {
                    1 => session!.BeginLayer(draw.Opacity, draw.Mask, draw.AlphaMask, draw.ImageMask),
                    2 => session!.EndLayer(),
                    _ => session!.Draw(draw.Source, draw.Rectangle, draw.IsMilSource, draw.Context)
                };
                if (hr < 0) return hr;
            }
            return 0;
        }
        catch (OverflowException) { return Direct3D9Factory.InvalidArgumentHResult; }
        finally
        {
            session?.Dispose();
            for (int i = draws.Count - 1; i >= 0; i--)
            {
                draws[i].ImageMask?.Dispose();
                Direct3D9Factory.Release(draws[i].Source);
            }
            root.Release();
        }
    }

    private static int CaptureMaskLayer(List<ImageDraw> draws, GeneratedProtocolResource? brush,
        GeneratedImageTransform transform, uint scalingMode, double opacity, MilRectD? contentBounds = null)
    {
        draws.EnsureCapacity(draws.Count + 1);
        if (brush is GeneratedImageBrushResource image)
        {
            if (!GeneratedImageMask.TryCapture(image, transform, scalingMode, out var mask, contentBounds)) return Direct3D9Factory.NotImplementedHResult;
            draws.Add(ImageDraw.BeginLayer(opacity, imageMask: mask));
            return 0;
        }
        if (!TrySolidMask(brush, out var alpha)) return Direct3D9Factory.NotImplementedHResult;
        draws.Add(ImageDraw.BeginLayer(opacity, alphaMask: alpha));
        return 0;
    }

    private static bool TrySolidMask(GeneratedProtocolResource? brush, out float? alpha)
    {
        alpha = null;
        if (brush is null) return true;
        if (brush is not GeneratedSolidColorBrushResource solid) return false;
        var value = solid.CurrentValue;
        if (!double.IsFinite(value.Opacity) || !float.IsFinite(value.Color.Alpha)) return false;
        alpha = (float)(Math.Clamp(value.Opacity, 0, 1) * Math.Clamp(value.Color.Alpha, 0, 1));
        return true;
    }

    private static int Capture(GeneratedVisualResource visual, List<ImageDraw> draws, GeneratedImageTransform parent, Direct3D9SurfaceRect? inheritedClip, int depth, uint inheritedScalingMode = 0, MilCompositingMode inheritedCompositing = MilCompositingMode.SourceOver)
    {
        if (depth > 256)
            return Direct3D9Factory.NotImplementedHResult;
        if (!double.IsFinite(visual.Alpha)) return Direct3D9Factory.NotImplementedHResult;
        uint scalingMode = visual.BitmapScalingMode ?? inheritedScalingMode;
        MilCompositingMode compositing = visual.CompositingMode ?? inheritedCompositing;

        if (!GeneratedImageTransform.TryResolve(visual.Transform, out GeneratedImageTransform local))
            return Direct3D9Factory.NotImplementedHResult;
        GeneratedImageTransform transform = parent.Prepend(new(1, 1, visual.Offset.X, visual.Offset.Y)).Prepend(local);
        bool hasLayer = visual.Alpha != 1 || visual.AlphaMask is not null;
        if (hasLayer)
        {
            MilRectD? contentBounds = null;
            if (visual.AlphaMask is GeneratedImageBrushResource imageBrush
                && (imageBrush.ViewportUnits == MilBrushMappingMode.RelativeToBoundingBox || imageBrush.RelativeTransform is not null))
            {
                if (!GeneratedImageContentBounds.TryGet(visual, out var bounds)) return Direct3D9Factory.NotImplementedHResult;
                contentBounds = bounds;
            }
            int hr = CaptureMaskLayer(draws, visual.AlphaMask, transform, scalingMode, Math.Clamp(visual.Alpha, 0, 1), contentBounds);
            if (hr < 0) return hr;
        }
        bool clipLayer = false;
        if (!GeneratedImageClip.TryIntersect(visual.Clip, transform, inheritedClip, out Direct3D9SurfaceRect? visualClip))
        {
            if (!GeneratedImageClip.TryCreateMask(visual.Clip, transform, out var mask))
                return Direct3D9Factory.NotImplementedHResult;
            visualClip = inheritedClip;
            draws.Add(ImageDraw.BeginLayer(1, mask));
            clipLayer = true;
        }
        if (visual.Content is GeneratedRenderDataResource data)
        {
            var states = new Stack<(bool Opacity, GeneratedImageTransform Transform, Direct3D9SurfaceRect? Clip)>();
            GeneratedImageTransform current = transform;
            Direct3D9SurfaceRect? currentClip = visualClip;
            foreach (GeneratedRenderDataInstruction instruction in data.Instructions)
            {
                if (instruction.Kind == GeneratedRenderDataKind.PushOpacityMask)
                {
                    int hr = CaptureMaskLayer(draws, instruction.ResourceSlots[0], current, scalingMode, 1);
                    if (hr < 0) return hr;
                    states.Push((true, current, currentClip));
                    continue;
                }
                if (instruction.Kind is GeneratedRenderDataKind.PushOpacity or GeneratedRenderDataKind.PushOpacityAnimate)
                {
                    double opacity = MemoryMarshal.Read<double>(instruction.Data.Span);
                    if (instruction.Kind == GeneratedRenderDataKind.PushOpacityAnimate
                        && instruction.ResourceSlots[0] is GeneratedValueResource<double> animatedOpacity)
                        opacity = animatedOpacity.Value;
                    if (!double.IsFinite(opacity)) return Direct3D9Factory.NotImplementedHResult;
                    states.Push((true, current, currentClip));
                    draws.Add(ImageDraw.BeginLayer(Math.Clamp(opacity, 0, 1)));
                    continue;
                }
                if (instruction.Kind == GeneratedRenderDataKind.PushTransform)
                {
                    if (!GeneratedImageTransform.TryResolve(instruction.ResourceSlots[0], out GeneratedImageTransform pushed))
                        return Direct3D9Factory.NotImplementedHResult;
                    states.Push((false, current, currentClip));
                    current = current.Prepend(pushed);
                    continue;
                }
                if (instruction.Kind == GeneratedRenderDataKind.PushClip)
                {
                    if (!GeneratedImageClip.TryIntersect(instruction.ResourceSlots[0], current, currentClip, out var nextClip))
                    {
                        if (!GeneratedImageClip.TryCreateMask(instruction.ResourceSlots[0], current, out var mask))
                            return Direct3D9Factory.NotImplementedHResult;
                        states.Push((true, current, currentClip));
                        draws.Add(ImageDraw.BeginLayer(1, mask));
                    }
                    else
                    {
                        states.Push((false, current, currentClip));
                        currentClip = nextClip;
                    }
                    continue;
                }
                if (instruction.Kind == GeneratedRenderDataKind.Pop)
                {
                    if (!states.TryPop(out var state)) return Direct3D9Factory.UceMalformedPacketHResult;
                    current = state.Transform;
                    currentClip = state.Clip;
                    if (state.Opacity) draws.Add(ImageDraw.EndLayer);
                    continue;
                }
                if (instruction.Kind is not (GeneratedRenderDataKind.DrawImage or GeneratedRenderDataKind.DrawImageAnimate))
                    return Direct3D9Factory.NotImplementedHResult;
                GeneratedProtocolResource? image = instruction.ResourceSlots[0];
                if (image is null) continue;
                if (image is not GeneratedDoubleBufferedBitmapResource and not GeneratedBitmapSourceResource)
                    return Direct3D9Factory.NotImplementedHResult;
                MilRectD rectangle = MemoryMarshal.Read<MilRectD>(instruction.Data.Span);
                if (instruction.Kind == GeneratedRenderDataKind.DrawImageAnimate
                    && instruction.ResourceSlots[1] is GeneratedValueResource<MilRectD> animatedRectangle)
                    rectangle = animatedRectangle.Value;
                draws.EnsureCapacity(draws.Count + 1);
                nint bitmap = image is GeneratedDoubleBufferedBitmapResource source
                    ? source.AcquireBitmapSource() : ((GeneratedBitmapSourceResource)image).AcquireBitmapSource();
                if (bitmap != 0) draws.Add(new(bitmap, rectangle, image is GeneratedBitmapSourceResource,
                    new(current, currentClip, scalingMode, compositing)));
            }
        }
        else if (visual.Content is not null) return Direct3D9Factory.NotImplementedHResult;
        foreach (GeneratedVisualResource child in visual.Children)
        {
            int result = Capture(child, draws, transform, visualClip, depth + 1, scalingMode, compositing);
            if (result < 0) return result;
        }
        if (clipLayer) draws.Add(ImageDraw.EndLayer);
        if (hasLayer) draws.Add(ImageDraw.EndLayer);
        return 0;
    }
}
