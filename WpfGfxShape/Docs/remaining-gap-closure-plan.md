# wpfgfx 剩余缺口关闭总计划

## 职责与阅读顺序

本页维护正式完成条件、依赖及关闭纪律，不复制 SHA、测试分母或逐轮状态。
唯一当前阶段/运行状态见[状态源](handoffs/current-stable-baseline.md)；恢复见[唯一入口](next-session-handoff.md)。
总计划驱动跨阶段完整模块交付，由监督者委派实际代码代理；阶段分类不强迫把紧密耦合模块拆成连续小切片。

## 七阶段正式完成条款（保留，不以导航或有限Green代替）

| 阶段 | 正式完成要求 |
|---|---|
| 1 HW backend | 设备/manager/资源拥有寿命、失败与恢复、线程边界及对应原生差集全部有生产实现与独立证据；既有限Closed不豁免新可达差集 |
| 2 HW内容管线 | 全部规定内容/格式/采样/shader/cache消费者和成功失败寿命闭环；底层组件Green不能代替完整内容链 |
| 3 软件呈现链 | 全部规定软件呈现、像素/变换/裁剪/格式与生命周期合同及消费者闭环；直接组件不能代替PresentationCore |
| 4 generated protocol | A/B/C、独立field/enum/pack/size、producer/consumer、required及physical records完整闭包；有限验证不能关闭未满足的physical/required及satisfied条件 |
| 5 resources/UCE | connection/channel/resource/batch/dispatch/scheduler/Compose/Present/target全链及失败、取消、线程、callback、逆序释放完成；对应5A布局、5B寿命、5C集成均有证据 |
| 6 meta/API与DLL | 6A生产创建/外部桥、6B完整公开导出/ABI语义及strict集合、6C资源/初始化/退出/架构与支持边界完成；目标矩阵缺项不得以有限生命周期通过替代 |
| 7 E2E与替换 | 7A E2E-01–05、7B E2E-06–08、7C E2E-09及全架构/配置/能力矩阵、完整Ledger与正式总验收满足后才可替换 |

## 必须关闭的A/B/C与真实消费者差集

原本节逐项拆至[主题合同](plan-module-contracts.md)：A FrozenGraph/Realize、B cache/shader失败、C GenProto required，及 UCE、双缓冲/D3DImage、媒体、FLOAT/HDR、glyph、perf/通知各完整条件。
这不是摘要替代原条件；每个具名子节均为本计划组成部分。

## 自主依赖与外部前置

具体交付物只在[外部清单](all-completion-external-prerequisites.md)维护；自主/外部依赖与阶段6→7接线见[主题合同](plan-module-contracts.md#自主工作与外部依赖)。
模块驻留以[DEC-0004](decisions/DEC-0004-process-lifetime-module.md)为准；卸载不适用不豁免资源寿命。

## 完成定义逐条追溯

| 原条件范围 | 权威位置 |
|---|---|
| 七阶段正式门 | 本页“七阶段正式完成条款” |
| A/B/C 与真实消费者差集 | [主题合同](plan-module-contracts.md)同名主题 |
| E2E-01–09独立矩阵 | [逐项导航](plan-module-contracts.md#独立-e2e-0109逐项不合并降级)、[独立可执行合同](e2e-executable-contracts.md) |
| 测试完成定义 | [测试策略§12](testing-comparison-and-acceptance.md#12-完成定义) |
| 生产Ledger完成定义 | [Ledger§11](ledger-records-and-acceptance.md#11-完成定义) |
| 全迁移定义（历史§9保留合同的现行承接） | [全部完成标准](plan-module-contracts.md#全迁移完成标准)，历史来源仅按需追溯 |
| 最高原则 | [章程](00-migration-charter.md) |

[source → current逐条文本核对](governance-authority-traceability.md)记录历史§9、阶段1–7与独立E2E-01–09的具体条款承接及来源边界，不维护运行状态。

## 持续收口与最终门

每个完整生产族交付后关联最终同 SHA 本族/相关/full/strict 与 identity，回读原始 JSON/TRX，更新状态源并新增独立 Closed/Open 档。
禁止 stub、猜 vtable/包头、私有 ABI、降低 required/skip/golden、SW 冒充 HW、clear/readback 冒充命令因果。
有限用例、PE 集合与 physical/required 有独立分母，不相加、不推算生产总体百分比。
历史主文档仅作证据；没有新原始证据的并行代理结果不外推 Closed。
