using System.Numerics;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9SoftwareBilinearFixedPointState(
    int M11,
    int M12,
    int M21,
    int M22,
    int Dx,
    int Dy,
    int UIncrement,
    int VIncrement,
    int ModulusWidth,
    int ModulusHeight,
    int XEdgeIncrement,
    int YEdgeIncrement,
    int XDeviceOffset,
    int YDeviceOffset);

internal readonly record struct Direct3D9SoftwareBilinearTexelSelection(
    int X1,
    int Y1,
    int X2,
    int Y2,
    int XFraction,
    int YFraction,
    bool AInside,
    bool BInside,
    bool CInside,
    bool DInside);

internal readonly record struct Direct3D9SoftwareBilinearAdvance(
    long U,
    long V,
    bool IsOnBorder);

internal readonly record struct Direct3D9SoftwareBilinearInteriorAdvance(
    int U,
    int V,
    uint RemainingCount);

internal readonly record struct Direct3D9SoftwareBilinearLoopState(
    int U,
    int V,
    Direct3D9SoftwareBilinearInteriorRegion URegion,
    Direct3D9SoftwareBilinearInteriorRegion VRegion,
    bool RequiresFallback);

internal readonly record struct Direct3D9SoftwareBilinearInteriorBounds(
    int UMinimum,
    int UMaximum,
    int VMinimum,
    int VMaximum,
    bool IsFlipped);

internal readonly record struct Direct3D9SoftwareBilinearInteriorBatchPlan(
    uint Count,
    bool IsFlipped,
    int U,
    int V,
    uint RemainingCount,
    Direct3D9SoftwareBilinearLoopState NextLoopState);

internal readonly record struct Direct3D9SoftwareBilinearInteriorSampleSlice(
    uint Count,
    Direct3D9SoftwareBilinearInteriorBounds Bounds,
    Direct3D9SoftwareBilinearLoopState FirstSample,
    Direct3D9SoftwareBilinearLoopState LastSample,
    Direct3D9SoftwareBilinearLoopState AdvancedSample);

internal readonly record struct Direct3D9SoftwareBilinearInteriorTexelSelectionSlice(
    uint Count,
    Direct3D9SoftwareBilinearTexelSelection FirstSelection,
    Direct3D9SoftwareBilinearTexelSelection LastSelection);

internal readonly record struct Direct3D9SoftwareBilinearInteriorArgbSlice(
    uint Count,
    uint FirstArgb,
    uint LastArgb);

internal readonly record struct Direct3D9SoftwareBilinearInteriorArgbBatch(
    uint Count,
    uint FirstArgb,
    uint LastArgb,
    int U,
    int V,
    uint RemainingCount,
    Direct3D9SoftwareBilinearLoopState NextLoopState,
    Direct3D9SoftwareBilinearNextBranch NextBranch);

internal readonly record struct Direct3D9SoftwareBilinearFallbackBatchAdvance(
    uint Count,
    int U,
    int V,
    uint RemainingCount,
    Direct3D9SoftwareBilinearNextBranch NextBranch);

internal readonly record struct Direct3D9SoftwareBilinearFallbackBatchSlice(
    uint CandidateCount,
    uint Count,
    int U,
    int V,
    uint RemainingCount,
    Direct3D9SoftwareBilinearNextBranch NextBranch);

internal readonly record struct Direct3D9SoftwareBilinearFallbackRequestSlice(
    uint RequestedCount,
    uint Count,
    int U,
    int V,
    uint RemainingCount,
    Direct3D9SoftwareBilinearNextBranch NextBranch);

internal enum Direct3D9SoftwareBilinearNextBranch
{
    Complete,
    Fallback,
    Interior,
}

internal enum Direct3D9SoftwareBilinearInteriorRegion
{
    Boundary,
    Forward,
    Mirrored,
}

internal static class Direct3D9SoftwareBilinearSpan
{
    private const uint Fixed16IntegerMaximum = 32767;
    private const uint AcceleratedTextureMaximum = 0x3FFF;
    private const long AcceleratedSpanMaximum = 0x3FFE0000;
    private const float Fixed16Scale = 1 << 16;

    internal static bool Requires64BitFallback(
        uint bitmapWidth,
        uint bitmapHeight,
        long u,
        long v,
        uint count,
        int uIncrement,
        int vIncrement)
    {
        if (bitmapWidth > AcceleratedTextureMaximum || bitmapHeight > AcceleratedTextureMaximum)
        {
            return true;
        }

        long uEnd = u + (count * (long) uIncrement);
        long vEnd = v + (count * (long) vIncrement);
        return IsOutsideAcceleratedSpan(u)
            || IsOutsideAcceleratedSpan(v)
            || IsOutsideAcceleratedSpan(uEnd)
            || IsOutsideAcceleratedSpan(vEnd);
    }

    internal static (int U, int V) WrapCanonicalPosition(
        int u,
        int v,
        int modulusWidth,
        int modulusHeight,
        MilBitmapWrapMode wrapMode)
    {
        if (wrapMode is MilBitmapWrapMode.Extend or MilBitmapWrapMode.Border)
        {
            throw new ArgumentOutOfRangeException(nameof(wrapMode));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(modulusWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(modulusHeight);

        return (WrapCanonicalCoordinate(u, modulusWidth), WrapCanonicalCoordinate(v, modulusHeight));
    }

    private static int WrapCanonicalCoordinate(int coordinate, int modulus)
    {
        if ((uint) coordinate < (uint) modulus)
        {
            return coordinate;
        }

        if ((uint) coordinate < (uint) (2 * (long) modulus))
        {
            return coordinate - modulus;
        }

        if ((uint) -coordinate <= (uint) modulus)
        {
            return coordinate + modulus;
        }

        int remainder = coordinate % modulus;
        return remainder < 0 ? remainder + modulus : remainder;
    }

    internal static Direct3D9SoftwareBilinearTexelSelection Select64BitFallbackTexels(
        long u,
        long v,
        uint bitmapWidth,
        uint bitmapHeight,
        MilBitmapWrapMode wrapMode)
    {
        ArgumentOutOfRangeException.ThrowIfZero(bitmapWidth);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapHeight);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapWidth, (uint) int.MaxValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapHeight, (uint) int.MaxValue);

        int width = (int) bitmapWidth;
        int height = (int) bitmapHeight;
        int x1 = checked((int) (u >> 16));
        int y1 = checked((int) (v >> 16));
        int x2 = checked(x1 + 1);
        int y2 = checked(y1 + 1);
        int xFraction = (int) ((u >> 8) & 0xFF);
        int yFraction = (int) ((v >> 8) & 0xFF);

        if (wrapMode == MilBitmapWrapMode.Extend)
        {
            x1 = Math.Clamp(x1, 0, width - 1);
            x2 = Math.Clamp(x2, 0, width - 1);
            y1 = Math.Clamp(y1, 0, height - 1);
            y2 = Math.Clamp(y2, 0, height - 1);
            return new(x1, y1, x2, y2, xFraction, yFraction, true, true, true, true);
        }

        if (wrapMode == MilBitmapWrapMode.Border)
        {
            bool x1Inside = (uint) x1 < bitmapWidth;
            bool x2Inside = (uint) x2 < bitmapWidth;
            bool y1Inside = (uint) y1 < bitmapHeight;
            bool y2Inside = (uint) y2 < bitmapHeight;
            return new(
                x1,
                y1,
                x2,
                y2,
                xFraction,
                yFraction,
                x1Inside && y1Inside,
                x2Inside && y1Inside,
                x1Inside && y2Inside,
                x2Inside && y2Inside);
        }

        int canonicalWidth = wrapMode is MilBitmapWrapMode.FlipX or MilBitmapWrapMode.FlipXY
            ? checked(width * 2)
            : width;
        int canonicalHeight = wrapMode is MilBitmapWrapMode.FlipY or MilBitmapWrapMode.FlipXY
            ? checked(height * 2)
            : height;
        x1 = ResolveTileCoordinate(x1, width, canonicalWidth);
        x2 = ResolveTileCoordinate(x2, width, canonicalWidth);
        y1 = ResolveTileCoordinate(y1, height, canonicalHeight);
        y2 = ResolveTileCoordinate(y2, height, canonicalHeight);
        return new(x1, y1, x2, y2, xFraction, yFraction, true, true, true, true);
    }

    internal static Direct3D9SoftwareBilinearTexelSelection SelectFlippedTileInteriorTexels(
        int u,
        int v,
        uint bitmapWidth,
        uint bitmapHeight,
        MilBitmapWrapMode wrapMode)
    {
        if (wrapMode is not (MilBitmapWrapMode.FlipX or MilBitmapWrapMode.FlipY or MilBitmapWrapMode.FlipXY))
        {
            throw new ArgumentOutOfRangeException(nameof(wrapMode));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(bitmapWidth, 2u);
        ArgumentOutOfRangeException.ThrowIfLessThan(bitmapHeight, 2u);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapWidth, AcceleratedTextureMaximum);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapHeight, AcceleratedTextureMaximum);
        ArgumentOutOfRangeException.ThrowIfNegative(u);
        ArgumentOutOfRangeException.ThrowIfNegative(v);

        int width = (int) bitmapWidth;
        int height = (int) bitmapHeight;
        int x1 = u >> 16;
        int y1 = v >> 16;
        int canonicalWidth = wrapMode is MilBitmapWrapMode.FlipX or MilBitmapWrapMode.FlipXY
            ? width * 2
            : width;
        int canonicalHeight = wrapMode is MilBitmapWrapMode.FlipY or MilBitmapWrapMode.FlipXY
            ? height * 2
            : height;

        bool flipX = wrapMode is MilBitmapWrapMode.FlipX or MilBitmapWrapMode.FlipXY;
        bool flipY = wrapMode is MilBitmapWrapMode.FlipY or MilBitmapWrapMode.FlipXY;
        if (x1 >= canonicalWidth - 1
            || ClassifyInteriorRegion(u, bitmapWidth, flipX) == Direct3D9SoftwareBilinearInteriorRegion.Boundary)
        {
            throw new ArgumentOutOfRangeException(nameof(u));
        }

        if (y1 >= canonicalHeight - 1
            || ClassifyInteriorRegion(v, bitmapHeight, flipY) == Direct3D9SoftwareBilinearInteriorRegion.Boundary)
        {
            throw new ArgumentOutOfRangeException(nameof(v));
        }

        bool flippedX = x1 >= width;
        if (flippedX)
        {
            x1 = (2 * width) - x1 - 2;
        }

        bool flippedY = y1 >= height;
        if (flippedY)
        {
            y1 = (2 * height) - y1 - 2;
        }

        int x2 = x1 + 1;
        int y2 = y1 + 1;
        return new(
            flippedX ? x2 : x1,
            flippedY ? y2 : y1,
            flippedX ? x1 : x2,
            flippedY ? y1 : y2,
            (u >> 8) & 0xFF,
            (v >> 8) & 0xFF,
            true,
            true,
            true,
            true);
    }

    internal static uint CalculateInteriorSpanLength(
        int u,
        int v,
        int uIncrement,
        int vIncrement,
        int uMin,
        int uMax,
        int vMin,
        int vMax,
        uint count)
    {
        ArgumentOutOfRangeException.ThrowIfZero(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(uMin, uMax);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(vMin, vMax);

        if (u < uMin || u >= uMax)
        {
            throw new ArgumentOutOfRangeException(nameof(u));
        }

        if (v < vMin || v >= vMax)
        {
            throw new ArgumentOutOfRangeException(nameof(v));
        }

        uint uLength = CalculateInteriorDimensionLength(u, uIncrement, uMin, uMax);
        uint vLength = CalculateInteriorDimensionLength(v, vIncrement, vMin, vMax);
        return Math.Min(count, Math.Min(uLength, vLength));
    }

    internal static Direct3D9SoftwareBilinearInteriorAdvance AdvanceInteriorSpan(
        int u,
        int v,
        int uIncrement,
        int vIncrement,
        uint count,
        uint processedCount)
    {
        ArgumentOutOfRangeException.ThrowIfZero(count);
        ArgumentOutOfRangeException.ThrowIfZero(processedCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(processedCount, count);

        int advancedU = checked((int) (u + (processedCount * (long) uIncrement)));
        int advancedV = checked((int) (v + (processedCount * (long) vIncrement)));
        return new(advancedU, advancedV, count - processedCount);
    }

    internal static Direct3D9SoftwareBilinearInteriorRegion ClassifyInteriorRegion(
        int coordinate,
        uint bitmapSize,
        bool flipEnabled)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bitmapSize, 2u);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapSize, AcceleratedTextureMaximum);

        int forwardMaximum = checked((int) ((bitmapSize - 1) << 16));
        if ((uint) coordinate < (uint) forwardMaximum)
        {
            return Direct3D9SoftwareBilinearInteriorRegion.Forward;
        }

        if (flipEnabled)
        {
            int mirroredMinimum = checked((int) (bitmapSize << 16));
            int mirroredMaximum = checked((int) ((2 * bitmapSize - 1) << 16));
            if (coordinate >= mirroredMinimum && coordinate < mirroredMaximum)
            {
                return Direct3D9SoftwareBilinearInteriorRegion.Mirrored;
            }
        }

        return Direct3D9SoftwareBilinearInteriorRegion.Boundary;
    }

    internal static Direct3D9SoftwareBilinearLoopState ClassifyCanonicalLoopState(
        int u,
        int v,
        uint bitmapWidth,
        uint bitmapHeight,
        MilBitmapWrapMode wrapMode)
    {
        if (wrapMode is MilBitmapWrapMode.Extend or MilBitmapWrapMode.Border)
        {
            throw new ArgumentOutOfRangeException(nameof(wrapMode));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(bitmapWidth, 2u);
        ArgumentOutOfRangeException.ThrowIfLessThan(bitmapHeight, 2u);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapWidth, AcceleratedTextureMaximum);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapHeight, AcceleratedTextureMaximum);

        bool flipX = wrapMode is MilBitmapWrapMode.FlipX or MilBitmapWrapMode.FlipXY;
        bool flipY = wrapMode is MilBitmapWrapMode.FlipY or MilBitmapWrapMode.FlipXY;
        int modulusWidth = checked((int) ((flipX ? 2u : 1u) * bitmapWidth << 16));
        int modulusHeight = checked((int) ((flipY ? 2u : 1u) * bitmapHeight << 16));
        int canonicalU = WrapCanonicalCoordinate(u, modulusWidth);
        int canonicalV = WrapCanonicalCoordinate(v, modulusHeight);
        Direct3D9SoftwareBilinearInteriorRegion uRegion =
            ClassifyInteriorRegion(canonicalU, bitmapWidth, flipX);
        Direct3D9SoftwareBilinearInteriorRegion vRegion =
            ClassifyInteriorRegion(canonicalV, bitmapHeight, flipY);

        return new(
            canonicalU,
            canonicalV,
            uRegion,
            vRegion,
            uRegion == Direct3D9SoftwareBilinearInteriorRegion.Boundary
                || vRegion == Direct3D9SoftwareBilinearInteriorRegion.Boundary);
    }

    internal static Direct3D9SoftwareBilinearInteriorBounds SelectInteriorBounds(
        Direct3D9SoftwareBilinearLoopState loopState,
        uint bitmapWidth,
        uint bitmapHeight)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bitmapWidth, 2u);
        ArgumentOutOfRangeException.ThrowIfLessThan(bitmapHeight, 2u);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapWidth, AcceleratedTextureMaximum);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapHeight, AcceleratedTextureMaximum);

        if (loopState.RequiresFallback
            || loopState.URegion == Direct3D9SoftwareBilinearInteriorRegion.Boundary
            || loopState.VRegion == Direct3D9SoftwareBilinearInteriorRegion.Boundary)
        {
            throw new ArgumentOutOfRangeException(nameof(loopState));
        }

        (int uMinimum, int uMaximum) = SelectInteriorDimensionBounds(loopState.URegion, bitmapWidth);
        (int vMinimum, int vMaximum) = SelectInteriorDimensionBounds(loopState.VRegion, bitmapHeight);
        return new(
            uMinimum,
            uMaximum,
            vMinimum,
            vMaximum,
            loopState.URegion == Direct3D9SoftwareBilinearInteriorRegion.Mirrored
                || loopState.VRegion == Direct3D9SoftwareBilinearInteriorRegion.Mirrored);
    }

    internal static Direct3D9SoftwareBilinearInteriorBatchPlan CreateInteriorBatchPlan(
        Direct3D9SoftwareBilinearLoopState loopState,
        uint bitmapWidth,
        uint bitmapHeight,
        MilBitmapWrapMode wrapMode,
        int uIncrement,
        int vIncrement,
        uint count)
    {
        Direct3D9SoftwareBilinearInteriorBounds bounds =
            SelectInteriorBounds(loopState, bitmapWidth, bitmapHeight);
        uint batchCount = CalculateInteriorSpanLength(
            loopState.U,
            loopState.V,
            uIncrement,
            vIncrement,
            bounds.UMinimum,
            bounds.UMaximum,
            bounds.VMinimum,
            bounds.VMaximum,
            count);
        Direct3D9SoftwareBilinearInteriorAdvance advance = AdvanceInteriorSpan(
            loopState.U,
            loopState.V,
            uIncrement,
            vIncrement,
            count,
            batchCount);
        Direct3D9SoftwareBilinearLoopState nextLoopState =
            ClassifyCanonicalLoopState(
                advance.U,
                advance.V,
                bitmapWidth,
                bitmapHeight,
                wrapMode);

        return new(
            batchCount,
            bounds.IsFlipped,
            advance.U,
            advance.V,
            advance.RemainingCount,
            nextLoopState);
    }

    internal static Direct3D9SoftwareBilinearInteriorSampleSlice CreateInteriorSampleSlice(
        Direct3D9SoftwareBilinearLoopState loopState,
        uint bitmapWidth,
        uint bitmapHeight,
        MilBitmapWrapMode wrapMode,
        int uIncrement,
        int vIncrement,
        uint count)
    {
        Direct3D9SoftwareBilinearInteriorBounds bounds =
            SelectInteriorBounds(loopState, bitmapWidth, bitmapHeight);
        Direct3D9SoftwareBilinearInteriorBatchPlan plan = CreateInteriorBatchPlan(
            loopState,
            bitmapWidth,
            bitmapHeight,
            wrapMode,
            uIncrement,
            vIncrement,
            count);
        uint lastSampleOffset = plan.Count - 1;
        int lastU = checked((int) (loopState.U + (lastSampleOffset * (long) uIncrement)));
        int lastV = checked((int) (loopState.V + (lastSampleOffset * (long) vIncrement)));

        if (lastU < bounds.UMinimum || lastU >= bounds.UMaximum
            || lastV < bounds.VMinimum || lastV >= bounds.VMaximum)
        {
            throw new InvalidOperationException();
        }

        Direct3D9SoftwareBilinearLoopState lastSample = ClassifyCanonicalLoopState(
            lastU,
            lastV,
            bitmapWidth,
            bitmapHeight,
            wrapMode);
        return new(plan.Count, bounds, loopState, lastSample, plan.NextLoopState);
    }

    internal static Direct3D9SoftwareBilinearInteriorTexelSelectionSlice CreateInteriorTexelSelectionSlice(
        Direct3D9SoftwareBilinearInteriorSampleSlice sampleSlice,
        uint bitmapWidth,
        uint bitmapHeight,
        MilBitmapWrapMode wrapMode)
    {
        if (sampleSlice.Count == 0
            || sampleSlice.FirstSample.RequiresFallback
            || sampleSlice.LastSample.RequiresFallback
            || sampleSlice.FirstSample.U < sampleSlice.Bounds.UMinimum
            || sampleSlice.FirstSample.U >= sampleSlice.Bounds.UMaximum
            || sampleSlice.FirstSample.V < sampleSlice.Bounds.VMinimum
            || sampleSlice.FirstSample.V >= sampleSlice.Bounds.VMaximum
            || sampleSlice.LastSample.U < sampleSlice.Bounds.UMinimum
            || sampleSlice.LastSample.U >= sampleSlice.Bounds.UMaximum
            || sampleSlice.LastSample.V < sampleSlice.Bounds.VMinimum
            || sampleSlice.LastSample.V >= sampleSlice.Bounds.VMaximum)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleSlice));
        }

        Direct3D9SoftwareBilinearTexelSelection firstSelection = SelectFlippedTileInteriorTexels(
            sampleSlice.FirstSample.U,
            sampleSlice.FirstSample.V,
            bitmapWidth,
            bitmapHeight,
            wrapMode);
        Direct3D9SoftwareBilinearTexelSelection lastSelection = SelectFlippedTileInteriorTexels(
            sampleSlice.LastSample.U,
            sampleSlice.LastSample.V,
            bitmapWidth,
            bitmapHeight,
            wrapMode);
        return new(sampleSlice.Count, firstSelection, lastSelection);
    }

    internal static Direct3D9SoftwareBilinearInteriorArgbSlice AssembleInteriorArgbSlice(
        Direct3D9SoftwareBilinearInteriorTexelSelectionSlice selectionSlice,
        uint firstA,
        uint firstB,
        uint firstC,
        uint firstD,
        uint lastA,
        uint lastB,
        uint lastC,
        uint lastD)
    {
        ArgumentOutOfRangeException.ThrowIfZero(selectionSlice.Count);

        uint firstArgb = InterpolateBilinearArgb(
            firstA,
            firstB,
            firstC,
            firstD,
            selectionSlice.FirstSelection.XFraction,
            selectionSlice.FirstSelection.YFraction);
        uint lastArgb = InterpolateBilinearArgb(
            lastA,
            lastB,
            lastC,
            lastD,
            selectionSlice.LastSelection.XFraction,
            selectionSlice.LastSelection.YFraction);
        return new(selectionSlice.Count, firstArgb, lastArgb);
    }

    internal static Direct3D9SoftwareBilinearInteriorArgbSlice CreateInteriorArgbSlice(
        Direct3D9SoftwareBilinearLoopState loopState,
        uint bitmapWidth,
        uint bitmapHeight,
        MilBitmapWrapMode wrapMode,
        int uIncrement,
        int vIncrement,
        uint count,
        uint firstA,
        uint firstB,
        uint firstC,
        uint firstD,
        uint lastA,
        uint lastB,
        uint lastC,
        uint lastD)
    {
        Direct3D9SoftwareBilinearInteriorArgbBatch batch = CreateInteriorArgbBatch(
            loopState,
            bitmapWidth,
            bitmapHeight,
            wrapMode,
            uIncrement,
            vIncrement,
            count,
            firstA,
            firstB,
            firstC,
            firstD,
            lastA,
            lastB,
            lastC,
            lastD);
        return new(batch.Count, batch.FirstArgb, batch.LastArgb);
    }

    internal static Direct3D9SoftwareBilinearInteriorArgbBatch CreateInteriorArgbBatch(
        Direct3D9SoftwareBilinearLoopState loopState,
        uint bitmapWidth,
        uint bitmapHeight,
        MilBitmapWrapMode wrapMode,
        int uIncrement,
        int vIncrement,
        uint count,
        uint firstA,
        uint firstB,
        uint firstC,
        uint firstD,
        uint lastA,
        uint lastB,
        uint lastC,
        uint lastD)
    {
        Direct3D9SoftwareBilinearInteriorSampleSlice sampleSlice = CreateInteriorSampleSlice(
            loopState,
            bitmapWidth,
            bitmapHeight,
            wrapMode,
            uIncrement,
            vIncrement,
            count);
        Direct3D9SoftwareBilinearInteriorTexelSelectionSlice selectionSlice =
            CreateInteriorTexelSelectionSlice(sampleSlice, bitmapWidth, bitmapHeight, wrapMode);
        Direct3D9SoftwareBilinearInteriorArgbSlice argbSlice = AssembleInteriorArgbSlice(
            selectionSlice,
            firstA,
            firstB,
            firstC,
            firstD,
            lastA,
            lastB,
            lastC,
            lastD);
        uint remainingCount = count - sampleSlice.Count;
        return new(
            argbSlice.Count,
            argbSlice.FirstArgb,
            argbSlice.LastArgb,
            sampleSlice.AdvancedSample.U,
            sampleSlice.AdvancedSample.V,
            remainingCount,
            sampleSlice.AdvancedSample,
            ClassifyNextBranch(remainingCount, sampleSlice.AdvancedSample));
    }

    internal static Direct3D9SoftwareBilinearNextBranch ClassifyNextBranch(
        uint remainingCount,
        Direct3D9SoftwareBilinearLoopState nextLoopState)
    {
        if (remainingCount == 0)
        {
            return Direct3D9SoftwareBilinearNextBranch.Complete;
        }

        return nextLoopState.RequiresFallback
            ? Direct3D9SoftwareBilinearNextBranch.Fallback
            : Direct3D9SoftwareBilinearNextBranch.Interior;
    }

    internal static Direct3D9SoftwareBilinearFallbackRequestSlice CreateFallbackRequestSlice(
        int u,
        int v,
        uint bitmapWidth,
        uint bitmapHeight,
        MilBitmapWrapMode wrapMode,
        int uIncrement,
        int vIncrement,
        uint count,
        uint processedCount)
    {
        uint requestedCount;
        if (wrapMode is MilBitmapWrapMode.Extend or MilBitmapWrapMode.Border)
        {
            requestedCount = EstimateOutsideTextureCount(
                u,
                v,
                bitmapWidth,
                bitmapHeight,
                uIncrement,
                vIncrement,
                count);
        }
        else if (wrapMode is MilBitmapWrapMode.FlipX
                 or MilBitmapWrapMode.FlipY
                 or MilBitmapWrapMode.FlipXY
                 or MilBitmapWrapMode.Tile)
        {
            requestedCount = count;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(wrapMode));
        }

        ArgumentOutOfRangeException.ThrowIfZero(processedCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(processedCount, requestedCount);

        Direct3D9SoftwareBilinearFallbackBatchAdvance advance = AdvanceFallbackBatch(
            u,
            v,
            bitmapWidth,
            bitmapHeight,
            wrapMode,
            uIncrement,
            vIncrement,
            count,
            processedCount);
        if (wrapMode is MilBitmapWrapMode.FlipX
                or MilBitmapWrapMode.FlipY
                or MilBitmapWrapMode.FlipXY
                or MilBitmapWrapMode.Tile
            && advance.RemainingCount > 0
            && advance.NextBranch != Direct3D9SoftwareBilinearNextBranch.Interior)
        {
            throw new ArgumentOutOfRangeException(nameof(processedCount));
        }

        return new(
            requestedCount,
            advance.Count,
            advance.U,
            advance.V,
            advance.RemainingCount,
            advance.NextBranch);
    }

    internal static Direct3D9SoftwareBilinearFallbackBatchSlice CreateFallbackBatchSlice(
        int u,
        int v,
        uint bitmapWidth,
        uint bitmapHeight,
        MilBitmapWrapMode wrapMode,
        int uIncrement,
        int vIncrement,
        uint count,
        uint processedCount)
    {
        if (wrapMode is not MilBitmapWrapMode.Extend and not MilBitmapWrapMode.Border)
        {
            throw new ArgumentOutOfRangeException(nameof(wrapMode));
        }

        uint candidateCount = EstimateOutsideTextureCount(
            u,
            v,
            bitmapWidth,
            bitmapHeight,
            uIncrement,
            vIncrement,
            count);
        ArgumentOutOfRangeException.ThrowIfZero(processedCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(processedCount, candidateCount);

        Direct3D9SoftwareBilinearFallbackBatchAdvance advance = AdvanceFallbackBatch(
            u,
            v,
            bitmapWidth,
            bitmapHeight,
            wrapMode,
            uIncrement,
            vIncrement,
            count,
            processedCount);
        return new(
            candidateCount,
            advance.Count,
            advance.U,
            advance.V,
            advance.RemainingCount,
            advance.NextBranch);
    }

    internal static Direct3D9SoftwareBilinearFallbackBatchAdvance AdvanceFallbackBatch(
        int u,
        int v,
        uint bitmapWidth,
        uint bitmapHeight,
        MilBitmapWrapMode wrapMode,
        int uIncrement,
        int vIncrement,
        uint count,
        uint processedCount)
    {
        ArgumentOutOfRangeException.ThrowIfZero(bitmapWidth);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapHeight);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapWidth, AcceleratedTextureMaximum);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapHeight, AcceleratedTextureMaximum);
        ArgumentOutOfRangeException.ThrowIfZero(count);
        ArgumentOutOfRangeException.ThrowIfZero(processedCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(processedCount, count);

        int advancedU = checked((int) (u + (processedCount * (long) uIncrement)));
        int advancedV = checked((int) (v + (processedCount * (long) vIncrement)));
        uint remainingCount = count - processedCount;
        if (remainingCount == 0)
        {
            return new(
                processedCount,
                advancedU,
                advancedV,
                remainingCount,
                Direct3D9SoftwareBilinearNextBranch.Complete);
        }

        if (wrapMode is MilBitmapWrapMode.Extend or MilBitmapWrapMode.Border)
        {
            int uMaximum = checked((int) (bitmapWidth << 16));
            int vMaximum = checked((int) (bitmapHeight << 16));
            bool remainsOutside = advancedU < 0
                || advancedV < 0
                || advancedU >= uMaximum
                || advancedV >= vMaximum;
            return new(
                processedCount,
                advancedU,
                advancedV,
                remainingCount,
                remainsOutside
                    ? Direct3D9SoftwareBilinearNextBranch.Fallback
                    : Direct3D9SoftwareBilinearNextBranch.Interior);
        }

        Direct3D9SoftwareBilinearLoopState nextLoopState = ClassifyCanonicalLoopState(
            advancedU,
            advancedV,
            bitmapWidth,
            bitmapHeight,
            wrapMode);
        return new(
            processedCount,
            nextLoopState.U,
            nextLoopState.V,
            remainingCount,
            nextLoopState.RequiresFallback
                ? Direct3D9SoftwareBilinearNextBranch.Fallback
                : Direct3D9SoftwareBilinearNextBranch.Interior);
    }

    private static uint EstimateOutsideTextureCount(
        int u,
        int v,
        uint bitmapWidth,
        uint bitmapHeight,
        int uIncrement,
        int vIncrement,
        uint count)
    {
        ArgumentOutOfRangeException.ThrowIfZero(bitmapWidth);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapHeight);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapWidth, AcceleratedTextureMaximum);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmapHeight, AcceleratedTextureMaximum);
        ArgumentOutOfRangeException.ThrowIfZero(count);

        int uMaximum = checked((int) ((bitmapWidth - 1) << 16));
        int vMaximum = checked((int) ((bitmapHeight - 1) << 16));
        if (u >= 0 && u < uMaximum && v >= 0 && v < vMaximum)
        {
            throw new ArgumentOutOfRangeException(nameof(u));
        }

        uint uLength = GetDistanceFromTexture(u, uMaximum, uIncrement);
        uint vLength = GetDistanceFromTexture(v, vMaximum, vIncrement);
        return Math.Min(count, Math.Min(uLength, vLength));
    }

    private static uint GetDistanceFromTexture(int start, int maximum, int increment)
    {
        if (start < 0 && increment > 0)
        {
            return checked((uint) (1 + ((-1L - start) / increment)));
        }

        if (start >= maximum && increment < 0)
        {
            return checked((uint) (1 + (((long) maximum - start) / increment)));
        }

        return int.MaxValue;
    }

    private static (int Minimum, int Maximum) SelectInteriorDimensionBounds(
        Direct3D9SoftwareBilinearInteriorRegion region,
        uint bitmapSize)
    {
        return region switch
        {
            Direct3D9SoftwareBilinearInteriorRegion.Forward =>
                (0, checked((int) ((bitmapSize - 1) << 16))),
            Direct3D9SoftwareBilinearInteriorRegion.Mirrored =>
                (checked((int) (bitmapSize << 16)), checked((int) ((2 * bitmapSize - 1) << 16))),
            _ => throw new ArgumentOutOfRangeException(nameof(region)),
        };
    }

    private static uint CalculateInteriorDimensionLength(int start, int increment, int minimum, int maximum)
    {
        if (increment > 0)
        {
            return checked((uint) (1 + (((long) maximum - 1 - start) / increment)));
        }

        if (increment < 0)
        {
            return checked((uint) (1 + (((long) minimum - start) / increment)));
        }

        return uint.MaxValue;
    }

    internal static Direct3D9SoftwareBilinearAdvance Advance64BitFallbackTilePosition(
        long u,
        long v,
        int uIncrement,
        int vIncrement,
        uint bitmapWidth,
        uint bitmapHeight,
        MilBitmapWrapMode wrapMode)
    {
        if (wrapMode is MilBitmapWrapMode.Extend or MilBitmapWrapMode.Border)
        {
            throw new ArgumentOutOfRangeException(nameof(wrapMode));
        }

        ArgumentOutOfRangeException.ThrowIfZero(bitmapWidth);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapHeight);

        uint canonicalWidth = wrapMode is MilBitmapWrapMode.FlipX or MilBitmapWrapMode.FlipXY
            ? checked(bitmapWidth * 2)
            : bitmapWidth;
        uint canonicalHeight = wrapMode is MilBitmapWrapMode.FlipY or MilBitmapWrapMode.FlipXY
            ? checked(bitmapHeight * 2)
            : bitmapHeight;
        long advancedU = WrapCanonicalFixed16(checked(u + uIncrement), canonicalWidth);
        long advancedV = WrapCanonicalFixed16(checked(v + vIncrement), canonicalHeight);
        return new(
            advancedU,
            advancedV,
            IsOnTileBorder(advancedU, advancedV, bitmapWidth, bitmapHeight, canonicalWidth, canonicalHeight));
    }

    private static int ResolveTileCoordinate(int coordinate, int size, int canonicalSize)
    {
        int result = WrapCanonicalInteger(coordinate, canonicalSize);
        return (uint) result >= (uint) size ? canonicalSize - 1 - result : result;
    }

    private static int WrapCanonicalInteger(int coordinate, int modulus)
    {
        int remainder = coordinate % modulus;
        return remainder < 0 ? remainder + modulus : remainder;
    }

    private static long WrapCanonicalFixed16(long coordinate, uint canonicalSize)
    {
        long modulus = checked((long) canonicalSize << 16);
        long remainder = coordinate % modulus;
        return remainder < 0 ? remainder + modulus : remainder;
    }

    private static bool IsOnTileBorder(
        long u,
        long v,
        uint bitmapWidth,
        uint bitmapHeight,
        uint canonicalWidth,
        uint canonicalHeight)
    {
        long inTileUMax = checked((long) (bitmapWidth - 1) << 16);
        long inTileVMax = checked((long) (bitmapHeight - 1) << 16);
        long flipTileUMin = checked((long) bitmapWidth << 16);
        long flipTileVMin = checked((long) bitmapHeight << 16);
        long inflipTileUMax = checked(((long) canonicalWidth - 1) << 16);
        long inflipTileVMax = checked(((long) canonicalHeight - 1) << 16);

        return !((u < inflipTileUMax)
            && (v < inflipTileVMax)
            && (u >= flipTileUMin || u < inTileUMax)
            && (v >= flipTileVMin || v < inTileVMax));
    }

    private static bool IsOutsideAcceleratedSpan(long coordinate) =>
        coordinate > AcceleratedSpanMaximum || coordinate < -AcceleratedSpanMaximum;

    internal static uint Assemble64BitFallbackArgb(
        Direct3D9SoftwareBilinearTexelSelection selection,
        uint a,
        uint b,
        uint c,
        uint d,
        uint borderArgb,
        MilBitmapWrapMode wrapMode)
    {
        if (wrapMode == MilBitmapWrapMode.Extend)
        {
            if (selection.X1 == selection.X2)
            {
                return InterpolateLinearArgb(a, c, selection.YFraction);
            }

            if (selection.Y1 == selection.Y2)
            {
                return InterpolateLinearArgb(a, b, selection.XFraction);
            }
        }
        else if (wrapMode == MilBitmapWrapMode.Border)
        {
            if (!selection.AInside && !selection.BInside && !selection.CInside && !selection.DInside)
            {
                return borderArgb;
            }

            a = selection.AInside ? a : borderArgb;
            b = selection.BInside ? b : borderArgb;
            c = selection.CInside ? c : borderArgb;
            d = selection.DInside ? d : borderArgb;
        }

        return InterpolateBilinearArgb(a, b, c, d, selection.XFraction, selection.YFraction);
    }

    internal static uint InterpolateLinearArgb(uint first, uint second, int fraction)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fraction);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fraction, 0xFF);

        return PackArgb(
            InterpolateLinearChannel((byte) (first >> 24), (byte) (second >> 24), fraction),
            InterpolateLinearChannel((byte) (first >> 16), (byte) (second >> 16), fraction),
            InterpolateLinearChannel((byte) (first >> 8), (byte) (second >> 8), fraction),
            InterpolateLinearChannel((byte) first, (byte) second, fraction));
    }

    internal static uint InterpolateBilinearArgb(
        uint a,
        uint b,
        uint c,
        uint d,
        int xFraction,
        int yFraction)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(xFraction);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(xFraction, 0xFF);
        ArgumentOutOfRangeException.ThrowIfNegative(yFraction);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(yFraction, 0xFF);

        return PackArgb(
            InterpolateBilinearChannel((byte) (a >> 24), (byte) (b >> 24), (byte) (c >> 24), (byte) (d >> 24), xFraction, yFraction),
            InterpolateBilinearChannel((byte) (a >> 16), (byte) (b >> 16), (byte) (c >> 16), (byte) (d >> 16), xFraction, yFraction),
            InterpolateBilinearChannel((byte) (a >> 8), (byte) (b >> 8), (byte) (c >> 8), (byte) (d >> 8), xFraction, yFraction),
            InterpolateBilinearChannel((byte) a, (byte) b, (byte) c, (byte) d, xFraction, yFraction));
    }

    private static byte InterpolateLinearChannel(int first, int second, int fraction) =>
        (byte) (((first << 8) + ((second - first) * fraction) + 0x80) >> 8);

    private static byte InterpolateBilinearChannel(
        int a,
        int b,
        int c,
        int d,
        int xFraction,
        int yFraction)
    {
        int top = (a << 8) + ((b - a) * xFraction);
        int bottom = (c << 8) + ((d - c) * xFraction);
        return (byte) ((((0x100 - yFraction) * top) + (yFraction * bottom) + 0x8000) >> 16);
    }

    private static uint PackArgb(byte alpha, byte red, byte green, byte blue) =>
        ((uint) alpha << 24) | ((uint) red << 16) | ((uint) green << 8) | blue;

    internal static bool CanUseMmx(uint bitmapWidth, uint bitmapHeight, MilBitmapWrapMode wrapMode)
    {
        uint maximumWidth = wrapMode is MilBitmapWrapMode.FlipX or MilBitmapWrapMode.FlipXY
            ? Fixed16IntegerMaximum / 2
            : Fixed16IntegerMaximum;
        uint maximumHeight = wrapMode is MilBitmapWrapMode.FlipY or MilBitmapWrapMode.FlipXY
            ? Fixed16IntegerMaximum / 2
            : Fixed16IntegerMaximum;

        return bitmapWidth <= maximumWidth && bitmapHeight <= maximumHeight;
    }

    internal static Direct3D9SoftwareBilinearFixedPointState CreateMmxFixedPointState(
        Matrix3x2 deviceToTexture,
        uint bitmapWidth,
        uint bitmapHeight,
        int stride,
        MilBitmapWrapMode wrapMode)
    {
        if (!CanUseMmx(bitmapWidth, bitmapHeight, wrapMode))
        {
            throw new ArgumentOutOfRangeException(nameof(bitmapWidth));
        }

        int m11 = Round(deviceToTexture.M11 * Fixed16Scale);
        int m12 = Round(deviceToTexture.M12 * Fixed16Scale);
        int m21 = Round(deviceToTexture.M21 * Fixed16Scale);
        int m22 = Round(deviceToTexture.M22 * Fixed16Scale);
        int dx = Round(deviceToTexture.M31 * Fixed16Scale);
        int dy = Round(deviceToTexture.M32 * Fixed16Scale);
        int xDeviceOffset = 0;
        int yDeviceOffset = 0;

        if (IsOverflowSentinel(dx) || IsOverflowSentinel(dy))
        {
            SetDeviceOffset(
                deviceToTexture,
                ref dx,
                ref dy,
                out xDeviceOffset,
                out yDeviceOffset);
        }

        int modulusWidth = checked((int) bitmapWidth << 16);
        int modulusHeight = checked((int) bitmapHeight << 16);
        int xEdgeIncrement = unchecked(4 * (1 - (int) bitmapWidth));
        int yEdgeIncrement = unchecked(-(int) (bitmapHeight - 1) * stride);

        if (wrapMode is MilBitmapWrapMode.FlipX or MilBitmapWrapMode.FlipXY)
        {
            modulusWidth = checked(modulusWidth * 2);
            xEdgeIncrement = 0;
        }

        if (wrapMode is MilBitmapWrapMode.FlipY or MilBitmapWrapMode.FlipXY)
        {
            modulusHeight = checked(modulusHeight * 2);
            yEdgeIncrement = 0;
        }

        return new(
            m11,
            m12,
            m21,
            m22,
            dx,
            dy,
            m11,
            m12,
            modulusWidth,
            modulusHeight,
            xEdgeIncrement,
            yEdgeIncrement,
            xDeviceOffset,
            yDeviceOffset);
    }

    private static void SetDeviceOffset(
        Matrix3x2 deviceToTexture,
        ref int dx,
        ref int dy,
        out int xDeviceOffset,
        out int yDeviceOffset)
    {
        xDeviceOffset = 0;
        yDeviceOffset = 0;

        if (!Matrix3x2.Invert(deviceToTexture, out Matrix3x2 textureToDevice))
        {
            return;
        }

        xDeviceOffset = Round(textureToDevice.M31);
        yDeviceOffset = Round(textureToDevice.M32);
        dx = Round(((xDeviceOffset * deviceToTexture.M11) + (yDeviceOffset * deviceToTexture.M21) + deviceToTexture.M31) * Fixed16Scale);
        dy = Round(((xDeviceOffset * deviceToTexture.M12) + (yDeviceOffset * deviceToTexture.M22) + deviceToTexture.M32) * Fixed16Scale);
    }

    private static bool IsOverflowSentinel(int value) => value is int.MinValue or int.MaxValue;

    private static int Round(float value)
    {
        if (float.IsNaN(value) || value <= int.MinValue)
        {
            return int.MinValue;
        }

        if (value >= int.MaxValue)
        {
            return int.MaxValue;
        }

        return checked((int) MathF.Floor(value + 0.5f));
    }
}
