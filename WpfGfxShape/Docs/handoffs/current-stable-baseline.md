# 唯一当前基线

## 有效合同

仅Windows x86/x64全部适用Debug/Release正式门；ARM64不适用而非Passed（DEC-0005）。
DLL进程驻留，真实资源/引用/线程/callback清理不豁免（DEC-0004）。
已安装官方完整runtime，同版同架构独立原版/候选对照；10.0.12只是历史身份（DEC-0006）。

## 当前证据状态

本轮既有Ledger有限验收JSON、views及过期过程页清理已收口，引用与清理说明同步；清理源码NativeAOT验证成功。仅GenProto最小项目build成功，没有生产构建、COM或GenProto full。
GenProtoEvidence与历史TestResults此前已删；旧46/46及GenerationTests 3/3均属历史，不能算本轮通过。
GenProto完整审计仍依赖历史GUID，尚未恢复，不skip、不用假报告补Satisfied。
Ledger evidence保留ID/subject依赖，剥离旧run/hash/路径和执行状态；Pending不满足required。
records/groups中的旧review/session、satisfiedEvidenceIds和实现状态只是待复核映射，不是当前执行认证；原readinessRule/doneRule不变。
不把全局生产SHA替换成新身份，不将旧strict、smoke、full拼成同一运行。

## 仍未完成

C5完整实际physical-record闭包及bilinearspan原来源、D4历史authority binding未满。
两个MILCMD required/parent仍Blocked，stage4Closed=false，总门Red。
x86/x64全配置ABI/E2E、持续HW/shader/D3DImage/recovery、失败/refs/callback合同仍需实现或验收。
333保守风险不是新增穷尽清零门；有限软件、x86 CLR/Shared、selected CL成果不重领，但缺原物不宣称当前复验。

## 来源

[唯一总计划](../remaining-gap-closure-plan.md)、[差集矩阵](../final-acceptance-remaining-matrix.md)、[外部页](../all-completion-external-prerequisites.md)。
[清理记录](../completed-work/process-cleanup.md)维护执行结果，历史数字不再复制到四短。
