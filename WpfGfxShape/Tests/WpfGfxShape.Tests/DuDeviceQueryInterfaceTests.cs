#if DU_DEVICE_QI
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class DuDeviceQueryInterfaceTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Parameters
    {
        internal uint Width, Height, Format, Count, Multisample, Quality, SwapEffect;
        internal nint Window;
        internal int Windowed, AutoDepth;
        internal uint DepthFormat, Flags, Refresh, Interval;
    }

    [DllImport("d3d9.dll", ExactSpelling = true)]
    private static extern nint Direct3DCreate9(uint version);
    [DllImport("user32.dll")]
    private static extern nint GetDesktopWindow();

    private static void Record(string stage, object data)
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(directory);
        using var stream = new FileStream(Path.Combine(directory, "du-qi.jsonl"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.WriteLine(JsonSerializer.Serialize(new { Stage = stage, Utc = DateTimeOffset.UtcNow, Pid = Environment.ProcessId, Data = data }));
        writer.Flush();
        stream.Flush(true);
    }

    private static uint Release(nint value) => value == 0 ? 0 : ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);

    /// <summary>Compares independent COM Stdcall QI with the exact generated binding on an owned real device.</summary>
    [TestMethod]
    public void WhenDuOwnedDeviceIsQueriedThenDirectAndBindingResultsAgree()
    {
        string assembly = typeof(DuDeviceQueryInterfaceTests).Assembly.Location;
        Record("identity", new { Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(), PointerSize = sizeof(nint), TestSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly))), Mvid = typeof(DuDeviceQueryInterfaceTests).Module.ModuleVersionId, Silk = typeof(IDirect3DDevice9).Assembly.FullName, SilkSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(IDirect3DDevice9).Assembly.Location))) });
        MethodInfo method = typeof(IDirect3DDevice9).GetMethod("QueryInterface", [typeof(Guid*), typeof(void**)])!;
        byte[] il = method.GetMethodBody()!.GetILAsByteArray()!;
        int calliToken = BitConverter.ToInt32(il, il.Length - 5);
        Record("binding-metadata", new { Method = method.ToString(), Attributes = method.GetCustomAttributesData().Select(a => a.ToString()).ToArray(), Il = Convert.ToHexString(il), CalliSignature = Convert.ToHexString(method.Module.ResolveSignature(calliToken)), LocalSignature = method.GetMethodBody()!.LocalSignatureMetadataToken });
        nint factory = 0, device = 0, surface = 0, owned = 0;
        try
        {
            factory = Direct3DCreate9(32);
            Assert.AreNotEqual((nint)0, factory);
            Parameters parameters = new() { Width = 12, Height = 3, Count = 1, SwapEffect = 1, Window = GetDesktopWindow(), Windowed = 1 };
            int hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, Parameters*, nint*, int>)(*(void***)factory)[16])(factory, 0, 1, parameters.Window, 0x26, &parameters, &device);
            Record("create-device", new { Hr = hr, Device = (long)device });
            Assert.AreEqual(0, hr);
            hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, uint, int, nint*, nint, int>)(*(void***)device)[28])(device, 12, 3, 21, 0, 0, 1, &surface, 0);
            Record("create-surface", new { Hr = hr, Surface = (long)surface });
            Assert.AreEqual(0, hr);
            hr = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)surface)[3])(surface, &owned);
            Record("get-device", new { Hr = hr, Owned = (long)owned, Original = (long)device });
            Assert.AreEqual(0, hr);
            Assert.AreEqual(device, owned);
            Guid iid = new("b18b10ce-2649-405a-870f-95f777d4313a");
            Assert.AreEqual(iid, IDirect3DDevice9Ex.Guid);
            void** table = *(void***)owned;
            Record("qi-before", new { Device = (long)owned, Vtable = (long)table, Slot0 = (long)table[0], Iid = iid, IidAddress = (long)&iid, Modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => (nuint)table[0] >= (nuint)m.BaseAddress && (nuint)table[0] < (nuint)m.BaseAddress + (nuint)m.ModuleMemorySize).Select(m => new { m.FileName, BaseAddress = (long)m.BaseAddress, m.ModuleMemorySize, Sha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(m.FileName))) }).ToArray() });
            nint direct = 0;
            int directHr = ((delegate* unmanaged[Stdcall]<nint, Guid*, void**, int>)table[0])(owned, &iid, (void**)&direct);
            Record("direct-qi-after", new { Hr = directHr, Output = (long)direct });
            if (direct != 0) Record("direct-qi-release", new { Count = Release(direct) });
            nint binding = 0;
            Record("binding-qi-before", new { Device = (long)owned });
            int bindingHr = ((IDirect3DDevice9*)owned)->QueryInterface(&iid, (void**)&binding);
            Record("binding-qi-after", new { Hr = bindingHr, Output = (long)binding });
            if (binding != 0) Record("binding-qi-release", new { Count = Release(binding) });
            Assert.AreEqual(directHr, bindingHr);
            Record("completed", new { DirectHr = directHr, BindingHr = bindingHr });
        }
        finally
        {
            Record("release-owned", new { Count = Release(owned) });
            Record("release-surface", new { Count = Release(surface) });
            Record("release-device", new { Count = Release(device) });
            Record("release-factory", new { Count = Release(factory) });
        }
    }
}
#endif
