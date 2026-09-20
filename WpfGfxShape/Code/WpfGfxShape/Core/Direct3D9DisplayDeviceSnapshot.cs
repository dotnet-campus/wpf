using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9DisplayDeviceSnapshot(
    string DeviceName,
    uint StateFlags,
    string DeviceKey = "");

internal readonly record struct Direct3D9DisplayDeviceEnumerationResult(
    ImmutableArray<Direct3D9DisplayDeviceSnapshot> Devices,
    bool IsNonLocalDevicePresent);

internal readonly record struct Direct3D9DisplayDeviceEnumerationEntry(
    bool HasDevice,
    ReadOnlyMemory<char> DeviceNameBuffer,
    uint StateFlags,
    ReadOnlyMemory<char> DeviceKeyBuffer = default);

internal delegate Direct3D9DisplayDeviceEnumerationEntry Direct3D9DisplayDeviceEnumerator(uint deviceIndex);

internal static class Direct3D9DisplayDeviceSnapshotFactory
{
    private const uint AttachedToDesktop = 0x00000001;
    private const uint MirroringDriver = 0x00000008;
    private const uint Remote = 0x04000000;

    internal static Direct3D9DisplayDeviceEnumerationResult Enumerate(
        Direct3D9DisplayDeviceEnumerator enumerator,
        bool isMultiAdapterCodeEnabled)
    {
        ArgumentNullException.ThrowIfNull(enumerator);

        ImmutableArray<Direct3D9DisplayDeviceSnapshot>.Builder devices =
            ImmutableArray.CreateBuilder<Direct3D9DisplayDeviceSnapshot>();
        bool isNonLocalDevicePresent = false;

        for (uint deviceIndex = 0; ; deviceIndex++)
        {
            Direct3D9DisplayDeviceEnumerationEntry entry = enumerator(deviceIndex);
            if (!entry.HasDevice)
            {
                break;
            }

            if ((entry.StateFlags & AttachedToDesktop) == 0)
            {
                continue;
            }

            if ((entry.StateFlags & (Remote | MirroringDriver)) != 0)
            {
                isNonLocalDevicePresent = true;
                if ((entry.StateFlags & MirroringDriver) != 0)
                {
                    continue;
                }
            }

            devices.Add(new Direct3D9DisplayDeviceSnapshot(
                ValidateDeviceName(entry.DeviceNameBuffer.Span),
                entry.StateFlags,
                ReadOptionalString(entry.DeviceKeyBuffer.Span)));

            if (!isMultiAdapterCodeEnabled)
            {
                break;
            }
        }

        return new Direct3D9DisplayDeviceEnumerationResult(devices.ToImmutable(), isNonLocalDevicePresent);
    }

    [SupportedOSPlatform("windows5.0")]
    internal static Direct3D9DisplayDeviceEnumerationResult EnumerateSystem(bool isMultiAdapterCodeEnabled)
    {
        return Enumerate(EnumerateSystemDevice, isMultiAdapterCodeEnabled);
    }

    private static string ValidateDeviceName(ReadOnlySpan<char> deviceNameBuffer)
    {
        int terminatorIndex = deviceNameBuffer.IndexOf('\0');
        if (terminatorIndex <= 0)
        {
            Marshal.ThrowExceptionForHR(unchecked((int) 0x80070057));
        }

        return new string(deviceNameBuffer[..terminatorIndex]);
    }

    private static string ReadOptionalString(ReadOnlySpan<char> buffer)
    {
        int terminatorIndex = buffer.IndexOf('\0');
        return terminatorIndex <= 0 ? string.Empty : new string(buffer[..terminatorIndex]);
    }

    [SupportedOSPlatform("windows5.0")]
    private static unsafe Direct3D9DisplayDeviceEnumerationEntry EnumerateSystemDevice(uint deviceIndex)
    {
        DISPLAY_DEVICEW displayDevice = default;
        displayDevice.cb = (uint) sizeof(DISPLAY_DEVICEW);

        bool hasDevice = PInvoke.EnumDisplayDevices(null, deviceIndex, ref displayDevice, 0);
        if (!hasDevice)
        {
            return default;
        }

        char[] deviceNameBuffer = new char[32];
        displayDevice.DeviceName.ToString().AsSpan().CopyTo(deviceNameBuffer);
        char[] deviceKeyBuffer = new char[128];
        displayDevice.DeviceKey.ToString().AsSpan().CopyTo(deviceKeyBuffer);

        return new Direct3D9DisplayDeviceEnumerationEntry(
            true,
            deviceNameBuffer,
            (uint) displayDevice.StateFlags,
            deviceKeyBuffer);
    }
}
