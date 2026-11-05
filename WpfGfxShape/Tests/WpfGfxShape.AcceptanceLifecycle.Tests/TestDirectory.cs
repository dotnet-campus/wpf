using System.Runtime.CompilerServices;

namespace WpfGfxShape.AcceptanceLifecycle.Tests;

internal static class TestDirectory
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
    }
}
