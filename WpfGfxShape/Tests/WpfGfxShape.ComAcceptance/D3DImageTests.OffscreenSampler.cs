using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    internal static void RunOffscreenSampler(bool candidate, string root)
    {
        Directory.CreateDirectory(root);
        var observations = new List<object>();
        void Record(string stage, object detail)
        {
            observations.Add(new { Stage = stage, Detail = detail });
            File.WriteAllText(Path.Combine(root, "evidence.json"), JsonSerializer.Serialize(observations, new JsonSerializerOptions { WriteIndented = true }));
            ExitDiagnostics.Stage("IW " + stage);
        }
        string selected = candidate ? AcceptancePaths.AotDll : AcceptancePaths.OriginalDll;
        nint module = NativeLibrary.Load(selected);
        if (candidate)
        {
            nint Resolve(string name, Assembly assembly, DllImportSearchPath? search)
                => name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) ? module : 0;
            NativeLibrary.SetDllImportResolver(typeof(Visual).Assembly, Resolve);
            NativeLibrary.SetDllImportResolver(typeof(Dispatcher).Assembly, Resolve);
        }
        object Identity(string path) => new { Path = path, SHA256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), Version = FileVersionInfo.GetVersionInfo(path).ProductVersion };
        Record("identity", new { Pid = Environment.ProcessId, Candidate = candidate, Selected = Identity(selected), Test = Identity(typeof(D3DImageTests).Assembly.Location), TestMvid = typeof(D3DImageTests).Module.ModuleVersionId, Wpf = Identity(typeof(Visual).Assembly.Location), WpfMvid = typeof(Visual).Module.ModuleVersionId, Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(), Contract = "SoftwareOnly offscreen sampler", Backend = "SoftwareOnly", Threshold = 0 });
        try
        {
            WeakReference[] references = RenderOffscreenSampler(root, Record);
            using (var process = Process.GetCurrentProcess())
            {
                var actual = process.Modules.Cast<ProcessModule>().Where(m => Path.GetFileName(m.FileName).Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase)).ToArray();
                Record("actualNative", actual.Select(m => Identity(m.FileName)).ToArray());
                Assert.AreEqual(1, actual.Length);
                Assert.AreEqual(Path.GetFullPath(selected), Path.GetFullPath(actual[0].FileName), true);
            }
            Dispatcher.CurrentDispatcher.InvokeShutdown();
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            Record("cleanup", new { ManagedAlive = references.Select(r => r.IsAlive).ToArray(), NativeComRefsMeasured = false, ModuleRetained = true });
            CollectionAssert.AreEqual(new bool[references.Length], references.Select(r => r.IsAlive).ToArray());
        }
        finally
        {
            if (!Dispatcher.CurrentDispatcher.HasShutdownStarted) Dispatcher.CurrentDispatcher.InvokeShutdown();
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            Record("finally", new { DispatcherShutdown = Dispatcher.CurrentDispatcher.HasShutdownFinished, FinalizersDrained = true });
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] RenderOffscreenSampler(string root, Action<string, object> record)
    {
        const int size = 16;
        var first = new WriteableBitmap(size, size, 96, 96, PixelFormats.Pbgra32, null);
        var second = new WriteableBitmap(size, size, 96, 96, PixelFormats.Pbgra32, null);
        void Write(WriteableBitmap bitmap, uint color) => bitmap.WritePixels(new Int32Rect(0, 0, size, size), Enumerable.Repeat(color, size * size).ToArray(), size * 4, 0);
        Write(first, 0xff0000ff); Write(second, 0xff00ff00);
        var input = new DrawingVisual();
        var secondary = new DrawingVisual();
        using (var drawing = input.RenderOpen()) drawing.DrawImage(first, new Rect(0, 0, size, size));
        using (var drawing = secondary.RenderOpen()) drawing.DrawImage(second, new Rect(0, 0, size, size));
        byte[] bytecode = MemoryMarshal.AsBytes(TwoInputShaderTokens.AsSpan()).ToArray();
        File.WriteAllBytes(Path.Combine(root, "shader.bin"), bytecode);
        using var stream = new MemoryStream(bytecode);
        var shader = new PixelShader { ShaderRenderMode = ShaderRenderMode.SoftwareOnly };
        shader.SetStreamSource(stream);
        var brush = new VisualBrush(secondary) { ViewportUnits = BrushMappingMode.RelativeToBoundingBox, ViewboxUnits = BrushMappingMode.RelativeToBoundingBox, Viewport = new Rect(0, 0, 1, 1), Viewbox = new Rect(0, 0, 1, 1), Stretch = Stretch.Fill, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top };
        var effect = new IwEffect(shader, brush);
        input.Effect = effect;
        var target = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        void Read(string stage, uint expected)
        {
            target.Clear(); target.Render(input);
            uint[] pixels = new uint[size * size]; target.CopyPixels(pixels, size * 4, 0);
            File.WriteAllBytes(Path.Combine(root, stage + ".bgra"), MemoryMarshal.AsBytes(pixels.AsSpan()).ToArray());
            uint[] rgb = pixels.Select(p => p & 0xffffffu).ToArray();
            record(stage, new { Expected = expected.ToString("X8"), Actual = pixels.Select(p => p.ToString("X8")).Distinct().ToArray(), Nonempty = rgb.Any(p => p != 0), Source = "RenderTargetBitmap.CopyPixels consumer target", ChangedResource = "WriteableBitmap second input" });
            CollectionAssert.AreEqual(Enumerable.Repeat(expected, pixels.Length).ToArray(), rgb);
        }
        try
        {
            Read("initial", 0x0000ff80);
            Write(second, 0xffff0000);
            Read("mutation", 0x00ff0080);
        }
        finally { input.Effect = null; brush.Visual = null; target.Clear(); record("scene detached", new { Effect = true, VisualBrush = true, TargetCleared = true }); }
        return [new(first), new(second), new(input), new(secondary), new(effect), new(brush), new(target), new(shader)];
    }

    private sealed class IwEffect : ShaderEffect
    {
        private static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(IwEffect), 0, SamplingMode.Bilinear);
        private static readonly DependencyProperty SecondaryProperty = RegisterPixelShaderSamplerProperty("Secondary", typeof(IwEffect), 1, SamplingMode.Bilinear);
        internal IwEffect(PixelShader shader, Brush secondary)
        {
            PixelShader = shader;
            SetValue(SecondaryProperty, secondary);
            UpdateShaderValue(InputProperty); UpdateShaderValue(SecondaryProperty);
        }
    }
}
