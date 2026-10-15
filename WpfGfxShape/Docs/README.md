# WpfGfxShape 文档分层索引

唯一恢复入口：[next-session-handoff.md](next-session-handoff.md)。本索引不保存运行数字；“当前/最新”只由[状态源](handoffs/current-stable-baseline.md)解释。

## 现行

- [执行规则](handoffs/current-execution-rules.md)、[状态源](handoffs/current-stable-baseline.md)、[工作项](handoffs/current-work-item.md)。
- [总计划](remaining-gap-closure-plan.md)：七阶段正式完成条件；[主题合同](plan-module-contracts.md)：A/B/C、消费者差集与 E2E 独立矩阵。
- [外部输入](all-completion-external-prerequisites.md)、[方法族边界](publicabi-family-delivery-plan.md)、[文件映射导航](native-to-managed-file-map.md)。

## 决策与稳定规范（不是运行状态）

- [决策索引](decisions/README.md)：DEC-0001–0004；驻留合同优先于历史卸载要求。
- [章程](00-migration-charter.md)、[源码拓扑](01-native-source-topology.md)、[ABI/调用方](02-abi-and-managed-callers.md)、[迁移顺序](03-migration-order.md)。
- [项目形态](04-nativeaot-project-shape.md)、[互操作](05-silknet-directx-assessment.md)、[Ledger schema](07-scope-and-ledger-schema.md)、[ABI schema](08-abi-manifest-spec.md)、[连续性协议](10-session-continuity-protocol.md)。

## 方法（不维护当前 SHA 或计数）

- [测试分层](06-testing-strategy.md)、[证据/E2E契约](09-test-evidence-and-e2e-contract.md)。
- [生产 COM 方法](com-nativeaot-testing-method.md)、[标准 MTP 沙箱操作](com-acceptance-sandbox-operations.md)。
- probe/旧 ABI Host 仅机制证据；生产 COM 唯一项目直接加载 AOT，不恢复自建 Host。

## 历史、冻结与撤销（按需查证，不是默认恢复路线）

- [历史隔离索引](history-index.md)：旧总体/分轮计划、撤销入口及冻结例外。
- [完成证据目录](completed-work/README.md)、[交接归档索引](handoffs/README.md)、[旧完成档规则](handoffs/completed-work/README.md)。
- [调查分类索引](investigations/README.md)：全部调查主题、历史 WIP、原始证据与辅助工程。
- 历史原始 SHA、失败、阶段边界保持原义，不批量替换成新值。

## 维护

状态只更新状态源；计划维护条件，工作项维护恢复点，方法维护规则。
长篇稳定规范按章节引用，不复制完成条件；冻结长档不机械拆改。
本轮修改及长度、例外与待独立复核项见[治理报告](docs-governance-report.md)。
