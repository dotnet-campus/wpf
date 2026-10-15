using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class FrozenVisualTreeTests
{
    [TestMethod]
    public void WhenVisualAlphaChangesThenCapturedTreeKeepsOldValue()
    {
        var visual = new GeneratedVisualResource();
        try
        {
            FrozenVisualTree.CapturePure(visual, [], out var snapshot);
            using (snapshot)
            {
                var packet = new byte[16];
                double alpha = .25;
                MemoryMarshal.Write(packet.AsSpan(8), in alpha);
                visual.ProcessCommand(new GeneratedProtocolHandleTable(), MilCommand.VisualSetAlpha, packet);
                Assert.AreEqual(1d, snapshot!.Alpha);
            }
        }
        finally { visual.Release(); }
    }

    [TestMethod]
    public void WhenNextTreeIsCapturedThenItUsesUpdatedAlpha()
    {
        var visual = new GeneratedVisualResource();
        try
        {
            var packet = new byte[16];
            double alpha = .25;
            MemoryMarshal.Write(packet.AsSpan(8), in alpha);
            visual.ProcessCommand(new GeneratedProtocolHandleTable(), MilCommand.VisualSetAlpha, packet);
            FrozenVisualTree.CapturePure(visual, [], out var snapshot);
            using (snapshot) Assert.AreEqual(.25, snapshot!.Alpha);
        }
        finally { visual.Release(); }
    }
}
