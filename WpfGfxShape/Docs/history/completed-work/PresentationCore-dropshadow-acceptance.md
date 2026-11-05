# PresentationCore DropShadow 有限验收收口

## 结论与执行边界

DropShadow 固定28帧合同 **Closed**：有效 Red E_NOTIMPL → 最终 compare06 **28 Green / 0 Red / 28**，threshold=0、每帧4096 bytes、两角色像素 SHA 相等。
本次仅只读既有运行结果并编辑 Docs；未新增代码、构建、发布、重跑、终端脚本或清理工件。生产 build/publish/stage/双角色运行是既有执行，不冒充本次执行。
早期 `dropshadow-red-candidate-01/Results/candidate/drop-shadow-scenes.json` 明记 `0x80004001` / NotImplementedException；旧 compare02/03 是历史差异，不再作为当前状态。

## 最终 source / frozen / actual 身份

以下路径均相对 [证据根 R](../../Investigations/PresentationCore-render/Results/)，只引用保全，不复制或删除唯一 images/JSON/TRX。
候选 SHA256 **3F4B424933C5F11076CAE3F90D6E216295EEC7D1335AE558D00CA8E99786990F**，4462080 bytes。
`dropshadow-final-candidate-06/source-identity.txt` 第10行将 `artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll` 绑定上述 SHA；同包 manifest.candidateSha256 一致。
`dropshadow-final-candidate-06/Results/candidate/result.json` 的 requestedNative / frozenCandidate / actualNativeModules 三者均为此 SHA，stage=completed；不是只看 staging 文件名。
原版 SHA256 **94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F**，1951496 bytes；`dropshadow-final-original-06/Results/original/result.json` requested/actual 均为原版，frozenCandidate 为上述新候选，不是原版实际加载。
未改 PresentationCore SHA256 **605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654**；WindowsBase **A7BDC35EC37C1DEB33D24D97213BA1077E3051ABB66250913C9C1FA735B0D3BC**。
运行 X64 / .NET10.0.1，target10.0.12Acceptance=false；source-identity 的本机 SDK10.0.301/runtime10.0.13 不冒充实际沙箱运行版本。
发布完整命令/构建退出日志未在本次已核原物中取得，不反推或补写；生产产物身份与实际运行证据独立成立。

## 完整比较与角色路径

五份比较 JSON 已逐条回读，不以场景生成 Green 代替独立比较；各结果不相加成覆盖率。

| 合同 | R 下比较 JSON | 结果 |
|---|---|---|
| DropShadow | `dropshadow-final-compare-06/Results/diff/nested-comparison.json` | 28/28零差 |
| Blur | `shadow-reg-blur-compare-06/Results/diff/nested-comparison.json` | 14/14零差 |
| nested | `shadow-reg-nested-compare-06/Results/diff/nested-comparison.json` | 16/16零差 |
| image | `shadow-reg-images-compare-06/Results/diff/image-comparison.json` | 10/10零差 |
| primitive | `shadow-reg-primitive-compare-06/Results/diff/comparison.json` | byte-exact Green；4096 bytes |

DropShadow十四组各两帧：zero、radius2、fractional、q1/q2/q3/q4、depth0、opacity0、opacity-half、color、quality、transform-clip、change；参数变更包含 radius/depth/direction/color/opacity 的既定输入，不外推全参数空间或全动画。
四回归分别以 `shadow-reg-{blur,nested,images,primitive}-{original,candidate}-06/Results/{original,candidate}/result.json` 记录独立角色；候选 requested/frozen/actual 均为上述候选 SHA，原版 requested/actual 为上述系统 SHA，原版包 frozenCandidate 同为新候选。
输入映射保全 `dropshadow-final-candidate-06-input-identities.txt` 与 `shadow-reg-input-identities-06.txt`；比较器 Baseline / ComparisonCandidate 是输入副本，不是加载候选的运行角色。
原始像素/图像保全上述各角色 `Results/original/`、`Results/candidate/` 下 `shadow-*.pbgra32`、场景 `.pbgra32`/`.png`；primitive 为 `pixels.pbgra32`、`image.png`。primitive 双方像素 SHA 为 `7F55E138C9605464AE90811BD28B5477871BB3E1AA71F5FD3A1C8B052E0C308B`。

## TRX RunID / Counters / identity 分账

| 原物（R 下） | RunID | passed / failed / total | 身份界限 |
|---|---|---|---|
| `dropshadow-com-related-06/Results/related.trx` | `ec96419d-c435-41e8-9a96-cbd8d1e42467` | 985 / 0 / 985 | 同目录 aot-identity.json 为最终候选 SHA |
| `dropshadow-com-full-06/Results/full.trx` | `02989116-9826-4b2e-9ad6-52554dc368d4` | 1427 / 32 / 1459 | 同目录 aot-identity.json 为最终候选 SHA；一次实际 full，非历史代填 |
| `lifetime-managed-06/lindexi_DESKTOP-51A5UGG_2026-10-15_12_18_28.6041940.trx` | `f3969f52-63f2-48b0-a0f8-5991a9ea01c5` | 4224 / 1 / 4225 | 已有普通测试运行；不是最终 AOT 加载证明 |
| `lifetime-managed-06/lindexi_DESKTOP-51A5UGG_2026-10-15_12_18_40.7585933.trx` | `e5e0b63a-7b30-4075-b883-3fc253a5acaf` | 28 / 0 / 28 | 已有 GenProto 运行；不是最终 AOT/完整协议闭包 |

普通唯一失败 `WhenRealSystemMediaHasNoFrameThenSoftwareSampleLeaseCanBeClosed`：指定 `Microsoft.WindowsDesktop.App/10.0.12/wpfgfx_cor3.dll` 或其依赖加载异常，0x8007007E；不扩大断言为仅某一个文件缺失。
COM full 保留32真实失败；已核例 `WhenStage4HardwareCacheGeometryDrawsThenHitAndMutationPreservePixelsAfterCallerRelease` 的 native control 为黑像素、conditionSatisfied=False，未进入目标cache验证；不能全部归环境，也不能推断cache已通过。
本证据集唯一明确未取得的最终同 SHA 验收报告为 **strict Missing/Extra/AllAbiPassed**；历史缺player两入口不得填作本 SHA 新结果，不因这项缺证据阻止上述有限实证收口。普通/GenProto只按表中真实原物列示，不伪造新的 shadow 回归 TRX。

## 通用算法修正与原生依据

生产修正不是28帧贴图或场景特判：`GeneratedVisualRenderer` 以内容 bounds 与 shadow方向边界+卷积radius padding 建立中间面，再通过输入/输出变换及clip合成，避免紧裁剪丢掉卷积需要的透明输入。
`GeneratedSoftwareBlurEffect` Gaussian 权重修正、逐pass字节量化/ToEven及零边界中间面对应原生 CPU 卷积；Box采用完整二维窗口累计后量化，不把两次一维量化混用。
shadow depth 乘最小轴 scale 后按direction计算float偏移再截断；颜色/opacity按字节量化，与保留原图的shadow合成次序一致。visual透明度在effect输入与effect后layer处理不能互换；shadow避免重复应用visual alpha，不能把不透明背景的成功外推所有透明场景。
原生依据：`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/DropShadowEffect.cpp` 的 ApplyEffectSw、GetLocalSpaceClipBounds/TransformBoundsForInflation、CalculateOffset（855–895行明确最小轴缩放），以及 `BlurEffect.cpp` 的 ApplyEffectSw/CalculateGaussianSamplingWeightsFullKernel；生产对应 `Core/GeneratedVisualRenderer.cs`、`GeneratedSoftwareBlurEffect.cs`、`FrozenDropShadowEffect.cs`、`GeneratedEffectResources.cs`，本次只读未改。

## Closed / Open 与下一模块

Closed 仅为上述固定shadow28和最终同SHA四回归有限像素合同；既有Blur/nested/image/primitive与生命周期36帧、目标锁恢复12项按各自完成档保留，不重领softwareeffects。
Open：完整refs/close、候选描画/核内部fault、通用动画/callback/格式/FLOAT/HDR、并发/取消/失败拥有链及完整目标矩阵；正常finally/退出叙述不替代逐对象引用证据。目标锁失败calls=0不证明进入卷积核；无原生依据不新造“并发OOM必须全Closed”条款。
父级继续委派的下一有限总目标为 **A/B HW image/visual/cache真实消费者与失败恢复拥有链**：整合冻结/realize、成功像素因果、失败保旧与重试、失效及逆序清理；不把补测或已闭softwareeffects重领成新模块。
[七阶段总计划](../remaining-gap-closure-plan.md)、[最终矩阵](../final-acceptance-remaining-matrix.md)、[E2E-01–09](../e2e-executable-contracts.md)不减；[四外部输入](../all-completion-external-prerequisites.md)仍唯一页；[DEC-0004](../decisions/DEC-0004-process-lifetime-module.md)DLL驻留、卸载/重载NA，不豁免资源/线程/callback清理。正式总门 **Red**，不宣称总计划完成。
