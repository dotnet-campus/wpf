# 第三批 I：独立监督审查与后续验收准备

## 结论与权限边界

本报告为只读代码/证据审查，仅新增本文件；未改生产、项目、测试、主交接或既有报告，未执行构建、发布、测试、终端或脚本，未争用 G/H 输出。结论针对读取时工作区快照，不是 G/H 最后提交后的验收。正式范围仍为两个正式测试项目、Windows x86/x64 Release、纯 C#、官方独立零阈值；SHA 用于结果归属，不阻止继续建设测试。整体**未完成完整替换验收**。

权威：`Docs/next-session-handoff.md:9-29`、`Docs/handoffs/current-execution-rules.md:5-27`、`Docs/wpfgfx-complete-replacement-validation-plan.md:90-120`。已读当前任务、基线、完整方案、首包/布局/HW/集成报告、stage2 生命周期报告及 supervised capture/state 报告。已有状态文档描述的是较早包，不能盖过后续原始结果。未发现 AGENTS 文件；不据此声称不存在其他历史约束。

证据等级：下列“直接回读”指本次打开的原始文件；“报告引用”仅指已读报告的陈述，不当成本次独立重跑或逐字节重验。没有将局部通过数相加成全量。

## 1. 捕获：LastPresentTime=0 的裁决

### 已确认、可接受的修复

- `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.DesktopDuplication.cs:46-59,82-117`：内容选择为 `lastPresent > 0 && lastPresent >= submissionStart`。先创建 duplication、释放 initial 帧、记录 QPC、执行提交，再有界等待。Acquire 成功与桌面内容更新被区分，鼠标时间非零不能充当内容帧。拒绝零时间戳对本测试目标正确；不能反过来宣称零时间戳对应资源在任何上下文都不可用。
- 直接回读 `artifacts/sandbox-readback/replacement-supervised-capture/final-v2-x64/TestResults/capture-final-x64.trx:534-558`：initial lastPresent=0、lastMouse=32021971738424、accumulated=0；提交起点32021971739104，之后内容时间32021972111668，像素 FF123456；同一 CopyResource/Map/RowPitch 链得到正确颜色。支持“旧首帧误用”的因果解释，而非放宽颜色断言。
- 同 TRX:851 为42执行/42通过；`capture-isolated-x86/TestResults/capture-isolated-x86.trx:113` 为6/6。这是捕获/选定历史族，不是两个项目全量。第二个 swap 的数值及旧2失败来自 `Docs/completed-work/replacement-supervised-capture-diagnosis.md:21-31` 的原始证据索引，本次未再逐项打开旧包。
- `D3DImageTests.PresentContract.cs:29-43` 保留真实 D3D9 Clear/Present、backbuffer 内容检查和提交回调，没有用 GetPixel fallback 替代 duplication。`DesktopDuplication.cs:129-155` 按 RowPitch 读取并写原始 PPM。四项谓词用例覆盖零/旧/等于/晚于起点；未见新增 skip 或放宽阈值。

### 尚不足之处：阻塞完整时序/HW 结论，不阻塞保留本修复

1. 非零且晚于起点只证明桌面发生更新，不证明该更新来自目标窗口。当前在首个合格元数据帧立即 break（DesktopDuplication.cs:112），若其他窗口先更新、目标 Present 尚未在桌面可见，就可能读取旧目标内容并失败。它目前不吞内容错误，但可能假失败；同色旧帧也可能使后续颜色断言无法识别假新鲜。需红→蓝交替/单调帧标识、多次提交、外部无关更新与 pointer-only 插入用例，保留每次选择及内容，不按颜色重试追 Green。
2. 谓词用例未覆盖 Acquire→Release→再 Acquire 的完整状态机：需要 pointer-only→旧内容→本次内容、多次 WAIT_TIMEOUT→成功、ACCESS_LOST、最终超时和 Release 次数的纯 C# 可控负控。5000ms 是截止条件，不是性能门；单次500ms等待可稍超截止点，不声称精确5秒。
3. 直接日志表明 adapter=`Microsoft Basic Render Driver`，output DISPLAY1/identity rotation/同 monitor（上述 TRX:538-540）。这只是本次 duplication 设备，不是候选内部物理 GPU/backend。固定 adapter0/output0 加不匹配即失败是显式限制，不是软件 fallback；多屏/旋转未覆盖（capture 报告:73-87）。不得把42项Passed推广为物理 HW/shader/cache 全关闭。
4. 官方 geometry/sampler 的独立角色仅落身份，未完成 TRX（capture 报告:49-59,83-87）；普通官方实色 HWND 和系统 D3D9 control 不能替代这些同场景官方结果。

### 新确认测试清理缺陷：仅交监督者，不抢改

`DesktopDuplication.cs:161-165`：finally 中先 `Assert.AreEqual(0, ReleaseFrame())`，随后才 Release 所有 COM 指针。若 ReleaseFrame 返回失败，断言抛出，后面的 staging/texture/resource/duplication/context/device/output/adapter/factory 释放全部被跳过。性质：确定的异常路径清理缺陷，不是已证实候选生产泄漏。

最小复现：用纯 C# 可控 duplication 夹具使 Acquire 成功、最终 ReleaseFrame 返回 E_FAIL，并记录相关 Release 回调计数；当前路径断言抛出后计数不会收敛。建议所有权负责者使释放在嵌套 finally 中必达，同时保留失败，不吞 HRESULT。本轮没有执行此注入；这是由控制流可直接证明的缺陷。它应在资源验收前补回归，不单独扩大成任意环境 HRESULT 等价门。

## 2. ABI / Realizer 生产变更与夹具

- `Code/WpfGfxShape/Abi/NativeBrushRealizerReader.cs:5-19`：slots6/10/11 使用 thiscall；Ensure 和 Effects 的失败原样返回，未 catch、未改成成功。`BitmapRenderTargetExports.Drawing.cs:266-284` 在失败时立即返回；brush=0 是合法无内容分支，不能据该分支单独判断空 consumer。
- `NativeImmediateSolidRealization.cs:168-182,230-273`：成员槽采用 thiscall，而引用槽保留 stdcall。NoTiling/NoClip/NoFixups/GetEffects(null effect) 的空操作符合“立即、无 tiling/效果”的有限对象形态，不能仅因空体就定为掩盖；也不能用它证明一般 bitmap/effect 语义已实现。Ensure 对非0/Max cache 返回 NotImplemented 而非假成功。内嵌 brush 的借用引用说明在269附近；需要真实 owner 释放及外部持有路径验证，不能以注释代生命周期证据。
- 直接回读 `artifacts/supervised-realizer-fix/readback/x64-final/TestResults/final-x64.trx:8-29,74`：5/5，仅 Ensure failure、Effects failure/recovery（各官方/候选）及身份。候选实际 FDF0A6…，不是旧2BFC4…；`Results/independent-realizer-True-925ad48b2e1e48b4a67f278fae27844d.json:1-34` 明确16次 E_ACCESSDENIED、Calls16、ContextMatches=true，PixelsBgra 是 base64，不是丢失像素。候选源码 ProductVersion 相同不能证明产物未变。
- `IndependentRealizerAbiTests.cs:21-36,77-111` 独立声明、实际加载官方/候选并重复调用，检查 HRESULT、次数、context、保留绿色内容；Adapter/Display只记录未断言，brushContext/creator没有区分值断言。`RealizerMemberRecoveryRegressionTests.cs:90-136` 验证8次失败后8次恢复、slots6/10/11计数及绿色保留；Scope诚实声明null brush，不证明成功非空绘制或device恢复。
- `IndependentBitmapRealizerAbiTests.cs:78-135` 有非空红色及16次回调断言，catch NullReferenceException 写诊断后 rethrow，不是吞错。但读取快照仍有matrix=80/texture=208固定布局、brush table缺少多数槽，官方/候选成功证据尚不能从前述5/5借来。`artifacts/supervised-abi-content-f/readback/x86-bitmap{,2}` 未见最终TRX；content/Results/content-x86.trx:76 的5/5是另一内容筛选。H正在修夹具，不把中途失败归生产、不抢改。
- 风险：非空bitmap、非空effects、transparent=true、区分参数、清理slot计数、真实owner引用、成功绘制后更新与关闭均需独立官方用例；独立结构自洽不替代实际调用。thiscall修复不能外推其他brush函数槽（Drawing.cs:277为默认unmanaged调用），应依据原生合同逐槽核查而非全局替换。

## 3. 窗口/0x05生产路径：未见隐藏失败，尚不能验收

- `Code/WpfGfxShape/Core/GeneratedVisualTargetResources.cs:272-305` 对0x33先验证类型/72字节、layer/transparency；在HWND尚未建立时接受合法尺寸状态，已有HWND才创建新target，创建失败原样返回。cookie不匹配的return0为状态过滤，应补官方乱序cookie合同，不能仅凭return0定吞错。
- `SameThreadChannel.cs:443-447` 实际处理0x05，不是空consumer。快照 `SameThreadPartition.Compose:19-40` 有 DwmFlush，失败记录并固化 Failure；SetVBlankSyncMode:47-67 有计时精度注册、订阅集及0x09状态消息。未见 blanket catch、修改绘图阈值或软件 fallback。但全partition DwmFlush时序、重复订阅、最后关闭timeEndPeriod、失败后能否恢复须用官方/真实窗口验证；该快照属于G进行中，不能签署最终行为。
- `Tests/WpfGfxShape.Tests/WindowSettingsBeforeInitializationTests.cs:9-73` 有enable先于HWND、重复enable、双订阅关闭、截断及0x33先于初始化；首项只peek一条消息，未验证第二条状态/禁用/重新enable、DwmFlush触发和回调偏序。
- 原始 `artifacts/replacement-window-e-2026/readback-acceptance-x64/TestResults/window-e-x64.trx:16-24,89` 为10执行9通过1失败，窗口候选仍退出-1073740771。首批 `replacement-state-a-win-x86-final/TestResults/replacement-state-a-x86.trx:64` 为3执行1通过2失败。它们是旧窗口失败证据，不能归属G新代码；G需以新原始结果关闭，不能仅引用单元通过。

## 4. 测试有效性、独立性与资源边界

- `ReplacementStateSequenceWindowTests.cs:23-44` 官方/候选由同exe隔离角色分别运行、12份原始帧逐字节比较，失败保留；49-56有空/旧/空数组负控。190-244逐像素校验实际红蓝绿黄、窗口实际尺寸、Rendering>0、Closed恰一次及注销后静默。使用RTB消费Image，确为WPF producer/consumer，但**不是屏幕HW呈现**，不能借窗口可见或Tier认定后台。证据边界也见 supervised-state 报告:25-31。
- 窗口Pump是150ms调度窗口，60秒STA join失败会使隔离角色返回124；NativeRun.cs:29-44外层三分钟超时kill并await，不是无限等待。单次静默窗口不证明所有在途callback都已收敛；补正在执行回调与关闭重入屏障。
- `Pcore/LifecycleSequenceScenes.cs:12-29,35-75` 为固定seed尺寸、8轮80帧、12弱引用对象、全像素非空/新帧断言；每帧新RTB，不是同一个HWND target resize，更不是真实device-loss。
- `Pcore/LifecycleSequenceTests.cs:10-31,35-64` 比较80文件与事件，负控有缺close、frame先于update。但其 `IsFresh` 只在负控方法使用，生产场景实际以另一个SequenceEqual检查；因此“旧帧负控”仅验证独立辅助谓词，不证明真实场景检测器被注入验证。应将负控接到同一检测链。缺重复callback/遗漏release/超时/恢复未发生的负控。
- stage2报告:27-55称双架构COM各98、managed各8及原始资源路径；本次这些回收目录未在当前sandbox-readback列表出现，故只作为报告引用，不算本次独立回读验证。弱引用alive0仅证明所追踪托管对象不可达，不证明GPU、COM引用、native队列、线程无增长。句柄/线程/private bytes样本尚需预热后趋势、官方对照、关闭后稳定期。

## 5. 完整方案八步证据状态

对应完整方案:90-99，不按通过率关门：

|步|本次可支持的实际进展|尚缺/验收状态|
|---|---|---|
|1 盘点|入口/规则、项目清单及首包五维表已存在|关键路径×双架构×五维逐项原始证据矩阵不完整；部分完成|
|2 当前完整基线|首包报告COM x64 1675/1677、集成双架构52/52；不是新生产全量|最新源码四格full无证据，managed旧4228缺回收TRX；未完成|
|3 历史32HW|直接回读capture x64 42/42、x866/6；零帧根因已局部关闭|官方geometry/sampler未完成、内部backend缺、x86历史组中断；部分完成|
|4 C# ABI|context布局修复/52回归（集成报告:5-43）；直接回读Realizer x64五项及原始JSON|H成功bitmap夹具、双架构全部相关槽/COM/callback/refs仍缺；部分完成|
|5 组合/WPF|RTB80帧报告；窗口官方12帧及候选失败保留；非空负控|真实窗口候选最新闭环、格式组合、1字节差异最小化、所有producer/consumer闭包未完成|
|6 时序资源恢复|callback单测/stage2有限循环、窗口新增关闭断言|真实loss→恢复新帧、连续HWND resize、持续媒体、native/GPU收敛缺；未完整覆盖|
|7 随机缩减负控|固定seed尺寸和少量内容/事件谓词负控|非合法图/合法操作序列随机差分；无缩减器、缺资源/重复回调负控；未完成|
|8 最终交付|有分包局部证据，身份各自归属|G/H稳定后四格完整适用矩阵与业务证据回读尚无；未完成|

首包/集成/布局/HW报告出处：`Docs/completed-work/replacement-{package1-initial-baseline-blocked,integration-build-package,x86-layout-content-package,x64-hw-capture-package}.md`；首包原始TRX12457行等仅按报告索引，本轮不冒认重新回读。旧磁盘阻塞曾被集成发布解决，不当成当前仍阻塞的事实。

## 6. 下一批可执行验收清单与输出配置

### A. 先完成有界闭环（G/H负责人，不与其并行构建）

1. 等G/H明确最后写入文件及构建结束；串行取得两架构0x05/0x33→Show→首帧→更新→resize→关闭结果。保留旧窗口失败，确认新包官方成功和候选新帧，而不是只消除退出码。
2. H先单独官方bitmap成功角色、后候选，持久化每次槽调用/参数/返回/原始像素，避免全量被危险夹具中断。确认合法原生布局而非猜偏移；随后重复、失败恢复、非空effects、资源释放回归。未完成不skip进全量制造Green。
3. 修清理缺陷并将捕获负控接到真实帧选择状态机；官方geometry/sampler独立完成后才跑历史组。可继续建设测试，不因SHA变化停工。

### B. 补测清单（全部在现有两个项目，纯C#）

- 约束随机：合法geometry/stroke/transform/clip/mask/bitmap/glyph/shader图及create/update/draw/resize/close序列；固定seed、完整参数、逐节点/帧原始内容，先官方对官方稳定性，再官方对候选零阈值。失败先保存后缩减节点/路径段/数值/操作前缀，生成确定回归，禁止随机追Green。
- 负控：颜色单字节篡改、两边同时空白/相同旧帧、外部桌面更新、重复/遗漏/关闭后callback、漏release、未发生loss却声称恢复。每项通过同一个实际比较器/轨迹检查器，不能用独立未接入的辅助方法代替。
- 真实恢复：可证明原device确实lost的触发/状态记录，保留旧surface/cache generation，恢复后不同内容、多次resize/再次loss、关闭最终引用。独占探针未触发loss应记未覆盖，不因环境HRESULT不设门就排除真实恢复。
- 持续媒体：真实解码producer→消费新帧标识，播放/更新/重绑/注销/回调内关闭和多实例竞争；必要偏序、次数/重复/关闭后静默、完成有界。现有MediaExports Passed不替代持续路径官方差分。
- 资源：先预热再循环，保存每次COM/surface/device/cache/callback/队列/线程/句柄/managed/private/GPU可观测样本及官方对照，验证关闭后稳定区间；不以单次内存阈值或托管weak ref替代资源合同。

### C. 最终四格发布/运行配置（准备，未执行）

生产/项目配置依据 `Directory.Build.props:3-10`、COM csproj:3-35、`ComAcceptance.Package.targets:29-39`、managed csproj:3-18。仅PublishDir分开**仍共享obj/bin**，因此G/H退出后四包串行发布；不改共享props、不清理共享输出。

固定一次唯一验收根，例如 `artifacts/replacement-final-i-next/`，每格独立 `publish/{com,managed}/{x86,x64}`、COM `package/{x86,x64}`、`readback/{com,managed}/{x86,x64}`。COM打包target会RemoveDir指定包目录，必须专有，严禁默认com-acceptance-package或G/H目录。所用路径传工作区绝对路径。

专用发布接口参数配方（不是shell/脚本；由下一批将单行完整发布参数交给专用接口）：

- COM：目标 `Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`，Release，RID分别win-x86/win-x64，SelfContained=true，RuntimeFrameworkVersion=10.0.9（仅复现当前已测环境，不声明固定支持合同），ComAcceptanceSandbox=true，AotRuntimeIdentifier对应RID，AotDllPath显式指定最终生产DLL，PublishDir和ComAcceptancePackageDirectory使用上述独有绝对路径。可沿用DebugType=None/DebugSymbols=false/ProduceReferenceAssembly=false，不关分析器。
- managed：目标 `Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj`，Release，对应RID，SelfContained=true，EnableMSTestRunner=true，RuntimeFrameworkVersion同包，PublishDir独有；不增加COM项目引用。
- 生产只有G/H需要的最终实现发布，不因审查重发。记录实际候选DLL、测试程序集及官方实际路径/版本/SHA/指针宽度；不同架构各自归属。

运行使用专用Windows沙箱执行接口：sourceDirectory分别为独立COM package或managed publish；executableRelativePath分别为 `WpfGfxShape.ComAcceptance.exe` / `WpfGfxShape.Tests.exe`；全量不传filter；timeoutSeconds=1800；outputRelativePath留空以回收整个任务；localOutputDirectory按上述四格分开。先有界预检，再串行COM x86、COM x64、managed x86、managed x64；中断保留所有证据，不补造总数。

**TRX路径硬前置条件，当前不能直接填相对路径开跑：** `AcceptancePaths.cs:12-16`明确MTP在模块初始化之前按宿主工作目录解析CLI路径；managed无相同重定向。首包报告:22证实TRX曾落 `C:/lindexi/WinRemoteShell/Results`。沙箱执行接口不提供工作目录或任务路径替换占位符；不得假设 `--results-directory TestResults` 就在回收根，也不得传 `%变量%` 指望shell展开。

因此下一批先在**现有正式exe**中实现/复用纯C#隔离运行入口：在创建TestApplication/子进程启动前，以 `Path.Combine(AppContext.BaseDirectory,"TestResults")` 构造绝对结果目录，给MTP参数 `--results-directory <该绝对目录> --report-trx --report-trx-filename final-<project>-<arch>.trx`；managed同样在正式项目内处理，不新增第三Host。若现有入口尚未实现此能力，该项是执行准备阻塞，不宣称已有可直接开跑的绝对路径。先用一个身份/负控测试验证TRX确实在回收根、业务Results也在包内，回读通过后再去掉filter全量。不要先跑full再找丢失TRX。

每格完成后回读：TRX TestRun/逐项失败与Counters（执行/通过/失败/未执行/timeout/aborted），实际identity、官方角色模块、原始内容/新帧、backend、事件及资源样本；child结果不重复累加。四格全部跑完也不自动关闭缺失的真实HW/loss/随机等方案项；最后由监督者更新主交接。

## 7. 阻塞与非阻塞最终裁决

- 阻塞完整替换：最新双架构窗口闭环与ABI成功夹具未验收、四格full缺失、官方同场景HW/backend缺、真实loss/持续媒体/资源收敛与随机缩减未闭、未解释内容差异仍需定位。依据上文具体源码和原始结果，不凭通过率判定。
- 确认但本轮不抢改：捕获finally的断言中断释放；窗口旧包候选失败仍留证。后者根因与G最终修复不能由退出码猜定。
- 疑点非已确认生产缺陷：bitmap固定布局/缺槽、无关内容帧被提前接受、cookie过滤时序、borrowed引用、DwmFlush分区行为。给对应负责人最小场景，不直接改。
- 非单独阻塞：SHA不同、base64像素表示、官方版本仅验证10.0.9、未直接观测ESP、未跑Debug/原生探针/性能/额外hardening。SHA不阻断测试建设，但不同结果不得拼成同产物full。
