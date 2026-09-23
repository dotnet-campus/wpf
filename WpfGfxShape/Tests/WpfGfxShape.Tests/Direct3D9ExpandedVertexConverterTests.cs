using System.Numerics;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9ExpandedVertexConverterTests
{
    [TestMethod]
    public void WhenNoAttributesAreGeneratedThenVertexBitsAreCopied()
    {
        Direct3D9ExpandedVertexConverter converter = CreateConverter(
            Direct3D9VertexFormatAttribute.Xy | Direct3D9VertexFormatAttribute.Diffuse | Direct3D9VertexFormatAttribute.Uv1,
            Direct3D9VertexFormatAttribute.Xy | Direct3D9VertexFormatAttribute.Diffuse | Direct3D9VertexFormatAttribute.Uv1);
        Direct3D9ExpandedVertex source = new()
        {
            X = BitConverter.UInt32BitsToSingle(0x7FC00001),
            Y = -0f,
            Diffuse = 0x12345678,
            U0 = BitConverter.UInt32BitsToSingle(0x3F123456),
            V0 = BitConverter.UInt32BitsToSingle(0xBF654321),
        };
        Direct3D9ExpandedVertex destination = default;

        int result = converter.TransferAndExpandVertices([source], new Span<Direct3D9ExpandedVertex>(ref destination), false);

        Assert.AreEqual(
            (0, 0x7FC00001u, 0x80000000u, 0x12345678u, 0x3F123456u, 0xBF654321u),
            (result,
             BitConverter.SingleToUInt32Bits(destination.X),
             BitConverter.SingleToUInt32Bits(destination.Y),
             destination.Diffuse,
             BitConverter.SingleToUInt32Bits(destination.U0),
             BitConverter.SingleToUInt32Bits(destination.V0)));
    }

    [TestMethod]
    public void WhenPositionTransformIsRequestedThenUvUsesTransformedPosition()
    {
        Direct3D9ExpandedVertexConverter converter = CreateConverter(
            Direct3D9VertexFormatAttribute.Xy,
            Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Uv1,
            configure: converter =>
            {
                converter.SetPositionTransform(new Matrix3x2(2, 0, 0, 3, 5, 7));
                Assert.AreEqual(0, converter.SetTextureMapping(0, uint.MaxValue, new Matrix3x2(4, 0, 0, 5, 11, 13)));
            });
        Direct3D9ExpandedVertex destination = default;

        int result = converter.TransferAndExpandVertices(
            [new Direct3D9ExpandedVertex { X = 1, Y = 2 }],
            new Span<Direct3D9ExpandedVertex>(ref destination),
            true);

        Assert.AreEqual((0, 7f, 13f, 39f, 78f), (result, destination.X, destination.Y, destination.U0, destination.V0));
    }

    [TestMethod]
    public void WhenZAndConstantDiffuseAreGeneratedThenConfiguredValuesAreUsed()
    {
        Direct3D9ExpandedVertexConverter converter = CreateConverter(
            Direct3D9VertexFormatAttribute.Xy,
            Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Diffuse,
            configure: converter =>
            {
                Assert.AreEqual(0, converter.SetZMapping(0.25f));
                Assert.AreEqual(0, converter.SetConstantDiffuseMapping(0x80402010));
            });
        Direct3D9ExpandedVertex vertex = new() { X = 2, Y = 3 };

        int result = converter.ExpandVertices(new Span<Direct3D9ExpandedVertex>(ref vertex));

        Assert.AreEqual((0, 0.25f, 0x80402010u), (result, vertex.Z, vertex.Diffuse));
    }

    [TestMethod]
    public void WhenCoverageScalesDiffuseThenNativePackedColorRoundingIsPreserved()
    {
        Direct3D9ExpandedVertexConverter converter = CreateConverter(
            Direct3D9VertexFormatAttribute.Xy,
            Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Diffuse,
            Direct3D9VertexFormatAttribute.Diffuse,
            converter => Assert.AreEqual(0, converter.SetConstantDiffuseMapping(0x80402010)));
        Direct3D9ExpandedVertex[] vertices =
        [
            new() { Diffuse = BitConverter.SingleToUInt32Bits(0f) },
            new() { Diffuse = BitConverter.SingleToUInt32Bits(0.5f) },
            new() { Diffuse = BitConverter.SingleToUInt32Bits(1f) },
        ];

        int result = converter.ExpandVertices(vertices);

        Assert.AreEqual((0, 0u, 0x40201008u, 0x80402010u),
            (result, vertices[0].Diffuse, vertices[1].Diffuse, vertices[2].Diffuse));
    }

    [TestMethod]
    public void WhenMultipleUvsAreGeneratedThenMappingsRemainInDestinationOrder()
    {
        Direct3D9ExpandedVertexConverter converter = CreateConverter(
            Direct3D9VertexFormatAttribute.Xy,
            Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Uv3,
            configure: converter =>
            {
                Assert.AreEqual(0, converter.SetTextureMapping(0, uint.MaxValue, Matrix3x2.Identity));
                Assert.AreEqual(0, converter.SetTextureMapping(1, uint.MaxValue, Matrix3x2.CreateTranslation(10, 20)));
                Assert.AreEqual(0, converter.SetTextureMapping(2, uint.MaxValue, Matrix3x2.CreateScale(2, 3)));
            });
        Direct3D9ExpandedVertex vertex = new() { X = 4, Y = 5 };

        int result = converter.ExpandVertices(new Span<Direct3D9ExpandedVertex>(ref vertex));

        Assert.AreEqual((0, new Vector2(4, 5), new Vector2(14, 25), new Vector2(8, 15)),
            (result, vertex.GetTextureCoordinate(0), vertex.GetTextureCoordinate(1), vertex.GetTextureCoordinate(2)));
    }

    [TestMethod]
    public void WhenFastAndGeneralPathsAreUsedThenResultsAreIdentical()
    {
        Direct3D9ExpandedVertexConverter converter = CreateConverter(
            Direct3D9VertexFormatAttribute.Xy,
            Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Diffuse | Direct3D9VertexFormatAttribute.Uv1,
            Direct3D9VertexFormatAttribute.Diffuse,
            converter =>
            {
                Assert.AreEqual(0, converter.SetConstantDiffuseMapping(0xFF804020));
                Assert.AreEqual(0, converter.SetTextureMapping(0, uint.MaxValue, new Matrix3x2(2, 3, 4, 5, 6, 7)));
            });
        Direct3D9ExpandedVertex[] source =
        [
            new() { X = 1, Y = 2, Diffuse = BitConverter.SingleToUInt32Bits(0.25f) },
            new() { X = -3, Y = 4, Diffuse = BitConverter.SingleToUInt32Bits(0.75f) },
        ];
        Direct3D9ExpandedVertex[] fast = new Direct3D9ExpandedVertex[source.Length];
        Direct3D9ExpandedVertex[] general = new Direct3D9ExpandedVertex[source.Length];

        int fastResult = converter.TransferAndExpandVertices(source, fast, false);
        int generalResult = converter.TransferAndExpandVerticesGeneral(source, general, false);

        Assert.AreEqual((0, 0, fast[0].X, fast[0].Y, fast[0].Z, fast[0].Diffuse, fast[0].U0, fast[0].V0,
            fast[1].X, fast[1].Y, fast[1].Z, fast[1].Diffuse, fast[1].U0, fast[1].V0),
            (fastResult, generalResult, general[0].X, general[0].Y, general[0].Z, general[0].Diffuse, general[0].U0, general[0].V0,
             general[1].X, general[1].Y, general[1].Z, general[1].Diffuse, general[1].U0, general[1].V0));
    }

    [TestMethod]
    public void WhenVertexCollectionIsEmptyThenConversionSucceeds()
    {
        Direct3D9ExpandedVertexConverter converter = CreateConverter(
            Direct3D9VertexFormatAttribute.Xy,
            Direct3D9VertexFormatAttribute.Xyz);

        int result = converter.ExpandVertices([]);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenFormatContainsUnsupportedAttributeThenCreationReturnsNotImplemented()
    {
        int result = Direct3D9ExpandedVertexConverter.Create(
            Direct3D9VertexFormatAttribute.Xy,
            Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Normal,
            Direct3D9VertexFormatAttribute.None,
            out Direct3D9ExpandedVertexConverter? converter);

        Assert.AreEqual((Direct3D9Factory.NotImplementedHResult, null), (result, converter));
    }

    [TestMethod]
    public void WhenGeneratedUvHasNoMappingThenFinalizeReturnsNotInitialized()
    {
        Assert.AreEqual(0, Direct3D9ExpandedVertexConverter.Create(
            Direct3D9VertexFormatAttribute.Xy,
            Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Uv1,
            Direct3D9VertexFormatAttribute.None,
            out Direct3D9ExpandedVertexConverter? converter));

        int result = converter!.FinalizeMappings();

        Assert.AreEqual(Direct3D9Factory.NotInitializedHResult, result);
    }

    [TestMethod]
    public void WhenComplexScanHasMultipleIntervalsThenDirectVerticesAreExpandedInOrder()
    {
        Direct3D9ExpandedVertexConverter converter = CreateCoverageConverter();
        List<Direct3D9ExpandedVertex[]> allocations = [];
        Direct3D9ComplexScanBuilder builder = CreateExpandedBuilder(converter, allocations, []);

        int result = builder.AddComplexScan(3,
        [
            new Direct3D9CoverageInterval(1, 16),
            new Direct3D9CoverageInterval(4, 32),
            new Direct3D9CoverageInterval(7, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Direct3D9ExpandedVertex[] vertices = allocations.Single();
        Assert.AreEqual((0, 4, 1.5f, 4.5f, 7.5f, 0x40404040u, 0x80808080u),
            (result, vertices.Length, vertices[0].X, vertices[1].X, vertices[3].X, vertices[0].Diffuse, vertices[3].Diffuse));
    }

    [TestMethod]
    public void WhenTopRowIsExpandedThenTriangleStripUsesFinalVertexMapping()
    {
        Direct3D9ExpandedVertexConverter converter = CreateCoverageConverter();
        List<Direct3D9ExpandedVertex[]> strips = [];
        Direct3D9ComplexScanBuilder builder = CreateExpandedBuilder(converter, [], strips, viewportTop: 2);

        int result = builder.AddComplexScan(2,
        [
            new Direct3D9CoverageInterval(1, 32),
            new Direct3D9CoverageInterval(4, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Direct3D9ExpandedVertex[] vertices = strips.Single();
        Assert.AreEqual((0, 6, 1f, 2f, 3f, 4f, 3f, 0x80808080u),
            (result, vertices.Length, vertices[0].X, vertices[0].Y, vertices[2].Y, vertices[5].X, vertices[5].Y, vertices[3].Diffuse));
    }

    [TestMethod]
    public void WhenWaffleSplitsLineThenEveryStripUsesFinalVertexMapping()
    {
        Direct3D9ExpandedVertexConverter converter = CreateCoverageConverter();
        List<Direct3D9ExpandedVertex[]> strips = [];
        Direct3D9ComplexScanBuilder builder = CreateExpandedBuilder(
            converter,
            [],
            strips,
            textureCoordinates: [new Direct3D9WaffleTextureCoordinate(new Matrix3x2(1, 0, 0, 4, 0, 0), Direct3D9WaffleMode.Enabled)]);

        int result = builder.AddComplexScan(1,
        [
            new Direct3D9CoverageInterval(0, 32),
            new Direct3D9CoverageInterval(3, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((0, 4, true), (result, strips.Count, strips.All(strip => strip.All(vertex => vertex.Diffuse == 0x80808080u))));
    }

    private static Direct3D9ExpandedVertexConverter CreateCoverageConverter()
    {
        return CreateConverter(
            Direct3D9VertexFormatAttribute.Xy,
            Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Diffuse,
            Direct3D9VertexFormatAttribute.Diffuse);
    }

    private static Direct3D9ExpandedVertexConverter CreateConverter(
        Direct3D9VertexFormatAttribute inputFormat,
        Direct3D9VertexFormatAttribute outputFormat,
        Direct3D9VertexFormatAttribute antiAliasScaleLocation = Direct3D9VertexFormatAttribute.None,
        Action<Direct3D9ExpandedVertexConverter>? configure = null)
    {
        Assert.AreEqual(0, Direct3D9ExpandedVertexConverter.Create(
            inputFormat,
            outputFormat,
            antiAliasScaleLocation,
            out Direct3D9ExpandedVertexConverter? converter));
        configure?.Invoke(converter!);
        Assert.AreEqual(0, converter!.FinalizeMappings());
        return converter;
    }

    private static Direct3D9ComplexScanBuilder CreateExpandedBuilder(
        Direct3D9ExpandedVertexConverter converter,
        List<Direct3D9ExpandedVertex[]> lineAllocations,
        List<Direct3D9ExpandedVertex[]> stripAllocations,
        float viewportTop = 0,
        IReadOnlyList<Direct3D9WaffleTextureCoordinate>? textureCoordinates = null)
    {
        return new Direct3D9ComplexScanBuilder(
            viewportTop,
            true,
            null,
            textureCoordinates ?? [],
            (_, _) => 0,
            converter,
            (int count, out Memory<Direct3D9ExpandedVertex> vertices) =>
            {
                Direct3D9ExpandedVertex[] allocation = new Direct3D9ExpandedVertex[count];
                lineAllocations.Add(allocation);
                vertices = allocation;
                return 0;
            },
            (int count, out Memory<Direct3D9ExpandedVertex> vertices) =>
            {
                Direct3D9ExpandedVertex[] allocation = new Direct3D9ExpandedVertex[count];
                stripAllocations.Add(allocation);
                vertices = allocation;
                return 0;
            });
    }
}
