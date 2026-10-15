# COM 标准 MTP 小包：发布、传输与验收

> 路径和发布命令相对于 `WpfGfxShape/`。以下仅文档命令，本轮未执行终端或脚本，也未重新发布。唯一测试项目/标准 MTP exe，不是自建 Host。

## 发布与输入边界

1. 生产专用发布：`dotnet publish Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。产物 `artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
2. 沙箱包专用发布：`dotnet publish Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`。包目标在 Build 后清理并重建 `artifacts/com-acceptance-package`，只复制列明运行依赖、ReferenceData、Native DLL，生成 SHA 和清单。
3. 只上传 **`artifacts/com-acceptance-package`**。不得上传项目根、publish 父目录、obj、历史源码或 PDB。包为框架依赖 win-x64 应用，远端需 .NET 10 Windows Desktop runtime；本次运行输出为 .NET 10.0.1。
4. 初始包必须为25文件、仅 Native/ReferenceData 两子目录，清单逐项见独立归档。运行新增 Results/TestResults 只属于回收证据，不能混回下一次初始上传包。

## 本机标准 MTP Test

工作目录必须为 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/`，使项目级 global.json 的 MTP runner 被发现：

- 全量：`dotnet test --project WpfGfxShape.ComAcceptance.csproj -c Release -- --report-trx --report-trx-filename local-final-full.trx`。
- 定向：`dotnet test --project WpfGfxShape.ComAcceptance.csproj -c Release -- --filter FullyQualifiedName~AcceptancePathsTests --report-trx --report-trx-filename local-paths.trx`。

这些是标准 MTP 命令说明，并非本轮成功执行记录。本轮从工作区根通过专用测试入口传 csproj 的定向调用仍失败于 VSTest target（退出1、测试未启动）。根目录 global.json 不选 MTP，项目路径不会切换 SDK 查找起点。项目级文件有必要承载局部 opt-in，但其 SDK 搜索路径未与根完全一致；保留不改，不声称 SDK 实际选择或专用入口兼容已经验证。不要改根 runner 影响其他测试项目，也不要新增调度层。

## 沙箱直接 exe（无需 SDK runner/Host）

以包根为执行目录，执行文件相对路径 `WpfGfxShape.ComAcceptance.exe`；沙箱专用执行能力的 sourceDirectory 仅设包目录。

- 全量参数：`--report-trx --report-trx-filename sandbox-full.trx`。
- 路径/身份/白名单定向参数：`--filter FullyQualifiedName~AcceptancePathsTests --report-trx --report-trx-filename closure-paths.trx`（本轮已实际执行10/10、零跳过）。
- 40项 smoke 可复用参数：`--filter FullyQualifiedName~EventProxyTests|FullyQualifiedName~AcceptancePathsTests --report-trx --report-trx-filename sandbox-smoke.trx`。参数应作为数组逐项传递，其中 filter 表达式为一个完整参数，不经 Shell；该40项组合来自原 smoke TRX 的类集合，本轮未重跑该组合。

保留退出码；全量失败退出2不能转换成功。完整回收任务目录到独立 `artifacts/sandbox-readback/<run>/`，至少保存 `TestResults/*.trx`、`Results/aot-identity.json`、校准 JSON、aot.sha256 及实际包清单。TRX 在 TestResults，业务输出在 Results；不要只回收其中一类。每轮用独立命名避免覆盖已归档证据。

## 历史工程验收快照（不是当前运行状态）

以下仅为小包工程交付当时的记录；当前状态只见[基线](handoffs/current-stable-baseline.md)。本节不再更新或用于领取任务。

SHA `271E8FB769FD76A9139A3C71A2B2F42C1E471B46358E569726481D8A90026441`。

- final-smoke/TestResults/sandbox-smoke.trx：RunID `2d934d81-5b33-4cc3-8f98-0c2cd9bc2f44`，40/40。
- final-full/TestResults/sandbox-full.trx：RunID `4d0727bd-1b99-403f-bd1f-616d107e0035`，1343 pass/22 fail/1365 executed，零跳过，退出2。
- 上述目录均位于 `artifacts/sandbox-readback/`，Results 身份同 SHA、Sandbox=true。
- 本机 local-final-full.trx：RunID `bcd5487d-1450-42a6-826f-346ebbd3a8f8`，1365/1365，TRX 日志同 SHA。

历史快照证明当时包可传输、启动、执行，不能将其中失败套作当前失败集合，也不能把窗口覆盖观察推广为全部仅环境。历史索引见[隔离层](history-index.md)，现行恢复见[唯一入口](next-session-handoff.md)。
