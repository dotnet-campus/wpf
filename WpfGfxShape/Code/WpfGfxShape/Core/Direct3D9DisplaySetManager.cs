using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal sealed class Direct3D9DisplaySetManager
{
    private const int MaximumCreationAttempts = 5;

    private readonly object _syncRoot = new();
    private readonly Func<uint> _displayUniquenessProvider;
    private readonly Func<uint> _externalUpdateCountProvider;
    private readonly Func<uint, uint, Direct3D9DisplaySet> _displaySetFactory;
    private readonly Action<Direct3D9DisplaySet, Direct3D9DisplaySet>? _displayChangeNotifier;
    private Direct3D9DisplaySet? _currentDisplaySet;

    internal Direct3D9DisplaySetManager(
        Func<uint> displayUniquenessProvider,
        Func<uint> externalUpdateCountProvider,
        Func<uint, uint, Direct3D9DisplaySet> displaySetFactory,
        Action<Direct3D9DisplaySet, Direct3D9DisplaySet>? displayChangeNotifier = null)
    {
        ArgumentNullException.ThrowIfNull(displayUniquenessProvider);
        ArgumentNullException.ThrowIfNull(externalUpdateCountProvider);
        ArgumentNullException.ThrowIfNull(displaySetFactory);

        _displayUniquenessProvider = displayUniquenessProvider;
        _externalUpdateCountProvider = externalUpdateCountProvider;
        _displaySetFactory = displaySetFactory;
        _displayChangeNotifier = displayChangeNotifier;
    }

    internal Direct3D9DisplaySet DangerousGetLatestDisplaySet()
    {
        lock (_syncRoot)
        {
            if (_currentDisplaySet?.IsUpToDate() == true)
            {
                return _currentDisplaySet;
            }
        }

        for (int attempt = 0; attempt < MaximumCreationAttempts; attempt++)
        {
            uint externalUpdateCount = _externalUpdateCountProvider();
            uint displayUniqueness = _displayUniquenessProvider();
            Direct3D9DisplaySet? candidate = null;
            try
            {
                try
                {
                    candidate = _displaySetFactory(displayUniqueness, externalUpdateCount);
                }
                catch (COMException exception) when (exception.HResult == Direct3D9Factory.DisplayStateInvalidHResult)
                {
                    continue;
                }

                if (candidate is null)
                {
                    throw new InvalidOperationException("The display-set factory returned null.");
                }

                candidate.BindLatestDisplaySetProvider(DangerousGetLatestDisplaySet);

                if (externalUpdateCount != _externalUpdateCountProvider()
                    || displayUniqueness != _displayUniquenessProvider())
                {
                    continue;
                }

                Direct3D9DisplaySet selectedDisplaySet;
                Direct3D9DisplaySet? obsoleteDisplaySet = null;
                lock (_syncRoot)
                {
                    bool currentIsUpToDate = _currentDisplaySet?.IsUpToDate() == true;
                    bool newIsUpToDate = candidate.IsUpToDate();

                    if (!currentIsUpToDate && !newIsUpToDate)
                    {
                        continue;
                    }

                    if (_currentDisplaySet is not null
                        && !currentIsUpToDate
                        && newIsUpToDate
                        && _currentDisplaySet.HasDirect3D9Ex
                        && _currentDisplaySet.IsEquivalentTo(candidate))
                    {
                        _currentDisplaySet.UpdateUniqueness(candidate);
                        currentIsUpToDate = true;
                    }

                    if (currentIsUpToDate)
                    {
                        selectedDisplaySet = _currentDisplaySet!;
                    }
                    else
                    {
                        selectedDisplaySet = candidate;
                        candidate = null;
                        obsoleteDisplaySet = _currentDisplaySet;
                        _currentDisplaySet = selectedDisplaySet;
                    }
                }

                if (obsoleteDisplaySet is not null)
                {
                    _displayChangeNotifier?.Invoke(obsoleteDisplaySet, selectedDisplaySet);
                }

                return selectedDisplaySet;
            }
            finally
            {
                candidate?.Dispose();
            }
        }

        Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
        throw new InvalidOperationException();
    }
}
