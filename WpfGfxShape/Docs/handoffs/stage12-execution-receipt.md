# Stage12 生命周期与正式 NativeAOT 执行账

## 执行前合同
Before：identity Notify持_gate调用事件入队；owner关闭后仍执行所有已接受命令；Shutdown/ProcessExitHandler是Close别名；同步ReadSample无强取消认证。公开factory E_NOTIMPL/null，禁止原wpfgfxloader副本或fallback。
After目标：Close原子关闭命令/帧准入，已入队但未开始命令结算为取消，不再启动reader操作；已开始COM操作只在MTA owner返回后释放reader/MF/COM，再完成Closed。关闭从Open/Paused/Playing/Seeking/Ended/Failed与回调可请求；重复关闭幂等。外部回调不持identity锁；事件入队各自临时proxy ref，Dispose取消尚未准入事件，不撤销已准入回调。已借bitmap lease独立存活。lastRelease返回引用计数，不是HRESULT。

## 状态、取消与进程界限
Closed(initial)→Opening→Opened/Paused/Playing/Seeking/Ended/Failed；任意状态→Closing→Closed(terminal)。_closing与初始Closed区分，terminal命令RO_E_CLOSED且out初始化。同步MFReadSample/Create/Seek无法安全跨线程Flush或Release；Close完成必须等在途返回。未知无界native阻塞仍NotProven，不使用假Task超时遗留worker。程序仅Windows沙箱；监督超时终止进程是隔离边界，不是取消成功。Shutdown/ProcessExitHandler需单独裁定，不以alias成功认证完整合同。

## 顺序与预算
1. 阅读生产owner/identity/source reader/eventproxy/framelease及Stage11规则。
2. 最小关闭准入/退订修正与真实函数指针固定信号测试。
3. 定向pure回归，再正式Release win-x64 net10.0 NativeAOT默认诊断false首次发布。
4. managed/source-linked系统生命周期包与实际新Native负向ABI包分层，不混旧6BDF初始化fixture。
5. 每次运行完整读取可得证据，最终报告与所有恢复入口同步回读。
预算normalbuild≤3/tests≤3/Nativepublish≤1/COMpublish≤2/sandbox≤3，每次≤300秒；当前全0。不得新Host/工程/SDK/TFM/包。不为用尽预算执行无关case。

## 已执行
Tests1/3：绝对 WpfGfxShape.Tests.csproj，九类OR filter同Stage11，return“执行成功。完整日志共61行”。已亲读1–61，exit0，68passed/0failed/0skip，41ms。隐式Debug编译1独列；stdout1600227字符，前1580227不可见，warning总数Unknown。不属于新增并发覆盖，也非系统MF取消认证。

Nativepublish1/1：实际绝对生产csproj，Release/win-x64/net10.0/self-contained=true/PublishAot=true/NativeLib=Shared，两项诊断false，PublishDir绝对independent-media-native-stage12/native。return成功59行；1–18、19–59全部亲读。exit0，Generating native code及创建wpfgfx_cor3.lib/.exp、最终独立native输出均实际出现。stdout615159字符，前595159不可见；warnings总数Unknown，不能宣称无IL警告。Native额度已用尽，不重发。normal0/3、tests1/3、COMpublish0/2、sandbox0/3，implicitDebug1/publishCompile1独列。

COMpublish1/2：绝对COM项目Release/win-x64/net10.0-windows/selfcontained=true/RuntimeFrameworkVersion10.0.13/PublishAot=false/ManagedAcceptance=true/ComAcceptanceSandbox=true，AotRuntimeIdentifier=win-x64，AotDllPath实际新native DLL；publish/clean为independent-media-native-stage12兄弟目录。return成功60行，1–60全部亲读，exit0；stdout1661333字符前1641333不可见，warnings总数Unknown。clean/aot.sha256实际4A18E44182D1D2241FB9D2BB4D57959580D00FDEA9A86D4A1A29FBC6B76A91DD。publishCompile累计2。

## 本轮继续执行（覆盖旧预算上限，不覆盖历史事实）
用户上限normal3/tests2/COMpublish2/sandbox2/Native1，保守计入上述既有执行：本轮初始剩余normal3/tests1/COMpublish1/sandbox2/Native0。既有Native发布先于新增生命周期强门，不得改写次序。本轮修改MediaEventProxy queued-ref drain/callback-inflight，identity Close等待事件completion、Release0延迟释放storage；新增三项有限无MF函数指针测试。
normal1/3：绝对d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.Tests\WpfGfxShape.Tests.csproj；Release/win-x64/net10.0。原return“执行成功。完整日志共 62 行，可使用 read_last_log_lines 按行读取。”全部1–62已读：exit0/3826warnings/0errors/22.87s。stdout3200167字符，前3180167不可见，仅尾20000。剩余normal2/tests1/COMpublish1/sandbox2/Native0。

tests2/2（本轮1次）：绝对Tests项目；原九类OR再加FullyQualifiedName~IndependentMediaLifecycleTests，精确filter见本轮报告。原return“执行成功。完整日志共 62 行，可使用 read_last_log_lines 按行读取。”1–62全部读完：exit0/71passed/0failed/0skip/45ms，原68+新增3有限无MFcase。stdout1600601字符，前1580601不可见，warning总数Unknown；隐式Debug本轮1/累计2。
normal2/3：绝对COM项目Release/win-x64/net10.0-windows；原return“执行成功。完整日志共 64 行，可使用 read_last_log_lines 按行读取。”已读头1–9尾55–64：exit0/3991warnings/0errors/10.27s。stdout3329244字符，前3309244不可见；不称全部stdout可读。
发现并亲读既有Native readback监督40行：task5489753d32dc450fa61cbf0410574fc4，child0/no timeout/kill/cancel，stdout12行8passed，TRX准确TestResults/child.trx共68行已读结果及counter，尚有definition中段未读。identity实测SHA4A18…，wrapper21E491F83185983CD1D40926374F28C3B06AF4DB97B37BFF52F9DF7C9A092B58/MVIDd7e19719-d850-4e7e-b853-db72519cd7bc/.NET10.0.13/X64。此既有sandbox计入1/2，非本轮亲自运行、非新生命周期。剩余normal1/tests0/COMpublish1/sandbox1/Native0。
预注册COMpublish2/2：绝对COM项目Release/win-x64/net10.0-windows，selfcontained=true、Core/Desktop10.0.13、PublishAot=false、WpfGfxShapeManagedAcceptance=true、ComAcceptanceSandbox=true；fixture绝对independent-media-stage8-r1/clean/Native/wpfgfx_cor3.dll，publish与clean绝对independent-media-lifecycle-stage12-r1的兄弟目录，不相等不互含。新系统唯一组合FQN WhenLifecycleClosesThenCallbacksAndMtaCleanupComplete，固定AVI在PTS5000000回调Close/Release0及外线程双Close，固定信号等待callback退出；独立JSON各子case，单TRX不膨胀分母。Native额度已用尽且旧发布先于本轮变更，不再发布。

## 本轮实际终态
COMpublish2/2原return“执行成功。完整日志共 60 行，可使用 read_last_log_lines 按行读取。”1–60全读：exit0，stdout1665205字符前1645205不可见，warnings总数Unknown；publishCompile累计3。clean实际API核验WhenLifecycleClosesThenCallbacksAndMtaCleanupComplete无参Task；fixture aot.sha256=6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403，仅旧初始化，不是最新Native代码。
sandbox2/2本轮唯一：sourceDirectory=WpfGfxShape/artifacts/independent-media-lifecycle-stage12-r1/clean，executableRelativePath=WpfGfxShape.ComAcceptance.exe，arguments=[--supervise-exit,Results/independent-media-lifecycle-stage12-r1,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenLifecycleClosesThenCallbacksAndMtaCleanupComplete]，timeout300，整任务localOutputDirectory=WpfGfxShape/artifacts/independent-media-lifecycle-stage12-r1/readback。原return含ForegroundTopology supervisor-before-child，task eb697a12a44e47e3893aee84a898e97d及结果保存绝对路径；原return没有child exit，不拿topology当通过。
实际亲读TRX26/监督40/stdout12/空stderr/identity单行/三JSON各104行完整：单组合1passed/childexit0，无timeout/cancel/kill/drain错误。真实AVI callback-close/callback-release/external-close分别passed，均accepted0/pendingAtAcceptance=true/6frames后停止/stateClosed/ThreadExited/refs0/registryRemoved/descriptorDisposed；callback-enter→close-accepted→callback-exit→owner-completed。7次sample/buffer释放，buffer.Release1/attributes.Release1是真实值，reader.Release0/MFShutdown0/CoUninitialize各1，nativeRefsUnknown。lease跨Close只验Source非零并Dispose，不冒像素读回。
wrapper B271821DDEE1A24EC22BDF419CDCE277B761A01FA0F55A7BF9FCBB9E256DE64C/MVIDfb5857ad-1d7f-46c1-8458-27fe3160b14e；source-linked FAB80817C67A0DF0B97AF0CEEA72B81B476C728856DEE724C8CE592068FA86C2/MVID2411f677-6f75-4628-9760-af6c7798a37e；实际.NET10.0.13/X64/PointerSize8。未记录新MF模块版本，不搬旧版本。
终态LimitedCallbackCloseAndReleaseValidated/OverallOpen；normal2/3 tests2/2 COMpublish2/2 sandbox2/2 Native1/1（本轮2/1/1/1/0），implicitDebug2/publishCompile3。最终owner变更后系统包重编译并执行3pure；原68没有在最终变更后再次全跑。旧Native4A18不含本轮变更，最新源码Native编译NotCompiled；不突破额度。剩余normal1，其余0。
late-event/失败Open退订/在途OpenRead/lease像素/官方Shutdown合同/MP4关闭/最新Native仍未完成；slot22/23仅有限Close请求。正式报告与七入口已同步，保留全部历史红门。下一直接这些生产门，不只静态选门。

## 回撤与验收
失败保留原HR/exit/warnings/截断，不改expected、不循环追绿。保持公开factory关闭；不添加生产后门导出。正式Native可只证导出/工厂负向，不能冒内部player正向。任何未实测关闭并发/强取消/Shutdown完整合同保留NotProven。旧full红、AVIgap、endpointzero、MemoryDescriptiveOnly、audio/AVsync/双架构finalfull均保留。
