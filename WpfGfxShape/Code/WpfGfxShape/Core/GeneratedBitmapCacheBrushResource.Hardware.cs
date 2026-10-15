using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal sealed partial class GeneratedBitmapCacheBrushResource
{
    internal sealed class HardwareCache : Direct3D9Resource, IDisposable
    {
        private int _references = 1;
        private readonly WeakReference<GeneratedBitmapCacheBrushResource> _owner;
        internal Direct3D9TextureRenderTarget Target { get; }
        internal Direct3D9Device CacheDevice { get; }
        internal HardwareKey Key { get; }
        internal HardwareCache(GeneratedBitmapCacheBrushResource owner, Direct3D9TextureRenderTarget target,
            Direct3D9Device device, HardwareKey key) : base(device.ResourceManager)
        {
            _owner = new(owner);
            Target = target; CacheDevice = device; Key = key;
        }
        internal HardwareCache AddRef()
        {
            ObjectDisposedException.ThrowIf(_references == 0 || !IsValid, this);
            checked { _references++; }
            return this;
        }
        public new void Dispose()
        {
            if (_references == 0 || --_references != 0) return;
            Target.Dispose();
            base.Dispose();
        }
        protected override void ReleaseD3DResources()
        {
            // The manager releases owning targets before the native device, even when a lease survives.
            if (_owner.TryGetTarget(out var owner)
                && owner._hardwareCaches.TryGetValue(CacheDevice, out var cached)
                && ReferenceEquals(cached, this))
            {
                owner._hardwareCaches.Remove(CacheDevice);
                Dispose();
            }
            Target.Dispose();
        }
    }

    internal readonly record struct HardwareKey(int Brush, int Target, int Config, double Scale,
        MilRectD Bounds, Format Format, uint Scaling);
    private readonly Dictionary<Direct3D9Device, HardwareCache> _hardwareCaches = [];
    private bool _hardwareCapturing;

    private void InvalidateHardwareCache()
    {
        var previous = _hardwareCaches.Values.ToArray();
        _hardwareCaches.Clear();
        foreach (var cache in previous) cache.Dispose();
    }

    internal int AcquireHardwareCache(FrozenCacheRequest request, Direct3D9Device device, Format format,
        uint scaling, out HardwareCache? cache, out bool candidate)
    {
        cache = null; candidate = false;
        if (_hardwareCapturing) return Direct3D9Factory.NotImplementedHResult;
        if (!double.IsFinite(request.Scale) || request.Scale < 0) return Direct3D9Factory.InvalidArgumentHResult;
        if (request.Snaps || request.ClearType) return Direct3D9Factory.NotImplementedHResult;
        if (request.Tree is not { } tree || request.Scale == 0) return 0;
        if (!GeneratedImageContentBounds.TryGet(tree, out var bounds)) return Direct3D9Factory.NotImplementedHResult;
        if (bounds.Width <= 0 || bounds.Height <= 0) return 0;
        var key = new HardwareKey(request.BrushChange, request.TargetChange, request.ConfigChange,
            request.Scale, bounds, format, scaling);
        if (_hardwareCaches.TryGetValue(device, out var hit) && hit.IsValid && hit.Key == key && hit.Target.IsValid)
        { cache = hit.AddRef(); return 0; }
        double w = Math.Ceiling(bounds.Width * request.Scale), h = Math.Ceiling(bounds.Height * request.Scale);
        if (!double.IsFinite(w) || !double.IsFinite(h) || w > int.MaxValue || h > int.MaxValue)
            return Direct3D9Factory.InvalidArgumentHResult;
        _hardwareCapturing = true;
        Direct3D9TextureRenderTarget? target = null;
        try
        {
            int hr = Direct3D9TextureRenderTarget.TryCreate((uint)Math.Max(1, w), (uint)Math.Max(1, h), device, null,
                out target, format == Format.A32B32G32R32f ? MilPixelFormat.Prgba128BppFloat : MilPixelFormat.Pbgra32Bpp);
            if (hr < 0) return hr;
            if (target is null) return Direct3D9Factory.InvalidCallHResult;
            hr = target.Clear(new(0, 0, 0, 0), null);
            if (hr >= 0) hr = GeneratedHardwareVisualRenderer.Visit(tree, device, target.SurfaceRenderTarget,
                new(request.Scale, request.Scale, -bounds.X * request.Scale, -bounds.Y * request.Scale),
                new(0, 0, (int)Math.Max(1, w), (int)Math.Max(1, h)), 0);
            if (hr < 0) return hr;
            cache = new(this, target, device, key);
            target = null;
            candidate = true;
            return 0;
        }
        finally { target?.Dispose(); _hardwareCapturing = false; }
    }

    internal void CommitHardwareCache(FrozenCacheRequest request, HardwareCache candidate)
    {
        if (IsReleased || !candidate.IsValid || !candidate.Target.IsValid || ChangeCount != request.BrushChange || !ReferenceEquals(InternalTarget, request.Target)
            || !ReferenceEquals(BitmapCache, request.Config) || (request.Target?.ChangeCount ?? 0) != request.TargetChange
            || (request.Config?.ChangeCount ?? 0) != request.ConfigChange) return;
        _hardwareCaches.TryGetValue(candidate.CacheDevice, out var previous);
        _hardwareCaches[candidate.CacheDevice] = candidate.AddRef();
        previous?.Dispose();
    }
}
