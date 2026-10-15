namespace WpfGfxShape.Core;

// Internal resource references do not invoke source COM; copy values before realization.
internal sealed class FrozenShaderInvocation : IDisposable
{
    private readonly GeneratedProtocolResource[] _leases;
    private bool _disposed;
    private readonly FrozenBrush?[] _samplerBrushes;
    internal FrozenVisualTree?[] SamplerVisualTrees { get; }
    internal FrozenCacheRequest?[] SamplerCacheRequests { get; }
    internal int CaptureResult { get; }
    internal FrozenBrush? SamplerBrush(int index) => _samplerBrushes[index];
    internal GeneratedShaderEffectData Data { get; }
    internal MilShaderRenderMode RenderMode { get; }
    internal bool CompileSoftwareShader { get; }
    internal byte[] Bytecode { get; }
    internal PixelShaderGeneration? Generation { get; }

    internal FrozenShaderInvocation(GeneratedShaderEffectData data,
        HashSet<GeneratedProtocolResource>? active = null)
    {
        Data = data with
        {
            FloatRegisters = (short[])data.FloatRegisters.Clone(),
            FloatValues = (float[])data.FloatValues.Clone(),
            IntRegisters = (short[])data.IntRegisters.Clone(),
            IntValues = (int[])data.IntValues.Clone(),
            BoolRegisters = (short[])data.BoolRegisters.Clone(),
            BoolValues = (int[])data.BoolValues.Clone(),
            SamplerInfo = (int[])data.SamplerInfo.Clone(),
            SamplerHandles = (uint[])data.SamplerHandles.Clone(),
            Samplers = data.Samplers.ToArray()
        };
        if (data.PixelShader is GeneratedPixelShaderResource shader)
        {
            Generation = shader.CaptureGeneration();
            RenderMode = Generation.RenderMode;
            CompileSoftwareShader = Generation.CompileSoftware;
            Bytecode = Generation.Bytecode.ToArray();
        }
        else Bytecode = [];
        _leases = Data.Samplers.Append(Data.PixelShader).OfType<GeneratedProtocolResource>().ToArray();
        foreach (var resource in _leases) resource.AddRef();
        _samplerBrushes = new FrozenBrush?[Data.Samplers.Count];
        SamplerVisualTrees = new FrozenVisualTree?[Data.Samplers.Count];
        SamplerCacheRequests = new FrozenCacheRequest?[Data.Samplers.Count];
        active ??= [];
        for (int i = 0; i < Data.Samplers.Count; i++)
        {
            if (Data.Samplers[i] is GeneratedImageBrushResource or GeneratedVisualBrushResource)
            {
                CaptureResult = FrozenBrush.CapturePure((GeneratedTileBrushResource)Data.Samplers[i]!, out _samplerBrushes[i], active);
                SamplerVisualTrees[i] = _samplerBrushes[i]?.Visual;
            }
            else if (Data.Samplers[i] is GeneratedBitmapCacheBrushResource cache)
                CaptureResult = FrozenCacheRequest.CapturePure(cache, active, out SamplerCacheRequests[i]);
            if (CaptureResult < 0) break;
        }
    }

    internal int GetSoftwareProgram(out SoftwarePixelShaderProgram? program)
    {
        program = null;
        return Generation is not null && Data.PixelShader is GeneratedPixelShaderResource shader
            ? Generation.GetSoftwareProgram(shader.BadShaderNotification, out program)
            : Direct3D9Factory.InvalidCallHResult;
    }

    // Releases the frame leases in reverse acquisition order.
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            for (int i = _samplerBrushes.Length - 1; i >= 0; i--)
            {
                SamplerCacheRequests[i]?.Dispose();
                _samplerBrushes[i]?.Dispose();
            }
        }
        finally
        {
            Generation?.Release();
            for (int i = _leases.Length - 1; i >= 0; i--) _leases[i].Release();
        }
    }
}
