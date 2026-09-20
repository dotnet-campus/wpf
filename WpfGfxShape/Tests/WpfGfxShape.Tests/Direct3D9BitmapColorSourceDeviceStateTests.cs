using System.Numerics;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9BitmapColorSourceDeviceStateTests
{
    [TestMethod]
    public void WhenSendingBorderedTrilinearStateThenNativeOrderAndHardwareTransformMatch()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Matrix3x2 textureTransform = new(1, 2, 3, 4, 5, 6);
        Direct3D9BitmapColorSourceDeviceState state = new(
            device,
            (IDirect3DBaseTexture9*) 42,
            MilBitmapInterpolationMode.TriLinear,
            Textureaddress.Border,
            Textureaddress.Mirror,
            useHardwareTransform: true,
            shaderTextureTransformRegister: null,
            textureTransform);

        int result = state.SendDeviceStates(2, 3);

        Assert.AreEqual(
            (0, "sampler:3:SampMagfilter:2|sampler:3:SampMinfilter:2|sampler:3:SampMipfilter:2|sampler:3:SampAddressu:4|sampler:3:SampAddressv:2|sampler:3:SampBordercolor:0|stage:2:TssTexcoordindex:2|transform:18:1,2,3,4,5,6|stage:2:TssTexturetransformflags:2|texture:3:42"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderOwnsTextureTransformThenFixedFunctionTransformStateIsSkipped()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9BitmapColorSourceDeviceState state = new(
            device,
            (IDirect3DBaseTexture9*) 73,
            MilBitmapInterpolationMode.NearestNeighbor,
            Textureaddress.Clamp,
            Textureaddress.Wrap,
            useHardwareTransform: true,
            shaderTextureTransformRegister: 9,
            Matrix3x2.Identity,
            (_, _, _) => 0);

        int result = state.SendDeviceStates(1, 1);

        Assert.AreEqual(
            (0, "sampler:1:SampMagfilter:1|sampler:1:SampMinfilter:1|sampler:1:SampMipfilter:0|sampler:1:SampAddressu:3|sampler:1:SampAddressv:1|stage:1:TssTexcoordindex:1|texture:1:73"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSamplerStateFailsThenFirstFailureStopsLaterStateAndTextureBinding()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(
            calls,
            (sampler, state, value) => state == Samplerstatetype.Addressu
                ? Direct3D9Factory.InvalidCallHResult
                : 0);
        Direct3D9BitmapColorSourceDeviceState sourceState = new(
            device,
            (IDirect3DBaseTexture9*) 99,
            MilBitmapInterpolationMode.Linear,
            Textureaddress.Clamp,
            Textureaddress.Clamp,
            useHardwareTransform: false,
            shaderTextureTransformRegister: null,
            Matrix3x2.Identity);

        int result = sourceState.SendDeviceStates(0, 0);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, "sampler:0:SampMagfilter:2|sampler:0:SampMinfilter:2|sampler:0:SampMipfilter:0|sampler:0:SampAddressu:3"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenHardwareTransformIsUnusedThenTextureTransformIsDisabledBeforeBinding()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9BitmapColorSourceDeviceState state = new(
            device,
            (IDirect3DBaseTexture9*) 11,
            MilBitmapInterpolationMode.Cubic,
            Textureaddress.Clamp,
            Textureaddress.Clamp,
            useHardwareTransform: false,
            shaderTextureTransformRegister: null,
            Matrix3x2.Identity);

        int result = state.SendDeviceStates(0, 0);

        Assert.AreEqual(
            (0, "sampler:0:SampMagfilter:2|sampler:0:SampMinfilter:2|sampler:0:SampMipfilter:0|sampler:0:SampAddressu:3|sampler:0:SampAddressv:3|stage:0:TssTexcoordindex:0|stage:0:TssTexturetransformflags:0|texture:0:11"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDynamicTextureChangesThenLatestTextureIsBound()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        IDirect3DBaseTexture9* texture = (IDirect3DBaseTexture9*) 31;
        Direct3D9BitmapColorSourceDeviceState state = new(
            device,
            () => texture,
            MilBitmapInterpolationMode.Linear,
            Textureaddress.Clamp,
            Textureaddress.Clamp,
            useHardwareTransform: false,
            shaderTextureTransformRegister: null,
            Matrix3x2.Identity);

        int firstResult = state.SendDeviceStates(0, 0);
        texture = (IDirect3DBaseTexture9*) 37;
        int secondResult = state.SendDeviceStates(0, 0);

        Assert.AreEqual((0, 0, 2, "texture:0:37"),
            (firstResult, secondResult, calls.Count(call => call.StartsWith("texture:", StringComparison.Ordinal)), calls.Last()));
    }

    [TestMethod]
    public void WhenDynamicTextureIsUnavailableThenBindingFails()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9BitmapColorSourceDeviceState state = new(
            device,
            () => null,
            MilBitmapInterpolationMode.Linear,
            Textureaddress.Clamp,
            Textureaddress.Clamp,
            useHardwareTransform: false,
            shaderTextureTransformRegister: null,
            Matrix3x2.Identity);

        int result = state.SendDeviceStates(0, 0);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenBitmapColorSourceIsAddedToShaderPipelineThenNewShaderHandleCanBeAssigned()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9BitmapColorSourceDeviceState state = new(
            device,
            (IDirect3DBaseTexture9*) 17,
            MilBitmapInterpolationMode.Linear,
            Textureaddress.Clamp,
            Textureaddress.Clamp,
            useHardwareTransform: true,
            shaderTextureTransformRegister: 3,
            Matrix3x2.Identity,
            (shader, register, _) =>
            {
                calls.Add($"matrix:{shader}:{register}");
                return 0;
            });
        Direct3D9PipelineColorSource colorSource = state.CreatePipelineColorSource(() => 0);

        _ = new Direct3D9ShaderPipelineItem(1, colorSource);
        state.SetTextureTransformHandle(11);
        int result = state.SendShaderData(29);

        Assert.AreEqual((0, "matrix:29:11"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPipelineReuseIsResetThenShaderHandleAndHardwareTransformAreCleared()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9BitmapColorSourceDeviceState state = new(
            device,
            (IDirect3DBaseTexture9*) 19,
            MilBitmapInterpolationMode.Linear,
            Textureaddress.Clamp,
            Textureaddress.Clamp,
            useHardwareTransform: true,
            shaderTextureTransformRegister: 7,
            Matrix3x2.Identity,
            (_, _, _) => 0);

        state.ResetForPipelineReuse();
        int result = state.SendDeviceStates(0, 0);

        Assert.AreEqual(
            (0, "sampler:0:SampMagfilter:2|sampler:0:SampMinfilter:2|sampler:0:SampMipfilter:0|sampler:0:SampAddressu:3|sampler:0:SampAddressv:3|stage:0:TssTexcoordindex:0|stage:0:TssTexturetransformflags:0|texture:0:19"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderHandleIsAssignedAfterResetThenShaderTransformOwnsTheMapping()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9BitmapColorSourceDeviceState state = new(
            device,
            (IDirect3DBaseTexture9*) 23,
            MilBitmapInterpolationMode.Linear,
            Textureaddress.Clamp,
            Textureaddress.Clamp,
            useHardwareTransform: true,
            shaderTextureTransformRegister: 3,
            Matrix3x2.Identity,
            (shader, register, _) =>
            {
                calls.Add($"matrix:{shader}:{register}");
                return Direct3D9Factory.SuccessHResult;
            });

        state.ResetForPipelineReuse();
        state.SetTextureTransformHandle(11);
        int result = state.SendVertexMapping(0, Direct3D9VertexFormatAttribute.Uv1);
        if (result >= 0)
        {
            result = state.SendDeviceStates(1, 1);
        }

        if (result >= 0)
        {
            result = state.SendShaderData(31);
        }

        Assert.AreEqual(
            (0, "sampler:1:SampMagfilter:2|sampler:1:SampMinfilter:2|sampler:1:SampMipfilter:0|sampler:1:SampAddressu:3|sampler:1:SampAddressv:3|stage:1:TssTexcoordindex:1|texture:1:23|matrix:31:11"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPreGeneratedVerticesAreMappedThenHardwareTransformIsSelected()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Matrix3x2 transform = new(1, 2, 3, 4, 5, 6);
        Direct3D9BitmapColorSourceDeviceState state = new(
            device,
            (IDirect3DBaseTexture9*) 29,
            MilBitmapInterpolationMode.Linear,
            Textureaddress.Clamp,
            Textureaddress.Clamp,
            useHardwareTransform: false,
            shaderTextureTransformRegister: null,
            transform);

        int result = state.SendVertexMapping(0, Direct3D9VertexFormatAttribute.Uv1);
        if (result >= 0)
        {
            result = state.SendDeviceStates(2, 2);
        }

        Assert.AreEqual(
            (0, "sampler:2:SampMagfilter:2|sampler:2:SampMinfilter:2|sampler:2:SampMipfilter:0|sampler:2:SampAddressu:3|sampler:2:SampAddressv:3|stage:2:TssTexcoordindex:2|transform:18:1,2,3,4,5,6|stage:2:TssTexturetransformflags:2|texture:2:29"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenGeneratedVerticesAreMappedThenCoordinateIndexAndMatrixAreSentToBuilder()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Matrix3x2 transform = new(1, 2, 3, 4, 5, 6);
        Direct3D9BitmapColorSourceDeviceState state = new(
            device,
            (IDirect3DBaseTexture9*) 37,
            MilBitmapInterpolationMode.Linear,
            Textureaddress.Clamp,
            Textureaddress.Clamp,
            useHardwareTransform: true,
            shaderTextureTransformRegister: null,
            transform);

        int result = state.SendVertexMapping(
            17,
            Direct3D9VertexFormatAttribute.Uv3 & ~Direct3D9VertexFormatAttribute.Uv2,
            (builder, destination, source, matrix) =>
            {
                calls.Add($"mapping:{builder}:{destination}:{source}:{matrix.M11},{matrix.M12},{matrix.M21},{matrix.M22},{matrix.M31},{matrix.M32}");
                return Direct3D9Factory.SuccessHResult;
            });
        if (result >= 0)
        {
            result = state.SendDeviceStates(0, 0);
        }

        Assert.AreEqual(
            (0, "mapping:17:2:4294967295:1,2,3,4,5,6|sampler:0:SampMagfilter:2|sampler:0:SampMinfilter:2|sampler:0:SampMipfilter:0|sampler:0:SampAddressu:3|sampler:0:SampAddressv:3|stage:0:TssTexcoordindex:0|stage:0:TssTexturetransformflags:0|texture:0:37"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenTextureMappingFailsThenFailureIsPreserved()
    {
        using Direct3D9Device device = CreateDevice([]);
        Direct3D9BitmapColorSourceDeviceState state = new(
            device,
            (IDirect3DBaseTexture9*) 43,
            MilBitmapInterpolationMode.Linear,
            Textureaddress.Clamp,
            Textureaddress.Clamp,
            useHardwareTransform: false,
            shaderTextureTransformRegister: null,
            Matrix3x2.Identity);

        int result = state.SendVertexMapping(
            17,
            Direct3D9VertexFormatAttribute.Uv1,
            (_, _, _, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    private static Direct3D9Device CreateDevice(
        List<string> calls,
        Direct3D9SetSamplerState? samplerResult = null)
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
            setTextureStageState: (stage, state, value) =>
            {
                calls.Add($"stage:{stage}:{state}:{value}");
                return 0;
            },
            setTexture: (stage, texture) =>
            {
                calls.Add($"texture:{stage}:{(nint) texture}");
                return 0;
            },
            setSamplerState: (sampler, state, value) =>
            {
                calls.Add($"sampler:{sampler}:{state}:{value}");
                return samplerResult?.Invoke(sampler, state, value) ?? 0;
            },
            setTransform: (state, matrix) =>
            {
                calls.Add($"transform:{(uint) state}:{matrix.M11},{matrix.M12},{matrix.M21},{matrix.M22},{matrix.M31},{matrix.M32}");
                return 0;
            });
    }
}
