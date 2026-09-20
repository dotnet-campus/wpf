using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9DisplayModeSnapshotTests
{
    private const int Direct3DNotAvailableHResult = unchecked((int) 0x8876086A);
    private const int Direct3DDeviceLostHResult = unchecked((int) 0x88760868);
    private const int Direct3DDriverInternalErrorHResult = unchecked((int) 0x88760827);

    [TestMethod]
    public void WhenDirect3D9ExReportsModeThenCompleteModeAndRotationArePreserved()
    {
        int gdiCalls = 0;

        ImmutableArray<Direct3D9DisplayModeSnapshot> result = Direct3D9DisplayModeSnapshotFactory.Read(
            Adapters(1, "display"),
            null,
            (uint _, out Direct3D9DisplayMode mode, out Displayrotation rotation) =>
            {
                mode = new(24, 1920, 1080, 144, Format.A2R10G10B10, Scanlineordering.Progressive);
                rotation = Displayrotation.Displayrotation90;
                return 0;
            },
            (string _, out Direct3D9GdiDisplayMode mode, out int lastError) =>
            {
                gdiCalls++;
                mode = default;
                lastError = 0;
                return true;
            });

        Assert.AreEqual(
            (new Direct3D9DisplayMode(24, 1920, 1080, 144, Format.A2R10G10B10, Scanlineordering.Progressive), Displayrotation.Displayrotation90, 32u, 0),
            (result[0].DisplayMode, result[0].DisplayRotation, result[0].BitsPerPixel, gdiCalls));
    }

    [TestMethod]
    public void WhenOnlySomeDisplaysAreDirect3DAdaptersThenRemainingDisplaysUseGdiOnly()
    {
        List<uint> direct3DRequests = [];
        List<string> gdiRequests = [];

        ImmutableArray<Direct3D9DisplayModeSnapshot> result = Direct3D9DisplayModeSnapshotFactory.Read(
            Adapters(1, "first", "second"),
            (uint displayIndex, out Direct3D9DisplayMode mode) =>
            {
                direct3DRequests.Add(displayIndex);
                mode = new(20, 1280, 720, 60, Format.X8R8G8B8, Scanlineordering.Unknown);
                return 0;
            },
            null,
            (string deviceName, out Direct3D9GdiDisplayMode mode, out int lastError) =>
            {
                gdiRequests.Add(deviceName);
                mode = new(1024, 768, 75, 16, 0, 0);
                lastError = 0;
                return true;
            });

        CollectionAssert.AreEqual(
            new[] { "d3d:0", "gdi:second", "formats:X8R8G8B8,R5G6B5" },
            new[]
            {
                $"d3d:{string.Join(',', direct3DRequests)}",
                $"gdi:{string.Join(',', gdiRequests)}",
                $"formats:{string.Join(',', result.Select(item => item.DisplayMode.Format))}"
            });
    }

    [TestMethod]
    public void WhenDirect3DModeIsNotAvailableThenGdiFillsModeFormatRotationAndBitsPerPixel()
    {
        ImmutableArray<Direct3D9DisplayModeSnapshot> result = Direct3D9DisplayModeSnapshotFactory.Read(
            Adapters(1, "display"),
            (uint _, out Direct3D9DisplayMode mode) =>
            {
                mode = default;
                return Direct3DNotAvailableHResult;
            },
            null,
            (string _, out Direct3D9GdiDisplayMode mode, out int lastError) =>
            {
                mode = new(1600, 900, 60, 24, 0x80, 3);
                lastError = 0;
                return true;
            });

        Assert.AreEqual(
            (1600u, 900u, 60u, Format.R8G8B8, Displayrotation.Displayrotation270, 24u),
            (result[0].DisplayMode.Width, result[0].DisplayMode.Height, result[0].DisplayMode.RefreshRate,
                result[0].DisplayMode.Format, result[0].DisplayRotation, result[0].BitsPerPixel));
    }

    [DataTestMethod]
    [DataRow(32u, Format.X8R8G8B8)]
    [DataRow(24u, Format.R8G8B8)]
    [DataRow(16u, Format.R5G6B5)]
    [DataRow(8u, Format.P8)]
    [DataRow(12u, Format.Unknown)]
    public void WhenGdiReportsBitsPerPixelThenNativeFormatMappingIsUsed(uint bitsPerPixel, Format expectedFormat)
    {
        ImmutableArray<Direct3D9DisplayModeSnapshot> result = Direct3D9DisplayModeSnapshotFactory.Read(
            Adapters(0, "display"),
            null,
            null,
            (string _, out Direct3D9GdiDisplayMode mode, out int lastError) =>
            {
                mode = new(800, 600, 60, bitsPerPixel, 0, 0);
                lastError = 0;
                return true;
            });

        Assert.AreEqual(expectedFormat, result[0].DisplayMode.Format);
    }

    [DataTestMethod]
    [DataRow(Direct3DDeviceLostHResult, Direct3D9Factory.DisplayStateInvalidHResult)]
    [DataRow(Direct3DDriverInternalErrorHResult, Direct3D9Factory.DisplayStateInvalidHResult)]
    [DataRow(unchecked((int) 0x80004005), unchecked((int) 0x80004005))]
    public void WhenDirect3DModeReadFailsThenNativeHResultTranslationIsUsed(int sourceHResult, int expectedHResult)
    {
        COMException exception = Assert.ThrowsExactly<COMException>(() => Direct3D9DisplayModeSnapshotFactory.Read(
            Adapters(1, "display"),
            (uint _, out Direct3D9DisplayMode mode) =>
            {
                mode = default;
                return sourceHResult;
            },
            null,
            GdiSuccess));

        Assert.AreEqual(expectedHResult, exception.HResult);
    }

    [DataTestMethod]
    [DataRow(0, Direct3D9Factory.DisplayStateInvalidHResult)]
    [DataRow(1461, Direct3D9Factory.DisplayStateInvalidHResult)]
    [DataRow(1801, Direct3D9Factory.DisplayStateInvalidHResult)]
    [DataRow(5, unchecked((int) 0x80070005))]
    public void WhenGdiModeReadFailsThenLastErrorIsInspectedLikeNativeCode(int lastError, int expectedHResult)
    {
        Exception exception = Assert.Throws<Exception>(() => Direct3D9DisplayModeSnapshotFactory.Read(
            Adapters(0, "display"),
            null,
            null,
            (string _, out Direct3D9GdiDisplayMode mode, out int error) =>
            {
                mode = default;
                error = lastError;
                return false;
            }));

        Assert.AreEqual(expectedHResult, exception.HResult);
    }

    private static Direct3D9AdapterArrangementResult Adapters(uint adapterCount, params string[] deviceNames)
    {
        return new Direct3D9AdapterArrangementResult(
            adapterCount,
            deviceNames.Select((deviceName, index) => new Direct3D9AdapterSnapshot(
                (uint) index,
                index + 1,
                index + 1,
                ImmutableArray<Direct3D9DisplayBounds>.Empty,
                deviceName,
                1)).ToImmutableArray());
    }

    private static bool GdiSuccess(string _, out Direct3D9GdiDisplayMode mode, out int lastError)
    {
        mode = new(800, 600, 60, 32, 0, 0);
        lastError = 0;
        return true;
    }
}
