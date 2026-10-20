using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

// Retail CPlainPen from cpen.h; DynArrayImpl<false> from dynarrayimpl.h.
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NativePlainPen
{
    internal float Width, Height, Angle;
    internal MilPenCap Start, End, Dash;
    internal MilPenJoin Join;
    internal float Miter;
    internal uint DashStyle;
    internal float Offset;
    internal NativeRealArray Dashes;
    internal uint StartMarker, EndMarker;
    internal nint StartShape, EndShape;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NativeRealArray
{
    internal float* Data;
    internal nint InitialAllocation;
    internal uint AllocSize, Capacity, Count;
}
