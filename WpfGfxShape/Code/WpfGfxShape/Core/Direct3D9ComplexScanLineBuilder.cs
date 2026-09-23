namespace WpfGfxShape.Core;

internal enum Direct3D9ComplexScanLinePath
{
    DirectLineList,
    VertexBufferSink,
    WafflePipeline,
}

internal sealed class Direct3D9ComplexScanLineBuilder
{
    private readonly float _viewportTop;
    private readonly Direct3D9AddWaffleLine _directLineListSink;
    private readonly Direct3D9AddWaffleLine _vertexBufferSink;
    private readonly Direct3D9LineWafflePipeline _wafflePipeline;

    internal Direct3D9ComplexScanLineBuilder(
        float viewportTop,
        IReadOnlyList<Direct3D9WaffleTextureCoordinate> textureCoordinates,
        Direct3D9AddWaffleLine directLineListSink,
        Direct3D9AddWaffleLine vertexBufferSink)
    {
        ArgumentNullException.ThrowIfNull(textureCoordinates);
        ArgumentNullException.ThrowIfNull(directLineListSink);
        ArgumentNullException.ThrowIfNull(vertexBufferSink);

        _viewportTop = viewportTop;
        _directLineListSink = directLineListSink;
        _vertexBufferSink = vertexBufferSink;
        _wafflePipeline = Direct3D9WafflePipelineBuilder.BuildLine(textureCoordinates, vertexBufferSink);
    }

    internal bool WafflersUsed => _wafflePipeline.WafflersUsed;

    internal Direct3D9ComplexScanLinePath SelectPath(float pixelCenterY)
    {
        if (_wafflePipeline.WafflersUsed)
        {
            return Direct3D9ComplexScanLinePath.WafflePipeline;
        }

        return pixelCenterY < _viewportTop + 1f
            ? Direct3D9ComplexScanLinePath.VertexBufferSink
            : Direct3D9ComplexScanLinePath.DirectLineList;
    }

    internal int AddLine(Direct3D9WafflePoint start, Direct3D9WafflePoint end)
    {
        return SelectPath(start.Y) switch
        {
            Direct3D9ComplexScanLinePath.WafflePipeline => _wafflePipeline.Sink(start, end),
            Direct3D9ComplexScanLinePath.VertexBufferSink => _vertexBufferSink(start, end),
            _ => _directLineListSink(start, end),
        };
    }
}
