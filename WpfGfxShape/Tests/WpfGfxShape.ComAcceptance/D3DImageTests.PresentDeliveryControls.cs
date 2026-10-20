#if CC_PRESENT_DELIVERY_DIAGNOSTICS
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct CcCreationParameters
    {
        internal uint Adapter, Type;
        internal nint Focus;
        internal uint Flags;
    }

    /// <summary>Separates destination override and device creation thread without changing the ordinary capture fixture.</summary>
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void WhenPresentDestinationAndCreationThreadAreControlledThenDeliveryIsMeasured(bool ownerCreation, bool nullOverride)
    {
        var records = new List<object>();
        nint window = CreateOutputWindow("CC native delivery control", 40, 40);
        nint factory = 0, device = 0;
        bool passed = true;
        uint creationThread = 0;
        try
        {
            EstablishObservableWindow(window);
            factory = Direct3DCreate9(32);
            Assert.AreNotEqual(0, factory);
            nint CreateDevice()
            {
                creationThread = GetCurrentThreadId();
                PresentParameters parameters = new() { Width = 32, Height = 16, Format = 21, Count = 1, SwapEffect = 1, Window = window, Windowed = 1 };
                nint result = 0;
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)factory)[16])(factory, 0, 1, window, 0x26, &parameters, &result));
                return result;
            }
            device = ownerCreation ? (windowThread ?? throw new InvalidOperationException()).Invoke(CreateDevice) : CreateDevice();
            uint owner = GetWindowThreadProcessId(window, out _);
            nint swap = 0;
            PresentParameters actual = default;
            CcCreationParameters creation = default;
            uint* mode = stackalloc uint[4];
            try
            {
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)device)[14])(device, 0, &swap));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, PresentParameters*, int>)(*(void***)swap)[9])(swap, &actual));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, CcCreationParameters*, int>)(*(void***)device)[9])(device, &creation));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)(*(void***)device)[8])(device, 0, mode));
                records.Add(new { stage = "actual-device", hwnd = (long)window, owner, creationThread, creationAdapter = creation.Adapter, creationType = creation.Type, focus = (long)creation.Focus, creationFlags = creation.Flags,
                    width = actual.Width, height = actual.Height, format = actual.Format, count = actual.Count, swapEffect = actual.SwapEffect, deviceWindow = (long)actual.Window, windowed = actual.Windowed,
                    flags = actual.Flags, interval = actual.Interval, multisample = actual.Multisample, quality = actual.Quality, refresh = actual.Refresh,
                    displayMode = new[] { mode[0], mode[1], mode[2], mode[3] }, adapterMonitor = (long)((delegate* unmanaged[Stdcall]<nint, uint, nint>)(*(void***)factory)[15])(factory, creation.Adapter), windowMonitor = (long)MonitorFromWindow(window, 2), destinationOverride = nullOverride ? 0L : (long)window });
                Assert.AreEqual(window, actual.Window);
            }
            finally { Release(swap); }
            foreach (uint expected in new uint[] { 0xffff0000, 0xff0000ff, 0xffff0000 })
            {
                uint backbuffer = 0, duplicated = 0, client = uint.MaxValue, screen = uint.MaxValue, front = uint.MaxValue, submitThread = 0;
                int present = int.MinValue, frontResult = int.MinValue;
                long submitted = 0, completed = 0;
                string? failure = null;
                try
                {
                    duplicated = ReadDuplicatedDesktopPixel(window, () =>
                    {
                        (windowThread ?? throw new InvalidOperationException()).Invoke(() =>
                        {
                            submitThread = GetCurrentThreadId();
                            Assert.AreEqual(owner, submitThread);
                            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)device)[43])(device, 0, 0, 1, expected, 1, 0));
                            backbuffer = ReadNativeBackBufferPixel(device);
                            Assert.AreEqual(expected, backbuffer);
                            submitted = System.Diagnostics.Stopwatch.GetTimestamp();
                            present = ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, int>)(*(void***)device)[17])(device, 0, 0, nullOverride ? 0 : window, 0);
                            Assert.AreEqual(0, present);
                            CompleteNativePresent(device, window);
                            completed = System.Diagnostics.Stopwatch.GetTimestamp();
                            return 0;
                        });
                        WindowPoint point = new() { X = 16, Y = 8 };
                        Assert.AreNotEqual(0, ClientToScreen(window, ref point));
                        nint clientDc = GetDC(window), screenDc = GetDC(0);
                        try { client = GetPixel(clientDc, 16, 8); screen = GetPixel(screenDc, point.X, point.Y); }
                        finally { if (clientDc != 0) ReleaseDC(window, clientDc); if (screenDc != 0) ReleaseDC(0, screenDc); }
                        (frontResult, front) = ReadFreshGateFrontPixel(device, point);
                    });
                }
                catch (AssertFailedException error) { failure = error.ToString(); }
                uint colorRef = ((expected & 255) << 16) | (expected & 0xff00) | ((expected >> 16) & 255);
                bool accepted = failure is null && (duplicated & 0xffffff) == (expected & 0xffffff) && client == colorRef && screen == colorRef && frontResult == 0 && (front & 0xffffff) == (expected & 0xffffff);
                passed &= accepted;
                records.Add(new { expected, backbuffer, present, submitted, completed, submitThread, owner, duplicated, client, screen, frontResult, front, accepted, failure });
            }
            DiagnoseNativePresent(window, device, false);
        }
        finally
        {
            Assembly assembly = typeof(D3DImageTests).Assembly;
            File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"cc-delivery-{ownerCreation}-{nullOverride}.json"), JsonSerializer.Serialize(new
            {
                ownerCreation, nullOverride, passed, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                testPath = assembly.Location, testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))), testMvid = assembly.ManifestModule.ModuleVersionId, records
            }, new JsonSerializerOptions { WriteIndented = true }));
            Release(device); Release(factory);
            if (window != 0) DestroyWindow(window);
        }
        Assert.IsTrue(passed);
    }
}
#endif
