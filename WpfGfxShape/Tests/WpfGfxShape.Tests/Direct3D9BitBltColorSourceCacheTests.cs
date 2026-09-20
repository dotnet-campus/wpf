using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitBltColorSourceCacheTests
{
    [TestMethod]
    public void WhenCreationSucceedsThenCallerAndCacheOwnReferences()
    {
        List<string> calls = [];
        using Direct3D9BitBltColorSourceCache cache = CreateCache(calls);

        int result = cache.TryCreate(
            (out nint colorSource) =>
            {
                calls.Add("create");
                colorSource = 71;
                return Direct3D9Factory.SuccessHResult;
            },
            out nint colorSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|71|71|create|add:71",
            $"{result}|{colorSource}|{cache.ColorSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenCacheAlreadyHasColorSourceThenCreationIsRejected()
    {
        List<string> calls = [];
        using Direct3D9BitBltColorSourceCache cache = CreateCache(calls);
        cache.TryCreate(CreateColorSource, out _);

        int result = cache.TryCreate(
            (out nint colorSource) =>
            {
                calls.Add("unexpected-create");
                colorSource = 72;
                return Direct3D9Factory.SuccessHResult;
            },
            out nint colorSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.UnexpectedHResult}|0|add:71",
            $"{result}|{colorSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenCreationFailsWithColorSourceThenReturnedReferenceIsReleased()
    {
        List<string> calls = [];
        using Direct3D9BitBltColorSourceCache cache = CreateCache(calls);

        int result = cache.TryCreate(
            (out nint colorSource) =>
            {
                calls.Add("create");
                colorSource = 71;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            out nint colorSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.OutOfMemoryHResult}|0|0|create|release:71",
            $"{result}|{colorSource}|{cache.ColorSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenCreationSucceedsWithoutColorSourceThenGenericFailureIsReturned()
    {
        using Direct3D9BitBltColorSourceCache cache = CreateCache([]);

        int result = cache.TryCreate(
            (out nint colorSource) =>
            {
                colorSource = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            out nint colorSource);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0),
            (result, colorSource, cache.ColorSource));
    }

    [TestMethod]
    public void WhenDisposedThenCachedReferenceIsReleasedOnceAndMembersAreProtected()
    {
        List<string> calls = [];
        Direct3D9BitBltColorSourceCache cache = CreateCache(calls);
        cache.TryCreate(CreateColorSource, out _);

        cache.Dispose();
        cache.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = cache.ColorSource);
        Assert.ThrowsExactly<ObjectDisposedException>(() => cache.TryCreate(CreateColorSource, out _));
        Assert.AreEqual("add:71|release:71", string.Join('|', calls));
    }

    private static Direct3D9BitBltColorSourceCache CreateCache(List<string> calls) =>
        new(
            colorSource => calls.Add($"add:{colorSource}"),
            colorSource => calls.Add($"release:{colorSource}"));

    private static int CreateColorSource(out nint colorSource)
    {
        colorSource = 71;
        return Direct3D9Factory.SuccessHResult;
    }
}
