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
        device->AddRef();
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
            int result = device->CreateTexture(description.Width, description.Height, 1, 1,
                description.Format, Pool.Default, &texture, sharedHandle != 0 ? &shared : null);
            if (result < 0) return result;
            candidate._texture = texture;
            IDirect3DSurface9* surface = null;
            result = texture->GetSurfaceLevel(0, &surface);
            if (result < 0) return result;
            candidate._surface = surface;
            if (sharedHandle == 0)
            {
                IDirect3DSurface9* transfer = null;
                result = device->CreateRenderTarget(description.Width, description.Height, description.Format,
                    (MultisampleType)0, 0, true, &transfer, (void**)null);
                if (result < 0) return result;
                candidate._transfer = transfer;
                candidate._primary = (IDirect3DSurface9*)primary;
                candidate._primary->AddRef();
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
            int hr = device->CreateTexture(description.Width, description.Height, 1, 0,
                description.Format, Pool.Default, &texture, null);
            if (hr < 0) return hr;
            candidate._texture = texture;
            hr = device->CreateTexture(description.Width, description.Height, 1, 0,
                description.Format, Pool.Systemmem, &staging, null);
            if (hr < 0) return hr;
            LockedRect pixels;
            hr = staging->LockRect(0, &pixels, null, 0);
            if (hr < 0) return hr;
            locked = true;
            hr = ((delegate* unmanaged[Stdcall]<nint, void*, uint, uint, byte*, int>)(*(void***)bitmap)[7])(
                bitmap, null, checked((uint)pixels.Pitch), checked((uint)pixels.Pitch * description.Height), (byte*)pixels.PBits);
            int unlock = staging->UnlockRect(0);
            locked = false;
            if (hr < 0) return hr;
            if (unlock < 0) return unlock;
            hr = device->UpdateTexture((IDirect3DBaseTexture9*)staging, (IDirect3DBaseTexture9*)texture);
            if (hr < 0) return hr;
            source = candidate;
            candidate = null;
            return 0;
        }
        finally
        {
            if (locked) staging->UnlockRect(0);
            if (staging != null) staging->Release();
            candidate?.Dispose();
        }
    }

    internal int Realize()
    {
        ObjectDisposedException.ThrowIf(_device == null, this);
        if (_primary == null) return 0;
        nint sourceDc = 0, destinationDc = 0;
        int result = _primary->GetDC(&sourceDc);
        if (result < 0) return result;
        try
        {
            result = _transfer->GetDC(&destinationDc);
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
                int release = _transfer->ReleaseDC(destinationDc);
                if (result >= 0) result = release;
            }
        }
        finally
        {
            int release = _primary->ReleaseDC(sourceDc);
            if (result >= 0) result = release;
        }
        if (result < 0) return result;
        return _device->StretchRect(_transfer, null, _surface, null, Texturefiltertype.None);
    }

    public void Dispose()
    {
        if (_primary != null) { _primary->Release(); _primary = null; }
        if (_transfer != null) { _transfer->Release(); _transfer = null; }
        if (_surface != null) { _surface->Release(); _surface = null; }
        if (_texture != null) { _texture->Release(); _texture = null; }
        if (_device != null) { _device->Release(); _device = null; }
    }

    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool BitBlt(nint destination, int x, int y, int width, int height,
        nint source, int sourceX, int sourceY, uint operation);
}
