using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9BitmapSystemMemorySurfaceSourceTests
{
    private static int _addRefCount;
    private static int _releaseCount;

    [TestInitialize]
    public void Initialize()
    {
        _addRefCount = 0;
        _releaseCount = 0;
    }

    [TestMethod]
    public void WhenBitsIdentityMatchesThenCachedSurfaceIsReturnedWithCallerReference()
    {
        using FakeSurfaceObject surfaceObject = new();
        int createCallCount = 0;
        void* pixelsPassedToCreate = null;
        Direct3D9BitmapSystemMemorySurfaceSource source = new(
            (uint _, uint _, Format _, void* pixels, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                createCallCount++;
                pixelsPassedToCreate = pixels;
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        byte* bits = stackalloc byte[32];

        int firstResult = source.GetSurface(bits, 8, 4, true, out Direct3D9SystemMemoryUpdateSurface? firstSurface);
        int secondResult = source.GetSurface(bits, 8, 4, true, out Direct3D9SystemMemoryUpdateSurface? secondSurface);
        bool sameSurface = firstSurface!.Surface == secondSurface!.Surface;
        firstSurface.Dispose();
        secondSurface.Dispose();
        source.Dispose();

        Assert.AreEqual(
            (0, 0, 1, (nint) bits, true, 2, 3),
            (firstResult, secondResult, createCallCount, (nint) pixelsPassedToCreate, sameSurface, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenBitsIdentityChangesThenCachedSurfaceIsNotReplaced()
    {
        using FakeSurfaceObject surfaceObject = new();
        int createCallCount = 0;
        Direct3D9BitmapSystemMemorySurfaceSource source = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                createCallCount++;
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        byte* firstBits = stackalloc byte[16];
        byte* secondBits = stackalloc byte[16];

        _ = source.GetSurface(firstBits, 4, 4, true, out Direct3D9SystemMemoryUpdateSurface? firstSurface);
        firstSurface!.Dispose();
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => source.GetSurface(secondBits, 4, 4, true, out _));
        source.Dispose();

        Assert.AreEqual(
            ("The bitmap bits moved after a system-memory surface cached their address.", 1, 1, 2),
            (exception.Message, createCallCount, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenCreationCannotReferenceBitsThenNullPixelsArePassedButIdentityIsCached()
    {
        using FakeSurfaceObject surfaceObject = new();
        int createCallCount = 0;
        nint pixelsPassedToCreate = -1;
        Direct3D9BitmapSystemMemorySurfaceSource source = new(
            (uint _, uint _, Format _, void* pixels, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                createCallCount++;
                pixelsPassedToCreate = (nint) pixels;
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        byte* bits = stackalloc byte[16];

        _ = source.GetSurface(bits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? firstSurface);
        firstSurface!.Dispose();
        _ = source.GetSurface(bits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? reusedSurface);
        reusedSurface!.Dispose();
        source.Dispose();

        Assert.AreEqual((1, 0, 2, 3), (createCallCount, pixelsPassedToCreate, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenCreationFailsThenUnexpectedReturnedSurfaceIsReleasedAndNotCached()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9BitmapSystemMemorySurfaceSource source = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.GenericFailureHResult;
            },
            Format.A8R8G8B8);
        byte* bits = stackalloc byte[16];

        int result = source.GetSurface(bits, 4, 4, true, out Direct3D9SystemMemoryUpdateSurface? surface);
        source.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, null, 0, 1),
            (result, surface, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenTargetHasNoCachedSurfaceThenReusableSurfaceIsAdoptedWithIndependentReference()
    {
        using FakeSurfaceObject surfaceObject = new();
        int createCallCount = 0;
        Direct3D9BitmapSystemMemorySurfaceSource reusableSource = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                createCallCount++;
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        Direct3D9BitmapSystemMemorySurfaceSource targetSource = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                createCallCount++;
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        byte* bits = stackalloc byte[16];
        _ = reusableSource.GetSurface(bits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? reusableSurface);
        reusableSurface!.Dispose();

        bool adopted = targetSource.TryAdoptCachedSurfaceFrom(reusableSource);
        int result = targetSource.GetSurface(bits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? adoptedSurface);
        bool sameSurface = adoptedSurface!.Surface == surfaceObject.Surface;
        adoptedSurface.Dispose();
        reusableSource.Dispose();
        targetSource.Dispose();

        Assert.AreEqual(
            (true, 0, true, 1, 3, 4),
            (adopted, result, sameSurface, createCallCount, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenTargetAlreadyHasCachedSurfaceThenReusableSurfaceIsNotAdopted()
    {
        using FakeSurfaceObject targetSurfaceObject = new();
        using FakeSurfaceObject reusableSurfaceObject = new();
        Direct3D9BitmapSystemMemorySurfaceSource targetSource = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                surface = new Direct3D9SystemMemoryUpdateSurface(targetSurfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        Direct3D9BitmapSystemMemorySurfaceSource reusableSource = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                surface = new Direct3D9SystemMemoryUpdateSurface(reusableSurfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        byte* targetBits = stackalloc byte[16];
        byte* reusableBits = stackalloc byte[16];
        _ = targetSource.GetSurface(targetBits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? targetSurface);
        _ = reusableSource.GetSurface(reusableBits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? reusableSurface);
        targetSurface!.Dispose();
        reusableSurface!.Dispose();

        bool adopted = targetSource.TryAdoptCachedSurfaceFrom(reusableSource);
        _ = targetSource.GetSurface(targetBits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? retainedSurface);
        bool retainedTargetSurface = retainedSurface!.Surface == targetSurfaceObject.Surface;
        retainedSurface.Dispose();
        targetSource.Dispose();
        reusableSource.Dispose();

        Assert.AreEqual((false, true, 3, 5), (adopted, retainedTargetSurface, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenReusableSourceHasNoCachedSurfaceThenNothingIsAdopted()
    {
        Direct3D9BitmapSystemMemorySurfaceSource targetSource = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.GenericFailureHResult;
            },
            Format.A8R8G8B8);
        Direct3D9BitmapSystemMemorySurfaceSource reusableSource = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.GenericFailureHResult;
            },
            Format.A8R8G8B8);

        bool adopted = targetSource.TryAdoptCachedSurfaceFrom(reusableSource);
        targetSource.Dispose();
        reusableSource.Dispose();

        Assert.AreEqual((false, 0, 0), (adopted, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenRegistryAdoptsRealizedOwnerSurfaceThenReferenceRemainsIndependent()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9BitmapSystemMemorySurfaceSource reusableSurfaceSource = CreateSurfaceSource(surfaceObject.Surface);
        Direct3D9BitmapSystemMemorySurfaceSource targetSurfaceSource = CreateSurfaceSource(surfaceObject.Surface);
        byte* bits = stackalloc byte[16];
        _ = reusableSurfaceSource.GetSurface(bits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? reusableSurface);
        reusableSurface!.Dispose();
        Direct3D9BitmapColorSourceTextureRealizer reusableRealizer = CreateRealizer(reusableSurfaceSource);
        Direct3D9BitmapColorSourceTextureRealizer targetRealizer = CreateRealizer(targetSurfaceSource);
        int reusableRealizeResult = reusableRealizer.Realize();
        int targetRealizeResult = targetRealizer.Realize();
        Direct3D9BitmapColorSourceRegistry registry = new();
        registry.Register(17, new Direct3D9BitmapColorSourceOwner(targetRealizer, isDeviceBitmap: false));
        registry.Register(23, new Direct3D9BitmapColorSourceOwner(reusableRealizer, isDeviceBitmap: false));

        bool adopted = registry.TryAdoptSystemMemorySurface(17, 23);
        reusableRealizer.Dispose();
        int result = targetSurfaceSource.GetSurface(bits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? adoptedSurface);
        bool sameSurface = adoptedSurface!.Surface == surfaceObject.Surface;
        adoptedSurface.Dispose();
        targetRealizer.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, Direct3D9Factory.SuccessHResult, true, Direct3D9Factory.SuccessHResult, true, 3, 4),
            (reusableRealizeResult, targetRealizeResult, adopted, result, sameSurface, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenRegistryConsumesMixedReusableCandidatesThenOrderOwnershipAndAdoptionArePreserved()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9BitmapSystemMemorySurfaceSource targetSurfaceSource = CreateSurfaceSource(surfaceObject.Surface);
        Direct3D9BitmapSystemMemorySurfaceSource reusableTextureSurfaceSource = CreateSurfaceSource(surfaceObject.Surface);
        Direct3D9BitmapSystemMemorySurfaceSource adoptionSurfaceSource = CreateSurfaceSource(surfaceObject.Surface);
        byte* bits = stackalloc byte[16];
        _ = adoptionSurfaceSource.GetSurface(bits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? reusableSurface);
        reusableSurface!.Dispose();
        Direct3D9BitmapColorSourceTextureRealizer targetRealizer = CreateRealizer(targetSurfaceSource, isRenderTarget: true);
        Direct3D9BitmapColorSourceTextureRealizer reusableTextureRealizer = CreateRealizer(reusableTextureSurfaceSource, isRenderTarget: true);
        Direct3D9BitmapColorSourceTextureRealizer adoptionRealizer = CreateRealizer(adoptionSurfaceSource, isRenderTarget: false);
        _ = targetRealizer.Realize();
        _ = reusableTextureRealizer.Realize();
        _ = adoptionRealizer.Realize();
        Direct3D9BitmapColorSourceRegistry registry = new();
        registry.Register(17, new Direct3D9BitmapColorSourceOwner(targetRealizer, isDeviceBitmap: false));
        registry.Register(23, new Direct3D9BitmapColorSourceOwner(reusableTextureRealizer, isDeviceBitmap: false));
        registry.Register(29, new Direct3D9BitmapColorSourceOwner(adoptionRealizer, isDeviceBitmap: false));
        Dictionary<nint, nint> next = [];
        List<string> ownership = [];
        using Direct3D9BitmapReusableRealizationCandidates candidates = new(
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            source => ownership.Add($"add:{source}"),
            source => ownership.Add($"release:{source}"));
        candidates.Add(29);
        candidates.Add(23);
        ownership.Clear();
        Direct3D9BitmapReusableRealizationSources sources = registry.CreateReusableRealizationSources(
            17,
            canStretchRectFromTextures: false,
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            source => ownership.Add($"add:{source}"),
            source => ownership.Add($"release:{source}"));

        sources.Consume(candidates);
        nint retainedSource = sources.Head;
        adoptionRealizer.Dispose();
        int result = targetSurfaceSource.GetSurface(bits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? adoptedSurface);
        bool sameSurface = adoptedSurface!.Surface == surfaceObject.Surface;
        adoptedSurface.Dispose();
        sources.Dispose();
        reusableTextureRealizer.Dispose();
        targetRealizer.Dispose();

        Assert.AreEqual(
            (23, "add:23,release:23,release:29,release:23", Direct3D9Factory.SuccessHResult, true, 3, 4),
            (retainedSource, string.Join(',', ownership), result, sameSurface, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenProductionContextFactoryUsesRegistryThenPopulationIsBuiltFromSelectedOwner()
    {
        using Direct3D9Device device = new(null, null, 0, Devtype.Hal, 0, default);
        Direct3D9BitmapSystemMemorySurfaceSource surfaceSource = CreateSurfaceSource(null);
        Direct3D9BitmapColorSourceTextureRealizer realizer = CreateRealizer(surfaceSource, isRenderTarget: true);
        _ = realizer.Realize();
        Direct3D9BitmapColorSourceRegistry registry = new();
        registry.Register(17, new Direct3D9BitmapColorSourceOwner(realizer, isDeviceBitmap: false));
        Dictionary<nint, nint> next = [];
        using Direct3D9BitmapReusableRealizationCandidates candidates = new(
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            _ => { },
            _ => { });
        Direct3D9BitmapReusableRealizationContextFactory factory = new(
            false,
            _ => throw new AssertFailedException(),
            device,
            registry,
            canStretchRectFromTextures: true,
            (nint _, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
            {
                rectangles = [];
                return Direct3D9Factory.SuccessHResult;
            },
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            _ => { },
            _ => { });

        using Direct3D9BitmapReusableRealizationContext context = factory.Create(17, candidates);
        bool productionDependenciesCreated = context.Sources.Head == 0 && context.Population is not null;
        realizer.Dispose();

        Assert.IsTrue(productionDependenciesCreated);
    }

    [TestMethod]
    public void WhenProductionContextFactoryConsumesMixedRegistryCandidatesThenOrderOwnershipAndAdoptionArePreserved()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Device device = new(null, null, 0, Devtype.Hal, 0, default);
        Direct3D9BitmapSystemMemorySurfaceSource targetSurfaceSource = CreateSurfaceSource(surfaceObject.Surface);
        Direct3D9BitmapSystemMemorySurfaceSource reusableTextureSurfaceSource = CreateSurfaceSource(surfaceObject.Surface);
        Direct3D9BitmapSystemMemorySurfaceSource adoptionSurfaceSource = CreateSurfaceSource(surfaceObject.Surface);
        byte* bits = stackalloc byte[16];
        _ = adoptionSurfaceSource.GetSurface(bits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? reusableSurface);
        reusableSurface!.Dispose();
        Direct3D9BitmapColorSourceTextureRealizer targetRealizer = CreateRealizer(targetSurfaceSource, isRenderTarget: true);
        Direct3D9BitmapColorSourceTextureRealizer reusableTextureRealizer = CreateRealizer(reusableTextureSurfaceSource, isRenderTarget: true);
        Direct3D9BitmapColorSourceTextureRealizer adoptionRealizer = CreateRealizer(adoptionSurfaceSource, isRenderTarget: false);
        _ = targetRealizer.Realize();
        _ = reusableTextureRealizer.Realize();
        _ = adoptionRealizer.Realize();
        Direct3D9BitmapColorSourceRegistry registry = new();
        registry.Register(17, new Direct3D9BitmapColorSourceOwner(targetRealizer, isDeviceBitmap: false));
        registry.Register(23, new Direct3D9BitmapColorSourceOwner(reusableTextureRealizer, isDeviceBitmap: false));
        registry.Register(29, new Direct3D9BitmapColorSourceOwner(adoptionRealizer, isDeviceBitmap: false));
        Dictionary<nint, nint> next = [];
        List<string> ownership = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = new(
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            source => ownership.Add($"add:{source}"),
            source => ownership.Add($"release:{source}"));
        candidates.Add(29);
        candidates.Add(23);
        ownership.Clear();
        Direct3D9BitmapReusableRealizationContextFactory factory = new(
            false,
            _ => throw new AssertFailedException(),
            device,
            registry,
            canStretchRectFromTextures: false,
            (nint _, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
            {
                rectangles = [];
                return Direct3D9Factory.SuccessHResult;
            },
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            source => ownership.Add($"add:{source}"),
            source => ownership.Add($"release:{source}"));
        Direct3D9BitmapReusableRealizationContext context = factory.Create(17, candidates);

        context.Sources.Consume(context.Candidates);
        nint retainedSource = context.Sources.Head;
        adoptionRealizer.Dispose();
        int result = targetSurfaceSource.GetSurface(bits, 4, 4, false, out Direct3D9SystemMemoryUpdateSurface? adoptedSurface);
        bool sameSurface = adoptedSurface!.Surface == surfaceObject.Surface;
        adoptedSurface.Dispose();
        context.Dispose();
        reusableTextureRealizer.Dispose();
        targetRealizer.Dispose();

        Assert.AreEqual(
            (23, "add:23,release:23,release:29,release:23", Direct3D9Factory.SuccessHResult, true, 3, 4),
            (retainedSource, string.Join(',', ownership), result, sameSurface, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenProductionPipelineFactoryMixedCandidateStateResolutionFailsThenTransferredCurrentAndRemainingSourcesAreReleased()
    {
        using Direct3D9Device device = new(null, null, 0, Devtype.Hal, 0, default);
        Direct3D9BitmapSystemMemorySurfaceSource targetSurfaceSource = CreateSurfaceSource(null);
        Direct3D9BitmapSystemMemorySurfaceSource reusableTextureSurfaceSource = CreateSurfaceSource(null);
        Direct3D9BitmapColorSourceTextureRealizer targetRealizer = CreateRealizer(targetSurfaceSource, isRenderTarget: true);
        Direct3D9BitmapColorSourceTextureRealizer reusableTextureRealizer = CreateRealizer(reusableTextureSurfaceSource, isRenderTarget: true);
        _ = targetRealizer.Realize();
        _ = reusableTextureRealizer.Realize();
        Direct3D9BitmapColorSourceRegistry registry = new();
        registry.Register(17, new Direct3D9BitmapColorSourceOwner(targetRealizer, isDeviceBitmap: false));
        registry.Register(23, new Direct3D9BitmapColorSourceOwner(reusableTextureRealizer, isDeviceBitmap: false));
        Dictionary<nint, nint> next = [];
        List<string> ownership = [];
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(manager, 11, 13, () => { }, _ => { }, _ => { });
        using Direct3D9BitmapFormatCacheEntry entries = new(_ => { }, _ => { });
        Direct3D9BitmapRealizationProperties properties = new(
            MilBitmapInterpolationMode.NearestNeighbor,
            Direct3D9TextureMipMapLevel.One,
            MilBitmapWrapMode.Extend,
            MilPixelFormat.Pbgra32Bpp,
            IsMinimumRealizationRectComputed: false,
            BitmapWidth: 4,
            BitmapHeight: 4,
            Width: 4,
            Height: 4)
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 4, 4)
        };
        Direct3D9BitmapPipelineColorSourceFactory pipelineFactory = new(
            (nint _, nint _, nint _, nint _, nint _, Direct3D9BitmapColorSourceContextParameters _, out Direct3D9BitmapColorSourceInitializationState? state) =>
            {
                state = new Direct3D9BitmapColorSourceInitializationState(
                    entries,
                    lifetime,
                    properties,
                    new Direct3D9DelayedBounds(),
                    false);
                return Direct3D9Factory.SuccessHResult;
            },
            () =>
            {
                Direct3D9BitmapReusableRealizationCandidates candidates = new(
                    source => next.GetValueOrDefault(source),
                    (source, value) => next[source] = value,
                    source => ownership.Add($"add:{source}"),
                    source => ownership.Add($"release:{source}"));
                candidates.Add(31);
                candidates.Add(29);
                candidates.Add(23);
                ownership.Clear();
                return candidates;
            },
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            _ => true,
            canStretchRectFromTextures: false,
            (bool _, out nint colorSource) =>
            {
                colorSource = 17;
                return Direct3D9Factory.SuccessHResult;
            },
            device,
            _ => throw new AssertFailedException(),
            registry,
            (nint _, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
            {
                rectangles = [];
                return Direct3D9Factory.SuccessHResult;
            },
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            source => ownership.Add($"add:{source}"),
            source => ownership.Add($"release:{source}"),
            context => new Direct3D9BitmapColorSourceTextureRealizer(
                () =>
                {
                    context.Sources.Consume(context.Candidates);
                    return (Direct3D9Factory.NotImplementedHResult, null);
                },
                context.Dispose),
            (_, textureRealizer) =>
            {
                _ = textureRealizer.Realize();
                throw new AssertFailedException();
            });

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => pipelineFactory.ChooseAndCreate(
            1,
            1,
            0,
            0,
            0,
            Direct3D9BitmapColorSourceContextParameters.FromExplicitSettings(
                MilBitmapInterpolationMode.NearestNeighbor,
                false,
                MilPixelFormat.Pbgra32Bpp,
                MilBitmapWrapMode.Extend),
            out _));
        reusableTextureRealizer.Dispose();
        targetRealizer.Dispose();

        Assert.AreEqual(
            ("The bitmap color source is not registered.", "add:23,release:23,release:29,release:31,release:23,release:17"),
            (exception.Message, string.Join(',', ownership)));
    }

    [TestMethod]
    public void WhenProductionPipelineFactoryMixedCandidateAdoptionFailsThenTransferredCurrentAndRemainingSourcesAreReleased()
    {
        using Direct3D9Device device = new(null, null, 0, Devtype.Hal, 0, default);
        Direct3D9BitmapSystemMemorySurfaceSource targetSurfaceSource = CreateSurfaceSource(null);
        Direct3D9BitmapSystemMemorySurfaceSource reusableTextureSurfaceSource = CreateSurfaceSource(null);
        Direct3D9BitmapSystemMemorySurfaceSource adoptionSurfaceSource = CreateSurfaceSource(null);
        Direct3D9BitmapColorSourceTextureRealizer targetRealizer = CreateRealizer(targetSurfaceSource, isRenderTarget: true);
        Direct3D9BitmapColorSourceTextureRealizer reusableTextureRealizer = CreateRealizer(reusableTextureSurfaceSource, isRenderTarget: true);
        Direct3D9BitmapColorSourceTextureRealizer adoptionRealizer = CreateRealizer(adoptionSurfaceSource);
        _ = targetRealizer.Realize();
        _ = reusableTextureRealizer.Realize();
        _ = adoptionRealizer.Realize();
        Direct3D9BitmapColorSourceRegistry registry = new();
        registry.Register(17, new Direct3D9BitmapColorSourceOwner(targetRealizer, isDeviceBitmap: false));
        registry.Register(23, new Direct3D9BitmapColorSourceOwner(reusableTextureRealizer, isDeviceBitmap: false));
        registry.Register(29, new Direct3D9BitmapColorSourceOwner(adoptionRealizer, isDeviceBitmap: false));
        adoptionSurfaceSource.Dispose();
        Dictionary<nint, nint> next = [];
        List<string> ownership = [];
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(manager, 11, 13, () => { }, _ => { }, _ => { });
        using Direct3D9BitmapFormatCacheEntry entries = new(_ => { }, _ => { });
        Direct3D9BitmapRealizationProperties properties = new(
            MilBitmapInterpolationMode.NearestNeighbor,
            Direct3D9TextureMipMapLevel.One,
            MilBitmapWrapMode.Extend,
            MilPixelFormat.Pbgra32Bpp,
            IsMinimumRealizationRectComputed: false,
            BitmapWidth: 4,
            BitmapHeight: 4,
            Width: 4,
            Height: 4)
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 4, 4)
        };
        Direct3D9BitmapPipelineColorSourceFactory pipelineFactory = new(
            (nint _, nint _, nint _, nint _, nint _, Direct3D9BitmapColorSourceContextParameters _, out Direct3D9BitmapColorSourceInitializationState? state) =>
            {
                state = new Direct3D9BitmapColorSourceInitializationState(
                    entries,
                    lifetime,
                    properties,
                    new Direct3D9DelayedBounds(),
                    false);
                return Direct3D9Factory.SuccessHResult;
            },
            () =>
            {
                Direct3D9BitmapReusableRealizationCandidates candidates = new(
                    source => next.GetValueOrDefault(source),
                    (source, value) => next[source] = value,
                    source => ownership.Add($"add:{source}"),
                    source => ownership.Add($"release:{source}"));
                candidates.Add(31);
                candidates.Add(29);
                candidates.Add(23);
                ownership.Clear();
                return candidates;
            },
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            _ => true,
            canStretchRectFromTextures: false,
            (bool _, out nint colorSource) =>
            {
                colorSource = 17;
                return Direct3D9Factory.SuccessHResult;
            },
            device,
            _ => throw new AssertFailedException(),
            registry,
            (nint _, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
            {
                rectangles = [];
                return Direct3D9Factory.SuccessHResult;
            },
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            source => ownership.Add($"add:{source}"),
            source => ownership.Add($"release:{source}"),
            context => new Direct3D9BitmapColorSourceTextureRealizer(
                () =>
                {
                    context.Sources.Consume(context.Candidates);
                    return (Direct3D9Factory.NotImplementedHResult, null);
                },
                context.Dispose),
            (_, textureRealizer) =>
            {
                _ = textureRealizer.Realize();
                throw new AssertFailedException();
            });

        ObjectDisposedException exception = Assert.ThrowsExactly<ObjectDisposedException>(() => pipelineFactory.ChooseAndCreate(
            1,
            1,
            0,
            0,
            0,
            Direct3D9BitmapColorSourceContextParameters.FromExplicitSettings(
                MilBitmapInterpolationMode.NearestNeighbor,
                false,
                MilPixelFormat.Pbgra32Bpp,
                MilBitmapWrapMode.Extend),
            out _));
        adoptionRealizer.Dispose();
        reusableTextureRealizer.Dispose();
        targetRealizer.Dispose();

        Assert.AreEqual(
            (typeof(Direct3D9BitmapSystemMemorySurfaceSource).FullName, "add:23,release:23,release:29,release:31,release:23,release:17"),
            (exception.ObjectName, string.Join(',', ownership)));
    }

    [TestMethod]
    public void WhenProductionPipelineFactoryTargetOwnerIsDisposedAfterStateResolutionThenFirstErrorAndCleanupArePreserved()
    {
        using Direct3D9Device device = new(null, null, 0, Devtype.Hal, 0, default);
        Direct3D9BitmapSystemMemorySurfaceSource targetSurfaceSource = CreateSurfaceSource(null);
        Direct3D9BitmapSystemMemorySurfaceSource reusableTextureSurfaceSource = CreateSurfaceSource(null);
        Direct3D9BitmapSystemMemorySurfaceSource adoptionSurfaceSource = CreateSurfaceSource(null);
        Direct3D9BitmapColorSourceTextureRealizer targetRealizer = CreateRealizer(targetSurfaceSource, isRenderTarget: true);
        Direct3D9BitmapColorSourceTextureRealizer reusableTextureRealizer = CreateRealizer(reusableTextureSurfaceSource, isRenderTarget: true);
        Direct3D9BitmapColorSourceTextureRealizer adoptionRealizer = CreateRealizer(adoptionSurfaceSource);
        _ = targetRealizer.Realize();
        _ = reusableTextureRealizer.Realize();
        _ = adoptionRealizer.Realize();
        Direct3D9BitmapColorSourceRegistry registry = new();
        registry.Register(17, new Direct3D9BitmapColorSourceOwner(targetRealizer, isDeviceBitmap: false));
        registry.Register(23, new Direct3D9BitmapColorSourceOwner(reusableTextureRealizer, isDeviceBitmap: false));
        registry.Register(29, new Direct3D9BitmapColorSourceOwner(adoptionRealizer, isDeviceBitmap: false));
        Dictionary<nint, nint> next = [];
        List<string> ownership = [];
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(manager, 11, 13, () => { }, _ => { }, _ => { });
        using Direct3D9BitmapFormatCacheEntry entries = new(_ => { }, _ => { });
        Direct3D9BitmapRealizationProperties properties = new(
            MilBitmapInterpolationMode.NearestNeighbor,
            Direct3D9TextureMipMapLevel.One,
            MilBitmapWrapMode.Extend,
            MilPixelFormat.Pbgra32Bpp,
            IsMinimumRealizationRectComputed: false,
            BitmapWidth: 4,
            BitmapHeight: 4,
            Width: 4,
            Height: 4)
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 4, 4)
        };
        Direct3D9BitmapPipelineColorSourceFactory pipelineFactory = new(
            (nint _, nint _, nint _, nint _, nint _, Direct3D9BitmapColorSourceContextParameters _, out Direct3D9BitmapColorSourceInitializationState? state) =>
            {
                state = new Direct3D9BitmapColorSourceInitializationState(
                    entries,
                    lifetime,
                    properties,
                    new Direct3D9DelayedBounds(),
                    false);
                return Direct3D9Factory.SuccessHResult;
            },
            () =>
            {
                Direct3D9BitmapReusableRealizationCandidates candidates = new(
                    source => next.GetValueOrDefault(source),
                    (source, value) => next[source] = value,
                    source => ownership.Add($"add:{source}"),
                    source => ownership.Add($"release:{source}"));
                candidates.Add(31);
                candidates.Add(29);
                candidates.Add(23);
                ownership.Clear();
                return candidates;
            },
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            _ => true,
            canStretchRectFromTextures: false,
            (bool _, out nint colorSource) =>
            {
                colorSource = 17;
                return Direct3D9Factory.SuccessHResult;
            },
            device,
            _ => throw new AssertFailedException(),
            registry,
            (nint _, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
            {
                rectangles = [];
                return Direct3D9Factory.SuccessHResult;
            },
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            source => ownership.Add($"add:{source}"),
            source => ownership.Add($"release:{source}"),
            context => new Direct3D9BitmapColorSourceTextureRealizer(
                () =>
                {
                    targetRealizer.Dispose();
                    context.Sources.Consume(context.Candidates);
                    return (Direct3D9Factory.NotImplementedHResult, null);
                },
                context.Dispose),
            (_, textureRealizer) =>
            {
                _ = textureRealizer.Realize();
                throw new AssertFailedException();
            });

        ObjectDisposedException exception = Assert.ThrowsExactly<ObjectDisposedException>(() => pipelineFactory.ChooseAndCreate(
            1,
            1,
            0,
            0,
            0,
            Direct3D9BitmapColorSourceContextParameters.FromExplicitSettings(
                MilBitmapInterpolationMode.NearestNeighbor,
                false,
                MilPixelFormat.Pbgra32Bpp,
                MilBitmapWrapMode.Extend),
            out _));
        adoptionRealizer.Dispose();
        reusableTextureRealizer.Dispose();
        targetRealizer.Dispose();

        Assert.AreEqual(
            (typeof(Direct3D9BitmapColorSourceTextureRealizer).FullName, "add:23,release:23,release:29,release:31,release:23,release:17"),
            (exception.ObjectName, string.Join(',', ownership)));
    }

    private static Direct3D9BitmapSystemMemorySurfaceSource CreateSurfaceSource(IDirect3DSurface9* surfacePointer) =>
        new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                surface = new Direct3D9SystemMemoryUpdateSurface(surfacePointer);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);

    private static Direct3D9BitmapColorSourceTextureRealizer CreateRealizer(
        Direct3D9BitmapSystemMemorySurfaceSource surfaceSource,
        bool isRenderTarget = false)
    {
        Direct3D9BitmapColorSourceRealizationState state = new(0, MilPixelFormat.Pbgra32Bpp, _ => 0);
        state.SetBitmapAndContextCacheParameters(
            1,
            new Direct3D9BitmapRealizationProperties(
                MilBitmapInterpolationMode.NearestNeighbor,
                Direct3D9TextureMipMapLevel.One,
                MilBitmapWrapMode.Extend,
                MilPixelFormat.Pbgra32Bpp,
                IsMinimumRealizationRectComputed: false,
                BitmapWidth: 4,
                BitmapHeight: 4,
                Width: 4,
                Height: 4)
            {
                SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 4, 4)
            });
        state.SetRequiredRealizationBounds(new Direct3D9BitmapRealizationRectangle(0, 0, 4, 4));
        Direct3D9BitmapTexturePopulator populator = new(
            (nint _, uint _, nint _, out nint bitmapLock, out bool copySource, out nint surface) =>
            {
                bitmapLock = 0;
                copySource = false;
                surface = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, uint _, nint _, nint _, bool _) => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            state,
            _ => { });
        Direct3D9BitmapColorSourceTextureUpdater updater = new(
            state,
            populator,
            (out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles, out uint uniquenessToken) =>
            {
                rectangles = [];
                uniquenessToken = 0;
                return true;
            });
        Direct3D9Texture texture = new(new Direct3D9ResourceManager(), null, 1, 1);
        Direct3D9BitmapColorSourceTextureRealization realization = new(
            updater,
            surfaceSource,
            texture,
            state,
            isRenderTarget);
        return new Direct3D9BitmapColorSourceTextureRealizer(
            () => (Direct3D9Factory.SuccessHResult, realization));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(void*** surface)
    {
        _addRefCount++;
        return (uint) (_addRefCount + 1);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(void*** surface)
    {
        _releaseCount++;
        return 0;
    }

    private struct FakeSurfaceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DSurface9* Surface;

        public FakeSurfaceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            Surface = (IDirect3DSurface9*) memory;
            void** vtable = memory + 1;
            *memory = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &AddRef;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &Release;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }
}
