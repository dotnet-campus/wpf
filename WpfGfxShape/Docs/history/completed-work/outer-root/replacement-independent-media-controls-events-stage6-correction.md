# Stage6 修正最终终态：ControlsEventsVideoValidated / CloseoutCompleted

## 最终裁定与回读
已亲自完成r4既有原件回读和正式收尾，不再运行。权威 [stage6正式报告](replacement-independent-media-controls-events-stage6.md) 逐项列明原件行号、身份、强门与未知边界；[原调用归属记录](../../WpfGfxShape/artifacts/independent-media-stage6/r4-closeout-readback-receipt.md) 先登记原return缺失及用户交接引用，不用r3 return冒r4。
最终task0b645f5a0b87442682afb1316e508fc3；readback/Results/independent-media-stage6-supervision/TestResults/child.trx按stdout精确路径映射已读26行，1passed/0failed/0notExecuted，监督40行childexit0；业务414行、identity1行、stdout12行/stderr空及diag65行关键段已读。目录漏列不是原件不存在，不重跑。
最终强门：固定expectedPause515ms及下限强断言，实际5235010ticks=523.501ms；count6→6/PTS5000000不变；原COM GetPosition S_OK/5000000、HasVideo S_OK/1、NaturalSize两次真实widthHr0/2、heightHr0/1。Stop→Seek25000000→Resume均hr0，epoch2仅25000000..39000000新蓝区15帧，旧epoch0六帧，published21/endedtrue。Stop归零/Stopped与Seek后Paused由既有源码强断言+TRX通过支持。
真实production DrawVideo软件RTB固定PTS5000000红0000FFFF0000FFFF/25000000蓝FF0000FFFF0000FF，2×1/stride8；Opened1→21 NewFrame13→Ended7；Ended回调CloseHr0，最终Closed/COMfinalReferences0/registryRemovedtrue/descriptorDisposedtrue直接已证。全native refs仍Unknown，不能补编0；软件回调采样不外推HW/物理显示。
Wrapper SHA784C55BE8863D0C74D614386327FD548F6C3B7727BB840E7DF777E06A3EC547B/MVIDc1742d32-420d-4105-a5e4-8f38c78cf880；source-linked Tests SHA A9B1E204D5E5789D707FD10D608475D96F33B52213202A4F01CF51A7E51B6A23/MVID82d031a0-8d5d-432c-bbfc-11a803b19adb；.NET10.0.13/X64，旧6BDF Native只fixture，Nativepublish0。
已有production HasVideo/NaturalSize有限状态+真实metadata修正及33unitpassed（非新跑）生效；默认factoryE_NOTIMPL/null、HasAudio未支持、原版loader/副本/fallback禁止不变。r1/r3较弱Passed与r2真实失败全部保留，不升级历史。
累计normal2/tests3/COMpublish5/sandbox4/Native0，implicitDebug3/publishCompile5（含首NETSDK1152失败）单列；本次源码/build/tests/publish/沙箱全0，终端脚本/再委派0。原return未知、来源stdout截断、未知warning总数/旧TRX路径/hash/MVID不补造。
正式report/correction及next-session-handoff/current-work-item/current-stable-baseline/planstatus/finalmatrix/完整方案/当前执行规则同步并逐份回读。关闭仅本控制/事件/视频slice，不是完整player/publicfactory/audio/cancel/NativeAOT。ProductionPlayerOpen/OverallOpen、旧full红/memoryGreen=null保留。下一直接MF音频解码→WASAPI输出→时钟同步实际实现；本次不启动。

## 以下为修正过程历史（保留失败及当时额度，非现行排程）

## Before / After
最新 r2 task f28b6bc80a7f4b01b6e3b83f4f260f54：Pause GetPosition=S_OK/5000000，HasVideo=S_OK/0，system 1 Failed/exit2。r1 task987ef411662f4757ae8dd0aa5514e350 较弱 Passed 仅历史，不能替代。
修正目标：已成功打开且真实 RGB 视频元数据宽高均非零时，Opened/Playing/Paused/Stopped/Seeking/Ended 均保留视频可用性；Opening/Closed/Closing/Failed/未Open、无视频元数据为false；自然尺寸共用同一事实判定，避免失败后泄露旧尺寸。HasAudio保持E_NOTIMPL/0，不声称真实音频输出。

## 状态与 owner 表
|状态|视频元数据查询|owner与资源|
|---|---|---|
|Closed/Opening|false/0|未成功Open，不冒已就绪|
|Opened/Playing/Paused/Stopped/Seeking/Ended|宽高均非零才true/真实尺寸|同一MTA reader；Seek Flush/SetCurrentPosition/epoch隔离|
|Failed|false/0|不可用，Close统一释放|
|Closing/Closed after Close|Closed HRESULT且输出0|禁止新通知；已在途通知允许收尾；异步等待owner，不在回调selfjoin|
音频only源当前不支持：source reader要求视频流及RGB32输出，不把任意非视频源标true。Opened/Ended/Failed仅真实操作产生；公开MediaExports仍E_NOTIMPL/null，不恢复原wpfgfx委托。

## 历史账（已亲读8份receipt）
normal1：绝对Tests Release/net10.0 build，exit0，3738 warnings/0 errors；stdout3131925前3111925截断。
tests1、tests2：各19passed/0failed/0skipped，exit0；各隐式Debug编译1，非系统控制。stdout1570763/1570992，各前1550763/1550992截断，TRX路径Unknown。
COMpublish1：exit1 NETSDK1152，漏PublishAot=false导致runtime native与托管同名；COMpublish2/3：exit0，显式false+WpfGfxShapeManagedAcceptance=true，r1/r2独立clean与publish。3次隐式编译，Native0；实际SDK10.0.402。publish2 stdout1629851前1609851截断，publish3 stdout1015033前995033截断。
sandbox1：task987ef411662f4757ae8dd0aa5514e350；sandbox2：taskf28b6bc80a7f4b01b6e3b83f4f260f54，正式单FQN监督120/外180，全目录回收。最新失败不得覆盖。
继承累计normal1/tests2/implicitDebug2/COMpublish3/publishCompile3/sandbox2/Native0。
本次追加限额normal≤2/tests≤2/COMpublish≤2/sandbox≤2/Native0；当前追加全0。每次执行后即时登记准确参数、exit、截断及task。

## 强门与边界
保留Pause固定墙钟≥515ms/PTS不变、Stop→Seek25000000/epoch2旧帧不发布、Resume真实蓝区EOS、productionDrawVideo两像素、真实Opened/NewFrame/Ended、Ended回调Close0/finalClosed/ref0/registryRemoved/descriptorDispose、19旧unit及4工厂negative。不得skip或放宽。
音频、完整seek/rate、强取消、NativeAOT/publicfactory及整体替换仍未完成。完成修正后同步正式报告及六份现行入口并回读。
