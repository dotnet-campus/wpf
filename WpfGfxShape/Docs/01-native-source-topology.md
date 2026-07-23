# wpfgfx 原生源码与项目拓扑调查

> 状态：本轮静态拓扑基线已完成；后续实现轮次按新证据增量维护  
> 调查日期：当前工作区调查轮次；允许使用的 IDE 工具未提供本地日历日期，具体日期待人工补录  
> 轮次说明：本轮仅调查 `src/Microsoft.DotNet.Wpf/src/WpfGfx` 原生项目拓扑、构建聚合、共享基础设施、生成协议和直接依赖，不创建或修改生产代码  
> 约束来源：`WpfGfxShape/Docs/00-migration-charter.md`

## 1. 调查范围与方法

### 1.1 范围

- 解决方案中位于 `src/Microsoft.DotNet.Wpf/src/WpfGfx` 下的全部 `.vcxproj`。
- `WpfGfx/common`、`WpfGfx/core`、`WpfGfx/shared` 及其子系统目录。
- `wpfgfx.vcxproj` 的编译、项目引用、对象聚合与最终链接边界。
- 公共 props/targets/dirs、预编译头、共享头、生成文件协议、资源文件，以及项目清单之外可能存在的静态依赖。
- WpfGfx 之外的直接原生依赖、构建依赖和候选平台依赖。
- 对“最终一个 .NET 10 生产项目，但目录镜像原边界”的约束输入。

### 1.2 方法与工具限制

本轮**未使用且不得使用任何命令行、终端或脚本**，包括 PowerShell、cmd、bash、Python、批处理、构建脚本，以及通过命令间接进行搜索、统计、读取、构建或测试。调查只使用 IDE/工作区提供的：

- 解决方案项目枚举；
- 项目文件清单；
- 文件名搜索；
- 代码/文本搜索与 grep；
- 符号导航；
- 指定范围文件读取；
- 本文档创建与写入。

因此，本文区分：

- **已确认事实**：由已读取文件或 IDE 项目枚举直接支持；
- **推断**：由命名、引用或多份静态证据综合得出，附置信度；
- **候选依赖**：存在引用迹象，但尚未确认是否进入最终 `wpfgfx` 构建/运行边界；
- **尚未验证**：需要构建日志、预处理输出、链接命令、二进制导出或精确脚本统计才能确认。

### 1.3 迁移原则边界

依据 `WpfGfxShape/Docs/00-migration-charter.md`：

- 初次迁移必须逐原生文件近似直译，并保持原路径到 C# 路径可追溯。
- 正确性、ABI、二进制布局、调用约定、生命周期和失败路径优先。
- 最终可统一为一个 .NET 10 Native AOT 生产项目，但目录必须镜像 `common`、`core`、`shared` 及原子系统边界。
- 等价迁移完成前禁止重构、职责合并、算法改写、并发模型变化或提前优化。
- 本专题只提供拓扑与后续盘点输入，不越权形成最终总体迁移计划。

## 2. 已读取的关键文件

| 文件 | 本轮用途 | 状态 |
|---|---|---|
| `WpfGfxShape/Docs/00-migration-charter.md` | 确认调查边界、工具禁令、逐文件直译、ABI 优先和文档最低要求 | 已读取全文 |
| 解决方案项目枚举（IDE 视图） | 初始识别全部 WpfGfx `.vcxproj` 及 WpfGfx 外候选依赖项目 | 已枚举；不是磁盘文件 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.vcxproj` | 确认 DLL 类型、目标名、直接链接库、延迟加载、入口点、项目引用和导出生成入口 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/{common,core,shared}/**/*.vcxproj` | 确认当前解决方案 24 个项目的类型、PCH 与编译项清单 | 已读取项目文件或通过 IDE 读取项目文件清单；职责细读继续进行 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/Graphics.props` | 确认全局图形 include 路径、DirectXMath/D3D9/D3DCompiler 链接和已处理生成头复制协议 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/Graphics.Paths.props` | 确认 `Include`、`Shared/inc` 路径变量 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/Directory.Build.Props` | 确认 WpfGfx 子树先继承仓库根属性，再统一导入 `Graphics.props` | 已读取全文 |
| `Directory.Build.props`、`Directory.Build.targets` | 确认 WPF Arcade SDK props/targets 进入所有项目的根导入链 | 已读取全文 |
| `eng/WpfArcadeSdk/tools/Wpf.Cpp.props` | 确认 Windows SDK、架构宏、x86 默认 stdcall、CRT/VCRT 和通用编译/链接属性 | 已读取全文 |
| `eng/WpfArcadeSdk/tools/GenerateModuleDefinitionFile.targets` | 确认 `.def` 通过 C++ 预处理生成中间 `.i` 后供链接器使用 | 已读取全文 |
| `eng/WpfArcadeSdk/tools/WpfProjectReference.targets` | 确认本地项目/运输包替代机制及 `bilinearspan` 项目映射 | 已读取全文 |
| `eng/WpfArcadeSdk/tools/FolderPaths.props`、`eng/WpfArcadeSdk/Sdk/Sdk.props` | 确认 `WpfGraphicsPath`、`WpfCppProps`、导出生成 targets 的来源 | 已读取相关全文/区段 |
| `eng/WpfArcadeSdk/tools/ShippingProjects.props` | 确认 `wpfgfx` 是外部运输项目，`bilinearspan` 是内部运输静态库项目 | 已读取相关区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/av.targets` | 确认 AV 项目以 Windows SDK TraceWPP 生成每实现文件对应的 `.tmh` | 已读取全文；未执行生成 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/hw/ShaderAssemblies.targets` | 确认 HLSL/FX 输入、`fxc.exe`、`.vsbin`/`.psbin` 输出及嵌入资源模板复制 | 已读取全文；未执行生成 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/control/dll/milctrl.vcxproj` | 识别 WpfGfx 范围内但未加载到当前解决方案的独立控制 DLL | 已读取全文；不属于 `wpfgfx` 聚合引用 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/exts/exts.vcxproj`、`DbgXHelper/DbgXHelper.vcxproj` | 识别 WpfGfx 调试扩展边界 | 已读取全文；不属于 `wpfgfx` 聚合引用 |
| `eng/WpfArcadeSdk/Sdk/Sdk.targets`、`eng/WpfArcadeSdk/tools/SdkReferences.targets`、`eng/WpfArcadeSdk/tools/Wpf.Cpp.targets`、`eng/WpfArcadeSdk/tools/ResourceLinking.targets` | 确认公共 targets 导入顺序、运输包原生库解析、资源跨静态库传递意图和原生版本资源生成 | 已读取相关全文/区段；未展开 Visual C++ 安装目录内 targets |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.def`、`milcore.rc`、`core/hw/hw.rc` | 确认导出清单输入、最终 DLL 资源和硬件着色器资源入口 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/{dll,common,sw/swlib,hw,av}/precomp.hpp`、`common/shared/precomp.hpp`、`shared/util/{DllUtil/Precomp.h,UtilLib/Pch.h}` | 识别项目局部 PCH 暴露的隐式模块依赖和 DLL 生命周期基础入口 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/dllentry.cpp`、`core/common/milcoredllentry.cpp`、`core/common/engine.cpp`、`core/sw/swlib/swinit.cpp`、`core/hw/hwinit.cpp`、`core/av/milav.cpp` | 确认用户 DLL 入口和 MilCore/引擎/软件/硬件/媒体初始化与关闭顺序 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/shared/util/DllUtil/{dllmain.cxx,dllmainimpl.cxx,data.cxx}`、`shared/util/UtilLib/MemUtils.cxx`、`shared/debug/DebugLib/debuglib.cxx` | 追踪链接器入口到 CRT、用户 `DllMain`、进程堆和 DebugLib 的外层生命周期 | 已读取相关全文/区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/{StateThread.cpp,avloader.cpp,eventproxy.cpp}`、`core/common/{d3dloader.cpp,dwritefactory.cpp,display.cpp,renderoptions.cpp}`、`core/hw/d3ddevicemanager.cpp`、`core/resources/ShaderEffect.h`、`core/fxjit/Platform/Platform.cpp` | 展开指定初始化函数的实际动作、惰性加载、锁和释放行为 | 已读取相关区段 |

> 后续每读取一批关键项目、构建、入口、预编译头、生成或资源文件，均在此表追加。

## 3. 项目/目录拓扑表

### 3.1 解决方案中已确认的 WpfGfx 原生项目

IDE 解决方案项目枚举直接显示 **24 个**位于 `src/Microsoft.DotNet.Wpf/src/WpfGfx` 下的 `.vcxproj`。此数字只表示当前解决方案枚举结果，不等同于磁盘上所有可能存在但未加载的项目文件。

| 大边界 | 子系统/项目 | 项目路径 | 初步职责 | 代表性源文件/规模 |
|---|---|---|---|---|
| `common` | DynamicCall | `src/Microsoft.DotNet.Wpf/src/WpfGfx/common/DynamicCall/DynamicCall.vcxproj` | 无 PCH 的延迟/动态调用薄层静态库 | **小**；IDE 项目项仅见 `DelayCall.cpp`；不代表全部磁盘头文件 |
| `common` | effects | `src/Microsoft.DotNet.Wpf/src/WpfGfx/common/effects/effects.vcxproj` | 效果列表/效果公共实现静态库 | **小**；`effectlist.cpp`、`precomp.cpp`；头文件未精确统计 |
| `common` | scanop | `src/Microsoft.DotNet.Wpf/src/WpfGfx/common/scanop/scanop.vcxproj` | 像素扫描操作、转换、混合、抖动、量化与扫描管线静态库 | **中**；代表：`scanpipeline.cpp`、`scanpipelinebuilder.cpp`、`soblend_sse2.cpp`、`soconvert.cpp`、`systembitmap.cpp`；头文件未精确统计 |
| `common` | shared | `src/Microsoft.DotNet.Wpf/src/WpfGfx/common/shared/shared.vcxproj` | 跨图形子系统共享的内存、COM、矩形、像素格式、引用计数、缓存、处理器特性与 ETW 基础静态库 | **中**；代表：`milcom.cpp`、`refcountbase.cpp`、`resourcecache.cpp`、`pixelformatutils.cpp`、`etwtrace.cpp`；头文件未精确统计 |
| `core` | api | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/api.vcxproj` | 外部 MIL/WGX API 工厂、画刷、编解码、着色器、3D 网格/光照与导出实现静态库 | **中**；代表：`exports.cpp`、`api_factory.cpp`、`api_codecfactory.cpp`、`api_shader.cpp`；头文件未精确统计 |
| `core` | av | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/av.vcxproj` | 媒体播放、EVR 呈现、DXVA、样本调度与状态线程静态库 | **大**；代表：`EvrPresenter.cpp`、`dxvamanagerwrapper.cpp`、`WmpPlayer.cpp`、`StateThread.cpp`、`SampleScheduler.cpp`；总文件未精确统计 |
| `core` | common | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/common/common.vcxproj` | 核心数学、矩阵、显示、D3D/DWM/DWrite 加载、内存流、资源池、兼容性与公共 DLL 生命周期静态库 | **大**；代表：`D3DLoader.cpp`、`DWMInterop.cpp`、`dwritefactory.cpp`、`matrix.cpp`、`milcoredllentry.cpp`；并直接编译 WpfGfx 外 `src/Microsoft.DotNet.Wpf/src/Shared/cpp/dwriteloader.cpp`；总文件未精确统计 |
| `core` | dll / wpfgfx | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.vcxproj` | **已确认最终动态库聚合项目**，目标名 `wpfgfx$(WpfVersionSuffix)` | 自身源码规模**小**但拓扑影响**超大**；项目项为 `precomp.cpp`、`dllentry.cpp`、`milcore.rc`，其余生产实现由项目引用汇聚；最终对象数未精确统计 |
| `core` | control/util | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/control/util/util.vcxproj` | 控制/诊断工具公共实现静态库；同时被独立 `milctrl.vcxproj` 引用 | **小**；`control.cpp`、`precomp.cpp`；头文件未精确统计 |
| `core` | fxjit/Collector | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/fxjit/Collector/Collector.vcxproj` | 建模 JIT 中间值、分支、惰性变量及标量/SIMD 类型操作的收集层 | **中**；代表：`Branch.cpp`、`MmValue.cpp`、`XmmValue.cpp`、`jitteraccess.cpp`、`C_f32x4.cpp`；头文件未精确统计 |
| `core` | fxjit/Compiler | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/fxjit/Compiler/Compiler.vcxproj` | JIT 运算符图、流控制、寄存器/内存映射、调度和机器码组装层 | **中**；代表：`Assemble.cpp`、`DependencyGraph.cpp`、`FlowControl.cpp`、`Mapper.cpp`、`program.cpp`；头文件未精确统计 |
| `core` | fxjit/PixelShader | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/fxjit/PixelShader/PixelShader.vcxproj` | 将像素着色器表示翻译为 JIT 程序的前端/寄存器转换层 | **小至中**；`pshader.cpp`、`pstrans.cpp`、`rdpstrans.cpp`、`shaderreg.cpp`；头文件未精确统计 |
| `core` | fxjit/Platform | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/fxjit/Platform/Platform.vcxproj` | JIT 平台能力与可执行代码承载适配层 | **小**；`Platform.cpp`、`precomp.cpp`；头文件未精确统计 |
| `core` | geometry | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/Geometry.vcxproj` | 贝塞尔、布尔运算、裁剪、扫描、描边、细分与精确算术静态库 | **中**；代表：`Boolean.cpp`、`BezierFlattener.cpp`、`FillTessellator.cpp`、`ExactArithmetic.cpp`、`LineSegmentIntersection.cpp`；头文件未精确统计 |
| `core` | glyph | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/glyph/glyph.vcxproj` | 字形运行核心、基础字形绘制和调试/性能辅助静态库 | **小**；`glyphruncore.cpp`、`baseglyphpainter.cpp`、`bitmapdbgio.cpp`、`perf.cpp`；头文件未精确统计 |
| `core` | hw | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/hw/hw.vcxproj` | D3D9 设备/资源/表面/交换链、硬件画刷、管线、着色器、字形与软回退静态库 | **超大**；代表：`d3ddevice.cpp`、`HwPipeline.cpp`、`HwShaderEffect.cpp`、`d3dswapchain.cpp`、`hwinit.cpp`，并含 `hw.rc`；总文件未精确统计 |
| `core` | meta | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/meta/meta.vcxproj` | 元渲染目标、桌面/HWND/位图组合目标与迭代器静态库 | **小至中**；代表：`metart.cpp`、`desktoprt.cpp`、`desktophwndrt.cpp`、`metaiterator.cpp`；头文件未精确统计 |
| `core` | resources | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/resources.vcxproj` | WPF 图形资源对象、画刷/几何/效果/3D/视觉/渲染数据及资源编组静态库 | **超大**；代表：`brush.cpp`、`geometry.cpp`、`ShaderEffect.cpp`、`visual3D.cpp`、`marshal_generated.cpp`、`renderdata_generated.cpp`；总文件未精确统计 |
| `core` | sw | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/swlib/sw.vcxproj` | 软件光栅化、覆盖率、扫描管线渲染、软件表面/HWND 目标、字形与 GDI 呈现静态库 | **中**；代表：`aarasterizer.cpp`、`scanpipelinerender.cpp`、`swrast.cpp`、`swpresentgdi.cpp`、`swinit.cpp`；头文件未精确统计 |
| `core` | targets | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/targets/targets.vcxproj` | 基础渲染目标/表面目标和形状裁剪静态库 | **小**；`basert.cpp`、`basesurfrt.cpp`、`shapeclipperforfeb.cpp`；头文件未精确统计 |
| `core` | uce | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/uce.vcxproj` | 合成引擎、客户端/服务端通道、命令批、分区线程、资源从属、渲染目标与场景遍历静态库 | **大**；代表：`composition.cpp`、`clientchannel.cpp`、`partitionthread.cpp`、`rendertarget.cpp`、`generated_resource_factory.cpp`；总文件未精确统计 |
| `shared` | debug/DebugLib | `src/Microsoft.DotNet.Wpf/src/WpfGfx/shared/debug/DebugLib/DebugLib.vcxproj` | 无 PCH 的共享调试支持静态库 | **小**；项目项仅 `debuglib.cxx`；头文件未精确统计 |
| `shared` | util/DllUtil | `src/Microsoft.DotNet.Wpf/src/WpfGfx/shared/util/DllUtil/DllUtil.vcxproj` | DLL 入口、DLL 生命周期实现和共享数据静态库 | **小**；`dllmain.cxx`、`dllmainimpl.cxx`、`data.cxx`、`precomp.cxx`；头文件未精确统计 |
| `shared` | util/UtilLib | `src/Microsoft.DotNet.Wpf/src/WpfGfx/shared/util/UtilLib/UtilLib.vcxproj` | 断言、内存、字符串、注册表、计时器、链表、诊断与调试中断静态库，目标名 `wpfutil` | **中**；代表：`memutils.cxx`、`strutil.cxx`、`registry.cxx`、`instrumentation.cxx`、`assert.cxx`；头文件未精确统计 |

### 3.2 初始分组结论

- `common`：4 个项目。
- `core`：17 个项目，其中 `fxjit` 细分为 4 个项目，`control/util` 和 `sw/swlib` 各形成更深目录边界。
- `shared`：3 个项目。
- 合计：24 个解决方案已加载 WpfGfx 原生项目。
- 已读取的 24 个解决方案项目中，`core/dll/wpfgfx.vcxproj` 为 `DynamicLibrary`；其余 23 个均为 `StaticLibrary`。
- 所有这些项目均显式导入 `$(WpfCppProps)`；WpfGfx 子树的 `Directory.Build.Props` 又统一导入 `Graphics.props`，所以单个 `.vcxproj` 不是完整构建事实来源。
- 上表“规模”只按 IDE 项目项清单和代表文件作定性分级；项目项清单主要显示编译/资源输入，不能证明磁盘上的全部头、内联文件、生成输入或条件项，故总文件数均视为**未精确统计**。
- 通过文件读取另确认 3 个位于 WpfGfx 源码树、但未出现在当前解决方案项目枚举中的项目：`core/control/dll/milctrl.vcxproj`、`exts/exts.vcxproj`、`DbgXHelper/DbgXHelper.vcxproj`。因此当前静态证据至少覆盖 **27 个可读取的 WpfGfx `.vcxproj`**；该数字仍不包含只有引用而未找到本地项目文件的 `core/sw/bilinearspan/bilinearspan.vcxproj`，也不宣称是仓库历史或条件项目的绝对总数。

### 3.3 解决方案枚举之外的 WpfGfx 项目/项目引用

静态项目文件调查证明，“解决方案已加载的 24 个项目”不是完整的磁盘/构建拓扑：

| 类别 | 路径/标识 | 已确认关系 | 是否进入 `wpfgfx` |
|---|---|---|---|
| 内部运输静态库 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/bilinearspan/bilinearspan.vcxproj` | `wpfgfx.vcxproj` 有直接 `ProjectReference`；`WpfProjectReference.targets` 将其映射为本地项目或运输包引用；`ShippingProjects.props` 把 `bilinearspan` 列为 `InternalShippingLibProjects` | **是，直接引用**；当前工作区文件搜索/项目清单未找到本地项目文件，可能由内部运输包满足 |
| 独立控制 DLL | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/control/dll/milctrl.vcxproj` | 动态库，引用 `core/control/util/util.vcxproj`，自身包含 `exports.cpp`、`milctrl.rc` | 未被 `wpfgfx.vcxproj` 引用；视为同源码树旁支工具/控制边界 |
| 调试扩展 DLL | `src/Microsoft.DotNet.Wpf/src/WpfGfx/exts/exts.vcxproj` | 目标名 `wpfx`，引用 `DbgXHelper`、`core/common`、`UtilLib`、`DebugLib` | 未被 `wpfgfx.vcxproj` 引用；属于调试扩展旁支 |
| 调试扩展静态库 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/DbgXHelper/DbgXHelper.vcxproj` | 为 `wpfx` 提供调试器输入/输出、事件、标志等支持 | 未被 `wpfgfx.vcxproj` 引用 |

这要求后续台账同时维护“生产 DLL 聚合拓扑”和“WpfGfx 源码树旁支项目拓扑”，不能把当前解决方案视图当作完整磁盘事实。

旁支项目的静态输出形态也已直接确认：`milctrl.vcxproj` 和 `exts.vcxproj` 为 `DynamicLibrary`，`DbgXHelper.vcxproj` 为 `StaticLibrary`。`milctrl` 只直接引用 `core/control/util`；`wpfx` 只直接引用 `DbgXHelper`、`core/common`、`shared/util/UtilLib` 和 `shared/debug/DebugLib`。这些旁支关系不得混入最终 `wpfgfx` 生产 DLL 的项目引用图。

### 3.4 本轮项目覆盖结论

- 已逐项目调用 IDE 项目文件清单复核解决方案内 24 个 WpfGfx 项目的编译/资源代表项；3.1 表已覆盖全部 24 个项目，没有把同解决方案中的 WpfGfx 外项目混入该数字。
- `fxjit` 四个项目不是单一扁平模块：`PixelShader` 负责着色器翻译前端，`Collector` 提供 JIT 值/指令收集表达，`Compiler` 处理图、流控制、映射和组装，`Platform` 提供平台承载；这一职责划分目前由项目项和文件命名支持，具体调用方向仍需源文件确认。
- `core/dll` 的源码项目项极少，但它是聚合边界；因此“源码规模小”不得误解为迁移风险小。
- `core/resources`、`core/hw`、`core/uce`、`core/av` 和 `core/common` 是首轮定性的大型/超大型调查域；该等级仅用于安排后续静态阅读批次，不代表迁移优先级或总体计划决定。

## 4. 构建与链接聚合关系

### 4.1 最终产物和项目级聚合形态

**静态证据确认**：`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.vcxproj` 是最终生产 DLL 边界：

- `ConfigurationType` 为 `DynamicLibrary`，`CLRSupport=false`。
- 目标名为 `wpfgfx$(WpfVersionSuffix)`。
- 自身只编译 `precomp.cpp`、`dllentry.cpp`，并编译 `milcore.rc`；绝大部分生产实现不在该项目中重复列出，而由项目引用汇聚。
- 当前解决方案加载的另外 23 个 WpfGfx 项目全是 `StaticLibrary`；`wpfgfx.vcxproj` 对这 23 个项目全部有直接 `ProjectReference`。
- 它还直接引用 WpfGfx 外的静态库 `OSVersionHelper.vcxproj`，并引用未在当前工作区找到本地项目文件的 `core/sw/bilinearspan/bilinearspan.vcxproj`。
- 在已搜索的 WpfGfx `.vcxproj`/`.props`/`.targets` 中，没有发现生产路径上的 `Utility`/对象库项目类型、直接列举外部 `.obj` 的聚合清单或 `/WHOLEARCHIVE` 设置。不能据此证明最终链接器从每个静态库抽取了哪些对象成员；该细节仍需实际链接命令或 map 文件验证。

按“逻辑引用”计，最终 DLL 项目有 25 个直接项目依赖：23 个当前解决方案中的 WpfGfx 静态库、1 个 `bilinearspan` 静态库槽位、1 个 `OSVersionHelper` 静态库。`OSVersionHelper` 的两个条件 `ProjectReference` 是同一 GUID 的本地路径二选一，不应重复计作两个逻辑依赖。

项目级关系可概括为：

```text
common/* 静态库 ─┐
core/* 静态库   ─┼─ ProjectReference ─┐
shared/* 静态库 ─┤                    │
bilinearspan     ─┤                    ├─ wpfgfx$(WpfVersionSuffix).dll
OSVersionHelper  ─┘                    │
wpfgfx 自有 dllentry.cpp/milcore.rc ───┘
```

这是一张**项目构建层面的星形聚合图**，不是源码依赖图。除最终 `wpfgfx`、旁支 `milctrl`/`wpfx` 外，已搜索的生产静态库项目没有自己的 `ProjectReference`；跨模块依赖主要通过公共 include/PCH、全局符号和最终链接解析表达，所以源码层是稠密网状依赖。

### 4.2 本地项目与运输包静态库替代

`bilinearspan` 是构建拓扑中的特殊前置：

1. `wpfgfx.vcxproj` 始终声明到 `core/sw/bilinearspan/bilinearspan.vcxproj` 的 `ProjectReference`。
2. `ShippingProjects.props` 将 `bilinearspan` 列入 `InternalShippingLibProjects`/`ShippingLibProjects`。
3. `WpfProjectReference.targets` 在 `ResolveProjectReferences` 前检查候选项目是否本地存在；不存在的匹配引用会从 `ProjectReference` 移除，并转成 `MicrosoftDotNetWpfGitHubReference`。
4. 对原生 `.vcxproj`，`Wpf.Cpp.targets` 可从 RID 对应的 `Microsoft.DotNet.Wpf.DncEng` 运输包枚举原生文件；`SdkReferences.targets` 再按引用名筛出对应原生库并加入 `@(Link)`。

因此，“本地 `bilinearspan.vcxproj` 不存在”并不等于构建意图中没有该库；静态配置明确支持以运输包中的原生库替代。**未确认**的是当前环境最终选择本地项目还是哪个具体包文件、版本和库成员，因为本轮未执行还原和构建。

### 4.3 链接输入、延迟加载和入口点

`wpfgfx.vcxproj` 直接声明的系统库为：

- `kernel32.lib`、`winmm.lib`、`user32.lib`、`gdi32.lib`；
- `ole32.lib`、`oleaut32.lib`、`uuid.lib`、`rpcrt4.lib`；
- `advapi32.lib`、`psapi.lib`、`ntdll.lib`；
- `windowscodecs.lib`；
- `evr.lib`、`strmbase.lib`。

目录级 `Graphics.props` 在非静态库上再追加 `d3d9.lib` 和 `$(D3DCompilerDllBaseName).lib`。公共 `Wpf.Cpp.props` 还根据 Debug/Release、UCRT/VCRT 选择追加显式 C/C++ 运行库及相应 `/nodefaultlib`/`/disallowlib` 选项。因此只阅读 `wpfgfx.vcxproj` 的 `AdditionalDependencies` 会漏掉 Direct3D 编译器和 CRT 输入。

已声明延迟加载 `winmm.dll`、`WindowsCodecs.dll`。非 ARM64 且启用当前 `UseDirectXMath=true` 路径时，链接器通过 `/dllrename` 将 D3DCompiler DLL 引用改成带 `$(WpfVersionSuffix)` 的私有名称。是否所有声明的延迟加载项在最终二进制中仍有导入，尚未枚举实际导入表。

链接入口点不是默认 CRT 入口：

- Debug：`_DllMainStartupDebug`；
- Release：`_DllMainStartup`；
- x86 再追加 stdcall 装饰 `@12`。

公共 `Wpf.Cpp.props` 同时规定原生 x86 默认 `StdCall`。这些入口和调用约定是 Native AOT 替换时必须逐架构验证的 ABI 事实，不能只按 x64 行为设计。

### 4.4 导出聚合

`wpfgfx.vcxproj` 不直接把原始 `wpfgfx.def` 交给链接器，而是：

1. 声明 `ModuleDefinitionInputFile=wpfgfx.def`、`ModuleDefinitionOutputFile=wpfgfx.i`；
2. 导入 `GenerateModuleDefinitionFile.targets`；
3. 用 C++ 预处理器以 `/DDLL_NAME=$(TargetName)` 处理 `.def`，去掉 C++ 风格注释并展开 DLL 名；
4. 把 `$(IntermediateOutputPath)wpfgfx.i` 设为链接器 `ModuleDefinitionFile`。

已读取的 `wpfgfx.def` 是最终导出面的重要静态来源，覆盖 COM 基础、工厂、媒体、软件位图、命令通道/资源、几何、字形、互操作设备位图、WIC 代理和渲染选项等导出。但它仍不能单独证明最终 DLL 的完整实际导出表：还需后续与源码定义、托管 P/Invoke 和构建后二进制导出枚举交叉核对。

### 4.5 资源如何进入最终 DLL

- `core/dll/milcore.rc` 直接进入最终 DLL，包含生成/公共的 `NativeVersion.rc` 和 `wpf-etw.rc`。
- 公共 `Wpf.Cpp.targets` 对带资源的动态库还会生成扩展版本资源并加入 `ResourceCompile`；本轮读取了生成逻辑，但未执行确认最终资源集合。
- `core/hw/hw.rc` 包含固定着色器资源和 `ShaderAssemblies/EmbeddedShaders.RC`。`ShaderAssemblies.targets` 先用 Windows SDK `fxc.exe` 把 `.fx` 编译为 `.vsbin`/`.psbin`，再把资源模板复制到中间目录。
- `hw.vcxproj` 把 `$(IntermediateOutputPath)hw.res` 放入 `@(AdditionalLinkerInputs)`；公共 `ResourceLinking.targets` 在 `GetResolvedLinkLibs` 前把这些项加入 `LibFullPath`，其注释明确说明目的是让静态库资源继续传到最终 DLL 链接。

资源传递的**配置意图已确认**，但 `hw.res` 在各配置下是否实际生成、最终 DLL 是否包含所有预期资源 ID，仍需构建和资源枚举验证。

### 4.6 公共导入链与 include 面

已确认的仓库内导入链：

1. 根 `Directory.Build.props` 选择本地 `eng/WpfArcadeSdk/Sdk/Sdk.props` 或同名 SDK 包。
2. `Sdk.props` 导入 `FolderPaths.props`、`ShippingProjects.props` 等，并定义 `WpfCppProps`、`GenerateModuleDefinitionFileTargets`。
3. WpfGfx 的 `Directory.Build.Props` 先向上导入根属性，再导入 `Graphics.props`。
4. 各 `.vcxproj` 在 `Microsoft.Cpp.Default.props` 与 `Microsoft.Cpp.props` 之间导入 `$(WpfCppProps)`，末尾导入 `Microsoft.Cpp.targets`；AV、HW、DLL 项目还分别导入自己的 `av.targets`、`ShaderAssemblies.targets`、模块定义生成 targets。
5. 根 `Directory.Build.targets` 导入 WPF Arcade `Sdk.targets`；后者导入项目引用、运输包解析、C++ 公共 targets 和资源链接 targets。

`Graphics.props` 向全部 WpfGfx 编译注入的公共 include 至少包括：`WpfGfx/Include`、`Include/Generated`、`Shared/inc`、`common`、`core`、`external/inc`、`common/DirectXLayer` 及其 `interfaces`/`factories`/`D3DX9`、`Shared/util/utillib`、已处理生成目录和项目中间目录。`Wpf.Cpp.targets` 还条件加入仓库共享 `inc`、公共 `inc`、运输包共享头、原生版本文件目录和 tracing 目录，并强制包含 `ddbanned.h`。

因此，`.vcxproj` 中没有列出的头文件仍可能是每个翻译单元的直接依赖；后续台账不能只复制项目项清单。

### 4.7 PCH 是隐式依赖清单，不只是编译优化

代表性 PCH 已显示以下边界：

| PCH | 直接暴露的模块/平台依赖 | 迁移含义 |
|---|---|---|
| `core/dll/precomp.hpp` | `common`、`scanop`、`glyph`、`geometry`、`api`、`meta`、`targets`、`sw`、`av` | DLL 入口翻译单元在源码层直接看到多个模块公共面；不可仅依据 `dllentry.cpp` 的显式 include 建依赖 |
| `core/common/precomp.hpp` | `dwriteloader`、`common`、`glyph`、`geometry`、`api`、`hwinit`/`HwCaps`、`swinit`、Media Foundation、DXVA、EVR、AV loader | `core/common` 同时承接引擎生命周期、图形设备和媒体加载边界，是初始化汇聚层 |
| `core/sw/swlib/precomp.hpp` | common/scanop/glyph/geometry/api/targets/meta/av/hw/resources/control，加 `d2d1.h` | 软件渲染并非孤立纯算法库；它可触达硬件、资源、媒体和控制接口 |
| `core/hw/precomp.hpp` | common/scanop/glyph/geometry/api/targets/sw/meta/av/resources/control、D3D/WGX 名称和着色器 | 硬件路径依赖面宽，并包含软件回退与效果资源 |
| `core/av/precomp.hpp` | common/scanop/glyph/geometry/api/targets/sw/hw/resources，WMP、DirectShow、D3D9、EVR、COM/OLE | AV 是媒体与图形合成的交叉边界，不可作为普通独立播放器模块迁移 |
| `common/shared/precomp.hpp` | `std.h`、OLE2、`shared.h` | common/shared 是较低层公共实现 |
| `shared/util/DllUtil/Precomp.h` | 自定义 DLL 启动、DebugLib、进程堆声明 | 自定义 DLL 入口链由共享工具库提供 |
| `shared/util/UtilLib/Pch.h` | `Always.h`、安全字符串/整数和 `Public.h` | 最底层工具/断言/内存公共面 |

大多数 `common`/`core` 项目使用项目本地 `precomp.cpp` 创建 `precomp.hpp`；`fxjit`、`glyph` 使用 `precomp.h`；`DllUtil`/`UtilLib` 分别使用 `Precomp.h`/`Pch.h`；`DynamicCall` 和 `DebugLib` 明确不使用 PCH。文件名大小写在 Windows 构建中不敏感，但迁移台账应记录仓库实际拼写。

### 4.8 构建调查限制

- `$(VCTargetsPath)\Microsoft.Cpp.Default.props`、`Microsoft.Cpp.props`、`Microsoft.Cpp.targets` 位于 Visual Studio/MSVC 安装环境，不在当前工作区可读范围内；标准 C++ `ProjectReference` 如何最终转成 `.lib` 输入未在本轮展开，只能结合项目声明和 MSBuild 常规语义作高置信度判断。
- 根配置还导入外部 `Microsoft.DotNet.Arcade.Sdk`；已读取仓库内 WPF 包装层，但未证明当前环境是否选用本地 SDK 还是解析器取得的包版本，也未展开包内所有条件。
- 未获取 MSBuild 预处理项目、binlog、编译器命令、链接器命令、map 文件或实际产物，所以库顺序、对象抽取、条件宏最终值、资源内容和二进制导入/导出仍未验证。

## 5. DLL 入口与初始化/关闭链

### 5.1 实际入口不是直接的用户 `DllMain`

最终链接入口由 `wpfgfx.vcxproj` 指向共享 `DllUtil` 静态库提供的 `_DllMainStartup`（Release）或 `_DllMainStartupDebug`（Debug），x86 名称再带 stdcall `@12` 装饰。静态调用栈为：

```text
Windows loader
  -> _DllMainStartup / _DllMainStartupDebug        [shared/util/DllUtil/dllmain.cxx]
    -> _DllMainStartupImpl                         [shared/util/DllUtil/dllmainimpl.cxx]
      -> _DllMainCRTStartup                        [MSVC CRT，工作区外]
        -> DllMain                                 [core/dll/dllentry.cpp]
          -> MILCoreDllMain                        [core/common/milcoredllentry.cpp]
```

`core/dll/dllentry.cpp` 的用户入口本身很薄：`extern "C"`、`__stdcall`，忽略第三个上下文参数，仅把模块句柄和 reason 转交 `MILCoreDllMain`。文件注释仍称 “MILCore.dll”，而当前项目输出名是 `wpfgfx$(WpfVersionSuffix)`；这是历史命名，不应据此把它当成另一个 DLL。

外层 `_DllMainStartupImpl` 在 CRT 之前和之后承担额外生命周期：

- 进程附加时先递增 `avalonutil_proc_attached`，创建基于 Win32 `GetProcessHeap()` 的 WPF 堆包装；Debug 下先装载/绑定 `PresentationDebug.dll`（失败则使用本地桩），然后才调用 `_DllMainCRTStartup`。
- 显式卸载路径中，Debug 下先做 `TermDebugLib(..., FALSE)`，再调用 CRT 入口使其分派用户 `DllMain`；返回后才最终释放 DebugLib，并销毁进程堆包装。这个外层顺序保证显式卸载时堆仍覆盖用户关闭逻辑和 CRT 阶段。
- `_DllMainCRTStartup` 内部何时运行 C/C++ 全局构造/析构属于 MSVC CRT 实现，当前工作区未提供该源码；本文不把具体 CRT 子顺序写成已确认事实。

### 5.2 进程附加的精确静态顺序

当 `MILCoreDllMain` 收到 `DLL_PROCESS_ATTACH` 时，当前源码顺序如下：

| 顺序 | 调用/动作 | 直接效果 | 是否主要为惰性基础设施 |
|---:|---|---|---|
| 1 | `g_DllInstance = dllHandle` | 保存 DLL 实例句柄；定义位于 `common/shared/engine.cpp` | 否 |
| 2 | `DisableThreadLibraryCalls` | 请求停止后续线程附加/分离通知；返回值未检查 | 否 |
| 3 | `AvDllInitialize()` | `CStateThread::Initialize()` 依次初始化媒体 apartment lock、event lock | 是；此时不创建媒体线程 |
| 4 | Debug 矩阵乘法预热 | 在默认 meter 下触发 DirectXMath 首次路径，避免后续调试内存计量断言 | Debug 专用 |
| 5 | `g_csCompositionEngine.Init()` | 初始化 UCE/组合引擎全局锁 | 是 |
| 6 | `g_csGraphicsStream.Init()` | 初始化图形流/连接路径全局锁 | 是 |
| 7 | `RenderOptions::Init()` | 清零进程级软件强制/RDP 硬件选项并初始化保护锁 | 是 |
| 8 | `Startup()` | 初始化 CPU 能力、注册表配置以及 D3D/显示/DWrite/AV loader | 见下表 |
| 9 | `SwStartup()` | 选择 MMX/SSE2 开关并创建 ShaderEffect/fxjit 的全局 jitter 锁 | 部分 |
| 10 | `HwStartup()` | `CD3DDeviceManager::Create()`，当前只初始化设备管理锁 | 是；不立即创建 D3D 设备 |
| 11 | Classic ETW 兼容检查 | 可选读取当前用户 `ClassicETW`，必要时初始化旧 tracing 支持 | 条件 |
| 12 | `EventRegisterMicrosoft_Windows_WPF()` | 注册 WPF ETW provider；返回状态未参与 attach 成败 | 否 |
| 13 | 可选 logger / Debug 状态 | `MIL_LOGGER` 下创建日志器；Debug 下恢复默认调试状态 | 条件 |

`core/common/engine.cpp::Startup()` 的内部顺序为：

1. `CCPUInfo::Initialize()`；
2. `CCommonRegistryData::InitializeFromRegistry()`；机器级 `Avalon.Graphics` 键不存在或无法打开会被归一化为 `S_OK`；
3. `CD3DModuleLoader::Startup()`：初始化 D3D/软件光栅模块加载器的管理锁，实际 `d3d9`/软件光栅 DLL 加载仍是惰性的；
4. `g_DisplayManager.Init()`：初始化显示集合管理锁，不在此处枚举显示器或创建设备；
5. `g_DWriteLoader.Startup()`：初始化 DirectWrite 动态加载保护锁，实际 DWrite 库在首次工厂请求时加载；
6. `CAVLoader::Startup()`：初始化 EVR/DXVA2 动态加载器保护锁，实际 EVR/DXVA2 加载按引用惰性发生。

必须区分两个名称相近但职责不同的 AV 初始化阶段：最早的 `AvDllInitialize()` 只初始化媒体状态线程所需的两个静态锁；稍后的 `CAVLoader::Startup()` 初始化 EVR/DXVA2 模块加载器。两者都没有在 DLL attach 时直接创建 WMP 播放器、媒体线程或 EVR 对象。

`SwStartup()` 先根据 CPU 能力设置全局 `g_fUseMMX`/`g_fUseSSE2`；`PRERELEASE` 下可由当前用户注册表禁用。随后它调用 `CMilShaderEffectDuce::InitializeJitterLock()`，该锁由 `fxjit/Platform` 通过 Win32 `CRITICAL_SECTION` 分配，供 `fxjit/Collector/jitteraccess.cpp` 串行化 JIT 访问。

媒体 WPP tracing 也不是在 `AvDllInitialize()` 中启动：已读代码显示首个 `CEventProxy` 构造时，媒体计数从 0 增加后调用 `WPP_INIT_TRACING(L"Microsoft\\Avamedia")`；DLL 关闭时 `AvDllShutdown()` 调用 `WPP_CLEANUP()`。

### 5.3 显式卸载的关闭顺序

只有当外层包装器把 `DLL_PROCESS_DETACH` 交给 CRT/用户 `DllMain` 时，`MILCoreDllMain` 才执行以下顺序：

1. Debug 下 `CAssertDllInUse::Check()` 首先检查是否仍有被跟踪对象存活。
2. `MIL_LOGGER` 下删除全局日志器。
3. `EventUnregisterMicrosoft_Windows_WPF()`。
4. `HwShutdown()` → `CD3DDeviceManager::Delete()`：若 D3D 已加载，释放顶层 D3D/DisplaySet 引用并清除加载标记；设备管理锁本身由全局对象析构处理。
5. `SwShutdown()` → 删除 ShaderEffect/fxjit 全局 jitter 锁。
6. `Shutdown()`：
   - `CAVLoader::Shutdown()`：释放全局 EVR load ref，并清理 EVR、DXVA2 动态模块；
   - `CD3DModuleLoader::Shutdown()`：清理 D3D 和软件光栅模块引用；
   - `g_DWriteLoader.Shutdown()`：释放动态加载的 DWrite 库并反初始化其锁。
7. `AvDllShutdown()`：释放 apartment/event 全局状态线程引用，然后执行 `WPP_CLEANUP()`。
8. Debug 下输出内存泄漏跟踪。
9. 依次 `DeInit()` composition engine lock、graphics stream lock、RenderOptions lock。
10. 返回外层 CRT 包装；DebugLib 最终释放和 `AvDestroyProcessHeap()` 位于 CRT 调用之后。

关闭链整体按 `Hw -> Sw -> engine -> AvDll` 回退了主要顶层 attach 阶段，但并不是每个局部对象都在命名的 `Shutdown()` 中严格逆序释放：

- `engine::Startup()` 顺序是 D3D loader → DisplayManager → DWrite → AV loader，而 `engine::Shutdown()` 是 AV loader → D3D loader → DWrite，且没有显式 `g_DisplayManager` shutdown。
- `CAVLoaderInternal`、`CD3DModuleLoaderInternal` 的管理锁在其全局对象析构函数中 `DeInit()`；`CDisplayManager` 的锁由成员 `CCriticalSection` 的隐式析构处理。
- `CStateThread::FinalShutdown()` 释放全局线程对象引用，但不直接 `DeInit()` 两个静态锁；静态锁自身析构负责兜底。
- `CD3DDeviceManager::Delete()` 释放已加载对象，但管理锁在全局 `CD3DDeviceManager` 析构中反初始化。

这些“命名 Shutdown + CRT 全局析构”双层释放语义必须在迁移时单独建模；不能仅把几个 `Shutdown()` 方法翻译成一个统一的托管 dispose。

### 5.4 正常进程退出与显式卸载不同

**静态证据确认**：`_DllMainStartupImpl` 在 `DLL_PROCESS_DETACH` 时检查 `lpreserved`。当它非空（Windows 用来表示进程终止）且 `g_fAlwaysDetach == FALSE` 时，函数立即返回 `TRUE`，不会调用 `_DllMainCRTStartup`，也不会在该路径销毁 WPF 进程堆。当前 WpfGfx 搜索仅找到 `g_fAlwaysDetach` 的定义，初值固定为 `FALSE`，未发现其它赋值。

因此：

- **显式 `FreeLibrary`/正常 DLL 卸载路径**（`lpreserved == NULL`）会执行完整 MilCore 关闭链。
- **进程终止路径**默认跳过该关闭链，以避免在进程退出期间执行大量不安全或无必要的清理。

这不是普通的“可省略优化”，而是生命周期兼容事实。Native AOT 迁移不得假设所有 shutdown 都会在进程退出时运行，也不得把显式卸载与进程终止无条件合并为同一清理策略。

### 5.5 失败传播、回滚和需要保真的异常点

- `MILCoreDllMain` 的 attach 阶段使用 `IFC(...)->Cleanup`，任一步返回失败都会令最终 `BOOL` 为 `FALSE`；函数内部没有按已完成阶段逐项回滚。
- 是否以及以何种顺序由 Windows loader/MSVC CRT 在 attach 返回 `FALSE` 后触发 detach/析构，属于工作区外运行时行为，本轮未验证。迁移前必须用失败注入验证原生基线，不能根据常识补写一套“更合理”的回滚。
- 多数关闭函数对空指针、未加载模块或无效 `CCriticalSection` 有保护，但这不等于所有部分初始化组合都已证明安全。
- `CStateThread::Initialize()` 先初始化 apartment lock，再初始化 event lock；第二步失败时没有同函数内回滚第一把锁。
- `EventRegisterMicrosoft_Windows_WPF()`、`DisableThreadLibraryCalls()` 的返回状态没有进入 attach 失败结果，说明这些操作在当前代码中是非致命的。
- **重要静态异常点**：`CMilShaderEffectDuce::InitializeJitterLock()` 在锁创建失败时执行 `IFC(E_FAIL)`，但 `Cleanup` 最终写的是 `RRETURN(S_OK)`，所以从源码看它始终向 `SwStartup()` 报告成功；此时 `g_LockJitterAccess` 可保持 `NULL`。这是必须先用原生失败注入固化的现有行为，初次直译不得静默“修复”为返回失败。
- `SwShutdown()` 只删除 jitter 锁，不重置 MMX/SSE2 全局标记；模块卸载后的地址空间回收承担其余清理。
- 顶层锁的显式反初始化顺序与初始化顺序相同（composition → graphics stream → RenderOptions），不是严格逆序。迁移时应先保持原顺序，再通过差分证据评估是否可改变。

### 5.6 初始化链对迁移顺序的直接约束

1. `shared/util/UtilLib` 的进程堆、锁和错误宏，以及 `shared/util/DllUtil` 的入口包装，是 `core/dll/dllentry.cpp` 之前的前置，不是可后补的工具代码。
2. `common/shared` 中的 `g_DllInstance`、CPU 能力和公共内存/COM 语义先于 `core/common` 引擎初始化。
3. `core/av` 的状态线程锁、`core/uce` 的全局组合锁、`core/common` 的 loader、`core/resources`/`fxjit` 的 jitter 锁和 `core/hw` 设备管理器在 attach 链交叉出现；初始化迁移不能简单按项目名线性完成。
4. 应先建立可观测的 Native AOT DLL attach/detach、显式卸载、进程退出和失败注入测试骨架，再逐函数近似直译该顺序。
5. 所有托管异常必须在入口内封锁，并映射为与 `BOOL`/`HRESULT` 原路径等价的结果；尤其不能让异常越过 loader/ABI 边界。

## 6. 模块职责与大致依赖层次

### 6.1 如何理解这里的“层次”

项目构建图是由 `wpfgfx.vcxproj` 直接引用各静态库的星形图，但源码并不是同样的星形，也不是严格无环分层。公共 include 路径和 PCH 允许静态库之间直接看到彼此类型；最终 DLL 链接再统一解析跨库符号。已读 PCH 直接显示以下回边：

- `resources` 包含 `uce`，而 `uce` 又包含 `resources`；
- `glyph` 的实现 PCH 包含 `sw`、`hw`、`resources`，而 `sw`/`hw` 又包含 `glyph`；
- `av` 包含 `sw`/`hw`/`resources`，这些渲染路径也包含 `av`；
- `targets.h` 包含资源层的 brush context/realizer，而 `resources`、`sw`、`hw` 又建立在 render-target 基类上；
- `api` 的 PCH 可见几乎所有主要渲染模块，既是外部工厂/代理层，也包含跨模块创建逻辑。

因此，下文的“较低层/较高层”只表示**主要职责和常见数据流**，不表示可以按层重构、拆分程序集或消除原有回边。初次 C# 迁移必须保留每个原生文件的直接依赖与调用顺序；循环依赖的消除属于等价完成后的独立重构课题。

### 6.2 模块职责表

| 模块 | 静态确认的主要职责 | 常见依赖/被依赖关系 | 代表证据 |
|---|---|---|---|
| `shared` 根边界 | 不是单一项目，而是仓库级低层共享区域；本轮已确认包含公共 `inc`、调试和 util 子树，并通过公共 targets 注入所有 WpfGfx 编译 | 位于几乎所有模块下方；同时承载构建期强制头、原生入口和调试基础 | `Wpf.Cpp.targets`、`shared/debug/*`、`shared/util/*` |
| `shared/util/UtilLib` | 断言、内存/进程堆、字符串、注册表、高分辨率计时、链表、instrumentation 和 debug break；输出名 `wpfutil` | 被 DllUtil、common/shared、core 各模块的宏和分配语义广泛依赖；其进程堆必须先于用户 `DllMain` | `UtilLib.vcxproj`、`Pch.h`、`MemUtils.cxx` |
| `shared/util/DllUtil` | 提供 `_DllMainStartup*`、CRT 包装、进程 attach 计数和显式卸载策略 | 依赖 UtilLib 的进程堆和 DebugLib 接口；位于 `core/dll::DllMain` 外层 | `DllUtil.vcxproj`、`dllmain.cxx`、`dllmainimpl.cxx` |
| `shared/debug/DebugLib` | 提供默认调试/内存计量桩，并在 Debug 下尝试动态绑定 `PresentationDebug.dll` | 被 DllUtil 的 Debug 入口调用；不属于发布算法层，但其加载失败回退是现有行为 | `DebugLib.vcxproj`、`debuglib.cxx` |
| `common/shared` | 图形公共基础：COM/refcount、内存与数组、矩形/区域、像素格式、CPU/SIMD 能力、资源缓存、ETW、公共 GUID 和 DLL 实例句柄 | 直接建立在 shared util 和 Windows/OLE 上；被 scanop、effects、core/common、geometry 以及几乎所有渲染模块使用 | `common/shared/shared.h`、`shared.vcxproj`、`common/shared/engine.cpp` |
| `common/DynamicCall` | 封装 `LoadLibrary(Ex)`/`GetProcAddress`、模块与函数指针缓存、可缓存/不缓存调用，以及 SEH 到 C++ 异常转换 | 依赖 shared 的模块句柄、SEH 和 Win32 error 基础；已确认由 DPI 兼容路径和 UCE 使用 | `DelayCall.h`、`DelayCall.cpp`、`common/shared/DpiUtil.h`、`uce/precomp.hpp` |
| `common/effects` | 实现 `IMILEffectList`：保存按 CLSID 标识的效果参数块和关联 COM 资源，维持资源 AddRef/Release 语义 | 建立在 common/shared、WGX 接口和 internal GUID 上；由 UCE drawing context、resources brush realizer、SW/meta 渲染路径消费 | `effectlist.h/.cpp`、`effects.vcxproj`、`wgx_render.h` |
| `common/scanop` | 通用像素扫描管线：位图包装、颜色/伽马转换、alpha 乘法、混合、复制、抖动、量化、半色调及 SSE2 变体 | 建立在 common/shared；`core/sw` 通过派生 scan pipeline/builder 执行真实软件渲染，其他图形模块也共享像素类型 | `scanop.h`、`scanop.vcxproj`、`core/sw/scanpipelinerender.h` |
| `core/geometry` | 纯几何核心为主：形状/figure、Bezier 展平、描边/加宽、布尔运算、扫描、裁剪、细分、面积、精确与区间算术 | 主要依赖 `core/common` 公共数学/内存；被 API、targets、glyph、SW/HW、resources、UCE 几乎全栈消费 | `geometry.h`、`Geometry.vcxproj`、`geometry_api.cpp` |
| `core/fxjit/Platform` | JIT 平台适配：内存、可执行代码承载、文件/debug、锁，以及当前 `CProgram` 的 Begin/EndCompile 上下文 | 为 Collector/Compiler/PixelShader 提供平台钩子；jitter 全局锁由 resources 创建、由 Platform 实现 | `Platform.cpp`、`warpplatform.h` |
| `core/fxjit/Compiler` | JIT 内部编译器：`CProgram`、operator/dependency graph、flow control、寄存器/内存定位与映射、调度、x86/x64 汇编和代码缓冲 | 接收 Collector 建立的操作图并产出机器码；依赖 Platform；其头被 Collector、PixelShader 和 resources 直接包含 | `Compiler/precomp.h`、`Assemble.cpp`、`DependencyGraph.cpp`、`Program.h` |
| `core/fxjit/Collector` | 面向调用方的类型化 SIMD/JIT 表达层：标量/向量值、变量、分支和操作收集；`CJitterAccess` 管理进入、编译、代码释放和 flow | 通过当前 `CProgram` 向 Compiler 追加操作；被 PixelShader 和软件 ShaderEffect 路径使用 | `Collector/precomp.h`、`SIMDJit.h`、`JitterAccess.h`、`jitteraccess.cpp` |
| `core/fxjit/PixelShader` | 解析/翻译 D3D 像素着色器字节码与寄存器，生成 `GenerateColorsEffect` 可执行函数 | 依赖 D3D9 shader 类型、Collector 公共表达和 Compiler；由 `core/resources/ShaderEffect` 的软件路径使用 | `PixelShader/precomp.h`、`pshader.h`、`pshader.cpp`、`pstrans.cpp` |
| `core/common` | 核心数学/矩阵、坐标与矩形、显示集合、渲染 tier、D3D/DWrite/DWM/AV 动态加载、内存流/资源池、注册表兼容、RenderOptions 和 MilCore 生命周期 | 建立在 common/shared；初始化时连接 HW/SW/AV；其公共头又被所有高层模块使用，是底层公共库与全局生命周期汇聚点 | `common.h`、`common.vcxproj`、`engine.cpp`、`milcoredllentry.cpp` |
| `core/control/util` | 跨进程 compositor 控制/诊断：共享内存控制文件、dirty-region/渲染开关、帧率和资源计数、位图染色辅助、性能计数器 | 由 UCE partition manager 创建并由 UCE/HW/SW/meta/targets 读取；同时供旁支 `milctrl.dll` 使用 | `control.h/.cpp`、`util.vcxproj`、`partitionmanager.cpp` |
| `core/targets` | `IRenderTargetInternal` 的基础实现、surface render target 模板/层栈和 FEB 形状裁剪 | 为 SW/HW/meta 的具体目标提供基类；依赖 common、geometry、API 和部分 resource brush 上下文，因而不是完全独立的最底层接口包 | `Targets.h`、`BaseRT.h`、`BaseSurfRT.h`、`targets.vcxproj` |
| `core/glyph` | 与具体后端相对独立的 glyph run 数据、基础 glyph painter、调试位图和性能计数 | 被 SW/HW 字形渲染使用；实现 PCH 又看到 scanop、geometry、targets、SW/HW/resources，形成文本渲染回边 | `glyph.h`、`GlyphRunCore.h`、`glyph.vcxproj` |
| `core/resources` | 组合服务器侧（DUCE）资源对象和渲染数据：2D/3D transform、geometry、brush、drawing、visual、effect、PixelShader、bitmap、video、缓存与 realizer | 与 UCE 的 handle/resource-slave/graph walker 紧耦合；调用 geometry、targets、SW/HW/AV/meta/fxjit；包含多份生成数据/编组实现 | `resources.h`、`resources.vcxproj`、`resources_generated.h`、`marshal_generated.cpp` |
| `core/sw` / `core/sw/swlib` | CPU 软件渲染：coverage/AA rasterizer、scan pipeline、brush span、bitmap/glyph、surface/HWND/bitmap 目标、GDI present、软件 ShaderEffect 和双缓冲位图 | 建立在 scanop、geometry、glyph、targets、resources 和 fxjit 上；也被 HW 作为回退路径、被 meta/UCE 选择；另需 `bilinearspan` | `sw.h`、`sw.vcxproj`、`swrast.cpp`、`swpresentgdi.cpp` |
| `core/hw` | D3D9/D3D9Ex 硬件渲染：设备/资源/纹理/表面/交换链、brush/color source、vertex/pipeline、shader、glyph、HWND/bitmap 目标、设备位图互操作及软件回退 | 建立在 common、targets、geometry、glyph、resources、effects；直接调用 SW 回退并承载 AV 表面；依赖编译后的 shader 资源 | `hw.h`、`hw.vcxproj`、`d3ddevice.cpp`、`HwPipeline.cpp`、`swfallback.h` |
| `core/av` | WMP/DirectShow/EVR/DXVA 媒体层：播放器状态机、COM apartment/event threads、sample queue/scheduler、媒体 buffer、HW/SW surface renderer 和事件代理 | 与 SW/HW/resources/UCE 合成路径双向耦合；依赖 COM/OLE、WMP、DirectShow、Media Foundation、EVR、DXVA2、D3D9 | `av.h`、`av.vcxproj`、`milav.cpp`、`WmpPlayer.h`、`EvrPresenter.cpp` |
| `core/meta` | 组合多个具体 render target 的元目标：desktop/HWND/bitmap/dummy targets、跨显示迭代和 bounds/transform 调整 | 包装/分派 targets、SW、HW、AV，并由 API/UCE 使用；是“选择/组合后端”而不是第三套光栅器 | `meta.h`、`metart.h`、`desktoprt.h`、`meta.vcxproj` |
| `core/uce` | 组合引擎与协议执行中心：连接、客户端/服务端 channel、command batch、handle table、resource factory、partition/thread/scheduler、scene traversal、dirty region、HWND/print target 和呈现 | 消费生成命令与 resources；调度 meta/SW/HW/AV；对外实现大量 `MilConnection`/`MilChannel`/`MilResource`/geometry/glyph 导出 | `uce.h`、`uce.vcxproj`、`apifunc.cpp`、`composition.cpp`、`generated_resource_factory.cpp` |
| `core/api` | 面向外部 MIL/WGX COM/C ABI 的对象工厂和代理：factory、render state/context、brush、codec/WIC、mesh/light/shader，以及部分直接导出 | PCH 可见 common/effects/scanop/glyph/geometry/targets/SW/HW/meta/resources/AV/UCE；创建并桥接这些实现 | `api_include.h`、`api.vcxproj`、`api_factory.cpp`、`exports.cpp` |
| `core/dll` | 最终 `wpfgfx` 二进制边界：自定义 DLL 入口转发、资源、`.def` 导出和所有静态库聚合 | 位于所有生产模块之上；自身不承载主要渲染算法 | `wpfgfx.vcxproj`、`dllentry.cpp`、`wpfgfx.def`、`milcore.rc` |

### 6.3 近似依赖带

在不掩盖循环的前提下，可用下列“依赖带”安排调查和脚手架前置：

```text
带 0：Windows/MSVC/CRT + shared/inc
      shared/util/UtilLib + shared/debug/DebugLib + shared/util/DllUtil

带 1：common/shared + common/DynamicCall
      common/scanop + common/effects + core/geometry
      fxjit Platform -> Compiler/Collector -> PixelShader（逻辑流水线）

带 2：core/common + core/control/util + core/targets + core/glyph

带 3：core/resources <-> core/uce
      core/sw <-> core/hw <-> core/av
      core/meta 在具体 render target 之上做组合/选择

带 4：core/api + core/uce 的 C ABI 实现

带 5：core/dll + DllUtil/CRT 启动与最终导出/资源
```

这不是建议的重构层次。它只说明：低层 ABI 类型、内存/锁/COM、生成协议和公共数学必须先可表达；随后才能让上层文件按原路径逐个落位。`resources`/`uce` 和 SW/HW/AV 的循环应通过“先建立类型/签名骨架，再逐文件填充实现”处理，而不是通过合并文件或重新设计接口消除。

### 6.4 典型运行数据流

**命令/合成主链（高概率静态归纳）**：托管调用方 → `wpfgfx` 导出 → `core/uce` connection/channel/command batch → generated resource factory / `core/resources` DUCE 对象 → UCE drawing/composition traversal → `core/meta`/`core/targets` → `core/hw` 或 `core/sw` → HWND/bitmap/交换链/GDI present。

**媒体链（高概率静态归纳）**：API/导出创建 `IMILMedia` → `core/av` WMP 状态线程与 EVR presenter → HW/SW media buffer/surface → `core/resources` video resource → UCE composition/render target。

**软件像素效果链（静态证据支持）**：`core/resources/ShaderEffect` → `core/fxjit/PixelShader` 翻译 → Collector 建立 SIMD 操作 → Compiler/Platform 生成可执行函数 → `core/sw` scan pipeline 执行。

**几何链（静态证据支持）**：UCE/API 几何导出或 resource geometry → `core/geometry` flatten/widen/combine/tessellate → targets/SW/HW 消费形状；glyph outline 也可进入几何路径。

以上数据流仍需调用点级台账和运行时验证；它们不能替代逐文件依赖记录。

## 7. 共享基础设施与生成代码

### 7.1 公共属性与路径注入

已确认的静态导入链为：

1. 仓库根 `Directory.Build.props` 选择并导入 `eng/WpfArcadeSdk/Sdk/Sdk.props`。
2. `Sdk.props` 导入 `FolderPaths.props`，定义 `WpfSourceDir`、`WpfGraphicsPath` 等路径，并定义 `WpfCppProps` 和 `GenerateModuleDefinitionFileTargets`。
3. `src/Microsoft.DotNet.Wpf/src/WpfGfx/Directory.Build.Props` 向上导入根 `Directory.Build.props`，随后统一导入同目录 `Graphics.props`。
4. 每个 `.vcxproj` 又在 C++ 默认 props 与 `Microsoft.Cpp.props` 之间显式导入 `$(WpfCppProps)`。
5. 根 `Directory.Build.targets` 最终导入 WPF Arcade SDK targets；其中包含项目引用解析和通用 C++ targets。

`Graphics.Paths.props`/`Graphics.props` 把以下路径注入全部 WpfGfx C++ 编译：

- `WpfGfx/Include`；
- `WpfGfx/Include/Generated`；
- `WpfGfx/Shared/inc`；
- `WpfGfx/common`、`WpfGfx/core`；
- `WpfGfx/external/inc`；
- `WpfGfx/common/DirectXLayer` 及 `interfaces`、`factories`、`D3DX9`；
- `WpfGfx/Shared/util/utillib`；
- 已处理生成文件目录和项目中间输出目录。

因此，大量头文件依赖不会出现在 `.vcxproj` 的 `ClCompile` 清单中；后续逐文件台账必须单独记录 include 来源。

### 7.2 公共编译/ABI 相关属性

`eng/WpfArcadeSdk/tools/Wpf.Cpp.props` 已确认：

- 使用 Visual Studio 2022 `v143` 工具集和 Windows SDK `10.0.26100.0`；
- 统一定义 Unicode、`STRICT`、Windows 版本宏和架构宏；
- 原生 x86 项目默认调用约定为 `StdCall`，这是 ABI 迁移必须单列验证的事实；
- Debug/Release、CRT/UCRT/VCRT、LTCG、CFG 与默认库选择由公共 props 控制，而不是由各 WpfGfx 项目重复声明；
- `TraceWpp` 路径来自 Windows SDK，并针对 ARM64 使用 x64 主机工具。

`Graphics.props` 已确认当前 `UseDirectXMath=true`：

- 定义 `DIRECTXMATH`；
- 对非静态库追加 `d3d9.lib` 和 `$(D3DCompilerDllBaseName).lib`；
- 非 ARM64 链接还通过 `/dllrename` 把 D3DCompiler DLL 名改为带 WPF 后缀的私有名称；
- D3DX9 旧路径保留为条件分支，但当前属性明确选择 DirectXMath；D3DX 分支是历史/条件边界，不应误记为当前已启用依赖。

### 7.3 已发现的生成和资源协议

| 生产者/输入 | 生成或中间产物 | 消费方式 | 静态确认状态 |
|---|---|---|---|
| `WpfGfx/include/processed` 签入文件 | `wgx_av_types.cs/.h`、`wgx_core_types.cs/.h` | `Graphics.props` 在构建前复制到 `IntermediateOutputPath` 并把该目录加入 include | 已确认复制协议；README 说明这些本应由 Build Task 生成，当前文件源自旧 NetFxDev1 中间输出；生成器尚未定位 |
| `core/dll/wpfgfx.def` | 中间 `wpfgfx.i` | `GenerateModuleDefinitionFile.targets` 用 C++ 预处理剥离注释/展开 `DLL_NAME`，链接器以中间 `.i` 作为 `ModuleDefinitionFile` | 已确认协议；尚未读取 `.def` 内容与实际生成结果 |
| `core/av` 全部 `ClCompile` + `avtrace.h`，Release 另用 `frewpp.ini` | 每个实现对应 `IntermediateOutputPath/<FileName>.tmh` | `av.targets` 在 Build 前调用 Windows SDK `tracewpp.exe`，并定义 `RUN_WPP` | 已确认协议；未执行工具，`.tmh` 完整集合未验证 |
| `core/hw/ShaderAssemblies/Source/*.fx` | `ShaderEffectVS.vsbin`、`ShaderEffectVS30.vsbin`、多个 Blur/DropShadow `.psbin` | `ShaderAssemblies.targets` 调用 Windows SDK `fxc.exe`，再复制 `EmbeddedShaders.RC` 模板；`hw.rc` 通过中间 include 路径嵌入，`hw.res` 作为附加链接输入 | 已确认协议；未执行着色器编译/资源编译 |
| `core/dll/milcore.rc` | `wpfgfx` DLL 资源 | 由最终 DLL 项目直接编译 | 已确认项目项；资源包含关系待细读 |
| `core/hw/hw.rc` | `hw.res` | 静态库项目将资源结果显式列为 `AdditionalLinkerInputs`，由最终聚合链带入 | 已确认项目项；实际传递方式待链接验证 |

### 7.4 PCH 边界初步盘点

- 大多数 `common`/`core` 项目各自在本目录用 `precomp.cpp` 创建 `precomp.hpp`。
- `fxjit/{Collector,Compiler,PixelShader,Platform}` 和 `core/glyph` 使用 `precomp.h`。
- `shared/util/DllUtil` 使用 `precomp.cxx` + `precomp.h`；`shared/util/UtilLib` 使用 `pch.cxx` + `pch.h`。
- `common/DynamicCall` 和 `shared/debug/DebugLib` 明确不使用 PCH。
- PCH 是项目局部编译加速边界，但其中 include 集合很可能也是模块真实隐式依赖边界；下一阶段将读取代表性 PCH。

## 8. 外部依赖边界

### 8.1 已确认的直接构建、链接或运行时依赖

| 依赖 | 静态证据 | 对迁移的含义 |
|---|---|---|
| `OSVersionHelper` | `wpfgfx.vcxproj` 直接 `ProjectReference`，本地共享路径二选一 | 需要调查其被调用 API，并在单一 C# 项目中以等价实现或最小互操作保留行为 |
| D3D9/D3D9Ex | `Graphics.props` 给最终动态库追加 `d3d9.lib`；`core/common/d3dloader.cpp` 动态解析 `Direct3DCreate9`/`Direct3DCreate9Ex` 并管理模块引用 | Silk.NET 评估必须覆盖 D3D9 与 D3D9Ex，不能只评估现代 DXGI/D3D11 |
| D3DCompiler | `Graphics.props` 追加 `$(D3DCompilerDllBaseName).lib` 并对非 ARM64 使用 `/dllrename`；`redist/D3DCompiler/D3DCompiler.vcxproj` 复制 SDK 的私有后缀 DLL | 新项目必须决定继续部署私有 D3DCompiler，还是使用系统/显式加载路径；不得无证据改变着色器编译行为 |
| DirectWrite 系统库 | `core/common/dwritefactory.cpp` 调用共享 `WPFUtils::LoadDWriteLibraryAndGetProcAddress`；`src/Shared/cpp/dwriteloader.cpp` 从 System32 加载 `dwrite.dll` 并解析 `DWriteCreateFactory` | wpfgfx 自身的 DirectWrite 路径是系统 DLL 的惰性动态加载，生命周期与错误映射要原样验证 |
| WIC | `wpfgfx.vcxproj` 链接 `windowscodecs.lib` 并延迟加载 `WindowsCodecs.dll` | 图像编解码、位图和 WIC 代理是明确的 ABI/COM 边界 |
| 媒体栈 | `wpfgfx.vcxproj` 链接 `evr.lib`、`strmbase.lib`；AV PCH/实现使用 WMP、DirectShow、Media Foundation、EVR、DXVA2 | `core/av` 不能仅用 Silk.NET.Direct3D 覆盖，需要单独的媒体/COM 互操作方案 |
| Win32/COM/GDI/系统服务 | 最终项目直接链接 `kernel32`、`user32`、`gdi32`、`ole32`、`oleaut32`、`uuid`、`advapi32`、`rpcrt4`、`psapi`、`ntdll`、`winmm` 等；各模块还动态解析平台入口 | 需建立平台 API 清单；Native AOT 的 P/Invoke、函数指针、COM 与句柄所有权是横切前置 |

### 8.2 与 PresentationCore 同属部署链、但不是 `wpfgfx` 直接项目引用的组件

| 组件 | 已确认关系 | 边界判断 |
|---|---|---|
| `DirectWriteForwarder` | `PresentationCore.csproj` 直接引用该 C++/CLI 动态库；`PresentationCore/ModuleInitializer.cs` 在模块初始化时调用其 `MS.Internal.NativeWPFDLLLoader.LoadDwrite()`。`wpfgfx.vcxproj` 没有对它的项目引用，wpfgfx 自己直接从 System32 加载 `dwrite.dll` | 它是 PresentationCore 的文本/DPI/兼容部署链组件，不是当前 wpfgfx 最终链接输入；迁移 wpfgfx 时仍需端到端考虑它与新 DLL 的共存和加载顺序 |
| `PresentationNative` | `PresentationCore` 中存在大量针对 `PresentationNative` 的独立 P/Invoke；`redist/PresentationNative.vcxproj` 从运输包发现并复制 DLL/PDB。`wpfgfx.vcxproj` 未引用它 | 它是相邻原生运行时组件，不属于本项目首要翻译范围；不得误把其导出纳入 wpfgfx ABI，但端到端测试环境需要同时提供 |

### 8.3 仍待调用点级确认的平台面

DXGI、DWM、D2D、软件光栅器 DLL、GDI 内部入口及若干按需加载模块已在头文件或实现中出现，但本专题没有形成逐 API/逐调用点清单。它们应在 Silk.NET 专题和逐文件迁移台账中继续分类为“链接依赖、动态加载依赖、仅类型头依赖或条件代码依赖”。

## 9. 对 C# 单项目目录布局的建议

初始约束建议，待完整拓扑证据补充：

- 单一 `.csproj` 只统一构建和发布边界，不合并源码职责。
- `WpfGfxShape/Code` 下生产源码应至少镜像 `common`、`core`、`shared` 三大根边界。
- 对 `core/fxjit/{Collector,Compiler,PixelShader,Platform}`、`core/control/util`、`core/sw/swlib`、`shared/debug/DebugLib`、`shared/util/{DllUtil,UtilLib}` 保留原层级。
- 原生实现文件原则上一一对应主要 C# 文件；共享头可对应显式的低层类型/常量/互操作文件，但不得借机抽象重组。
- 原项目边界可转写为命名空间、目录、内部依赖清单和迁移批次标签，不能被“单项目”解释为扁平化源码。

## 10. 已确认事实

1. `WpfGfxShape/Docs/00-migration-charter.md` 明确要求逐文件近似直译、正确性/ABI 优先，等价迁移前禁止重构与优化。
2. IDE 当前解决方案枚举显示 24 个 `src/Microsoft.DotNet.Wpf/src/WpfGfx` 下的 `.vcxproj`：`common` 4 个、`core` 17 个、`shared` 3 个。
3. 24 个已加载项目中，`wpfgfx.vcxproj` 是动态库，其余 23 个是静态库。
4. `wpfgfx.vcxproj` 还直接引用未加载/本地缺失的 `bilinearspan.vcxproj`；公共 targets 允许以内部运输包替代本地项目。
5. WpfGfx 子树存在未进入当前解决方案枚举且未被 `wpfgfx` 引用的旁支项目：`milctrl.vcxproj`、`exts.vcxproj`、`DbgXHelper.vcxproj`。
6. WpfGfx 全项目通过目录级属性统一获得 `Graphics.props` 的 include、DirectXMath 和 D3D 链接设置。
7. AV WPP、HW shader/resource、已处理 WGX 类型头以及 `.def -> .i` 均构成项目清单之外或项目清单不完整表达的生成依赖。
8. `OSVersionHelper` 是 `wpfgfx` 的直接项目引用；D3DCompiler 是最终链接和私有重分发依赖；wpfgfx 通过共享 loader 从 System32 惰性加载 `dwrite.dll`。
9. `DirectWriteForwarder` 和 `PresentationNative` 属于 PresentationCore 的相邻部署/调用链，不是 `wpfgfx.vcxproj` 的直接项目引用或链接输入。
10. WIC、D3D9/D3D9Ex、EVR/DirectShow、Win32、COM 和 GDI 均已有直接静态证据，不能在 C# 迁移中笼统归为一个“DirectX 依赖”。
11. 原拓扑调查轮次没有执行构建、测试、脚本统计或二进制导入/导出枚举；后续补充的项目项精确下限也不构成产物验证。

### 10.1 当前项目项精确下限（本轮 IDE 复核）

以下数字来自当前 24 个已加载 WpfGfx 生产项目的 `.vcxproj`/IDE 项目文件清单，只是**直接项目项下限**，不等于磁盘全文件闭包：

- `ClCompile` 直接项目项合计 **461** 个；其中约 **22** 个是 PCH 创建翻译单元，约 **439** 个是其它直接编译单元。
- 461 项中包含 1 个 WpfGfx 树外直接编译文件：`src/Microsoft.DotNet.Wpf/src/Shared/cpp/dwriteloader.cpp`。
- 生产聚合内直接 `ResourceCompile` 项为 **2** 个：`core/dll/milcore.rc` 与 `core/hw/hw.rc`。
- 最大的直接编译域为：`core/resources` **98**、`core/hw` **70**、`core/uce` **43**、`core/common` **42**、`core/av` **34**。
- `wpfgfx.vcxproj` 有 **25 个逻辑项目依赖**：其余 23 个已加载 WpfGfx 静态库、`bilinearspan` 供应槽位和一个逻辑 `OSVersionHelper`；后者在项目文件中以两个条件路径指向同一 GUID，不能重复计数。

上述数字不包含未列入项目项的头/PCH include 闭包、生成 `.h/.inl/.cs`、`.w`/XML 模型、WPP `.tmh`、shader `.fx/.vsbin/.psbin`、中间 `.res/.i`、公共 props/targets 或运输包内容。完整迁移分母必须由后续机器可读 Ledger 将这些并列视图合并后确定。

## 11. 推断及置信度

| 推断 | 置信度 | 理由 | 待确认方式 |
|---|---|---|---|
| `core/dll/wpfgfx.vcxproj` 是最终 DLL 聚合项目 | 高 | 已确认 `DynamicLibrary`、目标名、最终资源、导出中间文件和对子系统静态库的直接项目引用 | 仍需用实际链接命令/二进制验证输入完整性 |
| 已加载的其余 23 个 WpfGfx 项目以静态库参与聚合 | 高 | 各项目均声明 `StaticLibrary` 且被 `wpfgfx.vcxproj` 直接引用 | 实际库成员选择和链接顺序尚未运行验证 |
| `common/shared` 和 `shared/*` 提供跨子系统基础设施 | 高 | 已读取项目、PCH、进程堆、DLL 入口、DebugLib、COM/refcount 和公共引擎代表实现 | 后续通过逐文件台账补齐完整调用点，而非重新判断其基础层地位 |
| `fxjit` 四项目共同构成“像素着色器翻译 → JIT 表达收集 → 编译/组装 → 平台承载”管线 | 高 | 项目清单、PCH、`ShaderEffect` 调用路径、`jitteraccess` 和 Platform 锁实现共同支持 | 后续需验证生成代码、可执行内存策略及 Native AOT 下的替代可行性 |

## 12. 风险与未决问题

- 解决方案可能未加载磁盘上全部 `.vcxproj`、`.props`、`.targets` 或历史/条件项目。
- 项目文件可能通过公共导入、条件属性或通配方式隐藏实际源文件与链接输入。
- 未知是否存在生成后才出现的头、源、资源、导出或命令协议文件。
- 已知 `bilinearspan` 可能由运输包提供；仍未知该库的源码、版本匹配、静态库成员和在当前工作区解析出的实际文件。
- 未知 `wpfgfx` 是否除显式项目引用和公共 props 之外还消费其他预构建 `.lib`/`.obj`；实际链接顺序未验证。
- 禁止脚本导致无法快速、可靠地生成全量 include 图和精确源文件计数；必须避免伪造数字。
- 平台依赖名称可能只出现在头文件、宏或延迟加载逻辑中，不能把 include 自动等同于最终链接/运行依赖。

## 13. 原拓扑调查工具边界下尚未验证的事项

- 每个项目精确的 `.cpp`、`.c`、头文件和资源文件数量。
- MSBuild 预处理后的完整属性与导入展开结果。
- 实际编译器和链接器命令行、最终 `.lib`/`.obj` 输入顺序。
- 具体配置和架构下的条件差异。
- 实际 DLL 导出表、导入表、延迟加载表和静态库成员。
- 生成步骤是否可在当前环境成功执行，以及生成文件是否与仓库签入版本一致。
- 运行时加载 DirectWriteForwarder、D3DCompiler 或其它系统 DLL 的真实路径。

## 14. 对总体计划的输入

调查完成时应向总体计划提供：

- 原生项目与目录边界清单；
- 聚合/链接依赖图及事实/推断标记；
- 共享基础设施和生成协议的前置顺序；
- 外部依赖矩阵；
- 单一 .NET 10 项目中的镜像目录规则；
- 后续逐文件台账字段；
- 建议优先调查/迁移的模块层次，但不在本专题给出最终总计划。

### 14.1 后续逐文件台账候选字段

- 原生项目；
- 原始相对路径；
- 文件类别（实现/头/资源/生成输入/生成输出/构建文件）；
- 原职责；
- 代表类型/函数/导出；
- 直接 include；
- 项目内直接依赖；
- 跨项目依赖；
- 平台/API/COM 依赖；
- 生成来源与生成消费者；
- PCH 归属；
- ABI/布局/调用约定敏感点；
- 生命周期/线程/锁/失败路径敏感点；
- 拟议 C# 镜像路径；
- 迁移状态（使用章程规定状态）；
- 未解析依赖；
- 测试与差分证据；
- 已知偏差、风险与回退点。

## 15. 下一调查者可直接执行的动作

1. ABI 专题以 `core/dll/wpfgfx.def` 为主清单，逐项交叉到源码定义、共享头和 PresentationCore P/Invoke，不把 `PresentationNative` 导出混入其中。
2. 迁移顺序专题依据第 6.3 节依赖带建立逐文件台账，先处理 ABI 类型、生成协议、内存/锁/COM 和入口生命周期，再处理循环的 resources/UCE 与 SW/HW/AV。
3. Native AOT 项目专题为 x86、x64、ARM64 分别记录导出名、调用约定和加载行为；尤其保留 x86 stdcall 与显式卸载/进程退出差异。
4. Silk.NET 专题分别评估 D3D9/D3D9Ex、D3DCompiler、DirectWrite、WIC、DXVA/EVR 和 Win32/COM；不得以 D3D11 覆盖度推断 wpfgfx 可迁移性。
5. 测试专题把 DLL attach/detach、部分初始化失败、私有 D3DCompiler、系统 DirectWrite 惰性加载和硬件资源嵌入列为早期验证点。
6. 对需要 binlog、实际链接命令、资源/导入/导出枚举或运行行为才能确认的结论继续保留“尚未验证”标签；在用户未解除限制前不得使用命令行补齐。

## 16. 过程更新记录

- 已读取迁移章程全文。
- 本继续轮次已重新读取迁移章程与本文既有成果；继续沿用“正确性与 ABI 优先、逐原生文件近似直译、等价迁移前禁止重构、单项目仍镜像原模块边界”的最高原则。
- 已通过 IDE 解决方案枚举完成首轮 WpfGfx 项目盘点：24 个项目。
- 已创建本文档骨架；后续调查将分阶段直接补写，而不是只在最终消息汇报。
- 已读取 24 个已加载项目的项目清单，并确认 1 个最终动态库 + 23 个静态库的形态。
- 已发现 `bilinearspan` 直接引用缺口，以及 `milctrl`、`wpfx/exts`、`DbgXHelper` 三个未加载旁支项目边界。
- 已确认 WpfGfx 目录级 `Graphics.props`、WPF Arcade C++ 公共属性、AV WPP、HW shader/resource 和模块定义文件生成协议。
- 本继续轮次已逐项目复核 24 个已加载 WpfGfx 项目清单，补齐 `fxjit` 四模块代表文件和全部项目的定性规模；精确总文件数仍未统计。
- 已补齐外部边界：区分 wpfgfx 的直接项目/链接/动态加载依赖与 PresentationCore 的相邻部署组件，并将本文状态收口为本轮静态拓扑基线完成。
