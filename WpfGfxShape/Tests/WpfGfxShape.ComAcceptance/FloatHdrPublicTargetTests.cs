using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class FloatHdrPublicTargetTests
{
    public TestContext TestContext { get; set; } = null!;

    /// <summary>Records the installed original and candidate public float clear paths without an eight-bit oracle.</summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenPublicFloatTargetClearsHdrThenRawReadbackAndLockedRetryAreRecorded(bool original)
    {
        string path = original
            ? Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.Visual).Assembly.Location)!, "wpfgfx_cor3.dll")
            : AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        using var wic = new SystemWicBitmap();
        nint factory = 0, bitmap = 0, target = 0, heldLock = 0;
        Vector4 input = new(1.25f, -0.25f, 0.1234567f, 0.5f);
        try
        {
            var create = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
            var createTarget = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateSWRenderTargetForBitmap");
            Assert.AreEqual(0, create(&factory, 0x200184C0));
            bitmap = wic.Create(2, 2, floating: true);
            Guid format;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)bitmap)[4])(bitmap, &format));
            Assert.AreEqual(new Guid("6fddc324-4e03-4bfe-b185-3d77768dc91a"), format);
            int createResult = createTarget(factory, bitmap, &target);
            Assert.AreEqual(0, createResult);
            var clear = (delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)target)[4];
            int firstResult = clear(target, (float*)&input, 0);
            Assert.AreEqual(0, firstResult);
            Vector4[] first = Read(bitmap);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &heldLock));
            Vector4 replacement = new(0.125f, 0.25f, 0.375f, 1);
            int lockedResult = clear(target, (float*)&replacement, 0);
            Release(heldLock); heldLock = 0;
            Vector4[] afterFailure = Read(bitmap);
            int retryResult = clear(target, (float*)&input, 0);
            Assert.AreEqual(0, retryResult);
            Vector4[] afterRetry = Read(bitmap);
            Release(target); target = 0;
            Release(factory); factory = 0;
            Vector4[] afterOwnersReleased = Read(bitmap);
            string report = Path.Combine(AcceptancePaths.Results, $"float-hdr-public-target-{original}.json");
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                Original = original, NativePath = path, NativeVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(path).FileVersion, Runtime = Environment.Version.ToString(), NativeSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                Target = "System WIC IWICBitmap Prgba128Float / MILFactoryCreateSWRenderTargetForBitmap / public Clear",
                PixelFormat = format, Width = 2, Height = 2, Input = Channels(input),
                CreateResult = createResult, FirstResult = firstResult, LockedResult = lockedResult, RetryResult = retryResult,
                First = first.Select(Channels), AfterFailure = afterFailure.Select(Channels), AfterRetry = afterRetry.Select(Channels),
                AfterOwnersReleased = afterOwnersReleased.Select(Channels),
                RawFloatBuffer = Convert.ToHexString(MemoryMarshal.AsBytes(first.AsSpan())),
                Scope = "Public Clear only; not source17/18 drawing or shader same-format golden; no padding contract. DLL remains loaded."
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddResultFile(report);
            Assert.AreEqual(unchecked((int)0x88982F0D), lockedResult);
            CollectionAssert.AreEqual(first, afterFailure);
            CollectionAssert.AreEqual(first, afterRetry);
            CollectionAssert.AreEqual(first, afterOwnersReleased);
            if (original)
                CollectionAssert.AreEqual(Enumerable.Repeat(new Vector4(0.5019608f, 0, 0.06000361f, 0.5019608f), 4).ToArray(), first);
            if (!original)
            {
                CollectionAssert.AreEqual(Enumerable.Repeat(input * new Vector4(input.W, input.W, input.W, 1), 4).ToArray(), first);
                Assert.AreEqual(unchecked((int)0x88982F0D), lockedResult);
                CollectionAssert.AreEqual(first, afterFailure);
                CollectionAssert.AreEqual(first, afterRetry);
                CollectionAssert.AreEqual(first, afterOwnersReleased);
            }
        }
        finally { Release(heldLock); Release(target); Release(bitmap); Release(factory); }
    }

    private static Vector4[] Read(nint bitmap)
    {
        Vector4[] pixels = new Vector4[4];
        fixed (Vector4* data = pixels)
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 32, 64, (byte*)data));
        return pixels;
    }

    private static float[] Channels(Vector4 value) => [value.X, value.Y, value.Z, value.W];
    private static void Release(nint value)
    {
        if (value != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
    }
}
