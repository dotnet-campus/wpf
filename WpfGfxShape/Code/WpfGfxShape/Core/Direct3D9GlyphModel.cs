using System.Numerics;

namespace WpfGfxShape.Core;

internal enum Direct3D9GlyphBlendMode
{
    Grayscale,
    ClearType,
}

internal readonly record struct Direct3D9GlyphOffset(float AdvanceOffset, float AscenderOffset);

internal sealed record Direct3D9GlyphRun(
    long CacheKey,
    nint FontFace,
    float EmSize,
    IReadOnlyList<ushort> GlyphIndices,
    IReadOnlyList<float> Advances,
    IReadOnlyList<Direct3D9GlyphOffset> Offsets,
    Matrix3x2 GlyphToDevice,
    MilRectF Bounds,
    bool UseSubpixelPositioning)
{
    internal int Validate()
    {
        if (CacheKey == 0 || FontFace == 0 || !float.IsFinite(EmSize) || EmSize <= 0)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        int count = GlyphIndices.Count;
        if (Advances.Count != count || Offsets.Count != count)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        if (!IsFinite(GlyphToDevice)
            || !float.IsFinite(Bounds.Left)
            || !float.IsFinite(Bounds.Top)
            || !float.IsFinite(Bounds.Right)
            || !float.IsFinite(Bounds.Bottom)
            || Bounds.Right < Bounds.Left
            || Bounds.Bottom < Bounds.Top)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        for (int index = 0; index < count; index++)
        {
            Direct3D9GlyphOffset offset = Offsets[index];
            if (!float.IsFinite(Advances[index])
                || !float.IsFinite(offset.AdvanceOffset)
                || !float.IsFinite(offset.AscenderOffset))
            {
                return Direct3D9Factory.InvalidArgumentHResult;
            }
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal bool IsEmpty => GlyphIndices.Count == 0 || Bounds.Right <= Bounds.Left || Bounds.Bottom <= Bounds.Top;

    private static bool IsFinite(Matrix3x2 matrix) =>
        float.IsFinite(matrix.M11)
        && float.IsFinite(matrix.M12)
        && float.IsFinite(matrix.M21)
        && float.IsFinite(matrix.M22)
        && float.IsFinite(matrix.M31)
        && float.IsFinite(matrix.M32);
}

internal readonly record struct Direct3D9GlyphBankKey(
    nint DeviceIdentity,
    uint DisplayIndex,
    long RunKey,
    Direct3D9GlyphBlendMode BlendMode,
    bool UseSubpixelPositioning,
    float ScaleX,
    float ScaleY);

internal sealed class Direct3D9GlyphRealization : IDisposable
{
    private readonly Action _release;
    private bool _disposed;

    internal Direct3D9GlyphRealization(bool isEmpty, Action release)
    {
        ArgumentNullException.ThrowIfNull(release);
        IsEmpty = isEmpty;
        _release = release;
    }

    internal bool IsEmpty { get; }

    internal bool IsPersistent { get; private set; }

    internal void MarkPersistent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        IsPersistent = true;
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

internal delegate int Direct3D9CreateGlyphRealization(
    Direct3D9GlyphRun glyphRun,
    Direct3D9GlyphBankKey key,
    out Direct3D9GlyphRealization? realization);

internal sealed class Direct3D9GlyphBank : IDisposable
{
    private readonly int _capacity;
    private readonly Dictionary<Direct3D9GlyphBankKey, CacheEntry> _entries = [];
    private long _stamp;
    private bool _disposed;

    internal Direct3D9GlyphBank(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        _capacity = capacity;
    }

    internal int Count => _entries.Count;

    internal int GetOrCreate(
        Direct3D9GlyphRun glyphRun,
        Direct3D9GlyphBankKey key,
        Direct3D9CreateGlyphRealization createRealization,
        out Direct3D9GlyphRealization? realization)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(glyphRun);
        ArgumentNullException.ThrowIfNull(createRealization);

        if (_entries.TryGetValue(key, out CacheEntry? existing))
        {
            existing.Stamp = ++_stamp;
            existing.Realization.MarkPersistent();
            realization = existing.Realization;
            return Direct3D9Factory.SuccessHResult;
        }

        int result = createRealization(glyphRun, key, out realization);
        if (result < 0)
        {
            realization?.Dispose();
            realization = null;
            return result;
        }

        if (realization is null)
        {
            return Direct3D9Factory.InternalErrorHResult;
        }

        if (_entries.Count == _capacity)
        {
            KeyValuePair<Direct3D9GlyphBankKey, CacheEntry> oldest = _entries.MinBy(static pair => pair.Value.Stamp);
            _entries.Remove(oldest.Key);
            oldest.Value.Realization.Dispose();
        }

        try
        {
            _entries.Add(key, new CacheEntry(realization, ++_stamp));
            return Direct3D9Factory.SuccessHResult;
        }
        catch (OutOfMemoryException)
        {
            realization.Dispose();
            realization = null;
            return Direct3D9Factory.OutOfMemoryHResult;
        }
    }

    internal void InvalidateDevice(nint deviceIdentity)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        foreach (Direct3D9GlyphBankKey key in _entries.Keys.Where(key => key.DeviceIdentity == deviceIdentity).ToArray())
        {
            CacheEntry entry = _entries[key];
            _entries.Remove(key);
            entry.Realization.Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (CacheEntry entry in _entries.Values)
        {
            entry.Realization.Dispose();
        }

        _entries.Clear();
    }

    private sealed class CacheEntry(Direct3D9GlyphRealization realization, long stamp)
    {
        internal Direct3D9GlyphRealization Realization { get; } = realization;

        internal long Stamp { get; set; } = stamp;
    }
}

internal delegate int Direct3D9PaintGlyphRealization(
    Direct3D9GlyphRun glyphRun,
    Direct3D9GlyphRealization realization,
    bool useClearType);

internal sealed class Direct3D9GlyphRunPainter : IDisposable
{
    private readonly Direct3D9GlyphRun _glyphRun;
    private readonly Direct3D9GlyphRealization _realization;
    private readonly Direct3D9PaintGlyphRealization _paint;
    private readonly Action _release;
    private bool _disposed;

    internal Direct3D9GlyphRunPainter(
        Direct3D9GlyphRun glyphRun,
        Direct3D9GlyphRealization realization,
        bool useClearType,
        Direct3D9PaintGlyphRealization paint,
        Action release)
    {
        ArgumentNullException.ThrowIfNull(glyphRun);
        ArgumentNullException.ThrowIfNull(realization);
        ArgumentNullException.ThrowIfNull(paint);
        ArgumentNullException.ThrowIfNull(release);
        _glyphRun = glyphRun;
        _realization = realization;
        UseClearType = useClearType;
        _paint = paint;
        _release = release;
    }

    internal bool UseClearType { get; }

    internal int Paint()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _realization.IsEmpty
            ? Direct3D9Factory.SuccessHResult
            : _paint(_glyphRun, _realization, UseClearType);
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

internal sealed record Direct3D9GlyphRunDrawOperations(
    Direct3D9EnsureGlyphBrushRealization EnsureHardwareBrushRealization,
    Func<int> EnsureState,
    Direct3D9GlyphBank GlyphBank,
    Direct3D9CreateGlyphRealization CreateRealization,
    Direct3D9PaintGlyphRealization PaintRealization,
    Action ReleasePainter,
    Direct3D9SoftwareGlyphRenderer SoftwareRenderer,
    nint DeviceIdentity,
    uint DisplayIndex,
    Direct3D9GlyphBlendMode RecommendedBlendMode,
    float ScaleX = 1,
    float ScaleY = 1);
