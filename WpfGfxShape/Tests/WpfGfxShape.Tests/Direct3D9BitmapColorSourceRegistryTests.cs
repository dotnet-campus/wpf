using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapColorSourceRegistryTests
{
    [TestMethod]
    public void WhenPipelineColorSourceIsOwnedThenRegistryResolvesProductionOwner()
    {
        Direct3D9BitmapColorSourceRegistry registry = new();
        Direct3D9BitmapColorSourceTextureRealizer textureRealizer = new(
            () => (Direct3D9Factory.NotImplementedHResult, null));
        using Direct3D9BitmapPipelineColorSource pipelineColorSource = new(
            17,
            textureRealizer,
            new Direct3D9PipelineColorSource(
                Direct3D9ColorSourceType.Texture,
                () => Direct3D9Factory.SuccessHResult),
            _ => { },
            registry,
            isDeviceBitmap: true);

        Direct3D9BitmapColorSourceOwner owner = registry.Resolve(17);

        Assert.AreEqual(
            $"{true}|{true}",
            $"{ReferenceEquals(textureRealizer, owner.TextureRealizer)}|{owner.IsDeviceBitmap}");
    }

    [TestMethod]
    public void WhenPipelineColorSourceIsDisposedThenRegistryNoLongerResolvesOwner()
    {
        Direct3D9BitmapColorSourceRegistry registry = new();
        Direct3D9BitmapPipelineColorSource pipelineColorSource = new(
            17,
            new Direct3D9BitmapColorSourceTextureRealizer(
                () => (Direct3D9Factory.NotImplementedHResult, null)),
            new Direct3D9PipelineColorSource(
                Direct3D9ColorSourceType.Texture,
                () => Direct3D9Factory.SuccessHResult),
            _ => { },
            registry);

        pipelineColorSource.Dispose();

        Assert.ThrowsExactly<InvalidOperationException>(() => registry.Resolve(17));
    }

    [TestMethod]
    public void WhenOwnerHasNotRealizedThenRealizationStateResolutionIsRejected()
    {
        Direct3D9BitmapColorSourceOwner owner = new(
            new Direct3D9BitmapColorSourceTextureRealizer(
                () => (Direct3D9Factory.NotImplementedHResult, null)),
            isDeviceBitmap: false);

        Assert.ThrowsExactly<InvalidOperationException>(() => _ = owner.RealizationState);
    }
}
