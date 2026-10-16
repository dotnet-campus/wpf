using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

internal static unsafe partial class Program
{
    private static readonly List<object> Calls = [];
    private static nint module;
    private static uint sdkVersion;
    private static bool lifecyclePassed;
    private static string? projectionGlyphEvidenceInput;

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint LoadLibraryExW(string path, nint file, uint flags);
    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern nint GetProcAddress(nint handle, string name);
    [DllImport("ole32", ExactSpelling = true)]
    private static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32", ExactSpelling = true)]
    private static extern void CoUninitialize();

    private static int Main(string[] args)
    {
        string run = Guid.NewGuid().ToString("N");
        string results = Path.Combine(AppContext.BaseDirectory, "Results", run);
        Directory.CreateDirectory(results);
        string mode = args.Length == 0 ? "candidate" : args[0];
        projectionGlyphEvidenceInput = args.Length > 1 && args[1] != "perf-async"
            ? Path.GetFullPath(args[1], AppContext.BaseDirectory) : null;
        string path = "";
        string[] required = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Contracts", "wpfgfx.def"))
            .SkipWhile(line => line.Trim() != "EXPORTS").Skip(1)
            .Select(line => line.Split(';')[0].Trim()).Where(line => line.Length > 0).ToArray();
        string? error = null;
        string[] exports = [];
        string[] missing = [];
        string[] extra = [];
        string sha = "";
        bool com = false;
        try
        {
            path = mode == "reference" ? ReferencePath() : Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll");
            string versionLine = File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Contracts", "wgx_sdk_version.h"))
                .Single(line => line.StartsWith("#define MIL_SDK_VERSION ", StringComparison.Ordinal));
            sdkVersion = Convert.ToUInt32(versionLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[2][2..], 16);
            sha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
            exports = ReadExports(path);
            missing = required.Except(exports, StringComparer.Ordinal).ToArray();
            extra = exports.Except(required, StringComparer.Ordinal).ToArray();
            module = LoadLibraryExW(path, 0, 0x1100);
            if (module == 0) throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
            foreach (string name in required)
                Calls.Add(new { Operation = "GetProcAddress", Name = name, Present = GetProcAddress(module, name) != 0 });
            Check("Version", ((delegate* unmanaged[Stdcall]<uint, int>)Export("MilVersionCheck"))(sdkVersion));
            int mismatch = ((delegate* unmanaged[Stdcall]<uint, int>)Export("MilVersionCheck"))(0);
            Calls.Add(new { Operation = "VersionMismatch", HResult = mismatch });
            if (mismatch != unchecked((int)0x88982F0B)) throw new InvalidOperationException("VersionMismatch");
            Check("CoInitializeEx.MTA", CoInitializeEx(0, 0));
            com = true;
            if (args.Contains("perf-async", StringComparer.Ordinal))
            {
                asyncReferenceObservation = mode == "reference";
                PerfAsyncPresentContract();
                foreach (string scenario in new[] { "invalid-target", "invalid-settings", "zero-target", "wrong-type", "short-size", "unaligned-size", "valid-settings", "invalid-target-and-settings" })
                    PerfAsyncPresentContract(scenario);
                if (!asyncPerfPassed) throw new InvalidOperationException("AsyncPerf.ContractMismatch");
                lifecyclePassed = true;
                return 0;
            }
            ProjectionGlyphPerfContract();
            if (mode != "reference") VerifyProjectionGlyphReference();
            TileMappingContract();
            if (mode != "reference") VerifyTileMappingReference();
            ColorContextContract();
            for (int cycle = 0; cycle < 8; cycle++) Cycle(cycle);
            lifecyclePassed = true;
            if (missing.Length != 0 || extra.Length != 0) throw new InvalidOperationException("PEExportContractMismatch");
        }
        catch (Exception exception) when (exception is IOException or System.ComponentModel.Win32Exception or ExternalException or InvalidOperationException or BadImageFormatException)
        {
            error = exception.ToString();
        }
        finally
        {
            if (com) CoUninitialize();
            // NativeAOT unload is not supported by this test contract; process exit is explicit, not an unload claim.
            File.WriteAllText(Path.Combine(results, "report.json"), JsonSerializer.Serialize(new
            {
                SchemaVersion = 1, Runtime = RuntimeInformation.FrameworkDescription,
                OriginalSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(ReferencePath()))),
                RunId = run, Mode = mode, Path = path, Sha256 = sha,
                Version = File.Exists(path) ? FileVersionInfo.GetVersionInfo(path).FileVersion : null,
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(), Required = required, Exports = exports,
                Missing = missing, Extra = extra, Calls, Error = error, Passed = error is null,
                PeContractPassed = missing.Length == 0 && extra.Length == 0 && exports.Length != 0,
                LifecyclePassed = lifecyclePassed, AllAbiPassed = false, SdkVersion = sdkVersion,
                HarnessSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(Program).Assembly.Location))),
                FixedGolden1012Satisfied = false, NativeAotUnloadVerified = false,
                Scope = "x64 def/PE/GetProcAddress; IUnknown only (no private IID); bitmap clear/readback; four synchronous and four asynchronous channel shutdown/recreate pairs; not all exported method semantics"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        return error is null ? 0 : 1;
    }

    private static string ReferencePath()
    {
        string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "shared", "Microsoft.WindowsDesktop.App");
        return Directory.EnumerateDirectories(root).OrderByDescending(p => Version.Parse(Path.GetFileName(p).Split('-')[0]))
            .Select(p => Path.Combine(p, "wpfgfx_cor3.dll")).First(File.Exists);
    }

    private static nint Export(string name)
    {
        nint address = GetProcAddress(module, name);
        if (address == 0) throw new InvalidOperationException(name);
        return address;
    }

    private static void Check(string operation, int result)
    {
        Calls.Add(new { Operation = operation, HResult = result });
        if (result < 0) Marshal.ThrowExceptionForHR(result);
    }

    private static void Release(nint value)
    {
        if (value == 0) return;
        uint references = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
        Calls.Add(new { Operation = "IUnknown.Release", BeforeReferences = references + 1, AfterReferences = references, ReturnKind = "ULONG, not HRESULT" });
    }

    private static void Cycle(int cycle)
    {
        Calls.Add(new { Operation = "Cycle", Index = cycle, Transport = cycle % 2 == 0 ? "SameThread" : "CrossThread" });
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager");
        var shutdown = (delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager");
        var factoryCreate = (delegate* unmanaged[Stdcall]<nint*, uint, int>)Export("MILCreateFactory");
        var targetCreate = (delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int>)Export("MILFactoryCreateBitmapRenderTarget");
        var connectionCreate = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var resourceCreate = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        var resourceRelease = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)Export("MilResource_ReleaseOnChannel");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
        var getBitmap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)Export("MILRenderTargetBitmapGetBitmap");
        nint factory = 0, target = 0, bitmap = 0, connection = 0, channel = 0;
        Check("Initialize", initialize(0));
        try
        {
            Check("Factory", factoryCreate(&factory, sdkVersion));
            uint added = ((delegate* unmanaged[Stdcall]<nint, uint>)Export("MILAddRef"))(factory);
            uint remaining = ((delegate* unmanaged[Stdcall]<nint, uint>)Export("MILRelease"))(factory);
            Calls.Add(new { Operation = "MILAddRef", BeforeReferences = added - 1, AfterReferences = added, ReturnKind = "ULONG, not HRESULT" });
            Calls.Add(new { Operation = "MILRelease", BeforeReferences = added, AfterReferences = remaining, ReturnKind = "ULONG, not HRESULT" });
            if (added != remaining + 1) throw new InvalidOperationException("ExportReferencePair");
            Check("Connection", connectionCreate((byte)(cycle % 2 == 0 ? 1 : 0), &connection));
            Check("Channel", channelCreate(connection, 0, &channel));
            uint resource = 0;
            Check("Resource", resourceCreate(channel, 49, &resource));
            CommandContract(channel, resource);
            ResourceTransferContract(channel);
            int deleted = 0;
            Check("ResourceRelease", resourceRelease(channel, resource, &deleted));
            if (deleted != 1) throw new InvalidOperationException("ResourceRelease");
            Check("CloseBatch", close(channel));
            Check("Commit", commit(channel));
            Check("Command.SyncFlush", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush"))(channel));
            Check("Target", targetCreate(factory, 2, 2, 16, 96, 96, 1, &target));
            Guid iid = new("00000000-0000-0000-c000-000000000046");
            nint unknown = 0;
            Check("QueryInterface", ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)Export("MILQueryInterface"))(target, &iid, &unknown));
            Release(unknown);
            float* color = stackalloc float[] { 1, 0, 0, 1 };
            Check("ClearRed", ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)target)[4])(target, color, 0));
            Check("GetBitmap", getBitmap(target, &bitmap));
            byte* pixels = stackalloc byte[16];
            Check("CopyPixels", ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 8, 16, pixels));
            byte[] observed = new ReadOnlySpan<byte>(pixels, 16).ToArray();
            Calls.Add(new { Operation = "Pixels", Bytes = Convert.ToHexString(observed) });
            if (!observed.SequenceEqual(new byte[] { 0, 0, 255, 255, 0, 0, 255, 255, 0, 0, 255, 255, 0, 0, 255, 255 }))
                throw new InvalidOperationException("Pixels");
        }
        finally
        {
            Release(bitmap); Release(target); Release(factory);
            if (channel != 0) Check("DestroyChannel", destroy(channel));
            if (connection != 0) Check("Disconnect", disconnect(connection));
            Check("Shutdown", shutdown());
        }
    }

    private static string[] ReadExports(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        using var stream = new MemoryStream(bytes, false);
        using var pe = new PEReader(stream);
        if (pe.PEHeaders.CorHeader is not null || pe.PEHeaders.CoffHeader.Machine != System.Reflection.PortableExecutable.Machine.Amd64)
            throw new BadImageFormatException(path);
        int Offset(int rva)
        {
            var section = pe.PEHeaders.SectionHeaders.Single(s => rva >= s.VirtualAddress && rva < s.VirtualAddress + Math.Max(s.VirtualSize, s.SizeOfRawData));
            return checked(section.PointerToRawData + rva - section.VirtualAddress);
        }
        int directory = Offset(pe.PEHeaders.PEHeader!.ExportTableDirectory.RelativeVirtualAddress);
        int count = BitConverter.ToInt32(bytes, directory + 24);
        int names = Offset(BitConverter.ToInt32(bytes, directory + 32));
        return Enumerable.Range(0, count).Select(i =>
        {
            int offset = Offset(BitConverter.ToInt32(bytes, names + i * 4));
            int end = Array.IndexOf(bytes, (byte)0, offset);
            return System.Text.Encoding.ASCII.GetString(bytes, offset, end - offset);
        }).Order(StringComparer.Ordinal).ToArray();
    }
}
