using System.Numerics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class BitmapRenderTargetTests
{
    [TestMethod]
    [DataRow(0u, 0f, 1u)]
    [DataRow(1u, 0f, 1u)]
    [DataRow(0u, 0.5f, 1u)]
    [DataRow(1u, 0.5f, 1u)]
    [DataRow(0u, 0.5f, 0u)]
    [DataRow(1u, 0.5f, 0u)]
    public void WhenInternalDrawUsesFloatTargetThenHdrAndFractionalPrecisionSurvive(uint compositing, float offset, uint interpolation)
    {
        var api = new Api();
        nint factory = 0, target = 0, sourceTarget = 0, source = 0, sourceBitmap = 0, output = 0, view = 0, bitmapLock = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 1, 1, 26, 96, 96, 1, &target));
            Assert.AreEqual(0, api.CreateTarget(factory, 1, 1, 26, 96, 96, 1, &sourceTarget));
            Assert.AreEqual(0, api.GetBitmap(target, &output));
            Assert.AreEqual(0, api.GetBitmap(sourceTarget, &sourceBitmap));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)sourceTarget)[7])(sourceTarget, &source));
            WriteFloat(sourceBitmap, new(1.25f, -0.25f, 0.1234567f, 0.5f));
            WriteFloat(output, new(0.25f, 0.5f, 0.125f, 1));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            byte* context = stackalloc byte[2048]; byte* state = stackalloc byte[116];
            new Span<byte>(context, 2048).Clear(); new Span<byte>(state, 116).Clear();
            *(int*)(context + InternalContextLayout.Clip) = 1; *(nint*)(context + InternalContextLayout.RenderState) = (nint)state;
            *(Matrix4x4*)(context + InternalContextLayout.WorldToDevice) = Matrix4x4.CreateTranslation(offset, 0, 0);
            *(uint*)(state + 84) = interpolation; *(uint*)(state + 96) = 1; *(uint*)(state + 100) = compositing;
            var draw = (delegate* unmanaged[Stdcall]<nint, nint, nint, nint, int>)(*(void***)view)[8];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)output)[8])(output, 0, 2, &bitmapLock));
            Assert.AreEqual(unchecked((int)0x88982F0D), draw(view, (nint)context, source, 0));
            Release(bitmapLock); bitmapLock = 0;
            Assert.AreEqual(0, draw(view, (nint)context, source, 0));
            Vector4 pixel;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, 16, 16, (byte*)&pixel));
            Vector4 sample = new Vector4(1.25f, -0.25f, 0.1234567f, 0.5f) * (1 - offset);
            Vector4 expected = compositing == 0 ? sample + new Vector4(0.25f, 0.5f, 0.125f, 1) * (1 - sample.W) : sample;
            Assert.IsTrue(Vector4.Distance(expected, pixel) < 0.0000001f, $"Expected {expected}, actual {pixel}");
        }
        finally { Release(bitmapLock); Release(view); Release(output); Release(sourceBitmap); Release(source); Release(sourceTarget); Release(target); Release(factory); }
    }

    [TestMethod]
    [DataRow(0u, 4, 1u)]
    [DataRow(1u, 4, 1u)]
    [DataRow(0u, 8, 1u)]
    [DataRow(1u, 8, 1u)]
    [DataRow(0u, 16, 1u)]
    [DataRow(1u, 16, 1u)]
    [DataRow(0u, 2048, 1u)]
    [DataRow(1u, 2048, 1u)]
    [DataRow(0u, 6, 1u)]
    [DataRow(1u, 6, 1u)]
    [DataRow(0u, 10, 1u)]
    [DataRow(1u, 10, 1u)]
    [DataRow(0u, 2046, 1u)]
    [DataRow(1u, 2046, 1u)]
    [DataRow(0u, 4, 0u)]
    [DataRow(1u, 4, 0u)]
    [DataRow(0u, 8, 0u)]
    [DataRow(1u, 8, 0u)]
    [DataRow(0u, 16, 0u)]
    [DataRow(1u, 16, 0u)]
    [DataRow(0u, 2048, 0u)]
    [DataRow(1u, 2048, 0u)]
    [DataRow(0u, 6, 0u)]
    [DataRow(1u, 6, 0u)]
    [DataRow(0u, 10, 0u)]
    [DataRow(1u, 10, 0u)]
    [DataRow(0u, 2046, 0u)]
    [DataRow(1u, 2046, 0u)]
    public void WhenFloatSpanStretchesHdrGradientThenSinglePrecisionIpcCoordinatesAdvanceAcrossSurface(uint compositing, int width, uint interpolation)
    {
        var api = new Api();
        nint factory = 0, target = 0, sourceTarget = 0, source = 0, sourceBitmap = 0, output = 0, view = 0, bitmapLock = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, (uint)width, 1, 26, 96, 96, 1, &target));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, 26, 96, 96, 1, &sourceTarget));
            Assert.AreEqual(0, api.GetBitmap(target, &output));
            Assert.AreEqual(0, api.GetBitmap(sourceTarget, &sourceBitmap));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)sourceTarget)[7])(sourceTarget, &source));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)sourceBitmap)[8])(sourceBitmap, 0, 2, &bitmapLock));
            uint size; byte* pixels;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels));
            Vector4 first = new(1.25f, -0.25f, 0.1234567f, 1);
            Vector4 second = new(2.5f, -0.5f, 0.7654321f, 1);
            ((Vector4*)pixels)[0] = first; ((Vector4*)pixels)[1] = second;
            Release(bitmapLock); bitmapLock = 0;
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            byte* context = stackalloc byte[2048]; byte* state = stackalloc byte[116];
            new Span<byte>(context, 2048).Clear(); new Span<byte>(state, 116).Clear();
            *(int*)(context + InternalContextLayout.Clip) = 1; *(nint*)(context + InternalContextLayout.RenderState) = (nint)state;
            *(Matrix4x4*)(context + InternalContextLayout.WorldToDevice) = Matrix4x4.CreateScale(width / 2f, 1, 1);
            *(uint*)(state + 84) = interpolation; *(uint*)(state + 96) = 1; *(uint*)(state + 100) = compositing;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, int>)(*(void***)view)[8])(view, (nint)context, source, 0));
            Vector4* actual = stackalloc Vector4[width];
            uint bytes = checked((uint)width * 16);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, bytes, bytes, (byte*)actual));
            // CBilinearSpan_scRGB transforms the span endpoints, then accumulates FLOAT increments.
            float inverseScale = (float)(1d / (width / 2f));
            float coordinate = inverseScale * 0.5f - 0.5f;
            float end = width * inverseScale + coordinate;
            float increment = (end - coordinate) / width;
            for (int i = 0; i < width; i++, coordinate += increment)
            {
                float fraction = interpolation == 0 ? Math.Clamp(MathF.Floor(coordinate + 0.5f), 0, 1) : Math.Clamp(coordinate, 0, 1);
                Vector4 expected = first + (second - first) * fraction;
                Assert.IsTrue(Vector4.Distance(expected, actual[i]) < 0.0000001f, $"Pixel {i}: expected {expected}, actual {actual[i]}");
            }
        }
        finally { Release(bitmapLock); Release(view); Release(output); Release(sourceBitmap); Release(source); Release(sourceTarget); Release(target); Release(factory); }
    }

    private static void WriteFloat(nint bitmap, Vector4 value)
    {
        nint bitmapLock = 0;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock));
            uint size; byte* pixels;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels));
            *(Vector4*)pixels = value;
        }
        finally { Release(bitmapLock); }
    }
}
