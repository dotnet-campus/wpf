# Stage19-r4verified 正式收尾

状态：ExecutedFailed / CloseoutCompleted / OverallOpen。第一门 Red，第二门 NotRun/BlockedByFirstError。此结论覆盖所有旧工作稿当前状态；不提升媒体能力基线。

## 证据与亲读范围
既有Windows任务53bfc798058f4ee18c3ad6d62723ce18完整回收目录WpfGfxShape/artifacts/stage19-r4verified-audio-readback。实际结果路径Results/Results/candidate/result.json共525行，本次亲读1–525（511–514长异常另行精准补读）；uce-diagnostic.log全部3行已读。不仅引用前代理读证。目录内Results仅该result与UCE，无RTB帧、TRX或监督结果。执行参数来自既有即时收据：sourceDirectory=WpfGfxShape/artifacts/stage19-r4verified-clean；executableRelativePath=WpfGfxShape.ComAcceptance.exe；arguments=[--pcore,candidate,media-unsupported-audio-public]；timeoutSeconds=300。本次未重发任务；原工具return全文未在现存收据保存，不伪造return或OSexit；task由回收result中的实际路径确认。

## 第一强门：未通过
公开Open-return 0x00000000→真实Failed 0xC00D36B4，126.0896ms；opened0/ended0/failed1，未记录Opened/Prerolled。sourceProbe真实MF：hr0/HasAudio=true/AudioMetadataResult0，AAC流与H264流，640x360，duration100ns7438266666。固定输入Video.mp4长度47255617，SHA928410B89545B2DF0AA7C8A46756D512D2BE4879209282F90D92D578A30CA40A。
但failureContract快照HasAudio=false/HasVideo=false/width0/height0/DurationAutomatic/renderingEvents6；不能把独立sourceProbe结果替代公开player getter强门，也不能假定快照默认值证明getter真返回false。失败栈不再是r3的get_HasAudio：本次CopyMedia→BitmapVisualManager.Render→VideoDrawing/AddRefOnChannel→MediaPlayer.UpdateResourceInternal→MediaPlayerState.NeedUIFrameUpdate抛0x8007139F。精确异常在result513，failureStage=real-MediaFailed。实际status=Red/business exitCode=1，非Green/非预期安全拒绝闭合。
RTB输出未落盘，不能声称已验证全零帧或无静默视频输出。UCE仅Closed lifecycle-absence S_OK/sourceFalse、HW acquire S_OK/sourceFalse及lease-released S_OK/sourceFalse三条；无视频PTS，不等于Prerolled或有声播放。

## Identity（运行实际，不拿原输入清单冒actual）
actualNativeModules仅Native/wpfgfx_cor3.dll：E2B5F6608ECCAAE5D47CB2FB3E6FB714F7602500391315DF3AF651B3524B470A，4829696bytes；requested/frozen/root条目一致，无official模块，businessSourceLinkUsed=false。
Tests SHA035C683F7F045D64C8D7FB13C42C4243B71CC1FBD4CFB5AC1E6ECFD7D61C60F7，2016768bytes；MVID6b657a13-5212-4c69-af27-17fce1bf0545。运行.NET10.0.13/X64/Windows10.0.26200。x86仍旧89113C907F3CAC2F51A2ADD859A85E9836DD4C555E201060BC13A4FF56CDFB6E，未认证本次修复。

## Cleanup、退出与第二门
Close-return S_OK，Window-close IsDisposed=true，failureCleanup=Dispatcher shutdown and finalizers completed；cleanupDiagnostics为空。COMrefs/资源余额Unknown，不把空数组当归零。没有监督层exit记录，数值OSexit Unknown；business1不冒OSexit。
查WpfGfxShape/artifacts无stage19-r4verified-seeking-readback或其它r4verified seeking任务目录，当前收据无已执行Seeking；按第一强门失败立即停止，不运行唯一余额sandbox1。Seeking方法已确认公开无参，不再API发现，不改FQN。TRX/MTP matched/passed/failed/skipped N/A（未执行而非零匹配），真实MTA queue握手、Rate0/1、两Open各epoch1→11 exact1、DrawVideo像素、COMrefs和cleanup本轮均NotRun。

## 预算分账
本继续执行授权已用normal1/run_tests2/Nativepublish1/COMpublish1/sandbox1；另implicitDebugbuild2及两publish隐式编译各1。normal限2余1、tests限2余0、Native限1余0、COM限1余0、sandbox限2余1；首错冻结余量，不重发。当前收尾阶段源码/build/test/publish/Native/新增sandbox均0。
回归两轮各85Passed/0Failed/0Skipped，45ms与41ms，均Debug非RID/exit0；是同85两轮，不是170独立。normal Release net10.0无RID exit0/2405warnings/0errors；stdout截断细节及原发布参数见.coding-agent/stage19-r4-receipts.md。Native及COM publish各exit0，aggregate warnings Unknown，不能冒全量stdout。以上build/test/publish日志为前代理收据，本次未重新执行或宣称亲读原工具日志。历史49Passed、Release RID host runtime缺失counts N/A及r3红分账保留。

## 下一直接计划与永久边界
本轮完整收尾，不借余量修复或改预期。下一先定位Failed后NeedUIFrameUpdate请求状态的官方合同及最小生产根因，并核公开metadata来源/赋值时序；不能仅凭失败栈宣告根因已确定，不能吞AcquireFrame错误或把所有Failed方法改S_OK。新授权验证两强门闭合后，直接相同最终源码x86独立生产Native及x64/x86两arch默认full：默认Shared/Release/AOT、独立publish与clean、新AotDllPath/GetFileHash、新actual唯一模块/noofficial/sourceLinkfalse、新testSHA/MVID/runtime与完整TRX真实分母、监督/业务/OSexit分层、DrawVideo/refs/cleanup全回收。不得用旧x86 Green认证新源码，不泛选门。
audioendpoint0/AVI gap16/AVsync/强cancel/HWtargetcontent及loss/refs与resourcesUnknown/fullOpen/旧full红/memoryGreen=null保留。能力安全拒绝不等有声视频，尚未完整替换。原wpfgfx生产loader/副本/委托/fallback永久禁止，合法外部provider边界不变。
