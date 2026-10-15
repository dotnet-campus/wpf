# wpfgfx 迁移范围与机器可读 Ledger 规范

本页与两主题页共同构成权威规范。生产分母是最终 DLL 完整生产闭包，不等于直接编译下限；没有 Ledger 与测试定义不得编码。
运行状态只见[基线](handoffs/current-stable-baseline.md)，JSONL 与通过校验的派生视图为机器事实；原调查 revision 不冒充当前。

## 1. 目的
[生产分母定义](ledger-scope-and-identity.md#1-目的)。
## 2. 范围矩阵
[全部生产、合同、旁支、供应边界](ledger-scope-and-identity.md#2-范围矩阵)。
## 3. 工件位置与格式
[JSONL与派生视图](ledger-scope-and-identity.md#3-工件位置与格式)。
## 4. 稳定 ID
[完整规则](ledger-scope-and-identity.md#4-稳定-id)。
### 4.1 Record ID
[Record](ledger-scope-and-identity.md#41-record-id)。
### 4.2 Group ID
[Group](ledger-scope-and-identity.md#42-group-id)。
### 4.3 Work package ID
[工作包](ledger-scope-and-identity.md#43-work-package-id)。
## 5. `records.jsonl` 必填字段
[完整字段与附加必填语义](ledger-records-and-acceptance.md#5-recordsjsonl-必填字段)。
## 6. 状态机与允许迁移
[证据状态提升](ledger-records-and-acceptance.md#6-状态机与允许迁移)。
## 7. `groups.jsonl` 合同
[生成/类型/循环组](ledger-records-and-acceptance.md#7-groupsjsonl-合同)。
## 8. 完整性校验规则
[十项校验](ledger-records-and-acceptance.md#8-完整性校验规则)。
## 9. 基线与可重复性
[来源与重建](ledger-records-and-acceptance.md#9-基线与可重复性)。
## 10. 与当前工作的关系
[逐切片维护职责](ledger-records-and-acceptance.md#10-与当前工作的关系)。
## 11. 完成定义
[六项全部满足才关闭](ledger-records-and-acceptance.md#11-完成定义)。
