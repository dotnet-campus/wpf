# WP-00I 机器可读 Ledger 启动记录

> 状态：`RepairRequiredPartial`  
> 工作包：`WP-00I-MACHINE-READABLE-LEDGER`  
> 本轮边界：建立机器事实源与根记录；不翻译生产代码，不把 461 个 `ClCompile` 聚合下限冒充完整分母

## 1. 已创建工件

按 `07-scope-and-ledger-schema.md` 创建：

```text
WpfGfxShape/Ledger/
  schema-version.json
  records.jsonl
  groups.jsonl
  work-packages.jsonl
  evidence.jsonl
  views/
    projects.md
    batches.md
    blocked.md
    status.md
```

Schema 固定为 `1.0.0`。JSONL 是唯一事实源，Markdown 仅为派生视图。

## 2. 本轮已冻结的根记录

- 24 个生产项目哨兵：最终 `wpfgfx` 动态库 + 23 个静态库；
- 最终 `wpfgfx.vcxproj` 的 Debug/Release × Win32/x64/arm64 条件；
- WpfGfx 外直接项目 `OSVersionHelper`；
- WpfGfx 外直接编译实现 `src/Shared/cpp/dwriteloader.cpp`；
- 外部供应槽 `bilinearspan`；
- 最终 ABI 输入 `core/dll/wpfgfx.def`；
- 两个最终资源根 `core/dll/milcore.rc`、`core/hw/hw.rc`；
- MilCodeGen `Resource.xml`、`Resource.xsd`、`mcg.proj`；
- `wgx_core_types.w`、`wgx_av_types.w`；
- 原 Release win-x64 二进制 `ORIGINAL-WPFGFX-NET9.0.5-RELEASE-WIN-X64`，保持 `NotInspected`/`NotTested` 边界。

同时建立项目闭包、生成协议、生成类型、`.def -> .i`、资源、ABI 和既有循环组。

## 3. 完整性边界

当前工具可以读取项目 XML 和做文本搜索，但原生项目项枚举接口不可用；当前环境还禁止命令、脚本和进程执行。因此本轮没有伪造以下结果：

- 461 个直接 `ClCompile` 的逐项机器记录；
- 头文件、PCH/include、磁盘文件、构建导入、生成输出与消费者的完整多视图闭包；
- JSON Schema 或自定义校验器的实际运行结果；
- ABI entries、managed declarations 与真实 PE 的机器 manifest；
- MilCodeGen 隔离重生成差分；
- 原二进制 PE/export/import/resource/PDB 深度检查。

这些缺口分别记录为 `BLOCKER-LEDGER-461-EXPANSION`、`BLOCKER-LEDGER-MULTIVIEW-CLOSURE`、`BLOCKER-LEDGER-MACHINE-VALIDATION` 等，不以缺行表示已检查。

## 4. 人工一致性复核

本轮静态复核：

- 24 个生产项目均有稳定 Record ID；
- 生产/旁支边界未混淆；
- 根记录均含批次、状态、目标映射或明确 blocker、依赖与 required evidence；
- 所有本轮引用的 required evidence ID 均在 `evidence.jsonl` 以 `Planned`、`Deferred` 或真实已执行状态存在；
- 原二进制记录没有把静态字符串或 `.def` 升级为 PE 事实；
- 没有记录被提升到 `Scaffolded` 或更高生产实现状态。

由于当前环境不能运行验证器，`EVID-WP00I-MANUAL-JSONL-CONSISTENCY` 为 `PassedWithDeferredMachineValidation`，不是 schema validation pass。

## 5. 后续发现与当前修复边界

后续复核确认旧 `records.jsonl` 存在对象中途截断与多个 JSON object 粘连，原“24 个生产项目均有有效机器记录”的人工结论撤回。当前状态降为 `RepairRequiredPartial`，新增 `BLOCKER-LEDGER-RECORDS-RESTORATION`。

后续逐行复核又确认先前“5 条”及“7 条”完整记录结论均不成立：对象粘连残留在后续行中，覆写和尾部删除也会复用损坏片段。当前已止损为 2 条逐行完整记录：`DynamicCall` 项目哨兵与 `DelayCall.cpp` 首条物理记录；生产项目哨兵事实源为 1/24，直接 `ClCompile` 为 1/461。

本轮已完成 `core/api/api.vcxproj` 静态核查：Debug/Release × Win32/x64/arm64、`StaticLibrary`、`precomp.hpp`、`precomp.cpp` 与 12 个其它直接 `ClCompile`，无项目引用、资源或自定义生成项。该哨兵及此前核查的 `effects`、`scanop`、`common/shared`、`milctrl`、`exts` 继续作为待安全恢复材料，不冒充当前事实源。

在写入通道不能稳定证明对象边界前，继续扫描和静态核查后续项目，将候选事实写入权威文档；仅在逐行回读证明完整时增加 JSONL 计数。机器校验运行继续作为非阻塞延期。
