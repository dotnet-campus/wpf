# Stage18 独立媒体硬件视频验证

## Stage18-r2 最终：LimitedHardwareVideoCommandValidatedAndPublicVideoValidated / CloseoutCompleted / OverallOpen
本节为唯一当前结论；下文 Stage18 初轮与旧余额全部为历史，保留而不冲销。本次仅主代理亲自读取既有证据、写文档与回读；源码/build/test/publish/沙箱新增均0，无终端脚本、无再委派，不使用剩余normal1凑量，源码freeze不变。CloseoutCompleted仅指本slice证据与文档收尾，不是完整替换完成。

### 原件与身份强门
既有sandbox2 task `0a40dce0e4f14e7cbe5d4bb358b15af6`，回收根 `WpfGfxShape/artifacts/stage18-r2readback/`。本次分段补读 `Results/Results/candidate/result.json` 至659行完整尾，未停在头部Green；`uce-diagnostic.log` 1–14、`aot.sha256` 1行、`WpfGfxShape.ComAcceptance.runtimeconfig.json` 1–21全文核实；binary未作文本读取。
result421–457、623–652：requested=frozen=actual唯一Native `D6FCF4F8A7D56F6D496D4EA58B3BF76F48B2EE25F7BC97287C908BCD00A19198`，4829696bytes，与回收aot.sha256一致；actualNativeModules只有Native/wpfgfx_cor3.dll，无official。originalRuntimeNative是根目录同hash候选副本，不是官方模块证明。businessSourceLinkUsed=false；wrapper/source-linked独立身份无记录，Unknown，不借false扩大证明。
运行.NET10.0.13/X64；Core/Desktop runtimeconfig均10.0.13。test SHA `5C323858201F1F57404DD3F9165EB23E57A6230D2834C8E1C56FF913A69DCEA6`，2000896bytes，MVID `b18a023e-d486-4e89-9738-40d7f72d81a2`。MFplat/mfreadwrite/mfcore/mfsrcsnk路径均System32。fixture SHA `F277087DA15898B6C96720AAEF17062A6E9D221856055CD2B51014FED323E7E4` /864bytes。不能借C437/E6D09或其他历史greens认证此身份。

### 实际门与分层能力
|强门/能力|r2真实字段与结论|
|---|---|
|公开Open/metadata|Open-return S_OK；Opened327.8432ms；HasVideo=true/HasAudio=false/CanPause=true；4s、2x1|
|Play/Position/软件RTB|原合同seek0.5/2.5，actualPosition分别1/3秒；红0000FFFF/蓝FF0000FF；32x16、opacity1、bounds0,0,32,16；renderingEvents6/8；软件RTB通过，不改阈值或采样时点|
|事件/关闭|Ended2915.6442ms；beforeClose.position40000000；Close-return S_OK；Window-close IsDisposed=true|
|生产HW命令|ProductionHardwareVideoCommandsValidated：Closed合法无帧acquire/release S_OK；Opened PTS0，以及另一resource的PTS34000000，均acquire→draw-wic-upload-target→lease-released S_OK；后者未单独记录identity状态；14行未出现RO_E_CLOSED。仅认证本次已记录命令，不认证所有相位/所有帧|
|HWtargetpixel/windowDC/scanout|NotCovered，软件RTB不等于HW输出；无真实target像素readback或物理呈现证明|
|主error/业务结果|stage=completed、status=Green、exitCode=0；原件无独立主error/exception字段，主error字段Unknown（未记录），不伪造HRESULT或异常；PendingIndependentComparator不是零差分|
|清理|cleanupDiagnostics=[]；DrawingContext disposed；bitmap/visual references released；Dispatcher shutdown；finalizers drained；native module retained until process exit。底层refs/resources仍Unknown，不从空数组推出归零|
|工具退出|既有return为沙箱执行完成，无独立数值OSexit，Unknown；业务exitCode0不能替代工具OSexit|

### 语义修复与历史边界
据既有r2收据：AcquireFrame正常Closed/Opening/正常closing等absence返回S_OK/null；Failed保留真实Failure HRESULT；terminal仍RO_E_CLOSED；活动missing-owner保留8007139F，活动closed-owner保留80000013负控，不全局吞错。新增6例生命周期定向unit passed/0failed/0skip，与原72passed分别记账；不冒称Seekingrate/event新增专属unit，后者仍NotCovered。旧UCE缺state/epoch相位关联，r2日志不能倒签旧0x80000013发生相位。历史C437/E6D09失败与greens及旧full红均保留原范围。

### 执行账与下一直接工作
Stage18累计normal2/tests2/Native2/COM2/sandbox2；implicit Debug2/Nativecompile2/COMcompile2另列；历史余额normal1不再使用，本次无执行。normal旧3887warnings/0errors，新3895warnings/0errors；发布和测试aggregate warnings Unknown，不写0warnings。旧normal stdout3246707截前3226707，新3253501截前3233501；旧tests615444截前595444，新1627046截前1607046；r2 Native615532截前595532、COM1691107截前1671107，均仅末20000留存；完整raw缺失保留，既有执行参数见receipt，不新造raw或重跑补证。
仍Open：audioendpoint0/AVI16gap/audioAVsync/强cancel/public default full/当前Native双archHW；x86独立生产媒体公开链、HWtargetcontent、realconsumerloss仍未证；resources/底层refs Unknown、memoryGreen=null、comparator Pending不升级。
下一直接高价值顺序：最终源码相关主要回归（含Seekingrate/event专属unit）→x86生产独立media公开链→用户MP4含audio明确拒绝/纯video片段生产player→HWtargetcontent与realconsumerloss真实门。不泛选门、不等用户再次挑选；这不是本次新增运行授权。生产原版loader/副本/委托/fallback永久禁止，合法外部provider保留。
本次已同步并逐份回读receipt、next-session/current-work-item/current-stable-baseline/plan-status/final-matrix/完整方案/current-execution-rules；下文仅历史。

状态：Executed / CloseoutCompleted / HardwareStillOpen / OverallOpen。已亲自完成生产 NativeAOT 发布、COM 发布及真实 Windows 沙箱，不是静态审核结论。当前用户授权覆盖 Stage17 禁止新增运行的历史限制；仅实际工具，无终端脚本、无委派。

## 冻结与合同
本续未修改生产或测试源码。有限核查 GeneratedHardwareVisualRenderer 的 DrawVideo/DrawVideoAnimate → AcquireSystemFrame → MilBitmapSourceAdapter → WIC converter → D3D9 upload → ProductionDrawBitmap。animated rectangle、transform、clip 沿既有位图管线；visual alpha非1明确E_NOTIMPL，不是静默丢opacity。sample/source、WIC、converter、upload及operations均作用域释放。IndependentVideoFrameOwner复制有效aperture、校验stride并将MF RGB32第四字节设255。未知合法外部provider沿原scope，不新增原版loader/副本/委托/fallback。没有发现足以确定修正的新增格式/所有权bug，冻结后直接实际验证。

## 实际发布
全部完整参数、原return、截断范围见 `.coding-agent/stage18-receipts.md`。
- Native1：绝对生产csproj，Release/win-x64/net10.0，自包含/runtime10.0.13，PublishAot=true，ManagedAcceptance=false，两diagnostic=false，默认NativeLib Shared；独立 `WpfGfxShape/artifacts/stage18/native/`。原return成功，日志60行，亲读1-15、49-60，exit0并有Generating native code。stdout615530字符，前595530被截断，仅末20000可得；warnings aggregate Unknown。
- COM1：绝对COM csproj，Release/win-x64/net10.0-windows，自包含/PublishAot=false/ManagedAcceptance=true，显式新AotDllPath、RID及Sandbox=true，Stage17CandidateRoot=true，独立stage18/publish与clean。原return成功，日志60行，亲读1-10、51-60，exit0。stdout1687708字符，前1667708截断，仅末20000可得；warnings aggregate Unknown。
- clean/runtimeconfig全文亲读：Core与WindowsDesktop均10.0.13。正式打包GetFileHash的aot.sha256全文为 `6EDC1CE97B3C986195BEC65B4A4A0C8C274D695A4B3B341469821A870CB4268C`，不是历史C437。

## 唯一真实系统运行
source=stage18/clean；exe=WpfGfxShape.ComAcceptance.exe；args=[--pcore,candidate,purevideo-public]；timeout300秒；全任务回收stage18/readback，无第二official。
原return“沙箱执行完成”，无独立数值OSexit；task `ce5883a4357446c6a4384e327827ee66`。关键原件 `WpfGfxShape/artifacts/stage18/readback/Results/Results/candidate/result.json`（652行）、`uce-diagnostic.log`（104行）。identity/events/frames/cleanup/完整尾已亲读；UCE关键头及末尾已读。
actual唯一Native=requested=frozen=正式GetFileHash上列6EDC，4810752bytes；没有official模块。originalRuntimeNative字段只指根目录同hash候选副本，不是官方认证。businessSourceLinkUsed=false；.NET10.0.13/X64。test SHA E1DA0CB1E3390DD8127CA9A41D32F71AB2D0DFB40222BFE31B0A4D04E1247447，MVID d522dcdc-a220-40ea-94a9-b42dbbf623b3。MFplat/mfreadwrite/mfcore/mfsrcsnk均System32。

|门|本轮真实结果|
|---|---|
|公开Open/metadata|Open-return S_OK；Opened318.7997ms；HasVideo=true/HasAudio=false/CanPause=true，4s，2x1|
|Play/Position/软件RTB|固定seek0.5/2.5，actual0.8/2.8，32x16；红0000FFFF、蓝FF0000FF；不改阈值/采样时点|
|事件/关闭|Ended2029.648ms；Close S_OK；Window-close IsDisposed=true|
|业务结果|Green/exitCode0；PendingIndependentComparator，不是零差分|
|HW命令链|首Window.Draw depth1 kindsDrawVideo → Window.Visit → Targets.Render：真实HRESULT **0x80000013**；旧0x80004001未在本轮已读日志中出现，但不能称HW成功|
|D3D上传成功/GPU target内容/windowDC/scanout|NotCovered；没有成功读回或专用HW成功字段，RTB不代替HW|
|清理|cleanupDiagnostics=[]；DrawingContext disposed、bitmap/visual references released、Dispatcher shutdown、finalizers drained、module retained until process exit；不证明底层refs归零|

素材SHA F277087DA15898B6C96720AAEF17062A6E9D221856055CD2B51014FED323E7E4，864bytes。原采样合同与公开WPF入口未放宽。

## 新失败与停止修正条件
真实系统结果已经从旧HW E_NOTIMPL进入新的0x80000013失败，不能只依据业务Green验收HW。有限定位：IndependentMediaIdentity.AcquireFrame在_closing、Closed、Opening、Failed时返回ClosedError，缺owner也返回同值；IndependentVideoFrameOwner.Acquire在_closed时返回同值。当前UCE没有时间/状态/epoch相位关联，不能断言来自Opening还是Close，更不能通过将所有closed错误改S_OK隐藏问题。因此本续不做无证修正、不原样追绿，保留最后Native1/COM1/sandbox1用于确定原因后的最小修正。当前障碍是失败相位缺证，不是工具不可用。

## 覆盖、预算与保留项
累计normal1/tests1/Native1/COM1/sandbox1；余额normal2/tests1/Native1/COM1/sandbox1。implicit Debugcompile1/Nativecompile1/COMcompile1另列。承接normal Release构建exit0/3887warnings/0errors/21s，stdout3246707截前3226707；旧pure本轮真实72passed/0failed/0skip/39ms，stdout615444截前595444。不重跑旧72充数。
新增Seekingrate/event函数指针单元 **NotCovered**，包括SetRate0/1意图重锚与Opened1→Prerolled11每epoch单次；本轮系统事件不能替代这些定向单元。
整体full Open；audioendpoint0、AVIgap16、audioAVsync、强cancel、x86、旧full红、resources/底层refs Unknown、memoryGreen=null均保留，不提升完整HW或完整替换基线。

## 后续精确门
先取得0x80000013对应renderPhase与identity state/epoch，不改HRESULT合同；若定位确定bug，才用余额最小源码修正→必要定向测试→新Native/COM/候选沙箱，禁止原样追绿。成功命令级验证仍不能替代GPU内容readback。继续Seeking/event单元及当前新Native双架构/default full需要各自真实证据。
交接入口next-session、current-work-item、current-stable-baseline、plan-status、final-matrix、完整方案与current-execution-rules已同步本轮身份、新HRESULT与未覆盖边界；历史条款只保留历史范围。
