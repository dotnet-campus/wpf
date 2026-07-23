# WpfGfx 生成模型、命令协议与成对输出调查

> 状态：本轮静态调查基线已完成；生成器可重现性、布局和运行协议仍待后续验证  
> 上位约束：`WpfGfxShape/Docs/00-migration-charter.md`、`WpfGfxShape/Docs/01-native-source-topology.md`、`WpfGfxShape/Docs/02-abi-and-managed-callers.md`  
> 调查范围：WpfGfx/PresentationCore 共享生成模型、生成器、命令协议、原生/托管成对输出、资源工厂和通道表  
> 工具边界：绝对禁止命令行、终端、脚本、构建、测试和运行生成器；只使用 IDE 项目枚举、文件/文本搜索、符号导航、文件读取和本文档写入  
> 实施边界：不修改任何 WPF 源码、项目配置或 `WpfGfxShape/Code`

## 1. 范围与方法

### 1.1 调查目标

本文调查并建立以下静态基线：

- WpfGfx 生成器、生成模型和输入文件，重点包括资源模型、命令模型、类型模型、MilCodeGen、PresentationBuildTasks、T4 或 MSBuild task。
- 同一模型产生的原生/托管成对输出，重点包括 `wgx_commands.cs` 与原生命令头、`wgx_core_types.cs/.h`、`wgx_av_types.cs/.h`。
- `resources_generated.*`、`marshal_generated.cpp`、`renderdata_generated.cpp`、`generated_resource_factory.cpp`、客户端/服务端 channel tables 及其它 `*_generated` 文件。
- 命令编号、枚举值、结构布局、资源类型 ID、命令大小/对齐、字段顺序和工厂 dispatch 是否共享生成源。
- 输出是签入仓库、构建时复制、构建时真正生成，还是历史预生成文件。
- 自动生成标记、禁止直接修改生成代码的规则，以及迁移期应采用的冻结、镜像与近似直译策略。

### 1.2 方法

1. 先读取三份上位文档并继承其正确性、ABI、协议和逐文件近似直译约束。
2. 使用 IDE 工作区文件搜索和 grep 定位生成器、模型输入、构建任务、输出文件与消费者。
3. 读取关键输入、生成器实现、项目/targets 声明、生成输出头部和协议消费点。
4. 以仓库根目录相对路径记录证据；对同源关系尽量给出输入、生成器、原生输出、托管输出和消费方的闭环。
5. 不运行生成器，不构建，不以文件名相似替代内容证据。
6. 每完成一个调查层次即直接更新本文，不仅在最终回复给摘要。

### 1.3 证据标签

| 标签 | 含义 |
|---|---|
| **事实—文件** | 已由当前工作区文件正文、项目声明、targets/task 代码或生成输出标记直接确认 |
| **事实—静态关系** | 已由静态引用、同名模型节点、生成器注册或消费者字段对应关系交叉确认；仍不等于运行结果 |
| **推断—高/中/低** | 多份静态迹象支持，但缺少完整生成链、历史来源或运行验证 |
| **未确认** | 当前静态阅读不足，不能作结论 |
| **受工具禁令限制** | 必须运行生成器、构建、布局探针、二进制检查或字节流差分才能验证 |

### 1.4 最高迁移约束

- 正确性、ABI、协议编号、字段顺序、结构尺寸与对齐优先于代码风格、抽象和性能。
- 初次迁移必须按原生文件近似直译，保持生成文件及其消费者的路径、类型、字段和 dispatch 可追溯。
- 在等价迁移完成并通过差分门禁前，禁止重构、优化、压缩命令、重排字段、重编号、合并资源类型或重新设计协议。
- 自动生成文件不得作为随意手工修改点；必须先识别权威模型、生成器和冻结输出的关系。
- 若当前开源树无法重现历史生成过程，应显式建立冻结快照与来源台账，而不是猜测重生成或手改协议。

## 2. 关键文件

> 本节将在调查过程中持续追加。当前已确认的上位证据如下。

| 路径 | 用途 | 当前状态 |
|---|---|---|
| `WpfGfxShape/Docs/00-migration-charter.md` | 最高约束、工具禁令、逐文件近似直译与文档要求 | 已读取全文 |
| `WpfGfxShape/Docs/01-native-source-topology.md` | WpfGfx 项目/构建拓扑、processed 类型复制、resources/UCE 生成文件入口 | 已读取全文 |
| `WpfGfxShape/Docs/02-abi-and-managed-callers.md` | 命令布局、句柄类型、固定 64 位协议槽、ABI 和消费者边界 | 已读取全文 |
| `WpfGfxShape/Docs/investigations/03a-generated-model-and-protocol.md` | 本轮过程与结论的唯一目标文档 | 已创建并持续维护 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/README.md` | MilCodeGen 入口说明、生成输出范围、反射调用 `GeneratorBase.Go()` 的控制流 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/mcg.proj` | 独立生成项目；构建 CSP 后通过 `Exec` 调用两个生成脚本 | 已读取全文；未执行 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/tools/GenerateFiles.cmd` | `Resource.xml`/`Resource.xsd` 到仓库源码树的主生成入口 | 已静态读取；绝未执行 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/tools/GenerateElements.cmd` | `Elements.xsd` 生成模型类后处理 `Elements.xml` 的旁支入口 | 已静态读取；绝未执行 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/main/Resources.rsp` | CSP 编译/解释时纳入的主入口、资源模型、helper 与 generator 清单 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/main/ResourceGenerator.cs` | 校验 XML/XSD、创建 `ResourceModel`、反射发现并执行全部 concrete `GeneratorBase` | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/xml/Resource.xml` | MIL 类型、枚举、命令、资源、RenderData 指令、生成控制与模板实例的主权威输入 | 已读取关键区段；全文 4498 行未逐行语义审计 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/xml/Resource.xsd` | Resource.xml 的结构、字段和生成控制 schema | 已读取关键区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/ResourceModel/ResourceModel.cs` | XML 三阶段对象化：建对象、连引用、建 generation list | 已读取关键区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/ResourceModel/Command.cs` | XML 命令对象模型及 marshaled size 计算入口 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/helpers/PaddedCommand.cs` | 统一 command/resource/render-data 指令，注入 Type/Handle/Size 并计算 padding/alignment | 已读取关键区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/xml/Version.xml` | 协议 fingerprint 的手工 revision 输入，当前 MIL/DWM 均为 771 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/generators/ProtocolFingerprint.cs` | 对命令名、字段名/类型/大小/offset/pad/handle/animation 和顺序做 fingerprint | 已读取全文；发现主响应文件未列入该源文件 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/generators/{CommandStructure,CommandType,CommandProcessMessage,ResourceType,DuceResource,RenderData,ResourceFactory}.cs` | 命令结构/编号、服务端路由、资源编号、资源编组、RenderData 和工厂 dispatch 的主要 emitter | 已读取关键区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/{wgx_core_types.w,wgx_av_types.w,GraphicsInclude.nativeproj,gencode.pl}` | 旧 `.w` 模板成对生成链及聚合规则 | 已读取关键文件/区段；未执行 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/{README.txt,wgx_core_types.*,wgx_av_types.*}` | 当前原生构建复制消费的历史冻结输出 | 已读取关键文件/区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/Include/Generated/{wgx_command_types.*,wgx_commands.*,wgx_resource_types.*,wgx_renderdata_commands.h}` | MilCodeGen 签入协议输出 | 已读取代表文件/区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/{generated_process_message.inl,generated_resource_factory.*}` | 服务端命令路由和资源类型到 DUCE 类的工厂 dispatch | 已读取代表区段 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/{marshal_generated.cpp,data_generated.h,resources_generated.h,renderdata_generated.cpp}` | 资源更新编组、数据声明、前向声明和 RenderData 解析 | 已读取代表区段 |
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/{Generated/wgx_commands.cs,wgx_core_types.cs,wgx_av_types.cs,wgx_sdk_version.cs}` | PresentationCore 当前实际编译的协议/类型副本 | 已读取代表文件/区段 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/PresentationCore.csproj` | 确认当前托管消费者使用 Common/Graphics 副本 | 已读取相关区段 |
| `eng/WpfArcadeSdk/tools/WPF_Generated_Files.txt` | 仓库受保护生成文件清单 | 已读取全文 |
| `eng/WpfArcadeSdk/tools/pre-commit.githook` | 明确规定生成文件只能由生成脚本修改 | 已读取全文；未执行 |
| `src/Microsoft.DotNet.Wpf/src/PresentationBuildTasks/PresentationBuildTasks.csproj`、`Microsoft.WinFX.targets` | WPF XAML/资源/markup build tasks | 已读取关键区段；未发现 MilCodeGen/WpfGfx Resource.xml 关系 |
| `Documentation/codegen.md`、`eng/WpfArcadeSdk/tools/CodeGen/**` | 当前仓库通用 T4/AvTrace 体系 | 已读取关键文件；未发现被 WpfGfx/MilCodeGen 引用 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_core_types.w` | 核心类型聚合 schema；C# 与 C++ 分段，并嵌入 MilCodeGen 片段 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_av_types.w` | AV enum/packet 的 C#/C++/COMMON 分段 schema | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/GraphicsInclude.nativeproj` | 历史 schema 项目；用 Perl 将两个 `.w` 生成同名 `.h/.cs` | 已读取全文；不在当前解决方案枚举，未执行 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/gencode.pl` | `.w` 分段处理与 C++→C# 低层类型翻译器 | 已读取全文；绝未执行 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_core_types_compat.cs` | 仅 C# 聚合的兼容枚举/结构输入，并自带“保持 unmanaged 同步”警告 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/README.txt` | processed 四文件的历史来源声明 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_{core,av}_types.{h,cs}` | 当前签入历史预生成输出；原生构建复制来源 | 已读取代表区段/AV 全文 |
| `src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_{core,av}_types.cs` | PresentationCore 当前明确编译的托管副本 | 已读取代表区段/AV 全文 |
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/Graphics.props` | processed 四文件复制到各原生项目中间目录的当前构建动作 | 已读取全文 |
| `src/Microsoft.DotNet.Wpf/src/PresentationCore/PresentationCore.csproj` | 确认 Common/Graphics 命令、SDK、AV/core 类型副本进入当前托管编译 | 已读取相关区段 |

## 3. 生成器 / 输入 / 输出矩阵

| 生成器/任务 | 模型输入 | 原生输出 | 托管输出 | 消费方 | 状态/证据 |
|---|---|---|---|---|---|
| `mcg.proj` → `GenerateFiles.cmd` → CSP → `ResourceGenerator.Main` | `xml/Resource.xml` + `xml/Resource.xsd`；输出根为 `src/Microsoft.DotNet.Wpf` | 下列各 generator 的 WpfGfx 头/源 | PresentationCore、WindowsBase、PresentationFramework、Shared 等大量生成 C# | 独立显式生成项目，不是普通 WpfGfx/PresentationCore 项目构建中的已确认 target | **事实—文件**；本轮未执行 |
| `ResourceGenerator` + `ResourceModel` | `Resource.xml` 中 `Primitives`、`Structs`、`Enums`、`Commands`、`Resources`、`RenderDataInstructions`、`GenerationControl`、`TemplateGenerationControl` | 为下游 generator 提供统一对象模型 | 同左 | 所有 concrete `GeneratorBase` 子类 | **事实—文件** |
| `CommandStructure` | `ResourceModel.Commands` + 可生成的 Resources + RenderDataInstructions，经 `PaddedCommandCollection` 统一 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/Include/Generated/wgx_commands.h` | `src/Microsoft.DotNet.Wpf/src/WpfGfx/Include/Generated/wgx_commands.cs` | 原生 UCE/resources；托管 DUCE 命令写入 | **事实—文件**；当前另有 Common/Graphics 副本，后续核对 |
| `CommandType` | 与 `CommandStructure` 相同的 `PaddedCommandCollection` 及模型顺序 | `wgx_command_types.h`、`exts/cmdstruct.h` | `wgx_command_types.cs` | command switch、调试扩展、托管 DUCE | **事实—文件** |
| `ResourceType` | `ResourceModel.Resources` 的顺序；仅非 value type 且 `HasUnmanagedResource` | `wgx_resource_types.h` | `wgx_resource_types.cs` | CreateResource、资源工厂、托管资源句柄类型 | **事实—文件** |
| `DuceResource` | `GenerationControl` 中 `NativeDuce` 资源及其字段/继承/collection 属性 | `core/resources/marshal_generated.cpp`、`data_generated.h`、`resources_generated.h` | 资源类的 managed DUCE 更新代码由其它 managed generators 同源产生 | `core/resources` 与 generated managed resource classes | **事实—文件**；内容关系后续细查 |
| `RenderData` generator | `RenderDataInstructions` 及其字段、动画参数、NoOpGroups、三个目标目录 | `Include/Generated/wgx_renderdata_commands.h`、`core/resources/renderdata_generated.cpp` 等 | `PresentationCore/.../Media/Generated/DrawingContext*.cs`、`RenderData*.cs` 等 | 托管 DrawingContext/RenderData；原生 render-data parser | **事实—文件**；具体输出全集后续细查 |
| `ResourceFactory` | 可生成的非 abstract native DUCE resources 与资源类型名 | `core/uce/generated_resource_factory.h/.cpp` | 无直接成对 C# 工厂输出 | UCE CreateResource dispatch | **事实—文件**；dispatch 细节后续细查 |
| `CommandProcessMessage` | 同一 `PaddedCommandCollection`；排除 DWM/Redirection/RenderData instruction 等由其它路径消费的项 | `core/uce/generated_process_message.inl` | 无直接托管输出 | `CComposition::ProcessCommandBatch` | **事实—文件**；生成 size 检查、资源类型校验、transport gate 和路由 switch |
| `MiscDef` / `MilRenderTypesGenerated` / `WincodecPrivateGenerated` | `GenerationControl` 中 `Managed`、`NativeNotInKernel`、`NativeIncludingKernel`、`NativeMilRenderTypes`、`NativeWincodecPrivate` | `wgx_misc.h`、`wgx_render_types_generated.h`、`wincodec_private_generated.h` | `wgx_misc.cs` 等 | WpfGfx 与 managed common types | **事实—文件** |
| `ProtocolFingerprint`（源码存在） | 同一 `PaddedCommandCollection` + `xml/Version.xml`/`.xsd` | generator 声明输出 `Include/Generated/wgx_sdk_version.h` | generator 声明输出 `Include/Generated/wgx_sdk_version.cs` | `MilVersionCheck`、factory SDK check、PresentationCore `Version` | **事实—文件 + 未确认**：`Resources.rsp` 未列 `ProtocolFingerprint.cs`，实际签入文件位于 `WpfGfx/include` 与 `Common/Graphics`，路径/纳入方式存在漂移，禁止假定当前运行可重现 |
| `GenerateElements.cmd` + `SimpleGenerator` | `Elements.xml` + `Elements.xsd`；先由 `xsd.exe` 重建 `ResourceModel/Generated/Elements.cs` | 无本专题核心 native 协议输出 | `UIElement.cs`、`UIElement3D.cs`、`ContentElement.cs` | PresentationCore | **事实—文件**；与 Resource.xml 协议主链并列而非替代 |
| `GraphicsInclude.nativeproj` → `gencode.pl` | `wgx_core_types.w`，并递归嵌入 compat、SDK version 与 MilCodeGen `Generated/*` 片段 | `wgx_core_types.h` | `wgx_core_types.cs` | WpfGfx 全局 include；PresentationCore 历史发布路径 | **事实—文件**：历史项目依赖旧内部 targets；当前普通构建不执行，改复制 processed 快照 |
| `GraphicsInclude.nativeproj` → `gencode.pl` | `wgx_av_types.w` | `wgx_av_types.h`，额外含 C++-only `AVEventData` | `wgx_av_types.cs`，只含 shared `AVEvent` | core/av；PresentationCore media event parser | **事实—文件**：有意非对称成对输出 |
| `Graphics.props::CopyGeneratedIncludeFilesToIntermediateFolder` | `include/processed` 四个签入文件 | 复制 `.h` 到每个原生项目中间目录 | 同时复制 `.cs`，但当前 PresentationCore 不从该路径编译 | 所有导入 `Graphics.props` 的 WpfGfx C++ 项目 | **事实—文件**：只在目标不存在时复制，不是真正生成 |
| 通用 T4/AvTrace targets | 各项目 `AvTraceMessages.xml` + `.tt` | 本专题未见 WpfGfx 协议原生输出 | trace message/source C# | WindowsBase/相关 managed 项目 | **事实—文件**：当前 T4 体系与 MilCodeGen 是不同链，WpfGfx 子树未发现引用 |
| PresentationBuildTasks | XAML/Page/Resource 等项目项 | `.baml`/resources 等 markup 构建产物 | markup 编译生成源 | 普通 WPF 应用/框架项目 | **事实—文件**：未发现其参与 `Resource.xml`、MilCodeGen 或 WGX 协议生成 |

### 3.1 主生成控制流

**事实—文件：**静态控制流为：

1. `mcg.proj` 先构建 `src/Microsoft.DotNet.Wpf/src/WpfGfx/tools/csp/csp.csproj`，再以 `Exec` 调用 `GenerateFiles.cmd` 与 `GenerateElements.cmd`。
2. `GenerateFiles.cmd` 把输出根设为 `$(RepoRoot)/src/Microsoft.DotNet.Wpf`，向 CSP 传入 `main/Resources.rsp`、`xml/Resource.xml`、`xml/Resource.xsd`。
3. `Resources.rsp` 列出 `ResourceGenerator.cs`、ResourceModel、helpers 和 generators；CSP 是“compile & run in one step”的 C#/C# Prime interpreter。
4. `ResourceGenerator.Main` 通过 XSD 校验并读取 XML，构造 `ResourceModel`；随后反射遍历执行程序集中的每个 concrete `GeneratorBase.Go()`。
5. `ResourceModel` 分三阶段处理同一 XML：`CreateTypeObjects` 创建 enum/primitive/struct/command/resource/render-data 对象，`CreateTypeReferences` 解析字段/继承/目标引用，`CreateGenerationList` 解释 `GenerationControl` 与模板实例。

这说明不是为命令、资源、托管类分别维护孤立模型；一个 `Resource.xml` 对象图被多个 emitter 读取并写到多个仓库目录。

### 3.2 Resource.xml 承载的模型种类

**事实—文件：**`Resource.xsd` 和 `Resource.xml` 直接确认同一主输入至少承载：

- primitive 类型及 `MarshalAs`、`MarshaledSize`、`SameSize`；例如 `MILCMD=4`、`ResourceHandle=4`、`ResourceType=4`、`BOOL=4`、`UInt64=8`；
- struct、enum 及 enum 显式 `Value`；
- command 的 `Domain`、`Target`、`Name`、fields、`UnmanagedOnly`、`HasPayload`、安全属性；
- resource 的继承、字段、动画、managed/native 生成开关、collection、payload/identity/pack 等；
- RenderData 指令、字段、动画变体、NoOpGroups，以及 managed/native/export 三个目标目录；
- `GenerationControl` 中每个类型应进入的 managed/native code section 和目标目录；
- `TemplateGenerationControl` 的 generator 类型名及实例参数。

**事实—文件：**模型中已经显式把若干地址/句柄协议槽定义为 `UInt64`，例如 D3DImage 的 `pInteropDeviceBitmap`、`pSoftwareBitmap`、`hEvent` 和 MediaPlayer 的 `pMedia`；这不是 C# `IntPtr` 的自由选择。

### 3.3 PresentationBuildTasks 与 T4 的边界结论

- **事实—文件：**`PresentationBuildTasks` 的项目与 `Microsoft.WinFX.targets` 注册的是 markup compile、UID、resource generator、file classifier、temporary target assembly 和 localization 等任务；在 WpfGfx 子树和已读 MilCodeGen 文件中未找到对该程序集/任务的引用。
- **事实—文件：**`Documentation/codegen.md` 与 `eng/WpfArcadeSdk/tools/CodeGen/DesignTimeTextTemplating.targets` 描述的是当前通用 T4 体系；已发现的 `.tt` 仅为 AvTrace/PresentationTraceSources，WpfGfx 子树未找到对应 import。
- **结论：**本专题的 WGX/MIL 资源与命令协议主链是 **MilCodeGen + CSP + XML/XSD**，不是 PresentationBuildTasks，也不是当前 T4/AvTrace targets。
- **未确认：**历史内部构建是否曾通过更高层 build orchestration 间接调用 MilCodeGen；当前静态树只确认独立 `mcg.proj` 可显式调用，未确认普通产品构建自动依赖它。

### 3.4 已发现的生成链漂移/不完整点

- `ProtocolFingerprint.cs` 是 concrete `GeneratorBase`，但 `main/Resources.rsp` 当前未列出它；因此按已读主响应文件，它不会进入 CSP 执行程序集。与此同时，签入的 `wgx_sdk_version.cs/.h` 明确写着由该 generator 产生。
- generator 声明把 SDK version 写入 `src/WpfGfx/Include/Generated`，当前可见签入文件却位于 `src/WpfGfx/include/wgx_sdk_version.cs/.h`，另有托管副本 `src/Common/Graphics/wgx_sdk_version.cs`。
- `WPF_Generated_Files.txt` 列出 380 个受保护生成文件，但没有列入 SDK version 文件、`core/resources/*_generated.*`、`core/uce/generated_*`；因此该清单是仓库提交保护清单，不是完整 MilCodeGen 输出全集。
- `src/Microsoft.DotNet.Wpf/src/WpfGfx/tools/milcodegen/MilCodeGen.asmmeta` 带 CLR 1.1/旧程序集元数据痕迹，而当前 README 指向 CSP 源码式执行。其当前用途未找到静态引用，暂列**历史工具元数据候选**。

以上漂移意味着：即使生成器源码和模型均在仓库，也不能在未实际生成和差分前宣称“当前生成链可无损重现全部签入输出”。

## 4. 原生 / 托管成对输出

### 4.1 核心类型是两段式生成族，不是单一文件对

**事实—静态关系：**`wgx_core_types` 的完整历史生成链是：

```text
Resource.xml
  -> MilCodeGen generators
     -> Include/Generated/wgx_misc.{h,cs}
     -> Include/Generated/wgx_command_types.{h,cs}
     -> Include/Generated/wgx_resource_types.{h,cs}
     -> Include/Generated/wgx_commands.h
     -> Include/Generated/wgx_renderdata_commands.h

wgx_core_types_compat.cs + wgx_sdk_version.h + 上述 Generated 片段
  -> wgx_core_types.w
  -> gencode.pl
  -> wgx_core_types.h + wgx_core_types.cs
```

其中：

- `wgx_core_types.w` 的 `CSHARP_ONLY` 段嵌入 `wgx_core_types_compat.cs`、`Generated/wgx_misc.cs`、`Generated/wgx_command_types.cs`、`Generated/wgx_resource_types.cs`，并声明路径几何等显式布局结构。
- 其 `CPP_ONLY` 段定义 32 位协议 handle 与指针宽度对象 handle，嵌入 `wgx_sdk_version.h`、`wgx_misc.h`、资源/命令枚举、命令结构和 render-data 结构。
- 原生协议片段被整体包在 `#pragma pack(push, 1)` / `#pragma pack(pop)` 中。
- `gencode.pl` 不是普通文本复制：它识别 `CSHARP_ONLY`、`CPP_ONLY`、`COMMON`、`PUBLIC`，递归处理 `;include_header`，并把部分 C++ enum/struct/type 翻译成 C#。

因此，以下内容共享一个复合生成源：

- `MILCMD` 编号：`Resource.xml` 顺序 → `CommandType` → `wgx_command_types.*` → `wgx_core_types.*`；
- `MIL_RESOURCE_TYPE` 编号：`Resource.xml` 资源顺序 → `ResourceType` → `wgx_resource_types.*` → `wgx_core_types.*`；
- command native 结构：`Resource.xml` fields → `PaddedCommand` → `wgx_commands.h` → `wgx_core_types.h`；
- managed command 结构则由同一 `CommandStructure` 直接写 `wgx_commands.cs`，不嵌入 `wgx_core_types.cs`；
- shared enum/struct：部分来自 `Resource.xml`/`wgx_misc.*`，部分来自 `wgx_core_types_compat.cs` 或 `.w` 自身。

### 4.2 当前签入与消费路径

| 角色 | core | AV | 已确认状态 |
|---|---|---|---|
| schema/input | `include/wgx_core_types.w` + `wgx_core_types_compat.cs` + MilCodeGen fragments | `include/wgx_av_types.w` | 签入输入 |
| 历史 generator | `include/GraphicsInclude.nativeproj` + `gencode.pl` | 同左 | 依赖旧 `$(_NTDRIVE)$(_NTROOT)`/`Microsoft.DevDiv.Native.targets` 与 Perl；未进入当前解决方案枚举 |
| 历史预生成输出 | `include/processed/wgx_core_types.h/.cs` | `include/processed/wgx_av_types.h/.cs` | `processed/README.txt` 明称从 NetFxDev1 build intermediate outputs 复制 |
| 当前 native build 输入 | processed `.h` 被 `Graphics.props` 复制到各项目 `IntermediateOutputPath` | 同左 | 当前构建动作是复制，不是生成 |
| 当前 managed build 输入 | `Common/Graphics/wgx_core_types.cs` | `Common/Graphics/wgx_av_types.cs` | `PresentationCore.csproj` 明确编译 |

`Graphics.props` 的复制有 `!Exists(destination)` 条件；静态配置允许已有中间文件不被覆盖。是否在增量构建中产生陈旧消费必须由后续 clean/incremental build 证据确认。

### 4.3 Core 成对输出的不变量与实际漂移证据

**事实—文件：**processed 原生/托管输出与 Common 托管副本在代表性协议项上对应：

- `MilCmdPartitionRegisterForNotifications = 0x03`；
- `TYPE_NULL = 0`，后续资源类型按同一顺序连续编号；
- `MIL_PATHGEOMETRY` 字段 offset 为 `0/4/8/40/44`；
- `MIL_PATHFIGURE` 字段 offset 为 `0/4/8/12/16/32/36`；
- 32 位 `HMIL_OBJECT/HMIL_RESOURCE/HMIL_CHANNEL` 与指针宽度 `MIL_CHANNEL/HMIL_CONNECTION` 明确分离。

**事实—文件：当前副本已经存在内容漂移。**`wgx_core_types_compat.cs` 与当前实际编译的 `Common/Graphics/wgx_core_types.cs` 都含 `MIL_RT_DISABLE_DIRTY_RECTANGLES = 0x00010000`；`include/processed/wgx_core_types.cs` 不含该成员。它同时不应自动推导为原生 `.h` 必须有该 symbol，因为 compat 文件只在 `.w` 的 `CSHARP_ONLY` 段被聚合；但它直接证明：

1. `Common/Graphics/wgx_core_types.cs` 不是可假定与 processed C# 快照逐字相同的简单副本；
2. 当前托管事实来源与当前原生复制来源可能处于不同历史快照；
3. 迁移基线必须分别冻结“实际托管编译文件”和“实际原生编译文件”，不能任选其中之一代表全协议；
4. 只有逐成员/逐布局/逐值差分后才能裁决预期差异与危险漂移。

本轮还确认 Common 与 processed 文件头/空白/usings 等文本本就不同；未使用自动 diff，因此除上述明确成员差异外，不声称已穷尽全部差异。

### 4.4 AV 成对输出的有意非对称

`wgx_av_types.w` 的 `COMMON` 段定义 `AVEvent`，因此：

- processed `.h`、processed `.cs`、当前 `Common/Graphics/wgx_av_types.cs` 均有值 `0..13` 的同名枚举；代表值 `AVMediaOpened=1`、`AVMediaFailed=8`、`AVMediaScriptCommand=12`、`AVMediaNewFrame=13` 已静态核对一致。
- `AVEventData` 位于 `CPP_ONLY` 段，所以只进入 native `.h`。它按 4 字节对齐，字段顺序固定为四个 32 位头字段 `avEvent/errorHResult/typeLength/paramLength`，随后是 `WCHAR[1]` 变长尾。

这不是生成缺失。native `core/av/mediaeventproxy.cpp` 按该结构组包，PresentationCore `mediaeventshelper.cs` 则把 byte buffer 作为四个 32 位头字段和 UTF-16 尾数据读取。迁移时必须按包格式差分，而不是强行在 C# 侧添加同名结构后改变现有解析路径。

### 4.5 历史转换器自身也是协议证据

`gencode.pl` 的静态翻译规则包括：

- pointer → `IntPtr`；
- `HMIL_OBJECT`/`HMIL_RESOURCE` 等 → `UInt32`；
- `WMG_HANDLE64` → `UInt64`；
- `BOOL` → `Int32`；
- common struct → `[StructLayout(LayoutKind.Sequential, Pack=1)]`；
- enum 大值使用 unchecked cast，MIL flag enum 添加 `[Flags]`。

这些规则不能直接证明当前每个输出仍完全由该版本脚本产生，但它们说明历史成对输出的位宽和 pack 选择是显式生成规则，不是现代 C# 类型推断空间。

## 5. 命令协议

> 待记录命令 ID、资源类型 ID、枚举值、结构布局、大小/对齐、字段顺序、可变尾数据、固定 64 位槽、客户端写入与服务端解析。

## 6. 资源工厂

> 待记录资源类型到具体 DUCE 类的 dispatch、创建失败语义、默认分支、类型 ID 来源和与生成模型的关系。

## 7. 布局与编号不变量

> 待形成必须冻结和差分验证的不变量清单。

## 8. 签入 / 构建时状态

> 待区分：仓库签入输出、构建时复制、构建时真正生成、中间产物、历史预生成/来源不完整文件。

## 9. 迁移策略：冻结与近似直译，不重构

### 9.1 当前强制基线

- 等价迁移前不重新设计生成模型或命令协议。
- 在生成器可重现性未证明前，以当前被 WPF/WpfGfx 实际消费的签入输出建立版本化冻结快照。
- 原生生成输出迁移到 C# 时仍按文件和类型近似直译；不得把多个生成文件任意揉成新的抽象层。
- 任何以后复用或移植生成器的工作必须与首次协议镜像分阶段进行，并以字节级/布局级差分为门禁。

### 9.2 强制分阶段策略

1. **冻结当前消费基线。** 对原生实际 include/compile 的签入输出和 PresentationCore 实际编译副本建立 hash/清单、布局与协议测试；此阶段不运行 generator 替换文件。
2. **镜像输出并近似直译。** 在单一 C# 项目中按当前输出文件边界建立对应类型、常量、router、factory 和 resource update 文件；同时保留模型/emitter/消费者链接。
3. **恢复可重现性验证。** 独立运行当前 MilCodeGen 和历史 `.w` 链的可行部分，与冻结输出做全文/语义差分；任何漂移先记录，不自动接受新结果。
4. **决定长期生成方式。** 只有全部协议等价证据通过后，才可选择继续冻结输出、移植 generator 或建立新的共同模型；该决定不得与首次翻译混合。
5. **重构另立阶段。** 统一生成器、压缩重复文件、改变模板语言或优化协议均在完整等价迁移后另立计划。

## 10. 前置依赖

推荐前置顺序不是新架构分层，而是为保留现有协议建立承载面：

1. 固定宽度整数、HRESULT/WGX 错误、`BOOL`、GUID、RECT、32 位 DUCE handle、指针宽度对象 handle 和固定 64 位协议槽。
2. `GENPAIR-CORE-TYPES` / `GENPAIR-AV-TYPES` 的冻结清单和布局测试。
3. `MILCMD`、`MIL_RESOURCE_TYPE`、command header、payload、message/packet 和 SDK fingerprint。
4. handle table、resource base type、composition/channel 的签名骨架。
5. generated router、resource factory、resource data declarations 和 `ProcessUpdate` 签名骨架。
6. 逐资源实现、RenderData parser 和真实 UCE command processing。

循环只能通过同一项目中的类型/签名骨架承载，不得通过重新设计协议或合并 resources/UCE 职责消除。

## 11. 风险与未决问题

- 当前解决方案可能未加载历史生成器项目或内部工具。
- 文件名中的 `Generated`/`processed` 不足以证明当前构建会真正执行生成。
- 签入输出可能来自旧内部构建树；其模型或模板可能已缺失、漂移或只部分开源。
- 同一数值可能同时出现在模型、托管 enum、原生 enum 和 dispatch 表；必须确认权威来源，不能以重复文本自动认定同源。
- 当前禁止运行生成器和构建，不能证明模型与签入输出无漂移。
- `ProtocolFingerprint.cs` 与 `Resources.rsp` 的纳入不一致，以及 SDK version 输出路径漂移，是当前已知的生成可重现性风险。
- `Resource.xml` 的若干 primitive 仍记录指针/Win32 handle 为 4 字节，同时注释要求逐步改为 `UInt64`；必须区分这些历史 primitive 与实际命令中已经冻结的 64 位协议槽。
- `Resource.xml` 行序直接影响 command/resource 枚举的顺序编号；任何“整理 XML”都可能是协议破坏，而不是无害格式化。
- Common/Graphics 当前托管 core 类型副本与 processed C# 历史快照已有至少一个明确成员差异；在全量差分前，不能把任一副本单独称为完整权威模型。
- 历史 `GraphicsInclude.nativeproj` 依赖仓库外旧内部 targets，当前能否重现 `.w` 生成链未确认。

## 12. 未验证与工具禁令限制

当前明确未执行：

- 任何 MilCodeGen、T4、MSBuild task、代码生成脚本或其它生成器；
- 任何构建、还原、测试、预处理、编译或链接；
- native `sizeof/offsetof`、托管 `Marshal.SizeOf/OffsetOf` 或真实命令字节流探针；
- 签入输出与重新生成输出的自动差分；
- 实际 DLL、PDB、资源、导出或运行时 dispatch 验证。

这些项目在完成前不得写成“已验证一致”。

## 13. 对迁移顺序的输入

`03-migration-order.md` 必须吸收以下结论：

- 批次 0/1 先冻结并承载基础 ABI、core/AV types 和布局；完整 generated protocol 在独立批次收口。
- generated protocol 工作包必须以“模型/生成器 + 原生输出 + 托管输出 + 至少一个发送者 + 至少一个消费者”为单位，不能只抄一个头文件。
- `generated_process_message.inl`、`generated_resource_factory.*`、`marshal_generated.cpp` 与 `renderdata_generated.cpp` 属于 resources/UCE 循环域的协议枢纽，允许先骨架、后逐文件填充。
- 在命令 ID、resource ID、结构布局、payload 和 fingerprint 门禁通过前，不得开始大规模 resources/UCE 业务实现。
- 生成器可重现性不是首次迁移的前置；冻结当前消费输出才是前置。重跑 generator 产生差异时不得覆盖基线。

## 14. 文件台账字段

除通用逐文件字段外，生成项必须额外记录：

- Generated group ID；
- 模型节点/XPath 或 `.w` 段；
- generator/emitter 与响应文件纳入状态；
- 原生、托管和调试输出路径；
- 当前实际消费者与仅历史消费者；
- 签入/复制/构建时生成/中间产物分类；
- command/resource ID、字段 offset、固定头大小、payload 和 pack/align；
- factory/router/`ProcessUpdate` dispatch 目标；
- 当前消费基线与 generator 重新输出的差分状态；
- 冻结快照、布局、字节流和端到端证据链接。

## 15. 阻塞条件

出现以下任一情况时，相关工作包必须标记 `Blocked`：

1. 无法确定当前实际消费的是哪个重复生成副本。
2. command/resource ID、字段 offset、pack、bool/handle 宽度或 payload 起点未确认。
3. 模型、输出、router、factory 或客户端/服务端任一侧未进入台账。
4. 试图通过重排 XML、合并结构、改用序列化对象或 `IntPtr` 替代固定 `UInt64` 来“简化”。
5. generator 重新输出与冻结基线不同而尚未解释。
6. 实现返回虚假成功以跨过尚未迁移的 command/resource。
7. `MIL_SDK_VERSION`、真实 PresentationCore 调用版本或新 DLL 返回行为不一致。

在上述门禁解除前，文件最多可到 `Scaffolded`，不得标为 `Translated`、`Integrated` 或更高状态。

## 16. 后续动作

1. 下一轮创建项目骨架后，先把当前 command/resource/version/type 清单作为测试数据冻结，不运行 generator 覆盖源文件。
2. 建立 x86/x64/ARM64 布局探针，优先验证 command header、固定 64 位槽、core types、AV packet 和 descriptor。
3. 在允许命令执行后运行 MilCodeGen 到隔离临时目录，与签入输出做自动差分，禁止直接写回仓库。
4. 调查 `ProtocolFingerprint.cs` 未进入 `Resources.rsp`、SDK version 输出路径漂移和 Common/Graphics 副本发布方式。
5. 为 generated router/factory/marshal/render-data 建立独立 Ledger 项和后续循环骨架工作包。

## 17. 过程更新记录

- 已读取 `00-migration-charter.md`、`01-native-source-topology.md`、`02-abi-and-managed-callers.md` 全文。
- 已创建本文档骨架；后续每个调查层次将直接补写本文。
- 已确认本轮绝不使用命令行、终端、脚本、构建、测试或生成器。
- 已确认不修改任何 WPF 源码、项目配置或 `WpfGfxShape/Code`。
- 已定位 MilCodeGen 主链：`mcg.proj` → CSP → `GenerateFiles.cmd` → `ResourceGenerator.Main` → `ResourceModel` → concrete generators。
- 已确认 `Resource.xml` 同时定义 primitive/struct/enum、command、resource、RenderData instruction 和 generation/template control。
- 已确认 PresentationBuildTasks 与当前 T4/AvTrace targets 不属于 WGX/MIL 协议主生成链。
- 已记录 `ProtocolFingerprint.cs` 未列入 `Resources.rsp`、SDK version 输出路径漂移和生成保护清单不完整等静态不一致；尚未运行生成器验证。
- 已确认 `.w` 历史生成链、processed 冻结输出、Common/Graphics 当前托管副本和 MilCodeGen 片段在 core types 中汇合。
- 已确认当前 command 静态范围 `0x01..0x8D`、Debug 尾项 `0x8E`，resource type `1..97`、`TYPE_LAST=98`。
- 已确认 generated router 的 size/payload/resource/transport 路由语义和 resource factory/CreateEmptyResource 的创建、AddRef、初始化与失败清理顺序。
- 已完成冻结优先、近似直译、隔离重生成差分、最后才评估统一生成器的阶段策略和阻塞门禁。
- 已确认 `wgx_core_types` 是 MilCodeGen 片段 + `.w` schema + `gencode.pl` 聚合的两段式生成族；`wgx_av_types` 由同一历史 `.w` 流程成对输出。
- 已确认当前原生构建只复制 `include/processed` 的四个历史预生成文件，而 PresentationCore 编译 `Common/Graphics` 的独立托管副本。
- 已确认 AV 的 `AVEventData` 仅在 C++ 输出中出现是 schema 有意非对称，托管侧按字节包解析。
- 已记录明确漂移：`MIL_RT_DISABLE_DIRTY_RECTANGLES` 存在于 compat/当前 Common 托管副本，但不存在于 processed C# 快照；差异全集未验证。
