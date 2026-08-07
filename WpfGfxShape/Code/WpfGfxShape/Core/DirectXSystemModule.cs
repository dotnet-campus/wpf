using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.LibraryLoader;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal static unsafe class DirectXSystemModule
{
    internal static DirectXModuleHandle Load(DirectXModule module)
    {
        string libraryName = DirectXModuleInfo.GetLibraryName(module);
        fixed (char* libraryNamePointer = libraryName)
        {
            HMODULE moduleHandle = PInvoke.LoadLibraryEx(
                new PCWSTR(libraryNamePointer),
                default,
                LOAD_LIBRARY_FLAGS.LOAD_LIBRARY_SEARCH_SYSTEM32);
            if (moduleHandle.IsNull)
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }

            return new DirectXModuleHandle(moduleHandle);
        }
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class DirectXModuleHandle : SafeHandle
{
    internal DirectXModuleHandle(HMODULE moduleHandle)
        : base(0, true)
    {
        SetHandle((nint) moduleHandle.Value);
    }

    public override bool IsInvalid => handle == 0;

    protected override bool ReleaseHandle()
    {
        return PInvoke.FreeLibrary(new HMODULE(handle));
    }
}
