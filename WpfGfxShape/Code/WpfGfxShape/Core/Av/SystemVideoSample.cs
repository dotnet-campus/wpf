using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

// The system renderer accepts a null device for software sampling. Its resource
// registration ABI is deliberately not used: it requires a native CMilSlaveVideo.
internal sealed unsafe class SystemVideoSample : IDisposable
{
    private nint _renderer;
    private nint _source;
    private readonly bool _compositionBegun;

    private SystemVideoSample(nint renderer, nint source, bool compositionBegun)
    {
        _renderer = renderer;
        _source = source;
        _compositionBegun = compositionBegun;
    }

    internal nint Source => Volatile.Read(ref _source);

    // Media is borrowed. The returned lease owns both the sample and renderer;
    // EndRender must run while those references are still alive.
    internal static int Acquire(nint media, out SystemVideoSample? sample)
        => Acquire(media, false, true, out sample, out _);

    // This software composition scope is not notification registration. The native
    // composition list only compares caller values (UniqueList.inl); it never calls
    // CMilSlaveVideo. A null caller is paired with EndComposition on the same renderer.
    internal static int Acquire(nint media, bool displaySetChanged, bool syncChannel,
        out SystemVideoSample? sample, out bool newFrame)
    {
        sample = null;
        newFrame = false;
        if (media == 0) return Direct3D9Factory.InvalidArgumentHResult;

        nint provider = 0, renderer = 0, source = 0;
        bool renderBegun = false, compositionBegun = false;
        try
        {
            Guid providerId = new("E6F1CC74-A0EB-4EBE-8241-089D1CB079D7");
            int result = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)media)[0])
                (media, &providerId, &provider);
            if (result < 0) return result;
            if (provider == 0) return Direct3D9Factory.NoInterfaceHResult;

            result = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)provider)[3])
                (provider, &renderer);
            if (result < 0) return result;
            if (renderer == 0) return 0;

            long lastSampleTime = -1;
            int ready = 0;
            result = ((delegate* unmanaged[Stdcall]<nint, nint, int, int, long*, int*, int>)(*(void***)renderer)[3])
                (renderer, 0, displaySetChanged ? 1 : 0, syncChannel ? 1 : 0, &lastSampleTime, &ready);
            compositionBegun = true;
            if (result < 0) return result;
            newFrame = ready != 0;

            result = ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)(*(void***)renderer)[4])
                (renderer, 0, &source);
            if (result < 0) return result;
            renderBegun = true;
            if (source == 0) return 0;

            try
            {
                sample = new SystemVideoSample(renderer, source, compositionBegun);
            }
            catch (OutOfMemoryException)
            {
                return unchecked((int)0x8007000E);
            }
            renderer = source = 0;
            renderBegun = compositionBegun = false;
            return result;
        }
        finally
        {
            if (renderBegun)
            {
                _ = EndRender(renderer);
            }
            if (compositionBegun)
            {
                _ = EndComposition(renderer);
            }
            Direct3D9Factory.Release(source);
            Direct3D9Factory.Release(renderer);
            Direct3D9Factory.Release(provider);
        }
    }

    private static int EndRender(nint renderer)
        => ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)renderer)[5])(renderer);

    private static int EndComposition(nint renderer)
        => ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)renderer)[6])(renderer, 0);

    internal int Close()
    {
        nint renderer = Interlocked.Exchange(ref _renderer, 0);
        if (renderer == 0) return 0;
        nint source = Interlocked.Exchange(ref _source, 0);
        try
        {
            int result = EndRender(renderer);
            int compositionResult = _compositionBegun ? EndComposition(renderer) : 0;
            return result < 0 ? result : compositionResult;
        }
        finally
        {
            Direct3D9Factory.Release(source);
            Direct3D9Factory.Release(renderer);
        }
    }

    /// <summary>Ends software sampling and releases the owned COM references once.</summary>
    public void Dispose() => _ = Close();
}
