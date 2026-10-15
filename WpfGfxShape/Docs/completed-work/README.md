# 完成工作证据目录（历史冻结）

本目录每份交付档绑定当时产物与执行范围，不维护当前状态。[唯一恢复入口](../next-session-handoff.md)、[状态源](../handoffs/current-stable-baseline.md)。

## 有界主题索引

- Command-*、Geometry-*、Lock-notify-*：方法族与当时 strict 缺项。
- Four-*、Player-Hwnd-*：旧集成、3D/glyph/release/perf/HWND 与 player 边界。
- Perf-sync-*、Perf-async-*：分别对应其发布身份；旧全量 Green、短包试验不是当前结果或合法 oracle。
- Overall-plan-progress-*、System-policy-*、Tile-mapping-*：阶段/政策/像素有限交付，不是整体完成。
- S-*：对应会话独立证据，包括 FLOAT 未改复验、Visual/Cache 双输入阻塞等。

## 冻结例外旁注（不修改原文）

旧档“最新”仅指该 run；阶段5–7 Pending、x86 支持待证、卸载必需/待批准及旧 Host/probe 规则不覆盖现行合同。
生产驻留按[DEC-0004](../decisions/DEC-0004-process-lifetime-module.md)登记不适用；历史 unload=false 原样保留，不改为通过。
新生产/测试证据创建新档；旧 SHA、失败、原始日志不可全局替换。
与旧 [handoffs/completed-work](../handoffs/completed-work/README.md) 均属历史层，不相互竞争恢复点。
