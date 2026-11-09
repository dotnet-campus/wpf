using System.Reflection;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class IndependentMediaOpenPublicationTests
{
    /// <summary>Reflection assembles states around the production commit; this is not Native reader/preroll interleaving.</summary>
    [DataTestMethod]
    [DataRow(false, false, true)]
    [DataRow(true, false, false)]
    [DataRow(false, true, false)]
    public void WhenOpenCommitsThenOnlyCurrentActiveOwnerPublishes(bool stale, bool closing, bool committed)
    {
        var media = IndependentMediaIdentity.Create();
        var owner = new IndependentVideoPlaybackOwner();
        nint* table = *(nint**)media.Pointer;
        try
        {
            Set(owner, "Width", 2u);
            Set(owner, "Height", 1u);
            Set(owner, "Duration100ns", (long?)10000000);
            Field(media, "_owner", stale ? null : owner);
            Field(media, "_closing", closing);
            Set(media, "State", IndependentMediaState.Opening);
            byte before = 9, after = 9;
            ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[16])(media.Pointer, &before);
            bool result = (bool)typeof(IndependentMediaIdentity).GetMethod("TryCommitOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(media, [owner])!;
            ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[16])(media.Pointer, &after);
            Assert.AreEqual((committed, (byte)0, committed ? (byte)1 : (byte)0,
                committed ? IndependentMediaState.Opened : IndependentMediaState.Opening), (result, before, after, media.State));
        }
        finally
        {
            Field(media, "_closing", false);
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(media.Pointer);
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            owner.CloseAsync().WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(media.Pointer);
        }
    }

    private static void Set(object target, string property, object value)
        => target.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);

    private static void Field(object target, string field, object? value)
        => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);
}
