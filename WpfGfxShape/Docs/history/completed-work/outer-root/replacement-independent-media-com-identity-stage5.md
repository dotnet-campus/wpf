# 独立媒体 stage5：COM identity 与状态接入

## 正式终态：LimitedComIdentityAndVideoValidated / CloseoutCompleted
仅关闭内部COM identity/状态/transport/DrawVideo有限slice，ProductionPlayerOpen/OverallOpen持续。下一直接生产任务events与Pause/seek-rate控制：先根据真实状态、MTA串行owner、取消/回调重入/关闭合同实施，不再泛选门；本轮不新开实现支线。

唯一一次本地回归日志62行已立即亲读并存WpfGfxShape/artifacts/independent-media-stage5/closeout-test-receipt.md，含真实绝对targetPath、准确四组OR filter、工具return、exit0及日志截断。实际19passed/0failed/0skipped/total19（33ms），不是18；旧16不是本轮执行数。未选择MediaFoundationResourceTests内系统播放case。本轮normalbuild0/tests1/implicitDebugCompile1/COMpublish0/publishImplicitCompile0/sandbox0/Native0，源码改动0、终端脚本0、再委派0。累计normalbuild2（首fail后Release3738warn0err）/tests1/COMpublish1隐式compile1/sandbox1/Native0；Debug总warning数因stdout前1546597字符截断Unknown，不套旧Release数。冻结产物，禁止再build/publish/sandbox/full追绿。

## 当前实证（已在回归前落盘）
源码冻结。本轮不新增生产能力、不改expected、不发布、不进沙箱。已有task fcb3c2cf845c4083913add0ef75f52ea的业务result.json 262行、supervision.json 40行、child.trx 26行、stdout 12行已分段亲读。真实父TRX total/executed/passed=1/1/1，failed=0，child ExitCode=0，TimedOut=false。桥接内部调用3个IndependentMediaIdentityTests方法加1个真实系统播放方法，不是4条TRX。

业务直接字段：40帧PTS0..39000000、步1000000；published=40、ended=true、Failure=0；两固定PTS5000000/25000000，2×1 stride8，像素0000FFFF0000FFFF/FF0000FFFF0000FF；close=0、finalReferenceCount=0、registryRemoved=true、state=Closed。采样在发布回调内，不是独立墙钟采样，不是40帧逐帧RTB/HW/物理显示认证。
40个query均序列化为{}：positionHr/position/videoHr/video原始字段=Unknown；源码每次强断言(0,timestamp,0,1)，结合桥接Passed支持断言Passed，不补写JSON或重发包。

## 证据位置与身份
原件根：WpfGfxShape/artifacts/independent-media-stage5/readback/Results/；业务independent-media-stage5/result.json；监督independent-media-stage5-supervision/supervision.json；准确TRX independent-media-stage5-supervision/TestResults/child.trx（枚举未显示不代表缺失）。
桥接SHA E7C9187E3708DE1B5B3CAC6A77F785D49198EF6ABFC6F9462092CDA1927F6DB3 / MVID844eedb1-0949-4cba-822e-4cd1137d5a82。
source-linked Tests SHA EF22CBCF00F32526448FD413AD4AE3D49AC776D453988F4AB8B42F8AC78A4307 / MVID0586a828-1c2c-4079-9150-f349a5e9b295；.NET10.0.13/X64。mfplat/mfreadwrite/mfsrcsnk来自SYSTEM32 10.0.26100.9278，mfcore 10.0.26100.1。系统DLL加载仅MF后端证据，旧6BDF Native文件仅fixture初始化，均不认证新NativeAOT或完整AV。

## ABI逐槽审查
权威src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_render.h:728..843，对照IndependentMediaIdentity.cs:253..276及Abi/MediaExports.cs。IUnknown为0..2，共24slots/stdcall；bool为1字节。IID IMILMedia=00000141-a8f2-4877-ba0a-fd2b6645fb94，IUnknown=00000000-0000-0000-c000-000000000046。
|slot|官方method|当前实际语义|
|---|---|---|
|0|QueryInterface|仅IUnknown/IMILMedia同指针且+1；拒绝IID E_NOINTERFACE并清零；空指针E_POINTER|
|1|AddRef|registry锁内+1，ULONG|
|2|Release|减至0注销registry，BeginClose后释放identity内存，ULONG；不等待异步close|
|3|Open|仅初始Closed接受，进入Opening；owner.OpenAsync完成才Opened，异常记录Failure/Failed；S_OK仅接受操作|
|4|Stop|E_NOTIMPL|
|5|Close|幂等请求Closing，异步owner.CloseAsync及pending完成才Closed；S_OK不是同步close完成|
|6|GetPosition|返回最近发布PTS（100ns），非独立连续clock|
|7|SetPosition|E_NOTIMPL|
|8|SetRate|仅Opened且rate=1启动Playing；EOF才Ended；0/Pause及任意其他rate均E_NOTIMPL|
|9|SetVolume|E_NOTIMPL|
|10|SetBalance|E_NOTIMPL|
|11|SetIsScrubbingEnabled|E_NOTIMPL|
|12|IsBuffering|out清零/E_NOTIMPL|
|13|CanPause|out清零/E_NOTIMPL|
|14|GetDownloadProgress|out清零/E_NOTIMPL|
|15|GetBufferingProgress|out清零/E_NOTIMPL|
|16|HasVideo|owner Width>0且Opened/Playing/Ended返回1，否则0|
|17|HasAudio|out清零/E_NOTIMPL，不是假成功无音频|
|18|GetNaturalHeight|owner尺寸或0|
|19|GetNaturalWidth|owner尺寸或0|
|20|GetMediaLength|out清零/E_NOTIMPL|
|21|NeedUIFrameUpdate|E_NOTIMPL|
|22|Shutdown|当前别名Close，完整shutdown语义未认证|
|23|ProcessExitHandler|当前别名Close，完整进程退出语义未认证|
除ref返回ULONG其余HRESULT；有out的空指针先E_POINTER。关闭后多数操作RO_E_CLOSED(80000013)，Close仍幂等；非法状态Open/SetRate返回8007139F。不存在独立Pause槽，Pause通常经rate0表达，本实现尚不支持。Paused仅enum未实现。

## COM引用与关闭风险边界
初始ref1；QI+1；系统case显式AddRef至2，把transport交资源；删除资源释放transport，末次Release返回0且registryRemoved=true为直接证据。owner已await Close完成后才资源删除/lastRelease，此必要路径已证。不等于所有native refs均0：backend individual native ref counts未instrument，晚lease/在途native阻塞取消/回调中lastRelease边界未证。
Release实际不调用同步Wait/GetResult/Join；BeginClose在_gate内调用async CompleteCloseAsync，await owner.CloseAsync后await旧pending，已完成路径可同步续行且Monitor可重入。owner.CloseAsync只cancel/CompleteAdding并返回_closed.Task；MTA finally清reader/Frames/队列/token后SetResult，不Join当前线程。系统case已closing，lastRelease直接幂等返回；本地无owner路径同步Closed。回调中lastRelease、并发QI/关闭、native ReadSample阻塞、finally异常导致closed未完成均未认证，不以无Join源码冒充所有边界已证；下一必须基于真实owner取消合同完善。

## 工厂与未完成API
SystemMediaSource.Create始终E_NOTIMPL/null，Resolve恒null；默认工厂未切。空output E_POINTER；空factory E_INVALIDARG并清零；合法factory（canOpen=0/1）E_NOTIMPL/null，不保留event client。events proxy/通知包/退订/关闭后无通知、Pause/Stop、seek/rate/scrub、audio输出及同步、length/progress/buffering、nativecancel/flush-drain、完整shutdown/exit、NativeAOT/真实WPF/双arch/full仍Open。合法外部provider的QI/transport/Begin-End合同保留，不等生产主动官方回载；原版wpfgfx加载/副本/fallback及旧委托许可持续撤销。

## 最终执行账
继承用户已执行normalbuild2：首fail、后Release成功3738warnings/0errors；COMpublish1/implicitcompile1；sandbox1；Native0。原始normal失败日志本次未提供，不能捏造错误码或冲销失败。本轮新增normalbuild0/tests1（19passed/0failed/0skipped，implicitDebugCompile1）/publish0/sandbox0/Native0；不因文档变更重build。原预算剩normal1/tests2，本轮只用tests1，不追加执行。旧16回归未执行的计划文字不是stage5实际结果；不跑原full，旧full红、资源Unknown、memoryGreen=null保留。

## 历史预注册（非当前执行账）
原草稿Open，预算normal3/tests2/publish1/sandbox1/Native0且当时消耗0；拟IUnknown/QI/ref、双实例、unsupported、关闭和provider/resource/sample/factory回归及固定AVI系统case。旧草稿称lastRelease只请求Close、不自join，应以上面实际源码风险审查为准；完整替换持续Open。最终状态及实际计数已在顶部及最终执行账补齐。

## 同步完成
六入口已修改并逐份回读：WpfGfxShape/Docs/next-session-handoff.md、handoffs/current-work-item.md、handoffs/current-stable-baseline.md、replacement-issues-and-plan-status.md、final-acceptance-remaining-matrix.md、wpfgfx-complete-replacement-validation-plan.md；current-execution-rules.md亦已更新。本报告及artifact receipt均回读确认。历史failbuild/旧16未跑计划/旧full红/委托许可撤销原文保留并由stage5顶部权威覆盖，没有未同步文档尾项。
