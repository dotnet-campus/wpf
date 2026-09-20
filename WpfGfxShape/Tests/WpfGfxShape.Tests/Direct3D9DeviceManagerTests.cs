using System.Collections.Immutable;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceManagerTests
{
    [TestMethod]
    public void WhenCreatingDeviceThenManagerTracksItAsUsable()
    {
        using Direct3D9DeviceManager manager = CreateManager();

        manager.CreateDevice(CreateParameters());

        Assert.AreEqual(1, manager.UsableDeviceCount);
    }

    [TestMethod]
    public void WhenMarkingDeviceUnusableThenManagerMovesItToUnusablePartition()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9Device device = manager.CreateDevice(CreateParameters());

        device.MarkUnusable();

        Assert.AreEqual(1, manager.UnusableDeviceCount);
    }

    [TestMethod]
    public void WhenMarkingDeviceUnusableTwiceThenInvalidStatusIsNotRepeated()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        int invalidNotificationCount = 0;
        manager.AddAdapterStatusListener((_, isValid) => invalidNotificationCount += isValid ? 0 : 1);
        Direct3D9Device device = manager.CreateDevice(CreateParameters());

        device.MarkUnusable();
        device.MarkUnusable();

        Assert.AreEqual(1, invalidNotificationCount);
    }

    [TestMethod]
    public void WhenMarkingDeviceUnusableThenInvalidStatusIsNotifiedBeforePartitionMove()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        (int Usable, int Unusable) countsAtNotification = (-1, -1);
        manager.AddAdapterStatusListener((_, isValid) =>
        {
            if (!isValid)
            {
                countsAtNotification = (manager.UsableDeviceCount, manager.UnusableDeviceCount);
            }
        });
        Direct3D9Device device = manager.CreateDevice(CreateParameters());

        device.MarkUnusable();

        Assert.AreEqual(((1, 0), 0, 1),
            (countsAtNotification, manager.UsableDeviceCount, manager.UnusableDeviceCount));
    }

    [TestMethod]
    public void WhenDisposingUsableDeviceThenInvalidStatusIsNotifiedBeforeTrackingRemoval()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        (int Usable, int Unusable) countsAtNotification = (-1, -1);
        manager.AddAdapterStatusListener((_, isValid) =>
        {
            if (!isValid)
            {
                countsAtNotification = (manager.UsableDeviceCount, manager.UnusableDeviceCount);
            }
        });
        Direct3D9Device device = manager.CreateDevice(CreateParameters());

        device.Dispose();

        Assert.AreEqual(((1, 0), 0, 0),
            (countsAtNotification, manager.UsableDeviceCount, manager.UnusableDeviceCount));
    }

    [TestMethod]
    public void WhenDisposingTrackedDeviceThenManagerRemovesIt()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9Device device = manager.CreateDevice(CreateParameters());

        device.Dispose();

        Assert.AreEqual(0, manager.UsableDeviceCount + manager.UnusableDeviceCount);
    }

    [TestMethod]
    public void WhenDisposingUsableTrackedDeviceThenAdapterInvalidIsNotified()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        List<bool> statuses = [];
        manager.AddAdapterStatusListener((_, isValid) => statuses.Add(isValid));
        Direct3D9Device device = manager.CreateDevice(CreateParameters());

        device.Dispose();

        CollectionAssert.AreEqual(new[] { true, false }, statuses);
        Assert.AreEqual(0, manager.UsableDeviceCount + manager.UnusableDeviceCount);
    }

    [TestMethod]
    public void WhenDisposingAlreadyUnusableDeviceThenInvalidStatusIsNotRepeated()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        List<bool> statuses = [];
        manager.AddAdapterStatusListener((_, isValid) => statuses.Add(isValid));
        Direct3D9Device device = manager.CreateDevice(CreateParameters());
        device.MarkUnusable();

        device.Dispose();

        CollectionAssert.AreEqual(new[] { true, false }, statuses);
        Assert.AreEqual(0, manager.UsableDeviceCount + manager.UnusableDeviceCount);
    }

    [TestMethod]
    public void WhenDisposingIndependentSoftwareDeviceThenAdapterInvalidIsNotNotified()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        int invalidNotificationCount = 0;
        manager.AddAdapterStatusListener((_, isValid) => invalidNotificationCount += isValid ? 0 : 1);
        Direct3D9Device device = manager.GetSoftwareDevice();

        device.Dispose();

        Assert.AreEqual((0, 0), (invalidNotificationCount, manager.UsableDeviceCount + manager.UnusableDeviceCount));
    }

    [TestMethod]
    public void WhenIndependentAndTrackedSoftwareDevicesCoexistThenDisposingIndependentDevicePreservesTrackedDevice()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            });
        List<bool> statuses = [];
        manager.AddAdapterStatusListener((_, isValid) => statuses.Add(isValid));
        Direct3D9Device trackedDevice = manager.CreateDevice(CreateParameters() with { DeviceType = Devtype.SW });
        Direct3D9Device independentDevice = manager.GetSoftwareDevice();

        independentDevice.Dispose();
        Direct3D9Device replacementIndependentDevice = manager.GetSoftwareDevice();
        Direct3D9DeviceCreationParameters trackedParameters = CreateParameters() with { DeviceType = Devtype.SW };
        Direct3D9Device reusedTrackedDevice = manager.GetOrCreateDevice(ref trackedParameters);

        Assert.AreEqual(
            (1, 0, 3, true, false, "True"),
            (manager.UsableDeviceCount,
             manager.UnusableDeviceCount,
             creationCount,
             ReferenceEquals(trackedDevice, reusedTrackedDevice),
             ReferenceEquals(independentDevice, replacementIndependentDevice),
             string.Join('|', statuses)));
    }

    [TestMethod]
    public void WhenIndependentAndTrackedSoftwareDevicesCoexistThenDisposingTrackedDevicePreservesIndependentDevice()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            });
        List<bool> statuses = [];
        manager.AddAdapterStatusListener((_, isValid) => statuses.Add(isValid));
        Direct3D9DeviceCreationParameters trackedParameters = CreateParameters() with { DeviceType = Devtype.SW };
        Direct3D9Device trackedDevice = manager.CreateDevice(trackedParameters);
        Direct3D9Device independentDevice = manager.GetSoftwareDevice();

        trackedDevice.Dispose();
        Direct3D9Device reusedIndependentDevice = manager.GetSoftwareDevice();

        Assert.AreEqual(
            (0, 0, 2, true, "True|False"),
            (manager.UsableDeviceCount,
             manager.UnusableDeviceCount,
             creationCount,
             ReferenceEquals(independentDevice, reusedIndependentDevice),
             string.Join('|', statuses)));
    }

    [TestMethod]
    public void WhenRequestMatchesUsableDeviceThenExistingDeviceIsReused()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9DeviceCreationParameters parameters = CreateParameters();
        Direct3D9Device expected = manager.CreateDevice(parameters);

        Direct3D9Device actual = manager.GetOrCreateDevice(ref parameters);

        Assert.AreSame(expected, actual);
    }

    [TestMethod]
    public void WhenHardwareRequestUsesDifferentFocusWindowThenExistingDeviceIsReusedWithRequestedPresentParameters()
    {
        List<Direct3D9DeviceCreationParameters> creations = [];
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creations.Add(parameters);
                return CreateDevice(parameters, unusable, disposed);
            },
            isWindow: _ => true);
        Direct3D9DeviceRequest initialRequest = new(123, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);
        Direct3D9Device expected = manager.GetDeviceAndPresentParameters(initialRequest, default).Device;
        Direct3D9DeviceRequest subsequentRequest = initialRequest with { FocusWindow = 456 };

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(subsequentRequest, default);

        Assert.AreEqual(
            (true, 1, (nint) 123, (nint) 456),
            (ReferenceEquals(expected, result.Device), creations.Count, result.Device.FocusWindow, result.PresentParameters.HDeviceWindow));
    }

    [TestMethod]
    public void WhenMatchingDeviceIsUnusableThenNewDeviceIsCreated()
    {
        List<Direct3D9DeviceCreationParameters> creations = [];
        using Direct3D9DeviceManager manager = CreateManager(creations);
        Direct3D9DeviceCreationParameters parameters = CreateParameters();
        Direct3D9Device unusableDevice = manager.CreateDevice(parameters);
        unusableDevice.MarkUnusable();

        manager.GetOrCreateDevice(ref parameters);

        Assert.AreEqual(2, creations.Count);
    }

    [TestMethod]
    public void WhenBehaviorFlagsDifferOnlyByDriverManagementFlagThenActualFlagsAreReturned()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9DeviceCreationParameters createdParameters = CreateParameters() with
        {
            BehaviorFlags = CreateParameters().BehaviorFlags & unchecked((uint) ~D3D9.CreateDisableDriverManagementEX)
        };
        Direct3D9Device expected = manager.CreateDevice(createdParameters);
        Direct3D9DeviceCreationParameters requestedParameters = createdParameters with
        {
            BehaviorFlags = createdParameters.BehaviorFlags | D3D9.CreateDisableDriverManagementEX
        };

        manager.GetOrCreateDevice(ref requestedParameters);

        Assert.AreEqual(expected.BehaviorFlags, requestedParameters.BehaviorFlags);
    }

    [TestMethod]
    public void WhenAdapterDoesNotMatchThenNewDeviceIsCreated()
    {
        List<Direct3D9DeviceCreationParameters> creations = [];
        using Direct3D9DeviceManager manager = CreateManager(creations);
        Direct3D9DeviceCreationParameters parameters = CreateParameters();
        manager.CreateDevice(parameters);
        parameters = parameters with { AdapterOrdinal = parameters.AdapterOrdinal + 1 };

        manager.GetOrCreateDevice(ref parameters);

        Assert.AreEqual(2, creations.Count);
    }

    [TestMethod]
    public void WhenSoftwareDeviceTypeMatchesThenDeviceIsSharedAcrossAdaptersAndFlags()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9DeviceCreationParameters createdParameters = CreateParameters() with { DeviceType = Devtype.SW };
        Direct3D9Device expected = manager.CreateDevice(createdParameters);
        Direct3D9DeviceCreationParameters requestedParameters = createdParameters with
        {
            AdapterOrdinal = createdParameters.AdapterOrdinal + 1,
            BehaviorFlags = createdParameters.BehaviorFlags + 1
        };

        Direct3D9Device actual = manager.GetOrCreateDevice(ref requestedParameters);

        Assert.AreEqual(
            (true, expected.BehaviorFlags),
            (ReferenceEquals(expected, actual), requestedParameters.BehaviorFlags));
    }

    [TestMethod]
    public void WhenMatchingUsableWindowedHardwareDeviceExistsThenQueryReturnsTrue()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            2,
            Devtype.Hal);
        manager.GetDeviceAndPresentParameters(request, default);

        bool exists = manager.DoesWindowedHardwareDeviceExist(2);

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void WhenOnlyDifferentAdapterHardwareDeviceExistsThenQueryReturnsFalse()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            2,
            Devtype.Hal);
        manager.GetDeviceAndPresentParameters(request, default);

        bool exists = manager.DoesWindowedHardwareDeviceExist(3);

        Assert.IsFalse(exists);
    }

    [TestMethod]
    public void WhenMatchingHardwareDeviceIsUnusableThenQueryReturnsFalse()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            2,
            Devtype.Hal);
        Direct3D9Device device = manager.GetDeviceAndPresentParameters(request, default).Device;
        device.MarkUnusable();

        bool exists = manager.DoesWindowedHardwareDeviceExist(2);

        Assert.IsFalse(exists);
    }

    [TestMethod]
    public void WhenManagerIsDisposedThenWindowedHardwareDeviceQueryIsRejected()
    {
        Direct3D9DeviceManager manager = CreateManager();
        manager.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => manager.DoesWindowedHardwareDeviceExist(2));
    }

    [TestMethod]
    public void WhenIndependentSoftwareDeviceIsRequestedThenNativeCreationParametersAreUsed()
    {
        List<Direct3D9DeviceCreationParameters> creations = [];
        using Direct3D9DeviceManager manager = CreateManager(creations);

        Direct3D9Device device = manager.GetSoftwareDevice();

        Assert.AreEqual(1, creations.Count);
        Direct3D9DeviceCreationParameters parameters = creations[0];
        Assert.AreEqual(unchecked((uint) D3D9.AdapterDefault), parameters.AdapterOrdinal);
        Assert.AreEqual(Devtype.SW, parameters.DeviceType);
        Assert.AreEqual(
            (uint) (D3D9.CreateSoftwareVertexprocessing
            | D3D9.CreateMultithreaded
            | D3D9.CreateFpuPreserve
            | D3D9.CreateDisableDriverManagementEX),
            parameters.BehaviorFlags);
        Assert.AreEqual(1u, parameters.PresentParameters.BackBufferWidth);
        Assert.AreEqual(1u, parameters.PresentParameters.BackBufferHeight);
        Assert.AreEqual(Format.X8R8G8B8, parameters.PresentParameters.BackBufferFormat);
        Assert.AreEqual(1u, parameters.PresentParameters.BackBufferCount);
        Assert.AreEqual(Swapeffect.Discard, parameters.PresentParameters.SwapEffect);
        Assert.AreEqual(0, parameters.PresentParameters.HDeviceWindow);
        Assert.IsTrue(parameters.PresentParameters.Windowed);
        Assert.IsFalse(parameters.PresentParameters.EnableAutoDepthStencil);
        Assert.AreEqual(Format.Unknown, parameters.PresentParameters.AutoDepthStencilFormat);
        Assert.AreNotEqual(0, parameters.FocusWindow);
        Assert.IsFalse(parameters.UseExtendedDeviceCreate);
        Assert.IsFalse(parameters.RetryWithoutDriverManagementEx);
        Assert.AreSame(device, manager.GetSoftwareDevice());
    }

    [TestMethod]
    public void WhenIndependentSoftwareDeviceIsRequestedTwiceThenExistingDeviceIsReused()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            });
        Direct3D9Device expected = manager.GetSoftwareDevice();

        Direct3D9Device actual = manager.GetSoftwareDevice();

        Assert.AreEqual((expected, 1), (actual, creationCount));
    }

    [TestMethod]
    public void WhenIndependentSoftwareDeviceIsRequestedThenDisplayModeIsNotProbed()
    {
        int probeCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            adapterDisplayModeProbe: _ =>
            {
                probeCount++;
                return 0;
            });

        manager.GetSoftwareDevice();

        Assert.AreEqual(0, probeCount);
    }

    [TestMethod]
    public void WhenIndependentSoftwareDeviceIsRequestedThenItIsNotTrackedAsUsable()
    {
        int validNotificationCount = 0;
        using Direct3D9DeviceManager manager = CreateManager();
        manager.AddAdapterStatusListener((_, isValid) => validNotificationCount += isValid ? 1 : 0);

        Direct3D9Device device = manager.GetSoftwareDevice();

        Assert.AreEqual((0, 0, false), (manager.UsableDeviceCount, validNotificationCount, manager.DoesWindowedHardwareDeviceExist(D3D9.AdapterDefault)));
        Assert.AreEqual(Devtype.SW, device.DeviceType);
    }

    [TestMethod]
    public void WhenIndependentSoftwareDeviceCreationIsLostThenDisplayStateInvalidIsReported()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DeviceLostHResult);
                throw new InvalidOperationException();
            });

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetSoftwareDevice());

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, 1, 0), (exception.HResult, creationCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenIndependentSoftwareDeviceCreationFailsThenDeviceIsDisposedWithoutCaching()
    {
        int creationCount = 0;
        int disposedCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return new Direct3D9Device(
                    null,
                    null,
                    parameters.AdapterOrdinal,
                    parameters.DeviceType,
                    parameters.BehaviorFlags,
                    parameters.PresentParameters,
                    unusable,
                    device =>
                    {
                        disposedCount++;
                        disposed(device);
                    });
            },
            testLevel1Device: _ => creationCount == 1 ? Direct3D9Factory.InvalidCallHResult : 0);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetSoftwareDevice());
        Direct3D9Device device = manager.GetSoftwareDevice();

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 2, 1, 0, Devtype.SW),
            (exception.HResult, creationCount, disposedCount, manager.UsableDeviceCount, device.DeviceType));
    }

    [TestMethod]
    public void WhenIndependentSoftwareDeviceIsRequestedThenRasterizerIsRegisteredBeforeCreation()
    {
        List<string> events = [];
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                events.Add("create");
                return CreateDevice(parameters, unusable, disposed);
            },
            ensureSoftwareRasterizerRegistered: () => events.Add("register"));

        manager.GetSoftwareDevice();

        CollectionAssert.AreEqual(new[] { "register", "create" }, events);
    }

    [TestMethod]
    public void WhenIndependentSoftwareDeviceIsDisposedThenNextRequestCreatesANewDevice()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            });
        Direct3D9Device first = manager.GetSoftwareDevice();
        first.Dispose();

        Direct3D9Device second = manager.GetSoftwareDevice();

        Assert.AreNotSame(first, second);
        Assert.AreEqual(2, creationCount);
    }

    [TestMethod]
    public void WhenManagerIsDisposedThenIndependentSoftwareDeviceRequestIsRejected()
    {
        Direct3D9DeviceManager manager = CreateManager();
        manager.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => manager.GetSoftwareDevice());
    }

    [TestMethod]
    public void WhenDeviceIsReusedThenValidAdapterStatusIsNotRepeated()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        int validNotificationCount = 0;
        manager.AddAdapterStatusListener((_, isValid) => validNotificationCount += isValid ? 1 : 0);
        Direct3D9DeviceCreationParameters parameters = CreateParameters();
        manager.CreateDevice(parameters);

        manager.GetOrCreateDevice(ref parameters);

        Assert.AreEqual(1, validNotificationCount);
    }

    [TestMethod]
    public void WhenDeviceCreationAndLossArePairedThenEachStatusIsSentOnce()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        List<bool> statuses = [];
        manager.AddAdapterStatusListener((_, isValid) => statuses.Add(isValid));
        Direct3D9Device device = manager.CreateDevice(CreateParameters());

        device.MarkUnusable();
        device.MarkUnusable();

        CollectionAssert.AreEqual(new[] { true, false }, statuses);
    }

    [TestMethod]
    public void WhenHardwareDeviceRequestHasNoDisplayThenRequestIsRejected()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            null,
            Devtype.Hal);

        Assert.ThrowsExactly<ArgumentException>(() => manager.GetDeviceAndPresentParameters(request, default));
    }

    [TestMethod]
    public void WhenSoftwareDeviceRequestHasNoDisplayThenDefaultAdapterIsUsed()
    {
        List<Direct3D9DeviceCreationParameters> creations = [];
        using Direct3D9DeviceManager manager = CreateManager(creations);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            null,
            Devtype.SW);

        manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual(unchecked((uint) D3D9.AdapterDefault), creations[0].AdapterOrdinal);
    }

    [TestMethod]
    public void WhenDestinationAlphaIsRequiredThenArgbBackBufferIsComposed()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha,
            2,
            Devtype.Hal);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual(Format.A8R8G8B8, result.PresentParameters.BackBufferFormat);
    }

    [TestMethod]
    public void WhenRetainingContentsThenCopySwapEffectIsComposed()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.PresentRetainContents,
            2,
            Devtype.Hal);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual(Swapeffect.Copy, result.PresentParameters.SwapEffect);
    }

    [TestMethod]
    public void WhenPresentingImmediatelyThenImmediateIntervalIsComposed()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.PresentImmediately,
            2,
            Devtype.Hal);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual(D3D9.PresentIntervalImmediate, result.PresentParameters.PresentationInterval);
    }

    [TestMethod]
    public void WhenUsingNonHalPresentationThenBackBufferIsLockable()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9DeviceRequest request = new(
            0,
            (Direct3D9RenderTargetInitializationFlags) 0x40000000,
            2,
            Devtype.Hal);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreNotEqual(0u, result.PresentParameters.Flags & unchecked((uint) D3D9.PresentflagLockableBackbuffer));
    }

    [TestMethod]
    public void WhenFocusWindowIsNullThenWindowValidationIsSkipped()
    {
        int isWindowCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            isWindow: _ =>
            {
                isWindowCount++;
                return false;
            });
        Direct3D9DeviceRequest request = new(0, Direct3D9RenderTargetInitializationFlags.None, 2, Devtype.Hal);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual((0, 0), (isWindowCount, result.Device.FocusWindow));
    }

    [TestMethod]
    public void WhenFocusWindowIsInvalidThenInvalidWindowHandleIsReportedBeforeCreation()
    {
        int creationCount = 0;
        int isWindowCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            },
            isWindow: hwnd =>
            {
                isWindowCount++;
                return hwnd != 123;
            });
        Direct3D9DeviceRequest request = new(123, Direct3D9RenderTargetInitializationFlags.None, 2, Devtype.Hal);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual((Direct3D9Factory.InvalidWindowHandleHResult, 1, 0), (exception.HResult, isWindowCount, creationCount));
    }

    [TestMethod]
    public void WhenFocusWindowIsValidThenItIsForwardedToCreatedDevice()
    {
        nint observedWindow = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            isWindow: hwnd =>
            {
                observedWindow = hwnd;
                return hwnd == 123;
            });
        Direct3D9DeviceRequest request = new(123, Direct3D9RenderTargetInitializationFlags.None, 2, Devtype.Hal);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual((123, 123, 123), (observedWindow, result.Device.FocusWindow, result.PresentParameters.HDeviceWindow));
    }

    [TestMethod]
    public void WhenGetDeviceCapsSucceedsThenBehaviorFlagsUseQueriedCapabilities()
    {
        List<Direct3D9DeviceCreationParameters> creations = [];
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creations.Add(parameters);
                return CreateDevice(parameters, unusable, disposed);
            },
            getDeviceCaps: (uint adapterOrdinal, Devtype deviceType, out Caps9 capabilities) =>
            {
                capabilities = default;
                capabilities.DevCaps = D3D9.DevcapsHwtransformandlight | D3D9.DevcapsPuredevice;
                return adapterOrdinal == 2 && deviceType == Devtype.Hal ? 0 : Direct3D9Factory.InvalidCallHResult;
            });
        Caps9 unusedCapabilities = default;
        unusedCapabilities.DevCaps = 0;
        Direct3D9DeviceRequest request = new(0, Direct3D9RenderTargetInitializationFlags.None, 2, Devtype.Hal);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, unusedCapabilities);

        uint expectedFlags = D3D9.CreateDisableDriverManagementEX
            | D3D9.CreateFpuPreserve
            | D3D9.CreateMultithreaded
            | D3D9.CreateHardwareVertexprocessing
            | D3D9.CreatePuredevice;
        Assert.AreEqual(expectedFlags, creations[0].BehaviorFlags);
        Assert.AreEqual(expectedFlags, result.Device.BehaviorFlags);
        Assert.AreEqual(
            (uint) (D3D9.DevcapsHwtransformandlight | D3D9.DevcapsPuredevice),
            creations[0].Capabilities.DevCaps);
    }

    [TestMethod]
    public void WhenGetDeviceCapsFailsThenDeviceIsNotCreated()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            },
            getDeviceCaps: (uint _, Devtype _, out Caps9 capabilities) =>
            {
                capabilities = default;
                return Direct3D9Factory.InvalidCallHResult;
            });
        Direct3D9DeviceRequest request = new(0, Direct3D9RenderTargetInitializationFlags.None, 2, Devtype.Hal);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 0, 0), (exception.HResult, creationCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenFocusWindowIsInvalidThenGetDeviceCapsIsNotQueried()
    {
        int getDeviceCapsCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            isWindow: _ => false,
            getDeviceCaps: (uint _, Devtype _, out Caps9 capabilities) =>
            {
                getDeviceCapsCount++;
                capabilities = default;
                return 0;
            });
        Direct3D9DeviceRequest request = new(123, Direct3D9RenderTargetInitializationFlags.None, 2, Devtype.Hal);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual((Direct3D9Factory.InvalidWindowHandleHResult, 0), (exception.HResult, getDeviceCapsCount));
    }

    [TestMethod]
    public void WhenWindowedHardwareDeviceQueryFailsGetDeviceCapsThenQueryReturnsFalse()
    {
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            getDeviceCaps: (uint _, Devtype _, out Caps9 capabilities) =>
            {
                capabilities = default;
                return Direct3D9Factory.InvalidCallHResult;
            });
        manager.CreateDevice(CreateParameters());

        bool exists = manager.DoesWindowedHardwareDeviceExist(2);

        Assert.IsFalse(exists);
    }

    [TestMethod]
    public void WhenNewDeviceIsCreatedThenBackBufferFormatIsTestedBeforeTracking()
    {
        List<Format> testedFormats = [];
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            checkRenderTargetFormat: (_, format) =>
            {
                testedFormats.Add(format);
                return 0;
            });
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha,
            2,
            Devtype.Hal);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual((Format.A8R8G8B8, Format.A8R8G8B8, 1), (result.PresentParameters.BackBufferFormat, testedFormats[0], manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenNewDeviceFactorySucceedsThenAdapterValidIsNotifiedBeforeInitialization()
    {
        List<string> events = [];
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                events.Add("create");
                return CreateDevice(parameters, unusable, disposed);
            },
            testLevel1Device: _ =>
            {
                events.Add("init");
                return 0;
            },
            checkRenderTargetFormat: (_, _) =>
            {
                events.Add("format");
                return 0;
            });
        manager.AddAdapterStatusListener((_, isValid) => events.Add(isValid ? "valid" : "invalid"));

        manager.CreateDevice(CreateParameters());

        CollectionAssert.AreEqual(new[] { "create", "valid", "init", "format" }, events);
        Assert.AreEqual(1, manager.UsableDeviceCount);
    }

    [TestMethod]
    public void WhenBackBufferFormatTestFailsThenDeviceIsDisposedWithoutTracking()
    {
        int disposedCount = 0;
        int validNotificationCount = 0;
        int invalidNotificationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) => new Direct3D9Device(
                null,
                null,
                parameters.AdapterOrdinal,
                parameters.DeviceType,
                parameters.BehaviorFlags,
                parameters.PresentParameters,
                unusable,
                device =>
                {
                    disposedCount++;
                    disposed(device);
                }),
            checkRenderTargetFormat: (_, _) => Direct3D9Factory.InvalidCallHResult);
        manager.AddAdapterStatusListener((_, isValid) =>
        {
            if (isValid)
            {
                validNotificationCount++;
            }
            else
            {
                invalidNotificationCount++;
            }
        });

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.CreateDevice(CreateParameters()));

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, 0, 0),
            (exception.HResult, disposedCount, validNotificationCount, invalidNotificationCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenExistingDeviceIsReusedThenBackBufferFormatIsNotTestedAgain()
    {
        int checkCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            checkRenderTargetFormat: (_, _) =>
            {
                checkCount++;
                return 0;
            });
        Direct3D9DeviceRequest request = new(0, Direct3D9RenderTargetInitializationFlags.None, 2, Devtype.Hal);
        manager.GetDeviceAndPresentParameters(request, default);

        manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual(1, checkCount);
    }

    [TestMethod]
    public void WhenIndependentSoftwareDeviceIsRequestedThenBackBufferFormatIsNotTested()
    {
        int checkCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            checkRenderTargetFormat: (_, _) =>
            {
                checkCount++;
                return 0;
            });

        manager.GetSoftwareDevice();

        Assert.AreEqual((0, 0), (checkCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenNewDeviceIsCreatedThenAdapterDisplayModeIsProbedBeforeFactory()
    {
        List<string> events = [];
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                events.Add("create");
                return CreateDevice(parameters, unusable, disposed);
            },
            adapterDisplayModeProbe: adapterOrdinal =>
            {
                events.Add($"probe:{adapterOrdinal}");
                return 0;
            });
        Direct3D9DeviceRequest request = new(0, Direct3D9RenderTargetInitializationFlags.None, 2, Devtype.Hal);

        manager.GetDeviceAndPresentParameters(request, default);

        CollectionAssert.AreEqual(new[] { "probe:2", "create" }, events);
        Assert.AreEqual(1, manager.UsableDeviceCount);
    }

    [TestMethod]
    public void WhenAdapterDisplayModeProbeFailsThenDeviceIsNotCreated()
    {
        int creationCount = 0;
        int validNotificationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            },
            adapterDisplayModeProbe: _ => Direct3D9Factory.InvalidCallHResult);
        manager.AddAdapterStatusListener((_, isValid) => validNotificationCount += isValid ? 1 : 0);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.CreateDevice(CreateParameters()));

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, 0, 0, 0),
            (exception.HResult, creationCount, validNotificationCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenAdapterDisplayModeProbeFailsWithOutOfMemoryThenOriginalErrorIsPreserved()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            },
            adapterDisplayModeProbe: _ => Direct3D9Factory.OutOfMemoryHResult);

        OutOfMemoryException exception = Assert.ThrowsExactly<OutOfMemoryException>(
            () => manager.CreateDevice(CreateParameters()));

        Assert.AreEqual((Direct3D9Factory.OutOfMemoryHResult, 0, 0), (exception.HResult, creationCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenExistingDeviceIsReusedThenAdapterDisplayModeIsNotProbedAgain()
    {
        int probeCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            adapterDisplayModeProbe: _ =>
            {
                probeCount++;
                return 0;
            });
        Direct3D9DeviceRequest request = new(0, Direct3D9RenderTargetInitializationFlags.None, 2, Devtype.Hal);
        manager.GetDeviceAndPresentParameters(request, default);

        manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual(1, probeCount);
    }

    [TestMethod]
    public void WhenDeviceCreationIsLostThenDisplayStateInvalidIsReportedWithoutTracking()
    {
        int creationCount = 0;
        int validNotificationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.DeviceLostHResult);
                throw new InvalidOperationException();
            });
        manager.AddAdapterStatusListener((_, isValid) => validNotificationCount += isValid ? 1 : 0);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.CreateDevice(CreateParameters()));

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, 1, 0, 0),
            (exception.HResult, creationCount, validNotificationCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenDisplayStateIsInvalidBeforeCreationThenFactoryIsNotCalled()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            },
            hasDisplayStateChanged: () => true);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            1,
            Devtype.Hal);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, 0, 0),
            (exception.HResult, creationCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenSingleThreadedSoftwareDeviceIsRequestedThenMultithreadedFlagIsExcluded()
    {
        List<Direct3D9DeviceCreationParameters> creations = [];
        using Direct3D9DeviceManager manager = CreateManager(creations);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.SingleThreadedUsage,
            null,
            Devtype.SW);

        manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual(0u, creations[0].BehaviorFlags & D3D9.CreateMultithreaded);
    }

    [TestMethod]
    public void WhenTrackingFailsAfterDeviceWrappingThenDeviceIsDisposedAfterValidNotification()
    {
        int disposedCount = 0;
        int validNotificationCount = 0;
        Direct3D9DeviceManager? manager = null;
        manager = new Direct3D9DeviceManager(
            (parameters, unusable, disposed) => new Direct3D9Device(
                null,
                null,
                parameters.AdapterOrdinal,
                parameters.DeviceType,
                parameters.BehaviorFlags,
                parameters.PresentParameters,
                unusable,
                device =>
                {
                    disposedCount++;
                    disposed(device);
                }),
            checkRenderTargetFormat: (_, _) =>
            {
                manager.Dispose();
                return 0;
            });
        manager.AddAdapterStatusListener((_, isValid) => validNotificationCount += isValid ? 1 : 0);

        Assert.ThrowsExactly<ObjectDisposedException>(() => manager.CreateDevice(CreateParameters()));
        Assert.AreEqual(1, disposedCount);
        Assert.AreEqual(1, validNotificationCount);
    }

    [TestMethod]
    public void WhenHalAdapterIsOutsideAvailableRangeThenNoHardwareDeviceIsReported()
    {
        List<Direct3D9DeviceCreationParameters> creations = [];
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creations.Add(parameters);
                return CreateDevice(parameters, unusable, disposed);
            },
            adapterCountProvider: () => 2);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            2,
            Devtype.Hal);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual(Direct3D9Factory.NoHardwareDeviceHResult, exception.HResult);
    }

    [TestMethod]
    public void WhenSoftwareAdapterIsOutsideHalRangeThenDeviceCanStillBeCreated()
    {
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            adapterCountProvider: () => 0);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            7,
            Devtype.SW);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual(7u, result.Device.AdapterOrdinal);
    }

    [TestMethod]
    public void WhenHalAdapterIsDisabledThenNoHardwareDeviceIsReportedBeforeCreation()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            },
            adapterCountProvider: () => 2,
            isAdapterEnabled: _ => false);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            1,
            Devtype.Hal);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual((Direct3D9Factory.NoHardwareDeviceHResult, 0), (exception.HResult, creationCount));
    }

    [TestMethod]
    public void WhenSoftwareAdapterIsDisabledForHardwareThenSoftwareDeviceCanStillBeCreated()
    {
        int enableCheckCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            isAdapterEnabled: _ =>
            {
                enableCheckCount++;
                return false;
            });
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            null,
            Devtype.SW);

        manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual(0, enableCheckCount);
    }

    [TestMethod]
    public void WhenSoftwareDeviceIsRequestedThenRasterizerIsRegisteredBeforeCreation()
    {
        List<string> events = [];
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                events.Add("create");
                return CreateDevice(parameters, unusable, disposed);
            },
            ensureSoftwareRasterizerRegistered: () => events.Add("register"));
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            null,
            Devtype.SW);

        manager.GetDeviceAndPresentParameters(request, default);

        CollectionAssert.AreEqual(new[] { "register", "create" }, events);
    }

    [TestMethod]
    public void WhenHardwareDeviceIsRequestedThenSoftwareRasterizerIsNotRegistered()
    {
        int registrationCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            ensureSoftwareRasterizerRegistered: () => registrationCount++);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            1,
            Devtype.Hal);

        manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual(0, registrationCount);
    }

    [TestMethod]
    public void WhenSoftwareRasterizerRegistrationFailsThenDeviceIsNotCreated()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            },
            ensureSoftwareRasterizerRegistered: () =>
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.InvalidCallHResult));
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            null,
            Devtype.SW);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 0), (exception.HResult, creationCount));
    }

    [TestMethod]
    public void WhenDisplayStateChangesDuringDeviceAcquisitionThenDisplayStateInvalidIsReported()
    {
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            hasDisplayStateChanged: () => true);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            1,
            Devtype.Hal);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, 0),
            (exception.HResult, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenDeviceAcquisitionFailsAndDisplayStateChangesThenDisplayStateInvalidOverridesFailure()
    {
        int displayStateCheckCount = 0;
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (_, _, _) =>
            {
                creationCount++;
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Direct3D9Factory.InvalidCallHResult);
                throw new InvalidOperationException();
            },
            hasDisplayStateChanged: () => ++displayStateCheckCount > 1);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            1,
            Devtype.Hal);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, 1, 2, 0),
            (exception.HResult, creationCount, displayStateCheckCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenDisplayStateChangesAfterReusingDeviceThenExistingDeviceRemainsTracked()
    {
        bool hasDisplayStateChanged = false;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            hasDisplayStateChanged: () => hasDisplayStateChanged);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            1,
            Devtype.Hal);
        Direct3D9Device expected = manager.GetDeviceAndPresentParameters(request, default).Device;
        hasDisplayStateChanged = true;

        Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));
        Direct3D9DeviceCreationParameters creationParameters = CreateParametersForRequest(request);

        Assert.AreEqual((expected, 1), (manager.GetOrCreateDevice(ref creationParameters), manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenCurrentDisplaySetChangesThenUsableDevicesAreInvalidatedInReverseOrder()
    {
        Direct3D9DisplaySet oldDisplaySet = new();
        Direct3D9DisplaySet newDisplaySet = new();
        List<uint> invalidAdapters = [];
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            latestDisplaySetProvider: () => oldDisplaySet);
        manager.AddAdapterStatusListener((adapterOrdinal, isValid) =>
        {
            if (!isValid)
            {
                invalidAdapters.Add(adapterOrdinal);
            }
        });
        manager.GetDeviceAndPresentParameters(
            new Direct3D9DeviceRequest(0, Direct3D9RenderTargetInitializationFlags.None, 1, Devtype.Hal, oldDisplaySet),
            default);
        manager.GetDeviceAndPresentParameters(
            new Direct3D9DeviceRequest(0, Direct3D9RenderTargetInitializationFlags.None, 2, Devtype.Hal, oldDisplaySet),
            default);

        manager.NotifyDisplayChange(oldDisplaySet, newDisplaySet);

        CollectionAssert.AreEqual(new uint[] { 2, 1 }, invalidAdapters);
    }

    [TestMethod]
    public void WhenCurrentDisplaySetChangesThenHardwareDevicesAreEnteredDuringInvalidation()
    {
        Direct3D9DisplaySet oldDisplaySet = new();
        Direct3D9DisplaySet newDisplaySet = new();
        Dictionary<uint, Direct3D9Device> devices = [];
        List<(uint AdapterOrdinal, bool IsEntered)> invalidations = [];
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            latestDisplaySetProvider: () => oldDisplaySet);
        manager.AddAdapterStatusListener((adapterOrdinal, isValid) =>
        {
            if (!isValid)
            {
                invalidations.Add((adapterOrdinal, devices[adapterOrdinal].IsEntered()));
            }
        });
        devices.Add(1, manager.GetDeviceAndPresentParameters(
            new Direct3D9DeviceRequest(0, Direct3D9RenderTargetInitializationFlags.None, 1, Devtype.Hal, oldDisplaySet),
            default).Device);
        devices.Add(2, manager.GetDeviceAndPresentParameters(
            new Direct3D9DeviceRequest(0, Direct3D9RenderTargetInitializationFlags.None, 2, Devtype.Hal, oldDisplaySet),
            default).Device);

        manager.NotifyDisplayChange(oldDisplaySet, newDisplaySet);

        CollectionAssert.AreEqual(
            new[] { (2u, true), (1u, true) },
            invalidations);
        Assert.IsTrue(devices.Values.All(static device => !device.IsEntered()));
    }

    [TestMethod]
    public void WhenCurrentDisplaySetChangesThenIndependentSoftwareDeviceIsInvalidatedAndRemainsCached()
    {
        Direct3D9DisplaySet oldDisplaySet = new();
        Direct3D9DisplaySet newDisplaySet = new();
        Direct3D9DisplaySet latestDisplaySet = oldDisplaySet;
        int invalidNotificationCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            latestDisplaySetProvider: () => latestDisplaySet);
        manager.AddAdapterStatusListener((_, isValid) => invalidNotificationCount += isValid ? 0 : 1);
        Direct3D9Device expected = manager.GetSoftwareDevice();
        latestDisplaySet = newDisplaySet;

        manager.NotifyDisplayChange(oldDisplaySet, newDisplaySet);
        Direct3D9Device actual = manager.GetSoftwareDevice();

        Assert.AreEqual(
            (expected, Direct3D9Factory.DisplayStateInvalidHResult, 0),
            (actual, actual.UnusableReasonHResult, invalidNotificationCount));
    }

    [TestMethod]
    public void WhenInvalidatedIndependentSoftwareDeviceIsDisposedThenNextDisplaySetCreatesReplacement()
    {
        Direct3D9DisplaySet oldDisplaySet = new();
        Direct3D9DisplaySet newDisplaySet = new();
        Direct3D9DisplaySet latestDisplaySet = oldDisplaySet;
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            },
            latestDisplaySetProvider: () => latestDisplaySet);
        Direct3D9Device oldDevice = manager.GetSoftwareDevice();
        latestDisplaySet = newDisplaySet;
        manager.NotifyDisplayChange(oldDisplaySet, newDisplaySet);
        oldDevice.Dispose();

        Direct3D9Device replacement = manager.GetSoftwareDevice();

        Assert.AreEqual((false, 2), (ReferenceEquals(oldDevice, replacement), creationCount));
    }

    [TestMethod]
    public void WhenUnrelatedDisplaySetChangesThenCurrentDevicesRemainUsable()
    {
        Direct3D9DisplaySet currentDisplaySet = new();
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            latestDisplaySetProvider: () => currentDisplaySet);
        Direct3D9Device expected = manager.GetDeviceAndPresentParameters(
            new Direct3D9DeviceRequest(0, Direct3D9RenderTargetInitializationFlags.None, 1, Devtype.Hal, currentDisplaySet),
            default).Device;

        manager.NotifyDisplayChange(new Direct3D9DisplaySet(), new Direct3D9DisplaySet());
        Direct3D9Device actual = manager.GetDeviceAndPresentParameters(
            new Direct3D9DeviceRequest(0, Direct3D9RenderTargetInitializationFlags.None, 1, Devtype.Hal, currentDisplaySet),
            default).Device;

        Assert.AreEqual((expected, 1, 0), (actual, manager.UsableDeviceCount, manager.UnusableDeviceCount));
    }

    [TestMethod]
    public void WhenDeviceIsRequestedAfterDisplayChangeThenNewDisplaySetIsAdopted()
    {
        Direct3D9DisplaySet oldDisplaySet = new();
        Direct3D9DisplaySet newDisplaySet = new();
        Direct3D9DisplaySet latestDisplaySet = oldDisplaySet;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            latestDisplaySetProvider: () => latestDisplaySet);
        manager.GetDeviceAndPresentParameters(
            new Direct3D9DeviceRequest(0, Direct3D9RenderTargetInitializationFlags.None, 1, Devtype.Hal, oldDisplaySet),
            default);
        latestDisplaySet = newDisplaySet;
        manager.NotifyDisplayChange(oldDisplaySet, newDisplaySet);

        Direct3D9Device device = manager.GetDeviceAndPresentParameters(
            new Direct3D9DeviceRequest(0, Direct3D9RenderTargetInitializationFlags.None, 1, Devtype.Hal, newDisplaySet),
            default).Device;

        Assert.AreEqual((1, 1), (manager.UsableDeviceCount, manager.UnusableDeviceCount));
    }

    [TestMethod]
    public void WhenRequestUsesObsoleteDisplaySetThenDeviceIsNotCreated()
    {
        Direct3D9DisplaySet latestDisplaySet = new();
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            },
            latestDisplaySetProvider: () => latestDisplaySet);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            1,
            Devtype.Hal,
            new Direct3D9DisplaySet());

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, 0), (exception.HResult, creationCount));
    }

    [TestMethod]
    public void WhenHardwareDeviceIsCreatedThenAdapterLuidComesFromDisplaySnapshot()
    {
        const long expectedAdapterLuid = 0x123456789;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            latestDisplaySetProvider: () => CreateDisplaySet(
                isRecentDriver: true,
                isBadDriver: false,
                adapterLuid: expectedAdapterLuid));

        Direct3D9Device device = manager.GetDeviceAndPresentParameters(
            new Direct3D9DeviceRequest(0, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal),
            default).Device;

        Assert.AreEqual(expectedAdapterLuid, device.AdapterLuid);
    }

    [TestMethod]
    public void WhenSoftwareDeviceIsCreatedThenAdapterLuidComesFromDefaultDisplaySnapshot()
    {
        const long expectedAdapterLuid = 0x23456789A;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            latestDisplaySetProvider: () => CreateDisplaySet(
                isRecentDriver: true,
                isBadDriver: false,
                adapterLuid: expectedAdapterLuid));

        Direct3D9Device device = manager.GetSoftwareDevice();

        Assert.AreEqual(expectedAdapterLuid, device.AdapterLuid);
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenAdapterLuidAccessIsRejected()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9Device device = manager.CreateDevice(CreateParameters() with { AdapterLuid = 42 });
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.AdapterLuid);
    }

    [TestMethod]
    public void WhenDisplayModeIsRequestedThenTargetAdapterModeIsReturned()
    {
        Displaymode expected = new(width: 1920, height: 1080, refreshRate: 60, format: Format.A8R8G8B8);
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            displayModeProvider: adapterOrdinal => adapterOrdinal == 3 ? expected : default);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            3,
            Devtype.Hal);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual(expected, result.DisplayMode);
    }

    [TestMethod]
    public void WhenDisplaySetCachesModeThenCachedModeIsReturned()
    {
        Direct3D9DisplayMode cached = new(24, 1920, 1080, 60, Format.A8R8G8B8, Scanlineordering.Progressive);
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            latestDisplaySetProvider: () => CreateDisplaySet(isRecentDriver: true, isBadDriver: false, displayMode: cached));
        Direct3D9DeviceRequest request = new(0, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual(new Displaymode(1920, 1080, 60, Format.A8R8G8B8), result.DisplayMode);
    }

    [TestMethod]
    public void WhenDisplaySetCachesModeThenLiveDisplayModeProviderIsIgnored()
    {
        Direct3D9DisplayMode cached = new(24, 1280, 720, 75, Format.X8R8G8B8, Scanlineordering.Progressive);
        int providerCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            displayModeProvider: _ =>
            {
                providerCount++;
                return new Displaymode(width: 1920, height: 1080, refreshRate: 60, format: Format.A8R8G8B8);
            },
            latestDisplaySetProvider: () => CreateDisplaySet(isRecentDriver: true, isBadDriver: false, displayMode: cached));
        Direct3D9DeviceRequest request = new(0, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual((new Displaymode(1280, 720, 75, Format.X8R8G8B8), 0), (result.DisplayMode, providerCount));
    }

    [TestMethod]
    public void WhenCachedDisplayModeSizeIsZeroThenDeviceIsNotCreated()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            },
            latestDisplaySetProvider: () => CreateDisplaySet(
                isRecentDriver: true,
                isBadDriver: false,
                displayMode: new Direct3D9DisplayMode(0, 1920, 1080, 60, Format.X8R8G8B8, Scanlineordering.Progressive)));
        Direct3D9DeviceRequest request = new(0, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual((Direct3D9Factory.NotInitializedHResult, 0, 0), (exception.HResult, creationCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenCachedDisplayModeAdapterIsOutOfRangeThenDisplayStateIsInvalid()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            },
            latestDisplaySetProvider: () => CreateDisplaySet(isRecentDriver: true, isBadDriver: false));
        Direct3D9DeviceRequest request = new(0, Direct3D9RenderTargetInitializationFlags.None, 1, Devtype.Hal);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, 0, 0), (exception.HResult, creationCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenDisplayFormatIsCheckedThenSelectedTargetFormatIsUsed()
    {
        Format actualDisplayFormat = Format.Unknown;
        Format actualTargetFormat = Format.Unknown;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            displayModeProvider: _ => new Displaymode(format: Format.X8R8G8B8),
            checkDeviceType: (_, _, displayFormat, targetFormat) =>
            {
                actualDisplayFormat = displayFormat;
                actualTargetFormat = targetFormat;
                return 0;
            });
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha,
            1,
            Devtype.Hal);

        manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual((Format.X8R8G8B8, Format.A8R8G8B8), (actualDisplayFormat, actualTargetFormat));
    }

    [TestMethod]
    public void WhenDisplayFormatIsUnsupportedThenFailureIsPropagatedBeforeDeviceCreation()
    {
        int creationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) =>
            {
                creationCount++;
                return CreateDevice(parameters, unusable, disposed);
            },
            displayModeProvider: _ => new Displaymode(format: Format.X8R8G8B8),
            checkDeviceType: (_, _, _, _) => Direct3D9Factory.InvalidCallHResult);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            1,
            Devtype.Hal);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));
        Assert.AreEqual(Direct3D9Factory.InvalidCallHResult, exception.HResult);
        Assert.AreEqual(0, creationCount);
    }

    [TestMethod]
    public void WhenDeviceParametersAreComposedThenSingleAdapterGroupIsReported()
    {
        List<Direct3D9DeviceCreationParameters> creations = [];
        using Direct3D9DeviceManager manager = CreateManager(creations);
        Direct3D9DeviceRequest request = new(
            0,
            Direct3D9RenderTargetInitializationFlags.None,
            2,
            Devtype.Hal);

        Direct3D9DeviceAndPresentParameters result = manager.GetDeviceAndPresentParameters(request, default);

        Assert.AreEqual((0u, 1u, 0u),
            (creations[0].AdapterOrdinalInGroup, creations[0].NumberOfAdaptersInGroup, result.AdapterOrdinalInGroup));
    }

    [TestMethod]
    public void WhenLevel1TestReturnsStableHalFailureThenAdapterIsDisabledAndDeviceIsDisposedBeforeTracking()
    {
        int disabledAdapter = -1;
        int disposedCount = 0;
        int validNotificationCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) => new Direct3D9Device(
                null,
                null,
                parameters.AdapterOrdinal,
                parameters.DeviceType,
                parameters.BehaviorFlags,
                parameters.PresentParameters,
                unusable,
                device =>
                {
                    disposedCount++;
                    disposed(device);
                }),
            disableAdapter: adapterOrdinal => disabledAdapter = (int) adapterOrdinal,
            testLevel1Device: _ => Direct3D9Factory.InvalidCallHResult);
        manager.AddAdapterStatusListener((_, isValid) => validNotificationCount += isValid ? 1 : 0);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.CreateDevice(CreateParameters()));

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 2, 1, 1, 0),
            (exception.HResult, disabledAdapter, disposedCount, validNotificationCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.OutOfVideoMemoryHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenLevel1TestReturnsContextDependentHalFailureThenAdapterIsNotDisabled(int testResult)
    {
        int disableCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            disableAdapter: _ => disableCount++,
            testLevel1Device: _ => testResult);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.CreateDevice(CreateParameters()));

        Assert.AreEqual((testResult, 0), (exception.HResult, disableCount));
    }

    [TestMethod]
    public void WhenLevel1TestReturnsOutOfMemoryThenAdapterIsNotDisabled()
    {
        int disableCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            disableAdapter: _ => disableCount++,
            testLevel1Device: _ => Direct3D9Factory.OutOfMemoryHResult);

        Assert.ThrowsExactly<OutOfMemoryException>(() => manager.CreateDevice(CreateParameters()));

        Assert.AreEqual(0, disableCount);
    }

    [TestMethod]
    public void WhenLevel1TestReturnsStableSoftwareFailureThenAdapterIsNotDisabled()
    {
        int disableCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            disableAdapter: _ => disableCount++,
            testLevel1Device: _ => Direct3D9Factory.InvalidCallHResult);
        Direct3D9DeviceCreationParameters parameters = CreateParameters() with { DeviceType = Devtype.SW };

        Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(() => manager.CreateDevice(parameters));

        Assert.AreEqual(0, disableCount);
    }

    [TestMethod]
    public void WhenLevel1TestSucceedsThenDeviceIsTrackedAndAdapterRemainsEnabled()
    {
        int disableCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            disableAdapter: _ => disableCount++,
            testLevel1Device: _ => 0);

        manager.CreateDevice(CreateParameters());

        Assert.AreEqual((0, 1), (disableCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenPresentReturnsGenericFailureThenAdapterErrorIsCountedBeforeInvalidNotificationAndResourceRelease()
    {
        List<string> events = [];
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            handleUnexpectedAdapterError: _ => events.Add("error"));
        manager.AddAdapterStatusListener((_, isValid) =>
        {
            if (!isValid)
            {
                events.Add("invalid");
            }
        });
        Direct3D9Device device = manager.CreateDevice(CreateParameters());
        using TestResource resource = new(device.ResourceManager, () => events.Add("resource"));
        events.Clear();

        Direct3D9DeviceState state = device.HandlePresentFailure(Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9DeviceStateKind.Failure, "error,invalid,resource"),
            (state.HResult, state.Kind, string.Join(',', events)));
    }

    [TestMethod]
    public void WhenMultithreadedMarkIsNotEntryProtectedThenCleanupIsDeferredUntilProtectedEntry()
    {
        List<string> events = [];
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            handleUnexpectedAdapterError: _ => events.Add("error"));
        manager.AddAdapterStatusListener((_, isValid) =>
        {
            if (!isValid)
            {
                events.Add("invalid");
            }
        });
        Direct3D9Device device = manager.CreateDevice(CreateParameters());
        using TestResource resource = new(device.ResourceManager, () => events.Add("resource"));
        events.Clear();
        Thread worker = new(() => device.MarkUnusable(
            Direct3D9Factory.DriverInternalErrorHResult,
            mayBeMultithreadedCall: true));

        worker.Start();
        worker.Join();

        Assert.AreEqual(
            (true, 1, 0, 1, "error"),
            (device.IsUnusable, manager.UsableDeviceCount, manager.UnusableDeviceCount, device.ResourceCount, string.Join(',', events)));

        using (Direct3D9DeviceEntryGuard deviceEntry = new(device))
        {
            device.MarkUnusable(mayBeMultithreadedCall: true);
        }

        Assert.AreEqual(
            (0, 1, 0, "error,invalid,resource"),
            (manager.UsableDeviceCount, manager.UnusableDeviceCount, device.ResourceCount, string.Join(',', events)));
    }

    [TestMethod]
    public void WhenPresentReturnsDriverInternalErrorTwiceThenAdapterErrorIsCountedOnce()
    {
        int errorCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            handleUnexpectedAdapterError: _ => errorCount++);
        Direct3D9Device device = manager.CreateDevice(CreateParameters());

        device.HandlePresentFailure(Direct3D9Factory.DriverInternalErrorHResult);
        device.HandlePresentFailure(Direct3D9Factory.DriverInternalErrorHResult);

        Assert.AreEqual(1, errorCount);
    }

    [TestMethod]
    public void WhenDriverInternalErrorMarksDeviceUnusableThenDisplayInvalidIsCommitted()
    {
        int errorCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            handleUnexpectedAdapterError: _ => errorCount++);
        Direct3D9Device device = manager.CreateDevice(CreateParameters());

        device.MarkUnusable(Direct3D9Factory.DriverInternalErrorHResult);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, 1),
            (device.UnusableReasonHResult, errorCount));
    }

    [TestMethod]
    public void WhenUnusableDeviceIsMarkedWithAnotherReasonThenCommittedStateIsNotOverwritten()
    {
        int errorCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            handleUnexpectedAdapterError: _ => errorCount++);
        Direct3D9Device device = manager.CreateDevice(CreateParameters());
        device.MarkUnusable(Direct3D9Factory.DeviceLostHResult);

        device.MarkUnusable(Direct3D9Factory.DriverInternalErrorHResult);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, 0),
            (device.UnusableReasonHResult, errorCount));
    }

    [TestMethod]
    public void WhenPresentReturnsDeviceLostThenAdapterUnexpectedErrorIsNotCounted()
    {
        int errorCount = 0;
        using Direct3D9DeviceManager manager = new(
            CreateDevice,
            handleUnexpectedAdapterError: _ => errorCount++);
        Direct3D9Device device = manager.CreateDevice(CreateParameters());

        Direct3D9DeviceState state = device.HandlePresentFailure(Direct3D9Factory.DeviceLostHResult);

        Assert.AreEqual((0, Direct3D9Factory.DisplayStateInvalidHResult), (errorCount, state.HResult));
    }

    [TestMethod]
    public void WhenLddmPresentReturnsInvalidArgumentThenRecreateAndPresentIsRequestedWithoutMarkingDeviceUnusable()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9DeviceCreationParameters parameters = CreateParameters() with
        {
            Capabilities = new Caps9 { Caps2 = (uint) D3D9.Caps2Canshareresource }
        };
        Direct3D9Device device = manager.CreateDevice(parameters);

        Direct3D9DeviceState state = device.HandlePresentFailure(Direct3D9Factory.InvalidArgumentHResult);

        Assert.AreEqual(
            (Direct3D9Factory.NeedRecreateAndPresentHResult, false, 1, 0),
            (state.HResult, device.IsUnusable, manager.UsableDeviceCount, manager.UnusableDeviceCount));
    }

    [TestMethod]
    public void WhenNonLddmPresentReturnsInvalidArgumentThenFailureIsPreservedWithoutMarkingDeviceUnusable()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9Device device = manager.CreateDevice(CreateParameters());

        Direct3D9DeviceState state = device.HandlePresentFailure(Direct3D9Factory.InvalidArgumentHResult);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, false, 1, 0),
            (state.HResult, device.IsUnusable, manager.UsableDeviceCount, manager.UnusableDeviceCount));
    }

    [TestMethod]
    public void WhenHandlingSuccessfulPresentResultThenRequestIsRejected()
    {
        using Direct3D9DeviceManager manager = CreateManager();
        Direct3D9Device device = manager.CreateDevice(CreateParameters());

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => device.HandlePresentFailure(0));
    }

    [TestMethod]
    public void WhenRecreatingDeviceThenCreationParametersArePreserved()
    {
        List<Direct3D9DeviceCreationParameters> creations = [];
        using Direct3D9DeviceManager manager = CreateManager(creations);
        Direct3D9DeviceCreationParameters expected = CreateParameters() with { FocusWindow = 123 };
        Direct3D9Device device = manager.CreateDevice(expected);
        device.MarkUnusable();

        manager.RecreateDevice(device);

        Assert.AreEqual(expected, creations[1]);
    }

    [TestMethod]
    public void WhenRecreatingDeviceThenOldResourcesAreReleasedBeforeNewDeviceCreation()
    {
        List<string> events = [];
        using Direct3D9DeviceManager manager = new((parameters, unusable, disposed) =>
        {
            events.Add("create");
            return CreateDevice(parameters, unusable, disposed);
        });
        Direct3D9Device device = manager.CreateDevice(CreateParameters());
        using TestResource resource = new(device.ResourceManager, () => events.Add("release"));
        events.Clear();

        device.MarkUnusable();
        manager.RecreateDevice(device);

        CollectionAssert.AreEqual(new[] { "release", "create" }, events);
    }

    [TestMethod]
    public void WhenHardwareDriverIsAcceptedThenAdjustedCapabilitiesReachLevel1Test()
    {
        uint observedRasterCaps = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) => new Direct3D9Device(
                null,
                null,
                parameters.AdapterOrdinal,
                parameters.DeviceType,
                parameters.BehaviorFlags,
                parameters.PresentParameters,
                unusable,
                disposed,
                parameters.Capabilities),
            testLevel1Device: device =>
            {
                observedRasterCaps = device.Capabilities.RasterCaps;
                return 0;
            },
            latestDisplaySetProvider: () => CreateDisplaySet(isRecentDriver: true, isBadDriver: false));
        Caps9 capabilities = default;
        capabilities.RasterCaps = unchecked((uint) (D3D9.PrastercapsScissortest | 0x40));
        Direct3D9DeviceRequest request = new(0, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);

        manager.GetDeviceAndPresentParameters(request, capabilities);

        Assert.AreEqual(0x40u, observedRasterCaps);
    }

    [TestMethod]
    public void WhenTextureFormatSupportIsGatheredThenResultsReachLevel1Test()
    {
        Direct3D9TextureFormatSupport observed = default;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) => CreateDevice(parameters, unusable, disposed),
            testLevel1Device: device =>
            {
                observed = device.TextureFormatSupport;
                return 0;
            },
            checkDeviceFormat: (_, _, _, _, _, checkedFormat) => checkedFormat == Format.A8R8G8B8
                ? 0
                : Direct3D9Factory.GenericFailureHResult);

        manager.CreateDevice(CreateParameters());

        Assert.AreEqual(MilPixelFormat.Pbgra32Bpp, observed.SupportFor32BppPbgra);
    }

    [TestMethod]
    public void WhenGlyphAlphaTextureFormatIsInitializedThenItRunsAfterTextureSupportDetection()
    {
        Direct3D9GlyphAlphaTextureFormat observed = Direct3D9GlyphAlphaTextureFormat.Undefined;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) => CreateDevice(parameters, unusable, disposed),
            testLevel1Device: device =>
            {
                observed = device.GlyphAlphaTextureFormat;
                return 0;
            },
            checkDeviceFormat: (_, _, _, _, _, checkedFormat) => checkedFormat == Format.L8
                ? 0
                : Direct3D9Factory.GenericFailureHResult);

        Caps9 capabilities = new()
        {
            PixelShaderVersion = 0xFFFF0101,
            MaxTextureBlendStages = 4,
            SrcBlendCaps = (uint) D3D9.PblendcapsBlendfactor
        };
        manager.CreateDevice(CreateParameters() with { Capabilities = capabilities });

        Assert.AreEqual(Direct3D9GlyphAlphaTextureFormat.L8, observed);
    }

    [TestMethod]
    public void WhenHardwareTextIsDisabledThenAlphaTextureInitializationIsSkipped()
    {
        (bool Eligible, Direct3D9GlyphAlphaTextureFormat Format, bool CanDrawText) observed = default;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) => CreateDevice(parameters, unusable, disposed),
            testLevel1Device: device =>
            {
                observed = (device.IsTextPixelShaderInitializationEligible, device.GlyphAlphaTextureFormat, device.CanDrawText);
                return 0;
            },
            checkDeviceFormat: (_, _, _, _, _, checkedFormat) => checkedFormat == Format.A8
                ? 0
                : Direct3D9Factory.GenericFailureHResult,
            isHardwareTextDisabled: () => true);
        Caps9 capabilities = new()
        {
            PixelShaderVersion = 0xFFFF0101,
            MaxTextureBlendStages = 4,
            SrcBlendCaps = (uint) D3D9.PblendcapsBlendfactor
        };

        manager.CreateDevice(CreateParameters() with { Capabilities = capabilities });

        Assert.AreEqual((false, Direct3D9GlyphAlphaTextureFormat.Undefined, false), observed);
    }

    [TestMethod]
    public void WhenTextPixelShadersInitializeThenCapabilityIsVisibleToLevel1Test()
    {
        (int InitializationCount, bool Initialized, bool CanDrawText) observed = default;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) => CreateDevice(parameters, unusable, disposed),
            testLevel1Device: device =>
            {
                observed = (observed.InitializationCount, device.IsTextPixelShaderInitialized, device.CanDrawText);
                return 0;
            },
            checkDeviceFormat: (_, _, _, _, _, checkedFormat) => checkedFormat == Format.A8
                ? 0
                : Direct3D9Factory.GenericFailureHResult,
            initializeTextPixelShaders: device =>
            {
                observed.InitializationCount++;
                return device.InitializeTextPixelShaders((uint _, out Direct3D9PixelShader? shader) =>
                {
                    shader = new Direct3D9PixelShader(null);
                    return 0;
                });
            });
        Caps9 capabilities = new()
        {
            PixelShaderVersion = 0xFFFF0101,
            MaxTextureBlendStages = 4,
            SrcBlendCaps = (uint) D3D9.PblendcapsBlendfactor
        };

        manager.CreateDevice(CreateParameters() with { Capabilities = capabilities });

        Assert.AreEqual((1, true, true), observed);
    }

    [TestMethod]
    public void WhenTextPixelShaderInitializationFailsThenDeviceCreationPropagatesFirstError()
    {
        int testLevel1CallCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) => CreateDevice(parameters, unusable, disposed),
            testLevel1Device: _ =>
            {
                testLevel1CallCount++;
                return 0;
            },
            checkDeviceFormat: (_, _, _, _, _, checkedFormat) => checkedFormat == Format.A8
                ? 0
                : Direct3D9Factory.GenericFailureHResult,
            initializeTextPixelShaders: _ => Direct3D9Factory.InvalidCallHResult);
        Caps9 capabilities = new()
        {
            PixelShaderVersion = 0xFFFF0101,
            MaxTextureBlendStages = 4,
            SrcBlendCaps = (uint) D3D9.PblendcapsBlendfactor
        };

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.CreateDevice(CreateParameters() with { Capabilities = capabilities }));

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 0), (exception.HResult, testLevel1CallCount));
    }

    [TestMethod]
    public void WhenMultisampleSupportIsGatheredThenResultsReachLevel1Test()
    {
        Direct3D9MultisampleSupport observed = default;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) => CreateDevice(parameters, unusable, disposed),
            testLevel1Device: device =>
            {
                observed = device.MultisampleSupport;
                return 0;
            },
            maximumMultisampleTypeProvider: () => 4,
            checkDeviceMultisampleType: (_, _, format, _, type) =>
                format == Format.A8R8G8B8 && type > MultisampleType.Multisample2Samples
                    ? Direct3D9Factory.GenericFailureHResult
                    : 0);

        manager.CreateDevice(CreateParameters());

        Assert.AreEqual(
            new Direct3D9MultisampleSupport(
                MultisampleType.Multisample4Samples,
                MultisampleType.Multisample2Samples,
                MultisampleType.Multisample4Samples),
            observed);
    }

    [TestMethod]
    public void WhenRenderStateIsInitializedThenItRunsAfterSupportDetectionAndBeforeLevel1Test()
    {
        List<string> calls = [];
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) => CreateDevice(parameters, unusable, disposed),
            testLevel1Device: _ =>
            {
                calls.Add("level1");
                return 0;
            },
            checkDeviceFormat: (_, _, _, _, _, _) =>
            {
                if (!calls.Contains("texture"))
                {
                    calls.Add("texture");
                }

                return 0;
            },
            maximumMultisampleTypeProvider: () => 2,
            checkDeviceMultisampleType: (_, _, _, _, _) =>
            {
                if (!calls.Contains("multisample"))
                {
                    calls.Add("multisample");
                }

                return 0;
            },
            initializeRenderState: _ =>
            {
                calls.Add("render-state");
                return 0;
            });

        manager.CreateDevice(CreateParameters());

        CollectionAssert.AreEqual(new[] { "texture", "multisample", "render-state", "level1" }, calls);
    }

    [TestMethod]
    public void WhenRenderStateInitializationFailsThenCreatedDeviceIsDisposedBeforeTracking()
    {
        int disposedCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) => new Direct3D9Device(
                null,
                null,
                parameters.AdapterOrdinal,
                parameters.DeviceType,
                parameters.BehaviorFlags,
                parameters.PresentParameters,
                unusable,
                device =>
                {
                    disposedCount++;
                    disposed(device);
                },
                parameters.Capabilities),
            initializeRenderState: _ => Direct3D9Factory.GenericFailureHResult);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.CreateDevice(CreateParameters()));

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 1, 0),
            (exception.HResult, disposedCount, manager.UsableDeviceCount));
    }

    [TestMethod]
    public void WhenHardwareDriverIsBadThenCreatedDeviceIsDisposedBeforeTracking()
    {
        int disposedCount = 0;
        using Direct3D9DeviceManager manager = new(
            (parameters, unusable, disposed) => new Direct3D9Device(
                null,
                null,
                parameters.AdapterOrdinal,
                parameters.DeviceType,
                parameters.BehaviorFlags,
                parameters.PresentParameters,
                unusable,
                device =>
                {
                    disposedCount++;
                    disposed(device);
                },
                parameters.Capabilities),
            latestDisplaySetProvider: () => CreateDisplaySet(isRecentDriver: true, isBadDriver: true));
        Direct3D9DeviceRequest request = new(0, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => manager.GetDeviceAndPresentParameters(request, default));

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 1, 0),
            (exception.HResult, disposedCount, manager.UsableDeviceCount));
    }

    private static Direct3D9DisplaySet CreateDisplaySet(
        bool isRecentDriver,
        bool isBadDriver,
        Direct3D9DisplayMode? displayMode = null,
        long adapterLuid = 0)
    {
        Direct3D9Display display = new(
            DisplayIndex: 0,
            Direct3DAdapterLuid: adapterLuid,
            MonitorHandle: 0,
            Bounds: ImmutableArray<Direct3D9DisplayBounds>.Empty,
            DeviceName: "DISPLAY",
            StateFlags: 0,
            Settings: default,
            MemorySize: 0,
            isRecentDriver,
            isBadDriver,
            GraphicsCardVendorId: 0,
            GraphicsCardDeviceId: 0,
            DisplayMode: displayMode ?? new Direct3D9DisplayMode(24, 1, 1, 60, Format.X8R8G8B8, Scanlineordering.Progressive),
            DisplayRotation: default,
            GraphicsAccelerationCaps: default);
        Direct3D9DisplaySetCharacteristics characteristics = new(
            RequiredVideoDriverDate: 0,
            Direct3DAdapterCount: 1,
            IsNonLocalDevicePresent: false,
            DisplayBounds: ImmutableArray<Direct3D9DisplayBounds>.Empty,
            Displays: ImmutableArray.Create(display));
        return new Direct3D9DisplaySet(characteristics, 0, 0, () => 0, () => 0);
    }

    private static Direct3D9DeviceManager CreateManager(List<Direct3D9DeviceCreationParameters>? creations = null)
    {
        return new Direct3D9DeviceManager((parameters, unusable, disposed) =>
        {
            creations?.Add(parameters);
            return CreateDevice(parameters, unusable, disposed);
        });
    }

    private static Direct3D9Device CreateDevice(
        Direct3D9DeviceCreationParameters parameters,
        Action<Direct3D9Device> unusableNotification,
        Action<Direct3D9Device> disposedNotification)
    {
        return new Direct3D9Device(
            null,
            null,
            parameters.AdapterOrdinal,
            parameters.DeviceType,
            parameters.BehaviorFlags,
            parameters.PresentParameters,
            unusableNotification,
            disposedNotification,
            capabilities: parameters.Capabilities,
            focusWindow: parameters.FocusWindow,
            adapterLuid: parameters.AdapterLuid);
    }

    private static Direct3D9DeviceCreationParameters CreateParametersForRequest(Direct3D9DeviceRequest request)
    {
        return new Direct3D9DeviceCreationParameters(
            request.DisplayAdapterOrdinal ?? D3D9.AdapterDefault,
            request.DeviceType,
            D3D9.CreateDisableDriverManagementEX | D3D9.CreateFpuPreserve | D3D9.CreateMultithreaded | D3D9.CreateSoftwareVertexprocessing,
            new PresentParameters(
                backBufferWidth: 1,
                backBufferHeight: 1,
                backBufferFormat: Format.X8R8G8B8,
                backBufferCount: 1,
                swapEffect: Swapeffect.Discard,
                hDeviceWindow: request.FocusWindow,
                windowed: true,
                flags: unchecked((uint) D3D9.PresentflagDeviceclip),
                presentationInterval: D3D9.PresentIntervalOne));
    }

    private static Direct3D9DeviceCreationParameters CreateParameters()
    {
        return new Direct3D9DeviceCreationParameters(
            2,
            Devtype.Hal,
            0x1234,
            new PresentParameters(
                backBufferWidth: 3,
                backBufferHeight: 4,
                backBufferFormat: Format.X8R8G8B8,
                backBufferCount: 1,
                swapEffect: Swapeffect.Discard,
                windowed: true));
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
