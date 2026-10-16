using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class MediaExportsTests
{
    /// <summary>Measures references on a real system media object across public channel ownership boundaries.</summary>
    [DataTestMethod]
    [DataRow("RejectedSend")]
    [DataRow("UncommittedDestroy")]
    [DataRow("CommittedDestroy")]
    public void WhenCandidateMediaOwnershipIsMeasuredThenRealReferencesBalance(string scenario) => MeasureOwnership(true, scenario);

    /// <summary>Uses the installed native implementation as a separate reference, never as the candidate.</summary>
    [DataTestMethod]
    [DataRow("RejectedSend")]
    [DataRow("UncommittedDestroy")]
    [DataRow("CommittedDestroy")]
    public void WhenReferenceMediaOwnershipIsMeasuredThenRealReferencesBalance(string scenario) => MeasureOwnership(false, scenario);

    private static void MeasureOwnership(bool candidate, string scenario)
    {
        var source = new SystemMedia();
        string reference = Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll");
        string path = candidate ? AcceptancePaths.AotDll : reference;
        nint module = NativeLibrary.Load(path);
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var createChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var createResource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<uint, nint, nint, byte, int>)NativeLibrary.GetExport(module, "MilResource_SendCommandMedia");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var addRef = (delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source.Media)[1];
        var release = (delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source.Media)[2];
        uint Snapshot() { addRef(source.Media); return release(source.Media); }
        nint connection = 0, channel = 0;
        var calls = new List<object>();
        string progress = Path.Combine(AcceptancePaths.Results, $"required-progress-{candidate}-{scenario}.json");
        void Stage(string operation) => File.WriteAllText(progress, JsonSerializer.Serialize(new { candidate, scenario, operation, calls }));
        Stage("beforeQueryInterface");
        // The factory returns IMILMedia directly. CWmpPlayer::HrFindInterface rejects
        // IID_IMILMedia through its second if/else; query only COM identity here.
        Guid mediaId = new("00000000-0000-0000-C000-000000000046");
        nint queried = 0;
        int queriedResult = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)source.Media)[0])(source.Media, &mediaId, &queried);
        calls.Add(new { operation = "queryMedia", result = queriedResult });
        Assert.AreEqual(0, queriedResult);
        Release(queried);
        Stage("beforeInitialSnapshot");
        uint before = Snapshot(), after = 0;
        int sent = int.MinValue;
        int initialized = ((delegate* unmanaged[Stdcall]<int, int>)NativeLibrary.GetExport(module, "MilCompositionEngine_InitializePartitionManager"))(0);
        Assert.AreEqual(0, initialized);
        try
        {
            if (scenario != "RejectedSend")
            {
                Stage("beforeConnect");
                int result = connect(0, &connection);
                calls.Add(new { operation = "connect", result });
                Assert.AreEqual(0, result);
                Stage("beforeCreateChannel");
                result = createChannel(connection, 0, &channel);
                calls.Add(new { operation = "createChannel", result });
                Assert.AreEqual(0, result);
            }
            uint resource = 0;
            if (channel != 0)
            {
                Stage("beforeCreateResource");
                int result = createResource(channel, 1, &resource);
                calls.Add(new { operation = "createMediaResource", result });
                Assert.AreEqual(0, result);
            }
            Stage("beforeTransportAddRef");
            uint transportReferences = addRef(source.Media);
            calls.Add(new { operation = "transportAddRef", references = transportReferences });
            Stage("beforeSend");
            sent = send(resource, source.Media, channel, 0);
            calls.Add(new { operation = "sendMedia", result = sent });
            // The caller retains its extra reference on rejected SendCommand.
            if (sent < 0)
            {
                Stage("beforeRejectedTransportRelease");
                calls.Add(new { operation = "rejectedTransportRelease", references = release(source.Media) });
            }
            if (scenario == "RejectedSend") Assert.IsTrue(sent < 0);
            else Assert.AreEqual(0, sent);
            if (scenario == "CommittedDestroy")
            {
                Stage("beforeCloseBatch");
                int result = ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch"))(channel);
                calls.Add(new { operation = "closeBatch", result });
                Assert.AreEqual(0, result);
                Stage("beforeCommit");
                result = ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel"))(channel);
                calls.Add(new { operation = "commit", result });
                Assert.AreEqual(0, result);
                Stage("beforeSyncFlush");
                result = ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilComposition_SyncFlush"))(channel);
                calls.Add(new { operation = "syncFlush", result });
                Assert.AreEqual(0, result);
            }
            if (channel != 0)
            {
                nint closing = channel;
                channel = 0;
                Stage("beforeDestroy");
                int result = destroy(closing);
                calls.Add(new { operation = "destroyChannel", result });
                Assert.AreEqual(0, result);
            }
            if (connection != 0)
            {
                nint closing = connection;
                connection = 0;
                Stage("beforeDisconnect");
                int result = disconnect(closing);
                calls.Add(new { operation = "disconnect", result });
                Assert.AreEqual(0, result);
            }
            Stage("beforeFinalSnapshot");
            after = Snapshot();
            Stage("afterFinalSnapshot");
            Assert.AreEqual(before, after, "The retained real media object must return to its baseline reference count.");
        }
        finally
        {
            if (channel != 0) calls.Add(new { operation = "cleanupDestroy", result = destroy(channel) });
            if (connection != 0) calls.Add(new { operation = "cleanupDisconnect", result = disconnect(connection) });
            Stage("beforeDeinitialize");
            calls.Add(new { operation = "deinitialize", result = ((delegate* unmanaged[Stdcall]<int>)NativeLibrary.GetExport(module, "MilCompositionEngine_DeinitializePartitionManager"))() });
            Stage("beforeShutdownAndRootRelease");
            source.Dispose();
            Stage("afterShutdownAndRootRelease");
            File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"required-media-{candidate}-{scenario}.json"), JsonSerializer.Serialize(new
            {
                candidate, scenario, modulePath = path, moduleSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                mediaProvider = reference, runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                before, after, sent, calls, balanced = before == after, synchronousTransport = false,
                uncommittedDestroyIsDiscardOracle = false,
                boundary = "Public API and real IMILMedia IUnknown reference observations; not a native packet trace or final object destruction oracle"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
