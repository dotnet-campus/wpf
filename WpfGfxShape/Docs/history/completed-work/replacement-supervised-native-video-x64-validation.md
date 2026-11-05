# 新 x64 正式 Native 发布与 candidate-only 视频验收推进

## 本次独立接续：reference 静态解耦已确认；业务前隔离仍阻塞，安全停止

本节覆盖下方“下一需先找到reference”的视频专属推进建议，不改写历史执行结果。结论：PureVideoReferenceIndependentStatic / PreBusinessIsolationUnproven / CandidateVideoNotRun / FullNotRun / OverallOpen。本接续仅独立读取原件、静态审查和文档更新；没有完成独立打包模式、监督runner或验收源码修复。不能把本节理解为任务1–3已实施完成。

### 亲读证据与身份边界

亲读 `artifacts/supervised-native-video-x64-r1/identity-readback/Evidence/identity.json` 的身份头部、`source.sha256` 头部、身份辅助源代码与项目，以及 `execution-receipts.md` 全文。原件记载 DLL SHA FBD35CDD609DE2D67EB494B41A07FDB9240DC955B3F54D681D48F2423C8A4959、4854272字节、Amd64、无CLR、清单505项；不是本次重新计算的当前DLL或当前源码摘要。未逐项复算505项，不认证源码未变，不据此复用到新正式包。本轮未重发布Native，也未加载它。尚无本次最终包、harness SHA/MVID、最终包内aot.sha256匹配证据。

### 任务1：纯视频真正的reference依赖

静态调用链：`NativeRun.Main` 的首个 `--pcore` 分支直接进入 `Pcore.Program.Run`，不经过MTP的AssemblyInitialize；candidate路径先 `AcceptancePaths.ValidateCandidateIdentity`（PE、架构、aot.sha256、只读租约），然后 `RunWpf` 分派到 `RenderPureVideoPublic`。纯视频复用 `MediaScenes.WriteMediaAsset`、`WaitMedia`、`CopyMedia` 和 Program 的身份/Checkpoint设施。

`PureVideoPublic.cs` 与上述三个共享方法没有读取 `original-wpf.json`、`glyph-arial-original.json`、Baseline或ComparisonCandidate。`WriteMediaAsset(path, 0, includeAudio:false)` 就地生成40帧、10fps、前红后蓝的AVI；`CopyMedia` 是32×16/96DPI/Pbgra32 RenderTargetBitmap的consumer读回。0.5/2.5 seek后媒体时钟达到target+0.3、下一次Rendering、Render优先级派发、Pause、像素alpha255/红蓝dominance/非重复帧/实际时刻小于target+1，以及Ended/Close/解绑/退订/窗口释放/cleanupDiagnostics仍是原合同。40帧是资产输入，不是已测得40次native交付。

其他分支不能混淆：同文件的 `CompareMedia` 确实读取Baseline和ComparisonCandidate；full/glyph测试有各自reference合同，未获授权削弱。`ComAcceptance.Package.targets` 的 `ValidateComAcceptanceReferenceInputs` 是PrepareForBuild前的统一reference门，不是纯视频场景的运行依赖。因此“缺这两个oracle必然阻止任何纯视频验收”的推论不成立；可设计默认关闭、入口严格限candidate+purevideo的独立包，但本接续没有实现或验证该包，普通正式包的既有缺oracle失败仍然有效。

### 任务2：已证实的安全缺口与尚未证实的旁路

- `Pcore/Program.cs:75–81` 在 `RunWpf` 之前完成candidate身份校验及文件租约，这个既有边界有实际静态依据；`OriginalDll` getter有NoInlining，不主动取原版路径。
- `RunWpf:95–169` 虽有NoInlining，方法本体同时包含Assembly.Load、安装resolver、所有场景调用、`Dispatcher.CurrentDispatcher.InvokeShutdown`和失败清理。NoInlining仅保证调用边界，不是方法内部按源码语句顺序解析/JIT WPF类型的保证。`RenderPureVideoPublic`没有独立NoInlining。不能将“外层有NoInlining”认证成“resolver先于所有WPF初始化”。
- `VerifyLoaded()` 在render-and-copy-complete之后；场景启动前没有模块快照/拒绝门。它不能阻止已经发生的原版装载。resolver对不匹配库名返回0，匹配wpfgfx则绝对路径加载candidate；该resolver不是Windows进程级加载拦截，也不覆盖其他程序集或本机依赖的加载。
- 亲读本地 `src/Microsoft.DotNet.Wpf/src/PresentationCore/ModuleInitializer.cs`：模块初始化调用DWriteLoader和NativeWPFDLLLoader。继续亲读 `DirectWriteForwarder/main.cpp`：其LoadDwrite函数只写m_temp，旧注释虽提及加载wpfgfx，不能据该注释声称当前实现确实加载原版。没有发现可据此断言已发生旁路的证据。
- 以上本地源码不是沙盒宿主实际PresentationCore/WindowsBase/DirectWriteForwarder及递归本机依赖的二进制身份审计。还没有实际宿主路径、SHA/MVID、模块初始化/原生导入/显式加载路径的覆盖证据，也没有覆盖resolver之外路径的预防性约束。因此“不会尝试原版加载”仍未得到证明。仅增加业务前一次模块枚举、SetDefaultDllDirectories或事后枚举均不能单独闭合此保证；本次没有用它们冒充进程级阻断。

按用户明确的“无法保证则失败关闭”“不能安全执行先返回已证实阻塞”，本次不启动真实WPF或认证负控。这里是调查后的安全停止，不是宣称Windows隔离在技术上不可实现，也不是预算/授权阻塞。没有添加一条永久拒绝来假装修复，没有关闭full门，也没有改普通包忽略oracle。

### 未完成项与唯一下步

先在不加载WPF/native的只读辅助中冻结、审计拟用宿主实际WPF程序集及原生依赖/初始化路径；明确哪些路径能通过candidate resolver，哪些绕过resolver，形成可审计的业务前保证。若无法排除某条原版加载路径，继续停止并报告具体路径，不进行“试试看”的WPF运行。之后才实施最小独立candidate纯视频包：bootstrap不引用WPF类型、场景与清理独立NoInlining边界、身份和预业务模块门、严格拒绝original/compare/full/额外保护参数/未知场景；保留全部现有纯视频断言与普通full/reference门。再接有界C#监督，冻结最终包身份，先无native副作用负控后实际视频。

有界监督必须分别记录真实child PID/exit、超时/kill/reap/drain状态、stdout/stderr、原始JSON及身份快照；运行期限、kill后回收期限、输出drain期限分别有界。现有ExitDiagnostics偏向MTP过滤/full，不能把它直接当pcore监督已通过；本次没有运行PcoreComparisonTests。没有consumer像素、事件/控制/cleanup的本轮运行证据，managed frame不替代native consumer，视频更不替代full。

### 本接续执行账与改动范围

专用build/MSBuild/test/publish/Windows沙盒各0；子进程/WPF/Native/负控/original/compare/full各0；终端/脚本/原版复制/fallback各0。生产源码、验收源码、辅助源码、生产默认async、oracle/expected/阈值/DPI/flags修改均0。仅更新本报告、恢复入口并新增本接续执行账。没有新的程序原始结果文件或数值exit收据，不能把“未运行”写成运行失败或门已通过。历史Native发布成功、历史正式包缺oracle失败及只读身份结果仍分别有效，不移绑到本次。

---

以下为上一子任务原始报告，保留其历史状态与原始执行账。

## 当前结论

NativePublishPassed / NativeArtifactIdentityRead / FormalHarnessPackagingBlockedMissingTrustedOracle / PublicWpfVideoNotRun / FullNotRun / OverallOpen。

本实施者执行探索、代码和专用发布/Windows沙盒；监督者可只读本报告独立审查。本轮用户已明确授权专用build/test/publish/Windows沙盒、无新次数上限，不适用历史耗尽预算。停止原因是实际打包门禁失败及隔离门未闭合，不是缺授权。没有发现或修改生产缺陷；未改变生产逻辑、默认async、oracle、expected、阈值、DPI、flags，没有producer读回替换consumer。

## 已完成步骤与生产发布

先读恢复入口顶部、生命周期独立复核和正式包身份静态报告、current-execution-rules，并核对章程/连续性入口、解决方案、生产与正式验收项目及打包targets。实际工具工作根是用户所列项目根的父目录，使用 WpfGfxShape/ 前缀定位。

生产项目原配置 net10.0 / PublishAot=true / NativeLib=Shared / SelfContained=true；全局SDK要求10.0.107、rollForward latestFeature，没有修改SDK/TFM/语言版本。正式验收为net10.0-windows、UseWPF=true、MSTest4.0.1；生产不反向引用验收层。

通过专用publish执行一次全新独立生产发布：Release/win-x64/net10.0/self-contained/AOT Shared/ManagedAcceptance=false/两个diagnostic=false。隔离 obj-native、bin-native、native 三目录。工具exit0；亲读原日志尾部 Generating native code、lib/exp创建和发布路径，实际native目录含DLL与PDB。发布不是Native运行。

证据根（相对项目根）：`artifacts/supervised-native-video-x64-r1/`。
- `native/wpfgfx_cor3.dll`、`native/wpfgfx_cor3.pdb`：新产物。
- `bin-native/Release/net10.0/win-x64/native/`：AOT构建输出；不是用于打包的托管同名DLL路径。
- `execution-receipts.md`：逐次专用调用参数及结果人工转录，保留日志截断边界，不伪称完整原始日志。

## Native身份原始证据

只读辅助项目位于 `Tests/SupervisedNativeIdentity/`，无ProjectReference、无UseWPF、不加载native、不起子进程。先检查既有 `.coding-agent/evidence-reader`，其用途为TRX失败索引且返回值绑定历史91例，不适合身份读取，故未挪用。新辅助只使用BCL的PEReader、SHA256及JSON，不改生产与正式验收代码。

新辅助Content明确取本轮native发布DLL、当前生产/正式harness的.cs与项目配置、局部Directory.Build.*。沙盒只推送此独立identity-helper目录，没有runtime原版wpfgfx、旧self-contained目录或其他来源不明同名DLL。

远端task `2b3e04bdd0634eeca691fe17ce3686c2`，原始回收文件：
- `identity-readback/Evidence/identity.json`
- `identity-readback/Evidence/source.sha256`

已亲读identity.json头部：
- Native SHA256：`FBD35CDD609DE2D67EB494B41A07FDB9240DC955B3F54D681D48F2423C8A4959`
- 长度：4854272 bytes。
- PE machine：Amd64；PE optional header=true；CLR header=false；辅助进程X64。
- 远端路径：`C:\CodingAgentSandbox\Tasks\2b3e04bdd0634eeca691fe17ce3686c2\Native\wpfgfx_cor3.dll`。
- 辅助运行框架：.NET 10.0.1；不据此推断发布SDK版本。
- 源码清单505个文件，UTF8无BOM清单SHA256：`573E01CD88DE66AF509784B03D88D90CBA736D0D473C1B819F6C22BE7136469B`。

此为发布后所复制源文件的加密快照与新DLL PE/摘要，不是全repo、SDK、生成器、外部原生头文件及资源的完整可重现构建认证；不声称源码清单覆盖全部构建输入。构建期间未修改生产/harness，顺序可追溯；正式harness没有成功生成，因此没有可认证的harness二进制/MVID，也没有最终正式包Native SHA与aot.sha256匹配证据。沙盒工具原文给出stdout及回收路径，没有独立数值exit收据，不补造exit0。

## 正式COMAcceptance包实际阻塞

静态选择framework-dependent正式包分支（Release、win-x64、PublishAot=false、ManagedAcceptance=true、Sandbox=true），避免self-contained RuntimePackAsset带入runtime原版同名DLL；全新harness-build/harness-publish/clean目录，不复用旧self-contained。AotDllPath明确指向本轮native DLL；PackageComAcceptance的最终Native复制后GetFileHash SHA256与aot.sha256逻辑保留。

实际专用publish一次，**exit1**，完整12行工具日志已读：
`ComAcceptance.Package.targets(9,5): error : BlockedMissingTrustedOracle: ComAcceptanceReferenceInputDirectory`。

没有关闭ComAcceptanceRequireReferenceData，没有填假oracle，没有把candidate输出作为oracle。全工作区名称查找 `original-wpf.json` 与 `glyph-arial-original.json` 均无匹配；历史 `crossarch-fixture-prerequisites/original-clean-x86/ReferenceData`、`reference-glyph-trusted/readback-original-x86/ReferenceData` 两目录也实际不存在。不是推测依赖缺失，而是现有正式target确实拒绝打包。未加载原版来重新制作oracle。

生产Native发布和此失败完全分账：正式harness尚未成功编译/打包/身份冻结，不能称“包认证完成”。本轮不修改targets扩大或放宽门禁。下一需定位或由监督者提供已有可信只读reference输入（含来源/摘要），保持正式验证，不需重新请求用户执行授权。

## 公开WPF视频入口与原版隔离尚未闭合

已核对真实入口：`NativeRun.Main`首分支 `--pcore` → Pcore.Program.Run；纯candidate参数为 `--pcore candidate purevideo-public`，不需要也不调用PcoreComparisonTests/original分支。原有代码在RunWpf的NoInlining边界之前校验candidate PE/架构/aot.sha256，保留只读文件租约；在PresentationCore/WindowsBase上安装resolver；resolver按绝对selected加载candidate，不返回原版fallback。

但Program.cs的实际模块校验VerifyLoaded位于场景完成之后；当前“resolver-installed-before-WPF”Checkpoint只写JSON，**不是业务前排除宿主runtime原版加载的证明**。不能仅因有resolver就保证宿主没有其他加载路径。framework-dependent包本身不带原版也不能排除宿主runtime。需要后续验收层的业务前模块/加载路径失败关闭证据及有界监督设计，不能先运行业务后凭结尾模块枚举补认证。本轮没有为认证主动加载原版，没有启动真实WPF。

既有ExitDiagnostics的入口针对MTP过滤/full，不是任意pcore参数监督器；未猜测把pcore参数传给它。共享NativeRun.RoleAsync历史仍有Kill后无期限WaitForExitAsync风险；本轮未触碰共享回收语义。后续需先核定candidate-only角色的有界监督，外层沙盒timeout不等同业务清理通过。

PureVideoPublic既有业务合同已读：公开MediaPlayer/VideoDrawing、live visible HwndSource、CompositionTarget.Rendering，seek0.5/2.5后clock窗口、pause、CopyMedia(visual) consumer像素、红蓝dominance/alpha/stale-frame判定及Ended、Close/detach/unsubscribe/window-close证据。未改任何合同。它不是托管40帧/成功加载即可通过；本轮没有consumer显示、帧交付、像素、控制或生命周期运行证据，不能提升为Native媒体通过。认证拒绝负控同样NotRun；没有TRX/业务JSON/stdout/stderr/module SHA可报告。

## 精确执行账与实际修改

- 专用publish调用3：生产Native1成功；正式COMAcceptance1失败；只读身份辅助1成功。
- 独立run_build/run_msbuild/run_tests各0；publish隐式编译不重复算独立build。
- Windows沙盒1：只读身份辅助；Native加载/COM/公开WPF/原版/负控/full全部0。
- 终端0、脚本0、原版加载/副本/fallback0。

实际新增文件：
1. `Tests/SupervisedNativeIdentity/SupervisedNativeIdentity.csproj`
2. `Tests/SupervisedNativeIdentity/Program.cs`
3. 本报告。
4. `artifacts/supervised-native-video-x64-r1/execution-receipts.md` 及工具生成发布/回收产物。

实际修改：`Docs/next-session-handoff.md`仅顶部接续，旧原文保留。生产源码、正式harness源码、项目配置、oracle及测试标准均未修改。

## 下一门与独立审查建议

1. 独立核读新Native身份原件、发布路径和源码快照范围，不把它冒最终包认证。
2. 找到可信reference输入并保持原门禁重新正式打包；只推送明确清单来源，排除原版runtime DLL，认证最终Native/aot.sha256/harness MVID。
3. 先形成真实WPF业务前防原版加载证据及有界candidate-only监督；无法保证则失败关闭。
4. 执行真实candidate认证拒绝负控（不加载原版）和公开纯视频，亲读原stdout/stderr/JSON/TRX（如有）/模块路径SHA/consumer像素及生命周期。
5. 视频与身份/full分别报告；生产缺陷先留原始失败证据，不改生产追绿。x86/full继续Open。

本阶段可交监督者独立审查；未因旧预算或缺授权停工，未宣称视频或整体完成。
