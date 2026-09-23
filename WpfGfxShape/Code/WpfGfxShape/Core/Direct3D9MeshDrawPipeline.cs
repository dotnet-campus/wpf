namespace WpfGfxShape.Core;

internal sealed record Direct3D9ProductionMeshDrawOperations(
    Func<int> EnsureBrushRealizations,
    Direct3D9ApplyProjectedMeshTo2DState ApplyProjectedMeshTo2DState,
    Direct3D9DeriveMeshShader DeriveMeshShader,
    Func<Direct3D9ContextState, int>? EnsureState = null,
    Func<bool>? IsMeshBoundsDebugEnabled = null,
    Direct3D9GetMeshBounds? GetMeshBounds = null,
    Func<Direct3D9Box, int>? DrawMeshBounds = null);