using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace WpfGfxShape.Core;

internal static class JpBrushTrace
{
    [Conditional("JP_BRUSH_TRACE")]
    internal static void Record(string boundary, int hr, string context = "", [CallerLineNumber] int line = 0)
    {
        string? root = Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT");
        string directory = string.IsNullOrWhiteSpace(root) ? Path.Combine(AppContext.BaseDirectory, "Results") : root;
        Directory.CreateDirectory(directory);
        using var stream = new FileStream(Path.Combine(directory, $"jp-brush-{Environment.ProcessId}.jsonl"), FileMode.Append, FileAccess.Write, FileShare.Read);
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteNumber("pid", Environment.ProcessId);
            writer.WriteNumber("thread", Environment.CurrentManagedThreadId);
            writer.WriteString("boundary", boundary);
            writer.WriteNumber("line", line);
            writer.WriteNumber("hr", hr);
            writer.WriteString("context", context);
            writer.WriteString("base", AppContext.BaseDirectory);
            writer.WriteEndObject();
        }
        stream.WriteByte(10);
    }
}
