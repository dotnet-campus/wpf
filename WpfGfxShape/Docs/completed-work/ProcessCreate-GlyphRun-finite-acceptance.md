# ProcessCreate / GlyphRun 有限验收收口

## 文档交付与身份

本次仅依据已核证据落盘；没有新代码、运行、构建、发布、测试或终端脚本，不产生新 TRX，不需要为本次文档修改再测。
生产 SHA256：`57CE56EA65057F8F609AD56E318B459A4248790C1231B5909DDD4DEA6E0A6F74`。
本页承接[前轮 factory 分母详档](ProcessCreate-factory-denominator-20261015.md)，不修改其原始记录、旧 SHA 或旧运行事实。

## 原生语义与有限 Closed

真实 factory 分母仍为 97：76 个合法 case、21 个非 case 拒绝。不能把 GlyphRun 从合法 factory 分母删除以掩盖 dispatch 差异。
原生 `core/uce/generated_resource_factory.cpp:19–350` 确定合法 case、default malformed packet 与成功 AddRef；`htslave.cpp:33–108` 确定创建、初始化、入表及失败释放的原生顺序。
`composition.cpp:1470–1476` 的 ordinary Channel_CreateResource 对 GlyphRun 忽略；`1712–1778` 的显式 GlyphRun_Create 保留创建及 ProcessCreate、类型检查语义。
本轮已核修正按 native 忽略普通 create、保留显式创建；此前 3 Red 已进入最终 related **183/183**。该有限语义与 factory 分母接纳/释放 Closed，不外推所有 typed consumer 或完整失败关闭族。

## 最终运行与审计原物

下列原物均为既有执行结果；full、related 与下节四组独立 compare 绑定上述同一生产 SHA，不是本次文档任务新跑。

| 证据 | 已核事实 | 原物 |
|---|---|---|
| 最终 full | total 1586；1553 pass / 33 fail / 0 skip；RunID `2ab956e0-dc0a-440f-bdee-52da8b8b615f` | [Results](../../../.coding-agent/sandbox-results/dae600ca5dfc438ab982f8bc7b337311/Results) |
| 最终 related | 183/183，含 ordinary GlyphRun 修正后相关范围 | [Results](../../../.coding-agent/sandbox-results/988ff1abc4564a1e8fba2982904a4783/Results) |
| 与前轮 full 差分审计 | AddedFailures 与 RemovedFailures 均为空；33 failure 未解 | [audit.json](../../../.coding-agent/sandbox-results/5ba9e5bae3024963ad56a7912400e539/Results/audit.json) |

33 项真实失败保留，不全部归环境；差分失败集合未变不等于失败已解决，也不等于总门 Green。
普通测试 4224 pass / 1 fail / 4225，仍为指定 runtime 10.0.12 DLL 或依赖 `8007007E`，不将其不确定性改写为单一已证根因。
GenProto **34** 为本轮实际日志证据，并非新 TRX；不套用旧 RunID，不声称它是加载上述 AOT SHA 的测试。

## PresentationCore 同 SHA 独立回归

四组独立 compare 已读；各自分母不相加，不用历史 SHA 代填本轮。

| 范围 | 有限结果 | 独立 compare 任务原物 |
|---|---|---|
| primitive | 4096 byte exact | [a0c1f9a02f91413f97c376ed618875e2](../../../.coding-agent/sandbox-results/a0c1f9a02f91413f97c376ed618875e2) |
| Blur | 14，zero difference | [54e57a11474046329a7f06437ed69630](../../../.coding-agent/sandbox-results/54e57a11474046329a7f06437ed69630) |
| image | 10，zero difference | [566408b442714c1bba5ba18ae39f829c](../../../.coding-agent/sandbox-results/566408b442714c1bba5ba18ae39f829c) |
| media | 4 frame / 8192 byte exact | [d104adfd16b1461b8340578b79a20abb](../../../.coding-agent/sandbox-results/d104adfd16b1461b8340578b79a20abb) |

这些是有限软件消费者回归，不关闭完整 HW、持续媒体、全格式、全架构或 E2E-09；不重新领取已闭 software effect。

## source → dispatch → consumer → physical 边界

[Ledger 追加记录（已清理；合同/说明）](../history-index.md)关联有限 native factory source、GeneratedValueResources / GeneratedProtocol dispatch、21 拒绝、76 case 与 ordinary GlyphRun 特例，再指向上述真实报告。
这是有限签名与物理映射，不是全部 requiredSatisfied，不宣称所有 physical records、独立 field/enum/pack/size、producer/consumer 或 stage4Closed 已满足。GenProto 34 不能替代全 required 逐条裁决；最终同 SHA strict 报告仍缺，不用历史代填。

## 唯一下一自主生产模块与正式门

下一模块为 **ProcessCreate typed / partial-batch 失败清理、引用世代与关闭 owns 完整拥有链**：合并真实 typed consumer、首 HRESULT、批次中途失败逆序清理、引用替换/世代隔离、关闭与 callback/线程拥有关系，不以补测或文档 WIP 代替生产交付。
不重领已闭 factory 分母、ordinary GlyphRun 特例及 software effect；完整 ProcessCreate 方法族仍 Open。
四外部 authority 历史 binding、ARM64、完整 10.0.12、player protocol 只由[唯一外部页](../all-completion-external-prerequisites.md)维护。
[DEC-0004](../decisions/DEC-0004-process-lifetime-module.md)：DLL 驻留不卸载，不豁免资源、引用、线程与 callback 清理。
[总计划](../remaining-gap-closure-plan.md)与[最终矩阵](../final-acceptance-remaining-matrix.md)正式条件不减；33 failure、strict、required/physical 及全矩阵仍 Open，正式总门 **Red**，无全任务完成或替换资格。
