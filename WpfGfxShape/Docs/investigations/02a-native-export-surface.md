# wpfgfx 原生导出 ABI 调查

> 状态：历史调查底稿；只完成部分逐签名细化，稳定结论已吸收到 `../02-abi-and-managed-callers.md` 与 `../08-abi-manifest-spec.md`；不作为当前任务入口  
> 范围：`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.def` 所声明的原生导出面及其直接 ABI 类型来源  
> 最高原则：正确性/ABI 优先、逐原生文件近似直译、等价迁移完成前禁止重构  
> 约束来源：`WpfGfxShape/Docs/00-migration-charter.md`

## 1. 范围与方法

### 1.1 调查范围

- 完整读取并人工核对 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.def` 的每一个静态导出条目。
- 逐导出或逐同签名/同职责导出组定位：原生定义、声明头、调用约定宏、返回类型、参数、主要职责、COM/所有权/句柄/回调语义。
- 调查与导出 ABI 直接相关的 `WpfGfx/Include`、`Include/Generated`、`include/processed` 及共享 C# 生成输出。
- 识别名称不一致、别名、转发、ordinal、条件导出、宏生成和仅声明未定位项。
- 为后续 Native AOT `[UnmanagedCallersOnly]` 或等价薄导出层提供约束输入，但不设计最终项目、不改写 API。

### 1.2 明确范围外

- 不修改任何现有 WPF 源码、项目配置或 `WpfGfxShape/Code`。
- 不创建 Native AOT 项目，不翻译生产实现，不重构原 API。
- 不构建、不运行测试、不预处理 `.def`，不枚举实际 DLL 导出/导入。
- 不把 `PresentationNative`、`milctrl` 或 `wpfx` 的导出混入 `wpfgfx` 导出面。

### 1.3 工具与验证限制

本轮绝对禁止任何命令行、终端、脚本或通过命令间接执行，包括 PowerShell、cmd、bash、Python、批处理、dotnet、msbuild、dumpbin、link 和脚本统计。只使用 IDE 项目/文件搜索、grep/code search、符号导航、文件读取与本文档写入。

因此本文使用以下证据标签：

- **事实—文件**：由已读取的仓库文件直接确认。
- **事实—静态清单**：由 `.def` 文本直接确认，但未证明实际二进制结果。
- **推断（高/中/低）**：由命名、声明、引用或实现综合得出，仍需后续验证。
- **未验证—受禁令限制**：需要预处理、构建、链接、二进制枚举、布局探针或运行测试。
- **未定位**：在本轮允许的静态搜索范围内尚未找到定义或必要声明。

特别声明：本文**不声称已枚举实际 `wpfgfx` 二进制导出表**。下述数量仅是对原始 `.def` 中 `EXPORTS` 段静态名称条目的人工、文档化统计。

## 2. 已读取/待读取的关键文件

| 文件 | 用途 | 状态 |
|---|---|---|
| `WpfGfxShape/Docs/00-migration-charter.md` | 调查边界、ABI 优先、逐文件直译、工具禁令 | 已读取全文 |
| `WpfGfxShape/Docs/01-native-source-topology.md` | 原生项目、生成协议、链接聚合、x86 默认 stdcall 背景 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.def` | 静态导出主清单 | 已读取全文（141 行） |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.vcxproj` | `.def -> .i`、最终链接边界 | 拓扑文档已有静态证据；本专题待按 ABI 需要复核 |
| `eng/WpfArcadeSdk/tools/GenerateModuleDefinitionFile.targets` | `.def` 预处理协议 | 已读取全文 |
| `eng/WpfArcadeSdk/tools/Wpf.Cpp.props` | x86 默认调用约定和架构宏 | 已读取全文 |
| `eng/WpfArcadeSdk/tools/VersionSuffix.props` | 当前 native 目标后缀 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/Graphics.props` | Debug `PERFMETER`、公共编译与架构条件 | 已读取 ABI 相关区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/std.h` | Windows/COM 基础头、匿名 union 与 pack 风险说明 | 已读取 ABI 相关区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_render.h` | `MILAPI`、`extern "C"`、COM 接口与公开声明 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/shared/util/DllUtil/{Precomp.h,dllmain.cxx,dllmainimpl.cxx}` | DLL 入口调用约定及 Debug 数据导出候选 | 已读取相关全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/exports.cpp` | COM、工厂包装、媒体包装、双缓冲位图、stream、event proxy、WIC 代理 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/api_factory.{h,cpp}` | `MILCreateFactory` 与工厂虚方法实现 | 已读取 ABI 相关区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/doublebufferedbitmap.h`、`core/sw/swlib/doublebufferedbitmap.cpp` | 双缓冲位图对象、AddRef 输出和 back buffer 所有权 | 已读取 ABI 相关区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/common/scanop/bitmapwrappers.cpp` | render-target bitmap 到 `IWICBitmap` 包装 | 已读取相关实现 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/StreamAsIStream.cs` | `CStreamDescriptor` 托管镜像与委托存活期 | 已读取 ABI 相关区段 |
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_exports.cs`、`PresentationCore/.../UnsafeNativeMethodsMilCoreApi.cs` | 托管 P/Invoke 签名交叉证据 | 已读取本阶段相关区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp` | 连接/通道/资源/组合 API 候选 | 待读取 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/rsapi.cpp` | 渲染目标/位图 API 候选 | 待读取 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/geometry_api.cpp` | 几何 API 候选 | 待读取 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/vt_api.cpp` | visual/content target API 候选 | 待读取 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_render.h` | `IMILMedia`、`IMILEventProxy` COM vtable 声明 | 已读取相关接口全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/{milav,WmpPlayer,eventproxy,mediaeventproxy,MediaInstance}.{h,cpp}` | 媒体对象选择、实际方法、event 反向回调与线程 | 已读取 ABI 相关区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_av_types.h`、`src/Common/Graphics/wgx_av_types.cs` | AV 事件 ID 与 native packet 布局/托管枚举 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/{EventProxy,mediaeventshelper,safemediahandle,MediaPlayerState}.cs` | event descriptor 镜像、异常映射、SafeHandle 关闭和主要调用方 | 已读取 ABI 相关区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/Include`、`Include/Generated`、`include/processed` | ABI 声明与生成类型 | 待系统读取 |

## 3. `.def` 静态导出清单

### 3.1 文件级事实

- `LIBRARY DLL_NAME`：库名是预处理宏，而非在原始 `.def` 中写死的字面名称。
- `EXPORTS` 段共有 **106 个名称条目**。该数字由逐行人工编号与分类小计交叉核对得到；不是脚本统计，也不是实际二进制导出数。
- 原始 `.def` 中未见 `DATA`、`PRIVATE`、`NONAME`、显式 ordinal（如 `@123`）、转发语法（如 `Name=OtherDll.Target`）或显式别名（如 `Public=Internal`）。
- 原始 `.def` 中未见 `#if`/`#ifdef` 等条件导出；但文件会经 C++ 预处理生成 `wpfgfx.i`，预处理后的实际文本本轮未生成、未验证。
- 原始条目均写为未装饰名称；不同架构下链接器如何匹配源码符号，尤其 x86 stdcall 装饰，必须结合声明、编译属性与后续二进制验证，不能仅由 `.def` 断言。

### 3.2 按域分类的完整清单

> 覆盖规则：下表每个名称只出现一次；分类小计总和为 106。

| 编号范围 | 域 | 数量 | `.def` 静态条目（保持原拼写） |
|---:|---|---:|---|
| 001–003 | COM/IUnknown 薄包装 | 3 | `MILAddRef`；`MILRelease`；`MILQueryInterface` |
| 004 | 顶层工厂 | 1 | `MILCreateFactory` |
| 005–008 | 工厂创建与资源加载 | 4 | `MILFactoryCreateMediaPlayer`；`MILFactoryCreateSWRenderTargetForBitmap`；`MILFactoryCreateBitmapRenderTarget`；`MILLoadResource` |
| 009–010 | RenderTargetBitmap 操作 | 2 | `MILRenderTargetBitmapGetBitmap`；`MILRenderTargetBitmapClear` |
| 011–032 | 媒体与事件代理 | 22 | `MILMediaOpen`；`MILMediaStop`；`MILMediaClose`；`MILMediaGetPosition`；`MILMediaSetPosition`；`MILMediaSetVolume`；`MILMediaSetBalance`；`MILMediaSetIsScrubbingEnabled`；`MILMediaSetRate`；`MILMediaHasVideo`；`MILMediaHasAudio`；`MILMediaCanPause`；`MILMediaIsBuffering`；`MILMediaGetBufferingProgress`；`MILMediaGetDownloadProgress`；`MILMediaGetNaturalHeight`；`MILMediaGetNaturalWidth`；`MILMediaGetMediaLength`；`MILMediaNeedUIFrameUpdate`；`MILMediaShutdown`；`MILCreateEventProxy`；`MILMediaProcessExitHandler` |
| 033–036 | 软件双缓冲位图 | 4 | `MILSwDoubleBufferedBitmapCreate`；`MILSwDoubleBufferedBitmapGetBackBuffer`；`MILSwDoubleBufferedBitmapAddDirtyRect`；`MILSwDoubleBufferedBitmapProtectBackBuffer` |
| 037–038 | IStream/流描述符 | 2 | `MILCreateStreamFromStreamDescriptor`；`MILIStreamWrite` |
| 039 | 系统参数刷新 | 1 | `MILUpdateSystemParametersInfo` |
| 040 | 版本检查 | 1 | `MilVersionCheck` |
| 041–054 | 组合引擎、连接与通道生命周期 | 14 | `MilCompositionEngine_EnterCompositionEngineLock`；`MilCompositionEngine_ExitCompositionEngineLock`；`MilCompositionEngine_GetComposedEventId`；`MilConnection_CreateChannel`；`MilConnection_DestroyChannel`；`MilChannel_CommitChannel`；`MilChannel_CloseBatch`；`WgxConnection_Create`；`WgxConnection_ShouldForceSoftwareForGraphicsStreamClient`；`WgxConnection_SameThreadPresent`；`WgxConnection_Disconnect`；`MilCompositionEngine_InitializePartitionManager`；`MilCompositionEngine_UpdateSchedulerSettings`；`MilCompositionEngine_DeinitializePartitionManager` |
| 055–056 | 命令播放器 | 2 | `MilPlayer_Create`；`MilPlayer_Process` |
| 057–058 | TileBrush/像素复制工具 | 2 | `MilUtility_GetTileBrushMapping`；`MilUtility_CopyPixelBuffer` |
| 059 | 组合同步刷新 | 1 | `MilComposition_SyncFlush` |
| 060–061 | 组合消息队列 | 2 | `MilComposition_PeekNextMessage`；`MilComposition_WaitForNextMessage` |
| 062–065 | 资源句柄引用管理 | 4 | `MilResource_CreateOrAddRefOnChannel`；`MilResource_DuplicateHandle`；`MilResource_ReleaseOnChannel`；`MilResource_GetRefCountOnChannel` |
| 066–075 | 通道命令编组与特殊资源发送 | 10 | `MilChannel_GetMarshalType`；`MilChannel_SetReceiveBroadcastMessages`；`MilResource_SendCommand`；`MilChannel_BeginCommand`；`MilChannel_AppendCommandData`；`MilChannel_EndCommand`；`MilResource_CreateCWICWrapperBitmap`；`MilResource_SendCommandBitmapSource`；`MilResource_SendCommandMedia`；`MilChannel_SetNotificationWindow` |
| 076–087 | 几何工具 | 12 | `MilUtility_ArcToBezier`；`MilUtility_PathGeometryWiden`；`MilUtility_PathGeometryOutline`；`MilUtility_GetPointAtLengthFraction`；`MilUtility_PathGeometryCombine`；`MilUtility_PathGeometryFlatten`；`MilUtility_PolygonBounds`；`MilUtility_PathGeometryBounds`；`MilUtility_PolygonHitTest`；`MilUtility_PathGeometryHitTest`；`MilUtility_PathGeometryHitTestPathGeometry`；`MilUtility_GeometryGetArea` |
| 088 | 性能插桩标志 | 1 | `SetMilPerfInstrumentationFlags` |
| 089–090 | VisualTarget/HWND | 2 | `MilVisualTarget_AttachToHwnd`；`MilVisualTarget_DetachFromHwnd` |
| 091–092 | Content/HWND | 2 | `MilContent_AttachToHwnd`；`MilContent_DetachFromHwnd` |
| 093 | 3D 投影边界 | 1 | `MIL3DCalcProjected2DBounds` |
| 094–097 | InteropDeviceBitmap | 4 | `InteropDeviceBitmap_Create`；`InteropDeviceBitmap_Detach`；`InteropDeviceBitmap_AddDirtyRect`；`InteropDeviceBitmap_GetAsSoftwareBitmap` |
| 098–099 | Glyph outline | 2 | `MilGlyphRun_GetGlyphOutline`；`MilGlyphRun_ReleasePathGeometryData` |
| 100–102 | WIC ColorContext 代理 | 3 | `IWICColorContext_GetProfileBytes_Proxy`；`IWICColorContext_GetType_Proxy`；`IWICColorContext_GetExifColorSpace_Proxy` |
| 103 | 性能元素 ID | 1 | `GetNextPerfElementId` |
| 104–106 | 进程级 RenderOptions | 3 | `RenderOptions_ForceSoftwareRenderingModeForProcess`；`RenderOptions_IsSoftwareRenderingForcedForProcess`；`RenderOptions_EnableHardwareAccelerationInRdp` |
|  | **合计** | **106** | 人工分类总和：`3+1+4+2+22+4+2+1+1+14+2+2+1+2+4+10+12+1+2+2+1+4+2+3+1+3 = 106` |

### 3.3 `.def` 属性初判

| 属性 | 原始 `.def` 静态结果 | 说明 |
|---|---|---|
| `DATA` | 未见 | 106 项均按函数候选处理；仍需源码确认没有同名数据符号异常 |
| `PRIVATE` | 未见 | 静态清单未要求阻止 import library 暴露 |
| 显式别名 | 未见 | 未见 `Export=Internal`；后续仍需检查源码名/宏展开是否不一致 |
| 转发导出 | 未见 | 未见 `OtherDll.Symbol` 形式 |
| ordinal | 未见 | 没有显式 `@n`；是否由其它链接输入产生 ordinal 不在本文已确认范围 |
| `NONAME` | 未见 | 所有条目均有文本名称 |
| 条件导出 | 原始文件未见 | `.def` 仍经过 C++ 预处理；未验证中间 `.i` |
| 架构专属条目 | 原始文件未见 | 同一清单意图用于多架构；实际符号匹配和装饰待验证 |

### 3.4 `.def` 外的静态导出候选

`grep` 在 WpfGfx 源码树中只找到一个 `__declspec(dllexport)` 名称：`g_fNoMeterChecks`。

- `src/Microsoft.DotNet.Wpf/src/WpfGfx/shared/util/DllUtil/Precomp.h` 在 `DBG` 分支声明 `extern __declspec(dllexport) BOOL g_fNoMeterChecks;`。
- `src/Microsoft.DotNet.Wpf/src/WpfGfx/shared/util/DllUtil/dllmain.cxx` 在 Debug 分支写有 `__declspec(dllexport) BOOL g_fNoMeterChecks;`。
- `src/Microsoft.DotNet.Wpf/src/WpfGfx/shared/util/UtilLib/MemUtils.cxx` 在 `PERFMETER` 下提供初始化为 `FALSE` 的同名 `BOOL`；`Graphics.props` 对 Debug 定义 `PERFMETER=1`。

这形成一个**Debug 条件、数据符号、`.def` 外导出候选**。它不计入第 3.2 节的 106 个 `.def` 条目，也不能在没有链接与二进制证据时断言一定出现在最终 DLL、采用何种导出名或是否带 `DATA` 属性。它证明“106 个 `.def` 名称”不能被写成“实际 DLL 的全部导出数”。本轮未发现其它 WpfGfx `__declspec(dllexport)` 或源码级 `/EXPORT:` pragma；搜索结果仍不等价于链接器最终事实。

## 4. 导出到实现映射表

> 调查进行中。状态枚举：`定义已定位`、`声明已定位`、`仅引用已定位`、`未定位`、`待调查`。

| 导出/导出组 | 原生定义文件 | 声明/ABI 头 | 返回类型与参数 | 调用约定/链接名 | 主要职责 | 状态 |
|---|---|---|---|---|---|---|
| 全部 106 项 | 待按域展开 | 待按域展开 | 待按域展开 | `.def` 仅给出未装饰公开名 | 见第 3.2 节 | 待调查 |

### 4.1 COM 基础与工厂/位图/流/版本（001–010、033–040）

> 本表覆盖 18 个 `.def` 条目。除 `MILCreateFactory` 外，本组多数平面包装在已搜索原生头中没有找到先行声明；“声明”列因此也记录直接相关的 COM/类声明与托管 P/Invoke 作为次级证据。

| `.def` 导出 | 定义 | 声明/相关类型 | 原生签名（SAL 从略） | 主要职责与所有权 | 调用约定/状态 |
|---|---|---|---|---|---|
| `MILAddRef` | `core/api/exports.cpp` | `IUnknown`；托管 `UnsafeNativeMethodsMilCoreApi.cs::MILUnknown` | `ULONG (IUnknown*)` | 不做空检查，直接调用 `IUnknown::AddRef` 并返回新引用计数 | 定义未显式 `WINAPI`/`extern "C"`；x86 项目默认 stdcall；定义已定位，原生平面声明未定位 |
| `MILRelease` | `core/api/exports.cpp` | `IUnknown`；`SafeMILHandle.ReleaseHandle` 最终调用它 | `ULONG (IUnknown*)` | 不做空检查，直接调用 `Release`；托管 `ReleaseInterface` 先判空、调用后清零指针 | 同上；定义已定位，原生平面声明未定位 |
| `MILQueryInterface` | `core/api/exports.cpp` | `IUnknown`/`REFIID`；托管有裸指针和 `SafeMILHandle` 两个重载 | `HRESULT (IUnknown*, REFIID, void**)` | 检查对象和 out 指针，将 `*ppvObject=NULL` 后调用 QI；成功结果按 COM 规则带一份新引用 | 同上；定义已定位，原生平面声明未定位 |
| `MILCreateFactory` | `core/api/api_factory.cpp` | `include/wgx_render.h`；`core/api/api_factory.h` | `HRESULT WINAPI (IMILCoreFactory**, UINT SDKVersion)` | 校验 out 指针和 `MIL_SDK_VERSION`，创建 `CMILFactory`，把已 AddRef 的接口所有权交给调用方；失败清理临时引用 | `wgx_render.h` 的 `extern "C"` 声明 + `WINAPI`；定义与声明已定位 |
| `MILFactoryCreateMediaPlayer` | `core/api/exports.cpp` | `IMILCoreFactory::CreateMediaPlayer`（`wgx_render.h`/`api_factory.h`） | `HRESULT (IMILCoreFactory*, IUnknown* pEventProxy, bool canOpenAnyMedia, IMILMedia**)` | 检查 factory；转调 COM 虚方法。虚方法对 event proxy 做 `QI(IID_IMILEventProxy)`，再创建媒体对象；输出由 `SafeMediaHandle` 拥有 | 平面定义未显式约定/无平面头；定义已定位 |
| `MILFactoryCreateSWRenderTargetForBitmap` | `core/api/exports.cpp` | `IMILCoreFactory::CreateSWRenderTargetForBitmap` | `HRESULT (IMILCoreFactory*, IWICBitmap*, IMILRenderTargetBitmap**)` | 检查 factory；把调用方 WIC bitmap 包装为 `IWGXBitmap`，创建软件 render-target bitmap，返回拥有引用的 COM 指针 | 平面定义未显式约定/无平面头；定义已定位 |
| `MILFactoryCreateBitmapRenderTarget` | `core/api/exports.cpp` | `IMILCoreFactory::CreateBitmapRenderTarget` | `HRESULT (IMILCoreFactory*, UINT width, UINT height, MilPixelFormat::Enum, FLOAT dpiX, FLOAT dpiY, MilRTInitialization::Flags, IMILRenderTargetBitmap**)` | 检查 factory；虚方法验证非零尺寸、正 DPI、flags 和仅支持的像素格式；当前软件路径创建 `CSwRenderTargetBitmap`，HardwareOnly 返回未实现 | 平面定义未显式约定/无平面头；定义已定位 |
| `MILLoadResource` | `core/api/exports.cpp` | 未找到原生平面声明；未找到当前 C# 调用方 | `HRESULT (LPCWSTR src, LPBYTE* memPtr, long* size)` | 从 `FindResource(NULL, src, L"IMAGE")` 加载**进程可执行文件**资源，返回 `LockResource` 指针与 `SizeofResource`；返回指针是资源内存借用视图，不由调用方释放 | 定义未显式约定；定义已定位，声明/调用方未定位 |
| `MILRenderTargetBitmapGetBitmap` | `core/api/exports.cpp` | `IMILRenderTargetBitmap`（`wgx_render.h`）；托管 `MILRenderTargetBitmap.GetBitmap` | `HRESULT (IMILRenderTargetBitmap*, IWICBitmap**)` | 从 render target 取得 AddRef 的 `IWGXBitmap`，创建 `CWGXWrapperBitmap`（自身 AddRef 且持有底层引用），以拥有引用的 `IWICBitmap*` 返回 | 平面定义未显式约定/无平面头；定义已定位 |
| `MILRenderTargetBitmapClear` | `core/api/exports.cpp` | `IMILRenderTargetBitmap`/`IMILRenderTarget` | `HRESULT (IMILRenderTargetBitmap*)` | 建立 24-bit FPU guard，QI 到 render target，清为全零透明色，再释放临时 QI 引用 | 平面定义未显式约定/无平面头；定义已定位 |
| `MILSwDoubleBufferedBitmapCreate` | `core/api/exports.cpp` | `core/sw/doublebufferedbitmap.h`；托管 `wgx_exports.cs` | `HRESULT (UINT, UINT, double, double, REFWICPixelFormatGUID, IWICPalette*, CSwDoubleBufferedBitmap**)` | WIC pixel-format GUID 转 MIL enum；创建成对前/后缓冲对象。`Create` 内部将已 AddRef 的对象交给调用方，托管用 `SafeMILHandle` 释放 | 平面定义未显式约定；定义和对象声明已定位 |
| `MILSwDoubleBufferedBitmapGetBackBuffer` | `core/api/exports.cpp` | `CSwDoubleBufferedBitmap::GetBackBuffer` | `HRESULT (const CSwDoubleBufferedBitmap*, IWICBitmap**, UINT*)` | `SetInterface` 对 back buffer AddRef 后返回，并返回缓冲区字节数；托管声明 `PreserveSig=false`，失败会转异常 | 平面定义未显式约定；定义和对象声明已定位 |
| `MILSwDoubleBufferedBitmapAddDirtyRect` | `core/api/exports.cpp` | `MILRect`=`WICRect`；`CSwDoubleBufferedBitmap::AddDirtyRect` | `HRESULT (CSwDoubleBufferedBitmap*, const MILRect*)` | 将有符号 `X/Y/Width/Height` 分别安全转换为 `UINT`，构造 `CMilRectU` 后加入 dirty list；负数/转换失败沿 HRESULT 返回 | 平面定义未显式约定；托管 `PreserveSig=false` |
| `MILSwDoubleBufferedBitmapProtectBackBuffer` | `core/api/exports.cpp` | `CSwDoubleBufferedBitmap::ProtectBackBuffer` | `HRESULT (CSwDoubleBufferedBitmap*)` | 对 back buffer 启用写保护语义，错误原样返回 | 平面定义未显式约定；定义已定位 |
| `MILCreateStreamFromStreamDescriptor` | `core/api/exports.cpp` | 本文件局部 `CStreamDescriptor`；托管 `StreamAsIStream.cs::StreamDescriptor` | `HRESULT (CStreamDescriptor*, IStream**)` | 复制描述符到 `CManagedStreamWrapper`，返回已 AddRef 的 `IStream*`；包装对象析构时调用复制描述符中的 `pfnDispose(&m_sd)` 释放托管 `GCHandle` | 平面定义未显式约定；定义与托管镜像已定位，独立原生头未定位 |
| `MILIStreamWrite` | `core/api/exports.cpp` | `IStream::Write`；托管 `StreamAsIStream.cs` | `HRESULT (IStream*, const void* buf, ULONG cb, ULONG* cbWritten)` | 判空 stream 后直接调用 COM `IStream::Write`；托管以 `byte[]` + `SizeParamIndex=2` 传入 | 平面定义未显式约定；定义与托管声明已定位 |
| `MILUpdateSystemParametersInfo` | `core/api/exports.cpp` | `g_DisplayManager`；托管 `wgx_exports.cs` | `HRESULT ()` | 仅调用 `g_DisplayManager.ScheduleUpdate()`，随后固定返回 `S_OK` | 平面定义未显式约定；定义已定位，原生头未定位 |
| `MilVersionCheck` | `core/uce/apifunc.cpp` | 生成 `include/wgx_sdk_version.h` 与共享 `wgx_sdk_version.cs` | `HRESULT WINAPI (UINT uiCallerMilSdkVersion)` | 比较调用方协议指纹与 `MIL_SDK_VERSION`；相等为 `S_OK`，否则 `WGXERR_UNSUPPORTEDVERSION`。当前双方签入值均为 `0x200184C0` | 显式 `WINAPI`；定义已定位，独立原生声明未定位 |

#### 4.1.1 `CStreamDescriptor` 回调 ABI

native `CStreamDescriptor` 在 `exports.cpp` 内按以下顺序包含 **14 个 `__stdcall` 函数指针**，末尾为一个 `DWORD_PTR m_handle`：

1. `void Dispose(void* pSD)`；
2. `HRESULT Read(void* pSD, void* buf, ULONG cb, ULONG* cbRead)`；
3. `HRESULT Seek(void* pSD, LARGE_INTEGER offset, DWORD origin, ULARGE_INTEGER* newPos)`；
4. `HRESULT Stat(void* pSD, STATSTG* statstg, DWORD statFlag)`；
5. `HRESULT Write(void* pSD, const void* buf, ULONG cb, ULONG* cbWritten)`；
6. `HRESULT CopyTo(void* pSD, IStream* stream, ULARGE_INTEGER cb, ULARGE_INTEGER* cbRead, ULARGE_INTEGER* cbWritten)`；
7. `HRESULT SetSize(void* pSD, ULARGE_INTEGER newSize)`；
8. `HRESULT Commit(void* pSD, DWORD commitFlags)`；
9. `HRESULT Revert(void* pSD)`；
10. `HRESULT LockRegion(void* pSD, ULARGE_INTEGER offset, ULARGE_INTEGER cb, DWORD lockType)`；
11. `HRESULT UnlockRegion(void* pSD, ULARGE_INTEGER offset, ULARGE_INTEGER cb, DWORD lockType)`；
12. `HRESULT Clone(void* pSD, IStream** stream)`；
13. `HRESULT CanWrite(void* pSD, BOOL* canWrite)`；
14. `HRESULT CanSeek(void* pSD, BOOL* canSeek)`。

托管镜像使用 `[StructLayout(LayoutKind.Sequential)]`，按相同字段顺序声明 14 个 delegate，最后是 `GCHandle m_handle`；`StaticPtrs` 静态持有全部 delegate，避免函数指针被 GC 回收。该结构的每架构 `sizeof`、字段偏移、delegate 默认 Winapi 约定和 `GCHandle` 字段宽度必须做布局/回调探针验证。不得把它改成普通托管接口或把回调合并，因为 native 会复制结构并在 COM 包装对象整个存活期内反向调用。

#### 4.1.2 本组名称与覆盖异常

- `wgx_render.h` 注释提到较新的 `MILCreateFactory2`，但当前仓库搜索只找到该注释，没有找到定义、声明或 `.def` 条目；本专题不把它计入当前导出面。
- `MILLoadResource` 只定位到定义和 `.def`，当前仓库 C# 搜索未找到调用方；这只能标为“调用方未定位”，不能据此删除。
- 多数本组平面函数没有单独原生头声明；托管 P/Invoke 与实现共同构成当前静态签名证据。后续主 ABI 文档应把“缺少共享 native prototype”列为 x86/自动化校验风险。

### 4.2 媒体与事件代理（011–032）

> 本表覆盖 22 个 `.def` 条目。21 个 `MILMedia*` 平面函数定义在 `core/api/exports.cpp`，均先检查 `THIS_PTR`，随后转调 `IMILMedia` COM vtable；`MILCreateEventProxy` 也在同一文件。`IMILMedia`/`IMILEventProxy` 的权威 native 接口声明位于 `include/wgx_render.h`，COM 方法使用 `STDMETHOD`/`STDMETHODCALLTYPE`。

| `.def` 导出 | 原生签名（平面入口） | vtable/主要实现 | 主要职责与语义 | 状态/ABI 备注 |
|---|---|---|---|---|
| `MILMediaOpen` | `HRESULT (IMILMedia*, LPOLESTR src)` | `IMILMedia::Open(LPCWSTR)`；`CWmpPlayer::Open` | 托管以 BSTR 传 URL；实现比较当前 URL，不变则不重复排队；变化时重置位置/进度/长度/尺寸，复制字符串到 native heap，再把 open 操作加入状态引擎 | 定义、接口、真实实现、托管声明已定位；native 不保留调用方 BSTR 指针 |
| `MILMediaStop` | `HRESULT (IMILMedia*)` | `CWmpPlayer::Stop` | 将目标动作设为 Stop 并异步加入 WMP 状态引擎 | 已定位 |
| `MILMediaClose` | `HRESULT (IMILMedia*)` | `CWmpPlayer::Close` | 重置共享状态与当前 URL，生成 close 更新并加入状态引擎；不同于释放 COM 对象 | 已定位 |
| `MILMediaGetPosition` | `HRESULT (IMILMedia*, LONGLONG*)` | `CWmpPlayer::GetPosition` | 同步请求 transient 更新，代码传入超时值 `10`；超时时返回缓存的 timed-out position，否则返回共享位置；单位为 100ns tick | 已定位；64 位整数 out |
| `MILMediaSetPosition` | `HRESULT (IMILMedia*, LONGLONG)` | `CWmpPlayer::SetPosition` | 接收 100ns tick，内部除以 `10,000,000` 转 WMP 秒数，同时更新超时回退位置并排队 seek | 已定位；64 位按值 |
| `MILMediaSetVolume` | `HRESULT (IMILMedia*, double)` | `CWmpPlayer::SetVolume` | 要求 `[0,1]`；乘 100 加 0.5 后转 WMP `LONG`，排队应用 | 已定位 |
| `MILMediaSetBalance` | `HRESULT (IMILMedia*, double)` | `CWmpPlayer::SetBalance` | 要求 `[-1,1]`；乘 100 加 0.5 后转 native `long`，排队应用 | 已定位；负数舍入行为需原样保留 |
| `MILMediaSetIsScrubbingEnabled` | `HRESULT (IMILMedia*, bool)` | `CWmpPlayer::SetIsScrubbingEnabled` | 把 native C++ `bool` 状态写入 update state 并排队 | 已定位；1 字节 C++ bool 与默认 P/Invoke bool 是高风险验证点 |
| `MILMediaSetRate` | `HRESULT (IMILMedia*, double)` | `CWmpPlayer::SetRate` | 设置播放速率目标并排队；已读实现未在该方法内做数值范围检查 | 已定位 |
| `MILMediaHasVideo` | `HRESULT (IMILMedia*, bool*)` | `CWmpPlayer::HasVideo` | 从共享状态返回是否有视频 | 已定位；native `bool*` |
| `MILMediaHasAudio` | `HRESULT (IMILMedia*, bool*)` | `CWmpPlayer::HasAudio` | 从共享状态返回是否有音频 | 已定位；native `bool*` |
| `MILMediaCanPause` | `HRESULT (IMILMedia*, bool*)` | `CWmpPlayer::CanPause` | 从共享状态返回能否暂停；实现显式检查 out 指针 | 已定位；native `bool*` |
| `MILMediaIsBuffering` | `HRESULT (IMILMedia*, bool*)` | `CWmpPlayer::IsBuffering` | 从共享状态返回 buffering 标志 | 已定位；native `bool*`，已读方法未显式判空 |
| `MILMediaGetBufferingProgress` | `HRESULT (IMILMedia*, double*)` | `CWmpPlayer::GetBufferingProgress` | 同步 transient 更新；超时则返回缓存值，否则共享值 | 已定位 |
| `MILMediaGetDownloadProgress` | `HRESULT (IMILMedia*, double*)` | `CWmpPlayer::GetDownloadProgress` | 同步 transient 更新；超时则返回缓存值，否则共享值 | 已定位 |
| `MILMediaGetNaturalHeight` | `HRESULT (IMILMedia*, UINT*)` | `CWmpPlayer::GetNaturalHeight` | 返回共享状态中的视频自然高度 | 已定位 |
| `MILMediaGetNaturalWidth` | `HRESULT (IMILMedia*, UINT*)` | `CWmpPlayer::GetNaturalWidth` | 返回共享状态中的视频自然宽度 | 已定位 |
| `MILMediaGetMediaLength` | `HRESULT (IMILMedia*, LONGLONG*)` | `CWmpPlayer::GetMediaLength` | 返回媒体长度，注释明确单位为 100ns tick | 已定位 |
| `MILMediaNeedUIFrameUpdate` | `HRESULT (IMILMedia*)` | `CWmpPlayer::NeedUIFrameUpdate` | 通知状态引擎需要一次 UI frame 更新 | 已定位 |
| `MILMediaShutdown` | `HRESULT (IMILMedia*)` | `CWmpPlayer::Shutdown` | 标记 player 已 shutdown，并通过状态线程调用状态引擎 shutdown 以打破引用环；析构函数断言此前已 shutdown | 已定位；`SafeMediaHandle.ReleaseHandle` 先调用它，再 `MILRelease` |
| `MILCreateEventProxy` | `HRESULT (CEventProxyDescriptor*, CEventProxy**)` | `CEventProxy::Create` | 复制两个回调和 user handle，初始化锁，返回 refcount=1 的 `IMILEventProxy` 实现；托管 `SafeMILHandle` 接管 | 定义与 native/managed descriptor 已定位；独立平面 prototype 未定位 |
| `MILMediaProcessExitHandler` | `HRESULT (IMILMedia*)` | `CWmpPlayer::ProcessExitHandler`；Debug fake player 同语义 | 只关闭 `CMediaEventProxy/CEventProxy` 的反向通知通路，使后续 event callback 被抑制；不是完整 player shutdown | 已定位；由 AppDomain `ProcessExit` 弱引用 helper 调用 |

#### 4.2.1 真实与条件媒体实现

- 正常路径：`MILFactoryCreateMediaPlayer` → `IMILCoreFactory::CreateMediaPlayer` → 对传入 event proxy 执行 `QI(IID_IMILEventProxy)` → `CMILAV::CreateMedia` → `MediaInstance::Create` → `CMILAV::ChoosePlayer` → `CWmpPlayer::Create`。
- `CMILAV::ChoosePlayer` 通过 `SetInterface(*ppMedia, pRealAvPlayer)` 向调用方返回新增引用，随后释放局部引用；托管 `SafeMediaHandle` 拥有最终引用。
- **条件实现**：仅在嵌套 `DBG` + `PRERELEASE` 编译分支中，注册表 `EnableFakePlayerPresenter` 可选择 `CFakePP`；同一 21 个 `MILMedia*` 导出仍通过 `IMILMedia` vtable 工作。故映射应写成“主要实现 `CWmpPlayer`，条件实现 `CFakePP`”，不能声称导出只绑定一个具体类。
- 临时托管 `SafeMILHandle unmanagedProxy` 可在 `CreateMediaPlayer` 返回后释放其初始引用，因为 native `QI`/`MediaInstance` 链已对 `CEventProxy` 建立自己的引用。

#### 4.2.2 `CEventProxyDescriptor` 与反向 P/Invoke

native `CEventProxyDescriptor` 字段顺序为：

1. `void (__stdcall *pfnDispose)(void* pEPD)`；
2. `HRESULT (__stdcall *pfnRaiseEvent)(void* pEPD, void* pb, ULONG cb)`；
3. `DWORD_PTR m_handle`。

托管 `EventProxyDescriptor` 使用 `[StructLayout(LayoutKind.Sequential)]`，以两个 delegate 和一个 `GCHandle` 镜像；静态 `EventProxyStaticPtrs` 强引用 delegate，descriptor 中的 `GCHandle` 强引用 `EventProxyWrapper`。`CEventProxy` 把 descriptor **按值复制**，因此这些函数指针和 handle 必须在整个 native proxy 生命周期内稳定。

回调线程与封锁语义：

- `CMediaEventProxy::RaiseEvent` 创建 `EventItem` 并加入共享 AV event thread；`EventItem::Run` 在该 event thread 上组包并调用 `CEventProxy::RaiseEvent`，不是直接在 WMP/调用方线程上进入托管。
- `CEventProxy::RaiseEvent` 持有 `m_lock`；`Shutdown` 在同一锁下设置 `m_isShutdown=true`。shutdown 后的新回调被静默抑制并返回 `S_OK`。
- 托管 `EventProxyWrapper.RaiseEvent` 捕获所有 `Exception`，以 `Marshal.GetHRForException` 返回 HRESULT；目标弱引用失效时返回 `E_HANDLE`。异常不得穿越 reverse-P/Invoke ABI。
- `CEventProxy` 析构调用 `pfnDispose(&m_epd)` 释放托管 `GCHandle`。原生代码专门用 SEH 吞掉 CLR 进程关闭期间的 `E_PROCESS_SHUTDOWN_REENTRY` reverse-P/Invoke 异常；其它结构化异常继续搜索。Native AOT 替代必须有等价的进程退出安全策略，不能无条件在卸载期回调托管。

#### 4.2.3 AV event packet 布局

`include/processed/wgx_av_types.h` 定义：

- `AVEvent`：值 0–13；共享 C# `wgx_av_types.cs` 当前值逐项一致。
- `AVEventData`：`__declspec(align(4))`，顺序为四个 32 位字段 `DWORD avEvent`、`HRESULT errorHResult`、`ULONG typeLength`、`ULONG paramLength`，随后 `WCHAR typeAndParamStrings[1]` 尾部数组。
- 无字符串事件仍发送 `sizeof(AVEventData)`；托管解析只要求最小 16 字节并容忍更大 packet。
- script command 的两个长度均是不含 NUL 的 UTF-16 code-unit 数；native 使用尾部数组和 `sizeof(AVEventData)` 为两个 NUL 留空间，packet 上限为 4096 字节。托管按两个 `UInt32` 长度读取精确数量的 UTF-16 字符。

这是 align、尾部数组、变长 packet 与跨线程回调组合风险，必须建立 native/managed packet 差分测试，不能改成普通序列化对象而跳过原二进制格式。

## 5. 调用约定与架构

### 5.1 构建与 `.def` 生成事实

- `wpfgfx.vcxproj` 明确支持 `Win32`、`x64`、`arm64` 的 Debug/Release 配置，且 `CLRSupport=false`。
- native 目标名为 `wpfgfx$(WpfVersionSuffix)`；`VersionSuffix.props` 当前把后缀设为 `_cor3`，所以静态属性意图是 `wpfgfx_cor3`。是否当前环境最终采用同一属性值仍属构建验证项。
- 原始 `.def` 的 `LIBRARY DLL_NAME` 通过 `/DDLL_NAME=$(TargetName)` 展开。`GenerateModuleDefinitionFile.targets` 当前无条件把 `UseTwoStepModuleDefinitionGenerator` 设为 `true`：先把 `.def` 作为 C++ 预处理输入生成 `wpfgfx.i`，禁用 PCH、禁止把它作为对象参与链接，再在链接前移动到中间目录。
- `wpfgfx.vcxproj` 的链接器 `ModuleDefinitionFile` 指向中间目录 `wpfgfx.i`。本文没有生成或读取该中间文件。

### 5.2 调用约定宏

| 表达 | 静态定义/来源 | ABI 含义 |
|---|---|---|
| `MILAPI` | `include/wgx_render.h`：`#define MILAPI WINAPI` | 只是 `WINAPI` 的别名；当前已读导出定义并未统一使用此宏，不能把 `MILAPI` 当作自动导出属性 |
| `WINAPI` | Windows SDK `minwindef.h` 在当前 MSVC 条件下定义为 `__stdcall` | x86 上影响参数清栈和名称装饰；x64/ARM64 使用平台统一调用约定，但源码语义仍应保留 |
| `CALLBACK` | 同一 Windows SDK 头定义为 `__stdcall` | `geometry_api.cpp::AddFigureToList` 等回调必须按 stdcall/Winapi 建模 |
| `STDMETHODCALLTYPE` | Windows COM 宏语义为 `__stdcall` | `DECLARE_INTERFACE_`/`STDMETHOD` 形成 COM vtable ABI；它与平面导出函数 ABI 是两层契约 |
| 项目默认调用约定 | `Wpf.Cpp.props` 对非托管 x86 设置 `CallingConvention=StdCall` | 即使定义未显式写 `WINAPI`，x86 编译仍意图采用 `/Gz` 默认 stdcall；函数级显式约定可覆盖它 |

### 5.3 C/C++ 链接与未装饰公开名

- `.def` 的 106 个名字全部是未装饰公开名。
- `include/wgx_render.h` 在 `extern "C"` 区域声明 `HRESULT WINAPI MILCreateFactory(...)`，`api_factory.cpp` 的同名定义会继承该先行声明的 C 链接和 `WINAPI` 调用约定；这是目前静态闭环最清楚的例子。
- `core/dll/dllentry.cpp` 对 `DllMain` 显式使用 `extern "C"` + `__stdcall`；`wpfgfx.vcxproj` 对 x86 入口点显式追加 `@12`。这直接证明该构建重视 x86 stdcall 装饰，但 DLL 入口不是第 3.2 节的公开 API 条目。
- 多数 `core/uce`、几何、render-options 定义显式写 `WINAPI`，但没有发现单独的原生声明头；其 C/C++ 链接名如何由 `.def` 解析仍待链接器证据。
- `core/api/exports.cpp` 中已见的 `MILAddRef`、`MILRelease`、`MILQueryInterface`、工厂包装等定义既未显式写 `WINAPI`，也未在静态头搜索中找到先行声明。x86 项目默认 stdcall可解释调用约定意图，却不能单独证明 C 链接或最终内部修饰名。原工程显然依赖编译器/链接器与 `.def` 的既有配合，但本轮禁止生成链接命令、map、`.lib` 或二进制导出，故**不猜测其精确解析机制**。
- x64/ARM64 不使用传统 x86 `_Name@N` stdcall 形式，但 C++ 名称修饰、`.def` 内部名解析以及 Native AOT 最终公开名仍必须逐架构验证。

### 5.4 托管调用约定交叉证据

已抽查的 PresentationCore/WpfGfx 共享 C# 声明通常省略 `CallingConvention`，因此依赖 `DllImport` 的 `Winapi` 默认值；这与 native `WINAPI`/x86 默认 stdcall 的意图一致。它不消除以下风险：

- 原生 `bool` 与 Win32 `BOOL` 宽度不同；不能因两者在 C# 都写成 `bool` 就视为同一 ABI。
- `PreserveSig=false` 会把 native `HRESULT` 转为托管异常，Native AOT 边界仍必须返回原 HRESULT，而不是改成 `void`。
- 未显式 `ExactSpelling`、`CharSet` 或 `CallingConvention` 的历史 P/Invoke 元数据本身也是兼容输入，后续须逐声明交叉核对。

## 6. 类型与布局风险

当前已确认的首批高风险类型：

- `bool`：native C++ `bool` 通常是 1 字节；本阶段出现在 `MILFactoryCreateMediaPlayer`。不能与 Win32 `BOOL` 混用。
- `BOOL`：32 位 Win32 整数布尔；本阶段出现在 stream descriptor 的 `CanWrite`/`CanSeek` 回调。
- `ULONG`/`UINT`/`DWORD`/`long`：在 Windows ABI 中均为 32 位，但 C# 对应类型的有符号性不同；`MILLoadResource` 特别使用 `long* size`，不是跨平台 C/C++ `long` 推断。
- `LONGLONG`/`LARGE_INTEGER`/`ULARGE_INTEGER`：固定 64 位，且部分按值穿越回调边界。
- `DWORD_PTR`：指针宽度；`CStreamDescriptor.m_handle` 必须在 x86 与 64 位架构分别验证。
- `REFIID`/`REFWICPixelFormatGUID`：C++ 引用形式在 ABI 上是指向 16 字节 GUID 的地址；Native AOT 签名不可按值猜测。
- `MILRect` 是 `WICRect` typedef，四个有符号 32 位字段；与托管 `Int32Rect` 的顺序和大小需布局测试。
- `MilPixelFormat::Enum`、`MilRTInitialization::Flags`：C++ enum/flags 基础宽度需与生成 C# `int`/`uint` 声明逐项确认。
- `CStreamDescriptor`：14 个函数指针 + 指针宽度句柄，Sequential 托管镜像；是本专题首个必须逐架构做 `sizeof`/offset 与反向调用测试的结构。
- `CEventProxyDescriptor`：2 个 stdcall 函数指针 + `DWORD_PTR`；native 按值复制，托管以 Sequential delegate + `GCHandle` 镜像。
- `AVEventData`：显式 `align(4)`，四个 32 位头字段后接 `WCHAR[1]` 尾部数组；实际 packet 可变长且限制为 4096 字节。
- 媒体平面 ABI 直接使用 C++ `bool`/`bool*`，而现有 P/Invoke 未显式标 `[MarshalAs(UnmanagedType.I1)]`。当前系统显然依赖既有 CLR marshalling 行为，但其精确 native 位宽匹配必须由专门探针验证；Native AOT 入口不可仅照抄 C# `bool` 而不固化实际 ABI。

仍待调查并分类：其它固定宽度整数、句柄 typedef、结构体 pack/align、union、bitfield、固定/尾随数组、生成命令结构和共享 C# 镜像。

## 7. COM、所有权、句柄与回调语义

### 7.1 本阶段已确认

- `MILAddRef`/`MILRelease` 不接受空指针保护；托管包装必须在调用前负责判空。返回值分别是 COM 引用计数结果，`MILRelease` 的托管声明虽然使用 `int`，当前调用方有意忽略它。
- `MILQueryInterface` 对空对象或空 out 指针返回参数错误，并在真正 QI 前把输出清零；成功输出拥有一份 QI 新引用。
- `MILCreateFactory`、工厂创建 render target/media、双缓冲位图创建、render-target bitmap 包装和 stream wrapper 均向调用方交出拥有引用的 COM 风格指针；当前托管调用方以 `SafeMILHandle`/`SafeMediaHandle` 或显式 `MILRelease` 接管。
- `MILLoadResource` 返回的是模块资源内存的借用指针，没有配对释放导出；不得包装为需释放的 native allocation。
- `MILSwDoubleBufferedBitmapGetBackBuffer` 通过 `SetInterface` 对返回的 `IWICBitmap` AddRef；托管 `BitmapSourceSafeMILHandle` 必须释放。
- `CStreamDescriptor` 中的 delegate 必须存活到 native `CManagedStreamWrapper` 析构；析构会调用 `pfnDispose`，托管实现由此释放 `GCHandle`。这是 native 驱动的终结回调，不可提前释放或只依赖普通委托局部变量。
- 所有 stream descriptor 回调显式 `__stdcall`；回调可返回 HRESULT，Native AOT 边界必须确保托管异常在回调内部转换，不能穿越 native 调用栈。
- `SafeMediaHandle` 的释放是两阶段：先 `MILMediaShutdown` 打破媒体内部引用环并关闭状态引擎，再走 `MILRelease` 释放 COM 引用；顺序不可合并或倒置。
- `MILMediaProcessExitHandler` 与 `MILMediaShutdown` 不是同义词：前者只关闭 event proxy 的 reverse-P/Invoke 通路，后者关闭 player 状态；进程退出 helper 使用弱引用避免把 `MediaPlayerState` 保活到进程结束。
- event proxy 的 dispose 回调在 native 析构时释放托管 `GCHandle`；原实现对 CLR 进程关闭重入异常有专门 SEH 过滤，这是必须保留的失败路径事实。

### 7.2 后续重点

- 媒体 event proxy、InteropDeviceBitmap、几何 callback 的线程与存活期。
- UCE connection/channel/resource 句柄的创建、复制、引用计数和销毁语义。
- WIC 代理、glyph 路径数据和 device bitmap 输出的配对释放规则。

## 8. 名称差异、条件项与未定位项

当前仅确认原始 `.def` 未使用显式别名、转发、ordinal、`NONAME`、`DATA` 或 `PRIVATE`。源码名、宏生成定义和声明位置尚待逐项调查，不得提前认定 106 项均存在同名 C 函数定义。

## 9. Native AOT 导出薄层约束

待完成映射后汇总。当前最低约束基线：

1. 公开名称必须与目标架构实际 ABI 要求一致。
2. 参数顺序、位宽、符号性、返回值和调用约定必须逐项保持。
3. 托管异常不得越过原生边界。
4. COM 引用计数、句柄所有权、回调存活期和线程语义不得因托管封装改变。
5. 结构体、union、bitfield、数组及 pack/align 必须先由布局测试固化。
6. 薄层只做 ABI 转换、必要参数检查、异常封锁和向逐文件等价实现转发，不在边界层重构算法。

## 10. 事实、推断与风险

### 10.1 已确认事实

1. 原始 `wpfgfx.def` 的 `EXPORTS` 段包含 106 个静态名称条目。
2. 原始 `.def` 未显式使用 `DATA`、`PRIVATE`、别名、转发、ordinal、`NONAME` 或条件块。
3. 原始 `.def` 使用 `LIBRARY DLL_NAME`，需要构建时预处理展开库名。
4. `wpfgfx.vcxproj` 把预处理后的 `wpfgfx.i` 交给链接器，并支持 Win32/x64/ARM64。
5. native x86 项目默认调用约定为 stdcall；`WINAPI`/`CALLBACK` 在当前 Windows SDK 头中均落为 `__stdcall`。
6. Debug 源码另有 `.def` 外 `g_fNoMeterChecks` 数据导出候选，故 106 不能描述为实际 DLL 全部导出数。
7. 已定位 001–010、033–040 共 18 个条目的定义；其中 `MILCreateFactory` 有公开 native prototype，其余多数只有实现、COM/类声明或托管 P/Invoke 交叉证据。
8. `CStreamDescriptor` 是包含 14 个 stdcall 回调与一个指针宽度句柄的跨托管结构，native 会复制它并在 COM 包装对象存活期内调用。
9. 已定位 011–032 全部 22 个媒体/event proxy 条目的定义、`IMILMedia`/`IMILEventProxy` 声明和主要真实实现；Debug+PRERELEASE 可条件选择 `CFakePP`。
10. `CEventProxyDescriptor` 含两个 stdcall 回调和一个指针宽度 handle；媒体事件从 AV event thread 回调，`AVEventData` 是 align(4) 的变长尾部数组 packet。
11. 本轮没有也不得枚举实际二进制导出。

### 10.2 当前推断

- **高置信度**：106 个条目均意图作为函数导出，因为 `.def` 未使用 `DATA`，且命名均为动作/API 风格；仍需源码逐项确认。
- **高置信度**：导出面跨越 COM、媒体、软件位图、UCE 命令协议、几何、HW interop、WIC 和全局渲染状态，不可用单一高层封装替换。
- **中置信度**：多数实现将集中在 `core/api`、`core/av`、`core/uce`、`core/common`、`core/hw` 和 `core/resources`；具体映射待本专题完成。

### 10.3 当前风险

- x86 默认 stdcall 与 `.def` 未装饰公开名之间的链接规则未静态闭环。
- 生成头可能定义跨托管/原生共享的协议结构，任一位宽、pack、union 或 enum 基础类型偏差都会破坏 ABI。
- COM 与 UCE 句柄可能具有不同所有权模型，不能统一套用 GCHandle 或普通托管引用。
- 只找到声明而未找到定义不等于导出缺失；静态库聚合、宏或内联包装可能隐藏实现。

## 11. 未验证项（受工具禁令限制）

- `.def` 预处理后的 `wpfgfx.i` 精确内容。
- x86、x64、ARM64 各自产物中的实际导出名称、ordinal、转发和地址。
- 链接器是否对 x86 stdcall 自动消除装饰、以及所有条目是否成功解析。
- 任何 ABI 结构的实际 `sizeof`、`offsetof`、pack/align 和编译器特定 bitfield 布局。
- 实际调用栈、线程、失败码、引用计数、回调时序和卸载行为。
- 原生导出与 PresentationCore P/Invoke 的运行时一致性。

## 12. 对主 ABI 文档的输入

待本专题收口后提供：

- 106 项 `.def` 静态基线及分类。
- 导出到定义/声明/调用约定/签名/职责映射。
- 必须逐架构验证的名称装饰和 ABI 差异。
- 必须建立布局测试的生成类型与手写类型清单。
- COM、句柄、回调、所有权和异常边界约束。
- 未定位项、静态证据缺口及受禁令限制的验证清单。

## 13. 后续动作

1. 读取 ABI 宏、公共构建属性和 `.def` 生成 targets。
2. 按 `core/api`、`core/av`、`core/uce` 及其余实现域逐组定位 106 项定义和声明。
3. 读取 Include/Generated/processed 及共享 C# 输出，建立布局敏感类型清单。
4. 人工按 001–106 回勾映射覆盖，显式列出任何未定位项。
5. 将稳定结论输入后续主 ABI 文档；构建/二进制/运行验证留待禁令解除后的独立轮次。

## 14. 过程更新记录

- 已读取 `00-migration-charter.md` 与 `01-native-source-topology.md` 全文。
- 已创建本文档，而非等待最终回复。
- 已完整读取 141 行 `wpfgfx.def`。
- 已把 106 个 `.def` 静态条目逐名纳入分类表，并人工交叉核对分类小计。
- 已复核 `.def -> wpfgfx.i` 两阶段预处理、`DLL_NAME=$(TargetName)` 展开和 Win32/x64/ARM64 项目配置。
- 已确认 x86 默认 stdcall、`MILAPI=WINAPI`、`WINAPI`/`CALLBACK=__stdcall`，并记录未显式声明函数的 C 链接/修饰名仍未闭环。
- 已发现并单列 Debug 条件的 `.def` 外 `g_fNoMeterChecks` 数据导出候选。
- 已映射 COM 基础、工厂、render-target bitmap、双缓冲位图、stream wrapper、系统参数与版本检查 18 个 `.def` 条目。
- 已记录 `MILLoadResource` 当前托管调用方未定位，以及 `MILCreateFactory2` 仅见历史注释、并非当前 `.def` 条目。
- 已建立 `CStreamDescriptor` 14 个 stdcall 回调的字段顺序、托管镜像和 GCHandle 释放语义基线。
- 已映射 22 个媒体/event proxy 条目，确认 `IMILMedia` vtable、`CWmpPlayer` 主实现、Debug+PRERELEASE `CFakePP` 条件实现和 `SafeMediaHandle` 的 Shutdown→Release 顺序。
- 已记录 `CEventProxyDescriptor` reverse-P/Invoke、AV event thread、异常到 HRESULT、进程退出 SEH 防护和 `AVEventData` align(4)+尾部数组 packet。
- 尚未声称、也未执行实际二进制导出枚举。
