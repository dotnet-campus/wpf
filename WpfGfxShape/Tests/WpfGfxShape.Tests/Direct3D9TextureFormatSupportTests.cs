using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9TextureFormatSupportTests
{
    [TestMethod]
    public void WhenAllFormatsAreSupportedThenNativeMappingsAreUsed()
    {
        List<Format> calls = [];

        Direct3D9TextureFormatSupport result = Direct3D9TextureFormatSupportFactory.Gather(
            adapterOrdinal: 2,
            Devtype.Hal,
            Format.X8R8G8B8,
            (adapter, deviceType, adapterFormat, usage, resourceType, checkedFormat) =>
            {
                Assert.AreEqual(
                    (2u, Devtype.Hal, Format.X8R8G8B8, 0u, Resourcetype.Texture),
                    (adapter, deviceType, adapterFormat, usage, resourceType));
                calls.Add(checkedFormat);
                return 0;
            });

        Assert.AreEqual(
            new Direct3D9TextureFormatSupport(
                SupportsA8: true,
                SupportsP8: true,
                SupportsL8: true,
                MilPixelFormat.Prgba128BppFloat,
                MilPixelFormat.Prgba128BppFloat,
                MilPixelFormat.Bgr32Bpp101010,
                MilPixelFormat.Pbgra32Bpp,
                MilPixelFormat.Bgr32Bpp),
            result);
        CollectionAssert.AreEqual(
            new[]
            {
                Format.A8,
                Format.P8,
                Format.L8,
                Format.A32B32G32R32f,
                Format.A2R10G10B10,
                Format.A8R8G8B8,
                Format.X8R8G8B8
            },
            calls);
    }

    [TestMethod]
    [DataRow(false, false, false)]
    [DataRow(false, false, true)]
    [DataRow(false, true, false)]
    [DataRow(false, true, true)]
    [DataRow(true, false, false)]
    [DataRow(true, false, true)]
    [DataRow(true, true, false)]
    [DataRow(true, true, true)]
    public void WhenTextTextureFormatsAreGatheredThenEachFormatIsProbedIndependently(
        bool supportsA8,
        bool supportsP8,
        bool supportsL8)
    {
        Dictionary<Format, int> callCounts = [];

        Direct3D9TextureFormatSupport result = Direct3D9TextureFormatSupportFactory.Gather(
            adapterOrdinal: uint.MaxValue,
            Devtype.SW,
            Format.R5G6B5,
            (adapter, deviceType, adapterFormat, usage, resourceType, checkedFormat) =>
            {
                Assert.AreEqual(
                    (uint.MaxValue, Devtype.SW, Format.R5G6B5, 0u, Resourcetype.Texture),
                    (adapter, deviceType, adapterFormat, usage, resourceType));
                callCounts[checkedFormat] = callCounts.GetValueOrDefault(checkedFormat) + 1;
                return checkedFormat switch
                {
                    Format.A8 when supportsA8 => 0,
                    Format.P8 when supportsP8 => 0,
                    Format.L8 when supportsL8 => 0,
                    _ => Direct3D9Factory.GenericFailureHResult
                };
            });

        Assert.AreEqual(
            (supportsA8, supportsP8, supportsL8, 1, 1, 1),
            (result.SupportsA8,
                result.SupportsP8,
                result.SupportsL8,
                callCounts[Format.A8],
                callCounts[Format.P8],
                callCounts[Format.L8]));
    }

    [TestMethod]
    public void WhenOnlyFloatFormatIsSupportedThenLowerPrecisionFormatsUseFloatFallback()
    {
        Direct3D9TextureFormatSupport result = Direct3D9TextureFormatSupportFactory.Gather(
            adapterOrdinal: 0,
            Devtype.Hal,
            Format.X8R8G8B8,
            (_, _, _, _, _, checkedFormat) => checkedFormat == Format.A32B32G32R32f
                ? 0
                : Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            new Direct3D9TextureFormatSupport(
                SupportsA8: false,
                SupportsP8: false,
                SupportsL8: false,
                MilPixelFormat.Prgba128BppFloat,
                MilPixelFormat.Prgba128BppFloat,
                MilPixelFormat.Prgba128BppFloat,
                MilPixelFormat.Prgba128BppFloat,
                MilPixelFormat.Prgba128BppFloat),
            result);
    }

    [TestMethod]
    public void WhenAllFormatProbesFailThenNativeDefaultsArePreserved()
    {
        Direct3D9TextureFormatSupport result = Direct3D9TextureFormatSupportFactory.Gather(
            adapterOrdinal: 0,
            Devtype.Ref,
            Format.A8R8G8B8,
            (_, _, _, _, _, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(default, result);
    }

    [TestMethod]
    public void WhenFormatProbeReturnsNonzeroSuccessThenFormatIsSupported()
    {
        Direct3D9TextureFormatSupport result = Direct3D9TextureFormatSupportFactory.Gather(
            adapterOrdinal: 0,
            Devtype.Hal,
            Format.X8R8G8B8,
            (_, _, _, _, _, checkedFormat) => checkedFormat == Format.A32B32G32R32f
                ? 1
                : Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(MilPixelFormat.Prgba128BppFloat, result.SupportFor128BppPrgbaFloat);
    }

    [TestMethod]
    public void WhenBgrIsUnsupportedThenPbgraIsPreferredOverBgr101010()
    {
        Direct3D9TextureFormatSupport result = Direct3D9TextureFormatSupportFactory.Gather(
            adapterOrdinal: 0,
            Devtype.Hal,
            Format.X8R8G8B8,
            (_, _, _, _, _, checkedFormat) => checkedFormat is Format.A2R10G10B10 or Format.A8R8G8B8
                ? 0
                : Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(MilPixelFormat.Pbgra32Bpp, result.SupportFor32BppBgr);
    }

    [TestMethod]
    public void WhenBgrAndPbgraAreUnsupportedThenBgr101010IsUsed()
    {
        Direct3D9TextureFormatSupport result = Direct3D9TextureFormatSupportFactory.Gather(
            adapterOrdinal: 0,
            Devtype.Hal,
            Format.X8R8G8B8,
            (_, _, _, _, _, checkedFormat) => checkedFormat == Format.A2R10G10B10
                ? 0
                : Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(MilPixelFormat.Bgr32Bpp101010, result.SupportFor32BppBgr);
    }
}
