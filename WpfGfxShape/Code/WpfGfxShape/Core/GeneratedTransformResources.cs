using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
internal readonly struct MilTransformGroupCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly uint ChildrenSize;
    public uint Handle => _handle;
    internal MilTransformGroupCommand(uint handle, uint childrenSize) { Type = MilCommand.TransformGroup; _handle = handle; ChildrenSize = childrenSize; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 32)]
internal readonly struct MilTranslateTransformCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double X;
    [FieldOffset(16)] internal readonly double Y;
    [FieldOffset(24)] internal readonly uint XAnimation;
    [FieldOffset(28)] internal readonly uint YAnimation;
    public uint Handle => _handle;
    internal MilTranslateTransformCommand(uint handle, double x, double y, uint xAnimation, uint yAnimation)
    { Type = MilCommand.TranslateTransform; _handle = handle; X = x; Y = y; XAnimation = xAnimation; YAnimation = yAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 56)]
internal readonly struct MilScaleTransformCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double ScaleX;
    [FieldOffset(16)] internal readonly double ScaleY;
    [FieldOffset(24)] internal readonly double CenterX;
    [FieldOffset(32)] internal readonly double CenterY;
    [FieldOffset(40)] internal readonly uint ScaleXAnimation;
    [FieldOffset(44)] internal readonly uint ScaleYAnimation;
    [FieldOffset(48)] internal readonly uint CenterXAnimation;
    [FieldOffset(52)] internal readonly uint CenterYAnimation;
    public uint Handle => _handle;
    internal MilScaleTransformCommand(uint handle, double scaleX, double scaleY, double centerX, double centerY, uint scaleXAnimation, uint scaleYAnimation, uint centerXAnimation, uint centerYAnimation)
    { Type = MilCommand.ScaleTransform; _handle = handle; ScaleX = scaleX; ScaleY = scaleY; CenterX = centerX; CenterY = centerY; ScaleXAnimation = scaleXAnimation; ScaleYAnimation = scaleYAnimation; CenterXAnimation = centerXAnimation; CenterYAnimation = centerYAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 56)]
internal readonly struct MilSkewTransformCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double AngleX;
    [FieldOffset(16)] internal readonly double AngleY;
    [FieldOffset(24)] internal readonly double CenterX;
    [FieldOffset(32)] internal readonly double CenterY;
    [FieldOffset(40)] internal readonly uint AngleXAnimation;
    [FieldOffset(44)] internal readonly uint AngleYAnimation;
    [FieldOffset(48)] internal readonly uint CenterXAnimation;
    [FieldOffset(52)] internal readonly uint CenterYAnimation;
    public uint Handle => _handle;
    internal MilSkewTransformCommand(uint handle, double angleX, double angleY, double centerX, double centerY, uint angleXAnimation, uint angleYAnimation, uint centerXAnimation, uint centerYAnimation)
    { Type = MilCommand.SkewTransform; _handle = handle; AngleX = angleX; AngleY = angleY; CenterX = centerX; CenterY = centerY; AngleXAnimation = angleXAnimation; AngleYAnimation = angleYAnimation; CenterXAnimation = centerXAnimation; CenterYAnimation = centerYAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 44)]
internal readonly struct MilRotateTransformCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double Angle;
    [FieldOffset(16)] internal readonly double CenterX;
    [FieldOffset(24)] internal readonly double CenterY;
    [FieldOffset(32)] internal readonly uint AngleAnimation;
    [FieldOffset(36)] internal readonly uint CenterXAnimation;
    [FieldOffset(40)] internal readonly uint CenterYAnimation;
    public uint Handle => _handle;
    internal MilRotateTransformCommand(uint handle, double angle, double centerX, double centerY, uint angleAnimation, uint centerXAnimation, uint centerYAnimation)
    { Type = MilCommand.RotateTransform; _handle = handle; Angle = angle; CenterX = centerX; CenterY = centerY; AngleAnimation = angleAnimation; CenterXAnimation = centerXAnimation; CenterYAnimation = centerYAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 60)]
internal readonly struct MilMatrixTransformCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly MilMatrix3x2D Matrix;
    [FieldOffset(56)] internal readonly uint MatrixAnimation;
    public uint Handle => _handle;
    internal MilMatrixTransformCommand(uint handle, MilMatrix3x2D matrix, uint matrixAnimation)
    { Type = MilCommand.MatrixTransform; _handle = handle; Matrix = matrix; MatrixAnimation = matrixAnimation; }
}

internal static partial class GeneratedProtocolPacketWriter
{
    internal static byte[] WriteTransformGroup(uint handle, params uint[] children)
    {
        ArgumentNullException.ThrowIfNull(children);
        int payloadSize = checked(children.Length * sizeof(uint));
        byte[] packet = new byte[Marshal.SizeOf<MilTransformGroupCommand>() + payloadSize];
        MilTransformGroupCommand command = new(handle, (uint)payloadSize);
        MemoryMarshal.Write(packet, in command);
        children.AsSpan().CopyTo(MemoryMarshal.Cast<byte, uint>(packet.AsSpan(Marshal.SizeOf<MilTransformGroupCommand>())));
        return packet;
    }

    internal static byte[] WriteTranslateTransform(uint handle, double x, double y, uint xAnimation = 0, uint yAnimation = 0) => WriteTransform(new MilTranslateTransformCommand(handle, x, y, xAnimation, yAnimation));
    internal static byte[] WriteScaleTransform(uint handle, double scaleX, double scaleY, double centerX, double centerY, uint scaleXAnimation = 0, uint scaleYAnimation = 0, uint centerXAnimation = 0, uint centerYAnimation = 0) => WriteTransform(new MilScaleTransformCommand(handle, scaleX, scaleY, centerX, centerY, scaleXAnimation, scaleYAnimation, centerXAnimation, centerYAnimation));
    internal static byte[] WriteSkewTransform(uint handle, double angleX, double angleY, double centerX, double centerY, uint angleXAnimation = 0, uint angleYAnimation = 0, uint centerXAnimation = 0, uint centerYAnimation = 0) => WriteTransform(new MilSkewTransformCommand(handle, angleX, angleY, centerX, centerY, angleXAnimation, angleYAnimation, centerXAnimation, centerYAnimation));
    internal static byte[] WriteRotateTransform(uint handle, double angle, double centerX, double centerY, uint angleAnimation = 0, uint centerXAnimation = 0, uint centerYAnimation = 0) => WriteTransform(new MilRotateTransformCommand(handle, angle, centerX, centerY, angleAnimation, centerXAnimation, centerYAnimation));
    internal static byte[] WriteMatrixTransform(uint handle, MilMatrix3x2D matrix, uint matrixAnimation = 0) => WriteTransform(new MilMatrixTransformCommand(handle, matrix, matrixAnimation));

    private static byte[] WriteTransform<T>(T command) where T : unmanaged
    {
        byte[] packet = new byte[Marshal.SizeOf<T>()];
        MemoryMarshal.Write(packet, in command);
        return packet;
    }
}

internal abstract class GeneratedDependencyResource : GeneratedUpdatableResource
{
    private GeneratedProtocolResource[] _dependencies = [];

    protected GeneratedDependencyResource(MilResourceType resourceType) : base(resourceType) { }

    protected bool TryResolve(GeneratedProtocolHandleTable handleTable, uint handle, MilResourceType resourceType, out GeneratedProtocolResource? dependency)
        => handleTable.TryGetResource(handle, resourceType, out dependency);

    protected static bool TryResolve(
        GeneratedProtocolHandleTable handleTable,
        uint handle,
        Func<MilResourceType, bool> acceptsType,
        out GeneratedProtocolResource? dependency)
    {
        if (handle == 0)
        {
            dependency = null;
            return true;
        }

        return handleTable.TryGetResource(handle, out dependency)
            && dependency is not null
            && acceptsType(dependency.ResourceType);
    }

    protected int CommitDependencies(ReadOnlySpan<GeneratedProtocolResource?> dependencies, Action commitData)
    {
        GeneratedProtocolResource[] nextDependencies = dependencies
            .ToArray()
            .Where(static dependency => dependency is not null)
            .Cast<GeneratedProtocolResource>()
            .ToArray();
        foreach (GeneratedProtocolResource dependency in nextDependencies)
        {
            dependency.AddListener(this);
        }

        GeneratedProtocolResource[] previousDependencies = _dependencies;
        _dependencies = nextDependencies;
        commitData();
        foreach (GeneratedProtocolResource dependency in previousDependencies)
        {
            dependency.RemoveListener(this);
        }

        NotifyChanged();
        return Direct3D9Factory.SuccessHResult;
    }

    protected override void OnFinalRelease()
    {
        foreach (GeneratedProtocolResource dependency in _dependencies)
        {
            dependency.RemoveListener(this);
        }

        _dependencies = [];
    }
}

internal sealed class GeneratedTransformGroupResource : GeneratedDependencyResource
{
    internal GeneratedTransformGroupResource() : base(MilResourceType.TransformGroup) { }
    internal IReadOnlyList<GeneratedProtocolResource> Children { get; private set; } = [];

    internal override int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value)
    {
        if (value.Length < sizeof(uint))
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        uint childrenSize = MemoryMarshal.Read<uint>(value);
        ReadOnlySpan<byte> payload = value[sizeof(uint)..];
        if (childrenSize != payload.Length || childrenSize % sizeof(uint) != 0)
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        ReadOnlySpan<uint> handles = MemoryMarshal.Cast<byte, uint>(payload);
        GeneratedProtocolResource?[] children = new GeneratedProtocolResource?[handles.Length];
        for (int i = 0; i < handles.Length; i++)
        {
            uint handle = handles[i];
            if (handle == 0 || !handleTable.TryGetResource(handle, out GeneratedProtocolResource? child) || child is null || !IsTransform(child.ResourceType))
            {
                return Direct3D9Factory.UceMalformedPacketHResult;
            }

            children[i] = child;
        }

        return CommitDependencies(children, () => Children = children.Cast<GeneratedProtocolResource>().ToArray());
    }

    private static bool IsTransform(MilResourceType resourceType) => resourceType is
        MilResourceType.TransformGroup or MilResourceType.TranslateTransform or MilResourceType.ScaleTransform or
        MilResourceType.SkewTransform or MilResourceType.RotateTransform or MilResourceType.MatrixTransform;
}

internal sealed class GeneratedTranslateTransformResource : GeneratedDependencyResource
{
    internal GeneratedTranslateTransformResource() : base(MilResourceType.TranslateTransform) { }
    internal (double X, double Y) Value { get; private set; }
    private GeneratedProtocolResource?[] _animationSlots = [];
    internal (double X, double Y) CurrentValue => (
        GeneratedImageTransform.ReadAnimated(_animationSlots, 0, Value.X),
        GeneratedImageTransform.ReadAnimated(_animationSlots, 1, Value.Y));
    internal IReadOnlyList<GeneratedProtocolResource> Animations { get; private set; } = [];

    internal override int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value)
    {
        if (value.Length != 24) return Direct3D9Factory.UceMalformedPacketHResult;
        double x = MemoryMarshal.Read<double>(value);
        double y = MemoryMarshal.Read<double>(value[8..]);
        uint xHandle = MemoryMarshal.Read<uint>(value[16..]);
        uint yHandle = MemoryMarshal.Read<uint>(value[20..]);
        if (!TryResolve(handleTable, xHandle, MilResourceType.DoubleResource, out GeneratedProtocolResource? xAnimation)
            || !TryResolve(handleTable, yHandle, MilResourceType.DoubleResource, out GeneratedProtocolResource? yAnimation))
            return Direct3D9Factory.UceMalformedPacketHResult;
        GeneratedProtocolResource?[] dependencies = [xAnimation, yAnimation];
        return CommitDependencies(dependencies, () => { Value = (x, y); _animationSlots = dependencies; Animations = dependencies.Where(static item => item is not null).Cast<GeneratedProtocolResource>().ToArray(); });
    }
}

internal abstract class GeneratedFourDoubleTransformResource : GeneratedDependencyResource
{
    protected GeneratedFourDoubleTransformResource(MilResourceType resourceType) : base(resourceType) { }
    internal (double First, double Second, double Third, double Fourth) Value { get; private set; }
    private GeneratedProtocolResource?[] _animationSlots = [];
    internal (double First, double Second, double Third, double Fourth) CurrentValue => (
        GeneratedImageTransform.ReadAnimated(_animationSlots, 0, Value.First),
        GeneratedImageTransform.ReadAnimated(_animationSlots, 1, Value.Second),
        GeneratedImageTransform.ReadAnimated(_animationSlots, 2, Value.Third),
        GeneratedImageTransform.ReadAnimated(_animationSlots, 3, Value.Fourth));
    internal IReadOnlyList<GeneratedProtocolResource> Animations { get; private set; } = [];

    internal override int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value)
    {
        if (value.Length != 48) return Direct3D9Factory.UceMalformedPacketHResult;
        double first = MemoryMarshal.Read<double>(value);
        double second = MemoryMarshal.Read<double>(value[8..]);
        double third = MemoryMarshal.Read<double>(value[16..]);
        double fourth = MemoryMarshal.Read<double>(value[24..]);
        GeneratedProtocolResource?[] dependencies = new GeneratedProtocolResource?[4];
        for (int i = 0; i < dependencies.Length; i++)
        {
            uint handle = MemoryMarshal.Read<uint>(value[(32 + i * sizeof(uint))..]);
            if (!TryResolve(handleTable, handle, MilResourceType.DoubleResource, out dependencies[i]))
                return Direct3D9Factory.UceMalformedPacketHResult;
        }
        return CommitDependencies(dependencies, () => { Value = (first, second, third, fourth); _animationSlots = dependencies; Animations = dependencies.Where(static item => item is not null).Cast<GeneratedProtocolResource>().ToArray(); });
    }
}

internal sealed class GeneratedScaleTransformResource : GeneratedFourDoubleTransformResource
{
    internal GeneratedScaleTransformResource() : base(MilResourceType.ScaleTransform) { }
}

internal sealed class GeneratedSkewTransformResource : GeneratedFourDoubleTransformResource
{
    internal GeneratedSkewTransformResource() : base(MilResourceType.SkewTransform) { }
}

internal sealed class GeneratedRotateTransformResource : GeneratedDependencyResource
{
    internal GeneratedRotateTransformResource() : base(MilResourceType.RotateTransform) { }
    internal (double Angle, double CenterX, double CenterY) Value { get; private set; }
    private GeneratedProtocolResource?[] _animationSlots = [];
    internal (double Angle, double CenterX, double CenterY) CurrentValue => (
        GeneratedImageTransform.ReadAnimated(_animationSlots, 0, Value.Angle),
        GeneratedImageTransform.ReadAnimated(_animationSlots, 1, Value.CenterX),
        GeneratedImageTransform.ReadAnimated(_animationSlots, 2, Value.CenterY));
    internal IReadOnlyList<GeneratedProtocolResource> Animations { get; private set; } = [];

    internal override int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value)
    {
        if (value.Length != 36) return Direct3D9Factory.UceMalformedPacketHResult;
        double angle = MemoryMarshal.Read<double>(value);
        double centerX = MemoryMarshal.Read<double>(value[8..]);
        double centerY = MemoryMarshal.Read<double>(value[16..]);
        GeneratedProtocolResource?[] dependencies = new GeneratedProtocolResource?[3];
        for (int i = 0; i < dependencies.Length; i++)
        {
            uint handle = MemoryMarshal.Read<uint>(value[(24 + i * sizeof(uint))..]);
            if (!TryResolve(handleTable, handle, MilResourceType.DoubleResource, out dependencies[i]))
                return Direct3D9Factory.UceMalformedPacketHResult;
        }
        return CommitDependencies(dependencies, () => { Value = (angle, centerX, centerY); _animationSlots = dependencies; Animations = dependencies.Where(static item => item is not null).Cast<GeneratedProtocolResource>().ToArray(); });
    }
}

internal sealed class GeneratedMatrixTransformResource : GeneratedDependencyResource
{
    internal GeneratedMatrixTransformResource() : base(MilResourceType.MatrixTransform) { }
    internal MilMatrix3x2D Value { get; private set; }
    internal GeneratedProtocolResource? Animation { get; private set; }

    internal override int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value)
    {
        if (value.Length != 52) return Direct3D9Factory.UceMalformedPacketHResult;
        MilMatrix3x2D matrix = MemoryMarshal.Read<MilMatrix3x2D>(value);
        uint handle = MemoryMarshal.Read<uint>(value[48..]);
        if (!TryResolve(handleTable, handle, MilResourceType.MatrixResource, out GeneratedProtocolResource? animation))
            return Direct3D9Factory.UceMalformedPacketHResult;
        GeneratedProtocolResource?[] dependencies = [animation];
        return CommitDependencies(dependencies, () => { Value = matrix; Animation = animation; });
    }
}
