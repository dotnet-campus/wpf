namespace WpfGfxShape.Core;

internal delegate int Direct3D9QueryBitmapInterface(out nint bitmap);
internal delegate int Direct3D9QueryResourceCacheInterface(out nint resourceCache);
internal delegate int Direct3D9GetBitmapCacheIndex(out uint cacheIndex);
internal delegate int Direct3D9GetCachedBitmapResource(nint resourceCache, uint cacheIndex, out nint cachedResource);

internal static class Direct3D9BitmapCacheRetriever
{
    internal static int Retrieve(
        Direct3D9QueryBitmapInterface queryBitmap,
        Direct3D9QueryResourceCacheInterface queryResourceCache,
        Direct3D9GetBitmapCacheIndex getCacheIndex,
        Direct3D9GetCachedBitmapResource getCachedResource,
        Action<nint> release,
        out nint bitmapNoReference,
        out nint bitmapCache)
    {
        ArgumentNullException.ThrowIfNull(queryBitmap);
        ArgumentNullException.ThrowIfNull(queryResourceCache);
        ArgumentNullException.ThrowIfNull(getCacheIndex);
        ArgumentNullException.ThrowIfNull(getCachedResource);
        ArgumentNullException.ThrowIfNull(release);

        bitmapNoReference = 0;
        bitmapCache = 0;

        int result = queryBitmap(out nint queriedBitmap);
        if (result >= 0)
        {
            if (queriedBitmap == 0)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            bitmapNoReference = queriedBitmap;
            release(queriedBitmap);
        }
        else if (queriedBitmap != 0)
        {
            release(queriedBitmap);
        }

        result = queryResourceCache(out nint resourceCache);
        if (result < 0)
        {
            if (resourceCache != 0)
            {
                release(resourceCache);
            }

            bitmapNoReference = 0;
            return Direct3D9Factory.SuccessHResult;
        }

        if (resourceCache == 0)
        {
            bitmapNoReference = 0;
            return Direct3D9Factory.GenericFailureHResult;
        }

        try
        {
            result = getCacheIndex(out uint cacheIndex);
            if (result < 0)
            {
                return result;
            }

            result = getCachedResource(resourceCache, cacheIndex, out nint cachedResource);
            if (result < 0)
            {
                if (cachedResource != 0)
                {
                    release(cachedResource);
                }

                return result;
            }

            bitmapCache = cachedResource;
            return result;
        }
        finally
        {
            release(resourceCache);
        }
    }
}
