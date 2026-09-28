using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal enum MilShaderRenderMode : uint { Auto, SoftwareOnly, HardwareOnly }
internal enum MilKernelType : uint { Gaussian, Box }
internal enum MilEffectRenderingBias : uint { Performance, Quality }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilPixelShaderCommand(MilCommand Type, uint Handle, MilShaderRenderMode RenderMode, uint BytecodeSize, int CompileSoftwareShader) : IMilResourceUpdateCommand;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilImplicitInputBrushCommand(MilCommand Type, uint Handle, double Opacity, uint OpacityAnimation, uint Transform, uint RelativeTransform) : IMilResourceUpdateCommand;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilBlurEffectCommand(MilCommand Type, uint Handle, double Radius, uint RadiusAnimation, MilKernelType Kernel, MilEffectRenderingBias Bias) : IMilResourceUpdateCommand;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilDropShadowEffectCommand(MilCommand Type, uint Handle, double Depth, MilColorF Color, double Direction, double Opacity, double BlurRadius, uint DepthAnimation, uint ColorAnimation, uint DirectionAnimation, uint OpacityAnimation, uint BlurAnimation, MilEffectRenderingBias Bias) : IMilResourceUpdateCommand;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly record struct MilShaderEffectCommand(MilCommand Type, uint Handle, double Top, double Bottom, double Left, double Right, uint PixelShader, int DdxRegister, uint FloatRegistersSize, uint FloatValuesSize, uint IntRegistersSize, uint IntValuesSize, uint BoolRegistersSize, uint BoolValuesSize, uint SamplerInfoSize, uint SamplerValuesSize) : IMilResourceUpdateCommand;

internal abstract class GeneratedEffectResource(MilResourceType type) : GeneratedDependencyResource(type)
{
    protected static int Malformed => Direct3D9Factory.UceMalformedPacketHResult;

    protected static bool ReadCommand<T>(ReadOnlySpan<byte> value, out T command, bool variable = false) where T : unmanaged
    {
        int size = Marshal.SizeOf<T>();
        if (value.Length < size - 8 || (!variable && value.Length != size - 8))
        {
            command = default;
            return false;
        }

        // ProcessUpdate receives the command body without the transport identity header.
        Span<byte> packet = stackalloc byte[size];
        packet.Clear();
        value[..(size - 8)].CopyTo(packet[8..]);
        command = MemoryMarshal.Read<T>(packet);
        return true;
    }

    internal static bool IsBrush(MilResourceType type) => type == MilResourceType.ImplicitInputBrush || type is >= MilResourceType.SolidColorBrush and <= MilResourceType.BitmapCacheBrush;
    protected static bool IsTransform(MilResourceType type) => type is >= MilResourceType.TransformGroup and <= MilResourceType.MatrixTransform;
}

internal sealed class GeneratedPixelShaderResource() : GeneratedEffectResource(MilResourceType.PixelShader)
{
    internal MilShaderRenderMode RenderMode { get; private set; }
    internal bool CompileSoftwareShader { get; private set; }
    internal ReadOnlyMemory<byte> Bytecode { get; private set; }

    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (!ReadCommand(value, out MilPixelShaderCommand command, true) || command.RenderMode > MilShaderRenderMode.HardwareOnly
            || command.BytecodeSize != value.Length - 12 || command.BytecodeSize % 4 != 0) return Malformed;
        byte[] bytecode = value[12..].ToArray();
        return CommitDependencies([], () => { RenderMode = command.RenderMode; CompileSoftwareShader = command.CompileSoftwareShader != 0; Bytecode = bytecode; });
    }

    protected override void OnFinalRelease() { base.OnFinalRelease(); Bytecode = default; }
}

internal sealed class GeneratedImplicitInputBrushResource() : GeneratedEffectResource(MilResourceType.ImplicitInputBrush)
{
    internal MilImplicitInputBrushCommand Data { get; private set; }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (!ReadCommand(value, out MilImplicitInputBrushCommand command)
            || !TryResolve(table, command.OpacityAnimation, MilResourceType.DoubleResource, out var animation)
            || !TryResolve(table, command.Transform, IsTransform, out var transform)
            || !TryResolve(table, command.RelativeTransform, IsTransform, out var relative)) return Malformed;
        return CommitDependencies([animation, transform, relative], () => Data = command);
    }
}

internal sealed class GeneratedBlurEffectResource() : GeneratedEffectResource(MilResourceType.BlurEffect)
{
    internal MilBlurEffectCommand Data { get; private set; }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (!ReadCommand(value, out MilBlurEffectCommand command) || command.Kernel > MilKernelType.Box || command.Bias > MilEffectRenderingBias.Quality
            || !TryResolve(table, command.RadiusAnimation, MilResourceType.DoubleResource, out var animation)) return Malformed;
        return CommitDependencies([animation], () => Data = command);
    }
}

internal sealed class GeneratedDropShadowEffectResource() : GeneratedEffectResource(MilResourceType.DropShadowEffect)
{
    internal MilDropShadowEffectCommand Data { get; private set; }
    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (!ReadCommand(value, out MilDropShadowEffectCommand command) || command.Bias > MilEffectRenderingBias.Quality
            || !TryResolve(table, command.DepthAnimation, MilResourceType.DoubleResource, out var depth)
            || !TryResolve(table, command.ColorAnimation, MilResourceType.ColorResource, out var color)
            || !TryResolve(table, command.DirectionAnimation, MilResourceType.DoubleResource, out var direction)
            || !TryResolve(table, command.OpacityAnimation, MilResourceType.DoubleResource, out var opacity)
            || !TryResolve(table, command.BlurAnimation, MilResourceType.DoubleResource, out var blur)) return Malformed;
        return CommitDependencies([depth, color, direction, opacity, blur], () => Data = command);
    }
}

internal sealed record GeneratedShaderEffectData(MilShaderEffectCommand Command, short[] FloatRegisters, float[] FloatValues, short[] IntRegisters, int[] IntValues, short[] BoolRegisters, int[] BoolValues, int[] SamplerInfo, IReadOnlyList<GeneratedProtocolResource?> Samplers, GeneratedProtocolResource? PixelShader);

internal sealed class GeneratedShaderEffectResource() : GeneratedEffectResource(MilResourceType.ShaderEffect)
{
    internal GeneratedShaderEffectData? Data { get; private set; }

    internal override int ProcessUpdate(GeneratedProtocolHandleTable table, ReadOnlySpan<byte> value)
    {
        if (!ReadCommand(value, out MilShaderEffectCommand command, true)) return Malformed;
        uint[] sizes = [command.FloatRegistersSize, command.FloatValuesSize, command.IntRegistersSize, command.IntValuesSize, command.BoolRegistersSize, command.BoolValuesSize, command.SamplerInfoSize, command.SamplerValuesSize];
        int[] alignments = [2, 4, 2, 4, 2, 4, 8, 4];
        long total = 72;
        for (int i = 0; i < sizes.Length; i++)
        {
            if (sizes[i] % alignments[i] != 0) return Malformed;
            total += sizes[i];
        }
        if (total != value.Length || (long)sizes[0] * 8 != sizes[1] || (long)sizes[2] * 8 != sizes[3]
            || (long)sizes[4] * 2 != sizes[5] || sizes[6] / 8 != sizes[7] / 4
            || !TryResolve(table, command.PixelShader, MilResourceType.PixelShader, out var shader)) return Malformed;

        ReadOnlySpan<byte> payload = value[72..];
        short[] floatRegisters = Take<short>(ref payload, sizes[0]);
        float[] floatValues = Take<float>(ref payload, sizes[1]);
        short[] intRegisters = Take<short>(ref payload, sizes[2]);
        int[] intValues = Take<int>(ref payload, sizes[3]);
        short[] boolRegisters = Take<short>(ref payload, sizes[4]);
        int[] boolValues = Take<int>(ref payload, sizes[5]);
        int[] samplerInfo = Take<int>(ref payload, sizes[6]);
        uint[] samplerHandles = Take<uint>(ref payload, sizes[7]);
        if (floatRegisters.Any(static r => r < 0) || intRegisters.Any(static r => r < 0) || boolRegisters.Any(static r => r < 0)
            || samplerInfo.Any(static v => v < 0)) return Malformed;

        GeneratedProtocolResource?[] samplers = new GeneratedProtocolResource?[samplerHandles.Length];
        for (int i = 0; i < samplers.Length; i++)
        {
            if (samplerInfo[i * 2 + 1] > 2 || !TryResolve(table, samplerHandles[i], IsBrush, out samplers[i])) return Malformed;
        }
        GeneratedShaderEffectData data = new(command, floatRegisters, floatValues, intRegisters, intValues, boolRegisters, boolValues, samplerInfo, samplers, shader);
        return CommitDependencies([shader, .. samplers], () => Data = data);
    }

    private static T[] Take<T>(ref ReadOnlySpan<byte> payload, uint size) where T : unmanaged
    {
        T[] result = MemoryMarshal.Cast<byte, T>(payload[..(int)size]).ToArray();
        payload = payload[(int)size..];
        return result;
    }

    protected override void OnFinalRelease() { base.OnFinalRelease(); Data = null; }
}

internal static partial class GeneratedProtocolPacketWriter
{
    internal static byte[] WriteEffect<T>(T command, ReadOnlySpan<byte> payload = default) where T : unmanaged, IMilResourceUpdateCommand
    {
        int size = Marshal.SizeOf<T>();
        byte[] packet = new byte[checked(size + payload.Length)];
        MemoryMarshal.Write(packet, in command);
        payload.CopyTo(packet.AsSpan(size));
        return packet;
    }
}
