namespace WpfGfxShape.Core;

internal delegate int Direct3D9UpdateDeviceBitmapSurface(
    ReadOnlySpan<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
    nint sourceSurface);

internal sealed class Direct3D9DeviceBitmapColorSourceUpdater
{
    private readonly Direct3D9Device _device;
    private readonly bool _usesSharedHandle;
    private readonly Direct3D9UpdateDeviceBitmapSurface _updateSharedHandle;
    private readonly Direct3D9UpdateDeviceBitmapSurface _updateSoftware;

    internal Direct3D9DeviceBitmapColorSourceUpdater(
        Direct3D9Device device,
        bool usesSharedHandle,
        Direct3D9UpdateDeviceBitmapSurface updateSharedHandle,
        Direct3D9UpdateDeviceBitmapSurface updateSoftware)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(updateSharedHandle);
        ArgumentNullException.ThrowIfNull(updateSoftware);

        _device = device;
        _usesSharedHandle = usesSharedHandle;
        _updateSharedHandle = updateSharedHandle;
        _updateSoftware = updateSoftware;
    }

    internal int UpdateSurface(
        ReadOnlySpan<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
        nint sourceSurface)
    {
        ArgumentOutOfRangeException.ThrowIfZero(sourceSurface);
        ArgumentOutOfRangeException.ThrowIfZero(dirtyRectangles.Length);

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        return _usesSharedHandle
            ? _updateSharedHandle(dirtyRectangles, sourceSurface)
            : _updateSoftware(dirtyRectangles, sourceSurface);
    }
}
