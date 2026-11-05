# Resource lifecycle Thread handle correlation 阶段A正式收尾

## 终态：BuildFailedStopped / 本阶段收尾完成，非实现验收通过

亲自实现测试侧代码并执行唯一normalCOMbuild1，未委派。编译失败即停，未修后再试、未尾改源码；不存在自动执行/待后台收尾动作。**新增EndQpc自动属性后误留分号导致CS1597，这是本次引入的错误，当前代码不能视为可编译交付。** 失败位置ProcessHandleTypeSnapshot.cs:268:41，源码保持冻结失败状态以遵守授权。

- normalCOMbuild **1/1**，exit **1**，warnings **0**，errors **1**。
- 参数：WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj；ResourceLifecycleHandleTypesDiagnostic / win-x64 / net10.0-windows。
- [真实日志回执](../investigations/resource-lifecycle-thread-handle-correlation-stageA-build1.md)已即时持久化并完整回读；可见19行、无可见截断。MSBuild已用时间00:00:01.07，restore5.02秒，整体墙钟Unknown；不混合两种计时。
- test/publish/Native/沙箱/终端/脚本/再委派 **全部0**。生产、配置、NativeRun、业务合同 **0改动**。0warnings不是分析器完整通过证明。

## 预注册与实际改动

先写并回读[精确前后/预分类/预算合同](../investigations/resource-lifecycle-thread-handle-correlation-stageA-contract.md)，后实现，再写[源码冻结复核](../investigations/resource-lifecycle-thread-handle-correlation-stageA-freeze.md)，最后执行唯一构建。

实际修改仅三个COM测试文件：
1. ProcessHandleTypeSnapshot.cs：schema2；仅已分类Thread的同一个SAME_ACCESS自有duplicate查询GetThreadId→WaitForSingleObject(0)→GetExitCodeThread。每次调用前后QPC、status/value/即时Win32Error分别记录；不靠NT布局声称可靠权限，SynchronizePermission明确Unknown并直接尝试零等待，失败单列。每次新增查询前检查原预算。finally记录本duplicate关闭成功/错误及QPC；数字原handle值只作本快照候选，不序列化IntPtr。Thread列表、distinctTID、每TID handle数、查询秒数、EndQpc、关联Partial、非原子/复用歧义。
2. UceChannelTests.ResourceLifecycleTrend.cs：仅固定点传已有states/current CallbackThread数组；轮次对应索引，当前轮不重复。collector保存round/TID及round-XX/business.json、callbacks.jsonl相对路径、MatchedCallbackRounds；原回调文件qpc/pid/thread用于离线复核，不新增文件读取和生产日志。不称scheduler start-end。
3. ProcessHandleTypeSnapshotTests.cs：新增纯状态/错误/序列化自检，涵盖259+signaled、259+timeout、WAIT_FAILED、GetThreadId0、未知权限、无wait时259仍Unknown、timeout后非259过渡、error50非权限、同TID双条目、失败TID不计identity及numeric handle DTO。因编译失败，**未编译通过、未执行**，不得冒Windows动态验证。

本次还新增上述合同/冻结/日志及本报告，恢复入口/current-work-item/plan状态仅置顶最小更新，历史正文未改。

## 安全语义与静态检查边界

- 不OpenThread、不提升权限、不重dup更大权限、不DUPLICATE_CLOSE_SOURCE、不关闭原handle、不跨快照保留duplicate。
- 仅Thread零等待，不等待Event/Mutex，不消费业务信号。GetExitCode259不能独立判活：signaled=TerminatedAtWait，timeout=NotTerminatedAtWait，错误/缺失=Unknown；timeout后非259标TransitionOrInconsistent。
- Type Status与ThreadCorrelationStatus分开；原Partial及Failures保留，Reason/Error=null不当作无失败。仅Win32Error5标AccessDenied，50归OtherWin32Error，未知值不补零。
- 关联仅callback-worker候选；所有观察非原子，原handle在dup前可能复用、TID跨轮可能复用，无创建时间，数值相等不能证明owner、scheduler身份或GC强根。Thread类型/退出状态不证明Event owner或最终Close。
- 已读既有AsyncSharedOwnership清理及primary.Throw路径，原主异常优先保留未改。新增逻辑无Dispose、finally内throw或native地址lambda捕获；InBudget只捕获snapshot/start。不压CA2219/任何分析器。DTO为数值/记录/数组/字典，不裸IntPtr；实际序列化及PInvoke仍Unknown，编译失败不能给这些检查动态背书。
- 10固定点/role、原65samples、16轮4+12、无强制GC、softDescriptiveOnly、round20/role360/父840/外900、点2s/role10s、16384entries、表4MiB/类型64KiB/native累计16MiB均未改。新增ThreadQuerySeconds单列，仅系统查询区间，不等于全部额外托管分配/关联/序列化扰动；原capture+首写仍不含第二写自身落盘时间，自清理配平不证明无扰动。

## 历史账独立保留

旧20点全部Partial，D3F5有限PassedKnownOwnershipAndCallbacksOnly及旧Event/Thread增长不是本次新观测。资源仍PendingInvestigate / MemoryDescriptiveOnly / memoryGreen=null / WorkerExitNotProven，显示Unknown，**完整替换/full Open**。旧失败/预算/SHA、x86旧15A545、纯解析840 NotRun全部保留。本次不产生新的identity/TRX/业务或资源基线。

Uce connection Acquire后发布异常漏Release仍独立StaticConfirmed、未修，不能当作成功趋势根因。本次CS1597是测试侧新增编译错误，不与该生产异常混账。

## 下一预算申请（NotRun，不自动执行）

本阶段预算已用尽且失败；不能直接进入publish。须先另授权修复已明确的CS1597并重新给出编译验证预算，源码重新冻结并通过后，才可申请 **COMpublish≤1（隐式编译流程另计≤1）+唯一沙箱≤1、外900秒、Native0、追加normal0**。本报告不执行这些动作。

未来运行仍official→candidate各16轮65samples、10点/role、role360/父840/外900、无强制GC，预检D3F5/official/test实际身份；失败/超时/强杀/unreaped/清理失配即停，权限/预算/大量复用歧义保留能力不足，禁止提高权限/加点重跑。发布或运行前不得把当前失败源码当作已验证可发布，也不得把修复/再编译预算包含在本阶段唯一build内。
