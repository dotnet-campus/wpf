# wpfgfx ABI 与托管调用边界

> 状态：**权威静态基线已完成；真实二进制、布局与运行兼容仍受后续门禁约束**  
> 权威用途：后续 `wpfgfx` C#/.NET 10 Native AOT 迁移的 ABI、托管调用、加载、生命周期、所有权、回调和兼容验证入口  
> 适用范围：当前工作区中的 PresentationCore/共享托管调用方与 `src/Microsoft.DotNet.Wpf/src/WpfGfx` 原生实现  
> 工具边界：本轮**未使用且绝对禁止使用任何命令行、终端、脚本或其间接方式**  
> 详细原生证据：[investigations/02a-native-export-surface.md](investigations/02a-native-export-surface.md)  
> 详细托管证据：[investigations/02b-managed-callers.md](investigations/02b-managed-callers.md)

## 0. 最高原则与使用规则

本文件必须与 [00-migration-charter.md](00-migration-charter.md) 和 [01-native-source-topology.md](01-native-source-topology.md) 一起使用。后续任何设计、实现或测试不得违反以下最高原则：

1. **逐原生文件近似直译。** 原始路径、类型、函数、字段、分支、调用顺序、错误顺序、锁顺序、引用计数和释放顺序必须可追溯。
2. **正确性与 ABI 优先。** 导出名、参数顺序、位宽、调用约定、结构布局、生命周期、线程和失败路径高于代码简洁、现代化、抽象或性能。
3. **等价完成前禁止重构。** 单一 .NET 项目只统一构建与发布边界，不允许合并原模块职责、改写算法或重新设计协议。
4. **Native AOT 导出层必须薄。** 只做 ABI 类型转换、必要参数检查、异常封锁和向逐文件等价实现转发。
5. **托管异常不得跨越 C ABI 或 reverse P/Invoke 边界。** 必须按原路径映射为 HRESULT、返回值或已验证的等价失败语义。
6. **三类身份/句柄不得混用。** 32 位协议句柄、指针宽度客户端对象句柄、COM/native 指针具有不同表示、所有权和释放协议。
7. **静态清单不等于实际二进制。** `.def`、原生定义/声明、托管 `DllImport` 和真实 DLL 导出/运行行为必须四向核对。
8. **任何清单不一致都不得通过删除导出或修改托管声明“解决”。** 必须先枚举真实二进制并执行运行与差分测试，确认哪一侧是当前有效契约。

两份 `investigations` 文档保留调查过程、原始映射和未完成项；本文件综合其稳定结论，但不删除、覆盖或替代过程证据。

## 1. 范围、方法与证据等级

### 1.1 本文件覆盖

- 当前 PresentationCore 使用的 `wpfgfx` DLL 名和加载入口。
- PresentationCore 模块初始化、首次 P/Invoke 加载、MediaSystem/MediaContext/ChannelManager 生命周期。
- 原生 DLL attach、显式卸载和进程终止 detach 的差异。
- `wpfgfx.def` 静态名称基线、托管 `DllImport` 基线和双向差异。
- 调用约定、架构、名称装饰和 P/Invoke 元数据。
- COM、客户端对象句柄、DUCE 协议句柄、SafeHandle、回调和所有权。
- 关键结构、生成命令、数组、字符串、pack/align 和布尔宽度风险。
- Native AOT 薄导出层的强制检查项和首批兼容验证顺序。

### 1.2 明确不在本轮实施

- 不修改 WPF 原生或托管源码、项目文件和生成输入。
- 不修改 `WpfGfxShape/Code`，不创建 Native AOT 项目，不实现导出原型。
- 不构建、不运行测试、不预处理 `.def`、不枚举二进制导入/导出。
- 不使用 PowerShell、cmd、bash、Python、dotnet、msbuild、dumpbin、link 或任何脚本/命令替代方式。

### 1.3 证据等级

| 等级 | 含义 | 本文件中的典型用法 |
|---|---|---|
| **事实—文件** | 已由当前工作区源码、项目文件或声明直接确认 | DLL 常量、结构字段、函数定义、关闭顺序 |
| **事实—静态清单** | 已由 `.def` 或当前托管声明文本确认，但不代表产物 | 106 个 `.def` 名称、109 个声明出现、107 个唯一 EntryPoint |
| **推断—高/中/低** | 多份静态证据支持，但仍需运行或二进制验证 | 某不一致项可能陈旧或仅供其它 native caller |
| **未验证—门禁** | 必须通过构建、二进制、布局探针或运行测试确认 | x86 名称解析、实际导出、真实回调线程时序 |

本轮数量统计均由 IDE 文件读取/搜索和人工交叉核对完成，未使用脚本。后续自动化清单必须复核这些数字，但不得在复核前把它们降格为猜测。

## 2. 当前基线摘要

| 项目 | 当前静态结论 |
|---|---|
| PresentationCore 当前 DLL 常量 | **`wpfgfx_cor3.dll`** |
| 原生目标名意图 | `wpfgfx$(WpfVersionSuffix)`；当前 `WpfVersionSuffix=_cor3` |
| 历史常量 | `WpfGfx_v0400.dll` 存在于另一命名空间/生成边界，**不是当前 PresentationCore 调用常量** |
| `.def` 静态名称 | **106** 个 |
| 当前 MilCore `DllImport` 声明出现 | **10 个源文件、109 个声明** |
| 当前唯一 native EntryPoint | **107 个，已可靠人工去重** |
| `.def` 与托管唯一 EntryPoint 交集 | **99 个** |
| 托管有、`.def` 无 | **8 个，逐名列于第 6.1 节** |
| `.def` 有、当前托管声明未见 | **7 个，逐名列于第 6.2 节** |
| 实际二进制导出 | **未枚举，不得把 106 写成真实 DLL 全部导出数** |

可审计等式：

- `109 个声明出现 - 2 个重载产生的额外出现 = 107 个唯一 EntryPoint`；
- 两个重复 EntryPoint 分别为 `MILAddRef` 和 `MILQueryInterface`，二者各有两个托管重载；
- `107 = 99 个共同名称 + 8 个托管侧独有名称`；
- `106 = 99 个共同名称 + 7 个 .def 侧独有名称`。

因此，当前 107 个唯一托管 EntryPoint 的完整集合可精确重建为：**`wpfgfx.def` 的 106 个名称，移除第 6.2 节 7 个名称，再加入第 6.1 节 8 个名称**。

## 3. DLL 身份、加载与生命周期

### 3.1 当前 DLL 名

**事实—文件：**

- `src/Microsoft.DotNet.Wpf/src/Shared/RefAssemblyAttrs.cs` 在 `PRESENTATION_CORE` 条件下进入 `MS.Internal.PresentationCore` 命名空间。
- `BuildInfo.WCP_VERSION_SUFFIX` 为 `_cor3`。
- `DllImport.MilCore` 为 `$"wpfgfx{BuildInfo.WCP_VERSION_SUFFIX}.dll"`，当前静态展开值是 **`wpfgfx_cor3.dll`**。
- `eng/WpfArcadeSdk/tools/VersionSuffix.props` 把原生 `WpfVersionSuffix` 设为 `_cor3`；`wpfgfx.vcxproj` 的目标名是 `wpfgfx$(WpfVersionSuffix)`。
- `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_core_dllname.cs` 中的 `Microsoft.Internal.DllImport.MilCore = "WpfGfx_v0400.dll"` 位于不同命名空间和历史/另一生成边界。当前 PresentationCore 项目使用的是 `MS.Internal.PresentationCore.DllImport`，不能把旧常量当作当前加载名。

**门禁：**托管常量和原生属性意图均指向 `_cor3`，但本轮未构建或检查产物文件名，最终部署一致性仍须验证。

### 3.2 PresentationCore 模块初始化与首次加载

`src/Microsoft.DotNet.Wpf/src/PresentationCore/ModuleInitializer.cs` 的静态顺序为：

1. 若入口程序集未声明禁用 DPI awareness，则调用 `user32!SetProcessDPIAware`。
2. `DWriteLoader.LoadDWrite()` 从 System32 加载系统 `dwrite.dll` 并解析 `DWriteCreateFactory`。
3. 注册 ProcessExit，在退出时清空函数指针并卸载上述系统 `dwrite.dll`。
4. 调用 `MS.Internal.NativeWPFDLLLoader.LoadDwrite()` 触发 DirectWriteForwarder 相邻加载边界。

当前托管源码没有发现针对 `wpfgfx` 的 `NativeLibrary.Load`、`LoadLibrary`、`GetProcAddress`、自定义 `DllImportResolver` 或硬编码路径。因此当前主路径是：

> CLR 在首次执行任意 `DllImport.MilCore` 调用时，按 `wpfgfx_cor3.dll` 解析并加载模块。

“第一个入口”不是全局固定值。常规 `MediaContext` 构造路径中：

1. 先创建 `MediaContextNotificationWindow`；
2. 其构造函数调用 `MilContent_AttachToHwnd`；
3. 随后才进入 `MediaSystem.Startup` 并调用 `MilVersionCheck`。

因此，`MilContent_AttachToHwnd` 可以早于版本检查触发 DLL attach。`MilVersionCheck` 是协议兼容检查，不是可靠的预加载门。

### 3.3 原生 DLL attach

静态入口链为：

```text
Windows loader
  -> _DllMainStartup / _DllMainStartupDebug
  -> _DllMainStartupImpl
  -> _DllMainCRTStartup
  -> core/dll/dllentry.cpp::DllMain
  -> core/common/milcoredllentry.cpp::MILCoreDllMain
```

`DLL_PROCESS_ATTACH` 的关键顺序为：

1. DllUtil 增加进程 attach 计数并创建 WPF 进程堆包装；Debug 下初始化 DebugLib。
2. CRT 调用用户 `DllMain`。
3. 保存 `g_DllInstance`，调用 `DisableThreadLibraryCalls`。
4. `AvDllInitialize()` 初始化媒体状态线程所需锁。
5. 初始化 composition、graphics stream 和 RenderOptions 锁/状态。
6. `Startup()` 初始化 CPU/注册表以及 D3D、display、DWrite、AV loader 基础设施。
7. `SwStartup()` 初始化软件路径/JIT 锁，`HwStartup()` 初始化设备管理器。
8. 注册 ETW 等附加设施。

这些多数是基础设施初始化；D3D、DWrite、EVR/DXVA 等真实模块和对象仍有惰性加载路径。初次迁移不得把它们合并成一个无差别的托管静态构造器。

### 3.4 MediaSystem、MediaContext 与 ChannelManager

#### 每 Dispatcher 启动

`MediaContext` 每个 Dispatcher 一个实例，当前顺序为：

1. 创建通知 HWND，并调用 `MilContent_AttachToHwnd`。
2. `MediaSystem.Startup(this)`：
   - `MilVersionCheck(0x200184C0)`；
   - 进入 native composition engine lock；
   - 将当前 context 加入全局列表；
   - 首次启动时初始化 partition manager；
   - 查询是否应为 graphics stream client 强制软件渲染；
   - `WgxConnection_Create(false)` 创建 CrossThread connection；
   - 创建全局异步 service channel，从而建立 partition；
   - 增加全局引用计数并退出锁；
   - 锁外调用 `RenderOptions_EnableHardwareAccelerationInRdp`。
3. `MediaSystem.ConnectChannels` 为当前 context 创建普通 async channel 和 out-of-band async channel。
4. `MediaContext` 发送初始化命令、设置通知窗口、请求 tier、close/commit 并同步等待返回消息。
5. 完成后订阅 `Dispatcher.ShutdownFinished`。

#### 同步通道

- `ChannelManager.AllocateSyncChannel()` 惰性执行 `WgxConnection_Create(true)`，得到 SameThread connection。
- 同步 service channel 与业务 channel 属于单独 partition/连接池。
- RenderTargetBitmap 等路径 close/commit 后调用 `WgxConnection_SameThreadPresent`，再读取 back-channel 消息。
- `MilChannel_GetMarshalType` 区分 SameThread 与 CrossThread；两者通知和媒体帧更新语义不同。

#### 已知部分初始化风险

当前源码在若干失败路径上没有本地完整回滚，例如：

- `MediaSystem.Startup` 先把 context 加入列表，再完成 partition/connection/service channel 初始化；
- connection 创建后若 service channel 构造失败，没有本地 disconnect；
- async channel 创建成功而 out-of-band channel 创建失败时，没有本地关闭前者；
- `MediaContext` 较晚才订阅 Dispatcher shutdown。

这些是必须先保真的现有顺序，不能在迁移期凭直觉“修复”。应先用原生失败注入建立基线。

### 3.5 显式关闭

`MediaContext.Dispose()` 当前顺序为：

1. dispose 所有 composition targets；
2. 通知窗口调用 `MilContent_DetachFromHwnd`，再销毁 HWND；
3. 取消 Dispatcher shutdown handler，停止 TimeManager；
4. `RemoveChannels()` 关闭 async、out-of-band 和同步通道/连接；
5. `MediaSystem.Shutdown(this)` 移除 context 并减少全局引用计数；
6. 最后一个 context 关闭全局 service channel、断开异步 connection、反初始化 partition manager。

`DUCE.Channel` 持有裸 `IntPtr`，没有 SafeHandle/finalizer。`Close()` 的 native 调用顺序是：

1. `MilChannel_CloseBatch`；
2. `MilChannel_CommitChannel`；
3. `MilConnection_DestroyChannel`；
4. 将托管 `_hChannel` 清零。

因此显式关闭是协议的一部分，GC 不能替代。

### 3.6 ProcessExit、显式卸载与进程终止 detach

必须区分以下独立路径：

- PresentationCore 模块 ProcessExit：卸载其显式持有的系统 `dwrite.dll`。
- 每个 `MediaPlayerState` ProcessExit：调用 `MILMediaProcessExitHandler`，仅关闭媒体 event proxy 回调通路。
- `AppDomainShutdownMonitor`：当前已确认 D3DImage 在 DomainUnload/ProcessExit 时调用 `InteropDeviceBitmap_Detach`，阻止 composition thread 后续回调。
- Dispatcher shutdown：触发 `MediaContext.Dispose` 和 MediaSystem/channel 正常关闭。
- SafeHandle/finalizer：释放 COM/native 对象；其时序不等同于上述路径。
- 原生 DLL detach：由 Windows loader/CRT/DllUtil 控制。

原生 DllUtil 在 `DLL_PROCESS_DETACH` 检查 `lpreserved`：

- **显式卸载**（例如 `FreeLibrary`，`lpreserved == NULL`）会进入 CRT 和用户 `DllMain`，执行完整 `HwShutdown -> SwShutdown -> Shutdown -> AvDllShutdown`、锁反初始化、DebugLib 和进程堆清理。
- **进程终止**（`lpreserved != NULL`）且 `g_fAlwaysDetach == FALSE` 时立即返回，不调用 CRT/用户 `DllMain`，也不运行完整 MilCore shutdown。

当前 PresentationCore 没有显式卸载 `wpfgfx` 的代码；通常由 DllImport 模块生命周期管理。但 Native AOT 替换仍必须保持“显式卸载”和“进程终止”两条原生语义，不得无条件合并为同一个 `Dispose`。

## 4. 原生 `.def` 静态 ABI 基线

### 4.1 静态名称和属性

`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.def` 的 `EXPORTS` 段人工核对结果：

- **106 个名称条目**；
- `LIBRARY DLL_NAME` 由构建预处理展开；
- 未见显式 `DATA`、`PRIVATE`、`NONAME`、ordinal、forwarder 或 alias；
- 未见 `#if`/`#ifdef` 条件导出；
- 所有公开名称均以未装饰文本写入 `.def`；
- `.def` 会先预处理为 `wpfgfx.i`，本轮没有生成或读取中间文件。

### 4.2 `.def` 外候选

Debug 源码存在 `.def` 外数据导出候选 `g_fNoMeterChecks`：

- `shared/util/DllUtil/Precomp.h` 和 `dllmain.cxx` 使用 `__declspec(dllexport)`；
- Debug 配置同时定义 `PERFMETER`；
- 该符号不计入 106 个 `.def` 名称。

没有链接和二进制证据时，不能断言它一定出现在最终 DLL，也不能断言最终拼写或 `DATA` 属性。它足以证明“106”只能称为 `.def` 静态名称数，不能称为实际 DLL 全部导出数。

### 4.3 导出功能域与主要实现位置

下表不重复 106 行完整名称；完整名称清单与已展开签名见 [02a 原生导出调查](investigations/02a-native-export-surface.md)。

| 功能域 | 代表导出 | 主要实现/声明位置 | 当前覆盖状态 |
|---|---|---|---|
| COM/IUnknown | `MILAddRef`、`MILRelease`、`MILQueryInterface` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp`；COM 类型来自 Windows/`src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_render.h` | 定义已定位 |
| 顶层 factory | `MILCreateFactory` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/api_factory.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/api_factory.h`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_render.h` | 定义、公开 prototype 已定位 |
| factory 创建、RT bitmap、WIC 包装 | `MILFactoryCreate*`、`MILRenderTargetBitmap*`、WIC ColorContext proxies | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/api_factory.*`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/common/scanop/bitmapwrappers.cpp` | 关键签名已定位 |
| 软件双缓冲位图 | `MILSwDoubleBufferedBitmap*` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/doublebufferedbitmap.h`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/swlib/doublebufferedbitmap.cpp` | 关键签名/所有权已定位 |
| stream | `MILCreateStreamFromStreamDescriptor`、`MILIStreamWrite` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp`；托管镜像 `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/StreamAsIStream.cs` | 结构和回调已定位 |
| media/event proxy | `MILMedia*`、`MILCreateEventProxy` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/{milav,WmpPlayer,eventproxy,mediaeventproxy,MediaInstance}.*` | 22 项已详细映射 |
| 版本/系统参数 | `MilVersionCheck`、`MILUpdateSystemParametersInfo` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp` | 定义已定位 |
| connection/channel/resource/UCE | `WgxConnection_*`、`MilConnection_*`、`MilChannel_*`、`MilResource_*` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/connection.h`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/clientchannel.h`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/generated_resource_factory.cpp` | 主要家族已定位；逐项签名表待补全 |
| composition 消息/同步 | `MilComposition_*` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp` | 主要定义已定位 |
| 命令播放器/诊断 | `MilPlayer_*`、`SetMilPerfInstrumentationFlags`、`GetNextPerfElementId` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp` | 定义已定位；当前托管可达性不完整 |
| geometry | `MilUtility_PathGeometry*`、polygon/hit-test/area/arc | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/geometry_api.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/*` | 主要定义和 callback typedef 已定位 |
| glyph | `MilGlyphRun_GetGlyphOutline`、release | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp` 及其 DWrite/geometry 依赖 | 定义及配对释放已定位 |
| VisualTarget/Content HWND | `MilVisualTarget_*`、`MilContent_*` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/vt_api.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/rsapi.cpp` | 定义已定位 |
| 3D/像素工具 | `MIL3DCalcProjected2DBounds`、`MilUtility_CopyPixelBuffer` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/common/exports.cpp` | 定义已定位 |
| InteropDeviceBitmap | `InteropDeviceBitmap_*` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/hw/InteropDeviceBitmap.h`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/hw/InteropDeviceBitmap.cpp` | 定义、线程注释和 callback 已定位 |
| RenderOptions | `RenderOptions_*` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/common/renderoptions.cpp` | 定义、BOOL 语义和锁已定位 |

覆盖状态必须透明解释：

- `.def` **名称清单覆盖 106/106**；
- 02a 底稿当前已逐签名详细展开 40 项（COM/factory/bitmap/stream/版本 18 项，media/event 22 项）；
- 本文件又核对了其余主要实现家族和差异名称，但**没有把剩余 66 项全部扩展成逐行签名表**；
- 实际二进制导出覆盖为 **未验证**。

在 Native AOT 大规模翻译前，106 项逐入口签名、定义、所有权和测试映射仍是强制门禁。

## 5. 当前托管调用基线

### 5.1 统计范围

当前基线只统计 PresentationCore 当前项目编译输入及其直接编译的共享 `Common/Graphics` 源文件中：

```csharp
[DllImport(DllImport.MilCore, ...)]
```

不计入：

- `PresentationNative`、DirectWriteForwarder、WindowsCodecs、mscms、ole32 等相邻 DLL；
- `WpfGfx/include` 中未证明进入当前 PresentationCore 编译的历史生成 C#；
- 当前项目未包含的 `GlyphCache.cs`；
- COM vtable 调用、系统 DLL 动态查找和纯托管委托。

当前边界使用传统 `DllImport`；在已核对的 PresentationCore/Common/Shared 根中未发现当前 wpfgfx 主边界使用 `LibraryImport`。

### 5.2 10 个文件、109 个声明出现

| 文件 | 声明出现数 | 主要域 |
|---|---:|---|
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_exports.cs` | 28 | RT bitmap、media、双缓冲位图、系统参数 |
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/exports.cs` | 19 | DUCE connection/channel/resource/消息 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/UnsafeNativeMethodsMilCoreApi.cs` | 43 | 锁、版本、stream、geometry、glyph、COM、factory、WIC、InteropDeviceBitmap、render options |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/SafeNativeMethodsMilCoreApi.cs` | 3 | partition manager、perf ID |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/Composition.cs` | 8 | sync flush、geometry/polygon/area/arc |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/EventProxy.cs` | 1 | `MILCreateEventProxy` |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/MediaContextNotificationWindow.cs` | 2 | Content HWND attach/detach |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/MILUtilities.cs` | 2 | 3D bounds、pixel copy |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/StreamAsIStream.cs` | 1 | `MILIStreamWrite` |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/InterOp/HwndTarget.cs` | 2 | VisualTarget HWND attach/detach |
| **合计** | **109** | 源码声明出现数，重载分别计数 |

### 5.3 唯一 native EntryPoint：107 个

本轮已对 109 个声明逐项按实际 EntryPoint 去重，结果**可靠统计为 107 个唯一 native EntryPoint**。

只有两组重复：

- `MILAddRef`：`SafeMILHandle` 与 `SafeReversePInvokeWrapper` 两个托管重载；
- `MILQueryInterface`：裸 `IntPtr` 与 `SafeMILHandle` 两个托管重载。

未发现其它跨文件或同文件 EntryPoint 重复。该结果仍需后续自动 ABI 清单复核，但不是猜数。

### 5.4 托管方法名与 native EntryPoint 名不同，但不构成集合不一致

代表性显式映射：

- `MilConnection_CloseBatch` → `MilChannel_CloseBatch`；
- `MilConnection_CommitChannel` → `MilChannel_CommitChannel`；
- `MILCopyPixelBuffer` → `MilUtility_CopyPixelBuffer`；
- `VisualTarget_AttachToHwnd` / `VisualTarget_DetachFromHwnd` → `MilVisualTarget_*`；
- `CreateCWICWrapperBitmap` → `MilResource_CreateCWICWrapperBitmap`；
- `MILFactory2.CreateFactory` → `MILCreateFactory`；
- `MILFactory2.CreateMediaPlayer` → `MILFactoryCreateMediaPlayer`；
- `MILFactory2.CreateBitmapRenderTarget` → `MILFactoryCreateBitmapRenderTarget`；
- `MILFactory2.CreateBitmapRenderTargetForBitmap` → `MILFactoryCreateSWRenderTargetForBitmap`；
- `MILUnknown.AddRef/Release/QueryInterface` → `MILAddRef` / `MILRelease` / `MILQueryInterface`。

比较 `.def` 时必须使用 `EntryPoint`，不能使用托管包装方法名。

## 6. `.def` 与当前托管声明的全部已发现集合差异

### 6.1 托管声明有，但 `.def` 无：8 个

| native EntryPoint | 当前静态证据 | 初步分类 | 后续判定门禁 |
|---|---|---|---|
| `MilCompositionEngine_EnterMediaSystemLock` | `UnsafeNativeMethodsMilCoreApi.cs` 有声明；当前 PresentationCore 未见调用；WpfGfx 未见同名定义/导出 | **可能陈旧的历史声明** | 实际 DLL 导出、历史/条件构建、运行可达性 |
| `MilCompositionEngine_ExitMediaSystemLock` | 同上 | **可能陈旧的历史声明** | 同上 |
| `MilGlyphCache_BeginCommandAtRenderTime` | 声明存在；当前项目未包含 `GlyphCache.cs`；WpfGfx 未见定义/导出 | **疑似遗留 glyph cache 路径** | 项目条件、生成协议、实际 DLL |
| `MilGlyphCache_AppendCommandDataAtRenderTime` | 同上 | **疑似遗留 glyph cache 路径** | 同上 |
| `MilGlyphCache_EndCommandAtRenderTime` | 同上 | **疑似遗留 glyph cache 路径** | 同上 |
| `MilGlyphRun_SetGeometryAtRenderTime` | 声明存在但当前调用点未见；WpfGfx 未见定义/导出 | **疑似遗留 render-time glyph 路径** | 实际 DLL、历史/条件构建 |
| `MilCreateReversePInvokeWrapper` | 声明与已编译 `SafeReversePInvokeHandle.cs` 中包装调用存在；当前唯一找到的实例化使用在未编译 `GlyphCache.cs`；WpfGfx 未见定义/`.def` | **可能陈旧或不完整的 reverse-P/Invoke 基础设施** | 实际 DLL、项目条件、运行可达性 |
| `MilReleasePInvokePtrBlocking` | 与上项配对；释放顺序要求先阻塞停止回调，再 `MILRelease` | **可能陈旧或不完整的 reverse-P/Invoke 基础设施** | 同上 |

静态源码没有发现 wpfgfx 的托管 `GetProcAddress` 路径，因此当前没有证据表明这 8 个名称由托管动态查找绕过 `.def`。但只有真实二进制和运行测试才能确认它们是否彻底不可用。

### 6.2 `.def` 有，但当前托管声明未见：7 个

| `.def` 名称 | 原生静态证据 | 初步分类 | 不得做的事 |
|---|---|---|---|
| `MILLoadResource` | `core/api/exports.cpp` 定义；返回可执行文件 IMAGE 资源的借用指针 | **可能供其它 native/external caller；当前 PresentationCore 未见声明** | 不得因无当前 C# 调用方而删除 |
| `MilCompositionEngine_GetComposedEventId` | `core/uce/apifunc.cpp` 定义 | **可能为原生/外部同步设施或历史客户端 API** | 不得擅自认定死代码 |
| `MilCompositionEngine_UpdateSchedulerSettings` | `core/uce/apifunc.cpp` 定义 | **可能为原生/外部调度控制入口** | 不得用当前托管缺席否定 ABI |
| `MilPlayer_Create` | `core/uce/apifunc.cpp` 定义；`PRERELEASE` 下创建 record packet player，常规分支返回 `E_NOTIMPL` | **条件/诊断/原生专用候选** | 不得删除条件行为或改返回码 |
| `MilPlayer_Process` | 与 `MilPlayer_Create` 配对；`PRERELEASE` 处理记录包，常规分支 `E_NOTIMPL` | **条件/诊断/原生专用候选** | 同上 |
| `MilChannel_SetReceiveBroadcastMessages` | `core/uce/apifunc.cpp` 定义并设置 client channel 状态 | **可能供其它 native/历史客户端** | 不得因当前 PresentationCore 未声明而删除 |
| `SetMilPerfInstrumentationFlags` | `core/uce/apifunc.cpp` 定义，写全局性能插桩标志 | **诊断/性能工具或外部 native caller 候选** | 不得混入普通业务重构或删除 |

“当前托管声明未见”不等于“无人使用”。潜在消费者包括：

- 其它 native 模块通过 import library 直接调用；
- 外部诊断/测试/兼容组件；
- 条件或历史构建；
- 运行时动态查找（当前 PresentationCore 未见，但其它组件未穷尽）；
- 与平面导出并存的 COM/native 调用路径。

### 6.3 差异处理规则

1. 在真实二进制导出表、架构、配置和运行可达性确认前，8/7 两组均保持未决。
2. 不得通过从新 DLL 删除 7 个导出、或修改/删除 8 个托管声明来制造“清单一致”。
3. 首次 Native AOT 原型应能显式报告未实现/未验证入口，而不是静默改变调用方。
4. 最终裁决必须记录：实际导出、调用方、配置、架构、返回行为、所有权和测试证据。

## 7. 三类不能混淆的身份、句柄与指针

### 7.1 32 位 DUCE/UCE 协议值

原生 `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_core_types.w`：

```text
HMIL_OBJECT   = UINT32
HMIL_RESOURCE = HMIL_OBJECT
HMIL_CHANNEL  = HMIL_OBJECT
```

托管 `DUCE.ResourceHandle` 以显式布局包装一个 `UInt32`。这类值：

- 是 channel/partition 协议中的 32 位 ID；
- 可写入生成命令包；
- 通过 `MilResource_CreateOrAddRefOnChannel`、duplicate、release、get-ref-count 管理；
- 不能 `MILRelease`，不能装入 `SafeMILHandle`，不能当作地址解引用；
- 同一数值只在所属 channel/partition 语境中有意义。

### 7.2 指针宽度的客户端对象句柄

`DECLARE_MIL_HANDLE(name)` 把 `MIL_CHANNEL` 和 `HMIL_CONNECTION` 定义为不透明结构指针。`connection.h`/`clientchannel.h` 的 `HandleToPointer`、`PointerToHandle` 直接以 `reinterpret_cast` 在句柄和 `CMilConnection*`/`CMilChannel*` 间转换。

这类值：

- 在 x86 为 32 位，在 x64/ARM64 为 64 位；
- 托管侧使用 `IntPtr`；
- `WgxConnection_Disconnect` 对 connection 对象执行 `Release`；
- channel 必须按 close batch → commit → destroy 的协议显式关闭；
- `MIL_CHANNEL` 与 32 位 `HMIL_CHANNEL` 仅名称相近，身份完全不同。

`HMIL_PLAYER` 是 `void*`，也属于指针宽度对象句柄，但当前 `.def` 对应入口主要是条件/诊断路径。

### 7.3 COM/native 对象指针

`IUnknown*`、`IMILCoreFactory*`、`IMILMedia*`、`IWICBitmap*`、`CInteropDeviceBitmap*` 等是指针宽度对象身份：

- 创建、QI 或 getter 可能返回拥有引用；
- `MILAddRef`/`MILRelease`/`MILQueryInterface` 桥接 COM 规则；
- `MILQueryInterface` 成功返回一份新引用；
- 托管通常由 `SafeMILHandle`、`SafeMediaHandle`、`BitmapSourceSafeMILHandle` 或显式 `MILRelease` 接管；
- SafeHandle 的 GC memory pressure 计数不是 native COM 引用数。

### 7.4 其它容易误判的“句柄”

- `GCHandle`：托管对象上下文 token，字段宽度随指针宽度；由 descriptor dispose 回调释放，不是 native COM 指针。
- HWND/HANDLE/event handle：Win32 句柄，按各自 API 释放；不是 MIL resource handle。
- generated command 中的 `UInt64 hwnd`、`hEvent`、`pInteropDeviceBitmap`、`pIDWriteFont` 等是**固定 64 位协议槽位**，不能简单替换为平台大小 `IntPtr` 字段。
- `MILLoadResource` 返回的资源地址是借用视图，不应包装成拥有内存的 SafeHandle。

## 8. 调用约定、链接名与架构

### 8.1 原生侧

- `wpfgfx.vcxproj` 支持 Win32、x64、ARM64。
- `Wpf.Cpp.props` 对原生 x86 默认设置 stdcall（`/Gz`）。
- `WINAPI`、`CALLBACK` 和 COM `STDMETHODCALLTYPE` 在 x86 上均表达 stdcall 语义。
- `MILAPI` 只是 `WINAPI` 别名，不是自动导出属性。
- `.def` 的 106 个公开名均未装饰。
- x64/ARM64 使用平台统一调用约定，不采用传统 x86 `_Name@N` stdcall 公开装饰，但 C/C++ 链接名和导出解析仍须验证。

`MILCreateFactory` 有清楚的 `extern "C"` + `WINAPI` prototype。许多 `core/uce` 入口显式写 `WINAPI`。`core/api/exports.cpp` 中若干平面包装没有独立公开 prototype，只依赖项目默认调用约定和既有链接行为；这是 x86 Native AOT 原型的高风险点。

### 8.2 托管侧

当前 109 个 MilCore 声明：

- 没有发现显式 `CallingConvention`；依赖 `DllImport` 的 Winapi 默认行为；
- 没有发现显式 `CharSet`；
- 没有发现显式 `ExactSpelling`；
- 没有发现 `SetLastError = true`；
- 绝大多数保持默认 `PreserveSig=true`。

三个明确 `PreserveSig=false` 的当前声明：

- `MILSwDoubleBufferedBitmapGetBackBuffer`；
- `MILSwDoubleBufferedBitmapAddDirtyRect`；
- `MilUtility_CopyPixelBuffer`。

其它重要 marshalling 例外：

- `MILMediaOpen` 的字符串显式为 `UnmanagedType.BStr`；
- `MILIStreamWrite` 的 `byte[]` 使用 `LPArray` 和 `SizeParamIndex=2`；
- 多数 geometry/command 入口直接使用 pinned 指针。

后续不得为了“规范化”而统一添加属性。必须先获取实际托管元数据和各架构解析行为，尤其验证未显式 `ExactSpelling`/`CharSet` 时是否发生名称探测，以及它与未装饰 `.def` 名的关系。

### 8.3 x86 强制门禁

Native AOT 替换必须在 x86 单独验证：

- 每个入口的调用约定；
- 参数字节数和栈清理；
- 公开名是否完全未装饰；
- delegate/function pointer 的调用约定；
- COM vtable 与平面导出不可混为一层；
- `bool`、`BOOL`、结构返回和 64 位按值参数的 ABI。

## 9. 结构、布局与 marshalling 高风险清单

| 风险项 | 静态事实 | 关键源路径 | 必须验证 |
|---|---|---|---|
| `CStreamDescriptor` | 14 个 `__stdcall` 回调，末尾 `DWORD_PTR m_handle`；native 按值复制 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp`；`src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/StreamAsIStream.cs` | x86/x64/ARM64 `sizeof`、每字段 offset、delegate 约定、GCHandle 宽度、回调异常 |
| `CEventProxyDescriptor` | 2 个 `__stdcall` 回调 + `DWORD_PTR`；native 按值复制 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/eventproxy.h`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/eventproxy.cpp`；`src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/EventProxy.cs` | 布局、保活、锁、析构 dispose、进程退出重入 |
| `AVEventData` | `align(4)`；4 个 32 位头字段后接 `WCHAR[1]` 变长尾；最大 packet 4096 字节 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_av_types.h`；`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/mediaeventproxy.cpp` | header/tail offset、`sizeof`、UTF-16 长度、NUL、越界和跨线程解析 |
| generated command | 大量 `Explicit, Pack=1`；命令 ID/offset 固定；BOOL 为 `UInt32`；若干指针槽为 `UInt64` | `src/Microsoft.DotNet.Wpf/src/Common/Graphics/Generated/wgx_commands.cs`；`src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.h`；`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/marshal_generated.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/renderdata_generated.cpp` | 每命令大小/offset、固定 64 位槽、可变尾数据、native consumer |
| 路径/几何生成结构 | `MilPathGeometry`、figure/segment 有显式 offset、ForcePacking、枚举/flags | `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_core_types.cs`；`src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.h`；`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/geometry_api.cpp` | pack、枚举宽度、数组计数、点/段边界、ForcePacking |
| `bool` | C++ `bool` 通常 1 字节；media 和若干 UCE 参数直接使用 `bool`/`bool*` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/*`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp` | 当前 CLR marshalling 的真实位宽；Native AOT 不得照抄 C# `bool` 猜测 |
| `BOOL` | Win32 32 位整数；RenderOptions、D3DImage callback、geometry callback、stream CanWrite/CanSeek 使用 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/common/renderoptions.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/hw/InteropDeviceBitmap.h`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/geometry_api.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp` | 参数/返回 4 字节、非零规范化、托管 delegate/返回值 |
| GUID/REFIID | C++ 引用 ABI 是指向 16 字节 GUID 的地址 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_render.h`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp` | by-ref/by-value、对齐、所有权 |
| RECT/MILRect/WICRect | 四个 32 位字段，但有有符号/无符号转换和不同语义 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp`、`src/Microsoft.DotNet.Wpf/src/Common/Graphics/Generated/wgx_commands.cs`、Win32 declarations | 大小、字段顺序、负数、溢出、空矩形 |
| 数组和裸指针 | geometry、pixel copy、command、glyph 广泛传 `byte*`、`double*`、数组地址 | `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/Composition.cs`、`src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/UnsafeNativeMethodsMilCoreApi.cs`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/*` | pinning 生命周期、同步消费、长度单位、越界 |
| `size_t`/指针大小 | `MilComposition_PeekNextMessage` 的 size 参数托管为 `IntPtr` | `src/Microsoft.DotNet.Wpf/src/Common/Graphics/exports.cs`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp` | x86/64 位大小和符号性 |
| 字符串 | media 使用 BSTR；资源加载使用 LPCWSTR；AV packet 使用 UTF-16 尾数组 | `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_exports.cs`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/mediaeventproxy.cpp` | 分配者/释放者、NUL、字符数与字节数 |
| union/显式布局 | back-channel message 等使用 `Explicit, Pack=1` union | `src/Microsoft.DotNet.Wpf/src/Common/Graphics/exports.cs`、`src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.h` | union 大小、最大成员、保留字段、错误码 |

任何布局在真实 native `sizeof/offsetof` 探针和托管 `Marshal.SizeOf/OffsetOf` 对照前，不得标记为兼容。

## 10. 回调、reverse P/Invoke 与线程

### 10.1 StreamAsIStream

- `StreamDescriptor` 中 14 个 delegate 由静态 `StaticPtrs` 强引用，防止函数指针被 GC 回收。
- descriptor 的 `GCHandle` 强引用 `StreamAsIStream`。
- native `CManagedStreamWrapper` 复制 descriptor，并在析构时调用 `pfnDispose(&m_sd)` 释放 GCHandle。
- Read/Write/Seek/Stat 等托管实现捕获异常并转换为 HRESULT。
- callback 可能由 native codec/stream 消费线程进入；当前静态源码不能承诺统一 UI 线程。
- 不能把该 ABI 改成普通托管接口，不能提前释放 delegate/GCHandle。

### 10.2 EventProxy/media

- `EventProxyDescriptor` 的两个 delegate 由静态字段保活，GCHandle 强引用 wrapper。
- `CMediaEventProxy::EventItem::Run` 在 AV event thread 组装 `AVEventData` 并调用 `CEventProxy::RaiseEvent`。
- native proxy 在锁内检查 shutdown；shutdown 后抑制新回调并返回成功。
- 托管 `RaiseEvent` 捕获所有异常，以 `Marshal.GetHRForException` 返回 HRESULT；目标弱引用失效返回 `E_HANDLE`。
- native 析构对 CLR 进程关闭期间的 `E_PROCESS_SHUTDOWN_REENTRY` 有专门 SEH 过滤，避免 shutdown reverse-P/Invoke 导致进程崩溃。

### 10.3 D3DImage/InteropDeviceBitmap

- `FrontBufferAvailableCallback` 存在实例字段 `_availableCallback` 中以保活。
- native 回调从 composition/render thread 进入；托管 callback 只封装参数并 `Dispatcher.BeginInvoke` 回 UI 线程。
- 更换 back buffer、finalizer 和 AppDomain shutdown 都先调用 `InteropDeviceBitmap_Detach`，停止后续 callback。
- native callback 参数使用 `BOOL`，不是 1 字节 C++ `bool`。

### 10.4 geometry callback

- native `AddFigureToList` 明确为 `CALLBACK`，参数中的两个布尔是 `BOOL`。
- `MilUtility_PathGeometryWiden/Outline/Flatten/Combine` 在 P/Invoke 调用期间同步遍历结果并回调；当前实现没有保存函数指针到调用后。
- 托管 delegate 未长期存储，依赖上述同步性。
- callback 返回 `void`，而托管构造 PathFigure 的代码可能抛异常。后续必须专门验证当前 CLR 的异常行为，并设计不让异常跨 ABI 的等价封锁；不得未经基线就静默吞异常或改变外层 HRESULT。

### 10.5 reverse-P/Invoke wrapper 差异

`MilCreateReversePInvokeWrapper` / `MilReleasePInvokePtrBlocking` 的托管包装要求：

1. 创建 native wrapper；
2. 释放时先阻塞，确保没有正在执行或将进入的 callback；
3. 再 `MILRelease` wrapper COM/native 对象。

但这两个 EntryPoint 当前不在 `.def`，WpfGfx 也未定位定义，唯一实例化使用又位于未编译 `GlyphCache.cs`。它们必须作为 ABI 差异门禁处理，不能直接照搬成新导出，也不能删除托管基础设施后声称问题已解决。

### 10.6 回调统一约束

- delegate/function pointer 必须有明确调用约定；省略属性的历史行为也要验证。
- delegate、上下文和 native owner 的寿命必须组成一个可证明的所有权闭环。
- detach/shutdown 必须先阻止新回调，再释放上下文和对象。
- HRESULT 回调必须在内部捕获异常并转换；`void` 回调必须有经差分测试确认的封锁策略。
- 不得在 native 锁内引入新的 Dispatcher 同步等待或托管锁顺序。

## 11. 所有权、释放与错误传播

### 11.1 主要所有权规则

| 对象/返回值 | 创建或取得 | 当前释放/归还 | 关键约束 |
|---|---|---|---|
| MIL factory | `MILCreateFactory` | 最后一个 `FactoryMaker` 调用 `MILRelease` | 托管静态实例计数与 native COM 引用不同 |
| QI 结果 | `MILQueryInterface` | `SafeMILHandle` 或显式 `MILRelease` | 成功结果拥有一份新引用；out 先清零 |
| 普通 MIL/WIC 对象 | factory/getter/create | `SafeMILHandle` / `BitmapSourceSafeMILHandle` → `MILRelease` | getter 是否 AddRef 必须逐项确认 |
| media object | factory create | `SafeMediaHandle`: `MILMediaShutdown` → `MILRelease` | 顺序不可倒置或合并 |
| stream wrapper | `MILCreateStreamFromStreamDescriptor` | COM Release 触发 native 析构，再 descriptor dispose | GCHandle 由 native owner 终结时释放 |
| event proxy | `MILCreateEventProxy` | COM Release；native 析构回调 dispose | media 通过 QI 建立独立引用 |
| connection | `WgxConnection_Create` | `WgxConnection_Disconnect` → native `Release` | 指针宽度 handle，不是协议 ID |
| channel | `MilConnection_CreateChannel` | close batch → commit → destroy | 托管无 finalizer，必须显式关闭 |
| DUCE resource | create/addref/duplicate | `MilResource_ReleaseOnChannel` | 32 位、按 channel 计数，不走 COM |
| D3D interop bitmap | `InteropDeviceBitmap_Create` | 先 `Detach` 阻止回调/释放 user surface，再 SafeMILHandle release | `_pUserSurfaceUnsafe` 是不 AddRef 的托管辅助裸指针 |
| glyph path data | `MilGlyphRun_GetGlyphOutline` | `MilGlyphRun_ReleasePathGeometryData` | 分配/释放必须配对 |
| `MILLoadResource` 地址 | `FindResource/LockResource` | 无配对释放 | 借用模块资源内存，不能 free |

### 11.2 媒体的三类结束动作

三者不能合并：

1. `MILMediaClose`：关闭当前媒体内容/播放状态，不释放 `IMILMedia` 对象。
2. `MILMediaShutdown`：终结 player 状态、打破引用环；由 `SafeMediaHandle` 在最终 release 前调用。
3. `MILMediaProcessExitHandler`：仅关闭 event proxy 的 reverse callback 通路；用于 ProcessExit，不等于 close 或 shutdown。

### 11.3 错误传播

- 大部分 HRESULT 由托管显式 `HRESULT.Check` 转换为异常。
- 三个 `PreserveSig=false` 入口由 P/Invoke 层把失败 HRESULT 转异常。
- 某些返回值是 `void` 或 `BOOL`，不能擅自改成 HRESULT。
- 当前 MilCore 声明未使用 `SetLastError=true`；不得把相邻 mscms/user32 声明的 Win32 error 规则套入 wpfgfx。
- callback 异常必须在 callback 内转换，不能穿过 native 栈。
- Native AOT 导出中的任何异常都必须封锁，并映射为原实现的 HRESULT/BOOL/void 失败策略。
- `SafeMediaHandle.ReleaseHandle` 当前调用 `HRESULT.Check`；这在 finalizer/SafeHandle 路径具有异常风险，但属于现有事实，初次迁移不得擅自改成忽略错误。
- attach 和 MediaSystem 部分初始化没有完整回滚；不能用“更安全”的新回滚掩盖原行为，必须先差分验证。

## 12. Native AOT 导出薄层不可违反的检查表

> 本轮只定义门禁，**不实现**。

每个拟迁移入口必须逐项满足：

- [ ] 公开导出名与目标架构真实 DLL 基线完全一致。
- [ ] x86 明确验证 stdcall、参数字节数、栈清理和未装饰公开名。
- [ ] x64/ARM64 明确验证平台 ABI、结构传递和名称。
- [ ] 参数顺序、位宽、有符号性、指针级别、返回值和可空性与原实现一致。
- [ ] `bool`、`BOOL`、enum、GUID、RECT、HRESULT、size_t 分别使用已验证表示。
- [ ] 所有结构、union、固定/尾随数组、pack/align 先通过布局探针。
- [ ] 32 位 DUCE handle、指针宽度 client handle、COM pointer 使用不同类型，禁止隐式互换。
- [ ] COM AddRef/Release/QI、创建返回和 getter 所有权逐项保持。
- [ ] SafeHandle、显式 close、detach、shutdown 和 ProcessExit 顺序保持。
- [ ] 回调调用约定、线程、delegate 保活、GCHandle 和阻塞释放保持。
- [ ] 托管异常在导出或 callback 内封锁，不越过 ABI。
- [ ] HRESULT/BOOL/void/PreserveSig 行为与现有调用方一致。
- [ ] `DllImport` 的历史默认元数据未经验证不得“规范化”。
- [ ] 导出层只转换和转发，不放置业务算法、缓存、线程模型或跨模块重构。
- [ ] 每个入口保留原生文件到 C# 文件的一一可追溯关系。
- [ ] 对 8/7 差异项保持显式未决状态，不能靠改清单消除。
- [ ] 在替换现有 DLL 前，实际二进制导出、布局、失败码、线程、释放和端到端差分全部通过。

## 13. 首批 ABI 原型与兼容验证入口顺序

以下顺序按低风险到高风险排列；每一步均应同时对原生 DLL 与 Native AOT 候选执行差分验证。

1. **文件身份与导出清单门禁**
   - 验证实际文件名、架构、加载路径和导出名；
   - 记录 Debug/Release 是否出现 `g_fNoMeterChecks`；
   - 不调用复杂子系统。
2. **`MilVersionCheck`**
   - 正确版本 `0x200184C0` 应成功；
   - 错误版本应返回原生相同的 `WGXERR_UNSUPPORTEDVERSION`；
   - 验证 x86/x64/ARM64 调用约定。
3. **`MILCreateFactory` 最小 COM 创建/释放**
   - 创建 factory，验证空参数、错误 SDK、成功输出和一次最终 release；
   - 不先引入渲染或媒体。
4. **`MILAddRef` / `MILRelease` / `MILQueryInterface`**
   - 在已创建 factory 或受控 COM 对象上验证引用计数变化、QI 新引用、`E_NOINTERFACE`、空 out 清零；
   - 验证 SafeMILHandle 接管。
5. **低状态全局入口**
   - `RenderOptions_*`、`MILUpdateSystemParametersInfo`、`GetNextPerfElementId`；
   - 验证 BOOL 宽度、锁和无 HRESULT/有 HRESULT 差异。
6. **connection 创建/断开**
   - `WgxConnection_Create(false/true)` 与 `WgxConnection_Disconnect`；
   - 验证 CrossThread/SameThread、指针宽度 handle 和引用释放。
7. **channel 生命周期**
   - create → get marshal type → close batch → commit → destroy；
   - 覆盖无效句柄、重复关闭和错误顺序。
8. **资源与命令协议**
   - create/addref、duplicate、get refcount、release；
   - begin/append/end 和 `MilResource_SendCommand*`；
   - 先使用最小固定大小命令，再覆盖可变数据、back-channel 消息和 sync flush。
9. **bitmap/WIC/RT 边界**
   - factory bitmap render target、render-target bitmap getter、双缓冲位图、WIC proxy；
   - 验证 COM 引用、RECT、GUID、PreserveSig 和像素复制。
10. **同步 geometry callback**
    - 从 `MilUtility_ArcToBezier` 等无 callback 入口开始；
    - 再验证 Widen/Outline/Flatten/Combine 的 `CALLBACK`、BOOL、数组和异常封锁。
11. **长期回调与线程**
    - StreamAsIStream descriptor；
    - EventProxy + AVEventData；
    - D3DImage composition-thread callback/detach；
    - 最后再进入 media player 状态线程和 ProcessExit。
12. **首个真实 PresentationCore 端到端入口**
    - 当批次 8-9 的 connection/channel、资源命令和软件渲染前置已就绪后，使用真实 PresentationCore 创建 `DrawingVisual`，绘制确定性图形，调用 `RenderTargetBitmap.Render`，读取像素并与原生 wpfgfx 基线比较；
    - 该路径覆盖首次 DllImport/attach、MediaContext、SameThread connection/channel、命令/资源、软件或可控渲染和 WIC bitmap 输出；
    - 该门对应 `E2E-05`，是软件渲染批次和真实 PresentationCore 主链收口的强制条件，但不是批次 1-3 或早期纯逻辑逐文件翻译的前置。未通过前不得把软件链、真实 WPF 集成或候选 DLL描述为已贯通。

媒体播放、D3D9 用户 surface、设备丢失和真实 HWND 呈现应在上述闭环后逐步加入，因为它们同时引入外部设备、线程、回调和退出时序。

## 14. 已确认事实

1. PresentationCore 当前 `DllImport.MilCore` 静态展开为 `wpfgfx_cor3.dll`。
2. `WpfGfx_v0400.dll` 是不同命名空间/历史边界中的常量，不是当前 PresentationCore 调用常量。
3. 原始 `.def` 有 106 个名称，未显式使用 DATA/PRIVATE/NONAME/ordinal/forwarder/alias/条件块。
4. Debug 源码有 `.def` 外 `g_fNoMeterChecks` 数据导出候选。
5. 当前基线有 10 个源文件、109 个 MilCore `DllImport` 声明出现。
6. 109 个声明可靠人工去重为 107 个唯一 EntryPoint；重复只来自 `MILAddRef` 和 `MILQueryInterface` 的重载。
7. `.def` 与托管唯一 EntryPoint 的交集为 99，双向差异为 8/7，具体名称已完整列出。
8. 当前托管没有发现 wpfgfx 显式动态加载；首次任意 MilCore P/Invoke 可触发 DLL attach。
9. 常规 MediaContext 路径中 `MilContent_AttachToHwnd` 早于 `MilVersionCheck`。
10. 原生显式卸载执行完整 shutdown，进程终止 detach 默认跳过完整 shutdown。
11. `HMIL_RESOURCE/HMIL_CHANNEL` 是 32 位协议值；`MIL_CHANNEL/HMIL_CONNECTION` 是指针宽度客户端对象句柄。
12. `CStreamDescriptor`、`CEventProxyDescriptor`、`AVEventData` 和 generated command 是首批布局门禁。
13. media close、shutdown、process-exit handler 是三种不同动作。
14. 本轮没有枚举实际二进制、执行构建或运行测试。

## 15. 推断及置信度

| 推断 | 置信度 | 原因 | 重新评估触发条件 |
|---|---|---|---|
| 8 个托管侧独有名称多数是历史/遗留路径 | 中高 | 无当前调用或仅关联未编译 GlyphCache，WpfGfx 未见定义/`.def` | 实际 DLL 出现名称、条件项目纳入或运行调用成功 |
| 7 个 `.def` 侧独有名称主要供 native、诊断、条件或外部客户端 | 中高 | 均有原生定义，其中 MilPlayer 明确含 PRERELEASE 分支 | 找到当前托管/动态调用或实际外部消费者 |
| 99 个共同名称是当前替换 DLL 的最小显式托管兼容面 | 高 | 当前托管声明与 `.def` 同名交集 | 实际二进制缺失、额外转发/别名或配置差异 |
| `RenderTargetBitmap.Render` 是首个较低外部依赖的端到端入口 | 高 | 覆盖真实 PresentationCore/UCE/bitmap 路径，避免媒体和用户 D3D surface | 原生基线显示该路径仍依赖不可控硬件/环境 |

## 16. 风险与未决问题

- x86 默认 stdcall、C/C++ 内部修饰名与 `.def` 未装饰名之间的真实链接规则未闭环。
- media native `bool`/`bool*` 与托管默认 `bool` marshalling 可能存在位宽风险。
- 没有显式 `ExactSpelling`/`CharSet` 的 P/Invoke 名称解析必须按真实运行时验证。
- descriptor delegate 未显式标注 `UnmanagedFunctionPointer`，其历史默认约定必须固化。
- generated command 的固定 `UInt64` 指针槽在 x86 上仍是协议 64 位字段，容易被误改为 `IntPtr`。
- COM、connection/channel、DUCE resource 和 Win32 handle 容易被统一 SafeHandle 抽象错误合并。
- ProcessExit、Dispatcher shutdown、finalizer、callback detach 和原生 process-termination detach 的全局时序未知。
- `SafeMediaHandle.ReleaseHandle` 可能在 release 路径抛异常；现有行为需要专门基线。
- 部分初始化失败没有本地回滚，迁移者容易无意改变行为。
- 02a 尚未逐签名完成全部 106 项；大规模翻译前必须补齐。

## 17. 因无命令行禁令尚未验证的事项

- 预处理后的 `wpfgfx.i` 内容。
- Win32/x64/ARM64 Debug/Release 的实际 DLL 文件名和导出表。
- 实际 ordinal、forwarder、alias、数据导出和名称装饰。
- `g_fNoMeterChecks` 是否真正进入 Debug 二进制。
- 任何结构的 native `sizeof/offsetof` 和托管 `Marshal.SizeOf/OffsetOf` 对照。
- 109 个声明在最终程序集中的实际 P/Invoke 元数据。
- 每个 EntryPoint 的运行时可达性、首次加载顺序和 loader search path。
- 回调线程、异常行为、delegate 保活、卸载和进程退出时序。
- COM 引用计数、失败码、部分初始化和设备丢失行为。
- 原生与 Native AOT 候选的单元、差分、集成和端到端结果。

这些项目不得在文档、计划或实现状态中描述为“已验证”。

## 18. 对总体迁移计划的强制输入

总体计划必须吸收以下约束：

1. 以 106 `.def` 名称、107 托管唯一 EntryPoint、99 交集和 8/7 差异建立版本化 ABI 台账。
2. 第一阶段先建立真实二进制清单、x86/x64/ARM64 导出/布局探针和最小 Native AOT 薄层，不先翻译渲染算法。
3. 逐文件台账必须记录每个导出的原生定义、C# 镜像路径、签名、调用约定、所有权、线程、失败码和测试状态。
4. shared/util/DllUtil、common/shared、core/common 生命周期基础设施必须早于上层 API 大规模迁移。
5. `resources <-> uce`、`sw <-> hw <-> av` 的循环通过类型/签名骨架和逐文件填充处理，不通过重构消除。
6. 结构、generated command、回调和 COM/handle 模型是跨模块前置，不得分散到后期补救。
7. 显式卸载与进程终止差异、媒体三类结束动作、D3DImage/EventProxy detach 必须进入早期生命周期测试。
8. 任何清单差异的最终处理必须由真实二进制和运行测试决定，并形成书面决策。
9. 等价迁移达到门槛前，禁止性能优化、现代化抽象和改变并发/缓存/错误处理。

## 19. 后续维护规则

1. `.def`、托管 `DllImport`、原生 prototype/定义、生成协议或 DLL 常量任一变化时，必须同步更新本文件和对应 investigation。
2. 每次更新数量必须注明范围、配置、架构、证据方法；不得只改总数。
3. 维护四份并列清单：`.def`、源码定义/声明、托管 EntryPoint、实际二进制导出。
4. 8/7 差异项在被真实证据裁决前不得从文档移除；状态只能从“未决”变为“已验证保留/已验证不可达/已验证条件项”等有证据状态。
5. investigation 保留过程信息；本文件只吸收稳定结论和门禁，不重写调查历史。
6. 每个 Native AOT 导出完成时，应附对应原生文件、ABI 测试、失败路径、所有权和差分证据链接。
7. 任何为了“让清单一致”而删除导出、改 P/Invoke、改参数类型或改调用约定的提案，必须先被阻止并转为二进制/运行验证任务。
8. 达到端到端等价门槛后，重构和优化必须另立计划、另设基线，不与翻译提交混合。

---

本文件是后续迁移的入口，不是实际二进制事实的替代品。若本文件、两份 investigation、真实 DLL 或运行测试之间出现冲突，必须保留冲突、升级证据等级并重新验证；不得选择最方便实现的一侧作为“真相”。
