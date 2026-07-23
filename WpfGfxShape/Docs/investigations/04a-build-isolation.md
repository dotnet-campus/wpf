# 仓库构建继承与独立项目隔离调查

> 状态：静态调查已收口；实际 MSBuild 求值、还原、构建和发布留待 `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD` 验证  
> 适用范围：拟在 `WpfGfxShape` 下建立、但本轮不实施的独立 .NET 10 Native AOT 生产/测试构建边界  
> 上位约束：`WpfGfxShape/Docs/00-migration-charter.md`  
> 前置事实：`WpfGfxShape/Docs/01-native-source-topology.md`、`WpfGfxShape/Docs/02-abi-and-managed-callers.md`、`WpfGfxShape/Docs/03-migration-order.md`  
> 原专题调查写入边界：只维护本文；稳定结论现已吸收到主项目方案和总计划

## 0. 结论状态图例

本文严格区分：

- **事实—文件**：由当前工作区内已读取文件直接确认。
- **事实—搜索**：由 IDE 文件搜索、grep、代码搜索直接确认；仍不等同于最终 MSBuild 展开结果。
- **决策—建议**：供后续项目实施采用的隔离设计，本轮不创建对应文件。
- **推断—高/中/低**：由多份静态证据综合得出，附重新评估条件。
- **风险**：若不隔离可能污染构建、引用、产物或验证可信度的事项。
- **未决**：现有静态证据不足以裁决的设计或事实。
- **未验证**：必须通过还原、MSBuild 预处理、构建、发布、产物检查或运行测试确认；本轮禁止执行。

## 1. 范围与方法

### 1.1 调查目标

本专题调查：

1. 仓库根 `global.json`、`Directory.Build.props`、`Directory.Build.targets`、`Directory.Packages.props`（如有）、NuGet/Arcade 入口及其对 `WpfGfxShape` 下新 SDK-style 项目的自动影响。
2. 仓库内 `PublishAot`、`NativeLib`、`UnmanagedCallersOnly`、`IsAotCompatible`、`RuntimeIdentifiers`、`SelfContained`、`PublishTrimmed`、`DirectPInvoke` 等现有用法。
3. 哪些模式可复用，哪些 WPF/Arcade 专用逻辑不得继承。
4. 仅设计、不实施的 `WpfGfxShape/Directory.Build.props`/targets、本地包属性、生产项目、Tests 和原生基线边界。
5. 独立 build/publish 入口、配置/RID 矩阵、输出目录和产物命名规则。
6. 哪些根继承无法仅靠 `.csproj` 内属性可靠消除，以及为什么需要局部 `Directory.Build.props` 截断向上搜索。

### 1.2 明确范围外

本轮不执行且不得执行：

- 修改任何 WPF C/C++/C# 源码；
- 修改任何现有 `.vcxproj`、`.csproj`、`.props`、`.targets`、解决方案或仓库构建入口；
- 修改或创建 `WpfGfxShape/Code` 下任何内容；
- 创建生产/测试项目或真正的局部构建边界文件；
- 将新项目加入原 WPF 解决方案；
- 还原、构建、发布、测试、预处理 MSBuild、生成 binlog 或检查实际产物；
- 修改原实现以帮助新项目构建或测试。

### 1.3 工具与验证限制

本轮**绝对禁止任何命令行、终端、脚本或间接命令执行**，包括 PowerShell、cmd、bash、Python、dotnet、msbuild、NuGet CLI、批处理和通过命令调用的构建/枚举工具。

只使用 IDE/工作区提供的：

- 解决方案/项目结构读取；
- 文件名搜索；
- grep/代码搜索；
- 文件读取；
- 符号导航；
- 本文档创建与写入。

所以静态配置只能证明“声明与导入意图”，不能证明最终求值、还原、构建、发布或运行结果。

### 1.4 已先行阅读的文档

| 文档 | 已吸收的强制输入 |
|---|---|
| `WpfGfxShape/Docs/00-migration-charter.md` | 新代码最终位于 `WpfGfxShape/Code`；独立 .NET 10 Native AOT 生产项目和测试项目；尽量隔离仓库全局构建；原 WPF 完全不改；本轮仅静态调查和文档 |
| `WpfGfxShape/Docs/01-native-source-topology.md` | 根 `Directory.Build.props/targets` 会导入 WPF Arcade SDK；WpfGfx 原项目还叠加本地 `Directory.Build.Props` 与 `Graphics.props`；原构建含 WPF 专用项目引用、运输包、资源、C++、导出和输出逻辑 |
| `WpfGfxShape/Docs/02-abi-and-managed-callers.md` | 当前替换目标静态名为 `wpfgfx_cor3.dll`；x86/x64/ARM64 ABI 必须分别验证；测试不能以修改 PresentationCore/WpfGfx 项目制造兼容 |
| `WpfGfxShape/Docs/03-migration-order.md` | 批次 0 先建立独立构建/ABI 骨架；不得批量制造假导出；独立测试承载必须先证明不受 WPF 根 props/targets 干扰 |

## 2. 已读取的关键构建文件

> 调查过程中持续补充。

| 文件 | 用途 | 状态 |
|---|---|---|
| `global.json` | SDK 选择、roll-forward 与 MSBuild SDK 路径入口 | 已读取全文 |
| `Directory.Build.props` | 根级自动属性导入 | 已读取全文 |
| `Directory.Build.targets` | 根级自动目标导入 | 已读取全文 |
| `Directory.Packages.props` | 中央包管理与版本继承 | 精确文件名搜索未找到；相关中央包管理属性 grep 未见 |
| `NuGet.config` | 根包源与还原来源边界 | 已读取全文 |
| `eng/Versions.props` | 仓库包版本属性和产品版本 | 已读取全文；是否自动导入待 Arcade 链确认 |
| `eng/Version.Details.props`、`eng/Version.Details.xml` | Maestro 依赖流生成的版本属性与来源记录 | 已读取全文 |
| `eng/Testing.targets` | 根 targets 对测试项目追加参数和包引用 | 已读取全文 |
| `eng/vs-workaround.targets` | 根 props 经 `AfterMicrosoftNETSdkTargets` 指定的 SDK 后置 target | 已读取全文 |
| Arcade/WpfArcadeSdk 入口 | 根 SDK、仓库约定、生成/输出/项目引用逻辑 | 根导入入口已确认；内部链待展开 |

## 3. 根构建继承

### 3.1 自动发现与导入边界

**事实—文件：** 若未来 SDK-style 项目直接位于 `WpfGfxShape` 下，且该目录没有更近的同名边界文件，则当前仓库根文件构成默认上游：

1. `Directory.Build.props` 是根级 props 入口。它不只是设置几个可覆盖属性，还会在自身求值期间选择并导入 WPF 专用 Arcade SDK props。
2. `Directory.Build.targets` 是根级 targets 入口。它会选择并导入同一 WPF 专用 Arcade SDK targets，随后无条件导入 `eng/Testing.targets`。
3. props 与 targets 的目录搜索彼此独立；只创建局部 `Directory.Build.props` 不能阻止根 `Directory.Build.targets` 被发现，反之亦然。因此可靠隔离必须设计成**成对的局部 props/targets 边界**。
4. 当前文件名搜索只发现根、`packaging`、`src/Microsoft.DotNet.Wpf/src/System.Xaml` 和 tests 子树中的 `Directory.Build.*`；未发现现有 `WpfGfxShape/Directory.Build.props` 或 targets。故未来项目若现在创建，会落入根边界。
5. 对 `WpfGfxShape/NuGet.config`、`WpfGfxShape/global.json` 和 `WpfGfxShape/Directory.Packages.props` 的精确路径搜索也未找到结果；当前该子树不存在更近的 SDK、NuGet 或中央包边界。

**推断—高：** `Directory.Build.props` 通常在项目正文属性之前导入，而 `Directory.Build.targets` 在项目和 NuGet 生成 props/targets之后的后段导入。许多标量属性可在 csproj 中覆盖，但已经发生的 SDK import、item 注入和 target 定义不能用“晚写同名属性”完整撤销。最终精确顺序仍需 MSBuild 预处理验证。

### 3.2 SDK 选择与版本钉扎

**事实—文件：** 根 `global.json` 声明：

- .NET SDK `10.0.107`；
- `allowPrerelease=true`；
- `rollForward=latestFeature`；
- SDK 搜索路径依次包含仓库 `.dotnet` 和 `$host$`；
- 缺失 SDK 时的错误信息要求运行仓库 `eng/common/dotnet.cmd/sh`；
- `tools.dotnet` 同样为 `10.0.107`；
- MSBuild SDK 版本表包含 `Microsoft.DotNet.Arcade.Sdk=10.0.0-beta.26257.101`、`Microsoft.DotNet.Helix.Sdk=10.0.0-beta.26257.101` 和 `Microsoft.Build.NoTargets=3.7.56`。

**自动继承含义：** `global.json` 的 SDK 选择发生在项目求值之前，不能由 `.csproj` 内属性撤销。位于 `WpfGfxShape` 下的独立解决方案/项目仍会向上发现该文件，除非未来在更近位置放置自己的 `global.json` 或从不会发现仓库根的外部入口启动。根版本恰好符合章程的 .NET 10 目标，但它仍属于仓库耦合：SDK servicing/feature-band、预览允许策略和仓库本地 `.dotnet` 优先级会随根文件变化。

**决策—建议：** 批次 0 初始可明确接受并记录根 `global.json` 的 `10.0.107`，避免无理由复制；若目标是连 SDK 解析也与根仓库解耦，则应在 `WpfGfxShape/global.json` 明确钉扎经过验证的 SDK，并仅保留新项目确实需要的设置。是否创建该局部文件必须由主 Native AOT 项目方案裁决，本轮不实施。

`global.json` 中列出的 Arcade/Helix SDK 版本本身不会自动把这些 SDK 导入普通 `Microsoft.NET.Sdk` 项目；真正的自动导入来自根 `Directory.Build.props/targets`。但只要项目或被导入文件按名称请求这些 SDK，版本会由根 `global.json` 解析。

### 3.3 中央包管理与 NuGet

**事实—搜索：** 当前工作区精确文件搜索未找到 `Directory.Packages.props`；对 `ManagePackageVersionsCentrally`、`PackageVersion Include`、`CentralPackage*` 的配置搜索也未见仓库根中央包管理声明。现有依赖版本主要以 `eng/Versions.props` 和自动生成的 `eng/Version.Details.props` 中的 MSBuild 属性表达，但它们是否进入普通项目取决于 Arcade 导入链，而非 NuGet 的 `Directory.Packages.props` 自动发现。

**事实—文件：** 根 `NuGet.config`：

- 在 `<packageSources>` 开头使用 `<clear />`；
- 没有直接列出 `nuget.org`；
- 添加 dependency-flow 专用 `darc-pub-dotnet-dotnet-*` 源，以及 `dotnet-eng`、`dotnet-libraries-transport`、`dotnet-tools`、`dotnet-public`、`dotnet7/8/9/10` 和对应 transport 等 Azure DevOps 源；
- 未在该文件声明 package source mapping，`disabledPackageSources` 为空。

**自动继承含义：** NuGet 配置按目录层次参与还原，不能由普通 `.csproj` 属性完整替代。未来项目若不提供更近、显式清理来源的局部 `NuGet.config`，其包解析会受仓库根私有/公共镜像源集合、源优先级和后续自动更新影响。这会使“独立项目可在普通公开 NuGet 环境还原”无法由当前静态配置保证。

**决策—建议：**

- 若生产项目只使用框架内置 API且无需 PackageReference，可暂不创建局部包版本文件，但仍需验证 restore 不被根源策略或 Arcade 注入影响。
- 一旦引入 Silk.NET、测试框架或其它包，建议在 `WpfGfxShape/NuGet.config` 使用局部 `<clear />` 后只列允许的来源，并记录是否允许 `dotnet-public` 镜像或必须使用 `nuget.org`。本轮不创建。
- 不建议为模仿根仓库而复制完整 `eng/Versions.props`。若多个新项目共享少量版本，可在局部 `Directory.Packages.props` 使用真正的中央包管理，或使用一个局部、明确命名的 package props；二者只能选一种权威来源，避免同一包同时由根属性、CPM 和项目内 `Version` 控制。
- 采用局部 `Directory.Packages.props` 前必须验证 NuGet 的最近文件发现/父级合并行为；当前仓库无现成模式可直接证明。

### 3.4 根 targets 与后置构建行为

**事实—文件：** 根 `Directory.Build.targets` 只有三类动作，但影响面很大：

1. 本地 `eng/WpfArcadeSdk/Sdk/Sdk.targets` 存在时导入它；
2. 否则通过 MSBuild SDK resolver 导入 `Microsoft.DotNet.Arcade.Wpf.Sdk/Sdk.targets`；
3. 无条件导入 `eng/Testing.targets`。

这意味着“未来 csproj 没有显式写 WpfArcadeSdk”并不能证明它没有继承 WPF targets。更近的局部 `Directory.Build.targets` 是阻止该自动后置导入的直接边界。

`eng/Testing.targets` 进一步证明后置继承有可观察副作用：

- 当 `IsTestProject=true` 时，它追加 hang dump/crash dump 参数；
- 自动注入 `Microsoft.Testing.Extensions.HangDump` 和 `Microsoft.Testing.Extensions.CrashDump` 的 `PackageReference`；
- `Coverage=true` 时再注入 code coverage 包和仓库 `eng/CodeCoverage.config` 路径。

因此未来独立 Tests 若继承根 targets，即使测试 csproj 未声明这些包，也会改变还原图和测试入口。单纯在项目内选择 xUnit/MSTest 或设置自己的包版本不能证明测试项目独立。

根 props 还设置 `AfterMicrosoftNETSdkTargets=eng/vs-workaround.targets`；当前该文件只定义一个空的 `_WarnWhenUsingNET10AndVSPriorTo18` target，短期行为轻微，但属性指向根目录本身就是耦合。该文件未来变化会自动进入所有继承根 props 的 SDK-style 项目。

### 3.5 无法仅靠 `.csproj` 内属性规避的继承点

已静态确认的首批项目如下；Arcade 内部链展开后继续补充：

| 继承点 | 为什么项目内属性不足 | 建议边界 |
|---|---|---|
| 根 `global.json` 的 SDK 选择、roll-forward、SDK path | 在项目 XML 求值前决定使用哪个 SDK；csproj 尚未参与 | 明确接受根版本，或以后增加局部 `global.json` |
| 根 `Directory.Build.props` 的 WPF SDK props import | import 在项目正文前发生；后写属性不能“取消导入”，也不能可靠删除已注入 items/imports | `WpfGfxShape/Directory.Build.props` 截断向上发现 |
| 根 `Directory.Build.targets` 的 WPF SDK targets 与 `eng/Testing.targets` import | targets 文件本身已经载入；尝试逐 target 设开关依赖内部实现且易随仓库变化 | `WpfGfxShape/Directory.Build.targets` 对称截断 |
| 根 `NuGet.config` 的 `<clear />` 和源集合 | 包源由 NuGet 配置层次决定，不是普通项目属性的完整等价物 | 必要时增加局部 `WpfGfxShape/NuGet.config` |
| 根 props 中的 `AfterMicrosoftNETSdkTargets` | 根文件把它指向 `eng/vs-workaround.targets`；其消费发生在 SDK targets 导入期，逐项目清空虽可能覆盖但脆弱且不能解决其它根 import | 从源头不导入根 props |

根 `Directory.Build.props` 还预设 `RepositoryName=wpf`、仓库 URL、`WindowsDesktopARM64Support=true`、`TargetFramework=net10.0`、`TargetFrameworkVersion=10.0`、`BuildWithNetFrameworkHostedCompiler=true`、`PublishWindowsPdb=false`、`TestRunnerName=XUnitV3`，并在旧于 MSBuild 18 时注入 `net10.0` 支持项。标量值多数可被项目覆盖，但让未来隔离项目默默依赖“覆盖完整且顺序正确”不是可审计边界，且不能消除同文件中的 SDK import。

## 4. 仓库现有 Native AOT 用法

### 4.1 搜索范围

已搜索：`PublishAot`、`NativeLib`、`UnmanagedCallersOnly`、`IsAotCompatible`、`RuntimeIdentifiers`、`RuntimeIdentifier`、`SelfContained`、`PublishTrimmed`、`DirectPInvoke` 及相关原生库发布属性。

### 4.2 可复用模式

- 当前 WPF 仓库没有可直接复用的生产 `.NET 10 + PublishAot + NativeLib=Shared` 项目模板。
- `Silk.NET/src/Lab/Experiments/CoreRTTest` 属于旧 CoreRT/ILCompiler 实验，目标与工具链均不能复制为当前方案。
- 可复用的只有普通 SDK-style 项目属性写法、RID/输出目录分离思路和 unsafe 代码组织，不包括 WPF Arcade、旧 ILC 属性或历史 AOT 包引用。
- Native AOT shared-library 的具体项目属性与逐架构验证门禁由 `investigations/04b-nativeaot-exports.md` 维护。

### 4.3 不应继承的 WPF 特有逻辑

- 不导入 `Microsoft.DotNet.Arcade.Wpf.Sdk`、`eng/Testing.targets`、WpfProjectReference/运输包替代、WPF C++ props/targets 或仓库产品打包链。
- 不继承根测试包自动注入、仓库版本/签名/强名称策略、WPF 输出复制和 shipping 分类。
- 不引用 `PresentationCore`、原 `wpfgfx.vcxproj` 或其 23 个静态库项目；原实现只通过已冻结 native DLL/harness 参与差分。
- 不把本地 Silk.NET 仓库整体加入原 WPF 解决方案；其采用方式必须由独立 spike 决定。

## 5. 隔离威胁模型

调查将至少覆盖：

1. 根 SDK/Arcade 自动导入污染；
2. WpfArcadeSdk、运输包和仓库专用 PackageReference 注入；
3. 共享源、默认 glob、`Compile`/`None`/`EmbeddedResource` 重复纳入；
4. 隐式或显式 `ProjectReference` 越界；
5. 默认 AssemblyInfo、版本属性、强名称、仓库标识符重复；
6. manifest、资源、`.def`、native library 名和导出标识符冲突；
7. `BaseOutputPath`、`BaseIntermediateOutputPath`、`OutputPath`、`IntermediateOutputPath` 和 publish 目录碰撞；
8. 生产、Tests、原生基线工件相互复制或覆盖；
9. 将原 WPF 构建成功误当作独立项目验证；
10. 将原 WPF 源文件链接编译进新项目而破坏“独立迁移”边界。

## 6. 建议局部构建边界

### 6.1 边界目标

**决策—建议（待证据收口）：** `WpfGfxShape` 应成为一个显式的局部 MSBuild 边界，使其下未来项目不自动继承 WPF 根 `Directory.Build.props/targets`，同时只在局部文件中声明 Native AOT 项目共同需要的最小、可审计属性。

### 6.2 拟议文件职责

本轮只设计，不创建：

- `WpfGfxShape/Directory.Build.props`：截断根 props 搜索；声明局部通用属性和默认项策略。
- `WpfGfxShape/Directory.Build.targets`：截断根 targets 搜索；只加入局部验证/输出规则，不导入 WpfArcadeSdk。
- 可选本地 package props：仅在确有多个项目共享包版本且不与根中央包管理冲突时采用；具体形态待根配置证据确认。
- 独立解决方案/solution filter/build entrypoint：仅位于 `WpfGfxShape`，不修改原解决方案。

### 6.3 为什么需要截断 `Directory.Build.props` 搜索

根 `Directory.Build.props` 在项目正文之前导入 WPF Arcade SDK props，并设置仓库级 SDK/TFM、测试、版本和后置 targets 路径。项目内后写属性不能可靠撤销已经导入的 props、items 和 SDK 行为。故必须在 `WpfGfxShape` 放置更近的局部 `Directory.Build.props`，让 MSBuild 自动搜索在该目录停止；该文件只声明本迁移项目所需的最小公共属性，不手工向上再导入根文件。

### 6.4 局部 targets 的对称截断

根 `Directory.Build.targets` 会导入 WPF Arcade targets 和 `eng/Testing.targets`，后者可向测试项目注入包与运行参数。props 和 targets 的自动搜索彼此独立，因此仅截断 props 不够。必须同时创建局部 `WpfGfxShape/Directory.Build.targets`，默认保持最小或只包含本迁移自己的验证目标，明确不向上导入根 targets。

## 7. 项目、测试与原生基线关系

### 7.1 “原 WPF 完全不改”的实现含义

以下边界已由任务要求锁定：

- 新项目不得修改原 `.vcxproj`、`.csproj`、`.props`、`.targets` 或解决方案；
- 新项目不得以 `Compile Include`/链接文件等方式编译原 WPF/WpfGfx 源文件；
- 新项目不得引用 `PresentationCore`、`WpfGfx` 或其它原 WPF 项目；
- 测试不得通过 `ProjectReference` 调用原实现；
- 若测试需要原实现基线，只能调用已构建 native DLL 的 C ABI，或消费独立、不可变、带来源与架构标识的基线工件；
- 原生基线的生产过程不属于新项目图，不能由新测试项目递归触发原 WPF 构建。

### 7.2 允许的引用方向

```text
WpfGfxShape.Tests ───────> WpfGfxShape production project
native ABI caller/harness -> published wpfgfx_cor3.dll (file boundary only)
differential harness ─────> frozen original wpfgfx DLL artifact (file boundary only)
production project ───────> approved local Silk.NET subset or explicit low-level interop

禁止任何箭头从新项目指向原 WPF csproj/vcxproj/solution。
```

测试可访问生产项目的内部纯逻辑承载，但 ABI 测试必须从真实 native caller 经 PE export 进入，不能以普通托管方法调用替代。

### 7.3 明确禁止的跨项目引用

- 禁止引用 `PresentationCore.csproj`、`wpfgfx.vcxproj` 或任何原 WpfGfx 静态库项目。
- 禁止把原 C/C++ 文件通过链接文件、`Compile Include`、自定义生成或复制方式编入新生产项目。
- 禁止让测试项目递归构建原 WPF 以获取“基线”；基线必须是预先生成、带 hash/RID/config/source revision 的独立工件。
- 禁止从新项目修改或覆盖原仓库输出目录、WPF artifacts、运输包或现有 `wpfgfx_cor3.dll`。
- 禁止直接引用本地 Silk.NET 的总聚合项目；只允许引用经评估的最小项目集合，或采用经记录的冻结生成源码/局部修正版。

## 8. 建议文件树

本轮只设计、不创建：

```text
WpfGfxShape/
  Directory.Build.props
  Directory.Build.targets
  global.json                         # 可选；初始可明确沿用根 10.0.107
  NuGet.config                        # 引入包时建立局部、可审计来源
  WpfGfxShape.slnx                    # 独立入口，不修改 Microsoft.Dotnet.Wpf.sln
  Code/
    WpfGfxShape.csproj                # 唯一生产项目；AssemblyName=wpfgfx_cor3
    Abi/
    common/
    core/
    shared/
  Tests/
    WpfGfxShape.Tests.csproj          # 托管单元/布局/差分编排
    NativeCaller/                     # C/C++ 动态/静态 caller，具体项目形态由 WP-00A 验证
    Baselines/                        # 仅清单/元数据；大型原工件放受控外部位置
  artifacts/
    obj/<project>/<configuration>/<rid>/
    bin/<project>/<configuration>/<rid>/
    publish/<configuration>/<rid>/
    test-results/<work-package>/<configuration>/<rid>/
```

`Code/common`、`Code/core`、`Code/shared` 只建立目录边界，不在 `WP-00A` 创建生产业务类型。

## 9. 独立 build/publish 入口

只描述、不执行：所有入口从 `WpfGfxShape` 局部目录或独立 solution 启动；普通 build 只验证编译/analyzer，Native AOT 产物必须按单一 RID 独立 publish。下一轮需把精确命令/IDE 操作和结果写入交接，但本调查不提前宣称命令可用。

入口必须满足：

- 不以原 `Microsoft.Dotnet.Wpf.sln` 为构建入口；
- 每次显式选择生产项目或测试项目；
- 每次 publish 只选择一个 RID；
- 原生基线构建不是新项目入口的依赖；
- 输出路径包含 project/configuration/RID，禁止覆盖原 WPF artifacts。

## 10. 配置、RID 与输出

### 10.1 配置/RID 矩阵

初始矩阵为 Debug/Release × `win-x64`/`win-arm64`；`win-x86` 在官方和目标 SDK 支持裁决前保持 `Blocked`。一个 RID/配置通过不能替代另一格，具体 Native AOT 门禁见 `04b-nativeaot-exports.md`。

### 10.2 输出与中间目录

统一位于 `WpfGfxShape/artifacts`，至少按 `project/configuration/rid` 分离 `obj`、`bin`、`publish` 与 `test-results`。测试加载 DLL 时必须使用 publish 绝对路径并回读实际模块路径，不依赖当前目录或 PATH。

### 10.3 产物命名规则

必须区分：

- Native AOT 候选生产 DLL；
- 测试宿主/受控 C caller；
- 原生 WPF 基线 DLL；
- ABI 清单、布局探针和差分结果；
- Debug/Release、RID、架构和来源。

候选生产 DLL 的目标基础名为 `wpfgfx_cor3.dll`；最小 probe 使用非生产导出名 `WpfGfxShape_NativeAotAbiProbe_v1`，并在目录/manifest 中标记不可部署。原生基线工件文件名可保持原名，但其存放目录和 manifest 必须携带 `original/candidate`、configuration、RID、source revision 与 hash，禁止靠重命名区分来源。

## 11. 重复定义、资源与标识符冲突预防

- 局部边界只声明一次 `TargetFramework`、nullable/unsafe、输出根与 analyzer 策略；项目文件只声明项目特有属性。
- 生产项目固定唯一 `AssemblyName=wpfgfx_cor3`；测试和 native caller 使用不同名称，禁止生成第二个同名 DLL。
- `EnableDefaultCompileItems` 初始保持 SDK 默认，新增镜像目录时检查重复 glob；生成/冻结文件若显式 Include，必须排除默认重复纳入。
- managed `EmbeddedResource` 与 Win32 `.res/.rc` 严格分开；在 linker/resource spike 前不得把 shader/ETW/version 资源作为普通 embedded resource 伪装完成。
- 版本资源、AssemblyInfo 和 SDK 自动字段必须先枚举实际产物再决定是否覆盖，禁止同时生成冲突的 VERSIONINFO。
- 原生 caller 的头、import library 和 candidate DLL 只从对应 RID publish 目录取得，不从原 WPF 输出目录隐式搜索。

## 12. 事实与决策汇总

### 12.1 已确认事实

1. 前置文档要求未来项目完全独立，不修改原 WPF，且批次 0 必须先证明构建隔离。
2. 前置拓扑调查已静态确认仓库根 `Directory.Build.props/targets` 会导入 WPF Arcade SDK；本专题将直接读取原文件复核具体内容。
3. 当前 DLL 替换身份静态基线为 `wpfgfx_cor3.dll`，但本轮不会构建或替换任何 DLL。
4. 根 `Directory.Build.props` 与 `Directory.Build.targets` 分别自动导入 WPF Arcade props/targets，后者还导入 `eng/Testing.targets`；可靠隔离必须成对截断。
5. 根 `global.json` 当前请求 SDK `10.0.107`；本机 SDK 可用性与 Native AOT 工具链仍由实施工作包验证。
6. 当前仓库没有可直接复制的生产 Native AOT shared-library 项目；旧 Silk.NET CoreRT 实验不适用。

### 12.2 建议决策

1. 未来所有新生产/测试项目只位于 `WpfGfxShape` 局部边界内。
2. 原 WPF 源码、项目和解决方案保持只读；任何基线交互走二进制 C ABI 或独立工件。
3. 在完成根继承调查前，不应先创建 csproj 并依赖项目内属性尝试“覆盖”仓库行为。
4. `WP-00A` 创建局部 props/targets、一个生产项目、一个测试项目和 probe/native-caller 承载；不创建任何生产业务实现。
5. 初始明确接受根 `global.json` 的 10.0.107，同时由局部 props/targets 隔离 WPF 构建；是否另建局部 `global.json` 由首次求值证据决定。
6. 引入 Silk.NET/测试包前创建局部 NuGet 来源与版本策略，不继承不透明的仓库包注入。

## 13. 风险与未决问题

- `Directory.Build.*` 截断后的真实 MSBuild 预处理结果尚未验证。
- 根 `global.json`、根/局部 NuGet 配置和本地 Silk.NET 项目引用组合的还原行为尚未验证。
- `NativeLib=Shared` 的实际 DLL/LIB/EXP/PDB、资源和版本输入能力尚未验证。
- 原生 caller 最合适的独立项目形态及其是否需要 CMake/MSBuild 仍未裁决；不得因此把它并入生产项目。
- 本地 Silk.NET 旧 TFM、旧包依赖和 source generator 可能破坏隔离/AOT，需要 `WP-00G` 独立裁决。
- 是否需要局部 `global.json`、`NuGet.config`、`Directory.Packages.props` 必须由首次 restore/evaluate 证据决定，不能预先创建全部配置文件制造复杂度。

## 14. 本轮未执行与未验证

本轮不会执行：

- NuGet restore；
- MSBuild 项目预处理；
- build/publish；
- 单元、ABI、差分或端到端测试；
- 实际导入链/binlog 检查；
- 产物、导出、资源、AssemblyInfo 或输出目录检查。

因此，任何关于最终属性值、实际包图、最终 SDK 解析、文件复制或二进制产物的结论都必须保持“未验证”。

## 15. 对主 Native AOT 项目文档的输入

- 可直接采用的局部构建隔离原则；
- 必须禁止的根/WPF 构建继承；
- 生产、测试与基线项目边界；
- 配置/RID/输出矩阵；
- 后续批次 0 创建项目时的静态检查清单和运行验证门禁。

## 16. 后续动作

1. 下一轮执行 `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`，只创建本文件第 8 节的最小必要子集。
2. 用 MSBuild 预处理/binlog 或等价 IDE 证据证明局部项目未导入 WPF Arcade/Testing targets；若仍导入，先修隔离，不进入 probe。
3. 创建生产/测试项目和非生产 probe/native caller 承载；不添加 106 个生产导出。
4. 分 configuration/RID 验证输出目录和项目图；记录实际 package/import/target 列表。
5. 更新 `next-session-handoff.md`，把 Native AOT publish、PE/export 检查和动态/静态 native caller 执行统一记录为 `NotRun-ByWorkPackageScope`，并将 `WP-00B-NATIVEAOT-ABI-PROBE-X64` 设为下一唯一工作包；不得在 `WP-00A` 内执行这些项目。

## 17. 过程更新记录

- 已先阅读 `00-migration-charter.md`、`01-native-source-topology.md`、`02-abi-and-managed-callers.md`、`03-migration-order.md`。
- 已确认本轮只允许维护本文，不修改任何源码、项目配置、解决方案或 `WpfGfxShape/Code`。
- 已创建本文骨架；后续每完成一批根配置、Arcade、AOT 或隔离设计调查即直接补写。
- 已收口根 props/targets、NuGet、项目引用边界、建议文件树、输出矩阵和下一轮范围；所有求值/构建/发布结论仍保持未验证。
