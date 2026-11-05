# ImageBrush / PresentationCore 集成有限验收

## 结论与身份

2026-10-15 本轮运行现有最新 E2E，读取结果并完成文档收口；固定十场景有限里程碑 **Closed**。完整消费者、callback、动画、全部格式及失败/取消/线程寿命仍 **Open**；正式总门 **Red**，无整体完成或替换资格。

生产源码及生产 DLL 本轮未修改、未发布。冻结候选 SHA256：`B29395B8CE882E97EE569E8D32EB3630FDD0C7AB2A67E115092F1C6FA14AA050`，4447744 bytes；最新候选 result 的 requestedNative/frozenCandidate/actualNativeModules 三者一致。原版 native SHA 为 `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`。真实未改 PresentationCore 为 10.0.1，SHA `605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654`；target10.0.12Acceptance=false，不冒充目标版本验收。本轮不重新构造生产发布序号或命令。

## 最新十场景：已运行、已完整读回

原物根目录为 [PresentationCore-render/Results](../../Investigations/PresentationCore-render/Results/)，下文路径相对此根。运行现有 `WpfGfxShape.PresentationCoreE2E.exe` 的 `original images`、`candidate images`、`compare-images`，未增加源码或测试范围。

- [原版 result](../../Investigations/PresentationCore-render/Results/image-original-10/Results/original/result.json)：任务 c30f1074effb4f1e8ed9579b6fb7e1d6，stage=completed、Green、exitCode=0，实际模块为原版。
- [候选 result](../../Investigations/PresentationCore-render/Results/image-candidate-10/Results/candidate/result.json)：任务 377989f2067342239aa3a0df84af8965，stage=completed、Green、exitCode=0，实际模块为 B293。
- [原版像素计数](../../Investigations/PresentationCore-render/Results/image-original-10/Results/original/image-scenes.json)、[候选像素计数](../../Investigations/PresentationCore-render/Results/image-candidate-10/Results/candidate/image-scenes.json)及[独立比较完整 JSON](../../Investigations/PresentationCore-render/Results/image-compare-10/Results/diff/image-comparison.json)均已完整读回。

| 场景 | 原版/候选 opaquePixels | 原版/候选 coloredPixels | bytes（两端） | threshold / differingBytes |
|---|---|---|---|---|
| bitmap-direct | 1024 / 1024 | 576 / 576 | 4096 | 0 / 0 |
| image-basic | 1024 / 1024 | 256 / 256 | 4096 | 0 / 0 |
| image-tile | 1024 / 1024 | 1024 / 1024 | 4096 | 0 / 0 |
| image-matrix-clip-opacity | 1024 / 1024 | 224 / 224 | 4096 | 0 / 0 |
| drawing-basic | 1024 / 1024 | 256 / 256 | 4096 | 0 / 0 |
| drawing-tile | 1024 / 1024 | 1024 / 1024 | 4096 | 0 / 0 |
| drawing-matrix-clip-opacity | 1024 / 1024 | 224 / 224 | 4096 | 0 / 0 |
| visual-basic | 1024 / 1024 | 256 / 256 | 4096 | 0 / 0 |
| visual-tile | 1024 / 1024 | 1024 / 1024 | 4096 | 0 / 0 |
| visual-matrix-clip-opacity | 1024 / 1024 | 224 / 224 | 4096 | 0 / 0 |

每项均 Green、32×32、96 dpi、Pbgra32，逐项 SHA 相等。源码明确先绘制 32×32 白色背景，故本固定合同的 1024 opaque 有依据；不能把此约束外推合法透明背景场景。非空彩色检查必需且已满足，无须修断言。候选自身 PendingIndependentComparator 由独立比较结果闭合，不以进程 Green 替代比较。

输入为非空 8×8 Pbgra32，冻结 ImageDrawing，DrawingVisual DrawImage；ImageBrush/DrawingBrush/VisualBrush 基础、FlipXY tile、平移 matrix、双层 clip、opacity=0.5 属本有限矩阵。image-07 历史 drawing/visual 六项 E_NOTIMPL Red 不覆盖；image-09 十项零差原物继续保留，本轮以 image-10 最新 Program 像素计数及比较收口。

## 同 SHA primitive 与现有 miter 控制

已读 `image-primitive-original-10/Results/original/result.json` 与 `image-primitive-candidate-10/Results/candidate/result.json`：均 completed/Green/exit0、4096 bytes；候选 requested/frozen/actual 均 B293。运行既有 `compare`，并完整读回 [独立比较](../../Investigations/PresentationCore-render/Results/image-primitive-compare-10/Results/diff/comparison.json)：Green、byte-exact、threshold=0、Equal；两端像素 SHA 同为 `7F55E138C9605464AE90811BD28B5477871BB3E1AA71F5FD3A1C8B052E0C308B`，严格零差。比较器不加载 nativeModules。

运行现有 TriangleControls candidate 时首先发现执行包仍为旧 2F83；结果保留在 `image-miter-controls-10`，不拼作当前证据。仅将现有冻结 B293 DLL 同步至已有执行包，再运行并完整读取 [当前 controls.json](../../Investigations/PresentationCore-render/Results/image-miter-current-10/Results/candidate/controls.json)：nativeSha256=B293 完整 SHA，fill/stroke/both × Miter/Bevel/Round × aliased false/true 共18项，全部 count=0、differences=[]。沿用已有原版 Baseline，不新增控制场景或工具。历史692 primitive 分母不冒充本轮 B293 定向运行。

## 已有回归 TRX、RunID 与失败边界

本轮读取既有普通/GenProto及同 B293 COM 原物，不重跑 full 碰运气。各分母独立，不求和。

| 原始 TRX（相对 Results） | RunID | pass / fail / total |
|---|---|---|
| image-managed-10/lindexi_DESKTOP-51A5UGG_2026-10-15_09_57_35.8665773.trx | 16fd07df-3a81-44bf-934d-5a7aef7c0ee5 | 4224 / 1 / 4225 |
| image-managed-10/lindexi_DESKTOP-51A5UGG_2026-10-15_09_57_45.3138808.trx | 7399cfdb-56c1-4470-8c2b-2ce0c467076a | 28 / 0 / 28 |
| image-related-09/Results/Administrator_EN-DEV-01_2026-10-15_09_44_50.1151762.trx | bb43384e-c95d-48c1-aeba-a38599aa433a | 11 / 5 / 16 |
| image-full-com-09/Results/Administrator_EN-DEV-01_2026-10-15_09_46_06.7163154.trx | 78871899-d4f3-43c0-abc6-de6ce2136d33 | 1415 / 32 / 1447 |

上述 counters 均 executed=total、notExecuted=0；失败轮 ResultSummary=Failed、exit2，GenProto Completed。更早同目录普通 RunID 3c30f07b-fca1-4a6d-a2d5-415d4c9bc3a6 与 GenProto 912e7b79-aa3f-4724-a925-a56be587180d 为独立既有重复轮，不累计分母。

普通唯一失败为 `WhenRealSystemMediaHasNoFrameThenSoftwareSampleLeaseCanBeClosed`，原始异常 DllNotFoundException 指向 `C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\10.0.12\wpfgfx_cor3.dll` **or one of its dependencies**，0x8007007E；不能据此断言究竟本文件缺失还是依赖缺失。普通/GenProto不由这些数字证明加载 B293。

related/full 的 `Results/aot-identity.json` 已完整读回，均 B293、Sandbox=true；related filter 为 ImageBrush|DrawingBrush|VisualBrush|Frozen，其16项不是完整 ImageBrush 生命周期矩阵。related五败是 HardwareLazyWicReenters 的 action 0–4，在真实 nativeControl 呈现前提像素处失败。

full32失败继续按已有 control premise 与尚未进入目标 cache 验证的边界保留：native d3d9 control 的 clear/cooperative/rawPresent/flush 可返回成功，但 pixel=0、conditionSatisfied=False；这类前提失败不能当作 cache 命中/失效验证已完成，也不能一概归因生产或环境。其余 HWND/D3DImage、前缓冲、双输入/呈现像素失败仍是原始失败，不因固定软件十场景零差而消失。未确认逐项根因，不伪造更细归因或宣称32已修复。

## 操作与未闭合同

仅运行现有程序，使用既有 MSBuild 复制/哈希验收配置绑定 image-10 和 primitive-10，并同步现有控制包候选；构建成功。不增加代码、测试或能力，无终端脚本，无生产发布。Results 仅回收，不制造 Docs 过程副本，不删除唯一结果，不扩展清理任务。

result cleanup 明确 DrawingContext dispose、bitmap/visual 引用释放、Dispatcher shutdown、finalizers drained，native module retained until process exit。[DEC-0004](../decisions/DEC-0004-process-lifetime-module.md)：DLL驻留、不unload；不豁免资源、失败、取消、线程或 callback 清理。

Closed 仅限此固定十场景及本次同 SHA primitive/18控制实际结果。Open：全部格式、动画、callback、完整消费者/失败寿命、HW/cache/shader与正式E2E-01–09矩阵。[七阶段](../remaining-gap-closure-plan.md)整体未闭；[四外部输入](../all-completion-external-prerequisites.md) authority、ARM64工具链/真实环境、完整10.0.12 runtime、player协议/ownership/oracle仍Open，正式总门Red。
