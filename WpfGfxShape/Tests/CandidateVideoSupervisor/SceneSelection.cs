internal static class SceneSelection
{
#if GEOMETRY_R27
    internal const string Name = "geometry-public-r27";
    internal const string Completed = "geometry-public-r27-completed";
#else
    internal const string Name = "purevideo-public";
    internal const string Completed = "purevideo-completed";
#endif
}
