# wpfgfx 原生到 C# 文件映射

> 状态：权威实施对照表  
> 范围：仅记录已经开始或完成生产翻译的文件  
> 更新规则：每次新增、移动或扩展生产翻译时同步更新；不得用本表替代机器可读 Ledger 的全量范围管理

## 规则

- 原则上一个原生 `.cpp` 对应一个主要 C# 实现文件，并尽量镜像原目录边界。
- 一个 C# 文件暂时承载多个原生来源时，必须逐项列出具体切片和拆分计划。
- 公共低层互操作辅助可被多个映射复用，但不得把不同原生模块的业务职责继续集中到单个大文件。
- 状态使用 `Partial`、`Complete` 或 `Planned`；当前所有条目均是切片翻译，不代表整个原生文件已完成。

## 当前映射

| 原生 C++ 文件 | 主要 C# 文件 | 已翻译切片 | 状态 | 验证 |
|---|---|---|---|---|
| `core/common/d3dloader.cpp` | `Code/WpfGfxShape/Core/Direct3D9Factory.cs` | 系统 `d3d9.dll` 加载、`Direct3DCreate9/Ex` 解析与创建、基础/Ex 接口获取、失败清理 | Partial | `Direct3D9FactoryTests`、解决方案构建 |
| `core/hw/d3ddevicemanager.cpp` | `Code/WpfGfxShape/Core/Direct3D9Objects.cs` | adapter 数量、显示模式/caps 查询、桌面窗口参数、基础/Ex device 创建、兼容性重试、对象释放 | Partial | `Direct3D9FactoryTests`、解决方案构建 |
| `core/hw/d3ddevice.cpp` | `Code/WpfGfxShape/Core/Direct3D9Device.cs` | cooperative/Ex 状态检查、render target 创建、additional swap chain 创建、设备 COM 生命周期 | Partial | `Direct3D9FactoryTests`、解决方案构建 |
| `core/hw/d3dsurface.cpp` | `Code/WpfGfxShape/Core/Direct3D9Surface.cs` | surface 所有权、`GetDesc`、释放后保护 | Partial | `Direct3D9FactoryTests`、解决方案构建 |
| `core/hw/d3dswapchain.cpp` | `Code/WpfGfxShape/Core/Direct3D9SwapChain.cs` | present parameters、MONO back buffer、Present 原始状态、swap chain 生命周期 | Partial | `Direct3D9FactoryTests`、解决方案构建 |
| `core/hw/d3ddevice.cpp` | `Code/WpfGfxShape/Core/Direct3D9SwapChain.cs` | `PresentWithD3D` 的最小全区域 Present 调用语义由 swap chain 包装承载 | Partial | `Direct3D9FactoryTests`、解决方案构建 |

## 共享基础文件

| C# 文件 | 来源/用途 |
|---|---|
| `Code/WpfGfxShape/Core/DirectXModule.cs` | DirectX 模块名、入口名和 SDK 常量；服务于 `d3dloader.cpp` 切片 |
| `Code/WpfGfxShape/Core/DirectXSystemModule.cs` | CsWin32 `LoadLibraryExW`/`FreeLibrary` 与模块 SafeHandle 所有权；服务于 `d3dloader.cpp` 切片 |
| `Code/WpfGfxShape/Core/DirectXBindingInfo.cs` | 当前 Silk.NET DirectX 绑定版本事实 |

## 已知边界

- `Direct3D9Factory.cs` 原先混合了上述五个原生实现文件的职责，现已按主要原生文件边界拆分。
- `Direct3D9DeviceState` 暂放在 `Direct3D9Device.cs`，因为状态分类当前主要服务于 `d3ddevice.cpp` 的设备检查，同时被 swap-chain Present 结果复用。
- `Direct3D9Factory.Release` 是当前显式 `Stdcall` COM ABI 的共享低层辅助；后续只有在原生文件映射和行为验证允许时才继续拆分，不提前引入新的抽象层。
- 机器可读 Ledger 当前仍处于 `RepairRequiredPartial`；本表用于维护已实施代码的清晰路径映射，Ledger 修复不得阻塞可验证生产翻译。
