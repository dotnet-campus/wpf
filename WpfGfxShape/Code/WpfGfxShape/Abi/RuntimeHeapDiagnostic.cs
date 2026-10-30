#if WPFGFX_RUNTIME_HEAP_DIAGNOSTIC
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

internal static unsafe class RuntimeHeapDiagnostic
{
    private static readonly object Gate = new();
    private static readonly List<(WeakReference Scheduler, WeakReference Thread)> Owners = [];

    internal static void Track(object scheduler, Thread thread)
    {
        lock (Gate) Owners.Add((new(scheduler), new(thread)));
    }

    [UnmanagedCallersOnly(EntryPoint = "WpfGfxShape_RuntimeHeapDiagnostic_v1", CallConvs = [typeof(CallConvStdcall)])]
    private static int Observe(uint operation, long* output, uint count)
    {
        if (output == null || count != 6 || operation > 1) return unchecked((int)0x80070057);
        if (operation == 1)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        lock (Gate)
        {
            output[0] = Owners.Count;
            output[1] = Owners.Count(item => item.Scheduler.IsAlive);
            output[2] = Owners.Count(item => item.Thread.IsAlive);
        }
        output[3] = GC.CollectionCount(0);
        output[4] = GC.CollectionCount(1);
        output[5] = GC.CollectionCount(2);
        return 0;
    }
}
#endif
