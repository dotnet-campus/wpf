# Stage17 公开独立媒体工厂：正式最终 closeout

## 最高终态：PublicVideoOnlyNativeValidated / CloseoutCompleted / OverallOpen
本段替代下方历史失败的“当前”表述；仅公开纯视频功能闭环，非硬件窗口视频通过、非完整替换。正式接续 [correction](stage17-correction.md)，最终[执行收据](../../.coding-agent/stage17-preroll-r2-receipts.md)。本次只读源码、读写文档；normal/tests/Native/COM/sandbox 新增均0，无终端脚本、无再委派。旧委托许可撤销，主代理监督；不得据历史余额启动运行。

### 最终原件与公开能力
原件：[result.json](../../WpfGfxShape/artifacts/stage17-candidate-r3/readback/Results/Results/candidate/result.json)，task1543af5f33fc44c8bca7645bdb546d69；目录名r3是最终两轮的第二轮，不是新增第三轮。case Stage17-public-MediaPlayer-purevideo-v2，真实 PresentationCore MediaPlayer→VideoDrawing→离屏RTB，businessSourceLinkUsed=false。
- Open-return S_OK；Opened332.6895ms；HasVideo=true、HasAudio=false、CanPause=true、duration4s、自然尺寸2x1。
- 请求0.5s，实际Position0.8s；seek408.7126ms/capture754.8213ms，RTB32x16/Pbgra32样本0000FFFF（红），renderingEvents22。
- 请求2.5s，实际Position2.8s；seek782.0467ms/capture1032.6229ms，RTB32x16样本FF0000FF（蓝），renderingEvents40。opacity1/contentBounds0,0,32,16；非静态图片fallback。
- Ended2047.6118ms；beforeClose position40000000ticks/HasVideo1/HasAudio0/duration4s；Close S_OK、Window IsDisposed=true、cleanupDiagnostics=[]，Dispatcher shutdown/finalizers drained。底层引用和资源趋势仍Unknown，不由空cleanup推导无泄漏。
- status Green/业务exitCode0；comparisonStatus=PendingIndependentComparator，不是零差分。原sandbox工具无独立数值OSexit，仍Unknown；非TRX通过计数。既有official校准不重跑，不把官方转换色当候选expected。

### 实际身份与修正顺序
actualNativeModules唯一 Native/wpfgfx_cor3.dll，SHA C43749918C78A4B4B3B82AEDD452DCA0425FD0131C2F533AAE421A6B97A75B30，4809216bytes；requested/frozen/root originalRuntimeNative同SHA（root不是官方oracle），无官方wpfgfx模块。clean/aot.sha256同值。testSHA E1DA0CB1E3390DD8127CA9A41D32F71AB2D0DFB40222BFE31B0A4D04E1247447，MVID d522dcdc-a220-40ea-94a9-b42dbbf623b3；result1-8给出.NET10.0.13/X64/Windows10.0.26200。PresentationCore SHA0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31/version10.0.1326.50504；系统MFplat/mfreadwrite/mfcore/mfsrcsnk来自System32。身份出处为result404-449、598-647，不用raw地址充可信栈。
真实production修正：IndependentVideoPlaybackOwner.PrerollAsync 89-106在MTA预读、保留_pending首帧；IndependentMediaIdentity140-178先音轨能力检查、preroll/pending seek，再event1→11。第二修正Identity216-241在Seeking保存_deferredRate，310-328控制完成后应用。72purePassed在第二rate修正前，只证原八类纯测试，不假称最终源码全覆盖。
最终授权normal2/tests1/Native2/COM2/sandbox2已用：normal各exit0/1502warnings/0errors；隐式Debug1/Nativecompile2/COMcompile2。Native生产Release/win-x64/net10.0/runtime10.0.13/selfcontained/AOTtrue/defaultShared/两诊断false/ManagedAcceptancefalse；COM测试host PublishAot=false/ManagedAcceptancetrue，不改变生产AOT。完整参数、最后COM日志截断及出处见收据；本次未重发。
历史candidate-r1公开OpenTimeout与preroll第一轮task38d4eec78d544bd384beeb05e6f3714b新Native9C9AA6830F1869B4D32EE7ED7BD3AFB0A1EB0251E3C437F53B723FDB65788C5B：Opened324.172ms、红通过但Seeking后Play拒8007139F，均保留，不用最终绿冲销。

### E_NOTIMPL：确定的静态返回位置与调用相位
源码路径均相对于WpfGfxShape/Code/WpfGfxShape。
1. 最终uce-diagnostic.log共114行，本次全读：第1行channel0/thread2/80004001 packet解码为`Window.Draw depth=1 kinds=DrawVideo`，第2行`Window.Visit`，第3行`Targets.Render`；4-114为空packet重复错误，无方法地址/可信栈/时间关联。不得无证归因跨线程。
2. Core/GeneratedHardwareVisualRenderer.cs Draw81起逐条分派，仅处理PushTransform/PushClip/Pop、矩形、几何及DrawImage；120-121对非DrawImage/DrawImageAnimate直接NotImplementedHResult。**DrawVideo进入该条件必返80004001**，尚未AcquireSystemFrame，更未进入surface renderer。这是匹配Window.Draw日志的确定分支，不再笼统“待定位”。DrawContents65-71记录Window.Draw并原样返回，Render20-23记录Window.Visit。
3. 软件分支Core/GeneratedVisualRenderer.cs866-889明确接受DrawVideo：874 AcquireSystemFrame→source尺寸slot3→888持有SystemVideoSample加入draw。Core/GeneratedMediaPlayerResource.cs165通过Av.SystemMediaSource.Resolve；其32行→IndependentMediaIdentity.Resolve108-110只查registry。resource174-198注册composition/保存source；AcquireSystemFrame87-105优先独立source/frames，只有未知外部provider才走SystemVideoSample.Acquire。公开RTB红蓝属于这一分支，不证明窗口HW视频已呈现。
4. BeginComposition不是Window.Draw：Av/VideoSlave.cs140-143对_systemCompositionRegistered交换pending即S_OK；外部provider路径153-175才GetSurfaceRenderer/renderer.BeginComposition。ProcessUpdate的_composition=null时85行E_NOTIMPL是另一相位，不与已记录DrawVideo分派错误混同。
5. GetSurface/FrameSource概念映射：本实现独立帧没有名为GetSurface或FrameSource的生产成员；Av/IndependentVideoFrameOwner.cs13-31裁剪BGRA、强制alpha255，35-73建立WIC PBGRA32 GUID6fddc324-4e03-4bfe-b185-3d77768dc910，样本持source。Av/SystemVideoSample.cs32-49仅外部provider才QI→slot3取renderer→renderer slot3 BeginComposition→slot4 BeginRender取source，不是本独立registry优先路径。
6. RendererDrawVideo概念对应Core/Direct3D9SurfaceRenderTarget.cs3556-3571 ProductionDrawVideo→3609起DrawVideo，3641可调用device.DrawVideoToSurface再DrawBitmap；但当前GeneratedHardwareVisualRenderer根本未为DrawVideo接线，不能因底层存在接口就称HW支持。日志不能映射到单次packet的epoch/帧时刻，仍Unknown；后续如需逐调用动态归属，唯一待取字段为`renderPhase`（明确Window.Hardware.DrawVideo或RTB.Software.DrawVideo），不以raw地址代替。

### 下一必须回归（须另授权，不在本次执行）
- Seeking期间rate0/1保存、最后目标生效与Close/reopen/旧owner completion隔离的纯单元；不能复用修正前72项背书。
- packet1/11单次与epoch顺序、预读首帧不丢、不依赖Playing、seek清旧pending，以及失败无Prerolled。
- 真实含轨MF HasAudio=true→0xC00D36B4→公开Failed（且不Opened假成功），metadata unknown明确失败；当前仅静态分支，未系统验证。
- 补接硬件DrawVideo生产路径并验证Window.Hardware与RTB.Software两相位；当前新Native双架构/x86与默认full必须实跑，不迁移旧身份；独立比较器仍待运行。
- audio/真实volume balance/AVsync/强cancel/用户素材MP4继续Open；endpoint0、AVI16gap、resourcesUnknown、旧full红、memoryGreen=null、底层refsUnknown全保留。生产原loader/副本/委托/fallback永久禁止，合法外部provider保留。

### 阅读证据边界
前代理亲读范围只引用既有receipt记载：normal1 lastlog1-62、纯test1-61、Native各1-13、COM1 1-60、首轮result397-605及UCE29行；normal2仅1-8/56-63，未冒称中段亲读。本次亲读最终result1-76（工具截断，实际展示至76）、355-652分块，以及UCE1-114全量；不声称resolver77-354全读。最后COMpublish可保留日志1-60分块全读；原sandboxreturn本次未直接取得，仅主代理交接task/完成语义，不补造OSexit。关键events/pixels/metadata/modules/cleanup及最终尾已实读，源码静态行号见上；所有本次更新文档逐份回读。

## 以下为封存历史（不是当前终态）

状态：ExecutedFailed / CloseoutCompleted / OverallOpen。已亲自实际发布可运行COM包并两次启动Windows沙箱，未停在打包；不是PublicVideoValidated，不是完整替换。仅实际工具，无终端脚本、无再委派；本接续源码/项目改动0。

## 发布与身份
生产Native唯一发布此前exit0，stage17/native/wpfgfx_cor3.dll，默认PublishAot=true、NativeLib Shared、name wpfgfx_cor3，未改生产默认，未再次Native发布。追加唯一正式COMpublish3 exit0；绝对csproj，Release/win-x64/net10.0-windows/self-contained，PublishAot=false与WpfGfxShapeManagedAcceptance=true仅用于测试hostbuild；RuntimeFrameworkVersion10.0.13、TargetLatestRuntimePatch=false、AotRuntimeIdentifier win-x64、绝对AotDllPath、ComAcceptanceSandbox=true、Stage17CandidateRoot=true。完整原命令及截断见.coding-agent/stage17-r2-receipts.md。
全新stage17-r2/publish与clean分离；正式GetFileHash生成clean/aot.sha256=F165C4B0ABD9FB68E3B9054C1288917C7E0F8DBBABF793E1BE2C6EC1D398A31E。clean Native与root是同一新Native，root覆盖官方使默认PInvoke不回原版。测试侧false不代表生产新Native非AOT。Core/Desktop runtimeconfig均10.0.13。
COM原两次NETSDK1152失败保留：第二次虽ManagedAcceptance=true却漏PublishAot=false，命名条件不成立。追加return成功60行/exit0；stdout1687455字符/截前1667455/仅末20000；可见CA2007/MSTEST0037/MSTEST0044，aggregate warnings Unknown，不能写零。

## 真实公开候选
任务789e7dd15c3242458ed687a658673819，source仅stage17-r2/clean，exe WpfGfxShape.ComAcceptance.exe，准确args --pcore candidate purevideo-public，outer300，全任务回收stage17-r2/candidate-readback。
原结果Results/Results/candidate/result.json：Red/业务exitCode1，failureStage public-Open-before，ObjectDisposedException HRESULT0x80000013，栈MediaPlayerState.GetPosition→RenderPureVideoPublic→Run。mediaEvents=[]、mediaFrames=[]：Opened0/Ended0/Failed0，无Open-return，无已验证Position/HasVideo/Duration与红蓝RTB。不能把未运行说通过。
实际Native模块列表仅Native/wpfgfx_cor3.dll，SHA F165C4B0ABD9FB68E3B9054C1288917C7E0F8DBBABF793E1BE2C6EC1D398A31E/4808192bytes；没有第二official wpfgfx。originalRuntimeNative字段指root同F165，不是官方oracle。真实PresentationCore MediaPlayer/VideoDrawing路径，businessSourceLinkUsed=false，不以内链identity代替。
testSHA FD467AEFF40BD09115199E36BC7E079E7FBB4F3E324C6A9042DB77482C20FB7A/MVID35b0f8fa-cfe9-4cc3-9c79-b2bbc05e6a40/.NET10.0.13 X64。failureCleanup仅已证Dispatcher shutdown及finalizers completed。
重要缺口：测试finally在player.Close后读取player.Position，生产Position在_closing返回ClosedError；该清理异常覆盖原始Open异常并阻断后续模块/系统MF日志采集。原始Open HRESULT/message/slotlog/errorstack Unknown，不把0x80000013冒称原始媒体Failed；无静态图fallback，没有吞错判绿。GeneratedMedia resource/ResolveBridge/events真实到达未证，候选系统MF列表未采到。

## 第二sandbox：official独立fixture对照，不混候选
任务07f0fe1666aa4b7e8f4954332d7f418b；source stage17-r2/publish（保留官方root，而非clean），同exe args --pcore original purevideo-public/outer300；完整回收official-readback。
实际唯一官方模块AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1/version10.0.13；未加载candidate，frozenCandidate字段不等实际。相同testSHA/MVID、fixture SHA F277087DA15898B6C96720AAEF17062A6E9D221856055CD2B51014FED323E7E4/864bytes。源码WriteMediaAsset(includeAudio:false)仅vidsDIB流、40帧10fps；官方实测HasAudio=false、HasVideo=true、CanPause=true、Duration4s、2x1，认证无音轨事实与可打开，不认证候选MF检测。
事件Open-return0、Opened(position6222 ticks)、Play-return0、Close-return0(position5004079 ticks HasVideo=true)；Ended0/Failed0。0.5s首红像素等待超时，0x80131505/Red/业务exitCode1，mediaFrames=[]，无实际红蓝像素样本；fixture像素验证亦失败，不能声称候选像素回归根因已隔离。实际System32 MFPlat/MF/MFTranscode/MFCORE/mfsrcsnk/quartz/mfasfsrcsnk/mfps均回读；dispatcher/finalizers完成。
两次工具只返回完成与回收路径，无独立数值OSexit；仅业务result exit1，不造工具/监督进程exit。非MTP入口，无TRX计数。

## 生产缺口与精确下一修正（本轮不修改、不重发）
1. 已确认合同不兼容：官方MediaPlayerState.Open(645-658)先SetSource/OpenMedia再立即SetPosition(0)；IndependentMediaIdentity.BeginSeek(298-319)在Opening直接返回0x8007139F，而且无owner返回E_NOTIMPL。必须为尚无source/Opening建立真实pending position目标，Open完成后按epoch应用seek，不能假S_OK空行为；保持负数/越界与失败状态校验。该缺口足以阻止正常公开Open序列，但由于原异常被finally覆盖，尚不能认定它就是本次首个HRESULT。
2. 已确认Close语义/证据问题：Position在_closing返回0x80000013；明确Closed与Shutdown/最后Release区别，按公开WPF合同实现Closed getters与可重开状态；测试清理先保存原异常，Close/各getter/模块/系统MF/slot日志独立try并持久化，不能finally异常替换主异常。下轮须先采每个MILMedia入口slot与原HRESULT，确认是否还有SetSource/URI/关闭队列差异。
3. 另行授权后先修最小生命周期与pending seek，重做定向纯测试，再生产新Native发布与正式COM包；不修改冻结F165、不复用F48/6BDF证据。先获得公开Opened及真实Resolve/resource调用，再seek红蓝RTB与Ended，保留实际像素即使不符；official fixture RTB超时需单独查scrub/暂停/同步通道更新，禁止放宽像素判定或静态fallback追绿。
4. 候选纯视频通过后再跑含音轨：真实MF HasAudio=true→能力HRESULT0xC00D36B4与真实Failed，unknown metadata明确失败，不静默丢音。本轮未跑含音轨，不能把纯test或officialHasAudio=false代替此门。

## 最终账与范围
normal3/3、tests2/2（首次86pass1fail，最终87pass0fail0skip）、Nativepublish1/1、COM原2fail+新增1success=3（追加1/1）、sandbox2/2。隐式编译独列：Debug2、Nativecompile1、COMpublishcompile3。原normal2失败、原COM2失败、stdout截断/警告Unknown和追加预算均保留；本接续新增normal0/tests0/Native0/source0。所有余额封存，不再同条件candidate追绿。
默认公开factory已启用但为ExecutedFailed/UnverifiedSource，不升稳定生产媒体基线；既有有限ABI保护只保留其历史范围。audio/AVsync/强取消/x86 Native/full仍Open；旧endpoint0、AVI gap16、resources Unknown、full红、memoryGreen=null及底层refs Unknown不冲销。生产禁止原版loader/副本/委托/fallback，合法外部provider保留。无全替换声明。
