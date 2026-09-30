namespace WpfGfxShape.Core.Av;

// Composition-thread-owned, like CComposition::m_rgpVideo. The scheduler callback must be thread-safe.
internal sealed class VideoComposition : IVideoCompositionOwner
{
    private readonly List<GeneratedMediaPlayerResource> _videos = [];
    private readonly Action _scheduleCompositionPass;

    internal VideoComposition(Action scheduleCompositionPass)
    {
        ArgumentNullException.ThrowIfNull(scheduleCompositionPass);
        _scheduleCompositionPass = scheduleCompositionPass;
    }

    int IVideoCompositionOwner.RegisterVideo(GeneratedMediaPlayerResource video)
    {
        try
        {
            _videos.Add(video);
            return 0;
        }
        catch (OutOfMemoryException)
        {
            return unchecked((int)0x8007000E);
        }
    }

    void IVideoCompositionOwner.UnregisterVideo(GeneratedMediaPlayerResource video) => _videos.Remove(video);

    void IVideoCompositionOwner.ScheduleCompositionPass() => _scheduleCompositionPass();

    internal int BeginProcessVideo(bool displaySetChanged)
    {
        foreach (GeneratedMediaPlayerResource video in _videos)
        {
            int result = video.BeginComposition(displaySetChanged, out bool frameReady);
            if (result < 0)
            {
                return result;
            }

            if (frameReady)
            {
                video.NotifyFrameReady();
            }
        }

        return 0;
    }

    internal void EndProcessVideo()
    {
        foreach (GeneratedMediaPlayerResource video in _videos)
        {
            _ = video.EndComposition();
        }
    }
}
