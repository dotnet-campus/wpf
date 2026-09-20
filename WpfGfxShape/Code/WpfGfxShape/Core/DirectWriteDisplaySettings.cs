using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WpfGfxShape.Core;

internal sealed unsafe class DirectWriteRenderingParameters : IDisposable
{
    private void*** _renderingParameters;

    private DirectWriteRenderingParameters(void*** renderingParameters)
    {
        _renderingParameters = renderingParameters;
    }

    internal Direct3D9RenderingParametersSnapshot ReadSnapshot()
    {
        ObjectDisposedException.ThrowIf(_renderingParameters is null, this);

        void** vtable = *_renderingParameters;
        delegate* unmanaged[Stdcall]<void***, float> getGamma =
            (delegate* unmanaged[Stdcall]<void***, float>) vtable[3];
        delegate* unmanaged[Stdcall]<void***, float> getEnhancedContrast =
            (delegate* unmanaged[Stdcall]<void***, float>) vtable[4];
        delegate* unmanaged[Stdcall]<void***, float> getClearTypeLevel =
            (delegate* unmanaged[Stdcall]<void***, float>) vtable[5];
        delegate* unmanaged[Stdcall]<void***, Direct3D9PixelGeometry> getPixelGeometry =
            (delegate* unmanaged[Stdcall]<void***, Direct3D9PixelGeometry>) vtable[6];

        return new Direct3D9RenderingParametersSnapshot(
            getPixelGeometry(_renderingParameters),
            getGamma(_renderingParameters),
            getEnhancedContrast(_renderingParameters),
            getClearTypeLevel(_renderingParameters));
    }

    internal static DirectWriteRenderingParameters CreateDefault(nint factoryNoRef)
    {
        return Create(factoryNoRef, 10, 0);
    }

    internal static DirectWriteRenderingParameters CreateForMonitor(nint factoryNoRef, nint monitorHandle)
    {
        return Create(factoryNoRef, 11, monitorHandle);
    }

    public void Dispose()
    {
        void*** renderingParameters = _renderingParameters;
        _renderingParameters = null;
        if (renderingParameters is null)
        {
            return;
        }

        void** vtable = *renderingParameters;
        delegate* unmanaged[Stdcall]<void***, uint> release =
            (delegate* unmanaged[Stdcall]<void***, uint>) vtable[2];
        release(renderingParameters);
    }

    private static DirectWriteRenderingParameters Create(nint factoryNoRef, int vtableIndex, nint monitorHandle)
    {
        if (factoryNoRef == 0)
        {
            throw new ArgumentException("The borrowed DirectWrite factory pointer cannot be null.", nameof(factoryNoRef));
        }

        void*** factory = (void***) factoryNoRef;
        void*** renderingParameters = null;
        void** vtable = *factory;
        int result;
        if (vtableIndex == 10)
        {
            delegate* unmanaged[Stdcall]<void***, void****, int> createRenderingParameters =
                (delegate* unmanaged[Stdcall]<void***, void****, int>) vtable[vtableIndex];
            result = createRenderingParameters(factory, &renderingParameters);
        }
        else
        {
            delegate* unmanaged[Stdcall]<void***, nint, void****, int> createMonitorRenderingParameters =
                (delegate* unmanaged[Stdcall]<void***, nint, void****, int>) vtable[vtableIndex];
            result = createMonitorRenderingParameters(factory, monitorHandle, &renderingParameters);
        }

        if (result < 0 || renderingParameters is null)
        {
            ReleaseIfPresent(renderingParameters);
            Marshal.ThrowExceptionForHR(result < 0 ? result : unchecked((int) 0x80004003));
        }

        return new DirectWriteRenderingParameters(renderingParameters);
    }

    private static void ReleaseIfPresent(void*** renderingParameters)
    {
        if (renderingParameters is null)
        {
            return;
        }

        void** vtable = *renderingParameters;
        delegate* unmanaged[Stdcall]<void***, uint> release =
            (delegate* unmanaged[Stdcall]<void***, uint>) vtable[2];
        release(renderingParameters);
    }
}

internal static unsafe class DirectWriteDisplaySettingsReader
{
    private const uint FontSmoothingStandard = 1;

    [SupportedOSPlatform("windows5.0")]
    internal static Direct3D9DisplaySettingsSnapshotResult ReadSystem(
        Direct3D9AdapterArrangementResult adapters,
        System.Collections.Immutable.ImmutableArray<Direct3D9DisplayModeSnapshot> displayModes,
        nint factoryNoRef)
    {
        Direct3D9FontSmoothingSettings fontSmoothing = ReadFontSmoothingSettings();
        using DirectWriteRenderingParameters defaultParameters =
            DirectWriteRenderingParameters.CreateDefault(factoryNoRef);

        return Direct3D9DisplaySettingsSnapshotFactory.Read(
            adapters,
            displayModes,
            fontSmoothing,
            defaultParameters.ReadSnapshot(),
            monitorHandle =>
            {
                using DirectWriteRenderingParameters monitorParameters =
                    DirectWriteRenderingParameters.CreateForMonitor(factoryNoRef, monitorHandle);
                return monitorParameters.ReadSnapshot();
            });
    }

    [SupportedOSPlatform("windows5.0")]
    private static Direct3D9FontSmoothingSettings ReadFontSmoothingSettings()
    {
        int smoothing = 0;
        uint smoothingType = FontSmoothingStandard;

        PInvoke.SystemParametersInfo(
            SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETFONTSMOOTHING,
            0,
            &smoothing,
            0);
        PInvoke.SystemParametersInfo(
            SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETFONTSMOOTHINGTYPE,
            0,
            &smoothingType,
            0);

        return new Direct3D9FontSmoothingSettings(smoothing != 0, smoothingType);
    }
}
