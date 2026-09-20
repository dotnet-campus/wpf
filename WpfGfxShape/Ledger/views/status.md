# Ledger 状态视图

> 派生视图；`records.jsonl`、`groups.jsonl`、`work-packages.jsonl`、`evidence.jsonl` 为事实源。

## 当前快照

- Schema：`1.0.0`。
- Ledger：`RepairRequiredPartial`。
- `records.jsonl`：当前仅有 2 条逐行完整记录：`DynamicCall` 项目哨兵与 `DelayCall.cpp`。本轮发现既有第 5-7 行仍粘连，且受限接口在覆写和删除时也会复用损坏片段；已止损缩小事实源。
- 生产项目哨兵：当前事实源仅恢复 `DynamicCall`，进度 1/24。本轮已静态核查 `core/api/api.vcxproj`，此前核查的 `effects`、`scanop`、`common/shared` 和旁支项目材料仍保留，但均不得误报为机器记录已恢复。
- 直接 `ClCompile`：已建立首条 `DelayCall.cpp` 物理记录，当前为 1/461 聚合下限。
- 最终资源根、最终 `.def`、生成根、外部供应和原二进制根：稳定 ID 与静态证据仍保留，但当前有效 `records.jsonl` 中均为 0；不得误报已恢复。
- 生产完成分母：尚不可确定性计算。
- 机器校验：延期；仅完成静态人工一致性复核。

## 状态边界

本轮未创建任何生产实现、生产导出或假成功 stub。没有记录达到 `Scaffolded`、`Translated`、`UnitVerified`、`DifferentialVerified`、`Integrated`、`EndToEndVerified` 或 `Accepted`。
