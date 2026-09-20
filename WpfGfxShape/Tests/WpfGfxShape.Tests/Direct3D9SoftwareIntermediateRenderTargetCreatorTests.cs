using System.Collections.Immutable;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public class Direct3D9SoftwareIntermediateRenderTargetCreatorTests
{
    [TestMethod]
    public void WhenNoDisplayIsAssociatedThenAllCurrentDisplaysAreDisabled()
    {
        using Direct3D9DisplaySet displaySet = CreateDisplaySet(10, 20, 30);
        using Direct3D9SoftwareIntermediateRenderTargetCreator creator = CreateCreator(null, displaySet);
        bool[] enabledDisplays = [true, true, true];

        int result = creator.ReadEnabledDisplays(enabledDisplays);

        CollectionAssert.AreEqual(new[] { false, false, false }, enabledDisplays);
        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);
    }

    [DataTestMethod]
    [DataRow(10u, 0)]
    [DataRow(20u, 1)]
    [DataRow(30u, 2)]
    public void WhenDisplayIsAssociatedThenItsCurrentIndexIsEnabled(uint displayId, int expectedIndex)
    {
        using Direct3D9DisplaySet displaySet = CreateDisplaySet(10, 20, 30);
        using Direct3D9SoftwareIntermediateRenderTargetCreator creator = CreateCreator(displayId, displaySet);
        bool[] enabledDisplays = new bool[3];

        int result = creator.ReadEnabledDisplays(enabledDisplays);

        Assert.AreEqual((Direct3D9Factory.SuccessHResult, expectedIndex), (result, Array.IndexOf(enabledDisplays, true)));
    }

    [TestMethod]
    public void WhenDisplayIdIsUnknownThenFirstErrorIsReturnedWithoutChangingOutputAndDisplaySetIsReleased()
    {
        using Direct3D9DisplaySet displaySet = CreateDisplaySet(10, 20, 30);
        int releaseCount = 0;
        using Direct3D9SoftwareIntermediateRenderTargetCreator creator = new(
            MilPixelFormat.Pbgra32Bpp,
            99,
            (out Direct3D9DisplaySet? currentDisplaySet) =>
            {
                currentDisplaySet = displaySet;
                return Direct3D9Factory.SuccessHResult;
            },
            _ => releaseCount++);
        bool[] enabledDisplays = [true, false, true];

        int result = creator.ReadEnabledDisplays(enabledDisplays);

        CollectionAssert.AreEqual(new[] { true, false, true }, enabledDisplays);
        Assert.AreEqual((Direct3D9Factory.InvalidArgumentHResult, 1), (result, releaseCount));
    }

    [TestMethod]
    public void WhenEnabledDisplayCountDoesNotMatchCurrentDisplayCountThenOutputIsUnchanged()
    {
        using Direct3D9DisplaySet displaySet = CreateDisplaySet(10, 20, 30);
        using Direct3D9SoftwareIntermediateRenderTargetCreator creator = CreateCreator(20, displaySet);
        bool[] enabledDisplays = [true, false];

        int result = creator.ReadEnabledDisplays(enabledDisplays);

        CollectionAssert.AreEqual(new[] { true, false }, enabledDisplays);
        Assert.AreEqual(Direct3D9Factory.InvalidArgumentHResult, result);
    }

    [TestMethod]
    public void WhenCurrentDisplaySetIsEmptyThenEmptyOutputSucceeds()
    {
        using Direct3D9DisplaySet displaySet = CreateDisplaySet();
        using Direct3D9SoftwareIntermediateRenderTargetCreator creator = CreateCreator(null, displaySet);

        int result = creator.ReadEnabledDisplays([]);

        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);
    }

    [TestMethod]
    public void WhenCurrentDisplaySetQueryFailsWithAReferenceThenFirstErrorIsReturnedAndReferenceIsReleased()
    {
        using Direct3D9DisplaySet displaySet = CreateDisplaySet(10);
        int releaseCount = 0;
        using Direct3D9SoftwareIntermediateRenderTargetCreator creator = new(
            MilPixelFormat.Pbgra32Bpp,
            10,
            (out Direct3D9DisplaySet? currentDisplaySet) =>
            {
                currentDisplaySet = displaySet;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            _ => releaseCount++);
        bool[] enabledDisplays = [true];

        int result = creator.ReadEnabledDisplays(enabledDisplays);

        Assert.AreEqual((Direct3D9Factory.OutOfMemoryHResult, true, 1), (result, enabledDisplays[0], releaseCount));
    }

    [TestMethod]
    public void WhenDisposedThenEnabledDisplayQueryThrowsObjectDisposedException()
    {
        using Direct3D9DisplaySet displaySet = CreateDisplaySet(10);
        Direct3D9SoftwareIntermediateRenderTargetCreator creator = CreateCreator(10, displaySet);
        creator.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => creator.ReadEnabledDisplays(new bool[1]));
    }

    [TestMethod]
    public void WhenSurfaceReadsEnabledDisplaysThenItUsesItsIntermediateCreatorDisplayState()
    {
        using Direct3D9DisplaySet displaySet = CreateDisplaySet(10, 20, 30);
        int releaseCount = 0;
        using Direct3D9SoftwareRenderTargetSurface surface = new(
            1,
            1,
            (out byte[] pixels, out int stride) =>
            {
                pixels = new byte[4];
                stride = 4;
                return Direct3D9Factory.SuccessHResult;
            },
            () => { },
            (out Direct3D9Software3DSurface? software3DSurface) =>
            {
                software3DSurface = null;
                return Direct3D9Factory.NotAvailableHResult;
            },
            _ => { },
            _ => { },
            associatedDisplayIndex: 20,
            acquireCurrentDisplaySet: (out Direct3D9DisplaySet? currentDisplaySet) =>
            {
                currentDisplaySet = displaySet;
                return Direct3D9Factory.SuccessHResult;
            },
            releaseDisplaySet: _ => releaseCount++);
        bool[] enabledDisplays = new bool[3];

        int result = surface.ReadEnabledDisplays(enabledDisplays);

        Assert.AreEqual((Direct3D9Factory.SuccessHResult, 1, 1), (result, Array.IndexOf(enabledDisplays, true), releaseCount));
    }

    private static Direct3D9SoftwareIntermediateRenderTargetCreator CreateCreator(
        uint? associatedDisplayId,
        Direct3D9DisplaySet displaySet)
    {
        return new Direct3D9SoftwareIntermediateRenderTargetCreator(
            MilPixelFormat.Pbgra32Bpp,
            associatedDisplayId,
            (out Direct3D9DisplaySet? currentDisplaySet) =>
            {
                currentDisplaySet = displaySet;
                return Direct3D9Factory.SuccessHResult;
            });
    }

    private static Direct3D9DisplaySet CreateDisplaySet(params uint[] displayIds)
    {
        ImmutableArray<Direct3D9Display>.Builder displays = ImmutableArray.CreateBuilder<Direct3D9Display>(displayIds.Length);
        foreach (uint displayId in displayIds)
        {
            displays.Add(default(Direct3D9Display) with { DisplayIndex = displayId });
        }

        return new Direct3D9DisplaySet(
            new Direct3D9DisplaySetCharacteristics(0, 0, false, [], displays.MoveToImmutable()),
            0,
            0,
            () => 0,
            () => 0);
    }
}
