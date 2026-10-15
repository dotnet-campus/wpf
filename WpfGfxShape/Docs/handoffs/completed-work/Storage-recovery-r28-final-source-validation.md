# r28 存储恢复与当前源码验证（非总完成）

## 实际回收范围与保护

仅使用专用文件工具和构建工具，无终端、脚本、Exec、自定义任务或生产合同修改。
先读当前交接与 Media-retry-r28-storage-blocked，再检查本轮 Media-retry-r28 的 full/events/events-results 回收结构。未清理任何历史 authority、冻结档、原生 repo 文件、正式 publish、源码或生产 Native DLL。
旧远端任务无可定位清理入口，本轮未声称清理旧沙箱。工具不能证明可用空间或释放字节，不报告空间数值。

瘦档为 `Investigations/Storage-recovery-r28/slim/`（相对 WpfGfxShape）。Preserve.proj 使用 MSBuild 内置 Copy/GetFileHash/WriteLinesToFile，复制本轮回收目录所有非 DLL/EXE/PDB 文件，包括两份原始 TRX、JSON、identity、SHA、包清单、reference data、deps/runtimeconfig；原件均原位保留。source-evidence.sha256.txt/slim-evidence.sha256.txt 保存可列举路径及 SHA256。Verify.proj 全序列哈希相等门禁成功，verified.txt 已回读，两份 TRX 分别为：
- media-retry-r28-full.trx：249D2CDAFC105A46D142B6A3DCE2705964449F971FBB0D883D81538DBE3680D3。
- uce-final-full-r28.trx：028B498ED0F3C11A4A124ADD6B250575ADCC47117378F78F25FC7BD95619C549。

Cleanup.proj 在同一 Verify 门禁通过后仅按显式文件列表 Delete，未 RemoveDir。实际删除18个 DLL：
- `Media-retry-r28/full/` 根目录16个：WpfGfxShape.ComAcceptance.dll；Microsoft.Testing.Extensions.MSBuild.dll、TrxReport.Abstractions.dll、TrxReport.dll、VSTestBridge.dll（后三项均同 Microsoft.Testing.Extensions 前缀）；Microsoft.Testing.Platform.dll；Microsoft.TestPlatform.AdapterUtilities.dll、CoreUtilities.dll、PlatformAbstractions.dll、Utilities.dll；Microsoft.VisualStudio.TestPlatform.ObjectModel.dll；MSTest.TestAdapter.dll、MSTest.TestFramework.dll、MSTest.TestFramework.Extensions.dll、MSTestAdapter.PlatformServices.dll；Newtonsoft.Json.dll。
- `Media-retry-r28/events/Microsoft.Testing.Extensions.MSBuild.dll` 与 `events-results/Microsoft.Testing.Extensions.MSBuild.dll`：失败回收留下的空副本，SHA256 均为 E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855。

16个 full DLL 均在删除前与未动的 UCE-final-integration-20261014-r28/full 根目录对应 DLL 逐一 SHA256 比较一致，包括执行器。精确删除清单 deleted-files.txt；删除前哈希 delete-candidates.sha256.txt；清理工程构建成功，删除后目录回读根目录 DLL 已不存在，Native/ReferenceData/Results/原始清单仍在。保留 EXE，未删除任何 PDB、目录或证据原件。

## 构建、发布与执行

- COM 当前源码 Release build 成功，先前 CS8104 本次未复现。
- P1（本轮唯一发布）：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true -p:AotDllPath=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/Investigations/UCE-final-integration-20261014-r28/full/Native/wpfgfx_cor3.dll`，专用发布工具成功，net10.0-windows/Release，未指定 RID；标准目标重新生成 `WpfGfxShape/artifacts/com-acceptance-package`，撤回前诊断包已被标准包替换。无新生产 Native AOT publish，不冒充新生产实现。
- 新包 aot.sha256 回读为 59FA4BAB8C0A16C4B556656F4B32F6D915A80C3B8DA4775DA577322E13C28451。
- GenProto.Tests 当前源码 Release build 成功；专用 dotnet test 默认 Debug，退出0，24 pass / 0 fail / 24 total，零跳过，10秒。原历史解析器 nullable warnings 仍在，未修改 authority 源码。无新 GenProto TRX/RunID，结论依据测试工具日志，不沿用旧23项计数。
- 已读 IndependentManagedCoreTests 最新有限差异处理：MilGlyphRun 托管2/原生4；MIL_PEN_DATA 托管44/原生48，均记录 satisfied=false。24项通过仅表示既有有限审查测试通过，不证明两项 ABI 差异消失或 C Closed。本轮未改布局、断言预期、golden、deadline 或 skip。

直接运行新标准 MSTest EXE，两次仅回收 Results，均成功，证明本轮所需构建/写入/结果回收可操作；不推断剩余磁盘容量。

| 验证 | pass/fail/total | RunID | 任务 |
|---|---|---|---|
| UCE+路径定向 | 39/0/39 | 10b4337b-c0c7-4530-bf24-8c2d10237810 | 8dd8db7faccd4ea4b62376d99427f75a |
| COM full | 1364/40/1404 | 9422464d-ba3e-40bc-81e7-47fd69fc31a5 | 77311104ba5f44a6afa51431bf46d89e |

证据根 `Investigations/Storage-recovery-r28/{targeted,full}/Results/`，原始 storage-recovery-r28-{targeted,full}.trx 与 aot-identity.json 已回读。Counters 全部 executed、零 notExecuted；full 结果 Failed、退出2。两次 Sandbox=true、路径在各自任务内、同上生产 SHA。

## 当前真实 Red 与边界

full 最新不再是1396/8：八项媒体仍在15秒 WaitForNotification 失败，另有32项硬件/cache/shader/呈现相关失败。原生 GPU staging 读回 FF123456 正确，但 topmost/client/front-buffer 呈现对照失败；多项像素为0，部分日志存在外部窗口遮挡，不能把全部失败单一归因环境或生产，也不能把原生对照失败当候选实现已验收。原始 TRX保留具体参数、失败堆栈和 PresentCondition。

存储阻塞在本轮构建及 Results 回收范围已解除；旧失败事件对照任务仍无观测，未假称恢复。媒体原版/候选同fixture事件/查询/像素与fixture有效性仍缺证据。最新硬件呈现 Red 应同模块实证门禁保留，未改实现来隐藏失败。

无源码改动；生产 SHA沿用r28，不宣称最终生产重新发布或总完成。A/B/C开放，阶段4/5 Active，6–7 Pending；普通回归合法原版10.0.12加载先决条件和C ARM64/独立对端/历史authority等仍未关闭。
