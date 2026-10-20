namespace WpfGfxShape.ComAcceptance;

// Release CContextState: three pointers precede the fixed-width matrix/clip prefix.
// RenderState is pointer-aligned after CAliasedClip; WorldToDevice follows it.
internal static class InternalContextLayout
{
    internal static int Clip => 352 + 3 * IntPtr.Size;
    internal static int RenderState => IntPtr.Size == 4 ? 384 : 400;
    internal static int WorldToDevice => RenderState + IntPtr.Size;
}
