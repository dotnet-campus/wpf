using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9DisplayDeviceSnapshotTests
{
    private const uint AttachedToDesktop = 0x00000001;
    private const uint MirroringDriver = 0x00000008;
    private const uint Remote = 0x04000000;

    [TestMethod]
    public void WhenDevicesAreEnumeratedThenOnlyUsableDisplaysAreCapturedInOrder()
    {
        Direct3D9DisplayDeviceEnumerationEntry[] entries =
        [
            Entry("detached", 0),
            Entry("mirror", AttachedToDesktop | MirroringDriver),
            Entry("local", AttachedToDesktop),
            Entry("remote", AttachedToDesktop | Remote),
        ];

        Direct3D9DisplayDeviceEnumerationResult result = Enumerate(entries, true);

        CollectionAssert.AreEqual(new[] { "local", "remote" }, result.Devices.Select(device => device.DeviceName).ToArray());
    }

    [TestMethod]
    public void WhenRemoteOrMirroringDeviceIsAttachedThenNonLocalDeviceIsReported()
    {
        Direct3D9DisplayDeviceEnumerationResult result = Enumerate(
            [Entry("mirror", AttachedToDesktop | MirroringDriver)],
            true);

        Assert.IsTrue(result.IsNonLocalDevicePresent);
    }

    [TestMethod]
    public void WhenMultiAdapterCodeIsDisabledThenEnumerationStopsAfterFirstUsableDevice()
    {
        int enumerationCount = 0;
        Direct3D9DisplayDeviceEnumerationEntry[] entries =
        [
            Entry("detached", 0),
            Entry("first", AttachedToDesktop),
            Entry("second", AttachedToDesktop),
        ];

        Direct3D9DisplayDeviceEnumerationResult result = Direct3D9DisplayDeviceSnapshotFactory.Enumerate(
            index =>
            {
                enumerationCount++;
                return index < entries.Length ? entries[index] : default;
            },
            false);

        Assert.AreEqual(("first", 2), (result.Devices.Single().DeviceName, enumerationCount));
    }

    [TestMethod]
    public void WhenDeviceNameIsEmptyThenInvalidArgumentIsReported()
    {
        ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => Enumerate(
            [new Direct3D9DisplayDeviceEnumerationEntry(true, new char[32], AttachedToDesktop)],
            true));

        Assert.AreEqual(unchecked((int) 0x80070057), exception.HResult);
    }

    [TestMethod]
    public void WhenDeviceNameHasNoTerminatorThenInvalidArgumentIsReported()
    {
        ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => Enumerate(
            [new Direct3D9DisplayDeviceEnumerationEntry(true, new string('x', 32).ToCharArray(), AttachedToDesktop)],
            true));

        Assert.AreEqual(unchecked((int) 0x80070057), exception.HResult);
    }

    [TestMethod]
    public void WhenEnumeratorReportsFailureThenItsHResultIsPreserved()
    {
        const int failure = unchecked((int) 0x8007001F);

        COMException exception = Assert.ThrowsExactly<COMException>(() => Direct3D9DisplayDeviceSnapshotFactory.Enumerate(
            _ =>
            {
                Marshal.ThrowExceptionForHR(failure);
                return default;
            },
            true));

        Assert.AreEqual(failure, exception.HResult);
    }

    private static Direct3D9DisplayDeviceEnumerationResult Enumerate(
        Direct3D9DisplayDeviceEnumerationEntry[] entries,
        bool isMultiAdapterCodeEnabled)
    {
        return Direct3D9DisplayDeviceSnapshotFactory.Enumerate(
            index => index < entries.Length ? entries[index] : default,
            isMultiAdapterCodeEnabled);
    }

    private static Direct3D9DisplayDeviceEnumerationEntry Entry(string deviceName, uint stateFlags)
    {
        char[] buffer = new char[32];
        deviceName.AsSpan().CopyTo(buffer);
        return new Direct3D9DisplayDeviceEnumerationEntry(true, buffer, stateFlags);
    }
}
