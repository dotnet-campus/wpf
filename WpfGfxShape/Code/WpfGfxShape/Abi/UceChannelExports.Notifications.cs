using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe partial class UceChannelExports
{
    [UnmanagedCallersOnly(EntryPoint = "MilCompositionEngine_EnterCompositionEngineLock", CallConvs = [typeof(CallConvStdcall)])]
    private static void EnterCompositionLock() => Monitor.Enter(PartitionManagerExports.CompositionGate);

    [UnmanagedCallersOnly(EntryPoint = "MilCompositionEngine_ExitCompositionEngineLock", CallConvs = [typeof(CallConvStdcall)])]
    private static void ExitCompositionLock() => Monitor.Exit(PartitionManagerExports.CompositionGate);

    [UnmanagedCallersOnly(EntryPoint = "MilCompositionEngine_GetComposedEventId", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetComposedEventId(uint* output) => PartitionManagerExports.GetComposedEventId(output);

    [UnmanagedCallersOnly(EntryPoint = "MilChannel_GetMarshalType", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetMarshalType(nint identity, uint* output)
    {
        if (output == null) return InvalidArgument;
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            *output = channel!.Connection.Scheduler is null ? 1u : 2u;
            return 0;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilChannel_SetReceiveBroadcastMessages", CallConvs = [typeof(CallConvStdcall)])]
    private static int SetReceiveBroadcastMessages(nint identity, byte receive)
    {
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            channel!.State.ReceiveBroadcastMessages = receive != 0;
            return 0;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilChannel_SetNotificationWindow", CallConvs = [typeof(CallConvStdcall)])]
    private static int SetNotificationWindow(nint identity, nint window, uint message)
    {
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            channel!.State.SetNotificationWindow(window, message);
            return 0;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilComposition_WaitForNextMessage", CallConvs = [typeof(CallConvStdcall)])]
    private static int WaitForNextMessage(nint identity, uint count, nint* handles, int waitAll, uint timeout, uint* output)
    {
        if (output == null || count > 63 || (count != 0 && handles == null)) return InvalidArgument;
        SameThreadChannel state;
        try
        {
            lock (Gate)
            {
                if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
                state = channel!.State;
                if (state.Partition.Failure < 0) return state.Partition.Failure;
                int result = state.CloseBatch();
                if (result < 0) return result;
                if (channel.Connection.Scheduler is { } scheduler) scheduler.SubmitBatches(state);
                else
                {
                    result = state.Commit();
                    if (result < 0) return result;
                }
            }
            return state.WaitForMessage(count, handles, waitAll != 0, timeout, output);
        }
        catch (OutOfMemoryException) { return OutOfMemory; }
        catch (ArgumentException error) { return PartitionScheduler.RecordFailure(error); }
        catch (InvalidOperationException error) { return PartitionScheduler.RecordFailure(error); }
        catch (ExternalException error) { return PartitionScheduler.RecordFailure(error); }
        catch (OverflowException error) { return PartitionScheduler.RecordFailure(error); }
    }
}
