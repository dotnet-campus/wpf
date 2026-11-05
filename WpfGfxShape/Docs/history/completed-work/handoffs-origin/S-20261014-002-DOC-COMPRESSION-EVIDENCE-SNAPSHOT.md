# 文档压缩与证据快照（非生产完成）

## 维护范围与时间边界

本次仅维护 Docs：压缩四份恢复文档并新建本记录；未修改业务代码、测试、配置，未运行终端、脚本、发布或测试。只读取现有证据。阶段4仍 Active，A/B/C未关闭，不能替换原库。本记录冻结本次维护事实，不授权历史下一动作。

执行代理最新交接：生产 publish 成功，SHA256 `03C7CD3B6EFAB3EF4696A032B687EDAF69594EC06CC87C46A801EA1BEF6DEEE8`。维护未取得完整发布命令、序号及日志，不补造；由执行代理最终交付记录补齐。该产物先于最后源码编辑，不能作为最终源码编译成功证明。

截至本次维护，执行代理报告 `PresentContract.cs` 未完成重构：移除 staging 声明后 finally 仍保留引用，当前源码存在编译错误，暂待另一代码代理修复集成。此处不是永久编译坏结论；修复后短文档应整节替换为最终实测身份。本维护未读取或修改该代码。

## 最新证据读回

以下路径相对 WpfGfxShape/；仅读已有文件，没有重跑。

| 证据 | 计数（total/pass/fail） | RunID | 读回位置 |
|---|---|---|---|
| 沙箱短基线 | 21/21/0 | fdcb6ec0-a1d3-4cc0-b473-ff9db6f0fbfa | artifacts/sandbox-readback/hw-contract-baseline/TestResults/hw-contract-baseline.trx：2、190行 |
| 沙箱扩展 | 25/25/0 | eb41e5f0-1604-4e94-919d-fd1f81624bcc | artifacts/sandbox-readback/hw-contract-expanded/TestResults/hw-contract-expanded.trx：2、260行 |
| 本机COM full | 1384/1372/12 | a4500e95-af17-4058-87ca-dae6d1f370e1 | artifacts/bin/WpfGfxShape.ComAcceptance/Release/net10.0-windows/TestResults/gpu-readback-local-full.trx：2、10529行 |
| 沙箱COM full | 1384/1355/29 | 74b6e211-1980-4fe9-95a1-51e2b21f232a | artifacts/sandbox-readback/hw-contract-full/TestResults/hw-contract-full.trx：2、10608行 |

四份计数均 executed=total、notExecuted=0；扩展与full各自 Results/aot-identity.json 已读回同一 SHA03C7CD3B...、Sandbox=true。生产DLL身份相同不证明测试程序集相同，更不证明其后源码相同。本机full RunID与计数已独立回读；不以相邻单一identity文件替代TRX身份。

普通4211pass/1fail/4212由执行代理交接：固定原版10.0.12 runtime DLL缺失；旧调查记录有加载0x8007007E前置证据，不能改golden或自动skip。GenProto调用成功，但本轮成功计数待读；旧23/23不当作本轮计数。

执行代理观测：原生 staging 颜色正确；全量 frontbuffer/GetPixel 黑；短矩阵生产像素可见。呈现环境不稳定，尚未定根因。短矩阵可见纠正旧“所有生产输出未知”的绝对说法，但不关闭全量失败，也不能把失败全归环境。客户端Clear/staging仍不等于生产target/cache输出。

## 移出的旧恢复点与追溯

旧结论从当前恢复文档移出，不删除正式契约或冻结归档：

- next-session-handoff 原第6项的长WIP、18/3/21及旧full流水改为纯导航；current-work-item 的恢复身份与旧运行细节合并到唯一基线摘要和本快照，保留完整模块验收条件。
- current-stable-baseline 旧1351pass/29fail/1380、RunID5326abf5-520a-4725-9a5f-b595e87699b8及沙箱18pass/3fail/21、RunID6c3eed7f-16ab-42af-8345-9baf4265b0b6，是此前包结果，不再称最新。原始说明仍见 [INCOMPLETE报告](../../investigations/INCOMPLETE-HW-CACHE-CLOSEOUT/report.md) 与 [证据读回](../../investigations/INCOMPLETE-HW-CACHE-CLOSEOUT/evidence-readback.md)。注意同名本机gpu-readback-local-full.trx现已读到新RunID，不用旧归档行号声称当前文件仍是旧运行。
- remaining-gap-closure-plan 原1–109行堆叠的GenProto、COM失败、76DWORD Visual/Cache、sampler、media、HWND与D3DImage“最新/当前/必须优先”恢复段移为历史引用。对应原档仍在completed-work，不改档：S-GENPROTO-EVIDENCE-DELIVERY、S-20261013-COMFULL-FAILURE-READBACK、S-20261013-VISUAL-CACHE-76DWORD-HANDOFF、S-20261013-IMAGE-SAMPLER-REALIZATION-PARTIAL、S-20261013-SAMPLER-PROTOCOL-BASELINE-REVALIDATION；S-20261012系列的PRESENT-OCCLUSION-NATIVE-CONTROL、SHADER-CACHE-PUBLIC-HWND-REVALIDATION、MEDIA-LIFECYCLE-FINITE-CLOSEOUT、MEDIA-REBIND-PARALLEL-FULL、MEDIA-ASYNC-NOTIFICATION-RETEST-RED、MEDIA-EVENTPROXY-LOCKFREE-PARTIAL、SYSTEM-VIDEO-SAMPLE-PRODUCTION-PARTIAL、MANAGER-ABI-FULL-REVALIDATION、MEDIA-EXPORTS-SYSTEM-SOURCE-PARTIAL、HWND-FOCUS-PRESENT-ROOTCAUSE、D3DIMAGE-CALLBACK-REENTRANT-CLOSE、MANAGER-AVAILABILITY-BOUNDS-BUILD、D3DIMAGE-DIRTY-FULLPIXEL-REVALIDATION。
- 总计划原“最新有限差集”SHA E798360D...、4199/1346全过只属于 S-STAGE4-A-INVOCATION-IDENTITY-SYNC；不再覆盖当前失败。原DrawingImage长描述压成严格FLOAT/HDR、坐标链、层级、网格与生命周期门禁；细节见 S-20261012-MASK-ROLLBACK-DECODER-STABILIZATION。
- 旧SHA271E8FB7...的沙箱1343pass/22fail/1365与14窗口/双输入、8视频失败保留为未关闭历史门禁，不冒充最新29fail，也不假定两组逐项相同；包工程交付见 S-UNDATED-001-SANDBOX-PACKAGE-FINAL-CLOSEOUT。
- 稳定SW SHA20221306...及其本机17/17、1376/1376、沙箱SW17/17仍只属于 [SW历史稳定档](S-20261014-001-STAGE4-CACHE-TRANSACTION-STABLE.md)。原普通4212、GenProto23为历史交接，不升级到本轮。

删除旧当前性措辞的依据：上述新TRX计数与RunID、执行代理的发布/源码时间边界及新观测已取代旧恢复身份；不是因为门禁已通过。早期media“仍E_NOTIMPL”、HW“文件为空/仅D3DImage”、HWND“根因已永久关闭”等相互覆盖结论不再放在当前计划，须按有限交付原档和当前真实证据判定。所有 frozen completed-work、progress-completed-work、progress-hw-rendertarget 及旧调查报告保持原位未改。

## 回读校验与长度

四份短文档及本快照已回读；Markdown链接按所在目录解析，对照已读取文件与Docs目录清单核验，未新增悬空导航。当前恢复文档只有一节“最近结果”和一个“唯一下一完整模块”，历史标题不再参与当前导航。未运行发布或测试。

行数（含空行）：next-session-handoff 24→20；current-work-item 25→27；current-stable-baseline 30→36；remaining-gap-closure-plan 404→107；本快照新增51行。短文档增加少量行用于区分产物、源码与证据来源，不为压行丢契约；总计划移除297行重复流水。

## 仍有效的交付边界

总计划驱动跨阶段完整模块；阶段只是验收分类，不是阻止紧密耦合target/present/brush/cache生产工作的人为栅栏。MTP问题暂缓，不竞争入口、不抢主线。不虚构ABI、不改Host/slnx/runner，不以测试数量增长宣称生产完成。

模块仍须合并合法生产输出观测、HW+SW可达消费者、冻结ownlease、命中/失效真实CopyPixels与重绘计数、per-device key、首HRESULT、候选失败旧键恢复/重试、caller Release/callback替换/delete/close逆序释放和同最终产物发布/定向/full/白名单沙箱。精确CopyPixels与callback锁失败原生证据不豁免；B generation/per-device/BadShader/fallback/OOM与C ARM64/layout/X86/history/physical closure不自动满足。文档维护完成不等于该模块或生产完成。
