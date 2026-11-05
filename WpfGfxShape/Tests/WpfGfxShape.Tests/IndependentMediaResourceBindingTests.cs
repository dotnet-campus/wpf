using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Abi;
using WpfGfxShape.Core;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class IndependentMediaResourceBindingTests
{
    private sealed class Observation { internal int Disposals; internal int Packets; internal Action<byte[]>? Receive; }

    internal static nint CreateClient(out Func<int> disposals) => CreateClient(out disposals, out _);

    internal static nint CreateClient(out Func<int> disposals, out Func<int> packets)
        => CreateClient(out disposals, out packets, null);

    internal static nint CreateClient(out Func<int> disposals, out Func<int> packets, Action<byte[]>? receive)
    {
        Observation observation = new() { Receive = receive };
        packets = () => Volatile.Read(ref observation.Packets);
        GCHandle root = GCHandle.Alloc(observation);
        EventProxyDescriptor descriptor = new() { Dispose = &Dispose, RaiseEvent = &Receive, Handle = (nuint)GCHandle.ToIntPtr(root) };
        var method = typeof(EventProxyExports).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static)!;
        var create = (delegate* unmanaged[Stdcall]<EventProxyDescriptor*, nint*, int>)method.MethodHandle.GetFunctionPointer();
        nint client = 0;
        Assert.AreEqual(0, create(&descriptor, &client));
        disposals = () => observation.Disposals;
        return client;
    }

    [TestMethod]
    public void WhenInternalCreateBindsTwoResourcesThenReferencesAndRegistryAreBalanced()
    {
        nint client = CreateClient(out var disposals);
        nint media = 0;
        GeneratedProtocolHandleTable table = new();
        try
        {
            Assert.AreEqual(0, SystemMediaSource.Create(client, 1, out media));
            Assert.IsNotNull(SystemMediaSource.Resolve(media));
            Assert.AreEqual(0, table.Create(1, MilResourceType.MediaPlayer));
            Assert.AreEqual(0, table.Create(2, MilResourceType.MediaPlayer));
            Assert.IsTrue(table.TryGetResource(1, out var first));
            Assert.IsTrue(table.TryGetResource(2, out var second));
            Direct3D9Factory.AddRef(media);
            Assert.AreEqual(0, ((GeneratedMediaPlayerResource)first!).ProcessCommand(GeneratedProtocolPacketWriter.WriteMediaPlayer(1, (ulong)media, 0)));
            Direct3D9Factory.AddRef(media);
            Assert.AreEqual(0, ((GeneratedMediaPlayerResource)second!).ProcessCommand(GeneratedProtocolPacketWriter.WriteMediaPlayer(2, (ulong)media, 0)));
            Assert.AreEqual(0, table.Delete(1, MilResourceType.MediaPlayer));
            Assert.IsNotNull(SystemMediaSource.Resolve(media));
            Assert.AreEqual(0, table.Delete(2, MilResourceType.MediaPlayer));
        }
        finally
        {
            Direct3D9Factory.Release(media);
            Direct3D9Factory.Release(client);
        }
        Assert.IsNull(SystemMediaSource.Resolve(media));
        Assert.AreEqual(1, disposals());
    }

    [TestMethod]
    public void WhenOpaqueIdentityIsResolvedThenItIsRejectedWithoutDereference()
        => Assert.IsNull(SystemMediaSource.Resolve(1));

    [DataTestMethod]
    [DataRow((byte)0)]
    [DataRow((byte)1)]
    [DataRow((byte)2)]
    public void WhenClientIsNullThenInternalCreateClearsOutput(byte canOpen)
    {
        Assert.AreEqual((unchecked((int)0x80070057), (nint)0), Create(0, canOpen));
    }

    [TestMethod]
    public void WhenListenerUnsubscribesReentrantlyThenLaterNotificationsAreRejected()
    {
        IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
        int count = 0;
        Action? listener = null;
        listener = () => { count++; identity.Source.Unsubscribe(listener!); };
        try
        {
            identity.Source.Subscribe(listener);
            identity.Source.Subscribe(listener);
            identity.Source.NotifyFrame();
            identity.Source.NotifyFrame();
            Assert.AreEqual(1, count);
        }
        finally { Direct3D9Factory.Release(identity.Pointer); }
    }

    private static (int, nint) Create(nint client, byte canOpen)
    {
        int hr = SystemMediaSource.Create(client, canOpen, out nint output);
        return (hr, output);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Dispose(EventProxyDescriptor* descriptor)
    {
        GCHandle root = GCHandle.FromIntPtr((nint)descriptor->Handle);
        Interlocked.Increment(ref ((Observation)root.Target!).Disposals);
        root.Free();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Receive(EventProxyDescriptor* descriptor, byte* bytes, uint length)
    {
        var observation = (Observation)GCHandle.FromIntPtr((nint)descriptor->Handle).Target!;
        observation.Receive?.Invoke(new ReadOnlySpan<byte>(bytes, checked((int)length)).ToArray());
        Interlocked.Increment(ref observation.Packets);
        return 0;
    }
}
