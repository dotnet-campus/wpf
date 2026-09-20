using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9ShaderStateTests
{
    [TestMethod]
    public unsafe void WhenVertexShaderIsSetTwiceThenNativeCallIsSkippedForMatchingPointer()
    {
        int callCount = 0;
        IDirect3DVertexShader9* shader = (IDirect3DVertexShader9*) 1;
        using Direct3D9Device device = CreateDevice(
            setVertexShader: _ =>
            {
                callCount++;
                return 0;
            });

        int firstResult = device.SetVertexShader(shader);
        int secondResult = device.SetVertexShader(shader);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenMatchingVertexShaderIsForcedTwiceThenBothNativeCallsRun()
    {
        int callCount = 0;
        IDirect3DVertexShader9* shader = (IDirect3DVertexShader9*) 1;
        using Direct3D9Device device = CreateDevice(
            setVertexShader: _ =>
            {
                callCount++;
                return 1;
            });

        int firstResult = device.ForceSetVertexShader(shader);
        int secondResult = device.ForceSetVertexShader(shader);

        Assert.AreEqual((1, 1, 2), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenForcedVertexShaderFailsAfterKnownPointerThenMatchingRegularCallIsRetried()
    {
        int callCount = 0;
        IDirect3DVertexShader9* shader = (IDirect3DVertexShader9*) 1;
        using Direct3D9Device device = CreateDevice(
            setVertexShader: _ =>
            {
                callCount++;
                return callCount == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        int initialResult = device.SetVertexShader(shader);
        int forcedResult = device.ForceSetVertexShader(shader);
        int retryResult = device.SetVertexShader(shader);

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, 0, 3),
            (initialResult, forcedResult, retryResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenPixelShaderIsSetTwiceThenNativeCallIsSkippedForMatchingPointer()
    {
        int callCount = 0;
        IDirect3DPixelShader9* shader = (IDirect3DPixelShader9*) 1;
        using Direct3D9Device device = CreateDevice(
            setPixelShader: _ =>
            {
                callCount++;
                return 0;
            });

        int firstResult = device.SetPixelShader(shader);
        int secondResult = device.SetPixelShader(shader);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenMatchingPixelShaderIsForcedTwiceThenBothNativeCallsRun()
    {
        int callCount = 0;
        IDirect3DPixelShader9* shader = (IDirect3DPixelShader9*) 1;
        using Direct3D9Device device = CreateDevice(
            setPixelShader: _ =>
            {
                callCount++;
                return 1;
            });

        int firstResult = device.ForceSetPixelShader(shader);
        int secondResult = device.ForceSetPixelShader(shader);

        Assert.AreEqual((1, 1, 2), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenForcedPixelShaderFailsAfterKnownPointerThenMatchingRegularCallIsRetried()
    {
        int callCount = 0;
        IDirect3DPixelShader9* shader = (IDirect3DPixelShader9*) 1;
        using Direct3D9Device device = CreateDevice(
            setPixelShader: _ =>
            {
                callCount++;
                return callCount == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        int initialResult = device.SetPixelShader(shader);
        int forcedResult = device.ForceSetPixelShader(shader);
        int retryResult = device.SetPixelShader(shader);

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, 0, 3),
            (initialResult, forcedResult, retryResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenInitialNullShadersAreSetThenBothNativeCallsAreMade()
    {
        int vertexCallCount = 0;
        int pixelCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            setVertexShader: _ =>
            {
                vertexCallCount++;
                return 0;
            },
            setPixelShader: _ =>
            {
                pixelCallCount++;
                return 0;
            });

        int vertexResult = device.SetVertexShader(null);
        int pixelResult = device.SetPixelShader(null);

        Assert.AreEqual((0, 0, 1, 1), (vertexResult, pixelResult, vertexCallCount, pixelCallCount));
    }

    [TestMethod]
    public unsafe void WhenVertexShaderSetFailsThenMatchingPointerIsRetried()
    {
        int callCount = 0;
        IDirect3DVertexShader9* shader = (IDirect3DVertexShader9*) 1;
        using Direct3D9Device device = CreateDevice(
            setVertexShader: _ =>
            {
                callCount++;
                return callCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        int failedResult = device.SetVertexShader(shader);
        int retryResult = device.SetVertexShader(shader);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 2), (failedResult, retryResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenPixelShaderSetFailsThenMatchingPointerIsRetried()
    {
        int callCount = 0;
        IDirect3DPixelShader9* shader = (IDirect3DPixelShader9*) 1;
        using Direct3D9Device device = CreateDevice(
            setPixelShader: _ =>
            {
                callCount++;
                return callCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        int failedResult = device.SetPixelShader(shader);
        int retryResult = device.SetPixelShader(shader);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 2), (failedResult, retryResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenVertexShaderChangeFailsThenPreviouslyKnownPointerIsRetried()
    {
        List<nint> shaders = [];
        using Direct3D9Device device = CreateDevice(
            setVertexShader: shader =>
            {
                shaders.Add((nint) shader);
                return shaders.Count == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
            });
        IDirect3DVertexShader9* originalShader = (IDirect3DVertexShader9*) 1;

        int initialResult = device.SetVertexShader(originalShader);
        int failedResult = device.SetVertexShader((IDirect3DVertexShader9*) 2);
        int restoreResult = device.SetVertexShader(originalShader);

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, 0, "1,2,1"),
            (initialResult, failedResult, restoreResult, string.Join(',', shaders)));
    }

    [TestMethod]
    public unsafe void WhenPixelShaderChangeFailsThenPreviouslyKnownPointerIsRetried()
    {
        List<nint> shaders = [];
        using Direct3D9Device device = CreateDevice(
            setPixelShader: shader =>
            {
                shaders.Add((nint) shader);
                return shaders.Count == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
            });
        IDirect3DPixelShader9* originalShader = (IDirect3DPixelShader9*) 1;

        int initialResult = device.SetPixelShader(originalShader);
        int failedResult = device.SetPixelShader((IDirect3DPixelShader9*) 2);
        int restoreResult = device.SetPixelShader(originalShader);

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, 0, "1,2,1"),
            (initialResult, failedResult, restoreResult, string.Join(',', shaders)));
    }

    [TestMethod]
    public unsafe void WhenDeviceIsDisposedThenSettingShadersThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateDevice(setVertexShader: _ => 0, setPixelShader: _ => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetVertexShader(null));
        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetPixelShader(null));
    }

    private static unsafe Direct3D9Device CreateDevice(
        Direct3D9SetVertexShader? setVertexShader = null,
        Direct3D9SetPixelShader? setPixelShader = null)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            setVertexShader: setVertexShader,
            setPixelShader: setPixelShader);
    }
}
