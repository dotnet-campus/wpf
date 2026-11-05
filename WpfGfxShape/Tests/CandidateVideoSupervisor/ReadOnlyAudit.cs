using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;

internal static class ReadOnlyAudit
{
    internal static void Write(string package, string evidence)
    {
        var results = new List<object>();
        foreach (string path in Directory.GetFiles(package, "*.dll", SearchOption.AllDirectories))
        {
            using var pe = new PEReader(File.OpenRead(path));
            var header = pe.PEHeaders.PEHeader ?? throw new BadImageFormatException(path);
            var imports = ReadImports(pe, header.ImportTableDirectory, false);
            var delayImports = ReadImports(pe, header.DelayImportTableDirectory, true);
            var methods = new List<object>();
            var references = new List<object>();
            if (pe.HasMetadata)
            {
                var metadata = pe.GetMetadataReader();
                foreach (var typeHandle in metadata.TypeDefinitions)
                {
                    var type = metadata.GetTypeDefinition(typeHandle);
                    string typeName = metadata.GetString(type.Namespace) + "." + metadata.GetString(type.Name);
                    foreach (var methodHandle in type.GetMethods())
                    {
                        var method = metadata.GetMethodDefinition(methodHandle);
                        string name = metadata.GetString(method.Name);
                        if (System.Reflection.Metadata.Ecma335.MetadataTokens.GetRowNumber(methodHandle) > 3 && name != ".cctor" && !typeName.Contains("Loader", StringComparison.OrdinalIgnoreCase) && !name.Contains("LoadLibrary", StringComparison.OrdinalIgnoreCase) && !name.Contains("Initialize", StringComparison.OrdinalIgnoreCase)) continue;
                        var import = method.GetImport();
                        methods.Add(new { typeName, name, token = System.Reflection.Metadata.Ecma335.MetadataTokens.GetToken(methodHandle), rva = method.RelativeVirtualAddress,
                            il = method.RelativeVirtualAddress == 0 ? null : Convert.ToHexString(pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes()!),
                            library = import.Module.IsNil ? null : metadata.GetString(metadata.GetModuleReference(import.Module).Name),
                            entry = import.Module.IsNil ? null : metadata.GetString(import.Name) });
                    }
                }
                foreach (var handle in metadata.MemberReferences)
                {
                    var member = metadata.GetMemberReference(handle);
                    string name = metadata.GetString(member.Name);
                    if (!name.Contains("Load", StringComparison.OrdinalIgnoreCase) && !name.Contains("Initialize", StringComparison.OrdinalIgnoreCase)) continue;
                    string? owner = null;
                    if (member.Parent.Kind == HandleKind.TypeReference)
                    {
                        var type = metadata.GetTypeReference((TypeReferenceHandle)member.Parent);
                        owner = metadata.GetString(type.Namespace) + "." + metadata.GetString(type.Name);
                    }
                    references.Add(new { token = System.Reflection.Metadata.Ecma335.MetadataTokens.GetToken(handle), owner, name });
                }
            }
            results.Add(new { path = Path.GetRelativePath(package, path), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), imports, delayImports, methods, references });
        }
        File.WriteAllText(Path.Combine(evidence, "pe-initialization-audit.json"), JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static List<object> ReadImports(PEReader pe, DirectoryEntry directory, bool delay)
    {
        var imports = new List<object>();
        if (directory.RelativeVirtualAddress == 0) return imports;
        int size = delay ? 32 : 20;
        var reader = pe.GetSectionData(directory.RelativeVirtualAddress).GetReader(0, directory.Size);
        while (reader.RemainingBytes >= size)
        {
            uint[] fields = new uint[size / 4];
            for (int i = 0; i < fields.Length; i++) fields[i] = reader.ReadUInt32();
            if (fields.All(value => value == 0)) break;
            uint name = fields[delay ? 1 : 3];
            if (delay && (fields[0] & 1) == 0)
                name = checked((uint)(name - pe.PEHeaders.PEHeader!.ImageBase));
            var text = pe.GetSectionData(checked((int)name)).GetReader();
            var bytes = new List<byte>();
            byte value;
            while ((value = text.ReadByte()) != 0) bytes.Add(value);
            imports.Add(new { library = System.Text.Encoding.ASCII.GetString(bytes.ToArray()), fields });
        }
        return imports;
    }
}
