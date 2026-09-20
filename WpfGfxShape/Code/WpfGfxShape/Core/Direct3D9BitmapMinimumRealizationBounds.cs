namespace WpfGfxShape.Core;

internal delegate bool Direct3D9GetRealizationSamplingBounds(
    nint realizationBounds,
    out MilRectF bounds);

internal sealed class Direct3D9BitmapMinimumRealizationBounds
{
    private readonly Direct3D9GetRealizationSamplingBounds _getBounds;

    internal Direct3D9BitmapMinimumRealizationBounds(Direct3D9GetRealizationSamplingBounds getBounds)
    {
        ArgumentNullException.ThrowIfNull(getBounds);
        _getBounds = getBounds;
    }

    internal Direct3D9BitmapMinimumRealizationBounds(Direct3D9DelayedBounds bounds)
    {
        ArgumentNullException.ThrowIfNull(bounds);
        _getBounds = (nint _, out MilRectF result) => bounds.TryGetBounds(out result);
    }

    internal bool Compute(
        nint realizationBounds,
        Direct3D9BitmapRealizationProperties properties,
        ref Direct3D9BitmapRealizationRectangle minimumBounds)
    {
        if (!_getBounds(realizationBounds, out MilRectF bitmapBounds))
        {
            return false;
        }

        uint width = minimumBounds.Right;
        uint height = minimumBounds.Bottom;

        if (width != properties.BitmapWidth)
        {
            float widthPrefilterScale = (float) width / properties.BitmapWidth;
            bitmapBounds = bitmapBounds with
            {
                Left = bitmapBounds.Left * widthPrefilterScale,
                Right = bitmapBounds.Right * widthPrefilterScale
            };
        }

        if (height != properties.BitmapHeight)
        {
            float heightPrefilterScale = (float) height / properties.BitmapHeight;
            bitmapBounds = bitmapBounds with
            {
                Top = bitmapBounds.Top * heightPrefilterScale,
                Bottom = bitmapBounds.Bottom * heightPrefilterScale
            };
        }

        float roundingFactor = properties.InterpolationMode == MilBitmapInterpolationMode.NearestNeighbor
            ? 1.0f
            : 1.5f;

        (uint left, uint right) = ComputeSpan(
            bitmapBounds.Left,
            bitmapBounds.Right,
            width,
            roundingFactor,
            properties.WrapMode);
        (uint top, uint bottom) = ComputeSpan(
            bitmapBounds.Top,
            bitmapBounds.Bottom,
            height,
            roundingFactor,
            properties.WrapMode);

        minimumBounds = new Direct3D9BitmapRealizationRectangle(left, top, right, bottom);
        return true;
    }

    private static (uint Start, uint End) ComputeSpan(
        float sampleStart,
        float sampleEnd,
        uint span,
        float roundingFactor,
        MilBitmapWrapMode wrapMode)
    {
        int startSampleBound = CeilingSaturated(sampleStart - roundingFactor);
        int endSampleBound = FloorSaturated(sampleEnd + roundingFactor);

        if (startSampleBound >= endSampleBound)
        {
            return (0, span);
        }

        int signedSpan = checked((int) span);
        if (wrapMode != MilBitmapWrapMode.Extend)
        {
            return startSampleBound >= 0 && endSampleBound <= signedSpan
                ? ((uint) startSampleBound, (uint) endSampleBound)
                : (0, span);
        }

        uint start = startSampleBound > 0
            ? startSampleBound < signedSpan ? (uint) startSampleBound : span - 1
            : 0;
        uint end = endSampleBound < signedSpan
            ? endSampleBound > 0 ? (uint) endSampleBound : 1
            : span;

        return (start, end);
    }

    private static int FloorSaturated(float value)
    {
        if (!(value >= int.MinValue))
        {
            return int.MinValue;
        }

        return value < int.MaxValue
            ? (int) MathF.Floor(value)
            : int.MaxValue;
    }

    private static int CeilingSaturated(float value)
    {
        if (!(value >= int.MinValue))
        {
            return int.MinValue;
        }

        return value < int.MaxValue
            ? (int) MathF.Ceiling(value)
            : int.MaxValue;
    }
}
