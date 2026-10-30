# D3F5 x64 残余资源 owner 静态调查

## 裁决与执行账

静态调查 Completed；资源 **PendingInvestigate / MemoryDescriptiveOnly / memoryGreen=null / WorkerExitNotProven**；显示 Unknown，完整替换/full Open。源码修改/build/test/publish/Native/沙箱/终端/脚本/再委派全部 **0**，仅文档写入。保留所有历史失败、预算、旧SHA与有限Passed；不把本报告当新运行验收。

新x64 `D3F5E40D2ABC0782D678C843537210EC203987A74C84616E20164703B3895B82` 的16轮原硬门及八角色受影响回归按既有报告成立，本次未重跑。x86仍15A545，未包含event-owner修正，不能迁移结论。

**实质结果：成功趋势路径未找到第二个可直接确定性Dispose的产品缺口。发现一个确定的 connection 创建异常回滚缺陷，但不是成功16轮残余增长的根因证明。首要剩余候选已具体到 NativeAOT Thread 的 SafeWaitHandle 和 Monitor.Condition 的线程局部 AutoResetEvent，而不是由约+2拟合“两Event”。不据静态给运行时对象打生产补丁。**

## 1. 阅读范围与证据定位

已先读恢复入口、current-execution-rules/current-work-item/current-stable-baseline、连续性协议、旧 resource-lifecycle-growth-static-investigation、channel-message-event-owner-x64-closeout 和根 Docs/completed-work/replacement-channel-event-owner-affected-regression-x64-closeout 的当前裁决/角色表；长文件部分工具返回截断，不冒称全文。本文直接核读当前代码的创建/释放段，官方可读源码，无CLI。

证据根（均仓库相对）：
- N=`WpfGfxShape/artifacts/channel-message-event-owner-x64-r1/sandbox-results/Results/channel-message-event-owner-x64-r1/ResourceLifecycleTrend/ba8524588c8441e48c35a68c0cf68cb6/`。
- O=`WpfGfxShape/artifacts/resource-lifecycle-trend-x64-r2/sandbox-results/Results/resource-lifecycle-trend-x64-r2/ResourceLifecycleTrend/1493b441b1ae4d86a804fcae8b8eb4ea/`。
- N candidate/descriptive.json 195–260、939–1005、1683–1750、2235–2255、2426–2518；N original 2240–2252、2426–2518。O两role 2240–2252，candidate2426–2518，original2426–2450及2490–2518。
- N candidate/samples.jsonl 1–17（7–13初返回截断，以12、13单行补足）、18–21、26–37、54–57、62–65；N original18–21。**不是本次全量重读65或130行**。旧phase表/旧GC关键窗沿旧静态报告，不伪称本次重读旧全部samples；斜率直接引用现成descriptive，无脚本重新拟合。

## 2. 残余何时出现、phase及自然GC

顺序是 BeforeInitialize / AfterSourceCloseTargetUpdate / AfterCleanup / AfterCleanupPlusOneSecond；句柄原值未扣采样开销：

|轮|新candidate PID1716|旧candidate（旧静态报告）|新candidate宿主GC0/1/2关键变化|
|---|---|---|---|
|0|234 / 284 / 284 / 284|不在本次关键比较表|0/0/0 → cleanup1/1/1；首次装载初始化混杂|
|1|284 / 292 / 286 / 287|不在本次关键比较表|1/1/1 → cleanup2/2/2；+1秒不采集|
|2|287 / 295 / 289 / 289|不在本次关键比较表|2/2/2 → cleanup3/3/3|
|3|289 / 297 / 291 / 291|不在本次关键比较表|3/3/3 → cleanup4/4/4|
|4|291 / 299 / 293 / 293|299 / 308 / 303 / 303|4/4/4 → cleanup5/5/5|
|6|295 / 300 / 297 / 298|307 / 313 / 311 / 312|Before6/6/6 → source8/7/7；cleanup及+1秒不变|
|7|298 / 306 / 303 / 303|312 / 321 / 319 / 319|8/7/7 → cleanup9/8/8|
|8|303 / 311 / 302 / 302|319 / 328 / 320 / 320|9/8/8 → cleanup10/9/9|
|13|311 / 319 / 313 / 310|336 / 345 / 337 / 337|14/13/13 → cleanup15/14/14 → +1秒16/15/14|
|15|315 / 323 / 317 / 317|344 / 353 / 348 / 348|17/16/15 → cleanup18/17/16|

因此残余不是轮4才开始：轮0大幅启动阶跃不适合归因，轮1起即见重复轮净增加并跨轮延续。新measurement四phase斜率依次2.062937062937063 / 2.1538461538461537 / 2.1538461538461537 / 2.055944055944056，不能只取最有利phase。多数测量轮活动段+8，cleanup -6；轮6活动+5/cleanup -3/+1秒+1；轮7cleanup -3；轮8cleanup -9，轮13的+1秒另降3。存在释放/创建交错，不是每轮恒定+2。

N candidate +1秒序列293,295,298,303,302,304,306,308,311,310,315,317，窗口median296.5/305/313，firstLastDelta24；O candidate303,307,312,319,320,324,328,332,336,337,344,348，median309.5/326/340.5，delta45。旧slope3.944055944055944→新2.055944055944056只支持部分改善。新旧official均274–275、slope0.06993006993006994；官方平台与候选正斜率是差异而非根因定位。

新PrivateBytes +1秒slope251588.92307692306 bytes/轮（旧224105.62237762238），新范围22519808–30068736、首28696576末30068736；未改善。旧official slope -9251.804195804196，新official -50340.6993006993。轮13cleanup→+1秒句柄313→310同时PrivateBytes29040640→29933568，足以反驳“句柄回落就证明内存恢复”。轮8净-1与轮13自然GC后的-3不证明全部关闭，也不证明是哪一种finalizer。

轮4/6/7/8/13/15新candidate线程11→12→11→11，与短命scheduler相容；不是所有线程身份的退出证据。已知caller/channel/duplicate/connection/manager账cleanup归零、WIC scope已Dispose、callback当轮1/1/1。**宿主GC.CollectionCount只观测test host CLR，不观测DLL内NativeAOT运行时堆/终结队列**；即使同PID，也不能用宿主自然GC排除native托管延迟finalize。

## 3. Owner、引用与释放责任链

路径缩写P=`WpfGfxShape/Code/WpfGfxShape/`，T=`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/`。以下源码行号为本次当前文件，不冒充D3F5二进制符号对应证明。

|owner/创建|引用持有、静态可能|显式释放/最终化/异常与判断|
|---|---|---|
|SameThreadChannel私有event，P Core/SameThreadChannel.cs:248|channel→event→SafeWaitHandle；连接/Channels表、partition registry、Work.Channel可暂持channel；WaitForMessage借用302–316|构造失败347 Dispose；ReleaseResources653–661同_messages锁设destroyed、Set finally Dispose；Destroy640–648 finally进入清理；Wake285拒绝终态Set。活动wait DangerousAddRef/Release配对，关闭可能延到借用归还。类自身无finalizer，SafeHandle兜底。此趋势未调用消息等待，未找到新的借用不归还路径；已修缺口不重报。|
|PartitionScheduler Thread，P Core/PartitionScheduler.cs:31–42|scheduler._thread；ThreadStart(Run)在执行期间→scheduler；manager._scheduler、Connection.Scheduler、partition.Videos回调、pending Work及scheduled集合可持有；闭环本身不是GC root|Dispose201–209 stopping/PulseAll；非当前worker Join，当前worker避免自Join；Run finally198 CoUninitialize。Dispose没有清_thread字段/清队列，但Run按队列优先处理至空再返回；正常表摘除后没有发现产品静态历史scheduler列表。Thread本身无公共Dispose，不能强转IDisposable或反射关闭私有句柄。构造Start异常依运行时清理；CoInit失败Acquire30–33 Dispose。|
|Work.Completion、_initialized TCS（15、32）；同步GetResult（42及UceChannelExports192、240–241）|pending→Work→Channel；局部cleanup/registration；完成前Task continuation；完成后runtime交换为完成sentinel；scheduler._initialized存到scheduler不可达|TCS本身无Dispose。没有显式AsyncWaitHandle/WaitHandle或RegisterWaitForSingleObject。Task阻塞确实可创建SetOnInvokeMres，不等于每TCS自带内核event。MRES.Wait通常走Monitor，NativeAOT Monitor另外可以创建runtime event，见下文。对Task乱加Dispose不能清除线程局部Monitor事件，且与完成/等待竞争。|
|NativeAOT managed Thread._osHandle、启动GCHandle、ThreadStart|官方Thread.NativeAot(.Windows).cs：CreateThread→SafeWaitHandle；StartCore临时GCHandle<Thread>，启动线程TLS t_currentThread；StartThread先清_startHelper，再Run；共享Thread.cs StartHelper.RunWorker在调用前清_start|启动成功或普通创建失败finally Dispose启动GCHandle；源码保留异步异常等待启动时不清root的特殊风险，正常成功轮不支持该异常。JoinInternal只AddRef/Wait/DangerousRelease，不Dispose _osHandle；无公有Thread.Dispose，SafeWaitHandle靠本运行时finalization。**清delegate发生在启动/RunWorker，不是Join负责**。运行结束栈退出后delegate不再是永久root，不能声称Join会清业务队列。|
|NativeAOT Monitor.Condition.Waiter.ev及Lock._waitEvent|scheduler125 Monitor.Wait(_gate)→Condition.GetWaiterForCurrentThread→[ThreadStatic] Waiter→AutoResetEvent；Monitor.s_conditionTable是ConditionalWeakTable而非普通强key历史表；condition等待链仅在wait期间引用Waiter。Lock竞争可惰性创建独立AutoResetEvent|Condition.Wait finally/SignalOne RemoveWaiter并清prev/next；Waiter没有显式Dispose/finalizer，内含event/SafeWaitHandle最终化。线程TLS离开后可回收但何时最终化未知。Lock有runtime internal Dispose，不是产品可随意调用的公共API；CAS输家event显式Dispose。因此“产品只用Monitor所以不会有Event”不成立；也不能声称每轮恰好两个Event。|
|NativeAOT native runtime Thread.m_hOSThread（官方Runtime/thread.cpp）|Thread::Construct DuplicateHandle当前线程，ThreadStore attach list持有native thread；不是managed Thread._osHandle同一所有权|ThreadStore::DetachCurrentThread移表→Detach→Destroy；Destroy CloseHandle(m_hOSThread)。此native close是detach路径，不是宿主GC或managed Thread finalizer。Join未实际测每条detach链，不能据静态断言该handle全部残留。两种Thread handle不可混账。|
|connection/manager refs，P Abi/UceChannelExports.cs:13–26,49–57,122–135,209–243；PartitionManagerExports22–55,90–99|Connections/Channels静态字典强引用Connection/Channel；Channel→Connection→Scheduler。初始化与async connection各Acquire，末channel或disconnect才归还connection ref；manager末ref先_scheduler=null再Dispose|成功趋势先Destroy A/B，Disconnect移表/Release，再Deinitialize Release/Join，配对成立。字典容量保留不等于保留已删除值。CreateChannel178–204注册失败有Destroy与移表；Destroy提交失败保留表可重试，不能把所有失败都写成已关闭。**Create本身存在异常rollback缺陷，见§4。**|
|真实WIC descriptor/MemorySource/stream|T AsyncSharedOwnership161–184，MemorySource.Stream是MemoryStream；GCHandle.Alloc(this)使descriptor持source→state.OnDispose。caller stream/frame/wrapper及B资源/协议packet AddRef维持COM链；不是FileStream且没有此层新建event|T Lifecycle160–188成功转root，失败Dispose stream/finally Free；218–225 callback Stream.Dispose、root.Free、通知。P ManagedStreamExports71 NativeMemory分配，Release100–106 descriptor.Dispose finally Free，无finalizer；WicBitmapAdapter34–52 QI取owned，失败finally Release，Release95–97末ref Release Source/Free；BitmapSourceExports13–42 finally Release adapter/bitmap。系统decoder/factory由T SystemWicBitmap38–42 Release，frame交caller；CoUninitialize23。裸COM对象不靠SafeHandle finalizer兜底。未知系统WIC内部缓存/事件，未发现本层显式event owner缺口。|
|测试state/events、ProcessThread采样|T Trend25–26存16states/65samples，AsyncSharedOwnership的callback通过GCHandle临时持state；输出后Events=null；samples只存值数组，不持ProcessThread。角色timer closure为role级|Trend148 using Process，155–157 finally逐ProcessThread.Dispose。仅Threads.Count不说明为每个wrapper OpenThread；getter异常记录null/Error。不能重复修已有Dispose。角色两Timer61–62 using，非每轮两个event。状态对象保留导致有限托管分配，不能量化成全部PrivateBytes。|
|CTS/SafeHandle/ThreadPool等待注册|T NativeRun72–83 deadline/collection/Process/stream均using或await using；角色监督在父PID，不在candidate PID；WaitForExitAsync内部注册为每role而非每轮|父finally kill/reap/drain，异常记录，不能制造candidate每轮同样阶梯。scheduler链没有CTS/显式ThreadPool注册。RunContinuationsAsynchronously不意味着每SetResult必新建线程；同步等待的内部completion action可inline，ThreadPool只是runtime候选，不是可Dispose的用户owner。Av.StateThread虽另有MRES，不是本bitmap95/visual39合同的实际创建路径，不能拿相邻功能代替因果。|

### 官方可读源码依据与版本边界

已读取 GitHub dotnet/runtime `v10.0.13` 的 nativeaot/System.Private.CoreLib/src/System/Threading/Thread.NativeAot.cs、Thread.NativeAot.Windows.cs、Condition.cs；另读取 `v10.0.0` 的同文件、Monitor.NativeAot.cs、Runtime/thread.cpp、Runtime/threadstore.cpp、libraries/System.Private.CoreLib/src/System/Threading/{Thread.cs,ManualResetEventSlim.cs,Lock.cs,Tasks/Task.cs}。URL模式为 `https://github.com/dotnet/runtime/blob/<tag>/src/<path>`；均纯网页读取。raw.githubusercontent多次404已改读GitHub页面，未执行任何下载脚本。

Task.SpinThenBlockingWait源码明确：不Dispose其MRES是为了避免仍在执行的completion action竞争；不访问MRES.WaitHandle所以没有该MRES自有finalizable event。但MRES.Wait内部Monitor.Wait在NativeAOT会走Condition.Waiter.ev，不能将前一句错误扩大为“整个等待链绝无OS事件”。Condition的TLS事件属于runtime；本产品无法合法逐轮Dispose它。

宿主实际.NET10.0.13不自动证明已发布NativeAOT使用完全相同runtime commit；v10.0.13可读源码支持机制候选，未读取D3F5的编译runtime精确commit。v10.0.0 native ThreadStore也仅作机制参考，不假装是二进制创建栈/对象实测。

## 4. 第二个确定缺陷：只在connection发布失败路径

**StaticConfirmed：P Abi/UceChannelExports.cs:49–57 在Acquire成功（52）之后，NewIdentity checked增量（29/55）或Connections.Add（56）抛Overflow/OutOfMemory；catch60–65只返回错误，没有PartitionManagerExports.Release。** output预先置0，connection尚未发布，caller无法Disconnect补还。manager._references多一份；以后正常Deinitialize也可能留下_scheduler静态强引用、存活worker和间接对象。Connection没有finalizer/ref回滚。

这是引用所有权缺陷而非“Thread忘Dispose”。最小合法修正设计（本阶段不实施）：只改Create，把成功Acquire标为尚未转移的本地owner；成功Connections.Add后才移交给字典，返回output。所有失败/异常统一finally归还尚未移交ref，**必须在离开UceChannelExports.Gate之后** Release，保留现有HRESULT；不能在任一catch里直接Release并Join。

并发风险：产品Gate内Join可能等待worker正在callback重入同Gate而死锁；rollback必须只还本次ref，不能清共享_scheduler或Dispose其他connection的scheduler；成功发布后不得重复Release。与其他调用同时Acquire/Release需继续依manager自己的CompositionGate序列化；不得为了回滚改变正常销毁顺序。后续修正需定向失败路径证据，但不列为当前并行下一动作。

**不能解释N的16轮全成功创建：这些轮没有该失败，业务账正常配对；不据此宣称已经定位残余趋势根因，也不为此立即重发Native。**

## 5. 最多三个可证伪假设

1. **运行时Thread + Monitor TLS等待事件延迟finalize**。每轮新scheduler线程，加入Monitor.Wait；存在具体Thread/Event创建链且Join不Close。预测cleanup后Thread与/或Event类型增量跨轮累积；若增长集中其他类型或Thread/Event净量平台，则这条作为主要句柄解释被削弱。即使符合，也只是类型相容，不是owner/创建栈证明。
2. **锁竞争/等待与系统COM/WIC内部驻留，而非私有channel event**。Lock惰性Event、WIC内部未知；预测活动段与cleanup段有不同类型回落，残余可能Event/Section/其他。完整快照若无对应净增长或只Thread累积则削弱。仅Event类型绝不能归到channel。
3. **测试记录/双runtime堆及runtime缓存影响PrivateBytes，句柄与内存并非同一owner**。已知有限state/sample分配及自然GC回落，轮13内存/句柄反向；预测类型计数增量不足以说明PrivateBytes。若后续可证明同一明确owner的资源量与两者配对则削弱。当前不能把内存增长统称缓存或永久泄漏。

## 6. 唯一下一可实施动作：测试侧本PID句柄类型sidecar

**NotRun / 须另授权**：保留D3F5生产DLL不变，给原x64 official-first有限趋势加默认关闭的纯C#类型快照旁路；不另建Host/框架/包、不启用ETW/外部工具、不先强制GC。只有类型证据明确后，再选择owner归因及生产修复。§4异常缺陷独立登记，不插队冒充趋势修复。

### API、失败降级和安全边界

- 以GetCurrentProcess伪句柄查询本PID `NtQueryInformationProcess(ProcessHandleInformation=51)`；此信息类/结构不是稳定公开Win32契约。动态解析ntdll入口，记录OS版本/架构、信息类、返回NTSTATUS/ReturnLength及布局验证。Microsoft Learn的NtQueryInformationProcess明确内部结构可能变化，不把可调用等于长期受支持。
- 先实际查询；InvalidInfoClass/NotImplemented/入口缺失/布局不符标Unsupported，AccessDenied保留实码，其他失败标Failed。不能未查就说无权限。降级只保留现成HandleCount/Unknown类型与失败原件，不扫描猜测handle范围，不替换全系统枚举，不把缺失补0；本门类型证据不足，不判资源绿。
- 建议预注册总表buffer从64KiB起，最多4次扩容、硬上限4MiB、最多16384条；checked计算头/entry长度、pointer-size及returned count不越界。仅长度相关状态重试，记录每次；到限标Partial/Failed而非截断当完整。布局版本不明不强行解码。
- 对表中每个本PID handle逐个DuplicateHandle到自进程，non-inheritable、DUPLICATE_SAME_ACCESS，**绝不DUPLICATE_CLOSE_SOURCE**。只对自有duplicate作NtQueryObject(ObjectTypeInformation=2)，只取TypeName（不是ObjectName）；类型buffer起1KiB、最多3次、上限64KiB；UNICODE_STRING长度偶数且指针/长度都在有效buffer内才读取。
- duplicate立即以SafeHandle/可靠finally关闭，成功数/失败数/NTSTATUS/Win32错误分别记账；buffer同样finally释放。不Close任何原handle。句柄关闭/重用是快照与duplicate之间的竞争，记录竞态/失败和完整率，不把查询结果当原对象精确身份。类型索引最多本次快照内使用，不跨Windows版本当常量。
- 只类型查询，不名称/内容/创建栈，避免名称查询潜在阻塞；仍记录同步query耗时，调用返回后超本快照2秒/角色累计10秒即停止其余诊断并标Partial，不能声称同步nt调用可由CancellationToken强制中断；外部原监督保留兜底。不要为查询新增Thread/Task超时器污染候选。
- 记录快照前/后GetProcessHandleCount、UTC/QPC、分配字节预算、耗时、枚举数、已分类/未分类、duplicate created/closed/outstanding（必须0）、文件输出时机和诊断自身开销。只在全部duplicate关闭后写sidecar；不改原samples/descriptive schema，不把sidecar counts回填原65samples。不要查询ObjectTypeInformation保留字段中全系统对象总数冒充本PID对象数。

### 固定最少点与类型增量判别

两role完全同一预注册10点，始终在原sample采完后追加旁路：Initial；round0的Before/Source/Cleanup/+1秒；round3 +1秒；round4 Source/+1秒；round8 +1秒；round15 +1秒。不因观察到峰值临时增点、不在所有phase全枚举。每role仍一个PID、4warmup+12measurement/原65samples/16轮/原GC/等待/阈值。逐点比较本PID Event/Thread/Section/File/其他全部类型的**handle条目数**，以及跨round3→4→8→15的净增量；这些是handle数，不是唯一kernel object数，重复handle可指同一object。活动点与cleanup点区分瞬态存量，offical同点作诊断自身扰动控制；不拿旧无sidecar绝对值当零扰动基线。

若支持完整且分类覆盖足够，才能说增长主要是哪种类型；若混合/覆盖不全，结论保持Unknown。类型≠owner/创建栈；Event增长不自动归channel，Thread增长也不直接证明线程仍存活。禁止先强制GC后以句柄下降宣布无泄漏。

纯托管现有自有SafeHandle账可以准确证明**已包装并在本runtime可见**的创建/CloseOutstanding，比竞态枚举更可靠；现有KnownOwners已经给COM/逻辑owner的硬信号。但它看不到NativeAOT私有Thread._osHandle、Condition内部event及系统WIC内部handle，且新增生产计数改变Native身份/违反Native0。故本门优先测试sidecar，并只给诊断自有duplicates建精确账，不扩生产包装框架。

### 必要最小文件、配置与预算

只拟改测试侧：T `UceChannelTests.ResourceLifecycleTrend.cs`（原Sample后10点hook）、一个内部 `ProcessHandleTypeSnapshot.cs`（可显式Link共享到Tests）、两正式csproj（默认false的独立HandleTypeSnapshotDiagnostic开关/仅显式配置启用；不能因原RESOURCE_LIFECYCLE_TREND自动开）、必要AcceptanceResources.resx本地化文案、Tests中小型解析/上限/失败结果及自有duplicate关闭测试。若父角色环境未传递再最小改NativeRun传固定开关；不另造runner/包/Host，不碰生产源码或Native工程。两工程TFM/SDK不升级，Release默认不开。

另授权预算：normalbuild≤1；COMpublish≤1（隐式编译流程≤1另记）；唯一沙箱≤1、外900秒，沿既有父840/role360/轮20，不加轮/重试/full/发现；Native0。正常构建与发布对象/隐式编译分账，沙箱内只运行原协调体及必要同测试exe诊断自检，不另起诊断host。official成功并核原件后才candidate，任一业务失败/超时/forced/unreaped即停；诊断Unsupported/Partial不补跑，保留业务结果与类型不足分列。

explicit AotRuntimeIdentifier=win-x64，AotDllPath绑定已验收D3F5。原source路径缺失时允许复用**同SHA已验收clean**副本，不因来源路径偏好阻塞；实际加载SHA/路径仍在新business核验，test新SHA/MVID单独认证。先预注册点与sidecar合同再发布，运行后回读父监督/TRX、两role身份/全部原65样本及旁路完整率/类型增量；缺件保持Unknown。当前只交付设计，不消耗该预算。
