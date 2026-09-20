namespace WpfGfxShape.Core;

internal delegate int Direct3D9CreateBitmapCache(out nint bitmapCache);
internal delegate int Direct3D9SetCachedBitmapResource(nint resourceCache, uint cacheIndex, nint bitmapCache);

internal static class Direct3D9BitmapCacheResolver
{
    internal static int GetOrCreate(
        nint resourceCacheNoReference,
        bool setResourceRequired,
        Direct3D9GetBitmapCacheIndex getCacheIndex,
        Direct3D9GetCachedBitmapResource getCachedResource,
        Direct3D9CreateBitmapCache createBitmapCache,
        Direct3D9SetCachedBitmapResource setCachedResource,
        Action<nint> release,
        out nint bitmapCache)
    {
        ArgumentNullException.ThrowIfNull(getCacheIndex);
        ArgumentNullException.ThrowIfNull(getCachedResource);
        ArgumentNullException.ThrowIfNull(createBitmapCache);
        ArgumentNullException.ThrowIfNull(setCachedResource);
        ArgumentNullException.ThrowIfNull(release);

        bitmapCache = 0;
        if (resourceCacheNoReference == 0)
        {
            return Direct3D9Factory.NotImplementedHResult;
        }

        int result = getCacheIndex(out uint cacheIndex);
        if (result < 0)
        {
            return result;
        }

        result = getCachedResource(resourceCacheNoReference, cacheIndex, out nint cachedResource);
        if (result < 0)
        {
            if (cachedResource != 0)
            {
                release(cachedResource);
            }

            return result;
        }

        if (cachedResource != 0)
        {
            bitmapCache = cachedResource;
            return result;
        }

        result = createBitmapCache(out nint createdCache);
        if (result < 0)
        {
            if (createdCache != 0)
            {
                release(createdCache);
            }

            return result;
        }

        if (createdCache == 0)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        result = setCachedResource(resourceCacheNoReference, cacheIndex, createdCache);
        if (result < 0 && setResourceRequired)
        {
            release(createdCache);
            return result;
        }

        bitmapCache = createdCache;
        return Direct3D9Factory.SuccessHResult;
    }
}
