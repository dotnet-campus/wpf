using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [DllImport("dxgi.dll")]
    private static extern int CreateDXGIFactory1(in Guid iid, out nint factory);
    [DllImport("d3d11.dll")]
    private static extern int D3D11CreateDevice(nint adapter, uint driver, nint software, uint flags, nint levels, uint count, uint sdk, out nint device, out uint level, out nint context);

    [StructLayout(LayoutKind.Sequential)]
    private struct DesktopTextureDescription
    {
        internal uint Width, Height, MipLevels, ArraySize, Format, SampleCount, SampleQuality, Usage, BindFlags, CpuAccessFlags, MiscFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DesktopMappedTexture
    {
        internal nint Data;
        internal uint RowPitch, DepthPitch;
    }

    private static uint ReadDuplicatedDesktopPixel(nint window)
    {
        nint factory = 0, adapter = 0, output = 0, output1 = 0, device = 0, context = 0, duplication = 0;
        nint resource = 0, texture = 0, staging = 0;
        bool acquired = false;
        try
        {
            Assert.AreEqual(0, CreateDXGIFactory1(new Guid("770aae78-f26f-4dba-a829-253c83d1b387"), out factory));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)factory)[12])(factory, 0, &adapter));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)adapter)[7])(adapter, 0, &output));
            Guid iid = new("00cddea8-939b-4b83-a340-a685226666cc");
            Assert.AreEqual(0, Marshal.QueryInterface(output, in iid, out output1));
            Assert.AreEqual(0, D3D11CreateDevice(adapter, 0, 0, 0, 0, 0, 7, out device, out _, out context));
            int duplicate = ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)(*(void***)output1)[22])(output1, device, &duplication);
            Console.WriteLine($"DesktopDuplication create=0x{duplicate:X8} adapter=0 output=0");
            Assert.AreEqual(0, duplicate);
            byte* frameInfo = stackalloc byte[48];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, void*, nint*, int>)(*(void***)duplication)[8])(duplication, 5000, frameInfo, &resource));
            acquired = true;
            iid = new("6f15aaf2-d208-4e89-9ab4-489535d34f9c");
            Assert.AreEqual(0, Marshal.QueryInterface(resource, in iid, out texture));
            DesktopTextureDescription description = default;
            ((delegate* unmanaged[Stdcall]<nint, DesktopTextureDescription*, void>)(*(void***)texture)[10])(texture, &description);
            description.Usage = 3;
            description.BindFlags = 0;
            description.CpuAccessFlags = 0x20000;
            description.MiscFlags = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, DesktopTextureDescription*, nint, nint*, int>)(*(void***)device)[5])(device, &description, 0, &staging));
            ((delegate* unmanaged[Stdcall]<nint, nint, nint, void>)(*(void***)context)[47])(context, staging, texture);
            WindowPoint point = new() { X = 16, Y = 8 };
            Assert.AreNotEqual(0, ClientToScreen(window, ref point));
            ObservationMonitorInfo monitor = new() { Size = (uint)sizeof(ObservationMonitorInfo) };
            Assert.AreNotEqual(0, GetMonitorInfoW(MonitorFromWindow(window, 2), ref monitor));
            int x = point.X - monitor.Monitor.Left, y = point.Y - monitor.Monitor.Top;
            Assert.IsTrue(x >= 0 && y >= 0 && x < description.Width && y < description.Height);
            Assert.AreEqual(87u, description.Format);
            DesktopMappedTexture mapped = default;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint, DesktopMappedTexture*, int>)(*(void***)context)[14])(context, staging, 0, 1, 0, &mapped));
            uint pixel;
            try { pixel = *(uint*)((byte*)mapped.Data + y * mapped.RowPitch + x * sizeof(uint)); }
            finally { ((delegate* unmanaged[Stdcall]<nint, nint, uint, void>)(*(void***)context)[15])(context, staging, 0); }
            Console.WriteLine($"DesktopDuplication screen=({point.X},{point.Y}) texture={description.Width}x{description.Height} pixel=0x{pixel:X8}");
            return pixel;
        }
        finally
        {
            if (acquired) Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)duplication)[14])(duplication));
            Release(staging); Release(texture); Release(resource); Release(duplication);
            Release(context); Release(device); Release(output1); Release(output); Release(adapter); Release(factory);
        }
    }
}
