# partition 隔离双架构通过后的下一有限门选择（仅文档）

## 裁决与边界

唯一推荐：**已确认异步 partition 失败后，以仍存活的 source 创建新的共享 channel：创建应成功；新 channel 的 SyncFlush 应报告既有 partition 的普通失败；关闭 source 后新 channel 仍可有界 flush 并正常关闭。** 简称 post-failure attach 门。此为源码支持的待验证合同，不是已取得的官方运行结果，也不是真实 device-loss 恢复。

本阶段生产/测试源码/配置修改、build、publish、Native、沙箱、测试、终端/脚本、再委派均0；只修改文档。未重算任何DLL字节或离线枚举新包方法，不产生新actual/TRX/通过证据。完整替换仍 **Open**，显示线冻结，不恢复direct/native显示/WGC/等待/线程捕获及取色变体。

## 已读依据与现有覆盖

已审完整替换方案§1–10、final-acceptance-remaining-matrix、current-execution-rules全文、恢复入口/事项/基线最新段、partition-failure-isolation与x64报告及现有正式partial关键场景。覆盖映射见consumer-wgc-retirement-and-coverage §5；其中“x64未测/下一门x64”已被最新x64报告取代，不重跑旧门。

已关闭的是：**失败发生前** A(source0)、S(sourceA)、B(source0) 全部创建且成功flush，之后A/S失败，既存健康B在关闭A/S前后继续更新。x86 actual20B3对照official5EBD，x64 actual40D0对照officialAD1C；两架构各official/candidate1通过。此处按已交付报告引用，不声称本阶段重新逐项亲读运行原件。

- x86：official task36da6d9b341a4a24b9ab381708b3ae12 / candidate9541461d860440708814bc16017d25e6，test B9C7C936…、MVID2b624dfc-edd0-4c2b-b884-352fd56641ba。
- x64：official44785ac366ea4919b4f7a14e46bd62ba / candidate59063052ff034988947ee651067be954，test B4A1D110…、MVID2254f62a-814f-482c-b8d7-d55429bf40fe。
- 各门原账build1成功133warnings/0errors、publish1成功、沙箱2、Native0保持。direct累计build4(1败3成)/publish3/official3失败/candidate0；public独立1失败、WGC退场及通用打包残项、KS、KN/KO历史17/17及Ledger全部独立保留，不冲销、不重算。

## 官方合同：为何不是凭空规定“失败后Create必须失败/成功”

以下路径均为仓库官方参考源码 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/`，不能把参考源码等同于当前加载的官方二进制，后者必须official-first实测。

1. `apifunc.cpp:228–259`：MilConnection_CreateChannel从**仍有效的**source对象取内部channel handle，交给指定connection；没有source zombie拒绝分支。合法前置是同一仍存活connection、同连接尚未Destroy的真实source、有效输出地址，同一调用线程串行操作。不探测跨connection、已销毁句柄、任意指针或并发关闭。
2. `connectioncontext.cpp:182–290,585–646`：source非零取得同一composition并AddRef；source0新建composition。OpenChannel构造server channel、登记表、提交 **PartitionCommandOpenChannel**；创建的HRESULT来自这条管理路径，不是业务批次的失败HRESULT。
3. **关键明确分流** `partitionmanager.cpp:345–394`：zombie只拒绝PartitionCommandBatch，所有其他命令继续排队，以维护partition的channel表。`crossthreadcomposition.cpp:324–341`明确zombie仅处理attach/detach；`composition.cpp:191–214`在不执行普通batch时仍处理Open/Close。因此在正常分配成功的合法前置下，既有失败不是CreateChannel返回失败的依据，源码预期Create为S_OK且非零真实输出；不是要求在OOM等环境条件下也强制成功。
4. `connection.cpp:159–200`先建server endpoint再建client channel，表内另持client引用；`clientchannel.cpp:48–110`client持connection引用。`composition.cpp:1913–1950`attach持server channel引用，`:1962–2020`detach移表并释放存储/GetAttachedChannel等引用。source参数用于选择composition，不是要求source client一直活着的parent所有权合同。关闭source后仍存活的新共享channel不应失效；不得借此推测具体公开COM ref数字。
5. 新channel不是“已收到过去一次广播”的旧S。`connection.cpp:306–375`的SyncFlush会发送同步控制并提交普通batch；`crossthreadcomposition.cpp:130–171`在zombie拒绝普通batch时向**该batch的channel**通知保存的失败并SignalFinishedFlush，保证新channel也能观察失败而不无限等待。`clientchannel.cpp:814–839`返回保存错误；`partitionmanager.cpp:507–547`普通错误映射0x88980406，OOM/OOVM另论。本门只复用一次完整null-source普通错误，不注入内存不足。
6. `connection.cpp:439–458`内部消费zombie及失败SyncFlushReply，不以公开Peek type6作为门。`clientchannel.cpp:772–801`Destroy忽略失败flush、移除endpoint并释放自身引用；正常Destroy返回0不证明全部内部内存立即回到基线。

### 候选实现的具体风险（未执行，不判已证缺陷）

候选 `Abi/UceChannelExports.cs:138–180`为新共享channel创建Register work，加入Channels后等待registration.Completion，仅result>=0才向调用方写输出；`Core/PartitionScheduler.cs:149–183`虽执行Register，但finally仍把partition既有Failure映射为completion失败。由此存在**注册已发生但Create返回88980406、output仍0**的可达静态风险，与官方管理命令/业务失败分流不同。这提供比再跑A/S/B更强的新判别力。不能据此提前宣布20B3/40D0二进制必然失败，也不能本轮改生产。

## 候选比较（最多3项）

|候选|现有覆盖/身份空缺|合法前置和判别力|选择|
|---|---|---|---|
|失败后接入共享channel及source关闭后存活|现有isolation只建于失败前；尚无20B3/40D0对5EBD/AD1C的late attach独立结果。候选Register completion存在具体静态风险|活source、同connection、一次已确认普通错误；区分管理命令被错误拒绝、late channel未继承失败、source销毁错误连带失效|**唯一推荐**|
|首个失败HRESULT跨批次/共享channel粘性|AsyncUceChannelTests:69–82已有单通道跨batch重复88980406；isolation已有跨既存S失败。旧单通道硬绑定候选，不是本身份官方新证据|后续普通错误会被zombie丢弃且都归一88980406，公开API无法据相同值区分“保存首错”或“覆盖成另一个普通错”；需OOM/OOVM才区分，但没有稳定合法触发依据|不设独立门，不增加错误种类/矩阵|
|最后失败channel关闭后，同connection新建健康partition及资源所有权|isolation的B早已存在，未覆盖失败partition全关闭后的新建；source0新composition有明确源码依据。TypedLifetime/AsyncDrain有WIC排空但不等于该次序及本身份新证据|先关所有失败channel、connection未disconnect，再source0新建/合法更新/资源释放；不能要求新handle数字不同或资源ID全局唯一。精确资源所有权须额外真实COM引用/回调合同，不能用destroy0替代泄漏趋势|次优，独立合法但本次不混入；late attach后不得自动顺延扩张|

## 唯一门：最小参数与可观察证据

**仅1组**：async connect(0)、同一connection、A与失败后新建L；无预建S、无B、无新健康partition、无线程/循环/随机/resize/媒体/OOM。不是把A/S/B换名：关键Create发生在A的失败SyncFlush已经返回之后，并在source销毁后再观察L。

1. InitializePartitionManager(0)；connect(0)；创建A(source0)。A真实visual39，完整SetAlpha0x20/16字节，alpha0.75，CloseBatch/Commit/SyncFlush均0；真实bitmap95创建并flush0。
2. 一次完整BitmapSource0x0c，Source=null（x86 12字节、x64 16字节），Send/CloseBatch/Commit按现有已证路径0，A.SyncFlush=原始-2003303418/0x88980406。先冻结此必要偏序，不用sleep或猜工作线程进度。
3. **此后** CreateChannel(connection,A,&L)，预期S_OK、输出非零且L不是A（只要求同时存活对象身份不相同，不要求整数单调）。L.SyncFlush应88980406；不需要向失败partition添加新业务资源或第二种错误。
4. Destroy真实A=0，记录从ownership表移除；A句柄从此不再使用。L再次SyncFlush=88980406，随后Destroy真实L=0。
5. disconnect=0，deinitialize=0；按实际取得的对象逐项清理，created/destroyed集合和各一次性释放记录一致。资源visual/bitmap由真实A持有，失败后不假称资源删除命令已执行；本门用channel真实关闭承担终结，不新增WIC/event-proxy资源以免混入另一个合同。

这些只证明公开ABI创建/失败传播/必要时序/有限关闭，不直接观测partition指针、内部AddRef数字、GPU资源、线程数或泄漏收敛，不证明HW/loss/媒体。

### 负控与异常清理

用同一业务判定器的**结果副本**做最小负控：①将Create成功/非零改为失败/零，必须拒绝；②将L首次或source关闭后flush改为0（参数化两副本），必须拒绝；③删除一条真实Destroy或把Create顺序移到A失败前，必须拒绝。原始业务结果不可被负控覆盖；负控不是额外TRX测试或产品故障注入。

正常路径只释放成功取得的A/L；任一断言失败保留主异常，finally仍逐项Destroy可持有对象、disconnect、deinitialize，记录独立cleanup错误。若candidate出现失败Create/output0的静态风险，**不得猜隐藏句柄、伪造Destroy0或宣称cleaned=true**；记失败创建返回值、输出、当前真实owned集合，清理已知对象并停止该独立进程。进程退出只提供隔离回收，不证明内部注册对象已正常销毁；有残余/未收敛则记清理不足并由另授权定位，不再在该进程追加操作追绿。

## 最小实施形状、现有包与预算（均待另授权）

优先只新增正式 `Tests/WpfGfxShape.ComAcceptance/UceChannelTests.PostFailureAttach.cs`，沿现有sealed unsafe partial、C#独立ABI声明、身份/业务JSON/异常保留和正常Release默认编译习惯；必要用户提示沿资源文件，不建新抽象/Host/入口/条件编译。不改既有isolation的场景和expected，不改共享Api、NativeRun、项目/包规则、SDK/TFM或依赖。**生产0改、Native发布0**。

拟新增准确FQN（尚未创建）：
- `WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure`
- `WpfGfxShape.ComAcceptance.UceChannelTests.WhenFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure`

现有正式partial源码只有已执行的isolation两入口，AsyncUceChannelTests的现有跨batch入口也不执行本新次序；已读包报告仅认证旧方法。没有可准确宣称已存在新入口的包或零构建依据；**须新增测试并新build/publish**，不冒离线发现已做过。

拟预算：先x86正常COM Release/net10.0-windows/win-x86 **build≤1**，成功后既有正式self-contained10.0.13 **publish≤1**，显式绑定当前x86 Native20B3路径/AotRuntimeIdentifier=win-x86，全新独立publish/clean；默认obj/bin可用，普通build不强制候选路径，不再制造能力阻塞。沿既有白名单/缺件检查，不借publish代替build、不绕分析器、不重试。Native/显示/full/KN17/独立发现0。

同新clean独立进程，official精确等值FQN≤1；只有实际合同、身份、完整原件和清理均通过才candidate精确≤1；filtered120秒/外180秒。build/publish/official或身份前置任一失败即停。x64另授权，不本次顺带跑，也不提前把x86结果移绑40D0。

每角色须回读TRX total/executed/passed、testId/executionId/TestRun id，业务自生UID另列；监督Exit/timeout/kill/drain，完整顺序/每次原始HRESULT、connection/A/L/source参数、资源ID、包大小、真实创建销毁/主异常及cleanupErrors。actual依据module handle匹配BaseAddress后的完整路径/SHA/version/requested==actual；test SHA/MVID、runtime/WPF实际可得版本、架构/pointer/PID及源码改动清单，候选初始化hash不替代official actual。历史20B3/40D0、5EBD/AD1C仅为候选来源/对照先例，当前字节和实际角色必须新认证，不预填。

## 推进/停止标准

- 本次选门完成即可停止，实施须另授权；不自动执行。
- 若official在合法前置下不满足源码推导，或实际身份/清理证据不足：候选0，记合同/参考版本待核，不改expected适配偶然返回，不换输入继续。
- 若official完整通过而candidate失败：记录新差异，下一阶段只能另授权定位此管理命令与失败传播边界；不批量增加错误场景，不用隐藏泄漏的绕过修复。
- 若x86双方完整通过：仅该架构有限合同关闭；有价值的下一步仅另授权x64同合同对称验证，不能宣告双架构通过。
- 双架构该门均闭后，**停止继续扩partition普通错误矩阵**。仅有新的、非冗余的公开可观察结果与合法源码合同、且对应当前身份空缺时，才考虑另选；第2候选当前不满足。优先转回完整方案更高价值的独立ABI引用/回调/资源序列或持续媒体等真实缺口；若可行合同/环境不足，明确未覆盖并停止，不重开冻结显示线凑进度。
- 真实consumer/resize内容、Available/HWbackend、实际loss/recovery、持续media资源趋势、打包残项及最终双架构适用full仍Open。不得用旧1814/40与另4相减，或把旧full/旧SHA补成新认证。
