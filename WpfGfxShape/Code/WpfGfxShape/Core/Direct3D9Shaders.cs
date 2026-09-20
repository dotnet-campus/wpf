using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9VertexShader : IDisposable
{
    private IDirect3DVertexShader9* _vertexShader;

    internal Direct3D9VertexShader(IDirect3DVertexShader9* vertexShader)
    {
        _vertexShader = vertexShader;
    }

    internal IDirect3DVertexShader9* VertexShader
    {
        get
        {
            ObjectDisposedException.ThrowIf(_vertexShader is null, this);
            return _vertexShader;
        }
    }

    public void Dispose()
    {
        Direct3D9Factory.Release(_vertexShader);
        _vertexShader = null;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9PixelShader : IDisposable
{
    private IDirect3DPixelShader9* _pixelShader;

    internal Direct3D9PixelShader(IDirect3DPixelShader9* pixelShader)
    {
        _pixelShader = pixelShader;
    }

    internal IDirect3DPixelShader9* PixelShader
    {
        get
        {
            ObjectDisposedException.ThrowIf(_pixelShader is null, this);
            return _pixelShader;
        }
    }

    public void Dispose()
    {
        Direct3D9Factory.Release(_pixelShader);
        _pixelShader = null;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9PixelShaderEffect : Direct3D9Resource
{
    private readonly Direct3D9Device _deviceNoReference;
    private Direct3D9PixelShader? _pixelShader;

    private Direct3D9PixelShaderEffect(
        Direct3D9ResourceManager resourceManager,
        Direct3D9Device deviceNoReference,
        Direct3D9PixelShader pixelShader,
        uint resourceSize)
        : base(resourceManager, resourceSize: resourceSize)
    {
        _deviceNoReference = deviceNoReference;
        _pixelShader = pixelShader;
    }

    internal Direct3D9Device DeviceNoReference
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsReleased, this);
            return _deviceNoReference;
        }
    }

    internal IDirect3DPixelShader9* PixelShader
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsReleased || _pixelShader is null, this);
            return _pixelShader.PixelShader;
        }
    }

    internal static int TryCreate(
        Direct3D9Device device,
        uint* shaderFunction,
        uint resourceSize,
        out Direct3D9PixelShaderEffect? effect)
    {
        ArgumentNullException.ThrowIfNull(device);

        effect = null;
        int result = device.TryCreatePixelShader(shaderFunction, out Direct3D9PixelShader? pixelShader);
        if (result < 0)
        {
            return result;
        }

        bool ownershipTransferred = false;
        try
        {
            effect = new Direct3D9PixelShaderEffect(device.ResourceManager, device, pixelShader!, resourceSize);
            ownershipTransferred = true;
            return result;
        }
        finally
        {
            if (!ownershipTransferred)
            {
                pixelShader!.Dispose();
            }
        }
    }

    protected override void ReleaseD3DResources()
    {
        _pixelShader?.Dispose();
        _pixelShader = null;
    }
}
