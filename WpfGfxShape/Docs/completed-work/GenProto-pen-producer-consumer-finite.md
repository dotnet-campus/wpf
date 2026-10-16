# GenProto：Pen producer / consumer 有限实执行交付

本页只记录独立 GenProto 子合同，不更新四份短状态页，不代表 C/5A 或阶段4完成；未修改生产代码，未运行 COM 测试或发布，新增原生代码 0 行。

## 本次实际关闭的有限子条款

新增 `Tests/WpfGfxShape.GenProto.Tests/PenConsumerContractTests.cs`，不是重跑 LayerContractTests 的硬编码字段读写：从冻结 `wgx_core_types.cs` 提取真实 MIL_PEN_DATA、MIL_PEN_CAP、MIL_PEN_JOIN，从生产 `DashStyle.cs` 提取真实 `GetDashData` 方法体，Roslyn 隔离编译并执行。仅集合/属性提供支架，方法体不改；移除孤立预处理指令。首次编译暴露 CS1028 后已修复并重新实际通过。

- dash 输入数 0、1、2、3、5：逐一执行 producer，验证真实输出 double 数组、空数组/null、DashArraySize=count×8、DashOffset，以及全部八字段内容/偏移。
- producer 结构实测 44 字节；读取既有 Win32/x64 独立 native-core 测量，逐字段比较，并用实际测量的 48 字节 stride 构造双记录。第二记录 Thickness=9.5；错误使用44步长读不到第二记录，44–47尾填充保持0xA5。证明字段兼容不等于数组/按值 ABI 兼容。
- consumer 是明确标注的 C# 原生算法模型：按 DashArraySize/8 消费、double→float、奇数重复为偶数，验证整个输出序列；同时绑定真实 geometry_api.cpp 的相关表达式及 SHA256。没有执行 CPlainPen、原生分配器或生产 InitializePen，不能用此模型跨越生产 ABI 硬门。

## 原始结果与可复核归档

全量实跑 **34 executed / 34 passed / 0 failed / 0 skipped**，原28项加五组 producer/consumer 用例及一个归档用例。随后单跑归档用例通过，以保存已经完成的全量TRX，而非把前一次5项定向跑冒充全量。

- 交付目录：`Investigations/GenProtoEvidence/pen-delivery-0828c22f8f9a4cd4922c49d73afb887b/`。
- 原始 `genproto-full.trx` 已回读 Counters（第792行），RunID：`a2a273cb-9a8f-4027-afec-f0bcc7d9c6ce`。
- TRX SHA256：`8F989B6D2B4CC2F312CC65BF66ADB5B246BD25202035D5473AFD6E6972EB7E28`。
- `report.json` 保存全部测试源码哈希；五个 pen-consumer 子目录保存真实输入源码副本及哈希、编译 projection、实际 producer 输出、两架构测量引用/哈希和每例观测。测量引用来自既有 native-core 独立产物，本次不宣称重新执行原生编译或 Win32 runtime。
- 全量原有 EvidenceArchiveTests 仍按既有逻辑运行，读取先存 COM TRX/DLL并更新其自身Ledger快照；这不代表本执行者进行了COM运行/发布。新增Pen证据不依赖COM结果。

## 逐 required 边界与剩余

| required / 条款 | 本次增量 | 正式状态 |
|---|---|---|
| CORE-TYPES-LAYOUT 中 Pen 字段与尾部步长差异 | 真实托管 producer + 独立测量绑定 + 双记录反例，有限合同已证 | 完整 required 仍 Blocked；不是原生生产调用 |
| CORE-TYPES-CLOSURE / MILCMD-CLOSURE | 未把源码哈希当 physical record/mapping closure | 仍 Blocked |
| ProcessCreate 各命令字段/错误/引用/关闭 | 未执行，不从Pen外推 | Open，需真实生产消费者或另行明确隔离模型 |
| InitializePen 原生消费者 | 未执行 CPlainPen Set/SetDashArray、HRESULT/OOM与释放 | Open，生产public/COM验收由父级串行 |
| 全族 signatures/enums/physical records | 未改变分母、required列表或missing-history | Open，不能以5用例代替 |
| 历史CLR/输入绑定、ARM64真实布局执行 | 仓库inputs/manifest已存在；缺的是历史关联，不是所有inputs缺失 | 外部Open；没有伪造历史执行或ARM64 |

本次关闭的是可独立复核的producer字节/dash/stride有限模型子条款，不是完整required组。`completeRequiredEvidenceSatisfied=false`、`stage4Closed=false`在每份新报告中明确保留。下一自主生产差集仍是原生InitializePen真实消费及ProcessCreate错误/引用/关闭；其余代理独占生产/COM发布，本次没有抢占。
