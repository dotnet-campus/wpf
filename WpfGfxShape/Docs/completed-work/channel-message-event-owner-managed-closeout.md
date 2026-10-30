# SameThreadChannel message event：有限生产修正与托管验证收尾

## 裁决

**Implemented / ManagedValidationPassed / NativeNotRun / CloseoutCompleted**。仅修channel私有AutoReset EventWaitHandle显式所有权缺口，不认证资源趋势已消失，不等同永久泄漏归因。旧x86/x64硬门PassedKnownOwnershipAndCallbacksOnly原身份原范围保留；资源趋势PendingInvestigate、softDescriptiveOnly、memoryGreen=null、WorkerExitNotProven、GPU/长期NotProven、完整替换Open。

用户本轮选择先修已确定缺陷，替代旧静态调查§4“先类型诊断”的下一动作；不先扩类型诊断。本轮Native/COM publish/沙箱/新趋势/强制GC/终端脚本/再委派均0。已停止执行，不留本轮收尾待办。

## 依据、范围与快照

完整读取最新静态调查（分段消除4000字符截断）、恢复入口及规则/连续性主题、当前状态相关头部；读取SameThreadChannel事件owner、constructor、Destroy/ReleaseResources及PartitionScheduler Run/Submit/Dispose、Wait ABI和Destroy ABI、现有PartitionSchedulerTests全部及项目规则。不是声称全文重新审计整个生产类/全部历史日志。工作区实际仓库内路径有额外WpfGfxShape层，初次绝对Docs路径不存在后定位为WpfGfxShape/Docs，未在错误路径建文件。

变更前落盘[快照](../investigations/channel-message-event-owner-change-snapshot.md)，含变更前后、并发合同、回撤范围和唯一build/test预算；随后更正等待测试实施说明（保留实际证据边界）。SDK global.json10.0.107/latestFeature；生产与Tests均net10.0，nullable/unsafe开启，MSTest4.0.1；本轮不改任何项目、TFM、包、可见性或InternalsVisibleTo（原有friend供真实生产单元使用）。

真正代码改动仅：
- `Code/WpfGfxShape/Core/SameThreadChannel.cs`：constructor失败finally Dispose；WakeMessageWaiters终态guard；WaitForMessage句柄getter与DangerousAddRef移至_messages锁内且先判终态；ReleaseResources锁内幂等终态、Set后finally真实Dispose，并提前到后续外部资源释放之前。
- 新增 `Tests/WpfGfxShape.Tests/ChannelMessageEventLifecycleTests.cs`；既有PartitionSchedulerTests不修改。

未改调度架构、ABI签名、HRESULT expected、通知窗口/显示/媒体/NativeRun/采样器/统计/阈值，无新接口/外部库/反射/GC.Collect。确认兄弟路径：Post原有消息锁+destroyed guard保持；生产目录仅本处显式DangerousAddRef，不扩修其他owner。

## 并发论证和合同边界

实际路径是kernel32 **WaitForMultipleObjects裸句柄**，不是WaitOne；不能删除显式借用。获取SafeWaitHandle、成功AddRef、Reset在同一_messages锁下；成功借用唯一配对finally DangerousRelease，早退added=false不归还。Reset后native等待期间不持消息锁。ReleaseResources在同锁下提交destroyed，清消息/窗口，Set后finally Dispose：owner引用真实释放，不是置null或留finalizer；已有borrow保持native句柄有效，最后borrow返回后物理关闭。所有Set/Reset/getter/AddRef/owner Dispose遵守此锁。Dispose之后Post/Wake no-op，Wait在获取event前返回原0x80070006，合法存活Wake不屏蔽。

_scheduler.failed partition的management Destroy完成后仍Wake_ 是已核真实调用点；终态guard避免ObjectDisposedException破坏worker Completion。ReleaseResources终态和event释放幂等。Destroy/Commit命令及COM资源仍按既有ABI Gate或composition-thread串行合同，**没有宣称支持任意并行Destroy/Commit**。构造后续初始化未完成时finally关闭字段event，不吞原业务异常。事件关闭先于可能抛错的外部资源清理，不把其他资源异常清理扩成新重构。

等待结果沿原生产合同：等待前/后destroyed为0x80070006；ABI查不到identity仍E_INVALIDARG，两者不混。官方源`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/clientchannel.cpp:979–1071`规定有消息立即queue index=count、空队列锁内Reset后WaitForMultipleObjects，保留waitAll、timeout、外部handle结果。官方不定义已释放对象上继续调用，本轮不新创此ABI expected。AutoReset一次Set不广播；waitAll仍要求外部handles满足，不改既有多等待者/无限timeout合同，不承诺所有waitAll均因Destroy立即退出。

## 真实执行账

|执行|参数/配置|结果|
|---|---|---|
|normal build1/1|Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj，Release，net10.0，RID未传|exit0，3714warnings/0errors，20.52秒|
|定向test1/1|同Tests.csproj，准确OR filter见下|exit0，13passed/0failed/0skipped/13total，27ms|
|test隐式build1|工具无configuration/no-build参数，实际Debug/net10.0|成功，不冒Release测试；聚合warning/error数可见输出未给，Unknown|
|Native/COMpublish/沙箱/趋势/重试|未调用|全部0|

准确filter：`FullyQualifiedName~WpfGfxShape.Tests.ChannelMessageEventLifecycleTests|FullyQualifiedName~WpfGfxShape.Tests.PartitionSchedulerTests`。执行前已声明新9+既有4=13，实际13一致，不跑root solution/full。新9=六个普通方法+Post/Wake/Destroy三个DataRow。现4=failed late register、flush保留failure、source Destroy/shared failure、独立partition不受损。测试通过真实生产类/既有friend API，不mock。

原始可见结果运行后立即持久化于`WpfGfxShape/artifacts/channel-message-event-owner/build-result.md`及`test-result.md`。build可见日志1–62、test1–61全部读取。工具将stdout限制20000字符：build总3113219/前3093219截断；test总1557018/前1537018截断。尾部可见CA1416/MSTEST0044；**没有完整原始stdout归档，不能逐项认证新增warning数或宣称warning全属历史**。未NoWarn、关分析器、改skip、修后重试。test实际输出`artifacts/bin/WpfGfxShape.Tests/Debug/net10.0/WpfGfxShape.Tests.dll`；未给逐FQN/TRX路径，不虚构TRX、SHA、MVID、运行时版本或架构认证。

## 测试能证明与未证

已证行为：重复Destroy不触碰已Dispose event；Destroy后Wake/Post安全；Destroy后Wait原HRESULT及early-return output不变；已有消息queue index、零timeout、外部signaled handle；Post/Wake/Destroy并发入口；四项postfailure scheduler回归。等待用entered事件同步、native3000ms、Task.WaitAsync5秒；Wake以最多2秒SpinWait重复合法wake避免Reset前信号被清；finally销毁并5秒有界等待，不用随机Sleep/无界Task。

**限制**：entered在调用Wait之前，不能确定worker已跨私有AddRef或已阻塞native；Post/Destroy测的是入口竞态，不是可控暂停于borrow点。没有已公开event/safehandle状态入口，不能在不扩API、不反射的限制下直接断言IsClosed/CloseHandle；未以全局HandleCount下降或强制GC冒证据。最后borrow归还物理关闭是基于SafeHandle引用合同的静态论证，仍待Native实际资源验收。构造失败无合法确定触发，不造null非法输入/故障注入API；动态未证。ReleaseResources在外部cleanup抛异常后其他资源的既有风险未扩修。

因此本轮最多证有限事件生命周期修正+托管验证，不证明每轮+4全由event、永久泄漏、全部worker退出、GPU归零、长期无泄漏。AutoReset多并发wait和waitAll限制保持，不通过换ManualReset或新cancel handle重构。

## 下一唯一门（需另授权，本轮不执行）

Native x64新发布及原相同官方/候选有限资源合同复验，**建议新预算**：normalbuild0（已有托管build成功）、Native x64 publish1（发布隐式编译另计）、COM test publish1（隐式编译另计）、沙箱1外900秒；无自动重试、无x86搭车。沿既有resource-lifecycle-trend-x64-r2官方精确测试入口与opt-in链，official-first16轮成功才candidate16轮；各4warmup+12measurement/65samples，轮20/role360/父840/外900，原阈值/峰值/自然GC不变。先认证新实际Native/test SHA/MVID/模块来源，不拿15A545/54BC62旧身份认证修改后生产；新增Native哈希必变。任一publish/official/cleanup失败停止，保留失败及原样本；不得先类型诊断或因本轮13通过宣告增长消失。该建议不等授权，当前AwaitingAuthorization/NotRun。

## 收尾状态

恢复入口/current-work-item/current-stable-baseline/plan状态/矩阵最小同步，旧调查、旧趋势、所有历史账保留。当前代码冻结在唯一成功build/test之后，后续仅文档变更；回撤边界见编辑前快照。独立报告及导航回读完成后结束，不追加执行。
