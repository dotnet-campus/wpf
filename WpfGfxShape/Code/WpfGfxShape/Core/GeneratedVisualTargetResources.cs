using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

[Flags]
internal enum MilRenderTargetInitializationFlags : uint
{
    Default = 0,
    SoftwareOnly = 0x00000001,
    HardwareOnly = 0x00000002,
    Null = 0x00000003,
    TypeMask = 0x00000003,
    DisableDirtyRectangles = 0x00010000,
    UseRefRast = 0x01000000,
    UseRgbRast = 0x02000000
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilRectL(int Left, int Top, int Right, int Bottom);

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 20)]
internal readonly struct MilGuidelineSetCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly uint GuidelinesXSize;
    [FieldOffset(12)] internal readonly uint GuidelinesYSize;
    [FieldOffset(16)] internal readonly int IsDynamic;
    public uint Handle => _handle;
    internal MilGuidelineSetCommand(uint handle, uint xSize, uint ySize, bool isDynamic) { Type = MilCommand.GuidelineSet; _handle = handle; GuidelinesXSize = xSize; GuidelinesYSize = ySize; IsDynamic = isDynamic ? 1 : 0; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 32)]
internal readonly struct MilBitmapCacheCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double RenderAtScale;
    [FieldOffset(16)] internal readonly uint RenderAtScaleAnimation;
    [FieldOffset(20)] internal readonly int SnapsToDevicePixels;
    [FieldOffset(24)] internal readonly int EnableClearType;
    public uint Handle => _handle;
    internal MilBitmapCacheCommand(uint handle, double scale, uint animation, bool snaps, bool clearType) { Type = MilCommand.BitmapCache; _handle = handle; RenderAtScale = scale; RenderAtScaleAnimation = animation; SnapsToDevicePixels = snaps ? 1 : 0; EnableClearType = clearType ? 1 : 0; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
internal readonly struct MilVisualCreateCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; internal MilVisualCreateCommand(uint handle) { Type = MilCommand.VisualCreate; Handle = handle; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
internal readonly struct MilVisualSetOffsetCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; [FieldOffset(8)] internal readonly double X; [FieldOffset(16)] internal readonly double Y; internal MilVisualSetOffsetCommand(uint handle, double x, double y) { Type = MilCommand.VisualSetOffset; Handle = handle; X = x; Y = y; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
internal readonly struct MilVisualDependencyCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; [FieldOffset(8)] internal readonly uint Dependency; internal MilVisualDependencyCommand(MilCommand type, uint handle, uint dependency) { Type = type; Handle = handle; Dependency = dependency; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16)]
internal readonly struct MilVisualSetAlphaCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; [FieldOffset(8)] internal readonly double Alpha; internal MilVisualSetAlphaCommand(uint handle, double alpha) { Type = MilCommand.VisualSetAlpha; Handle = handle; Alpha = alpha; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
internal readonly struct MilVisualRemoveAllChildrenCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; internal MilVisualRemoveAllChildrenCommand(uint handle) { Type = MilCommand.VisualRemoveAllChildren; Handle = handle; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
internal readonly struct MilVisualRemoveChildCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; [FieldOffset(8)] internal readonly uint Child; internal MilVisualRemoveChildCommand(uint handle, uint child) { Type = MilCommand.VisualRemoveChild; Handle = handle; Child = child; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16)]
internal readonly struct MilVisualInsertChildAtCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; [FieldOffset(8)] internal readonly uint Child; [FieldOffset(12)] internal readonly uint Index; internal MilVisualInsertChildAtCommand(uint handle, uint child, uint index) { Type = MilCommand.VisualInsertChildAt; Handle = handle; Child = child; Index = index; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
internal readonly struct MilTargetSetRootCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; [FieldOffset(8)] internal readonly uint Root; internal MilTargetSetRootCommand(uint handle, uint root) { Type = MilCommand.TargetSetRoot; Handle = handle; Root = root; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
internal readonly struct MilTargetSetClearColorCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; [FieldOffset(8)] internal readonly MilColorF ClearColor; internal MilTargetSetClearColorCommand(uint handle, MilColorF color) { Type = MilCommand.TargetSetClearColor; Handle = handle; ClearColor = color; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
internal readonly struct MilTargetInvalidateCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; [FieldOffset(8)] internal readonly MilRectL Rect; internal MilTargetInvalidateCommand(uint handle, MilRectL rect) { Type = MilCommand.TargetInvalidate; Handle = handle; Rect = rect; } }
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
internal readonly struct MilTargetSetFlagsCommand { [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] internal readonly uint Handle; [FieldOffset(8)] internal readonly MilRenderTargetInitializationFlags Flags; internal MilTargetSetFlagsCommand(uint handle, MilRenderTargetInitializationFlags flags) { Type = MilCommand.TargetSetFlags; Handle = handle; Flags = flags; } }

internal static partial class GeneratedProtocolPacketWriter
{
    internal static byte[] WriteGuidelineSet(uint handle, ReadOnlySpan<double> x, ReadOnlySpan<double> y, bool isDynamic)
    {
        int xSize = checked(x.Length * sizeof(double)); int ySize = checked(y.Length * sizeof(double)); byte[] packet = new byte[Marshal.SizeOf<MilGuidelineSetCommand>() + xSize + ySize]; MilGuidelineSetCommand command = new(handle, (uint)xSize, (uint)ySize, isDynamic); MemoryMarshal.Write(packet, in command); x.CopyTo(MemoryMarshal.Cast<byte, double>(packet.AsSpan(Marshal.SizeOf<MilGuidelineSetCommand>(), xSize))); y.CopyTo(MemoryMarshal.Cast<byte, double>(packet.AsSpan(Marshal.SizeOf<MilGuidelineSetCommand>() + xSize, ySize))); return packet;
    }
    internal static byte[] WriteBitmapCache(uint handle, double scale, uint animation = 0, bool snaps = false, bool clearType = false) => WriteVisualTarget(new MilBitmapCacheCommand(handle, scale, animation, snaps, clearType));
    internal static byte[] WriteVisualCreate(uint handle) => WriteVisualTarget(new MilVisualCreateCommand(handle));
    internal static byte[] WriteVisualSetOffset(uint handle, double x, double y) => WriteVisualTarget(new MilVisualSetOffsetCommand(handle, x, y));
    internal static byte[] WriteVisualSetTransform(uint handle, uint dependency) => WriteVisualTarget(new MilVisualDependencyCommand(MilCommand.VisualSetTransform, handle, dependency));
    internal static byte[] WriteVisualSetClip(uint handle, uint dependency) => WriteVisualTarget(new MilVisualDependencyCommand(MilCommand.VisualSetClip, handle, dependency));
    internal static byte[] WriteVisualSetContent(uint handle, uint dependency) => WriteVisualTarget(new MilVisualDependencyCommand(MilCommand.VisualSetContent, handle, dependency));
    internal static byte[] WriteVisualSetAlphaMask(uint handle, uint dependency) => WriteVisualTarget(new MilVisualDependencyCommand(MilCommand.VisualSetAlphaMask, handle, dependency));
    internal static byte[] WriteVisualSetAlpha(uint handle, double alpha) => WriteVisualTarget(new MilVisualSetAlphaCommand(handle, alpha));
    internal static byte[] WriteVisualRemoveAllChildren(uint handle) => WriteVisualTarget(new MilVisualRemoveAllChildrenCommand(handle));
    internal static byte[] WriteVisualRemoveChild(uint handle, uint child) => WriteVisualTarget(new MilVisualRemoveChildCommand(handle, child));
    internal static byte[] WriteVisualInsertChildAt(uint handle, uint child, uint index) => WriteVisualTarget(new MilVisualInsertChildAtCommand(handle, child, index));
    internal static byte[] WriteTargetSetRoot(uint handle, uint root) => WriteVisualTarget(new MilTargetSetRootCommand(handle, root));
    internal static byte[] WriteTargetSetClearColor(uint handle, MilColorF color) => WriteVisualTarget(new MilTargetSetClearColorCommand(handle, color));
    internal static byte[] WriteTargetInvalidate(uint handle, MilRectL rect) => WriteVisualTarget(new MilTargetInvalidateCommand(handle, rect));
    internal static byte[] WriteTargetSetFlags(uint handle, MilRenderTargetInitializationFlags flags) => WriteVisualTarget(new MilTargetSetFlagsCommand(handle, flags));
    private static byte[] WriteVisualTarget<T>(T command) where T : unmanaged { byte[] packet = new byte[Marshal.SizeOf<T>()]; MemoryMarshal.Write(packet, in command); return packet; }
}

internal sealed class GeneratedGuidelineSetResource : GeneratedUpdatableResource
{
    internal GeneratedGuidelineSetResource() : base(MilResourceType.GuidelineSet) { }
    internal double[] GuidelinesX { get; private set; } = [];
    internal double[] GuidelinesY { get; private set; } = [];
    internal bool IsDynamic { get; private set; }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length < 12) return Direct3D9Factory.UceMalformedPacketHResult;
        uint xSize = MemoryMarshal.Read<uint>(value); uint ySize = MemoryMarshal.Read<uint>(value[4..]); int dynamic = MemoryMarshal.Read<int>(value[8..]); ReadOnlySpan<byte> payload = value[12..];
        if ((dynamic is not 0 and not 1) || xSize % sizeof(double) != 0 || ySize % sizeof(double) != 0 || (ulong)xSize + ySize != (ulong)payload.Length) return Direct3D9Factory.UceMalformedPacketHResult;
        double[] x = MemoryMarshal.Cast<byte, double>(payload[..(int)xSize]).ToArray(); double[] y = MemoryMarshal.Cast<byte, double>(payload[(int)xSize..]).ToArray();
        GuidelinesX = x; GuidelinesY = y; IsDynamic = dynamic != 0; NotifyChanged(); return Direct3D9Factory.SuccessHResult;
    }
}

internal sealed class GeneratedBitmapCacheResource : GeneratedDependencyResource
{
    internal GeneratedBitmapCacheResource() : base(MilResourceType.BitmapCache) { }
    internal double RenderAtScale { get; private set; }
    internal GeneratedProtocolResource? RenderAtScaleAnimation { get; private set; }
    internal bool SnapsToDevicePixels { get; private set; }
    internal bool EnableClearType { get; private set; }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length != 24) return Direct3D9Factory.UceMalformedPacketHResult;
        double scale = MemoryMarshal.Read<double>(value); uint animationHandle = MemoryMarshal.Read<uint>(value[8..]); int snaps = MemoryMarshal.Read<int>(value[12..]); int clearType = MemoryMarshal.Read<int>(value[16..]);
        if ((snaps is not 0 and not 1) || (clearType is not 0 and not 1) || !TryResolve(table, animationHandle, MilResourceType.DoubleResource, out GeneratedProtocolResource? animation)) return Direct3D9Factory.UceMalformedPacketHResult;
        GeneratedProtocolResource?[] dependencies = [animation]; return CommitDependencies(dependencies, () => { RenderAtScale = scale; RenderAtScaleAnimation = animation; SnapsToDevicePixels = snaps != 0; EnableClearType = clearType != 0; });
    }
}

internal sealed class GeneratedVisualResource : GeneratedProtocolResource
{
    private readonly List<GeneratedVisualResource> _children = [];
    private GeneratedProtocolResource? _transform;
    private GeneratedProtocolResource? _clip;
    private GeneratedProtocolResource? _content;
    private GeneratedProtocolResource? _alphaMask;
    internal GeneratedVisualResource() : base(MilResourceType.Visual) { }
    internal MilPoint2D Offset { get; private set; }
    internal double Alpha { get; private set; } = 1;
    internal uint? BitmapScalingMode { get; private set; }
    internal MilCompositingMode? CompositingMode { get; private set; }
    internal GeneratedProtocolResource? Transform => _transform;
    internal GeneratedProtocolResource? Clip => _clip;
    internal GeneratedProtocolResource? Content => _content;
    internal GeneratedProtocolResource? AlphaMask => _alphaMask;
    internal GeneratedVisualResource? Parent { get; private set; }
    internal IReadOnlyList<GeneratedVisualResource> Children => _children;
    internal int ProcessCommand(GeneratedProtocolHandleTable table, MilCommand command, ReadOnlySpan<byte> packet) => command switch
    {
        MilCommand.VisualCreate => packet.Length == 8 ? Direct3D9Factory.SuccessHResult : Direct3D9Factory.UceMalformedPacketHResult,
        MilCommand.VisualSetOffset => SetOffset(packet),
        MilCommand.VisualSetAlpha => SetAlpha(packet),
        MilCommand.VisualSetRenderOptions => SetRenderOptions(packet),
        MilCommand.VisualSetTransform => SetDependency(table, packet, static type => type is >= MilResourceType.TransformGroup and <= MilResourceType.MatrixTransform, 0),
        MilCommand.VisualSetClip => SetDependency(table, packet, static type => type is >= MilResourceType.LineGeometry and <= MilResourceType.PathGeometry, 1),
        MilCommand.VisualSetContent => SetDependency(table, packet, static type => type is MilResourceType.RenderData or >= MilResourceType.GeometryDrawing and <= MilResourceType.DrawingGroup, 2),
        MilCommand.VisualSetAlphaMask => SetDependency(table, packet, GeneratedEffectResource.IsBrush, 3),
        MilCommand.VisualInsertChildAt => InsertChild(table, packet),
        MilCommand.VisualRemoveChild => RemoveChild(table, packet),
        MilCommand.VisualRemoveAllChildren => RemoveAll(packet),
        _ => Direct3D9Factory.UceUnknownPacketHResult
    };
    private int SetRenderOptions(ReadOnlySpan<byte> packet)
    {
        if (packet.Length != 36) return Direct3D9Factory.UceMalformedPacketHResult;
        uint flags = MemoryMarshal.Read<uint>(packet[8..]);
        if ((flags & ~5u) != 0) return Direct3D9Factory.NotImplementedHResult;
        uint compositing = MemoryMarshal.Read<uint>(packet[16..]);
        if ((flags & 4) != 0 && compositing > 1) return Direct3D9Factory.NotImplementedHResult;
        uint mode = MemoryMarshal.Read<uint>(packet[20..]);
        if ((flags & 1) != 0 && mode > 3) return Direct3D9Factory.InvalidArgumentHResult;
        BitmapScalingMode = (flags & 1) != 0 && mode != 0 ? mode : null;
        CompositingMode = (flags & 4) != 0 ? (MilCompositingMode)compositing : null;
        NotifyChanged();
        return 0;
    }

    private int SetOffset(ReadOnlySpan<byte> packet) { if (packet.Length != 24) return Direct3D9Factory.UceMalformedPacketHResult; Offset = new(MemoryMarshal.Read<double>(packet[8..]), MemoryMarshal.Read<double>(packet[16..])); NotifyChanged(); return 0; }
    private int SetAlpha(ReadOnlySpan<byte> packet) { if (packet.Length != 16) return Direct3D9Factory.UceMalformedPacketHResult; Alpha = MemoryMarshal.Read<double>(packet[8..]); NotifyChanged(); return 0; }
    private int SetDependency(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> packet, Func<MilResourceType, bool> accepts, int slot)
    {
        if (packet.Length != 12) return Direct3D9Factory.UceMalformedPacketHResult; uint handle = MemoryMarshal.Read<uint>(packet[8..]); GeneratedProtocolResource? next = null;
        if (handle != 0 && (!table.TryGetResource(handle, out next) || next is null || !accepts(next.ResourceType))) return Direct3D9Factory.UceMalformedPacketHResult;
        GeneratedProtocolResource? previous = slot switch { 0 => _transform, 1 => _clip, 2 => _content, _ => _alphaMask }; if (ReferenceEquals(previous, next)) return 0;
        next?.AddListener(this); if (slot == 0) _transform = next; else if (slot == 1) _clip = next; else if (slot == 2) _content = next; else _alphaMask = next; previous?.RemoveListener(this); NotifyChanged(); return 0;
    }
    private int InsertChild(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> packet)
    {
        if (packet.Length != 16) return Direct3D9Factory.UceMalformedPacketHResult; uint childHandle = MemoryMarshal.Read<uint>(packet[8..]); uint index = MemoryMarshal.Read<uint>(packet[12..]);
        if (!table.TryGetResource(childHandle, MilResourceType.Visual, out GeneratedProtocolResource? value) || value is not GeneratedVisualResource child || index > _children.Count || ReferenceEquals(child, this) || child.Parent is not null || IsAncestorOf(child)) return Direct3D9Factory.UceMalformedPacketHResult;
        child.AddListener(this); _children.Insert((int)index, child); child.Parent = this; NotifyChanged(); return 0;
    }
    private bool IsAncestorOf(GeneratedVisualResource child) { for (GeneratedVisualResource? parent = this; parent is not null; parent = parent.Parent) if (ReferenceEquals(parent, child)) return true; return false; }
    private int RemoveChild(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> packet)
    {
        if (packet.Length != 12) return Direct3D9Factory.UceMalformedPacketHResult; uint childHandle = MemoryMarshal.Read<uint>(packet[8..]);
        if (!table.TryGetResource(childHandle, MilResourceType.Visual, out GeneratedProtocolResource? value) || value is not GeneratedVisualResource child || !ReferenceEquals(child.Parent, this) || !_children.Contains(child)) return Direct3D9Factory.UceMalformedPacketHResult;
        _children.Remove(child); child.Parent = null; child.RemoveListener(this); NotifyChanged(); return 0;
    }
    private int RemoveAll(ReadOnlySpan<byte> packet) { if (packet.Length != 8) return Direct3D9Factory.UceMalformedPacketHResult; foreach (GeneratedVisualResource child in _children) { child.Parent = null; child.RemoveListener(this); } _children.Clear(); NotifyChanged(); return 0; }
    protected override void OnFinalRelease() { foreach (GeneratedVisualResource child in _children) { child.Parent = null; child.RemoveListener(this); } _children.Clear(); ReleaseDependency(ref _transform); ReleaseDependency(ref _clip); ReleaseDependency(ref _content); ReleaseDependency(ref _alphaMask); }
    private void ReleaseDependency(ref GeneratedProtocolResource? dependency) { dependency?.RemoveListener(this); dependency = null; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct MilGenericTargetCreateCommand
{
    internal readonly MilCommand Type;
    internal readonly uint Handle;
    internal readonly ulong Window;
    internal readonly ulong RenderTarget;
    internal readonly uint Width;
    internal readonly uint Height;
    internal readonly uint Dummy;
}

internal sealed class GeneratedTargetResource : GeneratedProtocolResource
{
    private GeneratedVisualResource? _root;
    private nint _renderTarget;
    internal uint Width { get; private set; }
    internal uint Height { get; private set; }
    internal GeneratedTargetResource(MilResourceType type) : base(type) { }
    internal GeneratedVisualResource? Root => _root;
    internal MilColorF ClearColor { get; private set; }
    internal MilRectL InvalidatedRect { get; private set; }
    internal int InvalidationCount { get; private set; }
    internal MilRenderTargetInitializationFlags Flags { get; private set; }
    internal int ProcessCommand(GeneratedProtocolHandleTable table, MilCommand command, ReadOnlySpan<byte> packet) => command switch
    {
        MilCommand.GenericTargetCreate => CreateGenericTarget(packet),
        MilCommand.TargetSetRoot => SetRoot(table, packet),
        MilCommand.TargetSetClearColor => SetClearColor(packet),
        MilCommand.TargetInvalidate => Invalidate(packet),
        MilCommand.TargetSetFlags => SetFlags(packet),
        _ => Direct3D9Factory.UceUnknownPacketHResult
    };
    private int SetRoot(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> packet) { if (packet.Length != 12) return Direct3D9Factory.UceMalformedPacketHResult; uint handle = MemoryMarshal.Read<uint>(packet[8..]); GeneratedVisualResource? next = null; if (handle != 0) { if (!table.TryGetResource(handle, MilResourceType.Visual, out GeneratedProtocolResource? value) || value is not GeneratedVisualResource visual) return Direct3D9Factory.UceMalformedPacketHResult; next = visual; } if (ReferenceEquals(_root, next)) return 0; next?.AddListener(this); GeneratedVisualResource? previous = _root; _root = next; previous?.RemoveListener(this); NotifyChanged(); return 0; }
    private int SetClearColor(ReadOnlySpan<byte> packet) { if (packet.Length != 24) return Direct3D9Factory.UceMalformedPacketHResult; ClearColor = MemoryMarshal.Read<MilColorF>(packet[8..]); NotifyChanged(); return 0; }
    private int Invalidate(ReadOnlySpan<byte> packet) { if (packet.Length != 24) return Direct3D9Factory.UceMalformedPacketHResult; InvalidatedRect = MemoryMarshal.Read<MilRectL>(packet[8..]); InvalidationCount++; NotifyChanged(); return 0; }
    private int SetFlags(ReadOnlySpan<byte> packet) { if (packet.Length != 12) return Direct3D9Factory.UceMalformedPacketHResult; MilRenderTargetInitializationFlags flags = MemoryMarshal.Read<MilRenderTargetInitializationFlags>(packet[8..]); const MilRenderTargetInitializationFlags allowed = MilRenderTargetInitializationFlags.TypeMask | MilRenderTargetInitializationFlags.UseRefRast | MilRenderTargetInitializationFlags.UseRgbRast | MilRenderTargetInitializationFlags.DisableDirtyRectangles; if ((flags & ~allowed) != 0) return Direct3D9Factory.InvalidArgumentHResult; Flags = flags; NotifyChanged(); return 0; }
    private unsafe int CreateGenericTarget(ReadOnlySpan<byte> packet)
    {
        if (ResourceType != MilResourceType.GenericRenderTarget || packet.Length != Marshal.SizeOf<MilGenericTargetCreateCommand>())
            return Direct3D9Factory.UceMalformedPacketHResult;
        var command = MemoryMarshal.Read<MilGenericTargetCreateCommand>(packet);
        nint next = (nint)command.RenderTarget;
        Width = command.Width;
        Height = command.Height;
        // The command borrows its pointer; ProcessCreate acquires the resource's own reference.
        if (next != 0)
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)next)[1])(next);
        nint previous = _renderTarget;
        _renderTarget = next;
        Direct3D9Factory.Release(previous);
        return 0;
    }

    internal unsafe int Render()
    {
        nint target = _renderTarget;
        GeneratedVisualResource? root = _root;
        uint width = Width, height = Height;
        if (target == 0 || root is null || width == 0 || height == 0) return 0;
        root.AddRef();
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)target)[1])(target);
        try { return GeneratedVisualRenderer.Render(root, target, width, height); }
        finally
        {
            Direct3D9Factory.Release(target);
            root.Release();
        }
    }

    protected override void OnFinalRelease()
    {
        nint renderTarget = _renderTarget;
        _renderTarget = 0;
        _root?.RemoveListener(this);
        _root = null;
        Direct3D9Factory.Release(renderTarget);
    }
}
