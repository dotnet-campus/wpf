using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9MultisampleSupportTests
{
    [TestMethod]
    public void WhenWddmDeviceSupportsFourSamplesThenAllFormatsUseFourSamples()
    {
        List<(Format Format, MultisampleType Type)> calls = [];

        Direct3D9MultisampleSupport result = Direct3D9MultisampleSupportFactory.Gather(
            adapterOrdinal: 2,
            Devtype.Hal,
            hasWddmSupport: true,
            configuredMaximum: null,
            (adapter, deviceType, format, windowed, type) =>
            {
                Assert.AreEqual((2u, Devtype.Hal, true), (adapter, deviceType, windowed));
                calls.Add((format, type));
                return 0;
            });

        Assert.AreEqual(
            new Direct3D9MultisampleSupport(
                MultisampleType.Multisample4Samples,
                MultisampleType.Multisample4Samples,
                MultisampleType.Multisample4Samples),
            result);
    }

    [TestMethod]
    public void WhenDeviceIsNotWddmAndRegistryHasNoOverrideThenNoQueriesAreMade()
    {
        int callCount = 0;

        Direct3D9MultisampleSupport result = Direct3D9MultisampleSupportFactory.Gather(
            adapterOrdinal: 0,
            Devtype.Hal,
            hasWddmSupport: false,
            configuredMaximum: null,
            (_, _, _, _, _) =>
            {
                callCount++;
                return 0;
            });

        Assert.AreEqual((default(Direct3D9MultisampleSupport), 0), (result, callCount));
    }

    [TestMethod]
    public void WhenDepthSupportsOnlyTwoSamplesThenTargetQueriesDoNotExceedTwoSamples()
    {
        List<(Format Format, MultisampleType Type)> calls = [];

        Direct3D9MultisampleSupport result = Direct3D9MultisampleSupportFactory.Gather(
            adapterOrdinal: 0,
            Devtype.Hal,
            hasWddmSupport: false,
            configuredMaximum: 4,
            (_, _, format, _, type) =>
            {
                calls.Add((format, type));
                return format == Format.D24S8 && type > MultisampleType.Multisample2Samples
                    ? Direct3D9Factory.GenericFailureHResult
                    : 0;
            });

        Assert.AreEqual(
            new Direct3D9MultisampleSupport(
                MultisampleType.Multisample2Samples,
                MultisampleType.Multisample2Samples,
                MultisampleType.Multisample2Samples),
            result);
    }

    [TestMethod]
    public void WhenTargetFormatsDifferThenEachFormatFallsBackIndependently()
    {
        Direct3D9MultisampleSupport result = Direct3D9MultisampleSupportFactory.Gather(
            adapterOrdinal: 0,
            Devtype.Hal,
            hasWddmSupport: true,
            configuredMaximum: null,
            (_, _, format, _, type) => format switch
            {
                Format.X8R8G8B8 when type > MultisampleType.Multisample3Samples => Direct3D9Factory.GenericFailureHResult,
                Format.A8R8G8B8 when type > MultisampleType.Multisample2Samples => Direct3D9Factory.GenericFailureHResult,
                Format.A2R10G10B10 => Direct3D9Factory.GenericFailureHResult,
                _ => 0,
            });

        Assert.AreEqual(
            new Direct3D9MultisampleSupport(
                MultisampleType.Multisample3Samples,
                MultisampleType.Multisample2Samples,
                MultisampleType.MultisampleNone),
            result);
    }

    [TestMethod]
    public void WhenMaximumIsSixteenThenDepthIsFilteredBeforeEachTarget()
    {
        List<(Format Format, MultisampleType Type)> calls = [];

        _ = Direct3D9MultisampleSupportFactory.Gather(
            adapterOrdinal: 0,
            Devtype.Hal,
            hasWddmSupport: false,
            configuredMaximum: 16,
            (_, _, format, _, type) =>
            {
                calls.Add((format, type));
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                (Format.D24S8, MultisampleType.Multisample16Samples),
                (Format.X8R8G8B8, MultisampleType.Multisample16Samples),
                (Format.D24S8, MultisampleType.Multisample16Samples),
                (Format.A8R8G8B8, MultisampleType.Multisample16Samples),
                (Format.D24S8, MultisampleType.Multisample16Samples),
                (Format.A2R10G10B10, MultisampleType.Multisample16Samples),
                (Format.D24S8, MultisampleType.Multisample16Samples),
            },
            calls);
    }

    [DataTestMethod]
    [DataRow(0u)]
    [DataRow(1u)]
    public void WhenConfiguredMaximumIsBelowTwoThenNoQueriesAreMade(uint configuredMaximum)
    {
        int callCount = 0;

        Direct3D9MultisampleSupport result = Direct3D9MultisampleSupportFactory.Gather(
            adapterOrdinal: 0,
            Devtype.Hal,
            hasWddmSupport: true,
            configuredMaximum,
            (_, _, _, _, _) =>
            {
                callCount++;
                return 0;
            });

        Assert.AreEqual((default(Direct3D9MultisampleSupport), 0), (result, callCount));
    }
}
