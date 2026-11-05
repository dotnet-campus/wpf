# 两测试项目结构收口

## 最新用户合同与交付边界

本轮用户已裁决仅保留两类测试：托管单元/组件与真实生产原生 ABI/COM/PresentationCore 验收。该裁决覆盖旧规范关于独立 probe、GenProto 历史取证、Ledger 写入归档及额外测试项目的组织要求，不改变产品原有兼容目标。不得为迁移无限修生产，也不恢复用户不要的治理测试。

Tests 根实际仅有：

- `WpfGfxShape.Tests/`：net10.0 MSTest，引用生产托管项目，验证算法、布局、解析、状态及组件。
- `WpfGfxShape.ComAcceptance/`：net10.0-windows MSTest/MTP，加载真实生产 AOT DLL，验证公开导出、COM、原版差分及真实 PresentationCore。PublicAbi、Pcore、PlayerHwnd、WpfReference 是迁入辅助源码目录，不是第三项目；整个 Tests 递归仅两个项目文件。

自定义 MTP 单 exe 的角色入口保留；隔离原版/候选使用同一 exe，不新增 Host，也不以 exe 角色误判新增测试项目。

## 完整旧目录处理映射

既有清理账 `artifacts/test-results/consolidation/cleanup.json` 记录 **30 个项目、17 个旧目录**；下表按目录归并，项目数量不能再次累计。原账迁入 TestMethod 数为14，不等于数据行分母或新增14个通过；数据行、既有验收和真实媒体转入后以实际 TRX 为准。

| 旧目录 | 最终处理 |
|---|---|
| GenProtoC5Audit.Diagnostics | 删除历史 C5 治理/取证，不作为日常测试门 |
| GenProtoNativeLayoutDiagnostics | 删除独立 native layout 诊断工程，不宣称原诊断全部迁入 |
| GenProtoX86Clr.Diagnostics | 删除历史 CLR 取证工程，历史结果不冒充新结果 |
| NativeCaller | 删除纯原生 probe/caller 工程，不要求恢复第三类消费者 |
| Stage4Layout.Diagnostics | 删除独立布局诊断工程；实际协议布局行为由两类测试承载，不宣称逐旧用例保留 |
| Stage4Layout.X86.Diagnostics | 删除独立 x86 布局诊断工程；产品 x86 目标不变，不冒称已全验 |
| Tools | 删除旧测试辅助/治理项目集；不建立兼容壳或新项目 |
| WpfGfxShape.AbiIntegration | 删除非生产 probe 测试，不将 probe Green 算生产覆盖 |
| WpfGfxShape.AbiIntegration.Host | 删除 probe Host，不迁回 |
| WpfGfxShape.ComAcceptance.Host | 删除自建 COM Host，生产验收归现有 ComAcceptance |
| WpfGfxShape.GenProto.Tests | 删除历史审计与生成工程；不恢复 GUID/过程文件依赖 |
| WpfGfxShape.LegacyHash.Tests | 删除历史 hash 治理，不以旧 hash 约束当前官方 runtime |
| WpfGfxShape.PlayerHwndAcceptance | 行为源码合入 ComAcceptance/PlayerHwnd，不保留独立 proj |
| WpfGfxShape.PresentationCoreE2E | 行为源码合入 ComAcceptance/Pcore，八场景以真实结果记账 |
| WpfGfxShape.PublicAbi | 行为源码合入 ComAcceptance/PublicAbi，同轮官方/候选 metadata 与导出检查 |
| WpfGfxShape.WpfReference | 原版行为辅助源码合入 ComAcceptance/WpfReference |
| X86SharedSupport.Diagnostics | 删除历史 shared/架构诊断工程；不算新全架构通过 |

`SystemVideoSampleTests` 的空输入托管行为仍归 Tests，真实系统媒体行为归 ComAcceptance/SystemMediaSampleTests。协议纯 unit 保留 `GeneratedProtocolTests` 和 `Generated*ResourceTests` 等解析/布局/资源行为；它们不是已删 GenProto 生成器测试的重命名。源生成器 GenerationTests 未迁回：原生成工程关联历史审计与已删过程树，本轮不新增工程或为“所有旧用例都迁”重建治理依赖，因此不宣称当前已覆盖生成器重生/全差分。用户允许删除，不以删除用例制造原生 Green。

Ledger 核心 records/groups/work-packages/evidence/schema 继续保留为产品映射；Ledger 历史状态与归档写入不是两项目 build/test 的强依赖，也不能阻止结构收口。

## 真实覆盖分母与结果

本轮未改测试和生产源码；没有重发生产 DLL 或重跑原生 full 制造新包。

- 托管：此前通过专用测试接口实际运行整个 `WpfGfxShape.Tests.csproj`，Debug/net10.0，**4224 total / 4224 passed / 0 failed / 0 skipped**，退出0。此前真实4224不是新增覆盖；此前重新运行确认相同分母；本轮仅最终清理，不重跑测试。
- 原生：完整 XML 解析 `.coding-agent/sandbox-results/2a96fad1007146139499e9060eecae40/Results/integration-final-full.trx`，RunID `3632f149-097a-49bc-b40d-5f9f1c6c32f2`，机器 `EN-DEV-01`，UTC `2026-10-16T12:52:20.0958111Z` 至 `12:54:18.8010522Z`。原始 Counters：**1675 total / 1675 executed / 1634 passed / 41 failed / 0 notExecuted**；error/timeout/aborted/inconclusive/notRunnable 等均0，skip为0。解析1675条 UnitTestResult，不拿旧1616/1583/33或中途数字代替。
- 两类当前分母分别4224与1675；它们不是原生产兼容面的完整覆盖率，也不是对所有已删用例的迁移率。项目构建成功与范围收敛不等于生产全绿。

### 全部41项失败类别

| 类别 | 数据行数 | 同轮失败事实 |
|---|---:|---|
| Stage4 HW cache geometry/configuration/WIC/lazy reentry/write-lock/multi-device | 15 | 2+2+2+5+2+2；像素背景 `0x002E2E2E`，期望 `0x00563412` |
| PublicHwnd D3DImage | 8 | 两个非shared像素断言、六个shared/callback/shader前置返回141953144而非0 |
| Shader replace/malformed fallback/channel-device recreate | 3 | 返回141953144而非0，未据此宣称已验证fallback消费者 |
| NativeGpu topmost/frontbuffer-client | 3 | topmost背景；两个duplication取帧返回-2005270489 |
| SecondSampler Visual/Cache | 3 | 实际背景，对期望合成像素失败 |
| PublicAbiComparisonTests | 1 | 候选角色退出1，缺三个官方导出 |
| PcoreComparisonTests | 8 | primitive/images/nested/blur/shadow/lifetime/media/dispatcher 候选角色退出1，新增保护入口缺失阻塞真实链 |

前五类共32条，归 D3DImageTests；本轮没有历史 OriginalPublicHwnd 那条失败，不机械复用历史33项。观测/control问题和候选差异须分别诊断，不能统称41个产品bug或全部环境失败。

## 同轮两模块加载与官方合同

同轮 PublicAbi 根 `Results/publicabi/10bd1166281243a88aa54be26f5ba140/Results/`：

| 角色 | JSON RunId | 实际Path（沙箱任务根下） | SHA256 |
|---|---|---|---|
| reference | 9db80f07277e419381b6647ccc8d5828 | wpfgfx_cor3.dll | 098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E |
| candidate | a6024078342c49148c6db5623244a568 | Native/wpfgfx_cor3.dll | 3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D |

绝对根为 `C:/CodingAgentSandbox/Tasks/2a96fad1007146139499e9060eecae40/`；二者均X64/.NET10.0.9，路径和hash不同，不是候选自比。reference Missing/Extra均空；candidate Missing为 `MilPlayer_Create`、`MilPlayer_Process`、`WpfGfx_SetDisableBoundsCheckProtection`，Extra空。不能继续称当前strict仅缺两个player入口。

JSON `OfficialProtectionContract` 从实际官方 PresentationCore 反射取得 `MS.Win32.PresentationCore.UnsafeNativeMethods+MilCoreApi` 的 `Void WpfGfx_SetDisableBoundsCheckProtection(Boolean)`，DllImport wpfgfx_cor3.dll/同名EntryPoint/Winapi/ExactSpelling=false，参数System.Boolean、MarshalAs空。该metadata只证明签名，不证明保护策略实现。Pcore候选result.json记录resolver选中Native候选路径，缺入口是新runtime真实暴露，不是迁移编译错。

下一调查应查明该官方版本入口的原生保护语义、调用时机、状态/线程及成功失败合同，取得可靠依据后另行授权实现；不得写noop/stub、删断言或pin旧runtime。player record-packet合同仍独立缺口。本轮不实现上述生产能力。

## 清理与验证

清理源 `eng/process-cleanup/Cleanup.cs` 已改为幂等最终清单，不再重跑旧源码迁移或覆盖现有测试。专用 NativeAOT Release/win-x64 publish 实际退出0，日志包含 Generating native code，无IL3050/IL2026（有样式警告）。.NET MSBuild Roslyn任务以字节加载Cleanup.dll并真实调用Cleanup.Run，退出0、六行摘要；完整解析TRX与JSON摘要写ignored `artifacts/test-results/consolidation/final-evidence.md`，不是几万行JSON回显。

本轮最后源码仅补真实业务删除计数，专用NativeAOT发布退出0，日志含Generating native code，无IL3050/IL2026；随后.NET MSBuild Roslyn字节加载该编译DLL，连续两次调用Cleanup.Run，退出0、0警告/0错误。两次各输出本次业务清理0文件/0目录/0字节、迁移0；不把临时回收计业务、不与既有30项目/17目录账相加。Apply/cleanup-tool及根artifacts中的Cleanup.cs编译目录已实际自清；保留两正式源、原cleanup.json、唯一原始Results与生产发布物。eng回读仅Cleanup.cs和README.md；所有运行数据只在ignored artifacts/.coding-agent，不写Tests/ReferenceData。此后未再改清理源码。最终递归回查Tests只有两个csproj、slnx仅引用这两个测试项目；历史调查RotatedFloatContract.Tests的旧项目引用仅改指已有ComAcceptance，未改变测试源码或历史证据输入。

两个正式csproj最终Release build均退出0；ComAcceptance有55条既有警告、0错误，不宣称无警告。未修改测试/生产源码，原生full复用上列冻结原物，没有重新publish生产或重full制造包。

Pcore primitive候选JSON的actualNativeModules实际列出Native候选路径/SHA3F291D3E…（不仅requested/resolver）；exception为EntryPointNotFoundException，MediaSystem.Startup调用保护入口失败，HRESULT0x80131523。最终解析摘要同时保留Pcore JSON的actualNativeModules/exception，核实原版与候选实际加载，不倾倒完整JSON。

本交付只收口两项目结构、文档合同与清理执行；整体生产兼容与总计划仍未完成。
