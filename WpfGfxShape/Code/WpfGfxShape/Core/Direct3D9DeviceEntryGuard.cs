namespace WpfGfxShape.Core;

internal sealed class Direct3D9DeviceEntryGuard : IDisposable
{
    private Direct3D9Device? _device;

    internal Direct3D9DeviceEntryGuard(Direct3D9Device device)
    {
        ArgumentNullException.ThrowIfNull(device);
        device.Enter();
        _device = device;
    }

    public void Dispose()
    {
        Direct3D9Device? device = Interlocked.Exchange(ref _device, null);
        device?.Leave();
    }
}
