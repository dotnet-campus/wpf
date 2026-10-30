using System.Numerics;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe partial class BitmapRenderTargetExports
{
    // Release native layout prefix only. Never allocate this as a full CContextState.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeContextPrefix
    {
        internal nint DisplaySet, DisplaySettings, DpiProvider;
        internal Matrix4x4 UnitTransform;
        internal uint PageUnit;
        internal Matrix4x4 World3D, View3D, Projection3D, Viewport3D;
        internal float MeshLeft, MeshTop, MeshRight, MeshBottom;
        internal byte In3D;
        internal uint DepthFunction, CullMode;
        internal NativeAliasedClip Clip;
        internal NativeRenderState* RenderState;
        internal Matrix4x4 WorldToDevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRenderState
    {
        internal uint Options;
        internal Matrix4x4 LocalTransform;
        internal int SourceX, SourceY, SourceWidth, SourceHeight;
        internal uint Interpolation;
        internal byte Prefilter;
        internal float PrefilterThreshold;
        internal uint AntiAlias;
        internal MilCompositingMode Compositing;
        internal uint TextRendering, TextHinting;
    }

    private static int GetInternalClip(InternalView* self, nint context, out Direct3D9SurfaceRect clip)
    {
        clip = default;
        if (context == 0) return InvalidArgument;
        int left, top, right, bottom;
        if (self->Layer != null)
        {
            NativeLayer layer = *self->Layer;
            left = layer.Left;
            top = layer.Top;
            right = layer.Right;
            bottom = layer.Bottom;
        }
        else
        {
            uint width, height;
            nint bitmap = self->Owner->Bitmap;
            int result = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)bitmap)[3])(bitmap, &width, &height);
            if (result < 0) return result;
            if (width > int.MaxValue || height > int.MaxValue) return InvalidArgument;
            left = top = 0;
            right = (int)width;
            bottom = (int)height;
        }
        if (right <= left || bottom <= top) return 0;
        NativeAliasedClip requested = ((NativeContextPrefix*)context)->Clip;
        if (requested.IsNull == 0)
        {
            // Native UpdateCurrentClip consumes no shape, brush or surface lock on no intersection.
            if (float.IsNaN(requested.Left) || float.IsNaN(requested.Top)
                || float.IsNaN(requested.Right) || float.IsNaN(requested.Bottom)) return 0;
            left = Math.Max(left, ClipCoordinate(requested.Left, right));
            top = Math.Max(top, ClipCoordinate(requested.Top, bottom));
            right = Math.Min(right, ClipCoordinate(requested.Right, right));
            bottom = Math.Min(bottom, ClipCoordinate(requested.Bottom, bottom));
            if (right <= left || bottom <= top) return 0;
        }
        clip = new(left, top, right, bottom);
        return 0;
    }

    private static int DrawInternalPath(InternalView* self, nint context, nint brushContext, nint shape, nint pen, nint stroke, nint fill)
    {
        if (self->SoftwarePrimitive != 0)
        {
            if (context != self->SoftwareContext || self->ConsumingSoftwarePrimitive != 0)
                return Direct3D9Factory.WgxInvalidCallHResult;
            self->ConsumingSoftwarePrimitive = 1;
            try
            {
                var primitive = (SoftwarePathPrimitive)GCHandle.FromIntPtr(self->SoftwarePrimitive).Target!;
                return ConsumeSoftwarePath(self, primitive, context, brushContext, shape, pen, fill, stroke);
            }
            finally { self->ConsumingSoftwarePrimitive = 0; }
        }
        int result = GetInternalClip(self, context, out var clip);
        if (result < 0) return result;
        if (clip.Right <= clip.Left || clip.Bottom <= clip.Top) return 0;
        nint target = (nint)self->Owner;
        nint bitmap = self->Owner->Bitmap;
        nint surfaceLock = 0;
        // Foreign realization callbacks may release the caller's target reference.
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)target)[1])(target);
        try
        {
            result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 3, &surfaceLock);
            if (result < 0) return result;
            if (fill == 0 && (pen == 0 || stroke == 0)) return 0;
            NativeContextPrefix native = *(NativeContextPrefix*)context;
            if (native.RenderState == null) return InvalidArgument;
            NativeRenderState state = *native.RenderState;
            if (native.In3D != 0 || state.AntiAlias > 1
                || state.Compositing is not (MilCompositingMode.SourceOver or MilCompositingMode.SourceCopy)) return NotImplemented;
            uint width, height, stride, size;
            byte* pixels;
            Guid format;
            result = ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)bitmap)[4])(bitmap, &format);
            if (result < 0) return result;
            int code = SoftwareBitmap.FormatCode(format);
            if (code is not (16 or 26)) return NotImplemented;
            result = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)bitmap)[3])(bitmap, &width, &height);
            if (result < 0) return result;
            result = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)surfaceLock)[4])(surfaceLock, &stride);
            if (result < 0) return result;
            result = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)surfaceLock)[5])(surfaceLock, &size, &pixels);
            if (result < 0) return result;
            using var renderer = new Direct3D9SoftwareImageRenderer(pixels, size, stride, width, height, code == 26);
            return DrawSoftwarePath(clip, fill, pen == 0 ? 0 : stroke,
                realizer => FillInternalShape(self, renderer, context, brushContext,
                    shape, realizer, native.WorldToDevice, state, clip),
                realizer =>
                {
                    int hr = NativeStrokeReader.Widen(shape, pen, native.WorldToDevice, out var strokeCoverage);
                    return hr < 0 ? hr : FillInternalShape(self, renderer, context, brushContext, 0, realizer,
                        native.WorldToDevice, state, clip, strokeCoverage);
                });
        }
        finally
        {
            Direct3D9Factory.Release(surfaceLock);
            Direct3D9Factory.Release(target);
        }
    }

    private static int DrawSoftwarePath(Direct3D9SurfaceRect clip,
        nint fill, nint stroke, Func<nint, int> fillPath, Func<nint, int> strokePath)
    {
        if (clip.Right <= clip.Left || clip.Bottom <= clip.Top) return 0;
        int hr = fill == 0 ? 0 : fillPath(fill);
        return hr < 0 || stroke == 0 ? hr : strokePath(stroke);
    }

    // Generated geometry has already been transformed and widened. Keep that production
    // representation separate from foreign IShapeData/CPlainPen object layouts.
    private sealed record SoftwarePathPrimitive(Direct3D9SoftwareImageRenderer Renderer,
        SoftwareImageCoverage? FillCoverage, SoftwareImageDrawingContext FillContext,
        SoftwareImageCoverage? StrokeCoverage, SoftwareImageDrawingContext StrokeContext);

    internal static int DrawSoftwarePath(nint target, Direct3D9SoftwareImageRenderer renderer,
        nint fill, SoftwareImageCoverage? fillCoverage, SoftwareImageDrawingContext fillContext,
        nint stroke, SoftwareImageCoverage? strokeCoverage, SoftwareImageDrawingContext strokeContext,
        GeneratedNativeStroke? nativeStroke = null)
    {
        nint internalTarget = 0;
        Guid id = InternalTargetId;
        int hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])
            (target, &id, &internalTarget);
        if (hr < 0) return hr;
        GCHandle primitiveHandle = default;
        InternalView* view = (InternalView*)internalTarget;
        bool attached = false;
        try
        {
            if (view->SoftwarePrimitive != 0) return Direct3D9Factory.WgxInvalidCallHResult;
            using var nativePath = nativeStroke is null ? null : new NativeGeneratedPath(nativeStroke);
            var primitive = new SoftwarePathPrimitive(renderer, fillCoverage, fillContext, strokeCoverage, strokeContext);
            primitiveHandle = GCHandle.Alloc(primitive);
            // This prefix is consumed only by our software realizer contract, never
            // passed to a foreign CContextState implementation.
            NativeRenderState state = CreateSoftwareRenderState(fillContext);
            Matrix4x4 matrix = Matrix4x4.Identity;
            if (nativeStroke is { Transform: var transform })
            {
                matrix.M11 = (float)transform.ScaleX;
                matrix.M22 = (float)transform.ScaleY;
                matrix.M12 = (float)transform.M12;
                matrix.M21 = (float)transform.M21;
                matrix.M41 = (float)transform.X;
                matrix.M42 = (float)transform.Y;
            }
            NativeContextPrefix context = new() { RenderState = &state, WorldToDevice = matrix };
            var clip = fillContext.AliasedClip ?? strokeContext.AliasedClip;
            context.Clip = clip is { } area
                ? new NativeAliasedClip { Left = area.Left, Top = area.Top, Right = area.Right, Bottom = area.Bottom }
                : new NativeAliasedClip { IsNull = 1 };
            view->SoftwarePrimitive = GCHandle.ToIntPtr(primitiveHandle);
            view->SoftwareContext = (nint)(&context);
            attached = true;
            return ((delegate* unmanaged[Stdcall]<InternalView*, nint, nint, nint, nint, nint, nint, int>)view->Table[10])
                (view, (nint)(&context), 0, nativePath?.ShapePointer ?? 0, nativePath?.PenPointer ?? 0, stroke, fill);
        }
        finally
        {
            if (attached)
            {
                view->SoftwarePrimitive = view->SoftwareContext = 0;
                view->ConsumingSoftwarePrimitive = 0;
            }
            if (primitiveHandle.IsAllocated) primitiveHandle.Free();
            Direct3D9Factory.Release(internalTarget);
        }
    }

    private static NativeRenderState CreateSoftwareRenderState(SoftwareImageDrawingContext context) => new()
    {
        AntiAlias = context.Antialias ? 1u : 0u,
        Compositing = context.CompositingMode,
        Interpolation = context.BitmapScalingMode == 3 ? 0u : 1u,
        Prefilter = context.PrefilterEnabled == true ? (byte)1 : (byte)0,
        PrefilterThreshold = context.PrefilterThreshold ?? 0
    };

    private static int ConsumeSoftwarePath(InternalView* self, SoftwarePathPrimitive primitive,
        nint context, nint brushContext, nint shape, nint pen, nint fill, nint stroke)
    {
        int hr = GetInternalClip(self, context, out var clip);
        if (hr < 0) return hr;
        nint creator = (nint)(&self->CreatorTable);
        return DrawSoftwarePath(clip, fill, stroke,
            realizer =>
            {
                var coverage = primitive.FillCoverage;
                if (coverage is null && shape != 0)
                {
                    int result = NativeShapeReader.Capture(shape,
                        ToImageTransform(((NativeContextPrefix*)context)->WorldToDevice), out coverage, clip);
                    JpBrushTrace.Record("Path.FillShape", result, $"shape={shape};clip={clip}");
                    if (result < 0) return result;
                }
                return ConsumeSoftwareRealization(primitive.Renderer, realizer, brushContext, context, creator,
                    0, Matrix4x4.Identity, CreateSoftwareRenderState(primitive.FillContext), clip,
                    coverage, primitive.FillContext with { AliasedClip = clip });
            },
            realizer =>
            {
                var coverage = primitive.StrokeCoverage;
                if (shape != 0 && pen != 0)
                {
                    int result = NativeStrokeReader.Widen(shape, pen, ((NativeContextPrefix*)context)->WorldToDevice,
                        out coverage, out _, clip: clip, managedShape: true);
                    if (result < 0) return result;
                }
                return ConsumeSoftwareRealization(primitive.Renderer, realizer, brushContext, context, creator,
                    0, Matrix4x4.Identity, CreateSoftwareRenderState(primitive.StrokeContext), clip,
                    coverage, primitive.StrokeContext with { AliasedClip = clip });
            });
    }

    private static GeneratedImageTransform ToImageTransform(Matrix4x4 matrix) =>
        new(matrix.M11, matrix.M22, matrix.M41, matrix.M42, matrix.M12, matrix.M21);

    private static int FillInternalShape(InternalView* self, Direct3D9SoftwareImageRenderer renderer, nint context,
        nint brushContext, nint shape, nint realizer, Matrix4x4 matrix, NativeRenderState state, Direct3D9SurfaceRect clip,
        SoftwareImageCoverage? strokeCoverage = null)
    {
        return ConsumeSoftwareRealization(renderer, realizer, brushContext, context,
            (nint)(&self->CreatorTable), shape, matrix, state, clip, strokeCoverage);
    }

    private static int ConsumeSoftwareRealization(Direct3D9SoftwareImageRenderer renderer, nint realizer,
        nint brushContext, nint context, nint creator, nint shape, Matrix4x4 matrix,
        NativeRenderState state, Direct3D9SurfaceRect clip, SoftwareImageCoverage? strokeCoverage,
        SoftwareImageDrawingContext? drawingContext = null)
    {
        int result = NativeBrushRealizerReader.Realize(realizer, brushContext, context,
            creator, out nint brush, out nint effects);
        JpBrushTrace.Record("Path.Realize", result, $"shape={shape};realizer={realizer};brush={brush};effects={effects};clip={clip};coverage={strokeCoverage is not null};matrix={matrix}");
        if (result < 0) return result;
        if (brush == 0) return 0;
        result = SoftwareBitmapEffects.Capture(effects, out var snapshot);
        JpBrushTrace.Record("Path.Effects", result);
        if (result < 0) return result;
        using var ownedEffects = snapshot;
        uint brushType = ((delegate* unmanaged<nint, uint>)(*(void***)brush)[3])(brush);
        if (brushType is not (1 or 4)) return NotImplemented;
        if (matrix.M13 != 0 || matrix.M14 != 0 || matrix.M23 != 0 || matrix.M24 != 0
            || matrix.M31 != 0 || matrix.M32 != 0 || matrix.M33 != 1 || matrix.M34 != 0
            || matrix.M43 != 0 || matrix.M44 != 1) return NotImplemented;
        var transform = new GeneratedImageTransform(matrix.M11, matrix.M22, matrix.M41, matrix.M42, matrix.M12, matrix.M21);
        var coverage = strokeCoverage;
        if (coverage is null)
        {
            result = NativeShapeReader.Capture(shape, transform, out coverage, clip);
            JpBrushTrace.Record("Path.Shape", result, $"shape={shape};transform={transform};clip={clip}");
            if (result < 0) return result;
        }
        var drawing = drawingContext ?? new SoftwareImageDrawingContext(GeneratedImageTransform.Identity, clip, 3, state.Compositing,
            Antialias: state.AntiAlias != 0, Effects: snapshot);
        if (drawing.GeometryMask is { } contextCoverage)
            coverage = coverage is null ? contextCoverage
                : SoftwareImageCoverage.Combine(coverage, contextCoverage, MilCombineMode.Intersect);
        result = brushType == 4
            ? FillInternalBitmapBrush(renderer, brush, transform, coverage, drawing, state)
            : ImmediateSolidBrushRealizer.DrawBorrowedSolid(renderer, brush, coverage, drawing);
        JpBrushTrace.Record("Path.Brush", result, $"type={brushType};clip={clip};coverage={coverage is not null};transform={transform}");
        return result;
    }

    internal static int DrawSoftwareRealization(Direct3D9SoftwareImageRenderer renderer, nint realizer,
        SoftwareImageCoverage? coverage, SoftwareImageDrawingContext context)
    {
        var state = CreateSoftwareRenderState(context);
        var clip = context.AliasedClip ?? new Direct3D9SurfaceRect(0, 0, int.MaxValue, int.MaxValue);
        return ConsumeSoftwareRealization(renderer, realizer, 0, 0, 0, 0,
            Matrix4x4.Identity, state, clip, coverage, context);
    }

    private static int FillInternalBitmapBrush(Direct3D9SoftwareImageRenderer renderer, nint brush,
        GeneratedImageTransform worldToDevice, SoftwareImageCoverage? coverage, SoftwareImageDrawingContext drawing,
        NativeRenderState state)
    {
        NativeBitmapBrushLayout* bitmapBrush = (NativeBitmapBrushLayout*)brush;
        uint space = bitmapBrush->Space;
        Matrix4x4 matrix = bitmapBrush->Mapping;
        nint source = bitmapBrush->Source;
        uint wrap = bitmapBrush->Wrap;
        float opacity = bitmapBrush->Opacity;
        if (space > 1 || wrap > 5) return NotImplemented;
        Vector4 border = bitmapBrush->Border;
        if (!float.IsFinite(opacity)) return InvalidArgument;
        if (source == 0) return 0;
        if (matrix.M13 != 0 || matrix.M14 != 0 || matrix.M23 != 0 || matrix.M24 != 0
            || matrix.M31 != 0 || matrix.M32 != 0 || matrix.M33 != 1 || matrix.M34 != 0
            || matrix.M43 != 0 || matrix.M44 != 1) return NotImplemented;
        var mapping = NativeImmediateSolidRealization.TryGetExactMapping(brush, out var exactMapping)
            ? exactMapping
            : new GeneratedImageTransform(matrix.M11, matrix.M22, matrix.M41, matrix.M42, matrix.M12, matrix.M21);
        if (space == 1) mapping = worldToDevice.Prepend(mapping);
        if (!mapping.TryInvert(out _)) return 0;
        if (bitmapBrush->HasSourceClip != 0)
        {
            Vector2* points = &bitmapBrush->Clip.Data.P0;
            var clipTransform = space == 1 ? worldToDevice : GeneratedImageTransform.Identity;
            var polygon = new MilPoint2D[4];
            if (NativeImmediateSolidRealization.TryGetExactSourceClip(brush, out var rectangle))
            {
                var exactClip = NativeImmediateSolidRealization.TryGetExactClipMapping(brush, out var independentClip)
                    ? space == 1 ? worldToDevice.Prepend(independentClip) : independentClip
                    : mapping;
                var p0 = exactClip.Apply(rectangle.X, rectangle.Y);
                var p1 = exactClip.Apply(rectangle.X + rectangle.Width, rectangle.Y);
                var p2 = exactClip.Apply(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);
                var p3 = exactClip.Apply(rectangle.X, rectangle.Y + rectangle.Height);
                polygon[0] = new(p0.X, p0.Y);
                polygon[1] = new(p1.X, p1.Y);
                polygon[2] = new(p2.X, p2.Y);
                polygon[3] = new(p3.X, p3.Y);
            }
            else
            {
                for (int i = 0; i < polygon.Length; i++)
                {
                    var point = clipTransform.Apply(points[i].X, points[i].Y);
                    polygon[i] = new(point.X, point.Y);
                }
            }
            if (NativeImmediateSolidRealization.TryGetExactClipMapping(brush, out var viewportMapping)
                && NativeImmediateSolidRealization.TryGetExactSourceClip(brush, out var viewport))
            {
                if (space == 1) viewportMapping = worldToDevice.Prepend(viewportMapping);
                bool captured = SoftwareImageCoverage.TryCreate(viewport, viewportMapping, out var viewportCoverage, renderer.TargetBounds);
                JpBrushTrace.Record("Path.ViewportCoverage", captured ? 0 : InvalidArgument, $"viewport={viewport};mapping={viewportMapping};target={renderer.TargetBounds};polygon={string.Join(';', polygon)}");
                if (!captured) return InvalidArgument;
                if (coverage is null) coverage = viewportCoverage;
                else if (polygon.All(point => Math.Abs(point.X) < 524288 && Math.Abs(point.Y) < 524288)
                    && coverage.TryClipParallelogram(polygon, out var clippedCoverage))
                    coverage = clippedCoverage;
                else coverage = SoftwareImageCoverage.Combine(coverage, viewportCoverage, MilCombineMode.Intersect);
            }
            else
            {
                bool captured = SoftwarePathCoverage.TryCreateFigures([polygon], MilFillMode.Winding, out var path);
                JpBrushTrace.Record("Path.SourceClipCoverage", captured ? 0 : InvalidArgument, $"mapping={mapping};clipTransform={clipTransform};target={renderer.TargetBounds};polygon={string.Join(';', polygon)}");
                if (!captured) return InvalidArgument;
                if (path is null) return Direct3D9Factory.UnexpectedHResult;
                var sourceClip = new SoftwareImageCoverage(path);
                if (coverage is null) coverage = sourceClip;
                else if (!coverage.TryClipParallelogram(polygon, out coverage)) return NotImplemented;
            }
        }
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source)[1])(source);
        try
        {
            uint width, height;
            int result = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)source)[3])(source, &width, &height);
            if (result < 0) return result;
            if (width == 0 || height == 0) return 0;
            // Extend must cover the shape, not stop at the transformed bitmap rectangle.
            var wrapMode = (MilBitmapWrapMode)wrap;
            return renderer.DrawTransformed(source, new(0, 0, width, height), drawing with
            {
                WorldToDevice = mapping,
                WorkingFormat = NativeImmediateSolidRealization.GetWorkingFormat(brush),
                GeometryMask = coverage,
                WrapMode = wrapMode,
                BorderColor = new MilColorF(border.W, border.X, border.Y, border.Z),
                ExtendThroughGeometry = true,
                AlphaScale = drawing.AlphaScale * Math.Clamp(opacity, 0, 1),
                BitmapScalingMode = state.Interpolation == 0 ? 3u : state.Prefilter != 0 ? 2u : 1u,
                PrefilterEnabled = state.Prefilter != 0,
                PrefilterThreshold = state.PrefilterThreshold
            }, true);
        }
        finally { Direct3D9Factory.Release(source); }
    }

    private static int DrawInternalBitmap(InternalView* self, nint context, nint bitmap, nint effects)
    {
        if (context == 0 || bitmap == 0) return InvalidArgument;
        int clipResult = GetInternalClip(self, context, out var currentClip);
        if (clipResult < 0) return clipResult;
        if (currentClip.Right <= currentClip.Left || currentClip.Bottom <= currentClip.Top) return 0;

        var native = (NativeContextPrefix*)context;
        if (native->RenderState == null) return InvalidArgument;
        NativeRenderState state = *native->RenderState;
        Matrix4x4 m = native->WorldToDevice;
        if (native->In3D != 0 || (state.Options & ~1u) != 0 || state.AntiAlias > 1
            || state.Interpolation > 5 || state.Compositing is not (MilCompositingMode.SourceOver or MilCompositingMode.SourceCopy)
            || m.M13 != 0 || m.M14 != 0 || m.M23 != 0 || m.M24 != 0
            || m.M31 != 0 || m.M32 != 0 || m.M33 != 1 || m.M34 != 0 || m.M43 != 0 || m.M44 != 1)
            return NotImplemented;
        if (!float.IsFinite(m.M11) || !float.IsFinite(m.M12) || !float.IsFinite(m.M21)
            || !float.IsFinite(m.M22) || !float.IsFinite(m.M41) || !float.IsFinite(m.M42)) return InvalidArgument;
        if (state.Prefilter != 0 && !float.IsFinite(state.PrefilterThreshold)) return InvalidArgument;
        Direct3D9SurfaceRect? clip = currentClip;
        nint target = (nint)self->Owner;
        // Effect-list callbacks can release caller-owned references.
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)target)[1])(target);
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)bitmap)[1])(bitmap);
        try
        {
            int effectResult = SoftwareBitmapEffects.Capture(effects, out var effectSnapshot);
            if (effectResult < 0) return effectResult;
            using var ownedEffects = effectSnapshot;
            var drawContext = new SoftwareImageDrawingContext(new(m.M11, m.M22, m.M41, m.M42, m.M12, m.M21),
                clip, state.Interpolation == 0 ? 3u : state.Prefilter != 0 ? 2u : 1u, state.Compositing,
                (state.Options & 1) != 0 ? new MilRectD(state.SourceX, state.SourceY, state.SourceWidth, state.SourceHeight) : null,
                state.PrefilterThreshold, state.AntiAlias != 0, state.Prefilter != 0, effectSnapshot);
            uint width, height;
            int result = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)bitmap)[3])(bitmap, &width, &height);
            if (result < 0) return result;
            result = SoftwareImageRenderSession.Open(target, uint.MaxValue, uint.MaxValue, out var session);
            if (result < 0) return result;
            using (session)
                return session is null ? Direct3D9Factory.UnexpectedHResult
                    : session.Draw(bitmap, new(0, 0, width, height), true, drawContext);
        }
        finally { Direct3D9Factory.Release(bitmap); Direct3D9Factory.Release(target); }
    }
}
