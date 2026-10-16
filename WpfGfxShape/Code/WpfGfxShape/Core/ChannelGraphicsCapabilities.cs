using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace WpfGfxShape.Core;

internal static class ChannelGraphicsCapabilities
{
    internal static bool HasDisplays()
    {
        using var displays = Direct3D9DisplaySetFactory.Create(0, 0, () => 0, () => 0);
        return !displays.Characteristics.Displays.IsDefaultOrEmpty;
    }

    internal static void Query(bool common, Span<byte> destination, out uint uniqueness)
    {
        destination.Clear();
        uniqueness = 0;
        using var set = Direct3D9DisplaySetFactory.Create(0, 0, () => 0, () => 0);
        var displays = set.Characteristics.Displays;
        if (displays.IsDefaultOrEmpty || set.Characteristics.IsNonLocalDevicePresent) return;
        var caps = displays[0].GraphicsAccelerationCaps;
        // Display identity changes invalidate the capabilities observed by the UI thread.
        uint identity = 2166136261;
        foreach (var display in displays)
        {
            foreach (char character in display.DeviceName) identity = unchecked((identity ^ character) * 16777619);
            identity = unchecked((identity ^ display.DisplayMode.Width ^ display.DisplayMode.Height) * 16777619);
        }
        uniqueness = identity;
        if (common)
        {
            for (int index = 1; index < displays.Length; index++)
            {
                var secondary = displays[index].GraphicsAccelerationCaps;
                caps = caps with
                {
                    TierValue = Math.Min(caps.TierValue, secondary.TierValue),
                    HasWddmSupport = Math.Min(caps.HasWddmSupport, secondary.HasWddmSupport),
                    PixelShaderVersion = Math.Min(caps.PixelShaderVersion, secondary.PixelShaderVersion),
                    VertexShaderVersion = Math.Min(caps.VertexShaderVersion, secondary.VertexShaderVersion),
                    MaxTextureWidth = Math.Min(caps.MaxTextureWidth, secondary.MaxTextureWidth),
                    MaxTextureHeight = Math.Min(caps.MaxTextureHeight, secondary.MaxTextureHeight),
                    WindowCompatibleMode = Math.Min(caps.WindowCompatibleMode, secondary.WindowCompatibleMode),
                    HasSse2Support = Math.Min(caps.HasSse2Support, secondary.HasSse2Support),
                    MaxPixelShader30InstructionSlots = Math.Min(caps.MaxPixelShader30InstructionSlots, secondary.MaxPixelShader30InstructionSlots)
                };
            }
        }
        uint[] values = [unchecked((uint)caps.TierValue), unchecked((uint)caps.HasWddmSupport),
            caps.PixelShaderVersion, caps.VertexShaderVersion, caps.MaxTextureWidth, caps.MaxTextureHeight,
            unchecked((uint)caps.WindowCompatibleMode), caps.BitsPerPixel, caps.HasSse2Support, caps.MaxPixelShader30InstructionSlots];
        for (int index = 0; index < values.Length; index++)
            BinaryPrimitives.WriteUInt32LittleEndian(destination[(index * 4)..], values[index]);
    }

    internal static byte[] CreateTierMessage(int common)
    {
        byte[] message = new byte[64];
        BinaryPrimitives.WriteUInt32LittleEndian(message, 4);
        BinaryPrimitives.WriteInt32LittleEndian(message.AsSpan(8), common);
        Query(common != 0, message.AsSpan(16, 40), out uint uniqueness);
        BinaryPrimitives.WriteUInt32LittleEndian(message.AsSpan(12), uniqueness);
        using var assessment = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\WinSAT");
        if (assessment?.GetValue("VideoMemoryBandwidth") is int bandwidth)
            BinaryPrimitives.WriteInt32LittleEndian(message.AsSpan(56), bandwidth);
        if (assessment?.GetValue("VideoMemorySize") is int size)
            BinaryPrimitives.WriteInt32LittleEndian(message.AsSpan(60), size);
        return message;
    }
}
