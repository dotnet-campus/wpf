using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapColorSourceRealizerTests
{
    [TestMethod]
    public void WhenRealizerIsDisposedBeforeRealizationThenPendingStateIsReleasedOnce()
    {
        int disposeCount = 0;
        Direct3D9BitmapColorSourceTextureRealizer realizer = new(
            () => throw new AssertFailedException(),
            () => disposeCount++);

        realizer.Dispose();
        realizer.Dispose();

        Assert.AreEqual(1, disposeCount);
    }

    [TestMethod]
    public void WhenRealizationConsumesPendingStateThenDisposalDoesNotReleaseItAgain()
    {
        int disposeCount = 0;
        Direct3D9BitmapColorSourceTextureRealizer realizer = new(
            () => (Direct3D9Factory.NotImplementedHResult, null),
            () => disposeCount++);

        int result = realizer.Realize();
        realizer.Dispose();

        Assert.AreEqual($"{Direct3D9Factory.NotImplementedHResult}|1", $"{result}|{disposeCount}");
    }
}
