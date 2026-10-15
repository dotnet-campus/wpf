using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class HardwareRenderTargetExports
{
    private static readonly Guid UnknownId = new("00000000-0000-0000-c000-000000000046");
    private static readonly Guid TargetId = new("00000020-a8f2-4877-ba0a-fd2b6645fb94");
    private static readonly void** Table = CreateTable();

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern nint MonitorFromWindow(nint window, uint flags);

    [StructLayout(LayoutKind.Sequential)]
    private struct Instance
    {
        internal void** Vtable;
        internal int References;
        internal nint Handle;
    }

    private sealed class Target : IDisposable
    {
        internal Direct3D9Objects Objects = null!;
        internal Direct3D9DeviceManager Manager = null!;
        internal Direct3D9Device Device = null!;
        internal Direct3D9SurfaceRenderTarget Renderer = null!;
        internal readonly uint Width, Height;
        internal readonly nint Window;
        internal uint PolicyGeneration;
        internal nint SoftwareTarget, SoftwareBitmap;
        internal Direct3D9SoftwareGdiPresenter? Presenter;

        internal void EnsurePolicy()
        {
            uint generation = ProcessRenderPolicy.Generation;
            if (PolicyGeneration == generation) return;
            DisposeBackend();
            InitializeBackend();
        }

        private void InitializeBackend()
        {
            uint generation = ProcessRenderPolicy.Generation;
            if (ProcessRenderPolicy.ShouldUseSoftware())
            {
                try
                {
                    Marshal.ThrowExceptionForHR(BitmapRenderTargetExports.CreateDrawingIntermediate(Width, Height,
                        out SoftwareTarget, out SoftwareBitmap, floating: false));
                    Marshal.ThrowExceptionForHR(Direct3D9SoftwareGdiPresenter.Create(Window, Width, Height, out Presenter));
                }
                catch { DisposeBackend(); throw; }
            }
            else
            {
                try { InitializeHardware(); }
                catch
                {
                    Renderer = null!;
                    Device = null!;
                    Manager = null!;
                    Objects = null!;
                    throw;
                }
            }
            PolicyGeneration = generation;
        }

        private void DisposeBackend()
        {
            Presenter?.Dispose(); Presenter = null;
            Direct3D9Factory.Release(SoftwareTarget); SoftwareTarget = 0;
            Direct3D9Factory.Release(SoftwareBitmap); SoftwareBitmap = 0;
            Renderer?.Dispose(); Renderer = null!;
            Device?.Dispose(); Device = null!;
            Manager?.Dispose(); Manager = null!;
            Objects?.Dispose(); Objects = null!;
        }

        internal Target(nint window, uint width, uint height)
        {
            Width = width; Height = height; Window = window;
            InitializeBackend();
        }

        private void InitializeHardware()
        {
            nint window = Window;
            uint width = Width, height = Height;
            Objects = Direct3D9Factory.Create();
            try
            {
                uint adapter = 0;
                nint monitor = MonitorFromWindow(window, 2);
                for (uint i = 0; i < Objects.GetAdapterCount(); i++)
                    if (Objects.GetAdapterMonitor(i) == monitor) { adapter = i; break; }
                Manager = Objects.CreateHardwareTargetDeviceManager();
                try { Device = Objects.CreateDevice(adapter, focusWindow: window, manager: Manager); }
                catch { Manager.Dispose(); throw; }
                try
                {
                    Renderer = new(Device, MultisampleType.MultisampleNone,
                        pixelFormat: MilPixelFormat.Pbgra32Bpp,
                        presentParameters: new PresentParameters(
                            backBufferWidth: width, backBufferHeight: height,
                            backBufferFormat: Format.A8R8G8B8, backBufferCount: 1,
                            swapEffect: Swapeffect.Copy, hDeviceWindow: window,
                            windowed: true, enableAutoDepthStencil: false,
                            autoDepthStencilFormat: Format.Unknown));
                    try
                    {
                        Marshal.ThrowExceptionForHR(Renderer.Resize(width, height));
                    }
                    catch { Renderer.Dispose(); throw; }
                }
                catch
                {
                    try { Device.Dispose(); }
                    finally { Manager.Dispose(); }
                    throw;
                }
            }
            catch { Objects.Dispose(); throw; }
        }

        public void Dispose() => DisposeBackend();
    }

    internal static int Create(nint window, uint width, uint height, out nint target)
    {
        target = 0;
        if (window == 0 || width == 0 || height == 0 || width > int.MaxValue || height > int.MaxValue)
            return Direct3D9Factory.InvalidArgumentHResult;
        if (!Windows.Win32.PInvoke.IsWindow(new Windows.Win32.Foundation.HWND(window)))
            return Direct3D9Factory.InvalidWindowHandleHResult;
        Target? owner = null;
        GCHandle handle = default;
        try
        {
            owner = new(window, width, height);
            handle = GCHandle.Alloc(owner);
            Instance* instance = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
            instance->Vtable = Table;
            instance->References = 1;
            instance->Handle = GCHandle.ToIntPtr(handle);
            target = (nint)instance;
            owner = null; handle = default;
            return 0;
        }
        catch (COMException error) { return error.HResult; }
        catch (OutOfMemoryException) { return Direct3D9Factory.OutOfMemoryHResult; }
        finally { if (handle.IsAllocated) handle.Free(); owner?.Dispose(); }
    }

    internal static bool IsHardware(nint target) => target != 0 && *(void***)target == Table;
    private static Target Get(Instance* value) => (Target)GCHandle.FromIntPtr(value->Handle).Target!;

    internal static int Render(nint target, GeneratedVisualResource? visual, MilColorF clearColor, bool signalPresent)
    {
        Target owner = Get((Instance*)target);
        try { owner.EnsurePolicy(); }
        catch (COMException error) { return error.HResult; }
        catch (OutOfMemoryException) { return Direct3D9Factory.OutOfMemoryHResult; }
        int hr = Clear((Instance*)target, clearColor);
        if (owner.SoftwareTarget != 0)
        {
            if (hr < 0) return hr;
            hr = visual is null ? 0 : GeneratedVisualRenderer.Render(visual, owner.SoftwareTarget, owner.Width, owner.Height);
            if (hr < 0) return hr;
            hr = owner.Presenter!.Lock(out byte[] pixels, out int stride);
            if (hr < 0) return hr;
            try
            {
                fixed (byte* buffer = pixels)
                    hr = Direct3D9BitmapSource.CopyPixels(owner.SoftwareBitmap, new(0, 0, (int)owner.Width, (int)owner.Height), (uint)stride, (uint)pixels.Length, (nint)buffer);
            }
            finally { owner.Presenter.Unlock(); }
            return hr < 0 ? hr : PerformanceInstrumentation.Presented(owner.Window,
                owner.Presenter.Present(new(0, 0, (int)owner.Width, (int)owner.Height)), signalPresent);
        }
        if (hr < 0) return hr;
        hr = visual is null ? 0 : GeneratedHardwareVisualRenderer.Render(visual, owner.Device,
            owner.Renderer, owner.Width, owner.Height);
        return hr < 0 ? hr : PerformanceInstrumentation.Presented(owner.Window, owner.Renderer.Present(), signalPresent);
    }

    private static void** CreateTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(HardwareRenderTargetExports), 7 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<Instance*, Guid*, nint*, int>)&Query;
        table[1] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&AddRef;
        table[2] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&Release;
        table[3] = (delegate* unmanaged[Stdcall]<Instance*, MilRectF*, void>)&Bounds;
        table[4] = (delegate* unmanaged[Stdcall]<Instance*, MilColorF*, nint, int>)&ClearExport;
        table[5] = (delegate* unmanaged[Stdcall]<Instance*, nint, uint, byte, float, int>)&Begin3D;
        table[6] = (delegate* unmanaged[Stdcall]<Instance*, int>)&End3D;
        return table;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(Instance* self, Guid* id, nint* output)
    {
        if (output == null) return Direct3D9Factory.InvalidArgumentHResult;
        *output = 0;
        if (id == null) return Direct3D9Factory.InvalidArgumentHResult;
        if (*id != UnknownId && *id != TargetId) return Direct3D9Factory.NoInterfaceHResult;
        Interlocked.Increment(ref self->References); *output = (nint)self; return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(Instance* self) => (uint)Interlocked.Increment(ref self->References);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(Instance* self)
    {
        int count = Interlocked.Decrement(ref self->References);
        if (count == 0)
        {
            GCHandle handle = GCHandle.FromIntPtr(self->Handle);
            Get(self).Dispose(); handle.Free(); NativeMemory.Free(self);
        }
        return (uint)count;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Bounds(Instance* self, MilRectF* output)
    {
        if (output != null) { Target owner = Get(self); *output = new(0, 0, owner.Width, owner.Height); }
    }

    private static int Clear(Instance* self, MilColorF color)
    {
        Target owner = Get(self);
        if (owner.SoftwareTarget != 0)
            return ((delegate* unmanaged[Stdcall]<nint, MilColorF*, nint, int>)(*(void***)owner.SoftwareTarget)[4])(owner.SoftwareTarget, &color, 0);
        var state = new Direct3D9ContextState(Matrix4x4.Identity, Matrix4x4.Identity,
            Matrix4x4.Identity, Matrix4x4.Identity, Cull.None, Cmpfunc.Always, false);
        int hr = owner.Renderer.EnsureState(state);
        if (hr < 0) return hr;
        uint Byte(float value) => (uint)Math.Clamp((int)MathF.Round(value * 255), 0, 255);
        uint argb = Byte(color.Alpha) << 24 | Byte(color.Red * color.Alpha) << 16
            | Byte(color.Green * color.Alpha) << 8 | Byte(color.Blue * color.Alpha);
        return owner.Device.Device->Clear(0, null, 1, argb, 1, 0);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int ClearExport(Instance* self, MilColorF* color, nint clip) =>
        clip != 0 ? Direct3D9Factory.NotImplementedHResult : Clear(self, color == null ? default : *color);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Begin3D(Instance* self, nint bounds, uint mode, byte depth, float z) => Direct3D9Factory.NotImplementedHResult;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int End3D(Instance* self) => Direct3D9Factory.NotImplementedHResult;
}
