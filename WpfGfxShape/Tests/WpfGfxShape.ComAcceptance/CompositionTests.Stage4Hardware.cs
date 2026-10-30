using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [TestMethod]
    [DataRow(0xff00ff00u, 0x0000ff00u)]
    [DataRow(0xff0000ffu, 0x00ff0000u)]
    public void WhenStage4HardwareCacheGeometryDrawsThenHitAndMutationPreservePixelsAfterCallerRelease(uint color, uint expected)
        => RunVisualCacheConsumer(true, color, expected, true);

    /// <summary>Verifies hardware cache recovery after repeated rejected configurations.</summary>
    [TestMethod]
    [DataRow(0xff00ff00u, 0x0000ff00u)]
    [DataRow(0xff0000ffu, 0x00ff0000u)]
    public void WhenStage4HardwareCacheConfigurationFailsThenRecoveryAndMutationRenderRealPixels(uint color, uint expected)
        => RunVisualCacheConsumer(true, color, expected, true, true);

    [TestMethod]
    [DataRow(0xff00ff00u, 0x0000ff00u)]
    [DataRow(0xff0000ffu, 0x00ff0000u)]
    public void WhenStage4HardwareWicCacheDrawsThenHitAndSourceReplacementPreservePixels(uint color, uint expected)
        => RunVisualCacheConsumer(true, color, expected, true, false, true);

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive(int action)
        => RunVisualCacheConsumer(true, 0xff00ff00u, 0x0000ff00u, true, false, true, action);

    /// <summary>Verifies candidate CopyPixels failure and retry through a system WIC write lock.</summary>
    [TestMethod]
    [DataRow(0xff00ff00u, 0x0000ff00u)]
    [DataRow(0xff0000ffu, 0x00ff0000u)]
    public void WhenStage4HardwareWicCandidateIsWriteLockedThenCommittedFrameSurvivesAndSameGenerationRetries(uint color, uint expected)
        => RunVisualCacheConsumer(true, color, expected, true, false, true, -1, true);

    /// <summary>Verifies that two real HWND devices retain their own committed cache generation.</summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenStage4HardwareCacheHasTwoDevicesThenBothCommittedKeysHitWhileWicIsWriteLocked(bool closeSecondFirst)
        => RunVisualCacheConsumer(true, 0xff00ff00u, 0x0000ff00u, true, false, true, -1, true, true, closeSecondFirst);

    private static void WriteHardwareWic(nint bitmap, uint color)
    {
        nint bitmapLock = 0;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock));
            uint size; uint* pixels;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, uint**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels));
            pixels[0] = color; pixels[1] = color;
        }
        finally { Release(bitmapLock); }
    }

    private static uint BindHardwareCacheGeometry(
        delegate* unmanaged[Stdcall]<nint, uint, uint*, int> resource,
        delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int> send,
        nint channel, uint visual, uint source)
    {
        uint mode = 0, brush = 0, geometry = 0, data = 0;
        Assert.AreEqual(0, resource(channel, 94, &mode));
        Assert.AreEqual(0, resource(channel, 83, &brush));
        Assert.AreEqual(0, resource(channel, 69, &geometry));
        Assert.AreEqual(0, resource(channel, 43, &data));
        byte[] packet = new byte[28];
        MemoryMarshal.Write(packet.AsSpan(), 0x8du);
        MemoryMarshal.Write(packet.AsSpan(4), mode);
        MemoryMarshal.Write(packet.AsSpan(8), 1.0);
        SendSamplerPacket(send, channel, packet);
        packet = new byte[36];
        MemoryMarshal.Write(packet.AsSpan(), 0x84u);
        MemoryMarshal.Write(packet.AsSpan(4), brush);
        MemoryMarshal.Write(packet.AsSpan(8), 1.0);
        MemoryMarshal.Write(packet.AsSpan(28), mode);
        MemoryMarshal.Write(packet.AsSpan(32), source);
        SendSamplerPacket(send, channel, packet);
        packet = new byte[72];
        MemoryMarshal.Write(packet.AsSpan(), 0x79u);
        MemoryMarshal.Write(packet.AsSpan(4), geometry);
        MemoryMarshal.Write(packet.AsSpan(40), 32.0);
        MemoryMarshal.Write(packet.AsSpan(48), 16.0);
        SendSamplerPacket(send, channel, packet);
        packet = new byte[36];
        MemoryMarshal.Write(packet.AsSpan(), 0x18u);
        MemoryMarshal.Write(packet.AsSpan(4), data);
        MemoryMarshal.Write(packet.AsSpan(8), 24u);
        MemoryMarshal.Write(packet.AsSpan(12), 24u);
        MemoryMarshal.Write(packet.AsSpan(16), 0x46u);
        MemoryMarshal.Write(packet.AsSpan(20), brush);
        MemoryMarshal.Write(packet.AsSpan(28), geometry);
        SendSamplerPacket(send, channel, packet);
        uint* content = stackalloc uint[] { 0x22, visual, data };
        Assert.AreEqual(0, send(content, 12, 0, channel));
        return mode;
    }
}
