# 非显示真实资源持续生命周期：唯一有限趋势合同（静态交付）

## 裁决

调查完成，实施/执行 NotStarted，等待明确授权。当前阶段仅文档修改；源码、配置、build、test、publish、Native、沙箱、终端脚本、再委派均新增0。完整替换 Open。不是长期无界无泄漏、HW、loss、full 或媒体全闭。

唯一下一阶段：x86 Release，在现有 ComAcceptance exe 内新增默认不运行的显式有限循环合同，复用真实 AsyncSharedOwnership 的健康异步 WIC 跨 channel 完整生命周期，official 同序列先执行成功后 candidate，两个角色各只启动一个进程，各在该进程内连续16轮（固定预热4、测量12）。不以独立进程16次启动冒同进程累积。不是复跑已绿同例凑进度：新增且唯一的问题是完整 manager/connection/channel/外部真实 stream 所有权在同一 PID 的连续重建与关闭是否保持有限生命周期硬门，并保存逐轮趋势，而非再增加 ABI 分支。

申请外层沙箱900秒（工具最大1800秒以内），父监督840秒；不是沿用或擅改原180秒授权。本阶段不执行。未获900秒授权则停止，不缩轮数、删采样或改成多进程来塞180秒。

## 主要风险依据与不选其他场景的原因

PartitionScheduler.cs 的真实后台 MTA thread、pending work/scheduled partition 集合，以及 destroy 的 CommitSubmitted/finally Destroy、management completion、Dispose 外部 Join/回调线程不自 Join，是本次修改直接相关的累计风险边界。一次新进程通过不能发现重复初始化/反初始化后的残留 owner、队列或 worker。跨 channel WIC 路径真实调用系统 WIC decoder，经 wrapper/Bitmap95 transfer 与 duplicate 保持外部 callback stream；最终 disposal 是实际引用链到达末端的可观测信号，不是自增后自减的模拟资源数。

媒体候选通过 SystemMediaModule 借用实际同版官方 player/decoder/provider/sample；模块按设计进程驻留。decoder/player 及系统缓存不保证 Close 后立即返还私有提交，也不保证每轮新 worker。驻留既不能自动算泄漏，也不能自动豁免持续增长。当前媒体已有四帧/事件，但没有全 decoder refs、全 worker 退出或音频输出指标。媒体长期循环会把本次 scheduler 关闭风险和系统媒体缓存混在一起，解释力较低，故本次不同时加入媒体、多 connection、renderer cache 或大场景遍历。实际音频继续未证，不增加音频探针。

## 现存测试与指标盘点（不把静态存在当新实跑）

|现有入口/文件|真实观测与可复用部分|不能说明什么|
|---|---|---|
|ComAcceptance ReplacementResourceCycleTests.WhenProducerClosesThenConsumerRetainsFreshPixelsAndFinalOwnersRelease|同进程 official/candidate 两模块，各4预热+12测量；真实 software target/bitmap consumer，producer释放后读像素；四种 Release 返回值、descriptor callback/disposal；PrivateMemorySize64/WorkingSet64/HandleCount/Threads/GC.GetTotalMemory(false)逐轮|同步软件路径，不是本次异步 scheduler；两模块同 PID 会污染后跑者基线；检测延至循环结束，不符合本门逐轮失败先停；现成16轮不是内存阈值依据|
|UceChannelTests.AsyncSharedOwnership.cs 两无参正式入口|同connection、A/B shared partition；真实 stream→WIC frame→wrapper→Bitmap95 duplicate，独立Visual39 duplicate；A先关闭、B新alpha更新；最终两个deleted=1、真实disposal/callback各1且退出、destroy/disconnect/deinitialize和cleanup原件|Visual不引用bitmap；不证明bitmap像素消费、所有内部对象/GPU资源归零；现有单轮未断言worker退出；最适合作本门内核|
|UceChannelTests.AsyncDrain.cs C6/C8/O6/O8|真实队列关闭和外部stream disposal；candidate捕获release thread后5秒OS线程消失断言|official关闭sibling在caller，candidate callback重入，分支不同不能作等价循环；不再重开等待探针或把不同分支硬比较|
|AsyncUceChannelTests|connection引用先释放后channel仍可drain；真实callback、last owner和5秒worker退出检查|单例控制，不是同进程长期累计；不把已有有限绿升级全worker保证|
|MediaExportsTests.SolidFrame.cs|真实player创建关闭5×3色；并行player、20轮并行factory；replace/rebind/releaseCaller等现成路径|颜色exact适用性与本次弱语义媒体门不同；没有逐轮资源趋势报告，不直接重跑；非全refs/音频归还证据|
|Pcore/MediaScenes.cs RenderMedia|一个player两asset、四帧、换VideoDrawing、自然Ended、parallel Opened回调Close与预期失败Close、finally解除drawing/visual引用|Close后HasAudio/HasVideo仍可真；没有全refs/worker统计。WaitMedia原有可见/不同帧轮询保留历史，不复制成新颜色或统计追绿循环|
|Pcore/LifecycleSequenceScenes.cs|8轮、每轮10帧，NoInlining返回WeakReference，固定Collect/WaitForPendingFinalizers/Collect后tracked alive=0|只证列举的managed WPF对象可回收，不证所有native/GPU资源；GC改变实际运行条件|
|ReplacementStateSequenceWindowTests.cs|同PID3轮，GC后producer WeakReference与handles/threads/private/managed bytes|需窗口场景，本门不启用；producer死不等consumer全部native资源已归还|
|Pcore/DispatcherScenes.cs|2轮×3显式线程、shutdown与joined/callback记录|仅已知线程和资源；不是系统所有worker计数或全缓存|
|Tests/PartitionSchedulerTests.cs|生产scheduler的register/destroy/flush、共享/独立partition清理；直接关联管理关闭修改|含错误命令路径，不选入本次健康无故障注入循环；无真实官方同合同和逐轮进程内存序列|
|Tests/AvCompositionNotifierTests.WhenRegistrationCyclesCloseThenCallbacksRemainSilent|64轮注册/注销后通知，实际受控callback调用64|测试notification对象，不是decoder输出或真实媒体全生命周期|
|Tests/Direct3D9BitmapCacheLifetimeTests、BitBltColorSourceCacheTests|manager DestroyAllResources、同对象双owning slot按两次release、Dispose幂等；常量pointer/回调序列验证受控cache所有权|不是renderer全缓存驻留量，更不是实际GPU allocator。未发现正式名为RendererCache的全量统计入口，不引入私有诊断生产API|
|UceChannelTests资源addref/release、partition isolation、PublicAbi/Program.cs 8轮Cycle|真实deleted及管理清理、不同partition/connection相关现成覆盖可以提供有限语义证据|未找到可直接复用的“多connection长期趋势”完整采样入口；不扩本门或以计数快照冒所有连接/GPU资源|

指标解释：
- 实际外部stream disposal exactly once、callback entry/exit及无错误、真实Release/deleted、实际destroy/cleanup成功，是相应可见owner归还的硬证据；不延伸到不可见owner。
- 仅测试侧“创建数减关闭数=0”是已知对象账本回基线，须同时有真实返回值和末端disposal，不能自计数独立充当释放证明。Dispose被调用只是意图，必须核对其真实效果。
- Process.PrivateMemorySize64为进程私有提交，包含托管/原生allocator高水位、碎片、JIT、runtime及系统媒体缓存；WorkingSet64含驻留和OS裁剪，均非live allocation总量。可描述增长，不能据单值断泄漏或无泄漏。
- GC.GetTotalMemory(false)仅托管堆估计，未强制收集时含待回收对象；固定强制GC也只辅助观察受控reachable部分，不能统计native/GPU。
- HandleCount是进程kernel handle总数，不覆盖全部COM/GPU对象；Thread总数混有runtime/线程池，不能替代已知worker身份退出。GetGuiResources可数GDI/USER对象，不是DX资源；本次检索两个测试目录未发现现成该指标，不为本门新增GUI接口。
- snapshot可提供某类资源读回/有限快照，绝非所有GPU资源盘点。无现成全对象指标即记Unknown，不加生产私有诊断接口。

## 预注册：唯一执行序列，不得看数据调整

### 角色与轮数

一个正式协调测试体，建议精确新FQN：`WpfGfxShape.ComAcceptance.UceChannelTests.WhenRepeatedAsyncSharedWicLifecyclesCompleteThenEachRoundReleasesOwnershipAsync`。显式opt-in环境变量 `WPFGFX_RESOURCE_TREND=async-shared-wic-v1`；未设置则明确拒绝启动该特殊入口，不能作为skip通过。默认矩阵不新增执行：新方法与角色派发使用独立、默认未定义的 `RESOURCE_LIFECYCLE_TREND` 编译常量，仅授权Release发布通过属性 `ResourceLifecycleTrend=true` 添加；不是新增正式Configuration。

协调体在同一正式exe的新增受控 `--resource-lifecycle-trend-role original|candidate` 分支启动每角色一次。official完成16轮且全部硬门成功并正常exit后才candidate16轮；official失败则candidate0。每角色同PID、同模块实例贯穿16轮，轮间不得退出/重启进程、卸载DLL。每轮重新initialize manager/create connection/A/B与真实stream/WIC/frame/wrapper/资源，正常关闭反初始化，准确重复现有健康shared ownership协议。无随机参数、无业务故障注入、无重入新分支，无producer冒consumer。

固定轮号0–15；0–3预热仍逐轮业务硬门、原始记录全保存，不丢峰；4–15为12轮测量，固定窗口4–7、8–11、12–15。不根据曲线加预热、剔除异常值、改窗口、延后截止或重复到绿。

### 采样与GC

每角色起始、每轮BeforeInitialize、A关闭且B继续更新flush之后、全部close/deinitialize/WIC scope dispose/已知worker观察之后立即、以及该清理点后固定1秒，四点/轮；起始1+16×4=65条原始样本/角色。最后一轮延后点就是终点，不加看结果后的额外静置。轮号/phase/阶段、PID、UTC及单调时间、PrivateMemorySize64、WorkingSet64、HandleCount、Threads.Count、GC.GetTotalMemory(false)、GC.CollectionCount(0/1/2)、callback累计及每轮值、已知owner账本、实际thread identity一起写出。采样前Process.Refresh，及时Dispose Process及线程枚举对象。采样/日志开销属于测量条件，两角色一致。

显式GC策略：全16轮及起始均不强制Collect/WaitForPendingFinalizers；不使用GC.GetTotalMemory(true)，只记录自然GC计数，避免强制GC掩盖托管wrapper滞留或改变callback时序。本门不声称managed live set回基线。测试报告不长期持有WIC/COM wrapper、委托闭包或每轮完整events对象：落盘后只保留标量摘要；文本序列化/采样自身分配亦须承认。NoInlining不是native释放的替代。

### 每轮业务、清理与硬门

1. 原shared所有HRESULT正常、A/B创建不同且非0；两个资源duplicate非0；源A提交并flush后caller wrapper/frame/stream释放，源关闭前后disposals=0。
2. A真实destroy；B合法新alpha0.25命令/commit/flush成功，期间disposals仍0。这只是目标仍能处理更新，不冒bitmap颜色消费。
3. B两个资源release各deleted=1，commit/flush/destroy成功，disconnect/deinitialize成功；caller owning pointer全0，created/destroyed匹配，两显式duplicate释放账匹配，WIC scope Dispose完成，primary/cleanup/callback错误全空。
4. 真实stream disposal=1、callbackEntries=1且callbackExited=1，5秒截止；每角色累计应恰为轮号+1；后续固定延后点及下一轮不得发现已完成轮callback再进入。保留每轮轻量原子标量以识别迟到/重复callback，不保留大资源闭包。
5. candidate记录本轮真实disposal callback的OS thread ID/入口退出；若非caller且来自本协议释放链，复用现有DrainThreadExists方式，在deinitialize后5秒内验证该已知thread退出，再采清理点；caller thread不能假冒worker。若candidate未取得目标worker身份，则该worker项NotProven，整门不得宣称包含worker退出通过，停止而非另启探针。official仅记录callback线程及观察，不强迫其内部实现使用同一worker策略；两角色资源协议可比，worker实现项明确candidate-only，不能宣称官方全worker对照。
6. 已知本轮owned对象账归0，不声称生产全对象数归0。每轮完成即持久化独立business.json、事件序列和样本，先验证本轮业务/清理再进下一轮；预热同样首错停止。异常仍尽力独立清理并保存主失败和cleanup失败，不能清理异常覆盖业务首错。

时间合同：每轮20秒硬deadline（包含最多5秒callback、5秒candidate已知worker、固定1秒延后点及其余业务采样）；现有同步native调用不能靠CancellationToken保证取消，需角色内固定watchdog记录失败并终止该角色，不允许挂住后继续下一轮。角色上限360秒，协调体最多720秒两角色+比较/保存60秒=780秒，父监督840秒留清理回收余量，外层申请900秒。16×20×2=640秒是轮截止上界，不是预计正常耗时；无证支持32轮可靠完成于180秒，所以不申请180。若到角色/父deadline，记录Failed/TimedOut及部分证据，不补跑或自动加到1800秒。若后续实施发现20秒合同不可安全落地，停止请求修订授权，不能静默放宽。

## 软趋势报告与裁决

两角色分别以预热末轮3延后点为基线，保留所有65样本及按测量轮4–15各阶段的原始序列；逐阶段报告相对基线delta、三个四轮窗口的min/max/median、12轮普通最小二乘斜率（单位bytes或handles/轮）、首末差。峰值全保留。不得跨阶段混算或把official数据减掉当candidate净泄漏量；两个角色PID独立，初始化差异按各自基线描述。同版同架构同test/序列/采样合同才具可比性；不同reentrant分支不硬比较。

未找到适用官方/项目的内存bytes或handle容差阈值；本门预先决定**不判内存Green**，不发明“≤若干MB无泄漏”。即使official也增长，仍逐轮报告双方斜率/峰值/自然GC与边界，不自动豁免candidate；两边平坦也不证明无泄漏。硬门通过仅称“x86有限16轮持续生命周期已知所有权/回调及candidate已知worker切片通过；bytes/handles描述性观察，长期趋势未证”。任何硬门失败则整体该有限门Failed，先停，保留原始失败；不以soft指标平坦抵消。

## 最小实施范围与精确预算申请

仅两个正式项目可用：Tests为net10.0/MSTest4.0.1、引用生产，适合现有scheduler/cache单元；ComAcceptance为net10.0-windows/WPF/nullable/unsafe/exe，NativeRun为启动点、现有supervise-exit和RoleAsync。根及COM局部global.json均SDK10.0.107，保持不变。此次选择只改ComAcceptance测试侧，不改Tests、不新增第三Host/工程/库/资源/SDK/TFM。

授权后最小文件范围：
- `UceChannelTests.AsyncSharedOwnership.cs`：将现有单轮内核私有参数化（实施前补齐本次长段截断的源码读取，不新增调查或执行门），接受角色持有的module、唯一轮根/标量结果；旧无参入口仍保持单轮及原合同。module每角色只Load一次，不在16轮每次Load累积loader引用；不Free、不验卸载。原业务证据不覆盖写。
- 新增 `UceChannelTests.ResourceLifecycleTrend.cs`：上述正式协调体、角色循环/固定deadline、测试侧metrics与硬门验证/描述性汇总；不改生产接口，不加入错误输入或负控执行。最多一个新测试入口，不新增DataRow发现预算。
- `NativeRun.cs`：仅常量保护的角色分发和该协调体需要的有界角色等待；旧RoleAsync默认3分钟逻辑不变。新角色360秒不能直接误用旧3分钟上限；可新增仅本门私有helper，不扩大旧场景超时。
- `WpfGfxShape.ComAcceptance.csproj`：仅默认false属性及条件编译常量，Release win-x86原包规则继续用，不新增Configuration或改变默认矩阵。若添加用户可见错误文本，放现有AcceptanceResources.resx；避免新生产字符串。

预算：normal COM build≤1；COM publish≤1（如正常隐含编译发生单独记账，不再补normalbuild）；Native publish0/生产0；唯一沙箱≤1、正式协调体≤1、official角色≤1、candidate角色≤1、比较仅同体内一次；独立发现0/额外test调用0/重试0/显示ETW WGC等待探针0/full0。先x86，复用actual候选15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620与同版官方5EBD18E0…；实际加载认证必须本轮重新记录，不能只拿requested/frozen identity。x64 54BC62另授权，不自动扩。

独立输出建议 `artifacts/resource-lifecycle-trend-x86-r1/`：执行前冻结本合同/改动清单/预算，build/publish原始可见返回逐字保存及源工具截断边界，package身份/实际test SHA/MVID/runtime/pointer/两个actual模块/角色PID、supervision/TRX/stdout/stderr、每角色每轮business/events/65样本和描述性comparison完整保存。每轮创建即落阶段checkpoint，崩溃也保留partial；不得只存最终摘要。所有产物仅ignored目录，不覆盖原媒体/ownership/AsyncDrain/KN/显示账或原失败。

## 本次亲读范围、限制与恢复

本次亲读最新恢复入口顶部，x64 closeout与x86r2报告至尾、当前规则27行全文；完整方案§1–10及历史导航、remaining-gap全文、final矩阵各节（若长行源返回截断则不冒逐字全部亲读），current-work-item/plan-status顶部。源码亲读上述关键单轮AsyncSharedOwnership 1–263（长段截断处补读关键callback及后半完整），AsyncDrain入口与220–260、PartitionScheduler入口/执行/Dispose、ResourceCycle入口及循环采样、MediaScenes开关与Close/compare、SolidFrame入口、LifecycleSequence、Dispatcher片段、窗口样本、scheduler/notifier/cache单元片段，以及两个csproj/NativeRun入口；不是全仓源码审计，也未重新解析二进制/运行原件。

路径偏差：先枚举根Docs发现仅completed-work后定位实际WpfGfxShape/Docs；一次把源码文件作为搜索目录返回目录不存在，一次请求超出AsyncDrain尾部返回无内容，WpfGfxShape/global.json不存在后读取根及COM局部真实global.json，空Test1.cs无内容。均未启动程序或文本扫二进制，无新执行失败。

本调查静态Completed；下一Implementation/Execution AwaitingAuthorization，以上数字与单一选择已固定，不留“下次再设计”的Open占位。恢复入口/current-work-item/plan状态、方案/remaining-gap/最终矩阵顶部按本报告最小同步；所有旧段是独立历史账，不删不改原失败。整体包1–8与完整替换仍Open，显示Unknown，实际音频/全worker/所有CPU-GPU资源/长期无界趋势/HW/loss/full均不因本报告闭合。
