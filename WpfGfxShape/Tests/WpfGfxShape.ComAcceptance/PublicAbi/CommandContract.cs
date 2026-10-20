namespace NativeAcceptance.PublicAbi;
internal static unsafe partial class Program
{
    private static void ExpectCommand(string operation, int result, int expected)
    {
        Calls.Add(new { Operation = operation, HResult = result, Expected = expected });
        if (result != expected) throw new InvalidOperationException(operation);
    }

    private static void CommandContract(nint channel, uint resource)
    {
        var begin = (delegate* unmanaged[Stdcall]<nint, void*, uint, uint, int>)Export("MilChannel_BeginCommand");
        var append = (delegate* unmanaged[Stdcall]<nint, void*, uint, int>)Export("MilChannel_AppendCommandData");
        var end = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_EndCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
        var refs = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_GetRefCountOnChannel");
        uint count = 0;
        Check("Command.RefCount", refs(channel, resource, &count));
        if (count != 1) throw new InvalidOperationException("Command.RefCount.Value");
        ExpectCommand("Command.EndWithoutBegin", end(channel), unchecked((int)0x88980412));
        ExpectCommand("Command.AppendWithoutBegin", append(channel, null, 0), unchecked((int)0x88980412));
        uint* packet = stackalloc uint[] { 0x0E, resource, 0, 0x3FF00000 };
        ExpectCommand("Command.InvalidHeader", begin(channel, null, 4, 0), unchecked((int)0x80070057));
        ExpectCommand("Command.Overflow", begin(channel, packet, 4, uint.MaxValue), unchecked((int)0x80070216));
        Check("Command.Begin", begin(channel, packet, 8, 8));
        ExpectCommand("Command.NestedBegin", begin(channel, packet, 4, 0), unchecked((int)0x88980411));
        ExpectCommand("Command.ClosePartial", close(channel), unchecked((int)0x88980411));
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
        ExpectCommand("Command.FlushPartial", flush(channel), unchecked((int)0x88980411));
        ExpectCommand("Command.RefCountPartial", refs(channel, resource, &count), unchecked((int)0x88980411));
        ExpectCommand("Command.NullAppend", append(channel, null, 1), unchecked((int)0x80070057));
        ExpectCommand("Command.OversizedAppend", append(channel, packet, uint.MaxValue), unchecked((int)0x8000FFFF));
        Check("Command.EmptyAppend", append(channel, null, 0));
        Check("Command.Append", append(channel, packet + 2, 8));
        Check("Command.End", end(channel));
    }
}
