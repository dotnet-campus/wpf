# WpfGfxShape 迁移文档索引

## 推荐阅读顺序

1. [`00-migration-charter.md`](00-migration-charter.md) — 不可违反原则与跨会话协议
2. [`next-session-handoff.md`](next-session-handoff.md) — 当前唯一下一工作包
3. [`11-first-coding-step.md`](11-first-coding-step.md) — 开始编码后的第一项具体工作
4. [`migration-master-plan.md`](migration-master-plan.md) — 总体工作包树与完成条件
5. [`03-migration-order.md`](03-migration-order.md) — 逐文件顺序、Ledger、批次和工作包规范
6. [`06-testing-strategy.md`](06-testing-strategy.md) — 测试层级、渐进 E2E 和证据门禁
7. [`07-scope-and-ledger-schema.md`](07-scope-and-ledger-schema.md) — 生产分母与机器 Ledger schema
8. [`08-abi-manifest-spec.md`](08-abi-manifest-spec.md) — ABI manifest schema
9. [`09-test-evidence-and-e2e-contract.md`](09-test-evidence-and-e2e-contract.md) — harness 与 evidence 契约
10. [`10-session-continuity-protocol.md`](10-session-continuity-protocol.md) — 会话、交接、归档和决策协议
11. 与当前工作包直接相关的专题文档

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

- 当前实际结构已经创建：`WpfGfxShape.slnx`、`Code/WpfGfxShape/WpfGfxShape.csproj`、`Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj` 以及局部 `Directory.Build.props/targets`。
- 托管测试框架为 MSTest，当前项目引用 `MSTest` `4.0.1`。
- Native AOT 生产项目、唯一非生产 ABI probe 和内部 MSTest 已存在；内部 `NativeAotAbiProbe.Invoke(...)` 测试只属于快速单元测试，不能作为最终 ABI 证据。
- `Tests/NativeCaller` 是已作废设计，必须删除且不得恢复；项目不要求 C++ `.lib` 链接或纯原生消费者验证。
- 后续目录以 `Code`、`Tests`、`Docs` 为准，不再使用旧规划中的 `src/tests`。
- 当前唯一工作包：`WP-00B-MANAGED-PINVOKE-ABI-INTEGRATION`。
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
