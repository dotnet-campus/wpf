using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9DisplayModeSnapshot(
    uint DisplayIndex,
    Direct3D9DisplayMode DisplayMode,
    Displayrotation DisplayRotation,
    uint BitsPerPixel);

internal readonly record struct Direct3D9GdiDisplayMode(
    uint Width,
    uint Height,
    uint RefreshRate,
    uint BitsPerPixel,
    uint Fields,
    uint DisplayOrientation);

internal delegate int Direct3D9DisplayModeProvider(uint displayIndex, out Direct3D9DisplayMode displayMode);
internal delegate int Direct3D9DisplayModeExProvider(
    uint displayIndex,
    out Direct3D9DisplayMode displayMode,
    out Displayrotation displayRotation);
internal delegate bool Direct3D9GdiDisplayModeProvider(string deviceName, out Direct3D9GdiDisplayMode displayMode, out int lastError);

internal static partial class Direct3D9DisplayModeSnapshotFactory
{
    private const int Direct3DNotAvailableHResult = unchecked((int) 0x8876086A);
    private const int Direct3DDeviceLostHResult = unchecked((int) 0x88760868);
    private const int Direct3DDriverInternalErrorHResult = unchecked((int) 0x88760827);
    private const int ErrorInvalidPrinterName = 1801;
    private const int ErrorInvalidMonitorHandle = 1461;
    private const uint DisplayOrientationField = 0x00000080;
    private const int EnumCurrentSettings = -1;

    internal static ImmutableArray<Direct3D9DisplayModeSnapshot> Read(
        Direct3D9AdapterArrangementResult adapters,
        Direct3D9DisplayModeProvider? direct3DModeProvider,
        Direct3D9DisplayModeExProvider? direct3DExModeProvider,
        Direct3D9GdiDisplayModeProvider gdiModeProvider)
    {
        ArgumentNullException.ThrowIfNull(gdiModeProvider);

        if (adapters.Direct3DAdapterCount > adapters.Displays.Length)
        {
            ThrowHResult(Direct3D9Factory.DisplayStateInvalidHResult);
        }

        ImmutableArray<Direct3D9DisplayModeSnapshot>.Builder result =
            ImmutableArray.CreateBuilder<Direct3D9DisplayModeSnapshot>(adapters.Displays.Length);
        foreach (Direct3D9AdapterSnapshot display in adapters.Displays)
        {
            Direct3D9DisplayMode displayMode = default;
            Displayrotation displayRotation = 0;
            int hResult = 0;

            if (display.DisplayIndex < adapters.Direct3DAdapterCount)
            {
                if (direct3DExModeProvider is not null)
                {
                    hResult = direct3DExModeProvider(display.DisplayIndex, out displayMode, out displayRotation);
                }
                else if (direct3DModeProvider is not null)
                {
                    hResult = direct3DModeProvider(display.DisplayIndex, out displayMode);
                }
            }

            if (hResult == Direct3DNotAvailableHResult)
            {
                hResult = 0;
            }
            else if (hResult < 0)
            {
                ThrowTranslatedHResult(hResult);
            }

            uint bitsPerPixel;
            if (displayMode.Format == Format.Unknown)
            {
                if (!gdiModeProvider(display.DeviceName, out Direct3D9GdiDisplayMode gdiMode, out int lastError))
                {
                    ThrowLastError(lastError);
                }

                displayMode = new Direct3D9DisplayMode(
                    (uint) Marshal.OffsetOf<Displaymodeex>(nameof(Displaymodeex.ScanLineOrdering)),
                    gdiMode.Width,
                    gdiMode.Height,
                    gdiMode.RefreshRate,
                    GetFormat(gdiMode.BitsPerPixel),
                    Scanlineordering.Unknown);
                if (displayRotation == 0 && (gdiMode.Fields & DisplayOrientationField) != 0)
                {
                    displayRotation = (Displayrotation) (gdiMode.DisplayOrientation + 1);
                }

                bitsPerPixel = gdiMode.BitsPerPixel;
            }
            else
            {
                bitsPerPixel = 8 * GetFormatSize(displayMode.Format);
            }

            result.Add(new Direct3D9DisplayModeSnapshot(
                display.DisplayIndex,
                displayMode,
                displayRotation,
                bitsPerPixel));
        }

        return result.MoveToImmutable();
    }

    [SupportedOSPlatform("windows5.0")]
    internal static unsafe bool ReadSystemGdiMode(
        string deviceName,
        out Direct3D9GdiDisplayMode displayMode,
        out int lastError)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceName);

        NativeDisplayMode nativeMode = default;
        nativeMode.Size = (ushort) sizeof(NativeDisplayMode);
        Marshal.SetLastPInvokeError(0);
        bool succeeded = EnumDisplaySettings(deviceName, EnumCurrentSettings, ref nativeMode);
        lastError = Marshal.GetLastPInvokeError();
        displayMode = new Direct3D9GdiDisplayMode(
            nativeMode.PelsWidth,
            nativeMode.PelsHeight,
            nativeMode.DisplayFrequency,
            nativeMode.BitsPerPel,
            nativeMode.Fields,
            nativeMode.DisplayOrientation);
        return succeeded;
    }

    private static Format GetFormat(uint bitsPerPixel)
    {
        return bitsPerPixel switch
        {
            32 => Format.X8R8G8B8,
            24 => Format.R8G8B8,
            16 => Format.R5G6B5,
            8 => Format.P8,
            _ => Format.Unknown
        };
    }

    private static uint GetFormatSize(Format format)
    {
        return format switch
        {
            Format.A32B32G32R32f => 16,
            Format.A8R8G8B8 or Format.X8R8G8B8 or Format.D24S8 or Format.A2R10G10B10 => 4,
            Format.R8G8B8 => 3,
            Format.R5G6B5 or Format.X1R5G5B5 or Format.D16 => 2,
            Format.P8 or Format.L8 => 1,
            _ => 0
        };
    }

    private static void ThrowLastError(int lastError)
    {
        if (lastError is ErrorInvalidMonitorHandle or ErrorInvalidPrinterName || lastError == 0)
        {
            ThrowHResult(Direct3D9Factory.DisplayStateInvalidHResult);
        }

        ThrowHResult(unchecked((int) (0x80070000u | (uint) lastError)));
    }

    private static void ThrowTranslatedHResult(int hResult)
    {
        ThrowHResult(hResult is Direct3DDeviceLostHResult or Direct3DDriverInternalErrorHResult
            ? Direct3D9Factory.DisplayStateInvalidHResult
            : hResult);
    }

    private static void ThrowHResult(int hResult)
    {
        Marshal.ThrowExceptionForHR(hResult);
    }

    [LibraryImport("user32.dll", EntryPoint = "EnumDisplaySettingsW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnumDisplaySettings(string deviceName, int modeNumber, ref NativeDisplayMode displayMode);

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Size = 220)]
    private unsafe struct NativeDisplayMode
    {
        [FieldOffset(68)]
        internal ushort Size;
        [FieldOffset(72)]
        internal uint Fields;
        [FieldOffset(84)]
        internal uint DisplayOrientation;
        [FieldOffset(168)]
        internal uint BitsPerPel;
        [FieldOffset(172)]
        internal uint PelsWidth;
        [FieldOffset(176)]
        internal uint PelsHeight;
        [FieldOffset(184)]
        internal uint DisplayFrequency;
    }
}
