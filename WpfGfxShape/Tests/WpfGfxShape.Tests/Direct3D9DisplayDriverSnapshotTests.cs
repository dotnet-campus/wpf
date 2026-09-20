using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9DisplayDriverSnapshotTests
{
    [TestMethod]
    public void WhenDeviceRegistryValuesExistThenMemoryAndFirstDriverAreCaptured()
    {
        Direct3D9DisplayDriverSnapshot result = Direct3D9DisplayDriverSnapshotFactory.ReadDeviceRegistry(
            @"\Registry\Machine\System\Adapter",
            path =>
            {
                Assert.AreEqual(@"System\Adapter", path);
                return (unchecked((int) 0x80000000), new[] { "primary", "secondary" });
            });

        Assert.AreEqual((0x80000000U, "primary"), (result.MemorySize, result.InstalledDisplayDriver));
    }

    [TestMethod]
    public void WhenDeviceKeyDoesNotUseMachinePrefixThenRegistryIsNotRead()
    {
        bool wasRead = false;

        Direct3D9DisplayDriverSnapshot result = Direct3D9DisplayDriverSnapshotFactory.ReadDeviceRegistry(
            @"System\Adapter",
            _ =>
            {
                wasRead = true;
                return default;
            });

        Assert.AreEqual((false, default(Direct3D9DisplayDriverSnapshot)), (wasRead, result));
    }

    [TestMethod]
    public void WhenRequiredDriverDateIsValidThenFileTimeIsReturned()
    {
        ulong result = Direct3D9DisplayDriverSnapshotFactory.ReadRequiredVideoDriverDate(() => "2004/11/01");

        Assert.AreEqual(unchecked((ulong) new DateTime(2004, 11, 1, 0, 0, 0, DateTimeKind.Utc).ToFileTimeUtc()), result);
    }

    [TestMethod]
    public void WhenRequiredDriverDateIsInvalidThenNativeDefaultIsReturned()
    {
        ulong result = Direct3D9DisplayDriverSnapshotFactory.ReadRequiredVideoDriverDate(() => "invalid");

        Assert.AreEqual(Direct3D9DisplayDriverSnapshotFactory.DefaultRequiredVideoDriverDate, result);
    }

    [TestMethod]
    public void WhenDriverFileCannotBeFoundThenDriverIsConsideredRecent()
    {
        bool result = Direct3D9DisplayDriverSnapshotFactory.CheckForRecentDriver(
            "driver",
            100,
            _ => null);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void WhenDriverFilePredatesRequirementThenDriverIsNotRecent()
    {
        bool result = Direct3D9DisplayDriverSnapshotFactory.CheckForRecentDriver(
            "driver",
            100,
            _ => 99);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void WhenDriverFileMeetsRequirementThenDriverIsRecent()
    {
        bool result = Direct3D9DisplayDriverSnapshotFactory.CheckForRecentDriver(
            "driver",
            100,
            _ => 100);

        Assert.IsTrue(result);
    }
}
