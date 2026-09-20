using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9Level1DeviceTestTests
{
    [TestMethod]
    public void WhenCapabilitiesAreSupportedThenAlphaSolidBrushStatesAreSetInOrder()
    {
        List<string> actualCalls = [];

        int result = Direct3D9Level1DeviceTest.Test(
            CreateSupportedCapabilities(),
            (state, value) =>
            {
                actualCalls.Add($"RenderState:{state}:{value}");
                return 0;
            },
            () =>
            {
                actualCalls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                actualCalls.Add("VertexShader:null");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            },
            stage =>
            {
                actualCalls.Add($"Texture:null:{stage}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                "PixelShader:null",
                "VertexShader:null",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TADiffuse}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TADiffuse}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                "Texture:null:0",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenCapabilityCheckFailsThenRenderStateIsNotSet()
    {
        int callCount = 0;
        Caps9 capabilities = CreateSupportedCapabilities();
        capabilities.MaxTextureBlendStages = 1;

        int result = Direct3D9Level1DeviceTest.Test(
            capabilities,
            (_, _) =>
            {
                callCount++;
                return 0;
            },
            () =>
            {
                callCount++;
                return 0;
            },
            () =>
            {
                callCount++;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (result, callCount));
    }

    [TestMethod]
    public void WhenAlphaBlendEnableFailsThenOriginalHResultIsReturned()
    {
        int callCount = 0;

        int result = Direct3D9Level1DeviceTest.Test(
            CreateSupportedCapabilities(),
            (_, _) =>
            {
                callCount++;
                return Direct3D9Factory.InvalidCallHResult;
            },
            () =>
            {
                callCount++;
                return 0;
            },
            () =>
            {
                callCount++;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 1), (result, callCount));
    }

    [TestMethod]
    public void WhenSourceBlendFailsThenOriginalHResultIsReturned()
    {
        int callCount = 0;

        int result = Direct3D9Level1DeviceTest.Test(
            CreateSupportedCapabilities(),
            (_, _) => ++callCount == 1 ? 0 : Direct3D9Factory.InvalidCallHResult,
            () =>
            {
                callCount++;
                return 0;
            },
            () =>
            {
                callCount++;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 2), (result, callCount));
    }

    [TestMethod]
    public void WhenDestinationBlendFailsThenOriginalHResultIsReturned()
    {
        int callCount = 0;

        int result = Direct3D9Level1DeviceTest.Test(
            CreateSupportedCapabilities(),
            (_, _) => ++callCount < 3 ? 0 : Direct3D9Factory.InvalidCallHResult,
            () =>
            {
                callCount++;
                return 0;
            },
            () =>
            {
                callCount++;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 3), (result, callCount));
    }

    [TestMethod]
    public void WhenClearingPixelShaderFailsThenOriginalHResultIsReturned()
    {
        int callCount = 0;

        int result = Direct3D9Level1DeviceTest.Test(
            CreateSupportedCapabilities(),
            (_, _) =>
            {
                callCount++;
                return 0;
            },
            () =>
            {
                callCount++;
                return Direct3D9Factory.InvalidCallHResult;
            },
            () =>
            {
                callCount++;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 4), (result, callCount));
    }

    [TestMethod]
    public void WhenClearingVertexShaderFailsThenOriginalHResultIsReturned()
    {
        int callCount = 0;

        int result = Direct3D9Level1DeviceTest.Test(
            CreateSupportedCapabilities(),
            (_, _) =>
            {
                callCount++;
                return 0;
            },
            () =>
            {
                callCount++;
                return 0;
            },
            () =>
            {
                callCount++;
                return Direct3D9Factory.InvalidCallHResult;
            });

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 5), (result, callCount));
    }

    [TestMethod]
    public void WhenCapabilitiesAreSupportedThenDiffuseTextureStageOperationIsSetInOrder()
    {
        List<string> actualCalls = [];

        int result = Direct3D9Level1DeviceTest.Test(
            CreateSupportedCapabilities(),
            (_, _) => 0,
            () => 0,
            () => 0,
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            },
            stage =>
            {
                actualCalls.Add($"Texture:null:{stage}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TADiffuse}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TADiffuse}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                "Texture:null:0",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenDisableTextureStageEqualsMaximumThenDeviceStateIsNotSet()
    {
        int callCount = 0;

        int result = Direct3D9Level1DeviceTest.DisableTextureStage(1, 1, (_, _, _) =>
        {
            callCount++;
            return Direct3D9Factory.InvalidCallHResult;
        });

        Assert.AreEqual((0, 0), (result, callCount));
    }

    [TestMethod]
    public void WhenDisableTextureStageIsBelowMaximumThenColorOperationIsDisabled()
    {
        (uint Stage, Texturestagestatetype State, uint Value)? actualCall = null;

        int result = Direct3D9Level1DeviceTest.DisableTextureStage(1, 2, (stage, state, value) =>
        {
            actualCall = (stage, state, value);
            return 0;
        });

        Assert.AreEqual(
            (0, ((uint Stage, Texturestagestatetype State, uint Value)?) (1, Texturestagestatetype.Colorop, (uint) Textureop.Disable)),
            (result, actualCall));
    }

    [DataTestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    public void WhenDiffuseTextureStageOperationCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        int callCount = 0;

        int result = Direct3D9Level1DeviceTest.Test(
            CreateSupportedCapabilities(),
            (_, _) => 0,
            () => 0,
            () => 0,
            (_, _, _) => ++callCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0,
            _ => ++callCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, failingCall), (result, callCount));
    }

    [TestMethod]
    public void WhenNearestNeighborTextureStateIsSetThenCallsMatchNativeOrder()
    {
        List<string> actualCalls = [];

        int result = Direct3D9Level1DeviceTest.SetNearestNeighborTextureState(
            2,
            () =>
            {
                actualCalls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                actualCalls.Add("VertexShader:null");
                return 0;
            },
            (sampler, state, value) =>
            {
                actualCalls.Add($"SamplerState:{sampler}:{state}:{value}");
                return 0;
            },
            (state, value) =>
            {
                actualCalls.Add($"RenderState:{state}:{value}");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Point}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Point}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) Texturefiltertype.None}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    public void WhenNearestNeighborTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        int callCount = 0;
        int NextResult() => ++callCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;

        int result = Direct3D9Level1DeviceTest.SetNearestNeighborTextureState(
            2,
            NextResult,
            NextResult,
            (_, _, _) => NextResult(),
            (_, _) => NextResult(),
            (_, _, _) => NextResult());

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, failingCall), (result, callCount));
    }

    [TestMethod]
    public void WhenLinearTextureStateIsSetThenCallsMatchNativeOrder()
    {
        List<string> actualCalls = [];

        int result = Direct3D9Level1DeviceTest.SetLinearTextureState(
            2,
            () =>
            {
                actualCalls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                actualCalls.Add("VertexShader:null");
                return 0;
            },
            (sampler, state, value) =>
            {
                actualCalls.Add($"SamplerState:{sampler}:{state}:{value}");
                return 0;
            },
            (state, value) =>
            {
                actualCalls.Add($"RenderState:{state}:{value}");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) Texturefiltertype.None}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    public void WhenLinearTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        int callCount = 0;
        int NextResult() => ++callCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;

        int result = Direct3D9Level1DeviceTest.SetLinearTextureState(
            2,
            NextResult,
            NextResult,
            (_, _, _) => NextResult(),
            (_, _) => NextResult(),
            (_, _, _) => NextResult());

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, failingCall), (result, callCount));
    }

    [TestMethod]
    public void WhenTriLinearTextureStateIsSetThenCallsMatchNativeOrder()
    {
        List<string> actualCalls = [];

        int result = Direct3D9Level1DeviceTest.SetTriLinearTextureState(
            2,
            () =>
            {
                actualCalls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                actualCalls.Add("VertexShader:null");
                return 0;
            },
            (sampler, state, value) =>
            {
                actualCalls.Add($"SamplerState:{sampler}:{state}:{value}");
                return 0;
            },
            (state, value) =>
            {
                actualCalls.Add($"RenderState:{state}:{value}");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) Texturefiltertype.Linear}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    public void WhenTriLinearTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        int callCount = 0;
        int NextResult() => ++callCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;

        int result = Direct3D9Level1DeviceTest.SetTriLinearTextureState(
            2,
            NextResult,
            NextResult,
            (_, _, _) => NextResult(),
            (_, _) => NextResult(),
            (_, _, _) => NextResult());

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, failingCall), (result, callCount));
    }

    [TestMethod]
    public void WhenNearestNeighborDiffuseTextureStateIsSetThenCallsMatchNativeOrder()
    {
        List<string> actualCalls = [];

        int result = Direct3D9Level1DeviceTest.SetNearestNeighborDiffuseTextureState(
            2,
            () =>
            {
                actualCalls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                actualCalls.Add("VertexShader:null");
                return 0;
            },
            (sampler, state, value) =>
            {
                actualCalls.Add($"SamplerState:{sampler}:{state}:{value}");
                return 0;
            },
            (state, value) =>
            {
                actualCalls.Add($"RenderState:{state}:{value}");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Point}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Point}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) Texturefiltertype.None}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    public void WhenNearestNeighborDiffuseTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        int callCount = 0;
        int NextResult() => ++callCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;

        int result = Direct3D9Level1DeviceTest.SetNearestNeighborDiffuseTextureState(
            2,
            NextResult,
            NextResult,
            (_, _, _) => NextResult(),
            (_, _) => NextResult(),
            (_, _, _) => NextResult());

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, failingCall), (result, callCount));
    }

    [TestMethod]
    public void WhenLinearDiffuseTextureStateIsSetThenCallsMatchNativeOrder()
    {
        List<string> actualCalls = [];

        int result = Direct3D9Level1DeviceTest.SetLinearDiffuseTextureState(
            2,
            () =>
            {
                actualCalls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                actualCalls.Add("VertexShader:null");
                return 0;
            },
            (sampler, state, value) =>
            {
                actualCalls.Add($"SamplerState:{sampler}:{state}:{value}");
                return 0;
            },
            (state, value) =>
            {
                actualCalls.Add($"RenderState:{state}:{value}");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) Texturefiltertype.None}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    public void WhenLinearDiffuseTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        int callCount = 0;
        int NextResult() => ++callCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;

        int result = Direct3D9Level1DeviceTest.SetLinearDiffuseTextureState(
            2,
            NextResult,
            NextResult,
            (_, _, _) => NextResult(),
            (_, _) => NextResult(),
            (_, _, _) => NextResult());

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, failingCall), (result, callCount));
    }

    [TestMethod]
    public void WhenTriLinearDiffuseTextureStateIsSetThenCallsMatchNativeOrder()
    {
        List<string> actualCalls = [];

        int result = Direct3D9Level1DeviceTest.SetTriLinearDiffuseTextureState(
            2,
            () =>
            {
                actualCalls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                actualCalls.Add("VertexShader:null");
                return 0;
            },
            (sampler, state, value) =>
            {
                actualCalls.Add($"SamplerState:{sampler}:{state}:{value}");
                return 0;
            },
            (state, value) =>
            {
                actualCalls.Add($"RenderState:{state}:{value}");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) Texturefiltertype.Linear}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    public void WhenTriLinearDiffuseTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        int callCount = 0;
        int NextResult() => ++callCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;

        int result = Direct3D9Level1DeviceTest.SetTriLinearDiffuseTextureState(
            2,
            NextResult,
            NextResult,
            (_, _, _) => NextResult(),
            (_, _) => NextResult(),
            (_, _, _) => NextResult());

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, failingCall), (result, callCount));
    }

    [TestMethod]
    public void WhenNearestNeighborSpecularTextureStateIsSetThenCallsMatchNativeOrder()
    {
        List<string> actualCalls = [];

        int result = Direct3D9Level1DeviceTest.SetNearestNeighborSpecularTextureState(
            2,
            () =>
            {
                actualCalls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                actualCalls.Add("VertexShader:null");
                return 0;
            },
            (sampler, state, value) =>
            {
                actualCalls.Add($"SamplerState:{sampler}:{state}:{value}");
                return 0;
            },
            (state, value) =>
            {
                actualCalls.Add($"RenderState:{state}:{value}");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Point}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Point}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) Texturefiltertype.None}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TASpecular}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    public void WhenNearestNeighborSpecularTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        int callCount = 0;
        int NextResult() => ++callCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;

        int result = Direct3D9Level1DeviceTest.SetNearestNeighborSpecularTextureState(
            2,
            NextResult,
            NextResult,
            (_, _, _) => NextResult(),
            (_, _) => NextResult(),
            (_, _, _) => NextResult());

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, failingCall), (result, callCount));
    }

    [TestMethod]
    public void WhenLinearSpecularTextureStateIsSetThenCallsMatchNativeOrder()
    {
        List<string> actualCalls = [];

        int result = Direct3D9Level1DeviceTest.SetLinearSpecularTextureState(
            2,
            () =>
            {
                actualCalls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                actualCalls.Add("VertexShader:null");
                return 0;
            },
            (sampler, state, value) =>
            {
                actualCalls.Add($"SamplerState:{sampler}:{state}:{value}");
                return 0;
            },
            (state, value) =>
            {
                actualCalls.Add($"RenderState:{state}:{value}");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) Texturefiltertype.None}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TASpecular}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    public void WhenLinearSpecularTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        int callCount = 0;
        int NextResult() => ++callCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;

        int result = Direct3D9Level1DeviceTest.SetLinearSpecularTextureState(
            2,
            NextResult,
            NextResult,
            (_, _, _) => NextResult(),
            (_, _) => NextResult(),
            (_, _, _) => NextResult());

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, failingCall), (result, callCount));
    }

    [TestMethod]
    public void WhenTriLinearSpecularTextureStateIsSetThenCallsMatchNativeOrder()
    {
        List<string> actualCalls = [];

        int result = Direct3D9Level1DeviceTest.SetTriLinearSpecularTextureState(
            2,
            () =>
            {
                actualCalls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                actualCalls.Add("VertexShader:null");
                return 0;
            },
            (sampler, state, value) =>
            {
                actualCalls.Add($"SamplerState:{sampler}:{state}:{value}");
                return 0;
            },
            (state, value) =>
            {
                actualCalls.Add($"RenderState:{state}:{value}");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) Texturefiltertype.Linear}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TASpecular}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    public void WhenTriLinearSpecularTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        int callCount = 0;
        int NextResult() => ++callCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;

        int result = Direct3D9Level1DeviceTest.SetTriLinearSpecularTextureState(
            2,
            NextResult,
            NextResult,
            (_, _, _) => NextResult(),
            (_, _) => NextResult(),
            (_, _, _) => NextResult());

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, failingCall), (result, callCount));
    }

    [TestMethod]
    public void WhenNearestNeighborMaskedTextureStateIsSetThenCallsMatchNativeOrder()
    {
        List<string> actualCalls = [];

        int result = Direct3D9Level1DeviceTest.SetNearestNeighborMaskedTextureState(
            3,
            () =>
            {
                actualCalls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                actualCalls.Add("VertexShader:null");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"SamplerState:{stage}:{state}:{value}");
                return 0;
            },
            (state, value) =>
            {
                actualCalls.Add($"RenderState:{state}:{value}");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Point}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Point}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) Texturefiltertype.None}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"SamplerState:1:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Point}",
                $"SamplerState:1:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Point}",
                $"SamplerState:1:{Samplerstatetype.Mipfilter}:{(uint) Texturefiltertype.None}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:1:{Texturestagestatetype.Colorarg1}:{(uint) (D3D9.TATexture | D3D9.TAAlphareplicate)}",
                $"TextureStageState:1:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:1:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:1:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:1:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:1:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:2:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    [DataRow(17)]
    [DataRow(18)]
    [DataRow(19)]
    [DataRow(20)]
    [DataRow(21)]
    [DataRow(22)]
    [DataRow(23)]
    [DataRow(24)]
    [DataRow(25)]
    [DataRow(26)]
    public void WhenNearestNeighborMaskedTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        int callCount = 0;
        int NextResult() => ++callCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;

        int result = Direct3D9Level1DeviceTest.SetNearestNeighborMaskedTextureState(
            3,
            NextResult,
            NextResult,
            (_, _, _) => NextResult(),
            (_, _) => NextResult(),
            (_, _, _) => NextResult());

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, failingCall), (result, callCount));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenFilteredMaskedTextureStateIsSetThenCallsMatchNativeOrder(bool triLinear)
    {
        List<string> actualCalls = [];
        Texturefiltertype mipFilter = triLinear ? Texturefiltertype.Linear : Texturefiltertype.None;
        Func<uint, Func<int>, Func<int>, Func<uint, Samplerstatetype, uint, int>, Func<Renderstatetype, uint, int>, Func<uint, Texturestagestatetype, uint, int>, int> setState =
            triLinear
                ? Direct3D9Level1DeviceTest.SetTriLinearMaskedTextureState
                : Direct3D9Level1DeviceTest.SetLinearMaskedTextureState;

        int result = setState(
            3,
            () =>
            {
                actualCalls.Add("PixelShader:null");
                return 0;
            },
            () =>
            {
                actualCalls.Add("VertexShader:null");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"SamplerState:{stage}:{state}:{value}");
                return 0;
            },
            (state, value) =>
            {
                actualCalls.Add($"RenderState:{state}:{value}");
                return 0;
            },
            (stage, state, value) =>
            {
                actualCalls.Add($"TextureStageState:{stage}:{state}:{value}");
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) mipFilter}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"SamplerState:1:{Samplerstatetype.Magfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:1:{Samplerstatetype.Minfilter}:{(uint) Texturefiltertype.Linear}",
                $"SamplerState:1:{Samplerstatetype.Mipfilter}:{(uint) mipFilter}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:1:{Texturestagestatetype.Colorarg1}:{(uint) (D3D9.TATexture | D3D9.TAAlphareplicate)}",
                $"TextureStageState:1:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:1:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:1:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:1:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:1:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:2:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    [DataRow(17)]
    [DataRow(18)]
    [DataRow(19)]
    [DataRow(20)]
    [DataRow(21)]
    [DataRow(22)]
    [DataRow(23)]
    [DataRow(24)]
    [DataRow(25)]
    [DataRow(26)]
    public void WhenFilteredMaskedTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        int linearCallCount = 0;
        int triLinearCallCount = 0;
        int NextLinearResult() => ++linearCallCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;
        int NextTriLinearResult() => ++triLinearCallCount == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;

        int linearResult = Direct3D9Level1DeviceTest.SetLinearMaskedTextureState(
            3,
            NextLinearResult,
            NextLinearResult,
            (_, _, _) => NextLinearResult(),
            (_, _) => NextLinearResult(),
            (_, _, _) => NextLinearResult());
        int triLinearResult = Direct3D9Level1DeviceTest.SetTriLinearMaskedTextureState(
            3,
            NextTriLinearResult,
            NextTriLinearResult,
            (_, _, _) => NextTriLinearResult(),
            (_, _) => NextTriLinearResult(),
            (_, _, _) => NextTriLinearResult());

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, failingCall, Direct3D9Factory.InvalidCallHResult, failingCall),
            (linearResult, linearCallCount, triLinearResult, triLinearCallCount));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    public void WhenDiffuseMaskedTextureStateIsSetThenCallsMatchNativeOrder(int filterMode)
    {
        List<string> actualCalls = [];
        Texturefiltertype minMagFilter = filterMode == 0 ? Texturefiltertype.Point : Texturefiltertype.Linear;
        Texturefiltertype mipFilter = filterMode == 2 ? Texturefiltertype.Linear : Texturefiltertype.None;
        Func<uint, Func<int>, Func<int>, Func<uint, Samplerstatetype, uint, int>, Func<Renderstatetype, uint, int>, Func<uint, Texturestagestatetype, uint, int>, int> setState = filterMode switch
        {
            0 => Direct3D9Level1DeviceTest.SetNearestNeighborDiffuseMaskedTextureState,
            1 => Direct3D9Level1DeviceTest.SetLinearDiffuseMaskedTextureState,
            _ => Direct3D9Level1DeviceTest.SetTriLinearDiffuseMaskedTextureState
        };

        int result = setState(
            3,
            () => { actualCalls.Add("PixelShader:null"); return 0; },
            () => { actualCalls.Add("VertexShader:null"); return 0; },
            (stage, state, value) => { actualCalls.Add($"SamplerState:{stage}:{state}:{value}"); return 0; },
            (state, value) => { actualCalls.Add($"RenderState:{state}:{value}"); return 0; },
            (stage, state, value) => { actualCalls.Add($"TextureStageState:{stage}:{state}:{value}"); return 0; });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) minMagFilter}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) minMagFilter}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) mipFilter}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"SamplerState:1:{Samplerstatetype.Magfilter}:{(uint) minMagFilter}",
                $"SamplerState:1:{Samplerstatetype.Minfilter}:{(uint) minMagFilter}",
                $"SamplerState:1:{Samplerstatetype.Mipfilter}:{(uint) mipFilter}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:1:{Texturestagestatetype.Colorarg1}:{(uint) (D3D9.TATexture | D3D9.TAAlphareplicate)}",
                $"TextureStageState:1:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:1:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:1:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:1:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:1:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:2:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    public void WhenSpecularMaskedTextureStateIsSetThenCallsMatchNativeOrder(int filterMode)
    {
        List<string> actualCalls = [];
        Texturefiltertype minMagFilter = filterMode == 0 ? Texturefiltertype.Point : Texturefiltertype.Linear;
        Texturefiltertype mipFilter = filterMode == 2 ? Texturefiltertype.Linear : Texturefiltertype.None;
        Func<uint, Func<int>, Func<int>, Func<uint, Samplerstatetype, uint, int>, Func<Renderstatetype, uint, int>, Func<uint, Texturestagestatetype, uint, int>, int> setState = filterMode switch
        {
            0 => Direct3D9Level1DeviceTest.SetNearestNeighborSpecularMaskedTextureState,
            1 => Direct3D9Level1DeviceTest.SetLinearSpecularMaskedTextureState,
            _ => Direct3D9Level1DeviceTest.SetTriLinearSpecularMaskedTextureState
        };

        int result = setState(
            3,
            () => { actualCalls.Add("PixelShader:null"); return 0; },
            () => { actualCalls.Add("VertexShader:null"); return 0; },
            (stage, state, value) => { actualCalls.Add($"SamplerState:{stage}:{state}:{value}"); return 0; },
            (state, value) => { actualCalls.Add($"RenderState:{state}:{value}"); return 0; },
            (stage, state, value) => { actualCalls.Add($"TextureStageState:{stage}:{state}:{value}"); return 0; });

        CollectionAssert.AreEqual(
            new[]
            {
                "PixelShader:null",
                "VertexShader:null",
                $"SamplerState:0:{Samplerstatetype.Magfilter}:{(uint) minMagFilter}",
                $"SamplerState:0:{Samplerstatetype.Minfilter}:{(uint) minMagFilter}",
                $"SamplerState:0:{Samplerstatetype.Mipfilter}:{(uint) mipFilter}",
                $"RenderState:{Renderstatetype.Alphablendenable}:1",
                $"RenderState:{Renderstatetype.Srcblend}:{(uint) Blend.One}",
                $"RenderState:{Renderstatetype.Destblend}:{(uint) Blend.Invsrcalpha}",
                $"TextureStageState:0:{Texturestagestatetype.Colorop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TASpecular}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Selectarg1}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:0:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:0:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"SamplerState:1:{Samplerstatetype.Magfilter}:{(uint) minMagFilter}",
                $"SamplerState:1:{Samplerstatetype.Minfilter}:{(uint) minMagFilter}",
                $"SamplerState:1:{Samplerstatetype.Mipfilter}:{(uint) mipFilter}",
                $"TextureStageState:1:{Texturestagestatetype.Colorop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:1:{Texturestagestatetype.Colorarg1}:{(uint) (D3D9.TATexture | D3D9.TAAlphareplicate)}",
                $"TextureStageState:1:{Texturestagestatetype.Colorarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:1:{Texturestagestatetype.Alphaop}:{(uint) Textureop.Modulate}",
                $"TextureStageState:1:{Texturestagestatetype.Alphaarg1}:{(uint) D3D9.TATexture}",
                $"TextureStageState:1:{Texturestagestatetype.Alphaarg2}:{(uint) D3D9.TACurrent}",
                $"TextureStageState:1:{Texturestagestatetype.Texturetransformflags}:{(uint) Texturetransformflags.Disable}",
                $"TextureStageState:2:{Texturestagestatetype.Colorop}:{(uint) Textureop.Disable}"
            },
            actualCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    [DataRow(17)]
    [DataRow(18)]
    [DataRow(19)]
    [DataRow(20)]
    [DataRow(21)]
    [DataRow(22)]
    [DataRow(23)]
    [DataRow(24)]
    [DataRow(25)]
    [DataRow(26)]
    public void WhenSpecularMaskedTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        static int Invoke(
            int failingCall,
            Func<uint, Func<int>, Func<int>, Func<uint, Samplerstatetype, uint, int>, Func<Renderstatetype, uint, int>, Func<uint, Texturestagestatetype, uint, int>, int> setState,
            out int callCount)
        {
            int count = 0;
            int NextResult() => ++count == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;
            int result = setState(3, NextResult, NextResult, (_, _, _) => NextResult(), (_, _) => NextResult(), (_, _, _) => NextResult());
            callCount = count;
            return result;
        }

        int nearestResult = Invoke(failingCall, Direct3D9Level1DeviceTest.SetNearestNeighborSpecularMaskedTextureState, out int nearestCalls);
        int linearResult = Invoke(failingCall, Direct3D9Level1DeviceTest.SetLinearSpecularMaskedTextureState, out int linearCalls);
        int triLinearResult = Invoke(failingCall, Direct3D9Level1DeviceTest.SetTriLinearSpecularMaskedTextureState, out int triLinearCalls);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, failingCall, Direct3D9Factory.InvalidCallHResult, failingCall, Direct3D9Factory.InvalidCallHResult, failingCall),
            (nearestResult, nearestCalls, linearResult, linearCalls, triLinearResult, triLinearCalls));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [DataRow(12)]
    [DataRow(13)]
    [DataRow(14)]
    [DataRow(15)]
    [DataRow(16)]
    [DataRow(17)]
    [DataRow(18)]
    [DataRow(19)]
    [DataRow(20)]
    [DataRow(21)]
    [DataRow(22)]
    [DataRow(23)]
    [DataRow(24)]
    [DataRow(25)]
    [DataRow(26)]
    public void WhenDiffuseMaskedTextureStateCallFailsThenOriginalHResultIsReturned(int failingCall)
    {
        static int Invoke(
            int failingCall,
            Func<uint, Func<int>, Func<int>, Func<uint, Samplerstatetype, uint, int>, Func<Renderstatetype, uint, int>, Func<uint, Texturestagestatetype, uint, int>, int> setState,
            out int callCount)
        {
            int count = 0;
            int NextResult() => ++count == failingCall ? Direct3D9Factory.InvalidCallHResult : 0;
            int result = setState(3, NextResult, NextResult, (_, _, _) => NextResult(), (_, _) => NextResult(), (_, _, _) => NextResult());
            callCount = count;
            return result;
        }

        int nearestResult = Invoke(failingCall, Direct3D9Level1DeviceTest.SetNearestNeighborDiffuseMaskedTextureState, out int nearestCalls);
        int linearResult = Invoke(failingCall, Direct3D9Level1DeviceTest.SetLinearDiffuseMaskedTextureState, out int linearCalls);
        int triLinearResult = Invoke(failingCall, Direct3D9Level1DeviceTest.SetTriLinearDiffuseMaskedTextureState, out int triLinearCalls);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, failingCall, Direct3D9Factory.InvalidCallHResult, failingCall, Direct3D9Factory.InvalidCallHResult, failingCall),
            (nearestResult, nearestCalls, linearResult, linearCalls, triLinearResult, triLinearCalls));
    }

    [TestMethod]
    public void WhenTextureStatesAreTestedThenNoneModesPrecedeDiffuseAndSpecularModes()
    {
        List<uint> samplerValues = [];
        List<uint> colorOperations = [];
        List<uint> colorArguments2 = [];

        int result = Direct3D9Level1DeviceTest.Test(
            CreateSupportedCapabilities(),
            (_, _) => 0,
            () => 0,
            () => 0,
            (_, state, value) =>
            {
                if (state == Texturestagestatetype.Colorop && value != (uint) Textureop.Disable)
                {
                    colorOperations.Add(value);
                }
                else if (state == Texturestagestatetype.Colorarg2)
                {
                    colorArguments2.Add(value);
                }

                return 0;
            },
            _ => 0,
            (_, _, value) =>
            {
                samplerValues.Add(value);
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.Point,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.None,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear,
                (uint) Texturefiltertype.Linear
            },
            samplerValues);
        CollectionAssert.AreEqual(
            new[]
            {
                (uint) Textureop.Selectarg1,
                (uint) Textureop.Selectarg1,
                (uint) Textureop.Selectarg1,
                (uint) Textureop.Selectarg1,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Selectarg1,
                (uint) Textureop.Selectarg1,
                (uint) Textureop.Modulate,
                (uint) Textureop.Selectarg1,
                (uint) Textureop.Modulate,
                (uint) Textureop.Selectarg1,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate,
                (uint) Textureop.Modulate
            },
            colorOperations);
        CollectionAssert.AreEqual(
            new[]
            {
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TASpecular,
                (uint) D3D9.TASpecular,
                (uint) D3D9.TASpecular,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TASpecular,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TASpecular,
                (uint) D3D9.TACurrent,
                (uint) D3D9.TASpecular,
                (uint) D3D9.TACurrent
            },
            colorArguments2);
        Assert.AreEqual(0, result);
    }

    private static Caps9 CreateSupportedCapabilities()
    {
        return new Caps9(
            deviceType: Devtype.Hal,
            primitiveMiscCaps: D3D9.PmisccapsColorwriteenable,
            srcBlendCaps: D3D9.PblendcapsZero
                | D3D9.PblendcapsOne
                | D3D9.PblendcapsSrcalpha
                | D3D9.PblendcapsInvdestalpha,
            destBlendCaps: D3D9.PblendcapsZero
                | D3D9.PblendcapsOne
                | D3D9.PblendcapsInvsrccolor
                | D3D9.PblendcapsInvsrcalpha,
            maxTextureBlendStages: 2,
            maxSimultaneousTextures: 2);
    }
}
