using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceDrawTests
{
    private static Primitivetype _primitiveType;
    private static int _baseVertexIndex;
    private static uint _minIndex;
    private static uint _vertexCount;
    private static uint _startIndexOrVertex;
    private static uint _primitiveCount;
    private static int _drawResult;
    private static int _drawCallCount;
    private static int _setStreamSourceResult;
    private static int _bufferLockResult;
    private static int _bufferUnlockResult;
    private static nint _vertexBuffer;
    private static nint _lockedBufferData;
    private static uint _lockedBufferSize;
    private static byte[] _copiedVertexBytes = [];
    private static uint _flexibleVertexFormat;
    private static Direct3D9VertexXyzDiffuseUv2[] _primitiveVertices = [];
    private static readonly List<(Primitivetype PrimitiveType, uint PrimitiveCount, nint Data, uint Stride)> PrimitiveUpCalls = [];
    private static readonly List<(uint StreamNumber, nint StreamData, uint Offset, uint Stride)> StreamSourceCalls = [];

    [TestInitialize]
    public void Initialize()
    {
        _primitiveType = 0;
        _baseVertexIndex = 0;
        _minIndex = 0;
        _vertexCount = 0;
        _startIndexOrVertex = 0;
        _primitiveCount = 0;
        _drawResult = 0;
        _drawCallCount = 0;
        _setStreamSourceResult = 0;
        _bufferLockResult = 0;
        _bufferUnlockResult = 0;
        _vertexBuffer = 0;
        _lockedBufferData = 0;
        _lockedBufferSize = 0;
        _copiedVertexBytes = [];
        _flexibleVertexFormat = 0;
        _primitiveVertices = [];
        PrimitiveUpCalls.Clear();
        StreamSourceCalls.Clear();
    }

    [TestMethod]
    public void WhenDrawingIndexedTriangleListThenNativeSlot82ReceivesArguments()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.DrawIndexedTriangleList(7, 2, 11, 5, 3);

        Assert.AreEqual(
            (0, Primitivetype.Trianglelist, 7, 2u, 11u, 5u, 3u),
            (result, _primitiveType, _baseVertexIndex, _minIndex, _vertexCount, _startIndexOrVertex, _primitiveCount));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenDrawingIndexedTriangleListFailsThenOrdinaryResultIsPreserved(int expectedResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _drawResult = expectedResult;

        int result = device.DrawIndexedTriangleList(uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue);

        Assert.AreEqual(
            (expectedResult, 0, Primitivetype.Trianglelist, -1, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue),
            (result, device.UnusableReasonHResult, _primitiveType, _baseVertexIndex, _minIndex, _vertexCount,
                _startIndexOrVertex, _primitiveCount));
    }

    [TestMethod]
    public void WhenDrawingIndexedTriangleListFailsThenDriverInternalErrorIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _drawResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.DrawIndexedTriangleList(7, 2, 11, 5, 3);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenDrawingIndexedTriangleListOnUnusableDeviceThenNativeCallStillOccurs()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.DrawIndexedTriangleList(7, 2, 11, 5, 3);

        Assert.AreEqual((0, 1, Primitivetype.Trianglelist, 7, 2u, 11u, 5u, 3u),
            (result, _drawCallCount, _primitiveType, _baseVertexIndex, _minIndex, _vertexCount,
                _startIndexOrVertex, _primitiveCount));
    }

    [TestMethod]
    public void WhenDrawingIndexedTriangleListAfterDisposeThenThrowsWithoutNativeCall()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.DrawIndexedTriangleList(0, 0, 0, 0, 0));

        Assert.AreEqual(0, _drawCallCount);
    }

    [TestMethod]
    public void WhenDrawingTriangleListThenNativeSlot81ReceivesArguments()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.DrawTriangleList(9, 4);

        Assert.AreEqual(
            (0, Primitivetype.Trianglelist, 9u, 4u),
            (result, _primitiveType, _startIndexOrVertex, _primitiveCount));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenDrawingTriangleListFailsThenOrdinaryResultIsPreserved(int expectedResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _drawResult = expectedResult;

        int result = device.DrawTriangleList(uint.MaxValue, uint.MaxValue);

        Assert.AreEqual((expectedResult, 0), (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenDrawingTriangleListFailsThenDriverInternalErrorIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _drawResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.DrawTriangleList(9, 4);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenDrawingTriangleListOnUnusableDeviceThenNativeCallStillOccurs()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.DrawTriangleList(3, 2);

        Assert.AreEqual((0, 1, Primitivetype.Trianglelist, 3u, 2u),
            (result, _drawCallCount, _primitiveType, _startIndexOrVertex, _primitiveCount));
    }

    [TestMethod]
    public void WhenDrawingTriangleListAfterDisposeThenThrowsWithoutNativeCall()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.DrawTriangleList(0, 1));

        Assert.AreEqual(0, _drawCallCount);
    }

    [TestMethod]
    public void WhenDrawingTriangleStripThenNativeSlot81ReceivesArguments()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.DrawTriangleStrip(6, 5);

        Assert.AreEqual(
            (0, Primitivetype.Trianglestrip, 6u, 5u),
            (result, _primitiveType, _startIndexOrVertex, _primitiveCount));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenDrawingTriangleStripFailsThenOrdinaryResultIsPreserved(int expectedResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _drawResult = expectedResult;

        int result = device.DrawTriangleStrip(uint.MaxValue, uint.MaxValue);

        Assert.AreEqual((expectedResult, 0), (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenDrawingTriangleStripFailsThenDriverInternalErrorIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _drawResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.DrawTriangleStrip(0, 1);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenDrawingTriangleStripOnUnusableDeviceThenNativeCallStillOccurs()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.DrawTriangleStrip(3, 2);

        Assert.AreEqual((0, 1, Primitivetype.Trianglestrip, 3u, 2u),
            (result, _drawCallCount, _primitiveType, _startIndexOrVertex, _primitiveCount));
    }

    [TestMethod]
    public void WhenDrawingTriangleStripAfterDisposeThenThrowsWithoutNativeCall()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.DrawTriangleStrip(0, 1));

        Assert.AreEqual(0, _drawCallCount);
    }

    [TestMethod]
    public void WhenDynamicVertexBufferIsAvailableThenTriangleFanUsesFastPath()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.Buffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Assert.AreEqual(0, device.InitializeDynamicBuffers());
        byte* vertices = stackalloc byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        int result = device.DrawPrimitiveUp(Primitivetype.Trianglefan, 2, vertices, 2);

        Assert.AreEqual((0, Primitivetype.Trianglefan, 0u, 2u, 0),
            (result, _primitiveType, _startIndexOrVertex, _primitiveCount, PrimitiveUpCalls.Count));
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, _copiedVertexBytes);
        Assert.AreEqual((nint) vertexBufferObject.Buffer, StreamSourceCalls[^1].StreamData);
    }

    [TestMethod]
    [DataRow(Primitivetype.Linelist, 2u, 4u)]
    [DataRow(Primitivetype.Trianglelist, 2u, 6u)]
    [DataRow(Primitivetype.Trianglestrip, 2u, 4u)]
    public void WhenDynamicVertexBufferIsAvailableThenSupportedPrimitiveUsesExpectedVertexCount(
        Primitivetype primitiveType,
        uint primitiveCount,
        uint expectedVertexCount)
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.Buffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Assert.AreEqual(0, device.InitializeDynamicBuffers());
        byte* vertices = stackalloc byte[checked((int) expectedVertexCount)];

        int result = device.DrawPrimitiveUp(primitiveType, primitiveCount, vertices, 1);

        Assert.AreEqual((0, primitiveType, primitiveCount, checked((int) expectedVertexCount), 0),
            (result, _primitiveType, _primitiveCount, _copiedVertexBytes.Length, PrimitiveUpCalls.Count));
    }

    [TestMethod]
    public void WhenDynamicVertexBufferLockFailsThenNativePrimitiveUpFallbackIsUsed()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.Buffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Assert.AreEqual(0, device.InitializeDynamicBuffers());
        _bufferLockResult = Direct3D9Factory.GenericFailureHResult;
        byte* vertices = stackalloc byte[6];

        int result = device.DrawPrimitiveUp(Primitivetype.Trianglelist, 2, vertices, 1);

        Assert.AreEqual((0, 1, 1, 0),
            (result, PrimitiveUpCalls.Count, StreamSourceCalls.Count, _copiedVertexBytes.Length));
        Assert.AreEqual(0, StreamSourceCalls[0].StreamData);
    }

    [TestMethod]
    public void WhenDynamicVertexBufferUnlockFailsThenDrawStopsWithoutFallback()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.Buffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Assert.AreEqual(0, device.InitializeDynamicBuffers());
        _bufferUnlockResult = Direct3D9Factory.GenericFailureHResult;
        byte* vertices = stackalloc byte[6];

        int result = device.DrawPrimitiveUp(Primitivetype.Trianglelist, 2, vertices, 1);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 0u),
            (result, PrimitiveUpCalls.Count, _primitiveCount));
    }

    [TestMethod]
    public void WhenFastPathDrawFailsThenDriverInternalErrorIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.Buffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Assert.AreEqual(0, device.InitializeDynamicBuffers());
        _drawResult = Direct3D9Factory.DriverInternalErrorHResult;
        byte* vertices = stackalloc byte[6];

        int result = device.DrawPrimitiveUp(Primitivetype.Trianglelist, 2, vertices, 1);

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 0),
            (result, device.UnusableReasonHResult, PrimitiveUpCalls.Count));
    }

    [TestMethod]
    public void WhenDrawingPrimitiveUpAfterDisposeThenThrows()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.DrawPrimitiveUp(Primitivetype.Linelist, 1, (void*) 1, 1));
    }

    [TestMethod]
    public void WhenDrawingLargeTriangleListThenCallsAreSplitAndVertexPointerAdvances()
    {
        using FakeDeviceObject deviceObject = new();
        Caps9 capabilities = default;
        capabilities.MaxPrimitiveCount = 2;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        byte* vertices = stackalloc byte[15 * 4];

        int result = device.DrawPrimitiveUp(Primitivetype.Trianglelist, 5, vertices, 4);

        CollectionAssert.AreEqual(
            new[]
            {
                (Primitivetype.Trianglelist, 2u, (nint) vertices, 4u),
                (Primitivetype.Trianglelist, 2u, (nint) (vertices + 24), 4u),
                (Primitivetype.Trianglelist, 1u, (nint) (vertices + 48), 4u)
            },
            PrimitiveUpCalls);
        Assert.AreEqual((0, 1), (result, StreamSourceCalls.Count));
    }

    [TestMethod]
    public void WhenDrawingLargeLineListThenCallsAreSplitAndTwoVerticesPerPrimitiveAreSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        Caps9 capabilities = default;
        capabilities.MaxPrimitiveCount = 2;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        byte* vertices = stackalloc byte[10 * 4];

        int result = device.DrawPrimitiveUp(Primitivetype.Linelist, 5, vertices, 4);

        CollectionAssert.AreEqual(
            new[]
            {
                (Primitivetype.Linelist, 2u, (nint) vertices, 4u),
                (Primitivetype.Linelist, 2u, (nint) (vertices + 16), 4u),
                (Primitivetype.Linelist, 1u, (nint) (vertices + 32), 4u)
            },
            PrimitiveUpCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenDrawingLargeTriangleStripThenLastTwoVerticesAreReusedAcrossCalls()
    {
        using FakeDeviceObject deviceObject = new();
        Caps9 capabilities = default;
        capabilities.MaxPrimitiveCount = 2;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        byte* vertices = stackalloc byte[7 * 8];

        int result = device.DrawPrimitiveUp(Primitivetype.Trianglestrip, 5, vertices, 8);

        CollectionAssert.AreEqual(
            new[]
            {
                (Primitivetype.Trianglestrip, 2u, (nint) vertices, 8u),
                (Primitivetype.Trianglestrip, 2u, (nint) (vertices + 16), 8u),
                (Primitivetype.Trianglestrip, 1u, (nint) (vertices + 32), 8u)
            },
            PrimitiveUpCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenMaximumPrimitiveCountIsFFFFThenTriangleListIsSplitByUniqueVertexLimit()
    {
        using FakeDeviceObject deviceObject = new();
        Caps9 capabilities = default;
        capabilities.MaxPrimitiveCount = ushort.MaxValue;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        uint primitiveCount = (ushort.MaxValue / 3) + 1u;
        byte* vertices = stackalloc byte[1];

        int result = device.DrawPrimitiveUp(Primitivetype.Trianglelist, primitiveCount, vertices, 0);

        CollectionAssert.AreEqual(
            new[]
            {
                (Primitivetype.Trianglelist, (uint) (ushort.MaxValue / 3), (nint) vertices, 0u),
                (Primitivetype.Trianglelist, 1u, (nint) vertices, 0u)
            },
            PrimitiveUpCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenMaximumPrimitiveCountIsFFFFThenTriangleStripIsSplitByUniqueVertexLimit()
    {
        using FakeDeviceObject deviceObject = new();
        Caps9 capabilities = default;
        capabilities.MaxPrimitiveCount = ushort.MaxValue;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        uint primitiveCount = ushort.MaxValue - 1u;
        byte* vertices = stackalloc byte[1];

        int result = device.DrawPrimitiveUp(Primitivetype.Trianglestrip, primitiveCount, vertices, 0);

        CollectionAssert.AreEqual(
            new[]
            {
                (Primitivetype.Trianglestrip, ushort.MaxValue - 2u, (nint) vertices, 0u),
                (Primitivetype.Trianglestrip, 1u, (nint) vertices, 0u)
            },
            PrimitiveUpCalls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenLargeTriangleFanIsRequestedThenReturnsNotImplementedWithoutCallingDriver()
    {
        using FakeDeviceObject deviceObject = new();
        Caps9 capabilities = default;
        capabilities.MaxPrimitiveCount = 1;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        byte* vertices = stackalloc byte[4 * 4];

        int result = device.DrawPrimitiveUp(Primitivetype.Trianglefan, 2, vertices, 4);

        Assert.AreEqual((Direct3D9Factory.NotImplementedHResult, 0, 0), (result, PrimitiveUpCalls.Count, StreamSourceCalls.Count));
    }

    [TestMethod]
    public void WhenStreamResetFailsThenDrawStopsAndDriverInternalErrorIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        Caps9 capabilities = default;
        capabilities.MaxPrimitiveCount = 1;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        _setStreamSourceResult = Direct3D9Factory.DriverInternalErrorHResult;
        byte* vertices = stackalloc byte[6 * 4];

        int result = device.DrawPrimitiveUp(Primitivetype.Trianglelist, 2, vertices, 4);

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, 0, Direct3D9Factory.DriverInternalErrorHResult),
            (result, PrimitiveUpCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenLargeDrawFailsThenSubsequentBatchesAreNotSubmittedAndErrorIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        Caps9 capabilities = default;
        capabilities.MaxPrimitiveCount = 2;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        _drawResult = Direct3D9Factory.DriverInternalErrorHResult;
        byte* vertices = stackalloc byte[9 * 4];

        int result = device.DrawPrimitiveUp(Primitivetype.Trianglelist, 3, vertices, 4);

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, 1, Direct3D9Factory.DriverInternalErrorHResult),
            (result, PrimitiveUpCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenPrimitiveFanIsStartedAndEndedThenDeviceOwnedBufferIsClearedAndDrawn()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBuffer firstBuffer));
        Assert.AreEqual(0, firstBuffer.GetNewVertices(out Span<Direct3D9VertexXyzDiffuseUv2> vertices));
        vertices[0] = new Direct3D9VertexXyzDiffuseUv2(1, 2, 3, 4);
        Assert.AreEqual(0, device.EndPrimitiveFan(firstBuffer));
        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBuffer secondBuffer));

        Assert.AreEqual(
            ((uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex2), true, 0),
            (_flexibleVertexFormat, ReferenceEquals(firstBuffer, secondBuffer), secondBuffer.VertexCount));
        Assert.AreEqual(
            (Primitivetype.Trianglefan, 2u, (uint) sizeof(Direct3D9VertexXyzDiffuseUv2)),
            (PrimitiveUpCalls[0].PrimitiveType, PrimitiveUpCalls[0].PrimitiveCount, PrimitiveUpCalls[0].Stride));
        Assert.AreEqual(new Direct3D9VertexXyzDiffuseUv2(1, 2, 3, 4), _primitiveVertices[0]);
    }

    [TestMethod]
    public void WhenVertexRequestExactlyConsumesRemainingCapacityThenBufferGrowsFirst()
    {
        Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzDiffuseUv2> buffer = new();

        Assert.AreEqual(0, buffer.GetNewVertices(out _));
        Assert.AreEqual(0, buffer.GetNewVertices(out _));

        Direct3D9VertexXyzDiffuseUv2[] vertices = (Direct3D9VertexXyzDiffuseUv2[]) buffer.GetType()
            .GetField("_vertices", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(buffer)!;
        Assert.AreEqual(16, vertices.Length);
    }

    [TestMethod]
    public void WhenDuv6PrimitiveFanIsStartedAndEndedThenMatchingFvfAndStrideAreUsed()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBufferDuv6 firstBuffer));
        Assert.AreEqual(0, firstBuffer.GetNewVertices(out _));
        Assert.AreEqual(0, device.EndPrimitiveFan(firstBuffer));
        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBufferDuv6 secondBuffer));

        Assert.AreEqual(
            ((uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex6), true, 0),
            (_flexibleVertexFormat, ReferenceEquals(firstBuffer, secondBuffer), secondBuffer.VertexCount));
        Assert.AreEqual(
            (Primitivetype.Trianglefan, 2u, 64u),
            (PrimitiveUpCalls[0].PrimitiveType, PrimitiveUpCalls[0].PrimitiveCount, PrimitiveUpCalls[0].Stride));
    }

    [TestMethod]
    public void WhenXyzNormalDiffuseSpecularUv4PrimitiveFanIsStartedAndEndedThenMatchingFvfAndStrideAreUsed()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBufferXyzNormalDiffuseSpecularUv4 firstBuffer));
        Assert.AreEqual(0, firstBuffer.GetNewVertices(out _));
        Assert.AreEqual(0, device.EndPrimitiveFan(firstBuffer));
        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBufferXyzNormalDiffuseSpecularUv4 secondBuffer));

        Assert.AreEqual(
            ((uint) (D3D9.FvfXyz | D3D9.FvfNormal | D3D9.FvfDiffuse | D3D9.FvfSpecular | D3D9.FvfTex4), true, 0),
            (_flexibleVertexFormat, ReferenceEquals(firstBuffer, secondBuffer), secondBuffer.VertexCount));
        Assert.AreEqual(
            (Primitivetype.Trianglefan, 2u, 64u),
            (PrimitiveUpCalls[0].PrimitiveType, PrimitiveUpCalls[0].PrimitiveCount, PrimitiveUpCalls[0].Stride));
    }

    [TestMethod]
    public void WhenDuv2PrimitiveStartFailsThenOwnedBufferIsClearedAndResultIsPreserved()
    {
        const int expectedResult = Direct3D9Factory.GenericFailureHResult;
        int setFvfCallCount = 0;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, setFlexibleVertexFormat: flexibleVertexFormat =>
        {
            setFvfCallCount++;
            _flexibleVertexFormat = flexibleVertexFormat;
            return expectedResult;
        });

        int result = device.StartPrimitive(out Direct3D9PrimitiveVertexBuffer vertexBuffer);
        Assert.AreEqual(0, vertexBuffer.GetNewVertices(out _));
        result = device.StartPrimitive(out Direct3D9PrimitiveVertexBuffer secondBuffer);

        Assert.AreEqual(
            (expectedResult, 2, (uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex2), true, 0, 0, 0),
            (result, setFvfCallCount, _flexibleVertexFormat, ReferenceEquals(vertexBuffer, secondBuffer), secondBuffer.VertexCount,
                PrimitiveUpCalls.Count, StreamSourceCalls.Count));
    }

    [TestMethod]
    public void WhenDuv6PrimitiveStartReturnsNonzeroSuccessThenResultIsPreservedAndSubsequentStartClearsOwnedBuffer()
    {
        const int expectedResult = 1;
        int setFvfCallCount = 0;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, setFlexibleVertexFormat: flexibleVertexFormat =>
        {
            setFvfCallCount++;
            _flexibleVertexFormat = flexibleVertexFormat;
            return expectedResult;
        });

        int result = device.StartPrimitive(out Direct3D9PrimitiveVertexBufferDuv6 vertexBuffer);
        Assert.AreEqual(0, vertexBuffer.GetNewVertices(out _));
        int cachedResult = device.StartPrimitive(out Direct3D9PrimitiveVertexBufferDuv6 secondBuffer);

        Assert.AreEqual(
            (expectedResult, 0, 1, (uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex6), true, 0, 0, 0),
            (result, cachedResult, setFvfCallCount, _flexibleVertexFormat, ReferenceEquals(vertexBuffer, secondBuffer), secondBuffer.VertexCount,
                PrimitiveUpCalls.Count, StreamSourceCalls.Count));
    }

    [TestMethod]
    public void WhenXyzNormalDiffuseSpecularUv4PrimitiveStartFailsThenOwnedBufferIsClearedAndResultIsPreserved()
    {
        const int expectedResult = Direct3D9Factory.DeviceLostHResult;
        int setFvfCallCount = 0;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, setFlexibleVertexFormat: flexibleVertexFormat =>
        {
            setFvfCallCount++;
            _flexibleVertexFormat = flexibleVertexFormat;
            return expectedResult;
        });

        int result = device.StartPrimitive(out Direct3D9PrimitiveVertexBufferXyzNormalDiffuseSpecularUv4 vertexBuffer);
        Assert.AreEqual(0, vertexBuffer.GetNewVertices(out _));
        result = device.StartPrimitive(out Direct3D9PrimitiveVertexBufferXyzNormalDiffuseSpecularUv4 secondBuffer);

        Assert.AreEqual(
            (expectedResult, 2, (uint) (D3D9.FvfXyz | D3D9.FvfNormal | D3D9.FvfDiffuse | D3D9.FvfSpecular | D3D9.FvfTex4), true, 0, 0, 0),
            (result, setFvfCallCount, _flexibleVertexFormat, ReferenceEquals(vertexBuffer, secondBuffer), secondBuffer.VertexCount,
                PrimitiveUpCalls.Count, StreamSourceCalls.Count));
    }

    [TestMethod]
    public void WhenDifferentPrimitiveFormatsAreStartedThenOwnedBufferIdentitiesRemainIsolated()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBuffer duv2Buffer));
        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBufferDuv6 duv6Buffer));
        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBufferXyzNormalDiffuseSpecularUv4 xyzNdsUv4Buffer));

        Assert.AreEqual(
            (false, false, false),
            (ReferenceEquals(duv2Buffer, duv6Buffer), ReferenceEquals(duv2Buffer, xyzNdsUv4Buffer), ReferenceEquals(duv6Buffer, xyzNdsUv4Buffer)));
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    public void WhenPrimitiveFanHasFewerThanThreeVerticesThenDriverIsNotCalled(int vertexCount)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9PrimitiveVertexBuffer vertexBuffer = CreatePrimitiveVertexBuffer(vertexCount);

        int result = device.EndPrimitiveFan(vertexBuffer);

        Assert.AreEqual((0, 0), (result, PrimitiveUpCalls.Count));
    }

    [TestMethod]
    public void WhenDuv2PrimitiveFanDrawReturnsNonzeroSuccessThenResultAndBufferArePreserved()
    {
        const int expectedResult = 1;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _drawResult = expectedResult;
        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBuffer vertexBuffer));
        Assert.AreEqual(0, vertexBuffer.GetNewVertices(out Span<Direct3D9VertexXyzDiffuseUv2> vertices));
        vertices[0] = new Direct3D9VertexXyzDiffuseUv2(1, 2, 3, 4);

        int result = device.EndPrimitiveFan(vertexBuffer);

        Assert.AreEqual(
            (expectedResult, 4, 1, Primitivetype.Trianglefan, 2u, (uint) sizeof(Direct3D9VertexXyzDiffuseUv2),
                new Direct3D9VertexXyzDiffuseUv2(1, 2, 3, 4), 0),
            (result, vertexBuffer.VertexCount, PrimitiveUpCalls.Count, PrimitiveUpCalls[0].PrimitiveType,
                PrimitiveUpCalls[0].PrimitiveCount, PrimitiveUpCalls[0].Stride, _primitiveVertices[0],
                device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenDuv6PrimitiveFanDrawFailsThenOrdinaryResultAndBufferArePreserved()
    {
        const int expectedResult = Direct3D9Factory.GenericFailureHResult;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _drawResult = expectedResult;
        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBufferDuv6 vertexBuffer));
        Assert.AreEqual(0, vertexBuffer.GetNewVertices(out _));

        int result = device.EndPrimitiveFan(vertexBuffer);

        Assert.AreEqual(
            (expectedResult, 4, 1, Primitivetype.Trianglefan, 2u, 64u, 0),
            (result, vertexBuffer.VertexCount, PrimitiveUpCalls.Count, PrimitiveUpCalls[0].PrimitiveType,
                PrimitiveUpCalls[0].PrimitiveCount, PrimitiveUpCalls[0].Stride, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenXyzNormalDiffuseSpecularUv4PrimitiveFanDrawLosesDeviceThenResultAndBufferArePreserved()
    {
        const int expectedResult = Direct3D9Factory.DeviceLostHResult;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _drawResult = expectedResult;
        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBufferXyzNormalDiffuseSpecularUv4 vertexBuffer));
        Assert.AreEqual(0, vertexBuffer.GetNewVertices(out _));

        int result = device.EndPrimitiveFan(vertexBuffer);

        Assert.AreEqual(
            (expectedResult, 4, 1, Primitivetype.Trianglefan, 2u, 64u, 0),
            (result, vertexBuffer.VertexCount, PrimitiveUpCalls.Count, PrimitiveUpCalls[0].PrimitiveType,
                PrimitiveUpCalls[0].PrimitiveCount, PrimitiveUpCalls[0].Stride, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenPrimitiveFanDrawFailsThenDriverInternalErrorIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _drawResult = Direct3D9Factory.DriverInternalErrorHResult;
        Assert.AreEqual(0, device.StartPrimitive(out Direct3D9PrimitiveVertexBuffer vertexBuffer));
        Assert.AreEqual(0, vertexBuffer.GetNewVertices(out _));

        int result = device.EndPrimitiveFan(vertexBuffer);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenDrawingVideoToSurfaceThenBeginRenderReceivesDeviceAndReturnsBitmapSource()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9Device? receivedDevice = null;

        int result = device.DrawVideoToSurface(
            (Direct3D9Device currentDevice, out nint bitmapSource) =>
            {
                receivedDevice = currentDevice;
                bitmapSource = 42;
                return 0;
            },
            out nint bitmapSource);

        Assert.AreEqual((0, 42, true), (result, bitmapSource, ReferenceEquals(device, receivedDevice)));
    }

    [TestMethod]
    public void WhenDrawingVideoToSurfaceFailsThenResultAndOutputArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.DrawVideoToSurface(
            static (Direct3D9Device _, out nint bitmapSource) =>
            {
                bitmapSource = 43;
                return Direct3D9Factory.GenericFailureHResult;
            },
            out nint bitmapSource);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 43), (result, bitmapSource));
    }

    [TestMethod]
    public void WhenDrawingVideoToSurfaceGetsDriverInternalErrorThenResultIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.DrawVideoToSurface(
            static (Direct3D9Device _, out nint bitmapSource) =>
            {
                bitmapSource = 0;
                return Direct3D9Factory.DriverInternalErrorHResult;
            },
            out _);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenDrawingVideoToSurfaceAfterDisposeThenThrows()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.DrawVideoToSurface(
            static (Direct3D9Device _, out nint bitmapSource) =>
            {
                bitmapSource = 0;
                return 0;
            },
            out _));
    }

    [TestMethod]
    public void WhenGettingXyzDuv2VertexBufferRepeatedlyThenStableDeviceOwnedIdentityAndContentsArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzDiffuseUv2> firstBuffer = device.GetVertexBufferXyzDuv2();
        Assert.AreEqual(0, firstBuffer.GetNewVertices(out _));
        Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzDiffuseUv2> secondBuffer = device.GetVertexBufferXyzDuv2();

        Assert.AreEqual((true, 4), (ReferenceEquals(firstBuffer, secondBuffer), secondBuffer.VertexCount));
    }

    [TestMethod]
    public void WhenGettingXyzRhwDuv8VertexBufferRepeatedlyThenStableDeviceOwnedIdentityAndContentsArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzDiffuseUv8> firstBuffer = device.GetVertexBufferXyzRhwDuv8();
        Assert.AreEqual(0, firstBuffer.GetNewVertices(out _));
        Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzDiffuseUv8> secondBuffer = device.GetVertexBufferXyzRhwDuv8();

        Assert.AreEqual((true, 4), (ReferenceEquals(firstBuffer, secondBuffer), secondBuffer.VertexCount));
    }

    [TestMethod]
    public void WhenGettingDeviceOwnedVertexBuffersThenTypesAreIsolatedAndDeviceStateIsUnchanged()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        object xyzDuv2Buffer = device.GetVertexBufferXyzDuv2();
        object xyzRhwDuv8Buffer = device.GetVertexBufferXyzRhwDuv8();

        Assert.AreEqual((false, 0u, 0, 0),
            (ReferenceEquals(xyzDuv2Buffer, xyzRhwDuv8Buffer), _flexibleVertexFormat, _drawCallCount, PrimitiveUpCalls.Count));
    }

    [TestMethod]
    public void WhenGettingDeviceOwnedVertexBuffersAfterDisposeThenThrows()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.GetVertexBufferXyzDuv2());
        Assert.ThrowsExactly<ObjectDisposedException>(() => device.GetVertexBufferXyzRhwDuv8());
    }

    [TestMethod]
    public void WhenStartingOrEndingPrimitiveAfterDisposeThenThrows()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9PrimitiveVertexBuffer vertexBuffer = new();
        Direct3D9PrimitiveVertexBufferDuv6 duv6VertexBuffer = new();
        Direct3D9PrimitiveVertexBufferXyzNormalDiffuseSpecularUv4 xyzNdsUv4VertexBuffer = new();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.StartPrimitive(out Direct3D9PrimitiveVertexBuffer _));
        Assert.ThrowsExactly<ObjectDisposedException>(() => device.StartPrimitive(out Direct3D9PrimitiveVertexBufferDuv6 _));
        Assert.ThrowsExactly<ObjectDisposedException>(() => device.StartPrimitive(out Direct3D9PrimitiveVertexBufferXyzNormalDiffuseSpecularUv4 _));
        Assert.ThrowsExactly<ObjectDisposedException>(() => device.EndPrimitiveFan(vertexBuffer));
        Assert.ThrowsExactly<ObjectDisposedException>(() => device.EndPrimitiveFan(duv6VertexBuffer));
        Assert.ThrowsExactly<ObjectDisposedException>(() => device.EndPrimitiveFan(xyzNdsUv4VertexBuffer));
    }

    [TestMethod]
    public void WhenFirstSettingIndicesFailsThenEveryLaterCallIsRetried()
    {
        int callCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            setIndices: _ =>
            {
                callCount++;
                return callCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
            });
        IDirect3DIndexBuffer9* indexBuffer = (IDirect3DIndexBuffer9*) 1;

        int firstResult = device.SetIndices(indexBuffer);
        int secondResult = device.SetIndices(indexBuffer);
        int thirdResult = device.SetIndices(indexBuffer);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0, 3),
            (firstResult, secondResult, thirdResult, callCount));
    }

    [TestMethod]
    public void WhenTrackedDrawsSucceedThenPresentConsumesTopologyMetricsAndClearsThem()
    {
        List<Direct3D9FrameMetrics> consumedMetrics = [];
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, consumeFrameMetrics: consumedMetrics.Add);
        using Direct3D9SwapChain swapChain = new(device.ResourceManager, null, present: static () => 0);

        Assert.AreEqual(0, device.DrawIndexedTriangleList(0, 0, 7, 0, 3));
        Assert.AreEqual(0, device.DrawTriangleList(0, 2));
        Assert.AreEqual(0, device.DrawTriangleStrip(0, 4));
        _ = device.Present(swapChain, default);
        _ = device.Present(swapChain, default);

        CollectionAssert.AreEqual(
            new[]
            {
                new Direct3D9FrameMetrics(19, 9),
                new Direct3D9FrameMetrics(0, 0)
            },
            consumedMetrics);
    }

    [TestMethod]
    public void WhenTrackedDrawFailsThenPresentConsumesNoMetrics()
    {
        Direct3D9FrameMetrics consumedMetrics = default;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, consumeFrameMetrics: metrics => consumedMetrics = metrics);
        using Direct3D9SwapChain swapChain = new(device.ResourceManager, null, present: static () => 0);
        _drawResult = Direct3D9Factory.GenericFailureHResult;

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, device.DrawTriangleList(0, 2));
        _ = device.Present(swapChain, default);

        Assert.AreEqual(default, consumedMetrics);
    }

    [TestMethod]
    public void WhenLaterLargePrimitiveChunkFailsThenPresentConsumesOnlySuccessfulChunkMetrics()
    {
        int drawCallCount = 0;
        Direct3D9FrameMetrics consumedMetrics = default;
        Caps9 capabilities = new() { MaxPrimitiveCount = 2 };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            capabilities,
            drawPrimitiveUp: (_, _, _, _) => ++drawCallCount == 1 ? 0 : Direct3D9Factory.GenericFailureHResult,
            consumeFrameMetrics: metrics => consumedMetrics = metrics);
        using Direct3D9SwapChain swapChain = new(device.ResourceManager, null, present: static () => 0);
        byte* vertices = stackalloc byte[15];

        Assert.AreEqual(
            Direct3D9Factory.GenericFailureHResult,
            device.DrawLargePrimitiveUp(Primitivetype.Trianglelist, 5, vertices, 1));
        _ = device.Present(swapChain, default);

        Assert.AreEqual(new Direct3D9FrameMetrics(6, 2), consumedMetrics);
    }

    private static Direct3D9PrimitiveVertexBuffer CreatePrimitiveVertexBuffer(int vertexCount)
    {
        Direct3D9PrimitiveVertexBuffer vertexBuffer = new();
        object buffer = typeof(Direct3D9PrimitiveVertexBuffer)
            .GetField("_buffer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(vertexBuffer)!;
        Assert.AreEqual(0, ((Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzDiffuseUv2>) buffer).GetNewVertices(out _));
        buffer.GetType()
            .GetField("_vertexCount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(buffer, vertexCount);
        return vertexBuffer;
    }

    private static Direct3D9Device CreateDevice(
        IDirect3DDevice9* device,
        Caps9 capabilities = default,
        Direct3D9SetFlexibleVertexFormat? setFlexibleVertexFormat = null,
        Direct3D9DrawPrimitiveUp? drawPrimitiveUp = null,
        Action<Direct3D9FrameMetrics>? consumeFrameMetrics = null)
    {
        if (capabilities.MaxPrimitiveCount == 0)
        {
            capabilities.MaxPrimitiveCount = ushort.MaxValue;
        }

        return new Direct3D9Device(
            device,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            capabilities: capabilities,
            recordSuccessfulPresent: static () => 0,
            setFlexibleVertexFormat: setFlexibleVertexFormat ?? (flexibleVertexFormat =>
            {
                _flexibleVertexFormat = flexibleVertexFormat;
                return 0;
            }),
            drawPrimitiveUp: drawPrimitiveUp,
            consumeFrameMetrics: consumeFrameMetrics);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int DrawPrimitive(
        IDirect3DDevice9* self,
        Primitivetype primitiveType,
        uint startVertex,
        uint primitiveCount)
    {
        _drawCallCount++;
        _primitiveType = primitiveType;
        _startIndexOrVertex = startVertex;
        _primitiveCount = primitiveCount;
        return _drawResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int DrawPrimitiveUp(
        IDirect3DDevice9* self,
        Primitivetype primitiveType,
        uint primitiveCount,
        void* vertexStreamZeroData,
        uint vertexStreamZeroStride)
    {
        PrimitiveUpCalls.Add((primitiveType, primitiveCount, (nint) vertexStreamZeroData, vertexStreamZeroStride));
        if (primitiveType == Primitivetype.Trianglefan)
        {
            _primitiveVertices = new ReadOnlySpan<Direct3D9VertexXyzDiffuseUv2>(
                vertexStreamZeroData,
                checked((int) primitiveCount + 2)).ToArray();
        }

        return _drawResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetStreamSource(
        IDirect3DDevice9* self,
        uint streamNumber,
        IDirect3DVertexBuffer9* streamData,
        uint offsetInBytes,
        uint stride)
    {
        StreamSourceCalls.Add((streamNumber, (nint) streamData, offsetInBytes, stride));
        return _setStreamSourceResult;
    }

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
        *buffer = (IDirect3DVertexBuffer9*) _vertexBuffer;
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
        *buffer = (IDirect3DIndexBuffer9*) _vertexBuffer;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockBuffer(void* self, uint offset, uint size, void** data, uint flags)
    {
        if (_bufferLockResult < 0)
        {
            *data = null;
            return _bufferLockResult;
        }

        _lockedBufferData = (nint) NativeMemory.Alloc(size);
        _lockedBufferSize = size;
        *data = (void*) _lockedBufferData;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UnlockBuffer(void* self)
    {
        if (_lockedBufferData != 0)
        {
            _copiedVertexBytes = new ReadOnlySpan<byte>((void*) _lockedBufferData, checked((int) _lockedBufferSize)).ToArray();
            NativeMemory.Free((void*) _lockedBufferData);
            _lockedBufferData = 0;
            _lockedBufferSize = 0;
        }

        return _bufferUnlockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int DrawIndexedPrimitive(
        IDirect3DDevice9* self,
        Primitivetype primitiveType,
        int baseVertexIndex,
        uint minIndex,
        uint vertexCount,
        uint startIndex,
        uint primitiveCount)
    {
        _drawCallCount++;
        _primitiveType = primitiveType;
        _baseVertexIndex = baseVertexIndex;
        _minIndex = minIndex;
        _vertexCount = vertexCount;
        _startIndexOrVertex = startIndex;
        _primitiveCount = primitiveCount;
        return _drawResult;
    }

    private struct FakeBufferObject : IDisposable
    {
        private nint _memory;
        internal void* Buffer;

        public FakeBufferObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 14);
            void** memory = (void**) _memory;
            Buffer = memory;
            void** vtable = memory + 1;
            *(void***) Buffer = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[11] = (void*) (delegate* unmanaged[Stdcall]<void*, uint, uint, void**, uint, int>) &LockBuffer;
            vtable[12] = (void*) (delegate* unmanaged[Stdcall]<void*, int>) &UnlockBuffer;
        }

        public void Dispose()
        {
            if (_lockedBufferData != 0)
            {
                NativeMemory.Free((void*) _lockedBufferData);
                _lockedBufferData = 0;
                _lockedBufferSize = 0;
            }

            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Buffer = null;
        }
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 107);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[26] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, Pool, IDirect3DVertexBuffer9**, void**, int>) &CreateVertexBuffer;
            vtable[27] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, Pool, IDirect3DIndexBuffer9**, void**, int>) &CreateIndexBuffer;
            vtable[81] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, uint, uint, int>) &DrawPrimitive;
            vtable[82] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, int, uint, uint, uint, uint, int>) &DrawIndexedPrimitive;
            vtable[83] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, uint, void*, uint, int>) &DrawPrimitiveUp;
            vtable[100] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DVertexBuffer9*, uint, uint, int>) &SetStreamSource;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
