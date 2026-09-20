using System.Numerics;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9FixedFunctionMeshRendererTests
{
    [TestMethod]
    public void WhenRenderingMultiplePassesThenEachPipelineExecutesAndReleasesBeforeNextPass()
    {
        List<string> calls = [];

        int result = Direct3D9FixedFunctionMeshRenderer.Render(
            2,
            () =>
            {
                calls.Add("Lighting");
                return 0;
            },
            (uint passIndex, out Direct3D9Pipeline? pipeline, out Func<int>? renderGeometry) =>
            {
                calls.Add($"Setup:{passIndex}");
                pipeline = CreatePipeline(calls, passIndex);
                renderGeometry = () =>
                {
                    calls.Add($"Render:{passIndex}");
                    return 0;
                };
                return 0;
            });

        Assert.AreEqual(
            (0, "Lighting|Setup:0|Begin:0|Geometry:0|Flush:0|ReleaseBuilder:0|Render:0|ReleaseSources:0|Setup:1|Begin:1|Geometry:1|Flush:1|ReleaseBuilder:1|Render:1|ReleaseSources:1"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPrecomputeLightingFailsThenPassSetupIsSkipped()
    {
        int setupCalls = 0;

        int result = Direct3D9FixedFunctionMeshRenderer.Render(
            1,
            () => Direct3D9Factory.GenericFailureHResult,
            (uint _, out Direct3D9Pipeline? pipeline, out Func<int>? renderGeometry) =>
            {
                setupCalls++;
                pipeline = null;
                renderGeometry = null;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (result, setupCalls));
    }

    [TestMethod]
    public void WhenPassSetupFailsAfterReturningPipelineThenResourcesAreReleasedAndLaterPassesAreSkipped()
    {
        List<string> calls = [];
        int setupCalls = 0;

        int result = Direct3D9FixedFunctionMeshRenderer.Render(
            2,
            () => 0,
            (uint passIndex, out Direct3D9Pipeline? pipeline, out Func<int>? renderGeometry) =>
            {
                setupCalls++;
                pipeline = CreatePipeline(calls, passIndex);
                renderGeometry = null;
                return Direct3D9Factory.GenericFailureHResult;
            });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, "ReleaseSources:0|ReleaseBuilder:0"),
            (result, setupCalls, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPassSetupSucceedsWithoutCompletePassThenInternalErrorIsReturnedAndPipelineIsReleased()
    {
        List<string> calls = [];

        int result = Direct3D9FixedFunctionMeshRenderer.Render(
            1,
            () => 0,
            (uint passIndex, out Direct3D9Pipeline? pipeline, out Func<int>? renderGeometry) =>
            {
                pipeline = CreatePipeline(calls, passIndex);
                renderGeometry = null;
                return 0;
            });

        Assert.AreEqual(
            (Direct3D9Factory.InternalErrorHResult, "ReleaseSources:0|ReleaseBuilder:0"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPipelineExecutionFailsThenGeometryIsSkippedAndResourcesAreReleased()
    {
        List<string> calls = [];
        int renderCalls = 0;

        int result = Direct3D9FixedFunctionMeshRenderer.Render(
            1,
            () => 0,
            (uint passIndex, out Direct3D9Pipeline? pipeline, out Func<int>? renderGeometry) =>
            {
                pipeline = CreatePipeline(calls, passIndex, Direct3D9Factory.GenericFailureHResult);
                renderGeometry = () =>
                {
                    renderCalls++;
                    return 0;
                };
                return 0;
            });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, "Begin:0|ReleaseSources:0|ReleaseBuilder:0"),
            (result, renderCalls, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDiffuseLightingIsRequiredThenDiffuseColorsAreBorrowedForEveryPass()
    {
        uint[] diffuseColors = [0x00112233, 0x00445566, 0x00778899];
        List<string> calls = [];

        int result = Direct3D9FixedFunctionMeshRenderer.Render(
            2,
            Direct3D9FixedFunctionLightingValues.Diffuse,
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [new(0, 0), new(0, 0), new(0, 0)],
            [],
            diffuseColors,
            [1u, 2u, 3u],
            default,
            false,
            (_, _) => 0,
            () => 0,
            (uint passIndex, MilCompositingMode compositingMode, ref Direct3D9GeometryRenderer<uint> renderer, out Direct3D9Pipeline? pipeline) =>
            {
                calls.Add($"Setup:{passIndex}:{renderer.DiffuseColorsOrNormals[1]:X8}:{renderer.DefaultDiffuseOrNormal:X8}");
                pipeline = CreatePipeline([], passIndex);
                return 0;
            },
            (ref Direct3D9GeometryRenderer<uint> renderer) =>
            {
                calls.Add($"Render:{renderer.DiffuseColorsOrNormals[2]:X8}");
                return 0;
            });

        Assert.AreEqual(
            (0, "Setup:0:00445566:FFFFFFFF|Render:00778899|Setup:1:00445566:FFFFFFFF|Render:00778899"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSpecularLightingIsRequiredThenSpecularColorsAreBorrowed()
    {
        uint observedColor = 0;

        int result = Direct3D9FixedFunctionMeshRenderer.Render(
            1,
            Direct3D9FixedFunctionLightingValues.Specular,
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [new(0, 0), new(0, 0), new(0, 0)],
            [],
            [1u, 2u, 3u],
            [0x00112233u, 0x00445566u, 0x00778899u],
            default,
            false,
            (_, _) => 0,
            () => 0,
            (uint passIndex, MilCompositingMode compositingMode, ref Direct3D9GeometryRenderer<uint> renderer, out Direct3D9Pipeline? pipeline) =>
            {
                observedColor = renderer.DiffuseColorsOrNormals[1];
                pipeline = CreatePipeline([], passIndex);
                return 0;
            },
            (ref Direct3D9GeometryRenderer<uint> _) => 0);

        Assert.AreEqual((0, 0x00445566u), (result, observedColor));
    }

    [TestMethod]
    public void WhenZBufferIsEnabledThenDiffuseEnablesZWriteBeforeLightingAndUsesSourceOver()
    {
        List<string> calls = [];

        int result = RenderShaderPass(
            Direct3D9FixedFunctionLightingValues.Diffuse,
            true,
            (state, value) =>
            {
                calls.Add($"State:{state}:{value}");
                return 0;
            },
            () =>
            {
                calls.Add("Lighting");
                return 0;
            },
            compositingMode => calls.Add($"Setup:{compositingMode}"));

        Assert.AreEqual((0, "State:RSZwriteenable:1|Lighting|Setup:SourceOver"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow((int) Direct3D9FixedFunctionLightingValues.Specular)]
    [DataRow((int) Direct3D9FixedFunctionLightingValues.Emissive)]
    public void WhenZBufferIsEnabledThenAdditivePassDisablesZWrite(int lightingValues)
    {
        (Renderstatetype State, uint Value, MilCompositingMode Mode) observed = default;

        int result = RenderShaderPass(
            (Direct3D9FixedFunctionLightingValues) lightingValues,
            true,
            (state, value) =>
            {
                observed.State = state;
                observed.Value = value;
                return 0;
            },
            () => 0,
            compositingMode => observed.Mode = compositingMode);

        Assert.AreEqual(
            (0, (Renderstatetype.Zwriteenable, 0u, MilCompositingMode.SourceAdd)),
            (result, observed));
    }

    [TestMethod]
    public void WhenZBufferIsDisabledThenZWriteStateIsNotSent()
    {
        int stateCalls = 0;

        int result = RenderShaderPass(
            Direct3D9FixedFunctionLightingValues.Diffuse,
            false,
            (_, _) =>
            {
                stateCalls++;
                return 0;
            },
            () => 0,
            _ => { });

        Assert.AreEqual((0, 0), (result, stateCalls));
    }

    [TestMethod]
    public void WhenZWriteStateFailsThenLightingAndPassSetupAreSkipped()
    {
        int lightingCalls = 0;
        int setupCalls = 0;

        int result = RenderShaderPass(
            Direct3D9FixedFunctionLightingValues.Diffuse,
            true,
            (_, _) => Direct3D9Factory.GenericFailureHResult,
            () =>
            {
                lightingCalls++;
                return 0;
            },
            _ => setupCalls++);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0),
            (result, lightingCalls, setupCalls));
    }

    [TestMethod]
    public void WhenFixedFunctionPassSetupSucceedsWithoutPipelineThenInternalErrorIsReturned()
    {
        int renderCalls = 0;

        int result = Direct3D9FixedFunctionMeshRenderer.Render(
            1,
            Direct3D9FixedFunctionLightingValues.Diffuse,
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [default, default, default],
            [],
            [1u, 2u, 3u],
            [4u, 5u, 6u],
            default,
            false,
            (_, _) => 0,
            () => 0,
            (uint _, MilCompositingMode _, ref Direct3D9GeometryRenderer<uint> _, out Direct3D9Pipeline? pipeline) =>
            {
                pipeline = null;
                return 0;
            },
            (ref Direct3D9GeometryRenderer<uint> _) =>
            {
                renderCalls++;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.InternalErrorHResult, 0), (result, renderCalls));
    }

    [TestMethod]
    public void WhenEmissiveLightingIsRequiredThenMaterialColorIsUsedAsZeroAlphaDefault()
    {
        (int Count, uint DefaultColor) observed = default;

        int result = Direct3D9FixedFunctionMeshRenderer.Render(
            1,
            Direct3D9FixedFunctionLightingValues.Emissive,
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [new(0, 0), new(0, 0), new(0, 0)],
            [],
            [1u, 2u, 3u],
            [4u, 5u, 6u],
            new MilColorF(0.75f, 0.1f, 0.5f, 1.0f),
            false,
            (_, _) => 0,
            () => 0,
            (uint passIndex, MilCompositingMode compositingMode, ref Direct3D9GeometryRenderer<uint> renderer, out Direct3D9Pipeline? pipeline) =>
            {
                observed = (renderer.DiffuseColorsOrNormals.Length, renderer.DefaultDiffuseOrNormal);
                pipeline = CreatePipeline([], passIndex);
                return 0;
            },
            (ref Direct3D9GeometryRenderer<uint> _) => 0);

        Assert.AreEqual((0, (0, 0x001A80FFu)), (result, observed));
    }

    [TestMethod]
    public void WhenShaderPathSucceedsThenFixedFunctionIsSkippedAndFinishRuns()
    {
        List<string> calls = [];

        int result = Direct3D9MeshShaderRenderer.Render(
            () => AddCall(calls, "Begin", 0),
            () => true,
            () => AddCall(calls, "Shader", 0),
            () => AddCall(calls, "FixedFunction", 0),
            () => AddCall(calls, "Finish", 0));

        Assert.AreEqual((0, "Begin|Shader|Finish"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderPathFailsThenFixedFunctionFallbackRunsBeforeFinish()
    {
        List<string> calls = [];

        int result = Direct3D9MeshShaderRenderer.Render(
            () => AddCall(calls, "Begin", 0),
            () => true,
            () => AddCall(calls, "Shader", Direct3D9Factory.GenericFailureHResult),
            () => AddCall(calls, "FixedFunction", 0),
            () => AddCall(calls, "Finish", 0));

        Assert.AreEqual((0, "Begin|Shader|FixedFunction|Finish"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderMeshGeometryFailsThenPipelineIsReleasedBeforeFixedFunctionFallback()
    {
        List<string> calls = [];

        int result = Direct3D9MeshShaderRenderer.Render(
            () => AddCall(calls, "Begin", 0),
            () => true,
            () => Direct3D9ShaderMeshRenderer.Render(
                1,
                [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
                [new(0, 0, 1), new(0, 1, 0), new(1, 0, 0)],
                [default, default, default],
                [],
                (uint passIndex, ref Direct3D9GeometryRenderer<Vector3> _, out Direct3D9Pipeline? pipeline) =>
                {
                    calls.Add("ShaderSetup");
                    pipeline = CreatePipeline(calls, passIndex);
                    return 0;
                },
                (ref Direct3D9GeometryRenderer<Vector3> _) =>
                    AddCall(calls, "ShaderGeometry", Direct3D9Factory.GenericFailureHResult)),
            () => AddCall(calls, "FixedFunction", 0),
            () => AddCall(calls, "Finish", 0));

        Assert.AreEqual(
            (0, "Begin|ShaderSetup|Begin:0|Geometry:0|Flush:0|ReleaseBuilder:0|ShaderGeometry|ReleaseSources:0|FixedFunction|Finish"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderAndFixedFunctionMeshGeometryFailThenFallbackFailureIsPreservedAfterCleanupAndFinish()
    {
        const int fallbackFailure = unchecked((int) 0x80070057);
        List<string> calls = [];

        int result = Direct3D9MeshShaderRenderer.Render(
            () => AddCall(calls, "Begin", 0),
            () => true,
            () => Direct3D9ShaderMeshRenderer.Render(
                1,
                [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
                [new(0, 0, 1), new(0, 1, 0), new(1, 0, 0)],
                [default, default, default],
                [],
                (uint passIndex, ref Direct3D9GeometryRenderer<Vector3> _, out Direct3D9Pipeline? pipeline) =>
                {
                    calls.Add("ShaderSetup");
                    pipeline = CreatePipeline(calls, passIndex);
                    return 0;
                },
                (ref Direct3D9GeometryRenderer<Vector3> _) =>
                    AddCall(calls, "ShaderGeometry", Direct3D9Factory.GenericFailureHResult)),
            () => Direct3D9FixedFunctionMeshRenderer.Render(
                1,
                Direct3D9FixedFunctionLightingValues.Diffuse,
                [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
                [default, default, default],
                [],
                [uint.MaxValue, uint.MaxValue, uint.MaxValue],
                [],
                default,
                false,
                static (_, _) => 0,
                () => AddCall(calls, "FixedLighting", 0),
                (uint passIndex, MilCompositingMode _, ref Direct3D9GeometryRenderer<uint> _, out Direct3D9Pipeline? pipeline) =>
                {
                    calls.Add("FixedSetup");
                    pipeline = CreatePipeline(calls, passIndex);
                    return 0;
                },
                (ref Direct3D9GeometryRenderer<uint> _) => AddCall(calls, "FixedGeometry", fallbackFailure)),
            () => AddCall(calls, "Finish", Direct3D9Factory.GenericFailureHResult));

        Assert.AreEqual(
            (fallbackFailure, "Begin|ShaderSetup|Begin:0|Geometry:0|Flush:0|ReleaseBuilder:0|ShaderGeometry|ReleaseSources:0|FixedLighting|FixedSetup|Begin:0|Geometry:0|Flush:0|ReleaseBuilder:0|FixedGeometry|ReleaseSources:0|Finish"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderMeshFailsAndFixedFunctionMeshSucceedsThenFinishFailureIsReturnedAfterBothPipelinesRelease()
    {
        List<string> calls = [];

        int result = Direct3D9MeshShaderRenderer.Render(
            () => AddCall(calls, "Begin", 0),
            () => true,
            () => Direct3D9ShaderMeshRenderer.Render(
                1,
                [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
                [new(0, 0, 1), new(0, 1, 0), new(1, 0, 0)],
                [default, default, default],
                [],
                (uint passIndex, ref Direct3D9GeometryRenderer<Vector3> _, out Direct3D9Pipeline? pipeline) =>
                {
                    calls.Add("ShaderSetup");
                    pipeline = CreatePipeline(calls, passIndex);
                    return 0;
                },
                (ref Direct3D9GeometryRenderer<Vector3> _) =>
                    AddCall(calls, "ShaderGeometry", Direct3D9Factory.GenericFailureHResult)),
            () => Direct3D9FixedFunctionMeshRenderer.Render(
                1,
                Direct3D9FixedFunctionLightingValues.Diffuse,
                [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
                [default, default, default],
                [],
                [uint.MaxValue, uint.MaxValue, uint.MaxValue],
                [],
                default,
                false,
                static (_, _) => 0,
                () => AddCall(calls, "FixedLighting", 0),
                (uint passIndex, MilCompositingMode _, ref Direct3D9GeometryRenderer<uint> _, out Direct3D9Pipeline? pipeline) =>
                {
                    calls.Add("FixedSetup");
                    pipeline = CreatePipeline(calls, passIndex);
                    return 0;
                },
                (ref Direct3D9GeometryRenderer<uint> _) => AddCall(calls, "FixedGeometry", 0)),
            () => AddCall(calls, "Finish", Direct3D9Factory.GenericFailureHResult));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Begin|ShaderSetup|Begin:0|Geometry:0|Flush:0|ReleaseBuilder:0|ShaderGeometry|ReleaseSources:0|FixedLighting|FixedSetup|Begin:0|Geometry:0|Flush:0|ReleaseBuilder:0|FixedGeometry|ReleaseSources:0|Finish"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderPathIsUnavailableThenOnlyFixedFunctionRuns()
    {
        List<string> calls = [];

        int result = Direct3D9MeshShaderRenderer.Render(
            () => AddCall(calls, "Begin", 0),
            () => false,
            () => AddCall(calls, "Shader", 0),
            () => AddCall(calls, "FixedFunction", 0),
            () => AddCall(calls, "Finish", 0));

        Assert.AreEqual((0, "Begin|FixedFunction|Finish"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenBeginFailsThenNoDrawOrFinishRuns()
    {
        List<string> calls = [];

        int result = Direct3D9MeshShaderRenderer.Render(
            () => AddCall(calls, "Begin", Direct3D9Factory.GenericFailureHResult),
            () => true,
            () => AddCall(calls, "Shader", 0),
            () => AddCall(calls, "FixedFunction", 0),
            () => AddCall(calls, "Finish", 0));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Begin"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenFinishFailsAfterSuccessfulDrawThenFinishFailureIsReturned()
    {
        int result = Direct3D9MeshShaderRenderer.Render(
            () => 0,
            () => false,
            () => 0,
            () => 0,
            () => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenDrawAndFinishFailThenDrawFailureIsReturned()
    {
        const int drawFailure = unchecked((int) 0x80070057);

        int result = Direct3D9MeshShaderRenderer.Render(
            () => 0,
            () => false,
            () => 0,
            () => drawFailure,
            () => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(drawFailure, result);
    }

    [TestMethod]
    public void WhenMeshPipelineIsInitializedThenPassInputsAndCompositionReachFixedFunctionInitializer()
    {
        List<string> calls = [];
        Direct3D9FixedFunctionPassInputs passInputs = new(
            builder =>
            {
                calls.Add("Primary");
                return builder.SetConstant(new Direct3D9ConstantColorSource(new MilColorF(1, 1, 1, 1)));
            });
        Direct3D9FixedFunctionMeshPipelineInitializer meshInitializer = new(
            (mode, items) =>
            {
                calls.Add($"Initializer:{mode}:{items.Count}");
                return CreatePipelineInitializer(items, calls);
            },
            _ => 0,
            () =>
            {
                calls.Add("Begin");
                return 0;
            },
            () => false,
            (out nint vertexBuffer) =>
            {
                calls.Add("Flush");
                vertexBuffer = 17;
                return 0;
            },
            () => calls.Add("ReleaseSources"));
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [uint.MaxValue, uint.MaxValue, uint.MaxValue],
            [default, default, default],
            [],
            uint.MaxValue);

        int result = meshInitializer.Initialize(
            passInputs,
            MilCompositingMode.SourceAdd,
            ref renderer,
            out Direct3D9Pipeline? pipeline);
        Assert.IsNotNull(pipeline);
        int executeResult = pipeline.Execute();
        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual(
            (0, 0, "Primary|Initializer:SourceAdd:1|SetupBuilder|FinalizeMappings:31|Begin|Flush|ReleaseBuilder:31|ReleaseSources"),
            (result, executeResult, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDrawDataUsesMeshPipelineInitializerThenInitializePipelineUsesTheRealBoundary()
    {
        MilCompositingMode observedMode = default;
        Direct3D9FixedFunctionMeshPipelineInitializer meshInitializer = new(
            (mode, items) =>
            {
                observedMode = mode;
                return CreatePipelineInitializer(items, []);
            },
            static () => { });
        Direct3D9FixedFunctionMeshDrawData drawData = new(
            1,
            new Vector3[] { new(1, 2, 3), new(4, 5, 6), new(7, 8, 9) },
            new Vector2[] { default, default, default },
            ReadOnlyMemory<uint>.Empty,
            new uint[] { uint.MaxValue, uint.MaxValue, uint.MaxValue },
            ReadOnlyMemory<uint>.Empty,
            default,
            static () => 0,
            meshInitializer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            drawData.Positions.Span,
            drawData.DiffuseColors.Span,
            drawData.TextureCoordinates.Span,
            drawData.Indices.Span,
            uint.MaxValue);
        Direct3D9FixedFunctionPassInputs passInputs = new(
            static builder => builder.SetConstant(new Direct3D9ConstantColorSource(new MilColorF(1, 1, 1, 1))));

        int result = drawData.InitializePipeline(
            passInputs,
            MilCompositingMode.SourceOver,
            ref renderer,
            out Direct3D9Pipeline? pipeline);
        Assert.IsNotNull(pipeline);
        int executeResult = pipeline.Execute();
        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual((0, 0, MilCompositingMode.SourceOver), (result, executeResult, observedMode));
    }

    [DataTestMethod]
    [DataRow((int) Direct3D9FixedFunctionLightingValues.Diffuse, (int) MilCompositingMode.SourceOver)]
    [DataRow((int) Direct3D9FixedFunctionLightingValues.Specular, (int) MilCompositingMode.SourceAdd)]
    [DataRow((int) Direct3D9FixedFunctionLightingValues.Emissive, (int) MilCompositingMode.SourceAdd)]
    public void WhenShaderDrawDataInitializesPipelineThenShaderCompositingModeIsForwarded(
        int lightingValues,
        int expectedCompositingMode)
    {
        MilCompositingMode observedMode = default;
        Direct3D9ShaderMeshDrawData drawData = new(
            1,
            new Vector3[] { new(1, 2, 3), new(4, 5, 6), new(7, 8, 9) },
            new Vector3[] { new(0, 0, 1), new(0, 1, 0), new(1, 0, 0) },
            new Vector2[] { default, default, default },
            ReadOnlyMemory<uint>.Empty,
            (Direct3D9FixedFunctionPassInputs _, MilCompositingMode mode, ref Direct3D9GeometryRenderer<Vector3> _, out Direct3D9Pipeline? pipeline) =>
            {
                observedMode = mode;
                pipeline = null;
                return Direct3D9Factory.GenericFailureHResult;
            });
        Direct3D9GeometryRenderer<Vector3> renderer = new(
            drawData.Positions.Span,
            drawData.Normals.Span,
            drawData.TextureCoordinates.Span,
            drawData.Indices.Span,
            new Vector3(1, 0, 0));

        int result = drawData.InitializePipeline(
            new Direct3D9FixedFunctionPassInputs(static _ => 0),
            (Direct3D9FixedFunctionLightingValues) lightingValues == Direct3D9FixedFunctionLightingValues.Diffuse
                ? MilCompositingMode.SourceOver
                : MilCompositingMode.SourceAdd,
            ref renderer,
            out _);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, (MilCompositingMode) expectedCompositingMode),
            (result, observedMode));
    }

    [TestMethod]
    public void WhenShaderMeshRendersMultiplePassesThenEachPipelineExecutesRendersAndReleasesBeforeNextPass()
    {
        List<string> calls = [];

        int result = Direct3D9ShaderMeshRenderer.Render(
            2,
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [new(0, 0, 1), new(0, 1, 0), new(1, 0, 0)],
            [default, default, default],
            [],
            (uint passIndex, ref Direct3D9GeometryRenderer<Vector3> renderer, out Direct3D9Pipeline? pipeline) =>
            {
                calls.Add($"Setup:{passIndex}:{renderer.DiffuseColorsOrNormals[(int) passIndex]}");
                pipeline = CreatePipeline(calls, passIndex);
                return 0;
            },
            (ref Direct3D9GeometryRenderer<Vector3> renderer) =>
            {
                calls.Add($"Render:{renderer.DefaultDiffuseOrNormal}");
                return 0;
            });

        Assert.AreEqual(
            (0, "Setup:0:<0, 0, 1>|Begin:0|Geometry:0|Flush:0|ReleaseBuilder:0|Render:<1, 0, 0>|ReleaseSources:0|Setup:1:<0, 1, 0>|Begin:1|Geometry:1|Flush:1|ReleaseBuilder:1|Render:<1, 0, 0>|ReleaseSources:1"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderMeshPassSetupFailsThenReturnedPipelineIsReleasedAndLaterPassesAreSkipped()
    {
        List<string> calls = [];
        int setupCalls = 0;

        int result = Direct3D9ShaderMeshRenderer.Render(
            2,
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [new(0, 0, 1), new(0, 1, 0), new(1, 0, 0)],
            [default, default, default],
            [],
            (uint passIndex, ref Direct3D9GeometryRenderer<Vector3> _, out Direct3D9Pipeline? pipeline) =>
            {
                setupCalls++;
                pipeline = CreatePipeline(calls, passIndex);
                return Direct3D9Factory.GenericFailureHResult;
            },
            (ref Direct3D9GeometryRenderer<Vector3> _) => 0);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, "ReleaseSources:0|ReleaseBuilder:0"),
            (result, setupCalls, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderMeshPassSetupSucceedsWithoutPipelineThenInternalErrorIsReturned()
    {
        int renderCalls = 0;

        int result = Direct3D9ShaderMeshRenderer.Render(
            1,
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [new(0, 0, 1), new(0, 1, 0), new(1, 0, 0)],
            [default, default, default],
            [],
            (uint _, ref Direct3D9GeometryRenderer<Vector3> _, out Direct3D9Pipeline? pipeline) =>
            {
                pipeline = null;
                return 0;
            },
            (ref Direct3D9GeometryRenderer<Vector3> _) =>
            {
                renderCalls++;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.InternalErrorHResult, 0), (result, renderCalls));
    }

    [TestMethod]
    public void WhenShaderMeshPipelineExecutionFailsThenCurrentPipelineIsReleasedAndLaterPassesAreSkipped()
    {
        List<string> calls = [];
        int setupCalls = 0;
        int renderCalls = 0;

        int result = Direct3D9ShaderMeshRenderer.Render(
            2,
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [new(0, 0, 1), new(0, 1, 0), new(1, 0, 0)],
            [default, default, default],
            [],
            (uint passIndex, ref Direct3D9GeometryRenderer<Vector3> _, out Direct3D9Pipeline? pipeline) =>
            {
                setupCalls++;
                pipeline = CreatePipeline(calls, passIndex, Direct3D9Factory.GenericFailureHResult);
                return 0;
            },
            (ref Direct3D9GeometryRenderer<Vector3> _) =>
            {
                renderCalls++;
                return 0;
            });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, 0, "Begin:0|ReleaseSources:0|ReleaseBuilder:0"),
            (result, setupCalls, renderCalls, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderMeshGeometryFailsThenCurrentPipelineIsReleasedAndLaterPassesAreSkipped()
    {
        List<string> calls = [];
        int setupCalls = 0;

        int result = Direct3D9ShaderMeshRenderer.Render(
            2,
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [new(0, 0, 1), new(0, 1, 0), new(1, 0, 0)],
            [default, default, default],
            [],
            (uint passIndex, ref Direct3D9GeometryRenderer<Vector3> _, out Direct3D9Pipeline? pipeline) =>
            {
                setupCalls++;
                pipeline = CreatePipeline(calls, passIndex);
                return 0;
            },
            (ref Direct3D9GeometryRenderer<Vector3> _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, "Begin:0|Geometry:0|Flush:0|ReleaseBuilder:0|ReleaseSources:0"),
            (result, setupCalls, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderMeshRendersThenRequiredLightingPassIsSetBeforePassSetup()
    {
        List<string> calls = [];

        int result = Direct3D9ShaderMeshRenderer.Render(
            1,
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [new(0, 0, 1), new(0, 1, 0), new(1, 0, 0)],
            [default, default, default],
            [],
            (uint passIndex, ref Direct3D9GeometryRenderer<Vector3> _, out Direct3D9Pipeline? pipeline) =>
            {
                calls.Add($"Setup:{passIndex}");
                pipeline = CreatePipeline(calls, passIndex);
                return 0;
            },
            (ref Direct3D9GeometryRenderer<Vector3> _) =>
            {
                calls.Add("Render");
                return 0;
            },
            requiredLightingValues: Direct3D9FixedFunctionLightingValues.Specular,
            setLightingPass: lightingValues => calls.Add($"Lighting:{lightingValues}"));

        Assert.AreEqual(
            (0, "Lighting:Specular|Setup:0|Begin:0|Geometry:0|Flush:0|ReleaseBuilder:0|Render|ReleaseSources:0"),
            (result, string.Join('|', calls)));
    }

    private static Direct3D9FixedFunctionPipelineInitializer CreatePipelineInitializer(
        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items,
        List<string> calls)
    {
        return new Direct3D9FixedFunctionPipelineInitializer(
            items,
            (out nint vertexBuilder) =>
            {
                calls.Add("SetupBuilder");
                vertexBuilder = 31;
                return 0;
            },
            vertexBuilder => AddCall(calls, $"FinalizeMappings:{vertexBuilder}", 0),
            vertexBuilder => calls.Add($"ReleaseBuilder:{vertexBuilder}"),
            static (_, _, _) => { },
            static (_, _) => 0,
            static _ => 0,
            static _ => 0,
            static () => 0,
            static () => 0,
            static () => 0,
            setConstantMapping: static (_, _, _) => 0);
    }

    private static int AddCall(List<string> calls, string call, int result)
    {
        calls.Add(call);
        return result;
    }

    private static int RenderShaderPass(
        Direct3D9FixedFunctionLightingValues lightingValues,
        bool zBufferEnabled,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<int> precomputeLighting,
        Action<MilCompositingMode> observeCompositingMode)
    {
        return Direct3D9FixedFunctionMeshRenderer.Render(
            1,
            lightingValues,
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [new(0, 0), new(0, 0), new(0, 0)],
            [],
            [1u, 2u, 3u],
            [4u, 5u, 6u],
            default,
            zBufferEnabled,
            setRenderState,
            precomputeLighting,
            (uint passIndex, MilCompositingMode compositingMode, ref Direct3D9GeometryRenderer<uint> _, out Direct3D9Pipeline? pipeline) =>
            {
                observeCompositingMode(compositingMode);
                pipeline = CreatePipeline([], passIndex);
                return 0;
            },
            (ref Direct3D9GeometryRenderer<uint> _) => 0);
    }

    private static Direct3D9Pipeline CreatePipeline(
        List<string> calls,
        uint passIndex,
        int beginResult = Direct3D9Factory.SuccessHResult)
    {
        return new Direct3D9Pipeline(
            1,
            [],
            _ => 0,
            _ => 0,
            () =>
            {
                calls.Add($"Begin:{passIndex}");
                return beginResult;
            },
            () =>
            {
                calls.Add($"Geometry:{passIndex}");
                return 0;
            },
            () => false,
            (out nint vertexBuffer) =>
            {
                calls.Add($"Flush:{passIndex}");
                vertexBuffer = 17;
                return 0;
            },
            () => calls.Add($"ReleaseSources:{passIndex}"),
            () => calls.Add($"ReleaseBuilder:{passIndex}"));
    }
}
