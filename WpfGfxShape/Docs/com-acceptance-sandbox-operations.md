# COM 标准 MTP 小包：发布、传输与验收

> 路径和发布命令相对于 `WpfGfxShape/`。以下仅文档命令，本轮未执行终端或脚本，也未重新发布。唯一测试项目/标准 MTP exe，不是自建 Host。

## 发布与输入边界

1. 生产专用发布：`dotnet publish Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。产物 `artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
2. 沙箱包专用发布：`dotnet publish Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`。包目标在 Build 后清理并重建 `artifacts/com-acceptance-package`，只复制列明运行依赖、ReferenceData、Native DLL，生成 SHA 和清单。
3. 只上传 **`artifacts/com-acceptance-package`**。不得上传项目根、publish 父目录、obj、历史源码或 PDB。包为框架依赖 win-x64 应用，远端需 .NET 10 Windows Desktop runtime；实际 runtime 版本须在每次原始结果记录，不沿用历史版本。
4. 初始包仅包含显式白名单运行依赖与 Native/ReferenceData 输入；文件清单及 SHA 须按实际包核对，不把历史文件数量当永久分母。运行新增 Results/TestResults 只属于回收证据，不能混回下一次初始上传包。

## 本机标准 MTP Test

工作目录必须为 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/`，使项目级 global.json 的 MTP runner 被发现：

- 全量：`dotnet test --project WpfGfxShape.ComAcceptance.csproj -c Release -- --report-trx --report-trx-filename local-final-full.trx`。
- 定向：`dotnet test --project WpfGfxShape.ComAcceptance.csproj -c Release -- --filter FullyQualifiedName~AcceptancePathsTests --report-trx --report-trx-filename local-paths.trx`。

上述是参数说明，不授权终端调用；执行代理使用专用测试能力或下述标准编译应用沙箱入口。历史从根目录选错 runner 的失败见[冻结工程档](handoffs/completed-work/S-UNDATED-001-SANDBOX-PACKAGE-FINAL-CLOSEOUT.md)，本轮未验证 SDK 搜索路径/专用入口兼容。不要改根 runner 影响其他项目，也不要新增调度层。

## 沙箱直接 exe（无需 SDK runner/Host）

以包根为执行目录，执行文件相对路径 `WpfGfxShape.ComAcceptance.exe`；沙箱专用执行能力的 sourceDirectory 仅设包目录。不得改为历史 `WpfGfxShape.NativeCaller.exe` / `StaticNativeCaller.exe`，也不得上传 `.coding-agent/sandbox-results` 回收目录作为现行包。历史 caller 的启动/CRT 诊断不构成生产验收；定位结果见[独立报告](../Investigations/NativeCallerStartup/README.md)。

- 全量参数：`--report-trx --report-trx-filename sandbox-full.trx`。
- 路径/身份/白名单定向参数：`--filter FullyQualifiedName~AcceptancePathsTests --report-trx --report-trx-filename closure-paths.trx`。
- smoke 参数示例：`--filter FullyQualifiedName~EventProxyTests|FullyQualifiedName~AcceptancePathsTests --report-trx --report-trx-filename sandbox-smoke.trx`。参数按数组逐项传递，filter 表达式为一个完整参数，不经 Shell；实际用例分母按原始 TRX 记录。

保留退出码；全量失败退出2不能转换成功。完整回收任务目录到独立 `artifacts/sandbox-readback/<run>/`，至少保存 `TestResults/*.trx`、`Results/aot-identity.json`、校准 JSON、aot.sha256 及实际包清单。TRX 在 TestResults，业务输出在 Results；不要只回收其中一类。每轮用独立命名避免覆盖已归档证据。

## 证据与历史边界

当前状态只见[基线](handoffs/current-stable-baseline.md)。小包工程旧 SHA、smoke/full、runner 失败及白名单逐项证据保留于[冻结工程档](handoffs/completed-work/S-UNDATED-001-SANDBOX-PACKAGE-FINAL-CLOSEOUT.md)，不在方法页复制计数。
旧失败集合不能套作当前失败，也不能把窗口覆盖观察推广为全部仅环境。历史索引见[隔离层](history-index.md)，现行恢复见[唯一入口](next-session-handoff.md)。
