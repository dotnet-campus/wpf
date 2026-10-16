# GenProto C5：selected build evaluation 已测，active-input 闭包待证

## 本轮范围与结论

正式范围仍为 GENPROTO-MILCMD 原有 **6 physical + 3 evidence**，不是新增边分母。18 native + 2 generator MSBuild evaluation 已有真实原物，本轮先 build 并测试最新 SelectedEvaluationTests；未新增分析器、诊断项目或生产实现，未重新构建整个 WPF。C5 **待证/Blocked**，两个 required 及父级仍 Blocked，stage4Closed=false。用户全任务继续，不宣称自主剩余为零。

发现并修复两个证据链问题：

- WireCompilerArchiveTests 的 `wirecompiler-*` 搜索误把 delivery 归档当测量源，导致缺 summary。改为只选原有 wirecompiler-32位ID 测量目录；缺源/hash/assembly 断言均保留，不跳过 unknown。
- 原 333 项 reconciliation 只计算并断言、未写报告。现已将原数组持久化；现有归档测试完整读取 C5 JSON、校验 summary/source/assembly hash、断言 333 并保存快照。没有新增分析能力或正式 required。

## 最新真实执行与读回

最终源码 build 成功。专项 `12_22_34` 原 TRX 为 2/2、0 fail/skip（SelectedEvaluationTests + WireCompilerTests），RunID `58846cbb-e699-420c-9824-099094aaf746`。full `12_22_49` 为 46/46、0 fail/skip，RunID `5cc712a5-0337-4877-ae59-e1a3b47623aa`；full 后最终归档 `12_23_19` 为 1/1，RunID `99939695-c22a-44df-b064-1c3f9baed822`。原 TRX 均位于 `Tests/WpfGfxShape.GenProto.Tests/TestResults/lindexi_DESKTOP-51A5UGG_2026-10-16_<上述时间>_net10.0.trx`。

首轮 full `12_17_52` 的 45 pass/1 fail 原 TRX 保留：真实原因是 delivery/measurement 分类错误，不是 SDK 或业务 unknown 被忽略。修正后测量专项、归档专项及 full 均实际重验；最终源码改动后另完整执行上述 2/2、1/1、46/46、1/1。后续仅改文档/Ledger，无未测源码尾改。

最终归档目录：`Investigations/GenProtoEvidence/wirecompiler-delivery-ebf1bb9eb58647d2a68f5b243ce5d768/`。

| 原物 | SHA256 / 身份 |
|---|---|
| 归档 report.json | `6770D6733298D2A40B723A379EBD83B85A6AE80C0E951F296C111549CE4BD954` |
| C5 report.json / 归档 c5-report.json | `129609D5E8953FEB424DEF7CDE3E07A0E15BA5F07AFE7EF05AF5B09DFEEA2E71` |
| 最新 full 原 TRX / genproto-full.trx | `D05E3204B35C2F55BED7BC65153D89E482FCA4BEFA7F8C7449E18FB396BAEB2E` |
| test assembly | `C11E39974E4EAC04FF714D8CB3AFC99D36F28AA432A4540C86514C099F5A2E86`；MVID `ac2ca61c-15fa-4633-baf8-b05015fda449` |
| SelectedEvaluationTests.cs | `A30D6F90229BBF44F43C0D1E6EB0B80B1D80874C0E628DB83AAF86B1B1E75173` |
| WireCompilerArchiveTests.cs | `EB19E3FBBB9E0B45B2754929EDA595BE936B2D2812AE7484A0A65CCE1B4CCB13` |

最终归档测试完整解析 C5 report **115221 leaves**、summary 11 leaves，wire report 12078 leaves、summary 75 leaves，历史 wire report 9838 leaves、summary 99 leaves；读取全部 sourceIdentity、核对测量源及 test assembly，并复制/hash核对 full 原 TRX。归档 report 自身所有叶值再次解析，Ledger hash 断言成立。本轮已读回最终归档关键身份、原 TRX、C5 summary及333 reconciliation原文样本；完整报告读取由上述实际测试完成，不声称人工逐行阅读十余万行。归档的 `targeted` 字段仍是旧的 wire-only `12_20_21`，不冒充最新 combined target；最新 target 是上列原始 `12_22_34`。

## C5 子条款裁决

| 现有子条款 | 当前实际证据 | 裁决 |
|---|---|---|
| 正式入口与已有记录归属 | group 既有 member/authority/consumer 去重6 physical、required 3 evidence；report保留逐ID原记录 | Satisfied，仅入口冻结及记录可追溯，不是所有实际输入已闭 |
| selected projects/configurations/items/imports/defines/forced includes | resources/uce/wpfgfx Debug、Release × Win32/x64/arm64共18，mcg两配置共2；XML、imports、TU元数据及源hash | Satisfied，仅实际 evaluation，不是 native compilation |
| Win32/x64 selected SDK/VC 搜索路径 | 12项 v143、VC `14.44.35207`、SDK `10.0.26100.0`；windows.h/sal.h/stddef.h按捕获搜索路径定位/hash | Satisfied，仅路径与来源，不是头文件实际被包含 |
| ARM64 selected toolchain | 6项 VCToolsInstallDir/Version、WindowsSdkDir/include为空，显式 MissingArm64ToolchainNotHostEquivalent | Blocked，真实缺工具链/环境；非host等价，不假测 |
| active include / macro / transitive applicability | 888 direct candidates（150 distinct）均是 NotPreprocessed；原721历史包络保留，原332 Undetermined + 1 RealMissing 共333项逐项落盘 | 待证/Blocked；不得将候选或未到达当 active/excluded，也不把333升格正式required分母 |
| applicable input → existing physical owner/signature/generation/consumer/assertion | 已保留原边、selectedTranslationUnit、候选目标与 requiredSource | 待证/Blocked；尚缺确切条件栈与逐原边业务裁决 |

原 C1–C4、D1–D3 的当前有限 Satisfied 保持；D4 历史 generation/build authority 仍外部 Blocked。现有 consumer/factory/x86/enum/padding结构有限裁决不重领；padding内容、packet消费和完整transport等价不因此成立。

## 真实最小自主余项与下一有界步骤

自主正式余项仍只有 **C5一个原条款**，不是888或333个新required。缺的具体原物是：仅对冻结项目必要 Win32/x64 TU，使用其已捕获配置/defines/forced includes/按序 includes 得到的 **CL ShowIncludes include 栈或预处理输出（含条件/源行关联）**，再把适用输入绑定至原6 physical owner及已有声明/生成/消费者/assertion。原333数组含project/import/未展开属性边，不能全当C/C++ include处理。

另已读 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.vcxproj:142–144`：无局部条件的显式 ProjectReference 指向 `$(WpfGraphicsPath)core\\sw\\bilinearspan\\bilinearspan.vcxproj`；工作区名称检索未找到该文件。report保留 RealMissing，不伪称可排除；仍需该选定build的原项目文件/生成来源或真实条件适用性证明。此具体缺源不自动归入四项外部authority，也不能由TU预处理替代。

现有 NativeLayout.x64.vcxproj 编译 NativeLayout.cpp、v145、独立生成头路径；它不是所选v143生产TU。为避免范围漂移，本轮停止新增诊断项目，未声称运行 selected-native CL、未生成 ShowIncludes/预处理证据。下一有界执行是复用原项目的内置 CL task，以捕获的 selected输入只处理必要Win32/x64 TU、不调用Exec、不递归构建WPF；保存原始输出后裁决原333中适用边及其记录归属。若必须另造诊断项目/扩scope，继续停在本待证状态，不假补。

四真正外部项仍为历史authority binding、ARM64工具链/真实环境、完整合法10.0.12 reference、官方player协议；可见DWM/HW会话与control为实测前置，不凭空补。两transport packet/独立discard未证，但不是自行新增的原生成required强制门。

## 不变边界

生产 `3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D` 未改，无生产publish/COMfull；历史COM 1616总/1583通过/33失败/0跳过、strict Missing2/player Extra0，均非本轮新跑。DLL进程驻留，unload=N/A。当前evaluation有限子项满足不等于C5满足；原权威门尚不足，两个required及父级Blocked，stage4Closed=false，总门Red。独立Ledger、四短Docs、总计划与矩阵已按此同步。
