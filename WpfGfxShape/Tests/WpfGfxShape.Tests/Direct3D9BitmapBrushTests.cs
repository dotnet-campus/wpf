using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapBrushTests
{
    [TestMethod]
    public void WhenBrushIsSetThenBitmapColorSourceIsDerivedAndStored()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(calls);

        int result = brush.SetBrushAndContext(3, 5);

        Assert.AreEqual((0, (nint) 17, "Derive:3:5"), (result, brush.TexturedColorSource?.BitmapColorSource ?? 0, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenDerivationFailsWithAColorSourceThenReleaseCleansTheScratchBrush()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(calls, deriveResult: Direct3D9Factory.GenericFailureHResult);
        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, brush.SetBrushAndContext(3, 5));

        uint releaseResult = brush.Release();

        Assert.AreEqual((0u, true, "Derive:3:5|DisposeRealizer|Release:17"), (releaseResult, brush.TexturedColorSource is null, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenTextureHasMaskThenOperationsFollowNativeOrderAndReleaseMask()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(calls);
        Assert.AreEqual(0, brush.SetBrushAndContext(3, 5));

        int result = brush.SendOperations();

        Assert.AreEqual(
            (0, "Derive:3:5|Texture|GetMask:17|ResetMask:19|MultiplyMask:19|Release:19"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenTextureHasNoMaskThenOperationsStopAfterMaskLookup()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(
            calls,
            getMaskColorSource: (nint texturedColorSource, out nint maskColorSource) =>
            {
                calls.Add($"GetMask:{texturedColorSource}");
                maskColorSource = 0;
                return 0;
            });
        Assert.AreEqual(0, brush.SetBrushAndContext(3, 5));

        int result = brush.SendOperations();

        Assert.AreEqual((0, "Derive:3:5|Texture|GetMask:17"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenSettingTextureFailsThenMaskOperationsAreSkipped()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(
            calls,
            setTexture: _ =>
            {
                calls.Add("Texture");
                return Direct3D9Factory.GenericFailureHResult;
            });
        Assert.AreEqual(0, brush.SetBrushAndContext(3, 5));

        int result = brush.SendOperations();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Derive:3:5|Texture"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenMaskLookupFailsWithAMaskThenFailureIsPreservedAndMaskIsReleased()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(
            calls,
            getMaskColorSource: (nint texturedColorSource, out nint maskColorSource) =>
            {
                calls.Add($"GetMask:{texturedColorSource}");
                maskColorSource = 19;
                return Direct3D9Factory.GenericFailureHResult;
            });
        Assert.AreEqual(0, brush.SetBrushAndContext(3, 5));

        int result = brush.SendOperations();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Derive:3:5|Texture|GetMask:17|Release:19"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenAlphaMaskOperationFailsThenMaskIsStillReleased()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(
            calls,
            multiplyAlphaMask: maskColorSource =>
            {
                calls.Add($"MultiplyMask:{maskColorSource}");
                return Direct3D9Factory.GenericFailureHResult;
            });
        Assert.AreEqual(0, brush.SetBrushAndContext(3, 5));

        int result = brush.SendOperations();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Derive:3:5|Texture|GetMask:17|ResetMask:19|MultiplyMask:19|Release:19"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenAlphaMaskOperationFailsThenBrushReleaseDisposesOwnershipInReverseOrder()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(
            calls,
            multiplyAlphaMask: _ => Direct3D9Factory.GenericFailureHResult);
        Assert.AreEqual(0, brush.SetBrushAndContext(3, 5));
        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, brush.SendOperations());

        brush.Release();

        Assert.AreEqual(
            "Derive:3:5|Texture|GetMask:17|ResetMask:19|Release:19|DisposeRealizer|Release:17",
            string.Join('|', calls));
    }

    [TestMethod]
    public void WhenReleasedTwiceThenTexturedColorSourceIsReleasedOnlyOnce()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(calls);
        Assert.AreEqual(0, brush.SetBrushAndContext(3, 5));

        brush.Release();
        brush.Release();

        Assert.AreEqual("Derive:3:5|DisposeRealizer|Release:17", string.Join('|', calls));
    }

    [TestMethod]
    public void WhenBitmapPrimarySendsOperationsThenOwnedTextureReachesFixedFunctionBuilder()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(
            calls,
            getMaskColorSource: (nint texturedColorSource, out nint maskColorSource) =>
            {
                calls.Add($"GetMask:{texturedColorSource}");
                maskColorSource = 0;
                return 0;
            });
        Assert.AreEqual(0, brush.SetBrushAndContext(3, 5));
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        Direct3D9PipelineOperationSender sender = new(
            () => 0,
            () => 0,
            () => 0,
            () => 0,
            setTexture: builder.SetTexture);

        int result = sender.SendPipelineOperations(brush.PrimaryColorSource);
        brush.Release();
        Direct3D9FixedFunctionPipelineItem item = builder.DetachItems()[0];

        Assert.AreEqual(
            (0, (nint) 17, Direct3D9VertexFormatAttribute.Uv1, Direct3D9FixedFunctionBlendArgument.Texture, "Derive:3:5|GetMask:17"),
            (result, item.Ownership?.BitmapColorSource ?? 0, item.SourceLocation, item.Source1, string.Join('|', calls)));
        Direct3D9FixedFunctionPipelineItemBuilder.ReleaseOwnedColorSources([item]);
    }

    [TestMethod]
    public void WhenBitmapPrimarySendsOperationsThenOwnedTextureReachesShaderBuilderWithoutTransform()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(
            calls,
            getMaskColorSource: (nint texturedColorSource, out nint maskColorSource) =>
            {
                calls.Add($"GetMask:{texturedColorSource}");
                maskColorSource = 0;
                return 0;
            });
        Assert.AreEqual(0, brush.SetBrushAndContext(3, 5));
        using Direct3D9ShaderPipelineItemBuilder builder = new();
        Direct3D9PipelineOperationSender sender = new(
            () => 0,
            () => 0,
            () => 0,
            () => 0,
            setTexture: builder.SetTexture);

        int result = sender.SendPipelineOperations(brush.PrimaryColorSource);
        brush.Release();
        Direct3D9ShaderPipelineItem item = builder.DetachItems()[0];

        Assert.AreEqual(
            (0, (nint) 17, 0u, Direct3D9VertexFormatAttribute.Uv1, Direct3D9ShaderTextureFunction.MultiplyTextureNoTransformFromTextureCoordinate, "Derive:3:5|GetMask:17"),
            (result, item.Ownership?.BitmapColorSource ?? 0, item.Sampler, item.TextureCoordinates, item.TextureFunction, string.Join('|', calls)));
        Direct3D9ShaderPipelineItemBuilder.ReleaseOwnedColorSources([item]);
    }

    [TestMethod]
    public void WhenBitmapPrimaryUsesIncomingUvThenShaderBuilderAssignsTransformHandle()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(
            calls,
            getMaskColorSource: (nint texturedColorSource, out nint maskColorSource) =>
            {
                maskColorSource = 0;
                return 0;
            },
            pipelineColorSourceFactory: () => new Direct3D9PipelineColorSource(
                Direct3D9ColorSourceType.Texture,
                () => 0,
                ResetForPipelineReuse: () => calls.Add("Reset"),
                SetTextureTransformHandle: handle => calls.Add($"Handle:{handle}")));
        Assert.AreEqual(0, brush.SetBrushAndContext(3, 5));
        using Direct3D9ShaderPipelineItemBuilder builder = new(
            Direct3D9VertexFormatAttribute.Uv1,
            function =>
            {
                calls.Add($"Function:{function}");
                return 13;
            });
        Direct3D9PipelineOperationSender sender = new(
            () => 0,
            () => 0,
            () => 0,
            () => 0,
            setTexture: builder.SetTexture);

        int result = sender.SendPipelineOperations(brush.PrimaryColorSource);
        brush.Release();
        Direct3D9ShaderPipelineItem item = builder.DetachItems()[0];

        Assert.AreEqual(
            (0, Direct3D9VertexFormatAttribute.Uv1, Direct3D9ShaderTextureFunction.MultiplyTextureTransformFromVertexUv, "Derive:3:5|Function:MultiplyTextureTransformFromVertexUv|Reset|Handle:13"),
            (result, item.TextureCoordinates, item.TextureFunction, string.Join('|', calls)));
        Direct3D9ShaderPipelineItemBuilder.ReleaseOwnedColorSources([item]);
    }

    [TestMethod]
    public void WhenBitmapPrimarySetTextureFailsThenTransferredReferenceIsReleasedAndMaskIsSkipped()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(calls);
        Assert.AreEqual(0, brush.SetBrushAndContext(3, 5));
        Direct3D9PipelineOperationSender sender = new(
            () => 0,
            () => 0,
            () => 0,
            () => 0,
            setTexture: _ =>
            {
                calls.Add("SetTexture");
                return Direct3D9Factory.OutOfVideoMemoryHResult;
            });

        int result = sender.SendPipelineOperations(brush.PrimaryColorSource);
        brush.Release();

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, "Derive:3:5|SetTexture|DisposeRealizer|Release:17"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenBitmapPrimaryHasNoSetTextureTargetThenInvalidCallSkipsMaskAndLaterOperations()
    {
        List<string> calls = [];
        Direct3D9BitmapBrush brush = CreateBrush(calls);
        Assert.AreEqual(0, brush.SetBrushAndContext(3, 5));
        Direct3D9PipelineOperationSender sender = new(
            () =>
            {
                calls.Add("Effects");
                return 0;
            },
            () => 0,
            () => 0,
            () => 0);

        int result = sender.SendPipelineOperations(brush.PrimaryColorSource);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, "Derive:3:5"),
            (result, string.Join('|', calls)));
    }

    private static Direct3D9BitmapBrush CreateBrush(
        List<string> calls,
        int deriveResult = 0,
        Direct3D9GetMaskColorSource? getMaskColorSource = null,
        Func<Direct3D9PipelineColorSource, int>? setTexture = null,
        Func<nint, int>? multiplyAlphaMask = null,
        Func<Direct3D9PipelineColorSource>? pipelineColorSourceFactory = null)
    {
        return new Direct3D9BitmapBrush(
            (nint bitmapBrush, nint brushContext, out Direct3D9BitmapPipelineColorSource? texturedColorSource) =>
            {
                calls.Add($"Derive:{bitmapBrush}:{brushContext}");
                texturedColorSource = CreatePipelineColorSource(calls, pipelineColorSourceFactory);
                return deriveResult;
            },
            getMaskColorSource ?? ((nint texturedColorSource, out nint maskColorSource) =>
            {
                calls.Add($"GetMask:{texturedColorSource}");
                maskColorSource = 19;
                return 0;
            }),
            setTexture ?? (_ =>
            {
                calls.Add("Texture");
                return 0;
            }),
            maskColorSource => calls.Add($"ResetMask:{maskColorSource}"),
            multiplyAlphaMask ?? (maskColorSource =>
            {
                calls.Add($"MultiplyMask:{maskColorSource}");
                return 0;
            }),
            colorSource => calls.Add($"Release:{colorSource}"));
    }

    private static Direct3D9BitmapPipelineColorSource CreatePipelineColorSource(
        List<string> calls,
        Func<Direct3D9PipelineColorSource>? pipelineColorSourceFactory = null) => new(
        17,
        new CallbackDisposable(() => calls.Add("DisposeRealizer")),
        pipelineColorSourceFactory?.Invoke() ?? new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0),
        colorSource => calls.Add($"Release:{colorSource}"));

    private sealed class CallbackDisposable(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }
}
