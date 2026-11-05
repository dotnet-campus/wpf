using System.Text.Json;
namespace WpfGfxShape.ComAcceptance;
internal static class AcceptancePaths
{
    internal static string ValidateIdentity() => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(AotDll)));
    internal static string AotDll => Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll");
    internal static string OriginalDll => throw new InvalidOperationException("Original is prohibited in candidate-only resize.");
    internal static string Results { get { string path = Path.Combine(AppContext.BaseDirectory, "Evidence"); Directory.CreateDirectory(path); return path; } }
}
internal static class NativeRun { internal static string PathsRoot => AcceptancePaths.Results; }
internal static class ExitDiagnostics
{
    internal static void Stage(string name)
    {
        Console.WriteLine(name);
        File.AppendAllText(Path.Combine(AcceptancePaths.Results, "stages.jsonl"), JsonSerializer.Serialize(new { Utc = DateTimeOffset.UtcNow, Pid = Environment.ProcessId, Stage = name }) + Environment.NewLine);
    }
}
