using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class Direct3D9ResourceManagerTests
{
    [TestMethod]
    public void WhenRegisteringResourceThenManagerTracksIt()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);

        Assert.AreEqual(1, manager.ResourceCount);
    }

    [TestMethod]
    public void WhenDestroyingResourceThenReleaseOccursBeforeDetaching()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);

        manager.DestroyResource(resource);

        Assert.IsTrue(resource.WasManagedDuringRelease);
    }

    [TestMethod]
    public void WhenDestroyingAllResourcesThenEveryResourceIsReleased()
    {
        Direct3D9ResourceManager manager = new();
        TestResource first = new(manager);
        TestResource second = new(manager);

        manager.DestroyAllResources();

        Assert.IsTrue(first.IsReleased && second.IsReleased);
    }

    [TestMethod]
    public void WhenMarkingDeviceUnusableThenManagerIsNotifiedBeforeResourcesAreReleased()
    {
        List<string> events = [];
        using Direct3D9Device device = CreateDevice(_ => events.Add("manager"));
        using TestResource resource = new(device.ResourceManager, () => events.Add("resource"));

        device.MarkUnusable();

        CollectionAssert.AreEqual(new[] { "manager", "resource" }, events);
    }

    [TestMethod]
    public void WhenMarkingDeviceUnusableTwiceThenNotificationOccursOnce()
    {
        int notificationCount = 0;
        using Direct3D9Device device = CreateDevice(_ => notificationCount++);

        device.MarkUnusable();
        device.MarkUnusable();

        Assert.AreEqual(1, notificationCount);
    }

    private static Direct3D9Device CreateDevice(Action<Direct3D9Device> unusableNotification)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            unusableNotification);
    }

    private sealed class TestResource : Direct3D9Resource
    {
        private readonly Action? _releaseAction;

        internal TestResource(Direct3D9ResourceManager manager, Action? releaseAction = null)
            : base(manager)
        {
            _releaseAction = releaseAction;
        }

        internal bool WasManagedDuringRelease { get; private set; }

        protected override void ReleaseD3DResources()
        {
            WasManagedDuringRelease = IsManaged;
            _releaseAction?.Invoke();
        }
    }
}
