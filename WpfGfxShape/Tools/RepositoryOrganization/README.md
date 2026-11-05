# 仓库目录整理

`OrganizeRepository.cs` 是 .NET 10 file-based app，无 csproj、无第三方依赖。根路径固定为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape` 及其内层 `WpfGfxShape` 产品目录。

**用户已执行整理工具。迁后检查确认固定移动清单的目标均存在、旧入口及外层 Docs 已消失，未发现中断位置；不是历史内容逐字节完整性认证。不得再次运行。**

这是固定 Move/Delete 的一次性、非幂等工具，不可重入，不提供恢复或防御检查；失败不回滚。保留源码用于说明实际清单，不再作为迁后操作入口。仅编译可使用专用构建能力，目标为 `WpfGfxShape/Tools/RepositoryOrganization/OrganizeRepository.cs`（相对外层根）；构建不执行整理逻辑。

最终源码已用专用单文件构建复验：退出码 0，日志无 CS8604 警告，确认 `/` 分隔符切片修复可编译。保持现有 .NET 10 SDK、TFM 与输出机制，正常构建产物不构成违规；artifacts 完全不纳入本次盘点、移动、清理或配置修改。详细迁后检查见[当前维护检查](../../Docs/current/repository-organization-check.md)。

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

`Docs/README.md`、`Docs/decisions`、`Docs/investigations`、既有 `Docs/history` 其余内容保持原位。Ledger 五份文件保持原位。正式 `WpfGfxShape.Tests`、`WpfGfxShape.ComAcceptance`、辅助 AcceptanceLifecycle 项目及其余实验执行器都留在 Tests：已发现它们存在 ProjectReference、Compile Link 等相对依赖，本次不为了分类制造更多悬空依赖。整理工具本身不修改 Code、solution、csproj 内容；后续维护工程路径修复见末节。

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

整理工具本身只移动、不修正文档内容或引用。后续维护已修复当前入口和部分已读规范导航，并将 DocumentationMaintenance 的 Content 与验证源码改为迁后真实路径；其 verify 只检查保留归档与原审计，不再验证已删除兼容页，archive/pointer 已退出迁后入口。inventory 是旧阶段辅助，不在本任务运行。历史原文及审计字段原样保留，其内部链接按历史原基准解释。

旧当前维护台账缺失，迁后 Input 来源副本读取为空，不能伪造恢复。本次以短检查页记录范围，不宣称全库链接清零、历史完整复核或整个仓库可编译。solution 未引用维护工程，Tests 内未搜到该工程引用，无需修改 solution/ProjectReference。
