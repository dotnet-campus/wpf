using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9ShaderResourceCacheTests
{
    [TestMethod]
    public void WhenNativeTextPixelShaderIsLoadedThenOwnedBytecodeIsValidated()
    {
        Direct3D9ShaderBytecodeLoader loader = new();
        Assert.IsTrue(Direct3D9ShaderDescriptors.TryGetTextPixelShader(108, out Direct3D9ShaderDescriptor? descriptor));

        int result = loader.TryLoad(descriptor, out Direct3D9ShaderBytecode? bytecode);

        Assert.AreEqual((0, 0xFFFF0200u, 0x0000FFFFu), (result, bytecode!.Instructions[0], bytecode.Instructions[^1]));
    }

    [TestMethod]
    public void WhenEffectVertexShaderSourceIsLoadedThenRealBytecodeIsCompiled()
    {
        Direct3D9ShaderBytecodeLoader loader = new();

        int result = loader.TryLoad(Direct3D9ShaderDescriptors.EffectVertex20, out Direct3D9ShaderBytecode? bytecode);

        Assert.AreEqual((0, 0xFFFE0200u, 0x0000FFFFu), (result, bytecode!.Instructions[0], bytecode.Instructions[^1]));
    }

    [TestMethod]
    public void WhenCompiledBytecodeIsTruncatedThenItIsRejected()
    {
        Direct3D9ShaderBytecodeLoader loader = new(
            compileShader: (string _, string _, string _, out uint[]? instructions) =>
            {
                instructions = [0xFFFE0200u, 0x00000001u];
                return 0;
            });

        int result = loader.TryLoad(Direct3D9ShaderDescriptors.EffectVertex20, out Direct3D9ShaderBytecode? bytecode);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, null), (result, bytecode));
    }

    [TestMethod]
    public void WhenShaderIsRequestedTwiceThenDeviceCacheCreatesItOnce()
    {
        using Direct3D9Device device = CreateDevice(vertexShaderVersion: 0xFFFE0200u);
        int createCount = 0;
        Direct3D9ShaderCache cache = CreateVertexCache(
            device,
            (Direct3D9ShaderBytecode _, out Direct3D9CachedVertexShader? shader) =>
            {
                createCount++;
                shader = new Direct3D9CachedVertexShader(
                    device.ResourceManager,
                    Direct3D9ShaderDescriptors.EffectVertex20,
                    new Direct3D9VertexShader(null),
                    8);
                return 0;
            });

        int firstResult = cache.TryGetVertexShader(Direct3D9ShaderDescriptors.EffectVertex20, out Direct3D9CachedVertexShader? first);
        int secondResult = cache.TryGetVertexShader(Direct3D9ShaderDescriptors.EffectVertex20, out Direct3D9CachedVertexShader? second);

        Assert.AreEqual((0, 0, 1, true, 1), (firstResult, secondResult, createCount, ReferenceEquals(first, second), cache.Count));
    }

    [TestMethod]
    public void WhenCapabilityIsInsufficientThenCreationIsNotAttempted()
    {
        using Direct3D9Device device = CreateDevice(vertexShaderVersion: 0xFFFE0101u);
        int createCount = 0;
        Direct3D9ShaderCache cache = CreateVertexCache(
            device,
            (Direct3D9ShaderBytecode _, out Direct3D9CachedVertexShader? shader) =>
            {
                createCount++;
                shader = null;
                return 0;
            });

        int result = cache.TryGetVertexShader(Direct3D9ShaderDescriptors.EffectVertex20, out Direct3D9CachedVertexShader? shader);

        Assert.AreEqual((Direct3D9Factory.UnsupportedOperationHResult, 0, null), (result, createCount, shader));
    }

    [TestMethod]
    public void WhenShaderCreationFailsThenFailureIsNotCached()
    {
        using Direct3D9Device device = CreateDevice(vertexShaderVersion: 0xFFFE0200u);
        int createCount = 0;
        Direct3D9ShaderCache cache = CreateVertexCache(
            device,
            (Direct3D9ShaderBytecode _, out Direct3D9CachedVertexShader? shader) =>
            {
                createCount++;
                shader = null;
                return Direct3D9Factory.InvalidCallHResult;
            });

        int result = cache.TryGetVertexShader(Direct3D9ShaderDescriptors.EffectVertex20, out Direct3D9CachedVertexShader? shader);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 1, 0, null), (result, createCount, cache.Count, shader));
    }

    [TestMethod]
    public void WhenCacheIsInvalidatedThenShadersAreReleasedAndCanBeRecreated()
    {
        using Direct3D9Device device = CreateDevice(vertexShaderVersion: 0xFFFE0200u);
        int createCount = 0;
        Direct3D9ShaderCache cache = CreateVertexCache(
            device,
            (Direct3D9ShaderBytecode _, out Direct3D9CachedVertexShader? shader) =>
            {
                createCount++;
                shader = new Direct3D9CachedVertexShader(
                    device.ResourceManager,
                    Direct3D9ShaderDescriptors.EffectVertex20,
                    new Direct3D9VertexShader(null),
                    8);
                return 0;
            });
        Assert.AreEqual(0, cache.TryGetVertexShader(Direct3D9ShaderDescriptors.EffectVertex20, out Direct3D9CachedVertexShader? first));

        cache.Invalidate();
        int result = cache.TryGetVertexShader(Direct3D9ShaderDescriptors.EffectVertex20, out Direct3D9CachedVertexShader? second);

        Assert.AreEqual((0, 2, true, 1), (result, createCount, first!.IsReleased && !ReferenceEquals(first, second), cache.Count));
    }

    [TestMethod]
    public void WhenSourceResourceIsMissingThenLoadFailsWithoutBytecode()
    {
        Direct3D9ShaderDescriptor descriptor = Direct3D9ShaderDescriptors.EffectVertex20 with
        {
            ResourceId = 999,
            SourceResourceName = "WpfGfxShape.Resources.MissingShader.fx"
        };
        Direct3D9ShaderBytecodeLoader loader = new();

        int result = loader.TryLoad(descriptor, out Direct3D9ShaderBytecode? bytecode);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, null), (result, bytecode));
    }

    [TestMethod]
    public void WhenDeviceIsMarkedUnusableThenCachedShaderIsReleased()
    {
        using Direct3D9Device device = CreateDevice(vertexShaderVersion: 0xFFFE0200u);
        Direct3D9ShaderCache cache = CreateVertexCache(
            device,
            (Direct3D9ShaderBytecode _, out Direct3D9CachedVertexShader? shader) =>
            {
                shader = new Direct3D9CachedVertexShader(
                    device.ResourceManager,
                    Direct3D9ShaderDescriptors.EffectVertex20,
                    new Direct3D9VertexShader(null),
                    8);
                return 0;
            });
        Assert.AreEqual(0, cache.TryGetVertexShader(Direct3D9ShaderDescriptors.EffectVertex20, out Direct3D9CachedVertexShader? shader));

        device.MarkUnusable();

        Assert.IsTrue(shader!.IsReleased);
    }

    [TestMethod]
    public void WhenPipelineShadersBelongToAnotherDeviceThenProgramIsRejected()
    {
        using Direct3D9Device firstDevice = CreateDevice(0xFFFE0200u, 0xFFFF0200u);
        using Direct3D9Device secondDevice = CreateDevice(0xFFFE0200u, 0xFFFF0200u);
        Direct3D9CachedVertexShader vertexShader = new(
            firstDevice.ResourceManager,
            Direct3D9ShaderDescriptors.EffectVertex20,
            new Direct3D9VertexShader(null),
            8);
        Assert.IsTrue(Direct3D9ShaderDescriptors.TryGetTextPixelShader(108, out Direct3D9ShaderDescriptor? pixelDescriptor));
        Direct3D9CachedPixelShader pixelShader = new(
            firstDevice.ResourceManager,
            pixelDescriptor,
            new Direct3D9PixelShader(null),
            8);

        Assert.ThrowsExactly<ArgumentException>(() => new Direct3D9ShaderProgram(secondDevice, vertexShader, pixelShader));
    }

    private static Direct3D9ShaderCache CreateVertexCache(
        Direct3D9Device device,
        Direct3D9CreateCachedVertexShader createVertexShader)
    {
        Direct3D9ShaderBytecodeLoader loader = new(
            compileShader: (string _, string _, string _, out uint[]? instructions) =>
            {
                instructions = [0xFFFE0200u, 0x0000FFFFu];
                return 0;
            });
        return new Direct3D9ShaderCache(device, loader, createVertexShader);
    }

    private static unsafe Direct3D9Device CreateDevice(
        uint vertexShaderVersion,
        uint pixelShaderVersion = 0)
    {
        Caps9 capabilities = new()
        {
            DeviceType = Devtype.Hal,
            VertexShaderVersion = vertexShaderVersion,
            PixelShaderVersion = pixelShaderVersion
        };
        return new Direct3D9Device(null, null, 0, Devtype.Hal, 0, default, capabilities: capabilities);
    }
}
