# 独立媒体 stage6：ControlsEventsVideoValidated / CloseoutCompleted

## r4 最终终态（覆盖下方实施中历史，不删除历史）
最终依据 task `0b645f5a0b87442682afb1316e508fc3`，回收根 `WpfGfxShape/artifacts/independent-media-stage6-r4/readback`。本次仅亲自读写文档、主代理监督，源码/build/tests/publish/沙箱新增全0，终端脚本/再委派0。原 sandbox 工具 return 未交接保存，明确按本次用户交接引用绑定 task，再用监督/业务/stdout 路径核对；不补造原 return/工具 exit/完整回收数量。详见 [回读记录](../../WpfGfxShape/artifacts/independent-media-stage6/r4-closeout-readback-receipt.md) 及 [修正终态](replacement-independent-media-controls-events-stage6-correction.md)。

### 精准原件与最终强门
以下相对路径均位于上述 readback 根：
- `Results/independent-media-stage6-supervision/TestResults/child.trx` 1–26：单 FQN WhenControlsAndEventsRunThenPauseSeekResumeReachProductionDrawVideo，1 executed/1 passed/0 failed/0 notExecuted，duration 2.6479597s。TestResults 未被目录枚举列出，但按 stdout 第5行准确映射读取成功，绝非缺失。
- `Results/independent-media-stage6-supervision/supervision.json` 1–40：child exit0，Reason=exited，120秒监督；TimedOut/Cancelled/KillAttempted均false，OutputCollected=true，drain超时false，SupervisorError=null。原调用外超时180秒沿交接与既有执行合同，不冒称原return已读。
- `Results/independent-media-stage6/result.json` 1–414：Pause前后 GetPosition S_OK/5000000、HasVideo S_OK/1；NaturalSize DTO真实 widthHr0/width2/heightHr0/height1，两次一致。发布count6→6，PTS5000000→5000000；pauseWindowTicks5235010=523.501ms ≥ expectedPause515ms下限5150000ticks。对应既有测试固定Delay515ms及下限强断言，不沿用r3的500ms。
- Stop/Seek/Resume均hr0；Stop→Seek25000000，最终epoch2；旧epoch0仅PTS0..5000000共6帧，新epoch2仅25000000..39000000共15帧，无epoch1或旧目标前帧，published21/endedtrue/Failure0。Stop置0与Stopped、Seek后Paused由既有强断言结合此TRX支持，不伪造JSON字段。Rate2=-2147467263/E_NOTIMPL。
- production DrawVideo软件RTB真实samples：PTS5000000，2×1/stride8，`0000FFFF0000FFFF`（红）；PTS25000000，`FF0000FFFF0000FF`（蓝）。这是两固定PTS发布回调中的真实渲染，不是逐帧draw、独立墙钟draw/HW/物理显示认证。暂停窗口则是独立固定墙钟观察。
- packets/eventKinds真实顺序Opened(1)→21个NewFrame(13)→Ended(7)，通知线程10，帧线程11；Ended回调重入Close（既有ControlEvents.Receive代码）返回CloseHr0。最终stateClosed/finalReferences0/registryRemovedtrue/descriptorDisposedtrue，均直接字段；关闭后Acquire80000013由强断言+Passed支持。`nativeReferences`明确Unknown，individual backend counters not exported，绝不写全部native refs0。
- `Results/aot-identity.json` 1：旧Native SHA `6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403` 仅夹具初始化。不是本次新NativeAOT。
- stdout1–12确认单测试1成功0失败0跳过及准确TRX路径，中文显示存在编码乱码，未据此还原未知文本；stderr空。diag `log_261030115144405.diag` 1–65关键段已读：1832 discovered不等执行数、目标方法开始/完成/Passed、TRX同路径、session exit0；长行工具输出截断边界保留。

### 身份分层
监督wrapper WpfGfxShape.ComAcceptance：SHA256 `784C55BE8863D0C74D614386327FD548F6C3B7727BB840E7DF777E06A3EC547B`，MVID `c1742d32-420d-4105-a5e4-8f38c78cf880`。
业务source-linked Tests/production Av与GeneratedMediaPlayerResource：SHA256 `A9B1E204D5E5789D707FD10D608475D96F33B52213202A4F01CF51A7E51B6A23`，MVID `82d031a0-8d5d-432c-bbfc-11a803b19adb`。两层均.NET10.0.13/X64；wrapper通过Stage3Production别名调用MediaFoundationResourceTests.WhenComControlsPauseSeekResumeThenRealEventsAndDrawVideoRemainOrdered，不把两个MVID混成一个。业务列出系统MF模块mfplat/mfreadwrite/mfsrcsnk 10.0.26100.9278、mfcore10.0.26100.1；这些不认证新NativeAOT。

### 生产修正、预算与未关闭边界
已有production HasVideo/NaturalSize共用成功Open+非零真实视频metadata及有限可用状态判定；Closed/Opening/Failed等不冒可用。已有定向33unit passed/0failed/0skipped（旧19+新14，含4工厂negative）仅据correction-test1-receipt，不冒新运行或系统TRX。公开factory继续E_NOTIMPL/null，HasAudio未支持，合法外部provider保留；原版wpfgfx生产主动loader/副本/fallback持续禁止。
Stage6累计normal2/tests3/COMpublish5/sandbox4/Native0；implicitDebugCompile3/publishCompile5单列，含首NETSDK1152失败发布编译。r4补入的是既有第4次sandbox，不是本次新增执行。来源日志stdout截断及未知warning/TRX/hash字段按receipt保留。r1较弱Passed、r2 HasVideo0失败、r3仅500ms且无NaturalSize DTO的较弱Passed全部历史保留，均不替代r4。
仅ControlsEventsVideoValidated/CloseoutCompleted，不是完整player/公开factory/audio/任意rate/强取消/完整shutdown/NativeAOT通过。ProductionPlayerOpen/OverallOpen、旧full红、资源Unknown与memoryGreen=null不变。下一直接实际实现：MF音频解码→WASAPI输出→音视频时钟同步，不再重开本链文档或泛选门；本次不启动音频源码或执行。

## 以下为 stage6 实施中历史（额度/排程不得覆盖本次零执行）

## Before / After 与验收边界
Before：播放命令同步读到 EOF，owner 队列不能处理暂停/定位；rate0、Stop、Seek 和事件接入未实现。
After 目标：MTA owner 在每次读取及等待帧 PTS 时处理命令；暂停不读/不发布，恢复重锚单调 clock；Seek 在同 owner Flush + SetCurrentPosition，丢弃目标前解码帧、隔离旧 pending epoch；Stop 定位 0 并停止。
公开工厂继续 E_NOTIMPL/null，禁止原版 wpfgfx loader、副本、fallback。合法外部 COM provider 保留。未完成不称完整替换。

## 所有权与状态转移合同
|动作|前态|完成态|执行与所有权|
|---|---|---|---|
|Open|Closed|Opening → Opened / Failed|reader 创建、使用、释放均在 MTA owner|
|rate1|Opened/Paused/Stopped|Playing|恢复以第一待发 PTS 与 Stopwatch 重锚|
|rate0|Playing|Paused|owner 命令完成后不再发布帧；已在途回调可完成|
|Seek|Opened/Playing/Paused/Stopped/Ended|Seeking → 原控制态|Flush、VT_I8 SetCurrentPosition；旧 pending 丢弃、epoch 增加|
|Stop|已打开|Stopped|同 owner Seek(0)，不继续读|
|EOS|Playing|Ended|每个播放 epoch 一次 Ended|
|Close/last Release|任意|Closing → Closed|先禁止新通知，owner 退出后释放 reader/frames；已有 lease 自持引用|
|失败|操作中|Failed|保留真实 HRESULT，不能 S_OK 冒已执行|

COM 方法 S_OK 表示已接受异步操作；Completion 表示命令已执行（不是播放 EOF，原播放 completion 另保持）。不在回调中 Join/同步等待 owner；不持 identity 锁等待通知线程。同步 ReadSample 不能强取消，Close 等在途读取返回后才完成，不以超时放任工作冒已关闭。reader → sample/buffer → MFShutdown → COM uninitialize 沿现有 Dispose 顺序。

## 回撤
以本报告列出的 Av 及现有 Tests 文件为最小回撤单元；不得回撤到原版委托。公开 factory 门不变。出现真实业务失败只按确定根因修正，不改 expected/阈值/skip 追绿。

## 剩余完整后端
音频真实 WASAPI/MF renderer、音视频时钟、非 0/1 rate、scrub/progress/length、强取消与完整 shutdown、公开 factory、NativeAOT x64/x86、真实 WPF/HW/loss/full、自然资源趋势仍 Open。旧 full 红、memoryGreen=null 不冲销。

## 本轮预算账
上限 normalbuild4 / targeted tests3 / COMpublish3 / sandbox3 / Nativepublish0；隐式 Debug/publish 编译单列。继承已执行 normalbuild1/4（Release 3738 warnings/0 errors）与 targeted tests1/3（旧合同19 passed/0 failed/0 skipped；非新系统控制用例），implicitDebugCompile1；COMpublish0/3、sandbox0/3、Native0。原件 WpfGfxShape/artifacts/independent-media-stage6/build1-receipt.md 与 test1-receipt.md 已亲读。下一正式发布 independent-media-stage6-r1/clean，真实桥接 FQN=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenControlsAndEventsRunThenPauseSeekResumeReachProductionDrawVideo，调用 source-linked Tests.WhenComControlsPauseSeekResumeThenRealEventsAndDrawVideoRemainOrdered；单桥接仅1条TRX。监督120秒/外180秒。SDK10.0.107；Tests net10.0；不改 SDK/TFM/依赖。文档抓取 SDK SetCurrentPosition 页面 HTTP504（request_id 65340a23-ec1b-485b-93df-51ce0abfb337）；需核权威头再调用。
