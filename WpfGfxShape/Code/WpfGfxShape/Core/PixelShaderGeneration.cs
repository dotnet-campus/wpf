using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal sealed class PixelShaderGeneration
{
    private readonly Dictionary<Direct3D9Device, Direct3D9PixelShaderEffect> _hardware = [];
    private readonly List<Direct3D9PixelShaderEffect> _owned = [];
    private int _references = 1;
    private SoftwarePixelShaderProgram? _software;
    private Direct3D9ShaderBytecode? _compiled;
    private bool _ignoreHardware;
    private bool _validated;
    private static readonly uint[] Identity = [0xffff0200, 0x0200001f, 0x80000000, 0xb0030000,
        0x0200001f, 0x90000000, 0xa00f0800, 0x03000042, 0x800f0000,
        0xb0e40000, 0xa0e40800, 0x02000001, 0x800f0800, 0x80e40000, 0x0000ffff];

    internal PixelShaderGeneration(MilShaderRenderMode renderMode, bool compileSoftware, ReadOnlyMemory<byte> bytecode)
    {
        RenderMode = renderMode;
        CompileSoftware = compileSoftware;
        Bytecode = bytecode;
    }

    internal MilShaderRenderMode RenderMode { get; }
    internal bool CompileSoftware { get; }
    internal ReadOnlyMemory<byte> Bytecode { get; }
    internal void AddRef() => _references++;

    internal void Release()
    {
        if (--_references != 0) return;
        for (int i = _owned.Count - 1; i >= 0; i--) _owned[i].Dispose();
        _owned.Clear();
        _hardware.Clear();
    }

    private static bool IsMemoryFailure(int hr) => hr is unchecked((int)0x8007000E) or unchecked((int)0x8876017C);

    internal int GetSoftwareProgram(Action? notify, out SoftwarePixelShaderProgram? program)
    {
        program = _software;
        if (program is not null) return 0;
        int hr = SoftwarePixelShaderProgram.Compile(Bytecode.Span, out program);
        if (hr < 0 && !IsMemoryFailure(hr))
        {
            notify?.Invoke();
            hr = SoftwarePixelShaderProgram.Compile(default, out program);
            if (hr >= 0) _ignoreHardware = true;
        }
        if (hr >= 0) _software = program;
        return hr;
    }

    internal unsafe int GetHardwareShader(Direct3D9Device device, Action? notify, out Direct3D9PixelShaderEffect? shader)
    {
        shader = null;
        if (!_validated)
        {
            if (CompileSoftware)
            {
                int validation = GetSoftwareProgram(notify, out _);
                if (validation < 0) return validation;
            }
            _validated = true;
        }
        if (_hardware.TryGetValue(device, out var cached) && !cached.IsReleased)
        {
            shader = cached;
            return 0;
        }
        int hr = 0;
        if (!_ignoreHardware && !Bytecode.IsEmpty)
        {
            if (_compiled is null)
                hr = Direct3D9ShaderBytecode.TryCreate(new(0, Direct3D9ShaderKind.Pixel, 0xffff0200, "VisualShaderEffect"),
                    MemoryMarshal.Cast<byte, uint>(Bytecode.Span), out _compiled);
            if (hr >= 0 && _compiled is not null)
                fixed (uint* instructions = _compiled.Instructions)
                    hr = Direct3D9PixelShaderEffect.TryCreate(device, instructions, _compiled.SizeInBytes, out shader);
            if (hr < 0)
            {
                if (IsMemoryFailure(hr)) return hr;
                notify?.Invoke();
            }
        }
        if (shader is null)
            fixed (uint* instructions = Identity)
                hr = Direct3D9PixelShaderEffect.TryCreate(device, instructions, checked((uint)Identity.Length * 4), out shader);
        if (hr < 0) return hr;
        if (shader is null) return Direct3D9Factory.InvalidCallHResult;
        _hardware[device] = shader;
        _owned.Add(shader);
        return hr;
    }
}
