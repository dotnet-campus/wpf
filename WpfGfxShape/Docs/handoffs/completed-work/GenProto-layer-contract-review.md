# GenProto分层合同核查

## 本轮实际结果

仅修改GenProto测试与文档；未修改生产共享文件、冻结生成代码或COM发布包。GenProto Release build成功；默认Debug全量25通过/0失败/0跳过，退出0。历史解析器nullable警告保留。没有新TRX/RunID，不虚构运行身份。

## 两差异根因

- `MIL_PEN_DATA`：冻结兼容输入`include/wgx_core_types_compat.cs`明确Sequential/Pack=1，三个double及五个4字节字段占44字节。`processed/wgx_core_types.h:1477–1488`同字段原生结构为48，差在尾部对齐，不是缺字段。`core/uce/geometry_api.cpp:129–148`的InitializePen逐字段读取，并以DashArraySize/sizeof(double)消费独立dash数组；没有48字节整体复制。现有证据支持字段合同，不证明数组stride或全部生产ABI调用安全；保留satisfied=false。
- `MilGlyphRun`：冻结`Resource.xml:483`明确MarshaledSize=2。原生枚举容器为4字节，但`processed/wgx_core_types.h:2414–2433`实际命令字段为WORD，紧接UINT16Padding0。生产`GeneratedGlyphRunResource.cs`的Flags为ushort/offset16，MemoryMarshal.Read消费后按0x10判断offsets，符合线格式；不是应把生产字段扩成4字节。当前仓库未找到字面名GeneratedPacketReader，实际消费类是GeneratedGlyphRunResource及GeneratedProtocol。原生头宽度与线格式宽度不能混同。

权威原生回读限于上述结构、枚举及初始化片段，未超过400行。没有权威依据要求生产修改，故没有提交生产修补路径。

## 新独立证据及边界

`LayerContractTests`保存七份实际物理来源SHA与完整冻结managed enum/member分母；独立字节验证WORD flags、非零padding隔离、无符号扩展，以及44字节笔字段范围/DashArraySize消费算术。报告输出在`Investigations/GenProtoEvidence/layer-contract-*/report.json`。这是独立字节及源码消费合同核查，不是执行完整生产消费者；productionExecutionVerified=false，physicalLedgerClosureSatisfied=false。

IndependentManagedCore修复三个类型别名：MILCoreSegFlags→MilCoreSeg、MIL_PRESENTATION_RESULTS→MilPresentationResults、MILWindowLayerType→MilWindowLayerType。尚未完成全部成员别名：RTInitialization存在历史兼容成员与当前原生不同集合；FORCE_WORD是托管宽度哨兵，不能强配FORCE_DWORD；MILCMD/ResourceType需独立命令/资源authority分母。未按数值猜映射，不删除未覆盖项，不宣称enum全闭包。

## 真实关闭条款及剩余自主模块

1. 分离managed声明、wire packet、native容器、生产消费四层Ledger subject；差异原值及来源hash保留。现有Ledger required不降低，本轮未完成Ledger分层关联。
2. enum全分母逐成员显式别名、兼容保留项与宽度哨兵分类；命令/资源身份关联实际authority，不用缺项自动过滤。
3. 独立编译真实手写布局并执行实际生产消费，覆盖非零padding、边界flags、笔dash长度及错误输入；源码存在和独立算术不能代替此验收。
4. physical records补齐来源→生成→consumer依赖及可回读产物/hash，之后再逐项更新Satisfied，不能直接整体Closed。
5. ARM64工具链与历史authority缺独立外部依赖，仍开放，本轮未反复探测。

存储已经恢复；COM最新full40失败包含媒体和硬件/cache/shader/呈现，非全是media。本轮GenProto通过只说明有限回归，布局required及C未Closed，总计划仍未完。
