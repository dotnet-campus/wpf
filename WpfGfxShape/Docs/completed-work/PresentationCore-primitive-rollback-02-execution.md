# PresentationCore primitive：rollback-02 独立执行证据报告

## 本次文档维护与证据截点

本次仅实际读取并同步Docs；未修改生产代码、配置、测试、承载README/manifest，未使用终端脚本，未构建、发布或执行测试，也未覆盖原始证据。本文记录已有执行结果，不把读取结果冒充本轮执行。
本文的最新已核身份限定为 `rollback-*02`；移动中的publish与并行代理后续产物不自动成为本报告已验收版本。
此前初始化与primitive接入过程保留于[历史根因进展](PresentationCore-render-rootcause-progress.md)，不再作为当前待修停点。

## 生产交付与回撤边界

已交付DrawRectangle、DrawRoundedRectangle、DrawEllipse、DrawLine四类primitive及animated路径，经CaptureInstructions创建临时geometry、接入DrawGeometry，并保留临时资源finally释放接线。
真实未改PresentationCore已越过RenderOpen、RTB构造与Render/CopyPixels，产生非空4096-byte结果；这证明有限真实消费，不证明所有primitive/animated组合与资源寿命均已验收。
失败rail实验未改善严格比较，已精准撤回；保留Quantize、RGBA wire及primitive实现，不把整族实现误写成已撤销。
[rail比较原物](../../Investigations/PresentationCore-render/Results/rails-compare-01/Results/diff/comparison.json)仍为PixelMismatch，其候选像素SHA为 `40A01E55782C7A6EDD1D5935DAC5BDF41278EC211DD8FDD5B82D4A3D6B90CB2E`，不是当前rollback候选像素或DLL身份。
[rollback源码身份清单](../../Investigations/PresentationCore-render/Results/rollback-source-sha256.txt)保留当轮源码指纹；本次不重新计算，也不据此声称当前移动工作树与该快照相同。

## rollback-02 原始身份与真实加载

- [原版result.json](../../Investigations/PresentationCore-render/Results/rollback-original-02/Results/original/result.json)：task `46974ac9dc634f6cb30ddbfec57c66b7`，2026-10-15 08:32:34 UTC开始，stage=completed、Green、exitCode=0。
- [候选result.json](../../Investigations/PresentationCore-render/Results/rollback-candidate-02/Results/candidate/result.json)：task `9e0a2f2c1bbc41d982bfb5c02b77873d`，08:32:39 UTC开始，stage=completed、Green、exitCode=0。
- [候选包manifest](../../Investigations/PresentationCore-render/Results/rollback-candidate-02/manifest.json)的candidateSha256与运行JSON的requestedNative、frozenCandidate、actualNativeModules一致：`B9CC11ED64ADA4543B00CA3F8F82080CCC78BA9933C6E6C39681AD2FD4B75557`；DLL长度4445696。
- 原版实际加载系统wpfgfx SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`，不是包内冻结候选；候选实际加载任务目录Candidate中的B9CC DLL。
- 双方均为X64、.NET 10.0.1建设版，target10.0.12Acceptance=false；固定32×32、96 DPI、Pbgra32。不是目标10.0.12验收。
- 未改PresentationCore SHA `605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654`；WindowsBase SHA `A7BDC35EC37C1DEB33D24D97213BA1077E3051ABB66250913C9C1FA735B0D3BC`，文件版本10.0.125.57005。
- resolverCalls的owner为真实PresentationCore，并记录DrawingContext-open、RTB构造/渲染的选择路径；与实际模块身份对应，不是仅给测试程序集安装resolver的替身证据。

## 严格像素结果：执行Green，比较Red

[独立comparison.json](../../Investigations/PresentationCore-render/Results/rollback-compare-02/Results/diff/comparison.json)：Comparator、byte-exact、threshold=0、Red、PixelMismatch、nativeModulesLoaded=[]。
原版pixels SHA：`7F55E138C9605464AE90811BD28B5477871BB3E1AA71F5FD3A1C8B052E0C308B`。
候选pixels SHA：`5DFFD59D20FA5D52272B0733D22BFCAE80E5AC6574B248F00C5D49441C9A106B`。
两端pixelByteCount均4096；`5DFFD59…`是像素hash，不是DLL hash。rollback后像素回到primitive-consumers-01的同一有限结果，不表示DLL身份相同。
[rollback像素诊断](../../Investigations/PresentationCore-render/Results/rollback-pixels-02/Results/pixels.json)实际列出12个差异：
`(13,20) (14,20) (14,21) (15,21) (15,22) (16,22) (16,23) (17,23) (17,24) (18,25) (19,27) (20,28)`。
差异位于固定场景三角斜描边区域；这是现象定位，不是量化、rail或join根因已证明，不降低golden、阈值或required。

## 同B9CC身份COM结果与不同证据分母

| 范围 | 原始结果 | 身份与原物 |
|---|---|---|
| rollback full | 1415 pass / 32 fail / 1447，notExecuted=0 | [TRX](../../Investigations/PresentationCore-render/Results/rollback-com-full-02/Results/Administrator_EN-DEV-01_2026-10-15_08_34_51.2512879.trx)、[identity](../../Investigations/PresentationCore-render/Results/rollback-com-full-02/Results/aot-identity.json)，task `9748772839a64969940fea273d2d6958` |
| rollback related | 522 pass / 2 fail / 524，notExecuted=0 | [TRX](../../Investigations/PresentationCore-render/Results/rollback-com-related-02/Results/Administrator_EN-DEV-01_2026-10-15_08_35_58.8557722.trx)、[identity](../../Investigations/PresentationCore-render/Results/rollback-com-related-02/Results/aot-identity.json)，task `f39d9e34624c4b3a9e9aefaa4276e625` |

两个identity均明确为B9CC DLL；两份TRX ResultSummary均Failed。related筛选为BitmapRenderTargetTests、Geometry或Pen，不与旧765/765或1248分母拼接。full计数虽与历史773945相同，本次身份关联来自新TRX与identity，不是继承旧结果。
普通Tests既有4224 pass / 1 fail / 4225（指定10.0.12 runtime缺失）及GenProto 28/28为各自独立证据，不是本次B9CC同次回归。
GenProto [TRX](../../Tests/WpfGfxShape.GenProto.Tests/TestResults/lindexi_DESKTOP-51A5UGG_2026-10-15_10_51_03_net10.0.trx)、[delivery报告](../../Investigations/GenProtoEvidence/delivery-6a03b29edb064c22a50b7b1985affa62/report.json)与[恢复记录（已清理；合同/说明）](../history-index.md)保留；sourceWorkingTree=NotInspected、stage4Closed=false，旧delivery缺物不伪造恢复成功。

## 独立triangle对照的时间边界

[controls-original-02](../../Investigations/PresentationCore-render/Results/controls-original-02/Results/original/controls.json)已有独立原版fill/stroke/both及join/transform对照。
“原版已跑、候选未跑”仅描述rollback-02当轮交接截点；另一代理正在实际推进候选对照。目录中已可见controls-candidate-03/04与controls-miter-05，不能继续宣称当前工作区完全没有候选运行，也不能在未核完其身份与比较前提升为已验收结论。
本次只验收rollback-02，不把后续文件名、单个参数实验或并行代理口头进度写成根因已证明/正式闭环。

## Closed / Open 与正式合同

有限已证：真实模块/resolver身份、初始化越过、primitive经DrawGeometry接入、固定场景Render/CopyPixels非空成功、B9CC对应full/related原物。
仍Open：12pixel严格差异根因与修复、完整primitive/animated矩阵、成功/失败/取消/线程/callback/OOM及逆序释放、最终同SHA本族/related/full/strict与独立E2E完整验收。执行Green不是E2E-05完成，32 fail不得无证据全归环境。
[四外部输入](../all-completion-external-prerequisites.md)仍为历史authority、ARM64工具链+真实环境、完整10.0.12 runtime、player协议+ownership+oracle；不阻断10.0.1建设版自主推进，不豁免最终门。
[DEC-0004](../decisions/DEC-0004-process-lifetime-module.md)：DLL进程寿命驻留，不卸载/重载/热替换；运行JSON也记录native module retained until process exit。资源Release、channel关闭、线程退出、callback静止及初始化失败清理仍须验收。
[七阶段总计划](../remaining-gap-closure-plan.md)、[A/B/C主题合同](../plan-module-contracts.md)、[独立E2E-01–09合同](../e2e-executable-contracts.md)原条件不变；正式总门Red，无整体完成或替换资格。
