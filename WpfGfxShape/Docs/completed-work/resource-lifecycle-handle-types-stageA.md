# 残余句柄类型 sidecar 阶段A正式报告

## 裁决与执行终态
StageAImplemented / normalCOMbuild1 ExecutedPassed / 源码冻结。亲自仅用实际读写/网页/构建工具，无终端/脚本/再委派。生产源码0改、Native0、COMpublish0、测试执行0、沙箱0；只改COM验收项目，WpfGfxShape.Tests不改。构建成功后停止，不自动发布/测试/沙箱。
保留D3F5有限业务绿；资源仍PendingInvestigate / MemoryDescriptiveOnly / memoryGreen=null / WorkerExitNotProven；显示Unknown、full及完整替换Open。x86仍旧15A545，旧KN/媒体/所有历史失败、SHA及预算不迁移。

## 实际实现与变更前后
设计先于源码落盘：[预注册设计](../investigations/resource-lifecycle-handle-types-stageA-design.md)。读原残余owner调查1–115全文，工具长段截断以后续分段补齐；规则和原趋势关键实现核读，不把旧65/130样本说成本次重读。
- `Tests/WpfGfxShape.ComAcceptance/ProcessHandleTypeSnapshot.cs`：默认不编译的内部helper；稳定JSON DTO，无裸IntPtr；动态解析系统ntdll，NtQueryInformationProcess=51、仅自进程DuplicateHandle、仅NtQueryObject=2 TypeName。
- `UceChannelTests.ResourceLifecycleTrend.cs`：只增加条件编译的role级helper及原Sample写samples/checkpoint后的Append；不改原metric/schema/descriptive、轮数/GC/阈值。
- `WpfGfxShape.ComAcceptance.csproj`：仅新增非默认ResourceLifecycleHandleTypesDiagnostic，定义原RESOURCE_LIFECYCLE_TREND及独立RESOURCE_LIFECYCLE_HANDLE_TYPES，优化/portable/x64；旧ResourceLifecycleTrendDiagnostic保持原样。
- `ProcessHandleTypeSnapshotTests.cs`：纯解析/DTO/固定点MSTest，已编译，未执行；长度不足、过量/溢出count、条目越界、pointer-width、Unicode边界/typeunknown/JSON能力状态与10点选择。没有真实系统调用或close注入测试通过证据。
必要资源评估：没有新增用户交互文案，Reason/Status/Operation均稳定机器协议code，故resx0改。ExitDiagnostics及NativeRun0改：新symbol继承原精确840解析和既有840/841/非精确filter测试，opt-in值及父子传递完全保留。

## 固定点、输出与业务门
每role固定10点：Initial；round0 BeforeInitialize/Source/Cleanup/+1秒；round3 +1秒；round4 Source/+1秒；round8 +1秒；round15 +1秒（Source=AfterSourceCloseTargetUpdate，+1秒=AfterCleanupPlusOneSecond）。不按结果增点。原16轮4+12/65samples/无强制GC/official-first/role360/轮20/父840/外900保留。
全部自有duplicate/buffer清理后写`handle-types.jsonl`；另写`handle-types-writes.jsonl`记录capture+首文件写入扰动。原始NTSTATUS、ReturnLength、容量和每调用耗时、Win32错误、前后HandleCount、UTC/QPC/OS/架构/schema、分类/unknown及自有opened/closed/outstanding、buffer开关/累计分配保存。未枚举时Enumerated/Unknown=null，空Types不能当0增长。Partial/Unavailable/Unsupported能力不足不阻止原业务硬门，不冒业务失败，不补跑，不报趋势已闭。
类型是handle条目，不是唯一kernel object、owner或创建栈；Event不自动归channel，Thread不证明活线程。并发关闭/复用意味着duplicate后对象身份不保证与枚举时一致。

## 安全边界与来源
schema来源及字段快照保存在预注册设计：System Informer phnt ntpsapi.h GitHub master网页与Microsoft Learn NtQueryObject（raw获取404已保留），不是稳定公开Windows契约、不是固定commit认证。仅Windows X64/pointer8；表头16/entry40、UNICODE_STRING16/Buffer offset8、PUBLIC_TYPE头104。严格校验实际ReturnLength、count<=16384及entry足够；Unicode Length/MaximumLength偶数/大小关系、完整maximum区域在实返buffer内，绝不解引用表外类型指针，不读保留系统对象总数。
表64KiB起最多5调用/4次扩容/4MiB；类型1KiB起最多3调用/64KiB；仅长度状态重试；快照累计native分配16MiB，最多16384条/每条3次类型调用。复制托管数据也受相同单缓冲上限；未对整个CLR实际分配量作动态测量。
只DuplicateHandle SAME_ACCESS=2/non-inheritable，自进程源/目标；成功取得owner才Close自身duplicate，逐项finally归还自身buffer，不碰原handle或Thread私有SafeHandle。没有名称查询/等待线程退出/栈/驱动/CLI/新增诊断线程任务/TerminateThread。NT同步查询不能硬超时或取消；调用前检查2秒与role剩余10秒，返回后停后续，极端阻塞仍依原角色/父/外层监督，不伪装可取消。
主诊断异常与清理错误分列，类型采集不改变业务主异常；自身close失败保留outstanding并停后续。局部无addressTaken闭包、无finally throw、无reverse-PInvoke回调。静态清理审查不是OS关闭动态证明。

## normalCOMbuild1真实账（成功也停）
证据目录：`WpfGfxShape/artifacts/resource-lifecycle-handle-types-stageA/`，含static-review.md、build-call.json、build-immediate-return.json、build-result.json。
实际专用构建：targetPath=`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`；configuration=`ResourceLifecycleHandleTypesDiagnostic`；runtimeIdentifier=`win-x64`；targetFramework=`net10.0-windows`。
真实日志1–5：`dotnet build`，附加`--configuration ResourceLifecycleHandleTypesDiagnostic --runtime win-x64 --framework net10.0-windows`，**exit0 / 成功**。日志60–63：**144 warnings / 0 errors / 00:00:04.27**。无分析器抑制。normal build1/1，重试0。
构建工具即时返回“执行成功。完整日志共64行…”，已立即保存；再亲读日志1–35、54–64并保存结果。**日志8明确stdout原113573字符，只保留末20000字符，前93573字符不可见；不是完整stdout归档**，不把缺失警告说成全已核、不据末尾未见helper警告证明没有。可见CA2007/CA2014/MSTEST0037/0044/0045/0017等旧区域警告，144总数按真实summary。
源码构建后冻结，未修任何警告/源码、未重build。测试/真实NT调用/新趋势均NotRun；仅编译不证明布局真实系统支持、duplicate关闭或类型覆盖通过。

## 诚实限制与独立旧账
写入记录测量覆盖采集及第一JSON写入，不含第二记录自身写入耗时；role内存累计计入两次写入，但第二写入最终耗时没有再递归落盘。sidecar输出I/O失败写stderr并停后续，不制造业务失败；必须核stdout/stderr与预注册10条齐全，缺件Unknown，不能只看业务Passed。
没有真实系统状态/close故障注入/测试运行；上界与状态降级为静态审查和编译证据，不冒通过。正常构建默认AotDllPath元数据不是D3F5新运行认证，下一发布必须显式绑定D3F5同SHA已验收clean路径。
Uce connection Create在Acquire成功后NewIdentity/Connections.Add异常缺少Release：沿[原调查§4](../investigations/resource-lifecycle-residual-owner-static-investigation.md)StaticConfirmed，**本阶段不修**；它与成功16轮残余趋势根因联系未证，不混账。当前约+2 Handles/轮、PrivateBytes未改善的旧趋势事实不变。

## 下一唯一预算建议（NotRun，不自动执行）
另授权COMpublish<=1（隐式编译<=1另记，normalbuild追加0）、Native0、唯一沙箱<=1/外900。沿原父840/role360/轮20，原精确filter、official-first各16轮4+12/65samples，固定10点，失败/超时/forced/unreaped即停无重试。不扩full/架构/轮/点。
发布显式AotRuntimeIdentifier=win-x64、AotDllPath绑定D3F5E40D2ABC0782D678C843537210EC203987A74C84616E20164703B3895B82已验收clean（原路径缺失可同SHA副本），新test SHA/MVID与实际DLL路径/SHA单独认证。沙箱一次内可先在同exe运行纯解析/现成精确840测试（不额外build/host），再原协调体；不把纯解析成功当真实系统调用通过。回读父TRX/监督/两role身份、原65samples及10点旁路能力与扰动、opened=closed/outstanding0、类型增量；缺件维持Unknown，类型相容也不升级owner证明。主代理审核本报告及明确输出耗时边界后再决定是否授权下一门。
