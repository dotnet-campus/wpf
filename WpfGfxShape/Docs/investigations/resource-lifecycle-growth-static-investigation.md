# 资源趋势增长静态定位：PendingInvestigate

## 裁决与本阶段账

本阶段仅静态读取源码/原件及修改文档；源码修改/build/test/publish/Native/沙箱/脚本/终端/再委派均0。调查文档 Completed，资源趋势 **PendingInvestigate**，memoryGreen 仍 null；完整替换 **Open**。x86/x64既有 **PassedKnownOwnershipAndCallbacksOnly** 不撤销，也不扩为OS资源回收通过。WorkerExitNotProven、GPU及无界长期NotProven保留。旧报告、失败、r1未跑发布/SourceBlocked及全部独立账不改。

实质定位：当前源码存在 **SameThreadChannel 私有 AutoReset EventWaitHandle 没有确定性关闭** 的所有权缺口；不是采样器忘记 Dispose ProcessThread。事件最终可能由 SafeWaitHandle finalizer关闭，故不直接等同永久泄漏，更不能将约+4拟合成四个事件。每轮两个channel只直接支持两个此类event的创建量，剩余增长尚无类型证据。

## 原件及阅读边界

工作区相对路径均从仓库根起。正式入口 `WpfGfxShape/Docs/completed-work/replacement-resource-lifecycle-trend-x64-r2-closeout.md` 全文、x86入口及其链接的根 `Docs/completed-work/replacement-resource-lifecycle-trend-x86-r2-closeout.md` 全文已读。规则为 `Docs/handoffs/current-execution-rules.md`（此处Docs指WpfGfxShape下正式Docs），以及连续性协议及主题页。历史报告保留其原亲读/监督来源边界，不冒本次全部重验硬门。

定义 X64T=`WpfGfxShape/artifacts/resource-lifecycle-trend-x64-r2/sandbox-results/Results/resource-lifecycle-trend-x64-r2/ResourceLifecycleTrend/1493b441b1ae4d86a804fcae8b8eb4ea/`；X86T=`WpfGfxShape/artifacts/resource-lifecycle-trend-x86-r2/sandbox-results/Results/resource-lifecycle-trend-x86-r2/ResourceLifecycleTrend/ddaf998ea0f24f98a5ca26af11704444/`。

本次直接读取：两T的candidate/descriptive.json Handles四阶段现成12轮value序列（195–261、939–1005、1683–1750、2426–2518）；candidate PrivateBytes +1秒汇总2240–2252/2308–2331；两official +1秒Handles汇总2426–2436及2490–2517；两candidate samples.jsonl 18–21、26–29、62–65（轮4/6/15四phase完整行）。不是本次亲读全部260样本，warmup/Initial其余数据沿历史报告，不声称全样本重新统计。未执行脚本，斜率/窗口引用原descriptive，不重新回归；下表只对现成整数作静态差分。未补造句柄类型、创建栈或finalizer完成时间。

## 1. 确证的阶段和数量规律

下表每格为 BeforeInitialize / AfterSourceCloseTargetUpdate / AfterCleanup / AfterCleanupPlusOneSecond 的**原始HandleCount**，不是扣除采样器后的值。

|measurement轮|x64 candidate|x86 candidate|
|---|---|---|
|4|299 / 308 / 303 / 303|317 / 326 / 321 / 321|
|5|303 / 312 / 307 / 307|321 / 330 / 325 / 325|
|6|307 / 313 / 311 / 312|325 / 334 / 329 / 330|
|7|312 / 321 / 319 / 319|330 / 339 / 334 / 334|
|8|319 / 328 / 320 / 320|334 / 343 / 338 / 338|
|9|320 / 329 / 324 / 324|338 / 347 / 345 / 345|
|10|324 / 333 / 328 / 328|345 / 354 / 346 / 346|
|11|328 / 337 / 332 / 332|346 / 355 / 350 / 350|
|12|332 / 341 / 336 / 336|350 / 359 / 354 / 354|
|13|336 / 345 / 337 / 337|354 / 363 / 358 / 358|
|14|337 / 346 / 344 / 344|358 / 367 / 362 / 362|
|15|344 / 353 / 348 / 348|362 / 371 / 366 / 366|

因此可静态读出：

- x64 Source−Before依次9,9,6,9,9,9,9,9,9,9,9,9；Cleanup−Source依次-5,-5,-2,-2,-8,-5,-5,-5,-5,-8,-2,-5；Cleanup−Before为4,4,4,7,1,4,4,4,4,1,7,4。
- x86 Source−Before始终9；Cleanup−Source为-5,-5,-5,-5,-5,-2,-8,-5,-5,-5,-5,-5；Cleanup−Before为4,4,4,4,4,7,1,4,4,4,4,4。
- 两架构仅轮6的+1秒相对Cleanup增加1，其余11个测量轮不变。所有列中下一轮Before等于上一轮+1秒（表内轮5–15），说明残留计数跨轮延续，不是仅活动阶段瞬时峰值。不是严格恒定每轮+4。
- x64 +1秒Handles303–348，原slope3.944055944055944个/轮、firstLastDelta45；x86同phase321–366、slope4.059440559440559、delta45。official同phase x64 274–275 / x86 292–293，两者slope0.06993006993006994、delta1。**双架构句柄相似增长已经核原件，不是从x64推测x86。**
- x64 +1秒PrivateBytes22528000–29765632 bytes、slope224105.62237762238 bytes/轮、delta1077248；x86同phase16896000–24473600 bytes、slope4611.580419580419、delta675840。x86三窗median22935552/22018048/22398976，x64为27891712/27590656/28854272。不能说两个架构私有内存同斜率增长，也不能将内存量归给event。

自然GC的实际时序：x64轮4GC0/1/2由Before与Source的4/4/4至Cleanup的5/5/5，句柄299→308→303；轮15由17/16/15至18/17/16仍344→353→348。x86轮4由4/4/4至5/5/5仍317→326→321，轮15由16/15/15至17/16/16仍362→371→366。两架构这些轮的Cleanup至+1秒GC计数不动，句柄也不降。轮6 x64 GC发生在Before至Source（6/6/6→8/7/7），x86发生在Source至Cleanup（6/6/6→7/7/7）；两者+1秒均额外+1而GC计数不变。GC.CollectionCount只表明宿主CLR采集次数，不是SafeHandle finalizer已排空，更不直接观测NativeAOT DLL自身运行时堆/GC。不得从自然GC后残留就断言永久泄漏。

轮4/6/15样本的candidate线程均11→12→11→11，与每轮短命scheduler线程创建/Join相容，但计数不证明线程身份全部退出。全部65样本/16轮4warmup+12measurement由原测试及旧报告约束；本次不重选窗口/丢峰/加预热/改阈值。

## 2. 对象生命周期证据表

以下文件行号均为当前可读源码；TST=`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/`，PROD=`WpfGfxShape/Code/WpfGfxShape/`。当前源码事实与历史二进制因果归因须分开。

|对象/创建与owner|显式释放/异常路径/最终化|定位与结论|
|---|---|---|
|每次采样Process与ProcessThread|Capture使用using Process；Refresh后读取HandleCount；Threads getter返回集合，finally逐个Dispose ProcessThread。不持有线程wrapper进入samples。若getter本身抛错则Read记录Errors；未发现已取得对象漏Dispose的正常路径。|TST UceChannelTests.ResourceLifecycleTrend.cs:139–174，尤其148–158。**没有确定采样器Dispose缺陷，不建议重复修已有Dispose。** Thread Count不需要在此打开每个线程的额外属性；不能因有集合就宣称每个都开OS句柄。|
|每轮模块认证Process/Modules、文件读写|AsyncSharedOwnership:120 using Process；Modules只读取BaseAddress/FileName等。File.ReadAllBytes、AppendAllText、WriteAllText为同步便捷API，未暴露持久stream owner；候选采样事件同样写文件。OS文件/模块内部缓存未测。|TST AsyncSharedOwnership:100、119–131、280–297；Trend:78、225–226。没有证明File句柄每轮遗留。Process在try结束、进入cleanup与AfterCleanup前已Dispose。|
|samples/states、字典、CallbackThreads数组、事件记录|角色List保存65样本和16个SharedRoundState至角色退出；观测数组逐样本增长。轮事件队列被state持有至business写出后state.Events=null，CallbackErrors/路径/KnownOwners仍保留。均托管GC，非每轮清空；没有这些记录自有OS句柄的证据。|TST Trend:25–26、76–78、98–107、163–172；AsyncSharedOwnership:25–55、73–100、300。解释一部分managed分配/驻留的确定来源，但不能解释+4 handles或全部PrivateBytes。|
|MemoryStream、descriptor GCHandle、WIC factory/decoder/frame/wrapper|每轮字节内存流而非FileStream；MemorySource.Create成功将root转给descriptor，失败Dispose并finally Free；MemoryDispose同步Stream.Dispose、root.Free后调用state.OnDispose。WIC factory/decoder由finally COM Release，frame交给caller；wrapper/frame/stream caller各Release，B最后资源释放触发callback；WIC scope最后CoUninitialize。|TST ManagedStreamTests.Lifecycle:132–188、218–225；SystemWicBitmap:16–43；AsyncSharedOwnership:161–184、209–267。失败cleanup逐owner尝试并记录Aggregate/primary；没有finalizer兜底这些裸COM指针。成功路径已知ownership证据通过，不代表decoder内部所有OS对象均已归零。|
|**每channel私有AutoReset EventWaitHandle**|字段初始化立即创建；owner是SameThreadChannel。ReleaseResources只Set唤醒、从registry摘除、释放资源，**没有Dispose/Close**；类自身未声明finalizer，最终依赖不可达后的SafeWaitHandle finalizer。构造后续失败同样无显式event清理。WaitForMessage显式DangerousAddRef/Release保护活动等待。|PROD Core/SameThreadChannel.cs:248、286–312、330–336、618–670。**StaticConfirmed确定性所有权缺口**；本合同A/B各一channel即每轮2个此event。不能据此claim观测到的45个增长全是Event或永久泄漏。|
|scheduler Thread、队列、完成源、同步唤醒|Acquire新建scheduler，构造Thread MTA/Start；queue与scheduled集合用Monitor.Wait/Pulse，不是自建AutoResetEvent。初始化/连接分别持ref；Disconnect和Deinitialize释放，末ref清静态字段后Dispose，停止+PulseAll，非worker自身调用则Join；Run终于CoUninitialize。TaskCompletionSource本路径未调用AsyncWaitHandle。Thread内部OS资源/运行时wait对象最终化不是这里显式Close。|PROD Core/PartitionScheduler.cs:18–42、87–132、198–209；Abi/PartitionManagerExports.cs:22–55、58–99；Abi/UceChannelExports.cs:122–135、216–243。活动线程增加后回落与此一致；运行时可能残留handle但需类型证据。Join不是GC finalizer排空。|
|role监督Process/CTS/registration/文件stream、两watchdog Timer|NativeRun role专用Start结果using，CTS/linkedCTS均using，stdout/stderr await using；finally kill/reap/drain；WaitForExitAsync注册属框架内部，不是每轮新建监督进程。角色两个Timer是角色级using而非每轮。协调CTS为父进程级。|TST NativeRun.cs:51–134；Trend:55–62、235–249。监督对象在另一进程，不能直接制造candidate PID的每轮句柄阶梯；Timeout强退路径资源Unknown。|
|connection refs/表、mapping/module|静态Connections/Channels表持对象，销毁移表并等待worker Completion；断连移connection并末channel释放scheduler ref；Deinitialize释放初始化ref。MilResource_DuplicateHandle是协议资源ID，不是Windows DuplicateHandle。NativeLibrary.Load在role循环外一次、故意不Free，合同DLL进程驻留，不验卸载；单轮非retained路径也无Free但不在此循环重复Load。未在此调用链发现每轮CreateFileMapping/MapViewOfFile。|PROD Abi/UceChannelExports.cs:13–29、49–57、122–135、216–243、261起；TST Trend:84–101；AsyncSharedOwnership:119。字典容量/NativeAOT runtime与模块映射可驻留，不能未经类型/内存证据解释趋势。|

### 最多三个待证假设

1. **channel event确定性关闭缺口 + NativeAOT finalization延迟/可达性**：代码支持每轮2个私有event仅最终化释放；能解释部分候选特有句柄，但数量不匹配全部+4，需类型/寿命核验。
2. **每轮scheduler Thread/Monitor/COM及运行时内部等待对象延迟释放**：每轮新Thread且Join，线程数回落不排除Thread/Event等OS handle残留；这是候选类别，不是已确认OS事件泄漏。
3. **测量/记录/运行时驻留或其他COM内部对象**：角色保留有限记录，native模块/双CLR堆、WIC/COM/模块缓存可能影响PrivateBytes；Process/ProcessThread已Dispose且official同采样近平台使“共享采样器普遍漏Dispose”不受当前代码支持。仍需真实二进制版本与类型证据，不把差异直接归因产品或设施。

## 3. 确定缺口的最小修正设计（本次不实施）

仅修PROD Core/SameThreadChannel.cs的event owner，**不能只在Set后随手加Dispose**：

- ReleaseResources内既有_messages锁下，在_destroyed终态设置后唤醒并确定性Dispose _messageEvent；保持幂等，且此关闭放在可能抛错的后续外部资源释放之前/可靠finally内，确保异常也关闭。
- WakeMessageWaiters同_messages锁内检查_destroyed，已销毁直接返回；因为PartitionScheduler.Run:188在Destroy完成后仍可能因partition.Failure调用它，直接Dispose不加guard会引入ObjectDisposedException。PostMessage已有destroyed guard；WaitForMessage的SafeHandle获取移入_messages锁下、先检查destroyed再取句柄/AddRef，避免关闭与获取竞态；保留finally DangerousRelease，使已在等待的borrower完成后由最后引用CloseHandle。
- 构造器330–336若后续初始化抛错，catch释放字段event后rethrow；不能等待无法成功交给owner的channel finalization。具体异常包围范围在实施时按实际构造代码保持最小。
- 不自加finalizer、不强制GC、不修改ABI/expected/阈值，不改scheduler/业务failure模型。新postfailure修正改变管理完成/回滚，但正常本路径没有失败注册证据，不能无证归因；其失败回滚还应走同一event owner修正。

需覆盖的单元/实际回归计划（另授权且先补可验证契约，不在本阶段执行）：沿现有PartitionSchedulerTests真实scheduler/channel风格，验证Destroy重复、成功/失败注册回滚、zombie Destroy后Wake不抛、Destroy前等待被唤醒并退回无效句柄、Destroy后Wait不访问已关handle、已AddRef等待结束后自有event handle关闭（Windows GetHandleInformation仅验证保留的handle，禁止探查复用后误判）；异常构造路径若无法稳定注入则如实标覆盖限制，不新增生产公开接口。通过现有公开ABI的正常A/B/duplicate/callback合同验证成功及postfailure/partition isolation保持。此修正是产品channel所有权修正，不是测试豁免。采样器若另发现缺陷，按已有Dispose语义修设施并保留产品资源门，不能把改采样器后数字下降自动判产品Green。

## 版本边界

x64 actual Native54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD、officialAD1C…，trend test6D4998…/MVID b134506e-23ca-4f47-b543-5e293682213b。来源报告 `completed-work/replacement-postfailure-source-rebuild-x64-closeout.md:20–31` 给三处修正的限定源码快照→发布→actual链，明确不是全仓加密冻结；原native/source目录本次实际不存在，现以continuous-media-default clean同SHA历史链。当前源码静态缺口不等于已逐字认证54BC62编译输入，更不等于从DLL直接证明该event是增长对象；本次未反编译/重建。

x86实际Native15A545…及test1F53D830…与x64不是同二进制/测试SHA；架构对照用于数量现象，不合并为一个构建回归。当前源码显式ProcessThread Dispose不能仅凭源码阅读倒推两旧test DLL每个实现字节一致；旧报告阶段A/趋势发布链提供有限支持。后续测试侧诊断改变test SHA/MVID，需记录新身份；不把旧full转绑新test。若授权产品event修正则Native必变，**Native0的诊断预算不适用生产修正**，必须单独授生产发布和重新验收。

## 4. 唯一下一门：x64同合同句柄类型证据（AwaitingAuthorization，不执行）

优先批准测试侧最小诊断，而不是在类型未知时先宣告真实OS事件泄漏。保持生产54BC62不变：normal build≤1、COM publish≤1（隐式编译另计）、沙箱≤1外900秒、Native0。无外部CLI/脚本/新Host/ETW/显示/微ABI。新增纯C# Windows API诊断放现有标准COM测试，原精确FQN与opt-in链不变。仅x64；x86不搭车。

固定official-first，official16轮成功才candidate16轮；各独立role同一PID、fresh A/B、4warmup+12measurement、65原samples（Initial+每轮四phase），轮20秒/role360/父840/外900不改；自然GC仅记录，禁GC.Collect/WaitForPendingFinalizers、禁追加预热/丢峰/改门槛。所有原HandleCount保留不扣减。类型快照为独立sidecar，紧随原样本，计时包含在原预算内，不增加业务轮或将失败轮排除。

最小可证伪实现方案：C# P/Invoke NtQueryInformationProcess(ProcessHandleInformation，记录实际支持状态)获取本PID句柄值/类型索引/GrantedAccess，在边界按类型计数；使用自进程伪句柄，不开跨进程/不启SeDebugPrivilege；对需解析类型的自有snapshot句柄DuplicateHandle到自进程、NtQueryObject(ObjectTypeInformation)只问类型不问名称，finally CloseHandle所有本诊断duplicate、finally释放非托管查询buffer。禁止关闭原被观测handle；duplicate失败/访问拒绝/不支持/竞态均原样Unknown不改为0；限制缓冲重试次数和大小在编辑前固定（建议至多3次、16MiB，超过即停止类型诊断而判Unknown）。枚举本PID无需提权；若权限/系统限制不支持，停止此门，不扩大到全系统内核指针或外部工具。类型解析只对少量代表自有句柄且有界，不执行可能阻塞的对象名称查询。记录原HandleCount、类型计数总和及时间差，诊断前后自有句柄数/清理状态；快照前后的非原子竞态如实披露，不将不一致强行配平。

证据通过标准不是memoryGreen：16轮全完成、原硬门不退化、身份/样本齐备、sidecar无自有句柄残留，明确跨轮增长主要属于Event/Thread/File/Section/其他何类；以原handle值跨相邻边界跟踪消失/新增，声明复用无法当永久identity。若Event增长接近2或4，仅提高候选，不从类型等同创建栈；后续再决定授权channel修正或必要的窄创建点诊断。已有句柄枚举不会自动提供历史创建栈，不承诺虚构栈。

停止标准：唯一build/publish失败即停；official失败不启动candidate；任一轮业务/cleanup/类型诊断越界、超时、进程异常、权限不支持或未回收都停止且Unknown/Failed，保留已生成全部峰值与未执行轮；不重试、不换full、不绕过opt-in、不在看到数据后调整阈值。若类型采集本身不能有界实现，应在build前停为Blocked，不能在一次沙箱内试多个探针方案。此门只有诊断授权，不隐含生产修正；产品确定性event关闭设计已给出但仍NotImplemented/NotRun。
