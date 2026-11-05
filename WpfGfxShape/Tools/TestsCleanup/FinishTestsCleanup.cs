#:property TargetFramework=net10.0

using System.IO;

const string root = @"d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape";

Directory.Delete(Path.Combine(root, "Tests", "StrictRenderR28"), true);
Directory.Delete(Path.Combine(root, "Tests", "StrictRenderSupervisorR28"), true);
Directory.Delete(Path.Combine(root, "Tests", "CandidatePureVideo"), true);
File.Delete(Path.Combine(root, "Tools", "TestsCleanup", "CleanupTests.cs"));
File.Delete(Path.Combine(root, "Tools", "TestsCleanup", "README.md"));
