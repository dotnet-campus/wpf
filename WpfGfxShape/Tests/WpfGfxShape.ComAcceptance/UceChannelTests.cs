using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class UceChannelTests
{
    [TestMethod]
    public void WhenResourceIsAddedTwiceThenOnlyFinalReleaseDeletesIt()
    {
        var api = new Api();
        nint connection = 0, channel = 0;
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            uint resource = 0;
            Assert.AreEqual(0, api.CreateResource(channel, 39, &resource));
            Assert.AreNotEqual(0u, resource);
            Assert.AreEqual(0, api.CreateResource(channel, 0, &resource));
            int deleted = -1;
            Assert.AreEqual(0, api.ReleaseResource(channel, resource, &deleted));
            Assert.AreEqual(0, deleted);
            Assert.AreEqual(0, api.ReleaseResource(channel, resource, &deleted));
            Assert.AreEqual(1, deleted);
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(0, api.Commit(channel));
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
        }
    }

    [DataTestMethod]
    [DataRow(0u)]
    [DataRow(uint.MaxValue)]
    public void WhenMalformedPacketRemainsOpenThenCommitDoesNotExecuteIt(uint invalid)
    {
        var api = new Api();
        nint connection = 0, channel = 0;
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            Assert.AreEqual(0, api.Send(&invalid, sizeof(uint), 0, channel));
            Assert.AreEqual(0, api.Commit(channel));
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(channel));
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
        }
    }

    [TestMethod]
    public void WhenSourceChannelIsDestroyedThenDuplicateRemainsUsable()
    {
        var api = new Api();
        nint connection = 0, source = 0, target = 0;
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &source));
            Assert.AreEqual(0, api.CreateChannel(connection, source, &target));
            uint original = 0, duplicate = 0;
            Assert.AreEqual(0, api.CreateResource(source, 39, &original));
            Assert.AreEqual(0, api.Duplicate(source, original, target, &duplicate));
            Assert.AreEqual(0, api.Close(source));
            Assert.AreEqual(0, api.Commit(source));
            Assert.AreEqual(0, api.DestroyChannel(source));
            source = 0;
            Assert.AreEqual(0, api.Disconnect(connection));
            connection = 0;
            uint* update = stackalloc uint[] { 0x24, duplicate };
            Assert.AreEqual(0, api.Send(update, 8, 0, target));
            Assert.AreEqual(0, api.Close(target));
            Assert.AreEqual(0, api.Commit(target));
            int deleted = 0;
            Assert.AreEqual(0, api.ReleaseResource(target, duplicate, &deleted));
            Assert.AreEqual(1, deleted);
            Assert.AreEqual(0, api.Close(target));
            Assert.AreEqual(0, api.Commit(target));
        }
        finally
        {
            if (target != 0) api.DestroyChannel(target);
            if (source != 0) api.DestroyChannel(source);
            if (connection != 0) api.Disconnect(connection);
        }
    }

    [TestMethod]
    public void WhenSeparateBatchUsesAnUncommittedResourceThenPartitionKeepsFirstFailure()
    {
        var api = new Api();
        nint connection = 0, source = 0, target = 0;
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &source));
            Assert.AreEqual(0, api.CreateChannel(connection, source, &target));
            uint resource = 0;
            Assert.AreEqual(0, api.CreateResource(source, 39, &resource));
            uint* update = stackalloc uint[] { 0x24, resource };
            Assert.AreEqual(0, api.Send(update, 8, 1, source));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(source));
            Assert.AreEqual(0, api.Close(source));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(source));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(target));
        }
        finally
        {
            if (target != 0) api.DestroyChannel(target);
            if (source != 0) api.DestroyChannel(source);
            if (connection != 0) api.Disconnect(connection);
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenBitmapPacketIsDiscardedThenTransportReferenceIsReleased(bool failFirst)
    {
        var api = new Api();
        nint connection = 0, channel = 0, instance = 0;
        int disposals = 0;
        var descriptor = new EventDescriptor { Dispose = &DisposeEvent, Raise = &RaiseEvent, Handle = &disposals };
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            Assert.AreEqual(0, api.CreateEvent(&descriptor, &instance));
            // Invalid resource prevents the object from ever being treated as a bitmap.
            var packet = new BitmapPacket { Type = 0x0c, Handle = 99, Bitmap = instance };
            Assert.AreEqual(0, api.Send(&packet, (uint)sizeof(BitmapPacket), 0, channel));
            instance = 0;
            uint invalid = 0;
            Assert.AreEqual(0, api.Send(&invalid, 4, failFirst ? (byte)1 : (byte)0, channel));
            Assert.AreEqual(0, disposals);
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(channel));
            Assert.AreEqual(1, disposals);
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
            if (instance != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance);
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenTransportReleaseReentersThenRemainingReferencesAreReleasedOnce(bool destroy)
    {
        var api = new Api();
        nint connection = 0, channel = 0, first = 0, second = 0;
        ReentryState state = default;
        int secondDisposals = 0;
        var firstDescriptor = new EventDescriptor { Dispose = &DisposeReentrantEvent, Raise = &RaiseEvent, Handle = (int*)&state };
        var secondDescriptor = new EventDescriptor { Dispose = &DisposeEvent, Raise = &RaiseEvent, Handle = &secondDisposals };
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            state.Channel = channel;
            state.Callback = destroy ? api.DestroyChannel : api.Commit;
            Assert.AreEqual(0, api.CreateEvent(&firstDescriptor, &first));
            Assert.AreEqual(0, api.CreateEvent(&secondDescriptor, &second));
            var packet = new BitmapPacket { Type = 0x0c, Handle = 99, Bitmap = first };
            Assert.AreEqual(0, api.Send(&packet, (uint)sizeof(BitmapPacket), 0, channel));
            first = 0;
            packet.Bitmap = second;
            Assert.AreEqual(0, api.Send(&packet, (uint)sizeof(BitmapPacket), 0, channel));
            second = 0;
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(channel));
            Assert.AreEqual(1, state.Disposals);
            Assert.AreEqual(1, secondDisposals);
            Assert.AreEqual(destroy ? 0 : unchecked((int)0x8000FFFF), state.Result);
            Assert.AreEqual(destroy ? unchecked((int)0x80070057) : unchecked((int)0x88980403), api.Commit(channel));
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
            if (first != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)first)[2])(first);
            if (second != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)second)[2])(second);
        }
    }

    [TestMethod]
    public void WhenDestroyReleasesTransportThenCallbackCannotDestroyChannelAgain()
    {
        var api = new Api();
        nint connection = 0, channel = 0, instance = 0;
        ReentryState state = default;
        var descriptor = new EventDescriptor { Dispose = &DisposeReentrantEvent, Raise = &RaiseEvent, Handle = (int*)&state };
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            state.Channel = channel;
            state.Callback = api.DestroyChannel;
            Assert.AreEqual(0, api.CreateEvent(&descriptor, &instance));
            var packet = new BitmapPacket { Type = 0x0c, Handle = 99, Bitmap = instance };
            Assert.AreEqual(0, api.Send(&packet, (uint)sizeof(BitmapPacket), 0, channel));
            instance = 0;
            Assert.AreEqual(0, api.DestroyChannel(channel));
            channel = 0;
            Assert.AreEqual((1, unchecked((int)0x80070057)), (state.Disposals, state.Result));
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
            if (instance != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance);
        }
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenGenericTargetIsBoundThenItRetainsUntilUnbound(bool bindTwice)
    {
        var api = new Api();
        nint connection = 0, channel = 0, instance = 0;
        int disposals = 0;
        EventDescriptor descriptor = new() { Dispose = &DisposeEvent, Raise = &RaiseEvent, Handle = &disposals };
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            Assert.AreEqual(0, api.CreateEvent(&descriptor, &instance));
            uint resource = 0;
            Assert.AreEqual(0, api.CreateResource(channel, 47, &resource));
            GenericTargetPacket packet = new() { Type = 0x34, Handle = resource, Target = (ulong)instance, Width = 2, Height = 2 };
            Assert.AreEqual(0, api.Send(&packet, (uint)sizeof(GenericTargetPacket), 0, channel));
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(0, api.Commit(channel));
            // Rebinding the same pointer must acquire before releasing the old reference.
            packet.Target = bindTwice ? (ulong)instance : 0;
            Assert.AreEqual(0, api.Send(&packet, (uint)sizeof(GenericTargetPacket), 0, channel));
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(0, api.Commit(channel));
            uint remaining = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance);
            instance = 0;
            Assert.AreEqual(bindTwice ? 1u : 0u, remaining);
            Assert.AreEqual(0, api.DestroyChannel(channel));
            channel = 0;
            Assert.AreEqual(1, disposals);
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
            if (instance != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance);
        }
    }

    [TestMethod]
    public void WhenRepeatedTargetRegistrationIsDeletedThenPartitionRetainsRemainingRegistration()
    {
        var api = new Api();
        nint connection = 0, channel = 0, instance = 0;
        int disposals = 0;
        EventDescriptor descriptor = new() { Dispose = &DisposeEvent, Raise = &RaiseEvent, Handle = &disposals };
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            Assert.AreEqual(0, api.CreateEvent(&descriptor, &instance));
            uint resource = 0;
            Assert.AreEqual(0, api.CreateResource(channel, 47, &resource));
            GenericTargetPacket packet = new() { Type = 0x34, Handle = resource, Target = (ulong)instance, Width = 2, Height = 2 };
            Assert.AreEqual(0, api.Send(&packet, (uint)sizeof(GenericTargetPacket), 0, channel));
            Assert.AreEqual(0, api.Send(&packet, (uint)sizeof(GenericTargetPacket), 0, channel));
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(0, api.Commit(channel));
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance);
            instance = 0;
            int deleted = 0;
            Assert.AreEqual(0, api.ReleaseResource(channel, resource, &deleted));
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(0, api.Commit(channel));
            Assert.AreEqual(0, disposals);
            Assert.AreEqual(0, api.DestroyChannel(channel));
            channel = 0;
            Assert.AreEqual(1, disposals);
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
            if (instance != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance);
        }
    }

    [DataTestMethod]
    [DataRow(39u, 36u)]
    [DataRow(47u, 32u)]
    [DataRow(47u, 40u)]
    public void WhenGenericTargetPacketIsInvalidThenCommitRejectsIt(uint resourceType, uint packetSize)
    {
        var api = new Api();
        nint connection = 0, channel = 0;
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            uint resource = 0;
            Assert.AreEqual(0, api.CreateResource(channel, resourceType, &resource));
            byte* bytes = stackalloc byte[40];
            new Span<byte>(bytes, 40).Clear();
            GenericTargetPacket* packet = (GenericTargetPacket*)bytes;
            packet->Type = 0x34;
            packet->Handle = resource;
            Assert.AreEqual(0, api.Send(bytes, packetSize, 0, channel));
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(channel));
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
        }
    }

    [TestMethod]
    public void WhenGenericTargetPacketIsDiscardedThenCallerReferenceIsNotConsumed()
    {
        var api = new Api();
        nint connection = 0, channel = 0, instance = 0;
        int disposals = 0;
        EventDescriptor descriptor = new() { Dispose = &DisposeEvent, Raise = &RaiseEvent, Handle = &disposals };
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            Assert.AreEqual(0, api.CreateEvent(&descriptor, &instance));
            uint resource = 0;
            Assert.AreEqual(0, api.CreateResource(channel, 47, &resource));
            GenericTargetPacket packet = new() { Type = 0x34, Handle = resource, Target = (ulong)instance, Width = 2, Height = 2 };
            Assert.AreEqual(0, api.Send(&packet, (uint)sizeof(GenericTargetPacket), 0, channel));
            Assert.AreEqual(0, api.DestroyChannel(channel));
            channel = 0;
            Assert.AreEqual(0, disposals);
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
            if (instance != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance);
        }
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    public void WhenTargetRebindingDisposesOldOwnerThenReentrantLifecycleReleasesNewOwner(int operation)
    {
        var api = new Api();
        nint connection = 0, channel = 0, previous = 0, next = 0;
        int nextDisposals = 0;
        ReentryState state = default;
        EventDescriptor previousDescriptor = new() { Dispose = &DisposeReentrantEvent, Raise = &RaiseEvent, Handle = (int*)&state };
        EventDescriptor nextDescriptor = new() { Dispose = &DisposeEvent, Raise = &RaiseEvent, Handle = &nextDisposals };
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            state.Channel = operation == 0 ? channel : connection;
            state.Callback = operation == 0 ? api.DestroyChannel : operation == 1 ? api.Disconnect : api.Present;
            Assert.AreEqual(0, api.CreateEvent(&previousDescriptor, &previous));
            Assert.AreEqual(0, api.CreateEvent(&nextDescriptor, &next));
            uint resource = 0;
            Assert.AreEqual(0, api.CreateResource(channel, 47, &resource));
            GenericTargetPacket packet = new() { Type = 0x34, Handle = resource, Target = (ulong)previous, Width = 2, Height = 2 };
            Assert.AreEqual(0, api.Send(&packet, (uint)sizeof(GenericTargetPacket), 0, channel));
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(0, api.Commit(channel));
            Assert.AreEqual(1u, ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)previous)[2])(previous));
            previous = 0;
            packet.Target = (ulong)next;
            Assert.AreEqual(0, api.Send(&packet, (uint)sizeof(GenericTargetPacket), 0, channel));
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(0, api.Commit(channel));
            Assert.AreEqual(1, state.Disposals);
            Assert.AreEqual(0, state.Result);
            if (operation != 0)
            {
                if (operation == 1) connection = 0;
                Assert.AreEqual(0, api.DestroyChannel(channel));
            }
            channel = 0;
            Assert.AreEqual(0, nextDisposals);
            Assert.AreEqual(0u, ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)next)[2])(next));
            next = 0;
            Assert.AreEqual(1, nextDisposals);
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
            if (previous != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)previous)[2])(previous);
            if (next != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)next)[2])(next);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct GenericTargetPacket
    {
        internal uint Type, Handle;
        internal ulong Window, Target;
        internal uint Width, Height, Dummy;
    }

    private struct ReentryState
    {
        internal nint Channel;
        internal delegate* unmanaged[Stdcall]<nint, int> Callback;
        internal int Disposals, Result;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void DisposeReentrantEvent(EventDescriptor* descriptor)
    {
        ReentryState* state = (ReentryState*)descriptor->Handle;
        state->Disposals++;
        state->Result = state->Callback(state->Channel);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BitmapPacket
    {
        internal uint Type, Handle;
        internal nint Bitmap;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct EventDescriptor
    {
        internal delegate* unmanaged[Stdcall]<EventDescriptor*, void> Dispose;
        internal delegate* unmanaged[Stdcall]<EventDescriptor*, byte*, uint, int> Raise;
        internal int* Handle;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void DisposeEvent(EventDescriptor* descriptor) => (*descriptor->Handle)++;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int RaiseEvent(EventDescriptor* descriptor, byte* data, uint size) => 0;

    private sealed class Api
    {
        internal readonly delegate* unmanaged[Stdcall]<EventDescriptor*, nint*, int> CreateEvent;
        internal readonly delegate* unmanaged[Stdcall]<byte, nint*, int> Create;
        internal readonly delegate* unmanaged[Stdcall]<nint, int> Disconnect, DestroyChannel, Close, Commit, Present;
        internal readonly delegate* unmanaged[Stdcall]<nint, nint, nint*, int> CreateChannel;
        internal readonly delegate* unmanaged[Stdcall]<nint, uint, nint, uint*, int> Duplicate;
        internal readonly delegate* unmanaged[Stdcall]<nint, uint, uint*, int> CreateResource;
        internal readonly delegate* unmanaged[Stdcall]<nint, uint, int*, int> ReleaseResource;
        internal readonly delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int> Send;

        internal Api()
        {
            string path = typeof(UceChannelTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "AotDllPath").Value!;
            // Native AOT remains mapped until the test runner exits.
            nint module = NativeLibrary.Load(path);
            CreateEvent = (delegate* unmanaged[Stdcall]<EventDescriptor*, nint*, int>)NativeLibrary.GetExport(module, "MILCreateEventProxy");
            Create = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
            Disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
            Present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
            CreateChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
            DestroyChannel = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
            Close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
            Commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
            CreateResource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
            ReleaseResource = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)NativeLibrary.GetExport(module, "MilResource_ReleaseOnChannel");
            Send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
            Duplicate = (delegate* unmanaged[Stdcall]<nint, uint, nint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_DuplicateHandle");
        }
    }
}
