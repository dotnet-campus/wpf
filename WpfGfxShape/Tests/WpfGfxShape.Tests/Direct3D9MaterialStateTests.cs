using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9MaterialStateTests
{
    [TestMethod]
    public void WhenInitializingDefaultMaterialThenNativeValuesAreUsed()
    {
        Material9 actual = default;

        int result = Direct3D9MaterialState.InitializeDefaultMaterial(material =>
        {
            actual = material;
            return 0;
        });

        Assert.AreEqual(0, result);
        Assert.AreEqual(1.0f, actual.Diffuse.R);
        Assert.AreEqual(1.0f, actual.Diffuse.G);
        Assert.AreEqual(1.0f, actual.Diffuse.B);
        Assert.AreEqual(1.0f, actual.Diffuse.A);
        Assert.AreEqual(0.0f, actual.Specular.R);
        Assert.AreEqual(0.0f, actual.Specular.G);
        Assert.AreEqual(0.0f, actual.Specular.B);
        Assert.AreEqual(0.0f, actual.Specular.A);
        Assert.AreEqual(0.0f, actual.Ambient.R);
        Assert.AreEqual(0.0f, actual.Ambient.G);
        Assert.AreEqual(0.0f, actual.Ambient.B);
        Assert.AreEqual(0.0f, actual.Ambient.A);
        Assert.AreEqual(0.0f, actual.Emissive.R);
        Assert.AreEqual(0.0f, actual.Emissive.G);
        Assert.AreEqual(0.0f, actual.Emissive.B);
        Assert.AreEqual(0.0f, actual.Emissive.A);
        Assert.AreEqual(40.0f, actual.Power);
    }

    [TestMethod]
    public void WhenSettingDefaultMaterialFailsThenFailureIsReturned()
    {
        int result = Direct3D9MaterialState.InitializeDefaultMaterial(
            _ => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }
}
