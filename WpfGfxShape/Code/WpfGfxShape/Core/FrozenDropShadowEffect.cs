namespace WpfGfxShape.Core;

internal readonly record struct FrozenDropShadowEffect(double Depth, MilColorF Color,
    double Direction, double Opacity, double Radius, MilEffectRenderingBias Bias)
{
    internal double OffsetX => (float)(Depth * Math.Cos(Direction * Math.PI / 180));
    internal double OffsetY => (float)(-Depth * Math.Sin(Direction * Math.PI / 180));
    internal bool IsFinite => double.IsFinite(Depth) && double.IsFinite(Direction)
        && double.IsFinite(Opacity) && double.IsFinite(Radius);
    internal double Left => Radius + Math.Max(0, -OffsetX);
    internal double Top => Radius + Math.Max(0, -OffsetY);
    internal double Right => Radius + Math.Max(0, OffsetX);
    internal double Bottom => Radius + Math.Max(0, OffsetY);
    internal MilRectD Inflate(MilRectD bounds) => bounds.Width <= 0 || bounds.Height <= 0 ? bounds
        : new(bounds.X - Left, bounds.Y - Top, bounds.Width + Left + Right, bounds.Height + Top + Bottom);
}
