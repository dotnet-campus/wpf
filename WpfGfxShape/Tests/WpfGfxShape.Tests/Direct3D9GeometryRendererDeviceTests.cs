using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9GeometryRendererDeviceTests
{
    private static nint _vertexBufferToReturn;
    private static nint _indexBufferToReturn;
    private static nint _vertexBufferIdentity;
    private static nint _vertexData;
    private static nint _indexData;
    private static int _lockCallCount;
    private static int _vertexUnlockCount;
    private static int _indexUnlockCount;
    private static int _vertexLockResult;
    private static int _indexLockResult;
    private static int _vertexUnlockResult;
    private static int _indexUnlockResult;
    private static int _drawCallCount;

    [TestInitialize]
    public void Initialize()
    {
        _vertexBufferToReturn = 0;
        _indexBufferToReturn = 0;
        _vertexBufferIdentity = 0;
        _vertexData = 0;
        _indexData = 0;
        _lockCallCount = 0;
        _vertexUnlockCount = 0;
        _indexUnlockCount = 0;
        _vertexLockResult = 0;
        _indexLockResult = 0;
        _vertexUnlockResult = 0;
        _indexUnlockResult = 0;
        _drawCallCount = 0;
    }

    [TestMethod]
    public void WhenDiffusePerVertexDataTypeIsRequestedThenXyzDiffuseAndUv1AreReturned()
    {
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            uint.MaxValue);

        Direct3D9VertexFormatAttribute result = renderer.PerVertexDataType;

        Assert.AreEqual(
            Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Diffuse | Direct3D9VertexFormatAttribute.Uv1,
            result);
    }

    [TestMethod]
    public void WhenNormalPerVertexDataTypeIsRequestedThenOnlyTraitAttributeChanges()
    {
        Direct3D9GeometryRenderer<Vector3> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            Vector3.UnitZ);

        Direct3D9VertexFormatAttribute result = renderer.PerVertexDataType;

        Assert.AreEqual(
            Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Normal | Direct3D9VertexFormatAttribute.Uv1,
            result);
    }

    [TestMethod]
    public void WhenPerVertexDataTypeIsRequestedRepeatedlyThenResultIsStableAndDeviceStateIsUntouched()
    {
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [10u, 20u, 30u],
            [Vector2.Zero, Vector2.One, new Vector2(2, 2)],
            [2u, 0u, 1u],
            0);

        Direct3D9VertexFormatAttribute first = renderer.PerVertexDataType;
        Direct3D9VertexFormatAttribute second = renderer.PerVertexDataType;

        Assert.AreEqual((first, 0, 0, 0, 0), (second, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenSendingGeometryToNullSinkThenSuccessIsReturnedWithoutDeviceWork()
    {
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [10u, 20u, 30u],
            [Vector2.Zero, Vector2.One, new Vector2(2, 2)],
            [2u, 0u, 1u],
            0);

        int result = Direct3D9GeometryRenderer<uint>.SendGeometry(0);

        Assert.AreEqual((0, 0, 0, 0, 0), (result, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenSendingGeometryToNonNullSinkThenSinkIsIgnored()
    {
        Direct3D9GeometryRenderer<Vector3> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ],
            [Vector2.Zero, Vector2.One, new Vector2(2, 2)],
            [2u, 0u, 1u],
            Vector3.UnitZ);

        int result = Direct3D9GeometryRenderer<Vector3>.SendGeometry(unchecked((nint) 0x12345678));

        Assert.AreEqual((0, 0, 0, 0, 0), (result, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenSendingGeometryRepeatedlyThenCallsRemainSideEffectFree()
    {
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            uint.MaxValue);

        int first = Direct3D9GeometryRenderer<uint>.SendGeometry(0);
        int second = Direct3D9GeometryRenderer<uint>.SendGeometry(nint.MaxValue);

        Assert.AreEqual((first, 0, 0, 0, 0), (second, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenSendingGeometryModifiersToNullBuilderThenSuccessIsReturnedWithoutDeviceWork()
    {
        int result = Direct3D9GeometryRenderer<uint>.SendGeometryModifiers(null);

        Assert.AreEqual((0, 0, 0, 0, 0), (result, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenSendingGeometryModifiersToBuilderThenBuilderIsIgnored()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new(Direct3D9VertexFormatAttribute.Xyz);

        int result = Direct3D9GeometryRenderer<Vector3>.SendGeometryModifiers(builder);

        Assert.AreEqual((0, 0, 0, 0, 0), (result, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenSendingGeometryModifiersRepeatedlyThenCallsRemainSideEffectFree()
    {
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new(Direct3D9VertexFormatAttribute.Xyz);

        int first = Direct3D9GeometryRenderer<uint>.SendGeometryModifiers(null);
        int second = Direct3D9GeometryRenderer<uint>.SendGeometryModifiers(builder);

        Assert.AreEqual((first, 0, 0, 0, 0), (second, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenRenderingIndexedDiffuseGeometryThenStateAndDrawArgumentsMatchNativePath()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        ushort* indexData = stackalloc ushort[3];
        _vertexData = (nint) vertexData;
        _indexData = (nint) indexData;
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            flexibleVertexFormat =>
            {
                calls.Add($"fvf:{flexibleVertexFormat}");
                return 0;
            },
            (_, stream, offset, stride) =>
            {
                calls.Add($"stream:{(nint) stream}:{offset}:{stride}");
                return 0;
            },
            indices =>
            {
                calls.Add($"indices:{(nint) indices}");
                return 0;
            },
            (baseVertex, minIndex, vertexCount, startIndex, primitiveCount) =>
            {
                _drawCallCount++;
                calls.Add($"draw-indexed:{baseVertex}:{minIndex}:{vertexCount}:{startIndex}:{primitiveCount}");
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [10u, 20u, 30u],
            [new(0.1f, 0.2f), new(0.3f, 0.4f), new(0.5f, 0.6f)],
            [2u, 0u, 1u],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            $"fvf:322|stream:{_vertexBufferToReturn}:0:24|indices:{_indexBufferToReturn}|draw-indexed:0:0:3:0:1",
            string.Join('|', calls));
        Assert.AreEqual((0, 1, 2, 0, 1), (result, _drawCallCount, indexData[0], indexData[1], indexData[2]));
    }

    [TestMethod]
    public void WhenRenderingNonIndexedNormalGeometryThenIndicesAreNotSetAndTriangleListIsDrawn()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[96];
        _vertexData = (nint) vertexData;
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            flexibleVertexFormat =>
            {
                calls.Add($"fvf:{flexibleVertexFormat}");
                return 0;
            },
            (_, _, offset, stride) =>
            {
                calls.Add($"stream:{offset}:{stride}");
                return 0;
            },
            _ =>
            {
                calls.Add("indices");
                return 0;
            },
            drawTriangleList: (startVertex, primitiveCount) =>
            {
                calls.Add($"draw:{startVertex}:{primitiveCount}");
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 96, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<Vector3> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [new(0.1f, 0.2f), new(0.3f, 0.4f), new(0.5f, 0.6f)],
            [],
            Vector3.UnitZ);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual((0, "fvf:274|stream:0:32|draw:0:1"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenIndexedGeometryExceedsVertexBufferCapacityThenNonIndexedBatchesAreDrawnWithoutIndices()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        _vertexData = (nint) vertexData;
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            flexibleVertexFormat =>
            {
                calls.Add($"fvf:{flexibleVertexFormat}");
                return 0;
            },
            (_, _, offset, stride) =>
            {
                calls.Add($"stream:{offset}:{stride}");
                return 0;
            },
            _ =>
            {
                calls.Add("indices");
                return Direct3D9Factory.InvalidCallHResult;
            },
            drawTriangleList: (startVertex, primitiveCount) =>
            {
                calls.Add($"draw:{startVertex}:{primitiveCount}");
                _drawCallCount++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 12, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [
                new(1, 2, 3),
                new(4, 5, 6),
                new(7, 8, 9),
                new(10, 11, 12),
                new(13, 14, 15),
                new(16, 17, 18)
            ],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u, 5u, 4u, 3u],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            (0, "fvf:322|stream:0:24|draw:0:1|draw:0:1", 2, 2, 0),
            (result, string.Join('|', calls), _lockCallCount, _vertexUnlockCount, _indexUnlockCount));
    }

    [TestMethod]
    public void WhenOversizedIndexedGeometryFirstExpandedBatchDrawFailsThenRemainingBatchIsNotPrepared()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        _vertexData = (nint) vertexData;
        int indexCalls = 0;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ =>
            {
                indexCalls++;
                return 0;
            },
            drawTriangleList: (_, _) =>
            {
                _drawCallCount++;
                return Direct3D9Factory.GenericFailureHResult;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 12, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [
                new(1, 2, 3),
                new(4, 5, 6),
                new(7, 8, 9),
                new(10, 11, 12),
                new(13, 14, 15),
                new(16, 17, 18)
            ],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u, 5u, 4u, 3u],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 1, 1, 0, 1),
            (result, indexCalls, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenSendingIndexedDeviceStateThenNativeOrderAndArgumentsMatch()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            flexibleVertexFormat =>
            {
                calls.Add($"fvf:{flexibleVertexFormat}");
                return 0;
            },
            (streamNumber, streamData, offset, stride) =>
            {
                calls.Add($"stream:{streamNumber}:{(nint) streamData}:{offset}:{stride}");
                return 0;
            },
            indices =>
            {
                calls.Add($"indices:{(nint) indices}");
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u],
            0);

        int result = renderer.SendDeviceState(true, device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            $"fvf:322|stream:0:{_vertexBufferToReturn}:0:24|indices:{_indexBufferToReturn}",
            string.Join('|', calls));
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenSendingNonIndexedDeviceStateThenDisposedIndexBufferIsNotAccessed()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        int indexCalls = 0;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ =>
            {
                indexCalls++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            0);
        indexBuffer!.Dispose();

        int result = renderer.SendDeviceState(false, device, vertexBuffer!, indexBuffer);

        Assert.AreEqual((0, 0), (result, indexCalls));
    }

    [TestMethod]
    public void WhenSendingDeviceStateWithDisposedVertexBufferThenStreamAndIndicesAreNotCalled()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        int flexibleVertexFormatCalls = 0;
        int streamCalls = 0;
        int indexCalls = 0;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ =>
            {
                flexibleVertexFormatCalls++;
                return 0;
            },
            (_, _, _, _) =>
            {
                streamCalls++;
                return 0;
            },
            _ =>
            {
                indexCalls++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        vertexBuffer!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            new Direct3D9GeometryRenderer<uint>(
                [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
                [],
                [Vector2.Zero, Vector2.Zero, Vector2.Zero],
                [0u, 1u, 2u],
                0).SendDeviceState(true, device, vertexBuffer, indexBuffer!));
        Assert.AreEqual((1, 0, 0), (flexibleVertexFormatCalls, streamCalls, indexCalls));
    }

    [TestMethod]
    public void WhenSendingIndexedDeviceStateWithDisposedIndexBufferThenNativeIndicesAreNotCalled()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        int stateCalls = 0;
        int indexCalls = 0;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ =>
            {
                stateCalls++;
                return 0;
            },
            (_, _, _, _) =>
            {
                stateCalls++;
                return 0;
            },
            _ =>
            {
                indexCalls++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        indexBuffer!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            new Direct3D9GeometryRenderer<uint>(
                [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
                [],
                [Vector2.Zero, Vector2.Zero, Vector2.Zero],
                [0u, 1u, 2u],
                0).SendDeviceState(true, device, vertexBuffer!, indexBuffer));
        Assert.AreEqual((2, 0), (stateCalls, indexCalls));
    }

    [TestMethod]
    public void WhenSendingFlexibleVertexFormatFailsThenBufferStateAndDrawAreSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        int streamCalls = 0;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => Direct3D9Factory.GenericFailureHResult,
            (_, _, _, _) =>
            {
                streamCalls++;
                return 0;
            },
            _ => 0,
            drawTriangleList: (_, _) =>
            {
                _drawCallCount++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 0, 0), (result, streamCalls, _lockCallCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenSendingStreamSourceFailsThenIndicesBuffersAndDrawAreSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        int indexCalls = 0;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => Direct3D9Factory.GenericFailureHResult,
            _ =>
            {
                indexCalls++;
                return 0;
            },
            drawIndexedTriangleList: (_, _, _, _, _) =>
            {
                _drawCallCount++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0, 0),
            (result, indexCalls, _lockCallCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenSendingIndicesFailsThenBuffersAndDrawAreSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        int stateCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ =>
            {
                stateCallCount++;
                return 0;
            },
            (_, _, _, _) =>
            {
                stateCallCount++;
                return 0;
            },
            _ =>
            {
                stateCallCount++;
                return Direct3D9Factory.GenericFailureHResult;
            },
            drawIndexedTriangleList: (_, _, _, _, _) =>
            {
                _drawCallCount++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 3, 0, 0),
            (result, stateCallCount, _lockCallCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenIndexedIndexLockFailsThenVertexBufferIsUnlockedAndDrawIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        _vertexData = (nint) vertexData;
        _indexLockResult = Direct3D9Factory.GenericFailureHResult;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawIndexedTriangleList: (_, _, _, _, _) =>
            {
                _drawCallCount++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 2, 1, 0, 0),
            (result, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenIndexedIndexCopyAndVertexUnlockFailThenCopyFailureIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        _vertexData = (nint) vertexData;
        _indexLockResult = Direct3D9Factory.GenericFailureHResult;
        _vertexUnlockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawIndexedTriangleList: (_, _, _, _, _) =>
            {
                _drawCallCount++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 2, 1, 0, 0),
            (result, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenInitialIndexCopyAndVertexUnlockFailThenPendingVertexLockIsCleanedUpBeforeRetry()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        ushort* indexData = stackalloc ushort[3];
        _vertexData = (nint) vertexData;
        _indexData = (nint) indexData;
        _indexLockResult = Direct3D9Factory.GenericFailureHResult;
        _vertexUnlockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawIndexedTriangleList: (_, _, _, _, _) =>
            {
                _drawCallCount++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u],
            0);

        int firstResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        bool lockedAfterFirstFailure = vertexBuffer!.IsLocked;
        _vertexLockResult = Direct3D9Factory.InvalidCallHResult;
        _vertexUnlockResult = 0;
        int secondResult = renderer.Render(device, vertexBuffer, indexBuffer!);
        bool lockedAfterCleanup = vertexBuffer.IsLocked;
        _vertexLockResult = 0;
        _indexLockResult = 0;
        int thirdResult = renderer.Render(device, vertexBuffer, indexBuffer);
        Vector3 copiedFirstPosition = *(Vector3*) vertexData;

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.InvalidCallHResult, 0, true, false, 5, 3, 1, 1, new Vector3(1, 2, 3)),
            (firstResult, secondResult, thirdResult, lockedAfterFirstFailure, lockedAfterCleanup, _lockCallCount,
                _vertexUnlockCount, _indexUnlockCount, _drawCallCount, copiedFirstPosition));
    }

    [TestMethod]
    public void WhenIndexedIndexUnlockFailsThenFirstFailureIsReturnedAndDrawIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        ushort* indexData = stackalloc ushort[3];
        _vertexData = (nint) vertexData;
        _indexData = (nint) indexData;
        _indexUnlockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawIndexedTriangleList: (_, _, _, _, _) =>
            {
                _drawCallCount++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, 0),
            (result, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenIndexedIndexAndVertexUnlockFailThenIndexUnlockFailureIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        ushort* indexData = stackalloc ushort[3];
        _vertexData = (nint) vertexData;
        _indexData = (nint) indexData;
        _indexUnlockResult = Direct3D9Factory.GenericFailureHResult;
        _vertexUnlockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawIndexedTriangleList: (_, _, _, _, _) =>
            {
                _drawCallCount++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, 1, 0),
            (result, _indexUnlockCount, _vertexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenNonIndexedVertexUnlockFailsThenFailureIsReturnedAndDrawIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        _vertexData = (nint) vertexData;
        _vertexUnlockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawTriangleList: (_, _) =>
            {
                _drawCallCount++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 0, 0),
            (result, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenNonIndexedUnlockAndRelockFailThenPendingLockIsCleanedUpBeforeRemainingVerticesRetry()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        _vertexData = (nint) vertexData;
        _vertexUnlockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawTriangleList: (_, _) =>
            {
                _drawCallCount++;
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9), new(10, 11, 12), new(13, 14, 15), new(16, 17, 18)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            0);

        int firstResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        bool lockedAfterFirstFailure = vertexBuffer!.IsLocked;
        _vertexLockResult = Direct3D9Factory.GenericFailureHResult;
        _vertexUnlockResult = 0;
        int secondResult = renderer.Render(device, vertexBuffer, indexBuffer!);
        bool lockedAfterCleanup = vertexBuffer.IsLocked;
        _vertexLockResult = 0;
        int thirdResult = renderer.Render(device, vertexBuffer, indexBuffer);
        Vector3 retriedFirstPosition = *(Vector3*) vertexData;

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, Direct3D9Factory.GenericFailureHResult, 0, true, false, 3, 3, 1, new Vector3(10, 11, 12)),
            (firstResult, secondResult, thirdResult, lockedAfterFirstFailure, lockedAfterCleanup, _lockCallCount,
                _vertexUnlockCount, _drawCallCount, retriedFirstPosition));
    }

    [TestMethod]
    public void WhenCompletedNonIndexedGeometryHasPendingLockThenCleanupUnlockResultIsReturnedWithoutRelocking()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        _vertexData = (nint) vertexData;
        _vertexUnlockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            0);

        int firstResult = renderer.PrepareNonIndexed(vertexBuffer!, out _);
        _vertexUnlockResult = Direct3D9Factory.GenericFailureHResult;
        int secondResult = renderer.PrepareNonIndexed(vertexBuffer, out Direct3D9GeometryRenderBatch failedCleanupBatch);
        bool lockedAfterFailedCleanup = vertexBuffer.IsLocked;
        _vertexUnlockResult = 0;
        int thirdResult = renderer.PrepareNonIndexed(vertexBuffer, out Direct3D9GeometryRenderBatch successfulCleanupBatch);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, Direct3D9Factory.GenericFailureHResult, 0,
                new Direct3D9GeometryRenderBatch(0, 0, 0, false), new Direct3D9GeometryRenderBatch(0, 0, 0, false),
                true, false, 1, 3),
            (firstResult, secondResult, thirdResult, failedCleanupBatch, successfulCleanupBatch,
                lockedAfterFailedCleanup, vertexBuffer.IsLocked, _lockCallCount, _vertexUnlockCount));
    }

    [TestMethod]
    public void WhenCompletedIndexedGeometryHasPendingVertexLockThenCleanupUnlockResultIsReturnedWithoutRelocking()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        ushort* indexData = stackalloc ushort[3];
        _vertexData = (nint) vertexData;
        _indexData = (nint) indexData;
        _vertexUnlockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u],
            0);

        int firstResult = renderer.PrepareIndexed(vertexBuffer!, indexBuffer!, out _);
        _vertexUnlockResult = Direct3D9Factory.GenericFailureHResult;
        int secondResult = renderer.PrepareIndexed(vertexBuffer, indexBuffer, out Direct3D9GeometryRenderBatch failedCleanupBatch);
        bool lockedAfterFailedCleanup = vertexBuffer.IsLocked;
        _vertexUnlockResult = 0;
        int thirdResult = renderer.PrepareIndexed(vertexBuffer, indexBuffer, out Direct3D9GeometryRenderBatch successfulCleanupBatch);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, Direct3D9Factory.GenericFailureHResult, 0,
                new Direct3D9GeometryRenderBatch(0, 0, 0, false), new Direct3D9GeometryRenderBatch(0, 0, 0, false),
                true, false, 2, 3, 1),
            (firstResult, secondResult, thirdResult, failedCleanupBatch, successfulCleanupBatch,
                lockedAfterFailedCleanup, vertexBuffer.IsLocked, _lockCallCount, _vertexUnlockCount, _indexUnlockCount));
    }

    [TestMethod]
    public void WhenRenderingMultipleIndexedBatchesThenEveryDrawUsesNativeArgumentsAndLoopStopsAfterCompletion()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        ushort* indexData = stackalloc ushort[3];
        _vertexData = (nint) vertexData;
        _indexData = (nint) indexData;
        List<string> draws = [];
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawIndexedTriangleList: (baseVertex, minIndex, vertexCount, startIndex, primitiveCount) =>
            {
                draws.Add($"{baseVertex}:{minIndex}:{vertexCount}:{startIndex}:{primitiveCount}");
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u, 2u, 1u, 0u],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            (0, "0:0:3:0:1|0:0:3:0:1", 3, 1, 2),
            (result, string.Join('|', draws), _lockCallCount, _vertexUnlockCount, _indexUnlockCount));
    }

    [TestMethod]
    public void WhenFirstIndexedBatchDrawFailsThenLaterBatchesAreNotPreparedOrDrawn()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        ushort* indexData = stackalloc ushort[3];
        _vertexData = (nint) vertexData;
        _indexData = (nint) indexData;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawIndexedTriangleList: (_, _, _, _, _) =>
            {
                _drawCallCount++;
                return Direct3D9Factory.GenericFailureHResult;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u, 2u, 1u, 0u],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 2, 1, 1, 1),
            (result, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenFirstIndexedBatchDrawFailsThenNextRenderContinuesWithRemainingIndices()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        ushort* indexData = stackalloc ushort[3];
        _vertexData = (nint) vertexData;
        _indexData = (nint) indexData;
        int stateCallCount = 0;
        List<uint> baseVertices = [];
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ =>
            {
                stateCallCount++;
                return 0;
            },
            (_, _, _, _) =>
            {
                stateCallCount++;
                return 0;
            },
            _ =>
            {
                stateCallCount++;
                return 0;
            },
            drawIndexedTriangleList: (baseVertex, _, _, _, _) =>
            {
                _drawCallCount++;
                baseVertices.Add(baseVertex);
                return _drawCallCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u, 2u, 1u, 0u],
            0);

        int firstResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        int secondResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        (ushort First, ushort Second, ushort Third) copiedIndices = (indexData[0], indexData[1], indexData[2]);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 4, 3, 1, 2, 2, "0,0", (ushort) 2, (ushort) 1, (ushort) 0),
            (firstResult, secondResult, stateCallCount, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount,
                string.Join(',', baseVertices), copiedIndices.First, copiedIndices.Second, copiedIndices.Third));
    }

    [TestMethod]
    public void WhenRemainingIndexedCopyFailsAfterDrawFailureThenThirdRenderRetriesSameIndices()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        ushort* indexData = stackalloc ushort[3];
        _vertexData = (nint) vertexData;
        _indexData = (nint) indexData;
        List<uint> baseVertices = [];
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawIndexedTriangleList: (baseVertex, _, _, _, _) =>
            {
                _drawCallCount++;
                baseVertices.Add(baseVertex);
                return _drawCallCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u, 2u, 1u, 0u],
            0);

        int firstResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        _indexLockResult = Direct3D9Factory.InvalidCallHResult;
        int secondResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        _indexLockResult = 0;
        int thirdResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        (ushort First, ushort Second, ushort Third) copiedIndices = (indexData[0], indexData[1], indexData[2]);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.InvalidCallHResult, 0, 4, 1, 2, 2, "0,0", (ushort) 2, (ushort) 1, (ushort) 0),
            (firstResult, secondResult, thirdResult, _lockCallCount, _vertexUnlockCount, _indexUnlockCount, _drawCallCount,
                string.Join(',', baseVertices), copiedIndices.First, copiedIndices.Second, copiedIndices.Third));
    }

    [TestMethod]
    public void WhenFinalIndexedBatchUnlockFailsThenNextRenderRetriesBeforeObservingCompletedState()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        ushort* indexData = stackalloc ushort[3];
        _vertexData = (nint) vertexData;
        _indexData = (nint) indexData;
        int stateCallCount = 0;
        List<uint> baseVertices = [];
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ =>
            {
                stateCallCount++;
                return 0;
            },
            (_, _, _, _) =>
            {
                stateCallCount++;
                return 0;
            },
            _ =>
            {
                stateCallCount++;
                return 0;
            },
            drawIndexedTriangleList: (baseVertex, _, _, _, _) =>
            {
                _drawCallCount++;
                baseVertices.Add(baseVertex);
                return _drawCallCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u, 2u, 1u, 0u],
            0);

        int firstResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        _indexUnlockResult = Direct3D9Factory.InvalidCallHResult;
        int secondResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        _indexUnlockResult = 0;
        int thirdResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        int fourthResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        (ushort First, ushort Second, ushort Third) copiedIndices = (indexData[0], indexData[1], indexData[2]);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.InvalidCallHResult, 0, 0, 6, 4, 1, 3, 2, "0,0", (ushort) 2, (ushort) 1, (ushort) 0),
            (firstResult, secondResult, thirdResult, fourthResult, stateCallCount, _lockCallCount, _vertexUnlockCount,
                _indexUnlockCount, _drawCallCount, string.Join(',', baseVertices), copiedIndices.First, copiedIndices.Second, copiedIndices.Third));
    }

    [TestMethod]
    public void WhenFinalIndexedBatchRelockFailsThenCleanupUnlocksAndFollowingRenderRetriesSameIndices()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        ushort* indexData = stackalloc ushort[3];
        _vertexData = (nint) vertexData;
        _indexData = (nint) indexData;
        List<uint> baseVertices = [];
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawIndexedTriangleList: (baseVertex, _, _, _, _) =>
            {
                _drawCallCount++;
                baseVertices.Add(baseVertex);
                return _drawCallCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u, 2u, 1u, 0u],
            0);

        int firstResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        _indexUnlockResult = Direct3D9Factory.InvalidCallHResult;
        int secondResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        _indexLockResult = Direct3D9Factory.DriverInternalErrorHResult;
        _indexUnlockResult = 0;
        int thirdResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        bool isLockedAfterCleanup = indexBuffer!.IsLocked;
        _indexLockResult = 0;
        int fourthResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        (ushort First, ushort Second, ushort Third) copiedIndices = (indexData[0], indexData[1], indexData[2]);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.InvalidCallHResult,
                Direct3D9Factory.DriverInternalErrorHResult, 0, false, 5, 1, 4, 2, "0,0", (ushort) 2, (ushort) 1, (ushort) 0),
            (firstResult, secondResult, thirdResult, fourthResult, isLockedAfterCleanup, _lockCallCount,
                _vertexUnlockCount, _indexUnlockCount, _drawCallCount, string.Join(',', baseVertices),
                copiedIndices.First, copiedIndices.Second, copiedIndices.Third));
    }

    [TestMethod]
    public void WhenFinalIndexedBatchDrawFailsThenNextRenderObservesCompletedState()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        ushort* indexData = stackalloc ushort[3];
        _vertexData = (nint) vertexData;
        _indexData = (nint) indexData;
        int stateCallCount = 0;
        List<uint> baseVertices = [];
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ =>
            {
                stateCallCount++;
                return 0;
            },
            (_, _, _, _) =>
            {
                stateCallCount++;
                return 0;
            },
            _ =>
            {
                stateCallCount++;
                return 0;
            },
            drawIndexedTriangleList: (baseVertex, _, _, _, _) =>
            {
                _drawCallCount++;
                baseVertices.Add(baseVertex);
                return Direct3D9Factory.GenericFailureHResult;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u, 2u, 1u, 0u],
            0);

        int firstResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        int secondResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        int thirdResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        (ushort First, ushort Second, ushort Third) copiedIndices = (indexData[0], indexData[1], indexData[2]);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult, 0, 5, 3, 1, 2, 2, "0,0", (ushort) 2, (ushort) 1, (ushort) 0),
            (firstResult, secondResult, thirdResult, stateCallCount, _lockCallCount, _vertexUnlockCount, _indexUnlockCount,
                _drawCallCount, string.Join(',', baseVertices), copiedIndices.First, copiedIndices.Second, copiedIndices.Third));
    }

    [TestMethod]
    public void WhenRenderingMultipleNonIndexedBatchesThenEveryDrawUsesPreparedArgumentsAndLoopStopsAfterCompletion()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        _vertexData = (nint) vertexData;
        List<string> draws = [];
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawTriangleList: (startVertex, primitiveCount) =>
            {
                draws.Add($"{startVertex}:{primitiveCount}");
                return 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9), new(10, 11, 12), new(13, 14, 15), new(16, 17, 18)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual((0, "0:1|0:1", 2, 2), (result, string.Join('|', draws), _lockCallCount, _vertexUnlockCount));
    }

    [TestMethod]
    public void WhenFirstNonIndexedBatchDrawFailsThenLaterBatchesAreNotPreparedOrDrawn()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        _vertexData = (nint) vertexData;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawTriangleList: (_, _) =>
            {
                _drawCallCount++;
                return Direct3D9Factory.GenericFailureHResult;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9), new(10, 11, 12), new(13, 14, 15), new(16, 17, 18)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            0);

        int result = renderer.Render(device, vertexBuffer!, indexBuffer!);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, 1, 1),
            (result, _lockCallCount, _vertexUnlockCount, _drawCallCount));
    }

    [TestMethod]
    public void WhenFirstNonIndexedBatchDrawFailsThenNextRenderContinuesWithRemainingBatch()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _indexBufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _vertexBufferIdentity = _vertexBufferToReturn;
        byte* vertexData = stackalloc byte[72];
        _vertexData = (nint) vertexData;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            _ => 0,
            (_, _, _, _) => 0,
            _ => 0,
            drawTriangleList: (_, _) =>
            {
                _drawCallCount++;
                return _drawCallCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
            });
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9), new(10, 11, 12), new(13, 14, 15), new(16, 17, 18)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            0);

        int firstResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        int secondResult = renderer.Render(device, vertexBuffer!, indexBuffer!);
        Vector3 copiedFirstPosition = *(Vector3*) vertexData;

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 2, 2, 2, new Vector3(10, 11, 12)),
            (firstResult, secondResult, _lockCallCount, _vertexUnlockCount, _drawCallCount, copiedFirstPosition));
    }

    [TestMethod]
    public void WhenDrawReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            drawTriangleList: (_, _) => Direct3D9Factory.DriverInternalErrorHResult);

        int result = device.DrawTriangleList(3, 2);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    private static Direct3D9Device CreateDevice(
        IDirect3DDevice9* device,
        Direct3D9SetFlexibleVertexFormat setFlexibleVertexFormat,
        Direct3D9SetStreamSource setStreamSource,
        Direct3D9SetIndices setIndices,
        Direct3D9DrawIndexedTriangleList? drawIndexedTriangleList = null,
        Direct3D9DrawTriangleList? drawTriangleList = null)
    {
        return new Direct3D9Device(
            device,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            setFlexibleVertexFormat: setFlexibleVertexFormat,
            setStreamSource: setStreamSource,
            setIndices: setIndices,
            drawIndexedTriangleList: drawIndexedTriangleList,
            drawTriangleList: drawTriangleList);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateVertexBuffer(
        IDirect3DDevice9* self,
        uint length,
        uint usage,
        uint flexibleVertexFormat,
        Pool pool,
        IDirect3DVertexBuffer9** buffer,
        void** sharedHandle)
    {
        *buffer = (IDirect3DVertexBuffer9*) _vertexBufferToReturn;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateIndexBuffer(
        IDirect3DDevice9* self,
        uint length,
        uint usage,
        Format format,
        Pool pool,
        IDirect3DIndexBuffer9** buffer,
        void** sharedHandle)
    {
        *buffer = (IDirect3DIndexBuffer9*) _indexBufferToReturn;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockBuffer(void* self, uint offset, uint size, void** data, uint flags)
    {
        _lockCallCount++;
        bool isVertexBuffer = (nint) self == _vertexBufferIdentity;
        *data = (void*) (isVertexBuffer ? _vertexData : _indexData);
        return isVertexBuffer ? _vertexLockResult : _indexLockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UnlockBuffer(void* self)
    {
        if ((nint) self == _vertexBufferIdentity)
        {
            _vertexUnlockCount++;
            return _vertexUnlockResult;
        }

        _indexUnlockCount++;
        return _indexUnlockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseBuffer(void* self) => 0;

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 30);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[26] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, Pool, IDirect3DVertexBuffer9**, void**, int>) &CreateVertexBuffer;
            vtable[27] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, Pool, IDirect3DIndexBuffer9**, void**, int>) &CreateIndexBuffer;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private struct FakeBufferObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DVertexBuffer9* VertexBuffer;
        internal IDirect3DIndexBuffer9* IndexBuffer;

        public FakeBufferObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 14);
            void** memory = (void**) _memory;
            VertexBuffer = (IDirect3DVertexBuffer9*) memory;
            IndexBuffer = (IDirect3DIndexBuffer9*) memory;
            void** vtable = memory + 1;
            VertexBuffer->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &ReleaseBuffer;
            vtable[11] = (void*) (delegate* unmanaged[Stdcall]<void*, uint, uint, void**, uint, int>) &LockBuffer;
            vtable[12] = (void*) (delegate* unmanaged[Stdcall]<void*, int>) &UnlockBuffer;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            VertexBuffer = null;
            IndexBuffer = null;
        }
    }
}
