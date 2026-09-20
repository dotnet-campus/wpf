namespace WpfGfxShape.Core;

internal delegate int Direct3D9CreateCachedBitBltColorSource(out nint colorSource);

internal sealed class Direct3D9BitBltColorSourceCache : IDisposable
{
    private readonly Action<nint> _addReference;
    private readonly Action<nint> _release;
    private nint _colorSource;
    private bool _isDisposed;

    internal Direct3D9BitBltColorSourceCache(Action<nint> addReference, Action<nint> release)
    {
        ArgumentNullException.ThrowIfNull(addReference);
        ArgumentNullException.ThrowIfNull(release);

        _addReference = addReference;
        _release = release;
    }

    internal nint ColorSource
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _colorSource;
        }
    }

    internal int TryCreate(
        Direct3D9CreateCachedBitBltColorSource createColorSource,
        out nint colorSource)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(createColorSource);

        colorSource = 0;
        if (_colorSource != 0)
        {
            return Direct3D9Factory.UnexpectedHResult;
        }

        int result = createColorSource(out nint createdColorSource);
        if (result < 0)
        {
            if (createdColorSource != 0)
            {
                _release(createdColorSource);
            }

            return result;
        }

        if (createdColorSource == 0)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        _colorSource = createdColorSource;
        _addReference(createdColorSource);
        colorSource = createdColorSource;
        return result;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        nint colorSource = _colorSource;
        _colorSource = 0;
        if (colorSource != 0)
        {
            _release(colorSource);
        }
    }
}
