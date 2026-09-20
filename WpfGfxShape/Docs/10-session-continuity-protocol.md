# wpfgfx 跨会话连续性、交接与决策协议

> 状态：强制执行。
> 当前恢复入口：[`next-session-handoff.md`](next-session-handoff.md)。
> 最高约束：[`00-migration-charter.md`](00-migration-charter.md)。

## 1. 目的

任何关键事实、状态、决策、偏差、验证结果或下一动作不得只存在于对话上下文。本协议规定：

- 如何恢复当前工作；
- 如何保持每轮只有一个工作切片；
- 如何维护稳定基线和下一动作；
- 如何归档完成事实；
- 如何处理中断、失败、阻塞和决策；
- 如何避免历史文档重新成为当前调度源。

## 2. 文档职责

| 文档 | 唯一职责 |
|---|---|
| `next-session-handoff.md` | 当前唯一恢复入口，只保存阅读顺序和导航 |
| `handoffs/current-execution-rules.md` | 每轮执行规则和禁止扩展边界 |
| `handoffs/current-stable-baseline.md` | 当前阶段、稳定能力和最近验证基线 |
| `handoffs/current-work-item.md` | 当前唯一工作切片、范围、完成条件和停止点 |
| `remaining-gap-closure-plan.md` | 当前阶段状态、依赖顺序、剩余大项和完成条件 |
| `native-to-managed-file-map.md` | 原生职责到托管文件的当前事实映射 |
| `handoffs/completed-work/<session-topic>.md` | 每个已完成切片的独立历史文件 |
| `handoffs/progress-completed-work.md` | 冻结的旧完成历史，不再追加 |
| `Ledger/*.jsonl`、`Abi/*`、Evidence、Decision | 各自机器或决策事实源 |

不得把完成历史、阶段百分比、长测试流水或多个候选任务写回入口文件。

## 3. 会话恢复顺序

执行者必须按顺序：

1. 读取 `next-session-handoff.md`；
2. 读取 `current-execution-rules.md`；
3. 读取 `current-stable-baseline.md`；
4. 读取 `current-work-item.md`；
5. 按工作切片需要读取阶段计划、编号规范、原生源码和测试；
6. 检查工作区现有改动，不覆盖用户修改；
7. 确认目标声明、实现、调用者、所有权、测试和完成条件后才编辑代码。

不得从 `migration-master-plan.md`、`session-roadmap.md`、旧 handoff、旧完成历史或 investigation 自行领取任务。

## 4. 唯一工作切片

- 每轮只有一个主工作切片；
- 子任务必须直接服务于该切片完成条件；
- 不得顺手开始下一文件、下一阶段或重构；
- 新发现前置可在小范围内解决时登记后继续；
- 若前置改变切片边界，当前切片标记为 `Blocked`，`current-work-item.md` 只交接唯一前置；
- 机制验证、生产迁移、文档治理和优化不得混为同一生产切片。

## 5. 证据状态

统一使用：

- `StaticConfirmed`：源码、项目或静态工件直接支持；
- `ExecutedPassed`：实际 build、publish、test 或 inspection 通过；
- `ExecutedFailed`：已执行失败，保留日志和上下文；
- `NotRun`：未执行，并说明原因；
- `Blocked`：完成条件所需证据当前不可取得；
- `Invalidated`：输入变化使旧证据失效。

禁止用“应该通过”“看起来可行”或单纯 build success 代替对应 ABI、差分、集成或 E2E 证据。

## 6. 会话结束协议

结束前按顺序：

1. 停止扩大范围；
2. 完成当前切片要求的定向测试、全量主测试、ABI 测试和构建，或明确记录无法执行的原因；
3. 更新受影响的 Ledger、ABI、Evidence、映射和专题事实；
4. 更新 `current-stable-baseline.md` 的最近验证基线；
5. 更新 `current-work-item.md`，只留下一个下一动作或当前恢复点；
6. 若切片完成，在 `handoffs/completed-work/` 创建一个新的独立历史文件；
7. 检查链接、路径、ID、状态和范围一致；
8. 只有导航发生变化时才更新 `next-session-handoff.md`；若覆盖入口，再按需保存历史 snapshot；
9. 给出简短、可核验的结果总结。

旧 `progress-completed-work.md` 和 `progress-hw-rendertarget.md` 不拆分、不迁移、不再追加。

## 7. 完成历史文件

每个完成切片创建一个文件：

`handoffs/completed-work/S-YYYYMMDD-NNN-<topic>.md`

无法确定日期时使用 `S-UNDATED-NNN-<topic>.md`。

每个文件只记录一个切片：

- 原生声明、实现和调用者；
- 托管差集和修改；
- ABI、HRESULT、所有权和释放顺序；
- 定向测试、全量主测试、ABI 测试和构建结果；
- 明确未扩展的范围。

历史文件创建后保持不可变，仅允许修正明确笔误并记录原因。

## 8. 中断协议

无法完成时：

- 不开始新的实现文件；
- 保持工作区可编译或明确标识未完成变更；
- 不把状态提升到证据不支持的级别；
- 在 `current-work-item.md` 写明最后完成动作、当前文件/函数、未完成检查和首个恢复动作；
- 在 `current-stable-baseline.md` 明确哪些测试未运行；
- 下一动作保持“恢复并完成当前切片”或其唯一前置。

不得依赖下一轮从 diff 猜测意图。

## 9. 失败与阻塞

发生构建、测试或技术门失败时：

- 保存错误、配置、日志和相关工件；
- 标记 `ExecutedFailed` 或 `Blocked`；
- 区分实现缺陷、环境能力缺失、平台不支持和计划假设错误；
- 只修当前切片根因，不用 suppression、假成功或绕过隐藏失败；
- 若影响范围、架构或强约束，建立 Decision 并请求必要裁决。

## 10. Decision

稳定决策写入：

`Docs/decisions/DEC-<NNNN>-<short-title>.md`

必须包含：

- Decision ID 和状态；
- 背景与受影响范围；
- 已确认事实和未验证假设；
- 选择与被拒绝方案；
- ABI、正确性、追溯和测试影响；
- 重新评估触发条件；
- 用户批准要求；
- supersedes/superseded-by。

以下情况必须建立或更新 Decision：

- 改变生产范围或目标架构；
- 章程例外；
- COM、callback、unload 或 x86 机制变化；
- generator/frozen source 长期策略变化；
- 允许的 C# 承载偏差；
- 测试框架、candidate 加载或大型基线存储策略变化。

Decision 不能自行授权违反章程或删除兼容范围。

## 11. 进度报告

只报告：

- 当前切片是否完成；
- 当前 Active 阶段；
- 正式关闭与 Pending 阶段数量；
- 当前硬阻塞；
- 是否具备替换原 DLL 的条件。

在生产分母不可确定计算时，不报告整体完成百分比。单个切片完成 100% 不得表述为项目总进度 100%。

## 12. 一致性检查

每轮至少确认：

- `current-work-item.md` 与阶段计划一致；
- Ledger、ABI、Evidence 和映射状态一致；
- `NotRun` 未被写成通过；
- 下一动作只有一个；
- 完成历史写入独立文件；
- 无断链引用；
- 未修改原 WPF 产品代码；
- 未混入无关重构、优化或清理。
