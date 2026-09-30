using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe partial class BitmapRenderTargetTests
{
    [TestMethod]
    [TestCategory("InternalTargetContract")]
    public void WhenBitmapTargetBeginsNativeFrameThenInternalTargetCanBeAcquired()
    {
        var api = new Api();
        nint factory = 0, target = 0, internalTarget = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, 16, 96, 96, 1, &target));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            int result = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &internalTarget);
            Assert.AreEqual(0, result);
            Assert.AreNotEqual((nint)0, internalTarget);
            uint type = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)internalTarget)[18])(internalTarget, &type));
            Assert.AreEqual(0x400u, type);
            float* matrix = (float*)((delegate* unmanaged[Stdcall]<nint, nint>)(*(void***)internalTarget)[7])(internalTarget);
            Assert.AreEqual((1f, 1f, 1f, 1f), (matrix[0], matrix[5], matrix[10], matrix[15]));
        }
        finally { Release(internalTarget); Release(target); Release(factory); }
    }

    [TestMethod]
    [TestCategory("InternalTargetContract")]
    [DataRow(1u)]
    [DataRow(2u)]
    [DataRow(3u)]
    [DataRow(4u)]
    [DataRow(5u)]
    public void WhenInternalDrawBitmapConsumesContextThenPixelsAndTargetLockFailureMatch(uint interpolation)
    {
        var api = new Api();
        nint factory = 0, target = 0, sourceTarget = 0, source = 0, output = 0, view = 0, heldLock = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, 16, 96, 96, 1, &target));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, 16, 96, 96, 1, &sourceTarget));
            Assert.AreEqual(0, api.GetBitmap(target, &output));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)sourceTarget)[7])(sourceTarget, &source));
            float* red = stackalloc float[] { 1, 0, 0, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)sourceTarget)[4])(sourceTarget, red, 0));
            float* green = stackalloc float[] { 0, 1, 0, 1 };
            AliasedClip sourceClip = new() { Left = 1, Top = 0, Right = 2, Bottom = 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)sourceTarget)[4])(sourceTarget, green, (nint)(&sourceClip)));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            Assert.AreEqual(8, IntPtr.Size);
            byte* context = stackalloc byte[2048];
            byte* state = stackalloc byte[116];
            new Span<byte>(context, 2048).Clear(); new Span<byte>(state, 116).Clear();
            *(int*)(context + 376) = 1;
            *(nint*)(context + 400) = (nint)state;
            *(System.Numerics.Matrix4x4*)(context + 408) = System.Numerics.Matrix4x4.CreateTranslation(1, 0, 0);
            *(System.Numerics.Matrix4x4*)(state + 4) = System.Numerics.Matrix4x4.Identity;
            *(uint*)(state + 84) = interpolation;
            *(float*)(state + 92) = MathF.Sqrt(2);
            *(uint*)(state + 96) = 1;
            var draw = (delegate* unmanaged[Stdcall]<nint, nint, nint, nint, int>)(*(void***)view)[8];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)output)[8])(output, 0, 2, &heldLock));
            Assert.AreEqual(unchecked((int)0x88982F0D), draw(view, (nint)context, source, 0));
            Release(heldLock); heldLock = 0;
            Assert.AreEqual(0, draw(view, (nint)context, source, 0));
            uint* pixels = stackalloc uint[2];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, pixels));
            Assert.AreEqual((0u, 0xffff0000u), (pixels[0], pixels[1]));
            Assert.AreEqual(0, api.Clear(target));
            *(System.Numerics.Matrix4x4*)(context + 408) = System.Numerics.Matrix4x4.Identity;
            *(uint*)state = 1;
            *(int*)(state + 68) = 1; *(int*)(state + 76) = 1; *(int*)(state + 80) = 1;
            Assert.AreEqual(0, draw(view, (nint)context, source, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, pixels));
            Assert.AreEqual((0u, 0xff00ff00u), (pixels[0], pixels[1]));
            Assert.AreEqual(0, api.Clear(target));
            *(int*)(state + 76) = 0;
            Assert.AreEqual(0, draw(view, (nint)context, source, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, pixels));
            Assert.AreEqual((0u, 0u), (pixels[0], pixels[1]));
            *(uint*)state = 0;
            Assert.AreEqual(0, draw(view, (nint)context, source, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, pixels));
            Assert.AreEqual((0xffff0000u, 0xff00ff00u), (pixels[0], pixels[1]));
            Assert.AreEqual(0, api.Clear(target));
            *(int*)(context + 376) = 1;
            *(System.Numerics.Matrix4x4*)(context + 408) = System.Numerics.Matrix4x4.CreateScale(0.5f, 1, 1);
            *(byte*)(state + 88) = 1; *(float*)(state + 92) = 2;
            Assert.AreEqual(0, draw(view, (nint)context, source, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, pixels));
            Assert.AreEqual((0xff808000u, 0u), (pixels[0], pixels[1]));
            *(float*)(state + 92) = float.NaN;
            Assert.AreEqual(unchecked((int)0x80070057), draw(view, (nint)context, source, 0));
            *(float*)(state + 92) = 1;
            Assert.AreEqual(0, api.Clear(target));
            Assert.AreEqual(0, draw(view, (nint)context, source, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, pixels));
            Assert.AreEqual((0xff808000u, 0u), (pixels[0], pixels[1]));
            *(uint*)(state + 84) = 0;
            Assert.AreEqual(0, api.Clear(target));
            Assert.AreEqual(0, draw(view, (nint)context, source, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, pixels));
            Assert.AreEqual((0xff808000u, 0u), (pixels[0], pixels[1]));
            *(byte*)(state + 88) = 0;
            Assert.AreEqual(0, api.Clear(target));
            Assert.AreEqual(0, draw(view, (nint)context, source, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, pixels));
            Assert.AreEqual((0xff00ff00u, 0u), (pixels[0], pixels[1]));
            *(uint*)(state + 84) = interpolation;
            *(System.Numerics.Matrix4x4*)(context + 408) = System.Numerics.Matrix4x4.Identity;
            Assert.AreEqual(0, api.Clear(target));
            *(int*)(context + 376) = 0;
            *(float*)(context + 380) = 0.6f; *(float*)(context + 384) = 0;
            *(float*)(context + 388) = 1.6f; *(float*)(context + 392) = 1;
            Assert.AreEqual(0, draw(view, (nint)context, source, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, pixels));
            Assert.AreEqual((0u, 0xff00ff00u), (pixels[0], pixels[1]));
            *(int*)(context + 376) = 1;
            *(System.Numerics.Matrix4x4*)(context + 408) = System.Numerics.Matrix4x4.CreateTranslation(0.5f, 0, 0);
            *(uint*)(state + 96) = 0;
            Assert.AreEqual(0, api.Clear(target));
            Assert.AreEqual(0, draw(view, (nint)context, source, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, pixels));
            Assert.AreEqual((0xffff0000u, 0xff808000u), (pixels[0], pixels[1]));
            *(uint*)(state + 96) = 1;
            Assert.AreEqual(0, api.Clear(target));
            Assert.AreEqual(0, draw(view, (nint)context, source, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, pixels));
            Assert.AreEqual((0x80800000u, 0xff808000u), (pixels[0], pixels[1]));
        }
        finally { Release(heldLock); Release(view); Release(output); Release(source); Release(sourceTarget); Release(target); Release(factory); }
    }

    [TestMethod]
    [TestCategory("InternalTargetContract")]
    public void WhenClearTypeHintChangesThenInternalViewRemainsUsable()
    {
        var api = new Api();
        nint factory = 0, target = 0, view = 0, secondView = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, 16, 96, 96, 1, &target));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, byte, int>)(*(void***)view)[19])(view, 1));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &secondView));
            Assert.AreEqual(view, secondView);
            Release(target); target = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, byte, int>)(*(void***)secondView)[19])(secondView, 0));
            float* color = stackalloc float[] { 1, 0, 0, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)view)[4])(view, color, 0));
        }
        finally { Release(secondView); Release(view); Release(target); Release(factory); }
    }

    [TestMethod]
    [TestCategory("InternalTargetContract")]
    [DataRow(0ul)]
    [DataRow(1ul)]
    public void WhenInternalCreatorCreatesBitmapThenItSurvivesParentAndWritesPixels(ulong usage)
    {
        var api = new Api();
        nint factory = 0, target = 0, internalTarget = 0, intermediate = 0, bitmap = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, 16, 144, 144, 1, &target));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &internalTarget));
            nint creator = internalTarget + sizeof(nint);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, ulong, uint, nint*, nint, int>)(*(void***)creator)[0])(creator, 2, 1, usage, 0, &intermediate, 0));
            Release(internalTarget); internalTarget = 0;
            Release(target); target = 0;
            Release(factory); factory = 0;
            Assert.AreEqual(0, api.GetBitmap(intermediate, &bitmap));
            double x, y;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, double*, double*, int>)(*(void***)bitmap)[5])(bitmap, &x, &y));
            Assert.AreEqual((96d, 96d), (x, y));
            float* color = stackalloc float[] { 0, 1, 0, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)intermediate)[4])(intermediate, color, 0));
            Release(intermediate); intermediate = 0;
            uint* pixels = stackalloc uint[2];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)bitmap)[7])(bitmap, 0, 8, 8, pixels));
            Assert.AreEqual((0xff00ff00u, 0xff00ff00u), (pixels[0], pixels[1]));
        }
        finally { Release(bitmap); Release(intermediate); Release(internalTarget); Release(target); Release(factory); }
    }

    [TestMethod]
    [TestCategory("InternalTargetContract")]
    public void WhenInternalViewOutlivesBitmapViewThenClearWritesRetainedBitmap()
    {
        var api = new Api();
        nint factory = 0, target = 0, bitmap = 0, internalTarget = 0, identity = 0, originalIdentity = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, 16, 96, 96, 1, &target));
            Assert.AreEqual(0, api.GetBitmap(target, &bitmap));
            Guid unknown = new("00000000-0000-0000-c000-000000000046");
            Guid internalId = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &unknown, &originalIdentity));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &internalId, &internalTarget));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)internalTarget)[0])(internalTarget, &unknown, &identity));
            Assert.AreEqual(originalIdentity, identity);
            Release(identity); identity = 0;
            Release(originalIdentity); originalIdentity = 0;
            Release(target); target = 0;
            Release(factory); factory = 0;
            float* color = stackalloc float[] { 1, 0, 0, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)internalTarget)[4])(internalTarget, color, 0));
            Release(internalTarget); internalTarget = 0;
            uint* pixels = stackalloc uint[2];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)bitmap)[7])(bitmap, 0, 8, 8, pixels));
            Assert.AreEqual((0xffff0000u, 0xffff0000u), (pixels[0], pixels[1]));
        }
        finally
        {
            Release(identity); Release(originalIdentity); Release(internalTarget);
            Release(bitmap); Release(target); Release(factory);
        }
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    public void WhenExistingTargetArgumentIsNullThenExportRejectsIt(int missing)
    {
        var api = new Api();
        string path = typeof(BitmapRenderTargetTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "AotDllPath").Value!;
        nint module = NativeLibrary.Load(path);
        var create = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateSWRenderTargetForBitmap");
        nint factory = 0, original = 0, bitmap = 0, target = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, 16, 96, 96, 1, &original));
            Assert.AreEqual(0, api.GetBitmap(original, &bitmap));
            Assert.AreEqual(unchecked((int)0x80070057), create(missing == 0 ? 0 : factory, missing == 1 ? 0 : bitmap, missing == 2 ? null : &target));
        }
        finally { Release(target); Release(bitmap); Release(original); Release(factory); }
    }

    [TestMethod]
    public void WhenExistingBitmapBecomesTargetThenClearWritesOriginalAndRetainsOwnership()
    {
        using var wic = new SystemWicBitmap();
        var api = new Api();
        nint factory = 0, original = 0, bitmap = 0, target = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            bitmap = wic.Create(2, 1);
            var create = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)(*(void***)factory)[6];
            Assert.AreEqual(0, create(factory, bitmap, &target));
            Release(original); original = 0;
            float* color = stackalloc float[] { 1, 0, 0, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)target)[4])(target, color, 0));
            Release(target); target = 0;
            uint* pixels = stackalloc uint[2];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)bitmap)[7])(bitmap, 0, 8, 8, pixels));
            CollectionAssert.AreEqual(new uint[] { 0xffff0000, 0xffff0000 }, new uint[] { pixels[0], pixels[1] });
        }
        finally { Release(target); Release(bitmap); Release(original); Release(factory); }
    }

    [TestMethod]
    public void WhenSystemBitmapIsWrappedThenMilLockAndWicViewShareLifetime()
    {
        using var wic = new SystemWicBitmap();
        var api = new Api();
        nint factory = 0, bitmap = 0, target = 0, mil = 0, view = 0, identity = 0, locked = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            bitmap = wic.Create(2, 1);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)(*(void***)factory)[6])(factory, bitmap, &target));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)target)[9])(target, &mil));
            nint repeated = 0;
            try
            {
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)target)[9])(target, &repeated));
                Assert.AreEqual(mil, repeated);
            }
            finally { Release(repeated); }
            Guid wicId = new("00000121-a8f2-4877-ba0a-fd2b6645fb94");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)mil)[0])(mil, &wicId, &view));
            Guid unknownId = new("00000000-0000-0000-c000-000000000046");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)view)[0])(view, &unknownId, &identity));
            Assert.AreEqual(mil, identity);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)mil)[8])(mil, 0, 2, &locked));
            Release(identity); identity = 0;
            Release(mil); mil = 0;
            Release(target); target = 0;
            Release(bitmap); bitmap = 0;
            int format;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int*, int>)(*(void***)locked)[6])(locked, &format));
            Assert.AreEqual(16, format);
            uint size; uint* pixels;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, uint**, int>)(*(void***)locked)[5])(locked, &size, &pixels));
            pixels[0] = 0xffff0000; pixels[1] = 0xff00ff00;
            Release(locked); locked = 0;
            uint* output = stackalloc uint[2];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)view)[7])(view, 0, 8, 8, output));
            CollectionAssert.AreEqual(new uint[] { 0xffff0000, 0xff00ff00 }, new uint[] { output[0], output[1] });
        }
        finally { Release(locked); Release(identity); Release(view); Release(mil); Release(target); Release(bitmap); Release(factory); }
    }

    [DataTestMethod]
    [DataRow(16u, 4)]
    [DataRow(26u, 16)]
    public void WhenTargetIsReleasedThenReturnedBitmapRetainsPixels(uint format, int bytesPerPixel)
    {
        var api = new Api();
        nint factory = 0, target = 0, bitmap = 0, bitmapLock = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 2, format, 120, 144, 1, &target));
            Assert.AreEqual(0, api.GetBitmap(target, &bitmap));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock));
            uint size = 0;
            byte* pixels = null;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels));
            new Span<byte>(pixels, (int)size).Fill(0x7f);
            Release(bitmapLock); bitmapLock = 0;
            Assert.AreEqual(0, api.Clear(target));
            Release(target); target = 0;
            Release(factory); factory = 0;
            byte[] copied = new byte[4 * bytesPerPixel];
            fixed (byte* destination = copied)
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, (uint)(2 * bytesPerPixel), (uint)copied.Length, destination));
            CollectionAssert.AreEqual(new byte[copied.Length], copied);
        }
        finally
        {
            Release(bitmapLock); Release(bitmap); Release(target); Release(factory);
        }
    }

    [DataTestMethod]
    [DataRow(0u, 2u, 16u, 96f, 96f, 1u, unchecked((int)0x80070057))]
    [DataRow(2u, 2u, 16u, 0f, 96f, 1u, unchecked((int)0x80070057))]
    [DataRow(2u, 2u, 16u, 96f, 96f, 3u, unchecked((int)0x80070057))]
    [DataRow(2u, 2u, 14u, 96f, 96f, 1u, unchecked((int)0x88982F80))]
    [DataRow(2u, 2u, 16u, 96f, 96f, 2u, unchecked((int)0x80004001))]
    public void WhenCreationParametersAreInvalidThenNativeErrorIsReturned(uint width, uint height, uint format, float x, float y, uint flags, int expected)
    {
        var api = new Api();
        nint factory = 0, target = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(expected, api.CreateTarget(factory, width, height, format, x, y, flags, &target));
        }
        finally { Release(target); Release(factory); }
    }

    [TestMethod]
    public void WhenBitmapTargetIsQueriedThenMilBitmapHasIntegerPixelFormat()
    {
        var api = new Api();
        nint factory = 0, target = 0, bitmap = 0, identity = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 3, 2, 16, 120, 144, 0, &target));
            Guid id = new("00000020-a8f2-4877-ba0a-fd2b6645fb94");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &identity));
            Assert.AreEqual(target, identity);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)target)[9])(target, &bitmap));
            int format = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int*, int>)(*(void***)bitmap)[4])(bitmap, &format));
            Assert.AreEqual(16, format);
        }
        finally { Release(bitmap); Release(identity); Release(target); Release(factory); }
    }

    [DataTestMethod]
    [DataRow(0f, 2f, 0x80000080u, 0x80000080u)]
    [DataRow(1f, 2f, 0u, 0x80000080u)]
    [DataRow(0.5f, 1.5f, 0x80000080u, 0u)]
    [DataRow(3f, 4f, 0u, 0u)]
    [DataRow(float.NaN, 2f, 0u, 0u)]
    public void WhenColorClearIsClippedThenOnlyCoveredPixelsChange(float left, float right, uint first, uint second)
    {
        var api = new Api();
        nint factory = 0, target = 0, bitmap = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, 16, 96, 96, 1, &target));
            float* color = stackalloc float[] { 0, 0, 1, 0.5f };
            AliasedClip clip = new() { Left = left, Right = right, Bottom = 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, AliasedClip*, int>)(*(void***)target)[4])(target, color, &clip));
            Assert.AreEqual(0, api.GetBitmap(target, &bitmap));
            uint* pixels = stackalloc uint[2];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)bitmap)[7])(bitmap, 0, 8, 8, pixels));
            Assert.AreEqual((first, second), (pixels[0], pixels[1]));
        }
        finally { Release(bitmap); Release(target); Release(factory); }
    }

    [TestMethod]
    public void WhenClearReceivesFactoryThenTargetQueryFailureIsReturned()
    {
        var api = new Api();
        nint factory = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(unchecked((int)0x80004002), api.Clear(factory));
        }
        finally { Release(factory); }
    }

    [DataTestMethod]
    [DataRow(16u)]
    [DataRow(26u)]
    public void WhenClearFailsWhileBitmapIsLockedThenItCanBeRetried(uint format)
    {
        var api = new Api();
        nint factory = 0, target = 0, bitmap = 0, bitmapLock = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 2, format, 96, 96, 1, &target));
            Assert.AreEqual(0, api.GetBitmap(target, &bitmap));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock));
            Assert.AreEqual(unchecked((int)0x88982F0D), api.Clear(target));
            Release(bitmapLock); bitmapLock = 0;
            Assert.AreEqual(0, api.Clear(target));
        }
        finally { Release(bitmapLock); Release(bitmap); Release(target); Release(factory); }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AliasedClip
    {
        internal int IsNull;
        internal float Left, Top, Right, Bottom;
    }

    private static void Release(nint value)
    {
        if (value != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
    }

    private sealed class Api
    {
        internal readonly delegate* unmanaged[Stdcall]<nint*, uint, int> Create;
        internal readonly delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int> CreateTarget;
        internal readonly delegate* unmanaged[Stdcall]<nint, nint*, int> GetBitmap;
        internal readonly delegate* unmanaged[Stdcall]<nint, int> Clear;
        internal Api()
        {
            string path = typeof(BitmapRenderTargetTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "AotDllPath").Value!;
            nint module = NativeLibrary.Load(path);
            Create = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
            CreateTarget = (delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateBitmapRenderTarget");
            GetBitmap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MILRenderTargetBitmapGetBitmap");
            Clear = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MILRenderTargetBitmapClear");
        }
    }
}
