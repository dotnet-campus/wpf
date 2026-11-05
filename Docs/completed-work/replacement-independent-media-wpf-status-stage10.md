# Stage10 r2 — WPF status 与真实失败事件有限系统收尾

## 终态
LimitedWpfStatusAndFailureEventsValidated / CloseoutCompleted / OverallOpen。
本次主代理亲自操作监督，无再委派、终端或脚本；无源码更改、构建、定向测试、发布或NativeAOT执行。唯一Windows sandbox已执行并收尾，不再重跑。前代理交接是代理交接，不是用户认证。

## 调用、任务与原件
- task：b4c884699caf41509a4e640720039521。
- 远端根：C:\CodingAgentSandbox\Tasks\b4c884699caf41509a4e640720039521。
- sourceDirectory：WpfGfxShape/artifacts/independent-media-stage10-r2/clean。
- executableRelativePath：WpfGfxShape.ComAcceptance.exe。
- arguments：["--supervise-exit","Results/independent-media-stage10-r2","120","FullyQualifiedName=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenPublicComProxyRunsThenPacketsAndRealVideoControlsPass"]。
- timeoutSeconds：300；outputRelativePath省略，整任务回收。
- 本地：WpfGfxShape/artifacts/independent-media-stage10-r2/readback（以下简称R）。
调用前用实际DLL公开API确认此方法无参、返回Task，并核源码实际调用；不以文件名当类。预查artifact只有clean/publish，无readback、receipt无task、默认sandbox-results不存在；未发现重复执行证据。120秒预注册用于4秒原创AVI多case及固定观察/controls/清理，不变更515ms等强门。
工具return即时持久化在WpfGfxShape/Docs/handoffs/stage10-r2-execution-receipt.md；完整原return另存该artifact/sandbox-tool-return.txt。return本身只提供supervisor-before-child前台拓扑及回收路径，没有exit数字；exit从监督原件读取。

## 真实运行结果与完整阅读范围
R/Results/independent-media-stage10-r2/stdout.txt 12行完整读，stderr为空。stdout精确指向同目录TestResults/child.trx，按此路径读26行全文；目录枚举未列TestResults不作不存在推论。
TRX只计一个组合测试：executed1/passed1/failed0/notExecuted0，无skip；duration00:00:05.0737471。runId30953d13-b4e2-411f-8dd7-1f28824a8d2a。
监督supervision.json全40行：15:11:05.0620011至15:11:10.818225 UTC；supervisor12844/child10532；Reason=exited、ExitCode0、TimedOut=false、Cancelled=false、KillAttempted=false、HasExited=true、OutputCollected=true、drain两超时false、SupervisorError=null。
Diagnostic/log_261030151105159.diag全65行分段读，长段截断补读59–65，最终session exit0；MSTest4.0.1/MTP2.0.1、X64/.NET10.0.13、自带CoreLib，OS10.0.26200.0。没有把发现1837项写成执行1837项。
全部业务原件分段完整读：stage10-status/audio-paused.json124行、video-only-autoplay.json124行、missing.json48行；independent-media-stage6/result.json480行；identity1行。业务结果从程序真实输出读取，未手写或替换任何测试结果。

## 系统子case与强门
|真实执行子case|系统业务证据|有限结论|
|---|---|---|
|audio-paused|passed=true，原创含PCM AVI；preopen rate0/request1/consumed0/frame0；HasAudio hr0/value1；buffering0/download1/buffered1各hr0|真实音轨metadata成立，不代表发声|
|video-only-autoplay|passed=true，原创生成器includeAudio=false AVI；preopen rate1/request1/consumed0/frame0；HasAudio hr0/value0；同本地progress值|真实无音轨metadata成立|
|两有效case UI代际|第一次request1/consumed1/frame4/PTS3000000/Paused；第二次request2/consumed2/frame8/PTS7000000|有真实帧与PTS进展；请求不会当场自消费的中间(2,1)为已执行源码断言，非单独JSON快照|
|两有效case Close|close0、request2/consumed2、notificationCount9，固定200ms观察；包为Opened1+NewFrame13×8|关闭拒绝及有限无新通知；不声称永久静默或强取消|
|missing|passed=true；随机5f8292def7a547f4b933c9f2db51bb12.avi，exists=false；真实MF hr0x80070002（-2147024894）；仅packet8|无虚构Opened1/Ended7；packet原字节0800000002000780000000000000000000000000，准确保持HR|
|missing UI/Close|request1/consumed0；close0；notificationCount1，200ms观察|未产生帧消费，仅失败事件|
|原controls/DrawVideo|pauseWindowTicks5220660=522.066ms>=515；前后PTS5000000/count6不变；Seek25000000 hr0/epoch2，其后无旧区PTS；production DrawVideo软件RTB红0000FFFF0000FFFF、蓝FF0000FFFF0000FF|强门未放松，真实软件绘制非HW/完整WPF|
|EOS/事件|published21、lastPTS39000000（帧表）、ended=true；Opened1/NewFrame13×21/Ended7，20字节包HR0；duration40000000/canPause1，Rate2=80004001|Ended回调Close0；最终Closed/Failure0/epoch2|

### 证据层级限制（不能补写系统字段）
- preopen0后Open完成Paused、PublishedFrames0、固定150ms后仍Paused/0；随后rate1恢复进展：前半是本次已执行helper中的断言、组合TRX Passed间接支持，但JSON没有独立记录150ms前后快照；后半PTS3000000→7000000有业务原件。
- 请求仅被下一真实帧消费：helper断言新请求后(2,1)，播放后consumed2；业务只记录起点和两个消费点，不记录准确消费帧的原子瞬间。不能把frame4/8称为“正好第一帧”。
- Close后UiRequest返回80000013且代际不增、200ms计数不变：helper已执行断言；JSON记录closed-rejection/观察窗口/计数但未记录拒绝HR或观察前后两个计数。
- unopened HasAudio S_OK/false、null E_POINTER、closed80000013，以及unopened progress8007139F：IndependentMediaStatusTests源码明确且前代理64pure日志通过（非本次重跑），本次系统bridge未单独调用该pure类，不能填新的系统0failed/Passed。
- unknown audio metadata精确失败：生产实现保留AudioMetadataResult负HR，否则E_NOTIMPL；本次没有unknown-metadata系统子case或业务字段，仍NotSystemExercised，不冒充系统通过。
- pure COM桥两个方法在组合入口先执行，组合Passed，但无独立本轮TRX/packet业务文件；不从源码推算额外系统通过数量。duration replay、MP4、并发退订/回调Release未选中，不填0passed。

## 身份、系统模块与清理
监督wrapper SHA256=422D006191195D55CCB81DDFAA16EEDEC1780B857235C1D9AC064C360851019A，MVID=efd8c6d6-7755-4c3c-8366-f2fa893b24d4。
四业务原件source-linked assembly SHA256=B5D28EC79208804857129F2AA56B80589F8C137F7AD4BA032AB9B33EC9968AB7，MVID=9ed2ab1d-5812-4783-8380-bfaf964487b0。两层身份不同是不同程序集，不冒同一hash。
R/Results/aot-identity.json及clean/aot.sha256实读旧fixture SHA256=6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403，路径为task/Native/wpfgfx_cor3.dll，Sandbox=true/PointerSize8。仅旧PE fixture身份初始化，不是当前实现NativeAOT或public factory通过。
系统modules来自controls原件：mfplat.dll、mfreadwrite.dll、mfsrcsnk.dll均C:\WINDOWS\SYSTEM32或System32路径，version10.0.26100.9278；mfcore.dll为System32/version10.0.26100.1。未恢复生产原版wpfgfx loader或fallback。
三status子case均references0/registryRemoved=true/Disposed=true/close0；controls finalReferences0/registryRemoved=true/descriptorDisposed=true/CloseHr0/Closed。此为identity COM与descriptor清理证据，nativeReferences原件明确Unknown（backend计数未导出），不宣称全native引用0。没有kill或残留进程额外审计，不用正常exit替代全系统资源绿。

## 执行账与历史分离
本次新增normal0/test0/publish0/Native0/sandbox1（额度1耗尽），源码0。r2累积前代理normal2：Tests3810warnings/0errors，COM2464warnings/0errors；定向64pure passed/0failed/0skipped；publish1成功，implicitDebug1/publishCompile1单列。以上构建/tests/publish为代理交接和receipt支持，非本次重新执行或全部原始编译日志重读。
Stage10旧47pass/1fail、随后48pass/0fail、r1系统pass、Stage9 r2pass、64pure各自独立，不合并覆盖。旧Native fixture不是Native新执行。之前草稿Investigating/尚未执行文字已由本正式报告取代，不抹历史失败。

## 下一具体必要生产API与未闭合项
下一新授权直接实现IndependentMediaIdentity中COM slot11 SetIsScrubbingEnabled的真实受控视频scrub语义：在preopen保留意图、paused seek完成后发布真实帧并驱动已有UI request代际、Close拒绝、错误精确保留，不能以存bool后S_OK冒成功。先补公开调用路径合同与测试，再新预算的系统验证；本次不实现、不启用factory。
同时Volume/Balance setters(slot9/10)必须接真实可用音频输出能力才可成功，不能仅存储；当前音频端点受限证据不抹。真实音频输出/AVsync、强取消、并发Close退订/回调Release、当前源码NativeAOT、完整WPF及最终full仍Open。公开MILFactoryCreateMediaPlayer继续E_NOTIMPL/null，Scrub未完成；本次有限结论绝不晋升OverallClosed。

## 文档完成项
正式报告（本文件）、WpfGfxShape/Docs/handoffs/stage10-r2-execution-receipt.md、next-session-handoff.md、handoffs/current-work-item.md、handoffs/current-stable-baseline.md、replacement-issues-and-plan-status.md、final-acceptance-remaining-matrix.md、wpfgfx-complete-replacement-validation-plan.md、handoffs/current-execution-rules.md均更新Stage10 r2优先段；旧阶段内容保留为历史。逐份回读确认，不以代理交接冒用户交接。
