using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern uint GetModuleFileNameW(nint module, char* path, uint length);

    /// <summary>Checks the same two-input hardware scene across real HWND size changes in both modules.</summary>
    [TestMethod]
    public void WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive()
        => RunVisualCacheConsumer(true, 0xff00ff00, 0x0000ff80, false, resizeWindow: true);

    /// <summary>Executes the identical native scene against the actual official module in an isolated run.</summary>
    [TestMethod]
    public void WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive()
        => RunVisualCacheConsumer(true, 0xff00ff00, 0x0000ff80, false, resizeWindow: true, official: true);

    private static void LogHardwareModuleIdentity(nint module, string requested)
    {
        char* buffer = stackalloc char[32768];
        uint length = GetModuleFileNameW(module, buffer, 32768);
        Assert.IsTrue(length > 0 && length < 32768);
        string actual = new(buffer, 0, (int)length);
        Assert.AreEqual(Path.GetFullPath(requested), Path.GetFullPath(actual), true);
        FileVersionInfo version = FileVersionInfo.GetVersionInfo(actual);
        var identity = new
        {
            Requested = requested, Actual = actual,
            Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual))),
            version.FileVersion, version.ProductVersion,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            Runtime = RuntimeInformation.FrameworkDescription,
            TestAssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(D3DImageTests).Assembly.Location)))
        };
        string json = JsonSerializer.Serialize(identity, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
        File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"hw-module-{Path.GetFileName(Path.GetDirectoryName(actual))}-{Guid.NewGuid():N}.json"), json);
    }

    /// <summary>Requires genuine D3D9 exclusive-mode device loss and reset rather than simulating a failure.</summary>
    [TestMethod]
    public void WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover()
    {
        nint window = CreateOutputWindow("Exclusive device loss probe", 40, 40);
        nint otherWindow = CreateOutputWindow("Exclusive device loss trigger", 100, 40);
        nint factory = Direct3DCreate9(32), first = 0, second = 0;
        try
        {
            Assert.AreNotEqual(0, factory);
            NativeDisplayMode mode = default;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, NativeDisplayMode*, int>)(*(void***)factory)[8])(factory, 0, &mode));
            PresentParameters parameters = new() { Width = mode.Width, Height = mode.Height, Format = mode.Format, Count = 1, SwapEffect = 1, Window = window, Windowed = 0 };
            nint created = 0;
            int result = windowThread!.Invoke(() => CreateExclusiveDevice(factory, window, parameters, out created));
            first = created;
            Console.WriteLine($"DeviceLossProbe firstCreate=0x{result:X8} mode={mode.Width}x{mode.Height} format={mode.Format}");
            Assert.AreEqual(0, result);
            PresentParameters other = parameters;
            other.Window = otherWindow;
            result = windowThread.Invoke(() => CreateExclusiveDevice(factory, otherWindow, other, out created));
            second = created;
            int lost = ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)first)[3])(first);
            Console.WriteLine($"DeviceLossProbe secondCreate=0x{result:X8} firstCooperative=0x{lost:X8}");
            Assert.AreEqual(unchecked((int)0x88760868), lost, "Exclusive displacement must produce real D3DERR_DEVICELOST; no synthetic replacement is accepted.");
            windowThread.Invoke(() => { Release(second); return 0; });
            second = 0;
            EstablishObservableWindow(window);
            long deadline = Environment.TickCount64 + 5000;
            do
            {
                lost = ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)first)[3])(first);
                ProcessWindowMessages(window);
            } while (lost == unchecked((int)0x88760868) && Environment.TickCount64 < deadline);
            Console.WriteLine($"DeviceLossProbe beforeReset=0x{lost:X8}");
            Assert.AreEqual(unchecked((int)0x88760869), lost);
            result = windowThread.Invoke(() => ResetExclusiveDevice(first, parameters));
            Console.WriteLine($"DeviceLossProbe reset=0x{result:X8}");
            Assert.AreEqual(0, result);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)first)[3])(first));
        }
        finally
        {
            windowThread!.Invoke(() => { Release(second); Release(first); return 0; });
            Release(factory);
            DestroyWindow(otherWindow); DestroyWindow(window);
        }
    }

    private static int CreateExclusiveDevice(nint factory, nint window, PresentParameters parameters, out nint device)
    {
        nint created = 0;
        int result = ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)factory)[16])(factory, 0, 1, window, 0x26, &parameters, &created);
        device = created;
        return result;
    }

    private static int ResetExclusiveDevice(nint device, PresentParameters parameters)
        => ((delegate* unmanaged[Stdcall]<nint, PresentParameters*, int>)(*(void***)device)[16])(device, &parameters);
}
