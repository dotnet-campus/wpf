# wpfgfx 会话交接归档索引

> 当前唯一入口始终是 [`../next-session-handoff.md`](../next-session-handoff.md)。本目录保存当前交接支撑文件、不可覆盖的历史 snapshot 与按主题维护的完成进展归档；恢复工作仍必须先从唯一入口开始。
>
> 当前执行约束见 [`current-execution-rules.md`](current-execution-rules.md)，稳定基线见 [`current-stable-baseline.md`](current-stable-baseline.md)，当前工作切片见 [`current-work-item.md`](current-work-item.md)。
> 新完成切片按文件保存在 [`completed-work/`](completed-work/)；旧长期完成归档 [`progress-completed-work.md`](progress-completed-work.md) 和 HW render-target 旧历史 [`progress-hw-rendertarget.md`](progress-hw-rendertarget.md) 均已冻结保留。

## 命名

`S-YYYYMMDD-NNN-<work-package>.md`；无法可靠取得日期时使用 `S-UNDATED-NNN-<work-package>.md`。

## 归档规则

- `next-session-handoff.md` 只在导航变化时覆盖；覆盖前把旧版本原样归档。
- 当前规则、稳定基线和工作切片的常规更新不要求为每轮复制入口快照。
- 每个新完成切片在 `completed-work/` 中创建一个独立文件；旧 `progress-completed-work.md` 不拆分且不再追加。
- 归档文件不作为下一轮任务入口。
- 历史 snapshot 每项记录 session ID、工作包、结果、下一工作包和归档文件。
- 规则详见 [`../10-session-continuity-protocol.md`](../10-session-continuity-protocol.md)。

## 索引

| Session ID | 本轮工作 | 结果 | 下一工作包 | 文件 |
|---|---|---|---|---|
| `S-UNDATED-001` | 首版静态调查与迁移规划 | 历史快照；作为实施交接已完全过期 | 当时为 `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`，现已完成 | [`S-UNDATED-001-INITIAL-PLANNING.md`](S-UNDATED-001-INITIAL-PLANNING.md) |
