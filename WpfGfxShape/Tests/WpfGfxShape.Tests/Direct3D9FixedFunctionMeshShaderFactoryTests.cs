using System.Numerics;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9FixedFunctionMeshShaderFactoryTests
{
    [DataTestMethod]
    [DataRow(0, 0, 1, 1u)]
    [DataRow(1, 2, 2, 0u)]
    [DataRow(2, 2, 3, 0u)]
    public void WhenDerivingSupportedShaderThenNativeDispatchAndOwnershipOrderArePreserved(
        int shaderTypeValue,
        int expectedCompositingModeValue,
        int expectedLightingValuesValue,
        uint expectedZWrite)
    {
        Direct3D9MeshShaderType shaderType = (Direct3D9MeshShaderType) shaderTypeValue;
        MilCompositingMode expectedCompositingMode = (MilCompositingMode) expectedCompositingModeValue;
        Direct3D9FixedFunctionLightingValues expectedLightingValues = (Direct3D9FixedFunctionLightingValues) expectedLightingValuesValue;
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(calls);
        realizer.SetBrush(3, 5, skipMetaFixups: true);
        calls.Clear();
        Direct3D9FixedFunctionMeshShaderFactory factory = CreateFactory(calls);

        int result = factory.Derive(
            shaderType,
            (out Direct3D9ImmediateBrushRealizer? surfaceSource) =>
            {
                calls.Add("GetSurfaceSource");
                surfaceSource = realizer;
                return 0;
            },
            CreateProjectedState(),
            out Direct3D9DerivedMeshShader? shader);
        int beginResult = shader!.Begin();
        int drawResult = shader.FixedFunctionDrawMesh3D();
        int finishResult = shader.Finish();
        shader.Dispose();

        Assert.AreEqual(
            (0, 0, 0, 0,
                $"GetSurfaceSource|DeriveBrush:3:8|AddRefEffect:5|Release:5|Release:3|RSZwriteenable:{expectedZWrite}|BrushOperations|Effects:5:8|Draw:{expectedCompositingMode}:{expectedLightingValues}|ReleaseBrush|ReleaseEffect:5"),
            (result, beginResult, drawResult, finishResult, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenRealizedBrushIsNullThenTransparentBrushIsDerivedAndDepthPassIsRetained()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(calls);
        calls.Clear();
        Direct3D9FixedFunctionMeshShaderFactory factory = CreateFactory(calls);

        int result = factory.Derive(
            Direct3D9MeshShaderType.Diffuse,
            (out Direct3D9ImmediateBrushRealizer? surfaceSource) =>
            {
                surfaceSource = realizer;
                return 0;
            },
            CreateProjectedState(),
            out Direct3D9DerivedMeshShader? shader);
        shader!.Dispose();

        Assert.AreEqual((0, "DeriveBrush:11:8|ReleaseBrush"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSurfaceSourceFailsAfterReturningRealizerThenRealizerIsReleasedAndBrushIsNotDerived()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(calls);
        realizer.SetBrush(3, 5, skipMetaFixups: true);
        calls.Clear();
        Direct3D9FixedFunctionMeshShaderFactory factory = CreateFactory(calls);

        int result = factory.Derive(
            Direct3D9MeshShaderType.Diffuse,
            (out Direct3D9ImmediateBrushRealizer? surfaceSource) =>
            {
                calls.Add("GetSurfaceSource");
                surfaceSource = realizer;
                return Direct3D9Factory.GenericFailureHResult;
            },
            CreateProjectedState(),
            out Direct3D9DerivedMeshShader? shader);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, null, "GetSurfaceSource|Release:5|Release:3"),
            (result, shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenHardwareBrushDerivationFailsAfterReturningBrushThenBrushAndRealizerAreReleasedInOrder()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(calls);
        realizer.SetBrush(3, 5, skipMetaFixups: true);
        calls.Clear();
        Direct3D9FixedFunctionMeshShaderFactory factory = CreateFactory(
            calls,
            deriveHardwareBrush: (nint brush, Direct3D9ProjectedMeshState _, out Direct3D9MeshHardwareBrush? hardwareBrush) =>
            {
                calls.Add($"DeriveBrush:{brush}");
                hardwareBrush = new Direct3D9MeshHardwareBrush(static _ => 0, () => calls.Add("ReleaseBrush"));
                return Direct3D9Factory.GenericFailureHResult;
            });

        int result = factory.Derive(
            Direct3D9MeshShaderType.Specular,
            (out Direct3D9ImmediateBrushRealizer? surfaceSource) =>
            {
                surfaceSource = realizer;
                return 0;
            },
            CreateProjectedState(),
            out Direct3D9DerivedMeshShader? shader);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, null, "DeriveBrush:3|ReleaseBrush|Release:5|Release:3"),
            (result, shader, string.Join('|', calls)));
    }

    private static Direct3D9FixedFunctionMeshShaderFactory CreateFactory(
        List<string> calls,
        Direct3D9DeriveMeshHardwareBrush? deriveHardwareBrush = null)
    {
        return new Direct3D9FixedFunctionMeshShaderFactory(
            (state, value) =>
            {
                calls.Add($"{state}:{value}");
                return 0;
            },
            deriveHardwareBrush ?? ((nint brush, Direct3D9ProjectedMeshState context, out Direct3D9MeshHardwareBrush? hardwareBrush) =>
            {
                calls.Add($"DeriveBrush:{brush}:{context.RenderBoundsDeviceSpace.Right}");
                hardwareBrush = new Direct3D9MeshHardwareBrush(
                    builder =>
                    {
                        calls.Add("BrushOperations");
                        return builder.SetConstant(new Direct3D9ConstantColorSource(new MilColorF(1, 1, 1, 1)));
                    },
                    () => calls.Add("ReleaseBrush"));
                return 0;
            }),
            (passInputs, compositingMode, lightingValues) =>
            {
                int result = passInputs.CreateItems(out IReadOnlyList<Direct3D9FixedFunctionPipelineItem>? items);
                Direct3D9FixedFunctionPipelineItemBuilder.ReleaseOwnedColorSources(items ?? []);
                calls.Add($"Draw:{compositingMode}:{lightingValues}");
                return result;
            },
            effects => calls.Add($"AddRefEffect:{effects}"),
            effects => calls.Add($"ReleaseEffect:{effects}"),
            zBufferEnabled: true,
            (effects, context, _) =>
            {
                calls.Add($"Effects:{effects}:{context.RenderBoundsDeviceSpace.Right}");
                return 0;
            });
    }

    private static Direct3D9ImmediateBrushRealizer CreateRealizer(List<string> calls)
    {
        return new Direct3D9ImmediateBrushRealizer(
            11,
            value => calls.Add($"AddRef:{value}"),
            value => calls.Add($"Release:{value}"),
            (brush, color) => calls.Add($"Color:{brush}:{color.Alpha}:{color.Red}:{color.Green}:{color.Blue}"),
            static _ => false,
            static _ => Direct3D9BrushType.Solid,
            static _ => false,
            static _ => false,
            static _ => 0,
            static _ => 0,
            static (_, _, _, _) => 0,
            static (_, _, _, _) => 0,
            static (_, _) => { },
            static (_, _) => { });
    }

    private static Direct3D9ProjectedMeshState CreateProjectedState() => new(
        Matrix4x4.Identity,
        new Direct3D9SurfaceRect(0, 0, 8, 9),
        new MilRectF(0, 0, 1, 1),
        IsVisible: true);
}
