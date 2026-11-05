# postfailure attach 最小修正：设计冻结与静态审核

## 编辑前合同（先于生产编辑落盘）

|路径|变更前|变更后要求|
|---|---|---|
|Scheduler Register|Register已执行，Completion仍映射partition.Failure|Completion仅返回Register自身结果；真实异常转原有RecordFailure HRESULT，不强制成功|
|Scheduler Destroy/Unregister|CommitSubmitted返回既有业务Failure；Completion再映射，ABI忽略Completion|仍排空/释放批次并真实Destroy；成功注销返回0，注销异常返回真实HR；不以业务Failure覆盖|
|业务Commit/SyncFlush|保留首Failure并映射zombie公开HR|保持不变；late L两次flush仍88980406，不清Failure|
|CreateChannel失败|先Channels.Add，等待失败后output0，未回滚|注册失败在worker销毁未返回channel；调用方移除Channels项；提交前失败在调用方销毁；断连且最后channel时释放connection持有scheduler引用|
|独立partition|独立Failure|保持隔离；不引入共享错误状态|
|公开合同与测试|official late create0/非零，candidate20B3失败|复用原正式PostFailureAttach及PartitionFailureIsolation，不改expected/skip/阈值；新增真实scheduler单元，不拿结果副本充回归|

回撤边界：仅本报告登记的生产三处（Core/PartitionScheduler.cs、Abi/UceChannelExports.cs、必要时Core/SameThreadChannel.cs）、新增定向单元及导航状态文档。任何回撤仅撤本轮差分，不覆盖既有partial/资源、KN resize、用户其它修改；不触碰历史红原件、artifacts、Ledger、包规则、项目/SDK/依赖、显示代码。生产编辑后Native身份必变，旧20B3不能认证新源码。

## 工作步骤与状态

1. 阅读入口、规则、完整方案、原件与所有权：StaticConfirmed，已完成。
2. 最小生产修改：已实施，尚未编译/运行。
3. 定向真实调度单元及既有公开合同静态审查：已新增4个单元并复用4个公开角色入口；NotRun。
4. 静态回读、报告、入口/事项/plan更新：已完成；不是运行验收通过。
5. 执行验证：NotRun，本阶段build/test/publish/Native/沙箱预算全部0；须另授权。

## 原件与根因亲读

本轮本人只用文件工具，无终端、脚本、再委派；build/test/publish/Native/沙箱均0。工作区结构查询失败后读取实际目录与项目，不假定Native子项目。已读恢复入口、执行规则、当前基线/事项、连续性规则及完整方案1–120行；SDK10.0.107、生产/单元net10.0、nullable启用，项目与版本均未改。

亲读原件（不是引用报告代替原件）：
- `artifacts/postfailure-attach/readback/official/Results/postfailure-official/PostFailureAttach/4a99f2de81fa4216bf9fdf446f257d22/business.json` 的1–46、135–267行，task7e9e0e363f4547fbbd2e30cd73090d3e，actual5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；Order16 A失败，17 Create0/81762944，18与20 L均88980406，19关闭A0，21关闭L0；cleaned=true。
- `artifacts/postfailure-attach/readback/candidate/Results/postfailure-candidate/PostFailureAttach/77900378d8e646d69c8c221994d24cf0/business.json` 的1–44、135–221行，taskc9f330bdff8a479297b58a460383cf7c，actual20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D；Order17 Create88980406/output0，unknownRegistration=true、cleaned=false。已知对象清理成功不证明内部未知注册正常释放。

未声称重读全部TRX/监督或business中间setup；完整旧报告与全部原件保留。本轮没有新实际身份、TRX或哈希。

官方 `partitionmanager.cpp:345–400` 明确zombie只拒绝PartitionCommandBatch，管理命令仍入队；`crossthreadcomposition.cpp:125–180` 说明业务flush保存zombie错误，Open/Close不依靠业务channel指针。候选Run先执行Register，finally却无条件按partition.Failure映射Completion，CreateChannel先入全局表再等待；与实测一致，是普通partition管理ABI缺陷，不是显示或真实device-loss。

## 实施文件及合同

- `Code/WpfGfxShape/Core/PartitionScheduler.cs`：Register/Destroy管理completion采用本次执行result；业务completion仍首Failure/原zombie映射；Register失败在worker调用Destroy回滚，回滚异常精确分类记录，不替换主失败HR。Destroy仍消费批次释放transport引用，但不把CommitSubmitted返回的业务Failure当注销失败。
- `Code/WpfGfxShape/Abi/UceChannelExports.cs`：先保留表项再Submit，避免排队后再发布表项失败。未被接受的注册由调用线程销毁并移除表项；异步真实注册失败由worker先销毁，再由调用线程移除表项；若期间Disconnect且最后channel则释放连接持有的scheduler引用。output仅在成功后写入。Destroy等待路径返回completion自身HR，不再丢弃真实注销失败。
- `Code/WpfGfxShape/Core/SameThreadChannel.cs`：Register检查TryAdd结果，不将未插入当成功；不清Failure、不新增架构/接口。
- `Tests/WpfGfxShape.Tests/PartitionSchedulerTests.cs`：4个有公开XML注释的MSTest，真实MTA scheduler/真实router/真实channel，完整未知wire command触发业务Failure；没有直接赋值Failure、mock或结果副本。覆盖late register成功、flush保错、source关闭后共享表项仍在且flush保错、独立partition关闭后健康。复用既有friend关系，未扩大生产可见性或新加InternalsVisibleTo。

公开相邻ABI精确清单：`WgxConnection_Create`、`WgxConnection_Disconnect`、`WgxConnection_SameThreadPresent`、`MilConnection_CreateChannel`、`MilConnection_DestroyChannel`、`MilChannel_CloseBatch`、`MilChannel_CommitChannel`、`MilComposition_SyncFlush`、`MilComposition_WaitForNextMessage`、`MilComposition_PeekNextMessage`、`MilChannel_GetMarshalType`、`MilChannel_SetReceiveBroadcastMessages`、`MilChannel_SetNotificationWindow`、`MilCompositionEngine_InitializePartitionManager`、`MilCompositionEngine_UpdateSchedulerSettings`、`MilCompositionEngine_DeinitializePartitionManager`。候选无独立Unregister导出，等效为Destroy→ReleaseResources→Partition.Channels.Remove。只有Register工作与Destroy工作是此completion缺陷的管理兄弟；Commit/SyncFlush/Wait提交业务工作应保留Failure；Peek、通知窗口、广播、marshal直接处理自身管理数据，未套用此completion。连接断开/manager引用不经partition failure映射，未顺手修改。

## 有限边界与未证所有权风险

真实注册失败没有合法可确定的公开触发点：未捏造OOM、反射改表、重复内部Register或生产failure注入，也未将这些当公开回归。失败HR传播、worker回滚、全局表移除、断连引用释放目前仅静态支持，尚无原生实证。回滚失败会记录异常并保持主失败，不宣称其资源闭合；既有诊断文件I/O失败、极端构造中途OOM、外部COM释放重入/抛错、跨线程生命周期竞争尚未证明全部收敛。

Destroy在composition线程重入时沿既有合同不等待自身队列，返回的是接受关闭工作而非已完成注销；本轮没有把它说成已验证真实执行HR，此分支仍需独立证据，不能据普通外部同步关闭用例外推。此限制不改变已证postfailure外部调用合同，未扩大重入架构修复范围。

单元只补充验证真实调度实现，不能替代原生官方/候选差分。WaitAsync5秒有界观测不证明Dispose.Join永不阻塞，正式外层监督仍需120/180秒。未新增用户字符串/资源，未改公开测试expected、skip、阈值。KN resize、显示实现、项目/包/依赖/SDK、历史红报告及所有Ledger保持原样。

## 下一唯一执行计划（推荐，未执行/须外层另授权）

仅x86 Release，不做full/x64/显示/KN17：
1. 正常build受影响生产/Tests/ComAcceptance（各目标至多1次，不借publish替代build；失败即停），保留所有分析器结果。
2. 定向运行 `WpfGfxShape.Tests.PartitionSchedulerTests` 4项各1次；不以mock补运行，失败即停。
3. 从最后源码NativeAOT发布 `Code/WpfGfxShape/WpfGfxShape.csproj` win-x86 Release≤1，独立新目录，记录新Native SHA；**Native哈希必将变化，不得沿20B3假认证**。
4. 正常COM self-contained win-x86发布≤1，显式AotDllPath绑定新Native、AotRuntimeIdentifier=win-x86；新独立publish/clean，保留既有白名单/缺件/manifest/根摘要规则。核runtime、PE/架构、test SHA/MVID。
5. 精确FQN `WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure` 官方≤1；合同、actual身份、全部清理通过才运行 `WpfGfxShape.ComAcceptance.UceChannelTests.WhenFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure` 新candidate≤1。原合同Create0/非零、L两次88980406、真实A/L destroy及清理，120/180，失败即停不修后重试。
6. 通过后仅x86 isolation有限回归：`WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives` 官方≤1，通过才 `WpfGfxShape.ComAcceptance.UceChannelTests.WhenAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives` candidate≤1；同新clean、精确等值filter、120/180，读取原始business/身份/TRX/监督及正常清理闭合。
7. 分账新build/test/Native/COM/official/candidate并审阅原件，保存失败即停止。最多4个沙箱角色体，不加发现/重试/额外错误矩阵。此有限门通过也不关闭完整替换。

历史isolation x86(20B3/5EBD)、x64(40D0/AD1C)只归旧产物；direct累计build4(1败3成)/publish3/official3失败/candidate0、public独立失败、WGC退场/包残项、KS、KN/KO17/17、Ledger及历史postfailure build1/publish1/沙箱2均不冲销、不冒新源码认证。完整替换Open，真实HW/loss/media/full与显示线状态不变。

