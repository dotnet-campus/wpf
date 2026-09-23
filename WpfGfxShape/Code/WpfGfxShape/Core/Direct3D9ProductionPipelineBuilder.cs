namespace WpfGfxShape.Core;

internal sealed record Direct3D9ProductionPipelineBuildContext(
    Direct3D9PrimaryColorSource PrimaryColorSource,
    Func<Direct3D9ShaderPipelineItemBuilder, int>? ProcessShaderEffects = null,
    Func<Direct3D9FixedFunctionPipelineItemBuilder, int>? ProcessFixedFunctionEffects = null,
    Func<Direct3D9ShaderPipelineItemBuilder, int>? SendShaderGeometryModifiers = null,
    Func<Direct3D9FixedFunctionPipelineItemBuilder, int>? SendFixedFunctionGeometryModifiers = null,
    Func<Direct3D9ShaderPipelineItemBuilder, int>? SendShaderLighting = null,
    Func<Direct3D9FixedFunctionPipelineItemBuilder, int>? SendFixedFunctionLighting = null,
    Func<Direct3D9ShaderPipelineItemBuilder, int>? ProcessShaderClip = null,
    Func<Direct3D9FixedFunctionPipelineItemBuilder, int>? ProcessFixedFunctionClip = null,
    Direct3D9VertexFormatAttribute IncomingVertexFormat = Direct3D9VertexFormatAttribute.None);

internal static class Direct3D9ProductionPipelineBuilder
{
    internal static int BuildShader(
        Direct3D9ProductionPipelineBuildContext context,
        Direct3D9VertexPipelineOwner vertexOwner,
        Func<int> sendDeviceStates,
        Action releaseColorSources,
        out Direct3D9ProductionPipelineInitializer? initializer)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(vertexOwner);
        ArgumentNullException.ThrowIfNull(sendDeviceStates);
        ArgumentNullException.ThrowIfNull(releaseColorSources);

        initializer = null;
        using Direct3D9ShaderPipelineItemBuilder itemBuilder = new(context.IncomingVertexFormat);
        Direct3D9PipelineOperationSender sender = new(
            () => context.ProcessShaderEffects?.Invoke(itemBuilder) ?? Direct3D9Factory.SuccessHResult,
            () => context.SendShaderGeometryModifiers?.Invoke(itemBuilder) ?? Direct3D9Factory.SuccessHResult,
            () => context.SendShaderLighting?.Invoke(itemBuilder) ?? Direct3D9Factory.SuccessHResult,
            () => context.ProcessShaderClip?.Invoke(itemBuilder) ?? Direct3D9Factory.SuccessHResult,
            itemBuilder.SetConstant,
            itemBuilder.SetTexture);

        int result = sender.SendPipelineOperations(context.PrimaryColorSource);
        if (result < 0)
        {
            vertexOwner.Dispose();
            return result;
        }

        IReadOnlyList<Direct3D9ShaderPipelineItem> items = itemBuilder.DetachItems();
        try
        {
            initializer = Direct3D9ProductionPipelineInitializer.CreateShader(
                items,
                vertexOwner,
                sendDeviceStates,
                releaseColorSources);
            return Direct3D9Factory.SuccessHResult;
        }
        catch
        {
            Direct3D9ShaderPipelineItemBuilder.ReleaseOwnedColorSources(items);
            vertexOwner.Dispose();
            throw;
        }
    }

    internal static int BuildFixedFunction(
        Direct3D9ProductionPipelineBuildContext context,
        Direct3D9VertexPipelineOwner vertexOwner,
        Func<int> sendDeviceStates,
        Action releaseColorSources,
        out Direct3D9ProductionPipelineInitializer? initializer)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(vertexOwner);
        ArgumentNullException.ThrowIfNull(sendDeviceStates);
        ArgumentNullException.ThrowIfNull(releaseColorSources);

        initializer = null;
        using Direct3D9FixedFunctionPipelineItemBuilder itemBuilder = new(context.IncomingVertexFormat);
        Direct3D9PipelineOperationSender sender = new(
            () => context.ProcessFixedFunctionEffects?.Invoke(itemBuilder) ?? Direct3D9Factory.SuccessHResult,
            () => context.SendFixedFunctionGeometryModifiers?.Invoke(itemBuilder) ?? Direct3D9Factory.SuccessHResult,
            () => context.SendFixedFunctionLighting?.Invoke(itemBuilder) ?? Direct3D9Factory.SuccessHResult,
            () => context.ProcessFixedFunctionClip?.Invoke(itemBuilder) ?? Direct3D9Factory.SuccessHResult,
            itemBuilder.SetConstant,
            itemBuilder.SetTexture);

        int result = sender.SendPipelineOperations(context.PrimaryColorSource);
        if (result < 0)
        {
            vertexOwner.Dispose();
            return result;
        }

        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items = itemBuilder.DetachItems();
        try
        {
            initializer = Direct3D9ProductionPipelineInitializer.CreateFixedFunction(
                items,
                vertexOwner,
                sendDeviceStates,
                releaseColorSources);
            return Direct3D9Factory.SuccessHResult;
        }
        catch
        {
            Direct3D9FixedFunctionPipelineItemBuilder.ReleaseOwnedColorSources(items);
            vertexOwner.Dispose();
            throw;
        }
    }
}
