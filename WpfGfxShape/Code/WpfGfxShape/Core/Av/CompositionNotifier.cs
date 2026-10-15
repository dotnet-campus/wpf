namespace WpfGfxShape.Core.Av;

internal interface ICompositionVideoNotification
{
    bool NewFrame();
    void InvalidateLastCompositionSampleTime();
}

internal sealed class CompositionNotifier
{
    private readonly object _lock = new();
    private readonly LinkedList<ICompositionVideoNotification> _registeredResources = new();
    private readonly Action _raiseMediaNewFrame;
    private bool _outstandingUIFrame;

    internal CompositionNotifier(Action raiseMediaNewFrame)
    {
        ArgumentNullException.ThrowIfNull(raiseMediaNewFrame);
        _raiseMediaNewFrame = raiseMediaNewFrame;
    }

    internal int RegisterResource(ICompositionVideoNotification resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        lock (_lock)
        {
            for (var node = _registeredResources.First; node is not null; node = node.Next)
            {
                if (ReferenceEquals(node.Value, resource))
                {
                    return 0;
                }
            }

            try
            {
                _registeredResources.AddFirst(resource);
            }
            catch (OutOfMemoryException)
            {
                return unchecked((int)0x8007000E);
            }
        }

        return 0;
    }

    internal void UnregisterResource(ICompositionVideoNotification resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        lock (_lock)
        {
            for (var node = _registeredResources.First; node is not null; node = node.Next)
            {
                if (ReferenceEquals(node.Value, resource))
                {
                    _registeredResources.Remove(node);
                    return;
                }
            }
        }
    }

    internal void NotifyComposition()
    {
        ICompositionVideoNotification[] resources;
        bool displayUIFrame;
        lock (_lock)
        {
            resources = _registeredResources.ToArray();
            displayUIFrame = _outstandingUIFrame;
            _outstandingUIFrame = false;
        }

        // Snapshot ownership keeps callbacks alive without holding the registration lock.
        foreach (ICompositionVideoNotification resource in resources)
        {
            displayUIFrame = !resource.NewFrame() || displayUIFrame;
        }

        if (displayUIFrame)
        {
            _raiseMediaNewFrame();
        }
    }

    internal void InvalidateLastCompositionSampleTime()
    {
        ICompositionVideoNotification[] resources;
        lock (_lock)
        {
            resources = _registeredResources.ToArray();
        }

        foreach (ICompositionVideoNotification resource in resources)
        {
            resource.InvalidateLastCompositionSampleTime();
        }
    }

    internal void NeedUIFrameUpdate()
    {
        lock (_lock)
        {
            _outstandingUIFrame = true;
        }
    }
}
