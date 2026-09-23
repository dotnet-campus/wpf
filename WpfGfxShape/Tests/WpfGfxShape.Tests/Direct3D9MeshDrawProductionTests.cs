using System.Numerics;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    public unsafe void WhenProductionMeshShaderPathSucceedsThenFixedFunctionIsSkippedAndShaderIsReleased()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16),
            renderTargetSurface: surface);
        Assert.AreEqual(0, renderTarget.Begin3D(new MilRectF(0, 0, 8, 8), MilAntiAliasMode.None, useZBuffer: false, z: 0));
        List<string> calls = [];

        int result = renderTarget.ProductionDrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            CreateProductionMeshOperations(calls, shaderResult: 0, fixedFunctionResult: Direct3D9Factory.GenericFailureHResult));

        Assert.AreEqual(
            (0, "Brushes|Projected|Derive|Begin|CanShader|Shader|Finish|ReleaseShader"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionMeshShaderFailsThenFixedFunctionRunsBeforeCleanup()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16),
            renderTargetSurface: surface);
        Assert.AreEqual(0, renderTarget.Begin3D(new MilRectF(0, 0, 8, 8), MilAntiAliasMode.None, useZBuffer: false, z: 0));
        List<string> calls = [];

        int result = renderTarget.ProductionDrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            CreateProductionMeshOperations(calls, shaderResult: Direct3D9Factory.NotImplementedHResult, fixedFunctionResult: 0));

        Assert.AreEqual(
            (0, "Brushes|Projected|Derive|Begin|CanShader|Shader|Fixed|Finish|ReleaseShader"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionMeshIsInvisibleThenShaderResourcesAreNotCreated()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16),
            renderTargetSurface: surface);
        Assert.AreEqual(0, renderTarget.Begin3D(new MilRectF(0, 0, 8, 8), MilAntiAliasMode.None, useZBuffer: false, z: 0));
        List<string> calls = [];
        Direct3D9ProductionMeshDrawOperations operations = CreateProductionMeshOperations(calls) with
        {
            ApplyProjectedMeshTo2DState = (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                calls.Add("Projected");
                state = new Direct3D9ProjectedMeshState(Matrix4x4.Identity, default, default, IsVisible: false);
                return 0;
            }
        };

        int result = renderTarget.ProductionDrawMesh3D(Create3DContextState(isAntialiasingEnabled: false), operations);

        Assert.AreEqual((0, "Brushes|Projected"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionMeshRunsOutsideBegin3DThenDependenciesAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.ProductionDrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            CreateProductionMeshOperations(calls));

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, string.Empty), (result, string.Join('|', calls)));
    }

    private static Direct3D9ProductionMeshDrawOperations CreateProductionMeshOperations(
        List<string> calls,
        int shaderResult = Direct3D9Factory.SuccessHResult,
        int fixedFunctionResult = Direct3D9Factory.SuccessHResult) => new(
        () =>
        {
            calls.Add("Brushes");
            return 0;
        },
        (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
        {
            calls.Add("Projected");
            state = new Direct3D9ProjectedMeshState(
                Matrix4x4.Identity,
                new Direct3D9SurfaceRect(0, 0, 4, 4),
                new MilRectF(0, 0, 4, 4),
                IsVisible: true);
            return 0;
        },
        (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
        {
            calls.Add("Derive");
            shader = new Direct3D9DerivedMeshShader(
                () =>
                {
                    calls.Add("Begin");
                    return 0;
                },
                () =>
                {
                    calls.Add("CanShader");
                    return true;
                },
                () =>
                {
                    calls.Add("Shader");
                    return shaderResult;
                },
                () =>
                {
                    calls.Add("Fixed");
                    return fixedFunctionResult;
                },
                () =>
                {
                    calls.Add("Finish");
                    return 0;
                },
                () => calls.Add("ReleaseShader"));
            return 0;
        },
        _ => 0);
}