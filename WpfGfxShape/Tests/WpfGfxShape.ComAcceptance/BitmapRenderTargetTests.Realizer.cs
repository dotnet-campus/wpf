using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class BitmapRenderTargetTests
{
    private struct PathRealizer
    {
        internal void** Table;
        internal nint Brush;
        internal int Failure, Ensured, EffectsRead;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static int EnsurePathRealizer(PathRealizer* self, uint cache, uint display, nint brushContext, nint context, nint creator)
    {
        self->Ensured++;
        return self->Failure == 1 ? unchecked((int)0x80070005) : 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static nint GetPathBrush(PathRealizer* self, byte transparent) => self->Brush;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static int GetPathEffects(PathRealizer* self, nint* effects)
    {
        self->EffectsRead++;
        *effects = 0;
        return self->Failure == 2 ? unchecked((int)0x80070005) : 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static uint GetSolidPathType(nint self) => 1;

    private struct PathFigure
    {
        internal void** Table;
        internal Vector2* Points;
        internal int Segment;
        internal bool MiddleGap;
        internal bool Reversal;
    }

    private struct PathShape
    {
        internal void** Table;
        internal PathFigure* Figure;
    }

    [UnmanagedCallersOnly]
    private static uint GetPathFigureCount(nint self) => 1;

    [UnmanagedCallersOnly]
    private static byte NoPathFlag(nint self) => 0;

    [UnmanagedCallersOnly]
    private static byte IsPrivatePathClosed(PathFigure* self) => self->Reversal ? (byte)0 : (byte)1;

    [UnmanagedCallersOnly]
    private static byte IsMiddlePathGap(PathFigure* self) => self->MiddleGap && self->Segment == 1 ? (byte)1 : (byte)0;

    [UnmanagedCallersOnly]
    private static nint GetPathFigure(PathShape* self, uint index) => (nint)self->Figure;

    [UnmanagedCallersOnly]
    private static uint GetPathFillMode(nint self) => 0;

    [UnmanagedCallersOnly]
    private static byte IsPathFillable(nint self) => 1;

    [UnmanagedCallersOnly]
    private static nint GetPathStart(PathFigure* self) => (nint)self->Points;

    [UnmanagedCallersOnly]
    private static byte SetFirstPathSegment(PathFigure* self)
    {
        self->Segment = 0;
        return 1;
    }

    [UnmanagedCallersOnly]
    private static byte GetPathSegment(PathFigure* self, byte* type, nint* points)
    {
        *type = 1;
        *points = (nint)(self->Points + self->Segment + 1);
        return self->Segment == (self->Reversal ? 1 : 2) ? (byte)1 : (byte)0;
    }

    [UnmanagedCallersOnly]
    private static byte SetNextPathSegment(PathFigure* self)
    {
        self->Segment++;
        return self->Segment < 3 ? (byte)1 : (byte)0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static uint GetStrokeFigureCount(nint self) => 1;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static nint GetStrokeFigure(PathShape* self, uint index) => (nint)self->Figure;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static byte NoStrokeFlag(nint self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static byte IsStrokeClosed(PathFigure* self) => self->Reversal ? (byte)0 : (byte)1;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static byte IsStrokeGap(PathFigure* self) => self->MiddleGap && self->Segment == 1 ? (byte)1 : (byte)0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static nint GetStrokeStart(PathFigure* self) => (nint)self->Points;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static byte SetFirstStrokeSegment(PathFigure* self) { self->Segment = 0; return 1; }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static byte GetStrokeSegment(PathFigure* self, byte* type, nint* points)
    {
        *type = 1;
        *points = (nint)(self->Points + self->Segment + 1);
        return self->Segment == (self->Reversal ? 1 : 2) ? (byte)1 : (byte)0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static byte SetNextStrokeSegment(PathFigure* self) { self->Segment++; return self->Segment < 3 ? (byte)1 : (byte)0; }

    private static int DrawRealizedPath(nint view, nint context, nint realizer, bool finite, nint shape, nint pen = 0)
        => finite
            ? ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, nint, nint, int>)(*(void***)view)[10])(view, context, 0, shape, pen, pen != 0 ? realizer : 0, pen == 0 ? realizer : 0)
            : ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, int>)(*(void***)view)[11])(view, context, 0, realizer);

    /// <summary>Exercises private path and realizer slots through the production DLL, including lock failure, shape coverage and retry.</summary>
    [TestMethod]
    [TestCategory("InternalTargetContract")]
    [DataRow(0, false, false)]
    [DataRow(0, true, false)]
    [DataRow(1, false, false)]
    [DataRow(2, false, false)]
    [DataRow(2, true, false)]
    [DataRow(0, false, true)]
    [DataRow(0, true, true)]
    [DataRow(1, false, true)]
    [DataRow(2, false, true)]
    [DataRow(2, true, true)]
    [DataRow(0, false, true, true)]
    [DataRow(1, false, true, true)]
    [DataRow(2, false, true, true)]
    [DataRow(0, false, true, false, true)]
    [DataRow(1, false, true, false, true)]
    [DataRow(2, false, true, false, true)]
    [DataRow(0, false, true, false, true, true)]
    [DataRow(1, false, true, false, true, true)]
    [DataRow(2, false, true, false, true, true)]
    [DataRow(0, false, true, false, true, false, true)]
    [DataRow(1, false, true, false, true, false, true)]
    [DataRow(2, false, true, false, true, false, true)]
    [DataRow(0, false, true, false, true, false, false, true)]
    [DataRow(1, false, true, false, true, false, false, true)]
    [DataRow(2, false, true, false, true, false, false, true)]
    [DataRow(0, false, true, false, true, true, false, true)]
    [DataRow(1, false, true, false, true, true, false, true)]
    [DataRow(2, false, true, false, true, true, false, true)]
    [DataRow(0, false, true, false, true, false, false, false, 1)]
    [DataRow(1, false, true, false, true, false, false, false, 1)]
    [DataRow(2, false, true, false, true, false, false, false, 2)]
    [DataRow(0, false, true, false, true, false, false, false, 0, 3u)]
    [DataRow(1, false, true, false, true, false, false, false, 0, 3u)]
    [DataRow(2, false, true, false, true, false, false, false, 0, 3u)]
    [DataRow(0, false, true, false, true, true, false, false, 0, 0u, true)]
    [DataRow(1, false, true, false, true, true, false, false, 0, 3u, true)]
    [DataRow(0, false, true, false, false, false, false, false, 0, 0u, false, true)]
    [DataRow(1, false, true, false, true, false, false, false, 0, 0u, false, true)]
    public void WhenNativePathUsesRealizerThenPixelsAndFirstFailureArePreserved(int failure, bool nullBrush, bool finite, bool aliased = false, bool stroke = false, bool unclippedStroke = false, bool dashedStroke = false, bool middleGap = false, int penGeometry = 0, uint nativeJoin = 0, bool reversal = false, bool invalidColor = false)
    {
        var api = new Api();
        nint factory = 0, target = 0, view = 0, bitmap = 0, heldLock = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 3, 1, 16, 96, 96, 1, &target));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            Assert.AreEqual(0, api.GetBitmap(target, &bitmap));
            void** table = stackalloc void*[12];
            new Span<nint>(table, 12).Clear();
            table[6] = (delegate* unmanaged[Thiscall]<PathRealizer*, uint, uint, nint, nint, nint, int>)&EnsurePathRealizer;
            table[10] = (delegate* unmanaged[Thiscall]<PathRealizer*, byte, nint>)&GetPathBrush;
            table[11] = (delegate* unmanaged[Thiscall]<PathRealizer*, nint*, int>)&GetPathEffects;
            void** brushTable = stackalloc void*[6];
            brushTable[3] = (delegate* unmanaged[Thiscall]<nint, uint>)&GetSolidPathType;
            byte* brush = stackalloc byte[24];
            *(void***)brush = brushTable;
            float* color = (float*)(brush + sizeof(nint));
            color[0] = invalidColor ? float.NaN : 1; color[1] = 0; color[2] = 0; color[3] = 1;
            PathRealizer realizer = new() { Table = table, Brush = nullBrush ? 0 : (nint)brush, Failure = failure };
            byte* context = stackalloc byte[2048];
            byte* state = stackalloc byte[116];
            new Span<byte>(context, 2048).Clear(); new Span<byte>(state, 116).Clear();
            *(int*)(context + InternalContextLayout.Clip) = 1;
            *(nint*)(context + InternalContextLayout.RenderState) = (nint)state;
            *(Matrix4x4*)(context + InternalContextLayout.WorldToDevice) = Matrix4x4.Identity;
            *(uint*)(state + 96) = aliased ? 0u : 1u;
            void** shapeTable = stackalloc void*[7];
            new Span<nint>(shapeTable, 7).Clear();
            shapeTable[4] = (delegate* unmanaged<nint, uint>)&GetPathFigureCount;
            shapeTable[5] = (delegate* unmanaged<PathShape*, uint, nint>)&GetPathFigure;
            shapeTable[6] = (delegate* unmanaged<nint, uint>)&GetPathFillMode;
            void** figureTable = stackalloc void*[23];
            new Span<nint>(figureTable, 23).Clear();
            figureTable[1] = (delegate* unmanaged<nint, byte>)&NoPathFlag;
            figureTable[4] = (delegate* unmanaged<PathFigure*, byte>)&IsPrivatePathClosed;
            figureTable[5] = (delegate* unmanaged<nint, byte>)&NoPathFlag;
            figureTable[7] = (delegate* unmanaged<PathFigure*, byte>)&IsMiddlePathGap;
            figureTable[8] = (delegate* unmanaged<nint, byte>)&IsPathFillable;
            figureTable[15] = (delegate* unmanaged<PathFigure*, byte>)&SetFirstPathSegment;
            figureTable[16] = (delegate* unmanaged<PathFigure*, byte*, nint*, byte>)&GetPathSegment;
            figureTable[17] = (delegate* unmanaged<PathFigure*, byte>)&SetNextPathSegment;
            figureTable[22] = (delegate* unmanaged<PathFigure*, nint>)&GetPathStart;
            if (stroke)
            {
                shapeTable[4] = (delegate* unmanaged[Thiscall]<nint, uint>)&GetStrokeFigureCount;
                shapeTable[5] = (delegate* unmanaged[Thiscall]<PathShape*, uint, nint>)&GetStrokeFigure;
                figureTable[1] = (delegate* unmanaged[Thiscall]<nint, byte>)&NoStrokeFlag;
                figureTable[4] = (delegate* unmanaged[Thiscall]<PathFigure*, byte>)&IsStrokeClosed;
                figureTable[7] = (delegate* unmanaged[Thiscall]<PathFigure*, byte>)&IsStrokeGap;
                figureTable[15] = (delegate* unmanaged[Thiscall]<PathFigure*, byte>)&SetFirstStrokeSegment;
                figureTable[16] = (delegate* unmanaged[Thiscall]<PathFigure*, byte*, nint*, byte>)&GetStrokeSegment;
                figureTable[17] = (delegate* unmanaged[Thiscall]<PathFigure*, byte>)&SetNextStrokeSegment;
                figureTable[22] = (delegate* unmanaged[Thiscall]<PathFigure*, nint>)&GetStrokeStart;
            }
            Vector2* points = stackalloc Vector2[] { new(1, 0), new(2, 0), new(2, 1), new(1, 1) };
            if (reversal)
            {
                points[0] = new(1, 0.5f); points[1] = new(2, 0.5f); points[2] = new(1, 0.5f);
            }
            if (aliased)
                for (int i = 0; i < 4; i++) points[i].X += 0.25f;
            byte* nativePen = stackalloc byte[112]; new Span<byte>(nativePen, 112).Clear();
            ((float*)nativePen)[0] = 1; ((float*)nativePen)[1] = 1; ((float*)nativePen)[7] = 10;
            if (penGeometry != 0)
            {
                ((float*)nativePen)[0] = 2;
                if (penGeometry == 2) ((float*)nativePen)[2] = MathF.PI / 2;
            }
            *(uint*)(nativePen + 24) = nativeJoin;
            float* dashes = stackalloc float[] { 10, 1 };
            if (dashedStroke)
            {
                *(uint*)(nativePen + 32) = 5;
                *(nint*)(nativePen + 40) = (nint)dashes;
                *(uint*)(nativePen + 44 + 2 * sizeof(nint)) = 2;
                *(uint*)(nativePen + 48 + 2 * sizeof(nint)) = 2;
            }
            nint pen = stroke ? (nint)nativePen : 0;
            PathFigure figure = new() { Table = figureTable, Points = points, MiddleGap = middleGap, Reversal = reversal };
            PathShape shape = new() { Table = shapeTable, Figure = &figure };
            float* bounds = stackalloc float[] { finite && (!stroke || unclippedStroke) ? 0 : 1, 0, finite && (!stroke || unclippedStroke) ? 3 : 2, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint, nint, float, nint, int>)(*(void***)view)[15])(view, (nint)bounds, 0, 0, 0, 1, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &heldLock));
            Assert.AreEqual(unchecked((int)0x88982F0D), DrawRealizedPath(view, (nint)context, (nint)(&realizer), finite, (nint)(&shape), pen));
            Assert.AreEqual(0, realizer.Ensured);
            Release(heldLock); heldLock = 0;
            Assert.AreEqual(failure != 0 ? unchecked((int)0x80070005) : invalidColor ? unchecked((int)0x80070057) : 0, DrawRealizedPath(view, (nint)context, (nint)(&realizer), finite, (nint)(&shape), pen));
            Assert.AreEqual(1, realizer.Ensured);
            Assert.AreEqual(failure == 1 ? 0 : 1, realizer.EffectsRead);
            realizer.Failure = 0;
            color[0] = 1;
            Assert.AreEqual(0, DrawRealizedPath(view, (nint)context, (nint)(&realizer), finite, (nint)(&shape), pen));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)view)[16])(view));
            uint* pixels = stackalloc uint[3];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)bitmap)[7])(bitmap, 0, 12, 12, pixels));
            uint edge = unclippedStroke && !reversal ? (failure == 0 ? 0xc0c00000u : 0x80800000u) : 0u;
            uint rightEdge = reversal && nativeJoin == 0 ? 0xffff0000u : middleGap ? 0u : edge;
            Assert.AreEqual((edge, nullBrush ? 0u : 0xffff0000u, rightEdge), (pixels[0], pixels[1], pixels[2]));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &heldLock));
        }
        finally { Release(heldLock); Release(bitmap); Release(view); Release(target); Release(factory); }
    }
}
