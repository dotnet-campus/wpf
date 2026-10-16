# 当前唯一工作项：剩余 GenProtoEvidence 过程硬依赖治理

先读[过程清理交付](../completed-work/process-cleanup.md)与 `../../artifacts/process-cleanup/summary.json`。剩余 45,136 文件、26,551 目录、2,503,071,496 bytes；下一单独大批，不是本轮已删除。先分离固定 GUID 输入、目录动态选择、可再生输出，做最小输入迁移再有限删除；本轮不再大范围重构 GenProto。禁止重跑 46 项生成证据，读取既有 16_08_57 TRX 即可。

下列生产顺序保留为后续独立范围，不覆盖当前过程清理优先级。

## 合同与边界

- 正式仅 Windows x86/x64 全配置；ARM64 为 NotApplicable-UserScopeDecision，非 Passed，见 [DEC-0005](../decisions/DEC-0005-x86-x64-acceptance-scope.md)。
- 已安装官方完整 runtime、同版同架构独立原版/候选对照，见 [DEC-0006](../decisions/DEC-0006-installed-official-runtime-contract.md)。不再询问是否必须 10.0.12。
- DLL 进程驻留，卸载/重载/热替换不适用；真实资源清理不豁免。
- 本轮实际编辑清理入口与 Docs，专用编译并调用清理成功；不改生产或 Ledger，不发布生产、不跑 full。下列生产工程工作未在本轮执行。

## 后续最优立即执行顺序（不是本轮已执行）

1. 有界修复 `Tests/WpfGfxShape.Tests/SystemVideoSampleTests.cs:27–32` 对历史 JSON `NativeDll` 的直接加载；复用官方 runtime 定位/身份校验，分离历史 golden 与运行时路径。保持原断言，不 skip、不篡改 golden。
2. 在已安装同版官方 runtime 的 x86/x64 真实进程先专项后普通回归；分别记录 runtime/模块/架构/配置身份，消除普通 4224/1/4225 中该工程性失败，不要求旧 patch。
3. 复用当前正式 MTP 包与原版/native control，逐类处理 33 个 HW/control 失败的前置、可观察输出与真正候选差异；原版/control 失败不等于候选已过，也不能全归环境。
4. 按[九包总计划](../remaining-gap-closure-plan.md#当前剩余工作唯一有界清单)推进 HW A/B、D3DImage/recovery、持续媒体/寿命，再做最终全配置 ABI/strict/full/E2E。先诊断真实差集，不凭未知虚派生产方法。
5. 并行接收[唯一外部页](../all-completion-external-prerequisites.md)三项来源，按原合同有界复核 C5/D4/player；不是停工等待所有输入。

## 不重复领取

selected v143 的 12 配置 16 必要 TU 预处理已完成；不是完整 native link。bilinearspan 四配置 GetTargetPath 的 MSB3202 是确缺原项目来源，不是 header 失败。
正式 6 physical+3 evidence 不扩；333 保守风险原表保留，不增穷尽清零门。既有 C1–C4/D1–D3、x86 CLR/Shared、consumer/factory、有限软件效果/owning/多 Dispatcher 不重领。
当前C5完整实际闭包与D4未满，两required/parent Blocked，stage4Closed=false；[Ledger显式覆盖](../../Ledger/current-user-scope-overrides.json)只撤销ARM64/旧版本派生阻塞，不改Satisfied。

## 恢复与证据

当前身份只见[基线](current-stable-baseline.md)；原始材料只读。新 smoke 不是 full；GenProto 46/46 是此前源码回归，后续 C5 审计不是新的 testfull。
本次文档维护不计生产进度，整体任务未完成。
