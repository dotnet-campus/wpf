# PresentationCore Blur / nested 有限集成验收

## 范围与身份

本次仅只读审计现成生产源码、既有测试、JSON/TRX/identity并同步文档；已先实际写中间状态再完成此档。不新增代码/测试范围、不改生产算法、不构建/发布/重跑、不执行终端脚本、不制造执行包、不删除工件。本模块集成完成不等于父级全任务完成。已有发布不是本次发布，不编造发布命令/序号。

候选 SHA256 `161496D655519FE9964DF2298F995235EB71E3D026341E1B76AEA1DFBDA75BB8`，4456448 bytes。`blur-candidate-02/Results/candidate/result.json`、`final-candidate-{nested,images,primitive}-04/Results/candidate/result.json`的requestedNative/frozenCandidate/actualNativeModules相同，stage=completed、status=Green、exitCode=0；两个COM `Results/aot-identity.json`同SHA。

真实Windows x64、.NET10.0.1，未改PresentationCore SHA `605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654`，原版native SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`；target10.0.12Acceptance=false。各compare的source-identity.txt内SDK10.0.301/本机10.0.13是打包身份，不冒充实际执行runtime。

现成 `Tests/WpfGfxShape.PresentationCoreE2E/FinalAcceptance.proj`固定候选SHA并复制独立输入，`Investigations/PresentationCore-render/FinalRegression.proj`指定普通/GenProto回收；本次只读，不执行或修改。

## 完整比较回读：有限条款逐项 Closed

以下证据路径均相对 `Investigations/PresentationCore-render/Results/`。四份diff JSON首尾已完整回读，数组每项Green、threshold=0、originalBytes=candidateBytes=4096、differingBytes=0、像素SHA相同。candidate的PendingIndependentComparator不是最终结果，闭合依据是独立diff。

| 原物 | passed / failed / total | 有限结论 |
|---|---|---|
| `blur-compare-03/Results/diff/nested-comparison.json` | 14 / 0 / 14 | 七组各两帧逐项Closed |
| `final-compare-nested-04/Results/diff/nested-comparison.json` | 16 / 0 / 16 | 八组各两帧逐项Closed |
| `final-compare-images-04/Results/diff/image-comparison.json` | 10 / 0 / 10 | 十场景逐项Closed |
| `final-compare-primitive-04/Results/diff/comparison.json` | Green、byte-exact、Equal、threshold=0 | 固定primitive Closed |

Blur组名为 `visual-blur-{名称}-0/1`，每项4096 bytes：

| 名称 | 两帧coloredPixels（candidate原始JSON） | 状态 |
|---|---|---|
| zero | 484 / 484 | Closed |
| gaussian | 665 / 665 | Closed |
| box | 676 / 676 | Closed |
| quality | 665 / 665 | Closed |
| box-quality | 676 / 676 | Closed |
| change | 665 / 777 | Closed；固定参数变化不外推全动画 |
| fractional | 665 / 665 | Closed |

nested组名各带-0/1：image-geometry、drawing-geometry、visual-geometry、image-mask、drawing-mask、visual-mask各484/484；drawing-nested为441/441；visual-effect为665/665。八组两帧各4096 bytes、均独立零差Closed。旧14非空/2缺帧及Blur E_NOTIMPL只属历史，不再代表当前候选。

image：bitmap-direct coloredPixels=576；image/drawing/visual各basic=256、tile=1024、matrix-clip-opacity=224。十项各4096 bytes、opaquePixels=1024且非空彩色，白背景opaque不外推一般透明场景。primitive像素SHA `7F55E138C9605464AE90811BD28B5477871BB3E1AA71F5FD3A1C8B052E0C308B`，4096 bytes。独立分母不相加，不继承历史miter控制为当前SHA实测。

## 原始 TRX / RunID / Counters

| 原物 | RunID | passed / failed / total |
|---|---|---|
| `final-com-full-04/Results/blur-full-04.trx` | `446ad623-a4c5-46ae-ac0b-969d68792f77` | 1415 / 32 / 1447 |
| `final-com-related-04/Results/blur-related-04.trx` | `50e6e82c-eab8-47af-83a4-1bfb50ed4eb6` | 11 / 5 / 16 |
| `final-managed-04/lindexi_DESKTOP-51A5UGG_2026-10-15_11_48_09.7419501.trx` | `938df53b-1c3a-4cb9-94bb-fec8eda69363` | 4224 / 1 / 4225 |
| `final-managed-04/lindexi_DESKTOP-51A5UGG_2026-10-15_11_48_20.7965264.trx` | `2e957101-833b-4cc9-ab6c-3666e8e56a20` | 28 / 0 / 28 |

四份均executed=total、notExecuted=0，error/timeout/aborted=0。COM full/related实际identity同上述SHA。related filter为Generated/Frozen/ImageBrush/VisualBrush/DrawingBrush，不是独立Blur故障验收。HW失败所见PresentCondition conditionSatisfied=False、control像素不满足前提；不能把前提失败当目标cache验证通过，也不把全部32失败一概归环境，不重跑追Green。

普通唯一失败 `WhenRealSystemMediaHasNoFrameThenSoftwareSampleLeaseCanBeClosed` 为DllNotFoundException：`C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\10.0.12\wpfgfx_cor3.dll`或依赖，0x8007007E；未区分本文件与依赖谁缺。普通/GenProto是托管测试，不冒充加载该AOT SHA或目标runtime验收。

## Blur owning/finally 与既有测试审计

只读当前 `Code/WpfGfxShape/Core/`：

- `GeneratedSoftwareBlurEffect.cs:17–58`：source/output置零，Realize/CopyPixels/创建/Draw失败传播；具体OOM/overflow映射；finally先Release(output)再Release(source)。`:131–136`幂等Dispose输入snapshot。
- `GeneratedVisualRenderer.cs:16–67`：root.AddRef，FrozenVisualTree在target COM前捕获；draw失败AbortLayers；finally释放session、逆序draw effect/blur/brush/mask/video/source及root。
- `FrozenVisualTree.cs:57–61,95–105,248–266`：Own(blur)后复制finite radius/kernel/bias；失败candidate.Dispose；Own记录AddRef，Dispose逆序释放owners及子树/brush/geometry。

既有 `Tests/WpfGfxShape.Tests/GeneratedEffectResourceTests.cs:45–59` 两项在最终普通TRX的2278/2285行均Passed：`WhenBlurAnimationChangesThenNotificationPropagates`验证通知；`WhenBlurFamilyIsWrongThenOldStateAndReferencesRemain`验证malformed、旧Radius=4及引用数=2。这两条有限资源合同Closed，不代表Blur Draw故障或最终AOT引用寿命。

related已有 `WhenStage4CacheDecoderReentersThenFrozenOwnersSurviveReplacementDeletionAndClose` 是decoder/cache拥有链，不是Blur专用。正常cleanup文本的DrawingContext disposed/Dispatcher shutdown/finalizers drained/native retained不是引用平衡独立量测。

**Open**：Blur通用故障注入、Realize/CopyPixels/创建/Draw失败时独立引用平衡、callback重入/取消/线程/OOM和完整拥有寿命。源码审计不等于各分配/转移边界无泄漏实证；没有独立运行证据不Closed，不为收口临时新增代码或测试。

## 整体边界、工件与下一模块

阶段3整体、A/B、阶段4–7和E2E-01–09完整正式矩阵仍未闭，正式总门 **Red**，无全任务完成或替换资格。四外部输入的真正缺项继续仅见[唯一外部页](../all-completion-external-prerequisites.md)：历史authority binding、ARM64工具链/真实环境、完整10.0.12、player协议/ownership/oracle。

必要证据限定上述四compare、`blur-original-01`/`blur-candidate-02`、`final-original-{primitive,images,nested}-04`/`final-candidate-{primitive,images,nested}-04`、`final-com-full-04/Results`、`final-com-related-04/Results`、`final-managed-04`及现有input identity。保留原JSON/TRX/identity/source-identity和唯一image/pbgra32；不新复制执行器依赖或Docs过程包。本次未删除，未开展新无界清理，不宣称现有目录已瘦身，不报告释放空间；生产DLL/正式publish不动。

[DEC-0004](../decisions/DEC-0004-process-lifetime-module.md)：DLL进程驻留、不卸载，资源/线程/callback清理不豁免。

下一生产模块限定既定A/B合同的 **HW image/visual/cache消费者与失败恢复拥有链闭环**：成组交付真实生成资源消费、候选失败保旧/恢复重试、目标像素因果与引用清理及最终同SHA证据，不退回重跑本页像素或单独补测作为模块。此为父级继续工作的有限候选，不声称已经实现或全任务结束。
