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
        RunExclusiveFocusLoss();
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
