# Register回滚公开ABI有界审查与下一有限门（仅文档）

## 当前裁决

本阶段源码/项目/测试/build/test/publish/Native/沙箱/终端/脚本/再委派新增全部0；只读代码、既有报告并写文档。未新增运行证据，未重算SHA，未亲读本轮所引历史全部TRX/business，历史数字按具名收尾报告引用。审查完成，停止Register异常门，不构造注入器。

未找到合法、确定、不依赖资源耗尽/私有状态注入/失效指针的公开ABI调用序列，可以使一个真正提交的首次Register失败并可观察其异常回滚。结论是本次有界源码调查未发现入口，不是证明任意环境下不可失败。OOM、提交前构造/表项异常、真正注册异常及其全部所有权回滚继续未证。

重要纠正：composition线程重入Destroy不是“没有真实callback路径”。现存AsyncDrain已有真实stream→WIC→wrapper→bitmap→最终释放callback→Destroy sibling路径；但官方分支刻意不做该重入，新SHA仅两个有限合同没有运行这条路径，不能拿旧候选通过充新候选或官方等价。没有新增与官方明确保证相符的确定重入完成合同，本轮不新增hook、不扩重入变体。

唯一下一建议：另授权**健康异步共享partition内真实bitmap source的跨channel DuplicateHandle、source关闭后目标继续合法更新、目标最终释放所有权**有限门。不是继续partition普通错误矩阵，也不是回放postfailure/isolation。最多两个候选的取舍见下文；本阶段不实施。

## 当前身份与不可迁移覆盖

- x86 actual `15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`；x64 actual `54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD`。来源及日志截断边界分别见source-rebuild-x86/x64-closeout报告。
- 两架构当前只绑定postfailure attach/isolation两个有限ABI合同、真实正常清理与限定来源链；不是完整资源/时序验收。旧20B3真实失败、40D0隔离、KN17在20B3/40D0的有限resize/lifetime各自保留，不能转绑。
- 旧AsyncDrain有限验收：`async-drain-owning-finite-acceptance.md`记录57CE56…/win-x64/.NET10.0.1候选12/12，系统官方94C064…正常6/6；不等于当前5EBD/AD1C及15A545/54BC62。
- `replacement-supervised-notification-close-contract.md`的HU为75D136…/8F7458…对5EBD/AD1C，正常6/7/8、外部stream跨close、close后caller释放且最终恰一次；不是跨channel DuplicateHandle所有权或真实worker重入等价。
- `replacement-supervised-notification-unregister-race.md`的HW/HX为上述旧Native身份、四角色有限注销合同；类型5差异后续有HY修正历史。旧HW guard仍存在，不把重开旧交错当当前新覆盖。
- Available正常销毁回调，包括KN的正常失效，不能等同设备真的丢失、HW消费或恢复。显示、real-loss、media、最终full均Open；不以旧full相减。

## 任务一：注册失败入口判定与精确源码

以下官方定位为工作区Microsoft.DotNet.Wpf参考源码，不声称已安装DLL与其每行字节相等。路径前缀O=`src/Microsoft.DotNet.Wpf/src/WpfGfx/`，C=`WpfGfxShape/Code/WpfGfxShape/`，T=`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/`。

### 真正注册与异常所有权

- C`Abi/UceChannelExports.cs:138–210`：验证connection/source在前；分配新identity、构造新SameThreadChannel、全局Channels.Add后才Register或Submit。未接受时调用方finally移表/Destroy；已接受时等待Completion，失败后移表；Disconnected且最后channel才Release manager引用。output仅成功写出。
- C`Core/SameThreadChannel.cs:153,330–344`：partition内checked递增AllocateChannel；Register只有Channels.TryAdd与IsRegistered=true。C`Core/GeneratedProtocol.cs:774–782`只拒绝零/重复key及null表；真实构造传非null表，递增ID不复用。耗尽后checked溢出发生于构造/提交前，不是Register失败。
- C`Core/PartitionScheduler.cs:145–196`：Register异常被分类记录；只有首次尚未注册对象回滚Destroy；管理completion不映射既有业务Failure。重复注册既有对象内部Work可失败，但公开Create每次产生新对象，不能通过公开ABI重复提交同一对象Register。
- C`Core/SameThreadChannel.cs:618–669`：Destroy/ReleaseResources注销通知与present/vblank、清消息、仅IsRegistered时移除所拥有表项、释放排队包及资源。此为静态所有权解释，不是异常运行覆盖。第一验证报告已明确未包含构造中途OOM、诊断I/O、COM Release异常的全部闭包。
- O`core/uce/connection.cpp:158–202,214–250`：Open成功后客户端Create失败会Close server endpoint，客户端失败销毁handle。O`core/uce/connectioncontext.cpp:194–280`：Open管理batch分配、GetOwningComposition、AssignChannelInTable、SubmitBatch及失败retry Close；源码自己披露回滚健壮性限制。因此不能从官方有清理代码推出合法确定公开故障入口，更不能预设所有环境错误完全相同。

|边界|合法性/官方依据|候选到达位置与裁决|
|---|---|---|
|活source，同connection，新channel|O include/exports.cs:393–418明确同partition；connectioncontext.cpp:585–646按当前连接表找source|正常Register；业务Failure不等于注册失败，当前postfailure已覆盖，不再跑|
|source跨connection|apifunc.cpp:242–252把source对象转成其内部channel数值；GetOwningComposition在目标connection自己的表查询。未见跨connection共享保证；局部ID还可能碰巧命中目标表，不能以“真实活对象”推导合法共享|C:150–152明确ReferenceEquals检查，提交前E_INVALIDARG；不算Register回滚，不运行猜合同|
|已Destroy source/重复Destroy/再次Disconnect或用旧connection创建|O apifunc.cpp:210–276直接把handle当对象指针并解引用/Release，无统一安全句柄表合同；已释放指针可悬空，不作为安全触发|候选表检查失败在注册前；不得以E_INVALIDARG冒真正回滚|
|同线程重入Create|Register没有外部callback；同步Create持Gate、分配新对象，异步Create等待但不是消息pump合同|没有确定同对象重注册路径。由任意hook调用不能充公开ABI|
|DuplicateHandle|O clientchannel.cpp:174–242明确同connection/partition、源提交先于目标使用；操作的是资源handle而非channel注册|不能制造Register duplicate key；跨partition错误是资源操作校验，不是注册失败|
|Disconnect仍有活channel|O apifunc.cpp:210–225只Release连接引用；现存AsyncDrain断连后继续flush/drain；不应冒取消|候选保持scheduler到最后channel；断连前后有效对象可继续用，但断连旧入口再Create不是新合法注册异常门|
|竞争断连/停止scheduler|候选创建校验/Submit持Gate，连接引用及活channel维持manager；Submit停止检查不是确定公开健康序列|没有合法确定能令首次Register本体失败的安排；不增加竞态搜索/线程变体|
|OOM/ID耗尽|资源分配真实可能失败，但耗尽/分配时机不可确定，且ID溢出在Register之前|保留未证，不资源耗尽、不私有注入、不做无效指针实验|

### composition线程重入Destroy：真实路径与未证界限

T`UceChannelTests.AsyncDrain.cs:140–170,190–240`使用正式MILCreateStreamFromStreamDescriptor构造callback stream，经系统WIC frame/CWIC wrapper/Bitmap95持有；最终dispose callback中，候选非retainExternal分支真实调用Destroy(sibling)。记录releaseThread/reentry/处置次数，部分正常与directClose状态断言非caller线程，deinitialize后轮询该OS线程消失。不是内部mock或任意scheduler hook。

但同文件官方分支明确不在callback中Destroy；另由caller显式Destroy sibling。故旧报告“候选reentry=0”是旧身份下接受调用与最终有限收敛，不是官方同一重入保证；HU retainExternal分支也不重入，其callback发生在caller externalRelease，不能拿来证明composition线程。

T`UceChannelTests.cs:395–473`另有EventProxy dispose callback重入，但connect(1)为同步，不能冒composition worker。其generic target的对象用途也不能自动当独立官方异步fixture。C`Core/SameThreadChannel.cs:269–276`的通知只PostMessageW到窗口，窗口hook在owner线程不是composition线程直接callback。

C`Abi/UceChannelExports.cs:215–249`在scheduler.IsCurrentThread时不等待自己队列，返回0只证明接受；C`Core/PartitionScheduler.cs:145–196,200–211`后续真实清理与Dispose不self-join。要证明回收仍须实际callback退出、后续Destroy执行、最终处置/worker退出等证据，而非当场HR0或进程退出。当前新SHA没有这条实证；本轮不伪造真实ABI执行覆盖。

## 任务二：有界选门（仅两个候选）

### 候选1／唯一推荐：健康异步跨channel资源共享与关闭所有权

官方明确保证见O`core/uce/clientchannel.cpp:174–195`：同connection/partition，A创建资源→Duplicate到B→先提交A→B才使用。已补读O`core/uce/composition.cpp:1521–1552`按AttachedChannel路由；`core/uce/htslave.cpp:313–359`把同一resource指针复制到目标表并AddRef，明确支持目标独立持有，不只是两个客户端数值。T`UceChannelTests.TypedLifetime.cs:77–109`已有同步真实bitmap共享后source关闭不dispose、最后target关闭dispose一次的同类观测。对象活性由真实WIC source及其外部stream最终dispose观测，不依赖桌面、HW或像素。

现有测试并非空白：T`UceChannelTests.cs:93–129`已有同步connect(1)的source关闭/connection断连后duplicate继续用及最终Release.deleted=1；T`UceChannelTests.TypedLifetime.cs`已有同步真实bitmap transport/shared分支；T`UceChannelTests.AsyncDrain.cs`已有异步sibling与排空，但没有DuplicateHandle跨channel拥有关系。不是发明新契约，而是把现有保证交叉到本次刚改过的异步Register/Destroy管理路径；本次新Native实际只postfailure/isolation通过，没有该交叉的绑定证据。

建议另授权新增一个正式partial的两个薄角色入口，共同一场景。建议名（当前不存在，不冒现成可执行filter）：
- `WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalAsyncSharedBitmapOutlivesSourceThenFinalCloseReleasesOwnership`
- `WpfGfxShape.ComAcceptance.UceChannelTests.WhenAsyncSharedBitmapOutlivesSourceThenFinalCloseReleasesOwnership`

固定单一正常场景：显式manager(0)→connect(0)→A/source0与B/sourceA→真实2×1 BMP callback stream/系统WIC frame/wrapper→A资源95传入真实source，同时A创建Visual39。A的Bitmap95与Visual39都duplicate到B，Close/Commit/SyncFlush A成功后，释放全部caller wrapper/frame/stream引用，关闭A；此时dispose必须仍0，B的duplicate visual合法alpha更新并flush0；释放B两个duplicate（每个客户端最后Release.deleted=1），Close/Commit/Flush B，再Destroy B、Disconnect、Deinitialize，按既有最终清理完成窗口观察callback恰1且已退出。中间不要求bitmap未使用就立即处理，也不要求任何单次COM Release返回0；必须保留before/after处置、真实callback进入退出及全部API结果。仅一组async健康输入，不引入失败partition、resize、Present、媒体、窗口/取色或新等待变体。等待只复用既有5秒最终完成上限，不能加长追绿。

真正新增判别力：若source关闭错误释放共享资源，dispose会提前；若注销连带损坏共享表，B更新/flush失败；若duplicate最后引用漏释放，最终callback缺失；若资源重复释放，callback次数/退出与清理账不符。callback指的是stream真实最终释放，不是Available/device-loss。结果副本负控仅提前dispose/缺失最终dispose两种，可同进程判定器自检，不能向生产注入。

### 候选2／不选：通知注销与回调偏序

HW/HX旧四角色已覆盖合法在途唤醒可以完成，HU覆盖最终关闭偏序，HY有初态/注销/重注册payload。当前新SHA确有未绑定切片，但再跑guarded旧HW交错只是受影响有限回归，不是新的注销保证；无依据要求“注销返回后绝无先前在途callback”。本次不加窗口/线程/等待变体，不以Available析构回调冒真实loss。其优先级低于候选1的明确Duplicate提交次序与真实资源所有权交叉。

若实施前发现同一新SHA已有完整相同交叉证据，停止新增该门，不换名另造；改为**现有受影响有限回归**：AsyncDrain正常directClose queueState6（open）与8（committed），official/candidate各两case，覆盖本次Destroy排空两端；这只是备选处置，不与唯一门并跑。现有公开方法分别为WhenOriginalAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates / WhenAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates；不要用FQN整方法误跑6/12行，不猜MSTest参数过滤可用性。若正式包不能精确选6/8，另授权两个薄角色入口复用RunAsyncWicDrain，不扩全组；原候选callback重入仍只能按原候选范围记录，不能补官方等价。当前不需要走该备选，因未找到新SHA上述共享交叉证据。

## 下一门预算、前置与停止条件（建议，非执行授权）

- 先x86 Release；生产/共享Api/项目/依赖/包规则0改，Native publish0，复用已来源闭合的15A545完整值，不因测试/文档修改重发Native。正常COM build≤1、COM publish≤1；新的正式独立clean、显式Native路径/架构及runtime记录。
- official≤1进程/1case，成功且身份/对象拥有链/正常清理齐全才candidate≤1进程/1case；各内部120秒、外180秒，沙箱≤2，无重试、无发现/full/KN17/显示。测试partial可另授权增加，两角色同测试SHA/MVID/.NET与pointer4；actual官方5EBD完整身份、candidate15A545实际加载必须回读，不能用初始化候选身份替代official。
- 实施前复核已定位的Bitmap95 server Duplicate AddRef与最终释放链、真实caller引用归零时点，采用上文冻结Visual39+Bitmap95方案；本报告已读server Duplicate核心，不冒完整bitmap/WIC所有权闭包或已安装二进制认证。若无法确认同资源真实拥有关系/早释放可检测，停止回报，不偷偷替换成Visual-only弱门。
- official失败、没有真实callback、前置WIC/路径/ABI不满足或实际身份错、参数化filter扩大、超时/清理未知：立即停止，不跑candidate、不随机变参、不改阈值；双方失败不通过。candidate失败保存原件停，不重试、不自动生产修正/重发。
- 成功只新增该SHA的一个健康异步共享资源关闭合同；不证明真实Register失败回滚、OOM/QI/构造中途异常、composition重入等价、长期资源趋势、所有通知、跨connection共享、显示/resize/real-loss/media/full、x64或全部二进制兼容。成功后不继续衍生共享/错误矩阵；x64同一有限门必须另授权。

## 文档状态

本报告及恢复入口、当前事项、方案状态、完整方案和最终矩阵顶部增加当前审查裁决；历史全文/账本保留。当前基线数字不变。Register异常门状态为有界审查完成/停止构造，异常覆盖未证；完整替换Open。
