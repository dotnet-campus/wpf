# 当前会话执行约束

## 本轮授权

本轮授权有限过程清理、清理入口最小修改与专用编译/实际调用、Docs 收口；禁止终端脚本，不改生产或 Ledger，不 publishProduction、不跑 COM full、不重跑 GenProto 46 项。用户授权直接删除，无 dry-run/确认/路径安全框架/catch。历史迁移缺源跳过，不搬回 oracle。当前唯一整理事实见[过程清理交付](../completed-work/process-cleanup.md)及唯一 artifacts/process-cleanup/summary.json。
文件核验不冒充执行测试；历史完成档与原始 JSON/TRX/日志/golden 保持冻结。文档维护不计生产完成量。

## 当前有效合同

- [DEC-0005](../decisions/DEC-0005-x86-x64-acceptance-scope.md)：Windows x86/x64，Debug/Release 全部适用门；ARM64 编译/布局/运行/工具链/实机为 NotApplicable-UserScopeDecision，非 Passed，不请求重新批准。
- [DEC-0004](../decisions/DEC-0004-process-lifetime-module.md)：DLL 驻留至进程结束，无 unload/reload/热替换需求；Release、channel/player 关闭、线程退出、callback 静止及初始化失败清理不豁免。
- [DEC-0006](../decisions/DEC-0006-installed-official-runtime-contract.md)：已安装官方完整 runtime，同版同架构独立对照；不再把 10.0.12 当用户目标、待澄清口径或必需补包。旧版本 golden 只读。

## 后续生产执行纪律（非本轮执行授权）

- 总任务按[有界九包](../remaining-gap-closure-plan.md)推进，不能写“只剩外部/无自主余项”。C5 范围内未证需改生产，不等于全项目无实现/验收工作。
- 原生源 `src/Microsoft.DotNet.Wpf/src/WpfGfx/` 保持只读；不改自动生成文件、不造来源、项目或协议，不删引用绕 MSB3202。
- 不把全 WPF/保守扫描包络扩成正式门；6 physical+3 evidence、原 group readiness/doneRule 保持。333 风险不假清零、不一律 external、不强制穷尽。
- 已闭有限软件效果、owning、x86 CLR/Shared、selected CL、consumer/factory 不重领；有限成功不外推完整方法族与全配置。
- 以完整方法族交付真实消费者、成功/失败/关闭证据；仅按具名原生合同/可达路径补失败，不凭空加独立 cancel 或无限核内 OOM 门。
- 生产 COM 仅 `Tests/WpfGfxShape.ComAcceptance` 标准 MTP 直接加载 AOT。禁止 ComAcceptance.Host、自建 COM 子进程、生产托管 internal 代替公开边界；不恢复旧 NativeCaller，不创建/修改 .slnx。

## 证据纪律

full / related / compare / smoke 各独立分母和身份，不相加或推算总体百分比。最新 startup smoke 不代 full；旧 full 不绑新 run。
同 SHA strict 缺两个 player 导出与普通 MediaPlayer 有限 Closed 分开；缺官方 record-packet 合同不得 stub。
33 失败按原版/control 前置和候选差异分账，不全归环境、不全称生产 bug，不盲重跑追 Green。
实际模块路径/版本/SHA、runtime、架构/配置、TRX RunID/Counters、原图及差分均保留；未读原物的结论标为转述。
C5/两个 required/parent 仍 Blocked，stage4Closed=false；ARM64 覆盖是适用性 NA，不把冻结 false/Blocked 改成 Passed/Satisfied。
范围修订机器解释只见[覆盖记录](../../Ledger/current-user-scope-overrides.json)，不覆写冻结required原值。
当前状态只见[基线](current-stable-baseline.md)，补件只见[外部页](../all-completion-external-prerequisites.md)，恢复只见[工作项](current-work-item.md)。
