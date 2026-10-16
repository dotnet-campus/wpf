# 测试证据：能力、失败注入与执行状态

> 依据[DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)，当前正式仅x86/x64全部矩阵。下方RealArm64Host及ARM64环境需求只保留历史字段，当前NA-UserScopeDecision，不按CapabilityMissing阻塞、不改Passed、不再索取机器。

[证据规范](09-test-evidence-and-e2e-contract.md)§10–15正式合同，运行状态只见[基线](handoffs/current-stable-baseline.md)。

## 10. 环境标签与能力检测

每测试声明requiredCapabilities：WindowsDesktopInteractive、RealArm64Host、D3D9Hardware/D3D9Ex、WIC/DWrite/WMP/EVR/DXVA、RdpSession/ForceSoftware等。
缺能力=NotRun-CapabilityMissing，不是pass；若done要求则仍blocked。
生产DLL不要求SupportsNativeAotUnload，卸载依DEC-0004不适用，不套能力缺失规则，不把既有false改通过。

## 11. 失败注入

稳定ID至少覆盖allocation/heap、lock/init、loader DLL/symbol、COM QI/create、channel/service channel、resource handle/factory、callback registration、worker thread/event、device/resource、media state transition。
不改原产品语义注入；adapter保持产品函数不变，记录revision/可观察范围。

## 12. Evidence ID 与状态

格式 EVID-<work-package>-<level>-<case>，如EVID-WP-00B-T2-PROBE-X64-DEBUG-PINVOKE、EVID-WP-04A-T3-EXACT-ARITHMETIC-RANDOM、EVID-WP-09X-T5-E2E-05-RTB-PIXELS。
状态 Planned/ExecutedPassed/ExecutedFailed/NotRun/Invalidated；仅ExecutedPassed满足Ledger/ABI requirement。SDK/源码/golden/规则改变时标Invalidated并重跑。
不适用项登记适用性，不伪造ExecutedPassed。

## 13. 每个文件编码前的测试合同

工作包必须预写原文件/头/生成关系、observable输入输出副作用、normal/boundary/error/release、original取得方式、exact/tolerance、configuration/RID/environment、T1/T3/T4/T5各Evidence ID、尚未就绪高层为何不阻当前较低状态、禁止测试便利改变生产控制流。

## 14. 超时、重复与随机测试

每case硬超时，超时保存dump并终止隔离运行；由标准运行器/沙箱或已授权编译验收应用处理，不据此自建COM Host。
并发/寿命记录重复次数和seed；随机/属性可重放，失败保存最小输入/原seed；不能一次happy path关闭有状态包。

## 15. 已锁定承载与未关闭能力

稳定边界：固定库名resolver到publish绝对路径；probe覆盖协议/并发/缺DLL与导出/无效PE/错误架构/模块路径/identity，仅E2E-00。
历史AbiIntegration/AbiIntegration.Host不作为生产COM承载指令；唯一生产ComAcceptance标准MTP直接AOT；独立PublicAbi客户端不是自建COM Host，见[运行合同](evidence-run-contract.md#2-当前与目标承载边界)。
尚待按正式门取得的能力/裁决（状态不在此重复）：PresentationCore候选隔离加载具体机制、大型原DLL/媒体/图像存储方案、x86/x64真实HW/media运行矩阵（ARM64为范围不适用）、Windows x86完整ABI/config/E2E、crash/fail-fast证据。
具体外部内容只在[唯一外部清单](all-completion-external-prerequisites.md)，不复制包清单；x86 Shared有限证据不外推完整矩阵。
卸载不适用不再列未关闭项；最终兼容验收不减。
内部NativeAotAbiProbe.Invoke只计T1，不登记ABI通过。
