# 历史 NativeCaller 启动诊断（2026-10-16）

## 裁决与缺口

用户截图 `WpfGfxShape.NativeCaller.exe` 缺 `MSVCP140D.dll` 属 Windows loader/Debug CRT 依赖启动故障，不是 AOT 功能 Red。**原故障 EXE、上传调用记录、vcxproj/源文件均未找到，不能声称原故障配置已修复。** 工作区 `Tests/NativeCaller` 只有旧 README；其 NativeCaller/StaticNativeCaller/Run/Publish 项目描述已失效。本轮添加历史警示，保留旧证据，现行 COM 操作页明确禁止用旧 caller/回收目录替代当前入口。未发现当前生产项目或包目标引用旧 caller，因此没有可纠正的当前代码调用点。

正式范围仍仅 x86/x64；ARM64 按 DEC-0005 N/A。DLL 驻留、无卸载（DEC-0004）。本轮没有生产源码/工具链变更或生产 publish，不改变全局门禁。

## 实物 PE 证据

留存 EXE 位于工作区根 `.coding-agent/sandbox-results/connectivity-retry-release/` 与 `connectivity-retry-release-control/`（均非现行包）。两个目录对应文件 SHA 完全相同：

| 实物 | SHA256 | 结果 |
|---|---|---|
| WpfGfxShape.NativeCaller.exe | 2B5CE38BE9A1D71627E0494191FF798B3FA35F2AF5C063D7EE9A007443D4E5C2 | AMD64 / PE32+ |
| static/WpfGfxShape.StaticNativeCaller.exe | 00907DE6FB18B13D2EA381BA863023B6FB8E6F65546F0BF3FB7030D40BE740D3 | AMD64 / PE32+ |

两个 caller 的直接 imports 都包含 `MSVCP140.dll`、`VCRUNTIME140.dll`、`VCRUNTIME140_1.dll` 及正式 UCRT API-set；没有 delay imports，没有 `MSVCP140D.dll` / `VCRUNTIME140D.dll` / `ucrtbased.dll`。StaticNativeCaller 还直接 import `wpfgfx_cor3.dll`；“static”表示 import-library 链接，并非 CRT /MT 自足。目录名 Release 不证明 Configuration/RuntimeLibrary；实际依赖是正式动态 CRT，但 vcxproj 缺失，不能证明原 /MD、/MT、Debug/Release 或 x86 配置。这些留存物不能直接解释截图的 Debug CRT 依赖，截图可能来自另一 EXE 或其被加载模块；无证据确定来源。

现行小包 `WpfGfxShape.ComAcceptance.exe` SHA `D9F429C270000531E9A77B2CAB6BDCF59D4AD40DFBFE14E8D263763AC9D7C08D`，生产 DLL SHA `3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D`；均 AMD64/PE32+，直接/延迟 imports 无上述调试 CRT，生产身份未变。

原始完整 imports/SHA 报告（相对于 WpfGfxShape）：`artifacts/sandbox-readback/nativecaller-pe-audit-final/Results/pe-imports.json`，RunUtc `2026-10-16T06:38:22.6414828+00:00`，已完整读回 132 行。`PeAudit` 只读取 PE，不加载历史 EXE 或 DLL。自包含 Release win-x64 发布成功；首次 MSBuild 因未 restore NETSDK1004 失败，专用 publish restore 后 Release MSBuild 成功，0 警告/0 错误。新增 control 输入后重新 Release 发布并执行最终报告成功，未留未测源码尾改。

## 实际沙箱执行

- `StartupPackage.proj` 以 Release 专用 MSBuild 构建通过；**仅复制留存动态 caller，不是重编译 caller**。上传 `artifacts/nativecaller-startup-package`，只有一个 EXE，无 PDB/DLL。执行 `WpfGfxShape.NativeCaller.exe`，无参数，30 秒限时，返回 `Usage: NativeCaller.exe <absolute-path-to-wpfgfx_cor3.dll>`，未报 launch failure/超时。执行能力未返回数值 exit code，故不编造 0。回收 `artifacts/sandbox-readback/nativecaller-startup`。只证明该 x64 实物可以进入 main，不证明 probe/生产 ABI。
- 未运行静态 caller：缺历史兼容 probe DLL，不拿当前生产 DLL 强跑旧协议。未安装或下载 CRT/DLL，没有恢复已作废 probe。
- 当前真实入口上传 `artifacts/com-acceptance-package`（25 项白名单，不含 caller），执行 `WpfGfxShape.ComAcceptance.exe`。路径/身份 11/11；之后 EventProxy + 路径 smoke 40/40，fail/error/timeout/skip 均 0。MSTest 4.0.1、win-x64、远端 .NET 10.0.1，非用户要求固定 patch。EventProxy 实际加载生产 AOT `MILCreateEventProxy` 并保持模块驻留。
- smoke 原始 TRX：`artifacts/sandbox-readback/nativecaller-routing-smoke/TestResults/nativecaller-routing-smoke.trx`，RunID `123659bb-331c-4529-bca2-39cd9346085a`，Counters 40/40 已读回；同目录 Results/aot-identity.json、aot.sha256、package-files.txt 和实际包回收。路径轮见 `nativecaller-routing-paths`，RunID `2a3dcc09-421c-40f8-8204-a7e1eedf7512`，Counters 11/11 已读回。

## 未闭合事项

没有 caller 项目可按 Release × Win32/x64 重编译，没有 x86 caller 实物。故**x86 构建/启动与原 Debug CRT 根因修复未完成**，不以 x64 smoke 替代正式全矩阵，也未重跑 PublicAbi、PresentationCoreE2E 或全量 COM。若必须修截图对应 caller，需要取得实际上传 EXE（含 SHA）及对应项目/调用记录；不得猜路径、伪造 /MT 修复或要求安装调试运行库。当前已执行的纠正是选择真实现行 COM 测试入口，并防止过期 README 再引导误用，不等同修复未取得的历史二进制。
