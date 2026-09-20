namespace WpfGfxShape.Core;

internal sealed class Direct3D9BitmapReusableRealizationCandidates : IDisposable
{
    private readonly Func<nint, nint> _getNext;
    private readonly Action<nint, nint> _setNext;
    private readonly Action<nint> _addReference;
    private readonly Action<nint> _release;
    private nint _head;
    private bool _isDisposed;

    internal Direct3D9BitmapReusableRealizationCandidates(
        Func<nint, nint> getNext,
        Action<nint, nint> setNext,
        Action<nint> addReference,
        Action<nint> release)
    {
        ArgumentNullException.ThrowIfNull(getNext);
        ArgumentNullException.ThrowIfNull(setNext);
        ArgumentNullException.ThrowIfNull(addReference);
        ArgumentNullException.ThrowIfNull(release);

        _getNext = getNext;
        _setNext = setNext;
        _addReference = addReference;
        _release = release;
    }

    internal nint Head
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _head;
        }
    }

    internal void Add(nint source)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (source == 0)
        {
            return;
        }

        ReleaseChain(_getNext(source));
        _setNext(source, _head);
        _head = source;
        _addReference(source);
    }

    internal nint Detach()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        nint head = _head;
        _head = 0;
        return head;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        ReleaseChain(_head);
        _head = 0;
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
}
