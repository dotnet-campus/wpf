# stageAsync — AsyncDefaultManagedControlsValidatedAndNativeCompiled / CloseoutCompleted / OverallOpen

仅当前默认async接线有限闭环完成；不是全部替换完成。亲自实际工具，无终端/脚本/再委派；本接续生产与测试源码修改0，默认async保留，未触发回撤。旧阶段红绿独立。

## 实读范围与精确五门
R=WpfGfxShape/artifacts/stageAsync-default-r2/readback/Results。本接续亲读而非沿用前代理已读声明：stageAsync-default/stdout.txt1–12、supervision.json1–40、stderr空；TestResults/child.trx1–56分批（首次1–21截断后21–24及25–56补齐，结果8–19完整可见）。stdout精确指向 task20997cee72ba4770a1b0e33ad187e255/Results/stageAsync-default/TestResults/child.trx。
类均WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests；testId/execution映射已核：
- WhenCancellableReaderClosesThenRealCallbackFlushAndDrainCompleteAsync：1c527271/eb890277，Passed，0.1387531秒。
- WhenCancellableReceiveClosesThenCrossThreadAdmissionAndCleanupCompleteAsync：1b7aaa0b/c5ce19d1，Passed，0.1290546秒。
- WhenCancellableReaderReadsThenFortyFramesReachEosAsync：1c2b5300/2db5fdd0，Passed，0.0237349秒。
- WhenStage15InternalBridgeRunsThenRealResourceAndControlsComplete：126b509f/e84598a5，Passed，9.0786774秒。
- WhenStage13ShutdownExitAndReopenRunThenRealLifecycleCompletes：15f24097/03f8a69e，Passed，5.7140056秒。
Summary Completed；Counters total5/executed5/passed5/failed0/error0/timeout0/aborted0/inconclusive0/passedButRunAborted0/notRunnable0/notExecuted0/disconnected0/warning0/completed0/inProgress0/pending0。内部业务与十lifecycle不加TRX分母。
监督2026-11-02T07:03:56.6626824Z至07:04:12.5043467Z，supervisor7860/child5788，120秒/.NET10.0.13/X64，Reason exited/HasExitedtrue/ExitCode0；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut/OutputDrainCancellationTimedOut全false，OutputCollectedtrue/SupervisorErrornull。工具独立OSexit未知，不与child混同。

## 全部业务及cleanup尾实读
stage15/bridge.json1–312按1–100/101–210/211–312全文：passedtrue/40帧/ThreadExitedtrue，44包=1→11→13×40→7→故意missing的8；Closed Failure=-2147024894仅预期missing负控，不是播放错误。PTS5000000红0000FFFF0000FFFF、25000000蓝FF0000FFFF0000FF，2×1/stride8。通知subscribed/atUnsubscribe/afterUnsubscribe=11/11/11；resourceChanges2→32，delete前后32稳定；退订PTS1–3.9s、unbind3–3.9s。lateLease红/leaseDisposedtrue，final0/registryRemovedtrue/descriptorDisposals1。missing threadExittrue/通知11不变/观测259.1614ms。cleanup逐sample Unlock0/buffer.Release1/sample.Release0；尾Flush0/OnFlush+callbackDrain/reader0/callback0/MFShutdown0/CoUninitialize。不能把buffer1或attributes1说成全引用0。
stage15/public-audio-negative.json1–7：HasAudiotrue/Failed/-1072875852/0frames/ThreadExitedtrue，公开纯视频保护保留。
stage6/result.json1–511按1–110/111–240/241–370/371–511全文：Pause位置5000000/count6前后不变；wall5339360→10647502，独立pauseWindowTicks5289619=528.9619ms≥515。Seek25000000/hr0，首帧25000000/epoch2至39000000；Stop引入另一epoch，21发布不是40失败。Rate2 E_NOTIMPL(-2147467263)不称支持，Resume0/Endedtrue；1→11→13*→7；CloseHr0/finalReferences0/registryRemovedtrue/descriptorDisposedtrue/Closed/Failure0。
stage11/result.json1–323按1–100/101–210/211–323全文：scrub两目标5000000/25000000分别epoch1/2、发布1/2、UI请求消费帧代数1/2，红蓝严格一致。Pause5267627ticks=526.7627ms≥515，不继续发布；disabled seek epoch3/30000000发布仍2/UI消费2，恢复至39000000/12帧/EOS/CloseHr0/refs0/registryRemovedtrue/descriptorDisposedtrue。
stage8/duration-replay.json1–95全文：duration40000000/vt21，characteristics134/vt19；Ended位置40000000/epoch1；overrun=-1072875854且epoch1不变；Replay0/epoch2从0→1000000；Close0/length关闭错误正确/refs0/registryRemovedtrue/Failure0。
stage13/close-reopen.json1–66全文：两次Opened/两帧，同proxy每次1→11→13；refs0/descriptorDisposedtrue/registryRemovedtrue；尾Flush0/drain/reader0/callback0/MFShutdown0/CoUninitialize。

### 十lifecycle逐case分批全文
callback-close1–124、callback-release1–124、callback-shutdown1–124；callback-exit1–127、external-close1–127、external-shutdown1–127均按1–85/86–尾读完。六case全部passedtrue/accepted0/pendingAtAcceptancetrue/afterClose=-2147483629/6发布；callback-enter→close-accepted→callback-exit→owner-completed；250ms latePackets0、latelease红、Closed/threadExitedtrue/refs0/registryRemovedtrue/descriptorDisposedtrue。callback-exit的exitLeftOwnerRunningtrue是Exit非Shutdown合同。sample/buffer releases依次close6/release6/shutdown6/exit7/external-close7/external-shutdown7；多一为未发布预读样本，不是漏。六尾均Flush0/drain/reader0/callback0/MFShutdown0/CoUninitialize，shutdown/uninit各1。
opened-exit1–60、opened-shutdown1–60全文：passedtrue/accepted0/0发布/只有Opened包，11/EOS因提前终止NotReached；250ms无late、Closed/threadExit/refs0/registry移除/descriptor释放，预读样本各release1/buffer1；flush/drain/reader0/callback0/shutdown0/CoUninitialize各完整。opened-exit owner仍运行是预期Exit语义。
failed-close1–22、failed-release1–22全文：missing失败-2147024894/事件8，callbackResult0/refs0/Closed/threadExitedtrue/descriptorDisposedtrue/registryRemovedtrue；cleanup:null（未创建reader），底层引用Unknown，不能编清理调用。
证据文字backend仍写synchronous MF owner为旧固定标签，与实际本次尾callback/默认源码不符；不修改原件，不据此冒Native运行。pendingAtAcceptance仅业务回调边界，非producer读阻塞证明。

## 三async与边界
cancellable-reader.json1–50全文：真实native OnReadSample admitted且持有sample；cancel accepted后Close在回调持有时未完成，释放后read cancelled→close completed；delivered0/ThreadExitedtrue/read1/flush1/retained1/released1/Drainedtrue/FlushResult0/reader0/callback0/shutdown1/uninit1，cleanup尾完整。
cancellable-reader-reentrant.json1–24全文：跨线程Close admission，delivered1/ThreadExitedtrue/read1/flush1/reader0/callback0/shutdown1/uninit1；Unlock0/sample0/buffer1/Flush0/drain完整。
cancellable-reader-normal.json1–143按1–75/76–143全文：40帧/EOStrue/末39000000/threadExitedtrue/readCallbacks41/retained40；callback SamplesReleased0是转交消费，SampleReleaseCount40/BufferReleaseCount40/UnlockCount40平衡；尾reader0/callback0/Flush0/drain/MFShutdown0/CoUninitialize。无关键真实fail或cleanup error。
producer阻塞readpending精确状态Unknown；同步MFCreateSourceReaderFromURL仍不可取消。三reader门不扩称所有播放器/网络源强取消。

## 默认接线、epoch与版本
本接续实读IndependentVideoPlaybackOwner.cs1–373分段全文；默认Open与兼容Play都创建callback，单_pendingRead，Run共同等待commandReady/read/flush且PTS等待可被命令打断；Preroll保首帧，Pause最多一pending，Scrub单目标；Seek BeginFlush/PendingFlush完成后CompleteSeek再Epoch++；Close请求取消/唤醒，Dispose drain，WaitForExit最终Join非业务callback selfjoin。
实读callback1–255分段全文：RequestRead拒绝reading/ready重复；BeginFlush每轮_flushed=false和新completion，取消读并丢旧sample；OnFlush+active==0才完成；EndFlush不设永久closing、Epoch++。reader535–610/680–753核BeginFlush/CompleteSeek/Close。真实scrub epoch1/2/3及重播epoch2证明可重复seek可用；但业务JSON没有逐轮BeginFlush epoch日志，不能编造每轮独立计数。这是源码reset+真实重复Seek的联合证据，不是源端阻塞读精确时序。
COM wrapper源码已读三async及两原强门调用关系；source-linked Tests使用生产ProjectReference。wrapper SHA9D5C50C9311AB26B0A812F94F3C87BA22A5545BFF4D658F8561A974E1B7B8335/MVID842095ea-869c-4ef3-83d3-7bcb28aca997；sourceTests SHA4C32A434117365DF689F5584EBCAA2BD048646482F109CDB8093B1CA7B2148B8/MVID8340a050-eecd-4bda-8f66-60c452c0b622，implementationMvid5a750779-26ff-403c-b6d2-1f71179744cd。身份不同属不同程序集，不能混同。未做新hash比对，历史包到当前源码密码学一致性Unknown；本接续未修改生产/测试源码，发布使用当前相同工作区。78 pure按交接实际默认Debug，不称Release重测。

## 历史红绿与执行账
用户交接stageAsync累计normal2/tests2/COM2/sandbox2，本接续不重复上述执行，仅Native1。r1真实失败/修复细节在已找到stageAsync工作稿及Ledger的stageAsync检索中无记录，保持Unknown不猜。另亲读stageAsync-default-r1 stdout1–12，task681dba4b493a489d8cc2bbc3feddca34摘要却5/5/0；bridge240–280尾reader0/callback0，不能把交接所指其他r1失败擅自改写或冲销。未声称r1全证据审计完成。旧control-r3 Attach失败、旧91/full及其它红账独立保留。
本接续normal0/tests0/COM0/sandbox0/Native1；stageAsync最终normal2/tests2/COM2/sandbox2/Native1，所有该阶段执行预算已用，不再调用。

## 唯一Native及收尾
实调生产绝对项目Release/win-x64/net10.0/selfcontained/runtime10.0.13/AOTtrue/ManagedAcceptancefalse/RuntimeHeapDiagnosticfalse/DiagnosticAbifalse/default wpfgfx_cor3 Shared一次，exit0。完整原参数、返回、可见warning及截断在.coding-agent/stageAsync-default-native-publish-receipt.md；60行日志1–60均读，stdout615551字符前595551被工具截断，visible48条CA1416行，总warningsUnknown。57 Generating native code，58 native lib/exp，59全新artifacts/stageAsync-default-final-native-x64；实际DLL/PDB枚举存在。SHA256/大小/PDB身份Unknown待正式COMpack，不新增hashrunner/沙箱，D9AA初始化fixture非本async，F48不认证新产物。
仅sourcefreeze=本接续生产/测试源码未改，不是全repo加密；Native compile不等于媒体导出运行。当前闭环CloseoutCompleted，OverallOpen。
下一直接必要：另授权新Native正式COMpack身份/哈希及x64公开WPF新Native媒体执行，再同步x86和默认full；不再重复已经编码的默认async接线。audioendpoint0/AVI16gap/AVsync/cancelOpenURL/HWconsumerloss/resourcesfull、旧91/x64timeout保持Open。原版loader/副本/委托fallback永久禁止。
