using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed unsafe class IndependentRealizerAbiTests
{
    public TestContext TestContext { get; set; } = null!;

    [StructLayout(LayoutKind.Sequential)]
    private struct Realizer
    {
        internal void** Table;
        internal nint Context;
        internal uint Adapter, Display;
        internal int Calls;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static int Ensure(Realizer* self, uint adapter, uint display, nint brushContext, nint context, nint creator)
    {
        self->Calls++;
        self->Context = context;
        self->Adapter = adapter;
        self->Display = display;
        return unchecked((int)0x80070005);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static byte NoTiling(nint self, nint context) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static byte NoClip(nint self) => 0;

    /// <summary>Calls the C++ realizer member ABI independently of candidate declarations and preserves raw observations.</summary>
    [STATestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenRealizerMemberFailsRepeatedlyThenContextAndTargetRemainIntact(bool candidate)
    {
        string path = candidate ? AcceptancePaths.AotDll : AcceptancePaths.OriginalDll;
        nint module = NativeLibrary.Load(path);
        nint factory = 0, target = 0, view = 0, bitmap = 0;
        try
        {
            var create = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
            var createTarget = (delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateBitmapRenderTarget");
            var getBitmap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MILRenderTargetBitmapGetBitmap");
            Assert.AreEqual(0, create(&factory, 0x200184C0));
            Assert.AreEqual(0, createTarget(factory, 2, 1, 16, 96, 96, 1, &target));
            Guid iid = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &iid, &view));
            float* color = stackalloc float[] { 0, 1, 0, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)target)[4])(target, color, 0));
            byte* context = stackalloc byte[2048];
            byte* state = stackalloc byte[116];
            new Span<byte>(context, 2048).Clear();
            new Span<byte>(state, 116).Clear();
            *(int*)(context + InternalContextLayout.Clip) = 1;
            *(nint*)(context + InternalContextLayout.RenderState) = (nint)state;
            *(Matrix4x4*)(context + InternalContextLayout.WorldToDevice) = Matrix4x4.Identity;
            void** table = stackalloc void*[12];
            new Span<nint>(table, 12).Clear();
            table[3] = (delegate* unmanaged[Thiscall]<nint, nint, byte>)&NoTiling;
            table[4] = (delegate* unmanaged[Thiscall]<nint, byte>)&NoClip;
            table[5] = (delegate* unmanaged[Thiscall]<nint, nint, byte>)&NoTiling;
            table[6] = (delegate* unmanaged[Thiscall]<Realizer*, uint, uint, nint, nint, nint, int>)&Ensure;
            Realizer realizer = new() { Table = table };
            int[] results = new int[16];
            for (int i = 0; i < results.Length; i++)
                results[i] = ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, int>)(*(void***)view)[11])(view, (nint)context, 0, (nint)(&realizer));
            string callbackReport = Path.Combine(AcceptancePaths.Results, $"independent-realizer-callbacks-{candidate}-{Guid.NewGuid():N}.json");
            File.WriteAllText(callbackReport, JsonSerializer.Serialize(new
            {
                Candidate = candidate, Module = path,
                Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(), PointerSize = IntPtr.Size,
                Results = results, realizer.Calls, realizer.Adapter, realizer.Display,
                ContextMatches = realizer.Context == (nint)context
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddResultFile(callbackReport);
            Assert.AreEqual(0, getBitmap(target, &bitmap));
            byte[] pixels = new byte[8];
            fixed (byte* output = pixels)
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 8, 8, output));
            var version = FileVersionInfo.GetVersionInfo(path);
            string report = Path.Combine(AcceptancePaths.Results, $"independent-realizer-{candidate}-{Guid.NewGuid():N}.json");
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                Candidate = candidate, Module = path,
                Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                version.FileVersion, version.ProductVersion,
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(), PointerSize = IntPtr.Size,
                Runtime = RuntimeInformation.FrameworkDescription,
                Results = results, realizer.Calls, realizer.Adapter, realizer.Display,
                ContextMatches = realizer.Context == (nint)context, PixelsBgra = pixels,
                Scope = "CBrushRealizer::EnsureRealization failure and repeated subsequent readback; not successful bitmap realization or register/ESP observation"
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddResultFile(report);
            CollectionAssert.AreEqual(Enumerable.Repeat(unchecked((int)0x80070005), 16).ToArray(), results);
            Assert.AreEqual(16, realizer.Calls);
            Assert.AreEqual((nint)context, realizer.Context);
            CollectionAssert.AreEqual(new byte[] { 0, 255, 0, 255, 0, 255, 0, 255 }, pixels);
        }
        finally
        {
            Release(bitmap); Release(view); Release(target); Release(factory);
            NativeLibrary.Free(module);
        }
    }

    private static void Release(nint value)
    {
        if (value != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
    }
}
