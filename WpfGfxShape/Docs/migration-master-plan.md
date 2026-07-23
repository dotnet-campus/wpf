# wpfgfx 全量迁移到 C#/.NET 10 NativeAOT 总计划

> 状态：权威总体计划  
> 当前阶段：调查与规划已完成；尚未创建新项目、构建、发布或翻译生产实现  
> 下一唯一工作包：`WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`  
> 任务顺序细则：`03-migration-order.md`  
> 测试门禁：`06-testing-strategy.md`  
> 最新静态复核：`investigations/05-planning-audit-and-source-revalidation.md`

## 1. 项目目标

将整个 WPF `wpfgfx` 生产层逐步迁移为一个独立的 .NET 10 C# 生产项目，使用 Native AOT shared library 生成候选 `wpfgfx_cor3.dll`，保持现有 C ABI、数据布局、调用约定、COM/handle 所有权、线程、回调、资源和生命周期兼容。

另建独立测试项目与 native caller/harness。原 WPF/wpfgfx 源码、项目和解决方案保持只读；新项目不能通过引用或复制原源码掩盖迁移缺陷。

## 2. 不可违反原则

1. **正确性与可验证行为等价第一。**
2. **ABI、布局、调用约定、所有权和生命周期第二。**
3. **初次实现逐原生文件近似直译。** 原实现文件原则上对应一个主要 C# 文件。
4. **等价验收前禁止重构。** 不抽象重组、不更换算法、不优化、不改变并发/缓存/错误/释放顺序、不顺手清理。
5. **单一生产项目不等于架构扁平化。** 保留 `common/core/shared` 和原子目录边界。
6. **没有 Ledger 和预定义测试不得编码。**
7. **没有差分/集成证据不得称完成。**
8. **未实现生产入口不得返回假成功或批量 `E_NOTIMPL`。**
9. **重要事实、决策、偏差和阻塞必须写入文档与交接。**
10. **任何技术例外必须由用户明确批准。**

若本文与 `00-migration-charter.md` 冲突，以章程为准。

## 3. 当前事实基线

### 3.1 原生规模

- 当前解决方案加载 24 个 WpfGfx 生产项目：1 个最终 DynamicLibrary + 23 个 StaticLibrary。
- `wpfgfx.vcxproj` 有 25 个逻辑项目依赖：23 个已加载 WpfGfx 静态库、`bilinearspan` 供应槽位和一个逻辑 `OSVersionHelper`。
- 当前直接项目项下限：461 个 `ClCompile`，约 22 个 PCH 创建单元和 439 个其它编译单元；含 1 个 WpfGfx 外 `dwriteloader.cpp`。
- 生产 `ResourceCompile` 直接项为 2 个：`milcore.rc`、`hw.rc`。
- 最大直接编译域：resources 98、hw 70、uce 43、core/common 42、av 34。
- 上述不是完整迁移分母；头、inl、生成物、资源、构建文件和运输包仍需机器 Ledger 闭包。

### 3.2 ABI

- 当前调用方 DLL 名：`wpfgfx_cor3.dll`。
- `.def` 静态名称 106 个。
- 托管 MilCore 声明出现 109 个，唯一 native EntryPoint 107 个。
- 交集 99，托管独有 8，`.def` 独有 7。
- 原生目标架构：Win32/x86、x64、ARM64；x86 默认 stdcall，公开 `.def` 名未装饰。

### 3.3 生成与构建

- `.def -> wpfgfx.i` 预处理；
- processed `wgx_core_types/wgx_av_types` 是历史冻结输出，当前构建复制而非重生成；
- MilCodeGen 以 `Resource.xml/Resource.xsd` 生成命令、资源、router/factory/marshal/RenderData；
- AV 使用 WPP `.tmh`；HW 使用 `fxc.exe` 生成 shader binary 并经 `hw.res` 进入最终 DLL；
- 新项目若直接创建会继承 WPF 根 Arcade props/targets，必须先局部隔离。

### 3.4 Silk.NET

本地 `Silk.NET` 为 2.11/2021 代码线，覆盖 D3D9/D3D9Ex、DXGI、DXVA、D3DCompiler，但缺少 DWrite、WIC、D2D 和主要媒体栈；D3D9 COM vtable 存在 Cdecl/x86 风险。采用方式必须经 `WP-00G` 验证。

## 4. P0 决策门

在承诺“完整替换”或大规模依赖相关能力前，必须关闭：

| 门 | 当前状态 | 解除条件 |
|---|---|---|
| Windows x86 Native AOT shared library | `Blocked` | 官方/目标 SDK 支持 + publish + PE + stdcall/native caller |
| Native AOT unload/reload | `Blocked` | 官方支持边界 + load/call/free/reload 与线程/callback stress |
| AOT-safe COM-like vtable/object | `Blocked` | C++ caller 验证 IUnknown、vtable、IID、refcount、GC root、apartment |

平台不支持时不得自行删除 Win32、修改调用方或泄漏 DLL；必须请求用户裁决。

## 5. 工作包 ID 与会话粒度

### 5.1 ID

- `WP-00A...WP-00I`：项目/ABI 机制与事实工作包；
- `WP-01x`：基础类型/布局；
- `WP-02x`：shared util/DLL 生命周期；
- `WP-03x`：common/shared/DynamicCall；
- `WP-04x...WP-14x`：对应原 4-14 批次；
- 文件包建议：`WP-<批次><序号>-<原文件或协议族短名>`；
- Ledger ID 独立稳定，工作包变化不改变文件 Ledger ID。

### 5.2 每轮粒度

- 纯逻辑叶子：2-5 个紧密相关实现文件；
- 普通有状态文件：1-3 个；
- ABI、初始化、线程、COM、生成协议、循环枢纽：1 个主实现文件或 1 个明确骨架包；
- 超大文件可按连续职责段分轮，但原物理文件仍是主 Ledger 项；
- 每轮只有一个主目标。

## 6. 总体工作包树

### 批次 0：独立项目与可行性门

#### `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`

创建局部 props/targets、独立 solution、一个生产项目、一个测试项目、`common/core/shared` 空目录边界、非生产 ABI probe 源码/内部测试和 native caller 承载。不得翻译生产逻辑或引用 Silk.NET；Native AOT publish、PE export 和 native call 明确留给 `WP-00B`。

**Done**：项目图隔离；不导入 WPF Arcade/Testing；不引用原 WPF；输出按配置/RID 隔离；probe/caller 源结构存在；restore/普通 build/托管测试和导入证据准确记录；publish/export/native call 以 `NotRun-ByWorkPackageScope` 交给 `WP-00B`。

#### `WP-00B-NATIVEAOT-ABI-PROBE-X64`

完成 x64 Debug/Release Native AOT publish、PE/export、动态/静态 caller、异常封锁、并发首次进入和符号检查。

**Done**：`E2E-00` x64 两配置通过，无全局 warning suppression；只证明 probe 机制。

#### `WP-00C-ARCHITECTURE-AND-X86-DECISION`

在真实 ARM64 Windows 重复矩阵；读取官方/SDK证据并裁决 x86。若 x86 可用，增加 stdcall/ESP/未装饰名；否则升级用户决策。

**Done**：ARM64 结果与 x86 明确状态落盘。

#### `WP-00D-LINKER-RESOURCE-VERSION-SPIKE`

验证 `.def`、import library、额外 object/resource、VERSIONINFO、ETW、shader RCDATA、PDB 及 Debug data export。

**Done**：产物级差分和支持边界明确；不迁移生产资源逻辑。

#### `WP-00E-COM-VTABLE-SPIKE`

用受控 IUnknown-like 对象验证 AOT 对外 vtable、QI/AddRef/Release、GC root、线程/apartment。

**Done**：原生 C++ caller 跨目标架构通过，或形成明确阻塞。

#### `WP-00F-CALLBACK-LIFECYCLE-UNLOAD-SPIKE`

验证同步/长期 callback、CoreCLR token 不透明往返、异常、detach、在途调用、线程 join、process exit 和受支持 unload/reload。

**Done**：生命周期能力与限制有官方和运行证据。

#### `WP-00G-SILKNET-ADOPTION-SPIKE`

比较最小 ProjectReference、冻结生成源码、局部手写三种策略；验证 D3D9/D3D9Ex 代表 API 的 AOT、调用约定和布局。

**Done**：每个 API 家族采用方式有书面裁决；缺口进入 Ledger。

#### `WP-00H-ORIGINAL-BINARY-BASELINE`

冻结原 DLL 的 export/import/resource/version/PDB/hash 和关键运行行为。

**Done**：106/107/99/8/7、`g_fNoMeterChecks`、各架构/配置有真实基线。

#### `WP-00I-MACHINE-READABLE-LEDGER`

合并项目项、磁盘、include/PCH、构建导入、ABI、生成、资源与外部供应视图。

**Done**：每个物理文件/构建项有稳定 ID、批次、状态、映射和测试门；完整性可机器校验。

### 批次 1：基础类型、错误码和布局

迁移固定宽度标量、HRESULT/WGX 错误、enum、GUID/RECT、三类 handle、descriptor/packet/command 基础布局。生成族 `GENPAIR-CORE-TYPES` 和 `GENPAIR-AV-TYPES` 横跨本批与批次 7。

**工作包规则**：按类型家族，不按“所有基础类型”一次完成。

**Done**：x86/x64/ARM64 native/managed size/offset、enum/error 值和 version 行为通过；未知类型保持 `Blocked`。

### 批次 2：shared util 与 DLL 生命周期基础

逐文件翻译 `UtilLib`、`DebugLib`、`DllUtil` 的必要生产行为，建立 attach/detach/process-exit 事件序列。

**Done**：进程堆、DebugLib fallback、CRT/用户入口差异、显式卸载和终止短路有差分；不把它们合并为 `IDisposable`。

### 批次 3：common/shared 与 DynamicCall

逐文件翻译 COM/refcount、内存/数组/region/pixel 基础、锁、CPU feature、cache、动态模块和函数指针加载。

**Done**：AddRef/Release/QI、分配失败、锁/释放、loader 缺失/错误和核心纯逻辑差分通过。

### 批次 4：geometry、scanop、effects

先处理可独立差分的叶子，再处理有 sink/callback/平台依赖的文件。首个锁定真实包为 `WP-04A-EXACT-ARITHMETIC`，但只有批次 0-3 直接前置满足后才能领取。

**Done**：目标文件逐文件达到 `DifferentialVerified`；浮点、舍入、颜色、退化几何和 COM effect 引用有证据。

### 批次 5：FXJIT

按 Platform → Compiler/Collector → PixelShader 的真实依赖逐文件迁移，不替换编译器/算法。

**Do not start**：原 FXJIT code bytes/结果、W^X/CFG/cache/unwind/架构探针未完成。

**Done**：适用架构生成和执行结果等价；不可行时走用户批准例外，不静默改解释器或 GPU 路径。

### 批次 6：core/common、control、targets、glyph

逐文件迁移数学、loader/display/RenderOptions、诊断控制、render-target 基类和 glyph 基础。循环处只建必要原名骨架。

**Done**：数学/loader/target stack/glyph 数据差分；生命周期参与者接入持续事件序列。

### 批次 7：生成协议

工作包必须包含模型/emitter、冻结输出、托管/原生两侧和至少一个 producer/consumer。覆盖 `GENPROTO-MILCMD`、`GENPROTO-DUCE-RESOURCES`、RenderData 和 SDK fingerprint。

**Done**：全部 ID、size/offset/pack、golden bytes、router/factory 映射和负例通过；generator 只在隔离目录重放。

### 批次 8：resources 与 UCE

- `8A`：循环类型/签名/布局骨架；
- `8B`：handle table 与 connection/channel；
- `8C`：command batch 与资源生命周期；
- `8D`：resource update 与 generated dispatch；
- `8E`：composition/partition/thread/scheduler；
- `8F`：UCE ABI 家族与最小集成。

**Done**：`E2E-02`、`E2E-03`；最小 connection→channel→resource→command→message 闭环；未实现命令绝不假成功。

### 批次 9：软件渲染

逐文件迁移 rasterizer、scan pipeline、brush/glyph、software targets、double buffer、GDI present 和 `bilinearspan` 供应边界。

**Done**：`E2E-04` 软件像素与 `E2E-05` 真实 PresentationCore `RenderTargetBitmap`；确定性像素差分通过。

### 批次 10：硬件渲染

逐文件迁移 D3D9/D3D9Ex device/resource/surface/texture/swapchain/pipeline/shader/glyph/HWND 和 SW fallback。Silk.NET 只按 `WP-00G` 裁决使用。

**Done**：`E2E-06`、`E2E-07`；设备创建/reset/lost、resource refcount、shader resource、fallback 和 D3DImage detach/callback 通过。

### 批次 11：AV

逐文件迁移 COM apartment、state/event thread、WMP/DirectShow/EVR/DXVA、media buffer/surface、event proxy 和三类结束动作。

**Done**：`E2E-08`；open/event/frame/stop/close/shutdown/process-exit、callback thread 和超时行为有差分。

### 批次 12：meta 与 API

逐文件迁移 backend 组合/选择、factory、WIC/codec、stream、media、bitmap 和其它外部对象桥接。导出仍是薄层。

**Done**：主要 COM/API 对象能由兼容 ABI 创建/释放，后端选择与原实现一致。

### 批次 13：DLL、完整导出和二进制契约

收口完整 startup/shutdown、106/107 ABI、文件名、架构、资源、版本、符号和 candidate DLL 替换门。不是首次实现生命周期。

**Done**：各目标架构/配置的 export/layout/ownership/callback/lifecycle/PE/resource 全矩阵通过；8/7 差异有证据裁决。

### 批次 14：端到端收口

扩展并收口真实 PresentationCore 的 software、hardware、HWND、D3DImage、media、多 Dispatcher、失败、设备丢失和退出矩阵。

**Done**：`E2E-09`；所有目标文件状态、偏差和阻塞收口。一个 happy path 不构成完成。

## 7. 渐进 E2E 路线

`E2E-00 probe → E2E-01 version/COM → E2E-02 channel → E2E-03 resource command → E2E-04 software pixels → E2E-05 RenderTargetBitmap → E2E-06 HW/HWND → E2E-07 D3DImage → E2E-08 media → E2E-09 lifecycle`

每个工作包只解锁其真实能力；不得把较早级别的通过外推到未迁移子系统。

## 8. 每轮会话执行契约

开始：

1. 读 `00-migration-charter.md`；
2. 读 `next-session-handoff.md`；
3. 读当前工作包直接相关文档/源码；
4. 确认唯一工作包 ID、Ready 条件和停止点；
5. 更新/建立 Ledger 行，再编辑代码。

执行：

- 只做一个主目标；
- 先建立原基线和测试，再或同步逐文件翻译；
- 新依赖使当前文件不 Ready 时立即标记 `Blocked`，不要悄悄扩大范围；
- 不做重构、优化、重命名、helper 合并或无关清理；
- 每个状态提升必须有证据。

结束：

- 更新 Ledger、主计划/专题中的新事实；
- 记录已运行与未运行验证；
- 更新 `next-session-handoff.md`；
- 只指定一个下一主目标；
- 工作包未满足 done 时不得切换到下一包。

## 9. 完成标准

整个迁移只有在以下条件全部满足时才可称完成：

- 完整机器 Ledger 中所有生产范围项有最终状态和证据；
- 目标架构/配置 ABI、布局、COM、callback、resource、version、symbol 兼容；
- 所有逐文件实现均可追溯，无未记录重构或算法替换；
- 原与 candidate 的规定差分矩阵通过；
- 真实 PresentationCore E2E 全矩阵通过；
- P0 门已关闭或由用户批准明确范围例外；
- candidate 才可被描述为 `wpfgfx_cor3.dll` 的兼容替代。

等价迁移完成后，任何现代化、性能优化、架构重构必须另立计划与基线。
