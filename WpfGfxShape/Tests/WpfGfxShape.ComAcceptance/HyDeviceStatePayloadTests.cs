using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class HyDeviceStatePayloadTests
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 64)]
    private struct Message
    {
        internal uint Type;
        internal uint Reserved;
        internal uint OldState;
        internal uint NewState;
    }

    /// <summary>Checks an unrendered partition against an independent NoDevice contract in isolated repeated roles.</summary>
    [TestMethod]
    [Timeout(110000)]
    public async Task WhenHyDeviceStateGroupRunsThenUnrenderedSnapshotsContainNoDeviceAsync()
    {
        var exits = new List<int>();
        foreach (string role in new[] { "official", "candidate", "official-repeat" })
        {
            string root = Path.Combine(AcceptancePaths.Results, "hy-payload", role);
            Directory.CreateDirectory(root);
            var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException()) { UseShellExecute = false, WorkingDirectory = AppContext.BaseDirectory };
            foreach (string argument in new[] { "--filter", "FullyQualifiedName~WhenHyDeviceStateRoleRunsThenSnapshotsMatchIndependentLayout", "--report-trx", "--report-trx-filename", "role.trx", "--results-directory", root, "--no-progress" }) start.ArgumentList.Add(argument);
            start.Environment["HY_PAYLOAD_ROLE"] = role;
            start.Environment["HY_PAYLOAD_OUTPUT"] = root;
            using var process = Process.Start(start) ?? throw new InvalidOperationException();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { process.Kill(true); await process.WaitForExitAsync(); throw; }
            exits.Add(process.ExitCode);
            File.WriteAllText(Path.Combine(root, "exit.json"), JsonSerializer.Serialize(new { role, pid = process.Id, exitCode = process.ExitCode }, new JsonSerializerOptions { WriteIndented = true }));
        }
        CollectionAssert.AreEqual(new[] { 0, 0, 0 }, exits);
    }

    /// <summary>Captures complete messages before validating independently declared wire fields and cleanup.</summary>
    [TestMethod]
    [Timeout(20000)]
    public unsafe void WhenHyDeviceStateRoleRunsThenSnapshotsMatchIndependentLayout()
    {
        string role = Environment.GetEnvironmentVariable("HY_PAYLOAD_ROLE") ?? "candidate";
        string root = Environment.GetEnvironmentVariable("HY_PAYLOAD_OUTPUT") ?? Path.Combine(AcceptancePaths.Results, "hy-payload-direct", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string path = role == "candidate" ? AcceptancePaths.AotDll : AcceptancePaths.OriginalDll;
        nint module = NativeLibrary.Load(path);
        nint Export(string name) => NativeLibrary.GetExport(module, name);
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager");
        var deinitialize = (delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
        var create = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
        var peek = (delegate* unmanaged[Stdcall]<nint, void*, nuint, int*, int>)Export("MilComposition_PeekNextMessage");
        var samples = new List<byte[]>();
        var counts = new List<int>();
        var events = new List<object>();
        void Check(string stage, int hr) { events.Add(new { stage, hr }); Marshal.ThrowExceptionForHR(hr); }
        nint connection = 0;
        nint* channel = stackalloc nint[1];
        channel[0] = 0;
        bool initialized = false, cleaned = false;
        void Submit(uint enable)
        {
            uint* packet = stackalloc uint[] { 3, enable };
            Check("send-" + enable, send(packet, 8, 0, channel[0]));
            Check("flush", flush(channel[0]));
            int count = 0;
            byte* bytes = stackalloc byte[64];
            for (int i = 0; i < 16; i++)
            {
                new Span<byte>(bytes, 64).Clear();
                int retrieved = 0;
                Check("peek", peek(channel[0], bytes, 64, &retrieved));
                if (retrieved == 0) { counts.Add(count); return; }
                Assert.AreEqual(1, retrieved);
                samples.Add(new ReadOnlySpan<byte>(bytes, 64).ToArray());
                count++;
            }
            throw new InvalidOperationException();
        }
        object Identity(string file) => new { path = Path.GetFullPath(file), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))), version = FileVersionInfo.GetVersionInfo(file).FileVersion };
        try
        {
            Check("initialize", initialize(0)); initialized = true;
            Check("connect", connect(0, &connection));
            Check("create", create(connection, 0, channel));
            Submit(1);
            Submit(1);
            Submit(0);
            Submit(1);
            Submit(0);
        }
        finally
        {
            try
            {
                if (channel[0] != 0) Check("destroy", destroy(channel[0]));
                if (connection != 0) Check("disconnect", disconnect(connection));
                if (initialized) Check("deinitialize", deinitialize());
                cleaned = true;
            }
            finally
            {
                using var process = Process.GetCurrentProcess();
                string loaded = process.Modules.Cast<ProcessModule>().Single(item => item.BaseAddress == module).FileName;
                File.WriteAllText(Path.Combine(root, "business.json"), JsonSerializer.Serialize(new
                {
                    role, pid = Environment.ProcessId, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size,
                    loaded = Identity(loaded), test = Identity(typeof(HyDeviceStatePayloadTests).Assembly.Location), mvid = typeof(HyDeviceStatePayloadTests).Module.ModuleVersionId,
                    layoutSize = Marshal.SizeOf<Message>(), oldOffset = Marshal.OffsetOf<Message>(nameof(Message.OldState)).ToInt32(), newOffset = Marshal.OffsetOf<Message>(nameof(Message.NewState)).ToInt32(),
                    counts, samples = samples.Select(bytes => new { hex = Convert.ToHexString(bytes), type = BinaryPrimitives.ReadUInt32LittleEndian(bytes), oldState = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(8)), newState = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(12)) }), events, cleaned
                }, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        Assert.AreEqual(64, Marshal.SizeOf<Message>());
        Assert.AreEqual(8, Marshal.OffsetOf<Message>(nameof(Message.OldState)).ToInt32());
        Assert.AreEqual(12, Marshal.OffsetOf<Message>(nameof(Message.NewState)).ToInt32());
        CollectionAssert.AreEqual(new[] { 1, 0, 0, 1, 0 }, counts);
        Assert.AreEqual(2, samples.Count);
        // No render target or rendering command has been submitted: the partition has never acquired a device.
        byte[] expected = new byte[64];
        BinaryPrimitives.WriteUInt32LittleEndian(expected, 5);
        BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(8), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(12), 1);
        foreach (byte[] bytes in samples) CollectionAssert.AreEqual(expected, bytes);
        Assert.IsTrue(cleaned);
    }
}
