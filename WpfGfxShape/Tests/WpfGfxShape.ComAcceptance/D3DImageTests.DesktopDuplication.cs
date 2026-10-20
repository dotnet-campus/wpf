using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [DllImport("dxgi.dll")]
    private static extern int CreateDXGIFactory1(in Guid iid, out nint factory);
    [DllImport("d3d11.dll")]
    private static extern int D3D11CreateDevice(nint adapter, uint driver, nint software, uint flags, nint levels, uint count, uint sdk, out nint device, out uint level, out nint context);

    [StructLayout(LayoutKind.Sequential)]
    private struct DesktopTextureDescription
    {
        internal uint Width, Height, MipLevels, ArraySize, Format, SampleCount, SampleQuality, Usage, BindFlags, CpuAccessFlags, MiscFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DesktopMappedTexture
    {
        internal nint Data;
        internal uint RowPitch, DepthPitch;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DesktopOutputDescription
    {
        internal fixed char DeviceName[32];
        internal int Left, Top, Right, Bottom, Attached;
        internal uint Rotation;
        internal nint Monitor;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DesktopAdapterDescription
    {
        internal fixed char Description[128];
        internal uint Vendor, Device, Subsystem, Revision;
        internal nuint DedicatedVideoMemory, DedicatedSystemMemory, SharedSystemMemory;
        internal uint LuidLow;
        internal int LuidHigh;
        internal uint Flags;
    }

    /// <summary>Rejects pointer-only and older frames even when acquisition itself succeeds.</summary>
    [TestMethod]
    [DataRow(0L, 100L, false)]
    [DataRow(99L, 100L, false)]
    [DataRow(100L, 100L, true)]
    [DataRow(101L, 100L, true)]
    public void WhenDesktopFrameIsSelectedThenItBelongsToCurrentSubmission(long lastPresent, long submissionStart, bool expected)
        => Assert.AreEqual(expected, IsDesktopContentFrame(lastPresent, submissionStart));

    private static bool IsDesktopContentFrame(long lastPresent, long submissionStart)
        => lastPresent > 0 && lastPresent >= submissionStart;

    /// <summary>Checks that an unrelated fresh desktop update is not retried until the expected color appears.</summary>
    [TestMethod]
    public void WhenDesktopCaptureReceivesUnrelatedFreshFrameThenIndependentColorCheckRejectsIt()
    {
        var frames = new Queue<(int, long, uint)>([(0, 101, 0xff0000ffu), (0, 102, 0xffff0000u)]);
        uint pixel = 0;
        WaitForDesktopContent(100, () =>
        {
            var frame = frames.Dequeue();
            pixel = frame.Item3;
            return (frame.Item1, frame.Item2);
        }, () => Assert.Fail("A fresh unrelated frame must reach the independent content check."), () => true);
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreEqual(0x00ff0000u, pixel & 0x00ffffffu));
        Assert.AreEqual(1, frames.Count);
    }

    /// <summary>Records the installed official module separately from candidate artifact identity.</summary>
    [TestMethod]
    public void WhenDesktopCaptureOfficialIdentityIsRecordedThenModuleIsNotCandidate()
    {
        string path = AcceptancePaths.OriginalDll;
        nint module = NativeLibrary.Load(path);
        LogHardwareModuleIdentity(module, path);
        Assert.AreNotEqual(Path.GetFullPath(AcceptancePaths.AotDll), Path.GetFullPath(path));
    }

    private static void WaitForDesktopContent(long submissionStart, Func<(int Result, long LastPresent)> acquire,
        Action releaseRejected, Func<bool> beforeDeadline)
    {
        while (true)
        {
            Assert.IsTrue(beforeDeadline(), "No desktop content frame belonging to the current submission arrived.");
            var frame = acquire();
            if (frame.Result == unchecked((int)0x887A0027)) continue;
            Assert.AreEqual(0, frame.Result);
            if (IsDesktopContentFrame(frame.LastPresent, submissionStart)) return;
            releaseRejected();
        }
    }

    /// <summary>Exercises the same acquisition loop as real desktop capture, including pointer-only and stale releases.</summary>
    [TestMethod]
    public void WhenDesktopCaptureReceivesPointerAndOldFramesThenOnlyCurrentFrameIsHeld()
    {
        var frames = new Queue<(int, long)>([(unchecked((int)0x887A0027), 0), (0, 0), (0, 99), (unchecked((int)0x887A0027), 0), (0, 101)]);
        int releases = 0;
        WaitForDesktopContent(100, frames.Dequeue, () => releases++, () => true);
        Assert.AreEqual(2, releases);
        Assert.AreEqual(0, frames.Count);
    }

    /// <summary>Checks that deadline exhaustion and access loss cannot be mistaken for acquired content.</summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenDesktopCaptureCannotAcquireContentThenItFailsWithoutReleasingAnUnownedFrame(bool accessLost)
    {
        int calls = 0, releases = 0;
        Assert.ThrowsExactly<AssertFailedException>(() => WaitForDesktopContent(100,
            () => { calls++; return (accessLost ? unchecked((int)0x887A0026) : unchecked((int)0x887A0027), 0L); },
            () => releases++, () => calls < 3));
        Assert.AreEqual(0, releases);
        Assert.AreEqual(accessLost ? 1 : 3, calls);
    }

    /// <summary>Checks that a failed frame release remains visible after every owned COM reference is released.</summary>
    [TestMethod]
    public void WhenDesktopFrameReleaseFailsThenResourcesAreReleasedAndFailureRemainsVisible()
    {
        var released = new List<nint>();
        Assert.ThrowsExactly<AssertFailedException>(() => CleanupDesktopCapture(true, 11,
            new nint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
            _ => unchecked((int)0x80004005), released.Add));
        CollectionAssert.AreEqual(new nint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, released);
    }

    /// <summary>Checks cleanup even when the frame release callback throws instead of returning an HRESULT.</summary>
    [TestMethod]
    public void WhenDesktopFrameReleaseThrowsThenResourcesAreReleasedAndExceptionIsPreserved()
    {
        var released = new List<nint>();
        var failure = new InvalidOperationException();
        InvalidOperationException actual = Assert.ThrowsExactly<InvalidOperationException>(() =>
            CleanupDesktopCapture(true, 11, new nint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                _ => throw failure, released.Add));
        Assert.AreSame(failure, actual);
        CollectionAssert.AreEqual(new nint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, released);
    }

    /// <summary>Checks exactly-once frame release only for an acquired frame and unconditional reference cleanup.</summary>
    [TestMethod]
    [DataRow(false, 0)]
    [DataRow(true, 1)]
    public void WhenDesktopCaptureEndsThenOnlyOwnedFrameIsReleased(bool acquired, int expectedCalls)
    {
        int calls = 0;
        var released = new List<nint>();
        CleanupDesktopCapture(acquired, 11, new nint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
            _ => { calls++; return 0; }, released.Add);
        Assert.AreEqual(expectedCalls, calls);
        CollectionAssert.AreEqual(new nint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, released);
    }

    private static int ReleaseDesktopFrame(nint duplication)
        => ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)duplication)[14])(duplication);

    private static void CleanupDesktopCapture(bool acquired, nint duplication, ReadOnlySpan<nint> resources,
        Func<nint, int> releaseFrame, Action<nint> releaseResource)
    {
        try
        {
            if (acquired) Assert.AreEqual(0, releaseFrame(duplication));
        }
        finally
        {
            // A failed ReleaseFrame assertion must not strand the remaining owned COM references.
            foreach (nint resource in resources) releaseResource(resource);
        }
    }

    private static uint ReadDuplicatedDesktopPixel(nint window, Action? submitAfterDuplication = null)
    {
        nint factory = 0, adapter = 0, output = 0, output1 = 0, device = 0, context = 0, duplication = 0;
        nint resource = 0, texture = 0, staging = 0;
        bool acquired = false;
        try
        {
            Assert.AreEqual(0, CreateDXGIFactory1(new Guid("770aae78-f26f-4dba-a829-253c83d1b387"), out factory));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)factory)[12])(factory, 0, &adapter));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)adapter)[7])(adapter, 0, &output));
            DesktopOutputDescription outputDescription = default;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, DesktopOutputDescription*, int>)(*(void***)output)[7])(output, &outputDescription));
            DesktopAdapterDescription adapterDescription = default;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, DesktopAdapterDescription*, int>)(*(void***)adapter)[10])(adapter, &adapterDescription));
            Console.WriteLine($"DesktopDuplication adapterDescription={new string(adapterDescription.Description)} vendor=0x{adapterDescription.Vendor:X} device=0x{adapterDescription.Device:X} luid={adapterDescription.LuidHigh:X8}:{adapterDescription.LuidLow:X8} flags=0x{adapterDescription.Flags:X}");
            Console.WriteLine($"DesktopDuplication outputName={new string(outputDescription.DeviceName)} bounds=({outputDescription.Left},{outputDescription.Top},{outputDescription.Right},{outputDescription.Bottom}) attached={outputDescription.Attached} rotation={outputDescription.Rotation} outputMonitor=0x{outputDescription.Monitor:X} windowMonitor=0x{MonitorFromWindow(window, 2):X}");
            Assert.AreEqual(MonitorFromWindow(window, 2), outputDescription.Monitor);
            Assert.AreEqual(1u, outputDescription.Rotation);
            Guid iid = new("00cddea8-939b-4b83-a340-a685226666cc");
            Assert.AreEqual(0, Marshal.QueryInterface(output, in iid, out output1));
            Assert.AreEqual(0, D3D11CreateDevice(adapter, 0, 0, 0, 0, 0, 7, out device, out uint level, out context));
            int duplicate = ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)(*(void***)output1)[22])(output1, device, &duplication);
            Console.WriteLine($"DesktopDuplication create=0x{duplicate:X8} adapter=0 output=0");
            Assert.AreEqual(0, duplicate);
            byte* frameInfo = stackalloc byte[48];
            long submissionStart = 0;
            if (submitAfterDuplication is not null)
            {
                int initial = ((delegate* unmanaged[Stdcall]<nint, uint, void*, nint*, int>)(*(void***)duplication)[8])(duplication, 1000, frameInfo, &resource);
                Console.WriteLine($"DesktopDuplication initial result=0x{initial:X8} qpc={System.Diagnostics.Stopwatch.GetTimestamp()}");
                if (initial == 0)
                {
                    acquired = true;
                    Console.WriteLine($"DesktopDuplication initial lastPresent={*(long*)frameInfo} lastMouse={*(long*)(frameInfo + 8)} accumulated={*(uint*)(frameInfo + 16)}");
                    acquired = false;
                    Assert.AreEqual(0, ReleaseDesktopFrame(duplication));
                    Release(resource); resource = 0;
                }
                else Assert.AreEqual(unchecked((int)0x887A0027), initial);
                submissionStart = System.Diagnostics.Stopwatch.GetTimestamp();
                Console.WriteLine($"DesktopDuplication submissionStart={submissionStart}");
                submitAfterDuplication();
                Console.WriteLine($"DesktopDuplication submitted qpc={System.Diagnostics.Stopwatch.GetTimestamp()}");
            }
            long deadline = Environment.TickCount64 + 5000;
            nint captureDuplication = duplication;
            nint[] ownedResource = [0];
            try { WaitForDesktopContent(submissionStart, () =>
            {
                nint nextResource = 0;
                int next = ((delegate* unmanaged[Stdcall]<nint, uint, void*, nint*, int>)(*(void***)captureDuplication)[8])(captureDuplication, 500, frameInfo, &nextResource);
                ownedResource[0] = nextResource;
                acquired = next == 0;
#if CG_DWM_DELIVERY_DIAGNOSTICS
                cgAcquisition?.Invoke(next, acquired ? *(long*)frameInfo : 0, acquired ? *(long*)(frameInfo + 8) : 0, acquired ? *(uint*)(frameInfo + 16) : 0);
#endif
                Console.WriteLine($"DesktopDuplication acquire result=0x{next:X8} qpc={System.Diagnostics.Stopwatch.GetTimestamp()} submissionStart={submissionStart} resource=0x{nextResource:X}");
                if (acquired)
                    Console.WriteLine($"DesktopDuplication observed lastPresent={*(long*)frameInfo} lastMouse={*(long*)(frameInfo + 8)} accumulated={*(uint*)(frameInfo + 16)} coalesced={*(uint*)(frameInfo + 20)} protectedMasked={*(uint*)(frameInfo + 24)}");
                return (next, next == 0 ? *(long*)frameInfo : 0);
            }, () =>
            {
                acquired = false;
                try { Assert.AreEqual(0, ReleaseDesktopFrame(captureDuplication)); }
                finally { Release(ownedResource[0]); ownedResource[0] = 0; }
            }, () => Environment.TickCount64 < deadline); }
            finally { resource = ownedResource[0]; }
            Console.WriteLine($"DesktopDuplication frame lastPresent={*(long*)frameInfo} lastMouse={*(long*)(frameInfo + 8)} accumulated={*(uint*)(frameInfo + 16)} coalesced={*(uint*)(frameInfo + 20)} protectedMasked={*(uint*)(frameInfo + 24)}");
            iid = new("6f15aaf2-d208-4e89-9ab4-489535d34f9c");
            Assert.AreEqual(0, Marshal.QueryInterface(resource, in iid, out texture));
            DesktopTextureDescription description = default;
            ((delegate* unmanaged[Stdcall]<nint, DesktopTextureDescription*, void>)(*(void***)texture)[10])(texture, &description);
            description.Usage = 3;
            description.BindFlags = 0;
            description.CpuAccessFlags = 0x20000;
            description.MiscFlags = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, DesktopTextureDescription*, nint, nint*, int>)(*(void***)device)[5])(device, &description, 0, &staging));
            ((delegate* unmanaged[Stdcall]<nint, nint, nint, void>)(*(void***)context)[47])(context, staging, texture);
            WindowPoint point = new() { X = 16, Y = 8 };
            Assert.AreNotEqual(0, ClientToScreen(window, ref point));
            ObservationMonitorInfo monitor = new() { Size = (uint)sizeof(ObservationMonitorInfo) };
            Assert.AreNotEqual(0, GetMonitorInfoW(MonitorFromWindow(window, 2), ref monitor));
            int x = point.X - monitor.Monitor.Left, y = point.Y - monitor.Monitor.Top;
            Assert.IsTrue(x >= 0 && y >= 0 && x < description.Width && y < description.Height);
            Assert.AreEqual(87u, description.Format);
            DesktopMappedTexture mapped = default;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint, DesktopMappedTexture*, int>)(*(void***)context)[14])(context, staging, 0, 1, 0, &mapped));
            uint pixel;
            Console.WriteLine($"DesktopDuplication readback featureLevel=0x{level:X} monitor=({monitor.Monitor.Left},{monitor.Monitor.Top},{monitor.Monitor.Right},{monitor.Monitor.Bottom}) local=({x},{y}) rowPitch={mapped.RowPitch} depthPitch={mapped.DepthPitch}");
            try
            {
                pixel = *(uint*)((byte*)mapped.Data + y * mapped.RowPitch + x * sizeof(uint));
                using FileStream image = File.Create(Path.Combine(AcceptancePaths.Results, $"desktop-{Guid.NewGuid():N}.ppm"));
                byte[] header = System.Text.Encoding.ASCII.GetBytes($"P6\n{description.Width} {description.Height}\n255\n");
                image.Write(header);
                byte[] row = new byte[checked((int)description.Width * 3)];
                for (int rowIndex = 0; rowIndex < description.Height; rowIndex++)
                {
                    byte* source = (byte*)mapped.Data + rowIndex * mapped.RowPitch;
                    for (int column = 0; column < description.Width; column++)
                    {
                        row[column * 3] = source[column * 4 + 2];
                        row[column * 3 + 1] = source[column * 4 + 1];
                        row[column * 3 + 2] = source[column * 4];
                    }
                    image.Write(row);
                }
            }
            finally { ((delegate* unmanaged[Stdcall]<nint, nint, uint, void>)(*(void***)context)[15])(context, staging, 0); }
            Console.WriteLine($"DesktopDuplication screen=({point.X},{point.Y}) texture={description.Width}x{description.Height} pixel=0x{pixel:X8}");
            return pixel;
        }
        finally
        {
            CleanupDesktopCapture(acquired, duplication,
                [staging, texture, resource, duplication, context, device, output1, output, adapter, factory],
                ReleaseDesktopFrame, pointer => Release(pointer));
        }
    }
}
