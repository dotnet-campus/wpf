namespace WpfGfxShape.Core;

internal sealed class Direct3D9ResourceManager
{
    private readonly List<Direct3D9Resource> _resources = [];

    internal int ResourceCount => _resources.Count;

    internal void RegisterResource(Direct3D9Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        _resources.Add(resource);
    }

    internal void DestroyResource(Direct3D9Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (!_resources.Contains(resource))
        {
            return;
        }

        resource.ReleaseFromManager();
        resource.DetachFromManager(this);
        _resources.Remove(resource);
    }

    internal void DestroyAllResources()
    {
        while (_resources.Count > 0)
        {
            DestroyResource(_resources[0]);
        }
    }
}
