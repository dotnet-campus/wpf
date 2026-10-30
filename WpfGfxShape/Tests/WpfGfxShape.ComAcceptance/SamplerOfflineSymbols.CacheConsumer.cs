#if CACHE_CONSUMER_SYMBOL_PREPARATION
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WpfGfxShape.ComAcceptance;

internal static partial class SamplerOfflineSymbols
{
    internal static async Task<int> PrepareCacheConsumerAsync(string directory)
    {
        string root = Path.GetFullPath(directory);
        Directory.CreateDirectory(root);
        using var output = new StreamWriter(Path.Combine(root, "preparation.txt")) { AutoFlush = true };
        int result = 6;
        try
        {
            string candidateHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(AcceptancePaths.AotDll)));
            output.WriteLine($"CandidateSHA256={candidateHash};Mvid={typeof(NativeRun).Module.ModuleVersionId};Pid={Environment.ProcessId};Architecture={RuntimeInformation.ProcessArchitecture};Runtime={RuntimeInformation.FrameworkDescription};DesktopVersion={FileVersionInfo.GetVersionInfo(typeof(System.Windows.Application).Assembly.Location).ProductVersion};BusinessRuns=0;Breakpoints=0");
            if (candidateHash != "6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403") throw new InvalidDataException();
            string image = AcceptancePaths.OriginalDll;
            using var file = File.OpenRead(image);
            string imageHash = Convert.ToHexString(SHA256.HashData(file));
            output.WriteLine($"Image={image};SHA256={imageHash}");
            if (imageHash != "AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1") throw new InvalidDataException();
            file.Position = 0;
            using var pe = new PEReader(file);
            var cv = pe.ReadCodeViewDebugDirectoryData(pe.ReadDebugDirectory().Single(e => e.Type == DebugDirectoryEntryType.CodeView));
            output.WriteLine($"RSDS={cv.Guid:D};Age={cv.Age};Path={cv.Path};Machine={pe.PEHeaders.CoffHeader.Machine}");
            if (cv.Guid != new Guid("8756f755-10f4-48be-9886-22c9a80c75b8") || cv.Age != 1 || pe.PEHeaders.CoffHeader.Machine != Machine.Amd64) throw new InvalidDataException();
            string pdb = Path.Combine(root, "wpfgfx_cor3.pdb");
            string cachedPdb = Path.Combine(AppContext.BaseDirectory, "inputs", "wpfgfx_cor3.pdb");
            output.WriteLine($"CachedPdb={cachedPdb};NetworkRequests=0");
            byte[] bytes = await File.ReadAllBytesAsync(cachedPdb).ConfigureAwait(false);
            string hash = Convert.ToHexString(SHA256.HashData(bytes));
            output.WriteLine($"PdbBytes={bytes.Length};PdbSHA256={hash}");
            if (bytes.Length != 20287488 || hash != "732FB582AF469CA52FD3B1C39520FFAD892EE6E9FAFDA1BC69A6066EF6212582") throw new InvalidDataException();
            byte[] info = new Msf(bytes, output).Stream(1);
            if (info.Length < 28 || new Guid(info.AsSpan(12, 16)) != cv.Guid || U32(info, 8) != cv.Age) throw new InvalidDataException();
            output.WriteLine($"MSFMatchesRSDS=True;Guid={cv.Guid:D};Age={cv.Age}");
            await File.WriteAllBytesAsync(pdb, bytes).ConfigureAwait(false);
            File.Copy(image, Path.Combine(root, "wpfgfx_cor3.dll"), true);
            nint process = (nint)0x632F0067;
            const ulong loadBase = 0x180000000;
            SymSetOptions(0x4 | 0x10 | 0x200 | 0x400 | 0x80000);
            if (!SymInitialize(process, root, false)) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            try
            {
                ulong loaded = SymLoadModuleEx(process, file.SafeFileHandle.DangerousGetHandle(), Path.Combine(root, "wpfgfx_cor3.dll"), null, loadBase, (uint)(pe.PEHeaders.PEHeader?.SizeOfImage ?? 0), 0, 0);
                output.WriteLine($"SymLoadModuleEx=0x{loaded:X};LastError={Marshal.GetLastWin32Error()}");
                if (loaded != loadBase) throw new InvalidDataException();
                ReadModule(process, loadBase, "BeforeDeferredLoad", output);
                nint trigger = Marshal.AllocHGlobal(4096);
                try
                {
                    Marshal.Copy(new byte[4096], 0, trigger, 4096);
                    Marshal.WriteInt32(trigger, 88);
                    Marshal.WriteInt32(trigger, 80, 4000);
                    SetLastError(0);
                    bool found = SymFromAddr(process, loadBase + 0xB65F, out ulong displacement, trigger);
                    int error = Marshal.GetLastWin32Error();
                    int length = Marshal.ReadInt32(trigger, 76);
                    string? name = found && length >= 0 && length <= 4000 ? Marshal.PtrToStringAnsi(trigger + 84, length) : null;
                    output.WriteLine($"DeferredLoadTriggerRva=0xB65F;SymFromAddr={found};LastError={error};Name={name};Displacement=0x{displacement:X}");
                }
                finally { Marshal.FreeHGlobal(trigger); }
                var module = ReadModule(process, loadBase, "AfterDeferredLoad", output);
                if (module.BaseOfImage != loadBase || module.ImageSize != pe.PEHeaders.PEHeader?.SizeOfImage || string.IsNullOrWhiteSpace(module.LoadedPdbName) || module.PdbUnmatched || module.PdbSig70 != cv.Guid || module.PdbAge != cv.Age || module.MachineType != 0x8664 || module.SymType != 3) throw new InvalidDataException();
                bool get = ReadCacheConsumerFunction(process, loadBase, module.ImageSize, "CMilVisualCache::GetRenderTargetBitmap", output);
                bool update = ReadCacheConsumerFunction(process, loadBase, module.ImageSize, "CMilVisualCache::Update", output);
                result = get && update ? 0 : 5;
            }
            finally
            {
                bool cleanup = SymCleanup(process);
                output.WriteLine($"SymCleanup={cleanup};LastError={Marshal.GetLastWin32Error()}");
                if (!cleanup) result = 6;
            }
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or OverflowException or BadImageFormatException or OperationCanceledException or System.ComponentModel.Win32Exception)
        {
            output.WriteLine($"Result=Unknown;Error={error}");
        }
        output.WriteLine($"ApplicationExit={result};CacheConsumerState=UnknownNotRun;BusinessRuns=0;Breakpoints=0");
        return result;
    }

    private static bool ReadCacheConsumerFunction(nint process, ulong loadBase, uint imageSize, string name, TextWriter output)
    {
        nint symbol = Marshal.AllocHGlobal(4096);
        try
        {
            Marshal.Copy(new byte[4096], 0, symbol, 4096);
            Marshal.WriteInt32(symbol, 88);
            Marshal.WriteInt32(symbol, 80, 4000);
            SetLastError(0);
            bool found = SymFromName(process, name, symbol);
            int error = Marshal.GetLastWin32Error();
            int length = Marshal.ReadInt32(symbol, 76);
            string? actual = found && length >= 0 && length <= 4000 ? Marshal.PtrToStringAnsi(symbol + 84, length) : null;
            ulong address = (ulong)Marshal.ReadInt64(symbol, 56);
            uint size = (uint)Marshal.ReadInt32(symbol, 28);
            uint tag = (uint)Marshal.ReadInt32(symbol, 72);
            bool valid = found && actual == name && tag == 5 && size > 0 && address >= loadBase && address - loadBase < imageSize && size <= imageSize - (address - loadBase);
            output.WriteLine($"Requested={name};Found={found};LastError={error};Name={actual};Address=0x{address:X};Rva=0x{(address >= loadBase ? address - loadBase : 0):X};Size={size};Tag={tag};ExactFunction={valid}");
            return valid;
        }
        finally { Marshal.FreeHGlobal(symbol); }
    }

    [DllImport("dbghelp.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SymFromName(nint process, string name, nint symbol);
}
#endif
