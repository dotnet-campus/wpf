extern alias Stage3Production;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class IndependentVideoFrameLeaseTests
{
    [TestMethod]
    public void WhenRealMfFramesAreLeasedThenManagedProductionBitmapConsumptionSurvivesReplacementAndClose()
        => new Stage3Production::WpfGfxShape.Tests.MediaFoundationResourceTests()
            .WhenDecodedFramesReplaceThenResourceLeasesSurviveOwnerClose();
}
