# r5/r6 接续：立即重开生产修复的真实新 Native 验证

> 后续独立审查与当前恢复入口见 [r7报告](candidate-purevideo-independent-r7.md)：已亲核本报告两轮身份、父子退出、原始AVI/四帧各两轮及重开记录；历史85+2尚未找到可认证TRX。本轮新83+2测试不追认为历史执行。有限视频门保留通过；通知admission、最终故障观察/异常释放链与candidate-only resize入口仍Open，不作整体完成声明。

## 结论与范围

r6 新 Native 的 x64 公开视频回归已真实运行；最终同一身份包两次独立通过，包含原 seek/EOS、一次 Play 且中途不 Pause/seek 的红蓝连续播放、第二 EOS、同一公开 MediaPlayer 三轮 Close/Open（两调用之间无 I/O/等待）。三负控均通过，父监督及视频子进程真实 exit 均已回读。不是 full 闭合，也不是任意线程交错/故障注入的独立审查闭合。

本接续没有再修改生产源码，没有生产 Native 重发布；使用前子任务已经独立发布的 artifacts/reopen-r6-native/native，且实际认证了新 SHA，绝非把旧 Native 绿色移绑。本轮修改仅公开行为验收循环、系统测试保存旧 owner 的方式、只读身份工具的 Native 路径配置和本报告/恢复入口。没有改变默认 async、oracle、expected、颜色阈值、时钟上限、DPI、flags；无生产测试钩子或可见性修改，无原版加载/复制/fallback，无终端脚本。

## 历史纠正

r4 报告保留，不能代表 r5/r6：r4 不含本轮新 Native。亲读 r5-final/readback/Evidence：监督 PID7064 exit1，视频4480 exit1，均无超时且 reaped/drained，不能把 r5-final 称作通过。其完整场景根因本接续未重做诊断。

r6-immediate/readback/Evidence 原件仍保留：监督12908 exit1，视频3792 exit1，无超时且 reaped/drained；candidate-video.stderr.txt 和 result 含 ObjectDisposedException，HRESULT 0x80000013。此前 Close/Open 中间存在检查点磁盘 I/O 的绿色不认证立即重开，也不追认为生产修复证据。

## 生产源码亲核

IndependentMediaIdentity.cs 当前 SHA256：0BE8845F380AAB9336B0FDD777EF828BF617873F9F88DB422F4A9ECE82A257B7。

* BeginClose 在 identity 锁内将 _owner 分离、清除当前 intent/proxy 并同步进入 Closed；BeginOpen 只禁止 terminal，创建全新 owner。公开语义依据是 MediaPlayer.Close 后允许同一播放器重新 Open，而不是要求调用者等待内部异步 MF 释放。
* Close 的 _cleanup 通过 Task.WhenAll(_operation, _exitEvents, _cleanup) 累积之前任务；CompleteCloseAsync 等待本 owner CloseAsync、历史 pending 和本 epoch 事件完成。重复 Close 由 _stopRequested 幂等，下一次 Open 重置它。最后 Release 从 registry 删除，terminal Close 后 FreeIdentityAsync 在 finally 释放原生 identity 内存。旧清理不回写新 State。
* owner.FrameReady/PlaybackEnded/PlaybackFailed 均闭包捕获创建时 owner；状态/control/open 完成路径用 ReferenceEquals(_owner, owner) 排除旧 owner。不能用 State 相同代替身份比较。
* owner.CloseAsync 返回同一个 _completion；owner 线程 finally 释放 reader/frame/队列/事件，_closed 的异常进入 completion；WaitForExitAsync 在 finally Join。identity CompleteCloseAsync 没有吞清理异常。Open 失败若伴随清理失败会抛 AggregateException。
* MediaEventProxy.Dispose 取消尚未 admission 的工作，已经 admission 的 callback 保留独立 proxy 引用并可结束；不能在 identity 锁下等待 callback/Join，否则回调 Close/Release 可自锁。

### 审查仍未闭合的精确边界

本轮不将通过包冒充完整并发证明：Notify 及 OnFrame 在 identity 锁外做 RaiseEvent/Source.NotifyFrame，Close 分离与锁外 Dispose 存在间隙；已经 admission 的旧回调契约允许结束，但跨 Open 的观察边界尚无受控交错验证。ObserveOpenAsync 的 Opened/metadata 直接使用捕获的 epoch proxy，不重读新 proxy，仍需验证 admission 与重开的交错。Source.NotifyFrame 的订阅者通知是失效提示，不携带旧帧，但是否可能造成跨 epoch 多余通知未作确定性实验。_cleanup 确实保留故障 Task，不过最后 Release 的 _releaseCompletion 故障没有本轮动态错误回读证明；await Task.WhenAll 也不能仅凭单一抛出异常声称枚举了所有 inner failures。多个 pending close 故障、最终释放故障、in-flight callback 竞态仍需按既有框架补确定性行为测试与错误观察，不得靠 sleep/retry/测试开关修饰生产。

## 本轮回归改动与测试

PureVideoPublic（CandidatePureVideo 和正式 ComAcceptance 共用源码）保留原像素/时钟断言及连续播放路径，追加三轮相邻 player.Close(); player.Open(reopenUri);，之后才写事件证据。每轮要求精确 opened 计数及 ended==2，真实视频元数据不变。这是公开 API 回归，不是生产条件。

IndependentMediaLifecycleSystemTests 两处原测试在 Close 后使用 identity.Owner 已不符合“分离旧 owner”语义，改为关闭前保存 owner 后检查 ThreadExited，没有删释放断言。只读身份辅助增加 CandidateNativeDirectory，默认路径未改。

本轮 run_tests 共5次均 exit0：IdentityTests；生命周期/controls/重新打开筛选9项；相关 IndependentMedia/owner/COM桥85项；两次系统测试筛选（最后一次精确两项均通过）。首次系统筛选的 WhenFailedCallback 名称不匹配实际方法，因此后来用 WhenMissingOpenFailsThenCallbackCanCloseAndRelease 重跑，两项确实执行。运行总时间短，不将这些筛选当作全部 MF 系统集或独立 Windows 故障注入证据。独立 run_build 0，full 0。

## 冻结身份

证据根：artifacts/candidate-purevideo-r6-final。

* Native SHA256 2E5DC1BCC9CC7DEF60B0EE05C622954DC9BC7C2C3A57888FEE5767ED5705AFEE，4853248 字节，Amd64，无 CLR header。
* 最终 harness SHA256 057AFD4201CC4075497FBEF8252B063316C79070D12668471B6DA5BCF0932B29，MVID ff4cf6a1-b019-4f42-bce4-55a0a336ab92。
* identity-readback/Evidence/source.sha256 冻结505项；manifest SHA256 259453E8F9630018E6E031D1ECFB41EB1CF8ADCDA35198964BA5585AE55A0563。identity 工具未加载 Native。该清单是当前源码快照，不捏造前一子任务 publish 时的构建 attestation；本轮生产源码未再改。
* 两轮 supervisor 都检查包内 aot.sha256、唯一 Native、子进程实际加载 Native 和 harness 身份。原始 result/AVI/四份 Pbgra32 在 candidate-raw 下。

## 最终同包独立执行

|轮次|sandbox task|父监督 PID/exit|视频 PID/exit|三负控 PID（exit均1）|
|---|---|---|---|---|
|readback|f23ac84ca5004366ad6fa367a198accb|10812/0|12852/0|11660,9868,4624|
|recheck|8f12dd53bcfb4191b574690dd9a6ea34|1160/0|5664/0|3192,5132,9752|

两轮 timeout=false、reaped=true、drained=true、视频 killError=null，所有负控 passed=true/failures=[]。父真实 exit 来自 supervisor-exit-probe.json；子来自 candidate-video-process.json。最外层启动器未单独返回数值 exit，不捏造。

两轮 stage=purevideo-completed；连续播放采样0.8/2.8、中心0000FFFF/FF0000FF，paused=false；最终 reopenedState cycle2/opened4/ended2、HasVideo=true、HasAudio=false、CanPause=true、duration4秒、2x1。原 seek 断言仍执行。清理证据由完整 result 保留，非 producer 替代 consumer。

本轮另有 candidate-purevideo-r6-verified 两次先行通过（单轮立即重开版本）：父12704/0、11844/0；视频8916/0、11276/0。与最终循环版 harness 不同，不能混称同一最终身份。

## 实际执行账与恢复

本接续专用 publish5次成功：先行candidate/supervisor2，身份1，最终candidate/supervisor2；sandbox5次：先行2、只读身份1、最终2。生产 Native publish0，run_tests5次成功，独立build0，original/compare/full0。旧子任务次数未复算，不并入本轮。

恢复从本报告和 artifacts/candidate-purevideo-r6-final 开始，不回到 r4 的旧 Native。不应重新跑绿来替代尚缺的确定性审查；先补上文审查边界的公开行为/已有 ABI 框架回归，若确认真实缺陷则最小修生产并重新独立发布、冻结新身份、再运行完整同场景两轮。尚未覆盖 full、x86、resize、故障注入下多 pending close 异常聚合及 callback 精确交错；OverallOpen。
