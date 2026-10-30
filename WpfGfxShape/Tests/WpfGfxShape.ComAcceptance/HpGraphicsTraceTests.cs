#if HP_GRAPHICS_TRACE
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed partial class HpGraphicsTraceTests
{
    private const int Limit = 16 * 1024 * 1024;
    private static readonly string[] Terms = ["Present", "Occl", "Flip", "VSync", "Hwnd", "Token", "Blit"];

    /// <summary>Reads installed graphics manifests and access descriptors without enabling providers or rendering.</summary>
    [TestMethod]
    public void WhenGraphicsMetadataIsQueriedThenActualApiStatusIsRecorded()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "hp-graphics-trace");
        Directory.CreateDirectory(root);
        using var output = new StreamWriter(Path.Combine(root, "probe.jsonl"), false);
        void Log(object value) { output.WriteLine(JsonSerializer.Serialize(value)); output.Flush(); }
        Assembly assembly = typeof(HpGraphicsTraceTests).Assembly;
        Log(new { kind = "identity", utc = DateTime.UtcNow, qpc = Stopwatch.GetTimestamp(), frequency = Stopwatch.Frequency,
            pid = Environment.ProcessId, session = Process.GetCurrentProcess().SessionId,
            runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            assembly = assembly.Location, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))),
            mvid = assembly.ManifestModule.ModuleVersionId, os = Environment.OSVersion.VersionString,
            nativeDependency = "packaging only; not loaded by HP" });
        string[] manifest = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "package-files.txt"));
        string[] actual = Directory.GetFiles(AppContext.BaseDirectory, "*", SearchOption.AllDirectories)
            .Select(p => Path.GetRelativePath(AppContext.BaseDirectory, p).Replace('\\', '/'))
            .Where(p => !p.StartsWith("Results/", StringComparison.OrdinalIgnoreCase) && !p.StartsWith("TestResults/", StringComparison.OrdinalIgnoreCase)).Order(StringComparer.Ordinal).ToArray();
        Log(new { kind = "package", expected = manifest.Length, actual = actual.Length,
            missing = manifest.Except(actual).ToArray(), extra = actual.Except(manifest).ToArray(),
            nativeSha = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "aot.sha256")).Trim(),
            runtimeconfig = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "WpfGfxShape.ComAcceptance.runtimeconfig.json")) });
        CollectionAssert.AreEqual(manifest.Order(StringComparer.Ordinal).ToArray(), actual);
        Assert.AreEqual(16, Marshal.SizeOf<Descriptor>());
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        Log(new { kind = "token", administratorEnabled = new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator), impersonation = identity.ImpersonationLevel.ToString() });
        uint tokenSize = 0;
        GetTokenInformation(identity.AccessToken.DangerousGetHandle(), 3, IntPtr.Zero, 0, out tokenSize);
        int tokenSizingError = Marshal.GetLastWin32Error();
        if (tokenSize > 0 && tokenSize <= Limit)
        {
            IntPtr token = Marshal.AllocHGlobal((int)tokenSize);
            try
            {
                bool ok = GetTokenInformation(identity.AccessToken.DangerousGetHandle(), 3, token, tokenSize, out tokenSize);
                Log(new { kind = "tokenPrivileges", ok, error = ok ? 0 : Marshal.GetLastWin32Error(), sizingError = tokenSizingError });
                if (ok)
                    for (int i = 0, count = Marshal.ReadInt32(token); i < count; i++)
                    {
                        int offset = checked(4 + i * 12);
                        if (offset + 12 > tokenSize) throw new InvalidDataException("TOKEN_PRIVILEGES bounds");
                        long luid = Marshal.ReadInt64(token, offset);
                        uint length = 256;
                        var name = new char[length];
                        bool named = LookupPrivilegeNameW(null, ref luid, name, ref length);
                        Log(new { kind = "privilege", luid, named, name = named ? new string(name, 0, (int)length) : "", attributes = Marshal.ReadInt32(token, offset + 8), error = named ? 0 : Marshal.GetLastWin32Error() });
                    }
            }
            finally { Marshal.FreeHGlobal(token); }
        }
        uint bytes = 0;
        uint status = TdhEnumerateProviders(IntPtr.Zero, ref bytes);
        Log(new { kind = "providersSizing", status, bytes });
        Assert.AreEqual(122u, status);
        IntPtr providers = Allocate(bytes);
        int found = 0;
        try
        {
            status = TdhEnumerateProviders(providers, ref bytes);
            Log(new { kind = "providersRead", status, bytes });
            Assert.AreEqual(0u, status);
            int count = Marshal.ReadInt32(providers);
            for (int i = 0; i < count; i++)
            {
                int offset = checked(8 + i * 24);
                Check(bytes, offset, 24);
                string name = Text(providers, bytes, Marshal.ReadInt32(providers, offset + 20));
                if (!name.Equals("Microsoft-Windows-Dwm-Core", StringComparison.OrdinalIgnoreCase) &&
                    !name.Equals("Microsoft-Windows-DxgKrnl", StringComparison.OrdinalIgnoreCase)) continue;
                found++;
                Guid guid = Marshal.PtrToStructure<Guid>(providers + offset);
                Log(new { kind = "provider", name, guid, schemaSource = Marshal.ReadInt32(providers, offset + 16) });
                Security(guid, Log);
                Fields(guid, 0, Log);
                Fields(guid, 1, Log);
                Events(guid, Log);
            }
            Log(new { kind = "cleanup", providersFound = found, sessionsCreated = 0, sessionsStopped = 0, traceHandlesOpened = 0,
                buffersFreedByFinally = true, renderingStarted = false, eventConsumption = "not attempted", permissionToStart = "not tested; descriptor/token are not StartTrace/EnableTrace proof" });
        }
        finally { Marshal.FreeHGlobal(providers); }
        Assert.AreEqual(2, found);
    }

    private static void Security(Guid guid, Action<object> log)
    {
        uint bytes = 0;
        uint status = EventAccessQuery(ref guid, IntPtr.Zero, ref bytes);
        log(new { kind = "securitySizing", guid, status, bytes });
        if (status is not (122 or 234) || bytes == 0) return;
        IntPtr data = Allocate(bytes);
        try
        {
            status = EventAccessQuery(ref guid, data, ref bytes);
            string? sddl = null;
            if (status == 0)
            {
                byte[] raw = new byte[bytes];
                Marshal.Copy(data, raw, 0, raw.Length);
                sddl = new RawSecurityDescriptor(raw, 0).GetSddlForm(AccessControlSections.All);
            }
            log(new { kind = "security", guid, status, bytes, sddl });
        }
        finally { Marshal.FreeHGlobal(data); }
    }

    private static void Fields(Guid guid, uint type, Action<object> log)
    {
        uint bytes = 0;
        uint status = TdhEnumerateProviderFieldInformation(ref guid, type, IntPtr.Zero, ref bytes);
        log(new { kind = "fieldSizing", guid, type, status, bytes });
        if (status != 122 || bytes == 0) return;
        IntPtr data = Allocate(bytes);
        try
        {
            status = TdhEnumerateProviderFieldInformation(ref guid, type, data, ref bytes);
            log(new { kind = "fieldRead", guid, type, status, bytes });
            if (status != 0) return;
            for (int i = 0, count = Marshal.ReadInt32(data); i < count; i++)
            {
                int offset = checked(8 + i * 16);
                Check(bytes, offset, 16);
                log(new { kind = "field", guid, type, name = Text(data, bytes, Marshal.ReadInt32(data, offset)),
                    description = Text(data, bytes, Marshal.ReadInt32(data, offset + 4)), value = unchecked((ulong)Marshal.ReadInt64(data, offset + 8)).ToString("X16") });
            }
        }
        finally { Marshal.FreeHGlobal(data); }
    }

    private static void Events(Guid guid, Action<object> log)
    {
        uint bytes = 0;
        uint status = TdhEnumerateManifestProviderEvents(ref guid, IntPtr.Zero, ref bytes);
        log(new { kind = "eventsSizing", guid, status, bytes });
        if (status != 122 || bytes == 0) return;
        IntPtr events = Allocate(bytes);
        try
        {
            status = TdhEnumerateManifestProviderEvents(ref guid, events, ref bytes);
            log(new { kind = "eventsRead", guid, status, bytes });
            if (status != 0) return;
            int count = Marshal.ReadInt32(events), decoded = 0, selected = 0, emitted = 0, errors = 0;
            if (count > 4096) throw new InvalidDataException("Manifest descriptor limit");
            for (int i = 0; i < count; i++)
            {
                int offset = checked(8 + i * 16);
                Check(bytes, offset, 16);
                Descriptor descriptor = Marshal.PtrToStructure<Descriptor>(events + offset);
                uint size = 0;
                uint sizing = TdhGetManifestEventInformation(ref guid, ref descriptor, IntPtr.Zero, ref size);
                if (sizing != 122) { errors++; log(new { kind = "metadataError", guid, descriptor.Id, descriptor.Version, sizing, size }); continue; }
                IntPtr data = Allocate(size);
                try
                {
                    uint result = TdhGetManifestEventInformation(ref guid, ref descriptor, data, ref size);
                    if (result != 0) { errors++; log(new { kind = "metadataError", guid, descriptor.Id, descriptor.Version, result, size }); continue; }
                    Check(size, 0, 112);
                    decoded++;
                    string task = Text(data, size, Marshal.ReadInt32(data, 68));
                    string opcode = Text(data, size, Marshal.ReadInt32(data, 72));
                    int propertyCount = Marshal.ReadInt32(data, 100);
                    var properties = new List<object>();
                    var names = new List<string>();
                    for (int p = 0; p < propertyCount; p++)
                    {
                        int po = checked(112 + p * 24);
                        Check(size, po, 24);
                        string name = Text(data, size, Marshal.ReadInt32(data, po + 4));
                        names.Add(name);
                        properties.Add(new { name, flags = Marshal.ReadInt32(data, po), union0 = (ushort)Marshal.ReadInt16(data, po + 8), union1 = (ushort)Marshal.ReadInt16(data, po + 10), countOrIndex = (ushort)Marshal.ReadInt16(data, po + 16), lengthOrIndex = (ushort)Marshal.ReadInt16(data, po + 18) });
                    }
                    if (!Terms.Any(term => (task + " " + opcode + " " + string.Join(' ', names)).Contains(term, StringComparison.OrdinalIgnoreCase))) continue;
                    selected++;
                    if (emitted >= 60) continue;
                    emitted++;
                    log(new { kind = "eventMetadata", guid, id = descriptor.Id, version = descriptor.Version, channel = descriptor.Channel,
                        level = descriptor.Level, opcodeValue = descriptor.Opcode, taskValue = descriptor.Task, keyword = descriptor.Keyword.ToString("X16"),
                        task, opcode, properties, topLevel = Marshal.ReadInt32(data, 104), flags = Marshal.ReadInt32(data, 108),
                        source = "TdhGetManifestEventInformation; schema only, not observed event" });
                }
                finally { Marshal.FreeHGlobal(data); }
            }
            log(new { kind = "metadataSummary", guid, count, decoded, selected, emitted, omitted = selected - emitted, errors, maxBytesPerBuffer = Limit });
        }
        finally { Marshal.FreeHGlobal(events); }
    }

    private static IntPtr Allocate(uint bytes)
    {
        if (bytes == 0 || bytes > Limit) throw new InvalidDataException("TDH buffer limit");
        return Marshal.AllocHGlobal(checked((int)bytes));
    }

    private static void Check(uint bytes, int offset, int length)
    {
        if (offset < 0 || length < 0 || (long)offset + length > bytes) throw new InvalidDataException("TDH bounds");
    }

    private static string Text(IntPtr data, uint bytes, int offset)
    {
        if (offset == 0) return "";
        Check(bytes, offset, 2);
        for (int end = offset; (long)end + 2 <= bytes; end += 2)
            if (Marshal.ReadInt16(data, end) == 0) return Marshal.PtrToStringUni(data + offset, (end - offset) / 2) ?? "";
        throw new InvalidDataException("TDH UTF16 terminator");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Descriptor
    {
        internal ushort Id;
        internal byte Version, Channel, Level, Opcode;
        internal ushort Task;
        internal ulong Keyword;
    }

    [DllImport("tdh.dll", ExactSpelling = true)]
    private static extern uint TdhEnumerateProviders(IntPtr buffer, ref uint size);
    [DllImport("tdh.dll", ExactSpelling = true)]
    private static extern uint TdhEnumerateProviderFieldInformation(ref Guid provider, uint type, IntPtr buffer, ref uint size);
    [DllImport("tdh.dll", ExactSpelling = true)]
    private static extern uint TdhEnumerateManifestProviderEvents(ref Guid provider, IntPtr buffer, ref uint size);
    [DllImport("tdh.dll", ExactSpelling = true)]
    private static extern uint TdhGetManifestEventInformation(ref Guid provider, ref Descriptor descriptor, IntPtr buffer, ref uint size);
    [DllImport("advapi32.dll", ExactSpelling = true)]
    private static extern uint EventAccessQuery(ref Guid provider, IntPtr descriptor, ref uint size);
    [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetTokenInformation(IntPtr token, int informationClass, IntPtr information, uint length, out uint returned);
    [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LookupPrivilegeNameW(string? system, ref long luid, [Out] char[] name, ref uint length);
}
#endif
