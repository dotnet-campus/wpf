using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapCacheRetrieverTests
{
    [TestMethod]
    public void WhenBitmapAndCacheExistThenBitmapBecomesNoRefAndCacheReferenceIsTransferred()
    {
        List<string> calls = [];

        int result = Direct3D9BitmapCacheRetriever.Retrieve(
            (out nint bitmap) =>
            {
                calls.Add("query-bitmap");
                bitmap = 11;
                return Direct3D9Factory.SuccessHResult;
            },
            (out nint resourceCache) =>
            {
                calls.Add("query-cache");
                resourceCache = 12;
                return Direct3D9Factory.SuccessHResult;
            },
            (out uint cacheIndex) =>
            {
                calls.Add("get-index");
                cacheIndex = 7;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint resourceCache, uint cacheIndex, out nint cachedResource) =>
            {
                calls.Add($"get-resource:{resourceCache}:{cacheIndex}");
                cachedResource = 13;
                return Direct3D9Factory.SuccessHResult;
            },
            value => calls.Add($"release:{value}"),
            out nint bitmapNoReference,
            out nint bitmapCache);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|11|13|query-bitmap|release:11|query-cache|get-index|get-resource:12:7|release:12",
            $"{result}|{bitmapNoReference}|{bitmapCache}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenBitmapInterfaceIsUnavailableThenCacheLookupStillRuns()
    {
        int result = Direct3D9BitmapCacheRetriever.Retrieve(
            (out nint bitmap) =>
            {
                bitmap = 0;
                return Direct3D9Factory.NoInterfaceHResult;
            },
            (out nint resourceCache) =>
            {
                resourceCache = 12;
                return Direct3D9Factory.SuccessHResult;
            },
            (out uint cacheIndex) =>
            {
                cacheIndex = 7;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, uint _, out nint cachedResource) =>
            {
                cachedResource = 13;
                return Direct3D9Factory.SuccessHResult;
            },
            _ => { },
            out nint bitmapNoReference,
            out nint bitmapCache);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, 0, 13),
            (result, bitmapNoReference, bitmapCache));
    }

    [TestMethod]
    public void WhenResourceCacheInterfaceIsUnavailableThenOptionalOutputsAreClearedAndSuccessIsReturned()
    {
        List<nint> releases = [];

        int result = Direct3D9BitmapCacheRetriever.Retrieve(
            (out nint bitmap) =>
            {
                bitmap = 11;
                return Direct3D9Factory.SuccessHResult;
            },
            (out nint resourceCache) =>
            {
                resourceCache = 0;
                return Direct3D9Factory.NoInterfaceHResult;
            },
            UnexpectedCacheIndex,
            UnexpectedCachedResource,
            releases.Add,
            out nint bitmapNoReference,
            out nint bitmapCache);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|0|0|11",
            $"{result}|{bitmapNoReference}|{bitmapCache}|{string.Join('|', releases)}");
    }

    [TestMethod]
    public void WhenCacheIndexLookupFailsThenFirstFailureIsReturnedAndResourceCacheIsReleased()
    {
        List<nint> releases = [];

        int result = Direct3D9BitmapCacheRetriever.Retrieve(
            MissingBitmap,
            (out nint resourceCache) =>
            {
                resourceCache = 12;
                return Direct3D9Factory.SuccessHResult;
            },
            (out uint cacheIndex) =>
            {
                cacheIndex = 0;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            UnexpectedCachedResource,
            releases.Add,
            out _,
            out nint bitmapCache);

        Assert.AreEqual(
            $"{Direct3D9Factory.OutOfMemoryHResult}|0|12",
            $"{result}|{bitmapCache}|{string.Join('|', releases)}");
    }

    [TestMethod]
    public void WhenCachedResourceLookupFailsWithResourceThenReturnedReferenceIsReleasedBeforeCacheInterface()
    {
        List<string> calls = [];

        int result = Direct3D9BitmapCacheRetriever.Retrieve(
            MissingBitmap,
            (out nint resourceCache) =>
            {
                resourceCache = 12;
                return Direct3D9Factory.SuccessHResult;
            },
            (out uint cacheIndex) =>
            {
                cacheIndex = 7;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, uint _, out nint cachedResource) =>
            {
                cachedResource = 13;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            value => calls.Add($"release:{value}"),
            out _,
            out nint bitmapCache);

        Assert.AreEqual(
            $"{Direct3D9Factory.OutOfMemoryHResult}|0|release:13|release:12",
            $"{result}|{bitmapCache}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenResourceCacheQueryFailsWithInterfaceThenInterfaceIsReleasedAndOutputsAreCleared()
    {
        List<nint> releases = [];

        int result = Direct3D9BitmapCacheRetriever.Retrieve(
            MissingBitmap,
            (out nint resourceCache) =>
            {
                resourceCache = 12;
                return Direct3D9Factory.NoInterfaceHResult;
            },
            UnexpectedCacheIndex,
            UnexpectedCachedResource,
            releases.Add,
            out nint bitmapNoReference,
            out nint bitmapCache);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|0|0|12",
            $"{result}|{bitmapNoReference}|{bitmapCache}|{string.Join('|', releases)}");
    }

    private static int MissingBitmap(out nint bitmap)
    {
        bitmap = 0;
        return Direct3D9Factory.NoInterfaceHResult;
    }

    private static int UnexpectedCacheIndex(out uint cacheIndex)
    {
        Assert.Fail("Cache index lookup should not be called.");
        cacheIndex = 0;
        return Direct3D9Factory.GenericFailureHResult;
    }

    private static int UnexpectedCachedResource(nint resourceCache, uint cacheIndex, out nint cachedResource)
    {
        Assert.Fail($"Cached resource lookup should not be called: {resourceCache}, {cacheIndex}.");
        cachedResource = 0;
        return Direct3D9Factory.GenericFailureHResult;
    }
}
