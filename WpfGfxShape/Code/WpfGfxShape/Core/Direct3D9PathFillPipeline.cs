namespace WpfGfxShape.Core;

internal sealed class Direct3D9PathGeometryGenerator : IDisposable
{
    private readonly Action _release;
    private bool _disposed;

    internal Direct3D9PathGeometryGenerator(
        Direct3D9SendGeometryToVertexBuilder sendGeometry,
        Action release,
        bool verticesArePreGenerated = false)
    {
        ArgumentNullException.ThrowIfNull(sendGeometry);
        ArgumentNullException.ThrowIfNull(release);
        SendGeometry = sendGeometry;
        _release = release;
        VerticesArePreGenerated = verticesArePreGenerated;
    }

    internal Direct3D9SendGeometryToVertexBuilder SendGeometry { get; }

    internal bool VerticesArePreGenerated { get; }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _release();
    }
}

internal sealed class Direct3D9PathHardwareBrush : IDisposable
{
    private readonly Direct3D9CreatePathPipelineDescription _createShaderPipeline;
    private readonly Direct3D9CreatePathPipelineDescription _createFixedFunctionPipeline;
    private readonly Direct3D9CreateTypedPathPipelineDescription? _createTypedShaderPipeline;
    private readonly Direct3D9CreateTypedPathPipelineDescription? _createTypedFixedFunctionPipeline;
    private readonly Action _release;
    private bool _disposed;

    internal Direct3D9PathHardwareBrush(
        Direct3D9CreatePathPipelineDescription createShaderPipeline,
        Direct3D9CreatePathPipelineDescription createFixedFunctionPipeline,
        Action release)
    {
        ArgumentNullException.ThrowIfNull(createShaderPipeline);
        ArgumentNullException.ThrowIfNull(createFixedFunctionPipeline);
        ArgumentNullException.ThrowIfNull(release);
        _createShaderPipeline = createShaderPipeline;
        _createFixedFunctionPipeline = createFixedFunctionPipeline;
        _release = release;
    }

    internal Direct3D9PathHardwareBrush(
        Direct3D9CreateTypedPathPipelineDescription createShaderPipeline,
        Direct3D9CreateTypedPathPipelineDescription createFixedFunctionPipeline,
        Action release)
        : this(
            static (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint _, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) =>
            {
                description = null;
                return Direct3D9Factory.UnsupportedOperationHResult;
            },
            static (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint _, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) =>
            {
                description = null;
                return Direct3D9Factory.UnsupportedOperationHResult;
            },
            release)
    {
        ArgumentNullException.ThrowIfNull(createShaderPipeline);
        ArgumentNullException.ThrowIfNull(createFixedFunctionPipeline);
        _createTypedShaderPipeline = createShaderPipeline;
        _createTypedFixedFunctionPipeline = createFixedFunctionPipeline;
    }

    internal int CreateShaderPipeline(
        MilCompositingMode compositingMode,
        nint effects,
        Direct3D9PathBrushContext effectContext,
        out Direct3D9PathPipelineDescription? description) =>
        _createShaderPipeline(compositingMode, this, effects, effectContext, out description);

    internal int CreateFixedFunctionPipeline(
        MilCompositingMode compositingMode,
        nint effects,
        Direct3D9PathBrushContext effectContext,
        out Direct3D9PathPipelineDescription? description) =>
        _createFixedFunctionPipeline(compositingMode, this, effects, effectContext, out description);

    internal int CreateShaderPipeline(
        MilCompositingMode compositingMode,
        Direct3D9EffectList? effects,
        Direct3D9PathBrushContext effectContext,
        out Direct3D9PathPipelineDescription? description) =>
        _createTypedShaderPipeline is null
            ? CreateShaderPipeline(compositingMode, 0, effectContext, out description)
            : _createTypedShaderPipeline(compositingMode, this, effects, effectContext, out description);

    internal int CreateFixedFunctionPipeline(
        MilCompositingMode compositingMode,
        Direct3D9EffectList? effects,
        Direct3D9PathBrushContext effectContext,
        out Direct3D9PathPipelineDescription? description) =>
        _createTypedFixedFunctionPipeline is null
            ? CreateFixedFunctionPipeline(compositingMode, 0, effectContext, out description)
            : _createTypedFixedFunctionPipeline(compositingMode, this, effects, effectContext, out description);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _release();
    }
}

internal sealed record Direct3D9PathPipelineDescription(
    Direct3D9ProductionPipelineInitializer Initializer,
    Direct3D9SurfaceRect? OutsideBounds,
    bool NeedInside);

internal sealed record Direct3D9ProductionPathDrawOperations(
    Direct3D9EnsurePathBrushRealization EnsureBrushRealization,
    Direct3D9GetPathShapeBounds GetShapeBounds,
    Direct3D9WidenPathShape WidenShape,
    Direct3D9GetPathRealizedBrush GetRealizedBrush,
    Direct3D9ClipToSafeDeviceBounds ClipToSafeDeviceBounds,
    Func<int> EnsureState,
    Direct3D9SoftwareFillPath SoftwareFillPath,
    Direct3D9ApplyPathGuidelines ApplyGuidelines,
    Direct3D9ApplyPathBrushClip ApplyBrushClip,
    Direct3D9GetPathBoundsInDeviceSpace GetBoundsInDeviceSpace,
    Direct3D9CreatePathHardwareBrush CreateHardwareBrush,
    Direct3D9CreateTypedPathGeometryGenerator CreateAntialiasedGeometryGenerator,
    Direct3D9CreateTypedPathGeometryGenerator CreateAliasedGeometryGenerator,
    MilCompositingMode CompositingMode,
    MilAntiAliasMode AntiAliasMode,
    Direct3D9SurfaceRect CurrentClip);

internal delegate int Direct3D9CreatePathHardwareBrush(
    nint brush,
    Direct3D9PathBrushContext brushContext,
    out Direct3D9PathHardwareBrush? hardwareBrush);

internal delegate int Direct3D9CreateTypedPathGeometryGenerator(
    Direct3D9PathClipperState clipperState,
    out Direct3D9PathGeometryGenerator? geometryGenerator);

internal delegate int Direct3D9CreatePathPipelineDescription(
    MilCompositingMode compositingMode,
    Direct3D9PathHardwareBrush hardwareBrush,
    nint effects,
    Direct3D9PathBrushContext effectContext,
    out Direct3D9PathPipelineDescription? description);

internal delegate int Direct3D9CreateTypedPathPipelineDescription(
    MilCompositingMode compositingMode,
    Direct3D9PathHardwareBrush hardwareBrush,
    Direct3D9EffectList? effects,
    Direct3D9PathBrushContext effectContext,
    out Direct3D9PathPipelineDescription? description);

internal delegate int Direct3D9CreateOwnedPathPipelineDescription(
    Direct3D9PathHardwareBrush hardwareBrush,
    MilCompositingMode compositingMode,
    nint effects,
    Direct3D9PathBrushContext effectContext,
    out Direct3D9PathPipelineDescription? description);

internal delegate int Direct3D9CreateOwnedEffectPathPipelineDescription(
    Direct3D9PathHardwareBrush hardwareBrush,
    MilCompositingMode compositingMode,
    Direct3D9EffectList? effects,
    Direct3D9PathBrushContext effectContext,
    out Direct3D9PathPipelineDescription? description);

internal delegate int Direct3D9ExecuteTypedAcceleratedFillPath(
    Direct3D9PathGeometryGenerator geometryGenerator,
    Direct3D9PathHardwareBrush hardwareBrush,
    nint effects,
    Direct3D9PathBrushContext brushContext);

internal delegate int Direct3D9ExecuteEffectAcceleratedFillPath(
    Direct3D9PathGeometryGenerator geometryGenerator,
    Direct3D9PathHardwareBrush hardwareBrush,
    Direct3D9EffectList? effects,
    Direct3D9PathBrushContext brushContext);

internal sealed class Direct3D9LayerMaskShape : IDisposable
{
    private readonly Action _release;
    private bool _disposed;

    internal Direct3D9LayerMaskShape(nint handle, Action release)
    {
        ArgumentOutOfRangeException.ThrowIfZero(handle);
        ArgumentNullException.ThrowIfNull(release);
        Handle = handle;
        _release = release;
    }

    internal nint Handle { get; }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _release();
    }
}

internal sealed class Direct3D9LayerEffectList : IDisposable
{
    private readonly Action _release;
    private bool _disposed;

    internal Direct3D9LayerEffectList(nint handle, Action release)
    {
        ArgumentOutOfRangeException.ThrowIfZero(handle);
        ArgumentNullException.ThrowIfNull(release);
        Handle = handle;
        _release = release;
    }

    internal nint Handle { get; }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _release();
    }
}
