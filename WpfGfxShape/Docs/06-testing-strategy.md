# wpfgfx C# NativeAOT 迁移测试与证据策略

最新用户合同：正式测试仅 `Tests/WpfGfxShape.Tests`（托管单元/组件）与 `Tests/WpfGfxShape.ComAcceptance`（真实AOT ABI/COM/差分/PresentationCore）。T0–T5是证据层次，不是额外项目授权；原PublicAbi/Pcore等辅助源码已迁入后者，同exe角色不是新增Host。删除probe/治理与GenProto历史审计，不再要求恢复旧GUID、Ledger写入归档或逐旧用例迁移。纯协议解析/布局unit保留，生成器历史审计未保留原因见[收口记录](completed-work/two-test-projects-consolidation.md)。本裁决覆盖下列主题页的旧测试组织要求；产品兼容目标、真实失败断言和x86/x64目标不减。当前结果见[基线](handoffs/current-stable-baseline.md)。

## 当前验收与保护专项
本次最终生产Release/win-x64和标准native包已发布；候选C79CCF1D…未变，托管full4224/4224，原生full1676/1635pass/41fail/0skip，详见[矩阵](final-acceptance-remaining-matrix.md)。不能沿用前轮11失败。Retail player50参数独立差分保留；PRERELEASE私有协议不再外部阻塞。
保护必须绑定实际WPF build commit，验证false/default与true具体防护、功能返回/异常HRESULT映射、合法与可安全构造边界；不unsafe越界，不以天然数组检查直接证明忽略参数等价，不noop。当前来源未闭及HTTP实证见[独立详档](protection-api-contract.md)，Pcore8仍失败。desktopduplication两项保持真实失败，不skip、不放阈值、不随机追绿。
x86 Debug生产与验收项目构建成功，不等于ABI/ESP运行通过；x86/x64全部适用配置仍需验收。运行输出仅ignored artifacts/.coding-agent，不写tracked ReferenceData。

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
