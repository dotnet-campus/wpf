# IA：最小 partition 注册状态快照修复

## IB最终裁决：保留有限初态修复（不是完整状态机验收）

IB已实际独立审阅源码及四个既有任务核心原件，不重跑、不发布、不重建；本轮只更新本报告和三共享文档。最终保留 IA 的 SameThreadChannel.cs 最小生产改动，未作撤回，未增加生产算法。下方“进行中/正在回读”为 IA 历史。

### 状态来源、设计边界与保留依据
- 官方 `composition.cpp:45–47` 构造明确 `m_deviceState(NoDevice)`；1230–1280 注册读取同一 partition 当前字段写 old/new，并经 NotifyHelper 通知；2225–2274 状态变更另处理 Normal/NoDevice/Occluded，lost/released关联缓存失效，occluded关联调度。当前仓库官方源码是设计证据，不冒充安装 servicing 逐行同源。
- 当前 SameThreadPartition 的私有 enum 只有 NoDevice=1，字段初始化为该值；注册分配清零64字节，type=5，offset8/12读取该字段，余字节零。不是在通用 PostMessage 中硬塞1/1；字段有 partition 对象归属，但**只有构造赋值，运行行为等价固定初值，没有权威聚合状态更新机制**，不能粉饰为真实动态状态机。
- Compose/GeneratedTargetRegistry 仍无状态联动；Registry53行核读无新增回调。IA未改 target聚合/逐target HRESULT映射、广播、锁队列、唤醒、注销。初态缺陷和后续状态缺口必须分开：HY/HZ撤回的逐target映射及广播不复活；本次对充分实证的未渲染构造初态作局部纠正有价值，不能以未来状态机尚未完成无限撤回正确初态。
- 明确未修：成功渲染后注册仍会报告NoDevice，**不得作为正确行为验收**；状态转换、合法变更old/new、所有监听者广播、有效target/非零SyncModeStatus均未闭。这里保留的是有限修复，不是关闭设备状态功能。

### 四任务和实际身份（均为IA已执行，IB新运行0）
|范围|task|结果|
|---|---|---|
|x64 payload|a1185e06037545b6948e188b5893dbfc|父2/2；official/candidate/official-repeat各1/1，三子Exit0|
|x86 payload|40fb4374bcd048318cc50162775821ee|父2/2；三角色各1/1，三子Exit0|
|x64 regression|25d762d8c59544e0b3a6feed6d25cae0|30执行30通过，子Exit0|
|x86 regression|849373c9aad24379a8f07e3189a2b7dd|30执行30通过，子Exit0|

- actual new Native SHA x64 `531E096706C3E3AC4E9AE84C6A0158354382FC2B97A9018CF689B64289B09913`，x86 `21B396AE6701CFD0E7D036B63ABE8308EDD5555874582E194D5930EA765EDD60`；payload business与regression HW business实际加载路径身份一致，不拿包预期代替实际消费。
- payload test x64 SHA `0D8E01B8EF4F91E340A2BF70B24D546BE9ADF6CE781905AC327B59ADF3EFB774` / MVID `16f7b1bd-c4d5-4306-a0d3-b4761640f8fb`；x86 SHA `F18BE1D6512F83D7782D3DF98109B282A88DC1DB1B91442661EBB6EE139F76AC` / MVID `3089260a-1c09-4425-b89e-4e03537a7e95`。
- guard regression test x64 SHA `08A0586A0ACE5EBDD6F98DACFE296A364E9AB244D4ABAA119E0B47FD11A6DD6C` / MVID `72d823a8-07b9-4d5d-9078-c88eac73e84c`；x86 SHA `FC03C1D4E492B9B9F202622307A241B24576375FB1039B5E87DFE4C407E69A2B` / MVID `216a6f68-1af3-4a99-8e4e-f87284a84afe`。guard包与默认包test身份不同，不混称同一默认配置。
- 实际runtime均.NET10.0.13，对应X64/8与X86/4。官方payload x64 SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`；版本10,0,1326,50504 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。官方repeat独立PID：x64 official5200/repeat5476/candidate1696；x86 official7972/repeat5504/candidate3052。

### payload、旧红和回归结论
- 两父TRX32行直读，六role Counters各1执行1过、failed/error/timeout/aborted/notExecuted全0；父无Failed或RunInfo错误。六角色business137行的内容/生命周期已读：两完整64字节均 type5/reserved0/old1/new1，其余零，精确counts[1,0,0,1,0]，所有HRESULT0，destroy/disconnect/deinitialize0、cleaned=true。64是独立结构/保存长度，不虚称peek返回长度。
- 默认无role不空返回：源码默认candidate，payload父第二项真实执行，direct business x64 PID1364/x86 PID7436实际SHA同上、两条消息及counts均正确。独立ABI结构与全64字节预期不复用候选类型。
- 旧红task7389a0c29c444cc18e9d6db2c3c38fd3 candidate role.trx43行亲读：1失败，index8 expected1 actual0，RunInfo Exit2；business旧HA SHA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0，两消息old/new0/0而数量正确。故旧红→新双架构绿成立于未渲染初态；旧红仅x64，不冒x86独立旧红。本次未重跑旧红。
- 两regression TRX精确路径为 `readback/regression-{x64,x86}/Results/ia-regression/TestResults/child.trx`（各200行），不是目录搜索能否命中的判断。已直读结果区及尾部Counters：30/30、failed/error/timeout/aborted/notExecuted等全0，Completed，无RunInfo错误。两监督39行全读：120秒、Reason exited、ExitCode0、TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut=false、OutputCollected=true、SupervisorError=null。stdout122行双架构全读确认各隔离role通过。payload外层数值Exit/kill/drain没有同级监督文件，本报告只记父TRX/三子Exit，不补造外层数值。
- 精确filter为六个OR：HyDeviceStatePayloadTests；CompositionNotificationTests；WhenAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates；WhenOriginalAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates；WhenHwUnregisterGroupRunsThenIndependentRolesRecordQueueBoundaries；WhenAsyncCloseReturnsThenSurvivingExternalStreamReleasesExactlyOnce（每项前缀FullyQualifiedName~）。组成3通知/递归锁 + 2HY + 12候选排空(0..11) + 6官方排空(0,1,2,6,7,8) + 1HW注销group + 6关闭(True/False×6,7,8)=30。含显式HS/HW有限探索及HU隔离角色；不是30项默认原产品闭包，更非full。
- 必要事件定点亲核：双候选HW dispatch-enter→unregister-return→callbackexit→retained count1，x86尾部新backchannel完整1/1、ownerJoined=true/windowError=null/cleanup；双候选retainExternal state6业务 closeReturn时disposals0→externalRelease内callback enter/exit→最终1，随后sibling/deinit及三次wait finished=true。其余逐角色/逐状态全部事件全文未逐一复核，不把计数通过扩大为任意交错/长期资源证明。

### 包、构建及最终交接
- 输入为 `artifacts/ia-state-snapshot/package/{x64,x86,guard-x64,guard-x86}`，发布在独立publish，回收在独立readback；输入无Results，运行后readback新增Results是证据而不是白名单输入违规。正式com-acceptance-package产物与运行回收不得混为一个集合。
- package-files.txt四份存在，x64/guard-x64各419行、x86/guard-x86各418行；本次清单有分段读取但部分工具输出截断，**未完成四包逐项全集合差分、逐文件hash与RuntimePackAsset求值核验**，不声称完整包审计通过。已加载核心模块身份及消息断言独立成立，不能用清单审计不足抹掉通过的有限初态证据；未新增上传或改打包项目。
- 原IA normal default build专用日志已核：命令dotnet build、目标WpfGfxShape.slnx、退出0；3664warnings/0errors，17.08秒。IB无源码改动，因此不重build；不把该历史构建记成IB新构建。未动COM/GeneratedTargetRegistry/NativeRun/ExitDiagnostics/DRDS/VisualCache/historical/捕获/打包/CF CX CY。
- 下一唯一合同门：先以官方独立控制确定一个合法的partition级状态变更（不是逐target HRESULT猜测），验证转换后的old/new及渲染后注册快照同源；广播分支须有合法双listener官方控制再实现。不重跑本次已完成组，不扩大为full/显示/Event0。
- Available/HWloss/mediaresources/full四格/x64Event0继续开放，完整替换未完成。最终源码状态：保留SameThreadChannel初态/注册快照，默认HY测试和既有COM/fill保留，其余生产不改。

最后补记（IB）：本报告全文47行及三共享IB最新受改节均已写后回读；最终裁决为保留有限初态修复，非待裁决。其余角色完整事件及包全集合核验不足已如实登记，不作为撤回已通过初态修复的额外门。

## IA历史进行中

本轮只调查并修复 partition 所有者的真实初始化 NoDevice 状态及注册快照的 64 字节 MIL_MESSAGE 序列化。先读取 HY/HZ、HW/HX、正式合同和当前所有者及独立回归；不重引 target HRESULT 到全局状态映射，不修改 GeneratedTargetRegistry 或多 listener 广播规则，不宣称完整 device-loss 状态机。

执行门：当前源码新 x64 payload → 同组 x86 → 适用通知/异步排空/有限注销/关闭兄弟 → 最终 normal default build。使用独有 artifacts/ia-state-snapshot 输出，正常分析器与依赖图，正式 ComAcceptance 自包含 10.0.13；仅实际证据计数。关键门失败撤回本轮生产变更。

阶段进度：仅改 SameThreadChannel.cs 的 partition 初态字段与注册序列化。新 NativeAOT 双架构及默认正式测试包已发布；x64 task a1185e06037545b6948e188b5893dbfc、x86 task 40fb4374bcd048318cc50162775821ee，各父2/2及三独立role各1/1。既有 HW/HS guard 包监督 task x64 25d762d8c59544e0b3a6feed6d25cae0、x86 849373c9aad24379a8f07e3189a2b7dd，均子Exit0、无timeout/kill/drain失败。正在逐项回读，未作最终验收。HY/HZ 与 HW/HX 运行记录仅历史，不计本轮新绿。状态变化、广播、有效 target 非零 sync、长期资源及 full 四格保持开放。
