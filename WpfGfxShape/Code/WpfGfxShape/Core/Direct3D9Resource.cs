namespace WpfGfxShape.Core;

internal abstract class Direct3D9Resource : IDisposable
{
    private Direct3D9ResourceManager? _manager;
    private bool _isReleased;

    protected Direct3D9Resource(Direct3D9ResourceManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);
        _manager = manager;
        manager.RegisterResource(this);
    }

    internal bool IsManaged => _manager is not null;

    internal bool IsReleased => _isReleased;

    internal void ReleaseFromManager()
    {
        if (_isReleased)
        {
            return;
        }

        ReleaseD3DResources();
        _isReleased = true;
    }

    internal void DetachFromManager(Direct3D9ResourceManager manager)
    {
        if (ReferenceEquals(_manager, manager))
        {
            _manager = null;
        }
    }

    protected abstract void ReleaseD3DResources();

    public void Dispose()
    {
        Direct3D9ResourceManager? manager = _manager;
        if (manager is not null)
        {
            manager.DestroyResource(this);
            return;
        }

        ReleaseFromManager();
    }
}
