using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

internal readonly record struct FrozenBlurEffect(double Radius, MilKernelType Kernel, MilEffectRenderingBias Bias)
{
    internal MilRectD Inflate(MilRectD bounds) => bounds.Width <= 0 || bounds.Height <= 0 ? bounds
        : new(bounds.X - Radius, bounds.Y - Radius, bounds.Width + 2 * Radius, bounds.Height + 2 * Radius);
}

internal sealed unsafe class GeneratedSoftwareBlurEffect(
    GeneratedVisualRenderer.VisualSnapshot input, GeneratedShaderInputScope scope,
    FrozenBlurEffect data, int radius, FrozenDropShadowEffect? shadow = null) : IDisposable
{
    private bool _disposed;

    internal int Draw(SoftwareImageRenderSession session, SoftwareImageDrawingContext context)
    {
        if (_disposed) return Direct3D9Factory.WgxInvalidCallHResult;
        nint source = 0, output = 0;
        try
        {
            int hr = input.Realize(out source, floating: false);
            if (hr < 0) return hr;
            int width = checked((int)scope.Width), height = checked((int)scope.Height);
            uint[] pixels = new uint[checked(width * height)];
            fixed (uint* destination = pixels)
                hr = ((delegate* unmanaged[Stdcall]<nint, Direct3D9BitmapSourceRectangle*, uint, uint, byte*, int>)(*(void***)source)[7])
                    (source, null, checked(scope.Width * 4), checked((uint)pixels.Length * 4), (byte*)destination);
            if (hr < 0) return hr;
            uint[]? original = shadow is not null ? (uint[])pixels.Clone() : null;
            if (radius > 0)
            {
                uint[] intermediate = new uint[pixels.Length];
                if (data.Kernel == MilKernelType.Box)
                {
                    Box(pixels, intermediate, width, height, radius);
                    pixels = intermediate;
                }
                else
                {
                    float[] weights = Weights(radius, data.Kernel);
                    Pass(pixels, intermediate, width, height, radius, true, weights);
                    Array.Clear(pixels);
                    Pass(intermediate, pixels, width, height, radius, false, weights);
                }
            }
            if (shadow is { } shadowData && original is not null)
            {
                uint[] composite = new uint[pixels.Length];
                double depth = shadowData.Depth * Math.Min((float)scope.InputTransform.ScaleX, (float)scope.InputTransform.ScaleY);
                int dx = (int)(float)(depth * Math.Cos(shadowData.Direction * Math.PI / 180));
                int dy = (int)(float)(-depth * Math.Sin(shadowData.Direction * Math.PI / 180));
                uint opacity = (uint)(shadowData.Opacity * 255);
                uint red = (uint)(shadowData.Color.Red * 255), green = (uint)(shadowData.Color.Green * 255), blue = (uint)(shadowData.Color.Blue * 255);
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        uint sourcePixel = original[y * width + x];
                        int sx = x - dx, sy = y - dy;
                        uint alpha = sx >= 0 && sx < width && sy >= 0 && sy < height ? pixels[sy * width + sx] >> 24 : 0;
                        uint combination = alpha * (255 - (sourcePixel >> 24)) * opacity / 65536;
                        composite[y * width + x] = ((combination + (sourcePixel >> 24)) << 24)
                            | ((combination * red / 255 + ((sourcePixel >> 16) & 255)) << 16)
                            | ((combination * green / 255 + ((sourcePixel >> 8) & 255)) << 8)
                            | (combination * blue / 255 + (sourcePixel & 255));
                    }
                pixels = composite;
            }
            hr = SoftwareBitmap.Create(scope.Width, scope.Height, 96, 96,
                new Guid("6fddc324-4e03-4bfe-b185-3d77768dc910"), out output);
            if (hr < 0) return hr;
            var bitmap = (SoftwareBitmap.Bitmap*)output;
            for (int y = 0; y < height; y++)
                pixels.AsSpan(y * width, width).CopyTo(new Span<uint>(bitmap->Pixels + y * bitmap->Stride, width));
            return session.Draw(output, new(0, 0, scope.Width, scope.Height), false,
                context with { WorldToDevice = scope.OutputTransform });
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000e); }
        catch (OverflowException) { return Direct3D9Factory.InvalidArgumentHResult; }
        finally { Direct3D9Factory.Release(output); Direct3D9Factory.Release(source); }
    }

    // Match BlurEffect.cpp: float weights with additive normalization, and byte quantization after each pass.
    private static float[] Weights(int radius, MilKernelType kernel)
    {
        float[] weights = new float[radius + 1];
        double sum = 0;
        for (int i = 0; i <= radius; i++)
        {
            double sd = radius / 3.0;
            weights[i] = kernel == MilKernelType.Box ? (float)(1.0 / (2 * radius + 1))
                : (float)(Math.Exp(-(double)i * i / (2 * sd * sd)) / (sd * Math.Sqrt(2 * Math.PI)));
            sum += weights[i];
            if (i != 0) sum += weights[i];
        }
        float correction = (float)((1 - sum) / (2 * radius + 1));
        for (int i = 0; i <= radius; i++) weights[i] += correction;
        return weights;
    }

    // Native CPU Box accumulates integer column sums and quantizes only the complete 2D window.
    private static void Box(uint[] source, uint[] destination, int width, int height, int radius)
    {
        int length = checked(2 * radius + 1);
        float divisor = checked(length * length);
        int[] columns = new int[checked(width * 4)];
        for (int y = 0; y < length; y++)
            for (int x = 0; x < width; x++)
                for (int c = 0; c < 4; c++)
                    columns[x * 4 + c] += (int)((source[y * width + x] >> (c * 8)) & 255);
        for (int y = radius; y < height - radius; y++)
        {
            for (int c = 0; c < 4; c++)
            {
                int sum = 0;
                for (int x = 0; x < length; x++) sum += columns[x * 4 + c];
                for (int x = radius; x < width - radius; x++)
                {
                    destination[y * width + x] |= (uint)Math.Clamp((int)MathF.Round(sum / divisor, MidpointRounding.ToEven), 0, 255) << (c * 8);
                    if (x + radius + 1 < width)
                        sum += columns[(x + radius + 1) * 4 + c] - columns[(x - radius) * 4 + c];
                }
            }
            if (y + radius + 1 < height)
                for (int x = 0; x < width; x++)
                    for (int c = 0; c < 4; c++)
                        columns[x * 4 + c] += (int)((source[(y + radius + 1) * width + x] >> (c * 8)) & 255)
                            - (int)((source[(y - radius) * width + x] >> (c * 8)) & 255);
        }
    }

    private static void Pass(uint[] source, uint[] destination, int width, int height, int radius,
        bool vertical, float[] weights)
    {
        for (int y = vertical ? radius : 0; y < (vertical ? height - radius : height); y++)
            for (int x = vertical ? 0 : radius; x < (vertical ? width : width - radius); x++)
            {
                uint pixel = 0;
                for (int channel = 0; channel < 32; channel += 8)
                {
                    float sum = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int index = (vertical ? y + k : y) * width + (vertical ? x : x + k);
                        sum += ((source[index] >> channel) & 255) * weights[Math.Abs(k)];
                    }
                    pixel |= (uint)Math.Clamp((int)MathF.Round(sum, MidpointRounding.ToEven), 0, 255) << channel;
                }
                destination[y * width + x] = pixel;
            }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        input.Dispose();
    }
}
