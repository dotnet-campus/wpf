namespace MS.Internal.Csp
{
    // Resolve the authority parser's unqualified Path without changing its source.
    internal static class Path
    {
        internal static string GetFullPath(string path) => System.IO.Path.GetFullPath(path);
    }
}
