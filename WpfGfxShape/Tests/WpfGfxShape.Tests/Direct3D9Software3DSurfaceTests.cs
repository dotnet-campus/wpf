using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed class Direct3D9Software3DSurfaceTests
{
    [TestMethod]
    public void WhenCopyCompatibleSurfaceDrawsThenBoundsAreCopiedForwardAndBack()
    {
        byte[] target = CreatePixels(4, 3);
        byte[] surface = new byte[target.Length];
        List<string> calls = [];
        Direct3D9SurfaceRect bounds = new(1, 1, 3, 3);
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            surface,
            calls);

        int beginResult = softwareSurface.BeginSw3D(target, 16, bounds, useZBuffer: false, z: null);
        bool copiedForward = SurfaceRegionEquals(target, surface, bounds, 16);
        int drawResult = softwareSurface.DrawMesh3D(() =>
        {
            calls.Add("Draw");
            FillRegion(surface, bounds, 16, 0xA5);
            return 0;
        });
        int endResult = softwareSurface.EndSw3D(target, 16);

        Assert.AreEqual(
            (0, true, 0, 0, false, true, true, "Ensure|Lock:1,1,3,3|Unlock|Draw|End|Lock:1,1,3,3|Unlock"),
            (beginResult, copiedForward, drawResult, endResult, softwareSurface.In3D, softwareSurface.SurfaceDirty, RegionHasValue(target, bounds, 16, 0xA5), string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenBlendSurfaceBeginsThenBoundsAreClearedBeforeOptionalDepthInitialization()
    {
        byte[] target = CreatePixels(4, 3);
        byte[] surface = Enumerable.Repeat((byte) 0x7F, target.Length).ToArray();
        List<string> calls = [];
        Direct3D9SurfaceRect bounds = new(1, 0, 4, 2);
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Bgra32Bpp,
            surface,
            calls,
            begin3D: (z, useZBuffer) =>
            {
                calls.Add($"Depth:{z}:{useZBuffer}:{RegionHasValue(surface, bounds, 16, 0)}");
                return 0;
            });

        int result = softwareSurface.BeginSw3D(target, 16, bounds, useZBuffer: true, z: 0.25f);

        Assert.AreEqual(
            (0, true, false, "Ensure|Lock:1,0,4,2|Unlock|Depth:0.25:True:True"),
            (result, softwareSurface.In3D, softwareSurface.SurfaceDirty, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDepthInitializationFailsThenBoundsAreRestoredAndSurfaceIsUnlocked()
    {
        byte[] target = CreatePixels(4, 3);
        byte[] surface = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Bgr32Bpp,
            surface,
            calls,
            begin3D: (_, _) =>
            {
                calls.Add("Depth");
                return Direct3D9Factory.GenericFailureHResult;
            });

        int result = softwareSurface.BeginSw3D(
            target,
            16,
            new Direct3D9SurfaceRect(1, 1, 3, 2),
            useZBuffer: true,
            z: 1f);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, new Direct3D9SurfaceRect(0, 0, 4, 3), "Ensure|Lock:1,1,3,2|Unlock|Depth"),
            (result, softwareSurface.In3D, softwareSurface.Bounds, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenBeginIsReenteredThenEnsureAndStateAreUnchanged()
    {
        byte[] target = CreatePixels(4, 3);
        List<string> calls = [];
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            new byte[target.Length],
            calls);
        _ = softwareSurface.BeginSw3D(
            target,
            16,
            new Direct3D9SurfaceRect(1, 1, 3, 2),
            useZBuffer: false,
            z: null);
        Direct3D9SurfaceRect boundsBeforeReentry = softwareSurface.Bounds;

        int result = softwareSurface.BeginSw3D(
            target,
            16,
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            useZBuffer: false,
            z: null);

        Assert.AreEqual(
            (Direct3D9Factory.WgxInvalidCallHResult, true, false, boundsBeforeReentry, "Ensure|Lock:1,1,3,2|Unlock"),
            (result, softwareSurface.In3D, softwareSurface.SurfaceDirty, softwareSurface.Bounds, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenEnsureFailsThenBeginStateAndDirtyFlagArePreserved()
    {
        byte[] target = CreatePixels(4, 3);
        byte[] surface = new byte[target.Length];
        int ensureCount = 0;
        Direct3D9Software3DSurface softwareSurface = new(
            width: 4,
            height: 3,
            MilPixelFormat.Pbgra32Bpp,
            ensureSurface: () => ++ensureCount == 1 ? 0 : Direct3D9Factory.GenericFailureHResult,
            lockSurface: (Direct3D9SurfaceRect bounds, out Direct3D9LockedPixelBuffer lockedBuffer) =>
            {
                lockedBuffer = new Direct3D9LockedPixelBuffer(surface, bounds.Top * 16 + bounds.Left * 4, 16);
                return 0;
            },
            unlockSurface: () => 0,
            begin3DInternal: (_, _) => 0,
            end3D: () => 0,
            blendWithSoftwareTarget: (_, _) => 0);
        _ = softwareSurface.BeginSw3D(target, 16, new Direct3D9SurfaceRect(1, 1, 3, 2), false, null);
        _ = softwareSurface.DrawMesh3D(() => 0);
        _ = softwareSurface.EndSw3D(target, 16);
        Direct3D9SurfaceRect boundsBeforeFailure = softwareSurface.Bounds;

        int result = softwareSurface.BeginSw3D(target, 16, default, false, null);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, true, boundsBeforeFailure, 2),
            (result, softwareSurface.In3D, softwareSurface.SurfaceDirty, softwareSurface.Bounds, ensureCount));
    }

    [TestMethod]
    public void WhenLockedBufferIsInvalidThenItIsUnlockedOnceAndInitializationErrorWins()
    {
        byte[] target = CreatePixels(4, 3);
        List<string> calls = [];
        Direct3D9Software3DSurface softwareSurface = new(
            width: 4,
            height: 3,
            MilPixelFormat.Pbgra32Bpp,
            ensureSurface: () => 0,
            lockSurface: (Direct3D9SurfaceRect bounds, out Direct3D9LockedPixelBuffer lockedBuffer) =>
            {
                calls.Add("Lock");
                lockedBuffer = new Direct3D9LockedPixelBuffer([], 0, 0);
                return 0;
            },
            unlockSurface: () =>
            {
                calls.Add("Unlock");
                return Direct3D9Factory.GenericFailureHResult;
            },
            begin3DInternal: (_, _) =>
            {
                calls.Add("Depth");
                return 0;
            },
            end3D: () => 0,
            blendWithSoftwareTarget: (_, _) => 0);

        int result = softwareSurface.BeginSw3D(
            target,
            16,
            new Direct3D9SurfaceRect(1, 1, 3, 2),
            useZBuffer: true,
            z: 1f);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, false, new Direct3D9SurfaceRect(0, 0, 4, 3), "Lock|Unlock"),
            (result, softwareSurface.In3D, softwareSurface.Bounds, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenUnlockFailsThenDepthInitializationIsSkippedAndBoundsAreRestored()
    {
        byte[] target = CreatePixels(4, 3);
        byte[] surface = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface softwareSurface = new(
            width: 4,
            height: 3,
            MilPixelFormat.Pbgra32Bpp,
            ensureSurface: () => 0,
            lockSurface: (Direct3D9SurfaceRect bounds, out Direct3D9LockedPixelBuffer lockedBuffer) =>
            {
                calls.Add("Lock");
                lockedBuffer = new Direct3D9LockedPixelBuffer(surface, bounds.Top * 16 + bounds.Left * 4, 16);
                return 0;
            },
            unlockSurface: () =>
            {
                calls.Add("Unlock");
                return Direct3D9Factory.GenericFailureHResult;
            },
            begin3DInternal: (_, _) =>
            {
                calls.Add("Depth");
                return 0;
            },
            end3D: () => 0,
            blendWithSoftwareTarget: (_, _) => 0);

        int result = softwareSurface.BeginSw3D(
            target,
            16,
            new Direct3D9SurfaceRect(1, 1, 3, 2),
            useZBuffer: true,
            z: 1f);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, new Direct3D9SurfaceRect(0, 0, 4, 3), "Lock|Unlock"),
            (result, softwareSurface.In3D, softwareSurface.Bounds, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenBoundsAreEmptyThenBeginEntersPairedStateWithoutLockingOrDepthInitialization()
    {
        byte[] target = CreatePixels(4, 3);
        List<string> calls = [];
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            new byte[target.Length],
            calls,
            begin3D: (_, _) =>
            {
                calls.Add("Depth");
                return 0;
            });

        int result = softwareSurface.BeginSw3D(
            target,
            16,
            new Direct3D9SurfaceRect(4, 3, 4, 3),
            useZBuffer: true,
            z: 1f);

        Assert.AreEqual(
            (0, true, false, new Direct3D9SurfaceRect(4, 3, 4, 3), "Ensure"),
            (result, softwareSurface.In3D, softwareSurface.SurfaceDirty, softwareSurface.Bounds, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDrawIsCalledOutsideThreeDThenInvalidCallIsReturnedWithoutChangingState()
    {
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            new byte[48],
            []);
        int drawCalls = 0;

        int result = softwareSurface.DrawMesh3D(() =>
        {
            drawCalls++;
            return Direct3D9Factory.SuccessHResult;
        });

        Assert.AreEqual(
            (Direct3D9Factory.WgxInvalidCallHResult, 0, false, false, new Direct3D9SurfaceRect(0, 0, 4, 3)),
            (result, drawCalls, softwareSurface.In3D, softwareSurface.SurfaceDirty, softwareSurface.Bounds));
    }

    [TestMethod]
    public void WhenDrawBoundsAreEmptyThenDelegateIsSkippedAndSurfaceRemainsClean()
    {
        byte[] target = CreatePixels(4, 3);
        List<string> calls = [];
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            new byte[target.Length],
            calls);
        _ = softwareSurface.BeginSw3D(
            target,
            16,
            new Direct3D9SurfaceRect(4, 3, 4, 3),
            useZBuffer: false,
            z: null);
        int drawCalls = 0;

        int result = softwareSurface.DrawMesh3D(() =>
        {
            drawCalls++;
            return Direct3D9Factory.GenericFailureHResult;
        });

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, 0, true, false, new Direct3D9SurfaceRect(4, 3, 4, 3), "Ensure"),
            (result, drawCalls, softwareSurface.In3D, softwareSurface.SurfaceDirty, softwareSurface.Bounds, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDrawDelegateFailsThenFailureIsPreservedAndSurfaceRemainsClean()
    {
        byte[] target = CreatePixels(4, 3);
        List<string> calls = [];
        Direct3D9SurfaceRect bounds = new(1, 1, 3, 2);
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            new byte[target.Length],
            calls);
        _ = softwareSurface.BeginSw3D(target, 16, bounds, useZBuffer: false, z: null);

        int result = softwareSurface.DrawMesh3D(() =>
        {
            calls.Add("DrawFailure");
            return Direct3D9Factory.GenericFailureHResult;
        });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, false, bounds, "Ensure|Lock:1,1,3,2|Unlock|DrawFailure"),
            (result, softwareSurface.In3D, softwareSurface.SurfaceDirty, softwareSurface.Bounds, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDrawDelegateSucceedsThenSurfaceIsMarkedDirtyAndSavedBoundsAreComposited()
    {
        byte[] target = CreatePixels(4, 3);
        byte[] surface = new byte[target.Length];
        List<string> calls = [];
        Direct3D9SurfaceRect bounds = new(1, 1, 3, 2);
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            surface,
            calls);
        _ = softwareSurface.BeginSw3D(target, 16, bounds, useZBuffer: false, z: null);

        int drawResult = softwareSurface.DrawMesh3D(() =>
        {
            calls.Add("DrawSuccess");
            FillRegion(surface, bounds, 16, 0x6B);
            return Direct3D9Factory.SuccessHResult;
        });
        bool dirtyAfterDraw = softwareSurface.SurfaceDirty;
        int endResult = softwareSurface.EndSw3D(target, 16);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, true, Direct3D9Factory.SuccessHResult, false, new Direct3D9SurfaceRect(0, 0, 4, 3), true,
                "Ensure|Lock:1,1,3,2|Unlock|DrawSuccess|End|Lock:1,1,3,2|Unlock"),
            (drawResult, dirtyAfterDraw, endResult, softwareSurface.In3D, softwareSurface.Bounds,
                RegionHasValue(target, bounds, 16, 0x6B), string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenEnding3DFailsThenCompositeIsSkippedAndThreeDStateIsExited()
    {
        byte[] target = CreatePixels(4, 3);
        byte[] original = target.ToArray();
        byte[] surface = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            surface,
            calls,
            end3D: () =>
            {
                calls.Add("End");
                return Direct3D9Factory.GenericFailureHResult;
            });
        _ = softwareSurface.BeginSw3D(
            target,
            16,
            new Direct3D9SurfaceRect(1, 1, 3, 2),
            useZBuffer: false,
            z: null);
        _ = softwareSurface.DrawMesh3D(() =>
        {
            calls.Add("Draw");
            return 0;
        });

        int result = softwareSurface.EndSw3D(target, 16);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, true, new Direct3D9SurfaceRect(0, 0, 4, 3), true, "Ensure|Lock:1,1,3,2|Unlock|Draw|End"),
            (result, softwareSurface.In3D, softwareSurface.SurfaceDirty, softwareSurface.Bounds, target.SequenceEqual(original), string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSurfaceIsNotDirtyThenEndRestoresBoundsWithoutLocking()
    {
        byte[] target = CreatePixels(4, 3);
        List<string> calls = [];
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            new byte[target.Length],
            calls);
        _ = softwareSurface.BeginSw3D(
            target,
            16,
            new Direct3D9SurfaceRect(1, 1, 3, 2),
            useZBuffer: false,
            z: null);

        int result = softwareSurface.EndSw3D(target, 16);

        Assert.AreEqual(
            (0, false, new Direct3D9SurfaceRect(0, 0, 4, 3), "Ensure|Lock:1,1,3,2|Unlock|End"),
            (result, softwareSurface.In3D, softwareSurface.Bounds, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDirtyBoundsAreEmptyThenEndSkipsComposite()
    {
        byte[] target = CreatePixels(4, 3);
        List<string> calls = [];
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            new byte[target.Length],
            calls);
        _ = softwareSurface.BeginSw3D(
            target,
            16,
            new Direct3D9SurfaceRect(4, 3, 4, 3),
            useZBuffer: false,
            z: null);
        _ = softwareSurface.DrawMesh3D(() => Direct3D9Factory.SuccessHResult);

        int result = softwareSurface.EndSw3D(target, 16);

        Assert.AreEqual(
            (0, false, false, new Direct3D9SurfaceRect(0, 0, 4, 3), "Ensure|End"),
            (result, softwareSurface.In3D, softwareSurface.SurfaceDirty, softwareSurface.Bounds, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenCompositeTargetBufferIsInvalidThenSurfaceIsLockedAndUnlockedOnce()
    {
        byte[] surface = CreatePixels(4, 3);
        List<string> calls = [];
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            surface,
            calls);
        byte[] beginTarget = CreatePixels(4, 3);
        _ = softwareSurface.BeginSw3D(beginTarget, 16, new Direct3D9SurfaceRect(1, 1, 3, 2), false, null);
        _ = softwareSurface.DrawMesh3D(() => Direct3D9Factory.SuccessHResult);

        int result = softwareSurface.EndSw3D([], 16);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, false, new Direct3D9SurfaceRect(0, 0, 4, 3), "Ensure|Lock:1,1,3,2|Unlock|End|Lock:1,1,3,2|Unlock"),
            (result, softwareSurface.In3D, softwareSurface.Bounds, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenCompositeValidationAndUnlockFailThenValidationErrorWins()
    {
        byte[] target = CreatePixels(4, 3);
        byte[] surface = new byte[target.Length];
        int lockCount = 0;
        int unlockCount = 0;
        Direct3D9Software3DSurface softwareSurface = new(
            width: 4,
            height: 3,
            MilPixelFormat.Pbgra32Bpp,
            ensureSurface: () => Direct3D9Factory.SuccessHResult,
            lockSurface: (Direct3D9SurfaceRect bounds, out Direct3D9LockedPixelBuffer lockedBuffer) =>
            {
                lockCount++;
                lockedBuffer = lockCount == 1
                    ? new Direct3D9LockedPixelBuffer(surface, bounds.Top * 16 + bounds.Left * 4, 16)
                    : new Direct3D9LockedPixelBuffer([], 0, 0);
                return Direct3D9Factory.SuccessHResult;
            },
            unlockSurface: () =>
            {
                unlockCount++;
                return unlockCount == 1
                    ? Direct3D9Factory.SuccessHResult
                    : Direct3D9Factory.GenericFailureHResult;
            },
            begin3DInternal: (_, _) => Direct3D9Factory.SuccessHResult,
            end3D: () => Direct3D9Factory.SuccessHResult,
            blendWithSoftwareTarget: (_, _) => Direct3D9Factory.SuccessHResult);
        _ = softwareSurface.BeginSw3D(target, 16, new Direct3D9SurfaceRect(1, 1, 3, 2), false, null);
        _ = softwareSurface.DrawMesh3D(() => Direct3D9Factory.SuccessHResult);

        int result = softwareSurface.EndSw3D(target, 16);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, 2, 2, false),
            (result, lockCount, unlockCount, softwareSurface.In3D));
    }

    [TestMethod]
    public void WhenBlendFailsThenSurfaceIsUnlockedAndBlendErrorWins()
    {
        byte[] target = CreatePixels(4, 3);
        byte[] surface = new byte[target.Length];
        int unlockCount = 0;
        Direct3D9Software3DSurface softwareSurface = new(
            width: 4,
            height: 3,
            MilPixelFormat.Bgra32Bpp,
            ensureSurface: () => Direct3D9Factory.SuccessHResult,
            lockSurface: (Direct3D9SurfaceRect bounds, out Direct3D9LockedPixelBuffer lockedBuffer) =>
            {
                lockedBuffer = new Direct3D9LockedPixelBuffer(surface, bounds.Top * 16 + bounds.Left * 4, 16);
                return Direct3D9Factory.SuccessHResult;
            },
            unlockSurface: () =>
            {
                unlockCount++;
                return unlockCount == 1
                    ? Direct3D9Factory.SuccessHResult
                    : Direct3D9Factory.InvalidArgumentHResult;
            },
            begin3DInternal: (_, _) => Direct3D9Factory.SuccessHResult,
            end3D: () => Direct3D9Factory.SuccessHResult,
            blendWithSoftwareTarget: (_, _) => Direct3D9Factory.GenericFailureHResult);
        _ = softwareSurface.BeginSw3D(target, 16, new Direct3D9SurfaceRect(1, 1, 3, 2), false, null);
        _ = softwareSurface.DrawMesh3D(() => Direct3D9Factory.SuccessHResult);

        int result = softwareSurface.EndSw3D(target, 16);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 2, false, new Direct3D9SurfaceRect(0, 0, 4, 3)),
            (result, unlockCount, softwareSurface.In3D, softwareSurface.Bounds));
    }

    [TestMethod]
    public void WhenCompositeSucceedsAndUnlockFailsThenUnlockErrorIsReturned()
    {
        byte[] target = CreatePixels(4, 3);
        byte[] surface = new byte[target.Length];
        int unlockCount = 0;
        Direct3D9Software3DSurface softwareSurface = new(
            width: 4,
            height: 3,
            MilPixelFormat.Pbgra32Bpp,
            ensureSurface: () => Direct3D9Factory.SuccessHResult,
            lockSurface: (Direct3D9SurfaceRect bounds, out Direct3D9LockedPixelBuffer lockedBuffer) =>
            {
                lockedBuffer = new Direct3D9LockedPixelBuffer(surface, bounds.Top * 16 + bounds.Left * 4, 16);
                return Direct3D9Factory.SuccessHResult;
            },
            unlockSurface: () => ++unlockCount == 1
                ? Direct3D9Factory.SuccessHResult
                : Direct3D9Factory.GenericFailureHResult,
            begin3DInternal: (_, _) => Direct3D9Factory.SuccessHResult,
            end3D: () => Direct3D9Factory.SuccessHResult,
            blendWithSoftwareTarget: (_, _) => Direct3D9Factory.SuccessHResult);
        _ = softwareSurface.BeginSw3D(target, 16, new Direct3D9SurfaceRect(1, 1, 3, 2), false, null);
        _ = softwareSurface.DrawMesh3D(() => Direct3D9Factory.SuccessHResult);

        int result = softwareSurface.EndSw3D(target, 16);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 2, false, new Direct3D9SurfaceRect(0, 0, 4, 3)),
            (result, unlockCount, softwareSurface.In3D, softwareSurface.Bounds));
    }

    [TestMethod]
    public void WhenResizedSurfaceIsEnsuredThenNewBoundsAreCommitted()
    {
        byte[] target = CreatePixels(6, 2);
        byte[] surface = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface softwareSurface = new(
            width: 4,
            height: 3,
            MilPixelFormat.Pbgra32Bpp,
            ensureSurface: () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            lockSurface: (Direct3D9SurfaceRect bounds, out Direct3D9LockedPixelBuffer lockedBuffer) =>
            {
                calls.Add($"Lock:{bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}");
                lockedBuffer = new Direct3D9LockedPixelBuffer(surface, bounds.Top * 24 + bounds.Left * 4, 24);
                return 0;
            },
            unlockSurface: () => 0,
            begin3DInternal: (_, _) => 0,
            end3D: () => 0,
            blendWithSoftwareTarget: (_, _) => 0);
        softwareSurface.Resize(6, 2);

        int result = softwareSurface.BeginSw3D(
            target,
            24,
            new Direct3D9SurfaceRect(0, 0, 8, 4),
            useZBuffer: false,
            z: null);

        Assert.AreEqual(
            (0, new Direct3D9SurfaceRect(0, 0, 6, 2), "Ensure|Lock:0,0,6,2"),
            (result, softwareSurface.Bounds, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenEnsuringResizedSurfaceFailsThenOldBoundsRemainUntilRetrySucceeds()
    {
        byte[] target = CreatePixels(6, 3);
        byte[] surface = new byte[target.Length];
        int ensureCount = 0;
        Direct3D9Software3DSurface softwareSurface = new(
            width: 4,
            height: 3,
            MilPixelFormat.Pbgra32Bpp,
            ensureSurface: () => ++ensureCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0,
            lockSurface: (Direct3D9SurfaceRect bounds, out Direct3D9LockedPixelBuffer lockedBuffer) =>
            {
                lockedBuffer = new Direct3D9LockedPixelBuffer(surface, bounds.Top * 24 + bounds.Left * 4, 24);
                return 0;
            },
            unlockSurface: () => 0,
            begin3DInternal: (_, _) => 0,
            end3D: () => 0,
            blendWithSoftwareTarget: (_, _) => 0);
        softwareSurface.Resize(6, 2);

        int firstResult = softwareSurface.BeginSw3D(
            target,
            24,
            new Direct3D9SurfaceRect(0, 0, 6, 2),
            useZBuffer: false,
            z: null);
        Direct3D9SurfaceRect boundsAfterFailure = softwareSurface.Bounds;
        int retryResult = softwareSurface.BeginSw3D(
            target,
            24,
            new Direct3D9SurfaceRect(0, 0, 6, 2),
            useZBuffer: false,
            z: null);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, new Direct3D9SurfaceRect(0, 0, 4, 3), 0, new Direct3D9SurfaceRect(0, 0, 6, 2), 2),
            (firstResult, boundsAfterFailure, retryResult, softwareSurface.Bounds, ensureCount));
    }

    [TestMethod]
    public void WhenSameSizeBeginsRepeatedlyThenExistingBoundsAreReused()
    {
        byte[] target = CreatePixels(4, 3);
        byte[] surface = new byte[target.Length];
        int ensureCount = 0;
        Direct3D9Software3DSurface softwareSurface = new(
            width: 4,
            height: 3,
            MilPixelFormat.Pbgra32Bpp,
            ensureSurface: () =>
            {
                ensureCount++;
                return Direct3D9Factory.SuccessHResult;
            },
            lockSurface: (Direct3D9SurfaceRect bounds, out Direct3D9LockedPixelBuffer lockedBuffer) =>
            {
                lockedBuffer = new Direct3D9LockedPixelBuffer(surface, bounds.Top * 16 + bounds.Left * 4, 16);
                return Direct3D9Factory.SuccessHResult;
            },
            unlockSurface: () => Direct3D9Factory.SuccessHResult,
            begin3DInternal: (_, _) => Direct3D9Factory.SuccessHResult,
            end3D: () => Direct3D9Factory.SuccessHResult,
            blendWithSoftwareTarget: (_, _) => Direct3D9Factory.SuccessHResult);
        softwareSurface.Resize(4, 3);

        int firstResult = softwareSurface.BeginSw3D(
            target,
            16,
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            useZBuffer: false,
            z: null);
        _ = softwareSurface.EndSw3D(target, 16);
        int secondResult = softwareSurface.BeginSw3D(
            target,
            16,
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            useZBuffer: false,
            z: null);

        Assert.AreEqual(
            (0, 0, new Direct3D9SurfaceRect(0, 0, 4, 3), 2),
            (firstResult, secondResult, softwareSurface.Bounds, ensureCount));
    }

    [TestMethod]
    public void WhenResizeHasZeroDimensionThenRenderingIsDisabledAndPendingSizeIsPreserved()
    {
        byte[] target = CreatePixels(6, 2);
        byte[] surface = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface softwareSurface = new(
            width: 4,
            height: 3,
            MilPixelFormat.Pbgra32Bpp,
            ensureSurface: () =>
            {
                calls.Add("Ensure");
                return Direct3D9Factory.SuccessHResult;
            },
            lockSurface: (Direct3D9SurfaceRect bounds, out Direct3D9LockedPixelBuffer lockedBuffer) =>
            {
                calls.Add($"Lock:{bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}");
                lockedBuffer = new Direct3D9LockedPixelBuffer(surface, bounds.Top * 24 + bounds.Left * 4, 24);
                return Direct3D9Factory.SuccessHResult;
            },
            unlockSurface: () => Direct3D9Factory.SuccessHResult,
            begin3DInternal: (_, _) => Direct3D9Factory.SuccessHResult,
            end3D: () => Direct3D9Factory.SuccessHResult,
            blendWithSoftwareTarget: (_, _) => Direct3D9Factory.SuccessHResult);
        softwareSurface.Resize(6, 2);
        softwareSurface.Resize(0, 5);

        int beginResult = softwareSurface.BeginSw3D(
            target,
            24,
            default,
            useZBuffer: false,
            z: null);
        int endResult = softwareSurface.EndSw3D(target, 24);

        Assert.AreEqual(
            (0, 0, false, new Direct3D9SurfaceRect(0, 0, 6, 2), "Ensure"),
            (beginResult, endResult, softwareSurface.IsRenderingEnabled, softwareSurface.Bounds, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenNonZeroResizeFollowsDisabledResizeThenRenderingIsReenabled()
    {
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            CreatePixels(4, 3),
            []);
        softwareSurface.Resize(0, 3);

        softwareSurface.Resize(4, 3);

        Assert.IsTrue(softwareSurface.IsRenderingEnabled);
    }

    [TestMethod]
    public unsafe void WhenCleaningFreedResourcesThenDeviceIsExitedAfterImmediateCleanup()
    {
        using Direct3D9Device device = CreateDevice();
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            CreatePixels(4, 3),
            [],
            device: device);

        softwareSurface.CleanupFreedResources();

        Assert.AreEqual(
            (0u, 1u, 0u, 1u, false),
            (device.ResourceManager.CompletedFrameCount,
                device.ResourceManager.ReleasedResourcesFromLastFrameDestroyCount,
                device.ResourceManager.DelayedResourceDestroyCount,
                device.ResourceManager.ImmediateResourceDestroyCount,
                device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenCleaningFreedResourcesInsideExistingEntryThenOuterEntryIsRestored()
    {
        using Direct3D9Device device = CreateDevice();
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            CreatePixels(4, 3),
            [],
            device: device);
        device.Enter();

        softwareSurface.CleanupFreedResources();

        Assert.IsTrue(device.IsEntered() && device.IsProtected(true));
        device.Leave();
    }

    [TestMethod]
    public unsafe void WhenCleaningFreedResourcesThenResourceReleaseObservesDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice();
        bool wasEnteredDuringRelease = false;
        TestResource resource = new(device.ResourceManager, () => wasEnteredDuringRelease = device.IsEntered());
        device.ResourceManager.UnusedNotification(resource);
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            CreatePixels(4, 3),
            [],
            device: device);

        softwareSurface.CleanupFreedResources();

        Assert.IsTrue(wasEnteredDuringRelease && !device.IsEntered());
    }

    [TestMethod]
    public void WhenCleaningFreedResourcesWithoutDeviceThenOperationIsSkipped()
    {
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            CreatePixels(4, 3),
            []);

        softwareSurface.CleanupFreedResources();
    }

    [TestMethod]
    public unsafe void WhenCleaningFreedResourcesAfterDeviceDisposalThenOperationIsRejected()
    {
        Direct3D9Device device = CreateDevice();
        Direct3D9Software3DSurface softwareSurface = CreateSurface(
            MilPixelFormat.Pbgra32Bpp,
            CreatePixels(4, 3),
            [],
            device: device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(softwareSurface.CleanupFreedResources);
    }

    [TestMethod]
    [DataRow((int) MilPixelFormat.Bgr32Bpp, (int) MilPixelFormat.Bgr32Bpp, (int) Format.X8R8G8B8)]
    [DataRow((int) MilPixelFormat.Pbgra32Bpp, (int) MilPixelFormat.Pbgra32Bpp, (int) Format.A8R8G8B8)]
    [DataRow((int) MilPixelFormat.Bgra32Bpp, (int) MilPixelFormat.Pbgra32Bpp, (int) Format.A8R8G8B8)]
    public void WhenCreatingSoftwareSurfaceThenCopyFormatsArePreservedAndOtherFormatsUsePbgra(
        int requestedFormatValue,
        int expectedSurfaceFormatValue,
        int expectedDirect3DFormatValue)
    {
        MilPixelFormat requestedFormat = (MilPixelFormat) requestedFormatValue;
        MilPixelFormat expectedSurfaceFormat = (MilPixelFormat) expectedSurfaceFormatValue;
        Format expectedDirect3DFormat = (Format) expectedDirect3DFormatValue;
        Format checkedFormat = Format.Unknown;
        MilPixelFormat createdFormat = MilPixelFormat.Undefined;
        using Direct3D9DeviceManager manager = CreateSoftwareDeviceManager(
            (_, format) =>
            {
                checkedFormat = format;
                return Direct3D9Factory.SuccessHResult;
            });

        int result = Direct3D9Software3DSurface.TryCreate(
            manager,
            width: 4,
            height: 3,
            requestedFormat,
            (device, format) =>
            {
                createdFormat = format;
                return CreateSurface(format, CreatePixels(4, 3), [], device: device);
            },
            out Direct3D9Software3DSurface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, expectedDirect3DFormat, expectedSurfaceFormat, true),
            (result, checkedFormat, createdFormat, surface is not null));
    }

    [TestMethod]
    public void WhenRenderTargetFormatCheckFailsThenCreationIsSkippedAndOutputRemainsEmpty()
    {
        int createCount = 0;
        using Direct3D9DeviceManager manager = CreateSoftwareDeviceManager(
            (_, _) => Direct3D9Factory.GenericFailureHResult);

        int result = Direct3D9Software3DSurface.TryCreate(
            manager,
            width: 4,
            height: 3,
            MilPixelFormat.Pbgra32Bpp,
            (_, _) =>
            {
                createCount++;
                return CreateSurface(MilPixelFormat.Pbgra32Bpp, CreatePixels(4, 3), []);
            },
            out Direct3D9Software3DSurface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, null),
            (result, createCount, surface));
    }

    [TestMethod]
    public void WhenSoftwareSurfaceConstructionFailsThenHResultIsReturnedAndOutputRemainsEmpty()
    {
        using Direct3D9DeviceManager manager = CreateSoftwareDeviceManager(
            (_, _) => Direct3D9Factory.SuccessHResult);

        int result = Direct3D9Software3DSurface.TryCreate(
            manager,
            width: 4,
            height: 3,
            MilPixelFormat.Pbgra32Bpp,
            (_, _) => throw new SoftwareSurfaceCreationException(Direct3D9Factory.GenericFailureHResult),
            out Direct3D9Software3DSurface? surface);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, null), (result, surface));
    }

    private static Direct3D9Software3DSurface CreateSurface(
        MilPixelFormat pixelFormat,
        byte[] surface,
        List<string> calls,
        Func<float, bool, int>? begin3D = null,
        Func<int>? end3D = null,
        Direct3D9Device? device = null)
    {
        return new Direct3D9Software3DSurface(
            width: 4,
            height: 3,
            pixelFormat,
            ensureSurface: () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            lockSurface: (Direct3D9SurfaceRect bounds, out Direct3D9LockedPixelBuffer lockedBuffer) =>
            {
                calls.Add($"Lock:{bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}");
                lockedBuffer = new Direct3D9LockedPixelBuffer(surface, bounds.Top * 16 + bounds.Left * 4, 16);
                return 0;
            },
            unlockSurface: () =>
            {
                calls.Add("Unlock");
                return 0;
            },
            begin3DInternal: begin3D ?? ((_, _) => 0),
            end3D: end3D ?? (() =>
            {
                calls.Add("End");
                return 0;
            }),
            blendWithSoftwareTarget: (_, _) =>
            {
                calls.Add("Blend");
                return 0;
            },
            device);
    }

    private static unsafe Direct3D9Device CreateDevice()
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true));
    }

    private static unsafe Direct3D9DeviceManager CreateSoftwareDeviceManager(
        Func<Direct3D9Device, Format, int> checkRenderTargetFormat)
    {
        return new Direct3D9DeviceManager(
            (parameters, unusable, disposed) => new Direct3D9Device(
                null,
                null,
                parameters.AdapterOrdinal,
                parameters.DeviceType,
                parameters.BehaviorFlags,
                parameters.PresentParameters,
                unusable,
                disposed),
            checkRenderTargetFormat: checkRenderTargetFormat);
    }

    private static byte[] CreatePixels(int width, int height) =>
        Enumerable.Range(0, width * height * 4).Select(value => (byte) (value + 1)).ToArray();

    private static bool SurfaceRegionEquals(
        byte[] target,
        byte[] surface,
        Direct3D9SurfaceRect bounds,
        int stride)
    {
        for (int y = bounds.Top; y < bounds.Bottom; y++)
        {
            int offset = y * stride + bounds.Left * 4;
            int length = (bounds.Right - bounds.Left) * 4;
            if (!target.AsSpan(offset, length).SequenceEqual(surface.AsSpan(offset, length)))
            {
                return false;
            }
        }

        return true;
    }

    private static void FillRegion(
        byte[] pixels,
        Direct3D9SurfaceRect bounds,
        int stride,
        byte value)
    {
        for (int y = bounds.Top; y < bounds.Bottom; y++)
        {
            pixels.AsSpan(y * stride + bounds.Left * 4, (bounds.Right - bounds.Left) * 4).Fill(value);
        }
    }

    private static bool RegionHasValue(
        byte[] pixels,
        Direct3D9SurfaceRect bounds,
        int stride,
        byte value)
    {
        for (int y = bounds.Top; y < bounds.Bottom; y++)
        {
            foreach (byte pixel in pixels.AsSpan(y * stride + bounds.Left * 4, (bounds.Right - bounds.Left) * 4))
            {
                if (pixel != value)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private sealed class TestResource(Direct3D9ResourceManager manager, Action releaseAction)
        : Direct3D9Resource(manager)
    {
        protected override void ReleaseD3DResources() => releaseAction();
    }

    private sealed class SoftwareSurfaceCreationException : System.Runtime.InteropServices.COMException
    {
        internal SoftwareSurfaceCreationException(int hresult)
            : base(message: null, hresult)
        {
        }
    }
}
