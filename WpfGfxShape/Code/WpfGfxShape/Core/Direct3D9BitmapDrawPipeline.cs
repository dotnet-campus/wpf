namespace WpfGfxShape.Core;

internal sealed record Direct3D9ProductionBitmapDrawOperations(
    Direct3D9GetScratchBitmapBrush GetScratchBitmapBrush,
    Direct3D9SetScratchBitmapBrush SetScratchBitmapBrush,
    Action<nint> ClearScratchBitmapBrush,
    Func<nint, Direct3D9ImmediateBrushRealizer> CreateBrushRealizer,
    Direct3D9CreateBitmapShape CreateBitmapShape,
    Func<int> EnsureState,
    Direct3D9ClipToSafeDeviceBounds ClipToSafeDeviceBounds,
    Direct3D9SoftwareFillBitmapPath SoftwareFillPath,
    Direct3D9ApplyPathGuidelines ApplyGuidelines,
    Direct3D9ApplyPathBrushClip ApplyBrushClip,
    Direct3D9GetPathBoundsInDeviceSpace GetBoundsInDeviceSpace,
    Direct3D9CreatePathHardwareBrush CreateHardwareBrush,
    Direct3D9CreateTypedPathGeometryGenerator CreateAntialiasedGeometryGenerator,
    Direct3D9CreateTypedPathGeometryGenerator CreateAliasedGeometryGenerator,
    MilCompositingMode CompositingMode,
    MilAntiAliasMode AntiAliasMode,
    Direct3D9SurfaceRect CurrentClip);