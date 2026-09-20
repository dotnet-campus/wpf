namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9BitmapReusableRealizationSourceState(
    nint Bitmap,
    bool IsValid,
    bool IsRenderTarget,
    uint PrefilterWidth,
    uint PrefilterHeight,
    Direct3D9BitmapRealizationRectangle RequiredRealizationBounds,
    uint CachedUniquenessToken,
    bool HasValidDirtyRectInformation,
    bool IsCompletelyDirty);

internal readonly record struct Direct3D9BitmapReusableRealizationTargetState(
    nint Bitmap,
    bool IsRenderTarget,
    bool CanStretchRectFromTextures,
    uint PrefilterWidth,
    uint PrefilterHeight,
    Direct3D9BitmapRealizationRectangle RequiredRealizationBounds,
    uint CachedUniquenessToken);

internal delegate bool Direct3D9TryGetReusableRealizationSourceState(
    nint source,
    out Direct3D9BitmapReusableRealizationSourceState state);

internal sealed class Direct3D9BitmapReusableRealizationSources : IDisposable
{
    private readonly Direct3D9BitmapReusableRealizationTargetState _target;
    private readonly Direct3D9TryGetReusableRealizationSourceState _tryGetState;
    private readonly Func<nint, nint> _getNext;
    private readonly Action<nint, nint> _setNext;
    private readonly Action<nint> _addReference;
    private readonly Action<nint> _release;
    private readonly Action<nint> _tryAdoptSystemMemorySurface;
    private nint _head;
    private bool _isDisposed;

    internal Direct3D9BitmapReusableRealizationSources(
        Direct3D9BitmapReusableRealizationTargetState target,
        Direct3D9TryGetReusableRealizationSourceState tryGetState,
        Func<nint, nint> getNext,
        Action<nint, nint> setNext,
        Action<nint> addReference,
        Action<nint> release,
        Action<nint> tryAdoptSystemMemorySurface)
    {
        ArgumentNullException.ThrowIfNull(tryGetState);
        ArgumentNullException.ThrowIfNull(getNext);
        ArgumentNullException.ThrowIfNull(setNext);
        ArgumentNullException.ThrowIfNull(addReference);
        ArgumentNullException.ThrowIfNull(release);
        ArgumentNullException.ThrowIfNull(tryAdoptSystemMemorySurface);

        _target = target;
        _tryGetState = tryGetState;
        _getNext = getNext;
        _setNext = setNext;
        _addReference = addReference;
        _release = release;
        _tryAdoptSystemMemorySurface = tryAdoptSystemMemorySurface;
    }

    internal nint Head
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _head;
        }
    }

    internal void SetSources(nint sources)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (sources != 0)
        {
            _addReference(sources);
        }

        ReleaseSources();

        try
        {
            while (sources != 0)
            {
                nint current = sources;
                sources = _getNext(current);
                _setNext(current, 0);

                try
                {
                    CheckAndSetSource(current);
                }
                finally
                {
                    _release(current);
                }
            }
        }
        finally
        {
            ReleaseChain(sources);
        }
    }

    internal void Consume(Direct3D9BitmapReusableRealizationCandidates candidates)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(candidates);

        ReleaseSources();

        nint source = candidates.Detach();
        try
        {
            while (source != 0)
            {
                nint current = source;
                source = _getNext(current);
                _setNext(current, 0);

                try
                {
                    CheckAndSetSource(current);
                }
                finally
                {
                    _release(current);
                }
            }
        }
        finally
        {
            ReleaseChain(source);
        }
    }

    internal void ReleaseSources()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        nint source = _head;
        _head = 0;
        ReleaseChain(source);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        ReleaseSources();
        _isDisposed = true;
    }

    private void ReleaseChain(nint source)
    {
        while (source != 0)
        {
            nint next = _getNext(source);
            _setNext(source, 0);
            _release(source);
            source = next;
        }
    }

    private void CheckAndSetSource(nint source)
    {
        bool reuseSource = _tryGetState(source, out Direct3D9BitmapReusableRealizationSourceState state)
            && CanReuse(state);

        if (reuseSource)
        {
            _setNext(source, _head);
            _head = source;
            _addReference(source);
            return;
        }

        _tryAdoptSystemMemorySurface(source);
    }

    private bool CanReuse(Direct3D9BitmapReusableRealizationSourceState source)
    {
        if (source.Bitmap != 0 && source.Bitmap != _target.Bitmap)
        {
            return false;
        }

        if (!source.IsValid
            || !_target.IsRenderTarget
            || !(_target.CanStretchRectFromTextures || source.IsRenderTarget)
            || source.PrefilterWidth != _target.PrefilterWidth
            || source.PrefilterHeight != _target.PrefilterHeight
            || !Intersects(_target.RequiredRealizationBounds, source.RequiredRealizationBounds))
        {
            return false;
        }

        if (source.Bitmap == 0)
        {
            return true;
        }

        if (_target.CachedUniquenessToken == source.CachedUniquenessToken)
        {
            return false;
        }

        return source.HasValidDirtyRectInformation && !source.IsCompletelyDirty;
    }

    private static bool Intersects(
        Direct3D9BitmapRealizationRectangle first,
        Direct3D9BitmapRealizationRectangle second) =>
        first.Left < second.Right
        && second.Left < first.Right
        && first.Top < second.Bottom
        && second.Top < first.Bottom;
}
