# 治理前入口快照（历史冻结）

以下保存治理前 next-session-handoff.md 正文内容（相对链接已适配目录，并非逐字节原文）；其中“当前”仅指归档时，不参与恢复调度。恢复只从 [唯一入口](../../next-session-handoff.md) 开始。

# 当前交接：颜色 ABI 回归已测，正式总门 Red

## 权威入口

- [当前工作项](../current-work-item.md)：GenProto 证据恢复与剩余合同。
- [已测基线](../current-stable-baseline.md)：生产身份与真实失败分列。
- [glyph / color ABI 独立证据账（已清理；合同/说明）](../../history-index.md)：历史失败不冒充现状。
- [总计划](../../remaining-gap-closure-plan.md)：七阶段、A/B/C、E2E-01–09 条件不减。

## 当前已测

生产 SHA `77394561E16E13DEC82DD194DC5C0090D4A64A5477FEA83D7879DE2369D1D33D` 不变；颜色测试修复未重发布生产。普通 related 32/32、独立 ABI 13/13；COM related 765/765，扩展 1246 pass / 2 fail / 1248，full 1415 pass / 32 fail / 1447。普通 Tests 4224 pass / 1 fail / 4225（缺系统 10.0.12）；GenProto 起始 24 pass / 4 fail / 28，最终源码后全量 28/28、0 fail/skip；[独立恢复归档（已清理；合同/说明）](../../history-index.md)记录新 source 映射与缺失历史原物。原始 TRX、identity 和失败边界见[执行报告](../../../Investigations/Color-ABI-regression/Results/EXECUTION-REPORT.md)。旧 unit 27/29 与 7C62…35 fail 均为历史，不是现状。

Glyph normal/retry/ownerRelease 真实非空整面三组通过；coverage quantize 原生半向上、DrawGeometry padding、RGBA wire 有限条款已修。严格原版未初始化 padding raw 比较仍保留；通用 OOM/异常仍 Open。Missing2/Extra0 为上次 strict 已测结果，未在新 SHA 重跑，不声称新 strict 通过。

## 不变边界

正式总门 Red，无整体完成或替换资格。[四项外部输入](../../all-completion-external-prerequisites.md)：历史 authority、ARM64 真实环境、完整 10.0.12 runtime、官方 record-packet player 协议/ownership/oracle。DEC-0004 进程驻留已决，unload/reload/热替换不适用；资源 Release、线程退出、callback 静止和初始化失败清理仍验收。

归档说明：正文内容保留，因存放目录改变仅修正相对链接；不是原文件逐字节副本，也不是新增实测。
