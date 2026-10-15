using System.Numerics;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class GeneratedShaderInputScopeTests
{
    private static MilShaderEffectCommand Command(double top = 0, double bottom = 0,
        double left = 0, double right = 0) => new(MilCommand.ShaderEffect, 1,
        top, bottom, left, right, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    [TestMethod]
    public void WhenPaddingIsScaledThenIntermediateUsesLocalContentBounds()
    {
        int result = GeneratedShaderInputScope.TryCreate(new(10, 20, 4, 6),
            Command(1, 2, 3, 4), new(2, 3, 100, 200), out var scope);
        Assert.AreEqual((0, 22u, 27u, new GeneratedImageTransform(2, 3, -14, -57)),
            (result, scope.Width, scope.Height, scope.InputTransform));
    }

    [TestMethod]
    public void WhenInputOriginIsShiftedThenOutputRestoresDevicePosition()
    {
        _ = GeneratedShaderInputScope.TryCreate(new(10, 20, 4, 6),
            Command(1, 2, 3, 4), new(2, 3, 100, 200), out var scope);
        Assert.AreEqual((114.0, 257.0), scope.OutputTransform.Apply(0, 0));
    }

    [TestMethod]
    public void WhenTransformSwapsAxesThenUvDerivativesFollowInverseMatrix()
    {
        _ = GeneratedShaderInputScope.TryCreate(new(0, 0, 4, 6), Command(),
            new(0, 0, 10, 20, 2, -3), out var scope);
        Assert.AreEqual(new Vector4(0, -1f / 18, 1f / 8, 0), scope.UvDerivatives);
    }

    [TestMethod]
    public void WhenTransformIsSingularThenScopeFailsWithoutOutput()
    {
        int result = GeneratedShaderInputScope.TryCreate(new(0, 0, 4, 6), Command(),
            new(1, 1, 0, 0, 1, 1), out var scope);
        Assert.AreEqual((Direct3D9Factory.NonInvertibleMatrixHResult, default(GeneratedShaderInputScope)),
            (result, scope));
    }

    [TestMethod]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(-1.0)]
    public void WhenPaddingIsInvalidThenScopeRejectsIt(double padding)
    {
        int result = GeneratedShaderInputScope.TryCreate(new(0, 0, 4, 6), Command(left: padding),
            GeneratedImageTransform.Identity, out _);
        Assert.AreEqual(Direct3D9Factory.InvalidCallHResult, result);
    }
}
