using System.Numerics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed unsafe class DualFormatContractTests
{
    [TestMethod]
    public void WhenRotatedDrawingRequestsFloatThenIndependentAlphaRemainsUnquantized()
    {
        Vector4[] surface = RenderFloat();
        Assert.IsTrue(Vector4.Distance(new Vector4(0.13f, 0, 0, 0.13f), surface[10]) < 1e-7f);
    }

    [TestMethod]
    public void WhenRotatedDrawingRequestsBytesThenIndependentOriginalPixelRemains34()
    {
        byte[] surface = [];
        new CompositionTests().WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(
            0, false, 0, 0, curves: true, drawingImage: true, affineMode: 3,
            sourceMode: 18, tileMode: 1, sourceAlpha: 0.5f, floating: false,
            scalingMode: 2, consumerMode: 0, observeBgraSurface: pixels => surface = pixels);
        CollectionAssert.AreEqual(new byte[] { 0, 0, 34, 34 }, surface[40..44]);
    }

    [TestMethod]
    public void WhenSystemWicProjectsActualFloatPixelThenIndependentProjectionRemains33()
    {
        Vector4 pixel = RenderFloat()[10];
        using var wic = new SystemWicBitmap();
        nint factory = 0, source = 0, converter = 0;
        try
        {
            Guid clsid = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
            Guid iid = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
            Marshal.ThrowExceptionForHR(CoCreateInstance(&clsid, 0, 1, &iid, &factory));
            Guid floating = new("6fddc324-4e03-4bfe-b185-3d77768dc91a");
            Guid pbgra = new("6fddc324-4e03-4bfe-b185-3d77768dc910");
            Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, uint, uint, Guid*, uint, uint, byte*, nint*, int>)(*(void***)factory)[20])(
                factory, 1, 1, &floating, 16, 16, (byte*)&pixel, &source));
            Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)factory)[10])(factory, &converter));
            Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, nint, Guid*, uint, nint, double, uint, int>)(*(void***)converter)[8])(
                converter, source, &pbgra, 0, 0, 0, 0));
            byte[] bytes = new byte[4];
            fixed (byte* data = bytes)
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, void*, uint, uint, byte*, int>)(*(void***)converter)[7])(
                    converter, null, 4, 4, data));
            CollectionAssert.AreEqual(new byte[] { 0, 0, 33, 33 }, bytes);
        }
        finally
        {
            if (converter != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)converter)[2])(converter);
            if (source != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source)[2])(source);
            if (factory != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)factory)[2])(factory);
        }
    }

    private static Vector4[] RenderFloat()
    {
        Vector4[] surface = [];
        new CompositionTests().WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(
            0, false, 0, 0, curves: true, drawingImage: true, affineMode: 3,
            sourceMode: 18, tileMode: 1, sourceAlpha: 0.5f, floating: true,
            scalingMode: 2, consumerMode: 0, observeSurface: pixels => surface = pixels);
        return surface;
    }

    [DllImport("ole32.dll")]
    private static extern int CoCreateInstance(Guid* clsid, nint outer, uint context, Guid* iid, nint* instance);
}
