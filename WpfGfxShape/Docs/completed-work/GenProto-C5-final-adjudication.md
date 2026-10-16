# C5 唯一正式裁决：当前源码已实测，父级仍 Blocked

唯一逐行表：[C5-final-adjudication.json](../../Investigations/GenProtoEvidence/c5-final-adjudication/Results/C5-final-adjudication.json)。summary.json 只是同次计数投影。风险原文保留在表中 Signature，不删未知、不自动 NA。本页取代旧轮“必须清零332候选”的恢复任务。

## 实测身份与精确数字

当前审计器 Release build 成功，0 warning/0 error。首次 framework-dependent 运行因沙箱无 runtime 未启动；自包含 win-x64 发布后执行成功。边界纠偏字段修改后已再次 build/publish/run，最终程序集 SHA256 `F2590E8CFABD36E4DCE7844CC9467379D2460F430A984D98B4A8AF4D285382FD`，runUtc `2026-10-16T05:25:13.4747388+00:00`，X64，非 ARM64。之后仅修改 Docs/Ledger，无未测源码尾改。未重跑 GenProto46，未改生产、未生产 publish/COMfull，DLL 驻留 unload=N/A。

程序完整读取20份 evaluation、Ledger、旧 reconciliation、12配置16 TU原CL材料与被引用声明源码，保留216项输入身份。最终程序重新读取并解析整份报告，反序列化全部17,377行，与内存原行逐行序列化比较，数量或任何行不符即失败；本次通过完整回读校验后才写summary。summary已完整回读，人工另核对头部、状态与原required关联，不冒称逐字人工阅读全部记录。

| 域 | 数量 | 证明层次 |
|---|---:|---|
| selected properties | 240 | 168当前值；72 ARM64值行保留 MissingExecution，不是72个缺工具链 |
| active imports | 3672 | MSBuild实际选择及原import行；不是运行消费者 |
| ClCompile / ProjectReference | 858 / 150 | item选择；1002已证item，6个bilinearspan配置引用缺项目 |
| CL实际物理输入观察 | 12124 | #line到达；不证明完整父子边/所有宏分支 |
| 旧风险候选 | 333 | 106 build +227 include；不是required分母 |
| build候选裁决 | 106 | 106源行已定位；13适用声明已证、92条件未证、1缺原项目 |
| include候选裁决 | 227 | 227源行已定位；0精确指令适用性已证、227仍未证 |
| 总审计记录 | 17377 | 168+3672+1002+72+7+12124+92+13+227；7缺项目行=6配置item+1风险行，只缺1个项目 |

333=13+92+227+1；当前未知候选319，不是旧332。NA=0；源行已定位不等于适用已证。Ordinal17045–17377全部保留，不把任意未知写成外部或Excluded。13条只证明具体声明在保留evaluation被选择，不证明所有配置。

## 为什么循环，以及原权威允许的有限边界

权威是 `Ledger/groups.jsonl` 的 GENPROTO-MILCMD：

- readinessRule 原文：`All physical inputs, generator components, frozen outputs and consumers have Ledger records.`
- doneRule 原文：`Isolated regeneration and semantic/full-text differential evidence satisfy the group requirements.`
- invariants：model/schema/generator/frozen outputs/consumers分别追溯；差分审阅前不得覆盖冻结输出。
- requiredEvidenceIds 只有 CLOSURE、REGEN-DIFF、CONSUMER-DECLARATIONS。原意另见 `Docs/investigations/superseded/stage4-ledger-and-last-contract-closure-plan.md:68–82`；当前 evidence.jsonl:58–59仍为Blocked，历史绑定与ARM64未豁免。

6 physical+3 evidence是入口身份，不表示所有依赖当然已证；反之，保守扫描发现的From/Reference不是适用生产合同。循环源于将旧production分类332 undetermined+1 realMissing改写为“必须穷尽333才能完成”，随后又要求所有间接候选完整宏/父边，混淆风险包络、实际编译选择、运行断言与历史authority。原条款要求完整实际生成追溯，不要求对未证适用的全扫描包络建立每条精确父边。

**独立裁决：全333穷尽不是新增blocking验收门。** 原风险表及319未知保持；并不判它们不适用，也不宣称所有实际输入已记录。若某行证明属于正式生成输入/组件/冻结输出/消费者且缺record/映射，必须按该具体原条款保留阻塞；不能仅凭unknown自动升级，亦不能凭未到达排除。当前已证必须保留的是bilinearspan显式项目引用，以及历史binding、ARM64等正式缺口。C5仍Blocked，不以撤销额外门假清零。

## 每个正式 required：必要证据、当前充分性和 gap

| 正式ID及原意 | 必要且足以验该条款的证据 | 当前证据 / gap |
|---|---|---|
| EVID-GENPROTO-MILCMD-CLOSURE：完整生成输入/输出/消费者及映射清单 | 实际版本/配置的输入与schema、generator调用、冻结输出清单及生成映射、真实消费者/签名/既有record；适用依赖必须有来源，所需架构与原有执行断言独立绑定 | C1–C4有限Satisfied保持：P源码/签名、R378映射、F97 IDs/76构造/21拒绝与既有运行；当前20 evaluation、12配置16 TU及selected imports已证。C5不充分：wpfgfx适用bilinearspan原项目确缺；ARM64真实编译/执行及完整实际物理记录闭包尚不能确认。92/227只是风险，不额外要求全清零。Blocked |
| EVID-GENPROTO-MILCMD-REGEN-DIFF：隔离再生成和语义/全文差分，不覆盖冻结基线 | 冻结输入/generator/invocation身份、隔离run、逐输出baseline/generated/diff/hash与语义比较；原历史输出的generation/build authority绑定单独提供，不用当前hash替代 | D1–D3有限Satisfied：既有R378输出、275 byte-exact、378所列范围等价及身份；D4历史CLR/hash/输入/build版本binding缺，当前Resources.rsp无ProtocolFingerprint不能替代。Blocked |
| EVID-GENPROTO-CONSUMER-DECLARATIONS：消费者声明追溯 | 实际消费者源码/签名与生成声明匹配、报告身份和既有断言绑定 | 既有Satisfied保持；本轮CL证明所选消费输入到达，不冒称所有命令运行，也不重新领取已闭项 |

P/R/F及原C1–C5/D1–D4原文索引见[原条件表](GenProto-required-final-reconciliation.md)。116dispatch、108managed wire、8native-only、97factory各为不同合同，不能相加。packet消费者/API功能不等于历史原版packet trace；两transport packet/独立discard未知保留，但不自动变成原生成required新门。

## 稳定、有限完成定义与下一动作

本轮目标是当前源码build/run、准确计数、按原required纠偏、报告及Docs/Ledger同步；这些执行动作已完成，不等于产品/阶段4完成。正式完成必须原三ID及其既有架构/追溯条件充分，不能承诺缺资源也能完成。

证明层级固定为：源码追溯 → 当前编译选择 → 实际运行消费者 → 历史authority。每层只结算该层，不相互顶替。未知风险不追加无限门；已证适用缺口不删除。当前没有证据确定必须修改的生产方法，故不虚派一个生产修复或继续扫描任务。

下一最优有界动作是接收原 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/bilinearspan/bilinearspan.vcxproj` 及真实依赖/来源版本，或可重现的原权威生成合同；只核对原GUID `{3A6A5D23-CB65-4685-AAC9-0969054701EA}`、既有四配置引用/输出/record归属。不拼项目、不删引用、不重采同16 TU。其余确缺：历史CLR/generator/input/build authority包、ARM64编译工具链及真实Windows ARM64授权机器、合法完整WindowsDesktop10.0.12、官方player协议/ownership/oracle、可观测HW/control会话与测试权限，规格见[唯一外部页](../all-completion-external-prerequisites.md)。

C5、CLOSURE、REGEN-DIFF及父级Blocked，stage4Closed=false、总门Red。生产 `3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D` 不变；原七阶段/ABI/寿命/E2E不减。资源到位后仍须真实验证，不能预先承诺全部Satisfied。
