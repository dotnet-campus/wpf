# Stage12 独立媒体关闭、取消与回调生命周期

## Stage12-final 最高优先终态：LimitedLifecycleSliceValidated / CloseoutCompleted / OverallOpen
本节覆盖下方旧“当前/下一/预算”，下文保留原时间顺序及失败账。最终执行明细与完整Native参数：WpfGfxShape/Docs/handoffs/stage12-final-execution-receipt.md。此前final授权已normal1 Release exit0/3839warnings/0errors、tests1 72passed/0failed/0skip、COMpublish1 exit0、sandbox1；本接续normal/tests/COMpublish/额外sandbox均0，仅Nativepublish1。未用终端/脚本/委派，未改生产源码/SDK/TFM，未启动MP4或新沙箱。

### 当前系统证据（不是旧eb697 task）
task617b7b66768c4c57b46fe39b4f592539，sourceDirectory=WpfGfxShape/artifacts/stage12-final-managed/clean；exe=WpfGfxShape.ComAcceptance.exe；arguments=[--supervise-exit,Results/stage12-final-lifecycle,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenLifecycleClosesThenCallbacksAndMtaCleanupComplete]；timeout300，整任务readback=WpfGfxShape/artifacts/stage12-final-managed/readback。原工具return逐字不可恢复，Unknown，不编；参数由预注册与supervision/identity复核。stdout所列准确Results/stage12-final-lifecycle/TestResults/child.trx已全文26行读完（目录漏列不表示missing），total/executed/passed1、failed/notExecuted0。监督40行exit0/exited，无timeout/cancel/kill/drain错误；stdout12、空stderr、identity1、diag50–65含完整最终session exit0亲读。
三AVI callback-close/callback-release/external-close各107行分段完整亲读；missing failed-close/failed-release各21行完整复核，与前次全读引用分清。三AVI均passed=true，accepted0且pendingAtAcceptance=true；真实生产proxy callback内Close/最后Release0，或callback阻塞时两外线程Close均0；固定callback-enter→close-accepted→callback-exit→owner-completed。6帧终止，250ms latePackets0，无新frame lease准入(afterClose80000013)。关闭完成后调用lease.Source slot7 MILCopyPixels，8bytes=0000FFFF0000FFFF（两红像素），不只是Source非零；finally已借lease.Dispose，随后identity最终refs0/registryRemoved/descriptorDisposed。lease自身最终底层ref计数无单独字段，Unknown，不把identity refs0冒作lease/native全0。
ThreadExited=true；currentType.Release0/nativeType.Release0/attributes.Release1；7次Unlock0/buffer.Release1/sample.Release0；reader.Release0→MFShutdown0→CoUninitialize，sampleReleases7/bufferReleases7/shutdownCount1/uninitializeCount1/readerFinalReferences0，nativeReferences=Unknown。missing两case release=false/true、callbackResult0、failure80070002，仅Failed8 packet，stateClosed/threadExited/descriptorDisposed/registryRemoved/refs0；cleanup=null，不挪AVI reader清理计数。失败Open退订及事件屏障在本次最终源码72pure/系统组合覆盖；不外推全部竞态或强取消。
业务source-linked SHA A108E5AA256D441D543A762E7DF896911E173468342E9A7690BBFF95685000AA/MVID741083a0-7d00-47c2-af84-2b79d3a1f435；wrapper FB944B71BA17AE2F77610AD6F5DF78070532BE0FD2276341BB776D9F787EDF15/MVIDa84e63a8-c281-44b8-9a4f-9aee6588186f。.NET10.0.13/X64/pointer8；旧fixture6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403只PE初始化，不是当前Native媒体。

### 唯一当前NativeAOT实际发布
精确绝对生产项目d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj；Release/win-x64/net10.0/self-contained，RuntimeFrameworkVersion10.0.13，PublishAot=true，WpfGfxShapeManagedAcceptance=false、WpfGfxShapeRuntimeHeapDiagnostic=false、WpfGfxShapeDiagnosticAbi=false。保留默认AssemblyName wpfgfx_cor3/NativeLib Shared与分析器；无no-build/原wpfgfx委托。输出绝对WpfGfxShape/artifacts/independent-media-stage12-final-native-x64。实际return成功59行，日志exit0/Generating native code/lib/exp/准确publish输出；列目录实证wpfgfx_cor3.dll及.pdb存在。已读日志1–12/46–59；stdout615162字符、前595162不可见，CA1416可见；aggregate warnings/errors Unknown，不报虚假零汇总。Native SHA Unknown；诊断exportabsence仅源码条件，binary Unknown，不增runner/沙箱补hash。编译成功不是Native媒体导出运行认证。
source snapshot仅本次生产项目默认编译输入及项目/局部Directory.Build配置、global.json和关联生命周期测试证据范围；非全repo加密freeze。旧4A18 Native/负向8pass/历史失败预算仍原时序保留，不是当前产物身份。final累计normal1/tests1/COMpublish1/sandbox1/Native1；接续0/0/0/0/1，唯一Native额度已用尽。

### 最终矩阵与下一直接生产任务
|合同|最终状态|
|---|---|
|真实AVI回调Close/最后Release/双外Close、callback drain、MTA退出|有限Passed|
|关闭后250ms无新通知及拒绝新lease|固定窗口有限Passed，不代表无限期/所有竞态|
|已借lease跨Close实际MILCopyPixels红内容及finally Dispose|有限Passed；lease/native最终底层refs Unknown|
|真实missing Failed内Close/Release及描述符退订|有限Passed，cleanup null不伪造|
|最终源码72pure、当前默认prod NativeAOT编译|Passed，非Native媒体ABI运行|
|官方Shutdown/ProcessExit slot22/23|Open：源码table[22]=table[23]=table[5]仍Close别名，未修|
|在途MFCreate/ReadSample/Seek强取消、ShutdownExit、MP4中断|Open/NotProven，不承诺打断同步系统调用|
|公开factory/VolumeBalance/audio/AVsync/Native媒体ABI/完整WPF|Open；factory继续E_NOTIMPL/null|
已亲读官方WmpPlayer.cpp850–935：Shutdown用于断引用环、代理CWmpStateEngine.Shutdown；ProcessExitHandler调用MediaEventProxy.Shutdown。fakepp.cpp805–875同样Exit禁事件代理，Shutdown另负责线程终止/缓冲清理。不能用Close验证代表二者全合同。下一个直接任务是实现生产slot22真实Shutdown断环/清理语义及slot23 ProcessExitHandler代理Shutdown分离，并验证回调准入与引用释放；不是再泛静态选门。本轮不实现新支线。
所有原版回载/loader/委托/fallback禁止，合法opaque provider保持；旧full红、endpoint0/服务running不等有声、AVI缺16samples、resources/nativeRefs Unknown、MemoryDescriptiveOnly/memoryGreen=null完整保留。CloseoutCompleted仅本stage已验证生命周期slice与文档/编译闭环；完整替换仍Open。

## 以下为原Stage12执行历史（非当前终态）

## 终态：LimitedCallbackCloseAndReleaseValidated / OverallOpen
本轮亲自修改、构建、定向测试、发布并在Windows沙箱执行；未使用终端、脚本或委派。正式草稿先于本轮生产修改建立。本轮确实关闭了固定AVI真实生产回调Close/Release0及外线程双Close的有限生命周期关口；不是所有取消/Shutdown/NativeAOT媒体认证完成。

## Before / After 与状态机
已有stage12源码含owner队列关闭取消与锁外事件派发，但identity Release0立即free storage，Close completion不等待已准入事件退出，owner completion在MTA线程finally内部就完成。
本轮最小修正：MediaEventProxy记录callback-inflight及已入队item，Dispose取消尚未准入通知，已准入回调锁外执行并持proxy ref；completion等item.Dispose释放其ref且owner ref已释放。重复Dispose幂等。identity Close等待owner、既有operation和事件drain；Release0先移除registry并请求关闭，内部保留释放task，在completion后free identity storage。owner CloseAsync的completion异步等待清理并join已退出线程，回调内不等待自己；不关闭Thread私有handle、不GC强制。ClosedReader只作内部真实清理观察，不新增public测试接口/InternalsVisibleTo/工程/包/SDK/TFM。
状态：initialClosed→Opening→Opened/Paused/Playing/Seeking/Ended/Failed→Closing→terminalClosed；_closing区分初始与终态。CloseAccepted=原子拒绝新操作/发布并请求关闭，S_OK不代表已清理；CloseCompleted=异步Completion完成，已准入callback退出、队列事件ref释放、MTA reader/MF/COM清理与线程退出。COM Release返回refcount，不是HRESULT。不得在callback中同步等待Completion。已关闭新操作80000013，重复Close在identity仍有ref时幂等。
同步SourceReader实际slot9 ReadSample传输出sample指针、无异步callback。Seek的slot10 Flush和SetCurrentPosition仅创建MTA线程调用；Close依次reader.Release→MFShutdown→CoUninitialize。未增加跨线程Flush/Release或Task.WhenAny丢弃backend。未开始owner命令结算取消；在途Create/ReadSample/Seek必须返回后才能完成关闭，不能保证中断阻塞系统调用。

## 本轮执行与原返回
执行账：WpfGfxShape/Docs/handoffs/stage12-execution-receipt.md。
- normal1：绝对Tests csproj，Release/win-x64/net10.0；原return成功62行。1–62全部读，exit0/3826warnings/0errors/22.87s；stdout3200167字符，前3180167不可见。
- tests本轮1：同绝对Tests项目；filter为FullyQualifiedName~IndependentMediaDurationTests|FullyQualifiedName~IndependentMediaIdentityTests|FullyQualifiedName~GeneratedMediaPlayerResourceTests|FullyQualifiedName~SystemVideoSampleTests|FullyQualifiedName~MediaCreationUnavailableTests|FullyQualifiedName~IndependentVideoFrameLeaseTests|FullyQualifiedName~MediaComEventBridgeTests|FullyQualifiedName~IndependentMediaStatusTests|FullyQualifiedName~IndependentMediaScrubbingTests|FullyQualifiedName~IndependentMediaLifecycleTests。原return成功62行，全部读取，exit0/71passed/0failed/0skip/45ms=原68回归+本轮新增3有限函数指针case。隐式Debug编译1另计；stdout1600601，前1580601不可见，warnings总数Unknown。此运行早于后续owner线程completion修正；最终源码在新系统包重新编译并执行三项pure，但没有将原68再次全跑冒通过。
- normal2：绝对COMAcceptance csproj，Release/win-x64/net10.0-windows，原return成功64行，亲读1–9/55–64；exit0/3991warnings/0errors/10.27s，stdout3329244，前3309244不可见。
- COMpublish本轮1：绝对COM项目Release/win-x64/net10.0-windows，self-contained=true，RuntimeFrameworkVersion10.0.13、MicrosoftWindowsDesktopAppRuntimeFrameworkVersion10.0.13、PublishAot=false、WpfGfxShapeManagedAcceptance=true、ComAcceptanceSandbox=true、AotRuntimeIdentifier=win-x64；AotDllPath绝对stage8-r1/clean/Native/wpfgfx_cor3.dll。新输出independent-media-lifecycle-stage12-r1/publish与clean是绝对兄弟目录，不相等不互含。原return成功60行，全部读：exit0，stdout1665205，前1645205不可见，warnings总数Unknown。实际clean DLL API确认无参Task bridge。

## 本轮唯一系统执行与完整业务证据
沙箱task eb697a12a44e47e3893aee84a898e97d；sourceDirectory=WpfGfxShape/artifacts/independent-media-lifecycle-stage12-r1/clean；exe=WpfGfxShape.ComAcceptance.exe；arguments=[--supervise-exit, Results/independent-media-lifecycle-stage12-r1, 120, FullyQualifiedName=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenLifecycleClosesThenCallbacksAndMtaCleanupComplete]；timeoutSeconds300。工具原return含supervisor-before-child topology及回收成功路径，不自行提供child exit；随后实际读证。
整任务本地WpfGfxShape/artifacts/independent-media-lifecycle-stage12-r1/readback。Results/independent-media-lifecycle-stage12-r1/TestResults/child.trx全部26行，stdout12行、空stderr、监督40行、aot-identity单行已读。TRX total/executed/passed=1，failed/notExecuted=0；监督childexit0/exited，无timeout/cancel/kill/drain错误。一个组合，不把三种系统case或pure调用数算成额外TRX。
Results/independent-media-stage12/callback-close.json、callback-release.json、external-close.json各104行均分段完整亲读。三case都passed=true/accepted0/pendingAtAcceptance=true/closeFrames=publishedFrames=6/threadExited=true/stateClosed/references0/registryRemoved=true/descriptorDisposed=true，新Frames.Acquire=80000013。固定PTS阈值5000000触发第六帧通知，实际JSON未独立记录该触发PTS，不把源码阈值冒充独立观测字段。固定信号顺序均callback-enter→close-accepted→callback-exit→owner-completed，无随机Sleep。
callback-release是真实生产COM事件proxy中的identity最后Release0，不保留额外identity测试ref；external-close在callback阻住时两个Task线程真实Close，源码断言两者0，再放行callback。三case COM包均Opened1/NewFrame13×6，无后续新帧；20bytes、HR0、thread10。已借SystemVideoSample跨关闭Source仍非零并最终Dispose；没有关闭后像素读回，不扩大为全面lease渲染认证。
三case真实cleanup均currentType.Release0/nativeType.Release0/attributes.Release1；7次Unlock0/buffer.Release1/sample.Release0；reader.Release0、MFShutdown0、CoUninitialize；sampleReleases=bufferReleases=7，shutdownCount=uninitializeCount=1。底层buffer返回1是实值，nativeReferences仍Unknown，不能填全0。MTA ThreadExited为join后观察。
监督wrapper SHA B271821DDEE1A24EC22BDF419CDCE277B761A01FA0F55A7BF9FCBB9E256DE64C，MVIDfb5857ad-1d7f-46c1-8458-27fe3160b14e；业务source-linked SHA FAB80817C67A0DF0B97AF0CEEA72B81B476C728856DEE724C8CE592068FA86C2，MVID2411f677-6f75-4628-9760-af6c7798a37e。实际.NET10.0.13/X64/PointerSize8。diag已读1–25及45–60可见部分，尾59–65另补；runtime location为任务内System.Private.CoreLib.dll/IsDynamicCodeSupported=true；未记录本轮MF模块版本，Unknown不搬旧版本冒新观测。fixture SHA6BDF…只初始化，非本轮Native媒体代码。

## 继承NativeAOT与预算身份（不改写时间顺序）
开始发现既有stage12账记录tests1=68passed、Nativepublish1、COMpublish1；随后实际发现旧Native沙箱task5489753d32dc450fa61cbf0410574fc4。已读其监督40/stdout12/identity、TRX结果与counter：8passed/child0，仅factory E_NOTIMPL/null、null参数和artifact身份负向。TRX68行曾宽读取截断，未称全文亲读。实际Native SHA4A18E44182D1D2241FB9D2BB4D57959580D00FDEA9A86D4A1A29FBC6B76A91DD由该程序identity与包aot.sha256支持，不是文本hash二进制；wrapper21E491…/MVIDd7e19719-d850-4e7e-b853-db72519cd7bc。旧编译日志Generating native code与lib/exp成功仅既有账引用；ILC/runtime-source全部配置本轮未独立重核。
既有Native发布发生于本轮新增生命周期强门之前且不含本轮修正，绝不能认证为本轮最新源码Native编译或媒体ABI运行。本轮未再Nativepublish，保守不突破用户总1次预算。下一需新授权预算对最终源码默认prod win-x64 NativeAOT重新编译认证，诊断全false；这不是本轮已完成项。
总预算保守合并：normal2/3（本轮2）、tests2/2（继承1+本轮1）、COMpublish2/2（继承1+本轮1）、sandbox2/2（继承1+本轮1）、Nativepublish1/1（继承1+本轮0）；implicitDebug2、publishCompile3独列。剩余normal1，其余0。无编译失败、无原样重跑追绿。

## 最终矩阵与下一直接工作
|门|终态|
|---|---|
|回调内Close/Release0、外线程双Close|固定AVI/真实生产proxy有限Passed；三case一个TRX|
|callback退出/事件ref drain/MTA cleanup与线程退出|本轮实际有限Passed，未知全底层refs仍Unknown|
|CloseAccepted与Completed分离|固定信号实际证明pendingAtAcceptance=true|
|framelease跨Close|持有Source非零及最终Dispose已执行；关闭后像素读回NotProven|
|late-event、失败Open退订、在途Open/Read取消|独立确定性强门未补齐；同步阻塞系统调用强取消NotProven|
|Shutdown/ProcessExitHandler slots22/23|仍Close别名，严格有限“关闭请求接受”；不承诺同步完成/官方进程退出完整语义|
|最新源码NativeAOT|NotCompiled；继承旧编译/负向8pass不是最新修改认证|
|用户MP4连续播放关闭|本轮NotRun，不用固定AVI代称MP4|
|公开factory|继续E_NOTIMPL/null；Volume/Balance真实输出、其他rate、Shutdown/Exit合同、完整取消/错误退订/最新Native媒体ABI/WPF尚缺|
下一直接实现并验late-event与失败Open退订/在途OpenRead有界取消强门、关闭后lease像素读回及精确Shutdown合同；随后最终源码NativeAOT默认生产编译认证与实际新导出分层验收，不能仅再选门。保持原wpfgfx生产loader/委托/fallback删除，合法opaque外部provider仍兼容。
旧full红、MemoryDescriptiveOnly/memoryGreen=null、所有音频端点状态0/服务running、WASAPI未有声验、AVI音轨缺16samples、audio/AVsync、全底层refs未知与完整WPF/整体替换Open全部保留。不得以本轮有限关闭绿冲销历史。业务失败有根因才修复；未完成强门不是Passed，未用额度不挪用。
