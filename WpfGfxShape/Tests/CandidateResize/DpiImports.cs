using System.Runtime.InteropServices;
namespace WpfGfxShape.ComAcceptance;
public sealed unsafe partial class D3DImageTests
{
    [DllImport("user32.dll")] private static extern nint GetWindowDpiAwarenessContext(nint window);
    [DllImport("user32.dll")] private static extern int AreDpiAwarenessContextsEqual(nint first, nint second);
}
