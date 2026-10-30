# KL：统一合法初始化与有限原地 target resize

## KO 终态：KN 默认有限原地修复独立核证成立，保留；不是原像素或完整替换验收
KO 仅执行专用文件/API读取及本报告、三共享文档更新；新增 task/build/publish/沙箱均0，生产和测试源码改动0，无新Host/工程、终端或脚本，不重跑已有矩阵。启动时已在handoff登记核证进行中。本节覆盖下方KM/KL的当前行动建议，不改写其历史。核证开始时本报告只有61行、没有KN终态节；handoff只有KN进行中，故在原KL报告补齐KN事实与KO裁决，不另建重复报告。

### KN actual 两架构任务、父TRX及身份（KO亲读）
证据根 `artifacts/kn-inplace-resize/readback/{x86,x64}/Results/kn-{x86,x64}`；父TRX均为该根 `TestResults/child.trx`，不是BW子角色TRX。x86 task **5d545020b24440a399ecb67fc967673b**，08:39:07.424–08:39:13.026 UTC；x64 task **5ece82eddd464ed4bbca438cb1caca41**，08:40:01.520–08:40:06.206 UTC（2026-10-26）。两份Counters均 **total/executed/passed=17/17/17，failed/notExecuted/timeout/aborted=0**；stdout均17成功0失败0skip，监督业务Exit0/Reason exited。BL120；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut/OutputDrainCancellationTimedOut全false，HasExited/OutputCollected=true，SupervisorError=null。现有监督原件没有外层超时或工具数值Exit字段，不伪填外180或工具Exit0。
每架构分母精确为 **默认生命周期2（baseline/resize）+BW1+CK10（整数fill7、半整数round3）+真实软件snapshot2（21/22）+generic引用2（False/True）=17**；不是17个resize用例，也不是完整Renderer回归。两架构共34条父结果，不与历史full相减。

|actual身份|x86|x64|
|---|---|---|
|Native SHA256|20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D|40D093988D2F5BA13B40D58C02FA744CE31E84DB0C7DD8FD24C66746335DCF38|
|test SHA256|4565689159168EA37A17BF65B65E4416561FCE2DED8D149361095D8DDC3E9449|6BB784E9AFDFFD3AEB125A25B2845C30DA299DCB828D722AF2F85105060B6FD0|
|test MVID|0ea7a2f4-e898-4442-9e90-ceaf77b1b128|7473cd9b-8b02-40ee-b2c2-4fc50d2cfe8f|
|PresentationCore SHA256|D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D|0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31|
四份baseline/resize identity均逐份读取，actual Native路径为各task的Native/wpfgfx_cor3.dll，test为task根程序集；.NET **10.0.13**，架构分别X86/X64，WPF **10.0.13-servicing.26504.104+4099f83977f4d29604a9aeb971fb0a7bc1d5f699**；Native/test版本标识1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a，不把该版本字符串当未修改源码的唯一指纹。ConsumerPixelsObserved/DesktopCaptureExecuted均false。

### 默认生命周期、真实尺寸与身份层级
KO完整亲读两架构baseline/resize四份lifetime.json（各320行）、四份identity、两份完整22行target trace、两份40行supervision、stdout和空stderr，父TRX结果段和末Counters；不声称读过所有Diagnostic或BW子角色业务原件。四条生命周期均caller/Create/设置后surface refs **1→2→2**，snapshot **S_OK/non-null→S_OK/non-null**、临时snapshot Release0，producer cooperative0→0，设置期间通知[]。正常销毁channel/disconnect/deinitialize各S_OK；随后真实callback **Available0/Version701**，detach后bitmap/surface/device/factory四Release **0/0/0/0**，窗口销毁及cleanup complete全部存在。未删正常destroyNotify，也不能将设置[]写成Available=true。
x86 resize trace target地址B800818、managed target52815005/device48929204/manager66495956保持；真实surface.GetDevice后QI(IUnknown) **B97F600→B97F600**，GetDesc **32×16→64×24**，bounds同步64×24。x64对应target地址29188505D00、target52378879/device30374986/manager66231529保持，IUnknown **2918850F100→2918850F100**，GetDesc **32×16→64×24**。x64 surface地址复用本身不是身份保持证明；这里比较device IUnknown并读取新GetDesc。trace最后仍有disposeBackend→manager.deviceDisposed notify=True→adapter invalid→interop清surface，与cleanup callback相符。
尺寸/GetDevice/QI来自 **KI_RESIZE_TRACE默认关闭诊断分支中的真实D3D COM调用**；target/device/manager编号是内部RuntimeHelpers诊断身份。它们不是独立public target assert，也不是纯独立ABI全证明。公开producer引用、snapshot、callback是另一层实际证据，producer device不可冒充target device。actual包出现publicCOM trace，确认Native包含KI_RESIZE_TRACE分支；原地业务Resize本身不受该符号控制。KN_RESIZE_LIFETIME是测试文件默认局部define，非Native业务符号；KJ/KK/KL显式分支保留且默认不选择，默认方法调用所需Send/Settings/SnapshotStatus/References/清理均在同一guard内闭合。WPFGFX_DIAGNOSTIC_ABI是另一个默认false的项目开关，不能与KI混称；没有完整Native发布命令归档可读时，不声称已独立枚举actual包全部编译常量。

### 源码有限审计：没有具体新反例，不推测新算法
HardwareRenderTargetExports.Target.Resize先EnsurePolicy；generation变化仍DisposeBackend/InitializeBackend（Render入口同样EnsurePolicy），不是把policy变化无条件当纯尺寸更新。硬件复用现有Renderer.Resize，负HR立即传回，成功才提交Target.Width/Height；静态入口校验target及非零/int范围并传播COMException.HResult/OOM。UpdateWindowSettings仅在有效HWND、非零且矩形宽高变化时调用，负HR返回后不提交资源Width/Height，也不交换或Release整个target。软件先建next intermediate/bitmap及presenter，再换旧对象，finally释放未接管的新资源。软件失败保留旧资源；硬件Renderer失败会丢弃旧尺寸surface/chain并禁用render，不是事务回滚旧画面，不能据成功路径宣称失败恢复已实测。
该settings包没有像素格式参数；硬件初始化固定Pbgra32Bpp/A8R8G8B8，Resize只改present宽高。layer/transparency只校验，SetFlags是独立命令；未见本次把“格式变化”分支重写成Resize的具体反例，但不因此声称所有layer/flags/显示策略语义已支持。真实policy重建及manager正常销毁/loss通知源码保留，真实policy切换和loss恢复没有在这17条中触发。
Renderer.Resize清layers/contents、释放拥有surface及chain、清active3D并按尺寸释放multisample，建secondary chain/GetBackBuffer并更新bounds，失败清理并禁render。EnsureState→SetAsRenderTarget，Device.SetRenderTarget读取surface描述并设置viewport/clip/matrix；surface释放通知经OnSurfaceReleased→ReleaseUseOfRenderTarget→UnbindCurrentRenderTarget清设备缓存，避免仅因指针地址复用便沿用旧viewport。GeneratedHardwareVisualRenderer每帧CapturePure并用owner尺寸建立clip。以上是静态调用链核证，不是本组已跑带绘制viewport/cache/shader内容闭包；没有据此猜测或添加新cache算法。

### 初始化与官方来源、构建日志可读边界
默认夹具与KL/KK合法路径一致：connect1，92字节create/resource46/flags12/DPI awareness -1、DPI scale1，72字节settings及close/commit/flush/SameThreadPresent；非D3DImage同步Present。producer显式HAL adapter0；packet中的Adapter0是夹具合同记录，候选实际adapter由MonitorFromWindow选择（trace本次adapter0），不是独立官方backend/LUID读回。历史官方仍是 **KK task fbd2a0eaa7ce4521989887df58893221**；KO亲读其resize identity：官方SHA **5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834**、testSHA **CCE8D4CF61EF81574720BF8007A906DD48898EA95067EC711D449E895C01C629**、MVID **8c8511b2-21e3-45de-95e3-c50d6bad7a42**，同10.0.13 servicing；只复用历史官方控制，不称本轮同task双边单因子实测。官方源码hwhwndrt.cpp211起明确secondary flipping chain避免重建设备，是实现责任旁证，非运行backend oracle。
KO没有新构建。当前“最后日志”实际是KN **COM x64 publish**（Exit0/成功），不是默认solution build：共60行，stdout原52809字符，前32809被截，只保留末20000；尾段有实际analyzer warnings和发布路径，无solution构建warnings/errors总计。报告中尚无KN默认solution build日志原件；因此KO不能独立确认其Exit/warnings总数，不套KM的3806或KL的131，也不能将publish Exit0冒称新跑/default solution build通过。此为可追溯边界，不是已发现生产缺陷；不为了补数字重构建或机械撤回已有两架构关键生命周期修复。

### 有限保留裁决与唯一后继门
**有限保留独立成立**：当前默认业务保留真实target/device/manager、成功提交尺寸、错误传播、软件先建后换以及正常destroyNotify/policy重建；两架构实际关键正常生命周期门通过，未发现具体新反例。不因所有未来policy未验、原shader显示门未过或KN文档缺尾而额外阻断这项已有正确有限修复。默认生命周期纳入常规闭包；实际Native含诊断，不能把actual结果无条件外推所有无诊断发布包。
**未测清单**：原shader resize FF80→FF00本轮未跑，未称像素修复；新target创建/resize/清理专属Renderer兄弟未完整跑，snapshot/generic引用并非其同义替代；软件resize运行、失败后重试、格式/策略组合、真实loss、带绘制cache/viewport及consumer内容未覆盖。上述边界保留，不编造失败反例。
下一唯一门选 **currentNative原resize shader像素定向**：沿用原ID **1eded079-5019-809c-bf7c-07ef4caf0e29**、原参数及独立内容断言，绑定当前架构actual Native身份；前置不过就准确记failed/内容未覆盖，不重复随机变参追绿，不重跑本轮17条矩阵。本轮未执行该后继门；小Renderer专属回归保留为后续风险项，不并列发第二任务。旧full1814/40和另4保持历史、不得相减；Available/真实HWbackend/真实loss/mediaresources/full最终x86候选/x86官方/x64候选/x64官方四格仍开，完整替换未完成。

## KM 终态：实际实现并发布、x86 生命周期新绿；验收不完整，已精确撤回
本轮不是仅写方案：实际修改 HardwareRenderTargetExports.Target 的 readonly 尺寸字段，增加实例/static Resize；硬件复用现有 Renderer.Resize，成功后提交尺寸，软件先创建 intermediate/presenter 再交换释放；UpdateWindowSettings 改为调用原 target Resize。没有删除正常销毁/loss/Reset 通知或改变 COM ABI/整数fill/通知shader修复。KJ 文件默认加入严格 KM 两例：snapshot S_OK/non-null、refs保持、无 unavailable，沿用 KL 合法 connect1/flags12/DPI1/Compose；历史 KJ/KK/KL 分支保留。改前唯一文本表在 `artifacts/km-inplace-resize/change-table.txt`，可准确定位全部本轮业务变更。

### 实际发布与唯一 task
小 normal Release solution build 成功1；新 NativeAOT x86 publish 成功1、正式 COM 自包含10.0.13 publish 成功1。独有 `artifacts/km-inplace-resize/obj/bin/publish/native-x86/publish/acceptance-x86/com-acceptance-package`（后二者为该根下各自目录），显式绝对 PublishDir；仅上传 clean com-acceptance-package，正常依赖图/原白名单打包不改，source/readback 分离。实际小构建和两次发布是有限容量证据，空闲字节未知；没有广删或置空 DLL，没有 NoWarn/关闭分析器/改阈值/skip/fallback/全局禁并行。

唯一 task **715b335434b8483e8061e4eb968e3a61**，2026-10-26 08:25:48.218–08:25:51.750 UTC；精确 KM 方法与原 shader ID1eded079-5019-809c-bf7c-07ef4caf0e29 同组。TRX **3执行/2过/1失败/0skip**，业务 **Exit2**，Reason exited；BL120/外180，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut/OutputDrainCancellationTimedOut均false，HasExited/OutputCollected true，SupervisorError null。专用执行工具返回完成，未给独立数值工具退出码，不与业务Exit混同。
绝对 TRX：`C:\CodingAgentSandbox\Tasks\715b335434b8483e8061e4eb968e3a61\Results\km-x86\TestResults\child.trx`。回收根 `artifacts/km-inplace-resize/readback/x86/Results/km-x86`，亲读 supervision40行、stdout43行、TRX结果/计数、resize identity全文、resize lifetime全部320行及baseline关键refs/snapshot/cleanup段；stderr与baseline其余原件保留，未声称全量亲读。
actual Native **5742074481A861592ECF21487650BE06E2D785F4DF89FE378F7C448E41059D21**；test **208B61DCDCBF3772819AB8B888994C52D2D7A8C73308CE830B6166DEE58EEB2C**，MVID **1feb4656-9d95-49a7-846c-e7870b6c70d4**；actual .NET10.0.13/X86，PresentationCore D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D，10.0.13-servicing.26504.104。这些是已撤回实验包身份，不能绑定当前默认源码，亦非旧625DB身份。

### 有限观察与边界
两例均surface caller/Create/设置后refs **1/2/2**；resize前后snapshot **0/non-null→0/non-null**，临时snapshot Release0；producer cooperative **0→0**，设置阶段 notifications **[]**（只能说没有 unavailable，不能伪填 available=true）。两cleanup destroy/disconnect/deinitialize均0，真实销毁callback **0/701**仍保留；detach后bitmap/surface/device/factory四Release **0/0/0/0**，窗口销毁/cleanup complete。相对 KL 合法候选旧红的 refs2→1/snapshotE_FAIL，实验新包该正常路径实际变绿；KK 官方来源仍为历史 taskfbd2a0eaa7ce4521989887df58893221，非本轮新跑。请求resize32×16→64×24已执行，但夹具未独立断言目标实际64×24或目标device identity不变；不将public producer identity等同target device，不冒充consumer/shader成功。
原 shader 在 nativeControl 显示前置得到 **001E1D1C，预期00563412** 即失败，未进入其resource/shader/resize内容验收，不能称原FF00内容已修。无首个原生AV/中断，本组结束后没有扩量，没有重复旧红控制。x64、相邻targetResize创建/清理、BWCK、policy/失败可恢复分支未验；离线默认方法已被本次TRX两例真实发现，未完成其余依赖/闭包审核。

### 撤回裁决与当前源码
不是因显示前置失败自动否定原地修复，而是**双架构生命周期、目标尺寸/device身份、相邻回归尚未完成**，不满足有限保留门。按用户不完整验收必须精确撤回要求，已逐项撤销上述两生产文件及KJ默认测试增量，恢复原readonly和原Create/交换/Release路径，既有Notify/只读trace及此前验收修复不动；旧证据及本次新包/readback保留，未留半改业务。撤回后默认 Release solution build **成功/3806 warnings/0 errors/12.87秒**；warnings按本次实际输出，不套旧131。回读确认原字段、原替换块、原全文件guard；本轮最终生产业务净改0。
下一唯一门：以此已实跑正常路径为基础，完成默认回归的目标64×24/device身份验证及相邻回归，再按x86→x64一组完成有限保留裁决；不要重跑KL旧红或把本次撤回包当当前候选。旧full1814/40及4保持历史；Available/HWbackend/真实loss/mediaresources/final四格开放，完整替换未完成。


## 已完成的有限门：同合法初始化仍发生候选 producer 失效；生产修复未交付
本轮先建立本报告并登记 handoff 进行中，然后读取执行规则、完整替换方案、latest handoff、KF 中 KJ/KK 结论与实际夹具分支、GeneratedVisualTargetResources.UpdateWindowSettings、HardwareRenderTargetExports.Target、Direct3D9SurfaceRenderTarget.Resize、device 创建 secondary swapchain 与 manager.OnDeviceDisposed 合同。仅执行一组合法 candidate 控制，不重跑 official/full/显示红帧。生产修改为0，无待验收 default 业务补丁。

### 合法合同及差异裁决
在现有正式 COMAcceptance 程序集的 KJ 文件增加全文件默认关闭 KL_RESIZE_LIFETIME 分支；角色 kl-candidate-lifetime，仅加载 AcceptancePaths.AotDll。与 KK 官方分支共享 connect1、92字节 create/flags12/DPI awareness -1/scale1、72字节 settings、close/commit/flush/SameThreadPresent；不调用 D3DImage 同步 Present/Event0，不要求 desktopfresh。baseline 与 resize 两个 DataRow 各自独立创建并清理对象；producer 显式 HAL adapter0，32×16 device、12×1 A8R8G8B8 lockable surface，fallback0/version701；真实 SetWindowPos64×24 后发送 settings。未改 KG 参数、VisualCache/historical、DRDS、NativeRun/ExitDiagnostics、公共捕获、项目打包或 CF/CX/CY。

KK 原件 task fbd2a0eaa7ce4521989887df58893221 不重跑；本轮亲读其 resize identity 及 snapshot/refs/notifications 核心原件，源码相同合法传输路径，复用为历史官方控制，非本轮同一次 task。官方 SHA 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，10.0.13-servicing.26504.104/4099f83977f4d29604a9aeb971fb0a7bc1d5f699，test CCE8D4CF61EF81574720BF8007A906DD48898EA95067EC711D449E895C01C629/MVID8c8511b2-21e3-45de-95e3-c50d6bad7a42。

|观测|KL candidate baseline|KL candidate resize|KK official resize（复用）|
|---|---|---|---|
|surface refs caller/Create/设置后|1/2/2|1/2/1|1/2/2|
|snapshot 前→后|S_OK/non-null→S_OK/non-null|S_OK/non-null→80004005/null|S_OK/non-null→S_OK/non-null|
|producer cooperative|0→0|0→0|0→0|
|设置 available/version|[]|1/701→0/701|[]|
|cleanup callbacks|0/701|累计1/701、0/701、0/701|[]|
|Detach 后 bitmap/surface/device/factory Release|0/0/0/0|0/0/0/0|历史均0|

因此，旧 KJ 不合法/不同初始化并不能唯一解释候选失效：统一合法合同后同一失效仍成立。两项 Passed 是观测自洽测试，不是产品 resize 通过，亦不是精确旧红测试；未伪造旧红新绿。

候选现有 KI 只读 trace：resize old target BB6AE00/managed10726611、device57914551、manager27897252；new BB6A710/managed21098386、device24640104、manager10674031。旧 target Dispose→manager adapter0 invalid→bitmap BB6AE60 surface4E13900→0/DisabledTrue；producer cooperative 仍0，引用和快照独立确认后果。候选 target.hardwareReady 记录 adapter0 与真实 renderer 创建成功，属候选内部辅助，不是官方 backend oracle。trace 的 interop luid=0 不作真实 LUID。官方 target backend/device/adapter-LUID 未独立取得，本轮也未新增可公开读回该信息的接口；不能宣称同一真实 adapter 或唯一 HW 因果已闭合。consumerPixelsObserved/desktopCaptureExecuted 均false，front始终0。

### 唯一真实任务、身份及证据
新 task **bc44de8cbadf42a69546b83411629d4d**，2026-10-26 08:13:30.286–08:13:32.629 UTC，监督PID9104、child6768。TRX total/executed/passed=2/2/2、failed/notExecuted=0/0，业务 Exit0/Reason exited。filtered120秒/外180秒；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut/OutputDrainCancellationTimedOut均false，HasExited/OutputCollected=true，SupervisorError=null。工具仅返回执行完成，无独立数值工具 Exit，勿与子业务Exit0混同。

绝对 task TRX：`C:\CodingAgentSandbox\Tasks\bc44de8cbadf42a69546b83411629d4d\Results\kl-legal-candidate-x86\TestResults\child.trx`。
回收：`artifacts/kl-inplace-resize/readback/control-x86/Results/kl-legal-candidate-x86`。已亲读 supervision 全40行、TRX结果/末Counters、stdout全12行、空stderr、两identity全文、两lifetime全部有效行、KI trace全部有效行（超长工具响应分段补读末尾）。

实际 Native SHA **625DBB10879CFDB77CAA91F9A10F37B242247026D1B2930E5532031C53A63D7F**（复用已发布 KJ/KK 包依赖，无生产重发）；actual test SHA **0DB1D61F3ACE84AEADDC0B42139B066D27613BA942C860A4235C9354F6F19494**，MVID **0e55d8dd-e823-4b22-8664-212ee99810eb**；runtime .NET10.0.13/X86、自包含正式 Release。PresentationCore SHA D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D，10.0.13-servicing.26504.104/4099f83977f4d29604a9aeb971fb0a7bc1d5f699。

### 实现审计与未交付边界
官方 hwhwndrt.cpp211–306 本轮亲读：secondary flipping chain 为避免重建 device 而设计；Resize 先使旧内容无效并释放尺寸相关 surface/chain。候选 Direct3D9SurfaceRenderTarget.Resize4004–4081 已有对应能力：清 layers/contents、释放拥有surface和chain、处理multisample、更新present尺寸/CreateAdditionalSwapChain/GetBackBuffer/bounds、失败清资源并禁用render，HRESULT不能忽略。Direct3D9Device.TryCreateAdditionalSwapChain5206起使用现有 device 的真实 COM slot13，不是 Reset；已有 unusable/尺寸限制和失败资源释放。

当前上层 UpdateWindowSettings 仍是 Create 整 target→交换→Release旧 target。HardwareRenderTargetExports.Target 的 Width/Height 为 readonly，每 target 独有 Objects/Manager/Device；不能仅跳过 Release 或抑制 Notify。Manager.OnDeviceDisposed942–981 正常销毁亦通知 adapter invalid，应保留；真正 loss/reset 亦不能被尺寸路径修复吞掉。Generic target/shared COM AddRef 仍需保持同一 target 身份。尚未完成上层可恢复失败状态、software policy 分支、viewport/cache 与共享引用的完整有限实现和回归，因此本轮不提交生产修复。

本轮 COM publish成功1，default normal solution Release build成功1（最后测试源码修改后，131warnings/0errors/1.51秒）；无NoWarn/analyzer关闭、skip、golden/阈值或禁并行全局改动。KL/KJ/KK 全文件 guard 默认不进入编译，公开方法不进入默认源码闭包；未额外执行二进制API枚举。独有 artifacts/kl-inplace-resize/obj/bin/publish/acceptance/com-acceptance-package/readback，显式绝对 PublishDir，正常依赖图；仅上传 clean package，runtime/Contracts/Native 由既有白名单目标复制，回收仅Results。发布前未取得当前磁盘可用容量数值，属本轮执行纪律缺项；实际发布未报告 disk-full，未清理/置空任何 DLL、未扩大清理 scope。

### 裁决及下一唯一动作
合法 candidate 控制预算1组已用完，不再重复。**有限原地 resize 修复尚未实施/验收**；x64修复对照、默认真实resize生产回归、创建/清理/WBCK有限组及原shaderresize ID1eded079-5019-809c-bf7c-07ef4caf0e29均未执行，原像素门本轮未覆盖，不能用生命周期Passed替代。无两架构新生产SHA，旧full1814/40及4保持历史。

下一唯一动作：基于本次已确认合法候选失效，在保持正常销毁/loss通知的前提下完成 HardwareRenderTargetExports/UpdateWindowSettings 的有限原地 Resize（复用现有 secondary swapchain能力，成功后更新尺寸；失败可重试且无旧cache/泄漏），并加入默认独立生命周期回归，再按预算每架构一组真实验证；首个小构建前需补当前容量核。官方backend/LUID不可得仍明确未知，不得伪填。Available/真实HWbackend/真实loss/mediaresources/final四格开放，完整替换未完成。
