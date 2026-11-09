using System.Reflection;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class IndependentMediaCloseQueryTests
{
    /// <summary>Checks the six sourcecase COM getters before and after cleanup, including repeated Close.</summary>
    [DataTestMethod]
    [DataRow(6)]
    [DataRow(16)]
    [DataRow(17)]
    [DataRow(18)]
    [DataRow(19)]
    [DataRow(20)]
    public void WhenSourceIsClosedThenQueriesSucceedThroughRepeatedClose(int slot)
    {
        var media = IndependentMediaIdentity.Create();
        nint self = media.Pointer;
        nint* table = *(nint**)self;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(self));
            var immediate = Read(self, table, slot);
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            var completed = Read(self, table, slot);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(self));
            Assert.AreEqual(((0, 0L), (0, 0L), (0, 0L)), (immediate, completed, Read(self, table, slot)));
            Assert.AreEqual(unchecked((int)0x80004003),
                ((delegate* unmanaged[Stdcall]<nint, void*, int>)table[slot])(self, null));
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(self); }
        Assert.AreEqual((unchecked((int)0x80000013), 0L), Read(self, table, slot));
    }

    /// <summary>Reflection assembles known metadata; this is not a real Native asynchronous interleaving.</summary>
    [DataTestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void WhenSourceClosesThenPresenceCacheSurvivesStaleOwnerPublication(bool audio, bool video)
    {
        var media = IndependentMediaIdentity.Create();
        var owner = new IndependentVideoPlaybackOwner();
        nint self = media.Pointer;
        nint* table = *(nint**)self;
        try
        {
            typeof(IndependentMediaIdentity).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            Set(media, "State", IndependentMediaState.Opened);
            Set(owner, "HasAudio", (bool?)audio);
            Set(owner, "Width", video ? 2u : 0u);
            Set(owner, "Height", video ? 1u : 0u);
            Set(owner, "Duration100ns", (long?)10000000);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(self));
            var immediate = (Read(self, table, 17), Read(self, table, 16));
            Set(owner, "HasAudio", (bool?)!audio);
            Set(owner, "Width", video ? 0u : 2u);
            Set(owner, "Height", video ? 0u : 1u);
            typeof(IndependentMediaIdentity).GetMethod("PublishMetadata", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(media, [owner]);
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            var expected = ((0, audio ? 1L : 0L), (0, video ? 1L : 0L));
            Assert.AreEqual((expected, expected, (0, 0L), (0, 0L), (0, 0L), (0, 0L)),
                (immediate, (Read(self, table, 17), Read(self, table, 16)), Read(self, table, 19),
                    Read(self, table, 18), Read(self, table, 20), Read(self, table, 6)));
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(self); }
    }

    /// <summary>Close is reusable; opening a source again must not fail with a disposed-object HRESULT.</summary>
    [TestMethod]
    public void WhenClosedThenSubsequentOpenIsAcceptedAndSameSourceIsNoOp()
    {
        var media = IndependentMediaIdentity.Create();
        nint self = media.Pointer;
        nint* table = *(nint**)self;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(self));
            object gate = typeof(IndependentMediaIdentity).GetField("_gate", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(media)!;
            string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.mp4");
            lock (gate)
            {
                fixed (char* path = missing)
                {
                    int first = ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(self, path);
                    Task operation = media.Completion;
                    int same = ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(self, path);
                    Assert.AreEqual((0, 0, IndependentMediaState.Opening, operation), (first, same, media.State, media.Completion));
                }
            }
        }
        finally
        {
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(self);
            media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(self);
        }
    }

    /// <summary>Terminal shutdown still rejects every sourcecase getter.</summary>
    [DataTestMethod]
    [DataRow(6)]
    [DataRow(16)]
    [DataRow(17)]
    [DataRow(18)]
    [DataRow(19)]
    [DataRow(20)]
    public void WhenTerminalThenQueriesRemainInvalid(int slot)
    {
        var media = IndependentMediaIdentity.Create();
        nint self = media.Pointer;
        nint* table = *(nint**)self;
        try
        {
            ((delegate* unmanaged[Stdcall]<nint, int>)table[22])(self);
            Assert.AreEqual((unchecked((int)0x80000013), 0L), Read(self, table, slot));
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(self); }
    }

    private static (int Hr, long Value) Read(nint self, nint* table, int slot)
    {
        // All ABI output types fit here; use each real signature, not a widened output pointer.
        return slot switch
        {
            16 or 17 => ReadBoolean(self, table[slot]),
            18 or 19 => ReadDimension(self, table[slot]),
            _ => ReadTicks(self, table[slot])
        };
    }

    private static (int, long) ReadBoolean(nint self, nint method)
    {
        byte value = 9;
        int hr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)method)(self, &value);
        return (hr, value);
    }

    private static (int, long) ReadDimension(nint self, nint method)
    {
        uint value = 9;
        int hr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)method)(self, &value);
        return (hr, value);
    }

    private static (int, long) ReadTicks(nint self, nint method)
    {
        long value = 9;
        int hr = ((delegate* unmanaged[Stdcall]<nint, long*, int>)method)(self, &value);
        return (hr, value);
    }

    private static void Set(object target, string name, object? value)
        => target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.SetValue(target, value);
}
