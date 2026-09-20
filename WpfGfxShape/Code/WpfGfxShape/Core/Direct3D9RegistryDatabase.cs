using Microsoft.Win32;
using System.Security;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9RegistryValue(bool WasRead, RegistryValueKind Kind, uint DwordValue);

internal sealed class Direct3D9RegistryDatabase
{
    private const uint MaximumErrorCount = 5;
    private const string AvalonGraphicsKeyPath = @"Software\Microsoft\Avalon.Graphics";
    private const string DisableHardwareAccelerationValueName = "DisableHWAcceleration";

    private readonly object _syncRoot = new();
    private readonly uint[] _adapterErrorCounts;

    internal Direct3D9RegistryDatabase(uint adapterCount, Func<Direct3D9RegistryValue> disableHardwareAccelerationReader)
    {
        ArgumentNullException.ThrowIfNull(disableHardwareAccelerationReader);

        _adapterErrorCounts = new uint[adapterCount];
        Direct3D9RegistryValue setting = disableHardwareAccelerationReader();
        bool adaptersEnabled = !setting.WasRead
            || (setting.Kind == RegistryValueKind.DWord && setting.DwordValue == 0);
        if (!adaptersEnabled)
        {
            Array.Fill(_adapterErrorCounts, MaximumErrorCount);
        }
    }

    internal static Direct3D9RegistryDatabase Create(uint adapterCount)
    {
        return new Direct3D9RegistryDatabase(adapterCount, ReadDisableHardwareAcceleration);
    }

    internal bool IsAdapterEnabled(uint adapterOrdinal)
    {
        lock (_syncRoot)
        {
            ValidateAdapterOrdinal(adapterOrdinal);
            return _adapterErrorCounts[adapterOrdinal] < MaximumErrorCount;
        }
    }

    internal void DisableAdapter(uint adapterOrdinal)
    {
        lock (_syncRoot)
        {
            ValidateAdapterOrdinal(adapterOrdinal);
            _adapterErrorCounts[adapterOrdinal] = MaximumErrorCount;
        }
    }

    internal void HandleAdapterUnexpectedError(uint adapterOrdinal)
    {
        lock (_syncRoot)
        {
            ValidateAdapterOrdinal(adapterOrdinal);
            if (_adapterErrorCounts[adapterOrdinal] < MaximumErrorCount)
            {
                _adapterErrorCounts[adapterOrdinal]++;
            }
        }
    }

    private void ValidateAdapterOrdinal(uint adapterOrdinal)
    {
        if (adapterOrdinal >= _adapterErrorCounts.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(adapterOrdinal));
        }
    }

    private static Direct3D9RegistryValue ReadDisableHardwareAcceleration()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(AvalonGraphicsKeyPath);
            if (key is null || !key.GetValueNames().Contains(DisableHardwareAccelerationValueName, StringComparer.OrdinalIgnoreCase))
            {
                return default;
            }

            RegistryValueKind kind = key.GetValueKind(DisableHardwareAccelerationValueName);
            object? value = key.GetValue(DisableHardwareAccelerationValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            uint dwordValue = kind == RegistryValueKind.DWord && value is int intValue
                ? unchecked((uint) intValue)
                : 0;
            return new Direct3D9RegistryValue(WasRead: true, kind, dwordValue);
        }
        catch (Exception exception) when (exception is IOException or SecurityException or UnauthorizedAccessException)
        {
            return default;
        }
    }
}
