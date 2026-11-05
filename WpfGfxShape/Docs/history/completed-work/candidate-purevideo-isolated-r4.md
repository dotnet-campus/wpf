# Candidate purevideo r3失败交接与r4时序修复

## 状态

CandidatePureVideoPassedTwice / NegativeControlsPassed / SupervisorExitVerified / OverallOpen。不是full通过。

本轮修改仅验收：Pcore/PureVideoPublic.cs的暂停/seek/Rendering顺序和单调时序诊断；CandidateVideoSupervisor/Program.cs及SupervisorProbe.cs新增verify-video单轮父监督收据。生产gfx修改0、生产Native发布0。defaultasync、oracle、expected、阈值、DPI、flags、asset和producer均未改。不加载/复制原版，不使用fallback，不执行终端/脚本。

## r3原件、源码与二进制界限

亲读 artifacts/candidate-purevideo-r3/video-readback/Evidence，task d06edf1afc794ce59f3a8583f1cba86a。视频PID6812真实exit1，timeout=false/reaped=true/drained=true。首请求0.5，seekMs330.1238，clock-window在1876.0006ms已为2.1秒；post-Rendering2406.5865ms为2.6秒；actual2.6，中心FF0000FF。原content/clock/opacity/stale-frame断言失败，cleanupDiagnostics为空。不是生产缺陷定论。

该包harness SHA1B2440C7BC8B036D1D4AE013D2C1E992C0E3C9A20D647BEDDDC63EF0B892C58A，MVID3da04371-0d85-4c99-a5d9-6f13b19ebccd。当前源含逐帧File.WriteAllBytes，但r3回读目录没有candidate-raw，不能用后来源码认定r3包包含最后原始帧复制改动。r3父监督独立exit收据缺失，无法补造；仅确认子真实exit1，r3历史原件保持。r4重新publish并返回candidate-raw以消除此歧义。

## 定位证据

1. r4-diagnostic，仅增加Rendering及seek/Play/checkpoint QPC、Position、elapsedMs。task14a0928a87554b4dbbd93351a7fba082，视频PID6772 exit1。seek-return346.0192ms，Play-return352.6181ms/Position0.5；checkpoint-return360.2314ms/0.6，排除检查点独占一秒的猜测。首次后续Rendering1587.7879ms/1.8，clock-window1589.4668ms/1.8，再等Rendering2123.5973ms/2.3，最终2.3蓝色。墙钟与媒体推进一致，UI检测在首次提交长空档之后才执行。
2. r4中间尝试：暂停态额外RTB预热并提前Pause，视频PID4972 exit1。仍有首次Rendering约1.14秒长空档，预热不足，未当成功；原件保留 artifacts/candidate-purevideo-r4/readback/Evidence。
3. r4-synchronized：暂停态seek后先等Rendering并Render派发，再Play；到target+0.3立即Pause，再等post-clock Rendering和RTB。首seek410.9239ms，到1590.2542ms Rendering，Position一直0.5；Play1590.9493ms/0.5，clock1813.1411ms/0.8，Pause1813.2949ms/0.8，后Rendering2125.7045ms仍0.8。视频PID3924真实exit0。证据把首次长提交移到未播放时，未替换帧。
4. 最终删除无效额外RTB预热及重复Pause；每target仍只有一次真实RTB采样。两次独立最终包执行均通过，见下。

根因闭合范围：该视频验收把首次暂停seek提交与播放混在一起，并在到达目标后继续播放等待下一Rendering，使受墙钟驱动的正确媒体内容越过原颜色/时钟窗口。修复是验收控制同步，不是放宽上限、重试到绿或生产冻结伪帧。未证明首次提交空档具体由JIT还是渲染内部哪个工作项造成；这不是本次采样控制修复所需的生产性能结论。

## 生产语义独立审查

IndependentMediaIdentity.BeginSeek(355-379)将公开seek交给owner.SeekAsync；暂停且ScrubbingEnabled时scrub=true。IndependentVideoPlaybackOwner.SeekAsync与Pump(146-237)先flush，CompleteSeek后设LastTimestamp为target，scrub提交真实解码帧；MediaFoundationSample.CompleteSeek/SetPosition(576-607)最终调用IMFSourceReader vtable[8]设置时间。SetPlayingAsync(138-144)只切换播放和重置anchor，Tick(332-362)通过Stopwatch/PTS等待真实帧并更新LastTimestamp；公开Position(IndependentMediaIdentity563-578)返回该时间戳。没有验收状态、条件、开关注入生产。现有实测符合这条链，不支持修改生产seek或clock。

同类检索：preregistered media clock/post-clock composition只在PureVideoPublic出现；普通COMAcceptance与CandidatePureVideo直接链接同一个场景，修复覆盖两入口，后者已动态验证，前者本轮未运行。MediaHelpers.WaitMedia的Background/25ms/15秒限制未改。

## 最终包与真实执行账

本轮专用publish8次均成功（四个candidate/supervisor配对）；sandbox5次（诊断失败、中间失败、同步通过、最终通过、最终独立复验）。独立run_build/run_tests0，Native发布0，original/compare/full0。失败与成功目录互不覆盖，非同一进程内retry。

最终 artifacts/candidate-purevideo-r4-final/{readback,recheck}/Evidence：

|执行|task|监督PID/真实exit|视频PID/真实exit|三负控PID（均真实exit1、Passed）|
|---|---|---|---|---|
|final|15ae99a9417247e6861808dddf27b0b7|2936/0|11204/0|11324,148,9868|
|recheck|d6b25a1842014111865116bcdd3a24bd|2948/0|1252/0|9572,9148,4960|

两轮父/子均timeout=false,reaped=true,drained=true，视频killError=null。父真实exit见supervisor-exit-probe.json，子见candidate-video-process.json；负控见process-results.json。最外层启动器数值exit未由工具单独返回，不捏造。

两轮case Stage17-public-MediaPlayer-purevideo-v2均purevideo-completed；首actual0.8/sample0000FFFF，第二actual2.8/sampleFF0000FF，原不重复帧、alpha255、红/蓝dominance、target+1上限均保持通过。Opened/无音轨/HasVideo/CanPause/4秒duration、Ended及Close/解绑/Rendering退订/窗口释放全部经原路径，cleanupDiagnostics=[]。candidate-raw中有原AVI、两份Pbgra32和result.json，consumer为公开MediaPlayer/VideoDrawing/RenderTargetBitmap，不是producer输出冒充。

最终包Native SHA FBD35CDD609DE2D67EB494B41A07FDB9240DC955B3F54D681D48F2423C8A4959（既有生产DLL，未重发布）；harness SHA4B789758EC7740ECD435B32CC025EF7C9B7203AD3C295515B1CF1B39368A24A6，MVID822a77c2-1d28-4e09-aa33-9ed1f63d8dec。包内唯一gfx是Package/Native/wpfgfx_cor3.dll，package-identity与运行actualNative及负控身份由监督校验。r3不同harness身份不移绑到r4。

## 尚未覆盖

full替换、正式COMAcceptance完整包、多媒体其他资产/音轨/错误恢复/长期并发、超时kill失败故障注入未运行。此次单视频通过不能关闭总体替换；本报告只闭合指定真实视频失败及最终父子退出/cleanup证据。r3缺失父退出/帧原件不追认。首次渲染长空档的生产性能归因仍未做。
