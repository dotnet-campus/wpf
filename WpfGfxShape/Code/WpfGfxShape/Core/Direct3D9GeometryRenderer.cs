using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal readonly record struct Direct3D9GeometryRenderBatch(
    uint StartVertex,
    uint StartIndex,
    uint PrimitiveCount,
    bool NeedsRender);

[SupportedOSPlatform("windows5.1.2600")]
internal unsafe ref struct Direct3D9GeometryRenderer<TDiffuseOrNormal>
    where TDiffuseOrNormal : unmanaged
{
    private ReadOnlySpan<Vector3> _positions;
    private ReadOnlySpan<TDiffuseOrNormal> _diffuseColorsOrNormals;
    private ReadOnlySpan<Vector2> _textureCoordinates;
    private ReadOnlySpan<uint> _indices;
    private readonly TDiffuseOrNormal _defaultDiffuseOrNormal;
    private readonly Direct3D9CreateLightingColorSource? _createLightingColorSource;
    private uint _inputVertexCount;
    private uint _inputIndexCount;
    private uint _renderedIndexCount;
    private uint _indexedVertexStart;

    internal Direct3D9GeometryRenderer(
        ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<TDiffuseOrNormal> diffuseColorsOrNormals,
        ReadOnlySpan<Vector2> textureCoordinates,
        ReadOnlySpan<uint> indices,
        TDiffuseOrNormal defaultDiffuseOrNormal,
        Direct3D9CreateLightingColorSource? createLightingColorSource = null)
    {
        if (positions.IsEmpty)
        {
            throw new ArgumentException("Position data cannot be empty.", nameof(positions));
        }

        if (textureCoordinates.Length != positions.Length)
        {
            throw new ArgumentException("Texture coordinate count must match the position count.", nameof(textureCoordinates));
        }

        if (!diffuseColorsOrNormals.IsEmpty && diffuseColorsOrNormals.Length != positions.Length)
        {
            throw new ArgumentException("Diffuse color or normal count must match the position count.", nameof(diffuseColorsOrNormals));
        }

        uint inputIndexCount = indices.IsEmpty ? (uint) positions.Length : (uint) indices.Length;
        if (inputIndexCount % 3 != 0)
        {
            throw new ArgumentException("Triangle geometry must contain a multiple of three vertices or indices.", nameof(indices));
        }

        _positions = default;
        _diffuseColorsOrNormals = default;
        _textureCoordinates = default;
        _indices = default;
        _defaultDiffuseOrNormal = defaultDiffuseOrNormal;
        _createLightingColorSource = createLightingColorSource;
        _inputVertexCount = 0;
        _inputIndexCount = 0;
        _renderedIndexCount = 0;
        _indexedVertexStart = 0;

        SetArrays(positions, diffuseColorsOrNormals, textureCoordinates, (uint) positions.Length, indices, inputIndexCount);
    }

    internal void SetArrays(
        ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<TDiffuseOrNormal> diffuseColorsOrNormals,
        ReadOnlySpan<Vector2> textureCoordinates,
        uint vertexCount,
        ReadOnlySpan<uint> indices,
        uint indexCount)
    {
        _positions = positions;
        _diffuseColorsOrNormals = diffuseColorsOrNormals;
        _textureCoordinates = textureCoordinates;
        _indices = indices;
        _inputVertexCount = vertexCount;
        _inputIndexCount = indexCount;
        _renderedIndexCount = 0;
    }

    internal uint VertexStride => (uint) (sizeof(Vector3) + sizeof(TDiffuseOrNormal) + sizeof(Vector2));

    internal ReadOnlySpan<TDiffuseOrNormal> DiffuseColorsOrNormals => _diffuseColorsOrNormals;

    internal TDiffuseOrNormal DefaultDiffuseOrNormal => _defaultDiffuseOrNormal;

    internal Direct3D9VertexFormatAttribute PerVertexDataType => typeof(TDiffuseOrNormal) == typeof(Vector3)
        ? Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Normal | Direct3D9VertexFormatAttribute.Uv1
        : typeof(TDiffuseOrNormal) == typeof(uint)
            ? Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Diffuse | Direct3D9VertexFormatAttribute.Uv1
            : throw new NotSupportedException($"Unsupported diffuse or normal vertex data type: {typeof(TDiffuseOrNormal)}.");

    internal uint FlexibleVertexFormat => typeof(TDiffuseOrNormal) == typeof(Vector3)
        ? (uint) (D3D9.FvfXyz | D3D9.FvfNormal | D3D9.FvfTex1)
        : typeof(TDiffuseOrNormal) == typeof(uint)
            ? (uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex1)
            : throw new NotSupportedException($"Unsupported diffuse or normal vertex data type: {typeof(TDiffuseOrNormal)}.");

    internal bool CanRenderIndexed(Direct3D9HardwareVertexBuffer vertexBuffer)
    {
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        return _inputIndexCount != 0 && !_indices.IsEmpty && _inputVertexCount <= vertexBuffer.GetMaximumCapacity(VertexStride);
    }

    internal Direct3D9CreateLightingColorSource? CreateLightingColorSource => _createLightingColorSource;

    internal static int SendGeometry(nint geometrySink)
    {
        _ = geometrySink;
        return Direct3D9Factory.SuccessHResult;
    }

    internal static int SendGeometryModifiers(Direct3D9FixedFunctionPipelineItemBuilder? pipelineBuilder)
    {
        _ = pipelineBuilder;
        return Direct3D9Factory.SuccessHResult;
    }

    internal int Render(
        Direct3D9Device device,
        Direct3D9HardwareVertexBuffer vertexBuffer,
        Direct3D9HardwareIndexBuffer indexBuffer)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        ArgumentNullException.ThrowIfNull(indexBuffer);

        bool indexed = CanRenderIndexed(vertexBuffer);
        int result = SendDeviceState(indexed, device, vertexBuffer, indexBuffer);
        if (result < 0)
        {
            return result;
        }

        while (true)
        {
            result = indexed
                ? PrepareIndexed(vertexBuffer, indexBuffer, out Direct3D9GeometryRenderBatch batch)
                : PrepareNonIndexed(vertexBuffer, out batch);
            if (result < 0 || !batch.NeedsRender)
            {
                return result;
            }

            result = indexed
                ? device.DrawIndexedTriangleList(
                    batch.StartVertex,
                    0,
                    _inputVertexCount,
                    batch.StartIndex,
                    batch.PrimitiveCount)
                : device.DrawTriangleList(batch.StartVertex, batch.PrimitiveCount);
            if (result < 0)
            {
                return result;
            }
        }
    }

    internal int SendDeviceState(
        bool indexed,
        Direct3D9Device device,
        Direct3D9HardwareVertexBuffer vertexBuffer,
        Direct3D9HardwareIndexBuffer indexBuffer)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        ArgumentNullException.ThrowIfNull(indexBuffer);

        int result = device.SetFlexibleVertexFormat(FlexibleVertexFormat);
        if (result < 0)
        {
            return result;
        }

        result = device.SetStreamSource(
            0,
            vertexBuffer.DangerousGetDirect3DVertexBuffer(),
            0,
            VertexStride);
        if (result < 0 || !indexed)
        {
            return result;
        }

        return device.SetIndices(indexBuffer.DangerousGetDirect3DIndexBuffer());
    }

    internal int PrepareIndexed(
        Direct3D9HardwareVertexBuffer vertexBuffer,
        Direct3D9HardwareIndexBuffer indexBuffer,
        out Direct3D9GeometryRenderBatch batch)
    {
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        ArgumentNullException.ThrowIfNull(indexBuffer);
        _ = vertexBuffer.GetMaximumCapacity(VertexStride);
        _ = indexBuffer.GetMaximumCapacity();

        uint startIndex = 0;
        uint primitiveCount = 0;
        bool needsRender = true;
        int result = 0;

        if (_renderedIndexCount == 0)
        {
            result = vertexBuffer.Lock(_inputVertexCount, VertexStride, out void* lockedVertices, out _indexedVertexStart);
            if (result < 0)
            {
                goto Cleanup;
            }

            CopyVerticesIntoBuffer(lockedVertices, _inputVertexCount);
        }

        uint indicesToCopy = indexBuffer.GetNextUsableNumberOfIndices();
        uint remainingIndices = _inputIndexCount - _renderedIndexCount;
        if (indicesToCopy > remainingIndices)
        {
            indicesToCopy = remainingIndices;
            if (indicesToCopy == 0)
            {
                needsRender = false;
                goto Cleanup;
            }
        }

        result = indexBuffer.CopyFromInputBuffer(
            _indices.Slice((int) _renderedIndexCount, (int) indicesToCopy),
            out startIndex);
        if (result >= 0)
        {
            _renderedIndexCount += indicesToCopy;
            primitiveCount = indicesToCopy / 3;
        }

    Cleanup:
        if (vertexBuffer.IsLocked)
        {
            int unlockResult = vertexBuffer.Unlock(_inputVertexCount);
            if (result >= 0)
            {
                result = unlockResult;
            }
        }

        batch = new(_indexedVertexStart, startIndex, primitiveCount, needsRender);
        return result;
    }

    internal int PrepareNonIndexed(
        Direct3D9HardwareVertexBuffer vertexBuffer,
        out Direct3D9GeometryRenderBatch batch)
    {
        ArgumentNullException.ThrowIfNull(vertexBuffer);

        uint startVertex = 0;
        uint primitiveCount = 0;
        bool needsRender = true;
        int result = 0;
        void* lockedVertices = null;
        uint verticesToCopy = vertexBuffer.GetNextUsableNumberOfVertices(VertexStride);
        uint remainingIndices = _inputIndexCount - _renderedIndexCount;
        if (verticesToCopy > remainingIndices)
        {
            verticesToCopy = remainingIndices;
            if (verticesToCopy == 0)
            {
                needsRender = false;
                goto Cleanup;
            }
        }

        result = vertexBuffer.Lock(verticesToCopy, VertexStride, out lockedVertices, out startVertex);
        if (result < 0)
        {
            goto Cleanup;
        }

        CopyIndexOrderedVerticesIntoBuffer(lockedVertices, _renderedIndexCount, verticesToCopy);
        primitiveCount = verticesToCopy / 3;
        _renderedIndexCount += verticesToCopy;

    Cleanup:
        if (vertexBuffer.IsLocked)
        {
            int unlockResult = vertexBuffer.Unlock(verticesToCopy);
            if (result >= 0)
            {
                result = unlockResult;
            }
        }

        batch = new(startVertex, 0, primitiveCount, needsRender);
        return result;
    }

    private void CopyVerticesIntoBuffer(void* destination, uint vertexCount)
    {
        byte* output = (byte*) destination;
        for (uint index = 0; index < vertexCount; index++)
        {
            WriteVertex(output, index);
            output += VertexStride;
        }
    }

    private void CopyIndexOrderedVerticesIntoBuffer(void* destination, uint inputIndexStart, uint indexCount)
    {
        byte* output = (byte*) destination;
        for (uint index = 0; index < indexCount; index++)
        {
            uint currentVertex = _indices.IsEmpty
                ? inputIndexStart + index
                : _indices[(int) (inputIndexStart + index)];
            WriteVertex(output, currentVertex);
            output += VertexStride;
        }
    }

    private void WriteVertex(byte* destination, uint inputVertex)
    {
        Vector3 position = _positions[(int) inputVertex];
        TDiffuseOrNormal diffuseOrNormal = _diffuseColorsOrNormals.IsEmpty
            ? _defaultDiffuseOrNormal
            : _diffuseColorsOrNormals[(int) inputVertex];
        Vector2 textureCoordinate = _textureCoordinates[(int) inputVertex];

        Unsafe.WriteUnaligned(destination, position);
        Unsafe.WriteUnaligned(destination + sizeof(Vector3), diffuseOrNormal);
        Unsafe.WriteUnaligned(destination + sizeof(Vector3) + sizeof(TDiffuseOrNormal), textureCoordinate);
    }
}

internal delegate int Direct3D9SetupFixedFunctionMeshPass(
    uint passIndex,
    out Direct3D9Pipeline? pipeline,
    out Func<int>? renderGeometry);

internal delegate int Direct3D9CreateLightingColorSource(
    out Direct3D9LightingColorSource? colorSource);

internal enum Direct3D9FixedFunctionLightingValues
{
    None,
    Diffuse,
    Specular,
    Emissive
}

internal delegate int Direct3D9SetupFixedFunctionGeometryPass(
    uint passIndex,
    MilCompositingMode compositingMode,
    ref Direct3D9GeometryRenderer<uint> renderer,
    out Direct3D9Pipeline? pipeline);

internal delegate int Direct3D9RenderFixedFunctionGeometry(
    ref Direct3D9GeometryRenderer<uint> renderer);

internal delegate int Direct3D9SetupShaderGeometryPass(
    uint passIndex,
    ref Direct3D9GeometryRenderer<Vector3> renderer,
    out Direct3D9Pipeline? pipeline);

internal delegate int Direct3D9RenderShaderGeometry(
    ref Direct3D9GeometryRenderer<Vector3> renderer);

internal delegate int Direct3D9InitializeFixedFunctionGeometryPipeline(
    ref Direct3D9GeometryRenderer<uint> renderer,
    out Direct3D9Pipeline? pipeline);

internal delegate Direct3D9FixedFunctionPipelineInitializer Direct3D9CreateFixedFunctionPipelineInitializer(
    IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items);

internal delegate Direct3D9FixedFunctionPipelineInitializer Direct3D9CreateCompositedFixedFunctionPipelineInitializer(
    MilCompositingMode compositingMode,
    IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items);

internal sealed class Direct3D9FixedFunctionMeshPipelineInitializer
{
    private const nint PreGeneratedVertexBuffer = 1;

    private readonly Direct3D9CreateCompositedFixedFunctionPipelineInitializer _createInitializer;
    private readonly Direct3D9SurfaceRect? _outsideBounds;
    private readonly bool _needInside;
    private readonly Func<nint, int> _drawPrimitive;
    private readonly Func<int> _beginBuilding;
    private readonly Func<bool> _hasOutsideBounds;
    private readonly Direct3D9FlushPipelineVertexBuilder _flushTryGetVertexBuffer;
    private readonly Action _releaseColorSources;

    internal Direct3D9FixedFunctionMeshPipelineInitializer(
        Direct3D9CreateCompositedFixedFunctionPipelineInitializer createInitializer,
        Action releaseColorSources,
        Direct3D9SurfaceRect? outsideBounds = null,
        bool needInside = true)
        : this(
            createInitializer,
            static _ => Direct3D9Factory.InternalErrorHResult,
            static () => Direct3D9Factory.SuccessHResult,
            static () => false,
            FlushPreGeneratedVertexBuilder,
            releaseColorSources,
            outsideBounds,
            needInside)
    {
    }

    internal Direct3D9FixedFunctionMeshPipelineInitializer(
        Direct3D9CreateCompositedFixedFunctionPipelineInitializer createInitializer,
        Func<nint, int> drawPrimitive, 
        Func<int> beginBuilding,
        Func<bool> hasOutsideBounds,
        Direct3D9FlushPipelineVertexBuilder flushTryGetVertexBuffer,
        Action releaseColorSources,
        Direct3D9SurfaceRect? outsideBounds = null,
        bool needInside = true)
    {
        ArgumentNullException.ThrowIfNull(createInitializer);
        ArgumentNullException.ThrowIfNull(drawPrimitive);
        ArgumentNullException.ThrowIfNull(beginBuilding);
        ArgumentNullException.ThrowIfNull(hasOutsideBounds);
        ArgumentNullException.ThrowIfNull(flushTryGetVertexBuffer);
        ArgumentNullException.ThrowIfNull(releaseColorSources);

        _createInitializer = createInitializer;
        _drawPrimitive = drawPrimitive;
        _beginBuilding = beginBuilding;
        _hasOutsideBounds = hasOutsideBounds;
        _flushTryGetVertexBuffer = flushTryGetVertexBuffer;
        _releaseColorSources = releaseColorSources;
        _outsideBounds = outsideBounds;
        _needInside = needInside;
    }

    internal int Initialize(
        Direct3D9FixedFunctionPassInputs passInputs,
        MilCompositingMode compositingMode,
        ref Direct3D9GeometryRenderer<uint> renderer,
        out Direct3D9Pipeline? pipeline)
    {
        ArgumentNullException.ThrowIfNull(passInputs);

        return Direct3D9FixedFunctionGeometryPipeline.Initialize(
            ref renderer,
            passInputs,
            items => _createInitializer(compositingMode, items),
            _outsideBounds,
            _needInside,
            _drawPrimitive,
            _beginBuilding,
            _hasOutsideBounds,
            _flushTryGetVertexBuffer,
            _releaseColorSources,
            out pipeline);
    }

    private static int FlushPreGeneratedVertexBuilder(out nint vertexBuffer)
    {
        vertexBuffer = PreGeneratedVertexBuffer;
        return Direct3D9Factory.SuccessHResult;
    }
}

internal static class Direct3D9FixedFunctionGeometryPipeline
{
    internal static int Initialize(
        ref Direct3D9GeometryRenderer<uint> renderer,
        Direct3D9FixedFunctionPassInputs passInputs,
        Direct3D9CreateFixedFunctionPipelineInitializer createInitializer,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside,
        Func<nint, int> drawPrimitive,
        Func<int> beginBuilding,
        Func<bool> hasOutsideBounds,
        Direct3D9FlushPipelineVertexBuilder flushTryGetVertexBuffer,
        Action releaseColorSources,
        out Direct3D9Pipeline? pipeline)
    {
        ArgumentNullException.ThrowIfNull(passInputs);
        ArgumentNullException.ThrowIfNull(createInitializer);

        pipeline = null;
        Direct3D9CreateLightingColorSource? createLightingColorSource = renderer.CreateLightingColorSource;
        Func<Direct3D9FixedFunctionPipelineItemBuilder, int>? sendLighting = createLightingColorSource is null
            ? null
            : builder => SendLighting(createLightingColorSource, builder);
        int result = passInputs.CreateItems(sendLighting, out IReadOnlyList<Direct3D9FixedFunctionPipelineItem>? items);
        if (result < 0)
        {
            return result;
        }

        Direct3D9FixedFunctionPipelineInitializer initializer;
        try
        {
            initializer = createInitializer(items!);
        }
        catch
        {
            Direct3D9FixedFunctionPipelineItemBuilder.ReleaseOwnedColorSources(items!);
            throw;
        }

        return Initialize(
            ref renderer,
            initializer,
            outsideBounds,
            needInside,
            drawPrimitive,
            beginBuilding,
            hasOutsideBounds,
            flushTryGetVertexBuffer,
            releaseColorSources,
            out pipeline);
    }

    private static int SendLighting(
        Direct3D9CreateLightingColorSource createLightingColorSource,
        Direct3D9FixedFunctionPipelineItemBuilder builder)
    {
        int result = createLightingColorSource(out Direct3D9LightingColorSource? colorSource);
        if (result < 0)
        {
            colorSource?.Dispose();
            return result;
        }

        if (colorSource is null)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        result = builder.AddLighting(colorSource);
        if (result < 0)
        {
            colorSource.Dispose();
        }

        return result;
    }

    internal static int Initialize(
        ref Direct3D9GeometryRenderer<uint> renderer,
        Direct3D9FixedFunctionPipelineInitializer initializer,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside,
        Func<nint, int> drawPrimitive,
        Func<int> beginBuilding,
        Func<bool> hasOutsideBounds,
        Direct3D9FlushPipelineVertexBuilder flushTryGetVertexBuffer,
        Action releaseColorSources,
        out Direct3D9Pipeline? pipeline)
    {
        ArgumentNullException.ThrowIfNull(initializer);

        return initializer.InitializeForRendering(
            ref renderer,
            outsideBounds,
            needInside,
            drawPrimitive,
            beginBuilding,
            hasOutsideBounds,
            flushTryGetVertexBuffer,
            releaseColorSources,
            out pipeline);
    }

    internal static int Render(
        ref Direct3D9GeometryRenderer<uint> renderer,
        Direct3D9Device device)
    {
        ArgumentNullException.ThrowIfNull(device);

        Direct3D9HardwareVertexBuffer? vertexBuffer = device.Get3DVertexBuffer();
        Direct3D9HardwareIndexBuffer? indexBuffer = device.Get3DIndexBuffer();
        return vertexBuffer is null || indexBuffer is null
            ? Direct3D9Factory.InternalErrorHResult
            : renderer.Render(device, vertexBuffer, indexBuffer);
    }
}

internal static class Direct3D9MeshShaderRenderer
{
    internal static int Render(
        Func<int> begin,
        Func<bool> canRunShaderPath,
        Func<int> shaderDrawMesh3D,
        Func<int> fixedFunctionDrawMesh3D,
        Func<int> finish)
    {
        ArgumentNullException.ThrowIfNull(begin);
        ArgumentNullException.ThrowIfNull(canRunShaderPath);
        ArgumentNullException.ThrowIfNull(shaderDrawMesh3D);
        ArgumentNullException.ThrowIfNull(fixedFunctionDrawMesh3D);
        ArgumentNullException.ThrowIfNull(finish);

        int result = begin();
        if (result < 0)
        {
            return result;
        }

        try
        {
            bool useShaderPath = canRunShaderPath();
            if (useShaderPath)
            {
                result = shaderDrawMesh3D();
            }

            if (!useShaderPath || result < 0)
            {
                result = fixedFunctionDrawMesh3D();
            }
        }
        finally
        {
            int finishResult = finish();
            if (result >= 0)
            {
                result = finishResult;
            }
        }

        return result;
    }
}

internal static class Direct3D9ShaderMeshRenderer
{
    internal static int Render(
        uint passCount,
        ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<Vector3> normals,
        ReadOnlySpan<Vector2> textureCoordinates,
        ReadOnlySpan<uint> indices,
        Direct3D9Device device,
        Direct3D9SetupShaderGeometryPass setupPass,
        Direct3D9CreateLightingColorSource? createLightingColorSource = null,
        Direct3D9FixedFunctionLightingValues requiredLightingValues = Direct3D9FixedFunctionLightingValues.Diffuse,
        Action<Direct3D9FixedFunctionLightingValues>? setLightingPass = null)
    {
        ArgumentNullException.ThrowIfNull(device);

        return Render(
            passCount,
            positions,
            normals,
            textureCoordinates,
            indices,
            setupPass,
            (ref Direct3D9GeometryRenderer<Vector3> renderer) =>
            {
                Direct3D9HardwareVertexBuffer? vertexBuffer = device.Get3DVertexBuffer();
                Direct3D9HardwareIndexBuffer? indexBuffer = device.Get3DIndexBuffer();
                return vertexBuffer is null || indexBuffer is null
                    ? Direct3D9Factory.InternalErrorHResult
                    : renderer.Render(device, vertexBuffer, indexBuffer);
            },
            createLightingColorSource,
            requiredLightingValues,
            setLightingPass);
    }

    internal static int Render(
        uint passCount,
        ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<Vector3> normals,
        ReadOnlySpan<Vector2> textureCoordinates,
        ReadOnlySpan<uint> indices,
        Direct3D9SetupShaderGeometryPass setupPass,
        Direct3D9RenderShaderGeometry renderGeometry,
        Direct3D9CreateLightingColorSource? createLightingColorSource = null,
        Direct3D9FixedFunctionLightingValues requiredLightingValues = Direct3D9FixedFunctionLightingValues.Diffuse,
        Action<Direct3D9FixedFunctionLightingValues>? setLightingPass = null)
    {
        ArgumentOutOfRangeException.ThrowIfZero(passCount);
        ArgumentNullException.ThrowIfNull(setupPass);
        ArgumentNullException.ThrowIfNull(renderGeometry);

        setLightingPass?.Invoke(requiredLightingValues);

        for (uint passIndex = 0; passIndex < passCount; passIndex++)
        {
            Direct3D9GeometryRenderer<Vector3> renderer = new(
                positions,
                normals,
                textureCoordinates,
                indices,
                new Vector3(1, 0, 0),
                createLightingColorSource);
            int result = setupPass(passIndex, ref renderer, out Direct3D9Pipeline? pipeline);
            if (result < 0)
            {
                pipeline?.ReleaseExpensiveResources();
                return result;
            }

            if (pipeline is null)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }

            try
            {
                result = pipeline.Execute();
                if (result >= 0)
                {
                    result = renderGeometry(ref renderer);
                }
            }
            finally
            {
                pipeline.ReleaseExpensiveResources();
            }

            if (result < 0)
            {
                return result;
            }
        }

        return Direct3D9Factory.SuccessHResult;
    }
}

internal static class Direct3D9FixedFunctionMeshRenderer
{
    internal static int Render(
        uint passCount,
        Direct3D9FixedFunctionLightingValues requiredLightingValues,
        ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<Vector2> textureCoordinates,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<uint> diffuseColors,
        ReadOnlySpan<uint> specularColors,
        MilColorF materialEmissiveColor,
        bool zBufferEnabled,
        Direct3D9Device device,
        Func<int> precomputeLighting,
        Direct3D9SetupFixedFunctionGeometryPass setupPass,
        Direct3D9CreateLightingColorSource? createLightingColorSource = null)
    {
        ArgumentNullException.ThrowIfNull(device);

        return Render(
            passCount,
            requiredLightingValues,
            positions,
            textureCoordinates,
            indices,
            diffuseColors,
            specularColors,
            materialEmissiveColor,
            zBufferEnabled,
            device.SetRenderState,
            precomputeLighting,
            setupPass,
            (ref Direct3D9GeometryRenderer<uint> renderer) =>
                Direct3D9FixedFunctionGeometryPipeline.Render(ref renderer, device),
            createLightingColorSource);
    }

    internal static int Render(
        uint passCount,
        Direct3D9FixedFunctionLightingValues requiredLightingValues,
        ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<Vector2> textureCoordinates,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<uint> diffuseColors,
        ReadOnlySpan<uint> specularColors,
        MilColorF materialEmissiveColor,
        bool zBufferEnabled,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<int> precomputeLighting,
        Direct3D9SetupFixedFunctionGeometryPass setupPass,
        Direct3D9RenderFixedFunctionGeometry renderGeometry,
        Direct3D9CreateLightingColorSource? createLightingColorSource = null)
    {
        ArgumentOutOfRangeException.ThrowIfZero(passCount);
        ArgumentNullException.ThrowIfNull(setRenderState);
        ArgumentNullException.ThrowIfNull(precomputeLighting);
        ArgumentNullException.ThrowIfNull(setupPass);
        ArgumentNullException.ThrowIfNull(renderGeometry);

        MilCompositingMode compositingMode = requiredLightingValues switch
        {
            Direct3D9FixedFunctionLightingValues.Diffuse => MilCompositingMode.SourceOver,
            Direct3D9FixedFunctionLightingValues.Specular or Direct3D9FixedFunctionLightingValues.Emissive =>
                MilCompositingMode.SourceAdd,
            _ => throw new ArgumentOutOfRangeException(nameof(requiredLightingValues))
        };

        int result = zBufferEnabled
            ? setRenderState(
                Renderstatetype.Zwriteenable,
                requiredLightingValues == Direct3D9FixedFunctionLightingValues.Diffuse ? 1u : 0u)
            : Direct3D9Factory.SuccessHResult;
        if (result < 0)
        {
            return result;
        }

        result = precomputeLighting();
        if (result < 0)
        {
            return result;
        }

        ReadOnlySpan<uint> colors = requiredLightingValues switch
        {
            Direct3D9FixedFunctionLightingValues.Diffuse => diffuseColors,
            Direct3D9FixedFunctionLightingValues.Specular => specularColors,
            _ => []
        };
        uint defaultColor = requiredLightingValues == Direct3D9FixedFunctionLightingValues.Emissive
            ? ConvertSrgbToD3DColorZeroAlpha(materialEmissiveColor)
            : uint.MaxValue;

        for (uint passIndex = 0; passIndex < passCount; passIndex++)
        {
            Direct3D9GeometryRenderer<uint> renderer = new(
                positions,
                colors,
                textureCoordinates,
                indices,
                defaultColor,
                createLightingColorSource);
            result = setupPass(passIndex, compositingMode, ref renderer, out Direct3D9Pipeline? pipeline);
            if (result < 0)
            {
                pipeline?.ReleaseExpensiveResources();
                return result;
            }

            if (pipeline is null)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }

            try
            {
                result = pipeline.Execute();
                if (result >= 0)
                {
                    result = renderGeometry(ref renderer);
                }
            }
            finally
            {
                pipeline.ReleaseExpensiveResources();
            }

            if (result < 0)
            {
                return result;
            }
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal static int Render(
        uint passCount,
        Func<int> precomputeLighting,
        Direct3D9SetupFixedFunctionMeshPass setupPass)
    {
        ArgumentOutOfRangeException.ThrowIfZero(passCount);
        ArgumentNullException.ThrowIfNull(precomputeLighting);
        ArgumentNullException.ThrowIfNull(setupPass);

        int result = precomputeLighting();
        if (result < 0)
        {
            return result;
        }

        for (uint passIndex = 0; passIndex < passCount; passIndex++)
        {
            result = setupPass(passIndex, out Direct3D9Pipeline? pipeline, out Func<int>? renderGeometry);
            if (result < 0)
            {
                pipeline?.ReleaseExpensiveResources();
                return result;
            }

            if (pipeline is null || renderGeometry is null)
            {
                pipeline?.ReleaseExpensiveResources();
                return Direct3D9Factory.InternalErrorHResult;
            }

            try
            {
                result = pipeline.Execute();
                if (result >= 0)
                {
                    result = renderGeometry();
                }
            }
            finally
            {
                pipeline.ReleaseExpensiveResources();
            }

            if (result < 0)
            {
                return result;
            }
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private static uint ConvertSrgbToD3DColorZeroAlpha(MilColorF color)
    {
        uint red = (uint) MathF.Floor((color.Red * 255.0f) + 0.5f);
        uint green = (uint) MathF.Floor((color.Green * 255.0f) + 0.5f);
        uint blue = (uint) MathF.Floor((color.Blue * 255.0f) + 0.5f);
        return (red << 16) | (green << 8) | blue;
    }
}
