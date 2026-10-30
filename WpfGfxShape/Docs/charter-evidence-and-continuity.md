# 章程：工具、文档、状态与例外

本页为[章程](00-migration-charter.md)§8–13的完整主题合同，强制执行，不维护运行状态。

## 8. 工具与证据规则

- 遵守当前环境及用户约束，不以未授权方式绕过限制。
- 优先工作区专用结构读取、代码搜索、编辑、构建和测试能力。
- 区分静态调查与实际构建、发布、测试、二进制检查；未运行不称已验证。
- 生产变更必须执行可用定向、全量主测试、ABI 测试与解决方案构建；不能执行记录原因及完成条件影响。
- 稳定事实、决策、阻塞、结果写对应事实源，不只留对话。

禁止终端命令和脚本。专用 publish/build/test 与沙箱执行已编译验收应用允许，不外推 Shell/脚本许可。历史“本轮仅 Docs Markdown”限制已由用户本次生产删除与测试授权覆盖；具体预算见当前执行规则，不外推发布或沙箱许可。

## 9. 文档先行与跨会话协议

所有重要事实与决定必须落盘。

### 9.1 必备文档

- [章程](00-migration-charter.md)：最高原则及两主题页。
- [拓扑](01-native-source-topology.md)：源码、项目、依赖。
- [ABI/调用方](02-abi-and-managed-callers.md)：导出与托管边界。
- [迁移顺序](03-migration-order.md)：依赖驱动顺序与台账规则。
- [项目形态](04-nativeaot-project-shape.md)：独立 .NET 10 AOT 方案。
- [互操作评估](05-silknet-directx-assessment.md)：采用边界。
- [测试策略](06-testing-strategy.md)：体系与门禁。
- [总计划](remaining-gap-closure-plan.md)、[主题合同](plan-module-contracts.md)：正式条件、依赖、差集；状态只见基线。
- [历史总规划](history/plans/migration-master-plan.md)：原工作包树、最终定义，不维护当前状态。
- [唯一入口](next-session-handoff.md)：恢复导航，唯一动作由工作项维护。
- [README](README.md)：分层索引与阅读顺序。

### 9.2 专题调查文档最低内容

范围、已读关键文件、确认事实及证据路径、推断与置信度、未决问题、风险、总体计划影响、当前切片可引用的后续证据需求、未完成验证及原因/结论影响，均须具备。

### 9.3 过程文档要求

- 子任务尽早建骨架、调查中持续补充，不等全部结束只输出摘要。
- 事实来源路径用仓库根相对路径；仓库外同级源码可绝对路径；Markdown 导航链接按当前文件相对路径。
- 明确区分事实、建议、假设。
- 架构决定记录理由、替代方案、重新评估触发条件。

## 10. 当前交接结构强制格式

1. `next-session-handoff.md`：仅恢复顺序/导航。
2. `handoffs/current-execution-rules.md`：执行约束/禁止扩展。
3. `handoffs/current-stable-baseline.md`：阶段、稳定能力、最近基线。
4. `handoffs/current-work-item.md`：唯一下步、范围、完成条件、停止点。
5. `handoffs/completed-work/<session-topic>.md`：每切片新独立历史文件。
6. 旧 `handoffs/progress-completed-work.md` 冻结，不拆、不迁移、不追加。
7. Ledger/ABI/Evidence/Decision 写各自事实源。

结束前更新受影响职责页；入口不堆历史、百分比、长流水或多个同级候选。下一主目标只有一个。
开始先读章程、唯一入口、直接相关专题，然后才可改代码；入口内恢复顺序仍须完整执行。

## 11. 逐文件迁移台账状态

- `NotInvestigated`：未调查。
- `Investigated`：依赖、职责、测试点已记录。
- `Blocked`：有前置依赖或技术阻塞；Ledger 按正交 blocked 字段表达。
- `Scaffolded`：类型/签名骨架。
- `Translated`：近似直译，验证未完。
- `UnitVerified`：纯逻辑/局部验证。
- `DifferentialVerified`：原/候选差分通过。
- `Integrated`：进入组件链。
- `EndToEndVerified`：真实 E2E 通过。
- `Accepted`：满足该文件全部条件，不跳证据。

## 12. 变更与例外管理

- 强约束只由用户明确修改。
- 技术不可行先记录证据/影响，不自行绕开。
- 例外写总体计划及交接，注明适用范围；局部不扩为普遍。
- 重构/优化在等价完成后另立计划、测试基线，不混翻译提交。

## 13. 首版规划交付轮次边界（历史记录）

首版规划只完成静态探索、专题调查、总体计划、后续分派交接；不完成项目创建、AOT 原型、生产翻译、原 WPF 项目修改、构建/测试/实际导出枚举。
实现从后续轮次开始并遵守章程。该历史边界不阻止已明确分派实施，当前工作只从唯一入口恢复。
