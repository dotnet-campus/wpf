using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class Direct3D9ResourceManagerTests
{
    [TestMethod]
    public void WhenManagerBelongsToDeviceThenDeviceReturnsOwnerIdentity()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true));

        Assert.AreSame(device, device.ResourceManager.Device);
    }

    [TestMethod]
    public void WhenManagerHasNoDeviceThenDeviceIsNull()
    {
        Direct3D9ResourceManager manager = new();

        Assert.IsNull(manager.Device);
    }

    [TestMethod]
    public void WhenResourceIsManagedThenManagerReturnsBorrowedOwnerIdentity()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);

        Assert.AreSame(manager, resource.ManagerForTest);
    }

    [TestMethod]
    public void WhenResourceManagerBelongsToDeviceThenResourceDeviceReturnsBorrowedOwnerIdentity()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        using TestResource resource = new(device.ResourceManager);

        Assert.AreSame(device, resource.DeviceForTest);
    }

    [TestMethod]
    public void WhenResourceReleasesD3DResourcesThenManagerRemainsAvailableDuringCallback()
    {
        Direct3D9ResourceManager manager = new();
        Direct3D9ResourceManager? managerDuringRelease = null;
        TestResource? resource = null;
        resource = new TestResource(manager, () => managerDuringRelease = resource.ManagerForTest);

        manager.DestroyResource(resource);

        Assert.AreSame(manager, managerDuringRelease);
    }

    [TestMethod]
    public void WhenResourceIsDetachedThenManagerAccessIsRejected()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);
        manager.DestroyResource(resource);

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = resource.ManagerForTest);
    }

    [TestMethod]
    public void WhenResourceIsDetachedThenDeviceAccessIsRejected()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        TestResource resource = new(device.ResourceManager);
        device.ResourceManager.DestroyResource(resource);

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = resource.DeviceForTest);
    }

    [TestMethod]
    public void WhenRegisteringResourceThenManagerTracksIt()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);

        Assert.AreEqual(1, manager.ResourceCount);
    }

    [TestMethod]
    public void WhenNonEvictableResourceIsRegisteredThenItIsActive()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);

        Assert.IsTrue(manager.IsResourceActive(resource));
    }

    [TestMethod]
    public void WhenPreviousFrameResourceIsRegisteredThenItIsActive()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();
        manager.EndFrame();

        Assert.IsTrue(manager.IsResourceActive(resource));
    }

    [TestMethod]
    public void WhenCurrentFrameResourceIsNotInUseThenItIsActive()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();

        Assert.IsTrue(manager.IsResourceActive(resource));
    }

    [TestMethod]
    public void WhenCurrentFrameResourceIsInUseThenItIsActive()
    {
        Direct3D9ResourceManager manager = new();
        uint depth = manager.EnterUseContext();
        using TestResource resource = new(manager, isEvictable: true);

        Assert.IsTrue(manager.IsResourceActive(resource));

        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenResourceBelongsToAnotherManagerThenItIsNotActive()
    {
        Direct3D9ResourceManager manager = new();
        Direct3D9ResourceManager foreignManager = new();
        using TestResource resource = new(foreignManager);

        Assert.IsFalse(manager.IsResourceActive(resource));
    }

    [TestMethod]
    public void WhenResourceMovesAcrossFramesThenItRemainsActive()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();
        manager.EndFrame();
        uint depth = manager.EnterUseContext();
        manager.Use(resource);
        manager.ExitUseContext(depth);

        Assert.IsTrue(manager.IsResourceActive(resource));
    }

    [TestMethod]
    public void WhenResourceIsQueuedForReleaseThenItIsNotActive()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        manager.UnusedNotification(resource);

        Assert.IsFalse(manager.IsResourceActive(resource));
    }

    [TestMethod]
    public void WhenResourceIsQueuedForDelayedReleaseThenItIsNotActive()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager, requiresDelayedRelease: true);
        manager.UnusedNotification(resource);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        Assert.IsFalse(manager.IsResourceActive(resource));
    }

    [TestMethod]
    public void WhenResourceIsDisposedThenItIsNotActive()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);

        resource.Dispose();

        Assert.IsFalse(manager.IsResourceActive(resource));
    }

    [TestMethod]
    public void WhenResourceIsEvictedThenItIsNotActive()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();

        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, isSoftwareDevice: false);

        Assert.IsFalse(manager.IsResourceActive(resource));
    }

    [TestMethod]
    public void WhenNestedDestructionIsPendingThenResourceIsNotActive()
    {
        Direct3D9ResourceManager manager = new();
        bool? wasNestedResourceActive = null;
        TestResource nested = new(manager);
        TestResource outer = new(manager, () =>
        {
            nested.Dispose();
            wasNestedResourceActive = manager.IsResourceActive(nested);
        });

        outer.Dispose();

        Assert.IsFalse(wasNestedResourceActive);
    }

    [TestMethod]
    public void WhenDestroyingAllResourcesThenResourcesAreNotActive()
    {
        Direct3D9ResourceManager manager = new();
        TestResource first = new(manager);
        TestResource second = new(manager);

        manager.DestroyAllResources();

        Assert.IsFalse(manager.IsResourceActive(first) || manager.IsResourceActive(second));
    }

    [TestMethod]
    public void WhenDeviceClosesThenResourceIsNotActiveInBorrowedManager()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        TestResource resource = new(manager);

        device.Dispose();

        Assert.IsFalse(manager.IsResourceActive(resource));
    }

    [TestMethod]
    public void WhenQueryingResourceActivityRepeatedlyThenManagerAndResourceStateAreUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        uint depth = manager.EnterUseContext();
        using TestResource resource = new(manager, isEvictable: true, resourceSize: 48);
        (int ResourceCount, uint FrameCount, uint VideoMemory, uint ActiveDepth, bool IsValid, bool IsManaged) before =
            (manager.ResourceCount, manager.CompletedFrameCount, manager.TotalVideoMemoryConsumption,
                resource.ActiveUseContextDepth, resource.IsValid, resource.IsManaged);

        bool first = manager.IsResourceActive(resource);
        bool second = manager.IsResourceActive(resource);

        Assert.AreEqual(
            (true, true, before),
            (first, second,
                (manager.ResourceCount, manager.CompletedFrameCount, manager.TotalVideoMemoryConsumption,
                    resource.ActiveUseContextDepth, resource.IsValid, resource.IsManaged)));

        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenManagerIsEmptyThenNoActiveResourcesAreReported()
    {
        Direct3D9ResourceManager manager = new();

        Assert.IsFalse(manager.AreActiveResources());
    }

    [TestMethod]
    public void WhenNonEvictableResourceExistsThenActiveResourcesAreReported()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);

        Assert.IsTrue(manager.AreActiveResources());
    }

    [TestMethod]
    public void WhenPreviousFrameResourceExistsThenActiveResourcesAreReported()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();
        manager.EndFrame();

        Assert.IsTrue(manager.AreActiveResources());
    }

    [TestMethod]
    public void WhenCurrentFrameResourceIsNotInUseThenActiveResourcesAreReported()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();

        Assert.IsTrue(manager.AreActiveResources());
    }

    [TestMethod]
    public void WhenCurrentFrameResourceIsInUseThenActiveResourcesAreReported()
    {
        Direct3D9ResourceManager manager = new();
        uint depth = manager.EnterUseContext();
        using TestResource resource = new(manager, isEvictable: true);

        Assert.IsTrue(manager.AreActiveResources());

        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenOnlyCurrentFrameReleasedResourceExistsThenNoActiveResourcesAreReported()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        manager.UnusedNotification(resource);

        Assert.IsFalse(manager.AreActiveResources());
    }

    [TestMethod]
    public void WhenOnlyDelayedReleasedResourceExistsThenNoActiveResourcesAreReported()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager, requiresDelayedRelease: true);
        manager.UnusedNotification(resource);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        Assert.IsFalse(manager.AreActiveResources());
    }

    [TestMethod]
    public void WhenMixedListsContainReleasedResourcesThenRemainingActiveResourceIsReported()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource released = new(manager, requiresDelayedRelease: true);
        manager.UnusedNotification(released);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        using TestResource active = new(manager);
        active.SetAsEvictable();
        manager.EndFrame();

        Assert.IsTrue(manager.AreActiveResources());
    }

    [TestMethod]
    public void WhenNestedDestructionIsPendingThenInvalidResourcesAreNotReportedAsActive()
    {
        Direct3D9ResourceManager manager = new();
        bool? wereActiveDuringRelease = null;
        TestResource nested = new(manager);
        TestResource outer = new(manager, () =>
        {
            nested.Dispose();
            wereActiveDuringRelease = manager.AreActiveResources();
        });

        outer.Dispose();

        Assert.AreEqual((false, false, false, 0),
            (wereActiveDuringRelease, outer.IsManaged, nested.IsManaged, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenLastActiveResourceIsDisposedThenNoActiveResourcesAreReported()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);

        resource.Dispose();

        Assert.IsFalse(manager.AreActiveResources());
    }

    [TestMethod]
    public void WhenLastActiveResourceIsEvictedThenNoActiveResourcesAreReported()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();

        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, isSoftwareDevice: false);

        Assert.IsFalse(manager.AreActiveResources());
    }

    [TestMethod]
    public void WhenDestroyingAllResourcesThenNoActiveResourcesAreReported()
    {
        Direct3D9ResourceManager manager = new();
        _ = new TestResource(manager);
        _ = new TestResource(manager);

        manager.DestroyAllResources();

        Assert.IsFalse(manager.AreActiveResources());
    }

    [TestMethod]
    public void WhenDeviceClosesThenNoActiveResourcesAreReportedByBorrowedManager()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        _ = new TestResource(manager);

        device.Dispose();

        Assert.IsFalse(manager.AreActiveResources());
    }

    [TestMethod]
    public void WhenQueryingActiveResourcesThenManagerAndResourceStateAreUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        List<string> releases = [];
        TestResource first = new(manager, () => releases.Add("first"));
        TestResource second = new(manager, () => releases.Add("second"));
        uint depth = manager.EnterUseContext();
        second.SetAsEvictable();

        bool result = manager.AreActiveResources();

        Assert.AreEqual(
            (true, true, depth, true, true, true, true, 2),
            (result, manager.IsInUseContext, second.ActiveUseContextDepth,
                first.IsValid, second.IsValid, first.IsManaged, second.IsManaged, manager.ResourceCount));

        manager.DestroyAllResources();
        CollectionAssert.AreEqual(new[] { "first", "second" }, releases);
        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenRegisteringEvictableResourceThenRegistrationCountsAsUseAtCurrentDepth()
    {
        Direct3D9ResourceManager manager = new();
        uint depth = manager.EnterUseContext();
        using TestResource resource = new(manager, isEvictable: true);

        Assert.AreEqual(depth, resource.ActiveUseContextDepth);

        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenRegisteringEvictableResourcesThenRegistrationPreservesUseOrder()
    {
        Direct3D9ResourceManager manager = new();
        List<string> releases = [];
        uint depth = manager.EnterUseContext();
        TestResource first = new(manager, () => releases.Add("first"), isEvictable: true);
        TestResource second = new(manager, () => releases.Add("second"), isEvictable: true);
        manager.ExitUseContext(depth);

        manager.DestroyAllResources();

        CollectionAssert.AreEqual(new[] { "first", "second" }, releases);
    }

    [TestMethod]
    public void WhenRegisteringEvictableResourceOutsideUseContextThenRegistrationIsRejected()
    {
        Direct3D9ResourceManager manager = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => new TestResource(manager, isEvictable: true));
    }

    [TestMethod]
    public void WhenRegisteringResourceTwiceThenSecondRegistrationIsRejected()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);

        Assert.ThrowsExactly<InvalidOperationException>(() => manager.RegisterResource(resource));
    }

    [TestMethod]
    public void WhenRegisteringOrdinaryResourceThenIdentityCollectionAndMemoryAreEstablished()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager, resourceSize: 32);

        Assert.AreEqual(
            (manager, true, true, 1, 32u, 32u, 0u),
            (resource.Manager, resource.IsManaged, resource.IsValid, manager.ResourceCount,
                manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption,
                resource.ActiveUseContextDepth));
    }

    [TestMethod]
    public void WhenRegisteringEvictableResourceInUseContextThenAllRegistrationStateIsEstablished()
    {
        Direct3D9ResourceManager manager = new();
        uint depth = manager.EnterUseContext();
        using TestResource resource = new(manager, isEvictable: true, resourceSize: 48);

        Assert.AreEqual(
            (manager, true, true, true, 1, 48u, 48u, depth, true),
            (resource.Manager, resource.IsManaged, resource.IsValid, resource.IsEvictable,
                manager.ResourceCount, manager.TotalVideoMemoryConsumption,
                manager.PeakVideoMemoryConsumption, resource.ActiveUseContextDepth,
                manager.IsResourceActive(resource)));

        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenRegisteringInvalidResourceThenRegistrationStateRemainsUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager, resourceSize: 32);
        resource.InvalidateForTest();
        (int Count, uint Total, uint Peak, uint Depth, bool Managed) before =
            (manager.ResourceCount, manager.TotalVideoMemoryConsumption,
                manager.PeakVideoMemoryConsumption, manager.CurrentUseContextDepth, resource.IsManaged);

        Assert.ThrowsExactly<ObjectDisposedException>(() => manager.RegisterResource(resource));

        Assert.AreEqual(before,
            (manager.ResourceCount, manager.TotalVideoMemoryConsumption,
                manager.PeakVideoMemoryConsumption, manager.CurrentUseContextDepth, resource.IsManaged));
    }

    [TestMethod]
    public void WhenRegistrationWouldOverflowVideoMemoryThenManagerStateRemainsUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        System.Reflection.PropertyInfo totalConsumptionProperty = typeof(Direct3D9ResourceManager)
            .GetProperty(
                nameof(Direct3D9ResourceManager.TotalVideoMemoryConsumption),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        totalConsumptionProperty.SetValue(manager, uint.MaxValue);

        Assert.ThrowsExactly<OverflowException>(() => new TestResource(manager, resourceSize: 1));

        Assert.AreEqual((0, uint.MaxValue, 0u, 0u),
            (manager.ResourceCount, manager.TotalVideoMemoryConsumption,
                manager.PeakVideoMemoryConsumption, manager.CurrentUseContextDepth));
        totalConsumptionProperty.SetValue(manager, 0u);
    }

    [TestMethod]
    public void WhenDestroyingRegisteredEvictableResourceThenItReleasesAndDetachesOnce()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        uint depth = manager.EnterUseContext();
        TestResource resource = new(manager, () => releaseCount++, isEvictable: true);

        manager.DestroyResource(resource);
        manager.DestroyResource(resource);

        Assert.AreEqual((1, false, 0), (releaseCount, resource.IsManaged, manager.ResourceCount));
        manager.ExitUseContext(depth);
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
    public void WhenDestroyingAllResourcesThenNativeListOrderIsPreserved()
    {
        Direct3D9ResourceManager manager = new();
        List<string> releases = [];
        TestResource previousFrameReleased = new(manager, () => releases.Add("previous-frame-released"), requiresDelayedRelease: true);
        manager.UnusedNotification(previousFrameReleased);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        TestResource nonEvictable = new(manager, () => releases.Add("non-evictable"));
        TestResource previousFrame = new(manager, () => releases.Add("previous-frame"));
        previousFrame.SetAsEvictable();
        manager.EndFrame();
        TestResource currentFrameNotInUse = new(manager, () => releases.Add("current-frame-not-in-use"));
        currentFrameNotInUse.SetAsEvictable();
        uint depth = manager.EnterUseContext();
        TestResource currentFrameInUse = new(manager, () => releases.Add("current-frame-in-use"));
        currentFrameInUse.SetAsEvictable();
        TestResource currentFrameReleased = new(manager, () => releases.Add("current-frame-released"));
        manager.UnusedNotification(currentFrameReleased);
        TestResource currentFrameEvictableReleased = new(manager, () => releases.Add("current-frame-evictable-released"));
        currentFrameEvictableReleased.SetAsEvictable();
        manager.UnusedNotification(currentFrameEvictableReleased);

        manager.DestroyAllResources();

        CollectionAssert.AreEqual(
            new[]
            {
                "previous-frame-released",
                "non-evictable",
                "previous-frame",
                "current-frame-not-in-use",
                "current-frame-in-use",
                "current-frame-evictable-released",
                "current-frame-released"
            },
            releases);
        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenDestroyingAllResourcesAndReleaseQueuesActiveResourceThenFinalFlushLeavesNoRegistration()
    {
        Direct3D9ResourceManager manager = new();
        int queuedReleaseCount = 0;
        TestResource queued = new(manager, () => queuedReleaseCount++);
        TestResource previousFrameReleased = new(
            manager,
            () => manager.UnusedNotification(queued),
            requiresDelayedRelease: true);
        manager.UnusedNotification(previousFrameReleased);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        manager.DestroyAllResources();

        Assert.AreEqual((1, true, false, 0),
            (queuedReleaseCount, queued.IsReleased, queued.IsManaged, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenDestroyingAllResourcesInsideUseContextThenUseContextStateIsUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        uint depth = manager.EnterUseContext();
        TestResource resource = new(manager);
        resource.SetAsEvictable();

        manager.DestroyAllResources();

        Assert.AreEqual((true, 0u, 0),
            (manager.IsInUseContext, resource.ActiveUseContextDepth, manager.ResourceCount));
        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenResourceReleaseRequestsAnotherDestructionThenCurrentResourceIsDetachedFirst()
    {
        Direct3D9ResourceManager manager = new();
        TestResource? first = null;
        bool firstWasDetachedBeforeSecondRelease = false;
        TestResource second = new(manager, () => firstWasDetachedBeforeSecondRelease = !first!.IsManaged);
        first = new TestResource(manager, second.Dispose);

        manager.DestroyResource(first);

        Assert.IsTrue(firstWasDetachedBeforeSecondRelease && first.IsReleased && second.IsReleased);
    }

    [TestMethod]
    public void WhenDestroyingAllResourcesAndReleaseRequestsAnotherDestructionThenEveryResourceReleasesOnce()
    {
        Direct3D9ResourceManager manager = new();
        int firstReleaseCount = 0;
        int secondReleaseCount = 0;
        int thirdReleaseCount = 0;
        TestResource second = new(manager, () => secondReleaseCount++);
        TestResource first = new(manager, () =>
        {
            firstReleaseCount++;
            second.Dispose();
        });
        TestResource third = new(manager, () => thirdReleaseCount++);

        manager.DestroyAllResources();

        Assert.AreEqual((1, 1, 1, 0), (firstReleaseCount, secondReleaseCount, thirdReleaseCount, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenBulkDestructionReleasesResourceThenCallbackObservesInvalidResourceStillManaged()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);

        manager.DestroyAllResources();

        Assert.AreEqual((false, true, false, 0),
            (resource.WasValidDuringRelease, resource.WasManagedDuringRelease, resource.IsManaged, manager.ResourceCount));
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

    [TestMethod]
    public void WhenReleasingResourceThenItIsInvalidBeforeD3DResourcesAreReleased()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);

        resource.Dispose();

        Assert.IsFalse(resource.WasValidDuringRelease);
    }

    [TestMethod]
    public void WhenResourceBecomesUnusableThenItIsDetachedBeforeLastFrameResourcesAreReleased()
    {
        Direct3D9ResourceManager manager = new();
        List<string> releases = [];
        TestResource previousFrame = new(manager, () => releases.Add("previous-frame"), requiresDelayedRelease: true);
        manager.UnusedNotification(previousFrame);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        TestResource unusable = new(manager, () => releases.Add("unusable"));

        unusable.Dispose();

        Assert.AreEqual(("unusable,previous-frame", false, false, 0),
            (string.Join(',', releases), unusable.IsManaged, previousFrame.IsManaged, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenUnusableReleaseQueuesCurrentFrameResourceThenItWaitsForCurrentFrameFlush()
    {
        Direct3D9ResourceManager manager = new();
        TestResource queued = new(manager);
        TestResource previousFrame = new(manager, requiresDelayedRelease: true);
        manager.UnusedNotification(previousFrame);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        TestResource unusable = new(manager, () => manager.UnusedNotification(queued));

        unusable.Dispose();
        bool queuedWasRetained = queued.IsValid;
        uint currentFrameCount = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithoutDelay);

        Assert.AreEqual((true, 1u, 1u, 0),
            (queuedWasRetained, currentFrameCount, manager.ReleasedResourcesFromLastFrameDestroyCount,
                manager.ResourceCount));
    }

    [TestMethod]
    public void WhenNestedUnusableResourceIsQueuedThenItIsDestroyedBeforeLastFrameResources()
    {
        Direct3D9ResourceManager manager = new();
        List<string> releases = [];
        TestResource previousFrame = new(manager, () => releases.Add("previous-frame"), requiresDelayedRelease: true);
        manager.UnusedNotification(previousFrame);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        TestResource nested = new(manager, () => releases.Add("nested"));
        TestResource outer = new(manager, () =>
        {
            releases.Add("outer");
            nested.Dispose();
        });

        outer.Dispose();

        Assert.AreEqual(
            ("outer,nested,previous-frame", 0, 0, 0, false, false, false),
            (string.Join(',', releases), manager.ResourceCount, manager.DelayedReleasedResourceCount,
                manager.PendingResourceDestructionCount, outer.IsManaged, nested.IsManaged, previousFrame.IsManaged));
    }

    [TestMethod]
    public void WhenResourceBecomesUnusableThenUseContextStateIsUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);
        uint depth = manager.EnterUseContext();

        resource.Dispose();

        Assert.IsTrue(manager.IsInUseContext);
        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenDisposingValidResourceThenReleaseObservesInvalidResourceWithManagerAndDeviceIdentity()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        (bool IsValid, Direct3D9ResourceManager? Manager, Direct3D9Device? Device, int ResourceCount, uint Memory)? releaseState = null;
        TestResource? resource = null;
        resource = new TestResource(manager, () => releaseState =
            (resource.IsValid, resource.Manager, resource.Device, manager.ResourceCount, manager.TotalVideoMemoryConsumption),
            resourceSize: 32);

        resource.Dispose();

        Assert.AreEqual(
            (false, manager, device, 1, 32u, false, 0, 0u),
            (releaseState?.IsValid, releaseState?.Manager, releaseState?.Device, releaseState?.ResourceCount,
                releaseState?.Memory, resource.IsManaged, manager.ResourceCount, manager.TotalVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenDisposingEvictableResourcesFromEachActiveStateThenEachReleasesAndDetachesOnce()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        TestResource previousFrame = new(manager, () => releaseCount++);
        previousFrame.SetAsEvictable();
        manager.EndFrame();
        TestResource notInUse = new(manager, () => releaseCount++);
        notInUse.SetAsEvictable();
        uint depth = manager.EnterUseContext();
        TestResource inUse = new(manager, () => releaseCount++, isEvictable: true);

        previousFrame.Dispose();
        notInUse.Dispose();
        inUse.Dispose();
        previousFrame.Dispose();
        notInUse.Dispose();
        inUse.Dispose();

        Assert.AreEqual(
            (3, 0, false, false, false, true),
            (releaseCount, manager.ResourceCount, previousFrame.IsManaged, notInUse.IsManaged, inUse.IsManaged,
                manager.IsInUseContext));
        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenDisposingAlreadyInvalidOrDetachedResourceThenExistingLifetimeResultIsPreserved()
    {
        Direct3D9ResourceManager manager = new();
        int invalidReleaseCount = 0;
        int detachedReleaseCount = 0;
        TestResource invalid = new(manager, () => invalidReleaseCount++);
        TestResource detached = new(manager, () => detachedReleaseCount++);
        invalid.InvalidateFromManager();
        manager.DestroyResource(detached);

        invalid.Dispose();
        invalid.Dispose();
        detached.Dispose();
        detached.Dispose();

        Assert.AreEqual(
            (0, true, 1, false, 1),
            (invalidReleaseCount, invalid.IsManaged, detachedReleaseCount, detached.IsManaged, manager.ResourceCount));
        manager.DestroyResource(invalid);
    }

    [TestMethod]
    public void WhenDisposingResourceTwiceThenItRemainsInvalidAndReleasesOnce()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        TestResource resource = new(manager, () => releaseCount++);

        resource.Dispose();
        resource.Dispose();

        Assert.IsTrue(!resource.IsValid && !resource.IsManaged && releaseCount == 1 && manager.ResourceCount == 0);
    }

    [TestMethod]
    public void WhenGettingDeviceResourceManagerRepeatedlyThenSameEmbeddedInstanceIsReturned()
    {
        using Direct3D9Device device = CreateDevice(_ => { });

        Direct3D9ResourceManager first = device.ResourceManager;
        Direct3D9ResourceManager second = device.ResourceManager;

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void WhenGettingResourceIdentitiesRepeatedlyThenSameManagerAndDeviceAreReturned()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        using TestResource resource = new(device.ResourceManager);

        Assert.AreEqual(
            (resource.Manager, resource.Device),
            (resource.Manager, resource.Device));
    }

    [TestMethod]
    public void WhenResourcesBelongToDifferentDevicesThenTheirIdentitiesDiffer()
    {
        using Direct3D9Device firstDevice = CreateDevice(_ => { });
        using Direct3D9Device secondDevice = CreateDevice(_ => { });
        using TestResource firstResource = new(firstDevice.ResourceManager);
        using TestResource secondResource = new(secondDevice.ResourceManager);

        Assert.IsTrue(
            !ReferenceEquals(firstResource.Manager, secondResource.Manager) &&
            !ReferenceEquals(firstResource.Device, secondResource.Device));
    }

    [TestMethod]
    public void WhenGettingResourceIdentitiesThenManagerDeviceAndResourceStateAreUnchanged()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        TestResource resource = new(manager, resourceSize: 48);
        uint depth = manager.EnterUseContext();
        resource.SetAsEvictable();
        (int ResourceCount, uint Memory, uint Frame, uint ActiveDepth) before =
            (manager.ResourceCount, manager.TotalVideoMemoryConsumption, manager.CompletedFrameCount,
                resource.ActiveUseContextDepth);

        Direct3D9ResourceManager resultManager = resource.Manager;
        Direct3D9Device resultDevice = resource.Device;

        Assert.AreEqual(
            (manager, device, before, false, true, true, true),
            (resultManager, resultDevice,
                (manager.ResourceCount, manager.TotalVideoMemoryConsumption, manager.CompletedFrameCount,
                    resource.ActiveUseContextDepth),
                device.IsEntered(), manager.IsInUseContext, resource.IsValid, resource.IsEvictable));
        manager.ExitUseContext(depth);
        resource.Dispose();
    }

    [TestMethod]
    public void WhenResourceIsDisposedThenIdentitiesRemainAvailableDuringReleaseAndAreDetachedAfterward()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        Direct3D9ResourceManager? managerDuringRelease = null;
        Direct3D9Device? deviceDuringRelease = null;
        TestResource? resource = null;
        resource = new TestResource(manager, () =>
        {
            managerDuringRelease = resource!.Manager;
            deviceDuringRelease = resource.Device;
        });

        resource.Dispose();

        Assert.AreEqual((manager, device, false), (managerDuringRelease, deviceDuringRelease, resource.IsManaged));
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = resource.Manager);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = resource.Device);
    }

    [TestMethod]
    public void WhenDestroyingEvictableResourcesInBatchThenIdentitiesDetachWithoutChangingOwnership()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        List<(Direct3D9ResourceManager Manager, Direct3D9Device Device)> releaseIdentities = [];
        TestResource? first = null;
        TestResource? second = null;
        first = new TestResource(manager, () => releaseIdentities.Add((first!.Manager, first.Device)));
        second = new TestResource(manager, () => releaseIdentities.Add((second!.Manager, second.Device)));
        first.SetAsEvictable();
        second.SetAsEvictable();

        manager.DestroyAllResources();

        Assert.AreEqual(
            ((manager, device), (manager, device), false, false, 0),
            (releaseIdentities[0], releaseIdentities[1], first.IsManaged, second.IsManaged, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenDeviceClosesThenResourceIdentityIsDetachedAndBorrowedManagerCannotExtendLifetime()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        TestResource resource = new(manager);

        device.Dispose();

        Assert.IsFalse(resource.IsManaged);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = resource.Device);
        Assert.ThrowsExactly<ObjectDisposedException>(() => new TestResource(manager));
    }

    [TestMethod]
    public void WhenGettingDeviceResourceManagerThenDeviceAndManagerStateAreUnchanged()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        using TestResource resource = new(manager);
        manager.UnusedNotification(resource);
        (int ResourceCount, uint FrameCount, uint PreviousDestroyCount, uint ImmediateDestroyCount, uint DelayedDestroyCount) before =
            (manager.ResourceCount, manager.CompletedFrameCount, manager.ReleasedResourcesFromLastFrameDestroyCount,
                manager.ImmediateResourceDestroyCount, manager.DelayedResourceDestroyCount);

        Direct3D9ResourceManager result = device.ResourceManager;

        Assert.AreEqual(
            (manager, before, false, false, true, true),
            (result,
                (manager.ResourceCount, manager.CompletedFrameCount, manager.ReleasedResourcesFromLastFrameDestroyCount,
                    manager.ImmediateResourceDestroyCount, manager.DelayedResourceDestroyCount),
                device.IsEntered(), manager.IsInUseContext, resource.IsValid, resource.IsManaged));
    }

    [TestMethod]
    public void WhenGettingDeviceResourceManagerInsideExistingScopesThenScopesRemainActive()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        device.Enter();
        uint depth = device.EnterUseContext();

        Direct3D9ResourceManager manager = device.ResourceManager;

        Assert.AreEqual((true, true, depth), (device.IsEntered(), manager.IsInUseContext, depth));
        device.ExitUseContext(depth);
        device.Leave();
    }

    [TestMethod]
    public void WhenGettingDeviceResourceManagerAfterDeviceDisposalThenAccessIsRejected()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.ResourceManager);
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenPreviouslyBorrowedResourceManagerIsClosedByDevice()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => new TestResource(manager));
    }

    [TestMethod]
    public void WhenMarkingDeviceUnusableThenResourcesRemainInvalidAndReleaseOnce()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        int releaseCount = 0;
        TestResource resource = new(device.ResourceManager, () => releaseCount++);

        device.MarkUnusable();
        device.MarkUnusable();
        resource.Dispose();

        Assert.IsTrue(!resource.IsValid && !resource.IsManaged && releaseCount == 1 && device.ResourceCount == 0);
    }

    [TestMethod]
    public void WhenDisposingDeviceThenResourcesRemainInvalidAndReleaseOnce()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        int releaseCount = 0;
        TestResource resource = new(device.ResourceManager, () => releaseCount++);

        device.Dispose();
        device.Dispose();
        resource.Dispose();

        Assert.IsTrue(!resource.IsValid && !resource.IsManaged && releaseCount == 1 && device.ResourceCount == 0);
    }

    [TestMethod]
    public void WhenDeviceInvalidatesSurfaceAndSwapChainThenBothRemainUnavailable()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        using Direct3D9Surface surface = new(device.ResourceManager, null);
        using Direct3D9SwapChain swapChain = new(device.ResourceManager, null);

        device.MarkUnusable();

        Assert.IsFalse(surface.IsValid || swapChain.IsValid);
    }

    [TestMethod]
    public void WhenCleaningFreedResourcesThenPreviousAndCurrentResourcesAreDestroyedWithoutDelay()
    {
        using Direct3D9Device device = CreateDevice(_ => { });

        device.CleanupFreedResources();

        Assert.AreEqual(
            (1u, 1u, 0u),
            (device.ResourceManager.ReleasedResourcesFromLastFrameDestroyCount,
                device.ResourceManager.ImmediateResourceDestroyCount,
                device.ResourceManager.DelayedResourceDestroyCount));
    }

    [TestMethod]
    public void WhenCleaningFreedResourcesThenPreviousFrameResourcesAreDestroyedBeforeCurrentResources()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        List<string> releases = [];
        TestResource previousFirst = new(manager, () => releases.Add("previous-first"), requiresDelayedRelease: true);
        TestResource previousLast = new(manager, () => releases.Add("previous-last"), requiresDelayedRelease: true);
        manager.UnusedNotification(previousFirst);
        manager.UnusedNotification(previousLast);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        TestResource currentFirst = new(manager, () => releases.Add("current-first"));
        TestResource currentDelayed = new(manager, () => releases.Add("current-delayed"), requiresDelayedRelease: true);
        TestResource currentLast = new(manager, () => releases.Add("current-last"));
        manager.UnusedNotification(currentFirst);
        manager.UnusedNotification(currentDelayed);
        manager.UnusedNotification(currentLast);

        device.CleanupFreedResources();

        Assert.AreEqual(
            "previous-first,previous-last,current-last,current-delayed,current-first",
            string.Join(',', releases));
    }

    [TestMethod]
    public void WhenCleaningFreedResourcesThenActiveAndInUseResourcesRemainValidWithoutAdvancingFrame()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        TestResource active = new(manager);
        uint useContextDepth = manager.EnterUseContext();
        TestResource inUse = new(manager, isEvictable: true);
        TestResource released = new(manager, requiresDelayedRelease: true);
        manager.UnusedNotification(released);
        uint completedFrameCount = manager.CompletedFrameCount;

        device.CleanupFreedResources();

        Assert.AreEqual(
            (true, true, true, completedFrameCount, 0u, 1u, 1u),
            (active.IsValid, inUse.IsValid, released.IsReleased, manager.CompletedFrameCount,
                manager.DelayedResourceDestroyCount, manager.ReleasedResourcesFromLastFrameDestroyCount,
                manager.ImmediateResourceDestroyCount));
        manager.ExitUseContext(useContextDepth);
    }

    [TestMethod]
    public void WhenCleaningFreedResourcesRepeatedlyThenEachResourceIsReleasedOnce()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        int previousReleaseCount = 0;
        int currentReleaseCount = 0;
        TestResource previous = new(manager, () => previousReleaseCount++, requiresDelayedRelease: true);
        manager.UnusedNotification(previous);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        TestResource current = new(manager, () => currentReleaseCount++);
        manager.UnusedNotification(current);

        device.CleanupFreedResources();
        device.CleanupFreedResources();

        Assert.AreEqual((1, 1, 0), (previousReleaseCount, currentReleaseCount, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenAdvanceFrameReleaseFailsThenFrameIsCommittedAndNextFrameRetriesUnconsumedResources()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        int retainedReleaseCount = 0;
        TestResource retained = new(manager, () => retainedReleaseCount++, requiresDelayedRelease: true);
        TestResource failing = new(
            manager,
            () => throw new InvalidOperationException("Release failed."),
            requiresDelayedRelease: true);
        manager.UnusedNotification(failing);
        manager.UnusedNotification(retained);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        Assert.ThrowsExactly<InvalidOperationException>(() => device.AdvanceFrame(1));
        device.AdvanceFrame(1);
        device.AdvanceFrame(2);

        Assert.AreEqual(
            (2u, 2u, 2u, 1, 0, false),
            (manager.CompletedFrameCount,
                manager.ReleasedResourcesFromLastFrameDestroyCount,
                manager.DelayedResourceDestroyCount,
                retainedReleaseCount,
                manager.ResourceCount,
                retained.IsManaged));
    }

    [TestMethod]
    public void WhenDestroyingReleasedResourcesWithDelayThenOnlyDelayedResourcesMoveToLastFrame()
    {
        Direct3D9ResourceManager manager = new();
        List<string> releases = [];
        TestResource first = new(manager, () => releases.Add("first"));
        TestResource delayed = new(manager, () => releases.Add("delayed"), requiresDelayedRelease: true);
        TestResource last = new(manager, () => releases.Add("last"));
        manager.UnusedNotification(first);
        manager.UnusedNotification(delayed);
        manager.UnusedNotification(last);

        uint currentFrameCount = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        uint previousFrameCount = manager.DestroyReleasedResourcesFromLastFrame();

        Assert.AreEqual((2u, 1u, "last,first,delayed", 0),
            (currentFrameCount, previousFrameCount, string.Join(',', releases), manager.ResourceCount));
    }

    [TestMethod]
    public void WhenSeveralDelayedResourcesMoveToLastFrameThenMigrationReversesTheFlushedStack()
    {
        Direct3D9ResourceManager manager = new();
        List<string> releases = [];
        TestResource first = new(manager, () => releases.Add("first"), requiresDelayedRelease: true);
        TestResource second = new(manager, () => releases.Add("second"), requiresDelayedRelease: true);
        manager.UnusedNotification(first);
        manager.UnusedNotification(second);

        uint currentFrameCount = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        uint previousFrameCount = manager.DestroyReleasedResourcesFromLastFrame();

        Assert.AreEqual((0u, 2u, "first,second"),
            (currentFrameCount, previousFrameCount, string.Join(',', releases)));
    }

    [TestMethod]
    public void WhenDestroyingReleasedResourcesWithoutDelayThenDelayedResourceIsDestroyedImmediately()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager, requiresDelayedRelease: true);
        manager.UnusedNotification(resource);

        uint count = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithoutDelay);

        Assert.AreEqual((1u, true, 0), (count, resource.IsReleased, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenLastFrameReleaseQueuesCurrentFrameResourceThenNewResourceWaitsForCurrentFrameFlush()
    {
        Direct3D9ResourceManager manager = new();
        TestResource currentFrame = new(manager);
        TestResource previousFrame = new(
            manager,
            () => manager.UnusedNotification(currentFrame),
            requiresDelayedRelease: true);
        manager.UnusedNotification(previousFrame);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        uint previousFrameCount = manager.DestroyReleasedResourcesFromLastFrame();
        bool currentFrameWasRetained = currentFrame.IsValid;
        uint currentFrameCount = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        Assert.AreEqual((1u, true, 1u, false, 0),
            (previousFrameCount, currentFrameWasRetained, currentFrameCount, manager.IsInUseContext, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenCurrentFrameReleaseRequestsNestedDestructionThenNestedResourceWaitsForFlushedBatch()
    {
        Direct3D9ResourceManager manager = new();
        List<string> releases = [];
        TestResource nested = new(manager, () => releases.Add("nested"), resourceSize: 4);
        TestResource first = new(manager, () => releases.Add("first"), resourceSize: 8);
        TestResource last = new(
            manager,
            () =>
            {
                releases.Add("last");
                manager.DestroyResource(nested);
                manager.DestroyResource(nested);
            },
            resourceSize: 16);
        manager.UnusedNotification(first);
        manager.UnusedNotification(last);

        uint count = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithoutDelay);

        Assert.AreEqual((2u, "last,first,nested", 0, 0u),
            (count, string.Join(',', releases), manager.ResourceCount, manager.TotalVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenLastFrameReleaseRequestsNestedDestructionThenNestedResourceWaitsForFlushedBatch()
    {
        Direct3D9ResourceManager manager = new();
        List<string> releases = [];
        TestResource nested = new(manager, () => releases.Add("nested"), resourceSize: 4);
        TestResource first = new(
            manager,
            () =>
            {
                releases.Add("first");
                nested.Dispose();
            },
            requiresDelayedRelease: true,
            resourceSize: 8);
        TestResource last = new(
            manager,
            () => releases.Add("last"),
            requiresDelayedRelease: true,
            resourceSize: 16);
        manager.UnusedNotification(first);
        manager.UnusedNotification(last);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        uint count = manager.DestroyReleasedResourcesFromLastFrame();

        Assert.AreEqual((2u, "first,last,nested", 0, 0u),
            (count, string.Join(',', releases), manager.ResourceCount, manager.TotalVideoMemoryConsumption));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, false)]
    [DataRow(Direct3D9Factory.OutOfMemoryHResult, false)]
    public void WhenFreeingVideoMemoryForUnsupportedFailureThenReleasedQueuesRemainUntouched(
        int hResult,
        bool isSoftwareDevice)
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);
        manager.UnusedNotification(resource);

        bool shouldRetry = manager.FreeSomeVideoMemory(hResult, isSoftwareDevice);

        Assert.AreEqual((false, true, 0u, 0u),
            (shouldRetry, resource.IsValid, manager.ReleasedResourcesFromLastFrameDestroyCount,
                manager.DelayedResourceDestroyCount));
    }

    [TestMethod]
    public void WhenOutOfVideoMemoryAndPreviousFrameResourceIsDestroyedThenCurrentFrameIsNotFlushed()
    {
        Direct3D9ResourceManager manager = new();
        TestResource previousFrame = new(manager, requiresDelayedRelease: true);
        TestResource currentFrame = new(manager);
        manager.UnusedNotification(previousFrame);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        manager.UnusedNotification(currentFrame);

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((true, true, 1u),
            (shouldRetry, currentFrame.IsValid, manager.DelayedResourceDestroyCount));
    }

    [TestMethod]
    public void WhenSoftwareDeviceIsOutOfMemoryThenCurrentFrameResourceIsDestroyedBeforeThirdStage()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);
        manager.UnusedNotification(resource);

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfMemoryHResult, true);

        Assert.AreEqual((true, true, 1u, 1u),
            (shouldRetry, resource.IsReleased, manager.ReleasedResourcesFromLastFrameDestroyCount,
                manager.DelayedResourceDestroyCount));
    }

    [TestMethod]
    public void WhenOutOfVideoMemoryAndCurrentFrameResourceRequiresDelayThenThirdStageDestroysIt()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager, requiresDelayedRelease: true);
        manager.UnusedNotification(resource);

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((true, true, 2u, 1u),
            (shouldRetry, resource.IsReleased, manager.ReleasedResourcesFromLastFrameDestroyCount,
                manager.DelayedResourceDestroyCount));
    }

    [TestMethod]
    public void WhenFirstVideoMemoryStageQueuesCurrentFrameResourceThenItRemainsForLaterFlush()
    {
        Direct3D9ResourceManager manager = new();
        TestResource currentFrame = new(manager);
        TestResource previousFrame = new(
            manager,
            () => manager.UnusedNotification(currentFrame),
            requiresDelayedRelease: true);
        manager.UnusedNotification(previousFrame);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);
        uint laterCount = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        Assert.AreEqual((true, true, 1u, 0),
            (shouldRetry, currentFrame.IsReleased, laterCount, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenOutOfVideoMemoryThenPreviousFrameLruResourceIsTheOnlyActiveResourceEvicted()
    {
        Direct3D9ResourceManager manager = new();
        TestResource leastRecentlyUsed = new(manager);
        TestResource mostRecentlyUsed = new(manager);
        leastRecentlyUsed.SetAsEvictable();
        mostRecentlyUsed.SetAsEvictable();
        manager.EndFrame();

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((true, true, true, false, 1),
            (shouldRetry, leastRecentlyUsed.IsReleased, mostRecentlyUsed.IsValid,
                manager.IsInUseContext, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenActiveResourceIsEvictedThenReleaseObservesInvalidResourceStillManaged()
    {
        Direct3D9ResourceManager manager = new();
        TestResource? resource = null;
        bool wasValidDuringCallback = true;
        bool wasManagedDuringCallback = false;
        resource = new TestResource(manager, () =>
        {
            wasValidDuringCallback = resource.IsValid;
            wasManagedDuringCallback = resource.IsManaged;
        });
        resource.SetAsEvictable();
        manager.EndFrame();

        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((false, true, false, 0),
            (wasValidDuringCallback, wasManagedDuringCallback, resource.IsManaged, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenActiveEvictionReleaseRequestsNestedDestructionThenBothResourcesDetachAfterSingleRelease()
    {
        Direct3D9ResourceManager manager = new();
        int evictedReleaseCount = 0;
        int nestedReleaseCount = 0;
        TestResource nested = new(manager, () => nestedReleaseCount++);
        TestResource evicted = new(manager, () =>
        {
            evictedReleaseCount++;
            nested.Dispose();
        });
        evicted.SetAsEvictable();
        manager.EndFrame();

        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((1, 1, false, false, 0),
            (evictedReleaseCount, nestedReleaseCount, evicted.IsManaged, nested.IsManaged, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenPreviousFramesHaveNoResourceThenCurrentFrameMruResourceIsEvicted()
    {
        Direct3D9ResourceManager manager = new();
        TestResource leastRecentlyUsed = new(manager);
        TestResource mostRecentlyUsed = new(manager);
        leastRecentlyUsed.SetAsEvictable();
        mostRecentlyUsed.SetAsEvictable();

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((true, true, true, 1),
            (shouldRetry, leastRecentlyUsed.IsValid, mostRecentlyUsed.IsReleased, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenReleasedResourceIsDestroyedThenActiveResourceIsNotEvicted()
    {
        Direct3D9ResourceManager manager = new();
        TestResource activeResource = new(manager);
        TestResource releasedResource = new(manager);
        activeResource.SetAsEvictable();
        manager.UnusedNotification(releasedResource);

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((true, true, true, 1),
            (shouldRetry, activeResource.IsValid, releasedResource.IsReleased, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenUnsupportedFailureOccursThenActiveResourcesRemainUntouched()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);
        resource.SetAsEvictable();
        manager.EndFrame();

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.GenericFailureHResult, false);

        Assert.AreEqual((false, true, 1), (shouldRetry, resource.IsValid, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenPreviousAndCurrentFrameCandidatesExistThenPreviousFrameLruIsPreferred()
    {
        Direct3D9ResourceManager manager = new();
        TestResource previousFrame = new(manager, resourceSize: 16);
        previousFrame.SetAsEvictable();
        manager.EndFrame();
        TestResource currentFrame = new(manager, resourceSize: 32);
        currentFrame.SetAsEvictable();

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((true, true, true, 1, 32u),
            (shouldRetry, previousFrame.IsReleased, currentFrame.IsValid, manager.ResourceCount,
                manager.TotalVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenPreviousFrameLruIsReusedThenNextOldestPreviousFrameResourceIsSelected()
    {
        Direct3D9ResourceManager manager = new();
        TestResource reused = new(manager);
        TestResource retained = new(manager);
        reused.SetAsEvictable();
        retained.SetAsEvictable();
        manager.EndFrame();
        uint depth = manager.EnterUseContext();
        manager.Use(reused);
        manager.ExitUseContext(depth);

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((true, true, true, 2ul, 1),
            (shouldRetry, reused.IsValid, retained.IsReleased, reused.LastUsedFrame, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenReleasedPreviousFrameResourceIsFlushedThenCurrentFrameMruBecomesCandidate()
    {
        Direct3D9ResourceManager manager = new();
        int previousReleaseCount = 0;
        TestResource previousFrame = new(manager, () => previousReleaseCount++);
        previousFrame.SetAsEvictable();
        manager.EndFrame();
        previousFrame.Dispose();
        TestResource currentFrameLru = new(manager);
        TestResource currentFrameMru = new(manager);
        currentFrameLru.SetAsEvictable();
        currentFrameMru.SetAsEvictable();

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((true, 1, true, true, 1),
            (shouldRetry, previousReleaseCount, currentFrameLru.IsValid, currentFrameMru.IsReleased,
                manager.ResourceCount));
    }

    [TestMethod]
    public void WhenNoUnusedCandidateExistsThenVideoMemoryStateRemainsUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        using TestResource resource = new(manager, () => releaseCount++, resourceSize: 64);
        uint depth = manager.EnterUseContext();
        resource.SetAsEvictable();

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((false, true, true, depth, 1, 64u, 0),
            (shouldRetry, resource.IsValid, resource.IsManaged, resource.ActiveUseContextDepth,
                manager.ResourceCount, manager.TotalVideoMemoryConsumption, releaseCount));
        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenCleaningFreedResourcesAfterDeviceDisposalThenCallIsRejected()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(device.CleanupFreedResources);
    }

    [TestMethod]
    public void WhenUsingEvictableResourceThenItRemainsOwnedByOutermostUseContext()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();
        uint outerDepth = manager.EnterUseContext();
        manager.Use(resource);
        uint innerDepth = manager.EnterUseContext();

        manager.Use(resource);
        manager.ExitUseContext(innerDepth);

        Assert.AreEqual(outerDepth, resource.ActiveUseContextDepth);
        manager.ExitUseContext(outerDepth);
    }

    [TestMethod]
    public void WhenEnteringNestedEmptyUseContextsThenDepthStrictlyIncreasesAndExitsInReverseOrder()
    {
        Direct3D9ResourceManager manager = new();

        uint outerDepth = manager.EnterUseContext();
        uint innerDepth = manager.EnterUseContext();
        manager.ExitUseContext(innerDepth);
        manager.ExitUseContext(outerDepth);

        Assert.AreEqual((1u, 2u, 0u, false),
            (outerDepth, innerDepth, manager.CurrentUseContextDepth, manager.IsInUseContext));
    }

    [TestMethod]
    public void WhenExitingUseContextsOutOfOrderThenCallIsRejectedWithoutChangingState()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource outerResource = new(manager);
        using TestResource innerResource = new(manager);
        uint outerDepth = manager.EnterUseContext();
        outerResource.SetAsEvictable();
        uint innerDepth = manager.EnterUseContext();
        innerResource.SetAsEvictable();

        Assert.ThrowsExactly<InvalidOperationException>(() => manager.ExitUseContext(outerDepth));

        Assert.AreEqual(
            (innerDepth, outerDepth, innerDepth, 0u, true, true, true, true, 2),
            (manager.CurrentUseContextDepth, outerResource.ActiveUseContextDepth,
                innerResource.ActiveUseContextDepth, manager.CompletedFrameCount,
                outerResource.IsValid, innerResource.IsValid, outerResource.IsManaged,
                innerResource.IsManaged, manager.ResourceCount));
        manager.ExitUseContext(innerDepth);
        manager.ExitUseContext(outerDepth);
    }

    [TestMethod]
    public void WhenUsingOuterResourceAgainInsideInnerContextThenFirstUseDepthIsPreserved()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        uint outerDepth = manager.EnterUseContext();
        resource.SetAsEvictable();
        uint innerDepth = manager.EnterUseContext();

        manager.Use(resource);
        manager.ExitUseContext(innerDepth);

        Assert.AreEqual((outerDepth, outerDepth, true),
            (resource.ActiveUseContextDepth, manager.CurrentUseContextDepth, resource.IsValid));
        manager.ExitUseContext(outerDepth);
    }

    [TestMethod]
    public void WhenExitingInnerUseContextThenOnlyItsStableTailMovesToCurrentFrameMruOrder()
    {
        Direct3D9ResourceManager manager = new();
        TestResource outerResource = new(manager);
        TestResource firstInnerResource = new(manager);
        TestResource secondInnerResource = new(manager);
        uint outerDepth = manager.EnterUseContext();
        outerResource.SetAsEvictable();
        uint innerDepth = manager.EnterUseContext();
        firstInnerResource.SetAsEvictable();
        secondInnerResource.SetAsEvictable();
        manager.Use(outerResource);

        manager.ExitUseContext(innerDepth);
        bool firstRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);
        bool secondRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual(
            (true, true, true, false, true, true, outerDepth, 1),
            (firstRetry, secondRetry, firstInnerResource.IsReleased, firstInnerResource.IsValid,
                secondInnerResource.IsReleased, outerResource.IsValid,
                outerResource.ActiveUseContextDepth, manager.ResourceCount));
        manager.ExitUseContext(outerDepth);
        manager.EndFrame();
        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);
    }

    [TestMethod]
    public void WhenInnerTailMovesBeforeOuterResourceThenEndFrameKeepsStableLruOrder()
    {
        Direct3D9ResourceManager manager = new();
        TestResource previousFrameResource = new(manager);
        previousFrameResource.SetAsEvictable();
        manager.EndFrame();
        TestResource outerResource = new(manager);
        TestResource innerResource = new(manager);
        uint outerDepth = manager.EnterUseContext();
        outerResource.SetAsEvictable();
        uint innerDepth = manager.EnterUseContext();
        innerResource.SetAsEvictable();
        manager.ExitUseContext(innerDepth);
        manager.ExitUseContext(outerDepth);

        manager.EndFrame();
        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);
        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual(
            (true, true, true, 1),
            (previousFrameResource.IsReleased, innerResource.IsReleased,
                outerResource.IsValid, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenUsingEvictableResourceOutsideUseContextThenCallIsRejected()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();

        Assert.ThrowsExactly<InvalidOperationException>(() => manager.Use(resource));
    }

    [TestMethod]
    public void WhenMarkingResourceEvictableWithoutUseContextThenTemporaryContextFullyExitsWithoutChangingOwnershipState()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        using TestResource resource = new(manager, () => releaseCount++);

        resource.SetAsEvictable();

        Assert.AreEqual(
            (true, 0u, false, true, true, 1, 0u, 0),
            (resource.IsEvictable, resource.ActiveUseContextDepth, manager.IsInUseContext,
                resource.IsValid, resource.IsManaged, manager.ResourceCount, manager.CompletedFrameCount, releaseCount));
    }

    [TestMethod]
    public void WhenMarkingResourceEvictableInsideNestedUseContextThenCurrentDepthOwnsItsUse()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        uint outerDepth = manager.EnterUseContext();
        uint innerDepth = manager.EnterUseContext();

        resource.SetAsEvictable();

        Assert.AreEqual((innerDepth, true), (resource.ActiveUseContextDepth, manager.IsInUseContext));
        manager.ExitUseContext(innerDepth);
        manager.ExitUseContext(outerDepth);
    }

    [TestMethod]
    public void WhenMarkingActiveEvictableResourceAgainInsideNestedContextThenOutermostUseDepthIsPreserved()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        uint outerDepth = manager.EnterUseContext();
        resource.SetAsEvictable();
        uint innerDepth = manager.EnterUseContext();

        resource.SetAsEvictable();
        manager.ExitUseContext(innerDepth);

        Assert.AreEqual((outerDepth, true), (resource.ActiveUseContextDepth, manager.IsInUseContext));
        manager.ExitUseContext(outerDepth);
    }

    [TestMethod]
    public void WhenMarkingInactiveEvictableResourceAgainWithoutUseContextThenItBecomesCurrentFrameMru()
    {
        Direct3D9ResourceManager manager = new();
        TestResource first = new(manager);
        TestResource second = new(manager);
        first.SetAsEvictable();
        second.SetAsEvictable();

        first.SetAsEvictable();
        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual(
            (true, true, true, false, 1),
            (shouldRetry, first.IsReleased, second.IsValid, manager.IsInUseContext, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenMarkingResourceEvictableInsideUseContextThenItCannotBeEvictedUntilThatDepthExits()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);
        uint depth = manager.EnterUseContext();
        resource.SetAsEvictable();

        bool retryWhileActive = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);
        manager.ExitUseContext(depth);
        bool retryAfterExit = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((false, true, true, 0),
            (retryWhileActive, retryAfterExit, resource.IsReleased, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenReadingInitialResourceStateRepeatedlyThenManagerStateIsUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        using TestResource resource = new(manager, () => releaseCount++);

        bool firstValidity = resource.IsValid;
        bool firstEvictability = resource.IsEvictable;
        bool secondValidity = resource.IsValid;
        bool secondEvictability = resource.IsEvictable;

        Assert.AreEqual(
            (true, false, true, false, 0u, false, true, 1, 0u, 0),
            (firstValidity, firstEvictability, secondValidity, secondEvictability,
                resource.ActiveUseContextDepth, manager.IsInUseContext, resource.IsManaged,
                manager.ResourceCount, manager.CompletedFrameCount, releaseCount));
    }

    [TestMethod]
    public void WhenReadingEvictableStateInsideNestedUseContextsThenActiveDepthIsUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        uint outerDepth = manager.EnterUseContext();
        resource.SetAsEvictable();
        uint innerDepth = manager.EnterUseContext();

        _ = resource.IsValid;
        _ = resource.IsEvictable;
        _ = resource.IsValid;
        _ = resource.IsEvictable;

        Assert.AreEqual((true, true, outerDepth, true, 1, 0u),
            (resource.IsValid, resource.IsEvictable, resource.ActiveUseContextDepth,
                manager.IsInUseContext, manager.ResourceCount, manager.CompletedFrameCount));

        manager.ExitUseContext(innerDepth);
        manager.ExitUseContext(outerDepth);
    }

    [TestMethod]
    public void WhenReadingResourceStateThenCurrentFrameMruOrderIsUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        TestResource first = new(manager);
        TestResource second = new(manager);
        first.SetAsEvictable();
        second.SetAsEvictable();

        _ = first.IsValid;
        _ = first.IsEvictable;
        _ = first.IsValid;
        _ = first.IsEvictable;
        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((true, true, false, true, 1),
            (shouldRetry, first.IsValid, second.IsValid, first.IsManaged, manager.ResourceCount));

        first.Dispose();
    }

    [TestMethod]
    public void WhenReadingResourceStateAfterManagerInvalidationThenCachedFlagsRemainObservable()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        TestResource resource = new(manager, () => releaseCount++);
        resource.SetAsEvictable();

        bool shouldRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);
        bool firstValidity = resource.IsValid;
        bool firstEvictability = resource.IsEvictable;
        bool secondValidity = resource.IsValid;
        bool secondEvictability = resource.IsEvictable;

        Assert.AreEqual((true, false, true, false, true, 0u, false, 0, 0u, 1),
            (shouldRetry, firstValidity, firstEvictability, secondValidity, secondEvictability,
                resource.ActiveUseContextDepth, resource.IsManaged, manager.ResourceCount,
                manager.CompletedFrameCount, releaseCount));
    }

    [TestMethod]
    public void WhenReadingResourceStateAfterDisposeAndManagerDestructionThenCachedFlagsRemainObservable()
    {
        Direct3D9ResourceManager manager = new();
        int disposedReleaseCount = 0;
        int destroyedReleaseCount = 0;
        TestResource disposed = new(manager, () => disposedReleaseCount++);
        TestResource destroyed = new(manager, () => destroyedReleaseCount++);
        disposed.SetAsEvictable();
        destroyed.SetAsEvictable();

        disposed.Dispose();
        manager.DestroyAllResources();

        Assert.AreEqual(
            (false, true, false, true, false, false, 0, 1, 1),
            (disposed.IsValid, disposed.IsEvictable, destroyed.IsValid, destroyed.IsEvictable,
                disposed.IsManaged, destroyed.IsManaged, manager.ResourceCount,
                disposedReleaseCount, destroyedReleaseCount));
    }

    [TestMethod]
    public void WhenMarkingEvictableResourceAgainInsideUseContextThenItIsUsedAtCurrentDepth()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();
        uint depth = manager.EnterUseContext();

        resource.SetAsEvictable();

        Assert.AreEqual(depth, resource.ActiveUseContextDepth);
        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenExitingUseContextThenResourcesUsedAtThatDepthBecomeInactive()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        uint depth = manager.EnterUseContext();
        resource.SetAsEvictable();

        manager.ExitUseContext(depth);

        Assert.AreEqual(0u, resource.ActiveUseContextDepth);
    }

    [TestMethod]
    public void WhenEndingFrameInsideUseContextThenFailureDoesNotCommitAndLaterSuccessMigratesResourceOnce()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);
        uint depth = manager.EnterUseContext();
        resource.SetAsEvictable();

        Assert.ThrowsExactly<InvalidOperationException>(manager.EndFrame);
        uint completedFrameCountAfterFailure = manager.CompletedFrameCount;
        manager.ExitUseContext(depth);
        manager.EndFrame();
        bool firstRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);
        bool secondRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual(
            (0u, 1u, true, false, true, false, 0),
            (completedFrameCountAfterFailure, manager.CompletedFrameCount, firstRetry, secondRetry,
                resource.IsReleased, resource.IsManaged, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenEndingFrameWithNestedUseContextThenFailureDoesNotPartiallyMigrateResources()
    {
        Direct3D9ResourceManager manager = new();
        TestResource outerResource = new(manager);
        TestResource innerResource = new(manager);
        uint outerDepth = manager.EnterUseContext();
        outerResource.SetAsEvictable();
        uint innerDepth = manager.EnterUseContext();
        innerResource.SetAsEvictable();
        manager.ExitUseContext(innerDepth);

        Assert.ThrowsExactly<InvalidOperationException>(manager.EndFrame);
        manager.ExitUseContext(outerDepth);
        manager.EndFrame();
        bool firstRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);
        bool secondRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);
        bool thirdRetry = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual(
            (1u, true, true, false, true, true, 0),
            (manager.CompletedFrameCount, firstRetry, secondRetry, thirdRetry,
                innerResource.IsReleased, outerResource.IsReleased, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenEndingMultipleFramesThenResourcesKeepCrossFrameLruOrder()
    {
        Direct3D9ResourceManager manager = new();
        TestResource firstFrameLeastRecentlyUsed = new(manager);
        TestResource firstFrameMostRecentlyUsed = new(manager);
        firstFrameLeastRecentlyUsed.SetAsEvictable();
        firstFrameMostRecentlyUsed.SetAsEvictable();
        manager.EndFrame();
        TestResource secondFrameResource = new(manager);
        secondFrameResource.SetAsEvictable();
        manager.EndFrame();

        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);
        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual(
            (true, true, true, false),
            (firstFrameLeastRecentlyUsed.IsReleased,
                firstFrameMostRecentlyUsed.IsReleased,
                secondFrameResource.IsValid,
                manager.IsInUseContext));
    }

    [TestMethod]
    public void WhenEndingEmptyFrameThenExistingPreviousFrameLruOrderIsUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        TestResource leastRecentlyUsed = new(manager);
        TestResource mostRecentlyUsed = new(manager);
        leastRecentlyUsed.SetAsEvictable();
        mostRecentlyUsed.SetAsEvictable();
        manager.EndFrame();

        manager.EndFrame();
        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual(
            (true, true, 2u),
            (leastRecentlyUsed.IsReleased, mostRecentlyUsed.IsValid, manager.CompletedFrameCount));
    }

    [TestMethod]
    public void WhenEndingFrameThenResourceValidityAndManagementAreUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();

        manager.EndFrame();

        Assert.AreEqual((true, true, 1), (resource.IsValid, resource.IsManaged, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenMarkingReleasedResourceAsEvictableThenCallIsRejected()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager);
        resource.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(resource.SetAsEvictable);
    }

    [TestMethod]
    public void WhenQueryingDeviceWithoutUseContextThenFalseIsReturnedWithoutChangingState()
    {
        using Direct3D9Device device = CreateDevice(_ => { });

        bool firstResult = device.IsInUseContext();
        bool secondResult = device.IsInUseContext();

        Assert.AreEqual(
            (false, false, false, 0, 0u, false),
            (firstResult, secondResult, device.IsEntered(), device.ResourceCount,
                device.ResourceManager.CompletedFrameCount, device.IsInScene));
    }

    [TestMethod]
    public void WhenQueryingNestedDeviceUseContextsThenBooleanStateTracksEachExit()
    {
        using Direct3D9Device device = CreateDevice(_ => { });

        uint outerDepth = device.EnterUseContext();
        bool outerResult = device.IsInUseContext();
        uint innerDepth = device.EnterUseContext();
        bool innerResult = device.IsInUseContext();
        device.ExitUseContext(innerDepth);
        bool afterInnerExitResult = device.IsInUseContext();
        device.ExitUseContext(outerDepth);
        bool afterOuterExitResult = device.IsInUseContext();

        Assert.AreEqual(
            (1u, true, 2u, true, true, false),
            (outerDepth, outerResult, innerDepth, innerResult, afterInnerExitResult, afterOuterExitResult));
    }

    [TestMethod]
    public void WhenQueryingDeviceUseContextThenExistingDepthAndResourceStateAreUnchanged()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        uint depth = device.EnterUseContext();
        using TestResource resource = new(device.ResourceManager, isEvictable: true);

        bool firstResult = device.IsInUseContext();
        bool secondResult = device.IsInUseContext();

        Assert.AreEqual(
            (true, true, depth, false, 1, 0u, false, true, true),
            (firstResult, secondResult, resource.ActiveUseContextDepth, device.IsEntered(),
                device.ResourceCount, device.ResourceManager.CompletedFrameCount, device.IsInScene,
                resource.IsValid, resource.IsManaged));

        device.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenExitingDeviceUseContextsOutOfOrderThenCallIsRejected()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        uint outerDepth = device.EnterUseContext();
        uint innerDepth = device.EnterUseContext();

        Assert.ThrowsExactly<InvalidOperationException>(() => device.ExitUseContext(outerDepth));

        device.ExitUseContext(innerDepth);
        device.ExitUseContext(outerDepth);
    }

    [TestMethod]
    public void WhenEnteringDeviceUseContextAfterDisposalThenCallIsRejected()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.EnterUseContext());
    }

    [TestMethod]
    public void WhenExitingDeviceUseContextAfterDisposalThenCallIsRejected()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.ExitUseContext(1));
    }

    [TestMethod]
    public void WhenQueryingDeviceUseContextAfterDisposalThenCallIsRejected()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.IsInUseContext());
    }

    [TestMethod]
    public void WhenCreatingUseContextGuardThenDeviceIsInsideUseContext()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        using Direct3D9UseContextGuard guard = new(device);

        Assert.IsTrue(device.IsInUseContext());
    }

    [TestMethod]
    public void WhenDisposingUseContextGuardThenDeviceExitsUseContext()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9UseContextGuard guard = new(device);

        guard.Dispose();

        Assert.IsFalse(device.IsInUseContext());
    }

    [TestMethod]
    public void WhenDisposingUseContextGuardTwiceThenSecondDisposeDoesNothing()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9UseContextGuard guard = new(device);
        guard.Dispose();

        guard.Dispose();

        Assert.IsFalse(device.IsInUseContext());
    }

    [TestMethod]
    public void WhenNestingUseContextGuardsThenEachGuardOwnsItsDepth()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        using Direct3D9UseContextGuard outerGuard = new(device);
        Direct3D9UseContextGuard innerGuard = new(device);

        innerGuard.Dispose();

        Assert.IsTrue(device.IsInUseContext());
    }

    [TestMethod]
    public void WhenCreatingUseContextGuardAfterDeviceDisposalThenCallIsRejected()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => new Direct3D9UseContextGuard(device));
    }

    [TestMethod]
    public void WhenManagerIsCreatedThenLifecycleStateIsZeroInitialized()
    {
        Direct3D9ResourceManager manager = new();

        Assert.AreEqual(
            (null, false, 0, 0, 0, 0, 0u, 0u, 0u, false),
            (manager.Device, manager.IsClosed, manager.ResourceCount, manager.ReleasedResourceCount,
                manager.DelayedReleasedResourceCount, manager.PendingResourceDestructionCount,
                manager.CurrentUseContextDepth, manager.TotalVideoMemoryConsumption,
                manager.PeakVideoMemoryConsumption, manager.IsInUseContext));
    }

    [TestMethod]
    public void WhenManagerIsCreatedForDeviceThenBorrowedIdentityIsStableAndStateIsZeroInitialized()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;

        Assert.AreEqual(
            (device, device, false, 0, 0, 0, 0, 0u, 0u, 0u, false),
            (manager.Device, manager.Device, manager.IsClosed, manager.ResourceCount,
                manager.ReleasedResourceCount, manager.DelayedReleasedResourceCount,
                manager.PendingResourceDestructionCount, manager.CurrentUseContextDepth,
                manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption,
                manager.IsInUseContext));
    }

    [TestMethod]
    public void WhenCloseIsAttemptedDuringPendingDestructionThenFailureHasNoStateSideEffects()
    {
        Direct3D9ResourceManager manager = new();
        InvalidOperationException? closeException = null;
        (int Resources, int Pending, uint Total, uint Peak, bool IsClosed, bool NestedManaged)? stateDuringFailure = null;
        TestResource? nested = null;
        TestResource outer = new(manager, () =>
        {
            manager.DestroyResource(nested!);
            try
            {
                manager.Close();
            }
            catch (InvalidOperationException exception)
            {
                closeException = exception;
            }

            stateDuringFailure = (manager.ResourceCount, manager.PendingResourceDestructionCount,
                manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption,
                manager.IsClosed, nested!.IsManaged);
        }, resourceSize: 16);
        nested = new TestResource(manager, resourceSize: 32);

        manager.DestroyResource(outer);

        Assert.AreEqual(
            (true, (2, 1, 48u, 48u, false, true), 0, 0, 0u, 48u, false, false),
            (closeException is not null, stateDuringFailure, manager.ResourceCount,
                manager.PendingResourceDestructionCount, manager.TotalVideoMemoryConsumption,
                manager.PeakVideoMemoryConsumption, manager.IsClosed, nested.IsManaged));
    }

    [TestMethod]
    public void WhenClosingManagerWithCurrentFrameReleasedResourceThenCallIsRejected()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        manager.UnusedNotification(resource);

        Assert.ThrowsExactly<InvalidOperationException>(manager.Close);
    }

    [TestMethod]
    public void WhenClosingManagerWithDelayedReleasedResourceThenCallIsRejected()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager, requiresDelayedRelease: true);
        manager.UnusedNotification(resource);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        Assert.ThrowsExactly<InvalidOperationException>(manager.Close);
    }

    [TestMethod]
    public void WhenClosingManagerWithNonEvictableResourceThenCallIsRejected()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);

        Assert.ThrowsExactly<InvalidOperationException>(manager.Close);
    }

    [TestMethod]
    public void WhenClosingManagerWithPreviousFrameResourceThenCallIsRejected()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();
        manager.EndFrame();

        Assert.ThrowsExactly<InvalidOperationException>(manager.Close);
    }

    [TestMethod]
    public void WhenClosingManagerWithCurrentFrameResourceNotInUseThenCallIsRejected()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager);
        resource.SetAsEvictable();

        Assert.ThrowsExactly<InvalidOperationException>(manager.Close);
    }

    [TestMethod]
    public void WhenClosingManagerWithCurrentFrameResourceInUseThenCallIsRejected()
    {
        Direct3D9ResourceManager manager = new();
        uint depth = manager.EnterUseContext();
        using TestResource resource = new(manager, isEvictable: true);

        Assert.ThrowsExactly<InvalidOperationException>(manager.Close);

        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenClosingManagerInsideEmptyUseContextThenCallIsRejected()
    {
        Direct3D9ResourceManager manager = new();
        uint depth = manager.EnterUseContext();

        Assert.ThrowsExactly<InvalidOperationException>(manager.Close);

        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenRegisteringSizedResourcesThenManagerTracksTheirTotalConsumption()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource zeroSizedResource = new(manager);
        using TestResource sizedResource = new(manager, resourceSize: 128);

        Assert.AreEqual((0u, 128u), (zeroSizedResource.ResourceSize, manager.TotalVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenDisposingSizedResourceThenConsumptionIsSubtractedOnce()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager, resourceSize: 64);

        resource.Dispose();
        resource.Dispose();

        Assert.AreEqual(0u, manager.TotalVideoMemoryConsumption);
    }

    [TestMethod]
    public void WhenEvictingSizedResourceThenConsumptionIsSubtractedOnce()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager, resourceSize: 96);
        resource.SetAsEvictable();

        bool destroyed = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, isSoftwareDevice: false);

        Assert.AreEqual((true, 0u), (destroyed, manager.TotalVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenDestroyingSizedResourcesIncludingNestedDestructionThenConsumptionReturnsToZero()
    {
        Direct3D9ResourceManager manager = new();
        TestResource? second = null;
        TestResource first = new(manager, () => manager.DestroyResource(second!), resourceSize: 40);
        second = new TestResource(manager, resourceSize: 60);

        manager.DestroyResource(first);

        Assert.AreEqual((0u, 0), (manager.TotalVideoMemoryConsumption, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenReadingResourceSizeAndConsumptionThenStateIsUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager, resourceSize: 32);

        (uint FirstSize, uint FirstTotal, uint SecondSize, uint SecondTotal) result =
            (resource.ResourceSize, manager.TotalVideoMemoryConsumption,
                resource.ResourceSize, manager.TotalVideoMemoryConsumption);

        Assert.AreEqual((32u, 32u, 32u, 32u), result);
    }

    [TestMethod]
    public void WhenClosingEmptyManagerTwiceThenPeakAndBorrowedIdentityArePreserved()
    {
        using Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        int releaseCount = 0;
        TestResource resource = new(manager, () => releaseCount++, resourceSize: 64);
        manager.DestroyAllResources();

        manager.Close();
        (Direct3D9Device? Device, uint Peak, bool IsClosed) afterFirstClose =
            (manager.Device, manager.PeakVideoMemoryConsumption, manager.IsClosed);
        manager.Close();

        Assert.AreEqual(
            (1, false, 0, 0u, (device, 64u, true), device, 64u, true),
            (releaseCount, resource.IsManaged, manager.ResourceCount,
                manager.TotalVideoMemoryConsumption, afterFirstClose,
                manager.Device, manager.PeakVideoMemoryConsumption, manager.IsClosed));
    }

    [TestMethod]
    public void WhenRegisteringResourceAfterManagerClosesThenCallIsRejected()
    {
        Direct3D9ResourceManager manager = new();
        manager.Close();

        Assert.ThrowsExactly<ObjectDisposedException>(() => new TestResource(manager, resourceSize: 48));
        Assert.AreEqual((0u, 0), (manager.TotalVideoMemoryConsumption, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenDisposingDeviceInsideUseContextThenCallIsRejectedWithoutManagerStateChanges()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        int releaseCount = 0;
        using TestResource resource = new(manager, () => releaseCount++, resourceSize: 32);
        uint depth = device.EnterUseContext();
        (int Resources, int Released, int Delayed, int Pending, uint Depth, uint Total, uint Peak, bool IsClosed) before =
            (manager.ResourceCount, manager.ReleasedResourceCount, manager.DelayedReleasedResourceCount,
                manager.PendingResourceDestructionCount, manager.CurrentUseContextDepth,
                manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption, manager.IsClosed);

        Assert.ThrowsExactly<InvalidOperationException>(device.Dispose);

        Assert.AreEqual(
            (before, 0, true, true, device),
            ((manager.ResourceCount, manager.ReleasedResourceCount, manager.DelayedReleasedResourceCount,
                manager.PendingResourceDestructionCount, manager.CurrentUseContextDepth,
                manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption, manager.IsClosed),
                releaseCount, resource.IsValid, resource.IsManaged, manager.Device));

        device.ExitUseContext(depth);
        device.Dispose();
    }

    [TestMethod]
    public void WhenDeviceClosesThenManagerClosesWithZeroCurrentStateAndPreservedPeak()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        _ = new TestResource(manager, resourceSize: 96);

        device.Dispose();
        device.Dispose();

        Assert.AreEqual(
            (device, true, 0, 0, 0, 0, 0u, 0u, 96u, false),
            (manager.Device, manager.IsClosed, manager.ResourceCount, manager.ReleasedResourceCount,
                manager.DelayedReleasedResourceCount, manager.PendingResourceDestructionCount,
                manager.CurrentUseContextDepth, manager.TotalVideoMemoryConsumption,
                manager.PeakVideoMemoryConsumption, manager.IsInUseContext));
    }

    [TestMethod]
    public void WhenPreviousFrameListIsEmptyThenItIsSorted()
    {
        Direct3D9ResourceManager manager = new();

        Assert.IsTrue(manager.IsPreviousFrameListSorted());
    }

    [TestMethod]
    public void WhenResourcesAreAppendedAcrossFramesThenPreviousFrameListIsSortedOldestToNewest()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource firstFrame = new(manager);
        firstFrame.SetAsEvictable();
        manager.EndFrame();
        using TestResource secondFrame = new(manager);
        secondFrame.SetAsEvictable();
        manager.EndFrame();

        Assert.AreEqual((true, 1ul, 2ul, 0u, 0u),
            (manager.IsPreviousFrameListSorted(), firstFrame.LastUsedFrame, secondFrame.LastUsedFrame,
                firstFrame.ActiveUseContextDepth, secondFrame.ActiveUseContextDepth));
    }

    [TestMethod]
    public void WhenSeveralResourcesAreUsedInSameFrameThenStableMruOrderIsPreserved()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource leastRecentlyUsed = new(manager);
        using TestResource mostRecentlyUsed = new(manager);
        leastRecentlyUsed.SetAsEvictable();
        mostRecentlyUsed.SetAsEvictable();
        manager.EndFrame();

        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((true, 1ul, 1ul, true, true),
            (manager.IsPreviousFrameListSorted(), leastRecentlyUsed.LastUsedFrame,
                mostRecentlyUsed.LastUsedFrame, leastRecentlyUsed.IsReleased, mostRecentlyUsed.IsValid));
    }

    [TestMethod]
    public void WhenPreviousFrameResourceIsReusedThenItLeavesPreviousFrameOrdering()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource reused = new(manager);
        using TestResource retained = new(manager);
        reused.SetAsEvictable();
        retained.SetAsEvictable();
        manager.EndFrame();
        uint depth = manager.EnterUseContext();
        manager.Use(reused);

        bool sortedWhileReusedResourceIsInUse = manager.IsPreviousFrameListSorted();
        manager.ExitUseContext(depth);
        manager.EndFrame();

        Assert.AreEqual((true, true, 2ul, 1ul),
            (sortedWhileReusedResourceIsInUse, manager.IsPreviousFrameListSorted(),
                reused.LastUsedFrame, retained.LastUsedFrame));
    }

    [TestMethod]
    public void WhenPreviousFrameResourcesAreEvictedThenLruOrderRemainsSorted()
    {
        Direct3D9ResourceManager manager = new();
        TestResource first = new(manager);
        using TestResource second = new(manager);
        first.SetAsEvictable();
        second.SetAsEvictable();
        manager.EndFrame();

        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((true, true, true),
            (first.IsReleased, second.IsValid, manager.IsPreviousFrameListSorted()));
    }

    [TestMethod]
    public void WhenPreviousFrameResourcesEnterReleaseQueuesThenRemainingOrderingIsSorted()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource released = new(manager);
        using TestResource delayed = new(manager, requiresDelayedRelease: true);
        using TestResource active = new(manager);
        released.SetAsEvictable();
        delayed.SetAsEvictable();
        active.SetAsEvictable();
        manager.EndFrame();
        manager.UnusedNotification(released);
        manager.UnusedNotification(delayed);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        Assert.IsTrue(manager.IsPreviousFrameListSorted());
    }

    [TestMethod]
    public void WhenPreviousFrameResourceIsDisposedOrAllResourcesAreDestroyedThenOrderingIsSorted()
    {
        Direct3D9ResourceManager manager = new();
        TestResource disposed = new(manager);
        TestResource remaining = new(manager);
        disposed.SetAsEvictable();
        remaining.SetAsEvictable();
        manager.EndFrame();
        disposed.Dispose();
        bool sortedAfterDispose = manager.IsPreviousFrameListSorted();

        manager.DestroyAllResources();

        Assert.AreEqual((true, true, 0),
            (sortedAfterDispose, manager.IsPreviousFrameListSorted(), manager.ResourceCount));
    }

    [TestMethod]
    public void WhenDeviceClosesThenBorrowedManagerPreviousFrameOrderingIsSorted()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        TestResource resource = new(manager);
        resource.SetAsEvictable();
        manager.EndFrame();

        device.Dispose();

        Assert.IsTrue(manager.IsPreviousFrameListSorted());
    }

    [TestMethod]
    public void WhenQueryingPreviousFrameOrderingRepeatedlyThenManagerAndResourcesAreUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource first = new(manager, resourceSize: 16);
        using TestResource second = new(manager, resourceSize: 32);
        first.SetAsEvictable();
        second.SetAsEvictable();
        manager.EndFrame();
        (int Count, uint Frames, uint Memory, ulong FirstFrame, ulong SecondFrame, uint FirstDepth, uint SecondDepth) before =
            (manager.ResourceCount, manager.CompletedFrameCount, manager.TotalVideoMemoryConsumption,
                first.LastUsedFrame, second.LastUsedFrame, first.ActiveUseContextDepth, second.ActiveUseContextDepth);

        bool firstResult = manager.IsPreviousFrameListSorted();
        bool secondResult = manager.IsPreviousFrameListSorted();

        Assert.AreEqual((true, true, before),
            (firstResult, secondResult,
                (manager.ResourceCount, manager.CompletedFrameCount, manager.TotalVideoMemoryConsumption,
                    first.LastUsedFrame, second.LastUsedFrame, first.ActiveUseContextDepth, second.ActiveUseContextDepth)));
    }

    [TestMethod]
    public void WhenRegisteringSameResourceAgainThenCountAndStateRemainUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager, resourceSize: 32);

        Assert.ThrowsExactly<InvalidOperationException>(() => manager.RegisterResource(resource));

        Assert.AreEqual((1, 32u, true, true),
            (manager.ResourceCount, manager.TotalVideoMemoryConsumption, resource.IsValid, resource.IsManaged));
    }

    [TestMethod]
    public void WhenRegisteringForeignResourceThenBothManagerCountsRemainUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        Direct3D9ResourceManager foreignManager = new();
        using TestResource resource = new(foreignManager, resourceSize: 32);

        Assert.ThrowsExactly<InvalidOperationException>(() => manager.RegisterResource(resource));

        Assert.AreEqual((0, 1, 0u, 32u, true, true),
            (manager.ResourceCount, foreignManager.ResourceCount, manager.TotalVideoMemoryConsumption,
                foreignManager.TotalVideoMemoryConsumption, resource.IsValid, resource.IsManaged));
    }

    [TestMethod]
    public void WhenDisposingResourceRepeatedlyThenRegistrationIsRemovedOnce()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        TestResource resource = new(manager, () => releaseCount++, resourceSize: 32);

        resource.Dispose();
        resource.Dispose();
        manager.DestroyResource(resource);

        Assert.AreEqual((0, 0u, 1, false),
            (manager.ResourceCount, manager.TotalVideoMemoryConsumption, releaseCount, resource.IsManaged));
    }

    [TestMethod]
    public void WhenResourcesAreQueuedForReleaseThenCountChangesOnlyWhenDestructionCompletes()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource immediate = new(manager, resourceSize: 16);
        using TestResource delayed = new(manager, requiresDelayedRelease: true, resourceSize: 32);
        manager.UnusedNotification(immediate);
        manager.UnusedNotification(delayed);
        int countWhileReleased = manager.ResourceCount;

        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        int countWhileDelayed = manager.ResourceCount;
        _ = manager.DestroyReleasedResourcesFromLastFrame();

        Assert.AreEqual((2, 1, 0, 0u),
            (countWhileReleased, countWhileDelayed, manager.ResourceCount, manager.TotalVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenValidResourceIsQueuedRepeatedlyThenIdentityAndAccountingRemainUntilFirstFlush()
    {
        Direct3D9ResourceManager manager = new();
        uint depth = manager.EnterUseContext();
        TestResource resource = new(manager, isEvictable: true, resourceSize: 32);

        manager.UnusedNotification(resource);
        manager.UnusedNotification(resource);
        (bool IsValid, bool IsManaged, Direct3D9ResourceManager Manager, uint ActiveDepth, int Resources,
            int Released, uint Total, uint Peak) queuedState =
            (resource.IsValid, resource.IsManaged, resource.Manager, resource.ActiveUseContextDepth,
                manager.ResourceCount, manager.ReleasedResourceCount, manager.TotalVideoMemoryConsumption,
                manager.PeakVideoMemoryConsumption);

        uint destroyedCount = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithoutDelay);

        Assert.AreEqual(
            ((true, true, manager, depth, 1, 1, 32u, 32u), 1u, false, false, 0, 0, 0u, 32u),
            (queuedState, destroyedCount, resource.IsValid, resource.IsManaged, manager.ResourceCount,
                manager.ReleasedResourceCount, manager.TotalVideoMemoryConsumption,
                manager.PeakVideoMemoryConsumption));
        manager.ExitUseContext(depth);
    }

    [TestMethod]
    public void WhenEvictingResourceThenRegistrationIsRemovedOnce()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        TestResource resource = new(manager, () => releaseCount++, resourceSize: 32);
        resource.SetAsEvictable();

        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);
        _ = manager.FreeSomeVideoMemory(Direct3D9Factory.OutOfVideoMemoryHResult, false);

        Assert.AreEqual((0, 0u, 1, false),
            (manager.ResourceCount, manager.TotalVideoMemoryConsumption, releaseCount, resource.IsManaged));
    }

    [TestMethod]
    public void WhenNestedDestructionOccursThenEachRegistrationIsRemovedOnce()
    {
        Direct3D9ResourceManager manager = new();
        int outerReleaseCount = 0;
        int nestedReleaseCount = 0;
        int? countDuringOuterRelease = null;
        TestResource nested = new(manager, () => nestedReleaseCount++);
        TestResource outer = new(manager, () =>
        {
            outerReleaseCount++;
            nested.Dispose();
            countDuringOuterRelease = manager.ResourceCount;
        });

        outer.Dispose();

        Assert.AreEqual((2, 0, 1, 1, false, false),
            (countDuringOuterRelease, manager.ResourceCount, outerReleaseCount, nestedReleaseCount,
                outer.IsManaged, nested.IsManaged));
    }

    [TestMethod]
    public void WhenDestroyingMixedActiveResourcesThenListOrderIsPreserved()
    {
        Direct3D9ResourceManager manager = new();
        List<string> releases = [];
        TestResource nonEvictable = new(manager, () => releases.Add("non-evictable"), resourceSize: 1);
        TestResource previousFrame = new(manager, () => releases.Add("previous-frame"), resourceSize: 2);
        previousFrame.SetAsEvictable();
        manager.EndFrame();
        TestResource currentFrameNotInUse = new(manager, () => releases.Add("current-frame-not-in-use"), resourceSize: 4);
        currentFrameNotInUse.SetAsEvictable();
        uint depth = manager.EnterUseContext();
        TestResource currentFrameInUse = new(manager, () => releases.Add("current-frame-in-use"),
            isEvictable: true, resourceSize: 8);

        manager.DestroyAllResources();
        manager.ExitUseContext(depth);

        CollectionAssert.AreEqual(
            new[] { "non-evictable", "previous-frame", "current-frame-not-in-use", "current-frame-in-use" },
            releases);
    }

    [TestMethod]
    public void WhenDestroyingMixedActiveResourcesThenReleaseObservesInvalidManagedResources()
    {
        Direct3D9ResourceManager manager = new();
        TestResource nonEvictable = new(manager, resourceSize: 1);
        TestResource previousFrame = new(manager, resourceSize: 2);
        previousFrame.SetAsEvictable();
        manager.EndFrame();
        TestResource currentFrameNotInUse = new(manager, resourceSize: 4);
        currentFrameNotInUse.SetAsEvictable();
        uint depth = manager.EnterUseContext();
        TestResource currentFrameInUse = new(manager, isEvictable: true, resourceSize: 8);

        manager.DestroyAllResources();
        manager.ExitUseContext(depth);

        Assert.AreEqual(
            (true, false, true, false, true, false, true, false, false, false, false, false),
            (nonEvictable.WasManagedDuringRelease, nonEvictable.WasValidDuringRelease,
                previousFrame.WasManagedDuringRelease, previousFrame.WasValidDuringRelease,
                currentFrameNotInUse.WasManagedDuringRelease, currentFrameNotInUse.WasValidDuringRelease,
                currentFrameInUse.WasManagedDuringRelease, currentFrameInUse.WasValidDuringRelease,
                nonEvictable.IsManaged, previousFrame.IsManaged,
                currentFrameNotInUse.IsManaged, currentFrameInUse.IsManaged));
    }

    [TestMethod]
    public void WhenActiveResourceReleaseQueuesLaterResourceThenNestedDestructionIsDelayed()
    {
        Direct3D9ResourceManager manager = new();
        List<string> releases = [];
        TestResource previousFrame = new(manager, () => releases.Add("nested-previous"), resourceSize: 2);
        previousFrame.SetAsEvictable();
        manager.EndFrame();
        TestResource nonEvictable = new(manager, () =>
        {
            releases.Add("non-evictable");
            previousFrame.Dispose();
        }, resourceSize: 1);
        TestResource currentFrameNotInUse = new(manager, () => releases.Add("current-frame-not-in-use"), resourceSize: 4);
        currentFrameNotInUse.SetAsEvictable();

        manager.DestroyAllResources();

        CollectionAssert.AreEqual(
            new[] { "non-evictable", "current-frame-not-in-use", "nested-previous" },
            releases);
    }

    [TestMethod]
    public void WhenReleasedAndActiveResourcesAreDestroyedTogetherThenEachIsAccountedOnce()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        TestResource released = new(manager, () => releaseCount++, resourceSize: 1);
        TestResource delayed = new(manager, () => releaseCount++, requiresDelayedRelease: true, resourceSize: 2);
        _ = new TestResource(manager, () => releaseCount++, resourceSize: 4);
        manager.UnusedNotification(released);
        manager.UnusedNotification(delayed);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        manager.DestroyAllResources();
        manager.DestroyAllResources();

        Assert.AreEqual((0, 0u, 3),
            (manager.ResourceCount, manager.TotalVideoMemoryConsumption, releaseCount));
    }

    [TestMethod]
    public void WhenActiveReleaseQueuesCurrentFrameReleasedResourceThenFinalFlushDestroysItAfterActiveLists()
    {
        Direct3D9ResourceManager manager = new();
        List<string> releases = [];
        TestResource delayed = new(manager, () => releases.Add("delayed"), requiresDelayedRelease: true, resourceSize: 1);
        TestResource currentFrameReleased = new(manager, () => releases.Add("current-frame-released"), resourceSize: 2);
        _ = new TestResource(manager, () =>
        {
            releases.Add("active");
            manager.UnusedNotification(currentFrameReleased);
        }, resourceSize: 4);
        manager.UnusedNotification(delayed);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        manager.UnusedNotification(currentFrameReleased);

        manager.DestroyAllResources();
        manager.DestroyAllResources();

        Assert.AreEqual("delayed,active,current-frame-released", string.Join(',', releases));
        Assert.AreEqual((0, 0, 0, 0, 0u, 7u),
            (manager.ResourceCount, manager.ReleasedResourceCount, manager.DelayedReleasedResourceCount,
                manager.PendingResourceDestructionCount, manager.TotalVideoMemoryConsumption,
                manager.PeakVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenDestroyingAllResourcesOnEmptyManagerThenRepeatedCallsHaveNoEffect()
    {
        Direct3D9ResourceManager manager = new();

        manager.DestroyAllResources();
        manager.DestroyAllResources();

        Assert.AreEqual((0, 0u, false),
            (manager.ResourceCount, manager.TotalVideoMemoryConsumption, manager.AreActiveResources()));
    }

    [TestMethod]
    public void WhenDestroyingAllResourcesThenEveryRegistrationIsRemovedOnce()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        _ = new TestResource(manager, () => releaseCount++, resourceSize: 16);
        _ = new TestResource(manager, () => releaseCount++, resourceSize: 32);
        _ = new TestResource(manager, () => releaseCount++, resourceSize: 64);

        manager.DestroyAllResources();
        manager.DestroyAllResources();

        Assert.AreEqual((0, 0u, 3),
            (manager.ResourceCount, manager.TotalVideoMemoryConsumption, releaseCount));
    }

    [TestMethod]
    public void WhenDeviceClosesThenBorrowedManagerCountReachesZero()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        int releaseCount = 0;
        _ = new TestResource(manager, () => releaseCount++);
        _ = new TestResource(manager, () => releaseCount++);

        device.Dispose();
        device.Dispose();

        Assert.AreEqual((0, 2), (manager.ResourceCount, releaseCount));
    }

    [TestMethod]
    public void WhenRegisteringSizedResourcesThenPeakConsumptionTracksHighestTotal()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource zeroSizedResource = new(manager);
        using TestResource first = new(manager, resourceSize: 32);
        using TestResource second = new(manager, resourceSize: 64);

        Assert.AreEqual((96u, 96u),
            (manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenSizedResourceIsDestroyedThenPeakConsumptionDoesNotDecrease()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager, resourceSize: 64);

        resource.Dispose();

        Assert.AreEqual((0u, 64u),
            (manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenLargerTotalIsRegisteredAfterDestructionThenPeakConsumptionIncreases()
    {
        Direct3D9ResourceManager manager = new();
        TestResource first = new(manager, resourceSize: 32);
        first.Dispose();
        using TestResource second = new(manager, resourceSize: 96);

        Assert.AreEqual((96u, 96u),
            (manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenReleasedAndDelayedResourcesAreDestroyedThenPeakConsumptionIsPreserved()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource immediate = new(manager, resourceSize: 16);
        using TestResource delayed = new(manager, requiresDelayedRelease: true, resourceSize: 32);
        manager.UnusedNotification(immediate);
        manager.UnusedNotification(delayed);

        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        _ = manager.DestroyReleasedResourcesFromLastFrame();

        Assert.AreEqual((0u, 48u),
            (manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenNestedAndBatchDestructionCompletesThenPeakConsumptionIsPreserved()
    {
        Direct3D9ResourceManager manager = new();
        TestResource? nested = null;
        _ = new TestResource(manager, () => manager.DestroyResource(nested!), resourceSize: 16);
        nested = new TestResource(manager, resourceSize: 32);
        _ = new TestResource(manager, resourceSize: 64);

        manager.DestroyAllResources();

        Assert.AreEqual((0u, 112u),
            (manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenDeviceClosesThenPeakConsumptionIsPreserved()
    {
        Direct3D9Device device = CreateDevice(_ => { });
        Direct3D9ResourceManager manager = device.ResourceManager;
        _ = new TestResource(manager, resourceSize: 128);

        device.Dispose();

        Assert.AreEqual((0u, 128u),
            (manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenDuplicateRegistrationFailsThenPeakConsumptionIsUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        using TestResource resource = new(manager, resourceSize: 32);

        Assert.ThrowsExactly<InvalidOperationException>(() => manager.RegisterResource(resource));

        Assert.AreEqual((32u, 32u),
            (manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenForeignRegistrationFailsThenBothPeakConsumptionsAreUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        Direct3D9ResourceManager foreignManager = new();
        using TestResource resource = new(foreignManager, resourceSize: 32);

        Assert.ThrowsExactly<InvalidOperationException>(() => manager.RegisterResource(resource));

        Assert.AreEqual((0u, 0u, 32u, 32u),
            (manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption,
                foreignManager.TotalVideoMemoryConsumption, foreignManager.PeakVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenVideoMemorySubtractionWouldUnderflowThenAccountingAndResourceRemainUnchanged()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        TestResource resource = new(manager, () => releaseCount++, resourceSize: 32);
        System.Reflection.PropertyInfo totalConsumptionProperty = typeof(Direct3D9ResourceManager)
            .GetProperty(
                nameof(Direct3D9ResourceManager.TotalVideoMemoryConsumption),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        totalConsumptionProperty.SetValue(manager, 0u);

        Assert.ThrowsExactly<InvalidOperationException>(() => manager.DestroyResource(resource));

        Assert.AreEqual((0u, 32u, 1, 0, true, true),
            (manager.TotalVideoMemoryConsumption, manager.PeakVideoMemoryConsumption,
                manager.ResourceCount, releaseCount, resource.IsValid, resource.IsManaged));

        totalConsumptionProperty.SetValue(manager, 32u);
        resource.Dispose();
    }

    [TestMethod]
    public void WhenReleaseCallbackFailsThenResourceIsDetachedAndUntracked()
    {
        Direct3D9ResourceManager manager = new();
        TestResource resource = new(manager, () => throw new InvalidOperationException("Release failed."), resourceSize: 32);

        Assert.ThrowsExactly<InvalidOperationException>(() => manager.DestroyResource(resource));

        Assert.AreEqual((0, 0u, false, false),
            (manager.ResourceCount, manager.TotalVideoMemoryConsumption, resource.IsValid, resource.IsManaged));
    }

    [TestMethod]
    public void WhenReleaseCallbackFailsThenRepeatedDisposeDoesNotRestoreManagerAssociation()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        TestResource resource = new(manager, () =>
        {
            releaseCount++;
            throw new InvalidOperationException("Release failed.");
        });
        Assert.ThrowsExactly<InvalidOperationException>(() => manager.DestroyResource(resource));

        resource.Dispose();

        Assert.AreEqual((1, 0, false), (releaseCount, manager.ResourceCount, resource.IsManaged));
    }

    [TestMethod]
    public void WhenCurrentFrameReleaseCallbackFailsThenUnconsumedResourcesRemainQueuedForRetry()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        TestResource retained = new(manager, () => releaseCount++);
        TestResource failing = new(manager, () => throw new InvalidOperationException("Release failed."));
        manager.UnusedNotification(retained);
        manager.UnusedNotification(failing);

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithoutDelay));
        uint retryCount = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithoutDelay);

        Assert.AreEqual((1u, 1, 0, 0, false),
            (retryCount, releaseCount, manager.ResourceCount, manager.ReleasedResourceCount, retained.IsManaged));
    }

    [TestMethod]
    public void WhenLastFrameReleaseCallbackFailsThenUnconsumedResourcesRemainQueuedForRetry()
    {
        Direct3D9ResourceManager manager = new();
        int releaseCount = 0;
        TestResource retained = new(manager, () => releaseCount++, requiresDelayedRelease: true);
        TestResource failing = new(
            manager,
            () => throw new InvalidOperationException("Release failed."),
            requiresDelayedRelease: true);
        manager.UnusedNotification(failing);
        manager.UnusedNotification(retained);
        _ = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);

        Assert.ThrowsExactly<InvalidOperationException>(() => manager.DestroyReleasedResourcesFromLastFrame());
        uint retryCount = manager.DestroyReleasedResourcesFromLastFrame();

        Assert.AreEqual((1u, 1, 0, 0, false),
            (retryCount, releaseCount, manager.ResourceCount, manager.DelayedReleasedResourceCount, retained.IsManaged));
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

        internal TestResource(
            Direct3D9ResourceManager manager,
            Action? releaseAction = null,
            bool requiresDelayedRelease = false,
            bool isEvictable = false,
            uint resourceSize = 0)
            : base(manager, isEvictable, resourceSize)
        {
            _releaseAction = releaseAction;
            RequiresDelayedRelease = requiresDelayedRelease;
        }

        internal override bool RequiresDelayedRelease { get; }

        internal Direct3D9ResourceManager ManagerForTest => Manager;

        internal Direct3D9Device DeviceForTest => Device;

        internal bool WasManagedDuringRelease { get; private set; }

        internal bool WasValidDuringRelease { get; private set; }

        internal void InvalidateForTest()
        {
            InvalidateFromManager();
        }

        protected override void ReleaseD3DResources()
        {
            WasManagedDuringRelease = IsManaged;
            WasValidDuringRelease = IsValid;
            _releaseAction?.Invoke();
        }
    }
}
