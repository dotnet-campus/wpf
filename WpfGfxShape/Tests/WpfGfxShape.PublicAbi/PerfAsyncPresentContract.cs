using System.Diagnostics;
using System.Runtime.InteropServices;

internal static unsafe partial class Program
{
    private static bool asyncPerfPassed;
    private static bool asyncReferenceObservation;

    private static nint asyncPreviousProc;
    private static int asyncDetachResult, asyncCallbackCount;
    private static bool asyncDetachArmed;

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", ExactSpelling = true)]
    private static extern nint SetAsyncWindowProc(nint window, int index, nint procedure);
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern nint CallWindowProcW(nint previous, nint window, uint message, nuint wparam, nint lparam);
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern nint DispatchMessageW(Message* message);

    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
    private static nint AsyncWindowProc(nint window, uint message, nuint wparam, nint lparam)
    {
        if (message == 0x400 && asyncDetachArmed)
        {
            asyncDetachArmed = false;
            asyncCallbackCount++;
            asyncDetachResult = ((delegate* unmanaged[Stdcall]<nint, int>)GetProcAddress(module, "MilVisualTarget_DetachFromHwnd"))(window);
        }
        return CallWindowProcW(asyncPreviousProc, window, message, wparam, lparam);
    }

    private static void PerfAsyncPresentContract(string? failure = null)
    {
        if (failure is null) asyncPerfPassed = true;
        nint window = CreateWindowExW(8, "STATIC", "Async Present ABI", 0x90000000, 0, 0, 64, 32, 0, 0, 0, 0);
        nint connection = 0, channel = 0;
        var flags = (delegate* unmanaged[Stdcall]<uint, void>)Export("SetMilPerfInstrumentationFlags");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        bool initialized = false;
        try
        {
            if (window == 0) throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
            Check("AsyncPerf.Initialize", ((delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager"))(0));
            initialized = true;
            flags(0);
            Check("AsyncPerf.Connect", ((delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create"))(0, &connection));
            Check("AsyncPerf.Channel", ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel"))(connection, 0, &channel));
            uint marshalType = 0;
            Check("AsyncPerf.MarshalType", ((delegate* unmanaged[Stdcall]<nint, uint*, int>)Export("MilChannel_GetMarshalType"))(channel, &marshalType));
            if (marshalType != 2) throw new InvalidOperationException("AsyncPerf.NotCrossThread");
            uint target = 0, visual = 0;
            Check("AsyncPerf.Target", resource(channel, 46, &target));
            Check("AsyncPerf.Visual", resource(channel, 39, &visual));
            byte* packet = stackalloc byte[92];
            new Span<byte>(packet, 92).Clear();
            *(uint*)packet = 0x31; *(uint*)(packet + 4) = target;
            *(ulong*)(packet + 8) = (ulong)window;
            *(uint*)(packet + 32) = 64; *(uint*)(packet + 36) = 32;
            *(float*)(packet + 40) = 1;
            *(double*)(packet + 76) = 96; *(double*)(packet + 84) = 96;
            Check("AsyncPerf.CreateTarget", send(packet, 92, 0, channel));
            byte* settings = stackalloc byte[72];
            new Span<byte>(settings, 72).Clear();
            *(uint*)settings = 0x33; *(uint*)(settings + 4) = target;
            *(int*)(settings + 16) = 64; *(int*)(settings + 20) = 32;
            *(float*)(settings + 32) = 1; *(int*)(settings + 44) = 1;
            Check("AsyncPerf.WindowSettings", send(settings, 72, 0, channel));
            uint* binding = stackalloc uint[3] { 0x35, target, visual };
            Check("AsyncPerf.Bind", send(binding, 12, 0, channel));
            if (failure is not null)
            {
                FlushAsyncPerf(channel);
                ObserveAsyncPerf(window, "failure-setup", 0, 0);
                flags(2);
                uint size = 72;
                switch (failure)
                {
                    case "invalid-target": *(uint*)(settings + 4) = uint.MaxValue; break;
                    case "zero-target": *(uint*)(settings + 4) = 0; break;
                    case "wrong-type": *(uint*)(settings + 4) = visual; break;
                    case "short-size": size = 68; break;
                    case "unaligned-size": size = 71; break;
                    case "valid-settings": break;
                    case "invalid-target-and-settings":
                        *(uint*)(settings + 4) = uint.MaxValue;
                        *(uint*)(settings + 24) = uint.MaxValue;
                        break;
                    default: *(uint*)(settings + 24) = uint.MaxValue; break;
                }
                int sendResult = send(settings, size, 0, channel);
                TraceAsyncPerf("settings-send", sendResult);
                int closeResult = ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch"))(channel);
                TraceAsyncPerf("settings-close", closeResult);
                int commitResult = ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel"))(channel);
                TraceAsyncPerf("settings-commit", commitResult);
                int result = ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush"))(channel);
                TraceAsyncPerf("settings-flush", result);
                Calls.Add(new { Operation = "AsyncPerfFailure", Scenario = failure, Send = sendResult, Close = closeResult, Commit = commitResult, HResult = result });
                bool illegalSize = failure is "short-size" or "unaligned-size";
                bool rawOnly = illegalSize && asyncReferenceObservation;
                int expected = failure == "valid-settings" ? 0 : unchecked((int)0x88980406);
                Calls.Add(new { Operation = "AsyncPerfInputContract", Scenario = failure,
                    PacketBytes = size, NativeStructureBytes = 72, LegalInput = !illegalSize,
                    ExactReferenceOracle = !illegalSize, RawOnly = rawOnly });
                if (!rawOnly)
                    asyncPerfPassed &= sendResult == 0 && closeResult == 0 && commitResult == 0 && result == expected;
                ObserveAsyncPerf(window, failure, 2, rawOnly ? -1 : expected == 0 ? 1 : 0);
                int repeated = ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush"))(channel);
                TraceAsyncPerf("repeat-flush", repeated);
                Calls.Add(new { Operation = "AsyncPerfZombie", Scenario = failure, HResult = repeated });
                if (!rawOnly) asyncPerfPassed &= repeated == result;
                ObserveAsyncPerf(window, failure + "-repeat-flush", 2, rawOnly || expected == 0 ? -1 : 0);
                return;
            }
            FlushAsyncPerf(channel);
            ObserveAsyncPerf(window, "initial-disabled", 0, 0);
            foreach (uint value in new uint[] { 0, 2, 0 })
            {
                flags(value);
                flags(value);
                binding[2] = 0;
                Check("AsyncPerf.Unbind", send(binding, 12, 0, channel));
                binding[2] = visual;
                Check("AsyncPerf.Rebind", send(binding, 12, 0, channel));
                FlushAsyncPerf(channel);
                ObserveAsyncPerf(window, "dirty-submit", value, value == 2 ? 1 : 0);
                FlushAsyncPerf(channel);
                ObserveAsyncPerf(window, "empty-submit", value, value == 2 ? 1 : 0);
            }
            flags(2);
            binding[2] = 0;
            Check("AsyncPerf.DetachQueued", send(binding, 12, 0, channel));
            FlushAsyncPerf(channel);
            ObserveAsyncPerf(window, "detached-empty-visual", 2, 1);
            Check("AsyncPerf.CallbackAttach", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilVisualTarget_AttachToHwnd"))(window));
            asyncPreviousProc = SetAsyncWindowProc(window, -4, (nint)(delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, nint>)&AsyncWindowProc);
            if (asyncPreviousProc == 0) throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
            asyncCallbackCount = 0;
            asyncDetachResult = int.MinValue;
            asyncDetachArmed = true;
            binding[2] = visual;
            Check("AsyncPerf.ReentrantRebind", send(binding, 12, 0, channel));
            FlushAsyncPerf(channel);
            ObserveAsyncPerf(window, "rebound", 2, 1);
            Calls.Add(new { Operation = "AsyncPerfCallbackDetach", Count = asyncCallbackCount, HResult = asyncDetachResult });
            asyncPerfPassed &= asyncCallbackCount == 1 && asyncDetachResult == 0;
            SetAsyncWindowProc(window, -4, asyncPreviousProc);
            asyncPreviousProc = 0;
            flags(0);
        }
        finally
        {
            asyncDetachArmed = false;
            if (asyncPreviousProc != 0 && window != 0)
            {
                SetAsyncWindowProc(window, -4, asyncPreviousProc);
                asyncPreviousProc = 0;
            }
            flags(0);
            if (channel != 0) Check("AsyncPerf.Destroy", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel"))(channel));
            if (connection != 0) Check("AsyncPerf.Disconnect", ((delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect"))(connection));
            if (initialized) Check("AsyncPerf.Deinitialize", ((delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager"))());
            if (window != 0)
            {
                ObserveAsyncPerf(window, "closed", 0, 0);
                DestroyWindow(window);
            }
        }
    }

    private static void FlushAsyncPerf(nint channel)
    {
        int closed = ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch"))(channel);
        TraceAsyncPerf("close", closed);
        Check("AsyncPerf.CloseBatch", closed);
        int committed = ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel"))(channel);
        TraceAsyncPerf("commit", committed);
        Check("AsyncPerf.Commit", committed);
        int flushed = ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush"))(channel);
        TraceAsyncPerf("flush", flushed);
        Check("AsyncPerf.SyncFlush", flushed);
    }

    private static void TraceAsyncPerf(string boundary, int result)
    {
        Calls.Add(new { Operation = "AsyncPerfTimeline", Boundary = boundary, Timestamp = Stopwatch.GetTimestamp(), Frequency = Stopwatch.Frequency, HResult = result });
    }

    private static void ObserveAsyncPerf(nint window, string scenario, uint flags, int expected)
    {
        int count = 0;
        bool payload = true;
        Message message = default;
        var elapsed = Stopwatch.StartNew();
        // Observe the complete bounded interval, including unexpected late/duplicate signals.
        while (elapsed.ElapsedMilliseconds < 500)
        {
            while (PeekMessageW(&message, window, 0x400, 0x400, 1) != 0)
            {
                count++;
                Calls.Add(new { Operation = "AsyncPerfMessage", Scenario = scenario, Timestamp = Stopwatch.GetTimestamp(), QueueTime = message.Time, WParam = (ulong)message.WParam, LParam = (long)message.LParam });
                payload &= message.WParam == 123 && message.LParam == 456;
                if (asyncPreviousProc != 0) DispatchMessageW(&message);
            }
            Thread.Sleep(1);
        }
        // A successful submitted frame must be acknowledged; native batch snapshots
        // may split commit and flush into separate presents. A flush alone is not a frame request.
        bool passed = payload && (expected > 0 ? count >= expected : expected == 0 ? count == 0 : true);
        asyncPerfPassed &= passed;
        Calls.Add(new { Operation = "AsyncPerfPresent", Scenario = scenario, Flags = flags,
            Connection = "CrossThread", PartitionClock = "default", NotificationFlags = 0,
            Count = count, PayloadValid = payload, ExpectedCount = expected,
            RawExactCountMatch = expected >= 0 ? (bool?)(count == expected) : null,
            Confirmation = expected > 0 ? "AtLeastOnePresentedFrameAfterSuccessfulSubmitAndFlush" : expected == 0 ? "NoPresent" : "FlushOnlyRawPresentObservation",
            ObservationMilliseconds = 500,
            Passed = passed });
    }
}
