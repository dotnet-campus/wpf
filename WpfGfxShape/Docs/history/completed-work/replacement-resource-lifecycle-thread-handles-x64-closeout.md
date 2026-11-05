# Thread handles x64 r1 正式收尾

## 终态、来源与执行账

CloseoutCompleted：本次亲读原件并完成文档，不是资源问题已修复。原task `d6878f8fa1f44b20841518d523225f78`。新增源码/build/test/publish/Native/沙箱/终端脚本/再委派均0。先写实质草稿，再核证形成此稿。

证据根R：`WpfGfxShape/artifacts/resource-lifecycle-thread-handles-x64-r1/sandbox-results/Results/resource-lifecycle-thread-handles-x64-r1/ResourceLifecycleTrend/58f0aef8e4054665a92a51f0f086d055`。

前stageA CS1597 failed（exit1/0warnings/1error）独立保留。另授权追加normalCOMbuild1成功exit0/145warnings/0errors、MSBuild8.17秒；publish1成功exit0（隐式编译流程1另计）；sandbox1已用，不重复。build stdout前94245字符、publish前37501字符不可见；publish聚合warnings/errors和耗时Unknown，不补造日志。四份即时账`Docs/investigations/resource-lifecycle-thread-handles-x64-r1-*.md`全部保留。

## 身份及父子证据分层

父supervision全文1–40：test SHA `7E39AC16FA8F106B8434EA5622E72FE40E35A977DE1F70C7F92175FAF80226F2`、MVID `3d36ccb7-cc05-4a39-891d-4827ef6a3490`，.NET10.0.13 X64，exit0、TimedOut=false、KillAttempted=false、正常退出并收集输出。父stdout全文1–12显示总1/成功1/失败0/跳过0及35s784ms，中文编码损坏；引用child.trx路径，但当前Results名称查找未找到TRX，本次不声称亲读TRX，此前父TRX核验仅归前代理引用。

R/comparison全文1–53：PassedKnownOwnershipAndCallbacksOnly；official PID4232→candidate PID6580各16轮，无未执行/未完成轮。两role supervision全文各1–20：exit0/reaped=true/forcedTermination=false/cleanupErrors=[]；round20/role360/parent840、watchdogHardRealtime=false。父子进程正常退出不替代业务账，也不证明每个worker退出。

末轮business身份亲读：candidate requested/actual均Native/wpfgfx_cor3.dll，SHA `D3F5E40D2ABC0782D678C843537210EC203987A74C84616E20164703B3895B82`；official requested/actual均根wpfgfx_cor3.dll，SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`。official candidateInitializationSha256=D3F5且IsNotActualRoleEvidence=true，初始化文件不是实际角色模块证据。侧车OS10.0.26200/X64/pointer8、schema2/NotStablePublicContract。

## 业务硬门核验

32份round-00…15 business固定字段搜索亲读：completed/cleaned/deinitialized/trendCleaned=true，callbackEntries1、FinalDisposals1、CallbackExited1、callbackErrors=[]；CallerOwners/Channels/Duplicates0、ConnectionOwned/ManagerInitialized=false、WicDisposed=true；BeforeSourceClose/AfterSourceClose0。32个不同uid支持每轮新鲜构造，不误称每轮新PID。32份Deinitialize均在616行；末轮两侧606–650进一步亲读hr0及Wait.Callback.After finished=true。

32份callbacks.jsonl的1–2整条经限定R内容搜索亲读：Enter/Exit各1、entries1，同role PID固定。candidate轮0–15 TID依次9816/6652/8684/5804/4852/5404/11900/11404/9824/10708/9660/10176/3052/11884/4204/7056。两侧末samples第65行全文：累计entries/exits/disposals16、各轮1、KnownOwnership=ActualRoundBusinessCleanupVerified、对象账0。两侧descriptive头现成汇总samples65/forcedGC=false/DescriptiveOnly/memoryGreen=null。未逐行重读130条全部指标，不冒称全样本指标重算。

末+1s sample在侧车前：official PrivateBytes25550848/WorkingSet63733760/Handles275/Threads9；candidate30830592/65400832/316/11。仅描述，不升memory门。

## Thread最窄事实

20点现成字段逐点核为Status Partial和ThreadCorrelationStatus Partial。每role固定10点：Initial、r0 BeforeInitialize/AfterSourceCloseTargetUpdate/AfterCleanup/+1s、r3+1s、r4 AfterSourceCloseTargetUpdate/+1s、r8+1s、r15+1s。candidate Thread entries序列7/7/13/13/13/16/19/17/21/28，distinctTID6/6/8/9/9/12/13/13/17/24；official distinct6/6/7/7/7/7/8/7/7/7，末点8entries。

末candidate28entries/24distinct，直接亲读现成HandlesPerTid完整字典确认12156有4条、7056有2条、其他各1；不是由28-24自动认“4个唯一具体对象”。TID分组不是对象身份鉴定。

有限关键记录：

|candidate记录|查询结果|最窄可证|
|---|---|---|
|末entry132 handle532 TID1768|Tid/Wait/ExitCode Succeeded；wait258/exit259|NotTerminatedAtWait|
|末entry139 handle560 TID9396|Succeeded；wait258/exit259|NotTerminatedAtWait|
|末entry153 handle616 TID12156|Succeeded；wait258/exit259|NotTerminatedAtWait|
|entry252 handle1012 TID9816，侧车行3→4→10|行3 wait258/exit259；行4及末行wait0/exit0、Succeeded|末TerminatedAtWait、MatchedCallbackRounds=[0]|
|末TID7056关键条目|Succeeded；wait0/exit0|TerminatedAtWait、MatchedCallbackRounds=[15]|

9816和7056关键条目自有dup opened1/closed1/outstanding0，Close Succeeded。可证下界是末点至少2条不同TID已退出记录、至少3条不同TID当次未终止记录。其余未完成逐条状态分类，分布Unknown（不是断言原字段State=Unknown）。未找到现成State/Count字典汇总，20点完整Terminated/NotTerminated/Unknown计数Unknown；不脚本计算，不让软统计缺项阻塞已核业务硬门收尾。7056分组2条，不由首命中将两条均判同状态。

9816对应旧r0 callback.Enter/Exit，末仍有signaled/exit0句柄记录：支持存在“已退出callback候选TID的Thread句柄保留”，不支持全部worker仍活或全部退出。259不能单判活。OriginalMayHaveBeenReusedBeforeDuplicate/NoCreationTime/NonAtomicQueries、TID复用及非原子歧义保留；跨轮同handle值不保证同内核对象。没有创建栈、强根或仅runtime延迟回收的证明。WorkerExitNotProven不撤销。

## 自有账及开销

20点Opened=Closed/Outstanding0、BuffersOpened=BuffersClosed逐点现成字段已核。candidate opened171/171/213/213/213/220/228/222/231/245，buffers各多1；official180/180/210/200/200/201/214/201/202/202，buffers各多1。只证明collector自有资源配平，不是原始产品句柄已关闭。

candidate末Enumerated316/Classified245/Unknown71、Event109/Thread28、dup245/245/0、buffers246/246、AllocatedNativeBytes316416、Seconds.0012367、RoleSeconds.058031400000000004、ThreadQuerySeconds.000020900000000003。official末Enumerated275/Classified202/Unknown73、Event79/Thread8、dup202/202/0、buffers203/203、AllocatedNativeBytes272384、Seconds.0004723、RoleSeconds.05550130000000002。

20条writes现成字段已亲读：candidate CaptureAndFirstWriteSeconds首.0365337/末.0017928；official首.0374179/末.0009584。未重算全程总和；不含第二条记录自身写入，旁路会扰动后续采样，非无扰动反事实。

## 真实读取范围

- 四即时账全文：ledger1–22、publish-return1–17、sandbox-call1–9、sandbox-return1–5。
- 父supervision1–40/stdout1–12；comparison1–53；两role supervision各1–20全文。本次未读TRX原件。
- 32 business固定字段2/16–76/616命中；candidate末轮1–166读取被4000字符截断，仅引用可见部分；official末轮1–16；两末轮606–650。32 callbacks两整条均搜索返回可见。
- samples两侧65行及descriptive头；candidate descriptive较长读取截断，不冒称全篇。
- 20侧车行元数据/Partial/duplicate账/distinct及candidate HandlesPerTid经限定R内容命中核；candidate行1/10、official行10原行均4000字符截断，针对9816/7056等关键字段补读，不冒称整行所有Thread已读。无DLL内容扫描、无计算脚本、无委派。

## 最终矩阵和唯一下一建议

|门|终态|
|---|---|
|已知业务所有权及callback|PassedKnownOwnershipAndCallbacksOnly，32轮|
|Thread能力|ObservedPartial；已退出候选TID保留；全点状态计数Unknown|
|collector自有资源|20点配平/outstanding0|
|内存/长期泄漏自由|PendingInvestigate/MemoryDescriptiveOnly/memoryGreen=null/NotProven|
|所有worker退出|WorkerExitNotProven|
|独立Uce Acquire异常回滚|未修，不据此解释成功趋势或关闭残余|
|完整替换|Open|

唯一下一建议：另授权有界受控诊断区分runtime延迟回收与实际强保活，不实施。静态引用链未证强根，不能关闭runtime私有句柄或任意GC补丁。所有旧趋势/旧原件/stageA失败账保留。无证无关图像调用描述撤回，不计执行。

## 本次修改完整文件清单

均位于`WpfGfxShape/Docs/`：
- `completed-work/replacement-resource-lifecycle-thread-handles-x64-closeout.md`
- `next-session-handoff.md`
- `handoffs/current-work-item.md`
- `handoffs/current-stable-baseline.md`
- `replacement-issues-and-plan-status.md`
- `final-acceptance-remaining-matrix.md`

五入口仅追加顶部覆盖节，旧内容不删除。本切片文档收尾完成，不等于完整替换关闭。
