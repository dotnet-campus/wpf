# Resource lifecycle trend Stage B x86 r1 — Published / ExecutionBlocked / Closeout

## 本轮裁决

亲自核对并执行唯一COM发布，exit0；执行前发现监督/opt-in合同阻塞，**未进入沙箱**。Published不等业务Passed；本轮有限生命周期门NotRun，不判GREEN或资源归还。源码保持阶段A成功编译后的冻结状态，没有修复后重试。所有旧独立账与原失败保留。

准确新无参FQN：`WpfGfxShape.ComAcceptance.UceChannelTests.WhenRepeatedAsyncSharedWicLifecyclesCompleteThenEachRoundReleasesOwnershipAsync`，已从诊断partial核对，不是参数组。

## 授权、实际预算与时长

|项|授权上限|本轮实际|
|---|---|---|
|normalCOMbuild / Native / 独立发现 / 重试|0 / 0 / 0 / 0|全部0|
|COMpublish|1|1，exit0；正常隐式编译1另计，没有追加normalbuild|
|Windows沙箱|1，timeoutSeconds900，整任务回收|0，核包前置条件未全部满足，未消耗|
|父监督 / official / candidate|各最多1；840 / 360 / 360秒|全部0，没有手工子进程或额外对照|
|轮watchdog|20秒尽力、非硬实时|NotRun|
|终端 / 脚本 / 再委派|0|全部0|

发布工具未返回时长，发布耗时Unknown；沙箱/角色运行时长N/A（未启动），不把900秒授权写成实跑时长。发布warning/error合计Unknown，不套阶段A144warnings/0errors；实际exit0。阶段A追加build144warnings/0errors及历史初build失败原账独立保留。

## 发布与包证据

配置`ResourceLifecycleTrendDiagnostic`、RID`win-x86`、TFM`net10.0-windows`、self-contained true，RuntimeFrameworkVersion10.0.13、TargetLatestRuntimePatch=false、ComAcceptanceSandbox=true，显式AotRuntimeIdentifier=win-x86与AotDllPath当前`artifacts/postfailure-source-rebuild-x86-r1/native/wpfgfx_cor3.dll`。准确完整命令见ignored `artifacts/resource-lifecycle-trend-x86-r1/publish-request.json`；独立本r1/publish与本r1/clean，无历史目录覆盖。

clean/aot.sha256全文实读：`15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`。clean/runtimeconfig21行全文NETCore与Desktop均10.0.13。package-files头45行含正式exe/dll/deps/runtimeconfig、MTP/MSTest/TRX；包目标在复制前验证每个依赖存在，没有逐行亲读500+清单的虚假声明。此为包身份，**不是实际加载身份**。

## 两项独立执行前阻塞

1. `ExitDiagnostics.cs:32–43`真实Parse：filtered预算上限120秒，只有`--full`上限1500。用户指定clean根正式exe `--supervise-exit Results/resource-lifecycle-trend-x86-r1 840 FullyQualifiedName=上述FQN`在当前冻结源码必抛ArgumentOutOfRangeException，发生在Directory.CreateDirectory、父记录及Process.Start之前。这是静态确定的合同不相容，不冒实际运行失败或exit。不得缩840到120、不得换full扩大矩阵，源码冻结不自动修改。
2. `ResourceTrendCoordinator.RequireOptIn`必须环境`WPFGFX_RESOURCE_TREND=async-shared-wic-v1`。精确源码搜索仅该读取与资源文案，无设置点；现有Windows沙箱工具参数没有environment字段，不能用参数字符串代替环境，也不能绕过禁止脚本/新Host限制。沙箱继承环境未证；独立于首项，不能声称已满足opt-in。

依“包aot/runtime/roles/新协调入口核好才唯一沙箱”，停止执行，不制造必败运行、不修后重发、不追加测试。角色派发及新协调源码存在不等完整启动链可用。早期阶段A只静态核了专用role内部360与协调体840，漏掉最外层filtered120；该差异本轮明确发现并保留，不能继续沿用“840可运行”的旧静态推断。

## 实证范围与未执行项

没有任务ID、父TRX、父监督结果、角色result/progress、65样本、16轮business/callback原件、timeout checkpoint、identity、stdout/stderr；原因是未启动，不是已读到尾或输出不全。official完成0/16、candidate完成0/16；各预定16轮全部未执行、预热0/4、测量0/12，未产生active/unfinished轮。cleanup异常计数N/A，不伪造0错误通过；timeout/KillFailed/Unreaped/forcedTermination均N/A，资源清理Unknown/NotRun。

本轮actual testSHA/MVID/runtimeX86/PID/actual模块路径及hash全部NotObserved。候选包SHA15A545已核；官方5EBD仅历史预期不是本轮actual。系统decoder角色未执行；本门是真实WIC stream/frame/wrapper，不是media player/systemMediaService，不套旧媒体decoder身份。每轮实例新鲜、同PID16轮、exactly-once callback与退出、A关闭前后保活、B更新、最终duplicate释放、真实channel关闭全部NotRun。worker退出NotProven，callback退出即使未来证实也不能冒全thread归零。

软指标PrivateBytes/WorkingSet/handles/threads/managed/naturalGC本轮均无样本，无斜率/峰值/差值结果，不能填0。未来固定轮0–3预热（保留全部峰）、4–15测量；各65=Initial1+16×4阶段点，轮3延后点基线，逐phase12轮OLS横轴轮号/纵轴原值，bytes/轮、handles/轮、threads/轮、GC次数/轮，三个四轮窗口min/max/median与首末差。正斜率也仅DescriptiveOnly，不能自动FAIL或GREEN，不强制GC、不丢峰、不延长等绿。

## 原件、亲读与偏差

ignored本r1目录：execution-ledger.md、publish-request.json、publish-tool-return.txt、publish-log-evidence.md、publish-log-16-30/31-45/46-60.txt、package-readback.md、preflight-blocker.md、exit-parse-raw.txt。发布顶层工具实际返回即时逐字保存；日志全部可见60行已分段亲读至尾并即时保存诊断摘要，**未逐字保存全部warning原文**。源stdout57087字符仅末20000可见，前37087不可恢复，不声称完整raw。前置读取/写入工具未全部逐字即时保存，部分4000字符截断；这是用户逐工具raw要求的真实偏差，不能补造。ExitParse关键24–43单独补读并逐字保存。

读取阶段A最新追加终态与合同、恢复入口/当前规则、关键诊断partial/coordinator、NativeRun入口、ExitDiagnostics解析、项目和包目标/SDK、当前五处状态。非全仓审计。相对枚举漏ignored目录，绝对枚举确认候选真实存在；一次把文件路径用作搜索目录、一次漏正则开关无匹配、一次超文件尾读取无内容，均无新增编译或程序执行。

## 唯一下一门建议与终止边界

**另授权仅测试侧启动链最小修正与静态复核门**：限定本精确FQN支持filtered840及受控opt-in传递，保持通用旧120/默认矩阵不扩大；重新明确源码修改、normalbuild≤1的预算。此为建议未执行，不自动再publish/沙箱，不自动x64。当前源码和包保持冻结。

本轮完整阻塞裁决已完成；五处状态追加与回读后文档CloseoutCompleted。整体替换Open，实际音频、显示HW/shaderresize、real-loss/full、长期无界无泄漏、全GPU/全worker归还均未证，显示Unknown。未来通过也只能称16轮有限真实生命周期已知硬门+描述性趋势，不升级这些边界。
