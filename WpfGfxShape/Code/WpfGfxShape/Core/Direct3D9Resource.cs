namespace WpfGfxShape.Core;

internal abstract class Direct3D9Resource : IDisposable
{
    private Direct3D9ResourceManager? _manager;
    private bool _isValid = true;
    private bool _areD3DResourcesReleased;

    protected Direct3D9Resource(
        Direct3D9ResourceManager manager,
        bool isEvictable = false,
        uint resourceSize = 0)
    {
        ArgumentNullException.ThrowIfNull(manager);
        _manager = manager;
        IsEvictable = isEvictable;
        ResourceSize = resourceSize;
        manager.RegisterResource(this);
    }

    internal bool IsManaged => _manager is not null;

    internal Direct3D9ResourceManager Manager
    {
        get
        {
            ObjectDisposedException.ThrowIf(_manager is null, this);
            return _manager;
        }
    }

    internal Direct3D9Device Device => Manager.Device
        ?? throw new InvalidOperationException("The resource manager is not associated with a Direct3D device.");

    internal uint ResourceSize { get; }

    internal bool IsValid => _isValid;

    internal bool IsReleased => !_isValid;

    internal bool IsEvictable { get; private set; }

    internal uint ActiveUseContextDepth { get; set; }

    internal ulong LastUsedFrame { get; set; }

    internal virtual bool RequiresDelayedRelease => false;

    internal void SetAsEvictable()
    {
        ObjectDisposedException.ThrowIf(!_isValid || _manager is null, this);
        IsEvictable = true;
        if (_manager.IsInUseContext)
        {
            _manager.Use(this);
            return;
        }

        uint depth = _manager.EnterUseContext();
        try
        {
            _manager.Use(this);
        }
        finally
        {
            _manager.ExitUseContext(depth);
        }
    }

    internal void InvalidateFromManager()
    {
        _isValid = false;
        ActiveUseContextDepth = 0;
    }

    internal void ReleaseFromManager()
    {
        InvalidateFromManager();
        if (_areD3DResourcesReleased)
        {
            return;
        }

        _areD3DResourcesReleased = true;
        ReleaseD3DResources();
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
            if (_isValid)
            {
                _isValid = false;
                ActiveUseContextDepth = 0;
                manager.UnusableNotification(this);
            }

            return;
        }

        ReleaseFromManager();
    }
}
