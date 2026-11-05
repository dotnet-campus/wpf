using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;

internal static class InitializationClosure
{
    internal static void Write(string package, string evidence)
    {
        var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!).ToDictionary(o => unchecked((ushort)o.Value));
        var assemblies = new List<object>();
        foreach (string name in new[] { "PresentationCore.dll", "DirectWriteForwarder.dll" })
        {
            string path = Path.Combine(package, name);
            using var pe = new PEReader(File.OpenRead(path));
            var md = pe.GetMetadataReader();
            var pending = new Queue<MethodDefinitionHandle>();
            var visited = new HashSet<MethodDefinitionHandle>();
            foreach (var th in md.TypeDefinitions)
            {
                var type = md.GetTypeDefinition(th);
                string tn = md.GetString(type.Name);
                foreach (var mh in type.GetMethods())
                {
                    string mn = md.GetString(md.GetMethodDefinition(mh).Name);
                    if ((tn == "<Module>" && mn == ".cctor") || tn.Contains("DWriteLoader") || tn.Contains("NativeWPFDLLLoader") || tn.Contains("ModuleInitializer")) pending.Enqueue(mh);
                }
            }
            string Describe(EntityHandle handle)
            {
                if (handle.Kind == HandleKind.MethodDefinition) { var m = md.GetMethodDefinition((MethodDefinitionHandle)handle); var t = md.GetTypeDefinition(m.GetDeclaringType()); return md.GetString(t.Namespace) + "." + md.GetString(t.Name) + "::" + md.GetString(m.Name); }
                if (handle.Kind == HandleKind.MemberReference) { var m = md.GetMemberReference((MemberReferenceHandle)handle); string owner = m.Parent.Kind == HandleKind.TypeReference ? md.GetString(md.GetTypeReference((TypeReferenceHandle)m.Parent).Namespace) + "." + md.GetString(md.GetTypeReference((TypeReferenceHandle)m.Parent).Name) : m.Parent.Kind.ToString(); return owner + "::" + md.GetString(m.Name); }
                return handle.Kind.ToString();
            }
            var methods = new List<object>();
            while (pending.TryDequeue(out var mh))
            {
                if (!visited.Add(mh)) continue;
                var m = md.GetMethodDefinition(mh);
                var import = m.GetImport();
                var instructions = new List<object>();
                if (m.RelativeVirtualAddress != 0 && (m.ImplAttributes & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.IL)
                {
                    byte[] il = pe.GetMethodBody(m.RelativeVirtualAddress).GetILBytes()!;
                    for (int offset = 0; offset < il.Length;)
                    {
                        int start = offset;
                        ushort value = il[offset++]; if (value == 0xfe) value = (ushort)(0xfe00 | il[offset++]);
                        var op = opcodes[value];
                        int length = op.OperandType switch { OperandType.InlineNone => 0, OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1, OperandType.InlineVar => 2, OperandType.InlineI8 or OperandType.InlineR => 8, OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, offset), _ => 4 };
                        string? operand = length == 0 ? null : Convert.ToHexString(il.AsSpan(offset, length));
                        if (op.OperandType == OperandType.InlineString) operand = md.GetUserString(MetadataTokens.UserStringHandle(BitConverter.ToInt32(il, offset) & 0xffffff));
                        if (op.OperandType == OperandType.InlineMethod)
                        {
                            var target = MetadataTokens.EntityHandle(BitConverter.ToInt32(il, offset)); operand = Describe(target);
                            if (target.Kind == HandleKind.MethodDefinition) pending.Enqueue((MethodDefinitionHandle)target);
                        }
                        instructions.Add(new { offset = start, opcode = op.Name, operand }); offset += length;
                    }
                }
                methods.Add(new { token = $"0x{MetadataTokens.GetToken(mh):X8}", method = Describe(mh), rva = m.RelativeVirtualAddress, library = import.Module.IsNil ? null : md.GetString(md.GetModuleReference(import.Module).Name), entry = import.Module.IsNil ? null : md.GetString(import.Name), instructions });
            }
            assemblies.Add(new { name, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), methods });
        }
        File.WriteAllText(Path.Combine(evidence, "initialization-closure.json"), JsonSerializer.Serialize(assemblies, new JsonSerializerOptions { WriteIndented = true }));
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(assemblies));
        File.WriteAllLines(Path.Combine(evidence, "initialization-summary.txt"), document.RootElement.EnumerateArray().SelectMany(a => a.GetProperty("methods").EnumerateArray().Select(m => a.GetProperty("name").GetString() + " " + m.GetProperty("token").GetString() + " " + m.GetProperty("method").GetString() + " import=" + m.GetProperty("library").GetString() + ":" + m.GetProperty("entry").GetString() + " | " + string.Join("; ", m.GetProperty("instructions").EnumerateArray().Where(i => i.GetProperty("opcode").GetString() is "call" or "callvirt" or "calli" or "ldstr" or "ldftn").Select(i => i.GetProperty("opcode").GetString() + " " + i.GetProperty("operand").GetString())))));
    }
}
