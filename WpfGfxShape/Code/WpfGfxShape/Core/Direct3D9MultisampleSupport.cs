using Microsoft.Win32;
using Silk.NET.Direct3D9;
using System.Security;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9MultisampleSupport(
    MultisampleType Bgr32,
    MultisampleType Pbgra32,
    MultisampleType Bgr101010);

internal static class Direct3D9MultisampleSupportFactory
{
    private const string AvalonGraphicsKeyPath = @"Software\Microsoft\Avalon.Graphics";
    private const string MaxMultisampleTypeValueName = "MaxMultisampleType";

    internal static Direct3D9MultisampleSupport Gather(
        uint adapterOrdinal,
        Devtype deviceType,
        bool hasWddmSupport,
        uint? configuredMaximum,
        Func<uint, Devtype, Format, bool, MultisampleType, int> checkDeviceMultisampleType)
    {
        ArgumentNullException.ThrowIfNull(checkDeviceMultisampleType);

        MultisampleType maximum = configuredMaximum.HasValue
            ? (MultisampleType) configuredMaximum.Value
            : hasWddmSupport
                ? MultisampleType.Multisample4Samples
                : MultisampleType.MultisampleNone;

        maximum = GetMaximumWithDepthSupport(
            adapterOrdinal,
            deviceType,
            Format.D24S8,
            Format.D24S8,
            maximum,
            checkDeviceMultisampleType);

        return new Direct3D9MultisampleSupport(
            GetMaximumWithDepthSupport(
                adapterOrdinal,
                deviceType,
                Format.X8R8G8B8,
                Format.D24S8,
                maximum,
                checkDeviceMultisampleType),
            GetMaximumWithDepthSupport(
                adapterOrdinal,
                deviceType,
                Format.A8R8G8B8,
                Format.D24S8,
                maximum,
                checkDeviceMultisampleType),
            GetMaximumWithDepthSupport(
                adapterOrdinal,
                deviceType,
                Format.A2R10G10B10,
                Format.D24S8,
                maximum,
                checkDeviceMultisampleType));
    }

    internal static uint? ReadConfiguredMaximum()
    {
        try
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(AvalonGraphicsKeyPath);
            return key?.GetValue(MaxMultisampleTypeValueName) is int value
                ? unchecked((uint) value)
                : null;
        }
        catch (Exception exception) when (exception is IOException or SecurityException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static MultisampleType GetMaximumWithDepthSupport(
        uint adapterOrdinal,
        Devtype deviceType,
        Format targetFormat,
        Format depthFormat,
        MultisampleType maximum,
        Func<uint, Devtype, Format, bool, MultisampleType, int> checkDeviceMultisampleType)
    {
        for (int value = (int) maximum; value >= (int) MultisampleType.Multisample2Samples; value--)
        {
            MultisampleType candidate = (MultisampleType) value;
            if (checkDeviceMultisampleType(adapterOrdinal, deviceType, targetFormat, true, candidate) >= 0
                && (targetFormat == depthFormat
                    || checkDeviceMultisampleType(adapterOrdinal, deviceType, depthFormat, true, candidate) >= 0))
            {
                return candidate;
            }
        }

        return MultisampleType.MultisampleNone;
    }
}
