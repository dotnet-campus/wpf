# DEC-0001：以最终 wpfgfx DLL 生产闭包定义迁移分母

> 状态：Accepted  
> 日期：当前规划轮次（具体日历日期未可靠取得）

## 背景

用户目标是“整个 wpfgfx 层”迁移到一个 C#/.NET 10 Native AOT 生产项目。`src/Microsoft.DotNet.Wpf/src/WpfGfx` 磁盘树还包含未进入最终 `wpfgfx` DLL 的 `milctrl`、`wpfx` 和 `DbgXHelper` 旁支；若不明确，完成定义会歧义。

## 已确认事实

- `core/dll/wpfgfx.vcxproj` 是最终 DynamicLibrary。
- 它直接聚合 23 个已加载 WpfGfx 静态库、`bilinearspan` 供应槽和一个逻辑 `OSVersionHelper`。
- `core/common` 直接编译树外 `src/Microsoft.DotNet.Wpf/src/Shared/cpp/dwriteloader.cpp`。
- `milctrl.vcxproj` 与 `exts.vcxproj` 是独立 DLL；`DbgXHelper` 只服务于 `wpfx`，均未被 `wpfgfx.vcxproj` 引用。

## 决策

“整个 wpfgfx”的生产完成分母定义为：最终 `wpfgfx$(WpfVersionSuffix).dll` 的完整生产闭包，包括实现、声明、PCH、生成、资源、构建、ABI、外部供应和必要的托管协议对端证据。

- `milctrl/wpfx/DbgXHelper` 不要求迁入同一 Native AOT DLL。
- 它们保留为旁支兼容证据，尤其用于共享 `control/util` 行为和 generated command 布局。
- 任何实际进入最终 DLL闭包的新项必须加入生产分母。
- 从生产分母移除任何当前闭包项需要用户明确批准。

## 被拒绝替代方案

1. 把整个 `WpfGfx` 目录下所有项目都迁入一个 DLL：会错误合并独立控制/调试产品边界。
2. 只迁移 461 个直接 `ClCompile`：会漏掉头、PCH、生成协议、资源、构建和外部供应。
3. 只迁移 106 个导出可达代码：无法证明内部流程、生命周期和外部 native caller 兼容。

## 影响

- 具体 schema 见 `07-scope-and-ledger-schema.md`。
- `WP-00I` 必须计算该完整分母。
- 旁支共享消费关系仍进入 Ledger，但 `scopeDisposition=SidecarEvidence`。

## 重新评估触发条件

- 构建求值/链接证据证明旁支实际进入 `wpfgfx`；
- 用户明确要求同时迁移 `milctrl` 或 `wpfx`；
- 新发现外部供应或树外源码进入最终 DLL。