# AX 生命周期夹具越界修复及托管全量

## 最终状态

**生命周期夹具确定越界已最小修复；x86 最小独立两次通过，双架构 lifetime/interface 族通过，x64 无过滤全量4254/4254；x86 无过滤全量仍无完成 TRX，双架构托管全量未闭。** 本文先于源码修改创建，结束更新并回读。不把局部通过或进度计数冒充 full。

## 修改前登记与范围

已读 next-session-handoff、current-execution-rules、wpfgfx-complete-replacement-validation-plan、AV ownership-fixture-fix、AS x86-managed-diagnosis。初始事实：AV 已修 ownership 4→5槽，最小两次及 interface 双架构成功；AV x86 full 无完成TRX。当前生命周期同类夹具仍4槽/+2/[2]越界。

本轮仅修改本文与 Tests/WpfGfxShape.Tests/Direct3D9ObjectsLifetimeTests.cs。未改生产、曲线、入口、项目、其他代理文件、golden/阈值/skip；未禁并发，未新增测试/Host/项目。复用 OwnershipFixtureAssemblyIdentityTests 原样记录实际身份，保留其 av-ownership-assembly-identity.json 文件名。未用终端、shell或直接间接脚本；只用专用读写/搜索/构建发布工具及Windows沙箱正式C#测试exe。未清理共享，未重发 NativeAOT。

## 最小修复与有界兄弟核查

Direct3D9ObjectsLifetimeTests.cs:143 唯一源码修改：NativeMemory.AllocZeroed 的 sizeof(nint)*4 改为 *5。槽0对象vtable、槽1 GCHandle、vtable从槽2开始；vtable[2]写槽4，需要至少5槽。原x86分配16字节却写16..19，x64分配32字节却写32..39；现合法范围0..4。调用约定、释放顺序断言和既有DoNotParallelize均不变。修改后已回读143-150行。

搜索两个现有正式测试项目下 memory + 2，亲读兄弟布局：ownership现5槽最高4合法；BitmapLock 11槽最高10合法；DirectWriteFactory 10槽最高8合法；DirectWriteDisplaySettings 24槽最高22合法。Surface命中为 memory+20，不属于此次+2三槽IUnknown相同分配模式，未改。并非宣称全部unsafe布局均已审计。

## 构建及正式包

SDK global.json 10.0.107/latestFeature；Tests net10.0、Nullable enable、既有LangVersion latest，MSTest4.0.1，生产ProjectReference不变。读过局部Directory.Build.props/targets与既有SandboxTestEntryPoint。

源码修改后，每架构先通过专用发布工具执行正式Tests项目 -t:Build -c Release -r win-<arch>，使用独有 WpfGfxShapeArtifactsRoot。再同项目 Release 自包含发布：RuntimeFrameworkVersion=10.0.13、BuildProjectReferences=false、EnableMSTestRunner=true、DefineConstants=SANDBOX_TEST_ENTRY_POINT、StartupObject=WpfGfxShape.Tests.SandboxTestEntryPoint。第一步正常依赖构建生产，第二步入口宏不传播到生产。四次均执行成功，各58行日志；存在既有CA1416/MSTEST0044警告，不宣称零警告。

独有ignored根：d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/ax-lifetime-fixture-full-20260721/。包位于 managed-{x86,x64}/publish/WpfGfxShape.Tests/Release/win-{x86,x64}/，运行正式WpfGfxShape.Tests.exe。未发布ComAcceptance或无改NativeAOT，不把托管结果外推为两项目四格验收。

## 六次实跑及回收

全部保持Workers=4、Scope=MethodLevel。实际banner与六份identity均为.NET10.0.13。传入 --report-trx --report-trx-filename 与 --diagnostic --diagnostic-verbosity Trace。既有入口将结果目录补为 Path.Combine(AppContext.BaseDirectory,"TestResults")，实际均为任务内绝对 C:\CodingAgentSandbox\Tasks\<task>\TestResults；身份在同根绝对Results。未限制outputRelativePath，整个任务完整回收到下表目录，保留exe/DLL/runtime、TRX/diag/Results。

|本根下回收目录|任务号|时限秒|实际完成结果|
|---|---|---|---|
|min-x86-1|2fca4de47e5c4f0d943012fb1332f84c|120|ax-min-x86-1.trx Completed 2/2|
|min-x86-2|bbebd9eb55ba41859d21bf3164ac5e01|120|ax-min-x86-2.trx Completed 2/2|
|family-x86|8aac631d92984360a71c5b9ce8d812d5|180|ax-family-x86.trx Completed 22/22|
|family-x64|b0d3d672ff7b424e82b3302ad40d75a1|180|ax-family-x64.trx Completed 22/22|
|full-x86|366113f411974057a8bfa13e2189f7d3|1800|无parent完成TRX，不报全量完成数|
|full-x64|40c7e59342bc483e846fa30fca372746|1800|ax-full-x64.trx Completed 4254/4254|

最小filter：FullyQualifiedName~WhenBaseFactoryIsDisposedThenOwnedResourcesAreReleasedInNativeOrder|FullyQualifiedName~OwnershipFixtureAssemblyIdentityTests。各2为业务1+身份1，并非两个业务测试。两次使用不同任务独立进程。

族filter：FullyQualifiedName~Direct3D9ObjectsLifetimeTests|FullyQualifiedName~Direct3D9DeviceInterface|FullyQualifiedName~OwnershipFixtureAssemblyIdentityTests。各22为lifetime6+interface15+身份1。两个full均完全无filter。

已亲读 parent TRX：最小各29-30行、族各149-150行、x64full25533-25534行。total=executed=passed分别2、2、22、22、4254；所有failed/error/timeout/aborted/inconclusive/passedButRunAborted/notRunnable/notExecuted/disconnected/warning/completed/inProgress/pending均0。不是累加局部或child结果。x64full runner耗时3s358ms。

## 实际加载包身份（六份JSON逐份回读）

每次路径为 C:\CodingAgentSandbox\Tasks\<上表task>\WpfGfxShape.Tests.exe，以及同根 WpfGfxShape.Tests.dll、wpfgfx_cor3.dll；JSON实际Location与ProcessPath均已读，不用预期发布路径代替加载身份。FullName两程序集均Version=1.0.0.0/Culture=neutral/PublicKeyToken=null。x86四次身份完全一致，x64两次身份完全一致。

|架构/程序集|SHA256|MVID|
|---|---|---|
|x86 Tests|871338D4ABDC4A5D1703BC22940293F4F68216B28760F3667506462C0200DC01|ac2724df-e5a7-483b-b531-fbde8037e8de|
|x86 wpfgfx_cor3|5DCF389A383DDA8F93B431337DF73F63806C1A62C5EB41CFEFED52F281DC6011|24804a9c-7e6a-476e-bab8-3510a846cbe0|
|x64 Tests|F7EFBF113CEBA57B950FCDBAD0E21B78654CE2DAEFA78D2B8CDF07DDF08B5EC2|deb10e96-de94-4c4b-b1d7-53952a8f4a80|
|x64 wpfgfx_cor3|1E2044AFE0D94BE358447C52CD27DF999DA7E427B5DEF215D6D383304A35D47D|1f681ee5-4a6d-40c4-9d3b-d2751a0906dd|

x86 Architecture=X86/PointerSize=4，x64=X64/8。每份JSON均在对应回收目录Results/av-ownership-assembly-identity.json。SHA用于归属，不作为额外前置阻塞。身份绑定本次构建快照，不声称后续并行源码修改自动受此full覆盖。

## x86 未完成边界与下一最小动作

full-x86/TestResults目录亲列只有部署目录与 log_261019134903890.diag，无顶层ax-full-x86.trx。trace末行39315（已亲读）时间2026-10-19T13:49:06.5670573+00:00，仅 Executing test WhenBeginSceneFailsThenHResultIsPreservedAndSceneIsNotCommitted；前行参数-2005530520，来自Direct3D9DeviceSceneTests。前一参数-2147467259有完成记录。此末名只是终止可见信息，不能证明其根因，也不能由trace进度报全量计数。

接口已返回并完整回收，但没有数值退出码、异常类型/栈、dump或超时标志。不能把1800秒上限写成已观测timeout，不能声称访问冲突。该无完成事实已在x64full之前即时写入本文；未再次重跑x86full。读取trace定位末尾时数次指定超出文件行范围，随后按实际行范围纠正，属于读取探测，不是额外运行。

下一最小动作：由主代理协调Direct3D9DeviceSceneTests所有者，先只读核查该族的非托管分配/vtable边界及共享状态，再使用本次已知身份x86正式包对scene族或末参数做一次有界隔离，复用身份测试；有可疑分配则最小修复后再定向复验。应先获取可取得的退出/崩溃终止信息，不盲归最后测试、不无限full重跑、不禁并发。此次生命周期修复不能声称唯一根因闭合。最后集成仍需最终源码两项目×两架构完整适用full；官方差分、HW/恢复/资源等开放门未由本包关闭。
