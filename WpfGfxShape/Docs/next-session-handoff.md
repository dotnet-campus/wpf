# 下一轮交接：继续推进 wpfgfx 代码翻译

> 本文件是下一轮唯一权威入口  
> 已完成基础脚手架：`WP-00A`、`WP-00B`  
> 当前执行方向：持续交付可构建、可测试的生产代码切片  
> DirectX 技术选型：Silk.NET 2.23.0  
> Win32 技术选型：优先 Microsoft.Windows.CsWin32 0.3.298

## 1. 当前决定

现有文档已经足以支持编码。不得再以补全 Ledger、扩写迁移计划或继续大范围静态调查作为生产翻译的前置条件。

每轮应先扫描本文件，然后直接选择可独立验证的原生切片，完成近似直译、测试和构建。文档只记录代码实施产生的新事实。

## 2. 已完成的生产翻译

生产项目已经接入：

- `Silk.NET.Direct3D9` 2.23.0；
- `Silk.NET.Direct3D11` 2.23.0；
- `Silk.NET.DXGI` 2.23.0；
- `Microsoft.Windows.CsWin32` 0.3.298。

已完成 `core/common/d3dloader.cpp` 的首个低层切片：

- 定义 `d3d9.dll`、`d3d11.dll`、`dxgi.dll` 模块名称和代表性导出入口；
- 使用 CsWin32 生成的 `LoadLibraryExW` 与 `FreeLibrary`；
- 使用 `LOAD_LIBRARY_SEARCH_SYSTEM32` 加载系统 DirectX 模块；
- 由于 CsWin32 的原始 `PCWSTR` 重载返回 `HMODULE`，增加最小 `SafeHandle` 包装以固定模块所有权；
- 从已加载的受控模块解析 `Direct3DCreate9` 和可选的 `Direct3DCreate9Ex`，不再使用 Silk.NET 默认 loader；
- 函数指针按原生 `WINAPI` 使用 `Stdcall`，DirectX COM 类型使用 Silk.NET；
- 优先调用 `Direct3DCreate9Ex`；`D3DERR_NOTAVAILABLE` 保持原实现的降级语义，其他失败按 HRESULT 抛出；
- Ex 创建成功后查询 `IDirect3D9`；查询失败时继续调用 down-level `Direct3DCreate9`；
- 创建失败时释放已获得的 COM 接口和模块；成功对象按 `IDirect3D9Ex`、`IDirect3D9`、模块的顺序确定性释放；
- 未采用 Silk.NET 2.23.0 生成的 D3D9 COM 调用方法，因为本地源码将 vtable 声明为 `Cdecl`，当前实现显式保持 Windows COM 的 `Stdcall` ABI；
- 已加入最小 D3D9 adapter 能力查询：`GetAdapterCount`、默认 HAL `GetAdapterDisplayMode` 和 `GetDeviceCaps`；
- 查询使用 Silk.NET 的 `Devtype`、`Displaymode`、`Caps9` 类型，但 COM vtable 槽位继续显式按 Windows `Stdcall` 调用；
- 能力查询对象在释放后拒绝继续访问；显示模式或 caps HRESULT 失败时直接抛出，不返回部分写入的组合结果；
- 已近似直译 `core/hw/d3ddevicemanager.cpp` 的最小 D3D9 device create/释放切片；
- 按原实现使用 1×1、`X8R8G8B8`、单 back buffer、`DISCARD`、窗口化且不启用自动 depth/stencil 的 `PresentParameters`；
- 原 WPF 明确避免在库中创建 dummy window，并使用 `GetDesktopWindow` 满足 D3D9.0c 的有效窗口要求；当前实现同样通过 CsWin32 生成的 `GetDesktopWindow` 承载，而不是新增自定义隐藏窗口生命周期；
- 行为标志保留 `FPU_PRESERVE`、`MULTITHREADED`、`DISABLE_DRIVER_MANAGEMENT_EX`，并依据 `D3DDEVCAPS_HWTRANSFORMANDLIGHT` 选择硬件或软件顶点处理；
- `IDirect3D9Ex::CreateDeviceEx` vtable 槽 20 优先，基础 `IDirect3D9::CreateDevice` vtable 槽 16 用于 down-level 路径；两者继续显式使用 Windows `Stdcall`；
- Ex 创建成功后查询基础 `IDirect3DDevice9`；设备对象随后通过 `QueryInterface` 保留可选 `IDirect3DDevice9Ex` 独立引用，并按 Ex、基础接口顺序确定性释放，释放后拒绝访问；
- 保留原实现对 `D3DERR_INVALIDCALL + D3DCREATE_DISABLE_DRIVER_MANAGEMENT_EX` 的兼容性重试，重试前移除该标志；每次调用前清空输出，失败时释放任何部分返回的接口并传播 HRESULT；
- 已近似直译 `CD3DDeviceLevel1::CreateRenderTargetUntracked` 的最小 render-target surface 生命周期；
- `IDirect3DDevice9::CreateRenderTarget` 使用 vtable 槽 28，`IDirect3DSurface9::GetDesc` 使用槽 12，两者显式按 Windows `Stdcall` 调用；
- render target 创建使用 Silk.NET 的 `Format`、`MultisampleType`、`SurfaceDesc` 和 `IDirect3DSurface9`，调用前清空输出，HRESULT 失败时释放部分结果并抛出，空成功结果按无效状态拒绝；
- `Direct3D9Surface` 确定性拥有并释放 COM 引用，提供描述查询，释放后拒绝访问；
- 已近似直译 `CD3DDeviceLevel1::CreateAdditionalSwapChain`、`CD3DSwapChain::Init/GetBackBuffer` 和 `PresentWithD3D` 的最小 additional swap chain 生命周期；
- `IDirect3DDevice9::CreateAdditionalSwapChain` 使用 vtable 槽 13，`IDirect3DSwapChain9::Present`、`GetBackBuffer`、`GetPresentParameters` 分别使用槽 3、5、9，全部显式按 Windows `Stdcall` 调用；
- additional swap chain 使用桌面窗口、固定窗口化 `DISCARD`、单 back buffer、无自动 depth/stencil 参数，创建前清空输出，HRESULT 失败时释放部分结果并抛出；
- `Direct3D9SwapChain` 确定性拥有 COM 引用，支持参数查询、MONO back buffer 获取和最小全区域 Present；back buffer 继续交由 `Direct3D9Surface` 独立拥有，所有对象释放后拒绝访问；
- 已近似直译设备状态检查的首个切片：基础 `IDirect3DDevice9::TestCooperativeLevel` 使用 vtable 槽 3，D3D9Ex `IDirect3DDevice9Ex::CheckDeviceState` 使用槽 128，继续显式按 Windows `Stdcall` 调用；
- `Direct3D9DeviceState` 保留原始 HRESULT 和状态来源，不把设备丢失、模式变化或遮挡等状态统一转换为异常；状态来源可区分基础 cooperative-level、D3D9Ex 检查和 Present；
- `Direct3D9SwapChain.Present` 已不再调用 `Marshal.ThrowExceptionForHR`，而是返回统一状态分类：成功为 `Operational`、`S_PRESENT_OCCLUDED` 为仍可操作的 `Occluded`、`S_PRESENT_MODE_CHANGED` 为需要重建的 `ModeChanged`，`D3DERR_DEVICELOST`/`DEVICEHUNG`/`DEVICEREMOVED` 为需要重建的 `DeviceLost`，其他结果保留为 `Failure`；
- 正常设备与遮挡状态可直接判断为可操作，模式变化和设备丢失可判断为需要重建设备；设备和 swap chain 释放后状态检查或 Present 均拒绝访问；
- 已将原本集中在 `Direct3D9Factory.cs` 的实现按原生文件边界拆分为 `Direct3D9Factory.cs`、`Direct3D9Objects.cs`、`Direct3D9Device.cs`、`Direct3D9Surface.cs` 和 `Direct3D9SwapChain.cs`，未改变现有类型名和行为；
- 已创建权威实施对照表 `Docs/native-to-managed-file-map.md`，记录已开始生产翻译的原生 C++ 文件、主要 C# 文件、具体切片、状态和验证证据。

## 3. 已完成验证

本轮已获得实际验证结果：

- `WpfGfxShape.slnx` Debug 构建成功，0 警告、0 错误；
- 解决方案全部测试运行成功：`WpfGfxShape.Tests` 47/47，`WpfGfxShape.AbiIntegration` 8/8；
- `Direct3D9FactoryTests`：29/29 通过，真实执行了 D3D9 创建、adapter 枚举、默认 adapter 显示模式、HAL caps 查询、默认 HAL device 创建/释放、1×1 back buffer 参数、2×3 render target 创建、2×3 additional swap chain 创建、present parameters 查询、back buffer 描述查询、最小 Present 原始状态返回、正常设备状态检查、D3D9/D3D9Ex 状态路径匹配和释放后访问保护；并以确定性数据覆盖遮挡、模式变化、设备丢失/挂起/移除和未知失败分类；
- 生产项目 `Release + net10.0 + win-x64` 构建成功，0 警告、0 错误。

带 RID 的构建不是 Native AOT publish。当前尚无新的 Native AOT 发布结果，不得把该构建结果记录为发布验证。

## 4. 下一步必须执行

下一轮直接继续生产翻译，优先顺序如下：

1. 继续近似直译 `CD3DDeviceLevel1::MarkUnusable`：设备首次不可用时固定一次性处理、manager 通知和资源销毁调用顺序；
2. 建立最小 `Direct3D9ResourceManager`/资源登记切片，对应 `d3dresource.cpp` 的 `DestroyAllResources`、`DestroyResource` 与 `ReleaseD3DResources` 回调边界；先覆盖 surface 和 swap chain，保持资源先释放 D3D COM 对象、再脱离 manager 跟踪的顺序；
3. 原 WPF 当前直接路径没有调用 `IDirect3DDevice9::Reset` 或 `ResetEx`，而是将设备标记不可用、通知 manager、销毁全部资源并最终销毁/重建设备；不得在没有新的原生证据时新增就地 Reset 路径；
4. 推进 manager 的 unusable device 分区、移除和按既有创建参数重建设备的最小切片，避免在多窗口场景重复发送 device-lost 通知；
5. 复用现有 `Direct3D9Device`、`Direct3D9SwapChain`、`Direct3D9Surface` 所有权和显式 `Stdcall` COM ABI，保持 HRESULT、COM 引用计数、失败清理和释放后保护；
6. DirectX 类型、枚举、结构和常量继续优先使用 Silk.NET 2.23.0；测试不得依赖主动制造真实 GPU 丢失，优先使用可确定的资源登记、销毁顺序、幂等通知和释放后保护；
7. 每次新增或移动生产翻译同步更新 `Docs/native-to-managed-file-map.md`；完成后构建解决方案并运行全部测试；
8. 在现有构建工具可表达发布目标时补做 `win-x64` Native AOT 发布与 ABI 集成验证，但不得因此阻塞后续可完成的代码翻译。

## 5. 技术边界

- Silk.NET 只承担低层 DirectX 绑定，不得用高层封装重写 wpfgfx 算法；
- 不得用 D3D11/12 替代原本要求保持等价语义的 D3D9/D3D9Ex 路径；
- 同一 COM 接口在生产代码中只能有一份权威类型定义；
- CsWin32 生成符合预期时不得重复手写 P/Invoke；
- CsWin32 无法直接表达所有权或 ABI 时，允许最小范围包装或降级，并以测试固定行为；
- 不修改原 WPF/wpfgfx 源码；
- 不把文档完成度或暂时无法执行的低优先级验证作为停止实际翻译的理由。
