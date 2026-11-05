# Stage11 真实 scrub 执行预注册

本轮用户新授权覆盖历史“只读/额度已尽”；禁止终端、脚本、再委派，执行仅Windows沙箱。预算normalbuild≤3、定向tests≤2、COMpublish≤2、sandbox≤2、Native0；当前全部0，implicitDebug/publishCompile独列。

## before/after 与合同
Before：实际24槽vtable slot11为byte setter并返回E_NOTIMPL；slot13为CanPause输出指针。官方wgx_render.h 724–788按IUnknown三槽核对一致。WPF MediaPlayerState在属性改变时调用setter；AVElementHelper在Open前应用意图。官方wmpstateengine BeginScrubArc在Pause/Stop下取真实sample，不以playing状态伪装预览。
After目标：preopen保存意图；paused/opened/stopped SetPosition在同MTA owner Seek/Read，丢弃关键帧回退PTS直到首个>=target帧，单帧发布驱动既有UI代际；不开启连续playing。关闭scrub只seek不发布；播放重新锚时钟。seek失败保留HR、不造Ended，EOF无可用目标帧失败；unknown duration不假承诺精确scrub。已有lease保持可读。

## 实施顺序、状态与owner
1. 核官方not-scrubbing冻结、SourceReader seek/read及lease合同。
2. 最小internal owner seek单帧路径及slot11/preopen/state绑定，全部reader操作在MTA串行队列。
3. 真实函数指针pure与COM桥、固定4秒AVI系统scrub预注册场景。
4. 绝对Tests Release net10.0/COM Release net10.0-windows构建；pure实际类filter；发布兄弟publish/clean目录。
5. 首系统组合完整证据回读后才考虑第二MP4场景；逐份文档同步，不切factory。

## 回撤与失败规则
生产factory保持E_NOTIMPL/null；禁止原wpfgfx loader/fallback。失败不得改expected、降低515ms、循环按颜色追绿。最多一次确定编译修正；业务失败只有确证新原因再改再验。Close不承诺取消阻塞ReadSample，不以超时丢后台伪造成功；关闭后拒绝发布，事件/Release不得锁内调用外部代理，不selfjoin。

## 即时执行账
normal1/3：targetPath=d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.Tests\WpfGfxShape.Tests.csproj configuration=Release runtimeIdentifier=win-x64 targetFramework=net10.0；return“执行成功。完整日志共62行”。全62行读：exit0/3825warnings/0errors/21.77s；stdout3199419字符，前3179419截断，仅尾20000，未冒称完整输出可恢复。
tests1/2：同绝对Tests项目；filter为实际FQN IndependentMediaDurationTests/IndependentMediaIdentityTests/GeneratedMediaPlayerResourceTests/SystemVideoSampleTests/MediaCreationUnavailableTests/IndependentVideoFrameLeaseTests/MediaComEventBridgeTests/IndependentMediaStatusTests/IndependentMediaScrubbingTests九类的OR。return“执行成功。完整日志共61行”；头1–10尾53–61实读：exit0/68passed/0failed/0skipped/39ms。implicitDebug1独列，warning总数Unknown；stdout1600227字符，前1580227截断。
发布fixture已枚举stage8-r1/clean/Native/wpfgfx_cor3.dll，aot.sha256实读6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403。新stage11-r1/publish与clean是兄弟路径，不互含。normal2/3：绝对COMAcceptance.csproj Release/win-x64/net10.0-windows；return成功64行，读1–9/55–64：exit0/2480warnings/0errors/4.15s。stdout2092850字符，前2072850截断。
publish1/2：实际绝对COM项目Release win-x64 net10.0-windows self-contained=true RuntimeFrameworkVersion10.0.13 PublishAot=false WpfGfxShapeManagedAcceptance=true ComAcceptanceSandbox=true；AotDllPath为已验stage8-r1/clean/Native/wpfgfx_cor3.dll；PublishDir为stage11-r1/publish，ComAcceptancePackageDirectory为stage11-r1/clean（均绝对工作区artifacts路径）。return成功60行，读1–10/51–60 exit0，warning总数Unknown；stdout1661448字符，前1641448截断。publishCompile1独列。
已从实际clean DLL公开API核验WhenScrubbingRunsThenRealPausedFramesAndControlsPass为无参Task，非按文件名猜测。sandbox1预注册：sourceDirectory=WpfGfxShape/artifacts/independent-media-stage11-r1/clean，executableRelativePath=WpfGfxShape.ComAcceptance.exe，arguments=["--supervise-exit","Results/independent-media-stage11-r1","120","FullyQualifiedName=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenScrubbingRunsThenRealPausedFramesAndControlsPass"]，timeoutSeconds300，整任务回收localOutputDirectory=WpfGfxShape/artifacts/independent-media-stage11-r1/readback。固定4秒AVI红5000000/蓝25000000 PTS，scrubfalse seek30000000不发布，515ms观察两次，然后rate1到EOS；接原controls回归。实际全部读完后才下一执行。
sandbox1/2实际返回task78fa94bec0004a118ef8238bfe9e5818，远端C:\CodingAgentSandbox\Tasks\78fa94bec0004a118ef8238bfe9e5818，本地d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\independent-media-stage11-r1\readback。原return只给ForegroundTopology supervisor-before-child（caller12116,parent10596,session1,ownerAweSun1228；wrapper module size1974272 ModuleError=null）及结果路径，无exit数字，待监督/TRX，不据此冒Passed。参数严格为上述预注册。
当前normal2 tests1 publish1 sandbox1 Native0 implicitDebug1 publishCompile1。

## 证据与保留
必须记录真实HRESULT/状态/epoch/PTS/UI代际/发布时间/软件DrawVideo像素/固定暂停/播放EOS/identity refs与descriptor。native refs未知不填0；音频AAC metadata不当输出。Stage10及所有历史失败、endpointzero、AVIgap/full红、MemoryDescriptiveOnly保留。source-link不是NativeAOT，旧6BDF fixture仅PE初始化。

## 正式终态：LimitedScrubAndControlValidated / CloseoutCompleted / OverallOpen
根Docs/completed-work/replacement-independent-media-scrubbing-stage11.md正式报告已建立并补齐实质证据；当前next-session/current-work-item/current-stable-baseline/plan-status/final-matrix/remaining-gap-closure-plan/current-execution-rules七入口已加Stage11权威段，旧历史不改。本次亲自读写文档，主代理监督；新增源码/build/test/publish/sandbox/NativeAOT/终端脚本/再委派均0。
前代理实读scrub302/TRX26/监督40/controls1–240。本次亲读scrub1–85/86–160/161–235/236–302；controls补241–320/321–400/401–480并复核1–85/86–165/166–240；TRX1–26、监督1–40、stdout1–12、空stderr、aot-identity单行；diag宽范围有截断，另亲读28–42、59–65完整尾，不冒全文。准确原件Results/independent-media-stage11-r1/TestResults/child.trx已亲读26行，total/executed/passed1、failed/notExecuted0；监督exit0/exited/no timeout/no kill/OutputCollected无drain错误。唯一既有task78fa94bec0004a118ef8238bfe9e5818，不多计组合子业务为独立TRX。
preopen-scrub-rate0 hr0、opened Paused/epoch0/published0/UI0；红PTS5000000epoch1/UI1、蓝25000000epoch2/UI2 productionDrawVideo；516.0223ms>=515暂停published2/UI2不变；scrubfalse seek30000000epoch3 published2/请求3消费2帧2；Rate1后PTS30000000连续至39000000总12/EOS，Opened1/NewFrame13×12/Ended7。Ended回调Close0/stateClosed及references0/registryRemoved/descriptorDisposed。nativeReferences Unknown。
原controls本任务回归：529.3486ms>=515 count6/PTS5000000不变；Stop/seek25000000epoch2/红蓝/Rate2 E_NOTIMPL/Resume到39000000共21帧及COM1/13×21/7；Close0/finalReferences0/registryRemoved/descriptorDisposed/stateClosed/Failure0/Endedtrue；ClosedLength与CanPause hr80000013输出0。无额外独立TRX。
监督wrapper87EF97FE2A6907954EF95C597A92DD441CD7661B37C3F2240AAD3B13D545EECE/MVIDb2ad717e-d118-47a9-8b64-3df732edae11；source-linkedF5839A11A860977CC0B3184A8674A4C170C2AC2DCD6EF4C528E361DA13ABD0E4/MVID9dc5a999-3d09-4723-b4c9-65464bae8ccd；identity与监督实际X64/PointerSize8/.NET10.0.13。旧6BDF仅初始化，非当前源码NativeAOT。
最终历史账normal2（Tests3825warnings/0errors，COM2480warnings/0errors）、tests1（68pure=原64+4全部pass/0fail/0skip）、publish1、sandbox1、Native0、implicitDebug1/publishCompile1。截断stdout与隐式构建warning Unknown保留，本次0执行。
下一直接生产受控Close/cancel并发生命周期及当前源码NativeAOT首次编译/公开工厂门审定，不泛静态选门。factory E_NOTIMPL/null；VolumeBalance/非0/1rate未支持、22/23仅Close别名完整语义未证；真实audio/AVsync/强cancel/回调Release/nativeRefs/完整WPF/完整替换Open或NotProven。原loader/委托禁止、合法外部provider仍合法；endpoint0不虚听声/不假存volume生效；AVI缺16samples/full红/MemoryDescriptiveOnly不冲销。本段为新终态，以上预算与运行均历史。

## 文档同步实际回读回执
正式报告最终40行已分1–12/13–25/26–40亲读至文件末尾；七入口新增Stage11权威段分别实际回读：next-session1–9、current-work-item1–8、current-stable-baseline1–8、plan-status1–8、final-matrix1–18、完整方案1–8、规则1–9，均覆盖新增段末并进入旧历史。本receipt终态31–36及37–38已实际回读至尾；本回执写入后再读取本回执至尾。不声称长历史全文重读。九份文档已同步，无待同步文档或下一运行。
