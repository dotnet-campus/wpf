# DC：既有 DB full 结果恢复与独立交接

## 1. 最终裁决

**未恢复 DB 两次既有 full 的监督/父 TRX；这是执行结果观测阻塞，不是已证产品缺陷。完整替换未完成。** 不重复 full，不推断原任务超时、退出或仍在运行。主代理仅监督；本轮为独立只读取证。保留 CX/CY 生产修复。

先创建本文再调查、发布与只读恢复。修改仅专属 `Tests/WpfGfxShape.ComAcceptance/DcFullResultRecoveryTests.cs` 及本文、`Docs/next-session-handoff.md`、`Docs/completed-work/replacement-supervised-session-summary.md`、`Docs/replacement-issues-and-plan-status.md`。未改 ExitDiagnostics、NativeRun、生产、资源、项目/打包、业务断言、默认并行、golden/阈值/skip/fallback。无终端、直接间接脚本、Host/第三工程、工具配置改动、源任务清理或 kill。

## 2. 读取和本地恢复尝试

亲读执行规则27行、完整方案120行、current-work-item31行、current-stable-baseline20行；DB unfiltered-full-execution73行、CZ current-comacceptance-full74行、DA full-baseline-preflight-review79行分段补齐。枚举方案仅生产与两个正式测试工程，正式ComAcceptance文件已枚举；工作区AGENTS名称搜索无命中，不认证工作区外祖先规则。

亲读当前 ExitDiagnostics.cs 1–187（长段截断以70–141、136–148、146–187补齐）、ExitDiagnosticsTests.cs87行、AcceptanceResources.resx12行、csproj37行、项目global.json10行及本层Directory.Build.props/targets。TFM net10.0-windows/WPF/Nullable、MSTest4.0.1、SDK10.0.107/latestFeature。

工作区根 `.coding-agent` 枚举为空，log名称搜索无命中；本任务根 `WpfGfxShape/.coding-agent` 不存在。DB产物根只有control-x64、regression-x64、regression-x86、x64、x86，无full-x64/full-x86回收目录。DB日志名称搜索仅见既有三组MTP诊断及运行时DLL；两回归Diagnostic内搜索db-full均无命中。HttpClient.Timeout内容搜索命中运行时DLL资源，不是执行工具日志或新task记录，不据此推断任务身份。可见专用工具没有按旧taskID查询/回收入口，没有修改配置或尝试未提供接口。

DB报告记载既有各一次提交：`--supervise-exit --full Results/db-full-{arch} 1500`、外1800；客户端HttpClient.Timeout100秒/TaskCanceledException/-1073740791，无新taskID。这里是亲读DB报告，不冒称本轮重获原始工具异常日志；客户端退出码不是MTP退出码。

## 3. 正式项目显式只读恢复诊断及实跑

新增文件全体由 `DC_FULL_RESULT_RECOVERY` 编译符号隔离，默认不编译/发现。不改项目文件；仅本次专属发布传符号。测试只枚举 `C:\CodingAgentSandbox\Tasks` 下一层GUID任务目录，检查精确 `Results/db-full-x64` / `Results/db-full-x86`；拒绝reparse路径，不递归遍历用户文件。只对命中任务读取白名单监督、父child.trx、stdout/stderr、stages、顶层identity/参考消费、aot.sha256/package-files及BW两个identity。源文件只读共享打开，按初始长度复制有界快照；原路径、taskID、目标、长度、SHA或缺失/拒绝/错误均入日志。仅写当前隔离任务Results/dc-full-recovery；不修改源任务。

边界：30秒取消预算、最多4096个目录、256个文件、单文件32MiB/总256MiB。同步文件系统枚举调用不具备可证明硬实时中断，实际本轮无需接近期限。没有探测任意进程、退出或清理。

专用发布成功（日志11行、退出0）：正式ComAcceptance Release/win-x64/self-contained、RuntimeFrameworkVersion10.0.13、ComAcceptanceSandbox=true、AotRuntimeIdentifier=win-x64、显式CX native-x64候选；独有产物根 `artifacts/dc-full-recovery/build/`。正常restore/build，无NoBuild/NoDependencies；WarningLevel0仅限制日志，不宣称零警告。未重发生产、未触碰DB包。

执行仅显式filter `FullyQualifiedName~DcFullResultRecoveryTests`，不是full。外90秒，未进入ExitDiagnostics监督，无短阈值误杀原任务。工具返回新诊断taskID `5a0218f40a12454f814727cc13bdea9e`，MTP控制台1总计/0失败/1通过、475ms；此ID绝非DB full ID。本轮仅回收Results，诊断TRX在远端 `C:\CodingAgentSandbox\Tasks\5a0218f40a12454f814727cc13bdea9e\TestResults\dc-recovery.trx`，未本地回收/亲读，不称已核其Counters或退出字段。

亲读实际回收 `artifacts/dc-full-recovery/recovered/Results/dc-full-recovery/recovery.json` 全11行：Status=complete，TaskRoot为上述C盘路径，CurrentTask为新诊断任务，Scanned=1582、Matched=0、Copied=0、Bytes=0、Elapsed=00:00:00.1387329、Entries=[]。未触及目录数/时间/大小上限。**只证明本次可见任务集合未命中两个精确结果路径；不证明原任务不存在、被删除、未启动或已退出，也不证明不同沙箱实例共享任务集合。** 没有可读full结果可继续分类，未无限扩大搜索或再次运行长full。

## 4. DB入口独立审计与既有回归原件

当前Options以Filter=null区分full，ChildArguments直接省略--filter及表达式；完整固定数组仍含绝对TestResults/Diagnostic、child.trx及诊断/no-progress。Parse去掉监督开关后恰3参数；旧root/seconds/filter支持1–120，新--full/root/seconds支持1–1500。空白root/filter、数量、整数/溢出/边界拒绝；通配仍filtered。ExitArguments/ExitBudget位于既有resx经ResourceManager使用，本轮未改资源。

23个参数case独立计数：旧有效2、新有效2、越界6、非整数2、溢出1、空白4、数量3、null1、通配模式1、full模式1。完整full子参数数组测试验证无filter；不以这些case替代真正full。监督Mode/实际Arguments记录存在；原取消130、超时/排空124、整树kill、10秒cleanup、5秒排空及子退出码传播仍在，本轮不改行为。

下列原件位于 `artifacts/db-unfiltered-full/`，三份监督各39行全文亲读，父TRX ResultSummary/Counters及文件末尾亲读：

|用途|taskID|本地父TRX|亲读结果|
|---|---|---|---|
|旧filtered控制x64|7617ecc2c5ba4da6b1ed5c91bef0d2c8|control-x64/Results/db-control-x64/TestResults/child.trx:29–34|Completed，total=executed=passed=1，Exit0|
|x64回归|ccfa9636676f4137aeadc60cc3341767|regression-x64/Results/db-regression-x64/TestResults/child.trx:176–181|Completed，total=executed=passed=25，Exit0|
|x86回归|d93c60b3f67a46cc8534c06774de42ca|regression-x86/Results/db-regression-x86/TestResults/child.trx:190–195|Completed，total=executed=passed=26，Exit0|

三份所有其余Counters均0，ResultSummary无RunInfo；监督Mode=filtered、TimeoutSeconds120、Reason=exited、HasExited/OutputCollected=true、TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false、SupervisorError=null。实际Arguments分别包含PublishedArtifactTests控制，ExitDiagnosticsTests+GlyphReferenceTests+官方tile（x86另artifact）；明确不是full。监督.NET10.0.13、X64/X86，测试SHA x64 `23E2C64556A727D5DEBC1FD186E22B2E36873FBE2910CF044A7307CA022CE377` / x86 `4184432D899D8917E023C5947C0A8496ADD8B80BE633F51AA31340E856ACCEE9`，MVID `839f9bdc-289c-4cb5-979a-cf56657cb421` / `0f02cfb7-e6f6-4e6e-a966-cfbe19dccd0e`。仅入口回归证据，不冒称本轮重新执行这些测试或候选actual-loaded/full消费证据。

## 5. 未知项、平台交接和下一动作

|既有full|原taskID/路径|父TRX/Counters/RunInfo/失败组|退出/Mode/实际Arguments/actual身份|
|---|---|---|---|
|DB x64，各一次已提交|未知，未恢复|未知，未恢复|未知，未恢复|
|DB x86，各一次已提交|未知，未恢复|未知，未恢复|未知，未恢复|

需要平台提供：提交时即持久化/返回taskID及远端绝对目录；按ID异步状态查询；独立回收既有任务或同一沙箱任务索引访问；支持长请求的HTTP客户端超时/异步轮询，使100秒传输限制不截断1800秒执行预算。不改工具配置、不擅自调用未提供接口。平台定位后先取原任务完整目录，不重新full；亲读监督Mode=full/无filter实际Arguments、结束/退出/kill状态、父TRX全部Counters/RunInfo/Failed、实际test/native/official/runtime及业务原件，再分组失败，不越界生产修复。若平台无法定位，继续保留未知并由监督方明确后续授权。

旧BD x64 full1740/1699/41 Exit2和x86无父TRX不绑定新候选；CX/CY有限修复保留，不由本轮观测阻塞撤回。最终Tests/ComAcceptance × x86/x64 Release四格，以及GPU/backend、真实loss/resize新帧、media、资源/线程/callback/通知、同步Event0/E_HANDLE、首次堆损坏、圆角/大shader及其他nested/stroke合同仍未闭。不填0失败、不用入口25/26或诊断1通过冒充full。本文为完成恢复尝试后的准确阻塞交接，不是等待补写的初稿。
