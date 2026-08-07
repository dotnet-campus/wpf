using WpfGfxShape.Abi;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class NativeAotAbiProbeTests
{
    [TestMethod]
    public void WhenOperationIsChecksumThenReturnsExpectedOutput()
    {
        const ulong input = 0x1122334455667788UL;
        ulong contextValue = IntPtr.Size == 8
            ? 0x8877665544332211UL
            : 0x44332211U;
        nuint context = checked((nuint) contextValue);
        ulong output = 0;

        int result = NativeAotAbiProbe.Invoke(
            NativeAotAbiProbe.SupportedAbiVersion,
            0,
            input,
            context,
            &output);

        Assert.AreEqual(NativeAotAbiProbe.S_OK, result);
        Assert.AreEqual(NativeAotAbiProbe.CalculateChecksum(input, context), output);
    }

    [TestMethod]
    public void WhenOutputIsNullThenReturnsPointerError()
    {
        int result = NativeAotAbiProbe.Invoke(
            NativeAotAbiProbe.SupportedAbiVersion,
            0,
            0,
            0,
            null);

        Assert.AreEqual(NativeAotAbiProbe.E_POINTER, result);
    }

    [TestMethod]
    public void WhenAbiVersionIsUnsupportedThenClearsOutput()
    {
        ulong output = ulong.MaxValue;

        NativeAotAbiProbe.Invoke(2, 0, 0, 0, &output);

        Assert.AreEqual(0UL, output);
    }

    [TestMethod]
    public void WhenAbiVersionIsUnsupportedThenReturnsInvalidArgument()
    {
        ulong output = ulong.MaxValue;

        int result = NativeAotAbiProbe.Invoke(2, 0, 0, 0, &output);

        Assert.AreEqual(NativeAotAbiProbe.E_INVALIDARG, result);
    }

    [TestMethod]
    public void WhenOperationIsControlledArgumentFailureThenReturnsInvalidArgument()
    {
        ulong output = ulong.MaxValue;

        int result = NativeAotAbiProbe.Invoke(
            NativeAotAbiProbe.SupportedAbiVersion,
            1,
            0,
            0,
            &output);

        Assert.AreEqual(NativeAotAbiProbe.E_INVALIDARG, result);
    }

    [TestMethod]
    public void WhenOperationThrowsThenReturnsFailure()
    {
        ulong output = ulong.MaxValue;

        int result = NativeAotAbiProbe.Invoke(
            NativeAotAbiProbe.SupportedAbiVersion,
            2,
            0,
            0,
            &output);

        Assert.AreEqual(NativeAotAbiProbe.E_FAIL, result);
    }

    [TestMethod]
    public void WhenOperationThrowsThenClearsOutput()
    {
        ulong output = ulong.MaxValue;

        NativeAotAbiProbe.Invoke(
            NativeAotAbiProbe.SupportedAbiVersion,
            2,
            0,
            0,
            &output);

        Assert.AreEqual(0UL, output);
    }

    [TestMethod]
    public void WhenOperationIsUnknownThenReturnsNotImplemented()
    {
        ulong output = ulong.MaxValue;

        int result = NativeAotAbiProbe.Invoke(
            NativeAotAbiProbe.SupportedAbiVersion,
            uint.MaxValue,
            0,
            0,
            &output);

        Assert.AreEqual(NativeAotAbiProbe.E_NOTIMPL, result);
    }

    [TestMethod]
    public void WhenOperationIsUnknownThenClearsOutput()
    {
        ulong output = ulong.MaxValue;

        NativeAotAbiProbe.Invoke(
            NativeAotAbiProbe.SupportedAbiVersion,
            uint.MaxValue,
            0,
            0,
            &output);

        Assert.AreEqual(0UL, output);
    }

    [TestMethod]
    public void WhenChecksumRunsAfterControlledExceptionThenItStillSucceeds()
    {
        ulong output = ulong.MaxValue;
        NativeAotAbiProbe.Invoke(
            NativeAotAbiProbe.SupportedAbiVersion,
            2,
            0,
            0,
            &output);

        int result = NativeAotAbiProbe.Invoke(
            NativeAotAbiProbe.SupportedAbiVersion,
            0,
            42,
            7,
            &output);

        Assert.AreEqual(NativeAotAbiProbe.S_OK, result);
    }
}
