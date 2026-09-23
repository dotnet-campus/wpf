using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal interface IMilResourceUpdateCommand
{
    uint Handle { get; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilPoint2D(double X, double Y);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilRectD(double X, double Y, double Width, double Height);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilSizeD(double Width, double Height);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilMatrix3x2D(double M11, double M12, double M21, double M22, double OffsetX, double OffsetY);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilPoint3F(float X, float Y, float Z);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilQuaternionF(float X, float Y, float Z, float W);

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16)]
internal readonly struct MilDoubleResourceCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double Value;
    public uint Handle => _handle;
    internal MilDoubleResourceCommand(uint handle, double value) { Type = MilCommand.DoubleResource; _handle = handle; Value = value; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
internal readonly struct MilColorResourceCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly MilColorF Value;
    public uint Handle => _handle;
    internal MilColorResourceCommand(uint handle, MilColorF value) { Type = MilCommand.ColorResource; _handle = handle; Value = value; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
internal readonly struct MilPointResourceCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly MilPoint2D Value;
    public uint Handle => _handle;
    internal MilPointResourceCommand(uint handle, MilPoint2D value) { Type = MilCommand.PointResource; _handle = handle; Value = value; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 40)]
internal readonly struct MilRectResourceCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly MilRectD Value;
    public uint Handle => _handle;
    internal MilRectResourceCommand(uint handle, MilRectD value) { Type = MilCommand.RectResource; _handle = handle; Value = value; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
internal readonly struct MilSizeResourceCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly MilSizeD Value;
    public uint Handle => _handle;
    internal MilSizeResourceCommand(uint handle, MilSizeD value) { Type = MilCommand.SizeResource; _handle = handle; Value = value; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 56)]
internal readonly struct MilMatrixResourceCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly MilMatrix3x2D Value;
    public uint Handle => _handle;
    internal MilMatrixResourceCommand(uint handle, MilMatrix3x2D value) { Type = MilCommand.MatrixResource; _handle = handle; Value = value; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 20)]
internal readonly struct MilPoint3DResourceCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly MilPoint3F Value;
    public uint Handle => _handle;
    internal MilPoint3DResourceCommand(uint handle, MilPoint3F value) { Type = MilCommand.Point3DResource; _handle = handle; Value = value; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 20)]
internal readonly struct MilVector3DResourceCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly MilPoint3F Value;
    public uint Handle => _handle;
    internal MilVector3DResourceCommand(uint handle, MilPoint3F value) { Type = MilCommand.Vector3DResource; _handle = handle; Value = value; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
internal readonly struct MilQuaternionResourceCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly MilQuaternionF Value;
    public uint Handle => _handle;
    internal MilQuaternionResourceCommand(uint handle, MilQuaternionF value) { Type = MilCommand.QuaternionResource; _handle = handle; Value = value; }
}

internal static partial class GeneratedProtocolPacketWriter
{
    internal static byte[] WriteDoubleResource(uint handle, double value) => WriteValue(new MilDoubleResourceCommand(handle, value));
    internal static byte[] WriteColorResource(uint handle, MilColorF value) => WriteValue(new MilColorResourceCommand(handle, value));
    internal static byte[] WritePointResource(uint handle, MilPoint2D value) => WriteValue(new MilPointResourceCommand(handle, value));
    internal static byte[] WriteRectResource(uint handle, MilRectD value) => WriteValue(new MilRectResourceCommand(handle, value));
    internal static byte[] WriteSizeResource(uint handle, MilSizeD value) => WriteValue(new MilSizeResourceCommand(handle, value));
    internal static byte[] WriteMatrixResource(uint handle, MilMatrix3x2D value) => WriteValue(new MilMatrixResourceCommand(handle, value));
    internal static byte[] WritePoint3DResource(uint handle, MilPoint3F value) => WriteValue(new MilPoint3DResourceCommand(handle, value));
    internal static byte[] WriteVector3DResource(uint handle, MilPoint3F value) => WriteValue(new MilVector3DResourceCommand(handle, value));
    internal static byte[] WriteQuaternionResource(uint handle, MilQuaternionF value) => WriteValue(new MilQuaternionResourceCommand(handle, value));

    private static byte[] WriteValue<T>(T command) where T : unmanaged
    {
        byte[] packet = new byte[Marshal.SizeOf<T>()];
        MemoryMarshal.Write(packet, in command);
        return packet;
    }
}

internal abstract class GeneratedValueResource : GeneratedProtocolResource
{
    protected GeneratedValueResource(MilResourceType resourceType) : base(resourceType) { }
    internal abstract int ProcessUpdate(ReadOnlySpan<byte> value);
}

internal sealed class GeneratedValueResource<T> : GeneratedValueResource where T : unmanaged
{
    internal GeneratedValueResource(MilResourceType resourceType) : base(resourceType) { }
    internal T Value { get; private set; }
    internal int ChangeCount { get; private set; }

    internal override int ProcessUpdate(ReadOnlySpan<byte> value)
    {
        if (value.Length != Marshal.SizeOf<T>())
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        T nextValue = MemoryMarshal.Read<T>(value);
        Value = nextValue;
        ChangeCount++;
        return Direct3D9Factory.SuccessHResult;
    }
}

internal static class GeneratedResourceFactory
{
    internal static MilResourceType GetResourceType(MilCommand command) => command switch
    {
        MilCommand.DoubleResource => MilResourceType.DoubleResource,
        MilCommand.ColorResource => MilResourceType.ColorResource,
        MilCommand.PointResource => MilResourceType.PointResource,
        MilCommand.RectResource => MilResourceType.RectResource,
        MilCommand.SizeResource => MilResourceType.SizeResource,
        MilCommand.MatrixResource => MilResourceType.MatrixResource,
        MilCommand.Point3DResource => MilResourceType.Point3DResource,
        MilCommand.Vector3DResource => MilResourceType.Vector3DResource,
        MilCommand.QuaternionResource => MilResourceType.QuaternionResource,
        _ => MilResourceType.Null
    };

    internal static int Create(MilResourceType resourceType, out GeneratedProtocolResource? resource)
    {
        resource = resourceType switch
        {
            MilResourceType.DoubleResource => new GeneratedValueResource<double>(resourceType),
            MilResourceType.ColorResource => new GeneratedValueResource<MilColorF>(resourceType),
            MilResourceType.PointResource => new GeneratedValueResource<MilPoint2D>(resourceType),
            MilResourceType.RectResource => new GeneratedValueResource<MilRectD>(resourceType),
            MilResourceType.SizeResource => new GeneratedValueResource<MilSizeD>(resourceType),
            MilResourceType.MatrixResource => new GeneratedValueResource<MilMatrix3x2D>(resourceType),
            MilResourceType.Point3DResource => new GeneratedValueResource<MilPoint3F>(resourceType),
            MilResourceType.Vector3DResource => new GeneratedValueResource<MilPoint3F>(resourceType),
            MilResourceType.QuaternionResource => new GeneratedValueResource<MilQuaternionF>(resourceType),
            MilResourceType.GlyphRun => new GeneratedProtocolResource(resourceType),
            > MilResourceType.Null and < MilResourceType.Last => new GeneratedProtocolResource(resourceType),
            _ => null
        };

        return resource is null
            ? Direct3D9Factory.UceMalformedPacketHResult
            : Direct3D9Factory.SuccessHResult;
    }
}
