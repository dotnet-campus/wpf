using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal static class Direct3D9DeviceDriverWorkarounds
{
    internal static int Apply(
        Devtype deviceType,
        bool skipDriverCheck,
        bool isRecentDriver,
        bool isBadDriver,
        ref Caps9 capabilities)
    {
        if (skipDriverCheck || deviceType is Devtype.SW or Devtype.Ref)
        {
            return 0;
        }

        if (!isRecentDriver || isBadDriver)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        capabilities.RasterCaps &= unchecked((uint) ~D3D9.PrastercapsScissortest);
        return 0;
    }
}
