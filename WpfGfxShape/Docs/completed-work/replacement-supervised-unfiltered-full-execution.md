# DB — supervised unfiltered full execution

## 1. 结论与授权边界
入口最小修复及双架构专属回归已完成；双架构full各提交一次，但执行通道均报100秒HttpClient超时，未回收任务目录，不能交付full完成、失败计数或实际加载结论。不是用旧基线或定向结果拼full。完整替换和最终四格仍未完成。

先创建本文再调查并持续补充。已读执行规则27行、完整方案120行、CZ current-comacceptance-full和DA full-baseline-preflight-review全文（长输出分段补齐）。项目结构只有生产及两个正式测试工程；查找未发现工作区AGENTS。正式项目net10.0-windows/WPF/Nullable、MSTest4.0.1，SDK10.0.107/latestFeature。

修改仅：Tests/WpfGfxShape.ComAcceptance/ExitDiagnostics.cs、ExitDiagnosticsTests.cs、AcceptanceResources.resx及本文。未改NativeRun、生产、业务断言、项目打包、默认并行、golden/阈值/skip/fallback；无Host/第三工程、终端、直接或间接脚本、探索符号、Old依赖。产物独有artifacts/db-unfiltered-full/。

## 2. 模式改动
旧入口仍为 `--supervise-exit root seconds filter`，预算1–120秒；明确新模式为 `--supervise-exit --full root seconds`，预算1–1500秒。full不是空白或通配filter：其Filter为null，ChildArguments直接省略--filter及表达式。通配字符串仍属filtered。root经Path.GetFullPath后生成绝对TestResults/Diagnostic路径。

精确校验参数数量、null、空白root/filter、整数格式/溢出及模式上下界，分别ArgumentException/ArgumentNullException/FormatException/OverflowException/ArgumentOutOfRangeException。新错误文本沿用AcceptanceResources ResourceManager本地化约定。监督JSON增加Mode，保留完整实际Arguments。原CancelAfter、取消130、timeout/排空124、kill整树、10秒清理、5秒排空和子退出码传播均未改；没有把子失败转0。

## 3. 发布与专属回归
通过专用发布工具正常restore/build依赖图，两次Release、自包含、RuntimeFrameworkVersion10.0.13、ComAcceptanceSandbox=true、win-x64/win-x86、对应AotRuntimeIdentifier和CX native-{arch}绝对AotDllPath。独有WpfGfxShapeArtifactsRoot为本根x64/、x86/。均发布成功；无NoBuild/NoDependencies/DefineConstants/额外props；WarningLevel=0仅缩减日志不证明零警告。生产未修改未重发。

本地绝对证据根 R=`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\db-unfiltered-full\`。

|用途|taskID|R下完整回收目录|父TRX相对该目录|结果|
|---|---|---|---|---|
|旧filtered x64控制|7617ecc2c5ba4da6b1ed5c91bef0d2c8|control-x64|Results/db-control-x64/TestResults/child.trx|Completed 1/1 Exit0|
|x64参数及官方输入|ccfa9636676f4137aeadc60cc3341767|regression-x64|Results/db-regression-x64/TestResults/child.trx|Completed 25/25 Exit0|
|x86参数、官方输入及artifact控制|d93c60b3f67a46cc8534c06774de42ca|regression-x86|Results/db-regression-x86/TestResults/child.trx|Completed 26/26 Exit0|

三份父TRX Counters亲读：total=executed=passed分别1、25、26；failed/error/timeout/aborted/inconclusive/passedButRunAborted/notRunnable/notExecuted/disconnected/warning/completed/inProgress/pending全部0。ResultSummary无RunInfo、无Failed。控制仅哈希文件，不能冒称COM actual加载。23项参数回归覆盖filtered1/120、full1/1500、非法0/-1/121/1501、非整数、溢出、空白、参数数量、null、通配模式、full模式及完整无filter子参数数组。不为单元测试启动full。

三份监督全文亲读：Mode=filtered，TimeoutSeconds120，Reason=exited，HasExited/OutputCollected=true；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，SupervisorError=null，ExitCode0。
- 控制PID3416/8852，UTC2026-10-21 06:58:30.2389574→06:58:30.9762071。
- x64回归PID5424/3992，06:58:58.1712636→06:58:59.8376701。
- x86回归PID2920/6188，06:58:41.6511446→06:58:43.5403895。

远程父TRX绝对路径分别为 `C:\CodingAgentSandbox\Tasks\<上表taskID>\Results\db-control-x64\TestResults\child.trx`、对应x64回归的`Results\db-regression-x64\TestResults\child.trx`、x86回归的`Results\db-regression-x86\TestResults\child.trx`。全部回收而非仅Results，包含exe/runtime/ReferenceData、监督/Diagnostic/stages/stdout/stderr。

## 4. 已核身份与官方输入边界
|身份|x64|x86|
|---|---|---|
|候选文件SHA256|F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7|7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA|
|新测试程序集SHA256|23E2C64556A727D5DEBC1FD186E22B2E36873FBE2910CF044A7307CA022CE377|4184432D899D8917E023C5947C0A8496ADD8B80BE633F51AA31340E856ACCEE9|
|新测试MVID|839f9bdc-289c-4cb5-979a-cf56657cb421|0f02cfb7-e6f6-4e6e-a966-cfbe19dccd0e|
|官方输入来源SHA256|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|

监督取实际typeof程序集SHA/MVID；顶层aot-identity全文核候选文件SHA、X64/8和X86/4、实际命令行。该文件可能被子角色覆盖，本轮只用监督证明父命令；并非候选actual-loaded枚举。没有本轮COM实际消费模块证据。

两回归由既有GlyphReferenceTests和WhenTileFixturesAreRenderedThenOriginalWpfEvidenceIsRecorded生成ignored ReferenceData。亲读glyph第24–30行OriginalLibrary/SHA及original-wpf第1–14行：.NET10.0.13，PresentationCore为相应任务根PresentationCore.dll，官方为任务根wpfgfx_cor3.dll，10,0,1326,50504 @Commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；SoftwareOnly/Pbgra32非float/HDR oracle。未审完大像素数组，未本轮获取PresentationCore SHA/版本，不借CZ历史数值充新证据。完整回归目录含真实本地官方输入供full，无官方冒充、tracked ReferenceData修改或Old补档。

## 5. 各一次full调用与通道错误
在上述回归验收后顺序各提交一次：
- x64 source=R/regression-x64，正式WpfGfxShape.ComAcceptance.exe，参数 `--supervise-exit --full Results/db-full-x64 1500`，外timeout1800，预定完整回收R/full-x64。
- x86 source=R/regression-x86，同exe，参数 `--supervise-exit --full Results/db-full-x86 1500`，外timeout1800，预定完整回收R/full-x86。

未指定outputRelativePath，要求整任务回收。旧回归目录含其旧TRX，绝不作为新full父TRX。代码及参数回归证明构造无filter，但因未回收，不能冒称已读full实际supervision Arguments。

两次专用执行工具均返回相同错误：`InvalidOperationException: WinRemoteShell 执行远端命令失败，客户端退出码为 -1073740791`；内部`TaskCanceledException: The request was canceled due to the configured HttpClient.Timeout of 100 seconds elapsing`，TimeoutException/IOException/SocketException(995)，栈在WinRemoteShell.Client.ExecClient.ExecuteAsync及HttpClient.SendAsync。此处-1073740791是远程执行客户端退出码，绝非MTP子退出码；100秒是传输客户端限制，绝非1500秒业务监督到期。

工具没有返回任何新taskID或远程绝对目录。R枚举没有full-x64/full-x86目录；对精确 `R/full-x64/Results/db-full-x64/TestResults/child.trx` 和x86对应路径直接读均返回本地文件不存在。只能证明未回收到本地，不能声称远端没有TRX。工作区.coding-agent为空，本机C:\CodingAgentSandbox\Tasks不存在；没有把本地不可见等同远端不存在。没有二次full重跑，也未改变入口绕过通道。

|架构|full提交次数|远程taskID/绝对路径|父TRX/Counters/RunInfo/Failed|子ExitCode/TimedOut/kill/最后阶段|
|---|---|---|---|---|
|x64 Release|1|工具未返回，未知|未回收，未知|未回收，未知|
|x86 Release|1|工具未返回，未知|未回收，未知|未回收，未知|

远端是否仍执行、自然退出、崩溃或被杀均未知，不能说full已完成/已确认中断。未取得可读full结果不是结果待补初稿，而是本轮明确传输及证据回收阻塞；没有拿stdout末用例推断根因。

## 6. 分类与下一最小切片
- 已证：入口模式和参数回归、默认包双架构发布、filtered兼容控制、官方输入生成。仅文件身份，不是COM消费全量验证。
- 环境/执行前置：已观察远程执行客户端100秒HTTP超时并无结果回收；它不证明产品或夹具缺陷，不能把它当sampler崩溃证据。
- 产品/夹具：当前full未取回，不能分类具体业务Failed；历史sampler官方可崩仍不能跳过，也不能把最后Destroy阶段当首写坏根因。
- 输入缺失：本轮两类官方输入确实存在；其他full输入完整性未确认，不能预称缺输入导致失败。
- 未覆盖：当前full实际模块加载、业务差分、GPU/backend、device-loss/resize新帧、media、资源/线程/callback/通知收敛。CW/CY有限通过不外推这些能力或四格。
- 原44/10曲线差异和旧BD41失败不绑定新SHA；本轮没有full新证据裁决它们已闭或复现。

下一最小切片首先恢复执行通道的结果回收：由具备远端任务索引/回收能力的一方定位这两次既有任务，取整任务目录，勿再次启动full。需明确执行客户端100秒HttpClient限制与1800秒工具预算的差异。拿到原任务后亲读full监督实际无filter参数、结束状态、绝对父TRX/Counters/RunInfo/全部Failed、actual模块/测试身份/官方来源及业务原件，再决定最多一个最小有界诊断。若既有任务无法定位则保留未知并由监督方决定下一次授权，不无限重跑，不越界修生产或夹具。
