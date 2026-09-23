namespace WpfGfxShape.Core;

internal sealed class Direct3D9ProductionGlyphRenderer : IDisposable
{
    private readonly Func<int> _paint;
    private readonly Action _release;
    private bool _disposed;

    internal Direct3D9ProductionGlyphRenderer(Func<int> paint, Action release)
    {
        ArgumentNullException.ThrowIfNull(paint);
        ArgumentNullException.ThrowIfNull(release);
        _paint = paint;
        _release = release;
    }

    internal int Paint()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _paint();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _release();
    }
}

internal delegate int Direct3D9CreateProductionGlyphRenderer(
    bool targetSupportsClearType,
    out Direct3D9ProductionGlyphRenderer? renderer);

internal sealed record Direct3D9ProductionGlyphDrawOperations(
    Direct3D9EnsureGlyphBrushRealization EnsureHardwareBrushRealization,
    Func<int> EnsureState,
    Direct3D9CreateProductionGlyphRenderer CreateHardwareRenderer,
    Direct3D9SoftwareGlyphRenderer SoftwareRenderer,
    bool HasGlyphs = true);