# 本次文档同步与只读审计记录

本页只记录本次文档工作，不是第二份剩余清单或新运行证据。恢复见[唯一入口](next-session-handoff.md)，状态见[基线](handoffs/current-stable-baseline.md)，唯一剩余九包见[总计划](remaining-gap-closure-plan.md)。

## 授权与实际动作

仅文件读取、文本搜索、Docs编辑与写后回读；没有终端/脚本、代码/测试/配置/Ledger修改、安装下载、构建、发布或测试。既有DEC-0006和九包已存在，本次直接维护，不另造版本决议或竞争看板。

## 原物核验与边界

以下路径相对WpfGfxShape；未重新hash二进制，未用文件夹名推断运行时间。所核材料未发现可替代当前基线的更晚完整运行，不宣称穷尽全部工件。

| 只读来源 | 本次直接核对 | 不外推 |
|---|---|---|
| `artifacts/sandbox-readback/dispatcher-fixed-full/Results/dispatcher-fixed-full.trx` | RunID及12187–12193行：1616执行、1583pass、33fail、0未执行、失败退出2 | 未重新逐项解析33失败；矩阵分类为既有审计，不冒充本次全量复核 |
| `artifacts/sandbox-readback/dispatcher-final-strict/Results/05045b688eca4e9c9ee8b735ed0efe65/report.json` | 头部SHA为3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D；222–226行Missing两个player/Extra空；14163–14172行PE/总体false、有限LifecyclePassed=true | 不是全ABI通过；保留FixedGolden1012Satisfied及NativeAotUnloadVerified原false |
| `artifacts/sandbox-readback/nativecaller-routing-smoke/TestResults/nativecaller-routing-smoke.trx` | RunID及487–492行：40执行40pass，无失败/未执行 | 仅正式ComAcceptance入口smoke，不是新full或旧NativeCaller生产化 |
| `artifacts/sandbox-readback/dispatcher-fixed-compare/Results/dispatcher-comparison.json` | 抽读头部passed=true、threshold=0及部分逐帧零差 | 未重新遍历全部比较或退出事件；两轮36帧/各joined6沿用完成档有限结论 |
| `Tests/WpfGfxShape.Tests/SystemVideoSampleTests.cs:27–32` | 固定历史JSON定位并直接NativeLibrary.Load其NativeDll | 源码证明定位依赖历史路径；未改代码、未加载DLL，不称已修复或已确定具体缺失依赖 |
| `Tests/WpfGfxShape.WpfReference/ReferenceBehaviorTests.cs:397–445` | 采集实际runtime/架构/native身份，从DrawingVisual程序集目录定位原版 | 参考方法不是现有普通测试已修复；不证明当前两架构完整runtime均已就绪 |
| `Docs/completed-work/GenProto-C5-final-adjudication.md`及当前基线 | 独立C5审计身份、17377记录、333风险与319未知、原required/readiness/doneRule、C5/D4缺口 | GenProto46/46为此前回归；C5审计不是新testfull，333不是新增必须清零门 |
| `Ledger/current-user-scope-overrides.json`及DEC-0004/0005/0006 | executionEvidence=false、ARM64适用性NA非Passed、原required/parent仍Blocked、stage4Closed=false | 仅核对既有显式解释，不修改Ledger，不把范围裁决当执行通过 |

软件primitive/image10/nested16/Blur14/Shadow28/media4、Glyph/owning等有限Closed依基线及各原完成档复用，不把不同SHA或不同run拼成本次执行。bilinearspan四配置MSB3202及历史generation binding缺沿用已读审计，不称原native项目现在可编。

## 本次实际编辑清单

以下均在Docs内；入口四短保持20–80行范围，已编辑规范不超过150行，七阶段/A/B/C/E2E及最终完成定义保留。

- `README.md`、`next-session-handoff.md`：接入本次审计边界，统一Docs-only授权。
- `handoffs/current-execution-rules.md`、`current-stable-baseline.md`、`current-work-item.md`：区分本次回读与上轮实测，Ledger只读，后续优先R1/R2/R3。
- `remaining-gap-closure-plan.md`、`final-acceptance-remaining-matrix.md`：维护唯一九包及逐门差集；未证验收不预判生产缺陷，33分类不伪称本次重新全核。
- `all-completion-external-prerequisites.md`、`windowsdesktop-version-provenance.md`、`decisions/DEC-0006-installed-official-runtime-contract.md`：三项最小来源可合包，版本决议已生效，路径问题尚未工程修复。
- `publicabi-family-delivery-plan.md`：撤除方法页直接调度旧HW下一包的竞争顺序，统一R1→R2及R3有效control→R4/R5。
- `evidence-run-contract.md`、`testing-comparison-and-acceptance.md`：同版同架构独立身份、历史golden只读、control前置与只读审计报告边界。
- `history-index.md`：覆盖冻结档旧ARM64/patch/四类外部说法，纠正“当前SHA无strict”的旧导航解释；冻结正文不改。
- `documentation-readonly-audit.md`：本记录。

## 裁决与剩余

不再要求ARM64、用户补10.0.12或确认最终target、DLL卸载支持、旧NativeCaller故障EXE。正式仅Windows x86/x64 Debug/Release；使用已安装官方完整runtime，同版同架构隔离原/候选并分别记身份，历史golden/路径/SHA/失败保持。

唯一九包仍是R1路径工程、R2全配置ABI、R3有效HW基线/失败分账、R4内容/shader/cache、R5 D3DImage恢复、R6持续媒体/资源线程寿命、R7 C5/D4、R8官方PRERELEASE player、R9全项目E2E交付；依赖、自主/需用户、可复用范围及相对规模只在总计划维护。

最少用户来源是historical binding、bilinearspan原项目/依赖或权威生成合同、官方PRERELEASE player协议/ownership及匹配oracle；有效HW交互session/GPU/权限是验收环境条件，先自主核验现有条件，不能把33失败全归环境。普通MediaPlayer不是record-packet player。

两个required/parent继续Blocked，stage4Closed=false，总门Red；本次文档同步完成不等于生产或正式验收完成。
