# Pcore candidate 认证与原版共存门：最小源码修复已实施

## 独立静态审查接续（执行预算 0）

结论：StaticReviewWithMinimalSourceCorrections / NotCompiled / NotTested / OverallOpen。以下为本次独立文件审查，不是实施代理自检的执行复验。终端、脚本、构建、测试、发布、程序、COM、sandbox、Native 执行全部 0；未运行或加载原版，也未加载候选。未计算新二进制 hash、未认证新包，不移用 stageAsync 历史通过状态。

### 本次确定修正

- AcceptancePaths.OriginalDll 的 getter 直接引用 Visual。candidate 的两个入口虽然只在 original 分支调用它，但原实现没有 NoInlining，不能仅凭分支断言调用者 JIT 不会接触该 WPF 类型。为 getter 增加 NoInlining，隔开直接类型引用，不改 original 的环境变量或 runtime 路径语义。
- Program.Run 还调用 CompareImages / CompareFiles；两者直接使用 typeof(Visual)。同样补 NoInlining，防止含 WPF 类型的比较方法进入认证入口内联体。既有 RunWpf / RunDispatchers 隔离保留。此为源码边界加固，不声称已验证 CLR 的加载/JIT 时序。
- 未修改 SDK、TFM、LangVersion、包版本、生产媒体默认 async、resolver fallback 或 original loader 行为；未新增接口、工程或运行负控。

### 已静态核查

- 配置：global.json 为 SDK 10.0.107 / latestFeature；项目 net10.0-windows、UseWPF、Nullable、ImplicitUsings、unsafe；MSTest 4.0.1，既有 TestMethod + DataRow 风格可沿用。局部 Directory.Build.props/targets 隔离 Arcade；没有仅因新语法而改版本。
- 资源：InvalidNativeIdentity / UnexpectedNativeModules 均有唯一 resx data，与 ResourceManager 的项目默认根命名空间和资源文件名相符；资源 XML 起止及其余键静态一致。实际 manifest 名称和资源编译未验证。
- 合成 PE：DOS e_lfanew=0x80、签名与 COFF machine、SizeOfOptionalHeader 位于 0x94、optional header 起点 0x98；PE32/PE32+ 分别 224/240 字节，magic 0x10b/0x20b，SizeOfHeaders 偏移 60，NumberOfRvaAndSizes 偏移 92/108，16 个 directory 全零。512 字节容纳所有头，节数为零、无 CLR directory，不执行。静态未见偏移越界；PEReader 是否接受该头仍须目标运行时测试。
- 认证门：candidate 两入口都先取完整路径、持有 File.OpenRead（FileShare.Read，无 Write/Delete）再强制读取 aot.sha256 并校验 native PE、machine、SHA；非 Sandbox 同样 fail closed。SHA 空白/错值不能通过，大小写忽略；Identity 的复算摘要须匹配认证摘要。Load 使用保存的 selected/native，不重新取环境变量。AssemblyInitialize 的历史非 Sandbox 策略未顺带改动，--pcore 直接入口也不依赖它作为强认证门。
- WPF/native：Run 的认证前没有直接 WPF 类型局部量；RunWpf 才 Assembly.Load、安装 resolver 和跑场景；DispatcherAcceptance 自身无直接 WPF 类型，RunDispatchers 有 NoInlining。认证失败仅托管取证，不进 Dispatcher/GC 清理或 native Load。ApplyProtection 的函数指针调用位于 resolver 成功 Load 后。静态路径成立不等于进程中绝无外部预加载，仍需入口负控观察。
- 共存：主 VerifyLoaded 不再筛掉 service 模块，对所有同名模块执行唯一规范路径门；dispatcher candidate 在 original 容许条件前先走同一严格门。require=false 只容许零模块，不容许重复/异路径。candidate 的 media/dispatcher 原版共存例外均未保留；original-only dispatcher 的 service 条件还存在，不能把它报告为整个仓库已删除所有例外。
- original：未被新强制 aot.sha256 校验；OriginalDll、requestedNative、originalRuntimeNative 和 frozenCandidate 记录路径仍在。主 VerifyLoaded 现在无角色区分，original 也受严格唯一模块门约束，而 dispatcher original 仍允许 runtime service。这是可观察兼容性风险：显式 original 副本路径且另有 runtime 同名模块时主路径会拒绝。无基线差异或授权运行证据，不能宣称 original 完全无行为影响，也不凭猜测新增例外；默认 selected 为 runtime 路径时并无静态必然失败依据。本次未运行 original。

### 仍待新预算验证与边界

- 本轮 source changes 未编译未测；无编译器诊断、MSTest 发现/执行或资源 manifest 实证。现有测试是判定函数单测，不是两个真实入口的执行负控；缺少 CLR header/缺 optional header、require=false 异路径等显式回归，不能把覆盖称为完整。
- 只读句柄在正常路径覆盖认证、加载、场景返回和清理；异常展开会释放。RunDispatchers 在等待/Join 超时时，后台线程可能尚未退出，故不能断言失败场景所有 native 活动已终止后才释放；这是既有线程失败收尾未闭合风险，本次不扩张为线程生命周期重构。映像 loader 与 FileShare.Read 的兼容性仍待验证。
- 路径比较是 GetFullPath + OrdinalIgnoreCase，不解析 hardlink、重解析点或目录级替换；可信包根、冻结清单来源是前置条件，不是安全对抗边界或全包签名认证。FileVersionInfo 读取 PE 版本资源不等于执行 DLL，但其实际行为本轮未运行验证。
- 下一必须先取得明确新预算，认证新 Native 正式 COM 包的来源、架构、hash、清单与 harness，然后验证 x64 公开 WPF candidate 及认证失败负控，再 x86 / 默认 full。不得挪用历史 hash、发布成功或 stageAsync managed 5/5 充当新源码通过。OverallOpen。

## 同轮补充静态审查明细（非新增执行）

IndependentStaticReview / SourceChanges / NotCompiled / NotTested / OverallOpen。

- 本次仅文件读取、搜索、修改与回读；终端/脚本、构建、测试、发布、程序、COM、sandbox、Native 执行全部为 0；未加载或运行原版。下方实施记录及 stageAsync 历史成功证据不转化为本轮通过项。
- 已独立读取六个指定文件及必要的 SDK、局部构建隔离配置、项目的 net10.0-windows / UseWPF / nullable / MSTest 4.0.1、包清单生成配置和 NativeRun 分派路径。没有修改 SDK、TFM、语言版本或生产媒体源码。
- 修正 AcceptancePaths.OriginalDll getter：添加访问器级 NoInlining，阻止其 typeof(Visual) 被内联进入认证调用者；修正 Program.CompareImages / CompareFiles：添加 NoInlining，隔离 Run 在认证前分派所引用的两个直接含 WPF 类型的方法。不改变 original 的路径求值或比较算法。这是源码边界加固，不声称已由 JIT/运行证据证明零 WPF 加载。
- 资源检查：InvalidNativeIdentity、UnexpectedNativeModules 的调用与 resx 键一致，资源基名与默认项目命名一致，未见重复新增键。C# 集合表达式、nullable using 声明和访问器 MethodImpl 与当前 TFM 默认语言能力相容；未发现必须升级框架的依据。未经过编译器或资源生成验证。
- PE fixture 检查：MZ、e_lfanew=0x80、PE 签名、COFF machine、0x94 optional-header-size、0x98 optional-header 起点、SizeOfHeaders(+60)、NumberOfRvaAndSizes(PE32 +92 / PE32+ +108) 的位置及 224/240 大小自洽，512 字节容纳完整头；无节、代码或 CLR directory。错误架构、错误/空摘要与匹配摘要测试逻辑可静态对应检查门，但 PEReader 实际接受、MSTest 发现及断言结果均未验证。AssemblyInitialize 仍依赖合法外部 candidate；这些不是可脱离产物的纯隔离入口测试。
- candidate 主路径与 dispatcher 均先保存规范化路径、持有 File.OpenRead 句柄，再校验 native PE/进程架构及冻结 aot.sha256，后续 resolver 使用同一保存路径；Identity 的重新摘要须一致。只读句柄按正常 Windows 共享规则拒绝写入/删除，正常返回时覆盖场景与清理。认证另开只读流但原租约仍持有；不构成重解析点、目录替换或可信清单来源的完整安全边界。
- 认证失败分支不进入 RunWpf / RunDispatchers，不安装 resolver 或调用 NativeLibrary.Load，也不进入 WPF Dispatcher/finalizer 清理；主路径 RunWpf、dispatcher RunDispatchers 已有 NoInlining。入口附近的其他 Program 静态字段为托管集合/字符串，未见 WPF 静态对象初始化。静态推理不保证宿主、预加载或运行时绑定层面完全无 WPF assembly load，仍需获授权后观测。
- candidate 原版共存例外确已从两条判定路径删除：主路径所有同名模块进入严格门；dispatcher candidate 先严格门，之后 service 容许条件仅 role=original 可达。原版相关源码并非全删：OriginalDll、original resolver/证据和 dispatcher original service 条件仍在，仅保留历史角色，不授权运行原版。
- original 兼容性不能宣称已证明：主路径 VerifyLoaded 对 original 也调用严格唯一模块门，而 dispatcher original 保留 service 容许条件。无旧版差异基线或实测，不能断言 original 历史媒体共存完全不受影响；本次不恢复 candidate 例外、不新增 original fallback，也不凭猜测扩展 original 容许规则。
- 仍待验证：句柄与 Windows 映像 loader 兼容性、实际模块路径及摘要、认证失败无 WPF/native 副作用、x64/x86 入口及完整媒体/资源门。dispatcher 既有 readiness/join 超时会在后台线程可能存活时退出作用域，故租约“覆盖全部清理”仅能限定正常完成路径；本次未重构线程监督，不冒失败生命周期闭合。
- 下一步必须取得明确新预算，认证新 Native 正式 COM 包的来源/路径/哈希及本轮 source changes 对应产物，先 x64 公开 WPF 与认证负控，再 x86 / full。stageAsync Native 发布历史不是本轮新包认证，OverallOpen。

## 当前状态

Implemented / StaticInspectionOnly / NotCompiled / NotTested / OverallOpen。

本轮用户已明确授权最小源码和测试编辑，执行额度仍为零。本文原 AwaitingAuthorization 状态已被本轮实施取代；保留原文件名以维持交接链接。未构建、测试、发布、运行程序、终端或脚本；normal/tests/COM/sandbox/Native 新增执行均为 0。未认证现有二进制，未修改 stageAsync 历史成功证据或既有通过状态。

已读 current-execution-rules.md 的唯一当前 stageAsync 规则、项目 SDK/TFM、局部 Directory.Build.props/targets、既有源码及 MSTest 风格；未发现 AGENTS.md。旧 public-crossarch 冻结不能自动覆盖最新 stageAsync，sourcefreeze 仅历史事实，本次不再以它阻断编辑。原版 loader、副本、委托 fallback 禁令保持；没有新增 fallback 或原版运行验证。

## 修改路径与具体效果

均相对于 WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/：

1. AcceptancePaths.cs
   - 复用并拆出既有 ValidateIdentity 的流式 PE/架构/SHA256 检查；拒绝缺失 PE optional header、CLR header 以及不匹配进程架构的输入。
   - 新增内部 ValidateCandidateIdentity(path)：无论 Sandbox 开关如何，强制读取 AppContext.BaseDirectory/aot.sha256 并比较；缺失清单或空清单均不能通过。既有无参数入口保持其 Sandbox 策略，避免无关改变历史 AssemblyInitialize 角色。
   - 内部 ValidateDigest 供证据重新计算摘要核对；ValidateCandidateModules 供主路径和 dispatcher 共用严格唯一同名模块门。require=false 仅允许尚无模块，不允许错误路径或重复模块。
2. Pcore/Program.cs
   - candidate 在 Assembly.Load(PresentationCore/WindowsBase)、resolver 安装及 WPF 场景前解析一次规范化路径并认证。candidate 不再访问 OriginalDll 来构造原版身份记录。
   - 持有 File.OpenRead 只读共享句柄直至场景与清理返回，禁止正常 Windows 文件共享语义下写入/替换；认证、requested/frozen 身份和实际 NativeLibrary.Load 共用 selected，不再次读取候选环境变量。validatedNative 记录路径、认证摘要、进程架构；Identity 的重新计算摘要必须匹配认证摘要。
   - WPF 场景与 Dispatcher/finalizer 清理迁入 NoInlining 的 RunWpf。外层认证失败只写托管证据并返回 1，不调用 VerifyLoaded(false)、Dispatcher.CurrentDispatcher、GC/finalizer 或 RunWpf。
   - 场景开始后才做失败取证/清理；其错误单独记录，不替代原始 exception/hresult/failureStage。失败证据落盘出现 I/O/权限错误时将原报告写 stderr 并返回失败。
   - 删除 candidate media runtime 原版共存例外和 typeof(MediaPlayer) 服务路径查询。actualNativeModulePaths 先记录所有同名路径，actualNativeModules 记录身份，再执行严格路径/数量门，不再忽略 runtime 原版。
3. Pcore/DispatcherScenes.cs
   - 同样在加载 PresentationCore/WindowsBase 前认证，强制冻结清单、持有只读句柄、核对证据摘要，以局部 native 绑定 resolver 的 Load。
   - RunDispatchers 添加 NoInlining；认证失败不进入该 WPF 场景，也不执行 WPF 清理。报告保留原始异常文本及 failureStage、validatedNative；失败时避免额外读取 harness 身份覆盖原始错误。
   - candidate 先走严格唯一模块门；模块路径在判定前入 records。原有 runtime service 共存判断仅留给历史 original 分支，不对其做无关重构。
4. AcceptanceResources.resx
   - 新增模块身份错误资源 UnexpectedNativeModules；认证错误沿用 InvalidNativeIdentity。
5. AcceptancePathsTests.cs
   - 使用现有 MSTest TestMethod/DataRow，增加下列回归。PE 仅内存合成头，无节、代码、导入或 CLR directory，不落盘、不执行；模块仅为合成路径，无真实原版加载。

本文即第六个修改文件；未改生产播放器默认 async、stageAsync 历史报告或包内证据。

## 新增回归测试（均未运行）

- WhenCandidatePeIsInvalidThenIdentityIsRejected：非法 PE。
- WhenCandidateArchitectureDiffersThenIdentityIsRejected：与当前进程不同的 machine。
- WhenCandidateFrozenHashDiffersThenIdentityIsRejected：空清单摘要、错误摘要。
- WhenCandidateFrozenHashMatchesThenValidatedDigestIsReturned：冻结摘要正确且忽略大小写，返回实际摘要。
- WhenCandidateCoexistsWithRuntimeOriginalThenModuleGateRejectsIt：media/dispatcher 合成路径集合均拒绝共存；参数名只区分数据路径，不声称实际运行两个 WPF 入口。
- WhenCandidateWasNotLoadedThenModuleGateRejectsIt：缺失实际模块。
- WhenOnlyAuthenticatedCandidateIsLoadedThenModuleGateAcceptsIt：单一相同路径。
- WhenRecordedDigestDiffersFromValidatedDigestThenIdentityIsRejected：证据摘要不一致。
- WhenFailureInspectionFindsDuplicateCandidateModulesThenItRejectsThem：require=false 不豁免重复模块。

这些是认证与模块判定函数回归，不是整个 Pcore 入口的执行负控。未为测试引入 resolver/process-module 模拟层或新工程；入口认证失败无 resolver/native load/WPF cleanup、真实环境变量路径绑定等暂仅静态核查，尚无端到端测试证据。

## 静态自检结论与未解决风险

- 主路径：解析路径 → 持有只读句柄 → 强制清单认证 → requested 摘要核对 → NoInlining RunWpf → Assembly.Load → resolver → Load(selected)。认证失败在外层 catch 返回；外层没有 WPF 类型局部变量、Dispatcher/finalizer 调用。
- sibling：解析路径 → 只读句柄 → 强制清单认证 → 摘要核对 → Assembly.Load → resolver → NoInlining RunDispatchers。认证 catch 仅收集异常并输出托管报告。
- 两条 candidate 路径的实际 Load 均使用认证时保存的路径；未新增 loader/fallback，未加载原版做负控。所有同名模块均参与路径集合判断。
- 静态检查不是编译器/JIT/运行验证。合成 PE 是否被目标 PEReader 接受、MSTest 发现/执行、NoInlining 实际隔离、Windows 文件共享与映像 loader 兼容性，仍待执行验证。
- 只读句柄约束的是常规文件共享访问，不声称对恶意内核写入、重解析点/目录级操纵提供安全边界；包根及清单仍需可信冻结来源，本改动不是数字签名或全包认证。
- 强制清单会让缺少 AppContext.BaseDirectory/aot.sha256 的非 Sandbox candidate 调用立即失败，这是 fail-closed 行为，不新增自动生成清单或绕过开关。
- 场景开始后的既有业务线程/资源行为未重构；本次不闭合全部资源清理、媒体业务、x86/x64 或完整替换。OverallOpen。

## 待授权后的验证命令与对应测试（仅描述，未执行）

从 WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/ 使用 SDK 10.0.107 或兼容 latestFeature：

1. dotnet build WpfGfxShape.ComAcceptance.csproj：编译上述源码/资源及 MSTest 测试，检查诊断。正式包参数需使用另行冻结且可信的 candidate，不能沿用历史 hash。
2. dotnet test --project WpfGfxShape.ComAcceptance.csproj -- --filter-class WpfGfxShape.ComAcceptance.AcceptancePathsTests：包含上述九个测试方法及既有路径测试；DataRow 会展开更多 case。项目使用 Microsoft.Testing.Platform，后续执行前须依据所锁定版本确认筛选参数支持。AssemblyInitialize 仍要求合法 matching-architecture candidate fixture；它校验文件而不执行 DLL，不能省略该前置条件。不得为负控加载原版。
3. 对另行授权、重新构建的正式包分别执行 WpfGfxShape.ComAcceptance.exe --pcore candidate purevideo-public、--pcore candidate media、--pcore candidate dispatcher：验证强制认证、唯一实际模块、requested/validated/actual 身份。它们不是上述 MSTest 合成测试，也不是本轮已通过项；x64/x86 分开记录。
4. 另行授权端到端认证负控：隔离包替换为无效 PE/错误架构/不匹配摘要，分别通过 candidate 主入口和 dispatcher 入口调用，检查 exit 1、failureStage、原始异常、resolverCalls/resolutions 为空、无 native/WPF 初始化及清理副作用；只使用不执行的 fixture，不运行原版。

以上命令需要新执行额度及正式包准备授权。本轮没有执行，也没有通过旧成功结果推定新源码或新二进制通过。
