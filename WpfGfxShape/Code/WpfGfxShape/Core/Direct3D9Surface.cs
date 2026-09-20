using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9SurfaceDeviceContextResult(int HResult, nint DeviceContext);

internal sealed class Direct3D9TargetFormatTestStatus
{
    private int? _testHResult;
    private int? _getDeviceContextHResult;

    internal bool WasTested => _testHResult.HasValue;

    internal int TestHResult => _testHResult
        ?? throw new InvalidOperationException("The render-target format test has not run.");

    internal bool WasGetDeviceContextTested => _getDeviceContextHResult.HasValue;

    internal int GetDeviceContextHResult => _getDeviceContextHResult
        ?? throw new InvalidOperationException("The surface device-context test has not run.");

    internal int TestGetDeviceContext(
        Func<Direct3D9SurfaceDeviceContextResult> getDeviceContext,
        Func<nint, int> releaseDeviceContext,
        Func<int, int>? reinterpretFailure = null)
    {
        ArgumentNullException.ThrowIfNull(getDeviceContext);
        ArgumentNullException.ThrowIfNull(releaseDeviceContext);

        if (_getDeviceContextHResult.HasValue)
        {
            return _getDeviceContextHResult.Value;
        }

        Direct3D9SurfaceDeviceContextResult result = getDeviceContext();
        int hResult = reinterpretFailure?.Invoke(result.HResult) ?? result.HResult;
        _getDeviceContextHResult = hResult;

        if (result.DeviceContext != 0)
        {
            _ = releaseDeviceContext(result.DeviceContext);
        }

        return hResult;
    }

    internal int TestRenderTargetFormat(Func<Direct3D9TargetFormatTestStatus, int> test)
    {
        ArgumentNullException.ThrowIfNull(test);

        if (_testHResult.HasValue)
        {
            return _testHResult.Value;
        }

        int hResult = test(this);
        if (hResult is Direct3D9Factory.OutOfVideoMemoryHResult
            or Direct3D9Factory.OutOfMemoryHResult
            or Direct3D9Factory.DriverInternalErrorHResult)
        {
            return hResult;
        }

        _testHResult = hResult;
        if (hResult < 0 && !_getDeviceContextHResult.HasValue)
        {
            _getDeviceContextHResult = hResult;
        }

        return hResult;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9Surface : Direct3D9Resource
{
    private readonly Direct3D9ResourceManager _resourceManager;
    private readonly bool _notifyDeviceOnRelease;
    private IDirect3DSurface9* _surface;
    private SurfaceDesc? _description;

    internal Direct3D9Surface(Direct3D9ResourceManager resourceManager, IDirect3DSurface9* surface)
        : this(resourceManager, surface, notifyDeviceOnRelease: false)
    {
    }

    private Direct3D9Surface(
        Direct3D9ResourceManager resourceManager,
        IDirect3DSurface9* surface,
        bool notifyDeviceOnRelease,
        uint resourceSize = 0)
        : base(resourceManager, resourceSize: resourceSize)
    {
        _resourceManager = resourceManager;
        _notifyDeviceOnRelease = notifyDeviceOnRelease;
        _surface = surface;
    }

    internal static int TryCreate(
        Direct3D9ResourceManager resourceManager,
        IDirect3DSurface9* surface,
        out Direct3D9Surface? createdSurface)
    {
        return TryCreate(resourceManager, surface, addReference: false, countResourceSize: true, out createdSurface);
    }

    internal static int TryCreateTextureLevel(
        Direct3D9ResourceManager resourceManager,
        IDirect3DSurface9* surface,
        out Direct3D9Surface? textureSurface)
    {
        return TryCreate(resourceManager, surface, addReference: true, countResourceSize: false, out textureSurface);
    }

    internal static int TryCreateReference(
        Direct3D9ResourceManager resourceManager,
        IDirect3DSurface9* surface,
        out Direct3D9Surface? createdSurface)
    {
        return TryCreate(resourceManager, surface, addReference: true, countResourceSize: true, out createdSurface);
    }

    private static int TryCreate(
        Direct3D9ResourceManager resourceManager,
        IDirect3DSurface9* surface,
        bool addReference,
        bool countResourceSize,
        out Direct3D9Surface? createdSurface)
    {
        ArgumentNullException.ThrowIfNull(resourceManager);
        createdSurface = null;
        if (surface is null)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        void** vtable = surface->LpVtbl;
        if (addReference)
        {
            delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint> addRef =
                (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) vtable[1];
            _ = addRef(surface);
        }

        SurfaceDesc description = default;
        delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int> getDescription =
            (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) vtable[12];
        int result = getDescription(surface, &description);
        if (result < 0)
        {
            Direct3D9Factory.Release(surface);
            return result;
        }

        uint resourceSize = countResourceSize ? GetResourceSize(description) : 0;
        createdSurface = new Direct3D9Surface(
            resourceManager,
            surface,
            notifyDeviceOnRelease: !addReference,
            resourceSize)
        {
            _description = description
        };
        return result;
    }

    internal Direct3D9Surface AddRef()
    {
        ObjectDisposedException.ThrowIf(!IsValid || _surface is null, this);

        void** vtable = _surface->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint> addRef =
            (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) vtable[1];
        _ = addRef(_surface);

        return new Direct3D9Surface(_resourceManager, _surface, notifyDeviceOnRelease: false)
        {
            _description = _description
        };
    }

    internal IDirect3DSurface9* Surface
    {
        get
        {
            ObjectDisposedException.ThrowIf(!IsValid || _surface is null, this);
            return _surface;
        }
    }

    internal IDirect3DSurface9* SurfaceForDeviceCall
    {
        get
        {
            ObjectDisposedException.ThrowIf(!IsValid, this);
            return _surface;
        }
    }

    internal IDirect3DSurface9* NativeIdentity => _surface;

    internal int GetDevice(out nint device)
    {
        ObjectDisposedException.ThrowIf(!IsValid || _surface is null, this);

        device = 0;
        void** vtable = _surface->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint*, int> getDevice =
            (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint*, int>) vtable[3];
        fixed (nint* devicePointer = &device)
        {
            int result = getDevice(_surface, devicePointer);
            if (result < 0 && device != 0)
            {
                Direct3D9Factory.Release(device);
                device = 0;
            }

            return result;
        }
    }

    internal SurfaceDesc GetDescription()
    {
        ObjectDisposedException.ThrowIf(!IsValid || _surface is null, this);
        if (_description is SurfaceDesc cachedDescription)
        {
            return cachedDescription;
        }

        SurfaceDesc description = default;
        void** vtable = _surface->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int> getDescription =
            (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) vtable[12];
        int result = getDescription(_surface, &description);
        Marshal.ThrowExceptionForHR(result);
        return description;
    }

    internal int LockRect(out LockedRect lockedRect, Direct3D9SurfaceRect rectangle, uint flags)
    {
        ObjectDisposedException.ThrowIf(!IsValid || _surface is null, this);

        lockedRect = default;
        void** vtable = _surface->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSurface9*, LockedRect*, Direct3D9SurfaceRect*, uint, int> lockRect =
            (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, LockedRect*, Direct3D9SurfaceRect*, uint, int>) vtable[13];
        fixed (LockedRect* lockedRectPointer = &lockedRect)
        {
            return lockRect(_surface, lockedRectPointer, &rectangle, flags);
        }
    }

    internal int UnlockRect()
    {
        ObjectDisposedException.ThrowIf(!IsValid || _surface is null, this);

        void** vtable = _surface->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSurface9*, int> unlockRect =
            (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, int>) vtable[14];
        return unlockRect(_surface);
    }

    internal int ReadIntoSystemMemoryBuffer(
        Direct3D9BitmapRealizationRectangle sourceRectangle,
        IReadOnlyList<Direct3D9BitmapRealizationRectangle> clipRectangles,
        MilPixelFormat outputFormat,
        uint outputStride,
        uint outputBufferSize,
        nint outputBuffer)
    {
        ArgumentNullException.ThrowIfNull(clipRectangles);
        ObjectDisposedException.ThrowIf(!IsValid || _surface is null, this);
        if (!_resourceManager.IsInUseContext || outputBuffer == 0)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        SurfaceDesc description = GetDescription();
        if (sourceRectangle.Left >= sourceRectangle.Right
            || sourceRectangle.Top >= sourceRectangle.Bottom
            || sourceRectangle.Right > description.Width
            || sourceRectangle.Bottom > description.Height)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        byte bitsPerPixel = MilPixelFormatInfo.GetBitsPerPixel(outputFormat);
        Format outputDirect3DFormat = MilPixelFormatInfo.ToDirect3DFormat(outputFormat);
        if (bitsPerPixel == 0 || (bitsPerPixel % 8) != 0 || outputDirect3DFormat == Format.Unknown)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        uint bytesPerPixel = (uint) bitsPerPixel / 8;
        uint copyStride;
        ulong requiredBufferSize;
        try
        {
            copyStride = checked(sourceRectangle.Width * bytesPerPixel);
            requiredBufferSize = checked(((ulong) outputStride * (sourceRectangle.Height - 1)) + copyStride);
        }
        catch (OverflowException)
        {
            return Direct3D9Factory.ArithmeticOverflowHResult;
        }

        if (outputStride < copyStride || requiredBufferSize > outputBufferSize)
        {
            return Direct3D9Factory.InsufficientBufferHResult;
        }

        Direct3D9Device? device = _resourceManager.Device;
        bool usesOriginalSurface = description.Pool is Pool.Managed or Pool.Systemmem;
        if (usesOriginalSurface && outputDirect3DFormat != description.Format)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }
        if (!usesOriginalSurface && device is null)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        Direct3D9SystemMemoryUpdateSurface? systemMemorySurface = null;
        Direct3D9UntrackedSurface? temporaryRenderTarget = null;
        bool manuallyCopyBits = true;
        try
        {
            if (!usesOriginalSurface)
            {
                void* pixels = null;
                if (Direct3D9HardwareCapabilities.HasWddmSupport(device!.Capabilities)
                    && outputStride == copyStride
                    && clipRectangles.Count == 0)
                {
                    manuallyCopyBits = false;
                    pixels = (void*) outputBuffer;
                }

                int deviceResult = device.TryCreateSystemMemoryUpdateSurface(
                    sourceRectangle.Width,
                    sourceRectangle.Height,
                    outputDirect3DFormat,
                    pixels,
                    out systemMemorySurface);
                if (deviceResult < 0)
                {
                    return deviceResult;
                }

                IDirect3DSurface9* readbackSource = _surface;
                if (sourceRectangle.Width != description.Width
                    || sourceRectangle.Height != description.Height
                    || outputDirect3DFormat != description.Format)
                {
                    deviceResult = device.CheckRenderTargetFormat(description.Format);
                    if (deviceResult < 0)
                    {
                        return deviceResult;
                    }

                    deviceResult = device.TryCreateRenderTargetUntracked(
                        sourceRectangle.Width,
                        sourceRectangle.Height,
                        description.Format,
                        description.MultiSampleType,
                        description.MultiSampleQuality,
                        lockable: false,
                        out temporaryRenderTarget);
                    if (deviceResult < 0)
                    {
                        return deviceResult;
                    }

                    Direct3D9SurfaceRect source = ToSurfaceRect(sourceRectangle);
                    Direct3D9SurfaceRect destination = new(
                        0,
                        0,
                        checked((int) sourceRectangle.Width),
                        checked((int) sourceRectangle.Height));
                    deviceResult = device.StretchRect(
                        (nint) _surface,
                        source,
                        (nint) temporaryRenderTarget!.Surface,
                        destination);
                    if (deviceResult < 0)
                    {
                        return deviceResult;
                    }

                    readbackSource = temporaryRenderTarget.Surface;
                }

                deviceResult = device.GetRenderTargetData(readbackSource, systemMemorySurface!.Surface);
                if (deviceResult < 0)
                {
                    return deviceResult;
                }
            }

            if (!manuallyCopyBits)
            {
                return Direct3D9Factory.SuccessHResult;
            }

            Direct3D9SurfaceRect lockRectangle = new(
                0,
                0,
                checked((int) sourceRectangle.Width),
                checked((int) sourceRectangle.Height));
            int lockResult = usesOriginalSurface
                ? LockRect(out LockedRect lockedRect, ToSurfaceRect(sourceRectangle), (uint) D3D9.LockReadonly)
                : systemMemorySurface!.LockRect(out lockedRect, lockRectangle, (uint) D3D9.LockReadonly);
            if (lockResult < 0)
            {
                return lockResult;
            }

            int result = Direct3D9Factory.SuccessHResult;
            try
            {
                IReadOnlyList<Direct3D9BitmapRealizationRectangle> copies = clipRectangles.Count == 0
                    ? [sourceRectangle]
                    : clipRectangles;
                foreach (Direct3D9BitmapRealizationRectangle clipRectangle in copies)
                {
                    if (!TryIntersect(sourceRectangle, clipRectangle, out Direct3D9BitmapRealizationRectangle copyRectangle))
                    {
                        continue;
                    }

                    uint leftInset = checked((copyRectangle.Left - sourceRectangle.Left) * bytesPerPixel);
                    uint topInset = copyRectangle.Top - sourceRectangle.Top;
                    uint rowByteCount = checked(copyRectangle.Width * bytesPerPixel);
                    byte* destination = (byte*) outputBuffer + checked((nint) (((ulong) outputStride * topInset) + leftInset));
                    byte* source = (byte*) lockedRect.PBits + checked((nint) (((ulong) lockedRect.Pitch * topInset) + leftInset));
                    for (uint row = 0; row < copyRectangle.Height; row++)
                    {
                        Buffer.MemoryCopy(source, destination, rowByteCount, rowByteCount);
                        destination += outputStride;
                        source += lockedRect.Pitch;
                    }
                }
            }
            catch (OverflowException)
            {
                result = Direct3D9Factory.ArithmeticOverflowHResult;
            }
            finally
            {
                _ = usesOriginalSurface ? UnlockRect() : systemMemorySurface!.UnlockRect();
            }

            return result;
        }
        catch (OverflowException)
        {
            return Direct3D9Factory.ArithmeticOverflowHResult;
        }
        finally
        {
            systemMemorySurface?.Dispose();
            temporaryRenderTarget?.Dispose();
        }
    }

    private static Direct3D9SurfaceRect ToSurfaceRect(Direct3D9BitmapRealizationRectangle rectangle) => new(
        checked((int) rectangle.Left),
        checked((int) rectangle.Top),
        checked((int) rectangle.Right),
        checked((int) rectangle.Bottom));

    private static bool TryIntersect(
        Direct3D9BitmapRealizationRectangle first,
        Direct3D9BitmapRealizationRectangle second,
        out Direct3D9BitmapRealizationRectangle intersection)
    {
        uint left = Math.Max(first.Left, second.Left);
        uint top = Math.Max(first.Top, second.Top);
        uint right = Math.Min(first.Right, second.Right);
        uint bottom = Math.Min(first.Bottom, second.Bottom);
        intersection = left < right && top < bottom
            ? new Direct3D9BitmapRealizationRectangle(left, top, right, bottom)
            : default;
        return left < right && top < bottom;
    }

    internal int TestGetDeviceContext(
        Direct3D9TargetFormatTestStatus testStatus,
        Func<int, int>? reinterpretFailure = null)
    {
        ArgumentNullException.ThrowIfNull(testStatus);
        ObjectDisposedException.ThrowIf(!IsValid || _surface is null, this);

        return testStatus.TestGetDeviceContext(GetDeviceContext, ReleaseDeviceContext, reinterpretFailure);
    }

    internal Direct3D9SurfaceDeviceContextResult GetDeviceContext()
    {
        ObjectDisposedException.ThrowIf(!IsValid || _surface is null, this);

        nint deviceContext = 0;
        void** vtable = _surface->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint*, int> getDeviceContext =
            (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint*, int>) vtable[15];
        int result = getDeviceContext(_surface, &deviceContext);
        result = Direct3D9GuiHandleQuota.ReinterpretGetDeviceContextFailure(result);
        if (result < 0 && deviceContext != 0)
        {
            _ = ReleaseDeviceContext(deviceContext);
            deviceContext = 0;
        }

        return new Direct3D9SurfaceDeviceContextResult(result, deviceContext);
    }

    internal int ReleaseDeviceContext(nint deviceContext)
    {
        ObjectDisposedException.ThrowIf(!IsValid || _surface is null, this);
        ArgumentOutOfRangeException.ThrowIfZero(deviceContext);

        void** vtable = _surface->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint, int> releaseDeviceContext =
            (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint, int>) vtable[16];
        return releaseDeviceContext(_surface, deviceContext);
    }

    private static uint GetResourceSize(SurfaceDesc description)
    {
        if (description.Pool == Pool.Systemmem)
        {
            return 0;
        }

        uint samplesPerPixel = description.MultiSampleType >= MultisampleType.Multisample2Samples
            ? (uint) description.MultiSampleType
            : 1;
        uint formatSize = description.Format switch
        {
            Format.A32B32G32R32f => 16,
            Format.A8R8G8B8 or Format.X8R8G8B8 or Format.D24S8 or Format.A2R10G10B10 => 4,
            Format.R8G8B8 => 3,
            Format.R5G6B5 or Format.X1R5G5B5 or Format.D16 => 2,
            Format.P8 or Format.L8 => 1,
            _ => 0
        };
        return formatSize * description.Width * description.Height * samplesPerPixel;
    }

    protected override void ReleaseD3DResources()
    {
        if (_notifyDeviceOnRelease && _surface is not null && _description is SurfaceDesc description)
        {
            ResourceManagerSurfaceReleaseNotification((nint) _surface, description.Usage);
        }

        Direct3D9Factory.Release(_surface);
        _surface = null;
    }

    private void ResourceManagerSurfaceReleaseNotification(nint surface, uint usage)
    {
        _resourceManager.SurfaceReleaseNotification?.Invoke(surface, usage);
    }
}
