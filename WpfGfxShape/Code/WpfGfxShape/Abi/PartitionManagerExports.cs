using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static class PartitionManagerExports
{
    internal static readonly object CompositionGate = new();
    private static object Gate => CompositionGate;

    internal static unsafe int GetComposedEventId(uint* output)
    {
        if (output == null) return unchecked((int)0x80070057);
        // The default native partition thread returns S_OK without writing an ID.
        lock (Gate) return _scheduler is null ? unchecked((int)0x88982F0C) : 0;
    }
    private static PartitionScheduler? _scheduler;
    private static int _references;
    private static int _initializations;

    internal static int Acquire(out PartitionScheduler? scheduler)
    {
        lock (Gate)
        {
            scheduler = null;
            if (_scheduler is null)
            {
                var candidate = new PartitionScheduler();
                if (candidate.InitializationResult < 0)
                {
                    candidate.Dispose();
                    return candidate.InitializationResult;
                }
                _scheduler = candidate;
            }
            _references++;
            scheduler = _scheduler;
            return 0;
        }
    }

    internal static void Release()
    {
        PartitionScheduler? stop = null;
        lock (Gate)
        {
            if (_references == 0) return;
            if (--_references == 0)
            {
                stop = _scheduler;
                _scheduler = null;
            }
        }
        stop?.Dispose();
    }

    [UnmanagedCallersOnly(EntryPoint = "MilCompositionEngine_InitializePartitionManager", CallConvs = [typeof(CallConvStdcall)])]
    private static int Initialize(int priority)
    {
        if (priority is not (-2 or -1 or 0 or 1 or 2 or 15 or -15)) return unchecked((int)0x80070057);
        lock (Gate)
        {
            try
            {
                int result = Acquire(out PartitionScheduler? scheduler);
                if (result < 0) return result;
                result = scheduler!.UpdatePriority(priority);
                if (result < 0)
                {
                    Release();
                    return result;
                }
                _initializations++;
                return 0;
            }
            catch (OutOfMemoryException error) { return PartitionScheduler.RecordFailure(error); }
            catch (InvalidOperationException error) { return PartitionScheduler.RecordFailure(error); }
            catch (System.ComponentModel.Win32Exception error) { return PartitionScheduler.RecordFailure(error); }
            catch (ExternalException error) { return PartitionScheduler.RecordFailure(error); }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilCompositionEngine_UpdateSchedulerSettings", CallConvs = [typeof(CallConvStdcall)])]
    private static int Update(int priority)
    {
        lock (Gate) return _scheduler?.UpdatePriority(priority) ?? unchecked((int)0x8000FFFF);
    }

    [UnmanagedCallersOnly(EntryPoint = "MilCompositionEngine_DeinitializePartitionManager", CallConvs = [typeof(CallConvStdcall)])]
    private static int Deinitialize()
    {
        lock (Gate)
        {
            if (_initializations == 0) return 0;
            _initializations--;
        }
        Release();
        return 0;
    }
}
