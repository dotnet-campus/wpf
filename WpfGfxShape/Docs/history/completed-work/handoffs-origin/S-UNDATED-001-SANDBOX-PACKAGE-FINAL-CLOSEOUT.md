# 独立完成归档：COM 标准 MTP 沙箱小包最终文档收口

## 交付范围与证据级别

本轮为监督式独立核对和文档归档，不探索/修改生产代码，不扩大生产修复。已读取 Docs 章程、连续性协议、当前执行规则、归档规则及当前测试工程文件。无终端/脚本操作；仅专用文件、测试与沙箱执行能力。未修改任何冻结历史文件。

本轮核对的当前工程：唯一 `Tests/WpfGfxShape.ComAcceptance`，net10.0-windows、MSTest 4.0.1、OutputType=Exe、EnableMSTestRunner、无 ProjectReference；标准 MTP 生成验收应用，不是自建 Host。未新增 Host、生产导出、协议、架构或 .slnx。

StaticConfirmed：AcceptancePaths 对沙箱路径使用 AppContext.BaseDirectory、拒绝 rooted/越界路径；初始化在真实生产调用前验证原生 PE/架构/SHA 并写 Results/aot-identity.json。Package targets 使用显式白名单并清理包目录，不递归复制 obj/源码/PDB。包清单测试排除运行期 Results/TestResults，初始包不含这些输出。

## 发布记录（既有交付，本轮未新发布）

以下完整命令由用户本轮交接提供；本轮读到产物及运行身份，但没有重新执行 publish，也未取得既有 publish 完整日志或可靠发布序号，不能虚构发布次数/警告数/当前提交来源。

| 动作 | 命令（工作目录 WpfGfxShape/） | 配置/产物及核对 |
|---|---|---|
| 生产专用 publish，原序号未取得 | `dotnet publish Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64` | Release/win-x64；用户交接成功；`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`，本机及沙箱运行日志同 SHA |
| 沙箱包 publish，原序号未取得 | `dotnet publish Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true` | Release，TFM net10.0-windows；命令未显式 RID，不冒称自包含发布；生成 `artifacts/com-acceptance-package`，本轮目录/清单读回及直接 exe 定向执行成功 |

唯一生产 AOT SHA256：`271E8FB769FD76A9139A3C71A2B2F42C1E471B46358E569726481D8A90026441`。本轮没有额外计算二进制哈希；依据是 TRX 中实际加载日志、验收应用生成的身份 JSON 及包 aot.sha256 三者一致，不等同于构建提交证明或包整体哈希。

## 本机最终 COM TRX 实际读回

相对于 WpfGfxShape/：`Tests/WpfGfxShape.ComAcceptance/TestResults/local-final-full.trx`。

- RunID `bcd5487d-1450-42a6-826f-346ebbd3a8f8`。
- Times start `2026-10-14T02:10:31.4351904Z`，finish `2026-10-14T02:14:56.9089551Z`。
- 行10131：total=1365、executed=1365、passed=1365、failed=0、notExecuted=0、aborted=0。
- TRX 行1414等实际加载日志为发布路径 wpfgfx_cor3.dll 及上述 SHA。相邻 `aot-identity.json` 同 SHA、Sandbox=false。因此本机结果/RunID/同产物身份已实际核对，不借用旧绿色记录。
- 本机 ordinary4212/4212、GenProto23/23 是用户本轮交接；本轮未独立读回对应原始结果/RunID，明确缺证据，不编造。

## 沙箱证据实际读回

| 运行 | TRX（相对于 artifacts/sandbox-readback/） | RunID | 结果 |
|---|---|---|---|
| 既有 final smoke | final-smoke/TestResults/sandbox-smoke.trx | 2d934d81-5b33-4cc3-8f98-0c2cd9bc2f44 | 40/40，failed/notExecuted/aborted=0 |
| 既有 final full | final-full/TestResults/sandbox-full.trx | 4d0727bd-1b99-403f-bd1f-616d107e0035 | 1343 pass/22 fail/1365 total/executed，notExecuted/aborted=0；RunInfo退出2 |
| 本轮专用执行定向 | closure-paths/TestResults/closure-paths.trx | 932dcc34-fc40-4ed7-97a0-7a29acf21d24 | AcceptancePathsTests 10/10，零失败/跳过 |

三个目录的 `Results/aot-identity.json` 均读回上述 SHA、Sandbox=true；full 任务路径为 `C:\CodingAgentSandbox\Tasks\0556c048c04d4423afed6a9f9678b8a1\Native\wpfgfx_cor3.dll`。本轮定向仅上传 `artifacts/com-acceptance-package`，直接执行标准 exe，参数 `--filter FullyQualifiedName~AcceptancePathsTests --report-trx --report-trx-filename closure-paths.trx`，270ms，完整任务回收到 closure-paths。

全量 22 失败保留：14 HWND/双输入像素，8 视频通知超时。已读代表性像素失败输出：CASCADIA_HOSTING_WINDOW_CLASS 覆盖、nativeControl 的 rawPresent=S_OK 但 pixel=0、conditionSatisfied=False；这是观察，不证明所有候选行为正确，更不能裁定22项仅环境根因。媒体代表性输出为15秒内未收到真实系统通知的断言失败；TRX Counters timeout=0，因为这是用例断言失败，不是 runner timeout 分类。原断言、零跳过门禁和 Red 均不变。

## 实际包25文件/2子目录清单

本轮递归列举 `artifacts/com-acceptance-package`，与该目录 package-files.txt 的25行逐项一致。子目录仅 `Native`、`ReferenceData`；包只含生产 Native DLL、验收应用及必要运行依赖、三份参考 JSON、身份和清单，无 obj、历史源码、PDB、自建 Host 或额外运行结果。

1. WpfGfxShape.ComAcceptance.exe
2. WpfGfxShape.ComAcceptance.dll
3. WpfGfxShape.ComAcceptance.deps.json
4. WpfGfxShape.ComAcceptance.runtimeconfig.json
5. Microsoft.Testing.Platform.dll
6. Microsoft.Testing.Extensions.TrxReport.dll
7. Microsoft.Testing.Extensions.TrxReport.Abstractions.dll
8. Microsoft.Testing.Extensions.VSTestBridge.dll
9. Microsoft.VisualStudio.TestPlatform.ObjectModel.dll
10. Microsoft.TestPlatform.AdapterUtilities.dll
11. MSTest.TestAdapter.dll
12. MSTestAdapter.PlatformServices.dll
13. MSTest.TestFramework.dll
14. MSTest.TestFramework.Extensions.dll
15. Microsoft.TestPlatform.CoreUtilities.dll
16. Microsoft.TestPlatform.PlatformAbstractions.dll
17. Microsoft.TestPlatform.Utilities.dll
18. Newtonsoft.Json.dll
19. Microsoft.Testing.Extensions.MSBuild.dll
20. ReferenceData/glyph-arial-original.json
21. ReferenceData/original-wpf.json
22. ReferenceData/source-clip-intersection.json
23. Native/wpfgfx_cor3.dll
24. aot.sha256
25. package-files.txt

## 项目级 global.json 审查及真实入口限制

文件保留原样，未做配置清理。其 SDK version=10.0.107、allowPrerelease=true、rollForward=latestFeature 与根一致；但缺根的 sdk.paths (.dotnet/$host$) 及其他仓库配置，故不能声称实际 SDK 选择完全不变。局部 test.runner=Microsoft.Testing.Platform 对项目目录执行的 .NET10 标准 MTP Test 入口有作用，不应无验证删除，也不应修改根 runner 影响其他项目。

本轮用专用测试入口从工作区根指定 csproj，筛选 AcceptancePathsTests，真实结果退出1：Microsoft.Testing.Platform.MSBuild 2.0.1 提示 `.NET 10 SDK and later` 不支持 VSTest target，需 opt-in 新 dotnet test。未启动测试方法；不是代码断言失败，也不能说专用 dotnet test 已兼容。命令发现配置起点是工作目录，单传项目路径不会读取项目级 global.json。本轮工具未提供选择该命令工作目录的入口，因此未对正确目录的 MTP Test 命令作成功执行声明。

正确目录命令和已验证直接 exe 参数已经写入 `../../com-nativeaot-testing-method.md`、`../../com-acceptance-sandbox-operations.md`。本轮保留 SDK/TFM/配置，不引入额外架构；已用沙箱直接标准 exe 的10项验收验证路径、身份和白名单。没有必要为纯文档收口重跑耗时全量。

## 本轮实际文档修改与后续门禁

更新当前入口 next-session-handoff、current-stable-baseline、current-work-item、remaining-gap-closure-plan、com-nativeaot-testing-method；新增沙箱操作文档与本独立归档。当前基线/工作项移除相互竞争的历史恢复点，历史证据仍留既有不可变 completed-work 文件，本归档不改冻结历史。

优先顺序已按完整模块写入当前工作项：P0 沙箱22失败根因与同fixture原生对照→P1 Stage4-A cachehit CopyPixels/失效/失败事务重试/拥有寿命及HW+SW全矩阵→B generation/device cache/BadShader/fallback/OOM→C GenProto required evidence，再后续有限渲染缺口和阶段5–7。真实固定像素/计数/首错/引用清理、同SHA publish/full/TRX归档是关闭标准，不拆为连续小切片。

**裁定：小包修复已能传输、启动、执行；沙箱全量仍 Red。Stage4 Active，块A未完成，22失败及B/C门禁均未关闭；不跳过、不降断言、不宣称仅环境已确诊，不具备生产替换资格。**
