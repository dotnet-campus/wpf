using System.Numerics;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class BitmapRenderTargetTests
{
    [UnmanagedCallersOnly]
    private static uint GetBitmapPathType(nint self) => 4;

    /// <summary>Consumes a Release/x64 bitmap brush boundary through the production private path and verifies mapping, clipping, opacity and retry.</summary>
    [TestMethod]
    [TestCategory("InternalTargetContract")]
    [DataRow(0u, false, 1f, 16u, 0u)]
    [DataRow(4u, false, 1f, 16u, 0u)]
    [DataRow(1u, false, 1f, 16u, 0u)]
    [DataRow(2u, false, 1f, 16u, 0u)]
    [DataRow(3u, false, 1f, 16u, 0u)]
    [DataRow(4u, true, 1f, 16u, 0u)]
    [DataRow(4u, true, 0.5f, 16u, 0u)]
    [DataRow(4u, false, 0f, 16u, 0u)]
    [DataRow(5u, false, 1f, 16u, 0u)]
    [DataRow(5u, true, 1f, 16u, 0u)]
    [DataRow(5u, true, 0.5f, 16u, 0u)]
    [DataRow(5u, true, 1f, 16u, 1u)]
    [DataRow(5u, false, 1f, 26u, 0u)]
    [DataRow(5u, true, 0.5f, 26u, 0u)]
    [DataRow(5u, true, 0.5f, 26u, 1u)]
    [DataRow(0u, false, 1f, 16u, 0u, true)]
    [DataRow(5u, true, 0.5f, 26u, 1u, true)]
    [DataRow(0u, false, 1f, 16u, 0u, false, true)]
    [DataRow(5u, true, 0.5f, 26u, 1u, true, true)]
    [DataRow(0u, false, 1f, 16u, 0u, false, false, true)]
    [DataRow(0u, false, 1f, 16u, 0u, true, false, true)]
    [DataRow(0u, false, 1f, 26u, 0u)]
    [DataRow(1u, false, 1f, 26u, 0u)]
    [DataRow(2u, false, 1f, 26u, 0u)]
    [DataRow(3u, false, 1f, 26u, 0u)]
    [DataRow(4u, false, 1f, 26u, 0u)]
    [DataRow(0u, true, 0.5f, 26u, 0u)]
    [DataRow(1u, true, 0.5f, 26u, 0u)]
    [DataRow(2u, true, 0.5f, 26u, 0u)]
    [DataRow(3u, true, 0.5f, 26u, 0u)]
    [DataRow(4u, true, 0.5f, 26u, 0u)]
    public void WhenPrivateBitmapBrushIsRealizedThenMappingClipAndOpacityReachPixels(uint wrap, bool worldSpace, float opacity, uint format, uint interpolation,
        bool reverseClip = false, bool emptyClip = false, bool finiteIntersection = false)
    {
        var api = new Api();
        nint factory = 0, sourceTarget = 0, source = 0, milSource = 0, sourceLock = 0;
        nint target = 0, view = 0, bitmap = 0, heldLock = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 1, 1, 16, 96, 96, 1, &sourceTarget));
            Assert.AreEqual(0, api.GetBitmap(sourceTarget, &source));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)source)[8])(source, 0, 2, &sourceLock));
            uint size; byte* data;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)sourceLock)[5])(sourceLock, &size, &data));
            *(uint*)data = 0xffff0000;
            Release(sourceLock); sourceLock = 0;
            Guid sourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)source)[0])(source, &sourceId, &milSource));
            Assert.AreEqual(0, api.CreateTarget(factory, 4, finiteIntersection ? 2u : 1u, format, 96, 96, 1, &target));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            Assert.AreEqual(0, api.GetBitmap(target, &bitmap));
            void** table = stackalloc void*[12];
            new Span<nint>(table, 12).Clear();
            table[6] = (delegate* unmanaged<PathRealizer*, uint, uint, nint, nint, nint, int>)&EnsurePathRealizer;
            table[9] = (delegate* unmanaged<PathRealizer*, byte, nint>)&GetPathBrush;
            table[10] = (delegate* unmanaged<PathRealizer*, nint*, int>)&GetPathEffects;
            void** brushTable = stackalloc void*[6];
            new Span<nint>(brushTable, 6).Clear();
            brushTable[3] = (delegate* unmanaged<nint, uint>)&GetBitmapPathType;
            byte* brush = stackalloc byte[240];
            new Span<byte>(brush, 240).Clear();
            *(void***)brush = brushTable;
            *(uint*)(brush + 76) = worldSpace ? 1u : 0u;
            *(Matrix4x4*)(brush + 80) = interpolation == 1
                ? Matrix4x4.CreateTranslation(0.5f, 0, 0) : Matrix4x4.Identity;
            // Source clip is a native CParallelogram, not a bounds rectangle.
            Vector2* points = (Vector2*)(brush + 160);
            float left = emptyClip ? 10 : worldSpace ? 0 : 1;
            points[0] = new(left, 0); points[1] = new(left + 2, 0);
            points[2] = new(left + 2, 1); points[3] = new(left, 1);
            if (finiteIntersection)
            {
                points[0] = new(0.2f, -0.3f); points[1] = new(2.6f, 0.4f);
                points[2] = new(2.1f, 1.8f); points[3] = new(-0.3f, 1.1f);
            }
            if (reverseClip) (points[1], points[3]) = (points[3], points[1]);
            *(int*)(brush + 200) = 1;
            *(nint*)(brush + 208) = milSource;
            *(uint*)(brush + 216) = wrap;
            // Native MilColorF is RGBA scRGB; use an opaque blue border.
            *(Vector4*)(brush + 220) = new(0, 0, 1, 1);
            *(float*)(brush + 236) = opacity;
            PathRealizer realizer = new() { Table = table, Brush = (nint)brush, Failure = 2 };
            byte* context = stackalloc byte[2048];
            byte* state = stackalloc byte[116];
            new Span<byte>(context, 2048).Clear(); new Span<byte>(state, 116).Clear();
            *(int*)(context + 376) = 1;
            *(nint*)(context + 400) = (nint)state;
            *(Matrix4x4*)(context + 408) = finiteIntersection ? Matrix4x4.Identity : Matrix4x4.CreateTranslation(1, 0, 0);
            void** shapeTable = stackalloc void*[7];
            new Span<nint>(shapeTable, 7).Clear();
            shapeTable[4] = (delegate* unmanaged<nint, uint>)&GetPathFigureCount;
            shapeTable[5] = (delegate* unmanaged<PathShape*, uint, nint>)&GetPathFigure;
            shapeTable[6] = (delegate* unmanaged<nint, uint>)&GetPathFillMode;
            void** figureTable = stackalloc void*[23];
            new Span<nint>(figureTable, 23).Clear();
            figureTable[1] = (delegate* unmanaged<nint, byte>)&NoPathFlag;
            figureTable[8] = (delegate* unmanaged<nint, byte>)&IsPathFillable;
            figureTable[15] = (delegate* unmanaged<PathFigure*, byte>)&SetFirstPathSegment;
            figureTable[16] = (delegate* unmanaged<PathFigure*, byte*, nint*, byte>)&GetPathSegment;
            figureTable[17] = (delegate* unmanaged<PathFigure*, byte>)&SetNextPathSegment;
            figureTable[22] = (delegate* unmanaged<PathFigure*, nint>)&GetPathStart;
            Vector2* targetPoints = stackalloc Vector2[] { new(0.6f, 0.2f), new(2.5f, 0.2f), new(2.5f, 1.3f), new(0.6f, 1.3f) };
            PathFigure figure = new() { Table = figureTable, Points = targetPoints };
            PathShape shape = new() { Table = shapeTable, Figure = &figure };
            nint shapePointer = finiteIntersection ? (nint)(&shape) : 0;
            *(uint*)(state + 84) = interpolation;
            *(uint*)(state + 96) = 1;
            float* bounds = stackalloc float[] { 0, 0, 4, finiteIntersection ? 2 : 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint, nint, float, nint, int>)(*(void***)view)[15])(view, (nint)bounds, 0, 0, 0, 1, 0));
            Assert.AreEqual(unchecked((int)0x80070005), DrawRealizedPath(view, (nint)context, (nint)(&realizer), finiteIntersection, shapePointer));
            realizer.Failure = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &heldLock));
            Assert.AreEqual(unchecked((int)0x88982F0D), DrawRealizedPath(view, (nint)context, (nint)(&realizer), finiteIntersection, shapePointer));
            Release(heldLock); heldLock = 0;
            Assert.AreEqual(0, DrawRealizedPath(view, (nint)context, (nint)(&realizer), finiteIntersection, shapePointer));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)view)[16])(view));
            if (finiteIntersection)
            {
                byte[] actual = new byte[32];
                fixed (byte* pixels = actual)
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 16, 32, pixels));
                using JsonDocument reference = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ReferenceData", "source-clip-intersection.json")));
                byte[] expected = reference.RootElement.GetProperty("PixelsBgra").EnumerateArray().Select(value => value.GetByte()).ToArray();
                CollectionAssert.AreEqual(expected, actual,
                    $"Original [{string.Join(',', expected)}]; AOT [{string.Join(',', actual)}]");
            }
            else if (format == 26)
            {
                Vector4* pixels = stackalloc Vector4[4];
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 64, 64, (byte*)pixels));
                Vector4 red = (interpolation == 1 ? new Vector4(0.5f, 0, 0.5f, 1) : new Vector4(1, 0, 0, 1)) * opacity;
                Vector4 blue = (interpolation == 1 ? new Vector4(0.5f, 0, 0.5f, 1) : new Vector4(0, 0, 1, 1)) * opacity;
                Assert.AreEqual(emptyClip ? (Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero)
                    : wrap == 5 ? (Vector4.Zero, worldSpace ? red : blue, blue, Vector4.Zero)
                    : (Vector4.Zero, red, red, Vector4.Zero),
                    (pixels[0], pixels[1], pixels[2], pixels[3]));
            }
            else
            {
                uint* pixels = stackalloc uint[4];
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)bitmap)[7])(bitmap, 0, 16, 16, pixels));
                uint color = opacity == 0 ? 0u : opacity == 0.5f ? 0x80800000u : 0xffff0000u;
                if (interpolation == 1) color = 0xff800080u;
                uint border = interpolation == 1 ? color : opacity == 0.5f ? 0x80000080u : 0xff0000ffu;
                var expected = wrap == 5
                    ? (0u, worldSpace ? color : border, border, 0u)
                    : (0u, color, color, 0u);
                Assert.AreEqual(emptyClip ? (0u, 0u, 0u, 0u) : expected, (pixels[0], pixels[1], pixels[2], pixels[3]));
            }
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &heldLock));
        }
        finally
        {
            Release(heldLock); Release(bitmap); Release(view); Release(target);
            Release(milSource); Release(sourceLock); Release(source); Release(sourceTarget); Release(factory);
        }
    }
}
