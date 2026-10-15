using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.WpfReference;

[TestClass]
public sealed unsafe class FloatReadbackTests
{
    public TestContext TestContext { get; set; } = null!;

    [STATestMethod]
    public void WhenOriginalSoftwareFloatBitmapIsWrittenThenHdrReadbackIsExact()
    {
        NativeMethods.EnsureLoaded();
        nint module = NativeLibrary.Load(NativeMethods.LibraryPath);
        nint factory = 0, target = 0, bitmap = 0, bitmapLock = 0;
        try
        {
            var create = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
            var createTarget = (delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateBitmapRenderTarget");
            var getBitmap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MILRenderTargetBitmapGetBitmap");
            Assert.AreEqual(0, create(&factory, 0x200184C0));
            Assert.AreEqual(0, createTarget(factory, 2, 2, 26, 96, 96, 1, &target));
            // Native MilColorF aliases D3DCOLORVALUE: red, green, blue, alpha.
            float* color = stackalloc float[] { 1.25f, -0.25f, 0.1234567f, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)target)[4])(target, color, 0));
            Assert.AreEqual(0, getBitmap(target, &bitmap));
            Guid format;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)bitmap)[4])(bitmap, &format));
            Assert.AreEqual(new Guid("6fddc324-4e03-4bfe-b185-3d77768dc91a"), format);
            Vector4[] pixels = new Vector4[4];
            fixed (Vector4* values = pixels)
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(
                    bitmap, 0, 32, 64, (byte*)values));
            var clearPixels = pixels.ToArray();
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(
                bitmap, 0, 2, &bitmapLock));
            uint stride, size; byte* data;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)bitmapLock)[4])(bitmapLock, &stride));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &data));
            Assert.IsTrue(data != null && stride >= 32 && size >= stride + 32);
            Vector4 expected = new(1.25f, -0.25f, 0.1234567f, 1);
            for (int y = 0; y < 2; y++)
            for (int x = 0; x < 2; x++) ((Vector4*)(data + y * stride))[x] = expected;
            Release(bitmapLock); bitmapLock = 0;
            fixed (Vector4* values = pixels)
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(
                    bitmap, 0, 32, 64, (byte*)values));
            CollectionAssert.AreEqual(Enumerable.Repeat(expected, 4).ToArray(), pixels);
            string directory = typeof(FloatReadbackTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ReferenceResultsPath").Value
                ?? throw new InvalidOperationException("The reference results directory is not configured.");
            Directory.CreateDirectory(directory);
            string report = Path.Combine(directory, $"original-float-readback-{Guid.NewGuid():N}.json");
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                NativeDll = NativeMethods.LibraryPath,
                NativeSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(NativeMethods.LibraryPath))),
                Contract = "MILFactoryCreateBitmapRenderTarget format 26, SoftwareOnly, WIC write lock and CopyPixels; Clear is observational only",
                ClearPixelsPrgbaFloat = clearPixels.Select(pixel => new[] { pixel.X, pixel.Y, pixel.Z, pixel.W }).ToArray(),
                PixelFormat = format, Width = 2, Height = 2, Stride = 32,
                InputColorArgb = new float[] { 1, 1.25f, -0.25f, 0.1234567f },
                PixelsPrgbaFloat = pixels.Select(pixel => new[] { pixel.X, pixel.Y, pixel.Z, pixel.W }).ToArray(),
                Scope = "Float storage/readback only; Clear quantizes/clamps in this original runtime; not a DrawingImage or rotation golden"
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddResultFile(report);
            Console.WriteLine($"Original float readback report: {report}");
        }
        finally
        {
            Release(bitmapLock); Release(bitmap); Release(target); Release(factory);
            NativeLibrary.Free(module);
        }
    }

    [STATestMethod]
    [DataRow(6u)]
    [DataRow(10u)]
    public void WhenOriginalFloatTargetDrawsBitmapThenSrgbClampingIsRecorded(uint width)
    {
        NativeMethods.EnsureLoaded();
        nint module = NativeLibrary.Load(NativeMethods.LibraryPath);
        nint factory = 0, target = 0, sourceTarget = 0, source = 0, output = 0, sourceBitmap = 0, bitmapLock = 0, view = 0;
        try
        {
            var create = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
            var createTarget = (delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateBitmapRenderTarget");
            var getBitmap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MILRenderTargetBitmapGetBitmap");
            Assert.AreEqual(0, create(&factory, 0x200184C0));
            Assert.AreEqual(0, createTarget(factory, width, 1, 26, 96, 96, 1, &target));
            Assert.AreEqual(0, createTarget(factory, 2, 1, 26, 96, 96, 1, &sourceTarget));
            Assert.AreEqual(0, getBitmap(target, &output));
            Assert.AreEqual(0, getBitmap(sourceTarget, &sourceBitmap));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)sourceTarget)[7])(sourceTarget, &source));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)sourceBitmap)[8])(sourceBitmap, 0, 2, &bitmapLock));
            uint size; byte* data;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &data));
            Assert.IsTrue(data != null && size >= 32);
            Vector4 first = new(1.25f, -0.25f, 0.1234567f, 1), second = new(2.5f, -0.5f, 0.7654321f, 1);
            ((Vector4*)data)[0] = first; ((Vector4*)data)[1] = second;
            Release(bitmapLock); bitmapLock = 0;
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            byte* context = stackalloc byte[2048]; byte* state = stackalloc byte[116];
            new Span<byte>(context, 2048).Clear(); new Span<byte>(state, 116).Clear();
            *(int*)(context + 376) = 1; *(nint*)(context + 400) = (nint)state;
            *(Matrix4x4*)(context + 408) = Matrix4x4.CreateScale(width / 2f, 1, 1);
            *(uint*)(state + 84) = 1; *(uint*)(state + 96) = 1; *(uint*)(state + 100) = 1;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, int>)(*(void***)view)[8])(view, (nint)context, source, 0));
            Guid format;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)output)[4])(output, &format));
            Assert.AreEqual(new Guid("6fddc324-4e03-4bfe-b185-3d77768dc91a"), format);
            Vector4[] pixels = new Vector4[width];
            fixed (Vector4* values = pixels)
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, width * 16, width * 16, (byte*)values));
            CollectionAssert.AreEqual(Enumerable.Repeat(new Vector3(1, 0, 1), (int)width).ToArray(),
                pixels.Select(pixel => new Vector3(pixel.X, pixel.Y, pixel.W)).ToArray());
            string directory = typeof(FloatReadbackTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ReferenceResultsPath").Value
                ?? throw new InvalidOperationException("The reference results directory is not configured.");
            Directory.CreateDirectory(directory);
            string report = Path.Combine(directory, $"original-float-draw-{Guid.NewGuid():N}.json");
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                NativeDll = NativeMethods.LibraryPath,
                NativeSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(NativeMethods.LibraryPath))),
                Contract = "Original Release/x64 IRenderTargetInternal.DrawBitmap, format26, SourceCopy, linear, no prefilter",
                Width = width, Height = 1, PixelFormat = format,
                InputPixelsPrgbaFloat = new[] { new[] { first.X, first.Y, first.Z, first.W }, new[] { second.X, second.Y, second.Z, second.W } },
                PixelsPrgbaFloat = pixels.Select(pixel => new[] { pixel.X, pixel.Y, pixel.Z, pixel.W }).ToArray(),
                Scope = "Actual native bitmap sampling clamps HDR through sRGB despite float storage; not an HDR or DrawingImage golden"
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddResultFile(report);
            Console.WriteLine($"Original float draw report: {report}");
        }
        finally
        {
            Release(bitmapLock); Release(view); Release(output); Release(sourceBitmap); Release(source); Release(sourceTarget); Release(target); Release(factory);
            NativeLibrary.Free(module);
        }
    }

    private static void Release(nint value)
    {
        if (value != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
    }
}
