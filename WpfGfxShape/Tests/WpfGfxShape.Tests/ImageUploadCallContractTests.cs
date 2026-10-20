using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe partial class ImageUploadCallContractTests
{
    /// <summary>Exercises Windows COM upload and reads back distinguishable GPU pixels without binding-text assertions.</summary>
    [TestMethod]
    public void WhenWindowsBitmapIsUploadedThenGpuPixelsMatchAfterRepeatedCreationAndRelease()
    {
        Marshal.ThrowExceptionForHR(CoInitializeEx(0, 0));
        nint factory = 0, bitmap = 0;
        IDirect3DSurface9* render = null;
        IDirect3DSurface9* readback = null;
        try
        {
            Guid clsid = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
            Guid iid = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
            Marshal.ThrowExceptionForHR(CoCreateInstance(&clsid, 0, 1, &iid, &factory));
            using Direct3D9Objects objects = Direct3D9Factory.Create();
            using Direct3D9Device owner = objects.CreateDevice();
            IDirect3DDevice9* device = owner.Device;
            try
            {
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, MultisampleType, uint, int, IDirect3DSurface9**, void**, int>)device->LpVtbl[28])(
                    device, 2, 2, Format.A8R8G8B8, (MultisampleType)0, 0, 0, &render, null));
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, Pool, IDirect3DSurface9**, void**, int>)device->LpVtbl[36])(
                    device, 2, 2, Format.A8R8G8B8, Pool.Systemmem, &readback, null));
                List<uint> actual = [];
                List<uint> expected = [];
                for (int iteration = 0; iteration < 8; iteration++)
                {
                    uint[] pixels = [0xFF123456u + (uint)iteration, 0xFFABCDEF, 0xFF00FF00, 0xFFFF0000];
                    Guid format = new("6fddc324-4e03-4bfe-b185-3d77768dc910");
                    fixed (uint* data = pixels)
                        Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, uint, uint, Guid*, uint, uint, byte*, nint*, int>)(*(void***)factory)[20])(
                            factory, 2, 2, &format, 8, 16, (byte*)data, &bitmap));
                    try
                    {
                        SurfaceDesc description = new() { Width = 2, Height = 2, Format = Format.A8R8G8B8 };
                        Marshal.ThrowExceptionForHR(Direct3D9InteropColorSource.TryCreateFromBitmap(device, description, bitmap, out var source));
                        using (source)
                        {
                            IDirect3DSurface9* surface = null;
                            try
                            {
                                IDirect3DTexture9* texture = source!.Texture;
                                Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>)texture->LpVtbl[18])(texture, 0, &surface));
                                Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, void*, IDirect3DSurface9*, void*, Texturefiltertype, int>)device->LpVtbl[34])(
                                    device, surface, null, render, null, Texturefiltertype.None));
                                Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, IDirect3DSurface9*, int>)device->LpVtbl[32])(device, render, readback));
                                LockedRect locked;
                                Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<IDirect3DSurface9*, LockedRect*, void*, uint, int>)readback->LpVtbl[13])(readback, &locked, null, 0x10));
                                try
                                {
                                    for (int row = 0; row < 2; row++)
                                    for (int column = 0; column < 2; column++)
                                        actual.Add(*(uint*)((byte*)locked.PBits + row * locked.Pitch + column * 4));
                                }
                                finally
                                {
                                    Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<IDirect3DSurface9*, int>)readback->LpVtbl[14])(readback));
                                }
                            }
                            finally { Direct3D9Factory.Release(surface); }
                        }
                        expected.AddRange(pixels);
                    }
                    finally { Marshal.Release(bitmap); bitmap = 0; }
                }
                CollectionAssert.AreEqual(expected, actual);
            }
            finally
            {
                Direct3D9Factory.Release(readback); readback = null;
                Direct3D9Factory.Release(render); render = null;
            }
        }
        finally
        {
            if (bitmap != 0) Marshal.Release(bitmap);
            if (factory != 0) Marshal.Release(factory);
            CoUninitialize();
        }
    }

    [LibraryImport("ole32.dll")]
    private static partial int CoInitializeEx(nint reserved, uint flags);
    [LibraryImport("ole32.dll")]
    private static partial void CoUninitialize();
    [LibraryImport("ole32.dll")]
    private static partial int CoCreateInstance(Guid* clsid, nint outer, uint context, Guid* iid, nint* instance);
}
