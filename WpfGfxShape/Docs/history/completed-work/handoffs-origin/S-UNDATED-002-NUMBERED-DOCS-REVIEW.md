# 编号文档 00～11 职责与有效性梳理

## 完成内容

逐个核对并清理 `Docs/00-*.md` 至 `Docs/11-*.md`，保留仍有证据或规范价值的正文，同时明确每份文档的当前身份，避免历史状态、旧工作包和旧依赖版本被误认为当前事实。

## 逐项结论

- `00-migration-charter.md`：强制权威规范；只维护最高原则，不维护当前进度。DirectX 绑定规则已对齐当前 Silk.NET 2.23.0 基线。
- `01-native-source-topology.md`：历史静态事实基线；原生拓扑证据保留，“本轮”和工具限制仅描述当时调查。
- `02-abi-and-managed-callers.md`：权威静态事实基线；生产 ABI 实现进度不在本文维护，旧调查期禁令不适用于当前会话。
- `03-migration-order.md`：历史/规范基线；保留批次和门禁，旧状态不用于调度，收尾规则已改为当前拆分交接和独立历史文件。
- `04-nativeaot-project-shape.md`：历史设计基线；旧两项目结构、模板状态、`WP-00B` 当前工作包和待实现 ABI 主证据均已失效。
- `05-silknet-directx-assessment.md`：历史风险调查；对象为 Silk.NET 2.11 本地源码，当前生产绑定固定为 2.23.0。
- `06-testing-strategy.md`：权威测试分层规范；不维护最近测试数字，`E2E-00` 不代表生产 ABI 或 PresentationCore E2E。
- `07-scope-and-ledger-schema.md`：权威范围和 Ledger schema；Ledger 已存在但当前为 `RepairRequiredPartial`。
- `08-abi-manifest-spec.md`：权威 ABI manifest schema；当前探针级 ABI 不等于生产 manifest。
- `09-test-evidence-and-e2e-contract.md`：权威 harness/evidence 契约；当前项目和运行结果改由稳定基线维护。
- `10-session-continuity-protocol.md`：强制权威协议；修复会话结束、文档职责和首轮专用归档等过期规则。
- `11-first-coding-step.md`：完全过期的执行快照；`WP-00A` 已完成，正文清单不得继续执行。

## 交叉一致性

- `Docs/README.md` 已从统一的“权威文档”列表改为“文档职责与有效性”，逐项记录上述身份。
- 当前唯一恢复入口仍为 `Docs/next-session-handoff.md`。
- 当前唯一动作仍为 `Docs/handoffs/current-work-item.md`。
- 当前阶段和剩余缺口仍由 `Docs/remaining-gap-closure-plan.md` 维护。
- 本次未修改生产代码，未运行构建或测试。
