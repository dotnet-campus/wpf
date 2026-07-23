# PresentationCore 托管调用方与加载边界调查

> 状态：历史调查底稿；部分章节保留当时未完成标记，稳定结论已吸收到 `../02-abi-and-managed-callers.md`、`../08-abi-manifest-spec.md` 与 `../09-test-evidence-and-e2e-contract.md`；不作为当前任务入口  
> 专题范围：PresentationCore 与共享托管源码进入 `wpfgfx`/MilCore 的调用、加载、ABI、所有权、回调和命令协议边界  
> 约束来源：`WpfGfxShape/Docs/00-migration-charter.md`  
> 原生拓扑基线：`WpfGfxShape/Docs/01-native-source-topology.md`

## 1. 范围、原则与方法

### 1.1 调查范围

本专题只做静态调查，目标是：

- 找出 PresentationCore 与共享托管源码中指向 `wpfgfx`/MilCore 的 `DllImport`、`LibraryImport`、DLL 名常量、动态加载、模块初始化、SafeHandle、反向 P/Invoke/回调、委托和函数指针；
- 追踪 DLL 实际名称/版本后缀、加载顺序、初始化与关闭、进程退出、错误码转换、句柄所有权和跨线程调用；
- 按功能域建立“托管声明 → native EntryPoint → 原生定义/`.def`”映射；
- 审核默认及显式 `CharSet`、`CallingConvention`、`ExactSpelling`、`SetLastError`、`MarshalAs`、数组/字符串/结构体 marshalling、`unsafe` 指针和平台位宽等 ABI 敏感点；
- 区分 `wpfgfx`、`PresentationNative`、`DirectWriteForwarder`、系统 `dwrite.dll` 及其它相邻原生库；
- 为后续兼容验证给出托管调用方清单和最小端到端入口候选，但不编写生产代码，也不形成最终测试方案。

### 1.2 最高原则与禁止事项

严格遵守以下优先级：

1. 正确性和可验证的语义等价；
2. 现有 ABI、二进制布局、调用约定和生命周期兼容；
3. 逐文件近似直译、可审计、可回退；
4. 等价迁移完成前禁止重构、职责重组、算法改写和提前优化。

本专题不得修改任何现有 WPF 源码、项目配置或 `WpfGfxShape/Code`。本文是本轮唯一新增/持续修改的调查产物。

### 1.3 工具与验证限制

本轮绝对不调用任何命令行、终端、脚本或通过命令间接执行，包括 PowerShell、cmd、bash、Python、批处理、`dotnet`、`msbuild` 和脚本统计。只使用 IDE/工作区提供的：

- 项目和文件搜索；
- grep/code search；
- 符号导航；
- 文件读取；
- 本文档写入。

因此本文明确区分：

- **事实**：由已读取源码、项目文件或声明直接支持；
- **推断**：由多项静态证据归纳，注明置信度；
- **风险**：迁移或 ABI 兼容中可能导致行为偏差的点；
- **未确认**：当前静态证据不足；
- **受禁令限制的验证**：需要构建、运行、二进制枚举或脚本统计才能完成。

若无法精确统计声明数量，将明确说明覆盖范围和统计限制，不伪造数字。

## 2. 关键文件与阅读状态

| 文件 | 调查用途 | 状态 |
|---|---|---|
| `WpfGfxShape/Docs/00-migration-charter.md` | 确认 ABI 优先、逐文件近似直译、文档先行和工具禁令 | 已读取全文 |
| `WpfGfxShape/Docs/01-native-source-topology.md` | 获取 `wpfgfx` 聚合、`.def`、原生 DLL 生命周期和相邻库边界基线 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_exports.cs` | 共享 MilCore 导入声明：render-target bitmap、媒体、软件双缓冲位图、系统参数 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/exports.cs` | DUCE 通道/资源导入、命令发送包装、消息布局和资源句柄 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_render.cs` | 渲染/WIC 类型、HRESULT 转换、结构和联合布局 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_error.cs` | WIC、WGX、UCE、AV、D3DImage 错误码 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_core_types.cs` | 核心枚举、协议 ID、结构和显式布局 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_av_types.cs` | AV 事件枚举 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/Generated/wgx_commands.cs` | generated command 显式布局 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/SafeNativeMethodsMilCoreApi.cs` | 分区管理初始化/反初始化和性能 ID 的安全包装 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/UnsafeNativeMethodsMilCoreApi.cs` | MilCore 主声明面及同文件内 WIC/色彩管理/ole32 相邻边界 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/MediaSystem.cs` | MediaSystem 版本检查、分区管理、连接和全局 service channel 生命周期 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/FactoryMaker.cs` | MIL/WIC 工厂的进程内共享、引用计数与释放 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/ChannelManager.cs` | 异步/同步连接和 channel 创建、池化与关闭 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/Composition.cs` | 组合线程和调用边界 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/MediaContext.cs` | 每 Dispatcher 启动、channel 初始化、通知、同步等待和关闭顺序 | 已读取关键生命周期/消息区段 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/MediaPlayerState.cs` | native media 创建、ProcessExit 特殊入口和 transport AddRef | 已读取关键生命周期区段 |
| `src/Microsoft.DotNet.Wpf/src/Shared/MS/Internal/AppDomainShutdownMonitor.cs` | DomainUnload/ProcessExit 回调分发 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/ModuleInitializer.cs` | PresentationCore 模块初始化和 DirectWrite 预加载 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/MS/Internal/Text/TextInterface/DWriteLoader.cs` | 系统 `dwrite.dll` 的 System32 加载、函数指针和 ProcessExit 卸载 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/SafeMILHandle.cs` | 通用 MIL/COM 指针拥有与 `MILRelease` 释放 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/SafeMILHandleMemoryPressure.cs` | native 内存压力的托管共享引用计数 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/Imaging/BitmapSourceSafeMILHandle.cs` | WIC bitmap 大小估算、QI 临时引用和压力共享 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/Composition.cs` | 其余几何 MilCore P/Invoke 与托管/native 数值转换 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/EventProxy.cs` | AV 事件反向回调描述符、GCHandle 和创建入口 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/StreamAsIStream.cs` | 托管 Stream → native IStream 的回调表和创建入口 | 已读取关键声明、创建和释放区段 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/SafeReversePInvokeHandle.cs` | reverse P/Invoke 包装创建、阻塞释放与 COM release | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/safemediahandle.cs` | 媒体对象先 shutdown 再 COM release | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/MediaContextNotificationWindow.cs` | channel 通知 HWND、MilContent attach/detach 和 user32 动态查找 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/InterOp/HwndTarget.cs` | visual target attach/detach 与 HWND 线程约束 | 已读取相关区段 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/InterOp/D3DImage.cs` | InteropDeviceBitmap 回调、跨线程转发和命令所有权 | 已读取相关区段 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/PresentationCore.csproj` | 确认共享 Graphics 文件、关键调用方和各 SafeHandle 文件实际编译进入 PresentationCore | 已读取互操作相关项目清单区段 |
| `src/Microsoft.DotNet.Wpf/src/Shared/RefAssemblyAttrs.cs` | PresentationCore 实际 `DllImport` 常量和 `_cor3` 后缀 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_core_dllname.cs` | 历史/另一生成边界中的 `WpfGfx_v0400.dll` 常量 | 已读取全文；不视为 PresentationCore 当前常量 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.def` | native 导出名称基线 | 待读取/交叉核对 |

> 本表将在每批读取后更新，不把“已定位”误写为“已审阅”。

## 3. DLL 名、版本后缀与加载生命周期

### 3.1 DLL 名与常量

**事实：**

- `PresentationCore.csproj` 定义 `PRESENTATION_CORE`，并编译 `src/Microsoft.DotNet.Wpf/src/Shared/RefAssemblyAttrs.cs`。因此该文件进入命名空间 `MS.Internal.PresentationCore`。
- `BuildInfo.WCP_VERSION_SUFFIX` 在当前源码中为 `_cor3`；`DllImport.MilCore` 定义为 `$"wpfgfx{BuildInfo.WCP_VERSION_SUFFIX}.dll"`，即当前静态展开值 **`wpfgfx_cor3.dll`**。
- 同一常量类还明确区分 `PresentationNative_cor3.dll`、`PresentationCFFRasterizerNative_cor3.dll`、无后缀的 `WindowsCodecs.dll`、`WindowsCodecsExt.dll`、`mscms.dll`、`ole32.dll` 等。
- `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_core_dllname.cs` 另定义 `Microsoft.Internal.DllImport.MilCore = "WpfGfx_v0400.dll"`。其命名空间、路径和名称与 PresentationCore 当前编译的 `MS.Internal.PresentationCore.DllImport` 不同；在没有项目包含证据前，不能把它当作当前 PresentationCore 的 DLL 名。
- 原生项目目标名是 `wpfgfx$(WpfVersionSuffix)`；托管 `_cor3` 与 native `$(WpfVersionSuffix)` 的最终一致性需要构建属性/产物验证，本轮静态源码只确认托管侧展开值。

### 3.2 托管模块初始化与加载顺序

**PresentationCore 模块初始化的静态顺序：**

1. `ModuleInitializer.Initialize()` 检查入口程序集是否标注 `DisableDpiAwarenessAttribute`；若没有，调用 `user32!SetProcessDPIAware`。
2. `DWriteLoader.LoadDWrite()` 使用 `NativeLibrary.Load("dwrite.dll", typeof(DWriteLoader).Assembly, DllImportSearchPath.System32)` 从 System32 加载系统 DirectWrite，并解析 `DWriteCreateFactory` 为 `delegate* unmanaged<int, void*, void*, int>`。
3. 注册 `AppDomain.CurrentDomain.ProcessExit`，退出时先清空该函数指针，再 `NativeLibrary.Free` 上述 `dwrite.dll` 模块句柄。
4. 调用 C++/CLI `MS.Internal.NativeWPFDLLLoader.LoadDwrite()`，从而建立对 `DirectWriteForwarder` 项目的运行依赖并确保其程序集/模块初始化可触发。

**重要现状差异：**`DirectWriteForwarder/main.cpp` 中 `NativeWPFDLLLoader` 上方的历史注释称其加载版本化 `wpfgfx` 和 `PresentationNative`，但当前 `LoadDwrite()` 实现只给静态 `m_temp` 赋空值；同文件可见的模块初始化只初始化 TrueType subsetter 全局数组。当前静态源码没有证明该方法实际加载 `wpfgfx` 或 `PresentationNative`。注释不能覆盖实现事实。

**wpfgfx 加载边界：**

- 当前托管源码未见 wpfgfx 的显式 `NativeLibrary.Load`、`LoadLibrary`、`GetProcAddress` 或自定义 `DllImportResolver`。因此可确认的主路径是 CLR 在首次调用 `DllImport.MilCore` 时按 `wpfgfx_cor3.dll` 名称解析并加载。
- “首次入口”不是全局固定的：任何较早触发的 MilCore P/Invoke 都可成为加载点。
- 对常规首个 `MediaContext` 构造路径，构造函数先创建 `MediaContextNotificationWindow`；其构造函数调用 `MilContent_AttachToHwnd`，然后才调用 `MediaSystem.Startup`。因此该路径中 **`MilContent_AttachToHwnd` 早于 `MilVersionCheck`**，很可能是触发 wpfgfx 加载/native DLL attach 的第一个入口。版本检查是加载后的协议兼容检查，不是可靠的预加载门。
- 若应用先使用其它静态图形/工厂/几何 API，首次入口可不同；需要 loader trace 才能确认具体应用的真实首次解析顺序。

### 3.3 工厂、通道与媒体系统初始化

#### 3.3.1 每 Dispatcher 的 `MediaContext` 启动

`MediaContext` 每个 Dispatcher 一个实例。构造中的相关顺序为：

1. 创建隐藏 `MediaContextNotificationWindow`，调用 `MilContent_AttachToHwnd`。
2. 调用 `MediaSystem.Startup(this)`：
   - 每次先调用 `MilVersionCheck(0x200184C0)`；失败直接经 `HRESULT.Check` 抛异常。
   - 进入 native `MilCompositionEngine_EnterCompositionEngineLock`。
   - 把当前 `MediaContext` 加入全局 `_mediaContexts`。
   - 首次启动（`s_refCount == 0`）依次：
     1. `MilCompositionEngine_InitializePartitionManager(THREAD_PRIORITY_NORMAL)`；native 注释确认这会创建 composition 基础设施、scheduler 和 worker threads。
     2. `WgxConnection_ShouldForceSoftwareForGraphicsStreamClient()`。
     3. `WgxConnection_Create(false, out connection)`；native 将 `false` 映射为 `MilMarshalType::CrossThread`。
     4. 以空 reference channel 创建全局异步 service channel，从而创建新 partition。
     5. 标记 transport connected，并读取 animation smoothing 设置。
   - 增加 `s_refCount`，退出 composition engine lock。
   - 在锁外调用 `RenderOptions_EnableHardwareAccelerationInRdp`；该调用对每个 `MediaContext.Startup` 都执行，不只首次执行。
3. `MediaSystem.ConnectChannels(this)` 再次进入 composition engine lock；若全局 transport 已连接，为当前 MediaContext 创建两个 cross-thread channel：普通 async channel 和 out-of-band async channel，二者都引用全局 service channel，从而加入同一 partition。
4. `MediaContext.CreateChannels()` 紧接着发送 non-interactive policy 命令、设置 notification HWND、注册 back-channel 通知、创建 ETW resource、请求 tier、close/commit，并用 `CompleteRender` 同步等待处理完成后读取返回消息。
5. 只有上述步骤成功后，构造函数才订阅 `Dispatcher.ShutdownFinished`，并把自身写入 `dispatcher.Reserved0`。

#### 3.3.2 同步 channel

- `ChannelManager.AllocateSyncChannel()` 惰性调用 `WgxConnection_Create(true)`；native 将 `true` 映射为 `MilMarshalType::SameThread`。
- 每个 MediaContext 维护独立同步 connection、同步 service channel 和最多按现有条件缓存的同步 channel 队列。
- `Renderer` 的 RenderTargetBitmap 路径在 same-thread channel 上 close/commit 后调用 `WgxConnection_SameThreadPresent`，再读取同步 back-channel 消息。
- `DUCE.Channel.MarshalType` 通过 native `MilChannel_GetMarshalType` 查询。`MediaPlayer` 在 same-thread channel 上订阅 managed new-frame 更新，在 cross-thread channel 上要求 UCE 直接通知；两条路径语义不同。

#### 3.3.3 MIL/WIC factory

- `FactoryMaker` 声明为 free-threaded，并用托管静态锁和静态实例计数共享一个 `MILCreateFactory` 返回的裸 `IntPtr`。
- 首个 `FactoryMaker` 创建 MIL factory；后续实例只增加托管计数。最后一个实例释放时先 `MILRelease(s_pFactory)`，随后如存在再释放 WIC imaging factory。
- WIC imaging factory 由 `WindowsCodecs.dll!WICCreateImagingFactory_Proxy` 惰性创建，不是 wpfgfx factory 导出。
- `ImagingFactoryPtr` 在锁外检查空值，进入锁后没有第二次检查；静态源码允许两个并发首次调用顺序创建并覆盖 factory 指针的竞态。本文只记录现有行为，迁移期不得顺手重构或“修复”。

### 3.4 关闭、显式卸载与进程退出

#### 3.4.1 Dispatcher/显式 MediaContext 关闭

`MediaContext` 通过 `Dispatcher.ShutdownFinished` 在拥有该 Dispatcher 的线程调用 `Dispose()`。当前顺序为：

1. 先 dispose 所有注册的 composition targets；
2. `MediaContextNotificationWindow.Dispose()` 调用 `MilContent_DetachFromHwnd`，再销毁隐藏 HWND；
3. 取消 Dispatcher shutdown handler，停止 TimeManager，标记 context disposed；
4. `RemoveChannels()`：释放 ETW resource，退出 interlocked presentation，依次关闭 async channel、async out-of-band channel、同步 channel 池和同步 service channel，最后 `WgxConnection_Disconnect` 同步 connection；
5. `MediaSystem.Shutdown(this)` 在 composition engine lock 内移除 context 并减少全局引用计数；最后一个 context 依次关闭全局 service channel、断开全局 async connection、再 `MilCompositionEngine_DeinitializePartitionManager`；
6. 清空 TimeManager 引用并抑制终结。

`DUCE.Channel` 持有裸 `IntPtr`，没有 SafeHandle/finalizer。`Close()` 的 native 顺序是 close current batch → commit → destroy channel，然后把句柄清零。因此显式 MediaContext/ChannelManager 关闭是关键，不可假设 GC 会替代它。

#### 3.4.2 媒体对象的三类结束动作

- 用户 `MediaPlayer.Close` 最终调用 `MILMediaClose`：这是关闭当前媒体内容/播放状态，不释放 `IMILMedia` 对象。
- `SafeMediaHandle.ReleaseHandle` 调用 `MILMediaShutdown`，成功检查后再 `MILRelease`：这是对象终结/释放路径。
- `MediaPlayerState` 为每个 native media 注册 AppDomain `ProcessExit` handler；handler 通过弱引用取 SafeMediaHandle 并调用 `MILMediaProcessExitHandler`，返回 HRESULT 被忽略。这是专门的进程退出通知，不等于普通 `Close` 或 `Shutdown`。
- `MediaPlayerState` 自身 finalizer 只取消 ProcessExit 订阅；native media 的最终释放由 `SafeMediaHandle` 承担。

#### 3.4.3 进程退出和 AppDomain 关闭

- PresentationCore 模块初始化注册的 ProcessExit handler 只卸载其托管 `DWriteLoader` 持有的系统 `dwrite.dll`。
- `AppDomainShutdownMonitor` 同时监听 `DomainUnload` 和 `ProcessExit`；当前唯一已发现 listener 是 `D3DImage`，其 shutdown 回调调用 `InteropDeviceBitmap_Detach`，先阻止 composition thread 再回调托管对象。
- 每个 `MediaPlayerState` 另注册 `MILMediaProcessExitHandler`。
- `MediaSystem.Shutdown` 没有直接注册 ProcessExit；它依赖 Dispatcher 的 `ShutdownFinished`/显式 `Dispose`。进程退出时是否每个 Dispatcher 都先完成该关闭链，静态源码不能保证。
- 原生拓扑已确认 Windows 进程终止的 DLL detach 路径默认跳过完整 `MILCoreDllMain` shutdown。故托管 ProcessExit 通知、Dispatcher shutdown、SafeHandle finalization、系统 DLL 卸载和 native process-termination detach 是多条不同路径，不能合并为一个抽象 `Dispose`。
- 多个 ProcessExit handler 的实际调用顺序、异常处理和与 finalizer/native loader detach 的时序需运行验证；源码订阅位置不足以证明全局顺序。

### 3.5 跨线程调用与线程归属

已确认：

- 全局异步 transport 使用 `CrossThread` marshal type；同步 RenderTargetBitmap transport 使用 `SameThread` marshal type。
- `CompositionEngineLock` 不是普通托管 `lock`，而是每次通过两个 wpfgfx 导出进入/离开 native 全局 composition lock。它覆盖 MediaSystem 全局列表/refcount/连接，也广泛包围 generated resource channel 操作。
- `EnterMediaSystemLock`/`ExitMediaSystemLock` 两个托管声明在当前 PresentationCore 搜索中没有调用方；原生源码和 `wpfgfx.def` 搜索也未找到同名定义/导出。它们是首批“声明存在但当前不可达且疑似无导出”的不一致候选，后续映射节继续核对。
- async back-channel 通过 `MilChannel_SetNotificationWindow` 让 native 向隐藏 HWND 投递注册消息；消息在 owner Dispatcher 线程进入 `MediaContext.NotifyChannelMessage`。
- `CompleteRender` 可在调用线程阻塞于 `MilComposition_WaitForNextMessage` 或 `MilComposition_SyncFlush`，直到 composition thread 处理/呈现；这些不是纯异步 fire-and-forget 调用。
- `D3DImage` front-buffer callback 明确运行在 composition thread，再 `Dispatcher.BeginInvoke` 到 UI 线程。
- AV event proxy 可从 native 媒体线程进入托管；`MediaEventsHelper` 解包事件后按 Normal/Background 优先级投递到 MediaPlayer 的 Dispatcher。
- `FactoryMaker` 允许 free-threaded 获取/释放 shared factory；最后一个实例也可能在 finalizer thread 执行 release。迁移不得无证据增加 UI-thread 亲和性。

### 3.6 部分初始化和回滚风险

以下是静态源码可见、必须先保真的失败行为：

- `MediaSystem.Startup` 在进入首轮初始化前先把 `MediaContext` 加入 `_mediaContexts`；若 partition manager、connection 或 service channel 创建失败，本方法没有回滚列表、已初始化 partition manager或 connection，且 `s_refCount` 尚未增加。
- `ConnectTransport` 在 `WgxConnection_Create` 成功后才构造 service channel，并在最后才设 `IsTransportConnected=true`；若 service channel 构造失败，源码没有本地 disconnect 清理。
- `ChannelManager.CreateChannels` 先创建普通 async channel，再创建 out-of-band channel；第二步失败时没有本地关闭第一条 channel。
- `MediaContext` 直到 Startup/ConnectChannels 成功后才订阅 `Dispatcher.ShutdownFinished`；构造中更早创建的 notification window/attach 或部分 native 状态在异常路径如何收尾，需失败注入验证。
- 这些都不是允许迁移时自动补齐的“明显清理”；章程要求初次近似直译现有顺序，再用原生基线验证失败行为。

## 4. 托管声明到 native EntryPoint 分类表

> 下表将按功能域维护；“声明数量”只在可由静态证据可靠确认时填写。

| 功能域 | 托管声明/包装 | native EntryPoint | DLL | `.def`/原生定义状态 | 主要调用方 | ABI 备注 |
|---|---|---|---|---|---|---|
| DLL/进程初始化 | `MilContent_AttachToHwnd`; `MilVersionCheck`; 分区管理 `Initialize/Deinitialize`; connection create/disconnect | 同名 | `wpfgfx_cor3.dll` | 部分已定位 `core/uce/{rsapi,apifunc}.cpp`；`.def` 待整表 | `MediaContext` → `MediaSystem`/`ChannelManager` | 首次加载可早于版本检查；部分初始化无本地回滚 |
| 工厂/渲染 | `MILFactory2.CreateFactory/CreateMediaPlayer/CreateBitmapRenderTarget/CreateBitmapRenderTargetForBitmap` | `MILCreateFactory`; `MILFactoryCreateMediaPlayer`; `MILFactoryCreateBitmapRenderTarget`; `MILFactoryCreateSWRenderTargetForBitmap` | `wpfgfx_cor3.dll` | 待核对 | `FactoryMaker` 等待回溯 | COM 指针、SafeHandle、bool、float/enum |
| 连接/通道/命令 | `WgxConnection_Create/Disconnect/SameThreadPresent`; `MilConnection_*`; `MilChannel_*`; `MilResource_SendCommand*`; wait/peek | 多数同名；托管 `MilConnection_CloseBatch/CommitChannel` 分别显式映射 `MilChannel_CloseBatch/CommitChannel` | `wpfgfx_cor3.dll` | 待核对 | `DUCE.Channel` 已确认直接调用 | 大量 `IntPtr`、`byte*`、`IntPtr[]`、bool；关闭后部分包装静默返回 |
| 资源/句柄 | `MILAddRef`; `MILRelease`; `MILQueryInterface`; `MilResource_CreateOrAddRefOnChannel/DuplicateHandle/ReleaseOnChannel/GetRefCountOnChannel`; reverse wrapper | 同名 | `wpfgfx_cor3.dll` | 待核对 | `SafeMILHandle.ReleaseHandle`; DUCE resources | COM 引用计数与 32 位 DUCE handle 是不同所有权体系 |
| 几何/字形 | `MilUtility_PathGeometry*`; glyph cache begin/append/end；glyph outline；`MIL3DCalcProjected2DBounds` | 同名 | `wpfgfx_cor3.dll` | 待核对 | `PathGeometry`/`GlyphCache`/`MILUtilities` 待细读 | `unsafe` 指针、回调委托、路径二进制块、bool 宽度 |
| 位图/WIC/渲染目标 | render-target bitmap；双缓冲位图；CWIC wrapper；色彩上下文代理；InteropDeviceBitmap；Hwnd attach/detach | 见声明名/显式 `EntryPoint` | 主要为 `wpfgfx_cor3.dll`；同文件还混有 `WindowsCodecs*.dll` | 待核对 | Imaging、D3DImage、HwndTarget | 必须逐声明区分 wpfgfx 与系统 WIC 代理 |
| 媒体/AV | `MILMedia*`; `MILFactoryCreateMediaPlayer`; 事件代理待补 | 显式同名 | `wpfgfx_cor3.dll` | 待核对 | `MediaPlayerState`/`EventProxy` 待回溯 | `BStr`、SafeMediaHandle、bool/ref bool、进程退出入口 |
| 错误/诊断/选项 | render options；`MILUpdateSystemParametersInfo`; `GetNextPerfElementId`; notification helpers待补 | 同名/显式名 | `wpfgfx_cor3.dll` | 待核对 | RenderOptions、PerfService、通知窗口 | void/bool 返回的 native 宽度和错误丢失需核对 |

### 4.1 当前静态声明盘点及计数边界

通过对 `PresentationCore.csproj` 的显式编译输入、整个 `PresentationCore` 源码树和其直接编译的 `Common/Graphics` 源码进行分区 grep，并逐个读取命中文件，本轮静态源码盘点确认：

- **10 个源文件中共有 109 个 `[DllImport(DllImport.MilCore, ...)]` 声明出现。**
- 其中共享 `Common/Graphics` 两个文件 47 个，PresentationCore 自有八个文件 62 个。
- 这是**源码声明出现数**：重载按声明分别计数，例如 `MILAddRef` 两个托管重载和 `MILQueryInterface` 两个托管重载各自计入；它不表示唯一 EntryPoint 数、运行时实际解析次数、可达调用数或二进制导入表项数。
- 本计数未使用脚本；已通过按目录/文件首字母拆分搜索避开单次结果上限，但仍须由后续自动 ABI 清单复核。若项目条件或生成阶段加入当前静态项目清单之外的源码，此数字需要更新。
- 对 `PresentationCore`、`Common`、`Shared` C# 根的 `LibraryImport(` 搜索均未发现结果；当前 wpfgfx 主边界使用传统 `DllImport`。

逐文件声明出现数与 EntryPoint 集合如下：

| 文件 | 声明出现数 | 托管声明 / native EntryPoint |
|---|---:|---|
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_exports.cs` | 28 | `MILRenderTargetBitmapGetBitmap`, `MILRenderTargetBitmapClear`; `MILMediaOpen`, `MILMediaStop`, `MILMediaClose`, `MILMediaGetPosition`, `MILMediaSetPosition`, `MILMediaSetVolume`, `MILMediaSetBalance`, `MILMediaSetIsScrubbingEnabled`, `MILMediaIsBuffering`, `MILMediaCanPause`, `MILMediaGetDownloadProgress`, `MILMediaGetBufferingProgress`, `MILMediaSetRate`, `MILMediaHasVideo`, `MILMediaHasAudio`, `MILMediaGetNaturalHeight`, `MILMediaGetNaturalWidth`, `MILMediaGetMediaLength`, `MILMediaNeedUIFrameUpdate`, `MILMediaShutdown`, `MILMediaProcessExitHandler`; `MILSwDoubleBufferedBitmapCreate`, `MILSwDoubleBufferedBitmapGetBackBuffer`, `MILSwDoubleBufferedBitmapAddDirtyRect`, `MILSwDoubleBufferedBitmapProtectBackBuffer`; `MILUpdateSystemParametersInfo` |
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/exports.cs` | 19 | `MilResource_CreateOrAddRefOnChannel`, `MilResource_DuplicateHandle`, `MilConnection_CreateChannel`, `MilConnection_DestroyChannel`, `MilChannel_CloseBatch`, `MilChannel_CommitChannel`, `WgxConnection_SameThreadPresent`, `MilChannel_GetMarshalType`, `MilResource_SendCommand`, `MilChannel_BeginCommand`, `MilChannel_AppendCommandData`, `MilChannel_EndCommand`, `MilResource_SendCommandMedia`, `MilResource_SendCommandBitmapSource`, `MilResource_ReleaseOnChannel`, `MilChannel_SetNotificationWindow`, `MilComposition_WaitForNextMessage`, `MilComposition_PeekNextMessage`, `MilResource_GetRefCountOnChannel` |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/UnsafeNativeMethodsMilCoreApi.cs` | 43 | composition/media locks; version/connection; stream descriptor; tile/geometry; glyph cache/run; reverse wrapper; render options; CWIC wrapper; `MILAddRef`×2, `MILRelease`, `MILQueryInterface`×2; three `IWICColorContext_*_Proxy`; factory/media/render-target; four `InteropDeviceBitmap_*` |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/SafeNativeMethodsMilCoreApi.cs` | 3 | `MilCompositionEngine_InitializePartitionManager`, `MilCompositionEngine_DeinitializePartitionManager`, `GetNextPerfElementId` |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/Composition.cs` | 8 | `MilComposition_SyncFlush`, `MilUtility_GetPointAtLengthFraction`, `MilUtility_PolygonBounds`, `MilUtility_PolygonHitTest`, `MilUtility_PathGeometryHitTest`, `MilUtility_PathGeometryHitTestPathGeometry`, `MilUtility_GeometryGetArea`, `MilUtility_ArcToBezier` |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/EventProxy.cs` | 1 | `MILCreateEventProxy` |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/MediaContextNotificationWindow.cs` | 2 | `MilContent_AttachToHwnd`, `MilContent_DetachFromHwnd` |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/MILUtilities.cs` | 2 | `MIL3DCalcProjected2DBounds`; 托管 `MILCopyPixelBuffer` → `MilUtility_CopyPixelBuffer` |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/StreamAsIStream.cs` | 1 | `MILIStreamWrite` |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/InterOp/HwndTarget.cs` | 2 | 托管 `VisualTarget_AttachToHwnd/DetachFromHwnd` → `MilVisualTarget_AttachToHwnd/DetachFromHwnd` |

### 4.2 明确的托管名与 EntryPoint 名差异

- `exports.cs::MilConnection_CloseBatch` → `MilChannel_CloseBatch`。
- `exports.cs::MilConnection_CommitChannel` → `MilChannel_CommitChannel`。
- `MILUtilities.cs::MILCopyPixelBuffer` → `MilUtility_CopyPixelBuffer`。
- `HwndTarget.cs::VisualTarget_AttachToHwnd` → `MilVisualTarget_AttachToHwnd`。
- `HwndTarget.cs::VisualTarget_DetachFromHwnd` → `MilVisualTarget_DetachFromHwnd`。
- `UnsafeNativeMethodsMilCoreApi.cs::CreateCWICWrapperBitmap` → `MilResource_CreateCWICWrapperBitmap`。
- `UnsafeNativeMethodsMilCoreApi.cs::MILFactory2.CreateFactory` → `MILCreateFactory`；其它 `MILFactory2` 包装同样用简化托管名映射显式导出名。
- `UnsafeNativeMethodsMilCoreApi.cs::MILUnknown.AddRef/Release/QueryInterface` 是托管包装名，分别显式指向 `MILAddRef`、`MILRelease`、`MILQueryInterface`。

其余未显式设置 `EntryPoint` 的声明按托管方法名解析，但仍需考虑 `ExactSpelling` 默认值和字符集名称探测规则。

## 5. 类型布局与 marshalling 风险

### 5.1 P/Invoke 元数据默认值与显式值

初步事实：已读的 MilCore 声明绝大多数只写 `[DllImport(DllImport.MilCore)]` 或只加 `EntryPoint`，没有显式设置 `CharSet`、`CallingConvention`、`ExactSpelling` 或 `SetLastError`。这意味着迁移文档必须把 CLR `DllImportAttribute` 默认值作为契约的一部分核实，不能擅自“规范化”为统一显式值。

已见的例外：

- `MILSwDoubleBufferedBitmapGetBackBuffer`、`MILSwDoubleBufferedBitmapAddDirtyRect` 和 `MilUtility_CopyPixelBuffer` 使用 `PreserveSig = false`，托管签名为 `void`，native 失败由运行时按 HRESULT 抛异常；它们不能与普通返回 `int` 后调用 `HRESULT.Check` 的声明混为一类。
- `MILMedia.Open` 对字符串显式使用 `[In, MarshalAs(UnmanagedType.BStr)]`，不是默认 LPStr/LPWStr。
- `MILIStreamWrite` 对 `byte[]` 使用 `[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]`；长度来自第三个参数 `cb`（索引按参数表从 0 计）。
- 同一 `UnsafeNativeMethodsMilCoreApi.cs` 中只有 `mscms` 的部分声明显式 `SetLastError = true`；已读 MilCore 声明尚未发现 `SetLastError = true`。这进一步说明不能把相邻系统库声明的属性套到 wpfgfx。

- `CharSet`；
- `CallingConvention`；
- `ExactSpelling`；
- `SetLastError`；
- `BestFitMapping`/`ThrowOnUnmappableChar`（如存在）；
- 返回值和布尔值 marshalling；
- `PreserveSig`（如适用）。

### 5.2 字符串、数组、结构体与联合

调查中。

### 5.3 `unsafe` 指针、函数指针与平台位宽

初步事实：

- 几何、命令、glyph、像素复制等入口广泛直接传递 `byte*`、`double*`、结构指针和固定数组地址；native 边界依赖调用期间 pinning、精确尺寸和同步消费。
- generated command 中指针/句柄槽位常被固定编码为 `UInt64`，例如 `hwnd`、`hSection`、`pRenderTarget`、`pIDWriteFont`、事件句柄和 bitmap 指针；调用方会把 `IntPtr` 经 `UIntPtr` 转为 `UInt64` 以避免符号扩展。这是协议字段，不等同于普通平台大小 `IntPtr`。
- `MilComposition_PeekNextMessage` 的 `messageSize` 托管类型为 `IntPtr`，调用方传 `(IntPtr)sizeof(Message)`；这是 native `size_t` 语义，需按架构核对。

### 5.4 generated/processed 类型的布局来源

调查中。

## 6. SafeHandle、引用计数与所有权

### 6.1 SafeMILHandle 类型族

已确认：

- `SafeMILHandle : SafeHandleZeroOrMinusOneIsInvalid`，两个构造路径均声明拥有 native 指针；`ReleaseHandle` 调用 `MILRelease` 并把内部指针清零。
- `BitmapSourceSafeMILHandle` 派生自 `SafeMILHandle`，仍由同一 `MILRelease` 释放；它可计算 WIC bitmap 估算大小，也可从另一 SafeMILHandle 共享托管内存压力对象。
- `SafeMILHandleMemoryPressure` 是独立的托管引用计数，不代表 native COM 引用数。注释明确允许 native 仍有引用时移除 GC memory pressure。
- `BitmapSourceSafeMILHandle.ComputeEstimatedSize` 先对裸 bitmap 指针执行 `MILQueryInterface(IID_IWICBitmapSource)`，随后用新 `SafeMILHandle` 接管 QI 新增的引用；这是“QI 增引用 → SafeHandle 释放”的明确所有权配对。
- `SafeMediaHandle` 覆盖释放：先对 `IMILMedia` 调用 `MILMediaShutdown` 并执行 `HRESULT.Check`，再调用 `MILRelease`。这不是普通 `SafeMILHandle` 的单一步 release。
- `SafeReversePInvokeWrapper` 构造时把托管 delegate 函数地址交给 `MilCreateReversePInvokeWrapper`；释放时先 `MilReleasePInvokePtrBlocking`，然后再 `MILRelease` 包装对象。两个动作和顺序均是 ABI/并发契约。
- `UnknownBitmapDecoder.CoInitSafeHandle` 虽继承 `SafeMILHandle`，却把它当作生命周期哨兵：构造调用 `ole32!CoInitialize`，覆盖释放只调用 `CoUninitialize`，不持有 MIL COM 指针。按基类名归类会误判所有权和 DLL 边界。
- `SafeProfileHandle` 和 `ColorTransformHandle` 分别由 `mscms!CloseColorProfile` 与 `mscms!DeleteColorTransform` 释放，不属于 wpfgfx SafeHandle。

### 6.2 创建、借用、转移与释放

调查中。

### 6.3 父子句柄和跨线程释放

调查中。

### 6.4 失败路径与重复释放风险

调查中。

## 7. 反向 P/Invoke、回调与函数指针

已发现的 wpfgfx 相关入口：

- `MilCreateReversePInvokeWrapper(IntPtr pFcn, out IntPtr wrapper)` 和 `MilReleasePInvokePtrBlocking(IntPtr wrapper)` 明确建立 native 反向调用包装及阻塞释放边界。
- `PathGeometry.AddFigureToListDelegate` 被传给 combine/widen/outline/flatten native 几何入口。
- `InteropDeviceBitmap.FrontBufferAvailableCallback(bool lost, uint version)` 被传给 `InteropDeviceBitmap_Create`。
- `GlyphCache` 将托管 delegate 通过 `Marshal.GetFunctionPointerForDelegate` 转为函数地址，再进入 reverse wrapper；具体生存期和线程将在后续步骤细读。
- `StreamAsIStream` 通过 `StreamDescriptor` 向 native 传入 dispose/clone/commit/copy/read/seek/write 等一组回调及 `GCHandle` 上下文，属于完整的托管对象反向调用桥，而不是单个普通 P/Invoke。
- `EventProxyDescriptor` 用顺序布局承载 `Dispose`、`RaiseEvent` 两个 delegate 和一个 `GCHandle`；静态字段保活 delegate，native dispose 回调负责释放 `GCHandle`。`RaiseEvent` 捕获所有托管异常并以 `Marshal.GetHRForException` 返回，避免异常跨越 native 边界。
- `D3DImage` 把 `FrontBufferAvailableCallback` 保存在实例字段，native 在 composition/render 线程调用；回调不得直接触碰 DispatcherObject，而是通过 `Dispatcher.BeginInvoke` 转回 UI 线程。finalizer、换 back buffer 和 AppDomain shutdown 都会先调用 `InteropDeviceBitmap_Detach` 阻止后续回调。
- 几何 `AddFigureToListDelegate` 未显式存入长期字段；从同步几何 API 的调用形态推断 native 应只在 P/Invoke 调用期间使用。该同步性必须从原生实现和运行验证确认，不能仅凭 delegate 生命周期假设。
- `GlyphCache.cs` 包含 `Marshal.GetFunctionPointerForDelegate` → `SafeReversePInvokeWrapper` → command 中 `UInt64 CallbackPointer` 的额外回调路径，但当前 `PresentationCore.csproj` 显式编译清单和全项目引用搜索均未见 `GlyphCache.cs`/`GlyphCache` 被纳入或使用，且当前 `wgx_commands.cs`/`MILCMD` 也未见相应命令定义。暂列为**源码树中的疑似遗留/未编译边界**，不得计入上面的 109 个当前 P/Invoke 声明或当前协议面，后续需核对历史生成输入/条件项目。

后续仍需区分：

- native 调回托管的委托；
- 托管传入 native 的函数指针/回调上下文；
- 仅在托管内部使用的委托；
- COM 回调接口；
- 动态解析后转换为委托的函数地址。

## 8. generated command 协议

### 8.1 命令结构与布局

调查中。

### 8.2 命令发送、批处理与 native 消费者

调查中。

### 8.3 协议版本、枚举值和可变长度载荷风险

调查中。

## 9. 错误传播与 HRESULT 转换

调查中。将追踪：

- native `HRESULT`/MIL 错误码返回；
- `HRESULT.Check`、异常转换或手工分支；
- `SetLastError`/Win32 error 是否参与；
- SafeHandle 释放失败是否被忽略；
- 跨线程/异步错误如何回传托管调用方。

## 10. 与其它原生库的边界

| 原生库 | 当前边界判断 | 禁止混淆点 | 状态 |
|---|---|---|---|
| `wpfgfx` / 历史 MilCore 名称 | PresentationCore 当前常量静态展开为 `wpfgfx_cor3.dll` | `MilCoreApi`/`MIL*` 是历史 API 命名，不表示文件名为 `milcore.dll`；另有非当前常量 `WpfGfx_v0400.dll` | 已确认托管常量；产物待验证 |
| `PresentationNative` | 当前常量为 `PresentationNative_cor3.dll`，属于相邻原生组件 | 不得把其 P/Invoke、SafeHandle 或错误路径计入 wpfgfx ABI | 常量已确认；调用面待分类 |
| `DirectWriteForwarder` | PresentationCore 加载链组件；不是 `wpfgfx` 项目链接输入 | 其加载职责不得与 wpfgfx 自身 DirectWrite loader 合并 | 调查中 |
| 系统 `dwrite.dll` | wpfgfx 与/或 forwarder 的系统 DirectWrite 依赖 | 需区分谁加载、从何处加载、解析哪个入口及生命周期 | 调查中 |
| 其它系统/私有原生库 | 待枚举 | 不因同处一个互操作类就归入 wpfgfx | 调查中 |

## 11. 名称/导出一致性、未使用导出与动态查找

初步确认：PresentationCore 当前源码中没有发现对 `wpfgfx` 的 `NativeLibrary.Load`、`LoadLibrary`、`GetProcAddress`、`DllImportResolver` 或硬编码路径加载；主边界由首次 `DllImport` 调用触发运行时加载。此结论只针对已搜索的当前托管源码，不能代替运行时 loader trace。

已发现的相邻动态查找不属于 wpfgfx：

- `MediaContextNotificationWindow` 从已加载的 `user32.dll` 动态查找 `ChangeWindowMessageFilter`，并以 `[UnmanagedFunctionPointer(CallingConvention.Winapi)]` delegate 调用；这是 OS 兼容探测，不是 MilCore EntryPoint。
- `DWriteLoader` 使用 `NativeLibrary.Load("dwrite.dll", ..., DllImportSearchPath.System32)` 并 `GetExport("DWriteCreateFactory")`，保存为 `delegate* unmanaged<int, void*, void*, int>`；进程退出时清空函数指针并 `NativeLibrary.Free`。

后续将分别记录：

- 托管方法名与 `EntryPoint` 不同；
- `.def` 有导出但本次静态搜索未见托管调用；
- 托管声明存在但 `.def` 未见同名；
- 可能由宏、x86 装饰、转发或生成过程解释的差异；
- `LoadLibrary`/`GetProcAddress`/运行时函数地址查找；
- 仅由 COM vtable 而非 C 导出进入的边界。

## 12. 托管调用方清单与最小端到端入口候选

### 12.1 托管调用方清单

调查中。清单将至少包含声明位置、直接包装、上层调用方、线程、所有权和错误处理。

### 12.2 首批最小端到端入口候选

调查完成前不下结论。候选只用于后续兼容验证排序，不在本文编写生产代码或最终测试方案。

## 13. 已确认事实

1. `PresentationCore.csproj` 直接编译用户指定的七个共享 Graphics 文件及 `RefAssemblyAttrs.cs`；这些不是仅供其它项目参考的旁支源码。
2. PresentationCore 当前托管 `DllImport.MilCore` 静态展开为 `wpfgfx_cor3.dll`；原生聚合目标名为 `wpfgfx$(WpfVersionSuffix)`，两者最终产物一致性尚待构建验证。
3. 已读 MilCore 声明使用传统 `DllImport`；在已执行的 PresentationCore `LibraryImport(` 搜索中未发现结果。受搜索覆盖与条件源码限制，本文暂不声称整个仓库绝对不存在 `LibraryImport`，但当前主调用面不是 source-generated P/Invoke。
4. `UnsafeNativeMethodsMilCoreApi.cs` 同时包含 `wpfgfx`、`WindowsCodecs.dll`、`WindowsCodecsExt.dll`、`mscms.dll` 和 `ole32.dll` 声明；类/文件共置不是 DLL 归属证据。
5. DUCE native handle 是显式 32 位 `UInt32`，而 COM/native 对象指针使用 `IntPtr`/SafeHandle；两者释放协议分别是 channel resource release 和 `MILRelease`。
6. 本轮没有也不会执行命令行、构建、测试、二进制导入/导出枚举或脚本统计。
7. 当前静态源码清单确认 109 个 MilCore `DllImport` 声明出现；该数字按托管声明计重载，尚未去重为唯一 native EntryPoint，也未证明每个声明运行时可达。

## 14. 推断

| 推断 | 置信度 | 静态依据 | 待确认方式 |
|---|---|---|---|
| PresentationCore 可能通过共享 Graphics 源文件集中生成/复用大部分 MilCore 声明 | 中 | 用户指定的共享 `wgx_*.cs` 与 PresentationCore MilCore API 文件分层命名 | 读取项目包含关系、声明和调用点 |

## 15. 风险

- 同一托管互操作类可能同时声明多个 DLL 的入口，若按类名归类会混淆库边界。
- 未显式写出的 `DllImport` 默认值可能因平台、运行时或字符集推断产生 ABI 风险。
- x86 stdcall 导出装饰、`.def` 预处理和托管 `ExactSpelling` 之间可能存在仅在特定架构暴露的差异。
- SafeHandle 的 `ReleaseHandle` 只能说明托管释放入口，不能单独证明 native 对象的引用计数、线程亲和性或父子生命周期。
- generated command 结构即使托管字段相同，也必须逐项核对 native packing、枚举基础类型、可变长度数据和协议版本。
- 静态文本搜索可能遗漏源生成、条件编译、COM vtable 调用和通过 `IntPtr` 传播的回调。
- SafeHandle finalizer 路径中执行 `HRESULT.Check`（`SafeMediaHandle`）具有托管异常/终结器语义风险；当前只记录源码事实，不能在迁移时擅自改成忽略错误或统一释放。
- delegate 没有显式 `UnmanagedFunctionPointer` 时依赖默认委托调用约定；几何、事件、Stream 和 D3DImage 回调都必须与 native typedef 单独核对。

## 16. 未确认项

- PresentationCore 当前构建实际使用的 `wpfgfx` DLL 常量、版本后缀和各架构名称。
- 全量 MilCore P/Invoke 的精确数量。
- 109 个源码声明中去重后的唯一 native EntryPoint 数，以及哪些声明在当前构建/运行路径真正可达。
- 所有声明是否都进入当前目标框架/配置，是否存在条件编译排除。
- `.def`、源码定义、托管声明和实际二进制导出四者是否完全一致。
- 正常进程退出时托管 shutdown/finalizer 与原生进程终止跳过 detach 的实际交互。
- 动态加载路径、模块搜索路径和运行时解析结果。

## 17. 因禁令未运行的验证

- 未构建 PresentationCore 或 `wpfgfx`；
- 未运行任何单元、集成或端到端测试；
- 未枚举实际 DLL 导出表、导入表、依赖项或文件版本；
- 未查看 MSBuild 预处理结果、binlog、编译/链接命令或生成器输出；
- 未用脚本统计声明、调用点、结构数量或 `.def` 差异；
- 未加载 DLL 验证搜索路径、初始化顺序、回调线程或卸载行为。

## 18. 给主 ABI 文档的输入

调查完成后提供：

- 托管 DLL 常量和加载顺序；
- 按功能域整理的托管声明 → native EntryPoint → `.def`/定义映射；
- P/Invoke 元数据和结构布局风险；
- SafeHandle/COM/裸指针所有权矩阵；
- 回调和 generated command 协议入口；
- 名称差异、未使用导出和动态查找清单；
- 首批最小端到端兼容入口候选；
- 必须留待构建、二进制和运行验证的项目。

## 19. 后续动作

1. 定位并读取用户指定的共享 Graphics 与 PresentationCore 关键文件。
2. 枚举所有 MilCore DLL 常量、导入特性、SafeHandle、回调和动态加载相关符号。
3. 回溯模块初始化、工厂、通道、组合线程和关闭路径。
4. 与 `wpfgfx.def` 及 native 定义逐域交叉核对。
5. 持续把事实和风险增量写入本文，不等待最终汇总。

## 20. 过程更新记录

- 已读取 `WpfGfxShape/Docs/00-migration-charter.md` 全文。
- 已读取 `WpfGfxShape/Docs/01-native-source-topology.md` 全文。
- 已创建本文档骨架并记录调查边界、工具禁令、证据分类和全部必备章节。
- 已确认 PresentationCore 项目直接编译共享 Graphics 互操作/协议文件和 `RefAssemblyAttrs.cs`。
- 已读取共享声明、核心类型、错误码、generated command 和首批 SafeHandle 文件；已把 DLL 常量、初步声明域、元数据例外和所有权证据增量写入本文。
- 已完成当前编译输入的 MilCore 声明分区枚举：10 个文件、109 个传统 `DllImport` 声明出现；未发现当前边界使用 `LibraryImport`。
- 已补录 EventProxy、StreamAsIStream、D3DImage、SafeReversePInvokeWrapper、SafeMediaHandle 和疑似未编译 GlyphCache 的回调/所有权事实。
