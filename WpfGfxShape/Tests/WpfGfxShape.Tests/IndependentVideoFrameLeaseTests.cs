using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    [TestMethod]
    public unsafe void WhenDecodedFramesReplaceThenResourceLeasesSurviveOwnerClose()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-frame-lease");
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        MediaFoundationSourceReader? reader = null;
        using IndependentVideoFrameOwner owner = new();
        GeneratedMediaPlayerResource resource = new();
        SystemVideoSample? oldLease = null, newLease = null;
        var observations = new List<object>();
        bool completed = false;
        uint? finalOldReference = null, finalNewReference = null;
        var frames = new List<object>();
        void Save() => File.WriteAllText(Path.Combine(root, "result.json"), JsonSerializer.Serialize(new
        {
            completed, backend = "CPU software / managed production assembly; not NativeAOT or complete DrawVideo",
            runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(MediaFoundationResourceTests).Assembly.Location))),
            productSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(IndependentVideoFrameOwner).Assembly.Location))),
            testMvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId,
            productMvid = typeof(IndependentVideoFrameOwner).Assembly.ManifestModule.ModuleVersionId,
            inputSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(input))),
            modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
                .Where(module => module.ModuleName is "mfplat.dll" or "mfreadwrite.dll")
                .Select(module => new { module.FileName, version = module.FileVersionInfo.FileVersion }).ToArray(),
            frames, observations, finalOldReference, finalNewReference, reader?.Cleanup, reader?.ReaderFinalReferences,
            reader?.UnlockCount, reader?.SampleReleaseCount, reader?.BufferReleaseCount,
            reader?.ShutdownCount, reader?.UninitializeCount
        }, new JsonSerializerOptions { WriteIndented = true }));
        byte[] Read(SystemVideoSample lease)
        {
            nint source = lease.Source;
            uint width = 0, height = 0;
            int format = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)source)[3])(source, &width, &height));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int*, int>)(*(void***)source)[4])(source, &format));
            Assert.AreEqual((2u, 1u, 16), (width, height, format));
            byte[] pixels = new byte[8];
            fixed (byte* target = pixels)
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)source)[7])(source, 0, 8, 8, target));
            observations.Add(new { lease.Timestamp, width, height, format, pixels = Convert.ToHexString(pixels), sha256 = Convert.ToHexString(SHA256.HashData(pixels)) });
            return pixels;
        }
        try
        {
            Assert.AreEqual(0, MediaFoundationSourceReader.Create(input, out reader));
            Assert.IsNotNull(reader);
            resource.BindIndependentFrames(owner);
            for (int index = 0; index <= 20; index++)
            {
                Assert.AreEqual(0, reader.Read(out _, out _, out long time, out byte[]? pixels));
                Assert.IsNotNull(pixels);
                frames.Add(new { index, time, reader.Width, reader.Height, reader.Stride, reader.DisplayX, reader.DisplayY, reader.DisplayWidth, reader.DisplayHeight });
                owner.Publish(pixels, reader.Width, reader.Height, reader.DisplayX, reader.DisplayY,
                    reader.DisplayWidth, reader.DisplayHeight, time);
                if (index == 0) Assert.AreEqual(0, resource.AcquireSystemFrame(out oldLease));
            }
            Assert.AreEqual(0, resource.AcquireSystemFrame(out newLease));
            Assert.IsNotNull(oldLease);
            Assert.IsNotNull(newLease);
            CollectionAssert.AreEqual(new byte[] { 0, 0, 255, 255, 0, 0, 255, 255 }, Read(oldLease));
            CollectionAssert.AreEqual(new byte[] { 255, 0, 0, 255, 255, 0, 0, 255 }, Read(newLease));
            Assert.AreEqual((0L, 20000000L), (oldLease.Timestamp, newLease.Timestamp));
            owner.Dispose();
            Assert.AreEqual(unchecked((int)0x80000013), owner.Acquire(out _));
            CollectionAssert.AreEqual(new byte[] { 0, 0, 255, 255, 0, 0, 255, 255 }, Read(oldLease));
            CollectionAssert.AreEqual(new byte[] { 255, 0, 0, 255, 255, 0, 0, 255 }, Read(newLease));
            Assert.AreEqual(unchecked((int)0x80000013), resource.AcquireSystemFrame(out _));
            nint retained = oldLease.Source;
            Direct3D9Factory.AddRef(retained);
            Assert.AreEqual(0, oldLease.Close());
            Assert.AreEqual(0, oldLease.Close());
            finalOldReference = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)retained)[2])(retained);
            Assert.AreEqual(0u, finalOldReference);
            nint retainedNew = newLease.Source;
            Direct3D9Factory.AddRef(retainedNew);
            newLease.Dispose();
            newLease.Dispose();
            finalNewReference = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)retainedNew)[2])(retainedNew);
            Assert.AreEqual(0u, finalNewReference);
            Assert.AreEqual(0, reader.Close());
            completed = true;
        }
        finally
        {
            oldLease?.Dispose(); newLease?.Dispose();
            reader?.Dispose(); Save();
        }
    }
}

[TestClass]
public sealed class IndependentVideoFrameOwnerTests
{
    [TestMethod]
    public void WhenClosedThenAcquisitionIsRejectedWithoutSystemCalls()
    {
        using IndependentVideoFrameOwner owner = new();
        owner.Dispose();
        Assert.AreEqual(unchecked((int)0x80000013), owner.Acquire(out _));
    }

    [TestMethod]
    public void WhenClosedThenPublicationIsRejected()
    {
        using IndependentVideoFrameOwner owner = new();
        owner.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(() => owner.Publish(new byte[4], 1, 1, 0, 0, 1, 1, 0));
    }
}
