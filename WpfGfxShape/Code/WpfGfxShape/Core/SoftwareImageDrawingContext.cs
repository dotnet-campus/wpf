namespace WpfGfxShape.Core;

// Managed draw-call state; this is not the native CContextState memory layout.
internal readonly record struct SoftwareImageDrawingContext(
    GeneratedImageTransform WorldToDevice,
    Direct3D9SurfaceRect? AliasedClip,
    uint BitmapScalingMode,
    MilCompositingMode CompositingMode,
    MilRectD? SourceCoverage = null,
    float? PrefilterThreshold = null,
    bool Antialias = true,
    bool? PrefilterEnabled = null,
    SoftwareBitmapEffects? Effects = null,
    SoftwareImageCoverage? GeometryMask = null,
    MilBitmapWrapMode WrapMode = MilBitmapWrapMode.Extend,
    bool ExtendThroughGeometry = false,
    float AlphaScale = 1,
    MilColorF BorderColor = default,
    GeneratedImageTransform? EffectToDevice = null,
    GeneratedGlyphAlpha? GlyphAlpha = null,
    SoftwareImageWorkingFormat WorkingFormat = SoftwareImageWorkingFormat.Native);

internal enum SoftwareImageWorkingFormat
{
    Native,
    DrawingImagePbgra32
}
