using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9ProductionVertexDrawTests
{
    [TestMethod]
    public void WhenExpandedBatchesDrawThenFlexibleFormatAndNativePrimitiveKindsAreUsed()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9ProductionVertexDraw draw = new(device);

        int triangleResult = draw.Draw(new Direct3D9BuilderDrawBatch(
            Direct3D9BuilderPrimitiveKind.TriangleList,
            new Direct3D9ExpandedVertex[3],
            ReadOnlyMemory<ushort>.Empty));
        int lineResult = draw.Draw(new Direct3D9BuilderDrawBatch(
            Direct3D9BuilderPrimitiveKind.LineList,
            new Direct3D9ExpandedVertex[2],
            ReadOnlyMemory<ushort>.Empty));

        Assert.AreEqual(
            (0, 0, $"Fvf:{(uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex8)}|Draw:PTTrianglelist:1:80|Draw:PTLinelist:1:80"),
            (triangleResult, lineResult, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenExpandedIndexedBatchDrawsThenDirectIndexedPathReceivesVerticesAndIndices()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9ProductionVertexDraw draw = new(device);

        int result = draw.Draw(new Direct3D9BuilderDrawBatch(
            Direct3D9BuilderPrimitiveKind.IndexedTriangleList,
            new Direct3D9ExpandedVertex[3],
            new ushort[] { 0, 1, 2 }));

        Assert.AreEqual(
            (0, $"Fvf:{(uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex8)}|Indexed:3:1:0,1,2:80"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenExpandedBatchShapeIsInvalidThenDrawIsRejectedBeforeNativeSubmission()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls);
        Direct3D9ProductionVertexDraw draw = new(device);

        int result = draw.Draw(new Direct3D9BuilderDrawBatch(
            Direct3D9BuilderPrimitiveKind.TriangleList,
            new Direct3D9ExpandedVertex[2],
            ReadOnlyMemory<ushort>.Empty));

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, $"Fvf:{(uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex8)}"),
            (result, string.Join('|', calls)));
    }

    private static Direct3D9Device CreateDevice(List<string> calls)
    {
        Caps9 capabilities = default;
        capabilities.MaxPrimitiveCount = ushort.MaxValue;
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            capabilities: capabilities,
            recordSuccessfulPresent: static () => 0,
            setFlexibleVertexFormat: format =>
            {
                calls.Add($"Fvf:{format}");
                return 0;
            },
            setStreamSource: static (_, _, _, _) => 0,
            setIndices: static _ => 0,
            drawPrimitiveUp: (type, count, _, stride) =>
            {
                calls.Add($"Draw:{type}:{count}:{stride}");
                return 0;
            },
            drawIndexedTriangleListUp: (vertexCount, primitiveCount, indices, _, stride) =>
            {
                calls.Add($"Indexed:{vertexCount}:{primitiveCount}:{indices[0]},{indices[1]},{indices[2]}:{stride}");
                return 0;
            });
    }
}
