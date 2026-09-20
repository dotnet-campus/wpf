using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapCacheLifetimeTests
{
    [TestMethod]
    public void WhenConstructedThenBitmapAndDeviceAssociationsAreNonOwning()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheLifetime cache = CreateCache(calls);

        Assert.AreEqual("17|23|", $"{cache.BitmapNoReference}|{cache.DeviceNoReference}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenBitmapSourceIsAssociatedForFirstTimeThenCacheIsNotCleared()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheLifetime cache = CreateCache(calls);

        cache.AssociateBitmapSource(29);

        Assert.AreEqual("29|", $"{cache.BitmapSourceNoReference}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenSameBitmapSourceIsAssociatedAgainThenCacheIsNotCleared()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheLifetime cache = CreateCache(calls);
        cache.AssociateBitmapSource(29);

        cache.AssociateBitmapSource(29);

        Assert.AreEqual("29|", $"{cache.BitmapSourceNoReference}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenBitmapSourceAssociationChangesThenCacheIsClearedBeforeNewAssociationIsStored()
    {
        List<string> calls = [];
        Direct3D9BitmapCacheLifetime? cache = null;
        bool observeAssociation = true;
        cache = new(
            new Direct3D9ResourceManager(),
            bitmapNoReference: 17,
            deviceNoReference: 23,
            () => calls.Add(observeAssociation ? $"clear:{cache!.BitmapSourceNoReference}" : "clear"),
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
        using (cache)
        {
            cache.AssociateBitmapSource(29);

            cache.AssociateBitmapSource(31);

            Assert.AreEqual("31|clear:29", $"{cache.BitmapSourceNoReference}|{string.Join('|', calls)}");
            observeAssociation = false;
        }
    }

    [TestMethod]
    public void WhenDeviceBitmapColorSourceIsCachedThenCacheAddsOneReference()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheLifetime cache = CreateCache(calls);

        int result = cache.CacheDeviceBitmapColorSource(31);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|31|add:31",
            $"{result}|{cache.DeviceBitmapColorSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenDeviceBitmapColorSourceAlreadyExistsThenReplacementIsRejected()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheLifetime cache = CreateCache(calls);
        cache.CacheDeviceBitmapColorSource(31);

        int result = cache.CacheDeviceBitmapColorSource(37);

        Assert.AreEqual(
            $"{Direct3D9Factory.UnexpectedHResult}|31|add:31",
            $"{result}|{cache.DeviceBitmapColorSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenLastUsedColorSourceChangesThenPreviousReferenceIsReleasedBeforeNewReferenceIsAdded()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheLifetime cache = CreateCache(calls);
        cache.SetLastUsedColorSource(41);

        cache.SetLastUsedColorSource(43);

        Assert.AreEqual("43|add:41|release:41|add:43", $"{cache.LastUsedColorSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenLastUsedColorSourceIsUnchangedThenReferencesAreUnchanged()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheLifetime cache = CreateCache(calls);
        cache.SetLastUsedColorSource(41);

        cache.SetLastUsedColorSource(41);

        Assert.AreEqual("41|add:41", $"{cache.LastUsedColorSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenD3DResourcesAreReleasedThenRealizationsAndOwnedReferencesAreReleasedInNativeOrder()
    {
        List<string> calls = [];
        Direct3D9ResourceManager manager = new();
        Direct3D9BitmapCacheLifetime cache = CreateCache(calls, manager);
        cache.CacheDeviceBitmapColorSource(31);
        cache.SetLastUsedColorSource(41);
        calls.Clear();

        manager.DestroyAllResources();

        Assert.AreEqual("clear|release:41|release:31", string.Join('|', calls));
    }

    [TestMethod]
    public void WhenBothOwningSlotsContainSameColorSourceThenEachOwnedReferenceIsReleasedInNativeOrder()
    {
        List<string> calls = [];
        Direct3D9BitmapCacheLifetime cache = CreateCache(calls);
        cache.CacheDeviceBitmapColorSource(31);
        cache.SetLastUsedColorSource(31);
        calls.Clear();

        cache.Dispose();

        Assert.AreEqual("clear|release:31|release:31", string.Join('|', calls));
    }

    [TestMethod]
    public void WhenManagerDestroysCacheThenReleaseIsIdempotentAndMembersAreProtected()
    {
        List<string> calls = [];
        Direct3D9ResourceManager manager = new();
        Direct3D9BitmapCacheLifetime cache = CreateCache(calls, manager);
        cache.CacheDeviceBitmapColorSource(31);
        cache.SetLastUsedColorSource(31);
        calls.Clear();

        manager.DestroyAllResources();
        cache.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = cache.BitmapNoReference);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = cache.DeviceNoReference);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = cache.BitmapSourceNoReference);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = cache.DeviceBitmapColorSource);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = cache.LastUsedColorSource);
        Assert.ThrowsExactly<ObjectDisposedException>(() => cache.AssociateBitmapSource(29));
        Assert.ThrowsExactly<ObjectDisposedException>(() => cache.CacheDeviceBitmapColorSource(37));
        Assert.ThrowsExactly<ObjectDisposedException>(() => cache.SetLastUsedColorSource(43));
        Assert.AreEqual("clear|release:31|release:31", string.Join('|', calls));
    }

    [TestMethod]
    public void WhenDisposedThenReleaseIsIdempotentAndMembersAreProtected()
    {
        List<string> calls = [];
        Direct3D9BitmapCacheLifetime cache = CreateCache(calls);
        cache.CacheDeviceBitmapColorSource(31);
        cache.SetLastUsedColorSource(41);

        cache.Dispose();
        cache.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = cache.BitmapNoReference);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = cache.DeviceNoReference);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = cache.BitmapSourceNoReference);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = cache.DeviceBitmapColorSource);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = cache.LastUsedColorSource);
        Assert.ThrowsExactly<ObjectDisposedException>(() => cache.AssociateBitmapSource(29));
        Assert.ThrowsExactly<ObjectDisposedException>(() => cache.CacheDeviceBitmapColorSource(37));
        Assert.ThrowsExactly<ObjectDisposedException>(() => cache.SetLastUsedColorSource(43));
        Assert.AreEqual("add:31|add:41|clear|release:41|release:31", string.Join('|', calls));
    }

    private static Direct3D9BitmapCacheLifetime CreateCache(
        List<string> calls,
        Direct3D9ResourceManager? manager = null) =>
        new(
            manager ?? new Direct3D9ResourceManager(),
            bitmapNoReference: 17,
            deviceNoReference: 23,
            () => calls.Add("clear"),
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
}
