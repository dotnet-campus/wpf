using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class PresentNotificationContractYTests
{
    private sealed record Sample(string Stage, string[] Messages);

    /// <summary>Compares independently collected official and candidate notification queues at explicit composition boundaries.</summary>
    [DataTestMethod]
    [DataRow(123UL, 123UL)]
    [DataRow(0UL, 0UL)]
    [DataRow(0UL, 123UL)]
    [DataRow(123UL, 456UL)]
    [DataRow(456UL, 123UL)]
    [DataRow(456UL, 789UL)]
    public void WhenTargetRequestsPresentThenSameRunOfficialQueuesDefineContract(ulong first, ulong second)
    {
        string root = Path.Combine(AcceptancePaths.Results, "present-contract-y", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var original = Observe(AcceptancePaths.OriginalDll, "official", root, first, second);
        Assert.IsTrue(original.Single(s => s.Stage == "compose").Messages.Any(IsActualPresent),
            "Official actual Present notification trigger not covered: " + root);
        var candidate = Observe(AcceptancePaths.AotDll, "candidate", root, first, second);
        File.WriteAllText(Path.Combine(root, "comparison.json"), JsonSerializer.Serialize(new
        {
            Threshold = 0, First = first, Second = second, Original = original, Candidate = candidate,
            Scope = "CrossThread HWND Present; all 64 response bytes compared including time; no hardware VSync or late-callback proof"
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.AreEqual(2, original.Single(s => s.Stage == "compose").Messages.Count(IsActualPresent), root);
        CollectionAssert.AreEqual(original.Select(s => JsonSerializer.Serialize(s)).ToArray(),
            candidate.Select(s => JsonSerializer.Serialize(s)).ToArray(), root);
    }

    private static bool IsActualPresent(string hex)
    {
        byte[] bytes = Convert.FromHexString(hex);
        return BinaryPrimitives.ReadUInt32LittleEndian(bytes) == 10 && BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(8)) != 1;
    }

    private static List<Sample> Observe(string path, string role, string root, ulong first, ulong second)
    {
        string directory = Path.Combine(root, role);
        Directory.CreateDirectory(directory);
        nint module = NativeLibrary.Load(path);
        nint Export(string name) => NativeLibrary.GetExport(module, name);
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var create = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
        var notify = (delegate* unmanaged[Stdcall]<nint, nint, uint, int>)Export("MilChannel_SetNotificationWindow");
        var flags = (delegate* unmanaged[Stdcall]<uint, void>)Export("SetMilPerfInstrumentationFlags");
        var peek = (delegate* unmanaged[Stdcall]<nint, void*, nuint, int*, int>)Export("MilComposition_PeekNextMessage");
        var events = new List<string>();
        var samples = new List<Sample>();
        var packets = new List<string>();
        nint connection = 0;
        nint* channelStorage = stackalloc nint[1];
        channelStorage[0] = 0;
        nint window = CreateWindowExW(0, "STATIC", "Present contract Y", 0x10000000, 0, 0, 32, 16, 0, 0, 0, 0);
        void Check(string operation, int hr)
        {
            events.Add($"{operation}:0x{hr:X8}");
            Marshal.ThrowExceptionForHR(hr);
        }
        void Send(byte[] packet)
        {
            packets.Add(Convert.ToHexString(packet));
            fixed (byte* p = packet) Check("send", send(p, (uint)packet.Length, 0, channelStorage[0]));
        }
        void Drain(string stage, int milliseconds = 0)
        {
            var messages = new List<string>();
            var elapsed = Stopwatch.StartNew();
            int presentSignals = 0;
            Message message;
            do
            {
                while (PeekMessageW(&message, window, 0, 0, 1) != 0)
                {
                    events.Add($"window:{stage}:0x{message.Id:X}:{message.WParam}:{message.LParam}");
                    if (message.Id == 0x400 && message.WParam == 123 && message.LParam == 456) presentSignals++;
                    DispatchMessageW(&message);
                }
                if (milliseconds > 0) Thread.Sleep(1);
            } while (elapsed.ElapsedMilliseconds < milliseconds);
            events.Add($"present-signals:{stage}:{presentSignals}");
            byte* data = stackalloc byte[64];
            for (int i = 0; i < 64; i++)
            {
                new Span<byte>(data, 64).Clear();
                int retrieved = 0;
                Check("peek:" + stage, peek(channelStorage[0], data, 64, &retrieved));
                if (retrieved == 0)
                {
                    samples.Add(new(stage, messages.ToArray()));
                    if (stage == "compose" && presentSignals == 0)
                        throw new InvalidOperationException("No actual Present instrumentation signal: " + directory);
                    return;
                }
                string hex = Convert.ToHexString(new ReadOnlySpan<byte>(data, 64));
                messages.Add(hex);
                File.WriteAllBytes(Path.Combine(directory, $"{stage}-{i}.bin"), new ReadOnlySpan<byte>(data, 64).ToArray());
            }
            throw new InvalidOperationException("Notification queue did not converge");
        }
        try
        {
            if (window == 0) throw new System.ComponentModel.Win32Exception();
            var actual = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module);
            File.WriteAllText(Path.Combine(directory, "identity.json"), JsonSerializer.Serialize(new
            {
                Requested = path, Loaded = actual.FileName,
                Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual.FileName))),
                Version = actual.FileVersionInfo.FileVersion, Runtime = RuntimeInformation.FrameworkDescription,
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(), PointerSize = IntPtr.Size,
                Backend = "UCE CrossThread channel + real STATIC HWND; actual Present requires WM_USER instrumentation in events; no HW/VSync or pixel-content claim"
            }, new JsonSerializerOptions { WriteIndented = true }));
            Check("initialize", initialize(0));
            Check("connect", connect(0, &connection));
            Check("channel", create(connection, 0, channelStorage));
            uint marshalType = 0;
            Check("marshal", ((delegate* unmanaged[Stdcall]<nint, uint*, int>)Export("MilChannel_GetMarshalType"))(channelStorage[0], &marshalType));
            if (marshalType != 2) throw new InvalidOperationException("Expected CrossThread channel");
            Check("register-notification", notify(channelStorage[0], window, 0x8123));
            flags(2);
            uint target = 0;
            Check("target", resource(channelStorage[0], 46, &target));
            byte[] targetPacket = new byte[92];
            BinaryPrimitives.WriteUInt32LittleEndian(targetPacket, 0x31);
            BinaryPrimitives.WriteUInt32LittleEndian(targetPacket.AsSpan(4), target);
            BinaryPrimitives.WriteUInt64LittleEndian(targetPacket.AsSpan(8), (ulong)window);
            BinaryPrimitives.WriteUInt32LittleEndian(targetPacket.AsSpan(32), 32);
            BinaryPrimitives.WriteUInt32LittleEndian(targetPacket.AsSpan(36), 16);
            BinaryPrimitives.WriteSingleLittleEndian(targetPacket.AsSpan(40), 1);
            BinaryPrimitives.WriteDoubleLittleEndian(targetPacket.AsSpan(76), 96);
            BinaryPrimitives.WriteDoubleLittleEndian(targetPacket.AsSpan(84), 96);
            Send(targetPacket);
            byte[] settings = new byte[72];
            BinaryPrimitives.WriteUInt32LittleEndian(settings, 0x33);
            BinaryPrimitives.WriteUInt32LittleEndian(settings.AsSpan(4), target);
            BinaryPrimitives.WriteInt32LittleEndian(settings.AsSpan(16), 32);
            BinaryPrimitives.WriteInt32LittleEndian(settings.AsSpan(20), 16);
            BinaryPrimitives.WriteSingleLittleEndian(settings.AsSpan(32), 1);
            BinaryPrimitives.WriteInt32LittleEndian(settings.AsSpan(44), 1);
            Send(settings);
            uint visual = 0;
            Check("visual", resource(channelStorage[0], 39, &visual));
            byte[] binding = new byte[12];
            BinaryPrimitives.WriteUInt32LittleEndian(binding, 0x35);
            BinaryPrimitives.WriteUInt32LittleEndian(binding.AsSpan(4), target);
            BinaryPrimitives.WriteUInt32LittleEndian(binding.AsSpan(8), visual);
            Send(binding);
            foreach (ulong time in new[] { first, second })
            {
                byte[] request = new byte[12];
                BinaryPrimitives.WriteUInt32LittleEndian(request, 6);
                BinaryPrimitives.WriteUInt64LittleEndian(request.AsSpan(4), time);
                Send(request);
            }
            Drain("before-close");
            Check("close", close(channelStorage[0]));
            Check("commit", commit(channelStorage[0]));
            Check("flush", flush(channelStorage[0]));
            Drain("compose", 1500);
            Check("flush-again", flush(channelStorage[0]));
            Drain("compose-again", 300);
            Check("unregister-notification", notify(channelStorage[0], 0, 0));
            byte[] pending = new byte[12];
            BinaryPrimitives.WriteUInt32LittleEndian(pending, 6);
            BinaryPrimitives.WriteUInt64LittleEndian(pending.AsSpan(4), first);
            Send(pending);
            Check("close-pending", close(channelStorage[0]));
            Check("commit-pending", commit(channelStorage[0]));
            Check("destroy-pending", destroy(channelStorage[0]));
            channelStorage[0] = 0;
            Check("replacement-channel", create(connection, 0, channelStorage));
            Check("flush-after-destroy", flush(channelStorage[0]));
            Drain("replacement-channel", 300);
            return samples;
        }
        finally
        {
            flags(0);
            if (channelStorage[0] != 0) Check("destroy", destroy(channelStorage[0]));
            if (connection != 0) Check("disconnect", ((delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect"))(connection));
            Check("deinitialize", ((delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager"))());
            if (window != 0) DestroyWindow(window);
            File.WriteAllText(Path.Combine(directory, "events.json"), JsonSerializer.Serialize(events));
            File.WriteAllText(Path.Combine(directory, "packets.json"), JsonSerializer.Serialize(packets));
            File.WriteAllText(Path.Combine(directory, "samples.json"), JsonSerializer.Serialize(samples));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Message
    {
        public nint Window;
        public uint Id;
        public nuint WParam;
        public nint LParam;
        public uint Time;
        public int X, Y;
        public uint Private;
    }

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int PeekMessageW(Message* message, nint window, uint minimum, uint maximum, uint remove);
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern nint DispatchMessageW(Message* message);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern nint CreateWindowExW(uint ex, string cls, string title, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int DestroyWindow(nint window);
}
