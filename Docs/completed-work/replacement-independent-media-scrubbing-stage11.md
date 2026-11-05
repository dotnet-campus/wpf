# Stage11 独立媒体真实 scrub 与控制回归

## 正式结论：LimitedScrubAndControlValidated / CloseoutCompleted / OverallOpen
唯一既有系统任务 task78fa94bec0004a118ef8238bfe9e5818，原件 WpfGfxShape/artifacts/independent-media-stage11-r1/readback；预注册及分笔账 WpfGfxShape/Docs/handoffs/stage11-execution-receipt.md。本次亲自读写文档，主代理监督；源码/build/test/publish/sandbox/NativeAOT/终端脚本/再委派均0。不是新增运行或完整替换验收。

## 真实读取范围与唯一系统分母
前代理已读 scrub1–302、TRX1–26、监督1–40及controls1–240，是前读交接事实。本次另行亲读scrub1–85/86–160/161–235/236–302全部302行；controls先补241–320/321–400/401–480，再复核1–85/86–165/166–240，合计全480行；TRX1–26、监督1–40、stdout1–12、空stderr、aot-identity单行均实际读取。诊断读取1–35及43–65的宽范围输出有截断，另实读28–42及59–65完整尾，不冒称截断部分已全文读取。
TRX实际准确路径为 readback/Results/independent-media-stage11-r1/TestResults/child.trx，26行原件已亲读，不能据目录枚举遗漏说缺件。唯一FQN WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenScrubbingRunsThenRealPausedFramesAndControlsPass：total/executed/passed=1，failed/notExecuted=0。scrub与原controls为同一组合的两份业务证据，不计多个独立TRX。监督child exit0、exited、TimedOut/Cancelled/KillAttempted=false、OutputCollected=true，无drain超时/监督错误。diag59–65确认同一TRX产物及session exit0。

## scrub实际业务字段
所有快照hr0：preopen-scrub-rate0意图被接受；opened-paused为Paused/epoch0/PTS0/published0/UI0。第一次scrub seek PTS5000000/epoch1/published1/UI request=consumed=frame=1；第二次PTS25000000/epoch2/published2/UI全2。生产DrawVideo软件RTB读回2×1/stride8，红0000FFFF0000FFFF、蓝FF0000FFFF0000FF，不是按颜色循环追绿。
pauseTicks5160223=516.0223ms>=原515ms门；pause-before/after都Paused/epoch2/PTS25000000/published2/UI全2。关闭scrub再seek30000000为epoch3、published仍2、UI request3/consumed2/frame2，不发布新帧。随后预注册Rate1恢复，帧记录从PTS30000000按1000000增至39000000，epoch均3；连同两张scrub帧共12。Ended回调Close hr0/stateClosed/PTS39000000/published12/UI request3 consumed3 frame12。
COM packets为Opened1、NewFrame13×12、Ended7，均20bytes，HR字段0/字符串长度0，thread10，实际wall及frames.wallTicks已保留原JSON。最终CloseHr0/references0/registryRemoved=true/descriptorDisposed=true/Epoch3/PublishedFrames12/LastTimestamp39000000/Ended=true。nativeReferences明确Unknown，不填0。

## 同组合原controls回归
Open/Play/Pause/Stop/Seek/Resume均hr0；Opened/Playing/Paused/Stopped/SeekCompleted/Ended的Length40000000、CanPause1且hr0。WpfStatus audio1/buffering0/download1/buffered1均hr0，仅metadata和本地状态，不是音频输出。
暂停PTS5000000、count6前后不变；pauseWindowTicks5293486=529.3486ms>=515。Stop后seek25000000，epoch2蓝帧；Rate2=-2147467263（E_NOTIMPL）。恢复帧25000000至39000000，红蓝像素同上；总published21，Opened1/NewFrame13×21/Ended7。ClosedLength输出0、lengthHr/pauseHr=-2147483629（80000013）；CloseHr0/finalReferences0/registryRemoved=true/stateClosed/Failure0/ended=true/epoch2/descriptorDisposed=true。不能把此有限Close观察升级为所有关闭并发场景通过。

## 身份与执行账
监督wrapper SHA256 87EF97FE2A6907954EF95C597A92DD441CD7661B37C3F2240AAD3B13D545EECE，MVID b2ad717e-d118-47a9-8b64-3df732edae11；业务source-linked SHA256 F5839A11A860977CC0B3184A8674A4C170C2AC2DCD6EF4C528E361DA13ABD0E4，MVID 9dc5a999-3d09-4723-b4c9-65464bae8ccd。监督/identity/controls实际architecture X64、PointerSize8、runtime .NET10.0.13。业务backend明确source-linked production Av/GeneratedMediaPlayerResource→production DrawVideo software RTB，非NativeAOT。MF模块mfplat/mfreadwrite/mfsrcsnk 10.0.26100.9278，mfcore10.0.26100.1。
旧6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403 fixture仅PE初始化，不是当前源码新NativeAOT。历史normal2（Tests3825warnings/0errors、COM2480warnings/0errors）、tests1（68pure passed=原64+4，0fail/skip）、publish1、sandbox1、Native0，implicitDebug1/publishCompile1独列；发布及隐式编译warning总数Unknown，构建stdout截断按receipt保留。本次所有执行0，不开MP4、不补测试、不挪旧预算。

## 当前内部24槽与公开边界
|slots|真实能力与未支持边界|
|---|---|
|0 QI /1 AddRef /2 Release|有限identity/registry引用路径已验；回调中lastRelease与并发QI/退订/关闭NotProven|
|3 Open /4 Stop /5 Close|内部真实视频owner有限路径；Close是异步关闭请求，强取消阻塞ReadSample及全并发生命周期NotProven|
|6 GetPosition /7 SetPosition|最近视频PTS、有限seek及本轮paused scrub；不是音频clock/任意来源精确seek承诺|
|8 SetRate|preopen0及0/1有限视频语义；2明确E_NOTIMPL，其他速率未支持|
|9 SetVolume /10 SetBalance|E_NOTIMPL；无真实音频输出前不允许用保存变量假调音量/平衡|
|11 SetIsScrubbingEnabled|本轮preopen意图、暂停单帧真实seek、false只seek不发布有限已验；关闭拒绝等未单列系统字段不新增认证|
|12 IsBuffering /14 DownloadProgress /15 BufferingProgress /17 HasAudio|Stage10本地来源/真实流metadata有限能力；非网络缓冲/真实音频播放|
|13 CanPause /16 HasVideo /18 Height /19 Width /20 Length|真实characteristics、视频尺寸、duration有限能力；unknown metadata/duration不假成功|
|21 NeedUIFrameUpdate|请求/消费/帧代际有限已验；不是完整WPF视觉/派发验证|
|22 Shutdown /23 ProcessExitHandler|仅Close别名；完整shutdown/进程退出语义NotProven|
生产公开factory仍E_NOTIMPL/null，内部通过不代表公开MediaPlayer可用。原wpfgfx loader/委托/fallback禁止恢复；合法外部provider仍合法，不因禁止原委托误禁其他合法provider。
音频render/capture端点原四状态count0/默认端点失败不冲销，不虚听声。VolumeBalance、真实audio输出/AVsync、强cancel、未知nativeRefs、回调Release/并发退订/关闭、当前源码NativeAOT首次编译、完整WPF/完整替换仍Open/NotProven。原AVI缺16samples严格门失败、旧full红、MemoryDescriptiveOnly及资源Unknown保留，有限绿不冲销历史。

## 下一直接生产主任务（本次不执行）
受控Close/cancel并发生命周期合同与真实系统验证（含回调Release/退订/在途读取、无锁外部调用/selfjoin、关闭后拒绝发布、native refs可观测边界），并推进当前源码NativeAOT首次编译及公开工厂启用门审定；不得仅泛静态选门、提前打开factory或恢复原委托。音频无设备先保持真实阻断；完整替换OverallOpen。各入口新Stage11权威段优先于长历史旧排程。
