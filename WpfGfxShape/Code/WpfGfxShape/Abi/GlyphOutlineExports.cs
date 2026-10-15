using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class GlyphOutlineExports
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Sink { internal void** Table; internal nint Handle; internal int References; }
    [StructLayout(LayoutKind.Sequential)]
    private struct Point { internal float X, Y; }
    private sealed class Geometry
    {
        internal readonly List<byte> Bytes = new(new byte[48]);
        internal int Figure = -1, PreviousFigure, PreviousSegment, LastSegment;
        internal uint Fill, Flags;
        internal int Error;
        internal bool Closed;
        internal void UInt(int offset, uint value)
        {
            for (int i = 0; i < 4; i++) Bytes[offset + i] = (byte)(value >> (8 * i));
        }
        internal uint Read(int offset) => (uint)(Bytes[offset] | Bytes[offset + 1] << 8 | Bytes[offset + 2] << 16 | Bytes[offset + 3] << 24);
        internal void Double(int offset, double value)
        {
            ulong bits = (ulong)BitConverter.DoubleToInt64Bits(value);
            for (int i = 0; i < 8; i++) Bytes[offset + i] = (byte)(bits >> (8 * i));
        }
        internal void Add(int size) => Bytes.AddRange(new byte[size]);
    }
    private static readonly void** Table = CreateTable();

    [UnmanagedCallersOnly(EntryPoint = "MilGlyphRun_GetGlyphOutline", CallConvs = [typeof(CallConvStdcall)])]
    private static int Outline(nint face, ushort glyph, byte sideways, double em, byte** data, uint* size, uint* fill)
    {
        Sink* sink = null;
        GCHandle handle = default;
        try
        {
            if (face == 0 || data == null || size == null || fill == null) return unchecked((int)0x80070057);
            var geometry = new Geometry();
            handle = GCHandle.Alloc(geometry);
            sink = (Sink*)NativeMemory.AllocZeroed((nuint)sizeof(Sink));
            sink->Table = Table; sink->Handle = GCHandle.ToIntPtr(handle); sink->References = 1;
            int hr = ((delegate* unmanaged[Stdcall]<nint, float, ushort*, float*, void*, uint, int, int, Sink*, int>)(*(void***)face)[14])(
                face, (float)em, &glyph, null, null, 1, sideways != 0 ? 1 : 0, 0, sink);
            if (hr < 0) return hr;
            if (geometry.Error < 0) return geometry.Error;
            if (geometry.Figure >= 0) return unchecked((int)0x80004005);
            geometry.UInt(0, (uint)geometry.Bytes.Count);
            byte[] serialized = geometry.Bytes.ToArray();
            if (!MilPathGeometryValidator.IsValid(serialized)) return unchecked((int)0x80004005);
            byte* allocation = (byte*)NativeMemory.Alloc((nuint)serialized.Length);
            serialized.CopyTo(new Span<byte>(allocation, serialized.Length));
            *data = allocation; *size = (uint)serialized.Length; *fill = geometry.Fill;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        finally
        {
            if (sink != null) NativeMemory.Free(sink);
            if (handle.IsAllocated) handle.Free();
            Direct3D9Factory.Release(face);
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilGlyphRun_ReleasePathGeometryData", CallConvs = [typeof(CallConvStdcall)])]
    private static int Free(byte* data) { NativeMemory.Free(data); return 0; }
    private static Geometry Get(Sink* sink) => (Geometry)GCHandle.FromIntPtr(sink->Handle).Target!;
    private static void** CreateTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(GlyphOutlineExports), 10 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<Sink*, Guid*, nint*, int>)&Query;
        table[1] = (delegate* unmanaged[Stdcall]<Sink*, uint>)&AddRef;
        table[2] = (delegate* unmanaged[Stdcall]<Sink*, uint>)&Release;
        table[3] = (delegate* unmanaged[Stdcall]<Sink*, uint, void>)&Fill;
        table[4] = (delegate* unmanaged[Stdcall]<Sink*, uint, void>)&Flags;
        table[5] = (delegate* unmanaged[Stdcall]<Sink*, Point, uint, void>)&Begin;
        table[6] = (delegate* unmanaged[Stdcall]<Sink*, Point*, uint, void>)&Lines;
        table[7] = (delegate* unmanaged[Stdcall]<Sink*, Point*, uint, void>)&Beziers;
        table[8] = (delegate* unmanaged[Stdcall]<Sink*, uint, void>)&End;
        table[9] = (delegate* unmanaged[Stdcall]<Sink*, int>)&Close;
        return table;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(Sink* sink, Guid* id, nint* result)
    {
        if (result == null) return unchecked((int)0x80004003);
        *result = 0;
        if (id == null || (*id != new Guid("00000000-0000-0000-c000-000000000046") && *id != new Guid("2cd9069e-12e2-11dc-9fed-001143a055f9"))) return unchecked((int)0x80004002);
        *result = (nint)sink; Interlocked.Increment(ref sink->References); return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(Sink* sink) => (uint)Interlocked.Increment(ref sink->References);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(Sink* sink) => (uint)Interlocked.Decrement(ref sink->References);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Fill(Sink* sink, uint fill)
    {
        Geometry g = Get(sink);
        if (g.Closed || fill > 1) g.Error = unchecked((int)0x80004005); else g.Fill = fill;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Flags(Sink* sink, uint flags)
    {
        Geometry g = Get(sink);
        if (g.Closed || g.Figure < 0) g.Error = unchecked((int)0x80004005);
        else g.Flags = ((flags & 1) == 0 ? 0x4u : 0) | ((flags & 2) == 0 ? 0x8u : 0);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Begin(Sink* sink, Point point, uint begin)
    {
        Geometry g = Get(sink);
        if (g.Closed || g.Figure >= 0) { g.Error = unchecked((int)0x80004005); return; }
        try
        {
            g.Figure = g.Bytes.Count; g.Add(40);
            g.UInt(g.Figure, (uint)g.PreviousFigure);
            g.UInt(g.Figure + 4, begin == 0 ? 8u : 0);
            g.UInt(g.Figure + 12, 40);
            g.Double(g.Figure + 16, point.X); g.Double(g.Figure + 24, point.Y);
            g.PreviousSegment = 0; g.LastSegment = 0;
        }
        catch (OutOfMemoryException) { g.Error = unchecked((int)0x8007000E); }
    }
    private static void Segment(Geometry g, Point* points, int count)
    {
        if (g.Error < 0) return;
        if (g.Closed || g.Figure < 0) { g.Error = unchecked((int)0x80004005); return; }
        try
        {
            int start = g.Bytes.Count, length = count == 1 ? 32 : 64;
            g.Add(length); g.UInt(start, count == 1 ? 1u : 2u);
            uint flags = g.Flags | (count == 3 ? 0x20u : 0);
            g.UInt(start + 4, flags); g.UInt(start + 8, (uint)g.PreviousSegment);
            for (int i = 0; i < count; i++) { g.Double(start + 16 + i * 16, points[i].X); g.Double(start + 24 + i * 16, points[i].Y); }
            uint figureFlags = g.Read(g.Figure + 4) | ((flags & 4) == 0 ? 1u : 0) | ((flags & 0x20) == 0 ? 2u : 0);
            g.UInt(g.Figure + 4, figureFlags);
            g.UInt(g.Figure + 8, g.Read(g.Figure + 8) + 1);
            g.UInt(g.Figure + 12, (uint)(g.Bytes.Count - g.Figure));
            g.UInt(g.Figure + 32, (uint)(start - g.Figure));
            g.PreviousSegment = length; g.LastSegment = start;
        }
        catch (OutOfMemoryException) { g.Error = unchecked((int)0x8007000E); }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Lines(Sink* sink, Point* points, uint count)
    { Geometry g = Get(sink); for (uint i = 0; i < count; i++) Segment(g, points + i, 1); }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Beziers(Sink* sink, Point* points, uint count)
    { Geometry g = Get(sink); for (uint i = 0; i < count; i++) Segment(g, points + i * 3, 3); }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void End(Sink* sink, uint end)
    {
        Geometry g = Get(sink);
        if (g.Error < 0) return;
        if (g.Closed || g.Figure < 0 || g.LastSegment == 0) { g.Error = unchecked((int)0x80004005); return; }
        uint flags = g.Read(g.Figure + 4) | (end == 1 ? 4u : 0);
        g.UInt(g.Figure + 4, flags);
        g.UInt(4, g.Read(4) | ((flags & 2) != 0 ? 1u : 0) | ((flags & 1) != 0 ? 4u : 0) | ((flags & 8) == 0 ? 8u : 0));
        g.UInt(40, g.Read(40) + 1);
        g.PreviousFigure = g.Bytes.Count - g.Figure; g.Figure = -1;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Close(Sink* sink)
    {
        Geometry g = Get(sink);
        if (g.Figure >= 0 || g.Closed) g.Error = unchecked((int)0x80004005);
        g.Closed = true; return g.Error;
    }
}
