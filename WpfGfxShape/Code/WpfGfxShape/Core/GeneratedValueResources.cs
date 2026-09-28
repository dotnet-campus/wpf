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

internal abstract class GeneratedUpdatableResource : GeneratedProtocolResource
{
    protected GeneratedUpdatableResource(MilResourceType resourceType) : base(resourceType) { }
    internal abstract int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value);
}

internal abstract class GeneratedValueResource : GeneratedUpdatableResource
{
    protected GeneratedValueResource(MilResourceType resourceType) : base(resourceType) { }
}

internal sealed class GeneratedValueResource<T> : GeneratedValueResource where T : unmanaged
{
    internal GeneratedValueResource(MilResourceType resourceType) : base(resourceType) { }
    internal T Value { get; private set; }

    internal override int ProcessUpdate(GeneratedProtocolHandleTable handleTable, ReadOnlySpan<byte> value)
    {
        ArgumentNullException.ThrowIfNull(handleTable);
        if (value.Length != Marshal.SizeOf<T>())
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        Value = MemoryMarshal.Read<T>(value);
        NotifyChanged();
        return Direct3D9Factory.SuccessHResult;
    }
}

internal static class GeneratedResourceFactory
{
    internal static MilResourceType GetResourceType(MilCommand command) => command switch
    {
        MilCommand.PixelShader => MilResourceType.PixelShader,
        MilCommand.ImplicitInputBrush => MilResourceType.ImplicitInputBrush,
        MilCommand.BlurEffect => MilResourceType.BlurEffect,
        MilCommand.DropShadowEffect => MilResourceType.DropShadowEffect,
        MilCommand.ShaderEffect => MilResourceType.ShaderEffect,
        MilCommand.RenderData => MilResourceType.RenderData,
        MilCommand.AxisAngleRotation3D => MilResourceType.AxisAngleRotation3D,
        MilCommand.QuaternionRotation3D => MilResourceType.QuaternionRotation3D,
        MilCommand.PerspectiveCamera => MilResourceType.PerspectiveCamera,
        MilCommand.OrthographicCamera => MilResourceType.OrthographicCamera,
        MilCommand.MatrixCamera => MilResourceType.MatrixCamera,
        MilCommand.Model3DGroup => MilResourceType.Model3DGroup,
        MilCommand.AmbientLight => MilResourceType.AmbientLight,
        MilCommand.DirectionalLight => MilResourceType.DirectionalLight,
        MilCommand.PointLight => MilResourceType.PointLight,
        MilCommand.SpotLight => MilResourceType.SpotLight,
        MilCommand.GeometryModel3D => MilResourceType.GeometryModel3D,
        MilCommand.MeshGeometry3D => MilResourceType.MeshGeometry3D,
        MilCommand.MaterialGroup => MilResourceType.MaterialGroup,
        MilCommand.DiffuseMaterial => MilResourceType.DiffuseMaterial,
        MilCommand.SpecularMaterial => MilResourceType.SpecularMaterial,
        MilCommand.EmissiveMaterial => MilResourceType.EmissiveMaterial,
        MilCommand.Transform3DGroup => MilResourceType.Transform3DGroup,
        MilCommand.TranslateTransform3D => MilResourceType.TranslateTransform3D,
        MilCommand.ScaleTransform3D => MilResourceType.ScaleTransform3D,
        MilCommand.RotateTransform3D => MilResourceType.RotateTransform3D,
        MilCommand.MatrixTransform3D => MilResourceType.MatrixTransform3D,
        MilCommand.DoubleResource => MilResourceType.DoubleResource,
        MilCommand.ColorResource => MilResourceType.ColorResource,
        MilCommand.PointResource => MilResourceType.PointResource,
        MilCommand.RectResource => MilResourceType.RectResource,
        MilCommand.SizeResource => MilResourceType.SizeResource,
        MilCommand.MatrixResource => MilResourceType.MatrixResource,
        MilCommand.Point3DResource => MilResourceType.Point3DResource,
        MilCommand.Vector3DResource => MilResourceType.Vector3DResource,
        MilCommand.QuaternionResource => MilResourceType.QuaternionResource,
        MilCommand.TransformGroup => MilResourceType.TransformGroup,
        MilCommand.TranslateTransform => MilResourceType.TranslateTransform,
        MilCommand.ScaleTransform => MilResourceType.ScaleTransform,
        MilCommand.SkewTransform => MilResourceType.SkewTransform,
        MilCommand.RotateTransform => MilResourceType.RotateTransform,
        MilCommand.MatrixTransform => MilResourceType.MatrixTransform,
        MilCommand.LineGeometry => MilResourceType.LineGeometry,
        MilCommand.RectangleGeometry => MilResourceType.RectangleGeometry,
        MilCommand.EllipseGeometry => MilResourceType.EllipseGeometry,
        MilCommand.GeometryGroup => MilResourceType.GeometryGroup,
        MilCommand.CombinedGeometry => MilResourceType.CombinedGeometry,
        MilCommand.PathGeometry => MilResourceType.PathGeometry,
        MilCommand.SolidColorBrush => MilResourceType.SolidColorBrush,
        MilCommand.LinearGradientBrush => MilResourceType.LinearGradientBrush,
        MilCommand.RadialGradientBrush => MilResourceType.RadialGradientBrush,
        MilCommand.ImageBrush => MilResourceType.ImageBrush,
        MilCommand.DrawingBrush => MilResourceType.DrawingBrush,
        MilCommand.VisualBrush => MilResourceType.VisualBrush,
        MilCommand.BitmapCacheBrush => MilResourceType.BitmapCacheBrush,
        MilCommand.DashStyle => MilResourceType.DashStyle,
        MilCommand.Pen => MilResourceType.Pen,
        MilCommand.GeometryDrawing => MilResourceType.GeometryDrawing,
        MilCommand.GlyphRunDrawing => MilResourceType.GlyphRunDrawing,
        MilCommand.DrawingImage => MilResourceType.DrawingImage,
        MilCommand.ImageDrawing => MilResourceType.ImageDrawing,
        MilCommand.VideoDrawing => MilResourceType.VideoDrawing,
        MilCommand.DrawingGroup => MilResourceType.DrawingGroup,
        MilCommand.GuidelineSet => MilResourceType.GuidelineSet,
        MilCommand.BitmapCache => MilResourceType.BitmapCache,
        _ => MilResourceType.Null
    };

    internal static int Create(MilResourceType resourceType, out GeneratedProtocolResource? resource)
    {
        resource = resourceType switch
        {
            MilResourceType.PixelShader => new GeneratedPixelShaderResource(),
            MilResourceType.ImplicitInputBrush => new GeneratedImplicitInputBrushResource(),
            MilResourceType.BlurEffect => new GeneratedBlurEffectResource(),
            MilResourceType.DropShadowEffect => new GeneratedDropShadowEffectResource(),
            MilResourceType.ShaderEffect => new GeneratedShaderEffectResource(),
            MilResourceType.AxisAngleRotation3D => new GeneratedAxisAngleRotation3DResource(),
            MilResourceType.QuaternionRotation3D => new GeneratedQuaternionRotation3DResource(),
            MilResourceType.PerspectiveCamera => new GeneratedPerspectiveCameraResource(),
            MilResourceType.OrthographicCamera => new GeneratedOrthographicCameraResource(),
            MilResourceType.MatrixCamera => new GeneratedMatrixCameraResource(),
            MilResourceType.Model3DGroup => new Generated3DGroupResource(resourceType, 8, 4, static type => type is >= MilResourceType.Model3DGroup and <= MilResourceType.GeometryModel3D, 0),
            MilResourceType.AmbientLight => new GeneratedAmbientLightResource(),
            MilResourceType.DirectionalLight => new GeneratedDirectionalLightResource(),
            MilResourceType.PointLight => new GeneratedPointLightResource(),
            MilResourceType.SpotLight => new GeneratedSpotLightResource(),
            MilResourceType.GeometryModel3D => new GeneratedGeometryModel3DResource(),
            MilResourceType.MeshGeometry3D => new GeneratedMeshGeometry3DResource(),
            MilResourceType.MaterialGroup => new Generated3DGroupResource(resourceType, 4, 0, static type => type is >= MilResourceType.MaterialGroup and <= MilResourceType.EmissiveMaterial),
            MilResourceType.DiffuseMaterial => new GeneratedDiffuseMaterialResource(),
            MilResourceType.SpecularMaterial => new GeneratedSpecularMaterialResource(),
            MilResourceType.EmissiveMaterial => new GeneratedEmissiveMaterialResource(),
            MilResourceType.Transform3DGroup => new Generated3DGroupResource(resourceType, 4, 0, static type => type is >= MilResourceType.Transform3DGroup and <= MilResourceType.MatrixTransform3D),
            MilResourceType.TranslateTransform3D => new GeneratedTranslateTransform3DResource(),
            MilResourceType.ScaleTransform3D => new GeneratedScaleTransform3DResource(),
            MilResourceType.RotateTransform3D => new GeneratedRotateTransform3DResource(),
            MilResourceType.MatrixTransform3D => new GeneratedMatrixTransform3DResource(),
            MilResourceType.DoubleResource => new GeneratedValueResource<double>(resourceType),
            MilResourceType.ColorResource => new GeneratedValueResource<MilColorF>(resourceType),
            MilResourceType.PointResource => new GeneratedValueResource<MilPoint2D>(resourceType),
            MilResourceType.RectResource => new GeneratedValueResource<MilRectD>(resourceType),
            MilResourceType.SizeResource => new GeneratedValueResource<MilSizeD>(resourceType),
            MilResourceType.MatrixResource => new GeneratedValueResource<MilMatrix3x2D>(resourceType),
            MilResourceType.Point3DResource => new GeneratedValueResource<MilPoint3F>(resourceType),
            MilResourceType.Vector3DResource => new GeneratedValueResource<MilPoint3F>(resourceType),
            MilResourceType.QuaternionResource => new GeneratedValueResource<MilQuaternionF>(resourceType),
            MilResourceType.TransformGroup => new GeneratedTransformGroupResource(),
            MilResourceType.TranslateTransform => new GeneratedTranslateTransformResource(),
            MilResourceType.ScaleTransform => new GeneratedScaleTransformResource(),
            MilResourceType.SkewTransform => new GeneratedSkewTransformResource(),
            MilResourceType.RotateTransform => new GeneratedRotateTransformResource(),
            MilResourceType.MatrixTransform => new GeneratedMatrixTransformResource(),
            MilResourceType.LineGeometry => new GeneratedLineGeometryResource(),
            MilResourceType.RectangleGeometry => new GeneratedRectangleGeometryResource(),
            MilResourceType.EllipseGeometry => new GeneratedEllipseGeometryResource(),
            MilResourceType.GeometryGroup => new GeneratedGeometryGroupResource(),
            MilResourceType.CombinedGeometry => new GeneratedCombinedGeometryResource(),
            MilResourceType.PathGeometry => new GeneratedPathGeometryResource(),
            MilResourceType.SolidColorBrush => new GeneratedSolidColorBrushResource(),
            MilResourceType.LinearGradientBrush => new GeneratedLinearGradientBrushResource(),
            MilResourceType.RadialGradientBrush => new GeneratedRadialGradientBrushResource(),
            MilResourceType.ImageBrush => new GeneratedImageBrushResource(),
            MilResourceType.DrawingBrush => new GeneratedDrawingBrushResource(),
            MilResourceType.VisualBrush => new GeneratedVisualBrushResource(),
            MilResourceType.BitmapCacheBrush => new GeneratedBitmapCacheBrushResource(),
            MilResourceType.DashStyle => new GeneratedDashStyleResource(),
            MilResourceType.Pen => new GeneratedPenResource(),
            MilResourceType.GeometryDrawing => new GeneratedGeometryDrawingResource(),
            MilResourceType.GlyphRunDrawing => new GeneratedGlyphRunDrawingResource(),
            MilResourceType.MediaPlayer => new GeneratedMediaPlayerResource(),
            MilResourceType.BitmapSource => new GeneratedBitmapSourceResource(),
            MilResourceType.DrawingImage => new GeneratedDrawingImageResource(),
            MilResourceType.ImageDrawing => new GeneratedImageDrawingResource(),
            MilResourceType.VideoDrawing => new GeneratedVideoDrawingResource(),
            MilResourceType.DrawingGroup => new GeneratedDrawingGroupResource(),
            MilResourceType.GuidelineSet => new GeneratedGuidelineSetResource(),
            MilResourceType.BitmapCache => new GeneratedBitmapCacheResource(),
            MilResourceType.RenderData => new GeneratedRenderDataResource(),
            MilResourceType.Visual => new GeneratedVisualResource(),
            MilResourceType.Viewport3DVisual => new GeneratedViewport3DVisualResource(),
            MilResourceType.Visual3D => new GeneratedVisual3DResource(),
            MilResourceType.RenderTarget or MilResourceType.HwndRenderTarget or MilResourceType.GenericRenderTarget => new GeneratedTargetResource(resourceType),
            MilResourceType.GlyphRun => new GeneratedProtocolResource(resourceType),
            > MilResourceType.Null and < MilResourceType.Last => new GeneratedProtocolResource(resourceType),
            _ => null
        };

        return resource is null
            ? Direct3D9Factory.UceMalformedPacketHResult
            : Direct3D9Factory.SuccessHResult;
    }
}
