using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed unsafe class RealizerMemberRecoveryRegressionTests
{
    public TestContext TestContext { get; set; } = null!;

    private struct Realizer
    {
        internal void** Table;
        internal nint Context;
        internal int Ensures, Brushes, Effects, Failure;
        internal uint Adapter, Display;
        internal byte Transparent;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static int Ensure(Realizer* self, uint adapter, uint display, nint brushContext, nint context, nint creator)
    {
        self->Ensures++;
        self->Context = context;
        self->Adapter = adapter;
        self->Display = display;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static nint Brush(Realizer* self, byte transparent)
    {
        self->Brushes++;
        self->Transparent = transparent;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static nint BrushCore(Realizer* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static int Effects(Realizer* self, nint* effects)
    {
        self->Effects++;
        *effects = 0;
        return self->Failure;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static byte NoTiling(nint self, nint context) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static byte NoClip(nint self) => 0;

    /// <summary>Checks independent C++ member callbacks and target readback after effects failure and recovery.</summary>
    [STATestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenEffectsMemberFailsThenRepeatedRecoveryPreservesTarget(bool candidate)
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
            table[9] = (delegate* unmanaged[Thiscall]<Realizer*, nint>)&BrushCore;
            table[10] = (delegate* unmanaged[Thiscall]<Realizer*, byte, nint>)&Brush;
            table[11] = (delegate* unmanaged[Thiscall]<Realizer*, nint*, int>)&Effects;
            Realizer realizer = new() { Table = table, Failure = unchecked((int)0x80070005), Transparent = 255 };
            int[] results = new int[16];
            var draw = (delegate* unmanaged[Stdcall]<nint, nint, nint, nint, int>)(*(void***)view)[11];
            for (int i = 0; i < 8; i++) results[i] = draw(view, (nint)context, 0, (nint)(&realizer));
            int failureEnsures = realizer.Ensures, failureBrushes = realizer.Brushes, failureEffects = realizer.Effects;
            realizer.Failure = 0;
            for (int i = 8; i < 16; i++) results[i] = draw(view, (nint)context, 0, (nint)(&realizer));
            int bitmapResult = getBitmap(target, &bitmap);
            byte[] pixels = new byte[8];
            int copyResult = -1;
            if (bitmapResult == 0)
            {
                fixed (byte* output = pixels)
                    copyResult = ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 8, 8, output);
            }
            var version = FileVersionInfo.GetVersionInfo(path);
            string report = Path.Combine(AcceptancePaths.Results, $"realizer-member-recovery-{candidate}-{Guid.NewGuid():N}.json");
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                Candidate = candidate, Module = path, Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                version.FileVersion, version.ProductVersion, Runtime = RuntimeInformation.FrameworkDescription,
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(), PointerSize = IntPtr.Size,
                Results = results, failureEnsures, failureBrushes, failureEffects,
                realizer.Ensures, realizer.Brushes, realizer.Effects, realizer.Adapter, realizer.Display, realizer.Transparent,
                ContextMatches = realizer.Context == (nint)context, bitmapResult, copyResult, PixelsBgra = pixels,
                Scope = "Independent thiscall slots 6/10/11, null brush, effects failure and recovery; no successful non-null brush or ESP claim"
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddResultFile(report);
            CollectionAssert.AreEqual(Enumerable.Repeat(unchecked((int)0x80070005), 8).Concat(Enumerable.Repeat(0, 8)).ToArray(), results);
            Assert.AreEqual(8, failureEnsures);
            Assert.AreEqual(8, failureEffects);
            Assert.AreEqual(16, realizer.Ensures);
            Assert.AreEqual(16, realizer.Effects);
            Assert.AreEqual(8, failureBrushes);
            Assert.AreEqual(16, realizer.Brushes);
            Assert.AreEqual((byte)0, realizer.Transparent);
            Assert.AreEqual((nint)context, realizer.Context);
            Assert.AreEqual(0, bitmapResult);
            Assert.AreEqual(0, copyResult);
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
