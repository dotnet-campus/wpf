#:property ImportDirectoryBuildProps=false
#:property ImportDirectoryBuildTargets=false
#:property PublishAot=false

using System.IO;

const string outerRoot = @"d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape";
const string productRoot = @"d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape";

// 先移走真实历史，再整理原交接目录，避免把误发布的执行器带进历史主题。
MoveDirectory("Docs/completed-work", "Docs/history/completed-work");
MoveDirectoryFromOuter("Docs/completed-work", "Docs/history/completed-work/outer-root");
MoveDirectory("Docs/handoffs/completed-work", "Docs/history/completed-work/handoffs-origin");
MoveDirectory("Docs/handoffs/history", "Docs/history/handoffs");
MoveDirectory("Docs/Docs/handoffs/history", "Docs/history/handoffs/nested-docs-origin");
MoveDirectory("Docs/Output/handoffs/history", "Docs/history/handoffs/maintenance-output-origin");
MoveDirectory("Docs/handoffs/Output/handoffs/history", "Docs/history/handoffs/handoff-output-origin");
MoveFile("Docs/handoffs/Input/next-session-handoff.md", "Docs/history/handoffs/maintenance-input-handoff.md");

// Input 含真实原文及审计，不按名称推断为可删除副本，完整移动并保留来源。
MoveDirectory("Docs/Input", "Docs/history/handoffs/maintenance-input-origin");

string[] currentDocuments =
[
    "current-recovery.md",
    "current-comparison-contract.md",
    "current-validation-matrix.md",
    "next-code-task.md",
    "pcore-candidate-identity-minimal-fix-awaiting-authorization.md"
];
foreach (string name in currentDocuments)
{
    MoveFile($"Docs/handoffs/{name}", $"Docs/current/{name}");
}

// 已逐份读取：这些短页仅为兼容指针，完整历史原文在上面移动的 history 中。
string[] obsoletePointers =
[
    "Docs/next-session-handoff.md",
    "Docs/final-acceptance-remaining-matrix.md",
    "Docs/remaining-gap-closure-plan.md",
    "Docs/replacement-issues-and-plan-status.md",
    "Docs/handoffs/current-execution-rules.md",
    "Docs/handoffs/current-stable-baseline.md",
    "Docs/handoffs/current-work-item.md"
];
foreach (string path in obsoletePointers)
{
    File.Delete(Path.Combine(productRoot, path));
}

string[] misplacedBuildOutputs =
[
    "Docs/DocumentationMaintenance.pdb",
    "Docs/handoffs/DocumentationMaintenance.deps.json",
    "Docs/handoffs/DocumentationMaintenance.dll",
    "Docs/handoffs/DocumentationMaintenance.exe",
    "Docs/handoffs/DocumentationMaintenance.pdb",
    "Docs/handoffs/DocumentationMaintenance.runtimeconfig.json"
];
foreach (string path in misplacedBuildOutputs)
{
    File.Delete(Path.Combine(productRoot, path));
}

string[] specifications =
[
    "00-migration-charter.md",
    "01-native-source-topology.md",
    "02-abi-and-managed-callers.md",
    "03-migration-order.md",
    "04-nativeaot-project-shape.md",
    "05-silknet-directx-assessment.md",
    "06-testing-strategy.md",
    "07-scope-and-ledger-schema.md",
    "08-abi-manifest-spec.md",
    "09-test-evidence-and-e2e-contract.md",
    "10-session-continuity-protocol.md",
    "abi-entry-and-type-schema.md",
    "abi-lifetime-binary-and-gates.md",
    "all-completion-external-prerequisites.md",
    "charter-evidence-and-continuity.md",
    "charter-implementation-contract.md",
    "com-acceptance-sandbox-operations.md",
    "com-nativeaot-testing-method.md",
    "continuity-failures-and-decisions.md",
    "continuity-recovery-and-closeout.md",
    "e2e-executable-contracts.md",
    "evidence-capabilities-and-failures.md",
    "evidence-run-contract.md",
    "governance-authority-traceability.md",
    "ledger-records-and-acceptance.md",
    "ledger-scope-and-identity.md",
    "migration-coupled-domains.md",
    "migration-implementation-gates.md",
    "native-to-managed-file-map.md",
    "plan-module-contracts.md",
    "protection-api-contract.md",
    "publicabi-family-delivery-plan.md",
    "testing-comparison-and-acceptance.md",
    "testing-levels-and-gates.md",
    "windowsdesktop-version-provenance.md",
    "wpfgfx-complete-replacement-validation-plan.md"
];
foreach (string name in specifications)
{
    MoveFile($"Docs/{name}", $"Docs/specifications/{name}");
}
MoveFile("Docs/history-index.md", "Docs/history/history-index.md");

// 非递归删除已枚举的空壳；遗漏内容会由 Directory.Delete 原生异常暴露。
string[] emptyDirectories =
[
    "Docs/Docs/handoffs",
    "Docs/Docs",
    "Docs/Output/handoffs",
    "Docs/Output",
    "Docs/handoffs/Input",
    "Docs/handoffs/Output/handoffs",
    "Docs/handoffs/Output",
    "Input/handoffs/history",
    "Input/handoffs",
    "Input"
];
foreach (string path in emptyDirectories)
{
    Directory.Delete(Path.Combine(productRoot, path));
}

MoveDirectory("Docs/handoffs", "Docs/history/handoffs/receipts-origin");
MoveDirectory("Tests/DocumentationMaintenance", "Tools/DocumentationMaintenance");
Directory.Delete(Path.Combine(outerRoot, "Docs"));

static void MoveFile(string source, string destination)
{
    string target = Path.Combine(productRoot, destination);
    Directory.CreateDirectory(target[..target.LastIndexOf('/')]);
    File.Move(Path.Combine(productRoot, source), target);
}

static void MoveDirectory(string source, string destination)
{
    string target = Path.Combine(productRoot, destination);
    Directory.CreateDirectory(target[..target.LastIndexOf('/')]);
    Directory.Move(Path.Combine(productRoot, source), target);
}

static void MoveDirectoryFromOuter(string source, string destination)
{
    string target = Path.Combine(productRoot, destination);
    Directory.CreateDirectory(target[..target.LastIndexOf('/')]);
    Directory.Move(Path.Combine(outerRoot, source), target);
}
