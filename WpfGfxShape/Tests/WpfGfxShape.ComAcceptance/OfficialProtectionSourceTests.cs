using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

internal static class OfficialProtectionSourceTests
{
    internal static async Task<int> RunAsync(string revision)
    {
        string root = Path.Combine(AcceptancePaths.Results, "official-protection-source", revision);
        Directory.CreateDirectory(root);
        bool isCommit = revision.Length == 40 && revision.All(Uri.IsHexDigit);
        string url = "https://codeload.github.com/dotnet/wpf/zip/" + (isCommit ? revision : "refs/" + (revision == "main" ? "heads/main" : "tags/" + revision));
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var probes = new List<object>();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WpfGfxShape-Compatibility/1.0");
            string[] urls =
            [
                "https://api.github.com/repos/dotnet/wpf/commits/" + revision,
                "https://raw.githubusercontent.com/dotnet/wpf/" + revision + "/src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/WpfGfxSwitches.h",
                "https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json"
            ];
            foreach (string probeUrl in urls)
            {
                using HttpResponseMessage probe = await client.GetAsync(probeUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                string body = await probe.Content.ReadAsStringAsync().ConfigureAwait(false);
                string name = "probe-" + probes.Count + ".json";
                await File.WriteAllTextAsync(Path.Combine(root, name), body).ConfigureAwait(false);
                probes.Add(new { Url = probeUrl, StatusCode = (int)probe.StatusCode, probe.ReasonPhrase, BodyFile = name });
            }
            await File.WriteAllTextAsync(Path.Combine(root, "source-evidence.json"), JsonSerializer.Serialize(new { Url = url, StatusCode = (int)response.StatusCode, response.ReasonPhrase, Probes = probes }, new JsonSerializerOptions { WriteIndented = true })).ConfigureAwait(false);
            return 1;
        }
        byte[] archive = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        using var memory = new MemoryStream(archive);
        using var zip = new ZipArchive(memory, ZipArchiveMode.Read);
        var matches = new List<object>();
        foreach (ZipArchiveEntry entry in zip.Entries)
        {
            if (!(entry.FullName.EndsWith(".cpp", StringComparison.OrdinalIgnoreCase) || entry.FullName.EndsWith(".h", StringComparison.OrdinalIgnoreCase) || entry.FullName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))) continue;
            using var reader = new StreamReader(entry.Open());
            string source = await reader.ReadToEndAsync();
            if (!source.Contains("BoundsCheckProtection", StringComparison.OrdinalIgnoreCase)) continue;
            string relative = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
            string destination = Path.Combine(root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            await File.WriteAllTextAsync(destination, source);
            matches.Add(new { entry.FullName, Lines = source.Split('\n').Select((line, index) => new { Line = index + 1, Text = line }).Where(line => line.Text.Contains("BoundsCheckProtection", StringComparison.OrdinalIgnoreCase)).ToArray() });
        }
        await File.WriteAllTextAsync(Path.Combine(root, "source-evidence.json"), JsonSerializer.Serialize(new { Url = url, ArchiveSha256 = Convert.ToHexString(SHA256.HashData(archive)), Matches = matches }, new JsonSerializerOptions { WriteIndented = true }));
        return matches.Count == 0 ? 1 : 0;
    }
}
