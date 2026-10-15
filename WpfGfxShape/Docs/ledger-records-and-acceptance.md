# Ledger：字段、状态、校验与完成定义

[Ledger 规范](07-scope-and-ledger-schema.md)§5–11正式合同，不保存当前运行数字。

## 5. `records.jsonl` 必填字段

| 字段 | 类型/规则 |
|---|---|
| schemaVersion | string，1.0.0 |
| recordId | string，全局唯一稳定 |
| recordType | PhysicalFile/ProjectSentinel/ExternalSupply/ManagedContract/BinaryArtifact/EvidenceOnly |
| scopeDisposition | ProductionDenominator/ContractOnly/SidecarEvidence/AdjacentDeployment/ExcludedByApprovedDecision |
| nativeProject | string/null；公共项空时写 sourceOwner |
| nativePath | string/null；仓库相对，外部可空 |
| fileKind | Implementation/Header/PCH/Inline/GeneratedInput/GeneratedOutput/Resource/Build/ABI/Protocol/External/Evidence |
| buildParticipation | string[]：Compile/Include/ResourceInput/GeneratedInput/GeneratedOutput/LinkInput/ProjectReference/TransportSupply/ManagedConsumer/SidecarOnly/Unknown |
| sourceAuthority | CurrentConsumedSource/FrozenGeneratedOutput/HistoricalModel/HistoricalGenerator/DerivedCopy/BuildIntermediate/ExternalBinary/Unknown |
| originalResponsibility | string，只描述原职责，不描述重构 |
| keySymbols | string[]，类型/函数/导出/资源ID |
| pchAndDirectIncludes | string[]，未调查空并说明 blocker |
| dependencies | string[]，只 Record/Group ID，不自由路径 |
| platformDependencies | string[]，Win32/COM/D3D9/WIC/DWrite/EVR等 |
| generatedGroupIds/typeFamilyIds/cycleGroupIds | 各 string[]，可空 |
| abiContractIds | string[]，ABI manifest ID |
| investigationBatch | string，调查包/批次 |
| implementationBatch | string，0–14或子批 |
| proposedCSharpPath | string/null；生产编码前非空 |
| status | 状态枚举见§6 |
| readinessBlockers | string[]，明确ID/风险，不写“稍后” |
| requiredEvidenceIds | string[]，编码前定义 |
| satisfiedEvidenceIds | string[]，只真实已执行并满足的证据 |
| knownDeviations | object[]，原因/风险/验证/批准状态 |
| ownerWorkPackageId | string/null，当前负责包 |
| lastReviewedSession | string，会话归档ID |
| notesDecisionIds | string[]，决策索引 |

ProductionDenominator 不缺 implementationBatch/status/requiredEvidenceIds；PhysicalFile 必有 nativePath；ManagedContract 必有实际项目包含状态。
未发现类别输入，哨兵 checkedCategories 显式 CheckedNoneFound，缺行不等于检查。
集中生成实现如 marshal_generated.cpp 保持独立物理记录，不散方法后删追溯。

## 6. 状态机与允许迁移

NotInvestigated → Investigated → Scaffolded → Translated → UnitVerified → DifferentialVerified → Integrated → EndToEndVerified → Accepted。
Blocked 正交：status 保持已达状态，blocked=true，blockerIds 明确（如 RISK-P0-X86-NATIVEAOT）。
- Investigated：职责/依赖/映射/风险/测试合同/未决齐全。
- Scaffolded：仅原名类型/签名/布局/薄入口，无假成功。
- Translated：逐文件近似直译，验证不足。
- UnitVerified：规定 T1；DifferentialVerified：规定 T3 原候选差分；Integrated：规定 T4；EndToEndVerified：参与最高已就绪 T5。
- Accepted：全部要求证据满足，偏差记录获批，无未决 blocker。
提升必须有 evidence.jsonl 已执行证据；build 不直接提升 UnitVerified。

## 7. `groups.jsonl` 合同

每组至少 groupId/groupType/memberRecordIds、authorityRecordIds/consumerRecordIds、invariants（ID/布局/顺序/生成/循环）、readinessRule/doneRule、requiredEvidenceIds、status/blocked、lastReviewedSession。
生成组表达 input→generator/model→frozen output→consumer；循环组列允许骨架成员，不用新抽象消回边。

## 8. 完整性校验规则

可重复校验至少：
1. ID 唯一、跨引用存在。
2. wpfgfx.vcxproj 全直接项目引用有项目/供应记录。
3. 项目项、磁盘、PCH/include、生成、资源、ABI、供应各视图有记录或 CheckedNoneFound。
4. ProductionDenominator 物理文件唯一主记录。
5. 编码前 proposedCSharpPath 与测试定义存在。
6. 原/C# 主路径无无说明冲突占用。
7. ABI 定义/托管调用方/实现回链 Ledger。
8. Accepted 无 blocker 且全部 required 满足。
9. 项目哨兵各类别闭环才 Accepted。
10. 分母、完成、阻塞、未调查可确定性计算。

## 9. 基线与可重复性

每次生成更新记录 OriginWpf revision/dirty、参与的 Silk.NET revision、SDK/MSBuild/Windows SDK/toolchain、configuration/RID、枚举方法与专用操作参数或 IDE 操作、时间/schema、输入 hash/manifest。
每次重新记录，旧调查不代当前项目求值或磁盘闭包；记录操作不授权终端脚本。

## 10. 与当前工作的关系

当前 Ledger 状态只见[基线](handoffs/current-stable-baseline.md)，逐项事实见 Ledger/*.jsonl 与 views/status.md；本策略不复制修复状态。
增量补齐 binary Evidence/Artifact、ABI、生成、资源、供应；编码前确认记录/映射/测试，结束回写实际状态/依赖/路径/偏差/证据。
[工作项](handoffs/current-work-item.md)记录切片 Record/Group ID；缺记录先补，不把缺口当已检查/完成，不使用“尚未创建”分支。

## 11. 完成定义

本规范完成不等于 Ledger 完成；以下全部满足才关闭：
- 生产分母机器可计算。
- 多视图闭环通过。
- 首批工作包从记录确定性派生。
- schema 校验无错误。
- Markdown views 与 JSONL 一致。
- 全部未知项明确 blocker。
