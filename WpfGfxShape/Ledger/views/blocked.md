# Ledger 阻塞视图

> 派生视图；未知项必须保持显式，不以缺行表示已检查。

| Blocker ID | 优先级 | 影响 | 解除条件 |
|---|---|---|---|
| `BLOCKER-LEDGER-RECORDS-RESTORATION` | P0 | 旧事实源损坏已确认；覆写、追加和尾部删除均观察到损坏片段复用。现止损为 2 条完整记录，生产项目哨兵事实源为 1/24；`api` 等项目已静态核查但待安全写入 | 继续静态核查并在文档保留候选记录；仅在写入结果可逐行证明完整时提升事实源计数，机器验证保持 P1 且不阻塞静态推进 |
| `BLOCKER-LEDGER-461-EXPANSION` | P0 | 461 个直接 `ClCompile` 仍是聚合下限；当前已展开 1 条 | 逐项目展开剩余 460 条稳定物理记录并校验总数/条件 |
| `BLOCKER-LEDGER-MULTIVIEW-CLOSURE` | P0 | 完整生产分母不可计算 | 合并项目项、磁盘、PCH/include、构建导入、生成、资源、ABI、外部供应视图 |
| `BLOCKER-LEDGER-MACHINE-VALIDATION` | P1 | JSONL 尚无可执行 schema 校验结果 | 在允许执行验证器的环境运行唯一 ID、引用、枚举、路径和状态规则 |
| `BLOCKER-ABI-MACHINE-MANIFEST` | P0 | ABI 不能逐 entry 回链 Ledger | 建立 `Abi/*.jsonl` 并复核 106/107/99/8/7 |
| `BLOCKER-GENERATOR-REPRODUCIBILITY` | P1 | MilCodeGen 生成链不可重现 | 隔离运行并与冻结输出差分 |
| `BLOCKER-GENERATED-TYPE-CLOSURE` | P0 | 核心/AV 布局分母不完整 | 全部输入、输出、消费者建账并验证布局 |
| `BLOCKER-RESOURCE-BINARY-ENUMERATION` | P1 | 最终 PE 资源事实未知 | 对原/候选二进制枚举资源类型、名称、语言、大小和 hash |
| `BLOCKER-RESOURCE-SHADER-CLOSURE` | P1 | `hw.rc` shader 输入闭包未知 | 逐资源输入/条件/ID 建账并做候选差分 |
| `BLOCKER-EXTERNAL-BILINEARSPAN-SUPPLY` | P0 | 软件渲染闭包存在外部供应缺口 | 取得并冻结真实架构供应物或经批准的来源裁决 |
| `ExternalBaselineGap-LowPriority` | 低 | Debug/x86/ARM64 原 DLL 与深度 PE/PDB 证据缺失 | 获得真实工件后增量补齐；不阻塞静态 Ledger 推进 |

当前环境禁止命令和脚本，机器验证已延期并降为 P1；这不阻塞继续静态展开记录。
