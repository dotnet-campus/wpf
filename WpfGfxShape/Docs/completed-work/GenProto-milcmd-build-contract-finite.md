# GENPROTO-MILCMD build-contract 有限文档收口

## 证据与本次边界

本次仅实际读取既有报告并编辑文档/Ledger，不修改代码，不构建、发布、测试或执行终端脚本。最新回归身份、计数及原始TRX统一维护在[唯一状态源](../handoffs/current-stable-baseline.md)，不是本次Docs执行产生的新运行。

- [交付报告](../../Investigations/GenProtoEvidence/milcmd-delivery-f8534606a538471d9d0237dd9db8d95a/report.json)：SHA256 `918ADF00F0B40262358C3263647CFB2CCFD566335F2E71EA541E39FD3A1EFBCD`。
- [build-contract报告](../../Investigations/GenProtoEvidence/milcmd-build-contract-b6261bbffd564a0998b8cc208ac28724/report.json)：SHA256 `B7E69AD79704EB74AB49F34BBDF97814D43D2F687711B9197108D447F5CFEF4C`。
- 实际读取交付报告的original/copy/TRX身份、Counters、buildContract、source与assembly身份，以及build-contract的previousCounts、boundedClassificationCounts、auditScope、wire分母/缺差/MediaPlayer原因及未测量行。
- 上述报告hash采用交付给定值及报告内绑定，本次不另行计算文件hash。交付报告的fullyParsed=true是既有执行记录，不冒充本次完整解析全部报告。

## 两套分类分母不得混用

| 范围 | 真实分类/测量状态 | 验收边界 |
|---|---|---|
| 历史固定721边审计 | Undetermined303、ProductionResolvable91、SystemSdkExternal154、MisidentifiedNonCodeText170、ConditionalNotApplicable3 | 固定审计分类，不把未知边全部归用户输入，不将文本误识别当代码缺失 |
| production conservative边集 | ProductionResolvable1729、Undetermined332、ConditionalNotApplicable2、RealMissing1、SystemSdkExternal125、BuildGenerated7 | 独立保守分类，不与历史721相加或替代正式生产分母 |
| 历史审计触达 | ReachedConservativeProductionEnvelope543；NotReachedNotProvenExcluded178 | 未到达不是已证明排除，不可假exclude |

scopePolicy仅取三个正式native project roots及显式references/imports/items、按序include闭包；历史全WPF保守包络只用于审计，不作正式分母。Debug/Release × Win32/x64/arm64的逐项目/逐translation-unit evaluated ClCompile、imports、include顺序和defines仍缺确证；生产实际选中的WindowsTargetPlatformVersion/VCToolsVersion及SDK/VC header hash亦未确定。安装了header不等于被选用，未知XML条件/property函数/preprocessor分支继续Blocked。

## wire盘点不是compiler测量

native116 / managed108 / dispatch116 / union116只是报告声明与dispatch盘点。全部wire的native sizeof/alignof/offsetof、managed编译后size/offset/pack及enum宽度/值尚未成组测量；报告行明确RequiredNotMeasuredByThisTest，measuredOffset=null。源码FieldOffset/Pack不能冒充实测。

managed缺差共8项：
- MILCMD_BITMAP_SOURCE
- MILCMD_CHANNEL_CREATERESOURCE
- MILCMD_CHANNEL_DELETERESOURCE
- MILCMD_CHANNEL_DUPLICATEHANDLE
- MILCMD_TRANSPORT_DESTROYRESOURCESONCHANNEL
- MILCMD_TRANSPORT_SYNCFLUSH
- MILCMD_VISUAL_CREATE
- MILCMD_MEDIAPLAYER

前7项有Resource.xml的UnmanagedOnly XML依据，只说明managed意图不生成；native dispatch、布局与执行合同仍required，不能删除分母或宣称等价。MILCMD_MEDIAPLAYER继续Blocked：须确定ShouldGenerate(ManagedClass, MediaPlayer) section selection与精确CommandTypeName绑定；显式MediaPlayer Update的UnmanagedOnly不能单独证明该resource-origin packet映射。

## Ledger、下一模块与正式门

Ledger只追加本轮有限分类及wire测量待证record；不改既有required/consumer/factory记录，不将未定项标Satisfied。required父级保持Blocked，stage4Closed=false；本次没有完成完整wirecompiler组。

下一唯一自主模块为完整wirecompiler116测量＋8差映射：Win32/x64逐命令绑定编译器、声明、packing与真实native/managed结果，成组比较size/align/offset/pack/enum并逐一解释8差；ARM64真实compiler/runtime仍按外部前提保留。具体producer/update/资源引用生产断言绑定属于后续正式余项，不把本次盘点当执行覆盖，也不将本地自主差分全推给用户。

生产身份未变；同SHA历史COM full与strict只见状态源，明确不是本轮新跑。四外部authority历史binding、ARM64 toolchain/runtime、完整10.0.12 runtime、官方player protocol/oracle，以及HW可观测前提按[唯一外部页](../all-completion-external-prerequisites.md)保持Open，不stub、不把失败全部归环境。DLL进程寿命驻留，无unload/reload。

七阶段、七项正式条件、A/B/C及[独立E2E权威合同](../e2e-executable-contracts.md)不删减；[总计划](../remaining-gap-closure-plan.md)和[最终矩阵](../final-acceptance-remaining-matrix.md)继续有效。文档本轮已收口，未测量的是后续自主模块，不是文档待办；总门Red，不宣称替换资格。
