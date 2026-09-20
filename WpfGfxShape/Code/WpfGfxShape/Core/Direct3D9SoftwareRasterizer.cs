using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed class Direct3D9SoftwareRasterizerLoader : IDisposable
{
    internal const string CurrentLibraryName = "RGB9Rast.dll";
    internal const string DownlevelLibraryName = "RGB9Rast_2.dll";
    internal const string GetSoftwareInfoEntryPoint = "D3D9GetSWInfo";

    private readonly object _syncRoot = new();
    private readonly Func<string, SafeHandle> _moduleLoader;
    private readonly Func<nint, string, nint> _exportResolver;
    private SafeHandle? _moduleHandle;
    private ExceptionDispatchInfo? _loadFailure;
    private nint _getSoftwareInfo;
    private bool _loadAttempted;
    private bool _isDisposed;

    internal Direct3D9SoftwareRasterizerLoader(
        Func<string, SafeHandle>? moduleLoader = null,
        Func<nint, string, nint>? exportResolver = null)
    {
        _moduleLoader = moduleLoader ?? LoadSystemModule;
        _exportResolver = exportResolver ?? NativeLibrary.GetExport;
    }

    internal nint GetSoftwareInfo()
    {
        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            if (!_loadAttempted)
            {
                Load();
            }

            _loadFailure?.Throw();
            return _getSoftwareInfo;
        }
    }

    internal static string GetLibraryName()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(6)
            ? CurrentLibraryName
            : DownlevelLibraryName;
    }

    private void Load()
    {
        _loadAttempted = true;
        SafeHandle? moduleHandle = null;
        try
        {
            moduleHandle = _moduleLoader(GetLibraryName());
            nint getSoftwareInfo = _exportResolver(moduleHandle.DangerousGetHandle(), GetSoftwareInfoEntryPoint);
            if (getSoftwareInfo == 0)
            {
                throw new EntryPointNotFoundException(GetSoftwareInfoEntryPoint);
            }

            _moduleHandle = moduleHandle;
            _getSoftwareInfo = getSoftwareInfo;
        }
        catch (Win32Exception exception)
        {
            CacheLoadFailure(moduleHandle, exception);
            throw;
        }
        catch (DllNotFoundException exception)
        {
            CacheLoadFailure(moduleHandle, exception);
            throw;
        }
        catch (EntryPointNotFoundException exception)
        {
            CacheLoadFailure(moduleHandle, exception);
            throw;
        }
    }

    private void CacheLoadFailure(SafeHandle? moduleHandle, Exception exception)
    {
        moduleHandle?.Dispose();
        _loadFailure = ExceptionDispatchInfo.Capture(exception);
    }

    private static DirectXModuleHandle LoadSystemModule(string libraryName)
    {
        return DirectXSystemModule.Load(libraryName);
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _getSoftwareInfo = 0;
            _moduleHandle?.Dispose();
            _moduleHandle = null;
        }
    }
}

internal sealed class Direct3D9SoftwareRasterizerRegistration
{
    private readonly object _syncRoot = new();
    private readonly Direct3D9SoftwareRasterizerLoader _loader;
    private readonly Func<nint, int> _registerSoftwareDevice;
    private readonly Direct3D9DisplaySet _displaySet;
    private int? _registrationResult;

    internal Direct3D9SoftwareRasterizerRegistration(
        Direct3D9SoftwareRasterizerLoader loader,
        Func<nint, int> registerSoftwareDevice,
        Direct3D9DisplaySet? displaySet = null)
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentNullException.ThrowIfNull(registerSoftwareDevice);
        _loader = loader;
        _registerSoftwareDevice = registerSoftwareDevice;
        _displaySet = displaySet ?? new Direct3D9DisplaySet();
    }

    internal void EnsureRegistered()
    {
        if (_displaySet.DangerousHasDisplayStateChanged())
        {
            Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
        }

        lock (_syncRoot)
        {
            if (_registrationResult is null)
            {
                _registrationResult = _registerSoftwareDevice(_loader.GetSoftwareInfo());
            }

            Marshal.ThrowExceptionForHR(_registrationResult.GetValueOrDefault());
        }
    }
}
