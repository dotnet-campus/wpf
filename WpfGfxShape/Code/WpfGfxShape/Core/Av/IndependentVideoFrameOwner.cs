using WpfGfxShape.Abi;

namespace WpfGfxShape.Core.Av;

internal sealed class IndependentVideoFrameOwner : IDisposable
{
    private readonly object _gate = new();
    private Frame? _current;
    private bool _closed;

    private sealed record Frame(byte[] Pixels, int Width, int Height, long Timestamp);

    internal void Publish(ReadOnlySpan<byte> bgra, int codedWidth, int codedHeight,
        int x, int y, int width, int height, long timestamp)
    {
        if (codedWidth <= 0 || codedHeight <= 0 || x < 0 || y < 0 || width <= 0 || height <= 0
            || width > codedWidth || height > codedHeight || x > codedWidth - width || y > codedHeight - height
            || timestamp < 0 || bgra.Length != checked(codedWidth * codedHeight * 4))
            throw new ArgumentException(null, nameof(bgra));
        byte[] pixels = new byte[checked(width * height * 4)];
        for (int row = 0; row < height; row++)
        {
            bgra.Slice(checked(((y + row) * codedWidth + x) * 4), width * 4)
                .CopyTo(pixels.AsSpan(row * width * 4));
            // MF RGB32 has an unused fourth byte, not premultiplied alpha.
            for (int column = 0; column < width; column++) pixels[(row * width + column) * 4 + 3] = 255;
        }
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_closed, this);
            _current = new(pixels, width, height, timestamp);
        }
    }

    internal unsafe int Acquire(out SystemVideoSample? sample)
    {
        sample = null;
        Frame? frame;
        lock (_gate)
        {
            if (_closed) return unchecked((int)0x80000013);
            frame = _current;
        }
        if (frame is null) return 0;
        if (!OperatingSystem.IsWindows()) return unchecked((int)0x80004001);
        nint bitmap = 0, bitmapLock = 0, source = 0;
        try
        {
            int hr = SoftwareBitmap.Create((uint)frame.Width, (uint)frame.Height, 96, 96,
                new Guid("6fddc324-4e03-4bfe-b185-3d77768dc910"), out bitmap);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock);
            if (hr < 0) return hr;
            uint size = 0, stride = 0;
            byte* pixels = null;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)bitmapLock)[4])(bitmapLock, &stride);
            if (hr < 0) return hr;
            int rowBytes = checked(frame.Width * 4);
            if (pixels == null || stride < rowBytes || (ulong)(frame.Height - 1) * stride + (uint)rowBytes > size)
                return unchecked((int)0x80070057);
            for (int row = 0; row < frame.Height; row++)
                frame.Pixels.AsSpan(row * rowBytes, rowBytes).CopyTo(new Span<byte>(pixels + (nuint)row * stride, rowBytes));
            Direct3D9Factory.Release(bitmapLock); bitmapLock = 0;
            hr = SoftwareBitmap.Protect(bitmap);
            if (hr < 0) return hr;
            Guid iid = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)bitmap)[0])(bitmap, &iid, &source);
            if (hr < 0) return hr;
            sample = new SystemVideoSample(source, frame.Timestamp);
            source = 0;
            return hr;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000e); }
        finally
        {
            Direct3D9Factory.Release(source);
            Direct3D9Factory.Release(bitmapLock);
            Direct3D9Factory.Release(bitmap);
        }
    }

    /// <summary>Stops new acquisitions without invalidating already owned bitmap leases.</summary>
    public void Dispose()
    {
        lock (_gate) { _closed = true; _current = null; }
    }
}
