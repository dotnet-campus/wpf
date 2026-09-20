using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9AdapterSnapshot(
    uint DisplayIndex,
    long Direct3DAdapterLuid,
    nint MonitorHandle,
    ImmutableArray<Direct3D9DisplayBounds> Bounds,
    string DeviceName,
    uint StateFlags);

internal readonly record struct Direct3D9AdapterArrangementResult(
    uint Direct3DAdapterCount,
    ImmutableArray<Direct3D9AdapterSnapshot> Displays);

internal delegate int Direct3D9AdapterLuidProvider(uint adapterOrdinal, out long adapterLuid);

internal static class Direct3D9AdapterSnapshotFactory
{
    private static long _lastLocallyUniqueLuid;

    internal static Direct3D9AdapterArrangementResult Arrange(
        Direct3D9DisplayDeviceEnumerationResult displayDevices,
        ImmutableArray<Direct3D9MonitorSnapshot> monitors,
        bool isMultiAdapterCodeEnabled,
        Func<uint> adapterCountProvider,
        Func<uint, nint> adapterMonitorProvider,
        Direct3D9AdapterLuidProvider adapterLuidProvider)
    {
        ArgumentNullException.ThrowIfNull(adapterCountProvider);
        ArgumentNullException.ThrowIfNull(adapterMonitorProvider);
        ArgumentNullException.ThrowIfNull(adapterLuidProvider);

        if (displayDevices.Devices.Length != monitors.Length)
        {
            ThrowDisplayStateInvalid();
        }

        Direct3D9AdapterSnapshot[] displays = new Direct3D9AdapterSnapshot[displayDevices.Devices.Length];
        for (int index = 0; index < displays.Length; index++)
        {
            Direct3D9DisplayDeviceSnapshot device = displayDevices.Devices[index];
            Direct3D9MonitorSnapshot monitor = monitors[index];
            if (!string.Equals(device.DeviceName, monitor.DeviceName, StringComparison.Ordinal))
            {
                ThrowDisplayStateInvalid();
            }

            displays[index] = new Direct3D9AdapterSnapshot(
                (uint) index,
                0,
                monitor.MonitorHandle,
                monitor.Bounds,
                device.DeviceName,
                device.StateFlags);
        }

        uint adapterCount = adapterCountProvider();
        if (!isMultiAdapterCodeEnabled && adapterCount > 1)
        {
            adapterCount = 1;
        }

        if (adapterCount > displays.Length)
        {
            ThrowDisplayStateInvalid();
        }

        for (uint adapterOrdinal = 0; adapterOrdinal < adapterCount; adapterOrdinal++)
        {
            nint monitorHandle = adapterMonitorProvider(adapterOrdinal);
            if (!isMultiAdapterCodeEnabled && displays.Length > 0)
            {
                monitorHandle = displays[0].MonitorHandle;
            }

            if (monitorHandle == 0)
            {
                continue;
            }

            int displayIndex = FindDisplay(displays, monitorHandle);
            if (displayIndex < (int) adapterOrdinal)
            {
                ThrowDisplayStateInvalid();
            }

            if (displayIndex != (int) adapterOrdinal)
            {
                (displays[adapterOrdinal], displays[displayIndex]) =
                    (displays[displayIndex], displays[adapterOrdinal]);
                displays[adapterOrdinal] = displays[adapterOrdinal] with { DisplayIndex = adapterOrdinal };
                displays[displayIndex] = displays[displayIndex] with { DisplayIndex = (uint) displayIndex };
            }

            _ = adapterLuidProvider(adapterOrdinal, out long adapterLuid);
            displays[adapterOrdinal] = displays[adapterOrdinal] with { Direct3DAdapterLuid = adapterLuid };
        }

        return new Direct3D9AdapterArrangementResult(adapterCount, displays.ToImmutableArray());
    }

    internal static Direct3D9AdapterLuidProvider CreateLocallyUniqueLuidProvider()
    {
        return (uint _, out long adapterLuid) =>
        {
            adapterLuid = Interlocked.Increment(ref _lastLocallyUniqueLuid);
            return 0;
        };
    }

    private static int FindDisplay(Direct3D9AdapterSnapshot[] displays, nint monitorHandle)
    {
        for (int index = 0; index < displays.Length; index++)
        {
            if (displays[index].MonitorHandle == monitorHandle)
            {
                return index;
            }
        }

        ThrowDisplayStateInvalid();
        return -1;
    }

    private static void ThrowDisplayStateInvalid()
    {
        Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
    }
}
