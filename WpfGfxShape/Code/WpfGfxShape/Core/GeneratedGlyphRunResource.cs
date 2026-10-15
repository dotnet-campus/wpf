using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 76)]
internal readonly struct MilGlyphRunCreateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] internal readonly uint Handle;
    [FieldOffset(8)] internal readonly ulong Font;
    [FieldOffset(16)] internal readonly ushort Flags;
    [FieldOffset(20)] internal readonly float OriginX;
    [FieldOffset(24)] internal readonly float OriginY;
    [FieldOffset(28)] internal readonly float EmSize;
    [FieldOffset(32)] internal readonly MilRectD Bounds;
    [FieldOffset(64)] internal readonly ushort GlyphCount;
    [FieldOffset(68)] internal readonly ushort BidiLevel;
    [FieldOffset(72)] internal readonly ushort MeasuringMethod;
}

internal sealed unsafe class GeneratedGlyphRunResource : GeneratedProtocolResource
{
    private FrozenBitmapLease? _capturedFace;
    internal GeneratedGlyphRunResource CaptureValue()
        => new() { Command = Command, Indices = Indices.ToArray(), Advances = Advances.ToArray(),
            Offsets = Offsets.ToArray(), _capturedFace = PinFontFace() };
    private nint _font;
    private FrozenComSlot? _fontFaceSlot;
    internal FrozenBitmapLease? PinFontFace() => _fontFaceSlot?.Pin();
    internal GeneratedGlyphRunResource() : base(MilResourceType.GlyphRun) { }
    internal MilGlyphRunCreateCommand Command { get; private set; }
    internal ushort[] Indices { get; private set; } = [];
    internal float[] Advances { get; private set; } = [];
    internal Direct3D9GlyphOffset[] Offsets { get; private set; } = [];
    internal nint FontFace => _capturedFace?.Pointer ?? _fontFaceSlot?.Pointer ?? 0;

    // The queued command owns one font reference; consume it even if initialization fails.
    internal int ProcessCreate(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < 76) return Direct3D9Factory.UceMalformedPacketHResult;
        MilGlyphRunCreateCommand command = MemoryMarshal.Read<MilGlyphRunCreateCommand>(packet);
        nint font = (nint)command.Font;
        nint face = 0;
        try
        {
            if (IsReleased || command.GlyphCount == 0 || !(command.EmSize >= 0) || font == 0)
                return Direct3D9Factory.UceMalformedPacketHResult;
            int count = command.GlyphCount;
            int indicesSize = count * sizeof(ushort);
            int advancesSize = count * sizeof(float);
            bool hasOffsets = (command.Flags & 0x10) != 0;
            int payloadSize = indicesSize + advancesSize + (hasOffsets ? count * 8 : 0);
            if (packet.Length - 76 < payloadSize)
                return Direct3D9Factory.UceMalformedPacketHResult;
            ushort[] indices = MemoryMarshal.Cast<byte, ushort>(packet.Slice(76, indicesSize)).ToArray();
            float[] advances = MemoryMarshal.Cast<byte, float>(packet.Slice(76 + indicesSize, advancesSize)).ToArray();
            Direct3D9GlyphOffset[] offsets = hasOffsets
                ? MemoryMarshal.Cast<byte, Direct3D9GlyphOffset>(packet.Slice(76 + indicesSize + advancesSize, count * 8)).ToArray()
                : new Direct3D9GlyphOffset[count];
            int result = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)font)[13])(font, &face);
            if (result < 0) return result;
            if (face == 0) return unchecked((int)0x80004003);

            var nextFace = new FrozenComSlot(face);
            FrozenComSlot? previousFace = _fontFaceSlot;
            nint previousFont = _font;
            _font = font;
            _fontFaceSlot = nextFace;
            font = 0;
            face = 0;
            Command = command;
            Indices = indices;
            Advances = advances;
            Offsets = offsets;
            try { NotifyChanged(); }
            finally
            {
                previousFace?.Retire();
                Direct3D9Factory.Release(previousFont);
            }
            return Direct3D9Factory.SuccessHResult;
        }
        finally
        {
            Direct3D9Factory.Release(face);
            Direct3D9Factory.Release(font);
        }
    }

    protected override void OnFinalRelease()
    {
        _capturedFace?.Dispose();
        _capturedFace = null;
        FrozenComSlot? face = _fontFaceSlot;
        nint font = _font;
        _fontFaceSlot = null;
        _font = 0;
        Command = default;
        Indices = [];
        Advances = [];
        Offsets = [];
        face?.Retire();
        Direct3D9Factory.Release(font);
        base.OnFinalRelease();
    }
}
