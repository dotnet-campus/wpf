# 后续工作项

本轮Docs/Ledger清理与现行引用修复已收口，最终源码已验证；精确账只见[清理记录](../completed-work/process-cleanup.md)，本次不新增下列既有工作项。
不恢复已删除运行快照、scope overlay或旧WIP，不把清理当生产完成。

## 下一批范围

1. 有界解除GenProto审计对旧GUID原物的依赖：正式输入/源码合同与当轮artifact分离，保留原断言，不skip、不伪造报告。
2. 将仍写Investigations/GenProtoEvidence的生成工具逐个改为ignored artifacts；本轮仅修WireCompiler派生Ledger写入点，不扩成几十文件测试重写。
3. 复核records/groups中历史状态与required关系；保留sourceOwner、nativePath、依赖与readinessRule，不把历史satisfiedEvidenceIds当当前满足。
4. 普通SystemVideoSample测试的历史NativeDll绝对路径定位修复，遵守DEC-0006官方完整runtime/身份隔离。
5. 后续生产按[唯一九包/七阶段](../remaining-gap-closure-plan.md)推进，当前不是只剩外部输入。

## 不减合同

Windows x86/x64适用全部配置；ARM64不适用，DLL进程驻留，真实资源清理必需。
C5/D4、完整ABI/strict/full/E2E及真实HW/恢复仍未满；两个required/parent继续Blocked。
来源只向[唯一外部页](../all-completion-external-prerequisites.md)收敛，不索取ARM64或历史10.0.12。

## 证据边界

GenProto full尚未恢复，历史46/46非当前通过；本轮没有重跑full或生产publish。
运行结果输出artifacts，不tracked，不搬ReferenceData/Docs备份。
