using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceEntryTests
{
    [TestMethod]
    public void WhenHardwareDeviceIsCreatedThenEntryLockIsNotInitialized()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal);

        Assert.IsFalse(device.IsEnsuringCorrectMultithreadedRendering);
    }

    [TestMethod]
    public void WhenSoftwareDeviceIsCreatedThenEntryLockIsInitialized()
    {
        using Direct3D9Device device = CreateDevice(Devtype.SW);

        Assert.IsTrue(device.IsEnsuringCorrectMultithreadedRendering);
    }

    [TestMethod]
    public void WhenDisposedDeviceChecksEntryLockInitializationThenOperationIsRejected()
    {
        Direct3D9Device device = CreateDevice(Devtype.SW);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.IsEnsuringCorrectMultithreadedRendering);
    }

    [TestMethod]
    public void WhenSingleThreadedDeviceHasNotBeenEnteredThenItIsProtectedWithoutConfirmation()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal);

        Assert.IsTrue(device.IsProtected(false));
    }

    [TestMethod]
    public void WhenSingleThreadedDeviceHasNotBeenEnteredThenForcedConfirmationFails()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal);

        Assert.IsFalse(device.IsProtected(true));
    }

    [TestMethod]
    public void WhenDeviceHasNotBeenEnteredThenIsEnteredIsFalse()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal);

        Assert.IsFalse(device.IsEntered());
    }

    [TestMethod]
    public void WhenMultithreadedDeviceHasNotBeenEnteredThenItIsNotProtected()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal, D3D9.CreateMultithreaded);

        Assert.IsFalse(device.IsProtected(false));
    }

    [TestMethod]
    public void WhenDeviceIsEnteredThenCurrentThreadIsProtected()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal, D3D9.CreateMultithreaded);

        device.Enter();

        Assert.IsTrue(device.IsEntered() && device.IsProtected(true));
        device.Leave();
        Assert.IsFalse(device.IsEntered() || device.IsProtected(false));
    }

    [TestMethod]
    public void WhenNestedEnterIsPairedThenDeviceRemainsEnteredUntilFinalLeave()
    {
        using Direct3D9Device device = CreateDevice(Devtype.SW);

        device.Enter();
        device.Enter();
        device.Leave();

        Assert.IsTrue(device.IsEntered());
        device.Leave();
        Assert.IsFalse(device.IsEntered());
    }

    [TestMethod]
    public void WhenNestedEnterLeavesOnceThenCurrentThreadRemainsProtected()
    {
        using Direct3D9Device device = CreateDevice(Devtype.SW);

        device.Enter();
        device.Enter();
        device.Leave();

        Assert.IsTrue(device.IsProtected(true));
        device.Leave();
    }

    [TestMethod]
    public void WhenMultithreadedDeviceIsEnteredThenAnotherThreadIsEnteredButNotProtected()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal, D3D9.CreateMultithreaded);
        device.Enter();
        (bool IsEntered, bool IsProtected) observed = default;

        Thread worker = new(() => observed = (device.IsEntered(), device.IsProtected(false)));
        worker.Start();
        worker.Join();

        device.Leave();
        Assert.AreEqual((true, false), observed);
    }

    [TestMethod]
    public void WhenLeaveIsCalledWithoutEnterThenOperationIsRejected()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal);

        Assert.ThrowsExactly<InvalidOperationException>(device.Leave);
    }

    [TestMethod]
    public void WhenSoftwareDeviceIsEnteredThenSecondThreadWaitsForLeave()
    {
        using Direct3D9Device device = CreateDevice(Devtype.SW);
        using ManualResetEventSlim entered = new(false);
        using ManualResetEventSlim release = new(false);
        using ManualResetEventSlim secondEntered = new(false);

        Task first = Task.Run(() =>
        {
            device.Enter();
            entered.Set();
            release.Wait();
            device.Leave();
        });

        entered.Wait();
        Task second = Task.Run(() =>
        {
            device.Enter();
            secondEntered.Set();
            device.Leave();
        });

        Assert.IsFalse(secondEntered.Wait(TimeSpan.FromMilliseconds(200)));
        release.Set();
        Assert.IsTrue(secondEntered.Wait(TimeSpan.FromSeconds(5)));
        Task.WaitAll(first, second);
    }

    [TestMethod]
    public void WhenEntryGuardIsCreatedThenDeviceIsEntered()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal, D3D9.CreateMultithreaded);
        using Direct3D9DeviceEntryGuard guard = new(device);

        Assert.IsTrue(device.IsEntered());
    }

    [TestMethod]
    public void WhenEntryGuardIsDisposedThenDeviceIsLeft()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal, D3D9.CreateMultithreaded);
        Direct3D9DeviceEntryGuard guard = new(device);

        guard.Dispose();

        Assert.IsFalse(device.IsEntered());
    }

    [TestMethod]
    public void WhenEntryGuardIsDisposedTwiceThenDeviceIsLeftOnce()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal, D3D9.CreateMultithreaded);
        Direct3D9DeviceEntryGuard guard = new(device);

        guard.Dispose();
        guard.Dispose();

        Assert.IsFalse(device.IsEntered());
    }

    [TestMethod]
    public void WhenNestedEntryGuardIsDisposedThenOuterEntryRemains()
    {
        using Direct3D9Device device = CreateDevice(Devtype.SW);
        using Direct3D9DeviceEntryGuard outer = new(device);
        Direct3D9DeviceEntryGuard inner = new(device);

        inner.Dispose();

        Assert.IsTrue(device.IsEntered());
    }

    [TestMethod]
    public void WhenDisposedDeviceEntryGuardIsCreatedThenOperationIsRejected()
    {
        Direct3D9Device device = CreateDevice(Devtype.Hal);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => new Direct3D9DeviceEntryGuard(device));
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenEntryMethodsAreProtected()
    {
        Direct3D9Device device = CreateDevice(Devtype.Hal);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(device.Enter);
    }

    [TestMethod]
    public void WhenDisposingEnteredDeviceThenCallIsRejectedWithoutChangingEntryState()
    {
        Direct3D9Device device = CreateDevice(Devtype.Hal, D3D9.CreateMultithreaded);
        device.Enter();

        Assert.ThrowsExactly<InvalidOperationException>(device.Dispose);
        Assert.IsTrue(device.IsEntered() && device.IsProtected(true));

        device.Leave();
        device.Dispose();
    }

    [TestMethod]
    public void WhenDisposingNestedEnteredDeviceThenAllExternalEntriesMustLeaveFirst()
    {
        Direct3D9Device device = CreateDevice(Devtype.SW);
        device.Enter();
        device.Enter();
        device.Leave();

        Assert.ThrowsExactly<InvalidOperationException>(device.Dispose);
        Assert.IsTrue(device.IsEntered() && device.IsProtected(true));

        device.Leave();
        device.Dispose();
    }

    [TestMethod]
    public void WhenDeviceUsesManagedEvictableResourceThenExistingEntryAndUseContextRemainActive()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal);
        using TestResource resource = new(device.ResourceManager, static () => { });
        resource.SetAsEvictable();
        device.Enter();
        uint depth = device.EnterUseContext();

        device.Use(resource);

        Assert.AreEqual(
            (true, true, depth, 1, 0u, false),
            (device.IsEntered(), device.IsInUseContext(), resource.ActiveUseContextDepth,
                device.ResourceCount, device.ResourceManager.CompletedFrameCount, device.IsInScene));
        device.ExitUseContext(depth);
        device.Leave();
    }

    [TestMethod]
    public void WhenDeviceUsesEvictableResourceWithoutUseContextThenCallIsRejectedWithoutEnteringDevice()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal);
        using TestResource resource = new(device.ResourceManager, static () => { });
        resource.SetAsEvictable();

        Assert.ThrowsExactly<InvalidOperationException>(() => device.Use(resource));
        Assert.IsFalse(device.IsEntered() || device.IsInUseContext());
    }

    [TestMethod]
    public void WhenDeviceUsesNullResourceThenCallIsRejected()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal);

        Assert.ThrowsExactly<ArgumentNullException>(() => device.Use(null!));
    }

    [TestMethod]
    public void WhenDeviceUsesResourceManagedByAnotherManagerThenCallIsRejectedWithoutChangingOwnership()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal);
        Direct3D9ResourceManager foreignManager = new();
        using TestResource resource = new(foreignManager, static () => { });
        resource.SetAsEvictable();
        uint depth = device.EnterUseContext();

        Assert.ThrowsExactly<InvalidOperationException>(() => device.Use(resource));
        Assert.AreEqual((1, 0, 0u),
            (foreignManager.ResourceCount, device.ResourceCount, resource.ActiveUseContextDepth));
        device.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenDeviceUsesInvalidResourceThenCallIsRejected()
    {
        using Direct3D9Device device = CreateDevice(Devtype.Hal);
        TestResource resource = new(device.ResourceManager, static () => { });
        resource.SetAsEvictable();
        resource.Dispose();
        uint depth = device.EnterUseContext();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.Use(resource));
        device.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenDisposedDeviceUsesResourceThenCallIsRejectedBeforeResourceStateIsRead()
    {
        Direct3D9Device device = CreateDevice(Devtype.Hal);
        Direct3D9ResourceManager manager = device.ResourceManager;
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.Use(null!));
        Assert.AreEqual(0, manager.ResourceCount);
    }

    [TestMethod]
    public void WhenDisposingDeviceThenClosingChainRunsInsideDeviceEntryOnce()
    {
        Direct3D9Device? device = null;
        int releaseCount = 0;
        int disposedCount = 0;
        bool releaseWasEntered = false;
        bool releaseWasProtected = false;
        device = CreateDevice(Devtype.Hal, D3D9.CreateMultithreaded, _ => disposedCount++);
        Direct3D9ResourceManager resourceManager = device.ResourceManager;
        using TestResource resource = new(resourceManager, () =>
        {
            releaseCount++;
            releaseWasEntered = device.IsEntered();
            releaseWasProtected = device.IsProtected(true);
        });

        device.Dispose();
        device.Dispose();

        Assert.AreEqual((1, 1, true, true, false, false, 0),
            (releaseCount, disposedCount, releaseWasEntered, releaseWasProtected,
                resource.IsValid, resource.IsManaged, resourceManager.ResourceCount));
    }

    private static Direct3D9Device CreateDevice(
        Devtype deviceType,
        uint behaviorFlags = 0,
        Action<Direct3D9Device>? disposedNotification = null)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            deviceType,
            behaviorFlags,
            default,
            disposedNotification: disposedNotification);
    }

    private sealed class TestResource : Direct3D9Resource
    {
        private readonly Action _releaseAction;

        internal TestResource(Direct3D9ResourceManager manager, Action releaseAction)
            : base(manager)
        {
            _releaseAction = releaseAction;
        }

        protected override void ReleaseD3DResources()
        {
            _releaseAction();
        }
    }
}
