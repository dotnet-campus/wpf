# PresentationCore 独立根因与真实渲染进展

## 本轮范围与结论

本轮仅Docs维护：实际读取原始JSON、诊断log、承载Staging/manifest/README及相关生产源码；未修改代码、配置、README或manifest，未执行终端、脚本、build、publish或测试。原始证据不覆盖，不重新计算DLL哈希；下列身份来自运行JSON的requested/frozen/actual模块字段。

最新已核不是“仍卡初始化”，也不止3026的E_NOTIMPL：primitive消费者候选已真实Render/CopyPixels并产生4096 bytes，但独立byte-exact比较Red。当前主线为primitive完整消费与严格像素差异闭环，不宣称模块或E2E-05完成。

## 未改WPF与独立原版

- [primitive原版JSON](../../Investigations/PresentationCore-render/Results/primitive-original-01/Results/original/result.json)：x64、.NET 10.0.1、32×32、96 DPI、Pbgra32、threshold=0、Green。
- PresentationCore SHA `605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654`；WindowsBase `A7BDC35EC37C1DEB33D24D97213BA1077E3051ABB66250913C9C1FA735B0D3BC`；文件版本10.0.125.57005。
- 原版实际加载系统wpfgfx SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`；像素SHA `7F55E138C9605464AE90811BD28B5477871BB3E1AA71F5FD3A1C8B052E0C308B`。
- 原版/候选原始JSON包含真实PresentationCore resolver、选择路径及actualNativeModules；不是改PresentationCore或仅给测试程序集安装resolver的替身证据。

## 逐轮根因与身份隔离

| 证据 | 候选身份 | 实际结果与解释 |
|---|---|---|
| [历史final-candidate](../../artifacts/test-results/PresentationCoreE2E/evidence/final-candidate/Results/candidate/result.json) | `AAB3C60248ED76C710D512D59DB1640F4C7F0E25A053C38B07C7EA4974EC6760` | DrawingContext-open，MediaContext.CreateChannels/ConnectChannels/SyncFlush，0x88980406；历史初始化Red，不是当前停点 |
| [rootcause-01 log](../../artifacts/test-results/PresentationCoreE2E/rootcause-01/Results/candidate/uce-diagnostic.log) | 以该轮独立identity为准 | 明确InvalidOperationException：MoveToImmutable要求Count=Capacity；堆栈落Direct3D9MonitorSnapshotFactory.EnumerateSystem → ChannelGraphicsCapabilities.Query/CreateTierMessage → SameThreadChannel.CommitBatches |
| [rootcause-02 JSON](../../artifacts/test-results/PresentationCoreE2E/rootcause-02/Results/candidate/result.json) | `8A19D392901BAD7359B7F71E90CFEB95C756F35E0C56A6BAF3BFDA74E36E8178` | 已过RenderOpen和RTB构造，转至RTB Render，0x80004001；不能和3026视为同一DLL |
| [initial-candidate JSON](../../Investigations/PresentationCore-render/Results/initial-candidate/Results/candidate/result.json) | `3026A85F0FC2466BA0EEF662DDFEF12092629663CE81B48770E18960B55FA212` | 真实加载一致；RTB Render E_NOTIMPL，未到CopyPixels，清理完成 |
| [primitive-consumers-01 JSON](../../Investigations/PresentationCore-render/Results/primitive-consumers-01/Results/candidate/result.json) | `5DA218DE34C17C36A1964FFF7C68FD9495BCF3A8E5EA2A188748BE8753430F84` | stage=completed、执行Green、4096-byte CopyPixels；像素SHA `5DFFD59D20FA5D52272B0733D22BFCAE80E5AC6574B248F00C5D49441C9A106B`，不等于比较Green |

## 根因接线与当前差集

已核源码：SameThreadChannel处理0x3D非交互政策并通过ChannelGraphicsCapabilities.CreateTierMessage回包；Direct3D9MonitorSnapshotFactory.EnumerateSystem动态builder现为ToImmutable，不再要求容量恰好等于有效DPI context数量。不能把已过的初始化重新列为唯一下一主线，亦不能把其有限成功当完整失败/取消/OOM寿命验收。

3026的后续生产缺口定位为GeneratedVisualRenderer.CaptureInstructions遗漏DrawRectangle/DrawLine消费，而非解析器缺失。现源码已有GeneratedPrimitiveGeometry.IsPrimitive/Create，经DrawGeometry消费并在finally释放临时primitive；另一生产代理继续完整primitive族与寿命收口。本轮只核读，不代替其实现或验收。

[primitive-compare-01 comparison.json](../../Investigations/PresentationCore-render/Results/primitive-compare-01/Results/diff/comparison.json)明确Comparator Red、PixelMismatch、threshold=0、nativeModulesLoaded=[]；两端像素SHA与上列原版/候选一致。[像素诊断](../../Investigations/PresentationCore-render/Results/primitive-pixeldiff-01/Results/pixels.json)列出12个差异坐标，集中于斜线区域；这仅定位现象，不凭此断定量化/覆盖率的最终根因，不放宽golden或阈值。

## 承载可重建性与文档边界

已核[Staging.proj](../../Tests/WpfGfxShape.PresentationCoreE2E/Staging.proj)：Stage从正式publish读取候选，Copy后比对CandidateHash/SnapshotHash，自动写包内schemaVersion=2的candidateSha256 manifest及source-identity；Baseline读evidence/final-original，不依赖original-first执行包。Stage本身不生产publish，Archive另有原物Copy/SHA核验；不声称本轮重建、归档或回收。

工程[README](../../Tests/WpfGfxShape.PresentationCoreE2E/README.md)与[manifest](../../Tests/WpfGfxShape.PresentationCoreE2E/manifest.json)保留。README仍有735640固定候选和初始化旧说明；源码manifest的stagedCandidate现为F6AA6CFF…且标记维护者未运行。两者不作为最新已测身份权威；运行JSON、各包manifest/source-identity与本报告分别保留各自范围，不把移动中的正式publish等同已测5DA218或3026。

## Closed / Open

- 有限已证：未改WPF的10.0.1独立原版Green、resolver/模块身份、初始化越过、primitive候选完成Render与CopyPixels。
- Open：严格整面像素一致、完整primitive生产方法族/资源寿命、最终同SHA本族/related/full/strict及完整E2E矩阵；旧COM765/765、1415/32/1447只属773945，禁止绑定新SHA。
- [四外部输入](../all-completion-external-prerequisites.md)：authority、ARM64工具链及真实环境、完整10.0.12 runtime、player协议/ownership/oracle仍Open；不阻断当前10.0.1建设版自主推进，但不能豁免最终验收。
- [七阶段及原条件引用](../remaining-gap-closure-plan.md)、[独立E2E合同](../e2e-executable-contracts.md)不变；[DEC-0004](../decisions/DEC-0004-process-lifetime-module.md)进程驻留，卸载/重载NA，资源释放和线程/callback清理仍验收。正式总门Red，无替换资格。
