# 历史、冻结与撤销隔离索引

本页不是恢复入口。工作只从[唯一入口](next-session-handoff.md)恢复，状态只认[当前基线](handoffs/current-stable-baseline.md)。历史页中的“当前/最新/下一步”均绑定当时身份，不具有调度效力。

## 历史规划（不恢复旧状态；保留正式合同）

| 文件 | 保留内容与失效部分 |
|---|---|
| [migration-master-plan](history/plans/migration-master-plan.md) | 初始工作包树及§9最终完成定义保留；旧阶段、百分比、unload Blocked 不作当前门 |
| [session-roadmap](history/plans/session-roadmap.md) | 初始分轮计划已退出当前调度；旧固定下一包/探针轮次不可执行 |
| [DrawBitmap方案](history/plans/drawbitmap-effects-float-implementation-plan.md) | 历史设计与S-130–161续接；旧测试分母/续接不是当前任务 |
| [旧映射长档](history/plans/native-to-managed-file-map.md) | 各次源码阅读与产物身份追溯；多个“最新”属于历史，不是当前实现清单 |
| [首包完成结果](history/plans/11-first-coding-step.md) | 只保存启动边界及废弃方案，不领取WP-00A |

## 冻结证据：不改原文

- [completed-work](completed-work/README.md) 与 [handoffs/completed-work](handoffs/completed-work/README.md)全部完成档按各 run 冻结。即使页内写“最新”也不覆盖基线。
- [旧完成汇编](handoffs/progress-completed-work.md)、[HW旧汇编](handoffs/progress-hw-rendertarget.md)：保留原位、不拆、不迁移、不追加。
- [初始交接快照](handoffs/S-UNDATED-001-INITIAL-PLANNING.md)及 S-*：历史恢复点无效。
- 原始 log/TRX/JSON/图片与 Evidence 子树保持原样；缺原物不是允许重造旧 hash 的理由。

## 必要旁注（覆盖旧规则，不覆写原始证据）

- Four-exports、Overall-plan-progress、System-policy、Tile-mapping、GenProto-finite-closure-audit 等旧档的卸载必需/待批准说法，已被 [DEC-0004](decisions/DEC-0004-process-lifetime-module.md)取代；旧 NativeAotUnloadVerified=false 仍保留，不改为 true。
- Perf-sync 的旧全量 Green、Perf-async 的短包接受试验、Four-glyph 的旧 raw 结果均仅属其版本。短于合法通知结构的原生 OOB 不作 oracle。
- 旧完成档中的自建 Host、环境变量定位、probe、阶段5–7 Pending 不作为现行执行合同。
- 最新生产 SHA 未重跑的 strict 结果不得重新绑定；状态源明确区分“上次 strict”与当前生产回归。

## 已撤销/退出当前入口

- [overall-progress-assessment](history/plans/overall-progress-assessment.md)已撤销，不恢复百分比看板。
- session-roadmap 与 migration-master-plan 不再作为当前领取入口；仅在本隔离层查阅或从正式条件引用精确章节。

## 调查隔离

[调查索引](investigations/README.md)按专题分类；WIP/repair-plan 标题不是授权任务。稳定规则归决策/方法，运行事实归状态源。

## 实际归类

[历史方案目录](history/plans/README.md)保存已迁移正文；原路径只留兼容锚点。[调查状态](investigations/status-index.md)区分未闭合同、有限专项、替代方案和审计来源。
