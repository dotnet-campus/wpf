# 当前唯一恢复入口

本轮已完成[过程清理交付](completed-work/process-cleanup.md)，清理入口编译并实际调用成功；GenProto 只读完整解析既有 16_08_57 TRX，46/46，不重跑。历史文档审计记录不代表本轮授权。

## 先读合同

1. [执行规则](handoffs/current-execution-rules.md)：本轮授权有限过程清理与专用编译/调用；无终端脚本，不改生产/Ledger，不发布生产、不跑全量测试。
2. [当前基线](handoffs/current-stable-baseline.md)：区分历史 full、同 SHA strict、最新 startup smoke 与 GenProto 回归，不拼运行身份。
3. [当前工作项](handoffs/current-work-item.md)：先治理剩余 GenProtoEvidence 45,136 文件的过程硬依赖；不重领有限 Closed，不扩大本轮到生产主线。
4. [唯一剩余总计划](remaining-gap-closure-plan.md)：九个有界完整工作包；[矩阵](final-acceptance-remaining-matrix.md)逐门记验收差集。

## 已生效范围

- [DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)：仅 Windows x86/x64 全配置；ARM64 全部验收为 **NotApplicable-UserScopeDecision**，非 Passed，不再阻塞或索取设备。
- [DEC-0004](decisions/DEC-0004-process-lifetime-module.md)：DLL 加载后进程驻留，无卸载/重载/热替换需求；资源、引用、线程与 callback 清理仍验收。
- [DEC-0006](decisions/DEC-0006-installed-official-runtime-contract.md)：使用已安装官方完整 runtime，同版同架构原版/候选独立对照。10.0.12 仅历史路径身份，版本口径不再待澄清，不要求另交该版本。

## 当前结论

**整体未完成，不是只剩外部输入。** x86/x64 全配置 ABI/E2E、持续 HW/D3DImage/shader、device recovery 及正式可达失败/refs/callback 条款仍须实现或验收。
C5 真实缺 bilinearspan 原来源及完整实际 physical-record 闭包，D4 缺历史 binding；两 required/parent 仍 Blocked，stage4Closed=false，总门 Red。
C5 333 保守风险不是新增必须清零门；已证 selected CL、x86 CLR、有限软件/媒体/多 Dispatcher 不重领。

正式入口是 ComAcceptance 标准 MTP；最新 40/40 仅 startup smoke。旧 NativeCaller 截图原物缺失不列生产必需外部输入。
最少真正用户来源与 x86/x64 验收环境条件只维护于[外部页](all-completion-external-prerequisites.md)。
[Ledger范围覆盖](../Ledger/current-user-scope-overrides.json)显式保留原ARM64字段、不改Satisfied；父级因x86/x64原条件未满继续Blocked。
历史主文档的陈旧流水已退出恢复；冻结证据、golden、SHA 和原失败保留于[历史索引](history-index.md)，不冒充本轮执行。
