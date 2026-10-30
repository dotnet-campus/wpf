namespace WpfGfxShape.Core.Av;

// The former original-wpfgfx player/provider forwarding implementation is removed.
// Keep the resource binding type until an independent backend supplies notifications.
internal sealed class SystemMediaSource
{
    private SystemMediaSource() { }

    internal static int Create(nint client, byte canOpen, out nint output)
    {
        output = 0;
        return Abi.NativeAotAbiProbe.E_NOTIMPL;
    }

    internal static SystemMediaSource? Resolve(nint media) => null;

    internal void Subscribe(Action listener) => throw new NotSupportedException();
    internal void Unsubscribe(Action listener) => throw new NotSupportedException();
}
