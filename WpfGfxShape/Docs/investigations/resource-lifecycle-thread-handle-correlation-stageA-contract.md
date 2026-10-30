# Thread handle correlation 阶段A：变更前冻结合同

本合同在源码修改前落盘。授权仅测试侧实现及唯一 normalCOMbuild1；不执行 test/publish/Native/沙箱/终端/脚本/委派。

## 精确前后与预分类

- 前：ProcessHandleTypeSnapshot 仅分类类型并关闭自有 SAME_ACCESS duplicate；无 Thread TID/exit/wait、distinctTID 或逐轮关联。Partial 可以 Reason/Error=null 而 Failures 非空；错误50不代表权限不足。
- 后：只对严格 Thread 的同一自有 duplicate 增加 GetThreadId、WaitForSingleObject(0)、GetExitCodeThread。权限不能可靠确认，明确 Unknown，直接尝试零等待并保存 WAIT_FAILED 的即时错误；不重dup、不提升权限。不触碰 Event/Mutex 信号、不关闭原 handle、不保留 duplicate 跨快照。
- 每次调用前后 QPC，成功状态、实际数值及即时 Win32Error 分离；TID0保留失败，不参与distinct。259+signaled=TerminatedAtWait，259+timeout=NotTerminatedAtWait；失败/缺失=Unknown。timeout后非259另标TransitionOrInconsistent，所有观察非原子。
- 稳定DTO包含schema/role/pid/round/phase、entry index、原handle数值（仅本快照候选）、查询值/时刻、close账、distinctTID与每TID条目数；不序列化 IntPtr。类型Complete不等于关联Complete。
- 固定点复制已有states中的callbackThread及round、callbacks.jsonl相对路径，供按原文件qpc/pid/thread离线复核。关联只称callback-worker候选；无创建时间，所有跨轮/数值相等匹配均Ambiguous，不断言owner/GC根/scheduler start-end。无新增生产日志。
- 纯测试覆盖259/signaled、259/timeout、WAIT_FAILED、GetThreadId0、未知权限和DTO序列化；只编译，不冒Windows动态验证。

## 不变预算快照

默认关闭 RESOURCE_LIFECYCLE_HANDLE_TYPES；原配置/NativeRun/业务合同不变。10固定点/role，16轮4+12、65原samples、无强制GC、softDescriptiveOnly、round20/role360/父840/外900不变。每点2s、role10s、16384条、表4MiB、类型64KiB、native累计16MiB沿用。新增查询逐次检查原预算；新增调用时长单列，写入仍披露第二记录自身耗时未落盘。Thread只能证明类型与观测退出状态，不证明Event owner、强根或最终Close。

## 执行门

源码静态复核后冻结，只调用一次正常构建：WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，ResourceLifecycleHandleTypesDiagnostic，win-x64，net10.0-windows。成功也停；失败不修后再试。构建真实exit/warnings/errors/可见截断立即单独持久化回读。之后仅文档收尾，不尾改源码。

检查已有Dispose主异常保留/native地址闭包/CA2219/序列化，不压分析器。旧20 Partial、历史失败/SHA/预算、解析840 NotRun、未修Uce Acquire后发布异常独立保留。资源PendingInvestigate/MemoryDescriptiveOnly/memoryGreen=null/WorkerExitNotProven，完整替换Open。
