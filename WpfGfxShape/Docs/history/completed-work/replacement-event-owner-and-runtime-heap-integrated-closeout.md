# Event-owner 与 runtime heap 集成正式收尾

## 终态：有限闭环已完成，完整替换 Open

本文件先落实质草稿再执行唯一对照，现已完成结果核验。heap有限可回收性已证；新x86 postfailure、isolation、C6三个有限合同可保留event-owner修正。不是完整final适用集通过；official isolation TRX原件已按精确路径完整回读。

## 唯一新增执行、准确参数及原返回

sourceDirectory=`WpfGfxShape/artifacts/event-owner-production-x86-r1/clean`；executableRelativePath=`WpfGfxShape.ComAcceptance.exe`；arguments=`["--supervise-exit","Results/event-owner-x86-isolation-official","120","FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives"]`；timeoutSeconds=180；未设置outputRelativePath，完整回收；localOutputDirectory=`WpfGfxShape/artifacts/event-owner-production-x86-r1/isolation-official-closeout`。

工具原返回：沙箱执行完成。结果已保存到：d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\event-owner-production-x86-r1\isolation-official-closeout

实际task `48bda4f80c0b45dcb704e4419d305736`，business UID `699931a2a08449f7bf999850f6372a2c`。normalbuild0/tests工具0/Nativepublish0/COMpublish0；sandbox1/1（通过沙箱实际执行测试1），源码修改0、终端脚本0、再委派0、重试0。其余原额度没有开启新分支。

## 新 official isolation 本次亲读

以下路径均相对回收目录的 `Results/event-owner-x86-isolation-official`：
- supervision.json 1–40全部：08:52:56.8670486→08:52:57.810721 UTC，父12268/child3624，exit0、HasExited=true，无timeout/cancel/kill、输出收集完成、无drain失败、SupervisorError=null。内120/外180。
- business.json（PartitionFailureIsolation/上述UID）1–402分段完整：created=destroyed=[86874808,86874296,86874424]，completed/cleaned/disconnected/deinitialized=true，A/S=-2003303418（0x88980406），独立B关闭前后Send/CloseBatch/Commit/Flush均0；Destroy两失败通道、最后健康通道、Disconnect/Deinitialize均0；primary=null/cleanupErrors=[]。真实关系A(source0)、S(sourceA)、B(source0)，alpha .75→.5→.25，负对照仅结果副本。
- 实际official SHA256=`5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，版本10,0,1326,50504/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；requestedEqualsActual=true，moduleHandle=actualBaseAddress=1854210048。不是初始化candidate的845B。
- test SHA256=`DA6E29882DD7FA104BCCC38AAEA10C76581C1C9591FCEA50A6D5589184A38C12`、MVID=`a746e7f6-d5e4-4b9c-b835-22a4401d1e3c`、.NET10.0.13/X86/pointer4；Results/aot-identity.json全文仅证明初始化candidate845B，不能替代official actual。
- stdout1–12全文（部分中文解码乱码）报告total1/成功1/失败0，stderr空。Diagnostic第1–12及50–65：Passed节点UID1e3a533f-c51b-8855-b58a-2f84cc5b1ade，body0.1140177s、session4a89b7af-f38d-4e44-a5de-b59394b90428/MTP exit0；不是business UID。
- **TRX原件已补齐亲读**：最初目录枚举没有TestResults且名称查找child.trx无结果，临时登记Unknown；随后按stdout/diag的精确路径直接读取TestResults/child.trx成功，1–26全文已读，无额外执行。保留枚举/查找漏报这一工具差异，不再声称原件缺失。Counters total/executed/passed=1/1/1，failed/error/timeout/aborted=0；testId=1e3a533f-c51b-8855-b58a-2f84cc5b1ade，executionId=4e5a54ef-e6c0-4881-9640-5cde6f538808，TestRun=e5b64b0f-eb97-4e1b-a369-f691dca73620，body0.1140177s。监督/TRX/business/identity关键原件均已亲读。

## 新 x86 原有任务及身份（不重跑）

- postfailure official `38392b04b4c04c45bbc5b5adaa168749` / candidate `6e0e186db6464f5b98e2ef615d299570`。
- isolation candidate `eb4992319e014f56b275c391659fa2c5`。
- C6 candidate `432a424c1dd048d2938e97eac3645957`；discovery `cbf99e2a74044323a7346a7bd194bd90`。C6仅MTP exit0，不认证OS监督0。
- 上述既有角色通过与关键原件读范围引用用户交接/前代理回执，不冒充本次全量亲读；调用前原账见artifacts/event-owner-production-x86-r1/candidate-invocations.md（已亲读1–5）。五次旧sandbox已用完，本次另授1不冲销。
- 正式Native SHA256=`845B37962F888FFA13CA294B0763E868755EB4D4F51178E08EEA2CA1D1C401A3`；test/runtime同本次。新旧KN/媒体不整体移绑。

## Heap：32轮关键数组及cleanup补核完成

引用[前代理正式报告](runtime-heap-owner-r1-closeout.md)中父TRX26行、父监督40行、两role各20行、comparison53行、三个heap JSON全文、r15身份与末sample的既有读回执，不冒本次重读。最终task `fd66cfc5590b461c8968cdf33d7e6e98`，根artifacts/runtime-heap-owner-r1/complete-sandbox-results/Results/runtime-heap-owner-r1-complete/ResourceLifecycleTrend/a70df5ce8da541168b5b19f5b0e75d1c。

本次补核original与candidate round-00–15共32份business的字段原行18–36、62–76及681–682：先窄行阅读，再分字段正则精确提取避免每文件20命中截断。已逐角色核created/destroyed/released/callerPointers完整数字数组：每轮created与destroyed两项逐值相等，candidate为[2+3r,3+3r]、original每轮两个实际非零指针且逐值配对；所有released=[2,1]、callerPointers=[0,0,0]。32份均BeforeSourceClose/AfterSourceClose=0/0，FinalDisposals/CallbackExited=1/1，disconnected/deinitialized=true；CallerOwners/Channels/Duplicates=0，ConnectionOwned/ManagerInitialized=false，WicDisposed/wicDisposed/trendCleaned=true，callbackErrors/cleanupErrors=[]，primary=null。关键字段无未读尾部；不声称逐字全文或所有events HRESULT已人工复核。

宽查询曾每文件20行截断且maxFiles32遗漏original末六份；后续窄查询max100覆盖全部32并分角色读数组消除此处关键字段缺口。类型sidecar每entry原工具截断仍未消除，不冒全部归因。

短弱引用scheduler/thread：16/16→16/16→0/0；hostGC不降而DLLGC降。AfterHostGc→AfterDllGc Thread27→11、Event107→91，均Partial、unknown71。仅诊断16轮有限可回收，不能把全部32handles精确归属owner，不能把post-GC替代自然趋势，不能判生产无泄漏/加入生产GC补丁。

正式x64 default D3F5、新x86 default845B、diagnostic F4A各身份分开。F4A完整SHA/test/MVID及实际加载证据见heap报告，不能转绑845B。默认属性false和WPFGFX_RUNTIME_HEAP_DIAGNOSTIC源码条件隔离为前报告静态实证；正式default binary诊断导出absence无直接原件认证，状态Unknown。

## 历史账与真正未闭门

heap首次task792db958965f4236a903f4ca40938c45初始化exit2；task52865bb56c344c6e8d5ec0c97011aa7c漏三个末点；最终fd66补点。CS1597失败及追加build成功、publish日志前37353/57353字符不可见、warning总数/耗时Unknown等原账保留，见heap及thread正式报告。heap r1历史累计normal1/4、tests0/3、Native1/3、COM2/4、sandbox3/8；后续x86独立账以其原件为准，不倒扣为本阶段执行。

保持 PendingInvestigate / MemoryDescriptiveOnly / memoryGreen=null / WorkerExitNotProven / 完整替换Open。独立Uce Acquire后发布异常漏Release未修；显示/HW/device-loss/音频/无界趋势未闭。有限报告及六状态同步不等于整体Green。

## 剩余主门确定执行顺序（本阶段不越权执行）

1. **优先Uce Acquire异常修正，而非先对已知有缺陷源码跑final**：在源码层确认Acquire→构造/发布失败的所有权转移及兄弟分支，补成功/失败单元证明；修正只回滚实际取得引用，防双Release。需另授源码实现、normalbuild/tests预算，本阶段为0，故尚未实施。不能用此异常解释成功趋势。
2. 单元及source证明通过后，申请正式x64/x86 Native验收预算，分别冻结源码、Native生产publish、COM绑定publish、实际SHA/MVID/runtime证明；不默认复用845B/D3F5为新修正产物。每门失败停止，保留原失败，不追绿。
3. 对最终产物依次运行Acquire异常相关回归、event-owner/postfailure/isolation/shared ownership/directclose/C6等受影响主要回归；C6需要OS退出证据时使用有监督的既有正式合同。现有三合同通过不能代完整主要回归。每角色读监督、TRX原件、business、identity；本次TRX已由精确路径直接回读，目录/名称工具漏报历史保留，不重跑。
4. 同一最终hash执行完整计划§1–10适用集：托管、COM/ABI、PresentationCore集成、生命周期/资源和错误恢复；新旧KN/媒体逐项判适用性，不整体迁移。先固定集合及已有证据对应，再按未证项执行，不做重复显示前置/paint/capture无证探针。
5. 显示/HW真实输出、resize及device-loss恢复必须在能产生可观测证据的环境验收；没有真实呈现/恢复证据保持Open，不能软件fallback冒HW。音频/连续媒体独立真实结果和清理验收，旧KN不足以关闭。
6. 无界趋势不能由16轮+强制DLLGC证明。另定正式生产无强制GC、可证资源收敛及worker退出合同并验证；有限diag仅支持可回收，不批准memoryGreen。最后逐格消除Unknown/Blocked，final矩阵全适用门满足才关闭完整替换。

停止继续执行的真实原因：本阶段新增唯一sandbox已使用，normalbuild/tests/Nativepublish/COMpublish授权均0；显示/HW/loss/音频及长期资源仍缺最终适用证据。不是文档待写，不擅自启用历史余量。
