using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9PresentRequest(
    Direct3D9SurfaceRect? SourceRect,
    Direct3D9SurfaceRect? DestinationRect,
    IReadOnlyList<Direct3D9SurfaceRect>? DirtyRegion,
    uint Flags = 0,
    nint DestinationWindowOverride = 0);

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9SwapChain : Direct3D9Resource
{
    private readonly Direct3D9ResourceManager _resourceManager;
    private readonly Func<uint, (int HResult, Direct3D9Surface? Surface)>? _getBackBuffer;
    private readonly Func<int>? _present;
    private readonly Func<Direct3D9PresentRequest, int>? _presentWithParameters;
    private readonly Action? _release;
    private Direct3D9Surface[]? _backBuffers;
    private IDirect3DSwapChain9* _swapChain;
    private IDirect3DSwapChain9Ex* _swapChainEx;

    internal Direct3D9SwapChain(
        Direct3D9ResourceManager resourceManager,
        IDirect3DSwapChain9* swapChain,
        Func<uint, (int HResult, Direct3D9Surface? Surface)>? getBackBuffer = null,
        Action? release = null,
        Func<int>? present = null,
        Func<Direct3D9PresentRequest, int>? presentWithParameters = null)
        : base(resourceManager)
    {
        _resourceManager = resourceManager;
        _swapChain = swapChain;
        _getBackBuffer = getBackBuffer;
        _present = present;
        _presentWithParameters = presentWithParameters;
        _release = release;
    }

    internal static int TryCreate(
        Direct3D9ResourceManager resourceManager,
        IDirect3DSwapChain9* swapChain,
        uint backBufferCount,
        out Direct3D9SwapChain? createdSwapChain)
    {
        ArgumentNullException.ThrowIfNull(resourceManager);
        createdSwapChain = null;
        if (swapChain is null)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }
        if (backBufferCount == 0)
        {
            PresentParameters presentParameters = default;
            void** vtable = swapChain->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, PresentParameters*, int> getPresentParameters =
                (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, PresentParameters*, int>) vtable[9];
            int result = getPresentParameters(swapChain, &presentParameters);
            if (result < 0)
            {
                return result;
            }

            backBufferCount = presentParameters.BackBufferCount;
            if (backBufferCount == 0)
            {
                return Direct3D9Factory.InvalidArgumentHResult;
            }
        }

        void** swapChainVtable = swapChain->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, uint> addRef =
            (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, uint>) swapChainVtable[1];
        _ = addRef(swapChain);

        Direct3D9SwapChain candidate = new(resourceManager, swapChain)
        {
            _swapChainEx = QuerySwapChain9Ex(swapChain)
        };
        Direct3D9Surface[] backBuffers = new Direct3D9Surface[backBufferCount];
        for (uint index = 0; index < backBufferCount; index++)
        {
            int result = candidate.TryGetBackBufferFromNative(index, out Direct3D9Surface? backBuffer);
            if (result < 0)
            {
                foreach (Direct3D9Surface? initializedBackBuffer in backBuffers)
                {
                    initializedBackBuffer?.Dispose();
                }
                candidate.Dispose();
                return result;
            }

            backBuffers[index] = backBuffer!;
        }

        candidate._backBuffers = backBuffers;
        createdSwapChain = candidate;
        return 0;
    }

    private static IDirect3DSwapChain9Ex* QuerySwapChain9Ex(IDirect3DSwapChain9* swapChain)
    {
        IDirect3DSwapChain9Ex* swapChainEx = null;
        Guid interfaceId = IDirect3DSwapChain9Ex.Guid;
        void** vtable = swapChain->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, Guid*, void**, int> queryInterface =
            (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, Guid*, void**, int>) vtable[0];
        int result = queryInterface(swapChain, &interfaceId, (void**) &swapChainEx);
        if (result < 0)
        {
            Direct3D9Factory.Release((nint) swapChainEx);
            return null;
        }

        return swapChainEx;
    }

    internal IDirect3DSwapChain9* SwapChain
    {
        get
        {
            ObjectDisposedException.ThrowIf(!IsValid || _swapChain is null, this);
            return _swapChain;
        }
    }

    internal PresentParameters GetPresentParameters()
    {
        ObjectDisposedException.ThrowIf(!IsValid || _swapChain is null, this);

        PresentParameters presentParameters = default;
        void** vtable = _swapChain->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, PresentParameters*, int> getPresentParameters =
            (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, PresentParameters*, int>) vtable[9];
        int result = getPresentParameters(_swapChain, &presentParameters);
        Marshal.ThrowExceptionForHR(result);
        return presentParameters;
    }

    internal Direct3D9Surface GetBackBuffer(uint index = 0)
    {
        int result = TryGetBackBuffer(index, out Direct3D9Surface? surface);
        Marshal.ThrowExceptionForHR(result);
        return surface ?? throw new InvalidOperationException("Direct3D swap chain returned a null back buffer interface pointer.");
    }

    internal int TryGetBackBuffer(uint index, out Direct3D9Surface? backBuffer)
    {
        ObjectDisposedException.ThrowIf(!IsValid, this);
        if (_getBackBuffer is not null)
        {
            (int hResult, backBuffer) = _getBackBuffer(index);
            return hResult;
        }
        if (_backBuffers is not null)
        {
            if (index >= _backBuffers.Length)
            {
                backBuffer = null;
                return Direct3D9Factory.InvalidArgumentHResult;
            }

            backBuffer = _backBuffers[index].AddRef();
            return 0;
        }

        return TryGetBackBufferFromNative(index, out backBuffer);
    }

    private int TryGetBackBufferFromNative(uint index, out Direct3D9Surface? backBuffer)
    {
        ObjectDisposedException.ThrowIf(_swapChain is null, this);
        backBuffer = null;
        IDirect3DSurface9* surface = null;
        void** vtable = _swapChain->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, uint, BackbufferType, IDirect3DSurface9**, int> getBackBuffer =
            (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, uint, BackbufferType, IDirect3DSurface9**, int>) vtable[5];
        int result = getBackBuffer(_swapChain, index, BackbufferType.Mono, &surface);
        if (result < 0)
        {
            Direct3D9Factory.Release(surface);
            return result;
        }
        if (surface is null)
        {
            throw new InvalidOperationException("Direct3D swap chain returned a null back buffer interface pointer.");
        }

        return Direct3D9Surface.TryCreate(_resourceManager, surface, out backBuffer);
    }

    internal Direct3D9DeviceState Present()
    {
        return Present(default);
    }

    internal Direct3D9DeviceState Present(Direct3D9PresentRequest request)
    {
        ObjectDisposedException.ThrowIf(!IsValid, this);
        if (_presentWithParameters is not null)
        {
            return new Direct3D9DeviceState(_presentWithParameters(request), Direct3D9DeviceStateSource.Present);
        }
        if (_present is not null)
        {
            return new Direct3D9DeviceState(_present(), Direct3D9DeviceStateSource.Present);
        }

        ObjectDisposedException.ThrowIf(_swapChain is null, this);
        Direct3D9SurfaceRect sourceRect = request.SourceRect.GetValueOrDefault();
        Direct3D9SurfaceRect destinationRect = request.DestinationRect.GetValueOrDefault();
        byte[]? dirtyRegionData = CreateDirtyRegionData(request.DirtyRegion);
        fixed (byte* dirtyRegion = dirtyRegionData)
        {
            void** vtable = _swapChain->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, void*, void*, nint, void*, uint, int> present =
                (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, void*, void*, nint, void*, uint, int>) vtable[3];
            int result = present(
                _swapChain,
                request.SourceRect.HasValue ? &sourceRect : null,
                request.DestinationRect.HasValue ? &destinationRect : null,
                request.DestinationWindowOverride,
                dirtyRegionData is null ? null : dirtyRegion,
                request.Flags);
            return new Direct3D9DeviceState(result, Direct3D9DeviceStateSource.Present);
        }
    }

    internal static byte[]? CreateDirtyRegionData(IReadOnlyList<Direct3D9SurfaceRect>? rectangles)
    {
        if (rectangles is null || rectangles.Count == 0)
        {
            return null;
        }

        const int headerSize = 32;
        const int rectangleSize = 16;
        byte[] data = new byte[checked(headerSize + (rectangles.Count * rectangleSize))];
        Span<int> values = MemoryMarshal.Cast<byte, int>(data.AsSpan());
        values[0] = headerSize;
        values[1] = 1;
        values[2] = rectangles.Count;
        values[3] = checked(rectangles.Count * rectangleSize);

        Direct3D9SurfaceRect bounds = GetBounds(rectangles);
        values[4] = bounds.Left;
        values[5] = bounds.Top;
        values[6] = bounds.Right;
        values[7] = bounds.Bottom;
        for (int index = 0; index < rectangles.Count; index++)
        {
            Direct3D9SurfaceRect rectangle = rectangles[index];
            int offset = 8 + (index * 4);
            values[offset] = rectangle.Left;
            values[offset + 1] = rectangle.Top;
            values[offset + 2] = rectangle.Right;
            values[offset + 3] = rectangle.Bottom;
        }

        return data;
    }

    private static Direct3D9SurfaceRect GetBounds(IReadOnlyList<Direct3D9SurfaceRect> rectangles)
    {
        Direct3D9SurfaceRect bounds = rectangles[0];
        for (int index = 1; index < rectangles.Count; index++)
        {
            Direct3D9SurfaceRect rectangle = rectangles[index];
            bounds = new Direct3D9SurfaceRect(
                Math.Min(bounds.Left, rectangle.Left),
                Math.Min(bounds.Top, rectangle.Top),
                Math.Max(bounds.Right, rectangle.Right),
                Math.Max(bounds.Bottom, rectangle.Bottom));
        }
        return bounds;
    }

    protected override void ReleaseD3DResources()
    {
        _release?.Invoke();
        Direct3D9Factory.Release(_swapChain);
        _swapChain = null;
        Direct3D9Factory.Release((nint) _swapChainEx);
        _swapChainEx = null;

        if (_backBuffers is not null)
        {
            foreach (Direct3D9Surface backBuffer in _backBuffers)
            {
                backBuffer.Dispose();
            }
            _backBuffers = null;
        }
    }
}
