# 句柄类型 sidecar 阶段B：x64 正式收尾

## 终态与预算
CloseoutCompleted / 业务硬门 PassedKnownOwnershipAndCallbacksOnly / SidecarObservedPartial。**类型门不标Passed，资源PendingInvestigate / MemoryDescriptiveOnly / memoryGreen=null / WorkerExitNotProven，完整替换Open。**
亲自发布1/1（隐式编译流程1另计）及唯一Windows沙箱1/1；official-first各1个role、各16轮4+12/65samples。源码、normalbuild、Native、发现、额外test、纯解析/840测试执行、x86/full/显示/ETW/终端脚本/再委派全部0。StageA的normalbuild1/144warn/0err是旧账，本次不重build、不修源码、不抑warnings。纯解析及840边界测试仍仅编译NotRun。
证据根：`artifacts/resource-lifecycle-handle-types-x64-r1/`。准确发布与沙箱参数、原工具可见返回即时存00–03，核验台账04。独立publish/clean，不覆盖旧产物。生产connection Acquire后发布异常缺Release独立StaticConfirmed、未修，未证明能解释成功16轮。

## 发布与身份
ResourceLifecycleHandleTypesDiagnostic / win-x64 / net10.0-windows / self-contained，NETCore及Desktop10.0.13；显式AotRuntimeIdentifier=win-x64和AotDllPath原channel-message-event-owner-x64-r1/native/wpfgfx_cor3.dll。精确存在，历史clean根hash与actual认证已核，本次clean/aot.sha256再次为D3F5E40D2ABC0782D678C843537210EC203987A74C84616E20164703B3895B82。
发布真实exit0；原返回“执行成功。完整日志共 60 行，可使用 read_last_log_lines 按行读取。”无task号。stdout原57100字符仅末20000可见，前37100不可见；可见分析器warnings未抑制，缺聚合warnings/errors及发布耗时summary，**总warnings及发布真实耗时Unknown**，不能沿用144或造0。日志1–18/49–60核读，不冒完整stdout归档。隐式编译流程1，不另造实际Csc次数。
clean根runtimeconfig1–21全文NETCore/Desktop10.0.13；打包targets42–72核全部依赖存在与复制/hash；根exe/testDLL/MTP角色清单及反射准确无参Task协调方法已核。静态核精确filter840、optin async-shared-wic-v1及父子传播合同，未执行独立解析测试。
唯一远端task：`C:\CodingAgentSandbox\Tasks\3f8e0ea51f314bf79236878aee48c871`；完整回收到独立sandbox-results。趋势UID `6ca966b30adc413a9ecbe4217bac819f`，以下role路径均其original/candidate。
actual official=task根wpfgfx_cor3.dll，SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，版本10,0,1326,50504/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；candidate=task Native/wpfgfx_cor3.dll，SHA D3F5E40D2ABC0782D678C843537210EC203987A74C84616E20164703B3895B82。requestedEqualsActual=true；初始化candidate不是official actual证据。testSHA CFAE30E05EB5DD9F1F3FF90D587988B46438456D06AE4071B29B49696CDFC518，MVID2bbc6a6b-20c7-4faa-b0cb-417bef0341a5，.NET10.0.13 X64 pointer8，OS10.0.26200。

## 监督、硬门与真实清理
父监督全文04:07:07.0334096→04:07:43.0309884Z，**35.9975788秒**；Supervisor11864/父7588，exit0，HasExited/OutputCollected=true；无超时/取消/kill/drain timeout/SupervisorError。父TRX1–26全文total1/executed1/passed1/failed0，testUID1a0f2097-eccc-88e7-8188-9a94b56ac8a6，body **35.3436639秒**；diag同UID Passed、session3407049d-2126-4f54-9d3f-baf78da7f674 exit0/session35.7000351秒。
original3888完成监督04:07:25.124149Z后candidate8536完成04:07:42.9451754Z；两role Exited/exit0/reaped=true/forcedTermination=false/killFailed=false/cleanupErrors=[]，role360/round20/parent840/outer900原合同不变。原NT同步调用不可硬取消，此次正常退出不是其永不阻塞证明。
32轮关键business字段已逐轮定向核：completed/cleaned/disconnected/deinitialized=true，BeforeSourceClose0/AfterSourceClose0/FinalDisposals1/CallbackExited1/callbackEntries1，CallerOwners/Channels/Duplicates=0、ConnectionOwned/ManagerInitialized=false、WicDisposed=true，primary=null/cleanupErrors=[]。两role原samples各65行按四组全文读取，非只completed头；Source阶段owner仍存活、Cleanup/+1s归零，每轮回调进入/退出/disposal1，末累计16。无强制GC/加点/阈值/改预热。OS worker全部退出仍NotProven；callback退出不是OS线程退出。
父stderr及两role stdout/stderr读为空；父stdout中文出现解码替代字符，保留原件，不据此伪造内容。目录/名称搜索未呈现TRX但直接精确路径读取成功，没有因此重跑。详04读取台账；未声称32轮各689行全部事件逐字全读。

## 全部20固定类型点（不是全覆盖类型门）
每role10点与设计完全一致。下表H=BeforeHandles=AfterHandles=Enumerated（本次逐点相等，不保证无竞态）；C=Classified=Opened=Closed，Outstanding逐点0；B=BuffersOpened=BuffersClosed。全部Status=Partial，Reason/Error=null，CleanupErrors=[]，不是未知填0。
|点|O H/C/Unknown; Event/Thread/File; B; capture秒|C H/C/Unknown; Event/Thread/File; B; capture秒|
|---|---|---|
|Initial|245/180/65;75/7/48;181;.0045983|234/171/63;75/7/42;172;.0046854|
|r0 BeforeInitialize|245/180/65;75/7/48;181;.0003765|234/171/63;75/7/42;172;.0003838|
|r0 AfterSourceCloseTargetUpdate|291/210/81;82/9/53;211;.0004593|284/213/71;93/13/47;214;.0004777|
|r0 AfterCleanup|273/200/73;79/8/53;201;.0004266|284/213/71;94/13/47;214;.0004151|
|r0 AfterCleanupPlusOneSecond|273/200/73;79/8/53;201;.0004907|284/213/71;94/13/47;214;.0010947|
|r3 AfterCleanupPlusOneSecond|274/201/73;79/8/54;202;.0005587|291/220/71;97/16/48;221;.0009320|
|r4 AfterSourceCloseTargetUpdate|295/214/81;84/10/54;215;.0004576|299/228/71;101/19/48;229;.0004267|
|r4 AfterCleanupPlusOneSecond|274/201/73;79/8/54;202;.0003767|290/219/71;96/16/48;220;.0004916|
|r8 AfterCleanupPlusOneSecond|275/202/73;79/8/55;203;.0005001|302/231/71;102/21/49;232;.0004123|
|r15 AfterCleanupPlusOneSecond|275/202/73;79/8/55;203;.0003795|316/245/71;109/28/49;246;.0005309|
其余所有已分类类型完整记法：两role全部点WaitCompletionPacket9/IRTimer6/TpWorkerFactory3/Directory2/SchedulerSharedData1/WindowStation2/Desktop1；Initial和r0Before：IoCompletion4/ALPC Port2/Mutant2/Semaphore6/Section2，Key O10/C7。进入Source后Key O15/C14、ALPC Port5/Section4固定；IoCompletion仅Source点5、其余4；O Source点Mutant5/Semaphore8，其余2/6，C一直2/6。未列未知类型不当0，不把Types字典等同所有kernel类型。
自有duplicates总O opened1990=closed1990，C2124=2124；buffers O2000=2000、C2134=2134，逐点outstanding0，清理错误空。allocatedNativeBytes O249856,249856,280576,270336,270336,271360,284672,271360,272384,272384；C240640,240640,283648,283648,283648,290816,299008,289792,302080,316416。不是CLR总体内存测量。
class51原NTSTATUS逐点0，Capacity65536，ReturnLength O9816,9816,11656,10936,10936,10976,11816,10976,11016,11016；C9376,9376,11376,11376,11376,11656,11976,11616,12096,12656。class2/51非0status负模式检索无命中，capacity非1024/65536无命中；失败局部原文为DuplicateHandleRaceOrDenied/Win32Error50，非50负模式无命中。**原Calls/Failures完整文件保留，但长行工具截断，未声称每个调用/失败entry逐字全读。** Partial与Unknown（O65/81/73、C63/71）不能当0或唯一解释为竞态；关闭/复用始终可能，type不是owner/对象唯一身份/创建栈。
静态buffer上界仍为表64KiB→4MiB/最多5调用/16384entry、type1KiB→64KiB/最多3调用、累计native16MiB；snapshot2s/role10s仅返回后预算，无硬取消。此次没有触发上界不证明边界单测通过。

## 原65样本与描述量
130行均核，Warmup0–3/Measurement4–15；各phase一致比较，不混Source/Cleanup。descriptive核头与关键+1s PrivateBytes/Handles summary，不冒约3000行整份全读。
|测量轮4–15，AfterCleanupPlusOneSecond|official|candidate|
|---|---|---|
|原Handles首→末 / slope每轮|274→275 / .06993006993006994|290→316 / 2.132867132867133|
|原PrivateBytes首→末 / slope字节每轮|26136576→24862720 / -98690.68531468531|31309824→30318592 / 65621.93006993007|
|同固定type点r4/r8/r15 Event|79/79/79|96/102/109|
|同固定type点r4/r8/r15 Thread|8/8/8|16/21/28|
候选r4→r15同+1s点Event+13/Thread+12/File+1，classified+26且unknown71不变，与该窗口H+26相容；只证采到的句柄条目增长，不证明全是channel owner、活线程、runtime唯一根因或永久泄漏。r3→r4反而短降，不能挑点强称严格逐轮单调。official该固定窗口Event/Thread稳定、File+1。

## 旁路开销与扰动边界
capture十点合计O .0086240s/C .0098502s；capture+第一文件写入十条合计 **O .0280777s/C .0298133s**，首点 .0196778/.0194665s。write记录20条全文核；不含第二条记录自身写入耗时，此限制按授权接受，未改代码/重build。最终快照RoleSeconds O .0322673/C .0332526含此前完整写入及末次capture，不含最后写入，不冒完整扰动总时长。
**原HandleCount是在同点sidecar前采**；Initial candidate原233而类型前234，说明不是同一瞬时原子观测。sidecar自身查询/duplicates/分配/JSON/I/O可改变后续业务行为及GC/时序；旧无sidecar16轮不是本次无扰动反事实。即便分类查询完整也不能据此宣称无泄漏。

## 最窄假设与下一唯一动作
已观测候选同固定清理后点Event和Thread条目增长、已知业务owner归零、进程实际线程样本11基本稳定；最窄可证伪候选是NativeAOT Thread等待句柄与Monitor TLS事件的保活/终结合同，而非channel Event owner或创建栈已证。现在没有足以修改成功路径生产owner的确定证据。
**下一唯一动作：零执行静态核对实际.NET10.0.13 NativeAOT Thread/SafeWaitHandle、Join及Monitor.Condition TLS事件的创建/关闭/终结合同与候选引用链。** 不自动再跑趋势、不补快照、不增加测试或Native，不访问Thread私有SafeHandle；若静态证明最小产品owner缺口，再独立授权最小生产修正与适用验证。connection异常回滚缺口另案，不借此替代残余解释。
全部旧SHA/失败账、x86旧15A545未含event修正、KN/媒体旧证据不迁移；显示Unknown、HWloss/媒体音频趋势/full及完整替换Open。本次类型证据细化调查，不升级资源Green。
