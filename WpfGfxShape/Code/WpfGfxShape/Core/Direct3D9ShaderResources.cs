using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal enum Direct3D9ShaderKind
{
    Vertex,
    Pixel
}

internal sealed record Direct3D9ShaderDescriptor(
    uint ResourceId,
    Direct3D9ShaderKind Kind,
    uint MinimumShaderVersion,
    string Name,
    string? SourceResourceName = null,
    string? EntryPoint = null,
    string? Profile = null);

internal sealed class Direct3D9ShaderBytecode
{
    private readonly uint[] _instructions;

    private Direct3D9ShaderBytecode(Direct3D9ShaderDescriptor descriptor, uint[] instructions)
    {
        Descriptor = descriptor;
        _instructions = instructions;
    }

    internal Direct3D9ShaderDescriptor Descriptor { get; }

    internal ReadOnlySpan<uint> Instructions => _instructions;

    internal uint SizeInBytes => checked((uint) _instructions.Length * sizeof(uint));

    internal static int TryCreate(
        Direct3D9ShaderDescriptor descriptor,
        ReadOnlySpan<uint> instructions,
        out Direct3D9ShaderBytecode? bytecode)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        bytecode = null;
        if (instructions.Length < 2 || instructions[^1] != 0x0000FFFFu)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        uint expectedKind = descriptor.Kind == Direct3D9ShaderKind.Vertex ? 0xFFFE0000u : 0xFFFF0000u;
        if ((instructions[0] & 0xFFFF0000u) != expectedKind || instructions[0] < descriptor.MinimumShaderVersion)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        bytecode = new Direct3D9ShaderBytecode(descriptor, instructions.ToArray());
        return Direct3D9Factory.SuccessHResult;
    }
}

internal static class Direct3D9ShaderDescriptors
{
    internal static readonly Direct3D9ShaderDescriptor EffectVertex20 = new(
        900,
        Direct3D9ShaderKind.Vertex,
        0xFFFE0200u,
        "VS_ShaderEffects20",
        "WpfGfxShape.Resources.ShaderEffectsVS.fx",
        "VS",
        "vs_2_0");

    internal static readonly Direct3D9ShaderDescriptor EffectVertex30 = new(
        901,
        Direct3D9ShaderKind.Vertex,
        0xFFFE0300u,
        "VS_ShaderEffects30",
        "WpfGfxShape.Resources.ShaderEffectsVS.fx",
        "VS",
        "vs_3_0");

    internal static bool TryGetTextPixelShader(uint resourceId, out Direct3D9ShaderDescriptor? descriptor)
    {
        if (resourceId is < 100 or > 115)
        {
            descriptor = null;
            return false;
        }

        uint version = resourceId < 108 ? 0xFFFF0101u : 0xFFFF0200u;
        descriptor = new Direct3D9ShaderDescriptor(
            resourceId,
            Direct3D9ShaderKind.Pixel,
            version,
            $"TextPixelShader{resourceId}");
        return true;
    }
}

internal delegate int Direct3D9CompileShader(
    string source,
    string entryPoint,
    string profile,
    out uint[]? instructions);

internal sealed class Direct3D9ShaderBytecodeLoader
{
    private readonly Assembly _assembly;
    private readonly Direct3D9CompileShader _compileShader;

    internal Direct3D9ShaderBytecodeLoader(
        Assembly? assembly = null,
        Direct3D9CompileShader? compileShader = null)
    {
        _assembly = assembly ?? typeof(Direct3D9ShaderBytecodeLoader).Assembly;
        _compileShader = compileShader ?? Direct3D9ShaderCompiler.Compile;
    }

    internal int TryLoad(Direct3D9ShaderDescriptor descriptor, out Direct3D9ShaderBytecode? bytecode)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        bytecode = null;

        if (descriptor.SourceResourceName is null)
        {
            if (!Direct3D9TextPixelShaderResources.TryGetShaderBytecode(descriptor.ResourceId, out ReadOnlySpan<uint> instructions))
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            return Direct3D9ShaderBytecode.TryCreate(descriptor, instructions, out bytecode);
        }

        using Stream? stream = _assembly.GetManifestResourceStream(descriptor.SourceResourceName);
        if (stream is null || string.IsNullOrWhiteSpace(descriptor.EntryPoint) || string.IsNullOrWhiteSpace(descriptor.Profile))
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        string source = reader.ReadToEnd();
        int result = _compileShader(source, descriptor.EntryPoint, descriptor.Profile, out uint[]? compiledInstructions);
        if (result < 0)
        {
            return result;
        }

        return compiledInstructions is null
            ? Direct3D9Factory.GenericFailureHResult
            : Direct3D9ShaderBytecode.TryCreate(descriptor, compiledInstructions, out bytecode);
    }
}

[SupportedOSPlatform("windows")]
internal static unsafe class Direct3D9ShaderCompiler
{
    [DllImport("d3dcompiler_47.dll", EntryPoint = "D3DCompile", CallingConvention = CallingConvention.StdCall)]
    private static extern int D3DCompile(
        void* sourceData,
        nuint sourceDataSize,
        byte* sourceName,
        void* defines,
        void* include,
        byte* entryPoint,
        byte* target,
        uint flags1,
        uint flags2,
        void** code,
        void** errorMessages);

    internal static int Compile(string source, string entryPoint, string profile, out uint[]? instructions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryPoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(profile);
        instructions = null;

        byte[] sourceBytes = Encoding.UTF8.GetBytes(source);
        byte[] entryPointBytes = Encoding.ASCII.GetBytes(entryPoint + '\0');
        byte[] profileBytes = Encoding.ASCII.GetBytes(profile + '\0');
        void* code = null;
        void* errors = null;
        fixed (byte* sourcePointer = sourceBytes)
        fixed (byte* entryPointPointer = entryPointBytes)
        fixed (byte* profilePointer = profileBytes)
        {
            int result = D3DCompile(
                sourcePointer,
                (nuint) sourceBytes.Length,
                null,
                null,
                null,
                entryPointPointer,
                profilePointer,
                0,
                0,
                &code,
                &errors);
            ReleaseBlob(errors);
            if (result < 0)
            {
                ReleaseBlob(code);
                return result;
            }
        }

        try
        {
            void** vtable = *(void***) code;
            delegate* unmanaged[Stdcall]<void*, void*> getBufferPointer =
                (delegate* unmanaged[Stdcall]<void*, void*>) vtable[3];
            delegate* unmanaged[Stdcall]<void*, nuint> getBufferSize =
                (delegate* unmanaged[Stdcall]<void*, nuint>) vtable[4];
            nuint byteCount = getBufferSize(code);
            if (byteCount == 0 || byteCount % sizeof(uint) != 0 || byteCount > int.MaxValue)
            {
                return Direct3D9Factory.InvalidCallHResult;
            }

            instructions = new ReadOnlySpan<uint>(getBufferPointer(code), checked((int) (byteCount / sizeof(uint)))).ToArray();
            return Direct3D9Factory.SuccessHResult;
        }
        finally
        {
            ReleaseBlob(code);
        }
    }

    private static void ReleaseBlob(void* blob)
    {
        if (blob is null)
        {
            return;
        }

        void** vtable = *(void***) blob;
        delegate* unmanaged[Stdcall]<void*, uint> release = (delegate* unmanaged[Stdcall]<void*, uint>) vtable[2];
        _ = release(blob);
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal abstract unsafe class Direct3D9CachedShader : Direct3D9Resource
{
    protected Direct3D9CachedShader(
        Direct3D9ResourceManager manager,
        Direct3D9ShaderDescriptor descriptor,
        uint resourceSize)
        : base(manager, resourceSize: resourceSize)
    {
        Descriptor = descriptor;
    }

    internal Direct3D9ShaderDescriptor Descriptor { get; }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9CachedVertexShader : Direct3D9CachedShader
{
    private Direct3D9VertexShader? _shader;

    internal Direct3D9CachedVertexShader(
        Direct3D9ResourceManager manager,
        Direct3D9ShaderDescriptor descriptor,
        Direct3D9VertexShader shader,
        uint resourceSize)
        : base(manager, descriptor, resourceSize)
    {
        _shader = shader;
    }

    internal IDirect3DVertexShader9* Shader
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsReleased || _shader is null, this);
            return _shader.VertexShader;
        }
    }

    protected override void ReleaseD3DResources()
    {
        _shader?.Dispose();
        _shader = null;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9CachedPixelShader : Direct3D9CachedShader
{
    private Direct3D9PixelShader? _shader;

    internal Direct3D9CachedPixelShader(
        Direct3D9ResourceManager manager,
        Direct3D9ShaderDescriptor descriptor,
        Direct3D9PixelShader shader,
        uint resourceSize)
        : base(manager, descriptor, resourceSize)
    {
        _shader = shader;
    }

    internal IDirect3DPixelShader9* Shader
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsReleased || _shader is null, this);
            return _shader.PixelShader;
        }
    }

    protected override void ReleaseD3DResources()
    {
        _shader?.Dispose();
        _shader = null;
    }
}

internal delegate int Direct3D9CreateCachedVertexShader(
    Direct3D9ShaderBytecode bytecode,
    out Direct3D9CachedVertexShader? shader);

internal delegate int Direct3D9CreateCachedPixelShader(
    Direct3D9ShaderBytecode bytecode,
    out Direct3D9CachedPixelShader? shader);

[SupportedOSPlatform("windows5.1.2600")]
internal sealed class Direct3D9ShaderCache
{
    private readonly Direct3D9Device _device;
    private readonly Direct3D9ShaderBytecodeLoader _loader;
    private readonly Direct3D9CreateCachedVertexShader _createVertexShader;
    private readonly Direct3D9CreateCachedPixelShader _createPixelShader;
    private readonly Dictionary<Direct3D9ShaderDescriptor, Direct3D9CachedShader> _shaders = [];

    internal Direct3D9ShaderCache(
        Direct3D9Device device,
        Direct3D9ShaderBytecodeLoader? loader = null,
        Direct3D9CreateCachedVertexShader? createVertexShader = null,
        Direct3D9CreateCachedPixelShader? createPixelShader = null)
    {
        ArgumentNullException.ThrowIfNull(device);
        _device = device;
        _loader = loader ?? new Direct3D9ShaderBytecodeLoader();
        _createVertexShader = createVertexShader ?? CreateVertexShader;
        _createPixelShader = createPixelShader ?? CreatePixelShader;
    }

    internal int Count => _shaders.Count;

    internal int TryGetVertexShader(
        Direct3D9ShaderDescriptor descriptor,
        out Direct3D9CachedVertexShader? shader)
    {
        if (descriptor.Kind != Direct3D9ShaderKind.Vertex)
        {
            throw new ArgumentException("A vertex shader descriptor is required.", nameof(descriptor));
        }

        int result = TryGetShader(descriptor, out Direct3D9CachedShader? cachedShader);
        shader = cachedShader as Direct3D9CachedVertexShader;
        return result;
    }

    internal int TryGetPixelShader(
        Direct3D9ShaderDescriptor descriptor,
        out Direct3D9CachedPixelShader? shader)
    {
        if (descriptor.Kind != Direct3D9ShaderKind.Pixel)
        {
            throw new ArgumentException("A pixel shader descriptor is required.", nameof(descriptor));
        }

        int result = TryGetShader(descriptor, out Direct3D9CachedShader? cachedShader);
        shader = cachedShader as Direct3D9CachedPixelShader;
        return result;
    }

    internal void Invalidate()
    {
        Direct3D9CachedShader[] shaders = _shaders.Values.ToArray();
        _shaders.Clear();
        foreach (Direct3D9CachedShader shader in shaders)
        {
            shader.Dispose();
        }
    }

    private int TryGetShader(Direct3D9ShaderDescriptor descriptor, out Direct3D9CachedShader? shader)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (_shaders.TryGetValue(descriptor, out shader))
        {
            if (shader.IsValid)
            {
                return Direct3D9Factory.SuccessHResult;
            }

            _shaders.Remove(descriptor);
            shader = null;
        }

        uint supportedVersion = descriptor.Kind == Direct3D9ShaderKind.Vertex
            ? _device.Capabilities.VertexShaderVersion
            : _device.Capabilities.PixelShaderVersion;
        if (supportedVersion < descriptor.MinimumShaderVersion)
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        int result = _loader.TryLoad(descriptor, out Direct3D9ShaderBytecode? bytecode);
        if (result < 0)
        {
            return result;
        }

        if (descriptor.Kind == Direct3D9ShaderKind.Vertex)
        {
            result = _createVertexShader(bytecode!, out Direct3D9CachedVertexShader? vertexShader);
            shader = vertexShader;
        }
        else
        {
            result = _createPixelShader(bytecode!, out Direct3D9CachedPixelShader? pixelShader);
            shader = pixelShader;
        }
        if (result < 0)
        {
            shader?.Dispose();
            shader = null;
            return result;
        }
        if (shader is null)
        {
            throw new InvalidOperationException("Direct3D shader creation returned a null shader.");
        }

        _shaders.Add(descriptor, shader);
        return result;
    }

    private unsafe int CreateVertexShader(
        Direct3D9ShaderBytecode bytecode,
        out Direct3D9CachedVertexShader? shader)
    {
        shader = null;
        ReadOnlySpan<uint> instructions = bytecode.Instructions;
        fixed (uint* shaderFunction = instructions)
        {
            int result = _device.TryCreateVertexShader(shaderFunction, out Direct3D9VertexShader? nativeShader);
            if (result < 0)
            {
                return result;
            }

            try
            {
                shader = new Direct3D9CachedVertexShader(
                    _device.ResourceManager,
                    bytecode.Descriptor,
                    nativeShader!,
                    bytecode.SizeInBytes);
                nativeShader = null;
                return result;
            }
            finally
            {
                nativeShader?.Dispose();
            }
        }
    }

    private unsafe int CreatePixelShader(
        Direct3D9ShaderBytecode bytecode,
        out Direct3D9CachedPixelShader? shader)
    {
        shader = null;
        ReadOnlySpan<uint> instructions = bytecode.Instructions;
        fixed (uint* shaderFunction = instructions)
        {
            int result = _device.TryCreatePixelShader(shaderFunction, out Direct3D9PixelShader? nativeShader);
            if (result < 0)
            {
                return result;
            }

            try
            {
                shader = new Direct3D9CachedPixelShader(
                    _device.ResourceManager,
                    bytecode.Descriptor,
                    nativeShader!,
                    bytecode.SizeInBytes);
                nativeShader = null;
                return result;
            }
            finally
            {
                nativeShader?.Dispose();
            }
        }
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9ShaderProgram
{
    private readonly Direct3D9Device _device;
    private readonly Direct3D9CachedVertexShader _vertexShader;
    private readonly Direct3D9CachedPixelShader _pixelShader;

    internal Direct3D9ShaderProgram(
        Direct3D9Device device,
        Direct3D9CachedVertexShader vertexShader,
        Direct3D9CachedPixelShader pixelShader)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(vertexShader);
        ArgumentNullException.ThrowIfNull(pixelShader);
        if (!ReferenceEquals(vertexShader.Manager.Device, device) || !ReferenceEquals(pixelShader.Manager.Device, device))
        {
            throw new ArgumentException("Pipeline shaders must belong to the consuming Direct3D device.");
        }

        _device = device;
        _vertexShader = vertexShader;
        _pixelShader = pixelShader;
    }

    internal int SendToDevice()
    {
        int result = _device.SetVertexShader(_vertexShader.Shader);
        return result < 0 ? result : _device.SetPixelShader(_pixelShader.Shader);
    }
}
