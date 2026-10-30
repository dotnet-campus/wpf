# NativePaintBaseline r1 失败与 r2 有界修复验证

## 最新接续终态：诊断静态修正完成，唯一编译门 Passed，已停止

本次用户明确授权亲自读改限定 NativePaintBaseline partial/必要文档，不因主代理监督角色停工；未再委派、未用终端/脚本。下文 r1/r2 失败、原件和旧建议全部作为历史保留，本节取代“尚不能编译/待授权修编译”的当前状态，不追认历史包已更新。

### 实际步骤与修改

1. 已读本报告、最新恢复入口、current-execution-rules、current-work-item、计划状态；完整诊断 partial 原1–398行分段读取（长段有4000字符截断，后续小段补齐），核 SDK global.json 10.0.107/rollForward latestFeature、项目 net10.0-windows/Nullable enable 及既有诊断配置。首次结构查询传工作区目录返回 Function failed，改用目录读取；误读项目下 artifacts 的旧日志路径返回不存在，不冒已读原件。没有运行测试发现。
2. 唯一代码文件 Tests/WpfGfxShape.ComAcceptance/D3DImageTests.NativePaintBaseline.cs：新增显式参数私有 NpbCleanupRelease，query/device/factory/chain 按非零指针分别执行唯一一次 Release 调用，不重试。query/device 裸取址局部不再被 lambda 捕获；角色报告先将 device 转 DTO 字符串再捕获。清理返回 refs 由真实调用记录，query/device/factory 保留归零要求；隐式 chain 不强加归零要求。chain 获取/参数断言的主异常先保存，释放/记录异常收集到 chain.cleanup，finally 后主异常优先重抛；query 同策略。device、factory、DestroyWindow 逐项清理，某项失败不跳过后项。DC 单次采样在 GetPixel 抛异常时仍逐项尝试一次 ReleaseDC，正常路径记录真实 release 值，不新增采样。
3. NpbTryWrite 改接 Func<object>，最终 Report 构造及序列化/写盘均在同一保护边界内；角色 partial 构造/加入 roles 也在此边界内。写报告/独立 recording-error JSON/stderr 的次级异常显式返回或聚合，不能覆盖业务主异常；无主异常时重抛报告错误。无 finally throw、无静默 catch、无 NoWarn/分析器关闭。保留原有诊断异常记录策略。静态复核原生字段只经 DTO 或 long/int 输出，报告树无裸 IntPtr/UIntPtr；NpbState/原生结构自身未直接加入报告。合成完整报告自检、角色结束 partial、自检失败不执行角色策略均保留。本轮只编译，未执行这些运行路径，不冒实际释放/报告落盘认证。
4. A/B协议、FF123456/00563412、各一次 Clear/Present、完成等待及单DC阈值不改；WndProc hook、角色策略、原shader、原业务、共享helper、生产、项目配置、SDK/包均未改。构建后没有继续修改源码。

### 唯一 normal COM build 实际参数和可见结果

目标绝对路径 d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj；configuration=NativePaintBaselineDiagnostic，runtimeIdentifier=win-x86，targetFramework=net10.0-windows。调用一次，工具返回“执行成功，完整日志共63行”。逐段回读日志1–32及33–63，无漏掉可见行。

日志1–5：dotnet build，目标上述项目，附加参数 --configuration NativePaintBaselineDiagnostic --runtime win-x86 --framework net10.0-windows，exit0/成功。日志59–62：139 warnings、0 errors、00:00:02.50。stdout原109953字符，仅尾20000可见，前89953缺失（日志8明确截断）；日志9–58为可见警告，不据此补造完整警告清单。可见代码 CA2007/MSTEST0044/MSTEST0045/MSTEST0037/CA2014/MSTEST0017；本partial可见 MSTEST0037 在144、145、160、196、367行，未压制或为清零扩大改动。没有可见错误或独立stderr段；不能从无stderr段推断原始stderr内容。完整可见结果仍在本轮最后构建日志63行及工具会话返回中，不把截断前输出称已保存/亲读。CS1686 已经本次构建消除。

既有配置 obj/bin 沿 native-paint-baseline-x86-r1 是历史事实，本次可能更新其中构建产物；未改路径、未删产物。本门仅代码编译认证，不认证旧publish/clean已更新，不以旧包补造r1未知A/B。

|预算|r1历史|r2历史|本次独立编译门|这三阶段累计|
|---|---:|---:|---:|---:|
|normal COM build|1|1失败|1成功|3|
|COM publish|1|0|0|1|
|Windows沙箱|1|0|0|1|
|测试体|1（实际A/B次数未知）|0|0|1|
|Native/测试发现/额外official/x64/full/原shader/重试|0|0|0|0|

旧87d673efe07345f9b0133d2c3287e0f9独立build1/publish1/沙箱1/体1不并入上表；所有历史原件/账保留。r1真实exit2/序列化失败、r2 build exit1/138 warnings/2 CS1686且无运行不变。本次无新业务JSON/TRX/运行身份，r1未知A/B、四分支裁决、实际Clear/Present次数、cleanup/owner/WndProc运行顺序仍未知，完整替换Open/根因未定。

### 收尾与唯一下一预算建议（未执行）

Implementation静态修正Completed；本次Build Passed；本次Publish/Sandbox/Test/Discovery均NotExecuted，编译预算1/1已耗尽并停止。恢复入口/current-work-item/plan状态同步本节后回读。

仅建议另授权最后一次既定A/B：COM publish≤1（相同诊断配置/win-x86/net10.0-windows，self-contained .NET/Desktop10.0.13，显式当前15A545 AotDllPath，独立新publish/clean/readback目录，不冒旧clean）；静态核包成功后沙箱≤1/体≤1，精确 WpfGfxShape.ComAcceptance.D3DImageTests.WhenPaintScopedNativePresentIsComparedThenOriginalClientPixelIsRequired，既定 --supervise-exit 新Results目录 120、外180。新增normal build0/Native0/发现0/其他变体0；publish或核包失败即停，运行首错停，不重试、不追加观察变体。保留原四分支/证据失效规则，两侧失败停止paint方向。本轮不执行该建议。

## r1 真实失败 closeout（修复前登记）

任务 af7c2a93394241d6aaa71f4e1755cd2f；原件 artifacts/native-paint-baseline-x86-r1/{01-build-tool-return.md,02-publish-tool-return.md,03-preflight.md,04-sandbox-tool-return.md,readback} 保留不改。
监督原件亲读：.NET10.0.13/X86，test SHA0369BE645C59625412702E2DB52CD218C86F65D8E2FCFDD267B1C54C363F7E80，MVID ae74e3a2-3ae5-4915-b9f5-7de14d7e996e；OS exit2，TimedOut=false，SupervisorError=null。stdout 明确 NotSupportedException：$.roles.before.creation.Focus 的 System.IntPtr 不支持序列化。最终 finally 写业务报告可能覆盖原主异常，native-paint-baseline.json 未得到有效业务证据；不能判断A/B像素，不能假定两次实际提交或完整清理认证。兄弟 actual.Window 也为裸nint，必须一并修复。

r1 已用 normal COM build1、COM publish1、沙箱1、测试体1；build历史137 warnings/0 errors（前段输出截断，保留原登记）、publish成功。不冲销历史87d673efe07345f9b0133d2c3287e0f9独立build1/publish1/沙箱1/体1。Native/发现/额外official/x64/full/原shader/重试0。

## r2 入口阻塞快照与授权

当前入口旧文称NotStarted/待授权，已过时：r1设施失败已真实发生，内容证据未证，完整替换Open，根因未定而非外部权限不足。r2由本次用户授权，仅诊断partial/必要资源文档；不改WndProc hook、生产、原shader、VisualCache、公共helper、SDK/包。

r2 normal COM build<=1，NativePaintBaselineDiagnostic/win-x86/net10.0-windows；成功才COM publish<=1，self-contained .NET/Desktop10.0.13，显式当前15A545 AotDllPath，新 artifacts/native-paint-baseline-x86-r2/{publish,clean,readback}；Native0。成功静态核包后唯一沙箱<=1，--supervise-exit Results/native-paint-baseline-x86-r2 120 精确原新增FQN，外180。发现/official额外/x64/full/原shader0；首错停不修后重试，无第三轮。

修复目标：native字段明确DTO；同诊断内预持久化合成完整报告自检；每角色清理后即时partial；报告写失败独立终态，不覆盖主异常或阻止清理。A/B协议、各一次Clear/Present及单次DC阈值观察不变，不等绿、不重发。

## r2 当前状态

**终态 Failed.Build：设施修复未完成，首错停止。** 唯一构建exit1、138 warnings、2 errors；日志64行分两段回读。stdout原110429字符仅尾20000可见，前90429缺失，不声称完整原输出。partial(133,113) query、(272,59) device 报CS1686：取过地址的局部变量被新增清理lambda捕获。本次修改引入，不是外部权限问题。失败后没有继续源码修改或重试。

已写入但未验证通过的诊断修改：creation.Focus/actual.Window稳定16位十六进制DTO、client固定数值DTO；合成报告持久化自检；角色结束partial；最终报告移出finally；主异常优先、记录异常独立JSON/stderr、清理错误独立收集。原Clear/Present/完成/单DC采样、WndProc hook及原shader业务未改。编译失败，以上均不得认证生效，且chain.Release异常保全仍需后续静态审查，不冒完全修复。

r2未publish，未生成新clean，没有Windows任务、TRX、业务JSON/partial/运行identity/stdoutstderr/diag可回读；不运行r1旧包冒充。r1业务缺失、r2未执行，四类A/B结果均未证；timing/参数/cleanup/owner Destroy/实际Release/WndProc顺序无新认证。模块文件初始化不等于产品加载。

|预算|r1实际|r2实际|两轮累计|
|---|---:|---:|---:|
|normal COM build|1|1失败|2|
|COM publish|1|0|1|
|Windows沙箱|1|0|1|
|测试体|1|0|1|
|Native/发现/额外official/x64/full/原shader/重试|0|0|0|

r1实际Clear/Present次数未知，不假定2次；r2为0。87d…历史独立build1/publish1/沙箱1/体1不并入这两轮分母。配置obj/bin仍指r1目录，本次可能触及构建中间件；日志/readback保留，不能称全部r2中间输出独立。即时参数/返回见 artifacts/native-paint-baseline-x86-r2/01-build-tool-return.md。

**唯一主线建议：停止执行，另授权仅修诊断编译及异常保全缺口，再决定新的有界验证预算；不自动开启第三轮。** 确定最小原因是cleanup闭包捕获address-taken局部变量，后续可用私有显式参数方法消除捕获，并检查chain/query/device兄弟异常路径；不是paint/owner/override/格式/DPI/capture变体，不授权改原用例。没有B成功或两侧失败证据，不能提出paint修正，也不能判定该方向已实测否定。原内容门失败/未达，根因未定，完整替换Open而非ExternalBlocked。
