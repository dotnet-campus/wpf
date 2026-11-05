# Control integration r4 原件复核收尾

## 当前结论
ManagedStrongGatePassed（有限同步托管组合）/ OverallOpen。默认仍为同步 IndependentVideoPlaybackOwner，未接入 async；不能据 reader slice 宣称播放器取消通过。本文件为 WpfGfxShape/Docs 下正式入口，根 Docs/completed-work 同名报告保留为前次历史，不删除或移绑历史执行。

## 本次实际操作与预算
亲自使用文件工具核原件，无终端、脚本、再委派或本机 MF 运行。本次截至本收尾 normal0/run_tests0/COMpublish0/sandbox0/Nativepublish0；本次新授权上限分别3/2/2/2/1，尚未消费。此前 control-integration 累计 normal0/tests1/implicitDebug1/COM2/sandbox2/Native1 是历史执行账，不计成本次新执行。原71 pure passed 是事件 fixture 迁移前历史，不冒当前源码回归；58及27/35、async三门各自分账。

## 精确原件与真实分母
R=WpfGfxShape/artifacts/control-integration-r4/readback/Results。stdout 给出 task1d2617013bb84b818f71f64a9ee80eed/Results/control-integration/TestResults/child.trx；目录枚举漏 TestResults，按该精确路径实际读取成功。
TRX 两方法：IndependentVideoPlaybackOwnerTests.WhenStage15InternalBridgeRunsThenRealResourceAndControlsComplete Passed 9.2137693s；WhenStage13ShutdownExitAndReopenRunThenRealLifecycleCompletes Passed 5.6586947s。Definitions 类/FQN及尾 Counters total2/executed2/passed2/failed0，其余计数0、Completed 已核。不是十个独立TRX。
supervision.json 1–40：SupervisorPid12800/child9692，2026-11-01T12:53:07.892873Z 至12:53:23.4270728Z，120s，.NET10.0.13 X64，精确上述两个FQN；exited/ExitCode0/HasExitedtrue/OutputCollectedtrue；TimedOut、Cancelled、KillAttempted、KillReturned、两drain超时均false，SupervisorErrornull。stdout12行摘要2/2/0；独立工具OSexit未留原返回，Unknown。
r3 taska7cdd3e490ff46d3a5948948831d902a 的2/1/1 child2为历史保留，本次未重新读取其原TRX；历史重复Attach修正不等于撤销生产音轨guard。两任务累计4执行实例/3passed1failed，不是4个独立方法。

## 实际业务与资源
bridge.json 1–309 本次全文已读：PublishedFrames40/ThreadExitedtrue；packetBytes [1,11,13×40,7,8]，最后8为有意missing负控，不删Ended7。PTS5000000红0000FFFF0000FFFF、25000000蓝FF0000FFFF0000FF，2×1 stride8。subscribed/atUnsubscribe/afterUnsubscribe均11；resourceChanges非零2→32，resourceBeforeDelete/AfterDelete均32；卸载PTS1s至3.9s，无owner线程待入回调，解绑PTS3s至3.9s。lateLeasePixels红/leaseDisposedtrue/final0/registryRemovedtrue/descriptorDisposals1。
missingFailure=-2147024894(80070002)/missingOwnerThreadExitedtrue，missing通知11→11，观察256.63ms；不是以缺失文件假成功。public-audio-negative.json全文：HasAudiotrue、Failed、Failure=-1072875852(C00D36B4)、0frames、ThreadExitedtrue；内部PCM素材读视频不因此被禁止。
cleanup全文有逐sample Unlock0/buffer.Release1/sample.Release0，尾reader.Release0/MFShutdown0/CoUninitialize。buffer.Release1不冒0；底层sample/lease总引用Unknown。
controls result.json关键1–150、259–340、440–511已读：PauseBefore/After position5000000/count6，wall5326366→10576981，pauseWindowTicks5241901=524.1901ms≥515；seek25000000/hr0，seek后epoch2，红蓝DrawVideo真像素。事件1→11→13×21→7，published21/endedtrue/epoch2/Failure0/CloseHr0/finalReferences0/registryRemovedtrue/descriptorDisposedtrue。Rate2 E_NOTIMPL保留。随后补读151–258、341–439，controls1–511已全部读完；21帧为初段0至5000000的6帧及Seek后25000000至39000000的15帧，事件完整1→11→13×21→7。
scrub result.json关键1–120、170–205、260–323已读：preopen rate0/openedPaused0frames，红5000000 epoch1仅1帧、蓝25000000 epoch2总2帧；pauseTicks5248159=524.8159ms冻结2帧；scrubfalse seek30000000 epoch3仍2帧/UI请求3 consumed2 frame2。恢复至39000000总12/Endedtrue，Ended callback Close0，references0/registryRemoved/descriptorDisposed。随后补读121–169、206–259，scrub1–323已全部读完，12帧为两个scrub帧及恢复后的十帧，完整事件1→11→13×12→7。
duration-replay.json 1–95全文：duration40000000/vt21/characteristics134/vt19；Ended position40000000 epoch1，Overrun -1072875854不改Ended，Replay0 epoch2帧0→1000000，Close0后RO_E_CLOSED/0，finalReferences0/registryRemovedtrue/Failure0/Closed。
close-reopen.json1–66全文：opened2/frames2/[1,11,13,1,11,13]，references0/descriptorDisposed/registryRemoved，尾reader.Release0/MFShutdown0/CoUninitialize。TRX stdout另两个独立source cycle各[1,11]，不是每Seek重复Opened。

## 十个生命周期子场景（不加TRX分母）
callback-close/release/shutdown/exit、external-close/shutdown六JSON各1–124全文已读：passedtrue/accepted0/pendingAtAcceptancetrue/6frames/8packets，250ms latePackets0/observerCompletetrue/threadExitedtrue/Closed/references0/registryRemovedtrue/descriptorDisposedtrue/lease红。顺序callback-enter→close-accepted→callback-exit→owner-completed；callback-exit允许exitLeftOwnerRunningtrue，最终真实退出。各sampleReleases7/bufferReleases7/readerFinalReferences0/shutdownCount1/uninitializeCount1，尾MFShutdown0及CoUninitialize，nativeReferencesUnknown。此pending是同步生命周期接受标记，不能冒真实异步MF在途读取取消证据。
opened-shutdown/exit各57行全文：仅Opened1/0frames，250ms late0、线程退出、refs0、sample/buffer各1、reader0、shutdown/uninitialize各1。回调提前终止，Prerolled11及EOS为NotReached，不空集合All判Passed。
failed-close/release各22行全文：只有Failed8/80070002/callbackResult0/Closed/threadExited/refs0/descriptorDisposed/registryRemoved；cleanup:null，MF清理及底层引用Unknown，不虚构。

## 身份与Native边界
supervision原件wrapper SHA FA67F0A361A639622DCAF53A4F1BDD962875E249A8C11E9E5864FB49644EB1C3/MVID5ce72914-214e-4b6d-9d89-acab4e2cbbab。
bridge/controls/scrub/lifecycle原件sourceTests SHA6FBEBA164E1D72A400D683A430525F9B50C781B909E0689D021274FB3CA1A3E5/MVID6c8f96ee-c1e5-41b7-b310-527d03cf2945。实际modules含ManagedAcceptance，其独立SHA/MVIDUnknown；source-linked production Av→系统MF→lease→资源→DrawVideo软件RTB，非Native媒体导出。
aot-identity.json全文旧fixture SHA D9AA1E0ED5E99B29F10AC2DA4635D7F014B0F433C05DF2E5B18F2028836CD09C仅初始化，不是本次当前源码Native ABI。
前次Native发布receipt已读：生产绝对csproj/Release/win-x64/net10.0/selfcontained10.0.13/AOTtrue/ManagedAcceptancefalse/DiagnosticAbifalse/RuntimeHeapDiagnosticfalse/defaultShared；历史工具记录exit0/61行/生成native，stdout615552截前595552，warningsUnknown。此轮未重发布或亲自复跑，历史NativeCompilePassed不冒本轮实跑。历史输出control-integration-final-native-x64/hashUnknown，Native媒体执行0。

## before/after 与精确边界
本次收尾前后生产源码未改；IndependentVideoPlaybackOwner仍_prepared同步MediaFoundationSourceReader，PrerollAsync在MTA内循环reader.Read；CancellableVideoReaderOwner为opt-in独立slice。没有本次默认接线需撤回。后续只在新默认完整强门通过后保留async接入；失败须精确撤本次接线而不撤独立已验证slice、公开requireVideoOnlytrue、内部PCM读视频、lease/provider合同。
静态核验定位：MediaFoundationSample.cs实际承载MediaFoundationSourceReader（不存在同名独立文件）；Create265–310在创建MTA执行CoInitializeEx/MFStartup并把callback设置为MF_SOURCE_READER_ASYNC_CALLBACK；Read570–645为RequestRead→无限WaitSample→转换→finally释放sample/buffer；Close648–684 RequestClose→Flush→10s WaitDrain→reader.Release→callback.Release→MFShutdown/CoUninitialize。callback80–196对借入sample AddRef一次，关闭/占位时释放，否则移交Read finally释放；owner关闭可唤醒WaitSample。当前Seek560–568仅Flush后立即SetPosition，未等待OnFlush/drain；callback._closing为终态且_flushed无重复epoch重置。故仅给默认Create传callback或把Read包装Task，不满足可重复Seek/cancel合同。CancellableVideoReaderOwner.ReadAsync仍在自身MTA阻塞WaitSample，不能原样移作播放器控制队列。尚未完成可重复Flush协议、pendingReadTask接线或新强门，本次无源码变更/撤回。
同步URL Open不可取消必须真实保留；不可Task超时丢线程假CloseCompleted。后续具体实现Run/PrerollAsync/Tick/SeekAsync/ReadScrubFrame/CloseAsync单pendingReadTask及MTA控制共同等待，Seek/Close关read准入→Flush→OnFlush/callbackdrain→旧sample释放→SetPosition→新epochread，Pause最多一帧，Scrub实际单帧，不callback selfjoin。先静态核回调AddRef/Release及COM/MF成对初始化清理，不强制GC/关闭runtime私有句柄。
OverallOpen：旧91与x64timeout独立保留，audioendpoint0/AVI16gap/AVsync/默认强cancel/HWconsumerloss/resources完整门/双arch当前源码公开Native ABI/full均未关闭。原wpfgfx生产loader/副本/委托/fallback永久禁止。下一当前源码async强组合后再双arch公开及default full验收，不以有限托管门冒完整替换。
