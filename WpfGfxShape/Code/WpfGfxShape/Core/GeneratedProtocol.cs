using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal static class GeneratedProtocolFingerprint
{
    internal const uint MilSdkVersion = 0x200184C0;
    internal const uint DwmSdkVersion = 0x0BDDCB2B;
    internal const int LastCommandId = 0x8D;
    internal const int LastResourceTypeId = 0x61;
}

internal enum MilCommand : uint
{
    Invalid = 0x00,
    TransportSyncFlush = 0x01,
    TransportDestroyResourcesOnChannel = 0x02,
    PartitionRegisterForNotifications = 0x03,
    ChannelRequestTier = 0x04,
    PartitionSetVBlankSyncMode = 0x05,
    PartitionNotifyPresent = 0x06,
    ChannelCreateResource = 0x07,
    ChannelDeleteResource = 0x08,
    ChannelDuplicateHandle = 0x09,
    D3DImage = 0x0A,
    D3DImagePresent = 0x0B,
    BitmapSource = 0x0C,
    BitmapInvalidate = 0x0D,
    DoubleResource = 0x0E,
    ColorResource = 0x0F,
    PointResource = 0x10,
    RectResource = 0x11,
    SizeResource = 0x12,
    MatrixResource = 0x13,
    Point3DResource = 0x14,
    Vector3DResource = 0x15,
    QuaternionResource = 0x16,
    MediaPlayer = 0x17,
    RenderData = 0x18,
    EtwEventResource = 0x19,
    VisualCreate = 0x1A,
    VisualSetOffset = 0x1B,
    VisualSetTransform = 0x1C,
    VisualSetEffect = 0x1D,
    VisualSetCacheMode = 0x1E,
    VisualSetClip = 0x1F,
    VisualSetAlpha = 0x20,
    VisualSetRenderOptions = 0x21,
    VisualSetContent = 0x22,
    VisualSetAlphaMask = 0x23,
    VisualRemoveAllChildren = 0x24,
    VisualRemoveChild = 0x25,
    VisualInsertChildAt = 0x26,
    VisualSetGuidelineCollection = 0x27,
    VisualSetScrollableAreaClip = 0x28,
    Viewport3DVisualSetCamera = 0x29,
    Viewport3DVisualSetViewport = 0x2A,
    Viewport3DVisualSet3DChild = 0x2B,
    Visual3DSetContent = 0x2C,
    Visual3DSetTransform = 0x2D,
    Visual3DRemoveAllChildren = 0x2E,
    Visual3DRemoveChild = 0x2F,
    Visual3DInsertChildAt = 0x30,
    HwndTargetCreate = 0x31,
    HwndTargetSuppressLayered = 0x32,
    TargetUpdateWindowSettings = 0x33,
    GenericTargetCreate = 0x34,
    TargetSetRoot = 0x35,
    TargetSetClearColor = 0x36,
    TargetInvalidate = 0x37,
    TargetSetFlags = 0x38,
    HwndTargetDpiChanged = 0x39,
    GlyphRunCreate = 0x3A,
    DoubleBufferedBitmap = 0x3B,
    DoubleBufferedBitmapCopyForward = 0x3C,
    PartitionNotifyPolicyChangeForNonInteractiveMode = 0x3D,
    DrawLine = 0x3E,
    DrawLineAnimate = 0x3F,
    DrawRectangle = 0x40,
    DrawRectangleAnimate = 0x41,
    DrawRoundedRectangle = 0x42,
    DrawRoundedRectangleAnimate = 0x43,
    DrawEllipse = 0x44,
    DrawEllipseAnimate = 0x45,
    DrawGeometry = 0x46,
    DrawImage = 0x47,
    DrawImageAnimate = 0x48,
    DrawGlyphRun = 0x49,
    DrawDrawing = 0x4A,
    DrawVideo = 0x4B,
    DrawVideoAnimate = 0x4C,
    PushClip = 0x4D,
    PushOpacityMask = 0x4E,
    PushOpacity = 0x4F,
    PushOpacityAnimate = 0x50,
    PushTransform = 0x51,
    PushGuidelineSet = 0x52,
    PushGuidelineY1 = 0x53,
    PushGuidelineY2 = 0x54,
    PushEffect = 0x55,
    Pop = 0x56,
    AxisAngleRotation3D = 0x57,
    QuaternionRotation3D = 0x58,
    PerspectiveCamera = 0x59,
    OrthographicCamera = 0x5A,
    MatrixCamera = 0x5B,
    Model3DGroup = 0x5C,
    AmbientLight = 0x5D,
    DirectionalLight = 0x5E,
    PointLight = 0x5F,
    SpotLight = 0x60,
    GeometryModel3D = 0x61,
    MeshGeometry3D = 0x62,
    MaterialGroup = 0x63,
    DiffuseMaterial = 0x64,
    SpecularMaterial = 0x65,
    EmissiveMaterial = 0x66,
    Transform3DGroup = 0x67,
    TranslateTransform3D = 0x68,
    ScaleTransform3D = 0x69,
    RotateTransform3D = 0x6A,
    MatrixTransform3D = 0x6B,
    PixelShader = 0x6C,
    ImplicitInputBrush = 0x6D,
    BlurEffect = 0x6E,
    DropShadowEffect = 0x6F,
    ShaderEffect = 0x70,
    DrawingImage = 0x71,
    TransformGroup = 0x72,
    TranslateTransform = 0x73,
    ScaleTransform = 0x74,
    SkewTransform = 0x75,
    RotateTransform = 0x76,
    MatrixTransform = 0x77,
    LineGeometry = 0x78,
    RectangleGeometry = 0x79,
    EllipseGeometry = 0x7A,
    GeometryGroup = 0x7B,
    CombinedGeometry = 0x7C,
    PathGeometry = 0x7D,
    SolidColorBrush = 0x7E,
    LinearGradientBrush = 0x7F,
    RadialGradientBrush = 0x80,
    ImageBrush = 0x81,
    DrawingBrush = 0x82,
    VisualBrush = 0x83,
    BitmapCacheBrush = 0x84,
    DashStyle = 0x85,
    Pen = 0x86,
    GeometryDrawing = 0x87,
    GlyphRunDrawing = 0x88,
    ImageDrawing = 0x89,
    VideoDrawing = 0x8A,
    DrawingGroup = 0x8B,
    GuidelineSet = 0x8C,
    BitmapCache = 0x8D
}

internal enum MilResourceType : uint
{
    Null = 0,
    MediaPlayer = 1,
    Rotation3D = 2,
    AxisAngleRotation3D = 3,
    QuaternionRotation3D = 4,
    Camera = 5,
    ProjectionCamera = 6,
    PerspectiveCamera = 7,
    OrthographicCamera = 8,
    MatrixCamera = 9,
    Model3D = 10,
    Model3DGroup = 11,
    Light = 12,
    AmbientLight = 13,
    DirectionalLight = 14,
    PointLightBase = 15,
    PointLight = 16,
    SpotLight = 17,
    GeometryModel3D = 18,
    Geometry3D = 19,
    MeshGeometry3D = 20,
    Material = 21,
    MaterialGroup = 22,
    DiffuseMaterial = 23,
    SpecularMaterial = 24,
    EmissiveMaterial = 25,
    Transform3D = 26,
    Transform3DGroup = 27,
    AffineTransform3D = 28,
    TranslateTransform3D = 29,
    ScaleTransform3D = 30,
    RotateTransform3D = 31,
    MatrixTransform3D = 32,
    PixelShader = 33,
    ImplicitInputBrush = 34,
    Effect = 35,
    BlurEffect = 36,
    DropShadowEffect = 37,
    ShaderEffect = 38,
    Visual = 39,
    Viewport3DVisual = 40,
    Visual3D = 41,
    GlyphRun = 42,
    RenderData = 43,
    DrawingContext = 44,
    RenderTarget = 45,
    HwndRenderTarget = 46,
    GenericRenderTarget = 47,
    EtwEventResource = 48,
    DoubleResource = 49,
    ColorResource = 50,
    PointResource = 51,
    RectResource = 52,
    SizeResource = 53,
    MatrixResource = 54,
    Point3DResource = 55,
    Vector3DResource = 56,
    QuaternionResource = 57,
    ImageSource = 58,
    DrawingImage = 59,
    Transform = 60,
    TransformGroup = 61,
    TranslateTransform = 62,
    ScaleTransform = 63,
    SkewTransform = 64,
    RotateTransform = 65,
    MatrixTransform = 66,
    Geometry = 67,
    LineGeometry = 68,
    RectangleGeometry = 69,
    EllipseGeometry = 70,
    GeometryGroup = 71,
    CombinedGeometry = 72,
    PathGeometry = 73,
    Brush = 74,
    SolidColorBrush = 75,
    GradientBrush = 76,
    LinearGradientBrush = 77,
    RadialGradientBrush = 78,
    TileBrush = 79,
    ImageBrush = 80,
    DrawingBrush = 81,
    VisualBrush = 82,
    BitmapCacheBrush = 83,
    DashStyle = 84,
    Pen = 85,
    Drawing = 86,
    GeometryDrawing = 87,
    GlyphRunDrawing = 88,
    ImageDrawing = 89,
    VideoDrawing = 90,
    DrawingGroup = 91,
    GuidelineSet = 92,
    CacheMode = 93,
    BitmapCache = 94,
    BitmapSource = 95,
    DoubleBufferedBitmap = 96,
    D3DImage = 97,
    Last = 98,
    ForceDword = uint.MaxValue
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
internal readonly struct MilTransportSyncFlushCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;

    internal MilTransportSyncFlushCommand(MilCommand type) => Type = type;
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
internal readonly struct MilTransportDestroyResourcesOnChannelCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] internal readonly uint Channel;

    internal MilTransportDestroyResourcesOnChannelCommand(MilCommand type, uint channel)
    {
        Type = type;
        Channel = channel;
    }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
internal readonly struct MilChannelCreateResourceCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] internal readonly uint Handle;
    [FieldOffset(8)] internal readonly MilResourceType ResourceType;

    internal MilChannelCreateResourceCommand(MilCommand type, uint handle, MilResourceType resourceType)
    {
        Type = type;
        Handle = handle;
        ResourceType = resourceType;
    }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
internal readonly struct MilChannelDeleteResourceCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] internal readonly uint Handle;
    [FieldOffset(8)] internal readonly MilResourceType ResourceType;

    internal MilChannelDeleteResourceCommand(MilCommand type, uint handle, MilResourceType resourceType)
    {
        Type = type;
        Handle = handle;
        ResourceType = resourceType;
    }
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16)]
internal readonly struct MilChannelDuplicateHandleCommand
{
    [FieldOffset(0)] internal readonly MilCommand Type;
    [FieldOffset(4)] internal readonly uint Original;
    [FieldOffset(8)] internal readonly uint TargetChannel;
    [FieldOffset(12)] internal readonly uint Duplicate;

    internal MilChannelDuplicateHandleCommand(MilCommand type, uint original, uint targetChannel, uint duplicate)
    {
        Type = type;
        Original = original;
        TargetChannel = targetChannel;
        Duplicate = duplicate;
    }
}

internal static partial class GeneratedProtocolPacketWriter
{
    internal static byte[] WriteTransportSyncFlush()
    {
        return Write(new MilTransportSyncFlushCommand(MilCommand.TransportSyncFlush));
    }

    internal static byte[] WriteTransportDestroyResourcesOnChannel(uint channel)
    {
        return Write(new MilTransportDestroyResourcesOnChannelCommand(
            MilCommand.TransportDestroyResourcesOnChannel,
            channel));
    }

    internal static byte[] WriteChannelCreateResource(uint handle, MilResourceType resourceType)
    {
        return Write(new MilChannelCreateResourceCommand(MilCommand.ChannelCreateResource, handle, resourceType));
    }

    internal static byte[] WriteChannelDeleteResource(uint handle, MilResourceType resourceType)
    {
        return Write(new MilChannelDeleteResourceCommand(MilCommand.ChannelDeleteResource, handle, resourceType));
    }

    internal static byte[] WriteChannelDuplicateHandle(uint original, uint targetChannel, uint duplicate)
    {
        return Write(new MilChannelDuplicateHandleCommand(
            MilCommand.ChannelDuplicateHandle,
            original,
            targetChannel,
            duplicate));
    }

    private static byte[] Write<T>(T command)
        where T : unmanaged
    {
        byte[] packet = new byte[Marshal.SizeOf<T>()];
        MemoryMarshal.Write(packet, in command);
        return packet;
    }
}

internal delegate int GeneratedResourceUpdateHandler(MilCommand command, uint handle, ReadOnlySpan<byte> value);
internal delegate int GeneratedStateCommandHandler(MilCommand command, uint handle, ReadOnlySpan<byte> packet);

internal sealed record GeneratedProtocolHandlers(
    Func<int> TransportSyncFlush,
    Func<uint, int> TransportDestroyResourcesOnChannel,
    Func<uint, MilResourceType, int> ChannelCreateResource,
    Func<uint, MilResourceType, int> ChannelDeleteResource,
    Func<uint, uint, uint, int> ChannelDuplicateHandle,
    GeneratedResourceUpdateHandler? ResourceUpdate = null,
    GeneratedStateCommandHandler? StateCommand = null);

internal sealed class GeneratedProtocolRouter
{
    private readonly GeneratedProtocolHandlers _handlers;

    internal GeneratedProtocolRouter(GeneratedProtocolHandlers handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        _handlers = handlers;
    }

    internal int ProcessPacket(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < sizeof(uint))
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        MilCommand command = (MilCommand) BinaryPrimitives.ReadUInt32LittleEndian(packet);
        return command switch
        {
            MilCommand.TransportSyncFlush => ProcessExact<MilTransportSyncFlushCommand>(packet, _ => _handlers.TransportSyncFlush()),
            MilCommand.TransportDestroyResourcesOnChannel => ProcessExact<MilTransportDestroyResourcesOnChannelCommand>(packet, value => _handlers.TransportDestroyResourcesOnChannel(value.Channel)),
            MilCommand.ChannelCreateResource => ProcessExact<MilChannelCreateResourceCommand>(packet, value => _handlers.ChannelCreateResource(value.Handle, value.ResourceType)),
            MilCommand.ChannelDeleteResource => ProcessExact<MilChannelDeleteResourceCommand>(packet, value => _handlers.ChannelDeleteResource(value.Handle, value.ResourceType)),
            MilCommand.ChannelDuplicateHandle => ProcessExact<MilChannelDuplicateHandleCommand>(packet, value => _handlers.ChannelDuplicateHandle(value.Original, value.TargetChannel, value.Duplicate)),
            MilCommand.VisualCreate or MilCommand.VisualSetOffset or MilCommand.VisualSetTransform or MilCommand.VisualSetClip or MilCommand.VisualSetAlpha or MilCommand.VisualSetRenderOptions or MilCommand.VisualSetContent or MilCommand.VisualSetAlphaMask or MilCommand.VisualRemoveAllChildren or MilCommand.VisualRemoveChild or MilCommand.VisualInsertChildAt or MilCommand.Viewport3DVisualSetCamera or MilCommand.Viewport3DVisualSetViewport or MilCommand.Viewport3DVisualSet3DChild or MilCommand.Visual3DSetContent or MilCommand.Visual3DSetTransform or MilCommand.Visual3DRemoveAllChildren or MilCommand.Visual3DRemoveChild or MilCommand.Visual3DInsertChildAt or MilCommand.TargetSetRoot or MilCommand.TargetSetClearColor or MilCommand.TargetInvalidate or MilCommand.TargetSetFlags => ProcessStateCommand(packet, command),
            MilCommand.GenericTargetCreate or MilCommand.BitmapSource or MilCommand.BitmapInvalidate or MilCommand.MediaPlayer or MilCommand.DoubleBufferedBitmap or MilCommand.DoubleBufferedBitmapCopyForward => ProcessStateCommand(packet, command),
            MilCommand.PixelShader => ProcessVariableResourceUpdate<MilPixelShaderCommand>(packet, command),
            MilCommand.ImplicitInputBrush => ProcessResourceUpdate<MilImplicitInputBrushCommand>(packet, command),
            MilCommand.BlurEffect => ProcessResourceUpdate<MilBlurEffectCommand>(packet, command),
            MilCommand.DropShadowEffect => ProcessResourceUpdate<MilDropShadowEffectCommand>(packet, command),
            MilCommand.ShaderEffect => ProcessVariableResourceUpdate<MilShaderEffectCommand>(packet, command),
            MilCommand.RenderData => ProcessVariableResourceUpdate<MilRenderDataCommand>(packet, command),
            MilCommand.AxisAngleRotation3D => ProcessResourceUpdate<MilAxisAngleRotation3DCommand>(packet, command),
            MilCommand.QuaternionRotation3D => ProcessResourceUpdate<MilQuaternionRotation3DCommand>(packet, command),
            MilCommand.PerspectiveCamera or MilCommand.OrthographicCamera => ProcessResourceUpdate<MilProjectionCameraCommand>(packet, command),
            MilCommand.MatrixCamera => ProcessResourceUpdate<MilMatrixCameraCommand>(packet, command),
            MilCommand.Model3DGroup => ProcessVariableResourceUpdate<MilModel3DGroupCommand>(packet, command),
            MilCommand.AmbientLight => ProcessResourceUpdate<MilAmbientLightCommand>(packet, command),
            MilCommand.DirectionalLight => ProcessResourceUpdate<MilDirectionalLightCommand>(packet, command),
            MilCommand.PointLight => ProcessResourceUpdate<MilPointLightCommand>(packet, command),
            MilCommand.SpotLight => ProcessResourceUpdate<MilSpotLightCommand>(packet, command),
            MilCommand.GeometryModel3D => ProcessResourceUpdate<MilGeometryModel3DCommand>(packet, command),
            MilCommand.MeshGeometry3D => ProcessVariableResourceUpdate<MilMeshGeometry3DCommand>(packet, command),
            MilCommand.MaterialGroup or MilCommand.Transform3DGroup => ProcessVariableResourceUpdate<MilResourceGroup3DCommand>(packet, command),
            MilCommand.DiffuseMaterial => ProcessResourceUpdate<MilDiffuseMaterialCommand>(packet, command),
            MilCommand.SpecularMaterial => ProcessResourceUpdate<MilSpecularMaterialCommand>(packet, command),
            MilCommand.EmissiveMaterial => ProcessResourceUpdate<MilEmissiveMaterialCommand>(packet, command),
            MilCommand.TranslateTransform3D => ProcessResourceUpdate<MilTranslateTransform3DCommand>(packet, command),
            MilCommand.ScaleTransform3D => ProcessResourceUpdate<MilScaleTransform3DCommand>(packet, command),
            MilCommand.RotateTransform3D => ProcessResourceUpdate<MilRotateTransform3DCommand>(packet, command),
            MilCommand.MatrixTransform3D => ProcessResourceUpdate<MilMatrixTransform3DCommand>(packet, command),
            MilCommand.DoubleResource => ProcessResourceUpdate<MilDoubleResourceCommand>(packet, command),
            MilCommand.ColorResource => ProcessResourceUpdate<MilColorResourceCommand>(packet, command),
            MilCommand.PointResource => ProcessResourceUpdate<MilPointResourceCommand>(packet, command),
            MilCommand.RectResource => ProcessResourceUpdate<MilRectResourceCommand>(packet, command),
            MilCommand.SizeResource => ProcessResourceUpdate<MilSizeResourceCommand>(packet, command),
            MilCommand.MatrixResource => ProcessResourceUpdate<MilMatrixResourceCommand>(packet, command),
            MilCommand.Point3DResource => ProcessResourceUpdate<MilPoint3DResourceCommand>(packet, command),
            MilCommand.Vector3DResource => ProcessResourceUpdate<MilVector3DResourceCommand>(packet, command),
            MilCommand.QuaternionResource => ProcessResourceUpdate<MilQuaternionResourceCommand>(packet, command),
            MilCommand.TransformGroup => ProcessVariableResourceUpdate<MilTransformGroupCommand>(packet, command),
            MilCommand.TranslateTransform => ProcessResourceUpdate<MilTranslateTransformCommand>(packet, command),
            MilCommand.ScaleTransform => ProcessResourceUpdate<MilScaleTransformCommand>(packet, command),
            MilCommand.SkewTransform => ProcessResourceUpdate<MilSkewTransformCommand>(packet, command),
            MilCommand.RotateTransform => ProcessResourceUpdate<MilRotateTransformCommand>(packet, command),
            MilCommand.MatrixTransform => ProcessResourceUpdate<MilMatrixTransformCommand>(packet, command),
            MilCommand.LineGeometry => ProcessResourceUpdate<MilLineGeometryCommand>(packet, command),
            MilCommand.RectangleGeometry => ProcessResourceUpdate<MilRectangleGeometryCommand>(packet, command),
            MilCommand.EllipseGeometry => ProcessResourceUpdate<MilEllipseGeometryCommand>(packet, command),
            MilCommand.GeometryGroup => ProcessVariableResourceUpdate<MilGeometryGroupCommand>(packet, command),
            MilCommand.CombinedGeometry => ProcessResourceUpdate<MilCombinedGeometryCommand>(packet, command),
            MilCommand.PathGeometry => ProcessVariableResourceUpdate<MilPathGeometryCommand>(packet, command),
            MilCommand.SolidColorBrush => ProcessResourceUpdate<MilSolidColorBrushCommand>(packet, command),
            MilCommand.LinearGradientBrush => ProcessVariableResourceUpdate<MilLinearGradientBrushCommand>(packet, command),
            MilCommand.RadialGradientBrush => ProcessVariableResourceUpdate<MilRadialGradientBrushCommand>(packet, command),
            MilCommand.ImageBrush => ProcessResourceUpdate<MilTileBrushCommand>(packet, command),
            MilCommand.DrawingBrush => ProcessResourceUpdate<MilTileBrushCommand>(packet, command),
            MilCommand.VisualBrush => ProcessResourceUpdate<MilTileBrushCommand>(packet, command),
            MilCommand.BitmapCacheBrush => ProcessResourceUpdate<MilBitmapCacheBrushCommand>(packet, command),
            MilCommand.DashStyle => ProcessVariableResourceUpdate<MilDashStyleCommand>(packet, command),
            MilCommand.Pen => ProcessResourceUpdate<MilPenCommand>(packet, command),
            MilCommand.GeometryDrawing => ProcessResourceUpdate<MilGeometryDrawingCommand>(packet, command),
            MilCommand.GlyphRunDrawing => ProcessResourceUpdate<MilGlyphRunDrawingCommand>(packet, command),
            MilCommand.DrawingImage => ProcessResourceUpdate<MilDrawingImageCommand>(packet, command),
            MilCommand.ImageDrawing => ProcessResourceUpdate<MilImageDrawingCommand>(packet, command),
            MilCommand.VideoDrawing => ProcessResourceUpdate<MilVideoDrawingCommand>(packet, command),
            MilCommand.DrawingGroup => ProcessVariableResourceUpdate<MilDrawingGroupCommand>(packet, command),
            MilCommand.GuidelineSet => ProcessVariableResourceUpdate<MilGuidelineSetCommand>(packet, command),
            MilCommand.BitmapCache => ProcessResourceUpdate<MilBitmapCacheCommand>(packet, command),
            _ => Direct3D9Factory.UceMalformedPacketHResult
        };
    }

    internal int ProcessPackets(IEnumerable<ReadOnlyMemory<byte>> packets)
    {
        ArgumentNullException.ThrowIfNull(packets);
        foreach (ReadOnlyMemory<byte> packet in packets)
        {
            int result = ProcessPacket(packet.Span);
            if (result < 0)
            {
                return result;
            }
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private int ProcessStateCommand(ReadOnlySpan<byte> packet, MilCommand command)
    {
        if (_handlers.StateCommand is null)
        {
            return Direct3D9Factory.UceUnknownPacketHResult;
        }

        return packet.Length < 8
            ? Direct3D9Factory.UceMalformedPacketHResult
            : _handlers.StateCommand(command, BinaryPrimitives.ReadUInt32LittleEndian(packet[4..]), packet);
    }

    private int ProcessResourceUpdate<T>(ReadOnlySpan<byte> packet, MilCommand command)
        where T : unmanaged, IMilResourceUpdateCommand
    {
        if (_handlers.ResourceUpdate is null)
        {
            return Direct3D9Factory.UceUnknownPacketHResult;
        }

        if (packet.Length != Marshal.SizeOf<T>())
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        T value = MemoryMarshal.Read<T>(packet);
        return _handlers.ResourceUpdate(command, value.Handle, packet[8..]);
    }

    private int ProcessVariableResourceUpdate<T>(ReadOnlySpan<byte> packet, MilCommand command)
        where T : unmanaged, IMilResourceUpdateCommand
    {
        if (_handlers.ResourceUpdate is null)
        {
            return Direct3D9Factory.UceUnknownPacketHResult;
        }

        int commandSize = Marshal.SizeOf<T>();
        if (packet.Length < commandSize)
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        T value = MemoryMarshal.Read<T>(packet);
        return _handlers.ResourceUpdate(command, value.Handle, packet[8..]);
    }

    private static int ProcessExact<T>(ReadOnlySpan<byte> packet, Func<T, int> handler)
        where T : unmanaged
    {
        if (packet.Length != Marshal.SizeOf<T>())
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        T command = MemoryMarshal.Read<T>(packet);
        return handler(command);
    }
}

internal class GeneratedProtocolResource
{
    private readonly List<GeneratedProtocolResource> _listeners = [];
    private bool _isNotifying;

    internal GeneratedProtocolResource(MilResourceType resourceType)
    {
        ResourceType = resourceType;
        ReferenceCount = 1;
    }

    internal MilResourceType ResourceType { get; }

    internal int ReferenceCount { get; private set; }

    internal int ChangeCount { get; private set; }

    internal bool IsReleased => ReferenceCount == 0;

    internal void AddRef() => ReferenceCount++;

    internal void Release()
    {
        ReferenceCount--;
        if (ReferenceCount == 0)
        {
            OnFinalRelease();
        }
    }

    internal void AddListener(GeneratedProtocolResource listener)
    {
        _listeners.Add(listener);
        AddRef();
    }

    internal void RemoveListener(GeneratedProtocolResource listener)
    {
        if (_listeners.Remove(listener))
        {
            Release();
        }
    }

    protected void NotifyChanged()
    {
        if (_isNotifying)
        {
            return;
        }

        _isNotifying = true;
        try
        {
            ChangeCount++;
            GeneratedProtocolResource[] listeners = [.. _listeners];
            foreach (GeneratedProtocolResource listener in listeners)
            {
                listener.NotifyChanged();
            }
        }
        finally
        {
            _isNotifying = false;
        }
    }

    protected virtual void OnFinalRelease()
    {
    }
}

internal sealed class GeneratedProtocolHandleTable
{
    private readonly Dictionary<uint, GeneratedProtocolResource> _resources = [];
    private readonly IVideoCompositionOwner? _videoComposition;
    private readonly GeneratedTargetRegistry? _targets;

    internal GeneratedProtocolHandleTable(IVideoCompositionOwner? videoComposition = null, GeneratedTargetRegistry? targets = null)
    {
        _videoComposition = videoComposition;
        _targets = targets;
    }

    internal int Create(uint handle, MilResourceType resourceType)
    {
        if (handle == 0 || resourceType is MilResourceType.Null or >= MilResourceType.Last || _resources.ContainsKey(handle))
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        int result;
        GeneratedProtocolResource? resource;
        if (resourceType == MilResourceType.MediaPlayer && _videoComposition is not null)
        {
            resource = new GeneratedMediaPlayerResource(_videoComposition);
            result = 0;
        }
        else
        {
            result = GeneratedResourceFactory.Create(resourceType, out resource);
        }

        if (result < 0)
        {
            return result;
        }

        _resources.Add(handle, resource!);
        return Direct3D9Factory.SuccessHResult;
    }

    internal void ReleaseAll()
    {
        while (_resources.Count != 0)
        {
            using var enumerator = _resources.GetEnumerator();
            enumerator.MoveNext();
            KeyValuePair<uint, GeneratedProtocolResource> entry = enumerator.Current;
            _resources.Remove(entry.Key);
            entry.Value.Release();
        }
    }

    internal int Delete(uint handle, MilResourceType resourceType)
    {
        if (!_resources.TryGetValue(handle, out GeneratedProtocolResource? resource)
            || resource.ResourceType != resourceType)
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        _resources.Remove(handle);
        if (resource is GeneratedTargetResource target) _targets?.Remove(target);
        resource.Release();
        return Direct3D9Factory.SuccessHResult;
    }

    internal int DuplicateTo(uint original, GeneratedProtocolHandleTable target, uint duplicate)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (!_resources.TryGetValue(original, out GeneratedProtocolResource? resource)
            || duplicate == 0
            || target._resources.ContainsKey(duplicate))
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        resource.AddRef();
        target._resources.Add(duplicate, resource);
        return Direct3D9Factory.SuccessHResult;
    }

    internal bool TryGetResource(uint handle, out GeneratedProtocolResource? resource)
    {
        return _resources.TryGetValue(handle, out resource);
    }

    internal bool TryGetResource(uint handle, MilResourceType resourceType, out GeneratedProtocolResource? resource)
    {
        if (handle == 0)
        {
            resource = null;
            return true;
        }

        return _resources.TryGetValue(handle, out resource) && resource.ResourceType == resourceType;
    }

    internal int ProcessStateCommand(MilCommand command, uint handle, ReadOnlySpan<byte> packet)
    {
        if (!_resources.TryGetValue(handle, out GeneratedProtocolResource? resource))
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        if (command == MilCommand.GenericTargetCreate && resource is GeneratedTargetResource genericTarget)
        {
            int result = genericTarget.ProcessCommand(this, command, packet);
            if (result >= 0) _targets?.Add(genericTarget);
            return result;
        }

        return resource switch
        {
            GeneratedDoubleBufferedBitmapResource doubleBitmap when command is MilCommand.DoubleBufferedBitmap or MilCommand.DoubleBufferedBitmapCopyForward => doubleBitmap.ProcessCommand(command, packet),
            GeneratedMediaPlayerResource media when command == MilCommand.MediaPlayer => media.ProcessCommand(packet),
            GeneratedBitmapSourceResource bitmap when command is MilCommand.BitmapSource or MilCommand.BitmapInvalidate => bitmap.ProcessCommand(command, packet),
            GeneratedVisualResource visual when command is >= MilCommand.VisualCreate and <= MilCommand.VisualInsertChildAt => visual.ProcessCommand(this, command, packet),
            GeneratedViewport3DVisualResource viewport when command is >= MilCommand.Viewport3DVisualSetCamera and <= MilCommand.Viewport3DVisualSet3DChild => viewport.ProcessCommand(this, command, packet),
            GeneratedVisual3DResource visual3D when command is >= MilCommand.Visual3DSetContent and <= MilCommand.Visual3DInsertChildAt => visual3D.ProcessCommand(this, command, packet),
            GeneratedTargetResource target when command is MilCommand.GenericTargetCreate or (>= MilCommand.TargetSetRoot and <= MilCommand.TargetSetFlags) => target.ProcessCommand(this, command, packet),
            _ => Direct3D9Factory.UceMalformedPacketHResult
        };
    }

    internal int ProcessUpdate(MilCommand command, uint handle, ReadOnlySpan<byte> value)
    {
        MilResourceType expectedType = GeneratedResourceFactory.GetResourceType(command);
        if (expectedType == MilResourceType.Null
            || !_resources.TryGetValue(handle, out GeneratedProtocolResource? resource)
            || resource.ResourceType != expectedType
            || resource is not GeneratedUpdatableResource updatableResource)
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        return updatableResource.ProcessUpdate(this, value);
    }
}

internal sealed class GeneratedProtocolChannelRegistry
{
    private readonly Dictionary<uint, GeneratedProtocolHandleTable> _channels = [];

    internal bool TryAdd(uint channel, GeneratedProtocolHandleTable table)
    {
        ArgumentNullException.ThrowIfNull(table);
        return channel != 0 && _channels.TryAdd(channel, table);
    }

    internal int Count => _channels.Count;

    internal void Remove(uint channel)
    {
        _channels.Remove(channel);
    }

    internal bool TryGet(uint channel, out GeneratedProtocolHandleTable? table)
    {
        return _channels.TryGetValue(channel, out table);
    }
}

internal sealed class GeneratedProtocolProductionContext
{
    private readonly uint _currentChannel;
    private readonly GeneratedProtocolChannelRegistry _channels;
    private readonly Func<int> _syncFlush;
    private readonly Func<uint, int> _destroyResources;

    internal GeneratedProtocolProductionContext(
        uint currentChannel,
        GeneratedProtocolChannelRegistry channels,
        Func<int>? syncFlush = null,
        Func<uint, int>? destroyResources = null)
    {
        ArgumentNullException.ThrowIfNull(channels);
        _currentChannel = currentChannel;
        _channels = channels;
        _syncFlush = syncFlush ?? (() => Direct3D9Factory.SuccessHResult);
        _destroyResources = destroyResources ?? (_ => Direct3D9Factory.SuccessHResult);
    }

    internal GeneratedProtocolRouter CreateRouter()
    {
        return new GeneratedProtocolRouter(new GeneratedProtocolHandlers(
            _syncFlush,
            _destroyResources,
            CreateResource,
            DeleteResource,
            DuplicateHandle,
            UpdateResource,
            ProcessStateCommand));
    }

    private int CreateResource(uint handle, MilResourceType resourceType)
    {
        return TryGetCurrentTable(out GeneratedProtocolHandleTable? table)
            ? table.Create(handle, resourceType)
            : Direct3D9Factory.UceHandleLookupFailedHResult;
    }

    private int DeleteResource(uint handle, MilResourceType resourceType)
    {
        return TryGetCurrentTable(out GeneratedProtocolHandleTable? table)
            ? table.Delete(handle, resourceType)
            : Direct3D9Factory.UceHandleLookupFailedHResult;
    }

    private int DuplicateHandle(uint original, uint targetChannel, uint duplicate)
    {
        if (!TryGetCurrentTable(out GeneratedProtocolHandleTable? source)
            || !_channels.TryGet(targetChannel, out GeneratedProtocolHandleTable? target))
        {
            return Direct3D9Factory.UceHandleLookupFailedHResult;
        }

        return source.DuplicateTo(original, target, duplicate);
    }

    private int UpdateResource(MilCommand command, uint handle, ReadOnlySpan<byte> value)
    {
        return TryGetCurrentTable(out GeneratedProtocolHandleTable? table)
            ? table.ProcessUpdate(command, handle, value)
            : Direct3D9Factory.UceHandleLookupFailedHResult;
    }

    private int ProcessStateCommand(MilCommand command, uint handle, ReadOnlySpan<byte> packet)
    {
        return TryGetCurrentTable(out GeneratedProtocolHandleTable? table)
            ? table.ProcessStateCommand(command, handle, packet)
            : Direct3D9Factory.UceHandleLookupFailedHResult;
    }

    private bool TryGetCurrentTable(out GeneratedProtocolHandleTable? table)
    {
        return _channels.TryGet(_currentChannel, out table);
    }
}
