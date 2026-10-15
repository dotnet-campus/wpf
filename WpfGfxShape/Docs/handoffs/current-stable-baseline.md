# 唯一当前状态源：已测身份与有限基线

## 当前生产身份与运行边界

SHA256：`77394561E16E13DEC82DD194DC5C0090D4A64A5477FEA83D7879DE2369D1D33D`。颜色 ABI 测试修复未修改或重发布生产；本轮治理也未执行生产/测试。

| 范围 | 已证有限结果 |
|---|---|
| COM full | 1415 pass / 32 fail / 1447，0 skip |
| COM related / 扩展 | 765/765；扩展1246 pass / 2 fail /1248 |
| glyph | normal/retry/ownerRelease真实非空严格整面3/3；加红蓝与command共7/7 |
| 颜色普通related / 独立ABI | 32/32；13/13（固定IEEE bytes、sizeof/offsets） |
| 普通全量 | 4224 pass / 1 fail / 4225；指定系统10.0.12 runtime缺失 |
| GenProto | 起始24 pass /4 fail /28保留；最终源码后28/28，0 fail/skip，不与COM冒充同次执行 |

coverage quantize原生半向上、DrawGeometry padding、RGBA wire有限修复；严格原版未初始化padding raw不放宽，通用OOM/异常及完整复杂glyph消费寿命仍Open。32 present失败不全归环境，也不全断言生产回归。

## 原始证据定位

以下 COM 路径相对 `WpfGfxShape/Investigations/Color-ABI-regression/Results/`：
- full：`com-full/Results/Administrator_EN-DEV-01_2026-10-15_02_37_20.4701392.trx`及同目录`aot-identity.json`，task `b9e4be771379477f9ee35be64a92223a`。
- related：`com-related-765/Results/Administrator_EN-DEV-01_2026-10-15_02_39_47.1987772.trx`，task `1f905cb906714eba9754fbb2b3db3d10`。
- 扩展：`com-related/Results/Administrator_EN-DEV-01_2026-10-15_02_35_56.4530309.trx`，task `d614b97fb3c94d74a693226e0558b117`。
- 普通/ABI与glyph细项定位：[执行报告](../../Investigations/Color-ABI-regression/Results/EXECUTION-REPORT.md)、[证据账](../investigations/active/glyph-outline-wip.md)。

GenProto原始[TRX](../../Tests/WpfGfxShape.GenProto.Tests/TestResults/lindexi_DESKTOP-51A5UGG_2026-10-15_10_51_03_net10.0.trx)，RunID `b93b5895-7477-4c0c-8add-88bdb46010bc`。
新[delivery报告](../../Investigations/GenProtoEvidence/delivery-6a03b29edb064c22a50b7b1985affa62/report.json)，报告SHA `1274062D5CB9C18ED1644F26D269B5193B75E9478AB519FCC818657621D85107`（原TRX stdout）；sourceWorkingTree=NotInspected、stage4Closed=false。
[恢复记录](../investigations/active/genproto-evidence-recovery.md)区分新source映射与缺失旧`delivery-3e43…`：旧原物未恢复，不能伪造历史追溯成功。

## 阶段状态（唯一维护处）

| 阶段 | 当前有限状态与未闭边界 |
|---|---|
| 1 HW backend | 既有限直接设备/生命周期范围Closed；全域可达消费者与失败/恢复差集未闭，无冻结全域分母 |
| 2 HW内容 | 既底层范围Closed；generated消费者、shader失败政策与真实HW寿命未完 |
| 3 软件呈现 | 既直接组件范围Closed；完整PresentationCore/格式/变换未闭；source17/18 FLOAT/HDR Open，历史Pbgra32零差异不覆盖FLOAT差异 |
| 4 generated protocol | Active，stage4Closed=false；physical44/48、required2/4、satisfied=false保留，有限测试不替代完整闭包 |
| 5 resources/UCE | Active；全链失败/取消/线程/CoInitialize/OOM/Compose/callback异常与逆序清理未闭 |
| 6 meta/API与DLL | Active；生产创建/桥、完整ABI语义及架构矩阵未闭；仅历史strict Missing2/Extra0，缺player两入口 |
| 7 PresentationCore E2E | Active；7A/7B/7C与E2E-01–09完整矩阵未满足 |

[四外部输入清单](../all-completion-external-prerequisites.md)所列authority、ARM64工具链+真实环境、完整10.0.12 runtime、player协议+ownership+oracle仍确缺；其余可自主模块不停工。
[DEC-0004](../decisions/DEC-0004-process-lifetime-module.md)规定进程驻留，unload/reload/热替换不适用；Release、线程退出、callback静止和初始化失败清理仍验收。
正式总门Red，总计划未完，无整体完成或替换资格；不报告整体百分比。

## 历史身份与跨版本禁止拼接

- 202A8C9F…同步perf及1DAA…的全量Green属于旧产物；后者RunID `2b2deb74-0bc7-45af-94f7-78fa02a9e3d8`、task `563fbdcb56144acdbc574fa3c0e0fb2e`。
- 7C62…早期glyph全量35 fail、旧unit27/29只作历史，不冒充现状。
- Missing2（player）/Extra0、LifecyclePassed=true仅上次strict独立实证，未在当前SHA重跑；PE/Passed/AllAbiPassed=false不能被有限回归关闭。
- 历史HWND、3D、raw/release、source17/18与x86有限结果保留各自分母/身份，不拼成当前SHA定向结果。
- [总计划](../remaining-gap-closure-plan.md)与[主题合同](../plan-module-contracts.md)保存条件；历史页不竞争本状态源。
