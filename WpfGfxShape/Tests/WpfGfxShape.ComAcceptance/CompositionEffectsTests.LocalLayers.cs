namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionEffectsTests
{
    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(5)]
    public void WhenLocalMaskLayersAreNestedThenLockFailureAndRetryPreservePixels(int depth)
    {
        WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping(
            false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u,
            geometryFill: true, scopedGeometry: true, solidGeometry: true,
            strokePen: true, strokeOnly: true, nestedLayers: depth);
    }

    [TestMethod]
    [DataRow(1u, 1)]
    [DataRow(2u, 2)]
    [DataRow(3u, 5)]
    [DataRow(4u, 2)]
    public void WhenTiledLocalMaskLayersRetryThenNestedPixelsArePreserved(uint tileMode, int depth)
    {
        WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping(
            false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u,
            tileMode: tileMode, geometryFill: true, scopedGeometry: true, solidGeometry: true,
            strokePen: true, strokeOnly: true, nestedLayers: depth);
    }
}
