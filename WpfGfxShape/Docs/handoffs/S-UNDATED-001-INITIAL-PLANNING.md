# 下一轮交接：创建独立构建边界与 NativeAOT probe 脚手架

> 历史 snapshot：本文件归档自首版 `next-session-handoff.md`  
> 下一工作包：`WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`  
> 当时状态：只完成调查和文档；尚未创建项目、构建、发布、运行测试或翻译任何生产实现

## 1. 当前总体状态

已完成：

- 完整复核 `WpfGfxShape/Docs` 既有文档；
- 复核当前 wpfgfx 项目、构建、生成、ABI 和托管调用方；
- 调查本地 Silk.NET；
- 强化迁移章程；
- 创建 NativeAOT 项目方案、Silk.NET 评估、测试策略、总计划和分轮路线图；
- 将批次 0 拆为独立机制/事实工作包。

当前没有任何生产 C# 迁移代码。所有“可行”结论仍需后续 build/publish/native caller/差分证据。

## 2. 下一轮唯一主目标

执行 `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`：创建完全独立的 .NET 10 Native AOT 项目边界、一个生产项目、一个测试项目，以及唯一非生产 ABI probe/native caller 的测试承载。

下一轮不得领取 `WP-00B`、批次 1 或任何真实 wpfgfx 文件翻译作为第二主目标。

## 3. 范围内

1. 创建 `WpfGfxShape/Directory.Build.props`，截断仓库根 props 自动发现。
2. 创建 `WpfGfxShape/Directory.Build.targets`，截断仓库根 targets 和 `eng/Testing.targets`。
3. 创建独立 `WpfGfxShape.slnx`（或经当前 IDE/SDK验证的独立 solution 形式），不修改原解决方案。
4. 创建 `WpfGfxShape/Code/WpfGfxShape.csproj`：
   - `Microsoft.NET.Sdk`；
   - `net10.0`；
   - `PublishAot=true`；
   - `NativeLib=Shared`；
   - `AssemblyName=wpfgfx_cor3`；
   - unsafe 与 AOT/trim/PInvoke analyzer；
   - 独立输出/中间目录。
5. 创建 `WpfGfxShape/Tests/WpfGfxShape.Tests.csproj`，不继承 WPF 根测试包注入。
6. 创建 `Code/common`、`Code/core`、`Code/shared` 空边界，不添加业务实现。
7. 创建 `Abi/NativeAotAbiProbe.cs`，仅导出 `WpfGfxShape_NativeAotAbiProbe_v1`。
8. 创建 native caller/test 的最小承载；具体 C/C++ 项目形态以最小独立、可验证为准。
9. 验证项目图和 MSBuild 导入隔离；若环境允许，完成最小 x64 Debug publish/call；若不允许，将其作为下一唯一动作写回，不扩展范围。
10. 更新本文和相关文档中的实际项目路径、属性和验证结果。

## 4. 明确范围外

- 不添加 106/107 个生产导出；
- 不实现 `MilVersionCheck`、`MILCreateFactory` 或任何真实 wpfgfx API；
- 不翻译任何 C/C++ 文件；
- 不引用本地 Silk.NET；
- 不创建 WIC/D3D/COM 生产绑定；
- 不修改 `Microsoft.Dotnet.Wpf.sln`、原 WPF 项目或源码；
- 不把原 WPF C/C++ 源码链接/复制进新项目；
- 不执行 ARM64/x86、linker/resource、COM、callback/unload 等后续 spike；
- 不做重构、优化、架构设计或无关清理。

## 5. 必读文档

按顺序：

1. `WpfGfxShape/Docs/00-migration-charter.md`
2. `WpfGfxShape/Docs/next-session-handoff.md`
3. `WpfGfxShape/Docs/04-nativeaot-project-shape.md`
4. `WpfGfxShape/Docs/investigations/04a-build-isolation.md`
5. `WpfGfxShape/Docs/investigations/04b-nativeaot-exports.md`
6. `WpfGfxShape/Docs/06-testing-strategy.md`
7. `WpfGfxShape/Docs/migration-master-plan.md`

## 6. 已锁定设计

- 单一生产项目 + 独立测试项目/native caller；
- 生产项目目标 .NET 10；
- candidate 基础名 `wpfgfx_cor3.dll`；
- 唯一 probe 名 `WpfGfxShape_NativeAotAbiProbe_v1`；
- probe 不是生产 ABI，不可部署替换 wpfgfx；
- 所有输出按 project/configuration/RID 隔离；
- 初始可明确沿用根 SDK `10.0.107`；
- 不引用 Silk.NET，留给 `WP-00G`。

## 7. 建议执行顺序

1. 检查 Git/工作区状态。
2. 创建局部 `Directory.Build.props/targets`。
3. 创建独立 solution 和生产/测试项目。
4. 配置独立输出路径。
5. 建立 `common/core/shared` 空目录。
6. 实现唯一 probe 与异常封锁。
7. 写托管内部测试。
8. 创建 native caller。
9. 验证 MSBuild 导入隔离。
10. 在范围内 build/publish/test。
11. 写证据 manifest。
12. 更新当前 handoff。

## 8. 完成条件

- 局部 props/targets 截断根导入；
- 独立 solution、生产项目和测试项目存在；
- 生产项目无原 WPF引用/源码链接；
- 输出隔离；
- 目录边界存在且无业务代码；
- probe 与 caller 承载存在；
- 未使用生产 ABI 名；
- 未加入 Silk.NET；
- 已运行/未运行验证准确记录；
- 未修改原 WPF；
- 当前 handoff 已更新。

## 9. 风险

- Windows x86 Native AOT shared library；
- Native AOT unload/reload；
- AOT-safe COM-like vtable/object；
- linker/resource/data export；
- Silk.NET D3D9 Cdecl/x86；
- 原 DLL二进制基线；
- 完整机器 Ledger。

## 10. 当时未执行

未创建项目、未 build/publish/test、未枚举原 DLL、未运行 generator、未修改 WPF产品代码。原因：该轮只负责探索和文档。

## 11. 最强提醒

只做 `WP-00A`。正确性和可验证性优先。不要翻译生产代码，不要添加生产导出，不要引用 Silk.NET，不要修改 WPF。未来真实翻译必须逐原生文件近似直译，禁止重构、优化、重命名、helper 合并和无关清理。
