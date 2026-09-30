using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Core;

// These are managed dependency contracts, not COM declarations. Returned interfaces own a reference.
internal interface IVideoMediaSource
{
    int QuerySurfaceRendererProvider(out IVideoSurfaceRendererProvider? provider);
    void Release();
}

internal interface IVideoSurfaceRendererProvider
{
    int GetSurfaceRenderer(out IVideoSurfaceRenderer? renderer);
    int RegisterResource(GeneratedMediaPlayerResource video);
    int UnregisterResource(GeneratedMediaPlayerResource video);
    void Release();
}

internal interface IVideoSurfaceRenderer
{
    void AddRef();
    void Release();
    int BeginComposition(GeneratedMediaPlayerResource caller, bool displaySetChanged, bool syncChannel,
        ref long lastCompositionSampleTime, out bool newFrame);
    int EndComposition(GeneratedMediaPlayerResource caller);
}

internal interface IVideoCompositionOwner
{
    int RegisterVideo(GeneratedMediaPlayerResource video);
    void UnregisterVideo(GeneratedMediaPlayerResource video);
    void ScheduleCompositionPass();
}

internal sealed unsafe partial class GeneratedMediaPlayerResource : ICompositionVideoNotification
{
    private IVideoCompositionOwner? _composition;
    private IVideoSurfaceRendererProvider? _surfaceRendererProvider;
    private IVideoSurfaceRenderer? _currentRenderer;
    private long _lastCompositionSampleTime = -1;
    private long _sampleInvalidationVersion;

    internal GeneratedMediaPlayerResource(IVideoCompositionOwner composition) : this()
    {
        ArgumentNullException.ThrowIfNull(composition);
        _composition = composition;
    }

    // The caller transfers one media reference, just as the native command transport does.
    internal int ProcessUpdate(IVideoMediaSource? media, bool notifyUceDirect)
    {
        if (IsReleased)
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        NotifyUceDirect = notifyUceDirect;
        IVideoSurfaceRendererProvider? provider = null;
        try
        {
            if (media is null)
            {
                return unchecked((int)0x80070006);
            }

            int result = media.QuerySurfaceRendererProvider(out provider);
            if (result < 0)
            {
                return result;
            }

            if (provider is null)
            {
                return Direct3D9Factory.NoInterfaceHResult;
            }

            if (_surfaceRendererProvider is not null)
            {
                return ReferenceEquals(_surfaceRendererProvider, provider) ? 0 : Direct3D9Factory.InvalidArgumentHResult;
            }

            if (_composition is null)
            {
                return unchecked((int)0x80004001);
            }

            _surfaceRendererProvider = provider;
            provider = null;
            result = _composition.RegisterVideo(this);
            return result < 0 ? result : _surfaceRendererProvider.RegisterResource(this);
        }
        finally
        {
            media?.Release();
            provider?.Release();
        }
    }

    bool ICompositionVideoNotification.NewFrame()
    {
        bool direct = NotifyUceDirect;
        if (direct)
        {
            _composition?.ScheduleCompositionPass();
        }

        return direct;
    }

    void ICompositionVideoNotification.InvalidateLastCompositionSampleTime()
    {
        // Do not let a concurrent renderer write erase a newer invalidation.
        Interlocked.Increment(ref _sampleInvalidationVersion);
        Interlocked.Exchange(ref _lastCompositionSampleTime, -1);
    }

    internal int BeginComposition(bool displaySetChanged, out bool frameReady)
    {
        frameReady = false;
        if (IsReleased)
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        _ = EndComposition();
        IVideoSurfaceRenderer? renderer = null;
        int result = _surfaceRendererProvider?.GetSurfaceRenderer(out renderer) ?? 0;
        if (result < 0)
        {
            renderer?.Release();
            return 0;
        }

        _currentRenderer = renderer;
        if (renderer is not null)
        {
            long version = Interlocked.Read(ref _sampleInvalidationVersion);
            long sampleTime = Interlocked.Read(ref _lastCompositionSampleTime);
            result = renderer.BeginComposition(this, displaySetChanged, !NotifyUceDirect, ref sampleTime, out bool ready);
            Interlocked.Exchange(ref _lastCompositionSampleTime, sampleTime);
            if (version != Interlocked.Read(ref _sampleInvalidationVersion))
            {
                Interlocked.Exchange(ref _lastCompositionSampleTime, -1);
            }

            frameReady = result >= 0 && ready;
        }

        // Native video errors must not zombify the composition partition.
        return 0;
    }

    internal int EndComposition()
    {
        IVideoSurfaceRenderer? renderer = _currentRenderer;
        _currentRenderer = null;
        if (renderer is not null)
        {
            try
            {
                _ = renderer.EndComposition(this);
            }
            finally
            {
                renderer.Release();
            }
        }

        return 0;
    }

    internal int GetSurfaceRenderer(out IVideoSurfaceRenderer? renderer)
    {
        renderer = null;
        if (IsReleased)
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        renderer = _currentRenderer;
        renderer?.AddRef();
        return 0;
    }

    internal void NotifyFrameReady() => NotifyChanged();

    protected override void OnFinalRelease()
    {
        _ = EndComposition();
        if (_surfaceRendererProvider is not null)
        {
            _ = _surfaceRendererProvider.UnregisterResource(this);
            _composition?.UnregisterVideo(this);
            _surfaceRendererProvider.Release();
            _surfaceRendererProvider = null;
        }

        _composition = null;
    }
}
