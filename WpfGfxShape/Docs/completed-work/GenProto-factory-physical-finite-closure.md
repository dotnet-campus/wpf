# GenProto factory physical 有限证据闭合

## 本轮实测

最新 `FactoryPhysicalClosureTests.cs` 与 Ledger/archive 源码已成功 build，定向 Factory 1/1 通过；随后全 GenProto 一次 **35 total / 35 pass / 0 fail / 0 skip**。无需源码修复，本轮未改生产、测试源码、required、source hash 或 golden，最终回归后无源码尾改。35 是验证数量，不是新增生产能力。

- 原始 TRX：`Tests/WpfGfxShape.GenProto.Tests/TestResults/lindexi_DESKTOP-51A5UGG_2026-10-16_09_04_01_net10.0.trx`。
- RunID：`4794788d-6114-400a-93cb-1b72e2642ce8`。
- TRX SHA256：`ADC1A8CE445568C0DF3BA94F413B60972E08933F48643AB9DE89F72F279BAE88`。
- 完成全回归后，单独执行已有归档测试 1/1，通过逐字节 hash 相等断言；不是第二次 full，也不是新 AOT 执行。归档及所有测试源码 hash：[report.json](../../Investigations/GenProtoEvidence/pen-delivery-1f8b985f3b8f4759b65d002a794f63e9/report.json)，同目录 `genproto-full.trx` 是该 full 的原始副本。已读 RunID/Counters/Factory 输出。
- Factory 校验源码 SHA256：`AF5312D6A29E057B456429822F0D05C9C6BD7BB66682B096F908CA8494C0848B`；实测程序集 SHA256：`5761DE9C795F4401E6D8FF5DA0547DCDB9B7E07016DA6EE75561919E387B277C`。

## 完整有限分母与机器核查

[Factory report](../../Investigations/GenProtoEvidence/factory-physical-ef1f18b2c6ee4c67905564648db4434f/report.json)，run `ef1f18b2c6ee4c67905564648db4434f`，SHA256 `6EEE7ECBE54DBB0BF511F72D3A03070B204BBFA0A3C8FA3BE19A7B91D9AD39EB`。

| 层 | 本次核查及有限结果 |
|---|---|
| source | 原生 enum 的 1..97 与 managed ID、Resource.xml 每项声明一致；逐文件真实 SHA 记入 sourceFiles |
| generation | 当前生成 factory 与原生 factory 换行规范化后相等；生成文件 SHA、authority 输入逐文件 SHA 验证。绑定 `regeneration-3b4d442fa1f74575893393d760eaf4cf/results.json` 及 authority.json，不宣称历史 CLR 重放；Factory 测试自身不执行生成器 |
| producer / wire | SameThreadChannel → packet writer → 12-byte create packet；字段 0/4/8、native 字段签名和 dispatch 调用匹配；公开消费者 Stdcall export 签名核查 |
| consumer | 76 concrete factory cases 与 76 managed constructor 对应，每个真实 class 声明与文件入报告；其余 21 项拒绝。97 rows 无遗漏；ordinary GlyphRun 的普通 create 忽略及后续 delete 特例另有三条实际 passed 结果，不外推支持能力 |
| AOT execution | 97 项分别绑定既有真实 public AOT testName/testId/outcome，并核查生产 SHA、同任务 identity 和 AOT RunID；并非本轮重新执行 AOT |

实际 AOT 来源为现存 `artifacts/sandbox-readback/dispatcher-fixed-full/Results/dispatcher-fixed-full.trx`，RunID `262f277c-5f57-44d7-80a2-1ece70cba1e0`，SHA256 `78573F4B8BAFD9B1B29BAD686D28745DD453F93545A054441DB5507E42A9B3A0`。同任务 aot-identity SHA256 `393A122FF410B071B331CCDB0B68F690D63DFF6372027D2D44A89E037F1F84EE`。这些原物存在且测试通过，无需伪造旧路径或改映射。报告同目录保留 TRX/identity/generation/authority 副本。

生产仍为 `3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D`。既有同 SHA COM full **1616/1583/33/0**，strict **Missing2/Extra0**、有限 LifecyclePassed=true、AllAbiPassed=false；本轮不重 publish、不重 COM full，不把复用称新运行。33 HW/control 失败不隐藏、不全部归环境。

## Ledger 与边界

`Ledger/evidence.jsonl` 的 `EVID-GENPROTO-FACTORY-PHYSICAL-FINITE` 已由成功测试写入并回读：IndependentSubrequirement / Passed / Satisfied / executed=true，绑定上述 report 路径、runId、hash。schema-version 的 evidence count 随真实记录更新；不降低父级 required。报告 `errors=[]`、`parentRequiredSatisfied=false`、`stage4Closed=false`。Satisfied 仅表示上述当前源有限映射链全部有证，不是全 physical、全布局、历史 generation authority 或完整 producer/public-export/HW 闭包。C、历史 binding、ARM64 正式条件保持 Blocked；哈希存在本身不构成闭包。

## 下一自主正式门与外部剩余

不重领已闭多 Dispatcher、factory、typed/generation 或 async drain。下一自主门为 **GENPROTO-MILCMD 超出 factory 的全 generated → mapping → producer → wire → consumer 物理记录闭包**：以 `Ledger/groups.jsonl` 中 GENPROTO-MILCMD 现有 memberRecordIds、authorityRecordIds、consumerRecordIds、requiredEvidenceIds 的去重集合为冻结有限分母，先输出逐 ID 清单与总数，再对每项完成声明/生成 authority/实际消费者/对应执行结果或具名缺口的完整实现及机器回读；不以目录所有文件、35 tests 或只存在 hash 替代。独立 native layout/enum/pack/size 比对另列条件，不拿 managed 声明冒充。本轮最终回读 group 后，该去重集合明确为 **6 个 physical record + 3 个 required evidence = 9 个 ID**：physical 为 Resource.xml、Resource.xsd、mcg.proj、resources.vcxproj、uce.vcxproj、wpfgfx.vcxproj 对应现有 record；evidence 为 EVID-GENPROTO-MILCMD-CLOSURE、EVID-GENPROTO-MILCMD-REGEN-DIFF、EVID-GENPROTO-CONSUMER-DECLARATIONS。该9项是下一门入口有限分母，不代表递归物理文件已全枚举；应将其依赖展开后冻结文件清单，完整实现而非以9项冒充全C。不称自主工作全部被外部阻塞。

四类真正外部剩余仅维护于[唯一输入页](../all-completion-external-prerequisites.md)：历史 authority 的 binding；ARM64 工具链及真实运行；合法完整 WindowsDesktop10.0.12；官方 player protocol/ownership/oracle。真实 original/native 可观测 HW 会话成功前提仍未满足，不能把全部失败转成新增用户硬件包。DLL 进程驻留不卸载。总计划继续，正式总门 Red，本模块交付不等于总完成。
