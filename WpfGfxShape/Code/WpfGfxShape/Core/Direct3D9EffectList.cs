using System.Numerics;

namespace WpfGfxShape.Core;

internal enum Direct3D9EffectType
{
    AlphaScale,
    AlphaMask,
}

internal readonly record struct Direct3D9AlphaMaskParameters(Matrix4x4 Transform);

internal sealed class Direct3D9EffectResource : IDisposable
{
    private readonly Action<nint> _release;
    private bool _disposed;

    internal Direct3D9EffectResource(nint handle, Action<nint> release)
    {
        ArgumentOutOfRangeException.ThrowIfZero(handle);
        ArgumentNullException.ThrowIfNull(release);
        Handle = handle;
        _release = release;
    }

    internal nint Handle { get; }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _release(Handle);
    }
}

internal sealed record Direct3D9EffectEntry(
    Direct3D9EffectType Type,
    object Parameters,
    IReadOnlyList<Direct3D9EffectResource> Resources);

internal delegate int Direct3D9RetainEffectResource(nint resource, out nint retainedResource);

internal delegate int Direct3D9DeriveEffectMaskColorSource(
    nint maskBitmap,
    Direct3D9AlphaMaskParameters parameters,
    Direct3D9PathBrushContext effectContext,
    out Direct3D9BitmapPipelineColorSource? colorSource);

internal sealed class Direct3D9EffectList : IDisposable
{
    private readonly List<Direct3D9EffectEntry> _entries = [];
    private bool _disposed;

    internal IReadOnlyList<Direct3D9EffectEntry> Entries
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _entries;
        }
    }

    internal int AddAlphaScale(float alpha)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!float.IsFinite(alpha) || alpha < 0 || alpha > 1)
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        try
        {
            _entries.Add(new Direct3D9EffectEntry(Direct3D9EffectType.AlphaScale, alpha, []));
            return Direct3D9Factory.SuccessHResult;
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }
    }

    internal int AddAlphaMask(
        Direct3D9AlphaMaskParameters parameters,
        nint maskBitmap,
        Direct3D9RetainEffectResource retainResource,
        Action<nint> releaseResource)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfZero(maskBitmap);
        ArgumentNullException.ThrowIfNull(retainResource);
        ArgumentNullException.ThrowIfNull(releaseResource);

        int result = retainResource(maskBitmap, out nint retainedResource);
        if (result < 0)
        {
            return result;
        }

        if (retainedResource == 0)
        {
            return Direct3D9Factory.InternalErrorHResult;
        }

        Direct3D9EffectResource? resource = null;
        bool ownsRetainedResource = true;
        try
        {
            resource = new Direct3D9EffectResource(retainedResource, releaseResource);
            _entries.Add(new Direct3D9EffectEntry(Direct3D9EffectType.AlphaMask, parameters, [resource]));
            resource = null;
            ownsRetainedResource = false;
            return Direct3D9Factory.SuccessHResult;
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }
        finally
        {
            if (resource is not null)
            {
                resource.Dispose();
            }
            else if (ownsRetainedResource)
            {
                releaseResource(retainedResource);
            }
        }
    }

    internal void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ReleaseEntries();
        _entries.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        ReleaseEntries();
        _entries.Clear();
    }

    private void ReleaseEntries()
    {
        for (int entryIndex = _entries.Count - 1; entryIndex >= 0; entryIndex--)
        {
            IReadOnlyList<Direct3D9EffectResource> resources = _entries[entryIndex].Resources;
            for (int resourceIndex = resources.Count - 1; resourceIndex >= 0; resourceIndex--)
            {
                resources[resourceIndex].Dispose();
            }
        }
    }
}

internal static class Direct3D9EffectProcessor
{
    internal static int Process(
        Direct3D9EffectList effectList,
        Direct3D9ShaderPipelineItemBuilder builder,
        Direct3D9PathBrushContext effectContext,
        Direct3D9DeriveEffectMaskColorSource deriveMaskColorSource)
    {
        ArgumentNullException.ThrowIfNull(effectList);
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(effectContext);
        ArgumentNullException.ThrowIfNull(deriveMaskColorSource);

        foreach (Direct3D9EffectEntry entry in effectList.Entries)
        {
            int result = entry.Type switch
            {
                Direct3D9EffectType.AlphaScale when entry.Parameters is float alpha && entry.Resources.Count == 0 =>
                    builder.MultiplyConstantAlpha(new Direct3D9ConstantAlphaScalableColorSource(alpha)),
                Direct3D9EffectType.AlphaMask when entry.Parameters is Direct3D9AlphaMaskParameters parameters && entry.Resources.Count == 1 =>
                    ProcessAlphaMask(builder, effectContext, parameters, entry.Resources[0].Handle, deriveMaskColorSource),
                _ => Direct3D9Factory.UnsupportedOperationHResult,
            };
            if (result < 0)
            {
                return result;
            }
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal static int Process(
        Direct3D9EffectList effectList,
        Direct3D9FixedFunctionPipelineItemBuilder builder,
        Direct3D9PathBrushContext effectContext,
        Direct3D9DeriveEffectMaskColorSource deriveMaskColorSource)
    {
        ArgumentNullException.ThrowIfNull(effectList);
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(effectContext);
        ArgumentNullException.ThrowIfNull(deriveMaskColorSource);

        foreach (Direct3D9EffectEntry entry in effectList.Entries)
        {
            int result = entry.Type switch
            {
                Direct3D9EffectType.AlphaScale when entry.Parameters is float alpha && entry.Resources.Count == 0 =>
                    builder.ProcessAlphaScaleEffect(sizeof(float), 0, alpha),
                Direct3D9EffectType.AlphaMask when entry.Parameters is Direct3D9AlphaMaskParameters parameters && entry.Resources.Count == 1 =>
                    ProcessAlphaMask(builder, effectContext, parameters, entry.Resources[0].Handle, deriveMaskColorSource),
                _ => Direct3D9Factory.UnsupportedOperationHResult,
            };
            if (result < 0)
            {
                return result;
            }
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private static int ProcessAlphaMask(
        Direct3D9ShaderPipelineItemBuilder builder,
        Direct3D9PathBrushContext effectContext,
        Direct3D9AlphaMaskParameters parameters,
        nint maskBitmap,
        Direct3D9DeriveEffectMaskColorSource deriveMaskColorSource)
    {
        int result = deriveMaskColorSource(maskBitmap, parameters, effectContext, out Direct3D9BitmapPipelineColorSource? colorSource);
        if (result < 0)
        {
            return result;
        }

        if (colorSource is null)
        {
            return Direct3D9Factory.InternalErrorHResult;
        }

        result = builder.MultiplyAlphaMask(colorSource);
        if (result < 0)
        {
            colorSource.Dispose();
        }

        return result;
    }

    private static int ProcessAlphaMask(
        Direct3D9FixedFunctionPipelineItemBuilder builder,
        Direct3D9PathBrushContext effectContext,
        Direct3D9AlphaMaskParameters parameters,
        nint maskBitmap,
        Direct3D9DeriveEffectMaskColorSource deriveMaskColorSource)
    {
        int result = deriveMaskColorSource(maskBitmap, parameters, effectContext, out Direct3D9BitmapPipelineColorSource? colorSource);
        if (result < 0)
        {
            return result;
        }

        if (colorSource is null)
        {
            return Direct3D9Factory.InternalErrorHResult;
        }

        result = builder.MultiplyAlphaMask(colorSource);
        if (result < 0)
        {
            colorSource.Dispose();
        }

        return result;
    }
}
