using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapFormatCacheEntryTests
{
    [TestMethod]
    public void WhenFirstFormatIsRequestedThenHeadEntryIsAssigned()
    {
        using Direct3D9BitmapFormatCacheEntry formats = CreateFormats([]);

        Direct3D9BitmapCacheEntryList first = formats.GetOrCreateEntryList(MilPixelFormat.Pbgra32Bpp);
        Direct3D9BitmapCacheEntryList second = formats.GetOrCreateEntryList(MilPixelFormat.Pbgra32Bpp);

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void WhenDifferentFormatsAreRequestedThenEachFormatUsesItsOwnEntryList()
    {
        using Direct3D9BitmapFormatCacheEntry formats = CreateFormats([]);

        Direct3D9BitmapCacheEntryList first = formats.GetOrCreateEntryList(MilPixelFormat.Pbgra32Bpp);
        Direct3D9BitmapCacheEntryList second = formats.GetOrCreateEntryList(MilPixelFormat.Bgr32Bpp);

        Assert.AreNotSame(first, second);
    }

    [TestMethod]
    public void WhenExistingLaterFormatIsRequestedThenItsEntryListIsReused()
    {
        using Direct3D9BitmapFormatCacheEntry formats = CreateFormats([]);
        formats.GetOrCreateEntryList(MilPixelFormat.Pbgra32Bpp);
        Direct3D9BitmapCacheEntryList later = formats.GetOrCreateEntryList(MilPixelFormat.Bgr32Bpp);
        formats.GetOrCreateEntryList(MilPixelFormat.Gray8Bpp);

        Direct3D9BitmapCacheEntryList result = formats.GetOrCreateEntryList(MilPixelFormat.Bgr32Bpp);

        Assert.AreSame(later, result);
    }

    [TestMethod]
    public void WhenStoredFormatsDifferThenLookupUsesOnlyRequestedFormatEntryList()
    {
        List<string> calls = [];
        using Direct3D9BitmapFormatCacheEntry formats = CreateFormats(calls);
        Direct3D9BitmapRealizationProperties first = CreateProperties(MilPixelFormat.Pbgra32Bpp);
        Direct3D9BitmapRealizationProperties second = CreateProperties(MilPixelFormat.Bgr32Bpp);
        formats.Store(first, 31);
        formats.Store(second, 37);
        calls.Clear();

        bool acquired = formats.TryAcquire(ref second, _ => true, null, out nint colorSource);

        Assert.AreEqual("True|37|add:37", $"{acquired}|{colorSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenDisposedThenLaterEntriesAreReleasedBeforeHeadEntry()
    {
        List<string> calls = [];
        Direct3D9BitmapFormatCacheEntry formats = CreateFormats(calls);
        formats.GetOrCreateEntryList(MilPixelFormat.Pbgra32Bpp).Add(31);
        formats.GetOrCreateEntryList(MilPixelFormat.Bgr32Bpp).Add(37);
        formats.GetOrCreateEntryList(MilPixelFormat.Gray8Bpp).Add(41);
        calls.Clear();

        formats.Dispose();
        formats.Dispose();

        Assert.AreEqual("release:41|release:37|release:31", string.Join('|', calls));
    }

    [TestMethod]
    public void WhenUndefinedFormatIsRequestedThenArgumentIsRejected()
    {
        using Direct3D9BitmapFormatCacheEntry formats = CreateFormats([]);

        Assert.ThrowsExactly<ArgumentException>(() => formats.GetOrCreateEntryList(MilPixelFormat.Undefined));
    }

    [TestMethod]
    public void WhenDisposedThenMembersAreProtected()
    {
        Direct3D9BitmapFormatCacheEntry formats = CreateFormats([]);
        formats.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => formats.GetOrCreateEntryList(MilPixelFormat.Pbgra32Bpp));
    }

    private static Direct3D9BitmapRealizationProperties CreateProperties(MilPixelFormat format) =>
        new(
            MilBitmapInterpolationMode.Linear,
            Direct3D9TextureMipMapLevel.One,
            MilBitmapWrapMode.Extend,
            format,
            true,
            100,
            80,
            40,
            30)
        {
            LayoutU = new Direct3D9BitmapDimensionLayout(40, Direct3D9TexelLayout.Natural, Silk.NET.Direct3D9.Textureaddress.Clamp),
            LayoutV = new Direct3D9BitmapDimensionLayout(30, Direct3D9TexelLayout.Natural, Silk.NET.Direct3D9.Textureaddress.Clamp),
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 100, 80)
        };

    private static Direct3D9BitmapFormatCacheEntry CreateFormats(List<string> calls) =>
        new(
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
}
