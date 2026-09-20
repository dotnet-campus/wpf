using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal sealed unsafe class Direct3D9DisplaySetInitialization : IDisposable
{
    private IDisposable? _owner;
    private DirectWriteFactoryCache? _directWriteFactoryCache;

    internal Direct3D9DisplaySetInitialization(
        int hResult,
        Direct3D9Objects? objects,
        IDisposable? owner = null,
        DirectWriteFactoryCache? directWriteFactoryCache = null)
    {
        HResult = hResult;
        Objects = objects;
        _owner = owner ?? objects;
        _directWriteFactoryCache = directWriteFactoryCache;
    }

    internal int HResult { get; }

    internal Direct3D9Objects? Objects { get; }

    internal bool HasDirect3D9Ex => Objects?.Direct3DEx is not null;

    internal Direct3D9DisplayDeviceEnumerationResult DisplayDevices { get; private set; }

    internal ImmutableArray<Direct3D9MonitorSnapshot> Monitors { get; private set; }

    internal Direct3D9AdapterArrangementResult Adapters { get; private set; }

    internal ImmutableArray<Direct3D9DisplayModeSnapshot> DisplayModes { get; private set; }

    internal Direct3D9DisplaySettingsSnapshotResult DisplaySettings { get; private set; }

    internal nint GetDirectWriteFactoryNoRef()
    {
        ObjectDisposedException.ThrowIf(_owner is null, this);
        DirectWriteFactoryCache cache = _directWriteFactoryCache ??= new DirectWriteFactoryCache();
        return cache.GetFactoryNoRef();
    }

    internal void SetDisplayDevices(Direct3D9DisplayDeviceEnumerationResult displayDevices)
    {
        DisplayDevices = displayDevices;
    }

    internal void SetMonitors(ImmutableArray<Direct3D9MonitorSnapshot> monitors)
    {
        Monitors = monitors;
    }

    internal void SetAdapters(Direct3D9AdapterArrangementResult adapters)
    {
        Adapters = adapters;
    }

    internal void SetDisplayModes(ImmutableArray<Direct3D9DisplayModeSnapshot> displayModes)
    {
        DisplayModes = displayModes;
    }

    internal void SetDisplaySettings(Direct3D9DisplaySettingsSnapshotResult displaySettings)
    {
        DisplaySettings = displaySettings;
    }

    internal static Direct3D9DisplaySetInitialization Create()
    {
        try
        {
            Direct3D9Objects objects = Direct3D9Factory.Create();
            return new Direct3D9DisplaySetInitialization(0, objects);
        }
        catch (COMException exception)
        {
            return new Direct3D9DisplaySetInitialization(exception.HResult, null);
        }
    }

    public void Dispose()
    {
        _directWriteFactoryCache?.Dispose();
        _directWriteFactoryCache = null;
        _owner?.Dispose();
        _owner = null;
    }
}

internal static class Direct3D9DisplaySetFactory
{

    internal static Direct3D9DisplaySet Create(
        uint displayUniqueness,
        uint externalUpdateCount,
        Func<uint> displayUniquenessProvider,
        Func<uint> externalUpdateCountProvider,
        Func<Direct3D9DisplaySetInitialization> direct3DInitializationFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9DisplaySetCharacteristics> characteristicsFactory)
    {
        return Create(
            displayUniqueness,
            externalUpdateCount,
            displayUniquenessProvider,
            externalUpdateCountProvider,
            direct3DInitializationFactory,
            _ => default,
            _ => ImmutableArray<Direct3D9MonitorSnapshot>.Empty,
            characteristicsFactory);
    }

    internal static Direct3D9DisplaySet Create(
        uint displayUniqueness,
        uint externalUpdateCount,
        Func<uint> displayUniquenessProvider,
        Func<uint> externalUpdateCountProvider,
        Func<Direct3D9DisplaySetInitialization> direct3DInitializationFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9DisplayDeviceEnumerationResult> displayDeviceSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, ImmutableArray<Direct3D9MonitorSnapshot>> monitorSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9DisplaySetCharacteristics> characteristicsFactory)
    {
        return Create(
            displayUniqueness,
            externalUpdateCount,
            displayUniquenessProvider,
            externalUpdateCountProvider,
            direct3DInitializationFactory,
            displayDeviceSnapshotFactory,
            monitorSnapshotFactory,
            _ => default,
            _ => [],
            characteristicsFactory);
    }

    internal static Direct3D9DisplaySet Create(
        uint displayUniqueness,
        uint externalUpdateCount,
        Func<uint> displayUniquenessProvider,
        Func<uint> externalUpdateCountProvider,
        Func<Direct3D9DisplaySetInitialization> direct3DInitializationFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9DisplayDeviceEnumerationResult> displayDeviceSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, ImmutableArray<Direct3D9MonitorSnapshot>> monitorSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9AdapterArrangementResult> adapterSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9DisplaySetCharacteristics> characteristicsFactory)
    {
        return Create(
            displayUniqueness,
            externalUpdateCount,
            displayUniquenessProvider,
            externalUpdateCountProvider,
            direct3DInitializationFactory,
            displayDeviceSnapshotFactory,
            monitorSnapshotFactory,
            adapterSnapshotFactory,
            _ => [],
            characteristicsFactory);
    }

    internal static Direct3D9DisplaySet Create(
        uint displayUniqueness,
        uint externalUpdateCount,
        Func<uint> displayUniquenessProvider,
        Func<uint> externalUpdateCountProvider,
        Func<Direct3D9DisplaySetInitialization> direct3DInitializationFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9DisplayDeviceEnumerationResult> displayDeviceSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, ImmutableArray<Direct3D9MonitorSnapshot>> monitorSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9AdapterArrangementResult> adapterSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, ImmutableArray<Direct3D9DisplayModeSnapshot>> displayModeSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9DisplaySetCharacteristics> characteristicsFactory)
    {
        return Create(
            displayUniqueness,
            externalUpdateCount,
            displayUniquenessProvider,
            externalUpdateCountProvider,
            direct3DInitializationFactory,
            displayDeviceSnapshotFactory,
            monitorSnapshotFactory,
            adapterSnapshotFactory,
            displayModeSnapshotFactory,
            _ => default,
            characteristicsFactory);
    }

    internal static Direct3D9DisplaySet Create(
        uint displayUniqueness,
        uint externalUpdateCount,
        Func<uint> displayUniquenessProvider,
        Func<uint> externalUpdateCountProvider,
        Func<Direct3D9DisplaySetInitialization> direct3DInitializationFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9DisplayDeviceEnumerationResult> displayDeviceSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, ImmutableArray<Direct3D9MonitorSnapshot>> monitorSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9AdapterArrangementResult> adapterSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, ImmutableArray<Direct3D9DisplayModeSnapshot>> displayModeSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9DisplaySettingsSnapshotResult> displaySettingsSnapshotFactory,
        Func<Direct3D9DisplaySetInitialization, Direct3D9DisplaySetCharacteristics> characteristicsFactory)
    {
        ArgumentNullException.ThrowIfNull(displayUniquenessProvider);
        ArgumentNullException.ThrowIfNull(externalUpdateCountProvider);
        ArgumentNullException.ThrowIfNull(direct3DInitializationFactory);
        ArgumentNullException.ThrowIfNull(displayDeviceSnapshotFactory);
        ArgumentNullException.ThrowIfNull(monitorSnapshotFactory);
        ArgumentNullException.ThrowIfNull(adapterSnapshotFactory);
        ArgumentNullException.ThrowIfNull(displayModeSnapshotFactory);
        ArgumentNullException.ThrowIfNull(displaySettingsSnapshotFactory);
        ArgumentNullException.ThrowIfNull(characteristicsFactory);

        Direct3D9DisplaySetInitialization initialization = direct3DInitializationFactory()
            ?? throw new InvalidOperationException("The Direct3D initialization factory returned null.");
        try
        {
            initialization.SetDisplayDevices(displayDeviceSnapshotFactory(initialization));
            initialization.SetMonitors(monitorSnapshotFactory(initialization));
            initialization.SetAdapters(adapterSnapshotFactory(initialization));
            initialization.SetDisplayModes(displayModeSnapshotFactory(initialization));
            initialization.SetDisplaySettings(displaySettingsSnapshotFactory(initialization));
            Direct3D9DisplaySetCharacteristics characteristics = characteristicsFactory(initialization);
            Direct3D9DisplaySet displaySet = new(
                characteristics,
                displayUniqueness,
                externalUpdateCount,
                displayUniquenessProvider,
                externalUpdateCountProvider,
                initialization,
                initialization.HasDirect3D9Ex);
            initialization = null!;
            return displaySet;
        }
        finally
        {
            initialization?.Dispose();
        }
    }

    [SupportedOSPlatform("windows5.1.2600")]
    internal static Direct3D9DisplaySet Create(
        uint displayUniqueness,
        uint externalUpdateCount,
        Func<uint> displayUniquenessProvider,
        Func<uint> externalUpdateCountProvider)
    {
        return Create(
            displayUniqueness,
            externalUpdateCount,
            displayUniquenessProvider,
            externalUpdateCountProvider,
            CreateCharacteristics);
    }

    [SupportedOSPlatform("windows5.1.2600")]
    internal static Direct3D9DisplaySet Create(
        uint displayUniqueness,
        uint externalUpdateCount,
        Func<uint> displayUniquenessProvider,
        Func<uint> externalUpdateCountProvider,
        Func<Direct3D9DisplaySetInitialization, Direct3D9DisplaySetCharacteristics> characteristicsFactory)
    {
        return Create(
            displayUniqueness,
            externalUpdateCount,
            displayUniquenessProvider,
            externalUpdateCountProvider,
            Direct3D9DisplaySetInitialization.Create,
            _ => Direct3D9DisplayDeviceSnapshotFactory.EnumerateSystem(isMultiAdapterCodeEnabled: true),
            initialization => Direct3D9MonitorSnapshotFactory.EnumerateSystem(initialization.DisplayDevices),
            ArrangeSystemAdapters,
            ReadSystemDisplayModes,
            ReadSystemDisplaySettings,
            characteristicsFactory);
    }

    internal static Direct3D9DisplaySetCharacteristics CreateCharacteristics(
        Direct3D9DisplaySetInitialization initialization)
    {
        ArgumentNullException.ThrowIfNull(initialization);

        ImmutableArray<Direct3D9AdapterSnapshot> adapters = initialization.Adapters.Displays;
        ImmutableArray<Direct3D9DisplayModeSnapshot> displayModes = initialization.DisplayModes;
        ImmutableArray<Direct3D9CompiledDisplaySettings> displaySettings = initialization.DisplaySettings.Displays;
        if (adapters.Length != displayModes.Length || adapters.Length != displaySettings.Length)
        {
            Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
        }

        ulong requiredVideoDriverDate = Direct3D9DisplayDriverSnapshotFactory.ReadRequiredVideoDriverDate();
        ImmutableArray<Direct3D9DisplayDriverSnapshot> driverSnapshots =
            ReadDisplayDriverSnapshots(initialization);
        ImmutableArray<Direct3D9GraphicsAccelerationSnapshot> graphicsAccelerationSnapshots =
            ReadGraphicsAccelerationCaps(initialization, requiredVideoDriverDate, driverSnapshots);
        ImmutableArray<Direct3D9Display>.Builder displays =
            ImmutableArray.CreateBuilder<Direct3D9Display>(adapters.Length);
        for (int index = 0; index < adapters.Length; index++)
        {
            Direct3D9AdapterSnapshot adapter = adapters[index];
            Direct3D9DisplayModeSnapshot mode = displayModes[index];
            if (adapter.DisplayIndex != (uint) index || mode.DisplayIndex != adapter.DisplayIndex)
            {
                Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
            }

            Direct3D9DisplayDriverSnapshot driver = driverSnapshots[index];
            Direct3D9GraphicsAccelerationSnapshot graphicsAcceleration = graphicsAccelerationSnapshots[index];
            displays.Add(new Direct3D9Display(
                adapter.DisplayIndex,
                adapter.Direct3DAdapterLuid,
                adapter.MonitorHandle,
                adapter.Bounds,
                adapter.DeviceName,
                adapter.StateFlags,
                displaySettings[index].Settings,
                driver.MemorySize,
                graphicsAcceleration.IsRecentDriver,
                driver.IsBadDriver,
                driver.GraphicsCardVendorId,
                driver.GraphicsCardDeviceId,
                mode.DisplayMode,
                mode.DisplayRotation,
                graphicsAcceleration.Capabilities));
        }

        return new Direct3D9DisplaySetCharacteristics(
            requiredVideoDriverDate,
            initialization.Adapters.Direct3DAdapterCount,
            initialization.DisplayDevices.IsNonLocalDevicePresent,
            ComputeDisplayBounds(adapters),
            displays.MoveToImmutable());
    }

    private static ImmutableArray<Direct3D9DisplayBounds> ComputeDisplayBounds(
        ImmutableArray<Direct3D9AdapterSnapshot> adapters)
    {
        List<Direct3D9DisplayBounds> result = [];
        foreach (Direct3D9AdapterSnapshot adapter in adapters)
        {
            foreach (Direct3D9DisplayBounds bounds in adapter.Bounds)
            {
                int existingIndex = result.FindIndex(
                    candidate => candidate.DpiAwarenessContextValue == bounds.DpiAwarenessContextValue);
                if (existingIndex < 0)
                {
                    result.Add(bounds);
                    continue;
                }

                Direct3D9SurfaceRect existing = result[existingIndex].Bounds;
                Direct3D9SurfaceRect current = bounds.Bounds;
                result[existingIndex] = bounds with
                {
                    Bounds = new Direct3D9SurfaceRect(
                        Math.Min(existing.Left, current.Left),
                        Math.Min(existing.Top, current.Top),
                        Math.Max(existing.Right, current.Right),
                        Math.Max(existing.Bottom, current.Bottom))
                };
            }
        }

        return [.. result];
    }

    private static ImmutableArray<Direct3D9DisplayDriverSnapshot> ReadDisplayDriverSnapshots(
        Direct3D9DisplaySetInitialization initialization)
    {
        ImmutableArray<Direct3D9AdapterSnapshot> adapters = initialization.Adapters.Displays;
        ImmutableArray<Direct3D9DisplayDeviceSnapshot> devices = initialization.DisplayDevices.Devices;
        ImmutableArray<Direct3D9DisplayDriverSnapshot>.Builder result =
            ImmutableArray.CreateBuilder<Direct3D9DisplayDriverSnapshot>(adapters.Length);
        Direct3D9Objects? objects = initialization.Objects;
        foreach (Direct3D9AdapterSnapshot adapter in adapters)
        {
            Direct3D9DisplayDeviceSnapshot device = devices.First(
                candidate => string.Equals(candidate.DeviceName, adapter.DeviceName, StringComparison.Ordinal));
            Direct3D9DisplayDriverSnapshot driver =
                Direct3D9DisplayDriverSnapshotFactory.ReadDeviceRegistry(device.DeviceKey);
            if (objects is not null && adapter.DisplayIndex < initialization.Adapters.Direct3DAdapterCount)
            {
                int identifierHResult = objects.GetAdapterIdentifier(adapter.DisplayIndex, out AdapterIdentifier9 identifier);
                bool isBadDriver = identifierHResult < 0
                    || (identifier.VendorId == 0x8086 && identifier.DeviceId == 0x2562);
                driver = driver with
                {
                    IsBadDriver = isBadDriver,
                    GraphicsCardVendorId = identifier.VendorId,
                    GraphicsCardDeviceId = identifier.DeviceId,
                };
            }

            result.Add(driver);
        }

        return result.MoveToImmutable();
    }

    private static ImmutableArray<Direct3D9GraphicsAccelerationSnapshot> ReadGraphicsAccelerationCaps(
        Direct3D9DisplaySetInitialization initialization,
        ulong requiredVideoDriverDate,
        ImmutableArray<Direct3D9DisplayDriverSnapshot> driverSnapshots)
    {
        ImmutableArray<Direct3D9DisplayModeSnapshot> displayModes = initialization.DisplayModes;
        ImmutableArray<Direct3D9GraphicsAccelerationSnapshot>.Builder result =
            ImmutableArray.CreateBuilder<Direct3D9GraphicsAccelerationSnapshot>(displayModes.Length);
        Direct3D9Objects? objects = initialization.Objects;
        for (int index = 0; index < displayModes.Length; index++)
        {
            Direct3D9DisplayModeSnapshot mode = displayModes[index];
            Direct3D9DisplayDriverSnapshot driver = driverSnapshots[index];
            if (objects is null || mode.DisplayIndex >= initialization.Adapters.Direct3DAdapterCount)
            {
                result.Add(new Direct3D9GraphicsAccelerationSnapshot(
                    false,
                    Direct3D9HardwareCapabilities.CreateNoHardwareAccelerationCaps(mode.BitsPerPixel)));
                continue;
            }

            if (driver.IsBadDriver)
            {
                result.Add(new Direct3D9GraphicsAccelerationSnapshot(
                    false,
                    Direct3D9HardwareCapabilities.CreateNoHardwareAccelerationCaps(mode.BitsPerPixel)));
                continue;
            }

            int checkDisplayFormatHResult = objects.CheckWindowedDisplayFormat(
                mode.DisplayIndex,
                mode.DisplayMode.Format);
            Caps9 capabilities = default;
            int getDeviceCapsHResult = checkDisplayFormatHResult < 0
                ? Direct3D9Factory.GenericFailureHResult
                : objects.GetDeviceCaps(mode.DisplayIndex, Devtype.Hal, out capabilities);
            bool isRecentDriver = getDeviceCapsHResult >= 0
                && (Direct3D9HardwareCapabilities.HasWddmSupport(capabilities)
                    || Direct3D9DisplayDriverSnapshotFactory.CheckForRecentDriver(
                        driver.InstalledDisplayDriver,
                        requiredVideoDriverDate));
            result.Add(Direct3D9HardwareCapabilities.ReadGraphicsAccelerationCaps(
                mode.BitsPerPixel,
                driver.MemorySize,
                isRecentDriver,
                driver.IsBadDriver,
                checkDisplayFormatHResult,
                getDeviceCapsHResult,
                capabilities));
        }

        return result.MoveToImmutable();
    }

    private static Direct3D9AdapterArrangementResult ArrangeSystemAdapters(
        Direct3D9DisplaySetInitialization initialization)
    {
        Direct3D9Objects? objects = initialization.Objects;
        if (objects is null)
        {
            return Direct3D9AdapterSnapshotFactory.Arrange(
                initialization.DisplayDevices,
                initialization.Monitors,
                isMultiAdapterCodeEnabled: true,
                () => 0,
                _ => 0,
                (uint _, out long adapterLuid) =>
                {
                    adapterLuid = 0;
                    return 0;
                });
        }

        Direct3D9AdapterLuidProvider luidProvider = initialization.HasDirect3D9Ex
            ? objects.GetAdapterLuid
            : Direct3D9AdapterSnapshotFactory.CreateLocallyUniqueLuidProvider();
        return Direct3D9AdapterSnapshotFactory.Arrange(
            initialization.DisplayDevices,
            initialization.Monitors,
            isMultiAdapterCodeEnabled: true,
            objects.GetAdapterCount,
            objects.GetAdapterMonitor,
            luidProvider);
    }

    [SupportedOSPlatform("windows5.1.2600")]
    private static Direct3D9DisplaySettingsSnapshotResult ReadSystemDisplaySettings(
        Direct3D9DisplaySetInitialization initialization)
    {
        return DirectWriteDisplaySettingsReader.ReadSystem(
            initialization.Adapters,
            initialization.DisplayModes,
            initialization.GetDirectWriteFactoryNoRef());
    }

    [SupportedOSPlatform("windows5.1.2600")]
    private static ImmutableArray<Direct3D9DisplayModeSnapshot> ReadSystemDisplayModes(
        Direct3D9DisplaySetInitialization initialization)
    {
        Direct3D9Objects? objects = initialization.Objects;
        return Direct3D9DisplayModeSnapshotFactory.Read(
            initialization.Adapters,
            objects is not null && !initialization.HasDirect3D9Ex ? objects.GetAdapterDisplayMode : null,
            objects is not null && initialization.HasDirect3D9Ex ? objects.GetAdapterDisplayModeEx : null,
            Direct3D9DisplayModeSnapshotFactory.ReadSystemGdiMode);
    }
}
