#if SAMPLER_OFFLINE_SYMBOLS
using System.Buffers.Binary;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WpfGfxShape.ComAcceptance;

internal static partial class SamplerOfflineSymbols
{
    private static ReadMemoryCallback? offlineRead;
    private static string? callbackFailure;

    private static unsafe void WalkOriginal(nint process, ulong imageBase, int imageSize, PEReader pe, byte[] originalContext, byte[] dump, List<(ulong Start, ulong Length, ulong Data)> memory)
    {
        using var log = new StreamWriter(Path.Combine(AppContext.BaseDirectory, "Results", "sampler-offline-symbols", "unwind.txt")) { AutoFlush = true };
        const string imageHash = "AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1";
        byte[] image = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "wpfgfx_cor3.dll"));
        if (Convert.ToHexString(SHA256.HashData(image)) != imageHash) throw new InvalidDataException("Unwind image identity mismatch.");
        var header = pe.PEHeaders.PEHeader ?? throw new InvalidDataException("Missing PE header.");
        if (pe.PEHeaders.CoffHeader.Machine != Machine.Amd64 || header.Magic != PEMagic.PE32Plus || header.SizeOfImage != imageSize || imageSize <= 0 || header.SizeOfHeaders <= 0 || header.SizeOfHeaders > image.Length || header.SizeOfHeaders > imageSize || imageBase > ulong.MaxValue - (uint)imageSize)
            throw new InvalidDataException("Invalid AMD64 image bounds.");
        var sections = pe.PEHeaders.SectionHeaders;
        long previousEnd = header.SizeOfHeaders;
        foreach (var section in sections.OrderBy(s => s.VirtualAddress))
        {
            long extent = Math.Max(section.VirtualSize, section.SizeOfRawData);
            if (section.VirtualAddress < previousEnd || section.VirtualSize < 0 || section.SizeOfRawData < 0 || section.PointerToRawData < 0 || (long)section.VirtualAddress + extent > imageSize || (long)section.PointerToRawData + section.SizeOfRawData > image.Length)
                throw new InvalidDataException($"Invalid or overlapping PE section {section.Name}.");
            previousEnd = (long)section.VirtualAddress + extent;
            log.WriteLine($"Section={section.Name};Rva=0x{section.VirtualAddress:X};VirtualSize=0x{section.VirtualSize:X};RawSize=0x{section.SizeOfRawData:X};RawPointer=0x{section.PointerToRawData:X};Characteristics=0x{(uint)section.SectionCharacteristics:X}");
        }
        bool InImage(ulong address, ulong size = 1) => address >= imageBase && address - imageBase < (uint)imageSize && size <= (uint)imageSize - (address - imageBase);
        bool Map(uint rva, uint count, out byte[] data, out string source)
        {
            data = []; source = "UnmappedOrDisallowedImageRange";
            if (count == 0 || rva >= imageSize || count > (uint)imageSize - rva) return false;
            if (rva < header.SizeOfHeaders)
            {
                if (count > (uint)header.SizeOfHeaders - rva) return false;
                data = image.AsSpan(checked((int)rva), checked((int)count)).ToArray(); source = $"ImageHeader;FileOffset=0x{rva:X}"; return true;
            }
            foreach (var s in sections)
            {
                ulong start = (uint)s.VirtualAddress, extent = (uint)Math.Max(s.VirtualSize, s.SizeOfRawData);
                if (rva < start || rva - start >= extent) continue;
                ulong delta = rva - start;
                if (count > extent - delta) { source = "CrossSectionOrPadding"; return false; }
                bool allowed = (s.SectionCharacteristics & SectionCharacteristics.MemRead) != 0 && (s.SectionCharacteristics & SectionCharacteristics.MemWrite) == 0 && (s.Name is ".pdata" or ".xdata" or ".rdata" or ".text");
                if (!allowed) { source = "NotReadOnlyUnwindOrTextSection"; return false; }
                data = new byte[checked((int)count)];
                int rawCount = delta < (uint)s.SizeOfRawData ? checked((int)Math.Min(count, (uint)s.SizeOfRawData - delta)) : 0;
                if ((ulong)rawCount < count && (s.VirtualSize <= s.SizeOfRawData || delta + count > (uint)s.VirtualSize)) { data = []; source = "OutsideDefinedVirtualTail"; return false; }
                ulong fileOffset = checked((uint)s.PointerToRawData + delta);
                if (rawCount > 0) image.AsSpan(checked((int)fileOffset), rawCount).CopyTo(data);
                source = $"ImageSection={s.Name};FileOffset=0x{fileOffset:X};RawBytes={rawCount};DefinedVirtualTailBytes={count - (uint)rawCount}";
                return true;
            }
            return false;
        }
        ulong Q(int offset) => BinaryPrimitives.ReadUInt64LittleEndian(originalContext.AsSpan(offset, 8));
        bool Captured(ulong address, uint size) => memory.Any(r => address >= r.Start && address - r.Start < r.Length && size <= r.Length - (address - r.Start));
        log.WriteLine($"Mode=DumpFirstAuthenticatedImageFallback;ProcessToken=0x{process:X};ContextBytes={originalContext.Length};Flags=0x{U32(originalContext,48):X};StackFrameSize={Marshal.SizeOf<WalkFrame>()};ImageSHA256={imageHash};RSDS=8756f755-10f4-48be-9886-22c9a80c75b8;Age=1;ImageBase=0x{imageBase:X};ImageSize={imageSize};HeapStackFallback=False");
        bool blocked = false;
        callbackFailure = null;
        ReadMemoryCallback read = (nint _, ulong address, nint buffer, uint requested, out uint actual) =>
        {
            actual = 0;
            try
            {
                if (requested == 0 || requested > int.MaxValue || address > ulong.MaxValue - requested) throw new InvalidDataException("Invalid read range.");
                foreach (var range in memory)
                {
                    if (address < range.Start || address - range.Start >= range.Length || requested > range.Length - (address - range.Start)) continue;
                    ulong offset = checked(range.Data + address - range.Start);
                    if (offset > (ulong)dump.Length || requested > (ulong)dump.Length - offset) throw new InvalidDataException("Dump read outside file.");
                    Marshal.Copy(dump, checked((int)offset), buffer, checked((int)requested)); actual = requested;
                    log.WriteLine($"ReadMemory=True;Source=Dump;VA=0x{address:X};End=0x{address + requested:X};Requested={requested};Actual={actual};DumpRva=0x{offset:X};DumpSHA256=1851A7517FFE42FB688BD098EBF463624F59180BF183AA71ABD90E872451B85C");
                    return true;
                }
                string source = "OutsideAuthenticatedImage";
                if (InImage(address, requested) && Map(checked((uint)(address - imageBase)), requested, out byte[] data, out source))
                {
                    Marshal.Copy(data, 0, buffer, data.Length); actual = requested;
                    log.WriteLine($"ReadMemory=True;Source={source};VA=0x{address:X};End=0x{address + requested:X};Requested={requested};Actual={actual};ImageSHA256={imageHash};RSDS=8756f755-10f4-48be-9886-22c9a80c75b8;Age=1"); return true;
                }
                blocked = true;
                log.WriteLine($"ReadMemory=False;VA=0x{address:X};Requested={requested};Actual=0;Reason=NotCapturedInDump/{source};LiveRead=False"); return false;
            }
            catch (Exception error) { callbackFailure = error.ToString(); blocked = true; return false; }
        };
        nint Table(ulong address)
        {
            if (!InImage(address)) { blocked = true; log.WriteLine($"FunctionTable=0;PC=0x{address:X};Reason=UnknownModuleImage"); return 0; }
            SetLastError(0);
            nint value = SymFunctionTableAccess64(process, address);
            int error = Marshal.GetLastWin32Error();
            log.WriteLine($"SymFunctionTableAccess64=0x{value:X};LastError={error};PC=0x{address:X}");
            if (value == 0 || ((nuint)value & 3) != 0) { blocked = true; return 0; }
            uint begin = (uint)Marshal.ReadInt32(value), end = (uint)Marshal.ReadInt32(value, 4), unwind = (uint)Marshal.ReadInt32(value, 8);
            var directory = header.ExceptionTableDirectory;
            bool inTable = false;
            if (directory.RelativeVirtualAddress >= 0 && directory.Size > 0 && directory.Size % 12 == 0 && Map((uint)directory.RelativeVirtualAddress, (uint)directory.Size, out byte[] records, out string tableSource))
            {
                for (int offset = 0; offset < records.Length; offset += 12)
                    if (U32(records, offset) == begin && U32(records, offset + 4) == end && U32(records, offset + 8) == unwind) { inTable = true; break; }
                log.WriteLine($"TableEvidence={tableSource};Rva=0x{directory.RelativeVirtualAddress:X};Bytes={directory.Size};ImageSHA256={imageHash}");
            }
            bool valid = inTable && begin < end && end <= imageSize && (unwind & 3) == 0 && Map(unwind, 4, out byte[] _, out string _) && address - imageBase >= begin && address - imageBase < end;
            log.WriteLine($"RuntimeFunctionBegin=0x{begin:X};End=0x{end:X};UnwindRva=0x{unwind:X};MatchesImagePdata={inTable};Valid={valid};ImageSHA256={imageHash}");
            if (!valid) { blocked = true; return 0; }
            return value;
        }
        TableCallback table = (_, address) => { try { return Table(address); } catch (Exception error) { callbackFailure = error.ToString(); blocked = true; return 0; } };
        BaseCallback module = (_, address) =>
        {
            try
            {
                if (!InImage(address)) { blocked = true; log.WriteLine($"ModuleBase=0;PC=0x{address:X};Reason=UnknownModuleImage"); return 0; }
                ulong value = SymGetModuleBase64(process, address);
                log.WriteLine($"ModuleBase=0x{value:X};PC=0x{address:X};Authenticated={value == imageBase}");
                if (value != imageBase) blocked = true;
                return value == imageBase ? value : 0;
            }
            catch (Exception error) { callbackFailure = error.ToString(); blocked = true; return 0; }
        };
        nint allocation = Marshal.AllocHGlobal(1232 + 15);
        nint context = (nint)(((nuint)allocation + 15) & ~(nuint)15);
        nint symbol = Marshal.AllocHGlobal(4096);
        offlineRead = read;
        try
        {
            Marshal.Copy(originalContext, 0, context, 1232);
            var frame = new WalkFrame { PC = new Address { Offset = Q(248), Mode = 3 }, Stack = new Address { Offset = Q(152), Mode = 3 }, Frame = new Address { Offset = Q(160), Mode = 3 } };
            log.WriteLine($"ContextAlignment={(nuint)context % 16};OriginalPC=0x{frame.PC.Offset:X};OriginalSP=0x{frame.Stack.Offset:X};OriginalFP=0x{frame.Frame.Offset:X};OriginalRCX=0x{Q(128):X};CallerVolatileRegistersNotArguments=True");
            ulong previousPC = 0, previousSP = 0, expectedPC = Q(248);
            for (int step = 0; step < 24; step++)
            {
                if (table(process, expectedPC) == 0) { log.WriteLine("Stop=MissingUnwindMetadata;NoLeafGuessOrScan=True"); break; }
                SetLastError(0);
                bool ok = StackWalk64(0x8664, process, (nint)168, ref frame, context, read, table, module, 0);
                int error = Marshal.GetLastWin32Error();
                log.WriteLine($"Step={step};StackWalk64={ok};LastError={error};LastErrorNotAuthoritative=True;PC=0x{frame.PC.Offset:X};SP=0x{frame.Stack.Offset:X};FP=0x{frame.Frame.Offset:X};Return=0x{frame.Return.Offset:X};ContextPC=0x{(ulong)Marshal.ReadInt64(context,248):X};ContextSP=0x{(ulong)Marshal.ReadInt64(context,152):X};Blocked={blocked};CallbackFailure={callbackFailure}");
                if (!ok || blocked || callbackFailure != null) { log.WriteLine("Stop=ApiOrMemoryOrImageFailure;NoSyntheticFrames=True"); break; }
                bool progress = frame.PC.Offset == expectedPC && InImage(frame.PC.Offset) && Captured(frame.Stack.Offset, 8) && (frame.Stack.Offset & 7) == 0 && (step == 0 ? frame.Stack.Offset == Q(152) : frame.Stack.Offset > previousSP && frame.PC.Offset != previousPC);
                bool returnModule = InImage(frame.Return.Offset) && table(process, frame.Return.Offset) != 0;
                log.WriteLine($"FrameValidation={progress};ReturnModuleAuthenticated={returnModule};ReturnRva=0x{(InImage(frame.Return.Offset) ? frame.Return.Offset-imageBase : 0):X};NoStackScan=True");
                if (!progress || !returnModule || blocked) { log.WriteLine("Stop=FrameOrReturnNotAuthenticated;NoSyntheticFrames=True"); break; }
                Marshal.Copy(new byte[4096], 0, symbol, 4096);
                Marshal.WriteInt32(symbol, 88); Marshal.WriteInt32(symbol, 80, 4000);
                bool found = SymFromAddr(process, frame.PC.Offset, out ulong delta, symbol);
                int length = Marshal.ReadInt32(symbol, 76);
                string? name = found && length is >= 0 and <= 4000 ? Marshal.PtrToStringAnsi(symbol + 84, length) : null;
                var line = new LineInfo { SizeOfStruct = (uint)Marshal.SizeOf<LineInfo>() };
                bool hasLine = SymGetLineFromAddr64(process, frame.PC.Offset, out uint lineDelta, ref line);
                log.WriteLine($"UnwoundFrame={step};PC=0x{frame.PC.Offset:X};SP=0x{frame.Stack.Offset:X};FP=0x{frame.Frame.Offset:X};Function={name};Offset=0x{delta:X};PdbSource={(hasLine ? Marshal.PtrToStringAnsi(line.FileName) : null)};Line={line.LineNumber};LineOffset={lineDelta};OriginalFaultContext={step == 0};RawStackCandidate=False;ImageSHA256={imageHash};ModuleRSDS=8756f755-10f4-48be-9886-22c9a80c75b8;Age=1");
                previousPC = frame.PC.Offset; previousSP = frame.Stack.Offset; expectedPC = frame.Return.Offset;
            }
        }
        finally
        {
            log.WriteLine($"FinalCallbackFailure={callbackFailure};Blocked={blocked}");
            offlineRead = null;
            Marshal.FreeHGlobal(symbol); Marshal.FreeHGlobal(allocation);
            GC.KeepAlive(read); GC.KeepAlive(table); GC.KeepAlive(module);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Address { internal ulong Offset; internal ushort Segment; internal uint Mode; }
    [StructLayout(LayoutKind.Explicit, Size = 264)]
    private struct WalkFrame
    {
        [FieldOffset(0)] internal Address PC;
        [FieldOffset(16)] internal Address Return;
        [FieldOffset(32)] internal Address Frame;
        [FieldOffset(48)] internal Address Stack;
    }
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool ReadMemoryCallback(nint process, ulong address, nint buffer, uint size, out uint read);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate nint TableCallback(nint process, ulong address);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate ulong BaseCallback(nint process, ulong address);
    [DllImport("dbghelp.dll", SetLastError = true)] private static extern nint SymFunctionTableAccess64(nint process, ulong address);
    [DllImport("dbghelp.dll", SetLastError = true)] private static extern ulong SymGetModuleBase64(nint process, ulong address);
    [DllImport("dbghelp.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool StackWalk64(uint machine, nint process, nint thread, ref WalkFrame frame, nint context, ReadMemoryCallback read, TableCallback table, BaseCallback module, nint translate);
}
#endif
