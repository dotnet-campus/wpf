using System.Numerics;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public class Direct3D9PipelineTests
{
    [TestMethod]
    public void WhenFixedFunctionSourceOverSelectsOpaqueSourceThenCompositionIsPromotedToSourceCopy()
    {
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            IsOpaque: true);
        Direct3D9FinalizedFixedFunctionPipeline pipeline =
            Direct3D9FixedFunctionPipelineFinalizer.FinalizeBlendOperations(
            [
                new(
                    Direct3D9VertexFormatAttribute.Uv1,
                    colorSource,
                    blendOperation: Direct3D9FixedFunctionBlendOperation.SelectSource,
                    source1: Direct3D9FixedFunctionBlendArgument.Texture),
            ]);

        MilCompositingMode result = Direct3D9PipelineCompositionFinalizer.FinalizeFixedFunction(
            MilCompositingMode.SourceOver,
            antiAliasUsed: false,
            pipeline);

        Assert.AreEqual(MilCompositingMode.SourceCopy, result);
    }

    [TestMethod]
    public void WhenFixedFunctionSourceOverUsesAntiAliasingThenOpaqueSourceRemainsSourceOver()
    {
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            IsOpaque: true);
        Direct3D9FinalizedFixedFunctionPipeline pipeline =
            Direct3D9FixedFunctionPipelineFinalizer.FinalizeBlendOperations(
            [
                new(
                    Direct3D9VertexFormatAttribute.Uv1,
                    colorSource,
                    blendOperation: Direct3D9FixedFunctionBlendOperation.SelectSource,
                    source1: Direct3D9FixedFunctionBlendArgument.Texture),
            ]);

        MilCompositingMode result = Direct3D9PipelineCompositionFinalizer.FinalizeFixedFunction(
            MilCompositingMode.SourceOver,
            antiAliasUsed: true,
            pipeline);

        Assert.AreEqual(MilCompositingMode.SourceOver, result);
    }

    [TestMethod]
    public void WhenShaderSourceOverBlendsOpaqueSourceThenCompositionIsPromotedToSourceCopy()
    {
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            IsOpaque: true);
        Direct3D9ShaderPipelineItem[] items =
        [
            new(0, colorSource, transparencyEffect: Direct3D9ShaderTransparencyEffect.BlendsColorSource),
        ];

        MilCompositingMode result = Direct3D9PipelineCompositionFinalizer.FinalizeShader(
            MilCompositingMode.SourceOver,
            antiAliasUsed: false,
            items);

        Assert.AreEqual(MilCompositingMode.SourceCopy, result);
    }

    [TestMethod]
    public void WhenShaderSourceOverBlendsTransparentSourceThenCompositionRemainsSourceOver()
    {
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0);
        Direct3D9ShaderPipelineItem[] items =
        [
            new(0, colorSource, transparencyEffect: Direct3D9ShaderTransparencyEffect.BlendsColorSource),
        ];

        MilCompositingMode result = Direct3D9PipelineCompositionFinalizer.FinalizeShader(
            MilCompositingMode.SourceOver,
            antiAliasUsed: false,
            items);

        Assert.AreEqual(MilCompositingMode.SourceOver, result);
    }

    [TestMethod]
    public void WhenShaderOperationAddsTransparencyThenOpaqueSourceRemainsSourceOver()
    {
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            IsOpaque: true);
        Direct3D9ShaderPipelineItem[] items =
        [
            new(0, colorSource, transparencyEffect: Direct3D9ShaderTransparencyEffect.HasTransparency),
        ];

        MilCompositingMode result = Direct3D9PipelineCompositionFinalizer.FinalizeShader(
            MilCompositingMode.SourceOver,
            antiAliasUsed: false,
            items);

        Assert.AreEqual(MilCompositingMode.SourceOver, result);
    }

    [TestMethod]
    public void WhenShaderSourceOverUsesAntiAliasingThenOpaquePipelineRemainsSourceOver()
    {
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            IsOpaque: true);
        Direct3D9ShaderPipelineItem[] items =
        [
            new(0, colorSource, transparencyEffect: Direct3D9ShaderTransparencyEffect.BlendsColorSource),
        ];

        MilCompositingMode result = Direct3D9PipelineCompositionFinalizer.FinalizeShader(
            MilCompositingMode.SourceOver,
            antiAliasUsed: true,
            items);

        Assert.AreEqual(MilCompositingMode.SourceOver, result);
    }

    [TestMethod]
    public void WhenShaderPipelineItemAddsColorSourceThenPipelineReuseIsReset()
    {
        int resetCount = 0;
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            ResetForPipelineReuse: () => resetCount++);

        _ = new Direct3D9ShaderPipelineItem(2, colorSource);

        Assert.AreEqual(1, resetCount);
    }

    [TestMethod]
    public void WhenFixedFunctionPipelineAddsColorSourcesThenEachNonNullSourceIsReset()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource first = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            ResetForPipelineReuse: () => calls.Add("First"));
        Direct3D9PipelineColorSource second = new(
            Direct3D9ColorSourceType.Constant,
            () => 0,
            ResetForPipelineReuse: () => calls.Add("Second"));

        _ = CreatePipeline(calls, [first, null, second]);

        Assert.AreEqual("First|Second", string.Join('|', calls));
    }

    [TestMethod]
    public void WhenFixedFunctionPipelineItemAddsColorSourceThenPipelineReuseIsReset()
    {
        int resetCount = 0;
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            ResetForPipelineReuse: () => resetCount++);

        _ = new Direct3D9FixedFunctionPipelineItem(Direct3D9VertexFormatAttribute.Uv2, colorSource);

        Assert.AreEqual(1, resetCount);
    }

    [TestMethod]
    public void WhenFixedFunctionBlendOperationsAreFinalizedThenFirstDiffuseStageIsCollapsedIntoPrimaryTextureStage()
    {
        Direct3D9FixedFunctionPipelineItem[] items =
        [
            new(
                Direct3D9VertexFormatAttribute.Uv1,
                (Direct3D9PipelineColorSource?) null,
                stage: 0,
                sampler: 0,
                Direct3D9FixedFunctionBlendOperation.SelectSource,
                Direct3D9FixedFunctionBlendArgument.Texture),
            new(
                Direct3D9VertexFormatAttribute.None,
                (Direct3D9PipelineColorSource?) null,
                stage: 1,
                blendOperation: Direct3D9FixedFunctionBlendOperation.Multiply,
                source1: Direct3D9FixedFunctionBlendArgument.Diffuse,
                source2: Direct3D9FixedFunctionBlendArgument.Current),
        ];

        Direct3D9FinalizedFixedFunctionPipeline pipeline =
            Direct3D9FixedFunctionPipelineFinalizer.FinalizeBlendOperations(items);

        Assert.AreEqual(
            (1u, Direct3D9FixedFunctionBlendOperation.Multiply, Direct3D9FixedFunctionBlendArgument.Diffuse, Direct3D9FixedFunctionBlendArgument.Texture, Direct3D9FixedFunctionBlendOperation.Nop, 0u),
            (pipeline.FirstUnusedStage, pipeline.Items[0].BlendOperation, pipeline.Items[0].Source1, pipeline.Items[0].Source2, pipeline.Items[1].BlendOperation, pipeline.Items[1].Stage));
    }

    [TestMethod]
    public void WhenOpaquePrimaryTextureIsCollapsedThenCombinedOperationIgnoresTextureAlpha()
    {
        Direct3D9FixedFunctionPipelineItem[] items =
        [
            new(
                Direct3D9VertexFormatAttribute.Uv1,
                (Direct3D9PipelineColorSource?) null,
                sampler: 0,
                blendOperation: Direct3D9FixedFunctionBlendOperation.SelectSourceColorIgnoreAlpha,
                source1: Direct3D9FixedFunctionBlendArgument.Texture),
            new(
                Direct3D9VertexFormatAttribute.None,
                (Direct3D9PipelineColorSource?) null,
                stage: 1,
                blendOperation: Direct3D9FixedFunctionBlendOperation.Multiply,
                source1: Direct3D9FixedFunctionBlendArgument.Diffuse,
                source2: Direct3D9FixedFunctionBlendArgument.Current),
        ];

        Direct3D9FinalizedFixedFunctionPipeline pipeline =
            Direct3D9FixedFunctionPipelineFinalizer.FinalizeBlendOperations(items);

        Assert.AreEqual(Direct3D9FixedFunctionBlendOperation.MultiplyColorIgnoreAlpha, pipeline.Items[0].BlendOperation);
    }

    [TestMethod]
    public void WhenFinalizedFixedFunctionPipelineSendsDeviceStatesThenCollapsedStageAndCleanupAreConsumed()
    {
        List<string> calls = [];
        Direct3D9FinalizedFixedFunctionPipeline pipeline = Direct3D9FixedFunctionPipelineFinalizer.FinalizeBlendOperations(
        [
            new(
                Direct3D9VertexFormatAttribute.Uv1,
                new Direct3D9PipelineColorSource(
                    Direct3D9ColorSourceType.Texture,
                    () => 0,
                    SendDeviceStates: (stage, sampler) =>
                    {
                        calls.Add($"Texture:{stage}:{sampler}");
                        return 0;
                    }),
                stage: 0,
                sampler: 0,
                Direct3D9FixedFunctionBlendOperation.SelectSource,
                Direct3D9FixedFunctionBlendArgument.Texture),
            new(
                Direct3D9VertexFormatAttribute.None,
                new Direct3D9PipelineColorSource(
                    Direct3D9ColorSourceType.Constant,
                    () => 0,
                    SendDeviceStates: (stage, sampler) =>
                    {
                        calls.Add($"Diffuse:{stage}:{sampler}");
                        return 0;
                    }),
                stage: 1,
                blendOperation: Direct3D9FixedFunctionBlendOperation.Multiply,
                source1: Direct3D9FixedFunctionBlendArgument.Diffuse,
                source2: Direct3D9FixedFunctionBlendArgument.Current),
        ]);
        Direct3D9FixedFunctionPipelineDeviceStateSender sender = CreateFixedFunctionDeviceStateSender(calls, pipeline);

        int result = sender.SendDeviceStates(29);

        Assert.AreEqual(
            (0, "Stage:0:4:2:0:4:2:1|Texture:0:0|Diffuse:0:4294967295|Disable:1|VertexFormat:29|AlphaBlend|PixelShader:null|VertexShader:null"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenFixedFunctionDeviceStatesAreSentThenNativeOrderAndFinalizedStageParametersArePreserved()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendDeviceStates: (stage, sampler) =>
            {
                calls.Add($"ColorState:{stage}:{sampler}");
                return 0;
            });
        Direct3D9FixedFunctionPipelineDeviceStateSender sender = CreateFixedFunctionDeviceStateSender(
            calls,
            [new Direct3D9FixedFunctionPipelineItem(
                Direct3D9VertexFormatAttribute.Uv2,
                colorSource,
                stage: 2,
                sampler: 1,
                Direct3D9FixedFunctionBlendOperation.Multiply,
                Direct3D9FixedFunctionBlendArgument.Texture,
                Direct3D9FixedFunctionBlendArgument.Diffuse)],
            firstUnusedStage: 3);

        int result = sender.SendDeviceStates(29);

        Assert.AreEqual(
            (0, "Stage:2:4:2:0:4:2:1|ColorState:2:1|Disable:3|VertexFormat:29|AlphaBlend|PixelShader:null|VertexShader:null"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenFixedFunctionBlendCombinationIsUnsupportedThenNotImplementedIsReturnedBeforeColorState()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendDeviceStates: (_, _) =>
            {
                calls.Add("ColorState");
                return 0;
            });
        Direct3D9FixedFunctionPipelineDeviceStateSender sender = CreateFixedFunctionDeviceStateSender(
            calls,
            [new Direct3D9FixedFunctionPipelineItem(
                Direct3D9VertexFormatAttribute.Uv1,
                colorSource,
                blendOperation: Direct3D9FixedFunctionBlendOperation.SelectSource,
                source1: Direct3D9FixedFunctionBlendArgument.Current)]);

        int result = sender.SendDeviceStates(29);

        Assert.AreEqual(
            (Direct3D9Factory.NotImplementedHResult, string.Empty),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenFixedFunctionColorStateFailsThenFirstFailureStopsStageCleanupAndRenderStates()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendDeviceStates: (stage, sampler) =>
            {
                calls.Add($"ColorState:{stage}:{sampler}");
                return Direct3D9Factory.GenericFailureHResult;
            });
        Direct3D9FixedFunctionPipelineDeviceStateSender sender = CreateFixedFunctionDeviceStateSender(
            calls,
            [new Direct3D9FixedFunctionPipelineItem(
                Direct3D9VertexFormatAttribute.Uv1,
                colorSource,
                sampler: 0,
                blendOperation: Direct3D9FixedFunctionBlendOperation.SelectSource,
                source1: Direct3D9FixedFunctionBlendArgument.Texture)]);

        int result = sender.SendDeviceStates(29);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Stage:0:2:2:1:2:2:1|ColorState:0:0"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenFixedFunctionSenderIsUsedByPipelineThenStatesPrecedeEachCachedDraw()
    {
        List<string> calls = [];
        Direct3D9FixedFunctionPipelineDeviceStateSender sender = CreateFixedFunctionDeviceStateSender(
            calls,
            [new Direct3D9FixedFunctionPipelineItem(
                Direct3D9VertexFormatAttribute.Uv1,
                (Direct3D9PipelineColorSource?) null,
                blendOperation: Direct3D9FixedFunctionBlendOperation.SelectSource,
                source1: Direct3D9FixedFunctionBlendArgument.Diffuse)]);
        Direct3D9Pipeline pipeline = new(
            3,
            [null],
            sender.SendDeviceStates,
            vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return 0;
            },
            () => 0,
            () => 0,
            () => false,
            (out nint vertexBuffer) =>
            {
                vertexBuffer = 23;
                return 0;
            },
            () => { },
            () => { },
            resetColorSources: false);

        Assert.AreEqual(0, pipeline.Execute());
        calls.Clear();
        Assert.AreEqual(0, pipeline.Execute());

        Assert.AreEqual(
            "Stage:0:2:0:1:2:0:1|Disable:1|VertexFormat:23|AlphaBlend|PixelShader:null|VertexShader:null|Draw:23",
            string.Join('|', calls));
    }

    [TestMethod]
    [DataRow(3, Direct3D9Factory.GenericFailureHResult, "Stage:0|FirstColor|Stage:1")]
    [DataRow(4, Direct3D9Factory.InvalidCallHResult, "Stage:0|FirstColor|Stage:1|SecondColor")]
    [DataRow(5, Direct3D9Factory.OutOfVideoMemoryHResult, "Stage:0|FirstColor|Stage:1|SecondColor|Disable:2")]
    [DataRow(6, Direct3D9Factory.GenericFailureHResult, "Stage:0|FirstColor|Stage:1|SecondColor|Disable:2|VertexFormat:17")]
    [DataRow(7, Direct3D9Factory.InvalidCallHResult, "Stage:0|FirstColor|Stage:1|SecondColor|Disable:2|VertexFormat:17|AlphaBlend")]
    [DataRow(8, Direct3D9Factory.OutOfVideoMemoryHResult, "Stage:0|FirstColor|Stage:1|SecondColor|Disable:2|VertexFormat:17|AlphaBlend|PixelShader:null")]
    [DataRow(9, Direct3D9Factory.GenericFailureHResult, "Stage:0|FirstColor|Stage:1|SecondColor|Disable:2|VertexFormat:17|AlphaBlend|PixelShader:null|VertexShader:null")]
    [DataRow(10, Direct3D9Factory.OutOfVideoMemoryHResult, "Stage:0|FirstColor|Stage:1|SecondColor|Disable:2|VertexFormat:17|AlphaBlend|PixelShader:null|VertexShader:null|Draw:17")]
    public void WhenFixedFunctionCachedBufferStateOrDrawFailsThenRetryRestartsAndCleanupPreservesOwnership(
        int failureOperation,
        int failureResult,
        string failureCalls)
    {
        List<string> calls = [];
        int operation = 0;
        int Record(string call)
        {
            calls.Add(call);
            operation++;
            return operation == failureOperation ? failureResult : Direct3D9Factory.SuccessHResult;
        }

        Direct3D9PipelineColorSource firstColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendDeviceStates: (_, _) => Record("FirstColor"));
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendDeviceStates: (_, _) => Record("SecondColor"));
        Direct3D9FixedFunctionPipelineDeviceStateSender sender = new(
            [
                new Direct3D9FixedFunctionPipelineItem(
                    Direct3D9VertexFormatAttribute.Uv1,
                    firstColorSource,
                    stage: 0,
                    sampler: 0,
                    Direct3D9FixedFunctionBlendOperation.SelectSource,
                    Direct3D9FixedFunctionBlendArgument.Texture),
                new Direct3D9FixedFunctionPipelineItem(
                    Direct3D9VertexFormatAttribute.Uv2,
                    secondColorSource,
                    stage: 1,
                    sampler: 1,
                    Direct3D9FixedFunctionBlendOperation.Multiply,
                    Direct3D9FixedFunctionBlendArgument.Texture,
                    Direct3D9FixedFunctionBlendArgument.Current),
            ],
            firstUnusedStage: 2,
            (stage, _) => Record($"Stage:{stage}"),
            stage => Record($"Disable:{stage}"),
            vertexBuffer => Record($"VertexFormat:{vertexBuffer}"),
            () => Record("AlphaBlend"),
            () => Record("PixelShader:null"),
            () => Record("VertexShader:null"));
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendDeviceStates: sender.SendDeviceStates,
            drawPrimitive: vertexBuffer => Record($"Draw:{vertexBuffer}"));
        Assert.AreEqual(0, pipeline.Execute());
        calls.Clear();

        int failedResult = pipeline.Execute();
        string failedCalls = string.Join('|', calls);
        calls.Clear();
        int retryResult = pipeline.Execute();
        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual(
            (failureResult, failureCalls, 0, "Stage:0|FirstColor|Stage:1|SecondColor|Disable:2|VertexFormat:17|AlphaBlend|PixelShader:null|VertexShader:null|Draw:17|ReleaseColors"),
            (failedResult, failedCalls, retryResult, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenFixedFunctionPipelineIsInitializedThenMappingsBoundsBuildAndDrawUseOneBuilderLifecycle()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendDeviceStates: (stage, sampler) =>
            {
                calls.Add($"SourceState:{stage}:{sampler}");
                return 0;
            },
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"Mapping:{builder}:{location}");
                return 0;
            });
        Direct3D9FixedFunctionPipelineInitializer initializer = CreateFixedFunctionPipelineInitializer(
            calls,
            [new Direct3D9FixedFunctionPipelineItem(Direct3D9VertexFormatAttribute.Uv3, colorSource)]);

        int result = InitializeFixedFunctionPipeline(
            initializer,
            calls,
            new Direct3D9SurfaceRect(1, 2, 30, 40),
            false,
            out Direct3D9Pipeline? pipeline);
        Assert.AreEqual(0, result);
        Assert.AreEqual(0, pipeline!.Execute());
        Assert.AreEqual(0, pipeline.Execute());

        Assert.AreEqual(
            "SetupVertexBuilder|Mapping:17:Uv3|Finalize:17|Bounds:17:1,2,30,40:False|Begin|Geometry|Flush|ReleaseBuilder:17|SourceState:0:4294967295|Disable:1|VertexFormat:23|AlphaBlend|PixelShader:null|VertexShader:null|Draw:23",
            string.Join('|', calls));
    }

    [TestMethod]
    public void WhenFixedFunctionPipelineIsInitializedThenFinalizedBlendOperationsDriveCachedDrawState()
    {
        List<string> calls = [];
        Direct3D9FixedFunctionPipelineInitializer initializer = CreateFixedFunctionPipelineInitializer(
            calls,
            [
                new Direct3D9FixedFunctionPipelineItem(
                    Direct3D9VertexFormatAttribute.Uv1,
                    (Direct3D9PipelineColorSource?) null,
                    stage: 0,
                    sampler: 0,
                    Direct3D9FixedFunctionBlendOperation.SelectSource,
                    Direct3D9FixedFunctionBlendArgument.Texture),
                new Direct3D9FixedFunctionPipelineItem(
                    Direct3D9VertexFormatAttribute.None,
                    (Direct3D9PipelineColorSource?) null,
                    stage: 1,
                    blendOperation: Direct3D9FixedFunctionBlendOperation.Multiply,
                    source1: Direct3D9FixedFunctionBlendArgument.Diffuse,
                    source2: Direct3D9FixedFunctionBlendArgument.Current),
            ]);
        Assert.AreEqual(0, InitializeFixedFunctionPipeline(initializer, calls, null, true, out Direct3D9Pipeline? pipeline));
        Assert.AreEqual(0, pipeline!.Execute());
        calls.Clear();

        int result = pipeline.Execute();

        Assert.AreEqual(
            (0, "Stage:0:4:2:0:4:2:1|Disable:1|VertexFormat:23|AlphaBlend|PixelShader:null|VertexShader:null|Draw:23"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenRendererAwareFixedFunctionPipelineIsInitializedThenRendererFvfIdentifiesTheGeometryGeneratorAndGeometrySendIsEmpty()
    {
        List<string> calls = [];
        Direct3D9FixedFunctionPipelineInitializer initializer = CreateFixedFunctionPipelineInitializer(
            calls,
            [
                new Direct3D9FixedFunctionPipelineItem(
                    Direct3D9VertexFormatAttribute.None,
                    (Direct3D9PipelineColorSource?) null,
                    blendOperation: Direct3D9FixedFunctionBlendOperation.SelectSource,
                    source1: Direct3D9FixedFunctionBlendArgument.Diffuse)
            ]);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [new(0, 0), new(0, 0), new(0, 0)],
            [],
            uint.MaxValue);

        int result = Direct3D9FixedFunctionGeometryPipeline.Initialize(
            ref renderer,
            initializer,
            null,
            true,
            vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return 0;
            },
            () =>
            {
                calls.Add("Begin");
                return 0;
            },
            () => false,
            (out nint vertexBuffer) =>
            {
                calls.Add("Flush");
                vertexBuffer = 23;
                return 0;
            },
            () => calls.Add("ReleaseColors"),
            out Direct3D9Pipeline? pipeline);
        Assert.AreEqual(0, result);

        Assert.AreEqual(0, pipeline!.Execute());

        Assert.AreEqual((0, (nint) renderer.FlexibleVertexFormat), (result, pipeline.GeometryGenerator));
        Assert.AreEqual(
            "SetupVertexBuilder|Finalize:17|Begin|Flush|ReleaseBuilder:17",
            string.Join('|', calls));
    }

    [TestMethod]
    public void WhenPassInputsAreInitializedThenPrimaryEffectsModifiersLightingAndClipBuildTheGeometryPipelineInNativeOrder()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource texture = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendDeviceStates: (stage, sampler) =>
            {
                calls.Add($"State:{stage}:{sampler}");
                return 0;
            },
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"Mapping:{builder}:{location}");
                return 0;
            });
        Direct3D9FixedFunctionPassInputs passInputs = new(
            builder =>
            {
                calls.Add("Primary");
                return builder.SetTexture(texture);
            },
            (context, _) =>
            {
                calls.Add($"Effects:{context}");
                return 0;
            },
            effectContext: 11,
            sendGeometryModifiers: _ =>
            {
                calls.Add("Modifiers");
                return 0;
            },
            sendLighting: builder =>
            {
                calls.Add("Lighting");
                return builder.AddLighting(new Direct3D9LightingColorSource());
            },
            processClip: _ =>
            {
                calls.Add("Clip");
                return 0;
            });
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [new(0, 0), new(0, 0), new(0, 0)],
            [],
            uint.MaxValue);

        int result = Direct3D9FixedFunctionGeometryPipeline.Initialize(
            ref renderer,
            passInputs,
            items => CreateFixedFunctionPipelineInitializer(calls, items),
            null,
            true,
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
                vertexBuffer = 23;
                return 0;
            },
            () => calls.Add("ReleaseColors"),
            out Direct3D9Pipeline? pipeline);
        Assert.AreEqual(0, result);
        Assert.AreEqual(0, pipeline!.Execute());
        Assert.AreEqual(0, pipeline.Execute());

        Assert.AreEqual(
            "Primary|Effects:11|Modifiers|Lighting|Clip|SetupVertexBuilder|Mapping:17:Uv1|Finalize:17|Begin|Flush|ReleaseBuilder:17|Stage:0:4:2:0:4:2:1|State:0:0|Disable:1|VertexFormat:23|AlphaBlend|PixelShader:null|VertexShader:null", 
            string.Join('|', calls));
    }

    [TestMethod]
    public void WhenLightingUsesAvailableDiffuseThenBuilderGeneratesDiffuseAndOwnsTheSource()
    {
        List<string> calls = [];
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new(Direct3D9VertexFormatAttribute.Uv1);
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0)));
        Direct3D9LightingColorSource lighting = new(() => calls.Add("ReleaseLighting"));

        int result = builder.AddLighting(lighting);
        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items = builder.DetachItems();
        Direct3D9FixedFunctionPipelineItem item = items[1];
        Direct3D9FixedFunctionPipelineItemBuilder.ReleaseOwnedColorSources(items);

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.Diffuse, 1u, uint.MaxValue, Direct3D9ColorSourceType.Programmatic, Direct3D9VertexFormatAttribute.Diffuse, "ReleaseLighting"),
            (result, item.SourceLocation, item.Stage, item.Sampler, item.ColorSource!.SourceType, builder.GeneratedVertexFormat, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenLightingReferencesIncomingDiffuseThenBuilderDoesNotGenerateDiffuse()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new(
            Direct3D9VertexFormatAttribute.Diffuse | Direct3D9VertexFormatAttribute.Uv1);
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0)));

        int result = builder.AddLighting(new Direct3D9LightingColorSource());
        Direct3D9FixedFunctionPipelineItem item = builder.DetachItems()[1];

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.Diffuse, Direct3D9VertexFormatAttribute.None),
            (result, item.SourceLocation, builder.GeneratedVertexFormat));
    }

    [TestMethod]
    public void WhenDiffuseWasAlreadyGeneratedThenLightingFailsWithoutTakingOwnership()
    {
        List<string> calls = [];
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0)));
        Assert.AreEqual(0, builder.MultiplyConstantAlpha(0.5f));
        using Direct3D9LightingColorSource lighting = new(() => calls.Add("ReleaseLighting"));

        int result = builder.AddLighting(lighting);

        Assert.AreEqual(
            (Direct3D9Factory.UnsupportedOperationHResult, 2, string.Empty),
            (result, builder.DetachItems().Count, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenLightingIsDisposedThenPipelineCallbacksAreRejected()
    {
        Direct3D9LightingColorSource lighting = new();
        lighting.Dispose();

        Assert.Throws<ObjectDisposedException>(() => lighting.PipelineColorSource.Realize());
    }

    [TestMethod]
    public void WhenRendererLightingCreationFailsThenPipelineInitializationPreservesFirstFailure()
    {
        int lightingFailure = Direct3D9Factory.GenericFailureHResult;
        Direct3D9GeometryRenderer<uint> renderer = CreateRenderer(
            (out Direct3D9LightingColorSource? colorSource) =>
            {
                colorSource = null;
                return lightingFailure;
            });
        Direct3D9FixedFunctionPassInputs passInputs = CreateConstantPassInputs();

        int result = InitializeGeometryPipeline(ref renderer, passInputs, [], out Direct3D9Pipeline? pipeline);

        Assert.AreEqual((lightingFailure, null), (result, pipeline));
    }

    [TestMethod]
    public void WhenRendererLightingCreationFailsWithSourceThenFirstFailureIsPreservedAndSourceIsReleased()
    {
        List<string> calls = [];
        int lightingFailure = Direct3D9Factory.GenericFailureHResult;
        Direct3D9GeometryRenderer<uint> renderer = CreateRenderer(
            (out Direct3D9LightingColorSource? colorSource) =>
            {
                colorSource = new(() => calls.Add("ReleaseLighting"));
                return lightingFailure;
            });
        Direct3D9FixedFunctionPassInputs passInputs = CreateConstantPassInputs();

        int result = InitializeGeometryPipeline(ref renderer, passInputs, calls, out Direct3D9Pipeline? pipeline);

        Assert.AreEqual((lightingFailure, null, "ReleaseLighting"), (result, pipeline, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenRendererLightingCreationSucceedsWithoutSourceThenOutOfMemoryIsReturned()
    {
        Direct3D9GeometryRenderer<uint> renderer = CreateRenderer(
            (out Direct3D9LightingColorSource? colorSource) =>
            {
                colorSource = null;
                return 0;
            });
        Direct3D9FixedFunctionPassInputs passInputs = CreateConstantPassInputs();

        int result = InitializeGeometryPipeline(ref renderer, passInputs, [], out Direct3D9Pipeline? pipeline);

        Assert.AreEqual((Direct3D9Factory.OutOfMemoryHResult, null), (result, pipeline));
    }

    [TestMethod]
    public void WhenRendererLightingCannotBeAddedThenCreatedSourceIsReleased()
    {
        List<string> calls = [];
        Direct3D9GeometryRenderer<uint> renderer = CreateRenderer(
            (out Direct3D9LightingColorSource? colorSource) =>
            {
                colorSource = new(() => calls.Add("ReleaseLighting"));
                return 0;
            });
        Direct3D9FixedFunctionPassInputs passInputs = CreateConstantPassInputs();

        int result = InitializeGeometryPipeline(ref renderer, passInputs, calls, out Direct3D9Pipeline? pipeline);

        Assert.AreEqual(
            (Direct3D9Factory.UnsupportedOperationHResult, null, "ReleaseLighting"),
            (result, pipeline, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenRendererLightingIsAddedThenPipelineOwnsSourceUntilRelease()
    {
        List<string> calls = [];
        Direct3D9GeometryRenderer<uint> renderer = CreateRenderer(
            (out Direct3D9LightingColorSource? colorSource) =>
            {
                calls.Add("CreateLighting");
                colorSource = new(() => calls.Add("ReleaseLighting"));
                return 0;
            });
        Direct3D9FixedFunctionPassInputs passInputs = new(
            builder => builder.SetTexture(new Direct3D9PipelineColorSource(
                Direct3D9ColorSourceType.Texture,
                () => 0,
                SendVertexMapping: (_, _, _) => 0)));

        int result = InitializeGeometryPipeline(ref renderer, passInputs, calls, out Direct3D9Pipeline? pipeline);
        string callsBeforeRelease = string.Join('|', calls);
        pipeline!.ReleaseExpensiveResources();

        Assert.AreEqual(
            (0, "CreateLighting|SetupVertexBuilder|Finalize:17", "CreateLighting|SetupVertexBuilder|Finalize:17|ReleaseLighting|ReleaseColors|ReleaseBuilder:17"),
            (result, callsBeforeRelease, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPassEffectProcessingFailsThenPrimaryBitmapOwnershipIsReleasedAndPipelineIsNotCreated()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (_, _, _) => 0);
        Direct3D9BitmapPipelineColorSource ownership = new(
            31,
            new CallbackDisposable(() => calls.Add("DisposeRealizer")),
            colorSource,
            source => calls.Add($"Release:{source}"));
        Direct3D9FixedFunctionPassInputs passInputs = new(
            builder =>
            {
                int result = builder.SetTexture(ownership);
                ownership.Dispose();
                return result;
            },
            (_, _) => Direct3D9Factory.GenericFailureHResult,
            effectContext: 11);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [new(0, 0), new(0, 0), new(0, 0)],
            [],
            uint.MaxValue);

        int result = Direct3D9FixedFunctionGeometryPipeline.Initialize(
            ref renderer,
            passInputs,
            items => CreateFixedFunctionPipelineInitializer(calls, items),
            null,
            true,
            _ => 0,
            () => 0,
            () => false,
            (out nint vertexBuffer) =>
            {
                vertexBuffer = 0;
                return 0;
            },
            () => calls.Add("ReleaseColors"),
            out Direct3D9Pipeline? pipeline);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, null, "DisposeRealizer|Release:31"),
            (result, pipeline, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPrimaryConstantHasNoIncomingDiffuseThenBuilderGeneratesDiffuse()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        Direct3D9ConstantColorSource source = new(new MilColorF(1, 0.25f, 0.5f, 0.75f));

        int result = builder.SetConstant(source);
        Direct3D9FixedFunctionPipelineItem item = builder.DetachItems()[0];

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.Diffuse, 0u, uint.MaxValue, Direct3D9FixedFunctionBlendArgument.Diffuse),
            (result, item.SourceLocation, item.Stage, item.Sampler, item.Source1));
    }

    [TestMethod]
    public void WhenPrimaryConstantCannotGenerateDiffuseThenBuilderReusesIncomingUv1Texture()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new(
            Direct3D9VertexFormatAttribute.Diffuse | Direct3D9VertexFormatAttribute.Uv1);
        Direct3D9ConstantColorSource source = new(new MilColorF(1, 0.25f, 0.5f, 0.75f));

        int result = builder.SetConstant(source);
        Direct3D9FixedFunctionPipelineItem item = builder.DetachItems()[0];

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.Uv1, 0u, 0u, Direct3D9FixedFunctionBlendArgument.Texture),
            (result, item.SourceLocation, item.Stage, item.Sampler, item.Source1));
    }

    [TestMethod]
    public void WhenPassInputsContainIncomingDiffuseAndUv1ThenPrimaryConstantUsesTextureFallback()
    {
        Direct3D9FixedFunctionPassInputs passInputs = new(
            builder => builder.SetConstant(new Direct3D9ConstantColorSource(new MilColorF(1, 1, 1, 1))),
            incomingVertexFormat: Direct3D9VertexFormatAttribute.Diffuse | Direct3D9VertexFormatAttribute.Uv1);

        int result = passInputs.CreateItems(out IReadOnlyList<Direct3D9FixedFunctionPipelineItem>? items);

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.Uv1, Direct3D9FixedFunctionBlendArgument.Texture),
            (result, items![0].SourceLocation, items[0].Source1));
        Direct3D9FixedFunctionPipelineItemBuilder.ReleaseOwnedColorSources(items);
    }

    [TestMethod]
    public void WhenPrimaryConstantUsesUv1ThenSolidTextureIsCreatedLazilyAndReleasedWithBuilderOwnership()
    {
        List<string> calls = [];
        Direct3D9ConstantColorSource source = new(
            new MilColorF(1, 0.25f, 0.5f, 0.75f),
            (MilColorF _, out Direct3D9BitmapPipelineColorSource? colorSource) =>
            {
                calls.Add("CreateTexture");
                Direct3D9PipelineColorSource pipelineSource = new(
                    Direct3D9ColorSourceType.Texture,
                    () => 0,
                    SendVertexMapping: (vertexBuilder, location, _) =>
                    {
                        calls.Add($"Mapping:{vertexBuilder}:{location}");
                        return 0;
                    });
                colorSource = new Direct3D9BitmapPipelineColorSource(
                    31,
                    new CallbackDisposable(() => calls.Add("DisposeRealizer")),
                    pipelineSource,
                    value => calls.Add($"Release:{value}"));
                return 0;
            });
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new(
            Direct3D9VertexFormatAttribute.Diffuse | Direct3D9VertexFormatAttribute.Uv1);
        Assert.AreEqual(0, builder.SetConstant(source));
        Direct3D9FixedFunctionPipelineItem item = builder.DetachItems()[0];

        int firstResult = item.ColorSource!.SendVertexMapping!(17, item.SourceLocation, null);
        int secondResult = item.ColorSource.SendVertexMapping(17, item.SourceLocation, null);
        Direct3D9FixedFunctionPipelineItemBuilder.ReleaseOwnedColorSources([item]);

        Assert.AreEqual(
            (0, 0, "CreateTexture|Mapping:17:Uv1|Mapping:17:Uv1|DisposeRealizer|Release:31"),
            (firstResult, secondResult, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPrimaryTextureHasNoIncomingUv1ThenBuilderGeneratesUv1()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();

        int result = builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0));
        Direct3D9FixedFunctionPipelineItem item = builder.DetachItems()[0];

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.Uv1, Direct3D9VertexFormatAttribute.Uv1),
            (result, item.SourceLocation, builder.GeneratedVertexFormat));
    }

    [TestMethod]
    public void WhenPrimaryTextureReferencesIncomingUv1ThenBuilderDoesNotGenerateUv1()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new(Direct3D9VertexFormatAttribute.Uv1);

        int result = builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0));

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.None),
            (result, builder.GeneratedVertexFormat));
    }

    [TestMethod]
    public void WhenAlphaMaskUsesGeneratedVerticesThenBuilderGeneratesUv2AfterPrimaryUv1()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0)));

        int result = builder.MultiplyAlphaMask(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0));
        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items = builder.DetachItems();

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.Uv2, Direct3D9VertexFormatAttribute.Uv2),
            (result, items[1].SourceLocation, builder.GeneratedVertexFormat));
    }

    [TestMethod]
    public void WhenAlphaMaskUsesPreGeneratedVerticesThenBuilderReferencesIncomingUv1()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new(Direct3D9VertexFormatAttribute.Uv1);
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0)));

        int result = builder.MultiplyAlphaMask(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0));
        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items = builder.DetachItems();

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.Uv1, Direct3D9VertexFormatAttribute.None),
            (result, items[1].SourceLocation, builder.GeneratedVertexFormat));
    }

    [TestMethod]
    public void WhenAlphaScaleIsOneThenNoScalableColorSourceIsCreated()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0)));

        int result = builder.ProcessAlphaScaleEffect(sizeof(float), 0, 1, _ => throw new AssertFailedException());

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenAlphaScaleCanBeAppliedToExistingSourceThenItIsFoldedIntoThatSource()
    {
        float appliedScale = 0;
        Direct3D9PipelineColorSource scalableSource = new(
            Direct3D9ColorSourceType.Constant,
            () => 0,
            AlphaScale: scale => appliedScale = scale);
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        Assert.AreEqual(0, builder.SetTexture(scalableSource));

        int result = builder.ProcessAlphaScaleEffect(sizeof(float), 0, 0.25f, _ => throw new AssertFailedException());

        Assert.AreEqual((0, 0.25f, 1), (result, appliedScale, builder.DetachItems().Count));
    }

    [TestMethod]
    public void WhenAlphaScaleCannotBeFoldedThenProductionDiffuseSourceIsAdded()
    {
        Direct3D9PipelineColorSource primary = new(Direct3D9ColorSourceType.Texture, () => 0);
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        Assert.AreEqual(0, builder.SetTexture(primary));

        int result = builder.ProcessAlphaScaleEffect(sizeof(float), 0, 0.5f);
        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items = builder.DetachItems();

        Assert.AreEqual(
            (0, 2, Direct3D9VertexFormatAttribute.Diffuse, 1u, Direct3D9FixedFunctionBlendOperation.Multiply, Direct3D9FixedFunctionBlendArgument.Diffuse, Direct3D9FixedFunctionBlendArgument.Current, true),
            (result, items.Count, items[1].SourceLocation, items[1].Stage, items[1].BlendOperation, items[1].Source1, items[1].Source2, items[1].ColorSource?.SendConstantVertexMapping is not null));
    }

    [TestMethod]
    public void WhenAlphaScaleCannotUseDiffuseThenIncomingUv1IsReusedBeforeSpecular()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new(
            Direct3D9VertexFormatAttribute.Diffuse | Direct3D9VertexFormatAttribute.Uv1);
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0)));

        int result = builder.ProcessAlphaScaleEffect(
            sizeof(float),
            0,
            0.5f,
            _ => new Direct3D9PipelineColorSource(
                Direct3D9ColorSourceType.Constant | Direct3D9ColorSourceType.Texture,
                () => 0));
        Direct3D9FixedFunctionPipelineItem item = builder.DetachItems()[1];

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.Uv1, 1u, 1u, Direct3D9FixedFunctionBlendArgument.Texture, Direct3D9VertexFormatAttribute.None),
            (result, item.SourceLocation, item.Stage, item.Sampler, item.Source1, builder.GeneratedVertexFormat));
    }

    [TestMethod]
    public void WhenAlphaScaleCannotUseDiffuseOrIncomingUv1ThenSpecularIsGenerated()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new(Direct3D9VertexFormatAttribute.Diffuse);
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0)));

        int result = builder.ProcessAlphaScaleEffect(sizeof(float), 0, 0.5f);
        Direct3D9FixedFunctionPipelineItem item = builder.DetachItems()[1];

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.Specular, uint.MaxValue, Direct3D9FixedFunctionBlendArgument.Specular, Direct3D9VertexFormatAttribute.Uv1 | Direct3D9VertexFormatAttribute.Specular),
            (result, item.SourceLocation, item.Sampler, item.Source1, builder.GeneratedVertexFormat));
    }

    [TestMethod]
    public void WhenAlphaScaleHasNoAvailableVertexFieldThenSourceFactoryIsNotCalled()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new(
            Direct3D9VertexFormatAttribute.Diffuse | Direct3D9VertexFormatAttribute.Specular);
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0)));

        int result = builder.ProcessAlphaScaleEffect(
            sizeof(float),
            0,
            0.5f,
            _ => throw new AssertFailedException());

        Assert.AreEqual(Direct3D9Factory.UnsupportedOperationHResult, result);
    }

    [TestMethod]
    public void WhenAlphaScaleSourceFactoryFailsThenVertexFieldRemainsAvailable()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0)));
        Assert.AreEqual(
            Direct3D9Factory.OutOfMemoryHResult,
            builder.ProcessAlphaScaleEffect(sizeof(float), 0, 0.5f, _ => null));

        int result = builder.ProcessAlphaScaleEffect(sizeof(float), 0, 0.5f);
        Direct3D9FixedFunctionPipelineItem item = builder.DetachItems()[1];

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.Diffuse, Direct3D9VertexFormatAttribute.Uv1 | Direct3D9VertexFormatAttribute.Diffuse),
            (result, item.SourceLocation, builder.GeneratedVertexFormat));
    }

    [TestMethod]
    public void WhenProductionAlphaSourceMapsDiffuseThenPremultipliedSrgbColorIsFilled()
    {
        Direct3D9ConstantAlphaScalableColorSource source = new(0.5f);
        nint mappedBuilder = 0;
        Direct3D9VertexFormatAttribute mappedLocation = Direct3D9VertexFormatAttribute.None;
        uint mappedColor = 0;

        int result = source.PipelineColorSource.SendConstantVertexMapping!(
            17,
            Direct3D9VertexFormatAttribute.Diffuse,
            (builder, location, color) =>
            {
                mappedBuilder = builder;
                mappedLocation = location;
                mappedColor = color;
                return Direct3D9Factory.SuccessHResult;
            });

        Assert.AreEqual((0, (nint) 17, Direct3D9VertexFormatAttribute.Diffuse, 0x80808080u), (result, mappedBuilder, mappedLocation, mappedColor));
    }

    [TestMethod]
    public void WhenProductionAlphaSourceIsScaledThenMappedColorUsesAccumulatedAlpha()
    {
        Direct3D9ConstantAlphaScalableColorSource source = new(0.5f);
        source.PipelineColorSource.AlphaScale!(0.5f);
        uint mappedColor = 0;

        int result = source.PipelineColorSource.SendConstantVertexMapping!(
            17,
            Direct3D9VertexFormatAttribute.Specular,
            (_, _, color) =>
            {
                mappedColor = color;
                return Direct3D9Factory.SuccessHResult;
            });

        Assert.AreEqual((0, 0x40404040u), (result, mappedColor));
    }

    [TestMethod]
    public void WhenProductionConstantMappingFailsThenFirstHResultIsReturned()
    {
        Direct3D9ConstantAlphaScalableColorSource source = new(0.5f);

        int result = source.PipelineColorSource.SendConstantVertexMapping!(
            17,
            Direct3D9VertexFormatAttribute.Diffuse,
            (_, _, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenProductionAlphaSourceIsInitializedThenFixedFunctionInitializerMapsDiffuseConstant()
    {
        List<string> calls = [];
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (_, _, _) => 0)));
        Assert.AreEqual(0, builder.ProcessAlphaScaleEffect(sizeof(float), 0, 0.5f));
        Direct3D9FixedFunctionPipelineInitializer initializer = CreateFixedFunctionPipelineInitializer(
            calls,
            builder.DetachItems(),
            setConstantMapping: (vertexBuilder, location, color) =>
            {
                calls.Add($"Constant:{vertexBuilder}:{location}:{color:X8}");
                return Direct3D9Factory.SuccessHResult;
            });

        int result = InitializeFixedFunctionPipeline(initializer, calls, null, true, out _);

        Assert.AreEqual((0, "SetupVertexBuilder|Constant:17:Diffuse:80808080|Finalize:17"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow((uint) 0, (uint) 0, 0.5f)]
    [DataRow((uint) sizeof(float), (uint) 1, 0.5f)]
    [DataRow((uint) sizeof(float), (uint) 0, -0.1f)]
    [DataRow((uint) sizeof(float), (uint) 0, 1.1f)]
    [DataRow((uint) sizeof(float), (uint) 0, float.NaN)]
    public void WhenAlphaScaleParametersAreInvalidThenUnsupportedOperationIsReturned(
        uint parameterSize,
        uint resourceCount,
        float alpha)
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0)));

        int result = builder.ProcessAlphaScaleEffect(parameterSize, resourceCount, alpha, _ => throw new AssertFailedException());

        Assert.AreEqual(Direct3D9Factory.UnsupportedOperationHResult, result);
    }

    [TestMethod]
    public void WhenFixedFunctionPipelineReleasesExpensiveResourcesThenOwnedBitmapSourceIsReleasedAfterDraw()
    { 
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendDeviceStates: (_, _) => 0,
            SendVertexMapping: (_, _, _) => 0);
        Direct3D9BitmapPipelineColorSource ownership = new(
            31,
            new CallbackDisposable(() => calls.Add("DisposeRealizer")),
            colorSource,
            source => calls.Add($"Release:{source}"));
        Direct3D9FixedFunctionPipelineInitializer initializer = CreateFixedFunctionPipelineInitializer(
            calls,
            [new Direct3D9FixedFunctionPipelineItem(Direct3D9VertexFormatAttribute.Uv1, ownership)]);
        ownership.Dispose();
        Assert.AreEqual(0, InitializeFixedFunctionPipeline(initializer, calls, null, true, out Direct3D9Pipeline? pipeline));
        Assert.AreEqual(0, pipeline!.Execute());
        Assert.AreEqual(0, pipeline.Execute());

        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual(
            "SetupVertexBuilder|Finalize:17|Begin|Geometry|Flush|ReleaseBuilder:17|Disable:1|VertexFormat:23|AlphaBlend|PixelShader:null|VertexShader:null|Draw:23|DisposeRealizer|Release:31|ReleaseColors",
            string.Join('|', calls));
    }

    [TestMethod]
    public void WhenFixedFunctionMappingFailsThenOwnedBitmapSourceIsReleasedInReverseOrder()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (_, _, _) => Direct3D9Factory.GenericFailureHResult);
        Direct3D9BitmapPipelineColorSource ownership = new(
            31,
            new CallbackDisposable(() => calls.Add("DisposeRealizer")),
            colorSource,
            source => calls.Add($"Release:{source}"));
        Direct3D9FixedFunctionPipelineInitializer initializer = CreateFixedFunctionPipelineInitializer(
            calls,
            [new Direct3D9FixedFunctionPipelineItem(Direct3D9VertexFormatAttribute.Uv1, ownership)]);
        ownership.Dispose();

        int result = InitializeFixedFunctionPipeline(initializer, calls, null, true, out Direct3D9Pipeline? pipeline);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, null, "SetupVertexBuilder|ReleaseBuilder:17|DisposeRealizer|Release:31"),
            (result, pipeline, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenFixedFunctionVerticesArePreGeneratedThenNullBuilderIsMappedAndRealBuilderIsFinalized()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, _, _) =>
            {
                calls.Add($"Mapping:{builder}");
                return 0;
            });
        Direct3D9FixedFunctionPipelineInitializer initializer = CreateFixedFunctionPipelineInitializer(
            calls,
            [new Direct3D9FixedFunctionPipelineItem(Direct3D9VertexFormatAttribute.Uv1, colorSource)],
            verticesArePreGenerated: () => true);

        int result = InitializeFixedFunctionPipeline(initializer, calls, null, true, out _);

        Assert.AreEqual((0, "SetupVertexBuilder|Mapping:0|Finalize:17"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenFixedFunctionVertexMappingFailsThenBuilderIsReleasedAndPipelineIsNotCreated()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (_, _, _) => Direct3D9Factory.GenericFailureHResult);
        Direct3D9FixedFunctionPipelineInitializer initializer = CreateFixedFunctionPipelineInitializer(
            calls,
            [new Direct3D9FixedFunctionPipelineItem(Direct3D9VertexFormatAttribute.Uv1, colorSource)]);

        int result = InitializeFixedFunctionPipeline(initializer, calls, null, true, out Direct3D9Pipeline? pipeline);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, null, "SetupVertexBuilder|ReleaseBuilder:17"),
            (result, pipeline, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenFixedFunctionMappingFinalizationFailsThenBuilderIsReleasedAndPipelineIsNotCreated()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (_, _, _) => 0);
        Direct3D9FixedFunctionPipelineInitializer initializer = CreateFixedFunctionPipelineInitializer(
            calls,
            [new Direct3D9FixedFunctionPipelineItem(Direct3D9VertexFormatAttribute.Uv1, colorSource)],
            finalizeResult: Direct3D9Factory.InvalidCallHResult);

        int result = InitializeFixedFunctionPipeline(initializer, calls, null, true, out Direct3D9Pipeline? pipeline);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, null, "SetupVertexBuilder|Finalize:17|ReleaseBuilder:17"),
            (result, pipeline, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(0, Direct3D9Factory.GenericFailureHResult, "SetupVertexBuilder|FirstMapping:17:Uv1|ReleaseBuilder:17|ReleaseFirst|ReleaseSecond")]
    [DataRow(1, Direct3D9Factory.OutOfVideoMemoryHResult, "SetupVertexBuilder|FirstMapping:17:Uv1|SecondMapping:17:Uv2|ReleaseBuilder:17|ReleaseFirst|ReleaseSecond")]
    [DataRow(2, Direct3D9Factory.InvalidCallHResult, "SetupVertexBuilder|FirstMapping:17:Uv1|SecondMapping:17:Uv2|Finalize:17|ReleaseBuilder:17|ReleaseFirst|ReleaseSecond")]
    public void WhenFixedFunctionVertexMappingOrFinalizationFailsThenBuilderAndOwnedSourcesAreReleased(
        int failureStep,
        int failureResult,
        string expectedCalls)
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource firstColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"FirstMapping:{builder}:{location}");
                return failureStep == 0 ? failureResult : 0;
            });
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"SecondMapping:{builder}:{location}");
                return failureStep == 1 ? failureResult : 0;
            });
        Direct3D9FixedFunctionPipelineInitializer initializer = CreateFixedFunctionPipelineInitializer(
            calls,
            [
                new Direct3D9FixedFunctionPipelineItem(
                    Direct3D9VertexFormatAttribute.Uv1,
                    firstColorSource,
                    lifetimeOwner: new CallbackDisposable(() => calls.Add("ReleaseFirst"))),
                new Direct3D9FixedFunctionPipelineItem(
                    Direct3D9VertexFormatAttribute.Uv2,
                    secondColorSource,
                    lifetimeOwner: new CallbackDisposable(() => calls.Add("ReleaseSecond"))),
            ],
            finalizeResult: failureStep == 2 ? failureResult : 0);

        int result = InitializeFixedFunctionPipeline(
            initializer,
            calls,
            new Direct3D9SurfaceRect(1, 2, 30, 40),
            false,
            out Direct3D9Pipeline? pipeline);

        Assert.AreEqual((failureResult, null, expectedCalls), (result, pipeline, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenFixedFunctionVertexBuilderSetupFailsAfterReturningBuilderThenOwnedSourcesAreReleased()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource firstColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (_, _, _) => throw new AssertFailedException());
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (_, _, _) => throw new AssertFailedException());
        Direct3D9FixedFunctionPipelineInitializer initializer = CreateFixedFunctionPipelineInitializer(
            calls,
            [
                new Direct3D9FixedFunctionPipelineItem(
                    Direct3D9VertexFormatAttribute.Uv1,
                    firstColorSource,
                    lifetimeOwner: new CallbackDisposable(() => calls.Add("ReleaseFirst"))),
                new Direct3D9FixedFunctionPipelineItem(
                    Direct3D9VertexFormatAttribute.Uv2,
                    secondColorSource,
                    lifetimeOwner: new CallbackDisposable(() => calls.Add("ReleaseSecond"))),
            ],
            setupVertexBuilderResult: Direct3D9Factory.OutOfVideoMemoryHResult);

        int result = InitializeFixedFunctionPipeline(
            initializer,
            calls,
            new Direct3D9SurfaceRect(1, 2, 30, 40),
            false,
            out Direct3D9Pipeline? pipeline);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, null, "SetupVertexBuilder|ReleaseBuilder:17|ReleaseFirst|ReleaseSecond"),
            (result, pipeline, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderPipelineItemHasTextureTransformThenResetPrecedesHandleAssignment()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            ResetForPipelineReuse: () => calls.Add("Reset"),
            SetTextureTransformHandle: handle => calls.Add($"Handle:{handle}"));

        _ = new Direct3D9ShaderPipelineItem(
            2,
            colorSource,
            Direct3D9VertexFormatAttribute.Uv1,
            textureTransformHandle: 13);

        Assert.AreEqual("Reset|Handle:13", string.Join('|', calls));
    }

    [TestMethod]
    public void WhenVertexBuilderIsCreatedThenItemMappingsAreSentBeforeFinalization()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"Mapping:{builder}:{location}");
                return 0;
            });
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(
            true,
            calls,
            items: [new Direct3D9ShaderPipelineItem(0, colorSource, Direct3D9VertexFormatAttribute.Uv2)],
            finalizeVertexMappings: builder =>
            {
                calls.Add($"Finalize:{builder}");
                return 0;
            });

        int result = initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 7, null, true);

        Assert.AreEqual(
            (0, "Setup:True:SourceOver:3:5:0:7|SetupVertexBuilder|Mapping:17:Uv2|Finalize:17|GetShader"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenVerticesArePreGeneratedThenNullBuilderIsMappedAndRealBuilderIsFinalized()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, _, _) =>
            {
                calls.Add($"Mapping:{builder}");
                return 0;
            });
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(
            true,
            calls,
            items: [new Direct3D9ShaderPipelineItem(0, colorSource, Direct3D9VertexFormatAttribute.Uv1)],
            finalizeVertexMappings: builder =>
            {
                calls.Add($"Finalize:{builder}");
                return 0;
            },
            verticesArePreGenerated: () => true);

        int result = initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 7, null, true);

        Assert.AreEqual(
            (0, "Setup:True:SourceOver:3:5:0:7|SetupVertexBuilder|Mapping:0|Finalize:17|GetShader"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenVertexMappingFailsThenBuilderIsReleasedAndFinalizationIsSkipped()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (_, _, _) =>
            {
                calls.Add("Mapping");
                return Direct3D9Factory.GenericFailureHResult;
            });
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(
            true,
            calls,
            items: [new Direct3D9ShaderPipelineItem(0, colorSource, Direct3D9VertexFormatAttribute.Uv1)],
            finalizeVertexMappings: _ =>
            {
                calls.Add("Finalize");
                return 0;
            },
            releaseVertexBuilder: builder => calls.Add($"ReleaseVertexBuilder:{builder}"));

        int result = initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 7, null, true);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, "Setup:True:SourceOver:3:5:0:7|SetupVertexBuilder|Mapping|ReleaseVertexBuilder:17"),
            (result, initializer.VertexBuilder, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenVertexMappingFinalizationFailsThenBuilderIsReleased()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (_, _, _) =>
            {
                calls.Add("Mapping");
                return 0;
            });
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(
            true,
            calls,
            items: [new Direct3D9ShaderPipelineItem(0, colorSource, Direct3D9VertexFormatAttribute.Uv1)],
            finalizeVertexMappings: _ =>
            {
                calls.Add("Finalize");
                return Direct3D9Factory.InvalidCallHResult;
            },
            releaseVertexBuilder: builder => calls.Add($"ReleaseVertexBuilder:{builder}"));

        int result = initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 7, null, true);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, "Setup:True:SourceOver:3:5:0:7|SetupVertexBuilder|Mapping|Finalize|ReleaseVertexBuilder:17"),
            (result, initializer.VertexBuilder, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(0, Direct3D9Factory.GenericFailureHResult, "Setup:True:SourceOver:3:5:0:7|SetupVertexBuilder|FirstMapping:17:Uv1|ReleaseVertexBuilder:17|ReleaseFirst|ReleaseSecond")]
    [DataRow(1, Direct3D9Factory.OutOfVideoMemoryHResult, "Setup:True:SourceOver:3:5:0:7|SetupVertexBuilder|FirstMapping:17:Uv1|SecondMapping:17:Uv2|ReleaseVertexBuilder:17|ReleaseFirst|ReleaseSecond")]
    [DataRow(2, Direct3D9Factory.InvalidCallHResult, "Setup:True:SourceOver:3:5:0:7|SetupVertexBuilder|FirstMapping:17:Uv1|SecondMapping:17:Uv2|Finalize:17|ReleaseVertexBuilder:17|ReleaseFirst|ReleaseSecond")]
    public void WhenShaderVertexMappingOrFinalizationFailsThenShaderIsSkippedAndOwnedSourcesRemainForCleanup(
        int failureStep,
        int failureResult,
        string expectedCalls)
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource firstColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"FirstMapping:{builder}:{location}");
                return failureStep == 0 ? failureResult : 0;
            });
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"SecondMapping:{builder}:{location}");
                return failureStep == 1 ? failureResult : 0;
            });
        Direct3D9BitmapPipelineColorSource firstOwnership = new(
            31,
            new CallbackDisposable(() => calls.Add("ReleaseFirst")),
            firstColorSource,
            static _ => { });
        Direct3D9BitmapPipelineColorSource secondOwnership = new(
            37,
            new CallbackDisposable(() => calls.Add("ReleaseSecond")),
            secondColorSource,
            static _ => { });
        Direct3D9ShaderPipelineItem firstItem = new(0, firstOwnership, Direct3D9VertexFormatAttribute.Uv1);
        Direct3D9ShaderPipelineItem secondItem = new(1, secondOwnership, Direct3D9VertexFormatAttribute.Uv2);
        firstOwnership.Dispose();
        secondOwnership.Dispose();
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(
            true,
            calls,
            items: [firstItem, secondItem],
            finalizeVertexMappings: builder =>
            {
                calls.Add($"Finalize:{builder}");
                return failureStep == 2 ? failureResult : 0;
            },
            releaseVertexBuilder: builder => calls.Add($"ReleaseVertexBuilder:{builder}"));

        int result = initializer.InitializeForRendering(
            MilCompositingMode.SourceOver,
            3,
            5,
            0,
            7,
            new Direct3D9SurfaceRect(1, 2, 30, 40),
            false);
        firstItem.Ownership!.Dispose();
        secondItem.Ownership!.Dispose();

        Assert.AreEqual(
            (failureResult, nint.Zero, nint.Zero, nint.Zero, expectedCalls),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderAcquisitionFailsAfterBoundsThenBuilderAndGeometryRemainForCleanup()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource firstColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"FirstMapping:{builder}:{location}");
                return 0;
            });
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"SecondMapping:{builder}:{location}");
                return 0;
            });
        Direct3D9BitmapPipelineColorSource firstOwnership = new(
            31,
            new CallbackDisposable(() => calls.Add("ReleaseFirst")),
            firstColorSource,
            static _ => { });
        Direct3D9BitmapPipelineColorSource secondOwnership = new(
            37,
            new CallbackDisposable(() => calls.Add("ReleaseSecond")),
            secondColorSource,
            static _ => { });
        Direct3D9ShaderPipelineItem firstItem = new(0, firstOwnership, Direct3D9VertexFormatAttribute.Uv1);
        Direct3D9ShaderPipelineItem secondItem = new(1, secondOwnership, Direct3D9VertexFormatAttribute.Uv2);
        firstOwnership.Dispose();
        secondOwnership.Dispose();
        Direct3D9ShaderPipelineInitializer initializer = new(
            true,
            (_, _, _, _, _, _) =>
            {
                calls.Add("Setup");
                return 0;
            },
            (out nint vertexBuilder) =>
            {
                calls.Add("SetupVertexBuilder");
                vertexBuilder = 17;
                return 0;
            },
            (vertexBuilder, bounds, needInside) =>
                calls.Add($"Bounds:{vertexBuilder}:{bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}:{needInside}"),
            (out nint shader) =>
            {
                calls.Add("GetShader");
                shader = 23;
                return Direct3D9Factory.GenericFailureHResult;
            },
            shader => calls.Add($"ReleaseShader:{shader}"),
            [firstItem, secondItem],
            finalizeVertexMappings: builder =>
            {
                calls.Add($"Finalize:{builder}");
                return 0;
            });

        int result = initializer.InitializeForRendering(
            MilCompositingMode.SourceOver,
            3,
            5,
            0,
            7,
            new Direct3D9SurfaceRect(1, 2, 30, 40),
            false);
        firstItem.Ownership!.Dispose();
        secondItem.Ownership!.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 3, 17, nint.Zero, "Setup|SetupVertexBuilder|FirstMapping:17:Uv1|SecondMapping:17:Uv2|Finalize:17|Bounds:17:1,2,30,40:False|GetShader|ReleaseShader:23|ReleaseFirst|ReleaseSecond"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenFirstExecutionSucceedsThenGeometryIsBuiltFlushedAndBuilderIsReleased()
    {
        List<string> calls = [];
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            flushTryGetVertexBuffer: (out nint vertexBuffer) =>
            {
                calls.Add("Flush");
                vertexBuffer = 17;
                return 0;
            });

        int result = pipeline.Execute();

        Assert.AreEqual((0, "Begin|Geometry|Flush|ReleaseBuilder"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenGeometryIsEmptyWithoutOutsideBoundsThenExecutionSucceedsWithoutFlushing()
    {
        List<string> calls = [];
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendGeometry: () =>
            {
                calls.Add("Geometry");
                return Direct3D9Factory.EmptyFillHResult;
            });

        int result = pipeline.Execute();

        Assert.AreEqual((0, "Begin|Geometry|Outside:False"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenGeometryIsEmptyWithOutsideBoundsThenExecutionStillFlushes()
    {
        List<string> calls = [];
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendGeometry: () =>
            {
                calls.Add("Geometry");
                return Direct3D9Factory.EmptyFillHResult;
            },
            hasOutsideBounds: () =>
            {
                calls.Add("Outside:True");
                return true;
            });

        int result = pipeline.Execute();

        Assert.AreEqual((0, "Begin|Geometry|Outside:True|Flush|ReleaseBuilder"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(false, "Begin|Geometry|Outside|Flush:1|Begin|Geometry|Outside|Flush:2|ReleaseColors|ReleaseBuilder")]
    [DataRow(true, "Begin|Geometry|Outside|Flush:1|Begin|Geometry|Outside|Flush:2|ReleaseBuilder|State:17|Draw:17|ReleaseColors")]
    public void WhenEmptyGeometryHasOutsideBoundsThenFlushFailureRetriesAndCleanupMatchesCachedState(
        bool secondFlushSucceeds,
        string expectedCalls)
    {
        List<string> calls = [];
        int flushCount = 0;
        Direct3D9Pipeline pipeline = new(
            3,
            [],
            vertexBuffer =>
            {
                calls.Add($"State:{vertexBuffer}");
                return Direct3D9Factory.SuccessHResult;
            },
            vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return Direct3D9Factory.SuccessHResult;
            },
            () =>
            {
                calls.Add("Begin");
                return Direct3D9Factory.SuccessHResult;
            },
            () =>
            {
                calls.Add("Geometry");
                return Direct3D9Factory.EmptyFillHResult;
            },
            () =>
            {
                calls.Add("Outside");
                return true;
            },
            (out nint vertexBuffer) =>
            {
                flushCount++;
                calls.Add($"Flush:{flushCount}");
                vertexBuffer = secondFlushSucceeds && flushCount == 2 ? 17 : 0;
                return vertexBuffer == 0
                    ? Direct3D9Factory.GenericFailureHResult
                    : Direct3D9Factory.SuccessHResult;
            },
            () => calls.Add("ReleaseColors"),
            () => calls.Add("ReleaseBuilder"));

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, pipeline.Execute());
        Assert.AreEqual(
            secondFlushSucceeds ? Direct3D9Factory.SuccessHResult : Direct3D9Factory.GenericFailureHResult,
            pipeline.Execute());
        if (secondFlushSucceeds)
        {
            Assert.AreEqual(Direct3D9Factory.SuccessHResult, pipeline.Execute());
        }

        pipeline.ReleaseExpensiveResources();
        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual((expectedCalls, nint.Zero), (string.Join('|', calls), pipeline.GeometryGenerator));
    }

    [TestMethod]
    public void WhenGeometryGenerationFailsThenFailureIsPreservedWithoutFlushing()
    {
        List<string> calls = [];
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendGeometry: () =>
            {
                calls.Add("Geometry");
                return Direct3D9Factory.GenericFailureHResult;
            });

        int result = pipeline.Execute();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Begin|Geometry"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenVertexBufferWasCachedThenNextExecutionOnlySendsStateAndDraws()
    {
        List<string> calls = [];
        Direct3D9Pipeline pipeline = CreatePipeline(calls);
        Assert.AreEqual(0, pipeline.Execute());
        calls.Clear();

        int result = pipeline.Execute();

        Assert.AreEqual((0, "State:17|Draw:17"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSendingCachedVertexBufferStateFailsThenDrawingIsSkipped()
    {
        List<string> calls = [];
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendDeviceStates: vertexBuffer =>
            {
                calls.Add($"State:{vertexBuffer}");
                return Direct3D9Factory.GenericFailureHResult;
            });
        Assert.AreEqual(0, pipeline.Execute());
        calls.Clear();

        int result = pipeline.Execute();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "State:17"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDrawingCachedVertexBufferFailsThenFailureIsPreservedWithoutRebuildingGeometry()
    {
        List<string> calls = [];
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            drawPrimitive: vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return Direct3D9Factory.InvalidCallHResult;
            });
        Assert.AreEqual(0, pipeline.Execute());
        calls.Clear();

        int result = pipeline.Execute();

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, "State:17|Draw:17"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void WhenCachedVertexBufferExecutionFailsThenNextExecutionRetriesTheSameBuffer(bool failSendingState)
    {
        List<string> calls = [];
        int stateCall = 0;
        int drawCall = 0;
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendDeviceStates: vertexBuffer =>
            {
                calls.Add($"State:{vertexBuffer}");
                return failSendingState && stateCall++ == 0
                    ? Direct3D9Factory.GenericFailureHResult
                    : Direct3D9Factory.SuccessHResult;
            },
            drawPrimitive: vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return !failSendingState && drawCall++ == 0
                    ? Direct3D9Factory.InvalidCallHResult
                    : Direct3D9Factory.SuccessHResult;
            });
        Assert.AreEqual(0, pipeline.Execute());
        calls.Clear();
        Assert.IsTrue(pipeline.Execute() < 0);
        calls.Clear();

        int result = pipeline.Execute();

        Assert.AreEqual((0, "State:17|Draw:17"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void WhenThreeDimensionalShaderPipelineStateFailsThenCachedBufferIsRetried(bool failAlphaBlend)
    {
        List<string> calls = [];
        bool failNextState = true;
        Direct3D9PipelineColorSource colorSource = CreateShaderColorSource("Color", calls);
        Direct3D9ShaderPipelineDeviceStateSender sender = new(
            false,
            23,
            [new Direct3D9ShaderPipelineItem(5, colorSource)],
            vertexBuffer =>
            {
                calls.Add($"VertexFormat:{vertexBuffer}");
                return 0;
            },
            () =>
            {
                calls.Add("AlphaBlend");
                if (failAlphaBlend && failNextState)
                {
                    failNextState = false;
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return 0;
            },
            (shader, pipelineIs2D) =>
            {
                calls.Add($"ShaderState:{shader}:{pipelineIs2D}");
                if (!failAlphaBlend && failNextState)
                {
                    failNextState = false;
                    return Direct3D9Factory.InvalidCallHResult;
                }

                return 0;
            });
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendDeviceStates: sender.SendDeviceStates,
            drawPrimitive: vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return 0;
            });
        int cacheResult = pipeline.Execute();
        calls.Clear();

        int failureResult = pipeline.Execute();
        string failureCalls = string.Join('|', calls);
        calls.Clear();

        int retryResult = pipeline.Execute();

        Assert.AreEqual(
            failAlphaBlend
                ? (0, Direct3D9Factory.GenericFailureHResult, "ColorState:5:5|ColorData:23|AlphaBlend", 0, "ColorState:5:5|ColorData:23|AlphaBlend|ShaderState:23:False|Draw:17")
                : (0, Direct3D9Factory.InvalidCallHResult, "ColorState:5:5|ColorData:23|AlphaBlend|ShaderState:23:False", 0, "ColorState:5:5|ColorData:23|AlphaBlend|ShaderState:23:False|Draw:17"),
            (cacheResult, failureResult, failureCalls, retryResult, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void WhenThreeDimensionalShaderPipelineStateRecoversThenDrawFailureStillRetriesAndCleansUpBorrowedBuffer(bool failAlphaBlend)
    {
        List<string> calls = [];
        bool failNextState = true;
        bool failNextDraw = true;
        Direct3D9PipelineColorSource colorSource = CreateShaderColorSource("Color", calls);
        Direct3D9ShaderPipelineDeviceStateSender sender = new(
            false,
            23,
            [new Direct3D9ShaderPipelineItem(5, colorSource)],
            vertexBuffer =>
            {
                calls.Add($"VertexFormat:{vertexBuffer}");
                return 0;
            },
            () =>
            {
                calls.Add("AlphaBlend");
                if (failAlphaBlend && failNextState)
                {
                    failNextState = false;
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return 0;
            },
            (shader, pipelineIs2D) =>
            {
                calls.Add($"ShaderState:{shader}:{pipelineIs2D}");
                if (!failAlphaBlend && failNextState)
                {
                    failNextState = false;
                    return Direct3D9Factory.InvalidCallHResult;
                }

                return 0;
            });
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendDeviceStates: sender.SendDeviceStates,
            drawPrimitive: vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                if (failNextDraw)
                {
                    failNextDraw = false;
                    return Direct3D9Factory.OutOfVideoMemoryHResult;
                }

                return 0;
            });
        int cacheResult = pipeline.Execute();
        calls.Clear();

        int stateFailureResult = pipeline.Execute();
        string stateFailureCalls = string.Join('|', calls);
        calls.Clear();
        int drawFailureResult = pipeline.Execute();
        string drawFailureCalls = string.Join('|', calls);
        calls.Clear();
        int retryResult = pipeline.Execute();
        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual(
            failAlphaBlend
                ? (0, Direct3D9Factory.GenericFailureHResult, "ColorState:5:5|ColorData:23|AlphaBlend", Direct3D9Factory.OutOfVideoMemoryHResult, "ColorState:5:5|ColorData:23|AlphaBlend|ShaderState:23:False|Draw:17", 0, "ColorState:5:5|ColorData:23|AlphaBlend|ShaderState:23:False|Draw:17|ReleaseColors")
                : (0, Direct3D9Factory.InvalidCallHResult, "ColorState:5:5|ColorData:23|AlphaBlend|ShaderState:23:False", Direct3D9Factory.OutOfVideoMemoryHResult, "ColorState:5:5|ColorData:23|AlphaBlend|ShaderState:23:False|Draw:17", 0, "ColorState:5:5|ColorData:23|AlphaBlend|ShaderState:23:False|Draw:17|ReleaseColors"),
            (cacheResult, stateFailureResult, stateFailureCalls, drawFailureResult, drawFailureCalls, retryResult, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void WhenThreeDimensionalShaderPipelineMiddleColorSourceFailsThenCachedBufferRestartsAndCleansUp(bool failDeviceState)
    {
        List<string> calls = [];
        bool failNextColorPart = true;
        Direct3D9PipelineColorSource firstColorSource = CreateShaderColorSource("First", calls);
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => Direct3D9Factory.SuccessHResult,
            (textureStage, sampler) =>
            {
                calls.Add($"SecondState:{textureStage}:{sampler}");
                if (failDeviceState && failNextColorPart)
                {
                    failNextColorPart = false;
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return 0;
            },
            shader =>
            {
                calls.Add($"SecondData:{shader}");
                if (!failDeviceState && failNextColorPart)
                {
                    failNextColorPart = false;
                    return Direct3D9Factory.InvalidCallHResult;
                }

                return 0;
            });
        Direct3D9PipelineColorSource thirdColorSource = CreateShaderColorSource("Third", calls);
        Direct3D9ShaderPipelineDeviceStateSender sender = new(
            false,
            23,
            [
                new Direct3D9ShaderPipelineItem(1, firstColorSource),
                new Direct3D9ShaderPipelineItem(2, secondColorSource),
                new Direct3D9ShaderPipelineItem(3, thirdColorSource),
            ],
            vertexBuffer =>
            {
                calls.Add($"VertexFormat:{vertexBuffer}");
                return 0;
            },
            () =>
            {
                calls.Add("AlphaBlend");
                return 0;
            },
            (shader, pipelineIs2D) =>
            {
                calls.Add($"ShaderState:{shader}:{pipelineIs2D}");
                return 0;
            });
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendDeviceStates: sender.SendDeviceStates,
            drawPrimitive: vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return 0;
            });
        int cacheResult = pipeline.Execute();
        calls.Clear();

        int failureResult = pipeline.Execute();
        string failureCalls = string.Join('|', calls);
        calls.Clear();
        int retryResult = pipeline.Execute();
        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual(
            failDeviceState
                ? (0, Direct3D9Factory.GenericFailureHResult, "FirstState:1:1|FirstData:23|SecondState:2:2", 0, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|AlphaBlend|ShaderState:23:False|Draw:17|ReleaseColors")
                : (0, Direct3D9Factory.InvalidCallHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23", 0, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|AlphaBlend|ShaderState:23:False|Draw:17|ReleaseColors"),
            (cacheResult, failureResult, failureCalls, retryResult, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void WhenTwoDimensionalShaderPipelineMiddleColorSourceFailsThenCachedBufferRestartsAndCleansUp(bool failDeviceState)
    {
        List<string> calls = [];
        bool failNextColorPart = true;
        Direct3D9PipelineColorSource firstColorSource = CreateShaderColorSource("First", calls);
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => Direct3D9Factory.SuccessHResult,
            (textureStage, sampler) =>
            {
                calls.Add($"SecondState:{textureStage}:{sampler}");
                if (failDeviceState && failNextColorPart)
                {
                    failNextColorPart = false;
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return 0;
            },
            shader =>
            {
                calls.Add($"SecondData:{shader}");
                if (!failDeviceState && failNextColorPart)
                {
                    failNextColorPart = false;
                    return Direct3D9Factory.InvalidCallHResult;
                }

                return 0;
            });
        Direct3D9PipelineColorSource thirdColorSource = CreateShaderColorSource("Third", calls);
        Direct3D9ShaderPipelineDeviceStateSender sender = new(
            true,
            23,
            [
                new Direct3D9ShaderPipelineItem(1, firstColorSource),
                new Direct3D9ShaderPipelineItem(2, secondColorSource),
                new Direct3D9ShaderPipelineItem(3, thirdColorSource),
            ],
            vertexBuffer =>
            {
                calls.Add($"VertexFormat:{vertexBuffer}");
                return 0;
            },
            () =>
            {
                calls.Add("AlphaBlend");
                return 0;
            },
            (shader, pipelineIs2D) =>
            {
                calls.Add($"ShaderState:{shader}:{pipelineIs2D}");
                return 0;
            });
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendDeviceStates: sender.SendDeviceStates,
            drawPrimitive: vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return 0;
            });
        int cacheResult = pipeline.Execute();
        calls.Clear();

        int failureResult = pipeline.Execute();
        string failureCalls = string.Join('|', calls);
        calls.Clear();
        int retryResult = pipeline.Execute();
        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual(
            failDeviceState
                ? (0, Direct3D9Factory.GenericFailureHResult, "FirstState:1:1|FirstData:23|SecondState:2:2", 0, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend|ShaderState:23:True|Draw:17|ReleaseColors")
                : (0, Direct3D9Factory.InvalidCallHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23", 0, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend|ShaderState:23:True|Draw:17|ReleaseColors"),
            (cacheResult, failureResult, failureCalls, retryResult, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void WhenTwoDimensionalShaderPipelineColorSourceRecoversThenVertexFormatFailureRestartsAndCleansUp(bool failDeviceState)
    {
        List<string> calls = [];
        bool failNextColorPart = true;
        bool failNextVertexFormat = true;
        Direct3D9PipelineColorSource firstColorSource = CreateShaderColorSource("First", calls);
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => Direct3D9Factory.SuccessHResult,
            (textureStage, sampler) =>
            {
                calls.Add($"SecondState:{textureStage}:{sampler}");
                if (failDeviceState && failNextColorPart)
                {
                    failNextColorPart = false;
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return 0;
            },
            shader =>
            {
                calls.Add($"SecondData:{shader}");
                if (!failDeviceState && failNextColorPart)
                {
                    failNextColorPart = false;
                    return Direct3D9Factory.InvalidCallHResult;
                }

                return 0;
            });
        Direct3D9PipelineColorSource thirdColorSource = CreateShaderColorSource("Third", calls);
        Direct3D9ShaderPipelineDeviceStateSender sender = new(
            true,
            23,
            [
                new Direct3D9ShaderPipelineItem(1, firstColorSource),
                new Direct3D9ShaderPipelineItem(2, secondColorSource),
                new Direct3D9ShaderPipelineItem(3, thirdColorSource),
            ],
            vertexBuffer =>
            {
                calls.Add($"VertexFormat:{vertexBuffer}");
                if (failNextVertexFormat)
                {
                    failNextVertexFormat = false;
                    return Direct3D9Factory.OutOfVideoMemoryHResult;
                }

                return 0;
            },
            () =>
            {
                calls.Add("AlphaBlend");
                return 0;
            },
            (shader, pipelineIs2D) =>
            {
                calls.Add($"ShaderState:{shader}:{pipelineIs2D}");
                return 0;
            });
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendDeviceStates: sender.SendDeviceStates,
            drawPrimitive: vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return 0;
            });
        int cacheResult = pipeline.Execute();
        calls.Clear();

        int colorFailureResult = pipeline.Execute();
        string colorFailureCalls = string.Join('|', calls);
        calls.Clear();
        int vertexFormatFailureResult = pipeline.Execute();
        string vertexFormatFailureCalls = string.Join('|', calls);
        calls.Clear();
        int retryResult = pipeline.Execute();
        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual(
            failDeviceState
                ? (0, Direct3D9Factory.GenericFailureHResult, "FirstState:1:1|FirstData:23|SecondState:2:2", Direct3D9Factory.OutOfVideoMemoryHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17", 0, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend|ShaderState:23:True|Draw:17|ReleaseColors")
                : (0, Direct3D9Factory.InvalidCallHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23", Direct3D9Factory.OutOfVideoMemoryHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17", 0, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend|ShaderState:23:True|Draw:17|ReleaseColors"),
            (cacheResult, colorFailureResult, colorFailureCalls, vertexFormatFailureResult, vertexFormatFailureCalls, retryResult, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void WhenTwoDimensionalShaderPipelineVertexFormatRecoversThenAlphaBlendFailureRestartsAndCleansUp(bool failDeviceState)
    {
        List<string> calls = [];
        bool failNextColorPart = true;
        bool failNextVertexFormat = true;
        bool failNextAlphaBlend = true;
        Direct3D9PipelineColorSource firstColorSource = CreateShaderColorSource("First", calls);
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => Direct3D9Factory.SuccessHResult,
            (textureStage, sampler) =>
            {
                calls.Add($"SecondState:{textureStage}:{sampler}");
                if (failDeviceState && failNextColorPart)
                {
                    failNextColorPart = false;
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return 0;
            },
            shader =>
            {
                calls.Add($"SecondData:{shader}");
                if (!failDeviceState && failNextColorPart)
                {
                    failNextColorPart = false;
                    return Direct3D9Factory.InvalidCallHResult;
                }

                return 0;
            });
        Direct3D9PipelineColorSource thirdColorSource = CreateShaderColorSource("Third", calls);
        Direct3D9ShaderPipelineDeviceStateSender sender = new(
            true,
            23,
            [
                new Direct3D9ShaderPipelineItem(1, firstColorSource),
                new Direct3D9ShaderPipelineItem(2, secondColorSource),
                new Direct3D9ShaderPipelineItem(3, thirdColorSource),
            ],
            vertexBuffer =>
            {
                calls.Add($"VertexFormat:{vertexBuffer}");
                if (failNextVertexFormat)
                {
                    failNextVertexFormat = false;
                    return Direct3D9Factory.OutOfVideoMemoryHResult;
                }

                return 0;
            },
            () =>
            {
                calls.Add("AlphaBlend");
                if (failNextAlphaBlend)
                {
                    failNextAlphaBlend = false;
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return 0;
            },
            (shader, pipelineIs2D) =>
            {
                calls.Add($"ShaderState:{shader}:{pipelineIs2D}");
                return 0;
            });
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendDeviceStates: sender.SendDeviceStates,
            drawPrimitive: vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return 0;
            });
        int cacheResult = pipeline.Execute();
        calls.Clear();

        int colorFailureResult = pipeline.Execute();
        string colorFailureCalls = string.Join('|', calls);
        calls.Clear();
        int vertexFormatFailureResult = pipeline.Execute();
        string vertexFormatFailureCalls = string.Join('|', calls);
        calls.Clear();
        int alphaBlendFailureResult = pipeline.Execute();
        string alphaBlendFailureCalls = string.Join('|', calls);
        calls.Clear();
        int retryResult = pipeline.Execute();
        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual(
            failDeviceState
                ? (0, Direct3D9Factory.GenericFailureHResult, "FirstState:1:1|FirstData:23|SecondState:2:2", Direct3D9Factory.OutOfVideoMemoryHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17", Direct3D9Factory.GenericFailureHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend", 0, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend|ShaderState:23:True|Draw:17|ReleaseColors")
                : (0, Direct3D9Factory.InvalidCallHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23", Direct3D9Factory.OutOfVideoMemoryHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17", Direct3D9Factory.GenericFailureHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend", 0, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend|ShaderState:23:True|Draw:17|ReleaseColors"),
            (cacheResult, colorFailureResult, colorFailureCalls, vertexFormatFailureResult, vertexFormatFailureCalls, alphaBlendFailureResult, alphaBlendFailureCalls, retryResult, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void WhenTwoDimensionalShaderPipelineAlphaBlendRecoversThenShaderStateFailureRestartsAndCleansUp(bool failDeviceState)
    {
        List<string> calls = [];
        bool failNextColorPart = true;
        bool failNextVertexFormat = true;
        bool failNextAlphaBlend = true;
        bool failNextShaderState = true;
        Direct3D9PipelineColorSource firstColorSource = CreateShaderColorSource("First", calls);
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => Direct3D9Factory.SuccessHResult,
            (textureStage, sampler) =>
            {
                calls.Add($"SecondState:{textureStage}:{sampler}");
                if (failDeviceState && failNextColorPart)
                {
                    failNextColorPart = false;
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return 0;
            },
            shader =>
            {
                calls.Add($"SecondData:{shader}");
                if (!failDeviceState && failNextColorPart)
                {
                    failNextColorPart = false;
                    return Direct3D9Factory.InvalidCallHResult;
                }

                return 0;
            });
        Direct3D9PipelineColorSource thirdColorSource = CreateShaderColorSource("Third", calls);
        Direct3D9ShaderPipelineDeviceStateSender sender = new(
            true,
            23,
            [
                new Direct3D9ShaderPipelineItem(1, firstColorSource),
                new Direct3D9ShaderPipelineItem(2, secondColorSource),
                new Direct3D9ShaderPipelineItem(3, thirdColorSource),
            ],
            vertexBuffer =>
            {
                calls.Add($"VertexFormat:{vertexBuffer}");
                if (failNextVertexFormat)
                {
                    failNextVertexFormat = false;
                    return Direct3D9Factory.OutOfVideoMemoryHResult;
                }

                return 0;
            },
            () =>
            {
                calls.Add("AlphaBlend");
                if (failNextAlphaBlend)
                {
                    failNextAlphaBlend = false;
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return 0;
            },
            (shader, pipelineIs2D) =>
            {
                calls.Add($"ShaderState:{shader}:{pipelineIs2D}");
                if (failNextShaderState)
                {
                    failNextShaderState = false;
                    return Direct3D9Factory.InvalidCallHResult;
                }

                return 0;
            });
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendDeviceStates: sender.SendDeviceStates,
            drawPrimitive: vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return 0;
            });
        int cacheResult = pipeline.Execute();
        calls.Clear();

        int colorFailureResult = pipeline.Execute();
        string colorFailureCalls = string.Join('|', calls);
        calls.Clear();
        int vertexFormatFailureResult = pipeline.Execute();
        string vertexFormatFailureCalls = string.Join('|', calls);
        calls.Clear();
        int alphaBlendFailureResult = pipeline.Execute();
        string alphaBlendFailureCalls = string.Join('|', calls);
        calls.Clear();
        int shaderStateFailureResult = pipeline.Execute();
        string shaderStateFailureCalls = string.Join('|', calls);
        calls.Clear();
        int retryResult = pipeline.Execute();
        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual(
            failDeviceState
                ? (0, Direct3D9Factory.GenericFailureHResult, "FirstState:1:1|FirstData:23|SecondState:2:2", Direct3D9Factory.OutOfVideoMemoryHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17", Direct3D9Factory.GenericFailureHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend", Direct3D9Factory.InvalidCallHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend|ShaderState:23:True", 0, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend|ShaderState:23:True|Draw:17|ReleaseColors")
                : (0, Direct3D9Factory.InvalidCallHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23", Direct3D9Factory.OutOfVideoMemoryHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17", Direct3D9Factory.GenericFailureHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend", Direct3D9Factory.InvalidCallHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend|ShaderState:23:True", 0, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend|ShaderState:23:True|Draw:17|ReleaseColors"),
            (cacheResult, colorFailureResult, colorFailureCalls, vertexFormatFailureResult, vertexFormatFailureCalls, alphaBlendFailureResult, alphaBlendFailureCalls, shaderStateFailureResult, shaderStateFailureCalls, retryResult, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void WhenTwoDimensionalShaderPipelineStateRecoversThenDrawFailureRestartsAndCleansUp(bool failDeviceState)
    {
        List<string> calls = [];
        bool failNextColorPart = true;
        bool failNextVertexFormat = true;
        bool failNextAlphaBlend = true;
        bool failNextShaderState = true;
        bool failNextDraw = true;
        Direct3D9PipelineColorSource firstColorSource = CreateShaderColorSource("First", calls);
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => Direct3D9Factory.SuccessHResult,
            (textureStage, sampler) =>
            {
                calls.Add($"SecondState:{textureStage}:{sampler}");
                if (failDeviceState && failNextColorPart)
                {
                    failNextColorPart = false;
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return 0;
            },
            shader =>
            {
                calls.Add($"SecondData:{shader}");
                if (!failDeviceState && failNextColorPart)
                {
                    failNextColorPart = false;
                    return Direct3D9Factory.InvalidCallHResult;
                }

                return 0;
            });
        Direct3D9PipelineColorSource thirdColorSource = CreateShaderColorSource("Third", calls);
        Direct3D9ShaderPipelineDeviceStateSender sender = new(
            true,
            23,
            [
                new Direct3D9ShaderPipelineItem(1, firstColorSource),
                new Direct3D9ShaderPipelineItem(2, secondColorSource),
                new Direct3D9ShaderPipelineItem(3, thirdColorSource),
            ],
            vertexBuffer =>
            {
                calls.Add($"VertexFormat:{vertexBuffer}");
                if (failNextVertexFormat)
                {
                    failNextVertexFormat = false;
                    return Direct3D9Factory.OutOfVideoMemoryHResult;
                }

                return 0;
            },
            () =>
            {
                calls.Add("AlphaBlend");
                if (failNextAlphaBlend)
                {
                    failNextAlphaBlend = false;
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return 0;
            },
            (shader, pipelineIs2D) =>
            {
                calls.Add($"ShaderState:{shader}:{pipelineIs2D}");
                if (failNextShaderState)
                {
                    failNextShaderState = false;
                    return Direct3D9Factory.InvalidCallHResult;
                }

                return 0;
            });
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendDeviceStates: sender.SendDeviceStates,
            drawPrimitive: vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                if (failNextDraw)
                {
                    failNextDraw = false;
                    return Direct3D9Factory.OutOfVideoMemoryHResult;
                }

                return 0;
            });
        Assert.AreEqual(0, pipeline.Execute());
        calls.Clear();

        Assert.AreEqual(failDeviceState ? Direct3D9Factory.GenericFailureHResult : Direct3D9Factory.InvalidCallHResult, pipeline.Execute());
        calls.Clear();
        Assert.AreEqual(Direct3D9Factory.OutOfVideoMemoryHResult, pipeline.Execute());
        calls.Clear();
        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, pipeline.Execute());
        calls.Clear();
        Assert.AreEqual(Direct3D9Factory.InvalidCallHResult, pipeline.Execute());
        calls.Clear();

        int drawFailureResult = pipeline.Execute();
        string drawFailureCalls = string.Join('|', calls);
        calls.Clear();
        int retryResult = pipeline.Execute();
        pipeline.ReleaseExpensiveResources();

        const string SuccessfulStateCalls = "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23|ThirdState:3:3|ThirdData:23|VertexFormat:17|AlphaBlend|ShaderState:23:True|Draw:17";
        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, SuccessfulStateCalls, 0, $"{SuccessfulStateCalls}|ReleaseColors"),
            (drawFailureResult, drawFailureCalls, retryResult, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void WhenCachedVertexBufferExecutionFailsThenFinalCleanupDoesNotReleaseBorrowedBuffer(bool failSendingState)
    {
        List<string> calls = [];
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            sendDeviceStates: vertexBuffer =>
            {
                calls.Add($"State:{vertexBuffer}");
                return failSendingState ? Direct3D9Factory.GenericFailureHResult : 0;
            },
            drawPrimitive: vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return Direct3D9Factory.InvalidCallHResult;
            });
        Assert.AreEqual(0, pipeline.Execute());
        calls.Clear();
        Assert.IsTrue(pipeline.Execute() < 0);

        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual(
            failSendingState
                ? "State:17|ReleaseColors"
                : "State:17|Draw:17|ReleaseColors",
            string.Join('|', calls));
    }

    [TestMethod]
    public void WhenColorSourcesAreRealizedThenOnlyTextureSourcesAreRealizedBeforeState()
    {
        List<string> calls = [];
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            colorSources:
            [
                CreateColorSource(Direct3D9ColorSourceType.Constant, "Constant", calls),
                CreateColorSource(Direct3D9ColorSourceType.Texture, "Texture", calls),
                null,
                CreateColorSource(Direct3D9ColorSourceType.PrecomputedComponent, "Precomputed", calls),
                CreateColorSource(Direct3D9ColorSourceType.Texture | Direct3D9ColorSourceType.Constant, "TextureConstant", calls),
                CreateColorSource(Direct3D9ColorSourceType.Programmatic, "Programmatic", calls),
            ]);

        int result = pipeline.RealizeColorSourcesAndSendState(23);

        Assert.AreEqual((0, "Realize:Texture|Realize:TextureConstant|State:23"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenColorSourceRealizationFailsThenLaterSourcesAndStateAreSkipped()
    {
        List<string> calls = [];
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            colorSources:
            [
                CreateColorSource(Direct3D9ColorSourceType.Texture, "First", calls, Direct3D9Factory.GenericFailureHResult),
                CreateColorSource(Direct3D9ColorSourceType.Texture, "Second", calls),
            ]);

        int result = pipeline.RealizeColorSourcesAndSendState(23);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Realize:First"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenColorSourceTypeIsUnknownThenInternalErrorIsReturnedWithoutState()
    {
        List<string> calls = [];
        Direct3D9Pipeline pipeline = CreatePipeline(
            calls,
            colorSources: [CreateColorSource((Direct3D9ColorSourceType) 16, "Unknown", calls)]);

        int result = pipeline.RealizeColorSourcesAndSendState(23);

        Assert.AreEqual((Direct3D9Factory.InternalErrorHResult, string.Empty), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenExpensiveResourcesAreReleasedRepeatedlyThenOwnedResourcesAreReleasedOnce()
    {
        List<string> calls = [];
        Direct3D9Pipeline pipeline = CreatePipeline(calls);

        pipeline.ReleaseExpensiveResources();
        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual("ReleaseColors|ReleaseBuilder", string.Join('|', calls));
    }

    [TestMethod]
    [DataRow(0, "FirstOwned|SecondOwned|ReleaseColors|ReleaseBuilder")]
    [DataRow(1, "Begin|Geometry|FirstOwned|SecondOwned|ReleaseColors|ReleaseBuilder")]
    [DataRow(2, "Begin|Geometry|Flush|ReleaseBuilder|FirstOwned|SecondOwned|ReleaseColors")]
    public void WhenPipelineResourcesAreReleasedAtEachExecutionStageThenOwnedResourcesAreIndependentFromBorrowedResources(
        int executionStage,
        string expectedCalls)
    {
        List<string> calls = [];
        int sendGeometryResult = executionStage == 1
            ? Direct3D9Factory.GenericFailureHResult
            : Direct3D9Factory.SuccessHResult;
        Direct3D9Pipeline pipeline = new(
            3,
            [],
            _ => Direct3D9Factory.SuccessHResult,
            _ => Direct3D9Factory.SuccessHResult,
            () =>
            {
                calls.Add("Begin");
                return Direct3D9Factory.SuccessHResult;
            },
            () =>
            {
                calls.Add("Geometry");
                return sendGeometryResult;
            },
            () => false,
            (out nint vertexBuffer) =>
            {
                calls.Add("Flush");
                vertexBuffer = 17;
                return Direct3D9Factory.SuccessHResult;
            },
            () => calls.Add("ReleaseColors"),
            () => calls.Add("ReleaseBuilder"),
            ownedColorSources:
            [
                new CallbackDisposable(() => calls.Add("FirstOwned")),
                new CallbackDisposable(() => calls.Add("SecondOwned")),
            ]);

        if (executionStage != 0)
        {
            Assert.AreEqual(sendGeometryResult, pipeline.Execute());
        }

        pipeline.ReleaseExpensiveResources();
        pipeline.ReleaseExpensiveResources();

        Assert.AreEqual((expectedCalls, nint.Zero), (string.Join('|', calls), pipeline.GeometryGenerator));
    }

    [TestMethod]
    public void WhenTwoDimensionalShaderPipelineIsInitializedThenBuilderBoundsGeometryAndShaderFollowNativeOrder()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(true, calls);
        Direct3D9SurfaceRect outsideBounds = new(1, 2, 30, 40);

        int result = initializer.InitializeForRendering(
            MilCompositingMode.SourceOver,
            3,
            5,
            7,
            11,
            outsideBounds,
            false);

        Assert.AreEqual(
            (0, 3, 17, 23, "Setup:True:SourceOver:3:5:7:11|SetupVertexBuilder|Bounds:17:1,2,30,40:False|GetShader"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPrimaryColorSourceOperationsSucceedThenRemainingPipelineOperationsFollowNativeOrder()
    {
        List<string> calls = [];
        int AddCall(string name)
        {
            calls.Add(name);
            return Direct3D9Factory.SuccessHResult;
        }

        Direct3D9PipelineOperationSender sender = new(
            () => AddCall("Effects"),
            () => AddCall("GeometryModifiers"),
            () => AddCall("Lighting"),
            () => AddCall("Clip"));
        Direct3D9PrimaryColorSource primaryColorSource = new(
            builder =>
            {
                calls.Add($"Primary:{ReferenceEquals(builder, sender)}");
                return Direct3D9Factory.SuccessHResult;
            });

        int result = sender.SendPipelineOperations(primaryColorSource);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, "Primary:True|Effects|GeometryModifiers|Lighting|Clip"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(0, Direct3D9Factory.GenericFailureHResult, "Primary")]
    [DataRow(1, Direct3D9Factory.InvalidCallHResult, "Primary|Effects")]
    [DataRow(2, Direct3D9Factory.OutOfVideoMemoryHResult, "Primary|Effects|GeometryModifiers")]
    [DataRow(3, Direct3D9Factory.UnsupportedOperationHResult, "Primary|Effects|GeometryModifiers|Lighting")]
    [DataRow(4, Direct3D9Factory.GenericFailureHResult, "Primary|Effects|GeometryModifiers|Lighting|Clip")]
    public void WhenSendingPipelineOperationFailsThenLaterOperationsAreSkipped(
        int failingOperation,
        int failureResult,
        string expectedCalls)
    {
        List<string> calls = [];
        int operationIndex = 0;
        int SendOperation(string name)
        {
            calls.Add(name);
            return operationIndex++ == failingOperation
                ? failureResult
                : Direct3D9Factory.SuccessHResult;
        }

        Direct3D9PipelineOperationSender sender = new(
            () => SendOperation("Effects"),
            () => SendOperation("GeometryModifiers"),
            () => SendOperation("Lighting"),
            () => SendOperation("Clip"));
        Direct3D9PrimaryColorSource primaryColorSource = new(_ => SendOperation("Primary"));

        int result = sender.SendPipelineOperations(primaryColorSource);

        Assert.AreEqual((failureResult, expectedCalls), (result, string.Join('|', calls)));
    }

    [DataTestMethod]
    [DataRow(0, 1)]
    [DataRow(1, 2)]
    public void WhenSolidShaderPrimaryCanGenerateDiffuseThenBuilderUsesDiffusePiggyback(
        int alphaMultiplyOperationValue,
        int expectedFunctionValue)
    {
        Direct3D9ShaderAlphaMultiplyOperation alphaMultiplyOperation = (Direct3D9ShaderAlphaMultiplyOperation) alphaMultiplyOperationValue;
        Direct3D9ShaderConstantFunction expectedFunction = (Direct3D9ShaderConstantFunction) expectedFunctionValue;
        using Direct3D9ShaderPipelineItemBuilder builder = new(alphaMultiplyOperation: alphaMultiplyOperation);
        Direct3D9ConstantColorSource colorSource = new(new MilColorF(1, 0.25f, 0.5f, 0.75f));

        int result = builder.SetConstant(colorSource);
        Direct3D9ShaderPipelineItem item = builder.DetachItems()[0];

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, uint.MaxValue, Direct3D9VertexFormatAttribute.Diffuse, expectedFunction),
            (result, item.Sampler, item.TextureCoordinates, item.ConstantFunction));
        Direct3D9ShaderPipelineItemBuilder.ReleaseOwnedColorSources([item]);
    }

    [TestMethod]
    public void WhenSolidShaderPrimaryCannotGenerateDiffuseThenBuilderUsesPixelConstantHandle()
    {
        List<string> calls = [];
        Direct3D9ConstantColorSource colorSource = new(
            new MilColorF(0.5f, 1, 1, 1),
            setShaderFloat4: (shader, handle, value) =>
            {
                calls.Add($"Color:{shader}:{handle}:{value}");
                return Direct3D9Factory.SuccessHResult;
            });
        using Direct3D9ShaderPipelineItemBuilder builder = new(
            Direct3D9VertexFormatAttribute.Diffuse,
            getConstantColorHandle: function =>
            {
                calls.Add($"Handle:{function}");
                return 13;
            });

        int result = builder.SetConstant(colorSource);
        Direct3D9ShaderPipelineItem item = builder.DetachItems()[0];
        int shaderDataResult = item.ColorSource!.SendShaderData!(23);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, Direct3D9Factory.SuccessHResult, Direct3D9VertexFormatAttribute.None, Direct3D9ShaderConstantFunction.MultiplyConstant, "Handle:MultiplyConstant|Color:23:13:<0.5, 0.5, 0.5, 0.5>"),
            (result, shaderDataResult, item.TextureCoordinates, item.ConstantFunction, string.Join('|', calls)));
        Direct3D9ShaderPipelineItemBuilder.ReleaseOwnedColorSources([item]);
    }

    [DataTestMethod]
    [DataRow(0, 1)]
    [DataRow(1, 2)]
    public void WhenShaderConstantAlphaCanGenerateDiffuseThenBuilderUsesDiffusePiggyback(
        int alphaMultiplyOperationValue,
        int expectedFunctionValue)
    {
        using Direct3D9ShaderPipelineItemBuilder builder = new(
            alphaMultiplyOperation: (Direct3D9ShaderAlphaMultiplyOperation) alphaMultiplyOperationValue);
        Assert.AreEqual(Direct3D9Factory.SuccessHResult, builder.SetConstant(new Direct3D9ConstantColorSource(new MilColorF(1, 1, 1, 1))));

        int result = builder.MultiplyConstantAlpha(new Direct3D9ConstantAlphaScalableColorSource(0.5f));
        IReadOnlyList<Direct3D9ShaderPipelineItem> items = builder.DetachItems();

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, Direct3D9VertexFormatAttribute.Diffuse, (Direct3D9ShaderConstantFunction) expectedFunctionValue),
            (result, items[1].TextureCoordinates, items[1].ConstantFunction));
        Direct3D9ShaderPipelineItemBuilder.ReleaseOwnedColorSources(items);
    }

    [TestMethod]
    public void WhenShaderConstantAlphaCannotGenerateDiffuseThenPixelConstantIsUploaded()
    {
        List<string> calls = [];
        using Direct3D9ShaderPipelineItemBuilder builder = new(
            Direct3D9VertexFormatAttribute.Diffuse,
            getConstantColorHandle: function =>
            {
                calls.Add($"Handle:{function}");
                return 17;
            });
        Assert.AreEqual(Direct3D9Factory.SuccessHResult, builder.SetConstant(new Direct3D9ConstantColorSource(new MilColorF(1, 1, 1, 1))));
        Direct3D9ConstantAlphaScalableColorSource alpha = new(
            0.25f,
            (shader, handle, value) =>
            {
                calls.Add($"Alpha:{shader}:{handle}:{value}");
                return Direct3D9Factory.SuccessHResult;
            });

        int result = builder.MultiplyConstantAlpha(alpha);
        IReadOnlyList<Direct3D9ShaderPipelineItem> items = builder.DetachItems();
        int shaderDataResult = items[1].ColorSource!.SendShaderData!(23);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, Direct3D9Factory.SuccessHResult, Direct3D9ShaderConstantFunction.MultiplyAlpha, "Handle:MultiplyConstant|Handle:MultiplyAlpha|Alpha:23:17:<1, 1, 1, 0.25>"),
            (result, shaderDataResult, items[1].ConstantFunction, string.Join('|', calls)));
        Direct3D9ShaderPipelineItemBuilder.ReleaseOwnedColorSources(items);
    }

    [TestMethod]
    public void WhenSolidShaderItemsAreReleasedThenOwnedConstantSourceIsDisposed()
    {
        Direct3D9ConstantColorSource colorSource = new(new MilColorF(1, 1, 1, 1));
        using Direct3D9ShaderPipelineItemBuilder builder = new();
        Assert.AreEqual(Direct3D9Factory.SuccessHResult, builder.SetConstant(colorSource));
        Direct3D9ShaderPipelineItem item = builder.DetachItems()[0];

        Direct3D9ShaderPipelineItemBuilder.ReleaseOwnedColorSources([item]);

        Assert.Throws<ObjectDisposedException>(() => item.ColorSource!.Realize());
    }

    [TestMethod]
    public void WhenSolidPrimaryColorSourceSendsOperationsThenConstantReachesFixedFunctionBuilder()
    {                                           
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        using Direct3D9ConstantColorSource colorSource = new(new MilColorF(0.25f, 0.5f, 0.75f, 1));
        Direct3D9ConstantColorSource? receivedColorSource = null;
        Direct3D9PipelineOperationSender sender = new(
            () => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            source =>
            {
                receivedColorSource = source;
                return builder.SetConstant(source);
            });

        int result = sender.SendPipelineOperations(colorSource.PrimaryColorSource);
        Direct3D9FixedFunctionPipelineItem item = builder.DetachItems()[0];

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, true, Direct3D9VertexFormatAttribute.Diffuse, Direct3D9FixedFunctionBlendOperation.SelectSource, Direct3D9FixedFunctionBlendArgument.Diffuse),
            (result, ReferenceEquals(colorSource, receivedColorSource), item.SourceLocation, item.BlendOperation, item.Source1));
    }

    [TestMethod]
    public void WhenSolidPrimarySetConstantFailsThenFailureIsReturnedAndLaterOperationsAreSkipped()
    {
        List<string> calls = [];
        using Direct3D9ConstantColorSource colorSource = new(new MilColorF(1, 1, 1, 1));
        Direct3D9PipelineOperationSender sender = new(
            () =>
            {
                calls.Add("Effects");
                return Direct3D9Factory.SuccessHResult;
            },
            () => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            _ =>
            {
                calls.Add("SetConstant");
                return Direct3D9Factory.OutOfVideoMemoryHResult;
            });

        int result = sender.SendPipelineOperations(colorSource.PrimaryColorSource);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, "SetConstant"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSolidPrimaryHasNoSetConstantTargetThenInvalidCallIsReturned()
    {
        using Direct3D9ConstantColorSource colorSource = new(new MilColorF(1, 1, 1, 1));
        Direct3D9PipelineOperationSender sender = new(
            () => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult);

        int result = sender.SendPipelineOperations(colorSource.PrimaryColorSource);

        Assert.AreEqual(Direct3D9Factory.InvalidCallHResult, result);
    }

    [TestMethod]
    public void WhenPathShaderPipelineIsInitializedThenSharedBrushContextReachesBuilderSetup()
    {
        List<string> calls = [];
        Direct3D9PathBrushContext brushContext = new(
            Matrix4x4.CreateTranslation(3, 4, 0),
            new Direct3D9SurfaceRect(1, 2, 30, 40),
            new MilRectF(1, 2, 30, 40),
            true);
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(
            true,
            calls,
            setupPath: (pipelineIs2D, compositingMode, geometryGenerator, primaryColorSource, effects, effectContext) =>
            {
                calls.Add($"PathSetup:{pipelineIs2D}:{compositingMode}:{geometryGenerator}:{primaryColorSource}:{effects}:{ReferenceEquals(effectContext, brushContext)}:{effectContext.WorldToDevice == brushContext.WorldToDevice}:{effectContext.RenderingBounds}:{effectContext.SamplingBounds}:{effectContext.CanFallback}");
                return 0;
            });

        int result = initializer.InitializeForRendering(
            MilCompositingMode.SourceCopy,
            3,
            5,
            7,
            brushContext,
            null,
            false);

        Assert.AreEqual(
            (0, "PathSetup:True:SourceCopy:3:5:7:True:True:Direct3D9SurfaceRect { Left = 1, Top = 2, Right = 30, Bottom = 40 }:MilRectF { Left = 1, Top = 2, Right = 30, Bottom = 40 }:True|SetupVertexBuilder|GetShader"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenThreeDimensionalShaderPipelineIsInitializedThenVertexBuilderIsSkipped()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(false, calls);

        int result = initializer.InitializeForRendering(
            MilCompositingMode.SourceCopy,
            3,
            5,
            0,
            11,
            null,
            true);

        Assert.AreEqual(
            (0, 3, nint.Zero, 23, "Setup:False:SourceCopy:3:5:0:11|GetShader"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderPipelineSetupFailsThenLaterInitializationIsSkipped()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(
            true,
            calls,
            setupResult: Direct3D9Factory.GenericFailureHResult);

        int result = initializer.InitializeForRendering(
            MilCompositingMode.SourceOver,
            3,
            5,
            0,
            11,
            null,
            true);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, nint.Zero, nint.Zero, nint.Zero, "Setup:True:SourceOver:3:5:0:11"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenVertexBuilderSetupFailsThenBoundsGeometryAndShaderAreSkipped()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(
            true,
            calls,
            setupVertexBuilderResult: Direct3D9Factory.GenericFailureHResult);

        int result = initializer.InitializeForRendering(
            MilCompositingMode.SourceOver,
            3,
            5,
            0,
            11,
            new Direct3D9SurfaceRect(1, 2, 30, 40),
            true);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, nint.Zero, nint.Zero, "Setup:True:SourceOver:3:5:0:11|SetupVertexBuilder"),
            (result, initializer.GeometryGenerator, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderAcquisitionFailsThenGeometryWasRememberedAndFailureIsPreserved()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(
            true,
            calls,
            getShaderResult: Direct3D9Factory.GenericFailureHResult);

        int result = initializer.InitializeForRendering(
            MilCompositingMode.SourceOver,
            3,
            5,
            0,
            11,
            null,
            true);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 3, 17, nint.Zero, "Setup:True:SourceOver:3:5:0:11|SetupVertexBuilder|GetShader"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderAcquisitionFailsAfterReturningShaderThenTemporaryShaderIsReleasedAndMemberIsCleared()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineInitializer initializer = new(
            false,
            static (_, _, _, _, _, _) => Direct3D9Factory.SuccessHResult,
            static (out nint vertexBuilder) =>
            {
                vertexBuilder = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            static (_, _, _) => { },
            (out nint shader) =>
            {
                calls.Add("GetShader");
                shader = 23;
                return Direct3D9Factory.GenericFailureHResult;
            },
            shader => calls.Add($"ReleaseShader:{shader}"));

        int result = initializer.InitializeForRendering(
            MilCompositingMode.SourceOver,
            3,
            5,
            0,
            11,
            null,
            true);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, nint.Zero, "GetShader|ReleaseShader:23"),
            (result, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderPipelineIsReinitializedThenGeometryIsReusedAndShaderIsReplacedInNativeOrder()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(true, calls);
        Assert.AreEqual(0, initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 11, null, true));
        calls.Clear();

        int result = initializer.ReInitialize(
            MilCompositingMode.SourceCopy,
            7,
            13,
            17,
            new Direct3D9SurfaceRect(1, 2, 30, 40),
            false,
            false);

        Assert.AreEqual(
            (0, 3, 17, 23, "Setup:True:SourceCopy:3:7:13:17|Bounds:17:1,2,30,40:False|ReleaseShader:23|GetShader"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenCachedVertexBufferExistsThenReinitializeReusesGeometryWithoutTouchingVertexBuilder()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (vertexBuilder, _, _) =>
            {
                calls.Add($"Mapping:{vertexBuilder}");
                return 0;
            });
        Direct3D9ShaderPipelineInitializer initializer = CreateShaderPipelineInitializer(
            false,
            calls,
            items: [new Direct3D9ShaderPipelineItem(0, colorSource, Direct3D9VertexFormatAttribute.Uv1)],
            finalizeVertexMappings: vertexBuilder =>
            {
                calls.Add($"Finalize:{vertexBuilder}");
                return 0;
            },
            releaseVertexBuilder: vertexBuilder => calls.Add($"ReleaseVertexBuilder:{vertexBuilder}"));
        Assert.AreEqual(0, initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 11, null, true));
        calls.Clear();

        int result = initializer.ReInitialize(MilCompositingMode.SourceCopy, 0, 0, 17, null, true, true);

        Assert.AreEqual(
            (0, 3, nint.Zero, 23, "Setup:False:SourceCopy:3:0:0:17|ReleaseShader:23|GetShader"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenReinitializeSetupFailsThenExistingGeometryVertexBuilderAndShaderArePreserved()
    {
        List<string> calls = [];
        int setupCall = 0;
        Direct3D9ShaderPipelineInitializer initializer = new(
            true,
            (pipelineIs2D, compositingMode, geometryGenerator, primaryColorSource, effects, effectContext) =>
            {
                calls.Add($"Setup:{pipelineIs2D}:{compositingMode}:{geometryGenerator}:{primaryColorSource}:{effects}:{effectContext}");
                return setupCall++ == 0 ? 0 : Direct3D9Factory.GenericFailureHResult;
            },
            (out nint vertexBuilder) =>
            {
                calls.Add("SetupVertexBuilder");
                vertexBuilder = 17;
                return 0;
            },
            (vertexBuilder, bounds, needInside) => calls.Add("Bounds"),
            (out nint shader) =>
            {
                calls.Add("GetShader");
                shader = 23;
                return 0;
            },
            shader => calls.Add($"ReleaseShader:{shader}"),
            releaseVertexBuilder: vertexBuilder => calls.Add($"ReleaseVertexBuilder:{vertexBuilder}"));
        Assert.AreEqual(0, initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 11, null, true));
        calls.Clear();

        int result = initializer.ReInitialize(MilCompositingMode.SourceCopy, 7, 0, 17, null, true, false);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 3, 17, 23, "Setup:True:SourceCopy:3:7:0:17"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(true, false, 17)]
    [DataRow(false, true, 0)]
    public void WhenReinitializeSetupFailsThenExistingBuilderOrCachedBufferAndOwnedSourcesRemainForCleanup(
        bool is2D,
        bool hasCachedVertexBuffer,
        int expectedVertexBuilder)
    {
        List<string> calls = [];
        int setupCall = 0;
        Direct3D9BitmapPipelineColorSource firstOwnership = new(
            31,
            new CallbackDisposable(() => calls.Add("ReleaseFirst")),
            new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0),
            static _ => { });
        Direct3D9BitmapPipelineColorSource secondOwnership = new(
            37,
            new CallbackDisposable(() => calls.Add("ReleaseSecond")),
            new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0),
            static _ => { });
        Direct3D9ShaderPipelineItem firstItem = new(0, firstOwnership, Direct3D9VertexFormatAttribute.None);
        Direct3D9ShaderPipelineItem secondItem = new(1, secondOwnership, Direct3D9VertexFormatAttribute.None);
        firstOwnership.Dispose();
        secondOwnership.Dispose();
        Direct3D9ShaderPipelineInitializer initializer = new(
            is2D,
            (_, compositingMode, geometryGenerator, _, _, _) =>
            {
                calls.Add($"Setup:{compositingMode}:{geometryGenerator}");
                return setupCall++ == 0 ? 0 : Direct3D9Factory.OutOfVideoMemoryHResult;
            },
            (out nint vertexBuilder) =>
            {
                calls.Add("SetupVertexBuilder");
                vertexBuilder = 17;
                return 0;
            },
            (_, _, _) => calls.Add("Bounds"),
            (out nint shader) =>
            {
                calls.Add("GetShader");
                shader = 23;
                return 0;
            },
            shader => calls.Add($"ReleaseShader:{shader}"),
            [firstItem, secondItem],
            finalizeVertexMappings: builder =>
            {
                calls.Add($"Finalize:{builder}");
                return 0;
            },
            releaseVertexBuilder: builder => calls.Add($"ReleaseVertexBuilder:{builder}"));
        Assert.AreEqual(0, initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 11, null, true));
        calls.Clear();

        int result = initializer.ReInitialize(
            MilCompositingMode.SourceCopy,
            7,
            13,
            17,
            new Direct3D9SurfaceRect(1, 2, 30, 40),
            false,
            hasCachedVertexBuffer);
        firstItem.Ownership!.Dispose();
        secondItem.Ownership!.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, 3, (nint)expectedVertexBuilder, 23, "Setup:SourceCopy:3|ReleaseFirst|ReleaseSecond"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenReinitializeShaderAcquisitionFailsThenGeometryAndVertexBuilderArePreservedAfterOldShaderRelease()
    {
        List<string> calls = [];
        int shaderCall = 0;
        Direct3D9ShaderPipelineInitializer initializer = new(
            true,
            (pipelineIs2D, compositingMode, geometryGenerator, primaryColorSource, effects, effectContext) =>
            {
                calls.Add("Setup");
                return 0;
            },
            (out nint vertexBuilder) =>
            {
                calls.Add("SetupVertexBuilder");
                vertexBuilder = 17;
                return 0;
            },
            (vertexBuilder, bounds, needInside) => calls.Add("Bounds"),
            (out nint shader) =>
            {
                calls.Add("GetShader");
                shader = shaderCall++ == 0 ? 23 : 0;
                return shaderCall == 1 ? 0 : Direct3D9Factory.GenericFailureHResult;
            },
            shader => calls.Add($"ReleaseShader:{shader}"),
            releaseVertexBuilder: vertexBuilder => calls.Add($"ReleaseVertexBuilder:{vertexBuilder}"));
        Assert.AreEqual(0, initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 11, null, true));
        calls.Clear();

        int result = initializer.ReInitialize(MilCompositingMode.SourceCopy, 7, 0, 17, null, true, false);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 3, 17, nint.Zero, "Setup|ReleaseShader:23|GetShader"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenReinitializeShaderAcquisitionFailsAfterReturningShaderThenOldAndTemporaryShadersAreReleasedInOrder()
    {
        List<string> calls = [];
        int shaderCall = 0;
        Direct3D9ShaderPipelineInitializer initializer = new(
            false,
            (_, _, _, _, _, _) =>
            {
                calls.Add("Setup");
                return Direct3D9Factory.SuccessHResult;
            },
            static (out nint vertexBuilder) =>
            {
                vertexBuilder = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            static (_, _, _) => { },
            (out nint shader) =>
            {
                calls.Add("GetShader");
                shader = shaderCall++ == 0 ? 23 : 29;
                return shaderCall == 1
                    ? Direct3D9Factory.SuccessHResult
                    : Direct3D9Factory.GenericFailureHResult;
            },
            shader => calls.Add($"ReleaseShader:{shader}"));
        Assert.AreEqual(0, initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 11, null, true));
        calls.Clear();

        int result = initializer.ReInitialize(MilCompositingMode.SourceCopy, 7, 0, 17, null, true, true);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, nint.Zero, "Setup|ReleaseShader:23|GetShader|ReleaseShader:29"),
            (result, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenReinitializeShaderAcquisitionFailsAfterMappingsThenOwnedStateRemainsForCleanup()
    {
        List<string> calls = [];
        int shaderCall = 0;
        Direct3D9PipelineColorSource firstColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"FirstMapping:{builder}:{location}");
                return 0;
            });
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"SecondMapping:{builder}:{location}");
                return 0;
            });
        Direct3D9BitmapPipelineColorSource firstOwnership = new(
            31,
            new CallbackDisposable(() => calls.Add("ReleaseFirst")),
            firstColorSource,
            static _ => { });
        Direct3D9BitmapPipelineColorSource secondOwnership = new(
            37,
            new CallbackDisposable(() => calls.Add("ReleaseSecond")),
            secondColorSource,
            static _ => { });
        Direct3D9ShaderPipelineItem firstItem = new(0, firstOwnership, Direct3D9VertexFormatAttribute.Uv1);
        Direct3D9ShaderPipelineItem secondItem = new(1, secondOwnership, Direct3D9VertexFormatAttribute.Uv2);
        firstOwnership.Dispose();
        secondOwnership.Dispose();
        Direct3D9ShaderPipelineInitializer initializer = new(
            false,
            (_, compositingMode, geometryGenerator, _, _, _) =>
            {
                calls.Add($"Setup:{compositingMode}:{geometryGenerator}");
                return 0;
            },
            (out nint vertexBuilder) =>
            {
                calls.Add("SetupVertexBuilder");
                vertexBuilder = 17;
                return 0;
            },
            (vertexBuilder, bounds, needInside) =>
                calls.Add($"Bounds:{vertexBuilder}:{bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}:{needInside}"),
            (out nint shader) =>
            {
                calls.Add("GetShader");
                shader = shaderCall++ == 0 ? 23 : 29;
                return shaderCall == 1 ? 0 : Direct3D9Factory.GenericFailureHResult;
            },
            shader => calls.Add($"ReleaseShader:{shader}"),
            [firstItem, secondItem],
            finalizeVertexMappings: builder =>
            {
                calls.Add($"Finalize:{builder}");
                return 0;
            });
        Assert.AreEqual(0, initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 11, null, true));
        calls.Clear();

        int result = initializer.ReInitialize(
            MilCompositingMode.SourceCopy,
            7,
            0,
            17,
            new Direct3D9SurfaceRect(1, 2, 30, 40),
            false,
            false);
        firstItem.Ownership!.Dispose();
        secondItem.Ownership!.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 3, 17, nint.Zero, "Setup:SourceCopy:3|SetupVertexBuilder|FirstMapping:17:Uv1|SecondMapping:17:Uv2|Finalize:17|Bounds:17:1,2,30,40:False|ReleaseShader:23|GetShader|ReleaseShader:29|ReleaseFirst|ReleaseSecond"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(1, Direct3D9Factory.OutOfVideoMemoryHResult, "Setup:SourceCopy:3|SetupVertexBuilder|FirstMapping:17:Uv1|SecondMapping:17:Uv2|ReleaseVertexBuilder:17|ReleaseFirst|ReleaseSecond")]
    [DataRow(2, Direct3D9Factory.InvalidCallHResult, "Setup:SourceCopy:3|SetupVertexBuilder|FirstMapping:17:Uv1|SecondMapping:17:Uv2|Finalize:17|ReleaseVertexBuilder:17|ReleaseFirst|ReleaseSecond")]
    public void WhenReinitializeVertexMappingOrFinalizationFailsThenExistingShaderAndGeometryRemainForRetry(
        int failureStep,
        int failureResult,
        string expectedCalls)
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource firstColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"FirstMapping:{builder}:{location}");
                return 0;
            });
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (builder, location, _) =>
            {
                calls.Add($"SecondMapping:{builder}:{location}");
                return failureStep == 1 ? failureResult : 0;
            });
        Direct3D9BitmapPipelineColorSource firstOwnership = new(
            31,
            new CallbackDisposable(() => calls.Add("ReleaseFirst")),
            firstColorSource,
            static _ => { });
        Direct3D9BitmapPipelineColorSource secondOwnership = new(
            37,
            new CallbackDisposable(() => calls.Add("ReleaseSecond")),
            secondColorSource,
            static _ => { });
        Direct3D9ShaderPipelineItem firstItem = new(0, firstOwnership, Direct3D9VertexFormatAttribute.Uv1);
        Direct3D9ShaderPipelineItem secondItem = new(1, secondOwnership, Direct3D9VertexFormatAttribute.Uv2);
        firstOwnership.Dispose();
        secondOwnership.Dispose();
        Direct3D9ShaderPipelineInitializer initializer = new(
            false,
            (_, compositingMode, geometryGenerator, _, _, _) =>
            {
                calls.Add($"Setup:{compositingMode}:{geometryGenerator}");
                return 0;
            },
            (out nint vertexBuilder) =>
            {
                calls.Add("SetupVertexBuilder");
                vertexBuilder = 17;
                return 0;
            },
            (_, _, _) => calls.Add("Bounds"),
            (out nint shader) =>
            {
                calls.Add("GetShader");
                shader = 23;
                return 0;
            },
            shader => calls.Add($"ReleaseShader:{shader}"),
            [firstItem, secondItem],
            finalizeVertexMappings: builder =>
            {
                calls.Add($"Finalize:{builder}");
                return failureStep == 2 ? failureResult : 0;
            },
            releaseVertexBuilder: builder => calls.Add($"ReleaseVertexBuilder:{builder}"));
        Assert.AreEqual(0, initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 11, null, true));
        calls.Clear();

        int result = initializer.ReInitialize(
            MilCompositingMode.SourceCopy,
            7,
            0,
            17,
            new Direct3D9SurfaceRect(1, 2, 30, 40),
            false,
            false);
        firstItem.Ownership!.Dispose();
        secondItem.Ownership!.Dispose();

        Assert.AreEqual(
            (failureResult, 3, nint.Zero, 23, expectedCalls),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenReinitializeVertexBuilderSetupFailsAfterReturningBuilderThenExistingStateRemainsForRetry()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource firstColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (_, _, _) =>
            {
                calls.Add("FirstMapping");
                return 0;
            });
        Direct3D9PipelineColorSource secondColorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendVertexMapping: (_, _, _) =>
            {
                calls.Add("SecondMapping");
                return 0;
            });
        Direct3D9BitmapPipelineColorSource firstOwnership = new(
            31,
            new CallbackDisposable(() => calls.Add("ReleaseFirst")),
            firstColorSource,
            static _ => { });
        Direct3D9BitmapPipelineColorSource secondOwnership = new(
            37,
            new CallbackDisposable(() => calls.Add("ReleaseSecond")),
            secondColorSource,
            static _ => { });
        Direct3D9ShaderPipelineItem firstItem = new(0, firstOwnership, Direct3D9VertexFormatAttribute.Uv1);
        Direct3D9ShaderPipelineItem secondItem = new(1, secondOwnership, Direct3D9VertexFormatAttribute.Uv2);
        firstOwnership.Dispose();
        secondOwnership.Dispose();
        Direct3D9ShaderPipelineInitializer initializer = new(
            false,
            (_, compositingMode, geometryGenerator, _, _, _) =>
            {
                calls.Add($"Setup:{compositingMode}:{geometryGenerator}");
                return 0;
            },
            (out nint vertexBuilder) =>
            {
                calls.Add("SetupVertexBuilder");
                vertexBuilder = 17;
                return Direct3D9Factory.OutOfVideoMemoryHResult;
            },
            (_, _, _) => calls.Add("Bounds"),
            (out nint shader) =>
            {
                calls.Add("GetShader");
                shader = 23;
                return 0;
            },
            shader => calls.Add($"ReleaseShader:{shader}"),
            [firstItem, secondItem],
            finalizeVertexMappings: builder =>
            {
                calls.Add($"Finalize:{builder}");
                return 0;
            },
            releaseVertexBuilder: builder => calls.Add($"ReleaseVertexBuilder:{builder}"));
        Assert.AreEqual(0, initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 11, null, true));
        calls.Clear();

        int result = initializer.ReInitialize(
            MilCompositingMode.SourceCopy,
            7,
            0,
            17,
            new Direct3D9SurfaceRect(1, 2, 30, 40),
            false,
            false);
        firstItem.Ownership!.Dispose();
        secondItem.Ownership!.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, 3, nint.Zero, 23, "Setup:SourceCopy:3|SetupVertexBuilder|ReleaseVertexBuilder:17|ReleaseFirst|ReleaseSecond"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenReinitializingWithCachedVertexBufferThenVertexBuilderSetupIsSkipped()
    {
        List<string> calls = [];
        int shaderCall = 0;
        Direct3D9BitmapPipelineColorSource firstOwnership = new(
            31,
            new CallbackDisposable(() => calls.Add("ReleaseFirst")),
            new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0),
            static _ => { });
        Direct3D9BitmapPipelineColorSource secondOwnership = new(
            37,
            new CallbackDisposable(() => calls.Add("ReleaseSecond")),
            new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0),
            static _ => { });
        Direct3D9ShaderPipelineItem firstItem = new(0, firstOwnership, Direct3D9VertexFormatAttribute.Uv1);
        Direct3D9ShaderPipelineItem secondItem = new(1, secondOwnership, Direct3D9VertexFormatAttribute.Uv2);
        firstOwnership.Dispose();
        secondOwnership.Dispose();
        Direct3D9ShaderPipelineInitializer initializer = new(
            false,
            (_, compositingMode, geometryGenerator, _, _, _) =>
            {
                calls.Add($"Setup:{compositingMode}:{geometryGenerator}");
                return 0;
            },
            (out nint vertexBuilder) =>
            {
                calls.Add("SetupVertexBuilder");
                vertexBuilder = 17;
                return Direct3D9Factory.OutOfVideoMemoryHResult;
            },
            (_, _, _) => calls.Add("Bounds"),
            (out nint shader) =>
            {
                calls.Add("GetShader");
                shader = shaderCall++ == 0 ? 23 : 29;
                return 0;
            },
            shader => calls.Add($"ReleaseShader:{shader}"),
            [firstItem, secondItem],
            finalizeVertexMappings: builder =>
            {
                calls.Add($"Finalize:{builder}");
                return 0;
            },
            releaseVertexBuilder: builder => calls.Add($"ReleaseVertexBuilder:{builder}"));
        Assert.AreEqual(0, initializer.InitializeForRendering(MilCompositingMode.SourceOver, 3, 5, 0, 11, null, true));
        calls.Clear();

        int result = initializer.ReInitialize(
            MilCompositingMode.SourceCopy,
            7,
            0,
            17,
            null,
            false,
            true);
        firstItem.Ownership!.Dispose();
        secondItem.Ownership!.Dispose();

        Assert.AreEqual(
            (0, 3, nint.Zero, 29, "Setup:SourceCopy:3|ReleaseShader:23|GetShader|ReleaseFirst|ReleaseSecond"),
            (result, initializer.GeometryGenerator, initializer.VertexBuilder, initializer.Shader, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenTwoDimensionalShaderDeviceStatesAreSentThenNativeOrderAndSamplerArePreserved()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineDeviceStateSender sender = CreateShaderDeviceStateSender(true, calls);

        int result = sender.SendDeviceStates(29);

        Assert.AreEqual(
            (0, "ColorState:5:5|ShaderData:23|VertexFormat:29|AlphaBlend|ShaderState:23:True"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenThreeDimensionalShaderDeviceStatesAreSentThenVertexFormatIsSkipped()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineDeviceStateSender sender = CreateShaderDeviceStateSender(false, calls);

        int result = sender.SendDeviceStates(0);

        Assert.AreEqual(
            (0, "ColorState:5:5|ShaderData:23|AlphaBlend|ShaderState:23:False"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenColorSourceDeviceStateFailsThenLaterShaderStateIsSkipped()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineDeviceStateSender sender = CreateShaderDeviceStateSender(
            true,
            calls,
            colorStateResult: Direct3D9Factory.GenericFailureHResult);

        int result = sender.SendDeviceStates(29);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "ColorState:5:5"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenColorSourceShaderDataFailsThenVertexAndPipelineStateAreSkipped()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineDeviceStateSender sender = CreateShaderDeviceStateSender(
            true,
            calls,
            shaderDataResult: Direct3D9Factory.GenericFailureHResult);

        int result = sender.SendDeviceStates(29);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "ColorState:5:5|ShaderData:23"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void WhenSecondColorSourceStatePartFailsThenLaterItemsAndPipelineStateAreSkipped(bool failDeviceState)
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource firstColorSource = CreateShaderColorSource("First", calls);
        Direct3D9PipelineColorSource secondColorSource = CreateShaderColorSource(
            "Second",
            calls,
            failDeviceState ? Direct3D9Factory.GenericFailureHResult : 0,
            failDeviceState ? 0 : Direct3D9Factory.InvalidCallHResult);
        Direct3D9PipelineColorSource thirdColorSource = CreateShaderColorSource("Third", calls);
        Direct3D9ShaderPipelineDeviceStateSender sender = new(
            true,
            23,
            [
                new Direct3D9ShaderPipelineItem(1, firstColorSource),
                new Direct3D9ShaderPipelineItem(2, secondColorSource),
                new Direct3D9ShaderPipelineItem(3, thirdColorSource),
            ],
            vertexBuffer =>
            {
                calls.Add($"VertexFormat:{vertexBuffer}");
                return 0;
            },
            () =>
            {
                calls.Add("AlphaBlend");
                return 0;
            },
            (shader, pipelineIs2D) =>
            {
                calls.Add($"ShaderState:{shader}:{pipelineIs2D}");
                return 0;
            });

        int result = sender.SendDeviceStates(29);

        Assert.AreEqual(
            failDeviceState
                ? (Direct3D9Factory.GenericFailureHResult, "FirstState:1:1|FirstData:23|SecondState:2:2")
                : (Direct3D9Factory.InvalidCallHResult, "FirstState:1:1|FirstData:23|SecondState:2:2|SecondData:23"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenVertexFormatFailsThenBlendAndShaderStateAreSkipped()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineDeviceStateSender sender = CreateShaderDeviceStateSender(
            true,
            calls,
            vertexFormatResult: Direct3D9Factory.GenericFailureHResult);

        int result = sender.SendDeviceStates(29);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "ColorState:5:5|ShaderData:23|VertexFormat:29"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenAlphaBlendStateFailsThenShaderStateIsSkipped()
    {
        List<string> calls = [];
        Direct3D9ShaderPipelineDeviceStateSender sender = CreateShaderDeviceStateSender(
            true,
            calls,
            alphaBlendResult: Direct3D9Factory.GenericFailureHResult);

        int result = sender.SendDeviceStates(29);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "ColorState:5:5|ShaderData:23|VertexFormat:29|AlphaBlend"),
            (result, string.Join('|', calls)));
    }

    private static Direct3D9PipelineColorSource CreateShaderColorSource(
        string name,
        List<string> calls,
        int deviceStateResult = Direct3D9Factory.SuccessHResult,
        int shaderDataResult = Direct3D9Factory.SuccessHResult)
    {
        return new Direct3D9PipelineColorSource(
            Direct3D9ColorSourceType.Texture,
            () => Direct3D9Factory.SuccessHResult,
            (textureStage, sampler) =>
            {
                calls.Add($"{name}State:{textureStage}:{sampler}");
                return deviceStateResult;
            },
            shader =>
            {
                calls.Add($"{name}Data:{shader}");
                return shaderDataResult;
            });
    }

    private static Direct3D9ShaderPipelineDeviceStateSender CreateShaderDeviceStateSender(
        bool is2D,
        List<string> calls,
        int colorStateResult = Direct3D9Factory.SuccessHResult,
        int shaderDataResult = Direct3D9Factory.SuccessHResult,
        int vertexFormatResult = Direct3D9Factory.SuccessHResult,
        int alphaBlendResult = Direct3D9Factory.SuccessHResult)
    {
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => Direct3D9Factory.SuccessHResult,
            (textureStage, sampler) =>
            {
                calls.Add($"ColorState:{textureStage}:{sampler}");
                return colorStateResult;
            },
            shader =>
            {
                calls.Add($"ShaderData:{shader}");
                return shaderDataResult;
            });

        return new Direct3D9ShaderPipelineDeviceStateSender(
            is2D,
            23,
            [new Direct3D9ShaderPipelineItem(5, colorSource), new Direct3D9ShaderPipelineItem(7, (Direct3D9PipelineColorSource?) null)],
            vertexBuffer =>
            {
                calls.Add($"VertexFormat:{vertexBuffer}");
                return vertexFormatResult;
            },
            () =>
            {
                calls.Add("AlphaBlend");
                return alphaBlendResult;
            },
            (shader, pipelineIs2D) =>
            {
                calls.Add($"ShaderState:{shader}:{pipelineIs2D}");
                return Direct3D9Factory.SuccessHResult;
            });
    }

    private static Direct3D9FixedFunctionPipelineDeviceStateSender CreateFixedFunctionDeviceStateSender(
        List<string> calls,
        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items,
        uint firstUnusedStage = 1)
    {
        return CreateFixedFunctionDeviceStateSender(
            calls,
            new Direct3D9FinalizedFixedFunctionPipeline(items, firstUnusedStage));
    }

    private static Direct3D9FixedFunctionPipelineDeviceStateSender CreateFixedFunctionDeviceStateSender(
        List<string> calls,
        Direct3D9FinalizedFixedFunctionPipeline pipeline)
    {
        return new Direct3D9FixedFunctionPipelineDeviceStateSender(
            pipeline,
            (stage, operation) =>
            {
                calls.Add($"Stage:{stage}:{(uint) operation.ColorOperation}:{operation.ColorArgument1}:{operation.ColorArgument2}:{(uint) operation.AlphaOperation}:{operation.AlphaArgument1}:{operation.AlphaArgument2}");
                return 0;
            },
            stage =>
            {
                calls.Add($"Disable:{stage}");
                return 0;
            },
            vertexBuffer =>
            {
                calls.Add($"VertexFormat:{vertexBuffer}");
                return 0;
            },
            () =>
            {
                calls.Add("AlphaBlend");
                return 0;
            },
            () =>
            {
                calls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                calls.Add("VertexShader:null");
                return 0;
            });
    }

    private static Direct3D9GeometryRenderer<uint> CreateRenderer(
        Direct3D9CreateLightingColorSource createLightingColorSource)
    {
        Vector3[] positions = [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)];
        Vector2[] textureCoordinates = [new(0, 0), new(0, 0), new(0, 0)];
        return new(
            positions,
            [],
            textureCoordinates,
            [],
            uint.MaxValue,
            createLightingColorSource);
    }

    private static Direct3D9FixedFunctionPassInputs CreateConstantPassInputs() =>
        new(builder => builder.SetConstant(new Direct3D9ConstantColorSource(new MilColorF(1, 1, 1, 1))));

    private static int InitializeGeometryPipeline(
        ref Direct3D9GeometryRenderer<uint> renderer,
        Direct3D9FixedFunctionPassInputs passInputs,
        List<string> calls,
        out Direct3D9Pipeline? pipeline) =>
        Direct3D9FixedFunctionGeometryPipeline.Initialize(
            ref renderer,
            passInputs,
            items => CreateFixedFunctionPipelineInitializer(calls, items),
            null,
            true,
            _ => 0,
            () => 0,
            () => false,
            (out nint vertexBuffer) =>
            {
                vertexBuffer = 23;
                return 0;
            },
            () => calls.Add("ReleaseColors"),
            out pipeline);

    private static Direct3D9FixedFunctionPipelineInitializer CreateFixedFunctionPipelineInitializer(
        List<string> calls,
        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items,
        int finalizeResult = Direct3D9Factory.SuccessHResult,
        Func<bool>? verticesArePreGenerated = null,
        Direct3D9SetPipelineConstantMapping? setConstantMapping = null,
        int setupVertexBuilderResult = Direct3D9Factory.SuccessHResult)
    {
        return new Direct3D9FixedFunctionPipelineInitializer(
            items,
            (out nint vertexBuilder) =>
            {
                calls.Add("SetupVertexBuilder");
                vertexBuilder = 17;
                return setupVertexBuilderResult;
            },
            builder =>
            {
                calls.Add($"Finalize:{builder}");
                return finalizeResult;
            },
            builder => calls.Add($"ReleaseBuilder:{builder}"),
            (builder, bounds, needInside) =>
                calls.Add($"Bounds:{builder}:{bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}:{needInside}"),
            (stage, operation) =>
            {
                calls.Add($"Stage:{stage}:{(uint) operation.ColorOperation}:{operation.ColorArgument1}:{operation.ColorArgument2}:{(uint) operation.AlphaOperation}:{operation.AlphaArgument1}:{operation.AlphaArgument2}");
                return 0;
            },
            stage =>
            {
                calls.Add($"Disable:{stage}");
                return 0;
            },
            vertexBuffer =>
            {
                calls.Add($"VertexFormat:{vertexBuffer}");
                return 0;
            },
            () =>
            {
                calls.Add("AlphaBlend");
                return 0;
            },
            () =>
            {
                calls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                calls.Add("VertexShader:null");
                return 0;
            },
            verticesArePreGenerated: verticesArePreGenerated,
            setConstantMapping: setConstantMapping);
    }

    private static int InitializeFixedFunctionPipeline(
        Direct3D9FixedFunctionPipelineInitializer initializer,
        List<string> calls,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside,
        out Direct3D9Pipeline? pipeline)
    {
        return initializer.InitializeForRendering(
            3,
            outsideBounds,
            needInside,
            vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return 0;
            },
            () =>
            {
                calls.Add("Begin");
                return 0;
            },
            () =>
            {
                calls.Add("Geometry");
                return 0;
            },
            () => false,
            (out nint vertexBuffer) =>
            {
                calls.Add("Flush");
                vertexBuffer = 23;
                return 0;
            },
            () => calls.Add("ReleaseColors"),
            out pipeline);
    }

    private static Direct3D9ShaderPipelineInitializer CreateShaderPipelineInitializer(
        bool is2D,
        List<string> calls,
        int setupResult = Direct3D9Factory.SuccessHResult,
        int setupVertexBuilderResult = Direct3D9Factory.SuccessHResult,
        int getShaderResult = Direct3D9Factory.SuccessHResult,
        IReadOnlyList<Direct3D9ShaderPipelineItem>? items = null,
        Direct3D9FinalizePipelineVertexMappings? finalizeVertexMappings = null,
        Direct3D9ReleasePipelineVertexBuilder? releaseVertexBuilder = null,
        Func<bool>? verticesArePreGenerated = null,
        Direct3D9SetupPathShaderPipeline? setupPath = null)
    {
        return new Direct3D9ShaderPipelineInitializer(
            is2D,
            (pipelineIs2D, compositingMode, geometryGenerator, primaryColorSource, effects, effectContext) =>
            {
                calls.Add($"Setup:{pipelineIs2D}:{compositingMode}:{geometryGenerator}:{primaryColorSource}:{effects}:{effectContext}");
                return setupResult;
            },
            (out nint vertexBuilder) =>
            {
                calls.Add("SetupVertexBuilder");
                vertexBuilder = setupVertexBuilderResult < 0 ? 0 : 17;
                return setupVertexBuilderResult;
            },
            (vertexBuilder, bounds, needInside) =>
                calls.Add($"Bounds:{vertexBuilder}:{bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}:{needInside}"),
            (out nint shader) =>
            {
                calls.Add("GetShader");
                shader = getShaderResult < 0 ? 0 : 23;
                return getShaderResult;
            },
            shader => calls.Add($"ReleaseShader:{shader}"),
            items,
            verticesArePreGenerated: verticesArePreGenerated,
            finalizeVertexMappings: finalizeVertexMappings,
            releaseVertexBuilder: releaseVertexBuilder,
            setupPath: setupPath);
    }

    private static Direct3D9PipelineColorSource CreateColorSource(
        Direct3D9ColorSourceType sourceType,
        string name,
        List<string> calls,
        int result = Direct3D9Factory.SuccessHResult)
    {
        return new Direct3D9PipelineColorSource(
            sourceType,
            () =>
            {
                calls.Add($"Realize:{name}");
                return result;
            });
    }

    private sealed class CallbackDisposable(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }

    private static Direct3D9Pipeline CreatePipeline(
        List<string> calls,
        IReadOnlyList<Direct3D9PipelineColorSource?>? colorSources = null,
        Func<nint, int>? sendDeviceStates = null,
        Func<nint, int>? drawPrimitive = null,
        Func<int>? sendGeometry = null,
        Func<bool>? hasOutsideBounds = null,
        Direct3D9FlushPipelineVertexBuilder? flushTryGetVertexBuffer = null)
    {
        return new Direct3D9Pipeline(
            3,
            colorSources ?? [],
            sendDeviceStates ?? (vertexBuffer =>
            {
                calls.Add($"State:{vertexBuffer}");
                return 0;
            }),
            drawPrimitive ?? (vertexBuffer =>
            {
                calls.Add($"Draw:{vertexBuffer}");
                return 0;
            }),
            () =>
            {
                calls.Add("Begin");
                return 0;
            },
            sendGeometry ?? (() =>
            {
                calls.Add("Geometry");
                return 0;
            }),
            hasOutsideBounds ?? (() =>
            {
                calls.Add("Outside:False");
                return false;
            }),
            flushTryGetVertexBuffer ?? ((out nint vertexBuffer) =>
            {
                calls.Add("Flush");
                vertexBuffer = 17;
                return 0;
            }),
            () => calls.Add("ReleaseColors"),
            () => calls.Add("ReleaseBuilder"));
    }
}
