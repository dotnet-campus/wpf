using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace WpfGfxShape.Core;

internal sealed class SoftwarePixelShaderProgram
{
    private const int Rejected = unchecked((int)0x80070057);
    private readonly record struct Instruction(int Opcode, uint Destination, uint[] Sources, int Control = 0);
    private readonly Instruction[] _instructions;
    private readonly Dictionary<int, Vector4> _definitions;
    private SoftwarePixelShaderProgram(Instruction[] instructions, Dictionary<int, Vector4> definitions)
    { _instructions = instructions; _definitions = definitions; }

    // Owned ps_2_0 identity: dcl t0.xy; dcl_2d s0; texld r0,t0,s0; mov oC0,r0.
    internal static ReadOnlySpan<uint> Identity => [0xffff0200, 0x0200001f, 0x80000000, 0xb0030000,
        0x0200001f, 0x90000000, 0xa00f0800, 0x03000042, 0x800f0000, 0xb0e40000,
        0xa0e40800, 0x02000001, 0x800f0800, 0x80e40000, 0xffff];

    internal static int Compile(ReadOnlySpan<byte> bytes, out SoftwarePixelShaderProgram? program)
    {
        program = null;
        if (bytes.IsEmpty) bytes = MemoryMarshal.AsBytes(Identity);
        if (bytes.Length % 4 != 0 || bytes.Length < 8) return Rejected;
        var words = MemoryMarshal.Cast<byte, uint>(bytes);
        if (words[0] != 0xffff0200) return Rejected;
        List<Instruction> instructions = [];
        Dictionary<int, Vector4> definitions = [];
        uint samplers = 0;
        for (int position = 1; position < words.Length;)
        {
            uint token = words[position++];
            int opcode = (int)(token & 0xffff);
            if (token == 0xffff)
            {
                if (position != words.Length) return Rejected;
                foreach (var instruction in instructions)
                    if (instruction.Opcode == 66 && (samplers & (1u << (int)(instruction.Sources[1] & 0x7ff))) == 0) return Rejected;
                program = new(instructions.ToArray(), definitions);
                return 0;
            }
            if ((token & 0x80000000) != 0 || opcode == 0xffff) return Rejected;
            int length = opcode == 0xfffe ? (int)((token >> 16) & 0x7fff) : (int)((token >> 24) & 15);
            if (length > words.Length - position) return Rejected;
            var operands = words.Slice(position, length);
            position += length;
            if (opcode == 0xfffe) continue;
            int control = (int)((token >> 16) & 255);
            if ((token & 0x70000000) != 0 || (opcode == 66 ? control > 2 : control != 0)) return Rejected;
            if (opcode == 0) { if (length != 0) return Rejected; continue; }
            if (opcode == 31)
            {
                if (length != 2 || !ValidRegister(operands[1]) || (operands[0] & 0x80000000) == 0) return Rejected;
                int type = RegisterType(operands[1]);
                if (type == 10)
                {
                    if (((operands[0] >> 27) & 15) != 2) return Rejected;
                    samplers |= 1u << (int)(operands[1] & 0x7ff);
                }
                else if (type == 3) instructions.Add(new(31, operands[1], []));
                else return Rejected;
                continue;
            }
            if (opcode is 47 or 48)
            {
                int type = opcode == 47 ? 14 : 7;
                if (length != (opcode == 47 ? 2 : 5) || (operands[0] & 0x80002000) != 0x80000000
                    || RegisterType(operands[0]) != type || (operands[0] & 0x7ff) >= 16) return Rejected;
                continue;
            }
            if (opcode == 81)
            {
                if (length != 5 || !ValidRegister(operands[0]) || RegisterType(operands[0]) != 2) return Rejected;
                definitions[(int)(operands[0] & 0x7ff)] = new(
                    ClampDefinition(operands[1]), ClampDefinition(operands[2]), ClampDefinition(operands[3]), ClampDefinition(operands[4]));
                continue;
            }
            int sourceCount = opcode switch
            {
                1 or 6 or 7 or 14 or 15 or 19 or 35 or 36 => 1,
                2 or 3 or 5 or 8 or 9 or 10 or 11 or 20 or 21 or 22 or 23 or 24 or 32 or 33 or 66 => 2,
                4 or 18 or 37 or 88 or 90 => 3,
                65 => 0,
                _ => -1
            };
            if (sourceCount < 0) return Rejected;
            if (length != sourceCount + 1 || !ValidRegister(operands[0])) return Rejected;
            int destinationType = RegisterType(operands[0]);
            if (destinationType != 0 && destinationType != 8 && !(opcode == 65 && destinationType == 3)) return Rejected;
            if (((operands[0] >> 20) & 15) > 3 || (operands[0] & 0x0000c000) != 0) return Rejected;
            for (int i = 1; i < operands.Length; i++)
            {
                if (!ValidRegister(operands[i])) return Rejected;
                int type = RegisterType(operands[i]);
                bool sampler = opcode == 66 && i == 2;
                if (sampler ? type != 10 : type is not (0 or 2 or 3)) return Rejected;
                int modifier = (int)((operands[i] >> 24) & 15);
                if (modifier is not (0 or 1 or 11 or 12) || (sampler && modifier != 0)) return Rejected;
            }
            if (opcode is >= 20 and <= 24)
            {
                int rows = opcode switch { 21 or 23 => 3, 24 => 2, _ => 4 };
                uint matrix = operands[2];
                for (int row = 0; row < rows; row++)
                    if (((operands[0] >> (16 + row)) & 1) != 0 && !ValidRegister(matrix + (uint)row)) return Rejected;
            }
            if (opcode == 37 && ((operands[0] >> 16) & 12) != 0) return Rejected;
            instructions.Add(new(opcode, operands[0], operands[1..].ToArray(), control));
        }
        return Rejected;
    }

    private static int RegisterType(uint token) => (int)(((token >> 28) & 7) | ((token >> 8) & 24));
    private static bool ValidRegister(uint token)
    {
        if ((token & 0x80000000) == 0 || (token & 0x2000) != 0) return false;
        uint count = RegisterType(token) switch { 0 => 12, 2 => 32, 3 => 8, 8 => 4, 10 => 16, _ => 0 };
        return (token & 0x7ff) < count;
    }
    private static float ClampDefinition(uint bits) => Max(-float.MaxValue, Min(float.MaxValue, BitConverter.UInt32BitsToSingle(bits)));
    private static float Min(float a, float b) => a < b ? a : b;
    private static float Max(float a, float b) => a > b ? a : b;
    private static Vector4 Min(Vector4 a, Vector4 b) => new(Min(a.X,b.X), Min(a.Y,b.Y), Min(a.Z,b.Z), Min(a.W,b.W));
    private static Vector4 Max(Vector4 a, Vector4 b) => new(Max(a.X,b.X), Max(a.Y,b.Y), Max(a.Z,b.Z), Max(a.W,b.W));

    internal uint Execute(float u, float v, ReadOnlySpan<Vector4> constants, SoftwareShaderSampler[] samplers)
    {
        Vector4 output = ExecuteFloat(u, v, constants, samplers);
        return Pack(output.Z) | (Pack(output.Y) << 8) | (Pack(output.X) << 16) | (Pack(output.W) << 24);
    }

    internal Vector4 ExecuteFloat(float u, float v, ReadOnlySpan<Vector4> constants, SoftwareShaderSampler[] samplers)
    {
        Span<Vector4> registers = stackalloc Vector4[74];
        registers.Clear();
        constants.CopyTo(registers.Slice(14, constants.Length));
        foreach (var pair in _definitions) registers[14 + pair.Key] = pair.Value;
        uint killedComponents = 0;
        foreach (var instruction in _instructions)
        {
            int destination = Index(instruction.Destination);
            if (instruction.Opcode == 65)
            {
                var kill = registers[destination];
                uint killMask = (instruction.Destination >> 16) & 15;
                killedComponents |= killMask & (uint)((kill.X < 0 ? 1 : 0) | (kill.Y < 0 ? 2 : 0) |
                    (kill.Z < 0 ? 4 : 0) | (kill.W < 0 ? 8 : 0));
                continue;
            }
            if (instruction.Opcode == 31)
            {
                Write(registers, destination, instruction.Destination, new(u, v, 0, 0));
                continue;
            }
            Vector4 a = Read(instruction.Sources[0], registers);
            Vector4 b = instruction.Sources.Length > 1 ? Read(instruction.Sources[1], registers) : default;
            Vector4 c = instruction.Sources.Length > 2 ? Read(instruction.Sources[2], registers) : default;
            Vector4 result;
            if (instruction.Opcode is >= 20 and <= 24)
            {
                result = default;
                int rows = instruction.Opcode switch { 21 or 23 => 3, 24 => 2, _ => 4 };
                int columns = instruction.Opcode <= 21 ? 4 : 3;
                for (int row = 0; row < rows; row++)
                    if (((instruction.Destination >> (16 + row)) & 1) != 0)
                        result[row] = Dot(a, Read(instruction.Sources[1] + (uint)row, registers), columns);
            }
            else if (instruction.Opcode == 66)
            {
                if (instruction.Control == 1) a *= Reciprocal(a.W);
                result = Swizzle(samplers[instruction.Sources[1] & 0x7ff].Sample(a.X, a.Y), instruction.Sources[1]);
                if (destination != Index(instruction.Sources[0])) registers[destination] = default;
            }
            else result = instruction.Opcode switch
            {
                1 => a, 2 => a + b, 3 => a + -b, 5 => a * b, 4 => a * b + c,
                6 => new(Reciprocal(a.X)), 7 => new(ReciprocalSqrt(a.X)),
                8 => new(Dot(a,b,3)), 9 => new(Dot(a,b,4)),
                10 => Min(a,b), 11 => Max(a,b), 14 => new(Exp2(a.X)), 15 => new(Log2(a.X)),
                18 => a * (b - c) + c,
                19 => new(Fraction(a.X), Fraction(a.Y), Fraction(a.Z), Fraction(a.W)),
                32 => new(Exp2(Log2(a.X) * b.X)),
                33 => new(a.Y*b.Z-a.Z*b.Y, a.Z*b.X-a.X*b.Z, a.X*b.Y-a.Y*b.X, 0),
                35 => Vector4.Abs(a), 36 => a * ReciprocalSqrt(Dot(a,a,3)),
                37 => SinCos20(a.X,b,c),
                88 => new(a.X >= 0 ? b.X : c.X, a.Y >= 0 ? b.Y : c.Y, a.Z >= 0 ? b.Z : c.Z, a.W >= 0 ? b.W : c.W),
                90 => new(a.X * b.X + a.Y * b.Y + c.X),
                _ => throw new InvalidOperationException()
            };
            int shift = (int)((instruction.Destination >> 24) & 15);
            if (shift != 0) result *= MathF.ScaleB(1, shift < 8 ? shift : shift - 16);
            bool saturate = (instruction.Destination & 0x00100000) != 0;
            result = Min(Max(result, saturate ? Vector4.Zero : new(-float.MaxValue)), saturate ? Vector4.One : new(float.MaxValue));
            Write(registers, destination, instruction.Destination, result);
        }
        var output = registers[54];
        return new((killedComponents & 1) != 0 ? 0 : output.X, (killedComponents & 2) != 0 ? 0 : output.Y,
            (killedComponents & 4) != 0 ? 0 : output.Z, (killedComponents & 8) != 0 ? 0 : output.W);
    }

    private static void Write(Span<Vector4> registers, int destination, uint token, Vector4 result)
    {
        Vector4 old = registers[destination];
        uint mask = (token >> 16) & 15;
        registers[destination] = new((mask & 1) != 0 ? result.X : old.X, (mask & 2) != 0 ? result.Y : old.Y,
            (mask & 4) != 0 ? result.Z : old.Z, (mask & 8) != 0 ? result.W : old.W);
    }
    private static float Dot(Vector4 a, Vector4 b, int count)
    {
        float result = a.X*b.X + a.Y*b.Y + a.Z*b.Z;
        return count == 4 ? result + a.W*b.W : result;
    }
    private static Vector4 SinCos20(float x, Vector4 b, Vector4 c)
    {
        float q = x*x;
        float px = q*b.X+b.W, py = q*b.Y+b.Z;
        px = px*q+c.X; py = py*q+c.Y;
        px = px*q+c.W; py = py*q+c.Z;
        px *= x;
        float s = px;
        px *= s; py *= s;
        px += px; py += py;
        return new(c.Z-px, py, 0, 0);
    }
    private static float Reciprocal(float x) => Sse.IsSupported ? Sse.Reciprocal(Vector128.Create(x)).GetElement(0) : 1/x;
    private static float ReciprocalSqrt(float x) => Sse.IsSupported ? Sse.ReciprocalSqrt(Vector128.Create(MathF.Abs(x))).GetElement(0) : 1/MathF.Sqrt(MathF.Abs(x));
    private static float Log2(float x)
    {
        uint bits = BitConverter.SingleToUInt32Bits(MathF.Abs(x));
        int index = (int)((bits >> 21) & 3);
        ReadOnlySpan<float> table = [0, .321928095f, .584962501f, .807354922f];
        float m = BitConverter.UInt32BitsToSingle((bits & 0x7fffff) | 0x3f800000);
        float t = BitConverter.UInt32BitsToSingle(((uint)index << 21) | 0x3f800000);
        float z = (m-t)/(m+t), z2 = z*z, z3 = z*z2, z5 = z3*z2;
        float remainder = z*2.885390081777930f+z3*.961796693925976f+z5*.577078016355585f;
        // Native LOG discards the return value of its zero-input Blend.
        return ((int)(bits >> 23)-127)+table[index]+remainder;
    }
    private static float Exp2(float x)
    {
        float abs = MathF.Abs(x);
        uint bits = BitConverter.SingleToUInt32Bits(abs+1048576f);
        uint a = (bits >> 3) & 255, index = bits & 7;
        float b = index/8f, c = abs-(a+b), c2 = c*c, c3 = c2*c;
        ReadOnlySpan<float> table = [1,1.090507733f,1.189207115f,1.296839555f,1.414213562f,1.542210825f,1.681792831f,1.834008086f];
        float result = BitConverter.UInt32BitsToSingle((a+127)<<23)*table[(int)index]*(1+c*.693147180559945f+c2*.240226506959101f+c3*.0555041086648216f);
        if (x < 0) result = Reciprocal(result);
        if (abs > 128) result = x < 0 ? 0 : float.PositiveInfinity;
        return result;
    }
    private static float Fraction(float value) => ((BitConverter.SingleToUInt32Bits(value) >> 23) & 255) > 149
        ? 0 : Min(value-MathF.Floor(value), BitConverter.UInt32BitsToSingle(0x3f7fffff));
    private static uint Pack(float value) => (uint)MathF.Round(Max(Min(value*255,255),0), MidpointRounding.ToEven);
    private static int Index(uint token) => (RegisterType(token) switch { 0 => 0, 2 => 14, 3 => 46, 8 => 54, 10 => 58, _ => throw new InvalidOperationException() }) + (int)(token & 0x7ff);
    private static Vector4 Swizzle(Vector4 value, uint token)
    {
        uint swizzle = token >> 16;
        return new(value[(int)(swizzle & 3)], value[(int)((swizzle >> 2) & 3)], value[(int)((swizzle >> 4) & 3)], value[(int)((swizzle >> 6) & 3)]);
    }
    private static Vector4 Read(uint token, Span<Vector4> registers)
    {
        Vector4 result = Swizzle(registers[Index(token)],token);
        int modifier = (int)((token >> 24) & 15);
        if (modifier is 11 or 12) result = Vector4.Abs(result);
        return modifier is 1 or 12 ? -result : result;
    }
}

internal sealed record SoftwareShaderSampler(uint[] Pixels, int Width, int Height, bool Linear)
{
    internal Vector4[]? FloatPixels { get; init; }
    internal Vector4 Sample(float u, float v)
    {
        float x = u*Width, y = v*Height;
        x = x > 0 ? x : 0; y = y > 0 ? y : 0;
        if (!Linear) return Texel(ClampUnsigned(Convert(x),Width), ClampUnsigned(Convert(y),Height));
        int left = ClampUnsigned(Convert(MathF.Floor(x)),Width), top = ClampUnsigned(Convert(MathF.Floor(y)),Height);
        int right = Math.Min(left+1,Width-1), bottom = Math.Min(top+1,Height-1);
        float fx = x-left, fy = y-top;
        return (Texel(left,top)*(1-fx)+Texel(right,top)*fx)*(1-fy)
            +(Texel(left,bottom)*(1-fx)+Texel(right,bottom)*fx)*fy;
    }
    private static int ClampUnsigned(int value,int size) => (int)Math.Min((uint)value,(uint)(size-1));
    private static int Convert(float value) => float.IsNaN(value) || value >= 2147483648f || value < -2147483648f ? int.MinValue : (int)MathF.Round(value,MidpointRounding.ToEven);
    private Vector4 Texel(int x, int y)
    {
        if (FloatPixels is { } floating) return floating[y * Width + x];
        uint pixel = Pixels[y*Width+x];
        return new Vector4((pixel >> 16) & 255, (pixel >> 8) & 255, pixel & 255, pixel >> 24)/255;
    }
}
