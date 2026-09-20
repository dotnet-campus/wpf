namespace WpfGfxShape.Core;

internal delegate nint Direct3D9GetMetaIntermediate(nint resource);

internal delegate int Direct3D9ReplaceMetaIntermediate(
    nint resource,
    nint metaIntermediate,
    uint realizationCacheIndex,
    ulong realizationDestination);

internal enum Direct3D9BrushType
{
    Solid = 1,
    LinearGradient,
    RadialGradient,
    Bitmap,
    ShaderEffect
}

internal sealed class Direct3D9ImmediateBrushRealizer
{
    internal const uint InvalidRealizationCacheIndex = uint.MaxValue;
    internal const uint SoftwareRealizationCacheIndex = 0;

    private readonly nint _solidColorBrush;
    private readonly Action<nint> _addRef;
    private readonly Action<nint> _release;
    private readonly Action<nint, MilColorF> _setSolidColor;
    private readonly Func<nint, bool> _mayNeedNonPow2Tiling;
    private readonly Func<nint, Direct3D9BrushType> _getBrushType;
    private readonly Func<nint, bool> _hasSourceClip;
    private readonly Func<nint, bool> _sourceClipIsEntireSource;
    private readonly Direct3D9GetMetaIntermediate _getBrushMetaIntermediate;
    private readonly Direct3D9GetMetaIntermediate _getEffectMetaIntermediate;
    private readonly Direct3D9ReplaceMetaIntermediate _replaceBrushMetaIntermediate;
    private readonly Direct3D9ReplaceMetaIntermediate _replaceEffectMetaIntermediate;
    private readonly Action<nint, nint> _restoreBrushMetaIntermediate;
    private readonly Action<nint, nint> _restoreEffectMetaIntermediate;

    private nint _realizedBrush;
    private nint _effects;
    private nint _brushMetaIntermediate;
    private nint _effectMetaIntermediate;
    private bool _isReleased;

    internal Direct3D9ImmediateBrushRealizer(
        nint solidColorBrush,
        Action<nint> addRef,
        Action<nint> release,
        Action<nint, MilColorF> setSolidColor,
        Func<nint, bool> mayNeedNonPow2Tiling,
        Func<nint, Direct3D9BrushType> getBrushType,
        Func<nint, bool> hasSourceClip,
        Func<nint, bool> sourceClipIsEntireSource,
        Direct3D9GetMetaIntermediate getBrushMetaIntermediate,
        Direct3D9GetMetaIntermediate getEffectMetaIntermediate,
        Direct3D9ReplaceMetaIntermediate replaceBrushMetaIntermediate,
        Direct3D9ReplaceMetaIntermediate replaceEffectMetaIntermediate,
        Action<nint, nint> restoreBrushMetaIntermediate,
        Action<nint, nint> restoreEffectMetaIntermediate)
    {
        ArgumentOutOfRangeException.ThrowIfZero(solidColorBrush);
        ArgumentNullException.ThrowIfNull(addRef);
        ArgumentNullException.ThrowIfNull(release);
        ArgumentNullException.ThrowIfNull(setSolidColor);
        ArgumentNullException.ThrowIfNull(mayNeedNonPow2Tiling);
        ArgumentNullException.ThrowIfNull(getBrushType);
        ArgumentNullException.ThrowIfNull(hasSourceClip);
        ArgumentNullException.ThrowIfNull(sourceClipIsEntireSource);
        ArgumentNullException.ThrowIfNull(getBrushMetaIntermediate);
        ArgumentNullException.ThrowIfNull(getEffectMetaIntermediate);
        ArgumentNullException.ThrowIfNull(replaceBrushMetaIntermediate);
        ArgumentNullException.ThrowIfNull(replaceEffectMetaIntermediate);
        ArgumentNullException.ThrowIfNull(restoreBrushMetaIntermediate);
        ArgumentNullException.ThrowIfNull(restoreEffectMetaIntermediate);

        _solidColorBrush = solidColorBrush;
        _addRef = addRef;
        _release = release;
        _setSolidColor = setSolidColor;
        _mayNeedNonPow2Tiling = mayNeedNonPow2Tiling;
        _getBrushType = getBrushType;
        _hasSourceClip = hasSourceClip;
        _sourceClipIsEntireSource = sourceClipIsEntireSource;
        _getBrushMetaIntermediate = getBrushMetaIntermediate;
        _getEffectMetaIntermediate = getEffectMetaIntermediate;
        _replaceBrushMetaIntermediate = replaceBrushMetaIntermediate;
        _replaceEffectMetaIntermediate = replaceEffectMetaIntermediate;
        _restoreBrushMetaIntermediate = restoreBrushMetaIntermediate;
        _restoreEffectMetaIntermediate = restoreEffectMetaIntermediate;

        _setSolidColor(_solidColorBrush, new MilColorF(0, 0, 0, 0));
    }

    internal nint Effects => _effects;

    internal void SetBrush(nint brush, nint effects, bool skipMetaFixups)
    {
        ObjectDisposedException.ThrowIf(_isReleased, this);
        ArgumentOutOfRangeException.ThrowIfZero(brush);
        EnsureCleanState();

        _addRef(brush);
        _realizedBrush = brush;
        if (effects != 0)
        {
            _addRef(effects);
            _effects = effects;
        }

        if (!skipMetaFixups)
        {
            if (_getBrushType(brush) == Direct3D9BrushType.Bitmap)
            {
                _brushMetaIntermediate = _getBrushMetaIntermediate(brush);
            }

            if (effects != 0)
            {
                _effectMetaIntermediate = _getEffectMetaIntermediate(effects);
            }
        }
    }

    internal void SetSolidColor(MilColorF color)
    {
        ObjectDisposedException.ThrowIf(_isReleased, this);
        EnsureCleanState();

        _setSolidColor(_solidColorBrush, color);
        _addRef(_solidColorBrush);
        _realizedBrush = _solidColorBrush;
    }

    internal nint GetRealizedBrush(bool convertNullToTransparent)
    {
        ObjectDisposedException.ThrowIf(_isReleased, this);
        return convertNullToTransparent && _realizedBrush == 0
            ? _solidColorBrush
            : _realizedBrush;
    }

    internal bool RealizedBrushMayNeedNonPow2Tiling()
    {
        ObjectDisposedException.ThrowIf(_isReleased, this);
        return _realizedBrush != 0 && _mayNeedNonPow2Tiling(_realizedBrush);
    }

    internal bool RealizedBrushWillHaveSourceClip()
    {
        ObjectDisposedException.ThrowIf(_isReleased, this);
        return _realizedBrush != 0
            && _getBrushType(_realizedBrush) == Direct3D9BrushType.Bitmap
            && _hasSourceClip(_realizedBrush);
    }

    internal bool RealizedBrushSourceClipMayBeEntireSource()
    {
        ObjectDisposedException.ThrowIf(_isReleased, this);
        if (!RealizedBrushWillHaveSourceClip())
        {
            throw new InvalidOperationException();
        }

        return _sourceClipIsEntireSource(_realizedBrush);
    }

    internal int EnsureRealization(uint realizationCacheIndex, ulong realizationDestination)
    {
        ObjectDisposedException.ThrowIf(_isReleased, this);
        uint effectiveCacheIndex = realizationCacheIndex == InvalidRealizationCacheIndex
            ? SoftwareRealizationCacheIndex
            : realizationCacheIndex;

        if (_brushMetaIntermediate != 0)
        {
            int result = _replaceBrushMetaIntermediate(
                _realizedBrush,
                _brushMetaIntermediate,
                effectiveCacheIndex,
                realizationDestination);
            if (result < 0)
            {
                return result;
            }
        }

        if (_effectMetaIntermediate != 0)
        {
            return _replaceEffectMetaIntermediate(
                _effects,
                _effectMetaIntermediate,
                effectiveCacheIndex,
                realizationDestination);
        }

        return 0;
    }

    internal void RestoreMetaIntermediates()
    {
        ObjectDisposedException.ThrowIf(_isReleased, this);
        RestoreMetaIntermediatesCore();
    }

    internal uint Release()
    {
        if (_isReleased)
        {
            return 0;
        }

        RestoreMetaIntermediatesCore();

        nint effects = _effects;
        _effects = 0;
        if (effects != 0)
        {
            _release(effects);
        }

        nint realizedBrush = _realizedBrush;
        _realizedBrush = 0;
        if (realizedBrush != 0)
        {
            _release(realizedBrush);
        }

        _isReleased = true;
        return 0;
    }

    private void RestoreMetaIntermediatesCore()
    {
        nint brushMetaIntermediate = _brushMetaIntermediate;
        _brushMetaIntermediate = 0;
        if (brushMetaIntermediate != 0)
        {
            _restoreBrushMetaIntermediate(_realizedBrush, brushMetaIntermediate);
        }

        nint effectMetaIntermediate = _effectMetaIntermediate;
        _effectMetaIntermediate = 0;
        if (effectMetaIntermediate != 0)
        {
            _restoreEffectMetaIntermediate(_effects, effectMetaIntermediate);
        }
    }

    private void EnsureCleanState()
    {
        if (_realizedBrush != 0 || _effects != 0 || _brushMetaIntermediate != 0 || _effectMetaIntermediate != 0)
        {
            throw new InvalidOperationException();
        }
    }
}
