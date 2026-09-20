using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceRenderTextureTests
{
    [TestMethod]
    public void WhenMaterialStateReturnsDriverInternalErrorThenFailureIsMappedAndRemainingSetupIsSkipped()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = new(new Direct3D9ResourceManager(), textureObject.Texture, 128, 128);
        int renderStateCallCount = 0;
        int flexibleVertexFormatCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            setRenderState: (_, _) =>
            {
                renderStateCallCount++;
                return Direct3D9Factory.DriverInternalErrorHResult;
            },
            setFlexibleVertexFormat: _ =>
            {
                flexibleVertexFormatCallCount++;
                return 0;
            });

        int result = device.RenderTexture(texture);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1, 0),
            (result, device.UnusableReasonHResult, renderStateCallCount, flexibleVertexFormatCallCount));
    }

    [TestMethod]
    public void WhenFlexibleVertexFormatReturnsDriverInternalErrorThenFailureIsMapped()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = new(new Direct3D9ResourceManager(), textureObject.Texture, 128, 128);
        int flexibleVertexFormatCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            setRenderState: (_, _) => 0,
            setFlexibleVertexFormat: _ =>
            {
                flexibleVertexFormatCallCount++;
                return Direct3D9Factory.DriverInternalErrorHResult;
            });

        int result = device.RenderTexture(texture);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1),
            (result, device.UnusableReasonHResult, flexibleVertexFormatCallCount));
    }

    [TestMethod]
    public void WhenRenderingNonOriginSubTextureThenDuv2TriangleFanUsesDestinationAndTextureDimensions()
    {
        const int expectedResult = 1;
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = new(new Direct3D9ResourceManager(), textureObject.Texture, 320, 240);
        List<string> calls = [];
        Direct3D9VertexXyzDiffuseUv2[] drawnVertices = [];
        using Direct3D9Device device = CreateDevice(
            setRenderState: (state, _) =>
            {
                calls.Add($"render:{state}");
                return 0;
            },
            setFlexibleVertexFormat: _ =>
            {
                calls.Add("fvf");
                return 0;
            },
            setTexture: (_, _) =>
            {
                calls.Add("texture");
                return 0;
            },
            drawPrimitiveUp: (primitiveType, primitiveCount, data, stride) =>
            {
                calls.Add("draw");
                Assert.AreEqual((Primitivetype.Trianglefan, 2u, (uint) sizeof(Direct3D9VertexXyzDiffuseUv2)),
                    (primitiveType, primitiveCount, stride));
                drawnVertices = new ReadOnlySpan<Direct3D9VertexXyzDiffuseUv2>(data, 4).ToArray();
                return expectedResult;
            });

        int result = device.RenderTexture(texture, new Direct3D9PointAndSizeRect(17, 23, 80, 60));

        Assert.AreEqual(
            (expectedResult, "texture", Renderstatetype.Diffusematerialsource, Renderstatetype.Specularmaterialsource,
                "fvf", "draw", 4, 17f, 23f, 97f, 83f, 0.25f, 0.25f, uint.MaxValue, 0f, 0f),
            (result, calls[0], Enum.Parse<Renderstatetype>(calls[1][7..]), Enum.Parse<Renderstatetype>(calls[2][7..]),
                calls[^2], calls[^1], drawnVertices.Length, drawnVertices[0].X, drawnVertices[0].Y,
                drawnVertices[2].X, drawnVertices[2].Y, drawnVertices[2].U0, drawnVertices[2].V0,
                drawnVertices[3].Diffuse, drawnVertices[3].U1, drawnVertices[3].V1));
    }

    [TestMethod]
    [DataRow(0, 1u, Blend.One, Blend.Invsrcalpha)]
    [DataRow(1, 0u, Blend.One, Blend.Zero)]
    [DataRow(2, 1u, Blend.Zero, Blend.Invsrccolor)]
    [DataRow(3, 1u, Blend.One, Blend.One)]
    public void WhenBlendModeIsSelectedThenNativeAlphaBlendStateIsUsed(
        int blendMode,
        uint expectedAlphaBlendEnable,
        Blend expectedSourceBlend,
        Blend expectedDestinationBlend)
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = new(new Direct3D9ResourceManager(), textureObject.Texture, 128, 128);
        List<(Renderstatetype State, uint Value)> blendStates = [];
        using Direct3D9Device device = CreateDevice(
            setRenderState: (state, value) =>
            {
                if (state is Renderstatetype.Alphablendenable or Renderstatetype.Srcblend or Renderstatetype.Destblend)
                {
                    blendStates.Add((state, value));
                }

                return 0;
            },
            setFlexibleVertexFormat: _ => 0,
            drawPrimitiveUp: (_, _, _, _) => 0);

        int result = device.RenderTexture(
            texture,
            new Direct3D9PointAndSizeRect(0, 0, 128, 128),
            (Direct3D9TextureBlendMode) blendMode);

        CollectionAssert.AreEqual(
            new[]
            {
                (Renderstatetype.Alphablendenable, expectedAlphaBlendEnable),
                (Renderstatetype.Srcblend, (uint) expectedSourceBlend),
                (Renderstatetype.Destblend, (uint) expectedDestinationBlend)
            },
            blendStates);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenTexturePairUsesVectorAlphaThenAuxiliaryAndMainPassesUseNativeOrder()
    {
        using FakeTextureObject mainObject = new();
        using FakeTextureObject auxiliaryObject = new();
        using Direct3D9Texture mainTexture = new(new Direct3D9ResourceManager(), mainObject.Texture, 128, 128);
        using Direct3D9Texture auxiliaryTexture = new(new Direct3D9ResourceManager(), auxiliaryObject.Texture, 128, 128);
        Direct3D9LockableTexturePair pair = new(mainTexture);
        pair.InitializeAuxiliaryTexture(auxiliaryTexture);
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(
            setRenderState: (state, value) =>
            {
                if (state == Renderstatetype.Srcblend)
                {
                    calls.Add($"SrcBlend:{(Blend) value}");
                }

                return 0;
            },
            setFlexibleVertexFormat: _ => 0,
            setTexture: (_, texture) =>
            {
                calls.Add(texture == auxiliaryObject.Texture ? "Texture:Auxiliary" : "Texture:Main");
                return 0;
            },
            drawPrimitiveUp: (_, _, _, _) => 0);

        int result = pair.Draw(device, new Direct3D9PointAndSizeRect(0, 0, 128, 128), useAuxiliary: true);

        CollectionAssert.AreEqual(
            new[] { "Texture:Auxiliary", "SrcBlend:Zero", "Texture:Main", "SrcBlend:One" },
            calls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenFirstVectorAlphaPassFailsThenMainPassIsSkipped()
    {
        using FakeTextureObject mainObject = new();
        using FakeTextureObject auxiliaryObject = new();
        using Direct3D9Texture mainTexture = new(new Direct3D9ResourceManager(), mainObject.Texture, 128, 128);
        using Direct3D9Texture auxiliaryTexture = new(new Direct3D9ResourceManager(), auxiliaryObject.Texture, 128, 128);
        Direct3D9LockableTexturePair pair = new(mainTexture);
        pair.InitializeAuxiliaryTexture(auxiliaryTexture);
        int textureCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            setRenderState: (state, _) => state == Renderstatetype.Alphablendenable
                ? Direct3D9Factory.InvalidCallHResult
                : 0,
            setFlexibleVertexFormat: _ => 0,
            setTexture: (_, _) =>
            {
                textureCallCount++;
                return 0;
            });

        int result = pair.Draw(device, new Direct3D9PointAndSizeRect(0, 0, 128, 128), useAuxiliary: true);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 1), (result, textureCallCount));
    }

    [TestMethod]
    public void WhenTextureIsDisposedThenRenderingIsRejectedBeforeDeviceCalls()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9Texture texture = new(new Direct3D9ResourceManager(), textureObject.Texture, 128, 128);
        int deviceCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            setRenderState: (_, _) =>
            {
                deviceCallCount++;
                return 0;
            },
            setFlexibleVertexFormat: _ => 0,
            setTexture: (_, _) =>
            {
                deviceCallCount++;
                return 0;
            });
        texture.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.RenderTexture(texture));
        Assert.AreEqual(0, deviceCallCount);
    }

    private static Direct3D9Device CreateDevice(
        Direct3D9SetRenderState setRenderState,
        Direct3D9SetFlexibleVertexFormat setFlexibleVertexFormat,
        Direct3D9SetTexture? setTexture = null,
        Direct3D9DrawPrimitiveUp? drawPrimitiveUp = null)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            capabilities: new Caps9 { MaxTextureBlendStages = 8, MaxPrimitiveCount = ushort.MaxValue },
            setTexture: setTexture ?? ((_, _) => 0),
            setRenderState: setRenderState,
            setPixelShader: _ => 0,
            setVertexShader: _ => 0,
            setSamplerState: (_, _, _) => 0,
            setTextureStageState: (_, _, _) => 0,
            setFlexibleVertexFormat: setFlexibleVertexFormat,
            setStreamSource: (_, _, _, _) => 0,
            drawPrimitiveUp: drawPrimitiveUp);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(IDirect3DTexture9* self) => 0;

    private struct FakeTextureObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DTexture9* Texture;

        public FakeTextureObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            Texture = (IDirect3DTexture9*) memory;
            void** vtable = memory + 1;
            Texture->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &Release;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Texture = null;
        }
    }
}
