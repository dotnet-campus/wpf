# 当前唯一恢复入口

本轮既有 Docs/Ledger 清理已完成最终集成，现行引用已修；清理源码已验证 NativeAOT 与真实工作区幂等执行，不是生产验收。
精确执行与本次/历史分账见[清理记录](completed-work/process-cleanup.md)及 ignored artifacts/process-cleanup。

## 先读

1. [执行规则](handoffs/current-execution-rules.md)。
2. [唯一基线](handoffs/current-stable-baseline.md)。
3. [后续工作项](handoffs/current-work-item.md)。
4. [唯一总计划](remaining-gap-closure-plan.md)：七阶段最终目标不减。

## 有效合同

- [DEC-0004](decisions/DEC-0004-process-lifetime-module.md)：DLL进程驻留，无卸载/重载/热替换门；真实资源清理继续。
- [DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)：仅Windows x86/x64全部适用配置，ARM64不适用而非通过。
- [DEC-0006](decisions/DEC-0006-installed-official-runtime-contract.md)：官方完整runtime，同版同架构独立对照；10.0.12非必需补包。
- 重复 scope overlay 已归并上述既有决议，不会下轮自动重建。

## 当前结论

整体未完成，不是只剩外部输入；C5/D4与两个required/parent仍Blocked，stage4未关闭，总门Red。
GenProtoEvidence及历史TRX原物已删除，历史46/46不是当前通过；完整GenProto审计历史GUID依赖尚未恢复，本轮不跑full。
Ledger evidence现在只保留ID/依赖合同，缺当前原物不能Satisfied；records/groups/source ownership仍保留。
外部输入只见[唯一外部页](all-completion-external-prerequisites.md)，不恢复ARM64/旧patch/卸载门。
