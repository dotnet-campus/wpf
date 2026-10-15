using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

// Owns Release/x64 geometry for the CShapeBase/IFigureData methods consumed by
// NativeStrokeReader. These are C++ virtual objects, not IUnknown identities.
internal sealed unsafe class NativeGeneratedPath : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Shape
    {
        internal void** Table;
        internal Figure* Figures;
        internal uint Count;
        internal MilFillMode FillRule;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Figure
    {
        internal void** Table;
        internal float* Points;
        internal float* Controls;
        internal byte* Types;
        internal uint Segments;
        internal byte* Joins;
        internal byte* SegmentGaps;
        internal uint Count, Current;
        internal byte Closed, StartsAtGap, EndsAtGap, Stop, Fillable;
    }

    [StructLayout(LayoutKind.Explicit, Size = 96)]
    private struct Pen
    {
        [FieldOffset(0)] internal float Width;
        [FieldOffset(4)] internal float Height;
        [FieldOffset(8)] internal float Angle;
        [FieldOffset(12)] internal MilPenCap Start;
        [FieldOffset(16)] internal MilPenCap End;
        [FieldOffset(20)] internal MilPenCap Dash;
        [FieldOffset(24)] internal MilPenJoin Join;
        [FieldOffset(28)] internal float Miter;
        [FieldOffset(32)] internal uint DashStyle;
        [FieldOffset(36)] internal float Offset;
        [FieldOffset(40)] internal float* Dashes;
        [FieldOffset(48)] internal nint InitialAllocation;
        [FieldOffset(56)] internal uint AllocSize;
        [FieldOffset(60)] internal uint Capacity;
        [FieldOffset(64)] internal uint Count;
        [FieldOffset(72)] internal uint StartMarker;
        [FieldOffset(76)] internal uint EndMarker;
        [FieldOffset(80)] internal nint StartShape;
        [FieldOffset(88)] internal nint EndShape;
    }

    private static readonly void** ShapeTable = CreateShapeTable();
    private static readonly void** FigureTable = CreateFigureTable();
    private Shape* _shape;
    private Pen* _pen;
    internal nint ShapePointer => (nint)_shape;
    internal nint PenPointer => (nint)_pen;

    internal NativeGeneratedPath(GeneratedNativeStroke snapshot)
    {
        try
        {
            _shape = (Shape*)NativeMemory.AllocZeroed((nuint)sizeof(Shape));
            _shape->Table = ShapeTable;
            _shape->Figures = (Figure*)NativeMemory.AllocZeroed((nuint)snapshot.Figures.Length, (nuint)sizeof(Figure));
            _shape->Count = (uint)snapshot.Figures.Length;
            _shape->FillRule = snapshot.FillRule;
            for (int i = 0; i < snapshot.Figures.Length; i++)
            {
                var source = snapshot.Figures[i];
                Figure* figure = &_shape->Figures[i];
                figure->Table = FigureTable;
                figure->Count = (uint)source.Points.Length;
                figure->Closed = source.Closed ? (byte)1 : (byte)0;
                figure->Fillable = source.Fillable ? (byte)1 : (byte)0;
                figure->StartsAtGap = source.StartsAtGap ? (byte)1 : (byte)0;
                figure->EndsAtGap = source.EndsAtGap ? (byte)1 : (byte)0;
                figure->Points = (float*)NativeMemory.AllocZeroed((nuint)Math.Max(1, source.Points.Length * 2), sizeof(float));
                figure->Joins = (byte*)NativeMemory.AllocZeroed((nuint)Math.Max(1, source.Points.Length));
                figure->SegmentGaps = (byte*)NativeMemory.AllocZeroed((nuint)Math.Max(1, source.Points.Length));
                for (int j = 0; j < source.Points.Length; j++)
                {
                    figure->Points[j * 2] = (float)source.Points[j].X;
                    figure->Points[j * 2 + 1] = (float)source.Points[j].Y;
                    figure->Joins[j] = source.SmoothJoins?[j] == true ? (byte)1 : (byte)0;
                    figure->SegmentGaps[j] = source.SegmentGaps is { } gaps && j < gaps.Length && gaps[j] ? (byte)1 : (byte)0;
                }
                var segments = source.Segments ?? Enumerable.Range(1, Math.Max(0, source.Points.Length - 1))
                    .Select(j => new SoftwareStrokeContours.Segment([source.Points[j]], source.SmoothJoins?[j] == true,
                        source.SegmentGaps is { } g && j - 1 < g.Length && g[j - 1])).ToArray();
                figure->Segments = (uint)segments.Length;
                figure->Controls = (float*)NativeMemory.AllocZeroed((nuint)Math.Max(1, segments.Length * 6), sizeof(float));
                figure->Types = (byte*)NativeMemory.AllocZeroed((nuint)Math.Max(1, segments.Length));
                NativeMemory.Free(figure->Joins);
                NativeMemory.Free(figure->SegmentGaps);
                figure->Joins = (byte*)NativeMemory.AllocZeroed((nuint)Math.Max(1, segments.Length + 1));
                figure->SegmentGaps = (byte*)NativeMemory.AllocZeroed((nuint)Math.Max(1, segments.Length));
                for (int j = 0; j < segments.Length; j++)
                {
                    figure->Types[j] = segments[j].Points.Length == 3 ? (byte)2 : (byte)1;
                    figure->Joins[j + 1] = segments[j].Smooth ? (byte)1 : (byte)0;
                    figure->SegmentGaps[j] = segments[j].Gap ? (byte)1 : (byte)0;
                    for (int k = 0; k < segments[j].Points.Length; k++)
                    {
                        figure->Controls[j * 6 + k * 2] = (float)segments[j].Points[k].X;
                        figure->Controls[j * 6 + k * 2 + 1] = (float)segments[j].Points[k].Y;
                    }
                }
            }
            _pen = (Pen*)NativeMemory.AllocZeroed((nuint)sizeof(Pen));
            _pen->Width = _pen->Height = snapshot.Width;
            _pen->Start = snapshot.Start;
            _pen->End = snapshot.End;
            _pen->Dash = snapshot.Dash;
            _pen->Join = snapshot.Join;
            _pen->Miter = snapshot.Miter;
            if (snapshot.Dashes.Length != 0)
            {
                _pen->DashStyle = 5;
                _pen->Offset = snapshot.Offset;
                _pen->Count = _pen->Capacity = (uint)snapshot.Dashes.Length;
                _pen->Dashes = (float*)NativeMemory.Alloc((nuint)snapshot.Dashes.Length, sizeof(float));
                snapshot.Dashes.AsSpan().CopyTo(new Span<float>(_pen->Dashes, snapshot.Dashes.Length));
            }
        }
        catch (OutOfMemoryException) { Dispose(); throw; }
    }

    public void Dispose()
    {
        Shape* shape = _shape;
        Pen* pen = _pen;
        _shape = null;
        _pen = null;
        if (pen != null) { NativeMemory.Free(pen->Dashes); NativeMemory.Free(pen); }
        if (shape == null) return;
        if (shape->Figures != null)
        {
            for (uint i = shape->Count; i > 0; i--)
            {
                NativeMemory.Free(shape->Figures[i - 1].Types);
                NativeMemory.Free(shape->Figures[i - 1].Controls);
                NativeMemory.Free(shape->Figures[i - 1].SegmentGaps);
                NativeMemory.Free(shape->Figures[i - 1].Joins);
                NativeMemory.Free(shape->Figures[i - 1].Points);
            }
            NativeMemory.Free(shape->Figures);
        }
        NativeMemory.Free(shape);
    }

    private static void** CreateShapeTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(Shape), 8 * sizeof(nint));
        table[1] = (delegate* unmanaged<Shape*, byte>)&ShapeGaps;
        table[2] = (delegate* unmanaged<Shape*, byte>)&ShapeHollows;
        table[3] = (delegate* unmanaged<Shape*, byte>)&ShapeEmpty;
        table[4] = (delegate* unmanaged<Shape*, uint>)&FigureCount;
        table[5] = (delegate* unmanaged<Shape*, uint, nint>)&GetFigure;
        table[6] = (delegate* unmanaged<Shape*, uint>)&FillMode;
        table[7] = (delegate* unmanaged<Shape*, byte>)&ShapeFalse;
        return table;
    }

    private static void** CreateFigureTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(Figure), 26 * sizeof(nint));
        table[1] = (delegate* unmanaged<Figure*, byte>)&FigureEmpty;
        table[2] = (delegate* unmanaged<Figure*, byte>)&NoSegments;
        table[3] = (delegate* unmanaged<Figure*, uint*, uint*, int>)&Counts;
        table[4] = (delegate* unmanaged<Figure*, byte>)&Closed;
        table[5] = (delegate* unmanaged<Figure*, byte>)&Smooth;
        table[6] = (delegate* unmanaged<Figure*, byte>)&Gaps;
        table[7] = (delegate* unmanaged<Figure*, byte>)&Gap;
        table[8] = (delegate* unmanaged<Figure*, byte>)&Fillable;
        table[9] = table[10] = (delegate* unmanaged<Figure*, byte>)&FigureFalse;
        table[15] = (delegate* unmanaged<Figure*, byte>)&First;
        table[16] = (delegate* unmanaged<Figure*, byte*, nint*, byte>)&Segment;
        table[17] = (delegate* unmanaged<Figure*, byte>)&Next;
        table[18] = (delegate* unmanaged<Figure*, nint>)&SegmentStart;
        table[19] = (delegate* unmanaged<Figure*, void>)&Stop;
        table[20] = (delegate* unmanaged<Figure*, void>)&ResetStop;
        table[21] = (delegate* unmanaged<Figure*, byte>)&Stopped;
        table[22] = (delegate* unmanaged<Figure*, nint>)&StartPoint;
        table[23] = (delegate* unmanaged<Figure*, nint>)&EndPoint;
        table[24] = (delegate* unmanaged<Figure*, byte>)&Last;
        table[25] = (delegate* unmanaged<Figure*, byte>)&Previous;
        return table;
    }

    [UnmanagedCallersOnly] private static byte ShapeGaps(Shape* self)
    {
        for (uint i = 0; i < self->Count; i++)
            if (HasGaps(&self->Figures[i])) return 1;
        return 0;
    }
    [UnmanagedCallersOnly] private static byte ShapeHollows(Shape* self)
    {
        for (uint i = 0; i < self->Count; i++)
            if (self->Figures[i].Fillable == 0) return 1;
        return 0;
    }
    [UnmanagedCallersOnly] private static byte ShapeFalse(Shape* self) => 0;
    [UnmanagedCallersOnly] private static byte ShapeEmpty(Shape* self)
    {
        for (uint i = 0; i < self->Count; i++)
            if (self->Figures[i].Count != 0) return 0;
        return 1;
    }
    [UnmanagedCallersOnly] private static uint FigureCount(Shape* self) => self->Count;
    [UnmanagedCallersOnly] private static nint GetFigure(Shape* self, uint index) => index < self->Count ? (nint)(&self->Figures[index]) : 0;
    [UnmanagedCallersOnly] private static uint FillMode(Shape* self) => (uint)self->FillRule;
    [UnmanagedCallersOnly] private static byte FigureEmpty(Figure* self) => self->Count == 0 ? (byte)1 : (byte)0;
    [UnmanagedCallersOnly] private static byte NoSegments(Figure* self) => self->Segments == 0 ? (byte)1 : (byte)0;
    [UnmanagedCallersOnly] private static int Counts(Figure* self, uint* segments, uint* points)
    {
        *segments = self->Segments;
        *points = self->Count;
        return 0;
    }
    [UnmanagedCallersOnly] private static byte Closed(Figure* self) => self->Closed;
    [UnmanagedCallersOnly] private static byte Smooth(Figure* self) => self->Joins[self->Current + 1];
    private static bool HasGaps(Figure* self)
    {
        for (uint i = 0; i < self->Segments; i++)
            if (self->SegmentGaps[i] != 0) return true;
        return self->StartsAtGap != 0 || self->EndsAtGap != 0;
    }
    [UnmanagedCallersOnly] private static byte Gaps(Figure* self) => HasGaps(self) ? (byte)1 : (byte)0;
    [UnmanagedCallersOnly] private static byte Gap(Figure* self) => self->SegmentGaps[self->Current];
    [UnmanagedCallersOnly] private static byte Fillable(Figure* self) => self->Fillable;
    [UnmanagedCallersOnly] private static byte FigureFalse(Figure* self) => 0;
    [UnmanagedCallersOnly] private static byte First(Figure* self) { self->Current = 0; return self->Segments > 0 ? (byte)1 : (byte)0; }
    [UnmanagedCallersOnly] private static byte Segment(Figure* self, byte* type, nint* data)
    {
        *type = self->Types[self->Current];
        *data = (nint)(self->Controls + self->Current * 6);
        return self->Current + 1 >= self->Segments ? (byte)1 : (byte)0;
    }
    [UnmanagedCallersOnly] private static byte Next(Figure* self)
    {
        if (self->Current + 1 >= self->Segments) return 0;
        self->Current++;
        return 1;
    }
    [UnmanagedCallersOnly] private static nint SegmentStart(Figure* self) => self->Current == 0 ? (nint)self->Points
        : (nint)(self->Controls + (self->Current - 1) * 6 + (self->Types[self->Current - 1] == 2 ? 4 : 0));
    [UnmanagedCallersOnly] private static void Stop(Figure* self) => self->Stop = 1;
    [UnmanagedCallersOnly] private static void ResetStop(Figure* self) => self->Stop = 0;
    [UnmanagedCallersOnly] private static byte Stopped(Figure* self) => self->Stop;
    [UnmanagedCallersOnly] private static nint StartPoint(Figure* self) => (nint)self->Points;
    [UnmanagedCallersOnly] private static nint EndPoint(Figure* self) => (nint)(self->Points + (self->Count == 0 ? 0 : self->Count - 1) * 2);
    [UnmanagedCallersOnly] private static byte Last(Figure* self) { self->Current = self->Segments > 0 ? self->Segments - 1 : 0; return self->Segments > 0 ? (byte)1 : (byte)0; }
    [UnmanagedCallersOnly] private static byte Previous(Figure* self) { if (self->Current == 0) return 0; self->Current--; return 1; }
}
