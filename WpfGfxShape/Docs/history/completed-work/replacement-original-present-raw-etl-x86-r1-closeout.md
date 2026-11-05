# original-present-raw-etl-x86-r1 原件回读与最终收尾

## 1. 终态与本次边界

Publish=Passed（依据既有发布记录），Sandbox=Completed，原体=Failed.NativePrerequisitePixel，RawCollector=Executed.SuccessfulControllerPath，ETL结构/目标事件=Unknown，Readback/Closeout=Completed。完整替换及原最终矩阵Open，根因Unknown。本次接续仅亲读文件、静态核必要源码和写文档；源码/配置/build/publish/沙箱/测试/Native/独立发现/重试/终端/脚本/再委派全部新增0。未读取ETL为文本，未解码，未重采；不把本文件当新运行。

## 2. 精确路径、交接与证据完整性

工作区 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape`；artifact根 `WpfGfxShape/artifacts/original-present-raw-etl-x86-r1`（下文R）；实际回收根 `R/readback`，并非工作区根下另一个artifacts。执行任务b37ffafbac3a469caad8ba5594b609c9由用户交接；原件中的 `C:\CodingAgentSandbox\Tasks\b37ffafbac3a469caad8ba5594b609c9` 独立支持该ID。并未取得原沙箱工具消息全文。

已亲读R/execution-ledger.md全部6行、01-publish-return.md全部4行、02-preflight.md全部6行、03-sandbox-return.md全部4行；stage1报告全部41行、当前规则27行、continuity-recovery-and-closeout全部66行。01/03存档的即时返回只有摘要：发布“执行成功/60行日志”、沙箱“执行完成/保存路径”，无数字exit/task/超时字段，不补造详细原返回。02记载既有60行日志回读exit0，但stdout53305字符仅尾20000可见、前33305缺失；本次不声称重获原发布全文或warning总数。stage1 normalbuild stdout106029亦仅尾20000可见、前86029缺失。

`R/readback/Results` 下已读完：aot-identity.json 1行、hw-module-Native-190405739b6b443285fc0dccf18f98a1.json 10行；`original-present-raw-etl-x86/` 内 supervision.json 40行、stages.jsonl 21行、stdout.txt 43行、stderr.txt空、Diagnostic/log_261028064834438.diag全部120行、TestResults/child.trx全部64行；`WpfGfxShape-OriginalPresent-8b2f6187eae44b42b5943845c2b10127/` 内setup.json 1行、controller.json全部119行。controller按1–65/66–119读取；TRX初次1–35截断，补读1–22/23–30/30–64；diag按1–25/26–45/46–65/66–80/81–100及101–111/112–120补齐截断尾段。不以截断返回冒全文。

首轮递归目录与名称搜索均漏报TestResults/child.trx；随后按supervision/stdout精确路径直接列TestResults确认并读到原件。因此漏项不是不存在的证明。`original-present.etl`二进制保留原位；大小/SHA下文来自controller的采后记录，本次没有重新计算文件哈希或解析结构。未审核全部包文件、所有模块/进程/会话或线程。

## 3. 执行账与监督、单体选择

|阶段|normal COM build|COM publish|沙箱|实际原体|Native|独立发现|重试|
|---|---:|---:|---:|---:|---:|---:|---:|
|stage1既有|1（exit0/134 warnings/0 errors/5.94秒）|0|0|0|0|0|0|
|本次r1既有执行|0|1|1|1（失败）|0|0|0|
|本次接续回读收尾|0|0|0|0|0|0|0|
|本两阶段合计|1|1|1|1|0|0|0|

其他paint、87d、KN、HP/HQ/HT等历史账独立保留，不累入、不冲销、不拼接为本次成功。运行内部MSTest发现1822项是本次执行内枚举，不是另一次发现任务，也不是1822个体。

01记录的发布合同：OriginalPresentSystemTraceDiagnostic/win-x86/net10.0-windows self-contained，NETCore/Desktop10.0.13，既有postfailure-source-rebuild-x86-r1/native/wpfgfx_cor3.dll，输出独立publish/clean，不执行publish根。03记录唯一沙箱源R/clean，正式WpfGfxShape.ComAcceptance.exe，`--supervise-exit`、Results/original-present-raw-etl-x86、120、精确FQN `WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive`，任务上限180秒、整任务回收R/readback。

supervision实际：2026-10-28T06:48:34.2950096Z至06:48:36.4001238Z，SupervisorPid6524/子Pid2748，Reason=exited、HasExited=true、ExitCode2/0x00000002；TimedOut/Cancelled/KillAttempted均false，OutputCollected=true，两项drain timeout均false，SupervisorError=null。子参数为精确FQN filter/TRX child.trx/diagnostic同步写/no-progress。监督文件不是全部系统进程或线程收敛认证。

TRX：run120008a1-8840-4cd8-854e-8b95233950b0，test/业务UID `1eded079-5019-809c-bf7c-07ef4caf0e29`，execution661b7bab-52ae-4367-b8f4-2f7a68e3f92a；total1/executed1/failed1/passed0/error0/timeout0/aborted0。diag同UID InProgress→Failed、MTP SessionUid512dc718-3b4f-4b86-bb21-881aedca20fd最终exit2；不是collector错误造成的会话级伪失败。stdout部分中文乱码保留，不修造原输出；TRX/diag可读中文独立支持错误；stderr为空仅说明该文件无内容。

## 4. 身份：文件预检不替代实际加载

测试SHA256 `92D1762F6AF33028D97B99221657A5908D7AB7CF3781978A4D79790A0C65D53C`，MVID `b6b58a9c-58eb-46c2-b1e3-74df98948e53`，.NET10.0.13/X86/pointer4，supervision与setup/controller一致；UID见上节，不与旧testSHA/MVID混绑。

clean/aot.sha256预检及aot-identity为文件/配置身份，并不单独证明加载。此task另有hw-module原件：Requested=Actual=`C:\CodingAgentSandbox\Tasks\b37ffafbac3a469caad8ba5594b609c9\Native\wpfgfx_cor3.dll`，SHA256 `15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`，FileVersion1.0.0.0，ProductVersion1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a，testSHA同上。必要源码VisualCache 42–55及HardwareClosure 23–42显示NativeLibrary.Load后以真实module调用GetModuleFileNameW并核Requested/Actual、哈希实际路径。因此本task实际候选模块加载Observed，不应沿用旧paint“NotObserved”。但加载/导出解析不等于产品连接、shader、resize已执行，更不证明文件未被外部替换或全部依赖模块身份。

## 5. collector实际成功路径与未证明项

setup为Start前快照：PropertiesSize120/LogFileNameOffsetField108/LoggerThreadOffset104，ProcessId2748/TID4736/QpcFrequency10000000，Window0x58C0632/Device0xCC73E60/Extended=false；初始handle空、零GUID、null时标/统计是预落盘状态，不是Start失败。setup序列化/文件写入已实际到达。

controller最终owned session `WpfGfxShape-OriginalPresent-8b2f6187eae44b42b5943845c2b10127`，64位handle0x17，GUIDb92081d2-cf75-11f1-8172-00155d722a02。仅D3D9 GUID783aca0a-790e-4d7f-8451-aa850511c6b9，keyword0xf/level6，顺序文件16MiB请求上限，64KiB/min2/max8；没有DxgKrnl/DWM或PID过滤。

OpenProcessToken/LookupPrivilegeValueW状态0；AdjustTokenPrivileges.Bool1/LastError0，previous Count1/Attributes0/Bytes16；StartTraceW0、EnableTraceEx2.D3D9 0；RestoreTokenPrivileges.Bool1/LastError0，PrivilegeRestored=true、CloseTokenHandle0。这支持本task临时权限调整及按previous恢复的API成功路径，不认“缺权限阻碍”；没有恢复后token快照，不冒独立权限状态认证。

StartRequestedQpc39911863203613，PresentBefore39911863244854、PresentAfter39911863255850/Result0，StopRequested39911863260150、StopCompleted39911863280878；StopReason=PresentReturned，ControlTraceW.Stop0、StopStatus0。StatisticsValid=true，EventsLost/LogBuffersLost/RealTimeBuffersLost均0，BuffersWritten2/NumberOfBuffers8/LoggerThreadId0x28B4。ScopeCreated39911862932458、Setup39911863185793、ScopeClosed39911867820217，Errors=[]。支持此一次自有Start/Enable/Present返回/按handle Stop/摘要落盘及scope关闭路径，未触发2秒watchdog失败路径。

controller报告ETL131072 bytes，SHA256 `4085C964310A28A8FCEB686326F401B4405F8B8512C408307B89F81F050214E6`。非空文件、两个buffer、lost0及hr0都不证明含Present事件、provider目标事件数、ETL结构有效、目标PID/TID关联、提交/合成/scanout/呈现成功。没有OpenTrace/ProcessTrace/TDH，目标descriptor/版本/字段/时间关联均Unknown；可能只有结构/会话数据或无目标事件，须离线读才能裁决。

源码OriginalPresentSystemTrace 131–200确认Start成功owned才Stop，ControlTraceW(session,null,properties,1)，stopped防二次Stop，gate串行deadline/After/Dispose；201–242确认Dispose清ThreadStatic、Dispose timer、释放HGlobal、ScopeClosed后摘要。没有消费者线程；Timer.Dispose不等待排队回调，锁内disposed检查仅支持内存访问保护，不是所有线程退出证明。没有全系统session枚举、恢复后token读取或logger线程归零测量；不能以Stop0/ScopeClosed冒这些证明。

## 6. 原错误、时序变化与cleanup边界

原失败是前置native d3d9.dll非Ex Clear/Present后的AssertCombinedPixel：actual0x00F3F3F3≠expected0x00563412（逐通道容差1未改），cooperative0、可见窗口32×16、format22/count1/swapEffect1、源/目标rect NULL、override目标0x58C0632。PresentResult0不等于颜色正确。collector Errors=[]与原业务像素失败分层，未把错误归沙箱/驱动/权限或候选实现根因。

SwapChainObservation 24–47：Clear在Before之前；原slot17（本次）/121调用finally立即取After QPC并同步Stop，之后才原Assert(result)及CompleteNativePresent。OutputWindow 49–71原Complete含event query轮询/消息处理及两次DwmFlush；VisualCache 96–106 scope包PresentNativeControl，scope最终摘要在Complete返回后、DiagnoseNativePresent和像素断言前。因此ETL不覆盖原Clear、Complete等待、诊断取色或后续shader/resize。不能将JSON QPC标记冒ETL内marker。

新增Start/Enable/权限API前置成本、Present后同步Stop及scope退出文件写入实际改变原观察时间，即使原Present参数/线程和原Complete代码未改。Present before→after约1.0996ms，Stop请求→完成约2.0728ms；stages CreateDevice after/PresentNativeControl before06:48:35.1439644到PresentNativeControl after06:48:35.6473644约503.4ms，是含collector、Complete和摘要的合段，不是纯Present耗时。Diagnose后断言前06:48:36.1710308，不能冒零延迟对照或无扰动认证，亦不从此时差推内容根因。

stages没有AssertCombinedPixel after/connect before，转入finally；VisualCache 106之后才connect/channel及shader产品流程，故本次未进入产品连接/双输入shader/实际resize/后态合同。实际候选加载已观测，但无相应产品业务成功。不得拼接旧KN的refs/尺寸/生命周期绿证明此task未走分支。

stages cleanup destroy/disconnect/surfaces/bitmap0与1/device/windows/complete均到达，cleanup complete文本为空。VisualCache 63–78、476–500显示channel/connection初始0且有条件跳过，bitmap/surface均初始0；这些before/after是零句柄占位路径，不是真实destroy/disconnect或四个非零对象Release0。device/d3d已创建，finally确实调用Release；window已创建，调用DestroyWindow；本次stage未记录Release返回计数/DestroyWindow返回值，也无独立对象/线程快照。businessCompleted=false，末cleanup断言不执行。因此只认证该finally走完/记录无cleanup failure，不能认证全部资源、线程、callback归零；不以旧KN真实释放填空。

## 7. 唯一下一动作（另授权，本阶段不实施不运行）

ETL结构有效性仍Unknown。仅建议在两个正式项目WpfGfxShape.Tests/WpfGfxShape.ComAcceptance既有边界内，采用默认关闭的纯C#离线TDH单文件解析测试，复用本次回收ETL，在Windows沙箱离线解析。不得新Host/工程/外部CLI/脚本，不扩provider、不重采、不重跑原颜色体，不更改原文件。

先验收文件元数据/结构与实际D3D9 Start/Stop descriptor（Id/Version/Channel/Level/Opcode/Task/Keyword）、运行机schema实际字段、时间与PID/TID对JSON窗口的关联。保留原始descriptor与unknown字段/未知版本，不从旧头文件猜schema或把其他进程事件归目标；无法解码、无目标事件、有效文件但事件0均可作为真实结论保存，不追绿、不以空事件自动判无效。这里Start/Stop指实际事件描述符语义，不能把controller StartTrace/ControlTrace日志当D3D9事件。

提出最小另授权预算：normal COM build≤1、COM publish≤1、离线Windows沙箱≤1（离线单文件解析体≤1）；新ETL采集0、Native0、独立发现0、原业务体0、重试0。先编译/布局与回调异常保全审查，失败即停；不追加诊断变体或扩大provider。当前只交接这一动作，未获执行授权，未创建测试或运行；若获准以实际结果裁决结构与事件，不预承诺呈现因果。历史有限绿、原产品缺口和完整替换Open不变。
