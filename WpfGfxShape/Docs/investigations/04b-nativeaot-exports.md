# .NET 10 Native AOT 共享库与 C 导出调查

> 状态：静态调查完成；仅形成静态证据与待验证设计，不代表已构建、已发布或已运行  
> 目标：评估独立 SDK-style C# 项目以 .NET 10 Native AOT 生成 Windows native shared library，并以薄边界导出与现有 `wpfgfx` 调用方兼容的 C ABI  
> 写入边界：本轮仅维护本文；不修改任何 WPF 源码、项目配置或 `WpfGfxShape/Code`  
> 工具边界：绝对禁止 PowerShell、cmd、bash、Python、dotnet、msbuild、脚本、构建命令及任何间接命令执行；仅使用 IDE 文件/代码搜索、文件读取、符号导航和文档写入

## 0. 结论状态与证据标签

本文强制使用以下标签，避免把平台常识或设计建议误写成仓库已验证事实：

| 标签 | 含义 |
|---|---|
| **事实—仓库静态证据** | 当前工作区文件可直接证明；仍不等于实际产物或运行行为 |
| **已知语义—待 SDK 验证** | 基于 .NET/Native AOT 已知公开语义形成，但当前工作区不能证明，且本轮未构建 |
| **推断** | 由多项证据推导出的工程判断；必须给出依据和重新评估条件 |
| **风险** | 可能阻塞 ABI 等价、发布或运行的事项 |
| **未决** | 尚无足够证据裁决 |
| **受禁令限制未验证** | 原本需要构建、发布、二进制检查或运行测试，但本轮明确禁止执行 |

当前最重要的预警：**Windows x86 Native AOT 支持状态不能从工作区静态证据确认。本文不得猜测其已受支持；在取得目标 .NET 10 SDK 与官方支持矩阵证据前，按“高风险外部事实待 SDK/官方文档验证”处理。** 若 x86 不能发布 Native AOT shared library，则可能直接阻塞与原 `wpfgfx` Win32 产物完全等价。

### 0.1 当前技术结论摘要

| 项目 | 当前结论 | 等级 |
|---|---|---|
| 推荐项目形态 | 独立 `Microsoft.NET.Sdk`、`net10.0` 类库，`PublishAot=true`、`NativeLib=Shared`、`SelfContained=true`、`AssemblyName=wpfgfx_cor3`、`AllowUnsafeBlocks=true`，逐 RID publish，并开启 AOT/trim/PInvoke 分析 | **设计建议，全部待真实 SDK验证** |
| C 导出机制 | 每个唯一 native 名称最多一个 `static [UnmanagedCallersOnly(EntryPoint=..., CallConvs=...)]` 薄入口；只接受明确 unmanaged ABI 类型；禁止托管直调与异常越界 | **已知语义—待 .NET 10 产物验证** |
| x64 | 可作为首个最小机制 spike 候选 | **候选可行，未构建/未调用** |
| ARM64 | 可作为第二个最小机制 spike 候选；不能由 x64 结果代替 | **候选可行，未构建/未调用** |
| Windows x86 | 工作区没有 Native AOT 支持证据；不能从普通 `win-x86` RID 推导；可能无法产出 shared library 或满足 stdcall | **P0/阻塞级高风险外部事实待 SDK/官方文档验证** |
| 106/107 ABI | 以 106 `.def`、107 托管唯一 EntryPoint、99 交集和 8/7 差异并列建账；不创建假生产 stub | **仓库静态事实 + 强制设计约束** |
| COM/callback | 必须以 native pointer/vtable/function pointer 跨边界；CoreCLR 与 AOT 的对象、GCHandle、delegate、exception 不共享 | **高风险，需原生 caller/真实回调验证** |
| 显式卸载 | 原 DLL有完整 `FreeLibrary` detach；Native AOT shared library 的安全 unload/reload 不能假定 | **P0/阻塞级高风险外部事实待验证** |
| Win32 资源/版本 | managed embedded resource 不等于 `.rsrc`；必须保留宿主 EXE `IMAGE` 与 DLL自身 RCDATA/ETW/VERSIONINFO 的不同作用域 | **高风险 linker/产物门禁** |
| FXJIT | 原路径生成并执行 RWX x86/x64 风格机器码；W^X、CFG、instruction cache、ARM64 与 mitigation 均未闭环 | **生产迁移高风险阻塞；不阻塞最小 ABI spike** |

## 1. 范围与证据

### 1.1 调查范围

- SDK-style C# Native AOT Windows 共享库的项目属性和发布维度。
- `[UnmanagedCallersOnly]` C 导出约束、调用约定、异常边界与函数指针。
- `wpfgfx_cor3.dll` 身份、106 个 `.def` 静态名称、107 个托管唯一 EntryPoint、99 个交集与 8/7 双向差异如何映射到 AOT 薄导出。
- x86、x64、ARM64 的架构和 ABI 风险。
- COM、SafeHandle、P/Invoke、reverse P/Invoke、线程、静态初始化和卸载生命周期。
- 资源、版本信息、模块定义文件/额外导出、符号和调试。
- FXJIT 可执行代码生成与 W^X、Native AOT、CFG 的阻塞关系。
- 一个不迁移生产逻辑的最小 ABI spike 设计。
- 发布矩阵、产物检查、加载/导出/调用/异常/卸载门禁。

### 1.2 明确范围外

- 不创建或修改 Native AOT 项目。
- 不实现任何导出或迁移任何 wpfgfx 生产逻辑。
- 不修改 PresentationCore、WpfGfx、构建配置、生成输入或 `WpfGfxShape/Code`。
- 不构建、不发布、不运行测试、不枚举实际 PE 导出、不加载 DLL。
- 不为 FXJIT 设计替代架构或重构方案；只记录阻塞和验证需求。

### 1.3 已先读取的强制基线

- `WpfGfxShape/Docs/00-migration-charter.md`
- `WpfGfxShape/Docs/02-abi-and-managed-callers.md`
- `WpfGfxShape/Docs/03-migration-order.md`

### 1.4 初始仓库基线

**事实—仓库静态证据：**

- 根 `global.json` 请求 .NET SDK `10.0.107`，允许 prerelease，并以 `latestFeature` roll-forward；这只证明仓库 SDK 选择意图，不证明当前机器已安装或该 SDK 的 Native AOT 工具链可用。
- 根 `Directory.Build.props` 将默认 `TargetFramework` 设为 `net10.0`，并显式补充旧 MSBuild 对 `net10.0` 的识别；未来独立项目仍须按构建隔离专题避免误继承 WPF Arcade 逻辑。
- 当前 PresentationCore 的 MilCore DLL 常量静态展开为 `wpfgfx_cor3.dll`。
- `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.def` 有 106 个静态名称条目；这不是实际 DLL 导出数的运行证明。
- 当前托管基线有 109 个声明出现、107 个唯一 native EntryPoint；与 `.def` 交集为 99，托管侧独有 8 个，`.def` 侧独有 7 个。
- 原生工程静态支持 Win32、x64、ARM64；x86 原生默认调用约定为 stdcall，x64/ARM64 使用平台统一 ABI。
- Debug 源码存在 `.def` 外 `g_fNoMeterChecks` 数据导出候选，但是否进入真实二进制、最终名称及属性均未验证。
- IDE 全工作区静态搜索未发现可直接复用的生产 `PublishAot`、`NativeLib=Shared`、`IsAotCompatible`、`PublishTrimmed` 或 `DirectPInvoke` 配置。`Silk.NET/src/Lab/Experiments/CoreRTTest/CoreRTTest.csproj` 是旧 CoreRT/ILCompiler 实验，目标 `net5.0`，不能作为 .NET 10 Native AOT shared library 模板。
- 仓库存在普通 SDK-style 项目对 `AssemblyName`、`AllowUnsafeBlocks`、`RuntimeIdentifier` 和 self-contained 测试部署的用法；这些只证明属性在普通 .NET/WPF 构建中的使用，不证明 Native AOT shared library 支持。
- WPF 构建和文档使用 `win-x86`、`win-x64`、`win-arm64` RID 来选择现有运行时包和原生工件。尤其 `win-x86` 的存在只证明 WPF 普通运行时/原生包有该架构，**不能推导 Native AOT 编译器支持 Windows x86**。

### 1.5 已读取的关键证据文件

| 主题 | 关键路径 | 已吸收事实 |
|---|---|---|
| SDK/TFM | `global.json`；`Directory.Build.props` | SDK `10.0.107` 选择意图、`net10.0` 默认值；未证明本机 toolchain |
| 原 DLL项目 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.vcxproj`；`eng/WpfArcadeSdk/tools/VersionSuffix.props` | DynamicLibrary、Win32/x64/ARM64、`wpfgfx_cor3`、自定义入口、资源与 `.def` 输入 |
| 导出清单 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.def`；`eng/WpfArcadeSdk/tools/GenerateModuleDefinitionFile.targets` | 106 个静态名称、未装饰文本、`.def -> wpfgfx.i` 预处理 |
| 调用约定/符号 | `eng/WpfArcadeSdk/tools/Wpf.Cpp.props`；`eng/WpfArcadeSdk/tools/Wpf.Cpp.targets` | x86 默认 stdcall、Release CFG、PDB、VERSIONINFO 生成 |
| 当前 DLL身份 | `src/Microsoft.DotNet.Wpf/src/Shared/RefAssemblyAttrs.cs` | 当前 PresentationCore 名称为 `wpfgfx_cor3.dll` |
| 托管 ABI 基线 | `WpfGfxShape/Docs/02-abi-and-managed-callers.md`；`investigations/02a-native-export-surface.md`；`investigations/02b-managed-callers.md` | 109/107/106/99 与 8/7 差异、签名/布局/所有权/调用方 |
| SafeHandle/COM | `PresentationCore/.../SafeMILHandle.cs`；`safemediahandle.cs`；`SafeReversePInvokeHandle.cs` | release、shutdown→release、阻塞 callback 后 release |
| callback | `PresentationCore/.../StreamAsIStream.cs`；`EventProxy.cs`；`D3DImage.cs`；`WpfGfx/core/av/eventproxy.cpp`；`core/uce/geometry_api.cpp`；`core/hw/InteropDeviceBitmap.*` | descriptor、CoreCLR GCHandle、stdcall、线程、异常、detach |
| 初始化/卸载/线程 | `WpfGfx/shared/util/DllUtil/dllmainimpl.cxx`；`core/dll/dllentry.cpp`；`core/common/milcoredllentry.cpp`；`core/av/StateThread.cpp` | attach、显式 detach、进程终止短路、COM apartment、thread join |
| 动态加载 | `PresentationCore/ModuleInitializer.cs`；`MS/Internal/Text/TextInterface/DWriteLoader.cs` | System32 load、精确 GetExport、先清空指针再 Free |
| 资源/版本 | `core/dll/milcore.rc`；`Shared/Tracing/resources/wpf-etw.rc`；`core/hw/hw.rc`；`ShaderAssemblies.targets`；`EmbeddedShaders.RC`；`core/api/exports.cpp`；`core/resources/Effect.cpp` | 宿主 EXE `IMAGE`、DLL RCDATA、ETW、shader ID/字节、VERSIONINFO |
| FXJIT | `core/fxjit/Platform/Platform.cpp`；`Compiler/program.cpp`；`PixelShader/pshader.cpp`；`Public/SIMDJit.h`；`core/sw/swlib/brushspan.cpp` | RWX 分配、机器码写入/执行、x86/AMD64 倾向、代码寿命 |

以上路径均为仓库根目录相对路径的缩写；带省略号的 PresentationCore 路径在正文对应章节给出具体文件名。未读取任何构建产物、SDK安装目录或外部官方页面。

## 2. SDK-style 项目属性

> 下述形态是后续 spike 的推荐起点，不是本轮构建结果。凡涉及 Native AOT SDK 行为均标为“已知语义—待 SDK 验证”。

### 2.1 必要或候选属性清单

| 属性 | 推荐值/策略 | 必要性与理由 | 证据等级与待验证点 |
|---|---|---|---|
| `Sdk` | `Microsoft.NET.Sdk` | 应保持普通 SDK-style 类库边界；不应使用 `Microsoft.NET.Sdk.WindowsDesktop` 或启用 WPF，因为目标是独立 native shared library，不是 WPF 托管应用 | **建议**；需以隔离项目实际求值验证没有被仓库根 SDK/targets 改写 |
| `TargetFramework` | `net10.0` | 项目章程要求 .NET 10；仓库根也以 `net10.0` 为当前 TFM。仅因产物运行在 Windows，不必自动改为 `net10.0-windows`；只有真实 API/包要求时再书面决定 | **事实—仓库静态证据 + 建议**；SDK `10.0.107` 是否可用未验证 |
| `OutputType` | 保持类库默认值 `Library`，不要设为 `Exe` | shared library 项目不是可执行主程序；`NativeLib=Shared` 应负责改变 native 发布形态 | **已知语义—待 SDK 验证** |
| `PublishAot` | `true` | 进入 Native AOT publish 管线；普通 `Build` 不能替代 RID-specific AOT publish | **已知语义—待 SDK 验证**；仓库无现成生产用法 |
| `NativeLib` | `Shared` | 请求动态/共享 native library，而不是 Native AOT executable 或 static library | **已知语义—待 SDK 验证**；必须检查 Windows 实际 DLL、LIB、EXP、PDB 组合 |
| `SelfContained` | 建议显式 `true` | Native AOT 发布按已知语义是自包含的；显式写出可固定意图，避免把普通 framework-dependent/RID 行为混入设计 | **已知语义—待 SDK 验证**；确认目标 SDK 是否强制、推断或覆盖该值 |
| `RuntimeIdentifier` | 每次发布只设一个已验证 RID；首选候选为 `win-x64`、`win-arm64` | Native AOT 产物是架构特定 PE，必须逐 RID 发布和验证 | **已知语义—待 SDK 验证**；`win-x86` 目前为阻塞项，不能先写入支持矩阵后再补证据 |
| `RuntimeIdentifiers` | 可仅作为还原/矩阵声明候选，不把它当作一次生成多架构 DLL 的机制 | 复数属性不替代每次 publish 的单一 `RuntimeIdentifier`，也不证明列出的 RID 受 Native AOT 支持 | **已知语义—待 SDK 验证**；初始项目可不设，交由独立 publish 入口逐 RID 传入 |
| `AssemblyName` | `wpfgfx_cor3` | 当前 PresentationCore 静态加载名为 `wpfgfx_cor3.dll`；程序集基础名是控制候选 native 文件名的最直接项目属性 | **推断—高**；必须由 publish 产物和真实 loader 测试确认精确 DLL 名，不能只看 MSBuild 属性 |
| `AllowUnsafeBlocks` | `true` | 目标 ABI 大量使用裸指针、指针宽度句柄、固定缓冲、函数指针和显式布局；即使简单导出可不用 unsafe，本项目整体必须允许 | **事实—仓库 ABI + 建议**；不代表允许跳过布局/生命周期验证 |
| `IsAotCompatible` | 建议 `true` | 让项目在普通开发构建阶段也声明 AOT 兼容意图，并启用/联动相关兼容分析 | **已知语义—待 SDK 验证**；确认 .NET 10 SDK 的具体隐含属性和诊断集合 |
| `EnableAotAnalyzer` | 建议显式 `true`，至少在 spike 和 CI 中不得关闭 | 提前发现动态代码、反射和 AOT 不兼容调用；不能复制 WPF 主项目关闭部分互操作 analyzer 的做法 | **已知语义—待 SDK 验证**；与 `PublishAot`/`IsAotCompatible` 的默认联动需检查 |
| `EnableTrimAnalyzer` | 建议显式 `true`，至少在 spike 和 CI 中不得关闭 | Native AOT 依赖静态可达性；反射、回调保活和动态入口若无法分析会成为真实风险 | **已知语义—待 SDK 验证**；与 `IsAotCompatible` 的联动需检查 |
| `PublishTrimmed` | 不把它当作 `PublishAot` 的替代；若显式写则只能为 `true` | Native AOT 已知发布模型包含裁剪/静态可达性；设置为 `false` 不应作为消除 warning 的手段 | **已知语义—待 SDK 验证**；确认 shared library 模式下默认值和可配置范围 |
| `TrimMode` | 初始不覆盖 SDK 默认值 | 未有真实 warning 和可达性证据前，不能用自定义 trim 模式掩盖导出、反射、COM 或回调根问题 | **建议**；后续只有经产物/运行证据才能改变 |
| `EnablePInvokeAnalyzer` | 建议 `true` | 仓库部分大型 WPF 项目显式关闭它，但该历史选择不能复制到新的 ABI 骨架；新项目需要尽早暴露互操作签名问题 | **事实—仓库存在开/关用法 + 建议**；具体诊断需 SDK 验证 |
| warning 策略 | AOT/trim/interop warning 不得全局压制；先逐项登记，稳定后再决定哪些提升为错误 | 一个 warning 可能直接表示导出不可达、反射元数据缺失、动态代码不支持或 marshalling 不兼容 | **建议**；不得用 `NoWarn` 达成“绿色构建” |

### 2.2 推荐项目属性

后续 `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD` 与 `WP-00B-NATIVEAOT-ABI-PROBE-X64` 的最小推荐集如下；这是设计输入，不是可复制后即宣称成功的已验证项目：

| 分类 | 属性 |
|---|---|
| **项目形态必需** | `Sdk=Microsoft.NET.Sdk`、`TargetFramework=net10.0`、类库输出、`PublishAot=true`、`NativeLib=Shared` |
| **本项目 ABI 必需** | `AssemblyName=wpfgfx_cor3`、`AllowUnsafeBlocks=true` |
| **发布维度必需** | 每次只设置一个经支持矩阵确认的 `RuntimeIdentifier`；x64 和 ARM64 分开发布，x86 在裁决前保持阻塞 |
| **建议显式固定** | `SelfContained=true`、`IsAotCompatible=true`、`EnableAotAnalyzer=true`、`EnableTrimAnalyzer=true`、`EnablePInvokeAnalyzer=true` |
| **初始不应擅自设置** | `TrimMode`、旧 `Ilc*` 实验属性、关闭 stack trace/exception/debug 支持的体积优化、全局 AOT/trim warning suppression、`DirectPInvoke` 白名单、symbol stripping |

`RuntimeIdentifiers` 不是必须属性。若后续为了 restore/IDE 矩阵列出多个 RID，也必须继续用独立 publish 逐个产生和验证单架构 DLL；绝不能把复数 RID 声明解释成一个“通用” `wpfgfx_cor3.dll`。

### 2.3 裁剪、AOT 分析与可达性规则

**已知语义—待 SDK 验证：**

- Native AOT 在发布时静态编译可达代码，并对反射、动态代码生成和不可静态分析路径施加限制；shared library 并不会因为“所有代码都在同一程序集”而自动保留全部方法。
- 带 `EntryPoint` 的 `[UnmanagedCallersOnly]` 方法按 Native AOT 导出模型应成为外部入口和可达根，但必须用真实导出表和调用测试确认；不能以源码属性存在代替产物证据。
- 导出薄层应直接、静态地调用内部实现，避免字符串反射、`DynamicMethod`、运行时生成程序集、未声明的泛型实例化或只靠副作用触发的类型。
- callback target、COM 实现、反射构造类型和资源访问若不能由静态分析自然发现，必须按其各自支持机制显式保留；具体标注/descriptor 方案应由实际 warning 驱动，不在调查阶段猜写 suppressions。
- AOT/trim warning 是兼容性证据，不是噪声。任何 suppression 都必须指向具体调用、解释为何安全，并有 native caller/回调/资源运行测试覆盖。

**事实—仓库静态证据：**

- `src/Microsoft.DotNet.Wpf/src/PresentationBuildTasks/PresentationBuildTasks.csproj` 显式开启 `EnablePInvokeAnalyzer`，而 PresentationCore 等历史项目显式关闭它，说明仓库并无可直接套用的统一 analyzer 策略。
- 旧 Silk.NET CoreRT 实验包含 `IlcOptimizationPreference`、`IlcGenerateCompleteTypeMetadata` 等旧属性和独立 ILCompiler 包。其目标框架、工具链和输出形态均不符合本任务，禁止复制为 .NET 10 配置。

### 2.4 不得混淆的构建概念

- **普通 `Build`：**主要用于编译和 analyzer 反馈；即使成功，也不证明 Native AOT 编译器、native linker、导出或 DLL 可加载。
- **RID-specific `Publish`：**才是产生目标架构 Native AOT shared library 的候选动作；x64、ARM64、可能的 x86 必须分别执行、分别留证。
- **`SelfContained`：**描述运行时组成方式，不等于 `NativeLib=Shared`，也不等于 ABI 兼容。
- **`RuntimeIdentifier`：**选择目标运行时/架构，不等于 C# `PlatformTarget`，更不等于现有 WPF native 工程的 `Win32/x64/ARM64` 配置已经被 AOT 支持。
- **`AssemblyName`：**是产物基础名意图，不等于 Windows loader 最终看到的文件、导入库或 PE export name 已正确。
- **DLL/LIB/EXP/PDB：**是否全部产生、名称为何、哪个供 native caller 链接、调试信息是否分离，均属于目标 SDK/linker 的实际产物验证。
- **裁剪成功：**只表示编译器找到一种静态闭包，不证明未被调用的 106/107 ABI 入口、动态资源、COM 或 callback 行为完整。

## 3. `[UnmanagedCallersOnly]` 与 C 导出规则

### 3.1 方法与签名约束

**已知语义—待 .NET 10 SDK/产物验证：**

- 方法必须是 `static`；不能依赖实例 `this` 作为隐含 ABI 参数。
- 方法不能是泛型方法，也不能声明在泛型类型中。导出表必须落到封闭、稳定、非泛型的静态入口。
- 参数和返回值必须是运行时可直接穿越 unmanaged 边界的 blittable 形状；不能依赖普通 P/Invoke 的自动 marshalling。
- C# `unmanaged` 类型约束与互操作 blittable 并非完全同义。尤其 `bool`、`char`、字符串、数组、delegate、class、object、SafeHandle 和带引用字段的 struct 不能因为“C# 可表示”就直接放入导出签名。
- 对应原生 `BOOL` 应使用明确 32 位整数；对应 C++ `bool` 必须先由原二进制探针确认 1 字节 ABI，再选明确的 8 位表示。禁止把两者都写成 C# `bool` 猜测。
- 字符串应按原 ABI 使用 `char*`/`WCHAR*`/BSTR 指针及明确所有权；数组使用指针加长度；COM、Win32 handle、MIL client handle 和资源 ID 使用各自明确的指针或固定宽度整数。
- by-value struct 只有在字段全部 blittable 且 pack、align、字段 offset、返回/参数 lowering 都逐架构验证后才可进入导出签名；高风险结构优先以与原 ABI 一致的指针传递。
- 带 `EntryPoint` 的方法是 Native AOT native library 的导出候选。`EntryPoint` 值应逐字符等于目标公开名，包括大小写；不能依赖 C# 方法名、overload、命名空间或自动名称转换。
- 同一模块不得为两个方法声明同一个 `EntryPoint`。109 个托管声明中的重载不产生额外 native export；`MILAddRef` 和 `MILQueryInterface` 各只需要一个真实 ABI 入口。
- 标记 `[UnmanagedCallersOnly]` 的方法不能由普通托管代码直接调用。共享逻辑应放在普通内部静态实现中，由薄导出调用；托管单测可测内部实现，但 ABI 验证必须从 native caller 进入真实导出。
- 导出方法可以调用已 AOT 编译的普通托管代码，但这不会放宽裁剪、反射、动态代码、线程、GC 或异常约束。

推荐的逻辑分层只有两层，不构成架构重构：

1. **ABI 薄入口：**精确 `EntryPoint`、明确 `CallConvs`、原生宽度参数、必要校验、异常封锁和错误映射；
2. **逐原文件等价实现：**保留原控制流、所有权、锁、错误和释放顺序。

导出层不得承载新业务算法，也不得把多个原生入口合并成一个托管“通用入口”。

### 3.2 异常封锁

**已知语义—待 SDK/运行验证：** unmanaged caller 不能安全接收托管异常；异常越过 `[UnmanagedCallersOnly]` 或 reverse P/Invoke 边界不属于可支持的 C ABI，可能导致进程终止或其它不可接受结果。因此每个入口和 callback 都必须在最外层封锁异常。

强制规则：

1. 先按原实现要求初始化 out 参数；例如 QI 失败前清零输出，不能因异常路径留下未初始化地址。
2. 对已知 HRESULT 失败保持原 HRESULT，不先抛异常再丢失原错误。
3. 对可预期托管异常建立逐入口映射表；可使用何种映射机制必须以原错误语义和 Native AOT 运行验证为准，不能一律机械调用同一个 helper。
4. HRESULT 入口在未实现时不得返回 `S_OK`；BOOL/标量入口不得返回看似有效的成功值；指针入口不得返回伪对象。
5. `void` 导出或 `void` callback 没有天然错误通道。若原协议没有其它方式表达失败，则异常封锁策略必须先有原生差分证据；不能简单吞异常后继续。
6. 不能把 `OutOfMemoryException`、栈溢出、进程终止或运行时灾难假定为都可可靠映射。边界代码应最小化分配，并把不可恢复异常视为独立可靠性风险。
7. callback 内同样封锁：HRESULT callback 返回原语义错误；`void` callback 的策略单独审批；任何异常都不得回穿 native 锁、COM vtable 或线程入口。

“捕获所有普通异常并返回某个失败 HRESULT”只适合 spike 验证异常不会越界；生产入口仍需逐项对照原错误码、out 参数和副作用顺序。

### 3.3 调用约定与名称

**事实—仓库静态证据：**

- `eng/WpfArcadeSdk/tools/Wpf.Cpp.props` 对非托管 x86 设置 `CallingConvention=StdCall`；原注释明确说明 WPF x86 native C++ 历史上默认使用 stdcall。
- `WINAPI`、`CALLBACK` 和 COM `STDMETHODCALLTYPE` 在 x86 上表达 stdcall 语义；多数 UCE/geometry 入口显式使用 `WINAPI`，stream/event descriptor callback 显式为 `__stdcall`。
- `wpfgfx.def` 的 106 个条目均为未装饰公开文本名。`wpfgfx.vcxproj` 甚至对 x86 自定义 DLL 入口显式追加 `@12`，证明内部 stdcall 修饰与公开导出名必须区别处理。
- 当前托管 MilCore 声明没有显式 `CallingConvention`，依赖 `DllImport` 的 Winapi 默认值；也未显式 `ExactSpelling`/`CharSet`。这些历史元数据是待验证契约，不能为“整洁”而统一改写。

**已知语义—待 .NET 10 SDK/二进制验证：**

- 对原 Win32 ABI，导出应显式声明 `CallConvs` 含 `System.Runtime.CompilerServices.CallConvStdcall`，callback/function pointer 也使用匹配的 `delegate* unmanaged[Stdcall]<...>` 或等价明确标注。
- x64 与 ARM64 上 cdecl/stdcall 不再形成传统 x86 栈清理差异，平台使用统一 ABI；但参数分类、结构传递、返回值、寄存器、栈对齐和 unwind 信息仍必须各架构验证。
- `EntryPoint="MilVersionCheck"` 的设计意图是产生同名、同大小写的 PE export；是否确实未装饰、没有前导下划线/`@N`、没有额外 thunk 名，必须检查真实 DLL。
- `GetProcAddress` 按精确文本名查找且名称区分大小写；所有 ABI 清单比较也必须保持原拼写。
- callback 指针类型和 `[UnmanagedCallersOnly(CallConvs=...)]` 必须一致。把 stdcall callback 当作 cdecl 在 x86 上调用会造成栈破坏；x64/ARM64 上“看似能运行”不能为 x86 提供证据。
- 不应加入 `CallConvSuppressGCTransition` 作为优化。wpfgfx 入口可能分配、阻塞、获取锁、调用 COM、触发 GC 或长时间渲染；任何 GC transition 改变都必须另立性能/正确性验证，当前不允许。

函数指针有两类边界，必须区分：

1. **AOT 内部静态 callback 导出给 native consumer：**可由 `[UnmanagedCallersOnly]` 静态方法取得稳定 unmanaged function pointer；无需以 delegate 对象保活该方法本身，但 context token/GCHandle 和 owner 仍必须保活。
2. **现有 CoreCLR PresentationCore delegate 传给 AOT DLL：**AOT 侧只接收一个 native function pointer 和上下文值；delegate thunk 的保活仍由原 CoreCLR 调用方负责，AOT 不能把它当成本方托管引用或跨运行时对象。

## 4. 架构矩阵与 Windows x86 风险

### 4.1 静态架构基线

| 架构 | 原 wpfgfx 工程静态事实 | AOT 薄层必须保持 | Native AOT 状态 | 当前结论等级 |
|---|---|---|---|---|
| Win32/x86 | `wpfgfx.vcxproj` 有 Debug/Release Win32；native 默认 stdcall | stdcall、callee 栈清理、参数字节数、指针 32 位、`size_t` 32 位、未装饰公开名、callback stdcall | 工作区没有 Native AOT 支持证据；普通 `win-x86` WPF RID 不是 AOT 证据 | **高风险外部事实待 SDK/官方文档验证；可能为全等价阻塞** |
| x64 | `wpfgfx.vcxproj` 有 Debug/Release x64 | Windows x64 统一 ABI、指针 64 位、by-value struct/64 位参数、精确导出名 | 可作为首个 spike 候选，但本轮未发布 | **候选可行，实际 SDK/产物/调用全部未验证** |
| ARM64 | `wpfgfx.vcxproj` 有 Debug/Release arm64 | Windows ARM64 ABI、指针 64 位、结构/浮点参数、函数指针、精确导出名 | 可作为第二个 spike 候选，但本轮未发布 | **候选可行，实际 SDK/产物/调用全部未验证** |

`wpfgfx.vcxproj` 为 `DynamicLibrary` 且 `CLRSupport=false`，目标名为 `wpfgfx$(WpfVersionSuffix)`；`VersionSuffix.props` 将后缀设为 `_cor3`。这些是原 native 目标事实，不证明 AOT 项目用 `AssemblyName` 后会产生完全同形产物。

### 4.2 x86 阻塞含义

**风险判级：高。** 工作区无法证明 .NET 10 Native AOT 的 Windows x86 code generator、runtime pack、shared-library 模式和 stdcall export 均受支持。基于 Native AOT 已知平台支持历史，Windows x86 长期不是可以默认假设存在的目标；但该句属于**高风险外部事实待目标 SDK/官方支持矩阵验证**，本文不把“当前一定不支持”伪装成仓库事实，也绝不猜测“已经支持”。

若目标 .NET 10 Native AOT 不支持 Windows x86 shared library，或无法产生与现有 stdcall/未装饰导出兼容的 DLL，则：

- 无法声称完整替换原 Win32 `wpfgfx_cor3.dll`；
- 106/107 ABI 清单即使在 x64/ARM64 通过，也不能代表 Win32 等价；
- `RuntimeIdentifiers` 中不能凭愿望加入并宣称 `win-x86` 可发布；
- 必须在主计划中把“全架构等价”标记为阻塞，等待用户裁决支持范围或批准例外。

后续验证顺序应是：

1. 读取目标 .NET 10 官方 Native AOT 支持矩阵和 native-library 文档；
2. 用目标 SDK 探测 `win-x86` restore/publish，而不是从普通 WPF runtime pack 推断；
3. 若能产出，再检查 PE machine、stdcall 调用、未装饰名称、栈平衡和 callback；
4. 任一步失败即把 Win32 等价维持为 `Blocked`，不得通过改调用方为 x64 或删除 Win32 目标自行解锁。

**阶段裁决：** x64/ARM64 的最小 ABI spike 可以先行，用于验证 shared-library 机制；但它只能证明对应架构的技术可行性。大规模生产迁移和“完全替换”承诺前，必须取得 x86 支持/不支持的正式裁决及其对产品范围的用户决定。

## 5. ABI 清单到 AOT 薄导出的映射

### 5.1 DLL 身份

**事实—仓库静态证据：**

- `src/Microsoft.DotNet.Wpf/src/Shared/RefAssemblyAttrs.cs` 定义 `WCP_VERSION_SUFFIX="_cor3"`，`DllImport.MilCore=$"wpfgfx{...}.dll"`，当前展开为 `wpfgfx_cor3.dll`。
- `wpfgfx.vcxproj` 的 `TargetName` 为 `wpfgfx$(WpfVersionSuffix)`，而 `VersionSuffix.props` 设置 `_cor3`。
- 原 `.def` 的 `LIBRARY DLL_NAME` 通过 `/DDLL_NAME=$(TargetName)` 预处理后传给链接器；本轮没有生成 `wpfgfx.i`。

因此目标意图是让独立 AOT 项目的最终共享库基础名与当前调用方一致，即 `wpfgfx_cor3.dll`。`AssemblyName=wpfgfx_cor3` 是候选手段，但最终 DLL、import library、PDB 和 loader 命中路径必须由实际 publish 产物验证。

### 5.2 四份并列清单

必须同时维护：

1. 106 个 `.def` 静态名称；
2. 原生 prototype/定义；
3. 107 个托管唯一 EntryPoint；
4. 实际原 DLL 和 AOT 候选 DLL 的二进制导出。

映射单位是**唯一 native EntryPoint**，不是 C# 方法声明数：

- 109 个托管声明出现；
- `MILAddRef` 和 `MILQueryInterface` 各有两个托管重载；
- 因而是 107 个唯一 EntryPoint；
- 每个唯一公开名最多对应一个 `[UnmanagedCallersOnly(EntryPoint=...)]` 薄导出。

每条映射还必须记录：原定义/声明文件、C# 镜像、精确 unmanaged 签名、调用约定、架构、所有权、线程、初始化前置、错误码、callback、资源和验证状态。只有名称相同不足以生成入口。

### 5.3 99 个交集与 8/7 差异

#### 5.3.1 99 个共同名称

99 个 `.def` 与当前托管 EntryPoint 的交集，是当前最明确的双边兼容候选面。它们最终应逐一映射为：

```text
精确 PE export 名
  -> static [UnmanagedCallersOnly] ABI 薄入口
  -> 参数/位宽/所有权转换与异常封锁
  -> 对应原生文件的逐文件等价 C# 实现
```

这不是“立即创建 99 个 stub”的许可。某入口只有在真实实现和该入口的 ABI 测试就绪后才能成为可部署生产导出。

#### 5.3.2 托管有、`.def` 无：8 个

| EntryPoint | 当前静态状态 | AOT 映射规则 |
|---|---|---|
| `MilCompositionEngine_EnterMediaSystemLock` | 有托管声明；当前调用和 WpfGfx 定义/`.def` 未见 | 保持未决，不因声明存在自动导出 |
| `MilCompositionEngine_ExitMediaSystemLock` | 同上 | 同上 |
| `MilGlyphCache_BeginCommandAtRenderTime` | 声明关联当前未编译 GlyphCache 路径；定义/`.def` 未见 | 先核对实际原 DLL、项目条件和可达性 |
| `MilGlyphCache_AppendCommandDataAtRenderTime` | 同上 | 同上 |
| `MilGlyphCache_EndCommandAtRenderTime` | 同上 | 同上 |
| `MilGlyphRun_SetGeometryAtRenderTime` | 声明存在；当前调用、定义和 `.def` 未见 | 保持历史/条件候选，不猜测实现 |
| `MilCreateReversePInvokeWrapper` | 托管包装存在；当前定位的实例化路径未编译；定义/`.def` 未见 | 必须核对实际二进制与 wrapper 生命周期后再决定 |
| `MilReleasePInvokePtrBlocking` | 与上一项配对，语义要求先阻止回调再 release | 不得只补一个入口；必须成对裁决 |

这 8 个名称不能为了让新 DLL“看起来更完整”而先加到 AOT 导出，也不能从 PresentationCore 删除。最终状态只能由原 DLL 实际导出、条件构建、真实调用和兼容决定。

#### 5.3.3 `.def` 有、托管无：7 个

| `.def` 名称 | 当前静态状态 | AOT 映射规则 |
|---|---|---|
| `MILLoadResource` | 原生定义存在；返回 IMAGE 资源借用指针 | 保留 ABI 候选；资源模型未解决前阻塞实现 |
| `MilCompositionEngine_GetComposedEventId` | 原生定义存在 | 保留 native/external caller 候选 |
| `MilCompositionEngine_UpdateSchedulerSettings` | 原生定义存在 | 保留调度控制 ABI 候选 |
| `MilPlayer_Create` | 原生定义存在；常规分支为 `E_NOTIMPL`，PRERELEASE 有真实逻辑 | 必须逐配置保留原条件行为；已证实的原 `E_NOTIMPL` 分支不等于通用占位 stub |
| `MilPlayer_Process` | 与上一项配对 | 同上，必须成对迁移和验证 |
| `MilChannel_SetReceiveBroadcastMessages` | 原生定义存在 | 保留 channel ABI 候选，不因当前托管缺席删除 |
| `SetMilPerfInstrumentationFlags` | 原生定义存在，写性能插桩全局 | 保留诊断/native caller 候选，Debug/Release 行为待核对 |

当前 PresentationCore 没有声明不代表没有 native、诊断、历史或外部调用方。Native AOT 最终兼容 DLL在真实证据裁决前应把这 7 个名称保留在版本化 ABI 台账中。

#### 5.3.4 未实现入口规则

强制原则：

- 不得通过删除 7 个 `.def` 侧独有入口制造一致；
- 不得通过修改或删除 8 个托管侧独有声明制造一致；
- 真实实现尚未迁移的入口不得返回假 `S_OK`、假 `TRUE`、空对象或其它“成功”结果；
- spike 只导出专用受控测试入口，不批量伪造生产导出；
- 最终每个生产入口必须有真实实现、明确阻塞，或经二进制/调用方证据批准的书面裁决。
- 也不得批量返回 `E_NOTIMPL` 伪装为迁移完成；只有原实现本来就在对应配置返回 `E_NOTIMPL`，且分支、条件和副作用已被逐文件翻译并验证时，才是等价行为。
- 开发中的 AOT DLL若缺少生产实现，应标记为不可替换/不可部署，而不是靠 stub 满足导出计数。

### 5.4 `.def` 外数据导出候选

**事实—仓库静态证据：** `shared/util/DllUtil/Precomp.h` 和 `dllmain.cxx` 在 Debug/`DBG` 下分别声明和定义 `__declspec(dllexport) BOOL g_fNoMeterChecks`；`Graphics.props` 在 Debug 定义 `PERFMETER=1`。该名称不在 106 个 `.def` 条目中，真实 Debug DLL 是否导出、拼写为何、是否标记为 data 均未验证。

`g_fNoMeterChecks` 是 Native AOT **高风险**项：当前 `[UnmanagedCallersOnly]` 模型面向函数入口，不能据此假定可等价产生 PE 数据导出。把它改成 getter/setter 函数会改变外部 ABI，也不允许在等价迁移期自行采用。

是否可通过 Native AOT/linker 的额外 `.def`、对象文件、链接参数或其它受支持机制保留该数据符号、地址稳定性、最终名称和 `DATA` 属性，必须由目标 SDK、链接器和二进制检查验证。若 Debug 工具真实依赖直接读写该变量，而 AOT 无法输出等价 data export，则 Debug 产物兼容性被阻塞；不能仅让 Release 通过后宣称全部配置等价。

## 6. COM、SafeHandle、P/Invoke、回调与生命周期

### 6.1 COM 与 SafeHandle

#### 6.1.1 仓库静态事实

- `MILAddRef`、`MILRelease`、`MILQueryInterface` 是平面 C 导出，但操作的是 `IUnknown*`。`MILQueryInterface` 失败前清零 out，成功结果拥有一份新引用。
- `MILCreateFactory`、bitmap/WIC、stream、media 等入口返回 COM 或 COM-like 对象指针；调用方由 `SafeMILHandle`、`SafeMediaHandle` 或显式 release 接管。
- `SafeMILHandle.ReleaseHandle` 调用 `MILRelease` 并清零句柄；其 GC memory pressure 只是 GC 调度信息，不是 COM 引用计数。
- `SafeMediaHandle.ReleaseHandle` 先执行 `MILMediaShutdown`，再 `MILRelease`；现有代码还会对 shutdown HRESULT 执行 `HRESULT.Check`。这说明 close/shutdown/release 顺序和失败行为是兼容输入，不能合并为普通 finalizer。
- channel、connection、32 位 DUCE resource ID、Win32 handle 与 COM pointer 不是同一种资源，不能全部包装成同一 SafeHandle 模型。
- AV state thread 在工作线程中显式 `CoInitialize(NULL)`，退出时 `CoUninitialize()`；COM apartment 是线程协议的一部分。

#### 6.1.2 Native AOT 限制与要求

**已知语义—待 SDK 验证：** `[UnmanagedCallersOnly]` 只生成平面函数入口，不会自动把任意 C# 对象变成具有稳定 native vtable 的 COM 对象。导出返回值不能是托管对象引用，也不能把对象地址 pin 后冒充 `IUnknown*`。

对于 wpfgfx 的 COM 边界，必须分别验证：

1. **消费外部 COM：**AOT 代码调用 WIC、D3D9、DWrite、AV 等系统/第三方 COM 时，是否采用原始 vtable 函数指针、source-generated COM、`ComWrappers` 或目标 SDK 支持的其它 AOT-safe 机制；不得假设反射驱动的内置 RCW/CCW 在裁剪/AOT 下自动可用。
2. **向调用方提供 COM-like 对象：**factory、media、bitmap 等对象必须暴露精确 vtable 顺序、IUnknown 三槽、x86 stdcall/x64/ARM64 ABI、IID、AddRef/Release/QI 和对象内存寿命。任何 `ComWrappers`/source-generated COM 方案都必须用 C++ caller 和现有 PresentationCore 差分证明；本调查不预先选型。
3. **引用与 GC rooting：**native refcount 与托管 GC root 必须形成闭环。`AddRef` 不能只增加托管字段而不保证对象存活；最终 `Release` 必须在无并发调用后释放 native-facing vtable/object 和托管 root。
4. **apartment：**每个调用 COM 的线程保持原 `CoInitialize`/`CoUninitialize` 模式、错误和顺序；不能依赖“托管线程会自动初始化 COM”。
5. **aggregation/marshalling：**接口跨 apartment、代理/stub、free-threaded 与 agile 假设必须逐接口确认；不能因对象只在同一 DLL 实现就省略 COM 规则。

Native AOT 的 built-in COM interop、source-generated COM 和 `ComWrappers` 在目标 .NET 10 SDK 中各自支持到什么程度，是**外部平台事实待验证**。在此之前，COM 可表达性是高风险前置，而不是“unsafe 即可解决”的已知结论。

SafeHandle 可用于 AOT 实现内部拥有的 OS/native 资源，但：

- SafeHandle 不能直接出现在 `[UnmanagedCallersOnly]` ABI 签名中；边界必须是原生 handle/pointer。
- finalizer 时序不确定，不能替代 channel close→commit→destroy、media shutdown→release、callback detach 或线程 join。
- AOT DLL的调用方仍是 CoreCLR PresentationCore；调用方 SafeHandle 的最终化发生在另一运行时中。AOT 实现不能依赖访问该 SafeHandle 对象，只能实现其最终调用到的 native ABI。
- 卸载/进程退出期间是否仍会运行本方 finalizer、SafeHandle release 和 ProcessExit handler 必须实测；不得作为资源正确性的唯一机制。

### 6.2 P/Invoke 与动态原生依赖

#### 6.2.1 仓库静态事实

- 原 `wpfgfx.vcxproj` 直接链接多个 Windows import library，并对 `winmm.dll`、`WindowsCodecs.dll` 使用 delay-load；WpfGfx 还有 `DynamicCall` 和多种惰性 loader。
- PresentationCore 的 `DWriteLoader` 展示了明确动态加载模型：从 System32 `NativeLibrary.Load("dwrite.dll", ..., DllImportSearchPath.System32)`，`GetExport("DWriteCreateFactory")` 为 `delegate* unmanaged<...>`，退出时先清空函数指针再 `NativeLibrary.Free`。
- 当前 PresentationCore 对 `wpfgfx` 本身没有显式 `NativeLibrary.Load`；首次 MilCore P/Invoke 触发 loader 解析。

#### 6.2.2 Native AOT 限制与要求

**已知语义—待 SDK 验证：** AOT 编译不禁止调用 native DLL；静态 P/Invoke、source-generated `LibraryImport`、`NativeLibrary.Load/GetExport` 和 unmanaged function pointer 是候选机制。但以下语义必须保留：

- 普通动态 P/Invoke 与 Native AOT 的 direct P/Invoke/link-time binding 不是同一行为。后者可能把“首次调用/可选模块缺失”改变为“进程或 DLL 加载即失败”。
- 初始项目不应启用广泛 `DirectPInvoke`。原工程有 delay-load、可选 AV/D3D/DWrite 组件和缺失模块错误映射；只有逐库证明加载时机、搜索路径、import library 和失败语义等价后，才可考虑直接绑定。
- native 库名和搜索路径必须是静态、可审计输入。不得依赖当前目录偶然命中，也不得把 System32-only 加载改成默认搜索。
- 动态 `GetExport` 优先使用明确 `delegate* unmanaged[...]`；若使用 delegate marshalling，必须保留 delegate、确认调用约定并通过 trim/AOT analyzer。
- P/Invoke marshalling stub 本身受 AOT/裁剪约束。复杂字符串、数组、delegate、COM、`bool` 和结构不能只复制当前 `DllImport` 声明；应在 AOT 内部边界使用已验证低层形状。
- 每个动态模块的 handle 与函数指针必须按顺序失效：先阻止新调用/清空或切断指针，再等待在途调用，再 `Free`；DWriteLoader 的静态模式是证据，但不能自动推广到所有子系统。
- Native AOT shared library 自身包含运行时，并由 CoreCLR 进程当作普通 native DLL调用。两个运行时的 managed assembly、GC heap、exception、delegate 和 handle table 不共享。

### 6.3 reverse P/Invoke 与函数指针

#### 6.3.1 已确认的四类 callback

| callback | 仓库事实 | AOT 薄层要求 |
|---|---|---|
| `CStreamDescriptor` | 14 个 `__stdcall` 函数指针 + `DWORD_PTR`；native 复制 descriptor；托管静态字段保活 delegate；析构回调释放 CoreCLR `GCHandle` | AOT 侧只复制并调用外部函数指针；不得把 token 当作本方 `GCHandle`；x86 stdcall、结构 offset、长期保活和析构时机逐项验证 |
| `CEventProxyDescriptor` | 2 个 stdcall callback + `DWORD_PTR`；AV event thread 调用；锁内检查 shutdown；析构对 CoreCLR shutdown reentry 有 SEH 特例 | shutdown 先封住新 callback，再释放 owner；不得在本方运行时解释 CoreCLR token；异常/HRESULT 和进程退出重入必须差分 |
| geometry `AddFigureToList` | `CALLBACK`，两个 `BOOL`，同步遍历期间调用，返回 `void` | 明确 stdcall/BOOL；callback 抛异常的封锁策略必须有差分证据，不能吞错后伪成功 |
| D3DImage front-buffer callback | native 明确用 `BOOL` 避免 bool 歧义；render/composition thread 调用；托管实例字段保活；`Detach` 在锁内清空 callback | detach 必须先阻止 render thread 后续进入，再释放上下文；调用方 callback 是 CoreCLR thunk，AOT 只视为 native pointer |

#### 6.3.2 跨运行时 token 规则

这是 Native AOT 替换特有的关键约束：宿主 PresentationCore 运行在 CoreCLR，而 `wpfgfx_cor3.dll` 候选将包含独立 Native AOT 运行时。

- CoreCLR 创建的 `GCHandle` 数值只能由 CoreCLR callback 代码解释和释放。AOT DLL可以按 `nint`/`nuint` 复制、保存并原样传回，但禁止调用本方 `GCHandle.FromIntPtr` 读取它。
- AOT DLL为自身对象创建的 GCHandle/token 同理只能由本方运行时解释。若交给外部 native/COM 代码，只能作为不透明 context，并由本方 UCO callback 收回。
- 两个运行时的 delegate、exception、object reference、type handle、thread-static 和 GC root 不能跨边界传递；边界只能是 C ABI 数据和 native function pointer。
- callback descriptor 被 native/AOT 侧按值复制后，原地址、函数指针和 token 的寿命必须覆盖全部在途调用。创建失败时还要明确由哪一侧释放已经分配的 CoreCLR `GCHandle`，避免泄漏或双重释放。

#### 6.3.3 AOT 自身 callback

当 AOT 实现向 Windows、COM 或自身工作线程提供 callback 时，候选入口应是静态 `[UnmanagedCallersOnly(CallConvs=...)]` 方法或其它经目标 SDK证明的 AOT-safe thunk。必须：

- 用 context token 定位本方状态，不捕获实例；
- 在回调内附着/进入运行时的真实行为由目标 SDK验证；
- 支持并发和非 UI 线程进入；
- 封锁全部普通托管异常；
- 在释放 context 前先注销 callback、停止新进入并等待在途调用归零；
- x86 callback 使用精确 stdcall，x64/ARM64 分别验证。

`MilCreateReversePInvokeWrapper` / `MilReleasePInvokePtrBlocking` 的 8 项差异状态尤其说明“阻止新 callback + 等待在途 callback + 再 release”是既有要求，但这两个入口的真实定义/导出仍未确认，不能直接把名字复制进 AOT。

### 6.4 静态初始化、线程与卸载

#### 6.4.1 原 DLL 生命周期事实

原入口链为 `_DllMainStartup[Debug] -> _DllMainStartupImpl -> CRT -> core/dll/dllentry.cpp::DllMain -> MILCoreDllMain`。

| 路径 | 静态事实 |
|---|---|
| process attach | 创建 WPF process heap、DebugLib/CRT；保存 `g_DllInstance`；禁用 thread notifications；初始化 AV、composition/graphics locks、RenderOptions、`Startup`、`SwStartup`、`HwStartup`、ETW |
| attach 失败 | `MILCoreDllMain` 由 HRESULT 决定返回 FALSE；当前初始化并非一个可随意重排的托管静态构造器 |
| 显式 `FreeLibrary` detach | `lpreserved==NULL`，进入 CRT/用户 DllMain，执行 ETW、HW、SW、core、AV、锁、DebugLib 和 process heap 清理 |
| 进程终止 detach | `lpreserved!=NULL` 且 `g_fAlwaysDetach==FALSE` 时 DllUtil 直接返回，不执行完整用户/CRT shutdown，因为此阶段很多操作不安全 |
| AV state thread | 用 `CreateThread`，thread proc 为 stdcall，线程内 `CoInitialize`；最终对象析构会无限等待线程结束后关闭 handles |

PresentationCore 还有独立的 Dispatcher shutdown、MediaSystem shutdown、MediaPlayer `ProcessExitHandler`、D3DImage AppDomain shutdown/detach、SafeHandle/finalizer 和系统 DWrite `NativeLibrary.Free`。这些不是一个统一“退出事件”。

#### 6.4.2 Native AOT 初始化限制

**已知语义—待 SDK/运行验证：** Native AOT shared library 自有运行时和 PE 启动代码；用户不能假定能用普通 C# 直接替换自定义 `_DllMainStartup`/CRT 链，也不能假定 module initializer、静态构造器是在 `LoadLibrary`、首次 `GetProcAddress` 还是首次导出调用的某一精确时刻运行。

因此：

- 不得把原 attach 的全部工作塞入一个 `[ModuleInitializer]` 或大型类型初始化器。loader lock、异常传播、失败返回、初始化次序和可重入性都可能不同。
- 任意当前 MilCore P/Invoke 都可能是第一调用；正常路径甚至可能先 `MilContent_AttachToHwnd` 再 `MilVersionCheck`。所有导出都必须面对“首次进入时全局运行状态尚未显式建立”的问题。
- 若后续采用受控的一次性初始化状态机，它只能承载原 attach 语义并保持原顺序/失败；不能借机增加自动回滚或推迟本应在 attach 完成的可观察状态。具体机制属于后续实现设计。
- 静态构造异常、递归进入、并发首次调用和部分初始化必须有探针；不能只测单线程首次调用。
- Native AOT 的 `ModuleInitializer`、`AppDomain.CurrentDomain.ProcessExit`、finalizer 和 runtime shutdown 在 native-library host 中何时触发，均需目标 SDK实测，不能作为原 DllMain 等价物预设。

#### 6.4.3 卸载是最高级生命周期风险

**高风险外部事实待 .NET 10 官方文档/SDK验证：** Native AOT shared library 的历史平台约束中，`FreeLibrary`/`dlclose` 卸载通常不能被默认视为受支持的可重入生命周期。当前工作区没有证据证明 .NET 10 已支持安全卸载、再次加载或多个实例。

这与原 wpfgfx 明确支持显式 detach 的事实直接冲突，必须单独立门：

1. 验证 `LoadLibrary -> GetProcAddress -> call -> FreeLibrary` 是否受官方支持，而不只是偶然未崩溃；
2. 验证卸载前后台线程已停止并 join、COM 已释放、callback 已 detach、timer/ETW/TLS/finalizer 不再引用模块代码；
3. 验证调用方持有的 COM pointer、function pointer、SafeHandle 或 callback 在 unload 后全部失效且不会再调用；
4. 验证显式 unload 与进程终止仍有不同路径，不能在 process termination 中强行执行完整 shutdown；
5. 验证同一进程再次加载同名 DLL、重复初始化和静态状态是否被支持；
6. 若平台明确不支持卸载，则“与原 DLL 显式 unload 完全等价”必须标记 `Blocked`，不能用泄漏模块引用、禁止 `FreeLibrary` 或依赖进程退出作为静默 workaround。

当前 PresentationCore 没有显式卸载 `wpfgfx`，所以常规应用路径可能在进程期保持模块加载；但 ABI 契约和其它 native caller 仍可能要求显式 unload。这只能降低常见路径发生概率，不能消除兼容阻塞。

#### 6.4.4 线程关闭门禁

- 每个 native/managed 工作线程都登记创建者、apartment、回调目标、停止信号、join、handle close 和错误路径。
- 关闭顺序必须是：阻止新工作/回调 → 唤醒并请求退出 → 等待在途调用/线程结束 → 释放 callback context/COM/handles → 最后允许模块卸载。
- 不允许后台线程在 AOT DLL被卸载后继续执行本模块代码。
- 不允许在 DllMain/loader lock 风险上下文中等待可能依赖 loader 的线程；原实现的具体顺序需差分，不在本调查中重构。
- ThreadPool、托管 `Thread` 或 `Task` 不是原 `CreateThread`/event/COM apartment 的自动等价替代；采用任一机制都要保留线程身份、优先级、同步和关闭行为。

## 7. 资源、版本信息、额外导出、符号与调试

### 7.1 资源嵌入

#### 7.1.1 两种不同资源作用域

**事实—仓库静态证据：**

| 资源路径 | 当前实现 | 所属模块与寿命 | AOT 风险 |
|---|---|---|---|
| `MILLoadResource` | `FindResource(NULL, src, L"IMAGE")`、`LoadResource(NULL, ...)`、`LockResource`、`SizeofResource` | `NULL` 表示宿主**可执行文件**模块；返回宿主 EXE 资源的借用指针 | 绝不能改成读取 AOT 程序集的 managed embedded resource 或 AOT DLL资源；必须继续从调用进程 EXE 的 `IMAGE` 资源按名称读取 |
| 固定硬件/effect shader | `FindResource(g_DllInstance, MAKEINTRESOURCE(id), RT_RCDATA)`；shader/effect 路径返回借用指针 | 属于 `wpfgfx` DLL自身 PE 资源；指针有效到包含资源的模块卸载 | AOT DLL必须含相同 numeric ID、type、字节和模块作用域，并能取得自身真实 HMODULE；卸载后所有借用指针失效 |
| ETW message/template | `milcore.rc` 包含 `wpf-etw.rc`；后者包含 message table type 11 和 `WEVT_TEMPLATE` 二进制 | 属于最终 native DLL 资源 | 仅注册 provider 代码存在不够；message/template 资源缺失会改变 ETW 解码与诊断 |
| 版本资源 | `milcore.rc` 包含生成的 `NativeVersion.rc`；WPF C++ targets 还生成并加入扩展 `VS_VERSION_INFO` | 属于最终 PE | managed assembly attributes 是否自动映射为相同 PE VERSIONINFO 不可假定 |

`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/hw/hw.rc` 同时包含：

- 签入的固定 `Shaders.rc` RCDATA，资源 ID 至少包括 `100..115`；
- `ShaderAssemblies/EmbeddedShaders.RC`，ID `900..907`；
- 后一组由 `ShaderAssemblies.targets` 调用 Windows SDK `fxc.exe` 从 `.fx` 生成 `.vsbin`/`.psbin` 后编入 `hw.res`；
- `hw.vcxproj` 把 `hw.res` 作为额外链接输入传到最终 `wpfgfx` DLL。

本轮禁令阻止重新运行 `fxc`、生成 `hw.res` 或比较资源。因此后续迁移必须把当前实际资源字节、ID、type、配置/架构和生成来源冻结为基线；不能在 AOT 项目中重新编译后未经差分直接采用。

#### 7.1.2 Native AOT 资源门禁

**已知语义—待 SDK/linker 验证：** SDK-style `EmbeddedResource` 通常形成 managed resource，不等于 Win32 `.rsrc`。wpfgfx 依赖的是 PE Win32 resource API，故需要验证目标 Native AOT 发布链是否支持合并 `.res`/`.rc` 或其它受支持的 Win32 resource 输入。

必须验证：

1. AOT DLL自身 `HMODULE` 的取得时机和方法，不依赖不可控的自定义 DllMain 假设；
2. numeric/string resource ID、resource type、language、size 和逐字节内容；
3. `FindResource(NULL,...)` 仍查看宿主 EXE，而 `FindResource(self,...)` 查看 AOT DLL；
4. managed resource 与 native `.rsrc` 没有被误混；
5. AOT/runtime 自带 manifest、version 或其它 resource 与 WPF ID/type 无冲突；
6. Debug/Release、x64/ARM64/可能的 x86 产物资源集合一致性；
7. 借用指针在模块存活期间稳定，且卸载测试不在指针失效后继续使用。

资源缺失不能通过代码内置另一份数组静默绕过；是否允许改为等价字节存储属于后续逐文件偏差决策，必须先证明资源 API 本身是否是外部可观察契约。

### 7.2 文件版本与产品信息

**事实—仓库静态证据：** WPF C++ targets 为动态库生成 `VS_VERSION_INFO`，字段至少包括：

- fixed `FILEVERSION`、`PRODUCTVERSION`、flags、OS、`VFT_DLL`；
- `FileDescription`；
- `FileVersion`；
- `InternalName`；
- `LegalCopyright`；
- `OriginalFilename`；
- `ProductName`；
- `ProductVersion`；
- language/codepage `0409/04b0`。

**已知语义—待 .NET 10 SDK验证：** SDK-style 项目的 `AssemblyVersion`、`FileVersion`、`InformationalVersion`、`AssemblyTitle`、`Product` 等可能影响 native AOT PE metadata/version resource，但不能假定其字段、language、file type、original filename 或格式与 WPF C++ 资源完全一致。

后续项目必须显式记录并验证：

- `AssemblyName=wpfgfx_cor3` 与 `OriginalFilename=wpfgfx_cor3.dll` 是否一致；
- 文件版本、产品版本和协议 `MIL_SDK_VERSION=0x200184C0` 是三类不同版本，不得互相替代；
- Debug flag 与 Debug/Release 资源差异；
- source revision/informational version 是否进入可观察字符串；
- Windows 文件属性 UI、Version API 和 PE resource enumeration 的实际值；
- 签名、时间戳和可重现构建字段是否由发布流程另行处理。

若 SDK默认 VERSIONINFO 不足，是否通过 native resource merge 覆盖/补充必须与第 7.1、7.3 节共同验证，不能同时生成两个冲突的 `VS_VERSION_INFO`。

### 7.3 模块定义文件与额外导出

**事实—仓库静态证据：** 原 DLL把预处理后的 `wpfgfx.i` 作为链接器 `ModuleDefinitionFile`；原始 `.def` 的 106 项无 ordinal、alias、forwarder、`NONAME`、`PRIVATE` 或 `DATA`。Debug 另有源码级 data export 候选 `g_fNoMeterChecks`。

**已知语义—待 .NET 10 SDK/linker 验证：** Native AOT native-library 的正常函数导出模型是带 `EntryPoint` 的 `[UnmanagedCallersOnly]`。这不自动证明以下能力：

- 将现有 `.def` 直接作为受支持项目输入；
- 让 `.def` 与 UCO 导出合并而不重复/重命名；
- 指定 ordinal、alias、forwarder、`NONAME`、`PRIVATE`；
- 导出可读写 data symbol；
- 生成与原 C++ import library 完全兼容的 `.lib`/`.exp`；
- 对 x86 stdcall 内部修饰名应用原链接器的自动匹配规则；
- 链入额外 `.obj`/`.res` 后保留符号和初始化语义。

因此初始 spike 应只使用一个 UCO 函数导出证明正常路径，**不**先注入原 106 项 `.def`。随后建立隔离 linker spike，逐项验证模块定义文件/额外输入是否为目标 SDK的受支持扩展点。任何依赖内部/未文档化 ILC 参数的方案都应标为工具链耦合风险，不能直接进入生产基线。

产物门禁必须比较：

- export name、kind（code/data）、RVA、ordinal、有无 forwarder；
- Debug/Release 的额外符号；
- `.lib` 中公开符号及 native linker 能否不改 caller 地链接；
- `GetProcAddress` 与静态 import 两种 caller；
- `.def`、UCO 和额外 native object 之间的冲突诊断。

### 7.4 符号和调试

**事实—仓库静态证据：** 原 WPF native 构建对非托管项目始终启用 linker debug information，PDB 命名为 `$(TargetName).pdb`；Release 还请求 CodeView、pdata、fixup 调试信息。仓库根 `PublishWindowsPdb=false` 是当前仓库发布属性，不是未来隔离 AOT 项目应复制的决定。

**已知语义—待 SDK验证：** Native AOT 生成 native machine code，Windows 发布通常应有可供 native debugger/symbolizer 使用的符号产物；其文件种类、是否同时存在 portable managed PDB、默认发布/裁剪/strip 行为和 source mapping 必须以目标 SDK产物为准。

首轮不得为减小体积关闭：

- stack trace data；
- exception/unhandled-exception 体验；
- debugger support；
- native symbols/PDB 生成；
- 方法名/源映射所需 metadata。

必须验证：

1. PDB 能解析 `wpfgfx_cor3.dll` 的 UCO export thunk 和内部 C# 方法；
2. native C/C++ caller → AOT export → 内部实现 → P/Invoke/COM 的混合栈可读；
3. 同一进程同时存在 CoreCLR 与 Native AOT runtime 时，dump/debugger 能区分两套托管运行时及边界；
4. first-chance/未处理异常、边界内已映射异常和 process termination 的诊断差异；
5. Release 优化、内联和裁剪后仍能关联关键 ABI/生命周期帧；
6. x64 unwind/pdata 与 ARM64 unwind 信息有效；若 x86 可用则验证栈回溯和 stdcall；
7. 动态 FXJIT code 是否能被 profiler/debugger 识别，或至少能以地址、生成来源和寿命诊断；
8. 符号服务器/运输包是否携带正确架构与配置的符号，而不把原 native PDB 与 AOT PDB 混淆。

“托管单测堆栈可读”不能证明 native crash dump、loader fault、栈破坏、COM 回调或卸载后执行的可诊断性。

## 8. FXJIT、W^X、Native AOT 与 CFG 风险

> 本节仅记录事实、阻塞和验证需求，不设计重构或替代编译器。

### 8.1 仓库静态事实

- `core/fxjit/Platform/Platform.cpp::CJitterSupport::CodeAllocate` 使用 `VirtualAlloc(..., PAGE_EXECUTE_READWRITE)` 一次取得 RWX 页面；`CodeFree` 使用 `VirtualFree(..., MEM_RELEASE)`。
- `CProgram::Assemble` 先计算大小，再把常量和第二遍生成的机器码直接写入该 RWX 区域，随后把地址作为二进制代码返回。
- `CPixelShaderCompiler::Compile` 将该地址转换为 `GenerateColorsEffect*`；软件渲染 `CShaderEffectBrushSpan::GenerateColors` 通过函数指针直接调用。
- compiler/source 明确包含 `Coder86`，大量分支只区分 `WPFGFX_FXJIT_X86` 与注释中的 `_AMD64_`；`SIMDJit.h` 只在 `_X86_` 或旧 `_ARM_` 时定义 x86 路径。虽然 fxjit 项目列有 ARM64 配置，但当前静态代码不能证明会生成 ARM64 指令。
- 全局 `g_pProgram` 通过 `g_LockJitterAccess` 串行保护；`SwStartup` 初始化 jitter lock，`SwShutdown` 删除；`ShaderEffect.h` 的初始化函数在 lock 创建失败时经过 `IFC(E_FAIL)`，但最终固定 `RRETURN(S_OK)`，这是需要保真的现有异常行为，不能静默“修复”。
- WPF native Release 编译启用 Control Flow Guard；当前 FXJIT 路径的工作区搜索未见 `VirtualProtect`、`FlushInstructionCache`、`SetProcessValidCallTargets` 或等价 CFG target 注册。

### 8.2 与 Native AOT 的关系

**已知语义—待 SDK/Windows 验证：** Native AOT 的含义是托管 IL在发布时预编译，不提供普通托管 JIT/Reflection.Emit 动态编译能力。它不自动禁止应用通过 Win32 API生成并执行原生机器码；但 raw executable memory 是否可用由 Windows mitigation、进程策略、签名和 CPU 架构决定。

因此：

- `RuntimeFeature.IsDynamicCodeSupported=false` 或 AOT analyzer 对 managed dynamic code 的限制，不能直接回答 raw `VirtualAlloc` + function pointer 是否工作；两类“动态代码”必须分开验证。
- Native AOT 也不会自动给 FXJIT 生成代码补 unwind metadata、CFG registration、instruction-cache flush、profiler registration 或 W^X 转换。
- AOT 自身的静态代码可通过 CFG，不代表运行时新分配地址可作为合法 indirect call target。
- 在一个已启用 Arbitrary Code Guard/动态代码禁止策略的进程中，RWX 分配或变更可执行权限可能失败；这可能由宿主而非 DLL控制。

### 8.3 W^X、CFG 与架构阻塞

| 风险 | 当前静态状态 | 必须验证 |
|---|---|---|
| RWX/W^X | 原实现直接使用 `PAGE_EXECUTE_READWRITE`，没有写后转 RX | 目标 Windows mitigation 下是否允许；安全策略是否要求 RW→RX；任何改变都需差分和用户批准 |
| instruction cache | 当前路径未见显式 `FlushInstructionCache` | x86/x64/ARM64 的正确性；不能因 x86 缓存一致性经验推断 ARM64 |
| CFG | 原 Release 模块启用 CFG，动态 target 注册未见 | 生成函数通过 indirect call 是否允许；是否需要/可以注册精确 call target |
| unwind/exception | 动态代码未见 runtime function table/unwind metadata 注册 | generated code 内 fault、stack walk、dump 和异常是否可诊断/安全 |
| architecture | 生成器明显面向 x86/x64；ARM64 指令生成静态未证明 | 每架构 code bytes、调用约定、寄存器、stack alignment、返回和结果差分 |
| lifetime | compiler 析构释放 code；brush 只持 weak function pointer 并由 compiler owner 保活 | 并发调用期间不得释放；shutdown/unload 前全部 generated code 停止并释放 |
| policy | 进程 mitigation 由宿主/系统决定 | WPF 应用、沙箱、企业策略、CFG/ACG/CET 等组合 |

### 8.4 阻塞裁决

- FXJIT 不属于最小 ABI spike，也不能阻塞“一个测试导出”的机制验证。
- 它是软件 ShaderEffect 等价迁移的独立**高风险阻塞项**；进入批次 5 前必须有原 native 基线和 x86/x64/ARM64 探针。
- 若目标架构、W^X/CFG 或 Native AOT host 无法保持原 JIT 语义，必须按章程形成局部书面例外并由用户明确批准。
- 禁止在本调查中以解释器、表达式树、GPU shader、高层编译器或预生成代码替代 FXJIT；这些都属于重构/算法替换。

## 9. 最小 ABI spike（设计，不实施）

### 9.1 目的

只验证 Native AOT shared library 的发布与 C ABI 承载，不迁移 wpfgfx 生产逻辑，不证明渲染、COM、UCE、媒体、资源或 FXJIT 可行。

该 spike 只允许解锁 `WP-00B-NATIVEAOT-ABI-PROBE-X64` 的“项目/导出机制可承载”结论；`WP-00A` 只负责隔离边界与承载。即使全部通过，也不得把任何生产文件状态提升为 `Translated`，不得把候选 DLL描述为 `wpfgfx` 替代品。

### 9.2 唯一受控导出

建议专用名称：**`WpfGfxShape_NativeAotAbiProbe_v1`**。该名称不在 106/107 生产清单中，且带版本后缀，避免被误认为真实 wpfgfx API。

建议 C ABI 形状：

```text
HRESULT WINAPI WpfGfxShape_NativeAotAbiProbe_v1(
    uint32_t abiVersion,
    uint32_t operation,
    uint64_t input,
    uintptr_t context,
    uint64_t* output);
```

对应 AOT 入口设计约束：

- `static`、非泛型；
- `[UnmanagedCallersOnly(EntryPoint="WpfGfxShape_NativeAotAbiProbe_v1", CallConvs = new[] { typeof(CallConvStdcall) })]` 的等价精确声明；x64/ARM64 上仍按平台统一 ABI 验证；
- C# 参数使用明确的 `uint`、`ulong`、`nuint`、`ulong*`，不使用 `bool`、string、array、delegate、SafeHandle 或 managed struct；
- x86 上参数总栈宽和 stdcall cleanup 可专门验证；x64/ARM64 上覆盖 64 位参数、指针宽度和 out pointer；
- `output` 在任何可恢复失败前先清零；
- 内部只调用一个普通静态 probe implementation，使 managed unit test 与 native ABI test 能清楚分层。

推荐操作语义：

| `operation` | 行为 | 预期 HRESULT | `output` |
|---:|---|---:|---|
| `0` | 对 `input`、`context` 和当前 pointer size 做固定、公开、`unchecked` 的确定性 checksum | `S_OK` | 精确预期 checksum；用于验证参数顺序、64 位宽度、指针宽度和 out 写入 |
| `1` | 受控参数错误路径 | `E_INVALIDARG` | `0` |
| `2` | 普通内部 helper 有意抛 `InvalidOperationException`；UCO 最外层捕获并映射 | 固定 `E_FAIL` | `0` |
| 其它 | 未定义 probe operation | `E_NOTIMPL` | `0` |

通用参数规则：

- `abiVersion != 1` → `E_INVALIDARG`；
- `output == NULL` → `E_POINTER`；
- 不读写 `context` 指向的内存，只把其数值纳入 checksum，因此不会引入所有权；
- 预期异常与兜底普通异常使用不同固定失败码更利于测试，但不得让任何异常越界；
- 在异常调用后再次执行 operation 0，必须仍成功，证明运行时/栈未被破坏。

这里的 `E_NOTIMPL` 是**该测试协议明确规定的未知 operation 结果**，不是生产入口的占位返回；不得据此批量生成生产 stub。

### 9.3 native caller 与测试承载

至少需要两个独立 native caller 形态；都属于后续实现，不在本轮创建：

1. **动态加载 caller**
   - 用绝对路径和受控 loader flags 加载隔离目录中的 DLL；
   - 用 `GetModuleFileNameW` 证明加载的是目标文件；
   - 用精确大小写的 `GetProcAddress` 获取 probe；
   - 验证错误拼写、错误大小写和 x86 装饰候选名不应被意外解析；
   - 调用全部 operation、并发首次调用、重复调用和卸载场景。
2. **静态 import caller**
   - 若 publish 产生受支持 import library，则用 C header + `.lib` 正常链接；
   - 验证不修改 caller prototype 即可链接和运行；
   - 与动态 caller 的结果完全一致。

32 位 caller 还需一个低层 ABI shim，在调用前后记录 `ESP` 并验证相等，或使用等价的原生 ABI 检查机制；只观察“没有立刻崩溃”不足以证明 stdcall 栈清理正确。x64/ARM64 需验证非易失寄存器、栈对齐和 unwind，不可由 C# 单测替代。

所有加载/异常/卸载测试应在可隔离的子进程执行，使 loader crash、fail-fast 或栈破坏成为明确测试结果，而不是破坏整个测试运行器。

### 9.4 架构推进裁决

1. **先 x64 Debug**：最快验证项目属性、文件名、UCO export、C caller 和异常封锁。
2. **再 x64 Release**：验证优化、裁剪、符号、导出和异常行为没有配置漂移。
3. **随后 ARM64 Debug/Release**：必须在真实 ARM64 Windows 上加载/调用；仅交叉编译或由 x64 host 检查 PE 不算运行通过。
4. **x86 支持调查并行进行**：先取得官方 .NET 10 支持结论，再尝试 `win-x86` shared publish；若受支持，执行同一矩阵并增加 stdcall/ESP 检查。

x64/ARM64 **可以先行**，不必等待 x86 才验证基本机制；但：

- 批次 0 只能标记“x64/ARM64 机制通过”；
- x86 未裁决时，跨架构 ABI 工作项保持 `Blocked`；
- 不得开始以“所有目标架构可行”为前提的大规模生产逻辑迁移；
- 不得因 x64/ARM64 成功而从主计划删除 Win32 等价目标。

### 9.5 明确禁止

- 不复用生产 ABI 名称假装入口已迁移；
- 不创建 106 个 stub；
- 不以 `S_OK`/`TRUE` 掩盖未实现；
- 不进入 COM、渲染、媒体、UCE 或 FXJIT 生产迁移。
- 不把 probe DLL复制到 PresentationCore 或 WPF native 产物目录；即使基础名为 `wpfgfx_cor3.dll`，也只允许留在带 `SPIKE-NOT-FOR-DEPLOYMENT` 语义的隔离输出中。
- 不用 managed caller 代替 C/C++ caller；不以内部普通静态方法单测替代真实 PE export 调用。
- 不在最小 spike 加 callback、COM object、resource、background thread 或自定义 shutdown；这些属于后续独立 ABI/lifecycle spike。
- 不通过关闭 analyzer、裁剪、CFG、异常或符号支持让 publish“先过”。

### 9.6 spike 完成条件

只有在某一架构的 Debug 与 Release 都满足下列条件时，才能称该架构的最小机制 spike 通过：

- publish 成功且所有 AOT/trim/interop warning 已逐项处理，无全局压制；
- 文件名、PE machine、导出名、PDB/依赖与隔离输出正确；
- 动态和静态 native caller 均能调用；
- success、bad version、null out、explicit error、managed exception 和 post-exception success 全部符合；
- 多线程并发首次/重复调用不产生竞态或异常越界；
- 对应架构 ABI 检查通过；
- loader/unload 结果按官方支持语义记录；若 unload 不受支持，该项为阻塞而不是“测试跳过”；
- 结果明确写成“只验证 probe，不验证生产 wpfgfx”。

## 10. 发布矩阵与验证门禁

### 10.1 发布矩阵

当前设计矩阵如下，所有单元均为**未执行**：

| Configuration | RID | PE 架构 | 当前状态 | 最小 spike 门禁 | 生产兼容含义 |
|---|---|---|---|---|---|
| Debug | `win-x64` | x64 | 待 SDK发布 | 第一优先；完整 probe、符号、并发与 loader 测试 | 只证明 x64 Debug 机制 |
| Release | `win-x64` | x64 | 待 SDK发布 | 裁剪/优化/导出/符号与 Debug 对照 | 只证明 x64 Release 机制 |
| Debug | `win-arm64` | ARM64 | 待 SDK发布 | 真实 ARM64 主机加载/调用；不能只检查文件 | 只证明 ARM64 Debug 机制 |
| Release | `win-arm64` | ARM64 | 待 SDK发布 | Release 与 Debug 对照 | 只证明 ARM64 Release 机制 |
| Debug | `win-x86` | I386 | **Blocked** | 先确认官方/SDK支持；若支持，增加 stdcall/ESP/装饰名测试 | 未通过前不能声称 Win32 等价 |
| Release | `win-x86` | I386 | **Blocked** | 同上，且验证优化后栈/符号 | 未通过前不能声称全架构替换 |

矩阵规则：

- 每个单元使用独立输出和中间目录，至少包含 configuration、RID 和 source revision，禁止互相覆盖。
- `RuntimeIdentifiers` 只是候选 restore 元数据；每格仍以单一 `RuntimeIdentifier` 独立 publish。
- 一个配置通过不能替代另一个配置；一个架构通过不能替代另一个架构。
- 若目标 SDK明确不支持某格，状态写为 `Blocked/Unsupported by verified platform matrix`，附官方版本和证据；不能标成普通 `Skipped` 或静默删除。
- 后续生产发布还需在这些单元上加入完整 106/107 ABI、资源、COM、回调、渲染和生命周期门禁；本表只是批次 0。

### 10.2 产物检查清单

每个发布单元至少检查并留存机器可比对清单：

#### 身份与目录

- [ ] publish 输出位于隔离的 configuration/RID 目录，未污染原 WPF artifacts 或源码树。
- [ ] 主文件精确名为 `wpfgfx_cor3.dll`；大小写、扩展和路径符合部署预期。
- [ ] `GetModuleFileNameW` 返回实际加载的该路径，不是系统/旧工件/另一架构副本。
- [ ] 文件版本、产品版本、source revision、Debug flag 和 `OriginalFilename` 已记录。

#### PE 与架构

- [ ] PE machine 与 RID 一致：I386/x64/ARM64。
- [ ] 32/64 位 pointer size、subsystem、DLL characteristic、NX/ASLR/CFG 等安全标志已记录并与要求比较。
- [ ] import/dependency 列表已记录；不得意外依赖 CoreCLR 或开发机专用 SDK路径。
- [ ] 自包含含义和需要随附的 native/runtime 文件已列清，不以“单个 DLL存在”猜测部署完整。

#### 导出与链接

- [ ] `WpfGfxShape_NativeAotAbiProbe_v1` 精确存在、大小写一致、为 code export。
- [ ] 不存在意外的生产 wpfgfx 名称；spike 不能让调用方误以为 106 项已实现。
- [ ] x86 若可用，公开名无 `_`/`@N` 装饰，且没有错误的 decorated-only export；内部符号不作为公开契约。
- [ ] 工具链附加 export 全部分类并版本化；不能只过滤出想看的名称。
- [ ] DLL/LIB/EXP 是否产生、文件名及静态 import symbol 均记录；若没有 import library，明确说明静态 caller 门禁如何处理。
- [ ] ordinal、forwarder、alias、data export 状态已记录。

#### 资源与版本

- [ ] `.rsrc` 中 VERSIONINFO 的固定字段和字符串字段已枚举。
- [ ] manifest、ETW/message table、shader RCDATA 和其它资源在生产阶段逐 ID/type/language/size/hash 对照；spike 至少证明资源合并机制，不伪造完整资源集。
- [ ] `MILLoadResource` 所需宿主 EXE `IMAGE` 资源语义由专门 harness 验证，不用 DLL resource 测试替代。
- [ ] Debug data export 候选 `g_fNoMeterChecks` 在原 DLL和候选 DLL分别检查。

#### 符号与诊断

- [ ] PDB/native symbol 产物存在并与 DLL GUID/age 或对应标识匹配。
- [ ] UCO thunk、内部实现和异常映射帧可符号化。
- [ ] Debug/Release 的优化、内联、stack trace/unwind 差异已记录。
- [ ] x64/ARM64 unwind 信息与 crash dump 可用；x86 若可用，栈回溯不受 stdcall 错误破坏。

#### 可重复性与供应链

- [ ] 同一输入重复发布的差异已解释；时间戳、版本、签名等预期变化与代码变化分开。
- [ ] SDK、runtime pack、native toolchain/linker 版本和 source revision 被固定到结果。
- [ ] 产物未依赖未记录的环境变量、当前目录或仓库根 WPF targets。
- [ ] 若后续签名/打包，签名前后 hash、VERSIONINFO、PDB 和 loader 行为均有流程记录。

### 10.3 必须通过的运行测试

#### 批次 0 最小 spike

- [ ] **架构匹配加载：**对应位数 native process 从绝对路径加载成功；错误架构在隔离子进程中以预期 loader error 失败。
- [ ] **精确文件身份：**加载后回读模块路径并与预期工件相同。
- [ ] **精确导出：**正确名称成功；错误大小写、后缀和 decorated 变体按预期失败。
- [ ] **动态调用：**operation 0 checksum 在边界值、64 位 high-bit、零/非零 context 上正确。
- [ ] **静态 import：**若有 `.lib`，C caller 不改 ABI 即链接并得到相同结果。
- [ ] **参数错误：**bad ABI version、null output、unknown operation 返回固定错误并清零可写 output。
- [ ] **异常封锁：**operation 2 的 managed exception 只返回 `E_FAIL`，native caller 不接收到 SEH/崩溃；随后 operation 0 仍成功。
- [ ] **重复调用：**大量重复调用无栈漂移、内存增长或状态漂移；x86 显式验证 ESP。
- [ ] **并发首次调用：**多个 native threads 同时首次进入，结果一致，无静态初始化死锁/异常。
- [ ] **进程终止：**子进程调用后直接退出，不要求完整 unload，记录退出和 dump。
- [ ] **显式卸载：**只有在官方支持语义明确后才作为可通过门；执行 load/call/free 和 load/call/free/reload 循环。若官方不支持，结果是兼容阻塞，不是跳过通过。

#### 进入生产 ABI 前必须追加

- [ ] **106/107 export 差分：**原 DLL与候选 DLL逐架构/配置比较 code/data/name/ordinal/forwarder，8/7 状态有裁决。
- [ ] **真实 PresentationCore loader：**不修改其 DllImport 声明，确认加载名、路径、首次入口和错误。
- [ ] **布局：**native `sizeof/offsetof` 与 AOT 表示对照 descriptor、packet、command、union、BOOL/bool/GUID/RECT/handle。
- [ ] **COM：**C++ caller 检查 vtable、IID、QI、AddRef/Release、create/getter ownership、apartment 和并发。
- [ ] **SafeHandle/显式关闭：**GC/finalizer 与明确 shutdown/close 顺序、失败和重复释放。
- [ ] **reverse P/Invoke：**stream/event/geometry/D3DImage 的 stdcall、线程、GCHandle token、异常、detach、阻塞释放和进程退出。
- [ ] **线程：**same/cross-thread channel、AV apartment/event threads、启动失败、停止、join 和 unload 前静止。
- [ ] **资源：**宿主 EXE `IMAGE`、DLL shader RCDATA、ETW、VERSIONINFO 与借用指针寿命。
- [ ] **P/Invoke/native dependencies：**正常、缺失 DLL、缺失 symbol、搜索路径、delay/dynamic loading 和错误映射。
- [ ] **FXJIT：**在适用架构和 mitigation 下生成、执行、结果差分、CFG/W^X/cache/unwind 与释放。
- [ ] **端到端：**真实 PresentationCore `RenderTargetBitmap.Render` 确定性像素差分，之后再扩展 HWND、D3DImage、hardware、media 和退出矩阵。

### 10.4 不能仅由托管单元测试证明

托管单元测试适合内部纯逻辑和异常到错误码 helper，但以下事项必须由 PE 检查、native caller、系统 loader、debugger 或真实 PresentationCore 证明：

- PE 导出表和名称装饰；
- 原生调用方栈清理与寄存器 ABI；
- loader search path 和真实 DLL 身份；
- `.def`/数据导出/ordinal/资源/VERSIONINFO；
- DLL/LIB/EXP 与静态 native 链接；
- 错误架构加载和 PE machine；
- native COM vtable 与引用计数；
- reverse P/Invoke 的真实线程和异常边界；
- `FreeLibrary`、进程终止、后台线程和 callback teardown；
- W^X/CFG/动态代码生成；
- x86/x64/ARM64 的独立兼容性。
- 双运行时 CoreCLR/Native AOT 的 GCHandle token、callback 和 dump 行为；
- native resource 指针在模块卸载前后的寿命；
- PDB、unwind、崩溃 dump 和 Release 优化下的可诊断性；
- 原 PresentationCore 在不修改声明时的真实加载、调用、SafeHandle 和端到端渲染。

## 11. 事实、已知语义、推断、风险与未决

### 11.1 已确认事实

以下只称为工作区静态事实，不等同于实际 AOT 产物：

1. `global.json` 请求 SDK `10.0.107`，根 `Directory.Build.props` 默认 TFM 为 `net10.0`；当前环境是否实际具备该 SDK和 Native AOT toolchain 未验证。
2. 工作区未发现生产 `PublishAot`、`NativeLib=Shared`、`IsAotCompatible` 或 `DirectPInvoke` 模板；旧 Silk.NET CoreRT/ILCompiler 实验不能作为 .NET 10 依据。
3. `src/Microsoft.DotNet.Wpf/src/Shared/RefAssemblyAttrs.cs` 将当前 MilCore 名展开为 `wpfgfx_cor3.dll`；`wpfgfx.vcxproj` 的 native target name 意图同样为 `wpfgfx_cor3`。
4. `wpfgfx.vcxproj` 是 native `DynamicLibrary`，有 Debug/Release × Win32/x64/arm64 六个配置，并使用预处理后的 `.def`。
5. `wpfgfx.def` 有 106 个静态函数名称；未显式使用 ordinal、alias、forwarder、`DATA`、`PRIVATE` 或 `NONAME`。
6. 当前 PresentationCore 基线有 109 个 MilCore 声明出现、107 个唯一 native EntryPoint；99 个与 `.def` 相交，差异为托管独有 8、`.def` 独有 7。
7. x86 native C++ 默认 stdcall；`.def` 公开名未装饰；x64/ARM64 使用平台统一 ABI。
8. Debug 源码有 `.def` 外 `__declspec(dllexport) BOOL g_fNoMeterChecks` data export 候选，实际二进制状态未验证。
9. COM factory/media/bitmap/stream 等返回值由 SafeHandle 或显式 `MILRelease` 管理；media 是 shutdown→release 两阶段；channel 和 DUCE resource 使用不同协议。
10. stream/event descriptor 包含长期保存的 CoreCLR callback 和 `GCHandle` token；geometry callback 同步；D3DImage callback 来自 render/composition thread，`Detach` 清空函数指针。
11. 原 DLL显式 `FreeLibrary` 执行完整 detach，而进程终止默认跳过完整 shutdown；AV state thread 使用 `CreateThread`、COM 初始化和 join。
12. `MILLoadResource` 从宿主 EXE 的 `IMAGE` 资源返回借用指针；内置 shader 从 DLL自身 `RCDATA` 返回借用指针。
13. `milcore.rc` 包含 native version 与 ETW resources；HW 资源含固定 shader 和由 `fxc` 生成的 ID `900..907` shader binaries。
14. 原 native 构建生成 VERSIONINFO 和 PDB；AOT 是否产生等价资源/符号未验证。
15. FXJIT 使用 `VirtualAlloc(PAGE_EXECUTE_READWRITE)` 写入并执行机器码；当前源码搜索未见 W^X 转换、instruction cache flush 或 CFG target 注册。
16. 本轮未修改任何 WPF 源码、项目配置或 `WpfGfxShape/Code`，也未执行任何命令、构建或测试。

### 11.2 已知 Native AOT 语义—待 SDK 验证

1. Native AOT shared library 的候选项目形态是 `PublishAot=true` + `NativeLib=Shared` + 具体 RID；实际 Windows 产物必须通过 publish 验证。
2. `[UnmanagedCallersOnly]` 方法必须为 static、非泛型且不处于泛型类型，参数/返回必须为可直接传递的 unmanaged/blittable 形状。
3. 带 `EntryPoint` 的 UCO 方法是 native export 候选；实际导出名、可达性和 import library 仍需 PE 产物证明。
4. UCO 方法不能由普通托管调用；测试共享逻辑时需调用内部方法，测试 ABI 时必须从 native caller 进入。
5. 托管异常不能安全跨越 unmanaged call/reverse-call 边界；必须在最外层映射为原 ABI 失败语义。
6. x86 stdcall 与 x64/ARM64 平台 ABI 必须通过显式 call convention/function pointer 和真实 caller 独立验证。
7. AOT/trim analyzer 用于发现动态代码、反射和静态可达性问题；warning suppression 不等于兼容。
8. AOT DLL与宿主 CoreCLR 是两套运行时，不能共享托管对象身份、GCHandle 表、delegate 或 exception。
9. Native AOT 可调用 native API，但 direct P/Invoke 可能改变动态/延迟加载语义，不能全局开启。
10. managed embedded resource 不等于 Win32 PE resource；`.res`、VERSIONINFO、ETW message resource 需要 linker 级验证。
11. Native AOT 不提供普通 managed JIT/Reflection.Emit，并不自动禁止 raw native code generation；FXJIT 仍受 Windows mitigation、CFG、W^X 和架构约束。
12. native-library unload/reload、module initializer、ProcessExit 和 finalizer 的精确生命周期必须以目标 .NET 10 官方文档和运行验证为准。

### 11.3 推断

| 推断 | 置信度 | 依据 | 重新评估条件 |
|---|---|---|---|
| `AssemblyName=wpfgfx_cor3` 将是控制目标 DLL基础名的首选属性 | 高 | SDK-style 命名语义与当前 DLL契约 | 实际 publish 产生不同文件名或需额外 native library name 属性 |
| x64 是最适合首个 shared-library spike 的架构 | 高 | 原工程有 x64；避免 x86 支持和 stdcall 双重未知；工具链通常以 x64 为主 | 官方/目标 SDK不支持该形态或 ARM64 环境更先可用 |
| ARM64 应紧随 x64 验证，不能等到生产收口 | 高 | 原工程有 ARM64，结构/函数指针/缓存/FXJIT 风险独立 | 官方明确某功能不适用且用户批准范围变更 |
| Windows x86 可能成为完整替换的不可消除平台阻塞 | 高 | 工作区无 AOT x86 证据；原 ABI明确要求 Win32/stdcall | 官方 .NET 10 支持矩阵和实际 `win-x86` shared publish/调用全部通过 |
| 99 个共同名称是首个明确的生产兼容候选面 | 高 | `.def` 与托管 EntryPoint 双边静态交集 | 实际原 DLL导出或运行可达性与静态清单冲突 |
| 7 个 `.def` 独有名称仍可能有 native/诊断 caller | 中高 | 均有原生定义，部分有 PRERELEASE/诊断语义 | 实际二进制/全仓调用证据证明某项从未运输或不可达，并有书面裁决 |
| 8 个托管独有名称多数是历史/条件路径 | 中高 | 多项无当前调用或关联未编译 GlyphCache，定义/`.def` 未见 | 实际原 DLL导出、条件构建或真实调用证明仍有效 |
| 直接用 UCO 可承载平面函数，但不能独自解决 COM 对象/vtable | 高 | UCO 是函数入口模型；wpfgfx 返回大量 COM-like 指针 | 目标 SDK提供并实测完全匹配的 generated COM 路径 |
| 原显式卸载语义比常规 PresentationCore 路径更难满足 | 高 | PresentationCore通常不主动 unload，但原 DLL有完整 detach | 官方和 stress 测试证明 AOT unload/reload 受支持且等价 |

### 11.4 风险

| 严重度 | 风险 | 影响 | 解除条件 |
|---|---|---|---|
| **P0 阻塞** | Windows x86 Native AOT shared library 支持未知 | 可能无法替换 Win32 `wpfgfx_cor3.dll` | 官方 .NET 10 支持证据 + 实际 win-x86 publish/PE/调用/stdcall callback 全通过，或用户批准架构例外 |
| **P0 阻塞** | Native AOT shared library unload/reload 支持未知 | 无法等价原 `FreeLibrary` 完整 detach | 官方支持 + 重复 load/call/unload/reload、线程/COM/callback/finalizer stress 通过，或用户批准例外 |
| **P0 阻塞** | COM-like 对象/vtable 的 AOT-safe 实现未选定/验证 | factory、media、bitmap 等核心 API无法返回兼容对象 | 原生 C++ caller 验证 IUnknown/vtable/IID/refcount/apartment/所有权 |
| **P1 高** | x86 stdcall 与未装饰 PE export | 栈破坏或 EntryPointNotFound | 每入口/代表签名 native caller 与导出表验证 |
| **P1 高** | 双运行时 callback/token 误用 | GC corruption、泄漏、进程退出崩溃 | 跨 CoreCLR/AOT 长期 callback、失败、detach 和 shutdown stress |
| **P1 高** | Win32 resource/version/ETW/shader 合并 | 资源加载失败、诊断缺失、shader 不可用 | `.rsrc` 枚举、字节/ID/type/version 差分与运行加载测试 |
| **P1 高** | Debug data export `g_fNoMeterChecks` | Debug 工具/ABI 不等价 | 原 DLL实际导出确认 + AOT data export 可行性或用户批准配置例外 |
| **P1 高** | FXJIT RWX/CFG/W^X/ARM64 | 软件 ShaderEffect 失败或违反 mitigation | 分架构 native/AOT 差分与安全策略验证；失败则书面例外 |
| **P1 高** | attach/静态初始化无法映射到 DllMain | 首次任意入口竞态、部分初始化、失败行为变化 | 并发/递归首次调用、失败注入、顺序和 loader 测试 |
| **P1 高** | trim/AOT 将动态路径裁掉 | 运行时缺方法、callback、COM 或资源 | analyzer 清零且 native integration 覆盖所有动态根 |
| **P2 中高** | import library、额外 `.def`/linker input 不兼容 | 静态 native caller 无法链接、额外导出丢失 | DLL/LIB/EXP、静态 import 与 GetProcAddress 双测试 |
| **P2 中高** | 调试符号和混合栈不足 | 崩溃/栈破坏/卸载问题难定位 | Debug/Release dump、PDB、双运行时和动态代码诊断验证 |

### 11.5 未决

| 未决问题 | 当前状态 | 所需证据 |
|---|---|---|
| .NET 10 是否支持 Windows x86 Native AOT shared library | **Blocked** | 目标 SDK官方矩阵、runtime pack/ILC 支持和实际 publish |
| UCO x86 stdcall 是否产生精确未装饰 `EntryPoint` | **Blocked** | PE export + 32-bit C caller 栈/重复调用测试 |
| x64/ARM64 `NativeLib=Shared` 的实际文件组合 | 未验证 | 每 RID publish 的 DLL/LIB/EXP/PDB/依赖清单 |
| `AssemblyName` 是否足以得到 `wpfgfx_cor3.dll` | 未验证 | 实际文件名、import library、loader path |
| `.def`、`.res`、额外 native object/linker option 的受支持入口 | 未决 | .NET 10 native-library SDK文档与隔离 linker spike |
| `g_fNoMeterChecks` 是否在原 Debug DLL真实导出 | 未验证 | 原 DLL Debug PE export/data inspection |
| Native AOT DLL是否支持安全 `FreeLibrary`/reload | **Blocked** | 官方说明和 stress harness |
| Native AOT 下 COM 实现策略 | **Blocked** | 目标 SDK COM 支持文档、analyzer、C++ vtable/refcount tests |
| CoreCLR callback 在 AOT DLL卸载/进程退出时的安全序列 | **Blocked** | stream/event/D3DImage/geometry native integration tests |
| module initializer/静态构造/ProcessExit 的精确触发顺序 | 未验证 | loader/首次调用/退出 trace |
| Win32 resource、VERSIONINFO、ETW 与 shader byte 合并 | 未验证 | resource enumeration 与原产物逐项差分 |
| Native AOT PDB、混合栈和动态 code diagnostics | 未验证 | debugger/dump/profiler 证据 |
| FXJIT 在 x86/x64/ARM64、CFG/ACG/W^X 下可行性 | **Blocked for FXJIT batch** | 原/AOT code bytes、结果、mitigation、unwind、释放测试 |

## 12. 受禁令限制尚未验证

本轮明确不能完成：

- 安装/识别 .NET 10 SDK 与目标 workload/toolchain；
- `dotnet publish` 或 IDE 构建/发布；
- `win-x86`、`win-x64`、`win-arm64` 的实际 Native AOT 支持探测；
- DLL、LIB、EXP、PDB、资源和版本产物检查；
- PE machine、导出名、名称装饰、ordinal、data export 与依赖枚举；
- native C/C++ caller 的加载和调用；
- 异常、回调、多线程、重复加载、显式卸载和进程终止测试；
- COM、SafeHandle、P/Invoke、reverse P/Invoke 和 FXJIT 运行验证。

以上任何一项均不得在本文或主计划中描述为“已通过”。

## 13. 对主文档的输入

以下是本专题对主文档的稳定输入；本轮按用户边界只记录、不修改对应文件。

### 13.1 输入 `04-nativeaot-project-shape.md`

1. 推荐项目属性采用第 2.2 节最小集；全部保持“待目标 .NET 10 SDK验证”。
2. 一个 publish 只设置一个 `RuntimeIdentifier`；首轮 x64 Debug/Release，再 ARM64 Debug/Release；x86 保持 P0 阻塞。
3. `AssemblyName=wpfgfx_cor3` 只是文件名候选控制，实际 DLL/LIB/EXP/PDB 与 loader 身份是 done 门禁。
4. 最小 spike 只含 `WpfGfxShape_NativeAotAbiProbe_v1`，不得注入 106 项 `.def` 或生产 stub。
5. AOT/trim/PInvoke analyzer 不得关闭；初始不得采用旧 `Ilc*`、`DirectPInvoke`、symbol stripping 或体积优化属性。
6. 将 `.def`、`.res`、VERSIONINFO、import library 和 data export 作为独立 linker spike，而不是默认项目能力。

### 13.2 输入 `02-abi-and-managed-callers.md`

1. 每个唯一 EntryPoint 对应至多一个 UCO 薄入口；109 个声明重载不增加 export 数。
2. 增加“双运行时边界”：CoreCLR 与 Native AOT 不共享对象、GCHandle、delegate、exception 或 GC；descriptor token 只作不透明值往返。
3. 增加 `g_fNoMeterChecks` data export、Win32 `.rsrc`、VERSIONINFO 和 import library 兼容门。
4. 把 Native AOT unload/reload 与 x86 支持列为 P0 外部事实门禁。
5. 保留 106/107/99/8/7 清单；未实现生产入口不可导出假成功或批量 `E_NOTIMPL`。

### 13.3 输入 `03-migration-order.md`

1. `WP-00A` 只按构建隔离和脚手架条件判定；`WP-00B` done 必须按第 9.6 节判定 x64 Debug/Release，且只解锁项目/导出机制。
2. x64/ARM64 spike 可先行，但 x86 决策门在大规模生产迁移和完整替换承诺前必须关闭。
3. 批次 0 后新增独立前置：linker/resource/version spike、COM/vtable spike、callback/lifecycle/unload spike；不得把它们塞入一个假闭环。
4. FXJIT 保持批次 5 高风险阻塞，不进入最小 ABI spike，也不得设计替代算法。
5. 每个生产导出只有真实逐文件实现和 ABI 证据就绪后才加入候选 DLL。

### 13.4 输入 `06-testing-strategy.md`

1. 建立动态加载与静态 import 两类 native C/C++ caller；所有 crash/卸载测试在子进程隔离。
2. 发布/验证矩阵采用第 10.1 节六格；每格分别检查 PE、export、资源、符号和调用。
3. x86 增加 stdcall/ESP/未装饰名；x64/ARM64 增加寄存器、栈对齐和 unwind。
4. 增加双运行时 callback、COM apartment/refcount、resource pointer、显式 unload/reload、进程终止和 FXJIT mitigation 测试。
5. 明确第 10.4 节事项不能由托管单元测试单独证明。

### 13.5 输入总体计划与交接

1. 三个 P0 阻塞：Windows x86 Native AOT 支持、Native AOT shared-library unload/reload、COM-like vtable/object 可表达性。
2. P1 阻塞：stdcall/export、双运行时 callback、资源/version/ETW/shader、Debug data export、FXJIT、attach/静态初始化和 trim roots。
3. 下一实现轮只领取最小 probe；不得同时开始生产 ABI、COM 或渲染迁移。
4. 所有本轮结论标注为静态/已知语义/待验证，不得写成已经 build/publish/test。

本轮按用户边界不修改这些主文档，只在本文列出应吸收内容。

## 14. 后续动作

以下动作均不在本轮实施，并按优先级执行：

1. **P0 官方事实核验：**读取与目标 SDK版本匹配的 .NET 10 Native AOT 平台支持、native library、UCO export、COM 和 unload 官方文档；记录文档版本、发布日期和明确支持边界。
2. **P0 x86 裁决：**确认 `win-x86` codegen/runtime pack/shared library 是否存在；若官方不支持，立即把“完整 Win32 等价”升级为用户裁决，不尝试隐藏或猜测 workaround。
3. **构建隔离收口：**先完成 `04a-build-isolation.md`，确保未来项目不继承 WPF Arcade 根构建、输出和包图；本专题属性必须放在该隔离边界内验证。
4. **实施最小 x64 probe：**禁令解除后仅创建第 9 节 probe 和 native caller，先 Debug 后 Release；不创建生产入口。
5. **实施 ARM64 probe：**在真实 ARM64 Windows 上重复完整矩阵；交叉发布和 PE 检查不能替代运行。
6. **若 x86 受支持则实施 x86 probe：**增加 stdcall、ESP、未装饰 export、BOOL/bool 和 callback function pointer 检查。
7. **独立 linker/resource spike：**验证 `.def`、`.res`、VERSIONINFO、ETW、shader resources、DLL/LIB/EXP/PDB、extra code/data export；不与生产逻辑混合。
8. **独立 COM spike：**只用一个受控 IUnknown-like 对象验证 vtable、QI/AddRef/Release、apartment、GC root 和双运行时 caller；不先迁移 factory/media。
9. **独立 callback/lifecycle spike：**覆盖短期同步 callback、长期 descriptor、CoreCLR token、detach、在途调用、线程 join、process exit 和官方支持的 unload/reload。
10. **建立原 DLL真实基线：**逐架构/配置枚举实际 export/import/resource/version/symbol，并复核 106/107/99/8/7 与 `g_fNoMeterChecks`。
11. **FXJIT 前置探针：**在领取批次 5 前验证原实现与候选环境的 code bytes、调用结果、RWX/W^X、CFG、instruction cache、unwind、ARM64 和 mitigation；失败时启动章程例外流程。
12. **回写主文档：**每获得一项官方、构建、二进制或运行证据，即更新本专题及第 13 节列出的主文档；不得只在对话中改变结论。

重新评估触发条件：目标 SDK版本变化、官方支持矩阵变化、AOT linker/export 行为变化、原 wpfgfx ABI/资源变化，或用户明确调整目标架构/卸载兼容范围。
