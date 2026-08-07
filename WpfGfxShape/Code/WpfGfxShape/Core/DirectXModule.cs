using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal enum DirectXModule
{
    Direct3D9,
    Direct3D11,
    Dxgi,
}

internal static class DirectXModuleInfo
{
    internal const string Direct3D9LibraryName = "d3d9.dll";
    internal const string Direct3D11LibraryName = "d3d11.dll";
    internal const string DxgiLibraryName = "dxgi.dll";

    internal const string Direct3DCreate9EntryPoint = "Direct3DCreate9";
    internal const string Direct3DCreate9ExEntryPoint = "Direct3DCreate9Ex";
    internal const string Direct3D11CreateDeviceEntryPoint = "D3D11CreateDevice";
    internal const string CreateDxgiFactoryEntryPoint = "CreateDXGIFactory";
    internal const string CreateDxgiFactory1EntryPoint = "CreateDXGIFactory1";
    internal const string CreateDxgiFactory2EntryPoint = "CreateDXGIFactory2";

    internal static uint Direct3D9SdkVersion => D3D9.SdkVersion;

    internal static string GetLibraryName(DirectXModule module)
    {
        return module switch
        {
            DirectXModule.Direct3D9 => Direct3D9LibraryName,
            DirectXModule.Direct3D11 => Direct3D11LibraryName,
            DirectXModule.Dxgi => DxgiLibraryName,
            _ => throw new ArgumentOutOfRangeException(nameof(module), module, null),
        };
    }
}
