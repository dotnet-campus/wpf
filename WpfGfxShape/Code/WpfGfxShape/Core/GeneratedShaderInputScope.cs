using System.Numerics;

namespace WpfGfxShape.Core;

internal readonly record struct GeneratedShaderInputScope(uint Width, uint Height,
    GeneratedImageTransform InputTransform, GeneratedImageTransform OutputTransform,
    Vector4 UvDerivatives)
{
    internal static int TryCreate(MilRectD content, MilShaderEffectCommand command,
        GeneratedImageTransform transform, out GeneratedShaderInputScope scope)
    {
        scope = default;
        if (!double.IsFinite(command.Left) || !double.IsFinite(command.Right)
            || !double.IsFinite(command.Top) || !double.IsFinite(command.Bottom)
            || command.Left < 0 || command.Right < 0 || command.Top < 0 || command.Bottom < 0)
            return Direct3D9Factory.InvalidCallHResult;
        if (content.Width <= 0 || content.Height <= 0)
            return Direct3D9Factory.ClippedToEmptyHResult;
        if (!transform.TryDecompose(out var scale, out var rest))
            return Direct3D9Factory.NonInvertibleMatrixHResult;
        double left = Math.Floor((content.X - command.Left) * scale.ScaleX);
        double top = Math.Floor((content.Y - command.Top) * scale.ScaleY);
        double right = Math.Ceiling((content.X + content.Width + command.Right) * scale.ScaleX);
        double bottom = Math.Ceiling((content.Y + content.Height + command.Bottom) * scale.ScaleY);
        double width = right - left, height = bottom - top;
        if (!double.IsFinite(left) || !double.IsFinite(top)
            || !double.IsFinite(width) || !double.IsFinite(height)
            || width <= 0 || height <= 0 || width > uint.MaxValue || height > uint.MaxValue)
            return Direct3D9Factory.InvalidCallHResult;
        var output = rest.Prepend(new(1, 1, left, top));
        if (!output.TryInvert(out var inverse))
            return Direct3D9Factory.NonInvertibleMatrixHResult;
        scope = new((uint)width, (uint)height,
            new(scale.ScaleX, scale.ScaleY, -left, -top), output,
            new((float)(inverse.ScaleX / width), (float)(inverse.M12 / height),
                (float)(inverse.M21 / width), (float)(inverse.ScaleY / height)));
        return 0;
    }
}
