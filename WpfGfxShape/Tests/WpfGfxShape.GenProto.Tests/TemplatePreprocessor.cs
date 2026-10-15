using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace WpfGfxShape.GenProto.Tests;

// Test-only port of include/gencode.pl; line references identify the original rules.
internal sealed class TemplatePreprocessor
{
    private readonly string includeRoot;
    private readonly string wcpSuffix;
    private readonly string milSuffix;
    private bool doingStruct;
    private bool doingEnum;
    private bool isTypedef;
    private bool doingPublic;
    private string enumPrefix = "";
    internal List<string> Inputs { get; } = [];
    internal List<object> LineMap { get; } = [];

    internal TemplatePreprocessor(string includeRoot, string wcpSuffix = "", string milSuffix = "")
    {
        this.includeRoot = includeRoot;
        this.wcpSuffix = wcpSuffix;
        this.milSuffix = milSuffix;
    }

    private string[] Read(string path)
    {
        Inputs.Add(path);
        string text = File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal);
        return Regex.Matches(text, "[^\\n]*\\n|[^\\n]+$").Select(m => m.Value).ToArray();
    }

    internal (string Cpp, string Cs) Generate(string template)
    {
        var cpp = new StringBuilder();
        var cs = new StringBuilder();
        string[] lines = Read(template);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            Match start = Regex.Match(line, @";begin +(CSHARP_ONLY|CPP_ONLY|COMMON)");
            if (start.Success)
            {
                string section = start.Groups[1].Value;
                bool ended = false;
                while (++i < lines.Length)
                {
                    line = lines[i];
                    if (line.Contains(";end", StringComparison.Ordinal)) { ended = true; break; }
                    int cppStart = cpp.Length;
                    int csStart = cs.Length;
                    WriteLine(line, section, cpp, cs);
                    LineMap.Add(new { input = template, line = i + 1, section, cppStart, cppLength = cpp.Length - cppStart, csStart, csLength = cs.Length - csStart });
                }
                if (!ended) throw new InvalidDataException("Unterminated template section: " + template);
            }
            else if (Regex.IsMatch(line, @";begin +PUBLIC")) doingPublic = true;
            else if (Regex.IsMatch(line, @";end +PUBLIC")) doingPublic = false;
            else if (line.Contains(";begin", StringComparison.Ordinal)) throw new InvalidDataException("Unsupported template section: " + line);
        }
        return (cpp.ToString(), cs.ToString());
    }

    private void WriteLine(string line, string section, StringBuilder cpp, StringBuilder cs)
    {
        bool common = section == "COMMON";
        StringBuilder output = section == "CSHARP_ONLY" ? cs : cpp;
        Match include = Regex.Match(line, @"^\s*;(include_header|include)\s+(\S+)$");
        if (include.Success)
        {
            string path = Path.Combine(includeRoot, include.Groups[2].Value.Replace('\\', Path.DirectorySeparatorChar));
            string[] content = Read(path);
            if (include.Groups[1].Value == "include")
            {
                foreach (string item in content)
                {
                    output.Append(item);
                    if (common) cs.Append(item);
                }
            }
            else
            {
                // gencode.pl:492-504 consumes and drops the first non-// line too.
                int first = 0;
                while (first < content.Length && content[first].StartsWith("//", StringComparison.Ordinal)) first++;
                for (int i = first + 1; i < content.Length; i++)
                {
                    if (content[i].Contains("#pragma once", StringComparison.Ordinal)) continue;
                    output.Append(content[i]);
                    if (common) cs.Append(Translate(content[i]));
                }
            }
        }
        else if (!common && line.Contains("WCP_VERSION_SUFFIX", StringComparison.Ordinal))
            output.Append(Regex.Replace(line, "(.*)WCP_VERSION_SUFFIX(.*)", "$1" + wcpSuffix + "$2"));
        else if (!common && line.Contains("MIL_VERSION_SUFFIX", StringComparison.Ordinal))
            output.Append(Regex.Replace(line, "(.*)MIL_VERSION_SUFFIX(.*)", "$1" + milSuffix + "$2"));
        else if (!line.StartsWith(';'))
        {
            output.Append(line);
            if (common) cs.Append(Translate(line));
        }
    }

    private string Translate(string line)
    {
        // gencode.pl:44-48; only these directives are stripped, not general CPP evaluation.
        if (line.StartsWith("#ifndef", StringComparison.Ordinal) || line.StartsWith("#endif", StringComparison.Ordinal)) return "";
        string exposure = doingPublic ? "public" : "internal";
        if (doingStruct)
        {
            if (line.Contains('}'))
            {
                if (isTypedef) { line = "};\n"; isTypedef = false; }
                doingStruct = false;
            }
            if (Regex.IsMatch(line, @"^\s*//") || !Regex.IsMatch(line, @"\w+")) return "    " + line;
            Match member = Regex.Match(line, "( *)(.*)");
            string body = member.Groups[2].Value;
            string prefix = "    " + member.Groups[1].Value + exposure + " ";
            Match pointer = Regex.Match(body, @"(.*)\*(.*)");
            if (pointer.Success) return prefix + "IntPtr " + pointer.Groups[2].Value + "\n";
            body = Regex.Replace(body, "::Enum", "", RegexOptions.None, TimeSpan.FromSeconds(1));
            body = Regex.Replace(body, "::Flags", "", RegexOptions.None, TimeSpan.FromSeconds(1));
            (string Native, string Managed)[] types =
            [
                ("UINT32", "UInt32"), ("ULONG", "UInt32"), ("UINT16", "UInt16"), ("BYTE", "Byte"),
                ("INT32", "Int32"), ("WMG_HANDLE64", "UInt64"), ("WMG_HANDLE", "UInt32"),
                ("HMIL_OBJECT", "UInt32"), ("HMIL_CONTEXT", "UInt32"), ("HMIL_RESOURCE", "UInt32"),
                ("HMIL_COMPNODE", "UInt32"), ("MIL_VERTEXFORMAT", "UInt32"), ("MILRTInitializationFlags", "UInt32"),
                ("MILGradientWrapMode", "UInt32"), ("MILBitmapWrapMode", "UInt32"), ("MILBitmapOptions", "UInt32"),
                ("GUID", "Guid"), ("HWND", "IntPtr"), ("HANDLE", "IntPtr"), ("PVOID", "IntPtr"),
                ("UINT", "UInt32"), ("DWORD", "UInt32"), ("WORD", "UInt16"), ("MilPixelFormat::Enum", "Int32"),
                ("BOOL", "Int32"), ("FLOAT", "float"), ("DOUBLE", "double")
            ];
            foreach (var type in types)
            {
                Match match = Regex.Match(body, "(.*)" + Regex.Escape(type.Native) + "(.*)");
                if (!match.Success) continue;
                body = match.Groups[1].Value + type.Managed + match.Groups[2].Value;
                break;
            }
            return prefix + body + "\n";
        }
        if (doingEnum)
        {
            line = Regex.Replace(line, @"(\s*)END_MIL(FLAG)?ENUM", "$1    FORCE_DWORD = unchecked((int)0xffffffff)\n    $1};");
            Match item = Regex.Match(line, @"(\W*)(\w+)([^=]+=\s*)([x0-9A-Fa-f]+)(.*)");
            if (item.Success)
            {
                string value = item.Groups[4].Value;
                string digits = value.StartsWith("0x", StringComparison.Ordinal) ? value[2..] : value;
                ulong numeric = ulong.Parse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                if (numeric >= 0x8000000) value = "unchecked((int)" + value + ")";
                return "    " + item.Groups[1].Value + enumPrefix + item.Groups[2].Value + item.Groups[3].Value + value + item.Groups[5].Value + "\n";
            }
            if (line.Contains('}'))
            {
                if (isTypedef) { line = "};\n"; isTypedef = false; }
                doingEnum = false;
                enumPrefix = "";
            }
            return "    " + line;
        }
        foreach (string kind in new[] { "enum", "struct" })
        {
            if (!Regex.IsMatch(line, kind + "( |\n)")) continue;
            string name = Regex.Match(line, kind + @"\s+(\w+)").Groups[1].Value;
            if (Regex.IsMatch(line, @"typedef\s+")) { name = Regex.Replace(name, "^_", ""); isTypedef = true; }
            if (kind == "enum") { doingEnum = true; return "    " + exposure + " enum " + name; }
            doingStruct = true;
            return "    [StructLayout(LayoutKind.Sequential, Pack=1)]\n    " + exposure + " struct " + name;
        }
        Match macro = Regex.Match(line, @"^(\s*)BEGIN_MIL(FLAG|)ENUM\s*\(\s*(\w+)\s*\)(?:\s*//\s*(.*))?");
        if (macro.Success)
        {
            enumPrefix = Regex.Match(macro.Groups[4].Value, @"CSPrefix:(\S+)").Groups[1].Value;
            string space = macro.Groups[1].Value;
            bool flags = macro.Groups[2].Value.Length != 0;
            doingEnum = true;
            return "    " + (flags ? space + "[System.Flags]\n    " : "") + space + exposure + " enum " + macro.Groups[3].Value + (flags ? "Flags" : "") + "\n    " + space + "{\n";
        }
        return "    " + line;
    }
}
