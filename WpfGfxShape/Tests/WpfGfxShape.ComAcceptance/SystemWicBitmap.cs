using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

internal sealed unsafe class SystemWicBitmap : IDisposable
{
    [DllImport("ole32.dll")]
    private static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32.dll")]
    private static extern void CoUninitialize();
    [DllImport("ole32.dll")]
    private static extern int CoCreateInstance(Guid* clsid, nint outer, uint context, Guid* iid, nint* instance);

    private readonly bool _initialized;

    internal SystemWicBitmap()
    {
        int result = CoInitializeEx(0, 0);
        if (result < 0 && result != unchecked((int)0x80010106)) Marshal.ThrowExceptionForHR(result);
        _initialized = result >= 0;
    }

    public void Dispose() { if (_initialized) CoUninitialize(); }

    internal nint DecodeFrame(nint stream)
    {
        nint factory = 0, decoder = 0, frame = 0;
        try
        {
            Guid clsid = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
            Guid iid = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
            Marshal.ThrowExceptionForHR(CoCreateInstance(&clsid, 0, 1, &iid, &factory));
            Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, nint, Guid*, uint, nint*, int>)(*(void***)factory)[4])(factory, stream, null, 0, &decoder));
            Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)decoder)[13])(decoder, 0, &frame));
            nint result = frame; frame = 0;
            return result;
        }
        finally
        {
            if (frame != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)frame)[2])(frame);
            if (decoder != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)decoder)[2])(decoder);
            if (factory != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)factory)[2])(factory);
        }
    }

    internal nint CreateSource(nint bitmap)
    {
        nint factory = 0, scaler = 0;
        try
        {
            Guid clsid = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
            Guid iid = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
            Marshal.ThrowExceptionForHR(CoCreateInstance(&clsid, 0, 1, &iid, &factory));
            Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)factory)[11])(factory, &scaler));
            Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint, int>)(*(void***)scaler)[8])(scaler, bitmap, 2, 1, 0));
            nint result = scaler; scaler = 0;
            return result;
        }
        finally
        {
            if (scaler != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)scaler)[2])(scaler);
            if (factory != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)factory)[2])(factory);
        }
    }

    internal nint Create(uint width, uint height)
    {
        nint factory = 0, bitmap = 0;
        try
        {
            Guid clsid = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
            Guid iid = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
            Marshal.ThrowExceptionForHR(CoCreateInstance(&clsid, 0, 1, &iid, &factory));
            Guid format = new("6fddc324-4e03-4bfe-b185-3d77768dc910");
            Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, uint, uint, Guid*, uint, nint*, int>)(*(void***)factory)[17])(factory, width, height, &format, 2, &bitmap));
            return bitmap;
        }
        finally
        {
            if (factory != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)factory)[2])(factory);

        }
    }
}
