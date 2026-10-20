using System.Numerics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

// Retail api_brush.h: relative to the secondary CMILBrush base, not CMILObject.
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NativeBitmapBrushLayout
{
    internal void** Table;
    internal ResourceCache Cache;
    internal uint Uniqueness, Space;
    internal Matrix4x4 Mapping;
    internal Parallelogram Clip;
    internal int HasSourceClip, EntireSource;
    internal nint Source;
    internal uint Wrap;
    internal Vector4 Border;
    internal float Opacity;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ResourceCache
    {
        internal nint Table, Data, InitialAllocation;
        internal uint AllocSize, Capacity, Count;
        internal nint Initial0, Initial1;
        internal int InCall;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Parallelogram
    {
        internal nint Table;
        internal Figure Data;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Figure
    {
        internal nint Table;
        internal Vector2 P0, P1, P2, P3;
        internal uint CurrentSegment;
    }
}
