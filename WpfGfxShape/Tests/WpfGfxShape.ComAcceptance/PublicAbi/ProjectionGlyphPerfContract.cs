namespace NativeAcceptance.PublicAbi;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

internal static unsafe partial class Program
{
    [DllImport("dwrite.dll", ExactSpelling = true)]
    private static extern int DWriteCreateFactory(uint kind, Guid* iid, nint* factory);

    private static void ProjectionGlyphPerfContract()
    {
        var bounds = (delegate* unmanaged[Stdcall]<Matrix4x4*, float*, float*, int>)Export("MIL3DCalcProjected2DBounds");
        float* box = stackalloc float[6];
        float* rect = stackalloc float[4];
        for (int test = 0; test < 24; test++)
        {
            Matrix4x4 matrix = Matrix4x4.Identity;
            box[0] = -2; box[1] = -3; box[2] = .2f; box[3] = 5; box[4] = 7; box[5] = .5f;
            switch (test % 12)
            {
                case 1: box[2] = -2; break;
                case 2: box[2] = 2; break;
                case 3: box[2] = -.5f; box[5] = 2; break;
                case 4: box[3] = box[4] = box[5] = 0; break;
                case 5: box[3] = -5; break;
                case 6: matrix.M44 = 0; box[2] = box[5] = 0; break;
                case 7: matrix.M44 = -1; break;
                case 8: box[0] = float.NaN; break;
                case 9: matrix.M11 = float.PositiveInfinity; break;
                case 10: matrix.M14 = .25f; break;
                case 11: matrix = Matrix4x4.CreateRotationZ(.4f) * Matrix4x4.CreateTranslation(3, -4, 0); break;
            }
            if (test >= 12) matrix *= Matrix4x4.CreateScale(2, 3, 1);
            int hr = bounds(&matrix, box, rect);
            Calls.Add(new { Operation = "ProjectedBounds", Test = test, HResult = hr,
                Bits = Enumerable.Range(0, 4).Select(i => BitConverter.SingleToInt32Bits(rect[i])).ToArray() });
        }
        Matrix4x4 identity = Matrix4x4.Identity;
        Calls.Add(new { Operation = "ProjectedBounds.Null", HResult = bounds(null, box, rect) });
        var flags = (delegate* unmanaged[Stdcall]<uint, void>)Export("SetMilPerfInstrumentationFlags");
        foreach (uint value in new uint[] { 0, 2, uint.MaxValue, 0 })
        { flags(value); Calls.Add(new { Operation = "PerfFlags", Value = value }); }
        PerfPresentContract();
        GlyphContract();
    }

    private static void VerifyProjectionGlyphReference()
    {
        using var reference = System.Text.Json.JsonDocument.Parse(File.ReadAllText(
            projectionGlyphEvidenceInput ?? throw new InvalidOperationException("Supply the current original report as the candidate evidence input.")));
        var oracle = reference.RootElement;
        if (oracle.GetProperty("Mode").GetString() != "reference" || !oracle.GetProperty("Passed").GetBoolean()
            || oracle.GetProperty("SchemaVersion").GetInt32() != 1
            || oracle.GetProperty("Runtime").GetString() != RuntimeInformation.FrameworkDescription
            || oracle.GetProperty("OriginalSha256").GetString() != Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(ReferencePath()))))
            throw new InvalidOperationException("Original evidence schema, role or runtime identity differs.");
        using var actual = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(Calls));
        foreach (string operation in new[] { "ProjectedBounds", "ProjectedBounds.Null", "GlyphOutline" })
        {
            var expected = reference.RootElement.GetProperty("Calls").EnumerateArray()
                .Where(c => c.GetProperty("Operation").GetString() == operation).ToArray();
            var observed = actual.RootElement.EnumerateArray()
                .Where(c => c.GetProperty("Operation").GetString() == operation).ToArray();
            if (expected.Length != observed.Length) throw new InvalidOperationException(operation + ".CountMismatch");
            for (int i = 0; i < expected.Length; i++)
            {
                bool equal = System.Text.Json.JsonElement.DeepEquals(expected[i], observed[i]);
                Calls.Add(new { Operation = "ReferenceComparison", Family = operation, Test = i, Equal = equal });
                if (operation == "GlyphOutline")
                {
                    byte[] left = Convert.FromHexString(expected[i].GetProperty("GeometryBytes").GetString()!);
                    byte[] right = Convert.FromHexString(observed[i].GetProperty("GeometryBytes").GetString()!);
                    var undefined = GlyphPackingOffsets(left);
                    bool definedEqual = left.Length == right.Length && Enumerable.Range(0, left.Length)
                        .All(offset => undefined.Contains(offset) || left[offset] == right[offset]);
                    Calls.Add(new { Operation = "GlyphDefinedFieldsComparison", Test = i, Equal = definedEqual,
                        UndefinedOffsets = undefined.Order().ToArray(), StrictRawEqual = equal,
                        Authority = "GlyphRunGeometrySink.cpp Initialize/BeginFigure/AddGenericPoly: WPFAlloc without ForcePacking assignment; wgx_core_types.h MilPathGeometry/MilPathFigure/MilSegmentLine/MilSegmentBezier" });
                }
                if (!equal)
                {
                    foreach (var property in expected[i].EnumerateObject())
                    {
                        var value = observed[i].GetProperty(property.Name);
                        if (System.Text.Json.JsonElement.DeepEquals(property.Value, value)) continue;
                        if (property.Name == "GeometryBytes")
                        {
                            byte[] left = Convert.FromHexString(property.Value.GetString()!);
                            byte[] right = Convert.FromHexString(value.GetString()!);
                            for (int offset = 0; offset < Math.Min(left.Length, right.Length); offset++)
                                if (left[offset] != right[offset])
                                    Calls.Add(new { Operation = "GeometryByteDifference", Test = i, Offset = offset, Expected = left[offset], Actual = right[offset] });
                        }
                        else Calls.Add(new { Operation = "ReferenceDifference", Family = operation, Test = i, Field = property.Name, Expected = property.Value.Clone(), Actual = value.Clone() });
                    }
                }
            }
        }
    }

    private static HashSet<int> GlyphPackingOffsets(byte[] bytes)
    {
        HashSet<int> offsets = [];
        if (bytes.Length < 48) return offsets;
        void Add(int start) { for (int i = start; i < start + 4; i++) offsets.Add(i); }
        Add(44);
        int figure = 48;
        while (figure < bytes.Length)
        {
            Add(figure + 36);
            int size = checked((int)BitConverter.ToUInt32(bytes, figure + 12));
            int end = checked(figure + size);
            if (size < 40 || end > bytes.Length) throw new InvalidOperationException("GlyphFigureLayout");
            int segment = figure + 40;
            while (segment < end)
            {
                Add(segment + 12);
                uint type = BitConverter.ToUInt32(bytes, segment);
                segment += type switch { 1 => 32, 2 => 64, _ => throw new InvalidOperationException("GlyphSegmentLayout") };
            }
            if (segment != end) throw new InvalidOperationException("GlyphSegmentSize");
            figure = end;
        }
        return offsets;
    }

    private static void GlyphContract()
    {
        nint factory = 0, collection = 0, family = 0, font = 0, face = 0;
        Guid iid = new("b859ee5a-d838-4b5b-a2e8-1adc7d93db48");
        try
        {
            Check("DWrite.Factory", DWriteCreateFactory(0, &iid, &factory));
            Check("DWrite.Collection", ((delegate* unmanaged[Stdcall]<nint, nint*, int, int>)(*(void***)factory)[3])(factory, &collection, 0));
            uint index = 0; int exists = 0;
            fixed (char* name = "Arial")
                Check("DWrite.FindArial", ((delegate* unmanaged[Stdcall]<nint, char*, uint*, int*, int>)(*(void***)collection)[5])(collection, name, &index, &exists));
            if (exists == 0) throw new InvalidOperationException("ArialUnavailable");
            Check("DWrite.Family", ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)collection)[4])(collection, index, &family));
            Check("DWrite.Font", ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, nint*, int>)(*(void***)family)[7])(family, 400, 5, 0, &font));
            Check("DWrite.Face", ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)font)[13])(font, &face));
            var outline = (delegate* unmanaged[Stdcall]<nint, ushort, byte, double, byte**, uint*, uint*, int>)Export("MilGlyphRun_GetGlyphOutline");
            var free = (delegate* unmanaged[Stdcall]<byte*, int>)Export("MilGlyphRun_ReleasePathGeometryData");
            uint* characters = stackalloc uint[5] { 32, 65, 79, 233, 0x1F600 };
            ushort* glyphs = stackalloc ushort[5];
            Check("DWrite.Indices", ((delegate* unmanaged[Stdcall]<nint, uint*, uint, ushort*, int>)(*(void***)face)[11])(face, characters, 5, glyphs));
            for (int test = 0; test < 30; test++)
            {
                double em = test < 10 ? 24 : test < 20 ? 0 : test < 25 ? -1 : double.NaN;
                ushort glyph = glyphs[test % 5];
                byte* bytes = null; uint size = 0, fill = 99;
                ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)face)[1])(face);
                int hr = outline(face, glyph, (byte)(test % 2), em, &bytes, &size, &fill);
                string digest = bytes == null ? "" : Convert.ToHexString(SHA256.HashData(new ReadOnlySpan<byte>(bytes, checked((int)size))));
                string geometryBytes = bytes == null ? "" : Convert.ToHexString(new ReadOnlySpan<byte>(bytes, checked((int)size)));
                Calls.Add(new { Operation = "GlyphOutline", Test = test, Character = characters[test % 5], Glyph = glyph, HResult = hr, Size = size, Fill = fill, GeometrySha256 = digest, GeometryBytes = geometryBytes });
                if (bytes != null) Check("Glyph.Release", free(bytes));
                Check("Glyph.ReleaseNull", free(null));
            }
        }
        finally { Release(face); Release(font); Release(family); Release(collection); Release(factory); }
    }
}
