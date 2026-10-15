# wpfgfx C# NativeAOT 迁移测试与证据策略

权威测试合同由本页与下列主题页共同构成；不维护运行数字。E2E-00 只证明 probe，不能代生产 ABI/E2E。当前只见[基线](handoffs/current-stable-baseline.md)。

## 1. 测试目标
[五类问题](testing-levels-and-gates.md#1-测试目标)。
## 2. 证据分层
[T0–T5 完整合同](testing-levels-and-gates.md#2-证据分层)。
### T0：静态清单与来源证据
[来源合同](testing-levels-and-gates.md#t0静态清单与来源证据)。
### T1：托管单元与布局测试
[单元/布局](testing-levels-and-gates.md#t1托管单元与布局测试)。
### T2：Native AOT 发布后托管 ABI 集成测试
[发布后边界](testing-levels-and-gates.md#t2native-aot-发布后托管-abi-集成测试)。
### T3：原实现与 C# 差分
[差分合同](testing-levels-and-gates.md#t3原实现与-c-差分)。
### T4：组件集成
[组件闭环](testing-levels-and-gates.md#t4组件集成)。
### T5：真实 PresentationCore E2E
[真实链](testing-levels-and-gates.md#t5真实-presentationcore-e2e)。
## 3. 渐进 E2E 阶梯
[独立 E2E-00–09](testing-levels-and-gates.md#3-渐进-e2e-阶梯)。
## 4. 批次门禁
[批次0–14](testing-levels-and-gates.md#4-批次门禁)。
## 5. 逐文件测试契约
[编码前定义](testing-levels-and-gates.md#5-逐文件测试契约)。
## 6. 数值、几何与像素比较
[比较规则](testing-comparison-and-acceptance.md#6-数值几何与像素比较)。
## 7. 生成协议测试
[冻结与负例](testing-comparison-and-acceptance.md#7-生成协议测试)。
## 8. ABI、COM 与 callback 测试
[专项合同](testing-comparison-and-acceptance.md#8-abicom-与-callback-测试)。
### ABI
[ABI](testing-comparison-and-acceptance.md#abi)。
### COM
[COM](testing-comparison-and-acceptance.md#com)。
### callback
[callback](testing-comparison-and-acceptance.md#callback)。
## 9. 生命周期和故障注入
[真实寿命；卸载不适用](testing-comparison-and-acceptance.md#9-生命周期和故障注入)。
## 10. 平台矩阵
[全部适用配置/架构](testing-comparison-and-acceptance.md#10-平台矩阵)。
## 11. 证据存储
[完整留存](testing-comparison-and-acceptance.md#11-证据存储)。
## 12. 完成定义
[全部状态和 Accepted 条件](testing-comparison-and-acceptance.md#12-完成定义)。
## 13. 当前测试承载
[MTP、probe 与独立 PublicAbi 边界](testing-comparison-and-acceptance.md#13-当前测试承载)。
## 14. 架构化承载
[真实运行要求](testing-comparison-and-acceptance.md#14-架构化承载)。
