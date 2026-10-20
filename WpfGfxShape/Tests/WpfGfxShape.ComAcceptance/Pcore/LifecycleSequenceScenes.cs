namespace NativeAcceptance.Pcore;

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static partial class Program
{
    private static void RenderLifecycleSequence()
    {
        const int seed = 0x521724;
        var random = new Random(seed);
        var samples = new List<object>();
        for (int cycle = 0; cycle < 8; cycle++)
        {
            WeakReference[] references = RenderLifecycleCycle(cycle, random);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            int alive = references.Count(reference => reference.IsAlive);
            samples.Add(new { cycle, tracked = references.Length, alive });
            File.WriteAllText(Path.Combine(output, "sequence-resources.json"), JsonSerializer.Serialize(samples));
            if (alive != 0) throw new InvalidOperationException("Closed sequence retained tracked WPF objects.");
        }
        Evidence["caseId"] = "replacement-stage2-lifecycle-sequence-v1";
        Evidence["sequenceSeed"] = seed;
        Evidence["resourceSamples"] = samples;
        Evidence["backendScope"] = "WPF RenderTargetBitmap; not HWND presentation or device-loss";
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5394", Justification = "Fixed-seed test dimensions require reproducibility, not cryptographic unpredictability.")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] RenderLifecycleCycle(int cycle, Random random)
    {
        var visual = new DrawingVisual();
        var brush = new SolidColorBrush(Colors.Red);
        var references = new List<WeakReference> { new(visual), new(brush) };
        var events = new List<string> { "create" };
        byte[]? previous = null;
        int width = 16, height = 16;
        for (int frame = 0; frame < 10; frame++)
        {
            if (frame >= 2 && frame % 2 == 0)
            {
                width = random.Next(1, 49);
                height = random.Next(1, 49);
                events.Add("resize:" + frame);
            }
            Color color = frame % 2 == 0 ? Colors.Red : Colors.Blue;
            brush.Color = color;
            events.Add("update:" + frame);
            using (DrawingContext dc = visual.RenderOpen())
                dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
            var target = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            references.Add(new WeakReference(target));
            target.Render(visual);
            byte[] pixels = new byte[width * height * 4];
            target.CopyPixels(pixels, width * 4, 0);
            for (int offset = 0; offset < pixels.Length; offset += 4)
                if (pixels[offset] != color.B || pixels[offset + 1] != color.G || pixels[offset + 2] != color.R || pixels[offset + 3] != 255)
                    throw new InvalidOperationException("New sequence frame did not contain the submitted color.");
            if (previous is not null && previous.AsSpan().SequenceEqual(pixels))
                throw new InvalidOperationException("Stale sequence frame.");
            previous = pixels;
            File.WriteAllBytes(Path.Combine(output, $"sequence-{cycle:D2}-{frame:D2}-{width}x{height}.pbgra32"), pixels);
            if (cycle == 0 && frame < 3)
                File.WriteAllText(Path.Combine(output, $"sequence-raw-{frame}.json"), JsonSerializer.Serialize(new { width, height, pixels = pixels.Select(value => (int)value).ToArray() }));
            events.Add("frame:" + frame);
        }
        using (visual.RenderOpen()) { }
        events.Add("close");
        File.WriteAllText(Path.Combine(output, $"sequence-events-{cycle:D2}.json"), JsonSerializer.Serialize(events));
        return references.ToArray();
    }
}
