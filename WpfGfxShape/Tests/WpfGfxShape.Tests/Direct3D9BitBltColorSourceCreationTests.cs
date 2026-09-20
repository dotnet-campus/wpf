using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitBltColorSourceCreationTests
{
    [TestMethod]
    public void WhenCreationSucceedsThenNativeCreationOrderAndDependentFlagArePreserved()
    {
        List<string> calls = [];
        FakeDisposable colorSource = new("color", calls);

        int result = Direct3D9BitBltColorSourceCreation.TryCreate(
            isDependent: true,
            (out SurfaceDesc description, out uint levels) =>
            {
                calls.Add("prepare");
                description = CreateDescription();
                levels = 1;
                return Direct3D9Factory.SuccessHResult;
            },
            (SurfaceDesc description, uint levels, bool isDependent, out IDisposable? initializedColorSource) =>
            {
                calls.Add($"initialize:{description.Usage}:{levels}:{isDependent}");
                initializedColorSource = colorSource;
                return Direct3D9Factory.SuccessHResult;
            },
            format =>
            {
                calls.Add($"check:{format}");
                return Direct3D9Factory.SuccessHResult;
            },
            (uint width, uint height, Format format, MultisampleType multisampleType, uint quality, bool lockable, out nint surface) =>
            {
                calls.Add($"create:{width}:{height}:{format}:{multisampleType}:{quality}:{lockable}");
                surface = 71;
                return Direct3D9Factory.SuccessHResult;
            },
            surface => calls.Add($"release:{surface}"),
            out Direct3D9BitBltColorSourceCreation? creation);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|71|prepare|initialize:{D3D9.UsageRendertarget}:1:True|check:A8R8G8B8|create:80:60:A8R8G8B8:MultisampleNone:0:True",
            $"{result}|{creation!.GetValidTransferSurface(true)}|{string.Join('|', calls)}");

        creation.Dispose();
        Assert.AreEqual("release:71|dispose:color", string.Join('|', calls[^2..]));
    }

    [TestMethod]
    public void WhenPreparationFailsThenNoColorSourceOrTransferSurfaceIsCreated()
    {
        int initializeCalls = 0;
        int checkCalls = 0;

        int result = Direct3D9BitBltColorSourceCreation.TryCreate(
            false,
            (out SurfaceDesc description, out uint levels) =>
            {
                description = default;
                levels = 0;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            (SurfaceDesc _, uint _, bool _, out IDisposable? colorSource) =>
            {
                initializeCalls++;
                colorSource = null;
                return Direct3D9Factory.SuccessHResult;
            },
            _ =>
            {
                checkCalls++;
                return Direct3D9Factory.SuccessHResult;
            },
            CreateUnexpectedSurface,
            _ => { },
            out Direct3D9BitBltColorSourceCreation? creation);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, 0, 0, null),
            (result, initializeCalls, checkCalls, creation));
    }

    [TestMethod]
    public void WhenInitializationFailsWithColorSourceThenColorSourceIsReleasedAndSurfaceIsNotChecked()
    {
        List<string> calls = [];
        FakeDisposable colorSource = new("color", calls);

        int result = Direct3D9BitBltColorSourceCreation.TryCreate(
            false,
            Prepare,
            (SurfaceDesc _, uint _, bool _, out IDisposable? initializedColorSource) =>
            {
                calls.Add("initialize");
                initializedColorSource = colorSource;
                return Direct3D9Factory.InvalidCallHResult;
            },
            _ =>
            {
                calls.Add("check");
                return Direct3D9Factory.SuccessHResult;
            },
            CreateUnexpectedSurface,
            _ => { },
            out Direct3D9BitBltColorSourceCreation? creation);

        Assert.AreEqual(
            $"{Direct3D9Factory.InvalidCallHResult}|initialize|dispose:color|",
            $"{result}|{string.Join('|', calls)}|{creation}");
    }

    [TestMethod]
    public void WhenTransferSurfaceCreationFailsThenColorSourceIsReleasedAfterReturnedSurface()
    {
        List<string> calls = [];
        FakeDisposable colorSource = new("color", calls);

        int result = Direct3D9BitBltColorSourceCreation.TryCreate(
            false,
            Prepare,
            (SurfaceDesc _, uint _, bool _, out IDisposable? initializedColorSource) =>
            {
                initializedColorSource = colorSource;
                return Direct3D9Factory.SuccessHResult;
            },
            _ => Direct3D9Factory.SuccessHResult,
            (uint _, uint _, Format _, MultisampleType _, uint _, bool _, out nint surface) =>
            {
                surface = 83;
                return Direct3D9Factory.NotAvailableHResult;
            },
            surface => calls.Add($"release:{surface}"),
            out Direct3D9BitBltColorSourceCreation? creation);

        Assert.AreEqual(
            $"{Direct3D9Factory.NotAvailableHResult}|release:83|dispose:color|",
            $"{result}|{string.Join('|', calls)}|{creation}");
    }

    [TestMethod]
    public void WhenDisposedThenCreationIsIdempotentAndProtectedFromFurtherUse()
    {
        List<string> calls = [];
        FakeDisposable colorSource = new("color", calls);
        _ = Direct3D9BitBltColorSourceCreation.TryCreate(
            false,
            Prepare,
            (SurfaceDesc _, uint _, bool _, out IDisposable? initializedColorSource) =>
            {
                initializedColorSource = colorSource;
                return Direct3D9Factory.SuccessHResult;
            },
            _ => Direct3D9Factory.SuccessHResult,
            (uint _, uint _, Format _, MultisampleType _, uint _, bool _, out nint surface) =>
            {
                surface = 97;
                return Direct3D9Factory.SuccessHResult;
            },
            surface => calls.Add($"release:{surface}"),
            out Direct3D9BitBltColorSourceCreation? creation);

        creation!.Dispose();
        creation.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => creation.GetValidTransferSurface(true));
        Assert.AreEqual("release:97|dispose:color", string.Join('|', calls));
    }

    private static int Prepare(out SurfaceDesc description, out uint levels)
    {
        description = CreateDescription();
        levels = 1;
        return Direct3D9Factory.SuccessHResult;
    }

    private static SurfaceDesc CreateDescription() => new(
        format: Format.A8R8G8B8,
        usage: D3D9.UsageRendertarget,
        width: 80,
        height: 60);

    private static int CreateUnexpectedSurface(
        uint width,
        uint height,
        Format format,
        MultisampleType multisampleType,
        uint multisampleQuality,
        bool lockable,
        out nint surface)
    {
        surface = 0;
        Assert.Fail("The transfer surface must not be created.");
        return Direct3D9Factory.SuccessHResult;
    }

    private sealed class FakeDisposable(string name, List<string> calls) : IDisposable
    {
        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            calls.Add($"dispose:{name}");
        }
    }
}
