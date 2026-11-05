using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Abi;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class MediaComEventBridgeTests
{
    private sealed class Observation
    {
        internal byte[]? Packet;
        internal int Disposed;
    }

    [TestMethod]
    public void WhenProductionComProxyIsBridgedThenPacketAndFailureArePreservedUntilFinalRelease()
    {
        Observation observation = new();
        GCHandle root = GCHandle.Alloc(observation);
        EventProxyDescriptor descriptor = new() { Dispose = &Dispose, RaiseEvent = &Receive, Handle = (nuint)GCHandle.ToIntPtr(root) };
        var method = typeof(EventProxyExports).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static)!;
        var create = (delegate* unmanaged[Stdcall]<EventProxyDescriptor*, nint*, int>)method.MethodHandle.GetFunctionPointer();
        nint client = 0;
        Assert.AreEqual(0, create(&descriptor, &client));
        try
        {
            Assert.AreEqual(0, EventProxy.CreateCom(client, out EventProxy? bridge));
            try
            {
                Assert.AreEqual(1u, ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)client)[2])(client));
                client = 0;
                Assert.AreEqual(0, MediaEventProxy.BuildPacket(8, unchecked((int)0x80070002), null, null, out byte[]? packet));
                Assert.AreEqual(unchecked((int)0x80070002), bridge!.RaiseEvent(packet));
                CollectionAssert.AreEqual(packet, observation.Packet);
                Assert.AreEqual(0, observation.Disposed);
            }
            finally { Assert.AreEqual(0u, bridge!.Release()); }
            Assert.AreEqual(1, observation.Disposed);
        }
        finally
        {
            if (client != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)client)[2])(client);
        }
    }

    [TestMethod]
    public void WhenComClientIsNullThenBindingFailsWithoutAnOwner()
    {
        Assert.AreEqual(unchecked((int)0x80070057), EventProxy.CreateCom(0, out _));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Dispose(EventProxyDescriptor* descriptor)
    {
        GCHandle root = GCHandle.FromIntPtr((nint)descriptor->Handle);
        Interlocked.Increment(ref ((Observation)root.Target!).Disposed);
        root.Free();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Receive(EventProxyDescriptor* descriptor, byte* bytes, uint length)
    {
        ((Observation)GCHandle.FromIntPtr((nint)descriptor->Handle).Target!).Packet = new ReadOnlySpan<byte>(bytes, checked((int)length)).ToArray();
        return unchecked((int)0x80070002);
    }
}
