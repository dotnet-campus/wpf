# Independent media cancellable reader — 实施记录

状态：AsyncReaderSlicePassed / 默认接入未完成 / OverallOpen。

## 已亲读原实测闭环
原任务 task580360ecf413421194fee728da85ceac 两正式 TRX case Passed，total2/passed2/failed0；child0，无timeout/cancel/kill/drain失败。完整原件读取范围、SHA/MVID/runtime、timeline与cleanup见 `.coding-agent/cancellable-reader-receipts.md`。目录漏枚举的 TestResults/child.trx 已按 stdout 精确路径全文读取，非交接冒认证。
正常固定AVI40帧、41 callbacks、EOS，最后timestamp39000000；sample retain40交reader release40、buffer release40、unlock40；正常callback自身release0是所有权转移，不是泄漏。尾Flush0/OnFlush+drain/reader0/callback0/MFShutdown0/COMuninit，MTA ThreadExited true。
取消：真实OnReadSample准入屏障，Close在屏障持有时未完成；释放后read cancelled，无发布。callback1/OnFlush1/sample AddRef1 Release1/Flush0/readerFinal0/callbackFinal0/MFShutdown1/COMuninit1/ThreadExited true。源端ReadSample是否仍pending Unknown；同步Open/URL创建仍不可取消，不能用deadline丢线程当关成功。
原normal2（首错TFM NETSDK1005、后成功3919warnings0errors）/COMpublish2（首oracle参数失败、后成功）/sandbox1/Native0独立保留；旧工具numeric exit/截断详情未恢复不补造。本轮新增额度normal3/tests2/COM2/sandbox2/Native1，截至本节实际全部0。

## Before / After、线程与所有权边界
Before/default：IndependentVideoPlaybackOwner专用MTA同步ReadSample，Close排队；Frames/clock/pending pixel[]/epoch由该owner管理。Async slice：CancellableVideoReaderOwner也有独立MTA，MediaFoundationReaderCallback原生借用sample AddRef后交WaitSample；reader负责copy/unlock/release。MF callback不应同步等待MTA。不能把两个owner直接互等嫁接；ReadAsync目前把receive放在owner锁内，须先关闭该外部代理锁问题并实际回归。当前Frames属性不是默认播放器接入证明。
单pending/cancelFence/epoch及Seek的Flush→OnFlush→Position→再读尚未接IndependentVideoPlaybackOwner；Pause保留有界在途帧、scrub单帧/false不发布、旧epoch丢弃release、close停止新读/drain、回调Close/最后Release不selfjoin、跨Close旧lease有效，都尚无异步默认实测。

## 状态转换与回撤
现slice：Open→Ready→Read→Closing拒绝新命令→Flush→OnFlush+callbackDrain→reader/callback release→MFShutdown→COMuninit→MTA exit。仅认证上节实测边界。
默认迁移必须先通过真实控制/lease/事件组合：40帧EOS、Pause>=515ms冻结、Seek25000000无旧epoch、红蓝Scrub及false不发布、COMOpened1/Prerolled11/NewFrame/Ended、callback Close/Release、latelease CopyPixels、refs/registry/descriptor/MTA exit。未通过保持同步默认，不删功能/skip/改阈值/额外Sleep追绿。回撤为保持原同步构造，不半成品默认开启。

## 下一直接执行
先修复async owner外部receive持锁并通过正式system桥回归；完成后再评估默认迁移，不再造reader。最终托管回归通过后才允许本轮一次当前x64默认NativeAOT compile；不冒NativeABI运行。随后当前源码双arch public/full验收，不泛选微门。
原版wpfgfxloader副本/委托/fallback永久禁止，合法外部provider保留。audioendpoint0/AVI16gap/AVsync/HWconsumerloss/resources/fullOpen及旧91不相减。official-window旧NotReached保持历史，非本次取消失败。
