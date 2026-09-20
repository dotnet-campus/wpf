using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9TextureStateTests
{
    [TestMethod]
    public void WhenClearingTexturesThenEveryValidTextureBlendStageIsClearedInOrder()
    {
        List<uint> stages = [];

        int result = Direct3D9TextureState.ClearTextures(
            4,
            stage =>
            {
                stages.Add(stage);
                return 0;
            });

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(new uint[] { 0, 1, 2, 3 }, stages);
    }

    [TestMethod]
    public void WhenClearingTextureFailsThenFirstFailureIsReturnedAndLaterStagesAreSkipped()
    {
        List<uint> stages = [];

        int result = Direct3D9TextureState.ClearTextures(
            5,
            stage =>
            {
                stages.Add(stage);
                return stage == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(new uint[] { 0, 1, 2 }, stages);
    }
}
