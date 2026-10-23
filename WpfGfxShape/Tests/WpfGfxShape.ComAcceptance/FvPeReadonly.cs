#if FV_PE_READONLY
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WpfGfxShape.ComAcceptance;

internal static class FvPeReadonly
{
    private const string ExpectedSha = "1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53";

    private static int Main(string[] args)
    {
        bool calleeWindow = args.Length == 1 && args[0] == "--fw-callee-window";
        bool helperWindow = args.Length == 1 && args[0] == "--gu-helper-window";
        if (args.Length != 0 && !calleeWindow && !helperWindow)
            throw new ArgumentException("FV accepts only --fw-callee-window, --gu-helper-window or no arguments.", nameof(args));
        string executable = Environment.ProcessPath ?? throw new InvalidOperationException("Missing process path.");
        string root = Path.GetDirectoryName(executable) ?? throw new InvalidOperationException("Missing directory.");
        string result = Path.Combine(root, "result");
        Directory.CreateDirectory(result);
        using var output = new StreamWriter(Path.Combine(result, "fv-pe.txt"));
        output.AutoFlush = true;
        using (var self = File.OpenRead(executable))
            output.WriteLine($"TestFile={executable};Bytes={self.Length};SHA256={Convert.ToHexString(SHA256.HashData(self))};MVID={typeof(FvPeReadonly).Module.ModuleVersionId:D}");
        output.WriteLine($"Runtime={RuntimeInformation.FrameworkDescription};Process={RuntimeInformation.ProcessArchitecture};PointerSize={IntPtr.Size};Role=FV_PE_READONLY;CandidateLoaded=false");
        string image = Path.Combine(root, "input", "wpfgfx_cor3.dll");
        using var file = new FileStream(image, FileMode.Open, FileAccess.Read, FileShare.Read);
        string sha = Convert.ToHexString(SHA256.HashData(file));
        output.WriteLine($"Input={image};Bytes={file.Length};SHA256={sha};ExpectedSHA256={ExpectedSha}");
        if (sha != ExpectedSha) throw new InvalidDataException("FV image identity mismatch.");
        file.Position = 0;
        using var pe = new PEReader(file, PEStreamOptions.LeaveOpen);
        PEHeaders headers = pe.PEHeaders;
        PEHeader header = headers.PEHeader ?? throw new InvalidDataException("Missing PE header.");
        output.WriteLine($"Machine={headers.CoffHeader.Machine};Magic={header.Magic};SizeOfImage=0x{header.SizeOfImage:X};ImageBase=0x{header.ImageBase:X};SizeOfHeaders=0x{header.SizeOfHeaders:X}");
        if (headers.CoffHeader.Machine != Machine.I386 || header.Magic != PEMagic.PE32)
            throw new InvalidDataException("FV requires I386 PE32.");
        foreach (SectionHeader section in headers.SectionHeaders)
            output.WriteLine($"Section={section.Name};RVA=0x{section.VirtualAddress:X};VirtualSize=0x{section.VirtualSize:X};RawOffset=0x{section.PointerToRawData:X};RawSize=0x{section.SizeOfRawData:X};Characteristics={section.SectionCharacteristics}");
        if (helperWindow)
        {
            output.WriteLine("Scope=GU;FunctionRangeAuthenticated=false;BoundaryBasis=direct-call-target;WindowIsNotCompleteFunction=true;MaximumBytes=128");
            Extract("helper-window", 0x1DA460, 0x1DA4E0);
        }
        else if (calleeWindow)
        {
            output.WriteLine("Scope=FW;FunctionRangeAuthenticated=false;BoundaryBasis=direct-call-target;WindowIsNotCompleteFunction=true;MaximumBytes=384");
            Extract("callee-window", 0x135570, 0x1356F0);
        }
        else
        {
            Extract("create", 0x134F70, 0x135362);
            Extract("first-av-window", 0x1351D0, 0x135230);
        }
        output.WriteLine("ReadonlyExtractionComplete=true;RoleResult=0;NotMTP=true;DynamicExecutionEvidence=false");
        return 0;

        void Extract(string name, int start, int end)
        {
            if (start < header.SizeOfHeaders || end <= start || end > header.SizeOfImage)
                throw new InvalidDataException("FV range outside image.");
            SectionHeader[] matches = headers.SectionHeaders.Where(section =>
                start >= section.VirtualAddress && (long)end <= (long)section.VirtualAddress + section.SizeOfRawData).ToArray();
            if (matches.Length != 1) throw new InvalidDataException("FV range lacks unique raw section backing.");
            SectionHeader section = matches[0];
            if (calleeWindow && (name != "callee-window" || start != 0x135570 || end != 0x1356F0 ||
                section.Name != ".text" || end - start > 384))
                throw new InvalidDataException("FW range outside fixed text window whitelist.");
            if (helperWindow && (name != "helper-window" || start != 0x1DA460 || end != 0x1DA4E0 ||
                section.Name != ".text" || end - start > 128))
                throw new InvalidDataException("GU range outside fixed text window whitelist.");
            long offset = checked((long)section.PointerToRawData + start - section.VirtualAddress);
            int length = checked(end - start);
            if (section.PointerToRawData < 0 || offset < section.PointerToRawData || offset + length > file.Length ||
                offset + length > (long)section.PointerToRawData + section.SizeOfRawData)
                throw new InvalidDataException("FV raw range exceeds file backing.");
            file.Position = offset;
            byte[] bytes = new byte[length];
            file.ReadExactly(bytes);
            if (start <= 0x1351F0 && end >= 0x1351F6 &&
                Convert.ToHexString(bytes.AsSpan(0x1351F0 - start, 6)) != "8BB568FFFFFF")
                throw new InvalidDataException("FV first AV bytes mismatch.");
            string binary = Path.Combine(result, name + ".bin");
            File.WriteAllBytes(binary, bytes);
            output.WriteLine($"Segment={name};RVA=0x{start:X};EndExclusive=0x{end:X};FileOffset=0x{offset:X};Length={length};Section={section.Name};SHA256={Convert.ToHexString(SHA256.HashData(bytes))};Bin={Path.GetFileName(binary)}");
            for (int index = 0; index < bytes.Length; index += 16)
                output.WriteLine($"RVA={start + index:X8};Offset={offset + index:X8};Length={Math.Min(16, bytes.Length - index)};Hex={Convert.ToHexString(bytes.AsSpan(index, Math.Min(16, bytes.Length - index)))}");
        }
    }
}
#endif
