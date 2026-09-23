namespace WpfGfxShape.Core;

internal sealed record Direct3D9ProductionVideoDrawOperations(
    Direct3D9BitmapDrawState BitmapDrawState,
    nint Effects,
    Direct3D9ProductionBitmapDrawOperations BitmapOperations);