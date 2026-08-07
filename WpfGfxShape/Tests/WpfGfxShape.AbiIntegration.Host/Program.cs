using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

return await AbiIntegrationHost.RunAsync(args).ConfigureAwait(false);

internal static partial class AbiIntegrationHost
{
    private const string NativeLibraryName = "wpfgfx_cor3.dll";
    private const uint SupportedAbiVersion = 1;
    private const int S_OK = 0;
    private const int E_NOTIMPL = unchecked((int) 0x80004001);
    private const int E_POINTER = unchecked((int) 0x80004003);
    private const int E_FAIL = unchecked((int) 0x80004005);
    private const int E_INVALIDARG = unchecked((int) 0x80070057);
    private const ulong ChecksumSeed = 0x5750464746580001UL;

    private static string? s_libraryPath;
    private static nint s_libraryHandle;

    internal static async Task<int> RunAsync(string[] args)
    {
        HostResult result;

        try
        {
            HostOptions options = HostOptions.Parse(args);
            result = options.Case switch
            {
                "protocol" => RunProtocol(options),
                "concurrent" => await RunConcurrentAsync(options).ConfigureAwait(false),
                "missing-dll" => RunExpectedLoadFailure(options, typeof(DllNotFoundException)),
                "bad-image" => RunExpectedLoadFailure(options, typeof(BadImageFormatException)),
                "missing-export" => RunMissingExport(options),
                _ => throw new ArgumentException($"Unknown case '{options.Case}'.", nameof(args)),
            };

            await WriteResultAsync(options.ResultPath, result).ConfigureAwait(false);
            return result.Passed ? 0 : 1;
        }
        catch (Exception exception)
        {
            string? resultPath = HostOptions.TryGetResultPath(args);
            if (!string.IsNullOrWhiteSpace(resultPath))
            {
                result = new HostResult(false, null, null, null, exception.ToString());
                await WriteResultAsync(resultPath, result).ConfigureAwait(false);
            }

            Console.Error.WriteLine(exception);
            return 2;
        }
    }

    private static HostResult RunProtocol(HostOptions options)
    {
        ConfigureResolver(options.LibraryPath);

        const ulong input = 0x1122334455667788UL;
        nuint context = 0x12345678;
        ulong output = ulong.MaxValue;

        Require(NativeMethods.Invoke(SupportedAbiVersion, 0, input, context, out output) == S_OK, "Checksum call failed.");
        Require(output == CalculateChecksum(input, context), "Checksum output did not match.");

        output = ulong.MaxValue;
        Require(NativeMethods.Invoke(2, 0, 0, 0, out output) == E_INVALIDARG, "Unsupported ABI version was not rejected.");
        Require(output == 0, "Unsupported ABI version did not clear output.");

        Require(NativeMethods.InvokeWithNullOutput(SupportedAbiVersion, 0, 0, 0, 0) == E_POINTER, "Null output was not rejected.");

        output = ulong.MaxValue;
        Require(NativeMethods.Invoke(SupportedAbiVersion, 1, 0, 0, out output) == E_INVALIDARG, "Controlled argument failure returned the wrong HRESULT.");
        Require(output == 0, "Controlled argument failure did not clear output.");

        output = ulong.MaxValue;
        Require(NativeMethods.Invoke(SupportedAbiVersion, 2, 0, 0, out output) == E_FAIL, "Controlled exception returned the wrong HRESULT.");
        Require(output == 0, "Controlled exception did not clear output.");

        output = ulong.MaxValue;
        Require(NativeMethods.Invoke(SupportedAbiVersion, uint.MaxValue, 0, 0, out output) == E_NOTIMPL, "Unknown operation returned the wrong HRESULT.");
        Require(output == 0, "Unknown operation did not clear output.");

        output = 0;
        Require(NativeMethods.Invoke(SupportedAbiVersion, 0, input, context, out output) == S_OK, "Call after failure did not recover.");
        Require(output == CalculateChecksum(input, context), "Recovered checksum output did not match.");

        return CreateSuccessResult(options);
    }

    private static async Task<HostResult> RunConcurrentAsync(HostOptions options)
    {
        ConfigureResolver(options.LibraryPath);

        Task[] tasks = Enumerable.Range(0, 32)
            .Select(worker => Task.Run(() =>
            {
                for (int iteration = 0; iteration < 100; iteration++)
                {
                    ulong input = ((ulong) worker << 32) | (uint) iteration;
                    nuint context = (nuint) (worker + 1);
                    ulong output = 0;
                    int hresult = NativeMethods.Invoke(SupportedAbiVersion, 0, input, context, out output);
                    Require(hresult == S_OK, "Concurrent checksum call failed.");
                    Require(output == CalculateChecksum(input, context), "Concurrent checksum output did not match.");
                }
            }))
            .ToArray();

        await Task.WhenAll(tasks).ConfigureAwait(false);
        return CreateSuccessResult(options);
    }

    private static HostResult RunExpectedLoadFailure(HostOptions options, Type expectedExceptionType)
    {
        ConfigureResolver(options.LibraryPath);

        try
        {
            ulong output = 0;
            NativeMethods.Invoke(SupportedAbiVersion, 0, 0, 0, out output);
            throw new InvalidOperationException($"Expected {expectedExceptionType.Name} was not thrown.");
        }
        catch (Exception exception) when (expectedExceptionType.IsInstanceOfType(exception))
        {
            return new HostResult(true, Path.GetFullPath(options.LibraryPath), null, null, null);
        }
    }

    private static HostResult RunMissingExport(HostOptions options)
    {
        ConfigureResolver(options.LibraryPath);

        try
        {
            MissingExportMethods.Invoke();
            throw new InvalidOperationException("Expected EntryPointNotFoundException was not thrown.");
        }
        catch (EntryPointNotFoundException)
        {
            return CreateSuccessResult(options);
        }
    }

    private static void ConfigureResolver(string libraryPath)
    {
        s_libraryPath = Path.GetFullPath(libraryPath);
        NativeLibrary.SetDllImportResolver(typeof(AbiIntegrationHost).Assembly, ResolveLibrary);
    }

    private static nint ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, NativeLibraryName, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        s_libraryHandle = NativeLibrary.Load(s_libraryPath!);
        return s_libraryHandle;
    }

    private static HostResult CreateSuccessResult(HostOptions options)
    {
        string expectedPath = Path.GetFullPath(options.LibraryPath);
        string actualPath = GetLoadedModulePath(expectedPath);
        string actualHash = CalculateSha256(actualPath);

        Require(string.Equals(expectedPath, actualPath, StringComparison.OrdinalIgnoreCase), "Loaded module path did not match the published DLL path.");
        Require(string.Equals(options.ExpectedSha256, actualHash, StringComparison.OrdinalIgnoreCase), "Loaded module SHA-256 did not match the published DLL hash.");

        return new HostResult(true, expectedPath, actualPath, actualHash, null);
    }

    private static string GetLoadedModulePath(string expectedPath)
    {
        string? modulePath = Process.GetCurrentProcess().Modules
            .Cast<ProcessModule>()
            .Select(module => module.FileName)
            .FirstOrDefault(path => string.Equals(Path.GetFullPath(path), expectedPath, StringComparison.OrdinalIgnoreCase));

        return modulePath is null
            ? throw new InvalidOperationException("The loaded Native AOT module was not found in the process module list.")
            : Path.GetFullPath(modulePath);
    }

    private static ulong CalculateChecksum(ulong input, nuint context)
    {
        return unchecked(ChecksumSeed ^ input ^ (ulong) context ^ ((ulong) (uint) IntPtr.Size << 56));
    }

    private static string CalculateSha256(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static async Task WriteResultAsync(string resultPath, HostResult result)
    {
        string fullPath = Path.GetFullPath(resultPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        using FileStream stream = File.Create(fullPath);
        await JsonSerializer.SerializeAsync(stream, result, new JsonSerializerOptions { WriteIndented = true }).ConfigureAwait(false);
    }

    private static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryName, EntryPoint = "WpfGfxShape_NativeAotAbiProbe_v1")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
        internal static partial int Invoke(uint abiVersion, uint operation, ulong input, nuint context, out ulong output);

        [LibraryImport(NativeLibraryName, EntryPoint = "WpfGfxShape_NativeAotAbiProbe_v1")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
        internal static partial int InvokeWithNullOutput(uint abiVersion, uint operation, ulong input, nuint context, nint output);
    }

    private static partial class MissingExportMethods
    {
        [LibraryImport(NativeLibraryName, EntryPoint = "WpfGfxShape_MissingExport")]
        internal static partial int Invoke();
    }
}

internal sealed record HostOptions(string Case, string LibraryPath, string ExpectedSha256, string ResultPath)
{
    internal static HostOptions Parse(string[] args)
    {
        Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < args.Length; index += 2)
        {
            if (index + 1 >= args.Length || !args[index].StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException("Arguments must be supplied as --name value pairs.", nameof(args));
            }

            values.Add(args[index][2..], args[index + 1]);
        }

        return new HostOptions(
            GetRequired(values, "case"),
            GetRequired(values, "dll"),
            GetRequired(values, "expected-sha256", allowEmpty: true),
            GetRequired(values, "result"));
    }

    internal static string? TryGetResultPath(string[] args)
    {
        int index = Array.FindIndex(args, argument => string.Equals(argument, "--result", StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    private static string GetRequired(Dictionary<string, string> values, string name, bool allowEmpty = false)
    {
        return values.TryGetValue(name, out string? value) && (allowEmpty || !string.IsNullOrWhiteSpace(value))
            ? value
            : throw new ArgumentException($"Missing required --{name} argument.");
    }
}

internal sealed record HostResult(
    bool Passed,
    string? ExpectedModulePath,
    string? ActualModulePath,
    string? ActualSha256,
    string? Error);
