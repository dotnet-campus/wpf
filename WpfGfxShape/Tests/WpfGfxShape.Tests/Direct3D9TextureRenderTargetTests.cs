using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9TextureRenderTargetTests
{
    private static readonly List<string> ReleaseOrder = [];
    private static Direct3D9Device? _device;
    private static int _addRefCount;
    private static int _releaseCount;

    [TestInitialize]
    public void Initialize()
    {
        ReleaseOrder.Clear();
        _device = null;
        _addRefCount = 0;
        _releaseCount = 0;
    }

    [TestMethod]
    public void WhenValidTextureRenderTargetIsDisposedThenTextureIsMadeEvictableInsideDeviceScopeBeforeBaseTarget()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture);
        _device = device;

        renderTarget.Dispose();
        bool baseTargetDisposed = ThrowsDisposed(() => surfaceRenderTarget.ClearInvalidatedRects());
        renderTarget.Dispose();

        Assert.AreEqual(
            (true, true, 1, "Texture:True"),
            (texture.IsEvictable, baseTargetDisposed, _releaseCount, string.Join('|', ReleaseOrder)));
    }

    [TestMethod]
    public void WhenTextureWasInvalidatedBeforeRenderTargetThenDisposeSkipsEvictableTransition()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture);
        texture.ReleaseFromManager();

        renderTarget.Dispose();

        Assert.AreEqual((false, 1, true),
            (texture.IsEvictable, _releaseCount, ThrowsDisposed(() => renderTarget.GetBitmapSource(out _))));
    }

    [TestMethod]
    public void WhenPartiallyInitializedTextureRenderTargetIsDisposedThenBaseTargetIsStillReleasedExactlyOnce()
    {
        using Direct3D9Device device = CreateDevice();
        Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        renderTarget.Dispose();
        renderTarget.Dispose();

        Assert.IsTrue(ThrowsDisposed(() => surfaceRenderTarget.ClearInvalidatedRects()));
    }

    [TestMethod]
    public void WhenBitmapSourceOutlivesRenderTargetThenItsIndependentTextureReferenceRemainsUntilReleased()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture);
        _device = device;
        Assert.AreEqual(0, renderTarget.GetBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? bitmapSource));

        renderTarget.Dispose();
        bool sourceValidAfterTargetRelease = bitmapSource!.Texture.IsValid;
        bitmapSource.Dispose();
        bool sourceReleased = bitmapSource.IsReleased;
        bitmapSource.Dispose();

        Assert.AreEqual(
            (1, 2, true, true, "Texture:True|Texture:False"),
            (_addRefCount, _releaseCount, sourceValidAfterTargetRelease, sourceReleased, string.Join('|', ReleaseOrder)));
    }

    [TestMethod]
    public void WhenBitmapIsRequestedRepeatedlyThenCachedObjectAndIndependentReferencesArePreserved()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture);
        _device = device;

        int firstResult = renderTarget.GetBitmap(out Direct3D9TextureRenderTargetBitmapSource? firstBitmap);
        int secondResult = renderTarget.GetBitmap(out Direct3D9TextureRenderTargetBitmapSource? secondBitmap);
        firstBitmap!.Dispose();
        bool secondBitmapStillValid = secondBitmap!.Texture.IsValid;
        secondBitmap.Dispose();
        bool cacheStillOwnsBitmap = !secondBitmap.IsReleased;

        renderTarget.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, Direct3D9Factory.SuccessHResult, true, true, true, 1, 2),
            (firstResult, secondResult, ReferenceEquals(firstBitmap, secondBitmap), secondBitmapStillValid,
                cacheStillOwnsBitmap, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenBitmapIsRequestedAfterDrawingThenSameCachedObjectIsRevalidatedBeforeOutput()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture);
        Assert.AreEqual(Direct3D9Factory.SuccessHResult,
            renderTarget.GetBitmap(out Direct3D9TextureRenderTargetBitmapSource? firstBitmap));
        Assert.AreEqual(Direct3D9Factory.SuccessHResult,
            renderTarget.DrawBitmap(static () => Direct3D9Factory.SuccessHResult));
        Assert.IsFalse(firstBitmap!.HasValidContents);

        int result = renderTarget.GetBitmap(out Direct3D9TextureRenderTargetBitmapSource? secondBitmap);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, true, true),
            (result, ReferenceEquals(firstBitmap, secondBitmap), secondBitmap!.HasValidContents));
        firstBitmap.Dispose();
        secondBitmap.Dispose();
    }

    [TestMethod]
    public void WhenBitmapCreationFailsOnceThenRetryCreatesCachedObjectAndReturnsIndependentReference()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        int creationCount = 0;
        using Direct3D9TextureRenderTarget renderTarget = new(
            device,
            surfaceRenderTarget,
            texture,
            retainedTexture =>
            {
                if (++creationCount == 1)
                {
                    Marshal.ThrowExceptionForHR(Direct3D9Factory.OutOfVideoMemoryHResult);
                }

                return new Direct3D9TextureRenderTargetBitmapSource(retainedTexture);
            });

        int firstResult = renderTarget.GetBitmap(out Direct3D9TextureRenderTargetBitmapSource? failedBitmap);
        int secondResult = renderTarget.GetBitmap(out Direct3D9TextureRenderTargetBitmapSource? bitmap);
        bitmap!.Dispose();
        bool cacheStillOwnsBitmap = !bitmap.IsReleased;

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, null, Direct3D9Factory.SuccessHResult, 2, true, 2, 1),
            (firstResult, failedBitmap, secondResult, creationCount, cacheStillOwnsBitmap, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenCacheableBitmapSourceIsRequestedThenExistingBitmapSourceIsReturnedWithIndependentReferences()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture);

        int firstResult = renderTarget.GetCacheableBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? firstSource);
        int secondResult = renderTarget.GetCacheableBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? secondSource);
        firstSource!.Dispose();
        bool secondSourceStillValid = secondSource!.Texture.IsValid;
        secondSource.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, Direct3D9Factory.SuccessHResult, true, true, 1),
            (firstResult, secondResult, ReferenceEquals(firstSource, secondSource), secondSourceStillValid, _addRefCount));
    }

    [TestMethod]
    public void WhenCacheableBitmapSourceIsRequestedAfterDrawingThenCachedContentsAreRevalidated()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture);
        Assert.AreEqual(Direct3D9Factory.SuccessHResult, renderTarget.GetBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? firstSource));
        Assert.AreEqual(Direct3D9Factory.SuccessHResult, renderTarget.DrawBitmap(static () => Direct3D9Factory.SuccessHResult));

        int result = renderTarget.GetCacheableBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? cacheableSource);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, true, true),
            (result, ReferenceEquals(firstSource, cacheableSource), cacheableSource!.HasValidContents));
        firstSource!.Dispose();
        cacheableSource.Dispose();
    }

    [TestMethod]
    public void WhenCacheableBitmapSourceCreationFailsThenFailureAndClearedOutputArePreserved()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture);
        texture.ReleaseFromManager();

        int result = renderTarget.GetCacheableBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? bitmapSource);

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, null), (result, bitmapSource));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.OutOfVideoMemoryHResult)]
    [DataRow(Direct3D9Factory.OutOfMemoryHResult)]
    public void WhenBitmapSourceConstructionFailsThenTemporaryReferenceIsReleasedAndOutputRemainsCleared(
        int expectedResult)
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(
            device,
            surfaceRenderTarget,
            texture,
            _ =>
            {
                Marshal.ThrowExceptionForHR(expectedResult);
                throw new InvalidOperationException();
            });

        int result = renderTarget.GetBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? bitmapSource);

        Assert.AreEqual((expectedResult, null, 1, 1), (result, bitmapSource, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenBitmapSourceConstructionFailsOnceThenNextRequestCanCreateAndTransferOneReference()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        int creationCount = 0;
        using Direct3D9TextureRenderTarget renderTarget = new(
            device,
            surfaceRenderTarget,
            texture,
            retainedTexture =>
            {
                if (++creationCount == 1)
                {
                    Marshal.ThrowExceptionForHR(Direct3D9Factory.OutOfVideoMemoryHResult);
                }

                return new Direct3D9TextureRenderTargetBitmapSource(retainedTexture);
            });

        int firstResult = renderTarget.GetBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? failedSource);
        int secondResult = renderTarget.GetBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? bitmapSource);
        bitmapSource!.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, null, Direct3D9Factory.SuccessHResult, 2, 2, 1),
            (firstResult, failedSource, secondResult, creationCount, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenTextureIsRequestedThenBorrowedInstanceIsReturnedWithoutAddingReference()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture);

        Direct3D9Texture? borrowedTexture = renderTarget.GetTextureNoRef();

        Assert.AreEqual((true, 0), (ReferenceEquals(texture, borrowedTexture), _addRefCount));
    }

    [TestMethod]
    public void WhenUnderlyingTextureIsInvalidThenRenderTargetIsInvalid()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture);
        bool validBeforeInvalidation = renderTarget.IsValid;

        texture.ReleaseFromManager();

        Assert.AreEqual((true, false), (validBeforeInvalidation, renderTarget.IsValid));
    }

    [TestMethod]
    public void WhenDescriptionsAreRequestedThenStoredSurfaceValuesArePreservedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp,
            width: 16,
            height: 12);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);
        MilPixelFormat expectedPixelFormat = surfaceRenderTarget.GetPixelFormat();
        (uint Width, uint Height) expectedSize = surfaceRenderTarget.GetSize();
        Matrix3x2 expectedDeviceTransform = surfaceRenderTarget.GetDeviceTransform();

        MilPixelFormat pixelFormat = renderTarget.GetPixelFormat();
        (uint Width, uint Height) size = renderTarget.GetSize();
        Matrix3x2 deviceTransform = renderTarget.GetDeviceTransform();

        Assert.AreEqual(
            (expectedPixelFormat, expectedSize, expectedDeviceTransform, false),
            (pixelFormat, size, deviceTransform, device.IsEntered()));
    }

    [TestMethod]
    public void WhenDescriptionsAreRequestedFromDefaultSurfaceThenDefaultValuesArePreserved()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        MilPixelFormat pixelFormat = renderTarget.GetPixelFormat();
        (uint Width, uint Height) size = renderTarget.GetSize();
        Matrix3x2 deviceTransform = renderTarget.GetDeviceTransform();

        Assert.AreEqual(
            (MilPixelFormat.Undefined, (0u, 0u), surfaceRenderTarget.GetDeviceTransform(), false),
            (pixelFormat, size, deviceTransform, device.IsEntered()));
    }

    [TestMethod]
    [DataRow(Format.A16B16G16R16f)]
    [DataRow(Format.Unknown)]
    public void WhenDirect3DTextureFormatIsRequestedThenStoredSurfaceFormatIsPreservedWithoutDeviceEntry(
        Format targetSurfaceFormat)
    {
        using Direct3D9Device device = CreateDevice();
        PresentParameters presentParameters = new(
            backBufferWidth: 16,
            backBufferHeight: 12,
            backBufferFormat: targetSurfaceFormat,
            backBufferCount: 1,
            swapEffect: Swapeffect.Discard,
            windowed: true,
            enableAutoDepthStencil: false,
            autoDepthStencilFormat: Format.Unknown);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp,
            presentParameters: presentParameters);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        Format result = renderTarget.GetDirect3DTextureFormat();

        Assert.AreEqual((targetSurfaceFormat, false), (result, device.IsEntered()));
    }

    [TestMethod]
    [DataRow(3u)]
    [DataRow(null)]
    public void WhenDisplayIdIsRequestedThenStoredAssociatedDisplayIsPreservedWithoutDeviceEntry(
        uint? associatedDisplayIndex)
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            associatedDisplayIndex: associatedDisplayIndex);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        uint? result = renderTarget.GetDisplayId();

        Assert.AreEqual((associatedDisplayIndex, false, false),
            (result, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public void WhenClearTypeHintIsSetThenSurfaceTargetOwnsTheUpdatedStateWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);
        List<bool> observedClearTypeSupport = [];

        int enableResult = renderTarget.SetClearTypeHint(forceClearType: true);
        int firstDrawResult = ObserveSurfaceClearTypeSupport();
        int repeatedEnableResult = renderTarget.SetClearTypeHint(forceClearType: true);
        int secondDrawResult = ObserveSurfaceClearTypeSupport();
        int disableResult = renderTarget.SetClearTypeHint(forceClearType: false);
        int thirdDrawResult = ObserveSurfaceClearTypeSupport();
        int repeatedDisableResult = renderTarget.SetClearTypeHint(forceClearType: false);
        int fourthDrawResult = ObserveSurfaceClearTypeSupport();

        Assert.AreEqual(
            ("True|True|False|False", 0, 0, 0, 0, 0, 0, 0, 0, false, false),
            (string.Join('|', observedClearTypeSupport), enableResult, firstDrawResult, repeatedEnableResult,
                secondDrawResult, disableResult, thirdDrawResult, repeatedDisableResult, fourthDrawResult,
                device.IsEntered(), device.IsInUseContext()));

        int ObserveSurfaceClearTypeSupport() => surfaceRenderTarget.DrawGlyphs(
            canDrawText: true,
            static () => 0,
            static () => 0,
            supportsClearType =>
            {
                observedClearTypeSupport.Add(supportsClearType);
                return 0;
            },
            static (_, _) => Direct3D9Factory.GenericFailureHResult);
    }

    [TestMethod]
    [DataRow(0xFFFE0200u, 0xFFFF0200u, true)]
    [DataRow(0xFFFE0101u, 0xFFFF0200u, false)]
    [DataRow(0xFFFE0200u, 0xFFFF0104u, false)]
    public void WhenShaderPipelineCapabilityIsRequestedThenSurfaceDeviceCapabilityIsPreservedWithoutDeviceEntry(
        uint vertexShaderVersion,
        uint pixelShaderVersion,
        bool expected)
    {
        Caps9 capabilities = new()
        {
            VertexShaderVersion = vertexShaderVersion,
            PixelShaderVersion = pixelShaderVersion
        };
        using Direct3D9Device device = CreateDevice(capabilities: capabilities);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        bool result = renderTarget.CanUseShaderPipeline();

        Assert.AreEqual((expected, false), (result, device.IsEntered()));
    }

    [TestMethod]
    [DataRow(23u)]
    [DataRow(Direct3D9ImmediateBrushRealizer.InvalidRealizationCacheIndex)]
    public void WhenRealizationCacheIndexIsRequestedThenSurfaceDeviceTokenIsPreservedWithoutDeviceEntry(
        uint realizationCacheIndex)
    {
        using Direct3D9Device device = CreateDevice(realizationCacheIndex: realizationCacheIndex);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        uint result = renderTarget.GetRealizationCacheIndex();

        Assert.AreEqual((realizationCacheIndex, false), (result, device.IsEntered()));
    }

    [TestMethod]
    public void WhenBoundsAndClearAreRequestedThenSurfaceTargetBehaviorIsPreserved()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 16,
            height: 12);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        MilRectF bounds = renderTarget.GetBounds();
        int result = renderTarget.Clear(color: null, aliasedClip: null);

        Assert.AreEqual((new MilRectF(0, 0, 16, 12), 0), (bounds, result));
    }

    [TestMethod]
    public void WhenBegin3DSucceedsThenEnd3DPreservesSurfaceTargetPairing()
    {
        int beginCount = 0;
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 16,
            height: 12,
            begin3DInternal: (bounds, z, useZBuffer, requested) =>
            {
                beginCount++;
                return new Direct3D9Begin3DResult(0, requested);
            });
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        int beginResult = renderTarget.Begin3D(new MilRectF(1, 2, 10, 11), MilAntiAliasMode.None, true, 0.5f);
        int endResult = renderTarget.End3D();
        int secondEndResult = renderTarget.End3D();

        Assert.AreEqual((0, 0, Direct3D9Factory.WgxInvalidCallHResult, 1),
            (beginResult, endResult, secondEndResult, beginCount));
    }

    [TestMethod]
    public void WhenBegin3DFailsThenFirstErrorIsReturnedAndEndRemainsUnpaired()
    {
        int beginCount = 0;
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 16,
            height: 12,
            begin3DInternal: (bounds, z, useZBuffer, requested) =>
            {
                beginCount++;
                return new Direct3D9Begin3DResult(Direct3D9Factory.OutOfVideoMemoryHResult, requested);
            });
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        int beginResult = renderTarget.Begin3D(new MilRectF(0, 0, 16, 12), MilAntiAliasMode.None, false, 0);
        int endResult = renderTarget.End3D();

        Assert.AreEqual((Direct3D9Factory.OutOfVideoMemoryHResult, Direct3D9Factory.WgxInvalidCallHResult, 1),
            (beginResult, endResult, beginCount));
    }

    [TestMethod]
    [DataRow("Bitmap", Direct3D9Factory.SuccessHResult)]
    [DataRow("Bitmap", Direct3D9Factory.OutOfVideoMemoryHResult)]
    [DataRow("Mesh3D", Direct3D9Factory.SuccessHResult)]
    [DataRow("Mesh3D", Direct3D9Factory.OutOfVideoMemoryHResult)]
    [DataRow("Path", Direct3D9Factory.SuccessHResult)]
    [DataRow("Path", Direct3D9Factory.OutOfVideoMemoryHResult)]
    [DataRow("InfinitePath", Direct3D9Factory.SuccessHResult)]
    [DataRow("InfinitePath", Direct3D9Factory.OutOfVideoMemoryHResult)]
    [DataRow("Glyphs", Direct3D9Factory.SuccessHResult)]
    [DataRow("Glyphs", Direct3D9Factory.OutOfVideoMemoryHResult)]
    [DataRow("Video", Direct3D9Factory.SuccessHResult)]
    [DataRow("Video", Direct3D9Factory.OutOfVideoMemoryHResult)]
    public void WhenTextureTargetDrawsThenContentsAreInvalidatedBeforeExactlyOneSurfaceDelegation(
        string operation,
        int delegatedResult)
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture);
        Assert.AreEqual(0, renderTarget.GetBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? bitmapSource));
        int delegationCount = 0;

        int result = Draw(renderTarget, operation, () =>
        {
            delegationCount++;
            Assert.IsFalse(bitmapSource!.HasValidContents);
            return delegatedResult;
        });
        int secondResult = Draw(renderTarget, operation, () =>
        {
            delegationCount++;
            return delegatedResult;
        });

        Assert.AreEqual((delegatedResult, delegatedResult, 2, false),
            (result, secondResult, delegationCount, bitmapSource!.HasValidContents));
        bitmapSource.Dispose();
    }

    [TestMethod]
    [DataRow("Bitmap")]
    [DataRow("Mesh3D")]
    [DataRow("Path")]
    [DataRow("InfinitePath")]
    [DataRow("Glyphs")]
    [DataRow("Video")]
    public void WhenBitmapSourceIsRequestedAfterDrawingThenItsContentsBecomeValidAgain(string operation)
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Device device = CreateDevice();
        Direct3D9Texture texture = new(device.ResourceManager, textureObject.Texture, 16, 12);
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture);
        Assert.AreEqual(0, renderTarget.GetBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? firstSource));
        Assert.AreEqual(0, Draw(renderTarget, operation, static () => 0));

        int result = renderTarget.GetBitmapSource(out Direct3D9TextureRenderTargetBitmapSource? secondSource);

        Assert.AreEqual((0, true, true),
            (result, ReferenceEquals(firstSource, secondSource), secondSource!.HasValidContents));
        firstSource!.Dispose();
        secondSource.Dispose();
    }

    [TestMethod]
    public void WhenTextureRenderTargetIsDisposedThenDelegatedMembersAreProtected()
    {
        using Direct3D9Device device = CreateDevice();
        Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        renderTarget.Dispose();
        renderTarget.Dispose();

        Assert.IsTrue(
            ThrowsDisposed(() => _ = renderTarget.IsValid)
            && ThrowsDisposed(() => renderTarget.GetBitmap(out _))
            && ThrowsDisposed(() => renderTarget.GetCacheableBitmapSource(out _))
            && ThrowsDisposed(() => _ = renderTarget.GetTextureNoRef())
            && ThrowsDisposed(() => _ = renderTarget.GetPixelFormat())
            && ThrowsDisposed(() => _ = renderTarget.GetSize())
            && ThrowsDisposed(() => _ = renderTarget.GetDeviceTransform())
            && ThrowsDisposed(() => _ = renderTarget.GetDirect3DTextureFormat())
            && ThrowsDisposed(() => _ = renderTarget.CanUseShaderPipeline())
            && ThrowsDisposed(() => _ = renderTarget.GetRealizationCacheIndex())
            && ThrowsDisposed(() => _ = renderTarget.GetDisplayId())
            && ThrowsDisposed(() => renderTarget.SetClearTypeHint(forceClearType: true))
            && ThrowsDisposed(() => _ = renderTarget.GetBounds())
            && ThrowsDisposed(() => renderTarget.Clear(null, null))
            && ThrowsDisposed(() => renderTarget.Begin3D(default, MilAntiAliasMode.None, false, 0))
            && ThrowsDisposed(() => renderTarget.End3D())
            && ThrowsDisposed(() => renderTarget.DrawBitmap(static () => 0))
            && ThrowsDisposed(() => renderTarget.DrawMesh3D(static () => 0))
            && ThrowsDisposed(() => renderTarget.DrawPath(static () => 0))
            && ThrowsDisposed(() => renderTarget.DrawInfinitePath(static () => 0))
            && ThrowsDisposed(() => renderTarget.DrawGlyphs(static () => 0))
            && ThrowsDisposed(() => renderTarget.DrawVideo(static () => 0)));
    }

    [TestMethod]
    public void WhenRenderTargetFormatCheckFailsThenCandidateIsNotCreated()
    {
        int textureCreationCount = 0;
        using Direct3D9Device device = CreateDevice(
            checkRenderTargetFormat: format =>
            {
                Assert.AreEqual(Format.A8R8G8B8, format);
                return Direct3D9Factory.InvalidCallHResult;
            },
            createTexture: (_, _, _, _, _, _) =>
            {
                textureCreationCount++;
                return 0;
            });

        int result = Direct3D9TextureRenderTarget.TryCreate(16, 12, device, null, out Direct3D9TextureRenderTarget? renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, (Direct3D9TextureRenderTarget?) null),
            (result, textureCreationCount, renderTarget));
    }

    [TestMethod]
    public void WhenRealizationCacheIndexIsInvalidThenTextureIsNotCreated()
    {
        int textureCreationCount = 0;
        using Direct3D9Device device = CreateDevice(
            checkRenderTargetFormat: static _ => 0,
            createTexture: (_, _, _, _, _, _) =>
            {
                textureCreationCount++;
                return 0;
            });

        int result = Direct3D9TextureRenderTarget.TryCreate(16, 12, device, null, out Direct3D9TextureRenderTarget? renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, (Direct3D9TextureRenderTarget?) null),
            (result, textureCreationCount, renderTarget));
    }

    [TestMethod]
    public void WhenSurfaceDescriptionIsRequestedThenNativeTextureContractIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            capabilities: CreateCapabilities(64, 64));

        int result = Direct3D9TextureRenderTarget.GetSurfaceDescription(17, 31, device, out SurfaceDesc description);

        (int Result, Format Format, Resourcetype Type, uint Usage, Pool Pool,
            MultisampleType MultisampleType, uint MultisampleQuality, uint Width, uint Height) actual =
            (result, description.Format, description.Type, description.Usage, description.Pool,
                description.MultiSampleType, description.MultiSampleQuality, description.Width, description.Height);
        (int Result, Format Format, Resourcetype Type, uint Usage, Pool Pool,
            MultisampleType MultisampleType, uint MultisampleQuality, uint Width, uint Height) expected =
            (0, Format.A8R8G8B8, Resourcetype.Texture, D3D9.UsageRendertarget, Pool.Default,
                MultisampleType.MultisampleNone, 0, 17, 31);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void WhenSurfaceDimensionsExceedCapsThenUnsupportedTextureSizeIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            capabilities: CreateCapabilities(16, 12));

        int result = Direct3D9TextureRenderTarget.GetSurfaceDescription(17, 13, device, out _);

        Assert.AreEqual(Direct3D9Factory.UnsupportedTextureSizeHResult, result);
    }

    [TestMethod]
    public void WhenRenderTargetTextureCreationFailsThenFirstErrorAndFixedDescriptionArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        (uint Width, uint Height, uint Levels, uint Usage, Format Format, Pool Pool)? actual = null;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            capabilities: CreateCapabilities(64, 64),
            realizationCacheIndex: 3,
            checkRenderTargetFormat: static _ => 0,
            createTexture: (width, height, levels, usage, format, pool) =>
            {
                actual = (width, height, levels, usage, format, pool);
                return Direct3D9Factory.OutOfVideoMemoryHResult;
            });

        int result = Direct3D9TextureRenderTarget.TryCreate(17, 31, device, 2, out Direct3D9TextureRenderTarget? renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult,
                ((uint Width, uint Height, uint Levels, uint Usage, Format Format, Pool Pool)?)
                    (17u, 31u, 1u, D3D9.UsageRendertarget, Format.A8R8G8B8, Pool.Default),
                (Direct3D9TextureRenderTarget?) null),
            (result, actual, renderTarget));
    }

    [TestMethod]
    public void WhenInterfaceIsRequestedThenSurfaceResultAndClearedOutputArePreserved()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        int result = renderTarget.FindInterface(Guid.NewGuid(), out nint interfacePointer);

        Assert.AreEqual((Direct3D9Factory.NoInterfaceHResult, 0), (result, interfacePointer));
    }

    [TestMethod]
    public void WhenRenderTargetTypeIsRequestedThenSurfaceHardwareTypeIsPreserved()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        InternalRenderTargetType result = renderTarget.GetRenderTargetType();

        Assert.AreEqual(InternalRenderTargetType.HardwareRaster, result);
    }

    [TestMethod]
    public void WhenSurfaceTargetIsValidThenQueuedPresentResultAndOutputAreForwardedExactlyOnceInsideDeviceEntry()
    {
        int queryCalls = 0;
        Direct3D9Device? queriedDevice = null;
        using Direct3D9Device device = CreateDevice(
            getNumQueuedPresents: (out uint queuedPresentCount) =>
            {
                queryCalls++;
                queuedPresentCount = 4;
                Assert.IsTrue(queriedDevice!.IsEntered());
                return Direct3D9Factory.GenericFailureHResult;
            });
        queriedDevice = device;
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: new Direct3D9Surface(new Direct3D9ResourceManager(), null));
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        int result = renderTarget.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 4u, 1, false),
            (result, queuedPresentCount, queryCalls, device.IsEntered()));
    }

    [TestMethod]
    public void WhenSurfaceTargetIsInvalidThenQueuedPresentOutputIsZeroWithoutDeviceQuery()
    {
        int queryCalls = 0;
        using Direct3D9Device device = CreateDevice(
            getNumQueuedPresents: (out uint queuedPresentCount) =>
            {
                queryCalls++;
                queuedPresentCount = 4;
                return 0;
            });
        using Direct3D9SurfaceRenderTarget surfaceRenderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9TextureRenderTarget renderTarget = new(device, surfaceRenderTarget, texture: null);

        int result = renderTarget.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual((0, 0u, 0, false), (result, queuedPresentCount, queryCalls, device.IsEntered()));
    }

    [TestMethod]
    public void WhenDisposedThenInterfaceQueryRejectsUseAfterRepeatedDispose()
    {
        using Direct3D9Device device = CreateDevice();
        Direct3D9TextureRenderTarget renderTarget = new(
            device,
            new Direct3D9SurfaceRenderTarget(device, MultisampleType.MultisampleNone),
            texture: null);
        renderTarget.Dispose();
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.FindInterface(Guid.Empty, out _));
    }

    [TestMethod]
    public void WhenDisposedThenRenderTargetTypeQueryRejectsUseAfterRepeatedDispose()
    {
        using Direct3D9Device device = CreateDevice();
        Direct3D9TextureRenderTarget renderTarget = new(
            device,
            new Direct3D9SurfaceRenderTarget(device, MultisampleType.MultisampleNone),
            texture: null);
        renderTarget.Dispose();
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.GetRenderTargetType());
    }

    [TestMethod]
    public void WhenDisposedThenQueuedPresentQueryRejectsUseAfterRepeatedDispose()
    {
        using Direct3D9Device device = CreateDevice();
        Direct3D9TextureRenderTarget renderTarget = new(
            device,
            new Direct3D9SurfaceRenderTarget(device, MultisampleType.MultisampleNone),
            texture: null);
        renderTarget.Dispose();
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.GetNumQueuedPresents(out _));
    }

    private static int Draw(Direct3D9TextureRenderTarget renderTarget, string operation, Func<int> draw)
    {
        return operation switch
        {
            "Bitmap" => renderTarget.DrawBitmap(draw),
            "Mesh3D" => renderTarget.DrawMesh3D(draw),
            "Path" => renderTarget.DrawPath(draw),
            "InfinitePath" => renderTarget.DrawInfinitePath(draw),
            "Glyphs" => renderTarget.DrawGlyphs(draw),
            "Video" => renderTarget.DrawVideo(draw),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }

    private static Caps9 CreateCapabilities(uint maximumWidth, uint maximumHeight)
    {
        return new Caps9
        {
            MaxTextureWidth = maximumWidth,
            MaxTextureHeight = maximumHeight,
            TextureCaps = (uint) D3D9.PtexturecapsNonpow2Conditional
        };
    }

    private static Direct3D9Device CreateDevice(
        IDirect3DDevice9* nativeDevice = null,
        Caps9 capabilities = default,
        uint realizationCacheIndex = Direct3D9ImmediateBrushRealizer.InvalidRealizationCacheIndex,
        Func<Format, int>? checkRenderTargetFormat = null,
        Func<uint, uint, uint, uint, Format, Pool, int>? createTexture = null,
        Direct3D9GetNumQueuedPresents? getNumQueuedPresents = null)
    {
        return new Direct3D9Device(
            nativeDevice,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            capabilities: capabilities,
            createTexture: createTexture,
            checkRenderTargetFormat: checkRenderTargetFormat,
            getNumQueuedPresents: getNumQueuedPresents,
            realizationCacheIndex: realizationCacheIndex);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private static bool ThrowsDisposed(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ObjectDisposedException)
        {
            return true;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefTexture(IDirect3DTexture9* self)
    {
        return (uint) ++_addRefCount;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseTexture(IDirect3DTexture9* self)
    {
        _releaseCount++;
        ReleaseOrder.Add($"Texture:{_device?.IsEntered() == true}");
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint GetTextureLevelCount(IDirect3DTexture9* self) => 1;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetTextureLevelDescription(IDirect3DTexture9* self, uint level, SurfaceDesc* description)
    {
        *description = new SurfaceDesc(
            format: Format.A8R8G8B8,
            type: Resourcetype.Texture,
            pool: Pool.Default,
            width: 16,
            height: 12);
        return Direct3D9Factory.SuccessHResult;
    }

    private struct FakeTextureObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DTexture9* Texture;

        public FakeTextureObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 19);
            void** memory = (void**) _memory;
            Texture = (IDirect3DTexture9*) memory;
            void** vtable = memory + 1;
            Texture->LpVtbl = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &AddRefTexture;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &ReleaseTexture;
            vtable[13] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &GetTextureLevelCount;
            vtable[17] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, SurfaceDesc*, int>) &GetTextureLevelDescription;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Texture = null;
        }
    }
}
