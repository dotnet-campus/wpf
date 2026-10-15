using System.Numerics;
using System.Runtime.InteropServices;

internal static unsafe partial class Program
{
    [StructLayout(LayoutKind.Sequential)]
    private struct TileRect
    {
        internal double X, Y, Width, Height;
        internal TileRect(double x, double y, double width, double height)
            => (X, Y, Width, Height) = (x, y, width, height);
    }

    private static void TileMappingContract()
    {
        var mapping = (delegate* unmanaged[Stdcall]<Matrix4x4*, Matrix4x4*, uint, uint, uint, uint, uint,
            TileRect*, TileRect*, TileRect*, TileRect*, Matrix4x4*, int*, void>)Export("MilUtility_GetTileBrushMapping");
        for (int test = 0; test < 192; test++)
        {
            uint stretch = (uint)(test % 4), x = (uint)(test / 4 % 3), y = (uint)(test / 12 % 3);
            uint viewportUnits = (uint)(test / 36 % 2), viewboxUnits = (uint)(test / 72 % 2);
            TileRect shape = new(10, -5, 80, 40), content = new(-8, 3, 24, 16);
            TileRect viewport = new(.25, .125, .75, .5), box = new(.125, .25, .5, .75);
            Matrix4x4 transform = Matrix4x4.CreateRotationZ(.3f) * Matrix4x4.CreateTranslation(2, -7, 0);
            Matrix4x4 relative = Matrix4x4.CreateScale(.75f, 1.25f, 1) * Matrix4x4.CreateTranslation(.1f, -.2f, 0);
            Matrix4x4 result = new(91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106);
            int empty = -1;
            if (test >= 144)
            {
                switch (test % 8)
                {
                    case 0: box.Width = 0; break;
                    case 1: viewport.Height = -1; break;
                    case 2: box.X = double.NaN; break;
                    case 3: viewport.Width = double.NaN; break;
                    case 4: shape.Width = 0; break;
                    case 5: shape.Height = 0; break;
                    case 6: box.Width = double.PositiveInfinity; break;
                    case 7: viewport.X = double.PositiveInfinity; break;
                }
            }
            mapping(test % 3 == 0 ? null : &transform, test % 3 == 1 ? null : &relative,
                stretch, x, y, viewportUnits, viewboxUnits, &shape, &content, &viewport, &box, &result, &empty);
            Calls.Add(new { Operation = "TileMapping", Test = test, Stretch = stretch, AlignmentX = x,
                AlignmentY = y, ViewportUnits = viewportUnits, ViewboxUnits = viewboxUnits, Empty = empty,
                Viewport = new[] { Bits(viewport.X), Bits(viewport.Y), Bits(viewport.Width), Bits(viewport.Height) },
                Viewbox = new[] { Bits(box.X), Bits(box.Y), Bits(box.Width), Bits(box.Height) },
                Matrix = new[] { result.M11, result.M12, result.M13, result.M14, result.M21, result.M22, result.M23, result.M24,
                    result.M31, result.M32, result.M33, result.M34, result.M41, result.M42, result.M43, result.M44 }
                    .Select(BitConverter.SingleToInt32Bits).ToArray() });
        }
    }

    private static void VerifyTileMappingReference()
    {
        using var reference = System.Text.Json.JsonDocument.Parse(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "ReferenceData", "tile-reference.json")));
        var expected = reference.RootElement.GetProperty("Calls").EnumerateArray()
            .Where(c => c.GetProperty("Operation").GetString() == "TileMapping").ToArray();
        using var actual = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(Calls));
        var observed = actual.RootElement.EnumerateArray()
            .Where(c => c.GetProperty("Operation").GetString() == "TileMapping").ToArray();
        var differences = new List<int>();
        for (int i = 0; i < expected.Length; i++)
            if (expected[i].GetRawText().Replace(" ", "").Replace("\r", "").Replace("\n", "") != observed[i].GetRawText())
                differences.Add(i);
        Calls.Add(new { Operation = "TileMapping.Differential", Cases = observed.Length,
            Differences = differences, ReferenceSha256 = reference.RootElement.GetProperty("Sha256").GetString() });
        if (observed.Length != 192 || differences.Count != 0)
            throw new InvalidOperationException("TileMappingDifferentialMismatch");
    }

    private static long Bits(double value) => BitConverter.DoubleToInt64Bits(value);
}
