using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class MediaExportsTests
{
    [TestMethod]
    [Timeout(30000)]
    public void WhenProviderCallerOwnsReferenceThenPlayerSurvivesMediaCallerRelease()
    {
        using var source = new SystemMedia(candidate: true);
        nint provider = 0, identity = 0, renderer = 0;
        Guid providerId = new("E6F1CC74-A0EB-4EBE-8241-089D1CB079D7");
        Guid unknown = new("00000000-0000-0000-C000-000000000046");
        nint media = source.Media;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)media)[0])(media, &providerId, &provider));
            Assert.AreNotEqual(media, provider);
            Assert.AreEqual(1u, source.ReleaseCallerReference());
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)provider)[0])(provider, &unknown, &identity));
            Assert.AreEqual(media, identity);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)provider)[3])(provider, &renderer));
            Release(renderer); renderer = 0;
            Release(identity); identity = 0;
            Assert.AreEqual(0u, ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)provider)[2])(provider));
            provider = 0;
        }
        finally { Release(renderer); Release(identity); Release(provider); }
    }

    [TestMethod]
    [Timeout(30000)]
    public void WhenRealCallbackClosesAndReturnsLastCallerThenCallbackReferenceKeepsPlayerAlive()
    {
        string path = Path.Combine(Path.GetTempPath(), $"wpf-callback-{Guid.NewGuid():N}.avi");
        WriteSolidVideo(path, 0xffff0000u);
        using var source = new SystemMedia(candidate: true);
        nint module = LoadCandidate();
        nint media = source.Media;
        var open = (delegate* unmanaged[Stdcall]<nint, char*, int>)NativeLibrary.GetExport(module, "MILMediaOpen");
        int entered = 0, result = -1;
        uint remaining = uint.MaxValue;
        using var completed = new ManualResetEventSlim();
        source.Callback = () =>
        {
            if (Interlocked.Exchange(ref entered, 1) != 0) return;
            try
            {
                var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MILMediaClose");
                result = close(media);
                remaining = source.ReleaseCallerReference();
            }
            finally { completed.Set(); }
        };
        fixed (char* url = path) Assert.AreEqual(0, open(media, url));
        Assert.IsTrue(completed.Wait(TimeSpan.FromSeconds(15)), "Real callback did not complete before timeout.");
        Assert.IsNull(source.CallbackError);
        Assert.AreEqual((0, 1u), (result, remaining));
    }
}
