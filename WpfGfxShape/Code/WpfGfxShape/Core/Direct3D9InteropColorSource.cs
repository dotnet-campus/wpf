using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal sealed unsafe partial class Direct3D9InteropColorSource : IDisposable
{
    private IDirect3DTexture9* _texture;
    private IDirect3DSurface9* _surface;
    private IDirect3DSurface9* _transfer;
    private IDirect3DSurface9* _primary;
    private IDirect3DDevice9* _device;
    private readonly SurfaceDesc _description;
    private nint _frontIdentity, _sharedHandle;

    internal bool Matches(nint front, nint sharedHandle) =>
        _device != null && _frontIdentity == front && _sharedHandle == sharedHandle;

    private Direct3D9InteropColorSource(IDirect3DDevice9* device, SurfaceDesc description)
    {
        _device = device;
        Direct3D9Factory.AddRef((nint)device);
        _description = description;
    }

    internal IDirect3DTexture9* Texture => _texture;
    internal Direct3D9BitmapSourceRectangle Bounds => new(0, 0, (int)_description.Width, (int)_description.Height);

    internal Direct3D9PipelineColorSource CreatePipelineColorSource(Direct3D9Device device,
        System.Numerics.Matrix3x2 devicePointToTextureUv, MilBitmapInterpolationMode interpolationMode,
        Textureaddress addressU, Textureaddress addressV)
    {
        ArgumentNullException.ThrowIfNull(device);
        ObjectDisposedException.ThrowIf(_device == null, this);
        if (device.Device != _device) throw new ArgumentException(null, nameof(device));
        var state = new Direct3D9BitmapColorSourceDeviceState(device,
            () => (IDirect3DBaseTexture9*)Texture, interpolationMode, addressU, addressV,
            useHardwareTransform: false, shaderTextureTransformRegister: null, devicePointToTextureUv);
        return state.CreatePipelineColorSource(Realize, _description.Format == Format.X8R8G8B8);
    }

    internal static int TryCreate(IDirect3DDevice9* device, SurfaceDesc description,
        nint primary, nint sharedHandle, out Direct3D9InteropColorSource? source)
    {
        source = null;
        var candidate = new Direct3D9InteropColorSource(device, description)
        {
            _frontIdentity = primary,
            _sharedHandle = sharedHandle
        };
        try
        {
            void* shared = (void*)sharedHandle;
            IDirect3DTexture9* texture = null;
            int result = ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, uint, Format, Pool, IDirect3DTexture9**, void**, int>)device->LpVtbl[23])(
                device, description.Width, description.Height, 1, 1,
                description.Format, Pool.Default, &texture, sharedHandle != 0 ? &shared : null);
            if (result < 0) return result;
            candidate._texture = texture;
            IDirect3DSurface9* surface = null;
            result = ((delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>)texture->LpVtbl[18])(texture, 0, &surface);
            if (result < 0) return result;
            candidate._surface = surface;
            if (sharedHandle == 0)
            {
                IDirect3DSurface9* transfer = null;
                result = ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, MultisampleType, uint, int, IDirect3DSurface9**, void**, int>)device->LpVtbl[28])(
                    device, description.Width, description.Height, description.Format,
                    (MultisampleType)0, 0, 1, &transfer, null);
                if (result < 0) return result;
                candidate._transfer = transfer;
                candidate._primary = (IDirect3DSurface9*)primary;
                Direct3D9Factory.AddRef((nint)candidate._primary);
                result = candidate.Realize();
                if (result < 0) return result;
            }
            source = candidate;
            candidate = null;
            return 0;
        }
        finally { candidate?.Dispose(); }
    }

    internal static int TryCreateFromBitmap(IDirect3DDevice9* device, SurfaceDesc description,
        nint bitmap, out Direct3D9InteropColorSource? source)
    {
        source = null;
        var candidate = new Direct3D9InteropColorSource(device, description) { _frontIdentity = bitmap };
        IDirect3DTexture9* staging = null;
        bool locked = false;
        try
        {
            IDirect3DTexture9* texture = null;
            var createTexture = (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, uint, Format, Pool, IDirect3DTexture9**, void**, int>)device->LpVtbl[23];
            int hr = createTexture(device, description.Width, description.Height, 1, 0,
                description.Format, Pool.Default, &texture, null);
            TraceUpload("CreateTexture.Default", hr, device, texture, staging, description);
            if (hr < 0) return hr;
            candidate._texture = texture;
            hr = createTexture(device, description.Width, description.Height, 1, 0,
                description.Format, Pool.Systemmem, &staging, null);
            TraceUpload("CreateTexture.Systemmem", hr, device, texture, staging, description);
            if (hr < 0) return hr;
            LockedRect pixels;
            hr = ((delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, LockedRect*, void*, uint, int>)staging->LpVtbl[19])(staging, 0, &pixels, null, 0);
            TraceUpload("LockRect.Systemmem", hr, device, texture, staging, description);
            if (hr < 0) return hr;
            locked = true;
            hr = ((delegate* unmanaged[Stdcall]<nint, void*, uint, uint, byte*, int>)(*(void***)bitmap)[7])(
                bitmap, null, checked((uint)pixels.Pitch), checked((uint)pixels.Pitch * description.Height), (byte*)pixels.PBits);
            int unlock = Unlock(staging);
            locked = false;
            if (hr < 0) return hr;
            if (unlock < 0) return unlock;
            hr = ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DTexture9*, IDirect3DTexture9*, int>)device->LpVtbl[31])(device, staging, texture);
            TraceUpload("UpdateTexture", hr, device, texture, staging, description);
            if (hr < 0) return hr;
            source = candidate;
            candidate = null;
            return 0;
        }
        finally
        {
            if (locked) Unlock(staging);
            Direct3D9Factory.Release(staging);
            candidate?.Dispose();
        }
    }

    private static int Unlock(IDirect3DTexture9* texture) =>
        ((delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, int>)texture->LpVtbl[20])(texture, 0);

    private static void TraceUpload(string operation, int result, IDirect3DDevice9* device,
        IDirect3DTexture9* texture, IDirect3DTexture9* staging, SurfaceDesc description)
    {
        if (result >= 0) return;
        PartitionScheduler.RecordCommandFailure(0, System.Text.Encoding.UTF8.GetBytes(
            $"Image.Upload {operation} device=0x{(nuint)device:X} texture=0x{(nuint)texture:X} staging=0x{(nuint)staging:X} width={description.Width} height={description.Height} format={description.Format} levels=1 usage=0 pointerSize={IntPtr.Size} lockedRectSize={sizeof(LockedRect)}"), result);
    }

    internal int Realize()
    {
        ObjectDisposedException.ThrowIf(_device == null, this);
        if (_primary == null) return 0;
        nint sourceDc = 0, destinationDc = 0;
        int result = ((delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint*, int>)_primary->LpVtbl[15])(_primary, &sourceDc);
        if (result < 0) return result;
        try
        {
            result = ((delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint*, int>)_transfer->LpVtbl[15])(_transfer, &destinationDc);
            if (result < 0) return result;
            bool copied;
            try
            {
                copied = BitBlt(destinationDc, 0, 0, (int)_description.Width, (int)_description.Height,
                    sourceDc, 0, 0, 0x00CC0020);
                if (!copied) result = Marshal.GetHRForLastWin32Error();
            }
            finally
            {
                int release = ((delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint, int>)_transfer->LpVtbl[16])(_transfer, destinationDc);
                if (result >= 0) result = release;
            }
        }
        finally
        {
            int release = ((delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint, int>)_primary->LpVtbl[16])(_primary, sourceDc);
            if (result >= 0) result = release;
        }
        if (result < 0) return result;
        return ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, void*, IDirect3DSurface9*, void*, Texturefiltertype, int>)_device->LpVtbl[34])(
            _device, _transfer, null, _surface, null, Texturefiltertype.None);
    }

    public void Dispose()
    {
        Direct3D9Factory.Release(_primary); _primary = null;
        Direct3D9Factory.Release(_transfer); _transfer = null;
        Direct3D9Factory.Release(_surface); _surface = null;
        Direct3D9Factory.Release(_texture); _texture = null;
        Direct3D9Factory.Release(_device); _device = null;
    }

    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool BitBlt(nint destination, int x, int y, int width, int height,
        nint source, int sourceX, int sourceY, uint operation);
}
