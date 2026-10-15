using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class IndependentManagedCoreTests
{
    /// <summary>Measures frozen managed fields independently of native compiler expressions, preserving explicit offsets and pack.</summary>
    [TestMethod]
    public void WhenFrozenManagedCoreIsMeasuredThenFixedWidthFieldsMatchBothNativeTargets()
    {
        string root = typeof(IndependentManagedCoreTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string sourcePath = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.cs");
        var syntax = CSharpSyntaxTree.ParseText(File.ReadAllText(sourcePath)).GetRoot();
        var declarations = syntax.DescendantNodes().OfType<StructDeclarationSyntax>().ToArray();
        var enums = syntax.DescendantNodes().OfType<EnumDeclarationSyntax>().ToList();
        var inputs = new List<string> { sourcePath };
        var dependencyDeclarations = new List<StructDeclarationSyntax>();
        foreach (string name in new[] { "Point", "Size" })
        {
            string path = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Generated", name + ".cs");
            inputs.Add(path);
            var declaration = CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>().Single();
            dependencyDeclarations.Add(declaration.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(declaration.Members.OfType<FieldDeclarationSyntax>().Where(f => !f.Modifiers.Any(SyntaxKind.StaticKeyword)))).WithBaseList(null));
        }
        foreach (string name in new[] { "EdgeMode", "BitmapScalingMode", "ClearTypeHint", "TextRenderingMode", "TextHintingMode" })
        {
            string path = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/Generated", name + ".cs");
            inputs.Add(path);
            enums.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>().Single());
        }
        dependencyDeclarations = dependencyDeclarations.Select(d => d.WithAttributeLists(SyntaxFactory.List(d.AttributeLists.Where(a => a.ToString().Contains("StructLayout", StringComparison.Ordinal))))).ToList();
        string code = "using System; using System.Runtime.InteropServices; namespace LayoutEvidence {\n" +
            string.Join("\n", enums.Select(e => e.ToFullString())) + "\n" +
            string.Join("\n", declarations.Concat(dependencyDeclarations).Select(s => s.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(s.Members.OfType<FieldDeclarationSyntax>().Where(f => !f.Modifiers.Any(SyntaxKind.StaticKeyword)))).WithBaseList(null).ToFullString())) + "\n}";
        code = System.Text.RegularExpressions.Regex.Replace(code, @"^\s*#.*$", "", System.Text.RegularExpressions.RegexOptions.Multiline);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("CoreLayout_" + Guid.NewGuid().ToString("N"), [CSharpSyntaxTree.ParseText(code)], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var binary = new MemoryStream();
        var emitted = compilation.Emit(binary);
        Assert.IsTrue(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));
        binary.Position = 0;
        var context = new AssemblyLoadContext("CoreLayout", isCollectible: true);
        var assembly = context.LoadFromStream(binary);
        var maps = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["MIL_PEN_DATA"] = "MilPenData", ["MIL_GRADIENTSTOP"] = "MilGradientStop", ["MIL_PATHGEOMETRY"] = "MilPathGeometry", ["MIL_PATHFIGURE"] = "MilPathFigure",
            ["MIL_SEGMENT"] = "MilSegment", ["MIL_SEGMENT_LINE"] = "MilSegmentLine", ["MIL_SEGMENT_BEZIER"] = "MilSegmentBezier",
            ["MIL_SEGMENT_QUADRATICBEZIER"] = "MilSegmentQuadraticBezier", ["MIL_SEGMENT_ARC"] = "MilSegmentArc", ["MIL_SEGMENT_POLY"] = "MilSegmentPoly"
        };
        var errors = new List<string>();
        var measurements = new List<object>();
        var gaps = new List<string>();
        var enumMeasurements = new List<object>();
        var derivations = new List<object>();
        var reviewedDifferences = new List<object>();
        foreach (string architecture in new[] { "Win32", "x64" })
        {
            string report = Directory.EnumerateDirectories(Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence"), "native-core-" + architecture + "-*").Select(p => Path.Combine(p, "report.json")).MaxBy(File.GetLastWriteTimeUtc)!;
            using var document = JsonDocument.Parse(File.ReadAllText(report));
            var values = document.RootElement.GetProperty("values");
            foreach (var declaration in enums)
            {
                string name = declaration.Identifier.ValueText;
                Type type = assembly.GetType("LayoutEvidence." + name, throwOnError: true)!;
                string native = name switch
                {
                    "MIL_SEGMENT_TYPE" => "MilSegmentType", "MIL_PEN_CAP" => "MilPenCap", "MIL_PEN_JOIN" => "MilPenJoin",
                    "MILTransparencyFlags" => "MilTransparency", "MILRTInitializationFlags" => "MilRTInitialization",
                    "MILCoreSegFlags" => "MilCoreSeg", "MIL_PRESENTATION_RESULTS" => "MilPresentationResults", "MILWindowLayerType" => "MilWindowLayerType",
                    "MilRenderOptionFlags" => "MilRenderOptionFlags", _ => name.StartsWith("Mil", StringComparison.Ordinal) || name.StartsWith("MIL", StringComparison.Ordinal) ? name : "Mil" + name
                };
                string widthKey = "enum:" + native + ".width";
                if (!values.TryGetProperty(widthKey, out var width)) { gaps.Add(architecture + ": enum absent: " + native); continue; }
                int managedWidth = Derive(Enum.GetUnderlyingType(type)).Size;
                if (width.GetInt32() != managedWidth)
                {
                    reviewedDifferences.Add(new { architecture, name, kind = "EnumWidth", managed = managedWidth, native = width.GetInt32(), satisfied = false });
                    if (name != "MilGlyphRun" || managedWidth != 2 || width.GetInt32() != 4) errors.Add(architecture + ": unexpected enum width mismatch: " + name);
                }
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    string member = field.Name;
                    if (name == "MILRTInitializationFlags")
                    {
                        member = member switch
                        {
                            "MIL_RT_INITIALIZE_DEFAULT" => "Default", "MIL_RT_SOFTWARE_ONLY" => "SoftwareOnly",
                            "MIL_RT_HARDWARE_ONLY" => "HardwareOnly", "MIL_RT_NULL" => "Null", "MIL_RT_TYPE_MASK" => "TypeMask",
                            "MIL_RT_PRESENT_IMMEDIATELY" => "PresentImmediately", "MIL_RT_PRESENT_RETAIN_CONTENTS" => "PresentRetainContents",
                            "MIL_RT_NEED_DESTINATION_ALPHA" => "NeedDestinationAlpha", "MIL_RT_SINGLE_THREADED_USAGE" => "SingleThreadedUsage",
                            "MIL_RT_RENDER_NONCLIENT" => "RenderNonClient", "MIL_RT_DISABLE_DISPLAY_CLIPPING" => "DisableDisplayClipping",
                            "MIL_RT_DISABLE_MULTIMON_DISPLAY_CLIPPING" => "DisableMultimonDisplayClipping",
                            "MIL_RT_IS_DISABLE_MULTIMON_DISPLAY_CLIPPING_VALID" => "IsDisableMultimonDisplayClippingValid",
                            "MIL_RT_USE_REF_RAST" => "UseRefRast", "MIL_RT_USE_RGB_RAST" => "UseRgbRast",
                            "MIL_RT_PRESENT_USING_MASK" => "PresentUsingMask", "MIL_RT_PRESENT_USING_HAL" => "PresentUsingHal",
                            "MIL_RT_PRESENT_USING_BITBLT" => "PresentUsingBitBlt", "MIL_RT_PRESENT_USING_ALPHABLEND" => "PresentUsingAlphaBlend",
                            "MIL_RT_PRESENT_USING_ULW" => "PresentUsingUpdateLayeredWindow", "FORCE_DWORD" => "FORCE_DWORD",
                            _ => string.Empty
                        };
                        if (member.Length == 0)
                        {
                            reviewedDifferences.Add(new { architecture, name, field = field.Name, kind = "HistoricalCompatibilityMemberWithoutCurrentNativeAlias", satisfied = false });
                            gaps.Add(architecture + ": historical RT member requires authority: " + field.Name);
                            continue;
                        }
                    }
                    if (member == "FORCE_WORD")
                    {
                        reviewedDifferences.Add(new { architecture, name, field = field.Name, kind = "ManagedWidthSentinelNotNativeFORCE_DWORD", managed = unchecked((uint)Convert.ToInt64(field.GetRawConstantValue())), satisfied = false });
                        gaps.Add(architecture + ": managed width sentinel requires separate wire evidence: " + name + "." + member);
                        continue;
                    }
                    if (member.EndsWith("FORCE_DWORD", StringComparison.Ordinal)) member = "FORCE_DWORD";
                    else if (name is "MIL_PEN_CAP" or "MIL_PEN_JOIN") member = member[native.Length..];
                    else if (name == "MIL_SEGMENT_TYPE" && member.StartsWith("MilSegment", StringComparison.Ordinal)) member = member["MilSegment".Length..];
                    string key = "enum:" + native + "." + member;
                    if (!values.TryGetProperty(key, out var expected)) { gaps.Add(architecture + ": enum member absent: " + key); continue; }
                    uint actual = unchecked((uint)Convert.ToInt64(field.GetRawConstantValue()));
                    if (actual != expected.GetUInt32()) errors.Add(architecture + ": enum value mismatch: " + key);
                    enumMeasurements.Add(new { architecture, managed = name, field = field.Name, native = key, managedWidth, actual, expected = expected.GetUInt32() });
                }
            }
            foreach (var declaration in declarations)
            {
                string name = declaration.Identifier.ValueText;
                string native = maps.GetValueOrDefault(name, name);
                Type type = assembly.GetType("LayoutEvidence." + name, throwOnError: true)!;
                int size = Marshal.SizeOf(type);
                var derived = Derive(type);
                if (size != derived.Size) errors.Add("Independent fixed-width size mismatch: " + name);
                derivations.Add(new { architecture = "X86", managed = name, derived.Size, derived.Alignment, derived.Offsets, rule = "Recursive primitive widths, enum underlying width, sequential pack and explicit FieldOffset; no X86 runtime execution" });
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                bool fixedWidth = fields.All(f => IsFixedWidth(f.FieldType));
                if (!values.TryGetProperty(native + ".size", out var nativeSize)) { gaps.Add(architecture + ": native measurement absent: " + native); continue; }
                if (size != nativeSize.GetInt32())
                {
                    reviewedDifferences.Add(new { architecture, name, kind = "StructSize", managed = size, native = nativeSize.GetInt32(), satisfied = false });
                    if (name != "MIL_PEN_DATA" || size != 44 || nativeSize.GetInt32() != 48) errors.Add(architecture + ": unexpected size mismatch: " + name);
                }
                var offsets = new List<object>();
                foreach (var field in fields)
                {
                    int offset = Marshal.OffsetOf(type, field.Name).ToInt32();
                    if (offset != derived.Offsets[field.Name]) errors.Add("Independent fixed-width offset mismatch: " + name + "." + field.Name);
                    string nativeField = field.Name;
                    if (name is "MilRectD" or "MilRectF") nativeField = field.Name.TrimStart('_');
                    else if (name != "D3DMATRIX" && field.Name.StartsWith('_'))
                    {
                        nativeField = field.Name.TrimStart('_');
                        nativeField = char.ToUpperInvariant(nativeField[0]) + nativeField[1..];
                    }
                    string key = native + "." + nativeField;
                    if (!values.TryGetProperty(key, out var expected)) { gaps.Add(architecture + ": field measurement absent: " + key); continue; }
                    if (offset != expected.GetInt32()) errors.Add(architecture + ": offset mismatch: " + key);
                    offsets.Add(new { field = field.Name, type = field.FieldType.Name, offset, nativeOffset = expected.GetInt32() });
                }
                measurements.Add(new { architecture, managed = name, native, size, nativeSize = nativeSize.GetInt32(), layout = type.StructLayoutAttribute!.Value.ToString(), pack = type.StructLayoutAttribute.Pack, fixedWidth, x86RuntimeExecuted = false, x86Derivation = fixedWidth ? "Explicit offsets and recursively fixed-width primitive/enum/value fields; no IntPtr, references or architecture-sized members" : "Not proven", fields = offsets, nativeReport = Path.GetRelativePath(root, report).Replace('\\', '/') });
            }
        }
        string archive = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence", "independent-managed-core-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        File.WriteAllText(Path.Combine(archive, "projection.cs"), code);
        File.WriteAllText(Path.Combine(archive, "report.json"), JsonSerializer.Serialize(new { executed = true, actualArchitecture = RuntimeInformation.ProcessArchitecture.ToString(), rule = "Only methods and static fields removed from frozen managed declarations; instance field types and layout attributes retained. Point/Size instance fields from repository Generated sources, not invented stubs.", sources = inputs.Select(p => new { path = Path.GetRelativePath(root, p).Replace('\\', '/'), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))) }), managedStructCount = declarations.Length, managedEnumCount = enums.Count, enumComparisonExecuted = true, arm64CompilerExecuted = false, measurements, enumMeasurements, derivations, reviewedDifferences, gaps, errors, completeRequiredEvidenceSatisfied = false }, new JsonSerializerOptions { WriteIndented = true }));
        context.Unload();
        Console.WriteLine("Independent managed core: " + archive);
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors));
    }

    private static (int Size, int Alignment, Dictionary<string, int> Offsets) Derive(Type type)
    {
        if (type.IsEnum) return Derive(Enum.GetUnderlyingType(type));
        int primitive = type == typeof(byte) || type == typeof(sbyte) ? 1
            : type == typeof(short) || type == typeof(ushort) ? 2
            : type == typeof(int) || type == typeof(uint) || type == typeof(float) ? 4
            : type == typeof(long) || type == typeof(ulong) || type == typeof(double) ? 8 : 0;
        if (primitive != 0) return (primitive, primitive, new Dictionary<string, int>());
        if (!type.IsValueType || type == typeof(IntPtr) || type == typeof(UIntPtr)) throw new InvalidOperationException("Architecture-dependent type: " + type);
        var layout = type.StructLayoutAttribute ?? throw new InvalidOperationException("Missing layout: " + type);
        int pack = layout.Pack == 0 ? 8 : layout.Pack;
        int end = 0, alignment = 1;
        var offsets = new Dictionary<string, int>();
        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(f => f.MetadataToken))
        {
            var child = Derive(field.FieldType);
            int fieldAlignment = Math.Min(pack, child.Alignment);
            alignment = Math.Max(alignment, fieldAlignment);
            int offset = layout.Value == LayoutKind.Explicit
                ? field.GetCustomAttribute<FieldOffsetAttribute>()?.Value ?? throw new InvalidOperationException("Missing explicit offset: " + field)
                : (end + fieldAlignment - 1) / fieldAlignment * fieldAlignment;
            offsets.Add(field.Name, offset);
            end = Math.Max(end, offset + child.Size);
        }
        return (Math.Max(layout.Size, (end + alignment - 1) / alignment * alignment), alignment, offsets);
    }

    private static bool IsFixedWidth(Type type) => type.IsEnum || type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong) || type == typeof(float) || type == typeof(double) || (type.IsValueType && type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).All(f => IsFixedWidth(f.FieldType)));
}
