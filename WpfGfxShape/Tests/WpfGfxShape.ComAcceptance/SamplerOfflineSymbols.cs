#if SAMPLER_OFFLINE_SYMBOLS
using System.Buffers.Binary;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WpfGfxShape.ComAcceptance;

internal static partial class SamplerOfflineSymbols
{
    internal static Task<int> RunAsync(string directory)
    {
        int result = Run(directory);
        string native = Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll");
        string candidateHash = File.Exists(native) ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(native))) : "Missing";
        File.WriteAllText(Path.Combine(Path.GetFullPath(directory), "offline-exit.txt"), $"ApplicationExit={result};CandidateSHA256={candidateHash};BusinessRuns=0;NetworkRequests=0;DesktopVersion={FileVersionInfo.GetVersionInfo(typeof(System.Windows.Application).Assembly.Location).ProductVersion}");
        return Task.FromResult(result);
    }

    private static int Run(string directory)
    {
        string root = Path.GetFullPath(directory);
        Directory.CreateDirectory(root);
        using var output = new StreamWriter(Path.Combine(root, "symbols.txt")) { AutoFlush = true };
        try
        {
            string image = Path.Combine(AppContext.BaseDirectory, "wpfgfx_cor3.dll");
            string pdb = Path.Combine(AppContext.BaseDirectory, "wpfgfx_cor3.pdb");
            using var file = File.OpenRead(image);
            string hash = Convert.ToHexString(SHA256.HashData(file));
            output.WriteLine($"Image={image};SHA256={hash};TestMvid={typeof(NativeRun).Module.ModuleVersionId};Runtime={RuntimeInformation.FrameworkDescription};Architecture={RuntimeInformation.ProcessArchitecture};Pid={Environment.ProcessId}");
            if (hash != "AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1") throw new InvalidDataException("Image identity mismatch.");
            file.Position = 0;
            using var pe = new PEReader(file);
            int size = pe.PEHeaders.PEHeader?.SizeOfImage ?? throw new InvalidDataException("Missing PE header.");
            var cv = pe.ReadCodeViewDebugDirectoryData(pe.ReadDebugDirectory().Single(e => e.Type == DebugDirectoryEntryType.CodeView));
            output.WriteLine($"RSDS={cv.Guid:D};Age={cv.Age};Path={cv.Path};SizeOfImage={size};RVA=0xB65F");
            byte[] bytes = File.ReadAllBytes(pdb);
            string pdbHash = Convert.ToHexString(SHA256.HashData(bytes));
            output.WriteLine($"Pdb={pdb};PdbBytes={bytes.Length};PdbSha256={pdbHash}");
            if (bytes.Length != 20287488 || pdbHash != "732FB582AF469CA52FD3B1C39520FFAD892EE6E9FAFDA1BC69A6066EF6212582") throw new InvalidDataException("PDB hash mismatch.");
            var msf = new Msf(bytes, output);
            byte[] info = msf.Stream(1);
            if (info.Length < 28) throw new InvalidDataException("PDB info truncated.");
            uint age = U32(info, 8);
            Guid guid = new(info.AsSpan(12, 16));
            bool identity = guid == cv.Guid && age == cv.Age;
            output.WriteLine($"MSF7Version={U32(info, 0)};Signature=0x{U32(info, 4):X};Guid={guid:D};Age={age};MatchesRSDS={identity}");
            nint process = (nint)0x632F0066; // Stable offline token; never a live process handle.
            uint options = 0x4 | 0x10 | 0x200 | 0x400 | 0x80000 | 0x80000000;
            output.WriteLine($"DbgHelp={FileVersionInfo.GetVersionInfo(Path.Combine(Environment.SystemDirectory, "dbghelp.dll")).FileVersion};ModuleInfoSize={Marshal.SizeOf<ModuleInfo>()};GuidOffset={Marshal.OffsetOf<ModuleInfo>(nameof(ModuleInfo.PdbSig70))};MachineOffset={Marshal.OffsetOf<ModuleInfo>(nameof(ModuleInfo.MachineType))};LineSize={Marshal.SizeOf<LineInfo>()};SymbolSize=88;SymbolNameOffset=84;OptionsRequested=0x{options:X}");
            SetLastError(0);
            uint actual = SymSetOptions(options);
            int optionError = Marshal.GetLastWin32Error();
            output.WriteLine($"SymSetOptions=0x{actual:X};LastError={optionError};SearchPath={AppContext.BaseDirectory}");
            SetLastError(0);
            bool initialized = SymInitialize(process, AppContext.BaseDirectory, false);
            int initializeError = Marshal.GetLastWin32Error();
            output.WriteLine($"SymInitialize={initialized};LastError={initializeError}");
            if (!initialized) return 3;
            DebugCallback callback = (_, action, data, _) =>
            {
                try
                {
                    if (action == 6 && offlineRead != null)
                    {
                        nint request = (nint)data;
                        bool ok = offlineRead(process, (ulong)Marshal.ReadInt64(request), Marshal.ReadIntPtr(request, 8), (uint)Marshal.ReadInt32(request, 16), out uint read);
                        nint resultPointer = Marshal.ReadIntPtr(request, 24);
                        if (resultPointer != 0) Marshal.WriteInt32(resultPointer, (int)read);
                        return ok;
                    }
                    string? text = action == 0x10000000 ? Marshal.PtrToStringAnsi((nint)data) : null;
                    output.WriteLine($"Callback=0x{action:X};Data=0x{data:X};Text={text}");
                    return false;
                }
                catch (Exception error) { callbackFailure = error.ToString(); return false; }
            };
            try
            {
                SetLastError(0);
                bool registered = SymRegisterCallback64(process, callback, 0);
                int registerError = Marshal.GetLastWin32Error();
                output.WriteLine($"SymRegisterCallback64={registered};LastError={registerError}");
                SetLastError(0);
                bool pathSet = SymSetSearchPath(process, AppContext.BaseDirectory);
                int pathError = Marshal.GetLastWin32Error();
                output.WriteLine($"SymSetSearchPath={pathSet};LastError={pathError}");
                const ulong loadBase = 0x7FFC17C60000;
                SetLastError(0);
                ulong loaded = SymLoadModuleEx(process, file.SafeFileHandle.DangerousGetHandle(), image, null, loadBase, (uint)size, 0, 0);
                int loadError = Marshal.GetLastWin32Error();
                output.WriteLine($"SymLoadModuleEx=0x{loaded:X};LastError={loadError};ImageFileHandle={file.SafeFileHandle.DangerousGetHandle()}");
                ReadModule(process, loadBase, "Before", output);
                nint symbol = Marshal.AllocHGlobal(4096);
                bool found;
                ulong displacement;
                string? name = null;
                ulong symbolAddress = 0;
                uint symbolSize = 0;
                try
                {
                    Marshal.Copy(new byte[4096], 0, symbol, 4096);
                    Marshal.WriteInt32(symbol, 88);
                    Marshal.WriteInt32(symbol, 80, 4000);
                    SetLastError(0);
                    found = SymFromAddr(process, loadBase + 0xB65F, out displacement, symbol);
                    int symbolError = Marshal.GetLastWin32Error();
                    uint count = (uint)Marshal.ReadInt32(symbol, 76);
                    if (found && count <= 4000 && count <= 4096 - 84)
                    {
                        name = Marshal.PtrToStringAnsi(symbol + 84, (int)count);
                        symbolAddress = (ulong)Marshal.ReadInt64(symbol, 56);
                        symbolSize = (uint)Marshal.ReadInt32(symbol, 28);
                    }
                    output.WriteLine($"SymFromAddr={found};LastError={symbolError};NameLength={count};Name={name};Displacement=0x{displacement:X};Address=0x{symbolAddress:X};Size={symbolSize};Flags=0x{Marshal.ReadInt32(symbol, 24):X};Tag={Marshal.ReadInt32(symbol, 72)}");
                }
                finally { Marshal.FreeHGlobal(symbol); }
                var line = new LineInfo { SizeOfStruct = (uint)Marshal.SizeOf<LineInfo>() };
                SetLastError(0);
                bool hasLine = SymGetLineFromAddr64(process, loadBase + 0xB65F, out uint delta, ref line);
                int lineError = Marshal.GetLastWin32Error();
                output.WriteLine($"SymGetLineFromAddr64={hasLine};LastError={lineError};File={(hasLine ? Marshal.PtrToStringAnsi(line.FileName) : null)};Line={line.LineNumber};Displacement={delta};Address=0x{line.Address:X}");
                var module = ReadModule(process, loadBase, "After", output);
                bool authenticated = identity && loaded == loadBase && module.PdbSig70 == guid && module.PdbAge == age && !module.PdbUnmatched;
                bool contained = symbolAddress <= loadBase + 0xB65F && symbolAddress >= loadBase && displacement < symbolSize && symbolSize != 0;
                output.WriteLine($"Result={(authenticated && found && name != null ? "MatchedPdbSymbol" : "Unknown")};Authenticated={authenticated};FunctionRangeContainsRva={contained};NearestSymbolIsNotProofOfFunctionRange=True;StackDecoded=False");
                if (!found) ReadPublics(msf, pe, output);
                if (!authenticated || module.MachineType != 0x8664 || module.BaseOfImage != loadBase || module.ImageSize != size) throw new InvalidDataException("Unwind requires authenticated AMD64 module and PDB.");
                InspectOriginal(process, loadBase, size, pe, file, output);
                return authenticated && found && name != null ? 0 : 5;
            }
            finally
            {
                SetLastError(0);
                bool cleaned = SymCleanup(process);
                int cleanupError = Marshal.GetLastWin32Error();
                output.WriteLine($"SymCleanup={cleaned};LastError={cleanupError}");
                GC.KeepAlive(callback);
            }
        }
        catch (Exception error) when (error is IOException or InvalidOperationException or UnauthorizedAccessException or ArgumentException or OverflowException)
        {
            output.WriteLine($"Result=Unknown;Error={error}");
            return 6;
        }
    }

    private static ModuleInfo ReadModule(nint process, ulong address, string phase, TextWriter output)
    {
        var module = new ModuleInfo { SizeOfStruct = (uint)Marshal.SizeOf<ModuleInfo>() };
        SetLastError(0);
        bool result = SymGetModuleInfo64(process, address, ref module);
        int error = Marshal.GetLastWin32Error();
        output.WriteLine($"SymGetModuleInfo64.{phase}={result};LastError={error};Size={module.SizeOfStruct};Base=0x{module.BaseOfImage:X};ImageSize={module.ImageSize};NumSyms={module.NumSyms};SymType={module.SymType};Guid={module.PdbSig70:D};Age={module.PdbAge};PdbUnmatched={module.PdbUnmatched};LoadedPdb={module.LoadedPdbName};LoadedImage={module.LoadedImageName};Machine=0x{module.MachineType:X};Lines={module.LineNumbers};Publics={module.Publics}");
        return module;
    }

    private static void InspectOriginal(nint process, ulong loadBase, int imageSize, PEReader pe, FileStream image, TextWriter output)
    {
        string contextPath = Path.Combine(AppContext.BaseDirectory, "inputs", "context.json");
        if (!File.Exists(contextPath)) throw new FileNotFoundException("Original context input required.", contextPath);
        using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(contextPath));
        byte[] context = Convert.FromBase64String(json.RootElement.GetProperty("rawContext").GetString() ?? "");
        if (context.Length != 1232) throw new InvalidDataException("Unexpected AMD64 context length.");
        ulong Q(byte[] data, int offset)
        {
            if (offset < 0 || offset > data.Length - 8) throw new InvalidDataException("UInt64 outside input.");
            return BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(offset, 8));
        }
        // Windows SDK winnt.h AMD64 CONTEXT: six homes, flags/segments, six debug registers, then 17 DWORD64 registers.
        uint flags = U32(context, 48);
        if ((flags & 0x100003) != 0x100003 || Q(context, 248) != loadBase + 0xB65F) throw new InvalidDataException("AMD64 control/integer context or original RIP mismatch.");
        output.WriteLine($"ContextLayout=WindowsSDK-winnt.h-AMD64;Bytes=1232;ContextFlags=0x{flags:X};IntegerOffset=120;RipOffset=248;RegisterBits=64");
        string[] registers = ["Rax", "Rcx", "Rdx", "Rbx", "Rsp", "Rbp", "Rsi", "Rdi", "R8", "R9", "R10", "R11", "R12", "R13", "R14", "R15", "Rip"];
        for (int register = 0; register < registers.Length; register++) output.WriteLine($"OriginalRegister={registers[register]};Offset={120 + register * 8};Value=0x{Q(context,120 + register * 8):X}");
        var section = pe.PEHeaders.SectionHeaders.Single(s => 0xB650 >= s.VirtualAddress && (long)0xB650 + 48 <= (long)s.VirtualAddress + s.SizeOfRawData);
        if (section.PointerToRawData < 0 || (long)section.PointerToRawData + 0xB650 - section.VirtualAddress + 48 > image.Length) throw new InvalidDataException("Instruction bytes outside image file.");
        image.Position = checked(section.PointerToRawData + 0xB650 - section.VirtualAddress);
        byte[] code = new byte[48];
        image.ReadExactly(code);
        output.WriteLine($"OriginalImageCodeRvaB650={Convert.ToHexString(code)};FaultByteOffset=15;NotDisassembled=True");
        byte[] dump = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "inputs", "original.dmp"));
        output.WriteLine($"OriginalDumpBytes={dump.Length};SHA256={Convert.ToHexString(SHA256.HashData(dump))}");
        if (dump.Length != 1652255 || Convert.ToHexString(SHA256.HashData(dump)) != "1851A7517FFE42FB688BD098EBF463624F59180BF183AA71ABD90E872451B85C") throw new InvalidDataException("Original dump identity mismatch.");
        if (dump.Length < 32 || U32(dump, 0) != 0x504D444D) throw new InvalidDataException("Not minidump.");
        void Range(ulong offset, ulong length)
        {
            if (offset > (ulong)dump.Length || length > (ulong)dump.Length - offset) throw new InvalidDataException("Minidump range outside file.");
        }
        var memory = new List<(ulong Start, ulong Length, ulong Data)>();
        void AddMemory(ulong start, ulong length, ulong data)
        {
            Range(data, length);
            if (length > ulong.MaxValue - start) throw new InvalidDataException("Virtual memory range overflow.");
            memory.Add((start, length, data));
        }
        uint count = U32(dump, 8), directory = U32(dump, 12);
        if (directory > dump.Length || count > (dump.Length - directory) / 12) throw new InvalidDataException("Minidump directory invalid.");
        ulong rsp = Q(context, 152);
        for (int i = 0; i < count; i++)
        {
            int entry = checked((int)directory + i * 12);
            uint type = U32(dump, entry);
            uint length = U32(dump, entry + 4), offset = U32(dump, entry + 8);
            Range(offset, length);
            output.WriteLine($"DumpStreamType={type};Rva={offset};Length={length}");
            if (type == 5)
            {
                if (length < 4) throw new InvalidDataException("MemoryList header truncated.");
                uint ranges = U32(dump, (int)offset);
                if (ranges > (length - 4) / 16) throw new InvalidDataException("MemoryList descriptors truncated.");
                for (int r = 0; r < ranges; r++)
                {
                    int descriptor = checked((int)offset + 4 + r * 16);
                    AddMemory(Q(dump, descriptor), U32(dump, descriptor + 8), U32(dump, descriptor + 12));
                }
            }
            if (type == 9)
            {
                if (length < 16) throw new InvalidDataException("Memory64 header truncated.");
                ulong ranges = Q(dump, (int)offset), data = Q(dump, checked((int)offset + 8));
                if (ranges > (length - 16) / 16) throw new InvalidDataException("Memory64 descriptors truncated.");
                for (ulong r = 0; r < ranges; r++)
                {
                    int descriptor = checked((int)((ulong)offset + 16 + r * 16));
                    ulong bytes = Q(dump, descriptor + 8);
                    AddMemory(Q(dump, descriptor), bytes, data);
                    data = checked(data + bytes);
                }
            }
            if (type != 3) continue;
            if (length < 4) throw new InvalidDataException("Thread list invalid.");
            uint threads = U32(dump, (int)offset);
            if (length < 4 || threads > (length - 4) / 48) throw new InvalidDataException("Thread count invalid.");
            for (int j = 0; j < threads; j++)
            {
                int thread = checked((int)offset + 4 + j * 48);
                if (U32(dump, thread) != 168) continue;
                ulong start = Q(dump, thread + 24);
                uint bytes = U32(dump, thread + 32), data = U32(dump, thread + 36);
                if (data > dump.Length || bytes > dump.Length - data || rsp < start || rsp - start >= bytes) throw new InvalidDataException("Thread stack bounds invalid.");
                output.WriteLine($"OriginalThread=168;StackStart=0x{start:X};StackBytes={bytes};StackDataRva={data};StackScanNotUnwind=True");
                int first = checked((int)(rsp - start));
                nint symbol = Marshal.AllocHGlobal(4096);
                try
                {
                    for (int slot = first; slot <= Math.Min((int)bytes - 8, first + 2048); slot += 8)
                    {
                        ulong address = Q(dump, checked((int)data + slot));
                        if (address < loadBase || address >= loadBase + (uint)imageSize) continue;
                        Marshal.Copy(new byte[4096], 0, symbol, 4096);
                        Marshal.WriteInt32(symbol, 88); Marshal.WriteInt32(symbol, 80, 4000);
                        SetLastError(0);
                        bool found = SymFromAddr(process, address, out ulong delta, symbol);
                        int error = Marshal.GetLastWin32Error();
                        int nameLength = Marshal.ReadInt32(symbol, 76);
                        string? name = found && nameLength >= 0 && nameLength <= 4000 ? Marshal.PtrToStringAnsi(symbol + 84, nameLength) : null;
                        output.WriteLine($"StackCandidateOffset=0x{slot-first:X};Address=0x{address:X};Rva=0x{address-loadBase:X};SymFromAddr={found};LastError={error};Name={name};Displacement=0x{delta:X};NotUnwound=True");
                    }
                }
                finally { Marshal.FreeHGlobal(symbol); }
            }
        }
        WalkOriginal(process, loadBase, imageSize, pe, context, dump, memory);
        output.WriteLine($"ValidatedMemoryRanges={memory.Count};ExceptionStreamNotRequired=True;RawCandidatesBelowAreNotUnwind=True");
        foreach (var range in memory.Where(range => rsp >= range.Start && rsp - range.Start < range.Length))
        {
            ulong relative = rsp - range.Start;
            output.WriteLine($"MemoryStackStart=0x{range.Start:X};Length={range.Length};DataRva={range.Data};RspRelative={relative};NotUnwound=True");
            nint symbol = Marshal.AllocHGlobal(4096);
            try
            {
                ulong available = range.Length - relative;
                for (ulong slot = 0; slot + 8 <= Math.Min(available, 2048UL); slot += 8)
                {
                    ulong address = Q(dump, checked((int)(range.Data + relative + slot)));
                    if (address < loadBase || address - loadBase >= (uint)imageSize) continue;
                    Marshal.Copy(new byte[4096], 0, symbol, 4096);
                    Marshal.WriteInt32(symbol, 88);
                    Marshal.WriteInt32(symbol, 80, 4000);
                    SetLastError(0);
                    bool found = SymFromAddr(process, address, out ulong delta, symbol);
                    int error = Marshal.GetLastWin32Error();
                    int nameLength = Marshal.ReadInt32(symbol, 76);
                    string? name = found && nameLength >= 0 && nameLength <= 4000 ? Marshal.PtrToStringAnsi(symbol + 84, nameLength) : null;
                    output.WriteLine($"MemoryStackCandidateOffset=0x{slot:X};Address=0x{address:X};Rva=0x{address-loadBase:X};SymFromAddr={found};LastError={error};Name={name};Displacement=0x{delta:X};NotUnwound=True");
                }
            }
            finally { Marshal.FreeHGlobal(symbol); }
        }
    }

    private static uint U32(byte[] data, int offset)
    {
        if (offset < 0 || offset > data.Length - 4) throw new InvalidDataException("UInt32 outside stream.");
        return BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, 4));
    }

    private sealed class Msf
    {
        private readonly byte[] data;
        private readonly int blockSize;
        private readonly uint blocks;
        private readonly uint[] sizes;
        private readonly uint[][] pages;

        internal Msf(byte[] data, TextWriter output)
        {
            this.data = data;
            ReadOnlySpan<byte> magic = "Microsoft C/C++ MSF 7.00\r\n\u001aDS\0\0\0"u8;
            if (data.Length < 56 || !data.AsSpan(0, 32).SequenceEqual(magic)) throw new InvalidDataException("Not MSF7.");
            blockSize = checked((int)U32(data, 32));
            if (blockSize < 512 || blockSize > 65536 || (blockSize & (blockSize - 1)) != 0) throw new InvalidDataException("Invalid MSF block size.");
            blocks = U32(data, 40);
            if ((ulong)blocks * (uint)blockSize != (ulong)data.Length) throw new InvalidDataException("Invalid MSF block count.");
            uint directorySize = U32(data, 44);
            if (directorySize > data.Length) throw new InvalidDataException("Invalid MSF directory size.");
            int directoryPages = checked((int)(((ulong)directorySize + (uint)blockSize - 1) / (uint)blockSize));
            if (directoryPages > blockSize / 4) throw new InvalidDataException("Unsupported oversized MSF directory map.");
            uint map = U32(data, 52);
            var directoryBlocks = new uint[directoryPages];
            int mapOffset = PageOffset(map);
            for (int i = 0; i < directoryPages; i++) directoryBlocks[i] = U32(data, checked(mapOffset + i * 4));
            byte[] directory = ReadPages(directorySize, directoryBlocks);
            uint streams = U32(directory, 0);
            if (streams > (directory.Length - 4) / 4) throw new InvalidDataException("Invalid MSF stream count.");
            sizes = new uint[checked((int)streams)];
            pages = new uint[sizes.Length][];
            int cursor = checked(4 + sizes.Length * 4);
            for (int i = 0; i < sizes.Length; i++)
            {
                sizes[i] = U32(directory, 4 + i * 4);
                if (sizes[i] != uint.MaxValue && sizes[i] > data.Length) throw new InvalidDataException("Invalid MSF stream size.");
                int count = sizes[i] == uint.MaxValue ? 0 : checked((int)(((ulong)sizes[i] + (uint)blockSize - 1) / (uint)blockSize));
                if (count > (directory.Length - cursor) / 4) throw new InvalidDataException("MSF page list truncated.");
                pages[i] = new uint[count];
                for (int j = 0; j < count; j++) { pages[i][j] = U32(directory, cursor); cursor += 4; _ = PageOffset(pages[i][j]); }
            }
            output.WriteLine($"MSF7=True;BlockSize={blockSize};BlockCount={blocks};DirectoryBytes={directorySize};DirectoryPages={directoryPages};StreamCount={streams};DirectoryConsumed={cursor}/{directory.Length}");
        }

        private int PageOffset(uint page)
        {
            if (page >= blocks) throw new InvalidDataException("MSF page outside file.");
            return checked((int)((ulong)page * (uint)blockSize));
        }

        private byte[] ReadPages(uint size, uint[] list)
        {
            byte[] result = new byte[checked((int)size)];
            int position = 0;
            foreach (uint page in list)
            {
                int count = Math.Min(blockSize, result.Length - position);
                data.AsSpan(PageOffset(page), count).CopyTo(result.AsSpan(position));
                position += count;
            }
            if (position != result.Length) throw new InvalidDataException("MSF stream incomplete.");
            return result;
        }

        internal byte[] Stream(int index)
        {
            if ((uint)index >= sizes.Length || sizes[index] == uint.MaxValue) throw new InvalidDataException("Missing MSF stream.");
            return ReadPages(sizes[index], pages[index]);
        }
    }

    private static void ReadPublics(Msf msf, PEReader pe, TextWriter output)
    {
        byte[] dbi = msf.Stream(3);
        if (dbi.Length < 64) throw new InvalidDataException("DBI header truncated.");
        int stream = BinaryPrimitives.ReadUInt16LittleEndian(dbi.AsSpan(20, 2));
        byte[] records = msf.Stream(stream);
        uint nearest = 0, next = uint.MaxValue;
        string? nearestName = null;
        int count = 0;
        for (int offset = 0; offset < records.Length;)
        {
            if (offset > records.Length - 4) throw new InvalidDataException("Symbol header truncated.");
            int length = BinaryPrimitives.ReadUInt16LittleEndian(records.AsSpan(offset, 2));
            if (length < 2 || length > records.Length - offset - 2) throw new InvalidDataException("Symbol record truncated.");
            int end = offset + length + 2;
            ushort kind = BinaryPrimitives.ReadUInt16LittleEndian(records.AsSpan(offset + 2, 2));
            if (kind == 0x110E)
            {
                if (length < 13) throw new InvalidDataException("Public record truncated.");
                uint address = U32(records, offset + 8);
                int section = BinaryPrimitives.ReadUInt16LittleEndian(records.AsSpan(offset + 12, 2));
                int terminator = Array.IndexOf(records, (byte)0, offset + 14, end - offset - 14);
                if (terminator < 0 || section < 1 || section > pe.PEHeaders.SectionHeaders.Length) throw new InvalidDataException("Public name or section invalid.");
                var header = pe.PEHeaders.SectionHeaders[section - 1];
                uint extent = checked((uint)Math.Max(header.VirtualSize, header.SizeOfRawData));
                if (address >= extent) throw new InvalidDataException("Public offset outside section.");
                uint rva = checked((uint)header.VirtualAddress + address);
                count++;
                if (rva <= 0xB65F && (nearestName == null || rva > nearest))
                {
                    nearest = rva;
                    nearestName = System.Text.Encoding.UTF8.GetString(records, offset + 14, terminator - offset - 14);
                }
                if (rva > 0xB65F && rva < next) next = rva;
            }
            offset = end;
        }
        output.WriteLine($"PublicsFallbackStream={stream};PublicCount={count};Nearest={nearestName};Rva=0x{nearest:X};Delta=0x{0xB65F - nearest:X};Next=0x{next:X};FunctionAuthenticated=False;Reason=PublicsHaveNoFunctionExtent");
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 8)]
    private struct ModuleInfo
    {
        internal uint SizeOfStruct;
        internal ulong BaseOfImage;
        internal uint ImageSize, TimeDateStamp, CheckSum, NumSyms, SymType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] internal string? ModuleName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] internal string? ImageName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] internal string? LoadedImageName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] internal string? LoadedPdbName;
        internal uint CVSig;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 780)] internal string? CVData;
        internal uint PdbSig;
        internal Guid PdbSig70;
        internal uint PdbAge;
        [MarshalAs(UnmanagedType.Bool)] internal bool PdbUnmatched;
        [MarshalAs(UnmanagedType.Bool)] internal bool DbgUnmatched;
        [MarshalAs(UnmanagedType.Bool)] internal bool LineNumbers;
        [MarshalAs(UnmanagedType.Bool)] internal bool GlobalSymbols;
        [MarshalAs(UnmanagedType.Bool)] internal bool TypeInfo;
        [MarshalAs(UnmanagedType.Bool)] internal bool SourceIndexed;
        [MarshalAs(UnmanagedType.Bool)] internal bool Publics;
        internal uint MachineType, Reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct LineInfo { internal uint SizeOfStruct; internal nint Key; internal uint LineNumber; internal nint FileName; internal ulong Address; }
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool DebugCallback(nint process, uint action, ulong data, ulong context);
    [DllImport("kernel32.dll")] private static extern nint GetCurrentProcess();
    [DllImport("kernel32.dll")] private static extern void SetLastError(uint error);
    [DllImport("dbghelp.dll", SetLastError = true)] private static extern uint SymSetOptions(uint options);
    [DllImport("dbghelp.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymInitialize(nint process, string searchPath, [MarshalAs(UnmanagedType.Bool)] bool invade);
    [DllImport("dbghelp.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymRegisterCallback64(nint process, DebugCallback callback, ulong context);
    [DllImport("dbghelp.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymSetSearchPath(nint process, string path);
    [DllImport("dbghelp.dll", CharSet = CharSet.Ansi, SetLastError = true)] private static extern ulong SymLoadModuleEx(nint process, nint file, string image, string? name, ulong address, uint size, nint data, uint flags);
    [DllImport("dbghelp.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymGetModuleInfo64(nint process, ulong address, ref ModuleInfo info);
    [DllImport("dbghelp.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymFromAddr(nint process, ulong address, out ulong displacement, nint symbol);
    [DllImport("dbghelp.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymGetLineFromAddr64(nint process, ulong address, out uint displacement, ref LineInfo info);
    [DllImport("dbghelp.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymCleanup(nint process);
}
#endif
