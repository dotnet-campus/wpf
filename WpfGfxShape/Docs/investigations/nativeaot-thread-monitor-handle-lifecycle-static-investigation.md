# NativeAOT Thread / Monitor 句柄生命周期静态复核

## 终态与结论

文档调查 Completed；生产/测试源码修改、build、test、publish、Native、沙箱、终端、脚本、再委派全部 **0**。仅新增本报告并在恢复入口/工作项/计划状态追加最小条目。本次没有新增运行证据。

**未找到能解释成功16轮残余增长的确定产品强根缺口，不授权生产修正。** 官方 v10.0.13 NativeAOT 源码证明：线程终止与 managed Thread._osHandle 关闭不是同一事件；Join 不释放其 owner 引用；Monitor.Condition 的线程局部 Waiter.ev 无逐次等待 Dispose。线程退出后的不可达资源可以等本 NativeAOT 实例的 GC/finalizer。**这是可成立的机制，不是本 DLL 已证只有延迟终结，更不是已证无泄漏。**

新增具体发现：NativeAOT **SyncTable.s_entries 强持有 Lock**，条目的 DependentHandle 不强持有业务 owner；DeadEntryCollector 终结后摘除条目、在表锁外 Dispose Lock。它能延迟持有竞争产生的 Lock._waitEvent，但没有据此找到静态表→scheduler/Thread/Condition.Waiter 的反向强根。不能把所有 static 表笼统称为弱表，也不能把强持 Lock 错写成永久保活业务 owner。

原 D3F5E40D2ABC0782D678C843537210EC203987A74C84616E20164703B3895B82 有限硬门 PassedKnownOwnershipAndCallbacksOnly 保留。20快照全 Partial；candidate 同 +1s r4→15 Event96→109、Thread16→28，official79/8稳定，是历史类型条目观察，不等于活线程、owner或创建栈。资源 **PendingInvestigate / MemoryDescriptiveOnly / memoryGreen=null / WorkerExitNotProven**；显示 Unknown，完整替换/full Open。x86旧15A545、旧失败/预算/SHA、解析840 NotRun等所有历史账不变。connection Acquire后发布异常漏Release独立 StaticConfirmed、未修，不混作成功趋势根因。

## 1. 版本与取证边界

先读 next-session-handoff、HandleTypes阶段B正式收尾、residual-owner静态报告、event-owner变更合同及正式收尾顶部、current-execution-rules/current-work-item/current-stable-baseline、replacement-issues-and-plan-status。长本地文件有返回截断，只声明已读切片；当前产品/collector按下表相关方法补读，不声称全仓审计。

本次逐文件通过公开网页读取 dotnet/runtime **v10.0.13 tag**，不是拿 src/coreclr/vm 的 CoreCLR Thread/Monitor实现套 NativeAOT。网页返回源码没有行号；以下远端使用精确文件+方法/字段锚点，**远端精确行号及tag解析commit SHA Unknown，不伪造 #L 数字**。本地行号来自带行号读取。v10.0.13网页内容作为可读机制依据，不冒充已与二进制逐指令绑定的源码。

实际 D3F5 所用 Microsoft.DotNet.ILCompiler、runtime.win-x64.Microsoft.DotNet.ILCompiler/runtime pack 的精确版本、包 repository commit、编译 feature 开关与本 tag 的绑定 **Unknown**。现成收尾的 .NET10.0.13及runtimeconfig是 COM host 身份，不足以证明候选 DLL 内嵌 runtime 版本。本次名称搜索没有呈现原 artifacts/channel-message-event-owner-x64-r1 或 handle-types.jsonl，Code目录也未呈现 project.assets.json；因此本次类型数字、错误50及32轮硬门引用既有正式收尾，不冒称重新亲读全部原快照/原发布日志。查询目录与文件名曾有失败，未通过执行补取证。

### 公开源码索引（均本次读到）

URL基址：`https://github.com/dotnet/runtime/blob/v10.0.13/src/`。以下为基址后的完整相对路径：

|编号|文件|已核锚点|
|---|---|---|
|R1|coreclr/nativeaot/System.Private.CoreLib/src/System/Threading/Thread.NativeAot.Windows.cs|_osHandle、GetOSHandleForCurrentThread、CreateThread、JoinInternal、OnThreadExit|
|R2|coreclr/nativeaot/System.Private.CoreLib/src/System/Threading/Thread.NativeAot.cs|StartCore、StartThread、StopThread|
|R3|libraries/System.Private.CoreLib/src/System/Threading/Thread.cs|StartHelper.RunWorker、t_currentThread、Start、Join|
|R4|libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/GCHandle.T.cs|Normal InternalAlloc、Dispose→InternalFree|
|R5|coreclr/nativeaot/System.Private.CoreLib/src/System/Threading/Condition.cs|Waiter、t_waiterForCurrentThread、RemoveWaiter、Wait、SignalOne|
|R6|coreclr/nativeaot/System.Private.CoreLib/src/System/Threading/Monitor.NativeAot.cs|s_conditionTable、s_createCondition、Wait/PulseAll|
|R7|coreclr/nativeaot/System.Private.CoreLib/src/System/Threading/SyncTable.cs|AssignEntry、Entry、DeadEntryCollector finalizer|
|R8|libraries/System.Private.CoreLib/src/System/Threading/Lock.cs|_waitEvent、TryEnterSlow、CreateWaitEvent、internal Dispose|
|R9|coreclr/nativeaot/System.Private.CoreLib/src/Internal/Runtime/ThreadStatics.cs|GetInlinedThreadStaticBaseSlow、GetUninlinedThreadStaticBaseForTypeSlow|
|R10|coreclr/nativeaot/Runtime/gcenv.ee.cpp|GCToEEInterface.GcScanRoots、EnableFinalization|
|R11|coreclr/nativeaot/Runtime/threadstore.cpp|AttachCurrentThread、DetachCurrentThread|
|R12|coreclr/nativeaot/Runtime/thread.cpp|Construct、Detach、Destroy、GetThreadStaticStorage、RegisterInlinedThreadStaticRoot|
|R13|coreclr/nativeaot/Runtime/windows/PalMinWin.cpp|PalInitComAndFlsSlot、PalAttachThread、FiberDetachCallback|
|R14|coreclr/nativeaot/Runtime/startup.cpp|RuntimeThreadShutdown、进程shutdown例外|
|R15|libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/SafeHandle.cs|~SafeHandle、Dispose(bool)、InternalRelease|
|R16|libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeWaitHandle.cs及SafeWaitHandle.Windows.cs|默认ownsHandle=true、ReleaseHandle→CloseHandle|
|R17|coreclr/nativeaot/Runtime/FinalizerHelpers.cpp|RhInitializeFinalization、RhpGetNextFinalizableObject、RhpWaitForFinalizerRequest|
|R18|coreclr/nativeaot/Runtime.Base/src/System/Runtime/__Finalizer.cs|ProcessFinalizers、DrainQueue|
|R19|libraries/System.Private.CoreLib/src/System/Threading/Tasks/Task.cs|SpinThenBlockingWait、SetOnInvokeMres、FinishContinuations、RemoveFromActiveTasks|
|R20|libraries/System.Private.CoreLib/src/System/Threading/ManualResetEventSlim.cs|Wait、using cancellation registration、Dispose|
|R21|libraries/System.Private.CoreLib/src/System/Threading/EventWaitHandle.Windows.cs|CreateEventCore→CreateEventEx→SafeWaitHandle|
|R22|coreclr/nativeaot/System.Private.CoreLib/src/System.Private.CoreLib.csproj及libraries/System.Private.CoreLib/src/System.Private.CoreLib.Shared.projitems|FeatureNativeAot、NativeAOT Thread/Monitor/Condition/SyncTable、Runtime.Base __Finalizer、shared SafeHandle/Thread/Lock/Task/MRES及Windows SafeWaitHandle的实际Include关系|

因此shared文件被用于NativeAOT有项目纳入依据，不是凭同名猜测。远端曾试错的 Pal.cpp/PalRedhawkMinWin.cpp/gcrhenv.cpp、错误Finalizer/SyncTable/Lock路径404不作为证据；之后按公开目录定位了R7/R8/R10/R13/R18/R22。未执行下载器或脚本。

## 2. 创建 / 持有 / 退出 / 关闭表

P=`WpfGfxShape/Code/WpfGfxShape/`，T=`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/`。当前产品源码不冒称完整等同D3F5的符号映射。

|对象/创建|持有及真实root|退出/移除|关闭责任与可证明/Unknown|
|---|---|---|---|
|scheduler：P Core/PartitionScheduler.cs:31–42 new Thread(Run)/Start|manager._scheduler、connection.Scheduler；运行栈的Run delegate；不是仅靠闭环|Run125–132 pending优先Dequeue、scheduled Remove，stopping且两集合空才返回；198 CoUninitialize；Dispose201–209 stopping/PulseAll，非本人Join|无公共Thread.Dispose；没有发现成功链新增可确定Dispose的产品owner。Join完成与OS handle关闭可同时不同步；全部worker真实退出仍未动态证明|
|Thread启动Normal GCHandle：R2 StartCore / R4|handle table强root→Thread，仅启动握手|CreateThread普通失败或握手正常完成均finally Dispose；释放者是创建线程，不是worker Run finally|不能写“GCHandle.Alloc(worker)成功后永远自根”。waitingForThreadStart时异步异常特例源码明确跳过Dispose；没有成功16轮触发证据|
|ThreadStart：R2 StartThread / R3 RunWorker|StartHelper→delegate→scheduler，活跃调用栈也可持有scheduler|报告启动成功后、调用业务Run之前：thread._startHelper=null；RunWorker把_start拷局部并置null，再调用委托；不是OS创建之前清除|Join不负责清delegate。活动栈完成前合法保活；返回后不能仅用scheduler↔Thread闭环声称永久root。ExecutionContext同样需按可达性判断|
|managed Thread._osHandle：R1|CreateThread返回owned SafeWaitHandle；外来线程CurrentThread初始化则DuplicateHandle当前线程|StartThread finally/OnThreadExit只置状态/前台计数；JoinInternal只AddRef→Wait→finally DangerousRelease|退出/Join不Dispose _osHandle；没有Thread公共Dispose。最后managed owner不可达后，R15/R16 finalizer可CloseHandle；此DLL具体不可达时刻/终结进度Unknown|
|native runtime Thread.m_hOSThread：R12 Construct|另一次DuplicateHandle；ThreadStore active list持native thread|R13 FLS callback→R14 RuntimeThreadShutdown→R11 DetachCurrentThread：premortem回调、移active list、Detach、Destroy|R12 Destroy直接CloseHandle；与managed _osHandle独立，不能将两者混账。进程shutdown跳过detach例外不解释正常逐轮清理。实际每条detach执行未测|
|TLS→Thread/Condition.Waiter：R9/R10/R12|inlined TLS注册root或per-thread object[][]；GcScanRoots只遍历ThreadStore中的线程并报告两种TLS根|R11移除线程后该线程TLS不再经此扫描成为root；无需在Thread对象里逐字段清TLS|TLS teardown不对每个值调用Dispose。若无别的root，Waiter/Event可以随后由本NativeAOT GC发现不可达并终结；不是“线程退出即CloseHandle”，也不证明最终化已经发生|
|Condition.Waiter.ev：R5/R6|首次Monitor.Wait时本线程惰性new Waiter→AutoResetEvent；同线程复用；Condition等待链暂时持Waiter，CWT非强key历史字典|SignalOne先RemoveWaiter再Set；未signal的Wait finally Remove；Remove清prev/next；正常返回不在队列内|Waiter无Dispose/finalizer，ev由SafeWaitHandle终结兜底。scheduler125可走此链；不能保证每轮一定进入Wait，也不能由Event类型认定该来源|
|Monitor SyncTable Lock：R7/R8|static s_entries[i].Lock强引用Lock；Owner=DependentHandle(obj,collector)，不是强root obj。Lock只存owner数字ID，不是Thread引用|obj被GC判不可达后collector可终结；若Owner.Target尚在则ReRegisterForFinalize。否则表锁内Owner/Lock清空、entry入free list|表锁外dependentHandle.Dispose及lockToDispose.Dispose→_waitEvent.Dispose。表容量不缩不是owner泄漏；但“所有Lock都立刻随TLS退出关闭”错误。额外Event延迟可来自此处；非每Lock必建内核event|
|Lock竞争event：R8|TryEnterSlow的wait分支才CreateWaitEvent，CAS胜者留_waitEvent|CAS输家立即Dispose；scope.Dispose只Exit锁，不是销毁Lock|SyncTable拥有的Lock有R7清理；普通Lock最终靠内含SafeHandle，不准反射或调用内部Dispose修补运行时|
|SafeHandle：R15/R16/R17/R18|owning、非invalid、fullyInitialized，内部借用refcount；NativeAOT实例自己的finalizable队列|~SafeHandle→Dispose(false)→InternalRelease(true)，在最后ref归还时ReleaseHandle；显式Dispose抑制finalizer|SafeWaitHandle.ReleaseHandle直接CloseHandle。__Finalizer等待请求后DrainQueue调用FinalizerCode；helper从本实例GC队列取对象并跳过已suppress条目。没有“Join触发finalizer”或“一秒内完成”合同；实际pack/GC/Close成功实测Unknown|
|Work.Completion / _initialized TCS：P Core/PartitionScheduler.cs:8–20,32,126,190–194；R19/R20|pending Work→Channel、TCS；同步等待期间completion action、栈；完成后TCS本身仍可被owner持有|Task.FinishContinuations交换为完成sentinel；超时/取消未完成等待RemoveContinuation；MRES.Wait的cancellation registration using归还|SetOnInvokeMres无捕获业务worker的lambda，Invoke只Set；其不Dispose为避免completion竞争，没访问自己的WaitHandle。MRES.Wait仍能经NativeAOT Monitor创建TLS event；乱加Task.Dispose不解决该event|
|业务connection/channel静态表：P Abi/UceChannelExports.cs:13–26,122–135,209–243|Connections→Connection→Scheduler；Channels→Channel→Connection/State|Disconnect130 Remove并Disconnected=true；无channel则Release，否则最后Destroy233/242归还。Destroy232摘表；成功趋势先Destroy A/B再Disconnect、Deinitialize|字段Scheduler不置null不等于root：对象已无表root时其出边不是外部根。创建异常Acquire后漏Release另案仍在。不能把失败路径一概当成功移除|
|manager引用计数：P Abi/PartitionManagerExports.cs:18–55,90–99|_scheduler是真静态强root；initialization与async connection各一ref|末Release先_references归零、_scheduler=null，再锁外stop.Dispose；Deinitialize减_initializations后Release|成功配对链未发现静态历史scheduler列表。返回成功不是每个私有字段实测；如有未登记ref则会保活，但尚无成功轮证据|
|测试MemorySource GCHandle：T ManagedStreamTests.Lifecycle.cs:160–188,218–225|GCHandle.Alloc(this)强root source→Disposed delegate→SharedRoundState，属于host堆|Create失败finally Free；成功移交descriptor；MemoryDispose先Stream.Dispose再root.Free再source.Disposed.Invoke|本路径MemoryStream且正常callback1/退出1相容于Free已执行。Stream.Dispose若抛则Free未执行的结构事实不能当本成功路径根因。states列表保留SharedRoundState/数值/事件，不反向持候选managed Thread|

补充：Task的debug active表为static字典但R19完成路径RemoveFromActiveTasks；本产品用无state的promise TCS，没有发现把已完成任务永久登记到产品static表。即使callback/Task/Channel构成环，仍必须找到外部强root才是保活证明；内核Thread handle本身不是托管Thread对象的GC root。

## 3. 退出与积累为何可以同时成立

公开合同：[Thread.Join](https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread.join?view=net-10.0)是等待终止，不承诺释放Thread私有句柄；[Terminating a Thread](https://learn.microsoft.com/en-us/windows/win32/procthread/terminating-a-thread)明确线程终止置signaled，但thread object直到所有handle关闭才释放。结合R1，即使某轮scheduler已经终止，进程仍可以保留指向已退出Thread的句柄。多个条目还可能指同一thread object，Thread计数不等于不同TID数量。

因此“活线程数基本稳定”既不证明handle leak，也不否定已退出Thread句柄积累；已知某releaseThread退出也不能替代本趋势全部scheduler的TID身份。R1正常ThreadEntryPoint返回0，但不能将进程内所有Thread句柄退出码预设为0。

host的GC.CollectionCount只描述test host GC，不能证明DLL NativeAOT GC/finalizer运转。R17/R18进一步确认NativeAOT有实例自己的finalizer处理链；不先强制GC、不做私有CloseHandle/反射补丁、不凭回落宣布永久问题消失，不猜测runtime性能/缓存策略。

## 4. 20个Partial的具体来源与旧记录能力

本地 T ProcessHandleTypeSnapshot.cs:104–153：class51返回表并通过layout后，117先置Partial；只有153 `Classified == count && Opened == Closed`才升Complete。128–131 duplicate失败记录EntryIndex/操作/Win32Error并continue，不设置顶层Reason/Error；因此**Partial且Reason/Error=null完全可能是逐entry失败，不代表没有失败**。预算120–121、清理151另有显式Reason。TypeUnknown也可导致Partial，不能把所有Partial无条件解释为竞态。

既有正式收尾§全部20固定点记载：class51各点status0、H=Enumerated，已分类C=Opened=Closed；unknown O65/81/73、C63/71，局部失败为DuplicateHandleRaceOrDenied/Win32Error50，没有预算触发或cleanup错误。结合代码支持本次缺口在未duplicate成功条目，而不是总表已截断或type buffer普遍失败。错误50为“不支持该请求”，**不能由工具自定操作名推出AccessDenied或瞬时竞态，也不能指定未知条目真实类型**；原失败entry本次不可用，逐项复核Unknown。

Snapshot238–274只保存Types计数、Call以及Failure(EntryIndex,Operation,Win32Error)，无Thread逐entry记录、原handle关联、真实TID、exitCode或线程signal状态。原表虽在Capture局部可读，当前只抽取value并丢弃关联；旧20快照无法事后补算Thread→TID/exit。self duplicates/buffers配平outstanding0只证自有清理，**不证旁路无扰动**；原Handles在sidecar前采也不是后续行为无扰动证明。Event的类型名不携owner/创建栈，不增加Event wait/name推断。

## 5. 唯一下一动作（只建议、NotRun、须另授权）

**仅增强现有COM测试侧collector，在原有固定10点/role内，为已分类为Thread的自有duplicate记录TID/零等待退出状态，并关联现有逐轮callback TID。生产DLL保持D3F5，不发布Native。** 这不是重选一个无关major门，而是在现有相同有限趋势增加能区分“活线程”与“已退出但handle仍在”的信息。

### 已有可用业务TID，不新增生产诊断API

T UceChannelTests.AsyncSharedOwnership.cs:25–54：SharedRoundState.CallbackThread，由OnDispose39的DrainGetCurrentThreadId赋值；每轮business.json283字段 **callbackThread**。callbacks.jsonl每条有 **operation/utc/qpc/pid/thread/entries**，Callback.Enter/Exit的thread=CallbackThread；events.jsonl90–100另有 **order/operation/detail/utc/qpc/pid/thread/managedThread/disposals**。ResourceLifecycleTrend.cs104–108提供精确round-XX路径并将state保存到states列表，77–85是既有Sample→collector固定点。

这些是实际释放回调线程TID，不直接命名为scheduler专属TID。当前异步Destroy工作在PartitionScheduler.Run149–154执行channel.Destroy，故是可检验的scheduler释放线程候选；相关角色/回调迁移或重入都必须保留语义边界。没有独立scheduler start/end业务日志，不伪造该字段。优先复用现有callbackThread、round及qpc，不新增hook；如未来确需标注，也只在现有默认关闭测试配置里复制这些值作为“callback-worker候选关联”，不得加生产公共API或改Native取得私有字段。

### 最小安全采集合同

- 保持RESOURCE_LIFECYCLE_HANDLE_TYPES默认关闭（可在该诊断内再默认关闭Thread关联）；仅COM helper及必要纯解析/状态分类测试。无新Host/包/ETW/CLI，无OpenThread扫TID、权限提升、原handle关闭或DUPLICATE_CLOSE_SOURCE。
- 在135–141成功拿到**同一自有duplicate**且type严格为Thread后，在143–149既有finally关闭之前查询。[GetThreadId](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getthreadid)、[GetExitCodeThread](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getexitcodethread)都要求THREAD_QUERY_INFORMATION或THREAD_QUERY_LIMITED_INFORMATION；SAME_ACCESS不保证具备。失败即时保存last-error，5才叫权限不足，其他错误单列，null不补0。
- 可对Thread duplicate作[WaitForSingleObject](https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject)(0)，仅当具SYNCHRONIZE；不是等待Event/Mutex，不消费业务事件。WAIT_OBJECT_0证已退出，WAIT_TIMEOUT证此观测时尚未终止，WAIT_FAILED保留错误。同步调用前后不关闭duplicate；只零等待，不加轮询/等待点。进程对象不能当Thread处理。
- exitCode!=259且API成功可作为已终止信号；**259有歧义**，可能为线程真实退出码。不得仅凭259标StillRunning；无zero-wait证据则Status=UnknownStillActiveCode。signal与exitCode分时查询可能跨退出，记录每个观测Qpc、顺序及Inconsistent/Transition，不强拼原子状态。建议先zero-wait，退出确认后再解释exitCode。
- 每条记录建议：schemaVersion、role/pid/round/phase、EntryIndex、OriginalHandleValue（只标本快照候选，不当对象唯一ID）、GrantedAccess（在已验证x64 layout中读取，明确非稳定NT合同）、Type、QueryQpc、Tid?/TidError?、WaitResult?/WaitError?、ExitCode?/ExitError?、ExitState、MatchedCallbackRounds[]。Match是按业务TID的关联，不是创建栈或owner认证。
- TID可能退出后复用，handle值也可复用；跨点匹配加role/pid/round/qpc区间，并明确ReusedOrAmbiguous，重复handle同TID要分别统计条目与distinct TID，不能将一个TID匹配强认唯一对象/唯一scheduler轮。若无创建时间证据，不抹掉歧义。无须为此长期持有duplicate到下一轮。
- 保持原16轮4+12、65 samples、固定10点、原phase/预热/阈值、official-first、round20/role360/父840/外900。增加查询也计入原snapshot2s/role10s以及原allocation/entry上限；到限明确ThreadCorrelationPartial，不跳过类型失败、不把类型Complete与关联Complete混成一个门。写入扰动继续披露，包括原第二记录自身写入未计入问题，不以selfcleanup0声称无扰动。

### 最多两个可证伪假设

1. **新增Thread条目主要指向已退出的逐轮callback-worker候选TID**，与managed _osHandle等待本实例终结相容；Event可来自TLS waiter或SyncTable Lock清理延迟。预测r4/r8/r15新增Thread中能匹配先前轮TID且signaled，distinct exited TID跨轮累积。若具权限而增量主要是仍运行/无匹配其它TID，此假设作为Thread增量主解释被削弱。即使符合，也不能据Thread退出认定Event owner，不能区分Thread对象仍被强root还是已不可达待终结。
2. **增量主要不是那些逐轮scheduler释放线程，而是其他线程对象/同TID多handle或持续活跃线程**。预测新增条目的TID/状态与callback集合不一致，或集中少数TID。若增量主要由匹配旧轮且已退出的distinct TID组成，则削弱。它不是已证明产品static root缺陷；Thread采集失败/Unknown时不选赢家。

### 可直接授权范围、停止条件与预算

可直接授权“仅COM默认关闭Thread关联实现+对应纯状态/解析测试编译，normalbuild≤1；成功后COMpublish≤1（隐式编译流程另计≤1）及唯一沙箱≤1外900，Native0，原official→candidate各16轮”。不自动执行独立test发现/额外沙箱，不加无关ABI/full回归；测试可以并入既有同exe受控执行，若当前filter不能容纳则保留NotRun，不能偷偷额外启动。

预检D3F5/official/test身份或配置不符、编译/发布失败即停；不修后重试。执行任一业务失败、超时/强杀/unreaped、duplicate/buffer清理失配即停；关联权限不足/预算耗尽或大量复用歧义标能力不足，不变更权限、不加点重跑。一次有限运行结束就回读身份/TRX/业务/原样本/Thread记录及错误/配平：若已退出且匹配则只报告线程退出与handle保留共存；若活跃/不匹配则否证对应解释并保留Unknown；若不足则明确无判别结果，**均停止，不自动串生产修复或强制GC实验**。

新增判别力是“类型条目→TID→观测时退出状态→已有轮次回调身份”，而不是再次仅拟合斜率。无法证明GC root、最终Close、Event归属或资源收敛；这些边界不能随结果好看而取消。当前没有足以直接授权的成功路径生产root释放修正。
