# perf 同步 Compose 合同修复与证据核验

## 范围与身份

本档由 Docs 维护执行者读取既有源码、原始 JSON、TRX 与 identity 后落盘；本次未修改生产/测试/配置，未执行脚本、构建、测试或发布。生产修复及既有运行由实现执行者完成。本档不臆造缺失的发布命令/日志或提交来源；运行 identity 只证明实际加载文件。

- 最新生产 SHA256：`202A8C9F63D63CF4058BD06D236A045254F396D6FD92ED55D68D93646E4A6769`。
- 独立原版 SHA256：`94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`。
- [候选原始报告](../../Investigations/Perf-glyph-contract/candidate/Results/dc802a7952fa43d08b93852ffb6925c7/report.json)：RunId `dc802a7952fa43d08b93852ffb6925c7`，X64；候选 identity 同上。
- [COM identity](../../Investigations/Perf-glyph-contract/com-final-full/Results/aot-identity.json)：实际加载沙箱 `Native/wpfgfx_cor3.dll`，SHA 同上；不是旧 HWND SHA 的定向结果。
- [COM 原始 TRX](../../Investigations/Perf-glyph-contract/com-final-full/Results/perf-glyph-full.trx)：RunID `c0799a84-cdfb-4ed4-8c8b-1c496fc982c2`，Completed，total/executed/passed=1429，failed/notExecuted=0，其余错误/超时/中止计数=0。
- [原始审计](../../Investigations/Perf-glyph-contract/Audit/Results/audit.json)：SameFullIdentity=true；Missing=`MilPlayer_Create`、`MilPlayer_Process`，Extra=[]。full Green 不关闭 strict 缺项或授予替换资格。

## 原生合同、错误与修复边界

实际读取原 WPF `core/uce/connectioncontext.cpp:448–525`：`PresentAllPartitions` 用于同线程同步 compositor 的 batch/render；只对 `hSourceChannel == NULL` 的 partition 调用 `Compose(&fPresentNeeded)`，不调用真实 HWND `Present`。源码注释明确该路径不需要 presentation，结果由通用 bitmap target 消费。函数名及 S_OK 不能证明进入屏幕 Present。

历史 [perf 档](Four-perf-closeout.md) 的 flags0/2/0：候选 WM_USER 数0/1/0、原版0/0/0。候选错误地在同步 Compose 路径发出 SignalPresent，并非仅缺等待或 flags 位猜测。

本次实读生产 `Abi/UceChannelExports.cs:58–93`：同步入口保留根 partition 筛选、首错/失败处理与 finally 通知清理，调用 `partition.Targets.Render(signalPresent: false)`。`GeneratedTargetRegistry.Render`、`GeneratedVisualTargetResources.Render` 将标志传至 `HardwareRenderTargetExports.Render`，后者交给 `PerformanceInstrumentation.Presented`。

`PerformanceInstrumentation.cs` 仅在 signalPresent=true、result>=0 且 flags bit2 时 PostMessageW(WM_USER,123,456)。默认真实异步 Present 路径保留 signalPresent=true 与 bit2 信号，未全局禁用 perf。原生完整 needs-present/target/render-enabled/no-screen-access/zombie/底层成功条件仍须真实分支差分；本档不将源码保留等同运行等价。

| 同步 flags 顺序 | 候选 Count / HRESULT | 原版 Count / HRESULT | 审计 |
|---|---|---|---|
| 0 | 0 / 0 | 0 / 0 | Passed=true |
| 2 | 0 / 0 | 0 / 0 | Passed=true |
| 0（恢复） | 0 / 0 | 0 / 0 | Passed=true |

审计 CandidatePerf/ReferencePerf 均注明 `CConnectionContext::PresentAllPartitions/Compose`、ExpectedCount=0、PayloadValid=true。零消息用例不能证明真实异步消息 payload 已运行验证。同步误发修复为有限 Closed；另一代理的异步真实 Present 分支差分仍进行中，未标完成。禁 throttle/并发 flags/完整调度政策不被三项同步用例覆盖。

## 历史 32 失败逐项与本次结果

原始审计 `PreviousFailures` 保存旧错误与每项 CurrentOutcomes=[Passed]；下表按其顺序列出全部32项。旧像素失败包括0对0x00563412/0x00123456及容差断言。只报告本次不再失败，不将全部归为环境或同步 perf 修复，不推断所有根因已修。

| # | 测试（参数保持原值） | 本次 |
|---|---|---|
| 1 | WhenStage4HardwareCacheGeometryDrawsThenHitAndMutationPreservePixelsAfterCallerRelease (4278255360,65280) | Passed |
| 2 | 同上 (4278190335,16711680) | Passed |
| 3 | WhenStage4HardwareCacheConfigurationFailsThenRecoveryAndMutationRenderRealPixels (4278255360,65280) | Passed |
| 4 | 同上 (4278190335,16711680) | Passed |
| 5 | WhenStage4HardwareWicCacheDrawsThenHitAndSourceReplacementPreservePixels (4278255360,65280) | Passed |
| 6 | 同上 (4278190335,16711680) | Passed |
| 7 | WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive (0) | Passed |
| 8 | 同上 (1) | Passed |
| 9 | 同上 (2) | Passed |
| 10 | 同上 (3) | Passed |
| 11 | 同上 (4) | Passed |
| 12 | WhenStage4HardwareWicCandidateIsWriteLockedThenCommittedFrameSurvivesAndSameGenerationRetries (4278255360,65280) | Passed |
| 13 | 同上 (4278190335,16711680) | Passed |
| 14 | WhenStage4HardwareCacheHasTwoDevicesThenBothCommittedKeysHitWhileWicIsWriteLocked (False) | Passed |
| 15 | 同上 (True) | Passed |
| 16 | WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels (40,40,21,False,0,False) | Passed |
| 17 | 同上 (120,80,21,False,0,False) | Passed |
| 18 | 同上 (40,40,21,True,0,False) | Passed |
| 19 | 同上 (120,80,22,True,0,False) | Passed |
| 20 | 同上 (40,40,21,True,1,False) | Passed |
| 21 | 同上 (40,40,21,True,2,False) | Passed |
| 22 | 同上 (40,40,21,True,3,False) | Passed |
| 23 | 同上 (40,40,21,True,0,True) | Passed |
| 24 | WhenHardwareShaderGenerationIsReplacedThenTwoFramesConsumeLegalBytecode | Passed |
| 25 | WhenMalformedHardwareShaderFallsBackThenIdentityPixelsAndBadShaderFifoAreReal | Passed |
| 26 | WhenShaderChannelAndDeviceAreClosedThenNewPublicTargetRendersReplacementGeneration | Passed |
| 27 | WhenNativeGpuPresentsThenTopmostWindowIsObservable | Passed |
| 28 | WhenNativeGpuPresentsThenFrontBufferAndClientPixelsAgree (1) | Passed |
| 29 | 同上 (3) | Passed |
| 30 | WhenSecondSamplerUsesVisualOrCacheThenNonIdentityShaderCombinesBothInputs (False,4278255360,65408) | Passed |
| 31 | 同上 (True,4278190335,16711808) | Passed |
| 32 | 同上 (True,0,128) | Passed |

## Closed / Open 与总门

- Closed（有限）：同步 Compose 不发 perf；候选/原版0/0/0，HRESULT均0；本次 COM1429/1429、零跳过；历史32项在本次逐项 Passed。
- Open：异步真实 Present perf 差分正在进行；glyph raw22/30、定义字段30/30，未初始化 padding 裁决未闭，不能降低 raw 要求宣称完成。有效 glyph 消费、失败 callback/OOM/释放寿命为后续完整模块，不拆成仅字段复验。
- Missing2/Extra0仍为 player 官方 PRERELEASE 包头/dispatch/对象 destroy/ownership 协议及 oracle 缺失，不猜实现。
- [四类外部输入](../all-completion-external-prerequisites.md)仍为 authority、ARM64、完整10.0.12 runtime、player record protocol；一次 full Green 不关闭七阶段/A/B/C/E2E-01–09、架构矩阵或 Ledger。
- [DEC-0004](../decisions/DEC-0004-process-lifetime-module.md) 已接受：模块只加载并进程寿命驻留；unload/reload/热替换不适用，不再请求批准，资源/线程/callback 清理不豁免。
- 当前恢复入口：[交接](../next-session-handoff.md)、[当前工作](../handoffs/current-work-item.md)、[总计划](../remaining-gap-closure-plan.md)。无最终兼容替换资格。
