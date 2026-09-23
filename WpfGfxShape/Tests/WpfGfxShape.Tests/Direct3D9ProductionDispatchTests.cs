using System.Numerics;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9ProductionDispatchTests
{
    [TestMethod]
    public unsafe void WhenTextureProductionPrimitivesDispatchThenContentsAreInvalidatedAndSurfaceEntrypointsRun()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget surfaceTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget textureTarget = new(device, surfaceTarget, texture: null);
        int bitmapCalls = 0;
        int pathCalls = 0;
        int glyphCalls = 0;
        int videoCalls = 0;

        int bitmapResult = textureTarget.ProductionDrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity),
            1,
            0,
            CreateBitmapOperations(() => bitmapCalls++));
        int pathResult = textureTarget.ProductionDrawPath(
            Matrix4x4.Identity,
            1,
            0,
            0,
            0,
            CreatePathOperations(() => pathCalls++));
        int glyphResult = textureTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(true, true),
            CreateGlyphOperations(() => glyphCalls++, hasGlyphs: false));
        int videoResult = textureTarget.ProductionDrawVideo(
            new Direct3D9VideoRenderState(),
            null,
            0,
            new Direct3D9ProductionVideoDrawOperations(
                new Direct3D9BitmapDrawState(Matrix4x4.Identity),
                0,
                CreateBitmapOperations(() => videoCalls++)));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0, 0, 1, 0, 0, 0),
            (bitmapResult, pathResult, glyphResult, videoResult, bitmapCalls, pathCalls, glyphCalls, videoCalls));
    }

    [TestMethod]
    public unsafe void WhenTextureProductionDispatchIsDisposedThenDependenciesAreSkipped()
    {
        using Direct3D9Device device = CreateDevice();
        Direct3D9TextureRenderTarget textureTarget = new(
            device,
            new Direct3D9SurfaceRenderTarget(device, MultisampleType.MultisampleNone),
            texture: null);
        int calls = 0;
        textureTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => textureTarget.ProductionDrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity),
            1,
            0,
            CreateBitmapOperations(() => calls++)));
        Assert.AreEqual(0, calls);
    }

    private static Direct3D9ProductionBitmapDrawOperations CreateBitmapOperations(Action call) => new(
        (out nint brush) => { call(); brush = 0; return Direct3D9Factory.GenericFailureHResult; },
        static (_, _, _) => { },
        static _ => { },
        static _ => null!,
        static (MilRectF _, out nint shape) => { shape = 0; return 0; },
        static () => 0,
        static (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clipped) => { clipped = default; return 0; },
        static (_, _, _, _) => 0,
        static (Direct3D9PathClipperState state, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
        static (Direct3D9PathClipperState state, nint _, Matrix4x4 _, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
        static (Direct3D9PathClipperState state, out MilRectF bounds) => { bounds = state.Bounds; return 0; },
        static (nint _, Direct3D9PathBrushContext _, out Direct3D9PathHardwareBrush? brush) => { brush = null; return 0; },
        static (Direct3D9PathClipperState _, out Direct3D9PathGeometryGenerator? geometry) => { geometry = null; return 0; },
        static (Direct3D9PathClipperState _, out Direct3D9PathGeometryGenerator? geometry) => { geometry = null; return 0; },
        MilCompositingMode.SourceOver,
        MilAntiAliasMode.None,
        default);

    private static Direct3D9ProductionPathDrawOperations CreatePathOperations(Action call) => new(
        _ => { call(); return Direct3D9Factory.GenericFailureHResult; },
        static (nint _, out MilRectF bounds) => { bounds = default; return 0; },
        static (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint shape) => { shape = 0; return 0; },
        static (nint _, bool _, out nint brush, out nint effects) => { brush = 0; effects = 0; return 0; },
        static (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clipped) => { clipped = default; return 0; },
        static () => 0,
        static (_, _, _, _) => 0,
        static (Direct3D9PathClipperState state, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
        static (Direct3D9PathClipperState state, nint _, Matrix4x4 _, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
        static (Direct3D9PathClipperState state, out MilRectF bounds) => { bounds = state.Bounds; return 0; },
        static (nint _, Direct3D9PathBrushContext _, out Direct3D9PathHardwareBrush? brush) => { brush = null; return 0; },
        static (Direct3D9PathClipperState _, out Direct3D9PathGeometryGenerator? geometry) => { geometry = null; return 0; },
        static (Direct3D9PathClipperState _, out Direct3D9PathGeometryGenerator? geometry) => { geometry = null; return 0; },
        MilCompositingMode.SourceOver,
        MilAntiAliasMode.None,
        default);

    private static Direct3D9ProductionGlyphDrawOperations CreateGlyphOperations(Action call, bool hasGlyphs) => new(
        (out Direct3D9RealizedGlyphBrushState state) => { call(); state = default; return 0; },
        static () => 0,
        static (bool _, out Direct3D9ProductionGlyphRenderer? renderer) => { renderer = null; return 0; },
        new Direct3D9SoftwareGlyphRenderer(
            static (out Direct3D9RealizedSoftwareGlyphBrushState state) => { state = default; return 0; },
            static _ => 0,
            static (_, _) => 0),
        hasGlyphs);

    private static unsafe Direct3D9Device CreateDevice() => new(
        null,
        null,
        0,
        Devtype.Hal,
        0,
        new PresentParameters(windowed: true));
}