# wpfgfx 迁移范围与机器可读 Ledger 规范

本页与两主题页共同构成权威规范。生产分母是最终 DLL 完整生产闭包，不等于直接编译下限；没有 Ledger 与测试定义不得编码。
产品当前状态只见[恢复入口](../current/current-recovery.md)。JSONL保存逐项记录，通过校验的派生视图用于阅读，不独立提升证据状态；原调查revision、旧基线及schema校验成功均不认证当前产物。

Ledger中的ABI/差分证据须满足[严格对照合同](../current/current-comparison-contract.md)：原版/候选独立进程、同冻结调用逻辑与输入，不能以role分支补偿候选。公开导出缺失或签名错误记产品缺口及阻塞，不以候选专属调用绕过后提升为DifferentialVerified/Accepted。最终覆盖范围见[当前验收矩阵](../current/current-validation-matrix.md)，不改变本规范的生产分母或稳定ID。

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
