internal static unsafe partial class Program
{
    private static void ResourceTransferContract(nint channel)
    {
        Guid clsid = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
        Guid iid = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
        Guid format = new("6fddc324-4e03-4bfe-b185-3d77768dc910");
        nint factory = 0, bitmap = 0, wrapper = 0;
        var send = (delegate* unmanaged[Stdcall]<uint, nint, nint, int>)Export("MilResource_SendCommandBitmapSource");
        var create = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        var release = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)Export("MilResource_ReleaseOnChannel");
        try
        {
            Check("Transfer.WIC.Factory", CoCreateInstance(&clsid, 0, 1, &iid, &factory));
            byte* pixels = stackalloc byte[16] { 0, 0, 255, 255, 0, 255, 0, 255, 255, 0, 0, 255, 255, 255, 255, 255 };
            Check("Transfer.WIC.Bitmap", ((delegate* unmanaged[Stdcall]<nint, uint, uint, Guid*, uint, uint, byte*, nint*, int>)(*(void***)factory)[20])(factory, 2, 2, &format, 8, 16, pixels, &bitmap));
            Check("Transfer.WIC.Wrapper", ((delegate* unmanaged[Stdcall]<nint, nint*, int>)Export("MilResource_CreateCWICWrapperBitmap"))(bitmap, &wrapper));
            uint resource = 0;
            Check("Transfer.Create", create(channel, 95, &resource));
            ExpectCommand("Transfer.NullBitmap", send(resource, 0, channel), unchecked((int)0x80070057));
            ExpectCommand("Transfer.InvalidChannel", send(resource, wrapper, 0), unchecked((int)0x80070057));
            var begin = (delegate* unmanaged[Stdcall]<nint, void*, uint, uint, int>)Export("MilChannel_BeginCommand");
            var end = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_EndCommand");
            uint* packet = stackalloc uint[] { 3, 0 };
            Check("Transfer.Begin", begin(channel, packet, 8, 0));
            ExpectCommand("Transfer.Partial", send(resource, wrapper, channel), unchecked((int)0x88980411));
            Check("Transfer.End", end(channel));
            Check("Transfer.Send", send(resource, wrapper, channel));
            Check("Transfer.Replace", send(resource, wrapper, channel));
            Release(wrapper);
            wrapper = 0;
            Check("Transfer.Close", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch"))(channel));
            Check("Transfer.Commit", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel"))(channel));
            Check("Transfer.Flush", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush"))(channel));
            int deleted = 0;
            Check("Transfer.Release", release(channel, resource, &deleted));
            if (deleted != 1) throw new InvalidOperationException("Transfer.Release.Value");
        }
        finally
        {
            Release(wrapper);
            Release(bitmap);
            Release(factory);
        }
    }
}
