using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9DependentBitBltColorSourceRealizerTests
{
    [TestMethod]
    public void WhenColorSourceIsIndependentThenRealizeDoesNothing()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateInvalidState();
        List<string> calls = [];
        Direct3D9DependentBitBltColorSourceRealizer realizer = CreateRealizer(
            state,
            calls,
            isDependent: false);

        int result = realizer.Realize();

        Assert.AreEqual($"{Direct3D9Factory.SuccessHResult}|", $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenDependentRealizationIsValidThenDirtyRectanglesAreNotRequested()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateInvalidState();
        state.Commit(17, state.RequiredRealizationBounds);
        List<string> calls = [];
        Direct3D9DependentBitBltColorSourceRealizer realizer = CreateRealizer(state, calls);

        int result = realizer.Realize();

        Assert.AreEqual($"{Direct3D9Factory.SuccessHResult}|", $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenDirtyRectanglesAreUnavailableThenFullBitmapIsCopiedAndStateIsCommitted()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateInvalidState();
        List<string> calls = [];
        Direct3D9DependentBitBltColorSourceRealizer realizer = CreateRealizer(
            state,
            calls,
            getDirtyRectangles: (out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles, out uint uniqueness) =>
            {
                calls.Add("dirty");
                rectangles = [];
                uniqueness = 23;
                return false;
            });

        int result = realizer.Realize();

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|23|Direct3D9BitmapRealizationRectangle {{ Left = 4, Top = 5, Right = 30, Bottom = 20 }}|dirty|surface|update:71:Direct3D9BitmapRealizationRectangle {{ Left = 0, Top = 0, Right = 80, Bottom = 60 }}",
            $"{result}|{state.CachedUniquenessToken}|{state.CachedRealizationBounds}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenPrimaryTransferSurfaceDoesNotExistThenRealizeStaysDirty()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateInvalidState();
        List<string> calls = [];
        Direct3D9DependentBitBltColorSourceRealizer realizer = CreateRealizer(
            state,
            calls,
            getPrimaryTransferSurface: () =>
            {
                calls.Add("surface");
                return 0;
            });

        int result = realizer.Realize();

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|0|True|dirty|surface",
            $"{result}|{state.CachedUniquenessToken}|{!state.IsRealizationValid()}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenSurfaceUpdateFailsThenFailurePropagatesAndStateIsNotCommitted()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateInvalidState();
        List<string> calls = [];
        Direct3D9DependentBitBltColorSourceRealizer realizer = CreateRealizer(
            state,
            calls,
            updateSurface: (rectangles, surface) =>
            {
                calls.Add($"update:{surface}:{rectangles[0]}");
                return Direct3D9Factory.InvalidCallHResult;
            });

        int result = realizer.Realize();

        Assert.AreEqual(
            $"{Direct3D9Factory.InvalidCallHResult}|0|True|dirty|surface|update:71:Direct3D9BitmapRealizationRectangle {{ Left = 1, Top = 2, Right = 11, Bottom = 12 }}",
            $"{result}|{state.CachedUniquenessToken}|{!state.IsRealizationValid()}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenDirtyRectanglesAreAvailableThenTheyAreCopiedBeforeStateCommit()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateInvalidState();
        List<string> calls = [];
        Direct3D9DependentBitBltColorSourceRealizer realizer = CreateRealizer(state, calls);

        int result = realizer.Realize();

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|17|Direct3D9BitmapRealizationRectangle {{ Left = 4, Top = 5, Right = 30, Bottom = 20 }}|dirty|surface|update:71:Direct3D9BitmapRealizationRectangle {{ Left = 1, Top = 2, Right = 11, Bottom = 12 }}",
            $"{result}|{state.CachedUniquenessToken}|{state.CachedRealizationBounds}|{string.Join('|', calls)}");
    }

    private static Direct3D9BitmapColorSourceRealizationState CreateInvalidState()
    {
        Direct3D9BitmapColorSourceRealizationState state = new(
            13,
            MilPixelFormat.Pbgra32Bpp,
            _ => 17);
        state.SetRequiredRealizationBounds(new Direct3D9BitmapRealizationRectangle(4, 5, 30, 20));
        return state;
    }

    private static Direct3D9DependentBitBltColorSourceRealizer CreateRealizer(
        Direct3D9BitmapColorSourceRealizationState state,
        List<string> calls,
        bool isDependent = true,
        Direct3D9GetDependentBitBltDirtyRectangles? getDirtyRectangles = null,
        Func<nint>? getPrimaryTransferSurface = null,
        Direct3D9UpdateDependentBitBltSurface? updateSurface = null)
    {
        getDirtyRectangles ??= (out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles, out uint uniqueness) =>
        {
            calls.Add("dirty");
            rectangles = [new Direct3D9BitmapRealizationRectangle(1, 2, 11, 12)];
            uniqueness = 17;
            return true;
        };
        getPrimaryTransferSurface ??= () =>
        {
            calls.Add("surface");
            return 71;
        };
        updateSurface ??= (rectangles, surface) =>
        {
            calls.Add($"update:{surface}:{string.Join(',', rectangles.ToArray())}");
            return Direct3D9Factory.SuccessHResult;
        };

        return new Direct3D9DependentBitBltColorSourceRealizer(
            state,
            80,
            60,
            isDependent,
            getDirtyRectangles,
            getPrimaryTransferSurface,
            updateSurface);
    }
}
