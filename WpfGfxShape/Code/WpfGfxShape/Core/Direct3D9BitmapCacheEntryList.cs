namespace WpfGfxShape.Core;

internal enum Direct3D9BitmapSizeLayoutMatch
{
    NoMatch,
    ReusableSource,
    PartialOverlap,
    MeetsAllRequirements
}

internal sealed class Direct3D9BitmapCacheEntryList : IDisposable
{
    private readonly List<nint> _colorSources = [];
    private readonly List<Direct3D9BitmapRealizationProperties> _properties = [];
    private readonly Action<nint> _addReference;
    private readonly Action<nint> _release;
    private bool _isDisposed;

    internal Direct3D9BitmapCacheEntryList(Action<nint> addReference, Action<nint> release)
    {
        ArgumentNullException.ThrowIfNull(addReference);
        ArgumentNullException.ThrowIfNull(release);

        _addReference = addReference;
        _release = release;
    }

    internal int Count
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _colorSources.Count;
        }
    }

    internal int Add(nint colorSource) => Add(default, colorSource);

    internal int Add(Direct3D9BitmapRealizationProperties properties, nint colorSource)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        int index = _colorSources.Count;
        _properties.Add(properties);
        _colorSources.Add(colorSource);
        AddReference(colorSource);
        return index;
    }

    internal bool TryAcquire(
        ref Direct3D9BitmapRealizationProperties properties,
        Func<nint, bool> isValid,
        Direct3D9BitmapReusableRealizationCandidates? reusableCandidates,
        out nint colorSource)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(isValid);

        for (int index = 0; index < _colorSources.Count; index++)
        {
            Direct3D9BitmapSizeLayoutMatch match = CheckSizeLayoutMatch(_properties[index], properties);
            if (match == Direct3D9BitmapSizeLayoutMatch.ReusableSource)
            {
                nint reusableSource = _colorSources[index];
                if (reusableSource != 0)
                {
                    reusableCandidates?.Add(reusableSource);
                }

                continue;
            }

            if (match < Direct3D9BitmapSizeLayoutMatch.PartialOverlap)
            {
                continue;
            }

            nint cachedColorSource = _colorSources[index];
            if (cachedColorSource != 0
                && match != Direct3D9BitmapSizeLayoutMatch.PartialOverlap
                && isValid(cachedColorSource))
            {
                properties = RestoreRequestedTextureAddresses(_properties[index], properties);
                _addReference(cachedColorSource);
                colorSource = cachedColorSource;
                return true;
            }

            if (cachedColorSource != 0)
            {
                if (match == Direct3D9BitmapSizeLayoutMatch.PartialOverlap)
                {
                    _properties[index] = properties;
                }

                Release(cachedColorSource);
                _colorSources[index] = 0;
            }

            if (match == Direct3D9BitmapSizeLayoutMatch.PartialOverlap)
            {
                RemoveLaterPartialOverlaps(index, properties);
            }

            colorSource = 0;
            return false;
        }

        colorSource = 0;
        return false;
    }

    internal int Store(Direct3D9BitmapRealizationProperties properties, nint colorSource)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        for (int index = 0; index < _colorSources.Count; index++)
        {
            Direct3D9BitmapSizeLayoutMatch match = CheckSizeLayoutMatch(_properties[index], properties);
            if (match < Direct3D9BitmapSizeLayoutMatch.PartialOverlap)
            {
                continue;
            }

            _properties[index] = properties;
            Replace(index, colorSource);
            if (match == Direct3D9BitmapSizeLayoutMatch.PartialOverlap)
            {
                RemoveLaterPartialOverlaps(index, properties);
            }

            return index;
        }

        return Add(properties, colorSource);
    }

    internal static Direct3D9BitmapSizeLayoutMatch CheckSizeLayoutMatch(
        Direct3D9BitmapRealizationProperties cachedProperties,
        Direct3D9BitmapRealizationProperties newProperties)
    {
        if (cachedProperties.Width != newProperties.Width || cachedProperties.Height != newProperties.Height)
        {
            return Direct3D9BitmapSizeLayoutMatch.NoMatch;
        }

        Direct3D9BitmapSizeLayoutMatch match =
            !cachedProperties.OnlyContainsSubRectangleOfSource
            && !HasBorder(cachedProperties.LayoutU.TexelLayout)
            && !HasBorder(cachedProperties.LayoutV.TexelLayout)
            && !HasBorder(newProperties.LayoutU.TexelLayout)
            && !HasBorder(newProperties.LayoutV.TexelLayout)
                ? Direct3D9BitmapSizeLayoutMatch.ReusableSource
                : Direct3D9BitmapSizeLayoutMatch.NoMatch;

        if (cachedProperties.LayoutU.TexelLayout != newProperties.LayoutU.TexelLayout
            || cachedProperties.LayoutV.TexelLayout != newProperties.LayoutV.TexelLayout
            || cachedProperties.OnlyContainsSubRectangleOfSource != newProperties.OnlyContainsSubRectangleOfSource
            || cachedProperties.MipMapLevel < newProperties.MipMapLevel)
        {
            return match;
        }

        if (!cachedProperties.OnlyContainsSubRectangleOfSource)
        {
            return Direct3D9BitmapSizeLayoutMatch.MeetsAllRequirements;
        }

        if (!Intersects(cachedProperties.SourceContained, newProperties.SourceContained))
        {
            return match;
        }

        return Contains(cachedProperties.SourceContained, newProperties.SourceContained)
            ? Direct3D9BitmapSizeLayoutMatch.MeetsAllRequirements
            : Direct3D9BitmapSizeLayoutMatch.PartialOverlap;
    }

    internal void Replace(int index, nint colorSource)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        nint previousColorSource = _colorSources[index];
        Release(previousColorSource);
        _colorSources[index] = colorSource;
        AddReference(colorSource);
    }

    internal bool TryAcquireValid(int index, Func<nint, bool> isValid, out nint colorSource)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(isValid);

        nint cachedColorSource = _colorSources[index];
        if (cachedColorSource != 0 && isValid(cachedColorSource))
        {
            _addReference(cachedColorSource);
            colorSource = cachedColorSource;
            return true;
        }

        if (cachedColorSource != 0)
        {
            Release(cachedColorSource);
            _colorSources[index] = 0;
        }

        colorSource = 0;
        return false;
    }

    internal void RemoveEntriesAfter(int retainedIndex, Func<int, nint, bool> shouldRemove)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(shouldRemove);
        ArgumentOutOfRangeException.ThrowIfNegative(retainedIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(retainedIndex, _colorSources.Count);

        int index = retainedIndex + 1;
        while (index < _colorSources.Count)
        {
            nint colorSource = _colorSources[index];
            if (!shouldRemove(index, colorSource))
            {
                index++;
                continue;
            }

            Release(colorSource);
            int lastIndex = _colorSources.Count - 1;
            if (index != lastIndex)
            {
                _colorSources[index] = _colorSources[lastIndex];
                _properties[index] = _properties[lastIndex];
            }

            _colorSources.RemoveAt(lastIndex);
            _properties.RemoveAt(lastIndex);
        }
    }

    internal nint GetColorSourceNoReference(int index)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _colorSources[index];
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        foreach (nint colorSource in _colorSources)
        {
            Release(colorSource);
        }

        _colorSources.Clear();
        _properties.Clear();
    }

    private void RemoveLaterPartialOverlaps(
        int retainedIndex,
        Direct3D9BitmapRealizationProperties properties)
    {
        RemoveEntriesAfter(
            retainedIndex,
            (index, _) => CheckSizeLayoutMatch(_properties[index], properties)
                == Direct3D9BitmapSizeLayoutMatch.PartialOverlap);
    }

    private static Direct3D9BitmapRealizationProperties RestoreRequestedTextureAddresses(
        Direct3D9BitmapRealizationProperties cachedProperties,
        Direct3D9BitmapRealizationProperties requestedProperties) =>
        cachedProperties with
        {
            LayoutU = cachedProperties.LayoutU with
            {
                TextureAddress = requestedProperties.LayoutU.TextureAddress
            },
            LayoutV = cachedProperties.LayoutV with
            {
                TextureAddress = requestedProperties.LayoutV.TextureAddress
            }
        };

    private static bool HasBorder(Direct3D9TexelLayout layout) =>
        layout is Direct3D9TexelLayout.EdgeWrapped or Direct3D9TexelLayout.EdgeMirrored;

    private static bool Intersects(
        Direct3D9BitmapRealizationRectangle first,
        Direct3D9BitmapRealizationRectangle second) =>
        first.Right > second.Left
        && second.Right > first.Left
        && first.Bottom > second.Top
        && second.Bottom > first.Top;

    private static bool Contains(
        Direct3D9BitmapRealizationRectangle outer,
        Direct3D9BitmapRealizationRectangle inner) =>
        inner.Left >= outer.Left
        && inner.Top >= outer.Top
        && inner.Right <= outer.Right
        && inner.Bottom <= outer.Bottom;

    private void AddReference(nint colorSource)
    {
        if (colorSource != 0)
        {
            _addReference(colorSource);
        }
    }

    private void Release(nint colorSource)
    {
        if (colorSource != 0)
        {
            _release(colorSource);
        }
    }
}
