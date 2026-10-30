using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

// Slot order and one-byte bool follow include/wgx_render.h, IMILMedia.
internal static unsafe class MediaExports
{
    private const int InvalidArgument = unchecked((int)0x80070057);

    [UnmanagedCallersOnly(EntryPoint = "MILFactoryCreateMediaPlayer", CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateMediaPlayer(nint factory, nint eventProxy, byte canOpenAnyMedia, nint* output)
    {
        if (output == null) return unchecked((int)0x80004003);
        *output = 0;
        if (factory == 0) return InvalidArgument;
        try
        {
            int result = Core.Av.SystemMediaSource.Create(eventProxy, canOpenAnyMedia, out nint media);
            *output = media;
            return result;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        catch { return unchecked((int)0x8000FFFF); }
    }

    [UnmanagedCallersOnly(EntryPoint = "MILMediaOpen", CallConvs = [typeof(CallConvStdcall)])]
    private static int Open(nint media, char* url) => media == 0 ? InvalidArgument : ((delegate* unmanaged[Stdcall]<nint, char*, int>)(*(void***)media)[3])(media, url);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaStop", CallConvs = [typeof(CallConvStdcall)])]
    private static int Stop(nint media) => Invoke(media, 4);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaClose", CallConvs = [typeof(CallConvStdcall)])]
    private static int Close(nint media) => Invoke(media, 5);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaGetPosition", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetPosition(nint media, long* time) => media == 0 ? InvalidArgument : ((delegate* unmanaged[Stdcall]<nint, long*, int>)(*(void***)media)[6])(media, time);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaSetPosition", CallConvs = [typeof(CallConvStdcall)])]
    private static int SetPosition(nint media, long time) => media == 0 ? InvalidArgument : ((delegate* unmanaged[Stdcall]<nint, long, int>)(*(void***)media)[7])(media, time);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaSetRate", CallConvs = [typeof(CallConvStdcall)])]
    private static int SetRate(nint media, double value) => InvokeDouble(media, 8, value);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaSetVolume", CallConvs = [typeof(CallConvStdcall)])]
    private static int SetVolume(nint media, double value) => InvokeDouble(media, 9, value);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaSetBalance", CallConvs = [typeof(CallConvStdcall)])]
    private static int SetBalance(nint media, double value) => InvokeDouble(media, 10, value);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaSetIsScrubbingEnabled", CallConvs = [typeof(CallConvStdcall)])]
    private static int SetIsScrubbingEnabled(nint media, byte value) => media == 0 ? InvalidArgument : ((delegate* unmanaged[Stdcall]<nint, byte, int>)(*(void***)media)[11])(media, value);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaIsBuffering", CallConvs = [typeof(CallConvStdcall)])]
    private static int IsBuffering(nint media, byte* value) => InvokeBoolean(media, 12, value);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaCanPause", CallConvs = [typeof(CallConvStdcall)])]
    private static int CanPause(nint media, byte* value) => InvokeBoolean(media, 13, value);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaGetDownloadProgress", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetDownloadProgress(nint media, double* value) => InvokeDoubleOutput(media, 14, value);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaGetBufferingProgress", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetBufferingProgress(nint media, double* value) => InvokeDoubleOutput(media, 15, value);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaHasVideo", CallConvs = [typeof(CallConvStdcall)])]
    private static int HasVideo(nint media, byte* value) => InvokeBoolean(media, 16, value);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaHasAudio", CallConvs = [typeof(CallConvStdcall)])]
    private static int HasAudio(nint media, byte* value) => InvokeBoolean(media, 17, value);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaGetNaturalHeight", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetNaturalHeight(nint media, uint* value) => InvokeUIntOutput(media, 18, value);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaGetNaturalWidth", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetNaturalWidth(nint media, uint* value) => InvokeUIntOutput(media, 19, value);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaGetMediaLength", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetMediaLength(nint media, long* time) => media == 0 ? InvalidArgument : ((delegate* unmanaged[Stdcall]<nint, long*, int>)(*(void***)media)[20])(media, time);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaNeedUIFrameUpdate", CallConvs = [typeof(CallConvStdcall)])]
    private static int NeedUIFrameUpdate(nint media) => Invoke(media, 21);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaShutdown", CallConvs = [typeof(CallConvStdcall)])]
    private static int Shutdown(nint media) => Invoke(media, 22);

    [UnmanagedCallersOnly(EntryPoint = "MILMediaProcessExitHandler", CallConvs = [typeof(CallConvStdcall)])]
    private static int ProcessExitHandler(nint media) => Invoke(media, 23);

    private static int Invoke(nint media, int slot) => media == 0 ? InvalidArgument : ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)media)[slot])(media);
    private static int InvokeDouble(nint media, int slot, double value) => media == 0 ? InvalidArgument : ((delegate* unmanaged[Stdcall]<nint, double, int>)(*(void***)media)[slot])(media, value);
    private static int InvokeBoolean(nint media, int slot, byte* value) => media == 0 ? InvalidArgument : ((delegate* unmanaged[Stdcall]<nint, byte*, int>)(*(void***)media)[slot])(media, value);
    private static int InvokeDoubleOutput(nint media, int slot, double* value) => media == 0 ? InvalidArgument : ((delegate* unmanaged[Stdcall]<nint, double*, int>)(*(void***)media)[slot])(media, value);
    private static int InvokeUIntOutput(nint media, int slot, uint* value) => media == 0 ? InvalidArgument : ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)media)[slot])(media, value);
}
