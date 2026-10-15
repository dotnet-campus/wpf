namespace WpfGfxShape.Core;

// Frame-owned pure C# realization; never exposed as a CBrushRealizer ABI object.
internal sealed unsafe class SoftwareBrushRealization : IDisposable
{
    private GeneratedImageMask? _image;
    private nint _immediate;
    private bool _disposed;
    private bool _realizing;

    private SoftwareBrushRealization(GeneratedImageMask? image, MilColorF? color)
    {
        _image = image;
        if (image is null) _immediate = WpfGfxShape.Abi.NativeImmediateSolidRealization.Create(color);
    }

    internal static int Capture(GeneratedProtocolResource? resource, GeneratedImageTransform transform,
        uint scalingMode, MilRectD bounds, out SoftwareBrushRealization? realization)
    {
        realization = null;
        if (resource is null) { realization = new(null, null); return 0; }
        if (resource is GeneratedSolidColorBrushResource solid)
        {
            var value = solid.CurrentValue;
            if (!double.IsFinite(value.Opacity)) return Direct3D9Factory.InvalidArgumentHResult;
            return CreateImmediate(value.Color with
            {
                Alpha = value.Color.Alpha * (float)Math.Clamp(value.Opacity, 0, 1)
            }, out realization);
        }
        if (resource is not GeneratedImageBrushResource image) return Direct3D9Factory.NotImplementedHResult;
        if (!GeneratedImageMask.TryCapture(image, transform, scalingMode, out var snapshot, bounds))
            return Direct3D9Factory.NotImplementedHResult;
        try
        {
            realization = new(snapshot, null);
            return 0;
        }
        finally { if (realization is null) snapshot?.Dispose(); }
    }

    private static int CreateImmediate(MilColorF color, out SoftwareBrushRealization? realization)
    {
        realization = null;
        if (!float.IsFinite(color.Alpha) || !float.IsFinite(color.Red)
            || !float.IsFinite(color.Green) || !float.IsFinite(color.Blue))
            return Direct3D9Factory.InvalidArgumentHResult;
        realization = new(null, color);
        return 0;
    }

    internal int BeginPrimitive(SoftwareImageRenderSession session, ref SoftwareImageDrawingContext context,
        out nint realizer)
    {
        realizer = 0;
        if (_disposed || _realizing) return Direct3D9Factory.WgxInvalidCallHResult;
        _realizing = true;
        bool initialized = false;
        try
        {
            if (_image is not null)
            {
                realizer = WpfGfxShape.Abi.NativeResourceBrushRealization.Create(this, session);
                context = context with { WorldToDevice = GeneratedImageTransform.Identity,
                    BitmapScalingMode = 3, PrefilterEnabled = false };
            }
            else realizer = _immediate;
            initialized = true;
            return 0;
        }
        finally
        {
            if (!initialized) _realizing = false;
        }
    }

    internal void EndPrimitive(nint realizer)
    {
        try
        {
            if (_image is not null)
            {
                try
                {
                    if (realizer != 0)
                        ((delegate* unmanaged<nint, void>)(*(void***)realizer)[8])(realizer);
                    else FreeRealizationResources();
                }
                finally { WpfGfxShape.Abi.NativeImmediateSolidRealization.ReleaseOwner(realizer); }
            }
        }
        finally
        {
            _realizing = false;
            if (_disposed) ReleaseCapturedResources();
        }
    }

    internal nint Immediate => _immediate;

    internal int EnsureRealization(SoftwareImageRenderSession session)
    {
        if (_image is not { } image) return 0;
        FreeRealizationResources();
        nint candidate = 0;
        try
        {
            int hr = session.RealizeBrush(image, out candidate);
            if (hr < 0) return hr;
            if (_disposed) return Direct3D9Factory.WgxInvalidCallHResult;
            _immediate = candidate;
            candidate = 0;
            return hr;
        }
        finally { WpfGfxShape.Abi.NativeImmediateSolidRealization.ReleaseOwner(candidate); }
    }

    internal void FreeRealizationResources()
    {
        nint immediate = _immediate;
        _immediate = 0;
        WpfGfxShape.Abi.NativeImmediateSolidRealization.ReleaseOwner(immediate);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // Foreign COM callbacks must not release a source while Ensure or consumption uses it.
        if (!_realizing) ReleaseCapturedResources();
    }

    private void ReleaseCapturedResources()
    {
        var image = _image;
        _image = null;
        try { FreeRealizationResources(); }
        finally { image?.Dispose(); }
    }
}
