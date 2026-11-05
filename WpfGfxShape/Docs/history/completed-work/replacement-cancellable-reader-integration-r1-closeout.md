# Cancellable reader integration r1：原件核验与安全关闭

## 当前裁决
默认保持 IndependentVideoPlaybackOwner 的同步 MediaFoundationSourceReader；没有默认迁移源码，不需要撤回其它人的源码。异步 reader 三门均实际 Passed；原 bridge 合同仍失败，不能宣布完整回归或 player 默认可取消。OverallOpen。原 task 不重跑。

## 实际系统执行
原件根：WpfGfxShape/artifacts/cancellable-reader-integration-r1/readback/Results。
监督 reader-integration/supervision.json 全文：task c5efeb3cfe69427da5cc6d22cd42c9f8，supervisor8276/child3356，2026-11-01T12:23:34.508273Z—12:23:41.331398Z，filtered/120秒，child exit2/0x00000002，exited；timeout/cancel/kill及输出drain超时全false，SupervisorError null。工具独立OS exit Unknown。
stdout 1-21与精确 reader-integration/TestResults/child.trx 1-68已读，TRX total5/executed5/passed4/failed1/error0/timeout0/aborted0/notExecuted0。每个方法各命中1，不从filter估计分母。
- WhenCancellableReaderClosesThenRealCallbackFlushAndDrainCompleteAsync：Passed，0.2232354秒。
- WhenCancellableReceiveClosesThenCrossThreadAdmissionAndCleanupCompleteAsync：Passed，0.1452183秒。
- WhenCancellableReaderReadsThenFortyFramesReachEosAsync：Passed，0.0229979秒。
- WhenStage15InternalBridgeRunsThenRealResourceAndControlsComplete：Failed，0.0997148秒，原40帧断言实际0，未修改原合同。
- WhenLifecycleClosesThenCallbacksAndMtaCleanupComplete：Passed，5.5957443秒；它是一个TRX case，内部子场景不能重复计入分母。

## 三门业务原件
cancellable-reader-normal.json 1-143全文：frames40/EOS true/PTS39000000/ReadCallbacks41/retained40；callback SamplesReleased0表示转交，不是总sample未释放。reader SampleReleaseCount40/BufferReleaseCount40/UnlockCount40；40组三项清理已读。尾Flush0/OnFlush+callbackDrain/reader.Release0/callback.Release0/MFShutdown0/CoUninitialize；ThreadExited true。独立epoch、FlushCallbacks、ShutdownCount字段Unknown，不填0。
cancellable-reader.json 1-50全文：真实OnReadSample准入并持有sample；delivered0/readCallbacks1/flushCallbacks1/retained1/released1/Drained true/FlushResult0/readerFinal0/callbackFinal0/shutdown1/uninitialize1/threadExited true。timeline先cancel，再Close仍未完成，放callback后read取消及Close完成。源端尚未进入callback的pending状态Unknown；epoch未单独记录Unknown。
cancellable-reader-reentrant.json 1-24全文：delivered1/readCallbacks1/flushCallbacks1/readerFinal0/callbackFinal0/shutdown1/uninitialize1/threadExited true；Unlock0/buffer.Release1/sample.Release0/Flush0/OnFlush+callbackDrain/reader.Release0/callback.Release0/MFShutdown0/CoUninitialize。证明本次receive跨线程Close准入不受_gate持锁阻塞；不证明默认player所有callback可取消。该case无独立epoch计数字段，Unknown。

## 身份及原合同失败
MSTest4.0.1/.NET10.0.13/X64。COM test SHA 1E4BC50DF814B2B015203E3210BE66763C67788E015AB229BE18C7E71E683793 / MVID a0cc6095-28b5-4be1-8aed-7e45615687d2；业务source-linked Tests SHA A7E8E4EECEA3B47E8975CA10398A20DCE664E0CD723C35DA438FA6A2619AD374 / MVID46eab7df-6a15-43ec-8eab-978aeea12478；implementation MVIDbf4a9aad-a6a6-442e-a819-9c52af879a2b。
系统mfplat/mfreadwrite来自C:\WINDOWS\SYSTEM32，10.0.26100.9278。bridge.json全文：Failure -1072875852，PublishedFrames0，samples空，lateLeasePixels null/leaseDisposed false，ThreadExited true/Closed，final0/registryRemoved true/descriptorDisposals1。cleanup reader.Release0/MFShutdown0/CoUninitialize；controls及晚lease没有通过。原wrapper先await桥40帧方法，失败后后续WhenComControlsPauseSeekResumeThenRealEventsAndDrawVideoRemainOrdered与WhenPausedScrubSeeksThenRealFramesAndUiEpochsRemainPaused均NotReached，不将它们算成独立Failed。零帧错误根因尚未证实，不猜为receive锁外改动导致，不改fixture或断言追绿。public Native ABI和底层lease refs Unknown；不拿fixture Native宣称本次媒体ABI验证。

## 预算与执行来源
新增授权normal3/tests2/COM2/sandbox2/Native1；已执行normal1/tests0/COM1/sandbox1/Native0（前代理收据及当前原件核验）；余normal2/tests2/COM1/sandbox1/Native1。本次接续只读写文档，未新增执行。normal1 exit0/3923warnings/0errors，绝对Tests.csproj Release/net10.0/win-x64；输出3274561字符/截前3254561/留末20000。COM1 OfficialWindowHangDiagnostic/win-x64/net10.0-windows/selfcontained10.0.13/PublishAotfalse/ManagedAcceptancetrue/ComAcceptanceSandboxtrue；exit0，warnings Unknown，1703714字符/截前1683714/留末20000。具体已执行参数来源.coding-agent/cancellable-reader-receipts.md，不伪造丢失的原始工具返回。新publish与clean分开，非本次再次publish。

## 生命周期逐原件核验（10业务子场景，不新增TRX分母）
全部JSON亲自全文读取：callback-close/release/shutdown/exit及external-close/shutdown各1-116；opened-exit/shutdown各1-56；failed-close/release各1-21。十个passed字段均true。
- callback-close：published6/closeFrames6/sampleReleases7/bufferReleases7；callback-enter→close-accepted→callback-exit→owner-completed，exitLeftOwnerRunning false。
- callback-release：published6/closeFrames6/sampleReleases7/bufferReleases7，同序，exitLeftOwnerRunning false。
- callback-shutdown：published6/closeFrames6/sampleReleases7/bufferReleases7，同序，exitLeftOwnerRunning false。
- callback-exit：published6/closeFrames6/sampleReleases7/bufferReleases7，同序，exitLeftOwnerRunning true；最终threadExited true，不把此中间状态删掉。
- external-close：published6/closeFrames6/sampleReleases7/bufferReleases7，同序，exitLeftOwnerRunning false。
- external-shutdown：published6/closeFrames6/sampleReleases7/bufferReleases7，同序，exitLeftOwnerRunning false。
上述六例各accepted0/pendingAtAcceptance true/afterClose -2147483629/packetsAtAcceptance8/observerComplete true/latePackets0(250ms)/leasePixels0000FFFF0000FFFF；各Closed/threadExited true/references0/registryRemoved true/descriptorDisposed true；每例7组Unlock0/buffer.Release1/sample.Release0，reader.Release0/MFShutdown0/CoUninitialize，shutdown1/uninitialize1/readerFinal0；nativeReferences Unknown。它们为同步owner，无OnFlush/callbackFinal字段，Unknown，不套异步reader计数；epoch Unknown。
- opened-exit：published0/closeFrames0/packetsAtAcceptance1/exitLeftOwnerRunning true；opened-shutdown相同但exitLeftOwnerRunning false。各accepted0/pending true/observerComplete true/afterClose -2147483629/latePackets0(250ms)/leasePixels null，最终Closed/threadExited true/references0/registryRemoved true/descriptorDisposed true；各sample/buffer release1，Unlock0/reader.Release0/MFShutdown0/CoUninitialize，shutdown1/uninitialize1/readerFinal0；native refs/epoch/OnFlush/callbackFinal Unknown。
- failed-close及failed-release：各callbackResult0/references0/failure -2147024894/Closed/threadExited true/descriptorDisposed true/registryRemoved true，release分别false/true；各失败packet kind8。cleanup null，sample/MFShutdown/COMuninit/readerFinal/callbackFinal/epoch均Unknown，不填0。
以上backend明确source-linked production identity/eventproxy/synchronous MF owner，非当前NativeAOT媒体执行。六个帧case及两个openedcase均带source-linked SHA A7E8.../MVID46eab.../.NET10.0.13/X64。
aot-identity.json全文已读：sandbox true/X64/pointer8，fixture Native SHA D9AA1E0ED5E99B29F10AC2DA4635D7F014B0F433C05DF2E5B18F2028836CD09C；它不是本次新编译或媒体ABI证明。stderr为空。七入口新增当前段均已逐份回读1-6，保留旧历史而不覆盖。

## 剩余接入点（默认不开）
IndependentVideoPlaybackOwner.OpenAsync/PrerollAsync/Run/SeekAsync/ReadScrubFrame/CloseAsync目前仍同步reader。最小未来接入需要单pendingReadTask，Run等待期间继续出队控制；Seek同owner Flush/OnFlush+callbackDrain后SetPosition重新read，epoch过期不发布；Pause保留一个在途帧，Scrub只发一帧；Close先关准入再drain/release/MFShutdown/COMuninit/thread exit，callback Close不selfjoin且保护末ref。CancellableVideoReaderOwner.ReadAsync本次仅receive移到_gate外，不是上述默认迁移。先定位既有bridge零帧失败，保持原PauseSeekScrub/epoch/40framesEOS/COMproxy/CloseRelease/latelease合同，完整强组合过后才可默认接入。
OpenURL同步创建不可取消；音频不真实输出，不宣称可用。原loader副本/委托/fallback永久禁，合法external provider保留。audioendpoint0/AVI gap16/AVsync及cancel门/consumerHWloss/resources/旧1843/1752/91与fullOpen均不冲销。
Nativecompile本次NotRun：托管组合失败，productionfreeze前提不满足；不以未消费预算冒编译成功或ABI通过。
