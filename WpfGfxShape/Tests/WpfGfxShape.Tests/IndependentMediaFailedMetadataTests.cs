using System.Reflection;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class IndependentMediaFailedMetadataTests
{
    [DataTestMethod]
    [DataRow(true, true, 640u, 360u, 7438266666L, 0)]
    [DataRow(true, false, 0u, 0u, 40000000L, 0)]
    [DataRow(false, true, 2u, 1u, 40000000L, 0)]
    [DataRow(false, false, 0u, 0u, 0L, unchecked((int)0xc00d36e6))]
    public void WhenFailedThenComGettersReturnKnownMetadataWithoutChangingFailure(bool audio, bool video, uint width, uint height, long duration, int metadataResult)
    {
        var media = IndependentMediaIdentity.Create();
        var owner = new IndependentVideoPlaybackOwner();
        const int failure = unchecked((int)0xc00d36b4);
        try
        {
            Set(owner, "HasAudio", metadataResult == 0 ? (bool?)audio : null);
            Set(owner, "AudioMetadataResult", metadataResult);
            Set(owner, "Width", width);
            Set(owner, "Height", height);
            Set(owner, "Duration100ns", metadataResult == 0 ? (long?)duration : null);
            Set(owner, "DurationResult", metadataResult);
            typeof(IndependentMediaIdentity).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            Set(media, "State", IndependentMediaState.Failed);
            Set(media, "Failure", failure);
            nint* table = *(nint**)media.Pointer;
            byte actualAudio = 9, actualVideo = 9;
            uint actualWidth = 9, actualHeight = 9;
            long actualDuration = -1;
            int audioHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[17])(media.Pointer, &actualAudio);
            int videoHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[16])(media.Pointer, &actualVideo);
            int widthHr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[19])(media.Pointer, &actualWidth);
            int heightHr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[18])(media.Pointer, &actualHeight);
            int lengthHr = ((delegate* unmanaged[Stdcall]<nint, long*, int>)table[20])(media.Pointer, &actualDuration);
            Assert.AreEqual((0, audio ? (byte)1 : (byte)0, 0, video ? (byte)1 : (byte)0, 0, width, 0, height, 0, duration, failure),
                (audioHr, actualAudio, videoHr, actualVideo, widthHr, actualWidth, heightHr, actualHeight, lengthHr, actualDuration, media.Failure));
        }
        finally
        {
            nint* table = *(nint**)media.Pointer;
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(media.Pointer);
            ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(media.Pointer);
        }
    }

    private static void Set(object target, string property, object? value)
        => target.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);
}
