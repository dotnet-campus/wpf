using System.Numerics;

namespace WpfGfxShape.Core;

internal sealed unsafe class SoftwareBitmapEffects : IDisposable
{
    private readonly record struct Entry(float Scale, SoftwareBitmapMask? Mask);
    private readonly List<Entry> _entries = [];

    internal static int Capture(nint effects, out SoftwareBitmapEffects? result)
    {
        result = null;
        if (effects == 0) return 0;
        var candidate = new SoftwareBitmapEffects();
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)effects)[1])(effects);
        try
        {
            uint count;
            int hr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)effects)[6])(effects, &count);
            if (hr < 0) return hr;
            candidate._entries.EnsureCapacity(checked((int)count));
            for (uint i = 0; i < count; i++)
            {
                Guid id; uint size, resources;
                hr = ((delegate* unmanaged[Stdcall]<nint, uint, Guid*, int>)(*(void***)effects)[7])(effects, i, &id);
                if (hr < 0) return hr;
                hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)(*(void***)effects)[8])(effects, i, &size);
                if (hr < 0) return hr;
                hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)(*(void***)effects)[10])(effects, i, &resources);
                if (hr < 0) return hr;
                if (id == new Guid("00000520-a8f2-4877-ba0a-fd2b6645fb94") && size == 4 && resources == 0)
                {
                    float scale;
                    hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, void*, int>)(*(void***)effects)[9])(effects, i, 4, &scale);
                    if (hr < 0) return hr;
                    if (!float.IsFinite(scale)) return Direct3D9Factory.InvalidArgumentHResult;
                    candidate._entries.Add(new(Math.Clamp(scale, 0, 1), null));
                }
                else if (id == new Guid("00000521-a8f2-4877-ba0a-fd2b6645fb94") && size == 64 && resources == 1)
                {
                    Matrix4x4 m;
                    hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, void*, int>)(*(void***)effects)[9])(effects, i, 64, &m);
                    if (hr < 0) return hr;
                    if (m.M13 != 0 || m.M14 != 0 || m.M23 != 0 || m.M24 != 0 || m.M31 != 0 || m.M32 != 0
                        || m.M33 != 1 || m.M34 != 0 || m.M43 != 0 || m.M44 != 1) return Direct3D9Factory.NotImplementedHResult;
                    var transform = new GeneratedImageTransform(m.M11, m.M22, m.M41, m.M42, m.M12, m.M21);
                    if (!transform.TryInvert(out _)) return Direct3D9Factory.InvalidArgumentHResult;
                    nint resource = 0, source = 0;
                    try
                    {
                        hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint*, int>)(*(void***)effects)[11])(effects, i, 1, &resource);
                        if (hr < 0) return hr;
                        if (resource == 0) return Direct3D9Factory.NoInterfaceHResult;
                        Guid sourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
                        hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)resource)[0])(resource, &sourceId, &source);
                        if (hr < 0) return hr;
                        if (source == 0) return Direct3D9Factory.NoInterfaceHResult;
                        candidate._entries.Add(new(1, new SoftwareBitmapMask(source, transform)));
                        source = 0;
                    }
                    finally { Direct3D9Factory.Release(source); Direct3D9Factory.Release(resource); }
                }
                else return Direct3D9Factory.NotImplementedHResult;
            }
            result = candidate;
            return 0;
        }
        finally
        {
            Direct3D9Factory.Release(effects);
            if (result is null) candidate.Dispose();
        }
    }

    internal int Prepare(SoftwareImageDrawingContext context, bool floating)
    {
        foreach (var entry in _entries)
        {
            if (entry.Mask is null) continue;
            int hr = entry.Mask.Prepare(context, floating);
            if (hr < 0) return hr;
        }
        return 0;
    }

    internal uint Apply(uint pixel, int x, int y)
    {
        foreach (var entry in _entries)
        {
            int factor = entry.Mask is { } mask
                ? (int)MathF.Round(mask.Alpha(x, y) * 255) * 257
                : (int)MathF.Round(entry.Scale * 65536);
            uint next = 0;
            for (int channel = 0; channel < 4; channel++)
                next |= (uint)(((byte)(pixel >> (channel * 8)) * factor + 32768) >> 16) << (channel * 8);
            pixel = next;
        }
        return pixel;
    }

    internal Vector4 Apply(Vector4 pixel, int x, int y)
    {
        foreach (var entry in _entries) pixel *= entry.Mask?.Alpha(x, y) ?? entry.Scale;
        return pixel;
    }

    public void Dispose()
    {
        for (int i = _entries.Count - 1; i >= 0; i--) _entries[i].Mask?.Dispose();
        _entries.Clear();
    }
}
