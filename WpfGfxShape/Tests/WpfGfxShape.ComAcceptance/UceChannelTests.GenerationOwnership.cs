using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class UceChannelTests
{
    /// <summary>Compares legal native ownership transitions without assuming COM diagnostic counts.</summary>
    [DataTestMethod]
    [DataRow(false, false)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(true, true)]
    public void WhenSharedBitmapGenerationIsReplacedThenExternalOwnerAndHandleOwnersRemainIndependent(bool system, bool targetFirst)
    {
        string path = system
            ? Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll")
            : AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        nint Export(string name) => NativeLibrary.GetExport(module, name);
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var create = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        var release = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)Export("MilResource_ReleaseOnChannel");
        var duplicate = (delegate* unmanaged[Stdcall]<nint, uint, nint, uint*, int>)Export("MilResource_DuplicateHandle");
        var transfer = (delegate* unmanaged[Stdcall]<uint, nint, nint, int>)Export("MilResource_SendCommandBitmapSource");
        var wrap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)Export("MilResource_CreateCWICWrapperBitmap");
        var begin = (delegate* unmanaged[Stdcall]<nint, void*, uint, uint, int>)Export("MilChannel_BeginCommand");
        var end = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_EndCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
        using var wic = new SystemWicBitmap();
        nint connection = 0, source = 0, target = 0;
        nint[] streams = new nint[3], frames = new nint[3], wrappers = new nint[3];
        int[] disposed = new int[3];
        List<object> calls = [];
        bool completed = false;
        void Check(string operation, int result)
        {
            calls.Add(new { operation, result });
            Assert.AreEqual(0, result, operation);
        }
        void Snapshot(string operation, int a, int b, int c)
        {
            calls.Add(new { operation, disposed = disposed.ToArray() });
            CollectionAssert.AreEqual(new[] { a, b, c }, disposed, operation);
        }
        void ReleaseCom(ref nint value, string operation)
        {
            nint owned = value;
            value = 0;
            if (owned == 0) return;
            uint diagnosticCount = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)owned)[2])(owned);
            calls.Add(new { operation, diagnosticCount });
        }
        void Flush(nint channel, string operation)
        {
            Check(operation + ".Close", close(channel));
            Check(operation + ".Commit", commit(channel));
        }
        try
        {
            Check("Connect", connect(1, &connection));
            Check("Source", channelCreate(connection, 0, &source));
            Check("Target", channelCreate(connection, source, &target));
            uint handle = 0, alias = 0;
            Check("Create", create(source, 95, &handle));
            uint original = handle;
            Check("AddRefOnChannel", create(source, 95, &handle));
            Assert.AreEqual(original, handle);
            for (int i = 0; i < 3; i++)
            {
                byte[] bmp = new byte[62];
                bmp[0] = (byte)'B'; bmp[1] = (byte)'M';
                BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(2), bmp.Length);
                BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(10), 54);
                BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(14), 40);
                BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(18), 2);
                BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(22), 1);
                bmp[26] = 1; bmp[28] = 24; bmp[54 + i] = 255;
                int generation = i;
                streams[i] = ManagedStreamTests.CreateCallbackStream(module, bmp, () => { }, () => disposed[generation]++);
                frames[i] = wic.DecodeFrame(streams[i]);
                nint wrapper = 0;
                Check($"Wrap.{i}", wrap(frames[i], &wrapper));
                wrappers[i] = wrapper;
            }
            Check("Transfer.A", transfer(handle, wrappers[0], source));
            Flush(source, "Generation.A");
            Check("Duplicate", duplicate(source, handle, target, &alias));
            Flush(source, "Duplicate.Source");
            Flush(target, "Duplicate.Target");
            calls.Add(new { operation = "SharedIdentity", original, handle, alias });
            ReleaseCom(ref wrappers[0], "Caller.Wrapper.A");
            ReleaseCom(ref frames[0], "Caller.Frame.A");
            Check("Transfer.B.ThroughAlias", transfer(alias, wrappers[1], target));
            Flush(target, "Generation.B");
            ReleaseCom(ref wrappers[1], "Caller.Wrapper.B");
            ReleaseCom(ref frames[1], "Caller.Frame.B");
            ReleaseCom(ref streams[1], "Caller.Stream.B");
            Snapshot("AfterReplacement.OldExternalStreamStillOwned", 0, 0, 0);
            ReleaseCom(ref streams[0], "LastExternal.Stream.A");
            Snapshot("OldGenerationRetiredBeforeChannelsClose", 1, 0, 0);

            // A complete legal invalidate packet keeps SendCommand in its documented Begin/End guard.
            uint* invalidate = stackalloc uint[] { 0x0D, alias, 0, 0, 0, 0, 0 };
            Check("Begin.ValidInvalidate", begin(target, invalidate, 28, 0));
            int rejected = transfer(alias, wrappers[2], target);
            calls.Add(new { operation = "RejectedReplacement.C", result = rejected });
            Assert.AreEqual(unchecked((int)0x88980411), rejected);
            Check("End.ValidInvalidate", end(target));
            Flush(target, "AfterRejectedReplacement");
            ReleaseCom(ref wrappers[2], "Caller.Wrapper.C");
            ReleaseCom(ref frames[2], "Caller.Frame.C");
            ReleaseCom(ref streams[2], "LastExternal.Stream.C");
            Snapshot("RejectedGenerationReleasedAndCurrentGenerationRetained", 1, 0, 1);
            int deleted = -1;
            Check("Release.MasterNonfinal", release(source, handle, &deleted));
            Assert.AreEqual(0, deleted);
            Flush(source, "MasterNonfinal");
            Snapshot("MasterAddRefBalancedWithoutDeletingResource", 1, 0, 1);
            if (targetFirst)
            {
                Check("Destroy.TargetFirst", destroy(target)); target = 0;
                Snapshot("SourceHandleRetainsReplacementMadeThroughAlias", 1, 0, 1);
                Check("Release.MasterFinal", release(source, handle, &deleted));
                Assert.AreEqual(1, deleted);
                Flush(source, "DeleteResource.FinalOwner");
                Snapshot("DeleteResourceReleasesFinalGeneration", 1, 1, 1);
                Check("Destroy.SourceLast", destroy(source)); source = 0;
            }
            else
            {
                Check("Release.MasterFinal", release(source, handle, &deleted));
                Assert.AreEqual(1, deleted);
                Flush(source, "DeleteResource.SharedOwnerRemains");
                Check("Destroy.SourceFirst", destroy(source)); source = 0;
                Snapshot("AliasRetainsReplacementAfterSourceDeletion", 1, 0, 1);
                Check("Destroy.TargetLast", destroy(target)); target = 0;
            }
            Snapshot("AllExternalStreamsDisposedExactlyOnce", 1, 1, 1);
            completed = true;
        }
        finally
        {
            if (source != 0) destroy(source);
            if (target != 0) destroy(target);
            if (connection != 0) disconnect(connection);
            for (int i = 0; i < 3; i++)
            {
                ReleaseCom(ref wrappers[i], $"Cleanup.Wrapper.{i}");
                ReleaseCom(ref frames[i], $"Cleanup.Frame.{i}");
                ReleaseCom(ref streams[i], $"Cleanup.Stream.{i}");
            }
            Directory.CreateDirectory(AcceptancePaths.Results);
            File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"generation-owning-{system}-{targetFirst}.json"),
                JsonSerializer.Serialize(new { path, system, targetFirst, completed, calls, disposed,
                    sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                    scope = "Public ABI, real WIC decoder and callback stream. COM Release returns are diagnostic only; no internal counters or unload." },
                    new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
