using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

string root = AppContext.BaseDirectory;
string evidence = Path.Combine(root, "Evidence");
Directory.CreateDirectory(evidence);
bool parent = args.Length == 0;
string working = parent ? root : Path.Combine(root, "Package");
string executable = Path.Combine(working, parent ? "CandidateResizeDesktopSupervisor.exe" : "CandidateResizeDesktop.exe");
string name = parent ? "parent" : "desktop";
if (!parent)
{
    string native = Path.Combine(working, "Native", "wpfgfx_cor3.dll");
    string sha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(native)));
    if (sha != "E158C6277E3B3043E25A98D808C9D7B5ACB81CFE0DE8EBE5D3187B8381C1073B" || sha != File.ReadAllText(Path.Combine(working, "aot.sha256")).Trim() || File.Exists(Path.Combine(working, "wpfgfx_cor3.dll"))) return 65;
    File.WriteAllText(Path.Combine(evidence, "identity.json"), JsonSerializer.Serialize(new { nativeSha = sha, harnessSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(working, "CandidateResizeDesktop.dll")))), fixtureSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(working, "WpfGfxShape.ComAcceptance.dll")))) }));
}
using var child = new Process { StartInfo = new ProcessStartInfo(executable) { WorkingDirectory = working, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true } };
if (!parent) child.StartInfo.Environment["WPFGFX_CANDIDATE_DLL"] = Path.Combine(working, "Native", "wpfgfx_cor3.dll");
foreach (string argument in parent ? new[] { "run" } : new[] { "candidate", "desktop-resize" }) child.StartInfo.ArgumentList.Add(argument);
child.Start();
var stdout = child.StandardOutput.ReadToEndAsync();
var stderr = child.StandardError.ReadToEndAsync();
bool timeout = !child.WaitForExit(parent ? 170000 : 120000);
string? killError = null;
if (timeout)
{
    try { child.Kill(true); }
    catch (InvalidOperationException error) { killError = error.ToString(); }
    catch (System.ComponentModel.Win32Exception error) { killError = error.ToString(); }
}
bool reaped = child.WaitForExit(10000);
bool drained = Task.WaitAll([stdout, stderr], 10000);
if (drained)
{
    File.WriteAllText(Path.Combine(evidence, name + ".stdout.txt"), stdout.Result);
    File.WriteAllText(Path.Combine(evidence, name + ".stderr.txt"), stderr.Result);
}
int? exitCode = reaped ? child.ExitCode : null;
File.WriteAllText(Path.Combine(evidence, name + "-exit.json"), JsonSerializer.Serialize(new { pid = child.Id, timeout, reaped, drained, killError, exitCode }));
return !timeout && reaped && drained && killError is null ? exitCode!.Value : 124;
