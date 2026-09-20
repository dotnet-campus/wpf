using Microsoft.Win32;
using System.Security;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WpfGfxShape.Core;

internal static class Direct3D9GuiHandleQuota
{
    private const uint DefaultQuota = 10_000;
    private const string WindowsKeyPath = @"Software\Microsoft\Windows NT\CurrentVersion\Windows";
    private const string GdiProcessHandleQuotaValueName = "GDIProcessHandleQuota";

    private static uint _gdiTestBar;

    internal static int ReinterpretGetDeviceContextFailure(int hResult)
    {
        return ReinterpretGetDeviceContextFailure(hResult, GetCurrentGdiObjectCount, ReadGdiProcessHandleQuota);
    }

    internal static int ReinterpretGetDeviceContextFailure(
        int hResult,
        Func<uint> currentCountProvider,
        Func<uint?> quotaProvider)
    {
        ArgumentNullException.ThrowIfNull(currentCountProvider);
        ArgumentNullException.ThrowIfNull(quotaProvider);

        if (hResult != Direct3D9Factory.GenericFailureHResult)
        {
            return hResult;
        }

        uint currentCount = currentCountProvider();
        uint testBar = _gdiTestBar;
        if (testBar == 0)
        {
            uint quota = quotaProvider() is uint configuredQuota and > 0 ? configuredQuota : DefaultQuota;
            testBar = quota - (quota >> 3);
            _gdiTestBar = testBar;
        }

        return currentCount >= testBar
            ? Direct3D9Factory.OutOfMemoryHResult
            : Direct3D9Factory.DriverInternalErrorHResult;
    }

    internal static uint CalculateTestBar(uint quota)
    {
        uint effectiveQuota = quota > 0 ? quota : DefaultQuota;
        return effectiveQuota - (effectiveQuota >> 3);
    }

    private static unsafe uint GetCurrentGdiObjectCount()
    {
        HANDLE process = PInvoke.GetCurrentProcess();
        return PInvoke.GetGuiResources(process, GET_GUI_RESOURCES_FLAGS.GR_GDIOBJECTS);
    }

    private static uint? ReadGdiProcessHandleQuota()
    {
        try
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(WindowsKeyPath);
            return key?.GetValue(GdiProcessHandleQuotaValueName) is int value && value > 0
                ? unchecked((uint) value)
                : null;
        }
        catch (Exception exception) when (exception is IOException or SecurityException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
