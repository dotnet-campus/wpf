# wpfgfx 跨会话连续性、交接与决策协议

> 状态：强制执行  
> 当前入口：[`next-session-handoff.md`](next-session-handoff.md)  
> 最高约束：[`00-migration-charter.md`](00-migration-charter.md)

## 1. 目的

迁移预计跨越大量会话。任何关键事实、状态、决策、偏差或下一动作不得只存在于对话上下文。本协议规定：

- 每轮如何领取唯一工作包；
- 如何归档 handoff，避免覆盖后丢失历史；
- 如何记录决策与例外；
- 如何把 Ledger、ABI、测试证据和计划保持一致；
- 如何在被中断、失败或上下文耗尽时安全停下。

## 2. 单一当前入口与历史归档

- `Docs/next-session-handoff.md` 永远是**当前唯一下一轮入口**。
- 每轮开始编辑该文件前，将上一版原样归档到：

```text
Docs/handoffs/<session-id>-<completed-or-current-work-package>.md
```

- `session-id` 格式：`S-YYYYMMDD-NNN`；若无法可靠取得日期，使用 `S-UNDATED-NNN`，后续不得重编号。
- 归档文件一经写入只允许修正明确笔误并记录原因，不覆盖历史结论。
- `Docs/handoffs/README.md` 保存按顺序索引、工作包、结果和下一包。
- 当前 handoff 只描述一个当前/下一工作包，不累积长篇历史日志。

## 3. 会话开始协议

下一位执行者必须按顺序：

1. 读取 `00-migration-charter.md`；
2. 读取 `next-session-handoff.md`；
3. 读取 handoff 指定的专题/schema；
4. 检查当前 Git/工作区变更，禁止覆盖用户改动；
5. 确认工作包 ID、Ledger/ABI/Evidence ID、Ready 条件、范围外事项和唯一停止点；
6. 若 handoff 与 machine-readable Ledger/ABI/证据冲突，停止编码，记录冲突并先修事实源；
7. 更新受影响 Ledger 行/工作包为当前 owner 后，才编辑生产代码。

不得在每轮重新规划整个项目，也不得从 roadmap 自行选择另一个同优先级工作包。

## 4. 每轮唯一主目标

- 每轮只有一个主工作包。
- 可执行子任务必须都直接服务于该工作包 Done 条件。
- 新发现前置若可在当前包的小范围内解决，登记后继续；若改变包边界，当前目标转 `Blocked`，交接下一唯一前置包。
- 不得为了“顺手”开始下一文件、下一批或重构。
- 机制 spike、生产迁移和优化分别属于不同工作包。

## 5. Handoff 必填格式

`next-session-handoff.md` 每轮覆盖更新，必须显式包含：

1. 文档状态、current/completed work package、下一唯一工作包；
2. 当前总体状态与最近完成事项；
3. 本轮涉及的 Ledger Record/Group ID 及状态变化；Ledger 未建立时写 `None (WP-00I pending)`；
4. ABI/Evidence/Decision ID 及状态；
5. 实际新增/修改/删除文件；
6. 已运行证据：操作、配置/RID、结果和工件路径；
7. 未运行证据：项目、原因和对 Done 的影响；
8. 新发现事实、偏差、风险、阻塞和用户裁决；
9. 下一轮唯一主目标；
10. 范围内事项；
11. 明确范围外事项；
12. 必读文档/源码/工件；
13. 下一轮第一具体动作；
14. 顺序任务清单与每项完成条件；
15. 唯一停止点；
16. 最强约束提醒：逐文件近似直译、禁止提前重构；
17. 下一轮结束时应回写的 Ledger/ABI/Evidence/文档；
18. 本 handoff 的归档目标路径。

所有字段必须写明确值；不适用时写 `None` 与原因，不能省略。

## 6. 已运行与未运行证据

统一术语：

- `StaticConfirmed`：源码/项目文件直接支持；
- `ExecutedPassed`：实际 build/publish/test/inspection 通过；
- `ExecutedFailed`：已运行失败，附错误和工件；
- `NotRun`：未执行，附原因；
- `Blocked`：Done 所需证据无法执行；
- `Invalidated`：输入变化后旧证据不再有效。

禁止使用“应该通过”“看起来可行”“已完成”代替上述状态。静态调查不等于运行验证，build success 不等于 ABI/E2E pass。

## 7. 决策与例外

稳定决策写入：

```text
Docs/decisions/DEC-<NNNN>-<short-title>.md
```

每份包含：

- Decision ID、状态（Proposed/Accepted/Superseded/Rejected）；
- 背景和受影响工作包/Record/ABI/Evidence ID；
- 已确认事实与未验证假设；
- 选择；
- 被拒绝替代方案；
- 对 ABI、正确性、追溯和测试的影响；
- 重新评估触发条件；
- 用户批准（若涉及章程例外）；
- supersedes/superseded-by。

必须记录 Decision 的情况：

- 改变生产范围或目标架构；
- 章程例外；
- Silk.NET 每 API 家族采用 A/B/C；
- COM 实现机制；
- NativeAOT unload/x86 处理；
- generator/frozen source 长期策略；
- 允许的 C# 语言承载偏差；
- 测试框架、candidate 加载方式或大型基线存储策略（若影响长期项目图）。

强约束只能由用户明确批准。智能体不得用 Decision 文档自行授权重构或删减兼容范围。

## 8. 会话结束协议

结束前必须按顺序：

1. 停止新增范围；
2. 完成当前工作包规定验证，或明确记录阻塞；
3. 更新受影响 Ledger/ABI/Evidence；
4. 更新专题/主计划中的新稳定事实；
5. 检查本轮仅修改预期范围；
6. 归档旧 handoff；
7. 覆盖 `next-session-handoff.md`，只留下一个下一目标；
8. 检查所有链接、ID 和状态一致；
9. 给出本轮简短总结。

工作包 Done 未满足时，下一 handoff 仍指向同一工作包或其唯一前置，不得为了显示进度切换后续包。

## 9. 中断与上下文耗尽协议

若无法在当前会话完成：

- 不开始新的实现文件；
- 将正在编辑的文件保持可编译/可辨识状态，或回退未完成局部变更；
- Ledger 状态最多提升到真实证据支持的级别；
- handoff 写明最后完成动作、当前文件/函数、未完成检查、首个恢复动作；
- 明确哪些测试未运行；
- 唯一停止点写为“恢复并完成当前工作包”，不跳到下一包。

不得依赖下一轮从 Git diff 猜测意图。

## 10. 失败与阻塞协议

发生构建/测试/技术门失败时：

- 保存错误、命令、配置/RID、日志和工件；
- 对应 Evidence 标 `ExecutedFailed`；
- 当前 Record/Work package 设 `blocked=true`；
- 区分实现 bug、环境能力缺失、平台不支持和计划假设错误；
- 只修当前包根因，不用 suppression、假成功或绕过隐藏失败；
- 若影响范围/顺序/架构，写 Decision proposal 并在 handoff 请求用户裁决。

## 11. 文档职责与防漂移

- `00-migration-charter.md`：唯一最高原则。
- `migration-master-plan.md`：总体工作包树与最终 Done。
- `03-migration-order.md`：批次、逐文件工作包门禁。
- `06-testing-strategy.md`：测试层级与规范性测试规则。
- `07-scope-and-ledger-schema.md`：生产分母与 Ledger schema。
- `08-abi-manifest-spec.md`：ABI schema。
- `09-test-evidence-and-e2e-contract.md`：harness/evidence 可执行契约。
- 本文：会话、handoff、decision 协议。
- `next-session-handoff.md`：当前唯一动作。

其它文档应引用上述规范，而不是复制可漂移的完整规则。若必须摘录，注明“摘要；以权威文档为准”。

## 12. 一致性检查

每轮结束至少检查：

- handoff 的工作包与 master plan/roadmap 一致；
- Ledger owner/status 与 handoff 一致；
- ABI/Evidence ID 存在且状态一致；
- Done 条件没有把 NotRun 写成 pass；
- 下一目标只有一个；
- 无断链文档引用；
- 所有文件路径采用仓库相对路径（外部 Silk.NET 可用绝对根）；
- 未修改原 WPF产品代码，除非未来用户明确改变边界；
- 未混入重构、优化、重命名或无关清理。

## 13. 当前规划轮次的归档要求

本轮结束时应：

- 将上一轮 handoff 归档为首个历史 snapshot；
- 更新 handoff 索引；
- `next-session-handoff.md` 继续只指向 `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`；
- 明确本轮没有 Ledger ID，因为 `WP-00I` 尚未实施；
- 记录本轮没有 build/publish/test；
- 记录本次规划审计在读到章程工具禁令前误用的两次只读命令：一次列出 Docs 文件，一次统计文档行数；后续未再使用终端，并不得把这些操作当作源码、构建或运行证据。
