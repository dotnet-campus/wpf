namespace WpfGfxShape.Core;

internal sealed class FrozenBrush : IDisposable
{
    private FrozenBitmapLease? _bitmap;
    internal FrozenVisualTree? Drawing { get; private set; }
    internal FrozenVisualTree? Visual { get; private set; }
    private nint _selectedSource;
    private bool _interop;
    private GeneratedProtocolResource? _owner;
    private bool _disposed;

    internal GeneratedProtocolResource? Source { get; private init; }
    internal GeneratedImageTransform Transform { get; private init; }
    internal GeneratedImageTransform? RelativeTransform { get; private init; }
    internal MilRectD CurrentViewport { get; private init; }
    internal MilRectD CurrentViewbox { get; private init; }
    internal double CurrentOpacity { get; private init; }
    internal MilBrushMappingMode ViewportUnits { get; private init; }
    internal MilBrushMappingMode ViewboxUnits { get; private init; }
    internal MilStretch Stretch { get; private init; }
    internal MilTileMode TileMode { get; private init; }
    internal MilHorizontalAlignment AlignmentX { get; private init; }
    internal MilVerticalAlignment AlignmentY { get; private init; }
    internal bool HasContent { get; private set; }

    internal static int CaptureSourcePure(GeneratedProtocolResource source, out FrozenBrush? snapshot)
    {
        snapshot = new FrozenBrush { Source = source };
        snapshot._owner = source;
        source.AddRef();
        switch (source)
        {
            case GeneratedBitmapSourceResource bitmap:
                snapshot._bitmap = bitmap.PinBitmapSource();
                snapshot._selectedSource = snapshot._bitmap?.Pointer ?? 0;
                break;
            case GeneratedDoubleBufferedBitmapResource bitmap:
                snapshot._bitmap = bitmap.PinBitmapSource(out snapshot._selectedSource);
                break;
            case GeneratedD3DImageResource image:
                snapshot._bitmap = image.PinSoftwareSource();
                if (snapshot._bitmap is null) { snapshot._bitmap = image.PinInteropSource(); snapshot._interop = true; }
                snapshot._selectedSource = snapshot._bitmap?.Pointer ?? 0;
                break;
            default:
                snapshot.Dispose(); snapshot = null;
                return Direct3D9Factory.NotImplementedHResult;
        }
        snapshot.HasContent = snapshot._selectedSource != 0;
        return 0;
    }

    internal static int CapturePure(GeneratedTileBrushResource brush, out FrozenBrush? snapshot,
        HashSet<GeneratedProtocolResource>? active = null)
    {
        snapshot = null;
        if (!GeneratedImageTransform.TryResolve(brush.Transform, out var transform)
            || !GeneratedImageTransform.TryResolve(brush.RelativeTransform, out var relative))
            return Direct3D9Factory.NotImplementedHResult;
        var candidate = new FrozenBrush
        {
            Source = brush.Source, Transform = transform,
            RelativeTransform = brush.RelativeTransform is null ? null : relative,
            CurrentViewport = brush.CurrentViewport, CurrentViewbox = brush.CurrentViewbox,
            CurrentOpacity = brush.CurrentOpacity, ViewportUnits = brush.ViewportUnits,
            ViewboxUnits = brush.ViewboxUnits, Stretch = brush.Stretch, TileMode = brush.TileMode,
            AlignmentX = brush.AlignmentX, AlignmentY = brush.AlignmentY
        };
        bool captured = false;
        try
        {
            candidate._owner = candidate.Source;
            candidate._owner?.AddRef();
            switch (candidate.Source)
            {
                case GeneratedDrawingDependencyResource drawingResource when brush is GeneratedDrawingBrushResource:
                    active ??= [];
                    if (active.Count >= 256 || !active.Add(drawingResource)) return Direct3D9Factory.NotImplementedHResult;
                    try
                    {
                        int hr = FrozenVisualTree.CaptureDrawingPure(drawingResource, active, out var drawing);
                        if (hr < 0) return hr;
                        candidate.Drawing = drawing;
                    }
                    finally { active.Remove(drawingResource); }
                    break;
                case GeneratedVisualResource visual:
                    active ??= [];
                    int visualResult = FrozenVisualTree.CapturePure(visual, active, out var tree);
                    if (visualResult < 0) return visualResult;
                    candidate.Visual = tree;
                    break;
                case GeneratedDrawingImageResource image:
                    active ??= [];
                    if (active.Count >= 256 || !active.Add(image)) return Direct3D9Factory.NotImplementedHResult;
                    try
                    {
                        int hr = FrozenVisualTree.CaptureDrawingPure(image.Drawing, active, out var drawing);
                        if (hr < 0) return hr;
                        candidate.Drawing = drawing;
                    }
                    finally { active.Remove(image); }
                    break;
                case GeneratedBitmapSourceResource bitmap:
                    candidate._bitmap = bitmap.PinBitmapSource();
                    candidate._selectedSource = candidate._bitmap?.Pointer ?? 0;
                    break;
                case GeneratedDoubleBufferedBitmapResource bitmap:
                    candidate._bitmap = bitmap.PinBitmapSource(out candidate._selectedSource);
                    break;
                case GeneratedD3DImageResource image:
                    candidate._bitmap = image.PinSoftwareSource();
                    if (candidate._bitmap is null)
                    {
                        candidate._bitmap = image.PinInteropSource();
                        candidate._interop = true;
                    }
                    candidate._selectedSource = candidate._bitmap?.Pointer ?? 0;
                    break;
            }
            candidate.HasContent = candidate.Drawing is not null || candidate.Visual is not null
                || candidate._selectedSource != 0;
            snapshot = candidate;
            captured = true;
            return 0;
        }
        finally { if (!captured) candidate.Dispose(); }
    }

    internal nint AcquireSource()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_interop)
            return WpfGfxShape.Abi.InteropDeviceBitmapExports.AcquireBitmapSource(_selectedSource);
        Direct3D9Factory.AddRef(_selectedSource);
        return _selectedSource;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Visual?.Dispose();
        Visual = null;
        Drawing?.Dispose();
        Drawing = null;
        var bitmap = _bitmap;
        var owner = _owner;
        _bitmap = null;
        _owner = null;
        _selectedSource = 0;
        try { bitmap?.Dispose(); }
        finally { owner?.Release(); }
    }
}
