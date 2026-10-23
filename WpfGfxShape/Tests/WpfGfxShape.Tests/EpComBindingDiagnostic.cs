#if EP_COM_BINDING_READONLY
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Tests;

internal static class EpComBindingDiagnostic
{
    private static readonly Dictionary<short, OpCode> Codes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!).ToDictionary(c => c.Value);

    private static IEnumerable<(int Offset, OpCode Op, byte[] Operand)> Instructions(MethodBase method)
    {
        byte[] il = method.GetMethodBody()?.GetILAsByteArray() ?? [];
        for (int p = 0; p < il.Length;)
        {
            int offset = p;
            short key = il[p++] == 0xfe ? (short)(0xfe00 | il[p++]) : il[p - 1];
            OpCode op = Codes[key];
            int count = op.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineI or OperandType.ShortInlineVar or OperandType.ShortInlineBrTarget => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, p),
                _ => 4
            };
            yield return (offset, op, il.AsSpan(p, count).ToArray());
            p += count;
        }
    }

    private static int Compressed(byte[] bytes, ref int p)
    {
        int first = bytes[p++];
        if ((first & 0x80) == 0) return first;
        if ((first & 0xc0) == 0x80) return ((first & 0x3f) << 8) | bytes[p++];
        return ((first & 0x1f) << 24) | (bytes[p++] << 16) | (bytes[p++] << 8) | bytes[p++];
    }

    private static string SignatureType(Module module, byte[] bytes, ref int p)
    {
        int element = bytes[p++];
        if (element == 0x0f) return SignatureType(module, bytes, ref p) + "*";
        if (element == 0x10) return SignatureType(module, bytes, ref p) + "&";
        if (element == 0x15)
        {
            string generic = SignatureType(module, bytes, ref p);
            int count = Compressed(bytes, ref p);
            var arguments = new List<string>();
            for (int i = 0; i < count; i++) arguments.Add(SignatureType(module, bytes, ref p));
            return generic + "<" + string.Join(",", arguments) + ">";
        }
        if (element is 0x11 or 0x12)
        {
            int coded = Compressed(bytes, ref p);
            int table = (coded & 3) switch { 0 => 0x02000000, 1 => 0x01000000, 2 => 0x1b000000, _ => throw new BadImageFormatException() };
            return module.ResolveType(table | (coded >> 2)).FullName ?? "unnamed";
        }
        return element switch
        {
            1 => "void", 2 => "bool", 3 => "char", 4 => "int8", 5 => "uint8", 6 => "int16", 7 => "uint16",
            8 => "int32", 9 => "uint32", 10 => "int64", 11 => "uint64", 12 => "float32", 13 => "float64",
            0x18 => "native int", 0x19 => "native uint", _ => throw new NotSupportedException($"Signature element {element:X2}")
        };
    }

    private static void Identity(TextWriter output, Assembly assembly)
    {
        output.WriteLine($"Assembly={assembly.FullName}; Path={assembly.Location}; SHA256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location)))}; MVID={assembly.ManifestModule.ModuleVersionId}");
    }

    private static int Main()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(root);
        using var output = File.CreateText(Path.Combine(root, "ep-com-binding.txt"));
        using var summary = File.CreateText(Path.Combine(root, "ep-summary.txt"));
        output.WriteLine($"Runtime={RuntimeInformation.FrameworkDescription}; Architecture={RuntimeInformation.ProcessArchitecture}; PointerSize={IntPtr.Size}; OS={RuntimeInformation.OSDescription}");
        Assembly silk = typeof(IDirect3DDevice9).Assembly;
        Assembly product = Assembly.Load("wpfgfx_cor3");
        Identity(output, silk);
        Identity(output, typeof(EpComBindingDiagnostic).Assembly);
        Identity(output, product);
        var targets = new Dictionary<MethodBase, List<string>>();
        Type owner = product.GetType("WpfGfxShape.Abi.InteropDeviceBitmapExports", throwOnError: true)!;
        foreach (Type type in new[] { owner }.Concat(owner.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)))
        foreach (MethodInfo caller in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        foreach (var instruction in Instructions(caller).Where(i => i.Op.OperandType == OperandType.InlineMethod))
        {
            MethodBase? target = caller.Module.ResolveMethod(BitConverter.ToInt32(instruction.Operand), type.GetGenericArguments(), caller.GetGenericArguments());
            if (target?.DeclaringType?.Assembly != silk) continue;
            if (!targets.TryGetValue(target, out List<string>? callers)) targets.Add(target, callers = []);
            callers.Add($"{type.FullName}.{caller.Name}+IL_{instruction.Offset:X4}");
        }
        if (targets.Count == 0) throw new InvalidOperationException("No production Silk calls resolved");
        foreach (var item in targets.OrderBy(t => t.Key.DeclaringType!.FullName).ThenBy(t => t.Key.MetadataToken))
        {
            MethodBase method = item.Key;
            output.WriteLine($"METHOD {method.DeclaringType!.FullName}::{method}; Token={method.MetadataToken:X8}; ManagedConvention={method.CallingConvention}");
            output.WriteLine("ProductionCallers=" + string.Join(";", item.Value));
            summary.WriteLine($"{method.DeclaringType!.Name}::{method}; Token={method.MetadataToken:X8}; Callers={string.Join(";", item.Value)}");
            output.WriteLine("IL=" + Convert.ToHexString(method.GetMethodBody()?.GetILAsByteArray() ?? []));
            foreach (var instruction in Instructions(method))
            {
                output.WriteLine($"  {instruction.Offset:X4} {instruction.Op.Name} {Convert.ToHexString(instruction.Operand)}");
                if (instruction.Op.OperandType == OperandType.InlineSig)
                {
                    byte[] signature = method.Module.ResolveSignature(BitConverter.ToInt32(instruction.Operand));
                    int p = 0;
                    int convention = signature[p++];
                    int count = Compressed(signature, ref p);
                    string result = SignatureType(method.Module, signature, ref p);
                    var parameters = new List<string>();
                    for (int i = 0; i < count; i++) parameters.Add(SignatureType(method.Module, signature, ref p));
                    if (p != signature.Length) throw new BadImageFormatException("Trailing signature bytes");
                    summary.WriteLine($"  calli={Convert.ToHexString(signature)}; convention={convention:X2}; return={result}; explicit=[{string.Join(",", parameters)}]");
                    output.WriteLine($"  CALLI Raw={Convert.ToHexString(signature)}; ConventionByte={convention:X2}; Convention={(convention & 15) switch { 1 => "Cdecl", 2 => "Stdcall", 3 => "Thiscall", 4 => "Fastcall", _ => "Other" }}; HasThis={(convention & 0x20) != 0}; Return={result}; ExplicitParameters=[{string.Join(",", parameters)}]");
                }
            }
            output.WriteLine("ENDMETHOD");
        }
        foreach (Type type in new[] { typeof(SurfaceDesc), typeof(DeviceCreationParameters), typeof(Caps9), typeof(LockedRect), typeof(PresentParameters) })
        {
            output.WriteLine($"STRUCT {type.FullName}; Size={Marshal.SizeOf(type)}; Pack={type.StructLayoutAttribute?.Pack}");
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                output.WriteLine($"  {field.Name}; Offset={Marshal.OffsetOf(type, field.Name)}; Type={field.FieldType.FullName}");
        }
        output.WriteLine($"COMPLETE ProductionBoundMethods={targets.Count}; ReadOnlyJitMetadataNotNativeAotCausality=true");
        return 0;
    }
}
#endif
