# Stage9 — 真实COM视频事件闭环有限验收 / CloseoutCompleted / OverallOpen

## r2 当前权威（以下旧终态和预算均为历史）
本轮亲自使用正式clean包执行真实Windows系统用例，未使用终端、脚本或再委派，未改源码。task=`1b55572c97374fbc8203b2bda9efce7e`；完整回收根=`WpfGfxShape/artifacts/independent-media-stage9-r2/readback`。即时账=`WpfGfxShape/Docs/handoffs/stage9-r2-execution-receipt.md`。真实FQN为`WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenPublicComProxyRunsThenPacketsAndRealVideoControlsPass`，不是猜namespace。

### 打包及执行
实际确认stage8-r1/clean/Native/wpfgfx_cor3.dll存在，并精确回读其aot.sha256及stage8成功发布记录。发布使用绝对COMAcceptance.csproj、Release/win-x64/net10.0-windows、selfcontained=true、RuntimeFrameworkVersion=10.0.13、PublishAot=false、WpfGfxShapeManagedAcceptance=true、ComAcceptanceSandbox=true、AotRuntimeIdentifier=win-x64、明确AotDllPath。规范化publish=`d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/independent-media-stage9-r2/publish/`；clean为同父目录`clean/`。发布前关系写入receipt：不相等、不互为前缀/父子，RemoveDir仅清clean，不删除publish。实际publish1 exit0，58行日志已读；stdout615375字符，前595375截断，尾20000；有CA1416，warning/error汇总未提供，不虚报0warnings。publishCompile1另列。
新clean/aot.sha256由打包GetFileHash生成并回读为`6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403`；Core/Desktop runtimeconfig均10.0.13。旧PE仅file/身份初始化，不提供本轮AV实现，不是本轮NativeAOT。未绕ValidateIdentity，未从bin运行，未使用旧包冒新代码。上轮publish2失败原样保留：缺历史fixture；publish=clean导致RemoveDir意外删除自有输出，原件系统从未运行。
沙箱source仅新clean，exe=WpfGfxShape.ComAcceptance.exe；参数`--supervise-exit Results/stage9-r2 120 FullyQualifiedName=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenPublicComProxyRunsThenPacketsAndRealVideoControlsPass`，外层180秒，整任务回收。原工具return含ForegroundTopology及保存路径，未给外层exit；不补造。监督40行全部已读：child2700/supervisor1908，exited/ExitCode0/TimedOut=false/Cancelled=false/KillAttempted=false，输出完整排空，无SupervisorError。TRX26行全部已读，1executed/1passed/0failed/0skipped，duration2.6737862s；stdout12行全读、stderr空。列目录未显示TRX子目录，按stdout精确路径读取成功，未以漏列声称缺失。

### 实际系统packet、控制、引用证据
业务`Results/independent-media-stage6/result.json`虽沿用stage6目录名，但本轮新任务生成，469行已分段全文亲读。源码核实：生产MILCreateEventProxy导出方法函数指针创建真实COM代理；identity.AttachEvents(nint)经官方IID QI和slot3 RaiseEvent，不是只用Action订阅冒公开proxy。Action FrameReady仅作帧/渲染观察。
- Open0，Opened packet ID1；21个NewFrame ID13；Ended ID7，顺序1→13×21→7。全部实际packet20bytes，hex分别以01000000/0D000000/07000000开头，HRESULT=0、两UTF16长度=0、尾padding=0；本轮系统packet无非空UTF16字符串，非空字符串系统覆盖Unknown。回调RaiseControlEvent返回S_OK；协议子步骤验证Failed ID8 packet及0x80070002回调HR原样透传，但该值为人工协议输入，不是MF缺文件系统失败。
- GetMediaLength0/value40000000，CanPause0/value1，NaturalSize2×1。Play0，Pause0；pauseWindowTicks5197853=519.7853ms，超过515ms，count6和PTS5000000前后不变。
- Stop0；Seek25000000/HR0，epoch2；首后帧25000000，后续26000000…39000000均epoch2，没有旧epoch帧泄漏。Rate2=0x80004001，Resume1=0。超尾40000001→0xC00D36B2为源码断言，保持Opened；并非本轮额外duration/replay用例。
- 生产DrawVideo软件RTB实际红帧PTS5000000像素0000FFFF0000FFFF，蓝帧PTS25000000像素FF0000FFFF0000FF，width2/height1/stride8。
- Ended回调内重入CloseHr0，无self-join；EndedLength40000000且源码检查position40000000。最后stateClosed/Failure0/published21/endedtrue/epoch2；ClosedLength和CanPause均0x80000013并清输出。最终identity refs0、registryRemoved=true、descriptorDisposed=true。caller proxy Release返回1为本用例源码断言；桥子步骤最终Release0、descriptor Dispose一次，属于同一单用例已执行断言，不冒独立业务计数。
- 回调内Release、Close/退订并发边界、Close后有界绝无通知、真实missingfile Failed均Unknown/未实测；当前单桥未实现真实missingfile步骤，不把协议80070002当系统失败。所有底层MF refs仍Unknown；不从descriptorDisposed推断native全0。MF同步Open/ReadSample阻塞的强取消与可靠关闭仍未保证。

### 程序集身份和系统模块
wrapper COMAcceptance SHA=`3BD4B8220230E6206AAE810D151A0ED3C417B61F2A184DA2D913D6818F242CB6`，MVID=`eeb32c6b-38ef-4723-97a6-4c0ecf75f2ef`（监督）。source-linked Tests SHA=`B6D152215FCC004C5FC843451A32E48C49BD5CA39EEDFAB709FE25F4F31085DA`，MVID=`295a0604-b042-4e1f-a8e6-129e8dceb93b`（业务），与wrapper分层，不互换。生产ManagedAcceptance单独SHA/MVID未输出，Unknown；实际backend明确source-linked production Av + GeneratedMediaPlayerResource → production DrawVideo software RTB，not NativeAOT。runtime .NET10.0.13/X64。aot-identity全1行读完，Sandbox=true/PointerSize8/实际任务Native路径及SHA匹配。
实际系统modules完整读：C:\WINDOWS\SYSTEM32\mfplat.dll、mfreadwrite.dll均10.0.26100.9278；C:\WINDOWS\System32\mfcore.dll10.0.26100.1，mfsrcsnk.dll10.0.26100.9278（均WinBuild.160101.0800）。diag48起有显示截断，另63–65完整尾已读，明确TRX精确路径及session exit0，不声称整个diag未截断。

### WPF必需slots审定（纠正旧预注册过度概括）
亲读MediaPlayerState构造37–56、Init、CreateMedia773起、OpenMedia及SetSource、属性和NeedUIFrameUpdate；AVElementHelper460–635。
真实顺序：MediaPlayerState.Init仅缓存默认值→CreateMediaEventsHelper生产proxy→HRESULT.Check(CreateMediaPlayer)→注册helper/events。并非构造时必然调用SetVolume/Balance/Rate/Scrub。SetSource变化→OpenMedia→HRESULT.Check(Open)。MediaElement在wasSet时先设ScrubbingEnabled，再Open；Open后才按已设置条件Position→Volume→Balance→Muted→剩余Scrub→Play/Pause及SpeedRatio。故不能泛称volume一律先于Open；用户可在Open前设置这些属性，失败亦会阻断其调用链。
|slot/API|现状与准确阻断位置|
|---|---|
|factory|SystemMediaSource.Create始终E_NOTIMPL/output0；当前第一硬阻断，不能开启|
|9 SetVolume / 10 SetBalance|Audio返回E_NOTIMPL；MediaPlayerState仅值变化且volume非muted时调用并HRESULT.Check。Open前显式设值或MediaElement Open后重放值会抛，不能用只存储值声称实际音量/平衡生效|
|11 SetIsScrubbingEnabled|Scrub E_NOTIMPL；仅变化调用并Check；MediaElement wasSet时在Open前，true会挡住Open|
|8 SetRate|已有限0/1，2返回E_NOTIMPL；PrivateSpeedRatio无条件Check调用。不能泛称当前全E_NOTIMPL；pre-open设置和任意速率未认证|
|12 IsBuffering|BooleanUnsupported/E_NOTIMPL；getter直接Check，非必然构造步骤，但查询即抛|
|13 CanPause|已有真实characteristics bit4+可用视频状态语义，系统HR0/value1；getterCheck，不再列未实现|
|14 DownloadProgress /15 BufferingProgress|Progress/E_NOTIMPL；getter直接Check，非必然Open前步骤；需本地/网络来源与状态区分，不假填1|
|17 HasAudio|BooleanUnsupported/E_NOTIMPL；getter直接Check；需要真实MF媒体流metadata，不能把当前只选视频误判文件无音频|
|21 NeedUIFrameUpdate|Unsupported/E_NOTIMPL；SendMediaPlayerCommand在!notifyUceDirectly时调用并Check；同步UI帧路径阻断，不能以已看到红蓝RTB代替|
|16 HasVideo /18–19 size /20 length|有限已实现并系统检查，不要求所有可选slot泛化实现|
下一必需生产API顺序：先真实source presentation metadata的HasAudio/明确受限本地source buffering-download状态及测试，再Scrubbing/NeedUIFrameUpdate真实帧通知合同、pre-open rate语义；音量平衡需接真实输出能力，不伪存储。并补真实Failed和回调Release/并发Close退订系统边界，及同步MF阻塞取消/释放，再做当前源码NativeAOT和完整WPF接入。不是本轮已实现承诺，默认factory不提前开启。原生产loader/provider转发已删除，SystemMediaSource.Create只返回E_NOTIMPL，原loader不可达。

### 最终账及gate
本轮normal0/2、host tests0/2、COMpublish1/2、sandbox1/2、Native0/0；publishCompile1，implicitDebug0。源码修改0；无原样追绿重试，未花剩余额度开支线。继承上轮Release3789warnings/0errors及58pure passed(原56+2)，本轮没有重跑58；新增系统单FQN1passed/0failed/0skip，不累计成59个纯单元或把内部调用当多个TRX tests。
本阶段真实COM视频packet/控制闭环有限Passed，文档CloseoutCompleted；非完整WPF MediaPlayer，非完整stage9所有生命周期角落Passed。factory E_NOTIMPL/null，NativeAOT/audio输出/AVsync/强cancel/AVIgap/HWloss/自然resource/finalfull/OverallOpen保留；历史endpoint各状态count0、AVI缺16frames Failed、旧full红、资源Unknown/memoryGreen=null不冲销。

## 历史预注册与上一轮未完成记录（保留）

## Before / After目标
Before：identity.AttachEvents仅接受内部EventProxy；公开factory为E_NOTIMPL/null。stage6虽有packet，不能算调用者IMILEventProxy COM闭环。After目标：内部identity绑定调用者真实IUnknown，经官方IID QI得到IMILEventProxy，以slot3发送同一生产packet；默认factory不变，禁止旧loader/副本/fallback。

## ABI核对
来源：wgx_render.h 82/846–859、mediaeventproxy.cpp 290–380、PresentationCore mediaeventshelper.cs 225–268。
IMILEventProxy IID 342efd8b-669a-4d16-b163-d75f5ffd1a10；IUnknown slots0/1/2，RaiseEvent slot3 HRESULT(BYTE*,ULONG)，Stdcall。AVEventData uint32 event/offset0，HRESULT/4，uint32 UTF16 codeunit lengths/8和12，strings/16；native sizeof结构20（包含WCHAR[1]及尾对齐），普通事件20bytes；字符串packet20+2*(typeLength+paramLength)，上限4096；NUL截断，不编码替换代理项。Opened1/Ended7/Failed8/NewFrame13沿现生产映射，本轮不新增Closed/seek事件。

## owner / 状态 / 回撤
QI持注册ref，转交descriptor owner；派发由现EventProxy临时ref保护，foreign RaiseEvent不在identity锁内。Close解除owner并丢弃未admit队列；已admit允许在途完成，不能承诺Close返回即所有回调结束。回调可Close/Release，无self-join。Failed去重且保留真实HRESULT；Ended当前只证明video EOF，音频drain未实现，不等同完整player排空。回撤仅移除此slice，不恢复旧loader。

## 验证计划和启用门
定向生产函数指针测试：真实QI/注册ref/packet/最后Release；现56相关回归保持。系统仅Windows沙箱；运行须留TRX/监督/业务及身份、packet/ref/cleanup记录。预算 normal<=4/tests<=3/COMpublish<=2/sandbox<=2/Native<=1，当前全部0。未完成托管系统门前不启动NativeAOT。
factory不能仅因事件桥成功开启：WPF初始化SetVolume/SetBalance/SetRate/SetIsScrubbingEnabled失败会经HRESULT.Check抛出；这些现E_NOTIMPL尚须实现有效pre-open设置，不能存volume冒音量输出。还需IsBuffering/HasAudio/progress真实metadata状态、取消正在Open/ReadSample并可靠释放、音频输出及排空或明确可强制且验证的video-only能力边界。默认factory保持原4negative，不要求所有可选slot一概实现。

## 本轮终态：ImplementedPartially / SystemAcceptanceNotRun / OverallOpen
生产EventProxy.CreateCom按官方IID调用真实client QI并持注册ref，经descriptor桥接slot3，最后Dispose释放QI ref；identity增加nint绑定入口，QI不持identity锁。AVI测试已改走生产MILCreateEventProxy函数指针+真实COM代理+identity绑定；COM wrapper增加单入口先测桥再运行AVI。factory/原4negative未改，无loader恢复。

normal1/4：绝对Tests.csproj Release/net10.0，exit0，3789warnings/0errors，21.53s，日志62行、stdout3172795字符仅尾20000。tests1/3：原56筛选加MediaComEventBridgeTests，58passed/0failed/0skip，33ms，exit0；implicit Debug编译1另列。stdout1586919字符仅尾20000，未给TRX路径，不声称TRX存在。新增2项覆盖真实生产COM创建函数指针、官方QI、原packet和回调HRESULT80070002透传、caller释放剩余1、bridge最后Release0、descriptor Dispose一次。该HR是纯协议测试值，不是实际MF missingfile Failed证据。

COMpublish2/2均失败，publishCompile2另列。首次明确Release/net10.0-windows/win-x64/selfcontained10.0.13/PublishAot=false/ManagedAcceptance=true/Sandbox=true，但误用不存在owned-desktop-full-x64-r1/clean/Native夹具，MSB3030。第二使用已列目录确认存在audio-endpoint-capability/clean/Native/wpfgfx_cor3.dll；操作者又把-o与ComAcceptancePackageDirectory都设stage9-r1/clean，现有AfterPublish RemoveDir删除同路径，随后Missing clean-package dependency退出1。第二14行日志全文读；第一59行只亲读48–59且输出截断，warnings总数Unknown。不能用第三次publish突破额度。

sandbox0/2、Nativepublish0/1。未从非正式bin目录绕过clean manifest。未完成本轮AVI/真实missingfile/并发退订/回调Close及Release系统验收，没有本轮系统packet、refs、模块、SHA/MVID/runtime、TRX/监督；历史stage6/8不能冒本轮COM证据。系统关闭后有界无通知、在途边界和EOF排空仍待实测。本文是未完成报告，不是stage9 CloseoutCompleted。终端/脚本/再委派0；系统MF未在宿主运行。

## Factory裁定及下一直接任务
不能默认启用：factory在Open前无source信息，不能可靠选择video-only；slots9/10/11及buffer/progress/audio尚无WPF必需语义。已亲读MediaPlayerState.CreateMedia780的HRESULT.Check(factory)、OpenMedia的Check(Open)、NeedUIFrameUpdate的Check；本轮未完整核初始化调用顺序，前述预注册不能当顺序审定结论。无音频输出认证、强取消和audio drain，不能用volume存储/假进度代替。默认E_NOTIMPL/null及4negative保持，不新建未经验证的试验factory。
下一直接工作：新授权下正确分离publish/clean目录，使用已认证fixture重建正式包，执行WhenPublicComProxyRunsThenPacketsAndRealVideoControlsPass并全文回读；补真实missingfile一次Failed、packet边界、并发退订、close/release与有界无通知；完整核WPF初始化必需slots及video-only能力合同，再首次当前源码NativeAOT，随后完整player接入。真实audio/AVsync/AVIgap/HWloss/自然resource/finalfull继续Open，旧full红/资源Unknown/memoryGreen=null保留。
