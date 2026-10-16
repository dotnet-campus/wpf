# PublicAbi 完整方法族依赖与验收边界

> [DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)正式仅x86/x64全部矩阵，ARM64编译/布局/实机NA而非Passed。下文“四类外部”与“10.0.12必须目标”由[现行外部清单](all-completion-external-prerequisites.md)和[版本来源审计](windowsdesktop-version-provenance.md)覆盖；[DEC-0006](decisions/DEC-0006-installed-official-runtime-contract.md)已确定已安装官方完整runtime、同版同架构独立对照，不再待澄清patch，不抹除10.0.1原版Green，不批准整体完成。

本页维护方法族条件，不维护运行状态。身份、计数、strict 适用版本只见[状态源](handoffs/current-stable-baseline.md)。下列完成档均为历史有限证据，不拼成当前 SHA 定向执行。

| 方法族 | 证据定位与不可省略的剩余合同 |
|---|---|
| WICColorContext、分段命令/引用计数、bitmap/media 传输 | [历史交付](completed-work/Command-transfer-method-families-20261014.md)；完整绘制因果、批次中途失败/关闭无泄漏 |
| geometry、锁/通知 | [geometry](completed-work/Geometry-complete-method-family-run5-20261014.md)、[锁通知](completed-work/Lock-notify-final-20261014.md)；全资源/线程失败寿命不外推 |
| 系统策略、DLL资源、像素/tile | 有限导出/差分不替代政策真实消费、恢复、资源身份及寿命；旧 tile 证据不是新产物定向结果 |
| 3D bounds | [历史有限差分](completed-work/Four-3D-bounds-closeout.md)不替代完整消费者/跨架构 |
| glyph outline/release | [当前证据适用范围](handoffs/current-stable-baseline.md)；严格未初始化 padding raw 保留，复杂脚本/侧向 bidi/ClearType/动画/brush/仿射及通用异常/callback/OOM 全寿命仍须验收 |
| perf | 同步 signalPresent=false、异步 signal bit2；合法通知结构及失败恢复见[主题合同](plan-module-contracts.md)，旧短包 OOB 不作 oracle |
| HWND | 登记/注销及 DWM 提示有限交付不是完整绘制/ownership 证明 |
| MediaPlayer / MILMedia* | [最新E2E-08方法族报告](completed-work/PresentationCore-media-E2E08-finite-closeout.md)：两PCM AVI、四帧全字节、真实事件/替换/关闭及拥有链有限Closed；HW/持续/架构正式Open，系统媒体服务不替代候选UCE/VideoDrawing/RTB |
| record-packet player | 官方协议、dispatch、对象 destroy/ownership 与 oracle，见[外部输入](all-completion-external-prerequisites.md)；不猜、不用媒体对象或 E_NOTIMPL 替代 |

## Image/WIC、三Brush、Blur、nested 与 DropShadow 集成族

[DropShadow完成档](completed-work/PresentationCore-dropshadow-acceptance.md)收录有效Red E_NOTIMPL→final compare06固定28帧零差，以及最终同SHA Blur14/nested16/image10/primitive双角色四回归零差；五份比较JSON已完整回读，各finite合同Closed，不再回流旧WIP。原[Blur/nested完成档](completed-work/PresentationCore-blur-nested-acceptance.md)历史原身份/原合同不改。本次仅Docs，未新增代码、构建、发布、重跑或清理工件。

最终source/frozen/actual、COM related/full同SHA identity与TRX RunID/Counters已核；普通/GenProto只按真实历史原TRX列示，不冒充最终AOT新跑。精确身份/独立分母只见[状态源](handoffs/current-stable-baseline.md)。COM失败按[当前矩阵33项分账](final-acceptance-remaining-matrix.md#33项失败分账)保留control premise与未到达消费者边界，不全归环境或候选bug；普通旧10.0.12路径加载异常保留原失败、现归自主工程修复。最新同3F291生产SHA strict已真实取得，仅缺MilPlayer_Create/Process、Extra0；不将该报告绑到其它生产SHA或最新smoke。

本族完整 **Open**：固定28不外推全部参数/动画/透明/格式、并发/OOM、完整refs/close或候选描画/核内fault；已有finally/owning源不替代独立寿命实证。历史生命周期36帧及目标锁12项有限Closed保留，calls=0不代核内恢复；不自造无原生依据的必需核内OOM门。有限像素不代完整方法族/ABI/strict，不为收口扩代码/测试。

后续调度统一服从[九包总计划](remaining-gap-closure-plan.md)：先R1自主解除历史runtime路径绑定，推进R2正式x86/x64全配置，并由R3建立有效HW control；随后R4合包验收 **A/B HW image/visual/cache真实消费者与失败恢复拥有链**，包括冻结/realize、真实成功像素因果、失效/key/version、失败保旧/恢复重试及逆序资源清理。本方法页不另设竞争下一包；不重领已闭softwareeffects，不以单个补测代模块。真实三项来源与x86/x64验收环境条件只见唯一外部页；七阶段与E2E正式总门仍 **Red**。已安装官方runtime同版对照合同已生效，不要求10.0.12，不宣称总计划完成。

完整族交付须原生合同、真实成功/边界/失败/关闭差分及最终同 SHA 相关/full/strict 原始证据。不降 required/raw/阈值/skip/golden，不把 PE 缺项减少当项目百分比。
[总计划](remaining-gap-closure-plan.md)保留七阶段/A/B/C/独立 E2E；[DEC-0004](decisions/DEC-0004-process-lifetime-module.md)规定DLL驻留、不unload，不豁免资源清理或最终兼容验收。
