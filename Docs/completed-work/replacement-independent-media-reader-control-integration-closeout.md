# Control integration r4：ManagedStrongGatePassed / NativeCompilePassed / CloseoutCompleted / OverallOpen

## 原件、分母及监督
已先建立实质草稿，逐项实际核验后定稿；亲自操作，无终端/脚本/再委派，未重跑tests/COM/sandbox。
原件根R=WpfGfxShape/artifacts/control-integration-r4/readback/Results，监督绝对路径证实task1d2617013bb84b818f71f64a9ee80eed。旧receipt仅4行停COM0/sandbox0，现更正，不因枚举漏项否认原件。
R/control-integration/TestResults/child.trx 1–38已核Definitions/Results/尾：类WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests的WhenStage15InternalBridgeRunsThenRealResourceAndControlsComplete Passed 9.2137693秒；WhenStage13ShutdownExitAndReopenRunThenRealLifecycleCompletes Passed 5.6586947秒。Counters total2/executed2/passed2/failed0；error/timeout/aborted/inconclusive/passedButRunAborted/notRunnable/notExecuted/disconnected/warning/completed/inProgress/pending全部0，Summary Completed。内部子case不加TRX分母。
监督40行全文：2026-11-01T12:53:07.892873Z至12:53:23.4270728Z，supervisor12800/child9692，原两精确FQN/120秒/.NET10.0.13/X64。Reason exited/HasExitedtrue/childExit0，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut/OutputDrainCancellationTimedOut全false，OutputCollectedtrue/SupervisorErrornull；工具独立OSexit Unknown，不与child混同。stdout12行全文（中文编码损坏保留）摘要2/2/0。

## r3失败与r4接线修正
r3 taska7cdd3e490ff46d3a5948948831d902a监督40行及TRX错误/尾已读：2执行1过1败/child2，bridge0.0481088秒在新增公开音轨负控media.AttachEvents(proxy) expected0 actual-2147019873失败，lifecycle5.7449918秒Passed。后续bridge/controls/scrub/duration/epoch门NotReached，不编Passed。两次累计4执行实例/3过1败，不是4独立方法。
r4去掉的是新增辅助负控的重复Attach，不是撤生产拒音轨或公开factory保护。现IndependentMediaResourceBindingSystemTests.cs14–46仍Create(requireVideoOnly:true)、Open、Completion、严格HasAudio/Failed/0xC00D36B4/0frames/ThreadExited及最终Close/ref0。内部bridge仍RequireVideoOnlyfalse。生产Identity145–205仍Open后音轨guard，Preroll成功后Raise1→Raise11。原receipt记录官方MediaOpened/MediaPrerolled与native once合同；本次未改事件oracle追绿。

## 关键业务、尾及清理已实际核验
### stage15 bridge.json 1–309
passedtrue/PublishedFrames40/ThreadExitedtrue。packetBytes严格1→11→13×40→7，随后故意missing负控8，共44包；末8不是播放失败。真实DrawVideo样本PTS5000000红0000FFFF0000FFFF、25000000蓝FF0000FFFF0000FF，2×1/stride8。
subscribed=atUnsubscribe=afterUnsubscribe=11；resourceChanges从2增长至32，resourceBeforeDelete=resourceAfterDelete=32，卸载PTS1s–3.9s/unbind3s–3.9s，非零且稳定。lateLeasePixels红/leaseDisposedtrue；final0/registryRemovedtrue/descriptorDisposals1。
missingFailure0x80070002/missingOwnerThreadExitedtrue，通知11→11；观测43586636004993→43586638571293=256.63ms；最终Closed/Failure为有意missing。cleanup全段已读：currentType/nativeType Release0、attributes.Release1；逐sample Unlock0/buffer.Release1/sample.Release0，尾reader.Release0/MFShutdown0/CoUninitialize。native/lease底层总引用Unknown，不能将buffer.Release1说成0。
public-audio-negative.json7行：HasAudiotrue/Failed/Failure0xC00D36B4/frames0/ThreadExitedtrue。
### stage6 controls result.json 511行
Pause前后position5000000/count6，wall5326366→10576981，pauseWindowTicks5241901=524.1901ms≥515ms。Seek target25000000/hr0，首帧PTS25000000/epoch2，后至39000000；红蓝像素同上。Rate2 E_NOTIMPL(-2147467263)，Resume0后Endedtrue，不能称2倍速支持。
事件严格1→11→13×21→7；初始帧epoch0，seek后epoch2，seek不是重新Open。不宣称每seek再发1/11。published21/epoch2/Failure0/CloseHr0/finalReferences0/registryRemovedtrue/descriptorDisposedtrue/Closed；nativeReferencesUnknown。长frame数组132–258为逐帧原件范围，关键首尾/epoch/颜色及packets/尾已核。
### stage11 scrub result.json 1–323
passedtrue，preopen rate0、openedPaused/published0；seek红5000000/epoch1/published1/UI1，蓝25000000/epoch2/published2/UI2；pauseTicks5248159=524.8159ms且published2。disabled-seek-no-publication为epoch3/pts30000000/published仍2/request3/consumed2/frame2，false不发。恢复后epoch3十帧30000000至39000000，总12/Endedtrue，Ended callback Close0/Closed。Packets1→11→13×12→7；references0/registryRemovedtrue/descriptorDisposedtrue/nativeReferencesUnknown。
### stage8 duration-replay.json 95行全文
metadata duration100ns hr0/vt21/40000000，characteristics hr0/vt19/134。Ended length/position40000000/epoch1；Overrun -1072875854且Ended/epoch1；Replay0，epoch2帧0和1000000，ReplayPause0。Close0后length/pause为RO_E_CLOSED及0，finalReferences0/registryRemovedtrue/Failure0/Closed。sourceSha25601BE17E50794DD8A076967E49D0C2CD6265A3908247748B4B95689AB2F07C872是AVI，不是生产源码SHA。
### 每Open一次1/11与reopen
TRX stdout两独立Open cycle0/1各[1,11]，metadata2×1/no-audio/duration40000000；源码stage19循环两identity严格各[1,11]。包内epoch均0，不虚构全局seek epoch。
stage13/close-reopen.json66行全文：opened2/frames2/packets[1,11,13,1,11,13]，refs0/registryRemovedtrue/descriptorDisposedtrue，尾reader.Release0/MFShutdown0/CoUninitialize。
### stage12十个JSON（均已读关键及完整cleanup尾）
callback-close、callback-release、callback-shutdown、callback-exit、external-close、external-shutdown六份各passedtrue/accepted0/pendingAtAcceptancetrue，frames6/packetsAtAcceptance8，250ms latePackets0/observerCompletetrue/threadExitedtrue/Closed/refs0/registryRemovedtrue/descriptorDisposedtrue。late lease仍红；callback-enter→close-accepted→callback-exit→owner-completed。callback-exit exitLeftOwnerRunningtrue是Exit语义，最终threadExitedtrue。
六份各sampleReleases7/bufferReleases7/shutdownCount1/uninitializeCount1/readerFinalReferences0，Unlock0/sample.Release0/buffer.Release1，尾reader.Release0/MFShutdown0/CoUninitialize/nativeReferencesUnknown。
opened-exit/opened-shutdown各57行全文：accepted0/pendingtrue、frames0/packets1（仅1），250ms late0/threadExitedtrue/Closed/refs0/registryRemovedtrue/descriptorDisposedtrue；sample/buffer释放各1、shutdown/uninitialize各1/readerFinalReferences0。opened-exit exitLeftOwnerRunningtrue。被回调提前关闭，11/播放EOS子路径NotReached，不编Passed。
failed-close/failed-release各22行全文：passedtrue/CallbackResult0/Failure0x80070002/Closed/threadExitedtrue/refs0/registryRemovedtrue/descriptorDisposedtrue，仅8；cleanupnull/nativeReferencesUnknown，不虚构MF清理。

## 身份分层与系统模块
wrapper SHA FA67F0A361A639622DCAF53A4F1BDD962875E249A8C11E9E5864FB49644EB1C3/MVID5ce72914-214e-4b6d-9d89-acab4e2cbbab；source-linked Tests SHA6FBEBA164E1D72A400D683A430525F9B50C781B909E0689D021274FB3CA1A3E5/MVID6c8f96ee-c1e5-41b7-b310-527d03cf2945，不可互换。implementation为WpfGfxShape.ManagedAcceptance.dll，独立SHA/MVID未记录Unknown。bridge modules实际包括COM exe/dll、Tests、ManagedAcceptance。
实际图wrapper→Stage3Production aliased Tests→ManagedAcceptance Identity→同步IndependentVideoPlaybackOwner→系统MF reader→lease→GeneratedMediaPlayerResource/DrawVideo software RTB，非Native媒体导出。SYSTEM32 mfplat/mfreadwrite/mfsrcsnk10.0.26100.9278、mfcore10.0.26100.1；关键及尾已核。
aot-identity.json旧fixture D9AA1E0ED5E99B29F10AC2DA4635D7F014B0F433C05DF2E5B18F2028836CD09C，只fixture初始化，非新AV Native验证。

## 总账与历史
control-integration累计normal0/tests1/implicitDebug1/COM2/sandbox2/Native1；本收尾新增normal0/tests0/COM0/sandbox0/Native1，新full0/Native媒体运行0，COM/sandbox/Native额度耗尽。
原pure七家族filter见receipt：71passed/0failed/0skip/40ms exit0，默认Debug/net10.0/noRID；这是fixtureevent变动前快照，r4组合直接调用其中部分函数并完成系统业务，不是71在r4源码上重跑。原58、bridge recovery27/35及async三门分别历史，不相加为当前回归。原pure stdout1638944字符/前1618944截断/warningsUnknown；既有COM两次原参数完整返回/日志字符计数未随旧receipt持久化，Unknown，不伪造。r3/r4监督/TRX确证两任务。

## 唯一Native发布成功（不代运行）
冻结.coding-agent/control-integration-sourcefreeze.md，原命令/return/exit/log见.coding-agent/control-integration-native-publish-receipt.md。冻结后仅文档，无源码修改。绝对生产项目d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj，Release/win-x64/net10.0/self-contained true/RuntimeFrameworkVersion10.0.13/PublishAottrue/WpfGfxShapeManagedAcceptancefalse/WpfGfxShapeDiagnosticAbifalse/WpfGfxShapeRuntimeHeapDiagnosticfalse；NativeLib默认Shared。
实际1次exit0，工具return成功/日志61行；已读1–31和32–61，native code/lib/exp及独立输出明确。stdout615552字符/前595552截断/最后20000保留，CA1416存在/warnings总数Unknown，未关分析器。
独立新WpfGfxShape/artifacts/control-integration-final-native-x64实际枚举wpfgfx_cor3.dll及wpfgfx_cor3.pdb。新hash/大小/PDB身份Unknown，后打包认证，不虚SHA或复用D9AA。未运行新Native媒体导出，只有NativeCompilePassed，不是Native媒体ABI Passed。不修再发。

## 下一直接有限实现、完整Open边界
已运行有限闭环正式关闭；完整替换仍OverallOpen。默认仍同步，没有默认async迁移源码；独立asyncreader三门仅slice，SourceReader同步Open不可取消，不称播放器强cancel通过。
下一直接修改IndependentVideoPlaybackOwner.Run/PrerollAsync/SeekAsync/ReadScrubFrame：复用CancellableVideoReaderOwner接单pendingReadTask，MTA共同等待控制和读取；Pause最多保留一个pending；Seek经Flush/OnFlush/callbackDrain后SetPosition并推进generation/epoch淘汰晚样本；Close/Release取消+有界drain，callback不selfjoin，跨Close lease有效。不新Host/层，不再纯设计；完成后同包强组合与默认异步取消验证，不以reader微门替代。
默认强cancel、音频endpoint0/AVI16gap/AVsync、真实HWloss/resources、当前Native媒体导出、双arch/full均未闭。旧1843/1752/91与x64timeout分母Unknown不相减；全媒体最终验收保留。生产原版loader/副本/委托fallback永久禁止，合法external provider不误删。
