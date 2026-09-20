using Silk.NET.Direct3D9;
using Windows.Win32;

namespace WpfGfxShape.Core;

[Flags]
internal enum Direct3D9RenderTargetInitializationFlags : uint
{
    None = 0,
    SoftwareOnly = 0x00000001,
    PresentImmediately = 0x00000004,
    PresentRetainContents = 0x00000008,
    NeedDestinationAlpha = 0x00000040,
    SingleThreadedUsage = 0x00000100,
    DisableDisplayClipping = 0x00001000,
    ForceCompatible = 0x00002000,
    DisableDirtyRectangles = 0x00010000,
    PresentUsingMask = 0xC0000000,
    PresentUsingHal = 0x00000000
}

internal readonly record struct Direct3D9DeviceRequest(
    nint FocusWindow,
    Direct3D9RenderTargetInitializationFlags InitializationFlags,
    uint? DisplayAdapterOrdinal,
    Devtype DeviceType,
    Direct3D9DisplaySet? DisplaySet = null);

internal readonly record struct Direct3D9DeviceCreationParameters(
    uint AdapterOrdinal,
    Devtype DeviceType,
    uint BehaviorFlags,
    PresentParameters PresentParameters,
    nint FocusWindow = 0,
    uint AdapterOrdinalInGroup = 0,
    uint NumberOfAdaptersInGroup = 1,
    Displaymode DisplayMode = default,
    Caps9 Capabilities = default,
    bool UseExtendedDeviceCreate = true,
    bool RetryWithoutDriverManagementEx = true,
    long AdapterLuid = 0);

internal readonly record struct Direct3D9DeviceAndPresentParameters(
    Direct3D9Device Device,
    PresentParameters PresentParameters,
    uint AdapterOrdinalInGroup,
    Displaymode DisplayMode);

internal delegate int Direct3D9GetDeviceCaps(uint adapterOrdinal, Devtype deviceType, out Caps9 capabilities);

internal sealed class Direct3D9DeviceManager : IDisposable
{
    private readonly object _syncRoot = new();
    private readonly Func<Direct3D9DeviceCreationParameters, Action<Direct3D9Device>, Action<Direct3D9Device>, Direct3D9Device> _deviceFactory;
    private readonly Func<uint>? _adapterCountProvider;
    private readonly Func<uint, Displaymode>? _displayModeProvider;
    private readonly Func<uint, Devtype, Format, Format, int>? _checkDeviceType;
    private readonly Func<uint, bool>? _isAdapterEnabled;
    private readonly Action<uint>? _disableAdapter;
    private readonly Action<uint>? _handleUnexpectedAdapterError;
    private readonly Func<Direct3D9Device, int>? _testLevel1Device;
    private readonly Func<uint, Devtype, Format, uint, Resourcetype, Format, int>? _checkDeviceFormat;
    private readonly Func<uint?>? _maximumMultisampleTypeProvider;
    private readonly Func<uint, Devtype, Format, bool, MultisampleType, int>? _checkDeviceMultisampleType;
    private readonly Func<Direct3D9Device, int>? _initializeDynamicBuffers;
    private readonly Func<Direct3D9Device, int>? _initializeRenderState;
    private readonly Func<Direct3D9Device, int>? _initializeTextPixelShaders;
    private readonly Action? _ensureSoftwareRasterizerRegistered;
    private readonly Func<Direct3D9DisplaySet>? _latestDisplaySetProvider;
    private readonly Func<uint, int>? _adapterDisplayModeProbe;
    private readonly Func<nint, bool>? _isWindow;
    private readonly Direct3D9GetDeviceCaps? _getDeviceCaps;
    private readonly Func<Direct3D9Device, Format, int>? _checkRenderTargetFormat;
    private readonly Func<bool>? _isHardwareTextDisabled;
    private readonly List<Direct3D9Device> _usableDevices = [];
    private readonly List<Direct3D9Device> _unusableDevices = [];
    private readonly HashSet<Direct3D9Device> _lostDevices = [];
    private readonly List<Action<uint, bool>> _adapterStatusListeners = [];
    private Direct3D9DisplaySet _latestDisplaySet;
    private Direct3D9DisplaySet? _displaySet;
    private Direct3D9DisplaySet? _nextDisplaySet;
    private Direct3D9Device? _softwareDevice;
    private bool _isDisposed;

    internal Direct3D9DeviceManager(
        Func<Direct3D9DeviceCreationParameters, Action<Direct3D9Device>, Action<Direct3D9Device>, Direct3D9Device> deviceFactory,
        Func<uint>? adapterCountProvider = null,
        Func<uint, Displaymode>? displayModeProvider = null,
        Func<uint, Devtype, Format, Format, int>? checkDeviceType = null,
        Func<uint, bool>? isAdapterEnabled = null,
        Action<uint>? disableAdapter = null,
        Action<uint>? handleUnexpectedAdapterError = null,
        Func<Direct3D9Device, int>? testLevel1Device = null,
        Func<uint, Devtype, Format, uint, Resourcetype, Format, int>? checkDeviceFormat = null,
        Func<uint?>? maximumMultisampleTypeProvider = null,
        Func<uint, Devtype, Format, bool, MultisampleType, int>? checkDeviceMultisampleType = null,
        Func<Direct3D9Device, int>? initializeDynamicBuffers = null,
        Func<Direct3D9Device, int>? initializeRenderState = null,
        Func<Direct3D9Device, int>? initializeTextPixelShaders = null,
        Action? ensureSoftwareRasterizerRegistered = null,
        Func<bool>? hasDisplayStateChanged = null,
        Func<Direct3D9DisplaySet>? latestDisplaySetProvider = null,
        Func<uint, int>? adapterDisplayModeProbe = null,
        Func<nint, bool>? isWindow = null,
        Direct3D9GetDeviceCaps? getDeviceCaps = null,
        Func<Direct3D9Device, Format, int>? checkRenderTargetFormat = null,
        Func<bool>? isHardwareTextDisabled = null)
    {
        ArgumentNullException.ThrowIfNull(deviceFactory);
        _deviceFactory = deviceFactory;
        _adapterCountProvider = adapterCountProvider;
        _displayModeProvider = displayModeProvider;
        _checkDeviceType = checkDeviceType;
        _isAdapterEnabled = isAdapterEnabled;
        _disableAdapter = disableAdapter;
        _handleUnexpectedAdapterError = handleUnexpectedAdapterError;
        _testLevel1Device = testLevel1Device;
        _checkDeviceFormat = checkDeviceFormat;
        _maximumMultisampleTypeProvider = maximumMultisampleTypeProvider;
        _checkDeviceMultisampleType = checkDeviceMultisampleType;
        _initializeDynamicBuffers = initializeDynamicBuffers;
        _initializeRenderState = initializeRenderState;
        _initializeTextPixelShaders = initializeTextPixelShaders;
        _ensureSoftwareRasterizerRegistered = ensureSoftwareRasterizerRegistered;
        _latestDisplaySetProvider = latestDisplaySetProvider;
        _adapterDisplayModeProbe = adapterDisplayModeProbe;
        _isWindow = isWindow;
        _getDeviceCaps = getDeviceCaps;
        _checkRenderTargetFormat = checkRenderTargetFormat;
        _isHardwareTextDisabled = isHardwareTextDisabled;
        _latestDisplaySet = new Direct3D9DisplaySet(hasDisplayStateChanged);
    }

    internal int UsableDeviceCount
    {
        get
        {
            lock (_syncRoot)
            {
                return _usableDevices.Count;
            }
        }
    }

    internal int UnusableDeviceCount
    {
        get
        {
            lock (_syncRoot)
            {
                return _unusableDevices.Count;
            }
        }
    }

    internal Direct3D9Device CreateDevice(Direct3D9DeviceCreationParameters creationParameters)
    {
        Direct3D9Device device = CreateInitializedDevice(creationParameters, notifyAdapterValid: true);
        try
        {
            CheckBackBufferRenderTargetFormat(device, creationParameters.PresentParameters.BackBufferFormat);
            lock (_syncRoot)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, this);
                _usableDevices.Add(device);
            }

            return device;
        }
        catch
        {
            device.Dispose();
            throw;
        }
    }

    internal Direct3D9DeviceAndPresentParameters GetDeviceAndPresentParameters(
        Direct3D9DeviceRequest request,
        Caps9 capabilities)
    {
        Direct3D9Device? device = null;
        bool deviceCreated = false;
        try
        {
            InitializeDisplaySet(request.DisplaySet);

            if (request.DisplayAdapterOrdinal is null && request.DeviceType != Devtype.SW)
            {
                throw new ArgumentException("A display adapter is required for hardware Direct3D devices.", nameof(request));
            }

            uint adapterOrdinal = request.DisplayAdapterOrdinal ?? D3D9.AdapterDefault;
            if (request.DeviceType == Devtype.Hal)
            {
                if (_adapterCountProvider is not null && adapterOrdinal >= _adapterCountProvider())
                {
                    System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.NoHardwareDeviceHResult);
                }

                if (_isAdapterEnabled?.Invoke(adapterOrdinal) == false)
                {
                    System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.NoHardwareDeviceHResult);
                }
            }
            else if (request.DeviceType == Devtype.SW)
            {
                _ensureSoftwareRasterizerRegistered?.Invoke();
            }

            ValidateFocusWindow(request.FocusWindow);
            Caps9 resolvedCapabilities = GetAdapterCapabilities(adapterOrdinal, request.DeviceType, capabilities);
            uint behaviorFlags = ComposeBehaviorFlags(request.InitializationFlags, request.DeviceType, resolvedCapabilities);
            Displaymode displayMode = GetDisplayMode(adapterOrdinal, request.DeviceType, request.InitializationFlags);
            PresentParameters presentParameters = ComposePresentParameters(request.FocusWindow, request.InitializationFlags);
            Direct3D9DeviceCreationParameters creationParameters = new(
                adapterOrdinal,
                request.DeviceType,
                behaviorFlags,
                presentParameters,
                request.FocusWindow,
                AdapterOrdinalInGroup: 0,
                NumberOfAdaptersInGroup: 1,
                displayMode,
                resolvedCapabilities,
                AdapterLuid: GetAdapterLuid(adapterOrdinal));
            device = GetOrCreateDevice(ref creationParameters, out deviceCreated);
            return new Direct3D9DeviceAndPresentParameters(
                device,
                presentParameters,
                creationParameters.AdapterOrdinalInGroup,
                displayMode);
        }
        finally
        {
            if (_displaySet?.DangerousHasDisplayStateChanged() == true)
            {
                if (deviceCreated)
                {
                    device?.Dispose();
                }

                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
            }
        }
    }

    internal Direct3D9Device GetOrCreateDevice(ref Direct3D9DeviceCreationParameters creationParameters)
    {
        return GetOrCreateDevice(ref creationParameters, out _);
    }

    internal Direct3D9Device GetSoftwareDevice()
    {
        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            try
            {
                InitializeDisplaySet(null);
                _ensureSoftwareRasterizerRegistered?.Invoke();
                if (_softwareDevice is not null)
                {
                    return _softwareDevice;
                }

                Direct3D9Device device = CreateInitializedDevice(ComposeSoftwareDeviceCreationParameters());
                _softwareDevice = device;
                return device;
            }
            catch (Exception exception) when (exception.HResult == Direct3D9Factory.DeviceLostHResult)
            {
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
                throw;
            }
        }
    }

    internal int CheckRenderTargetFormat(Direct3D9Device device, Format format)
    {
        ArgumentNullException.ThrowIfNull(device);
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        return _checkRenderTargetFormat is null
            ? device.CheckRenderTargetFormat(format, static _ => Direct3D9Factory.SuccessHResult, out _)
            : _checkRenderTargetFormat(device, format);
    }

    internal bool DoesWindowedHardwareDeviceExist(uint adapterOrdinal)
    {
        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            Caps9? queriedCapabilities = null;
            if (_getDeviceCaps is not null)
            {
                int result = _getDeviceCaps(adapterOrdinal, Devtype.Hal, out Caps9 capabilities);
                if (result < 0)
                {
                    return false;
                }

                queriedCapabilities = capabilities;
            }

            foreach (Direct3D9Device device in _usableDevices)
            {
                if (device.DeviceType != Devtype.Hal || device.AdapterOrdinal != adapterOrdinal)
                {
                    continue;
                }

                uint behaviorFlags = ComposeBehaviorFlags(
                    Direct3D9RenderTargetInitializationFlags.None,
                    Devtype.Hal,
                    queriedCapabilities ?? device.Capabilities);
                uint behaviorFlagDifferences = behaviorFlags ^ device.BehaviorFlags;
                behaviorFlagDifferences &= unchecked((uint) ~D3D9.CreateDisableDriverManagementEX);
                if (behaviorFlagDifferences == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private Direct3D9Device GetOrCreateDevice(
        ref Direct3D9DeviceCreationParameters creationParameters,
        out bool deviceCreated)
    {
        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            Direct3D9Device? device = FindUsableDevice(creationParameters);
            deviceCreated = device is null;
            if (deviceCreated)
            {
                device = CreateDevice(creationParameters);
            }

            creationParameters = creationParameters with { BehaviorFlags = device.BehaviorFlags };
            return device;
        }
    }

    internal Direct3D9Device RecreateDevice(Direct3D9Device unusableDevice)
    {
        ArgumentNullException.ThrowIfNull(unusableDevice);

        Direct3D9DeviceCreationParameters creationParameters;
        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            if (!_unusableDevices.Contains(unusableDevice))
            {
                throw new InvalidOperationException("Only a tracked unusable Direct3D device can be recreated.");
            }

            creationParameters = new Direct3D9DeviceCreationParameters(
                unusableDevice.AdapterOrdinal,
                unusableDevice.DeviceType,
                unusableDevice.BehaviorFlags,
                unusableDevice.PresentParameters,
                FocusWindow: unusableDevice.FocusWindow,
                AdapterLuid: unusableDevice.AdapterLuid);
        }

        unusableDevice.Dispose();
        return CreateDevice(creationParameters);
    }

    internal void NotifyDisplayChange(Direct3D9DisplaySet oldDisplaySet, Direct3D9DisplaySet newDisplaySet)
    {
        ArgumentNullException.ThrowIfNull(oldDisplaySet);
        ArgumentNullException.ThrowIfNull(newDisplaySet);

        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            if (ReferenceEquals(oldDisplaySet, _latestDisplaySet))
            {
                _latestDisplaySet = newDisplaySet;
            }

            if (!ReferenceEquals(oldDisplaySet, _displaySet))
            {
                return;
            }

            while (_usableDevices.Count > 0)
            {
                Direct3D9Device device = _usableDevices[^1];
                using Direct3D9DeviceEntryGuard deviceEntry = new(device);
                device.MarkUnusable();
            }

            _softwareDevice?.MarkUnusable();

            _displaySet = null;
            _nextDisplaySet = newDisplaySet;
        }
    }

    internal void AddAdapterStatusListener(Action<uint, bool> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            _adapterStatusListeners.Add(listener);
        }
    }

    internal void RemoveAdapterStatusListener(Action<uint, bool> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        lock (_syncRoot)
        {
            _adapterStatusListeners.Remove(listener);
        }
    }

    private Direct3D9Device CreateInitializedDevice(
        Direct3D9DeviceCreationParameters creationParameters,
        bool notifyAdapterValid = false)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (creationParameters.UseExtendedDeviceCreate)
        {
            if (_displaySet?.DangerousHasDisplayStateChanged() == true)
            {
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
            }

            ProbeAdapterDisplayMode(creationParameters.AdapterOrdinal);
        }

        Direct3D9Device device;
        try
        {
            device = _deviceFactory(creationParameters, OnDeviceUnusable, OnDeviceDisposed);
            device.SetUnexpectedAdapterErrorNotification(_handleUnexpectedAdapterError);
        }
        catch (Exception exception) when (exception.HResult == Direct3D9Factory.DeviceLostHResult)
        {
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
            throw;
        }

        if (notifyAdapterValid)
        {
            NotifyAdapterStatus(device.AdapterOrdinal, isValid: true);
        }

        try
        {
            InitializeDeviceTier(device);
            ApplyDeviceDriverWorkarounds(device);
            GatherSupportedTextureFormats(device);
            device.InitializeTextRenderingPrerequisites(_isHardwareTextDisabled?.Invoke() == true);
            InitializeTextPixelShaders(device);
            GatherSupportedMultisampleTypes(device);
            InitializeDynamicBuffers(device);
            InitializeRenderState(device);
            TestLevel1Device(device);
            return device;
        }
        catch
        {
            device.Dispose();
            throw;
        }
    }

    private unsafe Direct3D9DeviceCreationParameters ComposeSoftwareDeviceCreationParameters()
    {
        PresentParameters presentParameters = new(
            backBufferWidth: 1,
            backBufferHeight: 1,
            backBufferFormat: Format.X8R8G8B8,
            backBufferCount: 1,
            swapEffect: Swapeffect.Discard,
            hDeviceWindow: 0,
            windowed: true,
            enableAutoDepthStencil: false,
            autoDepthStencilFormat: Format.Unknown);
        uint behaviorFlags = D3D9.CreateSoftwareVertexprocessing
            | D3D9.CreateMultithreaded
            | D3D9.CreateFpuPreserve
            | D3D9.CreateDisableDriverManagementEX;
        return new Direct3D9DeviceCreationParameters(
            unchecked((uint) D3D9.AdapterDefault),
            Devtype.SW,
            behaviorFlags,
            presentParameters,
            (nint) PInvoke.GetDesktopWindow().Value,
            UseExtendedDeviceCreate: false,
            RetryWithoutDriverManagementEx: false,
            AdapterLuid: GetAdapterLuid(unchecked((uint) D3D9.AdapterDefault)));
    }

    private void InitializeDisplaySet(Direct3D9DisplaySet? givenDisplaySet)
    {
        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            Direct3D9DisplaySet latestDisplaySet = _latestDisplaySetProvider?.Invoke() ?? _latestDisplaySet;
            if (givenDisplaySet is not null && !ReferenceEquals(givenDisplaySet, latestDisplaySet))
            {
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
            }

            if (_displaySet is not null && !ReferenceEquals(_displaySet, latestDisplaySet))
            {
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
            }

            _displaySet = latestDisplaySet;
            _nextDisplaySet = null;
        }
    }

    private void ValidateFocusWindow(nint focusWindow)
    {
        if (focusWindow == 0)
        {
            return;
        }

        bool isWindow = _isWindow?.Invoke(focusWindow)
            ?? PInvoke.IsWindow((Windows.Win32.Foundation.HWND) focusWindow);
        if (!isWindow)
        {
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.InvalidWindowHandleHResult);
        }
    }

    private Caps9 GetAdapterCapabilities(uint adapterOrdinal, Devtype deviceType, Caps9 fallback)
    {
        if (_getDeviceCaps is null)
        {
            return fallback;
        }

        int result = _getDeviceCaps(adapterOrdinal, deviceType, out Caps9 capabilities);
        System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(result);
        return capabilities;
    }

    private void ProbeAdapterDisplayMode(uint adapterOrdinal)
    {
        if (_adapterDisplayModeProbe is null)
        {
            return;
        }

        int result = _adapterDisplayModeProbe(adapterOrdinal);
        if (result >= 0)
        {
            return;
        }

        System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(
            Direct3D9Factory.IsOutOfMemory(result)
                ? result
                : Direct3D9Factory.DisplayStateInvalidHResult);
    }

    private static uint ComposeBehaviorFlags(
        Direct3D9RenderTargetInitializationFlags initializationFlags,
        Devtype deviceType,
        Caps9 capabilities)
    {
        uint behaviorFlags = D3D9.CreateDisableDriverManagementEX | D3D9.CreateFpuPreserve;
        if ((initializationFlags & Direct3D9RenderTargetInitializationFlags.SingleThreadedUsage) == 0)
        {
            behaviorFlags |= D3D9.CreateMultithreaded;
        }

        if ((capabilities.Caps2 & D3D9.Caps2Canshareresource) != 0)
        {
            behaviorFlags |= D3D9.CreateScreensaver | D3D9.CreateDisablePsgpThreading;
        }

        if ((capabilities.DevCaps & D3D9.DevcapsHwtransformandlight) != 0 && deviceType != Devtype.SW)
        {
            behaviorFlags |= D3D9.CreateHardwareVertexprocessing;
            if ((capabilities.DevCaps & D3D9.DevcapsPuredevice) != 0)
            {
                behaviorFlags |= D3D9.CreatePuredevice;
            }
        }
        else
        {
            behaviorFlags |= D3D9.CreateSoftwareVertexprocessing;
        }

        return behaviorFlags;
    }

    private Displaymode GetDisplayMode(
        uint adapterOrdinal,
        Devtype deviceType,
        Direct3D9RenderTargetInitializationFlags initializationFlags)
    {
        Displaymode displayMode;
        if (_displaySet is not null && !_displaySet.Characteristics.Displays.IsDefaultOrEmpty)
        {
            int modeResult = _displaySet.GetMode(adapterOrdinal, out Direct3D9DisplayMode cachedMode, out _);
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(modeResult);
            displayMode = new Displaymode(cachedMode.Width, cachedMode.Height, cachedMode.RefreshRate, cachedMode.Format);
        }
        else
        {
            displayMode = _displayModeProvider?.Invoke(adapterOrdinal) ?? default;
        }

        if (_checkDeviceType is not null)
        {
            Format targetFormat = (initializationFlags & Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha) != 0
                ? Format.A8R8G8B8
                : Format.X8R8G8B8;
            int result = _checkDeviceType(adapterOrdinal, deviceType, displayMode.Format, targetFormat);
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(result);
        }

        return displayMode;
    }

    private long GetAdapterLuid(uint adapterOrdinal)
    {
        if (_displaySet is null || _displaySet.Characteristics.Displays.IsDefaultOrEmpty)
        {
            return 0;
        }

        if (adapterOrdinal >= _displaySet.Characteristics.Displays.Length)
        {
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
        }

        Direct3D9Display display = _displaySet.Characteristics.Displays[(int) adapterOrdinal];
        if (display.DisplayIndex != adapterOrdinal)
        {
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
        }

        return display.Direct3DAdapterLuid;
    }

    private void InitializeDeviceTier(Direct3D9Device device)
    {
        if (device.DeviceType == Devtype.SW
            || _displaySet is null
            || _displaySet.Characteristics.Displays.IsDefaultOrEmpty)
        {
            return;
        }

        if (device.AdapterOrdinal >= _displaySet.Characteristics.Displays.Length)
        {
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
        }

        Direct3D9Display display = _displaySet.Characteristics.Displays[(int) device.AdapterOrdinal];
        if (display.DisplayIndex != device.AdapterOrdinal)
        {
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
        }

        device.UpdateTier(display.GraphicsAccelerationCaps.TierValue);
    }

    private void ApplyDeviceDriverWorkarounds(Direct3D9Device device)
    {
        if (device.DeviceType is Devtype.SW or Devtype.Ref
            || _displaySet is null
            || _displaySet.Characteristics.Displays.IsDefaultOrEmpty)
        {
            return;
        }

        if (device.AdapterOrdinal >= _displaySet.Characteristics.Displays.Length)
        {
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
        }

        Direct3D9Display display = _displaySet.Characteristics.Displays[(int) device.AdapterOrdinal];
        if (display.DisplayIndex != device.AdapterOrdinal)
        {
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
        }

        using Direct3D9DeviceEntryGuard deviceEntry = new(device);
        Caps9 capabilities = device.Capabilities;
        int result = Direct3D9DeviceDriverWorkarounds.Apply(
            device.DeviceType,
            skipDriverCheck: false,
            display.IsRecentDriver,
            display.IsBadDriver,
            ref capabilities);
        System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(result);
        device.UpdateCapabilities(capabilities);
    }

    private void GatherSupportedTextureFormats(Direct3D9Device device)
    {
        if (_checkDeviceFormat is null)
        {
            return;
        }

        Direct3D9TextureFormatSupport support = Direct3D9TextureFormatSupportFactory.Gather(
            device.AdapterOrdinal,
            device.DeviceType,
            device.DisplayMode.Format,
            _checkDeviceFormat);
        device.UpdateTextureFormatSupport(support);
    }

    private void GatherSupportedMultisampleTypes(Direct3D9Device device)
    {
        if (_checkDeviceMultisampleType is null)
        {
            return;
        }

        Direct3D9MultisampleSupport support = Direct3D9MultisampleSupportFactory.Gather(
            device.AdapterOrdinal,
            device.DeviceType,
            Direct3D9HardwareCapabilities.HasWddmSupport(device.Capabilities),
            _maximumMultisampleTypeProvider?.Invoke(),
            _checkDeviceMultisampleType);
        device.UpdateMultisampleSupport(support);
    }

    private void InitializeTextPixelShaders(Direct3D9Device device)
    {
        if (!device.IsTextPixelShaderInitializationEligible || _initializeTextPixelShaders is null)
        {
            return;
        }

        int result = _initializeTextPixelShaders(device);
        System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(result);
    }

    private void InitializeDynamicBuffers(Direct3D9Device device)
    {
        if (_initializeDynamicBuffers is null)
        {
            return;
        }

        int result = _initializeDynamicBuffers(device);
        System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(result);
    }

    private void InitializeRenderState(Direct3D9Device device)
    {
        if (_initializeRenderState is null)
        {
            return;
        }

        int result = _initializeRenderState(device);
        System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(result);
    }

    private void CheckBackBufferRenderTargetFormat(Direct3D9Device device, Format format)
    {
        if (_checkRenderTargetFormat is null)
        {
            return;
        }

        int result = _checkRenderTargetFormat(device, format);
        System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(result);
    }

    private void TestLevel1Device(Direct3D9Device device)
    {
        if (_testLevel1Device is null)
        {
            return;
        }

        int result = _testLevel1Device(device);
        if (result >= 0)
        {
            return;
        }

        if (device.DeviceType == Devtype.Hal
            && result is not Direct3D9Factory.OutOfVideoMemoryHResult
            and not Direct3D9Factory.OutOfMemoryHResult
            and not Direct3D9Factory.DriverInternalErrorHResult)
        {
            _disableAdapter?.Invoke(device.AdapterOrdinal);
        }

        System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(result);
    }

    private static PresentParameters ComposePresentParameters(
        nint focusWindow,
        Direct3D9RenderTargetInitializationFlags initializationFlags)
    {
        uint flags = (initializationFlags & Direct3D9RenderTargetInitializationFlags.DisableDisplayClipping) == 0
            ? unchecked((uint) D3D9.PresentflagDeviceclip)
            : 0;
        if ((initializationFlags & Direct3D9RenderTargetInitializationFlags.PresentUsingMask)
            != Direct3D9RenderTargetInitializationFlags.PresentUsingHal)
        {
            flags |= unchecked((uint) D3D9.PresentflagLockableBackbuffer);
        }

        return new PresentParameters(
            backBufferWidth: 1,
            backBufferHeight: 1,
            backBufferFormat: (initializationFlags & Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha) != 0
                ? Format.A8R8G8B8
                : Format.X8R8G8B8,
            backBufferCount: 1,
            multiSampleType: MultisampleType.MultisampleNone,
            multiSampleQuality: 0,
            swapEffect: (initializationFlags & Direct3D9RenderTargetInitializationFlags.PresentRetainContents) != 0
                ? Swapeffect.Copy
                : Swapeffect.Discard,
            hDeviceWindow: focusWindow,
            windowed: true,
            enableAutoDepthStencil: false,
            autoDepthStencilFormat: Format.Unknown,
            flags: flags,
            fullScreenRefreshRateInHz: 0,
            presentationInterval: (initializationFlags & Direct3D9RenderTargetInitializationFlags.PresentImmediately) != 0
                ? D3D9.PresentIntervalImmediate
                : D3D9.PresentIntervalOne);
    }

    private Direct3D9Device? FindUsableDevice(Direct3D9DeviceCreationParameters creationParameters)
    {
        foreach (Direct3D9Device device in _usableDevices)
        {
            if (creationParameters.DeviceType != device.DeviceType)
            {
                continue;
            }

            if (creationParameters.DeviceType == Devtype.SW)
            {
                return device;
            }

            uint behaviorFlagDifferences = creationParameters.BehaviorFlags ^ device.BehaviorFlags;
            behaviorFlagDifferences &= unchecked((uint) ~D3D9.CreateDisableDriverManagementEX);
            if (creationParameters.AdapterOrdinal == device.AdapterOrdinal && behaviorFlagDifferences == 0)
            {
                return device;
            }
        }

        return null;
    }

    private void OnDeviceUnusable(Direct3D9Device device)
    {
        if (ReferenceEquals(device, _softwareDevice))
        {
            return;
        }

        lock (_syncRoot)
        {
            int usableIndex = _usableDevices.IndexOf(device);
            if (usableIndex < 0)
            {
                return;
            }

            NotifyDeviceLost(device);
            _usableDevices.RemoveAt(usableIndex);
            _unusableDevices.Add(device);
        }
    }

    private void OnDeviceDisposed(Direct3D9Device device)
    {
        lock (_syncRoot)
        {
            if (ReferenceEquals(device, _softwareDevice))
            {
                _softwareDevice = null;
            }

            bool isTracked = _usableDevices.Contains(device) || _unusableDevices.Contains(device);
            if (isTracked)
            {
                NotifyDeviceLost(device);
            }

            _usableDevices.Remove(device);
            _unusableDevices.Remove(device);
            _lostDevices.Remove(device);
        }
    }

    private void NotifyDeviceLost(Direct3D9Device device)
    {
        if (_lostDevices.Add(device))
        {
            NotifyAdapterStatus(device.AdapterOrdinal, isValid: false);
        }
    }

    private void NotifyAdapterStatus(uint adapterOrdinal, bool isValid)
    {
        Action<uint, bool>[] listeners;
        lock (_syncRoot)
        {
            listeners = [.. _adapterStatusListeners];
        }

        foreach (Action<uint, bool> listener in listeners)
        {
            listener(adapterOrdinal, isValid);
        }
    }

    public void Dispose()
    {
        Direct3D9Device[] devices;
        lock (_syncRoot)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            devices = [.. _usableDevices, .. _unusableDevices];
            if (_softwareDevice is not null && !devices.Contains(_softwareDevice))
            {
                devices = [.. devices, _softwareDevice];
            }

            _usableDevices.Clear();
            _unusableDevices.Clear();
            _lostDevices.Clear();
            _softwareDevice = null;
            _adapterStatusListeners.Clear();
            _displaySet = null;
            _nextDisplaySet = null;
        }

        foreach (Direct3D9Device device in devices)
        {
            device.Dispose();
        }
    }
}
