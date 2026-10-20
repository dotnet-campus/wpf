namespace NativeAcceptance.PublicAbi;
using System.Runtime.InteropServices;

internal static unsafe partial class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern nint CreateWindowExW(uint ex, string cls, string title, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int DestroyWindow(nint window);
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int PeekMessageW(Message* message, nint window, uint first, uint last, uint remove);
    [StructLayout(LayoutKind.Sequential)]
    private struct Message
    {
        internal nint Window;
        internal uint Id;
        internal nuint WParam;
        internal nint LParam;
        internal uint Time;
        internal int X, Y;
        internal uint Private;
    }

    private static void PerfPresentContract()
    {
        nint window = CreateWindowExW(8, "STATIC", "Perf ABI", 0x90000000, 0, 0, 32, 16, 0, 0, 0, 0);
        nint connection = 0, channel = 0;
        var flags = (delegate* unmanaged[Stdcall]<uint, void>)Export("SetMilPerfInstrumentationFlags");
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        try
        {
            if (window == 0) throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
            Check("Perf.Initialize", initialize(0));
            Check("Perf.Connect", ((delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create"))(1, &connection));
            Check("Perf.Channel", ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel"))(connection, 0, &channel));
            uint target = 0, visual = 0;
            Check("Perf.Target", resource(channel, 46, &target));
            Check("Perf.Visual", resource(channel, 39, &visual));
            byte* packet = stackalloc byte[92];
            new Span<byte>(packet, 92).Clear();
            *(uint*)packet = 0x31; *(uint*)(packet + 4) = target;
            *(ulong*)(packet + 8) = (ulong)window;
            *(uint*)(packet + 32) = 32; *(uint*)(packet + 36) = 16;
            *(float*)(packet + 40) = 1;
            Check("Perf.CreateTarget", send(packet, 92, 0, channel));
            uint* binding = stackalloc uint[3] { 0x35, target, visual };
            Check("Perf.Bind", send(binding, 12, 0, channel));
            foreach (uint value in new uint[] { 0, 2, 0 })
            {
                Message message = default;
                while (PeekMessageW(&message, window, 0x400, 0x400, 1) != 0) { }
                flags(value);
                binding[2] = 0;
                Check("Perf.Unbind", send(binding, 12, 0, channel));
                binding[2] = visual;
                Check("Perf.Rebind", send(binding, 12, 0, channel));
                Check("Perf.Close", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch"))(channel));
                Check("Perf.Commit", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel"))(channel));
                int hr = ((delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_SameThreadPresent"))(connection);
                int count = 0;
                bool payload = true;
                while (PeekMessageW(&message, window, 0x400, 0x400, 1) != 0)
                { count++; payload &= message.WParam == 123 && message.LParam == 456; }
                // Native same-thread PresentAllPartitions composes but never calls the HWND Present method.
                Calls.Add(new { Operation = "PerfPresent", Flags = value, HResult = hr, Count = count, PayloadValid = payload,
                    NativeBranch = "CConnectionContext::PresentAllPartitions/Compose", ExpectedCount = 0,
                    Passed = hr >= 0 && payload && count == 0 });
                if (hr < 0 || !payload || count != 0) throw new InvalidOperationException("Perf.SynchronousComposeUnexpectedPresentSignal");
            }
        }
        finally
        {
            flags(0);
            if (channel != 0) Check("Perf.DestroyChannel", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel"))(channel));
            if (connection != 0) Check("Perf.Disconnect", ((delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect"))(connection));
            Check("Perf.Deinitialize", ((delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager"))());
            if (window != 0) DestroyWindow(window);
        }
    }
}
