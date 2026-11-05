# Stage16 当前生产 Native 有限 ABI 认证

状态：CloseoutCompleted（仅证据工作）/ LimitedCurrentNativeAbiProtection / OverallOpen。
本次收尾亲自只读核证与写文档，无终端脚本、再委派或新运行；源码/build/test/publish/沙箱均0。前轮实测已完成，不再以 Draft、ExecutionPending 或无证据的“工具不可用”作为阻塞。

## 已有实测终态
- 唯一任务 fe7e89c1be574a32bc1fb5ce4701d92f；原件 WpfGfxShape/artifacts/independent-media-stage16/readback/Results/stage16-native。精确三个既有无参 TestMethod 的 FullyQualifiedName 等值 OR，expected3；TRX total/executed/passed3、failed0；监督 child11924/exit0，filtered、内120/外300秒，无 timeout/cancel/drain 故障。沙箱工具原 return 没有独立数值 OS exit，不补造；child exit0不是工具 OS exit。
- 用例：IndependentNativeAbiTests.WhenStage14ProductionNativeIsLoadedThenFactoryGateAndEventAbiAreReal（历史方法名）、UceChannelTests.WhenFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure、UceChannelTests.WhenAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives。不是 full 或媒体正向测试。
- Native requested=actual：C:\CodingAgentSandbox\Tasks\fe7e89c1be574a32bc1fb5ce4701d92f\Native\wpfgfx_cor3.dll；SHA256 F48BEFF0CEB9EB27606EB446C4168CF8653574C55731DC61D366ACE1A37CAFD1；PE Amd64，module/BaseAddress 140720497033216。.NET10.0.13/X64/pointer8。
- 测试 DLL SHA256 9370629E7910C48CFC40629A9425D20AFB6522B49284308877874FAAC992C990，MVID fdb0ce08-9c8c-4f29-b1e2-07e50ab244b8；监督/业务/module身份一致，businessSourceLinkUsed=false。实际新 Native，不沿旧 F156/6BDF，不把包根文件存在解释成原版回载。
- 106正常 exports 存在；WpfGfxShape_RuntimeHeapDiagnostic_v1、WpfGfxShape_NativeAotAbiProbe_v1 两诊断缺席。真实 factory canOpen0/1 返回 E_NOTIMPL/null，opaque client 未触；null 参数校验保留。
- event proxy 两IID同identity，20字节packet与回调HRESULT80004005透传；callback.Release2，dispatch后AddRef2/Release1，最终proxy0/factory0/descriptorDisposals1。正常公开 channel 的 postfailure attach 与 partition isolation 合同通过；不是内部 player 的公开创建或媒体transport正向。

## 发布与预算最终账
Stage16实际 normal0/直接tests0/COMpublish1/隐式COMcompile1/sandbox1/production0/Nativepublish0。COM Release win-x64 net10.0-windows self-contained，NETCore/Desktop10.0.13，PublishAot=false，ManagedAcceptance/ComAcceptanceSandbox=true；显式 AotDllPath 指向已冻结 stage15-final-native-x64。正式 PackageSelfContainedComAcceptance target 生成 clean/aot.sha256，publish/clean分离，不手写身份。
原 publish return 执行成功，日志60行，已实读1–12/55–60 exit0；源stdout1686232字符、前1666232截断、仅末20000可见：aggregate warnings Unknown，有实际warning，不称零。完整原参数与原return边界见 .coding-agent/stage16-receipts.md；保留原record，不用新运行补日志。前轮预算 normal≤2/tests≤1/COM≤1/sandbox≤1/Native0/生产0封存，未用额度不补跑；本次文档接续所有运行计数0。

## 下一公开 factory 合同：只读结论，不在 Stage16 实施
依据工作区官方源 src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/MediaPlayerState.cs：
1. constructor 38–58：Init→CreateMedia→订阅NewFrame/MediaPrerolled；Init 66–76只初始化本地volume/balance/rate/scrub等。CreateMedia 772–798 创建event helper/proxy→FactoryMaker→CreateMediaPlayer(canOpen=true)，此时没有URL；不能凭未来可能有音轨统一拒绝整个对象。
2. Open 645–661：Verify→SetSource→SetPosition(0)；SetSource 922–940经OpenMedia→MILMedia.Open成功才缓存URI。Play/Pause通过SetRate控制；constructor/Open没有固定依次调用SetVolume/Balance/Rate/Scrub的链。各属性可独立、可在Open前调用，不能只实现一条脚本序列。
3. Volume 229–275在变值且未muted时SetVolume成功才写缓存；muted仅缓存。Balance 282–312亦成功后写缓存；ScrubbingEnabled 318–331变值才SetIsScrubbingEnabled；SetSpeedRatio/PrivateSpeedRatio 895–922提交SetRate，暂停/clock条件影响提交。Volume/Balance/SpeedRatio/Scrub getters是本地期望状态；HasAudio/HasVideo、尺寸、duration/position、CanPause、buffer/download等getter调用Native，须真实初始化输出、表达状态，未知不得伪装已知能力。
4. 官方源 src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/WmpPlayer.cpp 357–435：SetVolume/Balance校验→SetTargetVolume/Balance→state engine AddItem；成功是目标状态被接受/排队，不承诺同步可听输出。官方API Volume默认0.5范围0..1、Balance语义为左右声道，不能用恒S_OK空方法冒充。

### 状态与能力边界
- 可行的下一中间切片是：真正有效且独立identity的公开对象，Created/Opening/OpenedVideo/Failed/Closed/Shutdown状态明确；factory创建不依赖未来URL。当前Stage16仍E_NOTIMPL/null，只授权下一实现方向，不声称已经启用。
- sourceHasAudio必须来自源轨道枚举，Unknown与false分开。真正无音轨的纯视频，在无音频endpoint时没有需要输出的声音；可独立时钟播放至EOS、seek/pause/rate/scrub、帧通知和VideoDrawing/RTB，不应只因endpoint0拒绝。此为契约可行性结论，尚无公开WPF端到端实测证明。
- 无source或已证sourceHasAudio=false：SetVolume/Balance可在校验/生命周期检查后，实际保存有序、线程安全的目标状态，成功返回S_OK；getter/重开应用/Close重置语义须一致。无声源没有当前音频副作用不是空实现，但保存必须真发生；不能把“音轨存在但未支持”改报false。现有官方异步目标语义支持此设计，不代表当前替换实现已达标。
- 含音轨而audio sink/endpoint/decoder未支持：禁止默认丢音后Opened/Ended；Open异步接受可返回S_OK仅表示请求已受理，探测后必须Failed携带具体MF错误或明确能力不支持HRESULT，保留错误因果、停止并清理owner；不得发Opened成功。同步拒绝应返回精确失败，事件与异常路径按同一状态机去重，不能任意把所有错误归为“无endpoint”。失败事件通知是真实失败，不是完整播放通过。
- volume/balance已接受目标不等于未来音轨保证成功；音轨出现或切换时必须应用真实audio或明确失败。强cancel尚未支持须诚实失败/标Open，不靠S_OK或超时掩盖；能力未满足的其它stub亦不得S_OK无行为。

## 下一代码步骤与有界授权清单（下阶段，不本次运行）
1. 实现公开Create→IndependentMediaIdentity创建与注册、event proxy持有/回滚→SystemMediaSource.Resolve桥；保留合法external-provider路径、参数校验、COM IID/引用/失败释放；不得秘密export或source-link替代Native。
2. 实现上述目标状态、视频能力探测/真实getters、Open失败packet及Close/Shutdown/lastRelease/退订/cleanup失败合同；新Open隔离旧epoch与owner。先用公开API定向验证前置属性、无source/无音轨、含音轨明确失败、失败后重开与重入释放，不能无限停在factory拒绝保护门。
3. 通过后以新生产Native建立独立身份，实际WPF MediaPlayer→VideoDrawing→RenderTargetBitmap，验证无音轨真实源多帧像素/时序/EOS、controls/scrub、Failed与关闭清理；记录模块路径/SHA/BaseAddress，绝无原版wpfgfx模块/loader/副本/委托/fallback。未通过不得升级为公开媒体正向基线。
4. 建议下一阶段上限：normal build2、targeted pure test2、生产Nativepublish1、COMpublish1、sandbox1（内120/外300秒，预注册正负用例）；每次publish的隐式compile另记。失败即收据收尾，不重跑追绿、不扩大full；此为后续实现预算清单，本次运行授权仍0。
5. 然后继续真实audio endpoint/volume/balance/AVsync、强cancel与生命周期、MP4/用户素材、双架构和完整full矩阵，逐项授权验证。纯视频仅中间可验收切片，完整替换目标绝不降为video-only。

## 官方依据与限制
- https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.mediaplayer.volume?view=windowsdesktop-10.0
- https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.mediaplayer.hasaudio?view=windowsdesktop-10.0
- https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.mediaplayer.mediafailed?view=windowsdesktop-10.0
本次已读取上述官方API正文及上述工作区官方实现；存储/异步判断以真实实现为依据，不推定官方承诺任意codec或无endpoint音轨可播。

## 文档闭环与总体剩余
已同步并逐份回读七入口：next-session-handoff、current-work-item、current-stable-baseline、replacement-issues-and-plan-status、final-acceptance-remaining-matrix、wpfgfx-complete-replacement-validation-plan、current-execution-rules；本报告与receipt亦回读。旧阶段内容保留为历史，不作为当前阻塞/余额。
既有前代理已全文读TRX38/监督40/ABI业务153/exports650/post267/isolation402；本次只复核核心身份、监督与TRX计数/业务尾，不声称重新全文读取。证据上限始终 LimitedCurrentNativeAbiProtection；audio/full Open、旧full红、endpoint0、AVI gap16、resourceUnknown、MemoryDescriptiveOnly/memoryGreen=null、底层native/lease refs Unknown全部保留。Stage16证据收尾完成，不是用户要求的完整替换已经完成。
