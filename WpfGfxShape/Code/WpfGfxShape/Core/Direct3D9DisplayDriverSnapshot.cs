using Microsoft.Win32;
using System.Security;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9DisplayDriverSnapshot(
    uint MemorySize,
    string InstalledDisplayDriver,
    bool IsRecentDriver,
    bool IsBadDriver,
    uint GraphicsCardVendorId,
    uint GraphicsCardDeviceId);

internal static class Direct3D9DisplayDriverSnapshotFactory
{
    internal const ulong DefaultRequiredVideoDriverDate = 127437408000000000;

    private const string RegistryMachinePrefix = @"\Registry\Machine\";
    private const string AvalonGraphicsKeyPath = @"Software\Microsoft\Avalon.Graphics";
    private const string RequiredVideoDriverDateValueName = "RequiredVideoDriverDate";
    private const string MemorySizeValueName = "HardwareInformation.MemorySize";
    private const string InstalledDisplayDriversValueName = "InstalledDisplayDrivers";

    internal static Direct3D9DisplayDriverSnapshot ReadDeviceRegistry(
        string deviceKey,
        Func<string, (object? MemorySize, object? InstalledDisplayDrivers)> registryReader)
    {
        ArgumentNullException.ThrowIfNull(deviceKey);
        ArgumentNullException.ThrowIfNull(registryReader);

        if (!deviceKey.StartsWith(RegistryMachinePrefix, StringComparison.OrdinalIgnoreCase)
            || deviceKey.Length <= RegistryMachinePrefix.Length)
        {
            return default;
        }

        try
        {
            (object? memoryValue, object? driversValue) = registryReader(deviceKey[RegistryMachinePrefix.Length..]);
            uint memorySize = memoryValue is int signedMemorySize ? unchecked((uint) signedMemorySize) : 0;
            string installedDisplayDriver = driversValue switch
            {
                string driver => driver,
                string[] drivers when drivers.Length > 0 => drivers[0],
                _ => string.Empty,
            };
            return new Direct3D9DisplayDriverSnapshot(memorySize, installedDisplayDriver, false, false, 0, 0);
        }
        catch (Exception exception) when (exception is IOException or SecurityException or UnauthorizedAccessException)
        {
            return default;
        }
    }

    internal static Direct3D9DisplayDriverSnapshot ReadDeviceRegistry(string deviceKey)
    {
        return ReadDeviceRegistry(deviceKey, keyPath =>
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath);
            return key is null
                ? default
                : (key.GetValue(MemorySizeValueName), key.GetValue(InstalledDisplayDriversValueName));
        });
    }

    internal static ulong ReadRequiredVideoDriverDate(Func<object?> registryReader)
    {
        ArgumentNullException.ThrowIfNull(registryReader);

        try
        {
            if (registryReader() is string value
                && DateTime.TryParseExact(
                    value,
                    "yyyy/MM/dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime date))
            {
                return unchecked((ulong) DateTime.SpecifyKind(date, DateTimeKind.Utc).ToFileTimeUtc());
            }
        }
        catch (Exception exception) when (exception is IOException or SecurityException or UnauthorizedAccessException or ArgumentOutOfRangeException)
        {
        }

        return DefaultRequiredVideoDriverDate;
    }

    internal static ulong ReadRequiredVideoDriverDate()
    {
        return ReadRequiredVideoDriverDate(() =>
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(AvalonGraphicsKeyPath);
            return key?.GetValue(RequiredVideoDriverDateValueName);
        });
    }

    internal static bool CheckForRecentDriver(
        string installedDisplayDriver,
        ulong requiredVideoDriverDate,
        Func<string, long?> driverFileTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(installedDisplayDriver);
        ArgumentNullException.ThrowIfNull(driverFileTimeProvider);

        if (string.IsNullOrEmpty(installedDisplayDriver))
        {
            return true;
        }

        try
        {
            long? driverFileTime = driverFileTimeProvider(installedDisplayDriver);
            return !driverFileTime.HasValue || unchecked((ulong) driverFileTime.Value) >= requiredVideoDriverDate;
        }
        catch (Exception exception) when (exception is IOException or SecurityException or UnauthorizedAccessException or ArgumentException)
        {
            return true;
        }
    }

    internal static bool CheckForRecentDriver(string installedDisplayDriver, ulong requiredVideoDriverDate)
    {
        return CheckForRecentDriver(installedDisplayDriver, requiredVideoDriverDate, driver =>
        {
            string path = Path.Combine(Environment.SystemDirectory, $"{driver}.dll");
            return File.Exists(path) ? File.GetLastWriteTimeUtc(path).ToFileTimeUtc() : null;
        });
    }
}
