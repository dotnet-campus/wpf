namespace WpfGfxShape.Core.Av;

// Owns either an independent bitmap or a caller-provided renderer composition scope.
internal sealed unsafe class SystemVideoSample : IDisposable
{
    private nint _source;
    private nint _renderer;
    private readonly object _gate = new();

    internal SystemVideoSample(nint source, long timestamp)
    {
        _source = source;
        Timestamp = timestamp;
    }

    internal nint Source => Volatile.Read(ref _source);
    internal long Timestamp { get; }

    internal static int Acquire(nint media, out SystemVideoSample? sample)
        => Acquire(media, false, true, out sample, out _);

    internal static int Acquire(nint media, bool displaySetChanged, bool syncChannel,
        out SystemVideoSample? sample, out bool newFrame)
    {
        sample = null;
        newFrame = false;
        if (media == 0) return Direct3D9Factory.InvalidArgumentHResult;
        nint provider = 0, renderer = 0, source = 0;
        bool composition = false, render = false;
        try
        {
            Guid iid = new("E6F1CC74-A0EB-4EBE-8241-089D1CB079D7");
            int hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)media)[0])(media, &iid, &provider);
            if (hr < 0) return hr;
            if (provider == 0) return Direct3D9Factory.NoInterfaceHResult;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)provider)[3])(provider, &renderer);
            if (hr < 0) return hr;
            if (renderer == 0) return Direct3D9Factory.NoInterfaceHResult;
            long timestamp = 0;
            int changed = 0;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint, int, int, long*, int*, int>)(*(void***)renderer)[3])(
                renderer, 0, displaySetChanged ? 1 : 0, syncChannel ? 1 : 0, &timestamp, &changed);
            if (hr < 0) return hr;
            composition = true;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)(*(void***)renderer)[4])(renderer, 0, &source);
            if (hr < 0) return hr;
            render = true;
            newFrame = changed != 0;
            sample = new SystemVideoSample(source, timestamp) { _renderer = renderer };
            source = 0;
            renderer = 0;
            return hr;
        }
        finally
        {
            Direct3D9Factory.Release(source);
            if (renderer != 0)
            {
                if (render) _ = ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)renderer)[5])(renderer);
                if (composition) _ = ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)renderer)[6])(renderer, 0);
                Direct3D9Factory.Release(renderer);
            }
            Direct3D9Factory.Release(provider);
        }
    }

    internal int Close()
    {
        lock (_gate)
        {
            nint source = _source, renderer = _renderer;
            _source = 0;
            _renderer = 0;
            Direct3D9Factory.Release(source);
            if (renderer == 0) return 0;
            int render = ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)renderer)[5])(renderer);
            int composition = ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)renderer)[6])(renderer, 0);
            Direct3D9Factory.Release(renderer);
            return render < 0 ? render : composition;
        }
    }

    /// <summary>Releases this lease's source and any caller-provided renderer scope once.</summary>
    public void Dispose() => _ = Close();
}
