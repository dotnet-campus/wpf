using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class ReplacementConstrainedSequenceTests
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    public TestContext TestContext { get; set; } = null!;

    /// <summary>Runs bounded legal software clear/clip sequences against an independent official module.</summary>
    [DataTestMethod]
    [DataRow(104729)]
    [DataRow(130363)]
    [DataRow(155921)]
    public void WhenFixedSeedRunsThenEverySoftwareFrameMatchesOfficial(int seed)
    {
        string root = NewRoot("seed-" + seed);
        string replay = Path.Combine(AppContext.BaseDirectory, "replacement-r-replay.json");
        Input input = File.Exists(replay) ? JsonSerializer.Deserialize<Input>(File.ReadAllText(replay)) ?? throw new InvalidDataException() : Generate(seed);
        Validate(input);
        Save(Path.Combine(root, "input.json"), input);
        var official = new Api(AcceptancePaths.OriginalDll);
        var candidate = new Api(AcceptancePaths.AotDll);
        Save(Path.Combine(root, "identity.json"), new { Official = official.Identity, Candidate = candidate.Identity, Backend = "Software bitmap target; usage=1; no HWND/GPU/device-loss", Seed = input.Seed });
        Observation left = Execute(official, input);
        Save(Path.Combine(root, "official.json"), left);
        Observation right = Execute(candidate, input);
        Save(Path.Combine(root, "candidate.json"), right);
        WriteFrames(root, "official", left);
        WriteFrames(root, "candidate", right);
        string? failure = Detect(input, left, right);
        Save(Path.Combine(root, "comparison.json"), new { Failure = failure, Frames = input.Operations.Length, Threshold = 0 });
        if (failure is not null)
        {
            Save(Path.Combine(root, "failure-input.json"), input);
            int attempts = 0;
            Input minimized = Reduce(input, trial =>
            {
                string directory = Path.Combine(root, "shrink-" + (++attempts).ToString("D3"));
                Directory.CreateDirectory(directory);
                Observation original = Execute(official, trial), replacement = Execute(candidate, trial);
                Save(Path.Combine(directory, "input.json"), trial);
                Save(Path.Combine(directory, "official.json"), original);
                Save(Path.Combine(directory, "candidate.json"), replacement);
                string? result = Detect(trial, original, replacement);
                Save(Path.Combine(directory, "comparison.json"), new { Failure = result });
                return result == failure;
            });
            Save(Path.Combine(root, "minimal-input.json"), minimized);
            Save(Path.Combine(root, "shrink-summary.json"), new { Attempts = attempts, Original = input.Operations.Length, Remaining = minimized.Operations.Length, Guarantee = "single-operation deletion fixed point with mandatory initial full clear; not numeric/global minimum" });
        }
        TestContext.WriteLine(root);
        Assert.IsNull(failure, root);
    }

    internal static string NewRoot(string label)
    {
        string root = Path.Combine(AcceptancePaths.Results, "replacement-r", label + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    internal static void Save<T>(string path, T value) => File.WriteAllText(path, JsonSerializer.Serialize(value, Json));

    internal sealed record Operation(bool Full, int Left, int Top, int Right, int Bottom, int Color);
    internal sealed record Input(int Seed, int Width, int Height, Operation[] Operations);
    internal sealed record Observation(byte[][] Frames, int[] Results);

    internal static Input Generate(int seed)
    {
        // Explicit PRNG keeps the serialized seed contract independent of System.Random implementation changes.
        uint state = (uint)seed;
        int Next(int bound) { state = unchecked(state * 1664525 + 1013904223); return (int)(state % (uint)bound); }
        int width = 4 + Next(13), height = 4 + Next(13);
        var operations = new List<Operation>();
        int previous = -1;
        for (int index = 0; index < 16; index++)
        {
            int color = Next(3);
            if (color == previous) color = (color + 1) % 3;
            int x = Next(width), y = Next(height);
            operations.Add(new(index % 4 == 0, x, y, x + 1 + Next(width - x), y + 1 + Next(height - y), color));
            previous = color;
        }
        return new(seed, width, height, operations.ToArray());
    }

    private static void Validate(Input input)
    {
        if (input.Width is < 1 or > 16 || input.Height is < 1 or > 16 || input.Operations.Length is < 1 or > 16 || !input.Operations[0].Full)
            throw new InvalidDataException();
        foreach (Operation op in input.Operations)
            if (op.Color is < 0 or > 2 || op.Left < 0 || op.Top < 0 || op.Right > input.Width || op.Bottom > input.Height || op.Right <= op.Left || op.Bottom <= op.Top)
                throw new InvalidDataException();
    }

    internal static string? Detect(Input input, Observation official, Observation candidate)
    {
        if (official.Results.Any(hr => hr != 0) || candidate.Results.Any(hr => hr != 0)) return "call-failure";
        if (official.Frames.Length != input.Operations.Length || candidate.Frames.Length != input.Operations.Length) return "frame-count";
        for (int index = 0; index < input.Operations.Length; index++)
        {
            Operation op = input.Operations[index];
            int pixel = ((op.Full ? 0 : op.Top) * input.Width + (op.Full ? 0 : op.Left)) * 4;
            byte[] expected = op.Color switch { 0 => [0, 0, 255, 255], 1 => [0, 255, 0, 255], _ => [255, 0, 0, 255] };
            foreach (byte[] frame in new[] { official.Frames[index], candidate.Frames[index] })
            {
                if (frame.Length != input.Width * input.Height * 4) return "frame-size";
                if (!frame.AsSpan(pixel, 4).SequenceEqual(expected)) return "content";
            }
            if (!official.Frames[index].AsSpan().SequenceEqual(candidate.Frames[index])) return "pixels";
            if (index > 0 && !official.Frames[index - 1].AsSpan(pixel, 4).SequenceEqual(expected) &&
                (official.Frames[index].AsSpan().SequenceEqual(official.Frames[index - 1]) || candidate.Frames[index].AsSpan().SequenceEqual(candidate.Frames[index - 1]))) return "stale-frame";
        }
        return null;
    }

    internal static Input Reduce(Input input, Func<Input, bool> preservesFailure)
    {
        Input current = input;
        bool changed;
        do
        {
            changed = false;
            for (int index = current.Operations.Length - 1; index >= 1; index--)
            {
                Input trial = current with { Operations = current.Operations.Where((_, position) => position != index).ToArray() };
                if (!preservesFailure(trial)) continue;
                current = trial;
                changed = true;
                break;
            }
        } while (changed);
        return current;
    }

    internal static Observation Execute(Api api, Input input)
    {
        Validate(input);
        nint factory = 0, target = 0, bitmap = 0;
        var results = new List<int>();
        var frames = new List<byte[]>();
        try
        {
            int hr = api.Create(&factory, 0x200184C0);
            results.Add(hr);
            if (hr != 0) return new([], results.ToArray());
            hr = api.CreateTarget(factory, (uint)input.Width, (uint)input.Height, 16, 96, 96, 1, &target);
            results.Add(hr);
            if (hr != 0) return new([], results.ToArray());
            hr = api.GetBitmap(target, &bitmap);
            results.Add(hr);
            if (hr != 0) return new([], results.ToArray());
            float* color = stackalloc float[4];
            foreach (Operation op in input.Operations)
            {
                color[0] = op.Color == 0 ? 1 : 0; color[1] = op.Color == 1 ? 1 : 0; color[2] = op.Color == 2 ? 1 : 0; color[3] = 1;
                Clip clip = new() { Left = op.Left, Top = op.Top, Right = op.Right, Bottom = op.Bottom };
                results.Add(((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)target)[4])(target, color, op.Full ? 0 : (nint)(&clip)));
                byte[] frame = new byte[input.Width * input.Height * 4];
                fixed (byte* pixels = frame)
                    results.Add(((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, (uint)input.Width * 4, (uint)frame.Length, pixels));
                frames.Add(frame);
            }
        }
        finally { Release(bitmap); Release(target); Release(factory); }
        return new(frames.ToArray(), results.ToArray());
    }

    private static void WriteFrames(string root, string role, Observation observation)
    {
        for (int index = 0; index < observation.Frames.Length; index++)
            File.WriteAllBytes(Path.Combine(root, role + "-" + index.ToString("D2") + ".bgra"), observation.Frames[index]);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Clip { internal int IsNull; internal float Left, Top, Right, Bottom; }

    internal static void Release(nint value)
    {
        if (value != 0) _ = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
    }

    internal sealed class Api
    {
        internal readonly nint Module;
        internal readonly object Identity;
        internal readonly delegate* unmanaged[Stdcall]<nint*, uint, int> Create;
        internal readonly delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int> CreateTarget;
        internal readonly delegate* unmanaged[Stdcall]<nint, nint*, int> GetBitmap;
        internal Api(string path)
        {
            Module = NativeLibrary.Load(path);
            char[] buffer = new char[32768];
            uint length = GetModuleFileName(Module, buffer, buffer.Length);
            if (length == 0 || length >= buffer.Length) throw new InvalidOperationException();
            string actual = new(buffer, 0, (int)length);
            Identity = new { Path = actual, Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual))), Version = FileVersionInfo.GetVersionInfo(actual).FileVersion, Architecture = RuntimeInformation.ProcessArchitecture.ToString(), PointerSize = IntPtr.Size, Runtime = RuntimeInformation.FrameworkDescription, Wpf = typeof(System.Windows.Media.Visual).Assembly.FullName };
            Create = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(Module, "MILCreateFactory");
            CreateTarget = (delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int>)NativeLibrary.GetExport(Module, "MILFactoryCreateBitmapRenderTarget");
            GetBitmap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(Module, "MILRenderTargetBitmapGetBitmap");
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetModuleFileNameW")]
    private static extern uint GetModuleFileName(nint module, [Out] char[] path, int size);
}
