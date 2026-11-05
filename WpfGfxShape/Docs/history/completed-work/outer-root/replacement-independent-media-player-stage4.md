# 独立媒体 player stage4 r2：固定 PTS 发布回调 DrawVideo→软件 RTB 有限验证

## 正式终态：LimitedProductionDrawVideoValidated / CloseoutCompleted（仅当前 slice）
本轮亲自读写既有证据和文档，主代理监督；无终端、脚本、再委派，新增源码/build/tests/publish/沙箱均0。当前有限结果已正式收尾，六状态入口同步；不是完整播放器或完整替换收尾。下方原预注册、before/after及执行历史保留，旧“当前新增均0/ImplementationInProgress”只描述当时，不覆盖本节累计账。

### 已亲读原件与实际结果
证据根为`WpfGfxShape/artifacts/independent-player-render-r2/readback/`：
- `Results/independent-player-render-stage4/result.json`全部190行：40个发布记录，PTS0..39000000、步1000000（100ns），PublishedFrames40、Ended=true、LastTimestamp39000000、elapsed3.9436899；两处软件RTB 2×1/stride8像素：PTS5000000/wallTicks5420689=`0000FFFF0000FFFF`，PTS25000000/wallTicks25310046=`FF0000FFFF0000FF`。末帧wallTicks39384822。
- `Results/independent-player-render-r2/TestResults/child.trx`全部26行：准确类IndependentVideoPlaybackOwnerTests，方法WhenProductionDrawVideoRendersThenTimedRedBlueReachSoftwareRtb；total1/executed1/passed1/failed0，duration4.0025476秒。
- 同目录`supervision.json`全部40行：taskc5b5092e333046c5a0ddb8167674091d，120秒监督，child exit0，TimedOut/Cancelled/KillAttempted=false，OutputCollected=true，drain无超时；原外180秒约定保留，外层工具原return本次不在可重审日志中，不补造父exit。
- 同目录stdout.txt全部12行（存在解码乱码），可见总计1/失败0/成功1；计数以TRX为准。`Results/aot-identity.json`全部1行已亲读。
- 最后定向run_tests日志立即读取1..61行，确认是本次receipt对应日志而非别轮：第60行失败0、通过16、跳过0、总计16、81ms/net10.0。不能写18。准确filter为`FullyQualifiedName~WpfGfxShape.Tests.GeneratedMediaPlayerResourceTests|FullyQualifiedName~WpfGfxShape.Tests.SystemVideoSampleTests|FullyQualifiedName~WpfGfxShape.Tests.MediaCreationUnavailableTests`，目标`WpfGfxShape/Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj`。

### 采样要求与通过边界
真实新生产managed/source-linked路径：专用MTA owner→IndependentVideoFrameOwner→production `GeneratedMediaPlayerResource.BindIndependentPlayer`（订阅FrameReady）→既有GeneratedVisualRenderer.Render/Capture DrawVideo→SoftwareImageRenderSession→Direct3D9SoftwareImageRenderer→BitmapRenderTargetExports软件RTB，再从RTB bitmap读取像素。不是仅lease.CopyPixels或创建对象；40帧发布/EOS与两个固定PTS点的实际DrawVideo消费通过，不宣称40帧逐帧RTB绘制。
两次绘制在PTS等于5000000和25000000的发布回调内触发，wallTicks是回调时刻记录，不是独立墙钟触发或采样。保留原预注册“红蓝时刻独立断言”的要求，不事后改要求宣布更强验收。独立墙钟采样、显示/物理scanout、HW、完整播放状态机均未认证；本次只允许当前slice的LimitedProductionDrawVideoValidated。

### 关闭与引用：三层证据不能混同
1. 直接业务输出只有`IsReleased=true`，另有Ended=true；没有LateLease、最终全部ref0、系统模块清单或具体reader/sample释放计数。这些均Unknown，不借stage3数据补齐。
2. TRX Passed结合亲读测试`Tests/WpfGfxShape.Tests/IndependentVideoPlaybackOwnerTests.cs:11..93`，支持40帧/EOS/两个点/至少3.8秒、真实RTB双像素相等、await CloseAsync完成、resource.IsReleased强断言、关闭后Frames.Acquire=80000013与重复CloseAsync完成。断言在业务写出之后的也由Passed佐证，但不是JSON直接字段。回调中Assert.IsFalse(closed)不是主动观察关闭后晚回调的独立窗口，不能宣称晚回调实测完整覆盖。
3. 亲读生产源码`Core/GeneratedMediaPlayerResource.cs:39..60,111..131`：重绑先退订旧FrameReady，ReleaseSystemMedia退订并清空player/frames/system listener、注销composition、Release系统media；`Core/Av/IndependentVideoPlaybackOwner.cs`：同MTA串行Read/发布/reader using清理，Close取消定时等待/CompleteAdding，Run finally Dispose Frames后完成_closed，关闭完成排在回调之后。这是ownership/解绑路径源码证明，不是所有引用最终0的运行计数。RTB bitmap/target在测试finally调用Release但未记录返回值；晚lease跨Close存活与最终全ref0仍Unknown。原生阻塞ReadSample不能即时取消，不冒nativecancel通过。

### 实际身份分层
- 监督wrapper `WpfGfxShape.ComAcceptance.dll` SHA256=`DA77EA7FEEB1F8D86727E24BCA6DBEE258F2F7C79703EC1F17312BFBE603ED31`，MVID=`ccc1ed80-5f1d-4e98-b3f1-1cb70685129a`；实际runtime .NET10.0.13/X64。
- 业务`testMvid=6c811de5-e0bb-4fc1-8f45-95d45a251b54`对应wrapper调用的source-linked Tests程序集（由extern alias Stage3Production桥接），不是wrapper MVID；本次该程序集SHA及各managed产品DLL SHA无直接输出，Unknown。
- `aot-identity.json`实际Native fixture SHA=`6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403`，沙箱Native/wpfgfx_cor3.dll、X64/pointer8；只是既有fixture初始化，不是本次独立媒体执行路径或新NativeAOT认证。本次系统MF模块路径/版本无输出，Unknown，不沿用stage2/3版本冒本次实测。

### r2累计执行账与原件审阅边界
完整账见`WpfGfxShape/artifacts/independent-player-stage4/execution-r2.md`，本次已亲读并补齐：normal1/3（既有receipt成功、3734warnings/0errors、8.63秒，62行日志曾读；3129029字符前3109029被截断）；tests1/2（实际16passed/0failed/0skipped，隐式Debug编译1）；COMpublish2/2（首MSB3030/exit1，因fixture路径不存在；仅修正到同SHA现存路径后成功，隐式publish编译2单列）；sandbox1/2（上述TRX1passed、child0）；Native0。
首publish既有receipt59行、1623785字符前1603785被截断；成功publish原始工具日志已不在当前最后日志槽，完整参数/计时/warnings等本次不可重审，Unknown，不以当前test日志代替。定向test日志原输出1565149字符、前1545149被截断，不能声称warning0。readback产物、实际运行身份和完整业务/TRX/监督可独立支撑本有限结果；缺旧完整日志不无限阻止收尾，也不补造日志。旧轮NETSDK1152、零命中exit8、stage1–3失败与stage4旧历史一律保留；本轮0新增运行不是把历史累计抹零。

### 未关闭合同与下一具体实施
旧官版wpfgfx委托许可已撤销，生产主动加载/副本/fallback不得恢复；合法调用者提供的external provider QI/transport/BeginComposition、BeginRender-End合同保留，不等主动官方回载。公开工厂在IMILMedia基本合同未完备前继续E_NOTIMPL/null，4negative是安全失败而不是播放通过。
下一在现有生产Av/MediaExports与GeneratedMediaPlayerResource层落实独立IMILMedia COM identity：按权威ABI实现IUnknown/QI/AddRef/Release和基本IMILMedia状态方法、真实identity与引用所有权，接既有MTA owner及Open/Playing/Ended/Closing/Closed状态；将资源注册/transport持有释放接到该identity，保持当前BindIndependentPlayer/DrawVideo消费路径，接真实状态/失效事件而不伪造空vtable S_OK。事件proxy、seek/rate、audio同步、nativecancel/flush-drain、晚lease/全ref0及NativeAOT后续实现验证，未完成前不切成功工厂。
ProductionPlayerOpen/OverallOpen；IMILMedia COM identity/public factory/audio/events proxy/seek-rate/nativecancel/NativeAOT均Open；原full显示/HW/productloss/audio/naturalresource/finalfull未关闭，旧失败不迁移新SHA，memoryGreen=null。本次有限收尾及六状态同步已完成，无文档待同步尾项。

## 以下为保留的原预注册与执行历史

## 预注册（本轮，不覆盖 stage1–3 历史）
状态：ImplementationInProgress / ProductionPlayerOpen / OverallOpen。预算 normal build≤3、定向 tests≤2、COM publish≤2、sandbox≤2、Native0。禁止原 wpfgfx 主动加载、复制 fallback、脚本、终端、委派；合法 external provider ABI 保留。

Before：SourceReader 同调用者 MTA 同步读取；frame owner 可安全借出独立 bitmap；资源只能手动绑定，尚无独立 player identity、播放线程或时钟。After 目标：先实现可被等待的内部独占 MTA 播放 owner；未完成 COM identity/资源注册/DrawVideo 验证不能称闭环。

|对象|创建/访问/释放 owner|引用契约|
|---|---|---|
|SourceReader/系统 MF|唯一 MTA 专用线程|Create/Read/Close 均同线程；sample/buffer 在 Read 返回前释放|
|帧 owner|播放线程 Publish，消费者 Acquire|替换/Close 不释放消费者自有 bitmap lease|
|bitmap lease|consumer|持至 DrawVideo 消费完成后 Dispose；不得借 Source 跨 Dispose|
|独立 COM player|尚未实现|不得猜 IID/slot 或开放默认工厂|
|external provider|既有调用者 COM|保留既有 QI、BeginComposition/BeginRender-End 配对|

默认工厂切换门：真实 IMILMedia 全基本契约、identity/QI/ref、资源 transport、事件以及关闭语义验证前，Create 保持 E_NOTIMPL/null。
线程/取消契约：无 UI 同步等待、无 sync-over-async。单 FIFO command queue，专用 MTA 独占 reader；命令完成均可 await，不后台遗失任务。PTS 单位100ns，Stopwatch 单调时间对应首帧 PTS；先不丢迟到帧（迟到立即发布），不得按颜色等待。EOS 保持末帧至 Close。Stop/Close 通过取消信号唤醒定时等待，再由同 owner 串行清理；无法即时打断 native 阻塞 ReadSample，Flush 不能被宣称即时取消。Close 完成后不得再通知。
验收预定义：固定原创 AVI 40帧/10fps，单 FQN 监督120/外180秒；红蓝时刻独立断言并走真实 Renderer.DrawVideo/RTB，记录 PTS/wall/invalidation/EOS/引用清理和身份。只有静态一帧/创建对象不是连续播放。软件 RTB 不是硬件或音频认证。未实际执行项保持 Open。

## 执行账
前轮历史不能由本轮冲销：task9317f672c2e840a78c785e58e45c58cb 的 supervision.json 40行及 TestResults/child.trx 26行已亲读；executed1/passed1/failed0，child exit0，.NET10.0.13 X64，testSHA426818D3EC1C3FDFB8A1F8625EEFFD2E253DCFC621C51CF2646B4461EB352341/MVIDc0c40521-b123-4b5f-af9a-91dfd284660a。40帧PTS0..39000000/EOS、约3.9349秒仅为owner→BindIndependentFrames→AcquireSystemFrame，不是Render/DrawVideo/RTB。task26fc877331dd4260899c1f33976c5bc9的监督40行/TRX22行已亲读：executed0/passed0、exit8，不是18例回归通过。前publish曾NETSDK1152失败，须保留；旧轮normal/tests/publish准确次数尚待原始执行账核对，不填猜测数字。前轮两次sandbox、Native0。

本次新增授权独立计账：normalbuild≤3、定向tests≤2、COMpublish≤2、sandbox≤2、Native0；当前新增均0。隐式编译单列。每次工具参数/原return/截断/exit/task须落盘；不以source-link托管通过声称NativeAOT。

## 本次before/after与ownership预注册
已静态核实GeneratedVisualRenderer.DrawVideo直接从GeneratedMediaPlayerResource.AcquireSystemFrame取得bitmap lease（查询实际尺寸/AddRef），不强制IMILMedia identity；ProcessCommand外部provider仍有独立QI/transport契约，不能混淆。选择既有internal owner注册，不伪造S_OK空vtable；独立COM player identity仍Open，公开工厂E_NOTIMPL/null。
Before：BindIndependentFrames只赋帧owner并通知一次，无播放更新订阅。After目标：内部资源注册/播放通知/释放解绑→既有GeneratedVisualRenderer DrawVideo→offscreen软件RTB；预注册平台0.5秒红、2.5秒蓝，固定40帧AVI、不按颜色轮询、不改变阈值。必须从RTB读像素而非lease.CopyPixels。
所有权：reader/发布/清理唯一MTA线程；frame owner保持最新帧；resource订阅持有owner，Release解绑；renderer Acquire拥有独立sample/source引用，完成渲染释放；Close完成后无晚通知、queue退出及晚lease释放必须实际验证。Stopwatch/100nsPTS保持；native阻塞ReadSample不可即时取消，未实现不得冒取消通过。audio/eventsproxy/seek-rate/nativecancel/NativeAOT/publicfactory及完整替换继续Open。
