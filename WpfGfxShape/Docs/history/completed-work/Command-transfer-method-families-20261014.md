# Command transfer：完整方法族状态归档（2026-10-14）

本档完整列出本轮涉及的族及真实 Closed/Open，不宣称整个命令/ABI族完成。最新生产 SHA `E6E8FBCD2D95937FAEC85D28D834466F0FF556195295C641AFE578EE943AC6BC`。本次归档操作只改 Docs，未生产/测试/配置/发布。

## 已实现与有限裁决

| 方法族 / 条款 | 实际状态与不得外推边界 |
|---|---|
| IWICColorContext GetProfileBytes/GetType/GetExifColorSpace（3） | 已实现；当前生产PE三名存在。导出闭包 Closed；本档不以PE存在扩大所有WIC参数/COM失败合同验收 |
| MilChannel BeginCommand/AppendCommandData/EndCommand + MilResource GetRefCountOnChannel（4） | 已实现；当前公开有限生命周期通过，窄条款 Closed。完整分段绘制因果、异常中途清理仍 Open |
| MilResource SendCommandBitmapSource/SendCommandMedia（2） | 已实现，生产PE两名存在，移出旧缺40。bitmap显式AddRef，入队失败finally Release；media沿调用方供应引用合同，不盲目AddRef。相关861/861、真实SystemMedia两模式2/2有限通过；不等于bitmap命令Draw像素及全部资源失败/无泄漏已证 |
| SyncFlush partial close首错 | 有限 Closed：既有原版/候选同步与异步对照返回0x88980411，允许随后Append/End完成；[原归档](Command-syncflush-close-20261014.md)保留原始证据。不能推为服务端malformed/partition zombie可恢复 |
| 生产/诊断ABI隔离 | 已实现：生产项目默认WpfGfxShapeDiagnosticAbi=false，生产条件DebuggerSupport=false，diagnostic独立DefineConstants/PublishDir；probe仅诊断，不作生产兼容证据。受支持DebuggerSupport开关排除debugheader额外导出，当前Extra=[]；未删真实required |
| strict完整ABI | Open/Red：生产Missing47→38、Extra0；当前LifecyclePassed=true，PeContractPassed/Passed/AllAbiPassed=false |
| 整个命令族 | Active：分段命令→Commit→SyncFlush→离屏Draw→固定像素；resource批次首错/中途失败/关闭释放无泄漏、服务端失败寿命仍 Open，另一代理继续像素工作，不标完成 |
| 剩余生产ABI方法族 | 全部 Open，38项逐名及依赖见[新清单](../investigations/command-publicabi-missing38.md)与[一页计划](../publicabi-family-delivery-plan.md) |

## 实际回读证据（路径相对 WpfGfxShape）

- PublicAbi：`Investigations/Command-transfer-publicabi-validated/Results/fedd8765034b444f86b2dfa0f7c1950b/report.json`，RunId同目录名；第1–7行生产SHA/X64，第117–185行Exports，第186–225行Missing38，第226行Extra=[]，第3039–3042行三个false及LifecyclePassed=true。逐名清单来自实际报告，旧missing40不覆写。
- related：`Investigations/Command-transfer-related-validated/TestResults/command-transfer-related.trx`，Counters第5293行861总计/执行/通过，失败0；`Results/aot-identity.json`同SHA、Sandbox=true，过滤MediaExports/Composition/BitmapSourceWrapper/AsyncUceChannel。
- 真实Media：`Investigations/Command-transfer-media-validated/TestResults/media-transfer.trx`，RunID `14b50755-10e4-4d76-991b-eacf0ae398d5`，WhenRealSystemMediaIsTransferredThenBothChannelModesConsumeTheBatch (1)/(0)均Passed，第30行2/2、零失败/未执行；`Results/aot-identity.json`同SHA、Sandbox=true。仅证明所列真实传输批次消费，不推广完整持续异步/HW媒体。
- full：原始 `Investigations/Command-transfer-full-validated/TestResults/command-transfer-full.trx`，RunID `ec9c1400-36ed-4e54-a42a-e202c0729aba`；第10878行1408总计/执行、1376通过、32失败、notExecuted=0，ResultSummary=Failed、exit2。身份在独立 `Investigations/Command-transfer-full-validated/Results/aot-identity.json`，同SHA、Sandbox=true，不把TRX虚写成Results路径。
- 生产源码只读核对：`Code/WpfGfxShape/WpfGfxShape.csproj`第16–19行隔离开关；`Abi/UceChannelExports.cs`第334起两传输入口、bitmap失败引用清理及media调用方引用语义。源码存在不替代完整执行验收。

## 持续收口规则

1376/32/1408不是Green；32呈现观测失败保留，未确诊根因、不随机重跑追Green。阶段4/5/6/7 Active，正式ABI/E2E门及A/B/C Open，无替换资格。三类[外部前置](../all-completion-external-prerequisites.md)保持；x86 Shared/卸载仍须固定版本自主核实，不猜支持或不支持。后续每个完整生产族交付后立即回读新证据并替换四短导航当前整节，历史归档不覆写，直到总计划完成。
