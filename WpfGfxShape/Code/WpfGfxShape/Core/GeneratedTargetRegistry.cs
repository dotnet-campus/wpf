namespace WpfGfxShape.Core;

internal sealed class GeneratedTargetRegistry
{
    private readonly List<GeneratedTargetResource> _targets = [];

    internal void Add(GeneratedTargetResource target)
    {
        // Reserve before acquiring ownership so allocation failure cannot leak a reference.
        _targets.EnsureCapacity(_targets.Count + 1);
        target.AddRef();
        _targets.Add(target);
    }

    internal void Remove(GeneratedTargetResource target)
    {
        int index = _targets.IndexOf(target);
        if (index < 0) return;
        _targets.RemoveAt(index);
        target.Release();
    }

    internal int Render()
    {
        GeneratedTargetResource[] targets = [.. _targets];
        foreach (GeneratedTargetResource target in targets) target.AddRef();
        try
        {
            foreach (GeneratedTargetResource target in targets)
            {
                int result = target.Render();
                if (result < 0) return result;
            }
            return 0;
        }
        finally { foreach (GeneratedTargetResource target in targets) target.Release(); }
    }

    internal void ReleaseAll()
    {
        while (_targets.Count != 0)
        {
            int index = _targets.Count - 1;
            GeneratedTargetResource target = _targets[index];
            _targets.RemoveAt(index);
            target.Release();
        }
    }
}
