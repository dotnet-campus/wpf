# Stage19-r4 Failed metadata 收尾

## r4verified 正式收尾：ExecutedFailed / CloseoutCompleted / OverallOpen
权威Docs/completed-work/stage19-r4closeout.md。task53bfc798058f4ee18c3ad6d62723ce18 result525行及UCE3行本次全部亲读。第一门Red/business1：Open S_OK→Failed C00D36B4→RTB NeedUIFrameUpdate 8007139F；sourceProbe真实AAC/H264/640x360，不等于failureContract快照false/false/0/Automatic的公开metadata验证。RTB无帧输出，不能称Green。Seeking首错NotRun/TRX counts N/A。actual唯一新E2B5F6608ECCAAE5D47CB2FB3E6FB714F7602500391315DF3AF651B3524B470A/noofficial/sourceLinkfalse；testSHA035C683F7F045D64C8D7FB13C42C4243B71CC1FBD4CFB5AC1E6ECFD7D61C60F7/MVID6b657a13-5212-4c69-af27-17fce1bf0545/.NET10.0.13 X64。Close0/WindowDisposed/finalizers完成，refs/resources/OSexit Unknown。已用normal1/tests2/Native1/COM1/sandbox1，implicitDebug2；余1/0/0/0/1首错冻结，本收尾新增执行及源码修改0。两轮85不是170独立，历史49/host N/A保留。下一最小修复NeedUIFrameUpdate真实状态合同并新授权两门，再同最终源码x86 Native与两arch默认full；所有旧红及音频/HW/资源未证门不冲销。以下所有旧当前及预算仅历史。

## 继续执行正式工作稿（当前，覆盖下文历史状态）
当前主要pure已恢复：绝对Tests.csproj默认Debug/net10.0非RID，tests1 85/0/0 45ms，tests2准确原82filter加FailedMetadata为85/0/0 41ms，均exit0；非170独立case。normal Release/net10.0无RID exit0/2405warn/0err；stdout2032385前2012385截断，62行可见日志全文读。run_tests schema仅targetPath/filter；上一显式Release RID DLL导致testhost从RID目录找x64 runtime失败，不是生产红。当前默认宿主真实恢复，无机器环境/SDK/TFM/依赖变更；DOTNET_ROOT变量值工具未披露，不伪报。
本次normal1/tests2/Native0/COM0/sandbox0，implicitDebug2；新预算2/2/1/1/2。即将一次stage19-r4verified-native默认Shared生产AOT发布，独立COM publish/clean/runtime10.0.13，先MP4拒绝再Seeking。无新生产代码修改，历史49/hostN/A/87/101与CADD全分账。后续实际结果以本节及即时收据增补，不提前宣告两门通过。

状态：ProductionFixImplemented / PartialPurePassed / RegressionHostBlocked / TwoSystemGatesNotRun / OverallOpen。不是两门闭合，不是完整媒体完成。亲自执行，无终端、脚本、再委派。

## Contract root 与 owner
本轮亲读本地官方源码：WmpPlayer.cpp HasAudio/HasVideo(约760–825)只检查指针和shutdown assertion，直接读SharedState并返回S_OK；GetMediaLength470–485直接缓存/S_OK。SharedState.cpp1–175初始化false/0，getter锁内读值。WmpPlayer.Open135–185新URL清长度/尺寸；wmpstateengine.cpp3810–3860新URL清音视频属性。失败转移865–910只终止arc并RaiseEvent(AVMediaFailed,failureHr)；WmpEventHandler.cpp870–910同样报告原错误，没有清metadata。因此Failed不是getter非法状态，最后knownmetadata仍可查询；没有数据才是缓存默认false/0，不是把所有Failed methods返回S_OK。
MediaPlayerState.cs156–210直接调用native getter并HRESULT.Check，没有HasAudio/HasVideo托管缓存；380–399长度0映射Duration.Automatic；940–975 VerifyAPI仅验证dispatcher和native句柄。其职责是传播native错误，不应修改托管层隐藏8007139F。
生产owner OpenAsync55–90实际读取Width/Height/Duration/HasAudio；Identity.ObserveOpenAsync125–205在此后RequireVideoOnly发现HasAudio=true并报C00D36B4，且尚未Preroll。错误根因是AudioPresent把metadata查询错误依赖HasAvailableVideo播放状态白名单，Failed即8007139F；Video/Dimension/Length兄弟同样误隐藏known metadata。

## 最小修改 / before-after / 回撤
IndependentMediaIdentity.cs仅对Failed metadata查询返回owner最后known音轨/视频尺寸/时长，无值默认false/0；HasAvailableVideo播放状态、Failure、AcquireFrame真实失败、Close/terminal RO_E_CLOSED保持不变。非Failed未知时长原精确错误保留。无loader/ABI/SDK/TFM/包/Host/InternalsVisibleTo新增。
UnsupportedAudioPublic仍查询HasAudio/HasVideo，新增尺寸/时长记录并明确要求HasAudio=true/HasVideo=true/640x360。原HasVideo=false把视频轨存在混同输出帧，与权威getter职责冲突；无Opened/无Ended/精确一次C00D36B4/全零RTB保持独立无输出证明，不能将无音轨静音放行。固定原MP4及MF独立sourceProbe不变。此系统期望尚未实跑，不能宣称通过。
新增IndependentMediaFailedMetadataTests：四组真实函数指针查询覆盖known audio+video、audio only、video only、无metadata/error，验证5 getters的值/HR及原Failure不变；使用既有反射fixture惯例设置state/metadata，没有mock/newHost/test-visible生产API。既有Duration测试删除与官方Failed职责冲突的状态行，未删除该行为替代检查。回撤仅本轮metadata分支/场景修正/新测试，不覆盖既有代码和失败证据。

## 实际执行原件、计数与预算
1. normal build1：target绝对d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj；Release/win-x64/net10.0。工具成功，exit0，61行日志已完整读取；3907warnings/0errors/21.30s。stdout3262311字符，前3242311截断，未冒充全量stdout可见。
2. tests1：同绝对csproj，filter=FullyQualifiedName~IndependentMediaFailedMetadataTests|FullyQualifiedName~IndependentMediaIdentityTests|FullyQualifiedName~IndependentMediaDurationTests|FullyQualifiedName~IndependentMediaFrameLifecycleTests|FullyQualifiedName~IndependentMediaTargetFailureTests。exit0，49Passed/0Failed/0Skipped/49total，36ms。run_tests隐式Debug build1单列，实际Debug/net10.0输出；日志62行，stdout1631673/前1611673截断。TargetFailureTests是文件名而非实际类，该项不匹配，不能声称其4case跑过。
3. tests2：绝对Release/net10.0/win-x64/WpfGfxShape.Tests.dll，原82相关filter加FailedMetadataTests（完整参数保留.coding-agent/stage19-r4-receipts.md）。工具失败exit1，38行日志全文读取，无截断。testhost.exe未找到x64 Microsoft.NETCore.App10.0.0，.NET location指向RID输出目录，No frameworks were found；只列出x86 runtime10.0.14等。测试运行中止，1个文件匹配不是case匹配；case matched/passed/failed/skipped均N/A。不是业务断言失败，但按首失败停止，不耗额外预算追跑。旧82/101历史保留，不冒充本轮通过。
本轮实际normal1/tests2/Nativepublish0/COMpublish0/sandbox0；另implicitDebug1。余normal1/tests0/Native1/COM1/sandbox2，因首失败冻结，不擅自扩预算。没有新task，没有系统OSexit/业务码/TRX/MTP。

## 两门与 identity
MP4系统门NotRun；Seeking系统门NotRun/BlockedByRegressionHost。既有r3 task4e470408896140cc9e0e08b32d5a0002仍Red，不能修改为Passed。本轮没有新Native SHA。旧CADD0C4F857BA4546530D21E93A9843F03814DC259818430E8798020AB6DF289不包含本轮production修复，不能复用认证最终源码；x86旧89113仍未同步。没有新actual模块/测试MVID认证。固定输入47255617bytes/SHA928410B89545B2DF0AA7C8A46756D512D2BE4879209282F90D92D578A30CA40A不变。

## 剩余直接动作（不能跳到双arch full）
先在新测试授权下使用已经成功的非RID Debug DLL路径或正确runtime配置验证准确原82+新增Failed filter，切勿再猜RID runtime路径。补无owner Failed及Close全部getter组合后再明确生产发布：独立r4native默认Shared/Release/AOT10.0.13/diagnosticfalse/ManagedAcceptancefalse；COM Release win-x64/net10.0-windows PublishAotfalse/ManagedAcceptancetrue/selfcontained/CoreDesktop10.0.13/Sandboxtrue/显式新AotDllPath/publishclean分离/GetFileHash。系统第一门原MP4公开拒绝完整读证后，第二门准确无参FQN WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenStage19RealMfRunsThenSeekingRatesAndOpenEpochOrderArePreservedAsync，监督120/外300，两Open每epoch1→11exact1、Rate0/1、ownerqueue握手、DrawVideo、cleanup完整回读。仅两门闭合才直接当前最终源码双arch defaultfull。
音频output不可用与能力拒绝安全门严格分开；audioendpoint0、AVI16gap、AVsync、强cancel、HWtargetloss、refs/resourcesUnknown、fullOpen及旧full红/memoryGreen=null继续保留，未证明完整替换。
