using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class MilColorWireLayoutTests
{
    [TestMethod]
    public unsafe void WhenColorLayoutIsMeasuredThenNativeRgbaOffsetsAndSizeMatch()
    {
        Assert.AreEqual((16, 16, 0, 4, 8, 12),
            (sizeof(MilColorF), Marshal.SizeOf<MilColorF>(),
                Marshal.OffsetOf<MilColorF>("<Red>k__BackingField").ToInt32(),
                Marshal.OffsetOf<MilColorF>("<Green>k__BackingField").ToInt32(),
                Marshal.OffsetOf<MilColorF>("<Blue>k__BackingField").ToInt32(),
                Marshal.OffsetOf<MilColorF>("<Alpha>k__BackingField").ToInt32()));
    }

    [TestMethod]
    public void WhenInternalArgbConstructorIsSerializedThenIndependentRgbaBytesMatch()
    {
        MilColorF color = new(1, 0.25f, 0.5f, 0.75f);
        // Native wgx_misc.cs declares r,g,b,a; these IEEE-754 bytes are not derived from the managed layout.
        byte[] expected = [0, 0, 128, 62, 0, 0, 0, 63, 0, 0, 64, 63, 0, 0, 128, 63];
        CollectionAssert.AreEqual(expected, MemoryMarshal.AsBytes(new[] { color }.AsSpan()).ToArray());
    }

    [DataTestMethod]
    [DataRow("Value", 8)]
    [DataRow("Solid", 16)]
    [DataRow("Linear", 92)]
    [DataRow("Radial", 116)]
    [DataRow("Ambient", 8)]
    [DataRow("Directional", 8)]
    [DataRow("Diffuse", 8)]
    [DataRow("DiffuseAmbient", 24)]
    [DataRow("Specular", 8)]
    [DataRow("Emissive", 8)]
    [DataRow("Target", 8)]
    public void WhenSiblingColorPacketsAreWrittenThenIndependentRgbaBytesMatch(string packetKind, int offset)
    {
        MilColorF color = new(1, 0.25f, 0.5f, 0.75f);
        MilGradientStop[] stops = [new(0.25, color)];
        byte[] packet = packetKind switch
        {
            "Value" => GeneratedProtocolPacketWriter.WriteColorResource(1, color),
            "Solid" => GeneratedProtocolPacketWriter.WriteSolidColorBrush(1, 1, color),
            "Linear" => GeneratedProtocolPacketWriter.WriteLinearGradientBrush(1, 1, default, default, stops),
            "Radial" => GeneratedProtocolPacketWriter.WriteRadialGradientBrush(1, 1, default, 1, 1, default, stops),
            "Ambient" => GeneratedProtocolPacketWriter.WriteAmbientLight(1, color),
            "Directional" => GeneratedProtocolPacketWriter.WriteDirectionalLight(1, color, default),
            "Diffuse" or "DiffuseAmbient" => GeneratedProtocolPacketWriter.WriteDiffuseMaterial(1, color, color),
            "Specular" => GeneratedProtocolPacketWriter.WriteSpecularMaterial(1, color, 1),
            "Emissive" => GeneratedProtocolPacketWriter.WriteEmissiveMaterial(1, color),
            "Target" => GeneratedProtocolPacketWriter.WriteTargetSetClearColor(1, color),
            _ => throw new ArgumentOutOfRangeException(nameof(packetKind))
        };
        byte[] expected = [0, 0, 128, 62, 0, 0, 0, 63, 0, 0, 64, 63, 0, 0, 128, 63];
        CollectionAssert.AreEqual(expected, packet.AsSpan(offset, 16).ToArray());
    }
}
