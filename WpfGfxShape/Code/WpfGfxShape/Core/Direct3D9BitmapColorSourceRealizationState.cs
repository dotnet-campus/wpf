using System.Numerics;

namespace WpfGfxShape.Core;

internal enum Direct3D9BitmapRequiredBoundsCheck
{
    Required,
    Cached,
    PossibleAndUpdateRequired
}

internal delegate int Direct3D9GetDeviceBitmapValidSourceRectangles(
    nint bitmap,
    out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles);

internal sealed class Direct3D9BitmapColorSourceRealizationState
{
    private readonly nint _bitmap;
    private readonly Func<nint, uint> _getBitmapUniquenessToken;
    private nint _bitmapSource;
    private readonly MilPixelFormat _textureFormat;
    private Direct3D9BitmapRealizationProperties _properties;
    private uint _prefilterWidth;
    private uint _prefilterHeight;
    private Direct3D9BitmapRealizationRectangle _prefilteredBitmap;
    private Direct3D9TexelLayout _texelLayoutU;
    private Direct3D9TexelLayout _texelLayoutV;
    private Silk.NET.Direct3D9.Textureaddress _textureAddressU;
    private Silk.NET.Direct3D9.Textureaddress _textureAddressV;
    private MilBitmapInterpolationMode _interpolationMode;
    private Matrix3x2 _xSpaceToTextureUv;

    internal Direct3D9BitmapColorSourceRealizationState(
        nint bitmap,
        MilPixelFormat textureFormat,
        Func<nint, uint> getBitmapUniquenessToken)
    {
        ArgumentNullException.ThrowIfNull(getBitmapUniquenessToken);

        _bitmap = bitmap;
        _textureFormat = textureFormat;
        _getBitmapUniquenessToken = getBitmapUniquenessToken;
    }

    internal nint Bitmap => _bitmap;

    internal nint BitmapSource => _bitmapSource;

    internal uint CachedUniquenessToken { get; private set; }

    internal Direct3D9BitmapRealizationRectangle CachedRealizationBounds { get; private set; }

    internal Direct3D9BitmapRealizationRectangle RequiredRealizationBounds { get; private set; }

    internal uint PrefilterWidth => _prefilterWidth;

    internal uint PrefilterHeight => _prefilterHeight;

    internal uint BitmapWidth => _properties.BitmapWidth;

    internal uint BitmapHeight => _properties.BitmapHeight;

    internal Direct3D9BitmapRealizationRectangle PrefilteredBitmap => _prefilteredBitmap;

    internal Direct3D9TexelLayout TexelLayoutU => _texelLayoutU;

    internal Direct3D9TexelLayout TexelLayoutV => _texelLayoutV;

    internal Silk.NET.Direct3D9.Textureaddress TextureAddressU => _textureAddressU;

    internal Silk.NET.Direct3D9.Textureaddress TextureAddressV => _textureAddressV;

    internal MilBitmapInterpolationMode InterpolationMode => _interpolationMode;

    internal Matrix3x2 XSpaceToTextureUv => _xSpaceToTextureUv;

    internal void SetBitmapAndContext(
        nint bitmapSource,
        Direct3D9BitmapRealizationProperties properties)
    {
        SetBitmapAndContextCacheParameters(bitmapSource, properties);
        RequiredRealizationBounds = _prefilteredBitmap;
    }

    internal int SetBitmapAndContext(
        nint bitmapSource,
        Direct3D9BitmapRealizationProperties properties,
        Matrix3x2 bitmapToXSpace)
    {
        SetBitmapAndContext(bitmapSource, properties);
        return SetFilterModeAndTextureTransform(properties.InterpolationMode, bitmapToXSpace);
    }

    internal int SetBitmapAndContext(
        nint bitmapSource,
        Direct3D9BitmapRealizationProperties properties,
        bool isDeviceBitmap,
        Direct3D9DelayedBounds realizationBounds,
        Matrix3x2 bitmapToXSpace)
    {
        ArgumentNullException.ThrowIfNull(realizationBounds);

        SetBitmapAndContext(bitmapSource, properties);

        if (!properties.IsMinimumRealizationRectComputed && isDeviceBitmap)
        {
            ComputeMinimumRealizationBounds(realizationBounds, properties);
        }

        return SetFilterModeAndTextureTransform(properties.InterpolationMode, bitmapToXSpace);
    }

    internal int SetBitmapAndContext(
        nint bitmapSource,
        Direct3D9BitmapRealizationProperties properties,
        bool isDeviceBitmap,
        bool hasContributorFromDifferentAdapter,
        Direct3D9DelayedBounds realizationBounds,
        Direct3D9BitmapReusableRealizationCandidates reusableCandidates,
        Direct3D9BitmapReusableRealizationSources reusableSources,
        Matrix3x2 bitmapToXSpace)
    {
        ArgumentNullException.ThrowIfNull(realizationBounds);
        ArgumentNullException.ThrowIfNull(reusableCandidates);
        ArgumentNullException.ThrowIfNull(reusableSources);

        SetBitmapAndContext(bitmapSource, properties);

        if (!properties.IsMinimumRealizationRectComputed
            && isDeviceBitmap
            && (reusableCandidates.Head == 0 || hasContributorFromDifferentAdapter))
        {
            ComputeMinimumRealizationBounds(realizationBounds, properties);
        }

        reusableSources.Consume(reusableCandidates);
        return SetFilterModeAndTextureTransform(properties.InterpolationMode, bitmapToXSpace);
    }

    private void ComputeMinimumRealizationBounds(
        Direct3D9DelayedBounds realizationBounds,
        Direct3D9BitmapRealizationProperties properties)
    {
        Direct3D9BitmapRealizationRectangle requiredBounds = RequiredRealizationBounds;
        Direct3D9BitmapMinimumRealizationBounds minimumBounds = new(realizationBounds);
        minimumBounds.Compute(0, properties, ref requiredBounds);
        RequiredRealizationBounds = requiredBounds;
    }

    internal void SetBitmapAndContext(
        nint bitmapSource,
        Direct3D9BitmapRealizationProperties properties,
        bool minimumRealizationBoundsComputed,
        bool isDeviceBitmap,
        bool hasContributorFromDifferentAdapter,
        nint realizationBounds,
        Direct3D9BitmapReusableRealizationCandidates reusableCandidates,
        Direct3D9BitmapReusableRealizationSources reusableSources,
        Direct3D9ComputeMinimumRealizationBounds computeMinimumRealizationBounds)
    {
        ArgumentNullException.ThrowIfNull(reusableCandidates);
        ArgumentNullException.ThrowIfNull(reusableSources);
        ArgumentNullException.ThrowIfNull(computeMinimumRealizationBounds);

        SetBitmapAndContext(bitmapSource, properties);

        if (!minimumRealizationBoundsComputed
            && isDeviceBitmap
            && (reusableCandidates.Head == 0 || hasContributorFromDifferentAdapter))
        {
            Direct3D9BitmapRealizationRectangle requiredBounds = RequiredRealizationBounds;
            computeMinimumRealizationBounds(realizationBounds, properties, ref requiredBounds);
            RequiredRealizationBounds = requiredBounds;
        }

        reusableSources.Consume(reusableCandidates);
    }

    internal int SetBitmapAndContext(
        nint bitmapSource,
        Direct3D9BitmapRealizationProperties properties,
        bool minimumRealizationBoundsComputed,
        bool isDeviceBitmap,
        bool hasContributorFromDifferentAdapter,
        nint realizationBounds,
        Direct3D9BitmapReusableRealizationCandidates reusableCandidates,
        Direct3D9BitmapReusableRealizationSources reusableSources,
        Direct3D9ComputeMinimumRealizationBounds computeMinimumRealizationBounds,
        Matrix3x2 bitmapToXSpace)
    {
        SetBitmapAndContext(
            bitmapSource,
            properties,
            minimumRealizationBoundsComputed,
            isDeviceBitmap,
            hasContributorFromDifferentAdapter,
            realizationBounds,
            reusableCandidates,
            reusableSources,
            computeMinimumRealizationBounds);

        return SetFilterModeAndTextureTransform(properties.InterpolationMode, bitmapToXSpace);
    }

    internal int SetFilterModeAndTextureTransform(
        MilBitmapInterpolationMode interpolationMode,
        Matrix3x2 bitmapToXSpace)
    {
        int result = Direct3D9BitmapColorSourceTransform.Calculate(
            bitmapToXSpace,
            _properties.LayoutU.Length,
            _properties.LayoutV.Length,
            _properties.BitmapWidth,
            _properties.BitmapHeight,
            _prefilterWidth,
            _prefilterHeight,
            _prefilteredBitmap,
            _texelLayoutU,
            _texelLayoutV,
            out Matrix3x2 xSpaceToTextureUv);
        if (result < 0)
        {
            return result;
        }

        _interpolationMode = interpolationMode;
        _xSpaceToTextureUv = xSpaceToTextureUv;
        return Direct3D9Factory.SuccessHResult;
    }

    internal void SetBitmapAndContextCacheParameters(
        nint bitmapSource,
        Direct3D9BitmapRealizationProperties properties)
    {
        ArgumentOutOfRangeException.ThrowIfZero(bitmapSource);
        if (properties.TextureFormat != _textureFormat)
        {
            throw new InvalidOperationException("The bitmap color source texture format cannot change after creation.");
        }

        _bitmapSource = bitmapSource;
        _prefilterWidth = properties.Width;
        _prefilterHeight = properties.Height;
        _prefilteredBitmap = properties.SourceContained;
        _texelLayoutU = properties.LayoutU.TexelLayout;
        _texelLayoutV = properties.LayoutV.TexelLayout;
        _textureAddressU = properties.LayoutU.TextureAddress;
        _textureAddressV = properties.LayoutV.TextureAddress;
        _properties = properties;
    }

    internal void SetRequiredRealizationBounds(Direct3D9BitmapRealizationRectangle bounds) =>
        RequiredRealizationBounds = bounds;

    internal bool DoesContain(Direct3D9BitmapRealizationRectangle requiredBounds) =>
        Contains(_prefilteredBitmap, requiredBounds);

    internal void UpdateValidBounds(Direct3D9BitmapRealizationRectangle validBounds)
    {
        if (!DoesContain(validBounds))
        {
            throw new ArgumentOutOfRangeException(nameof(validBounds));
        }

        CachedRealizationBounds = validBounds;
        RequiredRealizationBounds = validBounds;
    }

    internal void ResetCachedRealization() => CachedRealizationBounds = default;

    internal void Commit(
        uint newestUniquenessToken,
        Direct3D9BitmapRealizationRectangle realizedBounds)
    {
        CachedUniquenessToken = newestUniquenessToken;
        CachedRealizationBounds = realizedBounds;
    }

    internal bool IsRealizationCurrent() =>
        _bitmap == 0 || CachedUniquenessToken == _getBitmapUniquenessToken(_bitmap);

    internal bool IsRealizationValid() =>
        Contains(CachedRealizationBounds, RequiredRealizationBounds) && IsRealizationCurrent();

    internal int GetValidSourceRectangles(
        bool isDeviceBitmap,
        Direct3D9GetDeviceBitmapValidSourceRectangles getDeviceBitmapValidSourceRectangles,
        out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles)
    {
        ArgumentNullException.ThrowIfNull(getDeviceBitmapValidSourceRectangles);

        if (isDeviceBitmap)
        {
            return getDeviceBitmapValidSourceRectangles(_bitmap, out rectangles);
        }

        rectangles = [RequiredRealizationBounds];
        return Direct3D9Factory.SuccessHResult;
    }

    internal IReadOnlyList<Direct3D9BitmapRealizationRectangle> GetUpdateRectangles(
        bool dirtyRectanglesAreValid,
        IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles)
    {
        ArgumentNullException.ThrowIfNull(dirtyRectangles);

        if (IsRealizationCurrent())
        {
            RequiredRealizationBounds = ExtendByAdjacentSections(
                RequiredRealizationBounds,
                CachedRealizationBounds);
        }

        bool completelyInvalid = !TryIntersect(
            CachedRealizationBounds,
            RequiredRealizationBounds,
            out Direct3D9BitmapRealizationRectangle retainedCachedBounds);
        CachedRealizationBounds = retainedCachedBounds;

        if (!dirtyRectanglesAreValid)
        {
            completelyInvalid = true;
        }

        List<Direct3D9BitmapRealizationRectangle> updateRectangles = [];
        if (!completelyInvalid)
        {
            foreach (Direct3D9BitmapRealizationRectangle dirtyRectangle in dirtyRectangles)
            {
                Direct3D9BitmapRealizationRectangle prefilteredDirtyRectangle = ScaleToPrefiltered(dirtyRectangle);
                if (TryIntersect(prefilteredDirtyRectangle, CachedRealizationBounds, out Direct3D9BitmapRealizationRectangle clippedDirtyRectangle))
                {
                    updateRectangles.Add(clippedDirtyRectangle);
                }
            }

            if (updateRectangles.Count > 0 && Contains(updateRectangles[0], CachedRealizationBounds))
            {
                completelyInvalid = true;
            }
        }

        if (completelyInvalid)
        {
            return [RequiredRealizationBounds];
        }

        AddSubtractionRectangles(RequiredRealizationBounds, CachedRealizationBounds, updateRectangles);
        return updateRectangles;
    }

    internal bool CheckRequiredRealizationBounds(
        Direct3D9DelayedBounds realizationBounds,
        MilBitmapInterpolationMode interpolationMode,
        MilBitmapWrapMode wrapMode,
        Direct3D9BitmapRequiredBoundsCheck check)
    {
        ArgumentNullException.ThrowIfNull(realizationBounds);

        Direct3D9BitmapMinimumRealizationBounds minimumBounds = new(realizationBounds);
        return CheckRequiredRealizationBounds(
            0,
            interpolationMode,
            wrapMode,
            check,
            minimumBounds.Compute);
    }

    internal bool CheckRequiredRealizationBounds(
        nint realizationBounds,
        MilBitmapInterpolationMode interpolationMode,
        MilBitmapWrapMode wrapMode,
        Direct3D9BitmapRequiredBoundsCheck check,
        Direct3D9ComputeMinimumRealizationBounds computeMinimumRealizationBounds)
    {
        ArgumentNullException.ThrowIfNull(computeMinimumRealizationBounds);

        if (check == Direct3D9BitmapRequiredBoundsCheck.Required
            && _properties.Width == RequiredRealizationBounds.Width
            && _properties.Height == RequiredRealizationBounds.Height)
        {
            return true;
        }

        Direct3D9BitmapRealizationRectangle requiredBounds = new(
            0,
            0,
            _properties.Width,
            _properties.Height);
        Direct3D9BitmapRealizationProperties properties = _properties with
        {
            InterpolationMode = interpolationMode,
            WrapMode = wrapMode
        };

        if (!computeMinimumRealizationBounds(realizationBounds, properties, ref requiredBounds))
        {
            return false;
        }

        Direct3D9BitmapRealizationRectangle checkBounds = check switch
        {
            Direct3D9BitmapRequiredBoundsCheck.Required => RequiredRealizationBounds,
            Direct3D9BitmapRequiredBoundsCheck.Cached => CachedRealizationBounds,
            Direct3D9BitmapRequiredBoundsCheck.PossibleAndUpdateRequired => _properties.SourceContained,
            _ => throw new ArgumentOutOfRangeException(nameof(check))
        };

        if (!Contains(checkBounds, requiredBounds))
        {
            return false;
        }

        if (check == Direct3D9BitmapRequiredBoundsCheck.PossibleAndUpdateRequired)
        {
            RequiredRealizationBounds = requiredBounds;
        }

        return true;
    }

    private Direct3D9BitmapRealizationRectangle ScaleToPrefiltered(
        Direct3D9BitmapRealizationRectangle rectangle)
    {
        uint left = rectangle.Left;
        uint top = rectangle.Top;
        uint right = rectangle.Right;
        uint bottom = rectangle.Bottom;

        if (_properties.BitmapWidth != _properties.Width)
        {
            ScaleInterval(ref left, ref right, _properties.BitmapWidth, _properties.Width);
        }

        if (_properties.BitmapHeight != _properties.Height)
        {
            ScaleInterval(ref top, ref bottom, _properties.BitmapHeight, _properties.Height);
        }

        return new Direct3D9BitmapRealizationRectangle(left, top, right, bottom);
    }

    private static void ScaleInterval(ref uint start, ref uint end, uint originalSize, uint prefilteredSize)
    {
        start = checked((uint) ((ulong) start * prefilteredSize / originalSize));
        end = checked((uint) (((ulong) end * prefilteredSize + originalSize - 1) / originalSize));
    }

    private static Direct3D9BitmapRealizationRectangle ExtendByAdjacentSections(
        Direct3D9BitmapRealizationRectangle baseRectangle,
        Direct3D9BitmapRealizationRectangle possibleExtension)
    {
        bool extendVertically = possibleExtension.Bottom >= baseRectangle.Top
            && possibleExtension.Top <= baseRectangle.Bottom
            && possibleExtension.Left <= baseRectangle.Left
            && baseRectangle.Right <= possibleExtension.Right;
        bool extendHorizontally = possibleExtension.Right >= baseRectangle.Left
            && possibleExtension.Left <= baseRectangle.Right
            && possibleExtension.Top <= baseRectangle.Top
            && baseRectangle.Bottom <= possibleExtension.Bottom;

        return new Direct3D9BitmapRealizationRectangle(
            extendHorizontally ? Math.Min(baseRectangle.Left, possibleExtension.Left) : baseRectangle.Left,
            extendVertically ? Math.Min(baseRectangle.Top, possibleExtension.Top) : baseRectangle.Top,
            extendHorizontally ? Math.Max(baseRectangle.Right, possibleExtension.Right) : baseRectangle.Right,
            extendVertically ? Math.Max(baseRectangle.Bottom, possibleExtension.Bottom) : baseRectangle.Bottom);
    }

    private static bool TryIntersect(
        Direct3D9BitmapRealizationRectangle first,
        Direct3D9BitmapRealizationRectangle second,
        out Direct3D9BitmapRealizationRectangle intersection)
    {
        uint left = Math.Max(first.Left, second.Left);
        uint top = Math.Max(first.Top, second.Top);
        uint right = Math.Min(first.Right, second.Right);
        uint bottom = Math.Min(first.Bottom, second.Bottom);
        intersection = left < right && top < bottom
            ? new Direct3D9BitmapRealizationRectangle(left, top, right, bottom)
            : default;
        return left < right && top < bottom;
    }

    private static void AddSubtractionRectangles(
        Direct3D9BitmapRealizationRectangle rectangle,
        Direct3D9BitmapRealizationRectangle intersection,
        List<Direct3D9BitmapRealizationRectangle> destination)
    {
        AddIfNotEmpty(destination, new(rectangle.Left, rectangle.Top, rectangle.Right, intersection.Top));
        AddIfNotEmpty(destination, new(rectangle.Left, intersection.Bottom, rectangle.Right, rectangle.Bottom));
        AddIfNotEmpty(destination, new(rectangle.Left, intersection.Top, intersection.Left, intersection.Bottom));
        AddIfNotEmpty(destination, new(intersection.Right, intersection.Top, rectangle.Right, intersection.Bottom));
    }

    private static void AddIfNotEmpty(
        List<Direct3D9BitmapRealizationRectangle> destination,
        Direct3D9BitmapRealizationRectangle rectangle)
    {
        if (rectangle.Left < rectangle.Right && rectangle.Top < rectangle.Bottom)
        {
            destination.Add(rectangle);
        }
    }

    private static bool Contains(
        Direct3D9BitmapRealizationRectangle outer,
        Direct3D9BitmapRealizationRectangle inner) =>
        outer.Left <= inner.Left
        && outer.Top <= inner.Top
        && outer.Right >= inner.Right
        && outer.Bottom >= inner.Bottom;
}
