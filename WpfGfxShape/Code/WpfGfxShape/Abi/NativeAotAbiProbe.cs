using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

internal static unsafe class NativeAotAbiProbe
{
    internal const uint SupportedAbiVersion = 1;

    internal const int S_OK = 0;
    internal const int E_NOTIMPL = unchecked((int) 0x80004001);
    internal const int E_POINTER = unchecked((int) 0x80004003);
    internal const int E_FAIL = unchecked((int) 0x80004005);
    internal const int E_UNEXPECTED = unchecked((int) 0x8000FFFF);
    internal const int E_INVALIDARG = unchecked((int) 0x80070057);

    private const ulong ChecksumSeed = 0x5750464746580001UL;

    [UnmanagedCallersOnly(
        EntryPoint = "WpfGfxShape_NativeAotAbiProbe_v1",
        CallConvs = [typeof(CallConvStdcall)])]
    private static int Export(
        uint abiVersion,
        uint operation,
        ulong input,
        nuint context,
        ulong* output)
    {
        return Invoke(abiVersion, operation, input, context, output);
    }

    internal static int Invoke(
        uint abiVersion,
        uint operation,
        ulong input,
        nuint context,
        ulong* output)
    {
        if (output is null)
        {
            return E_POINTER;
        }

        *output = 0;

        if (abiVersion != SupportedAbiVersion)
        {
            return E_INVALIDARG;
        }

        try
        {
            return Execute(operation, input, context, output);
        }
        catch (InvalidOperationException)
        {
            return E_FAIL;
        }
        catch
        {
            return E_UNEXPECTED;
        }
    }

    internal static ulong CalculateChecksum(ulong input, nuint context)
    {
        return unchecked(
            ChecksumSeed
            ^ input
            ^ (ulong) context
            ^ ((ulong) (uint) IntPtr.Size << 56));
    }

    private static int Execute(uint operation, ulong input, nuint context, ulong* output)
    {
        switch (operation)
        {
            case 0:
                *output = CalculateChecksum(input, context);
                return S_OK;
            case 1:
                return E_INVALIDARG;
            case 2:
                throw new InvalidOperationException("Controlled Native AOT ABI probe failure.");
            default:
                return E_NOTIMPL;
        }
    }
}
