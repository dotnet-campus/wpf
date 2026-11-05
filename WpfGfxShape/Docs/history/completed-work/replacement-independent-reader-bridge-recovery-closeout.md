# Independent reader bridge recovery：ZeroFrameFixed / ControlsFailed / OverallOpen

## 本次后继原件复核补正
正式后继现为 WpfGfxShape/Docs/completed-work/replacement-independent-media-reader-control-integration-closeout.md，已实际新建并回读，根Docs同名保留旧报告。r4原TRX2/2/0 child0与bridge309行/十lifecycle清理全文/controls-scrub关键/duration/reopen/身份已核；本报告旧失败不覆盖。默认同步未改，本次新增执行全0，前次Native1只历史，OverallOpen。

## 后继正式收尾：control-integration-r4（不覆盖本轮历史失败）
新权威根Docs/completed-work/replacement-independent-media-reader-control-integration-closeout.md及.coding-agent/control-integration-receipts.md。task1d2617013bb84b818f71f64a9ee80eed原TRX2/2/0 child0，40帧EOS/资源与通知退订/跨Closelease/失败threadExit/controls524.1901ms seekepoch2/scrub/duration/十lifecycle关键与清理尾已亲读。本报告r1/r2原失败及r3新增辅助重复Attach失败保留；r4删的是重复新增辅助接线，非撤生产拒音轨/factory保护。后继轮累计normal0/tests1/implicitDebug1/COM2/sandbox2/Native1；唯一条件生产Native发布exit0，新control-integration-final-native-x64 DLL/PDB存在/hashUnknown，非Native媒体导出运行。默认仍同步无async迁移源码，独立三门仅slice/Open不可取消。下一直接实现owner单pendingReadTask+控制等待/FlushOnFlushdrain/epoch/有界Close，不再停纯设计。旧91/audioendpoint0/AVI16gap/AVsync/cancel/HWloss/resources/fullOpen不冲销，原版loaderfallback永久禁。以下全为原bridge轮真实历史，不以新通过改写旧失败。


## 根因、边界与最小改动
原 integration-r1 taskc5efeb3cfe69427da5cc6d22cd42c9f8 TRX5/4/1，bridge40/0，Failure -1072875852 = 0xC00D36B4，MF_E_INVALIDMEDIATYPE。不是decoder零帧或callback锁外变更导致。SystemMediaSource.Create 无条件 RequireVideoOnly=true；公开 MediaExports 与内部资源桥共用入口。ObserveOpenAsync 在 owner.OpenAsync 后、Preroll 前拒绝真实 HasAudio=true。MediaFoundationSourceReader.Create 明确先读取 native audio major type，再取消音频选择，只选择视频。WriteSourceReaderAvi 默认4秒40帧/8kHz16bit PCM；既有 IndependentMediaControlsTests 要求内部 HasAudio=1，故内部 AV 视频源不应被公开音频能力安全限制拒绝。

实际生产仅两文件：SystemMediaSource.Create 加可选 requireVideoOnly=false，赋给 identity；MediaExports.CreateMediaPlayer 显式 true。公开音频输出能力未被冒充；公开 factory 仍严格拒绝带音轨，内部只解码视频。没有恢复 loader、副本、委托或 fallback。没有改全部 fixture 为video-only，没有吞Failure，没有改40帧/颜色/阈值/skip。

测试：实际公开导出函数指针验证 RequireVideoOnly；内部bridge断言false；增加真实 HasAudio=true/Failed/0frames/threadExit 负控，放入原组合门。原 AV 正向和最终音频输出验收保留。

Before 已有40/0；After r2 bridge业务 passed=true、40帧、真实DrawVideo红蓝、lease跨Close安全。中途r1揭露旧Close oracle已落后：当前 AcquireFrame 的普通Closed返回S_OK/null，专门 IndependentMediaFrameLifecycleTests 已覆盖，终态/活动frame owner关闭仍RO_E_CLOSED。仅将桥普通Close断言对齐现有合同并检查null，未改生产取帧行为。回撤范围仅上述策略参数与测试；不得恢复原版fallback。

## 执行总账与真实分母
新增预算 normal<=3/tests<=2/COM<=2/sandbox<=2/Native<=1。实际 normal0/tests2/implicitDebug2/COM2/sandbox2/Native0；COM及sandbox额度耗尽。不再追跑，不做未验证生产修复或异步开关。无终端、脚本、再委派；系统MF仅Windows沙箱。
- unit1 默认Debug无RID，实际生产函数指针/资源调用27passed/0failed/0skip，28ms，exit0；stdout1638943字符截前1618943。unit2加生命周期合同35passed/0failed/0skip，31ms，exit0。重复用例不合称62独立case。
- COM1/2 正式 OfficialWindowHangDiagnostic/net10.0-windows/win-x64/selfcontained Core/Desktop10.0.13/PublishAotfalse/ManagedAcceptancetrue/ComAcceptanceSandboxtrue；显式既有认证 Native、original-clean-x86/ReferenceData、trusted glyph 输入。bridge-recovery-r1及r2均publish与clean分离。两次exit0；stdout1705237/1639098字符，截前1685237/1619098；warnings总数Unknown，不称无warnings。没有单独normalbuild。
- r1 task2c5aa2c27635402f8386c22e5853bd2b：原TRX2/1/1 child2。bridge40帧恢复但旧普通Close oracle失败；controls/scrub NotReached；lifecycle1Passed。
- r2 task09b8edaaabbf44d2af1ee44ac7057111：原TRX2/executed2/passed1/failed1，child2。bridge组合6.8891406秒 Failed，lifecycle5.5466892秒 Passed。桥业务及公开音轨负控均完成，但controls事件CollectionAssert expected[1,7]（过滤13）实际第二项11；生产ObserveOpenAsync明确raise1及11，事件契约仍需原业务核实，不改oracle追绿。scrub NotReached。两次各2TRX，累计4执行实例/2passed/2failed，不是4独立方法。
两次监督均120秒，无timeout/cancel/kill/outputDrain超时/SupervisorError，正常exited；工具独立OSexit Unknown，不能以child2冒工具exit。

## r2业务原件与身份
根 WpfGfxShape/artifacts/bridge-recovery-r2/readback/Results。supervision.json全文、TRX主要结果与尾Counters已读。
independent-media-stage15/bridge.json：passed=true；PublishedFrames40/ThreadExitedtrue；subscribed=atUnsubscribe=afterUnsubscribe=11，resourceBeforeDelete=AfterDelete=32。PTS0.5s像素0000FFFF0000FFFF，PTS2.5s FF0000FFFF0000FF；lateLeasePixels保留红色、leaseDisposedtrue。missingFailure=0x80070002、missingOwnerThreadExitedtrue，卸载后260.6539ms仍11。尾reader.Release0/MFShutdown0/CoUninitialize。最终Failure是有意missing负控，不是播放失败。native/lease底层引用Unknown。公开音轨负控JSON全文：Failure0xC00D36B4/Failed/HasAudiotrue/frames0/ThreadExitedtrue。
controls result.json：Pause前后position5000000/count6，pauseWindowTicks5287036=528.7036ms；Seek epoch2、PTS39000000、红蓝像素均已到达；随后事件断言失败，不能称controls全通过。lifecycle是1个TRX，内部10子场景不得加到分母；本轮未逐份全文复核10JSON，不冒独立10Passed认证。
COM SHA72FC23ABADC36CA523B296F85AD37653C8F0B28BFB76CCF161BAE5F24BE0339A/MVID3abeb228-811b-433b-a5d5-bfd155d06e20两包wrapper未变；r2 source-linked Tests SHA C8DAC05BC69F15FDCCB5B9D09C50A94662B7FDC274DE30500E7BB0DF95CD173B/MVIDee997c64-c3b2-4d54-86fe-397f8e8a1de8，才反映桥测试修正。实际graph COM wrapper→source-linked Tests→ManagedAcceptance→IndependentMediaIdentity→同步IndependentVideoPlaybackOwner→系统MF reader→frame lease→GeneratedVisual DrawVideo。mfplat/mfreadwrite来自SYSTEM32版本10.0.26100.9278。fixture Native SHA D9AA1E0ED5E99B29F10AC2DA4635D7F014B0F433C05DF2E5B18F2028836CD09C仅既有fixture，不是本轮Native媒体ABI认证；没有新current-source Native。

## 默认异步与下一直接生产任务
默认仍同步，未接入单pendingReadTask；没有半成品需撤回。预算已不能验证完整迁移。下一首先核controls事件11（生产已发的preroll事件）对应原native/接口合同及调用消费，修真实归属或有据更新旧oracle，重新验证原bridge+controls+scrub强组合，而非再跑三reader微门造进度。
随后实质实现 IndependentVideoPlaybackOwner.Run 的单pendingReadTask/MTA控制等待，PrerollAsync异步结果接续，SeekAsync 在 Flush/OnFlush/callbackDrain 后设置Position并推进epoch，ReadScrubFrame单帧等待；Pause保留最多一个pending，Close/Release在callback上下文不selfjoin、旧lease跨Close有效。必要时有限internal reader adapter，不新接口/InternalsVisibleTo。必须同包验证40/EOS/DrawVideo/通知卸载/lease/失败threadExit/Pause515ms/Seek/Scrub/Close后才可迁移默认；本轮没有宣称默认取消可用。
旧 full1843/1752/91、x64timeout分母Unknown、audioAVI16gap/endpoint0/AVsync/强cancel/HWloss/resourcesUnknown/完整替换Open全部保留。全媒体最终验收不删，不虚声输出。Native未跑，因为托管主要组合仍失败，未满足条件。
