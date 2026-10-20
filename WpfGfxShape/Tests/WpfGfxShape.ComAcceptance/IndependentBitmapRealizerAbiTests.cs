using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed unsafe class IndependentBitmapRealizerAbiTests
{
    public TestContext TestContext { get; set; } = null!;

    [ThreadStatic]
    private static string? stagePath;

    private static void Stage(string value)
    {
        if (stagePath is not null) File.AppendAllText(stagePath, value + Environment.NewLine);
    }

    // Mirrors retail native bases, including the secondary CMILBrush subobject.
    [StructLayout(LayoutKind.Sequential)]
    private struct MilObject { internal nint Table; internal int References; internal nint Factory; }

    [StructLayout(LayoutKind.Sequential)]
    private struct ResourceCache
    {
        internal nint Table, Data, InitialAllocation;
        internal uint AllocSize, Capacity, Count;
        internal nint Initial0, Initial1;
        internal int InCall;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ParallelogramFigure
    {
        internal nint Table;
        internal Vector2 P0, P1, P2, P3;
        internal uint CurrentSegment;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Parallelogram { internal nint Table; internal ParallelogramFigure Figure; }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapBrush
    {
        internal MilObject Object;
        internal nint BrushTable;
        internal ResourceCache Cache;
        internal uint Uniqueness, SamplingSpace;
        internal Matrix4x4 Matrix;
        internal Parallelogram Clip;
        internal int UseClip, EntireSource;
        internal nint Texture;
        internal uint Wrap;
        internal Vector4 Border;
        internal float Opacity;
    }

    private struct Realizer
    {
        internal void** Table;
        internal nint Brush, Context;
        internal int Ensured, Brushes, Effects;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static int Ensure(Realizer* self, uint cache, uint display, nint brushContext, nint context, nint creator)
    {
        Stage("callback.Ensure");
        self->Ensured++;
        self->Context = context;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static nint Brush(Realizer* self, byte transparent) { Stage("callback.Brush"); self->Brushes++; return self->Brush; }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static int Effects(Realizer* self, nint* effects) { Stage("callback.Effects"); self->Effects++; *effects = 0; return 0; }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static uint Type(nint self) { Stage("callback.Type"); return 4; }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static byte No(nint self, nint context) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static byte NoClip(nint self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static void Cleanup(nint self) { }

    /// <summary>Checks successful bitmap realization through independent member declarations and exact nonempty content.</summary>
    [STATestMethod]
    [DataRow(false, false)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(true, true)]
    public void WhenBitmapRealizerSucceedsThenRepeatedMemberCallsProduceRed(bool candidate, bool translated)
    {
        string path = candidate ? AcceptancePaths.AotDll : AcceptancePaths.OriginalDll;
        stagePath = Path.Combine(AcceptancePaths.Results, $"bitmap-stages-{candidate}-{Guid.NewGuid():N}.log");
        TestContext.AddResultFile(stagePath);
        Stage(JsonSerializer.Serialize(new { candidate, path, Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), FileVersionInfo.GetVersionInfo(path).FileVersion, Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString() }));
        Stage("Load.before");
        nint module = NativeLibrary.Load(path);
        Stage("Load.after");
        nint factory = 0, sourceTarget = 0, source = 0, milSource = 0, target = 0, view = 0, bitmap = 0;
        try
        {
            var create = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
            var createTarget = (delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateBitmapRenderTarget");
            var getBitmap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MILRenderTargetBitmapGetBitmap");
            Stage("CreateFactory.before");
            Assert.AreEqual(0, create(&factory, 0x200184C0));
            Stage("CreateFactory.after");
            Stage("CreateSourceTarget.before");
            Assert.AreEqual(0, createTarget(factory, 1, 1, 16, 96, 96, 1, &sourceTarget));
            Stage("CreateSourceTarget.after");
            float* red = stackalloc float[] { 1, 0, 0, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)sourceTarget)[4])(sourceTarget, red, 0));
            Stage("ClearSource.after");
            Assert.AreEqual(0, getBitmap(sourceTarget, &source));
            Stage("GetSourceBitmap.after");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)sourceTarget)[7])(sourceTarget, &milSource));
            Stage("GetMilSource.after");
            Assert.AreEqual(0, createTarget(factory, 2, 1, 16, 96, 96, 1, &target));
            Stage("CreateTarget.after");
            Guid targetId = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &targetId, &view));
            Stage("QueryView.after");
            byte* context = stackalloc byte[2048];
            byte* state = stackalloc byte[116];
            new Span<byte>(context, 2048).Clear(); new Span<byte>(state, 116).Clear();
            int renderState = IntPtr.Size == 4 ? 384 : 400;
            *(int*)(context + 352 + 3 * IntPtr.Size) = 1;
            *(nint*)(context + renderState) = (nint)state;
            *(Matrix4x4*)(context + renderState + IntPtr.Size) = Matrix4x4.Identity;
            // Older candidates read an x64 prefix even on x86; keep that read inside initialized storage.
            byte* storage = stackalloc byte[384];
            new Span<byte>(storage, 384).Clear();
            BitmapBrush* nativeBrush = (BitmapBrush*)storage;
            int brushOffset = (int)Marshal.OffsetOf<BitmapBrush>(nameof(BitmapBrush.BrushTable));
            byte* brush = storage + brushOffset;
            void** brushTable = stackalloc void*[6];
            new Span<nint>(brushTable, 6).Clear();
            brushTable[3] = (delegate* unmanaged[Thiscall]<nint, uint>)&Type;
            *(void***)brush = brushTable;
            int matrix = (int)Marshal.OffsetOf<BitmapBrush>(nameof(BitmapBrush.Matrix)) - brushOffset;
            int texture = (int)Marshal.OffsetOf<BitmapBrush>(nameof(BitmapBrush.Texture)) - brushOffset;
            Stage(JsonSerializer.Serialize(new { brushOffset, matrix, texture, Size = sizeof(BitmapBrush) }));
            *(Matrix4x4*)(brush + matrix) = Matrix4x4.Identity;
            *(nint*)(brush + texture) = milSource;
            nativeBrush->Wrap = translated ? 5u : 0u;
            nativeBrush->Opacity = 1;
            if (translated) nativeBrush->Matrix.M41 = 1;
            void** table = stackalloc void*[12];
            new Span<nint>(table, 12).Clear();
            table[3] = (delegate* unmanaged[Thiscall]<nint, nint, byte>)&No;
            table[4] = (delegate* unmanaged[Thiscall]<nint, byte>)&NoClip;
            table[5] = (delegate* unmanaged[Thiscall]<nint, nint, byte>)&No;
            table[6] = (delegate* unmanaged[Thiscall]<Realizer*, uint, uint, nint, nint, nint, int>)&Ensure;
            table[7] = (delegate* unmanaged[Thiscall]<nint, void>)&Cleanup;
            table[8] = (delegate* unmanaged[Thiscall]<nint, void>)&Cleanup;
            table[10] = (delegate* unmanaged[Thiscall]<Realizer*, byte, nint>)&Brush;
            table[11] = (delegate* unmanaged[Thiscall]<Realizer*, nint*, int>)&Effects;
            Realizer realizer = new() { Table = table, Brush = (nint)brush };
            int[] results = new int[16];
            try
            {
                for (int i = 0; i < results.Length; i++)
                {
                    Stage($"DrawInfinitePath.{i}.before");
                    results[i] = ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, int>)(*(void***)view)[11])(view, (nint)context, 0, (nint)(&realizer));
                    Stage($"DrawInfinitePath.{i}.after={results[i]}");
                }
            }
            catch (NullReferenceException)
            {
                string failure = Path.Combine(AcceptancePaths.Results, $"bitmap-call-failure-{candidate}-{Guid.NewGuid():N}.json");
                File.WriteAllText(failure, JsonSerializer.Serialize(new { candidate, realizer.Ensured, realizer.Brushes, realizer.Effects, matrix, texture }));
                TestContext.AddResultFile(failure);
                throw;
            }
            Stage("GetTargetBitmap.before");
            Assert.AreEqual(0, getBitmap(target, &bitmap));
            Stage("GetTargetBitmap.after");
            byte[] pixels = new byte[8];
            fixed (byte* output = pixels)
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 8, 8, output));
            string report = Path.Combine(AcceptancePaths.Results, $"independent-bitmap-realizer-{candidate}-{Guid.NewGuid():N}.json");
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                Candidate = candidate, Translated = translated, Module = path, Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                FileVersionInfo.GetVersionInfo(path).FileVersion, FileVersionInfo.GetVersionInfo(path).ProductVersion, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                Runtime = RuntimeInformation.FrameworkDescription, PointerSize = IntPtr.Size, MatrixOffset = matrix, TextureOffset = texture,
                Results = results, realizer.Ensured, realizer.Brushes, realizer.Effects, ContextMatches = realizer.Context == (nint)context,
                PixelsBgra = pixels.Select(value => (int)value).ToArray()
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddResultFile(report);
            CollectionAssert.AreEqual(new int[16], results);
            Assert.AreEqual(16, realizer.Ensured);
            Assert.AreEqual(16, realizer.Brushes);
            Assert.AreEqual(16, realizer.Effects);
            Assert.AreEqual((nint)context, realizer.Context);
            CollectionAssert.AreEqual(translated
                ? new byte[] { 0, 0, 0, 0, 0, 0, 255, 255 }
                : new byte[] { 0, 0, 255, 255, 0, 0, 255, 255 }, pixels);
        }
        finally
        {
            Stage("Release.before");
            Release(bitmap); Release(view); Release(target); Release(milSource); Release(source); Release(sourceTarget); Release(factory);
            Stage("Release.after");
            stagePath = null;
        }
    }

    private static void Release(nint value)
    {
        if (value != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
    }
}
