# Ledger 项目视图

> 派生自 `records.jsonl` / `groups.jsonl`。当前状态：`RepairRequiredPartial`；通过 schema 校验的 JSONL 才是唯一事实源。

## 生产项目闭包

- 生产项目哨兵：24。
- 最终动态库项目：`REC-WPFGFX-PROJECT-CORE-DLL-WPFGFX-VCXPROJ`。
- 其余生产静态库项目：23。
- 当前直接 `ClCompile` 静态下限：461；`DelayCall.cpp` 已作为首条样板展开，当前 1/461。
- 本轮静态核查 `core/api/api.vcxproj`：Debug/Release × Win32/x64/arm64、`StaticLibrary`、`precomp.hpp`、`precomp.cpp` 加 12 个其它直接 `ClCompile`，无项目引用、资源或自定义生成项。
- 逐行复核发现此前“7 条完整记录”结论不成立：第 5-7 行仍有对象粘连。覆写与尾部删除也会复用损坏片段，故立即缩小事实源并停止继续批量写入。
- 当前 `records.jsonl` 仅保留 2 条逐行完整记录：`DynamicCall` 项目哨兵与 `DelayCall.cpp`。`effects`、`scanop`、`common/shared`、`api`、`milctrl`、`exts` 均已有静态核查材料，但当前不声称存在于事实源。
- 完整分母状态：不可计算；不得把 `groups.jsonl` 中的 24 个稳定 ID、已核查文档或 461 聚合下限冒充当前机器事实源已完整恢复。

## 当前有效根记录

| 类别 | 数量 | 状态 |
|---|---:|---|
| 生产项目哨兵 | 1 | 仅 `DynamicCall` 存在于有效事实源；`api` 等已核查哨兵待安全恢复 |
| 生产实现文件 | 1 | `DelayCall.cpp`，直接 `ClCompile` 为 1/461 |
| 旁支项目哨兵 | 0 | `milctrl`、`exts`、`DbgXHelper` 稳定 ID/核查材料保留，记录待恢复 |
| ABI/资源/生成/供应/树外依赖/原二进制根 | 0 | 稳定 ID 与静态证据仍保留，尚未恢复进有效 `records.jsonl` |

## 旁支边界

磁盘存在但不计入生产分母的 `milctrl.vcxproj`、`exts.vcxproj` 已有静态核查材料，`DbgXHelper.vcxproj` 的稳定 ID 也已保留；三者记录当前均待安全恢复，且均不计入 24 个生产项目分母。
