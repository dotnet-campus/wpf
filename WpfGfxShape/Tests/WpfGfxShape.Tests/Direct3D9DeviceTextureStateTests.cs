using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceTextureStateTests
{
    private static readonly List<(uint Stage, nint Texture)> NativeCalls = [];
    private static int _nativeResult;
    private static int _textureAddRefCount;
    private static int _textureReleaseCount;

    [TestInitialize]
    public void Initialize()
    {
        NativeCalls.Clear();
        _nativeResult = 0;
        _textureAddRefCount = 0;
        _textureReleaseCount = 0;
    }
    [TestMethod]
    public unsafe void WhenNullTextureIsSetTwiceThenFirstNativeCallIsMadeAndMatchingCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, texture) =>
        {
            Assert.AreEqual(0, (nint) texture);
            callCount++;
            return 0;
        });

        int firstResult = device.SetD3DTexture(0, null);
        int secondResult = device.SetD3DTexture(0, null);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenTextureIsSetTwiceThenMatchingNativeCallIsSkipped()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return 0;
        });
        IDirect3DBaseTexture9* texture = (IDirect3DBaseTexture9*) 1;

        int firstResult = device.SetD3DTexture(0, texture);
        int secondResult = device.SetD3DTexture(0, texture);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenTextureChangesThenNativeCallIsRepeated()
    {
        List<nint> textures = [];
        using Direct3D9Device device = CreateDevice((_, texture) =>
        {
            textures.Add((nint) texture);
            return 0;
        });

        int firstResult = device.SetD3DTexture(0, (IDirect3DBaseTexture9*) 1);
        int secondResult = device.SetD3DTexture(0, (IDirect3DBaseTexture9*) 2);

        Assert.AreEqual((0, 0, "1,2"), (firstResult, secondResult, string.Join(',', textures)));
    }

    [TestMethod]
    public unsafe void WhenSameTextureIsSetOnDifferentStagesThenBothNativeCallsAreMade()
    {
        List<uint> stages = [];
        using Direct3D9Device device = CreateDevice((stage, _) =>
        {
            stages.Add(stage);
            return 0;
        });
        IDirect3DBaseTexture9* texture = (IDirect3DBaseTexture9*) 1;

        int firstResult = device.SetD3DTexture(0, texture);
        int secondResult = device.SetD3DTexture(1, texture);

        Assert.AreEqual((0, 0, "0,1"), (firstResult, secondResult, string.Join(',', stages)));
    }

    [TestMethod]
    public unsafe void WhenTextureSetFailsThenMatchingStateIsRetried()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return callCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
        });
        IDirect3DBaseTexture9* texture = (IDirect3DBaseTexture9*) 1;

        int failedResult = device.SetD3DTexture(0, texture);
        int retryResult = device.SetD3DTexture(0, texture);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 2), (failedResult, retryResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenTextureChangeFailsThenPreviouslyKnownTextureIsRetried()
    {
        List<nint> textures = [];
        using Direct3D9Device device = CreateDevice((_, texture) =>
        {
            textures.Add((nint) texture);
            return textures.Count == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
        });
        IDirect3DBaseTexture9* originalTexture = (IDirect3DBaseTexture9*) 1;

        int initialResult = device.SetD3DTexture(0, originalTexture);
        int failedResult = device.SetD3DTexture(0, (IDirect3DBaseTexture9*) 2);
        int restoreResult = device.SetD3DTexture(0, originalTexture);

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, 0, "1,2,1"),
            (initialResult, failedResult, restoreResult, string.Join(',', textures)));
    }

    [TestMethod]
    public unsafe void WhenTextureSetReturnsDriverInternalErrorThenOriginalHResultIsPreservedAndStateIsRetried()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return callCount == 1 ? Direct3D9Factory.DriverInternalErrorHResult : 0;
        });
        IDirect3DBaseTexture9* texture = (IDirect3DBaseTexture9*) 1;

        int failedResult = device.SetD3DTexture(0, texture);
        int retryResult = device.SetD3DTexture(0, texture);

        Assert.AreEqual(
            (Direct3D9Factory.DriverInternalErrorHResult, 0, 0, 2),
            (failedResult, retryResult, device.UnusableReasonHResult, callCount));
    }

    [TestMethod]
    public unsafe void WhenTextureStageIsOutsideNativeStateTableThenSettingTextureThrowsArgumentOutOfRangeException()
    {
        using Direct3D9Device device = CreateDevice((_, _) => 0);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => device.SetD3DTexture(8, null));
    }

    [TestMethod]
    public unsafe void WhenDeviceIsDisposedThenSettingTextureThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateDevice((_, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetD3DTexture(0, null));
    }

    [TestMethod]
    public void WhenSettingTextureThenNativeSlot65ReceivesExactStageAndPointer()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DBaseTexture9* texture = (IDirect3DBaseTexture9*) 0x1234;

        int result = device.SetD3DTexture(uint.MaxValue % 8, texture);

        Assert.AreEqual((0, "7:4660"), (result, FormatNativeCalls()));
    }

    [TestMethod]
    public void WhenClearingTextureThenNativeSlot65ReceivesNull()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.SetD3DTexture(3, null);

        Assert.AreEqual((0, "3:0"), (result, FormatNativeCalls()));
    }

    [TestMethod]
    public void WhenMatchingTextureReturnsNonzeroSuccessThenStateIsCached()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _nativeResult = 1;

        int firstResult = device.SetD3DTexture(2, (IDirect3DBaseTexture9*) 0x1234);
        int secondResult = device.SetD3DTexture(2, (IDirect3DBaseTexture9*) 0x1234);

        Assert.AreEqual((1, 0, 1, 0), (firstResult, secondResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenNativeTextureSetFailsThenOriginalHResultIsPreservedAndStateIsRetried(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _nativeResult = failureHResult;

        int firstResult = device.SetD3DTexture(4, (IDirect3DBaseTexture9*) 0x1234);
        int secondResult = device.SetD3DTexture(4, (IDirect3DBaseTexture9*) 0x1234);

        Assert.AreEqual(
            (failureHResult, failureHResult, 2, 0),
            (firstResult, secondResult, NativeCalls.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenSettingTextureAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.SetD3DTexture(5, (IDirect3DBaseTexture9*) 0x1234);

        Assert.AreEqual((0, "5:4660"), (result, FormatNativeCalls()));
    }

    [TestMethod]
    public void WhenMatchingTextureIsForcedTwiceThenBothNativeCallsRun()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DBaseTexture9* texture = (IDirect3DBaseTexture9*) 0x1234;
        _nativeResult = 1;

        int firstResult = device.ForceSetTexture(2, texture);
        int secondResult = device.ForceSetTexture(2, texture);

        Assert.AreEqual((1, 1, "2:4660,2:4660"), (firstResult, secondResult, FormatNativeCalls()));
    }

    [TestMethod]
    public void WhenForcedTextureFailsAfterKnownPointerThenMatchingRegularCallIsRetried()
    {
        int callCount = 0;
        IDirect3DBaseTexture9* texture = (IDirect3DBaseTexture9*) 1;
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return callCount == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
        });

        int initialResult = device.SetD3DTexture(3, texture);
        int forcedResult = device.ForceSetTexture(3, texture);
        int retryResult = device.SetD3DTexture(3, texture);

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, 0, 3),
            (initialResult, forcedResult, retryResult, callCount));
    }

    [TestMethod]
    public void WhenSettingTextureThenPointerIsBorrowedWithoutReferenceCountChanges()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeTextureObject textureObject = new();

        using (Direct3D9Device device = CreateDevice(deviceObject.Device))
        {
            _ = device.SetD3DTexture(1, textureObject.Texture);
            _ = device.ForceSetTexture(1, textureObject.Texture);
        }

        Assert.AreEqual((0, 0), (_textureAddRefCount, _textureReleaseCount));
    }

    [TestMethod]
    public void WhenSettingTextureWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetD3DTexture(6, (IDirect3DBaseTexture9*) 0x1234));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    [TestMethod]
    public void WhenForcingTextureWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.ForceSetTexture(6, (IDirect3DBaseTexture9*) 0x1234));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    [TestMethod]
    public void WhenTypedEvictableTextureIsSetAgainInANewUseContextThenUseIsRegisteredAgainAndNativeStateIsCached()
    {
        int callCount = 0;
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return 0;
        });
        using Direct3D9Texture texture = new(
            device.ResourceManager,
            (IDirect3DTexture9*) textureObject.Texture,
            64,
            64);
        texture.SetAsEvictable();

        uint firstDepth = device.EnterUseContext();
        int firstResult = device.SetTexture(0, texture);
        device.ExitUseContext(firstDepth);
        uint secondDepth = device.EnterUseContext();
        int secondResult = device.SetTexture(0, texture);

        Assert.AreEqual((0, 0, secondDepth, 1),
            (firstResult, secondResult, texture.ActiveUseContextDepth, callCount));
        device.ExitUseContext(secondDepth);
    }

    [TestMethod]
    public void WhenTypedNullTextureIsSetThenNoUseContextIsRequired()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return 1;
        });

        int result = device.SetTexture(3, (Direct3D9Texture?) null);

        Assert.AreEqual((1, 1), (result, callCount));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult, Direct3D9Factory.DisplayStateInvalidHResult)]
    public void WhenTypedTextureStateFailsThenHandleDieIsApplied(int nativeResult, int expectedResult)
    {
        int callCount = 0;
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice((_, _) =>
        {
            callCount++;
            return nativeResult;
        });
        using Direct3D9Texture texture = new(
            device.ResourceManager,
            (IDirect3DTexture9*) textureObject.Texture,
            64,
            64);

        int result = device.SetTexture(0, texture);
        int expectedUnusableReason = nativeResult == Direct3D9Factory.DriverInternalErrorHResult
            ? nativeResult
            : 0;

        Assert.AreEqual((expectedResult, expectedUnusableReason, 1),
            (result, device.UnusableReasonHResult, callCount));
    }

    [TestMethod]
    public void WhenTypedTextureIsReleasedThenNativeTextureStateCallIsRejected()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice((_, _) => 0);
        Direct3D9Texture texture = new(
            device.ResourceManager,
            (IDirect3DTexture9*) textureObject.Texture,
            64,
            64);
        texture.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetTexture(0, texture));
        Assert.AreEqual(0, NativeCalls.Count);
    }

    [TestMethod]
    public void WhenTypedTextureIsSetThenNativePointerIsBorrowedWithoutAddingAReference()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice((_, _) => 0);

        using (Direct3D9Texture texture = new(
                   device.ResourceManager,
                   (IDirect3DTexture9*) textureObject.Texture,
                   64,
                   64))
        {
            _ = device.SetTexture(1, texture);
        }

        Assert.AreEqual((0, 1), (_textureAddRefCount, _textureReleaseCount));
    }

    private static string FormatNativeCalls() =>
        string.Join(',', NativeCalls.Select(call => $"{call.Stage}:{call.Texture}"));

    private static Direct3D9Device CreateDevice(Direct3D9SetTexture setTexture)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            setTexture: setTexture);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetTexture(IDirect3DDevice9* self, uint stage, IDirect3DBaseTexture9* texture)
    {
        NativeCalls.Add((stage, (nint) texture));
        return _nativeResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefTexture(IDirect3DBaseTexture9* self)
    {
        _textureAddRefCount++;
        return 2;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseTexture(IDirect3DBaseTexture9* self)
    {
        _textureReleaseCount++;
        return 1;
    }

    private struct FakeTextureObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DBaseTexture9* Texture;

        public FakeTextureObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            Texture = (IDirect3DBaseTexture9*) memory;
            void** vtable = memory + 1;
            Texture->LpVtbl = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DBaseTexture9*, uint>) &AddRefTexture;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DBaseTexture9*, uint>) &ReleaseTexture;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Texture = null;
        }
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 67);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[65] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DBaseTexture9*, int>) &SetTexture;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
