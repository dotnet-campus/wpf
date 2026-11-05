using System.Reflection;
using System.Runtime.InteropServices;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    private struct Variant32
    {
        [FieldOffset(0)] internal ushort Type;
        [FieldOffset(8)] internal long Signed;
        [FieldOffset(8)] internal ulong Unsigned;
        [FieldOffset(8)] internal short Boolean;
        [FieldOffset(8)] internal uint Count;
        [FieldOffset(12)] internal uint ArrayPointer;
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    private struct Variant64
    {
        [FieldOffset(0)] internal ushort Type;
        [FieldOffset(8)] internal long Signed;
        [FieldOffset(8)] internal ulong Unsigned;
        [FieldOffset(8)] internal short Boolean;
        [FieldOffset(8)] internal uint Count;
        [FieldOffset(16)] internal ulong ArrayPointer;
    }

    private static void VerifyPropVariantStorage()
    {
        Assert.AreEqual(16, Marshal.SizeOf<Variant32>());
        Assert.AreEqual(24, Marshal.SizeOf<Variant64>());
        Assert.AreEqual((nint)12, Marshal.OffsetOf<Variant32>(nameof(Variant32.ArrayPointer)));
        Assert.AreEqual((nint)16, Marshal.OffsetOf<Variant64>(nameof(Variant64.ArrayPointer)));
        Type reader = typeof(MediaFoundationSourceReader);
        foreach (string name in new[] { "SourceVariant", "SeekPosition" })
        {
            Type? storage = reader.GetNestedType(name, BindingFlags.NonPublic);
            Assert.IsNotNull(storage);
            int size = Marshal.SizeOf(storage);
            Assert.IsTrue(size >= (IntPtr.Size == 4 ? 16 : 24));
            Assert.AreEqual((nint)0, Marshal.OffsetOf(storage, "Type"));
            Assert.AreEqual((nint)8, Marshal.OffsetOf(storage, name == "SourceVariant" ? "Unsigned" : "Value"));
            if (name == "SourceVariant") Assert.AreEqual((nint)8, Marshal.OffsetOf(storage, "Pointer"));
        }
    }

    /// <summary>Checks scalar union bytes and both SDK architecture contracts without invoking MF.</summary>
    [TestMethod]
    public unsafe void WhenPropVariantScalarsAreStoredThenSdkOffsetsAndRangesArePreserved()
    {
        VerifyPropVariantStorage();
        Variant32 small = new() { Type = 20, Signed = long.MinValue };
        Variant64 large = new() { Type = 21, Unsigned = ulong.MaxValue };
        Assert.AreEqual((ushort)20, *(ushort*)&small);
        Assert.AreEqual(long.MinValue, *(long*)((byte*)&small + 8));
        Assert.AreEqual((ushort)21, *(ushort*)&large);
        Assert.AreEqual(ulong.MaxValue, *(ulong*)((byte*)&large + 8));
        small = new() { Type = 11, Boolean = -1 };
        Assert.AreEqual((short)-1, *(short*)((byte*)&small + 8));
        large = default;
        Assert.AreEqual((ushort)0, large.Type);
        Assert.AreEqual(0UL, large.Unsigned);
        Type? audio = typeof(MediaFoundationSourceReader).Assembly.GetType("WpfGfxShape.Core.Av.MediaFoundationAudioReader");
        if (audio is not null)
        {
            Type? variant = audio.GetNestedType("Variant", BindingFlags.NonPublic);
            Assert.IsNotNull(variant);
            Assert.AreEqual(IntPtr.Size == 4 ? 16 : 24, Marshal.SizeOf(variant));
            Assert.AreEqual((nint)8, Marshal.OffsetOf(variant, "Value"));
            Type? array = audio.GetNestedType("VariantArray", BindingFlags.NonPublic);
            Assert.IsNotNull(array);
            Assert.AreEqual((nint)(IntPtr.Size == 4 ? 4 : 8), Marshal.OffsetOf(array, "Pointer"));
        }
    }
}
