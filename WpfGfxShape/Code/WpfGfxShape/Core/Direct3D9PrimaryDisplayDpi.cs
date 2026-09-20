using System.Numerics;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal static unsafe class Direct3D9PrimaryDisplayDpi
{
    private const float DefaultPixelsPerInch = 96f;

    private static readonly Lazy<Matrix3x2> DeviceTransform = new(ReadDeviceTransform);

    internal static Matrix3x2 GetDeviceTransform()
    {
        return DeviceTransform.Value;
    }

    private static Matrix3x2 ReadDeviceTransform()
    {
        uint systemDpi = 0;
        try
        {
            systemDpi = PInvoke.GetDpiForSystem();
        }
        catch (EntryPointNotFoundException)
        {
        }

        if (systemDpi > 0)
        {
            float scale = systemDpi / DefaultPixelsPerInch;
            return Matrix3x2.CreateScale(scale);
        }

        fixed (char* display = "DISPLAY")
        {
            HDC deviceContext = PInvoke.CreateICW(new PCWSTR(display), default, default, null);
            if (!deviceContext.IsNull)
            {
                try
                {
                    int dpiX = PInvoke.GetDeviceCaps(deviceContext, GET_DEVICE_CAPS_INDEX.LOGPIXELSX);
                    int dpiY = PInvoke.GetDeviceCaps(deviceContext, GET_DEVICE_CAPS_INDEX.LOGPIXELSY);
                    if (dpiX > 0 && dpiY > 0)
                    {
                        return Matrix3x2.CreateScale(
                            dpiX / DefaultPixelsPerInch,
                            dpiY / DefaultPixelsPerInch);
                    }
                }
                finally
                {
                    _ = PInvoke.DeleteDC(deviceContext);
                }
            }
        }

        return Matrix3x2.Identity;
    }
}
