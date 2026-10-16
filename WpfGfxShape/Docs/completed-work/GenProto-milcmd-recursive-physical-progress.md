# GENPROTO-MILCMD 递归物理证据进展（未闭父级）

本轮新增 `MilcmdPhysicalClosureTests.cs`；修正原生布局诊断 DLL 的卸载行为，保持进程驻留。不改生产、不改自动生成文件或历史 frozen，不重 publish/COM full。

## 已执行并回读

- 当前原始 ResourceGenerator 隔离再生成定向通过；全文/语义差分随各输出保存。当前 authority/input manifest 存在，不再描述为当前 manifest 缺失。历史 CLR/hash binding 另列，未伪造。
- 去重入口逐 ID 及实际递归物理清单见 [source-manifest.json](../../Investigations/GenProtoEvidence/milcmd-physical-05f3d6983cde4339b77bcf1641a2dc50/source-manifest.json)。范围为生成器/原生树、Common Graphics、PresentationCore、迁移 producer/consumer 和验证器的保守递归包络；附逐文件 SHA、include/project 边及未解析项。不是声称所有文件均已执行或所有 C 已覆盖。
- [机器报告](../../Investigations/GenProtoEvidence/milcmd-physical-05f3d6983cde4339b77bcf1641a2dc50/report.json)，SHA256 `44327273E8B9C37E1D991648F183BA5CE118A042D89311CC3E102BA191085973`：sourceStable=true；保留全部 native dispatch case 原文/packet/call、实际 managed ProcessUpdate/UpdateResource 参数与方法体、generated/processed struct/enum/pack/FieldOffset/资源引用声明；不把枚举声明当作 native compiler 比较。
- 现存 factory/pen 独立报告及原始 AOT TRX/identity 已复制并逐字节 hash 回读。AOT 为复用，不是本轮原生 execution；pen model 不冒充原生执行。逐命令没有实际断言绑定的部分明确未证，不按测试名猜测。
- build、定向验证、最新源码 full 及 full 后独立归档已执行。运行数字、原始 TRX/RunID/hash 和 source hash 唯一见[状态源](../handoffs/current-stable-baseline.md)。没有最终回归后源码尾改。

## Ledger 和正式剩余

两个 MILCMD required 行已绑定本轮 report/run/hash、executed=true，仍 Blocked。独立已满足条款为清单冻结与读回、dispatch/update 签名枚举、当前再生成差分保存与输出绑定验证；这些有限验证不是完整语义等价、wire 布局或执行覆盖 Satisfied。既有 consumer/factory 独立 Satisfied 保持，不降低 required、不改 parentClosed。

本地仍须完成：未解析 property/SDK/include 边按真实 build evaluation 分类；全部命令 wire 的 native/managed size/offset/pack/enum 分母比较；每个 producer/资源引用/update 的具体生产断言绑定。不能将这些自主项都归外部。历史 binding、ARM64 和其它真实外部前置仍按[唯一输入页](../all-completion-external-prerequisites.md)；不伪造历史 CLR 或 ARM64。seven formal conditions 不变、stage4Closed=false、总门 Red。
