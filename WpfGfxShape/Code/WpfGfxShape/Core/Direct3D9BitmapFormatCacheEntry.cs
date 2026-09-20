namespace WpfGfxShape.Core;

internal sealed class Direct3D9BitmapFormatCacheEntry : IDisposable
{
    private readonly Action<nint> _addReference;
    private readonly Action<nint> _release;
    private readonly Direct3D9BitmapCacheEntryList _entries;
    private Direct3D9BitmapFormatCacheEntry? _next;
    private MilPixelFormat _format;
    private bool _isDisposed;

    internal Direct3D9BitmapFormatCacheEntry(Action<nint> addReference, Action<nint> release)
    {
        ArgumentNullException.ThrowIfNull(addReference);
        ArgumentNullException.ThrowIfNull(release);

        _addReference = addReference;
        _release = release;
        _entries = new Direct3D9BitmapCacheEntryList(addReference, release);
    }

    internal Direct3D9BitmapCacheEntryList GetOrCreateEntryList(MilPixelFormat format)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (format == MilPixelFormat.Undefined)
        {
            throw new ArgumentException("A defined texture format is required.", nameof(format));
        }

        if (_format == format)
        {
            return _entries;
        }

        if (_format == MilPixelFormat.Undefined)
        {
            _format = format;
            return _entries;
        }

        _next ??= new Direct3D9BitmapFormatCacheEntry(_addReference, _release);
        return _next.GetOrCreateEntryList(format);
    }

    internal bool TryAcquire(
        ref Direct3D9BitmapRealizationProperties properties,
        Func<nint, bool> isValid,
        Direct3D9BitmapReusableRealizationCandidates? reusableCandidates,
        out nint colorSource) =>
        GetOrCreateEntryList(properties.TextureFormat).TryAcquire(
            ref properties,
            isValid,
            reusableCandidates,
            out colorSource);

    internal int Store(Direct3D9BitmapRealizationProperties properties, nint colorSource) =>
        GetOrCreateEntryList(properties.TextureFormat).Store(properties, colorSource);

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _next?.Dispose();
        _next = null;
        _entries.Dispose();
    }
}
