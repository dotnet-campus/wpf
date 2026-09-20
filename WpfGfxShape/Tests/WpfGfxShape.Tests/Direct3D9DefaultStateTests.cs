using System.Numerics;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9DefaultStateTests
{
    [TestMethod]
    public void WhenInitializingDefaultStateThenNativeStateGroupsAreInitializedInOrder()
    {
        List<string> calls = [];
        Caps9 capabilities = default;
        capabilities.MaxTextureBlendStages = 8;
        capabilities.MaxStreams = 3;

        int result = Direct3D9DefaultState.Initialize(
            capabilities,
            (_, _) =>
            {
                calls.Add("render");
                return 0;
            },
            (_, state, _) =>
            {
                calls.Add(state switch
                {
                    Texturestagestatetype.Colorop => "color-operation",
                    Texturestagestatetype.Texcoordindex => "texture-coordinate-index",
                    Texturestagestatetype.Texturetransformflags => "texture-transform-flags",
                    _ => "texture-stage"
                });
                return 0;
            },
            (_, _, _) =>
            {
                calls.Add("sampler");
                return 0;
            },
            (_, matrix) =>
            {
                Assert.AreEqual(Matrix4x4.Identity, matrix);
                calls.Add("transform");
                return 0;
            },
            _ =>
            {
                calls.Add("material");
                return 0;
            },
            _ =>
            {
                calls.Add("texture");
                return 0;
            },
            () =>
            {
                calls.Add("pixel-shader");
                return 0;
            },
            () =>
            {
                calls.Add("stream-0");
                return 0;
            },
            stream =>
            {
                calls.Add($"stream-{stream}");
                return 0;
            },
            () =>
            {
                calls.Add("indices");
                return 0;
            },
            () => calls.Add("scissor-clip-cache"));

        Assert.AreEqual(0, result);
        Assert.AreEqual("render", calls[0]);
        Assert.IsTrue(calls.IndexOf("color-operation") < calls.IndexOf("sampler"));
        Assert.IsTrue(calls.IndexOf("sampler") < calls.IndexOf("texture-coordinate-index"));
        Assert.IsTrue(calls.IndexOf("texture-coordinate-index") < calls.IndexOf("texture-transform-flags"));
        Assert.IsTrue(calls.IndexOf("texture-transform-flags") < calls.IndexOf("transform"));
        Assert.IsTrue(calls.IndexOf("transform") < calls.IndexOf("material"));
        Assert.IsTrue(calls.IndexOf("material") < calls.IndexOf("texture"));
        Assert.IsTrue(calls.IndexOf("texture") < calls.IndexOf("pixel-shader"));
        CollectionAssert.AreEqual(
            new[] { "pixel-shader", "stream-0", "stream-1", "stream-2", "indices", "scissor-clip-cache" },
            calls[^6..]);
    }

    [TestMethod]
    public void WhenRenderStateInitializationFailsThenLaterStateGroupsAreSkipped()
    {
        int laterStateCallCount = 0;

        int result = Direct3D9DefaultState.Initialize(
            default,
            (_, _) => Direct3D9Factory.GenericFailureHResult,
            (_, _, _) => CountCall(),
            (_, _, _) => CountCall(),
            (_, _) => CountCall(),
            _ => CountCall(),
            _ => CountCall(),
            CountCall,
            CountCall,
            _ => CountCall(),
            CountCall);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual(0, laterStateCallCount);
        return;

        int CountCall()
        {
            laterStateCallCount++;
            return 0;
        }
    }

    [TestMethod]
    public void WhenTextureStageInitializationFailsThenSamplerStateIsSkipped()
    {
        int samplerCallCount = 0;
        Caps9 capabilities = default;
        capabilities.MaxTextureBlendStages = 8;

        int result = Direct3D9DefaultState.Initialize(
            capabilities,
            (_, _) => 0,
            (stage, _, _) => stage == 2 ? Direct3D9Factory.GenericFailureHResult : 0,
            (_, _, _) =>
            {
                samplerCallCount++;
                return 0;
            },
            (_, _) => 0,
            _ => 0,
            _ => 0,
            () => 0,
            () => 0,
            _ => 0,
            () => 0);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual(0, samplerCallCount);
    }

    [TestMethod]
    public void WhenTextureTransformFlagsFailThenTransformsAreSkipped()
    {
        int transformCallCount = 0;
        Caps9 capabilities = default;
        capabilities.MaxTextureBlendStages = 2;

        int result = Direct3D9DefaultState.Initialize(
            capabilities,
            (_, _) => 0,
            (_, state, _) => state == Texturestagestatetype.Texturetransformflags
                ? Direct3D9Factory.GenericFailureHResult
                : 0,
            (_, _, _) => 0,
            (_, _) =>
            {
                transformCallCount++;
                return 0;
            },
            _ => 0,
            _ => 0,
            () => 0,
            () => 0,
            _ => 0,
            () => 0);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual(0, transformCallCount);
    }

    [TestMethod]
    public void WhenTransformInitializationFailsThenMaterialIsSkipped()
    {
        int materialCallCount = 0;

        int result = Direct3D9DefaultState.Initialize(
            default,
            (_, _) => 0,
            (_, _, _) => 0,
            (_, _, _) => 0,
            (_, _) => Direct3D9Factory.GenericFailureHResult,
            _ =>
            {
                materialCallCount++;
                return 0;
            },
            _ => 0,
            () => 0,
            () => 0,
            _ => 0,
            () => 0);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual(0, materialCallCount);
    }

    [TestMethod]
    public void WhenMaterialInitializationFailsThenTexturesAreNotCleared()
    {
        int clearTextureCallCount = 0;

        int result = Direct3D9DefaultState.Initialize(
            default,
            (_, _) => 0,
            (_, _, _) => 0,
            (_, _, _) => 0,
            (_, _) => 0,
            _ => Direct3D9Factory.GenericFailureHResult,
            _ =>
            {
                clearTextureCallCount++;
                return 0;
            },
            () => 0,
            () => 0,
            _ => 0,
            () => 0);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual(0, clearTextureCallCount);
    }

    [TestMethod]
    public void WhenTextureClearFailsThenPixelShaderIsNotCleared()
    {
        int clearPixelShaderCallCount = 0;
        Caps9 capabilities = default;
        capabilities.MaxTextureBlendStages = 1;

        int result = Direct3D9DefaultState.Initialize(
            capabilities,
            (_, _) => 0,
            (_, _, _) => 0,
            (_, _, _) => 0,
            (_, _) => 0,
            _ => 0,
            _ => Direct3D9Factory.GenericFailureHResult,
            () =>
            {
                clearPixelShaderCallCount++;
                return 0;
            },
            () => 0,
            _ => 0,
            () => 0);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual(0, clearPixelShaderCallCount);
    }

    [TestMethod]
    public void WhenPixelShaderClearFailsThenStreamsAreNotCleared()
    {
        int streamCallCount = 0;

        int result = Direct3D9DefaultState.Initialize(
            default,
            (_, _) => 0,
            (_, _, _) => 0,
            (_, _, _) => 0,
            (_, _) => 0,
            _ => 0,
            _ => 0,
            () => Direct3D9Factory.GenericFailureHResult,
            () =>
            {
                streamCallCount++;
                return 0;
            },
            _ =>
            {
                streamCallCount++;
                return 0;
            },
            () => 0);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual(0, streamCallCount);
    }

    [TestMethod]
    public void WhenMaximumStreamsIsOneThenOnlyPrimaryStreamIsCleared()
    {
        int primaryStreamCallCount = 0;
        int additionalStreamCallCount = 0;
        Caps9 capabilities = default;
        capabilities.MaxStreams = 1;

        int result = InitializeThroughStreams(
            capabilities,
            () =>
            {
                primaryStreamCallCount++;
                return 0;
            },
            _ =>
            {
                additionalStreamCallCount++;
                return 0;
            },
            () => 0);

        Assert.AreEqual(0, result);
        Assert.AreEqual(1, primaryStreamCallCount);
        Assert.AreEqual(0, additionalStreamCallCount);
    }

    [TestMethod]
    [DataRow(0u, new uint[0])]
    [DataRow(1u, new uint[0])]
    [DataRow(4u, new uint[] { 1, 2, 3 })]
    [DataRow(uint.MaxValue, new uint[] { 1, 2, 3 })]
    public void WhenMaximumStreamsIsConsumedThenValueIsNotNormalized(
        uint maximumStreams,
        uint[] expectedAdditionalStreams)
    {
        List<uint> additionalStreams = [];
        Caps9 capabilities = default;
        capabilities.MaxStreams = maximumStreams;

        int result = InitializeThroughStreams(
            capabilities,
            () => 0,
            stream =>
            {
                additionalStreams.Add(stream);
                return stream == 3 ? Direct3D9Factory.GenericFailureHResult : 0;
            },
            () => 0);

        Assert.AreEqual(
            maximumStreams > 3 ? Direct3D9Factory.GenericFailureHResult : 0,
            result);
        CollectionAssert.AreEqual(expectedAdditionalStreams, additionalStreams);
    }

    [TestMethod]
    [DataRow(0u, 0u)]
    [DataRow(3u, 3u)]
    [DataRow(8u, 8u)]
    [DataRow(9u, 8u)]
    [DataRow(uint.MaxValue, 8u)]
    public void WhenTextureBlendStageCountIsConsumedThenOnlyTheCallerAppliesTheFixedLimit(
        uint maximumTextureBlendStages,
        uint expectedConsumedStages)
    {
        List<uint> samplerStages = [];
        List<uint> clearedTextureStages = [];
        Caps9 capabilities = default;
        capabilities.MaxTextureBlendStages = maximumTextureBlendStages;

        int result = Direct3D9DefaultState.Initialize(
            capabilities,
            (_, _) => 0,
            (_, _, _) => 0,
            (stage, _, _) =>
            {
                samplerStages.Add(stage);
                return 0;
            },
            (_, _) => 0,
            _ => 0,
            stage =>
            {
                clearedTextureStages.Add(stage);
                return 0;
            },
            () => 0,
            () => 0,
            _ => 0,
            () => 0);

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(
            Enumerable.Range(0, checked((int) expectedConsumedStages)).Select(stage => (uint) stage).ToArray(),
            samplerStages);
        CollectionAssert.AreEqual(samplerStages, clearedTextureStages);
    }

    [TestMethod]
    public void WhenPrimaryStreamClearFailsThenLaterStreamStateAndCacheResetAreSkipped()
    {
        int additionalStreamCallCount = 0;
        int indicesCallCount = 0;
        int resetCallCount = 0;
        Caps9 capabilities = default;
        capabilities.MaxStreams = 4;

        int result = Direct3D9DefaultState.Initialize(
            capabilities,
            (_, _) => 0,
            (_, _, _) => 0,
            (_, _, _) => 0,
            (_, _) => 0,
            _ => 0,
            _ => 0,
            () => 0,
            () => Direct3D9Factory.GenericFailureHResult,
            _ =>
            {
                additionalStreamCallCount++;
                return 0;
            },
            () =>
            {
                indicesCallCount++;
                return 0;
            },
            () => resetCallCount++);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0, 0),
            (result, additionalStreamCallCount, indicesCallCount, resetCallCount));
    }

    [TestMethod]
    public void WhenAdditionalStreamClearFailsThenLaterStreamStateAndCacheResetAreSkipped()
    {
        List<uint> clearedStreams = [];
        int indicesCallCount = 0;
        int resetCallCount = 0;
        Caps9 capabilities = default;
        capabilities.MaxStreams = 4;

        int result = Direct3D9DefaultState.Initialize(
            capabilities,
            (_, _) => 0,
            (_, _, _) => 0,
            (_, _, _) => 0,
            (_, _) => 0,
            _ => 0,
            _ => 0,
            () => 0,
            () => 0,
            stream =>
            {
                clearedStreams.Add(stream);
                return stream == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
            },
            () =>
            {
                indicesCallCount++;
                return 0;
            },
            () => resetCallCount++);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0),
            (result, indicesCallCount, resetCallCount));
        CollectionAssert.AreEqual(new uint[] { 1, 2 }, clearedStreams);
    }

    [TestMethod]
    public void WhenIndicesClearFailsThenItsHResultIsReturned()
    {
        Caps9 capabilities = default;
        capabilities.MaxStreams = 1;

        int result = InitializeThroughStreams(
            capabilities,
            () => 0,
            _ => 0,
            () => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenIndicesClearFailsThenScissorAndClipCacheIsNotReset()
    {
        int resetCallCount = 0;
        Caps9 capabilities = default;
        capabilities.MaxStreams = 1;

        int result = Direct3D9DefaultState.Initialize(
            capabilities,
            (_, _) => 0,
            (_, _, _) => 0,
            (_, _, _) => 0,
            (_, _) => 0,
            _ => 0,
            _ => 0,
            () => 0,
            () => 0,
            _ => 0,
            () => Direct3D9Factory.GenericFailureHResult,
            () => resetCallCount++);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual(0, resetCallCount);
    }

    [TestMethod]
    public unsafe void WhenDefaultStateInitializationIsRetriedThenSuccessfulAndFailedStatesAreForcedAgain()
    {
        int zEnableCallCount = 0;
        int zWriteCallCount = 0;
        int textureStageCallCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setRenderState: (state, _) =>
            {
                if (state == Renderstatetype.Zenable)
                {
                    zEnableCallCount++;
                }

                if (state == Renderstatetype.Zwriteenable)
                {
                    zWriteCallCount++;
                    return zWriteCallCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
                }

                return 0;
            },
            setTextureStageState: (_, _, _) =>
            {
                textureStageCallCount++;
                return 0;
            },
            setSamplerState: (_, _, _) => 0);

        int firstResult = Initialize();
        int knownResult = device.GetRenderState(Renderstatetype.Zenable, out uint knownValue);
        int unknownResult = device.GetRenderState(Renderstatetype.Zwriteenable, out uint unknownValue);
        int secondResult = Initialize();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0u, Direct3D9Factory.GenericFailureHResult, 0u, 0, 2, 2, 8),
            (firstResult, knownResult, knownValue, unknownResult, unknownValue, secondResult,
                zEnableCallCount, zWriteCallCount, textureStageCallCount));
        return;

        int Initialize()
        {
            return Direct3D9DefaultState.Initialize(
                default,
                device.ForceSetRenderState,
                device.ForceSetTextureStageState,
                device.ForceSetSamplerState,
                (_, _) => 0,
                _ => 0,
                _ => 0,
                () => 0,
                () => 0,
                _ => 0,
                () => 0);
        }
    }

    [TestMethod]
    public unsafe void WhenDefaultStateInitializationIsRetriedAfterMaterialFailureThenTransformsAreForcedAgain()
    {
        List<Transformstatetype> transforms = [];
        int materialCallCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setTransform: (state, _) =>
            {
                transforms.Add(state);
                return 0;
            });

        int firstResult = Initialize();
        int worldResult = device.GetTransform((Transformstatetype) 256, out Matrix4x4 world);
        int viewResult = device.GetTransform(Transformstatetype.View, out Matrix4x4 view);
        int projectionResult = device.GetTransform(Transformstatetype.Projection, out Matrix4x4 projection);
        int secondResult = Initialize();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, Matrix4x4.Identity, 0, Matrix4x4.Identity,
                0, Matrix4x4.Identity, 0, 2),
            (firstResult, worldResult, world, viewResult, view, projectionResult, projection,
                secondResult, materialCallCount));
        CollectionAssert.AreEqual(
            new[]
            {
                (Transformstatetype) 256,
                Transformstatetype.View,
                Transformstatetype.Projection,
                (Transformstatetype) 256,
                Transformstatetype.View,
                Transformstatetype.Projection
            },
            transforms);
        return;

        int Initialize()
        {
            return Direct3D9DefaultState.Initialize(
                default,
                (_, _) => 0,
                (_, _, _) => 0,
                (_, _, _) => 0,
                device.ForceSetTransform,
                _ => ++materialCallCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0,
                _ => 0,
                () => 0,
                () => 0,
                _ => 0,
                () => 0);
        }
    }

    [TestMethod]
    public void WhenIdentityTransformReturnsNonzeroSuccessThenDefaultMaterialStillRuns()
    {
        List<string> calls = [];

        int result = Direct3D9DefaultState.Initialize(
            default,
            (_, _) => 0,
            (_, _, _) => 0,
            (_, _, _) => 0,
            (state, _) =>
            {
                calls.Add(state switch
                {
                    (Transformstatetype) 256 => "world",
                    Transformstatetype.View => "view",
                    Transformstatetype.Projection => "projection",
                    _ => "other"
                });
                return 1;
            },
            _ =>
            {
                calls.Add("material");
                return 2;
            },
            _ => 0,
            () => 0,
            () => 0,
            _ => 0,
            () => 0);

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(new[] { "world", "view", "projection", "material" }, calls);
    }

    [TestMethod]
    public void WhenDefaultStateTailReturnsNonzeroSuccessThenEveryLaterGroupStillRunsInOrder()
    {
        List<string> calls = [];
        Caps9 capabilities = default;
        capabilities.MaxTextureBlendStages = 2;
        capabilities.MaxStreams = 3;

        int result = Direct3D9DefaultState.Initialize(
            capabilities,
            (_, _) => 0,
            (_, _, _) => 0,
            (_, _, _) => 0,
            (_, _) => 0,
            _ => 0,
            stage =>
            {
                calls.Add($"texture-{stage}");
                return 1;
            },
            () =>
            {
                calls.Add("pixel-shader");
                return 2;
            },
            () =>
            {
                calls.Add("stream-0");
                return 3;
            },
            stream =>
            {
                calls.Add($"stream-{stream}");
                return 4;
            },
            () =>
            {
                calls.Add("indices");
                return 5;
            },
            () => calls.Add("scissor-clip-cache"));

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(
            new[]
            {
                "texture-0",
                "texture-1",
                "pixel-shader",
                "stream-0",
                "stream-1",
                "stream-2",
                "indices",
                "scissor-clip-cache"
            },
            calls);
    }

    [TestMethod]
    public unsafe void WhenDefaultStateTailIsRetriedThenCachedTailStatesAreForcedAgain()
    {
        List<string> calls = [];
        Caps9 capabilities = default;
        capabilities.MaxTextureBlendStages = 2;
        capabilities.MaxStreams = 1;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: capabilities,
            setTexture: (stage, _) =>
            {
                calls.Add($"texture-{stage}");
                return 0;
            },
            setPixelShader: _ =>
            {
                calls.Add("pixel-shader");
                return 0;
            },
            setStreamSource: (stream, _, _, _) =>
            {
                calls.Add($"stream-{stream}");
                return 0;
            },
            setIndices: _ =>
            {
                calls.Add("indices");
                return 0;
            });

        int firstResult = Initialize();
        int cachedTextureResult = device.SetD3DTexture(0, null);
        int cachedPixelShaderResult = device.SetPixelShader(null);
        int cachedStreamResult = device.SetStreamSource(0, null, 0, 0);
        int secondResult = Initialize();

        Assert.AreEqual((0, 0, 0, 0, 0),
            (firstResult, cachedTextureResult, cachedPixelShaderResult, cachedStreamResult, secondResult));
        CollectionAssert.AreEqual(
            new[]
            {
                "texture-0", "texture-1", "pixel-shader", "stream-0", "indices",
                "texture-0", "texture-1", "pixel-shader", "stream-0", "indices"
            },
            calls);
        return;

        int Initialize()
        {
            return Direct3D9DefaultState.Initialize(
                capabilities,
                (_, _) => 0,
                (_, _, _) => 0,
                (_, _, _) => 0,
                (_, _) => 0,
                _ => 0,
                stage => device.ForceSetTexture(stage, null),
                () => device.ForceSetPixelShader(null),
                () => device.ForceSetStreamSource(null, 0),
                stream => device.SetStreamSource(stream, null, 0, 0),
                () => device.ForceSetIndices(null));
        }
    }

    private static int InitializeThroughStreams(
        Caps9 capabilities,
        Func<int> clearPrimaryStreamSource,
        Func<uint, int> clearAdditionalStreamSource,
        Func<int> clearIndices)
    {
        return Direct3D9DefaultState.Initialize(
            capabilities,
            (_, _) => 0,
            (_, _, _) => 0,
            (_, _, _) => 0,
            (_, _) => 0,
            _ => 0,
            _ => 0,
            () => 0,
            clearPrimaryStreamSource,
            clearAdditionalStreamSource,
            clearIndices);
    }
}
