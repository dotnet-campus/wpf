# 仓库目录整理

`OrganizeRepository.cs` 是 .NET 10 file-based app，无 csproj、无第三方依赖。根路径固定为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape` 及其内层 `WpfGfxShape` 产品目录。

**本次仅编写工具并编译验证，没有运行整理工具；现有文件尚未移动或删除。**

开发者日后执行方式（这里只是说明，不代表已执行）：`dotnet run --file WpfGfxShape/Tools/RepositoryOrganization/OrganizeRepository.cs`。仅编译方式：`dotnet build WpfGfxShape/Tools/RepositoryOrganization/OrganizeRepository.cs`。路径相对于外层根目录。源码声明关闭父级 Directory.Build.props/targets 导入；但本次专用构建日志证明未能阻止继承产品输出路径，不能宣称构建隔离有效。不改变仓库 SDK、TFM 或语言版本，使用仓库 global.json 指定的 .NET 10 SDK。在构建入口的隔离问题解决前，不应再次在此工作区构建或运行，以免构建过程触及 artifacts。

验证记录：专用构建退出码 0，首次版本有三个 CS8604 警告；随后将固定目标路径的父目录取得方式改为按清单使用的 `/` 分隔符切片，未加入防御检查。最终版本未再次构建，因为首次日志显示输出进入产品 artifacts。未读取、清理或移动该输出；未运行整理工具，也未创建真实 csproj 文件（日志中的 csproj 为 SDK 虚拟项目）。

## 固定移动范围

以下路径除标明外层外，均相对于内层产品根。源码列出确切执行顺序及文件名，不递归推断分类。

- `Docs/completed-work` → `Docs/history/completed-work`。
- 外层 `Docs/completed-work` → 内层 `Docs/history/completed-work/outer-root`，最后非递归删除已空的外层 `Docs`。外层文档全部归入产品，不保留第二套 Docs。
- `Docs/handoffs/completed-work` → `Docs/history/completed-work/handoffs-origin`。三个来源共同归入一个 completed-work 历史主题；来源子目录保留同名报告及各自 README，不覆盖、不按同名去重。例如内外两份 `replacement-independent-media-reader-control-integration-closeout.md` 均保留。
- `Docs/handoffs/history` → `Docs/history/handoffs`。
- `Docs/Docs/handoffs/history`、`Docs/Output/handoffs/history`、`Docs/handoffs/Output/handoffs/history` 分别 → `Docs/history/handoffs/{nested-docs-origin,maintenance-output-origin,handoff-output-origin}`。
- `Docs/handoffs/Input/next-session-handoff.md` → `Docs/history/handoffs/maintenance-input-handoff.md`。
- `Docs/Input` → `Docs/history/handoffs/maintenance-input-origin`。该目录实际含历史原文、审计记录及输入文档，全部移动保留，不当作无内容垃圾删除，不新建 Input 或 Fixtures。
- `Docs/handoffs` 中 `current-recovery.md`、`current-comparison-contract.md`、`current-validation-matrix.md`、`next-code-task.md`、`pcore-candidate-identity-minimal-fix-awaiting-authorization.md` → `Docs/current`。
- 源码 `specifications` 数组明确列出的 36 份规范 → `Docs/specifications`；包含仍有真实累计内容的 `wpfgfx-complete-replacement-validation-plan.md`，不把它误删为短指针。
- `Docs/history-index.md` → `Docs/history/history-index.md`；原 handoffs 剩余的历史回执、README、status-index 等整体 → `Docs/history/handoffs/receipts-origin`。
- `Tests/DocumentationMaintenance` → `Tools/DocumentationMaintenance`。其项目保留相同目录深度，所有文件原样移动。

`Docs/README.md`、`Docs/decisions`、`Docs/investigations`、既有 `Docs/history` 其余内容保持原位。Ledger 五份文件保持原位。正式 `WpfGfxShape.Tests`、`WpfGfxShape.ComAcceptance`、辅助 AcceptanceLifecycle 项目及其余实验执行器都留在 Tests：已发现它们存在 ProjectReference、Compile Link 等相对依赖，本次不为了分类制造更多悬空依赖。Code、solution、csproj 内容均不修改。

## 明确删除范围

仅删除以下已逐份读取、确认不再承载实际状态的短兼容指针：

- `Docs/next-session-handoff.md`
- `Docs/final-acceptance-remaining-matrix.md`
- `Docs/remaining-gap-closure-plan.md`
- `Docs/replacement-issues-and-plan-status.md`
- `Docs/handoffs/current-execution-rules.md`
- `Docs/handoffs/current-stable-baseline.md`
- `Docs/handoffs/current-work-item.md`

对应完整历史原文及审计记录保留并移动。没有对未审历史报告进行删除。

删除已枚举的误发布构建输出：`Docs/DocumentationMaintenance.pdb`，以及 `Docs/handoffs/DocumentationMaintenance` 的 `.deps.json`、`.dll`、`.exe`、`.pdb`、`.runtimeconfig.json` 六个实际文件合计（含前述根 PDB）。不把这些文件移回规范或历史文档目录。

非递归删除清空后的 `Docs/Docs/handoffs`、`Docs/Docs`、`Docs/Output/handoffs`、`Docs/Output`、`Docs/handoffs/Input`、`Docs/handoffs/Output/handoffs`、`Docs/handoffs/Output`；删除已枚举为空的产品根 `Input/handoffs/history`、`Input/handoffs`、`Input`。外层 Docs 最后非递归删除，遗漏内容会直接报错。

## 限制

这是按当前目录状态编写的一次性开发工具，不提供存在性检查、路径防御、覆盖、备份、确认、dry-run、异常吞咽或断点续跑。失败直接暴露原生异常，已完成的操作不会回滚。工具仅创建目标父目录、移动文件/目录、执行明确删除；不写 manifest、不计算 hash、不生成归档副本、不执行其他程序。artifacts 不在任何枚举、移动或删除操作范围内。

只移动、不修正文档内容或引用。因此 `Docs/README.md` 及规范、历史报告中的旧相对链接可能失效；源路径/审计字段也原样保留。DocumentationMaintenance 项目的 `../../Docs/<旧根文档名>`、`../../Docs/handoffs/...` Content 引用会因文档迁移或兼容页删除而失效，旧运行入口路径也改变；不承诺该维护项目迁移后仍可发布。项目自身同深度迁移并不能修复文档重排导致的引用失效。本次不运行该项目、不修 csproj/solution，不宣称整理后整个仓库可编译。
