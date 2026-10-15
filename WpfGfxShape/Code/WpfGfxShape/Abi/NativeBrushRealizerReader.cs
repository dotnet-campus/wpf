namespace WpfGfxShape.Abi;

internal static unsafe class NativeBrushRealizerReader
{
    internal static int Realize(nint realizer, nint brushContext, nint context, nint creator,
        out nint brush, out nint effects)
    {
        brush = effects = 0;
        void** table = *(void***)realizer;
        int result = ((delegate* unmanaged<nint, uint, uint, nint, nint, nint, int>)table[6])(
            realizer, 0, uint.MaxValue, brushContext, context, creator);
        if (result < 0) return result;
        brush = ((delegate* unmanaged<nint, byte, nint>)table[9])(realizer, 0);
        nint effect = 0;
        result = ((delegate* unmanaged<nint, nint*, int>)table[10])(realizer, &effect);
        if (result < 0) return result;
        effects = effect;
        return 0;
    }
}
