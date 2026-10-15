# Ledger 状态视图

## 2026-10-13 实际复验警示

证据 `../../Docs/handoffs/completed-work/S-20261013-SAMPLER-PROTOCOL-BASELINE-REVALIDATION.md`：SHA BB8B9B9B504C8903275C4C98D4F28119359201B79F2A339D3CFF96457B967934，RunID c4f48c60-b6a8-4100-b5b1-a736be341dd7；普通4197/4197，COM1275/25/1300，0跳过/abort。24项glyph失败未解释、旧FLOAT57/450独立，公开WhenPublic*无失败。不晋升稳定或Closed；机器ledger原有RepairRequiredPartial不因文档同步变为修复。

> 派生视图；`records.jsonl`、`groups.jsonl`、`work-packages.jsonl`、`evidence.jsonl` 为事实源。

## 当前验收门禁警示

归档 `../../Docs/handoffs/completed-work/S-20261012-246-PRESENT-OCCLUSION-NATIVE-CONTROL.md`：SHA CEA705677144815991C54627503B3ACDDABDC63A8AA854615698025F9A1DF9B0，RunID958c41c3-6935-4538-bf0d-168606d435c2，report wpf-calibration-5b470d80dd8c488eb79bb3d4def85de1.json。COM1286/9/1295；LockScreenBackstopFrame实测覆盖导致公开8例未通过，原生fixture Ex对照也Occluded。不是稳定或Closed；不改变下述机器ledger缺失事实。

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
