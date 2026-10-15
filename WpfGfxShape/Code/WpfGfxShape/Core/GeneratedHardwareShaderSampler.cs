using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal static unsafe class GeneratedHardwareShaderSampler
{
    internal static int Render(GeneratedProtocolResource brush, Direct3D9Device device,
        Direct3D9SurfaceRenderTarget target, uint width, uint height)
    {
        if (brush is not GeneratedImageBrushResource image)
            return Direct3D9Factory.NotImplementedHResult;
        if (image.Source is not GeneratedD3DImageResource source)
            return RenderCapturedImage(image, device, target, width, height);
        if (image.TileMode != MilTileMode.None || image.CurrentOpacity != 1)
            return RenderCapturedImage(image, device, target, width, height);
        if (!GeneratedImageTransform.TryResolve(image.Transform, out var transform)
            || !GeneratedImageTransform.TryResolve(image.RelativeTransform, out var relative))
            return Direct3D9Factory.NonInvertibleMatrixHResult;
        var sizing = new GeneratedImageTransform(width, height, 0, 0);
        if (!sizing.TryInvert(out var unit)) return Direct3D9Factory.NonInvertibleMatrixHResult;
        transform = transform.Prepend(sizing.Prepend(relative).Prepend(unit));
        MilRectD viewport = image.CurrentViewport;
        if (image.ViewportUnits == MilBrushMappingMode.RelativeToBoundingBox)
            viewport = new(viewport.X * width, viewport.Y * height,
                viewport.Width * width, viewport.Height * height);
        MilRectD content = source.GetBounds(), box = image.CurrentViewbox;
        if (image.ViewboxUnits == MilBrushMappingMode.RelativeToBoundingBox)
            box = new(content.X + box.X * content.Width, content.Y + box.Y * content.Height,
                box.Width * content.Width, box.Height * content.Height);
        if (content.Width <= 0 || content.Height <= 0 || box.Width <= 0 || box.Height <= 0
            || viewport.Width <= 0 || viewport.Height <= 0) return 0;
        double sx = viewport.Width / box.Width, sy = viewport.Height / box.Height;
        switch (image.Stretch)
        {
            case MilStretch.None: sx = sy = 1; break;
            case MilStretch.Uniform: sx = sy = Math.Min(sx, sy); break;
            case MilStretch.UniformToFill: sx = sy = Math.Max(sx, sy); break;
        }
        double ax = (uint)image.AlignmentX * 0.5, ay = (uint)image.AlignmentY * 0.5;
        double tx = viewport.X + viewport.Width * ax - (box.X + box.Width * ax) * sx;
        double ty = viewport.Y + viewport.Height * ay - (box.Y + box.Height * ay) * sy;
        var mapping = transform.Prepend(new(sx, sy, tx, ty));
        if (!mapping.TryInvert(out _)) return Direct3D9Factory.NonInvertibleMatrixHResult;
        byte[] rectangle = new byte[Marshal.SizeOf<MilRectD>()];
        MemoryMarshal.Write(rectangle, in content);
        // The secondary brush is realized before any shader sampler is bound.
        if (!transform.IsPositiveAxisAligned)
            return RenderCapturedImage(image, device, target, width, height);
        var deviceViewport = transform.Apply(viewport);
        if (!double.IsFinite(deviceViewport.X) || !double.IsFinite(deviceViewport.Y)
            || !double.IsFinite(deviceViewport.Width) || !double.IsFinite(deviceViewport.Height))
            return Direct3D9Factory.InvalidCallHResult;
        var clip = new Direct3D9SurfaceRect(
            checked((int)Math.Floor(deviceViewport.X)), checked((int)Math.Floor(deviceViewport.Y)),
            checked((int)Math.Ceiling(deviceViewport.X + deviceViewport.Width)),
            checked((int)Math.Ceiling(deviceViewport.Y + deviceViewport.Height)));
        return GeneratedHardwareVisualRenderer.Draw(
            [new(GeneratedRenderDataKind.DrawImage, [source], rectangle, [source])],
            device, target, mapping, clip, 0, MilCompositingMode.SourceOver);
    }

    private static int RenderCapturedImage(GeneratedImageBrushResource image, Direct3D9Device device,
        Direct3D9SurfaceRenderTarget target, uint width, uint height)
    {
        var bounds = new MilRectD(0, 0, width, height);
        var clip = new Direct3D9SurfaceRect(0, 0, checked((int)width), checked((int)height));
        int capture = SoftwareBrushRealization.Capture(image, GeneratedImageTransform.Identity, 0,
            bounds, out var brush, clip);
        if (capture < 0) return capture;
        using (brush) return RenderCapturedBrush(brush, device, target, width, height);
    }

    internal static int RenderCapturedVisual(GeneratedVisualRenderer.VisualSnapshot snapshot,
        Direct3D9Device device, Direct3D9SurfaceRenderTarget target, uint width, uint height)
    {
        bool floating = target.GetDirect3DTextureFormat() == Silk.NET.Direct3D9.Format.A32B32G32R32f;
        int hr = snapshot.Realize(out nint bitmap, floating);
        if (hr < 0) return hr;
        try { return UploadCapturedBitmap(bitmap, floating, device, target, width, height); }
        finally { Direct3D9Factory.Release(bitmap); }
    }

    private static int UploadCapturedBitmap(nint bitmap, bool floating, Direct3D9Device device,
        Direct3D9SurfaceRenderTarget target, uint width, uint height)
    {
        var description = new Silk.NET.Direct3D9.SurfaceDesc
        {
            Width = width, Height = height,
            Format = floating ? Silk.NET.Direct3D9.Format.A32B32G32R32f : Silk.NET.Direct3D9.Format.A8R8G8B8
        };
        int hr = Direct3D9InteropColorSource.TryCreateFromBitmap(device.Device, description, bitmap, out var uploaded);
        if (hr < 0) return hr;
        using (uploaded)
        {
            if (uploaded is null) return Direct3D9Factory.InvalidCallHResult;
            var uv = new System.Numerics.Matrix3x2(1f / width, 0, 0, 1f / height, 0, 0);
            var pipeline = uploaded.CreatePipelineColorSource(device, uv,
                MilBitmapInterpolationMode.NearestNeighbor,
                Silk.NET.Direct3D9.Textureaddress.Clamp, Silk.NET.Direct3D9.Textureaddress.Clamp);
            var clip = new Direct3D9SurfaceRect(0, 0, checked((int)width), checked((int)height));
            using var operations = new GeneratedHardwareBitmapOperations(device, target, pipeline,
                GeneratedImageTransform.Identity, clip, MilCompositingMode.SourceCopy);
            return target.ProductionDrawBitmap(new(System.Numerics.Matrix4x4.Identity,
                new Direct3D9PointAndSizeRect(0, 0, checked((int)width), checked((int)height))),
                operations.BitmapSource, 0, operations.Operations);
        }
    }

    internal static int RenderCapturedBrush(SoftwareBrushRealization? brush, Direct3D9Device device,
        Direct3D9SurfaceRenderTarget target, uint width, uint height)
    {
        var bounds = new MilRectD(0, 0, width, height);
        var clip = new Direct3D9SurfaceRect(0, 0, checked((int)width), checked((int)height));
        {
            nint bitmapTarget = 0, bitmap = 0;
            SoftwareImageRenderSession? session = null;
            try
            {
                bool floating = target.GetDirect3DTextureFormat() == Silk.NET.Direct3D9.Format.A32B32G32R32f;
                int hr = WpfGfxShape.Abi.BitmapRenderTargetExports.CreateDrawingIntermediate(
                    width, height, out bitmapTarget, out bitmap, floating);
                if (hr < 0) return hr;
                hr = SoftwareImageRenderSession.Open(bitmapTarget, width, height, out session);
                if (hr < 0) return hr;
                hr = session!.DrawPath(brush, null, null, null, bounds,
                    new(GeneratedImageTransform.Identity, clip, 0, MilCompositingMode.SourceCopy));
                if (hr < 0) return hr;
                session.Dispose(); session = null;
                return UploadCapturedBitmap(bitmap, floating, device, target, width, height);
            }
            finally
            {
                session?.Dispose();
                Direct3D9Factory.Release(bitmap); Direct3D9Factory.Release(bitmapTarget);
            }
        }
    }
}
