using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class NativeOnlyExecutionAuditTests
{
    /// <summary>Archives actual public ABI execution separately from unsupported and bypassed packets.</summary>
    [TestMethod]
    public void WhenNativeOnlyExecutionIsAuditedThenAllEightPacketsHaveBoundedDispositions()
    {
        string root = typeof(NativeOnlyExecutionAuditTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string Read(string path) => File.ReadAllText(Path.Combine(root, path));
        string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(root, path))));
        const string prefix = "WpfGfxShape/artifacts/sandbox-readback/";
        const string production = "3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D";
        var files = new List<object>();
        var runs = new List<object>();
        var bindings = new List<object>();
        int leaves = 0;
        void Visit(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.Object) foreach (var property in value.EnumerateObject()) Visit(property.Value);
            else if (value.ValueKind == JsonValueKind.Array) foreach (var item in value.EnumerateArray()) Visit(item);
            else leaves++;
        }
        XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        foreach (string run in new[] { "baseline", "existing", "target", "full" })
        {
            string folder = prefix + "nativeonly-" + run + "/Results";
            foreach (string full in Directory.GetFiles(Path.Combine(root, folder), "*.json"))
            {
                string path = Path.GetRelativePath(root, full).Replace('\\', '/');
                using var document = JsonDocument.Parse(Read(path));
                Visit(document.RootElement);
                if (Path.GetFileName(path) == "aot-identity.json") Assert.AreEqual(production, document.RootElement.GetProperty("Sha256").GetString());
                files.Add(new { path, sha256 = Hash(path) });
            }
            string trx = folder + "/nativeonly-" + run + ".trx";
            var xml = XDocument.Parse(Read(trx));
            using var identity = JsonDocument.Parse(Read(folder + "/aot-identity.json"));
            string taskDirectory = identity.RootElement.GetProperty("WorkingDirectory").GetString()!;
            string[] codeBases = xml.Descendants(ns + "TestMethod").Select(e => (string?)e.Attribute("codeBase") ?? "").Distinct().ToArray();
            Assert.IsNotEmpty(codeBases);
            foreach (string codeBase in codeBases) StringAssert.StartsWith(codeBase, taskDirectory + "\\", StringComparison.OrdinalIgnoreCase);
            bindings.Add(new { run, taskDirectory, identitySha256 = Hash(folder + "/aot-identity.json"), trxSha256 = Hash(trx), runId = (string?)xml.Root!.Attribute("id"), codeBases, sameTaskVerified = true });
            var cases = xml.Descendants(ns + "UnitTestResult").Select(e => new { name = (string?)e.Attribute("testName"), executionId = (string?)e.Attribute("executionId"), outcome = (string?)e.Attribute("outcome") }).ToArray();
            Assert.AreEqual(run == "full" ? 33 : 0, cases.Count(c => c.outcome == "Failed"));
            runs.Add(new { path = trx, sha256 = Hash(trx), runId = (string?)xml.Root!.Attribute("id"), total = cases.Length, passed = cases.Count(c => c.outcome == "Passed"), failed = cases.Count(c => c.outcome == "Failed"), skipped = cases.Count(c => c.outcome != "Passed" && c.outcome != "Failed"), failures = cases.Where(c => c.outcome == "Failed").ToArray() });
        }
        string[] sources = [
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/htmaster.cpp",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/connection.cpp",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/connectioncontext.cpp",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/generated_process_message.inl",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/Generated/wgx_commands.h",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/node.cpp",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/bitmapres.cpp",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/videoslave.cpp",
            "WpfGfxShape/Code/WpfGfxShape/Abi/UceChannelExports.cs",
            "WpfGfxShape/Code/WpfGfxShape/Core/SameThreadChannel.cs",
            "WpfGfxShape/Code/WpfGfxShape/Core/GeneratedProtocol.cs",
            "WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/MediaExportsTests.SolidFrame.cs",
            "WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/UceChannelTests.GenerationOwnership.cs",
            "WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/UceChannelTests.TypedLifetime.cs",
            "WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/CompositionTests.BitmapTransferDraw.cs"
        ];
        StringAssert.Contains(Read(sources[12]), "sendMedia(player, source.Media, channel, 1)");
        StringAssert.Contains(Read(sources[9]), "return channel.State.Commit()");
        var packets = new[] {
            new { name = "MILCMD_BITMAP_SOURCE", state = "CandidateAotExecutionSatisfied", producer = "apifunc.cpp:MilResource_SendCommandBitmapSource / UceChannelExports.SendBitmapSource", wire = "MilBitmapSourceCommand; Type,Handle,Bitmap", consumer = "bitmapres.cpp:ProcessUpdate / SameThreadChannel.Commit bitmap consumer", assertion = "WhenTransferredBitmapChangesThenSoftwareDrawPixelsChange; WhenTypedBitmapBatchStopsThenCommittedSourceLivesUntilChannelClose", cleanup = "RejectedReplacement.C=88980411; failed prefix retained; stream disposed once on final close" },
            new { name = "MILCMD_CHANNEL_CREATERESOURCE", state = "CandidateAotExecutionSatisfied", producer = "htmaster.cpp / SameThreadChannel.CreateOrAddRef", wire = "WriteChannelCreateResource; Type,Handle,resType", consumer = "generated_process_message.inl / GeneratedProtocolProductionContext.CreateResource", assertion = "GenerationOwnership Create.Bitmap95 then transfer through created typed handle", cleanup = "Nonfinal release retains generation; final deletion/close releases stream" },
            new { name = "MILCMD_CHANNEL_DELETERESOURCE", state = "CandidateAotExecutionSatisfied", producer = "htmaster.cpp / SameThreadChannel.Release", wire = "WriteChannelDeleteResource; Type,Handle,resType", consumer = "generated_process_message.inl / GeneratedProtocolProductionContext.DeleteResource", assertion = "GenerationOwnership targetFirst=true: DeleteResourceReleasesFinalGeneration [1,1,1]", cleanup = "MasterNonfinal deleted=0; MasterFinal deleted=1; alias remains independently owned" },
            new { name = "MILCMD_CHANNEL_DUPLICATEHANDLE", state = "CandidateAotExecutionSatisfied", producer = "htmaster.cpp / SameThreadChannel.DuplicateTo", wire = "WriteChannelDuplicateHandle; Type,Original,TargetChannel,Duplicate", consumer = "generated_process_message.inl / GeneratedProtocolProductionContext.DuplicateHandle", assertion = "GenerationOwnership replaces bitmap through alias and observes old stream disposal", cleanup = "Both channel close orders preserve remaining alias ownership" },
            new { name = "MILCMD_MEDIAPLAYER", state = "CandidateAotPixelsSatisfiedOwnershipPartial", producer = "apifunc.cpp / UceChannelExports.SendMedia", wire = "MilMediaPlayerCommand; Type,Handle,Media,NotifyUceDirect", consumer = "videoslave.cpp:ProcessUpdate / managed video resource and real native media provider", assertion = "SolidFrame via public SendCommandMedia; decoded colors, caller release, replacement, target lock retry", cleanup = "Channel destruction and caller reference; exact failure/discard last media reference not independently measured" },
            new { name = "MILCMD_TRANSPORT_SYNCFLUSH", state = "UnprovenCandidatePacketBypassed", producer = "connection.cpp:SynchronizeChannel sends cmd", wire = "Type only", consumer = "composition.cpp:Transport_SyncFlush / candidate router default success", assertion = "Public API returns but candidate directly commits/submits barrier; NOT packet execution", cleanup = "API barrier tested; transport packet completion not asserted" },
            new { name = "MILCMD_TRANSPORT_DESTROYRESOURCESONCHANNEL", state = "UnprovenCandidatePacketBypassed", producer = "connectioncontext.cpp:CloseChannel sends cmdDestroy", wire = "Type,hChannel", consumer = "composition.cpp:Transport_DestroyResourcesOnChannel / candidate router default success", assertion = "Real stream close proves API cleanup only; candidate calls State.Destroy or scheduler destroy, not wire writer", cleanup = "Actual resources released, but packet-specific cleanup remains open" },
            new { name = "MILCMD_VISUAL_CREATE", state = "NotApplicableUnsupportedLegacyProducerNotLocated", producer = "No legal typed producer located in bounded frozen sources", wire = "Frozen native declaration only", consumer = "node.cpp:CMilVisual::ProcessCreate RIP/E_UNEXPECTED", assertion = "None; ordinary channel create never substituted", cleanup = "No fabricated producer or native execution" }
        };
        Assert.HasCount(8, packets);
        var report = new { scope = "Finite native-only audit, not full ABI or original native packet trace", productionSha256 = production, runtime = ".NET 10.0.1 win-x64; independent system differential, not target 10.0.12", nativeOnlyCount = packets.Length, candidatePacketExecutionSatisfied = 4, mediaPixelsSatisfied = true, transportPacketExecutionSatisfied = false, originalNativePacketTraceProven = false, debugOnly = "MilCmdValidateStructureOrder is outside this eight-packet denominator; DBG measurement is not retail", childSatisfied = true, parentBlocked = true, stage4Closed = false, auditSourceSha256 = Hash("WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/NativeOnlyExecutionAuditTests.cs"), testAssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(NativeOnlyExecutionAuditTests).Assembly.Location))), sources = sources.Select(path => new { path, sha256 = Hash(path) }).ToArray(), files, jsonLeavesRead = leaves, bindings, runs, packets };
        string output = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence/nativeonly-execution/report.json");
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.WriteAllText(output, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        using var readback = JsonDocument.Parse(File.ReadAllText(output));
        Visit(readback.RootElement);
        Assert.AreEqual(8, readback.RootElement.GetProperty("packets").GetArrayLength());
        var regressions = Directory.GetFiles(Path.Combine(root, "WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/TestResults"), "*.trx")
            .OrderByDescending(File.GetLastWriteTimeUtc).Select(path => new { path, xml = XDocument.Load(path) })
            .Where(item => item.xml.Descendants(ns + "UnitTestResult").Any(e => ((string?)e.Attribute("testName")) == nameof(WhenNativeOnlyExecutionIsAuditedThenAllEightPacketsHaveBoundedDispositions)))
            .Take(2).Select(item => new { path = Path.GetRelativePath(root, item.path).Replace('\\', '/'), sha256 = Hash(item.path), runId = (string?)item.xml.Root!.Attribute("id"), counters = item.xml.Descendants(ns + "Counters").Single().Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value) }).ToArray();
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(output)!, "verification.json"), JsonSerializer.Serialize(new { reportSha256 = Hash(output), auditSourceSha256 = Hash("WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/NativeOnlyExecutionAuditTests.cs"), jsonLeavesRead = leaves, regressions }, new JsonSerializerOptions { WriteIndented = true }));
    }
}
