# 本地 Silk.NET 与 wpfgfx 平台互操作评估

> 状态：静态评估完成；采用方式待 `WP-00G-SILKNET-ADOPTION-SPIKE` 运行验证  
> 本地仓库：`D:/lindexi/Code/OriginWpf/Silk.NET`  
> 当前检出：`main` @ `ed4b3c299a44e8b6936f6b1c61c0b119e82237cd`  
> 上位约束：Silk.NET 只作为低层互操作工具，不得改变原算法、COM 生命周期或错误语义

## 1. 结论

本地 Silk.NET 可以作为 D3D9/D3D9Ex、DXGI、DXVA 和 D3DCompiler 的重要低层类型/常量/函数声明来源，但**不能直接作为整个 wpfgfx 平台层的现成解决方案**。

主要原因：

- 本地版本线为 Silk.NET 2.11（文档标注 December 2021 Update），`global.json` 使用 SDK 6.0.100；
- Microsoft bindings 的项目目标主要停留在 `netstandard2.0/2.1`、`netcoreapp3.1`、`net5.0`，Core 额外有 `net6.0`；
- 未发现 DirectWrite、WIC、Direct2D、Media Foundation、EVR、DirectShow 或 Windows Media Player 绑定；
- D3D9/DXVA 生成的 COM vtable 调用大量使用 `delegate* unmanaged[Cdecl]`，而 Win32 COM/WINAPI 的 x86 调用约定是关键兼容门；
- Silk.NET.Core 默认通过 `LoadLibrary`/`GetProcAddress` 动态加载，释放时 `FreeLibrary`，其搜索路径、生命周期和 AOT 行为不能自动等同于 wpfgfx 的惰性 loader；
- bindings 依赖 Silk.NET.Core、Silk.NET.Maths 和 SilkTouch source generator，整体项目引用会带入较旧包图和额外运行时基础设施。

因此首选策略是：**先把本地生成源码当作可审计的低层候选来源；经过代表性 ABI/AOT spike 后，再在“最小项目引用、冻结所需生成源码、局部修正/手写互操作”之间做书面裁决。**

## 2. 当前仓库事实

### 2.1 版本与构建

- Git HEAD：`ed4b3c299a44e8b6936f6b1c61c0b119e82237cd`。
- `build/props/common.props`：`VersionPrefix=2.11`，release notes 为 December 2021 Update。
- `Silk.NET/global.json`：SDK `6.0.100`，`rollForward=major`。
- Microsoft bindings 项目使用 `Microsoft.NET.Sdk`、unsafe 和 preview language。
- `bindings.props` 自动引用 `Silk.NET.Core` 和作为 analyzer 的 `Silk.NET.SilkTouch`。

### 2.2 已发现的 Microsoft bindings 项目

| 项目 | 与 wpfgfx 的关系 |
|---|---|
| `Silk.NET.Direct3D9` | 直接相关；含 `Direct3DCreate9/Ex`、D3D9/D3D9Ex COM 类型 |
| `Silk.NET.DXGI` | 部分相关；需按实际调用点确认 |
| `Silk.NET.DXVA` | AV/DXVA 相关；依赖 D3D9 和 Win32Extras |
| `Silk.NET.Direct3D.Compilers` | D3DCompiler 相关；默认加载 `D3DCompiler_47.dll` |
| `Silk.NET.Direct3D11` | 不是原 wpfgfx D3D9 路径的替代品 |
| `Silk.NET.Direct3D12` | 不属于首次等价迁移目标 |
| `Silk.NET.XAudio` | 非 wpfgfx 核心媒体状态机替代方案 |
| `Silk.NET.XInput` | 与当前 wpfgfx 主迁移无直接关系 |

## 3. 覆盖矩阵

| 平台/API 面 | 本地 Silk.NET | 结论 |
|---|---|---|
| D3D9 | 有 | 可作为候选；必须验证结构、vtable、loader、HRESULT 和 x86 调用约定 |
| D3D9Ex | 有 | 可作为候选；同上 |
| DXGI | 有 | 只在原调用点确实需要时采用，不扩大为 D3D11/12 重写 |
| DXVA | 有 | 只覆盖部分 AV 视频加速面；不能代替 EVR/WMP/DirectShow 状态机 |
| D3DCompiler 47 | 有 | 默认 DLL 名与 WPF 私有 `_cor3` 重命名部署语义不同，必须保留原加载/部署行为 |
| DirectWrite | 未发现 | 需要低层手写/其它经批准绑定；保持 System32 惰性加载与错误映射 |
| WIC | 未发现 | 需要低层 COM 互操作；包括 WIC proxy、factory、bitmap/codec |
| Direct2D | 未发现 | 若真实调用点需要，单独登记；不能从 DXGI 覆盖推断 |
| DWM | 未形成专用绑定结论 | 按 Win32 动态加载/API 清单处理 |
| GDI/User32/Kernel32/OLE/COM | Core.Win32Extras 仅部分覆盖 | 按调用点逐项验证或手写，不依赖“Win32Extras”名称推断完整性 |
| Media Foundation | 未发现 | AV 专题缺口 |
| EVR | 未发现 | AV 专题缺口 |
| DirectShow | 未发现 | AV 专题缺口 |
| WMP COM | 未发现 | AV 专题缺口 |

## 4. 调用约定风险

代表性静态证据：

- `Silk.NET.Direct3D9/Structs/IDirect3D9*.gen.cs` 的 QueryInterface、AddRef、Release 和多数 vtable slot 使用 `unmanaged[Cdecl]`；
- `Silk.NET.Core/Native/Structs/IUnknown.cs` 同样使用 `unmanaged[Cdecl]`；
- 部分 DXGI/DXVA 生成代码的 AddRef/Release 使用 `unmanaged[Stdcall]`，同一生成树中存在混合；
- `NativeApiAttribute` 的默认 Convention 为 Cdecl；
- 原 WPF x86 native C++ 默认 `StdCall`，COM/WINAPI 兼容必须在 32 位 caller 中验证栈平衡。

在 x64/ARM64 上，Cdecl/Stdcall 关键字通常落到平台统一 ABI，可能让错误绑定表面工作；这不能证明 Win32 正确。`WP-00G` 必须至少验证：

1. `Direct3DCreate9`/`Direct3DCreate9Ex` 精确导入；
2. IUnknown 三槽；
3. 一个简单 D3D9 方法；
4. 一个含结构/out pointer 的方法；
5. x86 重复调用和栈平衡；
6. x64/ARM64 的结构和返回值；
7. Native AOT analyzer/publish 与运行。

未通过前不得把本地 D3D9 COM struct 直接用于生产 HW 批次。

## 5. Loader 与生命周期风险

Silk.NET 的默认 `D3D9.GetApi()` 创建 `DefaultNativeContext`，后者使用 `UnmanagedLibrary` 和平台 `LibraryLoader`：

- Windows 使用 `LoadLibrary(name)`；
- symbol 使用 `GetProcAddress`；
- Dispose 使用 `FreeLibrary`；
- 默认 D3D9 名为 `d3d9.dll`；
- 默认 D3DCompiler 名为 `D3DCompiler_47.dll`。

这与 wpfgfx 的以下语义不自动等价：

- D3D/DWrite/AV loader 的惰性初始化锁、引用计数和失败归一化；
- System32-only 搜索边界；
- WPF 私有 `D3DCompiler_47_cor3.dll` 部署/链接重命名；
- 显式 shutdown 与进程终止差异；
- Native AOT shared library 自身 unload 风险。

生产迁移应优先保留原 loader 文件的逐文件控制流。即使复用 Silk.NET 类型，也不应直接调用 `GetApi()` 取代原 loader，除非差分证明加载路径、时机、错误和释放完全一致。

## 6. AOT 与依赖图风险

整体项目引用会引入：

- `Silk.NET.Core` 的动态 loader、vtable cache、GC utility 和 delegate helper；
- `Silk.NET.Maths`；
- SilkTouch source generator；
- 较旧的 `Microsoft.DotNet.PlatformAbstractions`、`Microsoft.Extensions.DependencyModel` 等包；
- 某些扩展 API 使用 `Activator.CreateInstance`。

这不代表这些代码一定不兼容 Native AOT，但会扩大 trim/AOT 分析面，并削弱新项目与仓库的构建隔离。必须由 `WP-00G` 比较三种采用方式：

### 方案 A：最小本地 ProjectReference

只引用确有需要的 bindings 及其依赖。优点是保留上游生成组织；缺点是旧 TFM/包图/source generator/loader 全部进入项目。

### 方案 B：冻结所需生成源码

将经许可和审计的必要 struct/enum/constant 作为第三方来源快照纳入独立边界，记录 commit、原路径、许可证和差异；loader 与原 wpfgfx 文件自行逐文件翻译。优点是最小 AOT 面；缺点是需维护来源和更新差分。

### 方案 C：局部手写互操作

对缺失 API、错误调用约定或只需极少 slot 的接口，按 Windows SDK 原型手写最低层 unsafe 定义。必须记录为什么不能直接使用本地生成物，并建立布局/vtable 测试。

这三种可以按 API 家族混合使用，但每个家族必须只有一个权威定义，禁止同一 COM 接口同时存在两套不一致 struct。

## 7. WP-00G 完成条件

- 固定本地 Silk.NET commit 和许可证/来源记录；
- 对 D3D9/D3D9Ex、DXGI、DXVA、D3DCompiler 建立“覆盖/缺口/实际原调用点”表；
- 代表性 API 在目标 Native AOT x64/ARM64 和可用时 x86 通过布局、vtable、HRESULT、调用约定与 loader 测试；
- 明确 D3D9 生成 Cdecl 风险如何处理；
- 比较 A/B/C 三种采用方式的实际项目图、AOT warning 和产物依赖；
- 对每个 API 家族形成书面采用裁决；
- 未覆盖的 DWrite/WIC/D2D/媒体/Win32 面进入 Ledger 和对应后续批次；
- 不修改 Silk.NET 仓库、不升级它、不重写 wpfgfx 算法。

## 8. 对迁移批次的影响

- 批次 3/6 的通用 Win32/COM/loader 不应等待 Silk.NET；按原文件低层翻译。
- 批次 10 HW 在 `WP-00G` 完成前保持 D3D9 互操作 `Blocked`。
- 批次 11 AV 只能把 DXVA 视为一个子面；Media Foundation/EVR/DirectShow/WMP 仍需独立互操作工作包。
- 批次 12 WIC/API 不可依赖本地 Silk.NET 覆盖；须提前建立 WIC COM 方案。
- DirectWrite 保持原 System32 动态加载专题，不与 D3D9 绑定混合。

## 9. 维护规则

- 只有 `WP-00G` 的实际 build/publish/caller 证据能把“候选”改为“可采用”。
- 后续若切换 Silk.NET commit，必须重新执行调用约定、布局和 AOT 门禁。
- 不得以“Silk.NET 支持 DirectX”为理由扩大到 D3D11/12 重写；原 wpfgfx 首次迁移仍以 D3D9/D3D9Ex 等价为目标。
