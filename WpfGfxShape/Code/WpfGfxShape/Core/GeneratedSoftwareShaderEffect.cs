using System.Numerics;
using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

internal sealed unsafe class GeneratedSoftwareShaderEffect : IDisposable
{
    private readonly GeneratedVisualRenderer.VisualSnapshot _input;
    private readonly SoftwarePixelShaderProgram _program;
    private readonly Vector4[] _constants;
    private readonly int[] _samplerInfo;
    private readonly bool[] _implicit;
    private readonly SoftwareBrushRealization?[] _brushes;
    private bool _disposed;
    internal GeneratedShaderInputScope Scope { get; }

    private GeneratedSoftwareShaderEffect(GeneratedVisualRenderer.VisualSnapshot input,
        SoftwarePixelShaderProgram program, Vector4[] constants, int[] samplerInfo, bool[] implicitInputs,
        GeneratedShaderInputScope scope, SoftwareBrushRealization?[] brushes)
    { _input = input; _program = program; _constants = constants; _samplerInfo = samplerInfo; _implicit = implicitInputs; Scope = scope; _brushes = brushes; }

    internal static int Capture(FrozenShaderInvocation invocation, GeneratedShaderInputScope scope,
        GeneratedVisualRenderer.VisualSnapshot input, MilRectD consumerBounds, out GeneratedSoftwareShaderEffect? effect)
    {
        effect = null;
        if (invocation.CaptureResult < 0) return invocation.CaptureResult;
        var data = invocation.Data;
        if (data.PixelShader is not GeneratedPixelShaderResource) return Direct3D9Factory.InvalidCallHResult;
        if (invocation.RenderMode == MilShaderRenderMode.HardwareOnly) return Direct3D9Factory.NotImplementedHResult;
        if (data.FloatRegisters.Any(static index => index >= 32)) return Direct3D9Factory.InvalidArgumentHResult;
        if (data.Samplers.Any(static sampler => sampler is not (null or GeneratedImplicitInputBrushResource
            or GeneratedImageBrushResource or GeneratedVisualBrushResource or GeneratedBitmapCacheBrushResource)))
            return Direct3D9Factory.NotImplementedHResult;
        if (data.SamplerInfo.Where((_, index) => index % 2 == 0).Any(static index => index >= 16))
            return Direct3D9Factory.InvalidArgumentHResult;
        int hr = invocation.GetSoftwareProgram(out var program);
        if (hr < 0 || program is null) return hr;
        Vector4[] constants = new Vector4[32];
        for (int i = 0; i < data.FloatRegisters.Length; i++)
            constants[data.FloatRegisters[i]] = new(data.FloatValues[i * 4], data.FloatValues[i * 4 + 1],
                data.FloatValues[i * 4 + 2], data.FloatValues[i * 4 + 3]);
        if (data.Command.DdxRegister is >= 0 and < 32) constants[data.Command.DdxRegister] = scope.UvDerivatives;
        int[] samplerInfo = (int[])data.SamplerInfo.Clone();
        var resources = data.Samplers.ToArray();
        bool[] implicitInputs = resources.Select(static sampler => sampler is GeneratedImplicitInputBrushResource).ToArray();
        var brushes = new SoftwareBrushRealization?[resources.Length];
        try
        {
            var clip = new Direct3D9SurfaceRect(0, 0, checked((int)scope.Width), checked((int)scope.Height));
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i] is null || implicitInputs[i]) continue;
                hr = invocation.SamplerBrush(i) is { } frozen
                    ? SoftwareBrushRealization.CaptureFrozen(frozen, scope.InputTransform, 0,
                        consumerBounds, out brushes[i], clip)
                    : invocation.SamplerCacheRequests[i] is { } request
                        ? SoftwareBrushRealization.CaptureFrozen(request, scope.InputTransform, 0,
                            consumerBounds, out brushes[i], clip)
                        : Direct3D9Factory.NotImplementedHResult;
                if (hr < 0) return hr;
            }
            effect = new(input, program, constants, samplerInfo, implicitInputs, scope, brushes);
            return 0;
        }
        finally
        {
            if (effect is null)
                for (int i = brushes.Length - 1; i >= 0; i--) brushes[i]?.Dispose();
        }
    }

    internal int Draw(SoftwareImageRenderSession session, SoftwareImageDrawingContext context)
    {
        if (_disposed) return Direct3D9Factory.WgxInvalidCallHResult;
        nint input = 0, output = 0;
        try
        {
            bool floating = session.Floating;
            int hr = _input.Realize(out input, floating: floating);
            if (hr < 0) return hr;
            int count = checked((int)(Scope.Width * Scope.Height));
            uint[] pixels = floating ? [] : new uint[count];
            Vector4[] values = floating ? new Vector4[count] : [];
            fixed (uint* packed = pixels)
            fixed (Vector4* linear = values)
                hr = ((delegate* unmanaged[Stdcall]<nint, Direct3D9BitmapSourceRectangle*, uint, uint, byte*, int>)(*(void***)input)[7])
                    (input, null, checked(Scope.Width * (floating ? 16u : 4u)), checked((uint)count * (floating ? 16u : 4u)), floating ? (byte*)linear : (byte*)packed);
            if (hr < 0) return hr;
            SoftwareShaderSampler[] samplers = new SoftwareShaderSampler[16];
            Array.Fill(samplers, new([0xffffffff], 1, 1, false));
            for (int i = 0; i < _implicit.Length; i++)
            {
                bool bilinear = _samplerInfo[i * 2 + 1] == 2;
                if (_implicit[i])
                    samplers[_samplerInfo[i * 2]] = new(pixels, (int)Scope.Width, (int)Scope.Height, bilinear) { FloatPixels = floating ? values : null };
                else if (_brushes[i] is { } brush)
                {
                    hr = GeneratedSoftwareShaderSampler.Realize(brush, Scope.Width, Scope.Height,
                        bilinear, out var sampler, floating);
                    if (hr < 0) return hr;
                    samplers[_samplerInfo[i * 2]] = sampler;
                }
                else samplers[_samplerInfo[i * 2]] = new([0], 1, 1, bilinear);
            }
            hr = SoftwareBitmap.Create(Scope.Width, Scope.Height, 96, 96,
                new Guid(floating ? "6fddc324-4e03-4bfe-b185-3d77768dc91a" : "6fddc324-4e03-4bfe-b185-3d77768dc910"), out output);
            if (hr < 0) return hr;
            var bitmap = (SoftwareBitmap.Bitmap*)output;
            for (uint y = 0; y < Scope.Height; y++)
                for (uint x = 0; x < Scope.Width; x += 4)
                    for (uint lane = 0; lane < Math.Min(4u, Scope.Width - x); lane++)
                        if (floating)
                            ((Vector4*)(bitmap->Pixels + y * bitmap->Stride))[x + lane] = _program.ExecuteFloat(
                                (float)(x + lane) / Scope.Width, (float)y / Scope.Height, _constants, samplers);
                        else
                            ((uint*)(bitmap->Pixels + y * bitmap->Stride))[x + lane] = _program.Execute(
                                (float)(x + lane) / Scope.Width, (float)y / Scope.Height, _constants, samplers);
            return session.Draw(output, new(0, 0, Scope.Width, Scope.Height), false,
                context with { WorldToDevice = Scope.OutputTransform });
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000e); }
        catch (OverflowException) { return Direct3D9Factory.InvalidArgumentHResult; }
        finally { Direct3D9Factory.Release(output); Direct3D9Factory.Release(input); }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _input.Dispose(); }
        finally { foreach (var brush in _brushes) brush?.Dispose(); }
    }
}
