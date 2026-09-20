using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed class Direct3D9SoftwareRenderTargetSurfaceTests
{
    [TestMethod]
    public void WhenRenderingThreeDThenTargetRemainsLockedUntilEndAndResourcesAreCleanedAfterEachOperation()
    {
        byte[] target = CreatePixels();
        byte[] surfacePixels = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(surfacePixels, calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            target,
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                calls.Add("Create");
                surface = software3DSurface;
                return 0;
            });

        int beginResult = renderTarget.Begin3D(
            new Direct3D9SurfaceRect(-1, 1, 3, 5),
            useZBuffer: true,
            z: 0.5f);
        int drawResult = renderTarget.DrawMesh3D(() =>
        {
            calls.Add("Draw");
            return 0;
        });
        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            (0, 0, 0, false, true, "TargetLock|Create|Ensure|SurfaceLock:0,1,3,3|SurfaceUnlock|Depth:0.5:True|Cleanup|Draw|Cleanup|EndSurface|SurfaceLock:0,1,3,3|SurfaceUnlock|TargetUnlock|Cleanup"),
            (beginResult, drawResult, endResult, renderTarget.In3D, renderTarget.HasSoftware3DSurface, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenThreeDDrawingIsDisabledThenInstructionIsConsumedAndResourcesAreCleanedOnce()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(new byte[target.Length], calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            target,
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                surface = software3DSurface;
                return 0;
            });
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: false, z: 1f);
        calls.Clear();

        int result = renderTarget.DrawMesh3D(() =>
        {
            calls.Add("UnexpectedDraw");
            return Direct3D9Factory.GenericFailureHResult;
        }, draw3DDisabled: true);

        Assert.AreEqual((0, true, "Cleanup"), (result, renderTarget.In3D, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenThreeDMeshDrawFailsThenFailureIsPreservedAndResourcesAreCleanedOnce()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(new byte[target.Length], calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            target,
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                surface = software3DSurface;
                return 0;
            });
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: false, z: 1f);
        calls.Clear();

        int result = renderTarget.DrawMesh3D(() =>
        {
            calls.Add("Draw");
            return Direct3D9Factory.GenericFailureHResult;
        });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, "Draw|Cleanup"),
            (result, renderTarget.In3D, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenThreeDMeshDrawFailsThenEndThreeDStillCompletesPairing()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(new byte[target.Length], calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            target,
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                surface = software3DSurface;
                return 0;
            });
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: false, z: 1f);
        _ = renderTarget.DrawMesh3D(() => Direct3D9Factory.GenericFailureHResult);
        calls.Clear();

        int result = renderTarget.End3D();

        Assert.AreEqual(
            (0, false, "EndSurface|TargetUnlock|Cleanup"),
            (result, renderTarget.In3D, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenThreeDMeshIsDrawnOutsideThreeDThenInvalidCallIsReturnedWithoutCallbacks()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            CreatePixels(),
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                calls.Add("UnexpectedCreate");
                surface = null;
                return 0;
            });

        int result = renderTarget.DrawMesh3D(() =>
        {
            calls.Add("UnexpectedDraw");
            return 0;
        });

        Assert.AreEqual(
            (Direct3D9Factory.WgxInvalidCallHResult, false, string.Empty),
            (result, renderTarget.In3D, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NotAvailableHResult)]
    [DataRow(Direct3D9Factory.NotFoundHResult)]
    public void WhenSoftwareRasterizerIsUnavailableThenThreeDInstructionsAreConsumed(int unavailableResult)
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            target,
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                calls.Add("CreateUnavailable");
                surface = null;
                return unavailableResult;
            });

        int beginResult = renderTarget.Begin3D(
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            useZBuffer: false,
            z: 1f);
        int drawResult = renderTarget.DrawMesh3D(() =>
        {
            calls.Add("UnexpectedDraw");
            return Direct3D9Factory.GenericFailureHResult;
        });
        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            (0, 0, 0, false, false, "TargetLock|CreateUnavailable|TargetUnlock|TargetUnlock"),
            (beginResult, drawResult, endResult, renderTarget.In3D, renderTarget.HasSoftware3DSurface, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSoftwareSurfaceCreationFailsThenFailureIsReturnedAfterUnlock()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            target,
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                calls.Add("CreateFailure");
                surface = null;
                return Direct3D9Factory.GenericFailureHResult;
            });

        int result = renderTarget.Begin3D(
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, "TargetLock|CreateFailure|TargetUnlock"),
            (result, renderTarget.In3D, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSoftwareSurfaceBeginFailsThenTargetIsUnlockedBeforeResourcesAreCleaned()
    {
        byte[] target = CreatePixels();
        byte[] surfacePixels = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(
            surfacePixels,
            calls,
            begin3D: (_, _) =>
            {
                calls.Add("DepthFailure");
                return Direct3D9Factory.GenericFailureHResult;
            });
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            target,
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                calls.Add("Create");
                surface = software3DSurface;
                return 0;
            });

        int result = renderTarget.Begin3D(
            new Direct3D9SurfaceRect(1, 0, 4, 2),
            useZBuffer: true,
            z: 0.25f);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, "TargetLock|Create|Ensure|SurfaceLock:1,0,4,2|SurfaceUnlock|DepthFailure|TargetUnlock|Cleanup"),
            (result, renderTarget.In3D, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenTargetIsResizedThenCachedSoftwareSurfaceUsesNewBounds()
    {
        byte[] target = new byte[6 * 3 * 4];
        byte[] surfacePixels = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = new(
            width: 4,
            height: 3,
            MilPixelFormat.Pbgra32Bpp,
            ensureSurface: () => 0,
            lockSurface: (Direct3D9SurfaceRect bounds, out Direct3D9LockedPixelBuffer lockedBuffer) =>
            {
                calls.Add($"SurfaceLock:{bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}");
                lockedBuffer = new Direct3D9LockedPixelBuffer(surfacePixels, bounds.Top * 24 + bounds.Left * 4, 24);
                return 0;
            },
            unlockSurface: () => 0,
            begin3DInternal: (_, _) => 0,
            end3D: () => 0,
            blendWithSoftwareTarget: (_, _) => 0);
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                pixels = target;
                stride = 24;
                return 0;
            },
            unlockTarget: () => { },
            createSoftware3DSurface: (out Direct3D9Software3DSurface? createdSurface) =>
            {
                createdSurface = software3DSurface;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { });
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 2), useZBuffer: false, z: 1f);
        _ = renderTarget.End3D();

        int resizeResult = renderTarget.Resize(6, 2);
        int beginResult = renderTarget.Begin3D(
            new Direct3D9SurfaceRect(0, 0, 8, 4),
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            (0, 0, "SurfaceLock:0,0,4,2|SurfaceLock:0,0,6,2"),
            (resizeResult, beginResult, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenFailedSurfaceCreationReturnsAnObjectThenItIsReleasedBeforeTargetUnlock()
    {
        byte[] target = CreatePixels();
        byte[] surfacePixels = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(surfacePixels, calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            target,
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                calls.Add("CreateFailure");
                surface = software3DSurface;
                return Direct3D9Factory.GenericFailureHResult;
            });

        int result = renderTarget.Begin3D(
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, false, "TargetLock|CreateFailure|Release|TargetUnlock"),
            (result, renderTarget.In3D, renderTarget.HasSoftware3DSurface, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSurfaceRebindingFailsThenCachedThreeDSurfaceIsReleasedAndNextBeginCreatesAnother()
    {
        byte[] target = CreatePixels();
        byte[] surfacePixels = new byte[target.Length];
        List<string> calls = [];
        int createCount = 0;
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            target,
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                calls.Add($"Create:{++createCount}");
                surface = CreateSoftware3DSurface(surfacePixels, calls);
                return 0;
            });
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: false, z: 1f);
        _ = renderTarget.End3D();

        int setSurfaceResult = renderTarget.SetSurface((out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            calls.Add("BindFailure");
            binding = new Direct3D9SoftwareRenderTargetBinding(
                6,
                2,
                MilPixelFormat.Prgba128BppFloat,
                120.0,
                144.0);
            return Direct3D9Factory.GenericFailureHResult;
        });
        int beginResult = renderTarget.Begin3D(
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 2, true, "TargetLock|Create:1|Ensure|SurfaceLock:0,0,4,3|SurfaceUnlock|Depth:1:False|Cleanup|EndSurface|TargetUnlock|Cleanup|BindFailure|Release|TargetLock|Create:2|Ensure|SurfaceLock:0,0,4,3|SurfaceUnlock|Depth:1:False|Cleanup"),
            (setSurfaceResult, beginResult, createCount, renderTarget.HasSoftware3DSurface, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSurfaceRebindingSucceedsThenOldBindingAndBuffersAreFreedBeforeBindAndCachedThreeDSurfaceIsReused()
    {
        byte[] target = CreatePixels();
        byte[] surfacePixels = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(surfacePixels, calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => { },
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                calls.Add("Create");
                surface = software3DSurface;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => calls.Add("UnexpectedRelease"),
            allocateIntermediateBuffers: state =>
            {
                calls.Add($"Allocate:{state.Width}");
                return 0;
            },
            initializeBaseRenderTarget: () =>
            {
                calls.Add("Init");
                return 0;
            },
            resizeSoftware3DSurface: (surface, width, height) =>
            {
                calls.Add($"Resize:{width}x{height}");
                surface.Resize(width, height);
                return 0;
            },
            cleanupSurfaceBinding: () => calls.Add("BindingCleanup"),
            freeIntermediateBuffers: () => calls.Add("FreeBuffers"));
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: false, z: 1f);
        _ = renderTarget.End3D();
        calls.Clear();

        int result = renderTarget.SetSurface((out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            calls.Add("Bind");
            binding = new Direct3D9SoftwareRenderTargetBinding(2, 2, MilPixelFormat.Bgr32Bpp, 120.0, 144.0);
            return 0;
        });

        Assert.AreEqual(
            (0, true, "BindingCleanup|FreeBuffers|Bind|Allocate:2|Init|Resize:2x2"),
            (result, renderTarget.HasSoftware3DSurface, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSurfaceRebindingSucceedsThenReturnedSizeIsCommittedToCachedThreeDSurface()
    {
        byte[] target = CreatePixels();
        byte[] surfacePixels = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(surfacePixels, calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            target,
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                calls.Add("Create");
                surface = software3DSurface;
                return 0;
            });
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: false, z: 1f);
        _ = renderTarget.End3D();

        int setSurfaceResult = renderTarget.SetSurface((out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            calls.Add("BindSuccess");
            binding = new Direct3D9SoftwareRenderTargetBinding(
                2,
                2,
                MilPixelFormat.Bgr32Bpp,
                120.0,
                144.0);
            return 0;
        });
        int beginResult = renderTarget.Begin3D(
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            (0, 0, "TargetLock|Create|Ensure|SurfaceLock:0,0,4,3|SurfaceUnlock|Depth:1:False|Cleanup|EndSurface|TargetUnlock|Cleanup|BindSuccess|TargetLock|Ensure|SurfaceLock:0,0,2,2|SurfaceUnlock|Depth:1:False|Cleanup"),
            (setSurfaceResult, beginResult, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSurfaceRebindingSucceedsThenPixelFormatColorDataAndDpiStateAreCommitted()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            target,
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.NotAvailableHResult;
            });

        int result = renderTarget.SetSurface((out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            binding = new Direct3D9SoftwareRenderTargetBinding(
                8,
                5,
                MilPixelFormat.Rgba128BppFloat,
                120.25,
                144.75);
            return 0;
        });

        Assert.AreEqual(
            (0, new Direct3D9SoftwareRenderTargetState(
                8,
                5,
                MilPixelFormat.Rgba128BppFloat,
                MilPixelFormat.Prgba128BppFloat,
                16,
                120.25f,
                144.75f)),
            (result, renderTarget.State));
    }

    [TestMethod]
    [DataRow((int) MilPixelFormat.Indexed8Bpp, Direct3D9Factory.InvalidArgumentHResult)]
    [DataRow((int) MilPixelFormat.Cmyk32Bpp, Direct3D9Factory.WinCodecInternalErrorHResult)]
    public void WhenSurfaceFormatIsInvalidThenOldStateIsPreservedAndCachedThreeDSurfaceIsReleased(
        int pixelFormatValue,
        int expectedResult)
    {
        MilPixelFormat pixelFormat = (MilPixelFormat) pixelFormatValue;
        byte[] target = CreatePixels();
        byte[] surfacePixels = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(surfacePixels, calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            target,
            calls,
            (out Direct3D9Software3DSurface? surface) =>
            {
                surface = software3DSurface;
                return 0;
            });
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: false, z: 1f);
        _ = renderTarget.End3D();
        Direct3D9SoftwareRenderTargetState oldState = renderTarget.State;

        int result = renderTarget.SetSurface((out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            binding = new Direct3D9SoftwareRenderTargetBinding(7, 6, pixelFormat, 120.0, 144.0);
            return 0;
        });

        Assert.AreEqual(
            (expectedResult, oldState, false, "TargetLock|Ensure|SurfaceLock:0,0,4,3|SurfaceUnlock|Depth:1:False|Cleanup|EndSurface|TargetUnlock|Cleanup|Release"),
            (result, renderTarget.State, renderTarget.HasSoftware3DSurface, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(15, 48)]
    [DataRow(16, 47)]
    public void WhenLockedTargetLayoutIsInvalidThenItIsUnlockedBeforeThreeDSurfaceCreation(
        int stride,
        int bufferLength)
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int targetStride) =>
            {
                calls.Add("TargetLock");
                pixels = new byte[bufferLength];
                targetStride = stride;
                return 0;
            },
            unlockTarget: () => calls.Add("TargetUnlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                calls.Add("UnexpectedCreate");
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => calls.Add("UnexpectedCleanup"),
            releaseSoftware3DSurface: _ => calls.Add("UnexpectedRelease"));

        int result = renderTarget.Begin3D(
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, false, false, "TargetLock|TargetUnlock"),
            (result, renderTarget.In3D, renderTarget.HasSoftware3DSurface, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenIntermediateBufferAllocationFailsThenFirstFailureCleansBindingAndPreservesOldState()
    {
        byte[] target = CreatePixels();
        byte[] surfacePixels = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(surfacePixels, calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => { },
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = software3DSurface;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => calls.Add("Release"),
            allocateIntermediateBuffers: state =>
            {
                calls.Add($"Allocate:{state.Width}");
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            initializeBaseRenderTarget: () =>
            {
                calls.Add("UnexpectedInit");
                return 0;
            },
            cleanupSurfaceBinding: () => calls.Add("BindingCleanup"));
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: false, z: 1f);
        _ = renderTarget.End3D();
        calls.Clear();
        Direct3D9SoftwareRenderTargetState oldState = renderTarget.State;

        int result = renderTarget.SetSurface((out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            calls.Add("Bind");
            binding = new Direct3D9SoftwareRenderTargetBinding(7, 6, MilPixelFormat.Bgr32Bpp, 120.0, 144.0);
            return 0;
        });

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, oldState, false, "BindingCleanup|Bind|Allocate:7|BindingCleanup|Release"),
            (result, renderTarget.State, renderTarget.HasSoftware3DSurface, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenBaseRenderTargetInitializationFailsThenFirstFailureCleansBindingAndPreservesOldState()
    {
        byte[] target = CreatePixels();
        byte[] surfacePixels = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(surfacePixels, calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => { },
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = software3DSurface;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => calls.Add("Release"),
            allocateIntermediateBuffers: state =>
            {
                calls.Add($"Allocate:{state.Width}");
                return 0;
            },
            initializeBaseRenderTarget: () =>
            {
                calls.Add("Init");
                return Direct3D9Factory.GenericFailureHResult;
            },
            cleanupSurfaceBinding: () => calls.Add("BindingCleanup"));
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: false, z: 1f);
        _ = renderTarget.End3D();
        calls.Clear();
        Direct3D9SoftwareRenderTargetState oldState = renderTarget.State;

        int result = renderTarget.SetSurface((out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            calls.Add("Bind");
            binding = new Direct3D9SoftwareRenderTargetBinding(7, 6, MilPixelFormat.Bgr32Bpp, 120.0, 144.0);
            return 0;
        });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, oldState, false, "BindingCleanup|Bind|Allocate:7|Init|BindingCleanup|Release"),
            (result, renderTarget.State, renderTarget.HasSoftware3DSurface, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenCachedThreeDSurfaceResizeFailsThenFirstFailureCleansBindingAndPreservesOldState()
    {
        byte[] target = CreatePixels();
        byte[] surfacePixels = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(surfacePixels, calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => { },
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = software3DSurface;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => calls.Add("Release"),
            allocateIntermediateBuffers: state =>
            {
                calls.Add($"Allocate:{state.Width}");
                return 0;
            },
            initializeBaseRenderTarget: () =>
            {
                calls.Add("Init");
                return 0;
            },
            resizeSoftware3DSurface: (_, width, height) =>
            {
                calls.Add($"Resize:{width}x{height}");
                return Direct3D9Factory.GenericFailureHResult;
            },
            cleanupSurfaceBinding: () => calls.Add("BindingCleanup"));
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: false, z: 1f);
        _ = renderTarget.End3D();
        calls.Clear();
        Direct3D9SoftwareRenderTargetState oldState = renderTarget.State;

        int result = renderTarget.SetSurface((out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            calls.Add("Bind");
            binding = new Direct3D9SoftwareRenderTargetBinding(7, 6, MilPixelFormat.Bgr32Bpp, 120.0, 144.0);
            return 0;
        });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, oldState, false, "BindingCleanup|Bind|Allocate:7|Init|Resize:7x6|BindingCleanup|Release"),
            (result, renderTarget.State, renderTarget.HasSoftware3DSurface, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenClearingPremultipliedTargetThenClipIsFilledBeforeUnlockWithoutDirtyNotification()
    {
        byte[] target = Enumerable.Repeat((byte) 0x11, 48).ToArray();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { });

        int result = renderTarget.Clear(
            new MilColorF(0.5f, 1f, 0f, 0f),
            new Direct3D9SurfaceRect(1, 1, 5, 4));

        Assert.AreEqual(
            (0, "Lock|Unlock", "11111111-00008080-00008080-00008080", "11111111-00008080-00008080-00008080"),
            (result, string.Join('|', calls), FormatRow(target, 16), FormatRow(target, 32)));
    }

    [DataTestMethod]
    [DataRow((int) MilPixelFormat.Bgra32Bpp)]
    [DataRow((int) MilPixelFormat.Bgr32Bpp)]
    public void WhenClearingNonPremultiplied32BppTargetThenArgbBytesIncludingAlphaAreWritten(int pixelFormatValue)
    {
        MilPixelFormat pixelFormat = (MilPixelFormat) pixelFormatValue;
        byte[] target = Enumerable.Repeat((byte) 0x11, 28).ToArray();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 2,
            height: 2,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 12;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            pixelFormat: pixelFormat);

        int result = renderTarget.Clear(new MilColorF(0.5f, 1f, 0f, 0f));

        Assert.AreEqual(
            (0, "Lock|Unlock", "0000FF80-0000FF80-11111111", "0000FF80-0000FF80-11111111"),
            (result, string.Join('|', calls), FormatPixels(target, 0, 12), FormatPixels(target, 12, 12)));
    }

    [TestMethod]
    public void WhenClearClipExtendsOutsideEverySurfaceEdgeThenOnlyIntersectionIsWritten()
    {
        byte[] target = Enumerable.Repeat((byte) 0x11, 48).ToArray();
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => { },
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { });

        int result = renderTarget.Clear(
            new MilColorF(1f, 0f, 1f, 0f),
            new Direct3D9SurfaceRect(-3, -2, 2, 2));

        Assert.AreEqual(
            (0, "00FF00FF-00FF00FF-11111111-11111111", "00FF00FF-00FF00FF-11111111-11111111", "11111111-11111111-11111111-11111111"),
            (result, FormatPixels(target, 0, 16), FormatPixels(target, 16, 16), FormatPixels(target, 32, 16)));
    }

    [TestMethod]
    public void WhenClearClipIsEmptyThenTargetIsLockedAndUnlockedWithoutWritingOrRasterizing()
    {
        byte[] target = Enumerable.Repeat((byte) 0x11, 36).ToArray();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 12;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            pixelFormat: MilPixelFormat.Bgr24Bpp,
            clearSoftwareRenderTarget: (_, _, _, _, _) =>
            {
                calls.Add("Rasterizer");
                return 0;
            });

        int result = renderTarget.Clear(
            new MilColorF(1f, 1f, 1f, 1f),
            new Direct3D9SurfaceRect(-4, 0, 0, 3));

        Assert.AreEqual(
            (0, "Lock|Unlock", new string('1', 72)),
            (result, string.Join('|', calls), Convert.ToHexString(target)));
    }

    [TestMethod]
    public void WhenClearColorIsNullThenUnlockIsAttemptedWithoutLocking()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = [];
                stride = 0;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { });

        int result = renderTarget.Clear(null);

        Assert.AreEqual((0, "Unlock"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenClearingComplexPixelFormatThenRasterizerFailureIsReturnedBeforeUnlock()
    {
        byte[] target = new byte[36];
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 12;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            pixelFormat: MilPixelFormat.Bgr24Bpp,
            clearSoftwareRenderTarget: (_, _, _, _, clip) =>
            {
                calls.Add($"Rasterizer:{clip.Left},{clip.Top},{clip.Right},{clip.Bottom}");
                return Direct3D9Factory.GenericFailureHResult;
            });

        int result = renderTarget.Clear(new MilColorF(1f, 0f, 1f, 0f));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Lock|Rasterizer:0,0,4,3|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenClearLockedTargetValidationFailsThenRasterizerIsSkippedAndUnlockStillRuns()
    {
        byte[] target = new byte[47];
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            clearSoftwareRenderTarget: (_, _, _, _, _) =>
            {
                calls.Add("Rasterizer");
                return 0;
            });

        int result = renderTarget.Clear(new MilColorF(1f, 1f, 1f, 1f));

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, "Lock|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDrawingBitmapThenClipIsIntersectedAndRasterizerRunsBeforeUnlock()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            drawBitmapSoftwareRenderTarget: (pixels, stride, state, clip) =>
            {
                calls.Add($"Rasterizer:{ReferenceEquals(target, pixels)}:{stride}:{state.PixelFormat}:{clip.Left},{clip.Top},{clip.Right},{clip.Bottom}");
                return 0;
            });

        int result = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(-2, 1, 3, 5));

        Assert.AreEqual(
            (0, new Direct3D9SurfaceRect(0, 1, 3, 3), "Lock|Rasterizer:True:16:Pbgra32Bpp:0,1,3,3|Unlock"),
            (result, renderTarget.GetCurrentClip(), string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenBitmapClipBecomesEmptyThenOldCurrentClipIsClearedWithoutLockingAgain()
    {
        byte[] target = CreatePixels();
        int lockCount = 0;
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                lockCount++;
                pixels = target;
                stride = 16;
                return Direct3D9Factory.SuccessHResult;
            },
            unlockTarget: () => { },
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.SuccessHResult;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            drawBitmapSoftwareRenderTarget: (_, _, _, _) => Direct3D9Factory.SuccessHResult);

        int firstResult = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(1, 1, 3, 3));
        int secondResult = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(5, 1, 7, 3));

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, Direct3D9Factory.SuccessHResult, default(Direct3D9SurfaceRect), 1),
            (firstResult, secondResult, renderTarget.GetCurrentClip(), lockCount));
    }

    [TestMethod]
    public void WhenSurfaceIsReboundThenCurrentClipUsesNewSurfaceBounds()
    {
        byte[] target = new byte[8 * 6 * 4];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                pixels = target;
                stride = 8 * 4;
                return Direct3D9Factory.SuccessHResult;
            },
            unlockTarget: () => { },
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.SuccessHResult;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            drawBitmapSoftwareRenderTarget: (_, _, _, _) => Direct3D9Factory.SuccessHResult);
        _ = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));

        int bindResult = renderTarget.SetSurface((out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            binding = new Direct3D9SoftwareRenderTargetBinding(2, 2, MilPixelFormat.Pbgra32Bpp, 96, 96);
            return Direct3D9Factory.SuccessHResult;
        });
        int drawResult = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, Direct3D9Factory.SuccessHResult, new Direct3D9SurfaceRect(0, 0, 2, 2)),
            (bindResult, drawResult, renderTarget.GetCurrentClip()));
    }

    [TestMethod]
    public void WhenCurrentClipIsQueriedAfterDisposeThenObjectDisposedExceptionIsThrown()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            CreatePixels(),
            [],
            (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.SuccessHResult;
            });
        renderTarget.Dispose();

        Action queryCurrentClip = () => renderTarget.GetCurrentClip();

        Assert.ThrowsExactly<ObjectDisposedException>(queryCurrentClip);
    }

    [TestMethod]
    public void WhenBitmapClipIsEmptyThenInstructionIsConsumedWithoutLocking()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("UnexpectedLock");
                pixels = [];
                stride = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            drawBitmapSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("UnexpectedRasterizer");
                return Direct3D9Factory.GenericFailureHResult;
            });

        int result = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(5, 1, 7, 3));

        Assert.AreEqual((0, "Unlock"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenBitmapRasterizerFailsThenFirstFailureIsReturnedBeforeUnlock()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            drawBitmapSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("Rasterizer");
                return Direct3D9Factory.OutOfMemoryHResult;
            });

        int result = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, "Lock|Rasterizer|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenBitmapRasterizerInitializationFailsThenCachedRasterizerCanBeRetried()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        int rasterizerCallCount = 0;
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return Direct3D9Factory.SuccessHResult;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.SuccessHResult;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            drawBitmapSoftwareRenderTarget: (_, _, _, _) =>
            {
                rasterizerCallCount++;
                calls.Add($"Rasterizer:{rasterizerCallCount}");
                return rasterizerCallCount == 1
                    ? Direct3D9Factory.GenericFailureHResult
                    : Direct3D9Factory.SuccessHResult;
            });

        int firstResult = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));
        int secondResult = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.SuccessHResult, "Lock|Rasterizer:1|Unlock|Lock|Rasterizer:2|Unlock"),
            (firstResult, secondResult, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenBitmapRasterizerMatrixInitializationCannotRenderThenCachedRasterizerCanBeRetried()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        int rasterizerCallCount = 0;
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return Direct3D9Factory.SuccessHResult;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.SuccessHResult;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            drawBitmapSoftwareRenderTarget: (_, _, _, _) =>
            {
                rasterizerCallCount++;
                calls.Add($"Rasterizer:{rasterizerCallCount}");
                return rasterizerCallCount == 1
                    ? Direct3D9Factory.NonInvertibleMatrixHResult
                    : Direct3D9Factory.SuccessHResult;
            });

        int firstResult = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));
        int secondResult = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, Direct3D9Factory.SuccessHResult, "Lock|Rasterizer:1|Unlock|Lock|Rasterizer:2|Unlock"),
            (firstResult, secondResult, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public void WhenBitmapLockReturnsNoRenderResultThenFailureIsPreserved(int lockResult)
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = [];
                stride = 0;
                return lockResult;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.SuccessHResult;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            drawBitmapSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("UnexpectedRasterizer");
                return Direct3D9Factory.SuccessHResult;
            });

        int result = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual((lockResult, "Lock|Unlock"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public void WhenBitmapCannotRenderThenInstructionIsConsumedBeforeUnlock(int noRenderResult)
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return Direct3D9Factory.SuccessHResult;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.SuccessHResult;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            drawBitmapSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("Rasterizer");
                return noRenderResult;
            });

        int result = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, "Lock|Rasterizer|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenClearLockFailsThenFirstFailureIsReturnedAndUnlockIsStillAttempted()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = [];
                stride = 0;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { });

        int result = renderTarget.Clear(new MilColorF(1f, 1f, 1f, 1f));

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, "Lock|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDrawingPathWithFillAndStrokeThenStagesShareOneLockAndUseIntersectedClip()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            fillPathSoftwareRenderTarget: (pixels, stride, state, clip) =>
            {
                calls.Add($"Fill:{ReferenceEquals(target, pixels)}:{stride}:{state.PixelFormat}:{clip.Left},{clip.Top},{clip.Right},{clip.Bottom}");
                return 0;
            },
            widenPath: () =>
            {
                calls.Add("Widen");
                return 0;
            },
            strokePathSoftwareRenderTarget: (_, _, _, clip) =>
            {
                calls.Add($"Stroke:{clip.Left},{clip.Top},{clip.Right},{clip.Bottom}");
                return 0;
            });

        int result = renderTarget.DrawPath(new Direct3D9SurfaceRect(-2, 1, 3, 5), hasFillBrush: true, hasPen: true, hasStrokeBrush: true);

        Assert.AreEqual(
            (0, "Lock|Fill:True:16:Pbgra32Bpp:0,1,3,3|Widen|Stroke:0,1,3,3|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPathFillFailsThenStrokeStagesAreSkippedAndFirstFailureIsReturnedBeforeUnlock()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            fillPathSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("Fill");
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            widenPath: () =>
            {
                calls.Add("UnexpectedWiden");
                return 0;
            },
            strokePathSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("UnexpectedStroke");
                return 0;
            });

        int result = renderTarget.DrawPath(new Direct3D9SurfaceRect(0, 0, 4, 3), hasFillBrush: true, hasPen: true, hasStrokeBrush: true);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, "Lock|Fill|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public void WhenPathStageCannotRenderThenInstructionIsConsumedAfterUnlock(int noRenderResult)
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            widenPath: () =>
            {
                calls.Add("Widen");
                return noRenderResult;
            },
            strokePathSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("UnexpectedStroke");
                return 0;
            });

        int result = renderTarget.DrawPath(new Direct3D9SurfaceRect(0, 0, 4, 3), hasFillBrush: false, hasPen: true, hasStrokeBrush: true);

        Assert.AreEqual((0, "Lock|Widen|Unlock"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDrawingInfinitePathThenSharedPathPipelineOnlyFillsIntersectedClip()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            fillPathSoftwareRenderTarget: (pixels, stride, state, clip) =>
            {
                calls.Add($"Fill:{ReferenceEquals(target, pixels)}:{stride}:{state.PixelFormat}:{clip.Left},{clip.Top},{clip.Right},{clip.Bottom}");
                return 0;
            },
            widenPath: () =>
            {
                calls.Add("UnexpectedWiden");
                return 0;
            },
            strokePathSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("UnexpectedStroke");
                return 0;
            });

        int result = renderTarget.DrawInfinitePath(new Direct3D9SurfaceRect(-2, 1, 3, 5));

        Assert.AreEqual(
            (0, "Lock|Fill:True:16:Pbgra32Bpp:0,1,3,3|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public void WhenInfinitePathCannotRenderThenInstructionIsConsumedAfterUnlock(int noRenderResult)
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            fillPathSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("Fill");
                return noRenderResult;
            });

        int result = renderTarget.DrawInfinitePath(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual((0, "Lock|Fill|Unlock"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDrawingGlyphsThenBrushIsRealizedBeforeLockAndRasterizerReceivesClipOpacityAndClearTypeSupport()
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            ensureGlyphBrushRealization: () =>
            {
                calls.Add("Realize");
                return 0;
            },
            hasRealizedGlyphBrush: () =>
            {
                calls.Add("Brush");
                return true;
            },
            getGlyphBrushOpacity: () =>
            {
                calls.Add("Opacity");
                return 0.625f;
            },
            drawGlyphsSoftwareRenderTarget: (pixels, stride, state, clip, alphaScale, supportsClearType) =>
            {
                calls.Add($"Draw:{ReferenceEquals(target, pixels)}:{stride}:{state.PixelFormat}:{clip.Left},{clip.Top},{clip.Right},{clip.Bottom}:{alphaScale}:{supportsClearType}");
                return 0;
            },
            forceClearType: true);

        int result = renderTarget.DrawGlyphs(new Direct3D9SurfaceRect(-2, 1, 3, 5));

        Assert.AreEqual(
            (0, "Realize|Brush|Opacity|Lock|Draw:True:16:Pbgra32Bpp:0,1,3,3:0.625:True|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDrawingGlyphsToStraightAlphaTargetThenClearTypeIsSupported()
    {
        byte[] target = CreatePixels();
        bool? targetSupportsClearType = null;
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                pixels = target;
                stride = 16;
                return Direct3D9Factory.SuccessHResult;
            },
            unlockTarget: () => { },
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.SuccessHResult;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            pixelFormat: MilPixelFormat.Bgra32Bpp,
            ensureGlyphBrushRealization: () => Direct3D9Factory.SuccessHResult,
            hasRealizedGlyphBrush: () => true,
            drawGlyphsSoftwareRenderTarget: (_, _, _, _, _, supportsClearType) =>
            {
                targetSupportsClearType = supportsClearType;
                return Direct3D9Factory.SuccessHResult;
            });

        int result = renderTarget.DrawGlyphs(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual((Direct3D9Factory.SuccessHResult, true), (result, targetSupportsClearType));
    }

    [TestMethod]
    public void WhenGlyphBrushIsEmptyThenInstructionIsConsumedWithSafeUnlock()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("UnexpectedLock");
                pixels = CreatePixels();
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            ensureGlyphBrushRealization: () =>
            {
                calls.Add("Realize");
                return 0;
            },
            hasRealizedGlyphBrush: () =>
            {
                calls.Add("Brush");
                return false;
            },
            getGlyphBrushOpacity: () =>
            {
                calls.Add("Opacity");
                return 1f;
            });

        int result = renderTarget.DrawGlyphs(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual((0, "Realize|Brush|Opacity|Unlock"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public void WhenGlyphBrushRealizationCannotRenderThenInstructionIsConsumedBeforeBrushAccess(int noRenderResult)
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("UnexpectedLock");
                pixels = CreatePixels();
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            ensureGlyphBrushRealization: () =>
            {
                calls.Add("Realize");
                return noRenderResult;
            },
            hasRealizedGlyphBrush: () =>
            {
                calls.Add("UnexpectedBrush");
                return true;
            },
            getGlyphBrushOpacity: () =>
            {
                calls.Add("UnexpectedOpacity");
                return 1f;
            });

        int result = renderTarget.DrawGlyphs(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual((0, "Realize|Unlock"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenGlyphTargetLockFailsThenFailureIsPreservedAndTargetIsSafelyUnlocked()
    {
        const int expectedResult = unchecked((int) 0x80004005);
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = [];
                stride = 0;
                return expectedResult;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            ensureGlyphBrushRealization: () =>
            {
                calls.Add("Realize");
                return 0;
            },
            hasRealizedGlyphBrush: () =>
            {
                calls.Add("Brush");
                return true;
            },
            getGlyphBrushOpacity: () =>
            {
                calls.Add("Opacity");
                return 1f;
            },
            drawGlyphsSoftwareRenderTarget: (_, _, _, _, _, _) =>
            {
                calls.Add("UnexpectedDraw");
                return 0;
            });

        int result = renderTarget.DrawGlyphs(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual((expectedResult, "Realize|Brush|Opacity|Lock|Unlock"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public void WhenGlyphRasterizerCannotRenderThenInstructionIsConsumedAfterUnlock(int noRenderResult)
    {
        byte[] target = CreatePixels();
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            ensureGlyphBrushRealization: () => 0,
            hasRealizedGlyphBrush: () => true,
            drawGlyphsSoftwareRenderTarget: (_, _, _, _, _, _) =>
            {
                calls.Add("Draw");
                return noRenderResult;
            });

        int result = renderTarget.DrawGlyphs(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual((0, "Lock|Draw|Unlock"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPathLockSucceedsWithNullPixelsThenUnexpectedFailureSkipsAllPathStagesBeforeUnlock()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = null!;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            fillPathSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("UnexpectedFill");
                return 0;
            },
            widenPath: () =>
            {
                calls.Add("UnexpectedWiden");
                return 0;
            },
            strokePathSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("UnexpectedStroke");
                return 0;
            });

        int result = renderTarget.DrawPath(new Direct3D9SurfaceRect(0, 0, 4, 3), hasFillBrush: true, hasPen: true, hasStrokeBrush: true);

        Assert.AreEqual(
            (Direct3D9Factory.UnexpectedHResult, "Lock|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(15, 48)]
    [DataRow(16, 47)]
    public void WhenPathLockedTargetLayoutIsInvalidThenValidationFailureSkipsAllPathStagesBeforeUnlock(
        int stride,
        int bufferLength)
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int targetStride) =>
            {
                calls.Add("Lock");
                pixels = new byte[bufferLength];
                targetStride = stride;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            fillPathSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("UnexpectedFill");
                return 0;
            },
            widenPath: () =>
            {
                calls.Add("UnexpectedWiden");
                return 0;
            },
            strokePathSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("UnexpectedStroke");
                return 0;
            });

        int result = renderTarget.DrawPath(new Direct3D9SurfaceRect(0, 0, 4, 3), hasFillBrush: true, hasPen: true, hasStrokeBrush: true);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, "Lock|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenGlyphLockSucceedsWithNullPixelsThenUnexpectedFailureSkipsRasterizerBeforeUnlock()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = null!;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            ensureGlyphBrushRealization: () =>
            {
                calls.Add("Realize");
                return 0;
            },
            hasRealizedGlyphBrush: () =>
            {
                calls.Add("Brush");
                return true;
            },
            getGlyphBrushOpacity: () =>
            {
                calls.Add("Opacity");
                return 1f;
            },
            drawGlyphsSoftwareRenderTarget: (_, _, _, _, _, _) =>
            {
                calls.Add("UnexpectedDraw");
                return 0;
            });

        int result = renderTarget.DrawGlyphs(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual(
            (Direct3D9Factory.UnexpectedHResult, "Realize|Brush|Opacity|Lock|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(15, 48)]
    [DataRow(16, 47)]
    public void WhenGlyphLockedTargetLayoutIsInvalidThenValidationFailureSkipsRasterizerBeforeUnlock(
        int stride,
        int bufferLength)
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int targetStride) =>
            {
                calls.Add("Lock");
                pixels = new byte[bufferLength];
                targetStride = stride;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            ensureGlyphBrushRealization: () =>
            {
                calls.Add("Realize");
                return 0;
            },
            hasRealizedGlyphBrush: () =>
            {
                calls.Add("Brush");
                return true;
            },
            getGlyphBrushOpacity: () =>
            {
                calls.Add("Opacity");
                return 1f;
            },
            drawGlyphsSoftwareRenderTarget: (_, _, _, _, _, _) =>
            {
                calls.Add("UnexpectedDraw");
                return 0;
            });

        int result = renderTarget.DrawGlyphs(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, "Realize|Brush|Opacity|Lock|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenSoftwareVideoRendererReturnsFrameThenDrawEndReleaseAndStateRestoreMatchNativeOrder()
    {
        using FakeSoftwareVideoBitmapSource bitmapSource = new();
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        List<string> calls = [];

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9SoftwareVideoSurfaceRenderer(
                (out nint value) =>
                {
                    calls.Add("Begin");
                    value = bitmapSource.Pointer;
                    return 0;
                },
                () =>
                {
                    calls.Add($"End:{renderState.PrefilterEnabled}");
                    return Direct3D9Factory.GenericFailureHResult;
                }),
            0,
            value =>
            {
                calls.Add($"Draw:{value == bitmapSource.Pointer}:{renderState.PrefilterEnabled}");
                return Direct3D9Factory.InvalidCallHResult;
            });

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, "Begin|Draw:True:False|End:False", true, 1),
            (result, string.Join('|', calls), renderState.PrefilterEnabled, bitmapSource.ReleaseCount));
    }

    [TestMethod]
    public void WhenSoftwareVideoRendererReturnsNoFrameThenEndRenderRunsWithoutChangingPrefilterState()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        List<string> calls = [];

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9SoftwareVideoSurfaceRenderer(
                (out nint value) =>
                {
                    calls.Add("Begin");
                    value = 0;
                    return 0;
                },
                () =>
                {
                    calls.Add("End");
                    return Direct3D9Factory.GenericFailureHResult;
                }),
            0,
            _ =>
            {
                calls.Add("UnexpectedDraw");
                return Direct3D9Factory.GenericFailureHResult;
            });

        Assert.AreEqual((0, "Begin|End", true), (result, string.Join('|', calls), renderState.PrefilterEnabled));
    }

    [TestMethod]
    public void WhenSoftwareVideoBeginRenderFailsThenFailureIsPreservedWithoutEndRenderOrDraw()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        int callbackCalls = 0;

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9SoftwareVideoSurfaceRenderer(
                (out nint value) =>
                {
                    callbackCalls++;
                    value = 0;
                    return Direct3D9Factory.GenericFailureHResult;
                },
                () =>
                {
                    callbackCalls++;
                    return 0;
                }),
            0,
            _ =>
            {
                callbackCalls++;
                return 0;
            });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, true),
            (result, callbackCalls, renderState.PrefilterEnabled));
    }

    [TestMethod]
    public unsafe void WhenSoftwareVideoUsesDirectBitmapThenAddRefAndReleaseAreBalanced()
    {
        using FakeSoftwareVideoBitmapSource bitmapSource = new();
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };

        int result = renderTarget.DrawVideo(
            renderState,
            surfaceRenderer: null,
            bitmapSource.Pointer,
            value => value == bitmapSource.Pointer ? 0 : Direct3D9Factory.InvalidArgumentHResult);

        Assert.AreEqual((0, 1, 1, true),
            (result, bitmapSource.AddRefCount, bitmapSource.ReleaseCount, renderState.PrefilterEnabled));
    }

    [TestMethod]
    public void WhenSoftwareVideoDirectBitmapIsNullThenEmptyFrameSucceedsWithoutDrawingOrChangingPrefilterState()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        int drawCalls = 0;

        int result = renderTarget.DrawVideo(
            renderState,
            surfaceRenderer: null,
            bitmapSource: 0,
            _ =>
            {
                drawCalls++;
                return Direct3D9Factory.GenericFailureHResult;
            });

        Assert.AreEqual((0, 0, true), (result, drawCalls, renderState.PrefilterEnabled));
    }

    [TestMethod]
    public unsafe void WhenSoftwareVideoDirectBitmapDrawFailsThenFailureIsPreservedAndReferenceIsReleased()
    {
        using FakeSoftwareVideoBitmapSource bitmapSource = new();
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = false };

        int result = renderTarget.DrawVideo(
            renderState,
            surfaceRenderer: null,
            bitmapSource.Pointer,
            _ => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, 1, false),
            (result, bitmapSource.AddRefCount, bitmapSource.ReleaseCount, renderState.PrefilterEnabled));
    }

    [TestMethod]
    public unsafe void WhenSoftwareVideoRendererDrawFailsThenCleanupOrderMatchesNativeCode()
    {
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        List<string> calls = [];
        using FakeSoftwareVideoBitmapSource bitmapSource = new(
            onRelease: () => calls.Add($"Release:{renderState.PrefilterEnabled}"));
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9SoftwareVideoSurfaceRenderer(
                (out nint value) =>
                {
                    calls.Add("Begin");
                    value = bitmapSource.Pointer;
                    return 0;
                },
                () =>
                {
                    calls.Add($"End:{renderState.PrefilterEnabled}");
                    return Direct3D9Factory.InvalidCallHResult;
                }),
            bitmapSource: 0,
            _ =>
            {
                calls.Add($"Draw:{renderState.PrefilterEnabled}");
                return Direct3D9Factory.GenericFailureHResult;
            });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Begin|Draw:False|End:False|Release:False", true),
            (result, string.Join('|', calls), renderState.PrefilterEnabled));
    }

    [TestMethod]
    public void WhenSoftwareVideoIsDrawnDuringThreeDThenInvalidCallIsReturnedWithoutCallbacks()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: false, z: 1f);
        int callbackCalls = 0;

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9SoftwareVideoSurfaceRenderer(
                (out nint value) =>
                {
                    callbackCalls++;
                    value = 0;
                    return 0;
                },
                () =>
                {
                    callbackCalls++;
                    return 0;
                }),
            0,
            _ =>
            {
                callbackCalls++;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.WgxInvalidCallHResult, 0, true),
            (result, callbackCalls, renderState.PrefilterEnabled));
    }

    [TestMethod]
    public void WhenSoftwareVideoIsDrawnAfterDisposeThenObjectDisposedExceptionIsThrownBeforeCallbacks()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        int callbackCalls = 0;
        renderTarget.Dispose();

        Action drawVideo = () => renderTarget.DrawVideo(
            renderState,
            new Direct3D9SoftwareVideoSurfaceRenderer(
                (out nint value) =>
                {
                    callbackCalls++;
                    value = 0;
                    return 0;
                },
                () =>
                {
                    callbackCalls++;
                    return 0;
                }),
            0,
            _ =>
            {
                callbackCalls++;
                return 0;
            });

        Assert.ThrowsExactly<ObjectDisposedException>(drawVideo);
        Assert.AreEqual((0, true), (callbackCalls, renderState.PrefilterEnabled));
    }

    [TestMethod]
    public void WhenRenderTargetTypeIsRequestedThenSoftwareRasterTypeIsReturned()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);

        InternalRenderTargetType result = renderTarget.GetRenderTargetType();

        Assert.AreEqual(InternalRenderTargetType.SoftwareRaster, result);
    }

    [TestMethod]
    public void WhenRealizationCacheIndexIsRequestedThenSoftwareCacheIndexIsReturned()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);

        uint result = renderTarget.GetRealizationCacheIndex();

        Assert.AreEqual(Direct3D9ImmediateBrushRealizer.SoftwareRealizationCacheIndex, result);
    }

    [TestMethod]
    public void WhenRenderTargetTypeIsRequestedAfterDisposeThenObjectDisposedExceptionIsThrown()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);
        renderTarget.Dispose();

        Action getRenderTargetType = () => renderTarget.GetRenderTargetType();

        Assert.ThrowsExactly<ObjectDisposedException>(getRenderTargetType);
    }

    [TestMethod]
    public void WhenRealizationCacheIndexIsRequestedAfterDisposeThenObjectDisposedExceptionIsThrown()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);
        renderTarget.Dispose();

        Action getRealizationCacheIndex = () => renderTarget.GetRealizationCacheIndex();

        Assert.ThrowsExactly<ObjectDisposedException>(getRealizationCacheIndex);
    }

    [TestMethod]
    public void WhenQueuedPresentCountIsRequestedThenSoftwareTargetReturnsZero()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);

        int result = renderTarget.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual((0, 0u), (result, queuedPresentCount));
    }

    [TestMethod]
    public void WhenQueuedPresentCountIsRequestedAfterDisposeThenObjectDisposedExceptionIsThrown()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreatePixels(), [], CreateUnavailableSurface);
        renderTarget.Dispose();

        Action getNumQueuedPresents = () => renderTarget.GetNumQueuedPresents(out _);

        Assert.ThrowsExactly<ObjectDisposedException>(getNumQueuedPresents);
    }

    [TestMethod]
    public void WhenDisposedThenLockedTargetAndCachedThreeDSurfaceAreReleasedOnceAndFurtherUseThrows()
    {
        byte[] target = CreatePixels();
        byte[] surfacePixels = new byte[target.Length];
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(surfacePixels, calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("TargetLock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("TargetUnlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                calls.Add("Create");
                surface = software3DSurface;
                return 0;
            },
            cleanup3DResources: _ => calls.Add("Cleanup"),
            releaseSoftware3DSurface: _ => calls.Add("Release"),
            cleanupSurfaceBinding: () => calls.Add("BindingCleanup"),
            freeIntermediateBuffers: () => calls.Add("FreeBuffers"));
        _ = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: false, z: 1f);

        renderTarget.Dispose();
        renderTarget.Dispose();
        Action resize = () => renderTarget.Resize(4, 3);

        Assert.AreEqual(
            (false, false, "TargetLock|Create|Ensure|SurfaceLock:0,0,4,3|SurfaceUnlock|Depth:1:False|Cleanup|TargetUnlock|BindingCleanup|FreeBuffers|Release", typeof(ObjectDisposedException)),
            (renderTarget.In3D, renderTarget.HasSoftware3DSurface, string.Join('|', calls), Assert.ThrowsExactly<ObjectDisposedException>(resize).GetType()));
    }

    [TestMethod]
    public void WhenSoftwareIntermediateRenderTargetSizeExceedsFloatLimitThenCreationIsRejectedBeforeFactory()
    {
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateIntermediateRenderTarget();
        bool factoryCalled = false;

        int result = renderTarget.CreateRenderTargetBitmap(
            (1u << 24) + 1,
            5,
            new Direct3D9IntermediateRenderTargetUsage(Direct3D9IntermediateRenderTargetUsageFlags.None, MilBitmapWrapMode.Extend),
            Direct3D9RenderTargetInitializationFlags.SoftwareOnly,
            (Direct3D9SoftwareRenderTargetBitmapCreationRequest _, out nint internalSurface) =>
            {
                factoryCalled = true;
                internalSurface = 0;
                return 0;
            },
            out Direct3D9SoftwareRenderTargetBitmap? renderTargetBitmap);

        Assert.AreEqual(
            (Direct3D9Factory.UnsupportedTextureSizeHResult, false, null),
            (result, factoryCalled, renderTargetBitmap));
    }

    [TestMethod]
    public unsafe void WhenSoftwareIntermediateRenderTargetIsCreatedForBlendingThenRequestAndOwnershipMatchNativeChain()
    {
        using FakeSoftwareVideoBitmapSource internalSurface = new();
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateIntermediateRenderTarget(
            MilPixelFormat.Rgba128BppFloat,
            associatedDisplayIndex: 7);
        Direct3D9SoftwareRenderTargetBitmapCreationRequest capturedRequest = default;

        int result = renderTarget.CreateRenderTargetBitmap(
            11,
            13,
            new Direct3D9IntermediateRenderTargetUsage(Direct3D9IntermediateRenderTargetUsageFlags.ForBlending, MilBitmapWrapMode.Tile),
            Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha,
            (Direct3D9SoftwareRenderTargetBitmapCreationRequest request, out nint surface) =>
            {
                capturedRequest = request;
                surface = internalSurface.Pointer;
                return 0;
            },
            out Direct3D9SoftwareRenderTargetBitmap? renderTargetBitmap);

        Assert.IsNotNull(renderTargetBitmap);
        Assert.AreEqual(
            (0, 11u, 13u, MilPixelFormat.Prgba128BppFloat, 96f, 96f, 7u, Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha, 1, 1),
            (result, capturedRequest.Width, capturedRequest.Height, capturedRequest.PixelFormat,
             capturedRequest.DpiX, capturedRequest.DpiY, capturedRequest.AssociatedDisplayIndex,
             capturedRequest.InitializationFlags, internalSurface.AddRefCount, internalSurface.ReleaseCount));
        Assert.AreEqual(capturedRequest, renderTargetBitmap.CreationRequest);

        renderTargetBitmap.Dispose();
        Assert.AreEqual(2, internalSurface.ReleaseCount);
    }

    [TestMethod]
    public unsafe void WhenSoftwareIntermediateRenderTargetIsNotForBlendingThenTargetFormatIsPreserved()
    {
        using FakeSoftwareVideoBitmapSource internalSurface = new();
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateIntermediateRenderTarget(MilPixelFormat.Bgra32Bpp);
        Direct3D9SoftwareRenderTargetBitmapCreationRequest capturedRequest = default;

        int result = renderTarget.CreateRenderTargetBitmap(
            3,
            4,
            new Direct3D9IntermediateRenderTargetUsage(Direct3D9IntermediateRenderTargetUsageFlags.ForUseIn3D, MilBitmapWrapMode.Extend),
            Direct3D9RenderTargetInitializationFlags.None,
            (Direct3D9SoftwareRenderTargetBitmapCreationRequest request, out nint surface) =>
            {
                capturedRequest = request;
                surface = internalSurface.Pointer;
                return 0;
            },
            out Direct3D9SoftwareRenderTargetBitmap? renderTargetBitmap);

        using (renderTargetBitmap)
        {
            Assert.AreEqual((0, MilPixelFormat.Bgra32Bpp), (result, capturedRequest.PixelFormat));
        }
    }

    [TestMethod]
    public unsafe void WhenSoftwareIntermediateSurfaceFactoryFailsWithAReferenceThenOutputIsEmptyAndReferenceIsReleased()
    {
        using FakeSoftwareVideoBitmapSource internalSurface = new();
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateIntermediateRenderTarget();

        int result = renderTarget.CreateRenderTargetBitmap(
            3,
            4,
            default,
            Direct3D9RenderTargetInitializationFlags.None,
            (Direct3D9SoftwareRenderTargetBitmapCreationRequest _, out nint surface) =>
            {
                surface = internalSurface.Pointer;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            out Direct3D9SoftwareRenderTargetBitmap? renderTargetBitmap);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, null, 0, 1),
            (result, renderTargetBitmap, internalSurface.AddRefCount, internalSurface.ReleaseCount));
    }

    [TestMethod]
    public void WhenSoftwareIntermediateSurfaceFactorySucceedsWithoutAReferenceThenUnexpectedFailureIsReturned()
    {
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateIntermediateRenderTarget();

        int result = renderTarget.CreateRenderTargetBitmap(
            3,
            4,
            default,
            Direct3D9RenderTargetInitializationFlags.None,
            (Direct3D9SoftwareRenderTargetBitmapCreationRequest _, out nint surface) =>
            {
                surface = 0;
                return 0;
            },
            out Direct3D9SoftwareRenderTargetBitmap? renderTargetBitmap);

        Assert.AreEqual((Direct3D9Factory.UnexpectedHResult, null), (result, renderTargetBitmap));
    }

    [TestMethod]
    public unsafe void WhenSoftwareIntermediateWrapperFailsThenSurfaceIsReleasedAndOutputRemainsEmpty()
    {
        using FakeSoftwareVideoBitmapSource internalSurface = new();
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateIntermediateRenderTarget();

        int result = renderTarget.CreateRenderTargetBitmap(
            3,
            4,
            default,
            Direct3D9RenderTargetInitializationFlags.None,
            (Direct3D9SoftwareRenderTargetBitmapCreationRequest _, out nint surface) =>
            {
                surface = internalSurface.Pointer;
                return 0;
            },
            out Direct3D9SoftwareRenderTargetBitmap? renderTargetBitmap,
            (nint _, Direct3D9SoftwareRenderTargetBitmapCreationRequest _, out Direct3D9SoftwareRenderTargetBitmap? bitmap) =>
            {
                bitmap = null;
                return Direct3D9Factory.GenericFailureHResult;
            });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, null, 0, 1),
            (result, renderTargetBitmap, internalSurface.AddRefCount, internalSurface.ReleaseCount));
    }

    [TestMethod]
    public unsafe void WhenSoftwareRenderTargetBitmapIsCreatedThenItOwnsInternalSurfaceUntilDisposed()
    {
        using FakeSoftwareVideoBitmapSource internalSurface = new();
        Direct3D9SoftwareRenderTargetBitmap renderTargetBitmap = new(internalSurface.Pointer);

        renderTargetBitmap.Dispose();
        renderTargetBitmap.Dispose();

        Assert.AreEqual((1, 1), (internalSurface.AddRefCount, internalSurface.ReleaseCount));
    }

    [TestMethod]
    public unsafe void WhenSoftwareRenderTargetBitmapSourcesAreRequestedThenEachResultOwnsAReference()
    {
        using FakeSoftwareVideoBitmapSource internalSurface = new();
        using Direct3D9SoftwareRenderTargetBitmap renderTargetBitmap = new(internalSurface.Pointer);

        int bitmapSourceResult = renderTargetBitmap.GetBitmapSource(out nint bitmapSource);
        int cacheableSourceResult = renderTargetBitmap.GetCacheableBitmapSource(out nint cacheableBitmapSource);
        int bitmapResult = renderTargetBitmap.GetBitmap(out nint bitmap);

        Assert.AreEqual(
            (0, 0, 0, internalSurface.Pointer, internalSurface.Pointer, internalSurface.Pointer, 4),
            (bitmapSourceResult, cacheableSourceResult, bitmapResult, bitmapSource, cacheableBitmapSource, bitmap, internalSurface.AddRefCount));

        Direct3D9Factory.Release(bitmapSource);
        Direct3D9Factory.Release(cacheableBitmapSource);
        Direct3D9Factory.Release(bitmap);
    }

    [TestMethod]
    public unsafe void WhenSoftwareRenderTargetBitmapIsDisposedThenQueriesThrowWithoutAddingReferences()
    {
        using FakeSoftwareVideoBitmapSource internalSurface = new();
        Direct3D9SoftwareRenderTargetBitmap renderTargetBitmap = new(internalSurface.Pointer);
        renderTargetBitmap.Dispose();

        Action getBitmapSource = () => renderTargetBitmap.GetBitmapSource(out _);
        Action getCacheableBitmapSource = () => renderTargetBitmap.GetCacheableBitmapSource(out _);
        Action getBitmap = () => renderTargetBitmap.GetBitmap(out _);

        Assert.AreEqual(
            (typeof(ObjectDisposedException), typeof(ObjectDisposedException), typeof(ObjectDisposedException), 1),
            (Assert.ThrowsExactly<ObjectDisposedException>(getBitmapSource).GetType(),
             Assert.ThrowsExactly<ObjectDisposedException>(getCacheableBitmapSource).GetType(),
             Assert.ThrowsExactly<ObjectDisposedException>(getBitmap).GetType(),
             internalSurface.AddRefCount));
    }

    [TestMethod]
    public void WhenBitmapLockSucceedsWithNullPixelsThenUnexpectedFailureIsReturnedBeforeRasterizerAndUnlock()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("Lock");
                pixels = null!;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            drawBitmapSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("UnexpectedRasterizer");
                return 0;
            });

        int result = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual(
            (Direct3D9Factory.UnexpectedHResult, "Lock|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(15, 48)]
    [DataRow(16, 47)]
    public void WhenBitmapLockedTargetLayoutIsInvalidThenValidationFailureIsReturnedBeforeRasterizerAndUnlock(
        int stride,
        int bufferLength)
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = new(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int targetStride) =>
            {
                calls.Add("Lock");
                pixels = new byte[bufferLength];
                targetStride = stride;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return 0;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            drawBitmapSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("UnexpectedRasterizer");
                return 0;
            });

        int result = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, "Lock|Unlock"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenStagedTargetBitmapSucceedsThenLockIsReleasedOnceAfterRasterizer()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateStagedLockRenderTarget(
            calls,
            getStride: () =>
            {
                calls.Add("Stride");
                return (0, 16);
            },
            getDataPointer: () =>
            {
                calls.Add("Data");
                return (0, CreatePixels());
            },
            drawBitmapSoftwareRenderTarget: (_, _, _, clip) =>
            {
                calls.Add($"Rasterizer:{clip.Left},{clip.Top},{clip.Right},{clip.Bottom}");
                return 0;
            });

        int result = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(-2, 1, 3, 5));

        Assert.AreEqual(
            (0, "Lock|Stride|Data|Rasterizer:0,1,3,3|Release"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public void WhenStagedTargetBitmapCannotRenderThenInstructionIsConsumedAndLockIsReleasedOnce(int noRenderResult)
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateStagedLockRenderTarget(
            calls,
            getStride: () =>
            {
                calls.Add("Stride");
                return (0, 16);
            },
            getDataPointer: () =>
            {
                calls.Add("Data");
                return (0, CreatePixels());
            },
            drawBitmapSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("Rasterizer");
                return noRenderResult;
            });

        int result = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual((0, "Lock|Stride|Data|Rasterizer|Release"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenStagedTargetClearSucceedsThenLockIsReleasedOnce()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateStagedLockRenderTarget(
            calls,
            getStride: () =>
            {
                calls.Add("Stride");
                return (0, 16);
            },
            getDataPointer: () =>
            {
                calls.Add("Data");
                return (0, CreatePixels());
            });

        int result = renderTarget.Clear(new MilColorF(1f, 0f, 0f, 0f));

        Assert.AreEqual((0, "Lock|Stride|Data|Release"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenStagedTargetPathSucceedsThenLockIsReleasedOnceAfterRasterizer()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateStagedLockRenderTarget(
            calls,
            getStride: () =>
            {
                calls.Add("Stride");
                return (0, 16);
            },
            getDataPointer: () =>
            {
                calls.Add("Data");
                return (0, CreatePixels());
            },
            fillPathSoftwareRenderTarget: (_, _, _, _) =>
            {
                calls.Add("Fill");
                return 0;
            });

        int result = renderTarget.DrawPath(
            new Direct3D9SurfaceRect(0, 0, 4, 3),
            hasFillBrush: true,
            hasPen: false,
            hasStrokeBrush: false);

        Assert.AreEqual((0, "Lock|Stride|Data|Fill|Release"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenStagedTargetGlyphsSucceedThenLockIsReleasedOnceAfterRasterizer()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateStagedLockRenderTarget(
            calls,
            getStride: () =>
            {
                calls.Add("Stride");
                return (0, 16);
            },
            getDataPointer: () =>
            {
                calls.Add("Data");
                return (0, CreatePixels());
            },
            drawGlyphsSoftwareRenderTarget: (_, _, _, _, _, _) =>
            {
                calls.Add("Glyphs");
                return 0;
            },
            ensureGlyphBrushRealization: () =>
            {
                calls.Add("Realize");
                return 0;
            },
            hasRealizedGlyphBrush: () =>
            {
                calls.Add("Brush");
                return true;
            },
            getGlyphBrushOpacity: () =>
            {
                calls.Add("Opacity");
                return 1f;
            });

        int result = renderTarget.DrawGlyphs(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual(
            (0, "Realize|Brush|Opacity|Lock|Stride|Data|Glyphs|Release"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenStagedTargetBeginsThreeDThenLockRemainsAcquiredUntilEndThreeD()
    {
        List<string> calls = [];
        Direct3D9Software3DSurface software3DSurface = CreateSoftware3DSurface(new byte[48], calls);
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateStagedLockRenderTarget(
            calls,
            getStride: () =>
            {
                calls.Add("Stride");
                return (0, 16);
            },
            getDataPointer: () =>
            {
                calls.Add("Data");
                return (0, CreatePixels());
            },
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                calls.Add("Create");
                surface = software3DSurface;
                return 0;
            });

        int beginResult = renderTarget.Begin3D(new Direct3D9SurfaceRect(0, 0, 4, 3), useZBuffer: true, z: 0.5f);
        bool in3DAfterBegin = renderTarget.In3D;
        string callsAfterBegin = string.Join('|', calls);
        int drawResult = renderTarget.DrawMesh3D(() =>
        {
            calls.Add("Draw");
            return 0;
        });
        string callsAfterDraw = string.Join('|', calls);
        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            (0, 0, 0, true, false,
                "Lock|Stride|Data|Create|Ensure|SurfaceLock:0,0,4,3|SurfaceUnlock|Depth:0.5:True|Cleanup",
                "Lock|Stride|Data|Create|Ensure|SurfaceLock:0,0,4,3|SurfaceUnlock|Depth:0.5:True|Cleanup|Draw|Cleanup",
                "Lock|Stride|Data|Create|Ensure|SurfaceLock:0,0,4,3|SurfaceUnlock|Depth:0.5:True|Cleanup|Draw|Cleanup|EndSurface|SurfaceLock:0,0,4,3|SurfaceUnlock|Release|Cleanup"),
            (beginResult, drawResult, endResult, in3DAfterBegin, callsAfterBegin.Contains("Release", StringComparison.Ordinal),
                callsAfterBegin, callsAfterDraw, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenStagedTargetStrideQueryFailsThenLockIsReleasedImmediatelyAndOuterUnlockIsSafe()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateStagedLockRenderTarget(
            calls,
            getStride: () =>
            {
                calls.Add("Stride");
                return (Direct3D9Factory.OutOfMemoryHResult, 64);
            },
            getDataPointer: () =>
            {
                calls.Add("UnexpectedData");
                return (0, CreatePixels());
            });

        int result = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, "Lock|Stride|Release"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenStagedTargetDataQueryFailsThenFirstFailureIsPreservedAndLockStateIsClearedForRetry()
    {
        List<string> calls = [];
        int dataQueryCount = 0;
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateStagedLockRenderTarget(
            calls,
            getStride: () =>
            {
                calls.Add("Stride");
                return (0, 16);
            },
            getDataPointer: () =>
            {
                dataQueryCount++;
                calls.Add($"Data:{dataQueryCount}");
                return dataQueryCount == 1
                    ? (Direct3D9Factory.GenericFailureHResult, null)
                    : (0, CreatePixels());
            });

        int firstResult = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));
        int secondResult = renderTarget.DrawBitmap(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, "Lock|Stride|Data:1|Release|Lock|Stride|Data:2|Rasterizer|Release"),
            (firstResult, secondResult, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenSoftwareBitmapTintFormatIsUnsupportedThenDataIsNotAccessedAndLockIsReleased()
    {
        using FakeSoftwareVideoBitmapSource internalSurface = new();
        List<string> calls = [];
        using Direct3D9SoftwareRenderTargetBitmap renderTargetBitmap = new(
            internalSurface.Pointer,
            shouldTintBitmapSource: () => true,
            lockBitmap: (nint _, out nint bitmapLock) =>
            {
                calls.Add("Lock");
                bitmapLock = 42;
                return 0;
            },
            getPixelFormat: _ =>
            {
                calls.Add("Format");
                return (0, MilPixelFormat.Bgr24Bpp);
            },
            getDataPointer: _ =>
            {
                calls.Add("UnexpectedData");
                return (0, 0u, 0);
            },
            release: _ => calls.Add("Release"));

        int result = renderTargetBitmap.GetBitmapSource(out nint bitmapSource);

        Assert.AreEqual((0, internalSurface.Pointer, "Lock|Format|Release"), (result, bitmapSource, string.Join('|', calls)));
        Direct3D9Factory.Release(bitmapSource);
    }

    [TestMethod]
    public unsafe void WhenSoftwareBitmapTintLockReturnsNoReferenceThenTintIsSkippedAndBitmapIsReturned()
    {
        using FakeSoftwareVideoBitmapSource internalSurface = new();
        List<string> calls = [];
        using Direct3D9SoftwareRenderTargetBitmap renderTargetBitmap = new(
            internalSurface.Pointer,
            shouldTintBitmapSource: () => true,
            lockBitmap: (nint _, out nint bitmapLock) =>
            {
                calls.Add("Lock");
                bitmapLock = 0;
                return 0;
            },
            getPixelFormat: _ =>
            {
                calls.Add("UnexpectedFormat");
                return (0, MilPixelFormat.Pbgra32Bpp);
            },
            release: _ => calls.Add("UnexpectedRelease"));

        int result = renderTargetBitmap.GetBitmapSource(out nint bitmapSource);

        Assert.AreEqual((0, internalSurface.Pointer, "Lock"), (result, bitmapSource, string.Join('|', calls)));
        Direct3D9Factory.Release(bitmapSource);
    }

    [TestMethod]
    public unsafe void WhenSoftwareBitmapTintIsEnabledThenBitmapIsLockedTintedReleasedAndReturned()
    {
        using FakeSoftwareVideoBitmapSource internalSurface = new();
        List<string> calls = [];
        using Direct3D9SoftwareRenderTargetBitmap renderTargetBitmap = new(
            internalSurface.Pointer,
            shouldTintBitmapSource: () => true,
            lockBitmap: (nint bitmap, out nint bitmapLock) =>
            {
                calls.Add($"Lock:{bitmap}:3");
                bitmapLock = 42;
                return 0;
            },
            getPixelFormat: bitmapLock =>
            {
                calls.Add($"Format:{bitmapLock}");
                return (0, MilPixelFormat.Pbgra32Bpp);
            },
            getDataPointer: bitmapLock =>
            {
                calls.Add($"Data:{bitmapLock}");
                return (0, 128u, (nint) 84);
            },
            getSize: (nint bitmapLock, out uint width, out uint height) =>
            {
                calls.Add($"Size:{bitmapLock}");
                width = 7;
                height = 5;
                return 0;
            },
            getStride: bitmapLock =>
            {
                calls.Add($"Stride:{bitmapLock}");
                return (0, 28u);
            },
            tintBitmap: (data, width, height, stride) => calls.Add($"Tint:{data}:{width}:{height}:{stride}"),
            release: bitmapLock => calls.Add($"Release:{bitmapLock}"));

        int result = renderTargetBitmap.GetBitmapSource(out nint bitmapSource);

        Assert.AreEqual(
            (0, internalSurface.Pointer, $"Lock:{internalSurface.Pointer}:3|Format:42|Data:42|Size:42|Stride:42|Tint:84:7:5:28|Release:42"),
            (result, bitmapSource, string.Join('|', calls)));
        Direct3D9Factory.Release(bitmapSource);
    }

    [TestMethod]
    public unsafe void WhenSoftwareBitmapTintStepFailsThenFailureIsSilentAndLockIsReleased()
    {
        using FakeSoftwareVideoBitmapSource internalSurface = new();
        List<string> calls = [];
        using Direct3D9SoftwareRenderTargetBitmap renderTargetBitmap = new(
            internalSurface.Pointer,
            shouldTintBitmapSource: () => true,
            lockBitmap: (nint _, out nint bitmapLock) =>
            {
                calls.Add("Lock");
                bitmapLock = 42;
                return 0;
            },
            getPixelFormat: _ =>
            {
                calls.Add("Format");
                return (0, MilPixelFormat.Bgra32Bpp);
            },
            getDataPointer: _ =>
            {
                calls.Add("Data");
                return (Direct3D9Factory.GenericFailureHResult, 0u, 0);
            },
            getSize: (nint _, out uint width, out uint height) =>
            {
                calls.Add("UnexpectedSize");
                width = 0;
                height = 0;
                return 0;
            },
            getStride: _ =>
            {
                calls.Add("UnexpectedStride");
                return (0, 0u);
            },
            tintBitmap: (_, _, _, _) => calls.Add("UnexpectedTint"),
            release: _ => calls.Add("Release"));

        int result = renderTargetBitmap.GetBitmapSource(out nint bitmapSource);

        Assert.AreEqual((0, internalSurface.Pointer, "Lock|Format|Data|Release"), (result, bitmapSource, string.Join('|', calls)));
        Direct3D9Factory.Release(bitmapSource);
    }

    private sealed unsafe class FakeSoftwareVideoBitmapSource : IDisposable
    {
        private readonly Action? _onRelease;
        private nint _memory;

        internal FakeSoftwareVideoBitmapSource(Action? onRelease = null)
        {
            _onRelease = onRelease;
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 7);
            void** memory = (void**) _memory;
            void** vtable = memory + 1;
            memory[0] = vtable;
            memory[6] = (void*) GCHandle.ToIntPtr(GCHandle.Alloc(this));
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &AddRef;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &Release;
        }

        internal int AddRefCount => checked((int) ((nint*) _memory)[4]);

        internal int ReleaseCount => checked((int) ((nint*) _memory)[5]);

        internal nint Pointer => _memory;

        public void Dispose()
        {
            if (_memory == 0)
            {
                return;
            }

            GCHandle.FromIntPtr(((nint*) _memory)[6]).Free();
            NativeMemory.Free((void*) _memory);
            _memory = 0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static uint AddRef(void*** instance)
        {
            ((nint*) instance)[4]++;
            return 2;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static uint Release(void*** instance)
        {
            ((nint*) instance)[5]++;
            GCHandle handle = GCHandle.FromIntPtr(((nint*) instance)[6]);
            ((FakeSoftwareVideoBitmapSource) handle.Target!)._onRelease?.Invoke();
            return 1;
        }
    }

    private static string FormatRow(byte[] pixels, int offset) => FormatPixels(pixels, offset, 16);

    private static string FormatPixels(byte[] pixels, int offset, int byteCount)
    {
        return string.Join('-', Enumerable.Range(0, byteCount / 4).Select(index => Convert.ToHexString(pixels, offset + (index * 4), 4)));
    }

    private static int CreateUnavailableSurface(out Direct3D9Software3DSurface? surface)
    {
        surface = null;
        return Direct3D9Factory.NotAvailableHResult;
    }

    private static Direct3D9SoftwareRenderTargetSurface CreateStagedLockRenderTarget(
        List<string> calls,
        Func<(int Result, int Stride)> getStride,
        Func<(int Result, byte[]? Pixels)> getDataPointer,
        Direct3D9DrawBitmapSoftwareRenderTarget? drawBitmapSoftwareRenderTarget = null,
        Direct3D9DrawPathSoftwareRenderTarget? fillPathSoftwareRenderTarget = null,
        Direct3D9DrawGlyphsSoftwareRenderTarget? drawGlyphsSoftwareRenderTarget = null,
        Func<int>? ensureGlyphBrushRealization = null,
        Func<bool>? hasRealizedGlyphBrush = null,
        Func<float>? getGlyphBrushOpacity = null,
        Direct3D9CreateSoftware3DSurface? createSoftware3DSurface = null)
    {
        return new Direct3D9SoftwareRenderTargetSurface(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                pixels = null!;
                stride = 0;
                return Direct3D9Factory.UnexpectedHResult;
            },
            unlockTarget: () => calls.Add("UnexpectedLegacyUnlock"),
            createSoftware3DSurface: createSoftware3DSurface ?? CreateUnavailableSurface,
            cleanup3DResources: _ => calls.Add("Cleanup"),
            releaseSoftware3DSurface: _ => calls.Add("ReleaseSurface"),
            drawBitmapSoftwareRenderTarget: drawBitmapSoftwareRenderTarget ?? ((_, _, _, _) =>
            {
                calls.Add("Rasterizer");
                return 0;
            }),
            fillPathSoftwareRenderTarget: fillPathSoftwareRenderTarget,
            ensureGlyphBrushRealization: ensureGlyphBrushRealization,
            hasRealizedGlyphBrush: hasRealizedGlyphBrush,
            getGlyphBrushOpacity: getGlyphBrushOpacity,
            drawGlyphsSoftwareRenderTarget: drawGlyphsSoftwareRenderTarget,
            stagedLockTarget: new Direct3D9SoftwareRenderTargetLockCallbacks(
                Lock: () =>
                {
                    calls.Add("Lock");
                    return 0;
                },
                GetStride: getStride,
                GetDataPointer: getDataPointer,
                Release: () => calls.Add("Release")));
    }

    private static Direct3D9SoftwareRenderTargetSurface CreateIntermediateRenderTarget(
        MilPixelFormat pixelFormat = MilPixelFormat.Pbgra32Bpp,
        uint? associatedDisplayIndex = null)
    {
        return new Direct3D9SoftwareRenderTargetSurface(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                pixels = CreatePixels();
                stride = 16;
                return 0;
            },
            unlockTarget: () => { },
            createSoftware3DSurface: CreateUnavailableSurface,
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            pixelFormat: pixelFormat,
            associatedDisplayIndex: associatedDisplayIndex);
    }

    private static Direct3D9SoftwareRenderTargetSurface CreateRenderTarget(
        byte[] target,
        List<string> calls,
        Direct3D9CreateSoftware3DSurface createSurface)
    {
        return new Direct3D9SoftwareRenderTargetSurface(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                calls.Add("TargetLock");
                pixels = target;
                stride = 16;
                return 0;
            },
            unlockTarget: () => calls.Add("TargetUnlock"),
            createSurface,
            cleanup3DResources: _ => calls.Add("Cleanup"),
            releaseSoftware3DSurface: _ => calls.Add("Release"));
    }

    private static Direct3D9Software3DSurface CreateSoftware3DSurface(
        byte[] surfacePixels,
        List<string> calls,
        Func<float, bool, int>? begin3D = null)
    {
        return new Direct3D9Software3DSurface(
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
                calls.Add($"SurfaceLock:{bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}");
                lockedBuffer = new Direct3D9LockedPixelBuffer(
                    surfacePixels,
                    bounds.Top * 16 + bounds.Left * 4,
                    16);
                return 0;
            },
            unlockSurface: () =>
            {
                calls.Add("SurfaceUnlock");
                return 0;
            },
            begin3DInternal: begin3D ?? ((z, useZBuffer) =>
            {
                calls.Add($"Depth:{z}:{useZBuffer}");
                return 0;
            }),
            end3D: () =>
            {
                calls.Add("EndSurface");
                return 0;
            },
            blendWithSoftwareTarget: (_, _) => 0);
    }

    private static byte[] CreatePixels()
    {
        return Enumerable.Range(0, 4 * 3 * 4).Select(value => (byte) (value + 1)).ToArray();
    }
}
