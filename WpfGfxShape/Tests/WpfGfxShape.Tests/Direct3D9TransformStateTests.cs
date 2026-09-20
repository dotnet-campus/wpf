using System.Numerics;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class Direct3D9TransformStateTests
{
    private const Transformstatetype World = (Transformstatetype) 256;

    [TestMethod]
    public void WhenInitializingTransformsThenWorldViewAndProjectionAreIdentityInOrder()
    {
        List<(Transformstatetype State, Matrix4x4 Matrix)> calls = [];

        int result = Direct3D9TransformState.InitializeIdentityTransforms(
            (state, matrix) =>
            {
                calls.Add((state, matrix));
                return 0;
            });

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(
            new[]
            {
                (World, Matrix4x4.Identity),
                (Transformstatetype.View, Matrix4x4.Identity),
                (Transformstatetype.Projection, Matrix4x4.Identity)
            },
            calls);
    }

    [TestMethod]
    public void WhenSettingTransformFailsThenFirstFailureIsReturnedAndLaterTransformsAreSkipped()
    {
        List<Transformstatetype> states = [];

        int result = Direct3D9TransformState.InitializeIdentityTransforms(
            (state, _) =>
            {
                states.Add(state);
                return state == Transformstatetype.View
                    ? Direct3D9Factory.GenericFailureHResult
                    : 0;
            });

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(
            new[] { World, Transformstatetype.View },
            states);
    }

    [TestMethod]
    public void When2DTransformsAreAlreadyAppliedThenNativeTransformsAreSkipped()
    {
        int callCount = 0;
        bool transformsApplied = true;

        int result = Direct3D9TransformState.Set2DTransformsForFixedFunction(
            Matrix4x4.CreateScale(2f),
            ref transformsApplied,
            (_, _) =>
            {
                callCount++;
                return 0;
            });

        Assert.AreEqual((0, 0, true), (result, callCount, transformsApplied));
    }

    [TestMethod]
    public void When2DTransformsAreAppliedThenWorldViewAndProjectionAreSetInOrder()
    {
        List<(Transformstatetype State, Matrix4x4 Matrix)> calls = [];
        Matrix4x4 projection = Matrix4x4.CreateScale(2f, -3f, 1f);
        bool transformsApplied = false;

        int result = Direct3D9TransformState.Set2DTransformsForFixedFunction(
            projection,
            ref transformsApplied,
            (state, matrix) =>
            {
                calls.Add((state, matrix));
                return 0;
            });

        Assert.AreEqual((0, true), (result, transformsApplied));
        CollectionAssert.AreEqual(
            new[]
            {
                (World, Matrix4x4.Identity),
                (Transformstatetype.View, Matrix4x4.Identity),
                (Transformstatetype.Projection, projection)
            },
            calls);
    }

    [TestMethod]
    public void When2DTransformApplicationFailsThenNextCallRetriesFromWorld()
    {
        List<Transformstatetype> calls = [];
        bool transformsApplied = false;
        bool failView = true;
        int SetTransform(Transformstatetype state, Matrix4x4 _)
        {
            calls.Add(state);
            if (state == Transformstatetype.View && failView)
            {
                failView = false;
                return Direct3D9Factory.GenericFailureHResult;
            }

            return 0;
        }

        int firstResult = Direct3D9TransformState.Set2DTransformsForFixedFunction(
            Matrix4x4.Identity,
            ref transformsApplied,
            SetTransform);
        int secondResult = Direct3D9TransformState.Set2DTransformsForFixedFunction(
            Matrix4x4.Identity,
            ref transformsApplied,
            SetTransform);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, true),
            (firstResult, secondResult, transformsApplied));
        CollectionAssert.AreEqual(
            new[]
            {
                World,
                Transformstatetype.View,
                World,
                Transformstatetype.View,
                Transformstatetype.Projection
            },
            calls);
    }

    [TestMethod]
    public void WhenSetting2DVertexShaderTransformThenProjectionIsTransposedIntoFourRegisters()
    {
        Matrix4x4 projection = new(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16);
        uint actualRegister = uint.MaxValue;
        Matrix4x4 actualMatrix = default;
        bool transformApplied = false;
        uint appliedStartRegister = uint.MaxValue;

        int result = Direct3D9TransformState.Set2DTransformForVertexShader(
            projection,
            7,
            ref transformApplied,
            ref appliedStartRegister,
            (startRegister, matrix) =>
            {
                actualRegister = startRegister;
                actualMatrix = matrix;
                return 0;
            });

        Assert.AreEqual((0, 7u, Matrix4x4.Transpose(projection), true, 7u),
            (result, actualRegister, actualMatrix, transformApplied, appliedStartRegister));
    }

    [TestMethod]
    public void When2DVertexShaderTransformIsAlreadyAppliedAtRegisterThenNativeCallIsSkipped()
    {
        int callCount = 0;
        bool transformApplied = true;
        uint appliedStartRegister = 3;

        int result = Direct3D9TransformState.Set2DTransformForVertexShader(
            Matrix4x4.Identity,
            3,
            ref transformApplied,
            ref appliedStartRegister,
            (_, _) =>
            {
                callCount++;
                return 0;
            });

        Assert.AreEqual((0, 0, true, 3u),
            (result, callCount, transformApplied, appliedStartRegister));
    }

    [TestMethod]
    public void When2DVertexShaderRegisterChangesThenTransformIsAppliedAgain()
    {
        List<uint> registers = [];
        bool transformApplied = true;
        uint appliedStartRegister = 3;

        int result = Direct3D9TransformState.Set2DTransformForVertexShader(
            Matrix4x4.Identity,
            8,
            ref transformApplied,
            ref appliedStartRegister,
            (startRegister, _) =>
            {
                registers.Add(startRegister);
                return 0;
            });

        Assert.AreEqual((0, true, 8u), (result, transformApplied, appliedStartRegister));
        CollectionAssert.AreEqual(new uint[] { 8 }, registers);
    }

    [TestMethod]
    public void When2DVertexShaderTransformApplicationFailsThenStateIsNotCachedAndNextCallRetries()
    {
        int callCount = 0;
        bool transformApplied = false;
        uint appliedStartRegister = uint.MaxValue;
        int SetConstants(uint _, Matrix4x4 __)
        {
            callCount++;
            return callCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
        }

        int firstResult = Direct3D9TransformState.Set2DTransformForVertexShader(
            Matrix4x4.Identity,
            2,
            ref transformApplied,
            ref appliedStartRegister,
            SetConstants);
        int secondResult = Direct3D9TransformState.Set2DTransformForVertexShader(
            Matrix4x4.Identity,
            2,
            ref transformApplied,
            ref appliedStartRegister,
            SetConstants);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 2, true, 2u),
            (firstResult, secondResult, callCount, transformApplied, appliedStartRegister));
    }

    [TestMethod]
    public void When2DTransformsAreRedefinedThenVertexShaderTransformIsAppliedAgain()
    {
        List<Matrix4x4> matrices = [];
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setVertexShaderConstants: (_, matrix) =>
            {
                matrices.Add(matrix);
                return 0;
            },
            setTransform: (_, _) => 0);
        Matrix4x4 firstProjection = Matrix4x4.CreateScale(2f);
        Matrix4x4 secondProjection = Matrix4x4.CreateTranslation(3f, 4f, 0f);

        device.Define2DTransforms(firstProjection);
        device.Set2DTransformForFixedFunction();
        int firstResult = device.Set2DTransformForVertexShader(0);
        int skippedResult = device.Set2DTransformForVertexShader(0);
        device.Define2DTransforms(secondProjection);
        device.Set2DTransformForFixedFunction();
        int secondResult = device.Set2DTransformForVertexShader(0);

        Assert.AreEqual((0, 0, 0), (firstResult, skippedResult, secondResult));
        CollectionAssert.AreEqual(
            new[] { Matrix4x4.Transpose(firstProjection), Matrix4x4.Transpose(secondProjection) },
            matrices);
    }

    [TestMethod]
    public void WhenGettingUnknownTransformThenGenericFailureIsReturned()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true));

        int result = device.GetTransform(Transformstatetype.Projection, out Matrix4x4 matrix);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, default(Matrix4x4)), (result, matrix));
    }

    [TestMethod]
    public void When2DVertexShaderTransformIsSetThenCurrentProjectionCacheIsTransposedIntoFourRegisters()
    {
        List<(uint StartRegister, Matrix4x4 Matrix)> writes = [];
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setVertexShaderConstants: (startRegister, matrix) =>
            {
                writes.Add((startRegister, matrix));
                return 0;
            },
            setTransform: (_, _) => 0);
        Matrix4x4 projection = new(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16);
        device.Define2DTransforms(projection);
        device.Set2DTransformForFixedFunction();

        int result = device.Set2DTransformForVertexShader(7);

        Assert.AreEqual((0, 7u, Matrix4x4.Transpose(projection)),
            (result, writes[0].StartRegister, writes[0].Matrix));
    }

    [TestMethod]
    public void WhenProjectionTransformChangesThen2DVertexShaderUsesChangedProjection()
    {
        List<Matrix4x4> writes = [];
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setVertexShaderConstants: (_, matrix) =>
            {
                writes.Add(matrix);
                return 0;
            },
            setTransform: (_, _) => 0);
        Matrix4x4 definedProjection = Matrix4x4.CreateScale(2f);
        Matrix4x4 changedProjection = Matrix4x4.CreateTranslation(3f, 4f, 0f);
        device.Define2DTransforms(definedProjection);
        device.Set2DTransformForFixedFunction();
        device.Set2DTransformForVertexShader(0);

        int changeResult = device.SetTransform(Transformstatetype.Projection, changedProjection);
        int shaderResult = device.Set2DTransformForVertexShader(0);

        Assert.AreEqual((0, 0, Matrix4x4.Transpose(changedProjection)),
            (changeResult, shaderResult, writes[1]));
    }

    [TestMethod]
    public void When2DVertexShaderRegistersAreOverwrittenThenTransformIsAppliedAgain()
    {
        List<Matrix4x4> writes = [];
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setVertexShaderConstants: (_, matrix) =>
            {
                writes.Add(matrix);
                return 0;
            },
            setTransform: (_, _) => 0);
        Matrix4x4 projection = Matrix4x4.CreateScale(2f);
        device.Define2DTransforms(projection);
        device.Set2DTransformForFixedFunction();
        device.Set2DTransformForVertexShader(4);
        Matrix4x4 shaderProjection = Matrix4x4.Transpose(projection);
        ReadOnlySpan<Vector4> projectionRegisters = new(&shaderProjection, 4);

        int overwriteResult = device.SetVertexShaderConstants(5, projectionRegisters.Slice(1, 1));
        int shaderResult = device.Set2DTransformForVertexShader(4);

        Assert.AreEqual((0, 0, 2, Matrix4x4.Transpose(projection)),
            (overwriteResult, shaderResult, writes.Count, writes[1]));
    }

    [TestMethod]
    public void WhenSetting2DVertexShaderTransformAfterDisposeThenThrowsObjectDisposedException()
    {
        Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setVertexShaderConstants: (_, _) => 0,
            setTransform: (_, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.Set2DTransformForVertexShader(0));
    }

    [TestMethod]
    [DataRow(256)]
    [DataRow((int) Transformstatetype.View)]
    public void WhenTransformIsSetTwiceThenMatchingNativeCallIsSkipped(int stateValue)
    {
        int callCount = 0;
        using Direct3D9Device device = CreateTransformDevice((_, _) =>
        {
            callCount++;
            return 0;
        });
        Transformstatetype state = (Transformstatetype) stateValue;
        Matrix4x4 matrix = Matrix4x4.CreateTranslation(2f, 3f, 4f);

        int firstResult = device.SetTransform(state, matrix);
        int secondResult = device.SetTransform(state, matrix);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public void WhenTransformSetFailsThenMatchingValueIsRetriedAndSuccessIsCached()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateTransformDevice((_, _) =>
        {
            callCount++;
            return callCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
        });
        Matrix4x4 matrix = Matrix4x4.CreateScale(2f);

        int failedResult = device.SetTransform(Transformstatetype.Projection, matrix);
        int retryResult = device.SetTransform(Transformstatetype.Projection, matrix);
        int cachedResult = device.SetTransform(Transformstatetype.Projection, matrix);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0, 2),
            (failedResult, retryResult, cachedResult, callCount));
    }

    [TestMethod]
    public void WhenRegularTransformChangesThenFixedFunction2DTransformsAreForcedAgain()
    {
        List<Transformstatetype> calls = [];
        using Direct3D9Device device = CreateTransformDevice((state, _) =>
        {
            calls.Add(state);
            return 0;
        });
        device.Define2DTransforms(Matrix4x4.CreateScale(2f));

        int firstResult = device.Set2DTransformForFixedFunction();
        int skippedResult = device.Set2DTransformForFixedFunction();
        int changeResult = device.SetTransform(World, Matrix4x4.CreateTranslation(1f, 0f, 0f));
        int restoredResult = device.Set2DTransformForFixedFunction();

        Assert.AreEqual((0, 0, 0, 0), (firstResult, skippedResult, changeResult, restoredResult));
        CollectionAssert.AreEqual(
            new[]
            {
                World,
                Transformstatetype.View,
                Transformstatetype.Projection,
                World,
                World,
                Transformstatetype.View,
                Transformstatetype.Projection
            },
            calls);
    }

    [TestMethod]
    public void When2DTransformsAreRedefinedThenFixedFunctionUsesTheNewProjection()
    {
        List<(Transformstatetype State, Matrix4x4 Matrix)> calls = [];
        using Direct3D9Device device = CreateTransformDevice((state, matrix) =>
        {
            calls.Add((state, matrix));
            return 0;
        });
        Matrix4x4 firstProjection = Matrix4x4.CreateScale(2f);
        Matrix4x4 secondProjection = Matrix4x4.CreateTranslation(3f, 4f, 0f);

        device.Define2DTransforms(firstProjection);
        int firstResult = device.Set2DTransformForFixedFunction();
        device.Define2DTransforms(secondProjection);
        int secondResult = device.Set2DTransformForFixedFunction();

        Assert.AreEqual((0, 0), (firstResult, secondResult));
        CollectionAssert.AreEqual(
            new[]
            {
                (World, Matrix4x4.Identity),
                (Transformstatetype.View, Matrix4x4.Identity),
                (Transformstatetype.Projection, firstProjection),
                (World, Matrix4x4.Identity),
                (Transformstatetype.View, Matrix4x4.Identity),
                (Transformstatetype.Projection, secondProjection)
            },
            calls);
    }

    [TestMethod]
    public void WhenDeviceApplies2DTransformAndViewFailsThenFirstFailureIsReturned()
    {
        List<Transformstatetype> calls = [];
        using Direct3D9Device device = CreateTransformDevice((state, _) =>
        {
            calls.Add(state);
            return state == Transformstatetype.View ? Direct3D9Factory.GenericFailureHResult : 0;
        });
        device.Define2DTransforms(Matrix4x4.CreateScale(2f));

        int result = device.Set2DTransformForFixedFunction();

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(new[] { World, Transformstatetype.View }, calls);
    }

    [TestMethod]
    public void WhenSetting3DTransformsThenProjectionUsesViewportModifierBeforeSurfaceToClip()
    {
        Matrix4x4 world = Matrix4x4.CreateScale(2f, 3f, 4f);
        Matrix4x4 view = Matrix4x4.CreateTranslation(5f, 6f, 7f);
        Matrix4x4 projection = Matrix4x4.CreateRotationZ(0.25f);
        Matrix4x4 viewportProjectionModifier = Matrix4x4.CreateScale(8f, 9f, 1f);
        Matrix4x4 surfaceToClip = Matrix4x4.CreateTranslation(10f, 11f, 0f);
        List<(Transformstatetype State, Matrix4x4 Matrix)> calls = [];

        int result = Direct3D9TransformState.Set3DTransforms(
            world,
            view,
            projection,
            viewportProjectionModifier,
            surfaceToClip,
            (state, matrix) =>
            {
                calls.Add((state, matrix));
                return 0;
            });

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(
            new[]
            {
                (World, world),
                (Transformstatetype.View, view),
                (Transformstatetype.Projection, projection * viewportProjectionModifier * surfaceToClip)
            },
            calls);
    }

    [TestMethod]
    [DataRow(256, "256")]
    [DataRow((int) Transformstatetype.View, "256,2")]
    [DataRow((int) Transformstatetype.Projection, "256,2,3")]
    public void WhenSetting3DTransformFailsThenFirstFailureIsReturnedAndLaterTransformsAreSkipped(
        int failingStateValue,
        string expectedStates)
    {
        List<Transformstatetype> states = [];
        Transformstatetype failingState = (Transformstatetype) failingStateValue;

        int result = Direct3D9TransformState.Set3DTransforms(
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            (state, _) =>
            {
                states.Add(state);
                return state == failingState ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, expectedStates),
            (result, string.Join(',', states.Select(static state => (int) state))));
    }

    [TestMethod]
    public void WhenWorldAndView3DTransformsReturnNonZeroSuccessThenProjectionStillRuns()
    {
        List<Transformstatetype> states = [];

        int result = Direct3D9TransformState.Set3DTransforms(
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            (state, _) =>
            {
                states.Add(state);
                return state == Transformstatetype.Projection ? 3 : 1;
            });

        Assert.AreEqual(3, result);
        CollectionAssert.AreEqual(
            new[] { World, Transformstatetype.View, Transformstatetype.Projection },
            states);
    }

    [TestMethod]
    public void WhenDeviceView3DTransformFailsThenProjectionIsSkippedAndFailureIsReturned()
    {
        List<Transformstatetype> states = [];
        using Direct3D9Device device = CreateTransformDevice((state, _) =>
        {
            states.Add(state);
            return state == Transformstatetype.View ? Direct3D9Factory.GenericFailureHResult : 0;
        });

        int result = device.Set3DTransforms(
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            Matrix4x4.Identity);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(new[] { World, Transformstatetype.View }, states);
    }

    [TestMethod]
    public void WhenDeviceSets3DTransformsThenCachedSurfaceToClipIsAppliedAndFinalSuccessIsReturned()
    {
        Matrix4x4 world = Matrix4x4.CreateScale(2f, 3f, 4f);
        Matrix4x4 view = Matrix4x4.CreateTranslation(5f, 6f, 7f);
        Matrix4x4 projection = Matrix4x4.CreateRotationZ(0.25f);
        Matrix4x4 viewportProjectionModifier = Matrix4x4.CreateScale(8f, 9f, 1f);
        Matrix4x4 surfaceToClip = Matrix4x4.CreateTranslation(10f, 11f, 0f);
        List<(Transformstatetype State, Matrix4x4 Matrix)> calls = [];
        using Direct3D9Device device = CreateTransformDevice((state, matrix) =>
        {
            calls.Add((state, matrix));
            return state == Transformstatetype.Projection ? 1 : 0;
        });
        device.Define2DTransforms(surfaceToClip);

        int result = device.Set3DTransforms(world, view, projection, viewportProjectionModifier);

        Assert.AreEqual(1, result);
        CollectionAssert.AreEqual(
            new[]
            {
                (World, world),
                (Transformstatetype.View, view),
                (Transformstatetype.Projection, projection * viewportProjectionModifier * surfaceToClip)
            },
            calls);
    }

    [TestMethod]
    public void WhenSetting3DTransformsAfterDisposeThenThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateTransformDevice((_, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.Set3DTransforms(
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            Matrix4x4.Identity));
    }

    [TestMethod]
    public void WhenSetting3DVertexShaderTransformThenMatricesUseNativeRegisterOrder()
    {
        Matrix4x4 world = Matrix4x4.CreateScale(2f, 3f, 4f);
        Matrix4x4 view = Matrix4x4.CreateTranslation(5f, 6f, 7f);
        Matrix4x4 projection = Matrix4x4.CreateRotationZ(0.25f);
        List<(uint Register, Matrix4x4 Matrix)> calls = [];

        int result = Direct3D9TransformState.Set3DTransformForVertexShader(
            world,
            view,
            projection,
            3,
            (register, matrix) =>
            {
                calls.Add((register, matrix));
                return 0;
            });

        Assert.AreEqual(0, result);
        Assert.AreEqual((3u, Matrix4x4.Transpose(world * view)), calls[0]);
        Assert.AreEqual((7u, Matrix4x4.Transpose(world * view * projection)), calls[1]);
        Assert.AreEqual(11u, calls[2].Register);
    }

    [TestMethod]
    public void WhenWorldViewHasNegativeDeterminantThenNormalAdjointSignIsCorrected()
    {
        Matrix4x4 actualNormalTransform = default;

        int result = Direct3D9TransformState.Set3DTransformForVertexShader(
            Matrix4x4.CreateScale(-2f, 3f, 4f),
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            0,
            (register, matrix) =>
            {
                if (register == 8)
                {
                    actualNormalTransform = matrix;
                }

                return 0;
            });

        Assert.AreEqual(0, result);
        Assert.AreEqual(new Matrix4x4(
            -12, 0, 0, 0,
            0, 8, 0, 0,
            0, 0, 6, 0,
            0, 0, 0, 24), actualNormalTransform);
    }

    [TestMethod]
    public void WhenSetting3DVertexShaderTransformFailsThenLaterMatricesAreSkipped()
    {
        List<uint> registers = [];

        int result = Direct3D9TransformState.Set3DTransformForVertexShader(
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            Matrix4x4.Identity,
            4,
            (register, _) =>
            {
                registers.Add(register);
                return register == 8 ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(new uint[] { 4, 8 }, registers);
    }

    [TestMethod]
    public void WhenDeviceSets3DVertexShaderTransformWithoutKnownWorldThenReturnsFailure()
    {
        int shaderCallCount = 0;
        using Direct3D9Device device = CreateTransformDevice(
            (_, _) => 0,
            (_, _) =>
            {
                shaderCallCount++;
                return 0;
            });

        int result = device.Set3DTransformForVertexShader(0);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual(0, shaderCallCount);
    }

    [TestMethod]
    public void WhenDeviceSets3DVertexShaderTransformThenCurrentTransformCacheIsUsed()
    {
        Matrix4x4 world = Matrix4x4.CreateScale(-2f, 3f, 4f);
        Matrix4x4 view = Matrix4x4.Identity;
        Matrix4x4 projection = Matrix4x4.CreateRotationZ(0.25f);
        List<(uint Register, Matrix4x4 Matrix)> calls = [];
        using Direct3D9Device device = CreateTransformDevice(
            (_, _) => 0,
            (register, matrix) =>
            {
                calls.Add((register, matrix));
                return 0;
            });
        device.SetTransform(World, world);
        device.SetTransform(Transformstatetype.View, view);
        device.SetTransform(Transformstatetype.Projection, projection);

        int result = device.Set3DTransformForVertexShader(3);

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(
            new[]
            {
                (3u, Matrix4x4.Transpose(world * view)),
                (7u, Matrix4x4.Transpose(world * view * projection)),
                (11u, new Matrix4x4(
                    -12, 0, 0, 0,
                    0, 8, 0, 0,
                    0, 0, 6, 0,
                    0, 0, 0, 24))
            },
            calls);
    }

    [TestMethod]
    public void WhenDeviceRepeats3DVertexShaderTransformThenConstantsAreSkipped()
    {
        int shaderCallCount = 0;
        using Direct3D9Device device = CreateTransformDevice(
            (_, _) => 0,
            (_, _) =>
            {
                shaderCallCount++;
                return 0;
            });
        device.SetTransform(World, Matrix4x4.Identity);
        device.SetTransform(Transformstatetype.View, Matrix4x4.Identity);
        device.SetTransform(Transformstatetype.Projection, Matrix4x4.Identity);

        int firstResult = device.Set3DTransformForVertexShader(0);
        int secondResult = device.Set3DTransformForVertexShader(0);

        Assert.AreEqual(0, firstResult);
        Assert.AreEqual(0, secondResult);
        Assert.AreEqual(3, shaderCallCount);
    }

    [TestMethod]
    public void WhenDefining2DTransformsAfterDisposeThenThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateTransformDevice((_, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.Define2DTransforms(Matrix4x4.Identity));
    }

    [TestMethod]
    public void WhenSetting2DFixedFunctionTransformAfterDisposeThenThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateTransformDevice((_, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.Set2DTransformForFixedFunction());
    }

    [TestMethod]
    public void WhenSetting3DVertexShaderTransformAfterDisposeThenThrowsObjectDisposedException()
    {
        Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setVertexShaderConstants: (_, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.Set3DTransformForVertexShader(0));
    }

    private static Direct3D9Device CreateTransformDevice(
        Direct3D9SetTransform setTransform,
        Func<uint, Matrix4x4, int>? setVertexShaderConstants = null)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setVertexShaderConstants: setVertexShaderConstants,
            setTransform: setTransform);
    }
}
