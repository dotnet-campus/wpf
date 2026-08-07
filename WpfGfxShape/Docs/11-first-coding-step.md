# 第一步编码工作：建立独立构建隔离边界

> 工作包：`WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`  
> 本文范围：定义开始编码后的**第一项具体工作**，不是整个 `WP-00A` 的实施清单  
> 状态：局部边界文件和项目脚手架已创建；实际 MSBuild 隔离验证未完成  
> 上位约束：[`00-migration-charter.md`](00-migration-charter.md)、[`next-session-handoff.md`](next-session-handoff.md)

## 1. 第一项具体工作

先创建并验证 `WpfGfxShape` 局部 MSBuild 隔离边界：

- `WpfGfxShape/Directory.Build.props`
- `WpfGfxShape/Directory.Build.targets`

这两个文件必须截断 MSBuild 对仓库根同名文件的自动发现，使后续生产项目和测试项目不继承 WPF Arcade SDK、`eng/Testing.targets`、根测试包注入和原仓库输出规则。

原执行门要求在隔离边界完成并确认前不创建生产项目、测试项目、solution 或 ABI probe。当前 solution 和两个项目已由用户创建，因此后续不得重复创建；应先补做实际隔离验证，再继续增加 probe。`Tests/NativeCaller` 已由后续决策明确否决。

## 2. 开始前检查

1. 检查工作区现有改动，确保不覆盖用户文件。
2. 读取并记录仓库根：
   - `Directory.Build.props`
   - `Directory.Build.targets`
   - `global.json`
   - `NuGet.config`
   - `eng/Testing.targets`
3. 确认局部 `WpfGfxShape/Directory.Build.props` 和 `WpfGfxShape/Directory.Build.targets` 尚不存在。
4. 若文件已经存在，先读取并判断它们是用户改动、已有实现还是未完成工作，不得直接覆盖。

## 3. `Directory.Build.props` 工作内容

创建最小局部 props，要求：

- 文件自身作为最近的 `Directory.Build.props`，阻止继续向上自动发现仓库根 props；
- 不手动导入仓库根 `Directory.Build.props`；
- 不导入 WPF Arcade SDK 或原 WPF 构建文件；
- 只放生产项目与测试项目都需要的最小公共属性；
- 为 `obj`、`bin`、`publish` 和测试结果预留统一的局部 `artifacts` 根；
- 输出路径必须能够按项目、配置和 RID 隔离；
- 不在此处加入 Native AOT linker 优化、warning suppression、Silk.NET 或测试框架包版本；
- 不创建局部 `global.json`、`NuGet.config` 或中央包管理文件，除非后续实际求值/还原证据证明必要。

候选输出语义：

```text
WpfGfxShape/artifacts/
  obj/<project>/<configuration>/<rid>/
  bin/<project>/<configuration>/<rid>/
  publish/<configuration>/<rid>/
  test-results/<work-package>/<configuration>/<rid>/
```

具体 MSBuild 属性写法应以当前 SDK 实际求值结果为准，不能仅凭路径看起来正确就判定完成。

## 4. `Directory.Build.targets` 工作内容

创建最小局部 targets，要求：

- 文件自身作为最近的 `Directory.Build.targets`，阻止继续向上自动发现仓库根 targets；
- 不手动导入仓库根 `Directory.Build.targets`；
- 不导入 `eng/Testing.targets`；
- 不注入 WPF Arcade targets、原 WPF 测试参数或包引用；
- 初始保持为空或只包含后续隔离验证确实需要的最小目标；
- 不加入 build/publish 后处理、PE 检查、资源注入或发布后 ABI 集成测试执行逻辑。

## 5. 验证要求

完成两个文件后，先验证隔离，再进入 `WP-00A` 的下一项工作。

必须获得并记录以下证据：

1. 位于 `WpfGfxShape/Code/WpfGfxShape` 和 `WpfGfxShape/Tests/WpfGfxShape.Tests` 的现有项目会命中局部 props/targets。
2. 求值后的导入链不包含仓库根 WPF Arcade props/targets。
3. 求值后的导入链不包含 `eng/Testing.targets`。
4. 输出路径不会写入原 WPF `artifacts` 目录。
5. 没有修改仓库根构建文件、原解决方案或任何 WPF/wpfgfx 源码。

证据状态必须使用：

- `StaticConfirmed`：仅由文件内容和路径规则确认；
- `ExecutedPassed`：实际 MSBuild 求值或构建验证通过；
- `ExecutedFailed`：已执行但失败，并记录错误与工件；
- `NotRun`：环境或当前工具限制导致未执行，并写明原因。

静态检查不能冒充实际 MSBuild 求值结果。

## 6. 完成条件

第一项具体工作只有满足以下条件才可结束：

- [x] 两个局部边界文件均已创建；
- [x] 两个文件均未显式导入仓库根同名文件；
- [x] 文件内容未显式导入 WPF Arcade SDK 和 `eng/Testing.targets`；
- [x] 局部 `artifacts` 输出隔离规则已确定；
- [ ] 已记录实际 MSBuild 求值证据及其准确状态；
- [x] 未修改原 WPF/wpfgfx 文件；
- [x] 未创建任何生产实现或生产 ABI 导出。

## 7. 失败停止条件

出现以下任一情况时停止，不继续创建 solution 或项目：

- 局部文件仍无法截断仓库根导入；
- 输出仍进入原 WPF artifacts；
- 测试项目仍被 `eng/Testing.targets` 注入；
- 必须修改仓库根构建文件才能继续；
- 当前 SDK/MSBuild 的实际行为与文档假设冲突。

失败时应把 `WP-00A` 保持为 `Blocked` 或进行中，记录冲突、实际导入链、错误和下一唯一修复动作，不得用更多项目属性掩盖根因。

## 8. 明确范围外

本步骤不做：

- 创建 `WpfGfxShape.slnx`；
- 创建生产或测试 `.csproj`；
- 创建 `NativeAotAbiProbe.cs`；
- 创建发布后托管 P/Invoke ABI 集成测试宿主；
- restore、Native AOT publish、PE/export 或发布后 P/Invoke 调用；
- 添加任何生产导出或翻译 wpfgfx 源码；
- 引用 Silk.NET；
- 修改原 WPF 项目、解决方案、源码或根构建配置。

## 9. 本步骤完成后的下一动作

隔离边界文件、独立 `WpfGfxShape.slnx`、`Code/WpfGfxShape/WpfGfxShape.csproj` 和 `Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj` 现已创建。继续 `WP-00A` 时应先核对并完善这些现有项目，不得按旧路径重复创建项目；托管测试使用 MSTest，仍不得进入 `WP-00B` 或翻译任何生产代码。
