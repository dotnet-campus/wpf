using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9RenderStateTests
{
    [TestMethod]
    public void WhenCapabilitiesAreSupportedThenConditionalStatesFollowNativeOrder()
    {
        List<(Renderstatetype State, uint Value)> calls = [];
        Caps9 capabilities = default;
        capabilities.SrcBlendCaps = (uint) D3D9.PblendcapsBlendfactor;
        capabilities.RasterCaps = (uint) D3D9.PrastercapsScissortest;

        int result = Direct3D9RenderState.Initialize(
            capabilities,
            (state, value) =>
            {
                calls.Add((state, value));
                return 0;
            });

        Assert.AreEqual(0, result);
        Assert.AreEqual((Renderstatetype.Blendfactor, 0u), calls[0]);
        Assert.AreEqual((Renderstatetype.Zenable, 0u), calls[1]);
        Assert.AreEqual((Renderstatetype.Blendop, 1u), calls[^2]);
        Assert.AreEqual((Renderstatetype.Scissortestenable, 0u), calls[^1]);
    }

    [TestMethod]
    public void WhenDepthStencilSurfaceIsClearedThenItFollowsAmbientMaterialSource()
    {
        List<string> calls = [];

        int result = Direct3D9RenderState.Initialize(
            default,
            (state, _) =>
            {
                calls.Add(state.ToString());
                return 0;
            },
            () =>
            {
                calls.Add("depth-stencil");
                return 0;
            });

        int depthStencilIndex = calls.IndexOf("depth-stencil");
        Assert.AreEqual(
            (Renderstatetype.Ambientmaterialsource.ToString(), "depth-stencil", Renderstatetype.Ambient.ToString()),
            (calls[depthStencilIndex - 1], calls[depthStencilIndex], calls[depthStencilIndex + 1]));
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenDepthStencilSurfaceClearFailsThenLaterRenderStatesAreSkipped()
    {
        List<Renderstatetype> calls = [];

        int result = Direct3D9RenderState.Initialize(
            default,
            (state, _) =>
            {
                calls.Add(state);
                return 0;
            },
            () => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual(Renderstatetype.Ambientmaterialsource, calls[^1]);
    }

    [TestMethod]
    public void WhenConditionalCapabilitiesAreMissingThenConditionalStatesAreSkipped()
    {
        List<Renderstatetype> calls = [];

        int result = Direct3D9RenderState.Initialize(
            default,
            (state, _) =>
            {
                calls.Add(state);
                return 0;
            });

        Assert.AreEqual(0, result);
        CollectionAssert.DoesNotContain(calls, Renderstatetype.Blendfactor);
        CollectionAssert.DoesNotContain(calls, Renderstatetype.Scissortestenable);
    }

    [TestMethod]
    public void WhenSettingAStateFailsThenFirstFailureIsReturnedAndLaterStatesAreSkipped()
    {
        List<Renderstatetype> calls = [];

        int result = Direct3D9RenderState.Initialize(
            default,
            (state, _) =>
            {
                calls.Add(state);
                return state == Renderstatetype.Cullmode
                    ? Direct3D9Factory.GenericFailureHResult
                    : 0;
            });

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual(Renderstatetype.Cullmode, calls[^1]);
    }
}
