using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapCacheResolverTests
{
    [TestMethod]
    public void WhenCachedResourceExistsThenReferenceIsTransferredWithoutCreation()
    {
        List<string> calls = [];

        int result = Resolve(
            calls,
            cachedResource: 31,
            createResult: Direct3D9Factory.SuccessHResult,
            createdCache: 41,
            setResult: Direct3D9Factory.SuccessHResult,
            setResourceRequired: true,
            out nint bitmapCache);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|31|get-index|get:12:7",
            $"{result}|{bitmapCache}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenCachedResourceIsMissingThenCreatedCacheReferenceIsTransferredAndStored()
    {
        List<string> calls = [];

        int result = Resolve(
            calls,
            cachedResource: 0,
            createResult: Direct3D9Factory.SuccessHResult,
            createdCache: 41,
            setResult: Direct3D9Factory.SuccessHResult,
            setResourceRequired: true,
            out nint bitmapCache);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|41|get-index|get:12:7|create|set:12:7:41",
            $"{result}|{bitmapCache}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenOptionalSetResourceFailsThenCreatedCacheIsStillReturned()
    {
        List<string> calls = [];

        int result = Resolve(
            calls,
            cachedResource: 0,
            createResult: Direct3D9Factory.SuccessHResult,
            createdCache: 41,
            setResult: Direct3D9Factory.OutOfMemoryHResult,
            setResourceRequired: false,
            out nint bitmapCache);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|41|get-index|get:12:7|create|set:12:7:41",
            $"{result}|{bitmapCache}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenRequiredSetResourceFailsThenCreatedCacheIsReleased()
    {
        List<string> calls = [];

        int result = Resolve(
            calls,
            cachedResource: 0,
            createResult: Direct3D9Factory.SuccessHResult,
            createdCache: 41,
            setResult: Direct3D9Factory.OutOfMemoryHResult,
            setResourceRequired: true,
            out nint bitmapCache);

        Assert.AreEqual(
            $"{Direct3D9Factory.OutOfMemoryHResult}|0|get-index|get:12:7|create|set:12:7:41|release:41",
            $"{result}|{bitmapCache}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenGetResourceFailsWithReferenceThenReferenceIsReleasedAndFirstFailureIsReturned()
    {
        List<string> calls = [];

        int result = Direct3D9BitmapCacheResolver.GetOrCreate(
            12,
            true,
            (out uint cacheIndex) =>
            {
                calls.Add("get-index");
                cacheIndex = 7;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint resourceCache, uint cacheIndex, out nint cachedResource) =>
            {
                calls.Add($"get:{resourceCache}:{cacheIndex}");
                cachedResource = 31;
                return Direct3D9Factory.GenericFailureHResult;
            },
            UnexpectedCreate,
            UnexpectedSet,
            value => calls.Add($"release:{value}"),
            out nint bitmapCache);

        Assert.AreEqual(
            $"{Direct3D9Factory.GenericFailureHResult}|0|get-index|get:12:7|release:31",
            $"{result}|{bitmapCache}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenCreationFailsWithReferenceThenReferenceIsReleasedAndSetIsSkipped()
    {
        List<string> calls = [];

        int result = Resolve(
            calls,
            cachedResource: 0,
            createResult: Direct3D9Factory.OutOfMemoryHResult,
            createdCache: 41,
            setResult: Direct3D9Factory.SuccessHResult,
            setResourceRequired: true,
            out nint bitmapCache);

        Assert.AreEqual(
            $"{Direct3D9Factory.OutOfMemoryHResult}|0|get-index|get:12:7|create|release:41",
            $"{result}|{bitmapCache}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenResourceCacheIsMissingThenNotImplementedIsReturnedBeforeCallbacks()
    {
        int result = Direct3D9BitmapCacheResolver.GetOrCreate(
            0,
            true,
            UnexpectedIndex,
            UnexpectedGet,
            UnexpectedCreate,
            UnexpectedSet,
            _ => Assert.Fail(),
            out nint bitmapCache);

        Assert.AreEqual(
            (Direct3D9Factory.NotImplementedHResult, 0),
            (result, bitmapCache));
    }

    private static int Resolve(
        List<string> calls,
        nint cachedResource,
        int createResult,
        nint createdCache,
        int setResult,
        bool setResourceRequired,
        out nint bitmapCache) =>
        Direct3D9BitmapCacheResolver.GetOrCreate(
            12,
            setResourceRequired,
            (out uint cacheIndex) =>
            {
                calls.Add("get-index");
                cacheIndex = 7;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint resourceCache, uint cacheIndex, out nint value) =>
            {
                calls.Add($"get:{resourceCache}:{cacheIndex}");
                value = cachedResource;
                return Direct3D9Factory.SuccessHResult;
            },
            (out nint value) =>
            {
                calls.Add("create");
                value = createdCache;
                return createResult;
            },
            (nint resourceCache, uint cacheIndex, nint value) =>
            {
                calls.Add($"set:{resourceCache}:{cacheIndex}:{value}");
                return setResult;
            },
            value => calls.Add($"release:{value}"),
            out bitmapCache);

    private static int UnexpectedIndex(out uint cacheIndex)
    {
        cacheIndex = 0;
        Assert.Fail();
        return Direct3D9Factory.GenericFailureHResult;
    }

    private static int UnexpectedGet(nint resourceCache, uint cacheIndex, out nint cachedResource)
    {
        cachedResource = 0;
        Assert.Fail();
        return Direct3D9Factory.GenericFailureHResult;
    }

    private static int UnexpectedCreate(out nint bitmapCache)
    {
        bitmapCache = 0;
        Assert.Fail();
        return Direct3D9Factory.GenericFailureHResult;
    }

    private static int UnexpectedSet(nint resourceCache, uint cacheIndex, nint bitmapCache)
    {
        Assert.Fail();
        return Direct3D9Factory.GenericFailureHResult;
    }
}
