using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed class Direct3D9ProductionVertexDraw
{
    private readonly Direct3D9Device _device;

    internal Direct3D9ProductionVertexDraw(Direct3D9Device device)
    {
        ArgumentNullException.ThrowIfNull(device);
        _device = device;
    }

    internal unsafe int Draw(Direct3D9BuilderDrawBatch batch)
    {
        ReadOnlySpan<Direct3D9ExpandedVertex> vertices = batch.Vertices.Span;
        if (vertices.IsEmpty)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        int result = _device.SetFlexibleVertexFormat((uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex8));
        if (result < 0)
        {
            return result;
        }

        fixed (Direct3D9ExpandedVertex* vertexData = vertices)
        {
            if (batch.Kind == Direct3D9BuilderPrimitiveKind.IndexedTriangleList)
            {
                ReadOnlySpan<ushort> indices = batch.Indices.Span;
                if (indices.IsEmpty || indices.Length % 3 != 0)
                {
                    return Direct3D9Factory.InvalidArgumentHResult;
                }

                fixed (ushort* indexData = indices)
                {
                    return _device.DrawIndexedTriangleListUp(
                        checked((uint) vertices.Length),
                        checked((uint) indices.Length / 3),
                        indexData,
                        vertexData,
                        (uint) Unsafe.SizeOf<Direct3D9ExpandedVertex>());
                }
            }

            uint primitiveCount = batch.Kind switch
            {
                Direct3D9BuilderPrimitiveKind.TriangleList when vertices.Length % 3 == 0 => checked((uint) vertices.Length / 3),
                Direct3D9BuilderPrimitiveKind.TriangleStrip when vertices.Length >= 3 => checked((uint) vertices.Length - 2),
                Direct3D9BuilderPrimitiveKind.LineList when vertices.Length % 2 == 0 => checked((uint) vertices.Length / 2),
                _ => 0,
            };
            if (primitiveCount == 0)
            {
                return Direct3D9Factory.InvalidArgumentHResult;
            }

            Primitivetype primitiveType = batch.Kind switch
            {
                Direct3D9BuilderPrimitiveKind.TriangleList => Primitivetype.Trianglelist,
                Direct3D9BuilderPrimitiveKind.TriangleStrip => Primitivetype.Trianglestrip,
                Direct3D9BuilderPrimitiveKind.LineList => Primitivetype.Linelist,
                _ => throw new InvalidOperationException("Unsupported production primitive kind."),
            };
            return _device.DrawPrimitiveUp(
                primitiveType,
                primitiveCount,
                vertexData,
                (uint) Unsafe.SizeOf<Direct3D9ExpandedVertex>());
        }
    }
}
