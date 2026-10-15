# 已测生产身份与有限基线

## 当前已测生产（不等于全局 Green）

SHA256：`77394561E16E13DEC82DD194DC5C0090D4A64A5477FEA83D7879DE2369D1D33D`。颜色 ABI 测试修复未修改或重发布生产。

- COM full：1415 pass / 32 fail / 1447，0 skip；`Investigations/Color-ABI-regression/Results/com-full/Results/Administrator_EN-DEV-01_2026-10-15_02_37_20.4701392.trx`，同目录 `aot-identity.json`，task `b9e4be771379477f9ee35be64a92223a`。
- COM related：765/765；task `1f905cb906714eba9754fbb2b3db3d10`；扩展 related 1246 pass / 2 fail / 1248，task `d614b97fb3c94d74a693226e0558b117`。
- Glyph normal/retry/ownerRelease 非空整面 3/3，红蓝两组、glyph command 两组，共 7/7。coverage quantize 半向上、DrawGeometry padding、RGBA wire 有限闭合；通用 OOM/异常仍 Open，原版未初始化 padding raw 比较不放宽。
- 普通 related 32/32、独立 ABI 13/13（固定 IEEE bytes、sizeof/offsets 等）；普通全量 4224 pass / 1 fail / 4225，缺系统 10.0.12。GenProto 起始24 pass / 4 fail /28；最终源码后有限全量28/28、0 fail/skip，RunID `b93b5895-7477-4c0c-8add-88bdb46010bc`；[新source映射与历史缺失](../investigations/genproto-evidence-recovery.md)单列，不声称整体全绿。

全部路径与失败边界见[执行报告](../../Investigations/Color-ABI-regression/Results/EXECUTION-REPORT.md)及[独立归档](../investigations/glyph-outline-wip.md)。32 present 失败不全部归环境，也不全部断言生产回归。

## 历史身份（不是现状）

1DAA… 的 1429/1429 是修改前有限 Green，原始 RunID `2b2deb74-0bc7-45af-94f7-78fa02a9e3d8`，task `563fbdcb56144acdbc574fa3c0e0fb2e`，位于 `Investigations/Perf-async/finite-final-full/Results`。7C62… 的 1405/1440、35 fail 是早期 glyph 失败快照，不能继续称最新结果。旧 unit 27/29 已由 related 32/32 与独立 ABI 13/13 替代。

Missing2（player）/Extra0、LifecyclePassed=true 仅上次 strict 已测；没有在当前 SHA 伪造 strict 新测，PE/AllAbi 总门仍不关闭。

[总计划](../remaining-gap-closure-plan.md)七阶段和[四项外部输入](../all-completion-external-prerequisites.md)保持。DEC-0004 进程驻留已决；unload 不适用，资源/线程/callback/初始化失败清理仍验收。正式总门 Red，无整体完成或替换资格。
