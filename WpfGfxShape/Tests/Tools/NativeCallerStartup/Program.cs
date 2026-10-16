using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

string root = AppContext.BaseDirectory;
var files = Directory.GetFiles(Path.Combine(root, "Input"), "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal);
var reports = new List<object>();
foreach (string path in files)
{
    byte[] bytes = File.ReadAllBytes(path);
    using var stream = new MemoryStream(bytes, false);
    using var pe = new PEReader(stream);
    PEHeader header = pe.PEHeaders.PEHeader ?? throw new BadImageFormatException(path);
    int Offset(int rva)
    {
        foreach (SectionHeader section in pe.PEHeaders.SectionHeaders)
        {
            int delta = rva - section.VirtualAddress;
            if (delta >= 0 && delta < section.SizeOfRawData)
                return checked(section.PointerToRawData + delta);
        }
        throw new BadImageFormatException($"Unmapped RVA {rva:X8}");
    }
    string Name(int rva)
    {
        int start = Offset(rva);
        int end = Array.IndexOf(bytes, (byte)0, start);
        if (end < 0) throw new BadImageFormatException(path);
        return Encoding.ASCII.GetString(bytes, start, end - start);
    }
    var imports = new List<string>();
    if (header.ImportTableDirectory.RelativeVirtualAddress != 0)
    {
        int offset = Offset(header.ImportTableDirectory.RelativeVirtualAddress);
        for (int size = 0; size + 20 <= header.ImportTableDirectory.Size; size += 20)
        {
            int nameRva = BitConverter.ToInt32(bytes, offset + size + 12);
            if (nameRva == 0) break;
            imports.Add(Name(nameRva));
        }
    }
    var delayImports = new List<string>();
    if (header.DelayImportTableDirectory.RelativeVirtualAddress != 0)
    {
        int offset = Offset(header.DelayImportTableDirectory.RelativeVirtualAddress);
        for (int size = 0; size + 32 <= header.DelayImportTableDirectory.Size; size += 32)
        {
            uint attributes = BitConverter.ToUInt32(bytes, offset + size);
            uint name = BitConverter.ToUInt32(bytes, offset + size + 4);
            if (name == 0) break;
            int rva = (attributes & 1) != 0 ? checked((int)name) : checked((int)(name - header.ImageBase));
            delayImports.Add(Name(rva));
        }
    }
    reports.Add(new
    {
        File = Path.GetRelativePath(root, path),
        Sha256 = Convert.ToHexString(SHA256.HashData(bytes)),
        Machine = pe.PEHeaders.CoffHeader.Machine.ToString(),
        Magic = header.Magic.ToString(),
        Imports = imports,
        DelayImports = delayImports,
        DebugCrt = imports.Concat(delayImports).Where(name => name.Equals("MSVCP140D.dll", StringComparison.OrdinalIgnoreCase)
            || name.Equals("VCRUNTIME140D.dll", StringComparison.OrdinalIgnoreCase)
            || name.Equals("ucrtbased.dll", StringComparison.OrdinalIgnoreCase)).ToArray()
    });
}
Directory.CreateDirectory(Path.Combine(root, "Results"));
string json = JsonSerializer.Serialize(new { RunUtc = DateTimeOffset.UtcNow, Files = reports }, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(Path.Combine(root, "Results", "pe-imports.json"), json);
Console.WriteLine(json);
