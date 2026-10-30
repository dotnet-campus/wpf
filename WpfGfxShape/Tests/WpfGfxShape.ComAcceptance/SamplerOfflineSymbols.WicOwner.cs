#if WIC_OWNER_OFFLINE_SYMBOLS
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

internal static partial class SamplerOfflineSymbols
{
    internal static async Task<int> RunWicOwnerAsync(string directory)
    {
        string root = Path.GetFullPath(directory);
        Directory.CreateDirectory(root);
        using var output = new StreamWriter(Path.Combine(root, "wic-owner-symbols.txt")) { AutoFlush = true };
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(250));
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(100) };
        using var input = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "inputs", "wic-final-release-owner.json")));
        output.WriteLine($"BusinessRuns=0;NewCaptures=0;StackDecoded=False;Input=taskd3bc29c5365549969765b406ea5ee635;Runtime={RuntimeInformation.FrameworkDescription};Arch={RuntimeInformation.ProcessArchitecture};DiagnosticMvid={typeof(NativeRun).Module.ModuleVersionId}");
        string candidate = Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll");
        string candidateSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(candidate)));
        output.WriteLine($"CandidateSHA256={candidateSha}");
        if (candidateSha != "B07C9073F75278ED61C44638EB3644547B413A281E2859B25211CEAAC3D1466C") return 7;
        int unknown = 0;
        foreach (string name in new[] { "wpfgfx_cor3.dll", "windowscodecs.dll" })
        {
            try
            {
                var original = input.RootElement.GetProperty("modules").EnumerateArray().Single(m => string.Equals(Path.GetFileName(m.GetProperty("path").GetString()), name, StringComparison.OrdinalIgnoreCase));
                string expected = original.GetProperty("sha256").GetString() ?? throw new InvalidDataException("Missing SHA256.");
                string image = name == "wpfgfx_cor3.dll" ? Path.Combine(AppContext.BaseDirectory, "inputs", name) : Path.Combine(Environment.SystemDirectory, name);
                using var stream = File.OpenRead(image);
                string sha = Convert.ToHexString(SHA256.HashData(stream));
                output.WriteLine($"Module={name};ActualImage={image};ExpectedSHA256={expected};ActualSHA256={sha};OriginalBase={original.GetProperty("baseAddress")};OriginalSize={original.GetProperty("size")}");
                if (sha != expected) throw new InvalidDataException("Exact original image SHA256 mismatch; no version substitution permitted.");
                stream.Position = 0;
                using var pe = new PEReader(stream);
                var header = pe.PEHeaders.PEHeader ?? throw new InvalidDataException("Missing PE header.");
                var cv = pe.ReadCodeViewDebugDirectoryData(pe.ReadDebugDirectory().Single(e => e.Type == DebugDirectoryEntryType.CodeView));
                string pdbName = Path.GetFileName(cv.Path);
                string key = cv.Guid.ToString("N").ToUpperInvariant() + cv.Age.ToString("X", CultureInfo.InvariantCulture);
                string cache = Path.Combine(root, name);
                Directory.CreateDirectory(cache);
                string pdbPath = Path.Combine(cache, pdbName);
                string url = $"https://msdl.microsoft.com/download/symbols/{Uri.EscapeDataString(pdbName)}/{key}/{Uri.EscapeDataString(pdbName)}";
                output.WriteLine($"ImageAuthenticated=True;RSDS={cv.Guid:D};Age={cv.Age};PdbName={pdbName};Machine={pe.PEHeaders.CoffHeader.Machine};TimeDateStamp=0x{pe.PEHeaders.CoffHeader.TimeDateStamp:X};SizeOfImage={header.SizeOfImage};PdbUrl={url}");
                using (var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false))
                {
                    output.WriteLine($"HttpStatus={(int)response.StatusCode};ContentLength={response.Content.Headers.ContentLength}");
                    response.EnsureSuccessStatusCode();
                    if (response.Content.Headers.ContentLength > 96 * 1024 * 1024) throw new InvalidDataException("PDB exceeds bounded download size.");
                    await using var source = await response.Content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false);
                    await using var destination = File.Create(pdbPath);
                    byte[] buffer = new byte[65536];
                    int total = 0, count;
                    while ((count = await source.ReadAsync(buffer, timeout.Token).ConfigureAwait(false)) != 0)
                    {
                        total = checked(total + count);
                        if (total > 96 * 1024 * 1024) throw new InvalidDataException("PDB exceeds bounded download size.");
                        await destination.WriteAsync(buffer.AsMemory(0, count), timeout.Token).ConfigureAwait(false);
                    }
                }
                byte[] bytes = File.ReadAllBytes(pdbPath);
                var msf = new Msf(bytes, output);
                byte[] info = msf.Stream(1);
                if (info.Length < 28) throw new InvalidDataException("PDB info truncated.");
                Guid guid = new(info.AsSpan(12, 16));
                uint age = U32(info, 8);
                output.WriteLine($"PdbSHA256={Convert.ToHexString(SHA256.HashData(bytes))};PdbBytes={bytes.Length};MSFGuid={guid:D};MSFAge={age};MatchesRSDS={guid == cv.Guid && age == cv.Age}");
                if (guid != cv.Guid || age != cv.Age) throw new InvalidDataException("PDB GUID/age mismatch.");
                File.Copy(image, Path.Combine(cache, name), true);
                var manifest = new { originalTask = "d3bc29c5365549969765b406ea5ee635", name, fileSHA256 = sha, rsdsGuid = cv.Guid, rsdsAge = cv.Age, pdbSHA256 = Convert.ToHexString(SHA256.HashData(bytes)), frames = input.RootElement.GetProperty("frames").EnumerateArray().Where(f => f.GetProperty("modulePath").ValueKind == JsonValueKind.String && string.Equals(Path.GetFileName(f.GetProperty("modulePath").GetString()), name, StringComparison.OrdinalIgnoreCase)).Select(f => f.Clone()).ToArray() };
                File.WriteAllText(Path.Combine(cache, "authenticated-input.json"), JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
                nint process = (nint)0x632F0077;
                SymSetOptions(0x4 | 0x10 | 0x200 | 0x80000);
                if (!SymInitialize(process, cache, false)) throw new InvalidOperationException($"SymInitialize:{Marshal.GetLastWin32Error()}");
                try
                {
                    const ulong loadBase = 0x10000000;
                    ulong loaded = SymLoadModuleEx(process, stream.SafeFileHandle.DangerousGetHandle(), image, name, loadBase, (uint)header.SizeOfImage, 0, 0);
                    output.WriteLine($"SymLoadModuleEx=0x{loaded:X};LastError={Marshal.GetLastWin32Error()}");
                    foreach (var frame in manifest.frames)
                    {
                        timeout.Token.ThrowIfCancellationRequested();
                        string rvaText = frame.GetProperty("rva").GetString() ?? throw new InvalidDataException("Missing RVA.");
                        uint rva = uint.Parse(rvaText.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        if (rva >= header.SizeOfImage) throw new InvalidDataException("RVA outside image.");
                        nint symbol = Marshal.AllocHGlobal(4096);
                        try
                        {
                            Marshal.Copy(new byte[4096], 0, symbol, 4096);
                            Marshal.WriteInt32(symbol, 88);
                            Marshal.WriteInt32(symbol, 80, 4000);
                            bool found = SymFromAddr(process, loadBase + rva, out ulong delta, symbol);
                            int error = Marshal.GetLastWin32Error();
                            int length = Marshal.ReadInt32(symbol, 76);
                            string? symbolName = found && length >= 0 && length <= 4000 ? Marshal.PtrToStringAnsi(symbol + 84, length) : null;
                            var module = ReadModule(process, loadBase, $"Frame{frame.GetProperty("index")}", output);
                            bool authenticated = loaded == loadBase && module.PdbSig70 == guid && module.PdbAge == age && !module.PdbUnmatched && module.ImageSize == header.SizeOfImage && module.MachineType == 0x14c;
                            var line = new LineInfo { SizeOfStruct = (uint)Marshal.SizeOf<LineInfo>() };
                            bool hasLine = authenticated && SymGetLineFromAddr64(process, loadBase + rva, out _, ref line);
                            output.WriteLine($"Frame={frame.GetProperty("index")};Module={name};RVA={rvaText};Result={(authenticated && found && symbolName != null ? "MatchedPdbSymbol" : "Unknown")};Name={(authenticated ? symbolName : null)};Offset=0x{delta:X};SymError={error};SymbolSize={Marshal.ReadInt32(symbol, 28)};Source={(hasLine ? Marshal.PtrToStringAnsi(line.FileName) : null)};Line={(hasLine ? line.LineNumber : 0)};UnwindComplete=False;NearestSymbolNotFunctionExtentProof=True");
                            if (!authenticated || !found || symbolName == null) unknown++;
                        }
                        finally { Marshal.FreeHGlobal(symbol); }
                    }
                }
                finally { output.WriteLine($"SymCleanup={SymCleanup(process)};LastError={Marshal.GetLastWin32Error()}"); }
            }
            catch (Exception error) when (error is IOException or HttpRequestException or OperationCanceledException or InvalidOperationException or UnauthorizedAccessException or ArgumentException or OverflowException)
            {
                unknown++;
                output.WriteLine($"Module={name};Result=Unknown;Error={error}");
            }
        }
        int exit = unknown == 0 ? 0 : 5;
        output.WriteLine($"ApplicationExit={exit};UnknownCount={unknown};BusinessRuns=0;NewCaptures=0;StackDecoded=False");
        File.WriteAllText(Path.Combine(root, "offline-exit.txt"), $"ApplicationExit={exit};CandidateSHA256={candidateSha};BusinessRuns=0;NewCaptures=0");
        return exit;
    }
}
#endif
