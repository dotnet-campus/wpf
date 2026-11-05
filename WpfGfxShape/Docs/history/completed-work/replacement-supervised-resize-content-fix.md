# KF：候选 resize 后内容差异定位

## KK 终态：官方公开同步生命周期保持存活，与 KJ 候选观测不同；生产未修

### 官方入口与资源合同
已读执行规则、完整方案1–120、同KF报告全部有效行及KJ夹具全文，核对DR独立Create/callback ABI与官方源码。`apifunc.cpp:186–203` 的 WgxConnection_Create(true)选择SameThread；`samethreadcomposition.cpp:100–138`只立即播放批次，ScheduleCompositionPass无操作。合法公开 `WgxConnection_SameThreadPresent`（apifunc.cpp:129–139）→connectioncontext.cpp:460–528 Compose，只渲染而不调用target Present；不是InteropDeviceBitmap Present，不发Event0。`composition.cpp:1800–1838`要求resource46/TYPE_HWNDRENDERTARGET并注册manager；`hwndtarget.cpp:398–420` ProcessCreate设置HWND、DPI及flags，实际backend在Render:88→EnsureRenderTargetInternal:846–906→CreateDesktopRenderTarget初始化。UpdateWindowSettings:511–651更新windowRect及flags，下一compose经UpdateWindowSettingsInternal:941 SetPosition落实尺寸。

沿用现有KJ夹具，增加全文件KK_RESIZE_LIFETIME分支/独立公开方法，只加载AcceptancePaths.OriginalDll；不复用候选私有状态作oracle。92字节create/72字节settings按官方generated wgx_commands.cs布局；flags12=立即呈现4|保留内容8、DPI awareness -1/unaware、scale1/1，masterDevice/hSection/hBitmap/stride/pixelFormat为0，符合exports.cs:2180–2260默认HW初始化。同步connect1，每次close/commit/flush后调用SameThreadPresent。真实HAL producer adapter0、32×16，12×1 A8R8G8B8 lockable RT surface、fallback0/version701、独立stdcall callback、snapshot及配对AddRef/Release保持KJ输入；真正SetWindowPos64×24及settings。无桌面fresh、无D3DImage Present、无shader消费、无snapshot像素读取。

注意比较边界：KJ旧角色connect0、flags0、DPI零，KK按官方合同改为同步connect1/flags12/DPI1并增加compose。因此是相同producer/尺寸/公开生命周期语义的官方独立控制，不是逐字节/传输完全一致的双边单因子差分。target实际backend/HW设备或adapter LUID未公开读回；producer明确adapter0，窗口沿用同位置，但官方target实际adapter匹配仅创建选择合同支持，不能冒称独立实测。观察到无callback不能推定官方曾发available=true，也不能据此断言其HW device已经创建；SameThreadPresent成功及源码调用链不是HW backend独立证据。

### 唯一执行、身份与原件
唯一x86 task **fbd2a0eaa7ce4521989887df58893221**，2026-10-26 08:03:14.954–08:03:16.516 UTC，监督PID2368/child6560。filter精确WhenKkOfficialTargetSettingsAreSubmittedThenProducerLifetimeIsObservable，false/true两独立DataRow各自initialize/channel/producer/cleanup。TRX2执行/2Passed/失败skip0、业务Exit0、Reason exited，BLfiltered120/外180；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut/OutputDrainCancellationTimedOut均false，HasExited/OutputCollected true、SupervisorError null。工具返回完成不等同提供数值工具exit；实际业务Exit单独取监督JSON。
绝对TRX `C:\CodingAgentSandbox\Tasks\fbd2a0eaa7ce4521989887df58893221\Results\kk-official-resize-x86\TestResults\child.trx`；回收根 `artifacts/kk-official-resize/readback`。实际官方模块task根`wpfgfx_cor3.dll`，SHA **5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834**，10.0.13-servicing.26504.104/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；实际testSHA **CCE8D4CF61EF81574720BF8007A906DD48898EA95067EC711D449E895C01C629**、MVID **8c8511b2-21e3-45de-95e3-c50d6bad7a42**。PresentationCore SHA D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D，同10.0.13 servicing；runtime.NET10.0.13/X86。根官方来自自包含WindowsDesktop runtime，与Native子目录候选明确分开。候选625DBB10879CFDB77CAA91F9A10F37B242247026D1B2930E5532031C53A63D7F只复用为包依赖，不是actual official。

### 亲读生命周期对比
|观测|KK官方baseline|KK官方resize|KJ既有candidate resize|
|---|---|---|---|
|surface caller/Create后/设置后refs|1/2/2|1/2/2|1/2/1|
|snapshot前/后HRESULT及payload|0/non-null→0/non-null|0/non-null→0/non-null|0/non-null→80004005/null|
|producer cooperative|0→0|0→0|0→0|
|设置阶段原始callback payload/次数|[]/0|[]/0|1/701→0/701，共2|
|cleanup target callback|[]/0|[]/0|再0/701|
|Detach后bitmap/surface/device/factory Release|0/0/0/0|0/0/0/0|0/0/0/0|

官方所有initialize/connect/channel/resource/send49及51/close/commit/flush/compose、producer创建/fill、InteropCreate、snapshot、cooperative及destroy/disconnect/deinitialize HRESULT均0。前后临时snapshot COM各Release0，DestroyWindow成功，两个cleanup complete。真实resize同bitmap202943904、surface63270240、producer201930528、HWND21038098；baseline bitmap201650736/surface63269760，各自产生独立对象。available []保留原始空数组，不伪造false/true。已亲读两identity、两lifetime完整各315行、supervision40行、TRX结果及末Counters、stdout/stderr、stages核心/末尾；stages沿用KJ前缀属标签，不是候选模块身份。无像素/consumer接受结论。两个Passed是观测自洽及清理通过，不是产品resize通过。

### 合同责任、最小后续门
官方`hwhwndrt.cpp:211–226` Resize调用UpdateFlippingChain；239–306明文用secondary chain以避免重建设备，仅释放target surface/swapchain再CreateAdditionalSwapChain。官方manager FindDeviceMatch:762–824按adapter/type/behavior共享AddRef，UnusedNotification:1093–1158只有ref0才Notify并删除，1846–1870明确正常销毁亦须通知。InteropDeviceBitmap:354–371以producer adapter注册listener；514–559收到invalid释放非fallback surface并disable。故不能删Notify，也不能在失效后偷改Disabled回false冒恢复。
候选GeneratedVisualTargetResources:295–300尺寸变化创建整个新target再Release旧；HardwareRenderTargetExports:97–105每target新Objects/Manager/Device，75–82销毁该整套backend，与官方原地换swapchain责任不同。结合KJ真实old Dispose→adapterinvalid→同bitmap丢surface，最小修复调查点是**保留target/device/manager，在既有Renderer/backend上resize交换尺寸相关资源**，而非抑制合法销毁通知；若必须跨target换device则须实现匹配参数共享拥有关系及正确listener作用域。该方向有独立源码与差异支持，但尚非二进制HW单因果证明。
下一唯一状态恢复门：先取得同合法flags/DPI/同步初始化的候选与官方target实际backend/adapter匹配证据，再在原地resize实现授权后验证同bitmap/surface保持、真实shader输入恢复及cleanup；不直接重跑旧红帧追绿。官方本次也未物理device-loss，不能关闭loss门。本轮原则生产0改，不提交未经验收default补丁。

### 构建与隔离、历史保留
独有`artifacts/kk-official-resize/{obj,bin,publish/acceptance,com-acceptance-package,readback}`，正常依赖图、显式绝对PublishDir，正常分析器无NoWarn/关闭；正式x86 Release selfcontained.NET/WindowsDesktop10.0.13。COM发布失败1（CS1686 connection被局部函数捕获后取地址），以未捕获createdConnection修正后成功1；无生产发布、无任务重试。现有PackageSelfContained目标清理正确com-acceptance-package并白名单复制，manifest418行首尾、Contracts/Native/root官方/runtimeconfig/aot.sha256已核。只上传clean package，不上传PublishDir/回收根/历史父目录，不另造Host/工程或脚本。
最后normal无KK/KJ符号solution Release build Exit0/131warnings/0errors/1.76秒，默认公开API无KK/KJ方法；源码最后修改后完成该build，未把build当运行。改动仅KJ专属条件分支、同KF报告及handoff/session-summary/issues三共享最新KK节；生产Notify/resize、DRDS、NativeRun/ExitDiagnostics、VisualCache/historical、公共捕获、项目打包、CF/CX/CY0改。旧full1814/40与1814/4身份保持，Available/真实HWbackend/loss/mediaresources/final full四格开放，完整替换未完成。

KK收尾登记：报告KK全文与三共享最新受改节写后回读；保留以下KJ/KI/KH/KF历史，不追加运行或把本次2项并入旧full。

## KJ 终态：真实非显示生命周期控制成立，原 shader 差异不关闭

### 可行性与合同审计
先登记 handoff KJ进行中，再读执行规则、完整方案1–120、KF/KH/KI全文、JF/JG合法四格和当前target/device/interop合同。原版manager FindDeviceMatch:762–824按adapter/type/behavior共享并AddRef，UnusedNotification:1093–1158仅ref0才销毁并Notify；1846–1870明定正常销毁也通知。原版InteropDeviceBitmap:514–559收到同adapter invalid亦释放非fallback用户surface/全部HW realizations、disable。故正常销毁Notify合法，不能删除；候选每target独立Objects/Manager/Device配合静态interop listeners的广播范围仍是待验证兼容假设，不拿候选内部实现作官方oracle。

无需desktopfresh即可判别surface所有权后果。新增唯一文件 `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.KjResizeLifetime.cs`，全文件 `KJ_RESIZE_LIFETIME` 条件，仅两个DataRow false/true，串行同正式exe显式filter。真实HAL producer + 系统CreateRenderTarget(12×1、A8R8G8B8、lockable)，合法Interop_Create fallback0，调用方一直保留surface/device；生产resource46 HwndTargetCreate32×16→实际UpdateWindowSettings32×16或SetWindowPos64×24及UpdateWindowSettings64×24→旧target正常销毁。不发Present、不接shader、不使用producer backbuffer冒consumer像素。公开决定性观测为version701 available callback、成对AddRef/Release surface引用、producer TestCooperativeLevel和GetAsSoftwareBitmap返回状态；snapshot只作接口可用性，未读其像素。候选IUnknown仅3槽且只支持IUnknown，未调用不存在的GetResolution槽。front本次始终0，不能证明既存front被清除。

### 唯一真实任务与身份
唯一task `817be18724c140ef97ae1f884421c83c`，监督PID8692/业务11168，2026-10-26 07:51:18.939–07:51:21.272 UTC。精确filter `FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenKjTargetSettingsAreSubmittedThenProducerLifetimeIsObservable`；TRX2执行/2通过/0失败/0skip，业务Exit0。两Passed只说明观测自洽及清理通过，不是resize兼容已通过。BL120/外180，Reason exited、HasExited/OutputCollected true；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut/OutputDrainCancellationTimedOut false、SupervisorError null。工具只报告完成，不造数值工具Exit。

绝对TRX `C:/CodingAgentSandbox/Tasks/817be18724c140ef97ae1f884421c83c/Results/kj-resize-x86/TestResults/child.trx`，本地 `artifacts/kj-resize-lifetime/readback/Results/kj-resize-x86/`。actual Native `C:/CodingAgentSandbox/Tasks/817be18724c140ef97ae1f884421c83c/Native/wpfgfx_cor3.dll` SHA **625DBB10879CFDB77CAA91F9A10F37B242247026D1B2930E5532031C53A63D7F**；actualtestSHA **71A52DA56E99AF7322E6D29553F1D7851FAEF014A1147C8611016B3CF93021A0**、MVID **674465a5-4619-44a9-a4fd-67e0c0f4a5af**，均版本1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a。runtime.NET10.0.13/X86/PointerSize4；实际PresentationCore来自task根，SHA D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D，10.0.13-servicing.26504.104+4099f83977f4d29604a9aeb971fb0a7bc1d5f699。root官方来自自包含WindowsDesktop runtime pack，未在KJ加载作对照，未新测其actualSHA；JF/JG官方5EBD18…仅历史身份，不能充本轮oracle。官方无显示target生命周期入口尚未核验执行，本组不冒官方差分。

### 核心生命周期实证
|观测|未resize|真实resize32×16→64×24|
|---|---|---|
|surface caller/Create后/设置后引用|1/2/2|1/2/1|
|GetAsSoftwareBitmap设置前/后|0+nonnull / 0+nonnull|0+nonnull / E_FAIL(80004005)+null|
|producer cooperative前/后|0/0|0/0|
|设置阶段available/version|无|1/701→0/701|
|cleanup目标销毁回调|0/701|再0/701|
|Detach后bitmap/surface/device/factory最终Release|0/0/0/0|0/0/0/0|

实际resize bitmap=B37BAE8、surface=4623B80、producer=189469056(ledger十进制)、targetHandle1、HWND47711956。KI辅助trace9–25：old target COM B4EF3D0/managed53225000/device15985875/manager59452375，new B37AC78/16852709/device28646684/manager2709966；new manager先adapter0 validTrue，再交换target，old Dispose通知adapter0 false；同bitmap由surface4623B80/DisabledFalse转surface0/DisabledTrue，front/hardwareFront均0。bitmap和surface地址与公开ledger对应；managed hash只辅助关联，不声称全局唯一。新device建立完成后的正常旧device销毁确实导致仍由调用方拥有且cooperative0的producer bitmap释放自身surface引用并不可快照。已超越KH双managed标志控制，但尚未证明原shader在相同状态/身份上因此透明，亦未证明官方同场景应该不通知。

亲读两lifetime完整285/297行、两关键identity、监督40行、TRX结果及Counters、trace30条关键链及cleanup、stages核心及结尾、空stderr。destroy channel/disconnect/deinit均0，接着Detach，四对象最终Release0，DestroyWindow成功，两个cleanup complete；snapshot临时COM均释放0。原始核心并非只读Passed。未声称长期无泄漏、真实HW loss或front恢复通过。

### 构建、隔离与默认安全
独有 `artifacts/kj-resize-lifetime/{obj/production,obj/acceptance,bin,publish/aot,publish/acceptance,com-acceptance-package,readback}`，两PublishDir显式绝对；production正常分析器KI_RESIZE_TRACE NativeAOT新发布1次，实际SHA不同KI BAD5…，未旧包假新。COM正常分析器x86 Release自包含10.0.13发布共4次：前三次Exit1（CS1686输出变量被局部函数捕获；中间仅调整日志可见性，ErrorsOnly只过滤控制台不关闭分析器/NoWarn），用不捕获临时输出修正后第4次成功。前两失败日志工具截断隐藏error，未称全日志审阅。生产发布成功1、COM成功1/失败3，沙箱task1；无显示/full/原方法复跑。

沿用现行PackageSelfContained目标先RemoveDir再白名单复制，418行manifest首尾、aot.sha256、runtimeconfig两framework10.0.13亲读；Contracts/Native/root官方/runtime齐，未另加ReferenceData输入。只上传clean package，readback不回灌源；无清理历史或扩大scope，包总字节/剩余容量未知。最后normal无KJ/KI符号solution Release build Exit0，131warnings/0errors/2.01秒；离线读取默认D3DImageTests公开API确认无KJ方法。生产0改、入口/项目打包/VisualCache/historical/公共捕获/DRDS/NativeRun/ExitDiagnostics/CF CX CY0改，现有修复保留。专属角色是观测实验而非默认产品回归，未验收代码不进入default发现。

### 裁决与下一唯一动作
KJ控制可行且已取得真实公开生命周期反例：正常producer未lost却受实际target替换的adapter invalid影响而丢bitmap持有surface。仍不建议生产修复：正常销毁Notify本来合法；缺的最小可控依赖是**同版官方模块无显示HwndTargetCreate→UpdateWindowSettings同场景的独立公开生命周期对照入口验证**，确认官方resize是否复用仍有引用的device并保持同一bitmap可用。下一唯一有信息动作是离线核准该官方resource协议/初始化与同步合同，再另行授权一次官方同场景非显示生命周期角色；不是重跑原红帧、不是再造COM变体。若官方入口无法无consumer安全走通，需提供该入口所需的官方partition/display初始化依赖，不能用候选trace替代官方oracle。

原KI task47d17…native首帧失败仍保留，未再次运行；旧full1814/40与1814/4原身份不改，官方首shader、官方samplerAV、真实loss独立开放，Available/HWbackend/mediaresources/final full四格未闭，完整替换未完成。KJ新增证据仅本组；KF/KH/KI及JF/JG历史保留，不冒全历史原始证据逐字审核。

KJ最后补记：本节全文及handoff/session-summary/issues最新KJ受改节写后回读登记；未追加沙箱任务，未把新2条计入旧full。

## KI 终态：原候选 x86 唯一真实 task，原生前置失败早停

### 执行与身份
离线亲读原 `D3DImageTests.HardwareClosure.cs` 精确方法及 VisualCache 原生前置/cleanup，未改原方法、expected、参数、共享夹具或 cleanup。filter 为 `FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive`，无尾括号。正常分析器 NativeAOT x86 Release 专属 `DefineConstants=KI_RESIZE_TRACE` 发布成功；默认 COM 项目自包含 .NET/WindowsDesktop 10.0.13 发布成功。独有 `artifacts/ki-resize-trace/obj/{production,acceptance}`、bin、publish/{aot,acceptance}，两次均显式绝对 PublishDir。未改项目/打包 targets；沿用正常依赖图及 correctclean 自包含包目标，上传目录仅 `artifacts/ki-resize-trace/com-acceptance-package`（Contracts/Native/runtime），回收独立 readback，不回灌源包。未加入额外 ReferenceData，未走 full 或 managedprobe。

唯一 task：`47d17e80b1684f998423f94d76bd27db`，BL filtered=120 秒，外层=180 秒，parent PID6764/child10632。监督 Reason=exited、ExitCode=2，TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut/OutputDrainCancellationTimedOut 全 false，OutputCollected=true、SupervisorError=null。runtime .NET 10.0.13、X86/PointerSize4。

actual Native：`C:\CodingAgentSandbox\Tasks\47d17e80b1684f998423f94d76bd27db\Native\wpfgfx_cor3.dll`，SHA256 `BAD5BA034FB195B6136A7B82A15950510CA584C9B403DA0CF9207FE1A0CBBE84`，FileVersion1.0.0.0、ProductVersion1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a。testSHA `47838C45B8FD3255DFCE89183A2B0F70439FB4F0A84C1D2F8A85C4700546D061`，MVID `55ac1a23-7efe-4dba-b209-6e86df5724db`。

远端绝对 TRX：`C:\CodingAgentSandbox\Tasks\47d17e80b1684f998423f94d76bd27db\Results\ki-resize-x86\TestResults\child.trx`。本地根 `artifacts/ki-resize-trace/readback`，实际亲读该 TRX、Results/aot-identity.json、hw-module-Native-c9e16f28d1574f5999589d53849a073c.json、Results/ki-resize-x86/{supervision.json,stdout.txt,stderr.txt,stages.jsonl}。TRX 原 testID `1eded079-5019-809c-bf7c-07ef4caf0e29`，1执行/0通过/1失败/0skip，不是零测试。

### 最早失败及因果边界
真实 nativeControl=d3d9.dll、Ex=false、cooperative=0，32x16，window visible1/iconic0、Default desktop、session1、cover为本进程本窗口；像素 actual `001E1D1C` / expected `00563412` 在 AssertCombinedPixel 失败。阶段日志到 `DiagnoseNativePresent after; AssertCombinedPixel before` 后进入 cleanup，未 connect/resource/shader，更未 resize。没有 resize 前 FF80 或 resize 后 FF00 的本轮数据；旧 ID 的 FF80→64x24→FF00仍只是历史。此失败不是专用工具错误，而是原生前置内容断言真实失败，依要求停止，无二次沙箱/变参追绿。

本次没有 ki-resize 生命周期记录，因为生产 target/device/bitmap 目标阶段未进入，而非已捕获空链证明没问题。不能判正常 old-device invalid 是否命中仍需消费的 bitmap，不能判新 front 是否重建，不能判 shader implicit 输入透明；也不能用日志时间顺序当因果。保留 KH 原版正常 Dispose 通知合同，不删 NotifyDeviceLost。缺陷分支仍开放，不提出未经实证的生产行为修复。

亲读全部21条 stages：destroy channel、disconnect、两 bitmap/surface、device、windows 清理顺序结束 `visual cleanup complete: `；没有错误后缀。stderr 空、进程自然退出。此为本轮失败路径清理证据，不外推未进入的生产对象资源收敛。

### 变更与默认验证
生产行为修复0。新增 `Core/KiResizeTrace.cs`：复用现有 FileStream/Utf8JsonWriter 逐字段安全写法，不反射 JSON，最多512条，每进程独立 jsonl，I/O异常仅 Debug 记录；Conditional KI_RESIZE_TRACE 默认无调用/参数计算。条件诊断仅 `GeneratedVisualTargetResources` resize 创建/交换/旧 release；`HardwareRenderTargetExports` 创建/硬件就绪/旧 backend dispose；`Direct3D9DeviceManager` 正常 dispose 与 adapter 通知；`InteropDeviceBitmapExports.ApplyAdapterStatus` 前后 front/HardwareFront/surface/Disabled/adapter/LUID/version/frontVersion。记录 COM 地址与 managed关联标识（RuntimeHelpers hash 仅诊断关联，不声称数学唯一）、policy generation及已有 HRESULT，不改输入/线程调度/输出位置/ref。

最终无 KI DefineConstants 的 solution normal Release build 成功，3675 warnings/0errors、9.65秒，正常分析器/未压警告。不把 build 当 x86 内容验收。原断言、COM/fill/partition/DRDS/NativeRunExitDiagnostics、VisualCache/historical/公共捕获/入口/打包工程/CF CX CY 均未改；已有修复保留。

### 历史与交付范围
已读执行规则、完整方案必要段、同KF报告全文及KH、目标方法、生产target/device/interop相关段，非全仓/全历史逐字审计。原full x64 1814/40、x86 1814/4保持历史身份，不绑定新trace SHA。Available/HWbackend/真实loss/mediaresources/full四格未闭，完整替换未完成。本轮真实沙箱task=1、发布=2、生产行为修复=0；决定性resize因果未覆盖的准确原因是本次原生前置失败。下一授权门仍须在前置成立时观察原方法生命周期，不以此失败直接更改通知。


KI 最后补记：以上KI全文及下列保留KF/KH全文、handoff/session-summary/issues最新KI受改节均已写后回读；TRX原ID与末尾Counters再次亲读确认1执行/1失败，未增加沙箱任务。证据范围仅上述原始文件和相关源码，不冒称全历史审核。

## KH 接续进行中（保留 KF 初稿）
已核静态事实：候选 OnDeviceDisposed 调用 NotifyDeviceLost；interop 同 adapter invalid 会清理 front、非 fallback surface 并 Disabled。这只是可疑链，不是实际 resize 根因。独立原版源码合同已发现反证：d3ddevicemanager.cpp 1146 正常销毁也调用 NotifyDeviceLost，1846-1853 明言 whenever a device is being destroyed。不能直接删除 Dispose 通知；正在执行有界生命周期控制，区分通知合同与原 resize 实际因果。

## KF 进行中
已读取当前执行规则、最新 handoff、KD/KE 分类结论。目标父 testID `1eded079-5019-809c-bf7c-07ef4caf0e29`；先核原始分类及 x86 旧红，再追真实 resize 协议及生产消费链。不由 FF00/FF80 两色推断缓存根因。x64 首帧阻断单独记账。

KF 初稿当时生产/测试业务零改，无新 build/publish/运行。保留 COM、fill、通知、shader 修复及旧证据；并行 KG 的 VisualCache/historical 只读，不覆盖公共夹具。旧 full 1814/40、1814/4 不变，Available/HWbackend/loss/mediaresources/final 仍开放。

## KH 实际控制终态：否定直接删 Dispose 通知的合同依据，原 resize 因果未闭

### 已核事实与独立合同
- 候选 `GeneratedVisualTargetResources.cs:295-300` 尺寸变化先 Create next、替换 `_renderTarget`、Release previous；`HardwareRenderTargetExports.cs:75-82` 正常销毁 Renderer/Device/Manager。manager `OnDeviceDisposed:942-958` 对尚未报告 lost 的 tracked device 调用 NotifyDeviceLost，再移除三种集合；缓存/注册移除确有独立语句，但不代表合同允许省略通知。
- 原版仓库源码 `core/hw/d3ddevicemanager.cpp:1146` 正常 UnusedNotification 删除设备前也调用 NotifyDeviceLost；`1846-1853` 明文要求 whenever a device is being destroyed，并以 fIsDeviceLost 防止多窗口重复通知。`1228` unusable 分支使用同一通知，不能仅按“没有物理 adapter loss”认定通知非法。
- 原版 `InteropDeviceBitmap.cpp:514-559` 收到同 adapter false 也释放非 fallback 用户 surface、ReleaseResources、永久 disable HW，除非重新 SetBackBuffer 创建 bitmap（fallback 例外）。候选 `ApplyAdapterStatus:533-559` 同方向。原版源码是独立合同依据，但未认证它就是当前10.0.13 servicing二进制对应提交；不把源码代替官方二进制实测。
- 因此用户要求的“正常 Dispose 不应收到 lost”不是可直接采用的独立 oracle。候选当前通知与该原版设计一致；异常更可能在 target/device 拥有关系、创建/销毁次序、重新 SetBackBuffer 协议或 per-manager/global 广播范围，但这些仍未实证。

### 实际执行，不是静态链交付
在现有 `Direct3D9DeviceManagerTests.cs` 添加 `WhenKhOldDeviceIsDisposedThenOfficialDestructionNotificationDoesNotMarkSurvivingDeviceUnusable`：同 adapter 创建两个真实 managed device/manager 对象，正常 Dispose old，观察通知 `True|True|False`，survivor.IsUnusable=false、usable=1/unusable=0。使用现有外部 D3D factory 空指针夹具，无 mock 自有 manager/device；无桌面显示依赖。

专用 run_tests 第一次旧合同控制3/3Passed、0Failed/0skip、Exit0、38ms；第二次新控制+既有 unusable/Dispose 去重分支3/3Passed、Exit0、27ms。均默认Debug net10.0、本机补充实验，不是正式双架构Release验收，不将重复旧2项累计为6项新增覆盖。完整执行摘要保存在 `artifacts/kh-resize-lifetime/managed-control.txt`。最后 normal Release solution build Exit0、2313warnings/0errors、7.84秒，正常分析器，未加NoWarn。

### 精确最早缺口与源码裁决
本控制实际证明“normal Dispose 发 invalid 但另一 managed device 仍 usable”，未证明另一 interop surface 仍可消费，未创建真实 COM bitmap、未运行 shader、未触发真实 HW loss。旧 MarkUnusable 测试只是通知去重，不冒称真实 loss 对照。没有修前真实失败回归，不能据此作生产修复；本轮生产行为改动0，保留所有此前修复，无需回撤生产补丁。

本轮沙箱tasks=0、publish=0、原x86trace=0；未新收集绝对TRX、testSHA/MVID、actualNativeSHA、runtime版本、BLExit或timeout/kill/drain字段，均记未采集而非通过。未重复KG官方红帧或full。原KD 1eded079 x86前FF80/resize64x24后FF00及7E0936…仅沿用已知历史，不冒称本轮亲读原始业务证据。新测试仅补充managed合同保护，不是用户要求的决定性独立COM控制。

下一唯一门：在原候选x86精确方法做一次默认关闭的KH专属生命周期trace，关联 old/new target/device与adapter通知、bitmap Disabled/front、实际shader像素；同时核是否因每target独立manager绕过原版共享device拥有关系而出现先valid后invalid。保持原expected/参数/cleanup及共享VisualCache不变；拿到真实因果前不删除正常销毁通知。若另行使用独立COM控制，必须验证合法producer surface真实消费而不是只有IsUnusable标志。本轮未完成该关键门，不宣称resize已修、有限生产修复通过或完整替换。

### 文件与历史范围
生产0改；仅manager专属测试一处、同KF报告及三共享最新KH节、ignored实验摘要；KG文件/VisualCache/historical/入口/项目打包未修改。已读执行规则与完整方案全部有效行、KF初稿全文、当前相关源码及原版上述合同、manager夹具相关段；没有全历史逐字审计。旧full1814/40和1814/4保持原身份；Available/HWbackend/真实loss/mediaresources/full四格仍开放。
