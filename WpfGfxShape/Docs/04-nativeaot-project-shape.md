# 独立 .NET 10 Native AOT 项目方案

> 状态：实施前设计基线；所有 SDK、linker、产物和运行行为必须由后续工作包验证  
> 上位约束：`00-migration-charter.md`  
> 调查依据：`investigations/04a-build-isolation.md`、`investigations/04b-nativeaot-exports.md`  
> 下一实施工作包：`WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`

## 1. 目标形态

最终生产形态固定为：

- 一个独立的 `Microsoft.NET.Sdk` C# 生产项目；
- 目标框架 `net10.0`；
- Native AOT shared library；
- 候选产物基础名 `wpfgfx_cor3.dll`；
- 一个独立托管测试项目；
- 独立 native C/C++ caller/harness；
- 不修改、不引用、不递归构建原 WPF/wpfgfx 项目；
- 生产源码仍镜像 `common`、`core`、`shared` 等原目录边界。

“一个项目”只约束生产代码的构建和发布边界，不包含测试 caller/harness，也不允许扁平化或重构原职责。

## 2. 强制隔离边界

新项目位于 `WpfGfxShape` 下时会默认向上发现仓库根 `Directory.Build.props`、`Directory.Build.targets`、`global.json` 和 `NuGet.config`。根 props/targets 会导入 WPF Arcade SDK 和 `eng/Testing.targets`，不能只靠项目内属性可靠撤销。

下一轮必须先创建并验证：

- `WpfGfxShape/Directory.Build.props`：截断根 props 自动发现，只保留局部最小公共属性；
- `WpfGfxShape/Directory.Build.targets`：截断根 targets 自动发现，不导入 WPF Arcade/Testing；
- 独立 solution/solution filter：不修改 `Microsoft.Dotnet.Wpf.sln`；
- 独立 `artifacts` 输出根：不覆盖原 WPF artifacts。

初始可明确接受根 `global.json` 的 SDK `10.0.107`。是否创建局部 `global.json`，只能由首次 SDK 求值和可重复性证据决定；不要为了“看起来独立”预先复制根配置。

一旦引入 Silk.NET 或测试包，应建立局部、可审计的 NuGet 来源和版本策略；不得依赖 WPF 根 targets 隐式注入测试包。

## 3. 建议文件树

下一轮只创建完成 `WP-00A` 所需的最小子集，不一次铺满所有文件：

```text
WpfGfxShape/
  Directory.Build.props
  Directory.Build.targets
  WpfGfxShape.slnx
  Code/
    WpfGfxShape.csproj
    Abi/
      NativeAotAbiProbe.cs
    common/
    core/
    shared/
  Tests/
    WpfGfxShape.Tests.csproj
    NativeCaller/
  artifacts/
    obj/<project>/<configuration>/<rid>/
    bin/<project>/<configuration>/<rid>/
    publish/<configuration>/<rid>/
    test-results/<work-package>/<configuration>/<rid>/
```

`common`、`core`、`shared` 目录在 `WP-00A` 中只能作为边界占位；不得创建生产业务类型或复制原生文件。

## 4. 生产项目属性基线

以下是候选配置，不是已验证结果：

| 属性/设置 | 候选值 | 约束 |
|---|---|---|
| SDK | `Microsoft.NET.Sdk` | 不使用 WindowsDesktop/WPF SDK |
| TargetFramework | `net10.0` | 只有真实依赖要求时才评估 `-windows` |
| OutputType | 类库默认 | 不设为 `Exe` |
| PublishAot | `true` | 普通 build 不替代 publish |
| NativeLib | `Shared` | 实际 DLL/LIB/EXP/PDB 待验证 |
| SelfContained | `true` | 显式固定意图，仍需 SDK 求值验证 |
| AssemblyName | `wpfgfx_cor3` | 只是文件名候选控制，最终由产物/loader 验证 |
| AllowUnsafeBlocks | `true` | ABI、指针、vtable 和布局所需 |
| IsAotCompatible | `true` | 不代表 analyzer 已通过 |
| EnableAotAnalyzer | `true` | 不允许全局压制 |
| EnableTrimAnalyzer | `true` | 不允许用裁剪设置掩盖动态根问题 |
| EnablePInvokeAnalyzer | `true` | 历史 WPF 的关闭策略不得照搬 |

初始不得擅自加入：旧 `Ilc*` 属性、广泛 `DirectPInvoke`、symbol stripping、体积优化、关闭异常/stack trace/debug 支持或 AOT/trim warning suppression。

## 5. 唯一最小导出 probe

`WP-00A/WP-00B` 只允许一个非生产导出：

`WpfGfxShape_NativeAotAbiProbe_v1`

建议 ABI：

```c
HRESULT WINAPI WpfGfxShape_NativeAotAbiProbe_v1(
    uint32_t abiVersion,
    uint32_t operation,
    uint64_t input,
    uintptr_t context,
    uint64_t* output);
```

约束：

- `static`、非泛型、明确 `EntryPoint` 和候选 `CallConvStdcall`；
- C# 只使用明确 blittable 的 `uint`、`ulong`、`nuint` 和指针；
- 任何可恢复失败前先清零 `output`；
- 覆盖成功、参数错误、受控内部异常和未知 operation；
- 异常必须在最外层封锁；
- probe 名不在生产 106/107 清单中；
- 不创建任何生产导出 stub，不返回假成功。

## 6. 配置和架构矩阵

| Configuration | RID | 状态 | 要求 |
|---|---|---|---|
| Debug | `win-x64` | 首个机制验证 | publish、PE/export、动态/静态 caller、异常和并发 |
| Release | `win-x64` | 紧随 Debug | 裁剪、优化、符号和行为对照 |
| Debug/Release | `win-arm64` | 独立验证 | 必须在真实 ARM64 Windows 运行 |
| Debug/Release | `win-x86` | `Blocked` | 先取得官方/目标 SDK 支持证据，再验证 stdcall、未装饰名和栈平衡 |

每次 publish 只设置一个 RID，且使用独立输出目录。一个架构或配置通过不能替代其它格。

## 7. 批次 0 工作包

| ID | 目标 | 主要产物/证据 |
|---|---|---|
| `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD` | 创建隔离边界、生产/测试项目、probe 源码/内部测试和 caller 骨架 | 项目图、MSBuild 导入证据、普通 build/test、目录/属性、无生产逻辑；不含 Native AOT publish/export/native call |
| `WP-00B-NATIVEAOT-ABI-PROBE-X64` | x64 Debug/Release shared-library 机制 | publish 产物、PE/export、native caller、异常/并发 |
| `WP-00C-ARCHITECTURE-AND-X86-DECISION` | ARM64 验证和 x86 裁决 | ARM64 真实运行；x86 支持或阻塞证据 |
| `WP-00D-LINKER-RESOURCE-VERSION-SPIKE` | `.def`、`.res`、VERSIONINFO、ETW、shader、data export | DLL/LIB/EXP/PDB 与资源差分 |
| `WP-00E-COM-VTABLE-SPIKE` | AOT 对外 native COM-like 对象 | IUnknown vtable、QI/AddRef/Release、GC root |
| `WP-00F-CALLBACK-LIFECYCLE-UNLOAD-SPIKE` | 双运行时 callback、线程和卸载 | token 不透明性、detach、join、exit/unload 证据 |
| `WP-00G-SILKNET-ADOPTION-SPIKE` | 决定 Silk.NET 复用形态 | 调用约定/AOT/布局结果与采用裁决 |
| `WP-00H-ORIGINAL-BINARY-BASELINE` | 冻结原 DLL 二进制事实 | export/import/resource/version/symbol/hash |
| `WP-00I-MACHINE-READABLE-LEDGER` | 建立完整迁移分母和稳定 ID | 多视图合并的 machine-readable Ledger |

不得把这些包合并为一次无法审计的“大项目初始化”。

## 8. P0 决策门

以下任一项未有证据时，不得承诺完整替换：

1. Windows x86 Native AOT shared library 与 stdcall/未装饰导出可行性；
2. Native AOT shared library 的官方 unload/reload 语义能否满足原显式 detach；
3. AOT-safe COM-like vtable/object 是否能满足 factory/media/bitmap 等返回对象。

若平台明确不支持，必须保留 `Blocked` 并请求用户决定支持范围或批准局部例外，不能自行删除 Win32、修改调用方或泄漏模块引用。

## 9. WP-00A 完成条件

- 局部 props/targets 已创建，并有证据证明未导入 WPF Arcade/Testing 目标；
- 一个生产项目和一个测试项目存在，生产项目没有引用原 WPF 项目；
- 独立 solution/入口存在，不修改原解决方案；
- 输出和中间目录按 project/configuration/RID 隔离；
- `common/core/shared` 边界存在但没有业务实现；
- probe 和 native caller 的源/测试承载存在；
- restore、普通 build、托管内部测试和项目图/导入隔离验证已在环境允许范围内执行并准确记录；
- Native AOT publish、PE/export、动态/静态 native call 明确记录为 `NotRun-ByWorkPackageScope`，由下一唯一工作包 `WP-00B-NATIVEAOT-ABI-PROBE-X64` 执行；
- 未添加任何生产导出、Silk.NET 依赖或 wpfgfx 文件翻译；
- `next-session-handoff.md` 已更新。

## 10. 维护规则

- SDK、Native AOT 或 linker 行为发生变化时，先更新 investigation，再更新本文件的稳定结论。
- 项目属性存在不等于产物已满足；所有结论必须链接到 build/publish/PE/native caller 证据。
- 本文件定义项目形态，任务顺序以 `migration-master-plan.md` 和 `03-migration-order.md` 为准。
