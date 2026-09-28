using System.Numerics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilPoint2F(float X, float Y);

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 36)]
internal readonly struct MilAxisAngleRotation3DCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double Angle; [FieldOffset(16)] internal readonly MilPoint3F Axis;
    [FieldOffset(28)] internal readonly uint AxisAnimation; [FieldOffset(32)] internal readonly uint AngleAnimation;
    public uint Handle => _handle;
    internal MilAxisAngleRotation3DCommand(uint handle, double angle, MilPoint3F axis, uint axisAnimation, uint angleAnimation) { Type = MilCommand.AxisAngleRotation3D; _handle = handle; Angle = angle; Axis = axis; AxisAnimation = axisAnimation; AngleAnimation = angleAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 28)]
internal readonly struct MilQuaternionRotation3DCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly MilQuaternionF Quaternion; [FieldOffset(24)] internal readonly uint Animation;
    public uint Handle => _handle;
    internal MilQuaternionRotation3DCommand(uint handle, MilQuaternionF quaternion, uint animation) { Type = MilCommand.QuaternionRotation3D; _handle = handle; Quaternion = quaternion; Animation = animation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 96)]
internal readonly struct MilProjectionCameraCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double NearPlaneDistance; [FieldOffset(16)] internal readonly double FarPlaneDistance; [FieldOffset(24)] internal readonly double ProjectionValue;
    [FieldOffset(32)] internal readonly MilPoint3F Position; [FieldOffset(44)] internal readonly uint Transform; [FieldOffset(48)] internal readonly MilPoint3F LookDirection; [FieldOffset(60)] internal readonly uint NearAnimation;
    [FieldOffset(64)] internal readonly MilPoint3F UpDirection; [FieldOffset(76)] internal readonly uint FarAnimation; [FieldOffset(80)] internal readonly uint PositionAnimation; [FieldOffset(84)] internal readonly uint LookAnimation; [FieldOffset(88)] internal readonly uint UpAnimation; [FieldOffset(92)] internal readonly uint ProjectionAnimation;
    public uint Handle => _handle;
    internal MilProjectionCameraCommand(MilCommand type, uint handle, double near, double far, double projection, MilPoint3F position, uint transform, MilPoint3F look, MilPoint3F up, uint nearAnimation, uint farAnimation, uint positionAnimation, uint lookAnimation, uint upAnimation, uint projectionAnimation)
    { Type = type; _handle = handle; NearPlaneDistance = near; FarPlaneDistance = far; ProjectionValue = projection; Position = position; Transform = transform; LookDirection = look; NearAnimation = nearAnimation; UpDirection = up; FarAnimation = farAnimation; PositionAnimation = positionAnimation; LookAnimation = lookAnimation; UpAnimation = upAnimation; ProjectionAnimation = projectionAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 140)]
internal readonly struct MilMatrixCameraCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly Matrix4x4 View; [FieldOffset(72)] internal readonly Matrix4x4 Projection; [FieldOffset(136)] internal readonly uint Transform;
    public uint Handle => _handle;
    internal MilMatrixCameraCommand(uint handle, Matrix4x4 view, Matrix4x4 projection, uint transform) { Type = MilCommand.MatrixCamera; _handle = handle; View = view; Projection = projection; Transform = transform; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16)]
internal readonly struct MilModel3DGroupCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly uint Transform; [FieldOffset(12)] internal readonly uint ChildrenSize;
    public uint Handle => _handle; internal MilModel3DGroupCommand(uint handle, uint transform, uint size) { Type = MilCommand.Model3DGroup; _handle = handle; Transform = transform; ChildrenSize = size; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 32)]
internal readonly struct MilAmbientLightCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly MilColorF Color; [FieldOffset(24)] internal readonly uint Transform; [FieldOffset(28)] internal readonly uint ColorAnimation;
    public uint Handle => _handle; internal MilAmbientLightCommand(uint handle, MilColorF color, uint transform, uint animation) { Type = MilCommand.AmbientLight; _handle = handle; Color = color; Transform = transform; ColorAnimation = animation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 48)]
internal readonly struct MilDirectionalLightCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly MilColorF Color; [FieldOffset(24)] internal readonly MilPoint3F Direction; [FieldOffset(36)] internal readonly uint Transform; [FieldOffset(40)] internal readonly uint ColorAnimation; [FieldOffset(44)] internal readonly uint DirectionAnimation;
    public uint Handle => _handle; internal MilDirectionalLightCommand(uint handle, MilColorF color, MilPoint3F direction, uint transform, uint colorAnimation, uint directionAnimation) { Type = MilCommand.DirectionalLight; _handle = handle; Color = color; Direction = direction; Transform = transform; ColorAnimation = colorAnimation; DirectionAnimation = directionAnimation; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 96)]
internal readonly struct MilPointLightCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly MilColorF Color; [FieldOffset(24)] internal readonly double Range; [FieldOffset(32)] internal readonly double ConstantAttenuation; [FieldOffset(40)] internal readonly double LinearAttenuation; [FieldOffset(48)] internal readonly double QuadraticAttenuation; [FieldOffset(56)] internal readonly MilPoint3F Position; [FieldOffset(68)] internal readonly uint Transform; [FieldOffset(72)] internal readonly uint ColorAnimation; [FieldOffset(76)] internal readonly uint PositionAnimation; [FieldOffset(80)] internal readonly uint RangeAnimation; [FieldOffset(84)] internal readonly uint ConstantAnimation; [FieldOffset(88)] internal readonly uint LinearAnimation; [FieldOffset(92)] internal readonly uint QuadraticAnimation;
    public uint Handle => _handle;
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 136)]
internal readonly struct MilSpotLightCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly MilColorF Color;
    [FieldOffset(24)] internal readonly double Range;
    [FieldOffset(32)] internal readonly double ConstantAttenuation;
    [FieldOffset(40)] internal readonly double LinearAttenuation;
    [FieldOffset(48)] internal readonly double QuadraticAttenuation;
    [FieldOffset(56)] internal readonly double OuterConeAngle;
    [FieldOffset(64)] internal readonly double InnerConeAngle;
    [FieldOffset(72)] internal readonly MilPoint3F Position;
    [FieldOffset(84)] internal readonly uint Transform;
    [FieldOffset(88)] internal readonly MilPoint3F Direction;
    [FieldOffset(100)] internal readonly uint ColorAnimation;
    [FieldOffset(104)] internal readonly uint PositionAnimation;
    [FieldOffset(108)] internal readonly uint RangeAnimation;
    [FieldOffset(112)] internal readonly uint ConstantAnimation;
    [FieldOffset(116)] internal readonly uint LinearAnimation;
    [FieldOffset(120)] internal readonly uint QuadraticAnimation;
    [FieldOffset(124)] internal readonly uint DirectionAnimation;
    [FieldOffset(128)] internal readonly uint OuterConeAnimation;
    [FieldOffset(132)] internal readonly uint InnerConeAnimation;
    public uint Handle => _handle;
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
internal readonly struct MilGeometryModel3DCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly uint Transform; [FieldOffset(12)] internal readonly uint Geometry; [FieldOffset(16)] internal readonly uint Material; [FieldOffset(20)] internal readonly uint BackMaterial;
    public uint Handle => _handle; internal MilGeometryModel3DCommand(uint handle, uint transform, uint geometry, uint material, uint backMaterial) { Type = MilCommand.GeometryModel3D; _handle = handle; Transform = transform; Geometry = geometry; Material = material; BackMaterial = backMaterial; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
internal readonly struct MilMeshGeometry3DCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly uint PositionsSize; [FieldOffset(12)] internal readonly uint NormalsSize; [FieldOffset(16)] internal readonly uint TextureCoordinatesSize; [FieldOffset(20)] internal readonly uint TriangleIndicesSize;
    public uint Handle => _handle; internal MilMeshGeometry3DCommand(uint handle, uint positions, uint normals, uint texture, uint indices) { Type = MilCommand.MeshGeometry3D; _handle = handle; PositionsSize = positions; NormalsSize = normals; TextureCoordinatesSize = texture; TriangleIndicesSize = indices; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
internal readonly struct MilResourceGroup3DCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly uint ChildrenSize;
    public uint Handle => _handle; internal MilResourceGroup3DCommand(MilCommand type, uint handle, uint size) { Type = type; _handle = handle; ChildrenSize = size; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 44)]
internal readonly struct MilDiffuseMaterialCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly MilColorF Color; [FieldOffset(24)] internal readonly MilColorF AmbientColor; [FieldOffset(40)] internal readonly uint Brush;
    public uint Handle => _handle; internal MilDiffuseMaterialCommand(uint handle, MilColorF color, MilColorF ambient, uint brush) { Type = MilCommand.DiffuseMaterial; _handle = handle; Color = color; AmbientColor = ambient; Brush = brush; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 36)]
internal readonly struct MilSpecularMaterialCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly MilColorF Color; [FieldOffset(24)] internal readonly double Power; [FieldOffset(32)] internal readonly uint Brush;
    public uint Handle => _handle; internal MilSpecularMaterialCommand(uint handle, MilColorF color, double power, uint brush) { Type = MilCommand.SpecularMaterial; _handle = handle; Color = color; Power = power; Brush = brush; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 28)]
internal readonly struct MilEmissiveMaterialCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly MilColorF Color; [FieldOffset(24)] internal readonly uint Brush;
    public uint Handle => _handle; internal MilEmissiveMaterialCommand(uint handle, MilColorF color, uint brush) { Type = MilCommand.EmissiveMaterial; _handle = handle; Color = color; Brush = brush; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 44)]
internal readonly struct MilTranslateTransform3DCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly double X; [FieldOffset(16)] internal readonly double Y; [FieldOffset(24)] internal readonly double Z; [FieldOffset(32)] internal readonly uint XAnimation; [FieldOffset(36)] internal readonly uint YAnimation; [FieldOffset(40)] internal readonly uint ZAnimation;
    public uint Handle => _handle; internal MilTranslateTransform3DCommand(uint handle, double x, double y, double z, uint xa, uint ya, uint za) { Type = MilCommand.TranslateTransform3D; _handle = handle; X = x; Y = y; Z = z; XAnimation = xa; YAnimation = ya; ZAnimation = za; }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 80)]
internal readonly struct MilScaleTransform3DCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double X;
    [FieldOffset(16)] internal readonly double Y;
    [FieldOffset(24)] internal readonly double Z;
    [FieldOffset(32)] internal readonly double CenterX;
    [FieldOffset(40)] internal readonly double CenterY;
    [FieldOffset(48)] internal readonly double CenterZ;
    [FieldOffset(56)] internal readonly uint XAnimation;
    [FieldOffset(60)] internal readonly uint YAnimation;
    [FieldOffset(64)] internal readonly uint ZAnimation;
    [FieldOffset(68)] internal readonly uint CenterXAnimation;
    [FieldOffset(72)] internal readonly uint CenterYAnimation;
    [FieldOffset(76)] internal readonly uint CenterZAnimation;
    public uint Handle => _handle;
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 48)]
internal readonly struct MilRotateTransform3DCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] private readonly uint _handle;
    [FieldOffset(8)] internal readonly double CenterX;
    [FieldOffset(16)] internal readonly double CenterY;
    [FieldOffset(24)] internal readonly double CenterZ;
    [FieldOffset(32)] internal readonly uint CenterXAnimation;
    [FieldOffset(36)] internal readonly uint CenterYAnimation;
    [FieldOffset(40)] internal readonly uint CenterZAnimation;
    [FieldOffset(44)] internal readonly uint Rotation;
    public uint Handle => _handle;
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 72)]
internal readonly struct MilMatrixTransform3DCommand : IMilResourceUpdateCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type; [FieldOffset(4)] private readonly uint _handle; [FieldOffset(8)] internal readonly Matrix4x4 Matrix;
    public uint Handle => _handle; internal MilMatrixTransform3DCommand(uint handle, Matrix4x4 matrix) { Type = MilCommand.MatrixTransform3D; _handle = handle; Matrix = matrix; }
}

internal static partial class GeneratedProtocolPacketWriter
{
    internal static byte[] WriteAxisAngleRotation3D(uint handle, double angle, MilPoint3F axis, uint axisAnimation = 0, uint angleAnimation = 0) => Write3D(new MilAxisAngleRotation3DCommand(handle, angle, axis, axisAnimation, angleAnimation));
    internal static byte[] WriteQuaternionRotation3D(uint handle, MilQuaternionF quaternion, uint animation = 0) => Write3D(new MilQuaternionRotation3DCommand(handle, quaternion, animation));
    internal static byte[] WritePerspectiveCamera(uint handle, double near, double far, double fieldOfView, MilPoint3F position, MilPoint3F look, MilPoint3F up, uint transform = 0, uint nearAnimation = 0, uint farAnimation = 0, uint positionAnimation = 0, uint lookAnimation = 0, uint upAnimation = 0, uint fieldOfViewAnimation = 0) => Write3D(new MilProjectionCameraCommand(MilCommand.PerspectiveCamera, handle, near, far, fieldOfView, position, transform, look, up, nearAnimation, farAnimation, positionAnimation, lookAnimation, upAnimation, fieldOfViewAnimation));
    internal static byte[] WriteOrthographicCamera(uint handle, double near, double far, double width, MilPoint3F position, MilPoint3F look, MilPoint3F up, uint transform = 0, uint nearAnimation = 0, uint farAnimation = 0, uint positionAnimation = 0, uint lookAnimation = 0, uint upAnimation = 0, uint widthAnimation = 0) => Write3D(new MilProjectionCameraCommand(MilCommand.OrthographicCamera, handle, near, far, width, position, transform, look, up, nearAnimation, farAnimation, positionAnimation, lookAnimation, upAnimation, widthAnimation));
    internal static byte[] WriteMatrixCamera(uint handle, Matrix4x4 view, Matrix4x4 projection, uint transform = 0) => Write3D(new MilMatrixCameraCommand(handle, view, projection, transform));
    internal static byte[] WriteModel3DGroup(uint handle, ReadOnlySpan<uint> children, uint transform = 0) => WriteGroup3D(new MilModel3DGroupCommand(handle, transform, checked((uint)(children.Length * sizeof(uint)))), children);
    internal static byte[] WriteAmbientLight(uint handle, MilColorF color, uint transform = 0, uint colorAnimation = 0) => Write3D(new MilAmbientLightCommand(handle, color, transform, colorAnimation));
    internal static byte[] WriteDirectionalLight(uint handle, MilColorF color, MilPoint3F direction, uint transform = 0, uint colorAnimation = 0, uint directionAnimation = 0) => Write3D(new MilDirectionalLightCommand(handle, color, direction, transform, colorAnimation, directionAnimation));
    internal static byte[] WriteGeometryModel3D(uint handle, uint geometry, uint material = 0, uint backMaterial = 0, uint transform = 0) => Write3D(new MilGeometryModel3DCommand(handle, transform, geometry, material, backMaterial));
    internal static byte[] WriteMaterialGroup(uint handle, ReadOnlySpan<uint> children) => WriteGroup3D(new MilResourceGroup3DCommand(MilCommand.MaterialGroup, handle, checked((uint)(children.Length * sizeof(uint)))), children);
    internal static byte[] WriteTransform3DGroup(uint handle, ReadOnlySpan<uint> children) => WriteGroup3D(new MilResourceGroup3DCommand(MilCommand.Transform3DGroup, handle, checked((uint)(children.Length * sizeof(uint)))), children);
    internal static byte[] WriteDiffuseMaterial(uint handle, MilColorF color, MilColorF ambient, uint brush = 0) => Write3D(new MilDiffuseMaterialCommand(handle, color, ambient, brush));
    internal static byte[] WriteSpecularMaterial(uint handle, MilColorF color, double power, uint brush = 0) => Write3D(new MilSpecularMaterialCommand(handle, color, power, brush));
    internal static byte[] WriteEmissiveMaterial(uint handle, MilColorF color, uint brush = 0) => Write3D(new MilEmissiveMaterialCommand(handle, color, brush));
    internal static byte[] WriteTranslateTransform3D(uint handle, double x, double y, double z, uint xa = 0, uint ya = 0, uint za = 0) => Write3D(new MilTranslateTransform3DCommand(handle, x, y, z, xa, ya, za));
    internal static byte[] WriteMatrixTransform3D(uint handle, Matrix4x4 matrix) => Write3D(new MilMatrixTransform3DCommand(handle, matrix));
    internal static byte[] WriteMeshGeometry3D(uint handle, ReadOnlySpan<MilPoint3F> positions, ReadOnlySpan<MilPoint3F> normals, ReadOnlySpan<MilPoint2F> textureCoordinates, ReadOnlySpan<uint> indices)
    {
        int positionsSize = checked(positions.Length * Marshal.SizeOf<MilPoint3F>()); int normalsSize = checked(normals.Length * Marshal.SizeOf<MilPoint3F>()); int textureSize = checked(textureCoordinates.Length * Marshal.SizeOf<MilPoint2F>()); int indicesSize = checked(indices.Length * sizeof(uint));
        MilMeshGeometry3DCommand command = new(handle, (uint)positionsSize, (uint)normalsSize, (uint)textureSize, (uint)indicesSize); byte[] packet = new byte[Marshal.SizeOf<MilMeshGeometry3DCommand>() + positionsSize + normalsSize + textureSize + indicesSize]; MemoryMarshal.Write(packet, in command); int offset = Marshal.SizeOf<MilMeshGeometry3DCommand>(); MemoryMarshal.AsBytes(positions).CopyTo(packet.AsSpan(offset)); offset += positionsSize; MemoryMarshal.AsBytes(normals).CopyTo(packet.AsSpan(offset)); offset += normalsSize; MemoryMarshal.AsBytes(textureCoordinates).CopyTo(packet.AsSpan(offset)); offset += textureSize; MemoryMarshal.AsBytes(indices).CopyTo(packet.AsSpan(offset)); return packet;
    }
    private static byte[] Write3D<T>(T command) where T : unmanaged { byte[] packet = new byte[Marshal.SizeOf<T>()]; MemoryMarshal.Write(packet, in command); return packet; }
    private static byte[] WriteGroup3D<T>(T command, ReadOnlySpan<uint> children) where T : unmanaged { byte[] packet = new byte[Marshal.SizeOf<T>() + children.Length * sizeof(uint)]; MemoryMarshal.Write(packet, in command); children.CopyTo(MemoryMarshal.Cast<byte, uint>(packet.AsSpan(Marshal.SizeOf<T>()))); return packet; }
}

internal readonly record struct Generated3DDependency(int Offset, Func<MilResourceType, bool> Accepts);

internal abstract class GeneratedFixed3DResource : GeneratedDependencyResource
{
    private readonly int _size; private readonly Generated3DDependency[] _descriptors;
    internal GeneratedFixed3DResource(MilResourceType type, int size, params Generated3DDependency[] descriptors) : base(type) { _size = size; _descriptors = descriptors; }
    protected abstract Action PrepareState(ReadOnlySpan<byte> value);
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length != _size) return Direct3D9Factory.UceMalformedPacketHResult;
        GeneratedProtocolResource?[] dependencies = new GeneratedProtocolResource?[_descriptors.Length];
        for (int i = 0; i < _descriptors.Length; i++) { Generated3DDependency descriptor = _descriptors[i]; uint handle = MemoryMarshal.Read<uint>(value[descriptor.Offset..]); if (!TryResolve(table, handle, descriptor.Accepts, out dependencies[i])) return Direct3D9Factory.UceMalformedPacketHResult; }
        Action commitState = PrepareState(value);
        return CommitDependencies(dependencies, commitState);
    }
    internal static bool IsTransform3D(MilResourceType type) => type is >= MilResourceType.Transform3DGroup and <= MilResourceType.MatrixTransform3D;
    internal static bool IsModel3D(MilResourceType type) => type is >= MilResourceType.Model3DGroup and <= MilResourceType.GeometryModel3D;
    internal static bool IsMaterial(MilResourceType type) => type is >= MilResourceType.MaterialGroup and <= MilResourceType.EmissiveMaterial;
    internal static bool IsBrush(MilResourceType type) => GeneratedEffectResource.IsBrush(type);
}

internal abstract class GeneratedFixed3DResource<TCommand>(MilResourceType type, int size, params Generated3DDependency[] descriptors)
    : GeneratedFixed3DResource(type, size, descriptors) where TCommand : unmanaged
{
    internal TCommand Data { get; private set; }

    protected override Action PrepareState(ReadOnlySpan<byte> value)
    {
        Span<byte> packet = stackalloc byte[Marshal.SizeOf<TCommand>()];
        packet.Clear();
        value.CopyTo(packet[8..]);
        TCommand data = MemoryMarshal.Read<TCommand>(packet);
        return () => Data = data;
    }
}

internal sealed class GeneratedAxisAngleRotation3DResource() : GeneratedFixed3DResource<MilAxisAngleRotation3DCommand>(MilResourceType.AxisAngleRotation3D, 28, new(20, static t => t == MilResourceType.Vector3DResource), new(24, static t => t == MilResourceType.DoubleResource));
internal sealed class GeneratedQuaternionRotation3DResource() : GeneratedFixed3DResource<MilQuaternionRotation3DCommand>(MilResourceType.QuaternionRotation3D, 20, new Generated3DDependency(16, static t => t == MilResourceType.QuaternionResource));
internal sealed class GeneratedPerspectiveCameraResource() : GeneratedFixed3DResource<MilProjectionCameraCommand>(MilResourceType.PerspectiveCamera, 88, new Generated3DDependency(36, IsTransform3D), new(52, static t => t == MilResourceType.DoubleResource), new(68, static t => t == MilResourceType.DoubleResource), new(72, static t => t == MilResourceType.Point3DResource), new(76, static t => t == MilResourceType.Vector3DResource), new(80, static t => t == MilResourceType.Vector3DResource), new(84, static t => t == MilResourceType.DoubleResource));
internal sealed class GeneratedOrthographicCameraResource() : GeneratedFixed3DResource<MilProjectionCameraCommand>(MilResourceType.OrthographicCamera, 88, new Generated3DDependency(36, IsTransform3D), new(52, static t => t == MilResourceType.DoubleResource), new(68, static t => t == MilResourceType.DoubleResource), new(72, static t => t == MilResourceType.Point3DResource), new(76, static t => t == MilResourceType.Vector3DResource), new(80, static t => t == MilResourceType.Vector3DResource), new(84, static t => t == MilResourceType.DoubleResource));
internal sealed class GeneratedMatrixCameraResource() : GeneratedFixed3DResource<MilMatrixCameraCommand>(MilResourceType.MatrixCamera, 132, new Generated3DDependency(128, IsTransform3D));
internal sealed class GeneratedAmbientLightResource() : GeneratedFixed3DResource<MilAmbientLightCommand>(MilResourceType.AmbientLight, 24, new Generated3DDependency(16, IsTransform3D), new(20, static t => t == MilResourceType.ColorResource));
internal sealed class GeneratedDirectionalLightResource() : GeneratedFixed3DResource<MilDirectionalLightCommand>(MilResourceType.DirectionalLight, 40, new Generated3DDependency(28, IsTransform3D), new(32, static t => t == MilResourceType.ColorResource), new(36, static t => t == MilResourceType.Vector3DResource));
internal sealed class GeneratedPointLightResource() : GeneratedFixed3DResource<MilPointLightCommand>(MilResourceType.PointLight, 88, new Generated3DDependency(60, IsTransform3D), new(64, static t => t == MilResourceType.ColorResource), new(68, static t => t == MilResourceType.Point3DResource), new(72, static t => t == MilResourceType.DoubleResource), new(76, static t => t == MilResourceType.DoubleResource), new(80, static t => t == MilResourceType.DoubleResource), new(84, static t => t == MilResourceType.DoubleResource));
internal sealed class GeneratedSpotLightResource() : GeneratedFixed3DResource<MilSpotLightCommand>(MilResourceType.SpotLight, 128, new Generated3DDependency(76, IsTransform3D), new(92, static t => t == MilResourceType.ColorResource), new(96, static t => t == MilResourceType.Point3DResource), new(100, static t => t == MilResourceType.DoubleResource), new(104, static t => t == MilResourceType.DoubleResource), new(108, static t => t == MilResourceType.DoubleResource), new(112, static t => t == MilResourceType.DoubleResource), new(116, static t => t == MilResourceType.Vector3DResource), new(120, static t => t == MilResourceType.DoubleResource), new(124, static t => t == MilResourceType.DoubleResource));
internal sealed class GeneratedGeometryModel3DResource() : GeneratedFixed3DResource<MilGeometryModel3DCommand>(MilResourceType.GeometryModel3D, 16, new Generated3DDependency(0, IsTransform3D), new(4, static t => t == MilResourceType.MeshGeometry3D), new Generated3DDependency(8, IsMaterial), new Generated3DDependency(12, IsMaterial));
internal sealed class GeneratedDiffuseMaterialResource() : GeneratedFixed3DResource<MilDiffuseMaterialCommand>(MilResourceType.DiffuseMaterial, 36, new Generated3DDependency(32, IsBrush));
internal sealed class GeneratedSpecularMaterialResource() : GeneratedFixed3DResource<MilSpecularMaterialCommand>(MilResourceType.SpecularMaterial, 28, new Generated3DDependency(24, IsBrush));
internal sealed class GeneratedEmissiveMaterialResource() : GeneratedFixed3DResource<MilEmissiveMaterialCommand>(MilResourceType.EmissiveMaterial, 20, new Generated3DDependency(16, IsBrush));
internal sealed class GeneratedTranslateTransform3DResource() : GeneratedFixed3DResource<MilTranslateTransform3DCommand>(MilResourceType.TranslateTransform3D, 36, new(24, static t => t == MilResourceType.DoubleResource), new(28, static t => t == MilResourceType.DoubleResource), new(32, static t => t == MilResourceType.DoubleResource));
internal sealed class GeneratedScaleTransform3DResource() : GeneratedFixed3DResource<MilScaleTransform3DCommand>(MilResourceType.ScaleTransform3D, 72, new(48, static t => t == MilResourceType.DoubleResource), new(52, static t => t == MilResourceType.DoubleResource), new(56, static t => t == MilResourceType.DoubleResource), new(60, static t => t == MilResourceType.DoubleResource), new(64, static t => t == MilResourceType.DoubleResource), new(68, static t => t == MilResourceType.DoubleResource));
internal sealed class GeneratedRotateTransform3DResource() : GeneratedFixed3DResource<MilRotateTransform3DCommand>(MilResourceType.RotateTransform3D, 40, new(24, static t => t == MilResourceType.DoubleResource), new(28, static t => t == MilResourceType.DoubleResource), new(32, static t => t == MilResourceType.DoubleResource), new(36, static t => t is MilResourceType.AxisAngleRotation3D or MilResourceType.QuaternionRotation3D));
internal sealed class GeneratedMatrixTransform3DResource() : GeneratedFixed3DResource<MilMatrixTransform3DCommand>(MilResourceType.MatrixTransform3D, 64);

internal sealed class Generated3DGroupResource : GeneratedDependencyResource
{
    private readonly Func<MilResourceType, bool> _accepts; private readonly int _headerSize; private readonly int _sizeOffset; private readonly int? _singleDependencyOffset;
    internal Generated3DGroupResource(MilResourceType type, int headerSize, int sizeOffset, Func<MilResourceType, bool> accepts, int? singleDependencyOffset = null) : base(type) { _headerSize = headerSize; _sizeOffset = sizeOffset; _accepts = accepts; _singleDependencyOffset = singleDependencyOffset; }
    internal IReadOnlyList<GeneratedProtocolResource> Children { get; private set; } = [];
    internal GeneratedProtocolResource? Transform { get; private set; }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length < _headerSize) return Direct3D9Factory.UceMalformedPacketHResult; uint declared = MemoryMarshal.Read<uint>(value[_sizeOffset..]); ReadOnlySpan<byte> payload = value[_headerSize..]; if (declared != payload.Length || payload.Length % sizeof(uint) != 0) return Direct3D9Factory.UceMalformedPacketHResult;
        List<GeneratedProtocolResource?> dependencies = []; if (_singleDependencyOffset is int offset) { uint handle = MemoryMarshal.Read<uint>(value[offset..]); if (!TryResolve(table, handle, GeneratedFixed3DResource.IsTransform3D, out GeneratedProtocolResource? transform)) return Direct3D9Factory.UceMalformedPacketHResult; dependencies.Add(transform); }
        List<GeneratedProtocolResource> children = []; foreach (uint handle in MemoryMarshal.Cast<byte, uint>(payload)) { if (handle == 0 || !TryResolve(table, handle, _accepts, out GeneratedProtocolResource? child) || child is null) return Direct3D9Factory.UceMalformedPacketHResult; children.Add(child); dependencies.Add(child); }
        GeneratedProtocolResource[] committed = [.. children];
        GeneratedProtocolResource? committedTransform = _singleDependencyOffset.HasValue ? dependencies[0] : null;
        return CommitDependencies([.. dependencies], () => { Children = committed; Transform = committedTransform; });
    }
}

internal sealed class GeneratedMeshGeometry3DResource : GeneratedDependencyResource
{
    internal GeneratedMeshGeometry3DResource() : base(MilResourceType.MeshGeometry3D) { }
    internal MilPoint3F[] Positions { get; private set; } = []; internal MilPoint3F[] Normals { get; private set; } = []; internal MilPoint2F[] TextureCoordinates { get; private set; } = []; internal uint[] TriangleIndices { get; private set; } = [];
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (value.Length < 16) return Direct3D9Factory.UceMalformedPacketHResult; uint ps = MemoryMarshal.Read<uint>(value); uint ns = MemoryMarshal.Read<uint>(value[4..]); uint ts = MemoryMarshal.Read<uint>(value[8..]); uint isz = MemoryMarshal.Read<uint>(value[12..]); long total = 16L + ps + ns + ts + isz;
        if (total != value.Length || ps % 12 != 0 || ns % 12 != 0 || ts % 8 != 0 || isz % 4 != 0) return Direct3D9Factory.UceMalformedPacketHResult; int offset = 16; MilPoint3F[] positions = MemoryMarshal.Cast<byte, MilPoint3F>(value.Slice(offset, (int)ps)).ToArray(); offset += (int)ps; MilPoint3F[] normals = MemoryMarshal.Cast<byte, MilPoint3F>(value.Slice(offset, (int)ns)).ToArray(); offset += (int)ns; MilPoint2F[] texture = MemoryMarshal.Cast<byte, MilPoint2F>(value.Slice(offset, (int)ts)).ToArray(); offset += (int)ts; uint[] indices = MemoryMarshal.Cast<byte, uint>(value.Slice(offset, (int)isz)).ToArray();
        return CommitDependencies([], () => { Positions = positions; Normals = normals; TextureCoordinates = texture; TriangleIndices = indices; });
    }
}
