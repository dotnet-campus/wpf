# Stage19-r5 Failed render contract closeout

状态：ThreeGatesValidated / CloseoutCompleted / OverallOpen。三门已执行，不再NotRun，不重复运行。权威收据 .coding-agent/stage19-r5-receipts.md、stage19-r5-com-publish-receipt.md、stage19-r5-sandbox1/2/3-receipt.md、stage19-r5-audio-verified.md、stage19-r5-seeking-verified.md、stage19-r5-purevideo-verified.md。

原MP4任务9fc034ffd1bb4664939e079bc0df4ccd：Open0→真实C00D36B4；公开HasAudio/HasVideo true、640x360、12:23.8266666，Opened0/Failed1，32x16 RTB nonzeroBytes0无状态异常。Green表示音频能力明确拒绝，不是有声视频播放。第二门06ef42c7b9824d059669677e7c1b2226：单FQN TRX total1/passed1、两Open epoch各[1,11]；监督child exit0/无timeout；source-linked生产逻辑+系统MF，不冒Native媒体ABI。本次前两门仅亲读verified收据，未冒称重新亲读全部原件。

第三门54a6037e852a44c59ab62bf030e03275：本次result.json1..659及UCE1..18完整亲读；stage completed/status Green/business exit0。Opened359.1399ms、Ended1967.0114ms、Close0/WindowDisposed；公开noaudio/4s/2x1/CanPause；原seek0.5/2.5实际0.8/2.8，32x16软件RTB红0000FFFF/蓝FF0000FF。cleanupDiagnostics空/Dispatcher及finalizers完成。HW命令hr均0不等于命令数0；HWpixelsNotCovered。独立比较器PendingIndependentComparator，非零差分认证。

actual唯一D9AA1E0ED5E99B29F10AC2DA4635D7F014B0F433C05DF2E5B18F2028836CD09C/4830208bytes=requested=frozen/noofficial/sourceLinkfalse；testSHA8EA50EC2F7FE20657DB9F90C6D89C61B9C6A41F2298925827ECD87AB35C4698F/MVID465f2658-cbfe-47e9-9c31-3bd834bf630a/.NET10.0.13 X64。顶层OSexit Unknown，Pcore业务exit不得冒独立OSexit。refs/resources计数Unknown，memoryGreen=null。

历史预算normal1/tests2/Native1/COM1/sandbox3，implicitDebug2。末轮87Passed；首轮86Passed/1Failed为新增terminal测试误用Close，后改Shutdown槽22；不冲销历史失败。normal exit0/3909warnings；Native与COM exit0，日志截断aggregate warnings Unknown。旧85/87不认证后续新hash全系统。

生产Failed NeedUIFrameUpdate接受预约，AcquireFrame合法无帧且保留Failure；创建失败前保存实际metadata。当前源码冻结，后续授权仅当前源码x86 Native与两arch默认full；旧89113/B07C不认证当前源码。全部旧fail/rootcause、Endpoint0、AVI16gap、AVsync、强cancel、HWtargetpixels/consumerloss、resourcesUnknown、memoryGreen=null继续保留，完整替换未完成。禁止原wpfgfx loader/副本/委托/fallback；不改skip/expected阈值，不增加微门或capture试验。
