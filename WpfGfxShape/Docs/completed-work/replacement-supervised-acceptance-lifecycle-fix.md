# 验收层生命周期修复：独立复核与交接

## 结论与授权

IndependentReview / ManagedTestsPassed / FormalAcceptanceBuildPassed / NativeNotRun / OverallOpen。

本承接者为独立复核实施者，不是只读分派的监督者。用户本轮明确允许专用 build/test/publish/Windows sandbox，无新次数上限，不沿用历史预算；禁止终端和脚本，禁止原版加载、复制及 fallback。运行已获授权，下一轮不得以缺授权阻塞。验收程序和测试不得影响 wpfgfx 生产逻辑；本次未改任何生产源码、默认 async、oracle、expected、阈值、DPI、flags、生产公共 API 或测试钩子。

先读 next-session-handoff 顶部、current-execution-rules、本轮 package-identity 静态报告，继读连续性协议、章程、基线及工作项。旧文档的耗尽预算和“下一另授权”仅为历史，不能覆盖本轮用户授权。

本次没有发现必须改动本切片源码的确定错误，没有进行源码修正。仍有未关闭重大风险，不能宣布整个验收生命周期或完整替换通过。

## 亲读源码与依赖边界

前实施者报告的范围与当前实际文件相符：

- `Tests/WpfGfxShape.ComAcceptance/Pcore/DispatcherScenes.cs`：进程寿命 candidate 文件租约、并发证据队列、worker owner 清理接线。
- `Tests/WpfGfxShape.ComAcceptance/Pcore/DispatcherWorkers.cs`：新增 internal owner，拥有 ready/start/stop、已启动线程及超时保留队列。
- `Tests/WpfGfxShape.ComAcceptance/AcceptancePaths.cs`、`AcceptancePathsTests.cs`：身份判定与边界测试；无效 PE fixture 当前为截断 MZ 两字节，不是全零输入。
- `Tests/WpfGfxShape.ComAcceptance/AcceptanceInitialization.cs`：独立 AssemblyInitialize 和真实文件身份测试。
- `Tests/WpfGfxShape.AcceptanceLifecycle.Tests/`：独立 csproj、DispatcherWorkersTests、TestDirectory。

这是对当前文件和项目依赖的直接检查，不是 Git 差异或源码加密冻结证明；前实施者完整历史修改集仍以其报告为来源。

生产项目 `Code/WpfGfxShape/WpfGfxShape.csproj` 为 net10.0 Native AOT Shared，没有到上述验收项目的反向 ProjectReference。独立托管测试为 net10.0-windows / MSTest 4.0.1，只链接 AcceptancePaths.cs、AcceptancePathsTests.cs、DispatcherWorkers.cs 及资源；没有 ProjectReference 到生产项目或正式验收应用，不链接 DispatcherScenes、Pcore Program、NativeRun、AcceptanceInitialization。它仍声明 UseWPF，因为链接文件中存在 OriginalDll getter 的 WPF 类型引用；getter 保留 NoInlining，但测试未调用它，不执行 WPF 场景。TestDirectory 只改变该专用测试进程的工作目录。

正式 ComAcceptance 项目依赖既有 WpfGfxShape.Tests（Stage3Production alias），且包含链接生产辅助源码，这是既有测试侧依赖，不是生产导入验收层。此次未改变依赖方向。独立新项目不在当前 slnx 三项目列表内，必须按 csproj 定向验证，不能假定解决方案测试已覆盖它。

### 初始化拆分是否削弱正式门禁

正式项目 SDK 默认包含 AcceptanceInitialization.cs，没有看到排除它的条件。AssemblyInitialize 仍调用真实 AcceptancePaths.ValidateIdentity，记录 aot-identity.json；Sandbox=true 仍读 aot.sha256，校验 PE/native-only/架构/摘要。真实身份测试也保留。身份门不是删除，而是从可链接判定器测试分离出去。本次构建证明正式项目可编译，不证明实际正式初始化已运行。

独立测试故意不含 AssemblyInitialize，不需要外部 candidate，测试的 PE 都是 MemoryStream 合成非可执行 fixture；模块名单测试只是字符串判定，不真的加载任何模块。这是诚实的单元隔离，不能称已认证 candidate 或真实入口负控。另两个测试 `WhenSandboxRunnerStartsThenRelativeResultsStayInsideApplication`、`WhenSandboxPackageIsBuiltThenManifestMatchesOnlyWhitelistedInputs` 在 Sandbox=false 下直接 return；因此 29 个真实测试结果不等于 29 个有效行为断言，准确是 27 个执行行为检查加 2 个不适用早返（MSTest 仍记 Passed）。未改这些既有正式包测试以追绿。

## 生命周期逐项独立检查

1. **租约与获取失败**：candidate File.OpenRead 在 ValidateCandidateIdentity、PresentationCore/WindowsBase 加载和 resolver 安装之前；成功的句柄直接进入静态 DispatcherProcessLeases，正常和失败都保留至专用进程退出。若 OpenRead 抛出，则没有 resolver/worker 启动，失败被外层捕获。若后续身份验证或 resolver 安装失败，已获取句柄仍保留。这修复了栈展开提前释放文件租约的原风险，但不是 native 资源已释放的证据。
2. **强引用/stop**：worker 闭包引用 owner；Cleanup 在 Join 之前 Cancel、Set，超时则静态 Retained 队列再持有 owner。Ready/Start/Stop 在全部已记录线程退出前不 Dispose。worker 开始等待及各帧、媒体段前有取消检查；native Render、媒体等待、dispatcher shutdown 不是可抢占取消。超时存活线程允许继续运行至进程退出，不能称收敛。
3. **Dispose 竞争**：当前调用者每 owner 单次 Cleanup，Started 仅父线程写，worker 不调用 Cleanup/Dispose。Join 全成功后才 Dispose，故当前使用下未见 worker/Dispose 竞争。owner 不支持任意并发或重复 Cleanup，不把 internal 辅助类型夸称线程安全公共 API。
4. **线程启动失败**：SetApartmentState/Start 位于外层 try 内；之前成功 Start 的线程已登记，finally 仍清理。Start 抛出但没有成功启动的线程不需 Join。成功 Start 后立即 Add；极端 OOM 不在可恢复异常合同内，未注入实测线程启动失败。
5. **清理异常**：worker finally 的 InvokeShutdown 异常独立入 failures，不会使父线程 Join 跳过。owner Cleanup 的 Cancel 当前无用户注册回调，sync 原语也无外部 Dispose 调用者，故未见当前路径的确定抛错根因；它本身并非通用异常聚合/可重入清理器，尚未做注入验证。外层 catch 不捕获 OOM/StackOverflow。
6. **总期限**：JoinUntil 用一个 Stopwatch 扣减所有线程的共享期限，不再按每线程累加 60 秒；Cleanup 同理共享 10 秒。每 cycle readiness 最多 20 秒、执行 Join 最多 60 秒、额外清理 Join 最多 10 秒；两 cycle 的这些等待名义上最多 180 秒，不包括同步 API/文件 I/O/线程启动/GC 等。**不是整个角色硬总期限**。
7. **证据锁**：records/failures/resolution 改为 ConcurrentQueue 后，入队和 ToArray 不再持 resolver 的 `gate`，所以 worker 卡在 NativeLibrary.Load/ApplyProtection 的 gate 内不会使报告线程为了复制 resolver 列表等待同一无限锁。gate 仍串行 native 加载；并发快照不是原子业务结束快照，但失败不会因此变成功。序列化、身份读取、模块枚举和文件写入本身也不是硬实时。

### 未关闭重大风险（不在本次擅自扩展范围）

- `NativeRun.RoleAsync` 仍三分钟等待，取消后 `Kill(true)` 再无 token 的 WaitForExitAsync；Kill 失败和退出等待没有完整有界取证合同。不要误读同文件 RESOURCE_LIFECYCLE_TREND 条件分支的较强回收代码为共享 RoleAsync 已修好。它影响多个角色，不是本 owner 的最小修正，本轮未改。
- RunDispatchers 正常尾部的 InvokeShutdown、GC.WaitForPendingFinalizers 无期限；若卡住不会写 dispatcher.json。需要外部有界监督/终止及失败证据，而非将 GC/线程清理视为自动通过。专用沙盒超时不证明业务清理成功。
- 未实际注入 lease 获取失败、启动失败、dispatcher 清理异常、Dispose 竞争及真实 native 卡住。本轮 4 个 owner 测试覆盖有限托管所有权，不覆盖这些整合路径。
- 原版资产隔离见下一节；未核清其来源，不能认证旧包满足禁止原版复制要求。

## 原始证据亲读（不是从摘要认证）

工作区根为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape`；本文所属项目根再下一级 `WpfGfxShape`。下述 `.coding-agent` 路径均相对工作区根。

### 前实施者最终原件

亲读 `.coding-agent/acceptance-lifecycle-pure-final/TestResults/lifecycle.trx` 的逐项 Results、Definitions/Entries 与 Summary，TestRun `26db3b64-a304-47a8-b722-b69298cc017c`，UTC 2026-11-04 02:04:35.8289012 至 02:04:46.0517812。实际 29 UnitTestResult，均 Passed；Counters total/executed/passed=29，failed/error/timeout/aborted=0。定义中 storage/codeBase 明确指向任务 `bd2ec83f55254fe9a69589d53875cd72` 的 `WpfGfxShape.AcceptanceLifecycle.Tests.dll`，不是正式 COM 或 Native 应用。

29 的构成为路径/包相关 10（含上述 2 个早返）、PE/摘要判定 7、模块/摘要记录判定 8、worker owner 4；参数化 DataRow 各算一次结果，未另加业务日志分母。四个原静态报告新增边界（CLR header、无 optional header、失败检查零模块、失败检查异路径模块）都有真实 Passed。四个 owner 方法对应 NoWorkers、WaitForStart、JoinDeadline、WorkerRemainsAlive；最后一个耗时 10.0081926 秒，与 Cleanup 10 秒期限相符。

该目录仅找到 TRX，没有 stdout.log、stderr.log 或独立进程退出收据。TRX Completed 不是 OS exit code。故前实施者“最后 exit0”仍是其报告，**本次无法从该目录原件独立证实数字退出码**，也不能伪造退出日志。没有以新运行替换旧任务事实。

另直接列目录发现旧 self-contained 目录根有 `wpfgfx_cor3.dll` 和 WPF runtime 文件。其来源/哈希本次未核，不能断言其一定是原版或已加载，但不能认证旧包满足“禁止原版复制”。源码路径未见测试主动加载它，TRX 没有模块快照；严格的运行级“未加载任何原版”仍未知。为避免再次复制该未认证资产，本次没有重推旧目录，而是新发 framework-dependent 纯测试包；新包按名称检查无 `wpfgfx_cor3.dll`。这不改变前次原件。

### 本次独立执行

1. 专用 publish 一次：AcceptanceLifecycle.Tests.csproj / Release / win-x64 / self-contained=false，输出 `.coding-agent/acceptance-lifecycle-independent-review`。日志 11 行已读，退出码 0。日志未出现警告诊断，不冒称前实施者发布产物身份一致。
2. Windows sandbox 一次：仅启动该测试 EXE，参数为 TRX 报告、文件 independent-lifecycle.trx、结果目录 TestResults；timeout=120 秒。任务 `89081cf3fd2541f581f6936571ed43dd`，MSTest 4.0.1、win-x64、.NET 10.0.1，工具 stdout 报 29/29、耗时约 10 秒，无任务超时报错。
3. 回拉原件 `.coding-agent/acceptance-lifecycle-independent-review-results/TestResults/independent-lifecycle.trx`，TestRun `96c8e0ca-81ef-4562-83bf-7e2c1543182a`，UTC 02:08:37.1594444 至 02:08:47.3130214；亲读关键 PE/模块/四 owner Results、任务定义和 Summary，29/29/0，owner 保留例约 10 秒。执行接口此次返回 stdout 和结果路径，未单独暴露数值 exit code；只记执行成功返回与 TRX 通过，**不宣称亲见 sandbox child exit=0**。
4. 专用 build 一次：正式 `Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj` Release，退出码 0、4107 warnings、0 errors、12.60 秒。亲读日志头尾；输出原有 3417921 字符、工具仅保留尾 20000 字符，不声称逐条复核全部 warning。此次不是前实施者的 171 warnings，不能混写。正式应用未运行。

本次准确次数：独立 build=1；run_tests=0；托管 publish=1；sandbox=1（测试进程运行=1）；生产 Native publish=0；真实 COM/WPF dispatcher=0；终端=0；脚本=0。publish 自带的编译不另算独立 build。没有新增源码修改。

前实施者报告次数另账：build=4（最终 exit0 / 171 warnings）；run_tests=1（MTP 冲突，测试未运行）；托管 publish=2；sandbox=3（最终 29/29）。本次独立证实的是其最终 TRX 29/29 与任务关联，不是其完整历史调用次数、历史 build exit/warnings 或最终数值进程 exit；这些仍标“前实施者报告”。全零 fixture 失败历史未抹除，当前截断 MZ 用例已在两份 TRX 有 Passed。

## 实际修改与未验证范围

本承接者只新建本文并更新 `Docs/next-session-handoff.md` 顶部导航；另产生专用发布/回拉证据目录及构建产物。没有改前述验收源码或任何生产源码，没有更新历史报告来追认运行。没有 Native 发布、COM、真实 dispatcher、实际 candidate-only 模块认证、x86/full、像素差分、生产资源回收证据。旧 resize consumer 前置失败仍不是 resize 通过。

## 下一唯一主动作：正式新 Native candidate-only WPF 验证

用户已授权运行，不需再以历史预算/缺授权作 blocker。本次未尝试生产发布，因此不能声称发布环境失败或新包确实不存在；实际前置是**尚无本次亲核来源和最终 SHA 的正式新 Native 包**，不能直接复用历史未知身份 DLL 或本次纯测试发布目录。

应由当前生产源码专用发布新隔离目录：Release / win-x64 / net10.0，PublishAot=true、NativeLib=Shared、SelfContained=true、WpfGfxShapeManagedAcceptance=false、WpfGfxShapeDiagnosticAbi=false、WpfGfxShapeRuntimeHeapDiagnostic=false，保留当前默认 async 和生产设置。按现有正式运行时约束准备匹配的 WPF/运行时（历史正式包为 10.0.13，不以本轮纯测试的 10.0.1替代）。保存完整发布日志、DLL/PDB、源码/构建身份；确认 native PE 无 CLR header、架构匹配，并对**最终包 Native/wpfgfx_cor3.dll**计算 SHA256、写 aot.sha256，记录来源路径及最终清单。

打包正式验收 EXE 和依赖时必须排除 runtime 原版 wpfgfx_cor3.dll；不可将完整 self-contained WPF runtime 原样推送再声称禁止原版复制。保留必要 WPF 托管依赖及其他合法公开组件，包内只允许认证 candidate 的 wpfgfx 同名模块。不运行当前比较测试中 original 角色，也不为生产提供原版副本/fallback。先直接 candidate-only dispatcher 入口与身份失败负控；应保留原始 TRX/业务 JSON/模块路径/解析记录、总期限监督与退出收据，并覆盖超时存活与错误清理，再扩到 x86/full。

当前共享 RoleAsync 的无期限回收必须先在验收层限定范围解决或选择已有可证明有界的外部监督方式；运行时原版共存应由正式模块门拒绝。若新 Native 发布工具链、正式包依赖/来源、所需运行时或交互桌面实际不可用，应记录具体失败日志/路径及未进入阶段，不能归因为“用户尚未授权”，更不能用纯托管 29/29 替代。
