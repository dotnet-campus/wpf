# wpfgfx 逐文件迁移顺序与工作包规范

> 状态：静态规划基线已收口；15 批次、Ledger、横切门禁和首个真实实现候选已锁定，后续按构建、二进制和运行证据增量维护  
> 适用范围：`src/Microsoft.DotNet.Wpf/src/WpfGfx`、其直接构建/生成依赖、wpfgfx ABI 实现和必要托管调用边界  
> 新生产代码根目录：`WpfGfxShape/Code`  
> 上位约束：`WpfGfxShape/Docs/00-migration-charter.md`  
> 事实基线：`01-native-source-topology.md`、`02-abi-and-managed-callers.md`  
> 调查底稿：`investigations/03a-generated-model-and-protocol.md`、`investigations/03b-file-ledger-and-batches.md`

## 0. 本文如何使用

本文是后续逐文件实施的权威排序入口。每轮实现会话必须：

1. 先阅读 `00-migration-charter.md` 和 `next-session-handoff.md`；
2. 再阅读本文中当前批次、工作包和门禁；
3. 只领取交接文件指定的一个主工作包；
4. 编码前建立或更新目标文件的迁移台账；
5. 编码后运行该工作包规定的验证；
6. 结束前更新台账、专题文档和 `next-session-handoff.md`。

本文的批次是实施和验证顺序，不是新的架构分层。原生源码中的循环、模块职责和文件边界必须保持可追溯，不得为了符合批次顺序而重构源码关系。

## 1. 不可协商的最高约束

### 1.1 逐原生文件近似直译

初次迁移必须遵守：

- 原生实现文件原则上对应一个主要 C# 实现文件；
- 保留原类型名、函数名、字段名、常量名、分支结构和调用顺序；
- 保留错误检查、失败返回、锁顺序、引用计数和资源释放顺序；
- 保留条件编译分支、看似不可达的兼容逻辑和现有异常行为；
- 一个变更只处理一个可说明的迁移单元，不顺带清理附近代码；
- C# 无法逐字表达时，使用最接近的低层表达，并记录偏差、风险和验证方法。

### 1.2 等价完成前禁止重构

在既定等价门槛达到前，禁止：

- 合并或拆分原模块职责；
- 引入新的高层渲染架构；
- 为“更符合 C# 习惯”改变算法或控制流；
- 改变缓存、并发、锁、线程、浮点、SIMD 或资源策略；
- 用高层第三方库替换原底层语义；
- 删除无当前托管调用方的导出或条件逻辑；
- 用虚假成功、空实现或吞错跨过尚未迁移的行为；
- 把重构、优化或无关清理混入翻译提交。

### 1.3 正确性和 ABI 优先级

决策顺序固定为：

1. 可验证的行为等价；
2. C ABI、布局、调用约定和生命周期兼容；
3. 逐文件可审计和可回退；
4. 可构建、可测试和可跨会话继续；
5. 迁移速度；
6. 代码简洁、现代化、抽象质量和性能优化。

## 2. 调查顺序与实现顺序必须分开

### 2.1 调查批次

调查按证据获取效率组织，可围绕：

- 一个原生项目及其 PCH；
- 一个 ABI 导出家族；
- 一个生成协议族；
- 一条初始化/关闭链；
- 一个源码循环组；
- 一个纯逻辑算法族。

调查完成只表示文件可进入 `Investigated`，不表示它应立即实现。

### 2.2 实现批次

实现按“能够承载依赖并形成最小验证闭环”组织：

- 先建立项目、ABI、类型、布局和测试承载；
- 再迁移公共运行基础和纯逻辑叶子；
- 对循环域先建立原名类型/签名骨架；
- 随后按原文件逐一填充实现；
- 最后收口完整导出、二进制替换和真实 PresentationCore 流程。

高耦合文件可以很早被完整调查，但必须等其真实前置满足后再翻译。

## 3. 源码到 C# 的路径映射

### 3.1 基本规则

原生路径：

`src/Microsoft.DotNet.Wpf/src/WpfGfx/<relative-dir>/<name>.<cpp|cxx|c>`

原则上映射到：

`WpfGfxShape/Code/<relative-dir>/<name>.cs`

必须保留：

- `common`、`core`、`shared` 三大根边界；
- `core/fxjit/{Platform,Compiler,Collector,PixelShader}`；
- `core/control/util`；
- `core/sw/swlib`；
- `shared/debug/DebugLib`；
- `shared/util/{DllUtil,UtilLib}`；
- 原 `Generated`、`processed`、资源和生成目录语义。

### 3.2 头文件与生成文件

- 头文件不强制一头一 C# 文件，但所有类型、常量和布局必须有完整映射记录；
- 若同名文件产生 C# 命名冲突，使用保留原扩展语义的稳定后缀，并记录例外；
- 自动生成输出不得与手写实现混在一个不可追溯文件中；
- WpfGfx 树外直接编译文件保留真实来源边界；
- 不得复制外部源码后把它描述为 WpfGfx 自有文件。

### 3.3 单项目不等于扁平化

最终生产代码位于一个 .NET 项目中，仅表示构建和发布边界统一。它不允许：

- 扁平化目录；
- 合并原静态库职责；
- 消除原源码回边；
- 建立与原实现无法并排审计的新架构。

## 4. 完整性与逐文件台账

### 4.1 必须并列使用的完整性视图

| 视图 | 主要来源 | 用途 | 不能单独证明 |
|---|---|---|---|
| 解决方案项目视图 | IDE 项目枚举 | 当前已加载项目 | 未加载、条件和运输包项目 |
| 项目项视图 | `.vcxproj`/IDE 文件清单 | 编译、资源、引用和局部生成项 | 未列入项目的头与公共导入 |
| 磁盘文件视图 | 文件搜索 | 头、内联、模板、旁支和签入生成物 | 当前配置是否参与构建 |
| include/PCH 视图 | PCH、直接 include、公共 include 路径 | 翻译单元真实声明依赖 | 链接成员与运行可达性 |
| 构建导入视图 | `.props`、`.targets`、目录配置 | 宏、生成、资源和条件项 | 最终展开值 |
| ABI 视图 | `.def`、prototype、定义、P/Invoke | 导出和调用方映射 | 实际二进制行为 |
| 生成协议视图 | 模型、generator、输出、消费者 | 防止协议/资源遗漏 | 当前环境可重现性 |

### 4.2 台账必填字段

每个文件或构建/生成项至少记录：

| 字段 | 要求 |
|---|---|
| Ledger ID | 稳定 ID；重命名时保留旧 ID |
| Native project | 原 `.vcxproj` 或公共/外部/生成供应者 |
| Native path | 仓库根相对路径，保留实际大小写 |
| File kind | Implementation/Header/PCH/Inline/GeneratedInput/GeneratedOutput/Resource/Build/ABI/TestEvidence/External |
| Build participation | 编译、include、资源、生成、链接、条件、旁支或未确认 |
| Original responsibility | 原职责，不写拟议重构后的职责 |
| Key symbols | 类型、函数、全局、导出、资源 ID 或模型节点 |
| Type family ID | 将手写头、手写实现、集中生成实现、命令和 factory case 连接到同一逻辑类型族；不能替代物理文件 Ledger ID |
| Symbol implementation locations | 记录同一类型的方法分别位于哪些手写/生成文件；集中生成文件必须可逐方法回溯 |
| PCH/direct includes | PCH 与显式/公共 include 来源 |
| Intra-project dependencies | 同项目直接依赖 |
| Cross-project dependencies | 跨目录/项目依赖及方向 |
| Platform/external dependencies | Win32、COM、D3D、WIC、DWrite、AV、GDI、CRT、运输包等 |
| Generated relation | 输入、generator、输出、消费者和签入状态 |
| Generated group ID | 生成族稳定 ID；组状态不替代文件状态 |
| Source authority | 当前消费基线、模型、generator、冻结输出、副本或中间产物 |
| ABI/layout sensitivity | 导出、调用约定、pack/align、bool、handle、COM、callback |
| Lifecycle/concurrency sensitivity | attach、锁、线程、refcount、释放和失败路径 |
| Cycle group | 已知循环组 ID |
| Investigation batch | 调查工作包 |
| Implementation batch | 实现批次/子批次 |
| Proposed C# path | 锁定后的镜像路径 |
| Scaffold allowance | 允许的声明/布局/薄边界，不得含假业务实现 |
| Required co-migration | 必须同步处理的头、输出或配对释放文件 |
| Status | 章程状态机 |
| Readiness blockers | 未满足的前置 |
| Verification evidence | 布局、ABI、单元、差分、集成和 E2E 链接 |
| Known deviations | 无法逐字表达处及验证方式 |
| Owner/session | 当前会话/工作包 |
| Last reviewed | 最近复核轮次 |
| Notes/decision links | 决策、例外、风险和交接链接 |

### 4.3 项目完成哨兵

每个 `.vcxproj` 建立一个 Build 类项目哨兵。只有项目内以下类别都已“已查有项”或“已查未见”，哨兵才可视为调查完整：

- 实现；
- 头、PCH 和内联；
- 生成；
- 资源；
- 构建；
- ABI；
- 外部供应；
- 测试证据。

项目哨兵不能代替逐文件状态，也不能因全部 `.cpp` 已翻译就标记 `Accepted`。

### 4.4 Ledger 的四个并列层级

为同时满足“逐物理文件审计”和“跨生成/ABI/类型族闭环”，必须并列维护四种记录；任何组级状态都不能覆盖文件状态：

| 层级 | 最小单位 | 主要用途 | 强制规则 |
|---|---|---|---|
| File Ledger | 一个原生/生成/构建/资源文件 | 保持原路径到 C# 路径的一一追溯 | 每个物理文件独立状态、依赖、偏差和证据 |
| Project sentinel | 一个 `.vcxproj` 或外部供应槽位 | 证明实现、头/PCH、生成、资源、构建、ABI、外部供应和测试均已检查 | 只表示项目调查闭环，不代替文件完成状态 |
| Type family | 一个原生类型或资源类型的跨文件实现族 | 连接手写头/实现、`data_generated.h`、`marshal_generated.cpp`、命令和 factory | 必须列出全部实现位置；不得把集中生成实现拆散后丢失原文件映射 |
| Contract/group Ledger | 一个 ABI 家族、生成族、循环组或生命周期链 | 管理跨文件共同编号、布局、路由、初始化和二进制门禁 | 组状态只汇总共同门禁；成员文件仍按章程状态机推进 |

首批必须建立并保持稳定的组 ID：

- `ABI-WPFGFX-EXPORTS`：106 个 `.def` 名称、107 个托管唯一 EntryPoint、99 个交集和 8/7 双向差异；
- `GENPAIR-CORE-TYPES`、`GENPAIR-AV-TYPES`：当前原生/托管实际消费类型副本及历史生成关系；
- `GENPROTO-MILCMD`：命令 ID、结构、payload、发送者、router 和消费者；
- `GENPROTO-DUCE-RESOURCES`：资源 ID、data、marshal、factory、resource update 和消费者；
- `LIFE-WPFGFX-DLL`：DllUtil/CRT 外壳、MilCore attach/shutdown、显式卸载和进程终止；
- `CYCLE-RES-UCE`、`CYCLE-SW-HW-AV`、`CYCLE-GLYPH-BACKENDS`、`CYCLE-TARGETS-RESOURCES`、`CYCLE-API-ALL`。

本轮未使用机器枚举，因此本文定义的是权威 Ledger 结构和强制锚点，不声称已经得到全部 WpfGfx 文件的机器级闭包。后续自动清单只能补齐成员，不能缩减字段或覆盖已记录的来源关系。

## 5. 状态机、readiness 与 done

### 5.1 文件状态机

严格使用：

`NotInvestigated -> Investigated -> Scaffolded -> Translated -> UnitVerified -> DifferentialVerified -> Integrated -> EndToEndVerified -> Accepted`

`Blocked` 可在尚未完成阶段出现。解除阻塞后只能回到已有证据支持的状态，不得跳级。

关键含义：

- `Investigated`：职责、依赖、敏感点、C# 路径、测试点和未决项完整；
- `Scaffolded`：仅有承载循环或 ABI 所需的类型、签名、常量、布局和薄入口；
- `Translated`：完成近似直译，但验证不足；
- `Accepted`：该文件要求的全部验证通过，所有偏差有记录。

### 5.2 批次 readiness

批次开始前必须全部满足：

1. 目标文件至少 `Investigated`；
2. 原路径与 C# 路径锁定；
3. 直接依赖已实现、允许骨架或显式阻塞；
4. ABI、布局、枚举、handle 和调用约定有验证方案；
5. 生成输入/输出和消费者已同批或前置登记；
6. 初始化、关闭、锁、引用和失败顺序已记录；
7. 编码前已定义最小测试/差分证据；
8. 工作包原文列出本批禁止的重构。

### 5.3 批次 done

批次完成前必须全部满足：

1. 所有目标文件达到该批规定状态；
2. 原文件、C# 文件、同步头/生成输出和测试映射完整；
3. 无未登记的新依赖、布局偏差或异常/生命周期差异；
4. 规定的验证已通过，未运行项仍明确标为未验证；
5. 评审确认没有重构、优化或范围外清理；
6. `next-session-handoff.md` 已更新。

## 6. 15 个实现批次总览

| 批次 | 名称 | 核心范围 | 排序性质 |
|---:|---|---|---|
| 0 | 构建/ABI 骨架 | 独立项目、目录镜像、最小导出和测试承载 | 前置骨架，不翻译渲染算法 |
| 1 | 基础类型和错误码 | 标量、HRESULT/WGX、枚举、handle、结构布局 | 全栈前置 |
| 2 | shared util / DLL 生命周期 | UtilLib、DebugLib、DllUtil、进程堆/入口语义 | attach 前置 |
| 3 | common/shared / DynamicCall | COM/refcount、内存、锁、CPU、像素基础、动态调用 | 公共运行基础 |
| 4 | geometry / scanop / effects | 几何、像素管线和效果列表 | 首批可差分算法 |
| 5 | fxjit | Platform、Compiler、Collector、PixelShader | 软件 ShaderEffect 前置 |
| 6 | core/common / control / targets / glyph | 数学、loader、控制、RT 基类、glyph | 上层渲染承载 |
| 7 | generated protocol | processed types、命令、资源类型、布局和生成映射 | resources/UCE 强前置 |
| 8 | resources / UCE | 资源、channel、composition、partition 和 generated dispatch | 核心循环域 |
| 9 | SW | 软件光栅、surface、目标和 GDI present | 首个可控后端 |
| 10 | HW | D3D9 设备、资源、管线、shader、回退 | SW/资源前置 |
| 11 | AV | WMP/EVR/DXVA、状态线程、surface 和回调 | 多域交叉后置 |
| 12 | meta / API | 后端组合、factory、WIC、外部对象桥接 | 依赖主要实现域 |
| 13 | DLL / export | 完整入口、资源、导出和二进制契约 | 最终收口 |
| 14 | 端到端整合 | 真实 PresentationCore、差分、失败与生命周期 | 最终验收带 |

批次 4、5、8、9、10、11 可拆为连续子批，例如 `8A`、`8B`。拆分只缩小工作包，不改变前置关系。

### 6.1 逐批 readiness / done 判定表

下表是第 5 节通用门禁的逐批具体化；它只增加约束，不降低第 7 节的详细证据要求。某批拆为子批时，每个子批先满足自己的同类条件，整批只有在全部目标 Ledger 项满足本行 done 后才可关闭。

| 批次 | Ready only when | Done only when |
|---:|---|---|
| 0 构建/ABI 骨架 | 章程、拓扑、ABI 静态基线和“不修改 WPF 源码/项目”边界已锁定；目标产物名、架构和唯一最小导出已定义 | 按 `WP-00A...WP-00I` 分包取得各自证据；`WP-00A` 只证明隔离项目/测试/probe/caller 承载，`WP-00B` 才证明 x64 Native AOT publish/export/native call；其它架构、linker/resource、COM、callback/unload、原二进制和 Ledger 各自保持独立门禁，未创建假业务导出 |
| 1 基础类型和错误码 | 批 0 测试承载可用；当前原生/托管实际消费副本已冻结并分别登记 | 标量、错误码、enum、三类 handle、固定 `UInt64`、descriptor/packet/基础 command 布局在目标架构达到规定布局/单元证据；不确定项为 `Blocked` |
| 2 shared util / DLL 生命周期 | 批 1 的 Win32/ABI 基础可表达；attach、显式卸载和进程终止探针已设计 | 目标 UtilLib/DebugLib/DllUtil 文件逐文件迁移并验证；Native AOT 无法逐字表达的 CRT/DllMain 差异有书面决策；失败和关闭顺序有证据 |
| 3 common/shared / DynamicCall | 批 1-2 所需内存、错误、锁和生命周期承载可用 | 公共 COM/refcount、内存、CPU/像素基础、缓存和 DelayCall 目标文件达到规定单元/差分状态；每个 owner、锁和动态模块有失败/释放证据 |
| 4 geometry / scanop / effects | 批 1、3 的直接语义依赖已完成；候选文件的 PCH 暴露面与真实语义依赖已分别登记；原生差分 harness 已定义 | 本批全部已领取叶子文件达到 `DifferentialVerified`；非叶子文件只在其直接依赖满足后推进；浮点、像素和 COM 语义无未登记偏差 |
| 5 fxjit | 批 1、3 和所需像素类型可用；可执行内存/W^X/CFG/架构探针已定义 | Platform→Compiler/Collector→PixelShader 规定闭环有差分、释放和失败证据；若原 JIT 不可表达，已有用户批准的局部例外，而非静默替换 |
| 6 core/common / control / targets / glyph | 批 1-5 中直接前置已满足；OSVersionHelper、`dwriteloader.cpp` 和循环骨架来源已登记 | 上层所需数学、loader、RenderOptions、control、target 和 glyph 行为可用；新增初始化参与者已进入生命周期探针；回边未被新接口消除 |
| 7 generated protocol | 批 0-1 的测试/布局承载可用；当前消费输出已冻结；模型、emitter、原生/托管输出和消费者均已建账 | command/resource ID、布局、payload、固定宽度、SDK fingerprint、router/factory/`ProcessUpdate` 签名和代表字节流/失败码通过；generator 漂移已解释或保持阻塞 |
| 8 resources / UCE | 批 1-7 完成；`CYCLE-RES-UCE` 最小原名骨架、handler/factory 映射和无假成功规则已评审 | 各连续子批完成；至少 connection→channel→resource→command→message 链 `Integrated`；资源更新、malformed packet、线程和部分失败顺序有差分证据 |
| 9 SW | 批 4-8 完成；fxjit 路径按需可用；`bilinearspan` 的本地/运输供应方案有书面事实 | 最小 UCE/RenderTargetBitmap 纯软件路径产生一致像素；dirty rect、stride、DPI、格式、GDI present 和释放证据通过 |
| 10 HW | 批 3-9 完成；D3D9/D3D9Ex 低层互操作、COM 布局和 shader 资源策略已验证 | 最小硬件 bitmap/HWND 路径集成；create/reset/device-lost、资源引用、shader、SW fallback 和 D3DImage detach 有差分证据 |
| 11 AV | 批 1-3、6-10 完成；descriptor、AV packet、COM apartment/线程和 HW/SW surface 接口可用 | open/stop/close/shutdown/process-exit、event packet、callback/shutdown reentry 和至少受控媒体帧路径 `Integrated` |
| 12 meta / API | 批 4、6、8-11 的真实实现可用；每个拟启用导出已映射到实现/所有权/失败语义 | meta 后端选择、factory/COM、WIC/codec、stream/media/bitmap wrapper 通过差分；导出仍为薄转发且 getter/create 所有权一致 |
| 13 DLL / export | 批 2-12 完成；每个 ABI 名称都有实现状态、签名、架构、所有权和测试映射；8/7 差异仍显式 | 实际二进制导出、文件名、资源、x86 stdcall/未装饰名、x64/ARM64 ABI、完整 attach/detach 和全 ABI smoke/differential 通过 |
| 14 端到端整合 | 批 0-13 的候选 DLL 已达到可替换状态；部署切换和原生基线捕获方法固定 | 规定架构/配置上的 PresentationCore 软件、HWND、D3DImage、硬件、媒体、多 Dispatcher、失败与退出矩阵通过；文件和偏差台账达到 `Accepted` 门槛 |

## 7. 各批次可执行说明

### 批次 0：构建与 ABI 骨架

**范围**

- `WpfGfxShape/Code` 下独立 .NET 10 Native AOT 生产项目；
- 独立测试项目；
- 目录镜像；
- 当前 DLL 名、架构和导出清单的测试数据；
- 一个受控最小 C 导出闭环。

**允许**

- 项目文件、构建隔离配置、最小导出入口、C caller/测试承载；
- 用明确测试入口验证异常封锁和 HRESULT 返回。

**禁止**

- 批量创建 106 个返回 `S_OK` 或 `E_NOTIMPL` 的假实现；
- 翻译任何渲染算法；
- 修改原 WPF 项目；
- 建立新的大一统业务层。

**最小证据**

- 独立构建不受 WPF 根 props/targets 干扰；
- 产物文件名、架构和导出名可验证；
- 外部 C caller 可调用；
- 导出内部异常不能越过 ABI。

### 批次 1：基础类型、错误码与布局

**范围**

- `wgx_core_types`、`wgx_av_types`、基础 generated enum/struct；
- HRESULT/WGX 错误；
- `BOOL` 与 C++ `bool`；
- GUID、RECT、固定宽度整数；
- 32 位 DUCE handle、指针宽度对象 handle、COM 指针和固定 `UInt64` 协议槽；
- stream/event descriptor 和 AV packet 基础布局。

**最小证据**

- x86/x64/ARM64 `sizeof/offsetof` 对照；
- enum/error 常量；
- `MilVersionCheck` 正确/错误版本；
- descriptor callback 约定和保活。

**禁止**

- 统一所有 handle；
- 把固定 64 位协议字段改为 `IntPtr`；
- 未经验证统一 P/Invoke 属性。

### 批次 2：shared util 与 DLL 生命周期

**范围**

- `shared/util/UtilLib`；
- `shared/debug/DebugLib`；
- `shared/util/DllUtil`；
- 进程堆、自定义入口、显式卸载和进程终止差异。

**最小证据**

- attach/detach/显式卸载/进程终止事件序列；
- 分配失败；
- DebugLib 缺失回退；
- shutdown 顺序。

**禁止**

- 用一个 `Dispose` 合并所有 loader/CRT/进程语义；
- 擅自补写原生没有的失败回滚。

### 批次 3：common/shared 与 DynamicCall

**范围**

- COM/refcount；
- 内存、数组、region、像素基础、CPU feature、缓存；
- `DelayCall` 动态加载和函数指针缓存。

**最小证据**

- AddRef/Release/QI；
- 缓存、像素格式和 region 单元/差分；
- 动态模块缺失、函数缺失和失败映射；
- 锁与释放顺序。

**禁止**

- 用 GC 替代 refcount；
- 改变缓存、CPU/SIMD 或锁策略。

### 批次 4：geometry、scanop 与 effects

**候选与当前裁决**

- 已锁定首个真实实现包：`core/geometry/ExactArithmetic.cpp`，详见第 14.2 节；
- `core/geometry/BezierFlattener.cpp`：保留为后续候选，先满足 sink 回调、HRESULT/WGX、abort 和浮点容差前置；
- `core/geometry/LineSegmentIntersection.cpp`：因 Exact/Interval arithmetic、超大文件和复杂排序状态后置；
- `common/shared/pixelformatutils.cpp`：因 WIC/D3D/生成像素类型/FPU/HRESULT 跨面后置；
- `common/scanop/soconvert.cpp`、`soblend.cpp`：待 SIMD、像素布局和调用链闭环后再裁决；
- `common/effects/effectlist.cpp`：待 COM 资源 AddRef/Release 和消费者闭环后再裁决。

**最小证据**

- 边界值和随机差分；
- NaN、Infinity、溢出和退化几何；
- 像素逐字节对比；
- effect list 的 COM 引用语义。

**禁止**

- 改变浮点精度、容差、舍入、颜色空间和 SIMD 分支；
- 为测试方便替换算法。

### 批次 5：FXJIT

**范围**

- Platform、Compiler、Collector、PixelShader；
- operator/IR、寄存器映射、机器码、代码缓冲和全局 jitter lock。

**最小证据**

- shader token 解析；
- IR/operator 图；
- 原生与新实现生成结果/行为差分；
- 可执行内存、W^X、CFG 和架构行为；
- 释放与失败注入。

若 Native AOT 无法保持原 JIT 语义，必须先形成局部书面例外并由用户确认。不得静默换成另一算法或高层编译器。

### 批次 6：core/common、control、targets 与 glyph

**范围**

- 核心数学、矩阵、loader、display、DWrite/DWM、RenderOptions；
- control/util；
- render target 基类；
- glyph 基础；
- WpfGfx 外 `dwriteloader.cpp` 和 OSVersionHelper 来源边界。

**最小证据**

- 数学差分；
- loader 惰性加载、缺失模块和错误映射；
- RenderOptions BOOL/锁；
- target 层栈；
- glyph 数据与 outline 局部测试。

不得通过新接口消除 targets/resources 或 glyph/backends 回边。

### 批次 7：生成协议

**范围**

- `GENPAIR-CORE-TYPES`；
- `GENPAIR-AV-TYPES`；
- command/resource ID；
- command 显式布局和 payload；
- `generated_process_message.inl`、resource factory、marshal/render-data 的签名承载；
- SDK fingerprint。

**策略**

1. 冻结当前实际消费输出；
2. 镜像输出并近似直译；
3. 仅在后续约束明确允许执行生成器时，才在隔离目录重新运行 generator 并差分；
4. 只在等价完成后决定长期生成方式。

**最小证据**

- 当前正式命令静态范围 `0x01..0x8D`；Debug 尾项 `0x8E`；
- resource type `1..97`，`TYPE_LAST=98`；
- 每个结构 size/offset、payload 起点和字节流；
- `MIL_SDK_VERSION=0x200184C0`；
- processed/Common 副本差分。

禁止重新生成后直接覆盖冻结基线。

### 批次 8：resources 与 UCE

这是首个超大循环域，必须拆成连续工作包，至少包括：

- `8A`：类型/签名/布局循环骨架；
- `8B`：handle table 与 connection/channel；
- `8C`：command batch 与资源生命周期；
- `8D`：resource update 与 generated dispatch；
- `8E`：composition/partition/thread/scheduler；
- `8F`：UCE ABI 家族与最小集成链。

**最小证据**

- connection/channel create/close/disconnect；
- resource create/duplicate/refcount/release；
- begin/append/end/commit；
- malformed packet；
- same-thread/cross-thread；
- partition zombie；
- generated resource update；
- 最小 command/message 闭环。

禁止用 actor、消息总线或新并发模型重写。

### 批次 9：软件渲染

**范围**

- rasterizer、coverage、scan pipeline；
- software surface/HWND/bitmap target；
- double-buffered bitmap；
- GDI present；
- `swinit.cpp`；
- `bilinearspan` 外部供应边界。

**最小证据**

- 确定性像素 golden/differential；
- dirty rect、stride、DPI、pixel format；
- GDI present；
- 软件强制模式；
- 资源释放。

**退出条件**

最小 UCE/RenderTargetBitmap 纯软件路径能够产生与原生一致的可观察像素。

不得使用 System.Drawing、Skia 或其它高层光栅器替换原算法。

### 批次 10：硬件渲染

**范围**

- D3D9/D3D9Ex device、resource、surface、texture、swap chain；
- HW pipeline、shader 资源、glyph；
- software fallback；
- InteropDeviceBitmap；
- `hwinit.cpp`。

**最小证据**

- adapter/caps/device create；
- reset/device lost；
- COM 引用和释放；
- shader 资源 ID/内容；
- HW/SW fallback；
- D3DImage callback/detach。

不得迁移到 D3D11/12，也不得让 Silk.NET 高层封装改变 COM 生命周期。

### 批次 11：AV

**范围**

- AV loader、COM apartment/event/state thread；
- WMP 状态机；
- EVR/DXVA；
- media buffer/surface；
- event proxy 和 process-exit 回调。

**最小证据**

- open/stop/close/shutdown/process-exit 三类结束语义；
- event packet；
- callback thread 和 shutdown reentry；
- position/progress 超时缓存；
- C++ `bool`/托管 marshalling；
- 受控媒体帧路径。

不得用高层 MediaPlayer 或新 Media Foundation 状态机重写。

### 批次 12：meta 与 API

**范围**

- meta desktop/HWND/bitmap/dummy targets；
- factory、WIC/codec、stream、media 和 bitmap wrappers；
- `core/api/exports.cpp` 真实实现对端。

**最小证据**

- factory 和 COM refcount；
- WIC bitmap/codec；
- stream descriptor；
- SW/HW target selection；
- meta iterator 和释放。

导出函数仍只能是薄边界，不得承载新业务架构。

### 批次 13：DLL 与完整导出收口

**范围**

- `engine.cpp`、`milcoredllentry.cpp`、`dllentry.cpp`；
- `wpfgfx.def`；
- `milcore.rc` 和 HW 资源；
- 106 个 `.def` 名称、107 个托管唯一 EntryPoint、99 交集和 8/7 差异；
- x86/x64/ARM64 二进制契约。

**最小证据**

- 实际导出表；
- x86 stdcall 和未装饰名；
- x64/ARM64 ABI；
- attach、显式卸载、进程终止；
- 异常封锁；
- 资源和 DLL 文件名；
- 全 ABI smoke/differential。

不得通过删除导出或修改托管 P/Invoke 制造清单一致。

### 批次 14：端到端整合

首个强制端到端路径：

1. 真实 PresentationCore 创建 `DrawingVisual`；
2. 绘制确定性图形；
3. 使用 `RenderTargetBitmap.Render`；
4. 读取像素；
5. 与原生 wpfgfx 基线差分。

随后逐步加入：

- HWND present；
- D3DImage；
- hardware path；
- media；
- 多 Dispatcher/connection；
- 失败、设备丢失、显式关闭和进程退出。

不得以一个 happy path 通过就宣称整体迁移完成。

## 8. 循环依赖处理

已知循环组：

- `CYCLE-RES-UCE`：resources 与 UCE；
- `CYCLE-SW-HW-AV`：软件、硬件和媒体；
- `CYCLE-GLYPH-BACKENDS`：glyph 与 SW/HW/resources；
- `CYCLE-TARGETS-RESOURCES`：target 基类与 brush/resource；
- `CYCLE-API-ALL`：API 聚合创建逻辑可见主要模块。

处理规则：

1. 在镜像目录建立原名类型、字段、签名、枚举和函数指针；
2. 骨架只承载编译和布局，不包含假成功实现；
3. 按原文件逐一填充方法体；
4. 每次填充更新直接依赖和未解析依赖；
5. 允许一个循环骨架工作包创建多个直接依赖文件，但每个文件仍有独立 Ledger 行；
6. 不因单项目可互相引用就省略原 include/PCH 关系；
7. 不允许以新接口、DI、事件总线或职责合并消除循环。

## 9. ABI、初始化与生成协议横切门禁

### 9.1 ABI

始终并列维护：

- `.def` 静态名称；
- 原生 prototype/定义；
- 托管 EntryPoint；
- 实际二进制导出。

每个导出记录：

- 真实实现文件；
- C# 镜像路径；
- 调用约定；
- 参数/返回布局；
- 所有权；
- 线程；
- 失败语义；
- 测试证据。

`ABI-WPFGFX-EXPORTS` 的硬门禁如下：

1. **DLL 身份门。** 当前托管常量按静态基线为 `wpfgfx_cor3.dll`；目标名、部署名、架构和真实加载路径必须由产物/运行证据确认，历史 `WpfGfx_v0400.dll` 不能混入当前契约。
2. **四清单门。** 106 个 `.def` 名称、原生 prototype/定义、107 个托管唯一 EntryPoint 和实际二进制导出始终并列；99 个交集和 8/7 双向差异在真实证据裁决前保持未决。
3. **架构门。** x86 逐入口验证 stdcall、参数字节数、栈清理和未装饰公开名；x64/ARM64 分别验证平台 ABI、结构传递、返回值和名称。一个架构通过不能替代另一个架构。
4. **类型/布局门。** `bool`、`BOOL`、HRESULT、enum、GUID、RECT、`size_t`、union、固定/尾随数组、descriptor、AV packet 和 generated command 均用已验证表示；不得按 C# 默认 marshalling 猜测。
5. **身份门。** 32 位 DUCE resource/channel ID、指针宽度 connection/channel handle、COM/native pointer、Win32 handle 和固定 64 位协议槽分别建模，禁止隐式互换。
6. **所有权门。** AddRef/Release/QI、create/getter、SafeHandle、channel close→commit→destroy、media close/shutdown/process-exit、glyph data 配对释放和借用资源地址逐项保留。
7. **回调门。** 调用约定、线程、delegate/function pointer 保活、GCHandle、detach、阻塞释放和 callback 异常映射有证据；异常不得穿越 ABI 或 reverse P/Invoke。
8. **薄边界门。** 导出层只做 ABI 转换、必要校验、异常封锁和向原文件等价实现转发；真实实现未迁移时只能显式阻塞，不得放业务算法或返回假成功。
9. **生命周期门。** 首次任意 P/Invoke 可能触发 attach，且 `MilContent_AttachToHwnd` 可早于 `MilVersionCheck`；显式卸载与进程终止必须保持不同路径。
10. **替换门。** 在实际导出表、布局、失败码、线程、释放、attach/detach 和至少规定端到端链通过前，不得把候选 DLL 描述为二进制兼容替代品。

### 9.2 初始化和关闭

初始化不是批次 13 才开始：

- 批次 0-3 建立 attach/detach/退出测试承载；
- 批次 5-11 每加入一个初始化参与者，就补真实 startup/shutdown；
- 批次 13 只收口完整链路。

必须保留：

- 显式卸载与进程终止差异；
- 命名 Shutdown 与全局析构的双层语义；
- 原始非严格逆序关闭；
- 部分初始化无本地回滚；
- 非致命调用和现有异常点。

`LIFE-WPFGFX-DLL` 必须按参与者持续接入，不得到批次 13 一次性补写：

| 参与者/阶段 | 主要原生证据 | 首次真实实现批次 | 接入时必须新增的证据 |
|---|---|---:|---|
| DllUtil、进程堆、DebugLib、CRT 外壳 | `shared/util/DllUtil/{dllmain,dllmainimpl,data}.cxx`、`shared/util/UtilLib/MemUtils.cxx`、`shared/debug/DebugLib/debuglib.cxx` | 2 | process attach、显式卸载、进程终止短路、DebugLib 缺失回退、堆销毁顺序 |
| `g_DllInstance`、公共内存/CPU/COM 基础 | `common/shared/engine.cpp` 及 common/shared 相关文件 | 3 | attach 状态可见性、CPU 初始化前置、引用/分配失败行为 |
| RenderOptions、core/common loader/display/DWrite 基础 | `core/common/{renderoptions,engine,d3dloader,dwritefactory,display}.cpp` | 6 | 惰性加载、锁、缺失模块、非致命/致命错误区分和对应 shutdown |
| composition/graphics stream、partition/UCE 全局状态 | `core/common/milcoredllentry.cpp`、`core/uce/*` | 8 | 两把全局锁、partition 部分初始化、same/cross-thread 关闭和 zombie 路径 |
| ShaderEffect/fxjit jitter lock 与 `SwStartup` | `core/sw/swlib/swinit.cpp`、`core/resources/ShaderEffect.*`、`core/fxjit/Platform/Platform.cpp` | 5、8、9 分段接入 | 锁创建失败的现有返回异常点、MMX/SSE2 选择、删除顺序；不得静默修复始终返回成功的历史行为 |
| D3D device manager 与 `HwStartup` | `core/hw/{hwinit,d3ddevicemanager}.cpp` | 10 | 只初始化管理锁与惰性设备创建的区别、已加载对象释放和设备丢失 |
| `AvDllInitialize`、CAVLoader、WPP/event proxy | `core/av/{milav,avloader,StateThread,eventproxy}.cpp` | 11 | 两阶段 AV 初始化、首个 proxy 才启动 WPP、process-exit callback 抑制和 shutdown reentry |
| ETW、最终 `MILCoreDllMain`、用户 `DllMain` 和 DLL 资源 | `core/common/milcoredllentry.cpp`、`core/dll/dllentry.cpp`、`milcore.rc` | 13 | 全 attach/关闭次序、返回 `FALSE` 失败路径、资源与实际二进制入口收口 |

任一参与者接入后，`LIFE-WPFGFX-DLL` 的事件序列基线和失败注入矩阵必须同步扩展；只实现 `Startup` 而把对应 shutdown、显式卸载或进程终止差异留到以后，不能满足该工作包 done。

### 9.3 生成协议

生成工作包必须包含：

- 模型或 `.w` 输入；
- generator/emitter；
- 当前原生输出；
- 当前托管输出；
- 至少一个发送方；
- 至少一个消费方；
- 布局/字节流证据。

生成器可重现性未证明前，以当前实际消费输出为冻结基线。

批次 7 及任何消费生成协议的后续工作包还必须通过以下硬门禁：

1. **当前消费者门。** 分别登记原生实际消费的 `include/processed/*.h`、PresentationCore 实际编译的 `Common/Graphics/*.cs` 以及 `Include/Generated`/`core/*_generated*` 文件；重复副本未裁决时不得任选一份代表全部协议。
2. **生成闭环门。** 每个组同时登记模型/XPath 或 `.w` 段、generator/emitter、响应文件纳入状态、原生输出、托管输出、至少一个发送者和至少一个消费者。
3. **编号门。** 冻结正式 `MILCMD 0x01..0x8D`、Debug 尾项 `0x8E`、resource type `1..97` 与 `TYPE_LAST=98`；任何缺项、重排或条件差异均阻塞。
4. **布局门。** 验证 `Pack=1` 的环境来源、每个字段 offset/size、4 字节 record 对齐、`BOOL`/resource handle 宽度、固定 `UInt64` 槽和 payload 起点；不得以平台 `IntPtr` 推断协议字段。
5. **路由门。** `generated_process_message.inl` 的每个 ID 必须映射到 handler、条件跳过或明确未实现阻塞；resource factory 的每个 type 必须映射到原 concrete constructor；禁止默认返回假成功。
6. **失败语义门。** 固定大小错误、截断 payload、错误 resource type、未知 ID、transport denied、render-data push/pop 失衡及特定容错分支必须有原生差分计划，重点保留 `WGXERR_UCE_MALFORMEDPACKET` 路径。
7. **版本门。** `MIL_SDK_VERSION=0x200184C0`、`Version.xml` revision、`ProtocolFingerprint.cs` 纳入漂移和实际 `MilVersionCheck` 行为必须共同建账；路径漂移不能靠手抄常量掩盖。
8. **冻结门。** 生成器重新输出只能写到隔离位置并与冻结基线差分；差异未解释前不得覆盖当前消费文件，也不得把“可重现 generator”设为首次镜像的虚假前置。

出现以下任一情况，相关文件最多到 `Scaffolded` 并标记 `Blocked`：当前实际消费副本不明；ID/offset/pack/宽度/payload 未确认；模型、输出、router、factory、sender 或 consumer 缺一侧；generator 输出与冻结基线不同且未解释；实现试图用通用序列化、重排 XML、统一 `IntPtr` 或假 `S_OK` 跨过缺口。

## 10. 每轮工作包规范

### 10.1 推荐粒度

- 纯逻辑叶子：2-5 个紧密相关实现文件及直接头/测试；
- 普通状态文件：1-3 个实现文件；
- ABI、初始化、线程、COM、生成协议或循环枢纽：1 个主实现文件或 1 个明确骨架包；
- 超大文件：可按原文件内连续职责段分轮，但 Ledger 主项仍是原文件；
- 生成族：共同模型、输出和消费者组成一个工作包。

### 10.2 工作包必备输入

1. 唯一主目标和批次号；
2. 原生主文件、配对头、生成/资源输入和拟议 C# 路径；
3. 直接前置及其状态；
4. 允许创建的骨架；
5. 必须保持的 ABI、布局、锁、引用和失败顺序；
6. 编码前定义的最小验证；
7. 明确范围外事项；
8. “逐文件近似直译、禁止重构/优化/无关清理”的原文提醒。

### 10.3 工作包完成输出

- Ledger 状态变化；
- 原生到 C# 路径映射；
- 新依赖、循环和阻塞；
- 已运行的测试及结果；
- 未运行验证及原因；
- 已知偏差；
- 修改文件列表；
- 下一轮唯一工作包；
- 更新后的 `next-session-handoff.md`。

### 10.4 工作包领取与关闭协议

1. 工作包使用稳定 ID，格式建议为 `WP-<批次或子批>-<原文件/协议族短名>`；同一原文件分段时追加连续段号，但 Ledger 主项不变。
2. 开始时只能有一个唯一主目标。配对头、测试和必要骨架是该目标的从属项，不能借此并入第二个算法域。
3. 领取前在交接文件中写明本包 `Ready` 的逐条证据；任一前置只写“以后补”即视为未 ready。
4. 实施中发现新直接依赖时，先把当前文件标记 `Blocked` 并建账；不得扩大范围后继续伪装为原工作包。
5. 一个提交/评审单元只关闭一个工作包。纯逻辑默认 2-5 个紧密相关实现文件，普通状态 1-3 个，高风险枢纽只允许 1 个主实现文件或 1 个明确骨架包。
6. 关闭时逐条给出 done 证据、Ledger 状态变化和未运行项；“已编译”不等于 `DifferentialVerified`，“测试未写”不等于完成。
7. 本包完成后只指定一个下一工作包；若存在多个候选，按依赖最少、可差分、无 ABI/线程/生成/循环枢纽者优先，并记录未选原因。
8. 每轮都必须重复原约束：逐文件近似直译；禁止重构、优化、职责合并、算法替换和无关清理。

## 11. 评审规则

每个评审单元只对应一个工作包。评审按原文件和 C# 文件并排核对：

- 控制流；
- 错误顺序；
- 锁顺序；
- 引用计数；
- 释放顺序；
- 常量和 enum；
- 条件编译；
- 未实现分支；
- ABI 和布局；
- 测试证据。

以下变化默认要求拆出并拒绝进入翻译提交：

- 重命名；
- 抽象提取；
- helper 合并；
- 缓存变化；
- 并发变化；
- 算法替换；
- 性能优化；
- 无关清理。

## 12. 阻塞与例外管理

### 12.1 必须阻塞的情况

- 直接依赖未台账化；
- ABI、布局或调用约定不确定；
- generator 输入/输出/消费者关系缺失；
- 外部供应源码或二进制不可确认；
- 需要重构才能“解决”循环；
- 无法保持失败、线程或生命周期语义；
- 测试无法区分新旧行为；
- Native AOT 或 Silk.NET 无法表达原语义。

### 12.2 例外流程

1. 记录原文件、具体语义和静态/运行证据；
2. 记录为什么低层直译不可行；
3. 列出最接近的替代方案及行为差异；
4. 定义差分和回退方法；
5. 更新主计划和当前交接；
6. 等待用户明确批准；
7. 例外仅适用于书面范围，不得扩展为通用重构许可。

## 13. 当前尚未验证的事项

规划基线形成时未完成构建、发布或运行测试，因此以下仍未验证；本轮仅以 IDE 项目/文件读取补充了直接项目项下限，未改变这些产物与运行状态：

- 全部 WpfGfx 文件的机器级闭包；
- 所有条件配置和架构的项目项；
- 实际 DLL 导出/导入、资源和静态库成员；
- Native AOT 共享库的真实文件名、导出和卸载行为；
- 所有 native/managed `sizeof/offsetof`；
- MilCodeGen 和 `.w` generator 的重新输出差分；
- `bilinearspan` 实际供应内容；
- FXJIT 可执行内存可行性；
- Silk.NET 各 API 的实际布局和生命周期兼容；
- 任何单元、差分、集成或端到端结果。

这些项目不得在后续文档中描述为“已通过”。

## 14. 执行入口与首个真实实现工作包

### 14.1 立即可领取的唯一工作包：`WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`

在当前仍无生产项目和运行证据的状态下，下一轮唯一可领取范围仍是批次 0：

- 创建成对的局部 `Directory.Build.props`/`Directory.Build.targets`，隔离 WPF 根 Arcade/Testing 导入；
- 创建隔离的 .NET 10 Native AOT 单一生产项目和独立测试项目；
- 建立 `common`/`core`/`shared` 镜像目录，但不创建业务实现；
- 验证不继承 WPF 根 props/targets 的构建隔离；
- 只建立 `WpfGfxShape_NativeAotAbiProbe_v1` 的源文件、内部托管测试和原生 caller 承载；本包可执行 restore、普通 build、托管测试和项目图/导入隔离验证，但**不得**执行或把 Native AOT publish、PE export 检查、`GetProcAddress`/静态 import 调用计入本包完成；这些全部属于 `WP-00B`；
- 建立按 configuration/RID 隔离的输出、中间目录、文件名、架构、导出、HRESULT/异常和后续布局测试承载。

该包不是 wpfgfx 真实生产逻辑翻译。它不得批量创建 106 个导出，不得返回假 `S_OK`/`E_NOTIMPL` 模拟未迁移功能，不得进入批次 1，不得引用/链接编译原 WPF 源码，不得修改现有 WPF/wpfgfx 项目。其唯一目标是把独立边界和 probe/caller **承载**创建正确；下一唯一工作包应为 `WP-00B-NATIVEAOT-ABI-PROBE-X64`。后续架构、linker/resource、COM、callback、Silk.NET 和原 DLL 基线分别由 14.2 的独立工作包领取。

### 14.2 批次 0 后续独立工作包

这些工作包必须逐个领取，不能合并成一次“大脚手架”提交：

| 工作包 | 唯一目标 | Done 门禁 | Do not start / 明确禁止 |
|---|---|---|---|
| `WP-00B-NATIVEAOT-ABI-PROBE-X64` | 完成 x64 Debug/Release 的 Native AOT shared-library probe、动态/静态 native caller 与异常封锁 | 两配置 publish；精确 DLL/PE/export；caller 成功/失败/异常/并发测试；无 AOT/trim/interop warning 压制 | `WP-00A` 未证明隔离；不得加入任何生产导出 |
| `WP-00C-ARCHITECTURE-AND-X86-DECISION` | 验证 ARM64，并以官方/目标 SDK 证据裁决 Windows x86 | ARM64 真实机器矩阵；x86 支持结论、stdcall/未装饰名探针或明确 `Blocked` + 用户裁决 | 不得从普通 `win-x86` RID 推断 AOT；不得静默删除 Win32 |
| `WP-00D-LINKER-RESOURCE-VERSION-SPIKE` | 验证 `.def`/import library/`.res`/VERSIONINFO/ETW/shader/data export 能力 | DLL/LIB/EXP/PDB、resource ID/type/hash、版本字段和 `g_fNoMeterChecks` 可行性有产物证据 | 不迁移 `MILLoadResource` 或生产 shader 逻辑；不依赖未记录 ILC 内部开关进入基线 |
| `WP-00E-COM-VTABLE-SPIKE` | 用一个受控 IUnknown-like 对象验证 AOT 向 native caller 提供 vtable | QI/AddRef/Release、IID、vtable 顺序、GC root、并发和各架构 caller 通过 | 不开始 `MILCreateFactory`、media、bitmap 等生产对象；不把 GC 地址当 COM 指针 |
| `WP-00F-CALLBACK-LIFECYCLE-UNLOAD-SPIKE` | 验证同步/长期 callback、双运行时 token、线程停止、process exit 与受支持卸载语义 | CoreCLR→AOT callback、异常、detach、在途计数、子进程退出及官方支持范围有证据 | 不假定 `FreeLibrary` 受支持；不把 CoreCLR `GCHandle` 交给 AOT 解释 |
| `WP-00G-SILKNET-ADOPTION-SPIKE` | 决定本地 Silk.NET 的可复用形态 | D3D9/D3D9Ex 代表 API 的布局/调用约定/AOT/x64/ARM64/x86 结果；整体引用、冻结源码或局部修正版三者有书面裁决 | 不因存在绑定就宣称覆盖；D3D9 COM Cdecl 风险未解决前不得进入 HW 批次 |
| `WP-00H-ORIGINAL-BINARY-BASELINE` | 冻结原 DLL 的实际二进制与行为基线 | 每架构/配置的 export/import/resource/version/symbol/hash；106/107/99/8/7 与数据导出状态回写 | 没有可追溯原工件时不得做候选 DLL 二进制兼容声明 |
| `WP-00I-MACHINE-READABLE-LEDGER` | 创建完整、可机器校验的文件/生成/ABI/资源台账 | 合并项目项、磁盘、include/PCH、构建导入、ABI、生成和外部供应视图；每项有稳定 ID | 不用 461 个 ClCompile 下限冒充全量分母；未发现项也要记录检查状态 |

批次 0 的工作包可以按独立证据并行调查，但生产迁移的解锁条件不是“全部文档写完”，而是与目标文件直接相关的 P0/P1 门有明确结果。Windows x86、unload/reload 或 COM 若被平台证明不支持，必须暂停完整替换承诺并请求用户裁决，不能自行降级目标。

### 14.3 首个真实源码实现候选：`WP-04A-EXACT-ARITHMETIC`

**裁决：**在批次 0-3 及下列具体前置全部完成后，首个真实 wpfgfx 源码翻译工作包锁定为 `core/geometry/ExactArithmetic.cpp` 的保守单文件包，而不是继续扩大调查或选择表面更小但依赖更宽的文件。

| 项目 | 锁定内容 |
|---|---|
| 批次/子批 | `4A`，geometry 纯逻辑叶子 |
| 主原生实现 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/ExactArithmetic.cpp` |
| 配对声明 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/ExactArithmetic.h` |
| 语义前置头 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/BaseTypes.h` |
| PCH/聚合证据 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/precomp.hpp`、`geometry.h`；只登记 PCH 暴露面，不把整个 geometry 目录并入本包 |
| 拟议 C# 主路径 | `WpfGfxShape/Code/core/geometry/ExactArithmetic.cs` |
| 前置 C# 类型路径 | `WpfGfxShape/Code/core/geometry/BaseTypes.cs`，应在批次 1/本包 ready 前完成并验证 enum/整数范围语义 |
| 主要符号 | `CZBase`、`CZ64`、`CZ128`、`CZ192`，以及原文件局部 `Ea*` digit 算法 |
| 直接消费者 | `core/geometry/LineSegmentIntersection.cpp`；消费者后置，不随本包迁移 |
| ABI/循环/线程 | 无已发现平面导出、COM、回调、线程、锁、生成协议或已知循环组；不能据此宣称无 PCH/公共运行依赖 |

**首包 Ledger 种子（静态状态，不代表已实现）**

| Ledger ID | Native path / role | Kind | Proposed C# mapping | Status | 当前 blocker |
|---|---|---|---|---|---|
| `GEO-BASETYPES-H` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/BaseTypes.h` | Header | `WpfGfxShape/Code/core/geometry/BaseTypes.cs` | `Investigated` | 批次 0-1 测试承载、enum/IEEE 754 整数范围验证 |
| `GEO-EXACTARITH-H` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/ExactArithmetic.h` | Header | 声明映射到 `WpfGfxShape/Code/core/geometry/ExactArithmetic.cs` | `Investigated` | `GEO-BASETYPES-H`、断言/内存原语承载 |
| `GEO-EXACTARITH-CPP` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/ExactArithmetic.cpp` | Implementation | `WpfGfxShape/Code/core/geometry/ExactArithmetic.cs` | `Investigated` | 批次 0-3 done、原生/C# 差分入口 |
| `GEO-PRECOMP-HPP` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/precomp.hpp` | PCH | 无生产 C# 文件；作为依赖映射 Ledger | `Investigated` | `std.h`、`common/common.h`、`geometry.h` 的公共承载面不得被误并入本包 |
| `GEO-EXACTARITH-DIFF` | 原生/C# `CZ64/CZ128/CZ192` 差分证据 | TestEvidence | 批次 0 锁定的独立测试项目路径 | `NotInvestigated` | 测试项目尚未创建，原生适配入口尚未验证 |

**选择理由**

- 算法和状态集中在一个实现文件及其配对头中，核心是固定 32 位 digit 数组、显式 carry/borrow 和 `ulong` 中间乘法；
- 没有 WIC、D3D、COM、callback、loader、resource/UCE、线程或二进制导出边界；
- 可在 `LineSegmentIntersection` 之前独立建立构造、比较、加减乘和溢出语义差分；
- `LineSegmentIntersection.cpp` 本身约 2800 行，直接交叉 Exact/Interval arithmetic 和大量排序/几何状态，不适合作为首包；
- `BezierFlattener.cpp` 含 sink 回调、HRESULT/WGX 错误、abort 和浮点容差；
- `common/shared/pixelformatutils.cpp` 虽含纯函数，但同一文件同时跨 WIC GUID、D3D format、生成像素类型、FPU 舍入和 HRESULT 缓冲检查，若整文件迁移耦合更高；
- 因此不再为寻找“更小文件”无限调查，明确采用该单文件包并记录前置。

**领取前置**

1. `WP-00A` 及批次 0 的测试/差分承载 done；批次 1 的固定宽度标量和错误/enum 基础可用。
2. `SIGNINDICATOR` 必须保持 `-1/0/1`，`COMPARISON` 必须保持 `-1/0/1/255`；`Integer31/33/53` 仍表示受约束、由 IEEE 754 `double` 精确承载的整数，不得擅自改成 `int`/`long`。
3. C# `uint` 模 2^32 的 `unchecked` 加减、`ulong` 中间乘法、最低有效 32 位 digit 在前的顺序和固定容量 `3/5/7` 已有单元承载；不得用 `BigInteger` 替换生产算法。
4. 原 `Assert`、`CopyMemory`、`ZeroMemory`、Debug `MILDebugOutput` 的等价承载或书面偏差已经在批次 2-3 建账；Release 与 Debug 条件行为均有验证方案。
5. 已能对原生 `CZ64/CZ128/CZ192` 建立不修改 WPF 源码的测试适配/差分入口；无法隔离原生基线时本包保持 `Blocked`，不得只与数学公式自证。
6. Ledger 已分别建立 `BaseTypes.h`、`ExactArithmetic.h`、`ExactArithmetic.cpp`、PCH 证据和测试证据项，原路径/C# 路径及消费者关系锁定。

**必须近似直译的语义**

- `EaNumDigits` 至 `EaMultiply` 的 digit 遍历方向、carry/borrow、数组长度前提和截断行为；
- sign 与 magnitude 分离、零值 sign、构造时 `-0.0` 的现有分支结果；
- `CZ64/CZ128/CZ192` 固定 digit 容量和低位在前的存储；
- `Compare` 的 sign-first 与 magnitude 比较顺序；
- `CZ192.Add` 的同号/异号分支和 sign 选择；
- `CZ192.Subtract` 的 self-subtract 分支，以及临时翻转 `other` sign、调用 `Add`、再恢复 sign 的可观察顺序；若 C# 无法逐字表达 `const_cast`，必须记录低层等价表达和异常/中断时恢复验证；
- Debug dump 开关和输出只作为条件行为保留，不得借机删除；
- 原断言前提不是输入规范化许可，生产实现不得自动扩容、饱和或“修复”越界前提。

**最小验证矩阵**

- `CZ64`：零、正负边界、单 digit 最大值、乘法 carry、同/异 sign 比较；
- `CZ128`：`Integer53` 边界、两 digit 初始化、不同 digit count 的乘法和比较；
- `CZ192`：正负/零的 add、subtract、multiply，magnitude 相等/大小互换、同一实例相减、`other` sign 恢复；
- helper：连续 `0xffffffff` carry/borrow、乘 0/1/最大 digit、前导零 digit count、结果截断与固定容量前提；
- 随机与构造边界输入的原生/C# 差分，逐项比较 sign、有效 digit 和全部固定槽位；
- Debug/Release 条件路径分别记录；所有架构应得到相同数值结果，任何 checked/unchecked 差异必须失败而非吞掉。

**工作包 done**

- 主文件完成逐文件近似直译并至少达到 `DifferentialVerified`；
- `BaseTypes.h`/`ExactArithmetic.h`/`ExactArithmetic.cpp` 的 Ledger、路径和测试映射完整；
- 没有使用 `BigInteger`、通用数值抽象、动态扩容或合并算法；
- 没有迁移或修改下列范围外文件；
- 尚未进入真实 geometry/UCE 链时不得把该文件标为 `Integrated`、`EndToEndVerified` 或 `Accepted`。

**明确范围外**

- `LineSegmentIntersection.cpp/.h`、`IntervalArithmetic.h`、`BezierFlattener.cpp/.h`；
- `geometry.h` 聚合的其它类型和整个 `Geometry.vcxproj`；
- scanop/effects、任何 ABI 导出、Native AOT DLL 生命周期和 PresentationCore 调用；
- 重命名类型、抽取通用大整数库、改用 `BigInteger`、优化循环、改变断言或顺带清理。

### 14.4 后续领取顺序

1. 当前先完成 `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`；
2. 完成 `WP-00B`、`WP-00C`、`WP-00H`、`WP-00I`：分别证明 x64 shared-library 机制、形成架构/x86 裁决、冻结原二进制基线、建立机器可读 Ledger；任一未完成都不得开始生产文件编码；
3. 对 `WP-00D` 至 `WP-00G` 按目标工作包的直接依赖执行：linker/resource 在生产资源/完整导出前完成，COM vtable 在 COM/refcount/factory 前完成，callback/unload 在生命周期/长期回调前完成，Silk.NET 在 HW D3D9 互操作前完成；不能因与当前纯逻辑文件无关而假写为已通过，也不能把多个 spike 合并；
4. 依次满足批次 1、2、3 的直接前置和 done；批次 2-3 若触及 unload、COM 或 callback，必须先完成对应 `WP-00E/WP-00F`；
5. 再领取 `WP-04A-EXACT-ARITHMETIC`；
6. 该包完成后，下一候选只能基于更新后的 Ledger 重新在批次 4 叶子中选择，不自动把 `LineSegmentIntersection` 作为下一包。

若 `WP-00C` 证明 x86 不受支持，或其它 P0 门形成平台阻塞，必须先取得用户对目标范围/例外的明确裁决。与当前文件无直接关系的 `WP-00D...WP-00G` 可以后置到其首个消费者之前，但未关闭的 P0 仍阻止“完整替换”承诺。

上述顺序既不允许现在越级翻译 `ExactArithmetic.cpp`，也不允许在前置完成后重新陷入无期限候选调查。
