# 句柄类型 sidecar 阶段A：预注册设计与预算

## 范围收缩（修改前落盘）
仅改 Tests/WpfGfxShape.ComAcceptance：一个内部helper、原Sample之后的编译条件hook、独立ResourceLifecycleHandleTypesDiagnostic配置及纯解析MSTest。WpfGfxShape.Tests无需共享，故不改；生产/Native源码0改。connection Acquire后发布异常漏Release只沿原调查独立登记，不修复，不归因成功趋势。

修改前：ResourceLifecycleTrendDiagnostic仅RESOURCE_LIFECYCLE_TREND；原65样本无类型旁路。修改后：旧配置完全不变；新配置同时定义RESOURCE_LIFECYCLE_TREND和RESOURCE_LIFECYCLE_HANDLE_TYPES，默认配置不包含helper/hook。原opt-in固定值及父子传递不改；现成精确filter解析840及其测试通过继承原symbol编入，无须更改解析器。只COM，不扩两个项目。

## 固定合同
每role恰好10个预注册点：Initial；round0 BeforeInitialize/AfterSourceCloseTargetUpdate/AfterCleanup/AfterCleanupPlusOneSecond；round3 AfterCleanupPlusOneSecond；round4 AfterSourceCloseTargetUpdate/AfterCleanupPlusOneSecond；round8 AfterCleanupPlusOneSecond；round15 AfterCleanupPlusOneSecond。只能在原metric写入samples和checkpoint后追加，独立handle-types.jsonl，不改65samples/descriptive schema。不按结果加点。
原official-first、16轮4+12、65samples、无强制GC、DescriptiveOnly软指标及role360/round20/父840/外900保留。Unsupported/Unavailable/Partial单独作能力结果，不阻止已存在业务硬门；不能把类型缺失当0或业务失败。进程超时/forced/unreaped仍按原业务监督失败即停。原句柄关闭/复用使duplicate后的对象不保证是枚举时原对象，类型不是owner，Event不自动归channel；Thread不证明活线程。

## API和安全预算
C#动态解析系统ntdll的NtQueryInformationProcess(51)、NtQueryObject(2)，GetCurrentProcess伪句柄，自进程DuplicateHandle仅DUPLICATE_SAME_ACCESS=2，不继承、不CLOSE_SOURCE，只close自己duplicate。绝不访问Thread私有SafeHandle，不等线程退出，不查名称/栈，不用驱动/CLI/额外线程任务超时器/TerminateThread。
仅Windows X64 pointer8，x64表头16、entry40：HANDLE/SIZE_T/SIZE_T各8，随后ACCESS_MASK/ULONG/ULONG/ULONG各4；TypeName UNICODE_STRING头16，Buffer偏移8。表从64KiB、最多4次扩容（5次调用）、4MiB硬上限、16384条；类型从1KiB最多3次调用、64KiB。只长度状态重试，保存每次NTSTATUS/实返长度/容量/时长和Win32错误。严格按实际ReturnLength检查header/count/entry，Unicode长度/MaximumLength偶数及pointer+MaximumLength在实返buffer内；不解读保留字段，不猜越界类型指针。
快照返回后累计2秒、role累计10秒停止后续诊断；单次同步NT调用无法硬超时/不可取消，必须依靠原角色360和外层监督。每项duplicate与buffer分别finally清理，不finally throw，不闭包取addressTaken局部；主异常与清理错误分存。JSON稳定DTO不含裸IntPtr，记录OS/架构/schema、UTC/QPC、前后HandleCount/错误、全部调用、classified/unknown、opened/closed/outstanding、buffer分配释放与耗时；只在cleanup之后写文件，输出耗时另以同目录write记录保存，记录整个sidecar扰动。若自身close失败，保留outstanding并停止后续快照，不谎报0。

## schema来源（不是稳定公开契约）
已通过网页读取System Informer phnt ntpsapi.h master完整schema相关定义：https://github.com/winsiderss/systeminformer/blob/master/phnt/include/ntpsapi.h 。ProcessHandleInformation在50之后=51；PROCESS_HANDLE_SNAPSHOT_INFORMATION的两个ULONG_PTR后跟PROCESS_HANDLE_TABLE_ENTRY_INFO。该来源为系统参考而非Microsoft公开兼容保证；保存上述字段与x64推导，不声称commit固定。raw地址获取404，改GitHub页成功。
Microsoft Learn NtQueryObject：https://learn.microsoft.com/en-us/windows/win32/api/winternl/nf-winternl-ntqueryobject 明确可更改/移除、动态链接、ObjectTypeInformation仅TypeName有效；PUBLIC_OBJECT_TYPE_INFORMATION为UNICODE_STRING+ULONG Reserved[22]。不使用全系统对象数。NtQueryInformationProcess内部契约亦无长期公开保证。

## 实施顺序与停止线
1 已读残余调查全文（分段补读截断）、规则与趋势关键实现；设计落盘。
2 实现仅COM helper/config/hook/纯解析边界测试。
3 静态审核固定点、长度/条目/Unicode/typeunknown/清理/默认关闭及840继承。
4 normalCOMbuild1：targetPath=WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，configuration=ResourceLifecycleHandleTypesDiagnostic，runtimeIdentifier=win-x64，targetFramework=net10.0-windows。不抑分析器。不发布、不运行测试或沙箱；失败不修后再build，成功也停。
5 即时保存真实返回/exit/warnings/errors/截断边界，回读；正式报告及恢复入口/current-work-item/plan最小更新并回读。
预算：normal build≤1；本阶段Native/COMpublish/测试/沙箱/终端/脚本/委派全部0。当前尚未build。
D3F5有限绿保留；资源PendingInvestigate/MemoryDescriptiveOnly/memoryGreen=null/WorkerExitNotProven/full Open；x86旧15A545、所有旧账保留。后续仅建议另授权COMpublish1（隐式编译另记）+唯一外900沙箱1，绑定原D3F5同SHA clean，实际路径/SHA及新test SHA/MVID须再核，不自动执行。
