using System.Collections.Immutable;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9MonitorEnumerationEntry(
    nint MonitorHandle,
    ReadOnlyMemory<char> DeviceNameBuffer,
    Direct3D9SurfaceRect Bounds);

internal readonly record struct Direct3D9MonitorSnapshot(
    string DeviceName,
    nint MonitorHandle,
    ImmutableArray<Direct3D9DisplayBounds> Bounds);

internal delegate void Direct3D9MonitorEnumerator(
    nint dpiAwarenessContextValue,
    Action<Direct3D9MonitorEnumerationEntry> callback);

internal static unsafe class Direct3D9MonitorSnapshotFactory
{
    private static readonly ImmutableArray<nint> DpiAwarenessContextValues = [-1, -2, -3, -4];

    internal static ImmutableArray<Direct3D9MonitorSnapshot> Enumerate(
        ImmutableArray<Direct3D9DisplayDeviceSnapshot> devices,
        ImmutableArray<nint> dpiAwarenessContextValues,
        Direct3D9MonitorEnumerator enumerator)
    {
        ArgumentNullException.ThrowIfNull(enumerator);

        MonitorSnapshotBuilder[] snapshots = devices
            .Select(device => new MonitorSnapshotBuilder(device.DeviceName))
            .ToArray();

        foreach (nint dpiAwarenessContextValue in dpiAwarenessContextValues)
        {
            enumerator(
                dpiAwarenessContextValue,
                entry =>
                {
                    string deviceName = ValidateDeviceName(entry.DeviceNameBuffer.Span);
                    MonitorSnapshotBuilder? snapshot = FindByName(snapshots, deviceName);
                    snapshot?.SetMonitorInfo(dpiAwarenessContextValue, entry.MonitorHandle, entry.Bounds);
                });
        }

        ImmutableArray<Direct3D9MonitorSnapshot>.Builder result =
            ImmutableArray.CreateBuilder<Direct3D9MonitorSnapshot>(snapshots.Length);
        foreach (MonitorSnapshotBuilder snapshot in snapshots)
        {
            result.Add(snapshot.Build());
        }

        return result.MoveToImmutable();
    }

    [SupportedOSPlatform("windows5.0")]
    internal static ImmutableArray<Direct3D9MonitorSnapshot> EnumerateSystem(
        Direct3D9DisplayDeviceEnumerationResult devices)
    {
        ImmutableArray<nint>.Builder dpiAwarenessContexts = ImmutableArray.CreateBuilder<nint>();
        foreach (nint dpiAwarenessContextValue in DpiAwarenessContextValues)
        {
            if (NativeMethods.IsValidDpiAwarenessContext(dpiAwarenessContextValue))
            {
                dpiAwarenessContexts.Add(dpiAwarenessContextValue);
            }
        }

        if (dpiAwarenessContexts.Count == 0)
        {
            dpiAwarenessContexts.Add(0);
        }

        return Enumerate(devices.Devices, dpiAwarenessContexts.MoveToImmutable(), EnumerateSystemMonitors);
    }

    private static MonitorSnapshotBuilder? FindByName(MonitorSnapshotBuilder[] snapshots, string deviceName)
    {
        for (int index = snapshots.Length - 1; index >= 0; index--)
        {
            if (string.Equals(snapshots[index].DeviceName, deviceName, StringComparison.Ordinal))
            {
                return snapshots[index];
            }
        }

        return null;
    }

    private static string ValidateDeviceName(ReadOnlySpan<char> deviceNameBuffer)
    {
        int terminatorIndex = deviceNameBuffer.IndexOf('\0');
        if (terminatorIndex <= 0)
        {
            Marshal.ThrowExceptionForHR(unchecked((int) 0x80070057));
        }

        return new string(deviceNameBuffer[..terminatorIndex]);
    }

    [SupportedOSPlatform("windows5.0")]
    private static void EnumerateSystemMonitors(
        nint dpiAwarenessContextValue,
        Action<Direct3D9MonitorEnumerationEntry> callback)
    {
        using DpiAwarenessScope scope = new(dpiAwarenessContextValue);
        ExceptionDispatchInfo? callbackFailure = null;
        MONITORENUMPROC monitorEnumProcedure = MonitorEnumProcedure;

        BOOL MonitorEnumProcedure(HMONITOR monitor, HDC monitorDeviceContext, RECT* bounds, LPARAM data)
        {
            try
            {
                MONITORINFOEXW monitorInfo = default;
                monitorInfo.monitorInfo.cbSize = (uint) sizeof(MONITORINFOEXW);
                Marshal.SetLastPInvokeError(0);
                if (!PInvoke.GetMonitorInfo(monitor, ref monitorInfo.monitorInfo))
                {
                    ThrowLastError();
                }

                char[] deviceNameBuffer = new char[32];
                monitorInfo.szDevice.ToString().AsSpan().CopyTo(deviceNameBuffer);
                callback(new Direct3D9MonitorEnumerationEntry(
                    (nint) monitor.Value,
                    deviceNameBuffer,
                    new Direct3D9SurfaceRect(bounds->left, bounds->top, bounds->right, bounds->bottom)));
                return true;
            }
            catch (Exception exception) when (exception is not StackOverflowException and not OutOfMemoryException)
            {
                callbackFailure = ExceptionDispatchInfo.Capture(exception);
                return false;
            }
        }

        Marshal.SetLastPInvokeError(0);
        bool succeeded = PInvoke.EnumDisplayMonitors(default, null, monitorEnumProcedure, default);
        GC.KeepAlive(monitorEnumProcedure);
        callbackFailure?.Throw();
        if (!succeeded)
        {
            ThrowLastError();
        }
    }

    private static void ThrowLastError()
    {
        const int errorInvalidMonitorHandle = 1461;
        const int errorInvalidPrinterName = 1801;

        int error = Marshal.GetLastPInvokeError();
        int hResult = error is 0 or errorInvalidMonitorHandle or errorInvalidPrinterName
            ? Direct3D9Factory.DisplayStateInvalidHResult
            : unchecked((int) (0x80070000U | (uint) error));
        Marshal.ThrowExceptionForHR(hResult);
    }

    private sealed class MonitorSnapshotBuilder(string deviceName)
    {
        private readonly ImmutableArray<Direct3D9DisplayBounds>.Builder _bounds =
            ImmutableArray.CreateBuilder<Direct3D9DisplayBounds>();

        internal string DeviceName { get; } = deviceName;

        private nint MonitorHandle { get; set; }

        internal void SetMonitorInfo(
            nint dpiAwarenessContextValue,
            nint monitorHandle,
            Direct3D9SurfaceRect bounds)
        {
            if (monitorHandle == 0)
            {
                Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
            }

            if (_bounds.Any(candidate => candidate.DpiAwarenessContextValue == dpiAwarenessContextValue))
            {
                Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
            }

            if (MonitorHandle == 0)
            {
                MonitorHandle = monitorHandle;
            }

            _bounds.Add(new Direct3D9DisplayBounds(dpiAwarenessContextValue, bounds));
        }

        internal Direct3D9MonitorSnapshot Build()
        {
            if (MonitorHandle == 0)
            {
                Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
            }

            return new Direct3D9MonitorSnapshot(DeviceName, MonitorHandle, _bounds.ToImmutable());
        }
    }

    private sealed class DpiAwarenessScope : IDisposable
    {
        private readonly nint _oldDpiAwarenessContext;

        internal DpiAwarenessScope(nint dpiAwarenessContext)
        {
            if (NativeMethods.IsValidDpiAwarenessContext(dpiAwarenessContext))
            {
                _oldDpiAwarenessContext = NativeMethods.SetThreadDpiAwarenessContext(dpiAwarenessContext);
            }
        }

        public void Dispose()
        {
            if (NativeMethods.IsValidDpiAwarenessContext(_oldDpiAwarenessContext))
            {
                NativeMethods.SetThreadDpiAwarenessContext(_oldDpiAwarenessContext);
            }
        }
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern nint SetThreadDpiAwarenessContext(nint dpiAwarenessContext);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsValidDpiAwarenessContext(nint dpiAwarenessContext);
    }
}
