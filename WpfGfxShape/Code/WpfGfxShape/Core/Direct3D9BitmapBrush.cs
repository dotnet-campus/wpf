namespace WpfGfxShape.Core;

internal delegate int Direct3D9DeriveBitmapColorSource(
    nint bitmapBrush,
    nint brushContext,
    out Direct3D9BitmapPipelineColorSource? texturedColorSource);

internal delegate int Direct3D9GetMaskColorSource(
    nint texturedColorSource,
    out nint maskColorSource);

internal sealed class Direct3D9BitmapBrush
{
    private readonly Direct3D9DeriveBitmapColorSource _deriveColorSource;
    private readonly Direct3D9GetMaskColorSource _getMaskColorSource;
    private readonly Func<Direct3D9PipelineColorSource, int> _setTexture;
    private readonly Action<nint> _resetMaskAlphaScaleFactor;
    private readonly Func<nint, int> _multiplyAlphaMask;
    private readonly Action<nint> _releaseColorSource;

    private Direct3D9BitmapPipelineColorSource? _texturedColorSource;

    internal Direct3D9BitmapBrush(
        Direct3D9DeriveBitmapColorSource deriveColorSource,
        Direct3D9GetMaskColorSource getMaskColorSource,
        Func<Direct3D9PipelineColorSource, int> setTexture,
        Action<nint> resetMaskAlphaScaleFactor,
        Func<nint, int> multiplyAlphaMask,
        Action<nint> releaseColorSource)
    {
        ArgumentNullException.ThrowIfNull(deriveColorSource);
        ArgumentNullException.ThrowIfNull(getMaskColorSource);
        ArgumentNullException.ThrowIfNull(setTexture);
        ArgumentNullException.ThrowIfNull(resetMaskAlphaScaleFactor);
        ArgumentNullException.ThrowIfNull(multiplyAlphaMask);
        ArgumentNullException.ThrowIfNull(releaseColorSource);

        _deriveColorSource = deriveColorSource;
        _getMaskColorSource = getMaskColorSource;
        _setTexture = setTexture;
        _resetMaskAlphaScaleFactor = resetMaskAlphaScaleFactor;
        _multiplyAlphaMask = multiplyAlphaMask;
        _releaseColorSource = releaseColorSource;
        PrimaryColorSource = new Direct3D9PrimaryColorSource(SendOperations);
    }

    internal Direct3D9BitmapPipelineColorSource? TexturedColorSource => _texturedColorSource;

    internal Direct3D9PrimaryColorSource PrimaryColorSource { get; }

    internal int SetBrushAndContext(nint bitmapBrush, nint brushContext)
    {
        ArgumentOutOfRangeException.ThrowIfZero(bitmapBrush);
        ArgumentOutOfRangeException.ThrowIfZero(brushContext);
        if (_texturedColorSource is not null)
        {
            throw new InvalidOperationException("The scratch bitmap brush is already in use.");
        }

        int result = _deriveColorSource(bitmapBrush, brushContext, out Direct3D9BitmapPipelineColorSource? texturedColorSource);
        _texturedColorSource = texturedColorSource;
        return result;
    }

    internal int SendOperations()
    {
        return SendOperationsCore(colorSource => _setTexture(colorSource.PipelineColorSource));
    }

    private int SendOperations(Direct3D9PipelineOperationSender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);
        return SendOperationsCore(sender.SetTexture);
    }

    private int SendOperationsCore(Func<Direct3D9BitmapPipelineColorSource, int> setTexture)
    {
        Direct3D9BitmapPipelineColorSource texturedColorSource = _texturedColorSource
            ?? throw new InvalidOperationException("The scratch bitmap brush has not been initialized.");

        int result = setTexture(texturedColorSource);
        if (result < 0)
        {
            return result;
        }

        result = _getMaskColorSource(texturedColorSource.BitmapColorSource, out nint maskColorSource);
        try
        {
            if (result < 0 || maskColorSource == 0)
            {
                return result;
            }

            _resetMaskAlphaScaleFactor(maskColorSource);
            return _multiplyAlphaMask(maskColorSource);
        }
        finally
        {
            if (maskColorSource != 0)
            {
                _releaseColorSource(maskColorSource);
            }
        }
    }

    internal uint Release()
    {
        Direct3D9BitmapPipelineColorSource? texturedColorSource = _texturedColorSource;
        _texturedColorSource = null;
        texturedColorSource?.Dispose();
        return 0;
    }
}
