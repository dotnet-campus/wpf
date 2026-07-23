# wpfgfx 迁移规划审计与源码复核

> 状态：本轮静态审计完成  
> 用途：复核既有规划是否仍由当前工作区源码支持，并记录需要修正的文档漂移  
> 上位约束：[`../00-migration-charter.md`](../00-migration-charter.md)  
> 当前唯一实施入口：[`../next-session-handoff.md`](../next-session-handoff.md)  
> 实施边界：本轮不创建项目、不编写 probe、不翻译生产代码、不构建、不发布、不运行测试

## 1. 审计目标

用户要求先阅读 `WpfGfxShape/Docs` 的既有成果，再探索当前 WPF/wpfgfx 与本地 Silk.NET，并输出可供后续多轮实施使用的计划和交接文档。本轮发现既有文档体系已包含章程、拓扑、ABI、迁移顺序、Native AOT 项目形态、Silk.NET 评估、测试、Ledger/ABI/Evidence schema、总计划、路线图和交接协议，因此本轮采用以下策略：

1. 不创建重复的第二套总体计划；
2. 完整审阅既有权威文档、调查底稿、决策和历史 handoff；
3. 用当前源码和项目文件抽样复核会影响范围、顺序、P0 门和下一工作包的关键事实；
4. 只修复会误导执行的文档冲突；
5. 保持下一轮仍只有一个工作包。

## 2. 最高原则复核

既有章程已经准确表达用户要求，本轮未发现需要修改的原则：

- 正确性和可验证行为等价优先；
- C ABI、布局、调用约定、所有权和生命周期兼容优先；
- 初次迁移按原生文件逐个近似直译；
- 原实现文件原则上对应一个主要 C# 实现文件；
- 保留原类型名、函数名、字段名、分支、错误顺序、锁顺序、引用计数和释放顺序；
- 单一 .NET 生产项目只统一构建/发布边界，不允许扁平化或合并原模块职责；
- 等价验收前禁止重构、优化、算法替换、并发/缓存变化、重命名和无关清理；
- 没有 Ledger、路径映射和预定义验证证据时不得开始生产文件翻译；
- 未实现生产行为不得用假 `S_OK`、假成功对象或批量 `E_NOTIMPL` 掩盖；
- 测试随迁移逐步增加，不能全部推迟到最后；
- 每轮结束必须维护唯一 `next-session-handoff.md`。

结论：[`../00-migration-charter.md`](../00-migration-charter.md) 继续作为不可违反的最高约束，无需另立替代章程。

## 3. wpfgfx 生产闭包复核

### 3.1 最终聚合项目

直接复核 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.vcxproj`：

- `ConfigurationType=DynamicLibrary`，`CLRSupport=false`；
- 支持 Debug/Release × Win32/x64/arm64；
- `TargetName=wpfgfx$(WpfVersionSuffix)`；
- 自身直接编译 `precomp.cpp`、`dllentry.cpp`，直接编译 `milcore.rc`；
- 链接器使用中间 `wpfgfx.i` 作为 `ModuleDefinitionFile`；
- Debug/Release 自定义入口分别为 `_DllMainStartupDebug`、`_DllMainStartup`，x86 追加 `@12`；
- 直接引用当前解决方案中的 23 个 WpfGfx 静态库；
- 另有一个 `bilinearspan` 项目引用槽位和一个逻辑 `OSVersionHelper` 项目引用。

IDE 当前解决方案项目枚举独立确认：WpfGfx 下已加载 24 个项目，其中一个最终 DLL、23 个静态库；`Shared/OSVersionHelper` 也在解决方案中，`bilinearspan` 项目未加载。

结论：[`../decisions/DEC-0001-production-scope.md`](../decisions/DEC-0001-production-scope.md) 对“整个 wpfgfx”的生产分母定义仍成立。

### 3.2 构建继承与隔离

直接复核：

- WpfGfx 子树 `Directory.Build.Props` 先向上导入根 `Directory.Build.props`，再导入 `Graphics.props`；
- 根 `Directory.Build.props` 导入 WPF Arcade SDK props；
- 根 `Directory.Build.targets` 导入 WPF Arcade SDK targets，并无条件导入 `eng/Testing.targets`；
- `eng/Testing.targets` 会对测试项目注入 hang/crash dump 包和参数；
- 根 `global.json` 当前请求 SDK `10.0.107`；
- `Graphics.props` 注入公共 include、DirectXMath、D3D9/D3DCompiler 链接，并复制 processed 类型文件。

结论：新项目必须在 `WpfGfxShape` 下用成对的局部 `Directory.Build.props` 和 `Directory.Build.targets` 截断根自动导入。只在 `.csproj` 内覆盖属性不足以证明隔离。

### 3.3 生成、资源和外部供应

直接复核：

- `GenerateModuleDefinitionFile.targets` 把 `.def` 作为 C++ 预处理输入，展开 `DLL_NAME=$(TargetName)` 并生成 `.i`；
- `Graphics.props` 当前只把 `include/processed/wgx_{core,av}_types.{h,cs}` 复制到中间目录，不在普通产品构建中重新生成；
- `core/av/av.targets` 用 TraceWPP 为实现文件生成 `.tmh`；
- `core/hw/ShaderAssemblies.targets` 用 Windows SDK `fxc.exe` 生成 `.vsbin/.psbin`，复制 `EmbeddedShaders.RC`；
- `hw.vcxproj` 把 `hw.res` 作为 `AdditionalLinkerInputs`；
- `ResourceLinking.targets` 将静态库附加 `.res` 传入最终 DLL 链接；
- `milcore.rc` 包含 native version 与 WPF ETW 资源；
- `bilinearspan.vcxproj` 当前未找到，但公共项目引用逻辑和 `ShippingProjects.props` 明确支持运输包替代；
- `core/common/common.vcxproj` 直接编译树外 `src/Microsoft.DotNet.Wpf/src/Shared/cpp/dwriteloader.cpp`。

结论：461 个直接 `ClCompile` 只能作为下限，不能充当完整迁移分母；`WP-00I` 的机器 Ledger 仍是强制前置。

### 3.4 生命周期

直接复核入口链：

```text
Windows loader
  -> _DllMainStartup / _DllMainStartupDebug
  -> _DllMainStartupImpl
  -> _DllMainCRTStartup
  -> core/dll/dllentry.cpp::DllMain
  -> core/common/milcoredllentry.cpp::MILCoreDllMain
```

确认的关键行为：

- DllUtil 在 CRT/用户 `DllMain` 之前创建 WPF process heap，Debug 下初始化 DebugLib；
- `MILCoreDllMain` attach 顺序仍为 AV 锁、composition/graphics locks、RenderOptions、core Startup、SW、HW、ETW 等；
- shutdown 仍按 HW → SW → core → AV，再反初始化顶层锁；
- `engine::Startup` 和 `Shutdown` 不是严格镜像逆序；
- 显式 `FreeLibrary` detach 执行完整关闭；
- 进程终止时 `lpreserved != NULL` 且 `g_fAlwaysDetach == FALSE`，DllUtil 直接返回，跳过完整用户/CRT shutdown；
- attach 失败和若干局部初始化没有本地完整回滚。

结论：Native AOT unload/reload 与 attach 等价性继续是 P0；不能把原生命周期合并为一个普通托管 `Dispose`。

## 4. ABI 与托管调用复核

### 4.1 DLL 身份和导出静态基线

直接复核：

- `RefAssemblyAttrs.cs` 当前 `WCP_VERSION_SUFFIX="_cor3"`；
- 当前 PresentationCore `DllImport.MilCore` 静态展开为 `wpfgfx_cor3.dll`；
- `VersionSuffix.props` 将 native `WpfVersionSuffix` 设为 `_cor3`；
- `wpfgfx.def` 原始 `EXPORTS` 段仍为 106 个静态名称，未显式使用 ordinal、alias、forwarder、`DATA`、`PRIVATE` 或 `NONAME`；
- Debug DllUtil 源码仍有 `.def` 外 `g_fNoMeterChecks` 数据导出候选。

### 4.2 独立静态集合复核

对当前 PresentationCore 实际编译输入中的 MilCore `DllImport` 独立复核，结果与既有文档完全一致：

- 10 个源文件；
- 109 个声明出现；
- 107 个唯一 native EntryPoint；
- 重复项只有 `MILAddRef` 和 `MILQueryInterface`，各两个托管重载；
- 与 106 个 `.def` 名称交集为 99；
- 托管独有 8；
- `.def` 独有 7。

未发现需要修改 [`../02-abi-and-managed-callers.md`](../02-abi-and-managed-callers.md) 中数量或完整差异列表的反证。

### 4.3 句柄、所有权和 callback

抽样复核确认：

- `HMIL_OBJECT/HMIL_RESOURCE/HMIL_CHANNEL` 是固定 32 位协议值；
- `MIL_CHANNEL/HMIL_CONNECTION` 是不透明结构指针，按架构为指针宽度；
- `DUCE.ResourceHandle` 在托管侧显式包装 `UInt32`；
- `CMilConnection` 的 handle/pointer 转换是 `reinterpret_cast`；
- channel 的托管关闭顺序仍为 close batch → commit → destroy；
- `SafeMILHandle` 最终调用 `MILRelease`；
- `SafeMediaHandle` 仍为 `MILMediaShutdown` → `MILRelease`；
- `SafeReversePInvokeWrapper` 仍为 blocking release → `MILRelease`；
- `CStreamDescriptor` 是 14 个 callback + pointer-sized handle，native 复制 descriptor 并在包装对象析构时调用 dispose；
- `CEventProxyDescriptor` 是两个 `__stdcall` callback + `DWORD_PTR`；
- D3DImage callback 参数原生明确使用 32 位 `BOOL`，并在 finalizer、换 back buffer 和 AppDomain shutdown 时先 detach；
- geometry callback 原生 typedef 使用 `CALLBACK`。

结论：布局、callback、COM、双运行时 token 和所有权门禁仍必须保留，不能用托管单元测试替代 native caller/差分证据。

### 4.4 首次加载与关闭路径

直接复核 `MediaContext`：

- 构造时先创建 `MediaContextNotificationWindow`；
- 该窗口构造中调用 `MilContent_AttachToHwnd`；
- 随后才调用 `MediaSystem.Startup`，其中执行 `MilVersionCheck`；
- `MediaContext.Dispose` 先释放 composition targets，再 detach/销毁 notification window，随后停止 TimeManager、关闭 channels，最后 `MediaSystem.Shutdown`。

结论：`MilVersionCheck` 不是可靠的第一个入口；任何生产初始化方案都必须支持其它 MilCore 导出首先进入。

## 5. 本地 Silk.NET 复核

直接复核 `D:/lindexi/Code/OriginWpf/Silk.NET`：

- `global.json` 请求 SDK `6.0.100`，`rollForward=major`；
- `build/props/common.props` 的 `VersionPrefix=2.11`，release notes 为 December 2021 Update；
- Microsoft bindings 项目主要目标为 `netstandard2.0/2.1`、`netcoreapp3.1`、`net5.0`；Core 另含 `net6.0`；
- bindings 自动引用 `Silk.NET.Core` 与 SilkTouch analyzer，D3D9/DXGI 另引用 Maths，DXVA 还引用 Win32Extras 和 D3D9；
- 已确认存在 D3D9、DXGI、DXVA、Direct3D.Compilers 项目；
- 未发现 DirectWrite、WIC/WindowsCodecs、Direct2D、Media Foundation、EVR、DirectShow 或 WMP 专用 bindings 项目；
- `Silk.NET.Core.Native.IUnknown`、`IDirect3D9`、`IDirect3D9Ex` 的代表 vtable 调用使用 `delegate* unmanaged[Cdecl]`；
- 默认 `GetApi()` 通过 `DefaultNativeContext`/`UnmanagedLibrary` 加载，Windows fallback loader 使用 `LoadLibrary`、`GetProcAddress`、`FreeLibrary`；
- D3D9 默认名为 `d3d9.dll`，D3DCompiler 默认名为 `D3DCompiler_47.dll`，不能自动保持 WPF 私有 `_cor3` 部署语义。

结论：[`../decisions/DEC-0002-silknet-adoption-boundary.md`](../decisions/DEC-0002-silknet-adoption-boundary.md) 仍成立。Silk.NET 是低层候选来源，不是可直接整体替换 wpfgfx 平台层的黑盒依赖；`WP-00G` 必须按 API 家族裁决 ProjectReference、冻结源码或局部手写互操作。

## 6. 发现的文档漂移

### 6.1 `WP-00A` 与 `WP-00B` 边界

权威主计划、迁移顺序和项目形态已经明确：

- `WP-00A`：创建隔离边界、项目/测试/probe/caller **承载**，可运行 restore、普通 build、托管测试和导入隔离验证；
- `WP-00B`：执行 x64 Debug/Release Native AOT publish、PE/export 检查、动态/静态 native caller 和 `E2E-00`。

但审计时发现：

- 当前旧版 `next-session-handoff.md` 曾允许 `WP-00A` 在环境允许时做 x64 publish/call；
- `investigations/04a-build-isolation.md` 的旧后续动作允许 publish/caller 留在同一包；
- `investigations/03b-file-ledger-and-batches.md` 的批次 0 汇总表仍保留拆包前的“大批次” Done 表述。

处理：当前 handoff 和 `04a` 必须改为严格分包；`03b` 保留历史表，但必须显式标注它是 `WP-00A...WP-00I` 拆分前摘要，不能作为当前工作包 Done 定义。

### 6.2 规划工具偏差记录

本轮在读到章程第 8 节前，误用了两次只读 PowerShell：

1. 枚举 `WpfGfxShape/Docs` 文件；
2. 统计这些文档的行数。

两次操作均未修改文件、未读取/生成产品产物、未构建、未测试，也不作为源码或运行证据。发现禁令后，本轮停止使用终端，后续调查只使用 IDE 文件/项目读取、搜索、符号导航和编辑工具。

## 7. 规划裁决

本轮不改变以下既有决策：

- 最终 `wpfgfx` DLL完整生产闭包是迁移分母；
- `milctrl/wpfx/DbgXHelper` 是旁支证据，不合入同一生产 DLL；
- 一个 .NET 10 生产项目 + 独立测试/harness；
- 15 个实现批次（0–14）；
- 批次 0 拆为 `WP-00A...WP-00I`；
- 三个 P0：Windows x86 Native AOT、unload/reload、AOT-safe COM-like vtable/object；
- 机器 Ledger、ABI manifest、Evidence/E2E 契约均为强制事实源；
- 首个真实生产文件候选仍为 `WP-04A-EXACT-ARITHMETIC`，但只有全部前置满足后才能领取；
- 下一轮唯一工作包仍为 `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`。

## 8. 本轮未验证

本轮没有执行：

- Git clean/dirty 状态验证；
- 原 WPF或新项目 restore/build/publish；
- Native AOT shared-library 产物生成；
- PE export/import/resource/version/PDB 检查；
- x86/x64/ARM64 caller；
- COM、callback、unload/reload、FXJIT 或 Silk.NET 运行 spike；
- 原 DLL真实二进制基线；
- 机器可读 Ledger；
- 单元、差分、集成或 E2E 测试。

上述项目继续保持 `NotRun`/`Blocked`，不能因本轮静态复核而提升状态。

## 9. 对下一轮的唯一结论

下一轮只执行 `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`。第一动作是先检查 Git/工作区状态并归档当前 handoff，然后创建成对的局部 `Directory.Build.props/targets`，证明 WPF Arcade/Testing 导入被截断。该轮不得执行 Native AOT publish、PE/export 检查或 native call；这些以 `NotRun-ByWorkPackageScope` 明确交给 `WP-00B`。
