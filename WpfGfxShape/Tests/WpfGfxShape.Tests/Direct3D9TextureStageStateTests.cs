using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9TextureStageStateTests
{
    [TestMethod]
    public void WhenDisablingValidTextureStageThenColorOperationIsDisabled()
    {
        (uint Stage, Texturestagestatetype State, uint Value)? call = null;

        int result = Direct3D9TextureStageState.DisableTextureStage(
            2,
            4,
            (stage, state, value) =>
            {
                call = (stage, state, value);
                return 0;
            });

        Assert.AreEqual(0, result);
        Assert.AreEqual((2u, Texturestagestatetype.Colorop, (uint) Textureop.Disable), call);
    }

    [TestMethod]
    public void WhenDisablingMaximumTextureStageThenNativeCallIsSkipped()
    {
        int callCount = 0;

        int result = Direct3D9TextureStageState.DisableTextureStage(
            4,
            4,
            (_, _, _) =>
            {
                callCount++;
                return 0;
            });

        Assert.AreEqual(0, result);
        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public void WhenDisablingTextureStageFailsThenFailureIsReturned()
    {
        int result = Direct3D9TextureStageState.DisableTextureStage(
            2,
            4,
            (_, _, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenDisablingTextureStageBeyondMaximumThenThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Direct3D9TextureStageState.DisableTextureStage(5, 4, (_, _, _) => 0));
    }

    [TestMethod]
    public unsafe void WhenDeviceDisablesTextureStageThenCapabilityLimitAndNativeCallAreUsed()
    {
        (uint Stage, Texturestagestatetype State, uint Value)? call = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: new Caps9 { MaxTextureBlendStages = 4 },
            setTextureStageState: (stage, state, value) =>
            {
                call = (stage, state, value);
                return 0;
            });

        int result = device.DisableTextureStage(3);

        Assert.AreEqual(0, result);
        Assert.AreEqual((3u, Texturestagestatetype.Colorop, (uint) Textureop.Disable), call);
    }

    [TestMethod]
    public unsafe void WhenDeviceDisablesTextureStageTwiceAfterNonZeroSuccessThenMatchingCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: new Caps9 { MaxTextureBlendStages = 4 },
            setTextureStageState: (_, _, _) =>
            {
                callCount++;
                return 1;
            });

        int firstResult = device.DisableTextureStage(2);
        int secondResult = device.DisableTextureStage(2);

        Assert.AreEqual((1, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenDeviceDisableTextureStageFailsThenOriginalFailureIsReturnedAndMatchingCallIsRetried()
    {
        int callCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: new Caps9 { MaxTextureBlendStages = 4 },
            setTextureStageState: (_, _, _) =>
            {
                callCount++;
                return callCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        int failedResult = device.DisableTextureStage(2);
        int retryResult = device.DisableTextureStage(2);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 2),
            (failedResult, retryResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenDisablingTextureStageAfterDeviceDisposeThenThrowsObjectDisposedException()
    {
        Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: new Caps9 { MaxTextureBlendStages = 4 },
            setTextureStageState: (_, _, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.DisableTextureStage(0));
    }

    [TestMethod]
    public void WhenInitializingColorOperationsThenNativeTextureStagesAreDisabledInOrder()
    {
        List<(uint Stage, Texturestagestatetype State, uint Value)> calls = [];

        int result = Direct3D9TextureStageState.InitializeColorOperations(
            8,
            (stage, state, value) =>
            {
                calls.Add((stage, state, value));
                return 0;
            });

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(
            Enumerable.Range(0, 8)
                .Select(stage => ((uint) stage, Texturestagestatetype.Colorop, (uint) Textureop.Disable))
                .ToArray(),
            calls);
    }

    [TestMethod]
    public void WhenMaximumBlendStagesExceedsNativeTableThenStageAfterTableBoundaryRemainsSkipped()
    {
        List<uint> stages = [];

        int result = Direct3D9TextureStageState.InitializeColorOperations(
            11,
            (stage, _, _) =>
            {
                stages.Add(stage);
                return 0;
            });

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 9, 10 }, stages);
    }

    [TestMethod]
    public void WhenSettingColorOperationFailsThenFirstFailureIsReturnedAndLaterStagesAreSkipped()
    {
        List<uint> stages = [];

        int result = Direct3D9TextureStageState.InitializeColorOperations(
            8,
            (stage, _, _) =>
            {
                stages.Add(stage);
                return stage == 3 ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(new uint[] { 0, 1, 2, 3 }, stages);
    }

    [TestMethod]
    public void WhenInitializingTextureCoordinateIndicesThenEachStageUsesItsOwnIndex()
    {
        List<(uint Stage, Texturestagestatetype State, uint Value)> calls = [];

        int result = Direct3D9TextureStageState.InitializeTextureCoordinateIndices(
            8,
            (stage, state, value) =>
            {
                calls.Add((stage, state, value));
                return 0;
            });

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(
            Enumerable.Range(0, 8)
                .Select(stage => ((uint) stage, Texturestagestatetype.Texcoordindex, (uint) stage))
                .ToArray(),
            calls);
    }

    [TestMethod]
    public void WhenSettingTextureCoordinateIndexFailsThenFirstFailureIsReturnedAndLaterStagesAreSkipped()
    {
        List<uint> stages = [];

        int result = Direct3D9TextureStageState.InitializeTextureCoordinateIndices(
            8,
            (stage, _, _) =>
            {
                stages.Add(stage);
                return stage == 3 ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(new uint[] { 0, 1, 2, 3 }, stages);
    }

    [TestMethod]
    public void WhenInitializingTextureTransformFlagsThenEachStageIsDisabledInOrder()
    {
        List<(uint Stage, Texturestagestatetype State, uint Value)> calls = [];

        int result = Direct3D9TextureStageState.InitializeTextureTransformFlags(
            8,
            (stage, state, value) =>
            {
                calls.Add((stage, state, value));
                return 0;
            });

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(
            Enumerable.Range(0, 8)
                .Select(stage => ((uint) stage, Texturestagestatetype.Texturetransformflags, (uint) Texturetransformflags.Disable))
                .ToArray(),
            calls);
    }

    [TestMethod]
    public void WhenSettingTextureTransformFlagsFailsThenFirstFailureIsReturnedAndLaterStagesAreSkipped()
    {
        List<uint> stages = [];

        int result = Direct3D9TextureStageState.InitializeTextureTransformFlags(
            8,
            (stage, _, _) =>
            {
                stages.Add(stage);
                return stage == 3 ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(new uint[] { 0, 1, 2, 3 }, stages);
    }
}
