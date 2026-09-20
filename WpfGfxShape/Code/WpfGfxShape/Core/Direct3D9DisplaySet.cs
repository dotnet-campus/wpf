using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal enum Direct3D9PixelGeometry
{
    Flat,
    Rgb,
    Bgr
}

internal enum Direct3D9RenderingMode
{
    BiLevel,
    Grayscale,
    ClearType
}

internal readonly record struct Direct3D9DisplaySettings(
    Direct3D9PixelGeometry PixelStructure,
    float Gamma,
    float EnhancedContrast,
    float ClearTypeLevel,
    Direct3D9RenderingMode DisplayRenderingMode,
    bool HasRenderingParameters = true)
{
    internal bool IsEquivalentTo(Direct3D9DisplaySettings settings)
    {
        return PixelStructure == settings.PixelStructure
            && HasRenderingParameters
            && settings.HasRenderingParameters
            && Gamma == settings.Gamma
            && EnhancedContrast == settings.EnhancedContrast
            && ClearTypeLevel == settings.ClearTypeLevel
            && DisplayRenderingMode == settings.DisplayRenderingMode;
    }
}

internal readonly record struct Direct3D9DisplayMode(
    uint Size,
    uint Width,
    uint Height,
    uint RefreshRate,
    Format Format,
    Scanlineordering ScanLineOrdering);

internal readonly record struct Direct3D9GraphicsAccelerationCaps(
    int TierValue,
    int HasWddmSupport,
    uint PixelShaderVersion,
    uint VertexShaderVersion,
    uint MaxTextureWidth,
    uint MaxTextureHeight,
    int WindowCompatibleMode,
    uint BitsPerPixel,
    uint HasSse2Support,
    uint MaxPixelShader30InstructionSlots);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct Direct3D9SurfaceRect(int Left, int Top, int Right, int Bottom);

internal readonly record struct Direct3D9Point(int X, int Y);

internal readonly record struct Direct3D9DisplayBounds(
    nint DpiAwarenessContextValue,
    Direct3D9SurfaceRect Bounds);

internal readonly record struct Direct3D9Display(
    uint DisplayIndex,
    long Direct3DAdapterLuid,
    nint MonitorHandle,
    ImmutableArray<Direct3D9DisplayBounds> Bounds,
    string DeviceName,
    uint StateFlags,
    Direct3D9DisplaySettings Settings,
    uint MemorySize,
    bool IsRecentDriver,
    bool IsBadDriver,
    uint GraphicsCardVendorId,
    uint GraphicsCardDeviceId,
    Direct3D9DisplayMode DisplayMode,
    Displayrotation DisplayRotation,
    Direct3D9GraphicsAccelerationCaps GraphicsAccelerationCaps)
{
    internal bool IsEquivalentTo(Direct3D9Display display)
    {
        return DisplayIndex == display.DisplayIndex
            && Direct3DAdapterLuid == display.Direct3DAdapterLuid
            && MonitorHandle == display.MonitorHandle
            && DisplayBoundsAreEquivalent(Bounds, display.Bounds)
            && string.Equals(DeviceName, display.DeviceName, StringComparison.Ordinal)
            && StateFlags == display.StateFlags
            && Settings.IsEquivalentTo(display.Settings)
            && MemorySize == display.MemorySize
            && IsRecentDriver == display.IsRecentDriver
            && IsBadDriver == display.IsBadDriver
            && GraphicsCardVendorId == display.GraphicsCardVendorId
            && GraphicsCardDeviceId == display.GraphicsCardDeviceId
            && DisplayMode == display.DisplayMode
            && DisplayRotation == display.DisplayRotation
            && GraphicsAccelerationCaps == display.GraphicsAccelerationCaps;
    }

    internal int GetMode(out Direct3D9DisplayMode displayMode, out Displayrotation displayRotation)
    {
        displayMode = DisplayMode;
        displayRotation = DisplayRotation;
        return DisplayMode.Size > 0
            ? Direct3D9Factory.SuccessHResult
            : Direct3D9Factory.NotInitializedHResult;
    }

    internal static bool DisplayBoundsAreEquivalent(
        ImmutableArray<Direct3D9DisplayBounds> first,
        ImmutableArray<Direct3D9DisplayBounds> second)
    {
        if (first.Length != second.Length)
        {
            return false;
        }

        foreach (Direct3D9DisplayBounds bounds in first)
        {
            bool foundEquivalentBounds = false;
            foreach (Direct3D9DisplayBounds candidate in second)
            {
                if (candidate.DpiAwarenessContextValue == bounds.DpiAwarenessContextValue)
                {
                    foundEquivalentBounds = candidate.Bounds == bounds.Bounds;
                    break;
                }
            }

            if (!foundEquivalentBounds)
            {
                return false;
            }
        }

        return true;
    }
}

internal readonly record struct Direct3D9DisplaySetCharacteristics(
    ulong RequiredVideoDriverDate,
    uint Direct3DAdapterCount,
    bool IsNonLocalDevicePresent,
    ImmutableArray<Direct3D9DisplayBounds> DisplayBounds,
    ImmutableArray<Direct3D9Display> Displays)
{
    internal bool IsEquivalentTo(Direct3D9DisplaySetCharacteristics characteristics)
    {
        if (RequiredVideoDriverDate != characteristics.RequiredVideoDriverDate
            || Direct3DAdapterCount != characteristics.Direct3DAdapterCount
            || IsNonLocalDevicePresent != characteristics.IsNonLocalDevicePresent
            || !Direct3D9Display.DisplayBoundsAreEquivalent(DisplayBounds, characteristics.DisplayBounds)
            || Displays.Length != characteristics.Displays.Length)
        {
            return false;
        }

        for (int index = 0; index < Displays.Length; index++)
        {
            if (!Displays[index].IsEquivalentTo(characteristics.Displays[index]))
            {
                return false;
            }
        }

        return true;
    }
}

internal sealed class Direct3D9DisplaySet : IDisposable
{
    private readonly object _syncRoot = new();
    private readonly Func<bool>? _hasDisplayStateChanged;
    private readonly Func<uint>? _displayUniquenessProvider;
    private readonly Func<uint>? _externalUpdateCountProvider;
    private Func<Direct3D9DisplaySet?>? _latestDisplaySetProvider;
    private Direct3D9SoftwareRasterizerRegistration? _softwareRasterizerRegistration;
    private Direct3D9DisplaySetInitialization? _direct3DInitialization;
    private uint _displayUniqueness;
    private uint _externalUpdateCount;

    internal Direct3D9DisplaySet(Func<bool>? hasDisplayStateChanged = null)
        : this(default, 0, 0, null, null, null, hasDisplayStateChanged)
    {
    }

    internal Direct3D9DisplaySet(
        Direct3D9DisplaySetCharacteristics characteristics,
        uint displayUniqueness,
        uint externalUpdateCount,
        Func<uint> displayUniquenessProvider,
        Func<uint> externalUpdateCountProvider,
        Func<Direct3D9DisplaySet?>? latestDisplaySetProvider = null,
        bool hasDirect3D9Ex = false)
        : this(
            characteristics,
            displayUniqueness,
            externalUpdateCount,
            displayUniquenessProvider,
            externalUpdateCountProvider,
            latestDisplaySetProvider,
            null,
            hasDirect3D9Ex)
    {
    }

    internal Direct3D9DisplaySet(
        Direct3D9DisplaySetCharacteristics characteristics,
        uint displayUniqueness,
        uint externalUpdateCount,
        Func<uint> displayUniquenessProvider,
        Func<uint> externalUpdateCountProvider,
        Direct3D9DisplaySetInitialization direct3DInitialization,
        bool hasDirect3D9Ex)
        : this(
            characteristics,
            displayUniqueness,
            externalUpdateCount,
            displayUniquenessProvider,
            externalUpdateCountProvider,
            null,
            null,
            hasDirect3D9Ex)
    {
        ArgumentNullException.ThrowIfNull(direct3DInitialization);
        _direct3DInitialization = direct3DInitialization;
    }

    private Direct3D9DisplaySet(
        Direct3D9DisplaySetCharacteristics characteristics,
        uint displayUniqueness,
        uint externalUpdateCount,
        Func<uint>? displayUniquenessProvider,
        Func<uint>? externalUpdateCountProvider,
        Func<Direct3D9DisplaySet?>? latestDisplaySetProvider,
        Func<bool>? hasDisplayStateChanged,
        bool hasDirect3D9Ex = false)
    {
        Characteristics = characteristics;
        HasDirect3D9Ex = hasDirect3D9Ex;
        _displayUniqueness = displayUniqueness;
        _externalUpdateCount = externalUpdateCount;
        _displayUniquenessProvider = displayUniquenessProvider;
        _externalUpdateCountProvider = externalUpdateCountProvider;
        _latestDisplaySetProvider = latestDisplaySetProvider;
        _hasDisplayStateChanged = hasDisplayStateChanged;
    }

    internal Direct3D9DisplaySetCharacteristics Characteristics { get; }

    internal bool HasDirect3D9Ex { get; }

    internal int Direct3DInitializationHResult => _direct3DInitialization?.HResult
        ?? Direct3D9Factory.NotInitializedHResult;

    internal Direct3D9Objects? Direct3DObjects => _direct3DInitialization?.Objects;

    internal int GetMode(uint adapterOrdinal, out Direct3D9DisplayMode displayMode, out Displayrotation displayRotation)
    {
        ImmutableArray<Direct3D9Display> displays = Characteristics.Displays;
        if (displays.IsDefaultOrEmpty || adapterOrdinal >= (uint) displays.Length)
        {
            displayMode = default;
            displayRotation = default;
            return Direct3D9Factory.DisplayStateInvalidHResult;
        }

        return displays[(int) adapterOrdinal].GetMode(out displayMode, out displayRotation);
    }

    internal int GetDisplayIndexFromDisplayId(uint displayId, out uint displayIndex)
    {
        ImmutableArray<Direct3D9Display> displays = Characteristics.Displays;
        for (int index = 0; index < displays.Length; index++)
        {
            if (displays[index].DisplayIndex == displayId)
            {
                displayIndex = (uint) index;
                return Direct3D9Factory.SuccessHResult;
            }
        }

        displayIndex = 0;
        return Direct3D9Factory.InvalidArgumentHResult;
    }

    internal bool IsUpToDate()
    {
        lock (_syncRoot)
        {
            return (_displayUniquenessProvider is null || _displayUniquenessProvider() == _displayUniqueness)
                && (_externalUpdateCountProvider is null || _externalUpdateCountProvider() == _externalUpdateCount);
        }
    }

    internal bool IsEquivalentTo(Direct3D9DisplaySet displaySet)
    {
        ArgumentNullException.ThrowIfNull(displaySet);
        return Characteristics.IsEquivalentTo(displaySet.Characteristics);
    }

    internal void UpdateUniqueness(Direct3D9DisplaySet displaySet)
    {
        ArgumentNullException.ThrowIfNull(displaySet);
        (uint displayUniqueness, uint externalUpdateCount) = displaySet.GetUniqueness();
        lock (_syncRoot)
        {
            _displayUniqueness = displayUniqueness;
            _externalUpdateCount = externalUpdateCount;
        }
    }

    private (uint DisplayUniqueness, uint ExternalUpdateCount) GetUniqueness()
    {
        lock (_syncRoot)
        {
            return (_displayUniqueness, _externalUpdateCount);
        }
    }

    internal void BindLatestDisplaySetProvider(Func<Direct3D9DisplaySet?> latestDisplaySetProvider)
    {
        ArgumentNullException.ThrowIfNull(latestDisplaySetProvider);
        lock (_syncRoot)
        {
            if (_latestDisplaySetProvider is not null
                && _latestDisplaySetProvider != latestDisplaySetProvider)
            {
                throw new InvalidOperationException("The display set is already bound to a different display-set provider.");
            }

            _latestDisplaySetProvider = latestDisplaySetProvider;
        }
    }

    internal void EnsureSoftwareRasterizerRegistered(
        Direct3D9SoftwareRasterizerLoader loader,
        Func<nint, int> registerSoftwareDevice)
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentNullException.ThrowIfNull(registerSoftwareDevice);

        Direct3D9SoftwareRasterizerRegistration registration;
        lock (_syncRoot)
        {
            registration = _softwareRasterizerRegistration ??= new(
                loader,
                registerSoftwareDevice,
                this);
        }

        registration.EnsureRegistered();
    }

    internal bool DangerousHasDisplayStateChanged()
    {
        if (_hasDisplayStateChanged is not null)
        {
            return _hasDisplayStateChanged();
        }

        if (IsUpToDate())
        {
            return false;
        }

        Func<Direct3D9DisplaySet?>? latestDisplaySetProvider;
        lock (_syncRoot)
        {
            latestDisplaySetProvider = _latestDisplaySetProvider;
        }

        try
        {
            return !ReferenceEquals(latestDisplaySetProvider?.Invoke(), this);
        }
        catch (COMException)
        {
            return true;
        }
    }

    public void Dispose()
    {
        Direct3D9DisplaySetInitialization? initialization;
        lock (_syncRoot)
        {
            initialization = _direct3DInitialization;
            _direct3DInitialization = null;
        }

        initialization?.Dispose();
    }
}
