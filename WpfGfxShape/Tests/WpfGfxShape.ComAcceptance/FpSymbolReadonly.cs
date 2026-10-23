#if FP_SYMBOL_READONLY
using System.Buffers.Binary;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace WpfGfxShape.ComAcceptance;

internal static class FpSymbolReadonly
{
    private const string ExpectedSha = "1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53";

    private static int Main()
    {
        string root = Path.GetDirectoryName(Environment.ProcessPath) ?? throw new InvalidOperationException("Missing process path.");
        using var output = new StreamWriter(Path.Combine(root, "fp-symbols.txt"));
        output.WriteLine($"ExecutableBytes={new FileInfo(Environment.ProcessPath!).Length}");
        string image = Path.Combine(root, "input", "wpfgfx_cor3.dll");
        string pdb = Path.ChangeExtension(image, ".pdb");
        output.WriteLine($"Process={RuntimeInformation.ProcessArchitecture};PointerSize={IntPtr.Size}");
        using var file = File.OpenRead(image);
        string sha = Convert.ToHexString(SHA256.HashData(file));
        output.WriteLine($"Image={image};Bytes={file.Length};SHA256={sha}");
        if (sha != ExpectedSha) throw new InvalidDataException("FP image identity mismatch.");
        file.Position = 0;
        using var pe = new PEReader(file);
        var header = pe.PEHeaders.PEHeader ?? throw new InvalidDataException("Missing PE header.");
        output.WriteLine($"Machine={pe.PEHeaders.CoffHeader.Machine};Magic={header.Magic};ImageBase=0x{header.ImageBase:X};SizeOfImage=0x{header.SizeOfImage:X}");
        foreach (var section in pe.PEHeaders.SectionHeaders)
            output.WriteLine($"Section={section.Name};RVA=0x{section.VirtualAddress:X};VirtualSize=0x{section.VirtualSize:X};RawSize=0x{section.SizeOfRawData:X}");
        var entries = pe.ReadDebugDirectory().Where(e => e.Type == DebugDirectoryEntryType.CodeView).ToArray();
        if (entries.Length != 1) throw new InvalidDataException("Expected one CodeView entry.");
        var cv = pe.ReadCodeViewDebugDirectoryData(entries[0]);
        output.WriteLine($"CodeView=RSDS;Guid={cv.Guid:D};Age={cv.Age};Path={cv.Path};Stamp=0x{entries[0].Stamp:X}");
        byte[] bytes = File.ReadAllBytes(pdb);
        output.WriteLine($"PDB={pdb};Bytes={bytes.Length};SHA256={Convert.ToHexString(SHA256.HashData(bytes))}");
        (Guid guid, uint age) = ReadPdbIdentity(bytes);
        output.WriteLine($"PDBFormat=MSF7;Guid={guid:D};Age={age};IdentityMatch={guid == cv.Guid && age == cv.Age}");
        output.Flush();
        if (guid != cv.Guid || age != cv.Age) throw new InvalidDataException("FP PDB identity mismatch.");
        nint process = GetCurrentProcess();
        // Do not consult environment paths, symbol servers, or permissive mismatched symbols.
        uint options = 0x10 | 0x200 | 0x400 | 0x1000 | 0x20000 | 0x40000 | 0x80000;
        output.WriteLine($"SymOptions=0x{SymSetOptions(options):X}");
        bool initialized = SymInitialize(process, Path.GetDirectoryName(pdb)!, false);
        output.WriteLine($"SymInitialize={initialized};Error={(initialized ? 0 : Marshal.GetLastWin32Error())}");
        if (!initialized) return 2;
        try
        {
            const ulong loadBase = 0x6E630000;
            ulong loaded = SymLoadModuleEx(process, 0, image, null, loadBase, (uint)header.SizeOfImage, 0, 0);
            output.WriteLine($"SymLoadModuleEx=0x{loaded:X};RequestedBase=0x{loadBase:X};Error={(loaded == 0 ? Marshal.GetLastWin32Error() : 0)}");
            output.Flush();
            if (loaded != loadBase) return 3;
            var module = new ImagehlpModule64 { SizeOfStruct = (uint)Marshal.SizeOf<ImagehlpModule64>() };
            bool moduleFound = SymGetModuleInfo64(process, loaded, ref module);
            int moduleError = moduleFound ? 0 : Marshal.GetLastWin32Error();
            output.WriteLine($"SymGetModuleInfo64={moduleFound};Error={moduleError};StructSize={Marshal.SizeOf<ImagehlpModule64>()};LoadedPdbName={module.LoadedPdbName};PdbUnmatched={module.PdbUnmatched};DbgUnmatched={module.DbgUnmatched};PdbSig70={module.PdbSig70:D};PdbAge={module.PdbAge};SymType={module.SymType};LineNumbers={module.LineNumbers}");
            bool authenticated = moduleFound && !module.PdbUnmatched && module.PdbSig70 == guid && module.PdbAge == age && string.Equals(Path.GetFullPath(module.LoadedPdbName ?? ""), Path.GetFullPath(pdb), StringComparison.OrdinalIgnoreCase);
            output.WriteLine($"ActualLoadedPdbAuthenticated={authenticated};ExpectedPdb={pdb}");
            if (!authenticated) return 4;
            foreach (uint rva in new uint[] { 0x1351F0, 0x1DE4D1 })
            {
                ulong address = loaded + rva;
                nint symbol = Marshal.AllocHGlobal(4096);
                try
                {
                    Marshal.Copy(new byte[4096], 0, symbol, 4096);
                    Marshal.WriteInt32(symbol, 88);
                    Marshal.WriteInt32(symbol, 80, 3900);
                    bool found = SymFromAddr(process, address, out ulong displacement, symbol);
                    int error = found ? 0 : Marshal.GetLastWin32Error();
                    string name = found ? Marshal.PtrToStringAnsi(symbol + 84, Marshal.ReadInt32(symbol, 76)) ?? "" : "";
                    output.WriteLine($"RVA=0x{rva:X};VA=0x{address:X};SymFromAddr={found};Error={error};Name={name};Displacement=0x{displacement:X};SymbolAddress=0x{(ulong)Marshal.ReadInt64(symbol, 56):X};ModuleBase=0x{(ulong)Marshal.ReadInt64(symbol, 32):X};Size={Marshal.ReadInt32(symbol, 28)};Flags=0x{Marshal.ReadInt32(symbol, 48):X}");
                    var line = new ImagehlpLine64 { SizeOfStruct = (uint)Marshal.SizeOf<ImagehlpLine64>() };
                    bool hasLine = SymGetLineFromAddr64(process, address, out uint lineDisplacement, ref line);
                    output.WriteLine($"RVA=0x{rva:X};SymGetLineFromAddr64={hasLine};Error={(hasLine ? 0 : Marshal.GetLastWin32Error())};File={Marshal.PtrToStringAnsi(line.FileName)};Line={line.LineNumber};Displacement=0x{lineDisplacement:X};LineAddress=0x{line.Address:X}");
                    if (hasLine && rva == 0x1351F0)
                    {
                        string source = Marshal.PtrToStringAnsi(line.FileName) ?? "";
                        WriteChecksum(process, loaded, source, root, output);
                        var previous = line;
                        var next = line;
                        bool previousFound = SymGetLinePrev64(process, ref previous);
                        int previousError = previousFound ? 0 : Marshal.GetLastWin32Error();
                        output.WriteLine($"PreviousLine={previousFound};Error={previousError};File={Marshal.PtrToStringAnsi(previous.FileName)};Line={previous.LineNumber};Address=0x{previous.Address:X}");
                        bool nextFound = SymGetLineNext64(process, ref next);
                        int nextError = nextFound ? 0 : Marshal.GetLastWin32Error();
                        output.WriteLine($"NextLine={nextFound};Error={nextError};File={Marshal.PtrToStringAnsi(next.FileName)};Line={next.LineNumber};Address=0x{next.Address:X};NotAnInlineOrStatementBoundary=True");
                    }
                }
                finally { Marshal.FreeHGlobal(symbol); }
            }
        }
        finally
        {
            bool cleaned = SymCleanup(process);
            output.WriteLine($"SymCleanup={cleaned};Error={(cleaned ? 0 : Marshal.GetLastWin32Error())}");
        }
        return 0;
    }

    private static void WriteChecksum(nint process, ulong loaded, string source, string root, TextWriter output)
    {
        nint library = NativeLibrary.Load("dbghelp.dll");
        try
        {
            if (!NativeLibrary.TryGetExport(library, "SymGetSourceFileChecksum", out nint address))
            {
                output.WriteLine("SourceChecksum=Unavailable;Reason=DbgHelpExportMissing;Algorithm=Unavailable;SourceBytesAuthenticated=False");
                return;
            }
            var checksum = Marshal.GetDelegateForFunctionPointer<SourceChecksum>(address);
            byte[] buffer = new byte[64];
            bool found = checksum(process, loaded, source, out uint type, buffer, (uint)buffer.Length, out uint actual);
            int error = found ? 0 : Marshal.GetLastWin32Error();
            output.WriteLine($"SourceChecksumApi={found};Error={error};AlgorithmId={type};Bytes={actual}");
            if (!found || actual > buffer.Length) return;
            string algorithm = type switch { 1 => "MD5", 2 => "SHA1", 3 => "SHA256", _ => "Unsupported" };
            output.WriteLine($"SourceChecksum={Convert.ToHexString(buffer.AsSpan(0, (int)actual))};Algorithm={algorithm};File={source}");
            string snapshot = Path.Combine(root, "input", "fe-production-experiment.cs.txt");
            if (!File.Exists(snapshot))
            {
                output.WriteLine("SnapshotComparison=Unavailable;Reason=OriginalSnapshotNotPresent;SourceBytesAuthenticated=False");
                return;
            }
            byte[] bytes = File.ReadAllBytes(snapshot);
            byte[]? hash = type switch { 1 => MD5.HashData(bytes), 2 => SHA1.HashData(bytes), 3 => SHA256.HashData(bytes), _ => null };
            output.WriteLine($"SnapshotBytes={bytes.Length};Algorithm={algorithm};SnapshotHash={(hash is null ? "Unavailable" : Convert.ToHexString(hash))};SourceBytesAuthenticated={hash is not null && hash.AsSpan().SequenceEqual(buffer.AsSpan(0, (int)actual))}");
        }
        finally { NativeLibrary.Free(library); }
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool SourceChecksum(nint process, ulong baseOfDll, string fileName, out uint checksumType, [Out] byte[] buffer, uint size, out uint required);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 8)]
    private struct ImagehlpModule64
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

    [DllImport("dbghelp.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymGetModuleInfo64(nint process, ulong address, ref ImagehlpModule64 module);
    [DllImport("dbghelp.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymGetLinePrev64(nint process, ref ImagehlpLine64 line);
    [DllImport("dbghelp.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymGetLineNext64(nint process, ref ImagehlpLine64 line);

    private static (Guid Guid, uint Age) ReadPdbIdentity(byte[] data)
    {
        if (!Encoding.ASCII.GetString(data, 0, 32).StartsWith("Microsoft C/C++ MSF 7.00\r\n\u001aDS", StringComparison.Ordinal))
            throw new InvalidDataException("Not MSF7 PDB.");
        int blockSize = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(32)));
        int directorySize = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(44)));
        int blockMap = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(52)));
        byte[] directory = new byte[directorySize];
        for (int offset = 0, index = 0; offset < directorySize; offset += blockSize, index++)
        {
            int block = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(checked(blockMap * blockSize + index * 4))));
            data.AsSpan(checked(block * blockSize), Math.Min(blockSize, directorySize - offset)).CopyTo(directory.AsSpan(offset));
        }
        int count = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(directory));
        int cursor = checked(4 + count * 4);
        for (int stream = 0; stream < count; stream++)
        {
            uint length = BinaryPrimitives.ReadUInt32LittleEndian(directory.AsSpan(4 + stream * 4));
            int blocks = length == uint.MaxValue ? 0 : checked((int)((length + blockSize - 1) / blockSize));
            if (stream == 1)
            {
                if (length < 28) throw new InvalidDataException("PDB info stream too short.");
                int block = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(directory.AsSpan(cursor)));
                ReadOnlySpan<byte> info = data.AsSpan(checked(block * blockSize), 28);
                return (new Guid(info.Slice(12, 16)), BinaryPrimitives.ReadUInt32LittleEndian(info.Slice(8, 4)));
            }
            cursor = checked(cursor + blocks * 4);
        }
        throw new InvalidDataException("Missing PDB info stream.");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ImagehlpLine64
    {
        internal uint SizeOfStruct;
        internal nint Key;
        internal uint LineNumber;
        internal nint FileName;
        internal ulong Address;
    }

    [DllImport("kernel32.dll")] private static extern nint GetCurrentProcess();
    [DllImport("dbghelp.dll")] private static extern uint SymSetOptions(uint options);
    [DllImport("dbghelp.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymInitialize(nint process, string searchPath, [MarshalAs(UnmanagedType.Bool)] bool invadeProcess);
    [DllImport("dbghelp.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern ulong SymLoadModuleEx(nint process, nint file, string imageName, string? moduleName, ulong baseOfDll, uint dllSize, nint data, uint flags);
    [DllImport("dbghelp.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymFromAddr(nint process, ulong address, out ulong displacement, nint symbol);
    [DllImport("dbghelp.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymGetLineFromAddr64(nint process, ulong address, out uint displacement, ref ImagehlpLine64 line);
    [DllImport("dbghelp.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SymCleanup(nint process);
}
#endif
