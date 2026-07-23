# WpfGfxShape 迁移文档索引

## 推荐阅读顺序

1. [`00-migration-charter.md`](00-migration-charter.md) — 不可违反原则与跨会话协议
2. [`next-session-handoff.md`](next-session-handoff.md) — 当前唯一下一工作包
3. [`migration-master-plan.md`](migration-master-plan.md) — 总体工作包树与完成条件
4. [`03-migration-order.md`](03-migration-order.md) — 逐文件顺序、Ledger、批次和工作包规范
5. [`06-testing-strategy.md`](06-testing-strategy.md) — 测试层级、渐进 E2E 和证据门禁
6. [`07-scope-and-ledger-schema.md`](07-scope-and-ledger-schema.md) — 生产分母与机器 Ledger schema
7. [`08-abi-manifest-spec.md`](08-abi-manifest-spec.md) — ABI manifest schema
8. [`09-test-evidence-and-e2e-contract.md`](09-test-evidence-and-e2e-contract.md) — harness 与 evidence 契约
9. [`10-session-continuity-protocol.md`](10-session-continuity-protocol.md) — 会话、交接、归档和决策协议
10. 与当前工作包直接相关的专题文档

## 权威文档

| 文档 | 职责 |
|---|---|
| [`00-migration-charter.md`](00-migration-charter.md) | 原则、最高约束、项目边界、交接硬规则 |
| [`01-native-source-topology.md`](01-native-source-topology.md) | 原生项目、构建、生成、依赖和规模事实 |
| [`02-abi-and-managed-callers.md`](02-abi-and-managed-callers.md) | ABI、托管调用方、布局、所有权和 callback 基线 |
| [`03-migration-order.md`](03-migration-order.md) | 15 批实施顺序、Ledger 和工作包门禁 |
| [`04-nativeaot-project-shape.md`](04-nativeaot-project-shape.md) | 独立 .NET 10 Native AOT 项目方案 |
| [`05-silknet-directx-assessment.md`](05-silknet-directx-assessment.md) | 本地 Silk.NET 覆盖、风险和采用条件 |
| [`06-testing-strategy.md`](06-testing-strategy.md) | 单元、native caller、差分、集成和 E2E 策略 |
| [`07-scope-and-ledger-schema.md`](07-scope-and-ledger-schema.md) | “整个 wpfgfx”的生产分母与机器 Ledger schema |
| [`08-abi-manifest-spec.md`](08-abi-manifest-spec.md) | 导出、类型、callback、COM 和二进制 ABI manifest schema |
| [`09-test-evidence-and-e2e-contract.md`](09-test-evidence-and-e2e-contract.md) | 测试承载、运行目录、Evidence ID 和 E2E 可执行契约 |
| [`10-session-continuity-protocol.md`](10-session-continuity-protocol.md) | 跨会话领取、归档、决策、失败和中断协议 |
| [`migration-master-plan.md`](migration-master-plan.md) | 总体目标、工作包树、P0 门和最终完成定义 |
| [`session-roadmap.md`](session-roadmap.md) | 后续对话的推荐领取顺序 |
| [`next-session-handoff.md`](next-session-handoff.md) | 每轮覆盖更新的唯一交接入口 |
| [`decisions/README.md`](decisions/README.md) | 已接受决策与待裁决 P0 索引 |
| [`handoffs/README.md`](handoffs/README.md) | 不可覆盖的历史交接快照索引 |

## 调查底稿

调查文档保存证据、推断、过程和未决项；稳定结论应吸收到上表权威文档，不在多个文件各自维护不同规则。

| 文档 | 主题 |
|---|---|
| [`investigations/02a-native-export-surface.md`](investigations/02a-native-export-surface.md) | 原生导出面 |
| [`investigations/02b-managed-callers.md`](investigations/02b-managed-callers.md) | 托管调用方与加载边界 |
| [`investigations/03a-generated-model-and-protocol.md`](investigations/03a-generated-model-and-protocol.md) | 生成模型、命令/资源协议 |
| [`investigations/03b-file-ledger-and-batches.md`](investigations/03b-file-ledger-and-batches.md) | 文件台账与依赖批次底稿 |
| [`investigations/04a-build-isolation.md`](investigations/04a-build-isolation.md) | 仓库构建继承与隔离 |
| [`investigations/04b-nativeaot-exports.md`](investigations/04b-nativeaot-exports.md) | Native AOT shared library 与 C 导出 |
| [`investigations/05-planning-audit-and-source-revalidation.md`](investigations/05-planning-audit-and-source-revalidation.md) | 既有规划的源码复核、执行边界审计与本轮偏差记录 |

## 当前状态摘要

- 最新规划审计已再次复核 wpfgfx 生产闭包、ABI/托管调用和本地 Silk.NET；没有创建项目或翻译生产代码。
- 下一唯一工作包：`WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`。
- 当前 P0：Windows x86 Native AOT、unload/reload、COM-like vtable/object。
- 当前 ABI 静态基线：106 `.def`、107 唯一 EntryPoint、99 交集、8/7 差异。
- 当前原生项目项下限：24 个生产项目、461 个直接 `ClCompile`，但完整迁移分母仍待 machine-readable Ledger。

## 维护规则

- 原则只在 `00` 修改；用户明确批准后才能改变。
- 总工作包只在 `migration-master-plan.md` 和 `03-migration-order.md` 维护。
- 测试规则只在 `06-testing-strategy.md` 维护。
- 调查文档保留历史证据，不作为下一轮任务入口。
- 每轮结束必须更新 `next-session-handoff.md`，且只留下一个下一主目标。
- 未 build/publish/test 的结论必须继续标记为未验证。
