namespace WpfGfxShape.Core;

internal sealed class Direct3D9BitmapCacheLifetime : Direct3D9Resource
{
    private readonly Action _clearCachedRealizations;
    private readonly Action<nint> _addReference;
    private readonly Action<nint> _release;
    private nint _bitmapSourceNoReference;
    private nint _deviceBitmapColorSource;
    private nint _lastUsedColorSource;
    private Direct3D9BitmapColorSourceContextParameters _lastUsedContextParameters;

    internal Direct3D9BitmapCacheLifetime(
        Direct3D9ResourceManager manager,
        nint bitmapNoReference,
        nint deviceNoReference,
        Action clearCachedRealizations,
        Action<nint> addReference,
        Action<nint> release)
        : base(manager)
    {
        ArgumentNullException.ThrowIfNull(clearCachedRealizations);
        ArgumentNullException.ThrowIfNull(addReference);
        ArgumentNullException.ThrowIfNull(release);

        BitmapNoReference = bitmapNoReference;
        DeviceNoReference = deviceNoReference;
        _clearCachedRealizations = clearCachedRealizations;
        _addReference = addReference;
        _release = release;
    }

    internal nint BitmapNoReference
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsReleased, this);
            return field;
        }
    }

    internal nint DeviceNoReference
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsReleased, this);
            return field;
        }
    }

    internal nint BitmapSourceNoReference
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsReleased, this);
            return _bitmapSourceNoReference;
        }
    }

    internal nint DeviceBitmapColorSource
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsReleased, this);
            return _deviceBitmapColorSource;
        }
    }

    internal nint LastUsedColorSource
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsReleased, this);
            return _lastUsedColorSource;
        }
    }

    internal Direct3D9BitmapColorSourceContextParameters LastUsedContextParameters
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsReleased, this);
            return _lastUsedContextParameters;
        }
    }

    internal void AssociateBitmapSource(nint bitmapSourceNoReference)
    {
        ObjectDisposedException.ThrowIf(IsReleased, this);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapSourceNoReference);

        if (_bitmapSourceNoReference == bitmapSourceNoReference)
        {
            return;
        }

        if (_bitmapSourceNoReference != 0)
        {
            _clearCachedRealizations();
        }

        _bitmapSourceNoReference = bitmapSourceNoReference;
    }

    internal int CacheDeviceBitmapColorSource(nint colorSource)
    {
        ObjectDisposedException.ThrowIf(IsReleased, this);
        ArgumentOutOfRangeException.ThrowIfZero(colorSource);

        if (_deviceBitmapColorSource != 0)
        {
            return Direct3D9Factory.UnexpectedHResult;
        }

        _deviceBitmapColorSource = colorSource;
        _addReference(colorSource);
        return Direct3D9Factory.SuccessHResult;
    }

    internal nint AcquireDeviceBitmapColorSource()
    {
        ObjectDisposedException.ThrowIf(IsReleased, this);

        nint colorSource = _deviceBitmapColorSource;
        if (colorSource != 0)
        {
            _addReference(colorSource);
        }

        return colorSource;
    }

    internal nint AcquireLastUsedColorSource()
    {
        ObjectDisposedException.ThrowIf(IsReleased, this);

        nint colorSource = _lastUsedColorSource;
        if (colorSource != 0)
        {
            _addReference(colorSource);
        }

        return colorSource;
    }

    internal void SetLastUsedColorSource(nint colorSource) =>
        SetLastUsedColorSource(colorSource, _lastUsedContextParameters);

    internal void SetLastUsedColorSource(
        nint colorSource,
        Direct3D9BitmapColorSourceContextParameters contextParameters)
    {
        ObjectDisposedException.ThrowIf(IsReleased, this);

        if (_lastUsedColorSource == colorSource)
        {
            _lastUsedContextParameters = _lastUsedContextParameters with
            {
                WrapMode = contextParameters.WrapMode
            };
            return;
        }

        nint previousColorSource = _lastUsedColorSource;
        _lastUsedColorSource = colorSource;
        if (previousColorSource != 0)
        {
            _release(previousColorSource);
        }

        if (colorSource != 0)
        {
            _lastUsedContextParameters = contextParameters;
            _addReference(colorSource);
        }
    }

    protected override void ReleaseD3DResources()
    {
        _clearCachedRealizations();
        Release(ref _lastUsedColorSource);
        Release(ref _deviceBitmapColorSource);
    }

    private void Release(ref nint colorSource)
    {
        nint value = colorSource;
        colorSource = 0;
        if (value != 0)
        {
            _release(value);
        }
    }
}
