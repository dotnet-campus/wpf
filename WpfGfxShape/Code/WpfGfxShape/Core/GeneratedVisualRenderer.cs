using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal static unsafe class GeneratedVisualRenderer
{
    private readonly record struct ImageDraw(nint Source, MilRectD Rectangle, bool IsMilSource,
        SoftwareImageDrawingContext Context, int LayerAction = 0, double Opacity = 1, SoftwareImageCoverage? Mask = null, float? AlphaMask = null, GeneratedImageMask? ImageMask = null, SoftwareBrushRealization? Brush = null, SoftwareBrushRealization? StrokeBrush = null,
        SoftwareImageCoverage? StrokeMask = null, MilRectD? Bounds = null, GeneratedNativeStroke? NativeStroke = null, Av.SystemVideoSample? VideoSample = null, GeneratedSoftwareShaderEffect? Effect = null, GeneratedSoftwareBlurEffect? Blur = null)
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
            int hr = FrozenVisualTree.CapturePure(root, [], out var frozen);
            if (hr < 0) { PartitionScheduler.RecordCommandFailure(0, "FrozenVisualTree.CapturePure"u8, hr); return hr; }
            using var tree = frozen;
            hr = Capture(tree!, draws, GeneratedImageTransform.Identity, new(0, 0, checked((int)width), checked((int)height)), 0);
            if (hr < 0) { PartitionScheduler.RecordCommandFailure(0, "GeneratedVisualRenderer.Capture"u8, hr); return hr; }
            hr = SoftwareImageRenderSession.Open(target, width, height, out session);
            if (hr < 0) { PartitionScheduler.RecordCommandFailure(0, "SoftwareImageRenderSession.Open"u8, hr); return hr; }
            foreach (ImageDraw draw in draws)
            {
                hr = draw.LayerAction switch
                {
                    1 => session!.BeginLayer(draw.Opacity, draw.Mask, draw.AlphaMask, draw.ImageMask),
                    2 => session!.EndLayer(),
                    4 => draw.Effect!.Draw(session!, draw.Context),
                    5 => draw.Blur!.Draw(session!, draw.Context),
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
                draws[i].Effect?.Dispose();
                draws[i].Blur?.Dispose();
                draws[i].StrokeBrush?.Dispose();
                draws[i].Brush?.Dispose();
                draws[i].ImageMask?.Dispose();
                draws[i].VideoSample?.Dispose();
                Direct3D9Factory.Release(draws[i].Source);
            }
            root.Release();
        }
    }

    internal static int CaptureVisual(GeneratedVisualResource root, MilRectD outerBounds,
        GeneratedImageTransform outerToPixel, uint width, uint height, uint scalingMode, out VisualSnapshot? snapshot)
        => CaptureSnapshot(root, outerBounds, outerToPixel, width, height, scalingMode, false, out snapshot);

    internal static int CaptureCacheContents(GeneratedVisualResource root, MilRectD innerBounds,
        GeneratedImageTransform localToPixel, uint width, uint height, uint scalingMode, out VisualSnapshot? snapshot)
        => CaptureSnapshot(root, innerBounds, localToPixel, width, height, scalingMode, true, out snapshot);

    private static int CaptureSnapshot(GeneratedVisualResource root, MilRectD bounds,
        GeneratedImageTransform mapping, uint width, uint height, uint scalingMode, bool cacheContents,
        out VisualSnapshot? snapshot)
    {
        snapshot = null;
        if (width == 0 || height == 0 || width > int.MaxValue || height > int.MaxValue)
            return Direct3D9Factory.InvalidArgumentHResult;
        root.AddRef();
        try
        {
            int hr = DrawingSnapshot.CaptureTree(root, bounds, mapping, width, height, scalingMode, cacheContents, out var drawing);
            if (hr < 0) return hr;
            try { snapshot = new VisualSnapshot(drawing!); }
            catch { drawing!.Dispose(); throw; }
            return 0;
        }
        finally { root.Release(); }
    }

    internal sealed class VisualSnapshot : IDisposable
    {
        private readonly DrawingSnapshot _drawing;
        internal VisualSnapshot(DrawingSnapshot drawing) { _drawing = drawing; }
        internal MilRectD Bounds => _drawing.Bounds;
        internal uint Width => _drawing.Width;
        internal uint Height => _drawing.Height;
        internal int Realize(out nint bitmap, bool floating = true) => _drawing.Realize(out bitmap, floating);
        public void Dispose() => _drawing.Dispose();
    }

    internal sealed class DrawingSnapshot : IDisposable
    {
        private readonly List<ImageDraw> _draws;
        internal MilRectD Bounds { get; }
        internal uint Width { get; }
        internal uint Height { get; }
        internal GeneratedImageTransform PixelToDevice { get; private set; }
        internal bool IsCompleteViewportTile { get; private set; }
        internal bool IsDeviceAligned { get; private set; }
        internal bool IsCroppedViewportTile { get; private set; }
        private bool _disposed;

        private DrawingSnapshot(List<ImageDraw> draws, MilRectD bounds, uint width, uint height)
        { _draws = draws; Bounds = bounds; Width = width; Height = height; }

        internal static bool TryCapture(GeneratedProtocolResource image, MilRectD viewbox, bool relativeViewbox,
            MilRectD viewport, GeneratedImageTransform deviceTransform, MilStretch stretch,
            MilHorizontalAlignment alignmentX, MilVerticalAlignment alignmentY, MilTileMode tileMode,
            Direct3D9SurfaceRect? deviceClip, uint scalingMode, double viewportScaleX, double viewportScaleY,
            out DrawingSnapshot? snapshot, bool deviceAligned = false, MilRectD? renderBounds = null,
            double brushScaleX = 1, double brushScaleY = 1, double worldScaleX = 1, double worldScaleY = 1,
            GeneratedImageTransform? worldTransform = null, MilRectD? worldBounds = null, FrozenVisualTree? frozen = null)
        {
            snapshot = null;
            GeneratedRenderDataInstruction[] instructions;
            bool read = frozen is not null ? (instructions = frozen.Instructions.ToArray()) is not null
                : GeneratedDrawingInstructions.TryRead(image is GeneratedDrawingImageResource drawingImage ? drawingImage.Drawing : image, out instructions);
            if (!read
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
            double ax = (uint)alignmentX * 0.5, ay = (uint)alignmentY * 0.5;
            double tx = viewport.X + viewport.Width * ax - (box.X + box.Width * ax) * sx;
            double ty = viewport.Y + viewport.Height * ay - (box.Y + box.Height * ay) * sy;
            var contentToDevice = deviceTransform.Prepend(new(sx, sy, tx, ty));
            if (!contentToDevice.TryInvert(out var deviceToContent)) return true;
            // The viewport fixes the sampling grid; content bounds must not remove transparent padding.
            double left = (viewport.X - tx) / sx;
            double top = (viewport.Y - ty) / sy;
            double right = (viewport.X + viewport.Width - tx) / sx;
            double bottom = (viewport.Y + viewport.Height - ty) / sy;
            if (bounds.X >= right || bounds.Y >= bottom
                || bounds.X + bounds.Width <= left || bounds.Y + bounds.Height <= top) return true;
            if (deviceAligned)
                return TryCaptureDeviceAligned(instructions, new(left, top, right - left, bottom - top),
                    contentToDevice, deviceClip, scalingMode, renderBounds, out snapshot, frozen);
            double xLength = Math.Abs(sx) * viewportScaleX;
            double yLength = Math.Abs(sy) * viewportScaleY;
            if (!(right > left) || !(bottom > top) || !(xLength > 0) || !(yLength > 0)) return true;
            double idealWidth = (right - left) * xLength, idealHeight = (bottom - top) * yLength;
            if (!double.IsFinite(idealWidth) || !double.IsFinite(idealHeight)) return false;
            double fullWidth = GeneratedImageMask.CalculateSurfaceSize(idealWidth);
            double fullHeight = GeneratedImageMask.CalculateSurfaceSize(idealHeight);
            bool oversizedGrid = idealWidth > int.MaxValue || idealHeight > int.MaxValue
                || Math.Abs(sx) * viewportScaleX > int.MaxValue
                || Math.Abs(sy) * viewportScaleY > int.MaxValue;
            float brushX = (float)brushScaleX, brushY = (float)brushScaleY;
            float worldX = (float)worldScaleX, worldY = (float)worldScaleY;
            float viewportX = brushX * worldX, viewportY = brushY * worldY;
            float idealLeft = ((float)viewport.X * brushX) * worldX;
            float idealTop = ((float)viewport.Y * brushY) * worldY;
            float idealRight = ((float)(viewport.X + viewport.Width) * brushX) * worldX;
            float idealBottom = ((float)(viewport.Y + viewport.Height) * brushY) * worldY;
            var contentToGrid = new GeneratedImageTransform(((float)sx * brushX) * worldX, ((float)sy * brushY) * worldY,
                ((float)tx * brushX) * worldX, ((float)ty * brushY) * worldY);
            var worldToDevice = worldTransform ?? GeneratedImageTransform.Identity;
            if (!worldToDevice.TryInvert(out var deviceToWorld)) return true;
            var viewportToWorld = deviceToWorld.Prepend(deviceTransform);
            if (!viewportToWorld.TryDecompose(out _, out var brushRest)) return true;
            if (!brushRest.TryInvert(out var worldToBrushRest)) return true;
            var worldToGrid = new GeneratedImageTransform(worldX, worldY, 0, 0).Prepend(worldToBrushRest);
            if (!oversizedGrid)
            {
                left = idealLeft; top = idealTop; right = idealRight; bottom = idealBottom;
                if (!(right > left) || !(bottom > top)) return true;
                idealWidth = (float)(idealRight - idealLeft);
                idealHeight = (float)(idealBottom - idealTop);
                fullWidth = GeneratedImageMask.CalculateSurfaceSize(idealWidth);
                fullHeight = GeneratedImageMask.CalculateSurfaceSize(idealHeight);
            }
            double baseLeft = left, baseTop = top, baseRight = right, baseBottom = bottom;
            MilRectD? visibleBounds = deviceClip is { } targetClip
                ? new(targetClip.Left, targetClip.Top, targetClip.Right - targetClip.Left, targetClip.Bottom - targetClip.Top)
                : null;
            if (renderBounds is { } shapeBounds)
            {
                if (!double.IsFinite(shapeBounds.X) || !double.IsFinite(shapeBounds.Y)
                    || !double.IsFinite(shapeBounds.Width) || !double.IsFinite(shapeBounds.Height)) return false;
                if (visibleBounds is { } targetBounds)
                {
                    double visibleX = Math.Max(targetBounds.X, shapeBounds.X);
                    double visibleY = Math.Max(targetBounds.Y, shapeBounds.Y);
                    visibleBounds = new(visibleX, visibleY,
                        Math.Min(targetBounds.X + targetBounds.Width, shapeBounds.X + shapeBounds.Width) - visibleX,
                        Math.Min(targetBounds.Y + targetBounds.Height, shapeBounds.Y + shapeBounds.Height) - visibleY);
                }
                else visibleBounds = shapeBounds;
            }
            if (!oversizedGrid)
            {
                MilRectD? visibleWorldBounds = worldBounds
                    ?? (renderBounds is { } rendered ? deviceToWorld.Apply(rendered) : null);
                if (deviceClip is { } sampleClip)
                {
                    var worldClip = deviceToWorld.Apply(new(sampleClip.Left, sampleClip.Top,
                        sampleClip.Right - sampleClip.Left, sampleClip.Bottom - sampleClip.Top));
                    if (visibleWorldBounds is { } renderedWorld)
                    {
                        double x = Math.Max(renderedWorld.X, worldClip.X);
                        double y = Math.Max(renderedWorld.Y, worldClip.Y);
                        visibleWorldBounds = new(x, y,
                            Math.Min(renderedWorld.X + renderedWorld.Width, worldClip.X + worldClip.Width) - x,
                            Math.Min(renderedWorld.Y + renderedWorld.Height, worldClip.Y + worldClip.Height) - y);
                    }
                    else visibleWorldBounds = worldClip;
                }
                visibleBounds = visibleWorldBounds;
            }
            if (visibleBounds is { } emptyBounds && (!(emptyBounds.Width > 0) || !(emptyBounds.Height > 0))) return true;
            double pixelsPerX = fullWidth / (right - left), pixelsPerY = fullHeight / (bottom - top);
            double cropX = 0, cropY = 0;
            if (tileMode == MilTileMode.None && visibleBounds is { } clip)
            {
                var visible = (oversizedGrid ? deviceToContent : worldToGrid).Apply(clip);
                double visibleLeft = Math.Max(left, visible.X), visibleTop = Math.Max(top, visible.Y);
                double visibleRight = Math.Min(right, visible.X + visible.Width);
                double visibleBottom = Math.Min(bottom, visible.Y + visible.Height);
                if (!(visibleRight > visibleLeft) || !(visibleBottom > visibleTop)) return true;
                // Crop integer texels on the original grid, retaining the bilinear neighbours.
                double firstX = Math.Max(0, Math.Floor((visibleLeft - left) * pixelsPerX - 0.5));
                double firstY = Math.Max(0, Math.Floor((visibleTop - top) * pixelsPerY - 0.5));
                double endX = Math.Min(fullWidth, Math.Floor((visibleRight - left) * pixelsPerX + 0.5) + 1);
                double endY = Math.Min(fullHeight, Math.Floor((visibleBottom - top) * pixelsPerY + 0.5) + 1);
                right = left + endX / pixelsPerX; bottom = top + endY / pixelsPerY;
                left += firstX / pixelsPerX; top += firstY / pixelsPerY;
                fullWidth = endX - firstX; fullHeight = endY - firstY;
                cropX = firstX; cropY = firstY;
            }
            double shiftX = 0, shiftY = 0;
            bool cropped = false;
            if (tileMode != MilTileMode.None && visibleBounds is { } tiledClip)
            {
                var visible = (oversizedGrid ? deviceToContent : worldToGrid).Apply(tiledClip);
                cropped = CropTileAxis(ref left, ref right, ref fullWidth, visible.X,
                    visible.X + visible.Width, tileMode is MilTileMode.FlipX or MilTileMode.FlipXY, out shiftX, out cropX);
                cropped |= CropTileAxis(ref top, ref bottom, ref fullHeight, visible.Y,
                    visible.Y + visible.Height, tileMode is MilTileMode.FlipY or MilTileMode.FlipXY, out shiftY, out cropY);
            }
            bounds = oversizedGrid ? new(left, top, right - left, bottom - top)
                : new((left / viewportX - tx) / sx, (top / viewportY - ty) / sy,
                    (right - left) / viewportX / sx, (bottom - top) / viewportY / sy);
            double w = fullWidth, h = fullHeight;
            GeneratedImageMask.AdjustSurfaceSize(ref w, ref h);
            if (!double.IsFinite(w) || !double.IsFinite(h)) return false;
            var draws = new List<ImageDraw>();
            var candidate = new DrawingSnapshot(draws, bounds, (uint)Math.Max(1, w), (uint)Math.Max(1, h))
            {
                IsCroppedViewportTile = cropped,
                IsCompleteViewportTile = !cropped && tileMode != MilTileMode.None
                    && left == baseLeft && top == baseTop && right == baseRight && bottom == baseBottom
            };
            try
            {
                GeneratedImageTransform mapping;
                if (!oversizedGrid)
                {
                    float surfaceScaleX = (float)GeneratedImageMask.CalculateSurfaceSize(idealWidth) / (idealRight - idealLeft);
                    float surfaceScaleY = (float)GeneratedImageMask.CalculateSurfaceSize(idealHeight) / (idealBottom - idealTop);
                    float surfaceOffsetX = -idealLeft * surfaceScaleX - (float)cropX;
                    float surfaceOffsetY = -idealTop * surfaceScaleY - (float)cropY;
                    float capX = candidate.Width / (float)fullWidth;
                    float capY = candidate.Height / (float)fullHeight;
                    surfaceScaleX *= capX; surfaceOffsetX *= capX;
                    surfaceScaleY *= capY; surfaceOffsetY *= capY;
                    float contentScaleX = (float)contentToGrid.ScaleX;
                    float contentScaleY = (float)contentToGrid.ScaleY;
                    float contentOffsetX = (float)contentToGrid.X;
                    float contentOffsetY = (float)contentToGrid.Y;
                    mapping = new(contentScaleX * surfaceScaleX, contentScaleY * surfaceScaleY,
                        contentOffsetX * surfaceScaleX + surfaceOffsetX,
                        contentOffsetY * surfaceScaleY + surfaceOffsetY);
                    var surfaceToScaledWorld = new GeneratedImageTransform(
                        (1f / surfaceScaleX) / worldX, (1f / surfaceScaleY) / worldY,
                        (-surfaceOffsetX / surfaceScaleX - (float)shiftX) / worldX,
                        (-surfaceOffsetY / surfaceScaleY - (float)shiftY) / worldY);
                    candidate.PixelToDevice = worldToDevice.Prepend(brushRest).Prepend(surfaceToScaledWorld);
                }
                else
                {
                    // Outside native integer clipping, preserve the bounded local tile's subpixel origin.
                    double localScaleX = candidate.Width / bounds.Width;
                    double localScaleY = candidate.Height / bounds.Height;
                    mapping = new(localScaleX, localScaleY, -bounds.X * localScaleX, -bounds.Y * localScaleY);
                    candidate.PixelToDevice = contentToDevice.Prepend(new(1 / localScaleX,
                        1 / localScaleY, bounds.X - shiftX, bounds.Y - shiftY));
                }

                if (CaptureInstructions(instructions, draws, mapping,
                    new(0, 0, (int)candidate.Width, (int)candidate.Height), scalingMode, MilCompositingMode.SourceOver, 0, frozen) < 0) return false;
                snapshot = candidate;
                return true;
            }
            finally { if (snapshot is null) candidate.Dispose(); }
        }

        private static bool CropTileAxis(ref double minimum, ref double maximum, ref double size,
            double visibleMinimum, double visibleMaximum, bool flip, out double translation, out double crop)
        {
            translation = 0; crop = 0;
            float scale = (float)size / ((float)maximum - (float)minimum);
            float offset = -(float)minimum * scale;
            float visibleFirst = (float)visibleMinimum * scale;
            float visibleLast = (float)visibleMaximum * scale;
            visibleFirst += offset; visibleLast += offset;
            double first = Math.Floor(visibleFirst - 0.5f);
            double last = Math.Floor(visibleLast + 0.5f);
            // Native integer arithmetic abandons clipping when the tile or bounds can overflow.
            double limit = int.MaxValue - size;
            if (!(limit > 0) || !double.IsFinite(first) || !double.IsFinite(last)
                || first <= -limit || first >= limit || last <= -limit || last >= limit || first > last)
                return false;
            double firstShift = -Math.Floor(first / size), lastShift = -Math.Floor(last / size);
            double shift = lastShift * size;
            if (firstShift == lastShift)
            {
                first += shift; last += shift;
            }
            else if (flip && lastShift - firstShift == -1)
            {
                first += shift; last += shift;
                last = Math.Max(-first, last); first = 0;
                if (last >= size) return false;
            }
            else return false;
            if (flip && Math.Abs(lastShift % 2) == 1)
            {
                (first, last) = (size - 1 - last, size - 1 - first);
                shift += 2 * (last + 1) - size;
            }
            if (first == 0 && last == size - 1 && shift == 0) return false;
            double origin = minimum;
            minimum = origin + first / scale;
            maximum = origin + (last + 1) / scale;
            size = last - first + 1;
            crop = first;
            translation = (float)shift / scale;
            return true;
        }

        private static bool TryCaptureDeviceAligned(IReadOnlyList<GeneratedRenderDataInstruction> instructions,
            MilRectD contentClip, GeneratedImageTransform contentToDevice, Direct3D9SurfaceRect? deviceClip,
            uint scalingMode, MilRectD? renderBounds, out DrawingSnapshot? snapshot, FrozenVisualTree? frozen)
        {
            snapshot = null;
            var ideal = contentToDevice.Apply(contentClip);
            double left = ideal.X, top = ideal.Y;
            double right = ideal.X + ideal.Width, bottom = ideal.Y + ideal.Height;
            if (renderBounds is { } shape)
            {
                if (!double.IsFinite(shape.X) || !double.IsFinite(shape.Y)
                    || !double.IsFinite(shape.Width) || !double.IsFinite(shape.Height)) return false;
                left = Math.Max(left, shape.X); top = Math.Max(top, shape.Y);
                right = Math.Min(right, shape.X + shape.Width); bottom = Math.Min(bottom, shape.Y + shape.Height);
            }
            if (deviceClip is { } clip)
            {
                left = Math.Max(left, clip.Left); top = Math.Max(top, clip.Top);
                right = Math.Min(right, clip.Right); bottom = Math.Min(bottom, clip.Bottom);
            }
            if (!(right > left) || !(bottom > top)) return true;
            left = Math.Floor(left); top = Math.Floor(top);
            right = Math.Ceiling(right); bottom = Math.Ceiling(bottom);
            double width = right - left, height = bottom - top;
            if (!double.IsFinite(width) || !double.IsFinite(height)) return false;
            uint w = (uint)Math.Max(1, Math.Min(width, 2048));
            uint h = (uint)Math.Max(1, Math.Min(height, 2048));
            // DeviceAligned caps each axis using FLOAT scale and offset, then inverts that mapping.
            float scaleX = width > 2048 ? 2048f / (float)width : 1;
            float scaleY = height > 2048 ? 2048f / (float)height : 1;
            float offsetX = -(float)left * scaleX, offsetY = -(float)top * scaleY;
            var surfaceToDevice = new GeneratedImageTransform(1f / scaleX, 1f / scaleY,
                -offsetX / scaleX, -offsetY / scaleY);
            var mapping = new GeneratedImageTransform(
                (float)contentToDevice.ScaleX * scaleX, (float)contentToDevice.ScaleY * scaleY,
                (float)contentToDevice.X * scaleX + offsetX, (float)contentToDevice.Y * scaleY + offsetY,
                (float)contentToDevice.M12 * scaleY, (float)contentToDevice.M21 * scaleX);
            var draws = new List<ImageDraw>();
            var candidate = new DrawingSnapshot(draws, contentClip, w, h)
            {
                PixelToDevice = surfaceToDevice,
                IsDeviceAligned = true
            };
            try
            {
                if (CaptureInstructions(instructions, draws, mapping, new(0, 0, (int)w, (int)h),
                    scalingMode, MilCompositingMode.SourceOver, 0, frozen) < 0) return false;
                snapshot = candidate;
                return true;
            }
            finally { if (snapshot is null) candidate.Dispose(); }
        }

        internal static int CaptureTree(GeneratedVisualResource root, MilRectD bounds,
            GeneratedImageTransform mapping, uint width, uint height, uint scalingMode, bool cacheContents,
            out DrawingSnapshot? snapshot)
        {
            snapshot = null;
            int hr = FrozenVisualTree.CapturePure(root, [], out var frozen);
            if (hr < 0) return hr;
            using var tree = frozen;
            return RealizeTree(tree!, bounds, mapping, width, height, scalingMode, cacheContents, out snapshot);
        }

        internal static int RealizeTree(FrozenVisualTree tree, MilRectD bounds,
            GeneratedImageTransform mapping, uint width, uint height, uint scalingMode, bool cacheContents,
            out DrawingSnapshot? snapshot)
        {
            snapshot = null;
            var draws = new List<ImageDraw>();
            var candidate = new DrawingSnapshot(draws, bounds, width, height);
            try
            {
                var clip = new Direct3D9SurfaceRect(0, 0, (int)width, (int)height);
                bool shadowOpacity = cacheContents && tree.Shadow is not null && tree.Alpha != 1;
                if (shadowOpacity) draws.Add(ImageDraw.BeginLayer(Math.Clamp(tree.Alpha, 0, 1)));
                int hr = cacheContents
                    ? CaptureContents(tree!, draws, mapping, clip, 0, tree!.BitmapScalingMode ?? scalingMode,
                        tree.CompositingMode ?? MilCompositingMode.SourceOver)
                    : Capture(tree!, draws, mapping, clip, 0, scalingMode);
                if (shadowOpacity) draws.Add(ImageDraw.EndLayer);
                if (hr < 0) return hr;
                snapshot = candidate;
                return 0;
            }
            finally { if (snapshot is null) candidate.Dispose(); }
        }

        internal int Realize(out nint bitmap, bool floating = true)
        {
            bitmap = 0;
            if (_disposed) return Direct3D9Factory.WgxInvalidCallHResult;
            nint target = 0, candidate = 0;
            SoftwareImageRenderSession? session = null;
            try
            {
                int hr = WpfGfxShape.Abi.BitmapRenderTargetExports.CreateDrawingIntermediate(Width, Height, out target, out candidate, floating);
                if (hr < 0) return hr;
                hr = SoftwareImageRenderSession.Open(target, Width, Height, out session);
                if (hr < 0) return hr;
                foreach (var draw in _draws)
                {
                    hr = draw.LayerAction switch
                    {
                        1 => session!.BeginLayer(draw.Opacity, draw.Mask, draw.AlphaMask, draw.ImageMask),
                        2 => session!.EndLayer(),
                        4 => draw.Effect!.Draw(session!, draw.Context),
                        5 => draw.Blur!.Draw(session!, draw.Context),
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
                _draws[i].Effect?.Dispose();
                _draws[i].Blur?.Dispose();
                _draws[i].StrokeBrush?.Dispose(); _draws[i].Brush?.Dispose();
                _draws[i].ImageMask?.Dispose(); _draws[i].VideoSample?.Dispose();
                Direct3D9Factory.Release(_draws[i].Source);
            }
            _draws.Clear();
        }
    }

    private static int CaptureMaskLayer(List<ImageDraw> draws, GeneratedProtocolResource? brush,
        GeneratedImageTransform transform, uint scalingMode, double opacity, MilRectD? contentBounds = null,
        Direct3D9SurfaceRect? deviceClip = null, FrozenVisualTree? frozen = null)
    {
        draws.EnsureCapacity(draws.Count + 1);
        if (brush is GeneratedTileBrushResource image)
        {
            GeneratedImageMask? mask;
            var imageBrush = frozen?.ImageBrush(image);
            if (image is GeneratedVisualBrushResource visual)
            {
                if (contentBounds is not { } bounds) return Direct3D9Factory.NotImplementedHResult;
                int hr = imageBrush is not null
                    ? GeneratedImageMask.CaptureVisualBrushFrozen(imageBrush, transform, scalingMode, bounds, deviceClip, out mask)
                    : GeneratedImageMask.CaptureVisualBrush(visual, transform, scalingMode, bounds, deviceClip, out mask);
                if (hr < 0) return hr;
            }
            else
            {
                bool captured = imageBrush is not null
                    ? GeneratedImageMask.TryCaptureFrozen(imageBrush, transform, scalingMode, out mask, contentBounds, deviceClip, infiniteRenderBounds: true)
                    : GeneratedImageMask.TryCapture(image, transform, scalingMode, out mask, contentBounds, deviceClip, infiniteRenderBounds: true);
                if (!captured) return Direct3D9Factory.NotImplementedHResult;
            }
            if (mask is not null) mask.LayerBounds = deviceClip;
            draws.Add(mask is null ? ImageDraw.BeginLayer(opacity, alphaMask: 0) : ImageDraw.BeginLayer(opacity, imageMask: mask));
            return 0;
        }
        if (!TrySolidMask(brush, out var alpha)) return Direct3D9Factory.NotImplementedHResult;
        draws.Add(ImageDraw.BeginLayer(opacity, alphaMask: alpha));
        return 0;
    }

    private static int CaptureGeometryBrush(List<ImageDraw> draws, GeneratedProtocolResource brush,
        GeneratedImageTransform transform, Direct3D9SurfaceRect? clip, uint scalingMode, MilCompositingMode compositing,
        SoftwareImageCoverage? coverage, MilRectD localBounds, bool appendStroke = false, FrozenVisualTree? frozen = null)
    {
        if (localBounds.Width <= 0 || localBounds.Height <= 0) return 0;
        draws.EnsureCapacity(draws.Count + 1);
        SoftwareBrushRealization? realization;
        int result = frozen?.ImageBrush(brush) is { } imageBrush
            ? SoftwareBrushRealization.CaptureFrozen(imageBrush, transform, scalingMode, localBounds, out realization, clip)
            : frozen?.CacheRequest(brush) is { } request
                ? SoftwareBrushRealization.CaptureFrozen(request, transform, scalingMode, localBounds, out realization, clip)
                : SoftwareBrushRealization.Capture(brush, transform, scalingMode, localBounds, out realization, clip);
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

    private static int Capture(FrozenVisualTree visual, List<ImageDraw> draws, GeneratedImageTransform parent, Direct3D9SurfaceRect? inheritedClip, int depth, uint inheritedScalingMode = 0, MilCompositingMode inheritedCompositing = MilCompositingMode.SourceOver)
    {
        if (depth > 256 || visual.HasUnsupportedEffect)
            return Direct3D9Factory.NotImplementedHResult;
        if (!double.IsFinite(visual.Alpha)) return Direct3D9Factory.NotImplementedHResult;
        uint scalingMode = visual.BitmapScalingMode ?? inheritedScalingMode;
        MilCompositingMode compositing = visual.CompositingMode ?? inheritedCompositing;

        GeneratedImageTransform local = visual.Transform;
        GeneratedImageTransform transform = parent.Prepend(new(1, 1, visual.Offset.X, visual.Offset.Y)).Prepend(local);
        bool rectangularClip = GeneratedImageClip.TryIntersect(visual.Clip?.Geometry, transform, inheritedClip,
            out Direct3D9SurfaceRect? visualClip);
        if (!rectangularClip) visualClip = inheritedClip;
        bool hasLayer = (visual.Alpha != 1 && visual.Shadow is null) || visual.AlphaMask is not null;
        if (hasLayer)
        {
            MilRectD? contentBounds = null;
            if (visual.AlphaMask is GeneratedTileBrushResource)
            {
                if (!GeneratedImageContentBounds.TryGet(visual, out var bounds)) return Direct3D9Factory.NotImplementedHResult;
                contentBounds = bounds;
            }
            Direct3D9SurfaceRect? maskClip = visualClip;
            if (contentBounds is { } layerBounds)
            {
                var deviceBounds = transform.Apply(layerBounds);
                double left = Math.Floor(deviceBounds.X), top = Math.Floor(deviceBounds.Y);
                double right = Math.Ceiling(deviceBounds.X + deviceBounds.Width);
                double bottom = Math.Ceiling(deviceBounds.Y + deviceBounds.Height);
                if (visualClip is { } currentClip)
                {
                    left = Math.Max(left, currentClip.Left); top = Math.Max(top, currentClip.Top);
                    right = Math.Min(right, currentClip.Right); bottom = Math.Min(bottom, currentClip.Bottom);
                }
                if (left >= int.MinValue && top >= int.MinValue && right <= int.MaxValue && bottom <= int.MaxValue
                    && right >= left && bottom >= top)
                    maskClip = new((int)left, (int)top, (int)right, (int)bottom);
            }
            int hr = CaptureMaskLayer(draws, visual.AlphaMask, transform, scalingMode, Math.Clamp(visual.Alpha, 0, 1), contentBounds, maskClip, visual);
            if (hr < 0) return hr;
        }
        bool clipLayer = false;
        if (!rectangularClip)
        {
            if (!GeneratedImageClip.TryCreateMask(visual.Clip?.Geometry, transform, out var mask))
                return Direct3D9Factory.NotImplementedHResult;
            visualClip = inheritedClip;
            draws.Add(ImageDraw.BeginLayer(1, mask));
            clipLayer = true;
        }
        int contentsResult;
        if (visual.Blur is not null || visual.Shadow is not null)
        {
            var shadow = visual.Shadow;
            var blur = visual.Blur ?? new FrozenBlurEffect(shadow!.Value.Radius, MilKernelType.Gaussian, shadow.Value.Bias);
            if (!GeneratedImageContentBounds.TryGet(visual, out var bounds)) return Direct3D9Factory.NotImplementedHResult;
            var padding = new MilShaderEffectCommand(default, 0, shadow is { } s ? s.Top + blur.Radius : blur.Radius, shadow is { } s2 ? s2.Bottom + blur.Radius : blur.Radius, shadow is { } s3 ? s3.Left + blur.Radius : blur.Radius, shadow is { } s4 ? s4.Right + blur.Radius : blur.Radius,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            contentsResult = GeneratedShaderInputScope.TryCreate(bounds, padding, transform, out var scope);
            if (contentsResult == Direct3D9Factory.ClippedToEmptyHResult) contentsResult = 0;
            else if (contentsResult >= 0)
            {
                contentsResult = DrawingSnapshot.RealizeTree(visual, bounds, scope.InputTransform,
                    scope.Width, scope.Height, scalingMode, true, out var drawing);
                if (contentsResult >= 0 && drawing is not null)
                {
                    int radius = (int)Math.Min(100, (float)(uint)Math.Min(uint.MaxValue, blur.Radius)
                        * Math.Min((float)scope.InputTransform.ScaleX, (float)scope.InputTransform.ScaleY));
                    draws.Add(new(0, default, false, new(scope.OutputTransform, visualClip, scalingMode, compositing),
                        LayerAction: 5, Blur: new(new(drawing), scope, blur, radius, shadow)));
                }
            }
        }
        else if (visual.Effect is { } invocation)
        {
            if (!GeneratedImageContentBounds.TryGet(visual, out var bounds)) return Direct3D9Factory.NotImplementedHResult;
            contentsResult = GeneratedShaderInputScope.TryCreate(bounds, invocation.Data.Command, transform, out var scope);
            if (contentsResult == Direct3D9Factory.ClippedToEmptyHResult) contentsResult = 0;
            else if (contentsResult >= 0)
            {
                contentsResult = DrawingSnapshot.RealizeTree(visual, bounds, scope.InputTransform,
                    scope.Width, scope.Height, scalingMode, true, out var drawing);
                VisualSnapshot? input = drawing is null ? null : new(drawing);
                if (contentsResult >= 0 && input is not null)
                {
                    contentsResult = GeneratedSoftwareShaderEffect.Capture(invocation, scope, input, bounds, out var effect);
                    if (contentsResult < 0) input.Dispose();
                    else draws.Add(new(0, default, false, new(scope.OutputTransform, visualClip, scalingMode,
                        compositing), LayerAction: 4, Effect: effect));
                }
            }
        }
        else contentsResult = CaptureContents(visual, draws, transform, visualClip, depth, scalingMode, compositing);
        if (contentsResult < 0) return contentsResult;
        if (clipLayer) draws.Add(ImageDraw.EndLayer);
        if (hasLayer) draws.Add(ImageDraw.EndLayer);
        return 0;
    }

    private static int CaptureContents(FrozenVisualTree visual, List<ImageDraw> draws,
        GeneratedImageTransform transform, Direct3D9SurfaceRect? clip, int depth,
        uint scalingMode, MilCompositingMode compositing)
    {
        if (depth > 256) return Direct3D9Factory.NotImplementedHResult;
        if (visual.Instructions.Count != 0)
        {
            int hr = CaptureInstructions(visual.Instructions, draws, transform, clip, scalingMode, compositing, depth, visual);
            if (hr < 0) return hr;
        }
        foreach (var child in visual.Children)
        {
            int hr = Capture(child, draws, transform, clip, depth + 1, scalingMode, compositing);
            if (hr < 0) return hr;
        }
        return 0;
    }

    private static int CaptureInstructions(IReadOnlyList<GeneratedRenderDataInstruction> instructions,
        List<ImageDraw> draws, GeneratedImageTransform transform, Direct3D9SurfaceRect? visualClip,
        uint scalingMode, MilCompositingMode compositing, int depth, FrozenVisualTree? frozen = null)
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
                    if (instruction.ResourceSlots[0] is GeneratedTileBrushResource maskBrush
                        && (maskBrush is GeneratedVisualBrushResource || maskBrush.ViewportUnits == MilBrushMappingMode.RelativeToBoundingBox || maskBrush.RelativeTransform is not null))
                    {
                        if (!GeneratedImageContentBounds.TryGetMaskScope(instructions, instructionIndex + 1, out var bounds))
                            return Direct3D9Factory.NotImplementedHResult;
                        scopeBounds = bounds;
                    }
                    int hr = CaptureMaskLayer(draws, instruction.ResourceSlots[0], current, scalingMode, 1, scopeBounds, currentClip, frozen);
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
                if (instruction.Kind == GeneratedRenderDataKind.DrawDrawing)
                {
                    if (!GeneratedDrawingInstructions.TryRead(instruction.ResourceSlots[0], out var drawingInstructions))
                        return Direct3D9Factory.NotImplementedHResult;
                    int drawingResult = CaptureInstructions(drawingInstructions, draws, current, currentClip,
                        scalingMode, compositing, depth + 1);
                    if (drawingResult < 0) return drawingResult;
                    continue;
                }
                if (instruction.Kind == GeneratedRenderDataKind.DrawGlyphRun)
                {
                    if (instruction.ResourceSlots[0] is not { } glyphBrush
                        || instruction.ResourceSlots[1] is not GeneratedGlyphRunResource glyph) continue;
                    int glyphResult = GeneratedGlyphAlpha.Create(glyph, current, currentClip, out var alpha);
                    if (glyphResult < 0) return glyphResult;
                    if (alpha is null || alpha.Bounds.Width <= 0 || alpha.Bounds.Height <= 0) continue;
                    draws.EnsureCapacity(draws.Count + 1);
                    glyphResult = SoftwareBrushRealization.Capture(glyphBrush, current, scalingMode,
                        glyph.Command.Bounds, out var glyphRealization, currentClip);
                    if (glyphResult < 0) return glyphResult;
                    draws.Add(new(0, default, false, new(GeneratedImageTransform.Identity, currentClip,
                        scalingMode, compositing, GlyphAlpha: alpha), 3, Brush: glyphRealization, Bounds: alpha.Bounds));
                    continue;
                }
                if (GeneratedPrimitiveGeometry.IsPrimitive(instruction.Kind))
                {
                    var primitive = GeneratedPrimitiveGeometry.Create(instruction, out var primitiveBrush, out var primitivePen);
                    if (primitive is null) continue;
                    try
                    {
                        GeneratedProtocolResource?[] slots = [primitiveBrush, primitivePen, primitive];
                        var geometryInstruction = new GeneratedRenderDataInstruction(GeneratedRenderDataKind.DrawGeometry,
                            slots.OfType<GeneratedProtocolResource>().ToArray(), ReadOnlyMemory<byte>.Empty, slots);
                        int primitiveResult = CaptureInstructions([geometryInstruction], draws, current, currentClip,
                            scalingMode, compositing, depth, frozen);
                        if (primitiveResult < 0) return primitiveResult;
                    }
                    finally { primitive.Release(); }
                    continue;
                }
                if (instruction.Kind == GeneratedRenderDataKind.DrawGeometry)
                {
                    if (instruction.ResourceSlots[2] is not { } liveGeometry) continue;
                    int geometryResult = FrozenGeometry.CapturePure(liveGeometry, [], out var capturedGeometry);
                    if (geometryResult < 0) return geometryResult;
                    using var geometrySnapshot = capturedGeometry;
                    if (geometrySnapshot?.Geometry is not { } geometry)
                        return Direct3D9Factory.NotImplementedHResult;
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
                            int fillResult = CaptureGeometryBrush(draws, fillBrush, current, currentClip, scalingMode, compositing, fillCoverage, fillBounds, frozen: frozen);
                            if (fillResult < 0) return fillResult;
                        }
                        SoftwareImageCoverage? strokeCoverage = null;
                        if ((!hasNativeStroke && !GeneratedStrokeGeometry.TryCreate(geometry, strokePen, current, out strokeCoverage, out _))
                            || !nativeStroke!.TryGetBounds(localSpace: true, out var strokeLocalBounds))
                            return Direct3D9Factory.NotImplementedHResult;
                        int strokeResult = CaptureGeometryBrush(draws, strokePen.Brush!, current, currentClip, scalingMode, compositing, strokeCoverage, strokeLocalBounds, appendStroke: draws.Count > firstDraw, frozen: frozen);
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
                    if (fill is not GeneratedTileBrushResource and not GeneratedSolidColorBrushResource and not GeneratedBitmapCacheBrushResource
                        || !GeneratedGeometryBounds.TryGet(geometry, GeneratedImageTransform.Identity, out var localBounds, current)
                        || !GeneratedGeometryBounds.TryGet(geometry, current, out var deviceBounds)
                        || (!hasNativeFill && !GeneratedImageClip.TryCreateMask(geometry, current, out coverage)))
                        return Direct3D9Factory.NotImplementedHResult;
                    if (localBounds.Width <= 0 || localBounds.Height <= 0) continue;
                    int firstFill = draws.Count;
                    int result = CaptureGeometryBrush(draws, fill, current, currentClip, scalingMode,
                        compositing, coverage, localBounds, frozen: frozen);
                    if (result < 0) return result;
                    if (draws.Count > firstFill && hasNativeFill)
                        draws[^1] = draws[^1] with { NativeStroke = nativeFill, Mask = null };
                    continue;
                }
                if (instruction.Kind is GeneratedRenderDataKind.DrawVideo or GeneratedRenderDataKind.DrawVideoAnimate)
                {
                    if (instruction.ResourceSlots[0] is not GeneratedMediaPlayerResource video) continue;
                    MilRectD videoRectangle = MemoryMarshal.Read<MilRectD>(instruction.Data.Span);
                    if (instruction.Kind == GeneratedRenderDataKind.DrawVideoAnimate
                        && instruction.ResourceSlots[1] is GeneratedValueResource<MilRectD> animatedVideoRectangle)
                        videoRectangle = animatedVideoRectangle.Value;
                    draws.EnsureCapacity(draws.Count + 1);
                    int videoResult = video.AcquireSystemFrame(out var sample);
                    if (videoResult < 0) { sample?.Dispose(); return videoResult; }
                    if (sample is null) continue;
                    nint source = sample.Source;
                    uint sampleWidth = 0, sampleHeight = 0;
                    videoResult = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)source)[3])
                        (source, &sampleWidth, &sampleHeight);
                    if (videoResult < 0) { sample.Dispose(); return videoResult; }
                    if (sampleWidth == 0 || sampleHeight == 0)
                    {
                        sample.Dispose();
                        continue;
                    }
                    Direct3D9Factory.AddRef(source);
                    draws.Add(new(source, videoRectangle, true, new(current, currentClip, scalingMode, compositing), VideoSample: sample));
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
                    int result = CaptureInstructions(drawingInstructions, draws, mapping, currentClip, scalingMode, compositing, depth + 1, frozen);
                    if (result < 0) return result;
                    continue;
                }
                if (image is not GeneratedDoubleBufferedBitmapResource and not GeneratedBitmapSourceResource and not GeneratedD3DImageResource)
                    return Direct3D9Factory.NotImplementedHResult;
                draws.EnsureCapacity(draws.Count + 1);
                nint bitmap = frozen?.ImageBrush(image) is { } sourcePin ? sourcePin.AcquireSource() : image switch
                {
                    GeneratedDoubleBufferedBitmapResource source => source.AcquireBitmapSource(),
                    GeneratedD3DImageResource source => source.AcquireBitmapSource(),
                    GeneratedBitmapSourceResource source => source.AcquireBitmapSource(),
                    _ => 0
                };
                if (bitmap != 0) draws.Add(new(bitmap, rectangle, image is GeneratedBitmapSourceResource,
                    new(current, currentClip, scalingMode, compositing)));
            }
        }
        return 0;
    }
}
