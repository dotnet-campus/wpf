using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal static class GeneratedHardwareVisualRenderer
{
    internal static int Render(GeneratedVisualResource visual, Direct3D9Device device,
        Direct3D9SurfaceRenderTarget target, uint width, uint height)
    {
        int hr = FrozenVisualTree.CapturePure(visual, [], out var tree);
        if (hr < 0)
        {
            PartitionScheduler.RecordCommandFailure(0, "Window.CapturePure"u8, hr);
            return hr;
        }
        using (tree)
        {
            if (tree is null) return Direct3D9Factory.InvalidCallHResult;
            hr = Visit(tree, device, target, GeneratedImageTransform.Identity,
                new(0, 0, checked((int)width), checked((int)height)), 0);
            if (hr < 0) PartitionScheduler.RecordCommandFailure(0, "Window.Visit"u8, hr);
            return hr;
        }
    }

    internal static int Visit(FrozenVisualTree visual, Direct3D9Device device,
        Direct3D9SurfaceRenderTarget target, GeneratedImageTransform parent,
        Direct3D9SurfaceRect? inheritedClip, int depth)
    {
        if (depth > 256 || visual.Alpha != 1 || visual.AlphaMask is not null)
            return Direct3D9Factory.NotImplementedHResult;
        var transform = parent.Prepend(new(1, 1, visual.Offset.X, visual.Offset.Y)).Prepend(visual.Transform);
        if (!GeneratedImageClip.TryIntersect(visual.Clip?.Geometry, transform, inheritedClip, out var clip))
            return Direct3D9Factory.NotImplementedHResult;
        if (visual.HasUnsupportedEffect) return Direct3D9Factory.NotImplementedHResult;
        if (visual.Effect is { } invocation)
        {
            if (invocation.CaptureResult < 0) return invocation.CaptureResult;
            if (!GeneratedImageContentBounds.TryGet(visual, out var contentBounds))
                return Direct3D9Factory.NotImplementedHResult;
            int scopeResult = GeneratedShaderInputScope.TryCreate(contentBounds, invocation.Data.Command,
                transform, out var scope);
            if (scopeResult == Direct3D9Factory.ClippedToEmptyHResult) return 0;
            if (scopeResult < 0) return scopeResult;
            int captureResult = GeneratedVisualRenderer.DrawingSnapshot.RealizeTree(visual, contentBounds,
                scope.InputTransform, scope.Width, scope.Height, visual.BitmapScalingMode ?? 0,
                true, out var inputSnapshot);
            if (captureResult < 0) return captureResult;
            if (inputSnapshot is null) return Direct3D9Factory.InvalidCallHResult;
            GeneratedVisualRenderer.VisualSnapshot input;
            try { input = new(inputSnapshot); }
            catch (OutOfMemoryException) { inputSnapshot.Dispose(); throw; }
            using (input)
                return target.DrawBitmap(() => GeneratedHardwareShaderEffect.Render(invocation, device, target,
                    clip, scope, contentBounds, input));
        }
        return DrawContents(visual, device, target, transform, clip, depth);
    }

    private static int DrawContents(FrozenVisualTree visual, Direct3D9Device device,
        Direct3D9SurfaceRenderTarget target, GeneratedImageTransform transform,
        Direct3D9SurfaceRect? clip, int depth)
    {
        int drawResult = Draw(visual.Instructions, device, target, transform, clip,
            visual.BitmapScalingMode ?? 0, visual.CompositingMode ?? MilCompositingMode.SourceOver, visual);
        if (drawResult < 0)
        {
            PartitionScheduler.RecordCommandFailure(0,
                System.Text.Encoding.UTF8.GetBytes($"Window.Draw depth={depth} kinds={string.Join(',', visual.Instructions.Select(static item => item.Kind))}"), drawResult);
            return drawResult;
        }
        foreach (var child in visual.Children)
        {
            int hr = Visit(child, device, target, transform, clip, depth + 1);
            if (hr < 0) return hr;
        }
        return 0;
    }

    internal static int Draw(IReadOnlyList<GeneratedRenderDataInstruction> instructions,
        Direct3D9Device device, Direct3D9SurfaceRenderTarget target,
        GeneratedImageTransform transform, Direct3D9SurfaceRect? clip, uint scaling,
        MilCompositingMode compositing, FrozenVisualTree? frozen = null)
    {
        var stack = new Stack<(GeneratedImageTransform Transform, Direct3D9SurfaceRect? Clip)>();
        foreach (var instruction in instructions)
        {
            if (instruction.Kind == GeneratedRenderDataKind.PushTransform)
            {
                if (!GeneratedImageTransform.TryResolve(instruction.ResourceSlots[0], out var local))
                    return Direct3D9Factory.NonInvertibleMatrixHResult;
                stack.Push((transform, clip)); transform = transform.Prepend(local); continue;
            }
            if (instruction.Kind == GeneratedRenderDataKind.PushClip)
            {
                if (!GeneratedImageClip.TryIntersect(instruction.ResourceSlots[0], transform, clip, out var next))
                    return Direct3D9Factory.NotImplementedHResult;
                stack.Push((transform, clip)); clip = next; continue;
            }
            if (instruction.Kind == GeneratedRenderDataKind.Pop)
            {
                if (!stack.TryPop(out var previous)) return Direct3D9Factory.UceMalformedPacketHResult;
                (transform, clip) = previous; continue;
            }
            if (instruction.Kind is GeneratedRenderDataKind.DrawRectangle or GeneratedRenderDataKind.DrawRectangleAnimate
                or GeneratedRenderDataKind.DrawRoundedRectangle or GeneratedRenderDataKind.DrawRoundedRectangleAnimate)
            {
                int rectangleResult = GeneratedHardwareRectangle.Draw(instruction, device, target, transform, clip, compositing);
                if (rectangleResult < 0) return rectangleResult;
                continue;
            }
            if (instruction.Kind == GeneratedRenderDataKind.DrawGeometry)
            {
                int geometryResult = GeneratedHardwareCacheGeometry.Draw(instruction, frozen, device,
                    target, transform, clip, scaling, compositing);
                if (geometryResult < 0) return geometryResult;
                continue;
            }
            if (instruction.Kind is not (GeneratedRenderDataKind.DrawImage or GeneratedRenderDataKind.DrawImageAnimate))
                return Direct3D9Factory.NotImplementedHResult;
            if (instruction.ResourceSlots[0] is GeneratedBitmapSourceResource or GeneratedDoubleBufferedBitmapResource)
            {
                int bitmapResult = DrawWic(instruction, frozen, device, target, transform, clip, scaling, compositing);
                if (bitmapResult < 0) return bitmapResult;
                continue;
            }
            if (instruction.ResourceSlots[0] is not GeneratedD3DImageResource image)
                return Direct3D9Factory.NotImplementedHResult;
            MilRectD rectangle = MemoryMarshal.Read<MilRectD>(instruction.Data.Span);
            if (instruction.Kind == GeneratedRenderDataKind.DrawImageAnimate
                && instruction.ResourceSlots[1] is GeneratedValueResource<MilRectD> animated)
                rectangle = animated.Value;
            if (rectangle.Width <= 0 || rectangle.Height <= 0) continue;
            MilRectD contentBounds = image.GetBounds();
            if (contentBounds.Width <= 0 || contentBounds.Height <= 0) continue;
            var sourceBounds = new Direct3D9PointAndSizeRect(0, 0,
                checked((int)Math.Ceiling(contentBounds.Width)),
                checked((int)Math.Ceiling(contentBounds.Height)));
            if (!transform.TryInvert(out var inverse)) return Direct3D9Factory.NonInvertibleMatrixHResult;
            var uv = new System.Numerics.Matrix3x2(
                (float)(inverse.ScaleX / rectangle.Width), (float)(inverse.M12 / rectangle.Height),
                (float)(inverse.M21 / rectangle.Width), (float)(inverse.ScaleY / rectangle.Height),
                (float)((inverse.X - rectangle.X) / rectangle.Width),
                (float)((inverse.Y - rectangle.Y) / rectangle.Height));
            int sourceResult = image.TryCreateDependentPipelineColorSource(device, uv,
                scaling == 1 ? MilBitmapInterpolationMode.NearestNeighbor : MilBitmapInterpolationMode.Linear,
                Textureaddress.Clamp, Textureaddress.Clamp, out var source);
            if (sourceResult < 0) return sourceResult;
            if (source is null) continue;
            if (compositing is not (MilCompositingMode.SourceOver or MilCompositingMode.SourceCopy))
                return Direct3D9Factory.NotImplementedHResult;
            using var operations = new GeneratedHardwareBitmapOperations(device, target, source,
                transform.Prepend(new(rectangle.Width / sourceBounds.Width,
                    rectangle.Height / sourceBounds.Height,
                    rectangle.X - sourceBounds.X * rectangle.Width / sourceBounds.Width,
                    rectangle.Y - sourceBounds.Y * rectangle.Height / sourceBounds.Height)),
                clip ?? new Direct3D9SurfaceRect(0, 0, int.MaxValue, int.MaxValue), compositing);
            int hr = target.ProductionDrawBitmap(
                new Direct3D9BitmapDrawState(System.Numerics.Matrix4x4.Identity, sourceBounds),
                operations.BitmapSource, 0, operations.Operations);
            if (hr < 0) return hr;
        }
        return stack.Count == 0 ? 0 : Direct3D9Factory.UceMalformedPacketHResult;
    }

    private static unsafe int DrawWic(GeneratedRenderDataInstruction instruction, FrozenVisualTree? frozen,
        Direct3D9Device device, Direct3D9SurfaceRenderTarget target, GeneratedImageTransform transform,
        Direct3D9SurfaceRect? clip, uint scaling, MilCompositingMode compositing)
    {
        var resource = instruction.ResourceSlots[0]!;
        FrozenBrush? local = null;
        var captured = frozen?.ImageBrush(resource);
        nint bitmap = 0, wic = 0, converted = 0;
        try
        {
            if (captured is null)
            {
                int capture = FrozenBrush.CaptureSourcePure(resource, out local);
                if (capture < 0) return capture;
                captured = local;
            }
            if (captured is null || !captured.HasContent) return 0;
            bitmap = captured.AcquireSource();
            Guid sourceId = new("00000120-a8f2-4877-ba0a-fd2b6645fb94");
            int hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)bitmap)[0])(bitmap, &sourceId, &wic);
            if (hr < 0) return hr;
            uint width, height;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)wic)[3])(wic, &width, &height);
            if (hr < 0) return hr;
            if (width == 0 || height == 0) return 0;
            MilRectD rectangle = MemoryMarshal.Read<MilRectD>(instruction.Data.Span);
            if (instruction.Kind == GeneratedRenderDataKind.DrawImageAnimate
                && instruction.ResourceSlots[1] is GeneratedValueResource<MilRectD> animated)
                rectangle = animated.Value;
            if (rectangle.Width <= 0 || rectangle.Height <= 0) return 0;
            if (!transform.TryInvert(out var inverse)) return Direct3D9Factory.NonInvertibleMatrixHResult;
            if (compositing is not (MilCompositingMode.SourceOver or MilCompositingMode.SourceCopy))
                return Direct3D9Factory.NotImplementedHResult;
            bool floating = target.GetDirect3DTextureFormat() == Format.A32B32G32R32f;
            Guid format = new(floating ? "6fddc324-4e03-4bfe-b185-3d77768dc91a" : "6fddc324-4e03-4bfe-b185-3d77768dc910");
            hr = WpfGfxShape.Abi.BitmapFormatConverter.Create(wic, format, out converted);
            if (hr < 0) return hr;
            var description = new SurfaceDesc { Width = width, Height = height,
                Format = floating ? Format.A32B32G32R32f : Format.A8R8G8B8 };
            hr = Direct3D9InteropColorSource.TryCreateFromBitmap(device.Device, description, converted, out var uploaded);
            if (hr < 0) return hr;
            using (uploaded)
            {
                if (uploaded is null) return Direct3D9Factory.InvalidCallHResult;
                var uv = new System.Numerics.Matrix3x2(
                    (float)(inverse.ScaleX / rectangle.Width), (float)(inverse.M12 / rectangle.Height),
                    (float)(inverse.M21 / rectangle.Width), (float)(inverse.ScaleY / rectangle.Height),
                    (float)((inverse.X - rectangle.X) / rectangle.Width),
                    (float)((inverse.Y - rectangle.Y) / rectangle.Height));
                var source = uploaded.CreatePipelineColorSource(device, uv,
                    scaling == 1 ? MilBitmapInterpolationMode.NearestNeighbor : MilBitmapInterpolationMode.Linear,
                    Textureaddress.Clamp, Textureaddress.Clamp);
                using var operations = new GeneratedHardwareBitmapOperations(device, target, source,
                    transform.Prepend(new(rectangle.Width / width, rectangle.Height / height, rectangle.X, rectangle.Y)),
                    clip ?? new(0, 0, int.MaxValue, int.MaxValue), compositing);
                return target.ProductionDrawBitmap(new(System.Numerics.Matrix4x4.Identity,
                    new Direct3D9PointAndSizeRect(0, 0, checked((int)width), checked((int)height))),
                    operations.BitmapSource, 0, operations.Operations);
            }
        }
        finally
        {
            Direct3D9Factory.Release(converted);
            Direct3D9Factory.Release(wic);
            Direct3D9Factory.Release(bitmap);
            local?.Dispose();
        }
    }
}
