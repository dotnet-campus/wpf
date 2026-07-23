# 下一轮交接：创建独立构建边界与 NativeAOT probe 脚手架

> 本文件是下一轮唯一权威入口  
> 下一工作包：`WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`  
> 当前状态：本轮只完成调查和文档；尚未创建项目、构建、发布、运行测试或翻译任何生产实现

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
5. `WpfGfxShape/Docs/investigations/04b-nativeaot-exports.md`，重点第 2、3、9、10 节
6. `WpfGfxShape/Docs/06-testing-strategy.md`，重点 T2、`E2E-00`
7. `WpfGfxShape/Docs/migration-master-plan.md`，批次 0

关键根配置：

- `global.json`
- `Directory.Build.props`
- `Directory.Build.targets`
- `NuGet.config`
- `eng/Testing.targets`

## 6. 已锁定设计

- 单一**生产**项目 + 独立测试项目/native caller；
- 生产项目目标 .NET 10；
- candidate 基础名 `wpfgfx_cor3.dll`；
- 唯一 probe 名 `WpfGfxShape_NativeAotAbiProbe_v1`；
- probe 不是生产 ABI，不可部署替换 wpfgfx；
- 所有输出按 project/configuration/RID 隔离；
- 初始可明确沿用根 SDK `10.0.107`，是否创建局部 `global.json` 由实际求值证据决定；
- 不提前创建局部 NuGet/CPM 文件，除非引入包时有实际需要；
- 不引用 Silk.NET，留给 `WP-00G`。

## 7. 建议执行顺序

1. 检查当前 Git/工作区状态，确认不会覆盖用户改动。
2. 创建局部 `Directory.Build.props/targets`，先证明导入边界。
3. 创建独立 solution 和生产/测试项目。
4. 配置独立 `obj/bin/publish/test-results` 路径。
5. 建立 `common/core/shared` 空目录语义。
6. 实现 probe 内部普通静态方法和薄 UCO 导出：
   - ABI version；
   - success checksum；
   - bad argument；
   - controlled managed exception → `E_FAIL`；
   - unknown operation → probe 协议的 `E_NOTIMPL`；
   - 任何失败前清零 output。
7. 为内部普通方法写托管单元测试；不要直接托管调用 UCO 方法。
8. 创建最小 native caller，按绝对路径加载并精确 `GetProcAddress`。
9. 使用 MSBuild 预处理/binlog/IDE 等证据验证不含 WPF Arcade/Testing 导入。
10. 在本轮允许范围内 build/publish/test；修复所有相关 warning，不压制 AOT/trim/interop warning。
11. 写证据 manifest 和本轮变更清单。
12. 更新本文，只留下一个下一主目标。

## 8. 完成条件

`WP-00A` 只有全部满足才可关闭：

- [ ] 局部 props/targets 已创建并截断根导入；
- [ ] 独立 solution、生产项目和测试项目存在；
- [ ] 生产项目没有原 WPF ProjectReference/源码链接；
- [ ] 输出与中间目录隔离；
- [ ] `common/core/shared` 边界存在且没有业务代码；
- [ ] probe 和 native caller/test 承载存在；
- [ ] probe 没有使用生产 ABI 名；
- [ ] 没有加入 Silk.NET 或其它未批准依赖；
- [ ] 已运行的 build/publish/test 结果准确记录；
- [ ] 未运行项及原因准确记录；
- [ ] 没有修改原 WPF/wpfgfx；
- [ ] `next-session-handoff.md` 已更新。

若隔离验证失败，停止在隔离修复；不得继续创建更多实现来掩盖问题。

## 9. 当前风险与阻塞

- Windows x86 Native AOT shared library：P0，未裁决；
- Native AOT unload/reload：P0，未裁决；
- AOT-safe COM-like vtable/object：P0，未裁决；
- `.def/.res/VERSIONINFO/data export`：后续 `WP-00D`；
- 本地 Silk.NET D3D9 COM Cdecl/x86 风险：后续 `WP-00G`；
- 原 DLL 实际二进制基线尚未建立：`WP-00H`；
- 完整机器 Ledger 尚未建立：`WP-00I`。

这些风险不阻止创建 WP-00A 脚手架，但阻止把它描述为 wpfgfx 替代品。

## 10. 本轮未执行

- 未创建项目；
- 未 build/publish；
- 未运行单元/native caller/E2E；
- 未枚举实际原 DLL；
- 未运行 generator；
- 未修改 WPF 产品代码。

原因：用户明确要求本轮只探索和输出文档，创建项目留到下一轮。

## 11. 本轮新增或修改的文件

修改：

- `WpfGfxShape/Docs/00-migration-charter.md`
- `WpfGfxShape/Docs/01-native-source-topology.md`
- `WpfGfxShape/Docs/03-migration-order.md`
- `WpfGfxShape/Docs/investigations/04a-build-isolation.md`

新增：

- `WpfGfxShape/Docs/04-nativeaot-project-shape.md`
- `WpfGfxShape/Docs/05-silknet-directx-assessment.md`
- `WpfGfxShape/Docs/06-testing-strategy.md`
- `WpfGfxShape/Docs/migration-master-plan.md`
- `WpfGfxShape/Docs/session-roadmap.md`
- `WpfGfxShape/Docs/next-session-handoff.md`
- `WpfGfxShape/Docs/README.md`

调查过程中已有专题：

- `WpfGfxShape/Docs/investigations/04b-nativeaot-exports.md`

## 12. 给下一位智能体的最强提醒

**只做 `WP-00A`。正确性和可验证性优先。不要翻译生产代码，不要添加生产导出，不要引用 Silk.NET，不要修改 WPF。项目结构也不得借机设计“大一统服务层”。**

未来开始真实翻译后，必须逐原生文件近似直译，保留控制流、错误顺序、锁、引用计数和释放顺序；禁止重构、优化、重命名、helper 合并和无关清理。

## 13. 下一轮结束时如何更新本文件

覆盖更新，而不是追加模糊日志：

1. 将当前工作包写为完成/未完成及逐条证据；
2. 列出实际创建/修改文件；
3. 列出项目图、build/publish/test 的实际结果；
4. 记录 analyzer warning 和处理；
5. 记录任何与设计不同的项目属性/路径及原因；
6. 更新 P0/P1 阻塞；
7. 指定**唯一一个**下一主工作包；
8. 给出下一轮第一动作和明确停止点；
9. 删除已经解决的临时提醒，保留有证据的决策。
