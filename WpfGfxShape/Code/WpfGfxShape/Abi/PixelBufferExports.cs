using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class PixelBufferExports
{
    [UnmanagedCallersOnly(EntryPoint = "MilUtility_CopyPixelBuffer", CallConvs = [typeof(CallConvStdcall)])]
    private static int Copy(byte* output, uint outputSize, uint outputStride, uint outputOffset,
        byte* input, uint inputSize, uint inputStride, uint inputOffset, uint height, uint width)
    {
        if (height == 0 || width == 0) return 0;
        if (outputOffset > 7 || inputOffset > 7) return Direct3D9Factory.InvalidArgumentHResult;
        int hr = Validate(outputSize, outputStride, outputOffset, height, width);
        if (hr < 0) return hr;
        hr = Validate(inputSize, inputStride, inputOffset, height, width);
        if (hr < 0) return hr;
        for (uint row = 0; row < height; row++)
        {
            byte* destination = output + (nuint)row * outputStride;
            byte* source = input + (nuint)row * inputStride;
            for (ulong bit = 0; bit < width; bit++)
            {
                ulong sourceBit = bit + inputOffset, destinationBit = bit + outputOffset;
                int sourceMask = 0x80 >> (int)(sourceBit & 7);
                int destinationMask = 0x80 >> (int)(destinationBit & 7);
                byte* value = destination + (nuint)(destinationBit >> 3);
                *value = (byte)((*value & ~destinationMask)
                    | ((source[(nuint)(sourceBit >> 3)] & sourceMask) != 0 ? destinationMask : 0));
            }
        }
        return 0;
    }

    private static int Validate(uint size, uint stride, uint offset, uint height, uint width)
    {
        ulong bits = (ulong)offset + width;
        if (bits > uint.MaxValue) return Direct3D9Factory.ArithmeticOverflowHResult;
        ulong rowBytes = (bits + 7) / 8;
        if (stride < rowBytes) return Direct3D9Factory.InvalidArgumentHResult;
        ulong precedingRows = (ulong)stride * (height - 1);
        if (precedingRows > uint.MaxValue || precedingRows + rowBytes > uint.MaxValue)
            return Direct3D9Factory.ArithmeticOverflowHResult;
        return size < precedingRows + rowBytes ? Direct3D9Factory.InvalidArgumentHResult : 0;
    }
}
