using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfGfxShape.ComAcceptance;

namespace NativeAcceptance.Pcore;

internal static class DesktopEntry
{
    internal static void Run()
    {
        var context = new DesktopContext();
        context.WriteLine("candidate-only desktop: original fixture initialization before first HWND; nativeControl remains mandatory; no independent continuation");
        D3DImageTests.StartPresentationThread(context);
        try
        {
            new D3DImageTests().WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive();
        }
        finally
        {
            D3DImageTests.StopPresentationThread();
            context.WriteLine("original fixture cleanup joined");
        }
    }

    private sealed class DesktopContext : TestContext
    {
        public override IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();
        public override void AddResultFile(string fileName) => WriteLine(fileName);
        public override void Write(string message) => Console.Write(message);
        public override void Write(string format, params object[] args) => Console.Write(format, args);
        public override void WriteLine(string message) => Console.WriteLine(message);
        public override void WriteLine(string format, params object[] args) => Console.WriteLine(format, args);
        public override void DisplayMessage(MessageLevel messageLevel, string message) => Console.WriteLine(message);
    }
}
