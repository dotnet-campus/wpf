using System.Runtime.InteropServices;
using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct MilD3DImageCommand
{
    internal readonly MilCommand Type;
    internal readonly uint Handle;
    internal readonly ulong InteropBitmap, SoftwareBitmap;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct MilD3DImagePresentCommand
{
    internal readonly MilCommand Type;
    internal readonly uint Handle;
    internal readonly ulong Event;
}

internal sealed unsafe class GeneratedD3DImageResource() : GeneratedProtocolResource(MilResourceType.D3DImage)
{
    private FrozenComSlot? _interopSlot, _softwareSlot;
    private nint _interop => _interopSlot?.Pointer ?? 0;
    private nint _software => _softwareSlot?.Pointer ?? 0;

    internal FrozenBitmapLease? PinSoftwareSource() => _softwareSlot?.Pin();
    internal FrozenBitmapLease? PinInteropSource() => _interopSlot?.Pin();
    private readonly Dictionary<Direct3D9Device, Direct3D9InteropColorSource> _dependentSources = [];

    private void ReleaseDependentSources()
    {
        foreach (Direct3D9InteropColorSource source in _dependentSources.Values) source.Dispose();
        _dependentSources.Clear();
    }

    internal MilRectD GetBounds()
    {
        if (_interop == 0) return default;
        return InteropDeviceBitmapExports.GetContentBounds(_interop);
    }

    internal int TryCreateDependentDeviceColorSource(Direct3D9Device device, out Direct3D9InteropColorSource? source)
    {
        ArgumentNullException.ThrowIfNull(device);
        source = null;
        if (_dependentSources.TryGetValue(device, out Direct3D9InteropColorSource? cached))
        {
            if (_software == 0 && InteropDeviceBitmapExports.IsDependentDeviceColorSourceCurrent(_interop, cached))
            {
                source = cached;
                return 0;
            }
            _dependentSources.Remove(device);
            cached.Dispose();
        }
        if (_software != 0 || _interop == 0) return 1;
        int hr = InteropDeviceBitmapExports.TryCreateDependentDeviceColorSource(_interop, device.Device, out source);
        if (hr != 0 || source is null) return hr;
        try { _dependentSources.Add(device, source); }
        catch (OutOfMemoryException)
        {
            source.Dispose();
            source = null;
            throw;
        }
        return 0;
    }

    internal int TryCreateDependentPipelineColorSource(Direct3D9Device device,
        System.Numerics.Matrix3x2 devicePointToTextureUv, MilBitmapInterpolationMode interpolationMode,
        Silk.NET.Direct3D9.Textureaddress addressU, Silk.NET.Direct3D9.Textureaddress addressV,
        out Direct3D9PipelineColorSource? source)
    {
        source = null;
        int hr = TryCreateDependentDeviceColorSource(device, out Direct3D9InteropColorSource? dependent);
        if (hr != 0 || dependent is null) return hr;
        source = dependent.CreatePipelineColorSource(device, devicePointToTextureUv,
            interpolationMode, addressU, addressV);
        return 0;
    }

    internal nint AcquireBitmapSource()
    {
        using FrozenBitmapLease? software = PinSoftwareSource();
        if (software is not null) return software.Acquire();
        using FrozenBitmapLease? interop = PinInteropSource();
        return InteropDeviceBitmapExports.AcquireBitmapSource(interop?.Pointer ?? 0);
    }

    internal int ProcessCommand(MilCommand command, ReadOnlySpan<byte> packet)
    {
        if (command == MilCommand.D3DImage && packet.Length == Marshal.SizeOf<MilD3DImageCommand>())
        {
            var update = MemoryMarshal.Read<MilD3DImageCommand>(packet);
            nint interop = (nint)update.InteropBitmap, software = (nint)update.SoftwareBitmap;
            FrozenComSlot? previousInterop = _interopSlot, previousSoftware = _softwareSlot;
            _interopSlot = _softwareSlot = null;
            ReleaseDependentSources();
            previousSoftware?.Retire();
            previousInterop?.Retire();
            try
            {
                if (interop != 0 && !InteropDeviceBitmapExports.IsInterop(interop)) return Direct3D9Factory.InvalidArgumentHResult;
                _interopSlot = interop == 0 ? null : new FrozenComSlot(interop); interop = 0;
                if (software != 0)
                {
                    int result = WicBitmapAdapter.Create(software, out nint adapter);
                    if (result < 0)
                    {
                        FrozenComSlot? failedInterop = _interopSlot;
                        _interopSlot = null;
                        failedInterop?.Retire();
                        return result;
                    }
                    try { _softwareSlot = new FrozenComSlot(adapter); }
                    catch (OutOfMemoryException) { Direct3D9Factory.Release(adapter); throw; }
                }
                else InstallSoftware(InteropDeviceBitmapExports.AcquireBitmapSource(_interop, fallbackOnly: true));
                return 0;
            }
            finally
            {
                Direct3D9Factory.Release(software);
                Direct3D9Factory.Release(interop);
                NotifyChanged();
            }
        }
        if (command == MilCommand.D3DImagePresent && packet.Length == Marshal.SizeOf<MilD3DImagePresentCommand>())
        {
            var present = MemoryMarshal.Read<MilD3DImagePresentCommand>(packet);
            try
            {
                using FrozenBitmapLease? lease = PinInteropSource();
                nint interop = lease?.Pointer ?? 0;
                if (interop == 0) return 0;
                int result = InteropDeviceBitmapExports.Present(interop);
                if (result < 0) return result;
                // Availability callbacks can replace the resource while Present is active.
                if (_interop != interop || IsReleased) return 0;
                nint software = InteropDeviceBitmapExports.AcquireBitmapSource(interop, fallbackOnly: true);
                InstallSoftware(software);
                NotifyChanged();
                return 0;
            }
            finally { GeneratedDoubleBufferedBitmapResource.Complete((nint)present.Event); }
        }
        return Direct3D9Factory.UceMalformedPacketHResult;
    }

    private void InstallSoftware(nint software)
    {
        FrozenComSlot? next;
        try { next = software == 0 ? null : new FrozenComSlot(software); }
        catch (OutOfMemoryException) { Direct3D9Factory.Release(software); throw; }
        FrozenComSlot? previous = _softwareSlot;
        _softwareSlot = next;
        previous?.Retire();
    }

    protected override void OnFinalRelease()
    {
        FrozenComSlot? interop = _interopSlot, software = _softwareSlot;
        _interopSlot = _softwareSlot = null;
        ReleaseDependentSources();
        software?.Retire();
        interop?.Retire();
    }
}
