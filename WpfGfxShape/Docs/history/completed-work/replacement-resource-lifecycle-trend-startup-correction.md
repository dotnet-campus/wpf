# Resource lifecycle trend 启动链修正 — BuildPassed / SourceFrozen / Execution NotRun

## 本轮实际裁决

亲自实施，仅测试启动链及解析边界用例。唯一normalCOMbuild1：`ResourceLifecycleTrendDiagnostic / win-x86 / net10.0-windows`，真实exit0、144 warnings / 0 errors、3.38秒。成功即停止，源码冻结；无重试、无分析器抑制。publish/Native/沙箱/测试/official/candidate/终端/脚本/再委派全部0。静态与编译通过不等运行通过，完整替换Open。

## 精确修正前后

- `ExitDiagnostics.Parse`原filtered统一120；现在仅既有非默认配置定义的`RESOURCE_LIFECYCLE_TREND`且完整相等过滤器 `FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests.WhenRepeatedAsyncSharedWicLifecyclesCompleteThenEachRoundReleasesOwnershipAsync`允许1..840。FQN亲读真实namespace/class/无参方法，常量用nameof绑定方法。其他过滤器（其他方法、模糊、组合、空格）仍120，full原1500不变，未借full绕过，默认配置无840分支。
- `ResourceTrendCoordinator`从依赖外部环境改为专用编译关闭协调体内部固定opt-in并校验，调用专用`ResourceTrendRoleAsync`传递；后者在启动前校验固定值及original|candidate，然后写入每个自建`ProcessStartInfo.Environment[WPFGFX_RESOURCE_TREND]=async-shared-wic-v1`。角色入口原`RequireOptIn`仍读取环境校验；未设/错误的手动role仍拒绝。没有全局环境设置、CLI flag、新Host或监督参数透传。
- `ExitDiagnosticsTests`新增精确840边界、841/0拒绝、其他方法/模糊/组合/空格拒绝用例，全部置诊断编译符号内，默认测试发现矩阵无新增。仅编译，未运行，不能写用例Passed。
- 旧RoleAsync、Native源码、AsyncSharedOwnership业务内核、项目/SDK/TFM/包规则/资源文案零改。

## 静态监督复核与风险

父`--supervise-exit`840 -> 正式测试协调CTS840 -> 专用role linked CTS360 -> role Timer360 / loop Timer20。official完整成功且16轮/65样本才candidate，角色分别自建一个进程；固定16轮4+12、65样本、无强制GC、DescriptiveOnly均保持。每个role均在Start前得到固定opt-in，进入角色后再次校验，静态链闭合；未执行验证实际环境。

role Start后双流采集/等待在try/finally内：异常/取消只kill自己创建的树，退出等待5秒、stdout/stderr各drain5秒，KillFailed/Unreaped/forcedTermination导致失败，资源Unknown。父原有超时树kill后有限退出10秒，drain5秒+取消后5秒；父catch可再尝试kill和10秒等待。Timer/取消非硬实时，Kill/文件IO/Dispose可受OS阻塞，外900秒边界尚未运行实证，不声称所有路径绝无孤儿。未扩大通用监督修复范围；强杀不是正常回收。worker NotProven，全部趋势/业务/实际身份NotRun，不能填0冒无泄漏。

## 实际构建参数与证据限制

实参：targetPath=`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`，configuration=`ResourceLifecycleTrendDiagnostic`，runtimeIdentifier=`win-x86`，targetFramework=`net10.0-windows`。既有配置已经绑定x86 AotRuntimeIdentifier/AotDllPath，不需要额外属性，无所谓普通build无法传属性阻塞。

ignored `artifacts/resource-lifecycle-trend-startup-normalbuild1/`保存prechange.md（变更前后/风险/预算）、static-review.md、build-result.md。build日志64行，亲读1–12和45–64真实header与summary：exit0、144warnings/0errors、3.38秒。stdout113567字符仅末20000保留，前93567不可恢复；没有逐字保存全部warning或每个工具响应，不冒完整raw/全日志亲读。多处早期长段读取4000字符截断；关键Parse、协调体、专用角色Start与回收和原guard已分段补读。一次把文件当搜索目录失败，无执行消耗。非全仓审计。

历史Stage A初失败、后成功与Stage B x86 r1 publish1均保留。旧`resource-lifecycle-trend-x86-r1/clean`为修正前已发布未运行包，绝不可冒本次新启动链修正版。

## 唯一下一授权需求与准确参数（本次不执行）

需另授COMpublish1（正常隐含编译1另计）+Windows沙箱1，独立新目录`resource-lifecycle-trend-x86-r2`，先核新包aot/runtime与角色入口，再唯一正式协调体1、official1先成功后candidate1，外900/父840/role360/loop20；normalbuild0/Native0/独立发现0/额外测试0/重试0/x640。失败即停，不修后重发/重跑。不要求工具env或外部维护者。

准确发布命令：
`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c ResourceLifecycleTrendDiagnostic -r win-x86 -f net10.0-windows --self-contained true -p:RuntimeFrameworkVersion=10.0.13 -p:TargetLatestRuntimePatch=false -p:ComAcceptanceSandbox=true -p:AotRuntimeIdentifier=win-x86 -p:AotDllPath=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/postfailure-source-rebuild-x86-r1/native/wpfgfx_cor3.dll -p:PublishDir=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/resource-lifecycle-trend-x86-r2/publish/ -p:ComAcceptancePackageDirectory=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/resource-lifecycle-trend-x86-r2/clean/`

准确沙箱参数：sourceDirectory=`WpfGfxShape/artifacts/resource-lifecycle-trend-x86-r2/clean`；executableRelativePath=`WpfGfxShape.ComAcceptance.exe`；arguments=`["--supervise-exit","Results/resource-lifecycle-trend-x86-r2","840","FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests.WhenRepeatedAsyncSharedWicLifecyclesCompleteThenEachRoundReleasesOwnershipAsync"]`；outputRelativePath=null（全任务回收）；localOutputDirectory=`WpfGfxShape/artifacts/resource-lifecycle-trend-x86-r2/sandbox-results`；timeoutSeconds=900。没有环境工具参数需求。

新包必须重新核test SHA/MVID、NETCore/Desktop10.0.13/x86、candidate实际模块路径/hash与official实际同版身份；历史候选15A545与官方5EBD仅预期，不冒actual。需回读父TRX/监督、每role进度/监督/65样本/逐轮business/events/callback/identity/描述性汇总，保留未执行/unfinished和超时资源Unknown。

## 完成状态

范围/规则恢复Completed；变更前快照Completed；最小修正Completed；静态复核Completed；唯一normalbuildPassed；源码冻结Completed；本轮报告与恢复状态同步Completed。执行门AwaitingAuthorization，不自动执行。完整替换Open；显示Unknown，实际音频、HW/shaderresize、loss/full、长期无界趋势、全GPU/全worker归还仍未证。
