using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

// Managed immediate realization snapshot; not a CBrushRealizer or CMILBrush ABI object.
internal static unsafe class ImmediateSolidBrushRealizer
{
    internal static int Draw(Direct3D9SoftwareImageRenderer renderer, SoftwareImageCoverage? coverage,
        SoftwareImageDrawingContext context, MilColorF color)
        => DrawColor(renderer, coverage, context, color);

    internal static int DrawBorrowedSolid(Direct3D9SoftwareImageRenderer renderer, nint brush,
        SoftwareImageCoverage? coverage, SoftwareImageDrawingContext context)
    {
        if (brush == 0) return 0;
        uint type = ((delegate* unmanaged<nint, uint>)(*(void***)brush)[3])(brush);
        if (type != 1) return Direct3D9Factory.NotImplementedHResult;
        float* color = (float*)(brush + sizeof(nint));
        return DrawColor(renderer, coverage, context, new(color[3], color[0], color[1], color[2]));
    }

    private static int DrawColor(Direct3D9SoftwareImageRenderer renderer, SoftwareImageCoverage? coverage,
        SoftwareImageDrawingContext context, MilColorF color)
    {
        if (!float.IsFinite(color.Alpha) || !float.IsFinite(color.Red)
            || !float.IsFinite(color.Green) || !float.IsFinite(color.Blue))
            return Direct3D9Factory.InvalidArgumentHResult;
        return renderer.DrawBrush(null, coverage, context, color);
    }
}
