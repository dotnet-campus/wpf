# 完整替换最终验收矩阵

权威任务见[实施方案](wpfgfx-complete-replacement-validation-plan.md)、[当前工作项](handoffs/current-work-item.md)及[执行规则](handoffs/current-execution-rules.md)。目标是实现并验证完整替换原来的 wpfgfx.dll，不是局部回归完成。

## 新任务待关闭矩阵

所有测试驱动/夹具/探针/比较器纯 C#/.NET，正式配置仅以下两项。待验不等于此前没有任何局部证据，只表示尚不足以关闭该完整维度。

| 关键维度 | x86 Release | x64 Release | 关闭要求 |
|---|---|---|---|
| 二进制兼容 | 待完整验收 | 待完整验收 | 独立 C# ABI、真实调用、结构/调用约定/COM/refs/callback及后续状态 |
| 功能结果 | 待完整验收 | 待完整验收 | 全部适用内容与组合、真实 producer/consumer、实际 HW/媒体、零阈值与新帧 |
| 时序行为 | 待完整验收 | 待完整验收 | 提交/完成/更新/重入/关闭必要偏序，无关键回调丢失或重复 |
| 资源行为 | 待完整验收 | 待完整验收 | 对象、引用、线程、callback、CPU/GPU资源循环后收敛 |
| 恢复能力 | 待完整验收 | 待完整验收 | resize及真实device-loss确认，恢复后新帧、持续使用和最终清理 |

子智能体须按能力/调用路径展开本矩阵，链接具体用例及实际产物身份、TRX、原始像素/数值、事件轨迹、资源样本。记录通过/失败/未覆盖/阻塞/有依据的不适用，不以比例替代关闭。

## 不单独设门

Debug、ARM64、DLL卸载、原生/直接ESP探针不在本任务范围。错误行为仅错误输入，不要求环境错误码/异常细节等价。性能和额外 hardening 不独立设门；有效输入受影响、卡死、泄漏和恢复失败仍按关键缺陷处理。retail 已 Closed；私有 PRERELEASE player 非目标。

## 已有局部实证（不是新任务最终验收）

- x86 Pcore8（各default/false/true）、geometry14、dash/pen119、最后SoftwareShader/FloatMaskSpan49/49通过。
- x64 同源码Release188/188及geometry尾项2/2通过。
- primitive 双架构三模式4096字节exact，BCL实读白底/红像素及实际模块身份已回读。
- 完整SHA、版本、原始路径只见[基线](handoffs/current-stable-baseline.md)和[报告](completed-work/x86-stroke-current-validation.md)。旧full不绑定新SHA。

## 未闭与最终裁决

历史HW cache15/PublicHwnd8/shader3/NativeGpu3/SecondSampler3共32失败保留旧身份，必须在当前候选重新裁决；不称已消失，也不全部归环境。关键生命周期、实际恢复、持续媒体和真实调用方覆盖仍需完成。

最终源码修改后必须重新验证最终双架构Release实际产物；每个架构绑定自己的SHA。没有未解释关键差异或关键未覆盖/阻塞，才可给出实际版本/环境范围内的完整替换结论。此次只更新文档，未新增实跑结果，整体未完成。
