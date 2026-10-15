using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal static unsafe class GeneratedVisualRenderer
{
    private readonly record struct ImageDraw(nint Source, MilRectD Rectangle, bool IsMilSource,
        SoftwareImageDrawingContext Context, int LayerAction = 0, double Opacity = 1, SoftwareImageCoverage? Mask = null, float? AlphaMask = null, GeneratedImageMask? ImageMask = null, SoftwareBrushRealization? Brush = null, SoftwareBrushRealization? StrokeBrush = null,
        SoftwareImageCoverage? StrokeMask = null, MilRectD? Bounds = null, GeneratedNativeStroke? NativeStroke = null)
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
                    3 => session!.DrawPath(draw.Brush, draw.Mask, draw.StrokeBrush, draw.StrokeMask,
                        draw.Bounds, draw.Context, draw.NativeStroke),
                    _ => session!.Draw(draw.Source, draw.Rectangle, draw.IsMilSource, draw.Context)
                };
                if (hr < 0)
                {
                    session.AbortLayers();
                    return hr;
                }
            }
            return 0;
        }
        catch (OverflowException) { return Direct3D9Factory.InvalidArgumentHResult; }
        finally
        {
            session?.Dispose();
            for (int i = draws.Count - 1; i >= 0; i--)
            {
                draws[i].StrokeBrush?.Dispose();
                draws[i].Brush?.Dispose();
                draws[i].ImageMask?.Dispose();
                Direct3D9Factory.Release(draws[i].Source);
            }
            root.Release();
        }
    }

    internal sealed class DrawingSnapshot : IDisposable
    {
        private readonly List<ImageDraw> _draws;
        internal MilRectD Bounds { get; }
        internal uint Width { get; }
        internal uint Height { get; }
        private bool _disposed;

        private DrawingSnapshot(List<ImageDraw> draws, MilRectD bounds, uint width, uint height)
        { _draws = draws; Bounds = bounds; Width = width; Height = height; }

        internal static bool TryCapture(GeneratedDrawingImageResource image, MilRectD viewbox, bool relativeViewbox,
            MilRectD viewport, GeneratedImageTransform deviceTransform, MilStretch stretch, out DrawingSnapshot? snapshot)
        {
            snapshot = null;
            if (!GeneratedDrawingInstructions.TryRead(image.Drawing, out var instructions)
                || !GeneratedImageContentBounds.TryGetDrawing(instructions, out var bounds)) return false;
            if (bounds.Width <= 0 || bounds.Height <= 0) return true;
            var box = relativeViewbox
                ? new MilRectD(bounds.X + viewbox.X * bounds.Width, bounds.Y + viewbox.Y * bounds.Height,
                    viewbox.Width * bounds.Width, viewbox.Height * bounds.Height) : viewbox;
            double sx = viewport.Width / box.Width, sy = viewport.Height / box.Height;
            switch (stretch)
            {
                case MilStretch.None: sx = sy = 1; break;
                case MilStretch.Uniform: sx = sy = Math.Min(sx, sy); break;
                case MilStretch.UniformToFill: sx = sy = Math.Max(sx, sy); break;
            }
            double xLength = Math.Sqrt(deviceTransform.ScaleX * deviceTransform.ScaleX + deviceTransform.M12 * deviceTransform.M12);
            double yLength = Math.Sqrt(deviceTransform.M21 * deviceTransform.M21 + deviceTransform.ScaleY * deviceTransform.ScaleY);
            double w = Math.Ceiling(bounds.Width * sx * xLength), h = Math.Ceiling(bounds.Height * sy * yLength);
            if (!double.IsFinite(w) || !double.IsFinite(h) || w > int.MaxValue || h > int.MaxValue) return false;
            var draws = new List<ImageDraw>();
            var candidate = new DrawingSnapshot(draws, bounds, (uint)Math.Max(1, w), (uint)Math.Max(1, h));
            try
            {
                var mapping = new GeneratedImageTransform(candidate.Width / bounds.Width, candidate.Height / bounds.Height,
                    -bounds.X * candidate.Width / bounds.Width, -bounds.Y * candidate.Height / bounds.Height);
                if (CaptureInstructions(instructions, draws, mapping, null, 0, MilCompositingMode.SourceOver, 0) < 0) return false;
                snapshot = candidate;
                return true;
            }
            finally { if (snapshot is null) candidate.Dispose(); }
        }

        internal int Realize(out nint bitmap)
        {
            bitmap = 0;
            if (_disposed) return Direct3D9Factory.WgxInvalidCallHResult;
            nint target = 0, candidate = 0;
            SoftwareImageRenderSession? session = null;
            try
            {
                int hr = WpfGfxShape.Abi.BitmapRenderTargetExports.CreateDrawingIntermediate(Width, Height, out target, out candidate);
                if (hr < 0) return hr;
                hr = SoftwareImageRenderSession.Open(target, Width, Height, out session);
                if (hr < 0) return hr;
                foreach (var draw in _draws)
                {
                    hr = draw.LayerAction switch
                    {
                        1 => session!.BeginLayer(draw.Opacity, draw.Mask, draw.AlphaMask, draw.ImageMask),
                        2 => session!.EndLayer(),
                        3 => session!.DrawPath(draw.Brush, draw.Mask, draw.StrokeBrush, draw.StrokeMask, draw.Bounds, draw.Context, draw.NativeStroke),
                        _ => session!.Draw(draw.Source, draw.Rectangle, draw.IsMilSource, draw.Context)
                    };
                    if (hr < 0) { session.AbortLayers(); return hr; }
                }
                bitmap = candidate; candidate = 0;
                return 0;
            }
            finally { session?.Dispose(); Direct3D9Factory.Release(candidate); Direct3D9Factory.Release(target); }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            for (int i = _draws.Count - 1; i >= 0; i--)
            {
                _draws[i].StrokeBrush?.Dispose(); _draws[i].Brush?.Dispose();
                _draws[i].ImageMask?.Dispose(); Direct3D9Factory.Release(_draws[i].Source);
            }
            _draws.Clear();
        }
    }

    private static int CaptureMaskLayer(List<ImageDraw> draws, GeneratedProtocolResource? brush,
        GeneratedImageTransform transform, uint scalingMode, double opacity, MilRectD? contentBounds = null)
    {
        draws.EnsureCapacity(draws.Count + 1);
        if (brush is GeneratedImageBrushResource image)
        {
            if (!GeneratedImageMask.TryCapture(image, transform, scalingMode, out var mask, contentBounds)) return Direct3D9Factory.NotImplementedHResult;
            draws.Add(mask is null ? ImageDraw.BeginLayer(opacity, alphaMask: 0) : ImageDraw.BeginLayer(opacity, imageMask: mask));
            return 0;
        }
        if (!TrySolidMask(brush, out var alpha)) return Direct3D9Factory.NotImplementedHResult;
        draws.Add(ImageDraw.BeginLayer(opacity, alphaMask: alpha));
        return 0;
    }

    private static int CaptureGeometryBrush(List<ImageDraw> draws, GeneratedProtocolResource brush,
        GeneratedImageTransform transform, Direct3D9SurfaceRect? clip, uint scalingMode, MilCompositingMode compositing,
        SoftwareImageCoverage? coverage, MilRectD localBounds, bool appendStroke = false)
    {
        if (localBounds.Width <= 0 || localBounds.Height <= 0) return 0;
        draws.EnsureCapacity(draws.Count + 1);
        int result = SoftwareBrushRealization.Capture(brush, transform, scalingMode, localBounds, out var realization);
        if (result < 0) return result;
        MilRectD bounds = transform.Apply(localBounds);
        if (appendStroke)
        {
            var fill = draws[^1];
            if (fill.Bounds is { } first)
            {
                double left = Math.Min(first.X, bounds.X), top = Math.Min(first.Y, bounds.Y);
                bounds = new(left, top, Math.Max(first.X + first.Width, bounds.X + bounds.Width) - left,
                    Math.Max(first.Y + first.Height, bounds.Y + bounds.Height) - top);
            }
            draws[^1] = fill with { StrokeBrush = realization, StrokeMask = coverage, Bounds = bounds };
        }
        else
        {
            draws.Add(new(0, default, false, new(GeneratedImageTransform.Identity, clip, scalingMode, compositing,
                Antialias: true), 3, Mask: coverage, Brush: realization, Bounds: bounds));
        }
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
            int result = CaptureInstructions(data.Instructions, draws, transform, visualClip, scalingMode, compositing, depth);
            if (result < 0) return result;
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

    private static int CaptureInstructions(IReadOnlyList<GeneratedRenderDataInstruction> instructions,
        List<ImageDraw> draws, GeneratedImageTransform transform, Direct3D9SurfaceRect? visualClip,
        uint scalingMode, MilCompositingMode compositing, int depth)
    {
        if (depth > 256) return Direct3D9Factory.NotImplementedHResult;
        {
            var states = new Stack<(bool Opacity, GeneratedImageTransform Transform, Direct3D9SurfaceRect? Clip)>();
            GeneratedImageTransform current = transform;
            Direct3D9SurfaceRect? currentClip = visualClip;
            for (int instructionIndex = 0; instructionIndex < instructions.Count; instructionIndex++)
            {
                GeneratedRenderDataInstruction instruction = instructions[instructionIndex];
                if (instruction.Kind == GeneratedRenderDataKind.PushOpacityMask)
                {
                    MilRectD? scopeBounds = null;
                    if (instruction.ResourceSlots[0] is GeneratedImageBrushResource maskBrush
                        && (maskBrush.ViewportUnits == MilBrushMappingMode.RelativeToBoundingBox || maskBrush.RelativeTransform is not null))
                    {
                        if (!GeneratedImageContentBounds.TryGetMaskScope(instructions, instructionIndex + 1, out var bounds))
                            return Direct3D9Factory.NotImplementedHResult;
                        scopeBounds = bounds;
                    }
                    int hr = CaptureMaskLayer(draws, instruction.ResourceSlots[0], current, scalingMode, 1, scopeBounds);
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
                if (instruction.Kind == GeneratedRenderDataKind.DrawGeometry)
                {
                    if (instruction.ResourceSlots[2] is not { } geometry) continue;
                    if (instruction.ResourceSlots[1] is GeneratedPenResource { HasNonEmptyStroke: true } strokePen)
                    {
                        int firstDraw = draws.Count;
                        bool hasNativeStroke = GeneratedNativeStroke.TryCapture(geometry, strokePen, current, out var nativeStroke);
                        if (!hasNativeStroke)
                            return Direct3D9Factory.NotImplementedHResult;
                        if (instruction.ResourceSlots[0] is { } fillBrush)
                        {
                            SoftwareImageCoverage? fillCoverage = null;
                            if (!GeneratedGeometryBounds.TryGet(geometry, GeneratedImageTransform.Identity, out var fillBounds, current)
                                || (!hasNativeStroke && !GeneratedImageClip.TryCreateMask(geometry, current, out fillCoverage)))
                                return Direct3D9Factory.NotImplementedHResult;
                            int fillResult = CaptureGeometryBrush(draws, fillBrush, current, currentClip, scalingMode, compositing, fillCoverage, fillBounds);
                            if (fillResult < 0) return fillResult;
                        }
                        SoftwareImageCoverage? strokeCoverage = null;
                        if ((!hasNativeStroke && !GeneratedStrokeGeometry.TryCreate(geometry, strokePen, current, out strokeCoverage, out _))
                            || !nativeStroke!.TryGetBounds(localSpace: true, out var strokeLocalBounds))
                            return Direct3D9Factory.NotImplementedHResult;
                        int strokeResult = CaptureGeometryBrush(draws, strokePen.Brush!, current, currentClip, scalingMode, compositing, strokeCoverage, strokeLocalBounds, appendStroke: draws.Count > firstDraw);
                        if (strokeResult < 0) return strokeResult;
                        if (draws.Count > firstDraw && hasNativeStroke)
                        {
                            var draw = draws[^1];
                            draws[^1] = draw.StrokeBrush is null
                                ? draw with { NativeStroke = nativeStroke, StrokeBrush = draw.Brush,
                                    Brush = null, StrokeMask = null, Mask = null }
                                : draw with { NativeStroke = nativeStroke, StrokeMask = null,
                                    Mask = nativeStroke!.Fillable ? null : draw.Mask };
                        }
                        continue;
                    }
                    if (instruction.ResourceSlots[0] is null) continue;
                    var fill = instruction.ResourceSlots[0];
                    bool hasNativeFill = GeneratedNativeStroke.TryCaptureFill(geometry, current, out var nativeFill);
                    if (!hasNativeFill)
                        return Direct3D9Factory.NotImplementedHResult;
                    SoftwareImageCoverage? coverage = null;
                    if (fill is not GeneratedImageBrushResource and not GeneratedSolidColorBrushResource
                        || !GeneratedGeometryBounds.TryGet(geometry, GeneratedImageTransform.Identity, out var localBounds, current)
                        || !GeneratedGeometryBounds.TryGet(geometry, current, out var deviceBounds)
                        || (!hasNativeFill && !GeneratedImageClip.TryCreateMask(geometry, current, out coverage)))
                        return Direct3D9Factory.NotImplementedHResult;
                    if (localBounds.Width <= 0 || localBounds.Height <= 0) continue;
                    int firstFill = draws.Count;
                    int result = CaptureGeometryBrush(draws, fill, current, currentClip, scalingMode,
                        compositing, coverage, localBounds);
                    if (result < 0) return result;
                    if (draws.Count > firstFill && hasNativeFill)
                        draws[^1] = draws[^1] with { NativeStroke = nativeFill, Mask = null };
                    continue;
                }
                if (instruction.Kind is not (GeneratedRenderDataKind.DrawImage or GeneratedRenderDataKind.DrawImageAnimate))
                    return Direct3D9Factory.NotImplementedHResult;
                GeneratedProtocolResource? image = instruction.ResourceSlots[0];
                if (image is null) continue;
                MilRectD rectangle = MemoryMarshal.Read<MilRectD>(instruction.Data.Span);
                if (instruction.Kind == GeneratedRenderDataKind.DrawImageAnimate
                    && instruction.ResourceSlots[1] is GeneratedValueResource<MilRectD> animatedRectangle)
                    rectangle = animatedRectangle.Value;
                if (image is GeneratedDrawingImageResource drawingImage)
                {
                    if (!double.IsFinite(rectangle.X) || !double.IsFinite(rectangle.Y)
                        || !double.IsFinite(rectangle.Width) || !double.IsFinite(rectangle.Height))
                        return Direct3D9Factory.InvalidArgumentHResult;
                    if (!GeneratedDrawingInstructions.TryRead(drawingImage.Drawing, out var drawingInstructions)
                        || !GeneratedImageContentBounds.TryGetDrawing(drawingInstructions, out var content))
                        return Direct3D9Factory.NotImplementedHResult;
                    if (rectangle.Width <= 0 || rectangle.Height <= 0 || content.Width <= 0 || content.Height <= 0) continue;
                    double sx = rectangle.Width / content.Width, sy = rectangle.Height / content.Height;
                    var mapping = current.Prepend(new(sx, sy, rectangle.X - content.X * sx, rectangle.Y - content.Y * sy));
                    if (!double.IsFinite(sx) || !double.IsFinite(sy)
                        || !double.IsFinite(mapping.ScaleX) || !double.IsFinite(mapping.ScaleY)
                        || !double.IsFinite(mapping.M12) || !double.IsFinite(mapping.M21)
                        || !double.IsFinite(mapping.X) || !double.IsFinite(mapping.Y))
                        return Direct3D9Factory.InvalidArgumentHResult;
                    int result = CaptureInstructions(drawingInstructions, draws, mapping, currentClip, scalingMode, compositing, depth + 1);
                    if (result < 0) return result;
                    continue;
                }
                if (image is not GeneratedDoubleBufferedBitmapResource and not GeneratedBitmapSourceResource)
                    return Direct3D9Factory.NotImplementedHResult;
                draws.EnsureCapacity(draws.Count + 1);
                nint bitmap = image is GeneratedDoubleBufferedBitmapResource source
                    ? source.AcquireBitmapSource() : ((GeneratedBitmapSourceResource)image).AcquireBitmapSource();
                if (bitmap != 0) draws.Add(new(bitmap, rectangle, image is GeneratedBitmapSourceResource,
                    new(current, currentClip, scalingMode, compositing)));
            }
        }
        return 0;
    }
}
