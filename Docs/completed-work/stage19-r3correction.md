# Stage19-r3 正式校正与收尾

状态：ExecutedFailed / CloseoutCompleted / OverallOpen。主代理亲自执行；本次新增源码、构建、测试、发布均0；没有终端、脚本或再委派。第一门失败后不执行第二门，不原样追跑，不改变预期/skip/颜色阈值/loader禁令。

## 原始证据与第一错误
任务4e470408896140cc9e0e08b32d5a0002，全任务回收WpfGfxShape/artifacts/stage19-r3audio-readback。真实路径Results/Results/candidate/result.json，450行已分段全文亲读；uce-diagnostic.log仅3行，已全文读。工具原return仅“沙箱执行完成。结果已保存到”该绝对目录，没有数值OSexit；OSexit Unknown。业务result.status=Red/exitCode=1；不能用业务码冒充工具OSexit。

固定用户MP4原47255617bytes，SHA928410B89545B2DF0AA7C8A46756D512D2BE4879209282F90D92D578A30CA40A。真实MF sourceProbe hr0/HasAudio=true/AudioMetadataResult0；AAC(stream0 subtype00001610)/H264(stream1 subtype34363248)，640x360，duration7438266666。原native subtype validation C00D36B4。
公开事件原序：Open-return S_OK → Failed C00D36B4(117.9025ms) → Close-return S_OK → Window-close IsDisposed=true。没有Opened/Ended记录。故合法targetseek/rate不再在Open内抢先同步失败的受测切片成立，真实MF Failed到达成立；不是timeout，也不把MediaFailed事件本身当红根因。
但其后构造failureContract时读取MediaPlayerState.get_HasAudio抛COMException 8007139F，failureStage=real-MediaFailed，最终Red/business1。failureContract整个对象未落盘，softwareRtb/nonzeroBytes未落盘；未证明无帧/像素安全门，不能ExpectedUnsupportedGreen/Passed，更不是audio播放。原异常完整保留result尾，禁止改预期绕过。
Close0、WindowDisposed、cleanupDiagnostics=[]、failureCleanup=Dispatcher shutdown and finalizers completed已核；空diagnostics不证明底层refs/resources归零，仍Unknown。native日志Closed lifecycle-absence/sourceFalse/S_OK及HWacquire/release S_OK，不证明失败后像素或HW输出。system MF模块独立清单未输出，Unknown，不借桥测试或文件存在冒充实际系统模块认证。

## 正式身份
原publish exact clean=WpfGfxShape/artifacts/stage19-r3clean。正式ComAcceptance.Package.targets中GetFileHash SHA256生成aot.sha256，已读出新hash CADD0C4F857BA4546530D21E93A9843F03814DC259818430E8798020AB6DF289；actualNativeModules恰1个Native/wpfgfx_cor3.dll/4829696bytes，同requested/frozen/清单一致。allWpfgfxModules无official/第二wpfgfx DLL（另外两项是测试exe/dll）。originalRuntimeNative字段指向根目录候选同hash，不能将字段名当作加载official证据。businessSourceLinkUsed=false。.NET10.0.13 X64/Windows10.0.26200。
本次test SHA94AE5D1885BF30418A79BE69DAC3F62286370FB5940FD118A6DD27E464989837，MVID e642f68b-4fa9-498d-bf51-7e84a00a9c4d。公开DLL metadata核实无参准确FQN WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenStage19RealMfRunsThenSeekingRatesAndOpenEpochOrderArePreservedAsync；音频scene实际caseId与调用栈证明编入，不文本读二进制。

## 第二门与预算
Seeking=NotRun/BlockedByFirstError；实际MTP/TRX matched/passed/failed/skipped counts均N/A（未启动，不是零匹配或Passed）。没有第二task/TRX/监督/business/OSexit。源码桥依次两Open epoch事件1→11恰1、Rate0/1；真实MTA queue admitted/resume握手，不瞬态状态轮询。但未执行不能声称系统通过，也未独立认证Stage3Production assembly SHA/MVID。
若后续获授权执行，固定同clean/exe参数[--supervise-exit,Results/stage19-r3-seeking,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenStage19RealMfRunsThenSeekingRatesAndOpenEpochOrderArePreservedAsync]，外300、全任务回收，不猜FQN、不以零匹配通过。
追加上限normal2/tests2/Native1/COM1/sandbox2；r3累计已用normal1/tests2/Native1/COM1/sandbox1，余normal1/tests0/Native0/COM0/sandbox1。firsterrorstop冻结余沙箱，不授权继续使用。既有r2累计normal2/tests1/Native1/COM2/sandbox2保留；合计normal3/tests3/Native2/COM3/sandbox3。本次仅sandbox1。旧implicit成本按旧收据保留，本次implicit0。
既有r3 Release3907warnings/0errors；pure4case及主要filter82Passed/0Failed/0Skipped（4包含在82，不相加）；旧101为历史非新重跑。Nativepublish1exit0；COMpublish1原lastlog全60行亲读exit0，stdout1696744/前1676744截断，aggregate warnings Unknown，原完整参数见即时收据。没有重新发布。

## 已闭切片与下一直接动作
已闭真实slice：新x64身份认证；Open接受合法target初始化并收到原真实MF C00D36B4；清理操作完成。未闭：MP4完整拒绝安全门（HasAudio查询8007139F）；Seeking系统门。新x64 sourcechanged，x86仍历史89113C907F3CAC2F51A2ADD859A85E9836DD4C555E201060BC13A4FF56CDFB6E，未同步，不借旧purevideo Green认证本轮。
下次首先复核官方Failed后HasAudio/HasVideo查询合同与当前生产实现，定位get_HasAudio的8007139F最小生产根因，不删查询/改预期；补生产API测试后按新授权验证两门。随后直接当前最终源码双arch production default full：固定同源码与SDK/runtime，不改TFM；x64/x86各默认Shared Release NativeAOT、独立publish/clean、正式GetFileHash与实际唯一模块认证，准确新测试身份；真实全分母TRX/业务/监督/OSexit分层，完整原件回收亲读，登记剩红，不用旧full/精选unit代替。此为下一阶段步骤，不是本次发布授权。
旧full红、audioendpoint0、AVI16gap、AVsync、强cancel、HWtargetpixels/realconsumerloss、refs/resourcesUnknown、memoryGreen=null与所有历史失败/greens保留；完整替换仍Open。生产official loader/副本/委托/fallback永久禁止，合法外部provider保留。
