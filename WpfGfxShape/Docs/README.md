# WpfGfxShape 文档分层索引

唯一恢复入口：[next-session-handoff.md](next-session-handoff.md)。本索引不保存运行数字；“当前/最新”只由[状态源](handoffs/current-stable-baseline.md)解释。

当前合同优先级：[DEC-0005正式仅x86/x64](decisions/DEC-0005-x86-x64-acceptance-scope.md)覆盖所有旧ARM64架构门；编译/布局/运行/补件均NA而非Passed。原Ledger未偷偷改Satisfied，父级仍Blocked。[WindowsDesktop版本来源单一审计](windowsdesktop-version-provenance.md)：10.0.12历史路径绑定，未证用户强制，精确版本待澄清，不是必需购买/永久补包。以下历史档的相冲突语句不驱动验收。

## 现行

- [执行规则](handoffs/current-execution-rules.md)、[状态源](handoffs/current-stable-baseline.md)、[工作项](handoffs/current-work-item.md)。
- 最新有限执行：[GenProto x86 CLR wire](completed-work/GenProto-x86-clr-wire-finite.md)。x86 CLR布局已完成，不重领；与先前x86 Shared生命周期分开，ARM64现按DEC-0005不适用，不宣称通过。历史档未执行等表述由当前导航覆盖，原文不改。
- [总计划](remaining-gap-closure-plan.md)：七阶段正式完成条件；[主题合同](plan-module-contracts.md)：A/B/C、消费者差集与 E2E 独立矩阵。
- [外部输入](all-completion-external-prerequisites.md)、[方法族边界](publicabi-family-delivery-plan.md)；文件映射按历史隔离索引查证。

## 决策与稳定规范（不是运行状态）

- [决策索引](decisions/README.md)：DEC-0001–0005；驻留合同优先于历史卸载要求。
- 章程：[实现/ABI](charter-implementation-contract.md)、[工具/文档/例外](charter-evidence-and-continuity.md)；[旧锚点索引](00-migration-charter.md)。
- [源码拓扑](01-native-source-topology.md)、[ABI/调用方](02-abi-and-managed-callers.md)、[迁移门禁](migration-implementation-gates.md)、[耦合域/例外](migration-coupled-domains.md)；[迁移旧锚点](03-migration-order.md)。
- [项目形态](04-nativeaot-project-shape.md)、[互操作](05-silknet-directx-assessment.md)。
- Ledger：[范围/ID](ledger-scope-and-identity.md)、[字段/状态/完成](ledger-records-and-acceptance.md)；[旧锚点](07-scope-and-ledger-schema.md)。
- ABI：[entry/类型](abi-entry-and-type-schema.md)、[寿命/binary/验收](abi-lifetime-binary-and-gates.md)；[旧锚点](08-abi-manifest-spec.md)。
- 连续性：[恢复/收尾](continuity-recovery-and-closeout.md)、[失败/决策](continuity-failures-and-decisions.md)；[旧锚点](10-session-continuity-protocol.md)。

## 方法（不维护当前 SHA 或计数）

- 测试：[分层/批次门禁](testing-levels-and-gates.md)、[比较/失败/完成](testing-comparison-and-acceptance.md)；[旧锚点](06-testing-strategy.md)。
- 证据：[运行/隔离/比较](evidence-run-contract.md)、[独立E2E-00–09](e2e-executable-contracts.md)、[能力/失败/状态](evidence-capabilities-and-failures.md)；[旧锚点](09-test-evidence-and-e2e-contract.md)。
- [生产 COM 方法](com-nativeaot-testing-method.md)、[标准 MTP 沙箱操作](com-acceptance-sandbox-operations.md)。
- probe/旧 ABI Host 仅机制证据；生产 COM 唯一项目直接加载 AOT，不恢复自建 Host。

## 历史、冻结与撤销（按需查证，不是默认恢复路线）

- [历史隔离索引](history-index.md)：旧总体/分轮计划、撤销入口及冻结例外。
- [完成证据目录](completed-work/README.md)、[交接归档索引](handoffs/README.md)、[旧完成档规则](handoffs/completed-work/README.md)。
- [调查分类索引](investigations/README.md)：全部调查主题、历史 WIP、原始证据与辅助工程。
- 历史原始 SHA、失败、阶段边界保持原义，不批量替换成新值。

## 维护

状态只更新状态源；计划维护条件，工作项维护恢复点，方法维护规则。
现行长规范按语义主题承载，编号原页保留所有旧章标题兼容跳转；主导航直达主题，不删正式条件。冻结长档不机械拆改。
本轮修改、长度、历史例外与文本/渲染核验边界见[最终治理报告](docs-governance-report.md)；正式条件逐条承接见[source → current追溯](governance-authority-traceability.md)。

## 目录整理导航

[调查独立状态索引](investigations/status-index.md)、[历史方案](history/plans/README.md)、[完成档状态](completed-work/status-index.md)、[交接状态](handoffs/status-index.md)；[实际迁移报告](docs-organization-report.md)。
