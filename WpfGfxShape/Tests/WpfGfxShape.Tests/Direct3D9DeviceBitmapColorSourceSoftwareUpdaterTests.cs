using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceBitmapColorSourceSoftwareUpdaterTests
{
    [TestMethod]
    public void WhenUpdateSucceedsThenSystemMemoryTextureIsCachedAndUpdatedInNativeOrder()
    {
        List<string> calls = [];
        Direct3D9DeviceBitmapColorSourceSoftwareUpdater updater = CreateUpdater(calls);
        Direct3D9BitmapRealizationRectangle[] firstDirtyRectangles =
        [
            new(1, 2, 11, 12),
            new(20, 21, 30, 31)
        ];

        int firstResult = updater.Update(firstDirtyRectangles, 13);
        int secondResult = updater.Update([new(3, 4, 8, 9)], 17);
        updater.Dispose();

        Assert.AreEqual(
            string.Join('|',
                $"results:{firstResult}:{secondResult}",
                "get-desc:13",
                "create:64x32:0:PoolSystemmem:MultisampleNone:0",
                $"lock:29:{(uint) D3D9.LockNoDirtyUpdate}",
                "read:13:Direct3D9BitmapRealizationRectangle { Left = 1, Top = 2, Right = 11, Bottom = 12 }:Bgra32Bpp:256:8192:31",
                "dirty:29:Direct3D9SurfaceRect { Left = 1, Top = 2, Right = 11, Bottom = 12 }",
                "read:13:Direct3D9BitmapRealizationRectangle { Left = 20, Top = 21, Right = 30, Bottom = 31 }:Bgra32Bpp:256:8192:31",
                "dirty:29:Direct3D9SurfaceRect { Left = 20, Top = 21, Right = 30, Bottom = 31 }",
                "unlock:29",
                "update:29:41",
                $"lock:29:{(uint) D3D9.LockNoDirtyUpdate}",
                "read:17:Direct3D9BitmapRealizationRectangle { Left = 3, Top = 4, Right = 8, Bottom = 9 }:Bgra32Bpp:256:8192:31",
                "dirty:29:Direct3D9SurfaceRect { Left = 3, Top = 4, Right = 8, Bottom = 9 }",
                "unlock:29",
                "update:29:41",
                "release:29"),
            string.Join('|', [$"results:{Direct3D9Factory.SuccessHResult}:{Direct3D9Factory.SuccessHResult}", .. calls]));
    }

    [TestMethod]
    public void WhenSecondReadFailsThenFirstFailurePropagatesAndCleanupUnlockRuns()
    {
        List<string> calls = [];
        int readCalls = 0;
        Direct3D9DeviceBitmapColorSourceSoftwareUpdater updater = CreateUpdater(
            calls,
            readRenderTarget: (surface, rectangle, format, stride, bufferSize, buffer) =>
            {
                readCalls++;
                calls.Add($"read:{readCalls}");
                return readCalls == 2
                    ? Direct3D9Factory.InvalidCallHResult
                    : Direct3D9Factory.SuccessHResult;
            });

        int result = updater.Update([new(1, 2, 3, 4), new(5, 6, 7, 8)], 13);
        updater.Dispose();

        Assert.AreEqual(
            string.Join('|',
                $"result:{Direct3D9Factory.InvalidCallHResult}",
                "get-desc:13",
                "create:64x32:0:PoolSystemmem:MultisampleNone:0",
                $"lock:29:{(uint) D3D9.LockNoDirtyUpdate}",
                "read:1",
                "dirty:29:Direct3D9SurfaceRect { Left = 1, Top = 2, Right = 3, Bottom = 4 }",
                "read:2",
                "unlock:29",
                "release:29"),
            string.Join('|', [$"result:{result}", .. calls]));
    }

    [TestMethod]
    public void WhenUnlockFailsThenFailurePropagatesAndCleanupRetriesUnlockWithoutUpdating()
    {
        List<string> calls = [];
        int unlockCalls = 0;
        Direct3D9DeviceBitmapColorSourceSoftwareUpdater updater = CreateUpdater(
            calls,
            unlockTexture: texture =>
            {
                unlockCalls++;
                calls.Add($"unlock:{unlockCalls}:{texture}");
                return unlockCalls == 1
                    ? Direct3D9Factory.InvalidCallHResult
                    : Direct3D9Factory.SuccessHResult;
            });

        int result = updater.Update([new(1, 2, 3, 4)], 13);
        updater.Dispose();

        Assert.AreEqual(
            string.Join('|',
                $"result:{Direct3D9Factory.InvalidCallHResult}",
                "get-desc:13",
                "create:64x32:0:PoolSystemmem:MultisampleNone:0",
                $"lock:29:{(uint) D3D9.LockNoDirtyUpdate}",
                "read:13:Direct3D9BitmapRealizationRectangle { Left = 1, Top = 2, Right = 3, Bottom = 4 }:Bgra32Bpp:256:8192:31",
                "dirty:29:Direct3D9SurfaceRect { Left = 1, Top = 2, Right = 3, Bottom = 4 }",
                "unlock:1:29",
                "unlock:2:29",
                "release:29"),
            string.Join('|', [$"result:{result}", .. calls]));
    }

    [TestMethod]
    public void WhenTextureCreationFailsWithPointerThenReturnedTextureIsReleasedAndNotCached()
    {
        List<string> calls = [];
        Direct3D9DeviceBitmapColorSourceSoftwareUpdater updater = CreateUpdater(
            calls,
            createTexture: (SurfaceDesc description, out nint texture) =>
            {
                calls.Add("create-failed");
                texture = 29;
                return Direct3D9Factory.InvalidCallHResult;
            });

        int result = updater.Update([new(1, 2, 3, 4)], 13);
        updater.Dispose();

        Assert.AreEqual(
            string.Join('|',
                $"result:{Direct3D9Factory.InvalidCallHResult}",
                "get-desc:13",
                "create-failed",
                "release:29"),
            string.Join('|', [$"result:{result}", .. calls]));
    }

    private static Direct3D9DeviceBitmapColorSourceSoftwareUpdater CreateUpdater(
        List<string> calls,
        Direct3D9CreateBitmapSystemMemoryTexture? createTexture = null,
        Direct3D9ReadBitmapRenderTarget? readRenderTarget = null,
        Direct3D9UnlockBitmapSystemMemoryTexture? unlockTexture = null)
    {
        return new(
            41,
            MilPixelFormat.Bgra32Bpp,
            32,
            (nint surface, out SurfaceDesc description) =>
            {
                calls.Add($"get-desc:{surface}");
                description = new(
                    format: Format.A8R8G8B8,
                    type: Resourcetype.Surface,
                    usage: D3D9.UsageRendertarget,
                    pool: Pool.Default,
                    multiSampleType: MultisampleType.Multisample4Samples,
                    multiSampleQuality: 7,
                    width: 64,
                    height: 32);
                return Direct3D9Factory.SuccessHResult;
            },
            createTexture ?? ((SurfaceDesc description, out nint texture) =>
            {
                calls.Add($"create:{description.Width}x{description.Height}:{description.Usage}:{description.Pool}:{description.MultiSampleType}:{description.MultiSampleQuality}");
                texture = 29;
                return Direct3D9Factory.SuccessHResult;
            }),
            (nint texture, out LockedRect lockedRectangle, uint flags) =>
            {
                calls.Add($"lock:{texture}:{flags}");
                lockedRectangle = new(256, (void*) 31);
                return Direct3D9Factory.SuccessHResult;
            },
            readRenderTarget ?? ((surface, rectangle, format, stride, bufferSize, buffer) =>
            {
                calls.Add($"read:{surface}:{rectangle}:{format}:{stride}:{bufferSize}:{buffer}");
                return Direct3D9Factory.SuccessHResult;
            }),
            (texture, rectangle) =>
            {
                calls.Add($"dirty:{texture}:{rectangle}");
                return Direct3D9Factory.SuccessHResult;
            },
            unlockTexture ?? (texture =>
            {
                calls.Add($"unlock:{texture}");
                return Direct3D9Factory.SuccessHResult;
            }),
            (source, destination) =>
            {
                calls.Add($"update:{source}:{destination}");
                return Direct3D9Factory.SuccessHResult;
            },
            texture => calls.Add($"release:{texture}"));
    }
}
