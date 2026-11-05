#:property TargetFramework=net10.0

using System.IO;

const string root = @"d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape";

Directory.CreateDirectory(Path.Combine(root, "Tests", "WpfGfxShape.ComAcceptance", "HarnessTests"));
File.Move(
    Path.Combine(root, "Tests", "WpfGfxShape.AcceptanceLifecycle.Tests", "DispatcherWorkersTests.cs"),
    Path.Combine(root, "Tests", "WpfGfxShape.ComAcceptance", "HarnessTests", "DispatcherWorkersTests.cs"));

Directory.Delete(Path.Combine(root, "Tests", "CandidateMediaCallbackR20"), true);
Directory.Delete(Path.Combine(root, "Tests", "CandidateMediaFailureR16"), true);
Directory.Delete(Path.Combine(root, "Tests", "CandidateResize"), true);
Directory.Delete(Path.Combine(root, "Tests", "CandidateResizeDesktop"), true);
Directory.Delete(Path.Combine(root, "Tests", "CandidateResizeDesktopSupervisor"), true);
Directory.Delete(Path.Combine(root, "Tests", "CandidateResizeSupervisor"), true);
Directory.Delete(Path.Combine(root, "Tests", "CandidateVideoSupervisor"), true);
Directory.Delete(Path.Combine(root, "Tests", "CurrentSourceAuditR15"), true);
Directory.Delete(Path.Combine(root, "Tests", "DesktopByteAuditR17"), true);
Directory.Delete(Path.Combine(root, "Tests", "DesktopObservationReadbackR16"), true);
Directory.Delete(Path.Combine(root, "Tests", "DesktopWgcR17"), true);
Directory.Delete(Path.Combine(root, "Tests", "StrictRenderReadbackR28"), true);
Directory.Delete(Path.Combine(root, "Tests", "SupervisedNativeIdentity"), true);
Directory.Delete(Path.Combine(root, "Tests", "WpfGfxShape.AcceptanceLifecycle.Tests"), true);

Directory.Delete(Path.Combine(root, "Tools", "RepositoryOrganization"), true);
