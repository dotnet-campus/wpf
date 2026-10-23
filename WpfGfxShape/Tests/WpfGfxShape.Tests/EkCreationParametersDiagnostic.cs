#if EK_ABI_READONLY
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Tests;

internal static unsafe class EkCreationParametersDiagnostic
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Expected
    {
        internal uint AdapterOrdinal, DeviceType;
        internal nint HFocusWindow;
        internal uint BehaviorFlags;
    }

    private static int Main()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(root);
        using var stream = File.Create(Path.Combine(root, "ek-abi.json"));
        using var json = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        Type device = typeof(IDirect3DDevice9);
        json.WriteStartObject();
        json.WriteString("Runtime", RuntimeInformation.FrameworkDescription);
        json.WriteString("Architecture", RuntimeInformation.ProcessArchitecture.ToString());
        json.WriteNumber("PointerSize", IntPtr.Size);
        json.WriteString("SilkAssembly", device.Assembly.FullName);
        json.WriteString("SilkPath", device.Assembly.Location);
        json.WriteString("SilkSha256", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(device.Assembly.Location))));
        json.WriteString("SilkMvid", device.Module.ModuleVersionId);
        json.WriteNumber("IndependentSize", sizeof(Expected));
        json.WriteNumber("SilkSize", sizeof(DeviceCreationParameters));
        json.WriteStartArray("IndependentFields");
        foreach (FieldInfo field in typeof(Expected).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            json.WriteStartObject();
            json.WriteString("Name", field.Name);
            json.WriteNumber("Offset", Marshal.OffsetOf<Expected>(field.Name).ToInt32());
            json.WriteString("Type", field.FieldType.FullName);
            json.WriteEndObject();
        }
        json.WriteEndArray();
        json.WriteStartArray("SilkFields");
        foreach (FieldInfo field in typeof(DeviceCreationParameters).GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            json.WriteStartObject();
            json.WriteString("Name", field.Name);
            json.WriteNumber("Offset", Marshal.OffsetOf<DeviceCreationParameters>(field.Name).ToInt32());
            json.WriteString("Type", field.FieldType.FullName);
            json.WriteEndObject();
        }
        json.WriteEndArray();
        Dictionary<short, OpCode> codes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!).ToDictionary(c => c.Value);
        json.WriteStartArray("Methods");
        foreach (MethodInfo method in device.GetMethods().Where(m => m.Name == "GetCreationParameters"))
        {
            byte[] il = method.GetMethodBody()?.GetILAsByteArray() ?? throw new InvalidOperationException("Missing IL");
            json.WriteStartObject();
            json.WriteString("Method", method.ToString());
            json.WriteString("ManagedCallingConvention", method.CallingConvention.ToString());
            json.WriteString("IL", Convert.ToHexString(il));
            json.WriteStartArray("Instructions");
            for (int p = 0; p < il.Length;)
            {
                int offset = p;
                short key = il[p++] == 0xfe ? (short)(0xfe00 | il[p++]) : il[p - 1];
                OpCode op = codes[key];
                int count = op.OperandType switch
                {
                    OperandType.InlineNone => 0,
                    OperandType.ShortInlineI or OperandType.ShortInlineVar or OperandType.ShortInlineBrTarget => 1,
                    OperandType.InlineVar => 2,
                    OperandType.InlineI8 or OperandType.InlineR => 8,
                    OperandType.InlineSwitch => throw new NotSupportedException("Switch IL"),
                    _ => 4
                };
                json.WriteStartObject();
                json.WriteNumber("Offset", offset);
                json.WriteString("Op", op.Name);
                json.WriteString("Operand", Convert.ToHexString(il.AsSpan(p, count)));
                if (op.OperandType == OperandType.InlineSig)
                    json.WriteString("CalliSignature", Convert.ToHexString(method.Module.ResolveSignature(BitConverter.ToInt32(il, p))));
                if (op.OperandType == OperandType.InlineMethod)
                    json.WriteString("Target", method.Module.ResolveMethod(BitConverter.ToInt32(il, p))?.ToString());
                p += count;
                json.WriteEndObject();
            }
            json.WriteEndArray();
            json.WriteEndObject();
        }
        json.WriteEndArray();
        json.WriteEndObject();
        return 0;
    }
}
#endif
