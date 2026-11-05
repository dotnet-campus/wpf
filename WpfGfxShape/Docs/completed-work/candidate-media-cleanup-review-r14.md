# r14 媒体 Close/Open 清理独立审查

## 结论与范围

有限修复已编译、专用测试及新独立 x64 Native 真实 consumer 两轮验证通过。仅编辑生产 `IndependentVideoPlaybackOwner.cs`、`IndependentMediaIdentity.cs` 及本报告；未更新共享入口、项目配置、可见性、测试框架或其他生产文件。没有扩大 resize/桌面门，没有原版加载、复制或 fallback，没有终端/脚本执行，没有生产测试状态或开关。默认 async、oracle、expected、阈值、DPI、flags 均未修改。

本轮先读 r7、实际存在的 r6（`candidate-purevideo-isolated-r6.md`）和当前实现。历史 `artifacts/candidate-purevideo-r6-final` 本工作区不存在，不将历史绿色移绑；复用现有 CandidatePureVideo、CandidateVideoSupervisor 与共用 PureVideoPublic 源码重新构建包并运行。

## 确认的实质问题与最小修改

### 1. owner finally 的独立资源释放

原 Run finally 在单一 try 中依次 Dispose reader、Frames、commands、commandReady；reader Dispose 抛出后，后续释放和 preroll/seek/play completion 取消均被跳过。

不是仅理论上的 Dispose 可能抛：`MediaFoundationSourceReader.Close`（MediaFoundationSample.cs 692–726）在 Flush HRESULT 失败、callback WaitDrain 超时返回失败，`Dispose` 使用 `Marshal.ThrowExceptionForHR`；MFShutdown 失败也沿相同路径传播。此前此类故障会令 Frames 仍可获取旧帧、独立队列/事件未释放、等待操作不结算。

现改为 reader、Frames、commands、commandReady 各自 try/catch 收集故障；reader 字段及 pending 字节在 finally 清除；仍取消 play/preroll/seek；所有尝试完成后才结算 `_closed`。单一故障保留原异常，多故障使用 AggregateException。现有 WaitForExitAsync 的 finally Join 不变，因此 CloseAsync 仍等待线程退出。catch 是线程退出边界的故障转交，不是 silent catch。

静态保证仅涵盖这些独立资源之间的继续尝试。没有强制拆解 reader 内部资源：Flush/drain 失败时 callback 可能尚在运行，强行 Release reader/callback/MFShutdown 没有安全性依据。不能声称本修复保证该情况下全部 COM 资源释放；该 reader 内部失败边界仍 Open。

### 2. pending close 异常完整性及最终 Release

BeginClose 原本累积 `Task.WhenAll(_operation, _exitEvents, _cleanup)`，但 CompleteCloseAsync 又用 async/await 包装一次 WhenAll：等待全部完成是成立的，向外故障集合完整性不成立，await 通常只重抛其中一个。现直接返回 WhenAll Task，其 Exception 保留完整聚合，不新增调度、同步等待或锁内回调。

`_releaseCompletion` 原仅赋值，没有读取；保存 Task 不等于观察异常。移除此字段，最终 Release 启动既有 FreeIdentityAsync；该方法捕获完成故障并将 `completion.Exception ?? error` 写到标准错误，finally 仍 FreeIdentity。读取 Exception 是完整聚合观察，异常文本包含 HRESULT、堆栈及内部异常，而非仅最外层 HRESULT。Console.Error 是实际运行时出口，不依赖 DEBUG/TRACE 或测试开关；无新本地化固定错误文案。没有声称标准错误一定由任意宿主持久化；重定向/采集由宿主负责。若错误输出自身抛出，finally 仍释放 identity，但输出可靠性及该次二次 Task 故障不在本轮保证内。

公开语义依据：IUnknown.Release 返回 ULONG 引用计数，不能用它返回稍后发生的异步清理 HRESULT；Close 的 HRESULT 表示请求接受，不承诺同步等待 MF 线程。故不把最终异步失败改成同步跨 ABI 抛出，也不新增回调到可能已释放的客户端。内部 Completion 仍可取完整 Close Task 故障；最终 Release 在 identity 内存释放前记录完整故障。成功路径以及 Close 后立即同一 MediaPlayer.Open 的既有语义不变。

## 其余 r7 开放项判定

* Notify/ObserveOpenAsync/OnFrame 的 owner 检查和锁外 proxy admission 之间仍存在窗口。MediaEventProxy 明确允许已 admission callback 完成、Dispose 拒绝后续 admission，但 identity 分离 owner 到锁外 Dispose 不构成原子 admission 边界。不能据现有证据声称跨 epoch 所有通知已被排除；也未确定这是要求改变的公开合同。未扩大修改，更没有把客户端回调或 Join 放进 identity 锁。
* SystemMediaSource.NotifyFrame 无帧参数，是失效提示；Acquire 经当前 identity 获取帧。可能多余失效提示不等于已携带旧帧。当前结论为静态边界，不是任意交错的动态证明。
* 本轮不新增公开 API、不改可见性、不增加反射或生产故障注入钩子。没有可安全从公开 API 确定性触发多个 Dispose/Close 同时失败的现成路径，因此没有伪造故障注入测试；上述异常分支证据为代码及真实 HRESULT 合同的静态证明。成功生命周期行为由现有公开 MediaPlayer consumer 回归。

## 新身份及并行 x86 边界

所有本轮输出隔离于 `artifacts/candidate-media-cleanup-r14`；Native/video/supervisor/identity 均另设本轮 obj/bin 路径，未使用并行 x86 发布目录。

生产编辑发生在本轮首次 run_tests 之前；顺序为生产编辑 → 83 项测试 → 新 x64 Native publish → video publish → supervisor publish → 第一轮 consumer → identity 快照 publish/回读 → 第二轮 consumer → 精确 2 项测试 → 本报告。工具未提供可信绝对编辑时间，故不编造日期/时刻；这里以操作顺序和内容 SHA 指定源边界。

只读 identity 工具冻结当前 505 项源清单：`identity-readback/Evidence/source.sha256`，manifest SHA256 `E59A19097000A40A91C6E3F7C96FF236FBBB6F873B50E0D69CFFBFE1653EE8F5`。该快照晚于 Native publish，不冒充整个并发仓库在编译瞬间的原子 attestation；本轮两个媒体文件在编辑后未再变更：

* IndependentMediaIdentity.cs：`D54165A10D6B2D8A54BC4C6CD6D8E99210B2600427D26B1BB7A3E6DFCE626C4F`，32894 字节。
* IndependentVideoPlaybackOwner.cs：`67B563A57AABDEAB875A2CA32703D53AD3232F3DB966E4A09D6B89094FB1B2DD`，15166 字节。

本轮新 Native：`E158C6277E3B3043E25A98D808C9D7B5ACB81CFE0DE8EBE5D3187B8381C1073B`，4895744 字节，Amd64，无 CLR header。两轮 actualNative 都是各自 task 的 Package/Native/wpfgfx_cor3.dll，该 SHA/长度一致，nativeBasenameHandleMatches=true。

Harness：`E61089A0A04DF598EF2CE05F0A9234DEB922710C7F373BA3D33BBC809E0B3B93`，MVID `ac869081-a1bb-47f2-871c-a6f00d7978d6`。不得把本次媒体修改和认证转用于同时进行的 x86 发布；本轮没有执行或认证 x86，也没有修改其目录。另一子任务须按其自身源快照/Native SHA 认证。

## 验证原件及执行账

### 专用测试

两次 run_tests 均工具返回成功，exit0：

1. 项目 Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj，筛选 `FullyQualifiedName~IndependentMedia|FullyQualifiedName~IndependentVideoPlaybackOwner|FullyQualifiedName~MediaEventProxy|FullyQualifiedName~SystemMediaSource`：83 通过/0 失败/0 跳过，51ms。
2. 精确筛选 `FullyQualifiedName~WhenClosedSourceReopensThenSameProxyReceivesOpenedAndFrames|FullyQualifiedName~WhenMissingOpenFailsThenCallbackCanCloseAndRelease`：2 通过/0 失败/0 跳过，112ms。

测试会构建当前生产。短耗时不夸大为完整系统故障注入覆盖。编译有既存 CA1416/MSTEST0044 等警告，未顺手修复。Native publish 实际执行 Generating native code 并成功，不是只构建 managed DLL。

### 两轮真实 WPF 视频与负控

原件根 `artifacts/candidate-media-cleanup-r14/{readback,recheck}/Evidence`。每轮保留 package-identity、candidate-video、candidate-raw/result.json、AVI 和四份 Pbgra32、子进程结果、监督 exit、stderr/stdout 及三负控结果。

|轮次|sandbox task|监督 PID/exit|视频 PID/exit|三负控 PID（均 exit1）|
|---|---|---|---|---|
|readback|61dfbbe31e754638a474c57e01388457|9096/0|1608/0|2656,10848,11820|
|recheck|8b2f8139cab14f0286fff99f9afc776e|7084/0|7784/0|2464,10940,4660|

监督结果来自 supervisor-exit-probe.json，视频来自 candidate-video-process.json，三负控来自 process-results.json。全部 timeout=false/reaped=true/drained=true，视频及负控 killError=null，passed=true/failures=[]。外层 sandbox 工具不提供数值 exit，不将其编造为0。

两轮共用未修改 PureVideoPublic：保留原 paused seek 0.5/2.5、独立颜色/时钟断言；连续播放一次 Play，中间无 Pause/seek，采样实际0.8/2.8，中心0000FFFF/FF0000FF、paused=false；第二 EOS 后三轮相邻 player.Close(); player.Open(uri);（135–145行确认两调用间无 I/O/等待），最终 cycle2/opened4/ended2、video=true/audio=false、duration4秒、2x1。两轮 cleanupDiagnostics=[]，candidate-video.stderr 均空。AVI 身份两轮为 `F277087DA15898B6C96720AAEF17062A6E9D221856055CD2B51014FED323E7E4` /864 字节。这里亲读 result 与模块身份；没有额外运行 r7 原始帧字节审计器，不把 result 字段说成独立原始字节复算。

三负控分别 missing-digest、wrong-sha、wrong-architecture，均按既有监督要求失败退出。没有降低阈值、改 oracle 或 retry-to-green；两轮是同包独立确认，不是失败后改包。

执行总账：run_tests 2，专用 publish 4（生产 Native1、video1、supervisor1、只读 identity1），sandbox 3（consumer2、只读identity1），独立 run_build 0。生产与测试文件编辑之外新增本报告；未新增验收逻辑。original/compare/full/resize/desktop/x86 执行均0。

## 剩余与不得外推

1. 未动态注入 reader Flush/drain/MFShutdown 故障或多 pending close 故障；独立释放继续尝试及完整聚合是静态证明，不是故障已复现/测试覆盖声明。
2. reader 内部在 drain 失败时的资源安全释放策略尚未闭合；强行释放可能造成 use-after-free，本轮未这样做。
3. 旧通知 admission 窗口及所有任意线程交错仍 Open；成功 Close/Open consumer 仅证明这些执行的公开行为。
4. 标准错误是最终异步故障的观察出口而非新增公开异常 API；宿主持久化、标准错误自身 I/O 失败均有上述边界。
5. 新 SHA 只认证本报告限定的 x64 视频路径，不认证并行 x86、resize、desktop 或整体 full。OverallOpen，但已完成的生产修改已编译、测试、独立发布和新身份真实复验，不留未验证生产改动。
