using System.Numerics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

// A bounded DirectWrite texture, not a second rasterization of the target surface.
internal sealed unsafe class GeneratedGlyphAlpha
{
    [StructLayout(LayoutKind.Sequential)]
    private struct GlyphRun
    {
        internal nint Face;
        internal float EmSize;
        internal uint Count;
        internal ushort* Indices;
        internal float* Advances;
        internal Direct3D9GlyphOffset* Offsets;
        internal int Sideways;
        internal uint Bidi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rectangle
    {
        internal int Left, Top, Right, Bottom;
    }

    private readonly byte[] _alpha;
    private readonly Rectangle _bounds;
    private GeneratedGlyphAlpha(byte[] alpha, Rectangle bounds) { _alpha = alpha; _bounds = bounds; }
    internal MilRectD Bounds => new(_bounds.Left, _bounds.Top,
        (double)_bounds.Right - _bounds.Left, (double)_bounds.Bottom - _bounds.Top);

    internal float Sample(int x, int y)
    {
        long column = (long)x - _bounds.Left, row = (long)y - _bounds.Top;
        long width = (long)_bounds.Right - _bounds.Left;
        if (column < 0 || column >= width || row < 0 || row >= (long)_bounds.Bottom - _bounds.Top) return 0;
        int index = checked((int)((row * width + column) * 3));
        // CSWGlyphRunPainter samples the center subpixel (+1), not an RGB average.
        return _alpha[index + 1] / 255f;
    }

    internal uint ApplyGrayscaleOver(int x, int y, uint source, uint destination)
    {
        uint coverage = (uint)MathF.Round(Sample(x, y) * 255);
        uint opacity = source >> 24;
        if (coverage == 0 || opacity == 0) return destination;
        if ((coverage & opacity) == 255) return source;
        uint corrected = ApplyGrayscale(x, y, source, false);
        uint inverse = 255 - (corrected >> 24);
        uint result = 0;
        for (int component = 0; component < 4; component++)
            result |= ((((destination >> (component * 8)) & 255) * inverse >> 8)
                + ((corrected >> (component * 8)) & 255)) << (component * 8);
        return result;
    }

    internal uint ApplyGrayscale(int x, int y, uint source, bool copy = true)
    {
        uint alpha = (uint)MathF.Round(Sample(x, y) * 255);
        if (alpha == 0) return 0;
        if (copy && alpha == 255) return source;
        uint opacity = source >> 24;
        if (opacity == 0) return 0;
        uint reciprocal = 0xFF0000 / opacity;
        uint red = (((source >> 16) & 255) * reciprocal) >> 16;
        uint green = (((source >> 8) & 255) * reciprocal) >> 16;
        uint blue = ((source & 255) * reciprocal) >> 16;
        alpha = (alpha * opacity) >> 8;
        uint luminance = (red + green + green + blue) >> 2;
        var row = _gamma[alpha];
        uint corrected = row.F1 + ((row.F2 * luminance) >> 8);
        return (corrected << 24) | (((red * corrected) >> 8) << 16)
            | (((green * corrected) >> 8) << 8) | ((blue * corrected) >> 8);
    }

    private (uint F1, uint F2)[] _gamma = new (uint, uint)[256];

    private void InitializeGamma(float gamma)
    {
        // CGammaHandler::sc_gammaRatios and CalculateGammaTable, with the native float rounding.
        (float G1, float G2, float G3, float G4)[] ratios =
        [
            (0, 0, 0, 0), (.0166f, -.0807f, .2227f, -.0751f),
            (.0350f, -.1760f, .4325f, -.1370f), (.0543f, -.2821f, .6302f, -.1876f),
            (.0739f, -.3963f, .8167f, -.2287f), (.0933f, -.5161f, .9926f, -.2616f),
            (.1121f, -.6395f, 1.1588f, -.2877f), (.1300f, -.7649f, 1.3159f, -.3080f),
            (.1469f, -.8911f, 1.4644f, -.3234f), (.1627f, -1.0170f, 1.6051f, -.3347f),
            (.1773f, -1.1420f, 1.7385f, -.3426f), (.1908f, -1.2652f, 1.8650f, -.3476f),
            (.2031f, -1.3864f, 1.9851f, -.3501f)
        ];
        int index = gamma < 1 ? 0 : Math.Min((int)((gamma - 1) * 10), 12);
        var r = ratios[index];
        float norm13 = (float)(65536d / (255 * 255) * 4);
        float norm24 = (float)(256d / 255 * 4);
        float g1 = norm13 * (r.G1 / 4), g2 = norm24 * (r.G2 / 4);
        float g3 = norm13 * (r.G3 / 4), g4 = norm24 * (r.G4 / 4);
        for (int i = 0; i < 256; i++)
        {
            float a = i * (float)(1d / 255);
            float f1 = a + a * (1 - a) * (g2 * a + g4);
            float f2 = a * (1 - a) * (g1 * a + g3);
            _gamma[i] = ((uint)MathF.Round(f1 * 255), (uint)MathF.Round(f2 * 255));
        }
    }

    [DllImport("dwrite.dll", ExactSpelling = true)]
    private static extern int DWriteCreateFactory(uint type, in Guid iid, out nint factory);

    internal static int Create(GeneratedGlyphRunResource resource, GeneratedImageTransform transform,
        Direct3D9SurfaceRect? clip, out GeneratedGlyphAlpha? alpha)
    {
        alpha = null;
        var command = resource.Command;
        nint face = resource.FontFace;
        if (face == 0) return Direct3D9Factory.WgxInvalidCallHResult;
        // Hold the borrowed face before the first external call; a callback can replace the resource.
        Direct3D9Factory.AddRef(face);
        ushort[] indices = resource.Indices;
        float[] advances = resource.Advances;
        Direct3D9GlyphOffset[] offsets = resource.Offsets;
        nint factory = 0, analysis = 0;
        try
        {
            if (command.EmSize == 0) { alpha = new([], default); return 0; }
            var origin = transform.Apply(command.OriginX, command.OriginY);
            Matrix3x2 matrix = new((float)transform.ScaleX, (float)transform.M12,
                (float)transform.M21, (float)transform.ScaleY, (float)origin.X, (float)origin.Y);
            int hr = DWriteCreateFactory(0, DirectWriteFactory.InterfaceId, out factory);
            if (hr < 0) return hr;
            if (factory == 0) return unchecked((int)0x80004003);
            fixed (ushort* indexPointer = indices)
            fixed (float* advancePointer = advances)
            fixed (Direct3D9GlyphOffset* offsetPointer = offsets)
            {
                GlyphRun run = new() { Face = face, EmSize = command.EmSize, Count = (uint)indices.Length,
                    Indices = indexPointer, Advances = advancePointer, Offsets = offsetPointer,
                    Sideways = (command.Flags & 1) != 0 ? 1 : 0, Bidi = command.BidiLevel };
                hr = ((delegate* unmanaged[Stdcall]<nint, GlyphRun*, float, Matrix3x2*, uint, uint, float, float, nint*, int>)
                    (*(void***)factory)[23])(factory, &run, 1, &matrix, 4, command.MeasuringMethod, 0, 0, &analysis);
            }
            if (hr < 0) return hr;
            if (analysis == 0) return unchecked((int)0x80004003);
            Rectangle bounds = default;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint, Rectangle*, int>)(*(void***)analysis)[3])(analysis, 1, &bounds);
            if (hr < 0) return hr;
            if (clip is { } clipping)
            {
                bounds.Left = Math.Max(bounds.Left, clipping.Left); bounds.Top = Math.Max(bounds.Top, clipping.Top);
                bounds.Right = Math.Min(bounds.Right, clipping.Right); bounds.Bottom = Math.Min(bounds.Bottom, clipping.Bottom);
            }
            if (bounds.Right <= bounds.Left || bounds.Bottom <= bounds.Top) { alpha = new([], default); return 0; }
            long size = checked(((long)bounds.Right - bounds.Left) * ((long)bounds.Bottom - bounds.Top) * 3);
            if (size > int.MaxValue) return Direct3D9Factory.OutOfMemoryHResult;
            byte[] pixels = new byte[(int)size];
            fixed (byte* pointer = pixels)
                hr = ((delegate* unmanaged[Stdcall]<nint, uint, Rectangle*, byte*, uint, int>)(*(void***)analysis)[4])
                    (analysis, 1, &bounds, pointer, (uint)pixels.Length);
            if (hr < 0) return hr;
            alpha = new(pixels, bounds);
            nint parameters = 0;
            try
            {
                hr = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)factory)[10])(factory, &parameters);
                if (hr < 0) return hr;
                if (parameters == 0) return unchecked((int)0x80004003);
                float gamma = 0, contrast = 0, clearType = 0;
                hr = ((delegate* unmanaged[Stdcall]<nint, nint, float*, float*, float*, int>)(*(void***)analysis)[5])
                    (analysis, parameters, &gamma, &contrast, &clearType);
                if (hr < 0) return hr;
                // Offscreen targets use default display settings: AllowGamma remains false.
                // glyphrunslave.cpp GetGammaTable therefore selects index zero, independently of analysis gamma.
                alpha.InitializeGamma(1);
                for (int i = 0; i < pixels.Length; i++)
                {
                    float value = pixels[i] * (1f / 255);
                    float adjusted = value * (contrast + 1) / (value * contrast + 1);
                    pixels[i] = (byte)(adjusted * 255 + 0.5f);
                }
            }
            finally { Direct3D9Factory.Release(parameters); }
            return 0;
        }
        catch (OverflowException) { return Direct3D9Factory.InvalidArgumentHResult; }
        finally
        {
            Direct3D9Factory.Release(analysis);
            Direct3D9Factory.Release(factory);
            Direct3D9Factory.Release(face);
        }
    }
}
