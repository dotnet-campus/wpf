# CY 当前源码 fill 恢复独立验收与最终交接

## 已取得任务与范围

已取得用户明确授权：补完 CX delivery 初稿，独立审读 artifacts/cx-fill-restoration 当前源码已完成运行，更新本报告及三份共享交接。主代理仅监督。本轮不修改生产、测试、项目，不使用终端或任何直接间接脚本，不新构建、发布、运行，不重跑已完成矩阵。

## 独立裁决

**当前源码三段恢复有限独立验收通过，应保留。原44/10在当前双架构已核有限场景修复；完整替换未完成。** 未发现新回归，不能因 CX 初稿滞后撤回通过修复。完整源码清单、实际SHA/MVID/版本/父路径表见授权补完的 [CX delivery](replacement-supervised-fill-restoration-delivery.md)，不是移绑历史归档。

## 设计与源码亲核

已读执行规则27行、完整方案120行、CW（截断补33–48）及CX初稿7行。NativeShapeReader恢复float设备乘加、半像素/28.4 TryRound，并真实消费SoftwareFillBezier.Flatten，65–88补读至末。Stroke TryRead的fillLocalFloat默认false，非fill仍原Add/Apply；Group ordinary path按fillPrimitive传参，group/combined递归、rectangle/ellipse curved保持CF。GeneratedNativeStroke全文56行确认TryCaptureFill→fillPrimitive链保持。未修改任何生产、测试或项目。

## 父与角色证据

根artifacts/cx-fill-restoration/。run-x64-02 task8c2daf9e393a4e6f85a3459974581884，Results/cx-x64/TestResults/child.trx末23/23/23/0；geometry-x64 task385b819965644d109ac98685fc3b1b33，Results/cx-x64-geometry/TestResults/child.trx末12/12/12/0；run-x86 taskb10fd8bf380b455395eed2b09fb6f4a1，Results/cx-x86/TestResults/child.trx末34/34/34/0。三份父监督各1–38全文：exited/Exit0、OutputCollected=true，无timeout/cancel/kill/drain timeout/error。父TRX读结果关键段和计数，未称定义/entry重复段全文审计。

x64父CQ5+CR1+BW1+CK10+Br5+格式1=23，另PublicGeometry12；x86父CQ5+CR1+BW1+CK10+Br5+PublicGeometry12=34。CQ5是四组fill及stroke父用例；fill206=10+178+2+16，stroke16不混入206。格式1不构成新产品门，Prgba128Float构造不支持，未取得FLOAT oracle。

四组fill的各official/candidate/official-repeat role-evidence.txt按1–3/4–6/7–9全文读完，含actual identity、监督与TRX计数摘录：每角色1/1 Exit0，无监督异常，同测试SHA/MVID。frames三角色另直接读TRX20–26；stroke baseline/candidate直接读identity1–14、监督23–38及TRX末计数。不是只看父结果；也不夸称所有角色原TRX每行全文。

actual候选x64 F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7，x86 7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA；同包Old/Native旧CF分别36353DDE3BA720B642A6F12CEE8AB0EDDA501BD7DC49F35270063480B8A0532A、C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB，actual确实不同。测试x64 SHA9BC9F796EA680A207B7E43999B895251E935343E55D63352BD5B36827490B2A5/MVID236b761c-ad2f-4292-9741-7a447b168465；x86 SHAAB92DF00DEBE205E06D3DAAFF1FA247DB47184CFE98185FDE56CFB845B004D68/MVIDf0813943-164f-4b7e-bc51-13e2a154e5e5。官方/repeat独立同版同架构.NET10.0.13/pointer8或4，完整官方SHA、版本与actual路径见CX表。

## comparison全文及关键结果

双架构Results/cq-fill-exact/{frames,device,precision,siblings}/comparison.json全部全文亲读，分别108/1788/28/168行。x64 device从1–60连续60行至1681–1740，再1741–1788；x86从1–45、46–105连续60行至1666–1725，再1726–1788。frames/siblings最多60行分段至末，无跳段，不仅看scenes.tsv。全206记录threshold0/exact/discriminating、差0/repeat0/非空/accepted。双架构stroke各1–60/61–87全文16差0非空，unchanged=true、baselineIsCurrentSnapshot=false；这是旧CF完整buffer不变记录，不代表stroke与官方所有场景等价。

precision candidate ellipse各1–30全文：rect(1,1,2^-20,16)、local M11=1.0000000596046448/OffsetX=-1、parent M11=1048576/OffsetX=8，32×24 stride128/nonempty。BW父Passed；precision两场景comparison差0。Br双架构0–4 evidence关键末节分别87–99、39–51、87–99、595–607、87–99亲核，均differences=[]/threshold0/nonempty，测试/官方身份一致；未全文再审所有trace。

PublicGeometry两架构12场景父结果核到normal/empty/degenerate/nonfinite/curves/complex/winding/transform/stroke/relative/callback-reject/null-callback。x86 normal业务1–20及x64 aot-identity全文核到对应新SHA。未独立重算24份业务大数组，不把路径SHA记录冒充每项actual模块枚举；Br没有候选actual加载身份不臆造。这一审计层级限制不虚构成生产新回归或泛化重跑门。

## 边界、未闭及下一步

大数组/像素/buffer SHA未独立重算，仅核已有原件记录。software RTB/hardwareVerified=false，不外推所有HFD/clip组合、大曲线、GPU、任意runtime或full。原44/10仅在当前双架构已核有限场景修复，nested/stroke其余差异不清零。
旧BDfull x64 1740/1699/41 Exit2、x86无父TRX不绑定新SHA。最终稳定源码Tests/ComAcceptance×x86/x64 Release四格完整适用full仍须做。GPU真实内容、loss/resize新帧、media、长期resource/线程/callback、通知、同步Event0、E_HANDLE、首次堆损坏、圆角/大shader等合同全部保留。
下一最小步在保留本修复基础上建立当前双架构ComAcceptance基线并定位剩余失败，不再撤回无回归修复或反复重做归档；本轮不执行运行。
本轮仅五文档交付：CX、本CY、next-session-handoff.md、replacement-supervised-session-summary.md、replacement-issues-and-plan-status.md。共享最新优先节更新当时状态并保留历史；文档核验不是新运行证据。
