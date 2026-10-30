# 非视觉异步 partition 失败共享/隔离门

## 终态：x86 两角色各1通过，有限普通错误隔离合同关闭

本次仅证据回填与文档收尾：源码/build/publish/沙箱/新增测试/终端脚本/再委派全部0，未运行任何程序。下方实施前冻结与旧零执行快照保留为历史，不代表当前终态。

### 亲读原件与非零测试裁决

证据根：`WpfGfxShape/artifacts/partition-failure-isolation/readback`。亲读 official/candidate 各自 `Results/partition-{official,candidate}/TestResults/child.trx` 全26行、`supervision.json` 全40行、`stdout.txt` 全12行与空 `stderr.txt`；两份 `PartitionFailureIsolation/{业务UID}/business.json` 各402行按1–85、86–190、191–300、301–402分段读完；两份 `Results/aot-identity.json` 全文。stdout中文解码有乱码，准确裁决取结构化TRX/业务/监督原件，不靠乱码摘要。未声称遍读全部Diagnostic。

|角色|task|TRX testId（定义/结果/Entry一致）|executionId|业务UID（不是TRX ID）|
|---|---|---|---|---|
|official|36da6d9b341a4a24b9ab381708b3ae12|1e3a533f-c51b-8855-b58a-2f84cc5b1ade|5af6cff4-9229-472e-9b71-52121c2b0e87|6430aec274d24af2be2969d31940fc7e|
|candidate|9541461d860440708814bc16017d25e6|18b58946-d629-8139-8233-224beea22724|94b72db1-3147-43aa-98a1-57a7ac399a11|6bd249584cde4249866e97a17b37fd3e|

TRX TestRun id分别为 `a4a00b39-84c1-486f-89a1-5b1768b2ed8d` / `aa5a475f-6f1c-424c-b7ad-40fa8208727e`。准确FQN为本报告冻结的Original方法/候选方法，均属 `WpfGfxShape.ComAcceptance.UceChannelTests`；两份Counters均 total/executed/passed=1/1/1，failed/error/timeout/aborted/notExecuted=0，结果Passed，非零测试。监督精确等值filter、filtered120，两者Reason=exited、业务Exit0，HasExited/OutputCollected=true；TimedOut/Cancelled/KillAttempted/KillReturned/两种drain超时均false，SupervisorError=null。外180为实施预算，不冒充监督原件字段或工具数值Exit。

已亲读正式partial第26–29行：业务UID由 `Guid.NewGuid().ToString("N")` 自生成并用于报告目录，不是MSTest testId、executionId或TestRun id。此前把6430…/6bd…列为业务UID是正确分类；任何把它们称TRX ID的表述均不采纳。

### 实际调用、失败传播与真实清理

两份业务顺序均42调用：Initialize/Connect0；A(source0)、S(sourceA)、B(source0)，三者visual39/完整SetAlpha32(16字节)/flush0；A bitmap95/flush0后一次CompleteNullSource12(12字节，x86=8+pointer4)。Order28/29 A/S原始HR均 **-2003303418 = 0x88980406**，未更换expected。B新合法更新alpha0.5及Order33 flush0；Order34/35真实销毁A/S均0；B再次新更新alpha0.25及Order39 flush0；Order40真实销毁B、41 disconnect、42 deinitialize均0。

official connection83050640，A/S/B=82983584/82985632/82984352；candidate connection1，A/S/B=2/3/4。created与destroyed逐项相同，不是未创建对象的清理占位；completed/cleaned/disconnected/deinitialized=true，primary=null、cleanupErrors=[]。两份siblingNegativeRejected/independentNegativeRejected=true仅为结果副本validator负控，不是产品故障注入，也不是额外TRX测试。

### actual身份：x86限定

共同test SHA256 **B9C7C936B22BB57FBFD50940FCF60DC84E30AE91258CD34B6BEF0920A69C5034**，MVID **2b624dfc-edd0-4c2b-b884-352fd56641ba**，.NET **10.0.13 / X86 / pointer4**；PID official2068/candidate7136。各test实际路径为各task根的 `WpfGfxShape.ComAcceptance.dll`，监督与业务SHA/MVID一致。clean runtimeconfig已亲读，两includedFrameworks均10.0.13。

- official actual/requested均 `C:\CodingAgentSandbox\Tasks\36da6d9b341a4a24b9ab381708b3ae12\wpfgfx_cor3.dll`，SHA **5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834**，version `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`，moduleHandle=actualBaseAddress=1854210048。
- candidate actual/requested均 `C:\CodingAgentSandbox\Tasks\9541461d860440708814bc16017d25e6\Native\wpfgfx_cor3.dll`，SHA **20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D**，version1.0.0.0，moduleHandle=actualBaseAddress=1832386560。

两份requestedEqualsActual=true；正式partial按模块handle匹配进程模块BaseAddress后读取actual路径/hash。初始化aot-identity两份均20B3，仅认证候选文件，不冒official实际角色。实际加载证据取各business.identity，不拿初始化hash替代。未把包内PresentationCore当actual WPF加载证明。

### 预算、账本与覆盖边界

本门实施账：正常COM build **1成功/133 warnings/0 errors**、self-contained publish **1成功**、沙箱 **2**（official1通过、candidate1通过）、Native发布 **0**；本次回填新增全0。build/publish数量采用已交付实施账，本次未取得或重跑其完整构建日志，不声称独立逐警告复核。运行通过则由以上原件独立核实。direct/public/WGC/KS保持下方历史独立账，不并表或冲销，Ledger不改。

关闭范围仅当前20B3对照5EBD的 **x86非视觉普通错误ABI传播/共享隔离/失败partition关闭后健康partition继续工作**。不是硬件故障/真实device loss或恢复，不证明Available=true、HW backend、consumer像素、media持续资源趋势、full或最终四格。x64本门未跑，不能由历史双架构17/17推本门双架构通过。显示线继续冻结；完整替换仍Open。

### 唯一下一门建议（未授权、未执行）

优先 **x64同一合同对称验证**，不重新发明门。已核现存 `artifacts/kn-inplace-resize/com-acceptance-package/Native/wpfgfx_cor3.dll` 文件存在，其同根 `aot.sha256` 为 **40D093988D2F5BA13B40D58C02FA744CE31E84DB0C7DD8FD24C66746335DCF38**。历史task `5ece82eddd464ed4bbca438cb1caca41` baseline identity亲读全24行，ActualNative为该task的Native路径且同40D0，test SHA6BB784E9AFDFFD3AEB125A25B2845C30DA299DCB828D722AF2F85105060B6FD0/MVID7473cd9b-8b02-40ee-b2c2-4fc50d2cfe8f/.NET10.0.13/X64；KN/KO报告各17/17仅历史有限resize/lifetime，不含本门。此为可信历史身份与当前路径候选，不是本次重算DLL hash；执行授权后必须先核当前文件字节/PE，不能仅信aot.sha256。旧共享包不能直接当包含新方法的包。

已只读核项目策略：TFM net10.0-windows、MSTest4.0.1；AotRuntimeIdentifier默认win-x64但必须显式绑定40D0 AotDllPath，不能沿用x86 Native20B3。ComAcceptanceSandbox=true的self-contained路径由PackageSelfContainedComAcceptance在Publish后清理独立包目录、按RuntimePackAsset及依赖白名单复制、缺件报错、写aot.sha256/package-files；不得改Package.targets或复用共享输出覆盖旧包。建议另授权Release/win-x64、两框架10.0.13的正常build≤1成功后publish≤1、新独立obj/bin/publish/clean，Native0、源码0；现有两准确FQN同逻辑，pointer8应自然记录bitmapPacketBytes16。先official精确1，实际合同/身份/清理全过再candidate精确1，总沙箱≤2，各filtered120/外180。x64官方实际SHA须由同架构原件独立读回，不预填x86的5EBD；40D0 actual/test新SHA/MVID亦须重新绑定。失败/身份不足/build或publish失败立即停，不重试、不降阈值、不关分析器；发现/full/显示/native/新门均0。

其后仍需关闭：当前身份真实consumer内容与原shader resize像素、Available准确状态/时序、实际consumer HW backend、真实loss/recovery及资源收敛、media持续消费/关闭趋势、最终产物托管及COM完整适用矩阵和x86候选/官方+x64候选/官方四格，以及打包退场残项。上述均非本次授权，不恢复direct/WGC/KS变体，旧1814/40与另4不相减推新full。

## 实施前冻结（历史：当时结果尚未产生）

授权范围：仅新增正式 `UceChannelTests.PartitionFailureIsolation.cs`、必要本地化资源和文档。已核 `UceChannelTests` 为 sealed unsafe partial，沿用准确 FQN：
- `WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives`
- `WpfGfxShape.ComAcceptance.UceChannelTests.WhenAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives`

两公开 API 同一逻辑，仅选择 OriginalDll/AotDll。默认 Release 编译，不增加 guard。生产、共享 Api、项目配置、依赖、SDK、Host、入口代码、Package.targets 零改。显示冻结，无 HWND/D3D/Present/GetPixel/native 显示前门，不重跑 KN17。

预定义业务顺序：Initialize(0) → connect(0) → A(source0)、S(sourceA)、B(source0) → 三通道各真实创建 visual39 并提交完整 SetAlpha(0x20,16字节) 合法更新/flush0 → A真实创建 bitmap95/flush0 → A一次完整 CompleteNullSource(0x0c,8+pointer 字节) → A/S flush 精确 0x88980406 → B新合法更新/flush0 → 关闭真实A/S → B再新合法更新/flush0 → 关闭B/disconnect/deinitialize。任一前置失败修测试，不换 expected 追绿。公开队列不要求 type6。

依据：官方 connectioncontext.cpp:585–646 决定共享/独立 composition；partitionmanager.cpp:507–547 普通错误映射 RENDERTHREADFAILURE；clientchannel.cpp:814–839 从 SyncFlush 暴露 zombie；connection.cpp:439–458 内部消费 type6；Generated/wgx_commands.cs SetAlpha offset0/4/8，已有 AsyncDrain 完整 null bitmap 包和 AsyncUceChannelTests 原始 88980406。错误输入不是 HW/loss 事件。

每次调用保存顺序、句柄/资源、原始HR；记录实际创建/销毁，不为未创建对象填成功。逐项清理即使前项失败仍继续，保存主异常；无 finally throw 或吞错。报告落盘先于合同断言。结果副本 validator 仅证明可识别 sibling 未失败、健康B被毒化，不改真实结果，不作为产品故障注入。

身份：请求路径与 module handle 匹配进程模块 BaseAddress 的 actual 完整路径/SHA/version；Requested==Actual；test SHA/MVID/runtime/架构/pointer/PID/运行UID。候选初始化文件校验与实际被加载角色分开记录。DLL驻留不卸载。

预算冻结：正常 COM Release/win-x86/net10.0-windows build≤1，成功后 self-contained10.0.13 publish≤1，明确 KP clean20B3 AotDllPath、新独立 clean；任一 build/publish失败立即停、不重试/抑分析器。核 clean 清单/根身份/官方候选角色/运行依赖后 official精确等值FQN≤1，只有真实合同+actual身份+cleanup全过才能 candidate≤1。同包独立进程，filtered120/外180。发现/Native发布/x64/full/显示均0。

## 历史隔离

public task03041eb56b8f4775a250759d0966e617（expected00563412/actual001E1D1C，actual官方身份不足）及独立1失败保留。direct build4(1败3成)/publish3/official3失败/candidate0；WGC build4(1败3成)/publish4(1败3成)/沙箱3(发现1真实2)/真实体2失败、退场default build1成功及通用包残项；KS build1/publish1/沙箱4/请求3/发现1/体1/A1/B1/native2；KN/KO历史双架构17/17有限resize/lifetime，均不并入本门。

## 当前状态

快照写入时 build/publish/沙箱/测试均0；实现待完成。完整替换未完成；本门不能关闭 HW、真实loss、媒体资源趋势或最终四格。
