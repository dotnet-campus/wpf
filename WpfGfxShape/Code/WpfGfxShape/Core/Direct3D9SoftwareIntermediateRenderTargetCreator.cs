namespace WpfGfxShape.Core;

internal delegate int Direct3D9AcquireCurrentDisplaySet(out Direct3D9DisplaySet? displaySet);

internal sealed class Direct3D9SoftwareIntermediateRenderTargetCreator : IDisposable
{
    private const uint MaxIntToFloat = 1u << 24;

    private readonly MilPixelFormat _targetPixelFormat;
    private readonly uint? _associatedDisplayId;
    private readonly Direct3D9AcquireCurrentDisplaySet _acquireCurrentDisplaySet;
    private readonly Action<Direct3D9DisplaySet> _releaseDisplaySet;
    private bool _isDisposed;

    internal Direct3D9SoftwareIntermediateRenderTargetCreator(
        MilPixelFormat targetPixelFormat,
        uint? associatedDisplayId,
        Direct3D9AcquireCurrentDisplaySet? acquireCurrentDisplaySet = null,
        Action<Direct3D9DisplaySet>? releaseDisplaySet = null)
    {
        _targetPixelFormat = targetPixelFormat;
        _associatedDisplayId = associatedDisplayId;
        _acquireCurrentDisplaySet = acquireCurrentDisplaySet ?? AcquireUnavailableDisplaySet;
        _releaseDisplaySet = releaseDisplaySet ?? (_ => { });
    }

    internal int CreateRenderTargetBitmap(
        uint width,
        uint height,
        Direct3D9IntermediateRenderTargetUsage usage,
        Direct3D9RenderTargetInitializationFlags initializationFlags,
        Direct3D9CreateSoftwareRenderTargetBitmapSurface createInternalSurface,
        out Direct3D9SoftwareRenderTargetBitmap? renderTargetBitmap,
        Direct3D9WrapSoftwareRenderTargetBitmap? wrapRenderTargetBitmap = null)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(createInternalSurface);
        renderTargetBitmap = null;

        if (width > MaxIntToFloat || height > MaxIntToFloat)
        {
            return Direct3D9Factory.UnsupportedTextureSizeHResult;
        }

        MilPixelFormat pixelFormat = _targetPixelFormat;
        if ((usage.Flags & Direct3D9IntermediateRenderTargetUsageFlags.ForBlending) != 0)
        {
            int formatResult = GetBestBlendingFormat(pixelFormat, out pixelFormat);
            if (formatResult < 0)
            {
                return formatResult;
            }
        }

        Direct3D9SoftwareRenderTargetBitmapCreationRequest request = new(
            width,
            height,
            pixelFormat,
            96f,
            96f,
            _associatedDisplayId,
            initializationFlags);

        int result = createInternalSurface(request, out nint internalSurface);
        if (result < 0 || internalSurface == 0)
        {
            if (internalSurface != 0)
            {
                Direct3D9Factory.Release(internalSurface);
            }

            return result < 0 ? result : Direct3D9Factory.UnexpectedHResult;
        }

        Direct3D9SoftwareRenderTargetBitmap? candidate = null;
        try
        {
            result = wrapRenderTargetBitmap is null
                ? WrapRenderTargetBitmap(internalSurface, request, out candidate)
                : wrapRenderTargetBitmap(internalSurface, request, out candidate);
            if (result < 0 || candidate is null)
            {
                candidate?.Dispose();
                return result < 0 ? result : Direct3D9Factory.UnexpectedHResult;
            }

            renderTargetBitmap = candidate;
            return result;
        }
        finally
        {
            Direct3D9Factory.Release(internalSurface);
        }
    }

    internal int ReadEnabledDisplays(Span<bool> enabledDisplays)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        Direct3D9DisplaySet? displaySet = null;
        try
        {
            int result = _acquireCurrentDisplaySet(out displaySet);
            if (result < 0)
            {
                return result;
            }

            if (displaySet is null)
            {
                return Direct3D9Factory.UnexpectedHResult;
            }

            int displayCount = displaySet.Characteristics.Displays.Length;
            if (enabledDisplays.Length != displayCount)
            {
                return Direct3D9Factory.InvalidArgumentHResult;
            }

            uint? enabledDisplayIndex = null;
            if (_associatedDisplayId is uint associatedDisplayId)
            {
                result = displaySet.GetDisplayIndexFromDisplayId(associatedDisplayId, out uint displayIndex);
                if (result < 0)
                {
                    return result;
                }

                enabledDisplayIndex = displayIndex;
            }

            bool[] pendingEnabledDisplays = new bool[displayCount];
            if (enabledDisplayIndex is uint index)
            {
                pendingEnabledDisplays[(int) index] = true;
            }

            pendingEnabledDisplays.CopyTo(enabledDisplays);
            return Direct3D9Factory.SuccessHResult;
        }
        finally
        {
            if (displaySet is not null)
            {
                _releaseDisplaySet(displaySet);
            }
        }
    }

    public void Dispose()
    {
        _isDisposed = true;
    }

    private static int AcquireUnavailableDisplaySet(out Direct3D9DisplaySet? displaySet)
    {
        displaySet = null;
        return Direct3D9Factory.NotInitializedHResult;
    }

    private static int WrapRenderTargetBitmap(
        nint internalSurface,
        Direct3D9SoftwareRenderTargetBitmapCreationRequest request,
        out Direct3D9SoftwareRenderTargetBitmap? renderTargetBitmap)
    {
        try
        {
            renderTargetBitmap = new Direct3D9SoftwareRenderTargetBitmap(internalSurface, request);
            return Direct3D9Factory.SuccessHResult;
        }
        catch (OutOfMemoryException)
        {
            renderTargetBitmap = null;
            return Direct3D9Factory.OutOfMemoryHResult;
        }
    }

    private static int GetBestBlendingFormat(MilPixelFormat pixelFormat, out MilPixelFormat blendingFormat)
    {
        if (pixelFormat is
            MilPixelFormat.Rgba128BppFloat or
            MilPixelFormat.Prgba128BppFloat or
            MilPixelFormat.Rgb128BppFloat or
            MilPixelFormat.Gray32BppFloat)
        {
            blendingFormat = MilPixelFormat.Prgba128BppFloat;
            return Direct3D9Factory.SuccessHResult;
        }

        if (pixelFormat is >= MilPixelFormat.Indexed1Bpp and <= MilPixelFormat.Prgba64Bpp)
        {
            blendingFormat = MilPixelFormat.Pbgra32Bpp;
            return Direct3D9Factory.SuccessHResult;
        }

        blendingFormat = MilPixelFormat.Undefined;
        return Direct3D9Factory.WinCodecInternalErrorHResult;
    }
}
