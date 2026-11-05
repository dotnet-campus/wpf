using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed unsafe class AudioEndpointCapabilityTests
{
    private readonly List<string> evidence = [];
    private static nint* V(nint p) => *(nint**)p;
    private int Hr(string name, int hr) { evidence.Add($"{name}:0x{hr:X8}"); return hr; }
    private void Win(string name, bool ok) => evidence.Add($"{name}:ok={ok};error={(ok ? 0 : Marshal.GetLastPInvokeError())}");
    private void Release(string name, nint p)
    {
        if (p != 0) evidence.Add($"{name}.Release:{((delegate* unmanaged[Stdcall]<nint, uint>)V(p)[2])(p)}");
    }

    /// <summary>Observes endpoint and service capability only; never renders or modifies system state.</summary>
    [TestMethod]
    public Task WhenEndpointCapabilityIsObservedThenRawStatesAndServiceStatusAreRecorded()
    {
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Thread owner = new(() =>
        {
            try { Observe(); completion.SetResult(); }
            catch (Exception error) { completion.SetException(error); }
        });
        owner.SetApartmentState(ApartmentState.MTA);
        owner.Start();
        return completion.Task;
    }

    private void Observe()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "Results", "audio-endpoint-capability");
        Directory.CreateDirectory(directory);
        nint enumerator = 0;
        bool com = false, enumerated = false;
        try
        {
            int hr = Hr("CoInitializeEx(MTA)", CoInitializeEx(0, 0));
            com = hr >= 0;
            if (com)
            {
                Guid clsid = new("bcde0395-e52f-467c-8e3d-c4579291692e"), iid = new("a95664d2-9614-4f35-a746-de8db63617e6");
                hr = Hr("CoCreateInstance(IMMDeviceEnumerator)", CoCreateInstance(&clsid, 0, 1, &iid, &enumerator));
                if (hr >= 0 && enumerator != 0)
                {
                    for (int flow = 0; flow < 2; flow++)
                    {
                        foreach (uint state in new uint[] { 1, 2, 4, 8 }) Enumerate(enumerator, flow, state);
                        for (int role = 0; role < 3; role++)
                        {
                            nint device = 0;
                            try
                            {
                                hr = Hr($"Default.flow{flow}.role{role}", ((delegate* unmanaged[Stdcall]<nint, int, int, nint*, int>)V(enumerator)[4])(enumerator, flow, role, &device));
                                if (hr >= 0 && device != 0) Device(device, $"Default.flow{flow}.role{role}");
                            }
                            finally { Release("DefaultDevice", device); }
                        }
                    }
                    enumerated = true;
                }
            }
            Services();
            Token();
        }
        finally
        {
            Release("enumerator", enumerator);
            if (com) { CoUninitialize(); evidence.Add("CoUninitialize:void"); }
            using Process process = Process.GetCurrentProcess();
            var modules = process.Modules.Cast<ProcessModule>()
                .Where(m => m.ModuleName.Equals("MMDevApi.dll", StringComparison.OrdinalIgnoreCase) || m.ModuleName.Equals("audioclient.dll", StringComparison.OrdinalIgnoreCase) || m.ModuleName.Equals("AudioSes.dll", StringComparison.OrdinalIgnoreCase))
                .Select(m => new { m.ModuleName, m.FileName, m.FileVersionInfo.FileVersion, Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(m.FileName))) }).ToArray();
            string assembly = typeof(AudioEndpointCapabilityTests).Assembly.Location;
            File.WriteAllText(Path.Combine(directory, "capability.json"), JsonSerializer.Serialize(new
            {
                Classification = "ObservationOnly.NotAudioOutputValidation", Enumerated = enumerated,
                Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                TestSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly))),
                Mvid = typeof(AudioEndpointCapabilityTests).Module.ModuleVersionId, Evidence = evidence, Modules = modules,
                OwnedAudioClients = "NotCreated", OwnedAudioBuffers = "NotCreated", OwnedEvents = "NotCreated",
                AudioRendering = "NotExecuted", SystemMutations = "None"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.IsTrue(enumerated, "Endpoint observation did not complete; raw HRESULT evidence retained.");
    }

    private void Enumerate(nint enumerator, int flow, uint state)
    {
        nint collection = 0;
        string label = $"Enum.flow{flow}.state{state}";
        try
        {
            int hr = Hr(label, ((delegate* unmanaged[Stdcall]<nint, int, uint, nint*, int>)V(enumerator)[3])(enumerator, flow, state, &collection));
            if (hr < 0 || collection == 0) return;
            uint count = 0;
            hr = Hr(label + ".GetCount", ((delegate* unmanaged[Stdcall]<nint, uint*, int>)V(collection)[3])(collection, &count));
            if (hr < 0) return;
            evidence.Add($"{label}.Count:{count}");
            for (uint index = 0; index < count; index++)
            {
                nint device = 0;
                try
                {
                    hr = Hr(label + $".Item{index}", ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)V(collection)[4])(collection, index, &device));
                    if (hr >= 0 && device != 0) Device(device, label + $".Item{index}");
                }
                finally { Release(label + ".Device", device); }
            }
        }
        finally { Release(label + ".Collection", collection); }
    }

    private void Device(nint device, string label)
    {
        uint state = 0;
        Hr(label + ".GetState", ((delegate* unmanaged[Stdcall]<nint, uint*, int>)V(device)[6])(device, &state));
        evidence.Add(label + $".State:0x{state:X8}");
        nint id = 0, store = 0;
        try
        {
            int hr = Hr(label + ".GetId", ((delegate* unmanaged[Stdcall]<nint, nint*, int>)V(device)[5])(device, &id));
            if (hr >= 0) evidence.Add(label + ".Id:" + Marshal.PtrToStringUni(id));
            hr = Hr(label + ".OpenPropertyStore(readonly)", ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)V(device)[4])(device, 0, &store));
            if (hr < 0 || store == 0) return;
            PropertyKey key = new() { Format = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), Id = 14 };
            PropVariant value = default;
            try
            {
                hr = Hr(label + ".FriendlyName.GetValue", ((delegate* unmanaged[Stdcall]<nint, PropertyKey*, PropVariant*, int>)V(store)[5])(store, &key, &value));
                evidence.Add(label + $".FriendlyName.vt:{value.Type}");
                if (hr >= 0 && value.Type == 31) evidence.Add(label + ".FriendlyName:" + Marshal.PtrToStringUni(value.Pointer));
            }
            finally { Hr(label + ".PropVariantClear", PropVariantClear(&value)); }
        }
        finally
        {
            Release(label + ".PropertyStore", store);
            if (id != 0) { Marshal.FreeCoTaskMem(id); evidence.Add(label + ".Id.CoTaskMemFree:void"); }
        }
    }

    private void Services()
    {
        nint manager = OpenSCManagerW(null, null, 1);
        Win("OpenSCManager(CONNECT)", manager != 0);
        if (manager == 0) return;
        try
        {
            HashSet<string> visited = new(StringComparer.OrdinalIgnoreCase);
            Service(manager, "AudioSrv", visited);
            Service(manager, "AudioEndpointBuilder", visited);
        }
        finally { Win("SCManager.Close", CloseServiceHandle(manager)); }
    }

    private void Service(nint manager, string name, HashSet<string> visited)
    {
        if (!visited.Add(name)) return;
        nint service = OpenServiceW(manager, name, 5);
        Win(name + ".OpenService(QUERY_CONFIG|QUERY_STATUS)", service != 0);
        if (service == 0) return;
        try
        {
            ServiceStatus status = default;
            uint needed = 0;
            bool ok = QueryServiceStatusEx(service, 0, &status, (uint)sizeof(ServiceStatus), &needed);
            Win(name + ".QueryServiceStatusEx", ok);
            if (ok) evidence.Add($"{name}.Status:type={status.Type};state={status.State};accepted={status.Accepted};win32Exit={status.Win32Exit};specificExit={status.SpecificExit};checkpoint={status.Checkpoint};waitHint={status.WaitHint};pid={status.Pid};flags={status.Flags}");
            bool sizing = QueryServiceConfigW(service, 0, 0, &needed);
            Win(name + ".QueryServiceConfig(size)", sizing);
            evidence.Add(name + $".ConfigBytes:{needed}");
            if (needed == 0 || needed > 1048576) return;
            nint memory = Marshal.AllocHGlobal(checked((int)needed));
            try
            {
                ok = QueryServiceConfigW(service, memory, needed, &needed);
                Win(name + ".QueryServiceConfig", ok);
                if (!ok) return;
                ServiceConfig config = Marshal.PtrToStructure<ServiceConfig>(memory);
                evidence.Add($"{name}.Config:type={config.Type};startType={config.Start};errorControl={config.Error}");
                char* dependency = (char*)config.Dependencies;
                while (dependency != null && *dependency != '\0')
                {
                    string item = new(dependency);
                    evidence.Add(name + ".DependsOn:" + item);
                    if (!item.StartsWith('+')) Service(manager, item, visited);
                    dependency += item.Length + 1;
                }
            }
            finally { Marshal.FreeHGlobal(memory); evidence.Add(name + ".Config.FreeHGlobal:void"); }
        }
        finally { Win(name + ".CloseServiceHandle", CloseServiceHandle(service)); }
    }

    private void Token()
    {
        uint session = 0;
        Win("ProcessIdToSessionId", ProcessIdToSessionId((uint)Environment.ProcessId, &session));
        evidence.Add($"SessionId:{session}");
        nint token = 0;
        bool ok = OpenProcessToken(GetCurrentProcess(), 8, &token);
        Win("OpenProcessToken(QUERY)", ok);
        if (!ok) return;
        try
        {
            foreach (int kind in new[] { 12, 18, 20, 25 })
            {
                uint needed = 0;
                Win($"Token{kind}.Size", GetTokenInformation(token, kind, 0, 0, &needed));
                if (needed == 0 || needed > 1048576) continue;
                nint memory = Marshal.AllocHGlobal(checked((int)needed));
                try
                {
                    ok = GetTokenInformation(token, kind, memory, needed, &needed);
                    Win($"Token{kind}.Get", ok);
                    if (!ok) continue;
                    if (kind != 25) evidence.Add($"Token{kind}.Value:{*(uint*)memory}");
                    else
                    {
                        nint sid = *(nint*)memory;
                        if (IsValidSid(sid))
                        {
                            byte count = *GetSidSubAuthorityCount(sid);
                            if (count > 0) evidence.Add($"TokenIntegrity.Rid:0x{*GetSidSubAuthority(sid, (uint)(count - 1)):X8}");
                        }
                    }
                }
                finally { Marshal.FreeHGlobal(memory); evidence.Add($"Token{kind}.FreeHGlobal:void"); }
            }
        }
        finally { Win("Token.CloseHandle", CloseHandle(token)); }
    }

    [StructLayout(LayoutKind.Sequential)] private struct PropertyKey { internal Guid Format; internal uint Id; }
    [StructLayout(LayoutKind.Explicit, Size = 24)] private struct PropVariant { [FieldOffset(0)] internal ushort Type; [FieldOffset(8)] internal nint Pointer; }
    [StructLayout(LayoutKind.Sequential)] private struct ServiceStatus { internal uint Type, State, Accepted, Win32Exit, SpecificExit, Checkpoint, WaitHint, Pid, Flags; }
    [StructLayout(LayoutKind.Sequential)] private struct ServiceConfig { internal uint Type, Start, Error; internal nint Binary, Group; internal uint Tag; internal nint Dependencies, Account, Display; }
    [DllImport("ole32.dll", ExactSpelling = true)] private static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32.dll", ExactSpelling = true)] private static extern void CoUninitialize();
    [DllImport("ole32.dll", ExactSpelling = true)] private static extern int CoCreateInstance(Guid* clsid, nint outer, uint context, Guid* iid, nint* result);
    [DllImport("ole32.dll", ExactSpelling = true)] private static extern int PropVariantClear(PropVariant* value);
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern nint OpenSCManagerW(string? machine, string? database, uint access);
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern nint OpenServiceW(nint manager, string name, uint access);
    [DllImport("advapi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool QueryServiceStatusEx(nint service, int level, ServiceStatus* status, uint size, uint* needed);
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool QueryServiceConfigW(nint service, nint config, uint size, uint* needed);
    [DllImport("advapi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CloseServiceHandle(nint service);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ProcessIdToSessionId(uint pid, uint* session);
    [DllImport("kernel32.dll")] private static extern nint GetCurrentProcess();
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CloseHandle(nint handle);
    [DllImport("advapi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool OpenProcessToken(nint process, uint access, nint* token);
    [DllImport("advapi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetTokenInformation(nint token, int kind, nint information, uint size, uint* needed);
    [DllImport("advapi32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsValidSid(nint sid);
    [DllImport("advapi32.dll")] private static extern byte* GetSidSubAuthorityCount(nint sid);
    [DllImport("advapi32.dll")] private static extern uint* GetSidSubAuthority(nint sid, uint index);
}
