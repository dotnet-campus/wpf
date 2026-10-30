#if RESOURCE_LIFECYCLE_HANDLE_TYPES
using System.Buffers.Binary;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class ProcessHandleTypeSnapshotTests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(15)]
    public void WhenTableHeaderIsShortThenParsingFails(int length)
    {
        Assert.IsFalse(ProcessHandleTypeSnapshot.TryCount(new byte[length], 8, out _));
    }

    [TestMethod]
    [DataRow(16385UL)]
    [DataRow(ulong.MaxValue)]
    [DataRow(1UL)]
    public void WhenCountExceedsBoundOrActualBufferThenParsingFails(ulong count)
    {
        byte[] data = new byte[16];
        BinaryPrimitives.WriteUInt64LittleEndian(data, count);
        Assert.IsFalse(ProcessHandleTypeSnapshot.TryCount(data, 8, out _));
    }

    [TestMethod]
    public void WhenPointerWidthIsNotX64ThenParsingFails()
    {
        Assert.IsFalse(ProcessHandleTypeSnapshot.TryCount(new byte[16], 4, out _));
    }

    [TestMethod]
    public void WhenOneCompleteEntryFitsThenCountIsOne()
    {
        byte[] data = new byte[56];
        BinaryPrimitives.WriteUInt64LittleEndian(data, 1);
        ProcessHandleTypeSnapshot.TryCount(data, 8, out int count);
        Assert.AreEqual(1, count);
    }

    [TestMethod]
    [DataRow(3, 4, 4200UL)]
    [DataRow(4, 3, 4200UL)]
    [DataRow(4, 4, 4095UL)]
    [DataRow(4, 4, 4206UL)]
    [DataRow(4, 4, ulong.MaxValue)]
    [DataRow(0, 0, 4200UL)]
    public void WhenUnicodeLengthOrPointerIsInvalidThenTypeIsUnknown(int length, int maximum, ulong address)
    {
        byte[] data = new byte[108];
        BinaryPrimitives.WriteUInt16LittleEndian(data, (ushort)length);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(2), (ushort)maximum);
        BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(8), address);
        Assert.IsNull(ProcessHandleTypeSnapshot.ParseType(data, 4096));
    }

    [TestMethod]
    public void WhenTypeHeaderIsShortThenTypeIsUnknown()
    {
        Assert.IsNull(ProcessHandleTypeSnapshot.ParseType(new byte[103], 4096));
    }

    [TestMethod]
    public void WhenUnicodeIsInActualBufferThenTypeIsParsed()
    {
        byte[] data = new byte[108];
        BinaryPrimitives.WriteUInt16LittleEndian(data, 4);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(2), 4);
        BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(8), 4200);
        data[104] = (byte)'A';
        data[106] = (byte)'B';
        Assert.AreEqual("AB", ProcessHandleTypeSnapshot.ParseType(data, 4096));
    }

    [TestMethod]
    [DataRow(0u, "TerminatedAtWait")]
    [DataRow(258u, "NotTerminatedAtWait")]
    [DataRow(uint.MaxValue, "Unknown")]
    public void WhenExitCodeIs259ThenWaitDeterminesObservation(uint wait, string expected)
    {
        var thread = new ProcessHandleTypeSnapshot.ThreadObservation(7, 123)
        {
            Wait = new("Succeeded", wait, null, 10, 20),
            ExitCode = new("Succeeded", 259, null, 21, 30)
        };
        Assert.AreEqual(expected, thread.State);
    }

    [TestMethod]
    public void WhenWaitFailsThenImmediateErrorIsPreserved()
    {
        var wait = new ProcessHandleTypeSnapshot.QueryObservation("Failed", uint.MaxValue, 5, 10, 20);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(wait));
        Assert.AreEqual(5, json.RootElement.GetProperty("Win32Error").GetInt32());
    }

    [TestMethod]
    public void WhenGetThreadIdReturnsZeroThenQueryFailed()
    {
        Assert.AreEqual("Failed", ProcessHandleTypeSnapshot.TidStatus(0));
    }

    [TestMethod]
    public void WhenRightsAreUnknownThenNoSynchronizePermissionIsClaimed()
    {
        Assert.AreEqual("Unknown;AttemptZeroWaitWithoutElevation",
            new ProcessHandleTypeSnapshot.ThreadObservation(0, 1).SynchronizePermission);
    }

    [TestMethod]
    public void WhenWaitWasNotObservedThen259DoesNotProveLiveness()
    {
        var thread = new ProcessHandleTypeSnapshot.ThreadObservation(0, 1)
        {
            ExitCode = new("Succeeded", 259, null, 1, 2)
        };
        Assert.AreEqual("Unknown", thread.State);
    }

    [TestMethod]
    public void WhenTimeoutPrecedesNon259ThenTransitionIsExplicit()
    {
        var thread = new ProcessHandleTypeSnapshot.ThreadObservation(0, 1)
        {
            Wait = new("Succeeded", 258, null, 1, 2),
            ExitCode = new("Succeeded", 0, null, 3, 4)
        };
        Assert.AreEqual("TransitionOrInconsistent", thread.Consistency);
    }

    [TestMethod]
    public void WhenErrorIs50ThenItIsNotAccessDenied()
    {
        Assert.AreEqual("OtherWin32Error", new ProcessHandleTypeSnapshot.QueryObservation("Failed", 0, 50, 1, 2).ErrorClass);
    }

    [TestMethod]
    public void WhenTwoEntriesShareTidThenHandleCountRetainsBoth()
    {
        var snapshot = new ProcessHandleTypeSnapshot.Snapshot("candidate", 4, "AfterCleanupPlusOneSecond");
        snapshot.Threads.Add(new(1, 12) { Tid = new("Succeeded", 77, null, 1, 2) });
        snapshot.Threads.Add(new(2, 16) { Tid = new("Succeeded", 77, null, 3, 4) });
        Assert.AreEqual(2, snapshot.HandlesPerTid[77]);
    }

    [TestMethod]
    public void WhenTidQueryFailsThenZeroIsNotCountedAsIdentity()
    {
        var snapshot = new ProcessHandleTypeSnapshot.Snapshot("candidate", 4, "AfterCleanupPlusOneSecond");
        snapshot.Threads.Add(new(1, 12) { Tid = new("Failed", 0, 5, 1, 2) });
        Assert.AreEqual(0, snapshot.HandlesPerTid.Count);
    }

    [TestMethod]
    public void WhenThreadSnapshotIsSerializedThenRawHandleIsNumericNotIntPtr()
    {
        var snapshot = new ProcessHandleTypeSnapshot.Snapshot("candidate", 4, "AfterCleanupPlusOneSecond");
        snapshot.Threads.Add(new(1, 123));
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(snapshot));
        Assert.AreEqual(123UL, json.RootElement.GetProperty("Threads")[0].GetProperty("OriginalHandleValue").GetUInt64());
    }

    [TestMethod]
    public void WhenCapabilityIsUnavailableThenUnknownIsNotZero()
    {
        Assert.IsNull(new ProcessHandleTypeSnapshot.Snapshot("original", -1, "Initial").Unknown);
    }

    [TestMethod]
    public void WhenSnapshotIsSerializedThenUnavailableStatusIsPreserved()
    {
        var value = new ProcessHandleTypeSnapshot.Snapshot("candidate", -1, "Initial");
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(value));
        Assert.AreEqual("Unavailable", json.RootElement.GetProperty("Status").GetString());
    }

    [TestMethod]
    public void WhenOriginalSampleGridIsUsedThenExactlyTenPointsAreSelected()
    {
        string[] phases = ["BeforeInitialize", "AfterSourceCloseTargetUpdate", "AfterCleanup", "AfterCleanupPlusOneSecond"];
        int count = 1 + Enumerable.Range(0, 16).Sum(round => phases.Count(phase => ProcessHandleTypeSnapshot.IsPoint(round, phase)));
        Assert.AreEqual(10, count);
    }

    [TestMethod]
    [DataRow(-1, "Initial")]
    [DataRow(0, "BeforeInitialize")]
    [DataRow(0, "AfterSourceCloseTargetUpdate")]
    [DataRow(0, "AfterCleanup")]
    [DataRow(0, "AfterCleanupPlusOneSecond")]
    [DataRow(3, "AfterCleanupPlusOneSecond")]
    [DataRow(4, "AfterSourceCloseTargetUpdate")]
    [DataRow(4, "AfterCleanupPlusOneSecond")]
    [DataRow(8, "AfterCleanupPlusOneSecond")]
    [DataRow(15, "AfterCleanupPlusOneSecond")]
    public void WhenPointWasPreregisteredThenItIsSelected(int round, string phase)
    {
        Assert.IsTrue(ProcessHandleTypeSnapshot.IsPoint(round, phase));
    }
}
#endif
