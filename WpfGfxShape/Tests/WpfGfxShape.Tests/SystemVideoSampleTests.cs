using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
public sealed unsafe class SystemVideoSampleTests
{
    [TestMethod]
    public void WhenMediaIsNullThenSampleAcquisitionReturnsInvalidArgument()
    {
        Assert.AreEqual(unchecked((int)0x80070057), SystemVideoSample.Acquire(0, out _));
    }

}
