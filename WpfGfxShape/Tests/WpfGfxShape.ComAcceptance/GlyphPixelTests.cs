using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class GlyphPixelTests
{
    [DllImport("dwrite.dll", ExactSpelling = true)]
    private static extern int DWriteCreateFactory(uint type, in Guid iid, out nint factory);

    [TestMethod]
    public void WhenGlyphRunDrawingIsPresentedThenWholeSurfaceMatchesIndependentOriginal()
        => Run(false, false, false);

    [TestMethod]
    public void WhenDirectDrawGlyphRunIsPresentedThenWholeSurfaceMatchesIndependentOriginal()
        => Run(true, false, false);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenGlyphTargetLockFailsThenRetryMatchesIndependentOriginal(bool direct)
        => Run(direct, true, false);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenGlyphVisualIsTranslatedThenWholeSurfaceMatchesTranslatedOriginal(bool direct)
        => Run(direct, false, true);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenGlyphClipIsScopedThenWholeSurfaceMatchesClippedOriginal(bool direct)
        => Run(direct, false, false, clipped: true);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenGlyphBrushIsBlueThenWholeSurfaceMatchesOriginalChannelPermutation(bool direct)
        => Run(direct, false, false, blue: true);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenGlyphFontFaceIsReplacedThenPresentedPixelsRemainOriginal(bool direct)
        => Run(direct, false, false, replaced: true);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenGlyphOpacityLayerIsScopedThenWholeSurfaceMatchesHalfOriginal(bool direct)
        => Run(direct, false, false, layer: true);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenGlyphBrushContainsFloatHdrThenDirectWriteCoveragePreservesUnclampedChannels(bool direct)
        => Run(direct, false, false, floating: true);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenGlyphsOverlapThenIntegerSourceOverMatchesNativeContract(bool direct)
        => Run(direct, false, false, overlap: true);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenGlyphHandleIsReleasedThenDrawingReferencesPreserveOriginalPixels(bool direct)
        => Run(direct, false, false, releaseGlyph: true);

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(true, true)]
    public void WhenGlyphOwnerClosesThenCallerBitmapPreservesOriginalPixels(bool direct, bool disconnectOwner)
        => Run(direct, false, false, closeOwner: true, disconnectOwner: disconnectOwner);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenGlyphCommandCreatesHandleThenWholeSurfaceMatchesIndependentOriginal(bool direct)
        => Run(direct, false, false, automaticHandle: true);

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    public void WhenPublicGlyphOutlineIsConsumedThenSoftwarePixelsMatchSystemGeometry(bool retry, bool closeOwner)
    {
        byte[] expected = Run(false, retry, false, closeOwner: closeOwner, outline: true, original: true);
        byte[] actual = Run(false, retry, false, closeOwner: closeOwner, outline: true);
        Directory.CreateDirectory("Results");
        File.WriteAllBytes(Path.Combine("Results", $"outline-original-{retry}-{closeOwner}.pixels"), expected);
        File.WriteAllBytes(Path.Combine("Results", $"outline-candidate-{retry}-{closeOwner}.pixels"), actual);
        File.WriteAllLines(Path.Combine("Results", $"outline-differences-{retry}-{closeOwner}.txt"),
            Enumerable.Range(0, expected.Length / 4)
                .Where(i => !expected.AsSpan(i * 4, 4).SequenceEqual(actual.AsSpan(i * 4, 4)))
                .Select(i => $"x={i % 32} y={i / 32} original={Convert.ToHexString(expected.AsSpan(i * 4, 4))} candidate={Convert.ToHexString(actual.AsSpan(i * 4, 4))}"));
        Assert.IsTrue(expected.Any(value => value != 0));
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenOutlineWireColorIsRedOrBlueThenWholeSurfaceMatchesOriginal(bool blue)
    {
        byte[] expected = Run(false, false, false, blue: blue, outline: true, original: true, coloredOutline: true);
        byte[] actual = Run(false, false, false, blue: blue, outline: true, coloredOutline: true);
        Assert.IsTrue(expected.Any(value => value != 0));
        Assert.IsTrue(Enumerable.Range(0, expected.Length / 4).Any(i => expected[i * 4 + (blue ? 0 : 2)] != 0));
        Assert.IsFalse(Enumerable.Range(0, expected.Length / 4).Any(i => expected[i * 4 + (blue ? 2 : 0)] != 0));
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(0, true)]
    [DataRow(1, true)]
    [DataRow(2, true)]
    [DataRow(3, true)]
    [DataRow(0, false)]
    [DataRow(1, false)]
    [DataRow(2, false)]
    [DataRow(3, false)]
    public void WhenMinimalBatchIsSubmittedThenPublicCommandsComplete(int stage, bool original)
    {
        nint module = NativeLibrary.Load(original ? Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll") : AcceptancePaths.AotDll);
        nint connection = 0, channel = 0;
        var create = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        try
        {
            Assert.AreEqual(0, create(1, &connection));
            Assert.AreEqual(0, channelCreate(connection, 0, &channel));
            uint handle = 0;
            Assert.AreEqual(0, resource(channel, stage == 0 ? 73u : stage == 1 ? 75u : stage == 2 ? 91u : 43u, &handle));
            byte* packet = stackalloc byte[68]; new Span<byte>(packet, 68).Clear();
            *(uint*)packet = stage == 0 ? 0x7du : stage == 1 ? 0x7eu : stage == 2 ? 0x8bu : 0x18u;
            *(uint*)(packet + 4) = handle;
            uint size = stage == 0 ? 68u : stage == 1 ? 48u : stage == 2 ? 52u : 36u;
            if (stage == 0) { *(uint*)(packet + 16) = 48; *(uint*)(packet + 20) = 48; }
            if (stage is 1 or 2) *(double*)(packet + 8) = 1;
            if (stage == 3)
            {
                *(uint*)(packet + 8) = 24; *(uint*)(packet + 12) = 24; *(uint*)(packet + 16) = 0x46;
            }
            Assert.AreEqual(0, send(packet, size, 0, channel));
            if (stage == 2)
            {
                uint data = 0, visual = 0;
                Assert.AreEqual(0, resource(channel, 43, &data));
                Assert.AreEqual(0, resource(channel, 39, &visual));
                uint* drawing = stackalloc uint[] { 0x18, data, 16, 16, 0x4a, handle, 0 };
                Assert.AreEqual(0, send(drawing, 28, 0, channel));
                uint* binding = stackalloc uint[] { 0x22, visual, data };
                Assert.AreEqual(0, send(binding, 12, 0, channel));
            }
            Directory.CreateDirectory("Results");
            File.AppendAllText("Results/glyph-minimal.txt", $"original={original} stage={stage} close{Environment.NewLine}");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch"))(channel));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel"))(channel));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent"))(connection));
            File.AppendAllText("Results/glyph-minimal.txt", $"original={original} stage={stage} completed{Environment.NewLine}");
        }
        finally
        {
            if (channel != 0) ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel"))(channel);
            if (connection != 0) ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect"))(connection);
        }
    }

    private static byte[] Run(bool direct, bool retry, bool translated, bool clipped = false,
        bool blue = false, bool replaced = false, bool layer = false, bool floating = false,
        bool overlap = false, bool releaseGlyph = false, bool closeOwner = false,
        bool disconnectOwner = false, bool automaticHandle = false, bool outline = false, bool original = false, bool coloredOutline = false)
    {
        using var reference = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ReferenceData", "glyph-arial-original.json")));
        var contract = reference.RootElement;
        byte[] expected = contract.GetProperty("PixelsBgra").EnumerateArray().Select(v => v.GetByte()).ToArray();
        if (translated)
        {
            byte[] shifted = new byte[expected.Length];
            for (int row = 0; row < 23; row++)
                Array.Copy(expected, row * 128, shifted, (row + 1) * 128 + 4, 124);
            expected = shifted;
        }
        if (overlap)
            for (int offset = 0; offset < expected.Length; offset += 4)
            {
                int alpha = expected[offset + 3];
                if (alpha == 0 || alpha == 255) continue;
                for (int component = 0; component < 4; component++)
                    expected[offset + component] += (byte)((expected[offset + component] * (255 - alpha)) >> 8);
            }
        for (int pixel = 0; pixel < expected.Length / 4; pixel++)
        {
            int offset = pixel * 4;
            if (clipped && pixel % 32 >= 8) Array.Clear(expected, offset, 4);
            if (blue) (expected[offset], expected[offset + 2]) = (expected[offset + 2], expected[offset]);
            if (layer)
                for (int component = 0; component < 4; component++)
                    expected[offset + component] = (byte)((expected[offset + component] * 32768 + 32768) >> 16);
        }
        string path = original
            ? Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll")
            : AcceptancePaths.AotDll;
        byte[] outlineData = [];
        uint outlineFill = 0;
        void Stage(string stage)
        {
            if (!outline) return;
            Directory.CreateDirectory("Results");
            File.AppendAllText(Path.Combine("Results", "glyph-outline-stages.txt"), $"{(original ? "original" : "candidate")} retry={retry} close={closeOwner}: {stage}{Environment.NewLine}");
        }
        Stage("load");
        nint module = NativeLibrary.Load(path);
        var create = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var resourceCreate = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
        nint factory = 0, collection = 0, family = 0, font = 0, connection = 0, channel = 0, milFactory = 0, target = 0, bitmap = 0;
        using var wic = new SystemWicBitmap();
        try
        {
            Assert.AreEqual(0, DWriteCreateFactory(0, new Guid("b859ee5a-d838-4b5b-a2e8-1adc7d93db48"), out factory));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int, int>)(*(void***)factory)[3])(factory, &collection, 0));
            uint familyIndex = 0; int exists = 0;
            fixed (char* name = "Arial") Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, char*, uint*, int*, int>)(*(void***)collection)[5])(collection, name, &familyIndex, &exists));
            Assert.AreEqual(1, exists);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)collection)[4])(collection, familyIndex, &family));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, nint*, int>)(*(void***)family)[7])(family, 400, 5, 0, &font));
            Assert.AreEqual(contract.GetProperty("Weight").GetUInt32(), ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)font)[4])(font));
            Assert.AreEqual(contract.GetProperty("Stretch").GetUInt32(), ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)font)[5])(font));
            Assert.AreEqual(0u, ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)font)[6])(font));
            nint face = 0;
            try
            {
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)font)[13])(font, &face));
                Assert.AreEqual(contract.GetProperty("FaceIndex").GetUInt32(), ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)face)[5])(face));
                uint character = 'A'; ushort index = 0;
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, uint, ushort*, int>)(*(void***)face)[11])(face, &character, 1, &index));
                Assert.AreEqual(contract.GetProperty("Index").GetUInt16(), index);
                if (outline)
                {
                    byte* geometry = null; uint geometrySize = 0;
                    try
                    {
                        // The outline export consumes one font-face reference, including on failure.
                        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)face)[1])(face);
                        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, ushort, byte, double, byte**, uint*, uint*, int>)NativeLibrary.GetExport(module, "MilGlyphRun_GetGlyphOutline"))(face, index, 0, 16, &geometry, &geometrySize, &outlineFill));
                        Stage($"outline returned size={geometrySize} fill={outlineFill}");
                        outlineData = new ReadOnlySpan<byte>(geometry, checked((int)geometrySize)).ToArray();
                        Stage($"outline copied header={Convert.ToHexString(outlineData.AsSpan(0, 48))} start={BitConverter.ToDouble(outlineData, 64)},{BitConverter.ToDouble(outlineData, 72)}");
                        Directory.CreateDirectory("Results");
                        File.WriteAllBytes(Path.Combine("Results", original ? "outline-original.bin" : "outline-candidate.bin"), outlineData);
                    }
                    finally
                    {
                        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<byte*, int>)NativeLibrary.GetExport(module, "MilGlyphRun_ReleasePathGeometryData"))(geometry));
                    }
                }
                byte* metrics = stackalloc byte[20];
                ((delegate* unmanaged[Stdcall]<nint, void*, void>)(*(void***)face)[8])(face, metrics);
                byte* glyphMetrics = stackalloc byte[28];
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, ushort*, uint, void*, int, int>)(*(void***)face)[10])(face, &index, 1, glyphMetrics, 0));
                Assert.AreEqual(contract.GetProperty("Advance").GetSingle(), *(uint*)(glyphMetrics + 4) * 16f / *(ushort*)metrics);
            }
            finally { Release(face); }
            Assert.AreEqual(0, create(1, &connection));
            Assert.AreEqual(0, channelCreate(connection, 0, &channel));
            var createFactory = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
            var createTarget = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateSWRenderTargetForBitmap");
            Stage("create factory");
            Assert.AreEqual(0, createFactory(&milFactory, 0x200184C0));
            bitmap = wic.Create(32, 24, floating);
            Stage("create software target");
            Assert.AreEqual(0, createTarget(milFactory, bitmap, &target));
            Stage("software target created");
            uint glyph = 0, brush = 0, drawing = 0, data = 0, visual = 0, destination = 0;
            if (automaticHandle) glyph = 9000;
            else Assert.AreEqual(0, resourceCreate(channel, 42, &glyph));
            Assert.AreEqual(0, resourceCreate(channel, 75, &brush));
            Assert.AreEqual(0, resourceCreate(channel, 88, &drawing));
            Assert.AreEqual(0, resourceCreate(channel, 43, &data));
            Assert.AreEqual(0, resourceCreate(channel, 39, &visual));
            Assert.AreEqual(0, resourceCreate(channel, 47, &destination));
            byte* packet = stackalloc byte[82]; new Span<byte>(packet, 82).Clear();
            *(uint*)packet = 0x3a; *(uint*)(packet + 4) = glyph; *(ulong*)(packet + 8) = (ulong)font;
            *(float*)(packet + 20) = 2; *(float*)(packet + 24) = 20; *(float*)(packet + 28) = 16;
            double[] bounds = contract.GetProperty("Bounds").EnumerateArray().Select(v => v.GetDouble()).ToArray();
            for (int i = 0; i < 4; i++) *(double*)(packet + 32 + i * 8) = bounds[i];
            *(ushort*)(packet + 64) = 1; *(ushort*)(packet + 76) = contract.GetProperty("Index").GetUInt16();
            *(float*)(packet + 78) = contract.GetProperty("Advance").GetSingle();
            if (!outline)
            {
                ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)font)[1])(font);
                Assert.AreEqual(0, send(packet, 82, 0, channel));
            }
            if (replaced)
            {
                ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)font)[1])(font);
                Assert.AreEqual(0, send(packet, 82, 0, channel));
            }
            byte* solid = stackalloc byte[48]; new Span<byte>(solid, 48).Clear();
            *(uint*)solid = 0x7e; *(uint*)(solid + 4) = brush; *(double*)(solid + 8) = 1;
            *(float*)(solid + 28) = 1; *(float*)(solid + 16) = blue ? 0 : 1;
            *(float*)(solid + 24) = blue ? 1 : 0;
            if (floating)
            {
                *(float*)(solid + 16) = 4; *(float*)(solid + 20) = -2; *(float*)(solid + 24) = 1e10f;
            }
            if (outline && !coloredOutline)
            {
                *(float*)(solid + 16) = 1; *(float*)(solid + 20) = 1;
                *(float*)(solid + 24) = 1; *(float*)(solid + 28) = 1;
            }
            Assert.AreEqual(0, send(solid, 48, 0, channel));
            uint* drawingPacket = stackalloc uint[] { 0x88, drawing, glyph, brush };
            if (!outline) Assert.AreEqual(0, send(drawingPacket, 16, 0, channel));
            uint* render = stackalloc uint[] { 0x18, data, 16, 16, direct ? 0x49u : 0x4au,
                direct ? brush : drawing, direct ? glyph : 0u };
            if (clipped || layer)
            {
                uint clip = 0;
                Assert.AreEqual(0, resourceCreate(channel, 69, &clip));
                byte* rectangle = stackalloc byte[72]; new Span<byte>(rectangle, 72).Clear();
                *(uint*)rectangle = 0x79; *(uint*)(rectangle + 4) = clip;
                *(double*)(rectangle + 40) = 8; *(double*)(rectangle + 48) = 24;
                Assert.AreEqual(0, send(rectangle, 72, 0, channel));
                byte* scoped = stackalloc byte[52]; new Span<byte>(scoped, 52).Clear();
                *(uint*)scoped = 0x18; *(uint*)(scoped + 4) = data; *(uint*)(scoped + 8) = 40;
                *(uint*)(scoped + 12) = 16; *(uint*)(scoped + 16) = clipped ? 0x4du : 0x4fu;
                if (clipped) *(uint*)(scoped + 20) = clip;
                else *(double*)(scoped + 20) = .5;
                new ReadOnlySpan<byte>(render + 3, 16).CopyTo(new Span<byte>(scoped + 28, 16));
                *(uint*)(scoped + 44) = 8; *(uint*)(scoped + 48) = 0x56;
                Assert.AreEqual(0, send(scoped, 52, 0, channel));
            }
            else if (overlap)
            {
                uint* twice = stackalloc uint[11]; new Span<uint>(twice, 11).Clear();
                twice[0] = 0x18; twice[1] = data; twice[2] = 32;
                new ReadOnlySpan<uint>(render + 3, 4).CopyTo(new Span<uint>(twice + 3, 4));
                new ReadOnlySpan<uint>(render + 3, 4).CopyTo(new Span<uint>(twice + 7, 4));
                Assert.AreEqual(0, send(twice, 44, 0, channel));
            }
            else if (outline)
            {
                uint geometryHandle = 0;
                Assert.AreEqual(0, resourceCreate(channel, 73, &geometryHandle));
                byte[] command = new byte[20 + outlineData.Length];
                fixed (byte* p = command)
                {
                    *(uint*)p = 0x7d; *(uint*)(p + 4) = geometryHandle;
                    *(uint*)(p + 12) = outlineFill; *(uint*)(p + 16) = (uint)outlineData.Length;
                    outlineData.CopyTo(command, 20);
                    Assert.AreEqual(0, send(p, (uint)command.Length, 0, channel));
                }
                Array.Clear(outlineData);
                uint* pathRender = stackalloc uint[] { 0x18, data, 24, 24, 0x46, brush, 0, geometryHandle, 0 };
                Assert.AreEqual(0, send(pathRender, 36, 0, channel));
            }
            else Assert.AreEqual(0, send(render, 28, 0, channel));
            byte* targetPacket = stackalloc byte[36]; new Span<byte>(targetPacket, 36).Clear();
            *(uint*)targetPacket = 0x34; *(uint*)(targetPacket + 4) = destination;
            *(ulong*)(targetPacket + 16) = (ulong)target; *(uint*)(targetPacket + 24) = 32; *(uint*)(targetPacket + 28) = 24;
            Assert.AreEqual(0, send(targetPacket, 36, 0, channel));
            uint* binding = stackalloc uint[] { 0x22, visual, data };
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            binding[0] = 0x35; binding[1] = destination; binding[2] = visual;
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            if (translated || outline)
            {
                byte* offset = stackalloc byte[24]; new Span<byte>(offset, 24).Clear();
                *(uint*)offset = 0x1b; *(uint*)(offset + 4) = visual;
                *(double*)(offset + 8) = outline ? 2 : 1; *(double*)(offset + 16) = outline ? 20 : 1;
                Assert.AreEqual(0, send(offset, 24, 0, channel));
            }
            Stage("close batch");
            Assert.AreEqual(0, close(channel));
            Stage("batch closed");
            Assert.AreEqual(0, commit(channel));
            Stage("committed");
            if (releaseGlyph)
            {
                var releaseOnChannel = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)NativeLibrary.GetExport(module, "MilResource_ReleaseOnChannel");
                int deleted = 0;
                Assert.AreEqual(0, releaseOnChannel(channel, glyph, &deleted));
                Assert.AreEqual(1, deleted);
                Assert.AreEqual(0, close(channel));
                Assert.AreEqual(0, commit(channel));
            }
            if (retry)
            {
                nint bitmapLock = 0;
                try
                {
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock));
                    Assert.AreEqual(unchecked((int)0x88982f0d), present(connection));
                }
                finally { Release(bitmapLock); }
            }
            Stage("present");
            Assert.AreEqual(0, present(connection));
            Stage("presented");
            if (closeOwner)
            {
                if (disconnectOwner)
                {
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect"))(connection));
                    connection = 0;
                }
                else
                {
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel"))(channel));
                    channel = 0;
                }
            }
            if (floating)
            {
                var actual = new System.Numerics.Vector4[32 * 24];
                fixed (System.Numerics.Vector4* pixels = actual)
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 512, (uint)actual.Length * 16, (byte*)pixels));
                foreach (var pixel in actual)
                    Assert.AreEqual(new System.Numerics.Vector4(4 * pixel.W, -2 * pixel.W, 1e10f * pixel.W, pixel.W), pixel);
                Assert.AreEqual(1f, actual.Max(pixel => pixel.W));
            }
            else
            {
                byte[] actual = new byte[expected.Length];
                fixed (byte* pixels = actual) Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 128, (uint)actual.Length, pixels));
                if (outline) return actual;
                CollectionAssert.AreEqual(expected, actual);
            }
            return [];
        }
        finally
        {
            if (channel != 0) ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel"))(channel);
            if (connection != 0) ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect"))(connection);
            Release(target); Release(bitmap); Release(milFactory); Release(font); Release(family); Release(collection); Release(factory);
        }
    }

    private static void Release(nint value) { if (value != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value); }
}
