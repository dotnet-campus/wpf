using System.Reflection;
using System.Runtime.CompilerServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
public sealed unsafe class BoundsCheckProtectionTests
{
    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(1, 0)]
    [DataRow(-1, 0)]
    [DataRow(2, 0)]
    public void WhenLargeValidShaderIsCompiledThenServicingCompatibleResourceSizeIsAccepted(int disabled, int expected)
    {
        var setter = typeof(BoundsCheckProtection).GetMethod("SetDisabled", BindingFlags.NonPublic | BindingFlags.Static)!;
        var set = (delegate* unmanaged[Stdcall]<int, void>)setter.MethodHandle.GetFunctionPointer();
        byte[] bytes = new byte[1024 * 1024 + 4];
        BitConverter.TryWriteBytes(bytes.AsSpan(), 0xffff0200u);
        BitConverter.TryWriteBytes(bytes.AsSpan(bytes.Length - 4), 0xffffu);
        try
        {
            set(disabled);
            Assert.AreEqual(expected, SoftwarePixelShaderProgram.Compile(bytes, out _));
        }
        finally { set(0); }
    }
}
