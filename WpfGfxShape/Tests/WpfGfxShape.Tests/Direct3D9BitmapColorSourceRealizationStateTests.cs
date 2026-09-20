using System.Numerics;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapColorSourceRealizationStateTests
{
    [TestMethod]
    public void WhenBitmapUniquenessMatchesThenRealizationIsCurrent()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 11);
        state.Commit(11, new Direct3D9BitmapRealizationRectangle(0, 0, 40, 30));

        Assert.IsTrue(state.IsRealizationCurrent());
    }

    [TestMethod]
    public void WhenBitmapUniquenessChangesThenRealizationIsNotCurrent()
    {
        uint uniqueness = 11;
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, () => uniqueness);
        state.Commit(11, new Direct3D9BitmapRealizationRectangle(0, 0, 40, 30));
        uniqueness = 12;

        Assert.IsFalse(state.IsRealizationCurrent());
    }

    [TestMethod]
    public void WhenNoBitmapIsAssociatedThenRealizationIsAlwaysCurrent()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 99);
        state.Commit(1, new Direct3D9BitmapRealizationRectangle(0, 0, 40, 30));

        Assert.IsTrue(state.IsRealizationCurrent());
    }

    [TestMethod]
    public void WhenCachedBoundsContainRequiredBoundsAndUniquenessMatchesThenRealizationIsValid()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 11);
        state.SetRequiredRealizationBounds(new Direct3D9BitmapRealizationRectangle(10, 5, 30, 25));
        state.Commit(11, new Direct3D9BitmapRealizationRectangle(0, 0, 40, 30));

        Assert.IsTrue(state.IsRealizationValid());
    }

    [TestMethod]
    public void WhenCachedBoundsDoNotContainRequiredBoundsThenRealizationIsInvalid()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 11);
        state.SetRequiredRealizationBounds(new Direct3D9BitmapRealizationRectangle(10, 5, 30, 25));
        state.Commit(11, new Direct3D9BitmapRealizationRectangle(10, 5, 29, 25));

        Assert.IsFalse(state.IsRealizationValid());
    }

    [TestMethod]
    public void WhenContextParametersAreSetThenNoRefBitmapSourceAndLayoutAreCaptured()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();

        state.SetBitmapAndContextCacheParameters(23, properties);

        Assert.AreEqual(
            ((nint) 23, properties.SourceContained),
            (state.BitmapSource, state.PrefilteredBitmap));
    }

    [TestMethod]
    public void WhenContextParametersAreSetThenPrefilterDimensionsAndSourceContainedAreCaptured()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();

        state.SetBitmapAndContextCacheParameters(23, properties);

        Assert.AreEqual(
            (properties.Width, properties.Height, properties.SourceContained),
            (state.PrefilterWidth, state.PrefilterHeight, state.PrefilteredBitmap));
    }

    [TestMethod]
    public void WhenContextParametersAreSetThenTexelLayoutsAndWrapAddressesAreCaptured()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties() with
        {
            LayoutU = new Direct3D9BitmapDimensionLayout(40, Direct3D9TexelLayout.EdgeWrapped, Textureaddress.Wrap),
            LayoutV = new Direct3D9BitmapDimensionLayout(30, Direct3D9TexelLayout.EdgeMirrored, Textureaddress.Mirror)
        };

        state.SetBitmapAndContextCacheParameters(23, properties);

        Assert.AreEqual(
            (properties.LayoutU.TexelLayout, properties.LayoutV.TexelLayout, properties.LayoutU.TextureAddress, properties.LayoutV.TextureAddress),
            (state.TexelLayoutU, state.TexelLayoutV, state.TextureAddressU, state.TextureAddressV));
    }

    [TestMethod]
    public void WhenContextParametersAreSetAgainThenCacheParameterStateIsReplaced()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties() with
        {
            Width = 32,
            Height = 24,
            SourceContained = new Direct3D9BitmapRealizationRectangle(3, 4, 35, 28),
            LayoutU = new Direct3D9BitmapDimensionLayout(34, Direct3D9TexelLayout.EdgeWrapped, Textureaddress.Wrap),
            LayoutV = new Direct3D9BitmapDimensionLayout(26, Direct3D9TexelLayout.EdgeMirrored, Textureaddress.Mirror)
        };

        state.SetBitmapAndContextCacheParameters(29, properties);

        Assert.AreEqual(
            ((nint) 29, properties.Width, properties.Height, properties.SourceContained, properties.LayoutU.TexelLayout, properties.LayoutV.TexelLayout, properties.LayoutU.TextureAddress, properties.LayoutV.TextureAddress),
            (state.BitmapSource, state.PrefilterWidth, state.PrefilterHeight, state.PrefilteredBitmap, state.TexelLayoutU, state.TexelLayoutV, state.TextureAddressU, state.TextureAddressV));
    }

    [TestMethod]
    public void WhenBitmapAndContextAreSetThenRequiredBoundsAreInitializedFromPrefilteredBitmap()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties() with
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(3, 4, 35, 28)
        };

        state.SetBitmapAndContext(23, properties);

        Assert.AreEqual(properties.SourceContained, state.RequiredRealizationBounds);
    }

    [TestMethod]
    public void WhenBitmapAndContextAreSetAgainThenRequiredBoundsAreReinitializedFromNewPrefilteredBitmap()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        state.SetRequiredRealizationBounds(new Direct3D9BitmapRealizationRectangle(10, 8, 20, 18));
        Direct3D9BitmapRealizationProperties properties = CreateProperties() with
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(5, 6, 37, 30)
        };

        state.SetBitmapAndContext(29, properties);

        Assert.AreEqual(properties.SourceContained, state.RequiredRealizationBounds);
    }

    [TestMethod]
    public void WhenDeviceBitmapHasNoReusableSourceThenMinimumBoundsAreComputedBeforeSourcesAreConsumed()
    {
        ReusableContextHarness harness = new();
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(properties);

        state.SetBitmapAndContext(
            23,
            properties,
            minimumRealizationBoundsComputed: false,
            isDeviceBitmap: true,
            hasContributorFromDifferentAdapter: false,
            realizationBounds: 31,
            candidates,
            sources,
            (nint bounds, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle minimumBounds) =>
            {
                harness.Record($"compute:{bounds}");
                minimumBounds = new Direct3D9BitmapRealizationRectangle(10, 8, 30, 24);
                return true;
            });

        Assert.AreEqual(
            (new Direct3D9BitmapRealizationRectangle(10, 8, 30, 24), "compute:31", 0),
            (state.RequiredRealizationBounds, harness.Calls, candidates.Head));
    }

    [TestMethod]
    public void WhenSameAdapterReusableSourceExistsThenMinimumBoundsAreNotComputedAndSourceIsConsumed()
    {
        ReusableContextHarness harness = new();
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(41);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(properties);
        harness.ClearCalls();

        state.SetBitmapAndContext(
            23,
            properties,
            minimumRealizationBoundsComputed: false,
            isDeviceBitmap: true,
            hasContributorFromDifferentAdapter: false,
            realizationBounds: 31,
            candidates,
            sources,
            (nint _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle _) =>
                throw new AssertFailedException());

        Assert.AreEqual(
            (properties.SourceContained, 41, 0, "set-next:41:0,set-next:41:0,add:41,release:41"),
            (state.RequiredRealizationBounds, sources.Head, candidates.Head, harness.Calls));
    }

    [TestMethod]
    public void WhenReusableSourceHasCrossAdapterContributorThenMinimumBoundsAreComputedBeforeConsumption()
    {
        ReusableContextHarness harness = new();
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(41);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(properties);
        harness.ClearCalls();

        state.SetBitmapAndContext(
            23,
            properties,
            minimumRealizationBoundsComputed: false,
            isDeviceBitmap: true,
            hasContributorFromDifferentAdapter: true,
            realizationBounds: 31,
            candidates,
            sources,
            (nint _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle minimumBounds) =>
            {
                harness.Record("compute");
                minimumBounds = new Direct3D9BitmapRealizationRectangle(10, 8, 30, 24);
                return true;
            });

        Assert.AreEqual(
            (new Direct3D9BitmapRealizationRectangle(10, 8, 30, 24), 41, "compute,set-next:41:0,set-next:41:0,add:41,release:41"),
            (state.RequiredRealizationBounds, sources.Head, harness.Calls));
    }

    [TestMethod]
    public void WhenMinimumBoundsWerePrecomputedThenPrefilteredBoundsArePreservedAndSourcesAreConsumed()
    {
        ReusableContextHarness harness = new();
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties() with
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(10, 8, 30, 24)
        };
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(41);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(properties);
        harness.ClearCalls();

        state.SetBitmapAndContext(
            23,
            properties,
            minimumRealizationBoundsComputed: true,
            isDeviceBitmap: true,
            hasContributorFromDifferentAdapter: true,
            realizationBounds: 31,
            candidates,
            sources,
            (nint _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle _) =>
                throw new AssertFailedException());

        Assert.AreEqual(
            (properties.SourceContained, 41, 0),
            (state.RequiredRealizationBounds, sources.Head, candidates.Head));
    }

    [TestMethod]
    public void WhenDelayedBoundsContextHasSameAdapterReusableSourceThenMinimumBoundsAreNotComputed()
    {
        ReusableContextHarness harness = new();
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        Direct3D9DelayedBounds delayedBounds = new();
        delayedBounds.SetBoundsRectAndInverseTransform(
            new MilRectF(24, 16, 52, 36),
            new Matrix3x2(2, 0, 0, 2, 0, 0));
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(41);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(properties);
        harness.ClearCalls();

        int result = state.SetBitmapAndContext(
            23,
            properties,
            isDeviceBitmap: true,
            hasContributorFromDifferentAdapter: false,
            delayedBounds,
            candidates,
            sources,
            Matrix3x2.Identity);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, properties.SourceContained, 41, 0),
            (result, state.RequiredRealizationBounds, sources.Head, candidates.Head));
    }

    [TestMethod]
    public void WhenDelayedBoundsContextHasCrossAdapterContributorThenMinimumBoundsAreComputedBeforeConsumption()
    {
        ReusableContextHarness harness = new();
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties() with
        {
            InterpolationMode = MilBitmapInterpolationMode.NearestNeighbor
        };
        Direct3D9DelayedBounds delayedBounds = new();
        delayedBounds.SetBoundsRectAndInverseTransform(
            new MilRectF(24, 16, 52, 36),
            new Matrix3x2(2, 0, 0, 2, 0, 0));
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(41);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(properties);
        harness.ClearCalls();

        int result = state.SetBitmapAndContext(
            23,
            properties,
            isDeviceBitmap: true,
            hasContributorFromDifferentAdapter: true,
            delayedBounds,
            candidates,
            sources,
            Matrix3x2.Identity);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, new Direct3D9BitmapRealizationRectangle(6, 4, 15, 12), 41, 0),
            (result, state.RequiredRealizationBounds, sources.Head, candidates.Head));
    }

    [TestMethod]
    public void WhenDelayedBoundsContextTransformFailsThenConsumedSourcesAndMinimumBoundsArePreserved()
    {
        ReusableContextHarness harness = new();
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        Direct3D9DelayedBounds delayedBounds = new();
        delayedBounds.SetBoundsRectAndInverseTransform(
            new MilRectF(24, 16, 52, 36),
            new Matrix3x2(2, 0, 0, 2, 0, 0));
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(41);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(properties);

        int result = state.SetBitmapAndContext(
            23,
            properties,
            isDeviceBitmap: true,
            hasContributorFromDifferentAdapter: true,
            delayedBounds,
            candidates,
            sources,
            new Matrix3x2(0, 0, 0, 1, 0, 0));

        Assert.AreEqual(
            (Direct3D9Factory.NonInvertibleMatrixHResult, new Direct3D9BitmapRealizationRectangle(6, 4, 16, 12), 41, 0),
            (result, state.RequiredRealizationBounds, sources.Head, candidates.Head));
    }

    [TestMethod]
    public void WhenFullBitmapContextIsSetWithTransformThenReusableSourcesAreConsumedBeforeTransform()
    {
        ReusableContextHarness harness = new();
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties() with
        {
            InterpolationMode = MilBitmapInterpolationMode.NearestNeighbor
        };
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(41);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(properties);
        harness.ClearCalls();

        int result = state.SetBitmapAndContext(
            23,
            properties,
            minimumRealizationBoundsComputed: false,
            isDeviceBitmap: true,
            hasContributorFromDifferentAdapter: true,
            realizationBounds: 31,
            candidates,
            sources,
            (nint _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle minimumBounds) =>
            {
                harness.Record("compute");
                minimumBounds = new Direct3D9BitmapRealizationRectangle(10, 8, 30, 24);
                return true;
            },
            new Matrix3x2(2, 0, 0, 4, 10, 20));

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, new Direct3D9BitmapRealizationRectangle(10, 8, 30, 24), 41, MilBitmapInterpolationMode.NearestNeighbor, "compute,set-next:41:0,set-next:41:0,add:41,release:41"),
            (result, state.RequiredRealizationBounds, sources.Head, state.InterpolationMode, harness.Calls));
    }

    [TestMethod]
    public void WhenFullBitmapContextTransformFailsThenReusableSourcesRemainConsumed()
    {
        ReusableContextHarness harness = new();
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(41);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources(properties);
        harness.ClearCalls();

        int result = state.SetBitmapAndContext(
            23,
            properties,
            minimumRealizationBoundsComputed: true,
            isDeviceBitmap: true,
            hasContributorFromDifferentAdapter: false,
            realizationBounds: 31,
            candidates,
            sources,
            (nint _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle _) =>
                throw new AssertFailedException(),
            new Matrix3x2(0, 0, 0, 1, 0, 0));

        Assert.AreEqual(
            (Direct3D9Factory.NonInvertibleMatrixHResult, properties.SourceContained, 41, 0, "set-next:41:0,set-next:41:0,add:41,release:41"),
            (result, state.RequiredRealizationBounds, sources.Head, candidates.Head, harness.Calls));
    }

    [TestMethod]
    public void WhenBitmapContextIsSetWithTransformThenProductionContextStateIsCaptured()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties() with
        {
            InterpolationMode = MilBitmapInterpolationMode.NearestNeighbor
        };

        int result = state.SetBitmapAndContext(
            23,
            properties,
            new Matrix3x2(2, 0, 0, 4, 10, 20));

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, (nint) 23, properties.SourceContained, MilBitmapInterpolationMode.NearestNeighbor, new Matrix3x2(0.00625f, 0, 0, 1f / 240f, -0.1875f, -19f / 60f)),
            (result, state.BitmapSource, state.RequiredRealizationBounds, state.InterpolationMode, state.XSpaceToTextureUv));
    }

    [TestMethod]
    public void WhenProductionDeviceBitmapContextUsesDelayedBoundsThenMinimumBoundsAreComputedBeforeTransform()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties() with
        {
            InterpolationMode = MilBitmapInterpolationMode.NearestNeighbor
        };
        Direct3D9DelayedBounds delayedBounds = new();
        delayedBounds.SetBoundsRectAndInverseTransform(
            new MilRectF(20, 16, 60, 48),
            new Matrix3x2(2, 0, 0, 2, 0, 0));

        int result = state.SetBitmapAndContext(
            23,
            properties,
            isDeviceBitmap: true,
            delayedBounds,
            new Matrix3x2(2, 0, 0, 4, 10, 20));

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, new Direct3D9BitmapRealizationRectangle(5, 4, 17, 15), MilBitmapInterpolationMode.NearestNeighbor),
            (result, state.RequiredRealizationBounds, state.InterpolationMode));
    }

    [TestMethod]
    public void WhenProductionNonDeviceBitmapContextUsesDelayedBoundsThenFullBoundsArePreserved()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        Direct3D9DelayedBounds delayedBounds = new();

        int result = state.SetBitmapAndContext(
            23,
            properties,
            isDeviceBitmap: false,
            delayedBounds,
            Matrix3x2.Identity);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, properties.SourceContained),
            (result, state.RequiredRealizationBounds));
    }

    [TestMethod]
    public void WhenProductionDeviceBitmapDelayedBoundsCannotBeComputedThenFullBoundsArePreserved()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        Direct3D9DelayedBounds delayedBounds = new();

        int result = state.SetBitmapAndContext(
            23,
            properties,
            isDeviceBitmap: true,
            delayedBounds,
            Matrix3x2.Identity);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, properties.SourceContained),
            (result, state.RequiredRealizationBounds));
    }

    [TestMethod]
    public void WhenBitmapContextTransformIsNotInvertibleThenTransformFailureIsReturnedAfterContextCapture()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();

        int result = state.SetBitmapAndContext(
            23,
            properties,
            new Matrix3x2(0, 0, 0, 1, 0, 0));

        Assert.AreEqual(
            (Direct3D9Factory.NonInvertibleMatrixHResult, (nint) 23, properties.SourceContained),
            (result, state.BitmapSource, state.RequiredRealizationBounds));
    }

    [TestMethod]
    public void WhenFilterModeAndTextureTransformAreSetThenContextStateIsCaptured()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        state.SetBitmapAndContextCacheParameters(23, CreateProperties());

        int result = state.SetFilterModeAndTextureTransform(
            MilBitmapInterpolationMode.NearestNeighbor,
            new Matrix3x2(2, 0, 0, 4, 10, 20));

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, MilBitmapInterpolationMode.NearestNeighbor, new Matrix3x2(0.00625f, 0, 0, 1f / 240f, -0.1875f, -19f / 60f)),
            (result, state.InterpolationMode, state.XSpaceToTextureUv));
    }

    [TestMethod]
    public void WhenTextureTransformIsNotInvertibleThenFilterAndTransformStateArePreserved()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        state.SetBitmapAndContextCacheParameters(23, CreateProperties());
        state.SetFilterModeAndTextureTransform(MilBitmapInterpolationMode.Linear, Matrix3x2.Identity);
        Matrix3x2 previousTransform = state.XSpaceToTextureUv;

        int result = state.SetFilterModeAndTextureTransform(
            MilBitmapInterpolationMode.NearestNeighbor,
            new Matrix3x2(0, 0, 0, 1, 0, 0));

        Assert.AreEqual(
            (Direct3D9Factory.NonInvertibleMatrixHResult, MilBitmapInterpolationMode.Linear, previousTransform),
            (result, state.InterpolationMode, state.XSpaceToTextureUv));
    }

    [TestMethod]
    public void WhenTextureFormatChangesThenContextSetupFails()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationProperties properties = CreateProperties() with
        {
            TextureFormat = MilPixelFormat.Bgra32Bpp
        };

        Assert.ThrowsExactly<InvalidOperationException>(
            () => state.SetBitmapAndContextCacheParameters(23, properties));
    }

    [TestMethod]
    public void WhenRequiredBoundsCoverFullPrefilteredDimensionsThenMinimumBoundsAreNotComputed()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        state.SetRequiredRealizationBounds(new Direct3D9BitmapRealizationRectangle(5, 7, 45, 37));
        bool computed = false;

        bool result = state.CheckRequiredRealizationBounds(
            17,
            MilBitmapInterpolationMode.Linear,
            MilBitmapWrapMode.Extend,
            Direct3D9BitmapRequiredBoundsCheck.Required,
            (nint _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle _) =>
            {
                computed = true;
                return false;
            });

        Assert.AreEqual((true, false), (result, computed));
    }

    [TestMethod]
    public void WhenCachedBoundsContainComputedBoundsThenCachedCheckSucceeds()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        state.Commit(0, new Direct3D9BitmapRealizationRectangle(10, 5, 30, 25));

        bool result = state.CheckRequiredRealizationBounds(
            17,
            MilBitmapInterpolationMode.Linear,
            MilBitmapWrapMode.Extend,
            Direct3D9BitmapRequiredBoundsCheck.Cached,
            static (nint _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle bounds) =>
            {
                bounds = new Direct3D9BitmapRealizationRectangle(12, 8, 28, 20);
                return true;
            });

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void WhenPrefilteredBitmapCanContainComputedBoundsThenRequiredBoundsAreUpdated()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationRectangle expected = new(12, 8, 28, 20);

        bool result = state.CheckRequiredRealizationBounds(
            17,
            MilBitmapInterpolationMode.Linear,
            MilBitmapWrapMode.Extend,
            Direct3D9BitmapRequiredBoundsCheck.PossibleAndUpdateRequired,
            (nint _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle bounds) =>
            {
                bounds = expected;
                return true;
            });

        Assert.AreEqual((true, expected), (result, state.RequiredRealizationBounds));
    }

    [TestMethod]
    public void WhenMinimumBoundsCannotBeComputedThenRequiredBoundsAreNotUpdated()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationRectangle original = new(2, 3, 4, 5);
        state.SetRequiredRealizationBounds(original);

        bool result = state.CheckRequiredRealizationBounds(
            17,
            MilBitmapInterpolationMode.Linear,
            MilBitmapWrapMode.Extend,
            Direct3D9BitmapRequiredBoundsCheck.PossibleAndUpdateRequired,
            static (nint _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle _) => false);

        Assert.AreEqual((false, original), (result, state.RequiredRealizationBounds));
    }

    [TestMethod]
    public void WhenDelayedBoundsFitCachedRealizationThenCachedCheckSucceeds()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        state.Commit(0, new Direct3D9BitmapRealizationRectangle(5, 3, 14, 10));
        Direct3D9DelayedBounds delayedBounds = new();
        delayedBounds.SetBoundsRectAndInverseTransform(
            new MilRectF(24, 16, 52, 36),
            new Matrix3x2(2, 0, 0, 2, 0, 0));

        bool result = state.CheckRequiredRealizationBounds(
            delayedBounds,
            MilBitmapInterpolationMode.NearestNeighbor,
            MilBitmapWrapMode.Extend,
            Direct3D9BitmapRequiredBoundsCheck.Cached);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void WhenDelayedBoundsCanFitPrefilteredBitmapThenPossibleCheckUpdatesRequiredBounds()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9DelayedBounds delayedBounds = new();
        delayedBounds.SetBoundsRectAndInverseTransform(
            new MilRectF(32, 32, 52, 48),
            new Matrix3x2(2, 0, 0, 2, 0, 0));

        bool result = state.CheckRequiredRealizationBounds(
            delayedBounds,
            MilBitmapInterpolationMode.NearestNeighbor,
            MilBitmapWrapMode.Extend,
            Direct3D9BitmapRequiredBoundsCheck.PossibleAndUpdateRequired);

        Assert.AreEqual(
            (true, new Direct3D9BitmapRealizationRectangle(7, 7, 14, 13)),
            (result, state.RequiredRealizationBounds));
    }

    [TestMethod]
    public void WhenDelayedBoundsAreNotInvertibleThenRequiredBoundsAreNotUpdated()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationRectangle original = new(2, 3, 4, 5);
        state.SetRequiredRealizationBounds(original);
        Direct3D9DelayedBounds delayedBounds = new();
        delayedBounds.SetBoundsRectAndInverseTransform(
            new MilRectF(24, 16, 52, 36),
            new Matrix3x2(0, 0, 0, 1, 0, 0));

        bool result = state.CheckRequiredRealizationBounds(
            delayedBounds,
            MilBitmapInterpolationMode.NearestNeighbor,
            MilBitmapWrapMode.Extend,
            Direct3D9BitmapRequiredBoundsCheck.PossibleAndUpdateRequired);

        Assert.AreEqual((false, original), (result, state.RequiredRealizationBounds));
    }

    [TestMethod]
    public void WhenNonDeviceBitmapValidSourceRectanglesAreRequestedThenRequiredBoundsAreReturned()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationRectangle required = new(2, 3, 20, 25);
        state.SetRequiredRealizationBounds(required);
        int providerCalls = 0;

        int result = state.GetValidSourceRectangles(
            isDeviceBitmap: false,
            (nint _, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
            {
                providerCalls++;
                rectangles = [];
                return Direct3D9Factory.GenericFailureHResult;
            },
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, required, 0),
            (result, rectangles.Single(), providerCalls));
    }

    [TestMethod]
    public void WhenDeviceBitmapValidSourceRectanglesAreRequestedThenDeviceRectanglesAreBorrowed()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        IReadOnlyList<Direct3D9BitmapRealizationRectangle> expected =
        [
            new(1, 2, 10, 12),
            new(14, 3, 20, 18)
        ];

        int result = state.GetValidSourceRectangles(
            isDeviceBitmap: true,
            (nint bitmap, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
            {
                Assert.AreEqual((nint) 7, bitmap);
                rectangles = expected;
                return Direct3D9Factory.SuccessHResult;
            },
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, expected),
            (result, rectangles));
    }

    [TestMethod]
    public void WhenDeviceBitmapValidSourceRectangleLookupFailsThenFailurePropagates()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 0);
        IReadOnlyList<Direct3D9BitmapRealizationRectangle> expected =
        [
            new(1, 2, 10, 12)
        ];

        int result = state.GetValidSourceRectangles(
            isDeviceBitmap: true,
            (nint _, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
            {
                rectangles = expected;
                return Direct3D9Factory.InvalidCallHResult;
            },
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, expected),
            (result, rectangles));
    }

    [TestMethod]
    public void WhenRequiredBoundsAreInsidePrefilteredBitmapThenDeviceBitmapContainsThem()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);

        bool result = state.DoesContain(new Direct3D9BitmapRealizationRectangle(10, 12, 30, 25));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void WhenRequiredBoundsExtendOutsidePrefilteredBitmapThenDeviceBitmapDoesNotContainThem()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);

        bool result = state.DoesContain(new Direct3D9BitmapRealizationRectangle(4, 12, 30, 25));

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void WhenValidBoundsAreUpdatedThenCachedAndRequiredBoundsMatch()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationRectangle valid = new(10, 12, 30, 25);

        state.UpdateValidBounds(valid);

        Assert.AreEqual((valid, valid), (state.CachedRealizationBounds, state.RequiredRealizationBounds));
    }

    [TestMethod]
    public void WhenValidBoundsExtendOutsidePrefilteredBitmapThenUpdateFails()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 0, uniqueness: 0);
        Direct3D9BitmapRealizationRectangle invalid = new(4, 12, 30, 25);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => state.UpdateValidBounds(invalid));
    }

    [TestMethod]
    public void WhenCurrentCachedAreaIsAdjacentThenRequiredAreaKeepsReusableCachedSections()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 11);
        state.SetRequiredRealizationBounds(new Direct3D9BitmapRealizationRectangle(10, 10, 30, 30));
        state.Commit(11, new Direct3D9BitmapRealizationRectangle(0, 10, 20, 30));

        IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles = state.GetUpdateRectangles(true, []);

        Assert.AreEqual(
            (new Direct3D9BitmapRealizationRectangle(0, 10, 30, 30), new Direct3D9BitmapRealizationRectangle(20, 10, 30, 30)),
            (state.RequiredRealizationBounds, rectangles.Single()));
    }

    [TestMethod]
    public void WhenCachedAreaIsNotCurrentThenRequiredAreaIsNotExtended()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 12);
        Direct3D9BitmapRealizationRectangle required = new(10, 10, 30, 30);
        state.SetRequiredRealizationBounds(required);
        state.Commit(11, new Direct3D9BitmapRealizationRectangle(0, 10, 20, 30));

        IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles = state.GetUpdateRectangles(true, []);

        Assert.AreEqual(
            (required, new Direct3D9BitmapRealizationRectangle(20, 10, 30, 30)),
            (state.RequiredRealizationBounds, rectangles.Single()));
    }

    [TestMethod]
    public void WhenDirtyRectangleIsScaledThenRoundingExpandsAndClipsItToCachedArea()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 12);
        state.SetRequiredRealizationBounds(new Direct3D9BitmapRealizationRectangle(5, 7, 45, 37));
        state.Commit(11, new Direct3D9BitmapRealizationRectangle(5, 7, 45, 37));

        IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles = state.GetUpdateRectangles(
            true,
            [new Direct3D9BitmapRealizationRectangle(11, 15, 19, 21)]);

        Assert.AreEqual(new Direct3D9BitmapRealizationRectangle(5, 7, 10, 11), rectangles.Single());
    }

    [TestMethod]
    public void WhenDirtyInformationIsInvalidThenEntireRequiredAreaIsUpdated()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 12);
        Direct3D9BitmapRealizationRectangle required = new(10, 8, 30, 25);
        state.SetRequiredRealizationBounds(required);
        state.Commit(11, new Direct3D9BitmapRealizationRectangle(12, 9, 28, 24));

        IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles = state.GetUpdateRectangles(false, []);

        Assert.AreEqual(required, rectangles.Single());
    }

    [TestMethod]
    public void WhenCachePartiallyCoversRequiredAreaThenUncachedAreaIsSplitIntoFourRectangles()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 12);
        state.SetRequiredRealizationBounds(new Direct3D9BitmapRealizationRectangle(0, 0, 40, 30));
        state.Commit(11, new Direct3D9BitmapRealizationRectangle(10, 5, 30, 25));

        IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles = state.GetUpdateRectangles(true, []);

        CollectionAssert.AreEqual(
            new[]
            {
                new Direct3D9BitmapRealizationRectangle(0, 0, 40, 5),
                new Direct3D9BitmapRealizationRectangle(0, 25, 40, 30),
                new Direct3D9BitmapRealizationRectangle(0, 5, 10, 25),
                new Direct3D9BitmapRealizationRectangle(30, 5, 40, 25)
            },
            rectangles.ToArray());
    }

    private static Direct3D9BitmapColorSourceRealizationState CreateState(nint bitmap, uint uniqueness) =>
        CreateState(bitmap, () => uniqueness);

    private static Direct3D9BitmapColorSourceRealizationState CreateState(nint bitmap, Func<uint> getUniqueness)
    {
        Direct3D9BitmapColorSourceRealizationState state = new(
            bitmap,
            MilPixelFormat.Pbgra32Bpp,
            _ => getUniqueness());
        state.SetBitmapAndContextCacheParameters(23, CreateProperties());
        return state;
    }

    private static Direct3D9BitmapRealizationProperties CreateProperties() =>
        new(
            MilBitmapInterpolationMode.Linear,
            Direct3D9TextureMipMapLevel.One,
            MilBitmapWrapMode.Extend,
            MilPixelFormat.Pbgra32Bpp,
            IsMinimumRealizationRectComputed: false,
            BitmapWidth: 80,
            BitmapHeight: 60,
            Width: 40,
            Height: 30)
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(5, 7, 45, 37),
            LayoutU = new Direct3D9BitmapDimensionLayout(40, Direct3D9TexelLayout.Natural, Textureaddress.Clamp),
            LayoutV = new Direct3D9BitmapDimensionLayout(30, Direct3D9TexelLayout.Natural, Textureaddress.Clamp)
        };

    private sealed class ReusableContextHarness
    {
        private readonly Dictionary<nint, nint> _next = new() { [41] = 0 };
        private readonly List<string> _calls = [];

        internal string Calls => string.Join(',', _calls);

        internal void Record(string call) => _calls.Add(call);

        internal void ClearCalls() => _calls.Clear();

        internal Direct3D9BitmapReusableRealizationCandidates CreateCandidates() =>
            new(GetNext, SetNext, AddReference, Release);

        internal Direct3D9BitmapReusableRealizationSources CreateSources(
            Direct3D9BitmapRealizationProperties properties) =>
            new(
                new Direct3D9BitmapReusableRealizationTargetState(
                    7,
                    true,
                    true,
                    properties.Width,
                    properties.Height,
                    properties.SourceContained,
                    8),
                (nint _, out Direct3D9BitmapReusableRealizationSourceState state) =>
                {
                    state = new Direct3D9BitmapReusableRealizationSourceState(
                        0,
                        true,
                        true,
                        properties.Width,
                        properties.Height,
                        properties.SourceContained,
                        7,
                        true,
                        false);
                    return true;
                },
                GetNext,
                SetNext,
                AddReference,
                Release,
                source => Record($"adopt:{source}"));

        private nint GetNext(nint source) => _next[source];

        private void SetNext(nint source, nint next)
        {
            _next[source] = next;
            Record($"set-next:{source}:{next}");
        }

        private void AddReference(nint source) => Record($"add:{source}");

        private void Release(nint source) => Record($"release:{source}");
    }
}
