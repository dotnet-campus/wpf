using Microsoft.Win32;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9RegistryDatabaseTests
{
    [TestMethod]
    public void WhenDisableHardwareAccelerationValueIsMissingThenAdaptersAreEnabled()
    {
        Direct3D9RegistryDatabase database = CreateDatabase(default);

        Assert.IsTrue(database.IsAdapterEnabled(1));
    }

    [TestMethod]
    public void WhenDisableHardwareAccelerationDwordIsZeroThenAdaptersAreEnabled()
    {
        Direct3D9RegistryDatabase database = CreateDatabase(
            new Direct3D9RegistryValue(WasRead: true, RegistryValueKind.DWord, DwordValue: 0));

        Assert.IsTrue(database.IsAdapterEnabled(1));
    }

    [TestMethod]
    public void WhenDisableHardwareAccelerationDwordIsNonZeroThenAdaptersAreDisabled()
    {
        Direct3D9RegistryDatabase database = CreateDatabase(
            new Direct3D9RegistryValue(WasRead: true, RegistryValueKind.DWord, DwordValue: 1));

        Assert.IsFalse(database.IsAdapterEnabled(1));
    }

    [TestMethod]
    public void WhenDisableHardwareAccelerationHasUnexpectedTypeThenAdaptersAreDisabled()
    {
        Direct3D9RegistryDatabase database = CreateDatabase(
            new Direct3D9RegistryValue(WasRead: true, RegistryValueKind.String, DwordValue: 0));

        Assert.IsFalse(database.IsAdapterEnabled(1));
    }

    [TestMethod]
    public void WhenUnexpectedErrorCountIsBelowThresholdThenAdapterRemainsEnabled()
    {
        Direct3D9RegistryDatabase database = CreateDatabase(default);

        for (int index = 0; index < 4; index++)
        {
            database.HandleAdapterUnexpectedError(1);
        }

        Assert.IsTrue(database.IsAdapterEnabled(1));
    }

    [TestMethod]
    public void WhenUnexpectedErrorCountReachesThresholdThenAdapterIsDisabled()
    {
        Direct3D9RegistryDatabase database = CreateDatabase(default);

        for (int index = 0; index < 5; index++)
        {
            database.HandleAdapterUnexpectedError(1);
        }

        Assert.IsFalse(database.IsAdapterEnabled(1));
    }

    [TestMethod]
    public void WhenUnexpectedErrorsContinuePastThresholdThenAdapterRemainsDisabled()
    {
        Direct3D9RegistryDatabase database = CreateDatabase(default);

        for (int index = 0; index < 7; index++)
        {
            database.HandleAdapterUnexpectedError(1);
        }

        Assert.IsFalse(database.IsAdapterEnabled(1));
    }

    [TestMethod]
    public void WhenAdapterIsDisabledExplicitlyThenItIsImmediatelyUnavailable()
    {
        Direct3D9RegistryDatabase database = CreateDatabase(default);

        database.DisableAdapter(1);

        Assert.IsFalse(database.IsAdapterEnabled(1));
    }

    [TestMethod]
    public void WhenAdapterIsOutsideInitializedRangeThenRequestIsRejected()
    {
        Direct3D9RegistryDatabase database = CreateDatabase(default);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => database.IsAdapterEnabled(2));
    }

    [TestMethod]
    public void WhenDisablingAdapterOutsideInitializedRangeThenRequestIsRejected()
    {
        Direct3D9RegistryDatabase database = CreateDatabase(default);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => database.DisableAdapter(2));
    }

    [TestMethod]
    public void WhenHandlingUnexpectedErrorOutsideInitializedRangeThenRequestIsRejected()
    {
        Direct3D9RegistryDatabase database = CreateDatabase(default);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => database.HandleAdapterUnexpectedError(2));
    }

    private static Direct3D9RegistryDatabase CreateDatabase(Direct3D9RegistryValue value)
    {
        return new Direct3D9RegistryDatabase(adapterCount: 2, () => value);
    }
}
