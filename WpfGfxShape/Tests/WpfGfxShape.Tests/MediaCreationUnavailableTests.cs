using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Abi;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class MediaCreationUnavailableTests
{
    private static delegate* unmanaged[Stdcall]<nint, nint, byte, nint*, int> FactoryExport()
        => (delegate* unmanaged[Stdcall]<nint, nint, byte, nint*, int>)typeof(MediaExports)
            .GetMethod("CreateMediaPlayer", BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();

    [DataTestMethod]
    [DataRow((byte)0)]
    [DataRow((byte)1)]
    public void WhenValidCreationIsRequestedThenIndependentIdentityCanCloseAndRelease(byte canOpen)
    {
        nint factory = 0, client = 0, output = 0;
        var createFactory = (delegate* unmanaged[Stdcall]<nint*, uint, int>)typeof(BitmapRenderTargetExports)
            .GetMethod("CreateFactory", BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();
        var createProxy = (delegate* unmanaged[Stdcall]<EventProxyDescriptor*, nint*, int>)typeof(EventProxyExports)
            .GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();
        EventProxyDescriptor descriptor = new() { Dispose = &Dispose, RaiseEvent = &Receive };
        try
        {
            Assert.AreEqual(0, createFactory(&factory, 0x200184C0));
            Assert.AreEqual(0, createProxy(&descriptor, &client));
            Assert.AreEqual(0, FactoryExport()(factory, client, canOpen, &output));
            Assert.IsNotNull(SystemMediaSource.Resolve(output));
            Assert.IsTrue(IndependentMediaIdentity.Resolve(output)!.RequireVideoOnly);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)output)[5])(output));
        }
        finally
        {
            if (output != 0) Assert.AreEqual(0u, Release(output));
            if (client != 0) Assert.AreEqual(0u, Release(client));
            if (factory != 0) Release(factory);
        }
    }

    [TestMethod]
    public void WhenFactoryIsNullThenReturnsInvalidArgumentAndClearsOutput()
    {
        nint output = -1;
        int result = FactoryExport()(0, 0, 1, &output);
        Assert.AreEqual((unchecked((int)0x80070057), (nint)0), (result, output));
    }

    [TestMethod]
    public void WhenOutputIsNullThenReturnsPointerError()
        => Assert.AreEqual(unchecked((int)0x80004003), FactoryExport()(0, 0, 1, null));

    [DataTestMethod]
    [DataRow((byte)0)]
    [DataRow((byte)1)]
    [DataRow((byte)2)]
    public void WhenClientIsNullThenReturnsInvalidArgumentAndClearsOutput(byte canOpen)
    {
        nint output = -1;
        int result = FactoryExport()(1, 0, canOpen, &output);
        Assert.AreEqual((unchecked((int)0x80070057), (nint)0), (result, output));
    }

    [DataTestMethod]
    [DataRow(9, 0.0)]
    [DataRow(9, 0.75)]
    [DataRow(9, 1.0)]
    [DataRow(10, -1.0)]
    [DataRow(10, 0.25)]
    [DataRow(10, 1.0)]
    public void WhenUnopenedThenFiniteAudioTargetIsStored(int slot, double value)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, double, int>)(*(nint**)media.Pointer)[slot])(media.Pointer, value));
            Assert.AreEqual(value, slot == 9 ? media.TargetVolume : media.TargetBalance);
        }
        finally { Release(media.Pointer); }
    }

    [DataTestMethod]
    [DataRow(9, double.NaN)]
    [DataRow(9, double.PositiveInfinity)]
    [DataRow(9, -0.1)]
    [DataRow(9, 1.1)]
    [DataRow(10, double.NegativeInfinity)]
    [DataRow(10, double.NaN)]
    [DataRow(10, -1.1)]
    [DataRow(10, 1.1)]
    public void WhenAudioTargetIsInvalidThenRejectWithoutMutation(int slot, double value)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            int hr = ((delegate* unmanaged[Stdcall]<nint, double, int>)(*(nint**)media.Pointer)[slot])(media.Pointer, value);
            Assert.AreEqual((unchecked((int)0x80070057), 0.5, 0.0), (hr, media.TargetVolume, media.TargetBalance));
        }
        finally { Release(media.Pointer); }
    }

    private static uint Release(nint value) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)value)[2])(value);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Dispose(EventProxyDescriptor* descriptor) { }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Receive(EventProxyDescriptor* descriptor, byte* data, uint size) => 0;
}
