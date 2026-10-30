using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace WpfGfxShape.Core;

internal static class KiResizeTrace
{
    private static int _sequence;
    internal static int Id(object? value) => value is null ? 0 : RuntimeHelpers.GetHashCode(value);

    [Conditional("KI_RESIZE_TRACE")]
    internal static void Record(string stage, string context, int hr = 0)
    {
        int sequence = Interlocked.Increment(ref _sequence);
        if (sequence > 512) return;
        try
        {
            string? root = Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT");
            string directory = string.IsNullOrWhiteSpace(root) ? Path.Combine(AppContext.BaseDirectory, "Results") : root;
            Directory.CreateDirectory(directory);
            using var stream = new FileStream(Path.Combine(directory, $"ki-resize-{Environment.ProcessId}.jsonl"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                writer.WriteNumber("sequence", sequence);
                writer.WriteNumber("pid", Environment.ProcessId);
                writer.WriteNumber("thread", Environment.CurrentManagedThreadId);
                writer.WriteString("stage", stage);
                writer.WriteNumber("hr", hr);
                writer.WriteString("context", context);
                writer.WriteEndObject();
            }
            stream.WriteByte(10);
        }
        catch (IOException error) { Debug.WriteLine(error); }
        catch (UnauthorizedAccessException error) { Debug.WriteLine(error); }
    }
}
