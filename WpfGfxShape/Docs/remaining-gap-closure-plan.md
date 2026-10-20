# wpfgfx 完整替换总计划

## 当前权威方案

目标为**实现并验证 WpfGfxShape 完整替换原来的 wpfgfx.dll**。后续子智能体执行[完整替换实施与验证方案](wpfgfx-complete-replacement-validation-plan.md)，任务分配见[当前工作项](handoffs/current-work-item.md)，执行约束见[执行规则](handoffs/current-execution-rules.md)。本文不再沿用旧轮次的 Debug 全矩阵或不新增测试限制。

## 范围

Windows x86 Release/x64 Release；测试实现完全采用 C#/.NET。仅两个正式项目 Tests/ComAcceptance，允许补测试及修复生产。DLL 驻留、对象资源仍需清理；ARM64/卸载非目标。官方同版同架构独立对照，无固定 runtime 合同，证据记录实际版本及身份。

关键维度为二进制兼容、功能结果、时序行为、资源行为、恢复能力（device-loss、resize）。错误行为仅错误输入；环境错误细节、性能和额外 hardening 不独立设门。retail 保留 Closed，私有 PRERELEASE player 非目标。

## 实施包

| 包 | 工作与完成证据 |
|---|---|
| 1 当前基线 | 当前候选双架构 Release 完整现有回归、实际模块/SHA、历史 HW 失败重新裁决 |
| 2 二进制兼容 | 独立纯 C# ABI 声明及官方/候选真实调用；结构、调用约定、COM、refs、callback、重复调用及后续状态 |
| 3 内容与真实调用方 | 组合渲染、软件/硬件实际内容、格式与媒体、真实 WPF producer/consumer E2E、非空与新帧验证 |
| 4 时序 | 更新、提交、完成、重入、注销、关闭、回调必要偏序与在途任务收敛 |
| 5 资源 | COM/device/surface、CPU/GPU 资源、线程及 callback 在重复循环后稳定收敛 |
| 6 恢复 | resize、真实 device-loss、恢复后新帧与持续媒体，cache/per-device/generation 失效及最终清理 |
| 7 未知差异 | 合法输入/操作序列随机差分、固定 seed、失败缩减、负控有效性及缺陷稳定回归 |
| 8 最终交付 | 最后源码对应实际产物的双架构 Release 全部适用矩阵、原始证据审阅与结论 |

发现缺陷必须修复生产根因，不改 golden/阈值/skip，不以软件 fallback 代替 HW。无真实条件的关键路径记阻塞/未覆盖，不能按通过算。错误输入维持有界辅助测试，不扩展为环境错误合同。

## 当前状态

现有局部通过结果、SHA 和实际版本只认[基线](handoffs/current-stable-baseline.md)及[最近报告](completed-work/x86-stroke-current-validation.md)。旧 full 不绑定新 SHA；历史 HW 失败尚需当前候选重新裁决。原生额外 hardening 未知如实保留，但不独立阻塞本任务。

本次文档更新不代表任何新测试已执行。只有全部适用关键能力有实际证据、没有关键差异/未覆盖/阻塞，才能宣布在明确验证范围内完整替换。整体未完成。
