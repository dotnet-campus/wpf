using System.Collections.Immutable;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9DisplaySetManagerTests
{
    private static readonly Direct3D9DisplaySetCharacteristics Characteristics = new(
        127437408000000000,
        2,
        false,
        ImmutableArray<Direct3D9DisplayBounds>.Empty,
        ImmutableArray<Direct3D9Display>.Empty);

    [TestMethod]
    public void WhenCreationKeepsReportingDisplayStateInvalidThenOnlyFiveAttemptsAreMade()
    {
        int creationCount = 0;
        Direct3D9DisplaySetManager manager = new(
            () => 1,
            () => 2,
            (_, _) =>
            {
                creationCount++;
                Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
                return null!;
            });

        COMException exception = Assert.ThrowsExactly<COMException>(manager.DangerousGetLatestDisplaySet);

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, 5), (exception.HResult, creationCount));
    }

    [TestMethod]
    public void WhenUniquenessChangesDuringCreationThenCreationIsRetried()
    {
        uint displayUniqueness = 1;
        int creationCount = 0;
        Direct3D9DisplaySetManager manager = new(
            () => displayUniqueness,
            () => 2,
            (capturedDisplayUniqueness, capturedExternalUpdateCount) =>
            {
                creationCount++;
                Direct3D9DisplaySet displaySet = CreateDisplaySet(
                    capturedDisplayUniqueness,
                    capturedExternalUpdateCount,
                    () => displayUniqueness,
                    () => 2);
                if (creationCount == 1)
                {
                    displayUniqueness++;
                }

                return displaySet;
            });

        Direct3D9DisplaySet displaySet = manager.DangerousGetLatestDisplaySet();

        Assert.AreEqual((2, true), (creationCount, displaySet.IsUpToDate()));
    }

    [TestMethod]
    public void WhenDisplayStateInvalidIsTransientThenCreationIsRetried()
    {
        int creationCount = 0;
        Direct3D9DisplaySetManager manager = new(
            () => 1,
            () => 2,
            (displayUniqueness, externalUpdateCount) =>
            {
                creationCount++;
                if (creationCount == 1)
                {
                    Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
                }

                return CreateDisplaySet(displayUniqueness, externalUpdateCount, () => 1, () => 2);
            });

        Direct3D9DisplaySet displaySet = manager.DangerousGetLatestDisplaySet();

        Assert.AreEqual((2, true), (creationCount, displaySet.IsUpToDate()));
    }

    [TestMethod]
    public void WhenAnotherCreationPublishesCurrentSetThenCurrentSetWins()
    {
        int creationCount = 0;
        Direct3D9DisplaySet? concurrentlyPublished = null;
        Direct3D9DisplaySetManager manager = null!;
        manager = new Direct3D9DisplaySetManager(
            () => 1,
            () => 2,
            (displayUniqueness, externalUpdateCount) =>
            {
                creationCount++;
                if (creationCount == 1)
                {
                    concurrentlyPublished = manager.DangerousGetLatestDisplaySet();
                }

                return CreateDisplaySet(displayUniqueness, externalUpdateCount, () => 1, () => 2);
            });

        Direct3D9DisplaySet selected = manager.DangerousGetLatestDisplaySet();

        Assert.AreSame(concurrentlyPublished, selected);
    }

    [TestMethod]
    public void WhenOutOfDateExSetIsEquivalentThenCurrentSetIsReusedAndUniquenessIsUpdated()
    {
        uint displayUniqueness = 1;
        int notificationCount = 0;
        Direct3D9DisplaySet? firstCreated = null;
        Direct3D9DisplaySetManager manager = new(
            () => displayUniqueness,
            () => 2,
            (capturedDisplayUniqueness, capturedExternalUpdateCount) =>
            {
                Direct3D9DisplaySet displaySet = CreateDisplaySet(
                    capturedDisplayUniqueness,
                    capturedExternalUpdateCount,
                    () => displayUniqueness,
                    () => 2,
                    hasDirect3D9Ex: true);
                firstCreated ??= displaySet;
                return displaySet;
            },
            (_, _) => notificationCount++);
        Direct3D9DisplaySet original = manager.DangerousGetLatestDisplaySet();
        displayUniqueness++;

        Direct3D9DisplaySet selected = manager.DangerousGetLatestDisplaySet();

        Assert.AreEqual((true, true, 0), (ReferenceEquals(original, selected), selected.IsUpToDate(), notificationCount));
    }

    [TestMethod]
    public void WhenOutOfDateNonExSetIsEquivalentThenNewSetReplacesCurrentSet()
    {
        uint displayUniqueness = 1;
        (Direct3D9DisplaySet? Old, Direct3D9DisplaySet? New) notification = default;
        Direct3D9DisplaySetManager manager = new(
            () => displayUniqueness,
            () => 2,
            (capturedDisplayUniqueness, capturedExternalUpdateCount) => CreateDisplaySet(
                capturedDisplayUniqueness,
                capturedExternalUpdateCount,
                () => displayUniqueness,
                () => 2),
            (oldDisplaySet, newDisplaySet) => notification = (oldDisplaySet, newDisplaySet));
        Direct3D9DisplaySet original = manager.DangerousGetLatestDisplaySet();
        displayUniqueness++;

        Direct3D9DisplaySet selected = manager.DangerousGetLatestDisplaySet();

        Assert.AreEqual(
            (false, true, true),
            (ReferenceEquals(original, selected), ReferenceEquals(original, notification.Old), ReferenceEquals(selected, notification.New)));
    }

    [TestMethod]
    public void WhenUniquenessChangesDuringCreationThenUnpublishedDisplaySetIsDisposed()
    {
        uint displayUniqueness = 1;
        TrackingDisposable firstOwner = new();
        int creationCount = 0;
        Direct3D9DisplaySetManager manager = new(
            () => displayUniqueness,
            () => 2,
            (capturedDisplayUniqueness, capturedExternalUpdateCount) =>
            {
                creationCount++;
                Direct3D9DisplaySet displaySet = CreateDisplaySet(
                    capturedDisplayUniqueness,
                    capturedExternalUpdateCount,
                    () => displayUniqueness,
                    () => 2,
                    owner: creationCount == 1 ? firstOwner : null);
                if (creationCount == 1)
                {
                    displayUniqueness++;
                }

                return displaySet;
            });

        _ = manager.DangerousGetLatestDisplaySet();

        Assert.AreEqual(1, firstOwner.DisposeCount);
    }

    [TestMethod]
    public void WhenAnotherCreationPublishesCurrentSetThenLosingDisplaySetIsDisposed()
    {
        TrackingDisposable losingOwner = new();
        int creationCount = 0;
        Direct3D9DisplaySetManager manager = null!;
        manager = new Direct3D9DisplaySetManager(
            () => 1,
            () => 2,
            (displayUniqueness, externalUpdateCount) =>
            {
                creationCount++;
                bool isLosingCreation = creationCount == 1;
                if (isLosingCreation)
                {
                    _ = manager.DangerousGetLatestDisplaySet();
                }

                return CreateDisplaySet(
                    displayUniqueness,
                    externalUpdateCount,
                    () => 1,
                    () => 2,
                    owner: isLosingCreation ? losingOwner : null);
            });

        _ = manager.DangerousGetLatestDisplaySet();

        Assert.AreEqual(1, losingOwner.DisposeCount);
    }

    [TestMethod]
    public void WhenEquivalentExDisplaySetReusesCurrentThenTemporaryDisplaySetIsDisposed()
    {
        uint displayUniqueness = 1;
        TrackingDisposable temporaryOwner = new();
        int creationCount = 0;
        Direct3D9DisplaySetManager manager = new(
            () => displayUniqueness,
            () => 2,
            (capturedDisplayUniqueness, capturedExternalUpdateCount) =>
            {
                creationCount++;
                return CreateDisplaySet(
                    capturedDisplayUniqueness,
                    capturedExternalUpdateCount,
                    () => displayUniqueness,
                    () => 2,
                    hasDirect3D9Ex: true,
                    owner: creationCount == 2 ? temporaryOwner : null);
            });
        _ = manager.DangerousGetLatestDisplaySet();
        displayUniqueness++;

        _ = manager.DangerousGetLatestDisplaySet();

        Assert.AreEqual(1, temporaryOwner.DisposeCount);
    }

    [TestMethod]
    public void WhenManagedDisplaySetBecomesOutOfDateThenItUsesSameManagerAsLatestProvider()
    {
        uint displayUniqueness = 1;
        int creationCount = 0;
        Direct3D9DisplaySetManager manager = new(
            () => displayUniqueness,
            () => 2,
            (capturedDisplayUniqueness, capturedExternalUpdateCount) =>
            {
                creationCount++;
                return CreateDisplaySet(
                    capturedDisplayUniqueness,
                    capturedExternalUpdateCount,
                    () => displayUniqueness,
                    () => 2);
            });
        Direct3D9DisplaySet original = manager.DangerousGetLatestDisplaySet();
        displayUniqueness++;

        bool displayStateChanged = original.DangerousHasDisplayStateChanged();

        Assert.AreEqual((true, 2), (displayStateChanged, creationCount));
    }

    private static Direct3D9DisplaySet CreateDisplaySet(
        uint displayUniqueness,
        uint externalUpdateCount,
        Func<uint> displayUniquenessProvider,
        Func<uint> externalUpdateCountProvider,
        bool hasDirect3D9Ex = false,
        IDisposable? owner = null)
    {
        if (owner is null)
        {
            return new Direct3D9DisplaySet(
                Characteristics,
                displayUniqueness,
                externalUpdateCount,
                displayUniquenessProvider,
                externalUpdateCountProvider,
                hasDirect3D9Ex: hasDirect3D9Ex);
        }

        return new Direct3D9DisplaySet(
            Characteristics,
            displayUniqueness,
            externalUpdateCount,
            displayUniquenessProvider,
            externalUpdateCountProvider,
            new Direct3D9DisplaySetInitialization(0, null, owner),
            hasDirect3D9Ex);
    }

    private sealed class TrackingDisposable : IDisposable
    {
        internal int DisposeCount { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
        }
    }
}
