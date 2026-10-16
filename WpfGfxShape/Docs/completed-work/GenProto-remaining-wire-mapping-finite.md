# GenProto remaining wire mapping 有限集成收口

## 本轮实际执行

只运行现有测试、读取源码和报告、更新Docs/Ledger；未修改生产或测试实现，未执行终端脚本，未publish或运行生产COM full。最新身份、数字及hash集中见[唯一基线](../handoffs/current-stable-baseline.md)。本轮不是用旧绿色结果替最新full。

首次full失败于WireCompilerArchiveTests按最后写入时间误选delivery目录，该目录只有wire-summary.json而没有summary.json。保留失败TRX，不补造summary、不改测试或过滤失败。随后运行现有WireCompilerTests及RemainingWireMappingTests生成本轮原始测量，再同轮完整full通过。最后只运行现有wire测量/归档测试，将已完成的最新full TRX复制、hash核验并写入交付报告。归档报告的targeted字段仍指历史单测；它不是本轮专项，不能冒充最新target。

## 有限Closed / Satisfied

- enum差集逐架构、enum、member绑定冻结声明、Resource.xml、EnumHelper及独立native compiler键；显式FillRule映射与sentinel保留依据，不靠名称猜alias。报告逐项含原值、nativeValue、compilerKey及authority。最新测试实际读取Win32/x64 PE导出并比较值，非仅扫描源码。
- RemainingEnums工程使用DBG=1。冻结声明中的diagnostic MilCmdValidateStructureOrder是debug命令，非retail；不能将全部冻结名称称为retail命令。该测量是debug声明配置下的编译器数值证据，不证明retail生产执行。
- padding逐字段保留managedNamedOffset=null，映射为ExplicitOffsetHoleOrTrailingSizeMarker；CodeGenHelpers/CommandStructure解释显式offset洞与尾部size marker。StructuralPaddingMappedNotSemanticField有限Satisfied；不把null改零、不要求padding字节相同、不证明raw transport。
- VisualCreate源码分类审阅Closed：搜索根src/Microsoft.DotNet.Wpf/src，扩展名.cs/.cpp/.h/.inl；命中清单、行号与hash在报告。CMilVisual::ProcessCreate明确RIP及E_UNEXPECTED，冻结native consumer不支持该legacy packet。Visual→VisualProxy→公开创建→channel/htmaster发送的是MILCMD_CHANNEL_CREATERESOURCE，不是MILCMD_VISUAL_CREATE。不把“没找到直接producer”夸成全世界不可达，也不新增producer。

## native-only：逐项裁决，不整体假Closed

| packet | 已证 | 未证 |
|---|---|---|
| BITMAP_SOURCE | apifunc typed producer；历史typed bitmap case按executionId绑定 | 无原生packet trace，不证明原版packet执行 |
| CHANNEL_CREATERESOURCE | htmaster/composition来源；同历史case绑定 | 不可替代VisualCreate；无packet trace |
| CHANNEL_DUPLICATEHANDLE | htmaster producer；同历史case绑定 | typed case通过不等于独立命令执行实测 |
| MEDIAPLAYER | XML显式UnmanagedOnly Update、发送/分派来源；历史系统媒体case绑定 | 非record packet player协议，不覆盖MilPlayer_Create/Process |
| CHANNEL_DELETERESOURCE | 公共Release producer来源 | 缺packet-specific runtime binding |
| TRANSPORT_DESTROYRESOURCESONCHANNEL | 内部channel teardown producer来源 | 缺packet-specific runtime binding |
| TRANSPORT_SYNCFLUSH | 公共Flush producer来源 | 缺packet-specific runtime binding |
| VISUAL_CREATE | native consumer明确不支持；未定位typed直接producer | 没有执行case，不能借ordinary create顶替 |

历史case只绑定基线中同版本候选AOT的原始TRX，不声称原版不存在的原物已运行；每行nativePacketExecutionProven=false。历史TRX、源码及测试assembly身份已读回，最新测试逐sources重新核hash；报告原始enum/padding/native-only数据由现有测试逐项生成及解析。wire归档测试完整读取JSON leaves并核source/assembly及full快照hash。这里不声称人工逐行阅读所有生成的enum重复行。

## 有限剩余清单

可自主继续的具体条款仅为：现存非VisualCreate producer的packet-specific执行/传输内容与失败关闭证据绑定；x86 CLR真实执行；既有MILCMD required/physical未闭条款。padding内容等价如被正式合同要求，须独立传输证据，不能以结构映射顶替。不得重复领取已闭enum差集，也不为不支持legacy packet创造新能力。

外部具体物按[唯一输入页](../all-completion-external-prerequisites.md)：历史CLR/hash及生成版本authority binding；ARM64编译器/SDK与真实runtime环境；完整合法WindowsDesktop runtime；官方record packet player协议、ownership及oracle。可观测HW会话仍缺成功前提，不把所有失败归环境。

独立子项Satisfied；两个MILCMD required及父级Blocked、stage4Closed=false。生产身份不变，历史COM/strict未重跑；DLL驻留无unload。七阶段、全required/physical、A/B/C与独立E2E仍未全部完成，总门Red。本轮源码回归及文档落地完成，无源码尾改或报告待读待办。
