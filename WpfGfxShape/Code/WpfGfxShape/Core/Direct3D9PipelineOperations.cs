namespace WpfGfxShape.Core;

internal delegate int Direct3D9FlushPipelineVertexBuilder(out nint vertexBuffer);
internal delegate int Direct3D9SendGeometryToVertexBuilder(Direct3D9VertexBufferBuilder vertexBuilder);

internal sealed record Direct3D9PrimaryColorSource(
    Func<Direct3D9PipelineOperationSender, int> SendOperations);

internal sealed class Direct3D9PipelineOperationSender
{
    private readonly Func<int> _processEffects;
    private readonly Func<int> _sendGeometryModifiers;
    private readonly Func<int> _sendLighting;
    private readonly Func<int> _processClip;
    private readonly Func<Direct3D9ConstantColorSource, int>? _setConstant;
    private readonly Func<Direct3D9BitmapPipelineColorSource, int>? _setTexture;

    internal Direct3D9PipelineOperationSender(
        Func<int> processEffects,
        Func<int> sendGeometryModifiers,
        Func<int> sendLighting,
        Func<int> processClip,
        Func<Direct3D9ConstantColorSource, int>? setConstant = null,
        Func<Direct3D9BitmapPipelineColorSource, int>? setTexture = null)
    {
        ArgumentNullException.ThrowIfNull(processEffects);
        ArgumentNullException.ThrowIfNull(sendGeometryModifiers);
        ArgumentNullException.ThrowIfNull(sendLighting);
        ArgumentNullException.ThrowIfNull(processClip);

        _processEffects = processEffects;
        _sendGeometryModifiers = sendGeometryModifiers;
        _sendLighting = sendLighting;
        _processClip = processClip;
        _setConstant = setConstant;
        _setTexture = setTexture;
    }

    internal int SetConstant(Direct3D9ConstantColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        return _setConstant?.Invoke(colorSource) ?? Direct3D9Factory.InvalidCallHResult;
    }

    internal int SetTexture(Direct3D9BitmapPipelineColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        if (_setTexture is null)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        Direct3D9BitmapPipelineColorSource ownedColorSource = colorSource.AddRef();
        int result = _setTexture(ownedColorSource);
        if (result < 0)
        {
            ownedColorSource.Dispose();
        }

        return result;
    }

    internal int SendPipelineOperations(Direct3D9PrimaryColorSource primaryColorSource)
    {
        ArgumentNullException.ThrowIfNull(primaryColorSource);
        ArgumentNullException.ThrowIfNull(primaryColorSource.SendOperations);

        int result = primaryColorSource.SendOperations(this);
        if (result < 0)
        {
            return result;
        }

        result = _processEffects();
        if (result < 0)
        {
            return result;
        }

        result = _sendGeometryModifiers();
        if (result < 0)
        {
            return result;
        }

        result = _sendLighting();
        return result < 0 ? result : _processClip();
    }
}
