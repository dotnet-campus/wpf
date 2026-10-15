namespace WpfGfxShape.Core;

internal sealed class FrozenCacheRequest : IDisposable
{
    private bool _disposed;
    internal GeneratedBitmapCacheBrushResource Brush { get; }
    internal GeneratedVisualResource? Target { get; }
    internal GeneratedBitmapCacheResource? Config { get; }
    internal int BrushChange { get; }
    internal int TargetChange { get; }
    internal int ConfigChange { get; }
    internal double Scale { get; }
    internal bool Snaps { get; }
    internal bool ClearType { get; }
    internal double CurrentOpacity { get; }
    internal GeneratedImageTransform Transform { get; }
    internal GeneratedImageTransform? RelativeTransform { get; }
    internal FrozenVisualTree? Tree { get; private set; }

    private FrozenCacheRequest(GeneratedBitmapCacheBrushResource brush,
        GeneratedImageTransform transform, GeneratedImageTransform? relative)
    {
        Brush = brush;
        Target = brush.InternalTarget as GeneratedVisualResource;
        Config = brush.BitmapCache as GeneratedBitmapCacheResource;
        BrushChange = brush.ChangeCount;
        TargetChange = Target?.ChangeCount ?? 0;
        ConfigChange = Config?.ChangeCount ?? 0;
        Scale = Config?.CurrentRenderAtScale ?? 1;
        Snaps = Config?.SnapsToDevicePixels ?? false;
        ClearType = Config?.EnableClearType ?? false;
        CurrentOpacity = brush.CurrentOpacity;
        Transform = transform;
        RelativeTransform = relative;
        Brush.AddRef();
        Target?.AddRef();
        Config?.AddRef();
    }

    internal static int CapturePure(GeneratedBitmapCacheBrushResource brush,
        HashSet<GeneratedProtocolResource> active, out FrozenCacheRequest? request)
    {
        request = null;
        if (active.Count >= 256 || !active.Add(brush)) return Direct3D9Factory.NotImplementedHResult;
        FrozenCacheRequest? candidate = null;
        try
        {
            if (!GeneratedImageTransform.TryResolve(brush.Transform, out var transform)
                || !GeneratedImageTransform.TryResolve(brush.RelativeTransform, out var relative))
                return Direct3D9Factory.NonInvertibleMatrixHResult;
            candidate = new(brush, transform, brush.RelativeTransform is null ? null : relative);
            if (candidate.Target is { } root)
            {
                int hr = FrozenVisualTree.CapturePure(root, active, out var tree);
                if (hr < 0) return hr;
                candidate.Tree = tree;
            }
            request = candidate;
            return 0;
        }
        finally
        {
            active.Remove(brush);
            if (request is null) candidate?.Dispose();
        }
    }

    internal int RealizeFrozen(MilRectD consumerBounds, GeneratedImageTransform targetTransform,
        double dpiX, double dpiY, bool floating, uint scalingMode, out nint bitmap)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Brush.RealizeFrozen(this, consumerBounds, targetTransform, dpiX, dpiY, floating, scalingMode, out bitmap);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { Tree?.Dispose(); }
        finally
        {
            Tree = null;
            Config?.Release();
            Target?.Release();
            Brush.Release();
        }
    }
}
