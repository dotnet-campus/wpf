# Ledger 批次视图

> 派生视图；JSONL 为唯一事实源。

| 批次 | 当前根范围 | 当前状态 |
|---|---|---|
| 0 | 构建、ABI、架构、二进制基线、机器 Ledger | `WP-00I` 为 `RepairRequiredPartial`；事实源仅 2 条完整记录 |
| 1 | 核心/AV 类型与布局 | 稳定 ID/组定义保留，根记录待恢复 |
| 2 | UtilLib、DebugLib、DllUtil | 稳定 ID 保留，项目哨兵与物理文件待恢复 |
| 3 | common/shared、DynamicCall | `DynamicCall` 哨兵与 `DelayCall.cpp` 有效；`common/shared` 已核查但待恢复 |
| 4 | geometry、scanop、effects | `scanop`、`effects` 已核查但待恢复；`WP-04A` 仍 NotReady |
| 5 | FXJIT | 稳定 ID 保留，项目哨兵待恢复，架构/AOT 风险保持阻塞 |
| 6 | core/common、control、targets、glyph | `core/api` 已核查但待恢复；其余根记录待恢复 |
| 7 | 生成类型/命令/资源协议 | 组定义保留，物理根记录待恢复 |
| 8 | resources、UCE | 组定义保留，项目/资源根记录待恢复 |
| 9 | software/meta | 根记录待恢复；bilinearspan 外部供应阻塞 |
| 10 | hardware | 项目/资源根记录待恢复；D3D9 风险未解除 |
| 11 | AV | 项目哨兵待恢复；媒体/COM 闭包未展开 |
| 12-14 | 上层集成、导出与替换 | 稳定合同 ID 保留；不得开始生产翻译 |
