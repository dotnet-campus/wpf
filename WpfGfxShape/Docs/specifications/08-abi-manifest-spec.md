# wpfgfx ABI Manifest 规范

本页及两主题页共同构成权威schema；probe不替代生产manifest。产品当前状态只见[恢复入口](../current/current-recovery.md)，本页不认证任何产物。
公开ABI验证遵循[严格对照合同](../current/current-comparison-contract.md)：同一调用逻辑在两个独立业务进程分别调用原版/候选；角色只选择实现身份与取证目录。缺导出/错误签名是产品缺口，不能改候选侧声明、增加上层补偿或放宽比较标准。该执行合同不改变真实ABI签名、布局、错误码或所有权规格；适用范围见[当前验收矩阵](../current/current-validation-matrix.md)。
静态调用边界见[02](02-abi-and-managed-callers.md)，测试见[06](06-testing-strategy.md)。

## 1. 目的
[四层事实](abi-entry-and-type-schema.md#1-目的)。
## 2. 目标工件结构
[JSONL与baseline](abi-entry-and-type-schema.md#2-目标工件结构)。
## 3. Entry ID 与状态
[ID及来源状态](abi-entry-and-type-schema.md#3-entry-id-与状态)。
### 3.1 ID
[ID](abi-entry-and-type-schema.md#31-id)。
### 3.2 来源存在状态
[分别记录](abi-entry-and-type-schema.md#32-来源存在状态)。
## 4. `entries.jsonl` 必填字段
[全部字段](abi-entry-and-type-schema.md#4-entriesjsonl-必填字段)。
## 5. 参数字段
[有序参数合同](abi-entry-and-type-schema.md#5-参数字段)。
## 6. 托管声明记录
[每声明出现单记](abi-entry-and-type-schema.md#6-托管声明记录)。
## 7. 类型与布局 manifest
[强制类型及架构布局](abi-entry-and-type-schema.md#7-类型与布局-manifest)。
## 8. Callback manifest
[四类callback与寿命](abi-lifetime-binary-and-gates.md#8-callback-manifest)。
## 9. COM interface manifest
[完整vtable/所有权](abi-lifetime-binary-and-gates.md#9-com-interface-manifest)。
## 10. Binary manifest
[具体产物身份](abi-lifetime-binary-and-gates.md#10-binary-manifest)。
## 11. 8/7 差异处理
[不可擅删或stub](abi-lifetime-binary-and-gates.md#11-87-差异处理)。
## 12. 生成 C header 的规则
[header反向校验](abi-lifetime-binary-and-gates.md#12-生成-c-header-的规则)。
## 13. 验证门
[Accepted九项](abi-lifetime-binary-and-gates.md#13-验证门)。
## 14. 证据适用范围
[不拼接身份](abi-lifetime-binary-and-gates.md#14-证据适用范围)。
