# wpfgfx 测试证据与渐进 E2E Harness 契约

本页与三主题页共同构成完整权威契约，不维护运行数字；E2E-00不替代生产ABI/E2E。状态只见[基线](handoffs/current-stable-baseline.md)。

## 1. 目的
[证据目标](evidence-run-contract.md#1-目的)。
## 2. 当前与目标承载边界
[标准MTP、历史probe、独立PublicAbi](evidence-run-contract.md#2-当前与目标承载边界)。
## 3. 测试框架与包版本决策
[MSTest规则](evidence-run-contract.md#3-测试框架与包版本决策)。
## 4. 测试运行单元
[run工件](evidence-run-contract.md#4-测试运行单元)。
### 4.1 `manifest.json` 必填
[完整身份](evidence-run-contract.md#41-manifestjson-必填)。
### 4.2 `commands.jsonl`
[专用操作留证，不授权脚本](evidence-run-contract.md#42-commandsjsonl)。
## 5. 角色与进程隔离
[高风险隔离与COM限制](evidence-run-contract.md#5-角色与进程隔离)。
## 6. 原/候选切换协议
[身份/部署](evidence-run-contract.md#6-原候选切换协议)。
## 7. 结果模型
[机器输出与独立比较](evidence-run-contract.md#7-结果模型)。
## 8. Golden 与差分规则
[不可随失败覆盖](evidence-run-contract.md#8-golden-与差分规则)。
## 9. E2E 阶梯可执行合同
[E2E-00–09逐项全文](e2e-executable-contracts.md#9-e2e-阶梯可执行合同)。
### `E2E-00` Native AOT probe
[00](e2e-executable-contracts.md#e2e-00-native-aot-probe)。
### `E2E-01` version / minimal COM
[01](e2e-executable-contracts.md#e2e-01-version--minimal-com)。
### `E2E-02` connection/channel
[02](e2e-executable-contracts.md#e2e-02-connectionchannel)。
### `E2E-03` resource command
[03](e2e-executable-contracts.md#e2e-03-resource-command)。
### `E2E-04` software pixels
[04](e2e-executable-contracts.md#e2e-04-software-pixels)。
### `E2E-05` 真实 PresentationCore bitmap
[05](e2e-executable-contracts.md#e2e-05-真实-presentationcore-bitmap)。
### `E2E-06` hardware/HWND
[06](e2e-executable-contracts.md#e2e-06-hardwarehwnd)。
### `E2E-07` D3DImage
[07](e2e-executable-contracts.md#e2e-07-d3dimage)。
### `E2E-08` media
[08](e2e-executable-contracts.md#e2e-08-media)。
### `E2E-09` lifecycle closure
[09](e2e-executable-contracts.md#e2e-09-lifecycle-closure)。
## 10. 环境标签与能力检测
[缺能力不记通过](evidence-capabilities-and-failures.md#10-环境标签与能力检测)。
## 11. 失败注入
[全部类别](evidence-capabilities-and-failures.md#11-失败注入)。
## 12. Evidence ID 与状态
[证据提升纪律](evidence-capabilities-and-failures.md#12-evidence-id-与状态)。
## 13. 每个文件编码前的测试合同
[预定义验证](evidence-capabilities-and-failures.md#13-每个文件编码前的测试合同)。
## 14. 超时、重复与随机测试
[可重放与超时](evidence-capabilities-and-failures.md#14-超时重复与随机测试)。
## 15. 已锁定承载与未关闭能力
[承载和未决设计](evidence-capabilities-and-failures.md#15-已锁定承载与未关闭能力)。
