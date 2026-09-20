# wpfgfx 原生源码与项目拓扑基线

> 本文保存迁移范围所需的稳定拓扑事实，不记录调查过程、当前工作切片或完成比例。
> 生产分母与 Ledger schema 见 [`07-scope-and-ledger-schema.md`](07-scope-and-ledger-schema.md)，当前实现映射见 [`native-to-managed-file-map.md`](native-to-managed-file-map.md)。

## 1. 原生生产边界

目标原生树为：

`src/Microsoft.DotNet.Wpf/src/WpfGfx/`

当前冻结的解决方案级生产下限包括：

- 1 个最终 DynamicLibrary：`core/dll/wpfgfx.vcxproj`；
- 23 个由最终 DLL 聚合的 WpfGfx StaticLibrary 项目；
- 直接项目项下限为 461 个 `ClCompile`；
- 约 22 个 PCH 创建单元和约 439 个其它直接编译单元；
- 2 个最终 `ResourceCompile` 根：`core/dll/milcore.rc` 与 `core/hw/hw.rc`。

这些数字是项目项下限，不是完整迁移分母。完整分母还包括头文件、inline/template、PCH/include、生成物、资源、构建导入、ABI、外部供应和托管协议对端。

## 2. 主要源码域

原生职责至少分布在：

- `common/DynamicCall`：动态调用和 loader 基础；
- `common/effects`：effect 基础；
- `common/scanop`：扫描线操作；
- `common/shared`：公共图形基础；
- `core/api`：factory、bitmap、stream、WIC/外部对象桥接；
- `core/av`：媒体和视频路径；
- `core/common`：显示、loader、通用图形基础；
- `core/fxjit/*`：Platform、Compiler、Collector、PixelShader；
- `core/geometry`：几何算法；
- `core/glyph`：glyph 公共职责；
- `core/hw`：D3D9/HW backend；
- `core/meta`：后端选择和组合；
- `core/resources`：资源模型与命令消费；
- `core/sw/swlib`：软件光栅与像素路径；
- `core/targets`：render-target 基础；
- `core/uce`：connection/channel/composition/partition；
- `core/control/util`：共享控制工具；
- `shared/debug/DebugLib`、`shared/util/DllUtil`、`shared/util/UtilLib`。

生产 C# 项目可以统一构建，但不得消除这些原生职责边界和可追溯关系。

## 3. 最终 DLL 聚合

`core/dll/wpfgfx.vcxproj` 负责：

- 聚合静态库；
- 生成/消费 `wpfgfx.def`；
- 编译 `milcore.rc`；
- 合并 HW shader 资源；
- 应用 DLL 入口、版本和最终链接设置；
- 形成 `wpfgfx$(WpfVersionSuffix).dll`，当前调用名为 `wpfgfx_cor3.dll`。

迁移不能只覆盖某个静态库；最终 DLL 的导出、资源、初始化和进程退出行为都属于生产闭包。

## 4. 直接外部与供应依赖

完整生产闭包还包括或依赖：

- `src/Microsoft.DotNet.Wpf/src/Shared/cpp/dwriteloader.cpp`；
- `OSVersionHelper` 中被生产路径消费的行为；
- `bilinearspan` 的真实源码或运输供应物；
- Windows SDK、D3D9、D3DCompiler、WIC、DWrite、DWM、GDI、COM 和媒体 API；
- processed/generated 类型和命令；
- shader binary、WPP、`.rc/.res`、版本和 ETW 输入。

相邻部署组件如 DirectWriteForwarder、PresentationNative 和 D3DCompiler redist 不翻译进同一生产项目，但必须进入加载和 E2E 环境事实。

## 5. 生成与资源闭包

必须分别追踪：

- MilCodeGen 的模型、schema、generator、冻结输出和消费者；
- `wgx_core_types`、`wgx_av_types` 等 processed 类型；
- generated command/resource/router/factory/marshal/render-data；
- AV WPP `.tmh`；
- HW shader 源、编译输出、资源 ID 和 `hw.rc`；
- `.def` 预处理输出；
- `milcore.rc`、VERSIONINFO、ETW 和其它 PE 资源。

不得把签入的生成输出当作无来源手写文件，也不得运行生成器后直接覆盖冻结基线而不做差分。

## 6. 不属于最终 wpfgfx 生产分母的旁支

以下项目或组件需要记录关系，但不迁入同一候选 DLL：

- `core/control/dll/milctrl.vcxproj`；
- `exts/exts.vcxproj` 和 `DbgXHelper`；
- 原 WPF 测试项目；
- 新建的托管测试、ABI Host、差分和 E2E harness。

共享头、命令或工具若被生产闭包消费，仍需作为合同或依赖建账。

## 7. 当前机器事实状态

机器 Ledger 已存在，但当前为 `RepairRequiredPartial`：

- 有效逐项记录尚未恢复到完整生产分母；
- 461 个直接 `ClCompile` 仍只是聚合下限；
- 项目项、磁盘、PCH/include、构建导入、生成、资源、ABI 和供应多视图尚未闭合；
- 因此不能从文件数计算整体完成百分比。

当前数量和阻塞以 `Ledger/*.jsonl` 与 `Ledger/views/status.md` 为准。

## 8. 迁移映射规则

原生实现原则上映射到：

`WpfGfxShape/Code/WpfGfxShape/<relative-domain>/<name>.cs`

要求：

- 保留原路径、类型、方法和调用关系可追溯；
- 一个原生实现文件原则上对应一个主要 C# 文件；
- 头文件可按类型族承载，但必须有映射记录；
- 生成输出与手写实现分开；
- 外部直接文件保留真实来源，不冒充 WpfGfx 自有文件；
- 不通过新架构消除原生循环或模块边界。

## 9. 当前事实来源

- 当前文件映射：[`native-to-managed-file-map.md`](native-to-managed-file-map.md)；
- 当前阶段与剩余缺口：[`remaining-gap-closure-plan.md`](remaining-gap-closure-plan.md)；
- Ledger schema：[`07-scope-and-ledger-schema.md`](07-scope-and-ledger-schema.md)；
- 初始调查细节：`investigations/03b-file-ledger-and-batches.md` 等调查文档。
