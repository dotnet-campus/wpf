# DEC-0005：正式验收仅 x86 / x64

状态：Accepted（用户最新明确范围裁决，无需再次批准）。

权威原文：“ARM64工具链及真实Windows ARM64环境，直接忽略，只要能确保x86和x64即可”。

- 当前正式架构集合仅 Windows x86、x64；Debug/Release、ABI/布局、生产加载与消费者、资源/失败/线程/callback/关闭、七阶段、A/B/C、E2E-00–09及最终同身份 strict/full 的全部适用条件不缩减。有限 x86 Shared/CLR 或 x64 Green 不等于完整矩阵通过。
- ARM64 编译、布局、交叉编译、真实运行、工具链/SDK/机器交付统一为 **NotApplicable-UserScopeDecision**，不是 Passed、Satisfied、能力缺失待补或阻塞；不从 x86/x64 外推 ARM64 支持。
- 本决议覆盖所有现行 Docs 与历史合同承接中的 ARM64 必需门（含 C/C5、4/5、6C、7、ABI schema 架构字段及 RealArm64Host 能力）。保留 ARM64 历史 MSB8020、MissingExecution、失败记录和冻结 JSON 原值；不删除、不改写为通过，不扩减原风险记录数字。

## Ledger 解释覆盖（仅文档，不修改冻结记录）

适用 `Ledger/mediaown-required-finite-acceptance.json`，以及 `Ledger/evidence.jsonl:58–59` 两个原 required：`EVID-GENPROTO-MILCMD-CLOSURE`、`EVID-GENPROTO-MILCMD-REGEN-DIFF`。

| 原字段/合同 | 当前有效解释 |
|---|---|
| `c5Subclauses.arm64ToolchainAndExecution = BlockedMissingExternalToolchainAndEnvironment`、原 required 中 ARM64 编译/运行/布局要求 | 原值作为历史证据保留；当前适用性为 NotApplicable-UserScopeDecision，不计阻塞，不偷偷改 Satisfied |
| `autonomousFormalRemaining` 及原详档称 ARM64 为 formal gap | 仅该架构子句由本决议撤销；其他原条件不豁免 |
| C5 / 两 required / parent | x86/x64 原 bilinearspan 项目来源、完整实际 physical-record 闭包及 D4 历史 binding 等仍未满，继续 **Blocked**；`parentBlocked=true`、`c5Satisfied=false`、`stage4Closed=false` 保持，总门 Red |

机器字段另有[显式范围覆盖记录](../../Ledger/current-user-scope-overrides.json)，保留原值而非偷偷Satisfied。冻结机器报告不是此次范围修订后的重新执行结果。仅文档范围覆盖，不新增或伪造运行证据。历史检索见 [隔离索引](../history-index.md)，当前矩阵见 [最终矩阵](../final-acceptance-remaining-matrix.md)。

运行时精确版本的独立来源审计见 [单一详档](../windowsdesktop-version-provenance.md)；不得将 ARM64 排除误解为 x86/x64 或版本验收已经通过。
