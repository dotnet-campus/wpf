using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

internal static unsafe partial class BitmapRenderTargetExports
{
    private struct NativeLayer
    {
        internal NativeLayer* Previous;
        internal int Left, Top, Right, Bottom;
    }

    private static int BeginNativeLayer(InternalView* self, nint bounds, uint mode, nint mask, float alpha, nint brush)
    {
        if (bounds == 0 || mode > 1 || !float.IsFinite(alpha) || alpha < 0 || alpha > 1) return InvalidArgument;
        float* requested = (float*)bounds;
        uint width, height;
        nint bitmap = self->Owner->Bitmap;
        int result = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)bitmap)[3])(bitmap, &width, &height);
        if (result < 0) return result;
        if (width > int.MaxValue || height > int.MaxValue) return InvalidArgument;
        int left = 0, top = 0, right = (int)width, bottom = (int)height;
        if (self->Layer != null)
        {
            left = self->Layer->Left;
            top = self->Layer->Top;
            right = self->Layer->Right;
            bottom = self->Layer->Bottom;
        }
        // Native aliased bounds intersection treats NaN as no intersection.
        if (float.IsNaN(requested[0]) || float.IsNaN(requested[1])
            || float.IsNaN(requested[2]) || float.IsNaN(requested[3]))
            left = top = right = bottom = 0;
        else
        {
            left = Math.Max(left, ClipCoordinate(requested[0], (int)width));
            top = Math.Max(top, ClipCoordinate(requested[1], (int)height));
            right = Math.Min(right, ClipCoordinate(requested[2], (int)width));
            bottom = Math.Min(bottom, ClipCoordinate(requested[3], (int)height));
        }
        Guid format;
        result = ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)bitmap)[4])(bitmap, &format);
        if (result < 0) return result;
        float alphaMinimum = SoftwareBitmap.FormatCode(format) == 26
            ? 1.17549435E-38f : 0.5f / 3354f;
        if (right <= left || bottom <= top || alpha < alphaMinimum)
            left = top = right = bottom = 0;
        // Native software targets with alpha cannot capture a fixup layer.
        else if (mask != 0 || brush != 0 || 1 - alpha >= alphaMinimum)
            return NotImplemented;
        NativeLayer* layer = (NativeLayer*)NativeMemory.Alloc((nuint)sizeof(NativeLayer));
        *layer = new NativeLayer { Previous = self->Layer, Left = left, Top = top, Right = right, Bottom = bottom };
        self->Layer = layer;
        return 0;
    }

    private static void DiscardNativeLayers(InternalView* self)
    {
        NativeLayer* layer = self->Layer;
        self->Layer = null;
        while (layer != null)
        {
            NativeLayer* previous = layer->Previous;
            NativeMemory.Free(layer);
            layer = previous;
        }
    }
}
