namespace WpfGfxShape.Core;

internal enum Direct3D9DestroyResourcesStyle
{
    WithDelay,
    WithoutDelay
}

internal sealed class Direct3D9ResourceManager
{
    private readonly List<Direct3D9Resource> _resources = [];
    private readonly List<Direct3D9Resource> _evictableResourcesInUse = [];
    private readonly List<Direct3D9Resource> _evictableResourcesNotInUse = [];
    private readonly List<Direct3D9Resource> _evictableResourcesFromPreviousFrames = [];
    private readonly List<Direct3D9Resource> _releasedResources = [];
    private readonly List<Direct3D9Resource> _delayedReleasedResources = [];
    private readonly Queue<Direct3D9Resource> _pendingResourceDestruction = [];
    private uint _currentUseContextDepth;
    private bool _isDestroyingResource;
    private bool _destroyReleasedResourcesAfterPendingDestruction;
    private bool _isClosed;

    internal Direct3D9ResourceManager(Direct3D9Device? device = null)
    {
        Device = device;
    }

    internal Direct3D9Device? Device { get; }

    internal bool IsClosed => _isClosed;

    internal int ResourceCount => _resources.Count;

    internal int ReleasedResourceCount => _releasedResources.Count;

    internal int DelayedReleasedResourceCount => _delayedReleasedResources.Count;

    internal int PendingResourceDestructionCount => _pendingResourceDestruction.Count;

    internal uint CurrentUseContextDepth => _currentUseContextDepth;

    internal uint TotalVideoMemoryConsumption { get; private set; }

    internal uint PeakVideoMemoryConsumption { get; private set; }

    internal bool AreActiveResources()
    {
        return _resources.Any(resource => !resource.IsEvictable && IsActiveResource(resource)) ||
               _evictableResourcesFromPreviousFrames.Any(IsActiveResource) ||
               _evictableResourcesNotInUse.Any(IsActiveResource) ||
               _evictableResourcesInUse.Any(IsActiveResource);
    }

    internal bool IsResourceActive(Direct3D9Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (!IsActiveResource(resource))
        {
            return false;
        }

        return (!resource.IsEvictable && _resources.Contains(resource)) ||
               _evictableResourcesFromPreviousFrames.Contains(resource) ||
               _evictableResourcesNotInUse.Contains(resource) ||
               _evictableResourcesInUse.Contains(resource);
    }

    internal bool IsPreviousFrameListSorted()
    {
        ulong lastUsedFrame = 0;
        foreach (Direct3D9Resource resource in _evictableResourcesFromPreviousFrames)
        {
            if (!IsActiveResource(resource))
            {
                continue;
            }

            if (resource.LastUsedFrame == 0 ||
                resource.LastUsedFrame < lastUsedFrame ||
                resource.ActiveUseContextDepth != 0)
            {
                return false;
            }

            lastUsedFrame = resource.LastUsedFrame;
        }

        return true;
    }

    internal uint CompletedFrameCount { get; private set; }

    internal uint ReleasedResourcesFromLastFrameDestroyCount { get; private set; }

    internal uint DelayedResourceDestroyCount { get; private set; }

    internal uint ImmediateResourceDestroyCount { get; private set; }

    internal bool IsInUseContext => _currentUseContextDepth > 0;

    internal Action<nint, uint>? SurfaceReleaseNotification { get; set; }

    internal void Close()
    {
        if (_isClosed)
        {
            return;
        }

        if (_resources.Count != 0 ||
            _releasedResources.Count != 0 ||
            _delayedReleasedResources.Count != 0 ||
            _pendingResourceDestruction.Count != 0 ||
            _isDestroyingResource ||
            _resources.Any(resource => !resource.IsEvictable) ||
            _evictableResourcesFromPreviousFrames.Count != 0 ||
            _evictableResourcesNotInUse.Count != 0 ||
            _evictableResourcesInUse.Count != 0 ||
            _currentUseContextDepth != 0 ||
            TotalVideoMemoryConsumption != 0)
        {
            throw new InvalidOperationException("The resource manager cannot close while resources or use contexts remain active.");
        }

        _isClosed = true;
    }

    internal void RegisterResource(Direct3D9Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ObjectDisposedException.ThrowIf(_isClosed, this);
        ObjectDisposedException.ThrowIf(!resource.IsValid, resource);
        if (!resource.IsManaged ||
            !ReferenceEquals(resource.Manager, this) ||
            _resources.Contains(resource))
        {
            throw new InvalidOperationException("Only valid, unregistered resources owned by this resource manager can be registered.");
        }

        if (resource.IsEvictable && !IsInUseContext)
        {
            throw new InvalidOperationException("Evictable resources can only be registered inside a use context.");
        }

        uint updatedVideoMemoryConsumption = checked(TotalVideoMemoryConsumption + resource.ResourceSize);
        _resources.Add(resource);
        TotalVideoMemoryConsumption = updatedVideoMemoryConsumption;
        if (PeakVideoMemoryConsumption < TotalVideoMemoryConsumption)
        {
            PeakVideoMemoryConsumption = TotalVideoMemoryConsumption;
        }

        if (resource.IsEvictable)
        {
            _evictableResourcesInUse.Add(resource);
            Use(resource);
        }
    }

    internal void DestroyResource(Direct3D9Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (!_resources.Contains(resource))
        {
            return;
        }

        if (_isDestroyingResource)
        {
            if (!_pendingResourceDestruction.Contains(resource))
            {
                _pendingResourceDestruction.Enqueue(resource);
            }

            return;
        }

        _isDestroyingResource = true;
        try
        {
            DestroyResourceCore(resource);
            DestroyPendingResources();
        }
        finally
        {
            _isDestroyingResource = false;
        }
    }

    private void DestroyResourceCore(Direct3D9Resource resource)
    {
        if (resource.ResourceSize > TotalVideoMemoryConsumption)
        {
            throw new InvalidOperationException("Resource video memory accounting cannot underflow.");
        }

        _evictableResourcesInUse.Remove(resource);
        _evictableResourcesNotInUse.Remove(resource);
        _evictableResourcesFromPreviousFrames.Remove(resource);
        _releasedResources.Remove(resource);
        _delayedReleasedResources.Remove(resource);
        try
        {
            resource.ReleaseFromManager();
        }
        finally
        {
            resource.DetachFromManager(this);
            _resources.Remove(resource);
            TotalVideoMemoryConsumption -= resource.ResourceSize;
        }
    }

    internal uint EnterUseContext()
    {
        return ++_currentUseContextDepth;
    }

    internal void ExitUseContext(uint depth)
    {
        if (_currentUseContextDepth == 0 || depth != _currentUseContextDepth)
        {
            throw new InvalidOperationException("Use contexts must be exited in reverse entry order.");
        }

        int firstResourceAtDepth = _evictableResourcesInUse.Count;
        while (firstResourceAtDepth > 0 &&
               _evictableResourcesInUse[firstResourceAtDepth - 1].ActiveUseContextDepth == _currentUseContextDepth)
        {
            firstResourceAtDepth--;
        }

        for (int index = firstResourceAtDepth; index < _evictableResourcesInUse.Count; index++)
        {
            Direct3D9Resource resource = _evictableResourcesInUse[index];
            resource.ActiveUseContextDepth = 0;
            _evictableResourcesNotInUse.Add(resource);
        }

        if (firstResourceAtDepth < _evictableResourcesInUse.Count)
        {
            _evictableResourcesInUse.RemoveRange(firstResourceAtDepth, _evictableResourcesInUse.Count - firstResourceAtDepth);
        }

        _currentUseContextDepth--;
    }

    internal void Use(Direct3D9Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (!resource.IsEvictable)
        {
            return;
        }

        ObjectDisposedException.ThrowIf(!resource.IsValid, resource);
        if (!resource.IsManaged || !_resources.Contains(resource))
        {
            throw new InvalidOperationException("Only resources managed by this resource manager can be used.");
        }

        if (!IsInUseContext)
        {
            throw new InvalidOperationException("Evictable resources can only be used inside a use context.");
        }

        resource.LastUsedFrame = (ulong)CompletedFrameCount + 1;
        if (resource.ActiveUseContextDepth != 0)
        {
            return;
        }

        resource.ActiveUseContextDepth = _currentUseContextDepth;
        _evictableResourcesInUse.Remove(resource);
        _evictableResourcesNotInUse.Remove(resource);
        _evictableResourcesFromPreviousFrames.Remove(resource);
        _evictableResourcesInUse.Add(resource);
    }

    internal void EndFrame()
    {
        if (IsInUseContext || _evictableResourcesInUse.Count != 0)
        {
            throw new InvalidOperationException("A frame cannot end while resources are in use.");
        }

        _evictableResourcesFromPreviousFrames.AddRange(_evictableResourcesNotInUse);
        _evictableResourcesNotInUse.Clear();
        CompletedFrameCount++;
    }

    internal void UnusedNotification(Direct3D9Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (!resource.IsValid || !resource.IsManaged || !_resources.Contains(resource))
        {
            return;
        }

        if (!_releasedResources.Contains(resource) && !_delayedReleasedResources.Contains(resource))
        {
            _releasedResources.Add(resource);
        }
    }

    internal void UnusableNotification(Direct3D9Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (resource.IsValid || !resource.IsManaged || !_resources.Contains(resource))
        {
            return;
        }

        DestroyResource(resource);
        if (resource.IsManaged)
        {
            _destroyReleasedResourcesAfterPendingDestruction = true;
            return;
        }

        _ = DestroyReleasedResourcesFromLastFrame();
    }

    internal bool FreeSomeVideoMemory(int hResult, bool isSoftwareDevice)
    {
        if (hResult != Direct3D9Factory.OutOfVideoMemoryHResult &&
            (hResult != Direct3D9Factory.OutOfMemoryHResult || !isSoftwareDevice))
        {
            return false;
        }

        if (DestroyReleasedResourcesFromLastFrame() > 0)
        {
            return true;
        }

        if (DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay) > 0)
        {
            return true;
        }

        if (DestroyReleasedResourcesFromLastFrame() > 0)
        {
            return true;
        }

        Direct3D9Resource? resourceToDestroy = FindLruResourceInPreviousFrames() ??
            FindMruResourceInCurrentFrame();
        if (resourceToDestroy is null)
        {
            return false;
        }

        InvalidateAndDestroyResource(resourceToDestroy);
        return true;
    }

    private bool IsActiveResource(Direct3D9Resource resource)
    {
        return resource.IsValid &&
               resource.IsManaged &&
               !_releasedResources.Contains(resource) &&
               !_delayedReleasedResources.Contains(resource);
    }

    private Direct3D9Resource? FindLruResourceInPreviousFrames()
    {
        return GetUnusedResource(_evictableResourcesFromPreviousFrames, fromEnd: false);
    }

    private Direct3D9Resource? FindMruResourceInCurrentFrame()
    {
        return GetUnusedResource(_evictableResourcesNotInUse, fromEnd: true);
    }

    private Direct3D9Resource? GetUnusedResource(List<Direct3D9Resource> resources, bool fromEnd)
    {
        if (resources.Count == 0)
        {
            return null;
        }

        Direct3D9Resource resource = resources[fromEnd ? resources.Count - 1 : 0];
        return resource.IsValid && resource.IsEvictable && resource.ActiveUseContextDepth == 0
            ? resource
            : null;
    }

    private void InvalidateAndDestroyResource(Direct3D9Resource resource)
    {
        resource.InvalidateFromManager();
        DestroyResource(resource);
    }

    internal uint DestroyReleasedResourcesFromLastFrame()
    {
        ReleasedResourcesFromLastFrameDestroyCount++;
        Direct3D9Resource[] releasedResources = _delayedReleasedResources.ToArray();
        _delayedReleasedResources.Clear();
        return DestroyReleasedResources(releasedResources, _delayedReleasedResources);
    }

    internal uint DestroyResources(Direct3D9DestroyResourcesStyle style)
    {
        if (style == Direct3D9DestroyResourcesStyle.WithDelay)
        {
            DelayedResourceDestroyCount++;
        }
        else
        {
            ImmediateResourceDestroyCount++;
        }

        Direct3D9Resource[] releasedResources = _releasedResources.ToArray();
        _releasedResources.Clear();
        return DestroyReleasedResources(releasedResources, _releasedResources, style);
    }

    private uint DestroyReleasedResources(
        Direct3D9Resource[] releasedResources,
        List<Direct3D9Resource> unconsumedResources,
        Direct3D9DestroyResourcesStyle? style = null)
    {
        bool ownsDestructionScope = !_isDestroyingResource;
        if (ownsDestructionScope)
        {
            _isDestroyingResource = true;
        }

        int index = releasedResources.Length - 1;
        try
        {
            uint count = 0;
            for (; index >= 0; index--)
            {
                Direct3D9Resource resource = releasedResources[index];
                if (!_resources.Contains(resource) || _pendingResourceDestruction.Contains(resource))
                {
                    continue;
                }

                if (style == Direct3D9DestroyResourcesStyle.WithDelay && resource.RequiresDelayedRelease)
                {
                    if (!_delayedReleasedResources.Contains(resource))
                    {
                        _delayedReleasedResources.Add(resource);
                    }

                    continue;
                }

                if (ownsDestructionScope)
                {
                    DestroyResourceCore(resource);
                }
                else
                {
                    _pendingResourceDestruction.Enqueue(resource);
                }

                count++;
            }

            if (ownsDestructionScope)
            {
                DestroyPendingResources();
            }

            return count;
        }
        catch
        {
            for (; index >= 0; index--)
            {
                Direct3D9Resource resource = releasedResources[index];
                if (_resources.Contains(resource) &&
                    !_pendingResourceDestruction.Contains(resource) &&
                    !unconsumedResources.Contains(resource))
                {
                    unconsumedResources.Add(resource);
                }
            }

            throw;
        }
        finally
        {
            if (ownsDestructionScope)
            {
                _isDestroyingResource = false;
            }
        }
    }

    internal void DestroyAllResources()
    {
        _ = DestroyReleasedResourcesFromLastFrame();
        DestroySomeActiveResources();
        _ = DestroyResources(Direct3D9DestroyResourcesStyle.WithoutDelay);
    }

    private void DestroySomeActiveResources()
    {
        _isDestroyingResource = true;
        try
        {
            DestroyListOfResources(_resources.Where(resource =>
                !resource.IsEvictable &&
                !_releasedResources.Contains(resource) &&
                !_delayedReleasedResources.Contains(resource)));
            DestroyListOfResources(_evictableResourcesFromPreviousFrames);
            DestroyListOfResources(_evictableResourcesNotInUse);
            DestroyListOfResources(_evictableResourcesInUse);
            DestroyPendingResources();
        }
        finally
        {
            _isDestroyingResource = false;
        }
    }

    private void DestroyListOfResources(IEnumerable<Direct3D9Resource> resources)
    {
        foreach (Direct3D9Resource resource in resources.ToArray())
        {
            if (_resources.Contains(resource) &&
                !_releasedResources.Contains(resource) &&
                !_delayedReleasedResources.Contains(resource) &&
                !_pendingResourceDestruction.Contains(resource))
            {
                resource.InvalidateFromManager();
                DestroyResourceCore(resource);
            }
        }
    }

    private void DestroyPendingResources()
    {
        while (true)
        {
            while (_pendingResourceDestruction.TryDequeue(out Direct3D9Resource? pendingResource))
            {
                if (_resources.Contains(pendingResource))
                {
                    DestroyResourceCore(pendingResource);
                }
            }

            if (!_destroyReleasedResourcesAfterPendingDestruction)
            {
                return;
            }

            _destroyReleasedResourcesAfterPendingDestruction = false;
            _ = DestroyReleasedResourcesFromLastFrame();
        }
    }
}
