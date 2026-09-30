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
    SoftwareBitmapEffects? Effects = null);
