using System.Numerics;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9BitmapColorSourceShaderDataTests
{
    [TestMethod]
    public void WhenCalculatingNaturalTextureTransformThenSourceToXSpaceIsInverted()
    {
        int result = Direct3D9BitmapColorSourceTransform.Calculate(
            new Matrix3x2(2, 0, 0, 4, 10, 20),
            100,
            50,
            200,
            100,
            200,
            100,
            new Direct3D9BitmapRealizationRectangle(0, 0, 100, 50),
            Direct3D9TexelLayout.Natural,
            Direct3D9TexelLayout.Natural,
            out Matrix3x2 transform);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, new Matrix3x2(0.005f, 0, 0, 0.005f, -0.05f, -0.1f)),
            (result, Round(transform)));
    }

    [TestMethod]
    public void WhenCalculatingEdgeWrappedTextureTransformThenPrefilteredSpanExcludesBorders()
    {
        int result = Direct3D9BitmapColorSourceTransform.Calculate(
            Matrix3x2.Identity,
            12,
            22,
            10,
            20,
            10,
            20,
            new Direct3D9BitmapRealizationRectangle(0, 0, 10, 20),
            Direct3D9TexelLayout.EdgeWrapped,
            Direct3D9TexelLayout.EdgeMirrored,
            out Matrix3x2 transform);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, new Matrix3x2(0.1f, 0, 0, 0.05f, 0, 0)),
            (result, Round(transform)));
    }

    [TestMethod]
    public void WhenTextureTransformIsNotInvertibleThenNativeFailureIsReturned()
    {
        int result = Direct3D9BitmapColorSourceTransform.Calculate(
            new Matrix3x2(0, 0, 0, 1, 0, 0),
            1,
            1,
            1,
            1,
            1,
            1,
            new Direct3D9BitmapRealizationRectangle(0, 0, 1, 1),
            Direct3D9TexelLayout.Natural,
            Direct3D9TexelLayout.Natural,
            out Matrix3x2 transform);

        Assert.AreEqual((Direct3D9Factory.NonInvertibleMatrixHResult, default(Matrix3x2)), (result, transform));
    }

    [TestMethod]
    public void WhenSettingShaderMatrixThenNativeTwoRegisterPackingIsUsed()
    {
        uint actualRegister = 0;
        Vector4[] actualConstants = [];
        Direct3D9PipelineShaderMatrix3x2State shaderState = new((startRegister, constants) =>
        {
            actualRegister = startRegister;
            actualConstants = constants.ToArray();
            return Direct3D9Factory.SuccessHResult;
        });

        int result = shaderState.SetMatrix3x2(17, 9, new Matrix3x2(1, 2, 3, 4, 5, 6));

        CollectionAssert.AreEqual(
            new object[] { Direct3D9Factory.SuccessHResult, 9u, new Vector4(1, 3, 5, 0), new Vector4(2, 4, 6, 0) },
            new object[] { result, actualRegister, actualConstants[0], actualConstants[1] });
    }

    [TestMethod]
    public void WhenPipelineSendsBitmapShaderDataThenTextureBindingPrecedesMatrixUpload()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9BitmapColorSourceDeviceState bitmapState = new(
            device,
            (IDirect3DBaseTexture9*) 31,
            MilBitmapInterpolationMode.Linear,
            Textureaddress.Clamp,
            Textureaddress.Clamp,
            useHardwareTransform: true,
            shaderTextureTransformRegister: 9,
            new Matrix3x2(1, 2, 3, 4, 5, 6),
            (shader, register, matrix) =>
            {
                calls.Add($"matrix:{shader}:{register}:{matrix.M11},{matrix.M12},{matrix.M21},{matrix.M22},{matrix.M31},{matrix.M32}");
                return Direct3D9Factory.SuccessHResult;
            });
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            bitmapState.SendDeviceStates,
            bitmapState.SendShaderData);
        Direct3D9ShaderPipelineDeviceStateSender sender = new(
            true,
            17,
            [new Direct3D9ShaderPipelineItem(2, colorSource)],
            vertexBuffer =>
            {
                calls.Add($"vertex:{vertexBuffer}");
                return 0;
            },
            () =>
            {
                calls.Add("blend");
                return 0;
            },
            (shader, is2D) =>
            {
                calls.Add($"shader:{shader}:{is2D}");
                return 0;
            });

        int result = sender.SendDeviceStates(23);

        Assert.AreEqual(
            (0, "sampler:SampMagfilter|sampler:SampMinfilter|sampler:SampMipfilter|sampler:SampAddressu|sampler:SampAddressv|stage:TssTexcoordindex|texture:2:31|matrix:17:9:1,2,3,4,5,6|vertex:23|blend|shader:17:True"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderMatrixUploadFailsThenLaterPipelineStateIsSkipped()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9BitmapColorSourceDeviceState bitmapState = new(
            device,
            (IDirect3DBaseTexture9*) 31,
            MilBitmapInterpolationMode.Linear,
            Textureaddress.Clamp,
            Textureaddress.Clamp,
            useHardwareTransform: true,
            shaderTextureTransformRegister: 9,
            Matrix3x2.Identity,
            (_, _, _) =>
            {
                calls.Add("matrix");
                return Direct3D9Factory.GenericFailureHResult;
            });
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            bitmapState.SendDeviceStates,
            bitmapState.SendShaderData);
        Direct3D9ShaderPipelineDeviceStateSender sender = new(
            true,
            17,
            [new Direct3D9ShaderPipelineItem(2, colorSource)],
            _ =>
            {
                calls.Add("vertex");
                return 0;
            },
            () =>
            {
                calls.Add("blend");
                return 0;
            },
            (_, _) =>
            {
                calls.Add("shader");
                return 0;
            });

        int result = sender.SendDeviceStates(23);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "sampler:SampMagfilter|sampler:SampMinfilter|sampler:SampMipfilter|sampler:SampAddressu|sampler:SampAddressv|stage:TssTexcoordindex|texture:2:31|matrix"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenCreatingStateFromRealizationThenLayoutAndTransformDrivePipelineState()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9BitmapRealizationProperties properties = new(
            MilBitmapInterpolationMode.TriLinear,
            Direct3D9TextureMipMapLevel.All,
            MilBitmapWrapMode.FlipX,
            MilPixelFormat.Pbgra32Bpp,
            IsMinimumRealizationRectComputed: true,
            BitmapWidth: 100,
            BitmapHeight: 80,
            Width: 50,
            Height: 40)
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(5, 4, 45, 36),
            LayoutU = new Direct3D9BitmapDimensionLayout(42, Direct3D9TexelLayout.EdgeWrapped, Textureaddress.Mirror),
            LayoutV = new Direct3D9BitmapDimensionLayout(34, Direct3D9TexelLayout.EdgeMirrored, Textureaddress.Wrap)
        };

        int result = Direct3D9BitmapColorSourceDeviceStateFactory.TryCreate(
            device,
            (IDirect3DBaseTexture9*) 41,
            properties,
            Matrix3x2.Identity,
            useHardwareTransform: false,
            shaderTextureTransformRegister: 7,
            (shader, register, matrix) =>
            {
                calls.Add($"matrix:{shader}:{register}:{Round(matrix).M11},{Round(matrix).M22},{Round(matrix).M31},{Round(matrix).M32}");
                return 0;
            },
            out Direct3D9BitmapColorSourceDeviceState? state);
        Assert.AreEqual(0, result);

        result = state!.SendDeviceStates(3, 2);
        if (result >= 0)
        {
            result = state.SendShaderData(17);
        }

        Assert.AreEqual(
            (0, "sampler:SampMagfilter|sampler:SampMinfilter|sampler:SampMipfilter|sampler:SampAddressu|sampler:SampAddressv|stage:TssTexcoordindex|texture:2:41|matrix:17:7:0.0125,0.015625,-0.125,-0.125"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenRealizationTransformIsNotInvertibleThenStateIsNotCreated()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9BitmapRealizationProperties properties = new(
            MilBitmapInterpolationMode.Linear,
            Direct3D9TextureMipMapLevel.One,
            MilBitmapWrapMode.Extend,
            MilPixelFormat.Pbgra32Bpp,
            IsMinimumRealizationRectComputed: false,
            BitmapWidth: 1,
            BitmapHeight: 1,
            Width: 1,
            Height: 1)
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 1, 1),
            LayoutU = new Direct3D9BitmapDimensionLayout(1, Direct3D9TexelLayout.Natural, Textureaddress.Clamp),
            LayoutV = new Direct3D9BitmapDimensionLayout(1, Direct3D9TexelLayout.Natural, Textureaddress.Clamp)
        };

        int result = Direct3D9BitmapColorSourceDeviceStateFactory.TryCreate(
            device,
            (IDirect3DBaseTexture9*) 41,
            properties,
            new Matrix3x2(0, 0, 0, 1, 0, 0),
            useHardwareTransform: true,
            shaderTextureTransformRegister: null,
            setShaderMatrix: null,
            out Direct3D9BitmapColorSourceDeviceState? state);

        Assert.AreEqual(
            (Direct3D9Factory.NonInvertibleMatrixHResult, null, string.Empty),
            (result, state, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow((int) MilPixelFormat.Bgr32Bpp, true)]
    [DataRow((int) MilPixelFormat.Pbgra32Bpp, false)]
    public void WhenCreatingBitmapPipelineColorSourceThenTextureFormatDeterminesOpacity(
        int textureFormatValue,
        bool expectedIsOpaque)
    {
        MilPixelFormat textureFormat = (MilPixelFormat) textureFormatValue;
        using Direct3D9Device device = CreateDevice([]);
        using Direct3D9BitmapColorSourceTextureRealizer realizer = new(
            () => (Direct3D9Factory.NotImplementedHResult, null));
        Direct3D9BitmapRealizationProperties properties = new(
            MilBitmapInterpolationMode.Linear,
            Direct3D9TextureMipMapLevel.One,
            MilBitmapWrapMode.Extend,
            textureFormat,
            IsMinimumRealizationRectComputed: false,
            BitmapWidth: 1,
            BitmapHeight: 1,
            Width: 1,
            Height: 1)
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 1, 1),
            LayoutU = new Direct3D9BitmapDimensionLayout(1, Direct3D9TexelLayout.Natural, Textureaddress.Clamp),
            LayoutV = new Direct3D9BitmapDimensionLayout(1, Direct3D9TexelLayout.Natural, Textureaddress.Clamp)
        };

        Direct3D9PipelineColorSource colorSource = realizer.CreatePipelineColorSource(
            device,
            properties,
            Matrix3x2.Identity,
            useHardwareTransform: false,
            shaderTextureTransformRegister: null);

        Assert.AreEqual(expectedIsOpaque, colorSource.IsOpaque);
    }

    private static Matrix3x2 Round(Matrix3x2 matrix) => new(
        MathF.Round(matrix.M11, 6),
        MathF.Round(matrix.M12, 6),
        MathF.Round(matrix.M21, 6),
        MathF.Round(matrix.M22, 6),
        MathF.Round(matrix.M31, 6),
        MathF.Round(matrix.M32, 6));

    private static Direct3D9Device CreateDevice(List<string> calls)
    {
        Caps9 capabilities = new()
        {
            MaxTextureBlendStages = 8,
            MaxSimultaneousTextures = 8
        };

        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            capabilities: capabilities,
            setTextureStageState: (_, state, _) =>
            {
                calls.Add($"stage:{state}");
                return 0;
            },
            setTexture: (stage, texture) =>
            {
                calls.Add($"texture:{stage}:{(nint) texture}");
                return 0;
            },
            setSamplerState: (_, state, _) =>
            {
                calls.Add($"sampler:{state}");
                return 0;
            });
    }
}
