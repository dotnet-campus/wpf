# Frozen consumer 正式发布与全量复验（阶段4 Active）

## 本轮范围

已有 main renderer FrozenVisualTree consumer、nested DrawingImage containers 当前代码已正式编译并发布。此次新增 FrozenShaderInvocation 的 invocation-wide ImageBrush sampler 参数/源 pin/DrawingImage pure tree 快照，SW/HW 两消费者复用同一 FrozenBrush，不在 realization 中重新捕获 live ImageBrush。捕获失败在 realization 前按 HRESULT 传播；DrawingImage sampler 复用 active 集合避免递归捕获重置循环检测。软件 sampler 失败释放改为逆序；invocation 先释放 sampler 快照，再逆序释放原资源 leases。

未新增生产类型；未修改 cache key/version 事务、测试 golden 或阈值。未新增测试。现有测试通过仅证明当前回归集，不证明整树冻结所有合同已经完成。

## 发布与测试身份

发布1（已有 consumer 当前源码）：
`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -o WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64`
成功；普通4212/4212，COM1346/1346，0失败/跳过。
COM RunID c82018f9-af18-4b41-bd42-430805f2f00c。此运行 TRX 后被最终 full 覆盖，初次 SHA 未完整回读，不作为最终稳定身份。

发布2（最终生产代码，所有上述变更之后）：
`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -o WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64`
成功。项目 Code/WpfGfxShape/WpfGfxShape.csproj，配置Release，RID win-x64。
产物：WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
SHA256：64840BEB34A43B95BF08EB73A84F35E1DCA2ABA00C689BD2B54EADEF033E764B。

最终 ordinary full：4212通过，0失败，0跳过；最终日志56–60行已回读。
最终 COM full：1346通过，0失败，0跳过；完整17行日志已回读。
TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
RunID：c2de8faf-2f3b-451a-b9ab-a1678283585a。
start：2026-10-13T18:01:31.8378465+08:00。
finish：2026-10-13T18:06:08.6866714+08:00。
TRX 50行实际回读 SHA 与最终产物一致。
独立 report：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-f38f2bf083c745c18c74bbc78c589c45.json；TRX 844行关联，report 4行 AotDllSha256 回读同 SHA。Compared=450；cross-format 为诊断而非同格式 float oracle，不据此关闭剩余合同。

构建错误发现：首次误选根WPF方案出现1个 TraceWpp.exe 缺失错误；随即选择真实独立生产项目，Release build成功，无 CS error。最终发布与两次full均成功，最终测试失败0；不存在已发现但未修复的新测试失败。构建保留既有 analyzer warnings，未扩大修复范围。

## Mapping / ledger / stable work item

- FrozenShaderInvocation → invocation 参数/引用、ImageBrush sampler pure capture；SW GeneratedSoftwareShaderEffect.Capture 与 HW GeneratedHardwareShaderEffect.RenderFrozen → CaptureFrozen 消费。当前只冻结 ImageBrush sampler，不能将 VisualBrush/BitmapCacheBrush live 分支算完成。
- FrozenVisualTree.CapturePure → 主软件 renderer 指令/children 消费，Nested DrawingImage FrozenBrush.Drawing → DrawingSnapshot.RealizeTree；当前既有消费者已 publish/full验证。
- cache 后验版本事务未改，当前不新增 cache CopyPixels 计数证据。
- Stable work item：Stage4-A-FrozenGraph-Realize，Active。稳定身份仅为本文件最终发布2的 SHA/RunID/report；ledger与handoff引用本文件。

## Remaining gaps（有限具体 patch 点）

1. GeneratedHardwareVisualRenderer.Render/Visit/DrawContents 仍 live 遍历整棵 visual；需要以 root FrozenVisualTree 实际消费拓扑、bounds与input scope，不能只有effect invocation冻结。
2. FrozenShaderInvocation 对 VisualBrush/BitmapCacheBrush 尚未 pure owning capture；GeneratedSoftwareShaderEffect.Capture 与 GeneratedHardwareShaderEffect.RenderFrozen 的非 ImageBrush sampler 仍走 live Capture。应冻结其目标整树与 cache key/version 选择，在回调前完成全图纯捕获，同时保留realization后版本校验，不将newversion贴oldpixel。
3. 真实公开 lazy WIC callback 改后 child/brush/geometry + release/clear/delete：新固定 current-frame旧值、next-frame新值测试尚未新增。继续复用 ManagedStreamTests.Lifecycle.cs 的真实 MemorySource/BeforeRead source helper和真实WIC decoder，不fake/内部直调。已有 ActiveRender 生命周期测试通过不等于这组新合同覆盖。
4. decoder Read callback 计数不是 CopyPixels 计数；cache hit/invalidation/retry 的真实CopyPixels计数/失败首错验收仍缺。
5. B/C generation/per-device cache、BadShader通知/fallback/OOM有限合同与GENPROTO required evidence尚未关闭。

本次完成的是当前consumer可编译可发布与ImageBrush invocation接线的稳定回归交付；没有完成用户要求的新增公开callback fixture、整树Purefreeze全范围或B/C。阶段4不得Closed。最终生产代码之后已publish/full；之后仅修改证据文档。
