# IR 共享 geometry 合法提交合同：实跑终态，默认修复未交付

## IT 终态补记：默认源码安全恢复，IS 现场核证（无新增运行）

### 恢复与构建

先将 handoff 标记 IT 安全收口进行中，再只改两个测试源码文件：VisualCache 删除 IS 遗留的整个 asyncConsumer cleanup 分支（引用已撤 handles/unregister/flush/initialized/deinitialize/events/observedBitmaps/submissionFailed），原 finally 的 destroy/disconnect/bitmap/surface/device/window 顺序和原断言保留；SharedConsumerLifetime 新文件整体置于显式 IS_SHARED_CONSUMER_EXPERIMENT 条件，默认不编译 helper、不发现新增两个未验收实验，文件保留供审计。未 Ignore/skip 任何历史回归。隔离不代表显式实验已通过或已完成异步修复，不应启用该符号充作安全配置。

当前普通 surface 仍 connect(1)、原 Available callback、Event0/非 separate 原提交、原 target 字段与原内容断言；不是把 async 修好后保留。Historical 五 DataRow 与 officialGeometry/officialSampler 入口均保留，RunHistoricalCaptureControl 仍传 geometry||cache 和 official 命名参数。CompositionTests.Stage4Hardware 全 103 行核读，geometry/configuration/WIC/callback/transaction/multipleDevices 的位置参数与当前签名对应，未增加 async 参数也未改该文件。生产 COM/partition/fill/DRDS、NativeRun/ExitDiagnostics/公共捕获打包、项目和版本均未修改，未重发 Native。

正常分析器 default Release 构建已通过：ComAcceptance 项目 win-x64 Exit0，117 warnings/0 errors，2.99 秒；随后完整 WpfGfxShape.slnx Release（不指定 RID）Exit0，117 warnings/0 errors，1.97 秒。首次 solution+RID 请求为 NETSDK1134（解决方案不支持 RID），已改用项目级 RID 与无 RID 解决方案构建，并非源码故障。未 NoWarn/关闭分析器/规则。项目构建返回 63 行日志，原 stdout 92334 字符仅保留末 20000、前 72334 不可见；已读全可见日志，含 CA2014、CA2007、MSTEST0037/0044/0045/0017 等，非零警告清单不能冒称全量审计。均普通增量 build，非 clean/rebuild，也不是 x86/x64 运行矩阵或 Native 重发布；构建成功不绑定 IS 实验运行身份。本轮没有测试/full/过滤重跑、publish、新 sandbox 或执行器。

### IS 原件及计数层级

本地根 artifacts/is-shared-consumer/readback/x64/Results/is-x64；亲读 supervision.json 全39行、stdout全2行、stderr空、diag 的五个 candidate Passed 节点及 official 两入口、stages 的开头和最后相关边界。task0727659920f44b25807d75918bc50d46，监督7880/子6940，2026-10-25 UTC08:21:57.4812026—08:22:05.1765994；Mode filtered、内120秒，Reason exited，Exit -1073741819/0xC0000005，OutputCollected=true，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，SupervisorError=null。不是 timeout kill。stdout 只有框架/诊断路径，不含最终汇总。

精确预期 TRX：C:\CodingAgentSandbox\Tasks\0727659920f44b25807d75918bc50d46\Results\is-x64\TestResults\child.trx。本地回收 Results/is-x64 下无 TestResults/child.trx，diag 到380行 EOF、无完成会话或 TRX 产物节点；没有完成 child TRX，不能伪造整组最终 counters，也不能把 filtered 当 full。candidate 五个 DataRow 的 Passed 节点在 diag109/162/215/268/321，只有节点级五行成功；officialGeometry UID18e39062-2997-8a5c-bf02-39b275a17ace 在374行 Passed；officialSampler UID1c6be0d5-e7e9-8416-85f5-e4e394a6710c 在378行 InProgress，末380行 Executing，无 Passed/失败结果。新增 WhenSharedConsumer 两个独立测试无已完成证据，不填通过计数。

officialGeometry 的 stages453–505（其中长段返回截断已明确）可见 initial Commit0、image1/4/4完成事件 True；485–505 完整可见 delete/close/commit/flush/destroy/disconnect/deinitialize均0，bitmap0/1、surface0/1、device/factory refs0，cleanup failures0；518历史 consumer after。该成功仅该场景，不外推 Send 失败或 sampler。officialSampler519起 official=True geometry=False cache=False，原生前置和两 surface 已过，548 bind effect after，550 flags12/context-2/dpi1，553 initial Commit0 即 EOF；没有后续 event send/completion、帧断言、cleanup/refs或 consumer after。末日志只给可见边界，不能定位 AV 线程、出错指令或首次坏写。旧共享 Event0 的 E_HANDLE 未在这次合法异步 geometry 成功中复现不等于已修；本轮恢复 default 后未再运行危险同步场景。

### actual 身份（不是包 hash 替代）

监督实记 test SHA06587CD317704DC2DB7809A2189DED13BFB6B2C5594BA674F1CB26A327C59767、MVID476c92bb-2279-477a-8a28-61f8f56c2de0，X64/.NET10.0.13。Results/aot-identity.json 与独立 hw-module-Native-106a608711e3456f88820484b9d95583.json 全文核读：Actual 为 task\Native\wpfgfx_cor3.dll，SHA0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A，复用 IP Native。

两个 official hw-module-0727659920f44b25807d75918bc50d46-{1e7ffebf5ac74f01989cc8729974e3e5,4a67668e940242f085b6e99a8e02e6da}.json 全文核读：Actual 均 task 根 wpfgfx_cor3.dll，SHAAD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，10.0.13-servicing.26504.104/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。diag 的 candidate/official actual 也分别一致。不能因为同进程先运行 candidate 就将随后 official sampler AV 归因候选生产，不为复制该 AV 修改生产。

### 静态差异、ABI 与证据边界

IR 仅 officialGeometry 单独进程成功，未跑 officialSampler；candidate 各五行包含 sampler，不是 official sampler 通过。IS 同一 filtered 子进程增加 officialSampler 和新增独立回归范围，先五 candidate 再 officialGeometry，再 officialSampler 中断。scope、进程历史和资源变化是差异，不是预定根因。IR 原件后两行内容保留：x64 candidate diag257–267 的 cache 蓝场景首帧00FF0080两次→mutation0000FF80两次；317–320 的 zero 场景00000080四次；不能删失败或覆盖原证据。

真实分派是 Historical→RunVisualCacheConsumer→geometry 时 BindHardwareCacheGeometry，否则 BindTwoInputEffect；official sampler 的 geometry=False/cache=False 使用 VisualBrush，不是 geometry/cache brush 路径。现源码 Create ABI 使用 stdcall、Available(int,uint) 静态 UnmanagedCallersOnly 空回调，D3D9真实surface、97/43/39资源，ImagePacket0xA 转交 bitmap AddRef，PresentPacket0xB 布局 uint/uint/ulong；恢复 default Event0，隔离 helper 的异步事件为 separate1+duplicate+commit+wait+flush。空 callback 不证明 Available 覆盖；Send失败的引用及 duplicate 所有权与合法异步清理仍未闭。

核读 BindTwoInputEffect 472–535：资源33/34/82或83/38；0x6C bytecode、0x6D implicit brush、0x83 VisualBrush/0x84 cache、0x70 ShaderEffect、0x1D绑定。对照官方 wgx_commands.h720–795、generated_process_message.inl2715–2765、marshal_generated.cpp3560–3625/3815–3865 和 ShaderEffect.cpp70–150：ShaderEffect 固定头80，payload42；float register字节数2（不是2个寄存器）、float值16、sampler info16、sampler值8，总42；当前122字节包的80/82/98/114各区间与该声明相符。分派查 TYPE_SHADEREFFECT，ProcessUpdate 再解码 sampler handle、查 TYPE_BRUSH/RegisterNotifier；仅当前已读字段未发现可据此直接判定的非法长度，绝不因此宣称全 ABI/资源有效性已验收。VisualBrush字段、全部句柄生命周期、shader/硬件约束与 IS 构建前源码精确差分仍需独立核证。仓库文件工具未找到 IR/IS 源码 snapshot（artifacts 中只有包和日志）；未把当前恢复源码冒作当时二进制精确源码快照，未据末日志断言首次坏写。

### 下一唯一最小任务及终态

下一任务仅 officialSampler 独立隔离合同设计/静态核证：在现有 exe 的隔离角色边界，先逐字段核独立 ABI、VisualBrush/ShaderEffect命令与资源句柄引用有效性，再闭合法 async/event duplicate 的成功、Send失败、批次丢弃/提交失败责任与完成等待/销毁偏序；拿到最小合法官方 sampler 合同后才另行授权有界验证。不先改生产，不把 scope 差异当根因。本轮不运行该任务。若未来 geometry 可有限按场景保留而 sampler 未闭，必须先设计合理默认配置、共享路径分流与历史回归门，不得为避崩随意 skip/default禁用历史测试。

IT 最后补记：源码安全恢复和正常 Release 可编译已完成，IS 为官方 sampler 原生 AV 中断，异步修复未交付。原 IR/IQ 历史与所有 artifacts 保留；完整报告与共享最新新增节写后回读。Available/HWbackend/loss/mediaresources/full 四格仍开放，完整替换未完成。下文 IR 为历史，不覆盖当前 IT 边界。

## 裁决与历史

初始先登记报告/handoff进行中，然后读取规则27行、完整方案120行、latest IP/IQ、IP报告可见开头、HK/HV撤回原因、HD四格摘要及HB/DS关键合同、当前VisualCache和历史直接调用。长返回截断不冒全文审计，HD四格全部原件未重审。

本轮确实运行旧HistoricalGeometry UID18e39062-2997-8a5c-bf02-39b275a17ace，而非HB替代。显式IR_SHARED_GEOMETRY编译实验下，两架构official均原生前置通过、initial Commit0、首帧/mutation原内容断言通过、子Exit0。旧E_HANDLE和C0000374在两次定向实验中未出现；不是首次坏写根因证明，不证明full中断修复。

实现未完成逐项资源deletion/flush与最终refs、事件Send失败所有权及official sampler兄弟验收，不满足默认保留门。已逐项撤回12组共享修改，不整文件覆盖；最终默认仍原实现，不能声称已交付修复。实验编译包及整task readback保留，未新增独立回归源码是明确欠项。

## 合同映射与实验

普通surface分支采用initialize(0)/connect0，排除WIC/configuration/callback/transaction/multipleDevices/resize；这些SameThreadPresent错误返回分支不机械迁移。原D3D9 DISCARD/count1/32x16/Format0、native expected00563412、geometry/stroke/cache/sampler/shader及后态断言全部保留。

目标flags12、真实DPIcontext映射、GetDpiForWindow/96、setup close/commit/flush及invalidate；资源flags未完成逐字段核证。两initial及secondary mutation各独立EventWaitHandle+DuplicateHandle，separate send/commit、5秒wait/flush。原柄using管理；native已处理的副本由ProcessPresent signal/close。官方d3dimage.cpp250–288已读，Cleanup即使Present失败仍SetEvent/CloseHandle，但不能外推Send失败或丢弃批次已执行；实验注释曾过度声称batch负责失败清理，随源码撤回，不继承该结论。未在责任不明时补关doubleclose。

原Destroy/disconnect/surface/device/window清理保持，新增partition deinit；但未实现完整资源deletion/flush和refs断言，因此撤回。没有跳失败清理/吞异常/关闭并行/skip/fallback/改golden或阈值。

## 正式包及身份

正常依赖图Release自包含10.0.13，两次发布成功；分析器启用，无NoWarn。显式IR_SHARED_GEOMETRY，不新增默认测试发现。API离线核旧official/candidate方法存在。独有artifacts/ir-shared-geometry及x86子目录各obj/bin/publish/com-acceptance-package，显式PublishDir；只上传clean正式package，source/readback分离。正式目标收集Contracts/runtime/Native/reference；未独立核全量清单/实际总字节，不冒全量clean核证。

复用IP Native且package aot.sha256与actual candidate TRX一致：
- x64 0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A
- x86 63B91E6CD6B5C1E8D0095E1673933DD5BFC539744F7EC36DC06C8DCB074E3136

actual official独立task根模块：x64 AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1；x86 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834。版本10.0.13-servicing.26504.104/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；candidate为Native子目录。实际.NET10.0.13，runtimeconfig双framework10.0.13；本夹具未独立记录actual WPF SHA/MVID，不能补历史。

test x64 SHA8A2F9EED3E4BF52DFD2810DCE85C59B12645F011570DB5430F1811071889C427，MVID093a3130-1da1-4996-8633-f1e85f660f23；x86 SHAECF960F9FF7E1E032EC9EE196A0EE256A3CB5D2C21D6EBBCAE4B35AB3EE9EE03，MVID50bd482a-6058-44dc-bef0-1b6629e59632。

## 真实任务与监督

|架构/角色|task|监督PID/子PID|2026-10-25 UTC开始—结束|TRX|
|---|---|---|---|---|
|x64 official|3708ca561ad240cba9655995f9f02b59|9632/1160|08:04:51.1934479—08:04:52.3836809|1/1 Passed|
|x64 candidate|ed81c846413041eeb57b042951db4855|9416/7124|08:05:11.7767149—08:05:15.400298|5/5 Passed|
|x86 official|26ee60740457425d89a81135afddc5a2|3816/6308|08:06:07.3842116—08:06:09.1250466|1/1 Passed|
|x86 candidate|c9b69738a2ef4a269a503efa358f9169|6412/9208|08:06:25.0701196—08:06:29.4694426|5/5 Passed|

各一次，Modefiltered内120/外180秒；四份supervision39行全文亲读：Reasonexited/Exit0/HasExited和OutputCollected=true；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，SupervisorError=null。没有额外设施纠正/full/其它COM/FH。外工具未给独立数值Exit，不伪造。

绝对TRX为C:\CodingAgentSandbox\Tasks\<task>\Results\ir-<arch>-<role>\TestResults\child.trx；四份Counters Completed，失败/timeout/skip均0。official filter为旧WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain；candidate为原WhenHistoricalCaptureCandidateRunsThenExpectedUpdatesRemain五DataRow，实际每架构2geometry+3sampler/cache兄弟，不是五task。没有official sampler覆盖，不称完整兄弟四格通过。

## 原始内容与回读边界

两official TRX原生00563412实值相等，geometry0000FF00两帧相等、mutation00FF0000两帧相等。x64 official stages31–59全文已读：flags12/context-2/dpi1，Commit0，image1/4/4三事件True，DestroyAfter/disconnect/surface/device/window cleanup complete及historical consumer after。candidate两架构geometry两行及sampler首行内容已读：绿→蓝、蓝→绿；sampler0000FF80→00FF0080。其余cache/zero行部分长返回截断，不冒全部逐帧核证。

两candidate与x86 official全部事件/cleanup尚未逐行回读，不造15/3已核事件计数。最终refs未记录，不能报refs0。四份监督/统计/首场景actual已核，不以TRX绿补资源门；这是明确证据不足终态，不悬挂成待裁决成功。

## 撤回与下一唯一门

12组逐项逆向恢复VisualCache；Tests搜索IR_SHARED/IrPresent/asyncConsumer无残留。未整文件覆盖，未修改Historical/HB/DS/NativeRun/ExitDiagnostics/公共WindowQ/项目/版本或生产。HA/IA/ID/IH全部保留，Native未重发。未再跑恢复后的危险同步Event0。

最终默认normal Release/win-x64 build成功，117 warnings/0 errors，3.47秒，日志58–63已读，无关闭分析器；实验包不重新发布覆盖。

下一唯一门：直接基于本轮已进入consumer且内容通过的旧场景补足Send失败所有权、资源注册清单/逐项deletion/flush、最终refs与独立回归，再对最后源码做双架构适用兄弟验收；不得重复原生前置迁移撤回循环，也不得把本实验直接升级默认修复。本轮四task预算已用完。

Available/HWbackend/loss/media/resources/完整full四格/x64Event0独立线仍开放，完整替换未完成。
