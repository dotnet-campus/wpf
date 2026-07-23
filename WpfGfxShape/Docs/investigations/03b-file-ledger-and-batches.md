# wpfgfx 逐文件迁移台账与依赖批次调查

> 状态：本轮静态批次基线已完成；精确全文件台账与运行验证留待后续轮次  
> 适用范围：`src/Microsoft.DotNet.Wpf/src/WpfGfx`、其直接构建/生成依赖、ABI 实现位置及必要托管调用边界  
> 输出用途：为 `WpfGfxShape/Docs/03-migration-order.md`、总体计划和后续逐文件实现会话提供可执行输入  
> 最高约束：逐原生文件近似直译；正确性与 ABI 优先；等价完成前禁止重构、优化或合并职责；一个 .NET 生产项目仍镜像原目录  
> 工具边界：本轮绝对禁止命令行、终端、脚本及任何间接命令执行；只使用 IDE 项目枚举、项目文件列表、文件搜索、grep/代码搜索、符号导航、文件读取和 Markdown 文档写入

## 0. 证据与决策标记

本文使用以下前缀，避免把静态事实、迁移排序和未确认事项混为一谈：

- **[事实]**：由已读取源码、项目文件、PCH、生成配置、ABI 声明或既有权威调查直接支持。
- **[排序决策]**：为降低迁移阻塞而提出的批次、门禁或工作包安排；不得反向解释为原生架构无环。
- **[未确认]**：受禁止命令行、未运行构建/测试、条件构建或文件枚举能力限制，尚不能闭环。
- **[风险]**：若未在进入实现前处理，可能造成 ABI、生命周期、布局、生成协议或逐文件追溯失真。

## 1. 范围与方法

### 1.1 调查范围

本专题负责把现有拓扑和 ABI 基线转化为：

1. 可逐项建立、更新和审计的原生文件迁移台账模板；
2. 不遗漏项目项之外头文件、PCH、资源、生成输入/输出和构建文件的完整性策略；
3. 明确区分的调查批次与实现批次；
4. 允许循环依赖先以类型/签名/布局骨架落位、再逐文件填充的实施方法；
5. 每个批次的 readiness、done、测试证据、阻塞升级和禁止重构规则；
6. 跨多轮会话可维持上下文的工作包和交接规范。

本轮不要求人工列出 WpfGfx 树中的每一个文件，但必须定义后续如何完整枚举并证明没有漏掉：

- `.c`、`.cc`、`.cpp`、`.cxx` 等实现文件；
- `.h`、`.hpp`、`.hxx`、`.inl`、`.inc`、`.w` 等声明、内联、协议或生成输入；
- `.rc`、资源模板、HLSL/FX、二进制着色器和其它资源输入/输出；
- `.vcxproj`、`.props`、`.targets`、`.def`、`.dirs` 及条件构建文件；
- 已签入生成输出、构建时生成输出和运输包提供的输入；
- WpfGfx 项目直接编译或包含的 WpfGfx 树外文件。

### 1.2 已读取的基线

- `WpfGfxShape/Docs/00-migration-charter.md`
- `WpfGfxShape/Docs/01-native-source-topology.md`
- `WpfGfxShape/Docs/02-abi-and-managed-callers.md`

### 1.3 调查方法

1. 以 IDE 解决方案项目枚举建立“已加载项目视图”。
2. 以各 `.vcxproj` 文件列表建立编译、资源、项目引用和项目局部生成输入视图。
3. 以公共 `.props`/`.targets`、PCH 和目录级 include 注入补足项目清单不可见依赖。
4. 以文件搜索、grep/代码搜索和符号导航追踪初始化链、ABI 实现位置、生成消费者及源码回边。
5. 把每条结论标为事实、排序决策或未确认，并把未运行验证写入门禁。
6. 只修改本文档；不修改任何 WPF 源码、项目配置或 `WpfGfxShape/Code`。

## 2. 排序原则

1. **正确性、ABI 和生命周期先于“可独立编译”的表面整洁。**
2. **调查批次不等于实现批次。** 调查可按项目/PCH/入口/循环域并行取证；实现必须先建立 ABI、类型、签名和布局承载面，再按原文件逐一填充。
3. **先骨架、后实现，不以重构消除回边。** `resources <-> uce`、`sw <-> hw <-> av` 等循环通过同一项目内的声明骨架承载。
4. **初始化链不是最后的 DLL 包装工作。** DllUtil、进程堆、锁、loader、SW/HW/AV 基础初始化在上层功能前建立，并随批次持续验证。
5. **生成协议与手写实现同等重要。** 生成输入、已签入输出、构建时输出和消费者必须同批记录，不能只迁移可见 `.cpp`。
6. **ABI 实现位置决定横切门禁，不强迫所有导出最后迁移。** 薄导出可先有签名/异常封锁骨架，真实业务实现随原文件批次填充。
7. **纯逻辑叶子可优先取得差分证据，但不能越过其基础类型/布局前置。**
8. **高耦合聚合文件不得因文件小而提前。** `core/dll/dllentry.cpp`、`core/common/milcoredllentry.cpp`、`core/uce/apifunc.cpp`、`core/api/exports.cpp` 等按横切依赖处理。
9. **一次变更只处理一个可说明的迁移单元。** 不顺带清理、现代化、合并职责或改变控制流。

## 3. 完整性策略

### 3.1 枚举必须使用的并列视图

后续完整台账不能由单一文件列表生成，必须并列维护：

| 视图 | 主要来源 | 用途 | 不能单独证明 |
|---|---|---|---|
| 解决方案项目视图 | IDE 项目枚举 | 当前已加载 WpfGfx 项目边界 | 磁盘上未加载/条件/运输包项目 |
| 项目项视图 | `.vcxproj` 与 IDE 项目文件列表 | 编译、资源、项目引用、局部生成项 | 未列入项目的头、公共导入和生成输出 |
| 磁盘文件名视图 | IDE 文件搜索 | 查找项目外头、旁支项目、模板和签入生成物 | 条件是否参与当前构建 |
| include/PCH 视图 | PCH、直接 include、公共 include 路径 | 翻译单元的隐式声明依赖 | 链接成员和运行时可达性 |
| 构建导入视图 | `.props`、`.targets`、目录级配置 | 公共宏、生成、资源传递和条件项 | 实际展开后的最终值 |
| ABI 视图 | `.def`、prototype、定义、托管 P/Invoke | 导出与调用方映射 | 实际二进制导出和运行兼容 |
| 生成协议视图 | 输入、生成器配置、签入/中间输出、消费者 | 保证协议与资源不漏 | 当前环境生成结果是否一致 |

### 3.2 完整枚举闭环

每个原生项目在进入“项目调查完成”前必须完成以下人工闭环：

1. 记录项目路径、配置类型、PCH、所有 `ClCompile`、`ResourceCompile`、`CustomBuild`、`None`、`ProjectReference` 和显式链接输入类别。
2. 用 IDE 文件搜索按该项目目录逐类寻找实现、头、内联、资源、生成输入/输出和局部构建文件。
3. 读取项目 PCH，记录其引入的跨目录公共面；PCH 自身作为独立台账项。
4. 对项目项中每个实现文件记录显式 include，并把未在项目项中的头加入台账。
5. 对 `.props`/`.targets` 注入的文件和目录建立独立台账项，不复制成“项目自有文件”。
6. 对生成文件建立“输入 -> 生成步骤/来源 -> 输出 -> 消费者”四联关系。
7. 对 WpfGfx 树外直接编译/包含文件记录真实原路径，不伪装成 WpfGfx 内文件。
8. 对不存在于本地但由项目引用或运输包替代的输入标记 `Blocked`/外部供应，不虚构源码路径。
9. 在允许自动化的后续轮次用机器清单复核人工台账；本轮禁止脚本，不能完成该复核。

### 3.3 防漏分类

每个项目至少检查以下类别，未发现也要记录“已查未见”，不能留空：

- 实现：C/C++ 源、汇编、内联实现；
- 声明：公共头、私有头、PCH、宏/模板头、IDL/`.w` 类协议输入；
- 生成：生成定义、签入输出、中间输出、工具配置；
- 资源：`.rc`、图标、manifest、HLSL/FX、二进制资源模板；
- 构建：项目、props、targets、dirs、def、项目引用、运输包映射；
- ABI：导出声明/定义、COM 接口、结构/枚举/回调、托管镜像；
- 测试证据：可执行探针、布局、差分、集成和端到端入口。

### 3.4 已加载的 24 个 WpfGfx 项目覆盖矩阵

[事实] 本轮重新调用 IDE 解决方案项目枚举，当前仍显示 **24 个**位于 `src/Microsoft.DotNet.Wpf/src/WpfGfx` 下的已加载项目：`common` 4 个、`core` 17 个、`shared` 3 个。逐项目读取 IDE 文件清单后，当前接口主要返回 `ClCompile` 和少量 `ResourceCompile` 项，**没有返回项目全部头文件**；所以“项目文件清单已读”不能等同于“项目全部文件已枚举”。

| 原生项目 | IDE 项目清单直接可见内容 | 台账完整性要求 |
|---|---|---|
| `common/DynamicCall/DynamicCall.vcxproj` | 仅见 `DelayCall.cpp` | 必须另搜 `DelayCall.h` 等声明；无 PCH 不表示无公共头依赖 |
| `common/effects/effects.vcxproj` | `effectlist.cpp`、`precomp.cpp` | 补录 `effectlist.h`、PCH 和 WGX/COM 依赖 |
| `common/scanop/scanop.vcxproj` | bitmap、halftone、scan pipeline、blend/convert/copy/dither/gamma/quantize、SSE2 等实现和 `precomp.cpp` | 每个实现的私有头、像素结构、SIMD 条件和 PCH 必须另列 |
| `common/shared/shared.vcxproj` | assert、array、engine、ETW、memory、COM、rect、pixel、CPU、refcount、cache、region 等实现和 `precomp.cpp` | `shared.h`/公共声明、宏、GUID、锁/内存和 ABI 基础是批次前置 |
| `core/api/api.vcxproj` | API 基类/brush/codec/factory/light/mesh/render state/shader、`exports.cpp`、`precomp.cpp` | 每个外部入口映射到定义、prototype、托管 EntryPoint、所有权和异常门 |
| `core/av/av.vcxproj` | loader、event、EVR/DXVA、media buffer、state thread、WMP 状态机等实现和 `precomp.cpp` | 另列全部头、WPP 输入/`.tmh` 输出、COM apartment、线程和回调 |
| `core/common/common.vcxproj` | 数学/矩阵、loader、display、DWM/DWrite、memory stream、registry、lifecycle、exports 等实现和 `precomp.cpp` | 特别记录直接编译的 WpfGfx 外 `src/.../Shared/cpp/dwriteloader.cpp` |
| `core/dll/wpfgfx.vcxproj` | `precomp.cpp`、`dllentry.cpp`、`milcore.rc` | 项目虽小却是聚合边界；还必须台账化 `.def`、生成 `.i`、入口符号、项目引用、系统库和资源 |
| `core/control/util/util.vcxproj` | `control.cpp`、`precomp.cpp` | 与 UCE/HW/SW/meta/targets 共享的诊断控制面及旁支 `milctrl` 消费关系必须保留 |
| `core/fxjit/Collector/Collector.vcxproj` | Branch、标量/SIMD 值族、lazy var、jitter access、MMX/XMM 等实现和 `precomp.cpp` | 类型族头、Compiler/Platform 依赖和当前 program 上下文另列 |
| `core/fxjit/Compiler/Compiler.vcxproj` | assemble、dependency graph、flow、locator/mapper、operator、program、shuffle 等实现和 `precomp.cpp` | 指令/操作码头、架构条件、代码缓冲和可执行内存依赖另列 |
| `core/fxjit/PixelShader/PixelShader.vcxproj` | shader parse/translate/register 实现和 `precomp.cpp` | D3D shader token、Collector/Compiler 公共头和生成函数签名另列 |
| `core/fxjit/Platform/Platform.vcxproj` | `Platform.cpp`、`precomp.cpp` | 锁、内存、可执行代码承载和平台 API 是高风险前置，不按“小项目”提前宣告完成 |
| `core/geometry/Geometry.vcxproj` | animation path、area、Bezier、boolean、exact arithmetic、intersection、scanner、shape、stroke、tessellation 等实现和 `precomp.cpp` | 逐文件区分纯算法叶子与 shape/figure/consumer 回边；全部配对头另搜 |
| `core/glyph/glyph.vcxproj` | glyph run core、base painter、debug bitmap、perf、`precomp.cpp` | PCH 对 SW/HW/resources 的回边必须进入循环组，不能按文件数量判定低耦合 |
| `core/hw/hw.vcxproj` | 大量 D3D device/resource/surface/texture/pipeline/shader/glyph/target/fallback 实现、`InteropDeviceBitmap.cpp`、`hw.rc` | 头、HLSL/FX、生成 shader 二进制、资源模板、`hw.res`、SW 回退和设备生命周期必须并列 |
| `core/meta/meta.vcxproj` | desktop/HWND/dummy/bitmap/meta target 与 iterator 实现、`precomp.cpp` | 依赖 targets、SW、HW、AV 的后端组合关系决定其后置 |
| `core/resources/resources.vcxproj` | 大量 2D/3D resource、brush/effect/visual/video、`marshal_generated.cpp`、`renderdata_generated.cpp`、`precomp.cpp` | 全部生成头/协议、UCE resource slave、SW/HW/AV/fxjit 消费关系必须形成循环骨架包 |
| `core/sw/swlib/sw.vcxproj` | AA/coverage、brush span、double buffer、scan render、bitmap/glyph、HWND/surface target、GDI present、rasterizer、`swinit.cpp`、`precomp.cpp` | 另列 `bilinearspan` 外部槽位、scanop/geometry/glyph/resources/fxjit 和 HW 回退关系 |
| `core/targets/targets.vcxproj` | base RT、base surface RT、shape clipper、`precomp.cpp` | brush context/realizer 造成 resources 回边；不能重构为新后端接口层 |
| `core/uce/uce.vcxproj` | API、connection/channel、command batch、composition、handle table、partition/thread、render target、resource slave、generated factory 等实现和 `precomp.cpp` | 与 resources 的类型/协议循环、ABI 导出、线程和命令布局必须先骨架后填充 |
| `shared/debug/DebugLib/DebugLib.vcxproj` | 仅见 `debuglib.cxx` | 无 PCH；仍要补录公共声明、Debug 动态绑定和失败回退语义 |
| `shared/util/DllUtil/DllUtil.vcxproj` | `data.cxx`、`dllmain.cxx`、`dllmainimpl.cxx`、`precomp.cxx` | 自定义入口、CRT 包装、进程终止分支、DebugLib 和进程堆必须作为早期生命周期包 |
| `shared/util/UtilLib/UtilLib.vcxproj` | assert、timer、instrumentation、list、memory、registry、string 等实现和 `pch.cxx` | `Pch.h`、公共宏/声明、分配/锁/错误语义是几乎全栈前置 |

### 3.5 已加载视图之外的项目与供应边界

| 分类 | 路径/标识 | 静态事实 | 台账处理 |
|---|---|---|---|
| 旁支控制 DLL | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/control/dll/milctrl.vcxproj` | 未在当前解决方案枚举中；`DynamicLibrary`，编译 `exports.cpp`、`milctrl.rc`，引用 `core/control/util` | 建立旁支台账，标记“不进入 wpfgfx 聚合”；共享 util 文件仍记录被双重消费 |
| 旁支调试 DLL | `src/Microsoft.DotNet.Wpf/src/WpfGfx/exts/exts.vcxproj` | 未加载；输出 `wpfx`，含 PCH 与调试扩展实现，引用 DbgXHelper、core/common、UtilLib、DebugLib | 与生产迁移批次隔离，但用于证明共享文件不能只按 wpfgfx 调用方理解 |
| 旁支调试静态库 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/DbgXHelper/DbgXHelper.vcxproj` | 未加载；静态库，含 PCH 和 debug/event/flags/input/output/helpers 实现 | 建立旁支项目哨兵项，不计入 wpfgfx 生产 done |
| 缺失/运输包静态库槽位 | `core/sw/bilinearspan/bilinearspan.vcxproj` | `wpfgfx.vcxproj` 有直接引用；当前工作区未找到项目文件，但存在 `core/sw/bilinearspan.h`，公共构建支持运输包替代 | 项目项标记 `Blocked-ExternalSupply`；头与二进制供应分别建账，不得凭头文件伪造实现 |
| WpfGfx 外直接项目 | `src/Microsoft.DotNet.Wpf/src/Shared/OSVersionHelper/OSVersionHelper.vcxproj` | `wpfgfx.vcxproj` 条件二选一路径直接引用 | 作为外部直接依赖建账，调查调用点后决定等价实现/互操作；不改写其原源码 |
| WpfGfx 外直接编译文件 | `src/Microsoft.DotNet.Wpf/src/Shared/cpp/dwriteloader.cpp` | 直接列入 `core/common` 项目清单 | 保留真实来源路径与单独 Ledger ID，不复制后冒充 `core/common` 自有文件 |

[排序决策] 主生产实现批次只以 24 个已加载生产项目、`bilinearspan` 供应槽位、OSVersionHelper 和直接编译共享文件为闭环；`milctrl/wpfx/DbgXHelper` 作为旁支调查域，不进入 wpfgfx 的完成分母，但任何共享文件的行为变更都要考虑旁支消费者。

### 3.6 项目级完成哨兵

每个 `.vcxproj` 必须建立一个 `File kind=Build` 的项目哨兵台账项。只有其实现、头/PCH、生成、资源、ABI、外部供应和测试类别均已“已查有项”或“已查未见”，项目哨兵才能从 `Investigated` 进入后续状态。项目哨兵不能代替文件状态，也不能因项目所有 `.cpp` 已翻译就标记 `Accepted`。

### 3.7 `wgx_core_types` / `wgx_av_types` 成对输出闭环

#### 3.7.1 已确认生成与消费链

| 角色 | 核心类型 | AV 类型 | 证据与含义 |
|---|---|---|---|
| 历史 schema/model | `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_core_types.w` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_av_types.w` | 两个 `.w` 同时含 `CSHARP_ONLY`、`CPP_ONLY`、`COMMON` 段，是成对输出的模型输入 |
| 历史项目 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/GraphicsInclude.nativeproj` | 同左 | `SchemaFile` 明确列出两个 `.w`；`GenerateCodeFromSchemas` 预期输出同名 `.h/.cs` |
| 历史生成器 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/gencode.pl` | 同左 | 读取 `-template/-cppfile/-csfile`；COMMON 段原样写 C++ 并翻译到 C#，支持 `;include_header` 聚合其它生成片段 |
| 当前签入原生输出 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.h` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_av_types.h` | 当前原生构建的有效静态输入；不是本轮生成结果 |
| 当前签入 processed C# 输出 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.cs` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_av_types.cs` | 与 `.h` 成对保存在 processed 目录；当前 `Graphics.props` 也复制它们，但 PresentationCore 不直接编译该路径 |
| 当前托管编译副本 | `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_core_types.cs` | `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_av_types.cs` | `PresentationCore.csproj` 明确直接编译这两个 Common/Graphics 文件 |
| 当前原生复制步骤 | `Graphics.props::CopyGeneratedIncludeFilesToIntermediateFolder` | 同左 | 四个 processed `.h/.cs` 在 Build 前复制到各项目 `IntermediateOutputPath`，后者进入 include 路径 |

[事实] `include/processed/README.txt` 明确写明：这些文件“应由 Build Task 生成并从中间目录消费”，当前签入版本是从 **NetFxDev1 build intermediate outputs** 复制而来。当前 `Graphics.props` 没有调用 `gencode.pl`，只复制这四个签入文件。

[事实] `GraphicsInclude.nativeproj` 依赖旧 `$(_NTDRIVE)$(_NTROOT)` 内部 targets，并通过 `Exec` 调 Perl；它没有出现在当前解决方案项目枚举中。本轮只能确认历史生成意图，不能确认当前仓库可直接重现该项目。

[风险] 当前复制动作带 `!Exists(destination)` 条件，而不是无条件覆盖；若中间目录已有旧副本，静态配置允许跳过复制。真实 clean/incremental build 是否可能消费陈旧版本必须后续用构建证据验证。

#### 3.7.2 核心类型并非单文件孤立输出

`wgx_core_types.w` 的 C# 段聚合：

- `wgx_core_types_compat.cs`；
- `Generated/wgx_misc.cs`；
- `Generated/wgx_command_types.cs`；
- `Generated/wgx_resource_types.cs`。

其 C++ 段聚合：

- `wgx_sdk_version.h`；
- `Generated/wgx_misc.h`；
- `Generated/wgx_resource_types.h`；
- `Generated/wgx_command_types.h`；
- `Generated/wgx_commands.h`；
- `Generated/wgx_renderdata_commands.h`。

并且 C++ 协议部分位于 `#pragma pack(push, 1)` / `#pragma pack(pop)` 之间。因此，台账中的 `wgx_core_types` 必须是一个**生成族**而非四个孤立文件：schema、compat 输入、MilCodeGen 片段、历史生成器、processed 输出、Common/Graphics C# 副本、原生消费者和 PresentationCore 消费者都要互相链接。

[事实] `wgx_core_types.w`/processed 原生输出定义 `HMIL_OBJECT/HMIL_RESOURCE/HMIL_CHANNEL` 为 32 位值，同时定义指针宽度 `MIL_CHANNEL/HMIL_CONNECTION`；这直接支撑“句柄类型不可统一抽象”的 ABI 门禁。

[事实] `gencode.pl` 包含显式低层翻译规则，例如 `HMIL_OBJECT/HMIL_RESOURCE -> UInt32`、`BOOL -> Int32`、`WMG_HANDLE64 -> UInt64`、指针 -> `IntPtr`。这些规则是历史输出语义的一部分，迁移时不能用 C# 默认类型直觉替代。

[事实] `wgx_core_types_compat.cs` 自身警告其中很多类型与 unmanaged 生成类型同步；它既是 schema 聚合输入，也是潜在人工维护漂移点，必须有独立 Ledger ID。

#### 3.7.3 AV 成对输出的有意非对称

- `wgx_av_types.w` 的 `COMMON` 段定义 `AVEvent`，所以 `.h` 与 `.cs` 都有数值 0-13 的枚举。
- `AVEventData` 位于 `CPP_ONLY` 段，包含四个 32 位头字段和 `WCHAR[1]` 变长尾，并显式 `align(4)`；所以 processed `.h` 有该结构，而 C# 输出只有枚举。
- 该非对称是 schema 明确表达的生成规则，**不能**因 C# 文件缺少 `AVEventData` 就判定生成不完整；托管侧按字节包解析和对应布局测试另行建账。

#### 3.7.4 台账组与门禁

为防止成对输出漂移，建议为每个生成族增加稳定组 ID：

- `GENPAIR-CORE-TYPES`：上述核心 schema、聚合输入、`.h/.cs` 输出与消费者；
- `GENPAIR-AV-TYPES`：AV schema、`.h/.cs` 输出、`AVEventData` 原生消费者与托管包解析消费者。

组内每项仍是独立 Ledger 行。组的 readiness 需要：

1. 当前有效原生输出与当前托管编译副本均已登记；
2. 所有被 `;include_header` 聚合的文件均有台账项；
3. enum 值、字段顺序、pack/align、固定宽度和句柄身份有验证计划；
4. 明确“冻结当前消费输出”还是“先恢复可重现生成器”的阶段决策；在生成器可重现性未证明前不得重新生成并替换基线；
5. `Common/Graphics/*.cs` 与 `include/processed/*.cs` 的全文一致性由后续允许的自动差分复核。本轮只做了代表区段人工核对，**未证明逐字一致**。

[排序决策] `GENPAIR-CORE-TYPES` 必须横跨实现批次 0、1、7：批次 0 建立冻结清单和测试承载，批次 1 落位句柄/错误/基础布局，批次 7 才收口完整命令/资源协议族。`GENPAIR-AV-TYPES` 的枚举/packet 布局也在批次 1/7 前置，而 AV 行为实现留在批次 11。

### 3.8 MIL 命令协议生成族

#### 3.8.1 模型、生成器、输出和消费者

建议稳定组 ID：`GENPROTO-MILCMD`。

| 角色 | 路径/文件 | 静态事实 |
|---|---|---|
| 权威模型候选 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/xml/Resource.xml` | `<Commands>`、Resources 和 `<RenderDataInstructions>` 共同定义命令名、域、字段、payload、managed/unmanaged 可见性和资源关系 |
| 模型 schema | `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/xml/Resource.xsd` | 约束 `MilTypes` 下 Commands/Resources 等模型形态 |
| 生成入口 | `codegen/mcg/main/ResourceGenerator.cs`、`main/Resources.rsp` | ResourceGenerator 反射实例化全部 `GeneratorBase` 子类并调用 `Go()`；rsp 同时列入 CommandType、CommandStructure、CommandProcessMessage、DuceResource、RenderData、ResourceFactory 等生成器 |
| 历史执行包装 | `codegen/mcg/tools/GenerateFiles.cmd` | 静态命令显示以 `Resource.xml`、`Resource.xsd` 和仓库根输出目录运行 MilCodeGen；本轮禁止执行该命令 |
| 命令 ID 生成器 | `codegen/mcg/generators/CommandType.cs` | 从 `PaddedCommandCollection` 依次生成 `wgx_command_types.h/.cs`，并生成旁支调试表 `exts/cmdstruct.h` |
| 命令结构生成器 | `codegen/mcg/generators/CommandStructure.cs` | 生成 `Include/Generated/wgx_commands.h/.cs`；C# 结构使用 `Explicit, Pack=1`，`BOOL` 映射为 `UInt32` |
| UCE dispatch 生成器 | `codegen/mcg/generators/commandprocessmessage.cs` | 生成 `core/uce/generated_process_message.inl`，执行命令大小检查、payload 切分、资源类型验证、transport 限制和 handler 路由 |
| render-data 生成器 | `codegen/mcg/generators/renderdata.cs` | 同一模型生成 `wgx_renderdata_commands.h`、`core/resources/renderdata_generated.cpp` 以及 PresentationCore 的 Drawing/RenderData 相关文件 |
| 当前原生聚合输出 | `include/processed/wgx_core_types.h` | 通过 `wgx_core_types.w` 聚合命令 enum、结构、render-data 结构，并在外层 `#pragma pack(push, 1)` 下消费 |
| 当前托管命令结构 | `src/Microsoft.DotNet.Wpf/src/Common/Graphics/Generated/wgx_commands.cs` | `PresentationCore.csproj` 直接编译；由托管代码填写并发送命令 |
| 原生服务端消费者 | `core/uce/composition.cpp` + `generated_process_message.inl`、resources/UCE 各 `Process*` | 解码、校验并路由到 CComposition 或具体 DUCE resource |
| 代表托管消费者 | `PresentationCore/.../D3DImage.cs`、`PresentationCore/.../Generated/RenderData.cs` | 分别发送固定结构命令和带 payload 的 render-data 命令 |
| 生成清单证据 | `eng/WpfArcadeSdk/tools/WPF_Generated_Files.txt` | 同时列出 Common/Graphics 托管副本和 WpfGfx `Include/Generated` 命令输出；应作为完整性视图之一 |

[未确认] `CommandStructure.cs` 当前可见代码直接写 `WpfGfx/Include/Generated/wgx_commands.cs`，而 PresentationCore 编译 `Common/Graphics/Generated/wgx_commands.cs`。本轮未在当前 props/targets/生成器代码中定位两者之间的复制/发布步骤；两者都被生成文件清单列出。后续必须把“两个独立生成输出、旧复制步骤或人工同步副本”裁决为书面事实，不能假定其中任一自动跟随另一份。

#### 3.8.2 编号和布局不变量

- [事实] 当前签入 `MILCMD` 的 retail 范围从 `MilCmdInvalid=0x00` 到 `MilCmdBitmapCache=0x8d`；Debug 额外在末尾放置 `MilCmdValidateStructureOrder=0x8e`。
- [事实] `CommandType.cs` 明确警告 Debug sentinel 必须保持列表末尾，否则 debugger extension 以及 managed/unmanaged enum ID 会失配。
- [事实] 命令 ID 顺序不是按单个 C++ 文件决定，而是由 `PaddedCommandCollection` 汇总普通命令、RenderData 命令/指令和隐式 resource 命令后顺序生成。
- [事实] C# 命令结构使用 `LayoutKind.Explicit, Pack=1` 与显式 `FieldOffset`；C++ `wgx_commands.h` 自身不声明 pack，而由 `wgx_core_types.w` 在包含生成头时施加外层 pack(1)。台账必须记录这种**环境式 pack 依赖**。
- [事实] `composition.cpp` 说明命令记录用 pack(1) 以跨架构/TS 通信，并要求每个 batch record 的总大小仍为 4 字节倍数；不能把 pack(1) 简化成“任意紧凑字节串”。
- [事实] `Boolean`/`BOOL` 命令字段在托管结构中是 `UInt32`；`ResourceHandle/HMIL_RESOURCE/HMIL_CHANNEL` 是 32 位协议值。
- [事实] D3DImage、media 等模型中部分 pointer/handle 槽显式建模为 `UInt64`，例如 `pInteropDeviceBitmap`、`pSoftwareBitmap`、`hEvent`、`pMedia`；即使 x86 运行也不能改成 32 位 `IntPtr` 字段。
- [事实] 可变 payload 命令以固定头结构后跟原始字节，dispatch 仅能先验证 `cbSize >= sizeof(header)`，再计算 `cbPayload`；固定命令要求精确大小。
- [事实] `generated_process_message.inl` 的默认分支和多类验证失败返回 `WGXERR_UCE_MALFORMEDPACKET`；transport 被拒绝时按模型选择失败或 no-op。迁移不得只实现 switch 路由而漏掉这些错误语义。

#### 3.8.3 有意非对称和不可漏项

- `CommandType` 的 C# 与 C++ enum 包含完整 ID 集合；但 `CommandStructure` 对 `UnmanagedOnly=true` 的命令不生成托管结构。
- 例如 transport/resource lifetime 的若干命令存在原生结构和 enum ID，但不应因为 Common `wgx_commands.cs` 缺失同名结构而判定输出损坏。
- RenderData 指令有特殊生成路径：原生普通命令结构含 `MILCMD Type`，托管 inline render-data 指令并非都按同一方式直接 marshal；`renderdata.cs` 同时生成客户端流写入与原生 `renderdata_generated.cpp` 解析。
- `exts/cmdstruct.h` 虽不进入 wpfgfx 生产 DLL，却按同一命令顺序生成调试描述。它是编号/结构顺序的旁路一致性证据，不能从生成台账中删除。
- `core/uce/generated_process_message.inl` 不出现在 IDE `uce.vcxproj` 的编译文件清单中，但被 `composition.cpp` 文本包含，是必须登记的生成输出。

#### 3.8.4 实现批次门禁

[排序决策] 批次 7 的命令协议工作不是“复制几个 struct”，而是以下同步工作包：

1. 冻结 `Resource.xml`/`Resource.xsd`、相关生成器和当前签入输出的来源清单；
2. 建立完整 `MILCMD` 数值、命令头、Pack/offset/size、payload 和 fixed-width 字段 C# 镜像；
3. 建立与原 `generated_process_message.inl` 一一对应的校验/路由骨架，handler 可仍为批次 8 的未填充原签名方法；
4. 同步登记 `wgx_renderdata_commands.h`、`renderdata_generated.cpp`、PresentationCore RenderData 生产方和资源消费者；
5. 保持 managed/unmanaged-only 的输出非对称，不为“统一模型”擅自生成新托管 API；
6. 在命令结构与 dispatch 骨架稳定前，不开始 resources/UCE 大规模方法体翻译。

批次 7 最小证据必须包括：

- 每个 retail/debug 命令 ID 对照；
- x86/x64/ARM64 每个可跨边界命令的 size/offset/pack 对照；
- 固定 64 位槽、BOOL、resource handle 和 enum 宽度断言；
- 至少一个固定命令、一个带 resource handle 命令、一个固定 UInt64 槽命令、一个 payload 命令的原生/C# 字节差分；
- 无效 command ID、错误固定大小、截断 payload、错误 resource 类型和 transport-denied 的失败码差分；
- `generated_process_message` 的每个 ID 均有明确 handler、显式跳过理由或条件配置说明。

批次 7 禁止：重新编号、压缩命令、改变字段顺序、把固定 `UInt64` 改成平台指针、把 `BOOL` 改为 C# `bool`、用通用序列化框架替换原字节协议、把 dispatch 重构成新插件/消息总线、删除 unmanaged-only 或当前托管未发送的命令。

### 3.9 DUCE resources / UCE 生成闭环

建议稳定组 ID：`GENPROTO-DUCE-RESOURCES`。该组与 `GENPROTO-MILCMD` 共享 `Resource.xml`/ResourceModel，但用途不同：前者定义资源类型、服务器对象、字段数据、更新/通知和创建，后者定义 wire command ID、结构与路由。

#### 3.9.1 生成输出矩阵

| 生成器/来源 | 生成输出 | 直接消费者 | 迁移含义 |
|---|---|---|---|
| `ResourceType.cs` + `ResourceModel.Resources` | `Include/Generated/wgx_resource_types.h/.cs` | `wgx_core_types.w` 聚合；托管 `DUCE.ResourceType`；UCE handle/resource factory | 资源 ID 是协议常量，不按实现顺序重排 |
| `DuceResource.cs` | `core/resources/resources_generated.h` | `core/resources/resources.h` 及所有 resource 头 | 只含必要 resource class 前向声明，原生成器明确用它承载循环依赖 |
| `DuceResource.cs` | `core/resources/data_generated.h` | 各 resource 头中的 `*_Data m_data` | 字段、数组长度、resource 指针和 BOOL/枚举布局必须与 command/model 同步 |
| `DuceResource.cs` | `core/resources/marshal_generated.cpp` | 各 concrete DUCE resource 类型 | 集中实现 `ProcessUpdate`、payload/handle 校验、Register/UnRegister、动画同步和循环资源 `GetResource` |
| `ResourceFactory.cs` | `core/uce/generated_resource_factory.h/.cpp` | `uce.h`、`htslave.cpp` | `MIL_RESOURCE_TYPE -> concrete class/constructor` 的生成 dispatch，连接 UCE handle table 与 resources 类型 |
| `CommandProcessMessage.cs` | `core/uce/generated_process_message.inl` | `composition.cpp` | `MILCMD -> CComposition/具体 resource Process*` 的校验和路由 |
| `RenderData.cs` generator | `Include/Generated/wgx_renderdata_commands.h`、`core/resources/renderdata_generated.cpp`、多份 PresentationCore RenderData 文件 | `CMilSlaveRenderData`、托管 Drawing/RenderData producer | 内层绘制指令流、handle 替换、push/pop 平衡和 malformed packet 规则 |

[事实] `resources.vcxproj` 直接编译 `marshal_generated.cpp`、`renderdata_generated.cpp`，但 `resources_generated.h`、`data_generated.h` 只通过 `resources.h` include 进入；`uce.vcxproj` 直接编译 `generated_resource_factory.cpp`，而 `generated_resource_factory.h` 和 `generated_process_message.inl` 只通过 include 进入。仅枚举 `ClCompile` 会漏掉半个生成闭环。

[事实] `eng/WpfArcadeSdk/tools/WPF_Generated_Files.txt` 已列出 WpfGfx `Include/Generated` 族和 Common/Graphics 托管输出，但本轮对该清单的文本搜索未见上述 `core/resources`/`core/uce` 生成文件。因此该清单也不能单独作为 WpfGfx 全部生成输出清单。

#### 3.9.2 原生文件边界是“手写类型 + 集中生成实现”

代表关系：

- `core/resources/axisanglerotation3d.h` 声明 concrete class、构造/析构、`ProcessUpdate`、Register/UnRegister、动画同步和 `CMilAxisAngleRotation3DDuce_Data m_data`；
- `data_generated.h` 定义该 `*_Data` 结构；
- `marshal_generated.cpp` 实现 `ProcessUpdate`、Register/UnRegister 和动画同步；
- `axisanglerotation3d.cpp` 只实现析构与几何 realization 算法；
- `generated_resource_factory.cpp` 根据 `TYPE_AXISANGLEROTATION3D` 调用受保护构造函数。

因此“一个原生实现文件对应一个主要 C# 文件”仍成立，但 resource 类型的完整行为分散于手写 `.cpp`、手写 `.h`、`data_generated.h`、`marshal_generated.cpp`、命令结构和 factory。逐文件台账必须同时保留两种追溯：

1. **物理文件追溯**：每个原生文件独立 Ledger 行与 C# 镜像；
2. **逻辑类型族追溯**：同一 concrete resource 的声明、数据、生成更新实现、手写算法、命令和 factory case 以稳定 Type Family ID 相连。

不得把 `marshal_generated.cpp` 的数千行方法分散复制进各 C# 资源文件后失去原文件追溯；若为了 C# partial 类型表达将方法落在按资源命名的 partial 文件中，也必须保留一个 `marshal_generated.cpp` 镜像/索引文件或等价映射清单，逐方法指出原偏移和目标文件，且该拆分只服务于语言承载，不构成职责重构。

#### 3.9.3 编译循环与运行时资源图循环必须区分

**编译期循环：** `DuceResource.cs` 的注释直接说明：data declarations 引用 resource 类，而每个 resource 类又需要自己的 data struct，因此生成 `resources_generated.h` 前向声明集合来打破头文件定义循环。这是原生系统自身采用的“先声明骨架、后完整类型”模式，直接支持本迁移的骨架策略。

**运行时图循环：** `ResourceModel.cs` 对引用 resource 的 `CanIntroduceCycles` 默认值是 `true`；未显式写 `false` 的相关资源会：

- 由 `ResourceFactory` 选择接收 `CMilSlaveHandleTable*` 的构造函数；
- 继承/组合 `CMilCyclicResourceListEntry` 并注册到 handle table；
- 由 `DuceResource` 生成 `GetResource()`；
- 在 `CMilSlaveHandleTable::BreakLinksForCyclicResources` 中按原 AddRef -> UnRegisterNotifiers -> NotifyOnChanged -> Release 顺序断开引用环。

已确认的代表性 cyclic resource 包括 MaterialGroup、Transform3DGroup、TransformGroup、GeometryGroup、CombinedGeometry、VisualBrush、BitmapCacheBrush、DrawingGroup。该集合后续必须由模型全量复核，不能只依赖代表项。

[风险] 将这些 C# 对象交给 GC 而删除显式 notifier unregister、cycle list 或 shutdown 断链，会改变可观察生命周期、通知和释放顺序；初次迁移禁止这样做。

#### 3.9.4 资源创建与失败顺序

[事实] `CMilSlaveHandleTable::CreateEmptyResource` 的顺序为：

1. 在指定 32 位 handle 处分配 handle entry；
2. 调 `CResourceFactory::Create`；
3. 调 resource `Initialize()`；
4. 把对象写入 handle table；
5. 发 ETW create event；
6. 转移引用给 out 参数。

失败时仅在已分配 entry 时删除 handle，并释放尚未转移的 resource。`generated_resource_factory.cpp` 对未知 type 返回 `WGXERR_UCE_MALFORMEDPACKET`，new 失败走 OOM，成功后 AddRef 再写 out。该顺序必须由批次 8 保留，不能用字典 `GetOrAdd` 或统一对象工厂改写。

#### 3.9.5 marshal / render-data 关键语义

- `marshal_generated.cpp` 在更新前先 `UnRegisterNotifiers`，解析基础字段、resource handle、动画 handle 和 payload 数组，再 RegisterNotifiers；失败清理再次 unregister，随后仍按生成代码调用 `NotifyOnChanged(this)`。
- resource 数组要求 payload 长度不超过剩余字节且可被 32 位 `HMIL_RESOURCE` 大小整除；每个 handle 按预期 `MIL_RESOURCE_TYPE` 验证，null 是否允许由模型控制。
- 非 handle 数组按元素大小整除，PathGeometry 等复杂 payload 有专门后续校验；不得统一成 `MemoryMarshal.Cast` 后跳过原检查顺序。
- `renderdata_generated.cpp` 逐内层 command 验证最小大小，将 resource handles 替换为已保活对象索引，并验证 push/pop 最终平衡；未知 ID、截断结构或栈不平衡均是 malformed packet。
- render-data 对 malformed guideline 有特殊容错：特定错误允许保存 null guideline kit；这类分支不得以“更严格验证”删除。

#### 3.9.6 Channel tables 的分类裁决

[事实] `core/uce/clientchanneltables.{h,cpp}` 与 `serverchanneltables.{h,cpp}` 没有自动生成文件头；在已搜索的 MilCodeGen 源中也未找到生成输出关系。它们是**手写 UCE handle-table 实现**，不是 generated protocol 输出。

- client table 有锁、事件 handle、master entry 复制和 channel 计数；
- server table 管理 source handle、server channel、composition device 和 Release 顺序；
- 二者放入实现批次 8 的 UCE 基础子批，而不是批次 7 的生成输出冻结子批。

#### 3.9.7 `resources/UCE` 最小骨架包

[排序决策] 批次 8 开始真实方法体前，至少同时建立以下原名骨架：

1. `MIL_RESOURCE_TYPE`/`DUCE.ResourceType`、所有 concrete resource 类型名和继承关系；
2. `CMilSlaveResource`、notifier 基础、`CMilCyclicResourceListEntry`、handle table、`CComposition` 和 `CResourceFactory` 签名；
3. `resources_generated.h` 对应的前向/类型声明承载；
4. `data_generated.h` 对应的全部 `*_Data` 字段骨架；
5. 各 resource 头声明的构造、`ProcessUpdate`、Register/UnRegister、动画同步、`GetResource` 和 `m_data`；
6. 命令结构与 `generated_process_message` 路由签名；
7. factory 的 type -> concrete constructor 映射骨架；
8. client/server channel table 和 resource handle table 的原身份类型。

允许骨架互相引用和使用 partial 类型；不允许创建新的“统一 IResource/消息处理器/依赖注入工厂”来消除回边。骨架方法若尚未实现必须显式阻塞，不返回假 `S_OK`。

#### 3.9.8 其它已发现生成哨兵

完整文件台账还必须单列下列生成族，不能因不属于 resources/UCE 而遗漏：

- `wgx_sdk_version.h/.cs` 与 `ProtocolFingerprint.cs`：批次 0/1 的版本 ABI 门；
- `Include/Generated/wgx_render_types_generated.h` 与 `MilRenderTypesGenerated.cs`：基础 render 类型/布局，批次 1/7；
- `Include/Generated/wincodec_private_generated.h` 与 `WincodecPrivateGenerated.cs`：WIC/API 边界，批次 12 前置；
- `core/hw/Shaders.h`、`Shaders.rc` 和 shader 生成/程序集资源：批次 10；
- `exts/cmdstruct.h`：旁支调试输出，不进入生产完成分母但参与命令编号一致性。

## 4. 逐文件台账模板

> 本节为模板；后续精确全文件台账可拆分为机器可读清单和按批 Markdown 视图，但字段语义不得缩水。

| 字段 | 必填规则 |
|---|---|
| Ledger ID | 稳定 ID；建议由原项目短名 + 原相对路径生成，重命名时保留旧 ID |
| Native project | 原 `.vcxproj` 或“公共/外部/生成供应者” |
| Native path | 仓库根相对路径；大小写按仓库实际拼写 |
| File kind | Implementation/Header/PCH/Inline/GeneratedInput/GeneratedOutput/Resource/Build/ABI/TestEvidence/External |
| Build participation | 直接编译、include、资源输入、生成输入、生成输出、链接输入、条件项、旁支、不进入 wpfgfx、未确认 |
| Original responsibility | 文件原职责；禁止以拟议重构后的职责描述 |
| Key symbols | 代表类型、函数、全局、导出、资源 ID 或生成记录 |
| Type family ID | 跨手写头/实现与集中生成实现的逻辑类型族；不能替代物理文件 Ledger ID |
| Symbol implementation locations | 某类型的方法分别位于哪些手写/生成文件；集中生成文件必须逐方法可追溯 |
| PCH/direct includes | PCH 归属、显式 include、公共注入来源 |
| Intra-project dependencies | 同原项目直接依赖 |
| Cross-project dependencies | 跨 WpfGfx 项目/目录依赖及方向 |
| Platform/external dependencies | Win32/COM/D3D/WIC/DWrite/AV/GDI/CRT/运输包等 |
| Generated relation | 生成输入、生成器/协议、输出、消费者及是否签入 |
| Generated group ID | 成对/成族输出稳定标识，如 `GENPAIR-CORE-TYPES`；组状态不替代文件状态 |
| Source authority | 当前消费基线、历史 schema、历史生成器、签入冻结输出、派生副本或中间产物 |
| ABI/layout sensitivity | 导出名、调用约定、结构、pack/align、bool/BOOL、句柄、COM、回调 |
| Lifecycle/concurrency sensitivity | attach/detach、初始化、锁、线程、引用计数、释放和失败路径 |
| Cycle group | 无或稳定循环组 ID，例如 `CYCLE-RES-UCE`、`CYCLE-SW-HW-AV` |
| Investigation batch | 静态调查工作包编号 |
| Implementation batch | 实现批次编号；可与调查批次不同 |
| Proposed C# path | 按第 5 节映射规则产生 |
| Scaffold allowance | 允许先创建的声明/布局/薄入口；不得含伪业务实现 |
| Required co-migration | 必须同步迁移的头/实现/生成输出/配对释放文件 |
| Status | 严格使用章程状态机 |
| Readiness blockers | 未满足的前置事实、类型、ABI、生成或测试门 |
| Verification evidence | 单元、布局、ABI、差分、集成、端到端证据链接 |
| Known deviations | C# 无法逐字表达处、原因、风险和验证方法 |
| Owner/session | 当前工作包/会话标识 |
| Last reviewed | 人工更新时间或轮次标识 |
| Notes/decision links | 决策、风险、例外和交接文档链接 |

### 4.1 路径映射初始规则

- 原生实现文件原则上对应一个主要 C# 实现文件。
- `src/Microsoft.DotNet.Wpf/src/WpfGfx/<relative-dir>/<name>.<cpp|cxx|c>` 映射到 `WpfGfxShape/Code/<relative-dir>/<name>.cs`。
- 保留 `common`、`core`、`shared` 及 `fxjit/*`、`control/util`、`sw/swlib`、`util/*` 等层级。
- 头文件不强制一头一 `.cs`，但其类型/常量/布局若拆到多个 C# 文件，台账必须列出完整映射；不得借拆分重组职责。
- 同名不同扩展文件必须保持可辨识关系；若 C# 文件名冲突，使用保留原扩展语义的稳定后缀并记录例外。
- 生成输出放在镜像 `Generated`/原生成目录语义下，不与手写实现混合。
- 同一生成族的模型、生成器和输出不得折叠为一个无来源的手写 C# 文件；若先冻结输出，路径和文件边界仍镜像当前实际消费者。
- WpfGfx 树外直接依赖保留来源边界，未经书面决策不得复制后宣称为本模块自有实现。

## 5. 状态、门禁与阻塞升级

### 5.1 文件状态机

严格沿用章程：

`NotInvestigated -> Investigated -> Scaffolded -> Translated -> UnitVerified -> DifferentialVerified -> Integrated -> EndToEndVerified -> Accepted`

`Blocked` 可在任一尚未完成状态出现，但解除后应回到有证据支持的原状态，不得跳级。

补充约束：

- `Investigated`：职责、依赖、ABI/生命周期敏感点、拟议路径、测试点和未解析项均已记录。
- `Scaffolded`：只有承载循环或 ABI 所需的类型、签名、布局、常量、函数指针和薄转发声明；不得返回虚假成功模拟完成。
- `Translated`：已按原文件近似直译，但没有足够验证证据；不能称“完成”。
- `Accepted`：该文件要求的布局/ABI/单元/差分/集成/端到端证据均满足，且无未记录偏差。

### 5.2 批次 readiness 定义

一个实现批次只有在以下条件全部满足时才可开始：

1. 批内文件已至少 `Investigated`，原路径和 C# 路径已锁定。
2. 直接依赖已标为已实现、允许骨架或显式阻塞；不存在隐式“稍后再看”。
3. 需要的 ABI 类型、结构布局、枚举宽度、句柄身份和调用约定已有验证计划。
4. 生成输入/输出和资源消费者已列入同批或明确前置批。
5. 初始化、关闭、失败路径和锁顺序已记录。
6. 批次最小测试/差分证据在编码前定义。
7. 本批禁止的重构已在工作包中原文列出。

### 5.3 批次 done 定义

一个实现批次只有在以下条件全部满足时才可标记 done：

1. 批内所有目标文件至少达到该批规定的状态，不以“能编译”代替验证。
2. 原文件、C# 文件、同步头/生成输出和测试证据映射完整。
3. 没有未登记的新跨批依赖、布局偏差、异常映射或生命周期差异。
4. 批次定义的 ABI/布局/单元/差分/集成证据通过；本轮未运行的项目明确保持未验证。
5. `next-session-handoff.md` 已更新完成项、剩余阻塞、下一唯一目标和修改文件列表。
6. 评审确认没有重构、优化、职责合并、算法替换或无关清理。

### 5.4 阻塞升级规则

- 文件内可解决、且不改变批次边界的缺失声明：记录后在当前工作包补齐。
- 发现未台账化的直接依赖：当前文件转 `Blocked`，新增依赖项并评估是否属于前置骨架。
- 发现 ABI/布局/调用约定不确定：升级为批次门禁；不得猜测类型继续实现。
- 发现生成来源或运输包输入缺失：升级到主迁移顺序/项目形态专题，不得手写替代并静默继续。
- 发现需要重构才能“解决”的循环：拒绝该方案，改用声明骨架；若 C# 表达确实不可行，形成书面例外请求。
- 阻塞跨两个以上后续批次或影响导出/初始化链：必须同步写入 `next-session-handoff.md` 和主计划风险区。

## 6. 调查批次与实现批次

### 6.1 两类批次的区别

- **调查批次**按证据获取效率组织，可围绕项目清单、PCH、符号族、初始化链、ABI 家族或循环域；目标是把文件推进到 `Investigated`。
- **实现批次**按可承载依赖和可验证闭环组织；先建立全局类型/签名/布局骨架，再逐文件填充，不能照搬调查阅读顺序。
- 同一文件可在早期调查批次被完整调查，却在较晚实现批次才翻译；高耦合文件通常如此。

### 6.2 实现批次总表（静态基线）

> 历史说明：本节及第 6.3 节形成于批次 0 被细拆为 `WP-00A...WP-00I` 之前，只保留“批次 0 总体最终能力”的历史摘要。当前可执行工作包边界和 Done 条件以 `../migration-master-plan.md`、`../03-migration-order.md` 第 14 节及 `../next-session-handoff.md` 为准；尤其 `WP-00A` 不执行 Native AOT publish、PE/export 检查或 native call，这些属于 `WP-00B`。

| 批次 | 名称 | 核心范围 | 排序性质 |
|---:|---|---|---|
| 0 | 构建/ABI 骨架 | 单项目目录镜像、基础 ABI 类型、导出清单与布局测试承载 | 前置骨架，不翻译渲染算法 |
| 1 | 基础类型和错误码 | 公共标量、HRESULT/WGX 错误、枚举、句柄身份、结构布局 | 全栈前置 |
| 2 | shared util / DLL 生命周期 | UtilLib、DebugLib、DllUtil、进程堆/入口语义 | attach 前置 |
| 3 | common/shared / DynamicCall | COM/refcount、内存、锁、CPU、像素基础、动态调用 | 公共运行基础 |
| 4 | geometry / scanop / effects | 纯逻辑优先的几何、像素管线与效果列表 | 首批可差分算法 |
| 5 | fxjit | Platform、Compiler、Collector、PixelShader | 软件 ShaderEffect 前置 |
| 6 | core/common / control / targets / glyph | 核心数学、loader、控制、RT 基类、glyph 基础 | 上层渲染承载 |
| 7 | generated protocol | processed types、命令、资源类型、布局和生成映射 | resources/UCE 强前置 |
| 8 | resources / UCE | 类型骨架先行，随后资源、channel、composition、generated factory | 核心循环域 |
| 9 | SW | 软件光栅、scan pipeline、软件目标、GDI present | 首个可控渲染后端 |
| 10 | HW | D3D9 设备/资源/管线/着色器/回退 | SW 与资源前置 |
| 11 | AV | 状态线程、WMP/EVR/DXVA、媒体 surface 与回调 | HW/SW/resources/UCE 交叉后置 |
| 12 | meta / API | 后端选择组合、factory/WIC/外部对象桥接 | 依赖主要实现域 |
| 13 | DLL / export | 完整薄导出、资源、入口和二进制契约收口 | 不是首次处理初始化；是最终收口 |
| 14 | 端到端整合 | PresentationCore 真实调用、差分、失败与生命周期 | 最终验收带 |

> [排序决策] 当前基线为 **15 个实现批次（0-14）**。后续可把超大循环域拆成连续子批，但不得重排已确认前置、合并职责或减少逐文件可追溯性。批次号是实施/门禁标签，不是新的架构分层。

### 6.3 逐批详细表

| 批次 | 原生范围与代表文件 | 直接前置 | 允许的骨架/同步项 | 最小验证证据 | 批次完成条件 | 本批特别禁止 |
|---:|---|---|---|---|---|---|
| 0 | 新独立项目承载；`core/dll/wpfgfx.def`、当前 DLL 名/导出清单、托管 P/Invoke 清单；不翻译渲染算法 | 章程、拓扑、ABI 文档 | 单一 .NET 10 Native AOT 项目目录镜像；测试项目；薄导出样例；ABI 清单/布局测试数据结构 | **批次 0 总体能力摘要（非单一工作包）**：隔离项目构建，并由后续 `WP-00B` 的最小 C caller 按未装饰名调用受控导出、验证异常不越界和产物身份 | `WP-00A...WP-00I` 各自 Done；x64 最小闭环属于 `WP-00B`，x86/ARM64 及其它机制分别有独立门禁；WPF 仓库未被修改 | 不创建“大一统服务层”；不批量加入 106 个返回 `S_OK` 的假导出；不开始算法翻译 |
| 1 | `include/processed/wgx_core_types.*`、`wgx_av_types.*`、`Include/Generated` 基础 enum/struct、`Common/Graphics` 对端、`wgx_error.cs` | 批 0 测试承载 | HRESULT/WGX 错误、固定宽度标量、`BOOL`/C++ `bool`、GUID/RECT、三类 handle、descriptor/packet/command 基础布局 | native/managed `sizeof/offsetof` 对照；enum/error 常量；x86/x64/ARM64 位宽；错误版本 `MilVersionCheck` | 所有上层所需基础类型至少 `UnitVerified`/布局验证；不确定类型转 `Blocked` | 不把所有 handle 包成一个类型；不把固定 `UInt64` 改为 `IntPtr`；不“规范化”历史 P/Invoke 元数据 |
| 2 | `shared/util/UtilLib/*`、`shared/debug/DebugLib/debuglib.cxx`、`shared/util/DllUtil/{data,dllmain,dllmainimpl}.cxx` | 批 1；Native AOT 卸载探针 | 原分配/断言/字符串/注册表/计时器签名；attach/detach 状态；DebugLib stub/binding；进程堆和显式卸载承载 | 初始化/显式卸载/进程终止对照；分配失败；DebugLib 缺失回退；关闭顺序事件日志 | 必需 UtilLib 文件逐文件翻译并验证；Native AOT 无法表达的 DllMain/CRT 差异形成书面决策 | 不以一个 `IDisposable` 合并 loader、CRT、用户 DllMain 和进程退出；不补写“更合理”回滚 |
| 3 | `common/shared/*`、`common/DynamicCall/{DelayCall.*}`；代表 `milcom.cpp`、`refcountbase.cpp`、`pixelformatutils.cpp`、`resourcecache.cpp` | 批 1-2 | COM/refcount、内存/数组/区域/像素基础、锁、CPU feature、动态模块/函数指针缓存 | AddRef/Release/QI；缓存/region/像素纯逻辑；动态加载成功/缺失/SEH 失败；锁与释放顺序 | 上层公共依赖已落位；每个全局/缓存/COM owner 有释放证据 | 不用 GC 生命周期替代 refcount；不改变 cache 策略、CPU/SIMD 选择或锁粒度 |
| 4 | `core/geometry/*`、`common/scanop/*`、`common/effects/effectlist.*`；优先 `ExactArithmetic.cpp`、`BezierFlattener.cpp`、`LineSegmentIntersection.cpp`、`soconvert.cpp`、`soblend.cpp` | 批 1、3；必要 core/common 数学骨架 | 原头/类型签名；纯算法文件可 2-5 个一组；SIMD 路径只按原条件落位 | 大量边界/随机差分；NaN/Inf/溢出/退化几何；像素逐字节；effect COM 引用 | 目标叶子达到 `DifferentialVerified`；非叶子只在依赖满足后推进 | 不改变浮点精度、容差、舍入、颜色空间、SIMD 分支或算法控制流 |
| 5 | `core/fxjit/{Platform,Compiler,Collector,PixelShader}`；代表 `Platform.cpp`、`Assemble.cpp`、`DependencyGraph.cpp`、`jitteraccess.cpp`、`pshader.cpp` | 批 1、3；部分批 4 像素类型 | 原操作码/value/program/代码缓冲签名；全局 jitter lock；可执行内存能力探针 | IR/operator 图差分；shader token 解析；生成代码输出/调用；W^X/CFG/架构；释放与失败注入 | 平台可行性有证据；若 Native AOT 限制阻塞原 JIT，形成用户批准的局部例外，不静默换算法 | 不先用表达式树、SIMD intrinsics 或新 shader 编译器重写；不删除 x86/x64 汇编路径 |
| 6 | `core/common/*`、`core/control/util/*`、`core/targets/*`、`core/glyph/*`；代表 `BaseMatrix.cpp`、`matrix.cpp`、loader/display/renderoptions、`control.cpp`、`basert.cpp`、`glyphruncore.cpp` | 批 1-5 中相应基础 | core/common 数学与 loader 分开逐文件；targets/resources 和 glyph/backends 仅建必要循环骨架；OSVersionHelper/dwriteloader 保留来源 | 数学差分；loader 缺失/惰性加载；RenderOptions BOOL/锁；target 层栈；glyph 数据/outline 局部测试 | 上层资源/后端需要的公共行为可用；生命周期参与者接入批 2 的 attach 测试 | 不把 loader 统一成高层服务；不消除 targets/resources、glyph/backends 回边；不提前迁移聚合 `engine.cpp` 全行为 |
| 7 | `GENPAIR-CORE-TYPES`、`GENPAIR-AV-TYPES`、MilCodeGen command/resource/render-data、`generated_process_message.inl`/factory/marshal 的签名面 | 批 0-1；批 3/6 的 handle/resource base 骨架 | 冻结当前消费输出；镜像 command/resource enum、显式布局、router/factory/`ProcessUpdate` 签名；模型/emitter/消费者台账 | 全 command/resource ID；结构 size/offset；字节流 round-trip；payload；SDK fingerprint；Common/processed 副本差分 | 生成协议达到布局/字节级验证；generator 漂移已记录；resources/UCE 批 readiness 通过 | 不运行 generator 后直接覆盖基线；不重排 XML/ID/字段；不用自定义序列化替换协议 |
| 8 | `core/resources/* <-> core/uce/*`；代表 `marshal_generated.cpp`、`renderdata_generated.cpp`、`generated_resource_factory.*`、`generated_process_message.inl`、`ht*`、`connection/clientchannel`、`composition/partition*`、`apifunc.cpp` | 批 1-7 | `CYCLE-RES-UCE` 多文件类型/签名骨架；随后按原文件逐一填充；薄导出随真实实现启用 | handle create/duplicate/release；batch begin/append/end；malformed packet；same/cross-thread；partition zombie；资源 update/依赖图；失败顺序 | 至少最小 connection→channel→resource→command→message 闭环 `Integrated`；全批可拆连续子批 | 不用 actor/message bus 重写线程模型；不合并资源类；不让未实现命令返回成功；不修复现有部分初始化顺序 |
| 9 | `core/sw/swlib/*` + `bilinearspan` 供应；代表 rasterizer、scanpipelinerender、swrast、doublebufferedbitmap、swpresentgdi、`swinit.cpp` | 批 4-8；fxjit 对 ShaderEffect 路径 | 软件 surface/RT/brush/glyph/scan pipeline 文件逐一落位；HW/AV 只保留已登记接口骨架 | 确定性像素 golden/differential；dirty rect；DPI/stride/pixel format；GDI present；资源释放；软件强制模式 | 可通过最小 UCE/RenderTargetBitmap 走纯软件路径生成一致像素；`bilinearspan` 方案有证据 | 不用 System.Drawing/Skia/高层图形库替代；不改变光栅、AA、混合或双缓冲算法 |
| 10 | `core/hw/*`、shader FX/二进制/资源、InteropDeviceBitmap；代表 `d3ddevice.cpp`、`HwPipeline.cpp`、`d3dswapchain.cpp`、`hwinit.cpp` | 批 3-9；Silk.NET 低层互操作验证 | D3D9/D3D9Ex COM 接口、结构、HRESULT、resource owner；SW fallback 接口保持；shader 资源按原 ID | adapter/device caps；create/reset/lost device；resource AddRef/Release；shader resource；HW/SW fallback；D3DImage callback/detach | 最小硬件 bitmap/HWND 路径集成，设备丢失和显式释放有差分证据 | 不迁到 D3D11/12；不以 Silk.NET 高层封装改变 COM 生命周期；不移除 fallback/设备丢失分支 |
| 11 | `core/av/*`；代表 `milav.cpp`、`StateThread.cpp`、`WmpPlayer.cpp`、`EvrPresenter.cpp`、event/media proxy、DXVA wrapper | 批 1-3、6-10 | COM apartment/event/state thread、descriptor、EVR/DXVA/WMP 接口逐文件；media surface 接到既有 HW/SW 骨架 | open/stop/close/shutdown/process-exit 区分；event packet；callback thread；超时缓存；scrubbing/bool；shutdown reentry | 媒体对象生命周期、事件和至少受控媒体帧路径 `Integrated`；退出路径有证据 | 不用 MediaPlayer/MediaFoundation 高层 API重写状态机；不合并三类结束动作；不改变线程/超时语义 |
| 12 | `core/meta/*`、`core/api/*`；代表 `metart.cpp`、desktop/HWND/bitmap targets、`api_factory.cpp`、`exports.cpp`、codec/WIC wrappers | 批 4、6、8-11 | 后端组合、factory/COM/WIC/stream/media wrappers 按原文件；导出仍是薄转发 | factory/COM refcount；WIC bitmap/codec；stream descriptor；SW/HW target selection；meta iterator | 主要 COM/API 对象可由现有 ABI 创建/释放；后端选择与原生差分 | 不把 `core/api/exports.cpp` 变成业务层；不合并 SW/HW/meta；不改变 getter/create 所有权 |
| 13 | `core/common/{engine,milcoredllentry}.cpp`、`core/dll/dllentry.cpp`、`wpfgfx.def`、`milcore.rc`、完整 106 导出 | 批 2-12 | 最终 attach/startup/shutdown 串接；完整薄导出；资源/文件名/调用约定；8/7 差异显式状态 | 实际导出表、x86 stdcall/未装饰名、x64/ARM64；attach/detach；异常封锁；资源；全 ABI smoke/differential | 候选 DLL 可作为二进制替代；导出、布局、所有权和生命周期门禁通过 | 不在导出层放算法；不靠删导出/改 P/Invoke 消除差异；不把进程终止当显式卸载 |
| 14 | 真实 PresentationCore + 新 DLL；软件、硬件、媒体和生命周期分层 E2E | 批 0-13 | 测试/部署切换与基线捕获，不再增加迁移实现范围 | 首个 `DrawingVisual -> RenderTargetBitmap -> pixels`；HWND present；D3DImage；media；多 Dispatcher；失败/退出；性能仅记录 | 规定平台/架构/配置的端到端矩阵通过，所有文件状态和偏差台账收口到 `Accepted` 门槛 | 不在验收期顺带重构/优化；不以单一 happy path 宣称整体完成 |

### 6.4 批次拆分规则

- 批次 4、5、8、9、10、11 可拆为 `4A/4B` 等连续子批；拆分只缩小工作包，不改变前置关系。
- 批次 8 必须至少拆为“循环骨架”“channel/handle”“resource update”“composition/partition”“ABI family”若干连续工作包；不得一次提交整个目录。
- 批次 13 的导出收口不表示所有 106 个入口在同一轮实现；入口可随真实实现逐批启用，但只有该批统一验证完整二进制契约。
- 若叶子文件经调查发现隐含平台、全局状态或循环依赖，应移到依赖满足的后续子批，而不是通过重构维持原排序。

## 7. 循环依赖处理

### 7.1 处理原则

- 先在镜像目录中建立原类型名、字段、签名、枚举、函数指针和必要前向关系。
- 骨架只为编译和布局承载服务，不引入新抽象接口替换原关系。
- 随后按原文件逐一填充方法体；每次填充更新台账直接依赖和未解析依赖。
- 对需要同批存在的配对类型，允许一个“循环骨架工作包”同时创建多个空壳文件，但每个文件仍有独立台账项。
- 不因 C# 同项目可互相引用，就省略原生 include/PCH 依赖记录。

### 7.2 已知循环组

- `CYCLE-RES-UCE`：`core/resources <-> core/uce`。
- `CYCLE-SW-HW-AV`：`core/sw <-> core/hw <-> core/av`，并与 resources/UCE 交叉。
- `CYCLE-GLYPH-BACKENDS`：glyph 与 SW/HW/resources 的实现可见性回边。
- `CYCLE-TARGETS-RESOURCES`：targets 使用 brush context/realizer，而资源与后端依赖 target 基类。
- `CYCLE-API-ALL`：API 聚合创建逻辑可见几乎全部主要模块；不作为低层实现前置。

## 8. 初始化链与 ABI 横切

### 8.1 初始化不是末批才开始

[事实] 原生入口从 DllUtil/CRT 包装进入 `core/dll/dllentry.cpp`，再进入 `core/common/milcoredllentry.cpp`；attach 中交叉调用 AV 锁、UCE 全局锁、RenderOptions、core/common loader、SW jitter 锁和 HW device manager。

[排序决策] 因此：

- 批次 0-3 就建立 attach/detach、显式卸载/进程终止差异和异常封锁的测试承载；
- 批次 5-11 每引入一个初始化参与者，就补齐其真实启动/关闭实现和失败路径；
- 批次 13 只做最终入口/导出/资源收口，不得到该批才首次实现生命周期。

### 8.2 ABI 横切规则

- `.def` 名称、原生定义/prototype、托管 EntryPoint、未来实际二进制导出四份清单并列维护。
- 每个导出记录真实实现文件；实现文件未迁移前，薄导出只允许显式未实现/阻塞策略，不能伪造成功。
- x86 stdcall、未装饰导出名、x64/ARM64 平台 ABI 分架构验证。
- 32 位 DUCE 句柄、指针宽度 connection/channel、COM/native 指针分别建模。
- generated command、descriptor、AVEventData、回调和固定 64 位协议槽在上层实现前完成布局门。
- 所有异常在 ABI/reverse-P/Invoke 边界内封锁并按原 HRESULT/BOOL/void 语义处理。

## 9. 每轮工作包规范

### 9.1 推荐粒度

初始规则，后续按实际复杂度校准：

- 纯逻辑叶子：每轮 **2-5 个紧密相关实现文件**，连同直接头和测试；不得跨两个算法域。
- 普通状态文件：每轮 **1-3 个实现文件**，必须包含配对头、所有权/失败路径和差分测试。
- ABI、初始化、线程、COM、生成协议或循环枢纽：每轮 **1 个主实现文件或 1 个明确骨架包**。
- 超大文件：按原文件内可独立审计的连续职责段分轮，但不拆成新的长期架构职责；台账仍以原文件为主项并记录子工作包。
- 生成族：一个工作包包含“一组共同生成源 + 对应输出 + 至少一个消费者”，不能只抄输出。

### 9.2 每轮输入

每个会话工作包必须明确：

1. 唯一主目标和批次编号；
2. 原生主文件、配对头、生成/资源输入和拟议 C# 路径；
3. 直接前置及其当前状态；
4. 允许创建的骨架边界；
5. 必须保持的 ABI、布局、锁、引用和失败顺序；
6. 最小测试/差分证据；
7. 明确范围外事项；
8. 禁止重构、优化、职责合并和无关清理的提醒。

### 9.3 `next-session-handoff.md` 更新规则

每轮结束必须按章程 12 项格式更新，并额外包含：

- 本轮涉及的 Ledger ID 和状态变化；
- 新发现的直接依赖、循环组和阻塞；
- 已创建/修改的原生到 C# 路径映射；
- 已运行与未运行的证据，禁止把未运行写成通过；
- 下一轮可直接领取的一个工作包，不同时留下多个同优先级主目标；
- 下一轮结束时应回写哪些台账行、批次 readiness/done 和测试证据。

## 10. 变更规模与评审规则

- 一个提交/评审单元只对应一个工作包；不顺带修复邻近问题。
- 默认上限由“文件语义”而非行数决定：纯逻辑 2-5 个实现文件，状态/ABI 1-3 个，高风险枢纽 1 个。
- 同一工作包中的 C# 骨架文件可以多于实现文件，但必须全部是直接依赖且有独立 Ledger ID。
- 评审按原文件并排核对：控制流、错误顺序、锁顺序、引用计数、释放顺序、常量/枚举、条件编译和未实现分支。
- 任何重命名、抽象提取、公共 helper 合并、缓存/并发变化都视为额外设计，迁移期默认拒绝。
- 评审发现范围外清理时应拆回，不以“更整洁”为合并理由。

## 11. 可优先叶子与不可提前高耦合文件

### 11.1 候选纯逻辑叶子

下列是**优先调查候选**，不是无条件可迁移清单：

- `core/geometry` 中精确/区间算术、Bezier 展平、线段相交等少平台状态算法；
- `common/scanop` 中纯像素格式、颜色转换、alpha/混合和量化叶子；
- `core/common` 中部分矩阵、矩形和纯数学文件；
- `fxjit` 中不触及可执行内存/全局上下文的局部图或值逻辑。

代表性首轮候选包括 `core/geometry/ExactArithmetic.cpp`、`BezierFlattener.cpp`、`LineSegmentIntersection.cpp`，`common/shared/pixelformatutils.cpp`，以及 `common/scanop/soconvert.cpp`/`soblend.cpp` 中可独立证明的纯函数段。候选不等于可立即实现；必须先确认其头、错误码、结构布局、SIMD/浮点和条件宏前置。若一个原文件同时包含高耦合状态，不得为了抢跑把它永久重构成新职责；只能按原文件记录受控子工作包。

### 11.2 不可提前迁移的高耦合文件/族

- `core/common/milcoredllentry.cpp`：全局 attach/detach 汇聚。
- `core/common/engine.cpp`：CPU/注册表/loader/display/DWrite/AV 初始化汇聚。
- `core/dll/dllentry.cpp` 与 DllUtil：真实 loader/CRT 生命周期外壳。
- `core/uce/apifunc.cpp`、`composition.cpp`、`partition*`：ABI、channel、线程和资源中心。
- `core/resources/marshal_generated.cpp`、`renderdata_generated.cpp`：生成协议与大规模资源布局中心。
- `core/api/exports.cpp`：COM、WIC、media、stream、bitmap 等外部桥接聚合。
- `core/hw/d3ddevice.cpp`、`HwPipeline.cpp`、InteropDeviceBitmap：设备、资源、回调、回退和线程高风险。
- `core/av/StateThread.cpp`、`WmpPlayer.cpp`、`EvrPresenter.cpp`：COM apartment、线程、回调和媒体状态机。

## 12. 风险与未决问题

- 解决方案已加载项目不等于磁盘或条件构建中的全部 WpfGfx 项目。
- `bilinearspan` 本地源码缺失/运输包替代边界尚未闭环。
- 项目文件清单主要覆盖编译/资源项，头、内联、生成模板和公共导入可能遗漏。
- `resources/UCE` 与 `SW/HW/AV` 循环的最小骨架集合尚未逐类型确定。
- 全部 106 个 `.def` 名称尚未逐入口完成签名、所有权和测试映射。
- ABI 的 8/7 静态差异项尚未由实际二进制与运行行为裁决。
- Native AOT 下 DllMain 等价生命周期、显式卸载与进程终止策略仍需专题验证。
- FXJIT 可执行内存、生成代码和平台限制可能要求书面例外，但当前不得预先改写算法。

## 13. 因禁止脚本未完成的精确枚举与未验证事项

### 13.1 未完成的精确全文件枚举

本轮明确没有、也不得声称已经完成：

- 全部 WpfGfx 目录的精确文件总数及按扩展名统计；
- 每个项目全部实现、头、内联、资源、生成、构建文件的机器级闭包；
- 全部条件配置/架构展开后的项目项集合；
- 全量 include 图、符号依赖图和源码强连通分量；
- 运输包中 `bilinearspan` 等原生输入的实际文件/成员清单；
- 构建后生成的 `.tmh`、shader 二进制、`.res`、预处理 `.def` 等精确输出集合。

原因：用户与章程绝对禁止 PowerShell/cmd/bash/Python/dotnet/msbuild、统计脚本及任何间接命令执行；IDE 枚举和人工搜索适合建立策略与静态证据，但不能诚实替代机器闭包证明。

### 13.2 尚未运行验证

- 未构建任何原生或 C# 项目。
- 未运行布局、ABI、单元、差分、集成或端到端测试。
- 未枚举真实 DLL 导出/导入、名称装饰、资源或静态库成员。
- 未验证实际初始化失败、显式卸载、进程退出、回调线程或设备丢失行为。
- 未验证生成输出与签入版本一致。

## 14. 对主迁移顺序文档的输入

`WpfGfxShape/Docs/03-migration-order.md` 应至少吸收：

1. 第 6.2 节的 15 批初始实现顺序及后续证据调整理由；
2. 调查批次与实现批次分离的原则；
3. 第 4 节完整 Ledger 字段和第 5 节状态/readiness/done 门禁；
4. 第 7 节“先类型/签名/布局骨架，再逐文件填充，禁止用新接口消环”；
5. 初始化链从早期批次持续推进、DLL/export 仅最终收口的规则；
6. 生成协议、PCH、资源和构建文件必须进入台账的完整性要求；
7. 第 9-10 节会话工作包、变更规模和评审规则；
8. 本轮未完成精确枚举和所有运行验证的明确限制。

## 15. 后续动作

1. 下一轮只领取批次 0：创建隔离项目骨架、测试承载和最小导出闭环，不开始批次 1 或算法翻译。
2. 在允许自动化后建立完整机器可读 Ledger，并用项目项、磁盘、include/PCH、构建导入、ABI 和生成视图交叉复核。
3. 为批次 1 建立首批具体 Ledger 行：core/AV types、错误码、句柄、descriptor 和 command header。
4. 为每一后续会话从第 6.3 节派生唯一工作包，并在结束前更新状态、证据和 `next-session-handoff.md`。
5. 任何改变批次前置或需要书面例外的发现先记录证据，再更新主迁移顺序与总体计划。

## 16. 过程更新记录

- 已读取 `00-migration-charter.md`、`01-native-source-topology.md`、`02-abi-and-managed-callers.md`。
- 已创建本文档首版骨架，先落盘调查范围、证据标签、完整性闭环、台账字段、状态/门禁、15 批初稿、循环处理、初始化/ABI 横切、会话工作包和未验证边界。
- 已补齐 `.w`/processed 成对输出、生成族字段和冻结基线门禁。
- 已把 0-14 共 15 个实现批次细化为原生范围、前置、允许骨架、同步迁移项、最小证据、完成条件与禁止事项。
- 已确认下一轮唯一可领取范围是批次 0；精确全文件台账和运行验证仍未完成。
