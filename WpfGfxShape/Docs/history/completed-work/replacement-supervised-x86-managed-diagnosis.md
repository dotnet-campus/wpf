# AS x86 托管全量终止诊断

## 最终状态

**已取得可重复的单测试无完成结果最小复现；已发现该夹具确定的越界写；尚未证明它是 AR 全量终止的唯一原因。未修复代码，未完成最终源码或双架构全量验收。**

本文先创建、结束更新并回读。仅修改本文、独有 ignored `artifacts/as-x86-managed-diagnosis-20260721/`；未清理共享目录，未新增 Host/项目，未改入口、项目、生产或测试代码，未禁并发/skip。没有执行终端、shell、脚本、dump 命令、构建或发布。使用现成正式 C# exe 在 Windows 沙箱中隔离执行。

## 已读规则与证据

- `Docs/handoffs/current-execution-rules.md`、`Docs/wpfgfx-complete-replacement-validation-plan.md`。
- `Docs/completed-work/replacement-supervised-managed-baseline.md`：AM x64 4249/4249 是快照，不是最后源码；x86 无完成 TRX，原接口无数值退出状态。
- AR `artifacts/ar-managed-baseline-20260721/readback-x86-diagnostic/TestResults/log_261019125013119.diag:37800-37830`：ownership 最后完成后 query-failure 首测试开始，4 worker；不能由末测试名断定根因。
- 当前两个 interface 测试源文件、Tests csproj（net10.0/MSTest 4.0.1/生产 ProjectReference）、Directory.Build.props、当前 SandboxTestEntryPoint。未改变 TFM/语言版本。

## 快照与身份边界

每次 sourceDirectory 均为原 AM 包：
`artifacts/am-bitmap-closure-20260721-a/managed-{x86,x64}/publish/WpfGfxShape.Tests/Release/win-{x86,x64}/`。
执行 `WpfGfxShape.Tests.exe`，均保留 runner 默认 Workers=4、MethodLevel；测试类已有 DoNotParallelize，未由本包改动。banner 实际为 win-x86/win-x64、.NET 10.0.13、MSTest 4.0.1；回收 x86 runtimeconfig includedFrameworks 确认 10.0.13。

完整任务包回收到各独有目录，保留 exe/DLL/runtime 与诊断。TRX codeBase 指向实际沙箱任务内测试 DLL；**未取得实际加载生产/测试程序集 SHA256、MVID 或 dirty 源码映射，不把原包命名、文件存在、NativeAOT SHA 或历史 ProductVersion 当成 actual 托管程序集身份。** 本包不重发生产，不用旧 SHA 拼接最后源码。当前源码的夹具越界发现与原包单测复现相互支持，但尚无包 IL 身份核验，不能冒充已绑定源码的修复验证。

## 所有实跑尝试（有界，非反复 full）

统一参数：`--filter <下列过滤式> --report-trx --report-trx-filename <文件名> --diagnostic --diagnostic-verbosity Trace`。输出均在本包 artifacts 下对应目录；沙箱完整任务已回收，未指定任务外路径。

|目录/架构|filter（FullyQualifiedName~ 后缀）|任务号|timeoutSeconds|实际结果|
|---|---|---|---|---|
|interface-x86|Direct3D9DeviceInterface|d86578db77794676aead037b6d7825fb|90|无顶层完成TRX；diag 91行，最后开始 ownership 首测试，无结束|
|single-x86-1|WhenBaseDeviceQueryForExtendedInterfaceFailsWithPointerThenTemporaryReferenceIsReleased|16cfb42cd89e44b991372f9b04fefbad|60|as-single-x86-1.trx：Completed，1/1通过|
|interface-x64|Direct3D9DeviceInterface|3bbe79f1c6844bd0b88841e356a1caad|90|as-interface-x64.trx：Completed，15/15通过|
|ownership-x86-1|Direct3D9DeviceInterfaceOwnershipTests|a14750fa04754dcb880e9dcb65538589|60|无顶层完成TRX；diag 91行，最后开始 ownership 首测试|
|ownership-single-x86-1|WhenBaseDeviceIsDisposedThenOwnedInterfaceIsReleasedOnce|0b303e84ed9143c391f62f9bf7f9a008|60|无顶层完成TRX；diag 91行，仅该测试开始，未见完成|
|query-x86|Direct3D9DeviceInterfaceQueryFailureTests|d9e1769ede214092a238461ff1b5acdb|60|as-query-x86.trx：Completed，11/11通过|
|ownership-single-x86-2|WhenBaseDeviceIsDisposedThenOwnedInterfaceIsReleasedOnce|5cc1d679ee964b20b28a7e6dfd0453bc|60|相同单测再次无顶层完成TRX；diag 91行，仅该测试开始，未见完成|

共7次真实执行。通过格的 TRX 已亲读：single:22-24、query:82-84、x64 interface:106-108；total/executed/passed 分别1、11、15，failed/error/timeout/aborted/notExecuted及其他非成功计数均0。不是合并全量计数。无TRX四格不填“0失败”、不由事件数推算完成总数。

四个不完成任务均由执行接口返回并完成回收，但接口**没有提供数值退出码、进程异常类型、超时标志、栈或dump**。因此可重复的是“该单测试进程运行后无完成TRX且trace截断”的结果，不声称已观测到某个NTSTATUS、FailFast、访问冲突或确定超时。回收“结果已保存”不代表成功。两次单测末时间分别12:57:47.7774300Z、12:58:16.4410716Z；trace:85-91已亲读，TestResults目录只有部署目录与diag，无顶层TRX。

初次定位原报告相对路径时漏了工作区的 WpfGfxShape 前缀，读取 AR trace 与列出 AM 包各一次返回不存在；随后据目录结果纠正，未触发执行。这是路径读取错误，不是产品失败。无其他隐藏实跑尝试。

## 最小复现及确定的夹具缺陷

最小复现无需新程序：上述 AM x86 正式exe，过滤式
`FullyQualifiedName~WhenBaseDeviceIsDisposedThenOwnedInterfaceIsReleasedOnce`，保留4worker默认值，两次均复现无完成TRX。无需先运行 query-failure、其他device族或 full。原 AR 末测试及其完整 query-failure 族分别1/1、11/11通过，不能把它的名字当根因。

当前 `Tests/WpfGfxShape.Tests/Direct3D9DeviceInterfaceOwnershipTests.cs:88-96` 存在确定的非托管堆越界写：

- 只分配 `sizeof(nint) * 4`，合法槽0..3。
- 槽0为vtable指针，槽1为GCHandle，vtable从 `memory + 2` 开始。
- 写 `vtable[2]` 即 `memory[4]`，是分配区之后一个指针位置；IUnknown前三项需要槽2..4，总共至少5个指针。
- x86请求16字节却写偏移16..19；x64请求32字节却写偏移32..39。x64通过也不使越界合法；分配器/堆布局差异可能改变外部症状，尚未直接观测实际损坏位置。

这是明确的夹具内存安全缺陷，不是仅凭测试名称猜测、也不是需要改COM调用约定的证据。Release声明是 Stdcall，与COM吻合；ReleaseOrder静态列表由该类原有DoNotParallelize保护，最小复现只选择一个测试，不能把此次症状简单归为4worker竞争。当前证据不要求禁并发。

兄弟模式已做有界搜索与抽查：PixelShaderStateTests的4槽布局从memory+1开始，vtable[2]落memory[3]合法；ComposeRectsTests的base device同样4槽且+1合法。故不能盲目把所有4槽分配改为5。没有把搜索命中都宣称已审计完成。

query-failure的“失败HRESULT却返回非空指针”属于合成的防御性错误输入，不能未经官方合同核实称为真实COM常态。本包没有扩张此错误输入为官方独立兼容门。查询测试通过只排除其单独运行必然导致本次症状，不能排除与更早损坏的序列关联。

## 根因结论及边界

**已明确定位 ownership 当前源夹具的越界写，并取得同名原包单测试可重复无完成结果；尚未完成修复前后对照，因此不宣布已因果证明 AR 全量终止根因，更不宣称只有这一处。** 原AR可完成部分ownership再在query开头截断，与堆损坏延迟显现不矛盾，但也不是证明。实际异常/退出状态和加载身份仍缺。

没有更改生产：没有证据表明生产需要修复。没有新增诊断Host/项目、改入口或运行长full。没有执行官方独立差分；本包结果仅用于诊断托管夹具，不能替代两架构Release官方零阈值验收。

## 下一确定动作

先单独授权/协调最小测试夹具修复：仅将 ownership FakeDeviceObject 的分配从4个指针改为5（两个对象槽+三个IUnknown槽）；不改Stdcall/并行/生产。将此单文件变更和构建依赖链绑定到新独有Release包，不覆盖AM。

随后新包x86先运行同一最小过滤两次，再两架构interface族；必须由纯C#现有正式项目诊断测试/角色记录实际加载两程序集SHA/MVID及子进程退出码、超时/取消状态（不改现有入口/项目、不新增Host）。若修复后仍无完成结果，则优先在该角色中有界定位构造/释放边界，而非继续full。如果最小格和族均通过，再依据AR顺序做确定的前缀/序列缩减，最后才协调全量托管验收。不得将旧AM的4249/4249或本包局部Green绑定新源码。
