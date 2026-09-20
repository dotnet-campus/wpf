# WpfGfxShape 迁移文档索引

## 推荐阅读顺序

1. [`00-migration-charter.md`](00-migration-charter.md) — 不可违反原则与跨会话协议
2. [`next-session-handoff.md`](next-session-handoff.md) — 当前唯一下一工作切片
3. [`remaining-gap-closure-plan.md`](remaining-gap-closure-plan.md) — 与当前实现同步的剩余缺口关闭顺序
4. [`native-to-managed-file-map.md`](native-to-managed-file-map.md) — 当前原生职责到托管文件的事实映射
5. [`03-migration-order.md`](03-migration-order.md) — 原始批次、Ledger 和工作包规范
6. [`06-testing-strategy.md`](06-testing-strategy.md) — 测试层级、渐进 E2E 和证据门禁
7. [`09-test-evidence-and-e2e-contract.md`](09-test-evidence-and-e2e-contract.md) — harness 与 evidence 契约
8. 与当前工作切片直接相关的专题文档

## 权威文档

| 文档 | 职责 |
|---|---|
| [`00-migration-charter.md`](00-migration-charter.md) | 原则、最高约束、项目边界、交接硬规则 |
| [`01-native-source-topology.md`](01-native-source-topology.md) | 原生项目、构建、生成、依赖和规模事实 |
| [`02-abi-and-managed-callers.md`](02-abi-and-managed-callers.md) | ABI、托管调用方、布局、所有权和 callback 基线 |
| [`03-migration-order.md`](03-migration-order.md) | 15 批实施顺序、Ledger 和工作包门禁 |
| [`04-nativeaot-project-shape.md`](04-nativeaot-project-shape.md) | 独立 .NET 10 Native AOT 项目方案 |
| [`05-silknet-directx-assessment.md`](05-silknet-directx-assessment.md) | 本地 Silk.NET 覆盖、风险和采用条件 |
| [`06-testing-strategy.md`](06-testing-strategy.md) | 单元、发布后托管 P/Invoke ABI、差分、集成和 E2E 策略 |
| [`07-scope-and-ledger-schema.md`](07-scope-and-ledger-schema.md) | “整个 wpfgfx”的生产分母与机器 Ledger schema |
| [`08-abi-manifest-spec.md`](08-abi-manifest-spec.md) | 导出、类型、callback、COM 和二进制 ABI manifest schema |
| [`09-test-evidence-and-e2e-contract.md`](09-test-evidence-and-e2e-contract.md) | 测试承载、运行目录、Evidence ID 和 E2E 可执行契约 |
| [`10-session-continuity-protocol.md`](10-session-continuity-protocol.md) | 跨会话领取、归档、决策、失败和中断协议 |
| [`11-first-coding-step.md`](11-first-coding-step.md) | `WP-00A` 开始编码后的第一项具体工作与停止条件 |
| [`migration-master-plan.md`](migration-master-plan.md) | 初始总体规划基线；其中阶段状态不代表当前执行状态 |
| [`session-roadmap.md`](session-roadmap.md) | 初始分轮规划基线；实际领取顺序不以其旧状态为准 |
| [`remaining-gap-closure-plan.md`](remaining-gap-closure-plan.md) | 当前中长期缺口分类、依赖顺序、进入条件和完成条件 |
| [`next-session-handoff.md`](next-session-handoff.md) | 当前唯一入口；只保存恢复顺序和文档导航 |
| [`handoffs/current-execution-rules.md`](handoffs/current-execution-rules.md) | 当前每轮执行规则与禁止扩展边界 |
| [`handoffs/current-stable-baseline.md`](handoffs/current-stable-baseline.md) | 当前阶段、稳定能力摘要和最近验证基线 |
| [`handoffs/current-work-item.md`](handoffs/current-work-item.md) | 当前唯一工作切片及完成条件 |
| [`native-to-managed-file-map.md`](native-to-managed-file-map.md) | 当前原生职责到主要 C# 文件的事实对照表 |
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

- 当前位于 [`remaining-gap-closure-plan.md`](remaining-gap-closure-plan.md) 的阶段 1：继续关闭 HW backend 生命周期和直接设备边界。
- 每轮只执行 [`next-session-handoff.md`](next-session-handoff.md) 指定的一个可独立验证切片。
- 当前尚不能替换原 `wpfgfx_cor3.dll`：生产 ABI、generated protocol、UCE/资源协议、完整内容管线和 PresentationCore E2E 尚未闭环。
- 当前代码和测试结构以 `WpfGfxShape.slnx` 中的生产、主测试、ABI Host、ABI 集成测试四个项目为准。
- 原生职责到托管文件的当前状态以 [`native-to-managed-file-map.md`](native-to-managed-file-map.md) 为准。
- 初始规划文档中的旧工作包状态、`Done` 标记和早期下一目标不得覆盖当前交接与缺口关闭计划。

## 维护规则

- 原则只在 `00-migration-charter.md` 修改；用户明确批准后才能改变。
- 当前中长期缺口顺序只在 `remaining-gap-closure-plan.md` 维护。
- 当前唯一动作只在 `next-session-handoff.md` 维护。
- 当前文件映射事实只在 `native-to-managed-file-map.md` 维护。
- 原始批次和工作包规范保留在 `migration-master-plan.md` 与 `03-migration-order.md`，但其中旧状态不代表当前状态。
- 测试规则在 `06-testing-strategy.md` 和 `09-test-evidence-and-e2e-contract.md` 维护。
- 调查文档保留历史证据，不作为下一轮任务入口。
- 未 build/publish/test 的结论必须继续标记为未验证。
