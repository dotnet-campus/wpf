using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

internal static class Program
{
    private static readonly List<object> Calls = [];
    private static nint module;

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint LoadLibraryExW(string path, nint file, uint flags);
    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern unsafe uint GetModuleFileNameW(nint handle, char* path, uint length);
    [DllImport("ole32", ExactSpelling = true)]
    private static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32", ExactSpelling = true)]
    private static extern void CoUninitialize();

    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "official-contract") return await FetchOfficialContractAsync().ConfigureAwait(false);
        return RunLifecycle();
    }

    private static async Task<int> FetchOfficialContractAsync()
    {
        string results = Path.Combine(AppContext.BaseDirectory, "onlyResults");
        Directory.CreateDirectory(results);
        const string url = "https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/libraries";
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            File.WriteAllText(Path.Combine(results, "official-native-libraries.html"), text);
            File.WriteAllText(Path.Combine(results, "official-source.json"), JsonSerializer.Serialize(new { Url = url, RetrievedUtc = DateTimeOffset.UtcNow, Status = (int)response.StatusCode, Sha256 = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text))) }, new JsonSerializerOptions { WriteIndented = true }));
            return 0;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or IOException)
        {
            File.WriteAllText(Path.Combine(results, "official-fetch-error.txt"), exception.ToString());
            return 1;
        }
    }

    private static unsafe int RunLifecycle()
    {
        string results = Path.Combine(AppContext.BaseDirectory, "onlyResults");
        Directory.CreateDirectory(results);
        string path = Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll");
        string sha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        string? error = null;
        string loadedPath = "";
        bool com = false;
        bool passed = false;
        try
        {
            using (var pe = new PEReader(File.OpenRead(path)))
            {
                Record(results, new { Operation = "PE", Machine = pe.PEHeaders.CoffHeader.Machine.ToString(), HasClrHeader = pe.PEHeaders.CorHeader is not null, Sha256 = sha });
                if (pe.PEHeaders.CoffHeader.Machine != Machine.I386 || pe.PEHeaders.CorHeader is not null || RuntimeInformation.ProcessArchitecture != Architecture.X86)
                    throw new BadImageFormatException(path);
            }
            module = LoadLibraryExW(path, 0, 0x1100);
            if (module == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
            char* buffer = stackalloc char[32768];
            uint length = GetModuleFileNameW(module, buffer, 32768);
            if (length == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
            loadedPath = new string(buffer, 0, checked((int)length));
            Record(results, new { Operation = "SharedLoad", LoadedPath = loadedPath });
            uint sdk = Convert.ToUInt32(File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Contracts", "wgx_sdk_version.h"))
                .Single(line => line.StartsWith("#define MIL_SDK_VERSION ", StringComparison.Ordinal)).Split(' ', StringSplitOptions.RemoveEmptyEntries)[2][2..], 16);
            Check(results, "Version", ((delegate* unmanaged[Stdcall]<uint, int>)Export("MilVersionCheck"))(sdk));
            Check(results, "CoInitializeEx", CoInitializeEx(0, 0));
            com = true;
            for (int cycle = 0; cycle < 8; cycle++) Cycle(results, sdk, cycle);
            passed = true;
        }
        catch (Exception exception) when (exception is IOException or Win32Exception or ExternalException or InvalidOperationException or BadImageFormatException)
        {
            error = exception.ToString();
        }
        finally
        {
            if (com) CoUninitialize();
            // Microsoft Learn explicitly excludes NativeAOT library unloading; keep it loaded until process exit.
            File.WriteAllText(Path.Combine(results, "report.json"), JsonSerializer.Serialize(new
            {
                Passed = passed, Error = error, Architecture = RuntimeInformation.ProcessArchitecture.ToString(), PointerSize = IntPtr.Size,
                Path = path, LoadedPath = loadedPath, Sha256 = sha, Calls,
                ClientSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(Program).Assembly.Location))),
                BuildIdentity = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "build-identity.txt")),
                NativeAotUnloadVerified = false, OfficialUnloadContract = "MicrosoftLearnNativeAotLibrariesExplicitlyUnsupported", FreeLibraryAttempted = false,
                Scope = "x86 shared load/version; eight initialize/factory/channel/resource/release/shutdown cycles; process exit, not DLL unload; not full ABI/E2E"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        return passed ? 0 : 1;
    }

    private static nint Export(string name) => NativeLibrary.GetExport(module, name);

    private static void Record(string results, object call)
    {
        Calls.Add(call);
        File.WriteAllText(Path.Combine(results, "checkpoint.json"), JsonSerializer.Serialize(Calls, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void Check(string results, string operation, int hr)
    {
        Record(results, new { Operation = operation, HResult = hr });
        if (hr < 0) Marshal.ThrowExceptionForHR(hr);
    }

    private static unsafe void Cycle(string results, uint sdk, int cycle)
    {
        Record(results, new { Operation = "Cycle", Index = cycle });
        nint factory = 0, connection = 0, channel = 0;
        uint factoryRemaining = 0;
        Check(results, "Initialize", ((delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager"))(0));
        try
        {
            Check(results, "Factory", ((delegate* unmanaged[Stdcall]<nint*, uint, int>)Export("MILCreateFactory"))(&factory, sdk));
            Check(results, "Connection", ((delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create"))((byte)(cycle % 2 == 0 ? 1 : 0), &connection));
            Check(results, "Channel", ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel"))(connection, 0, &channel));
            uint resource = 0;
            Check(results, "ResourceCreate", ((delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel"))(channel, 49, &resource));
            int deleted = 0;
            Check(results, "ResourceRelease", ((delegate* unmanaged[Stdcall]<nint, uint, int*, int>)Export("MilResource_ReleaseOnChannel"))(channel, resource, &deleted));
            Record(results, new { Operation = "ResourceDeleted", Deleted = deleted });
            if (deleted != 1) throw new InvalidOperationException("ResourceDeleted");
            Check(results, "CloseBatch", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch"))(channel));
            Check(results, "Commit", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel"))(channel));
            Check(results, "SyncFlush", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush"))(channel));
        }
        finally
        {
            if (channel != 0) Check(results, "DestroyChannel", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel"))(channel));
            if (connection != 0) Check(results, "Disconnect", ((delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect"))(connection));
            if (factory != 0)
            {
                factoryRemaining = ((delegate* unmanaged[Stdcall]<nint, uint>)Export("MILRelease"))(factory);
                Record(results, new { Operation = "FactoryRelease", Remaining = factoryRemaining });
            }
            Check(results, "Shutdown", ((delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager"))());
        }
        if (factoryRemaining != 0) throw new InvalidOperationException("FactoryRelease");
    }
}
