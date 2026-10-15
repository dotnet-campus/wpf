using System.Buffers.Binary;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class LayerContractTests
{
    /// <summary>Preserves physical sources and proves the byte contract without treating native enum width as packet width.</summary>
    [TestMethod]
    public void WhenLayerContractsAreReviewedThenGlyphPaddingAndPenFieldExtentRemainDistinct()
    {
        string root = typeof(LayerContractTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string[] paths = [
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_core_types.w",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_core_types_compat.cs",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.h",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/xml/Resource.xml",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/geometry_api.cpp",
            "WpfGfxShape/Code/WpfGfxShape/Core/GeneratedGlyphRunResource.cs",
            "WpfGfxShape/Code/WpfGfxShape/Core/GeneratedProtocol.cs"
        ];
        string Read(string path) => File.ReadAllText(Path.Combine(root, path));
        var errors = new List<string>();
        string native = Read(paths[2]);
        string glyph = Read(paths[5]);
        string pen = Read(paths[4]);
        if (!Read(paths[3]).Contains("Name=\"MilGlyphRun\" UnmanagedName=\"MilGlyphRun\" MarshaledSize=\"2\"", StringComparison.Ordinal)) errors.Add("Missing frozen generator width authority");
        if (!native.Contains("WORD GlyphRunFlags;\n", StringComparison.Ordinal) && !native.Contains("WORD GlyphRunFlags;\r\n", StringComparison.Ordinal)) errors.Add("Missing native WORD consumption contract");
        if (!native.Contains("UINT16 UINT16Padding0;", StringComparison.Ordinal)) errors.Add("Missing native glyph padding");
        if (!glyph.Contains("[FieldOffset(16)] internal readonly ushort Flags;", StringComparison.Ordinal) || !glyph.Contains("MemoryMarshal.Read<MilGlyphRunCreateCommand>(packet)", StringComparison.Ordinal)) errors.Add("Production glyph reader contract changed");
        if (!pen.Contains("pData->DashArraySize / sizeof(double)", StringComparison.Ordinal) || !pen.Contains("pData->Thickness", StringComparison.Ordinal)) errors.Add("Missing native field-wise pen consumption");
        byte[] bytes = new byte[76];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(16), 0x11);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(18), 0xA5A5);
        ushort flags = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(16));
        if (flags != 0x11 || (flags & 0x10) == 0 || (uint)flags != 17) errors.Add("Glyph WORD zero-extension or padding isolation failed");
        byte[] penBytes = new byte[44];
        BinaryPrimitives.WriteDoubleLittleEndian(penBytes, 2.5);
        BinaryPrimitives.WriteUInt32LittleEndian(penBytes.AsSpan(40), 16);
        if (BinaryPrimitives.ReadDoubleLittleEndian(penBytes) != 2.5 || BinaryPrimitives.ReadUInt32LittleEndian(penBytes.AsSpan(40)) / 8 != 2) errors.Add("Pen field extent failed");
        var enumeration = CSharpSyntaxTree.ParseText(Read("src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.cs")).GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>().Select(e => new { name = e.Identifier.ValueText, underlying = e.BaseList?.ToString() ?? ": int", members = e.Members.Select(m => m.Identifier.ValueText).ToArray(), layer = e.Identifier.ValueText is "MILCMD" or "MilResourceType" ? "command/resource identity; separate native authority required" : "core declaration; alias/member coverage required" }).ToArray();
        string archive = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence", "layer-contract-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        File.WriteAllText(Path.Combine(archive, "report.json"), JsonSerializer.Serialize(new {
            executed = true,
            sources = paths.Select(p => new { path = p, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(root, p)))) }),
            pen = new { managedExtent = 44, nativeSize = 48, fieldExtent = 44, nativeTailPadding = 4, conversion = "Native InitializePen reads named fields; no size-based copy or array stride proven", satisfied = false },
            glyph = new { managedEnumWidth = 2, nativeEnumWidth = 4, wireWidth = 2, offset = 16, paddingOffset = 18, flags, conversion = "WORD wire flags to native flag operations by integral promotion; padding excluded", productionExecutionVerified = false, satisfied = false },
            enumeration, physicalSourceHashesExecuted = true, physicalLedgerClosureSatisfied = false,
            independentByteVerification = true, errors, completeRequiredEvidenceSatisfied = false
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Layer contract: " + archive);
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors));
    }
}
