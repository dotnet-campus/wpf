# 连续性：恢复、工作模块与收尾

## r6历史记录（不调度当前任务）
[报告](../history/completed-work/sampler-native-crash-x64-r6-closeout.md)：build1/publish1/offline1实际执行，缺image-backed .xdata读取使unwind不可信，caller未知，任务未完成，非外部API能力障碍。offline角色用完需另授offline1修正验证，业务角色不挪用；fixture不改、23红与所有历史保留。这些预算与下一动作仅属于r6历史，当前恢复只见[唯一入口](../current/current-recovery.md)；诊断默认关闭，不代正式业务验收。

[连续性协议](10-session-continuity-protocol.md)§1–7正式合同；强制执行，上位[章程](00-migration-charter.md)。

## 1. 目的

关键事实、状态、决策、偏差、结果、下一动作不只留对话；规定恢复、唯一工作模块、基线/动作维护、归档、中断/失败/阻塞/决策与历史隔离。

## 2. 文档职责

| 文档 | 唯一职责 |
|---|---|
| Docs/current/current-recovery.md | 唯一当前状态与恢复导航 |
| Docs/current/current-comparison-contract.md | 唯一权威对照与执行规则 |
| Docs/current/next-code-task.md | 既有分派的边界，不自行领取新任务 |
| Docs/history/handoffs/*-legacy.md | 完整历史原文，原字节不改，链接按原基准解释 |
| Docs/current/repository-organization-check.md | 当前目录检查范围、修复及未审项，不复制产品状态 |
| Docs/specifications/plan-module-contracts.md | 正式条件/依赖，不复制状态 |
| Docs/history/plans/native-to-managed-file-map.md | 历史映射，旧身份/下步不生效 |
| Docs/history/completed-work/<session-topic>.md | 每切片独立历史，保留来源子目录 |
| 历史 progress 累计报告 | 冻结不追加 |
| Ledger/*.jsonl、Abi/*、Evidence、Decision | 各自机器/决策事实源 |

入口不写完成历史、百分比、长流水、多候选。

## 3. 会话恢复顺序

1. [唯一入口](../current/current-recovery.md)。
2. [严格对照与执行规则](../current/current-comparison-contract.md)。
3. 只按当前入口引用读取具体完成报告，历史结果不移绑。
4. [任务边界](../current/next-code-task.md)与用户本次明确分派；文档专项另读[维护检查](../current/repository-organization-check.md)。
5. 按模块读阶段计划/编号规范/原生源码/测试。
6. 检查已有改动，不覆盖用户修改。
7. 明确声明/实现/调用者/ownership/测试/完成条件后才编辑代码。

不从migration-master-plan、session-roadmap、旧handoff/完成档/investigation自行领取任务。

## 4. 唯一工作切片

每轮一个主目标，以可独立验收完整生产模块为默认；耦合方法族、消费者接线、成功失败、生命周期及验收是内部步骤，不拆连续小轮。
子任务直接服务完成条件，不顺手下一文件/阶段/重构；小前置登记后解决；前置改变边界则标Blocked，工作项只交接唯一前置，并保留原模块条件及重合并规则。
机制验证、生产迁移、文档治理、优化不混为同一生产切片。本轮仅文档治理，不是生产模块完成。

## 5. 证据状态

StaticConfirmed=静态直接支持；ExecutedPassed=实际构建/发布/测试/检查通过；ExecutedFailed=实际失败保留上下文；NotRun=未运行附原因；Blocked=所需证据不可得；Invalidated=输入变化失效。
不以“应通过/看似可行/build success”替ABI/差分/集成/E2E；卸载不适用单独记录适用性，不冒充通过。

## 6. 会话结束协议

1. 停止扩大范围。
2. 执行本切片要求的定向/full/ABI/构建，或记录不能执行原因；文档专轮遵守不执行限制。
3. 更新受影响Ledger/ABI/Evidence/映射/专题；不向历史映射长档续写当前状态。
4. 当前状态与最近有限证据只更新Docs/current/current-recovery，不追加到累计旧基线。
5. 已分派任务边界只留一个恢复点；不得把历史建议当新授权。
6. 完成后新建独立完成档。
7. 核对链接/路径/ID/状态/范围。
8. 仅导航改变才改入口；覆盖时按需保存snapshot。
9. 简短可核验总结。

旧progress-completed-work、progress-hw-rendertarget不拆、不迁移、不追加。

## 7. 完成历史文件

handoffs/completed-work/S-YYYYMMDD-NNN-<topic>.md；日期不能确定用S-UNDATED-NNN-<topic>.md。
每文件只一个切片：原声明/实现/调用者、托管差集修改、ABI/HRESULT/ownership/释放顺序、定向/full/ABI/构建实际结果、明确未扩范围。
创建后不可变，只修明确笔误并记录原因；文档治理档不伪造生产运行事实。
