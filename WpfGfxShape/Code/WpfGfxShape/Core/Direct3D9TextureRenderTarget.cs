using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed class Direct3D9TextureRenderTargetBitmapSource : IDisposable
{
    private Direct3D9Texture? _texture;
    private int _referenceCount = 1;

    internal Direct3D9TextureRenderTargetBitmapSource(Direct3D9Texture texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        _texture = texture;
    }

    internal bool IsReleased => _texture is null;

    internal Direct3D9Texture Texture => _texture
        ?? throw new ObjectDisposedException(nameof(Direct3D9TextureRenderTargetBitmapSource));

    internal bool HasValidContents { get; private set; } = true;

    internal void InvalidateContents()
    {
        ObjectDisposedException.ThrowIf(_texture is null, this);
        HasValidContents = false;
    }

    internal void ValidateContents()
    {
        ObjectDisposedException.ThrowIf(_texture is null, this);
        HasValidContents = true;
    }

    internal Direct3D9TextureRenderTargetBitmapSource AddRef()
    {
        ObjectDisposedException.ThrowIf(_texture is null, this);
        checked
        {
            _referenceCount++;
        }

        return this;
    }

    public void Dispose()
    {
        if (_texture is null)
        {
            return;
        }

        _referenceCount--;
        if (_referenceCount != 0)
        {
            return;
        }

        _texture.Dispose();
        _texture = null;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9TextureRenderTarget : IDisposable
{
    private readonly Direct3D9Device _device;
    private Direct3D9SurfaceRenderTarget? _surfaceRenderTarget;
    private Direct3D9Texture? _texture;
    private Direct3D9TextureRenderTargetBitmapSource? _deviceBitmap;
    private readonly Func<Direct3D9Texture, Direct3D9TextureRenderTargetBitmapSource> _bitmapSourceFactory;
    private bool _hasInvalidContents;
    private bool _isDisposed;

    internal Direct3D9TextureRenderTarget(
        Direct3D9Device device,
        Direct3D9SurfaceRenderTarget surfaceRenderTarget,
        Direct3D9Texture? texture,
        Func<Direct3D9Texture, Direct3D9TextureRenderTargetBitmapSource>? bitmapSourceFactory = null)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(surfaceRenderTarget);
        _device = device;
        _surfaceRenderTarget = surfaceRenderTarget;
        _texture = texture;
        _bitmapSourceFactory = bitmapSourceFactory ?? (static retainedTexture =>
            new Direct3D9TextureRenderTargetBitmapSource(retainedTexture));
    }

    internal bool IsValid
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _texture?.IsValid == true;
        }
    }

    internal Direct3D9Texture? GetTextureNoRef()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _texture;
    }

    internal MilPixelFormat GetPixelFormat()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.GetPixelFormat();
    }

    internal (uint Width, uint Height) GetSize()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.GetSize();
    }

    internal Matrix3x2 GetDeviceTransform()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.GetDeviceTransform();
    }

    internal Format GetDirect3DTextureFormat()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.GetDirect3DTextureFormat();
    }

    internal bool CanUseShaderPipeline()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.CanUseShaderPipeline();
    }

    internal uint GetRealizationCacheIndex()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.GetRealizationCacheIndex();
    }

    internal uint? GetDisplayId()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.GetDisplayId();
    }

    internal int SetClearTypeHint(bool forceClearType)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.SetClearTypeHint(forceClearType);
    }

    internal MilRectF GetBounds()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.GetBounds();
    }

    internal int Clear(MilColorF? color, Direct3D9SurfaceRect? aliasedClip)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.Clear(color, aliasedClip);
    }

    internal int Begin3D(MilRectF bounds, MilAntiAliasMode antiAliasMode, bool useZBuffer, float z)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.Begin3D(bounds, antiAliasMode, useZBuffer, z);
    }

    internal int End3D()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.End3D();
    }

    internal int FindInterface(Guid interfaceId, out nint interfacePointer)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.FindInterface(interfaceId, out interfacePointer);
    }

    internal InternalRenderTargetType GetRenderTargetType()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.GetRenderTargetType();
    }

    internal int GetNumQueuedPresents(out uint queuedPresentCount)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _surfaceRenderTarget!.GetNumQueuedPresents(out queuedPresentCount);
    }

    internal int DrawBitmap(Func<int> drawBitmap) => InvalidateContentsAndDraw(drawBitmap, static (target, draw) => target.DrawBitmap(draw));

    internal int DrawMesh3D(Func<int> drawMesh3D) => InvalidateContentsAndDraw(drawMesh3D, static (target, draw) => target.DrawMesh3D(draw));

    internal int DrawPath(Func<int> drawPath) => InvalidateContentsAndDraw(drawPath, static (target, draw) => target.DrawPath(draw));

    internal int DrawInfinitePath(Func<int> drawInfinitePath) => InvalidateContentsAndDraw(drawInfinitePath, static (target, draw) => target.DrawInfinitePath(draw));

    internal int DrawGlyphs(Func<int> drawGlyphs) => InvalidateContentsAndDraw(drawGlyphs, static (target, draw) => target.DrawGlyphs(draw));

    internal int DrawVideo(Func<int> drawVideo) => InvalidateContentsAndDraw(drawVideo, static (target, draw) => target.DrawVideo(draw));

    internal static int TryCreate(
        uint width,
        uint height,
        Direct3D9Device device,
        uint? associatedDisplayIndex,
        out Direct3D9TextureRenderTarget? renderTarget)
    {
        ArgumentNullException.ThrowIfNull(device);
        renderTarget = null;
        using Direct3D9DeviceEntryGuard deviceEntry = new(device);

        int result = device.CheckRenderTargetFormat(Format.A8R8G8B8);
        if (result < 0)
        {
            return result;
        }

        Direct3D9TextureRenderTarget? candidate = null;
        try
        {
            result = TryInitialize(width, height, device, associatedDisplayIndex, out candidate);
            if (result < 0)
            {
                candidate?.Dispose();
                return result;
            }

            renderTarget = candidate;
            candidate = null;
            return result;
        }
        catch (COMException exception)
        {
            candidate?.Dispose();
            return exception.HResult;
        }
        catch (OutOfMemoryException)
        {
            candidate?.Dispose();
            return Direct3D9Factory.OutOfMemoryHResult;
        }
    }

    internal static int GetSurfaceDescription(
        uint width,
        uint height,
        Direct3D9Device device,
        out SurfaceDesc description)
    {
        ArgumentNullException.ThrowIfNull(device);
        description = new SurfaceDesc(
            format: Format.A8R8G8B8,
            type: Resourcetype.Texture,
            usage: D3D9.UsageRendertarget,
            pool: Pool.Default,
            multiSampleType: MultisampleType.MultisampleNone,
            multiSampleQuality: 0,
            width: width,
            height: height);

        int result = device.GetMinimalTextureDescription(
            ref description,
            paletteUsesAlpha: true,
            Direct3D9MinimalTextureDescriptionFlags.NonPowerOfTwoConditionalAllowed |
            Direct3D9MinimalTextureDescriptionFlags.IgnoreFormat);
        if (result < 0)
        {
            return result;
        }
        if (result != Direct3D9Factory.SuccessHResult)
        {
            return Direct3D9Factory.UnsupportedTextureSizeHResult;
        }
        if (description.Width != width || description.Height != height)
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        return result;
    }

    private static int TryInitialize(
        uint width,
        uint height,
        Direct3D9Device device,
        uint? associatedDisplayIndex,
        out Direct3D9TextureRenderTarget? renderTarget)
    {
        renderTarget = null;
        if (device.RealizationCacheIndex == Direct3D9ImmediateBrushRealizer.InvalidRealizationCacheIndex)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        int result = GetSurfaceDescription(width, height, device, out SurfaceDesc description);
        if (result < 0)
        {
            return result;
        }

        result = device.TryCreateRenderTargetTexture(description, out Direct3D9Texture? texture);
        if (result < 0)
        {
            texture?.Dispose();
            return result;
        }
        if (texture is null)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        Direct3D9SurfaceRenderTarget? surfaceRenderTarget = null;
        try
        {
            result = texture.TryGetSurfaceLevel(0, out Direct3D9Surface? levelZeroSurface);
            if (result < 0)
            {
                return result;
            }
            if (levelZeroSurface is null)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            surfaceRenderTarget = new Direct3D9SurfaceRenderTarget(
                device,
                MultisampleType.MultisampleNone,
                renderTargetSurface: levelZeroSurface,
                pixelFormat: MilPixelFormat.Pbgra32Bpp,
                width: width,
                height: height,
                associatedDisplayIndex: associatedDisplayIndex);
            result = Direct3D9SurfaceRenderTarget.TransferInitializedDisplayRenderTarget(
                surfaceRenderTarget,
                out Direct3D9SurfaceRenderTarget? initializedSurfaceRenderTarget);
            surfaceRenderTarget = null;
            if (result < 0)
            {
                return result;
            }

            renderTarget = new Direct3D9TextureRenderTarget(device, initializedSurfaceRenderTarget!, texture);
            texture = null;
            return result;
        }
        finally
        {
            surfaceRenderTarget?.Dispose();
            texture?.Dispose();
        }
    }

    internal int GetCacheableBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? bitmapSource)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return GetBitmapSource(out bitmapSource);
    }

    internal int GetBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? bitmapSource)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        bitmapSource = null;

        int result = GetBitmap(out Direct3D9TextureRenderTargetBitmapSource? bitmap);
        if (result < 0)
        {
            bitmap?.Dispose();
            return result;
        }

        bitmapSource = bitmap;
        return result;
    }

    internal int GetBitmap(out Direct3D9TextureRenderTargetBitmapSource? bitmap)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        bitmap = null;
        if (_texture?.IsValid != true)
        {
            return Direct3D9Factory.DisplayStateInvalidHResult;
        }

        if (_deviceBitmap is null)
        {
            using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
            int result = _device.TryCreateBitmapTexture(
                _texture.Texture,
                isEvictable: false,
                out Direct3D9Texture? retainedTexture);
            if (result < 0)
            {
                retainedTexture?.Dispose();
                return result;
            }
            if (retainedTexture is null)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            try
            {
                _deviceBitmap = _bitmapSourceFactory(retainedTexture);
                retainedTexture = null;
            }
            catch (COMException exception)
            {
                return exception.HResult;
            }
            catch (OutOfMemoryException)
            {
                return Direct3D9Factory.OutOfMemoryHResult;
            }
            finally
            {
                retainedTexture?.Dispose();
            }
        }

        if (_hasInvalidContents)
        {
            _deviceBitmap.ValidateContents();
            _hasInvalidContents = false;
        }

        bitmap = _deviceBitmap.AddRef();
        return Direct3D9Factory.SuccessHResult;
    }

    private int InvalidateContentsAndDraw(
        Func<int> drawingOperation,
        Func<Direct3D9SurfaceRenderTarget, Func<int>, int> draw)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(drawingOperation);
        _hasInvalidContents = true;
        _deviceBitmap?.InvalidateContents();
        return draw(_surfaceRenderTarget!, drawingOperation);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);
        if (_texture?.IsValid == true)
        {
            _texture.SetAsEvictable();
        }

        _texture?.Dispose();
        _texture = null;
        _deviceBitmap?.Dispose();
        _deviceBitmap = null;
        _surfaceRenderTarget?.Dispose();
        _surfaceRenderTarget = null;
    }
}
