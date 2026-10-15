using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal static unsafe class GeneratedHardwareShaderEffect
{
    internal static int Render(FrozenShaderInvocation invocation, Direct3D9Device device,
        Direct3D9SurfaceRenderTarget target, Direct3D9SurfaceRect? clip, GeneratedShaderInputScope scope,
        MilRectD consumerBounds, GeneratedVisualRenderer.VisualSnapshot inputSnapshot)
    {
        if (invocation.Data.PixelShader is not GeneratedPixelShaderResource)
            return Direct3D9Factory.InvalidCallHResult;
        return RenderFrozen(invocation, device, target, clip, scope, consumerBounds, inputSnapshot);
    }

    private static int RenderFrozen(FrozenShaderInvocation invocation,
        Direct3D9Device device, Direct3D9SurfaceRenderTarget target, Direct3D9SurfaceRect? clip,
        GeneratedShaderInputScope scope, MilRectD consumerBounds, GeneratedVisualRenderer.VisualSnapshot inputSnapshot)
    {
        if (invocation.CaptureResult < 0) return invocation.CaptureResult;
        var data = invocation.Data;
        if (invocation.RenderMode == MilShaderRenderMode.SoftwareOnly)
            return Direct3D9Factory.NotImplementedHResult;
        for (int i = 0; i < data.Samplers.Count; i++)
            if (data.Samplers[i] is not (null or GeneratedImplicitInputBrushResource or GeneratedImageBrushResource
                or GeneratedVisualBrushResource or GeneratedBitmapCacheBrushResource)
                || data.SamplerInfo[i * 2] >= 16)
                return Direct3D9Factory.NotImplementedHResult;
        if (data.FloatRegisters.Any(static r => r >= 224)
            || data.IntRegisters.Any(static r => r >= 16)
            || data.BoolRegisters.Any(static r => r >= 16))
            return Direct3D9Factory.InvalidCallHResult;
        if (invocation.Generation is null || data.PixelShader is not GeneratedPixelShaderResource shader)
            return Direct3D9Factory.InvalidCallHResult;
        using Direct3D9DeviceEntryGuard entry = new(device);
        using Direct3D9UseContextGuard use = new(device);
        int hr = invocation.Generation.GetHardwareShader(device, shader.BadShaderNotification, out var hardwareShader);
        if (hr < 0) return hr;
        // The invocation owns the generation; the generation owns the cached device shader.
        {
            uint width = scope.Width, height = scope.Height;
            MilPixelFormat inputFormat = target.GetDirect3DTextureFormat() switch
            {
                Format.A32B32G32R32f => MilPixelFormat.Prgba128BppFloat,
                Format.A8R8G8B8 or Format.X8R8G8B8 => MilPixelFormat.Pbgra32Bpp,
                _ => MilPixelFormat.Undefined
            };
            hr = Direct3D9TextureRenderTarget.TryCreate(width, height, device, null, out var input, inputFormat);
            if (hr < 0) return hr;
            using (input)
            {
                if (input is null || hardwareShader is null) return Direct3D9Factory.InvalidCallHResult;
                var secondaryInputs = new Direct3D9TextureRenderTarget?[data.Samplers.Count];
                var captures = new SoftwareBrushRealization?[data.Samplers.Count];
                try
                {
                    for (int i = 0; i < data.Samplers.Count; i++)
                    {
                        if (data.Samplers[i] is null or GeneratedImplicitInputBrushResource) continue;
                        var samplerClip = new Direct3D9SurfaceRect(0, 0, checked((int)width), checked((int)height));
                        hr = invocation.SamplerBrush(i) is { } frozen
                            ? SoftwareBrushRealization.CaptureFrozen(frozen, scope.InputTransform, 0,
                                consumerBounds, out captures[i], samplerClip)
                            : invocation.SamplerCacheRequests[i] is { } request
                                ? SoftwareBrushRealization.CaptureFrozen(request, scope.InputTransform, 0,
                                    consumerBounds, out captures[i], samplerClip,
                                    inputFormat == MilPixelFormat.Prgba128BppFloat, 96, 96)
                                : Direct3D9Factory.NotImplementedHResult;
                        if (hr < 0) return hr;
                    }
                    hr = input.Clear(new MilColorF(0, 0, 0, 0), null);
                    if (hr >= 0) hr = GeneratedHardwareShaderSampler.RenderCapturedVisual(inputSnapshot,
                        device, input.SurfaceRenderTarget, width, height);
                    if (hr < 0) return hr;
                    for (int i = 0; i < data.Samplers.Count; i++)
                    {
                        if (data.Samplers[i] is null or GeneratedImplicitInputBrushResource) continue;
                        hr = Direct3D9TextureRenderTarget.TryCreate(width, height, device, null,
                            out secondaryInputs[i], inputFormat);
                        if (hr < 0) return hr;
                        if (secondaryInputs[i] is not { } secondary) return Direct3D9Factory.InvalidCallHResult;
                        hr = secondary.Clear(new MilColorF(0, 0, 0, 0), null);
                        if (hr >= 0) hr = GeneratedHardwareShaderSampler.RenderCapturedBrush(captures[i], device,
                            secondary.SurfaceRenderTarget, width, height);
                        if (hr < 0) return hr;
                    }
                uint boundSamplers = 0;
                bool shaderBound = false, sceneStarted = false;
                try
                {
                do
                {
                var state = new Direct3D9ContextState(Matrix4x4.Identity, Matrix4x4.Identity,
                    Matrix4x4.Identity, Matrix4x4.Identity, Cull.None, Cmpfunc.Always, false, AliasedClip: clip);
                hr = target.EnsureState(state);
                if (hr == Direct3D9Factory.ClippedToEmptyHResult) { hr = 0; break; }
                if (hr < 0) break;
                hr = device.SetPixelShader(hardwareShader.PixelShader);
                if (hr < 0) break;
                shaderBound = true;
                    for (int i = 0; i < data.Samplers.Count && hr >= 0; i++)
                    {
                        uint register = (uint)data.SamplerInfo[i * 2];
                        hr = device.SetTexture(register,
                            data.Samplers[i] is null ? null
                                : data.Samplers[i] is GeneratedImplicitInputBrushResource
                                    ? input.GetTextureNoRef() : secondaryInputs[i]!.GetTextureNoRef());
                        if (hr >= 0) boundSamplers |= 1u << (int)register;
                        if (hr >= 0) hr = device.SetTextureStageState(register, Texturestagestatetype.Texcoordindex, 0);
                        if (hr >= 0) hr = device.SetTextureStageState(register, Texturestagestatetype.Texturetransformflags, 0);
                        uint filter = data.SamplerInfo[i * 2 + 1] == 1 ? 1u : 2u;
                        if (hr >= 0) hr = device.SetSamplerState(register, Samplerstatetype.Minfilter, filter);
                        if (hr >= 0) hr = device.SetSamplerState(register, Samplerstatetype.Magfilter, filter);
                        if (hr >= 0) hr = device.SetSamplerState(register, Samplerstatetype.Addressu, 3);
                        if (hr >= 0) hr = device.SetSamplerState(register, Samplerstatetype.Addressv, 3);
                    }
                    for (int i = 0; i < data.FloatRegisters.Length && hr >= 0; i++)
                        hr = device.SetPixelShaderConstants((uint)data.FloatRegisters[i],
                            MemoryMarshal.Cast<float, Vector4>(data.FloatValues.AsSpan(i * 4, 4)));
                    for (int i = 0; i < data.IntRegisters.Length && hr >= 0; i++)
                        hr = device.SetPixelShaderInt4Constant((uint)data.IntRegisters[i], data.IntValues.AsSpan(i * 4, 4));
                    for (int i = 0; i < data.BoolRegisters.Length && hr >= 0; i++)
                        hr = device.SetPixelShaderBoolConstant((uint)data.BoolRegisters[i], data.BoolValues[i]);
                    if (hr >= 0 && data.Command.DdxRegister is >= 0 and < 224)
                    {
                        Span<Vector4> derivatives = stackalloc Vector4[1];
                        derivatives[0] = scope.UvDerivatives;
                        hr = device.SetPixelShaderConstants((uint)data.Command.DdxRegister, derivatives);
                    }
                    if (hr >= 0) hr = device.SetVertexShader(null);
                    if (hr >= 0) hr = device.SetFlexibleVertexFormat(0x104);
                    if (hr >= 0) hr = device.SetRenderState((Renderstatetype)27, 1);
                    if (hr >= 0) hr = device.SetRenderState((Renderstatetype)19, 2);
                    if (hr >= 0) hr = device.SetRenderState((Renderstatetype)20, 6);
                    if (hr < 0) break;
                    var a = scope.OutputTransform.Apply(0, 0);
                    var b = scope.OutputTransform.Apply(width, 0);
                    var c = scope.OutputTransform.Apply(0, height);
                    var d = scope.OutputTransform.Apply(width, height);
                    Span<float> vertices = stackalloc float[] {
                        (float)a.X - 0.5f, (float)a.Y - 0.5f, 0, 1, 0, 0,
                        (float)b.X - 0.5f, (float)b.Y - 0.5f, 0, 1, 1, 0,
                        (float)c.X - 0.5f, (float)c.Y - 0.5f, 0, 1, 0, 1,
                        (float)d.X - 0.5f, (float)d.Y - 0.5f, 0, 1, 1, 1 };
                    bool ownsScene = !device.IsInScene;
                    if (ownsScene)
                    {
                        hr = device.BeginScene();
                        sceneStarted = hr >= 0;
                    }
                    if (hr < 0) break;
                    fixed (float* verticesPointer = vertices)
                        hr = device.DrawPrimitiveUp(Primitivetype.Trianglestrip, 2, verticesPointer, 24);
                } while (false);
                }
                finally
                {
                    int cleanup;
                    if (sceneStarted)
                    {
                        cleanup = device.EndScene();
                        if (hr >= 0) hr = cleanup;
                    }
                    if (shaderBound)
                    {
                        cleanup = device.SetPixelShader(null);
                        if (hr >= 0) hr = cleanup;
                    }
                    for (uint register = 0; register < 16; register++)
                    {
                        if ((boundSamplers & (1u << (int)register)) == 0) continue;
                        cleanup = device.SetTexture(register, null);
                        if (hr >= 0) hr = cleanup;
                    }
                }
                return hr;
                }
                finally
                {
                    for (int i = secondaryInputs.Length - 1; i >= 0; i--)
                        secondaryInputs[i]?.Dispose();
                    for (int i = captures.Length - 1; i >= 0; i--) captures[i]?.Dispose();
                }
            }
        }
    }
}
