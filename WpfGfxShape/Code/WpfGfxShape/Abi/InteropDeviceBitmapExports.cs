using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class InteropDeviceBitmapExports
{
    [System.Diagnostics.Conditional("DT_INTEROP_CREATE_TRACE")]
    [System.Diagnostics.Conditional("DX_INTEROP_COM_DIAGNOSTICS")]
    private static void TraceCreate(string stage, nint address = 0, int result = 0)
    {
        string directory = System.IO.Path.Combine(AppContext.BaseDirectory, "Results", "dt-interop-create");
        System.IO.Directory.CreateDirectory(directory);
        using var stream = new System.IO.FileStream(System.IO.Path.Combine(directory, "stages.jsonl"), System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite);
        using var writer = new System.IO.StreamWriter(stream, leaveOpen: true);
        using (var json = new System.Text.Json.Utf8JsonWriter(stream))
        {
            json.WriteStartObject();
            json.WriteString("Stage", stage);
            json.WriteNumber("Address", (long)address);
            json.WriteNumber("Result", result);
            json.WriteNumber("PointerSize", sizeof(nint));
            json.WriteNumber("SurfaceDescSize", sizeof(SurfaceDesc));
            json.WriteNumber("CreationParametersSize", sizeof(DeviceCreationParameters));
            json.WriteNumber("CapsSize", sizeof(Caps9));
            json.WriteEndObject();
            json.Flush();
        }
        writer.WriteLine();
        writer.Flush();
        stream.Flush(true);
    }

    private static int QueryCom(void* self, Guid* iid, void** output)
        => ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)(*(void***)self)[0])(self, iid, output);

    private static uint ReleaseCom(void* self)
        => self == null ? 0 : ((delegate* unmanaged[Stdcall]<void*, uint>)(*(void***)self)[2])(self);

    private static void ReleaseCom(nint self) => ReleaseCom((void*)self);

    private static int GetSurfaceDevice(IDirect3DSurface9* self, IDirect3DDevice9** output)
        => ((delegate* unmanaged[Stdcall]<IDirect3DSurface9*, IDirect3DDevice9**, int>)(*(void***)self)[3])(self, output);

    private static int GetCreationParameters(IDirect3DDevice9* self, DeviceCreationParameters* output)
        => ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, DeviceCreationParameters*, int>)(*(void***)self)[9])(self, output);

    private static int UnlockSurface(IDirect3DSurface9* self)
        => ((delegate* unmanaged[Stdcall]<IDirect3DSurface9*, int>)(*(void***)self)[14])(self);

    private static int CreateRenderTarget(IDirect3DDevice9* self, uint width, uint height, Format format,
        int lockable, IDirect3DSurface9** output)
        => ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, MultisampleType, uint, int, IDirect3DSurface9**, void**, int>)(*(void***)self)[28])
            (self, width, height, format, (MultisampleType)0, 0, lockable, output, null);

    private static int StretchRect(IDirect3DDevice9* self, IDirect3DSurface9* source, void* sourceRect,
        IDirect3DSurface9* target, void* targetRect)
        => ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, void*, IDirect3DSurface9*, void*, Texturefiltertype, int>)(*(void***)self)[34])
            (self, source, sourceRect, target, targetRect, Texturefiltertype.None);

    private const int PointerError = unchecked((int)0x80004003);
    private const int InvalidArgument = unchecked((int)0x80070057);
    private const int Fail = unchecked((int)0x80004005);

    [StructLayout(LayoutKind.Sequential)]
    private struct Instance
    {
        internal void** Vtable;
        internal int References;
        internal nint State;
    }

    private sealed class BitmapState : IDisposable
    {
        internal readonly object Gate = new();
        internal nint Surface, HardwareFront, HardwareTexture, Front, Software;
        internal nint SharedHandle;
        internal ulong AdapterLuid;
        internal FrontBufferUpdateMethod UpdateMethod;
        internal uint FrontVersion;
        internal readonly SurfaceDesc Description;
        internal readonly double DpiX, DpiY;
        internal readonly uint Version;
        internal nint Callback;
        internal readonly bool Fallback;
        internal bool Disabled;
        private bool _disposed;
        internal readonly uint Adapter;
        private readonly Action<uint, bool> _adapterStatusListener;
        internal readonly List<Direct3D9BitmapSourceRectangle> Dirty = [];

        internal nint Identity;

        internal void NotifyAdapterStatus(uint adapter, bool valid)
        {
            nint callback;
            lock (Gate)
            {
                if (_disposed || adapter != Adapter) return;
                // Final Release can already be waiting for Gate; never resurrect zero references.
                ref int references = ref ((Instance*)Identity)->References;
                int count;
                do
                {
                    count = Volatile.Read(ref references);
                    if (count == 0) return;
                }
                while (Interlocked.CompareExchange(ref references, count + 1, count) != count);
            }
            try
            {
                lock (Gate)
                {
                    ApplyAdapterStatus(this, valid);
                    callback = Callback;
                }
                if (callback != 0)
                    ((delegate* unmanaged[Stdcall]<int, uint, void>)callback)(valid ? 1 : 0, Version);
            }
            finally { Direct3D9Factory.Release(Identity); }
        }

        internal BitmapState(nint surface, SurfaceDesc description, double dpiX, double dpiY, uint version, nint callback, bool fallback, uint adapter)
        {
            Surface = surface; Description = description; DpiX = dpiX; DpiY = dpiY;
            Version = version; Callback = callback; Fallback = fallback; Adapter = adapter;
            _adapterStatusListener = NotifyAdapterStatus;
        }

        internal void Register() => Direct3D9DeviceManager.AddInteropAdapterStatusListener(_adapterStatusListener);

        public void Dispose()
        {
            Direct3D9DeviceManager.RemoveInteropAdapterStatusListener(_adapterStatusListener);
            lock (Gate)
            {
                _disposed = true;
                nint surface = Surface, hardwareFront = HardwareFront, front = Front, software = Software;
                Surface = HardwareFront = Front = Software = Callback = 0;
                ReleaseHardwareTexture(this);
                Direct3D9Factory.Release(hardwareFront);
                Direct3D9Factory.Release(software);
                Direct3D9Factory.Release(front);
                Direct3D9Factory.Release(surface);
            }
        }
    }

    private static readonly void** Vtable = CreateTable();

    private static void** CreateTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(InteropDeviceBitmapExports), 3 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<Instance*, Guid*, nint*, int>)&Query;
        table[1] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&AddRef;
        table[2] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&Release;
        return table;
    }

    private static BitmapState State(nint identity) => (BitmapState)GCHandle.FromIntPtr(((Instance*)identity)->State).Target!;

    internal static bool IsInterop(nint identity) => identity != 0 && ((Instance*)identity)->Vtable == Vtable;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(Instance* self, Guid* iid, nint* output)
    {
        if (output == null) return PointerError;
        *output = 0;
        if (iid == null) return PointerError;
        if (*iid != new Guid("00000000-0000-0000-c000-000000000046")) return unchecked((int)0x80004002);
        Interlocked.Increment(ref self->References);
        *output = (nint)self;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(Instance* self) => (uint)Interlocked.Increment(ref self->References);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(Instance* self)
    {
        int references = Interlocked.Decrement(ref self->References);
        if (references == 0)
        {
            GCHandle handle = GCHandle.FromIntPtr(self->State);
            ((BitmapState)handle.Target!).Dispose();
            handle.Free();
            NativeMemory.Free(self);
        }
        return (uint)references;
    }

    [UnmanagedCallersOnly(EntryPoint = "InteropDeviceBitmap_Create", CallConvs = [typeof(CallConvStdcall)])]
    private static int Create(nint resource, double dpiX, double dpiY, uint version, nint callback, int fallback, nint* output, uint* width, uint* height)
    {
        if (output == null || width == null || height == null || resource == 0 || callback == 0) return PointerError;
        *output = 0; *width = *height = 0;
        if (dpiX < 0 || dpiY < 0) return InvalidArgument;
        IDirect3DSurface9* surface = null;
        IDirect3DDevice9* device = null;
        Instance* instance = null;
        GCHandle handle = default;
        try
        {
            Guid iid = IDirect3DSurface9.Guid;
            TraceCreate("QI.surface.before", resource);
            int result = QueryCom((void*)resource, &iid, (void**)&surface);
            TraceCreate("QI.surface.after", (nint)surface, result);
            if (result < 0) return result;
            SurfaceDesc description;
            TraceCreate("GetDesc.before", (nint)surface);
            result = ((delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>)(*(void***)surface)[12])(surface, &description);
            TraceCreate("GetDesc.after", (nint)surface, result);
            if (result < 0) return result;
            if (description.Width > 0x3fffffff || description.Height > 0x3fffffff) return unchecked((int)0x88980801);
            if (description.Format is not (Format.A8R8G8B8 or Format.X8R8G8B8)) return unchecked((int)0x88982F80);
            if ((description.Usage & 2) != 0 || (description.Usage & 1) == 0) return unchecked((int)0x88980800);
            if (description.Pool != Pool.Default) return unchecked((int)0x88980802);
            TraceCreate("GetDevice.before", (nint)surface);
            result = GetSurfaceDevice(surface, &device);
            TraceCreate("GetDevice.after", (nint)device, result);
            if (result < 0) return result;
            TraceCreate("CheckDevice.before", (nint)device);
            if (CheckDevice(device) < 0) return unchecked((int)0x88980803);
            TraceCreate("CheckDevice.after", (nint)device);
            DeviceCreationParameters parameters;
            TraceCreate("GetCreationParameters.before", (nint)device);
            result = GetCreationParameters(device, &parameters);
            TraceCreate("GetCreationParameters.after", (nint)device, result);
            if (result < 0) return result;
            TraceCreate("GetUpdateMethod.before", (nint)device);
            FrontBufferUpdateMethod method = GetUpdateMethod(device, surface);
            TraceCreate("GetUpdateMethod.after", (nint)device, (int)method);
            if ((description.MultiSampleType != (MultisampleType)0 || description.MultiSampleQuality != 0)
                && method != FrontBufferUpdateMethod.SharedSurface) return unchecked((int)0x88980800);
            var state = new BitmapState((nint)surface, description, dpiX, dpiY, version, callback, fallback != 0, parameters.AdapterOrdinal)
            {
                UpdateMethod = method,
                AdapterLuid = GetAdapterLuid(device)
            };
            handle = GCHandle.Alloc(state);
            instance = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
            instance->Vtable = Vtable; instance->References = 1; instance->State = GCHandle.ToIntPtr(handle);
            state.Identity = (nint)instance;
            TraceCreate("Register.before", (nint)instance);
            state.Register();
            TraceCreate("Register.after", (nint)instance);
            *output = (nint)instance; *width = description.Width; *height = description.Height;
            surface = null; instance = null; handle = default;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        finally
        {
            NativeMemory.Free(instance);
            if (handle.IsAllocated) handle.Free();
            ReleaseCom(device);
            ReleaseCom(surface);
        }
    }

    private enum FrontBufferUpdateMethod { Software, SharedSurface, BitBlt }

    private static void ReleaseHardwareTexture(BitmapState state)
    {
        nint texture = state.HardwareTexture;
        state.HardwareTexture = state.SharedHandle = 0;
        Direct3D9Factory.Release(texture);
    }

    private static FrontBufferUpdateMethod GetUpdateMethod(IDirect3DDevice9* device, IDirect3DSurface9* surface)
    {
        Caps9 caps;
        TraceCreate("GetUpdateMethod.GetDeviceCaps.before", (nint)device);
        int capsResult = ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Caps9*, int>)(*(void***)device)[7])(device, &caps);
        TraceCreate("GetUpdateMethod.GetDeviceCaps.after", (nint)device, capsResult);
        if (capsResult < 0 || (caps.DevCaps2 & 0x10) == 0)
            return FrontBufferUpdateMethod.Software;
        IDirect3DDevice9Ex* extended = null;
        Guid iid = IDirect3DDevice9Ex.Guid;
        TraceCreate("GetUpdateMethod.QI.before", (nint)device);
        int queryResult = QueryCom(device, &iid, (void**)&extended);
        TraceCreate("GetUpdateMethod.QI.after", (nint)extended, queryResult);
        if (queryResult >= 0)
        {
            TraceCreate("GetUpdateMethod.extended.Release.before", (nint)extended);
            uint references = ReleaseCom(extended);
            TraceCreate("GetUpdateMethod.extended.Release.after", (nint)extended, unchecked((int)references));
            if ((caps.Caps2 & 0x80000000) != 0) return FrontBufferUpdateMethod.SharedSurface;
        }
        if (!OperatingSystem.IsWindowsVersionAtLeast(6))
        {
            nint dc = 0;
            TraceCreate("GetUpdateMethod.GetDC.before", (nint)surface);
            int dcResult = ((delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint*, int>)(*(void***)surface)[15])(surface, &dc);
            TraceCreate("GetUpdateMethod.GetDC.after", dc, dcResult);
            if (dcResult >= 0)
            {
                TraceCreate("GetUpdateMethod.ReleaseDC.before", (nint)surface);
                int releaseResult = ((delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint, int>)(*(void***)surface)[16])(surface, dc);
                TraceCreate("GetUpdateMethod.ReleaseDC.after", (nint)surface, releaseResult);
                if (releaseResult >= 0) return FrontBufferUpdateMethod.BitBlt;
            }
        }
        return FrontBufferUpdateMethod.Software;
    }

    private static ulong GetAdapterLuid(IDirect3DDevice9* device)
    {
        IDirect3D9* factory = null;
        IDirect3D9Ex* extended = null;
        try
        {
            TraceCreate("GetDirect3D.before", (nint)device);
            if (((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3D9**, int>)(*(void***)device)[6])(device, &factory) < 0) return 0;
            TraceCreate("GetDirect3D.after", (nint)factory);
            Guid iid = IDirect3D9Ex.Guid;
            if (QueryCom(factory, &iid, (void**)&extended) < 0) return 0;
            DeviceCreationParameters parameters;
            if (GetCreationParameters(device, &parameters) < 0) return 0;
            Silk.NET.Core.Native.Luid luid;
            if (((delegate* unmanaged[Stdcall]<IDirect3D9Ex*, uint, Silk.NET.Core.Native.Luid*, int>)(*(void***)extended)[21])(extended, parameters.AdapterOrdinal, &luid) < 0) return 0;
            return *(ulong*)&luid;
        }
        finally
        {
            ReleaseCom(extended);
            ReleaseCom(factory);
        }
    }

    internal static int TryCreateDependentDeviceColorSource(nint identity, IDirect3DDevice9* device,
        out Direct3D9InteropColorSource? colorSource)
    {
        colorSource = null;
        if (identity == 0 || device == null) return 1;
        BitmapState state = State(identity);
        lock (state.Gate)
        {
            if (state.Disabled || state.HardwareFront == 0) return 1;
            bool shared = state.UpdateMethod == FrontBufferUpdateMethod.SharedSurface;
            if (state.UpdateMethod == FrontBufferUpdateMethod.Software
                || shared && (state.SharedHandle == 0 || state.AdapterLuid == 0 || state.AdapterLuid != GetAdapterLuid(device)))
                return state.Front == 0 ? 1 : Direct3D9InteropColorSource.TryCreateFromBitmap(
                    device, state.Description, state.Front, out colorSource);
            return Direct3D9InteropColorSource.TryCreate(device, state.Description, state.HardwareFront,
                shared ? state.SharedHandle : 0, out colorSource);
        }
    }

    internal static bool IsDependentDeviceColorSourceCurrent(nint identity, Direct3D9InteropColorSource source)
    {
        if (identity == 0) return false;
        BitmapState state = State(identity);
        lock (state.Gate)
        {
            return !state.Disabled && state.HardwareFront != 0
                && (source.Matches(state.Front, 0) || source.Matches(state.HardwareFront,
                    state.UpdateMethod == FrontBufferUpdateMethod.SharedSurface ? state.SharedHandle : 0));
        }
    }

    internal static MilRectD GetContentBounds(nint identity)
    {
        BitmapState state = State(identity);
        return new(0, 0, state.Description.Width * 96.0 / state.DpiX,
            state.Description.Height * 96.0 / state.DpiY);
    }

    private static int CheckDevice(IDirect3DDevice9* device)
    {
        IDirect3DDevice9Ex* extended = null;
        Guid iid = IDirect3DDevice9Ex.Guid;
        TraceCreate("CheckDevice.generated.QI.before", (nint)device);
        int result = QueryCom(device, &iid, (void**)&extended);
        TraceCreate("CheckDevice.generated.QI.after", (nint)extended, result);
        if (result < 0)
        {
            TraceCreate("CheckDevice.TestCooperativeLevel.before", (nint)device);
            int status = ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, int>)(*(void***)device)[3])(device);
            TraceCreate("CheckDevice.TestCooperativeLevel.after", (nint)device, status);
            return status;
        }
        try { return ((delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, nint, int>)(*(void***)extended)[128])(extended, 0); }
        finally
        {
            TraceCreate("CheckDevice.extended.Release.before", (nint)extended);
            uint references = ReleaseCom(extended);
            TraceCreate("CheckDevice.extended.Release.after", (nint)extended, unchecked((int)references));
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "InteropDeviceBitmap_Detach", CallConvs = [typeof(CallConvStdcall)])]
    private static void Detach(nint identity)
    {
        if (identity == 0) return;
        BitmapState state = State(identity);
        lock (state.Gate)
        {
            nint surface = state.Surface;
            state.Surface = state.Callback = 0;
            Direct3D9Factory.Release(surface);
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "InteropDeviceBitmap_AddDirtyRect", CallConvs = [typeof(CallConvStdcall)])]
    private static int AddDirty(int x, int y, int width, int height, nint identity)
    {
        if (identity == 0) return PointerError;
        if (x < 0 || y < 0 || width < 0 || height < 0) return InvalidArgument;
        if (width == 0 || height == 0) return 0;
        BitmapState state = State(identity);
        lock (state.Gate)
        {
            if ((ulong)(uint)x + (uint)width > state.Description.Width || (ulong)(uint)y + (uint)height > state.Description.Height) return InvalidArgument;
            int right = x + width, bottom = y + height;
            foreach (var dirty in state.Dirty)
            {
                if (dirty.X <= x && dirty.Y <= y
                    && dirty.X + dirty.Width >= right && dirty.Y + dirty.Height >= bottom) return 0;
            }
            if (state.Dirty.Count >= 5)
            {
                foreach (var dirty in state.Dirty)
                {
                    x = Math.Min(x, dirty.X); y = Math.Min(y, dirty.Y);
                    right = Math.Max(right, dirty.X + dirty.Width);
                    bottom = Math.Max(bottom, dirty.Y + dirty.Height);
                }
                state.Dirty.Clear();
                width = right - x; height = bottom - y;
            }
            state.Dirty.Add(new(x, y, width, height));
            return 0;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "InteropDeviceBitmap_GetAsSoftwareBitmap", CallConvs = [typeof(CallConvStdcall)])]
    private static int Snapshot(nint identity, nint* output)
    {
        if (identity == 0 || output == null) return PointerError;
        *output = 0;
        BitmapState state = State(identity);
        lock (state.Gate)
        {
            try { return ReadSurface(state, out *output); }
            catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        }
    }

    private static int ReadSurface(BitmapState state, out nint bitmap, nint surface = 0)
    {
        bitmap = 0;
        if (surface == 0) surface = state.Surface;
        if (surface == 0) return Fail;
        IDirect3DSurface9* source = (IDirect3DSurface9*)surface;
        IDirect3DDevice9* device = null;
        IDirect3DSurface9* staging = null;
        IDirect3DSurface9* resolved = null;
        nint copy = 0;
        bool locked = false;
        try
        {
            int result = GetSurfaceDevice(source, &device);
            if (result < 0) return result;
            SurfaceDesc description = state.Description;
            if (surface == state.Surface && description.MultiSampleType != (MultisampleType)0)
            {
                result = CreateRenderTarget(device, description.Width, description.Height, description.Format, 0, &resolved);
                if (result < 0) return result;
                result = StretchRect(device, source, null, resolved, null);
                if (result < 0) return result;
                source = resolved;
            }
            result = ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, Pool, IDirect3DSurface9**, void**, int>)(*(void***)device)[36])(device, description.Width, description.Height, description.Format, Pool.Systemmem, &staging, null);
            if (result < 0) return result;
            result = ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, IDirect3DSurface9*, int>)(*(void***)device)[32])(device, source, staging);
            if (result < 0) return result;
            LockedRect pixels;
            result = ((delegate* unmanaged[Stdcall]<IDirect3DSurface9*, LockedRect*, void*, uint, int>)(*(void***)staging)[13])(staging, &pixels, null, 0x10);
            if (result < 0) return result;
            locked = true;
            Guid format = description.Format == Format.A8R8G8B8
                ? new("6fddc324-4e03-4bfe-b185-3d77768dc910") : new("6fddc324-4e03-4bfe-b185-3d77768dc90e");
            result = SoftwareBitmap.Create(description.Width, description.Height, state.DpiX, state.DpiY, format, out copy);
            if (result < 0) return result;
            SoftwareBitmap.Bitmap* target = (SoftwareBitmap.Bitmap*)copy;
            for (uint y = 0; y < description.Height; y++)
                Buffer.MemoryCopy((byte*)pixels.PBits + (long)y * pixels.Pitch, target->Pixels + (long)y * target->Stride, target->Stride, (long)description.Width * 4);
            result = UnlockSurface(staging); locked = false;
            if (result < 0) return result;
            bitmap = copy; copy = 0;
            return 0;
        }
        finally
        {
            Direct3D9Factory.Release(copy);
            if (locked) UnlockSurface(staging);
            ReleaseCom(staging);
            ReleaseCom(resolved);
            ReleaseCom(device);
        }
    }

    internal static int Present(nint identity)
    {
        BitmapState state = State(identity);
        nint callback = 0;
        int available = 0;
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)identity)[1])(identity);
        try
        {
            lock (state.Gate)
            {
                int result = PresentLocked(state, out callback, out available);
                // Native Present suppresses D3D failures so transient device errors cannot poison composition.
                const int d3dFailureMask = unchecked((int)0x88760000);
                return (result & d3dFailureMask) == d3dFailureMask ? 0 : result;
            }
        }
        finally
        {
            try
            {
                if (callback != 0) ((delegate* unmanaged[Stdcall]<int, uint, void>)callback)(available, state.Version);
            }
            finally { Direct3D9Factory.Release(identity); }
        }
    }

    private static void ApplyAdapterStatus(BitmapState state, bool valid)
    {
        if (!valid)
        {
            if (state.Fallback)
            {
                if (ReadSurface(state, out nint fallback) >= 0)
                {
                    Direct3D9Factory.Release(state.Software); state.Software = fallback;
                }
            }
            else
            {
                nint surface = state.Surface; state.Surface = 0;
                Direct3D9Factory.Release(surface);
            }
            Direct3D9Factory.Release(state.HardwareFront); state.HardwareFront = 0;
            ReleaseHardwareTexture(state);
            Direct3D9Factory.Release(state.Front); state.Front = 0;
            state.Disabled = true;
        }
        else if (state.Fallback && state.Disabled)
        {
            state.Disabled = false;
            state.Dirty.Clear();
            state.Dirty.Add(new(0, 0, (int)state.Description.Width, (int)state.Description.Height));
        }
    }

    private static int PresentLocked(BitmapState state, out nint callback, out int available)
    {
        callback = 0;
        available = 0;
        if (state.Surface == 0) return 0;
        IDirect3DDevice9* device = null;
        int result = GetSurfaceDevice((IDirect3DSurface9*)state.Surface, &device);
        if (result < 0) return result;
        try
        {
            int status = CheckDevice(device);
            if (status < 0)
            {
                if (!state.Disabled)
                {
                    ApplyAdapterStatus(state, valid: false);
                    callback = state.Callback;
                }
                return status;
            }
            if (state.Disabled && state.Fallback)
            {
                ApplyAdapterStatus(state, valid: true);
                callback = state.Callback;
                available = 1;
            }
            if (state.Dirty.Count == 0) return 0;
            if (state.HardwareFront == 0)
            {
                IDirect3DSurface9* front = null;
                if (state.UpdateMethod == FrontBufferUpdateMethod.SharedSurface)
                {
                    IDirect3DTexture9* texture = null;
                    void* shared = null;
                    result = ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, uint, Format, Pool, IDirect3DTexture9**, void**, int>)(*(void***)device)[23])(device, state.Description.Width, state.Description.Height, 1, 1,
                        state.Description.Format, Pool.Default, &texture, &shared);
                    if (result < 0) return result;
                    result = ((delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>)(*(void***)texture)[18])(texture, 0, &front);
                    if (result < 0) { ReleaseCom(texture); return result; }
                    state.HardwareTexture = (nint)texture;
                    state.SharedHandle = (nint)shared;
                }
                else
                {
                    result = CreateRenderTarget(device, state.Description.Width, state.Description.Height,
                        state.Description.Format, state.UpdateMethod == FrontBufferUpdateMethod.BitBlt ? 1 : 0, &front);
                    if (result < 0) return result;
                }
                state.HardwareFront = (nint)front;
                result = ((delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, void*, uint, int>)(*(void***)device)[35])(device, front, null, 0);
                if (result < 0)
                {
                    Direct3D9Factory.Release(state.HardwareFront); state.HardwareFront = 0;
                    ReleaseHardwareTexture(state);
                    return result;
                }
            }
            foreach (Direct3D9BitmapSourceRectangle dirty in state.Dirty)
            {
                Silk.NET.Maths.Box2D<int> rectangle = new(dirty.X, dirty.Y, dirty.X + dirty.Width, dirty.Y + dirty.Height);
                result = StretchRect(device, (IDirect3DSurface9*)state.Surface, &rectangle,
                    (IDirect3DSurface9*)state.HardwareFront, &rectangle);
                if (result < 0) return result;
            }
            result = ReadSurface(state, out nint snapshot, state.HardwareFront);
            if (result < 0) return result;
            nint previous = state.Front;
            state.Front = snapshot;
            Direct3D9Factory.Release(previous);
            Direct3D9Factory.Release(state.Software); state.Software = 0;
            state.Dirty.Clear();
            state.FrontVersion++;
            return 0;
        }
        finally { ReleaseCom(device); }
    }

    internal static nint AcquireBitmapSource(nint identity, bool fallbackOnly = false)
        => PinBitmapSourceForCapture(identity, fallbackOnly);

    internal static nint PinBitmapSourceForCapture(nint identity, bool fallbackOnly = false)
    {
        if (identity == 0) return 0;
        BitmapState state = State(identity);
        lock (state.Gate)
        {
            nint source = state.Software != 0 ? state.Software : fallbackOnly || state.Disabled ? 0 : state.Front;
            if (source != 0) SoftwareBitmap.Retain(source);
            return source;
        }
    }
}
