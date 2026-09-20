namespace WpfGfxShape.Core;

internal sealed class Direct3D9UseContextGuard : IDisposable
{
    private Direct3D9Device? _device;
    private readonly uint _depth;

    internal Direct3D9UseContextGuard(Direct3D9Device device)
    {
        ArgumentNullException.ThrowIfNull(device);
        _device = device;
        _depth = device.EnterUseContext();
    }

    public void Dispose()
    {
        Direct3D9Device? device = Interlocked.Exchange(ref _device, null);
        device?.ExitUseContext(_depth);
    }
}
