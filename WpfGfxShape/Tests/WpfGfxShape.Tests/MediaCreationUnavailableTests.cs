using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class MediaCreationUnavailableTests
{
    private static delegate* unmanaged[Stdcall]<nint, nint, byte, nint*, int> FactoryExport()
    {
        Type type = typeof(Core.Direct3D9Factory).Assembly.GetType("WpfGfxShape.Abi.MediaExports", throwOnError: true)!;
        MethodInfo method = type.GetMethod("CreateMediaPlayer", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (delegate* unmanaged[Stdcall]<nint, nint, byte, nint*, int>)method.MethodHandle.GetFunctionPointer();
    }

    [DataTestMethod]
    [DataRow((byte)0)]
    [DataRow((byte)1)]
    public void WhenValidCreationIsRequestedThenReturnsNotImplementedWithoutRetainingClient(byte canOpen)
    {
        var create = FactoryExport();
        nint output = -1;

        // Non-null opaque inputs must remain untouched: no factory delegation,
        // event proxy AddRef, allocation or cleanup callback is needed on failure.
        int result = create(1, 1, canOpen, &output);

        Assert.AreEqual((unchecked((int)0x80004001), (nint)0), (result, output));
    }

    [TestMethod]
    public void WhenFactoryIsNullThenReturnsInvalidArgumentAndClearsOutput()
    {
        var create = FactoryExport();
        nint output = -1;

        int result = create(0, 1, 1, &output);

        Assert.AreEqual((unchecked((int)0x80070057), (nint)0), (result, output));
    }

    [TestMethod]
    public void WhenOutputIsNullThenReturnsPointerError()
    {
        var create = FactoryExport();

        int result = create(1, 1, 1, null);

        Assert.AreEqual(unchecked((int)0x80004003), result);
    }
}
