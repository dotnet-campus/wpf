using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe partial class MediaExportsTests
{
    private static nint LoadCandidate()
    {
        string path = AcceptancePaths.AotDll;
        return NativeLibrary.Load(path);
    }

    [TestMethod]
    [DataRow("MILMediaStop")]
    [DataRow("MILMediaClose")]
    [DataRow("MILMediaNeedUIFrameUpdate")]
    [DataRow("MILMediaShutdown")]
    [DataRow("MILMediaProcessExitHandler")]
    public void WhenMediaIsNullThenNoArgumentExportReturnsInvalidArgument(string name)
    {
        var call = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(LoadCandidate(), name);
        Assert.AreEqual(unchecked((int)0x80070057), call(0));
    }

    [TestMethod]
    public void WhenMediaIsNullThenOpenReturnsInvalidArgument()
    {
        var call = (delegate* unmanaged[Stdcall]<nint, char*, int>)NativeLibrary.GetExport(LoadCandidate(), "MILMediaOpen");
        Assert.AreEqual(unchecked((int)0x80070057), call(0, null));
    }

    [TestMethod]
    [DataRow("MILMediaSetRate")]
    [DataRow("MILMediaSetVolume")]
    [DataRow("MILMediaSetBalance")]
    public void WhenMediaIsNullThenDoubleSetterReturnsInvalidArgument(string name)
    {
        var call = (delegate* unmanaged[Stdcall]<nint, double, int>)NativeLibrary.GetExport(LoadCandidate(), name);
        Assert.AreEqual(unchecked((int)0x80070057), call(0, 0.5));
    }

    [TestMethod]
    public void WhenMediaIsNullThenPositionSetterReturnsInvalidArgument()
    {
        var call = (delegate* unmanaged[Stdcall]<nint, long, int>)NativeLibrary.GetExport(LoadCandidate(), "MILMediaSetPosition");
        Assert.AreEqual(unchecked((int)0x80070057), call(0, 1));
    }

    [TestMethod]
    public void WhenMediaIsNullThenScrubbingSetterReturnsInvalidArgument()
    {
        var call = (delegate* unmanaged[Stdcall]<nint, byte, int>)NativeLibrary.GetExport(LoadCandidate(), "MILMediaSetIsScrubbingEnabled");
        Assert.AreEqual(unchecked((int)0x80070057), call(0, 1));
    }

    [TestMethod]
    [DataRow("MILMediaIsBuffering")]
    [DataRow("MILMediaCanPause")]
    [DataRow("MILMediaHasVideo")]
    [DataRow("MILMediaHasAudio")]
    [DataRow("MILMediaGetPosition")]
    [DataRow("MILMediaGetMediaLength")]
    [DataRow("MILMediaGetNaturalWidth")]
    [DataRow("MILMediaGetNaturalHeight")]
    [DataRow("MILMediaGetDownloadProgress")]
    [DataRow("MILMediaGetBufferingProgress")]
    public void WhenMediaIsNullThenOutputExportReturnsInvalidArgument(string name)
    {
        var call = (delegate* unmanaged[Stdcall]<nint, void*, int>)NativeLibrary.GetExport(LoadCandidate(), name);
        long sentinel = -1;
        Assert.AreEqual((unchecked((int)0x80070057), -1L), (call(0, &sentinel), sentinel));
    }

    // This is a real installed WPF/WMP media object, not a test COM implementation.
    [TestMethod]
    [DataRow("MILMediaIsBuffering", 12)]
    [DataRow("MILMediaCanPause", 13)]
    [DataRow("MILMediaHasVideo", 16)]
    [DataRow("MILMediaHasAudio", 17)]
    public void WhenSystemMediaIsQueriedThenBooleanExportMatchesRealComSlot(string name, int slot)
    {
        using var source = new SystemMedia();
        var call = (delegate* unmanaged[Stdcall]<nint, byte*, int>)NativeLibrary.GetExport(LoadCandidate(), name);
        byte expected = 0xCC, actual = 0xCC;
        int expectedResult = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)(*(void***)source.Media)[slot])(source.Media, &expected);
        Assert.AreEqual((expectedResult, expected), (call(source.Media, &actual), actual));
    }

    [TestMethod]
    [DataRow("MILMediaGetNaturalHeight", 18)]
    [DataRow("MILMediaGetNaturalWidth", 19)]
    public void WhenSystemMediaIsQueriedThenDimensionExportMatchesRealComSlot(string name, int slot)
    {
        using var source = new SystemMedia();
        var call = (delegate* unmanaged[Stdcall]<nint, uint*, int>)NativeLibrary.GetExport(LoadCandidate(), name);
        uint expected = uint.MaxValue, actual = uint.MaxValue;
        int expectedResult = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)source.Media)[slot])(source.Media, &expected);
        Assert.AreEqual((expectedResult, expected), (call(source.Media, &actual), actual));
    }

    [TestMethod]
    [DataRow("MILMediaGetPosition", 6)]
    [DataRow("MILMediaGetMediaLength", 20)]
    public void WhenSystemMediaIsQueriedThenTimeExportMatchesRealComSlot(string name, int slot)
    {
        using var source = new SystemMedia();
        var call = (delegate* unmanaged[Stdcall]<nint, long*, int>)NativeLibrary.GetExport(LoadCandidate(), name);
        long expected = -1, actual = -1;
        int expectedResult = ((delegate* unmanaged[Stdcall]<nint, long*, int>)(*(void***)source.Media)[slot])(source.Media, &expected);
        Assert.AreEqual((expectedResult, expected), (call(source.Media, &actual), actual));
    }

    [TestMethod]
    [DataRow("MILMediaGetDownloadProgress", 14)]
    [DataRow("MILMediaGetBufferingProgress", 15)]
    public void WhenSystemMediaIsQueriedThenProgressExportMatchesRealComSlot(string name, int slot)
    {
        using var source = new SystemMedia();
        var call = (delegate* unmanaged[Stdcall]<nint, double*, int>)NativeLibrary.GetExport(LoadCandidate(), name);
        double expected = -1, actual = -1;
        int expectedResult = ((delegate* unmanaged[Stdcall]<nint, double*, int>)(*(void***)source.Media)[slot])(source.Media, &expected);
        Assert.AreEqual((expectedResult, expected), (call(source.Media, &actual), actual));
    }

    [TestMethod]
    public void WhenCandidateShutdownIsRepeatedThenNativeTeardownIsSubmittedOnce()
    {
        using var source = new SystemMedia(candidate: true);
        var shutdown = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(LoadCandidate(), "MILMediaShutdown");
        int first = shutdown(source.Media);
        int second = shutdown(source.Media);
        Assert.AreEqual((0, 0), (first, second));
    }

    [DataTestMethod]
    [DataRow((byte)1)]
    [DataRow((byte)0)]
    public void WhenRealSystemMediaIsTransferredThenBothChannelModesConsumeTheBatch(byte synchronous)
    {
        using var source = new SystemMedia();
        nint module = LoadCandidate();
        var create = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var resourceCreate = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<uint, nint, nint, byte, int>)NativeLibrary.GetExport(module, "MilResource_SendCommandMedia");
        nint connection = 0, channel = 0;
        try
        {
            Assert.AreEqual(0, create(synchronous, &connection));
            Assert.AreEqual(0, channelCreate(connection, 0, &channel));
            uint resource = 0;
            Assert.AreEqual(0, resourceCreate(channel, 1, &resource));
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source.Media)[1])(source.Media);
            int result = send(resource, source.Media, channel, 1);
            if (result < 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source.Media)[2])(source.Media);
            Assert.AreEqual(0, result);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch"))(channel));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel"))(channel));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilComposition_SyncFlush"))(channel));
        }
        finally
        {
            if (channel != 0) ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel"))(channel);
            if (connection != 0) ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect"))(connection);
        }
    }

    private sealed class SystemMedia : IDisposable
    {
        private nint _factory, _proxy;
        private readonly bool _uninitialize;
        internal readonly AutoResetEvent _notification = new(false);
        private GCHandle _eventRoot;
        internal bool WaitForNotification(TimeSpan timeout) => _notification.WaitOne(timeout);
        internal nint Media { get; private set; }

        internal uint ReleaseCallerReference()
        {
            nint media = Media;
            Media = 0;
            return ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)media)[2])(media);
        }

        internal Action? _callback;
        internal Exception? CallbackError;
        internal readonly System.Collections.Concurrent.ConcurrentQueue<string> Events = new();
        internal Action? Callback { set => Volatile.Write(ref _callback, value); }
        internal SystemMedia(bool candidate = false)
        {
            _ = typeof(System.Windows.Media.MediaPlayer).Assembly.Location;
            int apartment = CoInitializeEx(0, 2);
            Assert.IsTrue(apartment >= 0 || apartment == unchecked((int)0x80010106));
            _uninitialize = apartment >= 0;
            try
            {
                using JsonDocument fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ReferenceData", "original-wpf.json")));
                string path = AcceptancePaths.Sandbox
                    ? Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll")
                    : fixture.RootElement.GetProperty("NativeDll").GetString()!;
                nint module = candidate ? LoadCandidate() : NativeLibrary.Load(path);
                var createFactory = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
                var createProxy = (delegate* unmanaged[Stdcall]<Descriptor*, nint*, int>)NativeLibrary.GetExport(module, "MILCreateEventProxy");
                var createMedia = (delegate* unmanaged[Stdcall]<nint, nint, byte, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateMediaPlayer");
                _eventRoot = GCHandle.Alloc(this);
                Descriptor descriptor = new() { Dispose = &DisposeDescriptor, Raise = &RaiseEvent, Handle = (nuint)GCHandle.ToIntPtr(_eventRoot) };
                nint factory = 0, proxy = 0, media = 0;
                int result = createFactory(&factory, 0x200184C0);
                _factory = factory;
                Assert.AreEqual(0, result);
                result = createProxy(&descriptor, &proxy);
                _proxy = proxy;
                if (result >= 0) _eventRoot = default;
                Assert.AreEqual(0, result);
                result = createMedia(_factory, _proxy, 1, &media);
                Media = media;
                Assert.AreEqual(0, result);
                Assert.AreNotEqual((nint)0, Media);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            nint media = Media;
            Media = 0;
            if (media != 0)
            {
                _ = ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)media)[22])(media);
                Release(media);
            }
            Release(_proxy); _proxy = 0;
            if (_eventRoot.IsAllocated) _eventRoot.Free();
            Release(_factory); _factory = 0;
            if (_uninitialize) CoUninitialize();
        }
    }

    private static void Release(nint instance)
    {
        if (instance != 0) _ = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Descriptor
    {
        internal delegate* unmanaged[Stdcall]<Descriptor*, void> Dispose;
        internal delegate* unmanaged[Stdcall]<Descriptor*, byte*, uint, int> Raise;
        internal nuint Handle;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void DisposeDescriptor(Descriptor* descriptor)
    {
        if (descriptor->Handle != 0) GCHandle.FromIntPtr((nint)descriptor->Handle).Free();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int RaiseEvent(Descriptor* descriptor, byte* data, uint size)
    {
        if (size >= 4 && descriptor->Handle != 0)
        {
            var source = (SystemMedia)GCHandle.FromIntPtr((nint)descriptor->Handle).Target!;
            source.Events.Enqueue($"type={*(uint*)data}; failureHr=0x{(size >= 8 ? *(uint*)(data + 4) : 0):X8}; bytes={Convert.ToHexString(new ReadOnlySpan<byte>(data, checked((int)size)))}");
            try { Volatile.Read(ref source._callback)?.Invoke(); }
            catch (Exception error) { source.CallbackError = error; }
            finally { source._notification.Set(); }
        }
        return 0;
    }

    [DllImport("ole32.dll", ExactSpelling = true)]
    private static extern int CoInitializeEx(nint reserved, uint mode);

    [DllImport("ole32.dll", ExactSpelling = true)]
    private static extern void CoUninitialize();
}
