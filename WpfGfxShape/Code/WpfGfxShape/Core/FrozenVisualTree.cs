namespace WpfGfxShape.Core;

// Owns the pure visual topology and instruction storage; realization is a separate operation.
internal sealed class FrozenVisualTree : IDisposable
{
    private readonly List<GeneratedProtocolResource> _owners = [];
    private readonly List<FrozenGeometry> _geometries = [];
    private readonly List<FrozenBrush> _brushes = [];
    private readonly Dictionary<GeneratedProtocolResource, FrozenCacheRequest> _cacheRequests = [];
    internal FrozenCacheRequest? CacheRequest(GeneratedProtocolResource? resource)
        => resource is null ? null : _cacheRequests.GetValueOrDefault(resource)
            ?? _childrenDrawings.Select(tree => tree.CacheRequest(resource)).FirstOrDefault(request => request is not null);
    private readonly List<FrozenVisualTree> _children = [];
    private readonly List<FrozenVisualTree> _childrenDrawings = [];
    private bool _disposed;

    internal GeneratedVisualResource Owner { get; private set; } = null!;
    internal GeneratedProtocolResource? AlphaMask { get; private set; }
    internal FrozenShaderInvocation? Effect { get; private set; }
    internal FrozenBlurEffect? Blur { get; private set; }
    internal FrozenDropShadowEffect? Shadow { get; private set; }
    internal bool HasUnsupportedEffect { get; private set; }
    internal MilPoint2D Offset { get; private set; }
    internal double Alpha { get; private set; }
    internal uint? BitmapScalingMode { get; private set; }
    internal MilCompositingMode? CompositingMode { get; private set; }
    internal GeneratedImageTransform Transform { get; private set; }
    internal FrozenGeometry? Clip { get; private set; }
    internal IReadOnlyList<FrozenVisualTree> Children => _children;
    internal IReadOnlyList<GeneratedRenderDataInstruction> Instructions { get; private set; } = [];

    internal static int CapturePure(GeneratedVisualResource visual,
        HashSet<GeneratedProtocolResource> active, out FrozenVisualTree? snapshot)
    {
        snapshot = null;
        if (active.Count >= 256 || !active.Add(visual)) return Direct3D9Factory.NotImplementedHResult;
        var candidate = new FrozenVisualTree();
        bool captured = false;
        try
        {
            candidate.Own(visual);
            candidate.Owner = visual;
            candidate.Offset = visual.Offset;
            candidate.Alpha = visual.Alpha;
            candidate.BitmapScalingMode = visual.BitmapScalingMode;
            candidate.CompositingMode = visual.CompositingMode;
            if (!GeneratedImageTransform.TryResolve(visual.Transform, out var transform))
                return Direct3D9Factory.NotImplementedHResult;
            candidate.Transform = transform;
            int hr = FrozenGeometry.CapturePure(visual.Clip, active, out var clip);
            if (hr < 0) return hr;
            candidate.Clip = clip;
            if (clip is not null) candidate._geometries.Add(clip);
            hr = candidate.CaptureDependency(visual.AlphaMask, active);
            if (hr < 0) return hr;
            candidate.AlphaMask = candidate.FrozenResource(visual.AlphaMask);
            candidate.HasUnsupportedEffect = visual.Effect is not (null or GeneratedShaderEffectResource or GeneratedBlurEffectResource or GeneratedDropShadowEffectResource);
            if (visual.Effect is GeneratedDropShadowEffectResource shadow)
            {
                candidate.Own(shadow);
                var shadowData = shadow.Capture();
                if (!shadowData.IsFinite) return Direct3D9Factory.InvalidArgumentHResult;
                candidate.Shadow = shadowData;
            }
            if (visual.Effect is GeneratedBlurEffectResource blur)
            {
                candidate.Own(blur);
                if (!double.IsFinite(blur.Radius)) return Direct3D9Factory.InvalidArgumentHResult;
                candidate.Blur = new(blur.Radius, blur.Data.Kernel, blur.Data.Bias);
            }
            if (visual.Effect is GeneratedShaderEffectResource { Data: { } effectData })
            {
                candidate.Effect = new FrozenShaderInvocation(effectData, active);
                if (candidate.Effect.CaptureResult < 0) return candidate.Effect.CaptureResult;
            }
            GeneratedRenderDataInstruction[] instructions;
            if (visual.Content is GeneratedRenderDataResource data)
                instructions = data.Instructions.ToArray();
            else if (!GeneratedDrawingInstructions.TryRead(visual.Content, out instructions))
                return Direct3D9Factory.NotImplementedHResult;
            var expanded = new List<GeneratedRenderDataInstruction>();
            int expandResult = ExpandDrawings(instructions, expanded, active);
            if (expandResult < 0) return expandResult;
            var copied = new GeneratedRenderDataInstruction[expanded.Count];
            for (int i = 0; i < expanded.Count; i++)
            {
                var instruction = expanded[i];
                var slots = instruction.ResourceSlots.ToArray();
                foreach (var resource in slots)
                {
                    hr = candidate.CaptureDependency(resource, active);
                    if (hr < 0) return hr;
                }
                for (int slot = 0; slot < slots.Length; slot++) slots[slot] = candidate.FrozenResource(slots[slot]);
                copied[i] = new(instruction.Kind, slots.OfType<GeneratedProtocolResource>().ToArray(),
                    instruction.Data.ToArray(), slots);
            }
            candidate.Instructions = copied;
            foreach (var child in visual.Children.ToArray())
            {
                hr = CapturePure(child, active, out var tree);
                if (hr < 0) return hr;
                if (tree is not null) candidate._children.Add(tree);
            }
            snapshot = candidate;
            captured = true;
            return 0;
        }
        finally
        {
            active.Remove(visual);
            if (!captured) candidate.Dispose();
        }
    }

    internal static int CaptureDrawingPure(GeneratedProtocolResource? drawing,
        HashSet<GeneratedProtocolResource> active, out FrozenVisualTree? snapshot)
    {
        snapshot = null;
        if (!GeneratedDrawingInstructions.TryRead(drawing, out var instructions)) return Direct3D9Factory.NotImplementedHResult;
        var data = GeneratedRenderDataResource.CreateStatic(instructions);
        var root = GeneratedVisualResource.CreateStatic(data);
        try { return CapturePure(root, active, out snapshot); }
        finally { root.Release(); data.Release(); }
    }

    private readonly Dictionary<GeneratedProtocolResource, GeneratedProtocolResource> _resources = [];
    private readonly Dictionary<GeneratedProtocolResource, FrozenBrush> _imageBrushes = [];
    internal FrozenBrush? ImageBrush(GeneratedProtocolResource resource)
        => _imageBrushes.GetValueOrDefault(resource)
            ?? _childrenDrawings.Select(tree => tree.ImageBrush(resource)).FirstOrDefault(brush => brush is not null);
    private GeneratedProtocolResource? FrozenResource(GeneratedProtocolResource? resource)
        => resource is null ? null : _resources.GetValueOrDefault(resource, resource);

    private static int ExpandDrawings(IReadOnlyList<GeneratedRenderDataInstruction> instructions,
        List<GeneratedRenderDataInstruction> output, HashSet<GeneratedProtocolResource> active)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Kind != GeneratedRenderDataKind.DrawDrawing)
            {
                output.Add(instruction);
                continue;
            }
            var drawing = instruction.ResourceSlots[0];
            if (drawing is null) continue;
            if (active.Count >= 256 || !active.Add(drawing)) return Direct3D9Factory.NotImplementedHResult;
            try
            {
                if (!GeneratedDrawingInstructions.TryRead(drawing, out var nested))
                    return Direct3D9Factory.NotImplementedHResult;
                int hr = ExpandDrawings(nested, output, active);
                if (hr < 0) return hr;
            }
            finally { active.Remove(drawing); }
        }
        return 0;
    }

    private int CaptureDependency(GeneratedProtocolResource? resource, HashSet<GeneratedProtocolResource> active)
    {
        if (resource is null || _resources.ContainsKey(resource) || _imageBrushes.ContainsKey(resource)
            || _cacheRequests.ContainsKey(resource)) return 0;
        Own(resource);
        if (resource is GeneratedSolidColorBrushResource solid)
        {
            var clone = solid.CaptureValue();
            _owners.Add(clone);
            _resources.Add(resource, clone);
            return 0;
        }
        GeneratedProtocolResource? value = resource switch
        {
            GeneratedValueResource<double> animation => animation.CaptureValue(),
            GeneratedValueResource<MilRectD> animation => animation.CaptureValue(),
            GeneratedValueResource<MilPoint2D> animation => animation.CaptureValue(),
            GeneratedValueResource<MilColorF> animation => animation.CaptureValue(),
            GeneratedDashStyleResource dash => dash.CaptureValue(),
            GeneratedGlyphRunResource glyph => glyph.CaptureValue(),
            _ => null
        };
        if (resource is GeneratedPenResource pen)
        {
            int hr = CaptureDependency(pen.Brush, active);
            if (hr < 0) return hr;
            hr = CaptureDependency(pen.DashStyle, active);
            if (hr < 0) return hr;
            value = pen.CaptureValue(FrozenResource(pen.Brush), FrozenResource(pen.DashStyle) as GeneratedDashStyleResource);
        }
        if (resource.ResourceType is >= MilResourceType.TransformGroup and <= MilResourceType.MatrixTransform)
        {
            if (!GeneratedImageTransform.TryResolve(resource, out var matrix)) return Direct3D9Factory.NotImplementedHResult;
            value = GeneratedMatrixTransformResource.CreateStatic(new(matrix.ScaleX, matrix.M12, matrix.M21, matrix.ScaleY, matrix.X, matrix.Y));
        }
        if (value is not null)
        {
            _owners.Add(value);
            _resources.Add(resource, value);
            return 0;
        }
        if (resource is GeneratedDrawingImageResource drawingImage)
        {
            if (active.Count >= 256 || !active.Add(resource)) return Direct3D9Factory.NotImplementedHResult;
            try
            {
                int hr = CaptureDrawingPure(drawingImage.Drawing, active, out var drawing);
                if (hr < 0) return hr;
                _childrenDrawings.Add(drawing!);
                var data = GeneratedRenderDataResource.CreateStatic(drawing!.Instructions);
                _owners.Add(data);
                var clone = GeneratedDrawingImageResource.CreateStatic(data);
                _owners.Add(clone);
                _resources.Add(resource, clone);
                return 0;
            }
            finally { active.Remove(resource); }
        }
        if (resource is GeneratedBitmapSourceResource or GeneratedDoubleBufferedBitmapResource or GeneratedD3DImageResource)
        {
            int hr = FrozenBrush.CaptureSourcePure(resource, out var source);
            if (hr < 0) return hr;
            _brushes.Add(source!);
            _imageBrushes.Add(resource, source!);
            return 0;
        }
        if (resource is GeneratedBitmapCacheBrushResource cache)
        {
            int hr = FrozenCacheRequest.CapturePure(cache, active, out var request);
            if (hr < 0) return hr;
            _cacheRequests.Add(resource, request!);
            return 0;
        }
        if (resource is GeneratedTileBrushResource tile)
        {
            int hr = FrozenBrush.CapturePure(tile, out var brush, active);
            if (hr < 0) return hr;
            if (brush is not null)
            {
                _brushes.Add(brush);
                _imageBrushes.Add(resource, brush);
            }
        }
        else if (resource is GeneratedGeometryResource)
        {
            int hr = FrozenGeometry.CapturePure(resource, active, out var geometry);
            if (hr < 0) return hr;
            if (geometry is not null)
            {
                _geometries.Add(geometry);
                if (geometry.Geometry is { } detached) _resources.Add(resource, detached);
            }
        }
        return 0;
    }

    private void Own(GeneratedProtocolResource? resource)
    {
        if (resource is null) return;
        _owners.Add(resource);
        resource.AddRef();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Effect?.Dispose();
        foreach (var request in _cacheRequests.Values.Reverse()) request.Dispose();
        _cacheRequests.Clear();
        for (int i = _childrenDrawings.Count - 1; i >= 0; i--) _childrenDrawings[i].Dispose();
        for (int i = _children.Count - 1; i >= 0; i--) _children[i].Dispose();
        for (int i = _brushes.Count - 1; i >= 0; i--) _brushes[i].Dispose();
        for (int i = _geometries.Count - 1; i >= 0; i--) _geometries[i].Dispose();
        for (int i = _owners.Count - 1; i >= 0; i--) _owners[i].Release();
        _children.Clear();
        _brushes.Clear();
        _geometries.Clear();
        _owners.Clear();
    }
}
