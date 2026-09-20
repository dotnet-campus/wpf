namespace WpfGfxShape.Core;

internal sealed class Direct3D9BitmapReusableRealizationContext : IDisposable
{
    private bool _isDisposed;

    internal Direct3D9BitmapReusableRealizationContext(
        bool hasContributorFromDifferentAdapter,
        Direct3D9BitmapReusableRealizationCandidates candidates,
        Direct3D9BitmapReusableRealizationSources sources,
        Direct3D9BitmapReusableRealizationPopulation population)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(population);

        HasContributorFromDifferentAdapter = hasContributorFromDifferentAdapter;
        Candidates = candidates;
        Sources = sources;
        Population = population;
    }

    internal bool HasContributorFromDifferentAdapter { get; }

    internal Direct3D9BitmapReusableRealizationCandidates Candidates
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return field;
        }
    }

    internal Direct3D9BitmapReusableRealizationSources Sources
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return field;
        }
    }

    internal Direct3D9BitmapReusableRealizationPopulation Population
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return field;
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        Candidates.Dispose();
        Sources.Dispose();
        _isDisposed = true;
    }
}
