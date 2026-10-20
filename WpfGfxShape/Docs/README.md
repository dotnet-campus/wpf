# WpfGfxShape 文档入口

唯一恢复入口：[next-session-handoff.md](next-session-handoff.md)。
状态只见[当前基线](handoffs/current-stable-baseline.md)，历史机器验收不是当前证据。

## 当前合同

- [决议索引](decisions/README.md)：正式用户合同，不是运行报告。
- DEC-0004：DLL 进程驻留，资源/引用/线程/callback 清理不豁免。
- DEC-0005：仅 Windows x86/x64；ARM64 不适用而非通过。
- DEC-0006：已安装官方完整 .NET 10 WindowsDesktop，同版同架构独立对照；不要求历史 10.0.12。
- [唯一总计划](remaining-gap-closure-plan.md)：九个有界包及唯一七阶段完成条件。
- [主题合同](plan-module-contracts.md)、[最终差集](final-acceptance-remaining-matrix.md)。
- [唯一外部页](all-completion-external-prerequisites.md)：确缺来源与实际 x86/x64 验收条件；retail player私有协议不是外部阻塞。
- [保护API版本详档](protection-api-contract.md)：实际build commit、HTTPS失败/成功证据与尚未证明的bool语义。
- 本次候选C79CCF1D…，托管4224/4224，原生1676/1635pass/41fail/0skip；整体未完成，不沿用前轮11fail。

## 正式规范

- [实现章程](charter-implementation-contract.md)、[证据章程](charter-evidence-and-continuity.md)。
- [源码拓扑](01-native-source-topology.md)、[ABI调用方](02-abi-and-managed-callers.md)。
- [迁移门禁](migration-implementation-gates.md)、[耦合域](migration-coupled-domains.md)。
- [Ledger范围与ID](ledger-scope-and-identity.md)、[字段与完成](ledger-records-and-acceptance.md)。
- [ABI类型](abi-entry-and-type-schema.md)、[寿命与binary门](abi-lifetime-binary-and-gates.md)。
- [测试层次](testing-levels-and-gates.md)、[差分验收](testing-comparison-and-acceptance.md)。
- [运行证据合同](evidence-run-contract.md)、[独立E2E](e2e-executable-contracts.md)。
- [生产COM方法](com-nativeaot-testing-method.md)、[MTP沙箱操作](com-acceptance-sandbox-operations.md)。

## 维护与清理

[清理记录](completed-work/process-cleanup.md)登记实际执行，[历史索引](history-index.md)标明已清理材料，不承诺旧路径存在。
最终源码已验证 NativeAOT 与真实工作区幂等执行，精确分账只见清理记录；运行生成资料只输出 ignored artifacts，不迁到 ReferenceData 或 Docs 假清理。
独立不可再生输入、生产 manifest/资源声明和正式规范保留。
[两项目收口记录](completed-work/two-test-projects-consolidation.md)是本轮测试组织裁决：Tests仅托管Tests与原生ComAcceptance，旧30项目/17目录已授权删除。GenProto历史审计不再是测试门，Ledger核心保留但不强依赖；本轮裁决覆盖旧测试组织规则，产品兼容目标不变。
整体生产仍未完成；文档清理不关闭 C5/D4、ABI、HW、寿命或 E2E 门。
