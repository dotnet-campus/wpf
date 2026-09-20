using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9LockedPixelBuffer(byte[] Pixels, int Offset, int Pitch);

internal delegate int Direct3D9LockSoftware3DSurface(
    Direct3D9SurfaceRect bounds,
    out Direct3D9LockedPixelBuffer lockedBuffer);

internal sealed class Direct3D9Software3DSurface
{
    private const int TargetPixelSize = 4;

    private uint _width;
    private uint _height;
    private uint _newWidth;
    private uint _newHeight;
    private readonly bool _composeWithCopy;
    private readonly Func<int> _ensureSurface;
    private readonly Direct3D9LockSoftware3DSurface _lockSurface;
    private readonly Func<int> _unlockSurface;
    private readonly Func<float, bool, int> _begin3DInternal;
    private readonly Func<int> _end3D;
    private readonly Func<Direct3D9SurfaceRect, Direct3D9LockedPixelBuffer, int> _blendWithSoftwareTarget;
    private readonly Direct3D9Device? _device;
    private Direct3D9SurfaceRect _bounds;
    private Direct3D9SurfaceRect _boundsPre3D;
    private bool _isRenderingEnabled;
    private bool _in3D;
    private bool _surfaceDirty;

    internal Direct3D9Software3DSurface(
        uint width,
        uint height,
        MilPixelFormat targetPixelFormat,
        Func<int> ensureSurface,
        Direct3D9LockSoftware3DSurface lockSurface,
        Func<int> unlockSurface,
        Func<float, bool, int> begin3DInternal,
        Func<int> end3D,
        Func<Direct3D9SurfaceRect, Direct3D9LockedPixelBuffer, int> blendWithSoftwareTarget,
        Direct3D9Device? device = null)
    {
        ArgumentNullException.ThrowIfNull(ensureSurface);
        ArgumentNullException.ThrowIfNull(lockSurface);
        ArgumentNullException.ThrowIfNull(unlockSurface);
        ArgumentNullException.ThrowIfNull(begin3DInternal);
        ArgumentNullException.ThrowIfNull(end3D);
        ArgumentNullException.ThrowIfNull(blendWithSoftwareTarget);

        _width = width;
        _height = height;
        _newWidth = width;
        _newHeight = height;
        _composeWithCopy = targetPixelFormat is MilPixelFormat.Bgr32Bpp or MilPixelFormat.Pbgra32Bpp;
        _ensureSurface = ensureSurface;
        _lockSurface = lockSurface;
        _unlockSurface = unlockSurface;
        _begin3DInternal = begin3DInternal;
        _end3D = end3D;
        _blendWithSoftwareTarget = blendWithSoftwareTarget;
        _device = device;
        _bounds = new Direct3D9SurfaceRect(0, 0, checked((int) width), checked((int) height));
        _isRenderingEnabled = width != 0 && height != 0;
    }

    internal bool In3D => _in3D;

    internal bool SurfaceDirty => _surfaceDirty;

    internal Direct3D9SurfaceRect Bounds => _bounds;

    internal bool IsRenderingEnabled => _isRenderingEnabled;

    internal static int TryCreate(
        Direct3D9DeviceManager deviceManager,
        uint width,
        uint height,
        MilPixelFormat targetPixelFormat,
        Func<Direct3D9Device, MilPixelFormat, Direct3D9Software3DSurface> createSurface,
        out Direct3D9Software3DSurface? softwareSurface)
    {
        ArgumentNullException.ThrowIfNull(deviceManager);
        ArgumentNullException.ThrowIfNull(createSurface);

        softwareSurface = null;
        try
        {
            Direct3D9Device device = deviceManager.GetSoftwareDevice();
            MilPixelFormat surfacePixelFormat = targetPixelFormat is MilPixelFormat.Bgr32Bpp or MilPixelFormat.Pbgra32Bpp
                ? targetPixelFormat
                : MilPixelFormat.Pbgra32Bpp;
            Format surfaceFormat = surfacePixelFormat == MilPixelFormat.Bgr32Bpp
                ? Format.X8R8G8B8
                : Format.A8R8G8B8;

            int result = deviceManager.CheckRenderTargetFormat(device, surfaceFormat);
            if (result < 0)
            {
                return result;
            }

            Direct3D9Software3DSurface candidate = createSurface(device, surfacePixelFormat);
            candidate.Resize(width, height);
            softwareSurface = candidate;
            return Direct3D9Factory.SuccessHResult;
        }
        catch (COMException exception)
        {
            return exception.HResult;
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }
    }

    internal void Resize(uint width, uint height)
    {
        if (width == 0 || height == 0)
        {
            _isRenderingEnabled = false;
            return;
        }

        _newWidth = width;
        _newHeight = height;
        _isRenderingEnabled = true;
    }

    internal int BeginSw3D(
        byte[] targetPixels,
        int targetStride,
        Direct3D9SurfaceRect bounds,
        bool useZBuffer,
        float? z)
    {
        ArgumentNullException.ThrowIfNull(targetPixels);
        if (_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        int result = _ensureSurface();
        if (result < 0)
        {
            return result;
        }

        if (_width != _newWidth || _height != _newHeight)
        {
            _width = _newWidth;
            _height = _newHeight;
            _bounds = new Direct3D9SurfaceRect(0, 0, checked((int) _width), checked((int) _height));
        }

        _boundsPre3D = _bounds;
        _bounds = Intersect(_bounds, bounds);
        if (!IsEmpty(_bounds))
        {
            result = InitializeSurface(targetPixels, targetStride);
            if (result >= 0 && z.HasValue)
            {
                result = _begin3DInternal(z.Value, useZBuffer);
            }
        }

        if (result < 0)
        {
            _bounds = _boundsPre3D;
            return result;
        }

        _in3D = true;
        _surfaceDirty = false;
        return result;
    }

    internal void CleanupFreedResources()
    {
        if (_device is null)
        {
            return;
        }

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        _device.CleanupFreedResources();
    }

    internal int DrawMesh3D(Func<int> drawMesh3D)
    {
        ArgumentNullException.ThrowIfNull(drawMesh3D);
        if (!_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        if (IsEmpty(_bounds))
        {
            return Direct3D9Factory.SuccessHResult;
        }

        int result = drawMesh3D();
        if (result >= 0)
        {
            _surfaceDirty = true;
        }

        return result;
    }

    internal int EndSw3D(byte[] targetPixels, int targetStride)
    {
        ArgumentNullException.ThrowIfNull(targetPixels);
        if (!_in3D)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        Direct3D9SurfaceRect bounds3D = _bounds;
        int result;
        try
        {
            result = _end3D();
            if (result >= 0 && _surfaceDirty)
            {
                result = CompositeWithSoftwareTarget(targetPixels, targetStride, bounds3D);
            }
        }
        finally
        {
            _in3D = false;
            _bounds = _boundsPre3D;
        }

        return result;
    }

    private int InitializeSurface(byte[] targetPixels, int targetStride)
    {
        int result = _lockSurface(_bounds, out Direct3D9LockedPixelBuffer lockedBuffer);
        if (result < 0)
        {
            return result;
        }

        result = ValidateTargetBuffer(targetPixels, targetStride, _bounds);
        if (result >= 0)
        {
            result = ValidateLockedBuffer(lockedBuffer, _bounds);
        }

        if (result >= 0)
        {
            int rowBytes = (_bounds.Right - _bounds.Left) * TargetPixelSize;
            for (int y = _bounds.Top; y < _bounds.Bottom; y++)
            {
                int destinationOffset = lockedBuffer.Offset + (y - _bounds.Top) * lockedBuffer.Pitch;
                Span<byte> destination = lockedBuffer.Pixels.AsSpan(destinationOffset, rowBytes);
                if (_composeWithCopy)
                {
                    int sourceOffset = y * targetStride + _bounds.Left * TargetPixelSize;
                    targetPixels.AsSpan(sourceOffset, rowBytes).CopyTo(destination);
                }
                else
                {
                    destination.Clear();
                }
            }
        }

        int unlockResult = _unlockSurface();
        return result < 0 ? result : unlockResult;
    }

    private int CompositeWithSoftwareTarget(
        byte[] targetPixels,
        int targetStride,
        Direct3D9SurfaceRect bounds3D)
    {
        int result = _lockSurface(bounds3D, out Direct3D9LockedPixelBuffer lockedBuffer);
        if (result < 0)
        {
            return result;
        }

        result = ValidateTargetBuffer(targetPixels, targetStride, bounds3D);
        if (result >= 0)
        {
            result = ValidateLockedBuffer(lockedBuffer, bounds3D);
        }

        if (result >= 0)
        {
            if (_composeWithCopy)
            {
                int rowBytes = checked(bounds3D.Right - bounds3D.Left) * TargetPixelSize;
                for (int y = bounds3D.Top; y < bounds3D.Bottom; y++)
                {
                    int sourceOffset = checked(lockedBuffer.Offset + (y - bounds3D.Top) * lockedBuffer.Pitch);
                    int destinationOffset = checked(y * targetStride + bounds3D.Left * TargetPixelSize);
                    lockedBuffer.Pixels.AsSpan(sourceOffset, rowBytes)
                        .CopyTo(targetPixels.AsSpan(destinationOffset, rowBytes));
                }
            }
            else
            {
                result = _blendWithSoftwareTarget(bounds3D, lockedBuffer);
            }
        }

        int unlockResult = _unlockSurface();
        return result < 0 ? result : unlockResult;
    }

    private static int ValidateTargetBuffer(
        byte[] targetPixels,
        int targetStride,
        Direct3D9SurfaceRect bounds)
    {
        long requiredStride = (long) bounds.Right * TargetPixelSize;
        long requiredLength = ((long) bounds.Bottom - 1) * targetStride + requiredStride;
        if (bounds.Left < 0
            || bounds.Top < 0
            || bounds.Right < bounds.Left
            || bounds.Bottom < bounds.Top
            || targetStride < requiredStride
            || (bounds.Bottom > 0 && targetPixels.Length < requiredLength))
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private static int ValidateLockedBuffer(
        Direct3D9LockedPixelBuffer lockedBuffer,
        Direct3D9SurfaceRect bounds)
    {
        long rowBytes = (long) (bounds.Right - bounds.Left) * TargetPixelSize;
        int height = bounds.Bottom - bounds.Top;
        long requiredLength = ((long) height - 1) * lockedBuffer.Pitch + rowBytes;
        if (lockedBuffer.Pixels is null
            || lockedBuffer.Offset < 0
            || lockedBuffer.Pitch < rowBytes
            || height < 0
            || lockedBuffer.Offset > lockedBuffer.Pixels.Length
            || (height > 0 && lockedBuffer.Pixels.Length - lockedBuffer.Offset < requiredLength))
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private static Direct3D9SurfaceRect Intersect(
        Direct3D9SurfaceRect left,
        Direct3D9SurfaceRect right) => new(
            Math.Max(left.Left, right.Left),
            Math.Max(left.Top, right.Top),
            Math.Min(left.Right, right.Right),
            Math.Min(left.Bottom, right.Bottom));

    private static bool IsEmpty(Direct3D9SurfaceRect bounds) =>
        bounds.Left >= bounds.Right || bounds.Top >= bounds.Bottom;
}
