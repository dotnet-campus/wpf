using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class MediaExportsTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenOriginalFixtureIsOpenedThenPublicMediaEventsAndStateAreRecorded(bool candidate)
    {
        Directory.CreateDirectory(AcceptancePaths.Results);
        string path = Path.GetFullPath(Path.Combine(AcceptancePaths.Results, "media-original.avi"));
        WriteSolidVideo(path);
        using var source = new SystemMedia(candidate);
        void** table = *(void***)source.Media;
        int open;
        fixed (char* name = path) open = ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(source.Media, name);
        int rate = ((delegate* unmanaged[Stdcall]<nint, double, int>)table[8])(source.Media, 1);
        Stopwatch timer = Stopwatch.StartNew();
        string pixels = "not sampled";
        while (timer.Elapsed < TimeSpan.FromSeconds(15))
        {
            source.WaitForNotification(TimeSpan.FromMilliseconds(100));
            pixels = SampleDiagnosticPixels(source.Media);
        }
        RecordMediaState(source, path, candidate, $"open={open:X8}; rate={rate:X8}; {pixels}");
        byte hasVideo = 0;
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[16])(source.Media, &hasVideo));
        Assert.AreEqual((byte)1, hasVideo);
    }

    private static string SampleDiagnosticPixels(nint media)
    {
        nint provider = 0, renderer = 0, bitmap = 0;
        Guid iid = new("E6F1CC74-A0EB-4EBE-8241-089D1CB079D7");
        int hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)media)[0])(media, &iid, &provider);
        if (hr < 0) return $"provider={hr:X8}";
        try
        {
            hr = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)provider)[3])(provider, &renderer);
            if (hr < 0 || renderer == 0) return $"renderer={hr:X8}/{renderer}";
            long time = -1; int ready = 0;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint, int, int, long*, int*, int>)(*(void***)renderer)[3])(renderer, 0, 0, 1, &time, &ready);
            try
            {
                int renderHr = ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)(*(void***)renderer)[4])(renderer, 0, &bitmap);
                try
                {
                    if (bitmap == 0) return $"composition={hr:X8}; ready={ready}; render={renderHr:X8}; bitmap=null";
                    byte* bytes = stackalloc byte[8];
                    new Span<byte>(bytes, 8).Clear();
                    int copyHr = ((delegate* unmanaged[Stdcall]<nint, void*, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, null, 8, 8, bytes);
                    return $"composition={hr:X8}; ready={ready}; render={renderHr:X8}; copy={copyHr:X8}; pixels={Convert.ToHexString(new ReadOnlySpan<byte>(bytes, 8))}";
                }
                finally { if (renderHr >= 0) _ = ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)renderer)[5])(renderer); }
            }
            finally { _ = ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)renderer)[6])(renderer, 0); }
        }
        finally { Release(bitmap); Release(renderer); Release(provider); }
    }

    private static void RecordMediaState(SystemMedia source, string fixture, bool candidate, string pixels)
    {
        byte video = 0;
        long position = 0;
        uint width = 0, height = 0;
        void** table = *(void***)source.Media;
        int videoHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[16])(source.Media, &video);
        int positionHr = ((delegate* unmanaged[Stdcall]<nint, long*, int>)table[6])(source.Media, &position);
        int widthHr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[19])(source.Media, &width);
        int heightHr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[18])(source.Media, &height);
        Directory.CreateDirectory(AcceptancePaths.Results);
        string system = Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll");
        File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"media-{candidate}-{Guid.NewGuid():N}.json"), JsonSerializer.Serialize(new
        {
            candidate, fixture, fixtureSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fixture))),
            candidateSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(AcceptancePaths.AotDll))),
            system, systemVersion = FileVersionInfo.GetVersionInfo(system).FileVersion,
            events = source.Events.ToArray(), video, videoHr, position, positionHr, width, widthHr, height, heightHr, pixels
        }, new JsonSerializerOptions { WriteIndented = true }));
    }
}
