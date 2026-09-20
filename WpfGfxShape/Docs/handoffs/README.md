# wpfgfx 会话交接归档索引

> 当前唯一入口始终是 [`../next-session-handoff.md`](../next-session-handoff.md)。本目录保存当前交接支撑文件、不可覆盖的历史 snapshot 与按主题维护的完成进展归档；恢复工作仍必须先从唯一入口开始。
>
> 当前执行约束见 [`current-execution-rules.md`](current-execution-rules.md)，稳定基线见 [`current-stable-baseline.md`](current-stable-baseline.md)，当前工作切片见 [`current-work-item.md`](current-work-item.md)。
> 长期完成状态摘要见 [`progress-completed-work.md`](progress-completed-work.md)，HW render-target 逐轮历史见 [`progress-hw-rendertarget.md`](progress-hw-rendertarget.md)。

## 命名

`S-YYYYMMDD-NNN-<work-package>.md`；无法可靠取得日期时使用 `S-UNDATED-NNN-<work-package>.md`。

## 归档规则

- 开始覆盖 `next-session-handoff.md` 前，先把旧版本原样归档。
- 归档文件不作为下一轮任务入口。
- 每项记录 session ID、完成/当前工作包、结果、下一工作包和归档文件。
- 规则详见 [`../10-session-continuity-protocol.md`](../10-session-continuity-protocol.md)。

## 索引

| Session ID | 本轮工作 | 结果 | 下一工作包 | 文件 |
|---|---|---|---|---|
| `S-UNDATED-001` | 首版静态调查与迁移规划 | 已完成文档；未创建项目或运行验证 | `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD` | [`S-UNDATED-001-INITIAL-PLANNING.md`](S-UNDATED-001-INITIAL-PLANNING.md) |
