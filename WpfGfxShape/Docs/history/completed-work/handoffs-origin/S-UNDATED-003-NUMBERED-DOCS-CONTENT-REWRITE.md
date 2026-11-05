# 编号文档 00～11 正文级重写

> 本记录取代 `S-UNDATED-002-NUMBERED-DOCS-REVIEW.md` 中仅靠文件头标识有效性的清理方式。旧记录保留为历史，但不代表当前文档内容。

## 原因

上一轮主要修改文档头部和索引，没有删除正文中的旧“本轮/下一轮”、已完成工作包、旧项目结构、旧 Silk.NET 版本、旧测试数字和旧交接流程，仍可能导致执行者采取错误动作。

## 本轮正文修改

- `00-migration-charter.md`：删除初始规划轮次的永久工具禁令，改为当前环境与可验证证据规则；修正当前计划和交接职责。
- `01-native-source-topology.md`：全文重写为当前稳定拓扑基线，移除调查流水和旧工具边界。
- `02-abi-and-managed-callers.md`：全文重写为当前 ABI 静态基线，保留 106/107/99/8/7、句柄、所有权、错误和布局规则，删除旧调查状态。
- `03-migration-order.md`：全文重写为当前实施顺序与门禁，删除 `WP-00A` 立即领取、历史首包裁决和“无任何测试”等失效内容。
- `04-nativeaot-project-shape.md`：全文重写为当前四项目结构、构建隔离、Native AOT、依赖和架构边界。
- `05-silknet-directx-assessment.md`：全文重写为 Silk.NET 2.23.0 与 CsWin32 当前采用规范，删除旧 2.11/`WP-00G` 执行方案。
- `06-testing-strategy.md`：删除旧 `WP-00B/WP-00C` 运行数字，改为长期测试承载与架构规则。
- `07-scope-and-ledger-schema.md`：把未来创建语气改为当前 `RepairRequiredPartial` Ledger 结构和持续校验规则。
- `08-abi-manifest-spec.md`：移除旧工作包建立顺序作为当前语境，明确目标 ABI 工件结构。
- `09-test-evidence-and-e2e-contract.md`：删除已完成 `WP-00A/WP-00B`、固定旧包版本和旧测试数字；区分已存在与目标 harness。
- `10-session-continuity-protocol.md`：全文重写为当前 `next-session-handoff` + `current-*` + 独立完成历史结构。
- `11-first-coding-step.md`：全文缩减为已完成构建隔离结果，删除全部待办、复选框、停止条件和下一动作。

## 一致性结果

- 不再存在“立即可领取 WP-00A”；
- 不再存在“测试项目尚未创建”；
- 不再要求每轮覆盖 `next-session-handoff.md`；
- 不再把旧 Silk.NET 2.11 作为当前绑定；
- 不再在规范文档维护旧全量测试数字；
- Ledger 明确已存在且为 `RepairRequiredPartial`；
- 当前唯一动作仍由 `handoffs/current-work-item.md` 维护。

本次只修改文档，未修改生产代码，未运行构建或测试。
